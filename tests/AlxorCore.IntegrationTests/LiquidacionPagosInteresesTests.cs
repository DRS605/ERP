using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Liquidación de pagos con lo que Hispatec añade al cálculo: intereses de las entregas a cuenta (por días, a 769),
/// retención en el pago (a 4751) y el líquido documentado en un pagaré a fecha (efecto a pagar en 401), más su impreso.
/// </summary>
public sealed class LiquidacionPagosInteresesTests : IClassFixture<FabricaApiPruebas>
{
    private const string IbanBanco = "ES9121000418450200051332";

    private readonly FabricaApiPruebas _fabrica;

    public LiquidacionPagosInteresesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record BancoResp(Guid Id, string Subcuenta);
    private sealed record MovimientoResp(Guid Id, decimal Importe);
    private sealed record SaldoResp(decimal Pendiente, List<MovimientoResp> Movimientos);
    private sealed record LineaResp(string Tipo, decimal Importe);
    private sealed record LiquidacionResp(Guid Id, decimal APagar, decimal EntregasCanceladas, decimal Intereses, decimal Retencion, decimal Liquido, string FormaPago,
        Guid? PagareId, List<LineaResp> Lineas);
    private sealed record EfectoResp(Guid Id, string Sentido, string Documento, DateOnly Vencimiento, decimal Importe, decimal Pendiente);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(List<ApunteResp> Apuntes);

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static async Task<Dictionary<string, decimal>> SaldosAsync(HttpClient c)
    {
        var diario = (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;
        return diario.SelectMany(a => a.Apuntes).GroupBy(p => p.CuentaCodigo.StartsWith("400", StringComparison.Ordinal) ? "400" : p.CuentaCodigo)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Debe - p.Haber));
    }

    [Fact]
    public async Task Descuenta_intereses_y_retencion_y_documenta_el_liquido_en_un_pagare()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        var banco = await OkAsync<BancoResp>(await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = "Banco", Nombre = "Pagos", Iban = IbanBanco, Predeterminada = true }));
        var proveedor = (await OkAsync<IdResp>(await c.PostAsJsonAsync("/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() }))).Id;
        var gasto = (await OkAsync<IdResp>(await c.PostAsJsonAsync("/gastos", new { Concepto = "Fruta de marzo", ProveedorId = proveedor, BaseImponible = 3_000m, CodigoIva = "IVA0" }))).Id;

        // Entrega de 1.000 al 10 % anual hace 73 días: 1.000 × 10 % × 73 / 365 = 20 de intereses.
        (await c.PostAsJsonAsync("/pagos/entregas-cuenta", new { ProveedorId = proveedor, Importe = 1_000m, PorcentajeInteres = 150m })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.PostAsJsonAsync("/pagos/entregas-cuenta", new
        {
            ProveedorId = proveedor, Importe = 1_000m, Fecha = Hoy.AddDays(-73), PorcentajeInteres = 10m, CuentaBancariaId = banco.Id,
        })).EnsureSuccessStatusCode();

        (await CodigoAsync(await c.PostAsJsonAsync("/pagos/liquidaciones", new { ProveedorId = proveedor, FormaPago = "Pagare" }))).Should().Be("liquidacionpagos.pagare");

        // 3.000 − 1.000 de entrega − 20 de intereses − 60 de retención (2 %) = 1.920 en un pagaré a 60 días.
        var l = await OkAsync<LiquidacionResp>(await c.PostAsJsonAsync("/pagos/liquidaciones", new
        {
            ProveedorId = proveedor, FormaPago = "Pagare", PorcentajeRetencion = 2m, VencimientoPagare = Hoy.AddDays(60), NumeroPagare = "000123",
        }));
        l.Should().Match<LiquidacionResp>(x => x.APagar == 3_000m && x.EntregasCanceladas == 1_000m && x.Intereses == 20m && x.Retencion == 60m && x.Liquido == 1_920m
            && x.FormaPago == "Pagare" && x.PagareId != null);
        l.Lineas.Select(x => x.Tipo).Should().Contain(["EntregaCuenta", "Intereses", "Retencion", "Pagare"]);
        (await c.GetFromJsonAsync<SaldoResp>($"/gastos/{gasto}/saldo"))!.Pendiente.Should().Be(0m);
        var pagare = (await c.GetFromJsonAsync<List<EfectoResp>>("/cartera?sentido=Pago"))!.Single(e => e.Id == l.PagareId);
        pagare.Should().Match<EfectoResp>(e => e.Documento == "Pagaré 000123" && e.Importe == 1_920m && e.Vencimiento == Hoy.AddDays(60));
        var s = await SaldosAsync(c);
        s.GetValueOrDefault("400").Should().Be(0m);
        s.GetValueOrDefault("407").Should().Be(0m);
        s.GetValueOrDefault("769").Should().Be(-20m);
        s.GetValueOrDefault("4751").Should().Be(-60m);
        s.GetValueOrDefault("401").Should().Be(-1_920m);

        // Impreso.
        var pdf = await c.GetAsync(new Uri($"/pagos/liquidaciones/{l.Id}/pdf", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK, await pdf.Content.ReadAsStringAsync());
        System.Text.Encoding.ASCII.GetString((await pdf.Content.ReadAsByteArrayAsync())[..4]).Should().Be("%PDF");

        // Se paga el pagaré a su vencimiento: sale de 401. Pagado, la liquidación no se anula; ni el pagaré suelto.
        (await CodigoAsync(await c.PostAsJsonAsync($"/cartera/{pagare.Id}/anular", new { }))).Should().Be("cartera.de_liquidacion");
        (await c.PostAsJsonAsync($"/cartera/{pagare.Id}/movimientos", new { Importe = 1_920m, Metodo = "Transferencia", CuentaBancariaId = banco.Id })).EnsureSuccessStatusCode();
        s = await SaldosAsync(c);
        s.GetValueOrDefault("401").Should().Be(0m);
        s.GetValueOrDefault(banco.Subcuenta).Should().Be(-2_920m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/pagos/liquidaciones/{l.Id}/anular", new { Motivo = "x" }))).Should().Be("liquidacionpagos.pagare_pagado");

        // Anulado el pago del pagaré, la liquidación se anula: todo vuelve a quedar pendiente.
        var pagoPagare = (await c.GetFromJsonAsync<SaldoResp>($"/cartera/{pagare.Id}/saldo"))!.Movimientos.Single(m => m.Importe > 0m).Id;
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{pagoPagare}/anular", new { })).EnsureSuccessStatusCode();
        (await c.PostAsJsonAsync($"/pagos/liquidaciones/{l.Id}/anular", new { Motivo = "Error de retención" })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<SaldoResp>($"/gastos/{gasto}/saldo"))!.Pendiente.Should().Be(3_000m);
        (await c.GetFromJsonAsync<List<EfectoResp>>("/cartera?sentido=Pago&pendientes=true"))!.Should().NotContain(e => e.Id == pagare.Id);
        s = await SaldosAsync(c);
        s.GetValueOrDefault("769").Should().Be(0m);
        s.GetValueOrDefault("4751").Should().Be(0m);
        s.GetValueOrDefault("401").Should().Be(0m);
        s.GetValueOrDefault("407").Should().Be(1_000m, "la entrega vuelve a estar pendiente");
    }
}
