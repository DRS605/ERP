using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Plantillas de asiento por datos, como en Hispatec: el concepto con variables, el diario y la cuenta y el concepto de
/// cada papel (cliente, ingreso, IVA, banco…) salen de la plantilla del sentido o del origen; lo que no fija, como siempre.
/// </summary>
public sealed class PlantillasAsientoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PlantillasAsientoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, decimal Total, string NumeroCompleto);
    private sealed record ApunteResp(string CuentaCodigo, string? Concepto, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Origen, string Concepto, string? Diario, List<ApunteResp> Apuntes);
    private sealed record PlantillaResp(Guid Id, string Sentido, string? OrigenTipo, string? Concepto, string? Diario);
    private sealed record EsquemaResp(string Sentido, List<object> Papeles);

    private static async Task<List<AsientoResp>> DiarioAsync(HttpClient c) =>
        (await c.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"))!;

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    [Fact]
    public async Task La_plantilla_fija_concepto_diario_y_cuentas_y_sin_ella_se_contabiliza_como_siempre()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await c.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await c.GetFromJsonAsync<List<EsquemaResp>>("/contabilidad/plantillas/esquemas"))!.Select(e => e.Sentido).Should().Equal("Venta", "Compra", "Cobro", "Pago");
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Las cuentas deben existir; una por sentido y origen.
        (await CodigoAsync(await c.PostAsJsonAsync("/contabilidad/plantillas", new
        {
            Sentido = "Venta", Lineas = new[] { new { Papel = "Resultado", Cuenta = "7009999" } },
        }))).Should().Be("plantilla.cuenta_inexistente");
        (await CodigoAsync(await c.PostAsJsonAsync("/contabilidad/plantillas", new
        {
            Sentido = "Cobro", Lineas = new[] { new { Papel = "IvaRepercutido", Cuenta = "477" } },
        }))).Should().Be("plantilla.papel");

        var venta = await c.PostAsJsonAsync("/contabilidad/plantillas", new
        {
            Sentido = "Venta", Concepto = "N/Fra. {Referencia} ({Total} €) a {Tercero}", Diario = "GEN",
            Lineas = new object[] { new { Papel = "Resultado", Cuenta = "700" }, new { Papel = "IvaRepercutido", Concepto = "IVA de {Referencia}" } },
        });
        venta.StatusCode.Should().Be(HttpStatusCode.Created, await venta.Content.ReadAsStringAsync());
        var plantillaVenta = (await venta.Content.ReadFromJsonAsync<PlantillaResp>())!;
        (await CodigoAsync(await c.PostAsJsonAsync("/contabilidad/plantillas", new { Sentido = "Venta" }))).Should().Be("plantilla.repetida");
        (await c.PostAsJsonAsync("/contabilidad/plantillas", new
        {
            Sentido = "Cobro", OrigenTipo = "Movimiento", Concepto = "Cobro {Referencia} ({Fecha})",
            Lineas = new[] { new { Papel = "Tesoreria", Cuenta = "570" } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        var rf = await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Tomate", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        });
        rf.StatusCode.Should().Be(HttpStatusCode.Created, await rf.Content.ReadAsStringAsync());
        var f = (await rf.Content.ReadFromJsonAsync<FacturaResp>())!;
        (await c.PostAsJsonAsync("/cobros", new { FacturaId = f.Id, Importe = f.Total })).EnsureSuccessStatusCode();

        var diario = await DiarioAsync(c);
        var asientoVenta = diario.Single(a => a.Origen == "Venta");
        asientoVenta.Concepto.Should().StartWith("N/Fra. ").And.Contain("(121,00 €)").And.EndWith(" a Frutas del Norte SA");
        asientoVenta.Diario.Should().Be("GEN");
        asientoVenta.Apuntes.Should().Contain(a => a.CuentaCodigo == "700" && a.Haber == 100m);
        asientoVenta.Apuntes.Should().NotContain(a => a.CuentaCodigo == "705");
        asientoVenta.Apuntes.Single(a => a.CuentaCodigo == "477").Concepto.Should().StartWith("IVA de ");
        var cobro = diario.Single(a => a.Origen == "Cobro");
        cobro.Concepto.Should().StartWith("Cobro ").And.EndWith(")");
        cobro.Apuntes.Should().Contain(a => a.CuentaCodigo == "570" && a.Debe == 121m);

        // Sin plantilla, lo de siempre (705, diario de ventas).
        (await c.DeleteAsync(new Uri($"/contabilidad/plantillas/{plantillaVenta.Id}", UriKind.Relative))).EnsureSuccessStatusCode();
        await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Pimiento", PrecioUnitario = 50m, CodigoIva = "IVA21" } },
        });
        var segunda = (await DiarioAsync(c)).Where(a => a.Origen == "Venta").Single(a => a.Id != asientoVenta.Id);
        segunda.Apuntes.Should().Contain(a => a.CuentaCodigo == "705" && a.Haber == 50m);
        segunda.Diario.Should().Be("VEN");
        (await c.GetFromJsonAsync<List<PlantillaResp>>("/contabilidad/plantillas"))!.Should().ContainSingle(p => p.Sentido == "Cobro" && p.OrigenTipo == "Movimiento");
    }
}
