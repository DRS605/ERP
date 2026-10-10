using System.Globalization;

namespace AlxorCore.Api.Comun;

/// <summary>Cuerpo de una importación CSV: el contenido y si es solo previsualización.</summary>
public sealed record ImportarCsvPeticion(string Contenido, bool Previsualizar = true);

/// <summary>Utilidades de conversión de valores CSV (números, IVA, tipo de producto).</summary>
public static class ImportacionCsv
{
    /// <summary>Interpreta un número admitiendo coma o punto decimal y símbolos como € o %.</summary>
    public static decimal Numero(string? valor, decimal porDefecto = 0m)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return porDefecto;
        }

        var limpio = valor.Replace("€", string.Empty, StringComparison.Ordinal)
            .Replace("%", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal) // espacio duro
            .Trim();
        if (limpio.Length == 0)
        {
            return porDefecto;
        }

        // Normaliza coma/punto admitiendo ambas convenciones (español «1.234,56» e invariante «1234.56»,
        // como la que guarda Excel en las celdas numéricas: siempre punto decimal). Importante: NO se puede
        // suponer que el punto es siempre separador de millares, o un coste de 0.80 € se leería como 80.
        var puntos = limpio.Count(c => c == '.');
        var comas = limpio.Count(c => c == ',');
        if (puntos > 0 && comas > 0)
        {
            // Mixto: el último separador que aparece es el decimal; el otro, de millares.
            if (limpio.LastIndexOf(',') > limpio.LastIndexOf('.'))
            {
                limpio = limpio.Replace(".", string.Empty, StringComparison.Ordinal).Replace(",", ".", StringComparison.Ordinal);
            }
            else
            {
                limpio = limpio.Replace(",", string.Empty, StringComparison.Ordinal);
            }
        }
        else if (comas > 0)
        {
            // Una sola coma → decimal; varias → separadores de millares.
            limpio = comas == 1
                ? limpio.Replace(",", ".", StringComparison.Ordinal)
                : limpio.Replace(",", string.Empty, StringComparison.Ordinal);
        }
        else if (puntos > 1)
        {
            // Varios puntos → separadores de millares (p. ej. «1.234.567»).
            limpio = limpio.Replace(".", string.Empty, StringComparison.Ordinal);
        }
        // Un solo punto se deja como decimal invariante (p. ej. «0.80», «1234.56»).

        return decimal.TryParse(limpio, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : porDefecto;
    }

    /// <summary>Normaliza un valor de IVA (21, "21%", "IVA21") al código del catálogo (IVA21…).</summary>
    public static string? CodigoIva(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var v = valor.Trim().ToUpperInvariant().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (v.StartsWith("IVA", StringComparison.Ordinal))
        {
            return v;
        }

        var porcentaje = Numero(v);
        return porcentaje switch
        {
            21m => "IVA21",
            10m => "IVA10",
            4m => "IVA4",
            0m => "IVA0",
            _ => "IVA21",
        };
    }
}
