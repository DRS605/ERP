using System.Globalization;
using System.Text;

namespace AlxorCore.Documentos.Aplicacion;

/// <summary>
/// Codificador GS1-128 (Code 128 con FNC1 inicial) para las etiquetas logísticas. Codifica los números en el juego C
/// (dos dígitos por símbolo) y el texto en el juego B, y devuelve las anchuras de barras y espacios en módulos.
/// Los identificadores de aplicación (IA) de longitud variable que no van al final se separan con FNC1.
/// </summary>
public static class Gs1128
{
    /// <summary>Marca de separador FNC1 dentro del texto a codificar.</summary>
    public const char Fnc1 = '\u001d';

    private const int CodigoB = 100;
    private const int CodigoC = 99;
    private const int ValorFnc1 = 102;
    private const int InicioC = 105;
    private const int Parada = 106;

    /// <summary>Patrones Code 128 (anchuras barra-espacio alternas, en módulos) de los valores 0 a 106.</summary>
    public static IReadOnlyList<string> Patrones { get; } =
    [
        "212222", "222122", "222221", "121223", "121322", "131222", "122213", "122312", "132212", "221213",
        "221312", "231212", "112232", "122132", "122231", "113222", "123122", "123221", "223211", "221132",
        "221231", "213212", "223112", "312131", "311222", "321122", "321221", "312212", "322112", "322211",
        "212123", "212321", "232121", "111323", "131123", "131321", "112313", "132113", "132311", "211313",
        "231113", "231311", "112133", "112331", "132131", "113123", "113321", "133121", "313121", "211331",
        "231131", "213113", "213311", "213131", "311123", "311321", "331121", "312113", "312311", "332111",
        "314111", "221411", "431111", "111224", "111422", "121124", "121421", "141122", "141221", "112214",
        "112412", "122114", "122411", "142112", "142211", "241211", "221114", "413111", "241112", "134111",
        "111242", "121142", "121241", "114212", "124112", "124211", "411212", "421112", "421211", "212141",
        "214121", "412121", "111143", "111341", "131141", "114113", "114311", "411113", "411311", "113141",
        "114131", "311141", "411131", "211412", "211214", "211232", "2331112",
    ];

    /// <summary>
    /// Elementos (IA, valor) a texto GS1: los de longitud variable llevan FNC1 detrás salvo el último. Los IA de longitud
    /// fija que se usan en las etiquetas son 00, 01, 02, 11-17, 20 y 31xx-36xx.
    /// </summary>
    public static string Componer(IReadOnlyList<(string Ia, string Valor)> elementos)
    {
        ArgumentNullException.ThrowIfNull(elementos);
        var sb = new StringBuilder();
        for (var i = 0; i < elementos.Count; i++)
        {
            var (ia, valor) = elementos[i];
            sb.Append(ia).Append(valor);
            if (i < elementos.Count - 1 && !LongitudFija(ia))
            {
                sb.Append(Fnc1);
            }
        }

        return sb.ToString();
    }

    /// <summary>Texto legible: «(00) 384000000000000015 (37) 80».</summary>
    public static string Legible(IReadOnlyList<(string Ia, string Valor)> elementos)
    {
        ArgumentNullException.ThrowIfNull(elementos);
        return string.Join(" ", elementos.Select(e => $"({e.Ia}) {e.Valor}"));
    }

    private static bool LongitudFija(string ia) =>
        ia is "00" or "01" or "02" or "11" or "12" or "13" or "15" or "16" or "17" or "20"
        || (ia.Length == 4 && ia[0] == '3' && ia[1] is >= '1' and <= '6');

    /// <summary>Valores Code 128 del texto GS1 (inicio C, FNC1, datos, control y parada).</summary>
    public static IReadOnlyList<int> Valores(string datos)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var valores = new List<int> { InicioC, ValorFnc1 };
        var juegoC = true;
        var i = 0;
        while (i < datos.Length)
        {
            var c = datos[i];
            if (c == Fnc1)
            {
                valores.Add(ValorFnc1);
                i++;
                continue;
            }

            var digitos = Digitos(datos, i);
            if (juegoC)
            {
                if (digitos >= 2)
                {
                    valores.Add(((datos[i] - '0') * 10) + (datos[i + 1] - '0'));
                    i += 2;
                    continue;
                }

                valores.Add(CodigoB);
                juegoC = false;
            }
            else if (digitos >= 4 && digitos % 2 == 0)
            {
                valores.Add(CodigoC);
                juegoC = true;
                continue;
            }

            if (c is < ' ' or > '~')
            {
                throw new ArgumentException($"Carácter no codificable en GS1-128: U+{(int)c:X4}.", nameof(datos));
            }

            valores.Add(c - ' ');
            i++;
        }

        var suma = valores[0];
        for (var k = 1; k < valores.Count; k++)
        {
            suma += valores[k] * k;
        }

        valores.Add(suma % 103);
        valores.Add(Parada);
        return valores;
    }

    /// <summary>Anchuras en módulos (barra, espacio, barra…) del código, sin zonas de silencio.</summary>
    public static IReadOnlyList<int> Anchuras(string datos) =>
        Valores(datos).SelectMany(v => Patrones[v].Select(ch => ch - '0')).ToList();

    /// <summary>Peso neto como IA 310n: kilos con los decimales que quepan en 6 dígitos (n = 0 a 3).</summary>
    public static (string Ia, string Valor) PesoNeto(decimal kilos)
    {
        for (var n = 3; n >= 0; n--)
        {
            var valor = decimal.Round(kilos * (decimal)Math.Pow(10, n), 0, MidpointRounding.AwayFromZero);
            if (valor < 1_000_000m)
            {
                return ("310" + n.ToString(CultureInfo.InvariantCulture), ((long)valor).ToString("D6", CultureInfo.InvariantCulture));
            }
        }

        throw new ArgumentOutOfRangeException(nameof(kilos), kilos, "El peso no cabe en el IA 3100.");
    }

    private static int Digitos(string s, int desde)
    {
        var n = 0;
        while (desde + n < s.Length && char.IsAsciiDigit(s[desde + n]))
        {
            n++;
        }

        return n;
    }
}
