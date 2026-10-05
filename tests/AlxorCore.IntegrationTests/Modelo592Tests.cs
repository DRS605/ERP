using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Modelo 592 (envases de plástico no reutilizables): fichas de plástico, adquisiciones intracomunitarias, fabricación,
/// envíos fuera de España deducibles, exentos y cuota a 0,45 €/kg de plástico no reciclado, con su libro registro.
/// </summary>
public sealed class Modelo592Tests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public Modelo592Tests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaPedResp(Guid Id);
    private sealed record PedidoResp(Guid Id, List<LineaPedResp> Lineas);
    private sealed record FichaResp(Guid ProductoId, string Clave, decimal KgNoRecicladoPorUnidad);
    private sealed record ApunteResp(string Concepto, string Clave, decimal Unidades, decimal KgNoReciclado, bool Exento);
    private sealed record ClaveResp(string Clave, decimal KgAdquisiciones, decimal KgExentos);
    private sealed record ModeloResp(decimal KgAdquisiciones, decimal KgFabricacion, decimal KgDeducibles, decimal KgExentos, decimal BaseKg, decimal Cuota,
        List<ClaveResp> PorClave, List<ApunteResp> Libro, List<string> Avisos);

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

    private static async Task CompraAsync(HttpClient api, Guid proveedor, Guid producto, decimal cantidad)
    {
        var pedido = await OkAsync<PedidoResp>(api.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor, Lineas = new[] { new { ProductoId = producto, Descripcion = "Envase", Cantidad = cantidad, PrecioUnitario = 0.05m } },
        }));
        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new { Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = cantidad } } }))
            .EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Adquisiciones_fabricacion_envios_y_exentos_dan_la_base_y_la_cuota()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var bandeja = await IdAsync(api, "/productos", new { Nombre = "Bandeja PET 1 kg", PrecioUnitario = 0.1m, Tipo = "Bien", Unidad = "ud" });
        var film = await IdAsync(api, "/productos", new { Nombre = "Film sanitario", PrecioUnitario = 0.1m, Tipo = "Bien", Unidad = "ud" });
        var granza = await IdAsync(api, "/productos", new { Nombre = "Granza PET", PrecioUnitario = 0.02m, Tipo = "Bien", Unidad = "ud" });

        (await FalloAsync(api.PutAsJsonAsync($"/informes/plastico/fichas/{bandeja}", new { Clave = "Envase", KgPorUnidad = 0.02m, KgRecicladoPorUnidad = 0.03m }),
            HttpStatusCode.BadRequest)).Should().Be("plastico.reciclado");
        (await FalloAsync(api.PutAsJsonAsync($"/informes/plastico/fichas/{film}", new { Clave = "Cierre", KgPorUnidad = 0.01m, Exento = true }), HttpStatusCode.BadRequest))
            .Should().Be("plastico.motivo");
        (await OkAsync<FichaResp>(api.PutAsJsonAsync($"/informes/plastico/fichas/{bandeja}", new { Clave = "Envase", KgPorUnidad = 0.02m, KgRecicladoPorUnidad = 0.005m })))
            .KgNoRecicladoPorUnidad.Should().Be(0.015m);
        await OkAsync<FichaResp>(api.PutAsJsonAsync($"/informes/plastico/fichas/{film}", new { Clave = "Cierre", KgPorUnidad = 0.01m, Exento = true, MotivoExencion = "Uso sanitario" }));

        var portugues = await IdAsync(api, "/proveedores", new { Nombre = "Embalagens Lda", Pais = "PT", NifIva = "PT123456789" });
        var local = await IdAsync(api, "/proveedores", new { Nombre = "Envases Murcia SL", Pais = "ES", NifFiscal = Ayudas.GenerarNif() });
        await CompraAsync(api, portugues, bandeja, 1000m);  // 15 kg sujetos
        await CompraAsync(api, portugues, film, 100m);      // 1 kg exento
        await CompraAsync(api, local, bandeja, 1000m);      // nacional: ya lleva el impuesto repercutido

        // Fabricación de 200 bandejas: 3 kg.
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "A1", Nombre = "Central" });
        (await api.PutAsJsonAsync($"/productos/{bandeja}/composicion", new { Componentes = new[] { new { ComponenteId = granza, Cantidad = 1m } } })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = granza, AlmacenId = almacen, Cantidad = 500m })).EnsureSuccessStatusCode();
        var orden = await IdAsync(api, "/produccion/ordenes", new { ProductoId = bandeja, Cantidad = 200m, AlmacenId = almacen });
        (await api.PostAsJsonAsync($"/produccion/ordenes/{orden}/terminar", new { Lote = "B-1" })).EnsureSuccessStatusCode();

        // Venta de 100 bandejas a Francia: 1,5 kg deducibles.
        var frances = await IdAsync(api, "/clientes", new { Nombre = "Primeurs SARL", Pais = "FR", NifIva = "FR12345678901" });
        (await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = frances, Lineas = new[] { new { Cantidad = 100m, Descripcion = "Bandeja", PrecioUnitario = 0.2m, CodigoIva = "IVA0", ProductoId = bandeja } },
        })).EnsureSuccessStatusCode();

        var m = await OkAsync<ModeloResp>(api.GetAsync($"/informes/modelo-592?anio={Hoy.Year}&periodo={Hoy.Month:00}"));
        m.KgAdquisiciones.Should().Be(15m);
        m.KgFabricacion.Should().Be(3m);
        m.KgDeducibles.Should().Be(1.5m);
        m.KgExentos.Should().Be(1m);
        m.BaseKg.Should().Be(16.5m);
        m.Cuota.Should().Be(7.43m);
        m.Libro.Should().HaveCount(4);
        m.PorClave.Single(c => c.Clave == "C").KgExentos.Should().Be(1m);

        var csv = await (await api.GetAsync($"/informes/modelo-592?anio={Hoy.Year}&periodo={Hoy.Month:00}&formato=csv")).Content.ReadAsStringAsync();
        csv.Should().Contain("AdquisicionIntracomunitaria").And.Contain("Embalagens Lda").And.Contain("15");
        var trimestre = $"{((Hoy.Month - 1) / 3) + 1}T";
        (await OkAsync<ModeloResp>(api.GetAsync($"/informes/modelo-592?anio={Hoy.Year}&periodo={trimestre}"))).BaseKg.Should().Be(16.5m);
        (await FalloAsync(api.GetAsync($"/informes/modelo-592?anio={Hoy.Year}&periodo=5T"), HttpStatusCode.BadRequest)).Should().Be("m592.periodo");
    }

    [Fact]
    public async Task Adquisiciones_de_hasta_5_kg_en_el_mes_no_estan_sujetas()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var bolsa = await IdAsync(api, "/productos", new { Nombre = "Bolsa malla", PrecioUnitario = 0.1m, Tipo = "Bien", Unidad = "ud" });
        await OkAsync<FichaResp>(api.PutAsJsonAsync($"/informes/plastico/fichas/{bolsa}", new { Clave = "Envase", KgPorUnidad = 0.004m }));
        var italiano = await IdAsync(api, "/proveedores", new { Nombre = "Plastica SRL", Pais = "IT", NifIva = "IT12345678901" });
        await CompraAsync(api, italiano, bolsa, 1000m); // 4 kg
        var m = await OkAsync<ModeloResp>(api.GetAsync($"/informes/modelo-592?anio={Hoy.Year}&periodo={Hoy.Month:00}"));
        m.KgAdquisiciones.Should().Be(0m);
        m.Cuota.Should().Be(0m);
        m.Avisos.Should().Contain(a => a.Contains("no sujetas", StringComparison.Ordinal));
    }
}
