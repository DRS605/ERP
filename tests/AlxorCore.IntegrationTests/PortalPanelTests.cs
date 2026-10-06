using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Portal del agricultor y del cliente (enlace con clave → sesión que solo ve lo suyo y solo vale para /portal) y panel
/// de campaña en vivo con sus avisos de anomalías.
/// </summary>
public sealed class PortalPanelTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PortalPanelTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid Id);
    private sealed record RecepcionResp(Guid Id, string Estado, decimal NetoKg, List<LineaResp> Lineas);
    private sealed record AccesoResp(Guid Id, string Tipo, Guid TerceroId, string Nombre, bool Vigente);
    private sealed record ClaveResp(AccesoResp Acceso, string Clave);
    private sealed record SesionResp(string Token, string Tipo, string Nombre, string Empresa);
    private sealed record ResumenAgricultorResp(decimal KilosEntregados, int Entregas, decimal KilosPendientesLiquidar);
    private sealed record LineaEntregaResp(string Producto, decimal NetoKg);
    private sealed record EntregaResp(Guid Id, decimal NetoKg, List<LineaEntregaResp> Lineas);
    private sealed record FacturaPortalResp(Guid Id, decimal Total, decimal Pendiente, bool Vencida);
    private sealed record ResumenClienteResp(decimal Pendiente, decimal Vencido, int FacturasPendientes);
    private sealed record KilosResp(string Producto, decimal Kilos);
    private sealed record AnomaliaResp(string Codigo, string Gravedad, string Mensaje);
    private sealed record DiaResp(DateOnly Fecha, decimal KilosRecibidos);
    private sealed record PanelResp(string? Campana, decimal KilosHoy, int RecepcionesHoy, int RecepcionesBorrador, List<KilosResp> HoyPorProducto, decimal KilosEnAlmacen,
        List<KilosResp> AlmacenPorProducto, List<DiaResp> UltimosDias, decimal KilosCampana, List<AnomaliaResp> Anomalias);

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

    private async Task<HttpClient> SesionPortalAsync(string clave)
    {
        var portal = _fabrica.CreateClient();
        var sesion = await OkAsync<SesionResp>(portal.PostAsJsonAsync("/portal/entrar", new { Clave = clave }));
        portal.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);
        return portal;
    }

    private static async Task<RecepcionResp> RecibirAsync(HttpClient api, Guid agricultor, Guid producto, DateOnly fecha, decimal neto, bool confirmar = true)
    {
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = fecha });
        var r = await OkAsync<RecepcionResp>(api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = producto, FechaRecoleccion = fecha }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas.Single().Id}/pesadas", new { BrutoKg = neto + 500m, TaraKg = 500m }))
            .EnsureSuccessStatusCode();
        return confirmar ? await OkAsync<RecepcionResp>(api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null)) : r;
    }

    [Fact]
    public async Task Portal_del_agricultor_y_del_cliente_y_panel_de_campana()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await IdAsync(api, "/agro/campanas", new { Codigo = $"P{Hoy.Year}", Nombre = $"Campaña {Hoy.Year}", Desde = Hoy.AddDays(-200), Hasta = Hoy.AddDays(100) });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = await IdAsync(api, "/proveedores", new { Nombre = "Pepa Huertas", NifFiscal = Ayudas.GenerarNif() }), Regimen = "Reagp" });
        var otro = await IdAsync(api, "/agro/agricultores", new { ProveedorId = await IdAsync(api, "/proveedores", new { Nombre = "Otro Agricultor", NifFiscal = Ayudas.GenerarNif() }), Regimen = "Reagp" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Lane", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        await RecibirAsync(api, agricultor, naranja, Hoy, 4_000m);
        await RecibirAsync(api, agricultor, naranja, Hoy.AddDays(-10), 2_500m);
        await RecibirAsync(api, otro, naranja, Hoy, 1_000m);
        await RecibirAsync(api, otro, naranja, Hoy.AddDays(-3), 700m, confirmar: false);

        // Accesos: solo con permiso de gestión, para un agricultor o cliente que existe y uno vigente por tercero.
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SL", NifFiscal = Ayudas.GenerarNif() });
        (await FalloAsync(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "Cliente", TerceroId = Guid.NewGuid() }), HttpStatusCode.NotFound)).Should().Be("portal.tercero");
        var accesoAgricultor = await OkAsync<ClaveResp>(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "Agricultor", TerceroId = agricultor }));
        accesoAgricultor.Acceso.Nombre.Should().Be("Pepa Huertas");
        (await FalloAsync(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "Agricultor", TerceroId = agricultor }), HttpStatusCode.Conflict)).Should().Be("portal.duplicado");
        var accesoCliente = await OkAsync<ClaveResp>(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "Cliente", TerceroId = cliente }));
        (await OkAsync<List<AccesoResp>>(api.GetAsync("/accesos-portal"))).Should().HaveCount(2);

        // Claves malas: sin pistas de qué falla.
        var anonimo = _fabrica.CreateClient();
        (await FalloAsync(anonimo.PostAsJsonAsync("/portal/entrar", new { Clave = "basura" }), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
        (await FalloAsync(anonimo.PostAsJsonAsync("/portal/entrar", new { Clave = accesoAgricultor.Clave[..^3] + "xyz" }), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
        (await anonimo.GetAsync("/portal/agricultor/resumen")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        // El agricultor ve solo sus entregas confirmadas.
        var portal = await SesionPortalAsync(accesoAgricultor.Clave);
        var resumen = await OkAsync<ResumenAgricultorResp>(portal.GetAsync("/portal/agricultor/resumen"));
        resumen.Should().Be(new ResumenAgricultorResp(6_500m, 2, 6_500m));
        var entregas = await OkAsync<List<EntregaResp>>(portal.GetAsync("/portal/agricultor/entregas"));
        entregas.Should().HaveCount(2);
        entregas[0].Lineas.Single().Should().Be(new LineaEntregaResp("Naranja Lane", 4_000m));
        (await OkAsync<List<object>>(portal.GetAsync("/portal/agricultor/liquidaciones"))).Should().BeEmpty();
        (await FalloAsync(portal.GetAsync($"/portal/agricultor/liquidaciones/{Guid.NewGuid()}/pdf"), HttpStatusCode.NotFound)).Should().Be("portal.no_encontrado");

        // Ni la parte del cliente ni el resto del ERP.
        (await FalloAsync(portal.GetAsync("/portal/cliente/facturas"), HttpStatusCode.Forbidden)).Should().Be("portal.tipo");
        (await FalloAsync(portal.GetAsync("/clientes"), HttpStatusCode.Forbidden)).Should().Be("portal.solo_portal");
        (await FalloAsync(portal.GetAsync("/agro/recepciones"), HttpStatusCode.Forbidden)).Should().Be("portal.solo_portal");
        (await FalloAsync(portal.GetAsync("/accesos-portal"), HttpStatusCode.Forbidden)).Should().Be("portal.solo_portal");

        // El cliente: sus facturas con lo pendiente y lo vencido.
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente, FechaEmision = Hoy.AddDays(-40), DiasVencimiento = 30,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Naranja", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        })).EnsureSuccessStatusCode();
        var otroCliente = await IdAsync(api, "/clientes", new { Nombre = "Ajeno SA", NifFiscal = Ayudas.GenerarNif() });
        var ajena = await IdAsync(api, "/facturas", new
        {
            ClienteId = otroCliente, FechaEmision = Hoy, DiasVencimiento = 30,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Limón", PrecioUnitario = 50m, CodigoIva = "IVA21" } },
        });
        var portalCliente = await SesionPortalAsync(accesoCliente.Clave);
        var facturas = await OkAsync<List<FacturaPortalResp>>(portalCliente.GetAsync("/portal/cliente/facturas"));
        facturas.Should().ContainSingle().Which.Should().Match<FacturaPortalResp>(f => f.Total == 121m && f.Pendiente == 121m && f.Vencida);
        (await OkAsync<ResumenClienteResp>(portalCliente.GetAsync("/portal/cliente/resumen"))).Should().Be(new ResumenClienteResp(121m, 121m, 1));
        var pdf = await portalCliente.GetAsync($"/portal/cliente/facturas/{facturas[0].Id}/pdf");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK, await pdf.Content.ReadAsStringAsync());
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await FalloAsync(portalCliente.GetAsync($"/portal/cliente/facturas/{ajena}/pdf"), HttpStatusCode.NotFound)).Should().Be("portal.no_encontrado");
        (await FalloAsync(portalCliente.GetAsync("/portal/agricultor/resumen"), HttpStatusCode.Forbidden)).Should().Be("portal.tipo");
        (await OkAsync<List<object>>(portalCliente.GetAsync("/portal/cliente/albaranes"))).Should().BeEmpty();

        // Revocar corta el enlace y la sesión abierta; regenerar invalida la clave anterior.
        (await api.PostAsync(new Uri($"/accesos-portal/{accesoCliente.Acceso.Id}/revocar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await FalloAsync(portalCliente.GetAsync("/portal/cliente/resumen"), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
        (await FalloAsync(anonimo.PostAsJsonAsync("/portal/entrar", new { Clave = accesoCliente.Clave }), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
        var nueva = await OkAsync<ClaveResp>(api.PostAsync(new Uri($"/accesos-portal/{accesoAgricultor.Acceso.Id}/regenerar", UriKind.Relative), null));
        (await FalloAsync(anonimo.PostAsJsonAsync("/portal/entrar", new { Clave = accesoAgricultor.Clave }), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
        await SesionPortalAsync(nueva.Clave);

        // Panel de campaña.
        var panel = await OkAsync<PanelResp>(api.GetAsync("/agro/panel"));
        panel.Campana.Should().Be($"Campaña {Hoy.Year}");
        panel.KilosHoy.Should().Be(5_000m);
        panel.RecepcionesHoy.Should().Be(2);
        panel.RecepcionesBorrador.Should().Be(1);
        panel.HoyPorProducto.Should().ContainSingle().Which.Should().Be(new KilosResp("Naranja Lane", 5_000m));
        panel.KilosEnAlmacen.Should().Be(7_500m);
        panel.UltimosDias.Should().HaveCount(14);
        panel.UltimosDias[^1].Should().Be(new DiaResp(Hoy, 5_000m));
        panel.UltimosDias.Single(d => d.Fecha == Hoy.AddDays(-10)).KilosRecibidos.Should().Be(2_500m);
        panel.KilosCampana.Should().Be(7_500m);
        panel.Anomalias.Select(a => a.Codigo).Should().BeEquivalentTo(["recepcion.borrador", "partida.parada"]);
        panel.Anomalias.Single(a => a.Codigo == "partida.parada").Mensaje.Should().Contain("2.500").And.Contain("Naranja Lane");

        // El panel no es del portal.
        (await FalloAsync(portal.GetAsync("/agro/panel"), HttpStatusCode.Forbidden)).Should().Be("portal.solo_portal");
    }
}
