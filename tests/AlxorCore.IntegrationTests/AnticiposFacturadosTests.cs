using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Anticipos facturados (art. 75.2 LIVA): la factura del anticipo lleva su IVA (base a la 438), su cobro va contra
/// ella, la factura final lo descuenta con una línea negativa (base e IVA) y el 303 declara el IVA en cada momento.
/// </summary>
public sealed class AnticiposFacturadosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AnticiposFacturadosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record AnticipoResp(Guid Id, decimal Importe, decimal Disponible, string Estado, Guid? FacturaId, string? FacturaNumero, decimal? BaseFacturada, decimal DisponibleBase);
    private sealed record LineaResp(string Descripcion, decimal Base, decimal CuotaIva, string? CuentaContable, Guid? AnticipoId);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal BaseImponible, decimal CuotaIva, decimal Total, List<LineaResp> Lineas);
    private sealed record SaldoResp(decimal Pendiente);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, string Concepto, List<ApunteResp> Apuntes);

    private static decimal Debe(AsientoResp a, string cuenta) => a.Apuntes.Where(x => x.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(x => x.Debe);
    private static decimal Haber(AsientoResp a, string cuenta) => a.Apuntes.Where(x => x.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(x => x.Haber);

    [Fact]
    public async Task La_factura_del_anticipo_lleva_su_iva_y_la_final_lo_descuenta()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Obras Levante SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;

        // Anticipo de 1.210 € con factura: base 1.000 (a la 438) e IVA 210.
        var alta = await api.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 1210m, Facturar = true, CodigoIva = "IVA21", Concepto = "Pedido de naves" });
        alta.StatusCode.Should().Be(HttpStatusCode.Created, await alta.Content.ReadAsStringAsync());
        var anticipo = (await alta.Content.ReadFromJsonAsync<AnticipoResp>())!;
        anticipo.Should().Match<AnticipoResp>(a => a.Importe == 1210m && a.BaseFacturada == 1000m && a.DisponibleBase == 1000m && a.FacturaNumero != null);
        (await api.GetFromJsonAsync<SaldoResp>($"/facturas/{anticipo.FacturaId}/saldo"))!.Pendiente.Should().Be(0m, "el anticipo está cobrado");

        var diario = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!;
        var facturaAnticipo = diario.Single(a => a.Origen == "Venta");
        Debe(facturaAnticipo, "430").Should().Be(1210m);
        Haber(facturaAnticipo, "438").Should().Be(1000m);
        Haber(facturaAnticipo, "477").Should().Be(210m);
        Haber(facturaAnticipo, "70").Should().Be(0m, "un anticipo no es venta todavía");
        var cobro = diario.Single(a => a.Origen == "Cobro");
        Debe(cobro, "57").Should().Be(1210m);
        Haber(cobro, "430").Should().Be(1210m);

        // Un anticipo facturado no se aplica como cobro: se descuenta en la factura final.
        var comoCobro = await api.PostAsJsonAsync($"/anticipos/{anticipo.Id}/aplicar", new { FacturaId = anticipo.FacturaId });
        (await comoCobro.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("anticipo.facturado");

        // Factura final de 3.000 € de base con el anticipo descontado: 3.000 − 1.000 = 2.000 de base, 420 de IVA.
        var final = new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Nave industrial", PrecioUnitario = 3000m, CodigoIva = "IVA21" } },
            DescontarAnticipos = new[] { new { AnticipoId = anticipo.Id } },
        };
        var simulada = (await (await api.PostAsJsonAsync("/facturas/simular", final)).Content.ReadFromJsonAsync<FacturaResp>())!;
        simulada.Total.Should().Be(2420m);
        var r = await api.PostAsJsonAsync("/facturas", final);
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var f = (await r.Content.ReadFromJsonAsync<FacturaResp>())!;
        f.Should().Match<FacturaResp>(x => x.BaseImponible == 2000m && x.CuotaIva == 420m && x.Total == 2420m);
        f.Lineas.Select(l => l.Descripcion).First().Should().Be("Nave industrial", "las líneas salen en el orden de emisión; el descuento, al final");
        f.Lineas.Should().ContainSingle(l => l.AnticipoId == anticipo.Id).Which.Should().Match<LineaResp>(l => l.Base == -1000m && l.CuotaIva == -210m && l.CuentaContable == "438");

        var asientoFinal = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!
            .Where(a => a.Origen == "Venta").Single(a => a.Concepto.StartsWith(f.NumeroCompleto, StringComparison.Ordinal));
        Debe(asientoFinal, "430").Should().Be(2420m);
        Haber(asientoFinal, "70").Should().Be(3000m);
        Debe(asientoFinal, "438").Should().Be(1000m, "se cancela el anticipo facturado");
        Haber(asientoFinal, "477").Should().Be(420m);

        var tras = (await api.GetFromJsonAsync<List<AnticipoResp>>($"/anticipos?clienteId={cliente}"))!.Single();
        tras.Should().Match<AnticipoResp>(a => a.DisponibleBase == 0m && a.Disponible == 0m && a.Estado == "Aplicado");

        // 303: 210 al cobrar el anticipo y 420 en la final = 630, el 21 % de los 3.000.
        var trimestre = (DateTime.Today.Month - 1) / 3 + 1;
        using (var resumen = JsonDocument.Parse(await api.GetStringAsync($"/informes/resumen-trimestral?anio={DateTime.Today.Year}&trimestre={trimestre}")))
        {
            resumen.RootElement.GetProperty("modelo303").GetProperty("ivaDevengadoCuota").GetDecimal().Should().Be(630m);
        }

        // No se descuenta dos veces.
        var otra = await api.PostAsJsonAsync("/facturas", final);
        (await otra.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("anticipo.sin_saldo");

        // La factura del anticipo no se anula mientras esté descontado; anulada la final, vuelve a estar disponible.
        var anularAnticipo = await api.PostAsJsonAsync($"/facturas/{anticipo.FacturaId}/anular", new { Motivo = "Error" });
        (await anularAnticipo.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().BeOneOf("anticipo.descontado", "factura.con_cobros");
        (await api.PostAsJsonAsync($"/facturas/{f.Id}/anular", new { Motivo = "Error en el pedido" })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<AnticipoResp>>($"/anticipos?clienteId={cliente}"))!.Single().DisponibleBase.Should().Be(1000m);
    }

    [Fact]
    public async Task El_descuento_no_supera_la_factura_final()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var anticipo = (await (await api.PostAsJsonAsync("/anticipos", new { ClienteId = cliente, Importe = 1210m, Facturar = true, CodigoIva = "IVA21" })).Content.ReadFromJsonAsync<AnticipoResp>())!;

        // Factura de 400 € de base: se descuentan 400 y quedan 600 del anticipo para la siguiente.
        var r = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Primera entrega", PrecioUnitario = 400m, CodigoIva = "IVA21" } },
            DescontarAnticipos = new[] { new { AnticipoId = anticipo.Id } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        (await r.Content.ReadFromJsonAsync<FacturaResp>())!.Total.Should().Be(0m);
        (await api.GetFromJsonAsync<List<AnticipoResp>>($"/anticipos?clienteId={cliente}"))!.Single().DisponibleBase.Should().Be(600m);
    }
}
