using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Fábrica que, además de generar los PDF de verdad, guarda lo que reciben los generadores (los textos y el idioma) y los
/// correos enviados, para comprobar en qué idioma sale cada documento sin tener que leer el PDF.
/// </summary>
public sealed class FabricaIdiomasPruebas : FabricaApiPruebas
{
    public ConcurrentQueue<(FacturaDto Factura, string? Idioma)> Facturas { get; } = new();

    public ConcurrentQueue<(PresupuestoDto Presupuesto, string? Idioma)> Presupuestos { get; } = new();

    public ConcurrentQueue<DocumentoImpreso> Documentos { get; } = new();

    public ConcurrentQueue<MensajeCorreo> Correos { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(s =>
        {
            Envolver<IGeneradorPdfFactura>(s, real => new CapturaFactura(real, Facturas));
            Envolver<IGeneradorPdfPresupuesto>(s, real => new CapturaPresupuesto(real, Presupuestos));
            Envolver<IGeneradorPdfDocumento>(s, real => new CapturaDocumento(real, Documentos));
            s.AddSingleton<IServicioCorreo>(new CapturaCorreo(Correos));
        });
    }

    private static void Envolver<T>(IServiceCollection s, Func<T, T> envoltorio)
        where T : class
    {
        var original = s.Last(d => d.ServiceType == typeof(T));
        s.Remove(original);
        s.AddScoped(sp => envoltorio((T)ActivatorUtilities.CreateInstance(sp, original.ImplementationType!)));
    }

    private sealed class CapturaFactura(IGeneradorPdfFactura real, ConcurrentQueue<(FacturaDto, string?)> cola) : IGeneradorPdfFactura
    {
        public byte[] Generar(FacturaDto factura, EmpresaDto emisor, string? idioma = null)
        {
            cola.Enqueue((factura, idioma));
            return real.Generar(factura, emisor, idioma);
        }
    }

    private sealed class CapturaPresupuesto(IGeneradorPdfPresupuesto real, ConcurrentQueue<(PresupuestoDto, string?)> cola) : IGeneradorPdfPresupuesto
    {
        public byte[] Generar(PresupuestoDto presupuesto, EmpresaDto emisor, string? idioma = null)
        {
            cola.Enqueue((presupuesto, idioma));
            return real.Generar(presupuesto, emisor, idioma);
        }
    }

    private sealed class CapturaDocumento(IGeneradorPdfDocumento real, ConcurrentQueue<DocumentoImpreso> cola) : IGeneradorPdfDocumento
    {
        public byte[] Generar(DocumentoImpreso documento, EmpresaDto emisor)
        {
            cola.Enqueue(documento);
            return real.Generar(documento, emisor);
        }
    }

    private sealed class CapturaCorreo(ConcurrentQueue<MensajeCorreo> cola) : IServicioCorreo
    {
        public Task EnviarAsync(MensajeCorreo mensaje, CancellationToken ct = default)
        {
            cola.Enqueue(mensaje);
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Documentos en el idioma del cliente o proveedor: textos fijos, nombre traducido de los artículos, mención fiscal con
/// su traducción, formato de los números y correo; ?idioma= lo cambia para un documento.
/// </summary>
public sealed class IdiomasDocumentosTests : IClassFixture<FabricaIdiomasPruebas>
{
    private readonly FabricaIdiomasPruebas _fabrica;

    public IdiomasDocumentosTests(FabricaIdiomasPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ClienteResp(Guid Id, string? Idioma);
    private sealed record TraduccionResp(string Idioma, string Nombre);
    private sealed record ProductoResp(Guid Id, List<TraduccionResp> Traducciones);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task PdfAsync(HttpClient api, string ruta)
    {
        var r = await api.GetAsync(new Uri(ruta, UriKind.Relative));
        r.StatusCode.Should().Be(HttpStatusCode.OK, ruta);
        Encoding.ASCII.GetString((await r.Content.ReadAsByteArrayAsync())[..4]).Should().Be("%PDF");
    }

    [Fact]
    public async Task La_factura_el_albaran_y_el_presupuesto_salen_en_el_idioma_del_cliente_con_el_articulo_traducido()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Mal", Idioma = "xx" })).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo
            .Should().Be("tercero.idioma");
        var ingles = await IdAsync(api, "/clientes", new { Nombre = "Fresh Fruit Ltd", Pais = "GB", Idioma = "EN" });
        (await api.GetFromJsonAsync<ClienteResp>($"/clientes/{ingles}"))!.Idioma.Should().Be("en");
        var local = await IdAsync(api, "/clientes", new { Nombre = "Frutas Valencia SL", Idioma = "es" });
        (await api.GetFromJsonAsync<ClienteResp>($"/clientes/{local}"))!.Idioma.Should().BeNull("el castellano es el de siempre");

        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 1.25m, Tipo = "Bien", Unidad = "kg" });
        (await (await api.PutAsJsonAsync($"/productos/{naranja}/traducciones", new[] { new { Idioma = "es", Nombre = "Naranja" } }))
            .Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("producto.idioma");
        var tr = await api.PutAsJsonAsync($"/productos/{naranja}/traducciones", new[] { new { Idioma = "en", Nombre = "Navel orange" }, new { Idioma = "fr", Nombre = "Orange Navel" } });
        tr.StatusCode.Should().Be(HttpStatusCode.OK, await tr.Content.ReadAsStringAsync());
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{naranja}"))!.Traducciones.Should().HaveCount(2);

        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode(); // siembra el catálogo (EXPORT)

        // Factura exenta (exportación) al cliente inglés: textos, artículo y mención traducidos; la línea escrita a mano, no.
        var factura = await IdAsync(api, "/facturas", new
        {
            ClienteId = ingles,
            Lineas = new object[]
            {
                new { ProductoId = naranja, Descripcion = "Naranja Navel calibre 3", Cantidad = 1_200m, PrecioUnitario = 1.25m, CodigoIva = "EXPORT" },
                new { Descripcion = "Palés retornables", Cantidad = 2m, PrecioUnitario = 10m, CodigoIva = "EXPORT" },
            },
        });
        _fabrica.Facturas.Clear();
        await PdfAsync(api, $"/facturas/{factura}/pdf");
        var (f, idioma) = _fabrica.Facturas.Last();
        idioma.Should().Be("en");
        f.Lineas.Select(l => l.Descripcion).Should().Equal("Navel orange calibre 3", "Palés retornables");
        TextosImpreso.MencionFiscal(idioma, f.MencionFiscal).Should().Contain("Operación exenta. Exportación de bienes").And.Contain("Export of goods");
        TextosImpreso.Numero(idioma, 1_510m).Should().Be("1,510.00");
        TextosImpreso.T(idioma, "Factura").Should().Be("Invoice");

        // Se puede pedir otro idioma para un documento concreto.
        await PdfAsync(api, $"/facturas/{factura}/pdf?idioma=fr");
        _fabrica.Facturas.Last().Factura.Lineas[0].Descripcion.Should().Be("Orange Navel calibre 3");
        await PdfAsync(api, $"/facturas/{factura}/pdf?idioma=de");
        _fabrica.Facturas.Last().Factura.Lineas[0].Descripcion.Should().Be("Naranja Navel calibre 3", "sin traducción al alemán, el nombre de siempre");

        _fabrica.Correos.Clear();
        (await api.PostAsJsonAsync($"/facturas/{factura}/enviar", new { Email = "buyer@freshfruit.co.uk" })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var correo = _fabrica.Correos.Single();
        correo.Asunto.Should().StartWith("Invoice ");
        correo.Cuerpo.Should().Be("Please find your invoice attached. Thank you for your business.");

        // Albarán (la descripción sale del artículo) y presupuesto.
        var albaran = await IdAsync(api, "/albaranes-venta", new { ClienteId = ingles, Lineas = new[] { new { ProductoId = naranja, Cantidad = 500m, PrecioUnitario = 1.2m } } });
        _fabrica.Documentos.Clear();
        await PdfAsync(api, $"/albaranes-venta/{albaran}/pdf");
        var doc = _fabrica.Documentos.Last();
        doc.Should().Match<DocumentoImpreso>(d => d.Titulo == "Delivery note" && d.Idioma == "en" && d.Tercero.Etiqueta == "Customer"
            && d.Leyenda == "Amounts exclusive of tax: VAT is applied on the invoice.");
        doc.Lineas.Single().Descripcion.Should().Be("Navel orange");
        doc.Totales.Single().Etiqueta.Should().Be("Net");

        var presupuesto = await IdAsync(api, "/presupuestos", new
        {
            ClienteId = ingles, Lineas = new[] { new { ProductoId = naranja, Descripcion = "Naranja Navel", Cantidad = 10m, PrecioUnitario = 1m, CodigoIva = "IVA21" } },
        });
        await PdfAsync(api, $"/presupuestos/{presupuesto}/pdf");
        _fabrica.Presupuestos.Last().Should().Match<(PresupuestoDto P, string? I)>(x => x.I == "en" && x.P.Lineas[0].Descripcion == "Navel orange");

        // El cliente nacional, en castellano y sin tocar las líneas.
        var albaranLocal = await IdAsync(api, "/albaranes-venta", new { ClienteId = local, Lineas = new[] { new { ProductoId = naranja, Cantidad = 5m, PrecioUnitario = 1.2m } } });
        await PdfAsync(api, $"/albaranes-venta/{albaranLocal}/pdf");
        _fabrica.Documentos.Last().Should().Match<DocumentoImpreso>(d => d.Titulo == "Albarán" && d.Lineas[0].Descripcion == "Naranja Navel" && d.Idioma == "es");
    }

    [Fact]
    public async Task El_pedido_de_compra_sale_en_el_idioma_del_proveedor()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Emballages du Sud SARL", Pais = "FR", Idioma = "fr" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja de cartón 10 kg", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud" });
        (await api.PutAsJsonAsync($"/productos/{caja}/traducciones", new[] { new { Idioma = "fr", Nombre = "Carton 10 kg" } })).EnsureSuccessStatusCode();
        var pedido = await IdAsync(api, "/compras/pedidos", new
        {
            ProveedorId = proveedor, Lineas = new[] { new { ProductoId = caja, Descripcion = "Caja de cartón 10 kg", Cantidad = 2_000m, PrecioUnitario = 0.45m } },
        });

        await PdfAsync(api, $"/compras/pedidos/{pedido}/pdf");
        var doc = _fabrica.Documentos.Last();
        doc.Titulo.Should().Be("Bon de commande");
        doc.Tercero.Etiqueta.Should().Be("Fournisseur");
        doc.Lineas.Single().Descripcion.Should().Be("Carton 10 kg");
        doc.Datos.Should().BeEmpty("el estado del pedido es un dato interno: solo sale en castellano");
        doc.Leyenda.Should().Be("Merci de bien vouloir accuser réception de cette commande.");
    }
}
