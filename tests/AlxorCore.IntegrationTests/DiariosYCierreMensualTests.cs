using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Diarios (series) de asientos: cada asiento va a su diario con su propio número, además del correlativo del libro
/// diario; diarios propios que recogen orígenes. Cierre mensual: no se registran asientos en un mes cerrado.
/// </summary>
public sealed class DiariosYCierreMensualTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DiariosYCierreMensualTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record AsientoResp(Guid Id, int Numero, string Origen, string Diario, int NumeroDiario, DateOnly Fecha);
    private sealed record DiarioResp(Guid? Id, string Codigo, bool DeSistema, bool Activo, List<string> Origenes, int Asientos);
    private sealed record PendienteResp(Guid Id, string Sentido, string Estado);
    private sealed record MesResp(int Mes, bool Cerrado, int Asientos, int Pendientes);
    private sealed record PeriodosResp(DateOnly? CerradoHasta, List<MesResp> Meses);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        return api;
    }

    private static Task<HttpResponseMessage> AsientoAsync(HttpClient api, string fecha, string? diario = null) =>
        api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = fecha, Concepto = "Ajuste", Diario = diario,
            Lineas = new[] { new { CuentaCodigo = "629", Debe = 10m, Haber = 0m }, new { CuentaCodigo = "572", Debe = 0m, Haber = 10m } },
        });

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    [Fact]
    public async Task Cada_asiento_va_a_su_diario_con_su_numero_y_los_diarios_propios_recogen_origenes()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-02-10",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-11")).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-12")).EnsureSuccessStatusCode();

        var diario = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!;
        diario.Select(a => (a.Numero, a.Diario, a.NumeroDiario)).Should().Equal((1, "VEN", 1), (2, "GEN", 1), (3, "GEN", 2));

        // Diario propio de regularizaciones: recoge los asientos manuales.
        (await api.PostAsJsonAsync("/contabilidad/diarios", new { Codigo = "VEN", Nombre = "Otro" })).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var reg = await api.PostAsJsonAsync("/contabilidad/diarios", new { Codigo = "reg", Nombre = "Regularizaciones", Origenes = new[] { "Manual" } });
        reg.StatusCode.Should().Be(HttpStatusCode.Created, await reg.Content.ReadAsStringAsync());
        var regId = (await reg.Content.ReadFromJsonAsync<DiarioResp>())!.Id!.Value;
        (await api.PostAsJsonAsync("/contabilidad/diarios", new { Codigo = "REG2", Nombre = "Otro", Origenes = new[] { "Manual" } }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict, "cada origen va a un solo diario propio");
        var diarios = (await api.GetFromJsonAsync<List<DiarioResp>>("/contabilidad/diarios"))!;
        diarios.Single(d => d.Codigo == "GEN").Origenes.Should().BeEmpty("los manuales van ahora a REG");
        diarios.Single(d => d.Codigo == "GEN").Asientos.Should().Be(2);

        (await AsientoAsync(api, "2026-02-13")).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-14", "GEN")).EnsureSuccessStatusCode();
        (await CodigoAsync(await AsientoAsync(api, "2026-02-15", "XYZ"))).Should().Be("asiento.diario");
        var soloReg = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026&diario=REG"))!;
        soloReg.Should().ContainSingle().Which.Should().Match<AsientoResp>(a => a.Numero == 4 && a.NumeroDiario == 1);
        (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026&diario=GEN"))!.Select(a => a.NumeroDiario).Should().Equal(1, 2, 3);

        // El contraasiento va al diario del asiento que anula.
        var anul = await api.PostAsJsonAsync($"/contabilidad/asientos/{soloReg[0].Id}/anular", new { });
        anul.StatusCode.Should().Be(HttpStatusCode.OK, await anul.Content.ReadAsStringAsync());
        (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026&diario=REG"))!.Select(a => (a.Origen, a.NumeroDiario))
            .Should().Equal(("Manual", 1), ("Anulacion", 2));

        // Con asientos no se elimina: se da de baja, y los manuales vuelven al general.
        (await api.DeleteAsync(new Uri($"/contabilidad/diarios/{regId}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await api.PutAsJsonAsync($"/contabilidad/diarios/{regId}", new { Nombre = "Regularizaciones", Origenes = new[] { "Manual" }, Activo = false })).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-16")).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Last().Diario.Should().Be("GEN");
        (await CodigoAsync(await AsientoAsync(api, "2026-02-17", "REG"))).Should().Be("asiento.diario", "un diario de baja no admite asientos");
    }

    [Fact]
    public async Task Un_mes_cerrado_no_admite_asientos_y_sus_pendientes_esperan_a_reabrirlo()
    {
        var api = await CompletaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-01-20",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-05")).EnsureSuccessStatusCode();

        // Con la factura de enero sin contabilizar, cerrar hasta febrero pide confirmación.
        var cerrar = await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 2 });
        cerrar.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await CodigoAsync(cerrar)).Should().Be("periodo.pendientes");
        var periodos = (await (await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 2, Forzar = true })).Content.ReadFromJsonAsync<PeriodosResp>())!;
        periodos.CerradoHasta.Should().Be(new DateOnly(2026, 2, 28));
        periodos.Meses.Take(3).Select(m => (m.Mes, m.Cerrado, m.Asientos, m.Pendientes)).Should().Equal((1, true, 0, 1), (2, true, 1, 0), (3, false, 0, 0));

        // Ni asientos manuales, ni anulaciones, ni contabilizar con fecha de un mes cerrado.
        (await CodigoAsync(await AsientoAsync(api, "2026-02-27"))).Should().Be("asiento.periodo_cerrado");
        (await AsientoAsync(api, "2026-03-01")).EnsureSuccessStatusCode();
        var febrero = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.First();
        (await CodigoAsync(await api.PostAsJsonAsync($"/contabilidad/asientos/{febrero.Id}/anular", new { })))
            .Should().Be("asiento.periodo_cerrado", "el contraasiento lleva la fecha del original");
        (await api.PostAsJsonAsync($"/contabilidad/asientos/{febrero.Id}/anular", new { Fecha = "2026-03-02" })).EnsureSuccessStatusCode();
        var pendiente = (await api.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes"))!.Single(p => p.Sentido == "Venta");
        (await CodigoAsync(await api.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = new[] { pendiente.Id } })))
            .Should().Be("asiento.periodo_cerrado");

        // No se cierra hacia atrás; reabrir enero reabre también febrero.
        (await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 1 })).StatusCode.Should().Be(HttpStatusCode.Conflict);
        periodos = (await (await api.PostAsJsonAsync("/contabilidad/periodos/reabrir", new { Anio = 2026, Mes = 1 })).Content.ReadFromJsonAsync<PeriodosResp>())!;
        periodos.CerradoHasta.Should().Be(new DateOnly(2025, 12, 31), "lo anterior a enero sigue cerrado");
        (await api.PostAsJsonAsync("/contabilidad/pendientes/contabilizar", new { Ids = new[] { pendiente.Id } })).EnsureSuccessStatusCode();
        (await AsientoAsync(api, "2026-02-27")).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Con_contabilizacion_automatica_un_documento_de_un_mes_cerrado_queda_pendiente()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 3 })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var f = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-15",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        });
        f.IsSuccessStatusCode.Should().BeTrue("la factura se emite; lo que espera es su asiento");
        (await api.GetFromJsonAsync<List<PendienteResp>>("/contabilidad/pendientes"))!.Should().ContainSingle(p => p.Sentido == "Venta" && p.Estado == "Pendiente");
        (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!.Should().BeEmpty();
    }
}
