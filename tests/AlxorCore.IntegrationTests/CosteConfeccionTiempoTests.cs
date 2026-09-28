using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Coste de confección por tiempo teórico, como en Hispatec: la mano de obra de confección se reparte entre las salidas
/// por cajas × 3600 / rendimiento (cajas por hora del producto y envase) y el resto por kilos; sin rendimiento es un
/// error. El coste de las partidas del palé llega a la línea del albarán y a la factura como coste de la venta.
/// </summary>
public sealed class CosteConfeccionTiempoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CosteConfeccionTiempoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record ErrorResp(string Codigo, string Mensaje);
    private sealed record SalidaResp(int NumeroLinea, decimal Kilos, decimal Coste, decimal CosteKg, decimal? SegundosTeoricos, decimal CosteConfeccion);
    private sealed record ParteResp(Guid Id, decimal CosteTotal, decimal CosteManoObra, List<SalidaResp> Salidas, List<ErrorResp> Errores);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, Guid? AlbaranId);
    private sealed record LineaPedidoResp(Guid Id);
    private sealed record PedidoResp(Guid Id, List<LineaPedidoResp> Lineas);
    private sealed record LineaFacturaResp(string Descripcion, decimal Cantidad, decimal Base, decimal CosteUnitario, decimal Margen);
    private sealed record FacturaResp(Guid Id, List<LineaFacturaResp> Lineas);

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

    [Fact]
    public async Task La_mano_de_obra_de_confeccion_va_por_tiempo_teorico_y_el_coste_del_pale_llega_a_la_factura()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var anio = DateTime.UtcNow.Year;
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var extra = await IdAsync(api, "/productos", new { Nombre = "Tomate extra", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg", PrecioCompra = 0.10m });
        var segunda = await IdAsync(api, "/productos", new { Nombre = "Tomate 2ª", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja 10 kg", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud" });
        await IdAsync(api, "/agro/tarifas", new { Recurso = "ManoObra", Categoria = "PEON", TipoHora = "Normal", Desde = new DateOnly(anio, 1, 1), CosteUnitario = 12m });

        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = hoy });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = hoy, PrecioEstimadoKg = 0.30m }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 2_100m, TaraKg = 100m })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null))).Lineas[0].PartidaId!.Value;
        var pale = await OkAsync<PaleResp>(await api.PostAsJsonAsync("/agro/pales", new { Tipo = "Europeo" }));

        // Rendimiento del extra en su caja; del 2ª aún no hay.
        (await api.PutAsJsonAsync("/agro/rendimientos", new { ProductoId = extra, EnvaseProductoId = caja, CajasHora = 100m })).EnsureSuccessStatusCode();

        // Fruta 2.000 kg × 0,30 = 600; confección 10 h × 12 = 120 (por tiempo); apoyo 5 h × 12 = 60 (por kilos).
        var parte = await OkAsync<ParteResp>(await api.PostAsJsonAsync("/agro/partes", new
        {
            Fecha = hoy, Reparto = "PorTiempoTeorico",
            Consumos = new[] { new { PartidaId = partida, Kilos = 2_000m } },
            ManoObra = new object[]
            {
                new { Descripcion = "Encajado", Categoria = "PEON", TipoHora = "Normal", Horas = 10m, Confeccion = true },
                new { Descripcion = "Carretilla", Categoria = "PEON", TipoHora = "Normal", Horas = 5m, Confeccion = false },
            },
            Salidas = new object[]
            {
                new { ProductoId = extra, Kilos = 1_200m, Cajas = 120, EnvaseProductoId = caja, PaleId = pale.Id },
                new { ProductoId = segunda, Kilos = 600m, Cajas = 60 },
            },
        }));
        var valoracion = await OkAsync<ParteResp>(await api.PostAsync(new Uri($"/agro/partes/{parte.Id}/valorar", UriKind.Relative), null));
        valoracion.Errores.Should().ContainSingle(e => e.Codigo == "parte.sin_rendimiento");

        // 2ª: 50 cajas/h. Teórico: 120 × 36 = 4.320 s y 60 × 72 = 4.320 s → la confección, a medias (60 y 60).
        (await api.PutAsJsonAsync("/agro/rendimientos", new { ProductoId = segunda, CajasHora = 50m })).EnsureSuccessStatusCode();
        var validado = await OkAsync<ParteResp>(await api.PostAsync(new Uri($"/agro/partes/{parte.Id}/validar", UriKind.Relative), null));
        validado.CosteTotal.Should().Be(780m);
        var s1 = validado.Salidas.Single(s => s.NumeroLinea == 1);
        var s2 = validado.Salidas.Single(s => s.NumeroLinea == 2);
        s1.Should().Match<SalidaResp>(s => s.SegundosTeoricos == 4_320m && s.CosteConfeccion == 60m && s.Coste == 500m, "660 × 1.200 / 1.800 = 440 + 60");
        s2.Should().Match<SalidaResp>(s => s.SegundosTeoricos == 4_320m && s.CosteConfeccion == 60m && s.Coste == 280m, "660 × 600 / 1.800 = 220 + 60");

        // El palé del extra se vende: su coste por kilo (500 / 1.200) llega al albarán y a la factura.
        (await api.PostAsync(new Uri($"/agro/pales/{pale.Id}/cerrar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        var pedido = await OkAsync<PedidoResp>(await api.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = cliente, Lineas = new[] { new { ProductoId = extra, Descripcion = "Tomate extra", Cantidad = 1_200m, PrecioUnitario = 1.10m, CodigoIva = "IVA4" } },
        }));
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var expedidos = await OkAsync<List<PaleResp>>(await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pale.Id }, PedidoVentaId = pedido.Id }));
        var albaran = expedidos.Single().AlbaranId!.Value;
        var factura = await OkAsync<FacturaResp>(await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { albaran } }));
        factura.Lineas.Single().Should().Match<LineaFacturaResp>(l => l.CosteUnitario == 0.4167m && l.Cantidad == 1_200m && l.Margen == 819.96m,
            "el coste real del palé, no el precio de compra del artículo (0,10)");
    }
}
