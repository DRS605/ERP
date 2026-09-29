using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Cooperativas y SAT: socios, capital social con sus asientos, reparto del excedente con retorno cooperativo, retenciones, libros y actas.</summary>
public sealed class CooperativaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CooperativaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 3, 10);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record SaldoResp(decimal ObligatorioSuscrito, decimal ObligatorioDesembolsado, decimal VoluntarioSuscrito, decimal VoluntarioDesembolsado, decimal Suscrito,
        decimal Desembolsado, decimal Pendiente);
    private sealed record SocioResp(Guid Id, int Numero, Guid ProveedorId, string Nombre, string? Nif, string Tipo, DateOnly FechaAlta, DateOnly? FechaBaja, string? MotivoBaja,
        SaldoResp Capital, decimal FaltaParaElMinimo);
    private sealed record MovimientoResp(Guid Id, Guid SocioId, int NumeroSocio, DateOnly Fecha, string Tipo, string Clase, decimal Suscrito, decimal Desembolsado, decimal Deduccion,
        decimal ADevolver, Guid? GrupoId, bool Anulado, Guid? AsientoId, decimal SuscritoAcumulado, decimal DesembolsadoAcumulado);
    private sealed record CapitalSocioResp(SocioResp Socio, List<MovimientoResp> Movimientos);
    private sealed record ResumenResp(int Socios, int SociosActivos, SaldoResp Total, int SociosBajoElMinimo);
    private sealed record LibroSocioResp(int Numero, string Nombre, string? Nif, string? Domicilio, string Tipo, DateOnly? FechaBaja, SaldoResp Capital);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(Guid Id, string Concepto, decimal Total, List<ApunteResp> Apuntes, Guid? AnulaAsientoId);
    private sealed record LineaRepartoResp(Guid SocioId, int NumeroSocio, decimal Actividad, decimal Capital, decimal Intereses, decimal Retorno, decimal Retencion,
        decimal Capitalizado, decimal Neto);
    private sealed record RepartoResp(Guid Id, int Ejercicio, string Estado, string Base, decimal Excedente, decimal ImporteFro, decimal ImporteFep, decimal ReservasVoluntarias,
        decimal ImporteIntereses, decimal ImporteRetorno, decimal TotalRetencion, decimal TotalCapitalizado, decimal TotalNeto, Guid? AsientoId, List<LineaRepartoResp> Lineas);
    private sealed record RetencionResp(Guid SocioId, string? Nif, decimal Integro, decimal Retencion);
    private sealed record RetencionesResp(decimal Integro, decimal Retencion, List<RetencionResp> Socios);
    private sealed record ActaResp(Guid Id, string Organo, int? Numero, string Estado);
    private sealed record ConfigResp(string Forma, decimal PorcentajeFroMinimo, decimal PorcentajeFepMinimo, string Base);

    private async Task<(HttpClient Api, Guid Empresa)> EmpresaAsync(params string[] modulos)
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = modulos })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return (api, empresa);
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

    private static async Task<Guid> ProveedorAsync(HttpClient api, string nombre) =>
        await IdAsync(api, "/proveedores", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif(), Calle = "Camí Real 1", CodigoPostal = "46000", Poblacion = "Algemesí", Provincia = "Valencia" });

    private static async Task<SocioResp> SocioAsync(HttpClient api, Guid proveedor, string tipo = "Comun", decimal? aportacion = null, decimal? desembolsado = null) =>
        await OkAsync<SocioResp>(api.PostAsJsonAsync("/cooperativa/socios", new { ProveedorId = proveedor, Tipo = tipo, FechaAlta = new DateOnly(Anio, 1, 2), Aportacion = aportacion,
            Desembolsado = desembolsado }));

    private static async Task<List<AsientoResp>> DiarioAsync(HttpClient api) =>
        (await api.GetFromJsonAsync<List<AsientoResp>>($"/contabilidad/diario?ejercicio={Anio}"))!;

    private static Dictionary<string, (decimal Debe, decimal Haber)> Apuntes(AsientoResp a) =>
        a.Apuntes.GroupBy(x => x.CuentaCodigo).ToDictionary(g => g.Key, g => (g.Sum(x => x.Debe), g.Sum(x => x.Haber)));

    private static async Task<PostgresException> RechazoAsync(Guid empresa, string sql)
    {
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand(sql, c);
        var ex = await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
        return ex.Which;
    }

    [Fact]
    public async Task El_modulo_se_contrata_aparte()
    {
        var (api, _) = await EmpresaAsync();
        (await FalloAsync(api.GetAsync("/cooperativa/socios"), HttpStatusCode.Forbidden)).Should().Be("modulo.no_contratado");
        var (con, _) = await EmpresaAsync("cooperativa");
        (await con.GetAsync("/cooperativa/socios")).StatusCode.Should().Be(HttpStatusCode.OK);
        var config = await OkAsync<ConfigResp>(con.GetAsync("/cooperativa/configuracion"));
        config.Should().Match<ConfigResp>(c => c.Forma == "Cooperativa" && c.PorcentajeFroMinimo == 20m && c.PorcentajeFepMinimo == 5m && c.Base == "Kilos");
    }

    [Fact]
    public async Task Socios_y_capital_con_sus_asientos_y_los_libros_registro()
    {
        var (api, _) = await EmpresaAsync("cooperativa");
        (await api.PutAsJsonAsync("/cooperativa/configuracion", new { Forma = "Cooperativa", AportacionObligatoria = 600m })).EnsureSuccessStatusCode();
        var pa = await ProveedorAsync(api, "Rosa Llauradora");
        var pb = await ProveedorAsync(api, "Vicent Hortolà");

        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/socios", new { ProveedorId = pa, Aportacion = 500m }), HttpStatusCode.BadRequest))
            .Should().Be("socio.aportacion_minima");
        var a = await SocioAsync(api, pa, aportacion: 600m, desembolsado: 300m);
        a.Numero.Should().Be(1);
        a.Capital.Should().Match<SaldoResp>(s => s.ObligatorioSuscrito == 600m && s.ObligatorioDesembolsado == 300m && s.Pendiente == 300m);
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/socios", new { ProveedorId = pa }), HttpStatusCode.Conflict)).Should().Be("socio.duplicado");
        var b = await SocioAsync(api, pb);
        b.Numero.Should().Be(2);
        b.FaltaParaElMinimo.Should().Be(600m);

        // Alta con aportación: 103 contra 100 por lo suscrito y 572 contra 103 por lo desembolsado (neto por cuenta).
        var alta = (await DiarioAsync(api)).Single(x => x.Concepto.StartsWith("Socio 1 ", StringComparison.Ordinal));
        Apuntes(alta).Should().BeEquivalentTo(new Dictionary<string, (decimal, decimal)> { ["103"] = (300m, 0m), ["572"] = (300m, 0m), ["100"] = (0m, 600m) });

        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/capital/desembolsos", new { SocioId = a.Id, Clase = "Obligatoria", Importe = 400m, Fecha = Dia }), HttpStatusCode.BadRequest))
            .Should().Be("capital.sin_pendiente");
        await OkAsync<MovimientoResp>(api.PostAsJsonAsync("/cooperativa/capital/desembolsos", new { SocioId = a.Id, Clase = "Obligatoria", Importe = 300m, Fecha = Dia }));
        await OkAsync<MovimientoResp>(api.PostAsJsonAsync("/cooperativa/capital/suscripciones",
            new { SocioId = a.Id, Clase = "Voluntaria", Suscrito = 1000m, Desembolsado = 1000m, Fecha = Dia }));
        await OkAsync<MovimientoResp>(api.PostAsJsonAsync("/cooperativa/capital/suscripciones", new { SocioId = b.Id, Clase = "Obligatoria", Suscrito = 600m, Desembolsado = 600m, Fecha = Dia }));

        // Transmisión de 200 € voluntarios de A a B: sin asiento, dos movimientos unidos.
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/capital/transmisiones",
            new { DeSocioId = a.Id, ASocioId = b.Id, Clase = "Voluntaria", Importe = 5000m, Fecha = Dia }), HttpStatusCode.BadRequest)).Should().Be("capital.sin_saldo");
        var t = await OkAsync<List<MovimientoResp>>(api.PostAsJsonAsync("/cooperativa/capital/transmisiones",
            new { DeSocioId = a.Id, ASocioId = b.Id, Clase = "Voluntaria", Importe = 200m, Fecha = Dia }));
        t.Should().HaveCount(2).And.OnlyContain(m => m.AsientoId == null && m.GrupoId == t[0].GrupoId);

        // La anulación de la transmisión la deshace entera.
        var anulada = await OkAsync<List<MovimientoResp>>(api.PostAsJsonAsync($"/cooperativa/capital/movimientos/{t[1].Id}/anular", new { Motivo = "Error de socio", Fecha = Dia }));
        anulada.Should().HaveCount(2).And.OnlyContain(m => m.Tipo == "Anulacion");
        (await FalloAsync(api.PostAsJsonAsync($"/cooperativa/capital/movimientos/{t[0].Id}/anular", new { Motivo = "Otra vez", Fecha = Dia }), HttpStatusCode.Conflict))
            .Should().Be("capital.ya_anulado");
        await OkAsync<List<MovimientoResp>>(api.PostAsJsonAsync("/cooperativa/capital/transmisiones",
            new { DeSocioId = a.Id, ASocioId = b.Id, Clase = "Voluntaria", Importe = 200m, Fecha = Dia }));

        var libroA = await OkAsync<CapitalSocioResp>(api.GetAsync($"/cooperativa/socios/{a.Id}"));
        libroA.Socio.Capital.Should().Match<SaldoResp>(s => s.ObligatorioDesembolsado == 600m && s.VoluntarioSuscrito == 800m && s.Desembolsado == 1400m);
        libroA.Movimientos.Last().Should().Match<MovimientoResp>(m => m.SuscritoAcumulado == 1400m && m.DesembolsadoAcumulado == 1400m);
        libroA.Movimientos.Where(m => m.Tipo == "TransmisionSalida").Should().HaveCount(2).And.ContainSingle(m => m.Anulado);

        // El obligatorio solo se reembolsa con la baja; la deducción, dentro del máximo del motivo.
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/capital/reembolsos", new { SocioId = a.Id, Clase = "Obligatoria", Fecha = Dia }), HttpStatusCode.BadRequest))
            .Should().Be("capital.obligatorio_sin_baja");
        (await FalloAsync(api.PostAsJsonAsync($"/cooperativa/socios/{a.Id}/baja", new { Fecha = Dia, Motivo = "VoluntariaJustificada", PorcentajeDeduccion = 10m }),
            HttpStatusCode.BadRequest)).Should().Be("socio.deduccion");
        var baja = await OkAsync<SocioResp>(api.PostAsJsonAsync($"/cooperativa/socios/{a.Id}/baja", new { Fecha = Dia, Motivo = "Expulsion", PorcentajeDeduccion = 30m }));
        baja.FechaBaja.Should().Be(Dia);
        baja.Capital.Suscrito.Should().Be(0m);

        // Reembolso del obligatorio con deducción del 30 %: 100 al debe, 552 (lo que se le devuelve) y 112 (la deducción) al haber.
        var diario = await DiarioAsync(api);
        var reembolso = diario.Single(x => x.Concepto.Contains("Reembolso por baja (obligatorio)", StringComparison.Ordinal));
        Apuntes(reembolso).Should().BeEquivalentTo(new Dictionary<string, (decimal, decimal)> { ["100"] = (600m, 0m), ["552"] = (0m, 420m), ["112"] = (0m, 180m) });
        Apuntes(diario.Single(x => x.Concepto.Contains("Reembolso por baja (voluntario)", StringComparison.Ordinal)))
            .Should().BeEquivalentTo(new Dictionary<string, (decimal, decimal)> { ["100"] = (800m, 0m), ["552"] = (0m, 800m) });

        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/capital/suscripciones", new { SocioId = a.Id, Clase = "Voluntaria", Suscrito = 10m, Fecha = Dia }), HttpStatusCode.Conflict))
            .Should().Be("socio.de_baja");
        (await FalloAsync(api.DeleteAsync(new Uri($"/cooperativa/socios/{b.Id}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("socio.con_movimientos");

        // Vuelve: es un socio nuevo con otro número.
        (await SocioAsync(api, pa, aportacion: 600m)).Numero.Should().Be(3);

        var libro = await OkAsync<List<LibroSocioResp>>(api.GetAsync("/cooperativa/libros/socios"));
        libro.Select(s => s.Numero).Should().Equal(1, 2, 3);
        libro[0].Should().Match<LibroSocioResp>(s => s.FechaBaja == Dia && s.Nif != null && s.Domicilio!.Contains("Algemesí"));
        var resumen = await OkAsync<ResumenResp>(api.GetAsync("/cooperativa/capital"));
        resumen.Should().Match<ResumenResp>(r => r.Socios == 3 && r.SociosActivos == 2 && r.Total.Suscrito == 800m + 600m && r.SociosBajoElMinimo == 0);
        var aportaciones = await OkAsync<List<MovimientoResp>>(api.GetAsync($"/cooperativa/libros/aportaciones?socioId={b.Id}"));
        aportaciones.Last().SuscritoAcumulado.Should().Be(800m);
    }

    [Fact]
    public async Task Reparto_por_kilos_liquidados_con_fondos_intereses_retencion_y_capitalizacion()
    {
        var (api, _) = await EmpresaAsync("agro", "cooperativa");
        (await api.PutAsJsonAsync("/cooperativa/configuracion", new { Forma = "Cooperativa", AportacionObligatoria = 500m })).EnsureSuccessStatusCode();

        // Agro: dos socios entregan naranja (10.000 y 5.000 kg) y se les liquida.
        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        (await api.PutAsJsonAsync($"/agro/campanas/{campana}/articulos", new { ProductoId = naranja, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(api, $"/agro/campanas/{campana}/precios", new { ProductoId = naranja, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), PrecioKg = 0.30m });
        var proveedores = new List<Guid>();
        foreach (var (nombre, neto) in new[] { ("Rosa Llauradora", 10_000m), ("Vicent Hortolà", 5_000m) })
        {
            var proveedor = await ProveedorAsync(api, nombre);
            proveedores.Add(proveedor);
            var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) });
            var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = $"P-{proveedores.Count}", Nombre = "Hort", SuperficieHa = 2m });
            var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = Dia });
            var linea = (await OkAsync<RecepcionResp>(api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
                new { ProductoId = naranja, ParcelaId = parcela, FechaRecoleccion = Dia, EnvaseProductoId = palot }))).Lineas.Single().Id;
            (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = neto + 1_000m, TaraKg = 1_000m, Envases = 20, Bascula = "B1" }))
                .EnsureSuccessStatusCode();
            (await Post(api, $"/agro/recepciones/{rec}/confirmar")).EnsureSuccessStatusCode();
            var liq = await OkAsync<IdResp>(api.PostAsJsonAsync("/agro/liquidaciones",
                new { AgricultorId = agricultor, CampanaId = campana, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), Fecha = new DateOnly(Anio, 3, 31) }));
            (await Post(api, $"/agro/liquidaciones/{liq.Id}/emitir")).EnsureSuccessStatusCode();
        }

        var a = await SocioAsync(api, proveedores[0], aportacion: 1000m, desembolsado: 1000m);
        var b = await SocioAsync(api, proveedores[1], aportacion: 1000m, desembolsado: 500m);
        var c = await SocioAsync(api, await ProveedorAsync(api, "Capital Amic SL"), tipo: "Colaborador");
        await OkAsync<MovimientoResp>(api.PostAsJsonAsync("/cooperativa/capital/suscripciones", new { SocioId = c.Id, Clase = "Voluntaria", Suscrito = 2000m, Desembolsado = 2000m, Fecha = Dia }));

        // Fondos por debajo del mínimo o interés por encima del máximo: no.
        var fecha = new DateOnly(Anio, 12, 31);
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/repartos/simular", new { Ejercicio = Anio, Excedente = 10_000m, Fecha = fecha, PorcentajeFro = 10m }),
            HttpStatusCode.BadRequest)).Should().Be("reparto.fondos");
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/repartos/simular", new { Ejercicio = Anio, Excedente = 10_000m, Fecha = fecha, PorcentajeIntereses = 12m }),
            HttpStatusCode.BadRequest)).Should().Be("reparto.intereses");

        // 10.000 €: FRO 2.000, FEP 500, intereses al 5 % (50 + 25 + 100), reservas 325 y retorno 7.000 por kilos (2/3 y 1/3).
        var datos = new { Ejercicio = Anio, Excedente = 10_000m, Fecha = fecha, PorcentajeIntereses = 5m, ReservasVoluntarias = 325m, Capitalizan = new[] { a.Id } };
        var r = await OkAsync<RepartoResp>(api.PostAsJsonAsync("/cooperativa/repartos", datos));
        r.Should().Match<RepartoResp>(x => x.Estado == "Borrador" && x.ImporteFro == 2000m && x.ImporteFep == 500m && x.ImporteIntereses == 175m && x.ImporteRetorno == 7000m);
        var la = r.Lineas.Single(l => l.SocioId == a.Id);
        la.Should().Match<LineaRepartoResp>(l => l.Actividad == 10_000m && l.Retorno == 4666.67m && l.Capitalizado == 4666.67m && l.Intereses == 50m && l.Retencion == 9.50m
            && l.Neto == 40.50m);
        r.Lineas.Single(l => l.SocioId == b.Id).Should().Match<LineaRepartoResp>(l => l.Retorno == 2333.33m && l.Intereses == 25m && l.Retencion == 448.08m && l.Neto == 1910.25m);
        r.Lineas.Single(l => l.SocioId == c.Id).Should().Match<LineaRepartoResp>(l => l.Actividad == 0m && l.Retorno == 0m && l.Intereses == 100m && l.Neto == 81m);
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/repartos", datos), HttpStatusCode.Conflict)).Should().Be("reparto.duplicado");

        // Recalcular sin capitalizar, y volver a capitalizar.
        (await OkAsync<RepartoResp>(api.PutAsJsonAsync($"/cooperativa/repartos/{r.Id}", datos with { Capitalizan = Array.Empty<Guid>() }))).TotalCapitalizado.Should().Be(0m);
        await OkAsync<RepartoResp>(api.PutAsJsonAsync($"/cooperativa/repartos/{r.Id}", datos));

        var cont = await OkAsync<RepartoResp>(Post(api, $"/cooperativa/repartos/{r.Id}/contabilizar"));
        cont.Estado.Should().Be("Contabilizado");
        var asiento = (await DiarioAsync(api)).Single(x => x.Id == cont.AsientoId);
        Apuntes(asiento).Should().BeEquivalentTo(new Dictionary<string, (decimal, decimal)>
        {
            ["129"] = (10_000m, 0m), ["112"] = (0m, 2000m), ["144"] = (0m, 500m), ["113"] = (0m, 325m), ["526"] = (0m, 2031.75m), ["4751"] = (0m, 476.58m), ["100"] = (0m, 4666.67m),
        });
        (await OkAsync<CapitalSocioResp>(api.GetAsync($"/cooperativa/socios/{a.Id}"))).Socio.Capital.VoluntarioDesembolsado.Should().Be(4666.67m);
        (await FalloAsync(api.PutAsJsonAsync($"/cooperativa/repartos/{r.Id}", datos), HttpStatusCode.Conflict)).Should().Be("reparto.no_borrador");

        var ret = await OkAsync<RetencionesResp>(api.GetAsync($"/cooperativa/retenciones?ejercicio={Anio}"));
        ret.Retencion.Should().Be(476.58m);
        ret.Socios.Single(s => s.SocioId == b.Id).Should().Match<RetencionResp>(s => s.Integro == 2358.33m && s.Retencion == 448.08m && s.Nif != null);
        ret.Socios.Single(s => s.SocioId == a.Id).Integro.Should().Be(50m, "lo capitalizado no lleva retención");

        // Anular: contraasiento y el retorno capitalizado sale del capital; se puede rehacer.
        (await FalloAsync(api.PostAsJsonAsync($"/cooperativa/repartos/{r.Id}/anular", new { Motivo = "", Fecha = fecha }), HttpStatusCode.BadRequest)).Should().Be("reparto.motivo");
        (await OkAsync<RepartoResp>(api.PostAsJsonAsync($"/cooperativa/repartos/{r.Id}/anular", new { Motivo = "Acuerdo corregido", Fecha = fecha }))).Estado.Should().Be("Anulado");
        (await DiarioAsync(api)).Should().ContainSingle(x => x.AnulaAsientoId == cont.AsientoId);
        (await OkAsync<CapitalSocioResp>(api.GetAsync($"/cooperativa/socios/{a.Id}"))).Socio.Capital.VoluntarioDesembolsado.Should().Be(0m);
        (await OkAsync<RetencionesResp>(api.GetAsync($"/cooperativa/retenciones?ejercicio={Anio}"))).Socios.Should().BeEmpty();
        await OkAsync<RepartoResp>(api.PostAsJsonAsync("/cooperativa/repartos", datos));
    }

    private sealed record LineaRecepcionResp(Guid Id);
    private sealed record RecepcionResp(List<LineaRecepcionResp> Lineas);

    [Fact]
    public async Task Una_SAT_reparte_por_capital_sin_fondos_y_al_centimo()
    {
        var (api, _) = await EmpresaAsync("cooperativa");
        var config = await OkAsync<ConfigResp>(api.PutAsJsonAsync("/cooperativa/configuracion", new { Forma = "Sat" }));
        config.Should().Match<ConfigResp>(c => c.PorcentajeFroMinimo == 0m && c.PorcentajeFepMinimo == 0m && c.Base == "Capital");
        var socios = new List<SocioResp>();
        foreach (var nombre in new[] { "Uno", "Dos", "Tres" })
        {
            socios.Add(await SocioAsync(api, await ProveedorAsync(api, nombre), aportacion: 1000m, desembolsado: 1000m));
        }

        var r = await OkAsync<RepartoResp>(api.PostAsJsonAsync("/cooperativa/repartos/simular", new { Ejercicio = Anio, Excedente = 100m, Fecha = new DateOnly(Anio, 12, 31) }));
        r.Base.Should().Be("Capital");
        r.ImporteFro.Should().Be(0m);
        r.Lineas.Sum(l => l.Retorno).Should().Be(100m);
        r.Lineas.Select(l => l.Retorno).Should().BeEquivalentTo([33.34m, 33.33m, 33.33m]);
        (await api.GetFromJsonAsync<List<RepartoResp>>("/cooperativa/repartos"))!.Should().BeEmpty("simular no guarda nada");

        // Actividad a mano (sin agro): solo de socios.
        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/repartos/simular",
            new { Ejercicio = Anio, Excedente = 100m, Fecha = new DateOnly(Anio, 12, 31), Actividad = new[] { new { SocioId = Guid.NewGuid(), Actividad = 5m } } }),
            HttpStatusCode.BadRequest)).Should().Be("reparto.socio");
        var manual = await OkAsync<RepartoResp>(api.PostAsJsonAsync("/cooperativa/repartos/simular", new
        {
            Ejercicio = Anio, Excedente = 100m, Fecha = new DateOnly(Anio, 12, 31),
            Actividad = new[] { new { SocioId = socios[0].Id, Actividad = 3m }, new { SocioId = socios[1].Id, Actividad = 1m } },
        }));
        manual.Lineas.Single(l => l.SocioId == socios[0].Id).Retorno.Should().Be(75m);
        manual.Lineas.Should().NotContain(l => l.SocioId == socios[2].Id, "sin actividad ni intereses no le toca nada");
    }

    [Fact]
    public async Task Las_actas_se_numeran_al_aprobarlas_y_quedan_fijas()
    {
        var (api, _) = await EmpresaAsync("cooperativa");
        object Acta(string acuerdos) => new { Fecha = Dia, Caracter = "Ordinaria", OrdenDelDia = "1. Cuentas del ejercicio", Acuerdos = acuerdos, Presentes = 40, Representados = 5 };

        (await FalloAsync(api.PostAsJsonAsync("/cooperativa/actas",
            new { Organo = "AsambleaGeneral", Acta = new { Fecha = Dia, Caracter = "Ordinaria", OrdenDelDia = "x", Acuerdos = "" } }), HttpStatusCode.BadRequest))
            .Should().Be("acta.texto");
        var primera = await IdAsync(api, "/cooperativa/actas", new { Organo = "AsambleaGeneral", Acta = Acta("Se aprueban las cuentas.") });
        var borrador = await IdAsync(api, "/cooperativa/actas", new { Organo = "AsambleaGeneral", Acta = Acta("Borrador") });
        var consejo = await IdAsync(api, "/cooperativa/actas", new { Organo = "ConsejoRector", Acta = Acta("Se convoca la asamblea.") });
        (await api.PutAsJsonAsync($"/cooperativa/actas/{primera}", Acta("Se aprueban las cuentas y el reparto."))).EnsureSuccessStatusCode();

        (await OkAsync<ActaResp>(Post(api, $"/cooperativa/actas/{primera}/aprobar"))).Numero.Should().Be(1);
        (await OkAsync<ActaResp>(Post(api, $"/cooperativa/actas/{consejo}/aprobar"))).Numero.Should().Be(1, "cada órgano lleva su libro");
        (await FalloAsync(api.PutAsJsonAsync($"/cooperativa/actas/{primera}", Acta("Cambio")), HttpStatusCode.Conflict)).Should().Be("acta.aprobada");
        (await FalloAsync(api.DeleteAsync(new Uri($"/cooperativa/actas/{primera}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("acta.aprobada");
        (await api.DeleteAsync(new Uri($"/cooperativa/actas/{borrador}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var segunda = await IdAsync(api, "/cooperativa/actas", new { Organo = "AsambleaGeneral", Acta = Acta("Segunda") });
        (await OkAsync<ActaResp>(Post(api, $"/cooperativa/actas/{segunda}/aprobar"))).Numero.Should().Be(2, "el borrador eliminado no deja hueco");
        (await api.GetFromJsonAsync<List<ActaResp>>("/cooperativa/actas?organo=AsambleaGeneral"))!.Select(x => x.Numero).Should().Equal(1, 2);
    }

    [Fact]
    public async Task La_base_de_datos_protege_el_capital_los_socios_los_repartos_y_las_actas()
    {
        var (api, empresa) = await EmpresaAsync("cooperativa");
        var a = await SocioAsync(api, await ProveedorAsync(api, "Rosa"), aportacion: 100m, desembolsado: 100m);
        var b = await SocioAsync(api, await ProveedorAsync(api, "Vicent"));
        var mov = (await OkAsync<CapitalSocioResp>(api.GetAsync($"/cooperativa/socios/{a.Id}"))).Movimientos.Single();

        (await RechazoAsync(empresa, $"UPDATE cooperativa.movimiento_capital SET suscrito = 1 WHERE id = '{mov.Id}'")).Hint.Should().Be("capital.inmutable");
        (await RechazoAsync(empresa, $"DELETE FROM cooperativa.movimiento_capital WHERE id = '{mov.Id}'")).Hint.Should().Be("capital.inmutable");
        (await RechazoAsync(empresa, $"""
            INSERT INTO cooperativa.movimiento_capital (id, empresa_id, socio_id, fecha, tipo, clase, suscrito, desembolsado, deduccion, concepto, creado_en)
            VALUES (gen_random_uuid(), '{empresa}', '{b.Id}', '{Dia:yyyy-MM-dd}', 'Desembolso', 'Obligatoria', 0, 50, 0, 'x', now())
            """)).Hint.Should().Be("capital.descuadre");
        (await RechazoAsync(empresa, $"""
            INSERT INTO cooperativa.movimiento_capital (id, empresa_id, socio_id, fecha, tipo, clase, suscrito, desembolsado, deduccion, concepto, creado_en, anula_id)
            VALUES (gen_random_uuid(), '{empresa}', '{a.Id}', '{Dia:yyyy-MM-dd}', 'Anulacion', 'Obligatoria', -50, -50, 0, 'x', now(), '{mov.Id}')
            """)).Hint.Should().Be("capital.anulacion");
        (await RechazoAsync(empresa, $"UPDATE cooperativa.socio SET numero = 99 WHERE id = '{a.Id}'")).Hint.Should().Be("socio.inmutable");

        (await api.PostAsJsonAsync($"/cooperativa/socios/{b.Id}/baja", new { Fecha = Dia, Motivo = "VoluntariaJustificada" })).EnsureSuccessStatusCode();
        (await RechazoAsync(empresa, $"UPDATE cooperativa.socio SET fecha_baja = NULL, motivo_baja = NULL WHERE id = '{b.Id}'")).Hint.Should().Be("socio.baja_definitiva");

        var r = await OkAsync<RepartoResp>(api.PostAsJsonAsync("/cooperativa/repartos",
            new { Ejercicio = Anio, Excedente = 1000m, Fecha = new DateOnly(Anio, 12, 31), Actividad = new[] { new { SocioId = a.Id, Actividad = 1m } } }));
        (await RechazoAsync(empresa, $"UPDATE cooperativa.linea_reparto SET retorno = retorno + 1, neto = neto + 1 WHERE reparto_id = '{r.Id}'")).Hint.Should().Be("reparto.descuadre");
        await OkAsync<RepartoResp>(Post(api, $"/cooperativa/repartos/{r.Id}/contabilizar"));
        (await RechazoAsync(empresa, $"UPDATE cooperativa.reparto SET importe_fro = importe_fro + 1, importe_retorno = importe_retorno - 1 WHERE id = '{r.Id}'"))
            .Hint.Should().Be("reparto.contabilizado");
        (await RechazoAsync(empresa, $"DELETE FROM cooperativa.linea_reparto WHERE reparto_id = '{r.Id}'")).Hint.Should().Be("reparto.contabilizado");
        (await RechazoAsync(empresa, $"DELETE FROM cooperativa.reparto WHERE id = '{r.Id}'")).Hint.Should().Be("reparto.no_borrador");

        var acta = await IdAsync(api, "/cooperativa/actas", new { Organo = "AsambleaGeneral", Acta = new { Fecha = Dia, Caracter = "Universal", OrdenDelDia = "Único", Acuerdos = "Sí" } });
        (await Post(api, $"/cooperativa/actas/{acta}/aprobar")).EnsureSuccessStatusCode();
        (await RechazoAsync(empresa, $"UPDATE cooperativa.acta SET acuerdos = 'No' WHERE id = '{acta}'")).Hint.Should().Be("acta.aprobada");
        var otra = await IdAsync(api, "/cooperativa/actas", new { Organo = "AsambleaGeneral", Acta = new { Fecha = Dia, Caracter = "Universal", OrdenDelDia = "Único", Acuerdos = "Sí" } });
        (await RechazoAsync(empresa, $"UPDATE cooperativa.acta SET estado = 'Aprobada', numero = 5, aprobada_en = now() WHERE id = '{otra}'")).Hint.Should().Be("acta.numeracion");
    }
}
