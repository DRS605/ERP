using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Etiquetas por cliente o plataforma: plantilla con marca, campos, texto fijo y formato; referencia del artículo en el
/// cliente (código, descripción y GTIN); la general para el resto; y la etiqueta en ZPL para impresoras Zebra.
/// </summary>
public sealed class EtiquetasClienteTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public EtiquetasClienteTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);
    private const string Ean = "8412345678905";

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record UnidadResp(Guid Id, string Sscc);
    private sealed record PaletizadoResp(List<UnidadResp> Unidades);
    private sealed record PlantillaResp(Guid Id, List<string> Campos);

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

    [Fact]
    public async Task La_etiqueta_del_palé_sigue_la_plantilla_y_la_referencia_del_cliente()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = await IdAsync(api, "/productos", new { Nombre = "Tomate rama", PrecioUnitario = 1.2m, Tipo = "Bien", Unidad = "ud", CodigoIva = "IVA4" });
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "EXP", Nombre = "Expediciones" });
        var soporte = await IdAsync(api, "/logistica/soportes", new { Codigo = "EUR", Nombre = "Europalé", LargoMm = 1200, AnchoMm = 800, AltoMm = 144, TaraKg = 25m, CargaMaxKg = 1500m });
        (await api.PutAsJsonAsync($"/logistica/fichas/{producto}", new
        {
            Gtin = Ean, UnidadesPorCaja = 6, PesoNetoUnidadKg = 1m, PesoBrutoCajaKg = 6.5m, LargoCajaMm = 300, AnchoCajaMm = 200, AltoCajaMm = 250,
            CajasPorCapa = 8, Capas = 5, SoporteId = soporte, AlturaMaxPaleMm = 1600, PesoMaxPaleKg = 1000m,
        })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = almacen, Cantidad = 600m, Lote = "L-0510", FechaCaducidad = Hoy.AddDays(20) }))
            .EnsureSuccessStatusCode();
        var plataforma = await IdAsync(api, "/clientes", new { Nombre = "Plataforma Supermercados SA", NifFiscal = Ayudas.GenerarNif() });
        var otro = await IdAsync(api, "/clientes", new { Nombre = "Frutería Pepe", NifFiscal = Ayudas.GenerarNif() });

        var plantilla = await OkAsync<PlantillaResp>(api.PostAsJsonAsync("/extensiones/etiquetas/plantillas", new
        {
            Nombre = "Plataforma", ClienteId = plataforma, Marca = "Hacendado", Campos = new[] { "Producto", "Marca", "Lote", "Caducidad", "Cajas" },
            TextoLibre = "Categoría I · Origen España", Formato = "Rollo100x150",
        }));
        plantilla.Campos.Should().EndWith("TextoLibre", "el texto fijo se añade si no estaba en la lista");
        (await FalloAsync(api.PostAsJsonAsync("/extensiones/etiquetas/plantillas", new { Nombre = "Otra", ClienteId = plataforma }), HttpStatusCode.Conflict))
            .Should().Be("etiqueta.duplicada");
        await IdAsync(api, "/extensiones/etiquetas/plantillas", new { Nombre = "General", Marca = "Huerta del Sur", Campos = new[] { "Producto", "Marca", "PesoNeto" } });
        (await FalloAsync(api.PutAsJsonAsync("/extensiones/etiquetas/referencias", new { ClienteId = plataforma, ProductoId = producto, Codigo = "M-1", Gtin = "8412345678906" }),
            HttpStatusCode.BadRequest)).Should().Be("referencia.gtin");
        (await api.PutAsJsonAsync("/extensiones/etiquetas/referencias", new
        {
            ClienteId = plataforma, ProductoId = producto, Codigo = "MER-123", Descripcion = "Tomate rama 1 kg", Gtin = "8480000123459",
        })).EnsureSuccessStatusCode();

        var paleCliente = (await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar", new
        {
            ProductoId = producto, AlmacenId = almacen, Lote = "L-0510", SoloCompletos = true, ClienteId = plataforma, Cajas = 40,
        }))).Unidades[0];
        var zpl = await (await api.GetAsync($"/logistica/unidades/{paleCliente.Sscc}/etiqueta?formato=zpl")).Content.ReadAsStringAsync();
        zpl.Should().StartWith("^XA").And.Contain("^PW800").And.Contain("Marca: Hacendado").And.Contain("Ref. cliente: MER-123 · Tomate rama 1 kg")
            .And.Contain("Categoría I · Origen España").And.Contain($"(00){paleCliente.Sscc}").And.Contain("(02)08480000123459").And.EndWith("^XZ\n");
        var pdf = await api.GetAsync($"/logistica/unidades/{paleCliente.Sscc}/etiqueta");
        pdf.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await pdf.Content.ReadAsByteArrayAsync()).Should().StartWith("%PDF"u8.ToArray());

        // Otro cliente: la plantilla general, con el GTIN propio.
        var paleOtro = (await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar", new
        {
            ProductoId = producto, AlmacenId = almacen, Lote = "L-0510", SoloCompletos = true, ClienteId = otro, Cajas = 40,
        }))).Unidades[0];
        var zplOtro = await (await api.GetAsync($"/logistica/unidades/{paleOtro.Sscc}/etiqueta?formato=zpl")).Content.ReadAsStringAsync();
        zplOtro.Should().Contain("Marca: Huerta del Sur").And.Contain("(02)08412345678905").And.NotContain("Ref. cliente");

        // Sin plantillas activas, la etiqueta de siempre.
        (await api.PutAsJsonAsync($"/extensiones/etiquetas/plantillas/{plantilla.Id}", new { Nombre = "Plataforma", ClienteId = plataforma, Activa = false }))
            .EnsureSuccessStatusCode();
        var zplSinPlantilla = await (await api.GetAsync($"/logistica/unidades/{paleCliente.Sscc}/etiqueta?formato=zpl")).Content.ReadAsStringAsync();
        zplSinPlantilla.Should().Contain("Marca: Huerta del Sur", "queda la general").And.Contain("Ref. cliente: MER-123");
    }
}
