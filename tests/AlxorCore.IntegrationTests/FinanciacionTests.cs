using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Préstamos, leasing y pólizas de crédito: asiento de formalización repartido entre corto y largo plazo, cuotas con
/// capital, intereses e IVA, revisión del tipo, amortización anticipada, traspaso a corto plazo al cierre, disposiciones,
/// liquidación de intereses de la póliza y deshacer el último hecho.
/// </summary>
public sealed class FinanciacionTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FinanciacionTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record OperacionResp(Guid Id, string Estado, decimal Pendiente, decimal CortoPlazo, decimal LargoPlazo, decimal TipoVigente, decimal Dispuesto,
        decimal Disponible, int CuotasPagadas, int CuotasTotales, decimal? ProximoImporte);
    private sealed record FilaResp(int Numero, DateOnly Fecha, decimal Intereses, decimal Capital, decimal Cuota, decimal Iva, decimal Total, bool OpcionCompra, string Estado);
    private sealed record EventoResp(Guid Id, string Tipo, decimal Importe, decimal Intereses, decimal Comision, Guid? AsientoId);
    private sealed record DetalleResp(OperacionResp Operacion, List<FilaResp> Cuadro, List<EventoResp> Eventos);
    private sealed record LoteResp(int Asientos, decimal Importe, int EnPeriodosCerrados);
    private sealed record VencimientoResp(string Codigo, DateOnly Fecha, decimal Importe);
    private sealed record SaldoResp(string CuentaCodigo, decimal SumaDebe, decimal SumaHaber);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        return api;
    }

    private static async Task<DetalleResp> OkAsync(Task<HttpResponseMessage> peticion, HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<DetalleResp>())!;
    }

    private static async Task<string> CodigoAsync(Task<HttpResponseMessage> peticion) => (await (await peticion).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    /// <summary>Saldo deudor (debe − haber) de una cuenta en 2026.</summary>
    private static async Task<decimal> SaldoAsync(HttpClient api, string cuenta, int ejercicio = 2026) =>
        (await api.GetFromJsonAsync<List<SaldoResp>>($"/contabilidad/balance?ejercicio={ejercicio}"))!.Where(s => s.CuentaCodigo == cuenta).Sum(s => s.SumaDebe - s.SumaHaber);

    [Fact]
    public async Task Un_prestamo_se_formaliza_paga_revisa_anticipa_y_reclasifica_con_sus_asientos()
    {
        var api = await CompletaAsync(_fabrica);
        var alta = await OkAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Prestamo", Codigo = "ico-26", Descripcion = "Préstamo ICO nave", Entidad = "Banco Uno", FechaFormalizacion = "2026-03-15",
            Capital = 120_000m, TipoInteres = 3m, NumeroCuotas = 60,
        }), HttpStatusCode.Created);
        var id = alta.Operacion.Id;
        alta.Cuadro.Should().HaveCount(60);
        alta.Cuadro[0].Cuota.Should().Be(2_156.24m);
        var corto2026 = alta.Cuadro.Where(c => c.Fecha.Year == 2026).Sum(c => c.Capital);
        alta.Operacion.CortoPlazo.Should().Be(corto2026);
        (await SaldoAsync(api, "572")).Should().Be(120_000m);
        (await SaldoAsync(api, "520")).Should().Be(-corto2026);
        (await SaldoAsync(api, "170")).Should().Be(-(120_000m - corto2026));
        (await CodigoAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Prestamo", Codigo = "ICO-26", Descripcion = "Otro", FechaFormalizacion = "2026-03-15", Capital = 1_000m, TipoInteres = 3m, NumeroCuotas = 12,
        }))).Should().Be("financiacion.codigo_duplicado");

        // Tres cuotas (abril a junio): capital desde el corto plazo e intereses a la 6623, contra el banco.
        var lote = (await (await api.PostAsJsonAsync("/contabilidad/financiacion/cuotas", new { Hasta = "2026-06-30" })).Content.ReadFromJsonAsync<LoteResp>())!;
        lote.Should().Be(new LoteResp(3, 3 * 2_156.24m, 0));
        (await (await api.PostAsJsonAsync("/contabilidad/financiacion/cuotas", new { Hasta = "2026-06-30" })).Content.ReadFromJsonAsync<LoteResp>())!.Asientos
            .Should().Be(0, "cada cuota se contabiliza una vez");
        (await SaldoAsync(api, "6623")).Should().Be(alta.Cuadro.Take(3).Sum(c => c.Intereses));
        (await SaldoAsync(api, "520")).Should().Be(-(corto2026 - alta.Cuadro.Take(3).Sum(c => c.Capital)));

        // Revisión al 4 % desde julio: sube la cuota de lo que queda.
        var revisado = await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/revision", new { Fecha = "2026-07-01", TipoInteres = 4m }));
        revisado.Operacion.TipoVigente.Should().Be(4m);
        revisado.Cuadro[3].Cuota.Should().BeGreaterThan(2_156.24m);
        revisado.Cuadro.Take(3).Should().OnlyContain(c => c.Estado == "Pagada");

        // Anticipo: antes hay que pagar la cuota de julio.
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/anticipo", new { Fecha = "2026-07-20", Importe = 10_000m, Comision = 100m })))
            .Should().Be("financiacion.cuotas_pendientes");
        (await api.PostAsJsonAsync("/contabilidad/financiacion/cuotas", new { Hasta = "2026-07-31", OperacionId = id })).EnsureSuccessStatusCode();
        var antes572 = await SaldoAsync(api, "572");
        var anticipado = await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/anticipo", new { Fecha = "2026-07-20", Importe = 10_000m, Comision = 100m }));
        anticipado.Cuadro.Should().HaveCount(60, "se mantiene el plazo");
        anticipado.Cuadro[4].Cuota.Should().BeLessThan(revisado.Cuadro[4].Cuota);
        (await SaldoAsync(api, "572")).Should().Be(antes572 - 10_100m);
        (await SaldoAsync(api, "626")).Should().Be(100m);

        // Cierre de 2026: el capital de 2027 pasa de 170 a 520.
        var reclas = (await (await api.PostAsJsonAsync("/contabilidad/financiacion/reclasificar", new { Ejercicio = 2026 })).Content.ReadFromJsonAsync<LoteResp>())!;
        reclas.Asientos.Should().Be(1);
        var tras = await OkAsync(api.GetAsync(new Uri($"/contabilidad/financiacion/{id}", UriKind.Relative)));
        reclas.Importe.Should().Be(tras.Cuadro.Where(c => c.Fecha.Year == 2027).Sum(c => c.Capital));
        (await SaldoAsync(api, "520")).Should().Be(-tras.Operacion.CortoPlazo);
        (await SaldoAsync(api, "170")).Should().Be(-tras.Operacion.LargoPlazo);
        (tras.Operacion.CortoPlazo + tras.Operacion.LargoPlazo).Should().Be(tras.Operacion.Pendiente);
        tras.Operacion.Pendiente.Should().Be(tras.Cuadro.Where(c => c.Estado != "Pagada").Sum(c => c.Capital));

        var vencimientos = (await api.GetFromJsonAsync<List<VencimientoResp>>("/contabilidad/financiacion/vencimientos?hasta=2026-12-31"))!;
        vencimientos.Select(v => v.Fecha.Month).Should().Equal(8, 9, 10, 11, 12);

        // Deshacer: solo el último hecho, con su contraasiento.
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/deshacer", new { EventoId = tras.Eventos[1].Id })))
            .Should().Be("financiacion.no_es_el_ultimo");
        var deshecho = await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/deshacer", new { EventoId = tras.Eventos[^1].Id }));
        deshecho.Eventos[^1].Tipo.Should().Be("AmortizacionAnticipada");
        (await SaldoAsync(api, "170")).Should().Be(-deshecho.Operacion.LargoPlazo);

        (await CodigoAsync(api.DeleteAsync(new Uri($"/contabilidad/financiacion/{id}", UriKind.Relative)))).Should().Be("financiacion.con_hechos");
        (await OkAsync(api.PutAsJsonAsync($"/contabilidad/financiacion/{id}", new { Descripcion = "Préstamo ICO nave nueva", Entidad = "Banco Dos" })))
            .Operacion.Estado.Should().Be("Vigente");
    }

    [Fact]
    public async Task El_leasing_da_de_alta_el_bien_y_sus_cuotas_llevan_iva_hasta_la_opcion_de_compra()
    {
        var api = await CompletaAsync(_fabrica);
        (await CodigoAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Leasing", Codigo = "L1", Descripcion = "Furgoneta", FechaFormalizacion = "2026-03-15", Capital = 30_000m, TipoInteres = 3m, NumeroCuotas = 36,
        }))).Should().Be("financiacion.cuenta_activo");
        var alta = await OkAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Leasing", Codigo = "L1", Descripcion = "Furgoneta", FechaFormalizacion = "2026-03-15", Capital = 30_000m, TipoInteres = 3m, NumeroCuotas = 36,
            ValorResidual = 1_000m, PorcentajeIva = 21m, CuentaActivo = "218",
        }), HttpStatusCode.Created);
        alta.Cuadro.Should().HaveCount(37);
        alta.Cuadro[^1].OpcionCompra.Should().BeTrue();
        alta.Cuadro[0].Iva.Should().Be(177.63m, "21 % de 845,86");
        (await SaldoAsync(api, "218")).Should().Be(30_000m);
        (await SaldoAsync(api, "524") + await SaldoAsync(api, "174")).Should().Be(-30_000m);

        (await api.PostAsJsonAsync("/contabilidad/financiacion/cuotas", new { Hasta = "2026-04-30" })).EnsureSuccessStatusCode();
        (await SaldoAsync(api, "472")).Should().Be(177.63m);
        (await SaldoAsync(api, "572")).Should().Be(-(845.86m + 177.63m));
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{alta.Operacion.Id}/anticipo", new { Fecha = "2026-05-01", Importe = 5_000m })))
            .Should().Be("financiacion.leasing_parcial");
    }

    [Fact]
    public async Task La_poliza_dispone_hasta_el_limite_y_liquida_intereses_por_dias_con_la_comision_de_no_disponibilidad()
    {
        var api = await CompletaAsync(_fabrica);
        var alta = await OkAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Poliza", Codigo = "PC1", Descripcion = "Póliza de crédito", FechaFormalizacion = "2026-01-01", FechaVencimiento = "2027-01-01",
            Capital = 50_000m, TipoInteres = 5m, ComisionNoDisponible = 0.5m,
        }), HttpStatusCode.Created);
        var id = alta.Operacion.Id;
        alta.Cuadro.Should().BeEmpty();
        alta.Eventos.Single().AsientoId.Should().BeNull("la póliza no tiene asiento de alta: lo dispuesto se apunta al disponer");

        await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/disposicion", new { Fecha = "2026-01-11", Importe = 20_000m }));
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/disposicion", new { Fecha = "2026-01-12", Importe = 30_001m })))
            .Should().Be("financiacion.limite");
        var tras = await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/reintegro", new { Fecha = "2026-01-21", Importe = 5_000m }));
        tras.Operacion.Dispuesto.Should().Be(15_000m);
        tras.Operacion.Disponible.Should().Be(35_000m);
        (await SaldoAsync(api, "5201")).Should().Be(-15_000m);

        // Enero: 10 días sin disponer, 10 con 20.000 y 11 con 15.000. Intereses (20.000×10 + 15.000×11) × 5 % / 365 = 50,00;
        // no disponibilidad (50.000×10 + 30.000×10 + 35.000×11) × 0,5 % / 365 = 16,23.
        var liq = await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/liquidacion", new { Fecha = "2026-01-31" }));
        liq.Eventos[^1].Should().Match<EventoResp>(e => e.Intereses == 50m && e.Comision == 16.23m && e.AsientoId != null);
        (await SaldoAsync(api, "6623")).Should().Be(50m);
        (await SaldoAsync(api, "626")).Should().Be(16.23m);
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/liquidacion", new { Fecha = "2026-01-31" }))).Should().Be("financiacion.liquidada");
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/disposicion", new { Fecha = "2026-01-25", Importe = 1_000m })))
            .Should().Be("financiacion.fecha", "enero ya está liquidado");

        (await api.GetFromJsonAsync<List<VencimientoResp>>("/contabilidad/financiacion/vencimientos"))!
            .Should().ContainSingle(v => v.Codigo == "PC1" && v.Importe == 15_000m && v.Fecha == new DateOnly(2027, 1, 1));
        (await CodigoAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/cierre", new { Cerrar = true }))).Should().Be("financiacion.dispuesto");
        await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/reintegro", new { Fecha = "2026-02-10", Importe = 15_000m }));
        (await OkAsync(api.PostAsJsonAsync($"/contabilidad/financiacion/{id}/cierre", new { Cerrar = true }))).Operacion.Estado.Should().Be("Cancelada");
        (await SaldoAsync(api, "5201")).Should().Be(0m);
    }

    [Fact]
    public async Task Una_operacion_sin_hechos_se_elimina_y_anula_su_asiento_de_formalizacion()
    {
        var api = await CompletaAsync(_fabrica);
        var alta = await OkAsync(api.PostAsJsonAsync("/contabilidad/financiacion", new
        {
            Tipo = "Prestamo", Codigo = "ERR", Descripcion = "Dado de alta por error", FechaFormalizacion = "2026-05-01", Capital = 6_000m, TipoInteres = 0m,
            NumeroCuotas = 12, Sistema = "CapitalConstante",
        }), HttpStatusCode.Created);
        alta.Cuadro.Should().OnlyContain(c => c.Capital == 500m && c.Intereses == 0m);
        (await api.DeleteAsync(new Uri($"/contabilidad/financiacion/{alta.Operacion.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SaldoAsync(api, "572")).Should().Be(0m);
        (await api.GetFromJsonAsync<List<OperacionResp>>("/contabilidad/financiacion"))!.Should().BeEmpty();
    }
}
