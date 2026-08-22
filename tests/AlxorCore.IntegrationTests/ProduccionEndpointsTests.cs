using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Producción (órdenes de fabricación).</summary>
public sealed class ProduccionEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;
    public ProduccionEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record AlmacenResp(Guid Id, string Codigo, string Nombre);
    private sealed record ExistenciaResp(Guid ProductoId, Guid AlmacenId, decimal Cantidad);
    private sealed record ComponentePlanResp(Guid ComponenteId, string Nombre, decimal CantidadUnitaria, decimal CantidadTotal);
    private sealed record OrdenResp(Guid Id, int Numero, string ProductoNombre, decimal Cantidad, string Estado, List<ComponentePlanResp> Componentes);

    [Fact]
    public async Task Orden_de_fabricacion_consume_componentes_y_produce_el_articulo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var a = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Tabla", PrecioUnitario = 1m, PrecioCompra = 0.5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        var bb = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Pata", PrecioUnitario = 1m, PrecioCompra = 0.5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        var mesa = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Mesa", PrecioUnitario = 60m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        // 1 tabla + 4 patas por mesa.
        await cliente.PutAsJsonAsync($"/productos/{mesa.Id}/composicion", new { Componentes = new[] { new { ComponenteId = a.Id, Cantidad = 1m }, new { ComponenteId = bb.Id, Cantidad = 4m } } });

        var alm = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "FAB", Nombre = "Fábrica" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = a.Id, AlmacenId = alm.Id, Cantidad = 100m });
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = bb.Id, AlmacenId = alm.Id, Cantidad = 100m });

        // Orden de 10 mesas: planifica 10 tablas + 40 patas.
        var crear = await cliente.PostAsJsonAsync("/produccion/ordenes", new { ProductoId = mesa.Id, Cantidad = 10m, AlmacenId = alm.Id });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var orden = (await crear.Content.ReadFromJsonAsync<OrdenResp>())!;
        orden.Estado.Should().Be("Planificada");
        orden.Numero.Should().Be(1);
        orden.Componentes.Single(c => c.Nombre == "Tabla").CantidadTotal.Should().Be(10m);
        orden.Componentes.Single(c => c.Nombre == "Pata").CantidadTotal.Should().Be(40m);

        // Terminar: consume 10 tablas y 40 patas, produce 10 mesas.
        (await cliente.PostAsync($"/produccion/ordenes/{orden.Id}/terminar", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var sa = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{a.Id}");
        sa!.Single().Cantidad.Should().Be(90m);
        var sb = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{bb.Id}");
        sb!.Single().Cantidad.Should().Be(60m);
        var sm = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{mesa.Id}");
        sm!.Single().Cantidad.Should().Be(10m);

        var lista = await cliente.GetFromJsonAsync<List<OrdenResp>>("/produccion/ordenes");
        lista!.Single().Estado.Should().Be("Terminada");
    }

    [Fact]
    public async Task No_se_puede_crear_orden_de_un_articulo_no_compuesto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var simple = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Silla", PrecioUnitario = 30m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        var alm = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "F2", Nombre = "Fábrica 2" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        var crear = await cliente.PostAsJsonAsync("/produccion/ordenes", new { ProductoId = simple.Id, Cantidad = 5m, AlmacenId = alm.Id });
        crear.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
