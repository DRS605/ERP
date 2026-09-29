using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>GlobalG.A.P.: listas de puntos de control, autoevaluaciones y auditorías internas.</summary>
public sealed class AutoevaluacionesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AutoevaluacionesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record RespuestaResp(string Codigo, string Nivel, string? Resultado);
    private sealed record EvaluacionResp(Guid Id, string Lista, string? Agricultor, bool Cerrada, int Puntos, int Respondidos, int NoCumple, decimal CumplimientoMayores,
        decimal CumplimientoMenores, bool Supera, List<RespuestaResp> Respuestas);
    private sealed record EliminadoResp(bool Eliminado);
    private sealed record ListaResp(Guid Id, bool Activa);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    [Fact]
    public async Task La_autoevaluacion_se_responde_se_cierra_y_da_el_resultado()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });

        var lista = await IdAsync(api, "/agro/listas-control", new
        {
            Codigo = "ifa", Nombre = "Lista de control IFA", Version = "v6",
            Puntos = new object[]
            {
                new { Codigo = "AF 1.1", Texto = "Registro de las parcelas", Nivel = "Mayor" },
                new { Codigo = "AF 2.1", Texto = "Análisis de riesgos del terreno", Nivel = "Menor" },
                new { Codigo = "AF 3.1", Texto = "Plan de higiene", Nivel = "Menor" },
                new { Codigo = "AF 9.9", Texto = "Biodiversidad", Nivel = "Recomendacion" },
            },
        });
        var id = await IdAsync(api, "/agro/autoevaluaciones", new { ListaControlId = lista, AgricultorId = agricultor, Tipo = "AuditoriaInterna", Fecha = new DateOnly(2026, 4, 1), Auditor = "Ana Técnica" });

        // Sin responder todo no se cierra.
        var cerrar = await api.PostAsync(new Uri($"/agro/autoevaluaciones/{id}/cerrar", UriKind.Relative), null);
        (await cerrar.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("autoevaluacion.sin_responder");

        // Un «no cumple» sin acción correctiva, ni un «no aplica» sin justificación, tampoco.
        (await api.PutAsJsonAsync($"/agro/autoevaluaciones/{id}", new
        {
            Respuestas = new object[]
            {
                new { Codigo = "AF 1.1", Resultado = "Cumple" },
                new { Codigo = "AF 2.1", Resultado = "NoCumple" },
                new { Codigo = "AF 3.1", Resultado = "NoAplica" },
                new { Codigo = "AF 9.9", Resultado = "NoCumple" },
            },
        })).EnsureSuccessStatusCode();
        cerrar = await api.PostAsync(new Uri($"/agro/autoevaluaciones/{id}/cerrar", UriKind.Relative), null);
        (await cerrar.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("autoevaluacion.justificacion");
        (await api.PutAsJsonAsync($"/agro/autoevaluaciones/{id}", new
        {
            Respuestas = new object[] { new { Codigo = "AF 3.1", Resultado = "NoAplica", Comentario = "No hay manipulación en campo" } },
        })).EnsureSuccessStatusCode();
        cerrar = await api.PostAsync(new Uri($"/agro/autoevaluaciones/{id}/cerrar", UriKind.Relative), null);
        (await cerrar.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("autoevaluacion.accion_correctiva");
        var r = await api.PutAsJsonAsync($"/agro/autoevaluaciones/{id}", new
        {
            Respuestas = new object[] { new { Codigo = "AF 2.1", Resultado = "NoCumple", AccionCorrectiva = "Hacer el análisis de riesgos", FechaLimite = new DateOnly(2026, 5, 1) } },
        });
        var abierta = (await r.Content.ReadFromJsonAsync<EvaluacionResp>())!;
        abierta.Respondidos.Should().Be(4);
        abierta.CumplimientoMayores.Should().Be(100m);
        abierta.CumplimientoMenores.Should().Be(0m, "la única menor que aplica no se cumple");
        abierta.Supera.Should().BeFalse();

        cerrar = await api.PostAsync(new Uri($"/agro/autoevaluaciones/{id}/cerrar", UriKind.Relative), null);
        cerrar.IsSuccessStatusCode.Should().BeTrue(await cerrar.Content.ReadAsStringAsync());
        var cerrada = (await api.GetFromJsonAsync<EvaluacionResp>($"/agro/autoevaluaciones/{id}"))!;
        cerrada.Should().Match<EvaluacionResp>(e => e.Cerrada && e.Agricultor == "Juan Labrador" && e.Lista == "IFA" && e.NoCumple == 2 && e.Puntos == 4);

        // Cerrada: no cambia ni se borra.
        (await (await api.PutAsJsonAsync($"/agro/autoevaluaciones/{id}", new { Respuestas = new object[] { new { Codigo = "AF 2.1", Resultado = "Cumple" } } }))
            .Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("autoevaluacion.cerrada");
        (await api.DeleteAsync(new Uri($"/agro/autoevaluaciones/{id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await api.GetFromJsonAsync<List<EvaluacionResp>>($"/agro/autoevaluaciones?agricultorId={agricultor}&anio=2026"))!.Should().ContainSingle(e => e.Id == id);

        // La lista con evaluaciones se da de baja (no se borra) y ya no abre otras.
        var baja = (await (await api.DeleteAsync(new Uri($"/agro/listas-control/{lista}", UriKind.Relative))).Content.ReadFromJsonAsync<EliminadoResp>())!;
        baja.Eliminado.Should().BeFalse();
        (await api.GetFromJsonAsync<List<ListaResp>>("/agro/listas-control"))!.Single(l => l.Id == lista).Activa.Should().BeFalse();
        var otra = await api.PostAsJsonAsync("/agro/autoevaluaciones", new { ListaControlId = lista, Tipo = "Autoevaluacion", Fecha = new DateOnly(2026, 4, 2), Auditor = "Juan" });
        (await otra.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("autoevaluacion.lista_de_baja");

        // El agricultor con evaluaciones no se borra.
        var ag = await api.DeleteAsync(new Uri($"/agro/agricultores/{agricultor}", UriKind.Relative));
        (await ag.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("agricultor.en_uso");
    }
}
