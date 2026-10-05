using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Bodegas: depósitos con su composición por variedad y añada, elaboración, trasiego, coupage con reclasificación,
/// mermas, embotellado a existencias, venta a granel con albarán, declaración de existencias, anulación de la última
/// operación, trazabilidad hasta la uva y liquidación de la uva al viticultor con su autofactura.
/// </summary>
public sealed class BodegaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public BodegaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record ComponenteResp(string Variedad, int Anada, decimal Litros, decimal Porcentaje);
    private sealed record DepositoResp(Guid Id, string Codigo, decimal Litros, string? Producto, string? Calificacion, List<ComponenteResp> Composicion);
    private sealed record EntradaResp(Guid Id, string Numero, Guid? OperacionId, Guid? LiquidacionId);
    private sealed record LineaOpResp(string Deposito, string Clase, decimal Litros, string? Calificacion);
    private sealed record OperacionResp(Guid Id, string Numero, string Tipo, decimal MermaLitros, decimal? Rendimiento, Guid? AlbaranId, bool Anulada, List<LineaOpResp> Lineas);
    private sealed record FilaResp(string Producto, string? Calificacion, decimal Inicial, decimal Elaboracion, decimal EntradasInternas, decimal SalidasInternas,
        decimal Embotellado, decimal Granel, decimal Mermas, decimal Final);
    private sealed record DeclaracionResp(List<FilaResp> Filas, decimal TotalFinal);
    private sealed record OrigenUvaResp(string Entrada, string Viticultor, string Variedad);
    private sealed record OrigenResp(List<string> DepositosRecorridos, List<OrigenUvaResp> Uva);
    private sealed record LineaLiqResp(string Variedad, decimal Kilos, decimal PrecioKg, decimal Importe);
    private sealed record LiquidacionResp(Guid Id, string Numero, decimal BaseImponible, decimal TotalFactura, Guid? GastoId, string Estado, List<LineaLiqResp> Lineas);
    private sealed record ProductoResp(decimal Stock);
    private sealed record LineaAlbaranResp(decimal Cantidad, decimal PrecioUnitario);
    private sealed record AlbaranResp(List<LineaAlbaranResp> Lineas, bool Anulado);

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

    private static async Task<DepositoResp> DepositoAsync(HttpClient api, Guid id) => (await api.GetFromJsonAsync<DepositoResp>($"/bodega/depositos/{id}"))!;

    [Fact]
    public async Task El_modulo_se_contrata_aparte()
    {
        var sin = await EmpresaAsync();
        (await FalloAsync(sin.GetAsync("/bodega/depositos"), HttpStatusCode.Forbidden)).Should().Be("modulo.no_contratado");
        var con = await EmpresaAsync("bodega");
        (await con.GetAsync("/bodega/depositos")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task De_la_uva_a_la_botella_con_composicion_declaracion_anulacion_trazabilidad_y_liquidacion()
    {
        var api = await EmpresaAsync("bodega");
        Task<Guid> Deposito(string codigo, string tipo, decimal capacidad) => IdAsync(api, "/bodega/depositos", new { Codigo = codigo, Tipo = tipo, CapacidadLitros = capacidad });
        var d1 = await Deposito("D1", "Acero", 10_000m);
        var d2 = await Deposito("D2", "Acero", 5_000m);
        var barrica = await Deposito("B1", "Barrica", 225m);
        var d4 = await Deposito("D4", "Hormigon", 8_000m);
        var d5 = await Deposito("D5", "Acero", 2_000m);
        (await FalloAsync(api.PostAsJsonAsync("/bodega/depositos", new { Codigo = "d1", Tipo = "Acero", CapacidadLitros = 1m }), HttpStatusCode.Conflict)).Should().Be("deposito.duplicado");

        var ana = await IdAsync(api, "/proveedores", new { Nombre = "Ana Viñedos", NifFiscal = Ayudas.GenerarNif() });
        var luis = await IdAsync(api, "/proveedores", new { Nombre = "Luis Cepas", NifFiscal = Ayudas.GenerarNif() });
        var e1 = await OkAsync<EntradaResp>(api.PostAsJsonAsync("/bodega/uva", new { ViticultorId = ana, Variedad = "Tempranillo", Kilos = 6_000m, GradoBaume = 12.5m, Calificacion = "Rioja", Fecha = Hoy }));
        var e2 = await OkAsync<EntradaResp>(api.PostAsJsonAsync("/bodega/uva", new { ViticultorId = luis, Variedad = "Garnacha", Kilos = 2_000m, GradoBaume = 13m, Calificacion = "Rioja", Fecha = Hoy }));
        var e3 = await OkAsync<EntradaResp>(api.PostAsJsonAsync("/bodega/uva", new { ViticultorId = ana, Variedad = "Tempranillo", Kilos = 1_000m, GradoBaume = 11m, Fecha = Hoy }));
        e1.Numero.Should().Be($"U{Hoy.Year}/00001");

        // Elaboración: 5.600 l de tinto de Rioja con la composición por kilos (6.000 : 2.000).
        var elab = await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/elaboracion",
            new { EntradaIds = new[] { e1.Id, e2.Id }, DepositoId = d1, Litros = 5_600m, Producto = "VinoTinto", Fecha = Hoy }));
        elab.Rendimiento.Should().Be(70m);
        var dep1 = await DepositoAsync(api, d1);
        (dep1.Litros, dep1.Producto, dep1.Calificacion).Should().Be((5_600m, "VinoTinto", "Rioja"));
        dep1.Composicion.Select(c => (c.Variedad, c.Litros)).Should().BeEquivalentTo([("Tempranillo", 4_200m), ("Garnacha", 1_400m)]);
        (await FalloAsync(api.PostAsJsonAsync("/bodega/operaciones/elaboracion", new { EntradaIds = new[] { e1.Id }, DepositoId = d2, Litros = 100m, Producto = "VinoTinto" }),
            HttpStatusCode.Conflict)).Should().Be("uva.elaborada");
        (await FalloAsync(api.PostAsJsonAsync("/bodega/operaciones/elaboracion", new { EntradaIds = new[] { e3.Id }, DepositoId = barrica, Litros = 700m, Producto = "VinoTinto" }),
            HttpStatusCode.Conflict)).Should().Be("deposito.capacidad");
        (await FalloAsync(api.PostAsJsonAsync("/bodega/operaciones/elaboracion", new { EntradaIds = new[] { e3.Id }, DepositoId = d1, Litros = 700m, Producto = "VinoBlanco" }),
            HttpStatusCode.Conflict)).Should().Be("deposito.mezcla_producto");
        await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/elaboracion", new { EntradaIds = new[] { e3.Id }, DepositoId = d2, Litros = 700m, Producto = "VinoTinto", Fecha = Hoy }));

        // Trasiego con merma y coupage que mezcla Rioja con vino sin indicación: el conjunto pierde la calificación.
        var trasiego = await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/trasiego", new { OrigenId = d1, DestinoId = d4, Litros = 2_000m, MermaLitros = 10m, Fecha = Hoy }));
        (await DepositoAsync(api, d1)).Litros.Should().Be(3_590m);
        (await DepositoAsync(api, d4)).Composicion.Select(c => (c.Variedad, c.Litros)).Should().BeEquivalentTo([("Tempranillo", 1_500m), ("Garnacha", 500m)]);
        var coupage = await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/coupage",
            new { Origenes = new[] { new { DepositoId = d4, Litros = 500m }, new { DepositoId = d2, Litros = 700m } }, DestinoId = d5, Fecha = Hoy }));
        coupage.Lineas.Should().Contain(l => l.Clase == "Reclasifica" && l.Litros == -500m && l.Calificacion == "Rioja");
        var dep5 = await DepositoAsync(api, d5);
        (dep5.Litros, dep5.Calificacion).Should().Be((1_200m, null));
        dep5.Composicion.Select(c => (c.Variedad, c.Litros)).Should().BeEquivalentTo([("Tempranillo", 1_075m), ("Garnacha", 125m)]);

        // Merma, embotellado (las botellas entran en existencias) y venta a granel con albarán.
        (await FalloAsync(api.PostAsJsonAsync("/bodega/operaciones/merma", new { DepositoId = d1, Litros = 40m }), HttpStatusCode.BadRequest)).Should().Be("bodega.motivo");
        await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/merma", new { DepositoId = d1, Litros = 40m, Observaciones = "Evaporación", Fecha = Hoy }));
        var crianza = await IdAsync(api, "/productos", new { Nombre = "Crianza 75 cl", PrecioUnitario = 9m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        var embotellado = await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/embotellado",
            new { DepositoId = d1, ProductoId = crianza, Botellas = 1_000, FormatoLitros = 0.75m, MermaLitros = 5m, Lote = "L-CR-1", Fecha = Hoy }));
        (await DepositoAsync(api, d1)).Litros.Should().Be(2_795m);
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{crianza}"))!.Stock.Should().Be(1_000m);
        var granelArt = await IdAsync(api, "/productos", new { Nombre = "Tinto a granel", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "l" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Bodegas Compradoras SA", NifFiscal = Ayudas.GenerarNif() });
        var granel = await OkAsync<OperacionResp>(api.PostAsJsonAsync("/bodega/operaciones/granel",
            new { DepositoId = d4, ClienteId = cliente, ProductoId = granelArt, Litros = 1_500m, PrecioLitro = 1.20m, Fecha = Hoy }));
        var albaran = (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{granel.AlbaranId}"))!;
        albaran.Lineas.Single().Should().Match<LineaAlbaranResp>(l => l.Cantidad == 1_500m && l.PrecioUnitario == 1.20m);
        (await DepositoAsync(api, d4)).Should().Match<DepositoResp>(d => d.Litros == 0m && d.Producto == null && d.Composicion.Count == 0);

        // Declaración del mes: por producto y calificación; cuadra con lo que hay en los depósitos.
        var decl = (await api.GetFromJsonAsync<DeclaracionResp>($"/bodega/declaracion?anio={Hoy.Year}&mes={Hoy.Month}"))!;
        decl.Filas.Single(f => f.Calificacion == "Rioja").Should().BeEquivalentTo(new FilaResp("VinoTinto", "Rioja", 0m, 5_600m, 2_500m, 3_000m, 750m, 1_500m, 55m, 2_795m));
        decl.Filas.Single(f => f.Calificacion == null).Should().BeEquivalentTo(new FilaResp("VinoTinto", null, 0m, 700m, 1_200m, 700m, 0m, 0m, 0m, 1_200m));
        decl.TotalFinal.Should().Be(3_995m);

        // Solo se deshace la última operación de cada depósito.
        (await FalloAsync(api.PostAsJsonAsync($"/bodega/operaciones/{trasiego.Id}/anular", new { Motivo = "Error" }), HttpStatusCode.Conflict)).Should().Be("operacion_bodega.posterior");
        (await OkAsync<OperacionResp>(api.PostAsJsonAsync($"/bodega/operaciones/{granel.Id}/anular", new { Motivo = "No lo recogió" }))).Anulada.Should().BeTrue();
        (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{granel.AlbaranId}"))!.Anulado.Should().BeTrue();
        (await DepositoAsync(api, d4)).Should().Match<DepositoResp>(d => d.Litros == 1_500m && d.Calificacion == "Rioja" && d.Composicion.Count == 2);
        await OkAsync<OperacionResp>(api.PostAsJsonAsync($"/bodega/operaciones/{embotellado.Id}/anular", new { Motivo = "Lote mal etiquetado" }));
        (await DepositoAsync(api, d1)).Litros.Should().Be(3_550m);
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{crianza}"))!.Stock.Should().Be(0m);

        // Trazabilidad del coupage hasta la uva: las tres entradas, por D4 y D1 y por D2.
        var origen = (await api.GetFromJsonAsync<OrigenResp>($"/bodega/depositos/{d5}/origen"))!;
        origen.DepositosRecorridos.Should().BeEquivalentTo(["D1", "D2", "D4", "D5"]);
        origen.Uva.Select(u => (u.Viticultor, u.Variedad)).Should().BeEquivalentTo([("Ana Viñedos", "Tempranillo"), ("Luis Cepas", "Garnacha"), ("Ana Viñedos", "Tempranillo")]);

        // Liquidación de la uva: precio de la variedad corregido por el grado (±5 % por grado sobre el de referencia).
        (await FalloAsync(api.PostAsJsonAsync("/bodega/liquidaciones/simular", new { ViticultorId = ana, Desde = Hoy, Hasta = Hoy }), HttpStatusCode.BadRequest))
            .Should().Be("liquidacion_uva.sin_precio");
        await OkAsync<object>(api.PutAsJsonAsync("/bodega/precios-uva", new { Anada = Hoy.Year, Variedad = "Tempranillo", PrecioKg = 0.60m, GradoReferencia = 12m, PorcentajePorGrado = 5m }));
        var simulada = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/bodega/liquidaciones/simular", new { ViticultorId = ana, Desde = Hoy, Hasta = Hoy }));
        simulada.Lineas.Select(l => (l.Kilos, l.PrecioKg, l.Importe)).Should().BeEquivalentTo([(6_000m, 0.615m, 3_690m), (1_000m, 0.57m, 570m)]);
        var liq = await OkAsync<LiquidacionResp>(api.PostAsJsonAsync("/bodega/liquidaciones", new { ViticultorId = ana, Desde = Hoy, Hasta = Hoy, Fecha = Hoy }));
        (liq.BaseImponible, liq.TotalFactura).Should().Be((4_260m, 4_771.20m));
        liq.GastoId.Should().NotBeNull();
        (await FalloAsync(api.PostAsJsonAsync("/bodega/liquidaciones", new { ViticultorId = ana, Desde = Hoy, Hasta = Hoy }), HttpStatusCode.BadRequest))
            .Should().Be("liquidacion_uva.sin_entradas");
        (await FalloAsync(api.PostAsJsonAsync($"/bodega/uva/{e1.Id}/anular", new { Motivo = "x" }), HttpStatusCode.Conflict)).Should().Be("uva.usada");
        (await OkAsync<LiquidacionResp>(api.PostAsJsonAsync($"/bodega/liquidaciones/{liq.Id}/anular", new { Motivo = "Precio mal" }))).Estado.Should().Be("Anulada");
        (await api.GetFromJsonAsync<List<EntradaResp>>($"/bodega/uva?viticultorId={ana}"))!.Should().OnlyContain(e => e.LiquidacionId == null);

        // En la base de datos: lo que contiene un depósito cuadra con su composición y no pasa de su capacidad.
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{_empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var sinCuadrar = new NpgsqlCommand($"UPDATE bodega.deposito SET litros = litros + 1 WHERE id = '{d1}'", c);
        (await FluentActions.Awaiting(() => sinCuadrar.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("deposito.composicion");
        await using var lleno = new NpgsqlCommand($"UPDATE bodega.deposito SET capacidad_litros = 100 WHERE id = '{d1}'", c);
        (await FluentActions.Awaiting(() => lleno.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.ConstraintName.Should().Be("ck_deposito_litros");
        await using var borrar = new NpgsqlCommand($"DELETE FROM bodega.linea_operacion WHERE operacion_id = '{elab.Id}'", c);
        (await FluentActions.Awaiting(() => borrar.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("operacion_bodega.inmutable");
    }
}
