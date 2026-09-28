using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Situación de la deuda de clientes, como en Hispatec: un recibo devuelto pasa a impagados (4315); lo que se cobra
/// después sale de allí; la deuda dudosa pasa a 436 con su deterioro (694/490), que se revierte al cobrar (794) y se
/// aplica al declararla incobrable (650); y la renovación cancela el documento contra efectos en cartera (4310) con
/// gastos para el cliente (769). Anular un cobro deshace lo que movió.
/// </summary>
public sealed class DeudaClientesTests : IClassFixture<FabricaApiPruebas>
{
    private const string IbanBanco = "ES9121000418450200051332";
    private const string IbanCliente = "ES7620770024003102575766";

    private readonly FabricaApiPruebas _fabrica;

    public DeudaClientesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record BancoResp(Guid Id, string Subcuenta);
    private sealed record MovimientoResp(Guid Id, string Sentido, decimal Importe, string? Metodo);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, List<MovimientoResp> Movimientos);
    private sealed record LineaRemesaResp(Guid Id, decimal Importe, Guid? MovimientoId);
    private sealed record RemesaResp(Guid Id, List<LineaRemesaResp> Lineas);
    private sealed record RemesaCreadaResp(RemesaResp Remesa);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, List<ApunteResp> Apuntes);
    private sealed record SituacionResp(Guid Id, Guid DocumentoId, string? Cuenta, decimal Importe, string Clasificacion, decimal Dotado, DateOnly? IncobrableEl);
    private sealed record EfectoResp(Guid Id, string Documento, DateOnly Vencimiento, decimal Importe, decimal Pendiente);
    private sealed record RenovacionResp(Guid Id, decimal Importe, decimal Gastos, bool Anulada, List<EfectoResp> Efectos);

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
        return diario.SelectMany(a => a.Apuntes).GroupBy(p => p.CuentaCodigo.StartsWith("430", StringComparison.Ordinal) ? "430" : p.CuentaCodigo)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Debe - p.Haber));
    }

    private static decimal S(Dictionary<string, decimal> s, string cuenta) => s.GetValueOrDefault(cuenta);

    [Fact]
    public async Task Impagado_dudoso_incobrable_y_renovacion_llevan_la_deuda_por_sus_cuentas()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await c.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = IbanBanco, IdentificadorAcreedor = "ES12345M1234567890" })).EnsureSuccessStatusCode();
        var banco = await OkAsync<BancoResp>(await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = "Banco", Nombre = "Caja Rural", Iban = IbanBanco, Predeterminada = true }));
        var cliente = (await OkAsync<IdResp>(await c.PostAsJsonAsync("/clientes", new
        {
            Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif(), Iban = IbanCliente, MandatoReferencia = "MND-FDN", MandatoFecha = "2025-01-10",
        }))).Id;
        (await c.PutAsJsonAsync("/tesoreria/cartera/configuracion", new { ImpagadosA4315 = true, PorcentajeRenovacion = 1m })).EnsureSuccessStatusCode();
        var f = await OkAsync<FacturaResp>(await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Tomate", PrecioUnitario = 1_000m, CodigoIva = "IVA0" } },
        }));

        // Remesa cobrada y recibo devuelto: la deuda pasa del cliente a impagados.
        var remesa = (await OkAsync<RemesaCreadaResp>(await c.PostAsJsonAsync("/tesoreria/remesas", new
        {
            Tipo = "Cobro", FacturaIds = new[] { f.Id }, FechaCargo = Hoy, CuentaBancariaId = banco.Id,
        }))).Remesa;
        var cobrada = await OkAsync<RemesaResp>(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { Fecha = Hoy }));
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = cobrada.Lineas[0].MovimientoId, Motivo = "AM04" })).EnsureSuccessStatusCode();
        var s = await SaldosAsync(c);
        S(s, "4315").Should().Be(1_000m);
        S(s, "430").Should().Be(0m);

        // Cobra 400 en mano: salen de impagados.
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 400m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        s = await SaldosAsync(c);
        S(s, "4315").Should().Be(600m);
        S(s, "430").Should().Be(0m);

        // Dudoso con el 50 % de deterioro: 600 a 436 y 300 dotados.
        var dudosa = await OkAsync<SituacionResp>(await c.PostAsJsonAsync($"/tesoreria/deudas/Factura/{f.Id}/clasificar", new { Clasificacion = "Dudoso", PorcentajeDotacion = 50m }));
        dudosa.Should().Match<SituacionResp>(x => x.Cuenta == "436" && x.Importe == 600m && x.Dotado == 300m && x.Clasificacion == "Dudoso");
        s = await SaldosAsync(c);
        S(s, "4315").Should().Be(0m);
        S(s, "436").Should().Be(600m);
        S(s, "694").Should().Be(300m);
        S(s, "490").Should().Be(-300m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/deudas/{dudosa.Id}/dotar", new { Importe = 700m }))).Should().Be("deuda.dotacion");

        // Cobra 200: salen de 436 y se revierte su parte del deterioro (100); anular el cobro lo deshace.
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 200m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        var saldo = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!;
        s = await SaldosAsync(c);
        S(s, "436").Should().Be(400m);
        S(s, "490").Should().Be(-200m);
        S(s, "794").Should().Be(-100m);
        var cobro200 = saldo.Movimientos.Single(m => m.Importe == 200m).Id;
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobro200}/anular", new { })).EnsureSuccessStatusCode();
        s = await SaldosAsync(c);
        S(s, "436").Should().Be(600m);
        S(s, "490").Should().Be(-300m);
        S(s, "430").Should().Be(0m);

        // Incobrable: lo que queda (600) a pérdidas y el deterioro se aplica.
        var incobrable = await OkAsync<SituacionResp>(await c.PostAsJsonAsync($"/tesoreria/deudas/Factura/{f.Id}/incobrable", new { Motivo = "Concurso" }));
        incobrable.IncobrableEl.Should().Be(Hoy);
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!.Pendiente.Should().Be(0m);
        s = await SaldosAsync(c);
        S(s, "436").Should().Be(0m);
        S(s, "650").Should().Be(600m);
        S(s, "490").Should().Be(0m);
        S(s, "430").Should().Be(0m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/deudas/Factura/{f.Id}/incobrable", new { }))).Should().Be("deuda.sin_pendiente");

        // Renovación de otra factura (500) en dos efectos, con el 1 % de gastos: 505 en cartera (4310) y 5 a 769.
        var f2 = await OkAsync<FacturaResp>(await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Pepino", PrecioUnitario = 500m, CodigoIva = "IVA0" } },
        }));
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/deudas/Factura/{f2.Id}/renovar", new
        {
            Vencimientos = new[] { new { Fecha = Hoy.AddDays(30), Importe = 250m }, new { Fecha = Hoy.AddDays(60), Importe = 250m } },
        }))).Should().Be("renovacion.cuadre");
        var ren = await OkAsync<RenovacionResp>(await c.PostAsJsonAsync($"/tesoreria/deudas/Factura/{f2.Id}/renovar", new
        {
            Vencimientos = new[] { new { Fecha = Hoy.AddDays(30), Importe = 252.50m }, new { Fecha = Hoy.AddDays(60), Importe = 252.50m } },
        }));
        ren.Should().Match<RenovacionResp>(r => r.Importe == 500m && r.Gastos == 5m && r.Efectos.Count == 2 && r.Efectos.All(e => e.Pendiente == 252.50m));
        var saldo2 = (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f2.Id}/saldo"))!;
        saldo2.Pendiente.Should().Be(0m);
        s = await SaldosAsync(c);
        S(s, "4310").Should().Be(505m);
        S(s, "769").Should().Be(-5m);
        S(s, "430").Should().Be(0m);

        // No se anula el cobro de la renovación ni un efecto suelto; cobrar un efecto sale de 4310.
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/movimientos/{saldo2.Movimientos.Single().Id}/anular", new { }))).Should().Be("movimiento.de_renovacion");
        (await CodigoAsync(await c.PostAsJsonAsync($"/cartera/{ren.Efectos[0].Id}/anular", new { }))).Should().Be("cartera.de_renovacion");
        (await c.PostAsJsonAsync($"/cartera/{ren.Efectos[0].Id}/movimientos", new { Importe = 252.50m, Metodo = "Transferencia" })).EnsureSuccessStatusCode();
        var cobroEfecto = (await c.GetFromJsonAsync<SaldoResp>($"/cartera/{ren.Efectos[0].Id}/saldo"))!;
        s = await SaldosAsync(c);
        S(s, "4310").Should().Be(252.50m);
        (await CodigoAsync(await c.PostAsync(new Uri($"/tesoreria/renovaciones/{ren.Id}/anular", UriKind.Relative), null))).Should().Be("renovacion.con_cobros");

        // Anulado el cobro del efecto, la renovación se deshace: la factura vuelve a deber 500.
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobroEfecto.Movimientos.Single(m => m.Importe > 0m).Id}/anular", new { })).EnsureSuccessStatusCode();
        (await OkAsync<RenovacionResp>(await c.PostAsync(new Uri($"/tesoreria/renovaciones/{ren.Id}/anular", UriKind.Relative), null))).Anulada.Should().BeTrue();
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f2.Id}/saldo"))!.Pendiente.Should().Be(500m);
        s = await SaldosAsync(c);
        S(s, "4310").Should().Be(0m);
        S(s, "769").Should().Be(0m);
        S(s, "430").Should().Be(500m);
        (await c.GetFromJsonAsync<List<SituacionResp>>("/tesoreria/deudas"))!.Should().ContainSingle(x => x.DocumentoId == f.Id && x.IncobrableEl != null);
    }
}
