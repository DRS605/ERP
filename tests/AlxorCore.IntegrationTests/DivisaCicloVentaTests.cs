using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Divisa en el ciclo de venta (presupuesto, pedido, albarán y rectificativa: la factura conserva la divisa) y
/// revalorización al cierre de lo pendiente en divisa (668/768 a 31/12 y su reversión el 1/1).
/// </summary>
public sealed class DivisaCicloVentaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DivisaCicloVentaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, decimal BaseImponible, decimal Total, string? Moneda, decimal? TasaCambio, decimal? BaseDivisa, decimal? TotalDivisa);
    private sealed record DocumentoResp(Guid Id, string? Moneda);
    private sealed record LineaPedidoResp(Guid Id);
    private sealed record PedidoResp(Guid Id, string? Moneda, List<LineaPedidoResp> Lineas);
    private sealed record BalanceResp(string CuentaCodigo, decimal SumaDebe, decimal SumaHaber);
    private sealed record LineaRevResp(string TipoDocumento, string Moneda, decimal PendienteDivisa, decimal ValorLibros, decimal TasaCierre, decimal ValorCierre,
        decimal Diferencia, string Cuenta);
    private sealed record RevResp(Guid? Id, int Ejercicio, DateOnly FechaCierre, DateOnly FechaReversion, bool Anulada, decimal Ganancias, decimal Perdidas,
        List<LineaRevResp> Lineas);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        return api;
    }

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion, HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(Task<HttpResponseMessage> peticion) => (await (await peticion).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static async Task<decimal> SaldoAsync(HttpClient api, string cuenta, int ejercicio = 2026) =>
        (await api.GetFromJsonAsync<List<BalanceResp>>($"/contabilidad/balance?ejercicio={ejercicio}"))!.Where(s => s.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal))
            .Sum(s => s.SumaDebe - s.SumaHaber);

    private static object Linea(decimal cantidad, decimal? precio, string iva = "EXPORT") =>
        new { Descripcion = "Naranja", Cantidad = cantidad, PrecioUnitario = precio, CodigoIva = iva };

    [Fact]
    public async Task Presupuesto_pedido_y_albaranes_en_dolares_se_facturan_en_dolares()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-01-01", TasaEur = 0.9m })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Fresh Fruit Inc", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Presupuesto: los precios van en la divisa, así que cada línea lleva el suyo; y no admite conceptos.
        (await CodigoAsync(api.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Moneda = "USD", Lineas = new[] { Linea(1m, null) } })))
            .Should().Be("documento.divisa_precio");
        (await CodigoAsync(api.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Moneda = "US1", Lineas = new[] { Linea(1m, 1m) } })))
            .Should().Be("documento.moneda");
        var presupuesto = await OkAsync<DocumentoResp>(api.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Moneda = "usd", Lineas = new[] { Linea(100m, 2m) } }),
            HttpStatusCode.Created);
        presupuesto.Moneda.Should().Be("USD");
        var pdf = await api.GetAsync(new Uri($"/presupuestos/{presupuesto.Id}/pdf", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);

        // El pedido desde el presupuesto hereda la divisa.
        var desde = await OkAsync<PedidoResp>(api.PostAsJsonAsync("/pedidos-venta/desde-presupuesto", new { PresupuestoId = presupuesto.Id }), HttpStatusCode.Created);
        desde.Moneda.Should().Be("USD");

        // Aceptar el presupuesto: factura en dólares al cambio vigente (200 USD → 180 €).
        var fp = await OkAsync<FacturaResp>(api.PostAsJsonAsync($"/presupuestos/{presupuesto.Id}/aceptar", new { }), HttpStatusCode.Created);
        fp.Should().Match<FacturaResp>(f => f.Moneda == "USD" && f.TasaCambio == 0.9m && f.TotalDivisa == 200m && f.Total == 180m);

        // Pedido en dólares: la entrega genera un albarán en dólares y la factura del pedido, con el cambio indicado, también.
        var pedido = await OkAsync<PedidoResp>(api.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = cliente, Moneda = "USD",
            Lineas = new[] { new { Descripcion = "Naranja", Cantidad = 20m, PrecioUnitario = 1.5m, CodigoIva = "EXPORT" } },
        }), HttpStatusCode.Created);
        pedido.Moneda.Should().Be("USD");
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var entrega = await OkAsync<DocumentoResp>(api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar",
            new { Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 10m } } }));
        entrega.Moneda.Should().Be("USD");
        (await api.GetAsync(new Uri($"/albaranes-venta/{entrega.Id}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await api.GetAsync(new Uri($"/pedidos-venta/{pedido.Id}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        var fPedido = await OkAsync<FacturaResp>(api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { TasaCambio = 0.8m }), HttpStatusCode.Created);
        fPedido.Should().Match<FacturaResp>(f => f.Moneda == "USD" && f.TasaCambio == 0.8m && f.TotalDivisa == 30m && f.Total == 24m);

        // Albaranes directos: no se facturan juntos uno en dólares y otro en euros.
        var aUsd = await OkAsync<DocumentoResp>(api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Moneda = "USD", Lineas = new[] { Linea(10m, 3m) } }),
            HttpStatusCode.Created);
        aUsd.Moneda.Should().Be("USD");
        var aEur = await OkAsync<DocumentoResp>(api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Lineas = new[] { Linea(10m, 3m) } }),
            HttpStatusCode.Created);
        aEur.Moneda.Should().BeNull();
        (await CodigoAsync(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { aUsd.Id, aEur.Id } }))).Should().Be("albaranventa.monedas_distintas");
        var fAlb = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { aUsd.Id } }), HttpStatusCode.Created);
        fAlb.Should().Match<FacturaResp>(f => f.Moneda == "USD" && f.TotalDivisa == 30m && f.Total == 27m);

        // La rectificativa de una factura en dólares va en dólares, al cambio de la original.
        (await CodigoAsync(api.PostAsJsonAsync($"/facturas/{fAlb.Id}/rectificar", new { Motivo = "Precio", Lineas = new[] { Linea(10m, null) } })))
            .Should().Be("documento.divisa_precio");
        var rect = await OkAsync<FacturaResp>(api.PostAsJsonAsync($"/facturas/{fAlb.Id}/rectificar", new { Motivo = "Precio", Lineas = new[] { Linea(10m, 2.5m) } }),
            HttpStatusCode.Created);
        rect.Should().Match<FacturaResp>(f => f.Moneda == "USD" && f.TasaCambio == 0.9m && f.TotalDivisa == 25m && f.Total == 22.5m);
    }

    [Fact]
    public async Task Lo_pendiente_en_divisa_se_revaloriza_al_cierre_y_se_revierte_el_1_de_enero()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-03-01", TasaEur = 0.92m })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Fresh Fruit Inc", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var proveedor = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Packaging Corp", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Factura de 1.500 USD (1.380 €) cobrada en parte: quedan 1.000 USD, 920 € en libros.
        var f = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-10", Moneda = "USD", Lineas = new[] { Linea(1_200m, 1.25m) },
        }), HttpStatusCode.Created);
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, ImporteDivisa = 500m, Importe = 455m, Fecha = "2026-04-05" })).EnsureSuccessStatusCode();

        // Gasto de 242 USD (222,64 €) sin pagar.
        (await api.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Cajas", BaseImponible = 0m, ProveedorId = proveedor, Fecha = "2026-03-10", FechaFactura = "2026-03-10", NumeroFactura = "PC-1", Moneda = "USD",
            Lineas = new[] { new { Base = 200m, CodigoIva = "IVA21" } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        // A 31/12 el dólar vale 0,95 €: el cliente sube 30 € (ganancia) y el proveedor 7,26 € (pérdida).
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-12-31", TasaEur = 0.95m })).EnsureSuccessStatusCode();
        var sim = await OkAsync<RevResp>(api.PostAsJsonAsync("/divisas/revalorizaciones", new { Ejercicio = 2026, Simular = true }));
        sim.Should().Match<RevResp>(r => r.Id == null && r.Ganancias == 30m && r.Perdidas == 7.26m
            && r.FechaCierre == new DateOnly(2026, 12, 31) && r.FechaReversion == new DateOnly(2027, 1, 1));
        sim.Lineas.Select(l => (l.TipoDocumento, l.PendienteDivisa, l.ValorLibros, l.ValorCierre, l.Diferencia, l.Cuenta)).Should().BeEquivalentTo(new[]
        {
            ("Factura", 1_000m, 920m, 950m, 30m, "768"),
            ("Gasto", 242m, 222.64m, 229.9m, 7.26m, "668"),
        });
        (await api.GetFromJsonAsync<List<RevResp>>("/divisas/revalorizaciones"))!.Should().BeEmpty("simular no guarda nada");
        (await SaldoAsync(api, "768")).Should().Be(0m);

        var hecha = await OkAsync<RevResp>(api.PostAsJsonAsync("/divisas/revalorizaciones", new { Ejercicio = 2026 }), HttpStatusCode.Created);
        hecha.Id.Should().NotBeNull();
        (await SaldoAsync(api, "768")).Should().Be(-30m);
        (await SaldoAsync(api, "668")).Should().Be(12.26m, "5 € del cobro parcial y 7,26 € del proveedor");
        (await SaldoAsync(api, "430")).Should().Be(950m, "el cliente queda al cambio del cierre");
        (await SaldoAsync(api, "400")).Should().Be(-229.9m);

        // El 1 de enero se revierte: el nuevo ejercicio parte del valor en libros.
        (await SaldoAsync(api, "768", 2027)).Should().Be(30m);
        (await SaldoAsync(api, "668", 2027)).Should().Be(-7.26m);

        (await CodigoAsync(api.PostAsJsonAsync("/divisas/revalorizaciones", new { Ejercicio = 2026 }))).Should().Be("revalorizacion.hecha");

        // Anular deshace los dos asientos y permite repetirla.
        (await api.PostAsync(new Uri($"/divisas/revalorizaciones/{hecha.Id}/anular", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await SaldoAsync(api, "768")).Should().Be(0m);
        (await SaldoAsync(api, "668")).Should().Be(5m);
        (await SaldoAsync(api, "768", 2027)).Should().Be(0m);
        (await CodigoAsync(api.PostAsync(new Uri($"/divisas/revalorizaciones/{hecha.Id}/anular", UriKind.Relative), null))).Should().Be("revalorizacion.anulada");
        (await api.GetFromJsonAsync<List<RevResp>>("/divisas/revalorizaciones"))!.Should().ContainSingle(r => r.Anulada);
        (await api.PostAsJsonAsync("/divisas/revalorizaciones", new { Ejercicio = 2026 })).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed record ConceptoDivResp(string Codigo, string Efecto, decimal Valor, decimal Importe, decimal? ImporteDivisa);
    private sealed record LineaDivResp(decimal Base, decimal? BaseDivisa, decimal CosteConceptos, List<ConceptoDivResp> Conceptos);
    private sealed record FacturaConceptosResp(Guid Id, decimal BaseImponible, decimal Total, string? Moneda, decimal? BaseDivisa, decimal? TotalDivisa, List<LineaDivResp> Lineas);
    private sealed record AlbaranConceptosResp(Guid Id, string? Moneda, List<LineaDivResp> Lineas);
    private sealed record CargoResp(string Origen, string Codigo, decimal Importe);
    private sealed record SugeridoResp(string Codigo, decimal Valor);

    private static object ConceptoVenta(string codigo, string efecto, string sentido, string calculo, decimal valor, Guid? acreedorId = null) => new
    {
        Codigo = codigo,
        Datos = new { Nombre = codigo.ToLowerInvariant(), Ambito = "Ventas", Efecto = efecto, Sentido = sentido, Calculo = calculo, Valor = valor, Asignaciones = new[] { new { } }, AcreedorId = acreedorId },
    };

    [Fact]
    public async Task Los_cargos_y_abonos_en_divisa_van_en_la_divisa_y_su_contravalor_en_euros()
    {
        var api = await CompletaAsync(_fabrica);
        (await api.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-01-01", TasaEur = 0.8m })).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Fresh Fruit Inc", Pais = "US" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var transportista = (await (await api.PostAsJsonAsync("/proveedores", new { Nombre = "Transportes Levante SL" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Portes 0,10 €/ud con acreedor, rappel del 2 % y comisión (solo coste) de 0,04 €/ud: todos automáticos.
        foreach (var c in new[]
        {
            ConceptoVenta("PORTES", "Precio", "Suma", "PorUnidad", 0.10m, transportista), ConceptoVenta("RAPPEL", "Precio", "Resta", "Porcentaje", 2m),
            ConceptoVenta("COMIS", "Coste", "Suma", "PorUnidad", 0.04m),
        })
        {
            (await api.PostAsJsonAsync("/conceptos-linea", c)).StatusCode.Should().Be(HttpStatusCode.Created);
        }

        (await api.GetFromJsonAsync<List<SugeridoResp>>($"/conceptos-linea/sugeridos?terceroId={cliente}&moneda=USD&fecha=2026-03-10"))!
            .Single(x => x.Codigo == "PORTES").Valor.Should().Be(0.125m, "0,10 € al 0,80 son 0,125 USD");

        // 100 ud a 2 USD: portes 12,50 USD (10 €), rappel −4 USD (−3,20 €), comisión 5 USD (4 €, solo coste).
        var f = await OkAsync<FacturaConceptosResp>(api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = "2026-03-10", Moneda = "USD", Lineas = new[] { Linea(100m, 2m) },
        }), HttpStatusCode.Created);
        f.Should().Match<FacturaConceptosResp>(x => x.BaseDivisa == 208.5m && x.TotalDivisa == 208.5m && x.BaseImponible == 166.8m && x.Total == 166.8m);
        var linea = f.Lineas.Single();
        linea.Should().Match<LineaDivResp>(l => l.BaseDivisa == 208.5m && l.Base == 166.8m && l.CosteConceptos == 4m);
        linea.Conceptos.Select(c => (c.Codigo, c.Valor, c.ImporteDivisa, c.Importe)).Should().BeEquivalentTo(new[]
        {
            ("PORTES", 0.125m, (decimal?)12.5m, 10m), ("RAPPEL", 2m, (decimal?)(-4m), -3.2m), ("COMIS", 0.05m, (decimal?)5m, 4m),
        });
        (await api.GetAsync(new Uri($"/facturas/{f.Id}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // El albarán en dólares lleva los conceptos en dólares; su factura, otra vez con el contravalor.
        var a = await OkAsync<AlbaranConceptosResp>(api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Fecha = "2026-03-10", Moneda = "USD", Lineas = new[] { Linea(100m, 2m) } }),
            HttpStatusCode.Created);
        a.Lineas.Single().Should().Match<LineaDivResp>(l => l.Base == 208.5m && l.CosteConceptos == 5m);
        a.Lineas.Single().Conceptos.Select(c => (c.Codigo, c.Importe)).Should().BeEquivalentTo(new[] { ("PORTES", 12.5m), ("RAPPEL", -4m), ("COMIS", 5m) });
        var fa = await OkAsync<FacturaConceptosResp>(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a.Id } }), HttpStatusCode.Created);
        fa.Should().Match<FacturaConceptosResp>(x => x.Moneda == "USD" && x.BaseDivisa == 208.5m && x.BaseImponible == 166.8m);

        // Lo que se debe al transportista va en euros: 10 € de la factura directa y 10 € del albarán (su factura no cuenta otra vez).
        (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!
            .Select(c => (c.Origen, c.Importe)).Should().BeEquivalentTo(new[] { ("FacturaVenta", 10m), ("AlbaranVenta", 10m) });
    }
}
