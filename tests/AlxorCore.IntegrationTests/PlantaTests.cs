using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Planta de confección: líneas con capacidad y turnos, calibrados que pasan a la clasificación de la partida, órdenes por
/// línea y turno con la carga frente a la capacidad, paradas, lo real del parte y las órdenes generadas desde el plan.
/// </summary>
public sealed class PlantaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PlantaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 3, 10);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaRecResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaRecResp> Lineas);
    private sealed record LineaPlantaResp(Guid Id, string Codigo, decimal CapacidadDia, List<object> Salidas);
    private sealed record LineaFicheroResp(decimal Kilos, int? Salida, int? Piezas);
    private sealed record LineaCalResp(string? Calibre, string? Categoria, bool Destrio, decimal Kilos, decimal Porcentaje, decimal? GramosPieza);
    private sealed record CalibradoResp(Guid Id, string Estado, decimal KilosEntrada, decimal KilosSalida, decimal Merma, Guid? ClasificacionId, List<LineaCalResp> Lineas);
    private sealed record LineaClasifResp(Guid CategoriaId, decimal KgMuestra);
    private sealed record ClasificacionResp(Guid Id, string Estado, List<LineaClasifResp> Lineas);
    private sealed record CalibreResp(string? Calibre, decimal Kilos, decimal Porcentaje, decimal? GramosPieza);
    private sealed record ResumenResp(int Calibrados, decimal KilosSalida, List<CalibreResp> Calibres);
    private sealed record OrdenResp(Guid Id, DateOnly Fecha, int Turno, int Secuencia, decimal Kilos, decimal HorasPrevistas, string Estado, decimal? KilosReales);
    private sealed record CargaDiaResp(string Linea, DateOnly Fecha, decimal MinutosParada, decimal Capacidad, decimal Planificado, decimal? Ocupacion, bool Sobrecarga, decimal Real,
        decimal? Disponibilidad, int Ordenes);
    private sealed record CargaResp(List<CargaDiaResp> Dias);
    private sealed record ParteResp(Guid Id, string Estado);

    private sealed record Escenario(HttpClient Api, Guid Campana, Guid Agricultor, Guid Parcela, Guid Naranja, Guid Palot, Guid Extra, Guid Primera, Guid Destrio);

    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Huerto del Río", SuperficieHa = 2.5m });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var extra = await IdAsync(api, "/agro/categorias", new { Codigo = "EXTRA", Nombre = "Extra", Orden = 1 });
        var primera = await IdAsync(api, "/agro/categorias", new { Codigo = "1A", Nombre = "Primera", Orden = 2 });
        var destrio = await IdAsync(api, "/agro/categorias", new { Codigo = "DESTRIO", Nombre = "Destrío", EsDestrio = true, Orden = 3 });
        return new Escenario(api, campana, agricultor, parcela, naranja, palot, extra, primera, destrio);
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

    /// <summary>Recepción confirmada de 10.000 kg de naranja: su partida.</summary>
    private static async Task<Guid> PartidaAsync(Escenario e)
    {
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = Dia, Matricula = "1234BCD" });
        var r = await OkAsync<RecepcionResp>(e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            new { ProductoId = e.Naranja, ParcelaId = e.Parcela, FechaRecoleccion = Dia, EnvaseProductoId = e.Palot, PrecioEstimadoKg = 0.30m }));
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas.Single().Id}/pesadas", new { BrutoKg = 12_000m, TaraKg = 2_000m, Envases = 40, Bascula = "B1" }))
            .EnsureSuccessStatusCode();
        return (await OkAsync<RecepcionResp>(Post(e.Api, $"/agro/recepciones/{rec}/confirmar"))).Lineas.Single().PartidaId!.Value;
    }

    [Fact]
    public async Task El_calibrado_de_la_partida_pasa_a_su_clasificacion_y_da_el_reparto_por_calibres()
    {
        var e = await EscenarioAsync();
        var partida = await PartidaAsync(e);

        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/lineas", new
        {
            Codigo = "CAL1", Nombre = "Calibradora", Tipo = "Calibradora", CapacidadKgHora = 10_000m,
            Salidas = new object[] { new { Numero = 1, Calibre = "3" }, new { Numero = 1, Calibre = "4" } },
        }), HttpStatusCode.BadRequest)).Should().Be("linea_planta.salidas");
        var cal = await OkAsync<LineaPlantaResp>(e.Api.PostAsJsonAsync("/agro/planta/lineas", new
        {
            Codigo = "cal1", Nombre = "Calibradora óptica", Tipo = "Calibradora", CapacidadKgHora = 10_000m,
            Salidas = new object[]
            {
                new { Numero = 1, Calibre = "3", CategoriaId = e.Extra }, new { Numero = 2, Calibre = "4", CategoriaId = e.Primera },
                new { Numero = 3, CategoriaId = e.Destrio, Destrio = true },
            },
        }));
        cal.Codigo.Should().Be("CAL1");
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/lineas", new { Codigo = "CAL1", Nombre = "Otra", Tipo = "Confeccion", CapacidadKgHora = 1m }), HttpStatusCode.Conflict))
            .Should().Be("linea_planta.codigo_repetido");

        // El fichero de la calibradora: salida, kilos y frutos.
        var lineas = await OkAsync<List<LineaFicheroResp>>(e.Api.PostAsJsonAsync("/agro/planta/calibrados/leer-fichero",
            new { Contenido = "Salida;Kilos;Frutos\r\n1;6000,5;30000\r\n2;3000;20000\r\n3;799,5;\r\n" }));
        lineas.Should().BeEquivalentTo([new LineaFicheroResp(6000.5m, 1, 30000), new LineaFicheroResp(3000m, 2, 20000), new LineaFicheroResp(799.5m, 3, null)]);

        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/calibrados", new { LineaId = cal.Id, PartidaId = partida, Fecha = Dia, KilosEntrada = 11_000m, Lineas = lineas }),
            HttpStatusCode.BadRequest)).Should().Be("calibrado.mas_que_la_partida");
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/calibrados", new { LineaId = cal.Id, PartidaId = partida, Fecha = Dia, KilosEntrada = 9_000m, Lineas = lineas }),
            HttpStatusCode.BadRequest)).Should().Be("calibrado.mas_de_lo_entrado");
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/calibrados", new { LineaId = cal.Id, PartidaId = partida, Fecha = Dia, Lineas = new[] { new { Kilos = 1m, Salida = 9 } } }),
            HttpStatusCode.BadRequest)).Should().Be("calibrado.salida");

        var c = await OkAsync<CalibradoResp>(e.Api.PostAsJsonAsync("/agro/planta/calibrados",
            new { LineaId = cal.Id, PartidaId = partida, Fecha = Dia, KilosEntrada = 10_000m, Referencia = "VAC-0012", Lineas = lineas }));
        c.Should().Match<CalibradoResp>(x => x.Estado == "Borrador" && x.KilosSalida == 9_800m && x.Merma == 200m);
        c.Lineas.Single(l => l.Calibre == "3").Should().Match<LineaCalResp>(l => l.Categoria == "Extra" && l.GramosPieza == 200m && l.Porcentaje == 61.2m);

        var confirmado = await OkAsync<CalibradoResp>(Post(e.Api, $"/agro/planta/calibrados/{c.Id}/confirmar", new { Clasificar = true, Definitiva = true }));
        confirmado.Estado.Should().Be("Confirmado");
        var clasif = (await e.Api.GetFromJsonAsync<List<ClasificacionResp>>($"/agro/partidas/{partida}/clasificaciones"))!.Single();
        clasif.Id.Should().Be(confirmado.ClasificacionId!.Value);
        clasif.Estado.Should().Be("Definitiva");
        clasif.Lineas.Should().BeEquivalentTo([new LineaClasifResp(e.Extra, 6000.5m), new LineaClasifResp(e.Primera, 3000m), new LineaClasifResp(e.Destrio, 799.5m)]);

        (await FalloAsync(e.Api.PutAsJsonAsync($"/agro/planta/calibrados/{c.Id}", new { Fecha = Dia, Lineas = lineas }), HttpStatusCode.Conflict)).Should().Be("calibrado.no_borrador");
        (await FalloAsync(e.Api.DeleteAsync(new Uri($"/agro/planta/lineas/{cal.Id}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("linea_planta.en_uso");

        var resumen = (await e.Api.GetFromJsonAsync<ResumenResp>($"/agro/planta/calibres?desde={Dia:yyyy-MM-dd}&hasta={Dia:yyyy-MM-dd}&productoId={e.Naranja}"))!;
        resumen.Calibrados.Should().Be(1);
        resumen.Calibres.Single(x => x.Calibre == "4").Should().Be(new CalibreResp("4", 3000m, 30.6m, 150m));

        (await FalloAsync(Post(e.Api, $"/agro/planta/calibrados/{c.Id}/anular", new { Motivo = " " }), HttpStatusCode.BadRequest)).Should().Be("calibrado.motivo");
        (await OkAsync<CalibradoResp>(Post(e.Api, $"/agro/planta/calibrados/{c.Id}/anular", new { Motivo = "Fichero de otro vaciado" }))).Estado.Should().Be("Anulado");
        (await e.Api.GetFromJsonAsync<ResumenResp>($"/agro/planta/calibres?desde={Dia:yyyy-MM-dd}&hasta={Dia:yyyy-MM-dd}"))!.Calibrados.Should().Be(0, "el anulado no cuenta");
    }

    [Fact]
    public async Task Las_ordenes_cargan_la_linea_las_paradas_restan_capacidad_y_el_parte_da_lo_real()
    {
        var e = await EscenarioAsync();
        var partida = await PartidaAsync(e);
        var linea = await OkAsync<LineaPlantaResp>(e.Api.PostAsJsonAsync("/agro/planta/lineas",
            new { Codigo = "L1", Nombre = "Línea de confección 1", Tipo = "Confeccion", CapacidadKgHora = 1_000m, HorasTurno = 8m, Turnos = 2 }));
        linea.CapacidadDia.Should().Be(16_000m);
        var naranja1 = await IdAsync(e.Api, "/productos", new { Nombre = "Naranja 1ª calibre 3", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });

        var o1 = await OkAsync<OrdenResp>(e.Api.PostAsJsonAsync("/agro/planta/ordenes", new { LineaId = linea.Id, Fecha = Dia.AddDays(1), ProductoId = naranja1, Kilos = 10_000m, Turno = 1 }));
        var o2 = await OkAsync<OrdenResp>(e.Api.PostAsJsonAsync("/agro/planta/ordenes", new { LineaId = linea.Id, Fecha = Dia.AddDays(1), ProductoId = naranja1, Kilos = 4_000m, Turno = 1 }));
        await OkAsync<OrdenResp>(e.Api.PostAsJsonAsync("/agro/planta/ordenes", new { LineaId = linea.Id, Fecha = Dia.AddDays(1), ProductoId = naranja1, Kilos = 4_000m, Turno = 2 }));
        o1.HorasPrevistas.Should().Be(10m);
        (o1.Secuencia, o2.Secuencia).Should().Be((1, 2));
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/ordenes", new { LineaId = linea.Id, Fecha = Dia, ProductoId = naranja1, Kilos = 1m, Turno = 3 }), HttpStatusCode.BadRequest))
            .Should().Be("orden_linea.turno");

        // Una hora de parada en el turno 1: la capacidad baja a 15.000 kg.
        await IdAsync(e.Api, "/agro/planta/paradas", new { LineaId = linea.Id, Fecha = Dia.AddDays(1), Turno = 1, Minutos = 60m, Motivo = "Averia", Notas = "Rodillo" });
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/paradas", new { LineaId = linea.Id, Fecha = Dia.AddDays(1), Turno = 1, Minutos = 450m, Motivo = "Limpieza" }),
            HttpStatusCode.BadRequest)).Should().Be("parada.minutos");

        var carga = (await e.Api.GetFromJsonAsync<CargaResp>($"/agro/planta/carga?desde={Dia.AddDays(1):yyyy-MM-dd}&hasta={Dia.AddDays(1):yyyy-MM-dd}"))!.Dias.Single();
        carga.Should().Match<CargaDiaResp>(d => d.Capacidad == 15_000m && d.Planificado == 18_000m && d.Sobrecarga && d.Ocupacion == 120m && d.Disponibilidad == 93.8m && d.Ordenes == 3);

        // Se mueve la segunda orden al día siguiente; la primera empieza y ya no se mueve.
        await OkAsync<OrdenResp>(e.Api.PutAsJsonAsync($"/agro/planta/ordenes/{o2.Id}", new { LineaId = linea.Id, Fecha = Dia.AddDays(2), ProductoId = naranja1, Kilos = 4_000m, Turno = 1 }));
        (await OkAsync<OrdenResp>(Post(e.Api, $"/agro/planta/ordenes/{o1.Id}/iniciar"))).Estado.Should().Be("EnCurso");
        (await FalloAsync(e.Api.PutAsJsonAsync($"/agro/planta/ordenes/{o1.Id}", new { LineaId = linea.Id, Fecha = Dia.AddDays(3), ProductoId = naranja1, Kilos = 1m }),
            HttpStatusCode.Conflict)).Should().Be("orden_linea.no_planificada");

        // El parte validado cierra la orden y da lo real.
        var parte = await OkAsync<ParteResp>(e.Api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = Dia.AddDays(1), CampanaId = e.Campana, Descripcion = "L1", PorcentajeIndirectos = 0m, Reparto = "PorKilos",
            Consumos = new[] { new { PartidaId = partida, Kilos = 6_000m } }, ManoObra = Array.Empty<object>(), Maquinas = Array.Empty<object>(), Materiales = Array.Empty<object>(),
            Salidas = new[] { new { ProductoId = naranja1, Kilos = 5_800m, Factor = 1m } },
        }));
        (await FalloAsync(Post(e.Api, $"/agro/planta/ordenes/{o1.Id}/terminar", new { ParteConfeccionId = parte.Id }), HttpStatusCode.BadRequest))
            .Should().Be("orden_linea.parte_no_validado");
        (await OkAsync<ParteResp>(Post(e.Api, $"/agro/partes/{parte.Id}/validar"))).Estado.Should().Be("Validado");
        var terminada = await OkAsync<OrdenResp>(Post(e.Api, $"/agro/planta/ordenes/{o1.Id}/terminar", new { ParteConfeccionId = parte.Id }));
        terminada.Should().Match<OrdenResp>(o => o.Estado == "Terminada" && o.KilosReales == 5_800m);
        (await FalloAsync(e.Api.DeleteAsync(new Uri($"/agro/planta/ordenes/{o1.Id}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("orden_linea.no_planificada");

        carga = (await e.Api.GetFromJsonAsync<CargaResp>($"/agro/planta/carga?desde={Dia.AddDays(1):yyyy-MM-dd}&hasta={Dia.AddDays(1):yyyy-MM-dd}"))!.Dias.Single();
        carga.Should().Match<CargaDiaResp>(d => d.Planificado == 14_000m && d.Real == 5_800m && !d.Sobrecarga);

        // Reabrir la desenlaza del parte; se puede volver a cerrar.
        (await OkAsync<OrdenResp>(Post(e.Api, $"/agro/planta/ordenes/{o1.Id}/reabrir"))).KilosReales.Should().BeNull();
        await OkAsync<OrdenResp>(Post(e.Api, $"/agro/planta/ordenes/{o1.Id}/terminar", new { ParteConfeccionId = parte.Id }));
    }

    [Fact]
    public async Task Las_ordenes_se_generan_desde_el_plan_de_produccion_aprobado_sin_repetirse()
    {
        var e = await EscenarioAsync();
        var linea = await OkAsync<LineaPlantaResp>(e.Api.PostAsJsonAsync("/agro/planta/lineas",
            new { Codigo = "L2", Nombre = "Línea de mallas", Tipo = "Envasado", CapacidadKgHora = 500m }));
        var plan = await IdAsync(e.Api, "/agro/planes", new
        {
            Tipo = "Produccion", CampanaId = e.Campana, Nombre = "Producción marzo",
            Lineas = new object[]
            {
                new { Desde = Dia, Hasta = Dia.AddDays(1), ProductoId = e.Naranja, Kilos = 4_000m, LineaConfeccion = "l2", Cajas = 400 },
                new { Desde = Dia, Hasta = Dia, ProductoId = e.Naranja, Kilos = 1_000m, LineaConfeccion = "OTRA" },
            },
        });
        (await FalloAsync(e.Api.PostAsJsonAsync("/agro/planta/ordenes/desde-plan", new { Desde = Dia, Hasta = Dia.AddDays(5) }), HttpStatusCode.NotFound)).Should().Be("plan.no_encontrado");
        (await Post(e.Api, $"/agro/planes/{plan}/aprobar")).EnsureSuccessStatusCode();

        var creadas = await OkAsync<List<OrdenResp>>(e.Api.PostAsJsonAsync("/agro/planta/ordenes/desde-plan", new { Desde = Dia, Hasta = Dia.AddDays(5) }));
        creadas.Should().HaveCount(2, "la línea OTRA no es de la planta").And.OnlyContain(o => o.Kilos == 2_000m && o.HorasPrevistas == 4m);
        (await OkAsync<List<OrdenResp>>(e.Api.PostAsJsonAsync("/agro/planta/ordenes/desde-plan", new { Desde = Dia, Hasta = Dia.AddDays(5) }))).Should().BeEmpty("ya están generadas");
        (await e.Api.GetFromJsonAsync<List<OrdenResp>>($"/agro/planta/ordenes?desde={Dia:yyyy-MM-dd}&hasta={Dia.AddDays(5):yyyy-MM-dd}&lineaId={linea.Id}"))!.Should().HaveCount(2);
    }
}
