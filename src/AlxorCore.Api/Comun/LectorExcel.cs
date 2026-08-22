using System.IO.Compression;
using System.Xml.Linq;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Lector mínimo de hojas de cálculo <c>.xlsx</c> sin dependencias externas: abre el ZIP, resuelve la
/// tabla de cadenas compartidas y lee la primera hoja, exponiendo cada fila como
/// <see cref="LectorCsv.FilaCsv"/> (columna normalizada → valor), igual que el importador CSV. Las
/// fechas se devuelven como el número de serie de Excel; el importador que lo necesite lo convierte.
/// </summary>
public static class LectorExcel
{
    private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    /// <summary>Parsea el contenido binario de un .xlsx. La primera fila es la cabecera.</summary>
    public static IReadOnlyList<LectorCsv.FilaCsv> Parsear(byte[] contenido)
    {
        using var zip = new ZipArchive(new MemoryStream(contenido), ZipArchiveMode.Read);

        var compartidas = LeerCadenasCompartidas(zip);
        var hoja = zip.Entries.FirstOrDefault(e => e.FullName.StartsWith("xl/worksheets/sheet", StringComparison.OrdinalIgnoreCase) && e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("El archivo no contiene ninguna hoja de cálculo.");

        using var flujo = hoja.Open();
        var doc = XDocument.Load(flujo);
        var filasXml = doc.Root?.Element(Ns + "sheetData")?.Elements(Ns + "row").ToList() ?? [];
        if (filasXml.Count == 0)
        {
            return [];
        }

        var cabecera = CeldasPorColumna(filasXml[0], compartidas);
        var indiceCabecera = cabecera.ToDictionary(c => c.Columna, c => LectorCsv.Normalizar(c.Valor));

        var resultado = new List<LectorCsv.FilaCsv>();
        for (var i = 1; i < filasXml.Count; i++)
        {
            var celdas = CeldasPorColumna(filasXml[i], compartidas);
            var valores = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var celda in celdas)
            {
                if (indiceCabecera.TryGetValue(celda.Columna, out var col) && !string.IsNullOrEmpty(col))
                {
                    valores[col] = celda.Valor;
                }
            }

            if (valores.Values.Any(v => !string.IsNullOrWhiteSpace(v)))
            {
                resultado.Add(new LectorCsv.FilaCsv(i + 1, valores));
            }
        }

        return resultado;
    }

    /// <summary>Convierte un número de serie de Excel a fecha (base 1899-12-30). Null si no es numérico.</summary>
    public static DateOnly? FechaDeSerie(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        if (double.TryParse(valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var serie) && serie > 0)
        {
            return DateOnly.FromDateTime(new DateTime(1899, 12, 30, 0, 0, 0, DateTimeKind.Unspecified).AddDays(serie));
        }

        // Formatos habituales (modo invariante: no se usan culturas con nombre).
        var formatos = new[] { "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy" };
        if (DateOnly.TryParseExact(valor, formatos, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var f) ||
            DateOnly.TryParse(valor, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out f))
        {
            return f;
        }

        return null;
    }

    private static List<string> LeerCadenasCompartidas(ZipArchive zip)
    {
        var entrada = zip.GetEntry("xl/sharedStrings.xml");
        if (entrada is null)
        {
            return [];
        }

        using var flujo = entrada.Open();
        var doc = XDocument.Load(flujo);
        return (doc.Root?.Elements(Ns + "si") ?? []).Select(si => string.Concat(si.Descendants(Ns + "t").Select(t => t.Value))).ToList();
    }

    private static List<(int Columna, string Valor)> CeldasPorColumna(XElement fila, IReadOnlyList<string> compartidas)
    {
        var celdas = new List<(int, string)>();
        foreach (var c in fila.Elements(Ns + "c"))
        {
            var refCelda = (string?)c.Attribute("r") ?? string.Empty;
            var columna = ColumnaDeRef(refCelda);
            var tipo = (string?)c.Attribute("t");
            string valor;
            if (tipo == "s")
            {
                var idx = int.TryParse(c.Element(Ns + "v")?.Value, out var n) ? n : -1;
                valor = idx >= 0 && idx < compartidas.Count ? compartidas[idx] : string.Empty;
            }
            else if (tipo == "inlineStr")
            {
                valor = string.Concat(c.Descendants(Ns + "t").Select(t => t.Value));
            }
            else
            {
                valor = c.Element(Ns + "v")?.Value ?? string.Empty;
            }

            celdas.Add((columna, valor.Trim()));
        }

        return celdas;
    }

    private static int ColumnaDeRef(string refCelda)
    {
        var col = 0;
        foreach (var ch in refCelda)
        {
            if (char.IsLetter(ch))
            {
                col = (col * 26) + (char.ToUpperInvariant(ch) - 'A' + 1);
            }
            else
            {
                break;
            }
        }

        return col - 1;
    }
}
