using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del control de riesgo por cliente/proveedor (aviso o bloqueo, configurable).</summary>
public sealed class RiesgoEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public RiesgoEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total, string? AvisoRiesgo);
    private sealed record GastoResp(Guid Id, decimal Total, string? AvisoRiesgo);

    private static async Task<Guid> ClienteConLimiteAsync(HttpClient c, decimal limite) =>
        (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Cliente Riesgo SL", NifFiscal = "B12345674", LimiteRiesgo = limite })).Content.ReadFromJsonAsync<IdResp>())!.Id;

    private static object FacturaDe(Guid clienteId, decimal precio) => new
    {
        ClienteId = clienteId,
        Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = precio, CodigoIva = "IVA0" } },
    };

    [Fact]
    public async Task Por_defecto_avisa_pero_no_bloquea_al_superar_el_limite()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var clienteId = await ClienteConLimiteAsync(cliente, 100m);

        // 150 > 100 → supera; modo por defecto Aviso.
        var resp = await cliente.PostAsJsonAsync("/facturas", FacturaDe(clienteId, 150m));
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var factura = await resp.Content.ReadFromJsonAsync<FacturaResp>();
        factura!.AvisoRiesgo.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task En_modo_bloqueo_impide_emitir_si_supera_el_limite()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cliente.PutAsJsonAsync("/empresas/actual/control-riesgo", new { ControlRiesgo = "Bloqueo" })).StatusCode.Should().Be(HttpStatusCode.OK);
        var clienteId = await ClienteConLimiteAsync(cliente, 100m);

        var resp = await cliente.PostAsJsonAsync("/facturas", FacturaDe(clienteId, 150m));
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Dentro_del_limite_emite_sin_aviso()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/control-riesgo", new { ControlRiesgo = "Bloqueo" });
        var clienteId = await ClienteConLimiteAsync(cliente, 1000m);

        var resp = await cliente.PostAsJsonAsync("/facturas", FacturaDe(clienteId, 150m));
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var factura = await resp.Content.ReadFromJsonAsync<FacturaResp>();
        factura!.AvisoRiesgo.Should().BeNull();
    }

    [Fact]
    public async Task El_riesgo_vivo_acumula_las_facturas_pendientes()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cliente.PutAsJsonAsync("/empresas/actual/control-riesgo", new { ControlRiesgo = "Bloqueo" });
        var clienteId = await ClienteConLimiteAsync(cliente, 100m);

        // Primera de 60 pasa (60 <= 100). Segunda de 60 → 60 vivo + 60 = 120 > 100 → bloquea.
        (await cliente.PostAsJsonAsync("/facturas", FacturaDe(clienteId, 60m))).StatusCode.Should().Be(HttpStatusCode.Created);
        (await cliente.PostAsJsonAsync("/facturas", FacturaDe(clienteId, 60m))).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task El_limite_del_proveedor_avisa_al_registrar_un_gasto()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var provId = (await (await cliente.PostAsJsonAsync("/proveedores", new { Nombre = "Prov Riesgo SL", NifFiscal = "B12345674", LimiteRiesgo = 50m })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        var resp = await cliente.PostAsJsonAsync("/gastos", new { Concepto = "Compra", ProveedorId = provId, BaseImponible = 100m, CodigoIva = "IVA0" });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var gasto = await resp.Content.ReadFromJsonAsync<GastoResp>();
        gasto!.AvisoRiesgo.Should().NotBeNullOrEmpty();
    }
}
