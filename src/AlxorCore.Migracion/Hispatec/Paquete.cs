using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace AlxorCore.Migracion.Hispatec;

/// <summary>
/// Paquete de intercambio con Hispatec: un ZIP de CSV (UTF-8, separador «;», decimales con punto, fechas
/// aaaa-mm-dd, booleanos 0/1) que generan los scripts de <c>herramientas/hispatec</c> sobre la base de datos
/// de Hispatec. Cada archivo es una entidad ya normalizada: nombres completos, direcciones fiscales,
/// saldos calculados desde los apuntes, cartera pendiente, etc.
/// </summary>
public sealed class Paquete
{
    public const string Formato = "alxor-hispatec";
    public const int VersionSoportada = 1;

    /// <summary>Archivos del paquete y sus columnas obligatorias. Los que no están marcados como opcionales deben venir.</summary>
    public static IReadOnlyList<(string Nombre, bool Opcional, string[] Columnas)> Archivos { get; } =
    [
        ("manifiesto.csv", false, ["clave", "valor"]),
        ("terceros.csv", false, ["id_sujeto", "nif", "nombre", "calle", "codigo_postal", "poblacion", "provincia", "pais", "email", "iban"]),
        ("clientes.csv", false, ["id", "codigo", "id_sujeto", "subcuenta"]),
        ("proveedores.csv", false, ["id", "codigo", "id_sujeto", "subcuenta", "agricultor", "autoriza_autofactura", "retencion"]),
        ("familias.csv", true, ["id", "codigo", "nombre", "id_superior"]),
        ("articulos.csv", true, ["id", "codigo", "nombre", "id_familia", "unidad", "iva", "tipo", "controla_stock", "precio_venta", "precio_compra"]),
        ("cuentas.csv", true, ["codigo", "nombre"]),
        ("saldos.csv", true, ["cuenta", "debe", "haber"]),
        ("acumuladores.csv", true, ["cuenta", "debe", "haber"]),
        ("cartera.csv", true, ["id", "sentido", "id_tercero", "documento", "fecha_documento", "vencimiento", "importe"]),
        ("campanas.csv", true, ["id", "codigo", "nombre", "desde", "hasta"]),
        ("parcelas.csv", true, ["id", "codigo", "nombre", "id_proveedor", "sigpac", "superficie_ha", "id_articulo", "variedad"]),
    ];

    private readonly Dictionary<string, TablaCsv> _tablas;

    private Paquete(Dictionary<string, TablaCsv> tablas, IReadOnlyList<string> errores)
    {
        _tablas = tablas;
        ErroresLectura = errores;
    }

    /// <summary>Problemas de estructura (archivo que falta, columna que falta, ZIP ilegible…).</summary>
    public IReadOnlyList<string> ErroresLectura { get; }

    public IReadOnlyDictionary<string, string> Manifiesto =>
        Tabla("manifiesto.csv").Filas.Where(f => !string.IsNullOrWhiteSpace(f.Texto("clave")))
            .GroupBy(f => f.Texto("clave")!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Texto("valor") ?? string.Empty, StringComparer.OrdinalIgnoreCase);

    public TablaCsv Tabla(string nombre) => _tablas.TryGetValue(nombre, out var t) ? t : TablaCsv.Vacia(nombre);

    public bool Tiene(string nombre) => _tablas.ContainsKey(nombre);

    /// <summary>Lee el paquete desde el ZIP en base64.</summary>
    public static Paquete Leer(string? zipBase64)
    {
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(zipBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            return new Paquete([], ["El paquete no es un base64 válido."]);
        }

        return Leer(bytes);
    }

    public static Paquete Leer(byte[] zip)
    {
        ArgumentNullException.ThrowIfNull(zip);
        var tablas = new Dictionary<string, TablaCsv>(StringComparer.OrdinalIgnoreCase);
        var errores = new List<string>();
        try
        {
            using var archivo = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);
            foreach (var entrada in archivo.Entries.Where(e => e.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)))
            {
                using var lector = new StreamReader(entrada.Open(), new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true);
                tablas[entrada.Name.ToLowerInvariant()] = TablaCsv.Parsear(entrada.Name.ToLowerInvariant(), lector.ReadToEnd());
            }
        }
        catch (InvalidDataException)
        {
            return new Paquete([], ["El paquete no es un ZIP válido."]);
        }

        foreach (var (nombre, opcional, columnas) in Archivos)
        {
            if (!tablas.TryGetValue(nombre, out var t))
            {
                if (!opcional)
                {
                    errores.Add($"Falta el archivo {nombre}.");
                }

                continue;
            }

            var faltan = columnas.Where(c => !t.Columnas.Contains(c, StringComparer.OrdinalIgnoreCase)).ToList();
            if (faltan.Count > 0)
            {
                errores.Add($"{nombre}: faltan las columnas {string.Join(", ", faltan)}.");
            }
        }

        var paquete = new Paquete(tablas, errores);
        if (tablas.ContainsKey("manifiesto.csv"))
        {
            var m = paquete.Manifiesto;
            if (!string.Equals(m.GetValueOrDefault("formato"), Formato, StringComparison.OrdinalIgnoreCase))
            {
                errores.Add($"El manifiesto no es de un paquete «{Formato}».");
            }

            if (!int.TryParse(m.GetValueOrDefault("version"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) || v != VersionSoportada)
            {
                errores.Add($"Versión de paquete no soportada (se admite la {VersionSoportada}).");
            }

            if (Valor.Fecha(m.GetValueOrDefault("fecha_corte")) is null)
            {
                errores.Add("El manifiesto no indica una fecha_corte válida (aaaa-mm-dd).");
            }
        }

        return paquete;
    }

    public DateOnly FechaCorte => Valor.Fecha(Manifiesto.GetValueOrDefault("fecha_corte")) ?? DateOnly.MinValue;
}

/// <summary>Un CSV del paquete.</summary>
public sealed class TablaCsv
{
    private TablaCsv(string nombre, IReadOnlyList<string> columnas, IReadOnlyList<FilaCsv> filas)
    {
        Nombre = nombre;
        Columnas = columnas;
        Filas = filas;
    }

    public string Nombre { get; }

    public IReadOnlyList<string> Columnas { get; }

    public IReadOnlyList<FilaCsv> Filas { get; }

    public static TablaCsv Vacia(string nombre) => new(nombre, [], []);

    /// <summary>CSV con separador «;» y comillas dobles (una comilla dentro se escribe doble).</summary>
    public static TablaCsv Parsear(string nombre, string texto)
    {
        ArgumentNullException.ThrowIfNull(texto);
        var registros = Registros(texto).ToList();
        if (registros.Count == 0)
        {
            return Vacia(nombre);
        }

        var columnas = registros[0].Select(c => c.Trim().ToLowerInvariant()).ToList();
        var filas = registros.Skip(1)
            .Select((campos, i) => (campos, numero: i + 2))
            .Where(x => x.campos.Any(c => c.Length > 0))
            .Select(x => new FilaCsv(x.numero, columnas.Select((c, i) => (c, v: i < x.campos.Count ? x.campos[i] : string.Empty))
                .GroupBy(p => p.c).ToDictionary(g => g.Key, g => g.First().v, StringComparer.OrdinalIgnoreCase)))
            .ToList();
        return new TablaCsv(nombre, columnas, filas);
    }

    private static IEnumerable<List<string>> Registros(string texto)
    {
        var campo = new StringBuilder();
        var registro = new List<string>();
        var entreComillas = false;
        for (var i = 0; i < texto.Length; i++)
        {
            var c = texto[i];
            if (entreComillas)
            {
                if (c == '"' && i + 1 < texto.Length && texto[i + 1] == '"')
                {
                    campo.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    entreComillas = false;
                }
                else
                {
                    campo.Append(c);
                }

                continue;
            }

            switch (c)
            {
                case '"':
                    entreComillas = true;
                    break;
                case ';':
                    registro.Add(campo.ToString());
                    campo.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    registro.Add(campo.ToString());
                    campo.Clear();
                    yield return registro;
                    registro = [];
                    break;
                default:
                    campo.Append(c);
                    break;
            }
        }

        if (campo.Length > 0 || registro.Count > 0)
        {
            registro.Add(campo.ToString());
            yield return registro;
        }
    }
}

/// <summary>Una fila de un CSV del paquete.</summary>
public sealed class FilaCsv
{
    private readonly Dictionary<string, string> _valores;

    public FilaCsv(int numero, Dictionary<string, string> valores)
    {
        Numero = numero;
        _valores = valores;
    }

    /// <summary>Número de línea en el archivo (la cabecera es la 1).</summary>
    public int Numero { get; }

    /// <summary>Texto sin espacios a los lados; null si está vacío o es «NULL».</summary>
    public string? Texto(string columna)
    {
        var v = _valores.TryGetValue(columna, out var x) ? x.Trim() : null;
        return string.IsNullOrEmpty(v) || v.Equals("NULL", StringComparison.OrdinalIgnoreCase) ? null : v;
    }
}

/// <summary>Conversión de valores del paquete (formato invariable).</summary>
public static class Valor
{
    public static decimal? Numero(string? v) =>
        v is not null && decimal.TryParse(v.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;

    public static DateOnly? Fecha(string? v) =>
        v is not null && DateOnly.TryParseExact(v.Length >= 10 ? v[..10] : v, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

    public static bool Booleano(string? v) => v is "1" || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(v, "s", StringComparison.OrdinalIgnoreCase);
}
