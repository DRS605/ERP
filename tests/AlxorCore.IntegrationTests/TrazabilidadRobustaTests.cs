using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Regresiones de los fallos de trazabilidad vistos en el ERP anterior: cada prueba reproduce uno y comprueba que
/// aquí es imposible (en la aplicación y, donde toca, en la base de datos).
/// </summary>
public sealed class TrazabilidadRobustaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public TrazabilidadRobustaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, string Estado, List<LineaResp> Lineas);
    private sealed record PartidaResp(Guid Id, string Codigo, decimal Saldo, string Certificaciones);
    private sealed record SalidaResp(int NumeroLinea, Guid? PartidaId, decimal Kilos);
    private sealed record ErrorResp(string Codigo, string Mensaje);
    private sealed record ParteResp(Guid Id, string Estado, List<SalidaResp> Salidas, List<ErrorResp> Errores);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, decimal Kilos);
    private sealed record DescalificacionResp(string Quitadas, string Motivo, string? DocumentoTipo);
    private sealed record CertificadoResp(Guid Id, string Tipo, bool VigenteHoy, bool Baja);
    private sealed record PesadaResp(Guid Id, decimal BrutoKg, decimal TaraKg, decimal NetoKg, int Envases, decimal? TaraCamionKg, decimal TaraEnvasesKg);
    private sealed record EnvasePesadaResp(Guid EnvaseProductoId, int Cantidad, decimal TaraUnitariaKg, Guid? TaraEnvaseId);
    private sealed record PaleEntradaResp(Guid Id, string? SerieOrigen, int Envases, Guid? PaleId, decimal? KilosAsignados);
    private sealed record LineaCompletaResp(Guid Id, Guid? PartidaId, decimal NetoKg, decimal? KilosLiquidacion, int Pales);
    private sealed record RecepcionCompletaResp(Guid Id, string Estado, decimal NetoKg, List<LineaCompletaResp> Lineas, List<PesadaResp> Pesadas,
        List<EnvasePesadaResp> EnvasesPesadas, List<PaleEntradaResp> PalesEntrada);
    private sealed record TaraResp(Guid Id, decimal TaraKg, DateOnly Desde, DateOnly? Hasta, bool Usada);
    private sealed record LiquidacionResp(Guid Id, decimal Kilos, decimal Bruto);
    private sealed record RectificacionResp(decimal NetoAnteriorKg, decimal DiferenciaKg, decimal NetoNuevoKg, string Motivo, Guid? UsuarioId);
    private sealed record RecepcionRectificadaResp(Guid Id, string Estado, List<LineaCompletaResp> Lineas, List<RectificacionResp> Rectificaciones);
    private sealed record OrigenResp(string? Agricultor, decimal Kilos);
    private sealed record TrazaResp(List<OrigenResp> Origenes);
    private sealed record ParteMermaResp(Guid Id, string Estado, decimal PorcentajeMerma, string? MermaAprobadaPor, string? MotivoMerma, List<ErrorResp> Errores);
    private sealed record RepaletizadoResp(Guid Id, Guid DestinoPaleId, string? DestinoSscc, decimal Kilos, List<LineaRepaletizadoResp> Lineas);
    private sealed record LineaRepaletizadoResp(Guid OrigenPaleId, Guid PartidaId, decimal Kilos);

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 1, 10);

    private sealed record Escenario(HttpClient Api, Guid Empresa, Guid Campana, Guid Agricultor, Guid Parcela, Guid PimientoEco, Guid Pimiento, Guid Palot);

    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Finca La Vega", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Invernadero 1" });
        var eco = await IdAsync(api, "/productos", new { Nombre = "Pimiento verde ecológico", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var conv = await IdAsync(api, "/productos", new { Nombre = "Pimiento verde", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        return new Escenario(api, empresa, campana, agricultor, parcela, eco, conv, palot);
    }

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<ProblemaResp> ProblemaAsync(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!;
    }

    /// <summary>Recepción en borrador con una línea y una pesada (neto 10.000 kg).</summary>
    private static async Task<Guid> BorradorAsync(Escenario e, Guid producto, DateOnly fecha, string? motivo = null)
    {
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = fecha });
        var r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            new { ProductoId = producto, ParcelaId = e.Parcela, FechaRecoleccion = fecha, EnvaseProductoId = e.Palot, PrecioEstimadoKg = 0.5m, MotivoDescalificacion = motivo }));
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas.Single().Id}/pesadas", new { BrutoKg = 12_000m, TaraKg = 2_000m, Envases = 40 }))
            .EnsureSuccessStatusCode();
        return rec;
    }

    private static Task<HttpResponseMessage> ConfirmarAsync(Escenario e, Guid rec) => e.Api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null);

    private static async Task<Guid> RecibirAsync(Escenario e, Guid producto, DateOnly fecha, string? motivo = null) =>
        (await OkAsync<RecepcionResp>(await ConfirmarAsync(e, await BorradorAsync(e, producto, fecha, motivo)))).Lineas.Single().PartidaId!.Value;

    private static async Task<PartidaResp> PartidaAsync(Escenario e, Guid id) =>
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Single(p => p.Id == id);

    private static async Task<ParteResp> ParteAsync(Escenario e, DateOnly fecha, object[] consumos, object[] salidas) =>
        await OkAsync<ParteResp>(await e.Api.PostAsJsonAsync("/agro/partes", new { Fecha = fecha, CampanaId = e.Campana, Consumos = consumos, Salidas = salidas }));

    private static Task<HttpResponseMessage> ValidarAsync(Escenario e, Guid parte) => e.Api.PostAsync(new Uri($"/agro/partes/{parte}/validar", UriKind.Relative), null);

    private async Task<NpgsqlConnection> ConexionAsync(Guid empresa)
    {
        var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", c);
        await fijar.ExecuteNonQueryAsync();
        return c;
    }

    private async Task<PostgresException> RechazoAsync(Guid empresa, string sql)
    {
        await using var c = await ConexionAsync(empresa);
        await using var cmd = new NpgsqlCommand(sql, c);
        return (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
    }

    [Fact]
    public async Task Fallo6_la_genealogia_reparte_lo_consumido_entre_las_salidas_sin_duplicar_kilos()
    {
        var e = await EscenarioAsync();
        var a = await RecibirAsync(e, e.Pimiento, Dia);
        var b = await RecibirAsync(e, e.Pimiento, Dia);
        var parte = await ParteAsync(e, Dia.AddDays(1),
            [new { PartidaId = a, Kilos = 6_000m }, new { PartidaId = b, Kilos = 3_000m }],
            [new { ProductoId = e.Pimiento, Kilos = 6_000m }, new { ProductoId = e.Pimiento, Kilos = 2_000m }]);
        await OkAsync<ParteResp>(await ValidarAsync(e, parte.Id));

        await using var c = await ConexionAsync(e.Empresa);
        await using var cmd = new NpgsqlCommand($"SELECT origen_id, destino_id, kilos_origen FROM agro.genealogia WHERE parte_id = '{parte.Id}'", c);
        var aristas = new List<(Guid Origen, Guid Destino, decimal Kilos)>();
        await using (var lector = await cmd.ExecuteReaderAsync())
        {
            while (await lector.ReadAsync())
            {
                aristas.Add((lector.GetGuid(0), lector.GetGuid(1), lector.GetDecimal(2)));
            }
        }

        aristas.Should().HaveCount(4, "cada salida viene de las dos partidas");
        aristas.Where(x => x.Origen == a).Sum(x => x.Kilos).Should().Be(6_000m, "lo consumido de A se reparte, no se cuenta una vez por salida");
        aristas.Where(x => x.Origen == b).Sum(x => x.Kilos).Should().Be(3_000m);
        aristas.Sum(x => x.Kilos).Should().Be(9_000m);
        aristas.Where(x => x.Origen == a).Max(x => x.Kilos).Should().Be(4_500m, "la salida de 6.000 de 8.000 kg se lleva el 75 %");
    }

    [Fact]
    public async Task Fallo7_nada_se_consume_ni_se_expide_antes_de_existir()
    {
        var e = await EscenarioAsync();
        var partida = await RecibirAsync(e, e.Pimiento, Dia);

        // Consumir en un parte con fecha anterior a la recepción.
        var antes = await ParteAsync(e, Dia.AddDays(-1), [new { PartidaId = partida, Kilos = 1_000m }], [new { ProductoId = e.Pimiento, Kilos = 1_000m }]);
        (await ProblemaAsync(await ValidarAsync(e, antes.Id))).Codigo.Should().Be("parte.fecha_anterior");

        // Paletizar el día 15 y expedir el palé con fecha del 12: el día 12 el palé aún no tenía esos kilos.
        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/paletizar", new { PartidaId = partida, Kilos = 800m, Fecha = Dia.AddDays(5) }));
        (await e.Api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(-2), Referencia = "ALB-1" })))
            .Codigo.Should().Be("expedicion.fecha_anterior");
        var alReves = await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(2), Referencia = "ALB-1" });
        alReves.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ProblemaAsync(alReves)).Codigo.Should().Be("partida.saldo_fecha");
        (await e.Api.GetFromJsonAsync<PaleResp>($"/agro/pales/{pale.Sscc}"))!.Estado.Should().Be("Cerrado", "la expedición rechazada no deja nada a medias");
        await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(5), Referencia = "ALB-1" }));

        // En la base de datos: ni un movimiento anterior al alta de la partida, ni una salida anterior a su entrada.
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{partida}', '{Dia.AddDays(-3):yyyy-MM-dd}', 'Ajuste', -1, now())
            """)).Hint.Should().Be("partida.fecha_anterior");
        var otra = await RecibirAsync(e, e.Pimiento, Dia.AddDays(20));
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{otra}', '{Dia.AddDays(25):yyyy-MM-dd}', 'Ajuste', -10000, now()),
                   (gen_random_uuid(), '{e.Empresa}', '{otra}', '{Dia.AddDays(30):yyyy-MM-dd}', 'Ajuste', 5000, now()),
                   (gen_random_uuid(), '{e.Empresa}', '{otra}', '{Dia.AddDays(22):yyyy-MM-dd}', 'Ajuste', -4000, now())
            """)).Hint.Should().Be("partida.saldo_fecha", "el día 25 se habrían sacado 14.000 kg de 10.000 aunque el total cuadre al final");
    }

    [Fact]
    public async Task Fallo9_los_albaranes_no_repiten_numero()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Mercados Centrales SL" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja pimiento 5 kg", PrecioUnitario = 6m, Tipo = "Bien", Unidad = "ud" });
        var ids = new List<Guid>();
        foreach (var _ in new[] { 1, 2 })
        {
            ids.Add(await IdAsync(api, "/albaranes-venta", new
            {
                ClienteId = cliente, Fecha = Dia,
                Lineas = new[] { new { ProductoId = caja, Descripcion = "Caja pimiento 5 kg", Cantidad = 10m, PrecioUnitario = 6m, CodigoIva = "IVA10" } },
            }));
        }

        await using var c = await ConexionAsync(empresa);
        await using var cmd = new NpgsqlCommand(
            $"UPDATE facturacion.albaran_venta SET numero = (SELECT numero FROM facturacion.albaran_venta WHERE id = '{ids[0]}') WHERE id = '{ids[1]}'", c);
        (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.SqlState.Should().Be("23505");

        await using var indices = new NpgsqlCommand("SELECT count(*) FROM pg_indexes WHERE indexname IN ('ux_albaran_venta_numero', 'ux_albaran_compra_numero')", c);
        ((long)(await indices.ExecuteScalarAsync())!).Should().Be(2);
    }

    [Fact]
    public async Task Fallo10_lo_ecologico_no_se_vende_como_convencional_sin_decidirlo()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync($"/agro/declaraciones/{e.PimientoEco}", new { Exige = "Ecologico" })).EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync($"/agro/declaraciones/{e.Pimiento}", new { Exige = "Ninguna" })).EnsureSuccessStatusCode();

        // Sin certificado ecológico vigente, no entra como ecológico.
        var sinCert = await BorradorAsync(e, e.PimientoEco, Dia);
        (await ProblemaAsync(await ConfirmarAsync(e, sinCert))).Codigo.Should().Be("certificacion.falta");
        var cert = await IdAsync(e.Api, "/agro/certificados", new { AgricultorId = e.Agricultor, Tipo = "Ecologico", Codigo = "ES-ECO-020-AN-12345", Desde = new DateOnly(Anio, 1, 1) });
        var eco = (await OkAsync<RecepcionResp>(await ConfirmarAsync(e, sinCert))).Lineas.Single().PartidaId!.Value;
        (await PartidaAsync(e, eco)).Certificaciones.Should().Be("Ecologico");

        // Fruta ecológica con el artículo convencional: solo con motivo, y queda registrado.
        var conv = await BorradorAsync(e, e.Pimiento, Dia);
        (await ProblemaAsync(await ConfirmarAsync(e, conv))).Codigo.Should().Be("certificacion.descalificar");
        var descalificada = await RecibirAsync(e, e.Pimiento, Dia, "Calibre fuera de la norma ecológica del cliente");
        (await PartidaAsync(e, descalificada)).Certificaciones.Should().Be("Ninguna");
        (await e.Api.GetFromJsonAsync<List<DescalificacionResp>>($"/agro/partidas/{descalificada}/descalificaciones"))!.Should().ContainSingle()
            .Which.Should().Match<DescalificacionResp>(d => d.Quitadas == "Ecologico" && d.DocumentoTipo == "Recepcion");

        // Mezclar ecológico con convencional no da ecológico.
        var mezcla = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = eco, Kilos = 1_000m }, new { PartidaId = descalificada, Kilos = 1_000m }],
            [new { ProductoId = e.PimientoEco, Kilos = 1_900m }]);
        (await ProblemaAsync(await ValidarAsync(e, mezcla.Id))).Codigo.Should().Be("certificacion.falta");

        // Confeccionar convencional con fruta ecológica: solo con motivo.
        var aConv = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = eco, Kilos = 1_000m }], [new { ProductoId = e.Pimiento, Kilos = 950m }]);
        (await ProblemaAsync(await ValidarAsync(e, aConv.Id))).Codigo.Should().Be("certificacion.descalificar");
        var conMotivo = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = eco, Kilos = 1_000m }],
            [new { ProductoId = e.Pimiento, Kilos = 950m, MotivoDescalificacion = "Pedido convencional sin stock" }]);
        var validado = await OkAsync<ParteResp>(await ValidarAsync(e, conMotivo.Id));
        (await PartidaAsync(e, validado.Salidas.Single().PartidaId!.Value)).Certificaciones.Should().Be("Ninguna");

        // Ecológico con ecológico sigue siendo ecológico.
        var ecoEco = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = eco, Kilos = 1_000m }], [new { ProductoId = e.PimientoEco, Kilos = 980m }]);
        var partidaEco = (await OkAsync<ParteResp>(await ValidarAsync(e, ecoEco.Id))).Salidas.Single().PartidaId!.Value;
        (await PartidaAsync(e, partidaEco)).Certificaciones.Should().Be("Ecologico");

        // Si la partida pierde el ecológico (descalificación explícita), ya no sale como artículo ecológico.
        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/paletizar", new { PartidaId = partidaEco, Kilos = 500m, Fecha = Dia.AddDays(2) }));
        (await e.Api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/partidas/{partidaEco}/descalificar", new { Quitar = "Ecologico" }))).Codigo.Should().Be("certificacion.motivo");
        await OkAsync<List<DescalificacionResp>>(await e.Api.PostAsJsonAsync($"/agro/partidas/{partidaEco}/descalificar", new { Quitar = "Ecologico", Motivo = "Residuo detectado en análisis" }));
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(3), Referencia = "ALB-2" })))
            .Codigo.Should().Be("certificacion.falta");

        // El certificado dado de baja deja de certificar.
        (await OkAsync<CertificadoResp>(await e.Api.PostAsync(new Uri($"/agro/certificados/{cert}/baja", UriKind.Relative), null))).VigenteHoy.Should().BeFalse();
        (await ProblemaAsync(await ConfirmarAsync(e, await BorradorAsync(e, e.PimientoEco, Dia.AddDays(3))))).Codigo.Should().Be("certificacion.falta");

        // La base de datos: una partida no gana certificaciones ni las pierde sin su registro.
        (await RechazoAsync(e.Empresa, $"UPDATE agro.partida SET certificaciones = 1 WHERE id = '{descalificada}'")).Hint.Should().Be("partida.certificacion");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.partida SET certificaciones = 0 WHERE id = '{eco}'")).Hint.Should().Be("partida.descalificacion");
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.descalificacion_partida WHERE partida_id = '{descalificada}'")).Hint.Should().Be("descalificacion.inmutable");
    }

    private static async Task<RecepcionCompletaResp> RecepcionAsync(Escenario e, Guid rec) =>
        (await e.Api.GetFromJsonAsync<RecepcionCompletaResp>($"/agro/recepciones/{rec}"))!;

    private static async Task<(Guid Recepcion, Guid Linea)> BorradorSinPesadaAsync(Escenario e, Guid envase, DateOnly fecha, object? extra = null)
    {
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = fecha });
        var r = await OkAsync<RecepcionCompletaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            extra ?? new { ProductoId = e.Pimiento, ParcelaId = e.Parcela, EnvaseProductoId = envase, PrecioEstimadoKg = 0.5m }));
        return (rec, r.Lineas.Single().Id);
    }

    [Fact]
    public async Task Fallo3_la_cantidad_de_liquidacion_nunca_se_graba_como_neto()
    {
        var e = await EscenarioAsync();
        var box = await IdAsync(e.Api, "/productos", new { Nombre = "Box", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = Dia });

        // Kilos teóricos sin motivo: no.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = e.Pimiento, EnvaseProductoId = box, KilosLiquidacion = 19_760m })))
            .Codigo.Should().Be("recepcion.kilos_liquidacion");

        // 104 box × 190 kg de contrato = 19.760 kg a liquidar; la báscula dice 11.100 kg netos.
        var r = await OkAsync<RecepcionCompletaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new
        {
            ProductoId = e.Pimiento, ParcelaId = e.Parcela, EnvaseProductoId = box, KilosLiquidacion = 19_760m, MotivoKilosLiquidacion = "Contrato: 190 kg por box",
        }));
        var linea = r.Lineas.Single().Id;
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 16_100m, TaraKg = 5_000m, Envases = 104 })).EnsureSuccessStatusCode();
        var confirmada = await OkAsync<RecepcionCompletaResp>(await ConfirmarAsync(e, rec));
        confirmada.Lineas.Single().Should().Match<LineaCompletaResp>(l => l.NetoKg == 11_100m && l.KilosLiquidacion == 19_760m);
        (await PartidaAsync(e, confirmada.Lineas.Single().PartidaId!.Value)).Saldo.Should().Be(11_100m, "a la partida (y a la traza) van los kilos reales");

        // La liquidación va por los kilos de liquidación, y la base de datos lo exige.
        (await e.Api.PutAsJsonAsync($"/agro/agricultores/{e.Agricultor}", new { ProveedorId = await ProveedorAsync(e), Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) }))
            .EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Pimiento, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Pimiento, Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 1, 31), PrecioKg = 0.30m });
        var liq = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones",
            new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 1, 31), Fecha = new DateOnly(Anio, 1, 31) }));
        liq.Kilos.Should().Be(19_760m);
        liq.Bruto.Should().Be(5_928m);
        (await RechazoAsync(e.Empresa, $"UPDATE agro.linea_liquidacion SET kilos = 11100, importe = 3330 WHERE liquidacion_id = '{liq.Id}'")).Hint
            .Should().BeOneOf("liquidacion.kilos", "liquidacion.totales");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.linea_recepcion SET kilos_liquidacion = NULL WHERE id = '{linea}'")).Hint.Should().Be("recepcion.confirmada");
    }

    private async Task<Guid> ProveedorAsync(Escenario e)
    {
        await using var c = await ConexionAsync(e.Empresa);
        await using var cmd = new NpgsqlCommand($"SELECT proveedor_id FROM agro.agricultor WHERE id = '{e.Agricultor}'", c);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    [Fact]
    public async Task Fallo4_la_tara_sale_de_los_envases_contados_con_la_version_de_su_fecha()
    {
        var e = await EscenarioAsync();
        var box = await IdAsync(e.Api, "/productos", new { Nombre = "Box", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var madera = await IdAsync(e.Api, "/productos", new { Nombre = "Palé de madera", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var tara28 = await IdAsync(e.Api, "/agro/taras", new { EnvaseProductoId = box, TaraKg = 28m, Desde = new DateOnly(Anio, 1, 1), Observaciones = "Pesados 20 box vacíos" });

        // Sin tara del palé de madera no se pesa: nada de taras a ojo.
        var (rec, linea) = await BorradorSinPesadaAsync(e, box, Dia);
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new
        {
            BrutoKg = 18_000m, TaraCamionKg = 5_000m, EnvasesPorTipo = new object[] { new { EnvaseProductoId = box, Cantidad = 104 }, new { EnvaseProductoId = madera, Cantidad = 26 } },
        }))).Codigo.Should().Be("tara.falta");
        await IdAsync(e.Api, "/agro/taras", new { EnvaseProductoId = madera, TaraKg = 20m, Desde = new DateOnly(Anio, 1, 1) });

        var r = await OkAsync<RecepcionCompletaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new
        {
            BrutoKg = 18_000m, TaraCamionKg = 5_000m, EnvasesPorTipo = new object[] { new { EnvaseProductoId = box, Cantidad = 104 }, new { EnvaseProductoId = madera, Cantidad = 26 } },
        }));
        r.Pesadas.Single().Should().Match<PesadaResp>(p => p.TaraCamionKg == 5_000m && p.TaraEnvasesKg == 104 * 28m + 26 * 20m && p.NetoKg == 18_000m - 5_000m - 2_912m - 520m
            && p.Envases == 104);
        r.EnvasesPesadas.Single(x => x.EnvaseProductoId == box).TaraEnvaseId.Should().Be(tara28);
        await OkAsync<RecepcionCompletaResp>(await ConfirmarAsync(e, rec));

        // La tara real resulta ser otra: versión nueva desde el 1 de febrero. Lo ya pesado no cambia.
        await IdAsync(e.Api, "/agro/taras", new { EnvaseProductoId = box, TaraKg = 33m, Desde = new DateOnly(Anio, 2, 1) });
        var taras = (await e.Api.GetFromJsonAsync<List<TaraResp>>($"/agro/taras?envaseProductoId={box}"))!;
        taras.Single(t => t.Id == tara28).Should().Match<TaraResp>(t => t.Hasta == new DateOnly(Anio, 1, 31) && t.Usada);
        (await RecepcionAsync(e, rec)).Pesadas.Single().TaraEnvasesKg.Should().Be(3_432m, "la pesada guarda la tara con que se hizo");
        (await ProblemaAsync(await e.Api.PutAsJsonAsync($"/agro/taras/{tara28}", new { EnvaseProductoId = box, TaraKg = 30m, Desde = new DateOnly(Anio, 1, 1) })))
            .Codigo.Should().Be("tara.aplicada");
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/taras", new { EnvaseProductoId = box, TaraKg = 30m, Desde = new DateOnly(Anio, 1, 15) })))
            .Codigo.Should().Be("tara.solapada");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.tara_envase SET tara_kg = 33 WHERE id = '{tara28}'")).Hint.Should().Be("tara.aplicada");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.pesada SET tara_kg = tara_kg + 1 WHERE recepcion_id = '{rec}'")).Hint.Should().Be("recepcion.confirmada");

        // En febrero se aplica la nueva.
        var (feb, lineaFeb) = await BorradorSinPesadaAsync(e, box, new DateOnly(Anio, 2, 5));
        var rf = await OkAsync<RecepcionCompletaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{feb}/lineas/{lineaFeb}/pesadas", new
        {
            BrutoKg = 18_000m, TaraCamionKg = 5_000m, EnvasesPorTipo = new object[] { new { EnvaseProductoId = box, Cantidad = 104 } },
        }));
        rf.Pesadas.Single().NetoKg.Should().Be(18_000m - 5_000m - 104 * 33m);
    }

    [Fact]
    public async Task Fallo5_los_pales_son_los_contados_y_llevan_exactamente_el_neto()
    {
        var e = await EscenarioAsync();
        var box = await IdAsync(e.Api, "/productos", new { Nombre = "Box", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var (rec, linea) = await BorradorSinPesadaAsync(e, box, Dia);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 16_100m, TaraKg = 5_000m, Envases = 104 })).EnsureSuccessStatusCode();

        // 25 palés de 4 box = 100 box, y la pesada contó 104: no cuadra.
        for (var i = 1; i <= 25; i++)
        {
            (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { SerieOrigen = $"FV-{i:D4}", EnvaseProductoId = box, Envases = 4 })).EnsureSuccessStatusCode();
        }

        (await ProblemaAsync(await ConfirmarAsync(e, rec))).Codigo.Should().Be("recepcion.pales_envases");
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { SerieOrigen = "FV-0001", EnvaseProductoId = box, Envases = 4 })))
            .Codigo.Should().Be("pale_entrada.repetido");
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { SerieOrigen = "FV-0026", EnvaseProductoId = box, Envases = 4 })).EnsureSuccessStatusCode();

        // 26 palés reales (no los 34 que saldrían de «3 box por palé»), cada uno con su SSCC y su parte del neto.
        var r = await OkAsync<RecepcionCompletaResp>(await ConfirmarAsync(e, rec));
        r.Lineas.Single().Pales.Should().Be(26);
        r.PalesEntrada.Should().HaveCount(26).And.OnlyContain(p => p.PaleId != null);
        r.PalesEntrada.Sum(p => p.KilosAsignados!.Value).Should().Be(11_100m);
        var partida = r.Lineas.Single().PartidaId!.Value;
        var pale = r.PalesEntrada.First(p => p.SerieOrigen == "FV-0001");
        pale.KilosAsignados.Should().Be(426.923m);
        var pales = (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!.Where(p => r.PalesEntrada.Any(x => x.PaleId == p.Id)).ToList();
        pales.Should().HaveCount(26).And.OnlyContain(p => p.Estado == "Cerrado" && p.Kilos > 0);
        pales.Select(p => p.Sscc).Distinct().Should().HaveCount(26);

        // El palé de entrada se consume en confección sin abrirlo, y sus kilos cuadran con la partida.
        var parte = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = partida, PaleId = pale.PaleId, Kilos = pale.KilosAsignados }],
            [new { ProductoId = e.Pimiento, Kilos = 400m }]);
        await OkAsync<ParteResp>(await ValidarAsync(e, parte.Id));
        (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!.Single(p => p.Id == pale.PaleId).Kilos.Should().Be(0m);

        // La base de datos no deja meter más kilos en un palé de entrada ni en uno de otra recepción.
        var otra = await RecibirAsync(e, e.Pimiento, Dia);
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, pale_id, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{otra}', '{Dia:yyyy-MM-dd}', 'Entrada', 10, '{r.PalesEntrada[1].PaleId}', now())
            """)).Hint.Should().Be("pale.estado");
    }

    [Fact]
    public async Task Una_recepcion_con_pales_de_entrada_se_anula_si_no_se_ha_usado()
    {
        var e = await EscenarioAsync();
        var box = await IdAsync(e.Api, "/productos", new { Nombre = "Box", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var (rec, linea) = await BorradorSinPesadaAsync(e, box, Dia);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 3_000m, TaraKg = 1_000m, Envases = 8 })).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { EnvaseProductoId = box, Envases = 4, KilosNetos = 1_200m })).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { EnvaseProductoId = box, Envases = 4, KilosNetos = 700m })).EnsureSuccessStatusCode();
        (await ProblemaAsync(await ConfirmarAsync(e, rec))).Codigo.Should().Be("recepcion.pales_kilos", "pesados uno a uno suman 1.900 y el neto es 2.000");
        var r = await RecepcionAsync(e, rec);
        (await e.Api.DeleteAsync($"/agro/recepciones/{rec}/pales/{r.PalesEntrada[1].Id}")).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { EnvaseProductoId = box, Envases = 4, KilosNetos = 800m })).EnsureSuccessStatusCode();
        var confirmada = await OkAsync<RecepcionCompletaResp>(await ConfirmarAsync(e, rec));
        confirmada.PalesEntrada.Select(p => p.KilosAsignados).Should().BeEquivalentTo([1_200m, 800m]);

        var anulada = await OkAsync<RecepcionCompletaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/anular", new { Motivo = "Recepción duplicada" }));
        anulada.Estado.Should().Be("Anulada");
        (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!.Where(p => confirmada.PalesEntrada.Any(x => x.PaleId == p.Id)).Should().OnlyContain(p => p.Kilos == 0m);
    }

    [Fact]
    public async Task Fallo1_una_recepcion_ya_vendida_se_rectifica_sobre_la_misma_partida_sin_recrearla()
    {
        var e = await EscenarioAsync();
        var rec = await BorradorAsync(e, e.Pimiento, Dia);
        var r = await OkAsync<RecepcionCompletaResp>(await ConfirmarAsync(e, rec));
        var linea = r.Lineas.Single().Id;
        var partida = r.Lineas.Single().PartidaId!.Value;

        // Se vende parte: palé de 3.000 kg expedido.
        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/paletizar", new { PartidaId = partida, Kilos = 3_000m, Fecha = Dia.AddDays(1) }));
        (await e.Api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(2), Referencia = "ALB-9" }));

        // Rehacerla es imposible: no se anula (ya se usó) ni se tocan sus líneas en la base de datos.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/anular", new { Motivo = "Rehacer" }))).Codigo.Should().Be("recepcion.partidas_usadas");
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.linea_recepcion WHERE id = '{linea}'")).Hint.Should().Be("recepcion.confirmada");
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.partida WHERE id = '{partida}'")).Hint.Should().Be("partida.inmutable");

        // La báscula pesó de menos: se rectifica el neto (10.000 → 10.500) sobre la misma partida, con motivo.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/rectificar", new { NetoKg = 10_500m }))).Codigo.Should().Be("rectificacion.motivo");
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/rectificar",
            new { NetoKg = 10_500m, BrutoKg = 12_000m, TaraKg = 2_000m, Motivo = "Báscula descalibrada" }))).Codigo.Should().Be("rectificacion.pesada");
        var rect = await OkAsync<RecepcionRectificadaResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/rectificar",
            new { NetoKg = 10_500m, BrutoKg = 12_500m, TaraKg = 2_000m, Motivo = "Báscula descalibrada: repesado el 12/01", Fecha = Dia.AddDays(2) }));
        rect.Estado.Should().Be("Confirmada");
        rect.Lineas.Single().PartidaId.Should().Be(partida, "la partida (y lo ya vendido) no cambia");
        rect.Rectificaciones.Single().Should().Match<RectificacionResp>(x => x.NetoAnteriorKg == 10_000m && x.DiferenciaKg == 500m && x.NetoNuevoKg == 10_500m
            && x.UsuarioId != null);
        (await PartidaAsync(e, partida)).Saldo.Should().Be(7_500m);

        // La traza del palé vendido sigue llegando a la finca.
        (await e.Api.GetFromJsonAsync<TrazaResp>($"/agro/trazabilidad/atras?sscc={pale.Sscc}"))!.Origenes.Should().ContainSingle(o => o.Agricultor == "Finca La Vega");

        // No se quita más de lo que queda: lo vendido no se «desvende».
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/rectificar", new { NetoKg = 2_000m, Motivo = "Error" })))
            .Codigo.Should().Be("rectificacion.sin_saldo");

        // La liquidación va por el neto rectificado; con la entrega liquidada ya no se rectifica sin anular antes la liquidación.
        (await e.Api.PutAsJsonAsync($"/agro/agricultores/{e.Agricultor}", new { ProveedorId = await ProveedorAsync(e), Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) }))
            .EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Pimiento, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Pimiento, Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 1, 31), PrecioKg = 0.30m });
        var liq = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones",
            new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 1, 31), Fecha = new DateOnly(Anio, 1, 31) }));
        liq.Kilos.Should().Be(10_500m);
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/rectificar", new { NetoKg = 10_400m, Motivo = "Otra" })))
            .Codigo.Should().Be("rectificacion.liquidada");

        // En la base de datos: la rectificación no se borra, y no hay movimientos de rectificación sin su documento.
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.rectificacion_recepcion WHERE linea_recepcion_id = '{linea}'")).Hint.Should().Be("rectificacion.inmutable");
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, documento_tipo, documento_id, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{partida}', '{Dia.AddDays(3):yyyy-MM-dd}', 'Rectificacion', 100, 'Rectificacion', gen_random_uuid(), now())
            """)).Hint.Should().Be("rectificacion.sin_documento");
    }

    [Fact]
    public async Task La_merma_por_encima_de_la_tolerancia_necesita_aprobacion_y_las_transformaciones_se_limitan()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync("/agro/configuracion", new { PrefijoGs1 = "8400000", DigitoExtension = 0, ToleranciaMermaPct = 10m })).EnsureSuccessStatusCode();
        var partida = await RecibirAsync(e, e.Pimiento, Dia);

        // 15 % de merma: no se valida sin aprobar.
        var parte = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = partida, Kilos = 1_000m }], [new { ProductoId = e.Pimiento, Kilos = 850m }]);
        (await ProblemaAsync(await ValidarAsync(e, parte.Id))).Codigo.Should().Be("parte.merma_excesiva");
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/partes/{parte.Id}/aprobar-merma", new { }))).Codigo.Should().Be("parte.motivo_merma");
        var aprobado = await OkAsync<ParteMermaResp>(await e.Api.PostAsJsonAsync($"/agro/partes/{parte.Id}/aprobar-merma", new { Motivo = "Podrido por la lluvia" }));
        aprobado.Should().Match<ParteMermaResp>(p => p.PorcentajeMerma == 15m && p.MotivoMerma == "Podrido por la lluvia" && p.MermaAprobadaPor != null);
        await OkAsync<ParteResp>(await ValidarAsync(e, parte.Id));

        // Con reglas: del pimiento solo sale pimiento, y con un 5 % de merma como mucho (más estricto que el 10 % general).
        await IdAsync(e.Api, "/agro/transformaciones", new { ProductoOrigenId = e.Pimiento, ProductoDestinoId = e.Pimiento, MermaMaximaPct = 5m });
        var otro = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = partida, Kilos = 1_000m }], [new { ProductoId = e.PimientoEco, Kilos = 990m }]);
        (await ProblemaAsync(await ValidarAsync(e, otro.Id))).Codigo.Should().Be("parte.transformacion_no_permitida");
        var ocho = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = partida, Kilos = 1_000m }], [new { ProductoId = e.Pimiento, Kilos = 920m }]);
        (await ProblemaAsync(await ValidarAsync(e, ocho.Id))).Codigo.Should().Be("parte.merma_excesiva");
        var bien = await ParteAsync(e, Dia.AddDays(1), [new { PartidaId = partida, Kilos = 1_000m }], [new { ProductoId = e.Pimiento, Kilos = 960m }]);
        await OkAsync<ParteResp>(await ValidarAsync(e, bien.Id));
    }

    [Fact]
    public async Task El_repaletizado_pasa_kilos_entre_pales_en_una_operacion_registrada_que_conserva_la_traza()
    {
        var e = await EscenarioAsync();
        var partida = await RecibirAsync(e, e.Pimiento, Dia);
        var a = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        var b = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{a.Id}/paletizar", new { PartidaId = partida, Kilos = 600m, Fecha = Dia }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{b.Id}/paletizar", new { PartidaId = partida, Kilos = 400m, Fecha = Dia }));
        (await e.Api.PostAsync(new Uri($"/agro/pales/{a.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();

        // A (cerrado) entero y 150 kg de B, a un palé nuevo que se cierra.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/repaletizados", new { Lineas = new object[] { new { OrigenPaleId = b.Id, Kilos = 150m } } })))
            .Codigo.Should().Be("repaletizado.kilos");
        var rep = await OkAsync<RepaletizadoResp>(await e.Api.PostAsJsonAsync("/agro/repaletizados", new
        {
            Fecha = Dia.AddDays(1), Motivo = "Palés rotos", CerrarDestino = true,
            Lineas = new object[] { new { OrigenPaleId = a.Id }, new { OrigenPaleId = b.Id, PartidaId = partida, Kilos = 150m } },
        }));
        rep.Kilos.Should().Be(750m);
        rep.Lineas.Should().HaveCount(2);
        var pales = (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!;
        pales.Single(p => p.Id == a.Id).Kilos.Should().Be(0m);
        pales.Single(p => p.Id == b.Id).Kilos.Should().Be(250m);
        pales.Single(p => p.Id == rep.DestinoPaleId).Should().Match<PaleResp>(p => p.Kilos == 750m && p.Estado == "Cerrado");
        (await e.Api.GetFromJsonAsync<List<RepaletizadoResp>>($"/agro/repaletizados?paleId={a.Id}"))!.Should().ContainSingle(x => x.Id == rep.Id);
        (await e.Api.GetFromJsonAsync<TrazaResp>($"/agro/trazabilidad/atras?sscc={rep.DestinoSscc}"))!.Origenes.Should().ContainSingle(o => o.Agricultor == "Finca La Vega");

        // Ni se modifica ni se hacen movimientos de repaletizado sin su documento.
        (await RechazoAsync(e.Empresa, $"UPDATE agro.linea_repaletizado SET kilos = 1 WHERE repaletizado_id = '{rep.Id}'")).Hint.Should().Be("repaletizado.inmutable");
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, pale_id, documento_tipo, documento_id, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{partida}', '{Dia.AddDays(2):yyyy-MM-dd}', 'Paletizado', -10, '{b.Id}', 'Repaletizado', gen_random_uuid(), now())
            """)).Hint.Should().Be("repaletizado.sin_documento");
    }
}
