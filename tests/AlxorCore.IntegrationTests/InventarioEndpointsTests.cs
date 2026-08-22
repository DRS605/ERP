using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del inventario multi-almacén, ubicaciones y ubicación por defecto.</summary>
public sealed class InventarioEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InventarioEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record AlmacenResp(Guid Id, string Codigo, string Nombre);
    private sealed record UbicacionResp(Guid Id, Guid AlmacenId, string Codigo);
    private sealed record ExistenciaResp(Guid ProductoId, Guid AlmacenId, string AlmacenNombre, Guid? UbicacionId, string? UbicacionCodigo, decimal Cantidad, string? Lote);
    private sealed record MovimientoResp(Guid Id, string Tipo, decimal Cantidad, string? Lote);
    private sealed record TrazaResp(string Lote, List<ExistenciaResp> Existencias, List<MovimientoResp> Movimientos);
    private sealed record ProductoIdResp(Guid Id);
    private sealed record UbiDefResp(Guid Id, Guid ProductoId, Guid AlmacenId, Guid? ProveedorId, Guid UbicacionId, string UbicacionCodigo);

    [Fact]
    public async Task Entradas_salidas_ajuste_y_traspaso_entre_almacenes()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = Guid.NewGuid();

        var alm1 = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "CENTRAL", Nombre = "Almacén central" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        var alm2 = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "TIENDA", Nombre = "Tienda" })).Content.ReadFromJsonAsync<AlmacenResp>())!;

        // Entrada de 100 en central; salida de 30.
        (await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = alm1.Id, Cantidad = 100m, Motivo = "Compra" })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cliente.PostAsJsonAsync("/inventario/salida", new { ProductoId = producto, AlmacenId = alm1.Id, Cantidad = 30m })).StatusCode.Should().Be(HttpStatusCode.OK);

        // Salida por encima del stock → 400.
        (await cliente.PostAsJsonAsync("/inventario/salida", new { ProductoId = producto, AlmacenId = alm1.Id, Cantidad = 1000m })).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Traspaso de 20 de central a tienda.
        (await cliente.PostAsJsonAsync("/inventario/traspaso", new { ProductoId = producto, Cantidad = 20m, AlmacenOrigenId = alm1.Id, AlmacenDestinoId = alm2.Id })).StatusCode.Should().Be(HttpStatusCode.OK);

        var stock = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}");
        stock!.Single(s => s.AlmacenId == alm1.Id).Cantidad.Should().Be(50m); // 100 - 30 - 20
        stock.Single(s => s.AlmacenId == alm2.Id).Cantidad.Should().Be(20m);

        // Ajuste por recuento en central: fija a 45.
        (await cliente.PostAsJsonAsync("/inventario/ajuste", new { ProductoId = producto, AlmacenId = alm1.Id, Cantidad = 45m })).StatusCode.Should().Be(HttpStatusCode.OK);
        var stock2 = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{producto}");
        stock2!.Single(s => s.AlmacenId == alm1.Id).Cantidad.Should().Be(45m);
    }

    [Fact]
    public async Task Trazabilidad_por_lote_en_varios_almacenes()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // Artículo trazado por lote.
        var prod = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Vacuna", PrecioUnitario = 20m, CodigoIva = "IVA21", Seguimiento = "Lote" })).Content.ReadFromJsonAsync<ProductoIdResp>())!;
        var alm1 = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "A1", Nombre = "Nevera 1" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        var alm2 = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "A2", Nombre = "Nevera 2" })).Content.ReadFromJsonAsync<AlmacenResp>())!;

        // Mismo lote L1 en dos almacenes; y un lote L2 en el primero.
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = prod.Id, AlmacenId = alm1.Id, Cantidad = 100m, Lote = "L1" });
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = prod.Id, AlmacenId = alm2.Id, Cantidad = 40m, Lote = "L1" });
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = prod.Id, AlmacenId = alm1.Id, Cantidad = 10m, Lote = "L2" });

        // El stock del artículo distingue por lote (3 filas).
        var stock = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{prod.Id}");
        stock!.Should().HaveCount(3);
        stock.Where(s => s.Lote == "L1").Sum(s => s.Cantidad).Should().Be(140m);

        // Trazabilidad del lote L1: está en dos almacenes y tiene dos movimientos.
        var traza = await cliente.GetFromJsonAsync<TrazaResp>($"/inventario/trazabilidad/{prod.Id}?lote=L1");
        traza!.Lote.Should().Be("L1");
        traza.Existencias.Should().HaveCount(2);
        traza.Movimientos.Should().HaveCount(2);
        traza.Movimientos.Should().OnlyContain(m => m.Lote == "L1");
    }

    [Fact]
    public async Task Ubicacion_por_defecto_por_almacen_y_por_proveedor()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = Guid.NewGuid();
        var proveedor = Guid.NewGuid();

        var alm = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "C", Nombre = "Central" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        var ubiGeneral = (await (await cliente.PostAsJsonAsync("/inventario/ubicaciones", new { AlmacenId = alm.Id, Codigo = "A-1" })).Content.ReadFromJsonAsync<UbicacionResp>())!;
        var ubiProv = (await (await cliente.PostAsJsonAsync("/inventario/ubicaciones", new { AlmacenId = alm.Id, Codigo = "B-9" })).Content.ReadFromJsonAsync<UbicacionResp>())!;

        // Regla general por almacén.
        (await cliente.PostAsJsonAsync("/inventario/ubicacion-defecto", new { ProductoId = producto, AlmacenId = alm.Id, UbicacionId = ubiGeneral.Id })).StatusCode.Should().Be(HttpStatusCode.OK);
        // Regla específica por proveedor+almacén.
        (await cliente.PostAsJsonAsync("/inventario/ubicacion-defecto", new { ProductoId = producto, AlmacenId = alm.Id, ProveedorId = proveedor, UbicacionId = ubiProv.Id })).StatusCode.Should().Be(HttpStatusCode.OK);

        var reglas = await cliente.GetFromJsonAsync<List<UbiDefResp>>($"/inventario/ubicacion-defecto/producto/{producto}");
        reglas!.Should().HaveCount(2);
        reglas.Should().Contain(r => r.ProveedorId == null && r.UbicacionId == ubiGeneral.Id);
        reglas.Should().Contain(r => r.ProveedorId == proveedor && r.UbicacionId == ubiProv.Id);
    }
}
