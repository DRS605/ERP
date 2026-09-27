using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Modificar documentos que aún no han tenido consecuencias (pedidos sin entregas, solicitudes sin pedido).</summary>
public sealed class EdicionDocumentosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public EdicionDocumentosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(Guid Id, string Descripcion, decimal Cantidad, decimal PrecioUnitario);
    private sealed record PedidoResp(Guid Id, string Estado, Guid? ClienteId, decimal Total, List<LineaResp> Lineas);
    private sealed record ProblemaResp(string Title, string Codigo);

    [Fact]
    public async Task Un_pedido_de_venta_se_modifica_hasta_que_se_entrega()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Cliente", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var otro = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Otro", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var alta = await c.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Lineas = new[] { new { Descripcion = "Caja", Cantidad = 2m, PrecioUnitario = 10m } } });
        alta.IsSuccessStatusCode.Should().BeTrue(await alta.Content.ReadAsStringAsync());
        var pedido = (await alta.Content.ReadFromJsonAsync<PedidoResp>())!;

        var mod = await c.PutAsJsonAsync($"/pedidos-venta/{pedido.Id}", new
        {
            ClienteId = otro,
            Lineas = new[] { new { Descripcion = "Caja grande", Cantidad = 3m, PrecioUnitario = 12m }, new { Descripcion = "Portes", Cantidad = 1m, PrecioUnitario = 5m } },
        });
        mod.StatusCode.Should().Be(HttpStatusCode.OK, await mod.Content.ReadAsStringAsync());
        var tras = (await c.GetFromJsonAsync<PedidoResp>($"/pedidos-venta/{pedido.Id}"))!;
        tras.ClienteId.Should().Be(otro);
        tras.Lineas.Should().HaveCount(2);
        tras.Total.Should().Be(41m);

        // Confirmado y con una entrega: ya no se modifica.
        (await c.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeTrue();
        var linea = tras.Lineas[0];
        (await c.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar", new { Lineas = new[] { new { LineaPedidoId = linea.Id, Cantidad = 1m } } })).IsSuccessStatusCode.Should().BeTrue();
        var bloqueado = await c.PutAsJsonAsync($"/pedidos-venta/{pedido.Id}", new { ClienteId = otro, Lineas = new[] { new { Descripcion = "X", Cantidad = 1m, PrecioUnitario = 1m } } });
        bloqueado.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await bloqueado.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("pedidoventa.no_modificable");
    }

    [Fact]
    public async Task Un_pedido_de_compra_y_una_solicitud_se_modifican_y_la_solicitud_se_elimina()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var alta = await c.PostAsJsonAsync("/compras/pedidos", new { ProveedorTexto = "Suministros", Lineas = new[] { new { Descripcion = "Tornillos", Cantidad = 100m, PrecioUnitario = 0.1m } } });
        alta.IsSuccessStatusCode.Should().BeTrue(await alta.Content.ReadAsStringAsync());
        var pedido = (await alta.Content.ReadFromJsonAsync<PedidoResp>())!;
        var mod = await c.PutAsJsonAsync($"/compras/pedidos/{pedido.Id}", new { ProveedorTexto = "Suministros", Lineas = new[] { new { Descripcion = "Tornillos M6", Cantidad = 200m, PrecioUnitario = 0.1m } } });
        mod.StatusCode.Should().Be(HttpStatusCode.OK, await mod.Content.ReadAsStringAsync());
        (await c.GetFromJsonAsync<PedidoResp>($"/compras/pedidos/{pedido.Id}"))!.Total.Should().Be(20m);

        var sol = (await (await c.PostAsJsonAsync("/compras/solicitudes", new { Lineas = new[] { new { Descripcion = "Guantes", Cantidad = 10m } } })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PutAsJsonAsync($"/compras/solicitudes/{sol}", new { Lineas = new[] { new { Descripcion = "Guantes de nitrilo", Cantidad = 20m } }, Notas = "Talla M" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.DeleteAsync(new Uri($"/compras/solicitudes/{sol}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Una aprobada ya no se elimina.
        var aprobada = (await (await c.PostAsJsonAsync("/compras/solicitudes", new { Lineas = new[] { new { Descripcion = "Cajas", Cantidad = 5m } } })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PostAsync(new Uri($"/compras/solicitudes/{aprobada}/aprobar", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeTrue();
        (await c.DeleteAsync(new Uri($"/compras/solicitudes/{aprobada}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
