using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Personal (personas con tarifa).</summary>
public sealed class PersonalEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;
    public PersonalEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record PersonaResp(Guid Id, string Nombre, string? Puesto, decimal TarifaHora, bool Activo);

    [Fact]
    public async Task Alta_listado_y_edicion_de_personas()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // Alta.
        var crear = await cliente.PostAsJsonAsync("/personal", new { Nombre = "Ana Pérez", TarifaHora = 18.5m, Puesto = "Oficial 1ª" });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var ana = (await crear.Content.ReadFromJsonAsync<PersonaResp>())!;
        ana.Nombre.Should().Be("Ana Pérez");
        ana.TarifaHora.Should().Be(18.5m);
        ana.Activo.Should().BeTrue();

        // Una segunda persona, inactiva tras editar.
        var luis = (await (await cliente.PostAsJsonAsync("/personal", new { Nombre = "Luis Gómez", TarifaHora = 22m })).Content.ReadFromJsonAsync<PersonaResp>())!;
        (await cliente.PutAsJsonAsync($"/personal/{luis.Id}", new { Nombre = "Luis Gómez", TarifaHora = 24m, Puesto = "Encargado", Activo = false })).StatusCode.Should().Be(HttpStatusCode.OK);

        // El listado por defecto solo muestra activas.
        var lista = await cliente.GetFromJsonAsync<List<PersonaResp>>("/personal");
        lista!.Should().ContainSingle().Which.Nombre.Should().Be("Ana Pérez");
    }

    [Fact]
    public async Task Rechaza_tarifa_negativa()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cliente.PostAsJsonAsync("/personal", new { Nombre = "X", TarifaHora = -1m });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
