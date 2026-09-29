using System.Globalization;
using System.Text.RegularExpressions;

namespace AlxorCore.AgenteBascula;

/// <summary>Peso leído del indicador: kilos (con signo), si es estable y la trama original.</summary>
public sealed record Pesada(decimal Kilos, bool Estable, string Trama, DateTimeOffset LeidaEn);

/// <summary>
/// Interpreta las tramas que envían los indicadores de báscula (salida continua o bajo petición). Admite los formatos
/// habituales:
/// <list type="bullet">
/// <item>Dini Argeo, A&amp;D, Gram y compatibles: <c>ST,GS,+0001234kg</c> (<c>US</c> = inestable, <c>OL</c> = sobrecarga);</item>
/// <item>Mettler Toledo SICS: <c>S S     12.345 kg</c> (<c>S D</c> = dinámico, inestable);</item>
/// <item>Epelsa, Baxtran y genéricos: el número con su unidad (<c>kg</c>, <c>g</c> o <c>t</c>), estable si no se marca lo
/// contrario.</item>
/// </list>
/// Devuelve null si la trama no trae un peso (o marca sobrecarga o error).
/// </summary>
public static partial class LecturaBascula
{
    [GeneratedRegex(@"(?<signo>[+-])?\s*(?<num>\d+(?:[.,]\d+)?)\s*(?<unidad>kg|g|t)?\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Numero();

    public static Pesada? Interpretar(string? trama, DateTimeOffset ahora)
    {
        var t = (trama ?? string.Empty).Replace("\u0002", string.Empty, StringComparison.Ordinal).Replace("\u0003", string.Empty, StringComparison.Ordinal).Trim();
        if (t.Length == 0)
        {
            return null;
        }

        var mayus = t.ToUpperInvariant();
        if (mayus.StartsWith("OL", StringComparison.Ordinal) || mayus.Contains(",OL", StringComparison.Ordinal) || mayus.StartsWith("S +", StringComparison.Ordinal)
            || mayus.StartsWith("S -", StringComparison.Ordinal) || mayus.StartsWith("S I", StringComparison.Ordinal) || mayus.Contains("ERR", StringComparison.Ordinal))
        {
            return null;
        }

        var inestable = mayus.StartsWith("US", StringComparison.Ordinal) || mayus.Contains(",US", StringComparison.Ordinal) || mayus.StartsWith("S D", StringComparison.Ordinal)
            || mayus.Contains(" MO", StringComparison.Ordinal) || mayus.StartsWith("MO", StringComparison.Ordinal);

        // El peso es el último número con unidad; si no hay unidad, el último número.
        var numeros = Numero().Matches(mayus.StartsWith("S S", StringComparison.Ordinal) ? t[3..] : t).Where(m => m.Groups["num"].Success).ToList();
        if (numeros.Count == 0)
        {
            return null;
        }

        var elegido = numeros.LastOrDefault(m => m.Groups["unidad"].Success) ?? numeros[^1];
        if (!decimal.TryParse(elegido.Groups["num"].Value.Replace(',', '.'), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var valor))
        {
            return null;
        }

        if (elegido.Groups["signo"].Value == "-")
        {
            valor = -valor;
        }

        var kilos = elegido.Groups["unidad"].Value.ToUpperInvariant() switch
        {
            "G" => valor / 1000m,
            "T" => valor * 1000m,
            _ => valor,
        };
        return new Pesada(decimal.Round(kilos, 3), !inestable, t, ahora);
    }
}
