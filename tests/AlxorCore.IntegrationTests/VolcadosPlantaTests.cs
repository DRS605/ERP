using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// App de planta: terminal con acceso propio, datos para trabajar sin conexión, cola de volcados reenviable sin duplicar,
/// rechazo de lecturas erróneas y paso de los volcados a un parte de confección.
/// </summary>
public sealed class VolcadosPlantaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public VolcadosPlantaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaRecResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaRecResp> Lineas);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, decimal Kilos);
    private sealed record AccesoResp(Guid Id, string Tipo, string Nombre);
    private sealed record ClaveResp(AccesoResp Acceso, string Clave);
    private sealed record SesionResp(string Token, string Tipo);
    private sealed record LineaTerminalResp(Guid Id, string Codigo);
    private sealed record OrdenTerminalResp(Guid Id, Guid LineaId, string Producto);
    private sealed record PaleTerminalResp(string Sscc, string Producto, string? Agricultor, decimal Kilos);
    private sealed record DatosResp(string Terminal, List<LineaTerminalResp> Lineas, List<OrdenTerminalResp> Ordenes, List<PaleTerminalResp> Pales);
    private sealed record VolcadoResp(Guid Id, string Clave, string Sscc, decimal Kilos, string? Producto, string? Agricultor, string Terminal, string Estado, Guid? ParteConfeccionId,
        DateOnly Fecha);
    private sealed record ResultadoResp(string? Clave, string Estado, VolcadoResp? Volcado, string? Codigo);
    private sealed record ConsumoResp(Guid PartidaId, Guid? PaleId, decimal Kilos);
    private sealed record ParteResp(Guid Id, string Estado, decimal KilosConsumidos, List<ConsumoResp> Consumos);

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

    private static object Lectura(string clave, string sscc, Guid linea, DateTimeOffset cuando, Guid? orden = null) =>
        new { Clave = clave, Sscc = sscc, LineaId = linea, OrdenId = orden, VolcadoEn = cuando };

    [Fact]
    public async Task Terminal_sin_conexion_registra_volcados_sin_duplicar_y_genera_el_parte()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await IdAsync(api, "/agro/campanas", new { Codigo = $"V{Hoy.Year}", Nombre = "Campaña", Desde = Hoy.AddDays(-100), Hasta = Hoy.AddDays(100) });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = await IdAsync(api, "/proveedores", new { Nombre = "Toni Regadiu", NifFiscal = Ayudas.GenerarNif() }), Regimen = "Reagp" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = Hoy });
        var r = await OkAsync<RecepcionResp>(api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = naranja, FechaRecoleccion = Hoy }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 2_500m, TaraKg = 500m })).EnsureSuccessStatusCode();
        r = await OkAsync<RecepcionResp>(api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null));
        var partida = r.Lineas[0].PartidaId!.Value;
        var pales = new List<PaleResp>();
        foreach (var kilos in new[] { 600m, 500m, 400m })
        {
            var p = await OkAsync<PaleResp>(api.PostAsJsonAsync("/agro/pales", new { }));
            (await api.PostAsJsonAsync($"/agro/pales/{p.Id}/paletizar", new { PartidaId = partida, Kilos = kilos, Fecha = Hoy })).EnsureSuccessStatusCode();
            pales.Add(await OkAsync<PaleResp>(api.PostAsync(new Uri($"/agro/pales/{p.Id}/cerrar", UriKind.Relative), null)));
        }

        var linea = await IdAsync(api, "/agro/planta/lineas", new { Codigo = "L1", Nombre = "Línea de confección 1", Tipo = "Confeccion", CapacidadKgHora = 5_000m });
        var orden = await IdAsync(api, "/agro/planta/ordenes", new { LineaId = linea, Fecha = Hoy, ProductoId = naranja, Kilos = 1_000m, Turno = 1 });

        // Terminal: acceso propio que solo vale para /portal.
        (await FalloAsync(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "TerminalPlanta" }), HttpStatusCode.NotFound)).Should().Be("portal.tercero");
        var acceso = await OkAsync<ClaveResp>(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "TerminalPlanta", Nombre = "Volcador L1" }));
        await OkAsync<ClaveResp>(api.PostAsJsonAsync("/accesos-portal", new { Tipo = "TerminalPlanta", Nombre = "Volcador L2" }));
        var terminal = _fabrica.CreateClient();
        var sesion = await OkAsync<SesionResp>(terminal.PostAsJsonAsync("/portal/entrar", new { Clave = acceso.Clave }));
        sesion.Tipo.Should().Be("TerminalPlanta");
        terminal.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);
        (await FalloAsync(terminal.GetAsync("/agro/planta/volcados"), HttpStatusCode.Forbidden)).Should().Be("portal.solo_portal");
        (await FalloAsync(terminal.GetAsync("/portal/agricultor/resumen"), HttpStatusCode.Forbidden)).Should().Be("portal.tipo");

        // Datos para trabajar sin conexión.
        var datos = await OkAsync<DatosResp>(terminal.GetAsync("/portal/planta/datos"));
        datos.Terminal.Should().Be("Volcador L1");
        datos.Lineas.Should().ContainSingle(l => l.Id == linea);
        datos.Ordenes.Should().ContainSingle(o => o.Id == orden && o.Producto == "Naranja Navel");
        datos.Pales.Should().HaveCount(3).And.Contain(new PaleTerminalResp(pales[0].Sscc, "Naranja Navel", "Toni Regadiu", 600m));

        // Cola acumulada sin conexión: dos buenas, una repetida, una ilegible, una de un palé que no existe.
        var antes = DateTimeOffset.UtcNow.AddHours(-2);
        var lote = await OkAsync<List<ResultadoResp>>(terminal.PostAsJsonAsync("/portal/planta/volcados", new
        {
            Volcados = new[]
            {
                Lectura("t1-0001", pales[0].Sscc, linea, antes, orden),
                Lectura("t1-0002", "(00)" + pales[1].Sscc, linea, antes.AddMinutes(5), orden),
                Lectura("t1-0003", pales[0].Sscc, linea, antes.AddMinutes(9)),
                Lectura("t1-0004", "hola", linea, antes.AddMinutes(10)),
                Lectura("t1-0005", "123456789012345675", linea, antes.AddMinutes(11)),
            },
        }));
        lote.Select(x => (x.Clave, x.Estado, x.Codigo)).Should().Equal(
            ("t1-0001", "registrado", null), ("t1-0002", "registrado", null), ("t1-0003", "error", "volcado.repetido"), ("t1-0004", "error", "volcado.lectura"),
            ("t1-0005", "error", "volcado.pale_desconocido"));
        lote[0].Volcado!.Should().Match<VolcadoResp>(v => v.Kilos == 600m && v.Producto == "Naranja Navel" && v.Agricultor == "Toni Regadiu" && v.Terminal == "Volcador L1");

        // Se cortó la red antes de recibir la respuesta: reenviar la cola no duplica nada.
        var reenvio = await OkAsync<List<ResultadoResp>>(terminal.PostAsJsonAsync("/portal/planta/volcados", new
        {
            Volcados = new[] { Lectura("t1-0001", pales[0].Sscc, linea, antes, orden), Lectura("t1-0002", pales[1].Sscc, linea, antes.AddMinutes(5), orden) },
        }));
        reenvio.Select(x => x.Estado).Should().Equal("ya_registrado", "ya_registrado");
        reenvio[0].Volcado!.Id.Should().Be(lote[0].Volcado!.Id);
        (await FalloAsync(terminal.PostAsJsonAsync("/portal/planta/volcados", new { Volcados = Array.Empty<object>() }), HttpStatusCode.BadRequest)).Should().Be("volcado.lote");
        var futuro = await OkAsync<List<ResultadoResp>>(terminal.PostAsJsonAsync("/portal/planta/volcados",
            new { Volcados = new[] { Lectura("t1-0006", pales[2].Sscc, linea, DateTimeOffset.UtcNow.AddHours(3)) } }));
        futuro[0].Codigo.Should().Be("volcado.futuro");

        // Desde el ERP: un volcado más, uno erróneo que se anula.
        var erp = await OkAsync<List<ResultadoResp>>(api.PostAsJsonAsync("/agro/planta/volcados",
            new { Volcados = new[] { Lectura("erp-1", pales[2].Sscc, linea, DateTimeOffset.UtcNow) } }));
        erp[0].Estado.Should().Be("registrado");
        (await api.PostAsJsonAsync($"/agro/planta/volcados/{erp[0].Volcado!.Id}/anular", new { Motivo = "Palé equivocado" })).EnsureSuccessStatusCode();
        (await FalloAsync(api.PostAsJsonAsync($"/agro/planta/volcados/{erp[0].Volcado!.Id}/anular", new { }), HttpStatusCode.Conflict)).Should().Be("volcado.no_registrado");
        var delDia = await OkAsync<List<VolcadoResp>>(terminal.GetAsync("/portal/planta/volcados"));
        delDia.Select(v => v.Estado).Should().BeEquivalentTo(["Registrado", "Registrado", "Anulado"]);

        // Parte de confección con el consumo de los palés volcados.
        var parte = await OkAsync<ParteResp>(api.PostAsJsonAsync("/agro/planta/volcados/parte", new { LineaId = linea, Fecha = delDia[0].Fecha }));
        parte.Estado.Should().Be("Borrador");
        parte.KilosConsumidos.Should().Be(1_100m);
        parte.Consumos.Should().BeEquivalentTo([new ConsumoResp(partida, pales[0].Id, 600m), new ConsumoResp(partida, pales[1].Id, 500m)]);
        (await FalloAsync(api.PostAsJsonAsync("/agro/planta/volcados/parte", new { LineaId = linea, Fecha = delDia[0].Fecha }), HttpStatusCode.BadRequest))
            .Should().Be("volcado.sin_pendientes");
        (await OkAsync<List<VolcadoResp>>(api.GetAsync($"/agro/planta/volcados?desde={delDia[0].Fecha:yyyy-MM-dd}")))
            .Where(v => v.Estado != "Anulado").Should().OnlyContain(v => v.Estado == "EnParte" && v.ParteConfeccionId == parte.Id);
        (await FalloAsync(api.PostAsJsonAsync($"/agro/planta/volcados/{lote[0].Volcado!.Id}/anular", new { }), HttpStatusCode.Conflict)).Should().Be("volcado.no_registrado");

        // Borrar el borrador devuelve los volcados a pendientes; un palé en un parte pendiente no se vuelve a volcar.
        (await OkAsync<List<ResultadoResp>>(terminal.PostAsJsonAsync("/portal/planta/volcados",
            new { Volcados = new[] { Lectura("t1-0007", pales[0].Sscc, linea, DateTimeOffset.UtcNow) } })))[0].Codigo.Should().Be("volcado.repetido");
        (await api.DeleteAsync($"/agro/partes/{parte.Id}")).EnsureSuccessStatusCode();
        (await OkAsync<List<VolcadoResp>>(api.GetAsync($"/agro/planta/volcados?desde={delDia[0].Fecha:yyyy-MM-dd}"))).Count(v => v.Estado == "Registrado").Should().Be(2);

        // Revocar el terminal corta su sesión.
        (await api.PostAsync(new Uri($"/accesos-portal/{acceso.Acceso.Id}/revocar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await FalloAsync(terminal.GetAsync("/portal/planta/datos"), HttpStatusCode.Unauthorized)).Should().Be("portal.clave");
    }
}
