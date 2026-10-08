using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Ampliación de los cargos y abonos: suplidos en las compras (fuera de la base de la factura del proveedor, sin
/// impuesto y a su cuenta), reglas por envase en las ventas, provisión contable del coste con acreedor, y conceptos en
/// tickets, facturas periódicas, rectificativas por diferencias y facturas del buzón.
/// </summary>
public sealed class CargosAbonosAmpliacionTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CargosAbonosAmpliacionTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, string Concepto, List<ApunteResp> Apuntes);
    private sealed record PedidoCompraResp(Guid Id, decimal Total, decimal Suplidos);
    private sealed record DesgloseResp(string CodigoIva, decimal Base, decimal Cuota);
    private sealed record LineaGastoResp(decimal Base, decimal Cuota, bool Suplido, string? CuentaGasto);
    private sealed record GastoResp(Guid Id, decimal BaseImponible, decimal CuotaIva, decimal Suplidos, decimal Total, List<LineaGastoResp> Lineas, List<DesgloseResp> Desglose,
        string? NumeroFactura);

    private static async Task<HttpClient> CompletaAsync(FabricaApiPruebas fabrica)
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        return api;
    }

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion, HttpStatusCode esperado = HttpStatusCode.OK)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(Task<HttpResponseMessage> peticion) => (await (await peticion).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static object Concepto(string codigo, string ambito, string efecto, string sentido, string calculo, decimal valor, object[]? asignaciones = null,
        Guid? acreedorId = null, string? cuenta = null) => new
        {
            Codigo = codigo,
            Datos = new
            {
                Nombre = codigo.ToLowerInvariant(), Ambito = ambito, Efecto = efecto, Sentido = sentido, Calculo = calculo, Valor = valor,
                Asignaciones = asignaciones ?? [new { }], AcreedorId = acreedorId, CuentaContable = cuenta,
            },
        };

    private static async Task<List<ApunteResp>> ApuntesAsync(HttpClient api, string origen) =>
        (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!.Where(a => a.Origen == origen).SelectMany(a => a.Apuntes).ToList();

    private static decimal Debe(IEnumerable<ApunteResp> apuntes, string cuenta) => apuntes.Where(a => a.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(a => a.Debe);

    private static decimal Haber(IEnumerable<ApunteResp> apuntes, string cuenta) => apuntes.Where(a => a.CuentaCodigo.StartsWith(cuenta, StringComparison.Ordinal)).Sum(a => a.Haber);

    [Fact]
    public async Task Un_suplido_de_compras_va_aparte_en_la_factura_del_proveedor_sin_impuesto_y_a_su_cuenta()
    {
        var api = await CompletaAsync(_fabrica);
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Transitario Puerto SL" });

        // Tasas portuarias que paga el transitario por la empresa: 2 €/ud, sin IVA, a la 631.
        (await CodigoAsync(api.PostAsJsonAsync("/conceptos-linea", Concepto("TASA0", "Compras", "Suplido", "Suma", "PorUnidad", 2m)))).Should().Be("concepto.suplido");
        await IdAsync(api, "/conceptos-linea", Concepto("TASAS", "Compras", "Suplido", "Suma", "PorUnidad", 2m, [new { TerceroId = proveedor }], cuenta: "631"));

        var pedido = await OkAsync<PedidoCompraResp>(api.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor, Lineas = new[] { new { Descripcion = "Palés de madera", Cantidad = 10m, PrecioUnitario = 10m } },
        }), HttpStatusCode.Created);
        pedido.Should().Match<PedidoCompraResp>(p => p.Total == 100m && p.Suplidos == 20m);
        (await api.GetAsync(new Uri($"/compras/pedidos/{pedido.Id}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/facturar", new { CodigoIva = "IVA21", NumeroFactura = "TP-1" })).StatusCode.Should().Be(HttpStatusCode.OK);

        // Base 100 + IVA 21 + suplidos 20 = 141; el libro de IVA solo ve la base de 100.
        var id = (await api.GetFromJsonAsync<List<GastoResp>>("/gastos"))!.Single(g => g.NumeroFactura == "TP-1").Id;
        var gasto = (await api.GetFromJsonAsync<GastoResp>($"/gastos/{id}"))!;
        gasto.Should().Match<GastoResp>(g => g.BaseImponible == 100m && g.CuotaIva == 21m && g.Suplidos == 20m && g.Total == 141m);
        gasto.Lineas.Should().ContainSingle(l => l.Suplido).Which.Should().Be(new LineaGastoResp(20m, 0m, true, "631"));
        gasto.Desglose.Should().ContainSingle().Which.Should().Be(new DesgloseResp("IVA21", 100m, 21m));

        var apuntes = await ApuntesAsync(api, "Compra");
        Debe(apuntes, "631").Should().Be(20m);
        Debe(apuntes, "472").Should().Be(21m);
        Debe(apuntes, "629").Should().Be(100m);
        Haber(apuntes, "400").Should().Be(141m);

        // Una factura recibida a mano también admite la línea de suplido (con su cuenta).
        (await CodigoAsync(api.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Despacho", BaseImponible = 0m, ProveedorId = proveedor, NumeroFactura = "TP-2",
            Lineas = new object[] { new { Base = 50m, CodigoIva = "IVA21" }, new { Base = 10m, Suplido = true } },
        }))).Should().Be("gasto.suplido");
        var directo = await OkAsync<GastoResp>(api.PostAsJsonAsync("/gastos", new
        {
            Concepto = "Despacho", BaseImponible = 0m, ProveedorId = proveedor, NumeroFactura = "TP-3", PorcentajeIrpf = 1m,
            Lineas = new object[] { new { Base = 50m, CodigoIva = "IVA21" }, new { Base = 10m, Suplido = true, CuentaGasto = "4709" } },
        }), HttpStatusCode.Created);
        directo.Should().Match<GastoResp>(g => g.BaseImponible == 50m && g.Suplidos == 10m && g.Total == 70m, "50 + 10,50 de IVA − 0,50 de retención + 10");
    }

    private sealed record ConceptoResp(string Codigo, decimal Valor, decimal Importe);
    private sealed record LineaVentaResp(decimal Base, Guid? EnvaseProductoId, List<ConceptoResp> Conceptos);
    private sealed record FacturaVentaResp(Guid Id, decimal BaseImponible, List<LineaVentaResp> Lineas);
    private sealed record LineaPedidoResp(Guid Id, Guid? EnvaseProductoId);
    private sealed record PedidoVentaResp(Guid Id, List<LineaPedidoResp> Lineas);
    private sealed record AlbaranVentaResp(Guid Id, List<LineaVentaResp> Lineas);
    private sealed record SugeridoResp(string Codigo, decimal Valor);

    [Fact]
    public async Task Las_reglas_por_envase_solo_se_ponen_en_las_lineas_de_ese_envase()
    {
        var api = await CompletaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Norte SA" });
        var ifco = await IdAsync(api, "/productos", new { Nombre = "Caja IFCO 6420", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var carton = await IdAsync(api, "/productos", new { Nombre = "Caja cartón 10 kg", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });

        // Recargo de 0,05 €/ud solo en IFCO; descuento del 1 % general y del 3 % en cartón (la regla del envase gana).
        await IdAsync(api, "/conceptos-linea", Concepto("RECIFCO", "Ventas", "Precio", "Suma", "PorUnidad", 0.05m, [new { EnvaseProductoId = ifco }]));
        await IdAsync(api, "/conceptos-linea", Concepto("DTOENV", "Ventas", "Precio", "Resta", "Porcentaje", 1m, [new { }, new { EnvaseProductoId = carton, Valor = 3m }]));
        (await CodigoAsync(api.PostAsJsonAsync("/conceptos-linea", Concepto("MAL", "Ventas", "Precio", "Suma", "PorUnidad", 1m, [new { EnvaseProductoId = Guid.NewGuid() }]))))
            .Should().Be("concepto.envase_no_encontrado");

        (await api.GetFromJsonAsync<List<SugeridoResp>>($"/conceptos-linea/sugeridos?terceroId={cliente}&envaseProductoId={ifco}"))!.Select(x => x.Codigo)
            .Should().BeEquivalentTo(["RECIFCO", "DTOENV"]);
        (await api.GetFromJsonAsync<List<SugeridoResp>>($"/conceptos-linea/sugeridos?terceroId={cliente}&envaseProductoId={carton}"))!
            .Should().ContainSingle().Which.Should().Be(new SugeridoResp("DTOENV", 3m));

        object Linea(Guid? envase) => new { Descripcion = "Naranja", Cantidad = 100m, PrecioUnitario = 1m, CodigoIva = "IVA4", EnvaseProductoId = envase };
        var f = await OkAsync<FacturaVentaResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { Linea(ifco), Linea(carton), Linea(null) } }),
            HttpStatusCode.Created);
        f.Lineas.Select(l => (l.EnvaseProductoId, l.Base, string.Join(",", l.Conceptos.Select(c => $"{c.Codigo}:{c.Importe}")))).Should().Equal(
            ((Guid?)ifco, 104m, "DTOENV:-1.00,RECIFCO:5.00"), ((Guid?)carton, 97m, "DTOENV:-3.00"), ((Guid?)null, 99m, "DTOENV:-1.00"));

        // El envase del pedido pasa al albarán y a la factura.
        var pedido = await OkAsync<PedidoVentaResp>(api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Lineas = new[] { Linea(ifco) } }), HttpStatusCode.Created);
        pedido.Lineas.Single().EnvaseProductoId.Should().Be(ifco);
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var albaran = await OkAsync<AlbaranVentaResp>(api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/entregar",
            new { Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 100m } } }));
        albaran.Lineas.Single().Should().Match<LineaVentaResp>(l => l.EnvaseProductoId == ifco && l.Base == 104m);
        var fp = await OkAsync<FacturaVentaResp>(api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { }), HttpStatusCode.Created);
        fp.Lineas.Single().Should().Match<LineaVentaResp>(l => l.EnvaseProductoId == ifco && l.Base == 104m);
    }

    private sealed record ConceptoProvResp(string Codigo, decimal Importe, bool Provisionado);
    private sealed record LineaProvResp(List<ConceptoProvResp> Conceptos);
    private sealed record FacturaProvResp(Guid Id, List<LineaProvResp> Lineas);
    private sealed record CargoResp(string Origen, decimal Importe, bool Provisionado, string Clave);
    private sealed record LiquidacionResp(Guid GastoId, int Cargos, decimal Base);

    private static async Task<decimal> SaldoCuentaAsync(HttpClient api, string cuenta)
    {
        var apuntes = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!.SelectMany(a => a.Apuntes).ToList();
        return Debe(apuntes, cuenta) - Haber(apuntes, cuenta);
    }

    [Fact]
    public async Task El_coste_con_acreedor_se_provisiona_en_la_venta_y_la_factura_del_acreedor_cancela_la_provision()
    {
        var api = await CompletaAsync(_fabrica);
        var transportista = await IdAsync(api, "/proveedores", new { Nombre = "Transportes Levante SL" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Norte SA" });
        await IdAsync(api, "/conceptos-linea", Concepto("PORTES", "Ventas", "Coste", "Suma", "PorUnidad", 0.10m, acreedorId: transportista, cuenta: "624"));
        object Lineas(decimal cantidad) => new[] { new { Descripcion = "Naranja", Cantidad = cantidad, PrecioUnitario = 1m, CodigoIva = "IVA4" } };

        // Factura de 100 ud: 10 € de portes provisionados (624 a 4009).
        var f = await OkAsync<FacturaProvResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = Lineas(100m) }), HttpStatusCode.Created);
        f.Lineas.Single().Conceptos.Single().Should().Be(new ConceptoProvResp("PORTES", 10m, true));
        var provision = (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={DateTime.Today.Year}"))!
            .Where(a => a.Concepto.StartsWith("Provisión", StringComparison.Ordinal)).SelectMany(a => a.Apuntes).ToList();
        Debe(provision, "624").Should().Be(10m);
        Haber(provision, "4009").Should().Be(10m);

        // Albarán de 50 ud (5 € de portes) que se liquida antes de facturarlo: al facturarlo ya no se provisiona.
        var alb = await IdAsync(api, "/albaranes-venta", new { ClienteId = cliente, Lineas = Lineas(50m) });
        var deAlbaran = (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!.Single(c => c.Origen == "AlbaranVenta");
        deAlbaran.Provisionado.Should().BeFalse();
        await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/gastos/cargos-acreedores/liquidar", new { AcreedorId = transportista, NumeroFactura = "TL-1", Claves = new[] { deAlbaran.Clave } }), HttpStatusCode.Created);
        var fa = await OkAsync<FacturaProvResp>(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { alb } }), HttpStatusCode.Created);
        fa.Lineas.Single().Conceptos.Single().Provisionado.Should().BeFalse();

        // La factura del transportista por el cargo provisionado lo carga a la 4009: la provisión queda cancelada.
        var pendiente = (await api.GetFromJsonAsync<List<CargoResp>>($"/gastos/cargos-acreedores?acreedorId={transportista}"))!.Should().ContainSingle().Subject;
        pendiente.Should().Match<CargoResp>(c => c.Origen == "FacturaVenta" && c.Provisionado);
        await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/gastos/cargos-acreedores/liquidar", new { AcreedorId = transportista, NumeroFactura = "TL-2" }), HttpStatusCode.Created);

        (await SaldoCuentaAsync(api, "4009")).Should().Be(0m);
        (await SaldoCuentaAsync(api, "624")).Should().Be(15m, "10 € provisionados en la venta y 5 € de la factura del albarán ya liquidado");
    }

    private sealed record TicketResp(Guid Id, decimal BaseImponible, List<LineaVentaResp> Lineas);
    private sealed record ConceptoRecResp(Guid ConceptoId, decimal? Valor);
    private sealed record LineaRecResp(List<ConceptoRecResp>? Conceptos);
    private sealed record RecurrenteResp(Guid Id, List<LineaRecResp> Lineas);
    private sealed record ProcesoResp(int Emitidas, List<Guid> FacturasCreadas);
    private sealed record RecibidaResp(Guid Id, Guid? GastoId, List<ConceptoResp> Conceptos);

    [Fact]
    public async Task Los_conceptos_llegan_a_tickets_plantillas_periodicas_rectificativas_y_facturas_del_buzon()
    {
        var api = await CompletaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutería Central" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud" });
        // Automáticos para cualquiera: bolsa de 0,10 € por línea y portes de 0,05 €/ud. Descuento del 5 %, solo a mano.
        var bolsa = await IdAsync(api, "/conceptos-linea", Concepto("BOLSA", "Ventas", "Precio", "Suma", "Importe", 0.10m));
        await IdAsync(api, "/conceptos-linea", Concepto("PORTUD", "Ventas", "Precio", "Suma", "PorUnidad", 0.05m));
        var dto = await IdAsync(api, "/conceptos-linea", Concepto("DTO5", "Ventas", "Precio", "Resta", "Porcentaje", 5m, []));

        // Ticket de contado: lleva los automáticos.
        var t = await OkAsync<TicketResp>(api.PostAsJsonAsync("/tickets", new { Lineas = new[] { new { ProductoId = naranja, Cantidad = 10m, PrecioUnitario = 1m, CodigoIva = "IVA4" } } }),
            HttpStatusCode.Created);
        t.BaseImponible.Should().Be(10.60m, "10 € + bolsa 0,10 + portes 0,50");

        // Plantilla periódica con conceptos propios en la línea: solo el descuento (sin los automáticos).
        var hoy = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");
        var rec = await OkAsync<RecurrenteResp>(api.PostAsJsonAsync("/facturas-recurrentes", new
        {
            Nombre = "Suministro semanal", ClienteId = cliente, Periodicidad = "Mensual", PrimeraEmision = hoy,
            Lineas = new[] { new { ProductoId = naranja, Cantidad = 100m, PrecioUnitario = 1m, CodigoIva = "IVA4", Conceptos = new[] { new { ConceptoId = dto } } } },
        }), HttpStatusCode.Created);
        rec.Lineas.Single().Conceptos.Should().ContainSingle().Which.ConceptoId.Should().Be(dto);
        (await CodigoAsync(api.PostAsJsonAsync("/facturas-recurrentes", new
        {
            Nombre = "Mal", ClienteId = cliente, Periodicidad = "Mensual", PrimeraEmision = hoy,
            Lineas = new[] { new { Descripcion = "X", Cantidad = 1m, PrecioUnitario = 1m, CodigoIva = "IVA4", Conceptos = new[] { new { ConceptoId = Guid.NewGuid() } } } },
        }))).Should().Be("concepto.no_encontrado");
        var proceso = await OkAsync<ProcesoResp>(api.PostAsync(new Uri("/facturas-recurrentes/procesar", UriKind.Relative), null));
        var periodica = (await api.GetFromJsonAsync<FacturaVentaResp>($"/facturas/{proceso.FacturasCreadas.Single()}"))!;
        periodica.Lineas.Single().Conceptos.Select(c => c.Codigo).Should().Equal("DTO5");
        periodica.BaseImponible.Should().Be(95m);

        // Factura de 100 ud con los automáticos y su rectificativa (por sustitución) a 80 ud: los conceptos de la original se
        // recalculan sobre la línea corregida.
        var f = await OkAsync<FacturaVentaResp>(api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = naranja, Cantidad = 100m, PrecioUnitario = 1m, CodigoIva = "IVA4" } } }),
            HttpStatusCode.Created);
        f.BaseImponible.Should().Be(105.10m);
        var rect = await OkAsync<FacturaVentaResp>(api.PostAsJsonAsync($"/facturas/{f.Id}/rectificar", new
        {
            Motivo = "Eran 80 ud", Lineas = new[] { new { ProductoId = naranja, Cantidad = 80m, PrecioUnitario = 1m, CodigoIva = "IVA4" } },
        }), HttpStatusCode.Created);
        rect.Lineas.Single().Conceptos.Select(c => (c.Codigo, c.Importe)).Should().BeEquivalentTo(new[] { ("BOLSA", 0.10m), ("PORTUD", 4m) });
        rect.BaseImponible.Should().Be(84.10m);

        // Buzón: la factura recibida con portes a la 624 y un suplido a la 631.
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Suministros Ebro SL" });
        var portes = await IdAsync(api, "/conceptos-linea", Concepto("PORTESC", "Compras", "Precio", "Suma", "Importe", 15m, [], cuenta: "624"));
        var tasa = await IdAsync(api, "/conceptos-linea", Concepto("TASAC", "Compras", "Suplido", "Suma", "Importe", 7m, [], cuenta: "631"));
        var pdf = Convert.ToBase64String("%PDF"u8.ToArray());
        var recibida = await OkAsync<RecibidaResp>(api.PostAsJsonAsync("/recepcion/facturas", new { NombreArchivo = "f.pdf", ContenidoBase64 = pdf, TipoContenido = "application/pdf" }),
            HttpStatusCode.Created);
        var validada = await OkAsync<RecibidaResp>(api.PostAsJsonAsync($"/recepcion/facturas/{recibida.Id}/validar", new
        {
            BaseImponible = 300m, FechaFactura = hoy, ProveedorId = proveedor, NumeroFactura = "E-1", CodigoIva = "IVA21",
            Conceptos = new[] { new { ConceptoId = portes }, new { ConceptoId = tasa } },
        }));
        validada.Conceptos.Select(c => (c.Codigo, c.Importe)).Should().BeEquivalentTo(new[] { ("PORTESC", 15m), ("TASAC", 7m) });
        var contabilizada = await OkAsync<RecibidaResp>(api.PostAsync(new Uri($"/recepcion/facturas/{recibida.Id}/contabilizar", UriKind.Relative), null));
        var gasto = (await api.GetFromJsonAsync<GastoResp>($"/gastos/{contabilizada.GastoId}"))!;
        gasto.Should().Match<GastoResp>(g => g.BaseImponible == 315m && g.CuotaIva == 66.15m && g.Suplidos == 7m && g.Total == 388.15m);
        gasto.Lineas.Select(l => (l.Base, l.CuentaGasto, l.Suplido)).Should().BeEquivalentTo(new[] { (300m, (string?)null, false), (15m, (string?)"624", false), (7m, (string?)"631", true) });
    }
}
