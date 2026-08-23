using System.Net;
using System.Net.Http.Json;
using AlxorCore.Identidad.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de seguridad y operación: health checks, cabeceras, correlación y 2FA.</summary>
public sealed class SeguridadEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SeguridadEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record LoginRespuesta(string Token, bool Requiere2fa);
    private sealed record Preparacion(string Secreto, string UriOtpauth);
    private sealed record Activacion(string[] CodigosRecuperacion);

    [Fact]
    public async Task Health_liveness_y_readiness_responden()
    {
        var cli = _fabrica.CreateClient();
        (await cli.GetAsync(new Uri("/salud/vivo", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await cli.GetAsync(new Uri("/salud/listo", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Las_respuestas_llevan_cabeceras_de_seguridad()
    {
        var cli = _fabrica.CreateClient();
        var r = await cli.GetAsync(new Uri("/salud", UriKind.Relative));
        r.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        r.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        r.Headers.Contains("Referrer-Policy").Should().BeTrue();
    }

    [Fact]
    public async Task El_id_de_correlacion_se_devuelve_y_se_respeta_el_entrante()
    {
        var cli = _fabrica.CreateClient();

        var generado = await cli.GetAsync(new Uri("/salud", UriKind.Relative));
        generado.Headers.Contains("X-Correlation-Id").Should().BeTrue();

        using var peticion = new HttpRequestMessage(HttpMethod.Get, "/salud");
        peticion.Headers.Add("X-Correlation-Id", "correlacion-de-prueba");
        var eco = await cli.SendAsync(peticion);
        eco.Headers.GetValues("X-Correlation-Id").Should().Contain("correlacion-de-prueba");
    }

    [Fact]
    public async Task Flujo_completo_de_verificacion_en_dos_pasos()
    {
        var cli = _fabrica.CreateClient();
        var email = Ayudas.EmailUnico();
        await cli.PostAsJsonAsync("/auth/registro", new { Email = email, Nombre = "Ana", Contrasena = "contrasena123" });
        var login = await (await cli.PostAsJsonAsync("/auth/login", new { Email = email, Contrasena = "contrasena123" })).Content.ReadFromJsonAsync<LoginRespuesta>();
        cli.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", login!.Token);

        // Preparar y activar el 2FA calculando un TOTP válido con el secreto entregado.
        var prep = await (await cli.PostAsync(new Uri("/auth/2fa/preparar", UriKind.Relative), null)).Content.ReadFromJsonAsync<Preparacion>();
        var codigo = Totp.Calcular(prep!.Secreto, DateTimeOffset.UtcNow);
        var activacion = await cli.PostAsJsonAsync("/auth/2fa/activar", new { Codigo = codigo });
        activacion.StatusCode.Should().Be(HttpStatusCode.OK);
        var codigos = (await activacion.Content.ReadFromJsonAsync<Activacion>())!.CodigosRecuperacion;
        codigos.Should().HaveCount(8);

        // Un nuevo login ahora exige el segundo factor.
        var anon = _fabrica.CreateClient();
        var sinCodigo = await (await anon.PostAsJsonAsync("/auth/login", new { Email = email, Contrasena = "contrasena123" })).Content.ReadFromJsonAsync<LoginRespuesta>();
        sinCodigo!.Requiere2fa.Should().BeTrue();
        sinCodigo.Token.Should().BeEmpty();

        // Con un TOTP válido se completa el login.
        var conCodigo = await (await anon.PostAsJsonAsync("/auth/login", new { Email = email, Contrasena = "contrasena123", Codigo = Totp.Calcular(prep.Secreto, DateTimeOffset.UtcNow) }))
            .Content.ReadFromJsonAsync<LoginRespuesta>();
        conCodigo!.Requiere2fa.Should().BeFalse();
        conCodigo.Token.Should().NotBeEmpty();

        // Un código de recuperación también sirve como segundo factor.
        var conRecuperacion = await (await anon.PostAsJsonAsync("/auth/login", new { Email = email, Contrasena = "contrasena123", Codigo = codigos[0] }))
            .Content.ReadFromJsonAsync<LoginRespuesta>();
        conRecuperacion!.Token.Should().NotBeEmpty();
    }
}
