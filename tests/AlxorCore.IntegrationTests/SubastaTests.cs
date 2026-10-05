using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Subasta hortofrutícola: lotes con los kilos sueltos de las partidas, pujas al alza y reloj a la baja, cierre con un
/// albarán por comprador, el precio de subasta en la liquidación al agricultor y la anulación (no si ya está liquidada).
/// </summary>
public sealed class SubastaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SubastaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 3, 10);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaRecepcionResp(Guid Id);
    private sealed record RecepcionResp(List<LineaRecepcionResp> Lineas);
    private sealed record SubastableResp(Guid PartidaId, string Codigo, string? Agricultor, decimal Disponible);
    private sealed record LoteResp(Guid Id, int Orden, Guid PartidaId, decimal Kilos, string Estado, Guid? CompradorId, decimal? PrecioKg, decimal Importe, decimal? MejorPuja,
        Guid? AlbaranId, string? Albaran);
    private sealed record TotalResp(Guid Id, string Nombre, int Lotes, decimal Kilos, decimal Importe);
    private sealed record SesionResp(Guid Id, string Numero, string Tipo, string Estado, int Adjudicados, int Desiertos, decimal KilosAdjudicados, decimal Importe,
        List<LoteResp> DetalleLotes, List<TotalResp> PorComprador, List<TotalResp> PorAgricultor);
    private sealed record LineaAlbaranResp(decimal Cantidad, decimal PrecioUnitario);
    private sealed record AlbaranResp(Guid Id, Guid ClienteId, List<LineaAlbaranResp> Lineas, bool Anulado);
    private sealed record PartidaResp(Guid Id, decimal Saldo);
    private sealed record LineaLiqResp(Guid PartidaId, decimal Kilos, decimal PrecioKg, decimal Importe, Guid PrecioId);
    private sealed record LiquidacionResp(Guid Id, decimal Bruto, List<LineaLiqResp> Lineas);
    private sealed record VentaResp(string Sesion, Guid CompradorId, decimal Kilos, decimal Importe);

    private async Task<HttpClient> EmpresaAsync(params string[] modulos)
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = modulos })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _empresa = empresa;
        return api;
    }

    private Guid _empresa;

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

    /// <summary>Un agricultor que entrega <paramref name="neto"/> kg de naranja (sueltos) el día de la subasta.</summary>
    private static async Task<Guid> EntregaAsync(HttpClient api, Guid naranja, string nombre, decimal neto)
    {
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif(), Calle = "Paraje Las Losas", CodigoPostal = "04700", Poblacion = "El Ejido", Provincia = "Almería" });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-" + nombre[..4], Nombre = "Invernadero", SuperficieHa = 1m });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = Dia });
        var linea = (await OkAsync<RecepcionResp>(api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = naranja, ParcelaId = parcela, FechaRecoleccion = Dia })))
            .Lineas.Single().Id;
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = neto + 500m, TaraKg = 500m, Bascula = "B1" })).EnsureSuccessStatusCode();
        var confirmar = await Post(api, $"/agro/recepciones/{rec}/confirmar");
        confirmar.IsSuccessStatusCode.Should().BeTrue(await confirmar.Content.ReadAsStringAsync());
        return agricultor;
    }

    [Fact]
    public async Task Se_contrata_aparte_y_necesita_agro()
    {
        var sin = await EmpresaAsync("agro");
        (await FalloAsync(sin.GetAsync("/subasta/sesiones"), HttpStatusCode.Forbidden)).Should().Be("modulo.no_contratado");
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "subasta" } })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Subasta_al_alza_y_a_la_baja_albaranes_por_comprador_precio_al_agricultor_y_anulacion()
    {
        var api = await EmpresaAsync("agro", "subasta");
        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Tomate rama", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        (await api.PutAsJsonAsync($"/agro/campanas/{campana}/articulos", new { ProductoId = naranja, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(api, $"/agro/campanas/{campana}/precios", new { ProductoId = naranja, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), PrecioKg = 0.30m });
        var a1 = await EntregaAsync(api, naranja, "Juana Invernadero", 10_000m);
        var a2 = await EntregaAsync(api, naranja, "Paco Enarenado", 5_000m);
        var c1 = await IdAsync(api, "/clientes", new { Nombre = "Exportadora Poniente SL", NifFiscal = Ayudas.GenerarNif() });
        var c2 = await IdAsync(api, "/clientes", new { Nombre = "Hortícolas del Sur SA", NifFiscal = Ayudas.GenerarNif() });

        var subastables = await OkAsync<List<SubastableResp>>(api.GetAsync("/subasta/partidas"));
        var p1 = subastables.Single(p => p.Agricultor == "Juana Invernadero");
        var p2 = subastables.Single(p => p.Agricultor == "Paco Enarenado");
        p1.Disponible.Should().Be(10_000m);

        // Sesión al alza: dos lotes de la partida 1 (6.000 kg y el resto) y uno de la partida 2.
        var s = await OkAsync<SesionResp>(api.PostAsJsonAsync("/subasta/sesiones", new { Fecha = Dia, Tipo = "Alza" }));
        s.Numero.Should().Be($"SU{Anio}/0001");
        var ruta = $"/subasta/sesiones/{s.Id}";
        s = await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes", new { PartidaId = p1.PartidaId, Kilos = 6_000m, Envases = 6, PrecioSalida = 0.40m }));
        s = await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes", new { PartidaId = p1.PartidaId }));
        s.DetalleLotes.Last().Kilos.Should().Be(4_000m);
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/lotes", new { PartidaId = p1.PartidaId, Kilos = 1m }), HttpStatusCode.Conflict)).Should().Be("subasta.sin_kilos");
        s = await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes", new { PartidaId = p2.PartidaId }));
        var (loteA, loteB, loteC) = (s.DetalleLotes[0].Id, s.DetalleLotes[1].Id, s.DetalleLotes[2].Id);
        (await OkAsync<List<SubastableResp>>(api.GetAsync("/subasta/partidas"))).Should().NotContain(p => p.PartidaId == p1.PartidaId || p.PartidaId == p2.PartidaId);

        // Pujas: por debajo del precio de salida o sin superar la mejor, no.
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/lotes/{loteA}/pujas", new { CompradorId = c1, PrecioKg = 0.35m }), HttpStatusCode.BadRequest)).Should().Be("subasta.bajo_salida");
        await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes/{loteA}/pujas", new { CompradorId = c1, PrecioKg = 0.45m }));
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/lotes/{loteA}/pujas", new { CompradorId = c2, PrecioKg = 0.45m }), HttpStatusCode.BadRequest)).Should().Be("subasta.puja_baja");
        s = await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes/{loteA}/pujas", new { CompradorId = c2, PrecioKg = 0.50m }));
        s.DetalleLotes[0].MejorPuja.Should().Be(0.50m);
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/lotes/{loteB}/adjudicar", new { }), HttpStatusCode.BadRequest)).Should().Be("subasta.sin_pujas");

        // A la mejor puja, a mano y desierto; lo adjudicado se puede deshacer mientras la sesión está abierta.
        s = await OkAsync<SesionResp>(Post(api, $"{ruta}/lotes/{loteA}/adjudicar", new { }));
        s.DetalleLotes[0].Should().Match<LoteResp>(l => l.Estado == "Adjudicado" && l.CompradorId == c2 && l.PrecioKg == 0.50m && l.Importe == 3_000m);
        await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes/{loteB}/adjudicar", new { CompradorId = c2, PrecioKg = 0.41m }));
        await OkAsync<SesionResp>(Post(api, $"{ruta}/lotes/{loteB}/deshacer"));
        await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta}/lotes/{loteB}/adjudicar", new { CompradorId = c1, PrecioKg = 0.42m }));
        s = await OkAsync<SesionResp>(Post(api, $"{ruta}/lotes/{loteC}/desierto"));
        (s.Adjudicados, s.Desiertos, s.KilosAdjudicados, s.Importe).Should().Be((2, 1, 10_000m, 4_680m));
        s.PorAgricultor.Single().Importe.Should().Be(4_680m);

        // Con la sesión abierta, la partida no se liquida.
        (await FalloAsync(api.PostAsJsonAsync("/agro/liquidaciones", new { AgricultorId = a1, CampanaId = campana, Desde = Dia, Hasta = Dia, Fecha = Dia }), HttpStatusCode.BadRequest))
            .Should().Be("liquidacion.subasta_pendiente");

        // Cierre: un albarán por comprador al precio adjudicado; la partida 1 se queda sin kilos y la 2 sigue entera.
        s = await OkAsync<SesionResp>(Post(api, $"{ruta}/cerrar"));
        s.Estado.Should().Be("Cerrada");
        (await FalloAsync(api.PostAsJsonAsync($"{ruta}/lotes", new { PartidaId = p2.PartidaId }), HttpStatusCode.Conflict)).Should().Be("subasta.cerrada");
        var albaranC2 = await OkAsync<AlbaranResp>(api.GetAsync($"/albaranes-venta/{s.DetalleLotes[0].AlbaranId}"));
        albaranC2.Should().Match<AlbaranResp>(a => a.ClienteId == c2 && a.Lineas.Count == 1 && a.Lineas[0].Cantidad == 6_000m && a.Lineas[0].PrecioUnitario == 0.50m);
        var albaranC1 = await OkAsync<AlbaranResp>(api.GetAsync($"/albaranes-venta/{s.DetalleLotes[1].AlbaranId}"));
        albaranC1.Should().Match<AlbaranResp>(a => a.ClienteId == c1 && a.Lineas[0].Cantidad == 4_000m && a.Lineas[0].PrecioUnitario == 0.42m);
        s.DetalleLotes[2].AlbaranId.Should().BeNull();
        var partidas = await OkAsync<List<PartidaResp>>(api.GetAsync("/agro/partidas"));
        partidas.Should().NotContain(p => p.Id == p1.PartidaId && p.Saldo != 0m);
        partidas.Single(p => p.Id == p2.PartidaId).Saldo.Should().Be(5_000m);
        (await OkAsync<List<VentaResp>>(api.GetAsync($"/subasta/ventas?compradorId={c2}"))).Should().ContainSingle().Which.Importe.Should().Be(3_000m);

        // Liquidación al agricultor 1: el precio medio de su subasta ((3.000 + 1.680) / 10.000), no el de la campaña.
        var liq = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/agro/liquidaciones", new { AgricultorId = a1, CampanaId = campana, Desde = Dia, Hasta = Dia, Fecha = Dia }));
        liq.Lineas.Should().ContainSingle().Which.Should().Match<LineaLiqResp>(l => l.Kilos == 10_000m && l.PrecioKg == 0.468m && l.PrecioId == s.Id);
        liq.Bruto.Should().Be(4_680m);
        (await FalloAsync(Post(api, $"{ruta}/anular", new { Motivo = "Error" }), HttpStatusCode.Conflict)).Should().Be("subasta.liquidada");

        // Segunda sesión, a la baja, con la partida 2: no por encima del precio de salida.
        var s2 = await OkAsync<SesionResp>(api.PostAsJsonAsync("/subasta/sesiones", new { Fecha = Dia, Tipo = "Baja" }));
        var ruta2 = $"/subasta/sesiones/{s2.Id}";
        s2 = await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta2}/lotes", new { PartidaId = p2.PartidaId, PrecioSalida = 0.60m }));
        var lote = s2.DetalleLotes.Single().Id;
        (await FalloAsync(api.PostAsJsonAsync($"{ruta2}/lotes/{lote}/pujas", new { CompradorId = c1, PrecioKg = 0.50m }), HttpStatusCode.BadRequest)).Should().Be("subasta.sin_pujas");
        (await FalloAsync(api.PostAsJsonAsync($"{ruta2}/lotes/{lote}/adjudicar", new { CompradorId = c1, PrecioKg = 0.65m }), HttpStatusCode.BadRequest)).Should().Be("subasta.precio_salida");
        await OkAsync<SesionResp>(api.PostAsJsonAsync($"{ruta2}/lotes/{lote}/adjudicar", new { CompradorId = c1, PrecioKg = 0.55m }));
        s2 = await OkAsync<SesionResp>(Post(api, $"{ruta2}/cerrar"));
        (await OkAsync<List<PartidaResp>>(api.GetAsync("/agro/partidas"))).Should().NotContain(p => p.Id == p2.PartidaId && p.Saldo != 0m);

        // Anulada (sin liquidar): su albarán se anula y los kilos vuelven a la partida.
        s2 = await OkAsync<SesionResp>(Post(api, $"{ruta2}/anular", new { Motivo = "El comprador no retiró la fruta" }));
        s2.Estado.Should().Be("Anulada");
        (await OkAsync<AlbaranResp>(api.GetAsync($"/albaranes-venta/{s2.DetalleLotes.Single().AlbaranId}"))).Anulado.Should().BeTrue();
        (await OkAsync<List<PartidaResp>>(api.GetAsync("/agro/partidas"))).Single(p => p.Id == p2.PartidaId).Saldo.Should().Be(5_000m);
        (await OkAsync<List<SubastableResp>>(api.GetAsync("/subasta/partidas"))).Single(p => p.PartidaId == p2.PartidaId).Disponible.Should().Be(5_000m);
        _ = a2;

        // En la base de datos: un lote adjudicado lleva comprador y un importe que cuadra.
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{_empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand($"UPDATE agro.lote_subasta SET importe = importe + 1 WHERE id = '{loteA}'", c);
        (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be("ck_lote_subasta_adjudicacion");
    }
}
