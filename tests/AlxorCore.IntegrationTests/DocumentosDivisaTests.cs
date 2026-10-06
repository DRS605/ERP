using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Facturas y gastos en divisa: importes en la divisa con el tipo de cambio congelado y su contravalor en euros; cobros
/// y pagos en divisa con la diferencia de cambio a 668/768 (y su anulación); y presupuestos con todos los tipos de IVA.
/// </summary>
public sealed class DocumentosDivisaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DocumentosDivisaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(decimal PrecioUnitario, decimal Base, decimal CuotaIva, decimal? PrecioDivisa, decimal? BaseDivisa);
    private sealed record FacturaResp(Guid Id, decimal BaseImponible, decimal CuotaIva, decimal Total, string? Moneda, decimal? TasaCambio, decimal? BaseDivisa,
        decimal? CuotaDivisa, decimal? TotalDivisa, List<LineaResp> Lineas);
    private sealed record GastoResp(Guid Id, decimal BaseImponible, decimal CuotaIva, decimal Total, string? Moneda, decimal? TasaCambio, decimal? TotalDivisa);
    private sealed record MovimientoResp(Guid Id, decimal Importe, decimal? ImporteDivisa, decimal DiferenciaCambio);
    private sealed record SaldoResp(decimal Total, decimal Liquidado, decimal Pendiente, string Estado, List<MovimientoResp> Movimientos);
    private sealed record BalanceResp(string CuentaCodigo, decimal SumaDebe, decimal SumaHaber);
    private sealed record PresupuestoResp(Guid Id, decimal BaseImponible, decimal CuotaIva, decimal Total);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode(); // siembra el catálogo (EXPORT)
        return api;
    }

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion, HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(Task<HttpResponseMessage> peticion) => (await (await peticion).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static async Task<decimal> SaldoAsync(HttpClient api, string cuenta) =>
        (await api.GetFromJsonAsync<List<BalanceResp>>("/contabilidad/balance?ejercicio=2026"))!.Where(s => s.CuentaCodigo == cuenta).Sum(s => s.SumaDebe - s.SumaHaber);

    [Fact]
    public async Task La_factura_en_dolares_congela_el_cambio_y_los_cobros_llevan_la_diferencia_a_668_y_768()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-03-01", TasaEur = 0.92m })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Fresh Fruit Inc", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        (await CodigoAsync(api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-10", Moneda = "CHF", Lineas = new[] { new { Descripcion = "Naranja", Cantidad = 1m, PrecioUnitario = 1m, CodigoIva = "EXPORT" } },
        }))).Should().Be("factura.sin_tipo_cambio");

        // 1.200 kg a 1,25 USD (exportación exenta): 1.500 USD, contravalor 1.380 € al 0,92 del día.
        var f = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-10", Moneda = "usd",
            Lineas = new[] { new { Descripcion = "Naranja Navel", Cantidad = 1_200m, PrecioUnitario = 1.25m, CodigoIva = "EXPORT" } },
        }), HttpStatusCode.Created);
        f.Should().Match<FacturaResp>(x => x.Moneda == "USD" && x.TasaCambio == 0.92m && x.BaseDivisa == 1_500m && x.TotalDivisa == 1_500m
            && x.BaseImponible == 1_380m && x.Total == 1_380m);
        f.Lineas.Single().Should().Be(new LineaResp(1.15m, 1_380m, 0m, 1.25m, 1_500m));
        var pdf = await api.GetAsync(new Uri($"/facturas/{f.Id}/pdf", UriKind.Relative));
        Encoding.ASCII.GetString((await pdf.Content.ReadAsByteArrayAsync())[..4]).Should().Be("%PDF");

        // Con IVA y tipo indicado a mano: la cuota que vale es la de euros.
        var nacional = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-10", Moneda = "GBP", TasaCambio = 1.17m,
            Lineas = new[] { new { Descripcion = "Servicio", Cantidad = 3m, PrecioUnitario = 33.33m, CodigoIva = "IVA21" } },
        }), HttpStatusCode.Created);
        nacional.Should().Match<FacturaResp>(x => x.BaseDivisa == 99.99m && x.CuotaDivisa == 21m && x.TotalDivisa == 120.99m
            && x.BaseImponible == 116.99m && x.CuotaIva == 24.57m && x.Total == 141.56m);

        // Primer cobro: 500 USD por los que entran 455 € (al 0,91): se liquidan 460 € de la factura; 5 € de diferencia negativa.
        var s1 = await OkAsync<SaldoResp>(api.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, ImporteDivisa = 500m, Importe = 455m, Fecha = "2026-04-05" }));
        s1.Pendiente.Should().Be(920m);
        (await CodigoAsync(api.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, ImporteDivisa = 1_000.01m, Importe = 900m }))).Should().Be("movimiento.sobrepago");

        // Segundo cobro: los 1.000 USD restantes por 930 €: salda la factura (920 € pendientes) con 10 € de diferencia positiva.
        var s2 = await OkAsync<SaldoResp>(api.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, ImporteDivisa = 1_000m, Importe = 930m, Fecha = "2026-05-05" }));
        s2.Should().Match<SaldoResp>(s => s.Pendiente == 0m && s.Estado == "Liquidado");
        var saldo = (await api.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!;
        saldo.Movimientos.Select(m => (m.Importe, m.ImporteDivisa, m.DiferenciaCambio)).Should().BeEquivalentTo(new[] { (460m, (decimal?)500m, -5m), (920m, (decimal?)1_000m, 10m) });

        (await SaldoAsync(api, "668")).Should().Be(5m);
        (await SaldoAsync(api, "768")).Should().Be(-10m);
        (await SaldoAsync(api, "572")).Should().Be(1_385m, "los euros que entraron en el banco");

        // Se anula el segundo cobro: vuelve lo pendiente y se deshace su diferencia de cambio.
        var segundo = saldo.Movimientos.Single(m => m.Importe == 920m);
        (await api.PostAsJsonAsync($"/tesoreria/movimientos/{segundo.Id}/anular", new { Fecha = "2026-05-06" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!.Pendiente.Should().Be(920m);
        (await SaldoAsync(api, "768")).Should().Be(0m);
        (await SaldoAsync(api, "572")).Should().Be(455m);

        // Sin importe en divisa, los euros liquidan lo mismo (su divisa, al tipo de la factura).
        var s3 = await OkAsync<SaldoResp>(api.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = 920m, Fecha = "2026-05-07" }));
        s3.Estado.Should().Be("Liquidado");
        (await api.GetFromJsonAsync<SaldoResp>($"/facturas/{f.Id}/saldo"))!.Movimientos.Last().ImporteDivisa.Should().Be(1_000m);
        (await CodigoAsync(api.PostAsJsonAsync("/cobros", new { FacturaId = nacional.Id, Importe = 10m, ImporteDivisa = 10m })))
            .Should().NotBe("movimiento.no_divisa", "la factura en libras sí admite importe en divisa");
    }

    [Fact]
    public async Task La_factura_del_proveedor_en_dolares_se_paga_con_su_diferencia_de_cambio()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-02-01", TasaEur = 0.92m })).EnsureSuccessStatusCode();
        var proveedor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Packaging Corp", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // 200 USD + IVA 21 % = 242 USD; en euros 184 + 38,64 = 222,64 €.
        var g = await OkAsync<GastoResp>(api.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Cajas", BaseImponible = 0m, ProveedorId = proveedor, Fecha = "2026-02-10", FechaFactura = "2026-02-10", NumeroFactura = "PC-1", Moneda = "USD",
            Lineas = new[] { new { Base = 200m, CodigoIva = "IVA21" } },
        }), HttpStatusCode.Created);
        g.Should().Match<GastoResp>(x => x.Moneda == "USD" && x.TasaCambio == 0.92m && x.TotalDivisa == 242m && x.BaseImponible == 184m && x.Total == 222.64m);

        // Se pagan los 242 USD con 225 € (el dólar ha subido): 2,36 € de diferencia negativa.
        var s = await OkAsync<SaldoResp>(api.PostAsJsonAsync("/pagos", new { GastoId = g.Id, ImporteDivisa = 242m, Importe = 225m, Fecha = "2026-03-10" }));
        s.Estado.Should().Be("Liquidado");
        (await SaldoAsync(api, "668")).Should().Be(2.36m);
        (await SaldoAsync(api, "572")).Should().Be(-225m);

        var euros = (await (await api.PostAsJsonAsync("/gastos", new { Concepto = "Luz", BaseImponible = 100m, Fecha = "2026-02-10" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await CodigoAsync(api.PostAsJsonAsync("/pagos", new { GastoId = euros, Importe = 10m, ImporteDivisa = 10m }))).Should().Be("movimiento.no_divisa");
    }

    [Fact]
    public async Task El_presupuesto_admite_los_tipos_del_catalogo_de_la_empresa()
    {
        var api = await CompletaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Primeurs SARL", Pais = "FR" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var p = await OkAsync<PresupuestoResp>(api.PostAsJsonAsync("/presupuestos", new
        {
            ClienteId = cliente, Lineas = new object[]
            {
                new { Descripcion = "Naranja", Cantidad = 100m, PrecioUnitario = 1m, CodigoIva = "INTRA" },
                new { Descripcion = "Limón", Cantidad = 100m, PrecioUnitario = 2m, CodigoIva = "EXPORT" },
            },
        }), HttpStatusCode.Created);
        p.Should().Match<PresupuestoResp>(x => x.BaseImponible == 300m && x.CuotaIva == 0m && x.Total == 300m);
        (await CodigoAsync(api.PostAsJsonAsync("/presupuestos", new
        {
            ClienteId = cliente, Lineas = new[] { new { Descripcion = "X", Cantidad = 1m, PrecioUnitario = 1m, CodigoIva = "NOEXISTE" } },
        }))).Should().Be("impuesto.desconocido");
    }
}
