using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Centros dentro de la empresa: series por centro y por caja, el usuario limitado a sus centros (ve, abre y crea solo
/// en ellos), el cierre de una caja y la venta que sale del almacén habitual del centro.
/// </summary>
public sealed class CentrosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CentrosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record PerfilResp(Guid Id);
    private sealed record CajaResp(Guid Id, string Codigo, string Nombre, bool Activa);
    private sealed record CentroResp(Guid Id, string Codigo, string Nombre, Guid? AlmacenId, bool Activo, List<CajaResp> Cajas, int Usuarios);
    private sealed record FacturaResp(Guid Id, string NumeroCompleto, Guid? CentroId, Guid? CajaId, decimal Total);
    private sealed record ResumenResp(Guid Id, string NumeroCompleto, Guid? CentroId);
    private sealed record PaginaResp(List<ResumenResp> Elementos, int Total);
    private sealed record MisCentrosResp(bool Limitado, List<CentroResp> Centros);
    private sealed record CierreResp(int Tickets, decimal Total, decimal Cobrado, decimal Pendiente, string? PrimerTicket, string? UltimoTicket);
    private sealed record StockResp(Guid AlmacenId, decimal Cantidad);
    private sealed record AlbaranResp(Guid Id, Guid? CentroId);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
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

    private static object Factura(Guid cliente, Guid? centro, Guid? producto = null) => new
    {
        ClienteId = cliente, CentroId = centro,
        Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21", ProductoId = producto } },
    };

    [Fact]
    public async Task Series_por_centro_y_caja_y_usuario_limitado_a_su_centro()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var yo = (await api.GetFromJsonAsync<PerfilResp>("/auth/perfil"))!.Id;
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Cliente Centros SL", NifFiscal = Ayudas.GenerarNif() });

        var t1 = await OkAsync<CentroResp>(api.PostAsJsonAsync("/centros", new { Codigo = "t1", Nombre = "Tienda Centro" }));
        t1.Codigo.Should().Be("T1");
        var t2 = await IdAsync(api, "/centros", new { Codigo = "T2", Nombre = "Tienda Puerto" });
        (await FalloAsync(api.PostAsJsonAsync("/centros", new { Codigo = "T1" }), HttpStatusCode.Conflict)).Should().Be("centro.duplicado");
        var conCaja = await OkAsync<CentroResp>(api.PostAsJsonAsync($"/centros/{t1.Id}/cajas", new { Codigo = "K1", Nombre = "Caja mostrador" }));
        var caja = conCaja.Cajas.Single().Id;
        (await FalloAsync(api.PostAsJsonAsync($"/centros/{t1.Id}/cajas", new { Codigo = "K1" }), HttpStatusCode.Conflict)).Should().Be("centro.caja_duplicada");

        (await api.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Factura", Ambito = "Centro", TerceroId = t1.Id, Prefijo = "T1F" })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/series/asignaciones", new { TipoDocumento = "Ticket", Ambito = "Caja", TerceroId = caja, Prefijo = "K1T" })).EnsureSuccessStatusCode();

        var deT1 = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", Factura(cliente, t1.Id)));
        deT1.NumeroCompleto.Should().StartWith("T1F");
        deT1.CentroId.Should().Be(t1.Id);
        var sinCentro = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", Factura(cliente, null)));
        sinCentro.NumeroCompleto.Should().StartWith("FA");
        sinCentro.CentroId.Should().BeNull();
        var deT2 = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", Factura(cliente, t2)));
        deT2.NumeroCompleto.Should().StartWith("FA", "T2 no tiene serie propia: la de la empresa");

        var ticket = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/tickets", new
        {
            CentroId = t1.Id, CajaId = caja, Lineas = new[] { new { Cantidad = 2m, Descripcion = "Café", PrecioUnitario = 1.5m, CodigoIva = "IVA21" } },
        }));
        ticket.NumeroCompleto.Should().StartWith("K1T");
        ticket.CajaId.Should().Be(caja);
        (await FalloAsync(api.PostAsJsonAsync("/tickets", new { CajaId = caja, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 1m, CodigoIva = "IVA21" } } }),
            HttpStatusCode.BadRequest)).Should().Be("centro.caja_sin_centro");
        (await FalloAsync(api.PostAsJsonAsync("/tickets", new { CentroId = t2, CajaId = caja, Lineas = new[] { new { Cantidad = 1m, Descripcion = "X", PrecioUnitario = 1m, CodigoIva = "IVA21" } } }),
            HttpStatusCode.BadRequest)).Should().Be("centro.caja_no_encontrada");

        // Cierre de la caja: el ticket de hoy.
        var cierre = await OkAsync<CierreResp>(api.GetAsync($"/centros/{t1.Id}/cajas/{caja}/cierre"));
        cierre.Tickets.Should().Be(1);
        cierre.Total.Should().Be(ticket.Total);
        cierre.PrimerTicket.Should().Be(ticket.NumeroCompleto);

        // Un centro dado de baja no admite documentos nuevos.
        (await api.PutAsJsonAsync($"/centros/{t2}", new { Nombre = "Tienda Puerto", Activo = false })).EnsureSuccessStatusCode();
        (await FalloAsync(api.PostAsJsonAsync("/facturas", Factura(cliente, t2)), HttpStatusCode.BadRequest)).Should().Be("centro.inactivo");
        (await api.PutAsJsonAsync($"/centros/{t2}", new { Nombre = "Tienda Puerto", Activo = true })).EnsureSuccessStatusCode();

        // El usuario pasa a trabajar solo con T1.
        (await api.PutAsJsonAsync($"/centros/accesos/{yo}", new { Centros = new[] { t1.Id } })).EnsureSuccessStatusCode();
        var mios = await OkAsync<MisCentrosResp>(api.GetAsync("/centros/mios"));
        mios.Limitado.Should().BeTrue();
        mios.Centros.Should().ContainSingle(c => c.Id == t1.Id);

        (await OkAsync<List<ResumenResp>>(api.GetAsync("/facturas"))).Select(f => f.Id).Should().BeEquivalentTo(new[] { deT1.Id, ticket.Id });
        (await OkAsync<PaginaResp>(api.GetAsync("/facturas/buscar"))).Elementos.Select(f => f.Id).Should().BeEquivalentTo(new[] { deT1.Id, ticket.Id });
        (await FalloAsync(api.GetAsync($"/facturas/{sinCentro.Id}"), HttpStatusCode.NotFound)).Should().Be("centro.documento_ajeno");
        (await FalloAsync(api.GetAsync($"/facturas/{deT2.Id}"), HttpStatusCode.NotFound)).Should().Be("centro.documento_ajeno");
        (await FalloAsync(api.PostAsJsonAsync($"/facturas/{deT2.Id}/anular", new { Motivo = "x" }), HttpStatusCode.NotFound)).Should().Be("centro.documento_ajeno");
        (await api.GetAsync($"/facturas/{deT1.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Sin indicar centro, el suyo; en otro, no.
        var auto = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/facturas", Factura(cliente, null)));
        auto.CentroId.Should().Be(t1.Id);
        auto.NumeroCompleto.Should().StartWith("T1F");
        (await FalloAsync(api.PostAsJsonAsync("/facturas", Factura(cliente, t2)), HttpStatusCode.Forbidden)).Should().Be("centro.sin_acceso");
        var gasto = await OkAsync<IdResp>(api.PostAsJsonAsync("/gastos", new { Concepto = "Alquiler tienda", BaseImponible = 500m, CodigoIva = "IVA21" }));
        (await OkAsync<List<AlbaranResp>>(api.GetAsync("/gastos"))).Should().ContainSingle(g => g.Id == gasto.Id && g.CentroId == t1.Id);

        // Vuelve a trabajar con todos.
        (await api.PutAsJsonAsync($"/centros/accesos/{yo}", new { Centros = Array.Empty<Guid>() })).EnsureSuccessStatusCode();
        (await OkAsync<List<ResumenResp>>(api.GetAsync("/facturas"))).Should().HaveCount(5);
        (await OkAsync<List<ResumenResp>>(api.GetAsync($"/facturas?centroId={t1.Id}"))).Should().HaveCount(3);
        (await OkAsync<List<CentroResp>>(api.GetAsync("/centros"))).Should().HaveCount(2);

        // Un centro con documentos no se borra (se da de baja); uno sin ellos, sí.
        (await FalloAsync(api.DeleteAsync($"/centros/{t1.Id}"), HttpStatusCode.Conflict)).Should().Be("centro.en_uso");
        var vacio = await IdAsync(api, "/centros", new { Codigo = "VAC" });
        (await api.DeleteAsync($"/centros/{vacio}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task La_venta_de_un_centro_sale_de_su_almacen_y_sus_documentos_heredan_el_centro()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Cliente Almacén SL", NifFiscal = Ayudas.GenerarNif() });
        var central = await IdAsync(api, "/inventario/almacenes", new { Codigo = "A1", Nombre = "Central" });
        var tienda = await IdAsync(api, "/inventario/almacenes", new { Codigo = "A2", Nombre = "Tienda" });
        var producto = await IdAsync(api, "/productos", new { Nombre = "Saco de abono", PrecioUnitario = 10m, Tipo = "Bien", CodigoIva = "IVA21", ControlarStock = true });
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = central, Cantidad = 10m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = tienda, Cantidad = 10m })).EnsureSuccessStatusCode();
        var centro = await IdAsync(api, "/centros", new { Codigo = "TDA", Nombre = "Tienda", AlmacenId = tienda });

        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, CentroId = centro, Lineas = new[] { new { Cantidad = 3m, Descripcion = "Saco", PrecioUnitario = 10m, CodigoIva = "IVA21", ProductoId = producto } },
        })).EnsureSuccessStatusCode();
        var stock = await OkAsync<List<StockResp>>(api.GetAsync($"/inventario/stock/producto/{producto}"));
        stock.Single(s => s.AlmacenId == tienda).Cantidad.Should().Be(7m);
        stock.Single(s => s.AlmacenId == central).Cantidad.Should().Be(10m);

        // Pedido del centro → albarán del pedido (hereda el centro y saca de su almacén).
        var pedido = await IdAsync(api, "/pedidos-venta", new
        {
            ClienteId = cliente, CentroId = centro, Lineas = new[] { new { Descripcion = "Saco", Cantidad = 2m, PrecioUnitario = 10m, CodigoIva = "IVA21", ProductoId = producto } },
        });
        (await OkAsync<AlbaranResp>(api.GetAsync($"/pedidos-venta/{pedido}"))).CentroId.Should().Be(centro);
        var albaran = await IdAsync(api, "/albaranes-venta", new
        {
            ClienteId = cliente, CentroId = centro, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Saco", PrecioUnitario = 10m, CodigoIva = "IVA21", ProductoId = producto } },
        });
        (await OkAsync<List<AlbaranResp>>(api.GetAsync($"/albaranes-venta?centroId={centro}"))).Should().ContainSingle(a => a.Id == albaran);
        (await OkAsync<List<StockResp>>(api.GetAsync($"/inventario/stock/producto/{producto}"))).Single(s => s.AlmacenId == tienda).Cantidad.Should().Be(6m);

        // La factura de ese albarán es del centro; mezclar albaranes de dos centros, no.
        var otro = await IdAsync(api, "/albaranes-venta", new
        {
            ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Saco", PrecioUnitario = 10m, CodigoIva = "IVA21", ProductoId = producto } },
        });
        (await FalloAsync(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaran, otro } }), HttpStatusCode.Conflict))
            .Should().Be("albaranventa.centros_distintos");
        var factura = await OkAsync<FacturaResp>(api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaran } }));
        factura.CentroId.Should().Be(centro);
    }
}
