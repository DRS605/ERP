using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AlxorCore.Documentos.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Palés rápidos: plantilla de palé (cajas por palé, kilos por caja, mosaico), montaje de todos los palés de una partida
/// de una vez, paletizado por cajas con cierre automático, etiqueta GS1 y carta de porte al expedir.
/// </summary>
public sealed class PalesRapidosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public PalesRapidosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record PartidaResp(Guid Id, decimal Saldo);
    private sealed record ContenidoResp(Guid PartidaId, decimal Kilos, int Cajas);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, decimal Kilos, int Cajas, int? CajasPorPale, Guid? PlantillaId, Guid? CartaPorteId,
        string? ReferenciaExpedicion, List<ContenidoResp> Contenido);
    private sealed record PlantillaResp(Guid Id, string Codigo, int CajasPorPale, decimal KilosPorCaja, int? CajasPorCapa, int? Capas, decimal KilosPorPale);
    private sealed record LineaCartaResp(string Descripcion, int Bultos, decimal PesoKg);
    private sealed record CartaResp(Guid Id, string NumeroCompleto, Guid? DestinatarioClienteId, int TotalBultos, decimal TotalPesoKg, bool Anulada, List<LineaCartaResp> Lineas);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private sealed record Escenario(HttpClient Api, Guid Naranja, Guid Partida, Guid Cliente);

    /// <summary>Empresa con agro y una partida de 10.000 kg de naranja sueltos.</summary>
    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var dia = new DateOnly(Anio, 3, 10);
        await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Huerto", SuperficieHa = 2m });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = dia });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            new { ProductoId = naranja, ParcelaId = parcela, FechaRecoleccion = dia, EnvaseProductoId = palot }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 12_000m, TaraKg = 2_000m, Envases = 40 })).EnsureSuccessStatusCode();
        var confirmada = await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null));
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        return new Escenario(api, naranja, confirmada.Lineas[0].PartidaId!.Value, cliente);
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

    private static async Task<string> CodigoAsync(HttpResponseMessage r, HttpStatusCode esperado)
    {
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static async Task<decimal> SueltosAsync(Escenario e) =>
        (await e.Api.GetFromJsonAsync<List<PartidaResp>>("/agro/partidas"))!.Single(p => p.Id == e.Partida).Saldo;

    private static Task<HttpResponseMessage> PlantillaAsync(Escenario e, string codigo, Guid? producto = null) =>
        e.Api.PostAsJsonAsync("/agro/plantillas-pale", new
        {
            Codigo = codigo, Nombre = "Europeo 80 cajas de 12,5 kg", TipoPale = "Europeo", ProductoId = producto, Marca = "Sol de Levante",
            CajasPorPale = 80, KilosPorCaja = 12.5m, Filas = 4, Columnas = 5,
        });

    [Fact]
    public async Task Una_partida_se_monta_de_una_vez_en_palés_completos_y_cerrados()
    {
        var e = await EscenarioAsync();
        var plantilla = await OkAsync<PlantillaResp>(await PlantillaAsync(e, "EUR80", e.Naranja));
        plantilla.CajasPorCapa.Should().Be(20);
        plantilla.Capas.Should().Be(4);
        plantilla.KilosPorPale.Should().Be(1_000m);

        // 10.000 kg sueltos / 1.000 kg por palé = 10 palés completos, cerrados, con SSCC correlativos.
        var pales = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla.Id, PartidaId = e.Partida }));
        pales.Should().HaveCount(10);
        pales.Should().OnlyContain(p => p.Estado == "Cerrado" && p.Cajas == 80 && p.Kilos == 1_000m && p.CajasPorPale == 80 && p.PlantillaId == plantilla.Id);
        pales.Select(p => p.Sscc).Should().OnlyHaveUniqueItems();
        (await SueltosAsync(e)).Should().Be(10_000m, "el saldo de la partida incluye lo paletizado");

        // Ya no quedan kilos sueltos: otro montaje no llega a un palé.
        (await CodigoAsync(await e.Api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla.Id, PartidaId = e.Partida }), HttpStatusCode.Conflict))
            .Should().Be("montaje.sin_kilos");

        // Una plantilla para otro producto no admite la partida.
        var otro = await IdAsync(e.Api, "/productos", new { Nombre = "Mandarina", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var ajena = await OkAsync<PlantillaResp>(await PlantillaAsync(e, "MAND", otro));
        (await CodigoAsync(await e.Api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = ajena.Id, PartidaId = e.Partida, NumeroPales = 1 }), HttpStatusCode.BadRequest))
            .Should().Be("plantilla.producto");
    }

    [Fact]
    public async Task Por_cajas_el_ultimo_palé_queda_abierto_y_se_cierra_solo_al_completarse()
    {
        var e = await EscenarioAsync();
        var plantilla = await OkAsync<PlantillaResp>(await PlantillaAsync(e, "EUR80"));

        // 100 cajas: un palé completo (cerrado) y otro con 20 cajas (abierto).
        var pales = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla.Id, PartidaId = e.Partida, Cajas = 100 }));
        pales.Select(p => (p.Estado, p.Cajas)).Should().Equal(("Cerrado", 80), ("Abierto", 20));
        var abierto = pales[1];

        // Cabe lo que falta; más no.
        (await CodigoAsync(await e.Api.PostAsJsonAsync($"/agro/pales/{abierto.Id}/cajas", new { PartidaId = e.Partida, Cajas = 61 }), HttpStatusCode.Conflict))
            .Should().Be("pale.completo");

        // Se sacan 5 cajas (vuelven sueltas) y luego se completan: el palé se cierra solo.
        var menos = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{abierto.Id}/cajas", new { PartidaId = e.Partida, Cajas = -5 }));
        menos.Cajas.Should().Be(15);
        menos.Kilos.Should().Be(187.5m);
        var lleno = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{abierto.Id}/cajas", new { PartidaId = e.Partida, Cajas = 65 }));
        lleno.Estado.Should().Be("Cerrado");
        lleno.Cajas.Should().Be(80);
        lleno.Kilos.Should().Be(1_000m);

        // Un palé con plantilla no se paletiza por kilos.
        var nuevo = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { PlantillaId = plantilla.Id }));
        nuevo.PlantillaId.Should().Be(plantilla.Id);
        (await CodigoAsync(await e.Api.PostAsJsonAsync($"/agro/pales/{nuevo.Id}/paletizar", new { PartidaId = e.Partida, Kilos = 100m }), HttpStatusCode.BadRequest))
            .Should().Be("pale.por_cajas");

        // La plantilla usada no cambia sus medidas ni se elimina; una sin usar, sí.
        (await CodigoAsync(await e.Api.PutAsJsonAsync($"/agro/plantillas-pale/{plantilla.Id}",
            new { Nombre = "Otro", CajasPorPale = 60, KilosPorCaja = 12.5m, Filas = 4, Columnas = 5 }), HttpStatusCode.Conflict)).Should().Be("plantilla.en_uso");
        (await e.Api.PutAsJsonAsync($"/agro/plantillas-pale/{plantilla.Id}",
            new { Nombre = "Europeo (antiguo)", CajasPorPale = 80, KilosPorCaja = 12.5m, Filas = 4, Columnas = 5, Activa = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await CodigoAsync(await e.Api.DeleteAsync(new Uri($"/agro/plantillas-pale/{plantilla.Id}", UriKind.Relative)), HttpStatusCode.Conflict)).Should().Be("plantilla.en_uso");
        var libre = await OkAsync<PlantillaResp>(await PlantillaAsync(e, "LIBRE"));
        (await e.Api.DeleteAsync(new Uri($"/agro/plantillas-pale/{libre.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Mosaico incoherente: 80 cajas no son capas enteras de 3 × 7.
        (await CodigoAsync(await e.Api.PostAsJsonAsync("/agro/plantillas-pale", new { Codigo = "MAL", Nombre = "Mal", CajasPorPale = 80, KilosPorCaja = 10m, Filas = 3, Columnas = 7 }),
            HttpStatusCode.BadRequest)).Should().Be("plantilla.mosaico");
    }

    [Fact]
    public async Task Al_expedir_se_emite_la_carta_de_porte_y_se_anula_cuando_vuelven_todos_sus_palés()
    {
        var e = await EscenarioAsync();
        var plantilla = await OkAsync<PlantillaResp>(await PlantillaAsync(e, "EUR80", e.Naranja));
        var pales = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla.Id, PartidaId = e.Partida, NumeroPales = 2 }));

        // Sin cliente no hay carta de porte.
        (await CodigoAsync(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = pales.Select(p => p.Id), CartaPorte = true }), HttpStatusCode.BadRequest))
            .Should().Be("expedicion.carta_sin_cliente");

        var salida = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new
        {
            PaleIds = pales.Select(p => p.Id), ClienteId = e.Cliente, CartaPorte = true, Transportista = "Transportes Ribera", Matricula = "1234 BCD",
            LugarOrigen = "Almacén de Alzira", LugarDestino = "Mercabilbao",
        }));
        salida.Should().OnlyContain(p => p.Estado == "Expedido" && p.CartaPorteId != null && p.Cajas == 80);
        var cartaId = salida[0].CartaPorteId!.Value;
        var carta = (await e.Api.GetFromJsonAsync<CartaResp>($"/cartas-porte/{cartaId}"))!;
        carta.DestinatarioClienteId.Should().Be(e.Cliente);
        carta.TotalBultos.Should().Be(160);
        carta.TotalPesoKg.Should().Be(2_000m);
        carta.Lineas.Single().Descripcion.Should().Be("Naranja Navel · 2 palé(s)");
        salida[0].ReferenciaExpedicion.Should().Be(carta.NumeroCompleto);

        // Vuelve un palé: la carta sigue viva (el otro salió). Vuelve el otro: se anula.
        (await OkAsync<PaleResp>(await e.Api.PostAsync(new Uri($"/agro/pales/{pales[0].Id}/anular-expedicion", UriKind.Relative), null))).CartaPorteId.Should().BeNull();
        (await e.Api.GetFromJsonAsync<CartaResp>($"/cartas-porte/{cartaId}"))!.Anulada.Should().BeFalse();
        var vuelta = await OkAsync<PaleResp>(await e.Api.PostAsync(new Uri($"/agro/pales/{pales[1].Id}/anular-expedicion", UriKind.Relative), null));
        vuelta.Estado.Should().Be("Cerrado");
        vuelta.Cajas.Should().Be(80, "las cajas vuelven con sus kilos");
        (await e.Api.GetFromJsonAsync<CartaResp>($"/cartas-porte/{cartaId}"))!.Anulada.Should().BeTrue();

        // La etiqueta es un PDF.
        var etiqueta = await e.Api.GetAsync(new Uri($"/agro/pales/{vuelta.Sscc}/etiqueta", UriKind.Relative));
        etiqueta.StatusCode.Should().Be(HttpStatusCode.OK);
        etiqueta.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await etiqueta.Content.ReadAsByteArrayAsync()).Take(4).Should().Equal("%PDF"u8.ToArray());
    }

    /// <summary>Anchuras de referencia generadas con otro codificador (bwip-js, «gs1-128»).</summary>
    [Theory]
    [InlineData("00:384000000000000018", "2112324111312122221323112311132122222122222122222122222122222122222232112214112331112")]
    [InlineData("3102:002000|37:80|10:P-2026-0001",
        "2112324111312123212222212122222212312122221321131112424111312213121141313131211221321131412212313212211141311221321131412122222221221241122331112")]
    [InlineData("3103:012345|37:7|10:AB12",
        "2112324111312123211212232221223121311131231321131141313121314111311232211231221113231311231232212232111121332331112")]
    public void El_codigo_gs1_128_coincide_con_la_referencia(string elementos, string anchuras)
    {
        var lista = elementos.Split('|').Select(x => (x.Split(':')[0], x.Split(':')[1])).ToList();
        string.Concat(Gs1128.Anchuras(Gs1128.Componer(lista))).Should().Be(anchuras);
    }

    [Fact]
    public void El_peso_neto_usa_los_decimales_que_caben()
    {
        Gs1128.PesoNeto(1_000m).Should().Be(("3102", "100000"));
        Gs1128.PesoNeto(187.5m).Should().Be(("3103", "187500"));
        Gs1128.PesoNeto(12_000m).Should().Be(("3101", "120000"));
        Gs1128.Patrones.Should().OnlyContain(p => p.Length == 6 || p.Length == 7);
        Gs1128.Patrones.Take(106).Should().OnlyContain(p => p.Sum(c => c - '0') == 11);
    }
}
