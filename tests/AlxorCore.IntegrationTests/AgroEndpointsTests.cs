using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlxorCore.Persistencia;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Módulo agro: recepción, clasificación, liquidación con autofactura, confección, palés, expedición y trazabilidad.</summary>
public sealed class AgroEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public AgroEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo, string? Detail);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, int NumeroLinea, decimal NetoKg, int Envases, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, string? Numero, string Estado, decimal NetoKg, List<LineaResp> Lineas);
    private sealed record PartidaResp(Guid Id, string Codigo, decimal Saldo, decimal KilosIniciales, decimal? CosteKg, bool Anulada);
    private sealed record SaldoEnvaseResp(Guid EnvaseProductoId, int Saldo);
    private sealed record EnvasesResp(List<SaldoEnvaseResp> Saldos);
    private sealed record LineaLiqResp(Guid? CategoriaId, decimal Kilos, decimal PrecioKg, decimal Importe);
    private sealed record LiquidacionResp(Guid Id, string? Numero, string Estado, decimal Kilos, decimal Bruto, decimal TotalDescuentos, decimal BaseImponible,
        decimal CuotaImpuesto, decimal Retencion, decimal TotalFactura, decimal APagar, Guid? GastoId, string CodigoImpuesto, List<LineaLiqResp> Lineas);
    private sealed record ErrorResp(string Codigo, string Mensaje);
    private sealed record PrevisualizacionResp(bool Valida, List<ErrorResp> Errores, LiquidacionResp? Liquidacion);
    private sealed record GastoResp(Guid Id, string CodigoIva, decimal BaseImponible, decimal CuotaIva, decimal RetencionIrpf, decimal Total, string Estado);
    private sealed record SalidaResp(int NumeroLinea, decimal Kilos, Guid? PartidaId, decimal Coste, decimal CosteKg);
    private sealed record ParteResp(Guid Id, string? Numero, string Estado, decimal CosteTotal, decimal CosteManoObra, decimal Merma, List<SalidaResp> Salidas, List<ErrorResp> Errores);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, decimal Kilos);
    private sealed record OrigenResp(string? Agricultor, string? Parcela, string? ReferenciaSigpac, decimal Kilos);
    private sealed record DestinoResp(string Sscc, string Estado, Guid? ClienteId, decimal Kilos);
    private sealed record NodoResp(string Codigo, int Nivel);
    private sealed record TrazaResp(List<NodoResp> Partidas, List<OrigenResp> Origenes, List<DestinoResp> Destinos);
    private sealed record EntregasResp(string Agricultor, decimal KilosEntregados, decimal KilosLiquidados, decimal APagar);
    private sealed record ParcelaCosteResp(string Codigo, decimal KilosRecibidos, decimal? KilosPorHa, decimal? CosteCultivo, decimal? CosteCultivoPorKilo);
    private sealed record ConfeccionResp(Guid ProductoId, decimal KilosObtenidos, decimal CosteTotal, decimal CosteKg);
    private sealed record InformeResp(decimal KilosRecibidos, decimal KilosLiquidados, List<EntregasResp> Agricultores, List<ParcelaCosteResp> Parcelas, List<ConfeccionResp> Confeccion, bool AnaliticaDisponible);
    private sealed record Modelo303Resp(decimal CompensacionesReagpBase, decimal CompensacionesReagpCuota);
    private sealed record ResumenResp(Modelo303Resp Modelo303);

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 3, 10);

    /// <summary>Datos de partida: empresa con el módulo agro, campaña, agricultor REAGP con parcela, productos y categorías.</summary>
    private sealed record Escenario(HttpClient Api, Guid Empresa, Guid Campana, Guid Agricultor, Guid ProveedorId, Guid Parcela, Guid Naranja, Guid Palot,
        Guid Extra, Guid Primera, Guid Destrio);

    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Huerto del Río", ReferenciaSigpac = "46:250:0:0:12:45:1", SuperficieHa = 2.5m });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var extra = await IdAsync(api, "/agro/categorias", new { Codigo = "EXTRA", Nombre = "Extra", Orden = 1 });
        var primera = await IdAsync(api, "/agro/categorias", new { Codigo = "1A", Nombre = "Primera", Orden = 2 });
        var destrio = await IdAsync(api, "/agro/categorias", new { Codigo = "DESTRIO", Nombre = "Destrío", EsDestrio = true, Orden = 3 });
        return new Escenario(api, empresa, campana, agricultor, proveedor, parcela, naranja, palot, extra, primera, destrio);
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

    private static async Task<ProblemaResp> ProblemaAsync(HttpResponseMessage r, HttpStatusCode esperado)
    {
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!;
    }

    /// <summary>Recepción confirmada de naranja: dos pesadas (neto 10.000 kg) en 40 palots.</summary>
    private static async Task<RecepcionResp> RecibirAsync(Escenario e, DateOnly fecha, decimal bruto = 12_000m, decimal tara = 2_000m, decimal? precioEstimado = 0.30m)
    {
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = fecha, Matricula = "1234BCD" });
        var r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            new { ProductoId = e.Naranja, ParcelaId = e.Parcela, FechaRecoleccion = fecha, EnvaseProductoId = e.Palot, PrecioEstimadoKg = precioEstimado }));
        var linea = r.Lineas.Single().Id;
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = bruto / 2, TaraKg = tara / 2, Envases = 20, Bascula = "B1" })).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = bruto / 2, TaraKg = tara / 2, Envases = 20, Bascula = "B1" })).EnsureSuccessStatusCode();
        return await OkAsync<RecepcionResp>(await e.Api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null));
    }

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
        var ex = await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>();
        return ex.Which;
    }

    [Fact]
    public async Task El_modulo_agro_se_contrata_aparte()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var sinAgro = await api.GetAsync("/agro/campanas");
        (await ProblemaAsync(sinAgro, HttpStatusCode.Forbidden)).Codigo.Should().Be("modulo.no_contratado");

        var e = await EscenarioAsync();
        (await e.Api.GetAsync("/agro/campanas")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task La_recepcion_crea_partidas_devuelve_envases_y_queda_protegida()
    {
        var e = await EscenarioAsync();

        // Un artículo que no se mide en kilos no se recibe como fruta.
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = Dia });
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = e.Palot }), HttpStatusCode.BadRequest))
            .Codigo.Should().Be("recepcion.unidad");

        // Confirmar sin pesadas: todos los problemas en una sola respuesta.
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = e.Naranja })).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = e.Naranja })).EnsureSuccessStatusCode();
        var sinPesadas = await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null), HttpStatusCode.BadRequest);
        sinPesadas.Codigo.Should().Be("recepcion.sin_pesadas");
        sinPesadas.Title.Should().Contain("línea 1").And.Contain("línea 2");
        (await e.Api.DeleteAsync($"/agro/recepciones/{rec}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var r = await RecibirAsync(e, Dia);
        r.Estado.Should().Be("Confirmada");
        r.Numero.Should().Be($"REC-{Anio}-000001");
        r.NetoKg.Should().Be(10_000m);
        var partidaId = r.Lineas.Single().PartidaId!.Value;
        var partidas = await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas");
        partidas!.Single().Should().Match<PartidaResp>(p => p.Id == partidaId && p.Codigo == $"REC-{Anio}-000001/1" && p.Saldo == 10_000m);

        var envases = await e.Api.GetFromJsonAsync<EnvasesResp>($"/agro/agricultores/{e.Agricultor}/envases");
        envases!.Saldos.Single().Saldo.Should().Be(-40, "los palots llegan llenos: vuelven a la empresa");
        (await e.Api.PostAsJsonAsync($"/agro/agricultores/{e.Agricultor}/envases", new { EnvaseProductoId = e.Palot, Cantidad = 50 })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await e.Api.GetFromJsonAsync<EnvasesResp>($"/agro/agricultores/{e.Agricultor}/envases"))!.Saldos.Single().Saldo.Should().Be(10);

        // La base de datos impide cambiar una recepción confirmada, tocar el libro o dejar saldo negativo.
        (await RechazoAsync(e.Empresa, $"UPDATE agro.recepcion SET matricula = 'X' WHERE id = '{r.Id}'")).Hint.Should().Be("recepcion.confirmada");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.pesada SET bruto_kg = bruto_kg + 1 WHERE recepcion_id = '{r.Id}'")).Hint.Should().Be("recepcion.confirmada");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.movimiento_partida SET kilos = 1 WHERE partida_id = '{partidaId}'")).Hint.Should().Be("partida.movimiento_inmutable");
        var negativo = await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.movimiento_partida (id, empresa_id, partida_id, fecha, tipo, kilos, creado_en)
            VALUES (gen_random_uuid(), '{e.Empresa}', '{partidaId}', '{Dia:yyyy-MM-dd}', 'Ajuste', -10001, now())
            """);
        negativo.SqlState.Should().Be(GarantiasSql.CodigoError);
        negativo.Hint.Should().Be("partida.saldo_negativo");

        // Se anula mientras la partida no se haya usado.
        var anulada = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{r.Id}/anular", new { Motivo = "Báscula descalibrada" }));
        anulada.Estado.Should().Be("Anulada");
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Should().BeEmpty();
        (await e.Api.GetFromJsonAsync<EnvasesResp>($"/agro/agricultores/{e.Agricultor}/envases"))!.Saldos.Single().Saldo.Should().Be(50);
    }

    [Fact]
    public async Task La_liquidacion_por_clasificacion_genera_la_autofactura_con_compensacion_reagp()
    {
        var e = await EscenarioAsync();
        var r = await RecibirAsync(e, Dia);
        var partida = r.Lineas.Single().PartidaId!.Value;
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Naranja, Metodo = "PorClasificacion" })).EnsureSuccessStatusCode();
        await IdAsync(e.Api, "/agro/conceptos", new { Codigo = "TRANS", Nombre = "Transporte", Tipo = "PorKilo", Valor = 0.01m });

        var datos = new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), Fecha = new DateOnly(Anio, 3, 31) };

        // Sin clasificación ni precios: la previsualización dice todo lo que falta, sin guardar nada.
        var falta = await OkAsync<PrevisualizacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones/previsualizar", datos));
        falta.Valida.Should().BeFalse();
        falta.Errores.Single().Codigo.Should().Be("liquidacion.sin_clasificacion");

        await IdAsync(e.Api, $"/agro/partidas/{partida}/clasificaciones", new
        {
            Definitiva = true,
            Lineas = new[] { new { CategoriaId = e.Extra, KgMuestra = 60m }, new { CategoriaId = e.Primera, KgMuestra = 30m }, new { CategoriaId = e.Destrio, KgMuestra = 10m } },
        });
        var sinPrecio = await OkAsync<PrevisualizacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones/previsualizar", datos));
        sinPrecio.Errores.Select(x => x.Codigo).Should().Equal("liquidacion.sin_precio", "liquidacion.sin_precio", "liquidacion.sin_precio");

        foreach (var (categoria, precio) in new[] { (e.Extra, 0.40m), (e.Primera, 0.25m), (e.Destrio, 0m) })
        {
            await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Naranja, CategoriaId = categoria, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), PrecioKg = precio });
        }

        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/campanas/{e.Campana}/precios",
            new { ProductoId = e.Naranja, CategoriaId = e.Extra, Desde = new DateOnly(Anio, 3, 15), Hasta = new DateOnly(Anio, 4, 15), PrecioKg = 0.5m }), HttpStatusCode.Conflict))
            .Codigo.Should().Be("precio.solapado");

        var liq = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones", datos));
        liq.Estado.Should().Be("Borrador");
        liq.Kilos.Should().Be(10_000m);
        liq.Bruto.Should().Be(2_400m + 750m, "6.000 kg Extra × 0,40 + 3.000 kg Primera × 0,25; el destrío a 0");
        liq.TotalDescuentos.Should().Be(100m);
        liq.BaseImponible.Should().Be(3_050m);
        liq.CodigoImpuesto.Should().Be("REAGP12");
        liq.CuotaImpuesto.Should().Be(366m, "compensación REAGP del 12 %");
        liq.Retencion.Should().Be(61m, "retención del 2 %");
        liq.APagar.Should().Be(3_355m);

        // Las entregas quedan reservadas: otra liquidación del mismo periodo no las coge.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/liquidaciones", datos), HttpStatusCode.BadRequest)).Codigo.Should().Be("liquidacion.sin_lineas");

        // Sin autorización de autofacturación no se emite.
        (await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/emitir", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Codigo.Should().Be("liquidacion.sin_autofacturacion");
        (await e.Api.PutAsJsonAsync($"/agro/agricultores/{e.Agricultor}", new { e.ProveedorId, Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) })).EnsureSuccessStatusCode();

        var emitida = await OkAsync<LiquidacionResp>(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/emitir", UriKind.Relative), null));
        emitida.Numero.Should().Be($"LIQ-{Anio}-000001");
        var gasto = (await e.Api.GetFromJsonAsync<GastoResp>($"/gastos/{emitida.GastoId}"))!;
        gasto.CodigoIva.Should().Be("REAGP12");
        gasto.BaseImponible.Should().Be(3_050m);
        gasto.CuotaIva.Should().Be(366m);
        gasto.RetencionIrpf.Should().Be(61m);
        gasto.Total.Should().Be(emitida.APagar);

        var resumen = (await e.Api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={Anio}&trimestre=1"))!;
        resumen.Modelo303.CompensacionesReagpCuota.Should().Be(366m, "las compensaciones van a sus casillas del 303");

        // El precio aplicado ya no se toca, ni en la aplicación ni en la base de datos.
        var precioAplicado = (await e.Api.GetFromJsonAsync<LiquidacionResp>($"/agro/liquidaciones/{liq.Id}"))!;
        (await RechazoAsync(e.Empresa, $"UPDATE agro.liquidacion SET a_pagar = a_pagar + 1, total_factura = total_factura + 1 WHERE id = '{liq.Id}'")).Hint.Should().Be("liquidacion.emitida");
        _ = precioAplicado;

        // La partida recibida queda valorada al precio liquidado.
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Single().CosteKg.Should().Be(0.315m);

        // Anular la liquidación anula la autofactura y libera las entregas.
        var anulada = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync($"/agro/liquidaciones/{liq.Id}/anular", new { Motivo = "Precio mal pactado" }));
        anulada.Estado.Should().Be("Anulada");
        (await e.Api.GetFromJsonAsync<GastoResp>($"/gastos/{emitida.GastoId}"))!.Estado.Should().Be("Anulado");
        (await e.Api.PostAsJsonAsync("/agro/liquidaciones", datos)).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task La_liquidacion_por_periodo_en_regimen_general_detecta_precios_cambiados()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync($"/agro/agricultores/{e.Agricultor}", new { e.ProveedorId, Regimen = "General", AutofacturacionDesde = new DateOnly(Anio, 1, 1) })).EnsureSuccessStatusCode();
        await RecibirAsync(e, Dia, 5_500m, 500m);
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Naranja, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        var precio = await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Naranja, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 15), PrecioKg = 0.20m });

        var datos = new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 31), Fecha = new DateOnly(Anio, 3, 31) };
        var liq = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones", datos));
        liq.CodigoImpuesto.Should().Be("IVA4");
        liq.BaseImponible.Should().Be(1_000m);
        liq.CuotaImpuesto.Should().Be(40m);

        // Un precio en uso no se borra; cambia: la liquidación no se emite con importes viejos.
        (await ProblemaAsync(await e.Api.DeleteAsync($"/agro/precios/{precio}"), HttpStatusCode.Conflict)).Codigo.Should().Be("precio.en_uso");
        (await e.Api.PutAsJsonAsync($"/agro/precios/{precio}", new { ProductoId = e.Naranja, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 15), PrecioKg = 0.22m }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/emitir", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Codigo.Should().Be("liquidacion.desactualizada");

        var recalculada = await OkAsync<LiquidacionResp>(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/recalcular", UriKind.Relative), null));
        recalculada.BaseImponible.Should().Be(1_100m);
        var emitida = await OkAsync<LiquidacionResp>(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/emitir", UriKind.Relative), null));
        emitida.TotalFactura.Should().Be(1_144m);
        emitida.APagar.Should().Be(1_122m);

        // Aplicado en una emitida, el precio ya no cambia (tampoco saltándose la aplicación).
        (await ProblemaAsync(await e.Api.PutAsJsonAsync($"/agro/precios/{precio}", new { ProductoId = e.Naranja, Desde = new DateOnly(Anio, 3, 1), Hasta = new DateOnly(Anio, 3, 15), PrecioKg = 0.30m }),
            HttpStatusCode.Conflict)).Codigo.Should().Be("precio.aplicado");
    }

    [Fact]
    public async Task Confeccion_pales_expedicion_y_trazabilidad_del_campo_al_cliente()
    {
        var e = await EscenarioAsync();
        var r = await RecibirAsync(e, Dia);
        var partida = r.Lineas.Single().PartidaId!.Value;
        var caja = await IdAsync(e.Api, "/productos", new { Nombre = "Caja cartón 10 kg", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud", PrecioCompra = 0.30m });
        var naranja1 = await IdAsync(e.Api, "/productos", new { Nombre = "Naranja 1ª calibre 3", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var destrio = await IdAsync(e.Api, "/productos", new { Nombre = "Destrío industria", PrecioUnitario = 0.05m, Tipo = "Bien", Unidad = "kg" });
        var cliente = await IdAsync(e.Api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        await IdAsync(e.Api, "/agro/tarifas", new { Recurso = "ManoObra", Categoria = "PEON", TipoHora = "Normal", Desde = new DateOnly(Anio, 1, 1), CosteUnitario = 12m });
        await IdAsync(e.Api, "/agro/tarifas", new { Recurso = "ManoObra", Categoria = "PEON", TipoHora = "Destajo", Desde = new DateOnly(Anio, 1, 1), CosteUnitario = 0.10m });
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/tarifas", new { Recurso = "ManoObra", Categoria = "peon", TipoHora = "Normal", Desde = new DateOnly(Anio, 6, 1), CosteUnitario = 13m }),
            HttpStatusCode.Conflict)).Codigo.Should().Be("tarifa.solapada");

        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { Tipo = "Europeo" }));
        pale.Sscc.Should().HaveLength(18).And.StartWith("08400000");

        var parte = await OkAsync<ParteResp>(await e.Api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = Dia.AddDays(1), CampanaId = e.Campana, Descripcion = "Línea 1", PorcentajeIndirectos = 10m, Reparto = "PorFactor",
            Consumos = new[] { new { PartidaId = partida, Kilos = 6_000m } },
            ManoObra = new object[] { new { Descripcion = "Cuadrilla", Categoria = "PEON", TipoHora = "Normal", Horas = 20m }, new { Descripcion = "Encajadoras", Categoria = "PEON", TipoHora = "Destajo", Horas = 16m, Piezas = 500m } },
            Maquinas = new[] { new { Descripcion = "Calibradora", Categoria = "CALIBRADORA", Horas = 4m } },
            Materiales = new[] { new { ProductoId = caja, Cantidad = 500m } },
            Salidas = new object[] { new { ProductoId = naranja1, Kilos = 5_000m, Factor = 1m, PaleId = pale.Id }, new { ProductoId = destrio, Kilos = 800m, Factor = 0m } },
        }));

        // Falta la tarifa de la calibradora: la valoración lo dice y no valida.
        var valoracion = await OkAsync<ParteResp>(await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/valorar", UriKind.Relative), null));
        valoracion.Errores.Single().Codigo.Should().Be("parte.sin_tarifa");
        await IdAsync(e.Api, "/agro/tarifas", new { Recurso = "Maquina", Categoria = "CALIBRADORA", TipoHora = "Normal", Desde = new DateOnly(Anio, 1, 1), CosteUnitario = 30m });

        var validado = await OkAsync<ParteResp>(await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/validar", UriKind.Relative), null));
        validado.Numero.Should().Be($"PC-{Anio}-000001");
        validado.CosteManoObra.Should().Be(240m + 50m);
        // Fruta 6.000 kg × 0,30 (precio estimado) + cajas 150 + mano de obra 290 + máquina 120 + indirectos 41.
        validado.CosteTotal.Should().Be(1_800m + 150m + 290m + 120m + 41m);
        validado.Merma.Should().Be(200m);
        validado.Salidas[0].Coste.Should().Be(validado.CosteTotal, "el destrío tiene factor 0");
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Single(p => p.Id == partida).Saldo.Should().Be(4_000m);

        // Palé: cerrar y expedir al cliente.
        (await e.Api.GetFromJsonAsync<PaleResp>($"/agro/pales/{pale.Sscc}"))!.Kilos.Should().Be(5_000m);
        (await e.Api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var salida = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, ClienteId = cliente, Referencia = "ALB-77" }));
        salida.Single().Estado.Should().Be("Expedido");
        salida.Single().Kilos.Should().Be(5_000m);

        // Trazabilidad hacia atrás desde el SSCC hasta la parcela.
        var atras = (await e.Api.GetFromJsonAsync<TrazaResp>($"/agro/trazabilidad/atras?sscc={pale.Sscc}"))!;
        atras.Origenes.Single().Should().Match<OrigenResp>(o => o.Agricultor == "Juan Labrador" && o.Parcela == "P-01" && o.ReferenciaSigpac == "46:250:0:0:12:45:1");
        atras.Partidas.Should().HaveCount(2);

        // Hacia delante desde la recepción hasta el cliente.
        var adelante = (await e.Api.GetFromJsonAsync<TrazaResp>($"/agro/trazabilidad/adelante?recepcionId={r.Id}"))!;
        adelante.Destinos.Single().Should().Match<DestinoResp>(d => d.Sscc == pale.Sscc && d.Estado == "Expedido" && d.ClienteId == cliente && d.Kilos == 5_000m);

        // Con la salida ya expedida, el parte no se anula; y la base de datos no deja tocar el palé expedido.
        (await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/anular", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Codigo.Should().Be("parte.salidas_usadas");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.pale SET estado = 'Abierto', fecha_expedicion = NULL, cliente_id = NULL, referencia_expedicion = NULL WHERE id = '{pale.Id}'"))
            .Hint.Should().Be("pale.transicion");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.salida_parte SET coste = coste + 1 WHERE parte_id = '{parte.Id}'")).Hint.Should().Be("parte.validado");

        // Informe de campaña: kilos por agricultor y parcela, coste por kilo de la confección.
        var informe = (await e.Api.GetFromJsonAsync<InformeResp>($"/agro/informes/campana/{e.Campana}"))!;
        informe.KilosRecibidos.Should().Be(10_000m);
        informe.Agricultores.Single().KilosEntregados.Should().Be(10_000m);
        informe.Parcelas.Single().Should().Match<ParcelaCosteResp>(p => p.Codigo == "P-01" && p.KilosPorHa == 4_000m);
        informe.Confeccion.Single(c => c.ProductoId == naranja1).CosteKg.Should().Be(decimal.Round(validado.CosteTotal / 5_000m, 4));
    }

    [Fact]
    public async Task El_coste_por_kilo_de_la_parcela_sale_de_la_analitica()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        var centro = await IdAsync(e.Api, "/contabilidad/analitica/centros", new { Codigo = "FINCA-P01", Nombre = "Huerto del Río", Tipo = "Finca" });
        (await e.Api.PutAsJsonAsync($"/agro/parcelas/{e.Parcela}", new { Nombre = "Huerto del Río", ReferenciaSigpac = "46:250:0:0:12:45:1", SuperficieHa = 2.5m, CentroAnaliticoId = centro })).EnsureSuccessStatusCode();
        var fitos = await IdAsync(e.Api, "/proveedores", new { Nombre = "Fitosanitarios Levante SL", NifFiscal = Ayudas.GenerarNif() });
        await IdAsync(e.Api, "/contabilidad/analitica/reglas", new { TerceroId = fitos, CentroId = centro });
        (await e.Api.PostAsJsonAsync("/gastos", new { Concepto = "Tratamiento", BaseImponible = 1_500m, ProveedorId = fitos, Fecha = Dia })).StatusCode.Should().Be(HttpStatusCode.Created);

        await RecibirAsync(e, Dia);
        var informe = (await e.Api.GetFromJsonAsync<InformeResp>($"/agro/informes/campana/{e.Campana}"))!;
        informe.AnaliticaDisponible.Should().BeTrue();
        var parcela = informe.Parcelas.Single();
        parcela.CosteCultivo.Should().Be(1_500m);
        parcela.CosteCultivoPorKilo.Should().Be(0.15m, "1.500 € de la finca entre 10.000 kg");
    }

    [Fact]
    public async Task Confirmaciones_simultaneas_se_numeran_sin_huecos()
    {
        var e = await EscenarioAsync();
        var ids = new List<Guid>();
        for (var i = 0; i < 6; i++)
        {
            var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = e.Agricultor, Fecha = Dia });
            var r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = e.Naranja }));
            (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas.Single().Id}/pesadas", new { BrutoKg = 1_000m + i, TaraKg = 100m })).EnsureSuccessStatusCode();
            ids.Add(rec);
        }

        var respuestas = await Task.WhenAll(ids.Select(id => e.Api.PostAsync(new Uri($"/agro/recepciones/{id}/confirmar", UriKind.Relative), null)));
        respuestas.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        var numeros = (await Task.WhenAll(respuestas.Select(r => r.Content.ReadFromJsonAsync<RecepcionResp>()))).Select(r => r!.Numero).ToList();
        numeros.Should().BeEquivalentTo(Enumerable.Range(1, 6).Select(n => $"REC-{Anio}-{n:D6}"));

        // Confirmar dos veces la misma no la numera dos veces.
        (await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/recepciones/{ids[0]}/confirmar", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Codigo.Should().Be("recepcion.no_borrador");
    }

    [Fact]
    public async Task Anular_un_parte_devuelve_los_kilos_y_la_definitiva_no_se_sustituye_si_esta_liquidada()
    {
        var e = await EscenarioAsync();
        var r = await RecibirAsync(e, Dia);
        var partida = r.Lineas.Single().PartidaId!.Value;
        var naranja1 = await IdAsync(e.Api, "/productos", new { Nombre = "Naranja 1ª", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var parte = await OkAsync<ParteResp>(await e.Api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = Dia, Consumos = new[] { new { PartidaId = partida, Kilos = 2_000m } }, Salidas = new[] { new { ProductoId = naranja1, Kilos = 1_900m } },
        }));
        (await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/validar", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Single(p => p.Id == partida).Saldo.Should().Be(8_000m);

        // Una partida usada no deja anular su recepción.
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{r.Id}/anular", new { Motivo = "x" }), HttpStatusCode.Conflict))
            .Codigo.Should().Be("recepcion.partidas_usadas");

        var anulado = await OkAsync<ParteResp>(await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/anular", UriKind.Relative), null));
        anulado.Estado.Should().Be("Anulado");
        var partidas = (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!;
        partidas.Single().Should().Match<PartidaResp>(p => p.Id == partida && p.Saldo == 10_000m, "la salida anulada desaparece y el consumo vuelve");

        // Clasificación definitiva y liquidación en borrador: ya no se sustituye.
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Naranja, Metodo = "PorClasificacion" })).EnsureSuccessStatusCode();
        await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Naranja, CategoriaId = e.Extra, Desde = Dia, Hasta = Dia, PrecioKg = 0.4m });
        await IdAsync(e.Api, $"/agro/partidas/{partida}/clasificaciones", new { Definitiva = true, Lineas = new[] { new { CategoriaId = e.Extra, KgMuestra = 50m } } });
        (await e.Api.PostAsJsonAsync("/agro/liquidaciones", new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = Dia, Hasta = Dia, Fecha = Dia })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/partidas/{partida}/clasificaciones",
            new { Definitiva = true, Lineas = new[] { new { CategoriaId = e.Primera, KgMuestra = 50m } } }), HttpStatusCode.Conflict)).Codigo.Should().Be("clasificacion.liquidada");
    }

    [Fact]
    public async Task La_baja_de_la_empresa_borra_sus_datos_agro()
    {
        var e = await EscenarioAsync();
        var r = await RecibirAsync(e, Dia);
        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        (await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/paletizar", new { PartidaId = r.Lineas.Single().PartidaId, Kilos = 500m })).StatusCode.Should().Be(HttpStatusCode.OK);

        (await e.Api.DeleteAsync(new Uri("/cuenta", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand($"SELECT (SELECT count(*) FROM agro.recepcion WHERE empresa_id = '{e.Empresa}') + (SELECT count(*) FROM agro.movimiento_partida WHERE empresa_id = '{e.Empresa}')", c);
        ((long)(await cmd.ExecuteScalarAsync())!).Should().Be(0);
    }

    [Fact]
    public async Task Anular_la_liquidacion_contabiliza_el_contraasiento_de_la_autofactura()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        (await e.Api.PutAsJsonAsync($"/agro/agricultores/{e.Agricultor}", new { e.ProveedorId, Regimen = "Reagp", AutofacturacionDesde = new DateOnly(Anio, 1, 1) })).EnsureSuccessStatusCode();
        await RecibirAsync(e, Dia);
        (await e.Api.PutAsJsonAsync($"/agro/campanas/{e.Campana}/articulos", new { ProductoId = e.Naranja, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(e.Api, $"/agro/campanas/{e.Campana}/precios", new { ProductoId = e.Naranja, Desde = Dia, Hasta = Dia, PrecioKg = 0.25m });
        var liq = await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync("/agro/liquidaciones", new { AgricultorId = e.Agricultor, CampanaId = e.Campana, Desde = Dia, Hasta = Dia, Fecha = Dia }));
        await OkAsync<LiquidacionResp>(await e.Api.PostAsync(new Uri($"/agro/liquidaciones/{liq.Id}/emitir", UriKind.Relative), null));

        async Task<(decimal Debe, decimal Haber, long Asientos)> MayorAsync(string prefijo)
        {
            await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaAdmin);
            await c.OpenAsync();
            await using var cmd = new NpgsqlCommand($"""
                SELECT coalesce(sum(p.debe), 0), coalesce(sum(p.haber), 0), count(DISTINCT a.id)
                  FROM contabilidad.apunte p JOIN contabilidad.asiento a ON a.id = p.asiento_id
                 WHERE a.empresa_id = '{e.Empresa}' AND p.cuenta_codigo LIKE '{prefijo}%'
                """, c);
            await using var r = await cmd.ExecuteReaderAsync();
            await r.ReadAsync();
            return (r.GetDecimal(0), r.GetDecimal(1), r.GetInt64(2));
        }

        (await MayorAsync("472")).Should().Be((300m, 0m, 1L), "la compensación REAGP del 12 % sobre 2.500 € es IVA soportado deducible");
        (await OkAsync<LiquidacionResp>(await e.Api.PostAsJsonAsync($"/agro/liquidaciones/{liq.Id}/anular", new { Motivo = "Duplicada" }))).Estado.Should().Be("Anulada");
        (await MayorAsync("472")).Should().Be((300m, 300m, 2L), "el contraasiento deja la 472 a cero");
        var proveedor = await MayorAsync("40");
        proveedor.Debe.Should().Be(proveedor.Haber, "la deuda con el agricultor desaparece");
    }
}
