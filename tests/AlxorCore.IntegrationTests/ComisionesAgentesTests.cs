using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Comisiones de agentes: cartera de clientes con fechas, reglas por familia y cliente, devengo al facturar y al cobrar,
/// liquidación que no repite facturas, factura del agente externo y anulación.
/// </summary>
public sealed class ComisionesAgentesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ComisionesAgentesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal Total);
    private sealed record LineaResp(Guid FacturaId, decimal BaseVenta, decimal Comision, decimal PorcentajeDevengado, decimal YaLiquidado, decimal Pendiente);
    private sealed record CalculoResp(string Devengo, decimal Pendiente, List<LineaResp> Lineas);
    private sealed record LiquidacionResp(Guid Id, string Numero, string Estado, decimal Importe, Guid? GastoId);
    private sealed record GastoResp(Guid Id, decimal BaseImponible, decimal RetencionIrpf, decimal Total, string Estado, string? NumeroFactura);
    private sealed record AgenteResp(Guid Id, string Nombre, int Clientes);

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> FalloAsync(Task<HttpResponseMessage> peticion, HttpStatusCode esperado)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo) => (await OkAsync<IdResp>(api.PostAsJsonAsync(ruta, cuerpo))).Id;

    [Fact]
    public async Task Comisiones_por_reglas_devengo_al_facturar_y_al_cobrar_y_liquidaciones()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var a = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Alba SL", NifFiscal = Ayudas.GenerarNif() });
        var b = await IdAsync(api, "/clientes", new { Nombre = "Fruterías Bea SL", NifFiscal = Ayudas.GenerarNif() });
        var vinos = await IdAsync(api, "/familias", new { Nombre = "Vinos", Codigo = "VIN" });
        var vino = await IdAsync(api, "/productos", new { Nombre = "Crianza", PrecioUnitario = 10m, CodigoIva = "IVA21", FamiliaId = vinos });
        var aceite = await IdAsync(api, "/productos", new { Nombre = "Aceite", PrecioUnitario = 10m, CodigoIva = "IVA21" });

        var proveedorRep = await IdAsync(api, "/proveedores", new { Nombre = "Representaciones Ruiz SL", NifFiscal = Ayudas.GenerarNif() });
        var rep = (await OkAsync<IdResp>(api.PostAsJsonAsync("/comisiones/agentes", new { Nombre = "Representaciones Ruiz", Porcentaje = 5m, ProveedorId = proveedorRep, PorcentajeIrpf = 15m }))).Id;
        var lucia = (await OkAsync<IdResp>(api.PostAsJsonAsync("/comisiones/agentes", new { Nombre = "Lucía (vendedora)", Porcentaje = 3m, Devengo = "Cobrado" }))).Id;
        (await FalloAsync(api.PostAsJsonAsync("/comisiones/agentes", new { Nombre = "X", Porcentaje = 120m }), HttpStatusCode.BadRequest)).Should().Be("agente.porcentaje");

        // Vino al 10 % para el representante; al cliente B, todo al 2 %.
        (await api.PostAsJsonAsync("/comisiones/reglas", new { AgenteId = rep, FamiliaId = vinos, Porcentaje = 10m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/comisiones/reglas", new { AgenteId = rep, ClienteId = b, Porcentaje = 2m })).EnsureSuccessStatusCode();
        (await FalloAsync(api.PostAsJsonAsync("/comisiones/reglas", new { AgenteId = rep, Porcentaje = 2m }), HttpStatusCode.BadRequest)).Should().Be("comision.regla");

        // A y B, del representante; B pasa a Lucía hace 10 días.
        (await api.PostAsJsonAsync("/comisiones/asignaciones", new { ClienteId = a, AgenteId = rep, Desde = Hoy.AddDays(-60) })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/comisiones/asignaciones", new { ClienteId = b, AgenteId = rep, Desde = Hoy.AddDays(-60) })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/comisiones/asignaciones", new { ClienteId = b, AgenteId = lucia, Desde = Hoy.AddDays(-10) })).EnsureSuccessStatusCode();
        (await OkAsync<List<AgenteResp>>(api.GetAsync("/comisiones/agentes"))).Single(x => x.Id == lucia).Clientes.Should().Be(1);

        object Linea(Guid producto, decimal base_) => new { ProductoId = producto, Cantidad = 1m, Descripcion = "Venta", PrecioUnitario = base_, CodigoIva = "IVA21" };
        var fa = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = a, FechaEmision = Hoy.AddDays(-30), Lineas = new[] { Linea(vino, 100m), Linea(aceite, 200m) } }));
        var fb1 = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = b, FechaEmision = Hoy.AddDays(-20), Lineas = new[] { Linea(vino, 300m) } }));
        var fb2 = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = b, FechaEmision = Hoy.AddDays(-5), Lineas = new[] { Linea(aceite, 1000m) } }));

        // Representante: A = 100 × 10 % + 200 × 5 % = 20; B (regla de cliente y familia: gana la de cliente, más concreta) = 300 × 2 % = 6.
        var calc = await OkAsync<CalculoResp>(api.GetAsync($"/comisiones/calculo/{rep}?desde={Hoy.AddDays(-60):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"));
        calc.Lineas.Select(l => (l.FacturaId, l.Pendiente)).Should().Equal((fa.Id, 20m), (fb1.Id, 6m));
        calc.Pendiente.Should().Be(26m);

        // Liquidación con la factura del representante (IRPF 15 %).
        var liq = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/comisiones/liquidaciones", new
        {
            AgenteId = rep, Desde = Hoy.AddDays(-60), Hasta = Hoy, GenerarFactura = true, NumeroFacturaAgente = "R-2026-14",
        }));
        liq.Should().Match<LiquidacionResp>(l => l.Importe == 26m && l.Estado == "Emitida" && l.GastoId != null && l.Numero.StartsWith("LAG-"));
        var gasto = (await OkAsync<List<GastoResp>>(api.GetAsync("/gastos"))).Single(g => g.Id == liq.GastoId);
        gasto.Should().Match<GastoResp>(g => g.BaseImponible == 26m && g.RetencionIrpf == 3.90m && g.Total == 27.56m && g.NumeroFactura == "R-2026-14");
        (await FalloAsync(api.PostAsJsonAsync("/comisiones/liquidaciones", new { AgenteId = rep, Desde = Hoy.AddDays(-60), Hasta = Hoy }), HttpStatusCode.BadRequest))
            .Should().Be("liquidacion_agente.vacia");

        // Lucía cobra al cobro: con la mitad cobrada, 1.000 × 3 % × 50 % = 15.
        (await OkAsync<CalculoResp>(api.GetAsync($"/comisiones/calculo/{lucia}?desde={Hoy.AddDays(-30):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"))).Pendiente.Should().Be(0m);
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = fb2.Id, Importe = fb2.Total / 2 })).EnsureSuccessStatusCode();
        var cl = await OkAsync<CalculoResp>(api.GetAsync($"/comisiones/calculo/{lucia}?desde={Hoy.AddDays(-30):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"));
        cl.Lineas.Single().Should().Match<LineaResp>(l => l.Comision == 30m && l.PorcentajeDevengado == 50m && l.Pendiente == 15m);
        (await FalloAsync(api.PostAsJsonAsync("/comisiones/liquidaciones", new { AgenteId = lucia, Desde = Hoy.AddDays(-30), Hasta = Hoy, GenerarFactura = true }),
            HttpStatusCode.BadRequest)).Should().Be("liquidacion_agente.sin_proveedor");
        (await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/comisiones/liquidaciones", new { AgenteId = lucia, Desde = Hoy.AddDays(-30), Hasta = Hoy }))).Importe.Should().Be(15m);
        // Cobrado el resto: solo queda la otra mitad.
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = fb2.Id, Importe = fb2.Total - fb2.Total / 2 })).EnsureSuccessStatusCode();
        (await OkAsync<CalculoResp>(api.GetAsync($"/comisiones/calculo/{lucia}?desde={Hoy.AddDays(-30):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"))).Lineas.Single()
            .Should().Match<LineaResp>(l => l.YaLiquidado == 15m && l.Pendiente == 15m);

        // Anular la del representante anula su factura y deja las comisiones pendientes otra vez.
        (await api.PostAsync(new Uri($"/comisiones/liquidaciones/{liq.Id}/anular", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await OkAsync<List<GastoResp>>(api.GetAsync("/gastos"))).Single(g => g.Id == liq.GastoId).Estado.Should().Be("Anulado");
        (await OkAsync<CalculoResp>(api.GetAsync($"/comisiones/calculo/{rep}?desde={Hoy.AddDays(-60):yyyy-MM-dd}&hasta={Hoy:yyyy-MM-dd}"))).Pendiente.Should().Be(26m);
        (await FalloAsync(api.PostAsync(new Uri($"/comisiones/liquidaciones/{liq.Id}/anular", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Should().Be("liquidacion_agente.anulada");
    }
}
