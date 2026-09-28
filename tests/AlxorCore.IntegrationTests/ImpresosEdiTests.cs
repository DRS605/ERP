using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// PDF de albarán, pedidos y liquidación al agricultor, y EDI EANCOM con la gran distribución: el ORDERS del cliente
/// entra como pedido de venta (cliente por GLN, artículo por GTIN, sin duplicados); el albarán sale como DESADV con el
/// SSCC del palé y el número de pedido del cliente; la factura, como INVOIC; el RECADV se contrasta con lo expedido.
/// </summary>
public sealed class ImpresosEdiTests : IClassFixture<FabricaApiPruebas>
{
    private const string GlnEmpresa = "8412345000010";
    private const string GlnCliente = "8412345000027";
    private const string GlnTienda = "8412345000034";
    private const string Gtin = "8437000000013";

    private readonly FabricaApiPruebas _fabrica;

    public ImpresosEdiTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record ParteResp(Guid Id);
    private sealed record PaleResp(Guid Id, string Sscc, Guid? AlbaranId);
    private sealed record ErrorResp(string Codigo);
    private sealed record ImportadoResp(Guid PedidoVentaId, string NumeroPedido, string NumeroCliente, int Lineas);
    private sealed record FacturaResp(Guid Id);
    private sealed record DiferenciaResp(string? Gtin, decimal Expedido, decimal Recibido, decimal Diferencia);
    private sealed record RecadvResp(bool Conforme, Guid? AlbaranId, List<DiferenciaResp> Lineas);
    private sealed record AlbaranResp(Guid Id, string NumeroCompleto);

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
        return (await r.Content.ReadFromJsonAsync<ErrorResp>())!.Codigo;
    }

    private static async Task PdfAsync(HttpClient api, string ruta)
    {
        var r = await api.GetAsync(new Uri(ruta, UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.OK, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        r.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        var bytes = await r.Content.ReadAsByteArrayAsync();
        System.Text.Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF", ruta);
    }

    private static async Task<string> TextoAsync(HttpClient api, string ruta)
    {
        var r = await api.GetAsync(new Uri(ruta, UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.OK, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return await r.Content.ReadAsStringAsync();
    }

    [Fact]
    public async Task El_pedido_llega_por_EDI_sale_con_DESADV_e_INVOIC_se_contrasta_el_RECADV_y_todo_se_imprime()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var anio = DateTime.UtcNow.Year;
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        // Configuración: GLN de la empresa (con dígito de control) y el cliente como socio EDI.
        (await CodigoAsync(await api.PutAsJsonAsync("/integraciones/edi/configuracion", new { GlnEmpresa = "8412345000011" }), HttpStatusCode.BadRequest)).Should().Be("edi.gln");
        (await api.PutAsJsonAsync("/integraciones/edi/configuracion", new { GlnEmpresa })).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Hipermercados del Sur SA", NifFiscal = Ayudas.GenerarNif() });
        await IdAsync(api, "/integraciones/edi/socios", new { ClienteId = cliente, GlnComprador = GlnCliente, GlnEntrega = GlnTienda });
        (await CodigoAsync(await api.PostAsJsonAsync("/integraciones/edi/socios", new { ClienteId = cliente, GlnComprador = GlnTienda }), HttpStatusCode.Conflict))
            .Should().Be("edi.socio_duplicado");

        // Producto con su GTIN como referencia y su fruta confeccionada en un palé.
        await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var extra = await IdAsync(api, "/productos", new { Referencia = Gtin, Nombre = "Tomate extra", PrecioUnitario = 1.20m, Tipo = "Bien", Unidad = "kg" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = hoy });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = hoy, PrecioEstimadoKg = 0.30m }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 1_100m, TaraKg = 100m })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null))).Lineas[0].PartidaId!.Value;
        var pale = await OkAsync<PaleResp>(await api.PostAsJsonAsync("/agro/pales", new { Tipo = "Europeo" }));
        var parte = await OkAsync<ParteResp>(await api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = hoy,
            Consumos = new[] { new { PartidaId = partida, Kilos = 1_000m } },
            Salidas = new object[] { new { ProductoId = extra, Kilos = 1_000m, Cajas = 100, PaleId = pale.Id } },
        }));
        (await api.PostAsync(new Uri($"/agro/partes/{parte.Id}/validar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();

        // ORDERS: pedido del cliente (GLN) con el artículo por GTIN; importarlo dos veces no duplica.
        var orders = $"UNA:+.? 'UNB+UNOC:3+{GlnCliente}:14+{GlnEmpresa}:14+{hoy:yyMMdd}:0800+ORD1'UNH+1+ORDERS:D:96A:UN:EAN008'BGM+220+PO-4711+9'"
            + $"DTM+137:{hoy:yyyyMMdd}:102'DTM+2:{hoy:yyyyMMdd}:102'NAD+BY+{GlnCliente}::9'NAD+DP+{GlnTienda}::9'"
            + $"LIN+1++{Gtin}:EN'IMD+F++:::Tomate extra ?+ calibre M'QTY+21:1000'PRI+AAA:1.15'UNS+S'CNT+2:1'UNT+12+1'UNZ+1+ORD1'";
        var importado = await OkAsync<ImportadoResp>(await api.PostAsJsonAsync("/integraciones/edi/orders", new { Contenido = orders }));
        importado.NumeroCliente.Should().Be("PO-4711");
        importado.Lineas.Should().Be(1);
        (await CodigoAsync(await api.PostAsJsonAsync("/integraciones/edi/orders", new { Contenido = orders }), HttpStatusCode.Conflict)).Should().Be("edi.pedido_duplicado");
        (await CodigoAsync(await api.PostAsJsonAsync("/integraciones/edi/orders", new { Contenido = orders.Replace(Gtin, "8437000000020", StringComparison.Ordinal).Replace("PO-4711", "PO-9", StringComparison.Ordinal) }),
            HttpStatusCode.BadRequest)).Should().Be("edi.articulo_desconocido");
        (await CodigoAsync(await api.PostAsJsonAsync("/integraciones/edi/orders", new { Contenido = "UNH+1+INVOIC:D:96A:UN'" }), HttpStatusCode.BadRequest)).Should().Be("edi.formato");

        // Se expide el palé con el pedido: el DESADV lleva el SSCC, el pedido del cliente y los GLN.
        var pedido = importado.PedidoVentaId;
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var expedidos = await OkAsync<List<PaleResp>>(await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, PedidoVentaId = pedido }));
        var albaran = expedidos.Single().AlbaranId!.Value;
        var desadv = await TextoAsync(api, $"/integraciones/edi/albaranes/{albaran}/desadv");
        desadv.Should().StartWith("UNA:+.? '").And.Contain("DESADV:D:96A:UN:EAN007").And.Contain("RFF+ON:PO-4711")
            .And.Contain($"NAD+DP+{GlnTienda}::9").And.Contain($"GIN+BJ+{pale.Sscc}").And.Contain($"LIN+1++{Gtin}:EN").And.Contain("QTY+12:1000");

        // Factura → INVOIC con el precio pactado en el ORDERS, el IVA y los totales.
        var factura = await OkAsync<FacturaResp>(await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaran } }));
        var invoic = await TextoAsync(api, $"/integraciones/edi/facturas/{factura.Id}/invoic");
        invoic.Should().Contain("INVOIC:D:96A:UN:EAN008").And.Contain("BGM+380+").And.Contain("RFF+ON:PO-4711").And.Contain($"NAD+SU+{GlnEmpresa}::9")
            .And.Contain($"LIN+1++{Gtin}:EN").And.Contain("QTY+47:1000").And.Contain("PRI+AAA:1.15").And.Contain("MOA+125:1150");

        // RECADV: el cliente recibe 980 kg de los 1.000 → no conforme, −20.
        var numeroAlbaran = (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{albaran}"))!.NumeroCompleto;
        var recadv = $"UNH+1+RECADV:D:96A:UN:EAN003'BGM+632+REC-1+9'RFF+AAK:{numeroAlbaran.Replace("/", "?/", StringComparison.Ordinal)}'RFF+ON:PO-4711'"
            + $"LIN+1++{Gtin}:EN'QTY+194:980'UNT+7+1'";
        var contraste = await OkAsync<RecadvResp>(await api.PostAsJsonAsync("/integraciones/edi/recadv", new { Contenido = recadv }));
        contraste.AlbaranId.Should().Be(albaran);
        contraste.Conforme.Should().BeFalse();
        contraste.Lineas.Should().ContainSingle().Which.Should().Match<DiferenciaResp>(l => l.Gtin == Gtin && l.Expedido == 1_000m && l.Recibido == 980m && l.Diferencia == -20m);

        // PDF: albarán (valorado y sin precios), pedidos de venta y de compra, y liquidación al agricultor.
        await PdfAsync(api, $"/albaranes-venta/{albaran}/pdf");
        await PdfAsync(api, $"/albaranes-venta/{albaran}/pdf?valorado=false");
        await PdfAsync(api, $"/pedidos-venta/{pedido}/pdf");
        var compra = await IdAsync(api, "/compras/pedidos", new { ProveedorTexto = "Envases Levante SL", Lineas = new[] { new { Descripcion = "Cajas", Cantidad = 100m, PrecioUnitario = 0.5m } } });
        await PdfAsync(api, $"/compras/pedidos/{compra}/pdf");
        var campana = (await api.GetFromJsonAsync<List<IdResp>>("/agro/campanas"))!.Single().Id;
        (await api.PutAsJsonAsync($"/agro/campanas/{campana}/articulos", new { ProductoId = tomate, Metodo = "PorPeriodo" })).EnsureSuccessStatusCode();
        await IdAsync(api, $"/agro/campanas/{campana}/precios", new { ProductoId = tomate, Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31), PrecioKg = 0.30m });
        var liquidacion = await IdAsync(api, "/agro/liquidaciones", new { AgricultorId = agricultor, CampanaId = campana, Desde = new DateOnly(anio, 1, 1), Hasta = hoy, Fecha = hoy });
        await PdfAsync(api, $"/agro/liquidaciones/{liquidacion}/pdf");
        (await api.GetAsync(new Uri($"/agro/liquidaciones/{Guid.NewGuid()}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
