using System.Globalization;
using System.Text;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Constructor de un registro de <b>ancho fijo</b> para los ficheros telemáticos de la AEAT
/// (declaraciones informativas: 190, 347, 349…). Todos los registros miden 500 posiciones, se
/// codifican en <b>ISO-8859-1</b> y se separan por CRLF.
///
/// <para>Convenciones oficiales (verificadas):</para>
/// <list type="bullet">
///   <item>Campos <b>alfanuméricos</b>: alineados a la izquierda, rellenos de blancos a la derecha,
///   en MAYÚSCULAS y sin acentos (la Ñ se conserva vía ISO-8859-1).</item>
///   <item>Campos <b>numéricos</b>: alineados a la derecha, rellenos de ceros a la izquierda.</item>
///   <item>Campos de <b>importe</b>: sin coma; los 2 últimos dígitos son los céntimos (decimales
///   implícitos). El <b>signo</b> va en un campo aparte inmediatamente anterior: «N» si es
///   negativo, espacio en blanco si es positivo o cero.</item>
/// </list>
///
/// Todas las posiciones son <b>1-indexadas e inclusivas</b> (como en los diseños de registro del BOE).
/// </summary>
public sealed class RegistroAeat
{
    /// <summary>Longitud fija de todos los registros de las declaraciones informativas.</summary>
    public const int Longitud = 500;

    private readonly char[] _buffer;

    public RegistroAeat(int longitud = Longitud)
    {
        _buffer = new char[longitud];
        Array.Fill(_buffer, ' ');
    }

    /// <summary>Escribe texto alfanumérico (izquierda, blancos a la derecha, mayúsculas sin acentos).</summary>
    public RegistroAeat Alfa(int desde, int hasta, string? valor)
    {
        var longitud = hasta - desde + 1;
        var limpio = NormalizarTexto(valor);
        if (limpio.Length > longitud)
        {
            limpio = limpio[..longitud];
        }

        Copiar(desde, limpio.PadRight(longitud));
        return this;
    }

    /// <summary>Escribe un entero (derecha, ceros a la izquierda).</summary>
    public RegistroAeat Num(int desde, int hasta, long valor)
    {
        var longitud = hasta - desde + 1;
        var texto = Math.Abs(valor).ToString(CultureInfo.InvariantCulture);
        if (texto.Length > longitud)
        {
            texto = texto[^longitud..];
        }

        Copiar(desde, texto.PadLeft(longitud, '0'));
        return this;
    }

    /// <summary>
    /// Escribe un importe con su signo en un campo aparte: <paramref name="posSigno"/> recibe «N»
    /// (negativo) o espacio; el importe ocupa [<paramref name="desde"/>..<paramref name="hasta"/>]
    /// con 2 decimales implícitos (valor × 100, ceros a la izquierda).
    /// </summary>
    public RegistroAeat Importe(int posSigno, int desde, int hasta, decimal valor)
    {
        _buffer[posSigno - 1] = valor < 0m ? 'N' : ' ';
        var centimos = (long)Math.Round(Math.Abs(valor) * 100m, MidpointRounding.AwayFromZero);
        return Num(desde, hasta, centimos);
    }

    /// <summary>Fija un único carácter (constante) en una posición.</summary>
    public RegistroAeat Car(int posicion, char valor)
    {
        _buffer[posicion - 1] = valor;
        return this;
    }

    public override string ToString() => new(_buffer);

    private void Copiar(int desde, string texto)
    {
        for (var i = 0; i < texto.Length; i++)
        {
            _buffer[desde - 1 + i] = texto[i];
        }
    }

    /// <summary>
    /// Mayúsculas, sin vocales acentuadas (la Ñ se conserva), solo caracteres válidos en los ficheros
    /// AEAT. Usa un mapeo explícito de acentos (no depende de la normalización Unicode, que en modo
    /// «globalization-invariant» no descompone).
    /// </summary>
    internal static string NormalizarTexto(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var mayus = valor.Trim().ToUpperInvariant();
        var sb = new StringBuilder(mayus.Length);
        foreach (var c in mayus)
        {
            var s = Desacentuar(c);
            if (s == 'Ñ' || (s >= 'A' && s <= 'Z') || (s >= '0' && s <= '9') || s is ' ' or '-' or '.' or '/' or ',' or '&')
            {
                sb.Append(s);
            }
            // cualquier otro carácter (símbolos raros) se descarta
        }

        return sb.ToString();
    }

    private static char Desacentuar(char c) => c switch
    {
        'Á' or 'À' or 'Ä' or 'Â' or 'Ã' or 'Å' => 'A',
        'É' or 'È' or 'Ë' or 'Ê' => 'E',
        'Í' or 'Ì' or 'Ï' or 'Î' => 'I',
        'Ó' or 'Ò' or 'Ö' or 'Ô' or 'Õ' => 'O',
        'Ú' or 'Ù' or 'Ü' or 'Û' => 'U',
        'Ç' => 'C',
        _ => c,
    };

    /// <summary>Codifica en ISO-8859-1 el fichero completo (une los registros con CRLF).</summary>
    public static byte[] AFichero(IEnumerable<RegistroAeat> registros)
    {
        ArgumentNullException.ThrowIfNull(registros);
        var texto = string.Join("\r\n", registros.Select(r => r.ToString()));
        return Encoding.Latin1.GetBytes(texto);
    }
}

/// <summary>
/// Códigos INE de provincia (2 dígitos) para los ficheros de la AEAT, resueltos desde el nombre de
/// la provincia (sin distinguir acentos ni mayúsculas). «99» = no residente / desconocido.
/// </summary>
public static class ProvinciasAeat
{
    public const string NoResidenteODesconocida = "99";

    private static readonly Dictionary<string, string> PorNombre = Construir();

    /// <summary>Devuelve el código INE de la provincia, o «99» si no se reconoce.</summary>
    public static string Codigo(string? provincia)
    {
        if (string.IsNullOrWhiteSpace(provincia))
        {
            return NoResidenteODesconocida;
        }

        var clave = RegistroAeat.NormalizarTexto(provincia);
        return PorNombre.TryGetValue(clave, out var codigo) ? codigo : NoResidenteODesconocida;
    }

    private static Dictionary<string, string> Construir()
    {
        // Nombres ya normalizados (mayúsculas, sin acentos) para casar con RegistroAeat.NormalizarTexto.
        var pares = new (string Nombre, string Codigo)[]
        {
            ("ARABA", "01"), ("ALAVA", "01"), ("ALBACETE", "02"), ("ALICANTE", "03"), ("ALACANT", "03"),
            ("ALMERIA", "04"), ("AVILA", "05"), ("BADAJOZ", "06"), ("ILLES BALEARS", "07"), ("BALEARES", "07"),
            ("BARCELONA", "08"), ("BURGOS", "09"), ("CACERES", "10"), ("CADIZ", "11"), ("CASTELLON", "12"),
            ("CASTELLO", "12"), ("CIUDAD REAL", "13"), ("CORDOBA", "14"), ("A CORUNA", "15"), ("LA CORUNA", "15"),
            ("CUENCA", "16"), ("GIRONA", "17"), ("GERONA", "17"), ("GRANADA", "18"), ("GUADALAJARA", "19"),
            ("GIPUZKOA", "20"), ("GUIPUZCOA", "20"), ("HUELVA", "21"), ("HUESCA", "22"), ("JAEN", "23"),
            ("LEON", "24"), ("LLEIDA", "25"), ("LERIDA", "25"), ("LA RIOJA", "26"), ("RIOJA", "26"),
            ("LUGO", "27"), ("MADRID", "28"), ("MALAGA", "29"), ("MURCIA", "30"), ("NAVARRA", "31"),
            ("NAFARROA", "31"), ("OURENSE", "32"), ("ORENSE", "32"), ("ASTURIAS", "33"), ("PALENCIA", "34"),
            ("LAS PALMAS", "35"), ("PONTEVEDRA", "36"), ("SALAMANCA", "37"), ("SANTA CRUZ DE TENERIFE", "38"),
            ("TENERIFE", "38"), ("CANTABRIA", "39"), ("SEGOVIA", "40"), ("SEVILLA", "41"), ("SORIA", "42"),
            ("TARRAGONA", "43"), ("TERUEL", "44"), ("TOLEDO", "45"), ("VALENCIA", "46"), ("VALENCIA/VALENCIA", "46"),
            ("VALLADOLID", "47"), ("BIZKAIA", "48"), ("VIZCAYA", "48"), ("ZAMORA", "49"), ("ZARAGOZA", "50"),
            ("CEUTA", "51"), ("MELILLA", "52"),
        };

        var mapa = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (nombre, codigo) in pares)
        {
            mapa[nombre] = codigo;
        }

        return mapa;
    }
}
