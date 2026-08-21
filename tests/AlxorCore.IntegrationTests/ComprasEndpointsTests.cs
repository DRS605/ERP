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
    private sealed record LineaPedResp(Guid Id, string Descripcion, decimal Cantidad, decimal CantidadRecibida, decimal CantidadFacturada);
    private sealed record PedidoResp(Guid Id, string Estado, string ProveedorTexto, decimal Total, bool RecibidoCompleto, List<LineaPedResp> Lineas);
    private sealed record AlbaranResp(Guid Id, Guid PedidoId, List<object> Lineas);
    private sealed record GastoResp(Guid Id, string Concepto, decimal BaseImponible, decimal Total);

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
