using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Contabilidad (partida doble) y su enganche con Recepción.</summary>
public sealed class ContabilidadEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ContabilidadEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, int Ejercicio, int Numero, string Concepto, string Origen, decimal Total, List<ApunteResp> Apuntes);
    private sealed record ModoResp(string Modo);
    private sealed record FacturaRecibidaResp(Guid Id, string Estado, Guid? GastoId);

    [Fact]
    public async Task Modo_por_defecto_es_simple()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var modo = await cliente.GetFromJsonAsync<ModoResp>("/contabilidad/modo");
        modo!.Modo.Should().Be("Simple");
    }

    [Fact]
    public async Task Crear_asiento_manual_cuadrado_aparece_en_el_diario()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var crear = await cliente.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-03-15",
            Concepto = "Aportación de socio",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 1000m, Haber = 0m, Concepto = (string?)null },
                new { CuentaCodigo = "100", Debe = 0m, Haber = 1000m, Concepto = (string?)null },
            },
        });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario!.Should().ContainSingle(a => a.Concepto == "Aportación de socio" && a.Total == 1000m);
    }

    [Fact]
    public async Task Asiento_descuadrado_devuelve_400()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var crear = await cliente.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-03-15",
            Concepto = "Descuadrado",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 100m, Haber = 0m },
                new { CuentaCodigo = "700", Debe = 0m, Haber = 90m },
            },
        });
        crear.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task En_modo_completo_contabilizar_una_factura_genera_el_asiento_de_partida_doble()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // Activar partida doble.
        var modo = await cliente.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" });
        modo.StatusCode.Should().Be(HttpStatusCode.OK);

        // Recibir + validar + contabilizar una factura de proveedor (base 300, IVA 21 %).
        var pdf = Convert.ToBase64String(new byte[] { 0x25, 0x50, 0x44, 0x46 });
        var recibida = (await (await cliente.PostAsJsonAsync("/recepcion/facturas", new { NombreArchivo = "f.pdf", ContenidoBase64 = pdf, TipoContenido = "application/pdf" })).Content.ReadFromJsonAsync<FacturaRecibidaResp>())!;
        await cliente.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new { BaseImponible = 300m, FechaFactura = "2026-08-01", ProveedorTexto = "Suministros Ebro SL", NumeroFactura = "P-1", CodigoIva = "IVA21", PorcentajeIrpf = 0m });
        var contab = await cliente.PostAsync($"/recepcion/facturas/{recibida.Id}/contabilizar", null);
        contab.StatusCode.Should().Be(HttpStatusCode.OK);
        (await contab.Content.ReadFromJsonAsync<FacturaRecibidaResp>())!.Estado.Should().Be("Contabilizada");

        // El diario del 2026 tiene el asiento de compra, cuadrado.
        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        var asiento = diario!.Single(a => a.Origen == "Compra");
        asiento.Total.Should().Be(363m);
        asiento.Apuntes.Sum(x => x.Debe).Should().Be(asiento.Apuntes.Sum(x => x.Haber));
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "629" && x.Debe == 300m);
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "472" && x.Debe == 63m);
        asiento.Apuntes.Should().Contain(x => x.CuentaCodigo == "400" && x.Haber == 363m);
    }
}
