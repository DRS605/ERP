using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Cómo trabaja el almacén (respuestas del cliente): etiquetas SSCC puestas en la finca, pesada del camión entero con
/// varios productos, merma por familia, certificación por parcela, venta de cajas sueltas, palés con cajas distintas y
/// corrección de expediciones.
/// </summary>
public sealed class RespuestasTrazabilidadTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public RespuestasTrazabilidadTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record SeleccionResp(string Token);
    private sealed record EtiquetaResp(Guid Id, string Sscc, Guid AgricultorId, Guid? ParcelaId, Guid? PaleId);
    private sealed record PesadaResp(Guid Id, Guid LineaId, decimal BrutoKg, decimal NetoKg, decimal? TaraCamionKg, decimal TaraEnvasesKg, Guid? GrupoCamion, decimal? BrutoCamionKg);
    private sealed record PaleEntradaResp(Guid Id, string? SerieOrigen, Guid? PaleId, decimal? KilosAsignados);
    private sealed record LineaResp(Guid Id, Guid? PartidaId, decimal NetoKg);
    private sealed record RecepcionResp(Guid Id, string Estado, List<LineaResp> Lineas, List<PesadaResp> Pesadas, List<PaleEntradaResp> PalesEntrada);
    private sealed record PaleResp(Guid Id, string Sscc, string? Tipo, string Estado, decimal Kilos, Guid? ClienteId, string? ReferenciaExpedicion, int Cajas, int? CajasPorPale);
    private sealed record ErrorResp(string Codigo, string Mensaje);
    private sealed record ParteResp(Guid Id, string Estado, decimal PorcentajeMerma, decimal? ToleranciaMermaPct, List<ErrorResp> Errores);
    private sealed record CorreccionResp(string Tipo, Guid? ClienteAnteriorId, Guid? ClienteNuevoId, string? ReferenciaNueva, string? Motivo);
    private sealed record OrigenResp(string? Agricultor, decimal Kilos);
    private sealed record TrazaResp(List<OrigenResp> Origenes);

    private static readonly int Anio = DateTime.UtcNow.Year;
    private static readonly DateOnly Dia = new(Anio, 1, 10);

    private sealed record Escenario(HttpClient Api, Guid Empresa, Guid Campana, Guid Agricultor, Guid Parcela, Guid Pimiento, Guid Box);

    private async Task<Escenario> EscenarioAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var agricultor = await AgricultorAsync(api, "Finca La Vega");
        var parcela = await IdAsync(api, $"/agro/agricultores/{agricultor}/parcelas", new { Codigo = "P-01", Nombre = "Invernadero 1" });
        var pimiento = await IdAsync(api, "/productos", new { Nombre = "Pimiento verde", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var box = await IdAsync(api, "/productos", new { Nombre = "Box", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        await IdAsync(api, "/agro/taras", new { EnvaseProductoId = box, TaraKg = 28m, Desde = new DateOnly(Anio, 1, 1) });
        return new Escenario(api, empresa, campana, agricultor, parcela, pimiento, box);
    }

    private static async Task<Guid> AgricultorAsync(HttpClient api, string nombre)
    {
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif() });
        return await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
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

    private static Task<HttpResponseMessage> ConfirmarAsync(Escenario e, Guid rec) => e.Api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null);

    private static async Task<(Guid Recepcion, Guid Linea)> BorradorAsync(Escenario e, Guid agricultor, Guid? parcela, Guid producto)
    {
        var rec = await IdAsync(e.Api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = Dia });
        var r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas",
            new { ProductoId = producto, ParcelaId = parcela, EnvaseProductoId = e.Box, PrecioEstimadoKg = 0.5m }));
        return (rec, r.Lineas.Single().Id);
    }

    private static async Task<Guid> PartidaConKilosAsync(Escenario e)
    {
        var (rec, linea) = await BorradorAsync(e, e.Agricultor, e.Parcela, e.Pimiento);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 12_000m, TaraKg = 2_000m, Envases = 40 })).EnsureSuccessStatusCode();
        return (await OkAsync<RecepcionResp>(await ConfirmarAsync(e, rec))).Lineas.Single().PartidaId!.Value;
    }

    private async Task<PostgresException> RechazoAsync(Guid empresa, string sql)
    {
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using (var fijar = new NpgsqlCommand($"SELECT set_config('app.empresa_actual', '{empresa}', false)", c))
        {
            await fijar.ExecuteNonQueryAsync();
        }

        await using var cmd = new NpgsqlCommand(sql, c);
        return (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which;
    }

    [Fact]
    public async Task La_etiqueta_puesta_en_la_finca_es_el_SSCC_del_pale_de_entrada()
    {
        var e = await EscenarioAsync();
        var etiquetas = await OkAsync<List<EtiquetaResp>>(await e.Api.PostAsJsonAsync("/agro/etiquetas-campo", new { AgricultorId = e.Agricultor, ParcelaId = e.Parcela, Cantidad = 2 }));
        etiquetas.Should().HaveCount(2).And.OnlyContain(x => x.Sscc.Length == 18 && x.PaleId == null);
        var pdf = await e.Api.GetAsync(new Uri($"/agro/etiquetas-campo/pdf?ids={string.Join(',', etiquetas.Select(x => x.Id))}", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");

        // En la báscula: un palé trae la etiqueta de campo (leída con la pistola, con su IA 00) y otro llega sin etiqueta.
        var (rec, linea) = await BorradorAsync(e, e.Agricultor, e.Parcela, e.Pimiento);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 3_000m, TaraKg = 1_000m, Envases = 20 })).EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { SerieOrigen = $"(00){etiquetas[0].Sscc}", EnvaseProductoId = e.Box, Envases = 10 }))
            .EnsureSuccessStatusCode();
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pales", new { EnvaseProductoId = e.Box, Envases = 10 })).EnsureSuccessStatusCode();
        var r = await OkAsync<RecepcionResp>(await ConfirmarAsync(e, rec));
        var pales = (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!;
        var conEtiqueta = r.PalesEntrada.Single(p => p.SerieOrigen == etiquetas[0].Sscc);
        pales.Single(p => p.Id == conEtiqueta.PaleId).Sscc.Should().Be(etiquetas[0].Sscc, "el palé conserva el SSCC que se le puso en la finca");
        var nuevo = pales.Single(p => p.Id == r.PalesEntrada.Single(x => x.SerieOrigen == null).PaleId);
        nuevo.Sscc.Should().NotBe(etiquetas[0].Sscc).And.NotBe(etiquetas[1].Sscc, "el que llega sin etiqueta recibe un SSCC nuevo, sin chocar con las emitidas");
        (await e.Api.GetFromJsonAsync<List<EtiquetaResp>>("/agro/etiquetas-campo"))!.Single(x => x.Id == etiquetas[0].Id).PaleId.Should().Be(conEtiqueta.PaleId);
        (await e.Api.GetFromJsonAsync<List<EtiquetaResp>>("/agro/etiquetas-campo?libres=true"))!.Should().ContainSingle(x => x.Id == etiquetas[1].Id);

        // Una etiqueta no se usa dos veces ni con otro agricultor.
        var (otra, lineaOtra) = await BorradorAsync(e, e.Agricultor, e.Parcela, e.Pimiento);
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{otra}/lineas/{lineaOtra}/pales", new { SerieOrigen = etiquetas[0].Sscc, Envases = 1 })))
            .Codigo.Should().Be("etiqueta_campo.usada");
        var ajeno = await AgricultorAsync(e.Api, "Finca El Llano");
        var (deOtro, lineaDeOtro) = await BorradorAsync(e, ajeno, null, e.Pimiento);
        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{deOtro}/lineas/{lineaDeOtro}/pales", new { SerieOrigen = etiquetas[1].Sscc, Envases = 1 })))
            .Codigo.Should().Be("etiqueta_campo.otro_origen");

        // La base de datos: ningún otro palé toma el SSCC de una etiqueta, y la etiqueta no cambia.
        (await RechazoAsync(e.Empresa, $"""
            INSERT INTO agro.pale (id, empresa_id, sscc, estado, creado_en) VALUES (gen_random_uuid(), '{e.Empresa}', '{etiquetas[1].Sscc}', 'Abierto', now())
            """)).Hint.Should().Be("pale.sscc_de_etiqueta");
        (await RechazoAsync(e.Empresa, $"UPDATE agro.etiqueta_campo SET agricultor_id = '{ajeno}' WHERE id = '{etiquetas[1].Id}'")).Hint.Should().Be("etiqueta_campo.inmutable");
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.etiqueta_campo WHERE id = '{etiquetas[0].Id}'")).Hint.Should().Be("etiqueta_campo.usada");
    }

    [Fact]
    public async Task La_pesada_del_camion_entero_reparte_el_neto_entre_los_productos_por_sus_envases()
    {
        var e = await EscenarioAsync();
        var melon = await IdAsync(e.Api, "/productos", new { Nombre = "Melón piel de sapo", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var (rec, pimiento) = await BorradorAsync(e, e.Agricultor, e.Parcela, e.Pimiento);
        var r0 = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = melon, ParcelaId = e.Parcela, EnvaseProductoId = e.Box }));
        var melonLinea = r0.Lineas.Single(l => l.Id != pimiento).Id;

        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/pesada-camion", new
        {
            BrutoKg = 16_000m, TaraCamionKg = 5_000m, Lineas = new object[] { new { LineaId = pimiento, EnvasesPorTipo = new object[] { new { EnvaseProductoId = e.Box, Cantidad = 60 } } } },
        }))).Codigo.Should().Be("pesada_camion.lineas");

        // 60 + 40 box a 28 kg: 16.000 − 5.000 − 2.800 = 8.200 kg netos, 60/40 entre pimiento y melón.
        var r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/pesada-camion", new
        {
            BrutoKg = 16_000m, TaraCamionKg = 5_000m, Bascula = "T-1234",
            Lineas = new object[]
            {
                new { LineaId = pimiento, EnvasesPorTipo = new object[] { new { EnvaseProductoId = e.Box, Cantidad = 60 } } },
                new { LineaId = melonLinea, EnvasesPorTipo = new object[] { new { EnvaseProductoId = e.Box, Cantidad = 40 } } },
            },
        }));
        r.Pesadas.Should().HaveCount(2).And.OnlyContain(p => p.GrupoCamion != null && p.BrutoCamionKg == 16_000m);
        r.Pesadas.Sum(p => p.BrutoKg).Should().Be(16_000m);
        r.Pesadas.Single(p => p.LineaId == pimiento).Should().Match<PesadaResp>(p => p.NetoKg == 4_920m && p.TaraCamionKg == 3_000m && p.TaraEnvasesKg == 1_680m);
        r.Pesadas.Single(p => p.LineaId == melonLinea).NetoKg.Should().Be(3_280m);

        // Se quita entera, y la base de datos no deja descuadrarla.
        var quitada = await OkAsync<RecepcionResp>(await e.Api.DeleteAsync(new Uri($"/agro/recepciones/{rec}/pesadas/{r.Pesadas[0].Id}", UriKind.Relative)));
        quitada.Pesadas.Should().BeEmpty();
        r = await OkAsync<RecepcionResp>(await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/pesada-camion", new
        {
            BrutoKg = 16_000m, TaraCamionKg = 5_000m,
            Lineas = new object[]
            {
                new { LineaId = pimiento, EnvasesPorTipo = new object[] { new { EnvaseProductoId = e.Box, Cantidad = 60 } } },
                new { LineaId = melonLinea, EnvasesPorTipo = new object[] { new { EnvaseProductoId = e.Box, Cantidad = 40 } } },
            },
        }));
        (await RechazoAsync(e.Empresa, $"UPDATE agro.pesada SET bruto_kg = bruto_kg + 100 WHERE id = '{r.Pesadas[0].Id}'")).Hint.Should().Be("pesada_camion.no_cuadra");
        var confirmada = await OkAsync<RecepcionResp>(await ConfirmarAsync(e, rec));
        confirmada.Lineas.Sum(l => l.NetoKg).Should().Be(8_200m);
    }

    [Fact]
    public async Task La_merma_se_tolera_por_familia_y_la_certificacion_exige_la_parcela()
    {
        var e = await EscenarioAsync();
        (await e.Api.PutAsJsonAsync("/agro/configuracion", new { PrefijoGs1 = "8400000", DigitoExtension = 0, ToleranciaMermaPct = 5m })).EnsureSuccessStatusCode();
        var familia = await IdAsync(e.Api, "/familias", new { Nombre = "Sandía", Codigo = "SAN" });
        var sandia = await IdAsync(e.Api, "/productos", new { Nombre = "Sandía negra", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg", FamiliaId = familia });
        (await e.Api.PutAsJsonAsync($"/agro/tolerancias-merma/{familia}", new { MermaMaximaPct = 15m })).EnsureSuccessStatusCode();

        var (rec, linea) = await BorradorAsync(e, e.Agricultor, e.Parcela, sandia);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{linea}/pesadas", new { BrutoKg = 12_000m, TaraKg = 2_000m, Envases = 40 })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await ConfirmarAsync(e, rec))).Lineas.Single().PartidaId!.Value;

        // 12 % de merma en sandía: dentro de su 15 %, aunque la general sea el 5 %. La base de datos lo admite con la tolerancia aplicada.
        var parte = await OkAsync<ParteResp>(await e.Api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = Dia.AddDays(1), CampanaId = e.Campana, Consumos = new object[] { new { PartidaId = partida, Kilos = 1_000m } },
            Salidas = new object[] { new { ProductoId = sandia, Kilos = 880m } },
        }));
        var validado = await OkAsync<ParteResp>(await e.Api.PostAsync(new Uri($"/agro/partes/{parte.Id}/validar", UriKind.Relative), null));
        validado.ToleranciaMermaPct.Should().Be(15m);

        (await e.Api.PutAsJsonAsync($"/agro/tolerancias-merma/{familia}", new { MermaMaximaPct = 10m })).EnsureSuccessStatusCode();
        var otro = await OkAsync<ParteResp>(await e.Api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = Dia.AddDays(1), CampanaId = e.Campana, Consumos = new object[] { new { PartidaId = partida, Kilos = 1_000m } },
            Salidas = new object[] { new { ProductoId = sandia, Kilos = 880m } },
        }));
        (await ProblemaAsync(await e.Api.PostAsync(new Uri($"/agro/partes/{otro.Id}/validar", UriKind.Relative), null))).Codigo.Should().Be("parte.merma_excesiva");

        // Lo que se vende certificado tiene que venir de una parcela.
        (await e.Api.PutAsJsonAsync($"/agro/declaraciones/{sandia}", new { Exige = "Ecologico" })).EnsureSuccessStatusCode();
        var (sinParcela, lineaSin) = await BorradorAsync(e, e.Agricultor, null, sandia);
        (await e.Api.PostAsJsonAsync($"/agro/recepciones/{sinParcela}/lineas/{lineaSin}/pesadas", new { BrutoKg = 3_000m, TaraKg = 1_000m, Envases = 10 })).EnsureSuccessStatusCode();
        (await ProblemaAsync(await ConfirmarAsync(e, sinParcela))).Codigo.Should().Be("recepcion.parcela_certificada");
    }

    [Fact]
    public async Task Se_venden_cajas_sueltas_de_un_pale_y_cada_pale_lleva_sus_cajas()
    {
        var e = await EscenarioAsync();
        var partida = await PartidaConKilosAsync(e);
        var plantilla = await IdAsync(e.Api, "/agro/plantillas-pale", new { Codigo = "P50", Nombre = "50 cajas de 10 kg", CajasPorPale = 50, KilosPorCaja = 10m, EnvaseProductoId = e.Box });

        // Este palé lleva 30 cajas, no las 50 de la plantilla: se cierra al llegar a 30.
        var corto = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { PlantillaId = plantilla, CajasPorPale = 30 }));
        corto.CajasPorPale.Should().Be(30);
        var lleno = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{corto.Id}/cajas", new { PartidaId = partida, Cajas = 30, Fecha = Dia }));
        lleno.Should().Match<PaleResp>(p => p.Estado == "Cerrado" && p.Cajas == 30 && p.Kilos == 300m);

        // Del palé cerrado se venden 7 cajas sueltas: salen en su propio bulto con SSCC, y el palé se queda con 23.
        var expedidos = await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new
        {
            Fecha = Dia.AddDays(1), Referencia = "Venta mostrador", Sueltas = new object[] { new { OrigenPaleId = corto.Id, PartidaId = partida, Cajas = 7 } },
        }));
        var bulto = expedidos.Single();
        bulto.Tipo.Should().Be("Cajas sueltas");
        bulto.Estado.Should().Be("Expedido");
        bulto.Kilos.Should().Be(70m, "el bulto muestra lo que salió en él");
        bulto.Cajas.Should().Be(7);
        var pales = (await e.Api.GetFromJsonAsync<List<PaleResp>>("/agro/pales"))!;
        pales.Single(p => p.Id == corto.Id).Should().Match<PaleResp>(p => p.Cajas == 23 && p.Kilos == 230m && p.Estado == "Cerrado");
        (await e.Api.GetFromJsonAsync<TrazaResp>($"/agro/trazabilidad/atras?sscc={bulto.Sscc}"))!.Origenes.Should().ContainSingle(o => o.Agricultor == "Finca La Vega");
        (await ProblemaAsync(await e.Api.PostAsJsonAsync("/agro/expediciones", new
        {
            Fecha = Dia.AddDays(1), Sueltas = new object[] { new { OrigenPaleId = corto.Id, PartidaId = partida, Cajas = 24 } },
        }))).Codigo.Should().Be("repaletizado.cajas");
    }

    [Fact]
    public async Task Una_expedicion_se_corrige_con_motivo_y_queda_registrado_el_antes_y_el_despues()
    {
        var e = await EscenarioAsync();
        var partida = await PartidaConKilosAsync(e);
        var cliente = await IdAsync(e.Api, "/clientes", new { Nombre = "Frutas del Norte", NifFiscal = Ayudas.GenerarNif() });
        var pale = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync("/agro/pales", new { }));
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/paletizar", new { PartidaId = partida, Kilos = 500m, Fecha = Dia }));
        (await e.Api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        await OkAsync<List<PaleResp>>(await e.Api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, Fecha = Dia.AddDays(1), Referencia = "R-1" }));

        (await ProblemaAsync(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/corregir-expedicion", new { ClienteId = cliente, Referencia = "R-1" })))
            .Codigo.Should().Be("correccion.motivo");
        var corregido = await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/corregir-expedicion",
            new { ClienteId = cliente, Referencia = "R-1B", Motivo = "Se cargó para Frutas del Norte" }));
        corregido.Should().Match<PaleResp>(p => p.ClienteId == cliente && p.ReferenciaExpedicion == "R-1B" && p.Estado == "Expedido");

        // Anular la expedición también queda registrado.
        await OkAsync<PaleResp>(await e.Api.PostAsJsonAsync($"/agro/pales/{pale.Id}/anular-expedicion", new { Motivo = "Devuelto en el muelle" }));
        var correcciones = (await e.Api.GetFromJsonAsync<List<CorreccionResp>>($"/agro/pales/{pale.Id}/correcciones"))!;
        correcciones.Should().HaveCount(2);
        correcciones[0].Should().Match<CorreccionResp>(c => c.Tipo == "Datos" && c.ClienteAnteriorId == null && c.ClienteNuevoId == cliente && c.ReferenciaNueva == "R-1B");
        correcciones[1].Should().Match<CorreccionResp>(c => c.Tipo == "Anulacion" && c.ClienteAnteriorId == cliente && c.Motivo == "Devuelto en el muelle");
        (await RechazoAsync(e.Empresa, $"DELETE FROM agro.correccion_expedicion WHERE pale_id = '{pale.Id}'")).Hint.Should().Be("correccion_expedicion.inmutable");
    }
}
