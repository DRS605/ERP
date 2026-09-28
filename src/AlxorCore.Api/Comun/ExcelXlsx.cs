using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace AlxorCore.Api.Comun;

/// <summary>Tipo de dato de una columna exportada a Excel: decide el formato de la celda.</summary>
public enum TipoColumnaExcel
{
    /// <summary>Texto libre (también códigos, NIF, números de documento…: nunca se convierte a número).</summary>
    Texto,

    /// <summary>Número con separador de miles (dos decimales si alguna fila los tiene).</summary>
    Numero,

    /// <summary>Importe en euros: <c>#,##0.00 €</c>.</summary>
    Moneda,

    /// <summary>Fecha <c>dd/mm/yyyy</c>.</summary>
    Fecha,

    /// <summary>
    /// Porcentaje <c>0.00%</c>. El valor se da <b>en tanto por ciento</b> (21 = 21 %) y se guarda como
    /// fracción (0,21), que es lo que Excel espera.
    /// </summary>
    Porcentaje,
}

/// <summary>Qué calcula la fila de totales en una columna.</summary>
public enum TotalColumnaExcel
{
    /// <summary>Suma en columnas <see cref="TipoColumnaExcel.Numero"/> y <see cref="TipoColumnaExcel.Moneda"/>; nada en las demás.</summary>
    Auto,

    /// <summary>Sin total.</summary>
    Ninguno,

    /// <summary>Fórmula <c>SUM</c>.</summary>
    Suma,

    /// <summary>Fórmula <c>AVERAGE</c> (precios, porcentajes…).</summary>
    Media,
}

/// <summary>Columna de una exportación a Excel.</summary>
/// <param name="Titulo">Texto de la cabecera.</param>
/// <param name="Tipo">Tipo de dato (formato de celda).</param>
/// <param name="Total">Cálculo de la fila de totales.</param>
public sealed record ColumnaExcel(string Titulo, TipoColumnaExcel Tipo = TipoColumnaExcel.Texto, TotalColumnaExcel Total = TotalColumnaExcel.Auto);

/// <summary>
/// Escritor mínimo de hojas de cálculo <c>.xlsx</c> (SpreadsheetML) sin dependencias externas: una hoja con
/// cabecera en negrita y fondo, fila de cabecera inmovilizada, autofiltro, anchos de columna razonables,
/// formatos numéricos (moneda, porcentaje, fecha) y, si se pide, una fila de totales con fórmulas
/// <c>SUM</c>/<c>AVERAGE</c> (con su valor ya calculado, para visores que no recalculan).
/// </summary>
/// <remarks>
/// Valores admitidos en las filas: <c>null</c>, cadenas, números (<see cref="int"/>, <see cref="long"/>,
/// <see cref="decimal"/>, <see cref="double"/>…), <see cref="bool"/>, <see cref="DateOnly"/>,
/// <see cref="DateTime"/> y <see cref="DateTimeOffset"/>. En columnas numéricas o de fecha también se aceptan
/// cadenas: «1.234,56 €», «12,5 %», «1234.56», «31/12/2026» o «2026-12-31»; si no se pueden interpretar,
/// se escriben como texto.
/// </remarks>
public static class ExcelXlsx
{
    /// <summary>Máximo de filas de datos por exportación.</summary>
    public const int MaximoFilas = 100_000;

    /// <summary>Máximo de columnas por exportación.</summary>
    public const int MaximoColumnas = 200;

    /// <summary>Tipo MIME de un .xlsx.</summary>
    public const string TipoMime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private const int MaximoTextoCelda = 32_767;
    private static readonly DateTime OrigenExcel = new(1899, 12, 30, 0, 0, 0, DateTimeKind.Unspecified);

    // Índices de cellXfs (ver EstilosXml).
    private const int EstiloCabecera = 1;
    private const int EstiloTexto = 0;
    private const int EstiloDecimal = 2;
    private const int EstiloEntero = 3;
    private const int EstiloMoneda = 4;
    private const int EstiloPorcentaje = 5;
    private const int EstiloFecha = 6;
    private const int EstiloTotalTexto = 7;

    /// <summary>Estilo de la fila de totales para un estilo de dato (negrita, fondo y doble raya).</summary>
    private static int EstiloTotal(int estiloDato) => estiloDato == EstiloTexto ? EstiloTotalTexto : estiloDato + 6;

    /// <summary>Genera el .xlsx. Con <paramref name="filaTotales"/> añade una fila «Total» con fórmulas.</summary>
    /// <exception cref="ArgumentException">Sin columnas, demasiadas columnas o demasiadas filas.</exception>
    public static byte[] Generar(string titulo, IReadOnlyList<ColumnaExcel> columnas, IEnumerable<IReadOnlyList<object?>> filas, bool filaTotales)
        => Generar(titulo, columnas, filas, filaTotales ? [] : null);

    /// <summary>
    /// Genera el .xlsx con una fila de totales si <paramref name="totales"/> no es <c>null</c>. En las columnas
    /// con total (<see cref="TotalColumnaExcel"/>) se escribe la fórmula; en las demás, el valor que traiga
    /// <paramref name="totales"/> en esa posición (p. ej. una etiqueta «Total · 23»). Si la primera columna
    /// no tiene total ni valor, se rotula «Total».
    /// </summary>
    public static byte[] Generar(string titulo, IReadOnlyList<ColumnaExcel> columnas, IEnumerable<IReadOnlyList<object?>> filas, IReadOnlyList<object?>? totales)
    {
        ArgumentNullException.ThrowIfNull(columnas);
        ArgumentNullException.ThrowIfNull(filas);
        if (columnas.Count == 0)
        {
            throw new ArgumentException("La exportación necesita al menos una columna.", nameof(columnas));
        }

        if (columnas.Count > MaximoColumnas)
        {
            throw new ArgumentException($"Como máximo {MaximoColumnas} columnas.", nameof(columnas));
        }

        var datos = new List<object?[]>();
        foreach (var fila in filas)
        {
            if (datos.Count >= MaximoFilas)
            {
                throw new ArgumentException($"Como máximo {MaximoFilas:N0} filas por exportación.", nameof(filas));
            }

            var normalizada = new object?[columnas.Count];
            for (var j = 0; j < columnas.Count && fila is not null && j < fila.Count; j++)
            {
                normalizada[j] = Normalizar(fila[j], columnas[j].Tipo);
            }

            datos.Add(normalizada);
        }

        var hoja = HojaXml(columnas, datos, totales);
        var nombreHoja = NombreHoja(titulo);

        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            Escribir(zip, "[Content_Types].xml", TiposContenidoXml);
            Escribir(zip, "_rels/.rels", RelacionesRaizXml);
            Escribir(zip, "docProps/core.xml", PropiedadesXml(titulo));
            Escribir(zip, "docProps/app.xml", AplicacionXml);
            Escribir(zip, "xl/workbook.xml", LibroXml(nombreHoja, columnas.Count, datos.Count));
            Escribir(zip, "xl/_rels/workbook.xml.rels", RelacionesLibroXml);
            Escribir(zip, "xl/styles.xml", EstilosXml);
            Escribir(zip, "xl/worksheets/sheet1.xml", hoja);
        }

        return ms.ToArray();
    }

    /// <summary>Nombre de hoja válido para Excel: sin <c>[]:*?/\</c>, sin apóstrofos en los extremos y ≤ 31 caracteres.</summary>
    public static string NombreHoja(string? titulo)
    {
        var sb = new StringBuilder();
        foreach (var c in titulo ?? string.Empty)
        {
            if (c is '[' or ']' or ':' or '*' or '?' or '/' or '\\' || char.IsControl(c))
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append(c);
            }
        }

        var nombre = string.Join(' ', sb.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)).Trim('\'').Trim();
        if (nombre.Length > 31)
        {
            nombre = nombre[..31].TrimEnd().Trim('\'');
        }

        return string.IsNullOrWhiteSpace(nombre) ? "Hoja1" : nombre;
    }

    /// <summary>Letras de columna de Excel (0 → A, 25 → Z, 26 → AA…).</summary>
    public static string LetraColumna(int indice)
    {
        var s = string.Empty;
        for (var n = indice + 1; n > 0; n = (n - 1) / 26)
        {
            s = (char)('A' + ((n - 1) % 26)) + s;
        }

        return s;
    }

    // ---------------------------------------------------------------- valores

    private static object? Normalizar(object? valor, TipoColumnaExcel tipo)
    {
        switch (valor)
        {
            case null:
                return null;
            case string s when string.IsNullOrWhiteSpace(s):
                return null;
            case string s:
                return tipo switch
                {
                    TipoColumnaExcel.Numero or TipoColumnaExcel.Moneda or TipoColumnaExcel.Porcentaje =>
                        LeerNumero(s) is { } n ? Numerico(n, tipo) : s,
                    TipoColumnaExcel.Fecha => LeerFecha(s) is { } f ? f : s,
                    _ => s,
                };
            case bool b:
                return tipo == TipoColumnaExcel.Texto ? (b ? "Sí" : "No") : (b ? 1m : 0m);
            case DateOnly d:
                return d.ToDateTime(TimeOnly.MinValue);
            case DateTimeOffset o:
                return o.DateTime;
            case DateTime dt:
                return dt;
            case decimal or double or float or int or long or short or byte or uint or ulong or ushort or sbyte:
                var numero = Convert.ToDecimal(valor, CultureInfo.InvariantCulture);
                return tipo == TipoColumnaExcel.Texto ? numero.ToString(CultureInfo.InvariantCulture) : Numerico(numero, tipo);
            default:
                return Convert.ToString(valor, CultureInfo.InvariantCulture);
        }
    }

    private static decimal Numerico(decimal n, TipoColumnaExcel tipo) => tipo == TipoColumnaExcel.Porcentaje ? n / 100m : n;

    /// <summary>Lee «1.234,56 €», «-12,00», «12,5 %», «1234.56» o «1,234.56».</summary>
    internal static decimal? LeerNumero(string texto)
    {
        var s = new string(texto.Where(c => char.IsDigit(c) || c is ',' or '.' or '-').ToArray());
        if (s.Length == 0 || !s.Any(char.IsDigit))
        {
            return null;
        }

        var coma = s.LastIndexOf(',');
        var punto = s.LastIndexOf('.');
        if (coma >= 0 && punto >= 0)
        {
            // El último separador es el decimal.
            s = coma > punto ? s.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.') : s.Replace(",", string.Empty, StringComparison.Ordinal);
        }
        else if (coma >= 0)
        {
            s = s.Replace(',', '.');
        }
        else if (punto >= 0 && s.Count(c => c == '.') > 1)
        {
            s = s.Replace(".", string.Empty, StringComparison.Ordinal); // 1.234.567
        }

        return decimal.TryParse(s, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var n) ? n : null;
    }

    private static DateTime? LeerFecha(string texto)
    {
        var t = texto.Trim();
        string[] formatos = ["dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ssK", "yyyy-MM-ddTHH:mm:ss.FFFFFFFK"];
        return DateTime.TryParseExact(t, formatos, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var f) ? f.Date : null;
    }

    private static string Texto(string s)
    {
        var limpio = new StringBuilder(Math.Min(s.Length, MaximoTextoCelda));
        foreach (var c in s)
        {
            if (limpio.Length >= MaximoTextoCelda)
            {
                break;
            }

            // Solo caracteres válidos en XML 1.0.
            if (c is '\t' or '\n' or '\r' || (c >= 0x20 && c != 0xFFFE && c != 0xFFFF))
            {
                limpio.Append(c);
            }
        }

        return SecurityElement.Escape(limpio.ToString());
    }

    private static string Num(decimal n) => n.ToString(CultureInfo.InvariantCulture);

    private static string Num(double n) => n.ToString("R", CultureInfo.InvariantCulture);

    // ---------------------------------------------------------------- hoja

    private static string HojaXml(IReadOnlyList<ColumnaExcel> columnas, List<object?[]> datos, IReadOnlyList<object?>? totales)
    {
        var n = columnas.Count;
        var estilos = new int[n];
        var anchos = new double[n];
        for (var j = 0; j < n; j++)
        {
            var conDecimales = datos.Any(f => f[j] is decimal d && d != decimal.Truncate(d));
            estilos[j] = columnas[j].Tipo switch
            {
                TipoColumnaExcel.Moneda => EstiloMoneda,
                TipoColumnaExcel.Porcentaje => EstiloPorcentaje,
                TipoColumnaExcel.Fecha => EstiloFecha,
                TipoColumnaExcel.Numero => conDecimales ? EstiloDecimal : EstiloEntero,
                _ => EstiloTexto,
            };

            var maximo = (columnas[j].Titulo ?? string.Empty).Length + 3; // hueco para la flecha del autofiltro
            foreach (var f in datos.Take(5_000))
            {
                var largo = f[j] switch
                {
                    null => 0,
                    string s => s.Length,
                    DateTime => 10,
                    decimal d => d.ToString("#,##0.00", CultureInfo.InvariantCulture).Length + (columnas[j].Tipo == TipoColumnaExcel.Moneda ? 2 : 0),
                    _ => 8,
                };
                maximo = Math.Max(maximo, largo);
            }

            anchos[j] = Math.Clamp(maximo + 2, 8, 60);
        }

        var ultimaFila = datos.Count + 1;
        var ultimaCol = LetraColumna(n - 1);
        var sb = new StringBuilder(1024 + (datos.Count * n * 24));
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
        sb.Append("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">");
        sb.Append("<sheetPr><pageSetUpPr fitToPage=\"1\"/></sheetPr>");
        sb.Append(CultureInfo.InvariantCulture, $"<dimension ref=\"A1:{ultimaCol}{ultimaFila + (totales is null ? 0 : 1)}\"/>");
        sb.Append("<sheetViews><sheetView workbookViewId=\"0\"><pane ySplit=\"1\" topLeftCell=\"A2\" activePane=\"bottomLeft\" state=\"frozen\"/><selection pane=\"bottomLeft\" activeCell=\"A2\" sqref=\"A2\"/></sheetView></sheetViews>");
        sb.Append("<sheetFormatPr defaultRowHeight=\"15\"/>");
        sb.Append("<cols>");
        for (var j = 0; j < n; j++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<col min=\"{j + 1}\" max=\"{j + 1}\" width=\"{Num(anchos[j])}\" customWidth=\"1\"/>");
        }

        sb.Append("</cols><sheetData>");

        // Cabecera.
        sb.Append("<row r=\"1\">");
        for (var j = 0; j < n; j++)
        {
            sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{LetraColumna(j)}1\" s=\"{EstiloCabecera}\" t=\"inlineStr\"><is><t>{Texto(columnas[j].Titulo ?? string.Empty)}</t></is></c>");
        }

        sb.Append("</row>");

        for (var i = 0; i < datos.Count; i++)
        {
            var r = i + 2;
            sb.Append(CultureInfo.InvariantCulture, $"<row r=\"{r}\">");
            for (var j = 0; j < n; j++)
            {
                Celda(sb, $"{LetraColumna(j)}{r}", datos[i][j], estilos[j]);
            }

            sb.Append("</row>");
        }

        if (totales is not null)
        {
            var r = ultimaFila + 1;
            sb.Append(CultureInfo.InvariantCulture, $"<row r=\"{r}\">");
            for (var j = 0; j < n; j++)
            {
                var referencia = $"{LetraColumna(j)}{r}";
                var total = columnas[j].Total == TotalColumnaExcel.Auto
                    ? (columnas[j].Tipo is TipoColumnaExcel.Numero or TipoColumnaExcel.Moneda ? TotalColumnaExcel.Suma : TotalColumnaExcel.Ninguno)
                    : columnas[j].Total;
                var estiloTotal = EstiloTotal(estilos[j]);
                if (total is TotalColumnaExcel.Suma or TotalColumnaExcel.Media && columnas[j].Tipo != TipoColumnaExcel.Texto)
                {
                    var valores = datos.Select(f => f[j]).OfType<decimal>().ToList();
                    var calculado = total == TotalColumnaExcel.Suma ? valores.Sum() : (valores.Count > 0 ? valores.Average() : 0m);
                    var funcion = total == TotalColumnaExcel.Suma ? "SUM" : "AVERAGE";
                    var rango = datos.Count > 0 ? $"{LetraColumna(j)}2:{LetraColumna(j)}{ultimaFila}" : $"{LetraColumna(j)}2:{LetraColumna(j)}2";
                    sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\" s=\"{estiloTotal}\"><f>{funcion}({rango})</f><v>{Num(calculado)}</v></c>");
                    continue;
                }

                var valor = j < totales.Count ? Normalizar(totales[j], columnas[j].Tipo) : null;
                if (valor is null && j == 0)
                {
                    valor = "Total";
                }

                Celda(sb, referencia, valor, valor is string ? EstiloTotalTexto : estiloTotal);
            }

            sb.Append("</row>");
        }

        sb.Append("</sheetData>");
        sb.Append(CultureInfo.InvariantCulture, $"<autoFilter ref=\"A1:{ultimaCol}{Math.Max(ultimaFila, 2)}\"/>");
        sb.Append("<pageMargins left=\"0.5\" right=\"0.5\" top=\"0.75\" bottom=\"0.75\" header=\"0.3\" footer=\"0.3\"/>");
        sb.Append("<pageSetup paperSize=\"9\" orientation=\"landscape\" fitToWidth=\"1\" fitToHeight=\"0\"/>");
        sb.Append("<headerFooter><oddFooter>&amp;L&amp;A&amp;RPágina &amp;P de &amp;N</oddFooter></headerFooter>");
        sb.Append("</worksheet>");
        return sb.ToString();
    }

    private static void Celda(StringBuilder sb, string referencia, object? valor, int estilo)
    {
        switch (valor)
        {
            case null:
                if (estilo != EstiloTexto)
                {
                    sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\" s=\"{estilo}\"/>");
                }

                break;
            case decimal d:
                sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\" s=\"{estilo}\"><v>{Num(d)}</v></c>");
                break;
            case DateTime f:
                var serie = (f.Date - OrigenExcel).TotalDays;
                sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\" s=\"{(estilo >= EstiloTotalTexto ? EstiloTotal(EstiloFecha) : EstiloFecha)}\"><v>{Num(serie)}</v></c>");
                break;
            default:
                // Texto en línea: nunca se interpreta como fórmula (a diferencia del CSV).
                var t = Convert.ToString(valor, CultureInfo.InvariantCulture) ?? string.Empty;
                var espacio = t.Length > 0 && (char.IsWhiteSpace(t[0]) || char.IsWhiteSpace(t[^1])) ? " xml:space=\"preserve\"" : string.Empty;
                var estiloTexto = estilo >= EstiloTotalTexto ? EstiloTotalTexto : EstiloTexto;
                sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\"{(estiloTexto == EstiloTexto ? string.Empty : $" s=\"{estiloTexto}\"")} t=\"inlineStr\"><is><t{espacio}>{Texto(t)}</t></is></c>");
                break;
        }
    }

    // ---------------------------------------------------------------- partes fijas

    private static void Escribir(ZipArchive zip, string ruta, string contenido)
    {
        var entrada = zip.CreateEntry(ruta, CompressionLevel.Optimal);
        using var w = new StreamWriter(entrada.Open(), new UTF8Encoding(false));
        w.Write(contenido);
    }

    private static string LibroXml(string nombreHoja, int columnas, int filas)
        => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
           + "<workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\">"
           + "<bookViews><workbookView/></bookViews>"
           + $"<sheets><sheet name=\"{SecurityElement.Escape(nombreHoja)}\" sheetId=\"1\" r:id=\"rId1\"/></sheets>"
           + $"<definedNames><definedName name=\"_xlnm._FilterDatabase\" localSheetId=\"0\" hidden=\"1\">'{SecurityElement.Escape(nombreHoja.Replace("'", "''", StringComparison.Ordinal))}'!$A$1:${LetraColumna(columnas - 1)}${Math.Max(filas + 1, 2)}</definedName>"
           + $"<definedName name=\"_xlnm.Print_Titles\" localSheetId=\"0\">'{SecurityElement.Escape(nombreHoja.Replace("'", "''", StringComparison.Ordinal))}'!$1:$1</definedName></definedNames>"
           + "<calcPr calcId=\"191029\" fullCalcOnLoad=\"1\"/>"
           + "</workbook>";

    private static string PropiedadesXml(string? titulo)
        => "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
           + "<cp:coreProperties xmlns:cp=\"http://schemas.openxmlformats.org/package/2006/metadata/core-properties\" xmlns:dc=\"http://purl.org/dc/elements/1.1/\" xmlns:dcterms=\"http://purl.org/dc/terms/\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">"
           + $"<dc:title>{Texto(titulo ?? string.Empty)}</dc:title><dc:creator>ALXOR Core</dc:creator>"
           + $"<dcterms:created xsi:type=\"dcterms:W3CDTF\">{DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)}</dcterms:created>"
           + "</cp:coreProperties>";

    private const string AplicacionXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<Properties xmlns=\"http://schemas.openxmlformats.org/officeDocument/2006/extended-properties\"><Application>ALXOR Core</Application></Properties>";

    private const string TiposContenidoXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
        + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
        + "<Default Extension=\"xml\" ContentType=\"application/xml\"/>"
        + "<Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/>"
        + "<Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>"
        + "<Override PartName=\"/xl/styles.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml\"/>"
        + "<Override PartName=\"/docProps/core.xml\" ContentType=\"application/vnd.openxmlformats-package.core-properties+xml\"/>"
        + "<Override PartName=\"/docProps/app.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.extended-properties+xml\"/>"
        + "</Types>";

    private const string RelacionesRaizXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
        + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
        + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties\" Target=\"docProps/core.xml\"/>"
        + "<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties\" Target=\"docProps/app.xml\"/>"
        + "</Relationships>";

    private const string RelacionesLibroXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
        + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/>"
        + "<Relationship Id=\"rId2\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>"
        + "</Relationships>";

    // cellXfs: 0 texto · 1 cabecera · 2 decimal · 3 entero · 4 moneda · 5 porcentaje · 6 fecha
    //          7 total texto · 8 total decimal · 9 total entero · 10 total moneda · 11 total porcentaje · 12 total fecha
    private const string EstilosXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>"
        + "<styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
        + "<numFmts count=\"2\"><numFmt numFmtId=\"164\" formatCode=\"#,##0.00\\ &quot;€&quot;\"/><numFmt numFmtId=\"165\" formatCode=\"dd/mm/yyyy\"/></numFmts>"
        + "<fonts count=\"2\"><font><sz val=\"11\"/><name val=\"Calibri\"/><family val=\"2\"/></font><font><b/><sz val=\"11\"/><name val=\"Calibri\"/><family val=\"2\"/></font></fonts>"
        + "<fills count=\"4\"><fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill>"
        + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFE0F7FB\"/><bgColor indexed=\"64\"/></patternFill></fill>"
        + "<fill><patternFill patternType=\"solid\"><fgColor rgb=\"FFF1F5F9\"/><bgColor indexed=\"64\"/></patternFill></fill></fills>"
        + "<borders count=\"3\"><border><left/><right/><top/><bottom/><diagonal/></border>"
        + "<border><left/><right/><top/><bottom style=\"thin\"><color rgb=\"FF0891B2\"/></bottom><diagonal/></border>"
        + "<border><left/><right/><top style=\"thin\"><color rgb=\"FF0F172A\"/></top><bottom style=\"double\"><color rgb=\"FF0F172A\"/></bottom><diagonal/></border></borders>"
        + "<cellStyleXfs count=\"1\"><xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/></cellStyleXfs>"
        + "<cellXfs count=\"13\">"
        + "<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\"/>"
        + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"2\" borderId=\"1\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\" applyAlignment=\"1\"><alignment vertical=\"center\" wrapText=\"1\"/></xf>"
        + "<xf numFmtId=\"4\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"3\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"164\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"10\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"165\" fontId=\"0\" fillId=\"0\" borderId=\"0\" xfId=\"0\" applyNumberFormat=\"1\"/>"
        + "<xf numFmtId=\"0\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "<xf numFmtId=\"4\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "<xf numFmtId=\"3\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "<xf numFmtId=\"164\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "<xf numFmtId=\"10\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "<xf numFmtId=\"165\" fontId=\"1\" fillId=\"3\" borderId=\"2\" xfId=\"0\" applyNumberFormat=\"1\" applyFont=\"1\" applyFill=\"1\" applyBorder=\"1\"/>"
        + "</cellXfs>"
        + "<cellStyles count=\"1\"><cellStyle name=\"Normal\" xfId=\"0\" builtinId=\"0\"/></cellStyles>"
        + "</styleSheet>";
}
