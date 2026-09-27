using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Artículos compuestos: árbol de varios niveles con coste y peso calculados, control de ciclos, dónde se usa un
/// artículo, búsqueda de un compuesto con la misma lista, y kits de venta (precio según componentes y descuento de
/// las existencias de sus componentes al venderlos).
/// </summary>
public sealed class CompuestosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CompuestosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record ProductoResp(Guid Id, string Nombre, decimal PrecioUnitario, decimal Stock, string TipoComposicion, decimal? PesoKg);
    private sealed record ComponenteResp(Guid ComponenteId, string Nombre, decimal Cantidad, decimal CosteUnitario, decimal CosteLinea, bool EsCompuesto, List<ComponenteResp>? Componentes);
    private sealed record NecesidadResp(Guid ProductoId, string Nombre, decimal Cantidad);
    private sealed record ComposicionResp(bool EsCompuesto, decimal CosteTotal, List<ComponenteResp> Componentes, string TipoComposicion, decimal PrecioComponentes,
        decimal? PrecioCalculado, decimal? PesoKg, int Niveles, List<NecesidadResp> Explosion, decimal? DisponibleKit, List<string> CompuestosIguales);
    private sealed record UsoResp(Guid ProductoId, string Nombre, int Nivel, decimal CantidadPorUnidad);
    private sealed record IgualResp(Guid Id, string Nombre);

    private static async Task<Guid> ProductoAsync(HttpClient api, string nombre, decimal precio, decimal coste, decimal? peso = null, decimal? stock = null)
    {
        var r = await api.PostAsJsonAsync("/productos", new
        {
            Nombre = nombre, PrecioUnitario = precio, PrecioCompra = coste, Tipo = "Bien", Unidad = "ud", PesoKg = peso,
            ControlarStock = stock is not null, StockInicial = stock ?? 0m,
        });
        r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<HttpResponseMessage> ComponerAsync(HttpClient api, Guid id, object componentes, string tipo = "Fabricacion", bool precioSegun = false, decimal ajuste = 0m) =>
        await api.PutAsJsonAsync($"/productos/{id}/composicion", new { Componentes = componentes, Tipo = tipo, PrecioSegunComponentes = precioSegun, AjustePrecio = ajuste });

    [Fact]
    public async Task Un_compuesto_de_varios_niveles_calcula_coste_y_peso_y_no_admite_ciclos()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var caja = await ProductoAsync(api, "Caja cartón", 0.5m, 0.30m, peso: 0.2m);
        var etiqueta = await ProductoAsync(api, "Etiqueta", 0.05m, 0.02m, peso: 0.001m);
        var naranja = await ProductoAsync(api, "Naranja granel", 1.5m, 0.80m, peso: 1m);
        var cajaEtiquetada = await ProductoAsync(api, "Caja etiquetada", 0m, 0m);
        var cajaNaranja = await ProductoAsync(api, "Caja naranja 10 kg", 0m, 0m);

        (await ComponerAsync(api, cajaEtiquetada, new[] { new { ComponenteId = caja, Cantidad = 1m }, new { ComponenteId = etiqueta, Cantidad = 1m } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var r = await ComponerAsync(api, cajaNaranja, new[] { new { ComponenteId = cajaEtiquetada, Cantidad = 1m }, new { ComponenteId = naranja, Cantidad = 10m } });
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        var c = (await r.Content.ReadFromJsonAsync<ComposicionResp>())!;

        c.Niveles.Should().Be(2);
        c.CosteTotal.Should().Be(0.32m + 8m, "el coste de la caja etiquetada sale de sus componentes");
        c.PesoKg.Should().Be(10.201m);
        c.Componentes.Single(x => x.ComponenteId == cajaEtiquetada).Componentes.Should().HaveCount(2);
        c.Explosion.Select(x => (x.Nombre, x.Cantidad)).Should().BeEquivalentTo([("Caja cartón", 1m), ("Etiqueta", 1m), ("Naranja granel", 10m)]);

        // Ciclo: la caja etiquetada no puede llevar la caja de naranja, que ya la lleva a ella.
        var ciclo = await ComponerAsync(api, cajaEtiquetada, new[] { new { ComponenteId = caja, Cantidad = 1m }, new { ComponenteId = cajaNaranja, Cantidad = 1m } });
        ciclo.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ciclo.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("composicion.ciclo");

        // Dónde se usa la caja: directamente y dentro de la caja de naranja.
        var usos = (await api.GetFromJsonAsync<List<UsoResp>>($"/productos/{caja}/usos"))!;
        usos.Select(u => (u.Nombre, u.Nivel, u.CantidadPorUnidad)).Should().Equal(("Caja etiquetada", 1, 1m), ("Caja naranja 10 kg", 2, 1m));

        // Antes de crear otro compuesto: ya hay uno con la misma lista.
        var iguales = await api.PostAsJsonAsync("/productos/composiciones/buscar",
            new { Componentes = new[] { new { ComponenteId = etiqueta, Cantidad = 1m }, new { ComponenteId = caja, Cantidad = 1m } } });
        (await iguales.Content.ReadFromJsonAsync<List<IgualResp>>())!.Select(x => x.Id).Should().Equal(cajaEtiquetada);
        var otra = await ProductoAsync(api, "Caja etiquetada (repetida)", 0m, 0m);
        (await (await ComponerAsync(api, otra, new[] { new { ComponenteId = caja, Cantidad = 1m }, new { ComponenteId = etiqueta, Cantidad = 1m } }))
            .Content.ReadFromJsonAsync<ComposicionResp>())!.CompuestosIguales.Should().Equal("Caja etiquetada");
    }

    [Fact]
    public async Task Un_kit_toma_el_precio_de_sus_componentes_y_al_venderse_descuenta_sus_existencias()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var aceite = await ProductoAsync(api, "Aceite 1 l", 10m, 6m, stock: 10m);
        var vino = await ProductoAsync(api, "Vino tinto", 20m, 9m, stock: 4m);
        var pack = await ProductoAsync(api, "Pack regalo", 0m, 0m);

        // 2 aceites + 1 vino = 40 €, con un 10 % de descuento de pack.
        var r = await ComponerAsync(api, pack, new[] { new { ComponenteId = aceite, Cantidad = 2m }, new { ComponenteId = vino, Cantidad = 1m } }, "Kit", true, -10m);
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        var c = (await r.Content.ReadFromJsonAsync<ComposicionResp>())!;
        c.TipoComposicion.Should().Be("Kit");
        c.PrecioComponentes.Should().Be(40m);
        c.PrecioCalculado.Should().Be(36m);
        c.DisponibleKit.Should().Be(4m, "el vino alcanza para 4 packs y el aceite para 5");
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{pack}"))!.PrecioUnitario.Should().Be(36m);

        // Se venden 3 packs: salen 6 aceites y 3 vinos; el pack no tiene existencias propias.
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Tienda", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var f = await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = pack, Cantidad = 3m, Descripcion = "Pack regalo", PrecioUnitario = 36m, CodigoIva = "IVA21" } } });
        f.IsSuccessStatusCode.Should().BeTrue(await f.Content.ReadAsStringAsync());
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{aceite}"))!.Stock.Should().Be(4m);
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{vino}"))!.Stock.Should().Be(1m);
        (await api.GetFromJsonAsync<ComposicionResp>($"/productos/{pack}/composicion"))!.DisponibleKit.Should().Be(1m);

        // Si sube el aceite, recalcular deja el pack en (2 × 12 + 20) × 0,9.
        (await api.PutAsJsonAsync($"/productos/{aceite}", new { Nombre = "Aceite 1 l", PrecioUnitario = 12m, PrecioCompra = 6m, Tipo = "Bien", Unidad = "ud", ControlarStock = true }))
            .IsSuccessStatusCode.Should().BeTrue();
        (await api.PostAsync(new Uri("/productos/composiciones/recalcular-precios", UriKind.Relative), null)).IsSuccessStatusCode.Should().BeTrue();
        (await api.GetFromJsonAsync<ProductoResp>($"/productos/{pack}"))!.PrecioUnitario.Should().Be(39.6m);
    }
}
