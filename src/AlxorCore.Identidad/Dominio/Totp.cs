using System.Security.Cryptography;
using System.Text;

namespace AlxorCore.Identidad.Dominio;

/// <summary>
/// Implementación de contraseñas de un solo uso basadas en tiempo (<b>TOTP</b>, RFC 6238) con
/// HMAC-SHA1, pasos de 30 s y códigos de 6 dígitos. Es lo que usan Google Authenticator, Authy,
/// 1Password, etc. El secreto se codifica en <b>Base32</b> (RFC 4648) para el URI <c>otpauth://</c>.
/// </summary>
public static class Totp
{
    private const int Digitos = 6;
    private const int PasoSegundos = 30;
    private const string AlfabetoBase32 = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    /// <summary>Genera un secreto aleatorio de 20 bytes codificado en Base32 (longitud recomendada para SHA-1).</summary>
    public static string GenerarSecreto()
    {
        return CodificarBase32(RandomNumberGenerator.GetBytes(20));
    }

    /// <summary>Construye el URI <c>otpauth://totp/...</c> que la app de autenticación lee (por QR o a mano).</summary>
    public static string ConstruirUri(string secreto, string emisor, string cuenta)
    {
        var etiqueta = Uri.EscapeDataString($"{emisor}:{cuenta}");
        var e = Uri.EscapeDataString(emisor);
        return $"otpauth://totp/{etiqueta}?secret={secreto}&issuer={e}&algorithm=SHA1&digits={Digitos}&period={PasoSegundos}";
    }

    /// <summary>Calcula el código TOTP para un instante dado.</summary>
    public static string Calcular(string secreto, DateTimeOffset instante)
    {
        var contador = instante.ToUnixTimeSeconds() / PasoSegundos;
        return CalcularParaContador(secreto, contador);
    }

    /// <summary>
    /// Verifica un código contra el secreto admitiendo una <paramref name="ventana"/> de pasos a cada
    /// lado (por defecto ±1, ~90 s) para tolerar el desfase de reloj entre el servidor y el móvil.
    /// </summary>
    public static bool Verificar(string secreto, string? codigo, DateTimeOffset ahora, int ventana = 1)
    {
        if (string.IsNullOrWhiteSpace(secreto) || string.IsNullOrWhiteSpace(codigo))
        {
            return false;
        }

        var limpio = codigo.Trim().Replace(" ", string.Empty, StringComparison.Ordinal);
        if (limpio.Length != Digitos || !limpio.All(char.IsDigit))
        {
            return false;
        }

        var contador = ahora.ToUnixTimeSeconds() / PasoSegundos;
        for (var i = -ventana; i <= ventana; i++)
        {
            var esperado = CalcularParaContador(secreto, contador + i);
            if (CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(esperado), Encoding.ASCII.GetBytes(limpio)))
            {
                return true;
            }
        }

        return false;
    }

    private static string CalcularParaContador(string secreto, long contador)
    {
        var clave = DecodificarBase32(secreto);
        var buffer = new byte[8];
        for (var i = 7; i >= 0; i--)
        {
            buffer[i] = (byte)(contador & 0xFF);
            contador >>= 8;
        }

        // HMAC-SHA1 es el algoritmo estándar de TOTP (RFC 6238) y el que usan las apps de
        // autenticación (Google Authenticator, Authy…). Su uso aquí es obligatorio por interoperabilidad,
        // no una elección criptográfica débil para proteger datos.
#pragma warning disable CA5350
        using var hmac = new HMACSHA1(clave);
#pragma warning restore CA5350
        var hash = hmac.ComputeHash(buffer);
        var desplazamiento = hash[^1] & 0x0F;
        var binario = ((hash[desplazamiento] & 0x7F) << 24)
            | ((hash[desplazamiento + 1] & 0xFF) << 16)
            | ((hash[desplazamiento + 2] & 0xFF) << 8)
            | (hash[desplazamiento + 3] & 0xFF);
        var codigo = binario % (int)Math.Pow(10, Digitos);
        return codigo.ToString().PadLeft(Digitos, '0');
    }

    private static string CodificarBase32(byte[] datos)
    {
        var sb = new StringBuilder();
        int buffer = 0, bits = 0;
        foreach (var b in datos)
        {
            buffer = (buffer << 8) | b;
            bits += 8;
            while (bits >= 5)
            {
                bits -= 5;
                sb.Append(AlfabetoBase32[(buffer >> bits) & 0x1F]);
            }
        }

        if (bits > 0)
        {
            sb.Append(AlfabetoBase32[(buffer << (5 - bits)) & 0x1F]);
        }

        return sb.ToString();
    }

    private static byte[] DecodificarBase32(string base32)
    {
        var limpio = base32.TrimEnd('=').ToUpperInvariant();
        var salida = new List<byte>(limpio.Length * 5 / 8);
        int buffer = 0, bits = 0;
        foreach (var c in limpio)
        {
            var valor = AlfabetoBase32.IndexOf(c);
            if (valor < 0)
            {
                continue;
            }

            buffer = (buffer << 5) | valor;
            bits += 5;
            if (bits >= 8)
            {
                bits -= 8;
                salida.Add((byte)((buffer >> bits) & 0xFF));
            }
        }

        return salida.ToArray();
    }
}
