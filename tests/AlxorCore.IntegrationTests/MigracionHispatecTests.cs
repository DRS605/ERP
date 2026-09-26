using System.Net;
using System.Net.Http.Json;
using AlxorCore.Migracion.Tests;
using AlxorCore.Persistencia;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Migración desde Hispatec: validar sin tocar nada, cargar lo válido, repetir sin duplicar y operar con lo migrado.</summary>
public sealed class MigracionHispatecTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public MigracionHispatecTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record IncidenciaResp(string Archivo, int? Fila, string Codigo, string Nivel, string Mensaje);
    private sealed record CuadresResp(decimal SaldosDebe, decimal SaldosHaber, int CuentasDescuadradasConAcumuladores);
    private sealed record InformeResp(int Errores, int Avisos, bool PuedeCargar, List<IncidenciaResp> Incidencias, CuadresResp Cuadres);
    private sealed record PasoResp(string Paso, int Creados, int Reutilizados, int YaMigrados, List<string> Errores);
    private sealed record ResultadoResp(InformeResp Informe, bool Cargado, List<PasoResp> Pasos);
    private sealed record ClienteResp(Guid Id, string Nombre, string? NifFiscal);
    private sealed record ProductoResp(Guid Id, string? Referencia, string Nombre, string CodigoIva, Guid? FamiliaId);
    private sealed record EfectoResp(Guid Id, string Sentido, Guid? TerceroId, string TerceroNombre, string Documento, decimal Importe, decimal Pendiente, string Estado);
    private sealed record SaldoResp(decimal Liquidado, decimal Pendiente, string Estado);
    private sealed record AgricultorResp(Guid Id, string Nombre, string Regimen, DateOnly? AutofacturacionDesde);
    private sealed record ParcelaResp(string Codigo, string? ReferenciaSigpac, decimal? SuperficieHa, Guid? ProductoId);
    private sealed record EjecucionResp(DateOnly FechaCorte, string Resumen);
    private sealed record SeleccionResp(string Token);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private async Task<(HttpClient Api, Guid Empresa)> EmpresaAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        return (api, empresa);
    }

    private static object Paquete(Dictionary<string, string>? archivos = null) =>
        new { ContenidoBase64 = Convert.ToBase64String(PaqueteEjemplo.Zip(archivos ?? PaqueteEjemplo.Archivos(Anio))) };

    private static async Task<ResultadoResp> PostAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ResultadoResp>())!;
    }

    [Fact]
    public async Task Validar_informa_sin_cargar_nada()
    {
        var (api, _) = await EmpresaAsync();
        var r = await PostAsync(api, "/migracion/hispatec/validar", Paquete());

        r.Cargado.Should().BeFalse();
        r.Informe.PuedeCargar.Should().BeTrue();
        r.Informe.Incidencias.Where(i => i.Nivel == "Error").Select(i => i.Codigo).Should().BeEquivalentTo(["cliente.huerfano", "cartera.huerfana", "parcela.sin_agricultor"]);
        r.Informe.Cuadres.CuentasDescuadradasConAcumuladores.Should().Be(1);
        (await api.GetFromJsonAsync<List<ClienteResp>>("/clientes"))!.Should().BeEmpty();
    }

    [Fact]
    public async Task Carga_lo_valido_y_repetir_no_duplica()
    {
        var (api, empresa) = await EmpresaAsync();
        var r = await PostAsync(api, "/migracion/hispatec/cargar", Paquete());
        r.Cargado.Should().BeTrue();
        var paso = r.Pasos.ToDictionary(p => p.Paso);
        paso["Clientes"].Should().Match<PasoResp>(p => p.Creados == 2 && p.Reutilizados == 1, "CLI002 tiene el mismo NIF que CLI001: se fusionan");
        paso["Proveedores"].Creados.Should().Be(2);
        paso["Familias"].Creados.Should().Be(3);
        paso["Artículos"].Creados.Should().Be(2);
        paso["Asiento de apertura"].Creados.Should().Be(1);
        paso["Cartera pendiente"].Creados.Should().Be(3);
        paso["Agricultores"].Creados.Should().Be(1);
        paso["Campañas"].Creados.Should().Be(1);
        paso["Parcelas"].Creados.Should().Be(1);
        r.Pasos.SelectMany(p => p.Errores).Should().BeEmpty();

        // Terceros, catálogo y agro.
        var clientes = (await api.GetFromJsonAsync<List<ClienteResp>>("/clientes"))!;
        clientes.Select(c => c.NifFiscal).Should().BeEquivalentTo(["B12345674", "X1234"]);
        var productos = (await api.GetFromJsonAsync<List<ProductoResp>>("/productos"))!;
        productos.Single(p => p.Referencia == "NAR").Should().Match<ProductoResp>(p => p.CodigoIva == "IVA4" && p.FamiliaId != null);
        var agricultor = (await api.GetFromJsonAsync<List<AgricultorResp>>("/agro/agricultores"))!.Single();
        agricultor.Should().Match<AgricultorResp>(a => a.Nombre == "Juan Labrador Ferrer" && a.Regimen == "Reagp" && a.AutofacturacionDesde == new DateOnly(Anio, 12, 31));
        (await api.GetFromJsonAsync<List<ParcelaResp>>("/agro/parcelas"))!.Single()
            .Should().Match<ParcelaResp>(p => p.Codigo == "HR-500" && p.ReferenciaSigpac == "46:190:0:0:12:45:1" && p.SuperficieHa == 2.5m && p.ProductoId != null);

        // Apertura: el día siguiente al corte, con los saldos de los apuntes y las subcuentas enlazadas con su tercero.
        await using (var c = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin))
        {
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand($"""
                SELECT a.fecha, sum(p.debe), sum(p.haber), sum(p.debe) FILTER (WHERE p.cuenta_codigo = '4300001'),
                       (SELECT count(*) FROM contabilidad.cuenta WHERE empresa_id = '{empresa}' AND codigo IN ('4300001', '4000001') AND tercero_id IS NOT NULL)
                  FROM contabilidad.asiento a JOIN contabilidad.apunte p ON p.asiento_id = a.id
                 WHERE a.empresa_id = '{empresa}' GROUP BY a.fecha
                """, c);
            await using var lector = await cmd.ExecuteReaderAsync();
            (await lector.ReadAsync()).Should().BeTrue();
            lector.GetFieldValue<DateOnly>(0).Should().Be(new DateOnly(Anio + 1, 1, 1));
            lector.GetDecimal(1).Should().Be(11_500m);
            lector.GetDecimal(2).Should().Be(11_500m);
            lector.GetDecimal(3).Should().Be(1_500m, "se usan los apuntes, no el acumulador descuadrado (1.400 €)");
            lector.GetInt64(4).Should().Be(2);
        }

        // La cartera migrada se cobra en ALXOR.
        var cartera = (await api.GetFromJsonAsync<List<EfectoResp>>("/cartera"))!;
        cartera.Should().HaveCount(3);
        var efecto = cartera.Single(e => e.Documento.StartsWith($"F-{Anio}-120", StringComparison.Ordinal));
        efecto.TerceroId.Should().Be(clientes.Single(c => c.NifFiscal == "B12345674").Id);
        var cobro = await api.PostAsJsonAsync($"/cartera/{efecto.Id}/movimientos", new { Importe = 400m, Fecha = new DateOnly(Anio + 1, 1, 10) });
        cobro.StatusCode.Should().Be(HttpStatusCode.OK, await cobro.Content.ReadAsStringAsync());
        (await cobro.Content.ReadFromJsonAsync<SaldoResp>())!.Should().Match<SaldoResp>(s => s.Pendiente == 600m && s.Estado == "Parcial");
        (await api.PostAsJsonAsync($"/cartera/{efecto.Id}/movimientos", new { Importe = 700m })).StatusCode.Should().Be(HttpStatusCode.Conflict, "no se cobra más de lo pendiente");

        // Repetir la carga no duplica nada.
        var otra = await PostAsync(api, "/migracion/hispatec/cargar", Paquete());
        otra.Pasos.Where(p => p.Paso != "Plan de cuentas" && p.Paso != "Subcuentas de terceros").Should().OnlyContain(p => p.Creados == 0);
        otra.Pasos.Single(p => p.Paso == "Asiento de apertura").YaMigrados.Should().Be(1);
        (await api.GetFromJsonAsync<List<ClienteResp>>("/clientes"))!.Should().HaveCount(2);
        (await api.GetFromJsonAsync<List<EfectoResp>>("/cartera?pendientes=false"))!.Should().HaveCount(3);
        (await api.GetFromJsonAsync<List<EjecucionResp>>("/migracion/hispatec/ejecuciones"))!.Should().HaveCount(2);

        // Lo migrado queda como se trajo: ni la cartera ni la correspondencia se reescriben.
        await using var conexion = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await conexion.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", conexion))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var cambio = new NpgsqlCommand($"UPDATE tesoreria.efecto_cartera SET importe = 1 WHERE id = '{efecto.Id}'", conexion);
        var ex = await FluentActions.Awaiting(() => cambio.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
        ex.Which.SqlState.Should().Be(GarantiasSql.CodigoError);
    }

    [Fact]
    public async Task Unos_saldos_descuadrados_no_se_cargan()
    {
        var (api, _) = await EmpresaAsync();
        var archivos = PaqueteEjemplo.Archivos(Anio);
        archivos["saldos.csv"] = "cuenta;debe;haber\n5720001;100;0\n";
        var r = await PostAsync(api, "/migracion/hispatec/cargar", Paquete(archivos));
        r.Cargado.Should().BeFalse();
        r.Informe.PuedeCargar.Should().BeFalse();
        r.Informe.Incidencias.Should().Contain(i => i.Codigo == "saldo.descuadre");
        (await api.GetFromJsonAsync<List<ClienteResp>>("/clientes"))!.Should().BeEmpty();
    }

    [Fact]
    public async Task La_baja_de_la_empresa_borra_lo_migrado()
    {
        var (api, empresa) = await EmpresaAsync();
        (await PostAsync(api, "/migracion/hispatec/cargar", Paquete())).Cargado.Should().BeTrue();
        (await api.DeleteAsync(new Uri("/cuenta", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand($"SELECT (SELECT count(*) FROM migracion.correspondencia WHERE empresa_id = '{empresa}') + (SELECT count(*) FROM tesoreria.efecto_cartera WHERE empresa_id = '{empresa}')", c);
        ((long)(await cmd.ExecuteScalarAsync())!).Should().Be(0);
    }
}
