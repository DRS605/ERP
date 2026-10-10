using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del plan de suscripción por empresa (Autónomo/Pyme/Empresa).</summary>
public sealed class PlanEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PlanEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record EmpresaResp(string Plan);

    [Fact]
    public async Task Una_empresa_nueva_nace_en_plan_autonomo()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var emp = await cli.GetFromJsonAsync<EmpresaResp>("/empresas/actual");
        emp!.Plan.Should().Be("Autonomo");
    }

    [Fact]
    public async Task Se_puede_cambiar_el_plan()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var r = await cli.PutAsJsonAsync("/empresas/actual/plan", new { Plan = "Pyme" });
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var emp = await cli.GetFromJsonAsync<EmpresaResp>("/empresas/actual");
        emp!.Plan.Should().Be("Pyme");
    }
}
