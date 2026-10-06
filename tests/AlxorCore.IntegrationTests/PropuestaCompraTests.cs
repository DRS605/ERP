using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Stock mínimo y máximo, cálculo de necesidades de fabricación y propuesta de compra convertida en pedidos.</summary>
public sealed class PropuestaCompraTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PropuestaCompraTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid ProductoId, string Producto, string Motivo, decimal Stock, decimal PendienteRecibir, decimal NecesidadFabricacion, decimal Disponible,
        decimal CantidadBase, decimal CantidadCompra, string UnidadCompra, Guid? ProveedorId, string? Proveedor, decimal PrecioCompra, decimal Importe);
    private sealed record PropuestaResp(List<LineaResp> Lineas, decimal Importe, int SinProveedor);
    private sealed record LineaPedidoResp(Guid? ProductoId, decimal Cantidad, decimal PrecioUnitario);
    private sealed record PedidoResp(Guid Id, string Estado, Guid? ProveedorId, List<LineaPedidoResp> Lineas);
    private sealed record GeneradosResp(List<PedidoResp> Pedidos, List<string> Omitidas);

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> FalloAsync(Task<HttpResponseMessage> peticion, HttpStatusCode esperado)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    [Fact]
    public async Task Propuesta_por_minimos_y_por_fabricacion_y_pedidos_por_proveedor()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var maderas = (await OkAsync<IdResp>(api.PostAsJsonAsync("/proveedores", new { Nombre = "Maderas del Norte", NifFiscal = Ayudas.GenerarNif() }))).Id;
        var tornillos = (await OkAsync<IdResp>(api.PostAsJsonAsync("/proveedores", new { Nombre = "Tornillería Express", NifFiscal = Ayudas.GenerarNif() }))).Id;
        var almacen = (await OkAsync<IdResp>(api.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "C", Nombre = "Central" }))).Id;
        // Tablero: se compra por unidades. Tornillo: en cajas de 100.
        var tablero = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Tablero roble", PrecioUnitario = 30m, PrecioCompra = 12m, CodigoIva = "IVA21", ProveedorHabitualId = maderas }))).Id;
        var tornillo = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new
        {
            Nombre = "Tornillo 4x40", PrecioUnitario = 0.05m, PrecioCompra = 0.02m, CodigoIva = "IVA21", Unidad = "ud", UnidadCompra = "caja", FactorCompra = 100m, ProveedorHabitualId = tornillos,
        }))).Id;
        var cola = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Cola blanca", PrecioUnitario = 5m, PrecioCompra = 2m, CodigoIva = "IVA21" }))).Id;
        var mesa = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Mesa", PrecioUnitario = 200m, CodigoIva = "IVA21" }))).Id;
        (await api.PutAsJsonAsync($"/productos/{mesa}/composicion", new
        {
            Componentes = new[] { new { ComponenteId = tablero, Cantidad = 2m }, new { ComponenteId = tornillo, Cantidad = 30m } },
        })).EnsureSuccessStatusCode();

        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = tablero, AlmacenId = almacen, Cantidad = 8m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = tornillo, AlmacenId = almacen, Cantidad = 150m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = cola, AlmacenId = almacen, Cantidad = 3m })).EnsureSuccessStatusCode();

        // Reglas: tablero mínimo 10 / máximo 20 en múltiplos de 5; cola mínimo 5 / máximo 12.
        (await FalloAsync(api.PostAsJsonAsync("/inventario/reaprovisionamiento", new { ProductoId = cola, Minimo = 5m, Maximo = 2m }), HttpStatusCode.BadRequest))
            .Should().Be("reaprovisionamiento.maximo");
        (await api.PostAsJsonAsync("/inventario/reaprovisionamiento", new { ProductoId = tablero, Minimo = 10m, Maximo = 20m, Multiplo = 5m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/reaprovisionamiento", new { ProductoId = cola, Minimo = 5m, Maximo = 12m })).EnsureSuccessStatusCode();
        // Volver a fijarla la cambia (no duplica).
        (await api.PostAsJsonAsync("/inventario/reaprovisionamiento", new { ProductoId = cola, Minimo = 5m, Maximo = 10m })).EnsureSuccessStatusCode();
        (await OkAsync<List<object>>(api.GetAsync("/inventario/reaprovisionamiento"))).Should().HaveCount(2);

        // Orden de fabricación de 5 mesas: 10 tableros y 150 tornillos.
        (await api.PostAsJsonAsync("/produccion/ordenes", new { ProductoId = mesa, Cantidad = 5m, AlmacenId = almacen })).EnsureSuccessStatusCode();

        // Un pedido en borrador de 2 tableros ya cuenta como pendiente de recibir.
        (await api.PostAsJsonAsync("/compras/pedidos", new { ProveedorId = maderas, Lineas = new[] { new { ProductoId = tablero, Descripcion = "Tablero", Cantidad = 2m, PrecioUnitario = 12m } } }))
            .EnsureSuccessStatusCode();

        var p = await OkAsync<PropuestaResp>(api.GetAsync("/compras/propuesta"));
        var lTablero = p.Lineas.Single(l => l.ProductoId == tablero);
        // Disponible = 8 + 2 − 10 = 0 → hasta 20, en múltiplos de 5 → 20.
        lTablero.Should().Match<LineaResp>(l => l.Stock == 8m && l.PendienteRecibir == 2m && l.NecesidadFabricacion == 10m && l.Disponible == 0m && l.CantidadBase == 20m
            && l.CantidadCompra == 20m && l.Proveedor == "Maderas del Norte" && l.Importe == 240m);
        // Tornillo sin regla: faltan 150 − 150 = 0 → no aparece... salvo que falten. Cola: 3 < 5 → hasta 10 = 7, sin proveedor.
        p.Lineas.Should().NotContain(l => l.ProductoId == tornillo);
        p.Lineas.Single(l => l.ProductoId == cola).Should().Match<LineaResp>(l => l.Motivo == "Bajo mínimo" && l.CantidadBase == 7m && l.ProveedorId == null);
        p.SinProveedor.Should().Be(1);

        // Otra orden de 2 mesas: faltan 60 tornillos → 1 caja de 100.
        (await api.PostAsJsonAsync("/produccion/ordenes", new { ProductoId = mesa, Cantidad = 2m, AlmacenId = almacen })).EnsureSuccessStatusCode();
        p = await OkAsync<PropuestaResp>(api.GetAsync("/compras/propuesta"));
        p.Lineas.Single(l => l.ProductoId == tornillo).Should().Match<LineaResp>(l => l.Motivo == "Fabricación" && l.NecesidadFabricacion == 210m && l.CantidadBase == 60m
            && l.CantidadCompra == 1m && l.UnidadCompra == "caja" && l.PrecioCompra == 2m && l.Proveedor == "Tornillería Express");
        p.Lineas.Single(l => l.ProductoId == tablero).CantidadBase.Should().Be(25m, "8 + 2 − 14 = −4 → hasta 20 son 24, en múltiplos de 5 → 25");

        // Pedidos: uno por proveedor; la cola sin proveedor se omite.
        var generados = await OkAsync<GeneradosResp>(api.PostAsJsonAsync("/compras/propuesta/pedidos", new
        {
            Lineas = p.Lineas.Select(l => new { l.ProductoId, l.ProveedorId, Cantidad = l.CantidadCompra }).ToArray(),
        }));
        generados.Pedidos.Should().HaveCount(2).And.OnlyContain(x => x.Estado == "Borrador");
        generados.Pedidos.Single(x => x.ProveedorId == tornillos).Lineas.Single().Should().Be(new LineaPedidoResp(tornillo, 1m, 2m));
        generados.Omitidas.Should().ContainSingle().Which.Should().Contain("Cola blanca");

        // Con los pedidos ya hechos, la propuesta solo deja la cola.
        (await OkAsync<PropuestaResp>(api.GetAsync("/compras/propuesta"))).Lineas.Select(l => l.ProductoId).Should().Equal(cola);
        (await FalloAsync(api.PostAsJsonAsync("/compras/propuesta/pedidos", new { Lineas = Array.Empty<object>() }), HttpStatusCode.BadRequest)).Should().Be("propuesta.sin_lineas");
    }
}
