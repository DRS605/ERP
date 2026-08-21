using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de la recepción y contabilización de facturas de proveedor.</summary>
public sealed class RecepcionEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public RecepcionEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record FacturaRecibidaResp(Guid Id, string Estado, string NombreArchivo, long TamanoBytes, string? ProveedorTexto, decimal? BaseImponible, Guid? GastoId);
    private sealed record GastoResp(Guid Id, string Concepto, decimal BaseImponible, decimal CuotaIva, decimal Total);

    private static string PdfDemo() => Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }); // "%PDF-1.4"

    private static async Task<FacturaRecibidaResp> RecibirAsync(HttpClient cliente)
    {
        var resp = await cliente.PostAsJsonAsync("/recepcion/facturas", new
        {
            NombreArchivo = "factura-proveedor.pdf",
            ContenidoBase64 = PdfDemo(),
            TipoContenido = "application/pdf",
            RemitenteCorreo = "facturas@proveedor.com",
            AsuntoCorreo = "Factura agosto",
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await resp.Content.ReadFromJsonAsync<FacturaRecibidaResp>())!;
    }

    [Fact]
    public async Task Flujo_completo_recibir_validar_y_contabilizar_genera_un_gasto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var recibida = await RecibirAsync(cliente);
        recibida.Estado.Should().Be("Recibida");
        recibida.TamanoBytes.Should().BeGreaterThan(0);

        // No se puede contabilizar sin validar antes.
        var sinValidar = await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);
        sinValidar.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Validación (revisión humana): base 300 €, IVA 21 %.
        var validar = await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new
        {
            BaseImponible = 300m,
            FechaFactura = "2026-08-01",
            ProveedorTexto = "Suministros Ebro SL",
            NumeroFactura = "P-2026-88",
            CodigoIva = "IVA21",
            PorcentajeIrpf = 0m,
        });
        validar.StatusCode.Should().Be(HttpStatusCode.OK);
        var validada = await validar.Content.ReadFromJsonAsync<FacturaRecibidaResp>();
        validada!.Estado.Should().Be("Validada");
        validada.ProveedorTexto.Should().Be("Suministros Ebro SL");

        // Contabilización: genera un gasto con IVA soportado.
        var contab = await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);
        contab.StatusCode.Should().Be(HttpStatusCode.OK);
        var contabilizada = await contab.Content.ReadFromJsonAsync<FacturaRecibidaResp>();
        contabilizada!.Estado.Should().Be("Contabilizada");
        contabilizada.GastoId.Should().NotBeNull();

        // El gasto existe y cuadra (300 base + 63 IVA = 363).
        var gastos = await cliente.GetFromJsonAsync<List<GastoResp>>("/gastos");
        var gasto = gastos!.Single(g => g.Id == contabilizada.GastoId);
        gasto.BaseImponible.Should().Be(300m);
        gasto.CuotaIva.Should().Be(63m);
        gasto.Total.Should().Be(363m);
        gasto.Concepto.Should().Contain("P-2026-88");
    }

    [Fact]
    public async Task Validar_sin_proveedor_devuelve_400()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var recibida = await RecibirAsync(cliente);

        var validar = await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new
        {
            BaseImponible = 100m,
            FechaFactura = "2026-08-01",
            CodigoIva = "IVA21",
        });
        validar.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Rechazar_saca_la_factura_de_la_bandeja()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var recibida = await RecibirAsync(cliente);

        var rechazar = await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/rechazar", new { Motivo = "Duplicada" });
        rechazar.StatusCode.Should().Be(HttpStatusCode.OK);

        var pendientes = await cliente.GetFromJsonAsync<List<FacturaRecibidaResp>>("/recepcion/facturas?estado=Recibida");
        pendientes!.Should().NotContain(f => f.Id == recibida.Id);
    }

    [Fact]
    public async Task Descargar_documento_devuelve_el_pdf()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var recibida = await RecibirAsync(cliente);

        var doc = await cliente.GetAsync($"/recepcion/facturas/{recibida.Id}/documento");
        doc.StatusCode.Should().Be(HttpStatusCode.OK);
        doc.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await doc.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
    }
}
