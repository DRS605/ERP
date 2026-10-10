using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Pruebas del tubo de importación de compras: factura de proveedor por EAN (crea artículos nuevos,
/// da entrada de stock y registra el gasto) e importación de precios de venta por EAN.
/// </summary>
public sealed class ImportacionComprasEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ImportacionComprasEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ResultadoResp(int Total, int Correctas, int Errores, bool Aplicado, List<string> Mensajes);
    private sealed record ProductoResp(Guid Id, string? Referencia, string Nombre, decimal Stock, decimal PrecioUnitario, decimal PrecioCompra, string CodigoIva, decimal PorcentajeIva, bool ControlarStock);
    private sealed record GastoResp(Guid Id, string Concepto, decimal BaseImponible, string CodigoIva);

    /// <summary>Genera un .xlsx mínimo (una hoja, celdas inline) desde una rejilla de textos.</summary>
    private static string XlsxBase64(string[][] filas)
    {
        string Ref(int col, int fila) => $"{(char)('A' + col)}{fila}";
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        for (var i = 0; i < filas.Length; i++)
        {
            sb.Append($"<row r=\"{i + 1}\">");
            for (var j = 0; j < filas[i].Length; j++)
            {
                var v = System.Security.SecurityElement.Escape(filas[i][j]);
                sb.Append($"<c r=\"{Ref(j, i + 1)}\" t=\"inlineStr\"><is><t>{v}</t></is></c>");
            }

            sb.Append("</row>");
        }

        sb.Append("</sheetData></worksheet>");

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            var entry = zip.CreateEntry("xl/worksheets/sheet1.xml");
            using var w = new StreamWriter(entry.Open(), new UTF8Encoding(false));
            w.Write(sb.ToString());
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private static string CsvBase64(string contenido) => Convert.ToBase64String(Encoding.UTF8.GetBytes(contenido));

    [Fact]
    public async Task Factura_proveedor_crea_articulos_nuevos_con_stock_y_gasto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var xlsx = XlsxBase64(new[]
        {
            new[] { "ean", "descripcion", "cantidad", "coste", "iva" },
            new[] { "8410000000017", "Leche entera 1L", "12", "0.80", "4" },
            new[] { "8410000000024", "Galletas 500g", "6", "1.50", "10" },
        });

        var prev = await (await cliente.PostAsJsonAsync("/importar/factura-proveedor", new { ContenidoBase64 = xlsx, Previsualizar = true, ProveedorTexto = "Distribuidora SL" })).Content.ReadFromJsonAsync<ResultadoResp>();
        prev!.Aplicado.Should().BeFalse();
        prev.Correctas.Should().Be(2);

        var apl = await cliente.PostAsJsonAsync("/importar/factura-proveedor", new { ContenidoBase64 = xlsx, Previsualizar = false, ProveedorTexto = "Distribuidora SL" });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Aplicado.Should().BeTrue();

        var productos = await cliente.GetFromJsonAsync<List<ProductoResp>>("/productos");
        var leche = productos!.Single(p => p.Referencia == "8410000000017");
        leche.Nombre.Should().Be("Leche entera 1L");
        leche.CodigoIva.Should().Be("IVA4");
        leche.Stock.Should().Be(12m);
        var galletas = productos.Single(p => p.Referencia == "8410000000024");
        galletas.CodigoIva.Should().Be("IVA10");
        galletas.Stock.Should().Be(6m);

        var gastos = await cliente.GetFromJsonAsync<List<GastoResp>>("/gastos");
        // Dos tipos de IVA ⇒ dos gastos; base = Σ(cantidad·coste) por tipo.
        gastos!.Should().Contain(g => g.CodigoIva == "IVA4" && g.BaseImponible == 9.60m);
        gastos.Should().Contain(g => g.CodigoIva == "IVA10" && g.BaseImponible == 9.00m);
    }

    [Fact]
    public async Task Factura_proveedor_modo_solo_articulos_no_toca_stock_ni_gasto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var csv = CsvBase64("ean;descripcion;cantidad;coste;iva\n8490000000019;Producto A;10;0.80;21\n8490000000026;Producto B;5;1.20;10\n");

        var apl = await cliente.PostAsJsonAsync("/importar/factura-proveedor", new { ContenidoBase64 = csv, Previsualizar = false, SoloArticulos = true });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Aplicado.Should().BeTrue();

        var productos = await cliente.GetFromJsonAsync<List<ProductoResp>>("/productos");
        var a = productos!.Single(p => p.Referencia == "8490000000019");
        a.PrecioCompra.Should().Be(0.80m);
        a.Stock.Should().Be(0m); // no se suma stock en modo solo artículos
        productos.Single(p => p.Referencia == "8490000000026").Stock.Should().Be(0m);

        // No se registra ningún gasto.
        var gastos = await cliente.GetFromJsonAsync<List<GastoResp>>("/gastos");
        gastos!.Should().BeEmpty();
    }

    [Fact]
    public async Task Factura_proveedor_solo_articulos_crea_con_precio_de_venta_pvp()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        // pvp con IVA: 21% -> base 1.00; 10% -> base 2.00
        var csv = CsvBase64("ean;descripcion;coste;iva;pvp\n8495000000013;Articulo PVP 21;0.40;21;1.21\n8495000000020;Articulo PVP 10;0.90;10;2.20\n");

        var apl = await cliente.PostAsJsonAsync("/importar/factura-proveedor", new { ContenidoBase64 = csv, Previsualizar = false, SoloArticulos = true, PreciosConIva = true });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);

        var productos = await cliente.GetFromJsonAsync<List<ProductoResp>>("/productos");
        var a = productos!.Single(p => p.Referencia == "8495000000013");
        a.PrecioCompra.Should().Be(0.40m);
        a.PrecioUnitario.Should().Be(1.00m); // 1.21 / 1.21
        a.Stock.Should().Be(0m);
        productos.Single(p => p.Referencia == "8495000000020").PrecioUnitario.Should().Be(2.00m); // 2.20 / 1.10
    }

    [Fact]
    public async Task Factura_proveedor_solo_suma_stock_a_los_ya_existentes()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var creado = await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Agua 1.5L", Referencia = "8400000000011", PrecioUnitario = 0.50m, CodigoIva = "IVA10", Tipo = "Bien", ControlarStock = true })).Content.ReadFromJsonAsync<ProductoResp>();

        var csv = CsvBase64("ean;cantidad;coste\n8400000000011;24;0.30\n");
        var apl = await cliente.PostAsJsonAsync("/importar/factura-proveedor", new { ContenidoBase64 = csv, Previsualizar = false, ProveedorTexto = "Aguas SA" });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Aplicado.Should().BeTrue();

        var actualizado = await cliente.GetFromJsonAsync<ProductoResp>($"/productos/{creado!.Id}");
        actualizado!.Stock.Should().Be(24m);
        // No se duplica el artículo.
        var productos = await cliente.GetFromJsonAsync<List<ProductoResp>>("/productos");
        productos!.Count(p => p.Referencia == "8400000000011").Should().Be(1);
    }

    [Fact]
    public async Task Precios_venta_con_iva_fija_la_base_por_ean()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var creado = await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Refresco", Referencia = "8411111111118", PrecioUnitario = 1m, CodigoIva = "IVA21", Tipo = "Bien", ControlarStock = true })).Content.ReadFromJsonAsync<ProductoResp>();

        var csv = CsvBase64("ean;precio\n8411111111118;1.21\n");
        var apl = await cliente.PostAsJsonAsync("/importar/precios-venta", new { ContenidoBase64 = csv, Previsualizar = false, PreciosConIva = true });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Correctas.Should().Be(1);

        var actualizado = await cliente.GetFromJsonAsync<ProductoResp>($"/productos/{creado!.Id}");
        actualizado!.PrecioUnitario.Should().Be(1m); // 1.21 / 1.21 = 1.00 base
    }

    [Fact]
    public async Task Precios_venta_informa_de_ean_no_encontrado()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var csv = CsvBase64("ean;precio\n0000000000000;9.99\n");
        var res = await (await cliente.PostAsJsonAsync("/importar/precios-venta", new { ContenidoBase64 = csv, Previsualizar = true, PreciosConIva = true })).Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Correctas.Should().Be(0);
        res.Errores.Should().Be(1);
        res.Mensajes.Should().Contain(m => m.Contains("no encontrado", StringComparison.OrdinalIgnoreCase));
    }
}
