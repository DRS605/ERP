using System.Net;
using System.Net.Http.Json;
using AlxorCore.Nucleo.Comun;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Logística: fichas, soportes, plantillas, cálculo de palés, paletizado FEFO y de fabricación, ciclo de unidades, lector GS1 y etiqueta.</summary>
public sealed class LogisticaEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public LogisticaEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private const string Ean = "8412345678905";
    private const string Gtin = "08412345678905";

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid ProductoId, string? Lote, DateOnly? FechaCaducidad, int Cajas, decimal Unidades, decimal PesoNetoKg);
    private sealed record UnidadResp(Guid Id, string Sscc, string Estado, string Origen, Guid AlmacenId, Guid? ClienteId, Guid? PadreId, Guid? OrdenFabricacionId, int Cajas,
        int? CajasCompleta, decimal PesoBrutoKg, int? AlturaMm, List<LineaResp> Contenido, List<string> Hijas);
    private sealed record PaletizadoResp(List<UnidadResp> Unidades, int Cajas, int Completas, int Abiertas, List<string> Avisos);
    private sealed record PaleResp(int Cajas, decimal PesoBrutoKg, int? AlturaMm);
    private sealed record CalculoResp(int CajasTotales, int CajasPorPale, int PalesCompletos, PaleResp? Pico, int Pales, List<string> Avisos);
    private sealed record CalculoDtoResp(string OrigenMosaico, int CajasPorCapa, int Capas, string? Soporte, CalculoResp Calculo);
    private sealed record CalculoPedidoResp(int Pales, int PalesCompletos, int Picos);
    private sealed record LecturaResp(string Tipo, UnidadResp? Unidad, Guid? ProductoId, string? Lote, DateOnly? FechaCaducidad, int? Cantidad);
    private sealed record PedidoResp(Guid Id);

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> FalloAsync(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    /// <summary>Artículo con ficha (6 ud por caja, 8×5 en europalé) y existencias de dos lotes en el almacén.</summary>
    private async Task<(HttpClient Api, Guid Producto, Guid Almacen, Guid Soporte)> EscenarioAsync()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var producto = await IdAsync(api, "/productos", new { Nombre = "Zumo 1 L", PrecioUnitario = 1.2m, Tipo = "Bien", Unidad = "ud", CodigoIva = "IVA10" });
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "EXP", Nombre = "Expediciones" });
        var soporte = await IdAsync(api, "/logistica/soportes", new { Codigo = "EUR", Nombre = "Europalé", LargoMm = 1200, AnchoMm = 800, AltoMm = 144, TaraKg = 25m, CargaMaxKg = 1500m });
        (await api.PutAsJsonAsync($"/logistica/fichas/{producto}", new
        {
            Gtin = Ean, GtinCaja = (string?)null, UnidadesPorCaja = 6, PesoNetoUnidadKg = 1.05m, PesoBrutoCajaKg = 6.6m, LargoCajaMm = 300, AnchoCajaMm = 200, AltoCajaMm = 250,
            CajasPorCapa = 8, Capas = 5, SoporteId = soporte, AlturaMaxPaleMm = 1600, PesoMaxPaleKg = 1000m, VidaMinimaEntregaDias = 30,
        })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = almacen, Cantidad = 300m, Lote = "L-TARDE", FechaCaducidad = Hoy.AddDays(200) }))
            .EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = producto, AlmacenId = almacen, Cantidad = 300m, Lote = "L-PRONTO", FechaCaducidad = Hoy.AddDays(20) }))
            .EnsureSuccessStatusCode();
        return (api, producto, almacen, soporte);
    }

    [Fact]
    public async Task Calcula_los_palés_con_la_ficha_y_con_la_plantilla_del_cliente()
    {
        var (api, producto, _, soporte) = await EscenarioAsync();

        var c = await OkAsync<CalculoDtoResp>(api.GetAsync(new Uri($"/logistica/calculo?productoId={producto}&unidades=570", UriKind.Relative)));
        c.OrigenMosaico.Should().Be("Ficha logística");
        c.Soporte.Should().Be("Europalé");
        c.Calculo.CajasTotales.Should().Be(95);
        c.Calculo.CajasPorPale.Should().Be(40);
        c.Calculo.PalesCompletos.Should().Be(2);
        c.Calculo.Pico!.Cajas.Should().Be(15);
        c.Calculo.Pico.AlturaMm.Should().Be(144 + 2 * 250);
        c.Calculo.Avisos.Should().BeEmpty();

        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Hipermercados Norte SA" });
        await IdAsync(api, "/logistica/plantillas", new { ProductoId = producto, ClienteId = cliente, SoporteId = soporte, CajasPorCapa = 8, Capas = 4, AlturaMaxMm = 1200 });
        var pc = await OkAsync<CalculoDtoResp>(api.GetAsync(new Uri($"/logistica/calculo?productoId={producto}&cajas=95&clienteId={cliente}", UriKind.Relative)));
        pc.OrigenMosaico.Should().NotBe("Ficha logística");
        pc.Calculo.CajasPorPale.Should().Be(32);
        pc.Calculo.Pales.Should().Be(3);

        var pedido = (await (await api.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { ProductoId = producto, Descripcion = "Zumo 1 L", Cantidad = 600m, PrecioUnitario = 1.2m, CodigoIva = "IVA10" } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;
        (await api.PostAsync(new Uri($"/pedidos-venta/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var pp = await OkAsync<CalculoPedidoResp>(api.GetAsync(new Uri($"/logistica/pedidos/{pedido.Id}/pales", UriKind.Relative)));
        pp.Pales.Should().Be(4);
        pp.PalesCompletos.Should().Be(3);
        pp.Picos.Should().Be(1);
    }

    [Fact]
    public async Task Paletiza_por_caducidad_sin_mezclar_lotes_y_no_vuelve_a_paletizar_lo_montado()
    {
        var (api, producto, almacen, _) = await EscenarioAsync();

        var r = await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar", new { ProductoId = producto, AlmacenId = almacen, Cajas = 100 }));
        r.Cajas.Should().Be(100);
        r.Unidades.Should().HaveCount(4);
        r.Completas.Should().Be(2);
        r.Abiertas.Should().Be(2);
        r.Unidades[0].Contenido.Should().ContainSingle().Which.Lote.Should().Be("L-PRONTO");
        r.Unidades[0].Estado.Should().Be("Cerrada");
        r.Unidades[0].PesoBrutoKg.Should().Be(40 * 6.6m + 25m);
        r.Unidades[1].Cajas.Should().Be(10);
        r.Unidades[1].Estado.Should().Be("Abierta");
        r.Unidades[2].Contenido.Single().Lote.Should().Be("L-TARDE");
        r.Unidades.Should().OnlyContain(u => u.Contenido.Count == 1 && Gs1.EsSsccValido(u.Sscc));
        r.Avisos.Should().Contain(a => a.Contains("días de vida", StringComparison.Ordinal));

        (await FalloAsync(api.PostAsJsonAsync("/logistica/paletizar", new { ProductoId = producto, AlmacenId = almacen, Cajas = 1 }))).Should().Be("paletizado.sin_existencias");

        // Anular un palé libera su contenido; poner cajas en el pico lo completa y lo cierra.
        var pico = r.Unidades[1];
        (await FalloAsync(api.PostAsJsonAsync($"/logistica/unidades/{pico.Id}/contenido", new { ProductoId = producto, Lote = "L-PRONTO", Cajas = 30 })))
            .Should().Be("unidad.sin_existencias");
        var anulada = await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{r.Unidades[3].Id}/anular", new { Motivo = "Se rehace" }));
        anulada.Estado.Should().Be("Anulada");
        var lleno = await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{r.Unidades[2].Id}/abrir", new { }));
        lleno.Estado.Should().Be("Abierta");
        (await FalloAsync(api.PostAsJsonAsync($"/logistica/unidades/{lleno.Id}/contenido", new { ProductoId = producto, Lote = "L-TARDE", Cajas = 1 }))).Should().Be("unidad.llena");
        var picoTarde = await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{pico.Id}/contenido", new { ProductoId = producto, Lote = "L-TARDE", Cajas = 5 }));
        picoTarde.Cajas.Should().Be(15);
        picoTarde.Contenido.Should().HaveCount(2);

        var libres = await OkAsync<List<UnidadResp>>(api.GetAsync(new Uri($"/logistica/unidades?estado=Abierta&productoId={producto}", UriKind.Relative)));
        libres.Should().HaveCount(2);

        // Caja con SSCC dentro de un palé mixto: el palé suma su peso.
        var caja = await IdAsync(api, "/logistica/unidades", new { AlmacenId = almacen, Tipo = "Caja" });
        await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{caja}/contenido", new { ProductoId = producto, Lote = "L-TARDE", Unidades = 6m }));
        await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{caja}/cerrar", new { }));
        var dentro = await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{caja}/meter", new { PadreSscc = "00" + picoTarde.Sscc }));
        dentro.PadreId.Should().Be(pico.Id);
        var padre = await OkAsync<UnidadResp>(api.GetAsync(new Uri($"/logistica/unidades/{picoTarde.Sscc}", UriKind.Relative)));
        padre.Hijas.Should().ContainSingle();
        padre.PesoBrutoKg.Should().BeGreaterThan(picoTarde.PesoBrutoKg);

        // Cerrar y expedir el palé expide también la caja que lleva dentro.
        await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{pico.Id}/cerrar", new { }));
        var expedida = await OkAsync<UnidadResp>(api.PostAsJsonAsync($"/logistica/unidades/{pico.Id}/expedir", new { Fecha = Hoy, Referencia = "ALB-1" }));
        expedida.Estado.Should().Be("Expedida");
        (await OkAsync<UnidadResp>(api.GetAsync(new Uri($"/logistica/unidades/{caja}", UriKind.Relative)))).Estado.Should().Be("Expedida");
    }

    [Fact]
    public async Task Lee_lo_que_envía_la_pistola_y_saca_la_etiqueta()
    {
        var (api, producto, almacen, _) = await EscenarioAsync();
        var r = await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar", new { ProductoId = producto, AlmacenId = almacen, Lote = "L-TARDE", SoloCompletos = true }));
        r.Unidades.Should().ContainSingle();
        var pale = r.Unidades[0];

        var porSscc = await OkAsync<LecturaResp>(api.GetAsync(new Uri($"/logistica/lectura?codigo={Uri.EscapeDataString("]C100" + pale.Sscc)}", UriKind.Relative)));
        porSscc.Tipo.Should().Be("Unidad");
        porSscc.Unidad!.Id.Should().Be(pale.Id);

        var codigo = $"01{Gtin}10L-TARDE\u001d17{Gs1.Aammdd(Hoy.AddDays(200))}";
        var articulo = await OkAsync<LecturaResp>(api.GetAsync(new Uri($"/logistica/lectura?codigo={Uri.EscapeDataString(codigo)}", UriKind.Relative)));
        articulo.Tipo.Should().Be("Articulo");
        articulo.ProductoId.Should().Be(producto);
        articulo.Lote.Should().Be("L-TARDE");
        articulo.FechaCaducidad.Should().Be(Hoy.AddDays(200));

        (await OkAsync<LecturaResp>(api.GetAsync(new Uri("/logistica/lectura?codigo=hola", UriKind.Relative)))).Tipo.Should().Be("Desconocido");

        var etiqueta = await api.GetAsync(new Uri($"/logistica/unidades/{pale.Sscc}/etiqueta", UriKind.Relative));
        etiqueta.StatusCode.Should().Be(HttpStatusCode.OK, await etiqueta.Content.ReadAsStringAsync());
        etiqueta.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await etiqueta.Content.ReadAsByteArrayAsync()).Should().StartWith("%PDF"u8.ToArray());
    }

    [Fact]
    public async Task Paletiza_lo_fabricado_en_una_orden_con_su_lote()
    {
        var (api, _, almacen, soporte) = await EscenarioAsync();
        var botella = await IdAsync(api, "/productos", new { Nombre = "Botella vacía", PrecioUnitario = 0.1m, Tipo = "Bien", Unidad = "ud" });
        var refresco = await IdAsync(api, "/productos", new { Nombre = "Refresco 33 cl", PrecioUnitario = 0.8m, Tipo = "Bien", Unidad = "ud" });
        (await api.PutAsJsonAsync($"/productos/{refresco}/composicion", new { Componentes = new[] { new { ComponenteId = botella, Cantidad = 1m } } })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = botella, AlmacenId = almacen, Cantidad = 1000m })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync($"/logistica/fichas/{refresco}", new
        {
            UnidadesPorCaja = 24, PesoNetoUnidadKg = 0.35m, PesoBrutoCajaKg = 8.8m, AltoCajaMm = 130, CajasPorCapa = 10, Capas = 8, SoporteId = soporte,
        })).EnsureSuccessStatusCode();

        var orden = await IdAsync(api, "/produccion/ordenes", new { ProductoId = refresco, Cantidad = 480m, AlmacenId = almacen });
        (await FalloAsync(api.PostAsJsonAsync("/logistica/paletizar-fabricacion", new { OrdenFabricacionId = orden, AlmacenId = almacen })))
            .Should().Be("paletizado.orden_no_terminada");
        (await api.PostAsJsonAsync($"/produccion/ordenes/{orden}/terminar", new { Lote = "F-0929", FechaCaducidad = Hoy.AddDays(365) })).EnsureSuccessStatusCode();

        var r = await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar-fabricacion", new { OrdenFabricacionId = orden, AlmacenId = almacen }));
        r.Cajas.Should().Be(20);
        var pale = r.Unidades.Should().ContainSingle().Subject;
        pale.OrdenFabricacionId.Should().Be(orden);
        pale.Origen.Should().Be("Fabricacion");
        pale.Contenido.Single().Lote.Should().Be("F-0929");
        pale.Contenido.Single().FechaCaducidad.Should().Be(Hoy.AddDays(365));

        (await FalloAsync(api.PostAsJsonAsync("/logistica/paletizar-fabricacion", new { OrdenFabricacionId = orden, AlmacenId = almacen })))
            .Should().Be("paletizado.fabricacion_completa");
        var deLaOrden = await OkAsync<List<UnidadResp>>(api.GetAsync(new Uri($"/logistica/unidades?ordenFabricacionId={orden}", UriKind.Relative)));
        deLaOrden.Should().ContainSingle();
    }

    [Fact]
    public async Task Maestros_validan_y_se_dan_de_baja_si_se_usaron()
    {
        var (api, producto, almacen, soporte) = await EscenarioAsync();
        (await api.PutAsJsonAsync($"/logistica/fichas/{producto}", new { Gtin = "8412345678904", UnidadesPorCaja = 6 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await api.PostAsJsonAsync("/logistica/soportes", new { Codigo = "EUR", Nombre = "Otro", LargoMm = 1200, AnchoMm = 800, AltoMm = 144, TaraKg = 25m }))
            .IsSuccessStatusCode.Should().BeFalse();

        var libre = await IdAsync(api, "/logistica/soportes", new { Codigo = "MED", Nombre = "Medio palé", LargoMm = 800, AnchoMm = 600, AltoMm = 144, TaraKg = 10m });
        (await OkAsync<EliminadoResp>(api.DeleteAsync(new Uri($"/logistica/soportes/{libre}", UriKind.Relative)))).Eliminado.Should().BeTrue();

        await OkAsync<PaletizadoResp>(api.PostAsJsonAsync("/logistica/paletizar", new { ProductoId = producto, AlmacenId = almacen, Cajas = 5 }));
        (await OkAsync<EliminadoResp>(api.DeleteAsync(new Uri($"/logistica/soportes/{soporte}", UriKind.Relative)))).Eliminado.Should().BeFalse();

        var config = await OkAsync<ConfigResp>(api.GetAsync(new Uri("/logistica/configuracion", UriKind.Relative)));
        config.UltimaSerie.Should().Be(1);
        (await api.PutAsJsonAsync("/logistica/configuracion", new { PrefijoGs1 = "84ABC", DigitoExtension = 0 })).IsSuccessStatusCode.Should().BeFalse();
        (await api.PutAsJsonAsync("/logistica/configuracion", new { PrefijoGs1 = "84123456", DigitoExtension = 1, MezclarLotes = true })).EnsureSuccessStatusCode();
    }

    private sealed record EliminadoResp(bool Eliminado);
    private sealed record ConfigResp(string PrefijoGs1, long UltimaSerie);
}
