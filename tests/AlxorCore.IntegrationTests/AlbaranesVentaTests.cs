using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// El albarán de venta como documento central (como en Hispatec): saca la mercancía del almacén al emitirse, se entrega
/// a precio por fijar y se valora después, y se facturan varios albaranes del cliente en una sola factura, que ya no
/// mueve existencias. Anular la factura deja los albaranes pendientes; anular un albarán devuelve la mercancía.
/// </summary>
public sealed class AlbaranesVentaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AlbaranesVentaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ProductoResp(Guid Id, decimal Stock);
    private sealed record LineaAlbaranResp(int Orden, string Descripcion, decimal Cantidad, decimal PrecioUnitario, bool PrecioFijado, decimal Base);
    private sealed record AlbaranResp(Guid Id, string NumeroCompleto, Guid? PedidoId, string Estado, decimal Base, Guid? FacturaId, List<LineaAlbaranResp> Lineas);
    private sealed record LineaFacturaResp(string Descripcion, decimal Cantidad, decimal Base, Guid? AlbaranVentaId);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal BaseImponible, decimal Total, DateOnly FechaOperacion, List<LineaFacturaResp> Lineas);
    private sealed record FacturaGeneradaResp(Guid FacturaId, string Numero, Guid ClienteId, int Albaranes, decimal Total);
    private sealed record MasivaResp(List<FacturaGeneradaResp> Facturas, List<object> Errores, List<AlbaranResp> SinValorar);
    private sealed record PedidoResp(Guid Id, string Estado, List<LineaPedidoResp> Lineas);
    private sealed record LineaPedidoResp(Guid Id, decimal CantidadServida, decimal CantidadFacturada);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<decimal> StockAsync(HttpClient api, Guid producto) => (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock;

    private static async Task<AlbaranResp> AlbaranAsync(HttpClient api, object cuerpo)
    {
        var r = await api.PostAsJsonAsync("/albaranes-venta", cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<AlbaranResp>())!;
    }

    [Fact]
    public async Task Albaranes_directos_sacan_stock_se_valoran_y_se_facturan_juntos()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SL" });
        var otro = await IdAsync(api, "/clientes", new { Nombre = "Mercado Central SA" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate rama", PrecioUnitario = 1.20m, Tipo = "Bien", Unidad = "kg", ControlarStock = true, CodigoIva = "IVA4" });
        (await api.PostAsJsonAsync($"/productos/{tomate}/stock", new { Tipo = "Entrada", Cantidad = 1000m })).EnsureSuccessStatusCode();

        // Primer albarán a precio de tarifa (el del artículo): la mercancía sale al emitirlo.
        var a1 = await AlbaranAsync(api, new { ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Cantidad = 100m } } });
        a1.Should().Match<AlbaranResp>(a => a.Estado == "PendienteFacturar" && a.PedidoId == null && a.Base == 120m);
        (await StockAsync(api, tomate)).Should().Be(900m, "el albarán saca la mercancía");

        // Segundo albarán a precio por fijar (venta a resultas): se entrega con un precio estimado.
        var a2 = await AlbaranAsync(api, new { ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Cantidad = 200m, PrecioUnitario = 1m, PrecioPorFijar = true } } });
        a2.Estado.Should().Be("PendienteValorar");
        a2.Lineas.Single().Should().Match<LineaAlbaranResp>(l => !l.PrecioFijado && l.PrecioUnitario == 1m);
        (await StockAsync(api, tomate)).Should().Be(700m);

        // Un albarán de otro cliente, y uno sin precio que no se puede emitir.
        var a3 = await AlbaranAsync(api, new { ClienteId = otro, Lineas = new[] { new { Descripcion = "Portes", Cantidad = 1m, PrecioUnitario = 50m, CodigoIva = "IVA21" } } });
        var sinPrecio = await api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = otro, Lineas = new[] { new { Descripcion = "Algo", Cantidad = 1m } } });
        (await sinPrecio.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("albaranventa.linea_sin_precio");

        // No se factura un albarán sin valorar, ni juntos albaranes de clientes distintos.
        var sinValorar = await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a1.Id, a2.Id } });
        (await sinValorar.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("albaranventa.sin_valorar");
        var mezclados = await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a1.Id, a3.Id } });
        (await mezclados.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("albaranventa.varios_clientes");

        // Valoración a posteriori: precio definitivo 1,10 €/kg.
        var valorado = await api.PutAsJsonAsync($"/albaranes-venta/{a2.Id}/valorar", new { Lineas = new[] { new { Orden = 1, PrecioUnitario = 1.10m } } });
        valorado.StatusCode.Should().Be(HttpStatusCode.OK, await valorado.Content.ReadAsStringAsync());
        (await valorado.Content.ReadFromJsonAsync<AlbaranResp>())!.Should().Match<AlbaranResp>(a => a.Estado == "PendienteFacturar" && a.Base == 220m);

        // Una factura con los dos albaranes: 120 + 220 = 340 de base al 4 %; la factura no vuelve a sacar la mercancía.
        var r = await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a2.Id, a1.Id } });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var factura = (await r.Content.ReadFromJsonAsync<FacturaResp>())!;
        factura.Should().Match<FacturaResp>(f => f.BaseImponible == 340m && f.Total == 353.60m);
        factura.Lineas.Select(l => l.AlbaranVentaId).Should().Equal(a1.Id, a2.Id);
        factura.Lineas[0].Descripcion.Should().StartWith($"Alb. {a1.NumeroCompleto}");
        (await StockAsync(api, tomate)).Should().Be(700m, "la mercancía ya salió con los albaranes");

        (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{a1.Id}"))!.Should().Match<AlbaranResp>(a => a.Estado == "Facturado" && a.FacturaId == factura.Id);
        var otraVez = await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a1.Id } });
        (await otraVez.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("albaranventa.facturado");

        // Un albarán facturado no se anula; anulada la factura, vuelve a estar pendiente y ya se puede anular (la mercancía vuelve).
        var anularFacturado = await api.PostAsJsonAsync($"/albaranes-venta/{a1.Id}/anular", new { Motivo = "Error" });
        (await anularFacturado.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("albaranventa.facturado");
        (await api.PostAsJsonAsync($"/facturas/{factura.Id}/anular", new { Motivo = "Precio mal" })).EnsureSuccessStatusCode();
        var pendientes = (await api.GetFromJsonAsync<List<AlbaranResp>>($"/albaranes-venta?clienteId={cliente}&estado=PendienteFacturar"))!;
        pendientes.Select(a => a.Id).Should().BeEquivalentTo([a1.Id, a2.Id]);
        (await api.PostAsJsonAsync($"/albaranes-venta/{a1.Id}/anular", new { Motivo = "No se entregó" })).EnsureSuccessStatusCode();
        (await StockAsync(api, tomate)).Should().Be(800m, "el albarán anulado devuelve sus 100 kg");

        // Facturación masiva: una factura por cliente con lo pendiente y valorado.
        var a4 = await AlbaranAsync(api, new { ClienteId = otro, Lineas = new[] { new { ProductoId = tomate, Cantidad = 10m, PrecioPorFijar = true } } });
        var masiva = (await (await api.PostAsJsonAsync("/albaranes-venta/facturacion-masiva", new { Hasta = DateOnly.FromDateTime(DateTime.Today) }))
            .Content.ReadFromJsonAsync<MasivaResp>())!;
        masiva.Errores.Should().BeEmpty();
        masiva.Facturas.Should().HaveCount(2);
        masiva.Facturas.Single(f => f.ClienteId == cliente).Should().Match<FacturaGeneradaResp>(f => f.Albaranes == 1 && f.Total == 228.80m);
        masiva.Facturas.Single(f => f.ClienteId == otro).Should().Match<FacturaGeneradaResp>(f => f.Albaranes == 1 && f.Total == 60.50m);
        masiva.SinValorar.Select(a => a.Id).Should().Equal(a4.Id);
    }

    [Fact]
    public async Task El_pedido_se_factura_con_sus_albaranes_y_la_mercancia_sale_una_sola_vez()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Distribuciones Sur SL" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja 5 kg", PrecioUnitario = 6m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        (await api.PostAsJsonAsync($"/productos/{caja}/stock", new { Tipo = "Entrada", Cantidad = 100m })).EnsureSuccessStatusCode();

        var pedido = (await (await api.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { ProductoId = caja, Descripcion = "Caja 5 kg", Cantidad = 30m, PrecioUnitario = 6m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();

        // Dos entregas de 10: cada albarán saca su mercancía.
        foreach (var _ in new[] { 1, 2 })
        {
            (await api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar", new { Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 10m } } }))
                .EnsureSuccessStatusCode();
        }

        (await StockAsync(api, caja)).Should().Be(80m);
        var albaranes = (await api.GetFromJsonAsync<List<AlbaranResp>>($"/albaranes-venta?clienteId={cliente}"))!;
        albaranes.Should().HaveCount(2).And.OnlyContain(a => a.PedidoId == pedido.Id && a.Base == 60m);

        // Se factura el primer albarán solo: el pedido anota 10 facturadas y sigue servido.
        (await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaranes[^1].Id } })).EnsureSuccessStatusCode();
        var tras = (await api.GetFromJsonAsync<PedidoResp>($"/pedidos-venta/{pedido.Id}"))!;
        tras.Should().Match<PedidoResp>(p => p.Estado == "Servido" && p.Lineas[0].CantidadFacturada == 10m);

        // Facturar el pedido recoge el otro albarán (sin mover stock) y las 10 sin servir (esas sí salen).
        var r = await api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var factura = (await r.Content.ReadFromJsonAsync<FacturaResp>())!;
        factura.BaseImponible.Should().Be(120m);
        factura.Lineas.Should().HaveCount(2);
        factura.Lineas.Count(l => l.AlbaranVentaId == albaranes[0].Id).Should().Be(1);
        (await StockAsync(api, caja)).Should().Be(70m, "solo salen las 10 cajas que no llevaban albarán");
        (await api.GetFromJsonAsync<PedidoResp>($"/pedidos-venta/{pedido.Id}"))!.Estado.Should().Be("Facturado");
        (await api.GetFromJsonAsync<List<AlbaranResp>>($"/albaranes-venta?clienteId={cliente}&estado=Facturado"))!.Should().HaveCount(2);
    }
}
