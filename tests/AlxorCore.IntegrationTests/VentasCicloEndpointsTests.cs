using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del ciclo de venta: pedido → albarán de entrega → factura.</summary>
public sealed class VentasCicloEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public VentasCicloEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaPedidoResp(Guid Id, string Descripcion, decimal Cantidad, decimal CantidadServida, decimal CantidadFacturada, decimal PendienteServir);
    private sealed record PedidoResp(Guid Id, string Estado, string NumeroCompleto, string ClienteNombre, decimal Total, bool ServidoCompleto, Guid? FacturaId, List<LineaPedidoResp> Lineas);
    private sealed record AlbaranResp(Guid Id, string NumeroCompleto, List<System.Text.Json.JsonElement> Lineas);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal Total);

    private static async Task<Guid> CrearClienteAsync(HttpClient c, string nombre = "Cliente Ventas SL") =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = nombre, NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    private static async Task<PedidoResp> CrearPedidoAsync(HttpClient c, Guid clienteId)
    {
        var r = await c.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = clienteId,
            Lineas = new[]
            {
                new { Descripcion = "Servicio A", Cantidad = 2m, PrecioUnitario = 500m, CodigoIva = "IVA21", PorcentajeDescuento = 0m },
                new { Descripcion = "Servicio B", Cantidad = 1m, PrecioUnitario = 200m, CodigoIva = "IVA21", PorcentajeDescuento = 0m },
            },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await r.Content.ReadFromJsonAsync<PedidoResp>())!;
    }

    [Fact]
    public async Task Crear_pedido_calcula_el_total_y_arranca_en_borrador()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);
        var pedido = await CrearPedidoAsync(cliente, clienteId);

        pedido.Estado.Should().Be("Borrador");
        pedido.Total.Should().Be(1200m); // 2·500 + 1·200
        pedido.Lineas.Should().HaveCount(2);
    }

    [Fact]
    public async Task No_se_puede_facturar_un_pedido_sin_confirmar()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);
        var pedido = await CrearPedidoAsync(cliente, clienteId);

        var r = await cliente.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { });
        r.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task El_albaran_actualiza_lo_servido_del_pedido()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);
        var pedido = await CrearPedidoAsync(cliente, clienteId);
        (await cliente.PostAsync($"/pedidos-venta/{pedido.Id}/confirmar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var lineaA = pedido.Lineas.Single(l => l.Descripcion == "Servicio A");
        var entrega = await cliente.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar", new
        {
            Lineas = new[] { new { LineaPedidoId = lineaA.Id, Cantidad = 1m } },
        });
        entrega.StatusCode.Should().Be(HttpStatusCode.OK);

        var recargado = await cliente.GetFromJsonAsync<PedidoResp>($"/pedidos-venta/{pedido.Id}");
        recargado!.Estado.Should().Be("Servido");
        recargado.Lineas.Single(l => l.Id == lineaA.Id).CantidadServida.Should().Be(1m);
        recargado.ServidoCompleto.Should().BeFalse();

        var albaranes = await cliente.GetFromJsonAsync<List<AlbaranResp>>($"/pedidos-venta/{pedido.Id}/albaranes");
        albaranes!.Should().HaveCount(1);
    }

    [Fact]
    public async Task No_se_puede_entregar_mas_de_lo_pedido()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);
        var pedido = await CrearPedidoAsync(cliente, clienteId);
        await cliente.PostAsync($"/pedidos-venta/{pedido.Id}/confirmar", null);

        var lineaB = pedido.Lineas.Single(l => l.Descripcion == "Servicio B"); // cantidad 1
        var r = await cliente.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar", new
        {
            Lineas = new[] { new { LineaPedidoId = lineaB.Id, Cantidad = 5m } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Facturar_un_pedido_confirmado_genera_una_factura_real_y_lo_enlaza()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);
        var pedido = await CrearPedidoAsync(cliente, clienteId);
        await cliente.PostAsync($"/pedidos-venta/{pedido.Id}/confirmar", null);

        var facturar = await cliente.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { });
        facturar.StatusCode.Should().Be(HttpStatusCode.Created);
        var factura = (await facturar.Content.ReadFromJsonAsync<FacturaResp>())!;
        factura.Total.Should().Be(1452m); // 1200 base + 21% IVA

        // La factura existe en el listado de facturas y el pedido queda enlazado y facturado.
        var recargado = await cliente.GetFromJsonAsync<PedidoResp>($"/pedidos-venta/{pedido.Id}");
        recargado!.Estado.Should().Be("Facturado");
        recargado.FacturaId.Should().Be(factura.Id);

        // No se puede facturar dos veces.
        (await cliente.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { })).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Se_puede_crear_un_pedido_desde_un_presupuesto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cliente);

        var presu = await (await cliente.PostAsJsonAsync("/presupuestos", new
        {
            ClienteId = clienteId,
            Validez = "2026-12-31",
            Lineas = new[] { new { Descripcion = "Consultoría", Cantidad = 10m, PrecioUnitario = 100m, CodigoIva = "IVA21", PorcentajeDescuento = 0m } },
        })).Content.ReadFromJsonAsync<IdResp>();

        var r = await cliente.PostAsJsonAsync("/pedidos-venta/desde-presupuesto", new { PresupuestoId = presu!.Id });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        var pedido = (await r.Content.ReadFromJsonAsync<PedidoResp>())!;
        pedido.Total.Should().Be(1000m);
        pedido.Lineas.Should().ContainSingle(l => l.Descripcion == "Consultoría");
    }
}
