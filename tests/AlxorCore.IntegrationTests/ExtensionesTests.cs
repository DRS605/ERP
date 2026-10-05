using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Extensiones: campos personalizados con tipos y validación, búsqueda por valor, adjuntos con descarga y huella, y
/// alertas (vencidas, riesgo, fecha de un campo y evento) que se crean una vez y se resuelven solas.
/// </summary>
public sealed class ExtensionesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ExtensionesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo, string? Title);
    private sealed record ValorResp(Guid CampoId, string Codigo, string Tipo, bool Obligatorio, string? Valor, bool PorDefecto);
    private sealed record AdjuntoResp(Guid Id, string Nombre, string TipoMime, long Tamano, string Huella, string? SubidoPor);
    private sealed record AlertaResp(Guid Id, Guid ReglaId, string Titulo, string? Detalle, string? Entidad, Guid? EntidadId, DateTimeOffset? ResueltaEn, bool Leida);
    private sealed record ResumenResp(int Vivas, int SinLeer, List<AlertaResp> Alertas);
    private sealed record EvaluacionResp(int Nuevas, int Resueltas);
    private sealed record ReglaResp(Guid Id, string Tipo, int? Dias, int Vivas, bool Activa);

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

    private static Task<Guid> ClienteAsync(HttpClient api, string nombre, decimal? limite = null) =>
        IdAsync(api, "/clientes", new { Nombre = nombre, NifFiscal = Ayudas.GenerarNif(), LimiteRiesgo = limite });

    private static Task<Guid> FacturaAsync(HttpClient api, Guid cliente, int haceDias) =>
        IdAsync(api, "/facturas", new
        {
            ClienteId = cliente, FechaEmision = Hoy.AddDays(-haceDias), DiasVencimiento = 0,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "Servicio", PrecioUnitario = 100m, CodigoIva = "IVA21" } },
        });

    [Fact]
    public async Task Los_campos_validan_normalizan_y_se_buscan()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(api, "Frutas Sol SL");
        var otro = await ClienteAsync(api, "Otro SL");
        var zona = await IdAsync(api, "/extensiones/campos", new { Entidad = "cliente", Codigo = "zona", Tipo = "Lista", Etiqueta = "Zona", Opciones = new[] { "Norte", "Sur" }, Obligatorio = true, ValorPorDefecto = "norte" });
        await IdAsync(api, "/extensiones/campos", new { Entidad = "cliente", Codigo = "visitas", Tipo = "Numero" });
        await IdAsync(api, "/extensiones/campos", new { Entidad = "cliente", Codigo = "alta_global", Tipo = "Fecha" });
        await IdAsync(api, "/extensiones/campos", new { Entidad = "cliente", Codigo = "vip", Tipo = "SiNo" });

        (await FalloAsync(api.PostAsJsonAsync("/extensiones/campos", new { Entidad = "cliente", Codigo = "Zona", Tipo = "Texto" }), HttpStatusCode.Conflict)).Should().Be("campo.duplicado");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/campos", new { Entidad = "cliente", Codigo = "1mal", Tipo = "Texto" }), HttpStatusCode.BadRequest)).Should().Be("campo.codigo");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/campos", new { Entidad = "nave", Codigo = "x", Tipo = "Texto" }), HttpStatusCode.BadRequest)).Should().Be("campo.entidad");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/campos", new { Entidad = "cliente", Codigo = "tipo", Tipo = "Lista" }), HttpStatusCode.BadRequest)).Should().Be("campo.opciones");

        // Sin valores: el obligatorio sale con su defecto (normalizado a la opción).
        var vacio = await OkAsync<List<ValorResp>>(api.GetAsync($"/extensiones/valores/cliente/{cliente}"));
        vacio.Should().HaveCount(4);
        vacio.Single(v => v.Codigo == "zona").Should().Match<ValorResp>(v => v.Valor == "Norte" && v.PorDefecto);

        // Valores con formato español: se normalizan; un error en uno no guarda ninguno.
        (await FalloAsync(api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}", new { Valores = new Dictionary<string, string?> { ["visitas"] = "12,5", ["vip"] = "quizá" } }),
            HttpStatusCode.BadRequest)).Should().Be("campo.valores");
        var guardado = await OkAsync<List<ValorResp>>(api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}",
            new { Valores = new Dictionary<string, string?> { ["visitas"] = "12,5", ["alta_global"] = "15/03/2026", ["vip"] = "sí", ["zona"] = "sur" } }));
        guardado.Select(v => (v.Codigo, v.Valor)).Should().BeEquivalentTo(new[] { ("zona", "Sur"), ("visitas", "12.5"), ("alta_global", "2026-03-15"), ("vip", "true") });
        (await FalloAsync(api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}", new { Valores = new Dictionary<string, string?> { ["zona"] = "" } }), HttpStatusCode.BadRequest))
            .Should().Be("campo.valores");
        (await FalloAsync(api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}", new { Valores = new Dictionary<string, string?> { ["nada"] = "x" } }), HttpStatusCode.BadRequest))
            .Should().Be("campo.valores");
        // El otro cliente toma el defecto del obligatorio al guardar.
        await OkAsync<List<ValorResp>>(api.PutAsJsonAsync($"/extensiones/valores/cliente/{otro}", new { Valores = new Dictionary<string, string?> { ["visitas"] = "3" } }));

        (await OkAsync<List<Guid>>(api.GetAsync("/extensiones/buscar/cliente?codigo=zona&valor=sur"))).Should().Equal(cliente);
        (await OkAsync<List<Guid>>(api.GetAsync("/extensiones/buscar/cliente?codigo=zona&valor=Norte"))).Should().Equal(otro);
        (await OkAsync<List<Guid>>(api.GetAsync("/extensiones/buscar/cliente?codigo=visitas&desde=5"))).Should().Equal(cliente);
        (await OkAsync<List<Guid>>(api.GetAsync("/extensiones/buscar/cliente?codigo=visitas&hasta=5"))).Should().Equal(otro);

        // Una opción en uso no se puede quitar; un campo con valores no se borra (se desactiva).
        (await FalloAsync(api.PutAsJsonAsync($"/extensiones/campos/{zona}", new { Tipo = "Lista", Etiqueta = "Zona", Opciones = new[] { "Norte" }, Obligatorio = true }),
            HttpStatusCode.Conflict)).Should().Be("campo.opciones_en_uso");
        (await FalloAsync(api.DeleteAsync($"/extensiones/campos/{zona}"), HttpStatusCode.Conflict)).Should().Be("campo.con_valores");
        (await api.PutAsJsonAsync($"/extensiones/campos/{zona}", new { Tipo = "Lista", Etiqueta = "Zona", Opciones = new[] { "Norte", "Sur", "Este" }, Activo = false }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await OkAsync<List<ValorResp>>(api.GetAsync($"/extensiones/valores/cliente/{cliente}"))).Should().NotContain(v => v.Codigo == "zona");
        var libre = await IdAsync(api, "/extensiones/campos", new { Entidad = "producto", Codigo = "temporal", Tipo = "Texto" });
        (await api.DeleteAsync($"/extensiones/campos/{libre}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Los_adjuntos_se_suben_descargan_y_quitan()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(api, "Adjuntos SL");
        var contenido = Encoding.UTF8.GetBytes("certificado GlobalG.A.P. de prueba");
        var subido = await OkAsync<AdjuntoResp>(api.PostAsJsonAsync($"/extensiones/adjuntos/cliente/{cliente}",
            new { Nombre = @"C:\docs\certificado.pdf", ContenidoBase64 = Convert.ToBase64String(contenido), Descripcion = "Vigente" }));
        subido.Nombre.Should().Be("certificado.pdf");
        subido.TipoMime.Should().Be("application/pdf");
        subido.Tamano.Should().Be(contenido.Length);
        subido.Huella.Should().Be(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(contenido)));

        (await FalloAsync(api.PostAsJsonAsync($"/extensiones/adjuntos/cliente/{cliente}", new { Nombre = "virus.exe", ContenidoBase64 = "AAAA" }), HttpStatusCode.BadRequest))
            .Should().Be("adjunto.tipo");
        (await FalloAsync(api.PostAsJsonAsync($"/extensiones/adjuntos/cliente/{cliente}", new { Nombre = "a.pdf", ContenidoBase64 = "no es base64!" }), HttpStatusCode.BadRequest))
            .Should().Be("adjunto.contenido");
        (await FalloAsync(api.PostAsJsonAsync($"/extensiones/adjuntos/nave/{cliente}", new { Nombre = "a.pdf", ContenidoBase64 = "AAAA" }), HttpStatusCode.BadRequest))
            .Should().Be("extensiones.entidad");

        (await OkAsync<List<AdjuntoResp>>(api.GetAsync($"/extensiones/adjuntos/cliente/{cliente}"))).Should().ContainSingle(a => a.Id == subido.Id);
        (await OkAsync<Dictionary<Guid, int>>(api.PostAsJsonAsync("/extensiones/adjuntos/cliente/contar", new { Ids = new[] { cliente, Guid.NewGuid() } })))
            .Should().BeEquivalentTo(new Dictionary<Guid, int> { [cliente] = 1 });
        var descarga = await api.GetAsync($"/extensiones/adjuntos/{subido.Id}/contenido");
        descarga.StatusCode.Should().Be(HttpStatusCode.OK);
        descarga.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await descarga.Content.ReadAsByteArrayAsync()).Should().Equal(contenido);

        // Otra empresa no lo ve.
        var (ajena, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await ajena.GetAsync($"/extensiones/adjuntos/{subido.Id}/contenido")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        (await api.DeleteAsync($"/extensiones/adjuntos/{subido.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await OkAsync<List<AdjuntoResp>>(api.GetAsync($"/extensiones/adjuntos/cliente/{cliente}"))).Should().BeEmpty();
    }

    [Fact]
    public async Task Las_alertas_se_crean_una_vez_y_se_resuelven_solas()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await ClienteAsync(api, "Moroso SL", limite: 300m);
        var factura = await FacturaAsync(api, cliente, haceDias: 40);
        await FacturaAsync(api, cliente, haceDias: 5);

        var vencidas = await IdAsync(api, "/extensiones/reglas", new { Tipo = "FacturasVencidas", Nombre = "Vencidas +30", Dias = 30, PermisoDestino = "factura.leer" });
        var riesgo = await IdAsync(api, "/extensiones/reglas", new { Tipo = "RiesgoSuperado", Nombre = "Riesgo 80 %", Porcentaje = 80m });
        var itv = await IdAsync(api, "/extensiones/campos", new { Entidad = "cliente", Codigo = "itv", Tipo = "Fecha", Etiqueta = "ITV furgoneta" });
        var fecha = await IdAsync(api, "/extensiones/reglas", new { Tipo = "FechaCampo", Nombre = "ITV", CampoId = itv, Dias = 15 });
        (await api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}", new { Valores = new Dictionary<string, string?> { ["itv"] = Hoy.AddDays(10).ToString("yyyy-MM-dd") } }))
            .EnsureSuccessStatusCode();
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/reglas", new { Tipo = "FechaCampo", Nombre = "X", CampoId = Guid.NewGuid() }), HttpStatusCode.BadRequest))
            .Should().Be("alerta.campo");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/reglas", new { Tipo = "Evento", Nombre = "X", Evento = "Inventado" }), HttpStatusCode.BadRequest))
            .Should().Be("alerta.evento");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/reglas", new { Tipo = "Evento", Nombre = "X", Evento = "FacturaEmitida", PermisoDestino = "no.existe" }), HttpStatusCode.BadRequest))
            .Should().Be("alerta.permiso");

        // Al consultarlas se evalúan: una vencida (la de hace 40 días), riesgo 242 € ≥ 240 € (80 % de 300 €) y la ITV.
        var resumen = await OkAsync<ResumenResp>(api.GetAsync("/extensiones/alertas"));
        resumen.Vivas.Should().Be(3);
        resumen.SinLeer.Should().Be(3);
        resumen.Alertas.Single(a => a.ReglaId == vencidas).EntidadId.Should().Be(factura);
        resumen.Alertas.Single(a => a.ReglaId == riesgo).Titulo.Should().Contain("81 %");
        resumen.Alertas.Single(a => a.ReglaId == fecha).Titulo.Should().StartWith("ITV furgoneta: vence el");

        // Evaluar otra vez no repite nada.
        (await OkAsync<EvaluacionResp>(api.PostAsync(new Uri("/extensiones/alertas/evaluar", UriKind.Relative), null))).Should().Be(new EvaluacionResp(0, 0));

        // Leída y resuelta a mano.
        var a1 = resumen.Alertas.Single(a => a.ReglaId == fecha).Id;
        (await api.PostAsync(new Uri($"/extensiones/alertas/{a1}/leida", UriKind.Relative), null)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await OkAsync<ResumenResp>(api.GetAsync("/extensiones/alertas"))).SinLeer.Should().Be(2);

        // La ITV se renueva y se cobra la vencida: sus alertas se resuelven solas.
        (await api.PutAsJsonAsync($"/extensiones/valores/cliente/{cliente}", new { Valores = new Dictionary<string, string?> { ["itv"] = Hoy.AddYears(1).ToString("yyyy-MM-dd") } }))
            .EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/cobros", new { FacturaId = factura, Importe = 121m })).EnsureSuccessStatusCode();
        var tras = await OkAsync<EvaluacionResp>(api.PostAsync(new Uri("/extensiones/alertas/evaluar", UriKind.Relative), null));
        tras.Nuevas.Should().Be(0);
        tras.Resueltas.Should().Be(3, "el riesgo baja a 121 €, por debajo del 80 %: también se resuelve");
        (await OkAsync<ResumenResp>(api.GetAsync("/extensiones/alertas"))).Vivas.Should().Be(0);

        var historico = await OkAsync<ResumenResp>(api.GetAsync("/extensiones/alertas?incluirResueltas=true"));
        historico.Alertas.Should().HaveCount(3).And.OnlyContain(a => a.ResueltaEn != null);
        (await FalloAsync(api.PostAsync(new Uri($"/extensiones/alertas/{a1}/resolver", UriKind.Relative), null), HttpStatusCode.Conflict)).Should().Be("alerta.resuelta");

        // Una regla con alertas no se borra: se desactiva.
        (await FalloAsync(api.DeleteAsync($"/extensiones/reglas/{fecha}"), HttpStatusCode.Conflict)).Should().Be("alerta.regla_con_alertas");
        (await api.PutAsJsonAsync($"/extensiones/reglas/{fecha}", new { Tipo = "FechaCampo", Nombre = "ITV", CampoId = itv, Dias = 15, Activa = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        (await OkAsync<List<ReglaResp>>(api.GetAsync("/extensiones/reglas"))).Single(r => r.Id == fecha).Activa.Should().BeFalse();
        (await FalloAsync(api.DeleteAsync($"/extensiones/campos/{itv}"), HttpStatusCode.Conflict)).Should().Be("campo.con_valores");
    }

    [Fact]
    public async Task Una_regla_de_evento_avisa_cuando_ocurre()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var regla = await IdAsync(api, "/extensiones/reglas", new { Tipo = "Evento", Nombre = "Cliente nuevo", Evento = "ClienteCreado", PermisoDestino = "factura.leer" });
        var otra = await IdAsync(api, "/extensiones/reglas", new { Tipo = "Evento", Nombre = "Factura", Evento = "FacturaEmitida" });
        var sinAlertas = await IdAsync(api, "/extensiones/reglas", new { Tipo = "Evento", Nombre = "Gasto", Evento = "GastoRegistrado" });
        var cliente = await ClienteAsync(api, "Recién llegado SL");
        var factura = await FacturaAsync(api, cliente, 0);

        var resumen = await OkAsync<ResumenResp>(api.GetAsync("/extensiones/alertas"));
        resumen.Alertas.Single(a => a.ReglaId == regla).Should().Match<AlertaResp>(a => a.Entidad == "cliente" && a.EntidadId == cliente);
        resumen.Alertas.Single(a => a.ReglaId == otra).Should().Match<AlertaResp>(a => a.EntidadId == factura && a.Detalle == "Total 121,00 €");

        (await api.DeleteAsync($"/extensiones/reglas/{sinAlertas}")).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await OkAsync<List<ReglaResp>>(api.GetAsync("/extensiones/reglas"))).Should().HaveCount(2).And.Contain(r => r.Id == regla && r.Vivas == 1);
    }
}
