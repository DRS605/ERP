using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Conceptos de línea: recargos, bonificaciones y costes que se ponen solos o a mano en las líneas de ventas y
/// compras, repartidos desde el documento, copiados de un documento a otro, y su efecto en importe, margen y coste
/// de entrada en almacén.
/// </summary>
public sealed class ConceptosLineaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ConceptosLineaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ConceptoResp(string Codigo, string Efecto, decimal Valor, decimal Importe, bool Repartido);
    private sealed record LineaFacturaResp(string Descripcion, decimal Base, decimal CosteUnitario, decimal Margen, List<ConceptoResp> Conceptos, decimal ImporteConceptos, decimal CosteConceptos);
    private sealed record FacturaResp(Guid Id, decimal BaseImponible, decimal Total, List<LineaFacturaResp> Lineas);
    private sealed record SugeridoResp(string Codigo, decimal Valor);
    private sealed record LineaPedidoVentaResp(decimal Base, List<ConceptoResp> Conceptos);
    private sealed record PedidoVentaResp(Guid Id, decimal Total, List<LineaPedidoVentaResp> Lineas);
    private sealed record LineaPedidoResp(Guid Id, decimal Importe, decimal CosteConceptos, decimal CosteUnitarioEntrada, List<ConceptoResp> Conceptos);
    private sealed record PedidoResp(Guid Id, decimal Total, List<LineaPedidoResp> Lineas);
    private sealed record ResumenResp(string Codigo, string Efecto, decimal Ventas, decimal Compras);
    private sealed record InformeResp(List<ResumenResp> Conceptos, decimal PrecioVentas, decimal CosteVentas, decimal PrecioCompras, decimal CosteCompras,
        decimal PrecioSinFacturar = 0m);
    private sealed record ValLineaResp(Guid ProductoId, decimal CosteUnitario);
    private sealed record ValoracionResp(List<ValLineaResp> Lineas);
    private sealed record BajaResp(bool Eliminado, bool Activo);
    private sealed record ConceptoMaestroResp(Guid Id, bool Activo);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static object Concepto(string codigo, string ambito, string efecto, string sentido, string calculo, decimal valor, object[]? asignaciones = null, string reparto = "PorImporte") =>
        new { Codigo = codigo, Datos = new { Nombre = codigo.ToLowerInvariant(), Ambito = ambito, Efecto = efecto, Sentido = sentido, Calculo = calculo, Valor = valor, Reparto = reparto, Asignaciones = asignaciones } };

    [Fact]
    public async Task La_factura_lleva_los_conceptos_automaticos_y_los_del_documento_repartidos()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 2m, PrecioCompra = 1.2m, Tipo = "Bien", Unidad = "kg" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Mercados del Norte SA" });
        await IdAsync(api, "/conceptos-linea", Concepto("PORTES", "Ventas", "Precio", "Suma", "PorKilo", 0.10m, [new { TerceroId = cliente }]));
        await IdAsync(api, "/conceptos-linea", Concepto("RAPPEL", "Ventas", "Precio", "Resta", "Porcentaje", 3m, [new { }]));
        await IdAsync(api, "/conceptos-linea", Concepto("COMIS", "Ventas", "Coste", "Suma", "Porcentaje", 5m, [new { ProductoId = naranja }]));
        var envase = await IdAsync(api, "/conceptos-linea", Concepto("ENVASE", "Ambos", "Precio", "Suma", "Importe", 30m));
        var soloCompras = await IdAsync(api, "/conceptos-linea", Concepto("ARANCEL", "Compras", "Coste", "Suma", "Porcentaje", 4m));

        (await api.GetFromJsonAsync<List<SugeridoResp>>($"/conceptos-linea/sugeridos?ambito=Ventas&terceroId={cliente}&productoId={naranja}"))!
            .Select(s => s.Codigo).Should().Equal("COMIS", "PORTES", "RAPPEL");

        var r = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new object[]
            {
                new { ProductoId = naranja, Cantidad = 100m },
                new { Descripcion = "Manipulado", Cantidad = 1m, PrecioUnitario = 100m },
            },
            ConceptosDocumento = new[] { new { ConceptoId = envase } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var f = (await r.Content.ReadFromJsonAsync<FacturaResp>())!;

        // Línea 1: 200 + portes 10 − rappel 6 + envase 20 (reparto 200/300 de 30); comisión 10 al coste.
        var l1 = f.Lineas[0];
        l1.Conceptos.Select(c => (c.Codigo, c.Importe)).Should().Equal(("COMIS", 10m), ("PORTES", 10m), ("RAPPEL", -6m), ("ENVASE", 20m));
        l1.Base.Should().Be(224m);
        l1.ImporteConceptos.Should().Be(24m);
        l1.CosteConceptos.Should().Be(10m);
        l1.Margen.Should().Be(224m - 120m - 10m, "la comisión es coste de la venta, no está en la factura");
        // Línea 2 (sin artículo ni peso): rappel −3 y envase 10; los portes por kilo no se ponen solos.
        f.Lineas[1].Conceptos.Select(c => (c.Codigo, c.Importe)).Should().Equal(("RAPPEL", -3m), ("ENVASE", 10m));
        f.Lineas[1].Base.Should().Be(107m);
        f.BaseImponible.Should().Be(331m);
        f.Total.Should().Be(400.51m);
        (await api.GetAsync(new Uri($"/facturas/{f.Id}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Con la lista de conceptos vacía no se pone ninguno; con valor, manda el de la línea.
        var sin = (await (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { ProductoId = naranja, Cantidad = 10m, Conceptos = new[] { new { ConceptoId = envase, Valor = (decimal?)5m } } } },
        })).Content.ReadFromJsonAsync<FacturaResp>())!;
        sin.Lineas[0].Conceptos.Should().ContainSingle().Which.Importe.Should().Be(5m);
        sin.BaseImponible.Should().Be(25m);

        var ambito = await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = naranja, Cantidad = 1m, Conceptos = new[] { new { ConceptoId = soloCompras } } } } });
        ambito.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ambito.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("concepto.ambito");

        var negativa = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Descripcion = "Muestra", Cantidad = 1m, PrecioUnitario = 1m, Conceptos = Array.Empty<object>() } },
            ConceptosDocumento = new[] { new { ConceptoId = (await api.GetFromJsonAsync<List<ConceptoCodigoResp>>("/conceptos-linea"))!.Single(c => c.Codigo == "RAPPEL").Id, Valor = (decimal?)100m } },
        });
        negativa.StatusCode.Should().Be(HttpStatusCode.Created, "un −100 % deja la línea a cero, no en negativo");

        // Un albarán entregado y sin facturar también cuenta en el informe.
        var albaran = await api.PostAsJsonAsync("/albaranes-venta", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { ProductoId = naranja, Cantidad = 10m, PrecioUnitario = 2m, Conceptos = new[] { new { ConceptoId = envase, Valor = (decimal?)5m } } } },
        });
        albaran.StatusCode.Should().Be(HttpStatusCode.Created, await albaran.Content.ReadAsStringAsync());

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var informe = (await api.GetFromJsonAsync<InformeResp>($"/conceptos-linea/informe?desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}"))!;
        informe.PrecioSinFacturar.Should().Be(5m);
        informe.Conceptos.Single(c => c.Codigo == "ENVASE").Ventas.Should().Be(40m);
        informe.Conceptos.Single(c => c.Codigo == "RAPPEL").Ventas.Should().Be(-10m);
        informe.Conceptos.Single(c => c.Codigo == "COMIS").Ventas.Should().Be(10m, "en la segunda factura se dieron los conceptos a mano, sin la comisión");
        informe.PrecioVentas.Should().Be(10m + 40m - 10m);
        informe.CosteVentas.Should().Be(10m);
    }

    private sealed record ConceptoCodigoResp(Guid Id, string Codigo);

    [Fact]
    public async Task El_pedido_de_venta_pasa_sus_conceptos_a_la_factura()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Distribuciones Sur SL", NifFiscal = "B12345678" });
        await IdAsync(api, "/conceptos-linea", Concepto("RECARGO", "Ventas", "Precio", "Suma", "PorUnidad", 0.5m, [new { TerceroId = cliente }]));

        var r = await api.PostAsJsonAsync("/pedidos-venta", new { ClienteId = cliente, Lineas = new[] { new { Descripcion = "Caja", Cantidad = 10m, PrecioUnitario = 4m } } });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        var pedido = (await r.Content.ReadFromJsonAsync<PedidoVentaResp>())!;
        pedido.Total.Should().Be(45m);
        pedido.Lineas[0].Conceptos.Should().ContainSingle(c => c.Codigo == "RECARGO" && c.Importe == 5m);

        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var fr = await api.PostAsJsonAsync($"/pedidos-venta/{pedido.Id}/facturar", new { });
        fr.IsSuccessStatusCode.Should().BeTrue(await fr.Content.ReadAsStringAsync());
        var factura = (await fr.Content.ReadFromJsonAsync<FacturaResp>())!;
        factura.BaseImponible.Should().Be(45m);
        factura.Lineas[0].Conceptos.Should().ContainSingle(c => c.Codigo == "RECARGO" && c.Importe == 5m);

        // En Facturae el concepto que suma va como cargo de la línea: GrossAmount = TotalCost + cargos.
        var xml = await api.GetStringAsync(new Uri($"/facturas/{factura.Id}/facturae.xml", UriKind.Relative));
        xml.Should().Contain("<TotalCost>40.00</TotalCost>").And.Contain("<ChargeReason>recargo</ChargeReason>").And.Contain("<ChargeAmount>5.00</ChargeAmount>")
            .And.Contain("<GrossAmount>45.00</GrossAmount>");
    }

    [Fact]
    public async Task Los_conceptos_de_coste_de_la_compra_suben_el_coste_de_entrada_en_almacen()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var envase = await IdAsync(api, "/productos", new { Nombre = "Caja de cartón", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Cartonajes Levante SL" });
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "CEN", Nombre = "Central" });
        await IdAsync(api, "/conceptos-linea", Concepto("PRONTOPAGO", "Compras", "Precio", "Resta", "Porcentaje", 2m, [new { TerceroId = proveedor }]));
        var portes = await IdAsync(api, "/conceptos-linea", Concepto("PORTESC", "Compras", "Coste", "Suma", "Importe", 50m, reparto: "PorCantidad"));

        var r = await api.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor,
            Lineas = new[] { new { ProductoId = envase, Descripcion = "Caja", Cantidad = 100m, PrecioUnitario = 10m } },
            ConceptosDocumento = new[] { new { ConceptoId = portes } },
        });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        var pedido = (await r.Content.ReadFromJsonAsync<PedidoResp>())!;
        pedido.Total.Should().Be(980m, "el pronto pago lo descuenta el proveedor; los portes los cobra otro");
        pedido.Lineas[0].CosteConceptos.Should().Be(50m);
        pedido.Lineas[0].CosteUnitarioEntrada.Should().Be(10.30m);

        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new { AlmacenId = almacen, Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 100m } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await api.PutAsJsonAsync("/empresas/actual/metodo-valoracion", new { MetodoValoracion = "Pmp" })).EnsureSuccessStatusCode();
        var valoracion = (await api.GetFromJsonAsync<ValoracionResp>("/inventario/valoracion"))!;
        valoracion.Lineas.Single(l => l.ProductoId == envase).CosteUnitario.Should().Be(10.30m);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var informe = (await api.GetFromJsonAsync<InformeResp>($"/conceptos-linea/informe?desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}"))!;
        informe.PrecioCompras.Should().Be(-20m);
        informe.CosteCompras.Should().Be(50m);
    }

    [Fact]
    public async Task El_maestro_valida_y_un_concepto_usado_se_da_de_baja()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var id = await IdAsync(api, "/conceptos-linea", Concepto("PALE", "Ventas", "Precio", "Suma", "Importe", 12m));
        (await api.PostAsJsonAsync("/conceptos-linea", Concepto("PALE", "Ventas", "Precio", "Suma", "Importe", 1m))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        var malo = await api.PostAsJsonAsync("/conceptos-linea", Concepto("PCT", "Ventas", "Precio", "Suma", "Porcentaje", 120m));
        (await malo.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("concepto.valor");
        var libre = await IdAsync(api, "/conceptos-linea", Concepto("LIBRE", "Compras", "Coste", "Suma", "PorUnidad", 1m));
        (await (await api.DeleteAsync(new Uri($"/conceptos-linea/{libre}", UriKind.Relative))).Content.ReadFromJsonAsync<BajaResp>())!.Eliminado.Should().BeTrue();

        (await api.PutAsJsonAsync($"/conceptos-linea/{id}", new { Datos = new { Nombre = "Palé europeo", Ambito = "Ventas", Efecto = "Precio", Sentido = "Suma", Calculo = "Importe", Valor = 15m } }))
            .EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas Bravo" });
        (await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Descripcion = "Pedido", Cantidad = 1m, PrecioUnitario = 50m, Conceptos = new[] { new { ConceptoId = id } } } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var baja = (await (await api.DeleteAsync(new Uri($"/conceptos-linea/{id}", UriKind.Relative))).Content.ReadFromJsonAsync<BajaResp>())!;
        baja.Should().Be(new BajaResp(false, false), "ya está en una factura: se da de baja y la factura conserva su copia");
        (await api.GetFromJsonAsync<ConceptoMaestroResp>($"/conceptos-linea/{id}"))!.Activo.Should().BeFalse();
    }

    private sealed record LineaGastoResp(decimal Base, string? CuentaGasto);
    private sealed record GastoConLineasResp(Guid Id, decimal BaseImponible, List<LineaGastoResp> Lineas);

    [Fact]
    public async Task Un_concepto_de_compra_con_cuenta_propia_va_a_su_linea_del_gasto()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Suministros Norte SL" });
        await IdAsync(api, "/conceptos-linea", new
        {
            Codigo = "PORTESP", Datos = new { Nombre = "Portes pagados", Ambito = "Compras", Efecto = "Precio", Sentido = "Suma", Calculo = "Importe", Valor = 30m,
                CuentaContable = "624", Asignaciones = new[] { new { TerceroId = proveedor } } },
        });
        var r = await api.PostAsJsonAsync("/compras/pedidos", new { ProveedorId = proveedor, Lineas = new[] { new { Descripcion = "Material", Cantidad = 10m, PrecioUnitario = 10m } } });
        var pedido = (await r.Content.ReadFromJsonAsync<PedidoResp>())!;
        pedido.Total.Should().Be(130m);
        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var f = await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/facturar", new { CodigoIva = "IVA21" });
        f.IsSuccessStatusCode.Should().BeTrue(await f.Content.ReadAsStringAsync());

        var gasto = (await api.GetFromJsonAsync<List<GastoConLineasResp>>("/gastos"))!.Single(g => g.BaseImponible == 130m);
        gasto.Lineas.Should().ContainSingle(l => l.CuentaGasto == "624" && l.Base == 30m);
        gasto.Lineas.Should().ContainSingle(l => l.CuentaGasto == null && l.Base == 100m);
    }
}
