using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Cálculo de documentos sin guardar (lo que usa la pantalla mientras se edita) y existencias únicas: con almacenes, las
/// ventas salen del almacén y la ficha del artículo refleja el total de los almacenes.
/// </summary>
public sealed class DocumentosYExistenciasTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DocumentosYExistenciasTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(Guid Id, string Descripcion, decimal PrecioUnitario, decimal PorcentajeDescuento, decimal Base, decimal Margen);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal BaseImponible, decimal CuotaIva, decimal Total, List<LineaResp> Lineas, string? AvisoRiesgo);
    private sealed record ProductoResp(Guid Id, decimal Stock);
    private sealed record ExistenciaResp(Guid AlmacenId, decimal Cantidad);
    private sealed record LineaPedidoResp(Guid Id, decimal Importe, decimal CosteUnitarioEntrada);
    private sealed record PedidoResp(Guid Id, int Numero, decimal Total, List<LineaPedidoResp> Lineas);
    private sealed record PaginaResp(int Total);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    [Fact]
    public async Task Simular_calcula_la_factura_con_la_tarifa_sin_numerarla_ni_guardarla()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = await IdAsync(api, "/productos", new { Nombre = "Aceite 5 L", PrecioUnitario = 30m, PrecioCompra = 18m, Tipo = "Bien", CodigoIva = "IVA10" });
        var tarifa = await IdAsync(api, "/tarifas", new { Codigo = "MAYOR", Nombre = "Mayoristas", Lineas = new[] { new { ProductoId = producto, Precio = (decimal?)25m, PorcentajeDescuento = 4m } } });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Hostelería Levante SL", LimiteRiesgo = 100m });
        (await api.PutAsJsonAsync($"/clientes/{cliente}/tarifa", new { TarifaId = tarifa })).EnsureSuccessStatusCode();

        var r = await api.PostAsJsonAsync("/facturas/simular", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Cantidad = 10m } } });
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        var f = (await r.Content.ReadFromJsonAsync<FacturaResp>())!;
        f.Lineas.Single().Should().Match<LineaResp>(l => l.Descripcion == "Aceite 5 L" && l.PrecioUnitario == 25m && l.PorcentajeDescuento == 4m && l.Base == 240m && l.Margen == 60m);
        f.CuotaIva.Should().Be(24m);
        f.Total.Should().Be(264m);
        f.AvisoRiesgo.Should().Contain("límite de riesgo");

        // No se ha numerado ni guardado nada: la primera factura real es la número 1.
        (await api.GetFromJsonAsync<PaginaResp>("/facturas/buscar"))!.Total.Should().Be(0);
        var real = (await (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Cantidad = 1m } } })).Content.ReadFromJsonAsync<FacturaResp>())!;
        real.NumeroCompleto.Should().EndWith("/000001");

        var pr = await api.PostAsJsonAsync("/compras/pedidos/simular", new { ProveedorTexto = "Almazara Sur", Lineas = new[] { new { Descripcion = "Aceite", Cantidad = 100m, PrecioUnitario = 18m } } });
        pr.StatusCode.Should().Be(HttpStatusCode.OK, await pr.Content.ReadAsStringAsync());
        (await pr.Content.ReadFromJsonAsync<PedidoResp>())!.Should().Match<PedidoResp>(p => p.Numero == 0 && p.Total == 1800m && p.Lineas[0].CosteUnitarioEntrada == 18m);
        (await api.GetFromJsonAsync<List<PedidoResp>>("/compras/pedidos"))!.Should().BeEmpty();
    }

    [Fact]
    public async Task Con_almacenes_la_venta_sale_del_almacen_y_la_ficha_refleja_el_total()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "A1", Nombre = "Central" });
        var producto = await IdAsync(api, "/productos", new { Nombre = "Caja de cartón", PrecioUnitario = 1m, PrecioCompra = 0.5m, Tipo = "Bien", ControlarStock = true, StockInicial = 20m });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Cartonajes SL" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas Bravo" });

        // El stock inicial del alta entra en el almacén principal.
        (await api.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}"))!.Sum(e => e.Cantidad).Should().Be(20m);

        var pedido = (await (await api.PostAsJsonAsync("/compras/pedidos", new { ProveedorId = proveedor, Lineas = new[] { new { ProductoId = producto, Descripcion = "Caja", Cantidad = 100m, PrecioUnitario = 0.5m } } })).Content.ReadFromJsonAsync<PedidoResp>())!;
        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new { AlmacenId = almacen, Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 100m } } }))
            .EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock.Should().Be(120m, "la ficha refleja lo que hay en los almacenes");

        (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Cantidad = 30m } } })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}"))!.Sum(e => e.Cantidad).Should().Be(90m, "la venta sale del almacén");
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock.Should().Be(90m);

        // Vender más de lo que hay no bloquea la factura: el almacén principal queda en negativo, a la vista.
        (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Cantidad = 100m } } })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock.Should().Be(-10m);

        // Los movimientos desde la ficha también van al almacén.
        (await api.PostAsJsonAsync($"/productos/{producto}/stock", new { Tipo = "Entrada", Cantidad = 50m, Motivo = "Regularización" })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}"))!.Sum(e => e.Cantidad).Should().Be(40m);
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock.Should().Be(40m);
    }
}
