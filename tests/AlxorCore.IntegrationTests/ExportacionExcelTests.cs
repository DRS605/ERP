using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Xml.Linq;
using AlxorCore.Api.Comun;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del escritor de Excel (<see cref="ExcelXlsx"/>): se abre el .xlsx generado y se revisa su XML.</summary>
public sealed class ExcelXlsxTests
{
    private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    internal static Dictionary<string, string> Partes(byte[] xlsx)
    {
        using var zip = new ZipArchive(new MemoryStream(xlsx), ZipArchiveMode.Read);
        return zip.Entries.ToDictionary(e => e.FullName, e =>
        {
            using var r = new StreamReader(e.Open());
            return r.ReadToEnd();
        });
    }

    private static XElement Celda(XDocument hoja, string referencia)
        => hoja.Descendants(Ns + "c").Single(c => (string?)c.Attribute("r") == referencia);

    [Fact]
    public void Genera_un_libro_valido_con_cabecera_formatos_y_totales()
    {
        ColumnaExcel[] columnas =
        [
            new("Número"), new("Fecha", TipoColumnaExcel.Fecha), new("Cliente"),
            new("Base", TipoColumnaExcel.Moneda), new("IVA %", TipoColumnaExcel.Porcentaje, TotalColumnaExcel.Media), new("Kilos", TipoColumnaExcel.Numero),
        ];
        object?[][] filas =
        [
            ["F-2026/0001", new DateOnly(2026, 1, 31), "Frutas <Sol> & Co", 1234.56m, 21m, 100],
            ["F-2026/0002", "15/02/2026", "=HIPERVINCULO(\"x\")", "1.000,44 €", "10 %", "250"],
        ];

        var xlsx = ExcelXlsx.Generar("Facturas emitidas: 1T/2026 [ventas] con un título largo", columnas, filas, filaTotales: true);
        var partes = Partes(xlsx);

        partes.Keys.Should().Contain(["[Content_Types].xml", "_rels/.rels", "xl/workbook.xml", "xl/_rels/workbook.xml.rels", "xl/styles.xml", "xl/worksheets/sheet1.xml"]);
        var libro = XDocument.Parse(partes["xl/workbook.xml"]);
        var nombre = (string)libro.Descendants(Ns + "sheet").Single().Attribute("name")!;
        nombre.Should().HaveLength(31).And.NotContainAny(":", "/", "[", "]");
        nombre.Should().StartWith("Facturas emitidas 1T 2026 venta");

        var hoja = XDocument.Parse(partes["xl/worksheets/sheet1.xml"]);
        hoja.Descendants(Ns + "pane").Single().Attribute("state")!.Value.Should().Be("frozen");
        hoja.Descendants(Ns + "autoFilter").Single().Attribute("ref")!.Value.Should().Be("A1:F3");
        hoja.Descendants(Ns + "col").Should().HaveCount(6);

        // Cabecera en negrita (estilo 1) como texto en línea.
        Celda(hoja, "A1").Attribute("s")!.Value.Should().Be("1");
        Celda(hoja, "A1").Value.Should().Be("Número");

        // Fecha como número de serie con formato de fecha; texto con caracteres especiales escapado y nunca como fórmula.
        Celda(hoja, "B2").Element(Ns + "v")!.Value.Should().Be("46053");
        Celda(hoja, "B3").Element(Ns + "v")!.Value.Should().Be("46068");
        Celda(hoja, "C2").Value.Should().Be("Frutas <Sol> & Co");
        Celda(hoja, "C3").Element(Ns + "f").Should().BeNull();
        Celda(hoja, "C3").Attribute("t")!.Value.Should().Be("inlineStr");

        // Importes y porcentajes: los textos con formato español se convierten a número.
        Celda(hoja, "D3").Element(Ns + "v")!.Value.Should().Be("1000.44");
        Celda(hoja, "E2").Element(Ns + "v")!.Value.Should().Be("0.21");
        Celda(hoja, "F3").Element(Ns + "v")!.Value.Should().Be("250");

        // Fila de totales: SUM en importes y números, AVERAGE donde se pide, «Total» en la primera.
        Celda(hoja, "A4").Value.Should().Be("Total");
        Celda(hoja, "D4").Element(Ns + "f")!.Value.Should().Be("SUM(D2:D3)");
        Celda(hoja, "D4").Element(Ns + "v")!.Value.Should().Be("2235.00");
        Celda(hoja, "E4").Element(Ns + "f")!.Value.Should().Be("AVERAGE(E2:E3)");
        Celda(hoja, "F4").Element(Ns + "f")!.Value.Should().Be("SUM(F2:F3)");
        Celda(hoja, "B4").Element(Ns + "f").Should().BeNull();

        // Formatos: moneda en euros, porcentaje y fecha.
        var estilos = partes["xl/styles.xml"];
        estilos.Should().Contain("formatCode=\"#,##0.00\\ &quot;€&quot;\"").And.Contain("formatCode=\"dd/mm/yyyy\"");
        var xfs = XDocument.Parse(estilos).Descendants(Ns + "cellXfs").Single().Elements(Ns + "xf").ToList();
        xfs[int.Parse(Celda(hoja, "D2").Attribute("s")!.Value, System.Globalization.CultureInfo.InvariantCulture)].Attribute("numFmtId")!.Value.Should().Be("164");
        xfs[int.Parse(Celda(hoja, "E2").Attribute("s")!.Value, System.Globalization.CultureInfo.InvariantCulture)].Attribute("numFmtId")!.Value.Should().Be("10");
        xfs[int.Parse(Celda(hoja, "B2").Attribute("s")!.Value, System.Globalization.CultureInfo.InvariantCulture)].Attribute("numFmtId")!.Value.Should().Be("165");
    }

    [Fact]
    public void Sin_totales_no_hay_fila_de_total_y_las_filas_cortas_se_completan()
    {
        var xlsx = ExcelXlsx.Generar("Clientes", [new("Nombre"), new("Saldo", TipoColumnaExcel.Moneda)], [["Ana"], ["Luis", 5m]], filaTotales: false);
        var hoja = XDocument.Parse(Partes(xlsx)["xl/worksheets/sheet1.xml"]);
        hoja.Descendants(Ns + "row").Should().HaveCount(3);
        hoja.Descendants(Ns + "f").Should().BeEmpty();
    }

    [Fact]
    public void Totales_con_etiqueta_propia()
    {
        var xlsx = ExcelXlsx.Generar("Cobros", [new("Cliente"), new("Importe", TipoColumnaExcel.Moneda)], [["Ana", 10m], ["Luis", 5m]], ["Total · 2", null]);
        var hoja = XDocument.Parse(Partes(xlsx)["xl/worksheets/sheet1.xml"]);
        Celda(hoja, "A4").Value.Should().Be("Total · 2");
        Celda(hoja, "B4").Element(Ns + "f")!.Value.Should().Be("SUM(B2:B3)");
    }

    [Fact]
    public void Valida_columnas_y_limite_de_filas()
    {
        var sinColumnas = () => ExcelXlsx.Generar("x", [], [], filaTotales: false);
        sinColumnas.Should().Throw<ArgumentException>();

        var demasiadas = () => ExcelXlsx.Generar("x", [new("A")], Enumerable.Range(0, ExcelXlsx.MaximoFilas + 1).Select(i => (IReadOnlyList<object?>)[i]), filaTotales: false);
        demasiadas.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("Libro [IVA] 2026/1T", "Libro IVA 2026 1T")]
    [InlineData("'Mayor'", "Mayor")]
    [InlineData("", "Hoja1")]
    [InlineData("  ::  ", "Hoja1")]
    public void Nombre_de_hoja_saneado(string titulo, string esperado) => ExcelXlsx.NombreHoja(titulo).Should().Be(esperado);

    [Theory]
    [InlineData(0, "A")]
    [InlineData(25, "Z")]
    [InlineData(26, "AA")]
    [InlineData(701, "ZZ")]
    [InlineData(702, "AAA")]
    public void Letras_de_columna(int indice, string esperado) => ExcelXlsx.LetraColumna(indice).Should().Be(esperado);
}

/// <summary>Pruebas del endpoint genérico <c>POST /exportar/xlsx</c>.</summary>
public sealed class ExportacionExcelEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ExportacionExcelEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task Devuelve_un_xlsx_con_las_filas_enviadas()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var resp = await c.PostAsJsonAsync("/exportar/xlsx", new
        {
            titulo = "Cobros pendientes",
            columnas = new object[]
            {
                new { titulo = "Cliente", tipo = "texto" }, new { titulo = "Vencimiento", tipo = "fecha" },
                new { titulo = "Importe", tipo = "moneda" }, new { titulo = "Descuento", tipo = "porcentaje", total = "media" },
            },
            filas = new object?[][] { ["Ana", "31/12/2026", 100.5m, 5], ["Luis", "2026-11-30", "1.200,00 €", null] },
            totales = new object?[] { "Total · 2" },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.OK, await resp.Content.ReadAsStringAsync());
        resp.Content.Headers.ContentType!.MediaType.Should().Be(ExcelXlsx.TipoMime);
        resp.Content.Headers.ContentDisposition!.FileNameStar.Should().StartWith("cobros-pendientes-").And.EndWith(".xlsx");

        var partes = ExcelXlsxTests.Partes(await resp.Content.ReadAsByteArrayAsync());
        var hoja = partes["xl/worksheets/sheet1.xml"];
        hoja.Should().Contain("<f>SUM(C2:C3)</f><v>1300.50</v>").And.Contain("<f>AVERAGE(D2:D3)</f>").And.Contain("Total · 2");
        partes["xl/workbook.xml"].Should().Contain("name=\"Cobros pendientes\"");
    }

    [Fact]
    public async Task Rechaza_peticiones_no_validas_y_exige_sesion()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await c.PostAsJsonAsync("/exportar/xlsx", new { titulo = "x", columnas = Array.Empty<object>(), filas = Array.Empty<object>() }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.PostAsJsonAsync("/exportar/xlsx", new { titulo = "x", columnas = new[] { new { titulo = "A", tipo = "raro" } }, filas = Array.Empty<object>() }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var anonimo = _fabrica.CreateClient();
        (await anonimo.PostAsJsonAsync("/exportar/xlsx", new { titulo = "x", columnas = new[] { new { titulo = "A" } }, filas = Array.Empty<object>() }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
