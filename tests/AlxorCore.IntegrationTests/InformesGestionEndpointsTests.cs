using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de los informes de gestión (ventas, compras, cartera, rotación, comparativa).</summary>
public sealed class InformesGestionEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InformesGestionEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record VentaCliente(Guid? ClienteId, string Cliente, int NumFacturas, decimal Base, decimal Total);
    private sealed record VentasCliente(decimal Total, List<VentaCliente> Clientes);
    private sealed record VentaArticulo(Guid? ProductoId, string Descripcion, decimal Unidades, decimal Ingresos, decimal Margen);
    private sealed record VentasArticulo(decimal Ingresos, List<VentaArticulo> Articulos);
    private sealed record CompraProv(Guid? ProveedorId, string Proveedor, decimal Total);
    private sealed record ComprasProv(decimal Total, List<CompraProv> Proveedores);
    private sealed record MesComp(int Mes, decimal Ventas, decimal Gastos, decimal Resultado);
    private sealed record Comparativa(int Ejercicio, decimal Ventas, decimal Gastos, decimal Resultado, List<MesComp> Meses);
    private sealed record Tramo(string TramoNombre, int NumDocumentos, decimal Importe);
    private sealed record Aging(decimal TotalPendiente, List<TramoRaw> Tramos);
    private sealed record TramoRaw(string Tramo, int NumDocumentos, decimal Importe);
    private sealed record LineaExtracto(string Documento, decimal Total, decimal Liquidado, decimal Pendiente);
    private sealed record Extracto(string Nombre, decimal TotalDocumentos, decimal Pendiente, List<LineaExtracto> Lineas);
    private sealed record RotArt(string Nombre, decimal UnidadesVendidas, decimal StockActual);
    private sealed record Rotacion(List<RotArt> Articulos);

    private static async Task<Guid> ClienteAsync(HttpClient c, string nombre) =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = nombre, NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    private static Task FacturaAsync(HttpClient c, Guid clienteId, string fecha, decimal precio, Guid? productoId = null, string desc = "Servicio") =>
        c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = fecha,
            Lineas = new[] { new { ProductoId = productoId, Cantidad = 2m, Descripcion = desc, PrecioUnitario = precio, CodigoIva = "IVA21" } },
        });

    [Fact]
    public async Task Ventas_por_cliente_ordena_por_total()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var a = await ClienteAsync(cli, "Cliente Grande SL");
        var b = await ClienteAsync(cli, "Cliente Pequeño SL");
        await FacturaAsync(cli, a, "2026-02-01", 500m); // 2×500 = 1000 base
        await FacturaAsync(cli, b, "2026-02-05", 100m); // 2×100 = 200 base

        var r = await cli.GetFromJsonAsync<VentasCliente>("/informes/ventas-cliente?desde=2026-01-01&hasta=2026-12-31");
        r!.Clientes.Should().HaveCount(2);
        r.Clientes[0].Cliente.Should().Be("Cliente Grande SL"); // el mayor primero
        r.Clientes[0].Base.Should().Be(1000m);
    }

    [Fact]
    public async Task Ventas_por_articulo_agrega_unidades_e_ingresos()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prod = (await (await cli.PostAsJsonAsync("/productos", new { Nombre = "Widget", PrecioUnitario = 50m, PrecioCompra = 30m, CodigoIva = "IVA21", ControlarStock = true, StockInicial = 100m })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var c = await ClienteAsync(cli, "Cliente SL");
        await FacturaAsync(cli, c, "2026-03-01", 50m, prod, "Widget");
        await FacturaAsync(cli, c, "2026-03-10", 50m, prod, "Widget");

        var r = await cli.GetFromJsonAsync<VentasArticulo>("/informes/ventas-articulo?desde=2026-01-01&hasta=2026-12-31");
        var w = r!.Articulos.Single(a => a.ProductoId == prod);
        w.Unidades.Should().Be(4m);       // 2 facturas × 2 uds
        w.Ingresos.Should().Be(200m);     // 4 × 50
        w.Margen.Should().Be(80m);        // 200 − 4×30
    }

    [Fact]
    public async Task Compras_por_proveedor_agrega_por_proveedor()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prov = (await (await cli.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros SA" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        await cli.PostAsJsonAsync("/gastos", new { ProveedorId = prov, Concepto = "Material", BaseImponible = 300m, CodigoIva = "IVA21", Fecha = "2026-02-01" });
        await cli.PostAsJsonAsync("/gastos", new { ProveedorId = prov, Concepto = "Más material", BaseImponible = 200m, CodigoIva = "IVA21", Fecha = "2026-02-15" });

        var r = await cli.GetFromJsonAsync<ComprasProv>("/informes/compras-proveedor?desde=2026-01-01&hasta=2026-12-31");
        var p = r!.Proveedores.Single(x => x.ProveedorId == prov);
        p.Proveedor.Should().Be("Suministros SA");
        p.Total.Should().Be(605m); // (300+200) × 1,21
    }

    [Fact]
    public async Task Comparativa_mensual_separa_por_mes()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var c = await ClienteAsync(cli, "Cliente SL");
        await FacturaAsync(cli, c, "2026-01-15", 500m); // enero: 1000 base
        await FacturaAsync(cli, c, "2026-03-15", 250m); // marzo: 500 base
        await cli.PostAsJsonAsync("/gastos", new { Concepto = "Gasto enero", BaseImponible = 400m, CodigoIva = "IVA21", Fecha = "2026-01-20" });

        var r = await cli.GetFromJsonAsync<Comparativa>("/informes/comparativa-mensual?anio=2026");
        r!.Meses.Should().HaveCount(12);
        r.Meses[0].Ventas.Should().Be(1000m); // enero
        r.Meses[0].Gastos.Should().Be(400m);
        r.Meses[0].Resultado.Should().Be(600m);
        r.Meses[2].Ventas.Should().Be(500m);  // marzo
        r.Ventas.Should().Be(1500m);
    }

    [Fact]
    public async Task Aging_y_extracto_reflejan_el_pendiente_y_bajan_al_cobrar()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var c = await ClienteAsync(cli, "Deudor SL");
        var facturaId = (await (await cli.PostAsJsonAsync("/facturas", new
        {
            ClienteId = c,
            FechaEmision = "2026-03-01",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        // total = 1210

        var aging1 = await cli.GetFromJsonAsync<Aging>("/informes/aging-cartera");
        aging1!.TotalPendiente.Should().Be(1210m);

        var extracto = await cli.GetFromJsonAsync<Extracto>($"/informes/extracto-tercero?tipo=Cliente&terceroId={c}&desde=2026-01-01&hasta=2026-12-31");
        extracto!.Lineas.Should().ContainSingle();
        extracto.Pendiente.Should().Be(1210m);

        // Cobro parcial de 210 → pendiente 1000.
        await cli.PostAsJsonAsync("/cobros", new { FacturaId = facturaId, Importe = 210m, Fecha = "2026-03-05" });
        var aging2 = await cli.GetFromJsonAsync<Aging>("/informes/aging-cartera");
        aging2!.TotalPendiente.Should().Be(1000m);
    }

    [Fact]
    public async Task Rotacion_cruza_ventas_con_stock()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prod = (await (await cli.PostAsJsonAsync("/productos", new { Nombre = "Rotativo", PrecioUnitario = 10m, PrecioCompra = 6m, CodigoIva = "IVA21", ControlarStock = true, StockInicial = 100m })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var c = await ClienteAsync(cli, "Cliente SL");
        await FacturaAsync(cli, c, "2026-04-01", 10m, prod, "Rotativo"); // vende 2 uds

        var r = await cli.GetFromJsonAsync<Rotacion>("/informes/rotacion-stock?desde=2026-01-01&hasta=2026-12-31");
        var a = r!.Articulos.Single(x => x.Nombre == "Rotativo");
        a.UnidadesVendidas.Should().Be(2m);
        a.StockActual.Should().Be(98m); // 100 − 2 vendidas
    }
}
