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
}
