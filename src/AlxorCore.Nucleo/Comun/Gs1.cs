using System.Globalization;
using System.Text;

namespace AlxorCore.Nucleo.Comun;

/// <summary>Datos de una lectura GS1 (código de barras GS1-128, DataMatrix o QR GS1) ya separados por identificador de aplicación.</summary>
public sealed record LecturaGs1(IReadOnlyDictionary<string, string> Elementos)
{
    /// <summary>SSCC (IA 00): código de la unidad logística.</summary>
    public string? Sscc => Elementos.GetValueOrDefault("00");

    /// <summary>GTIN del artículo (IA 01) o del contenido de la unidad (IA 02).</summary>
    public string? Gtin => Elementos.GetValueOrDefault("01") ?? Elementos.GetValueOrDefault("02");

    /// <summary>Lote (IA 10).</summary>
    public string? Lote => Elementos.GetValueOrDefault("10");

    /// <summary>Número de serie (IA 21).</summary>
    public string? Serie => Elementos.GetValueOrDefault("21");

    /// <summary>Fecha de caducidad (IA 17) o de consumo preferente (IA 15).</summary>
    public DateOnly? Caducidad => Gs1.Fecha(Elementos.GetValueOrDefault("17") ?? Elementos.GetValueOrDefault("15"));

    /// <summary>Fecha de fabricación (IA 11) o de envasado (IA 13).</summary>
    public DateOnly? Fabricacion => Gs1.Fecha(Elementos.GetValueOrDefault("11") ?? Elementos.GetValueOrDefault("13"));

    /// <summary>Unidades de comercio contenidas (IA 37) o cantidad variable (IA 30).</summary>
    public int? Cantidad => int.TryParse(Elementos.GetValueOrDefault("37") ?? Elementos.GetValueOrDefault("30"), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : null;

    /// <summary>Peso neto en kg (IA 310n, con n decimales).</summary>
    public decimal? PesoNetoKg => Gs1.Medida(Elementos, "310");

    /// <summary>Peso bruto logístico en kg (IA 330n).</summary>
    public decimal? PesoBrutoKg => Gs1.Medida(Elementos, "330");

    /// <summary>Número de pedido del cliente (IA 400).</summary>
    public string? PedidoCliente => Elementos.GetValueOrDefault("400");
}

/// <summary>
/// Reglas de los códigos GS1: dígito de control (GTIN, GLN, SSCC), composición del SSCC y lectura de lo que envía un
/// escáner. La lectura admite el texto con paréntesis («(00)384…(10)L1»), el de una pistola o una cámara con el
/// separador FNC1 (GS, carácter 29) o su identificador de simbología («]C1», «]d2», «]Q3») y, si no hay separadores,
/// los IA de longitud fija seguidos de uno variable al final.
/// </summary>
public static class Gs1
{
    public const char Separador = '\u001d';

    /// <summary>Longitud fija de los IA que la tienen (sin contar el IA).</summary>
    private static readonly Dictionary<string, int> Fijos = new(StringComparer.Ordinal)
    {
        ["00"] = 18, ["01"] = 14, ["02"] = 14, ["11"] = 6, ["12"] = 6, ["13"] = 6, ["15"] = 6, ["16"] = 6, ["17"] = 6, ["20"] = 2,
    };

    /// <summary>Longitud máxima de los IA variables usuales.</summary>
    private static readonly Dictionary<string, int> Variables = new(StringComparer.Ordinal)
    {
        ["10"] = 20, ["21"] = 20, ["22"] = 20, ["30"] = 8, ["37"] = 8, ["240"] = 30, ["241"] = 30, ["250"] = 30, ["251"] = 30,
        ["400"] = 30, ["401"] = 30, ["402"] = 17, ["403"] = 30, ["410"] = 13, ["413"] = 13, ["414"] = 13, ["420"] = 20, ["422"] = 3,
    };

    /// <summary>Dígito de control GS1 (módulo 10 con pesos 3 y 1 desde la derecha) del cuerpo sin él.</summary>
    public static int DigitoControl(string cuerpo)
    {
        ArgumentNullException.ThrowIfNull(cuerpo);
        var suma = 0;
        for (var i = 0; i < cuerpo.Length; i++)
        {
            var d = cuerpo[cuerpo.Length - 1 - i] - '0';
            suma += i % 2 == 0 ? d * 3 : d;
        }

        return (10 - (suma % 10)) % 10;
    }

    private static bool ConControl(string? codigo, params int[] longitudes) =>
        codigo is not null && longitudes.Contains(codigo.Length) && codigo.All(char.IsAsciiDigit) && DigitoControl(codigo[..^1]) == codigo[^1] - '0';

    /// <summary>GTIN de 8, 12, 13 o 14 dígitos con su dígito de control.</summary>
    public static bool EsGtinValido(string? gtin) => ConControl(gtin, 8, 12, 13, 14);

    public static bool EsSsccValido(string? sscc) => ConControl(sscc, 18);

    /// <summary>GTIN normalizado a 14 dígitos (con ceros a la izquierda), para comparar un EAN-13 con un GTIN-14.</summary>
    public static string? Gtin14(string? gtin) => EsGtinValido(gtin) ? gtin!.PadLeft(14, '0') : null;

    /// <summary>SSCC: extensión + prefijo de empresa + serie (hasta completar 17 dígitos) + control. Null si la serie no cabe.</summary>
    public static string? Sscc(int extension, string prefijo, long serie)
    {
        ArgumentNullException.ThrowIfNull(prefijo);
        var digitos = 16 - prefijo.Length;
        if (extension is < 0 or > 9 || digitos < 1 || serie < 1 || serie > (long)Math.Pow(10, digitos) - 1)
        {
            return null;
        }

        var cuerpo = extension.ToString(CultureInfo.InvariantCulture) + prefijo + serie.ToString(CultureInfo.InvariantCulture).PadLeft(digitos, '0');
        return cuerpo + DigitoControl(cuerpo).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Separa una lectura en sus IA. Un código de 18 dígitos válido se toma como SSCC y uno de 8, 12, 13 o 14 como GTIN
    /// (lo que lee una pistola de un EAN-13 o un ITF-14). Si no se entiende, devuelve null.
    /// </summary>
    public static LecturaGs1? Leer(string? lectura)
    {
        if (string.IsNullOrWhiteSpace(lectura))
        {
            return null;
        }

        var t = lectura.Trim().Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", string.Empty, StringComparison.Ordinal);
        if (t.Length > 3 && t[0] == ']')
        {
            t = t[3..];
        }

        // Algunas pistolas envían el FNC1 como «<GS>», «{GS}» o «~1»; y hay quien teclea el texto legible.
        t = t.Replace("<GS>", Separador.ToString(), StringComparison.OrdinalIgnoreCase).Replace("{GS}", Separador.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("~1", Separador.ToString(), StringComparison.Ordinal);
        var elementos = new Dictionary<string, string>(StringComparer.Ordinal);
        if (t.Contains('(', StringComparison.Ordinal))
        {
            foreach (var trozo in t.Split('(', StringSplitOptions.RemoveEmptyEntries))
            {
                var fin = trozo.IndexOf(')', StringComparison.Ordinal);
                if (fin < 2)
                {
                    return null;
                }

                elementos[trozo[..fin]] = trozo[(fin + 1)..].Trim().TrimEnd(Separador);
            }

            return Validar(elementos);
        }

        var solo = t.Trim(Separador);
        if (solo.All(char.IsAsciiDigit) && !solo.Contains(Separador, StringComparison.Ordinal))
        {
            if (solo.Length == 18 && EsSsccValido(solo))
            {
                return new LecturaGs1(new Dictionary<string, string>(StringComparer.Ordinal) { ["00"] = solo });
            }

            if (solo.Length is 8 or 12 or 13 or 14 && EsGtinValido(solo))
            {
                return new LecturaGs1(new Dictionary<string, string>(StringComparer.Ordinal) { ["01"] = solo.PadLeft(14, '0') });
            }
        }

        var i = 0;
        while (i < solo.Length)
        {
            if (solo[i] == Separador)
            {
                i++;
                continue;
            }

            var (ia, longitud) = IaEn(solo, i);
            if (ia is null)
            {
                return null;
            }

            i += ia.Length;
            string valor;
            if (longitud is { } fija)
            {
                if (i + fija > solo.Length)
                {
                    return null;
                }

                valor = solo.Substring(i, fija);
                i += fija;
            }
            else
            {
                var fin = solo.IndexOf(Separador, i);
                var maximo = Variables.GetValueOrDefault(ia, 30);
                fin = fin < 0 ? solo.Length : fin;
                valor = solo[i..Math.Min(fin, i + maximo)];
                i += valor.Length;
            }

            elementos[ia] = valor;
        }

        return Validar(elementos);
    }

    private static (string? Ia, int? Longitud) IaEn(string t, int i)
    {
        if (i + 2 > t.Length)
        {
            return (null, null);
        }

        var dos = t.Substring(i, 2);
        if (Fijos.TryGetValue(dos, out var f))
        {
            return (dos, f);
        }

        if (dos is "10" or "21" or "22" or "30" or "37")
        {
            return (dos, null);
        }

        // 31nn-36nn: medidas de 6 dígitos con el número de decimales en el cuarto dígito del IA.
        if (i + 4 <= t.Length && dos is "31" or "32" or "33" or "34" or "35" or "36")
        {
            return (t.Substring(i, 4), 6);
        }

        if (i + 3 <= t.Length && Variables.ContainsKey(t.Substring(i, 3)))
        {
            return (t.Substring(i, 3), null);
        }

        return (null, null);
    }

    private static LecturaGs1? Validar(Dictionary<string, string> elementos)
    {
        if (elementos.Count == 0)
        {
            return null;
        }

        if (elementos.TryGetValue("00", out var sscc) && !EsSsccValido(sscc))
        {
            return null;
        }

        foreach (var ia in new[] { "01", "02" })
        {
            if (elementos.TryGetValue(ia, out var gtin))
            {
                if (!EsGtinValido(gtin))
                {
                    return null;
                }

                elementos[ia] = gtin.PadLeft(14, '0');
            }
        }

        return new LecturaGs1(elementos);
    }

    /// <summary>Fecha GS1 «AAMMDD» (siglo según la regla GS1 de ±50 años; día 00 = último del mes).</summary>
    public static DateOnly? Fecha(string? aammdd)
    {
        if (aammdd is not { Length: 6 } || !aammdd.All(char.IsAsciiDigit))
        {
            return null;
        }

        var aa = int.Parse(aammdd[..2], CultureInfo.InvariantCulture);
        var mm = int.Parse(aammdd[2..4], CultureInfo.InvariantCulture);
        var dd = int.Parse(aammdd[4..], CultureInfo.InvariantCulture);
        var anio = 2000 + aa;
        if (mm is < 1 or > 12)
        {
            return null;
        }

        dd = dd == 0 ? DateTime.DaysInMonth(anio, mm) : dd;
        return dd <= DateTime.DaysInMonth(anio, mm) ? new DateOnly(anio, mm, dd) : null;
    }

    /// <summary>Fecha a «AAMMDD».</summary>
    public static string Aammdd(DateOnly fecha) => fecha.ToString("yyMMdd", CultureInfo.InvariantCulture);

    /// <summary>Medida 31nn-36nn del prefijo dado (p. ej. «310» peso neto kg): el último dígito del IA son los decimales.</summary>
    public static decimal? Medida(IReadOnlyDictionary<string, string> elementos, string prefijo)
    {
        ArgumentNullException.ThrowIfNull(elementos);
        foreach (var (ia, valor) in elementos)
        {
            if (ia.Length == 4 && ia.StartsWith(prefijo, StringComparison.Ordinal) && long.TryParse(valor, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            {
                return n / (decimal)Math.Pow(10, ia[3] - '0');
            }
        }

        return null;
    }

    /// <summary>Peso en kg a su IA 310n/330n con 3 decimales (hasta 999,999 kg) o menos si no cabe.</summary>
    public static (string Ia, string Valor) Peso(string prefijo, decimal kg)
    {
        for (var decimales = 3; decimales >= 0; decimales--)
        {
            var n = decimal.Round(kg * (decimal)Math.Pow(10, decimales), 0, MidpointRounding.AwayFromZero);
            if (n <= 999_999m)
            {
                return (prefijo + decimales.ToString(CultureInfo.InvariantCulture), ((long)n).ToString("D6", CultureInfo.InvariantCulture));
            }
        }

        return (prefijo + "0", "999999");
    }

    /// <summary>Texto legible de unos elementos: «(00) 384… (10) L1».</summary>
    public static string Legible(IEnumerable<(string Ia, string Valor)> elementos) =>
        string.Join(' ', (elementos ?? []).Select(e => $"({e.Ia}) {e.Valor}"));

    /// <summary>Elementos a texto de código con FNC1 tras los variables que no van al final.</summary>
    public static string Codificar(IReadOnlyList<(string Ia, string Valor)> elementos)
    {
        ArgumentNullException.ThrowIfNull(elementos);
        var sb = new StringBuilder();
        for (var i = 0; i < elementos.Count; i++)
        {
            var (ia, valor) = elementos[i];
            sb.Append(ia).Append(valor);
            var fija = Fijos.ContainsKey(ia) || (ia.Length == 4 && ia[0] == '3' && ia[1] is >= '1' and <= '6');
            if (i < elementos.Count - 1 && !fija)
            {
                sb.Append(Separador);
            }
        }

        return sb.ToString();
    }
}
