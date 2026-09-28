using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Entregas a cuenta a proveedores y liquidación de pagos, como en Hispatec: la liquidación cancela las entregas a cuenta
/// (400 a 407), compensa lo que el proveedor debe como cliente con el mismo NIF (a través de 555) y paga el líquido; nunca
/// sale negativa; anulada, todo vuelve a quedar pendiente. La masiva liquida a todos y junta los líquidos en una remesa.
/// </summary>
public sealed class LiquidacionesPagosTests : IClassFixture<FabricaApiPruebas>
{
    private const string IbanBanco = "ES9121000418450200051332";
    private const string IbanProveedor = "ES7620770024003102575766";

    private readonly FabricaApiPruebas _fabrica;

    public LiquidacionesPagosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record BancoResp(Guid Id, string Subcuenta);
    private sealed record MovimientoResp(Guid Id, string Sentido, decimal Importe, string? Metodo);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado, List<MovimientoResp> Movimientos);
    private sealed record EntregaResp(Guid Id, decimal Importe, decimal Cancelado, decimal Pendiente, string Estado);
    private sealed record LineaResp(string Tipo, Guid DocumentoId, decimal Importe, Guid? MovimientoId);
    private sealed record LiquidacionResp(Guid Id, string Numero, decimal APagar, decimal EntregasCanceladas, decimal Compensado, decimal Liquido, string FormaPago,
        Guid? RemesaId, Guid? ClienteId, string Estado, List<LineaResp> Lineas);
    private sealed record PropuestaResp(Guid? ClienteId, decimal APagar, decimal EntregasCanceladas, decimal Compensado, decimal Liquido, List<LineaResp> Lineas);
    private sealed record MasivaResp(Guid LoteId, int Liquidaciones, decimal Liquido, Guid? RemesaId, List<LiquidacionResp> Detalle, List<string> Omitidos);
    private sealed record PendienteResp(Guid ProveedorId, int Facturas, decimal Pendiente, Guid? ClienteId, decimal PendienteCliente, decimal Entregas, bool TieneIban);
    private sealed record RemesaResp(Guid Id, string Estado, decimal Total, int NumeroLineas);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Origen, string Concepto, List<ApunteResp> Apuntes);

    private static string Hoy => DateTime.UtcNow.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private static async Task<T> CreadoAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static async Task<HttpClient> EmpresaAsync(FabricaApiPruebas fabrica)
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = IbanBanco, IdentificadorAcreedor = "ES12345M1234567890" })).EnsureSuccessStatusCode();
        return c;
    }

    private static async Task<Guid> GastoAsync(HttpClient c, Guid proveedor, decimal baseImponible, string concepto) =>
        (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/gastos", new { Concepto = concepto, ProveedorId = proveedor, BaseImponible = baseImponible, CodigoIva = "IVA0" }))).Id;

    private static async Task<SaldoResp> SaldoAsync(HttpClient c, string tipo, Guid id) => (await c.GetFromJsonAsync<SaldoResp>($"/{tipo}/{id}/saldo"))!;

    private static decimal Saldo(IEnumerable<AsientoResp> a, string cuenta) =>
        a.SelectMany(x => x.Apuntes).Where(p => p.CuentaCodigo == cuenta).Sum(p => p.Debe - p.Haber);

    [Fact]
    public async Task La_liquidacion_cancela_entregas_compensa_el_mismo_nif_paga_el_liquido_y_se_deshace_al_anularla()
    {
        var c = await EmpresaAsync(_fabrica);
        var banco = await CreadoAsync<BancoResp>(await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = "Banco", Nombre = "Pagos", Iban = IbanBanco, Predeterminada = true }));
        var nif = Ayudas.GenerarNif();
        var proveedor = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/proveedores", new { Nombre = "Juan Labrador", NifFiscal = nif, Iban = IbanProveedor }))).Id;
        var cliente = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/clientes", new { Nombre = "Juan Labrador (cliente)", NifFiscal = nif.Insert(3, "-") }))).Id;
        var g1 = await GastoAsync(c, proveedor, 1_000m, "Liquidación fruta enero");
        var g2 = await GastoAsync(c, proveedor, 500m, "Liquidación fruta febrero");
        var factura = await CreadoAsync<FacturaResp>(await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Plantones y abono", PrecioUnitario = 300m, CodigoIva = "IVA0" } },
        }));

        // Entrega a cuenta (sin IVA): 407 a banco.
        var entrega = await CreadoAsync<EntregaResp>(await c.PostAsJsonAsync("/pagos/entregas-cuenta", new { ProveedorId = proveedor, Importe = 400m, Concepto = "Anticipo de campaña", CuentaBancariaId = banco.Id }));
        entrega.Should().Match<EntregaResp>(e => e.Pendiente == 400m && e.Estado == "Pendiente");

        var pendientes = (await c.GetFromJsonAsync<List<PendienteResp>>($"/pagos/liquidaciones/pendientes?hasta={Hoy}"))!.Single(p => p.ProveedorId == proveedor);
        pendientes.Should().Match<PendienteResp>(p => p.Facturas == 2 && p.Pendiente == 1_500m && p.ClienteId == cliente && p.PendienteCliente == 300m && p.Entregas == 400m && p.TieneIban);

        // 1.500 a pagar − 400 de entrega − 300 que debe como cliente = 800 de líquido.
        var propuesta = await CreadoAsync<PropuestaResp>(await c.PostAsJsonAsync("/pagos/liquidaciones/previsualizar", new { ProveedorId = proveedor, FormaPago = "Directo" }));
        propuesta.Should().Match<PropuestaResp>(p => p.ClienteId == cliente && p.APagar == 1_500m && p.EntregasCanceladas == 400m && p.Compensado == 300m && p.Liquido == 800m);

        var l = await CreadoAsync<LiquidacionResp>(await c.PostAsJsonAsync("/pagos/liquidaciones", new { ProveedorId = proveedor, FormaPago = "Directo", CuentaBancariaId = banco.Id }));
        l.Should().Match<LiquidacionResp>(x => x.Numero.StartsWith("LP-") && x.Liquido == 800m && x.Estado == "Emitida");
        l.Lineas.Should().Contain(x => x.Tipo == "EntregaCuenta" && x.DocumentoId == g1 && x.Importe == 400m);
        l.Lineas.Should().Contain(x => x.Tipo == "CobroCompensado" && x.DocumentoId == factura.Id && x.Importe == 300m);
        l.Lineas.Where(x => x.Tipo == "Pago").Sum(x => x.Importe).Should().Be(800m);
        (await SaldoAsync(c, "gastos", g1)).Pendiente.Should().Be(0m);
        (await SaldoAsync(c, "gastos", g2)).Pendiente.Should().Be(0m);
        (await SaldoAsync(c, "facturas", factura.Id)).Pendiente.Should().Be(0m);
        (await c.GetFromJsonAsync<List<EntregaResp>>($"/pagos/entregas-cuenta?proveedorId={proveedor}"))!.Single().Estado.Should().Be("Cancelada");

        // Asientos: la entrega a cuenta queda saldada, el puente de compensación a cero y del banco solo sale entrega + líquido.
        var diario = (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;
        Saldo(diario, "407").Should().Be(0m);
        Saldo(diario, "555").Should().Be(0m);
        Saldo(diario, banco.Subcuenta).Should().Be(-1_200m);

        // No se deshace una parte suelta: se anula la liquidación entera.
        var compensacion = l.Lineas.First(x => x.Tipo == "Compensacion").MovimientoId!.Value;
        (await CodigoAsync(await c.PostAsJsonAsync($"/tesoreria/movimientos/{compensacion}/anular", new { }))).Should().Be("movimiento.de_liquidacion");
        (await CodigoAsync(await c.PostAsJsonAsync($"/pagos/entregas-cuenta/{entrega.Id}/anular", new { Motivo = "Error" }))).Should().Be("entregacuenta.cancelada");

        (await c.PostAsJsonAsync($"/pagos/liquidaciones/{l.Id}/anular", new { Motivo = "Faltaba una factura" })).EnsureSuccessStatusCode();
        (await SaldoAsync(c, "gastos", g1)).Pendiente.Should().Be(1_000m);
        (await SaldoAsync(c, "gastos", g2)).Pendiente.Should().Be(500m);
        (await SaldoAsync(c, "facturas", factura.Id)).Pendiente.Should().Be(300m);
        (await c.GetFromJsonAsync<List<EntregaResp>>($"/pagos/entregas-cuenta?proveedorId={proveedor}"))!.Single().Pendiente.Should().Be(400m);
        diario = (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;
        Saldo(diario, "407").Should().Be(400m, "la entrega vuelve a estar pendiente de cancelar");
        Saldo(diario, "555").Should().Be(0m);
        Saldo(diario, banco.Subcuenta).Should().Be(-400m, "solo queda la salida de la entrega a cuenta");
        (await CodigoAsync(await c.PostAsJsonAsync($"/pagos/liquidaciones/{l.Id}/anular", new { }))).Should().Be("liquidacionpagos.anulada");
    }

    [Fact]
    public async Task La_compensacion_nunca_deja_la_liquidacion_en_negativo_y_la_entrega_se_aplica_a_mano()
    {
        var c = await EmpresaAsync(_fabrica);
        var nif = Ayudas.GenerarNif();
        var proveedor = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/proveedores", new { Nombre = "Cooperativa del Valle", NifFiscal = nif, Iban = IbanProveedor }))).Id;
        var cliente = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/clientes", new { Nombre = "Cooperativa del Valle", NifFiscal = nif }))).Id;
        var gasto = await GastoAsync(c, proveedor, 200m, "Transporte");
        var factura = await CreadoAsync<FacturaResp>(await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Venta de cajas", PrecioUnitario = 350m, CodigoIva = "IVA0" } },
        }));

        var l = await CreadoAsync<LiquidacionResp>(await c.PostAsJsonAsync("/pagos/liquidaciones", new { ProveedorId = proveedor }));
        l.Should().Match<LiquidacionResp>(x => x.Compensado == 200m && x.Liquido == 0m && x.FormaPago == "Pendiente");
        (await SaldoAsync(c, "facturas", factura.Id)).Pendiente.Should().Be(150m, "solo se compensa hasta cubrir lo que se le debe");
        (await CodigoAsync(await c.PostAsJsonAsync("/pagos/liquidaciones", new { ProveedorId = proveedor }))).Should().Be("liquidacionpagos.sin_facturas");

        // Entrega aplicada a mano a una factura del proveedor; su pago se anula como cualquier otro y la entrega vuelve.
        var otro = await GastoAsync(c, proveedor, 90m, "Reparación");
        var entrega = await CreadoAsync<EntregaResp>(await c.PostAsJsonAsync("/pagos/entregas-cuenta", new { ProveedorId = proveedor, Importe = 100m }));
        var aplicada = await CreadoAsync<EntregaResp>(await c.PostAsJsonAsync($"/pagos/entregas-cuenta/{entrega.Id}/aplicar", new { GastoId = otro }));
        aplicada.Should().Match<EntregaResp>(e => e.Cancelado == 90m && e.Pendiente == 10m && e.Estado == "Parcial");
        var pago = (await SaldoAsync(c, "gastos", otro)).Movimientos.Single();
        pago.Metodo.Should().Be("Entrega a cuenta");
        (await c.PostAsJsonAsync($"/tesoreria/movimientos/{pago.Id}/anular", new { })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<List<EntregaResp>>($"/pagos/entregas-cuenta?proveedorId={proveedor}"))!.Single().Pendiente.Should().Be(100m);
        (await c.PostAsJsonAsync($"/pagos/entregas-cuenta/{entrega.Id}/anular", new { Motivo = "Devuelta" })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task La_liquidacion_masiva_liquida_a_todos_y_junta_los_liquidos_en_una_remesa()
    {
        var c = await EmpresaAsync(_fabrica);
        var banco = await CreadoAsync<BancoResp>(await c.PostAsJsonAsync("/cuentas-bancarias", new { Tipo = "Banco", Nombre = "Pagos", Iban = IbanBanco, Predeterminada = true }));
        var p1 = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/proveedores", new { Nombre = "Agricultor Uno", NifFiscal = Ayudas.GenerarNif(), Iban = IbanProveedor }))).Id;
        var p2 = (await CreadoAsync<IdResp>(await c.PostAsJsonAsync("/proveedores", new { Nombre = "Agricultor Dos", NifFiscal = Ayudas.GenerarNif(), Iban = IbanProveedor }))).Id;
        var g1 = await GastoAsync(c, p1, 700m, "Fruta");
        var g2 = await GastoAsync(c, p2, 300m, "Fruta");
        await CreadoAsync<EntregaResp>(await c.PostAsJsonAsync("/pagos/entregas-cuenta", new { ProveedorId = p1, Importe = 200m, CuentaBancariaId = banco.Id }));

        // Lo de otra empresa no se ve ni se liquida.
        var otra = await EmpresaAsync(_fabrica);
        var ajeno = (await CreadoAsync<IdResp>(await otra.PostAsJsonAsync("/proveedores", new { Nombre = "Proveedor Ajeno", NifFiscal = Ayudas.GenerarNif(), Iban = IbanProveedor }))).Id;
        await GastoAsync(otra, ajeno, 999m, "Ajeno");
        (await c.GetFromJsonAsync<List<PendienteResp>>($"/pagos/liquidaciones/pendientes?hasta={Hoy}"))!.Select(x => x.ProveedorId).Should().BeEquivalentTo([p1, p2]);

        var r = await CreadoAsync<MasivaResp>(await c.PostAsJsonAsync("/pagos/liquidaciones/masiva", new { Hasta = Hoy, FormaPago = "Remesa", CuentaBancariaId = banco.Id }));
        r.Omitidos.Should().BeEmpty();
        r.Liquidaciones.Should().Be(2);
        r.Liquido.Should().Be(800m);
        r.RemesaId.Should().NotBeNull();
        r.Detalle.Should().OnlyContain(x => x.FormaPago == "Remesa" && x.RemesaId == r.RemesaId);
        var remesa = (await c.GetFromJsonAsync<RemesaResp>($"/tesoreria/remesas/{r.RemesaId}"))!;
        remesa.Should().Match<RemesaResp>(x => x.Total == 800m && x.NumeroLineas == 2 && x.Estado == "Generada");
        (await c.GetFromJsonAsync<List<LiquidacionResp>>($"/pagos/liquidaciones?loteId={r.LoteId}"))!.Should().HaveCount(2);

        // Con la remesa viva no se anula; al liquidarla salen los pagos y las facturas quedan pagadas.
        var l1 = r.Detalle.Single(x => x.Liquido == 500m);
        (await CodigoAsync(await c.PostAsJsonAsync($"/pagos/liquidaciones/{l1.Id}/anular", new { }))).Should().Be("liquidacionpagos.en_remesa");
        (await c.PostAsJsonAsync($"/tesoreria/remesas/{r.RemesaId}/liquidar", new { })).EnsureSuccessStatusCode();
        (await SaldoAsync(c, "gastos", g1)).Pendiente.Should().Be(0m);
        (await SaldoAsync(c, "gastos", g2)).Pendiente.Should().Be(0m);
        (await CodigoAsync(await c.PostAsJsonAsync("/pagos/liquidaciones/masiva", new { Hasta = Hoy }))).Should().Be("liquidacionpagos.nada");
    }
}
