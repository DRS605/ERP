using AlxorCore.Identidad.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Identidad.Tests.Dominio;

public class TotpTests
{
    private static readonly DateTimeOffset Instante = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void El_codigo_calculado_es_de_seis_digitos_y_se_verifica()
    {
        var secreto = Totp.GenerarSecreto();
        var codigo = Totp.Calcular(secreto, Instante);

        codigo.Should().MatchRegex("^[0-9]{6}$");
        Totp.Verificar(secreto, codigo, Instante).Should().BeTrue();
    }

    [Fact]
    public void Un_codigo_de_otro_secreto_no_verifica()
    {
        var a = Totp.GenerarSecreto();
        var b = Totp.GenerarSecreto();
        Totp.Verificar(b, Totp.Calcular(a, Instante), Instante).Should().BeFalse();
    }

    [Fact]
    public void Tolera_el_desfase_de_reloj_dentro_de_la_ventana()
    {
        var secreto = Totp.GenerarSecreto();
        var codigo = Totp.Calcular(secreto, Instante);

        // ±30 s (un paso) entra en la ventana; fuera de ±ventana, no.
        Totp.Verificar(secreto, codigo, Instante.AddSeconds(30)).Should().BeTrue();
        Totp.Verificar(secreto, codigo, Instante.AddSeconds(-30)).Should().BeTrue();
        Totp.Verificar(secreto, codigo, Instante.AddMinutes(5)).Should().BeFalse();
    }

    [Fact]
    public void Rechaza_codigos_mal_formados()
    {
        var secreto = Totp.GenerarSecreto();
        Totp.Verificar(secreto, null, Instante).Should().BeFalse();
        Totp.Verificar(secreto, "abc", Instante).Should().BeFalse();
        Totp.Verificar(secreto, "12345", Instante).Should().BeFalse();
    }

    [Fact]
    public void El_uri_otpauth_incluye_secreto_y_emisor()
    {
        var uri = Totp.ConstruirUri("ABCDEFGH", "ALXOR Core", "ana@ejemplo.com");
        uri.Should().StartWith("otpauth://totp/");
        uri.Should().Contain("secret=ABCDEFGH");
        uri.Should().Contain("issuer=ALXOR%20Core");
    }
}
