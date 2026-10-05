using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Viveros: lote de planta por fases (solo avanza), bajas con motivo, encargos con reserva que no se dan a otros,
/// entrega con albarán y pasaporte fitosanitario, paso a existencias, anulaciones y el libro del lote cuadrado.
/// </summary>
public sealed class ViveroTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ViveroTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record MovimientoResp(Guid Id, string Tipo, int Plantas, string Fase, bool Anulado);
    private sealed record LoteResp(Guid Id, string Codigo, string Fase, string? Ubicacion, int PlantasIniciales, int PlantasVivas, int Reservadas, int Disponibles, int Bajas,
        decimal PorcentajeBajas, List<MovimientoResp> Movimientos);
    private sealed record EncargoResp(Guid Id, string Numero, string Estado, Guid? LoteId, Guid? AlbaranId, string? Albaran);
    private sealed record PasaporteResp(string Especie, string? Variedad, string CodigoRegistro, string CodigoTrazabilidad, string PaisOrigen, int? Plantas, string? Cliente,
        string? Albaran);
    private sealed record LibroResp(string Lote, string Tipo, int Plantas);
    private sealed record ProductoResp(decimal Stock);
    private sealed record LineaAlbaranResp(decimal Cantidad, decimal PrecioUnitario);
    private sealed record AlbaranResp(string? Referencia, List<LineaAlbaranResp> Lineas, bool Anulado);

    private Guid _empresa;

    private async Task<HttpClient> EmpresaAsync(params string[] modulos)
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = modulos })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _empresa = empresa;
        return api;
    }

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

    private static Task<HttpResponseMessage> Post(HttpClient api, string ruta, object? cuerpo = null) =>
        cuerpo is null ? api.PostAsync(new Uri(ruta, UriKind.Relative), null) : api.PostAsJsonAsync(ruta, cuerpo);

    [Fact]
    public async Task El_modulo_se_contrata_aparte()
    {
        var sin = await EmpresaAsync();
        (await FalloAsync(sin.GetAsync("/vivero/lotes"), HttpStatusCode.Forbidden)).Should().Be("modulo.no_contratado");
        var con = await EmpresaAsync("vivero");
        (await con.GetAsync("/vivero/lotes")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Lote_por_fases_encargos_con_reserva_entrega_con_pasaporte_y_paso_a_existencias()
    {
        var api = await EmpresaAsync("vivero");
        var planta = await IdAsync(api, "/productos", new { Nombre = "Planta de tomate injertada", PrecioUnitario = 0.45m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Invernaderos del Campo SL", NifFiscal = Ayudas.GenerarNif() });
        var otro = await IdAsync(api, "/clientes", new { Nombre = "Huerta Grande SAT", NifFiscal = Ayudas.GenerarNif() });

        // Lote sembrado: 10.000 plantas en semillero.
        var lote = await OkAsync<LoteResp>(api.PostAsJsonAsync("/vivero/lotes", new
        {
            Especie = "Solanum lycopersicum", Variedad = "Raf", Portainjerto = "Maxifort", ProductoId = planta, Plantas = 10_000, FechaSiembra = Hoy,
            Ubicacion = "Invernadero 1 · mesa 4", OrigenMaterial = "Semillas Sur, lote S-77",
        }));
        lote.Codigo.Should().Be($"LP{Hoy.Year}-0001");
        var ruta = $"/vivero/lotes/{lote.Id}";

        // Fases: avanza (se puede saltar una) y nunca vuelve atrás.
        await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/avanzar", new { Fase = "Injerto", Fecha = Hoy }));
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/avanzar", new { Fase = "Semillero" }), HttpStatusCode.BadRequest)).Should().Be("lote_planta.fase");
        await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/avanzar", new { Ubicacion = "Invernadero 2", Fecha = Hoy }));

        // Bajas con motivo.
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/bajas", new { Plantas = 300 }), HttpStatusCode.BadRequest)).Should().Be("lote_planta.motivo");
        lote = await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/bajas", new { Plantas = 300, Motivo = "Fallo del injerto", Fecha = Hoy }));
        (lote.PlantasVivas, lote.Bajas, lote.PorcentajeBajas).Should().Be((9_700, 300, 3m));

        // Encargos: el primero reserva 6.000; el segundo no cabe en lo que queda sin reservar.
        var e1 = await OkAsync<EncargoResp>(api.PostAsJsonAsync("/vivero/encargos",
            new { ClienteId = cliente, ProductoId = planta, Plantas = 6_000, FechaEntrega = Hoy.AddDays(30), PrecioPlanta = 0.42m, LoteId = lote.Id, Fecha = Hoy }));
        e1.Estado.Should().Be("Reservado");
        var e2 = await OkAsync<EncargoResp>(api.PostAsJsonAsync("/vivero/encargos", new { ClienteId = otro, ProductoId = planta, Plantas = 4_000, FechaEntrega = Hoy.AddDays(30), Fecha = Hoy }));
        (await FalloAsync(api.PostAsJsonAsync($"/vivero/encargos/{e2.Id}/reservar", new { LoteId = lote.Id }), HttpStatusCode.Conflict)).Should().Be("encargo.sin_plantas");
        lote = await OkAsync<LoteResp>(api.GetAsync(ruta));
        (lote.Reservadas, lote.Disponibles).Should().Be((6_000, 3_700));

        // Sin estar lista no se entrega ni pasa a existencias.
        (await FalloAsync(Post(api, $"/vivero/encargos/{e1.Id}/servir"), HttpStatusCode.Conflict)).Should().Be("lote_planta.no_lista");
        await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/avanzar", new { Fase = "Lista", Fecha = Hoy }));

        // Lo reservado no pasa a existencias; lo disponible, sí.
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/existencias", new { Plantas = 4_000 }), HttpStatusCode.Conflict)).Should().Be("lote_planta.sin_plantas");
        lote = await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/existencias", new { Plantas = 3_000, Fecha = Hoy }));
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{planta}"))!.Stock.Should().Be(3_000m);

        // Entrega del encargo con albarán; el pasaporte lleva especie, registro, trazabilidad y origen.
        (await FalloAsync(api.GetAsync($"{ruta}/pasaporte"), HttpStatusCode.BadRequest)).Should().Be("vivero.registro");
        await OkAsync<object>(api.PutAsJsonAsync("/vivero/configuracion", new { CodigoRegistro = "es-04-12-0123", PaisOrigen = "ES" }));
        e1 = await OkAsync<EncargoResp>(Post(api, $"/vivero/encargos/{e1.Id}/servir", new { Fecha = Hoy }));
        e1.Estado.Should().Be("Servido");
        var albaran = (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{e1.AlbaranId}"))!;
        albaran.Lineas.Single().Should().Match<LineaAlbaranResp>(l => l.Cantidad == 6_000m && l.PrecioUnitario == 0.42m);
        albaran.Referencia.Should().Contain(lote.Codigo);
        var pasaporte = await OkAsync<PasaporteResp>(api.GetAsync($"{ruta}/pasaporte?encargoId={e1.Id}"));
        pasaporte.Should().BeEquivalentTo(new PasaporteResp("Solanum lycopersicum", "Raf", "ES-04-12-0123", lote.Codigo, "ES", 6_000, "Invernaderos del Campo SL", e1.Albaran));
        lote = await OkAsync<LoteResp>(api.GetAsync(ruta));
        (lote.PlantasVivas, lote.Reservadas, lote.Disponibles).Should().Be((700, 0, 700));

        // Una entrega se deshace anulando su encargo (y su albarán); las plantas vuelven al lote.
        var entrega = lote.Movimientos.Single(m => m.Tipo == "Entrega");
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/movimientos/{entrega.Id}/anular", new { Motivo = "x" }), HttpStatusCode.Conflict)).Should().Be("lote_planta.entrega");
        (await OkAsync<EncargoResp>(api.PostAsJsonAsync($"/vivero/encargos/{e1.Id}/anular", new { Motivo = "Devuelto: helada" }))).Estado.Should().Be("Anulado");
        (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{e1.AlbaranId}"))!.Anulado.Should().BeTrue();
        lote = await OkAsync<LoteResp>(api.GetAsync(ruta));
        lote.PlantasVivas.Should().Be(6_700);

        // El paso a existencias se anula: las plantas salen del stock y vuelven al lote.
        var paso = lote.Movimientos.Single(m => m.Tipo == "PasoExistencias");
        lote = await OkAsync<LoteResp>(api.PostAsJsonAsync($"{ruta}/movimientos/{paso.Id}/anular", new { Motivo = "Error de cantidad" }));
        lote.PlantasVivas.Should().Be(9_700);
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{planta}"))!.Stock.Should().Be(0m);
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/movimientos/{paso.Id}/anular", new { Motivo = "otra vez" }), HttpStatusCode.Conflict)).Should().Be("lote_planta.ya_anulado");
        (await FalloAsync(Post(api, $"{ruta}/anular"), HttpStatusCode.Conflict)).Should().Be("lote_planta.con_movimientos");

        // Libro del vivero: cada movimiento con sus plantas; suman las vivas.
        var libro = (await api.GetFromJsonAsync<List<LibroResp>>($"/vivero/libro?desde={Hoy:yyyy-MM-dd}"))!.Where(x => x.Lote == lote.Codigo).ToList();
        libro.Sum(x => x.Plantas).Should().Be(9_700);
        libro.Select(x => x.Tipo).Should().Contain(["Siembra", "Cambio", "Baja", "PasoExistencias", "Entrega", "Anulacion"]);

        // En la base de datos: las plantas vivas cuadran con el libro y el libro no se toca.
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{_empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var descuadre = new NpgsqlCommand($"UPDATE vivero.lote_planta SET plantas_vivas = plantas_vivas - 1 WHERE id = '{lote.Id}'", c);
        (await FluentActions.Awaiting(() => descuadre.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("lote_planta.descuadre");
        await using var borrar = new NpgsqlCommand($"DELETE FROM vivero.movimiento_lote WHERE lote_planta_id = '{lote.Id}'", c);
        (await FluentActions.Awaiting(() => borrar.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("lote_planta.inmutable");
    }
}
