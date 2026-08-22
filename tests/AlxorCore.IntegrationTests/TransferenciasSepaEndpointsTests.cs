using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la remesa de transferencias SEPA (pain.001 / Cuaderno 34) para pagar gastos.</summary>
public sealed class TransferenciasSepaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public TransferenciasSepaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record RemesaResp(string Fichero, string NombreArchivo, int NumeroPagos, decimal Total, System.Collections.Generic.List<string> Omitidos);

    [Fact]
    public async Task Genera_un_fichero_pain001_con_los_gastos_de_proveedores_con_iban()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = "ES9121000418450200051332", IdentificadorAcreedor = "ES12345Z" });
        var provId = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Suministros Ebro SL", NifFiscal = "B12345674", Iban = "ES7620770024003102575766" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var gastoId = (await (await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Material", ProveedorId = provId, BaseImponible = 100m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var resp = await cliente.PostAsJsonAsync("/tesoreria/transferencias", new { GastoIds = new[] { gastoId } });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var remesa = await resp.Content.ReadFromJsonAsync<RemesaResp>();

        remesa!.NumeroPagos.Should().Be(1);
        remesa.Total.Should().Be(121m);
        remesa.Fichero.Should().Contain("CstmrCdtTrfInitn");
        remesa.Fichero.Should().Contain("pain.001.001.03");
        remesa.Fichero.Should().Contain("ES7620770024003102575766");
    }

    [Fact]
    public async Task Omite_los_gastos_de_proveedores_sin_iban()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = "ES9121000418450200051332", IdentificadorAcreedor = "ES12345Z" });
        var provId = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Sin IBAN SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var gastoId = (await (await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Material", ProveedorId = provId, BaseImponible = 100m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var resp = await cliente.PostAsJsonAsync("/tesoreria/transferencias", new { GastoIds = new[] { gastoId } });
        // Ninguno pagable → 400 con el motivo.
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
