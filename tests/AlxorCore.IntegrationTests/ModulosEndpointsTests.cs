using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlxorCore.Api.Comun;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de las ediciones y módulos contratables: qué puede usar cada plan.</summary>
public sealed class ModulosEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ModulosEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record PlanResp(string Edicion, List<string> ModulosAdicionales, List<string> ModulosActivos);
    private sealed record SeleccionPlanResp(string Token, Guid EmpresaId, string Edicion, List<string> Modulos);
    private sealed record ProblemaResp(string Title, string Codigo);

    /// <summary>Cambia el plan y vuelve a seleccionar la empresa para que el token lo refleje.</summary>
    private static async Task<SeleccionPlanResp> ContratarAsync(HttpClient c, Guid empresaId, string edicion, params string[] adicionales)
    {
        var cambio = await c.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = edicion, ModulosAdicionales = adicionales });
        cambio.StatusCode.Should().Be(HttpStatusCode.OK, await cambio.Content.ReadAsStringAsync());
        var seleccion = await (await c.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null))
            .Content.ReadFromJsonAsync<SeleccionPlanResp>();
        c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seleccion!.Token);
        return seleccion;
    }

    [Fact]
    public async Task Una_empresa_nueva_tiene_la_edicion_completa_y_todo_disponible()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var plan = await c.GetFromJsonAsync<PlanResp>("/empresas/actual/plan");
        plan!.Edicion.Should().Be("completa");
        (await c.GetAsync("/contabilidad/cuentas")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Con_la_edicion_gestion_se_usa_el_ciclo_comercial_pero_no_la_contabilidad()
    {
        var (c, empresaId) = await Ayudas.ConEmpresaAsync(_fabrica);
        var seleccion = await ContratarAsync(c, empresaId, "gestion");
        seleccion.Edicion.Should().Be("gestion");
        seleccion.Modulos.Should().Contain(["ventas", "compras", "inventario"]).And.NotContain("contabilidad");

        (await c.GetAsync("/inventario/almacenes")).StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        (await c.GetAsync("/facturas")).StatusCode.Should().Be(HttpStatusCode.OK);          // la base siempre

        var resp = await c.GetAsync("/contabilidad/cuentas");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var problema = await resp.Content.ReadFromJsonAsync<ProblemaResp>();
        problema!.Codigo.Should().Be("modulo.no_contratado");
        problema.Title.Should().Be("Tu plan (Gestión) no incluye el módulo Contabilidad. Puedes contratarlo en Ajustes → Plan.");
    }

    [Fact]
    public async Task Start_solo_permite_la_base_y_un_modulo_suelto_lo_amplia()
    {
        var (c, empresaId) = await Ayudas.ConEmpresaAsync(_fabrica);
        await ContratarAsync(c, empresaId, "start");
        (await c.GetAsync("/facturas")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.PostAsJsonAsync("/tesoreria/previsiones", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await c.GetAsync("/contabilidad/cuentas")).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await ContratarAsync(c, empresaId, "start", "contabilidad");
        (await c.GetAsync("/contabilidad/cuentas")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.GetAsync("/contabilidad/inmovilizado")).StatusCode.Should().Be(HttpStatusCode.Forbidden);   // otro módulo
    }

    [Fact]
    public async Task Un_plan_incoherente_se_rechaza()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var resp = await c.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "start", ModulosAdicionales = new[] { "produccion" } });
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await resp.Content.ReadFromJsonAsync<ProblemaResp>())!.Title.Should().Be("Producción necesita Inventario.");
    }

    [Fact]
    public async Task El_catalogo_de_planes_es_publico()
    {
        var c = _fabrica.CreateClient();
        var resp = await c.GetAsync("/planes");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        (await resp.Content.ReadAsStringAsync()).Should().Contain("\"gestion_finanzas\"");
    }

    /// <summary>
    /// Guardián: toda ruta de la API tiene que pertenecer a la base o a un módulo contratable.
    /// Si alguien añade un endpoint nuevo sin clasificarlo, este test lo señala.
    /// </summary>
    [Fact]
    public void Toda_ruta_esta_clasificada()
    {
        var rutas = _fabrica.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Select(e => "/" + (e.RoutePattern.RawText ?? string.Empty).TrimStart('/'))
            .Where(r => !r.Contains("{*", StringComparison.Ordinal))           // fallbacks de la interfaz web
            .Distinct()
            .ToList();

        var sinClasificar = rutas
            .Where(r => RutasModulos.ModuloDe(r) is null
                        && !RutasModulos.Base.Any(b => r.Equals(b, StringComparison.OrdinalIgnoreCase)
                                                       || r.StartsWith(b + "/", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(r => r, StringComparer.Ordinal)
            .ToList();

        string.Join(", ", sinClasificar).Should().BeEmpty("cada ruta debe estar en RutasModulos.Mapa (módulo) o en RutasModulos.Base");
    }
}
