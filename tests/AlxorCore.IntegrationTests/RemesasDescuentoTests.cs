using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Remesas de cobro al descuento y en gestión de cobro, como en Hispatec: al descuento el banco abona el nominal menos
/// intereses (por días hasta el vencimiento, con mínimo), comisión y gastos; las facturas pasan a efectos descontados
/// (4311) y queda la deuda con el banco (5208) hasta el vencimiento, cuando se cancelan entre sí. Un recibo devuelto
/// vuelve al cliente y el banco nos carga su nominal. En gestión de cobro se cobra al vencimiento con comisión e IVA.
/// </summary>
public sealed class RemesasDescuentoTests : IClassFixture<FabricaApiPruebas>
{
    private const string IbanBanco = "ES9121000418450200051332";
    private const string IbanCliente = "ES7620770024003102575766";

    private readonly FabricaApiPruebas _fabrica;

    public RemesasDescuentoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record BancoResp(Guid Id, string Subcuenta);
    private sealed record MovimientoResp(Guid Id, string Sentido, decimal Importe, string? Metodo);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado, List<MovimientoResp> Movimientos);
    private sealed record LineaRemesaResp(Guid Id, string Documento, decimal Importe, Guid? MovimientoId, bool Devuelta);
    private sealed record RemesaResp(Guid Id, string Codigo, string Estado, string EstadoTexto, decimal Total, string Modalidad, decimal Intereses, decimal Comision,
        decimal IvaComision, decimal Gastos, decimal? Liquido, DateOnly? RiesgoCanceladoEn, List<LineaRemesaResp> Lineas);
    private sealed record RemesaCreadaResp(RemesaResp Remesa, List<string> Omitidos);
    private sealed record CalculoResp(int Dias, decimal Intereses, decimal Comision, decimal IvaComision, decimal Gastos, decimal Liquido);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Origen, string Concepto, List<ApunteResp> Apuntes);

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static async Task<(HttpClient C, BancoResp Banco, Guid Cliente)> PrepararAsync(FabricaApiPruebas fabrica)
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await c.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = IbanBanco, IdentificadorAcreedor = "ES12345M1234567890" })).EnsureSuccessStatusCode();
        var banco = await OkAsync<BancoResp>(await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = "Banco", Nombre = "Caja Rural", Iban = IbanBanco, Predeterminada = true }));
        var cliente = (await OkAsync<IdResp>(await c.PostAsJsonAsync("/clientes", new
        {
            Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif(), Iban = IbanCliente, MandatoReferencia = "MND-FDN", MandatoFecha = "2025-01-10",
        }))).Id;
        return (c, banco, cliente);
    }

    private static async Task<FacturaResp> FacturaAsync(HttpClient c, Guid cliente, decimal precio) =>
        await OkAsync<FacturaResp>(await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Tomate", PrecioUnitario = precio, CodigoIva = "IVA0" } },
        }));

    private static async Task<List<AsientoResp>> DiarioAsync(HttpClient c) =>
        (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;

    private static decimal Saldo(IEnumerable<AsientoResp> a, string cuenta) =>
        a.SelectMany(x => x.Apuntes).Where(p => p.CuentaCodigo == cuenta).Sum(p => p.Debe - p.Haber);

    [Fact]
    public async Task Al_descuento_el_banco_abona_el_liquido_el_riesgo_queda_en_5208_y_se_cancela_al_vencer()
    {
        var (c, banco, cliente) = await PrepararAsync(_fabrica);
        var f1 = await FacturaAsync(c, cliente, 6_000m);
        var f2 = await FacturaAsync(c, cliente, 3_000m);
        var vencimiento = Hoy.AddDays(60);

        // 9.000 al 6 % a 60 días = 90 de intereses; 0,5 % de comisión = 45; 3 de gastos por efecto × 2 + 10 fijos = 16.
        var remesa = (await OkAsync<RemesaCreadaResp>(await c.PostAsJsonAsync("/tesoreria/remesas", new
        {
            Tipo = "Cobro", FacturaIds = new[] { f1.Id, f2.Id }, FechaCargo = vencimiento, CuentaBancariaId = banco.Id, Modalidad = "Descuento",
            Condiciones = new { PorcentajeInteres = 6m, DiasMinimos = 15, GastosFijos = 10m, GastosPorEfecto = 3m, PorcentajeComision = 0.5m },
        }))).Remesa;
        remesa.Modalidad.Should().Be("Descuento");
        var calculo = await OkAsync<CalculoResp>(await c.GetAsync(new Uri($"/tesoreria/remesas/{remesa.Id}/calculo?fecha={Hoy:yyyy-MM-dd}", UriKind.Relative)));
        calculo.Should().Be(new CalculoResp(60, 90m, 45m, 0m, 16m, 8_849m));

        var descontada = await OkAsync<RemesaResp>(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { Fecha = Hoy }));
        descontada.Should().Match<RemesaResp>(r => r.EstadoTexto == "Descontada" && r.Liquido == 8_849m && r.Intereses == 90m && r.RiesgoCanceladoEn == null);
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f1.Id}/saldo"))!.Pendiente.Should().Be(0m, "el cliente ya no debe la factura: debe el efecto descontado");

        var diario = await DiarioAsync(c);
        Saldo(diario, banco.Subcuenta).Should().Be(8_849m, "el banco abona el líquido");
        Saldo(diario, "4311").Should().Be(9_000m);
        Saldo(diario, "5208").Should().Be(-9_000m);
        Saldo(diario, "665").Should().Be(90m);
        Saldo(diario, "626").Should().Be(61m);

        // Un movimiento del descuento no se anula suelto: se registra su devolución.
        var cobro1 = descontada.Lineas.Single(l => l.Importe == 6_000m).MovimientoId!.Value;
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/movimientos/{cobro1}/anular", new { }))).Should().Be("movimiento.de_descuento");

        // Devolución antes del vencimiento: la factura vuelve a estar pendiente y el banco nos carga el nominal (contra 5208).
        (await c.PostAsJsonAsync("/tesoreria/devoluciones", new { MovimientoId = cobro1, Motivo = "AM04", Gastos = 0m })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f1.Id}/saldo"))!.Pendiente.Should().Be(6_000m);
        diario = await DiarioAsync(c);
        Saldo(diario, "4311").Should().Be(3_000m);
        Saldo(diario, "5208").Should().Be(-3_000m);
        Saldo(diario, banco.Subcuenta).Should().Be(2_849m);

        // Al vencimiento se cancela el riesgo que queda.
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/cancelar-riesgo", new { Fecha = Hoy }))).Should().Be("remesa.no_vencida");
        var vencida = await OkAsync<RemesaResp>(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/cancelar-riesgo", new { Fecha = vencimiento }));
        vencida.Should().Match<RemesaResp>(r => r.EstadoTexto == "Vencida" && r.RiesgoCanceladoEn == vencimiento);
        diario = await DiarioAsync(c);
        Saldo(diario, "4311").Should().Be(0m);
        Saldo(diario, "5208").Should().Be(0m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/cancelar-riesgo", new { Fecha = vencimiento }))).Should().Be("remesa.riesgo_cancelado");
    }

    [Fact]
    public async Task En_gestion_de_cobro_se_cobra_al_vencimiento_con_su_comision_e_iva()
    {
        var (c, banco, cliente) = await PrepararAsync(_fabrica);
        var f = await FacturaAsync(c, cliente, 2_000m);
        var remesa = (await OkAsync<RemesaCreadaResp>(await c.PostAsJsonAsync("/tesoreria/remesas", new
        {
            Tipo = "Cobro", FacturaIds = new[] { f.Id }, FechaCargo = Hoy.AddDays(5), CuentaBancariaId = banco.Id,
        }))).Remesa;

        // Se pasa a gestión de cobro antes de liquidarla: 1 % de comisión con el 21 % de IVA.
        (await OkAsync<RemesaResp>(await c.PutAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/condiciones", new
        {
            Modalidad = "GestionCobro", Condiciones = new { PorcentajeComision = 1m, PorcentajeIvaComision = 21m },
        }))).Modalidad.Should().Be("GestionCobro");
        var cobrada = await OkAsync<RemesaResp>(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/liquidar", new { }));
        cobrada.Should().Match<RemesaResp>(r => r.EstadoTexto == "Cobrada" && r.Comision == 20m && r.IvaComision == 4.2m && r.Liquido == 1_975.8m);
        (await c.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!.Pendiente.Should().Be(0m);
        var diario = await DiarioAsync(c);
        Saldo(diario, banco.Subcuenta).Should().Be(1_975.8m);
        Saldo(diario, "626").Should().Be(20m);
        Saldo(diario, "472").Should().Be(4.2m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/remesas/{remesa.Id}/cancelar-riesgo", new { }))).Should().Be("remesa.no_descontada");
    }
}
