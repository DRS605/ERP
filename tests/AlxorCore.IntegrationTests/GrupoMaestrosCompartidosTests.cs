using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Pruebas del concepto de grupo (holding): los maestros de Terceros se comparten entre las empresas
/// del mismo grupo, y quedan aislados de empresas de otros grupos.
/// </summary>
public sealed class GrupoMaestrosCompartidosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public GrupoMaestrosCompartidosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record GrupoResp(Guid Id);
    private sealed record EmpresaResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ClienteResp(Guid Id, string Nombre);

    private static async Task SeleccionarAsync(HttpClient cli, Guid empresaId)
    {
        var sel = await (await cli.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null))
            .Content.ReadFromJsonAsync<SeleccionResp>();
        cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sel!.Token);
    }

    [Fact]
    public async Task Dos_empresas_del_mismo_grupo_comparten_los_clientes()
    {
        // Empresa A (crea su propio grupo) + un cliente.
        var (cli, empresaA) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await cli.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        var creado = await (await cli.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Compartido", NifFiscal = "12345678Z" }))
            .Content.ReadFromJsonAsync<ClienteResp>();

        // Empresa B en el MISMO grupo.
        var empresaB = (await (await cli.PostAsJsonAsync("/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Empresa B SL", GrupoId = grupo }))
            .Content.ReadFromJsonAsync<EmpresaResp>())!.Id;
        await SeleccionarAsync(cli, empresaB);

        // Desde B se ve el cliente creado en A (maestro compartido por grupo).
        var clientesB = await cli.GetFromJsonAsync<List<ClienteResp>>("/clientes");
        clientesB!.Should().Contain(c => c.Id == creado!.Id);
    }

    [Fact]
    public async Task Empresas_de_grupos_distintos_no_comparten_clientes()
    {
        var (cliA, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await (await cliA.PostAsJsonAsync("/clientes", new { Nombre = "Cliente de A", NifFiscal = "12345678Z" })).Content.ReadFromJsonAsync<ClienteResp>();

        // Un usuario/empresa totalmente independiente (su propio grupo) no ve los clientes de A.
        var (cliB, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clientesB = await cliB.GetFromJsonAsync<List<ClienteResp>>("/clientes");
        clientesB!.Should().NotContain(c => c.Nombre == "Cliente de A");
    }
}
