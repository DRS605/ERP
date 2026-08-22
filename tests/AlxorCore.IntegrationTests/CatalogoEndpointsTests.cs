using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Catálogo (productos e impuestos).</summary>
public sealed class CatalogoEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CatalogoEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ProductoDto(Guid Id, string Nombre, decimal PrecioUnitario, string CodigoIva, decimal PorcentajeIva, bool Activo, decimal PrecioCompra, Guid? ProveedorHabitualId);
    private sealed record ProveedorResp(Guid Id);

    private sealed record ImpuestoDto(string Codigo, string Nombre, string Tipo, decimal Porcentaje);

    private sealed record HistoricoPrecioResp(DateTimeOffset RegistradoEn, decimal PrecioVenta, decimal PrecioCompra);

    [Fact]
    public async Task Listar_impuestos_devuelve_los_tipos_de_iva()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var impuestos = await cliente.GetFromJsonAsync<List<ImpuestoDto>>("/impuestos");

        impuestos.Should().Contain(i => i.Codigo == "IVA21" && i.Porcentaje == 21m);
        impuestos.Should().Contain(i => i.Codigo == "IVA4");
    }

    [Fact]
    public async Task Crear_listar_y_obtener_producto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var crear = await cliente.PostAsJsonAsync("/productos", new { Nombre = "Consultoría", PrecioUnitario = 90m, Tipo = "Servicio", CodigoIva = "IVA21" });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var creado = await crear.Content.ReadFromJsonAsync<ProductoDto>();
        creado!.PorcentajeIva.Should().Be(21m);

        var lista = await cliente.GetFromJsonAsync<List<ProductoDto>>("/productos");
        lista.Should().ContainSingle(p => p.Id == creado.Id);

        var obtenido = await cliente.GetFromJsonAsync<ProductoDto>($"/productos/{creado.Id}");
        obtenido!.Nombre.Should().Be("Consultoría");
    }

    private sealed record ProductoEnvaseDto(Guid Id, string Unidad, string? UnidadCompra, decimal FactorCompra,
        string? UnidadVenta, decimal FactorVenta, decimal PrecioCompra, decimal PrecioCompraPorUnidadCompra, decimal PrecioVentaPorUnidadVenta, decimal PrecioUnitario);

    [Fact]
    public async Task Articulo_con_unidades_de_compra_y_venta()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var crear = await cliente.PostAsJsonAsync("/productos", new
        {
            Nombre = "Refresco", PrecioUnitario = 0.90m, PrecioCompra = 0.50m, CodigoIva = "IVA21",
            Unidad = "ud", UnidadCompra = "caja", FactorCompra = 12m, UnidadVenta = "lata", FactorVenta = 1m,
        });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var creado = (await crear.Content.ReadFromJsonAsync<ProductoEnvaseDto>())!;

        var p = await cliente.GetFromJsonAsync<ProductoEnvaseDto>($"/productos/{creado.Id}");
        p!.Unidad.Should().Be("ud");
        p.UnidadCompra.Should().Be("caja");
        p.FactorCompra.Should().Be(12m);
        p.PrecioCompraPorUnidadCompra.Should().Be(6m); // 0,50 × 12

        // Un factor no positivo se rechaza.
        var malo = await cliente.PostAsJsonAsync("/productos", new { Nombre = "X", PrecioUnitario = 1m, CodigoIva = "IVA21", FactorCompra = 0m });
        malo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record IdResp(Guid Id);
    private sealed record ComponenteResp(Guid ComponenteId, string Nombre, decimal Cantidad, decimal CosteUnitario, decimal CosteLinea);
    private sealed record ComposicionResp(Guid ProductoId, bool EsCompuesto, decimal CosteTotal, List<ComponenteResp> Componentes);

    [Fact]
    public async Task Articulo_compuesto_con_lista_de_materiales_y_escandallo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var tuerca = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Tuerca", PrecioUnitario = 0.5m, PrecioCompra = 0.20m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        var tornillo = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Tornillo", PrecioUnitario = 0.6m, PrecioCompra = 0.30m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;
        var conjunto = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Conjunto fijación", PrecioUnitario = 3m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;

        // 2 tuercas + 4 tornillos por conjunto.
        var def = await cliente.PutAsJsonAsync($"/productos/{conjunto.Id}/composicion", new { Componentes = new[] { new { ComponenteId = tuerca.Id, Cantidad = 2m }, new { ComponenteId = tornillo.Id, Cantidad = 4m } } });
        def.StatusCode.Should().Be(HttpStatusCode.OK);

        var comp = await cliente.GetFromJsonAsync<ComposicionResp>($"/productos/{conjunto.Id}/composicion");
        comp!.EsCompuesto.Should().BeTrue();
        comp.Componentes.Should().HaveCount(2);
        comp.CosteTotal.Should().Be(1.60m); // 2×0,20 + 4×0,30

        // Un componente inexistente se rechaza.
        var malo = await cliente.PutAsJsonAsync($"/productos/{conjunto.Id}/composicion", new { Componentes = new[] { new { ComponenteId = Guid.NewGuid(), Cantidad = 1m } } });
        malo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record VarianteResp(Guid Id, string Nombre, string? Referencia, decimal PrecioUnitario, Guid? ProductoPadreId, bool EsPlantilla, string Variante);

    [Fact]
    public async Task Variantes_de_un_articulo_plantilla()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var padre = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Camiseta", PrecioUnitario = 15m, PrecioCompra = 6m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!;

        var v1 = await cliente.PostAsJsonAsync($"/productos/{padre.Id}/variantes", new { Referencia = "CAM-M-ROJO", Atributos = new[] { new { Nombre = "Talla", Valor = "M" }, new { Nombre = "Color", Valor = "Rojo" } } });
        v1.StatusCode.Should().Be(HttpStatusCode.Created);
        var variante = (await v1.Content.ReadFromJsonAsync<VarianteResp>())!;
        variante.Nombre.Should().Be("Camiseta M / Rojo");
        variante.ProductoPadreId.Should().Be(padre.Id);
        variante.Variante.Should().Be("M · Rojo");
        variante.PrecioUnitario.Should().Be(15m); // heredado del padre

        // Otra variante con precio propio.
        await cliente.PostAsJsonAsync($"/productos/{padre.Id}/variantes", new { PrecioUnitario = 18m, Atributos = new[] { new { Nombre = "Talla", Valor = "L" }, new { Nombre = "Color", Valor = "Azul" } } });

        var variantes = await cliente.GetFromJsonAsync<List<VarianteResp>>($"/productos/{padre.Id}/variantes");
        variantes.Should().HaveCount(2);

        // El padre queda como plantilla.
        var padreDto = await cliente.GetFromJsonAsync<VarianteResp>($"/productos/{padre.Id}");
        padreDto!.EsPlantilla.Should().BeTrue();

        // Sin atributos, se rechaza.
        var malo = await cliente.PostAsJsonAsync($"/productos/{padre.Id}/variantes", new { Atributos = Array.Empty<object>() });
        malo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Un_articulo_puede_tener_proveedor_habitual()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prov = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Mayorista SL" })).Content.ReadFromJsonAsync<ProveedorResp>())!;

        var creado = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Artículo", PrecioUnitario = 10m, CodigoIva = "IVA21", ProveedorHabitualId = prov.Id }))
            .Content.ReadFromJsonAsync<ProductoDto>())!;
        creado.ProveedorHabitualId.Should().Be(prov.Id);

        var obtenido = await cliente.GetFromJsonAsync<ProductoDto>($"/productos/{creado.Id}");
        obtenido!.ProveedorHabitualId.Should().Be(prov.Id);
    }

    [Fact]
    public async Task El_historico_de_precios_registra_alta_y_cambios()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var creado = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Artículo", PrecioUnitario = 100m, PrecioCompra = 60m, CodigoIva = "IVA21" }))
            .Content.ReadFromJsonAsync<ProductoDto>())!;
        creado.PrecioCompra.Should().Be(60m);

        // Cambio de precios -> nueva fila de histórico.
        await cliente.PutAsJsonAsync($"/productos/{creado.Id}", new { Nombre = "Artículo", PrecioUnitario = 120m, PrecioCompra = 70m, CodigoIva = "IVA21" });

        var historico = await cliente.GetFromJsonAsync<List<HistoricoPrecioResp>>($"/productos/{creado.Id}/precios");
        historico.Should().HaveCount(2);
        historico![0].PrecioVenta.Should().Be(120m); // más reciente primero
        historico[0].PrecioCompra.Should().Be(70m);
        historico[1].PrecioVenta.Should().Be(100m);
    }

    [Fact]
    public async Task Crear_producto_con_iva_invalido_devuelve_400()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var crear = await cliente.PostAsJsonAsync("/productos", new { Nombre = "X", PrecioUnitario = 10m, CodigoIva = "IVA99" });
        crear.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Los_productos_estan_aislados_por_empresa()
    {
        var (empresaA, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await empresaA.PostAsJsonAsync("/productos", new { Nombre = "Producto de A", PrecioUnitario = 5m });

        var (empresaB, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var listaB = await empresaB.GetFromJsonAsync<List<ProductoDto>>("/productos");

        listaB.Should().BeEmpty();
    }

    private sealed record ProductoStockDto(Guid Id, string Nombre, bool ControlarStock, decimal Stock);
    private sealed record MovimientoResp(string Tipo, decimal Cantidad, decimal StockResultante, string? Motivo);
    private sealed record ClienteResp(Guid Id);
    private sealed record FacturaResp(Guid Id);

    [Fact]
    public async Task Entrada_de_stock_incrementa_las_existencias()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var creado = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Café 1kg", PrecioUnitario = 12m, Tipo = "Bien", ControlarStock = true, StockInicial = 10m })).Content.ReadFromJsonAsync<ProductoStockDto>())!;
        creado.ControlarStock.Should().BeTrue();
        creado.Stock.Should().Be(10m);

        var tras = await (await cliente.PostAsJsonAsync($"/productos/{creado.Id}/stock", new { Tipo = "Entrada", Cantidad = 25m, Motivo = "Compra" })).Content.ReadFromJsonAsync<ProductoStockDto>();
        tras!.Stock.Should().Be(35m);

        var movs = await cliente.GetFromJsonAsync<List<MovimientoResp>>($"/productos/{creado.Id}/stock");
        movs.Should().ContainSingle(m => m.Tipo == "Entrada" && m.Cantidad == 25m && m.StockResultante == 35m);
    }

    [Fact]
    public async Task Emitir_factura_descuenta_el_stock_de_los_articulos_controlados()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Filtro A/A", PrecioUnitario = 30m, Tipo = "Bien", ControlarStock = true, StockInicial = 8m })).Content.ReadFromJsonAsync<ProductoStockDto>())!;
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<ClienteResp>())!.Id;

        var comando = new { ClienteId = clienteId, Lineas = new[] { new { Cantidad = 3m, Descripcion = "Filtro", PrecioUnitario = 30m, CodigoIva = "IVA21", ProductoId = producto.Id } } };
        (await cliente.PostAsJsonAsync("/facturas", comando)).StatusCode.Should().Be(HttpStatusCode.Created);

        var tras = await cliente.GetFromJsonAsync<ProductoStockDto>($"/productos/{producto.Id}");
        tras!.Stock.Should().Be(5m); // 8 - 3

        var movs = await cliente.GetFromJsonAsync<List<MovimientoResp>>($"/productos/{producto.Id}/stock");
        movs.Should().ContainSingle(m => m.Tipo == "Venta" && m.Cantidad == -3m);
    }

    [Fact]
    public async Task Un_articulo_sin_control_de_stock_no_admite_movimientos()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var creado = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Servicio", PrecioUnitario = 50m })).Content.ReadFromJsonAsync<ProductoStockDto>())!;

        var r = await cliente.PostAsJsonAsync($"/productos/{creado.Id}/stock", new { Tipo = "Entrada", Cantidad = 5m });
        r.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
