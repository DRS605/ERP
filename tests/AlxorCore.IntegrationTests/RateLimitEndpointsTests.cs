using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Fábrica con un límite de autenticación bajo, aislada del resto de pruebas, para comprobar el
/// <i>rate limiting</i> sin afectar a la batería (que hace muchos logins).
/// </summary>
public sealed class FabricaApiRateLimit : FabricaApiPruebas
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((_, cfg) =>
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seguridad:RateLimitPeticiones"] = "3",
                ["Seguridad:RateLimitVentanaSegundos"] = "60",
            }));
    }
}

/// <summary>Pruebas del rate limiting en autenticación.</summary>
public sealed class RateLimitEndpointsTests : IClassFixture<FabricaApiRateLimit>
{
    private readonly FabricaApiRateLimit _fabrica;

    public RateLimitEndpointsTests(FabricaApiRateLimit fabrica) => _fabrica = fabrica;

    [Fact]
    public async Task El_login_se_limita_tras_superar_el_cupo()
    {
        var cli = _fabrica.CreateClient();
        var cuerpo = new { Email = "noexiste@ejemplo.com", Contrasena = "loquesea" };

        // Con el límite en 3, las primeras se procesan (401 credenciales) y a partir de ahí llega el 429.
        var codigos = new List<HttpStatusCode>();
        for (var i = 0; i < 6; i++)
        {
            var r = await cli.PostAsJsonAsync("/auth/login", cuerpo);
            codigos.Add(r.StatusCode);
        }

        Assert.Contains(HttpStatusCode.TooManyRequests, codigos);
    }
}
