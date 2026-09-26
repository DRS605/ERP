using System.Net;
using System.Net.Http.Json;
using AlxorCore.Persistencia;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Presupuestos contables: carga, aprobación, versiones, generación desde el real y seguimiento.</summary>
public sealed class PresupuestosContablesEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private const string Base = "/contabilidad/presupuestos";
    private readonly FabricaApiPruebas _fabrica;

    public PresupuestosContablesEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(string CuentaCodigo, Guid? CentroId, List<decimal> Importes, decimal Total);
    private sealed record PresupuestoResp(Guid Id, string Estado, int Meses, decimal TotalIngresos, decimal TotalGastos, decimal Resultado, List<LineaResp> Lineas);
    private sealed record SegLineaResp(string CuentaCodigo, Guid? CentroId, List<decimal> RealMensual, decimal PresupuestoAcumulado, decimal RealAcumulado,
        decimal Desviacion, decimal? PorcentajeEjecucion, bool Favorable);
    private sealed record SeguimientoResp(int MesesTranscurridos, List<SegLineaResp> Lineas, decimal GastosPresupuesto, decimal GastosReal,
        decimal IngresosPresupuesto, decimal IngresosReal, decimal ResultadoPresupuesto, decimal ResultadoReal, List<string> Avisos);
    private sealed record ProblemaResp(string Title, string Codigo);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private async Task<(HttpClient Api, Guid Empresa, Guid Adm)> EmpresaAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        var adm = (await (await api.PostAsJsonAsync("/contabilidad/analitica/centros", new { Codigo = "ADM", Nombre = "Administración" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await api.PostAsJsonAsync("/contabilidad/analitica/reglas", new { PrefijoCuenta = "629", CentroId = adm })).StatusCode.Should().Be(HttpStatusCode.Created);
        return (api, empresa, adm);
    }

    private static async Task GastoAsync(HttpClient api, decimal baseImponible, int mes) =>
        (await api.PostAsJsonAsync("/gastos", new { Concepto = "Servicios", BaseImponible = baseImponible, Fecha = new DateOnly(Anio, mes, 10) }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private static async Task<PresupuestoResp> CrearAsync(HttpClient api, object cuerpo)
    {
        var resp = await api.PostAsJsonAsync(Base, cuerpo);
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        return (await resp.Content.ReadFromJsonAsync<PresupuestoResp>())!;
    }

    [Fact]
    public async Task El_seguimiento_compara_con_el_real_por_cuenta_y_por_centro()
    {
        var (api, _, adm) = await EmpresaAsync();
        var p = await CrearAsync(api, new
        {
            Codigo = $"P{Anio}",
            Nombre = "Presupuesto anual",
            Desde = new DateOnly(Anio, 1, 1),
            Meses = 12,
            Lineas = new object[]
            {
                new { CuentaCodigo = "62", Total = 12000m },                         // todo el subgrupo 62, 1.000 €/mes
                new { CuentaCodigo = "629", CentroId = adm, Total = 6000m },          // lo de administración, 500 €/mes
                new { CuentaCodigo = "705", Importes = Enumerable.Repeat(2000m, 12).ToArray() },
            },
        });
        p.Lineas.Single(l => l.CuentaCodigo == "62").Importes.Should().AllBeEquivalentTo(1000m);
        p.TotalGastos.Should().Be(18000m);
        p.TotalIngresos.Should().Be(24000m);

        await GastoAsync(api, 800m, 1);
        await GastoAsync(api, 1500m, 3);

        var s = (await api.GetFromJsonAsync<SeguimientoResp>($"{Base}/{p.Id}/seguimiento?hasta={Anio}-03-31"))!;
        s.MesesTranscurridos.Should().Be(3);
        var cuenta = s.Lineas.Single(l => l.CuentaCodigo == "62");
        cuenta.PresupuestoAcumulado.Should().Be(3000m);
        cuenta.RealAcumulado.Should().Be(2300m);
        cuenta.RealMensual.Take(3).Should().Equal(800m, 0m, 1500m);
        cuenta.Desviacion.Should().Be(-700m);
        cuenta.Favorable.Should().BeTrue("se ha gastado menos de lo previsto");

        var centro = s.Lineas.Single(l => l.CentroId == adm);
        centro.PresupuestoAcumulado.Should().Be(1500m);
        centro.RealAcumulado.Should().Be(2300m, "el real por centro sale de la analítica");
        centro.Favorable.Should().BeFalse();
        centro.PorcentajeEjecucion.Should().Be(153.33m);

        var ventas = s.Lineas.Single(l => l.CuentaCodigo == "705");
        ventas.Favorable.Should().BeFalse("no se ha vendido nada");
        s.ResultadoPresupuesto.Should().Be(6000m - 3000m - 1500m);
        s.Avisos.Should().BeEmpty("las líneas 62 y 629 tienen distinto centro: no se solapan");
    }

    [Fact]
    public async Task Un_presupuesto_aprobado_no_se_cambia_y_se_versiona_copiandolo()
    {
        var (api, empresa, _) = await EmpresaAsync();
        var p = await CrearAsync(api, new { Codigo = "V1", Nombre = "Versión 1", Desde = new DateOnly(Anio, 9, 1), Meses = 12, Lineas = new[] { new { CuentaCodigo = "629", Total = 1200m } } });
        (await api.PostAsync(new Uri($"{Base}/{p.Id}/aprobar", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var cambio = await api.PutAsJsonAsync($"{Base}/{p.Id}/lineas", new { Lineas = new[] { new { CuentaCodigo = "629", Total = 9000m } } });
        cambio.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await cambio.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("presupuesto.aprobado");

        var copia = (await (await api.PostAsJsonAsync($"{Base}/{p.Id}/copiar", new { Codigo = "V2", Nombre = "Versión 2", IncrementoPorcentaje = 10m })).Content.ReadFromJsonAsync<PresupuestoResp>())!;
        copia.Estado.Should().Be("Borrador");
        copia.TotalGastos.Should().Be(1320m);

        // La base de datos también lo impide, aunque se salte la aplicación.
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand($"UPDATE contabilidad.linea_presupuesto SET importes = importes WHERE presupuesto_id = '{p.Id}'", c);
        var ex = await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
        ex.Which.SqlState.Should().Be(GarantiasSql.CodigoError);
        ex.Which.Hint.Should().Be("presupuesto.aprobado");
    }

    [Fact]
    public async Task Se_genera_desde_el_real_de_otro_periodo_con_incremento()
    {
        var (api, _, adm) = await EmpresaAsync();
        await GastoAsync(api, 1000m, 3);
        await GastoAsync(api, 500m, 3);

        var resp = await api.PostAsJsonAsync($"{Base}/desde-real", new
        {
            Codigo = "SIG", Nombre = "Año siguiente", Desde = new DateOnly(Anio + 1, 1, 1), Meses = 12,
            OrigenDesde = new DateOnly(Anio, 1, 1), IncrementoPorcentaje = 10m,
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        var generado = (await resp.Content.ReadFromJsonAsync<PresupuestoResp>())!;
        var linea = generado.Lineas.Single();
        linea.CuentaCodigo.Should().Be("629");
        linea.Importes[2].Should().Be(1650m, "marzo: 1.500 € + 10 %");
        linea.Total.Should().Be(1650m);

        var porCentro = (await (await api.PostAsJsonAsync($"{Base}/desde-real", new
        {
            Codigo = "SIGC", Nombre = "Por centro", Desde = new DateOnly(Anio + 1, 1, 1), Meses = 12, OrigenDesde = new DateOnly(Anio, 1, 1), PorCentro = true,
        })).Content.ReadFromJsonAsync<PresupuestoResp>())!;
        porCentro.Lineas.Single().CentroId.Should().Be(adm);
    }

    [Fact]
    public async Task Las_lineas_se_validan()
    {
        var (api, _, _) = await EmpresaAsync();
        async Task<string> CodigoAsync(object linea)
        {
            var r = await api.PostAsJsonAsync(Base, new { Codigo = Guid.NewGuid().ToString("N")[..8], Nombre = "X", Desde = new DateOnly(Anio, 1, 1), Meses = 12, Lineas = new[] { linea } });
            r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
        }

        (await CodigoAsync(new { CuentaCodigo = "572", Total = 100m })).Should().Be("presupuesto.cuenta");
        (await CodigoAsync(new { CuentaCodigo = "629", Importes = new[] { 1m, 2m } })).Should().Be("presupuesto.importes");
        (await CodigoAsync(new { CuentaCodigo = "629", Total = 100m, Importes = Enumerable.Repeat(1m, 12).ToArray() })).Should().Be("presupuesto.importes");
        (await CodigoAsync(new { CuentaCodigo = "629", Total = -5m })).Should().Be("presupuesto.importes");

        var reparto = await CrearAsync(api, new { Codigo = "R", Nombre = "Reparto", Desde = new DateOnly(Anio, 1, 15), Meses = 12, Lineas = new[] { new { CuentaCodigo = "629", Total = 100m } } });
        reparto.Lineas.Single().Importes.Sum().Should().Be(100m, "el total se reparte al céntimo");
        reparto.Lineas.Single().Importes.Should().Equal(8.34m, 8.34m, 8.34m, 8.34m, 8.33m, 8.33m, 8.33m, 8.33m, 8.33m, 8.33m, 8.33m, 8.33m);
    }
}
