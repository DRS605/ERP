using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de la plantilla de documentos configurable (datos de marca + logo) y su PDF.</summary>
public sealed class PlantillaDocumentoEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PlantillaDocumentoEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private const string PngBase64 = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M8AAAMBAQDJ/pLvAAAAAElFTkSuQmCC";

    private sealed record EmpresaResp(string RazonSocial, string Calle, string Poblacion, string? Telefono, string? Web, string? EmailContacto, string? ColorPrincipal, string? TextoPie, byte[]? LogoPng);
    private sealed record IdResp(Guid Id);

    [Fact]
    public async Task Guardar_y_releer_la_plantilla_con_logo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var r = await cliente.PutAsJsonAsync("/empresas/actual/plantilla", new
        {
            RazonSocial = "Panadería López SL",
            Calle = "C/ Mayor 1",
            CodigoPostal = "28001",
            Poblacion = "Madrid",
            Provincia = "Madrid",
            Telefono = "911234567",
            Web = "www.panaderialopez.es",
            Email = "hola@panaderialopez.es",
            ColorPrincipal = "#C0392B",
            TextoPie = "Gracias por su compra · Pago a 30 días",
            LogoPngBase64 = PngBase64,
        });
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var emp = await cliente.GetFromJsonAsync<EmpresaResp>("/empresas/actual");
        emp!.RazonSocial.Should().Be("Panadería López SL");
        emp.Calle.Should().Be("C/ Mayor 1");
        emp.Poblacion.Should().Be("Madrid");
        emp.Telefono.Should().Be("911234567");
        emp.ColorPrincipal.Should().Be("#C0392B");
        emp.TextoPie.Should().Contain("30 días");
        emp.LogoPng.Should().NotBeNull();
        emp.LogoPng!.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Un_color_invalido_da_error()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var r = await cliente.PutAsJsonAsync("/empresas/actual/plantilla", new { ColorPrincipal = "rojo" });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task La_factura_en_pdf_se_genera_con_la_plantilla_aplicada()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/plantilla", new
        {
            ColorPrincipal = "#0EA5B7",
            TextoPie = "Documento de prueba",
            LogoPngBase64 = PngBase64,
        });

        var clienteId = (await (await cliente.PostAsJsonAsync("/clientes", new { Nombre = "Cliente PDF SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var facturaId = (await (await cliente.PostAsJsonAsync("/facturas", new
        {
            ClienteId = clienteId,
            FechaEmision = "2026-03-15",
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var pdf = await cliente.GetAsync($"/facturas/{facturaId}/pdf");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        bytes.Length.Should().BeGreaterThan(1000);
        // Firma de un PDF: «%PDF».
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
    }
}
