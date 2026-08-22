using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de la cadena de compras: solicitud → pedido → albarán → factura.</summary>
public sealed class ComprasEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ComprasEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record LineaSolResp(Guid Id, string Descripcion, decimal Cantidad);
    private sealed record SolicitudResp(Guid Id, string Estado, List<LineaSolResp> Lineas);
    private sealed record LineaPedResp(Guid Id, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal CantidadRecibida, decimal CantidadFacturada);
    private sealed record PedidoResp(Guid Id, string Estado, int Ejercicio, int Numero, Guid? ProveedorId, string ProveedorTexto, decimal Total, bool RecibidoCompleto, List<LineaPedResp> Lineas);
    private sealed record AlbaranResp(Guid Id, Guid PedidoId, int Numero, List<object> Lineas);
    private sealed record GastoResp(Guid Id, string Concepto, decimal BaseImponible, decimal Total);
    private sealed record ProveedorResp(Guid Id, string Nombre);
    private sealed record AlmacenResp(Guid Id, string Codigo, string Nombre);
    private sealed record UbicacionResp(Guid Id, Guid AlmacenId, string Codigo);
    private sealed record ExistenciaResp(Guid ProductoId, Guid AlmacenId, Guid? UbicacionId, decimal Cantidad);

    [Fact]
    public async Task Cadena_completa_solicitud_pedido_albaran_factura()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // 1) Solicitud de compra.
        var sol = (await (await cliente.PostAsJsonAsync("/compras/solicitudes", new
        {
            ProveedorSugerido = "Suministros Ebro SL",
            Lineas = new[] { new { Descripcion = "Tornillos", Cantidad = 10m }, new { Descripcion = "Tuercas", Cantidad = 20m } },
        })).Content.ReadFromJsonAsync<SolicitudResp>())!;
        sol.Estado.Should().Be("Borrador");

        // 2) Aprobar.
        (await cliente.PostAsync($"/compras/solicitudes/{sol.Id}/aprobar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // 3) Pedido desde la solicitud (con precios).
        var pedidoResp = await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorTexto = "Suministros Ebro SL",
            SolicitudOrigenId = sol.Id,
            Lineas = new[] { new { Descripcion = "Tornillos", Cantidad = 10m, PrecioUnitario = 2m }, new { Descripcion = "Tuercas", Cantidad = 20m, PrecioUnitario = 1m } },
        });
        pedidoResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = (await pedidoResp.Content.ReadFromJsonAsync<PedidoResp>())!;
        pedido.Total.Should().Be(40m);

        // La solicitud queda convertida.
        var sol2 = await cliente.GetFromJsonAsync<List<SolicitudResp>>("/compras/solicitudes");
        sol2!.Single(s => s.Id == sol.Id).Estado.Should().Be("Convertida");

        // 4) Confirmar y recibir mercancía (albarán total).
        (await cliente.PostAsync($"/compras/pedidos/{pedido.Id}/confirmar", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var recibir = await cliente.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new
        {
            Lineas = pedido.Lineas.Select(l => new { LineaPedidoId = l.Id, Cantidad = l.Cantidad }).ToArray(),
        });
        recibir.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedido2 = (await cliente.GetFromJsonAsync<PedidoResp>($"/compras/pedidos/{pedido.Id}"))!;
        pedido2.Estado.Should().Be("Recibido");
        pedido2.RecibidoCompleto.Should().BeTrue();

        // 5) Facturar → genera un gasto por 40 € (base).
        var facturar = await cliente.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/facturar", new { CodigoIva = "IVA21", PorcentajeIrpf = 0m });
        facturar.StatusCode.Should().Be(HttpStatusCode.OK);
        (await facturar.Content.ReadFromJsonAsync<PedidoResp>())!.Estado.Should().Be("Facturado");

        var gastos = await cliente.GetFromJsonAsync<List<GastoResp>>("/gastos");
        var gasto = gastos!.Single(g => g.Concepto.Contains("Suministros Ebro SL"));
        gasto.BaseImponible.Should().Be(40m);
        gasto.Total.Should().Be(48.40m); // 40 + 21%
    }

    [Fact]
    public async Task Numeracion_de_pedidos_por_proveedor_y_ano()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var provA = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Proveedor A" })).Content.ReadFromJsonAsync<ProveedorResp>())!;
        var provB = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Proveedor B" })).Content.ReadFromJsonAsync<ProveedorResp>())!;

        async Task<PedidoResp> Pedir(Guid proveedorId) => (await (await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedorId,
            Fecha = "2026-03-10",
            Lineas = new[] { new { Descripcion = "Material", Cantidad = 1m, PrecioUnitario = 10m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;

        var a1 = await Pedir(provA.Id);
        var a2 = await Pedir(provA.Id);
        var b1 = await Pedir(provB.Id);

        // La serie es independiente por proveedor: A→1,2 y B→1.
        a1.Ejercicio.Should().Be(2026);
        a1.Numero.Should().Be(1);
        a2.Numero.Should().Be(2);
        b1.Numero.Should().Be(1);
    }

    [Fact]
    public async Task Recibir_con_almacen_da_entrada_automatica_en_la_ubicacion_del_proveedor()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = Guid.NewGuid();
        var proveedor = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros Ebro SL" })).Content.ReadFromJsonAsync<ProveedorResp>())!;

        var almacen = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "CENTRAL", Nombre = "Central" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        var ubiGeneral = (await (await cliente.PostAsJsonAsync("/inventario/ubicaciones", new { AlmacenId = almacen.Id, Codigo = "A-1" })).Content.ReadFromJsonAsync<UbicacionResp>())!;
        var ubiProv = (await (await cliente.PostAsJsonAsync("/inventario/ubicaciones", new { AlmacenId = almacen.Id, Codigo = "B-9" })).Content.ReadFromJsonAsync<UbicacionResp>())!;

        // Ubicación por defecto: general del almacén y específica del proveedor (esta debe ganar).
        await cliente.PostAsJsonAsync("/inventario/ubicacion-defecto", new { ProductoId = producto, AlmacenId = almacen.Id, UbicacionId = ubiGeneral.Id });
        await cliente.PostAsJsonAsync("/inventario/ubicacion-defecto", new { ProductoId = producto, AlmacenId = almacen.Id, ProveedorId = proveedor.Id, UbicacionId = ubiProv.Id });

        // Pedido con artículo del catálogo.
        var pedido = (await (await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor.Id,
            Lineas = new[] { new { ProductoId = producto, Descripcion = "Tornillos", Cantidad = 50m, PrecioUnitario = 2m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;

        (await cliente.PostAsync($"/compras/pedidos/{pedido.Id}/confirmar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Recepción parcial (30) indicando almacén → entrada automática.
        var recibir = await cliente.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new
        {
            AlmacenId = almacen.Id,
            Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 30m } },
        });
        recibir.StatusCode.Should().Be(HttpStatusCode.Created);
        (await recibir.Content.ReadFromJsonAsync<AlbaranResp>())!.Numero.Should().Be(1);

        var stock = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}");
        var existencia = stock!.Single(s => s.AlmacenId == almacen.Id && s.UbicacionId == ubiProv.Id);
        existencia.Cantidad.Should().Be(30m);
        // No debe haber caído en la ubicación general.
        stock.Should().NotContain(s => s.UbicacionId == ubiGeneral.Id);
    }

    private sealed record ProductoRespC(Guid Id, string Nombre);

    [Fact]
    public async Task Comprar_en_cajas_da_entrada_en_unidades_base()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var proveedor = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Bebidas Aragón" })).Content.ReadFromJsonAsync<ProveedorResp>())!;
        // Artículo que se compra en cajas de 12 unidades base.
        var prod = (await (await cliente.PostAsJsonAsync("/productos", new
        {
            Nombre = "Refresco", PrecioUnitario = 0.90m, PrecioCompra = 0.50m, CodigoIva = "IVA21",
            Unidad = "ud", UnidadCompra = "caja", FactorCompra = 12m,
        })).Content.ReadFromJsonAsync<ProductoRespC>())!;
        var almacen = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "C", Nombre = "Central" })).Content.ReadFromJsonAsync<AlmacenResp>())!;

        // Pedido de 2 cajas (cantidad en unidad de compra).
        var pedido = (await (await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor.Id,
            Lineas = new[] { new { ProductoId = prod.Id, Descripcion = "Refresco (caja 12)", Cantidad = 2m, PrecioUnitario = 6m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;
        (await cliente.PostAsync($"/compras/pedidos/{pedido.Id}/confirmar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await cliente.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new
        {
            AlmacenId = almacen.Id,
            Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 2m } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        // 2 cajas × 12 = 24 unidades base en stock.
        var stock = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{prod.Id}");
        stock!.Single(s => s.AlmacenId == almacen.Id).Cantidad.Should().Be(24m);
    }

    [Fact]
    public async Task No_se_recibe_un_pedido_sin_confirmar()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var pedido = (await (await cliente.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorTexto = "Proveedor X",
            Lineas = new[] { new { Descripcion = "Algo", Cantidad = 5m, PrecioUnitario = 10m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;

        var recibir = await cliente.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new
        {
            Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 5m } },
        });
        recibir.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
