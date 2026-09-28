using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Cargos y abonos al estilo Hispatec sobre los conceptos de línea: la regla del tercero manda sobre la del artículo
/// (cliente, tipo de cliente, general; y dentro, artículo, familia, todos) con vigencia; porcentajes en cascada por orden;
/// conceptos en los albaranes que pasan a la factura; cargos con acreedor pendientes y su liquidación en la factura del
/// acreedor; y la cuenta contable propia de un concepto en el asiento.
/// </summary>
public sealed class CargosAbonosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CargosAbonosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SugeridoResp(string Codigo, decimal Valor, Guid? AcreedorId);
    private sealed record ConceptoResp(string Codigo, string Efecto, decimal Valor, decimal Importe, Guid? AcreedorId);
    private sealed record LineaResp(decimal Base, List<ConceptoResp> Conceptos, Guid? AlbaranVentaId);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, decimal BaseImponible, List<LineaResp> Lineas);
    private sealed record LineaAlbaranResp(decimal Base, List<ConceptoResp> Conceptos, decimal CosteConceptos);
    private sealed record AlbaranResp(Guid Id, List<LineaAlbaranResp> Lineas);
    private sealed record CargoResp(string Origen, Guid DocumentoId, string Codigo, decimal Importe, Guid AcreedorId, string AcreedorNombre, string Clave);
    private sealed record LiquidacionResp(Guid GastoId, int Cargos, decimal Base, decimal Total);
    private sealed record GastoResp(Guid Id, decimal BaseImponible, decimal RetencionIrpf, decimal Total, string? NumeroFactura);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, string Concepto, List<ApunteResp> Apuntes);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static object Concepto(string codigo, string efecto, string sentido, string calculo, decimal valor, object[]? asignaciones = null, int orden = 0,
        string basePorcentaje = "Linea", Guid? acreedorId = null, string? cuenta = null) =>
        new
        {
            Codigo = codigo,
            Datos = new
            {
                Nombre = codigo.ToLowerInvariant(), Ambito = "Ventas", Efecto = efecto, Sentido = sentido, Calculo = calculo, Valor = valor, Asignaciones = asignaciones,
                Orden = orden, BasePorcentaje = basePorcentaje, AcreedorId = acreedorId, CuentaContable = cuenta,
            },
        };

    [Fact]
    public async Task Manda_la_regla_del_tercero_y_despues_la_del_articulo()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var pepino = await IdAsync(api, "/productos", new { Nombre = "Pepino", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var concreto = await IdAsync(api, "/clientes", new { Nombre = "Cliente concreto" });
        var mayorista = await IdAsync(api, "/clientes", new { Nombre = "Mayorista SA", Tipo = "Mayorista" });
        var otro = await IdAsync(api, "/clientes", new { Nombre = "Otro cliente" });
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Comisión: 5 % para el tomate (cualquier cliente), 2 % para el cliente concreto (todos los artículos) y 4 % para los
        // mayoristas este mes.
        await IdAsync(api, "/conceptos-linea", Concepto("COMIS", "Coste", "Suma", "Porcentaje", 1m,
        [
            new { ProductoId = tomate, Valor = 5m },
            new { TerceroId = concreto, Valor = 2m },
            new { TipoTercero = "Mayorista", Valor = 4m, Desde = hoy.AddDays(-5), Hasta = hoy.AddDays(5) },
        ]));

        async Task<decimal?> ValorAsync(Guid cliente, Guid producto, DateOnly? fecha = null) =>
            (await api.GetFromJsonAsync<List<SugeridoResp>>($"/conceptos-linea/sugeridos?ambito=Ventas&terceroId={cliente}&productoId={producto}"
                + (fecha is { } f ? $"&fecha={f:yyyy-MM-dd}" : string.Empty)))!.SingleOrDefault(s => s.Codigo == "COMIS")?.Valor;

        (await ValorAsync(concreto, tomate)).Should().Be(2m, "la regla del cliente gana a la del artículo (en ALXOR antes ganaba el artículo)");
        (await ValorAsync(mayorista, tomate)).Should().Be(4m, "el tipo de cliente va antes que «cualquier cliente»");
        (await ValorAsync(mayorista, tomate, hoy.AddDays(30))).Should().Be(5m, "fuera de su vigencia, la regla del tipo no vale");
        (await ValorAsync(otro, tomate)).Should().Be(5m);
        (await ValorAsync(otro, pepino)).Should().BeNull("ninguna regla vale para otro cliente y otro artículo");

        // La factura aplica la misma jerarquía (el mayorista, a su 4 %).
        var r = await api.PostAsJsonAsync("/facturas", new { ClienteId = mayorista, Lineas = new[] { new { ProductoId = tomate, Cantidad = 100m, PrecioUnitario = 1m, CodigoIva = "IVA4" } } });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        (await r.Content.ReadFromJsonAsync<FacturaResp>())!.Lineas[0].Conceptos.Single().Importe.Should().Be(4m);

        // Una regla con la fecha final antes que la inicial no se admite.
        var mala = await api.PostAsJsonAsync("/conceptos-linea", Concepto("MAL", "Coste", "Suma", "Porcentaje", 1m, [new { Desde = hoy, Hasta = hoy.AddDays(-1) }]));
        (await mala.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("concepto.vigencia");
    }

    [Fact]
    public async Task Los_porcentajes_en_cascada_van_sobre_la_linea_y_los_cargos_anteriores()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Cliente cascada" });
        var recargo = await IdAsync(api, "/conceptos-linea", Concepto("RECARGO", "Precio", "Suma", "Porcentaje", 10m, orden: 1));
        var seguro = await IdAsync(api, "/conceptos-linea", Concepto("SEGURO", "Precio", "Suma", "Porcentaje", 5m, orden: 2, basePorcentaje: "Cascada"));

        // Se piden al revés: se aplican por su orden. Recargo 10 % de 100 = 10; seguro 5 % de 110 = 5,50.
        var r = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Descripcion = "Servicio", Cantidad = 1m, PrecioUnitario = 100m, CodigoIva = "IVA21", Conceptos = new[] { new { ConceptoId = seguro }, new { ConceptoId = recargo } } } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var linea = (await r.Content.ReadFromJsonAsync<FacturaResp>())!.Lineas.Single();
        linea.Conceptos.Select(c => (c.Codigo, c.Importe)).Should().Equal(("RECARGO", 10m), ("SEGURO", 5.50m));
        linea.Base.Should().Be(115.50m);
    }

    [Fact]
    public async Task Los_cargos_con_acreedor_se_liquidan_en_su_factura()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var transportista = await IdAsync(api, "/proveedores", new { Nombre = "Transportes Levante SL", NifFiscal = "B87654321" });
        var otroTransportista = await IdAsync(api, "/proveedores", new { Nombre = "Frío Rápido SL" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Norte SA" });
        var norte = await IdAsync(api, "/clientes", new { Nombre = "Cliente ruta norte" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate rama", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });

        // Portes pagados al transportista: 0,05 €/kg, solo coste. Para el cliente de la ruta norte, otro transportista.
        await IdAsync(api, "/conceptos-linea", Concepto("PORTES", "Coste", "Suma", "PorKilo", 0.05m,
            [new { }, new { TerceroId = norte, AcreedorId = otroTransportista }], acreedorId: transportista));

        // Albarán de 1.000 kg (portes 50 €) y factura directa de 600 kg (portes 30 €).
        var albaran = await api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Cantidad = 1000m, PrecioUnitario = 1.2m } } });
        albaran.StatusCode.Should().Be(HttpStatusCode.Created, await albaran.Content.ReadAsStringAsync());
        var alb = (await albaran.Content.ReadFromJsonAsync<AlbaranResp>())!;
        alb.Lineas[0].Should().Match<LineaAlbaranResp>(l => l.CosteConceptos == 50m && l.Base == 1200m && l.Conceptos.Single().AcreedorId == transportista);
        await IdAsync(api, "/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Cantidad = 600m, PrecioUnitario = 1.2m } } });
        await IdAsync(api, "/facturas", new { ClienteId = norte, Lineas = new[] { new { ProductoId = tomate, Cantidad = 200m, PrecioUnitario = 1.2m } } });

        // Al facturar el albarán, la factura lleva sus conceptos, pero el cargo es uno solo (el del albarán).
        var factura = (await (await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { alb.Id } })).Content.ReadFromJsonAsync<FacturaResp>())!;
        factura.Lineas[0].Conceptos.Single().Should().Match<ConceptoResp>(c => c.Importe == 50m && c.AcreedorId == transportista);

        var pendientes = (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!;
        pendientes.Select(c => (c.Origen, c.Importe)).Should().BeEquivalentTo([("AlbaranVenta", 50m), ("FacturaVenta", 30m)]);
        pendientes.Should().OnlyContain(c => c.AcreedorNombre == "Transportes Levante SL");
        (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={otroTransportista}"))!
            .Should().ContainSingle().Which.Importe.Should().Be(10m, "la regla del cliente cambia el acreedor");

        // Liquidación: la factura del transportista con los dos cargos (80 € + IVA 21 % − retención 1 %).
        var r = await api.PostAsJsonAsync("/gastos/cargos-acreedores/liquidar", new { AcreedorId = transportista, NumeroFactura = "TL-2026-77", PorcentajeIrpf = 1m });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var liq = (await r.Content.ReadFromJsonAsync<LiquidacionResp>())!;
        liq.Should().Match<LiquidacionResp>(l => l.Cargos == 2 && l.Base == 80m && l.Total == 96m);
        var gasto = (await api.GetFromJsonAsync<GastoResp>($"/gastos/{liq.GastoId}"))!;
        gasto.Should().Match<GastoResp>(g => g.NumeroFactura == "TL-2026-77" && g.RetencionIrpf == 0.8m);

        (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!.Should().BeEmpty();
        (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}&todos=true"))!.Should().HaveCount(2);
        var otraVez = await api.PostAsJsonAsync("/gastos/cargos-acreedores/liquidar", new { AcreedorId = transportista, NumeroFactura = "TL-2026-78" });
        (await otraVez.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("acreedor.sin_cargos");

        // Anulada la factura del acreedor, los cargos vuelven a estar pendientes.
        (await api.PostAsJsonAsync($"/gastos/{liq.GastoId}/anular", new { Motivo = "Error" })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!.Should().HaveCount(2);

        // Liquidar solo uno por su clave.
        var clave = (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!.Single(c => c.Origen == "FacturaVenta").Clave;
        var uno = await api.PostAsJsonAsync("/gastos/cargos-acreedores/liquidar", new { AcreedorId = transportista, NumeroFactura = "TL-2026-79", Claves = new[] { clave } });
        (await uno.Content.ReadFromJsonAsync<LiquidacionResp>())!.Base.Should().Be(30m);
    }

    [Fact]
    public async Task Un_concepto_con_cuenta_propia_se_contabiliza_en_ella()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Cliente portes" });
        // Portes cobrados al cliente (20 € por línea) a la 7590 «Ingresos por portes».
        await IdAsync(api, "/conceptos-linea", Concepto("PORTESCOB", "Precio", "Suma", "Importe", 20m, [new { }], cuenta: "7590"));

        var f = (await (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Descripcion = "Mercancía", Cantidad = 1m, PrecioUnitario = 100m, CodigoIva = "IVA21" } } }))
            .Content.ReadFromJsonAsync<FacturaResp>())!;
        f.BaseImponible.Should().Be(120m);
        var asiento = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!.Single(a => a.Origen == "Venta");
        asiento.Apuntes.Where(a => a.CuentaCodigo.StartsWith("759", StringComparison.Ordinal)).Sum(a => a.Haber).Should().Be(20m);
        asiento.Apuntes.Where(a => a.CuentaCodigo.StartsWith("70", StringComparison.Ordinal)).Sum(a => a.Haber).Should().Be(100m);
        asiento.Apuntes.Where(a => a.CuentaCodigo.StartsWith("430", StringComparison.Ordinal)).Sum(a => a.Debe).Should().Be(145.20m);
    }
}
