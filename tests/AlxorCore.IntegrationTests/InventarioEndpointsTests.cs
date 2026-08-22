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
    private sealed record ValLineaResp(Guid ProductoId, string Nombre, decimal Cantidad, decimal CosteUnitario, decimal Valor);
    private sealed record ValoracionResp(string Metodo, decimal ValorTotal, List<ValLineaResp> Lineas);

    [Fact]
    public async Task Valoracion_de_inventario_segun_metodo_de_la_empresa()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prod = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Materia prima", PrecioUnitario = 10m, PrecioCompra = 2.5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<ProductoIdResp>())!;
        var alm = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "VAL", Nombre = "Val" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        // Dos entradas con distinto coste: 100 @ 2 y 100 @ 4.
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = prod.Id, AlmacenId = alm.Id, Cantidad = 100m, CosteUnitario = 2m });
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = prod.Id, AlmacenId = alm.Id, Cantidad = 100m, CosteUnitario = 4m });

        // PMP: (100×2 + 100×4)/200 = 3 → valor 200×3 = 600.
        (await cliente.PutAsJsonAsync("/empresas/actual/metodo-valoracion", new { MetodoValoracion = "Pmp" })).StatusCode.Should().Be(HttpStatusCode.OK);
        var pmp = await cliente.GetFromJsonAsync<ValoracionResp>("/inventario/valoracion");
        pmp!.Metodo.Should().Be("Pmp");
        var lp = pmp.Lineas.Single(l => l.ProductoId == prod.Id);
        lp.CosteUnitario.Should().Be(3m);
        lp.Valor.Should().Be(600m);

        // Última compra: coste 4 → valor 800.
        await cliente.PutAsJsonAsync("/empresas/actual/metodo-valoracion", new { MetodoValoracion = "UltimaCompra" });
        var uc = await cliente.GetFromJsonAsync<ValoracionResp>("/inventario/valoracion");
        uc!.Lineas.Single(l => l.ProductoId == prod.Id).CosteUnitario.Should().Be(4m);

        // Estándar: coste de ficha 2,5 → valor 500.
        await cliente.PutAsJsonAsync("/empresas/actual/metodo-valoracion", new { MetodoValoracion = "Estandar" });
        var est = await cliente.GetFromJsonAsync<ValoracionResp>("/inventario/valoracion");
        est!.Lineas.Single(l => l.ProductoId == prod.Id).CosteUnitario.Should().Be(2.5m);
    }

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
    public async Task Montaje_consume_componentes_y_produce_el_compuesto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var a = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Pieza A", PrecioUnitario = 1m, PrecioCompra = 0.5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<ProductoIdResp>())!;
        var bb = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Pieza B", PrecioUnitario = 1m, PrecioCompra = 0.5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<ProductoIdResp>())!;
        var conj = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Conjunto", PrecioUnitario = 5m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<ProductoIdResp>())!;
        await cliente.PutAsJsonAsync($"/productos/{conj.Id}/composicion", new { Componentes = new[] { new { ComponenteId = a.Id, Cantidad = 2m }, new { ComponenteId = bb.Id, Cantidad = 1m } } });

        var alm = (await (await cliente.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "TALLER", Nombre = "Taller" })).Content.ReadFromJsonAsync<AlmacenResp>())!;
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = a.Id, AlmacenId = alm.Id, Cantidad = 100m });
        await cliente.PostAsJsonAsync("/inventario/entrada", new { ProductoId = bb.Id, AlmacenId = alm.Id, Cantidad = 100m });

        // Montar 10 conjuntos: consume 20 de A y 10 de B, produce 10 del conjunto.
        var montaje = await cliente.PostAsJsonAsync("/inventario/montaje", new { ProductoId = conj.Id, Cantidad = 10m, AlmacenId = alm.Id });
        montaje.StatusCode.Should().Be(HttpStatusCode.OK);

        var sa = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{a.Id}");
        sa!.Single().Cantidad.Should().Be(80m);
        var sb = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{bb.Id}");
        sb!.Single().Cantidad.Should().Be(90m);
        var sc = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{conj.Id}");
        sc!.Single().Cantidad.Should().Be(10m);

        // Sin stock suficiente de componentes, el montaje falla y no altera existencias.
        var falla = await cliente.PostAsJsonAsync("/inventario/montaje", new { ProductoId = conj.Id, Cantidad = 1000m, AlmacenId = alm.Id });
        falla.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var sa2 = await cliente.GetFromJsonAsync<List<ExistenciaResp>>($"/inventario/stock/producto/{a.Id}");
        sa2!.Single().Cantidad.Should().Be(80m);
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
