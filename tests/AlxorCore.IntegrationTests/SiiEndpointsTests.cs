using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la generación del XML del SII (facturas emitidas y recibidas).</summary>
public sealed class SiiEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SiiEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);

    [Fact]
    public async Task El_sii_de_emitidas_incluye_la_factura_del_periodo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SII SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-03-15",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 1000m, CodigoIva = "IVA21" } },
        });

        var r = await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=3");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        r.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");
        var xml = await r.Content.ReadAsStringAsync();

        xml.Should().Contain("SuministroLRFacturasEmitidas");
        xml.Should().Contain("RegistroLRFacturasEmitidas");
        xml.Should().Contain("B12345674");                 // NIF de la contraparte
        xml.Should().Contain("<siiLR:ImporteTotal");       // total de la factura presente
        xml.Should().Contain("1210.00");                   // 1000 + 21%
        xml.Should().Contain("21");                        // tipo impositivo
    }

    [Fact]
    public async Task El_sii_de_recibidas_incluye_el_gasto_del_periodo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Suministros marzo", BaseImponible = 200m, CodigoIva = "IVA21", Fecha = "2026-03-10" });

        var r = await cliente.GetAsync("/informes/sii?tipo=Recibidas&ejercicio=2026&periodo=3");
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var xml = await r.Content.ReadAsStringAsync();
        xml.Should().Contain("SuministroLRFacturasRecibidas");
        xml.Should().Contain("Suministros marzo");
        xml.Should().Contain("CuotaDeducible");
    }

    [Fact]
    public async Task Un_periodo_invalido_da_error()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.GetAsync("/informes/sii?tipo=Emitidas&ejercicio=2026&periodo=13")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
