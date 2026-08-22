using System.IO.Compression;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la importación por Excel (.xlsx) de saldos, cartera y stock.</summary>
public sealed class ImportacionExcelEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ImportacionExcelEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ResultadoResp(int Total, int Correctas, int Errores, bool Aplicado, List<string> Mensajes);
    private sealed record AsientoResp(string Origen, decimal Total);
    private sealed record PrevisionResp(string Sentido, string Concepto, decimal Importe);
    private sealed record ProductoResp(Guid Id, decimal Stock);

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

    [Fact]
    public async Task Importa_saldos_como_asiento_de_apertura()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var xlsx = XlsxBase64(new[]
        {
            new[] { "cuenta", "debe", "haber" },
            new[] { "572", "1000", "0" },
            new[] { "100", "0", "1000" },
        });

        var prev = await (await cliente.PostAsJsonAsync("/importar/saldos", new { ContenidoBase64 = xlsx, Previsualizar = true })).Content.ReadFromJsonAsync<ResultadoResp>();
        prev!.Correctas.Should().Be(2);
        prev.Errores.Should().Be(0);
        prev.Aplicado.Should().BeFalse();

        var apl = await cliente.PostAsJsonAsync("/importar/saldos", new { ContenidoBase64 = xlsx, Previsualizar = false });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var anio = DateTime.UtcNow.Year;
        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={anio}");
        diario!.Should().Contain(a => a.Total == 1000m);
    }

    [Fact]
    public async Task Un_asiento_de_saldos_descuadrado_no_se_aplica()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var xlsx = XlsxBase64(new[]
        {
            new[] { "cuenta", "debe", "haber" },
            new[] { "572", "1000", "0" },
            new[] { "100", "0", "900" },
        });

        var apl = await cliente.PostAsJsonAsync("/importar/saldos", new { ContenidoBase64 = xlsx, Previsualizar = false });
        apl.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Importa_cartera_como_previsiones()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var xlsx = XlsxBase64(new[]
        {
            new[] { "sentido", "concepto", "importe", "fecha" },
            new[] { "cobro", "Factura 2025/100 · Cliente A", "500", "2026-01-31" },
            new[] { "pago", "Factura prov X", "300", "2026-02-15" },
        });

        var apl = await cliente.PostAsJsonAsync("/importar/cartera", new { ContenidoBase64 = xlsx, Previsualizar = false });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Correctas.Should().Be(2);

        var previsiones = await cliente.GetFromJsonAsync<List<PrevisionResp>>("/tesoreria/previsiones");
        previsiones!.Should().Contain(p => p.Sentido == "Ingreso" && p.Importe == 500m);
        previsiones.Should().Contain(p => p.Sentido == "Gasto" && p.Importe == 300m);
    }

    [Fact]
    public async Task Importa_existencias_iniciales_de_stock()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var prod = (await (await cliente.PostAsJsonAsync("/productos", new { Nombre = "Tornillo", Referencia = "TOR-1", PrecioUnitario = 1m, CodigoIva = "IVA21", Tipo = "Bien", ControlarStock = true })).Content.ReadFromJsonAsync<ProductoResp>())!;

        var xlsx = XlsxBase64(new[]
        {
            new[] { "referencia", "cantidad" },
            new[] { "TOR-1", "250" },
        });

        var apl = await cliente.PostAsJsonAsync("/importar/stock", new { ContenidoBase64 = xlsx, Previsualizar = false });
        apl.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = await apl.Content.ReadFromJsonAsync<ResultadoResp>();
        res!.Correctas.Should().Be(1);

        var actualizado = await cliente.GetFromJsonAsync<ProductoResp>($"/productos/{prod.Id}");
        actualizado!.Stock.Should().Be(250m);
    }
}
