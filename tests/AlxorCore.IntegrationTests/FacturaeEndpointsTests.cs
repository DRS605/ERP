using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de la factura electrónica española (Facturae 3.2.2) y sus centros DIR3 (FACe).</summary>
public sealed class FacturaeEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FacturaeEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ClienteResp(Guid Id);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto);

    private static async Task<Guid> CrearClienteAsync(HttpClient cli, object cuerpo) =>
        (await (await cli.PostAsJsonAsync("/clientes", cuerpo)).Content.ReadFromJsonAsync<ClienteResp>())!.Id;

    private static async Task<FacturaResp> EmitirAsync(HttpClient cli, Guid clienteId, decimal precio = 100m, string iva = "IVA21") =>
        (await (await cli.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio de consultoría", PrecioUnitario = precio, CodigoIva = iva } },
        })).Content.ReadFromJsonAsync<FacturaResp>())!;

    [Fact]
    public async Task Genera_facturae_322_con_totales_y_partes()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cli, new { Nombre = "Empresa Privada SL", NifFiscal = "B12345678" });
        var factura = await EmitirAsync(cli, clienteId);

        var resp = await cli.GetAsync(new Uri($"/facturas/{factura.Id}/facturae.xml", UriKind.Relative));
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        resp.Content.Headers.ContentType!.MediaType.Should().Be("application/xml");

        var xml = await resp.Content.ReadAsStringAsync();
        xml.Should().Contain("<SchemaVersion>3.2.2</SchemaVersion>");
        xml.Should().Contain("<InvoiceNumber>" + factura.NumeroCompleto + "</InvoiceNumber>");
        xml.Should().Contain("<CorporateName>Empresa Privada SL</CorporateName>");
        xml.Should().Contain("<TaxIdentificationNumber>B12345678</TaxIdentificationNumber>");
        // 100 base + 21 % IVA = 121 total ejecutable.
        xml.Should().Contain("<InvoiceTotal>121.00</InvoiceTotal>");
        xml.Should().Contain("<TotalExecutableAmount>121.00</TotalExecutableAmount>");
        xml.Should().Contain("<TaxRate>21.00</TaxRate>");
        // Un cliente privado no lleva centros administrativos.
        xml.Should().NotContain("<AdministrativeCentres>");
    }

    [Fact]
    public async Task Cliente_administracion_publica_incluye_centros_dir3()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await CrearClienteAsync(cli, new
        {
            Nombre = "Ayuntamiento de Ejemplo",
            NifFiscal = "P2800000H",
            EsAdministracionPublica = true,
            Dir3OficinaContable = "L01280796",
            Dir3OrganoGestor = "LA0002982",
            Dir3UnidadTramitadora = "GE0010034",
        });
        var factura = await EmitirAsync(cli, clienteId);

        var xml = await cli.GetStringAsync($"/facturas/{factura.Id}/facturae.xml");
        xml.Should().Contain("<AdministrativeCentres>");
        xml.Should().Contain("<CentreCode>L01280796</CentreCode>");
        xml.Should().Contain("<CentreCode>LA0002982</CentreCode>");
        xml.Should().Contain("<CentreCode>GE0010034</CentreCode>");
        xml.Should().Contain("<RoleTypeCode>01</RoleTypeCode>");
        xml.Should().Contain("<RoleTypeCode>02</RoleTypeCode>");
        xml.Should().Contain("<RoleTypeCode>03</RoleTypeCode>");
    }

    [Fact]
    public async Task Ticket_sin_destinatario_no_puede_generar_facturae()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var ticket = await (await cli.PostAsJsonAsync("/tickets", new
        {
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Café", PrecioUnitario = 2m, CodigoIva = "IVA10" } },
        })).Content.ReadFromJsonAsync<FacturaResp>();

        var resp = await cli.GetAsync(new Uri($"/facturas/{ticket!.Id}/facturae.xml", UriKind.Relative));
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
