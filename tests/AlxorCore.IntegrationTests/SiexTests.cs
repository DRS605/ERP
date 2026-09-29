using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Cuaderno digital de explotación (SIEX): datos de la explotación y de los recintos, labores completas, análisis, plan de abonado, validación y exportación.</summary>
public sealed class SiexTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SiexTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly int Anio = DateTime.UtcNow.Year - 1;
    private static readonly DateOnly Dia = new(Anio, 4, 10);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record IncidenciaResp(string Nivel, string Ambito, Guid? Id, string Codigo);
    private sealed record ValidacionResp(int Errores, int Avisos, bool Completo, List<IncidenciaResp> Incidencias);
    private sealed record TratamientoResp(Guid Id, string? CarneAplicador, string? EquipoRoma, string? Asesor, string? Eficacia, string? TipoFertilizante);
    private sealed record BalanceResp(Guid ParcelaId, Guid? PlanId, decimal? PlanN, decimal AportadoN, decimal AportadoP, decimal? DiferenciaN, int Abonados);
    private sealed record SigpacResp(int Provincia, int Municipio, int Agregado, int Zona, int Poligono, int Parcela, int Recinto);
    private sealed record ParcelaSiexResp(string Codigo, SigpacResp? Sigpac, string? Cultivo, string? Sistema, string Produccion);
    private sealed record LaborResp(string Tipo, string? CarneAplicador, string? TipoFertilizante);
    private sealed record CosechaResp(string Parcela, decimal Kilos);
    private sealed record ExplotacionResp(string Titular, string? Nif, string? CodigoRegepa);
    private sealed record CuadernoResp(string Formato, int Anio, ExplotacionResp Explotacion, List<ParcelaSiexResp> Parcelas, List<LaborResp> TratamientosFitosanitarios,
        List<LaborResp> Fertilizaciones, List<LaborResp> Riegos, List<CosechaResp> Cosechas, List<BalanceResp> PlanesAbonado, List<IncidenciaResp> Incidencias);
    private sealed record LineaResp(Guid Id);
    private sealed record RecepcionResp(List<LineaResp> Lineas);

    private async Task<(HttpClient Api, Guid Empresa, Guid Agricultor, Guid Parcela, Guid Naranja)> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Rosa Llauradora", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Hort del Riu" });
        return (api, empresa, agricultor, parcela, naranja);
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

    [Fact]
    public async Task La_validacion_dice_lo_que_falta_y_el_cuaderno_queda_completo_al_rellenarlo()
    {
        var (api, _, agricultor, parcela, naranja) = await EscenarioAsync();

        // Tratamiento a medias: sin registro, plaga, dosis, superficie ni aplicador.
        await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new { ParcelaId = parcela, Fecha = Dia, Producto = "Aceite de parafina", PlazoSeguridadDias = 1 }));
        await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new { ParcelaId = parcela, Fecha = Dia, Producto = "Nitrato potásico", PlazoSeguridadDias = 0, Tipo = "Abonado" }));
        await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new { ParcelaId = parcela, Fecha = Dia, Producto = "Pozo", PlazoSeguridadDias = 0, Tipo = "Riego" }));

        var v = await OkAsync<ValidacionResp>(api.GetAsync($"/agro/siex/validacion?agricultorId={agricultor}&anio={Anio}"));
        v.Completo.Should().BeFalse();
        v.Incidencias.Where(i => i.Nivel == "Error").Select(i => i.Codigo).Should().Contain(
        [
            "siex.regepa", "siex.sigpac", "siex.superficie", "siex.cultivo", "siex.registro", "siex.plaga", "siex.dosis", "siex.superficie_tratada", "siex.aplicador",
            "siex.fertilizante", "siex.cantidad", "siex.volumen",
        ]);
        v.Incidencias.Where(i => i.Nivel == "Aviso").Select(i => i.Codigo).Should().Contain(["siex.sistema", "siex.roma", "siex.asesor", "siex.eficacia", "siex.metodo"]);

        // Las labores del cuaderno son un registro: se anulan, no se cambian.
        var todas = await api.GetFromJsonAsync<List<IdResp>>($"/agro/tratamientos?agricultorId={agricultor}");
        foreach (var t in todas!)
        {
            (await api.PostAsJsonAsync($"/agro/tratamientos/{t.Id}/anular", new { Motivo = "Faltaban datos" })).EnsureSuccessStatusCode();
        }

        // Explotación con REGEPA, asesor, aplicador y equipo habituales; recinto completo.
        (await api.PutAsJsonAsync($"/agro/siex/explotaciones/{agricultor}", new
        {
            CodigoRegepa = "es460001234", AsesorNombre = "Laura Asesora", AsesorRopo = "ROPO-46-0099", CarneAplicador = "ROPO-46-1234", EquipoRoma = "ROMA-46-555",
            PlanAbonadoObligatorio = true,
        })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync($"/agro/parcelas/{parcela}", new
        {
            Nombre = "Hort del Riu", ReferenciaSigpac = "46:250:0:0:12:45:1", SuperficieHa = 2.5m, ProductoId = naranja, Variedad = "Navelina", Sistema = "Regadio",
            Modo = "AireLibre", Produccion = "Integrada",
        })).EnsureSuccessStatusCode();

        var completo = await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new
        {
            ParcelaId = parcela, Fecha = Dia, Producto = "Aceite de parafina", PlazoSeguridadDias = 1, NumeroRegistro = "ES-00123", Motivo = "Piojo rojo", Dosis = 1.5m,
            UnidadDosis = "l/hl", SuperficieTratadaHa = 2.5m, Eficacia = "Buena",
        }));
        completo.Should().Match<TratamientoResp>(t => t.CarneAplicador == "ROPO-46-1234" && t.EquipoRoma == "ROMA-46-555" && t.Asesor == "Laura Asesora (ROPO-46-0099)",
            "sin los suyos, la labor toma los habituales de la explotación");
        var abonado = await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new
        {
            ParcelaId = parcela, Fecha = Dia, Producto = "Nitrato potásico", PlazoSeguridadDias = 0, Tipo = "Abonado", TipoFertilizante = "Mineral", Dosis = 300m,
            UnidadDosis = "kg/ha", NitrogenoKgHa = 130m, PotasioKgHa = 40m, MetodoAplicacion = "Fertirrigación", CarneAplicador = "NO-VA",
        }));
        abonado.Should().Match<TratamientoResp>(t => t.TipoFertilizante == "Mineral" && t.CarneAplicador == null, "el carné de aplicador es de los fitosanitarios");
        await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new
        {
            ParcelaId = parcela, Fecha = Dia, Producto = "Pozo", PlazoSeguridadDias = 0, Tipo = "Riego", VolumenM3 = 800m, MetodoAplicacion = "Goteo",
        }));

        // Obligada al plan de abonado: sin él, error; con uno de 100 kg N/ha y 130 aportados, aviso de exceso.
        (await OkAsync<ValidacionResp>(api.GetAsync($"/agro/siex/validacion?agricultorId={agricultor}&anio={Anio}"))).Incidencias
            .Should().ContainSingle(i => i.Nivel == "Error").Which.Codigo.Should().Be("siex.plan_abonado");
        var balance = await OkAsync<BalanceResp>(api.PutAsJsonAsync("/agro/siex/planes-abonado",
            new { ParcelaId = parcela, Anio, NitrogenoKgHa = 100m, FosforoKgHa = 40m, PotasioKgHa = 60m, ProduccionEsperadaKgHa = 40_000m }));
        balance.Should().Match<BalanceResp>(b => b.PlanN == 100m && b.AportadoN == 130m && b.DiferenciaN == 30m && b.Abonados == 1);
        var otraVez = await OkAsync<BalanceResp>(api.PutAsJsonAsync("/agro/siex/planes-abonado",
            new { ParcelaId = parcela, Anio, NitrogenoKgHa = 125m, FosforoKgHa = 40m, PotasioKgHa = 60m }));
        otraVez.PlanId.Should().Be(balance.PlanId, "hay un plan por parcela y año: se cambia");

        // Análisis de residuos por encima del límite: aviso.
        await IdAsync(api, "/agro/siex/analisis", new { ParcelaId = parcela, Analisis = new { Fecha = Dia, Tipo = "Residuos", Laboratorio = "Lab Agro", SuperaLimites = true } });
        (await (await api.PostAsJsonAsync("/agro/siex/analisis", new { ParcelaId = parcela, Analisis = new { Fecha = Dia, Tipo = "Suelo", Laboratorio = "Lab", Ph = 20m } }))
            .Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("analisis.valores");

        var fin = await OkAsync<ValidacionResp>(api.GetAsync($"/agro/siex/validacion?agricultorId={agricultor}&anio={Anio}"));
        fin.Completo.Should().BeTrue(string.Join(", ", fin.Incidencias.Where(i => i.Nivel == "Error").Select(i => i.Codigo)));
        fin.Incidencias.Select(i => i.Codigo).Should().Contain("siex.residuos").And.NotContain("siex.exceso_nitrogeno", "130 no pasa del 110 % de 125");

        // Cosecha: la entrega de la parcela.
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = Dia.AddDays(5) });
        var linea = (await OkAsync<RecepcionResp>(api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = naranja, ParcelaId = parcela, FechaRecoleccion = Dia.AddDays(5) })))
            .Lineas.Single().Id;
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 3000m, TaraKg = 500m, Bascula = "B1" })).EnsureSuccessStatusCode();
        (await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();

        var c = await OkAsync<CuadernoResp>(api.GetAsync($"/agro/siex/cuaderno?agricultorId={agricultor}&anio={Anio}"));
        c.Formato.Should().Be("ALXOR-CUE");
        c.Explotacion.CodigoRegepa.Should().Be("ES460001234");
        c.Parcelas.Single().Should().Match<ParcelaSiexResp>(p => p.Sigpac!.Provincia == 46 && p.Sigpac.Municipio == 250 && p.Sigpac.Recinto == 1 && p.Cultivo == "Naranja Navel"
            && p.Sistema == "Regadio" && p.Produccion == "Integrada");
        c.TratamientosFitosanitarios.Should().ContainSingle().Which.CarneAplicador.Should().Be("ROPO-46-1234");
        c.Fertilizaciones.Should().ContainSingle().Which.TipoFertilizante.Should().Be("Mineral");
        c.Riegos.Should().ContainSingle();
        c.Cosechas.Should().ContainSingle().Which.Kilos.Should().Be(2500m);
        c.PlanesAbonado.Single().PlanN.Should().Be(125m);

        var fichero = await api.GetAsync($"/agro/siex/cuaderno?agricultorId={agricultor}&anio={Anio}&descargar=true");
        fichero.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        fichero.Content.Headers.ContentDisposition!.FileName.Should().Contain($"cuaderno-digital-").And.Contain($"{Anio}");
        (await fichero.Content.ReadAsStringAsync()).Should().Contain("\"tratamientosFitosanitarios\"").And.Contain("\"Regadio\"");
    }

    [Fact]
    public async Task La_eficacia_se_anota_despues_y_la_base_de_datos_no_deja_cambiar_ni_borrar_una_labor()
    {
        var (api, empresa, _, parcela, _) = await EscenarioAsync();
        var t = await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new { ParcelaId = parcela, Fecha = Dia, Producto = "Cobre", PlazoSeguridadDias = 7 }));
        (await OkAsync<TratamientoResp>(api.PostAsJsonAsync($"/agro/tratamientos/{t.Id}/eficacia", new { Eficacia = "Regular" }))).Eficacia.Should().Be("Regular");
        var riego = await OkAsync<TratamientoResp>(api.PostAsJsonAsync("/agro/tratamientos", new { ParcelaId = parcela, Fecha = Dia, Producto = "Pozo", PlazoSeguridadDias = 0, Tipo = "Riego", VolumenM3 = 10m }));
        (await (await api.PostAsJsonAsync($"/agro/tratamientos/{riego.Id}/eficacia", new { Eficacia = "Buena" })).Content.ReadFromJsonAsync<ProblemaResp>())!
            .Codigo.Should().Be("tratamiento.eficacia");

        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        foreach (var sql in new[]
        {
            $"UPDATE agro.tratamiento_parcela SET producto = 'Otro' WHERE id = '{t.Id}'",
            $"DELETE FROM agro.tratamiento_parcela WHERE id = '{t.Id}'",
            $"UPDATE agro.tratamiento_parcela SET tipo_fertilizante = 'Mineral' WHERE id = '{riego.Id}'",
        })
        {
            await using var cmd = new NpgsqlCommand(sql, c);
            var ex = await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
            ex.Which.Hint.Should().Be("tratamiento.inmutable", sql);
        }

        (await api.PostAsJsonAsync($"/agro/tratamientos/{t.Id}/anular", new { Motivo = "Error" })).EnsureSuccessStatusCode();
        await using var reabrir = new NpgsqlCommand($"UPDATE agro.tratamiento_parcela SET anulado = false WHERE id = '{t.Id}'", c);
        (await FluentActions.Awaiting(() => reabrir.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("tratamiento.inmutable");
    }
}
