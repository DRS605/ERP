using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Venta en comisión: la liquidación del cliente (lo vendido, el precio bruto, su comisión y sus gastos) valora los albaranes
/// enviados a precio por fijar, a neto o a bruto con la factura de gastos del comisionista, y se anula devolviendo el precio
/// estimado. La rentabilidad da el precio neto de lo enviado.
/// </summary>
public sealed class LiquidacionesComisionTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public LiquidacionesComisionTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Dia = new(DateTime.UtcNow.Year, 2, 10);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaAlbaranResp(int Orden, decimal Cantidad, decimal PrecioUnitario, bool PrecioFijado);
    private sealed record AlbaranResp(Guid Id, string NumeroCompleto, string Estado, decimal Base, List<LineaAlbaranResp> Lineas);
    private sealed record PendienteResp(Guid AlbaranId, int Orden, decimal Cantidad, decimal PrecioEstimado);
    private sealed record LineaLiqResp(Guid AlbaranId, decimal CantidadVendida, decimal Merma, decimal ImporteBruto, decimal Gastos, decimal ImporteNeto, decimal PrecioAlbaran,
        decimal? DiferenciaEstimado);
    private sealed record LiquidacionResp(Guid Id, string? Numero, string Estado, string Modo, Guid? GastoId, decimal ImporteBruto, decimal TotalGastos, decimal ImporteNeto,
        decimal? PorcentajeGastos, List<LineaLiqResp> Lineas);
    private sealed record RentabilidadResp(int Liquidaciones, decimal CantidadEnviada, decimal CantidadVendida, decimal PorcentajeMerma, decimal ImporteBruto, decimal Gastos,
        decimal ImporteNeto, decimal? PrecioBrutoMedio, decimal? PrecioNetoMedio, decimal? PrecioNetoPorEnviado);
    private sealed record LineaGastoResp(string? CuentaGasto, decimal Base);
    private sealed record GastoResp(Guid Id, decimal BaseImponible, string Estado, string? NumeroFactura, List<LineaGastoResp> Lineas);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

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

    private static Task<AlbaranResp> AlbaranAsync(HttpClient api, Guid cliente, Guid producto, decimal kilos, bool porFijar = true) =>
        OkAsync<AlbaranResp>(api.PostAsJsonAsync("/albaranes-venta", new
        {
            ClienteId = cliente, Fecha = Dia, Lineas = new[] { new { ProductoId = producto, Cantidad = kilos, PrecioUnitario = 0.50m, PrecioPorFijar = porFijar } },
        }));

    private static Task<AlbaranResp> VerAsync(HttpClient api, Guid id) => OkAsync<AlbaranResp>(api.GetAsync($"/albaranes-venta/{id}"));

    [Fact]
    public async Task La_liquidacion_del_comisionista_valora_a_neto_se_anula_y_da_la_rentabilidad()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Fruchthandel GmbH" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel cal. 3", PrecioUnitario = 0.60m, Tipo = "Bien", Unidad = "kg", CodigoIva = "IVA4" });
        var a1 = await AlbaranAsync(api, cliente, naranja, 1_000m);
        var a2 = await AlbaranAsync(api, cliente, naranja, 500m);
        var fijo = await AlbaranAsync(api, cliente, naranja, 100m, porFijar: false);

        var pendientes = await OkAsync<List<PendienteResp>>(api.GetAsync($"/liquidaciones-comision/pendientes?clienteId={cliente}"));
        pendientes.Select(p => p.AlbaranId).Should().BeEquivalentTo([a1.Id, a2.Id], "el valorado no está pendiente");

        object Cuerpo(string modo, object[] lineas, object[] gastos, Guid? proveedor = null) =>
            new { ClienteId = cliente, Fecha = Dia.AddDays(20), Modo = modo, ReferenciaCliente = "AS-4471", Lineas = lineas, Gastos = gastos, ProveedorId = proveedor };
        var lineas = new object[] { new { AlbaranId = a1.Id, Orden = 1, CantidadVendida = 950m, PrecioBruto = 0.80m }, new { AlbaranId = a2.Id, Orden = 1, CantidadVendida = 500m, PrecioBruto = 0.70m } };
        var gastos = new object[] { new { Tipo = "Comision", Porcentaje = 8m }, new { Tipo = "Transporte", Importe = 150m } };

        (await FalloAsync(api.PostAsJsonAsync("/liquidaciones-comision", Cuerpo("PrecioNeto", [new { AlbaranId = fijo.Id, Orden = 1, CantidadVendida = 1m, PrecioBruto = 1m }], gastos)),
            HttpStatusCode.Conflict)).Should().Be("liquidacion_comision.linea_no_pendiente");
        (await FalloAsync(api.PostAsJsonAsync("/liquidaciones-comision", Cuerpo("PrecioNeto", [new { AlbaranId = a1.Id, Orden = 1, CantidadVendida = 1_001m, PrecioBruto = 1m }], gastos)),
            HttpStatusCode.BadRequest)).Should().Be("liquidacion_comision.cantidad");
        (await FalloAsync(api.PostAsJsonAsync("/liquidaciones-comision", Cuerpo("BrutoConFacturaGastos", lineas, gastos)), HttpStatusCode.BadRequest))
            .Should().Be("liquidacion_comision.proveedor");

        // Bruto 1.110 € (950 × 0,80 + 500 × 0,70); comisión 8 % = 88,80 y portes 150: gastos 238,80 repartidos por el bruto.
        var liq = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/liquidaciones-comision", Cuerpo("PrecioNeto", lineas, gastos)));
        liq.Should().Match<LiquidacionResp>(l => l.Estado == "Borrador" && l.Numero == null && l.ImporteBruto == 1_110m && l.TotalGastos == 238.80m && l.ImporteNeto == 871.20m
            && l.PorcentajeGastos == 21.51m);
        var l1 = liq.Lineas.Single(x => x.AlbaranId == a1.Id);
        l1.Should().Match<LineaLiqResp>(x => x.Merma == 50m && x.Gastos == 163.50m && x.ImporteNeto == 596.50m && x.PrecioAlbaran == 0.5965m && x.DiferenciaEstimado == 0.0965m);
        liq.Lineas.Single(x => x.AlbaranId == a2.Id).PrecioAlbaran.Should().Be(0.5494m, "274,70 € entre 500 kg");

        var conf = await OkAsync<LiquidacionResp>(api.PostAsync(new Uri($"/liquidaciones-comision/{liq.Id}/confirmar", UriKind.Relative), null));
        conf.Should().Match<LiquidacionResp>(l => l.Estado == "Confirmada" && l.Numero == $"LC-{Dia.Year}-000001" && l.GastoId == null);
        var v1 = await VerAsync(api, a1.Id);
        v1.Should().Match<AlbaranResp>(a => a.Estado == "PendienteFacturar" && a.Base == 596.50m);
        v1.Lineas.Single().Should().Match<LineaAlbaranResp>(l => l.PrecioFijado && l.PrecioUnitario == 0.5965m);

        (await FalloAsync(api.PostAsJsonAsync("/liquidaciones-comision", Cuerpo("PrecioNeto", lineas, gastos)), HttpStatusCode.Conflict))
            .Should().BeOneOf("liquidacion_comision.linea_no_pendiente", "liquidacion_comision.linea_en_otra");
        (await FalloAsync(api.PutAsJsonAsync($"/albaranes-venta/{a1.Id}/valorar", new { Lineas = new[] { new { Orden = 1, PrecioUnitario = 2m } } }), HttpStatusCode.Conflict))
            .Should().Be("albaranventa.en_liquidacion_comision");
        (await FalloAsync(api.DeleteAsync(new Uri($"/liquidaciones-comision/{liq.Id}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("liquidacion_comision.no_borrador");
        (await FalloAsync(api.PostAsJsonAsync($"/albaranes-venta/{a2.Id}/anular", new { Motivo = "No se entregó" }), HttpStatusCode.Conflict))
            .Should().Be("liquidacion_comision.albaran", "la base de datos no deja anular un albarán liquidado");

        var rent = (await OkAsync<List<RentabilidadResp>>(api.GetAsync($"/liquidaciones-comision/rentabilidad?desde={Dia:yyyy-MM-dd}&hasta={Dia.AddDays(60):yyyy-MM-dd}"))).Single();
        rent.Should().Be(new RentabilidadResp(1, 1_500m, 1_450m, 3.33m, 1_110m, 238.80m, 871.20m, 0.7655m, 0.6008m, 0.5808m));

        // Anular: los albaranes vuelven a su precio estimado y a estar pendientes.
        (await FalloAsync(api.PostAsJsonAsync($"/liquidaciones-comision/{liq.Id}/anular", new { Motivo = "" }), HttpStatusCode.BadRequest)).Should().Be("liquidacion_comision.motivo");
        (await OkAsync<LiquidacionResp>(api.PostAsJsonAsync($"/liquidaciones-comision/{liq.Id}/anular", new { Motivo = "El cliente corrigió su liquidación" }))).Estado.Should().Be("Anulada");
        (await VerAsync(api, a1.Id)).Should().Match<AlbaranResp>(a => a.Estado == "PendienteValorar" && a.Lineas.Single().PrecioUnitario == 0.50m && !a.Lineas.Single().PrecioFijado);
        (await OkAsync<List<PendienteResp>>(api.GetAsync($"/liquidaciones-comision/pendientes?clienteId={cliente}"))).Should().HaveCount(2);
    }

    [Fact]
    public async Task A_bruto_los_gastos_son_una_factura_del_comisionista_y_lo_facturado_no_se_anula()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Primeur BV" });
        var comisionista = await IdAsync(api, "/proveedores", new { Nombre = "Primeur BV (comisionista)" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel cal. 4", PrecioUnitario = 0.60m, Tipo = "Bien", Unidad = "kg", CodigoIva = "IVA4" });
        var a1 = await AlbaranAsync(api, cliente, naranja, 1_000m);

        var liq = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/liquidaciones-comision", new
        {
            ClienteId = cliente, Fecha = Dia.AddDays(15), Modo = "BrutoConFacturaGastos", ReferenciaCliente = "PB-0098", ProveedorId = comisionista, CodigoIvaGastos = "IVA0",
            Lineas = new[] { new { AlbaranId = a1.Id, Orden = 1, CantidadVendida = 1_000m, PrecioBruto = 0.76m } },
            Gastos = new object[] { new { Tipo = "Comision", Porcentaje = 10m }, new { Tipo = "Transporte", Importe = 40m } },
        }));
        var conf = await OkAsync<LiquidacionResp>(api.PostAsync(new Uri($"/liquidaciones-comision/{liq.Id}/confirmar", UriKind.Relative), null));
        conf.GastoId.Should().NotBeNull();
        conf.Lineas.Single().PrecioAlbaran.Should().Be(0.76m, "a bruto el albarán lleva el precio de venta");
        var gasto = await OkAsync<GastoResp>(api.GetAsync($"/gastos/{conf.GastoId}"));
        gasto.Should().Match<GastoResp>(g => g.BaseImponible == 116m && g.NumeroFactura == "PB-0098");
        gasto.Lineas.Select(l => (l.CuentaGasto, l.Base)).Should().BeEquivalentTo([("623", 76m), ("624", 40m)]);

        // Facturado el albarán, la liquidación ya no se anula.
        (await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a1.Id } })).EnsureSuccessStatusCode();
        (await FalloAsync(api.PostAsJsonAsync($"/liquidaciones-comision/{liq.Id}/anular", new { Motivo = "error" }), HttpStatusCode.Conflict)).Should().Be("albaranventa.facturado");
        (await OkAsync<GastoResp>(api.GetAsync($"/gastos/{conf.GastoId}"))).Estado.Should().NotBe("Anulado");
    }
}
