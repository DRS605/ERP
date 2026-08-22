using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de los ficheros bancarios de ancho fijo: Cuaderno 19 clásico y Confirming (C68).</summary>
public sealed class FicherosCsbEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FicherosCsbEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FicheroResp(string Fichero, string NombreArchivo, int NumeroRegistros, decimal Total, System.Collections.Generic.List<string> Omitidos);

    [Fact]
    public async Task El_cuaderno_19_clasico_produce_registros_de_162_posiciones()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = "ES9121000418450200051332", IdentificadorAcreedor = "ES12345Z" });
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente CSB SL", NifFiscal = "B12345674", Iban = "ES7620770024003102575766" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var facturaId = (await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA0" } },
        })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var resp = await cliente.PostAsJsonAsync("/tesoreria/cuaderno19", new { FacturaIds = new[] { facturaId } });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fichero = await resp.Content.ReadFromJsonAsync<FicheroResp>();

        fichero!.NumeroRegistros.Should().Be(1);
        var lineas = fichero.Fichero.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries);
        lineas.Should().OnlyContain(l => l.Length == 162);
        lineas[0].Should().StartWith("5180");
        lineas[^1].Should().StartWith("5980");
    }

    [Fact]
    public async Task El_confirming_produce_una_cabecera_detalle_y_totales()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/cobro", new { Iban = "ES9121000418450200051332", IdentificadorAcreedor = "ES12345Z" });
        var provId = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Proveedor Confirming SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var gastoId = (await (await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Compra", ProveedorId = provId, BaseImponible = 100m, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var resp = await cliente.PostAsJsonAsync("/tesoreria/confirming", new { GastoIds = new[] { gastoId } });
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var fichero = await resp.Content.ReadFromJsonAsync<FicheroResp>();

        fichero!.NumeroRegistros.Should().Be(1);
        fichero.Total.Should().Be(121m);
        var lineas = fichero.Fichero.Split("\r\n", System.StringSplitOptions.RemoveEmptyEntries);
        lineas.Should().OnlyContain(l => l.Length == 100);
        lineas[0].Should().StartWith("01");   // cabecera
        lineas[^1].Should().StartWith("09");   // totales
    }
}
