using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Periodificaciones: un gasto o ingreso de varios meses se reclasifica a 480/485 y pasa a resultados mes a mes; se
/// cancela (lo pendiente de una vez) o se anula (todos sus asientos), y el cierre mensual avisa de sus cuotas.
/// </summary>
public sealed class PeriodificacionesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PeriodificacionesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record CuotaResp(int Mes, decimal Importe, Guid? AsientoId);
    private sealed record PeriodificacionResp(Guid Id, string Estado, decimal Imputado, decimal Pendiente, Guid? AsientoReclasificacionId, List<CuotaResp> Cuotas);
    private sealed record GeneracionResp(int Asientos, decimal Importe, int EnMesesCerrados);
    private sealed record SaldoResp(string CuentaCodigo, decimal SumaDebe, decimal SumaHaber);
    private sealed record AsientoResp(Guid Id, int Numero, string Origen, string Diario, int NumeroDiario, DateOnly Fecha);
    private sealed record MesResp(int Mes, int Periodificaciones);
    private sealed record PeriodosResp(List<MesResp> Meses);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        return api;
    }

    private static async Task<PeriodificacionResp> AltaAsync(HttpClient api, object datos)
    {
        var r = await api.PostAsJsonAsync("/contabilidad/periodificaciones", datos);
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<PeriodificacionResp>())!;
    }

    private static async Task<decimal> SaldoAsync(HttpClient api, string cuenta) =>
        (await api.GetFromJsonAsync<List<SaldoResp>>("/contabilidad/balance?ejercicio=2026"))!.Where(s => s.CuentaCodigo == cuenta).Sum(s => s.SumaDebe - s.SumaHaber);

    [Fact]
    public async Task Un_seguro_anual_se_reclasifica_y_pasa_a_gasto_mes_a_mes_hasta_cancelarlo()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/contabilidad/periodificaciones", new { Descripcion = "Mal", Tipo = "Gasto", CuentaResultado = "705", Importe = 100m, Fecha = "2026-03-01", Meses = 12 }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "un gasto va a una cuenta del grupo 6");

        var seguro = await AltaAsync(api, new { Descripcion = "Seguro anual", Tipo = "Gasto", CuentaResultado = "625", Importe = 1_200m, Fecha = "2026-03-01", Meses = 12 });
        seguro.AsientoReclasificacionId.Should().NotBeNull();
        seguro.Cuotas.Should().HaveCount(12).And.OnlyContain(c => c.Importe == 100m);
        (await SaldoAsync(api, "480")).Should().Be(1_200m);

        var gen = (await (await api.PostAsJsonAsync("/contabilidad/periodificaciones/generar", new { Hasta = "2026-05-31" })).Content.ReadFromJsonAsync<GeneracionResp>())!;
        gen.Should().Be(new GeneracionResp(3, 300m, 0));
        (await (await api.PostAsJsonAsync("/contabilidad/periodificaciones/generar", new { Hasta = "2026-05-31" })).Content.ReadFromJsonAsync<GeneracionResp>())!
            .Asientos.Should().Be(0, "cada cuota se contabiliza una vez");
        (await SaldoAsync(api, "480")).Should().Be(900m);
        (await SaldoAsync(api, "625")).Should().Be(-900m, "la reclasificación sacó 1.200 del gasto y han vuelto 300");

        var diario = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026"))!;
        diario.Should().HaveCount(4).And.OnlyContain(a => a.Origen == "Periodificacion" && a.Diario == "PER");
        diario.Skip(1).Select(a => a.Fecha).Should().Equal(new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 30), new DateOnly(2026, 5, 31));
        // Las tres cuotas se grabaron juntas: su número en el diario sigue el del asiento.
        diario.OrderBy(a => a.Numero).Select(a => (a.Numero, a.NumeroDiario)).Should().Equal((1, 1), (2, 2), (3, 3), (4, 4));
        (await (await api.PostAsJsonAsync($"/contabilidad/asientos/{diario[1].Id}/anular", new { })).Content.ReadFromJsonAsync<ProblemaResp>())!
            .Codigo.Should().Be("asiento.de_documento", "se anula desde la periodificación");

        // El cierre de junio avisa de la cuota sin generar.
        (await api.GetFromJsonAsync<PeriodosResp>("/contabilidad/periodos?ejercicio=2026"))!.Meses.Single(m => m.Mes == 6).Periodificaciones.Should().Be(1);
        var cerrar = await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 6 });
        (await cerrar.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("periodo.pendientes");

        // Se cancela el seguro en junio: lo pendiente (900) va al gasto de una vez.
        var cancelada = await api.PostAsJsonAsync($"/contabilidad/periodificaciones/{seguro.Id}/cancelar", new { Fecha = "2026-06-15" });
        cancelada.StatusCode.Should().Be(HttpStatusCode.OK, await cancelada.Content.ReadAsStringAsync());
        (await cancelada.Content.ReadFromJsonAsync<PeriodificacionResp>())!.Estado.Should().Be("Cancelada");
        (await SaldoAsync(api, "480")).Should().Be(0m);
        (await SaldoAsync(api, "625")).Should().Be(0m);
        (await api.PostAsJsonAsync("/contabilidad/periodos/cerrar", new { Anio = 2026, Mes = 6 })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Un_ingreso_anticipado_se_reparte_con_el_redondeo_en_la_ultima_cuota_y_se_anula_entero()
    {
        var api = await CompletaAsync(_fabrica);
        // Se cobran 1.000 de alquiler de enero a marzo (ingreso al 752) y se periodifican en tres meses.
        (await api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-01-10", Concepto = "Cobro alquiler T1",
            Lineas = new[] { new { CuentaCodigo = "572", Debe = 1_000m, Haber = 0m }, new { CuentaCodigo = "752", Debe = 0m, Haber = 1_000m } },
        })).EnsureSuccessStatusCode();
        var ingreso = await AltaAsync(api, new { Descripcion = "Alquiler cobrado", Tipo = "Ingreso", CuentaResultado = "752", Importe = 1_000m, Fecha = "2026-01-10", Meses = 3 });
        ingreso.Cuotas.Select(c => c.Importe).Should().Equal(333.33m, 333.33m, 333.34m);
        (await api.PostAsJsonAsync("/contabilidad/periodificaciones/generar", new { Hasta = "2026-01-31" })).EnsureSuccessStatusCode();
        (await SaldoAsync(api, "752")).Should().Be(-333.33m, "en enero solo es ingreso su tercio");
        (await SaldoAsync(api, "485")).Should().Be(-666.67m);

        (await api.DeleteAsync(new Uri($"/contabilidad/periodificaciones/{ingreso.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await (await api.PostAsJsonAsync($"/contabilidad/periodificaciones/{ingreso.Id}/anular", new { Fecha = "2026-01-15" })).Content.ReadFromJsonAsync<ProblemaResp>())!
            .Codigo.Should().Be("periodificacion.fecha_anulacion", "su cuota de enero es del día 31");
        var anulada = await api.PostAsJsonAsync($"/contabilidad/periodificaciones/{ingreso.Id}/anular", new { Fecha = "2026-02-01" });
        anulada.StatusCode.Should().Be(HttpStatusCode.OK, await anulada.Content.ReadAsStringAsync());
        (await SaldoAsync(api, "752")).Should().Be(-1_000m, "anulada, el ingreso vuelve a estar entero donde lo dejó el cobro");
        (await SaldoAsync(api, "485")).Should().Be(0m);
        (await api.PostAsJsonAsync($"/contabilidad/periodificaciones/{ingreso.Id}/anular", new { Fecha = "2026-02-01" })).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var tras = await api.PostAsJsonAsync("/contabilidad/periodificaciones/generar", new { Hasta = "2026-12-31" });
        (await tras.Content.ReadFromJsonAsync<GeneracionResp>())!.Asientos.Should().Be(0, "una periodificación anulada no genera más cuotas");

        // Sin reclasificar (el importe ya estaba en la 480) y sin asientos: se elimina.
        var sin = await AltaAsync(api, new { Descripcion = "Suscripción", Tipo = "Gasto", CuentaResultado = "629", Importe = 240m, Fecha = "2026-09-01", Meses = 12, Reclasificar = false });
        sin.AsientoReclasificacionId.Should().BeNull();
        (await api.DeleteAsync(new Uri($"/contabilidad/periodificaciones/{sin.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
