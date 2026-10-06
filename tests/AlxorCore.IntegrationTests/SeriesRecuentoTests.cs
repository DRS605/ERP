using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Números de serie (una unidad por número, sin duplicados) e inventario físico con regularización al cerrar.</summary>
public sealed class SeriesRecuentoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public SeriesRecuentoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ExistenciaResp(Guid ProductoId, Guid AlmacenId, Guid? UbicacionId, decimal Cantidad, string? Lote);
    private sealed record MovimientoResp(string Tipo, decimal Cantidad, string? Motivo, string? Lote);
    private sealed record LineaResp(Guid Id, Guid ProductoId, string? Lote, decimal Teorico, decimal? Contado, decimal? Diferencia, bool Añadida);
    private sealed record RecuentoResp(Guid Id, string Codigo, string Estado, int Lineas, int Contadas, int ConDiferencia, List<LineaResp>? Detalle);

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
    public async Task Numeros_de_serie_y_recuento_fisico_con_movimientos_durante_el_conteo()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var a1 = (await OkAsync<IdResp>(api.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "CEN", Nombre = "Central" }))).Id;
        var a2 = (await OkAsync<IdResp>(api.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "TDA", Nombre = "Tienda" }))).Id;
        var taladro = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Taladro percutor", PrecioUnitario = 120m, CodigoIva = "IVA21", Seguimiento = "Serie" }))).Id;
        var tornillo = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Tornillo 4x40", PrecioUnitario = 0.05m, CodigoIva = "IVA21" }))).Id;

        // Con número de serie: cada unidad su número, una por movimiento y sin duplicados.
        (await FalloAsync(api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = taladro, AlmacenId = a1, Cantidad = 1m }), HttpStatusCode.BadRequest))
            .Should().Be("inventario.serie_requerida");
        (await FalloAsync(api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = taladro, AlmacenId = a1, Cantidad = 2m, Lote = "TP-001" }), HttpStatusCode.BadRequest))
            .Should().Be("inventario.serie_unidad");
        (await OkAsync<List<ExistenciaResp>>(api.PostAsJsonAsync("/inventario/series/entrada", new { ProductoId = taladro, AlmacenId = a1, Series = new[] { "TP-001", "TP-002", "TP-003" }, CosteUnitario = 70m })))
            .Should().HaveCount(3);
        (await FalloAsync(api.PostAsJsonAsync("/inventario/series/entrada", new { ProductoId = taladro, AlmacenId = a2, Series = new[] { "TP-004", "TP-002" } }), HttpStatusCode.Conflict))
            .Should().Be("inventario.serie_duplicada");
        (await FalloAsync(api.PostAsJsonAsync("/inventario/series/entrada", new { ProductoId = taladro, AlmacenId = a2, Series = new[] { "TP-005", "TP-005" } }), HttpStatusCode.BadRequest))
            .Should().Be("inventario.serie_repetida");
        (await FalloAsync(api.PostAsJsonAsync("/inventario/series/entrada", new { ProductoId = tornillo, AlmacenId = a1, Series = new[] { "X" } }), HttpStatusCode.BadRequest))
            .Should().Be("inventario.sin_serie");
        (await api.PostAsJsonAsync("/inventario/salida", new { ProductoId = taladro, AlmacenId = a1, Cantidad = 1m, Lote = "TP-001" })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/inventario/traspaso", new { ProductoId = taladro, Cantidad = 1m, AlmacenOrigenId = a1, AlmacenDestinoId = a2, Lote = "TP-002" })).EnsureSuccessStatusCode();
        var series = await OkAsync<List<ExistenciaResp>>(api.GetAsync($"/inventario/series/{taladro}"));
        series.Select(s => (s.Lote, s.AlmacenId)).Should().Equal(("TP-002", a2), ("TP-003", a1));
        // Vendido y vuelto a entrar (devolución): el número vuelve a valer.
        (await OkAsync<List<ExistenciaResp>>(api.PostAsJsonAsync("/inventario/series/entrada", new { ProductoId = taladro, AlmacenId = a1, Series = new[] { "TP-001" } })))
            .Should().ContainSingle();
        (await api.PostAsJsonAsync("/inventario/salida", new { ProductoId = taladro, AlmacenId = a1, Cantidad = 1m, Lote = "TP-001" })).EnsureSuccessStatusCode();

        // Recuento del almacén central: teórico congelado.
        (await api.PostAsJsonAsync("/inventario/entrada", new { ProductoId = tornillo, AlmacenId = a1, Cantidad = 100m })).EnsureSuccessStatusCode();
        var rec = await OkAsync<RecuentoResp>(api.PostAsJsonAsync("/inventario/recuentos", new { AlmacenId = a1, Descripcion = "Cierre de ejercicio" }));
        rec.Codigo.Should().MatchRegex(@"^INV-\d{4}-0001$");
        rec.Detalle!.Select(l => (l.ProductoId, l.Lote, l.Teorico)).Should().BeEquivalentTo([(taladro, (string?)"TP-003", 1m), (tornillo, (string?)null, 100m)]);
        (await FalloAsync(api.PostAsJsonAsync("/inventario/recuentos", new { AlmacenId = a1 }), HttpStatusCode.Conflict)).Should().Be("recuento.abierto");

        // Mientras se cuenta se venden 10 tornillos.
        (await api.PostAsJsonAsync("/inventario/salida", new { ProductoId = tornillo, AlmacenId = a1, Cantidad = 10m })).EnsureSuccessStatusCode();

        (await FalloAsync(api.PutAsJsonAsync($"/inventario/recuentos/{rec.Id}/conteos", new { Lineas = new[] { new { ProductoId = taladro, Cantidad = (decimal?)2m, Lote = "TP-003" } } }),
            HttpStatusCode.BadRequest)).Should().Be("recuento.serie");
        var contado = await OkAsync<RecuentoResp>(api.PutAsJsonAsync($"/inventario/recuentos/{rec.Id}/conteos", new
        {
            Lineas = new object[]
            {
                new { ProductoId = tornillo, Cantidad = 97m },
                new { ProductoId = taladro, Cantidad = 0m, Lote = "TP-003" },
                new { ProductoId = taladro, Cantidad = 1m, Lote = "TP-009" },
            },
        }));
        contado.Should().Match<RecuentoResp>(r => r.Lineas == 3 && r.Contadas == 3 && r.ConDiferencia == 3);
        contado.Detalle!.Single(l => l.Lote == "TP-009").Añadida.Should().BeTrue();

        var cerrado = await OkAsync<RecuentoResp>(api.PostAsJsonAsync($"/inventario/recuentos/{rec.Id}/cerrar", new { }));
        cerrado.Estado.Should().Be("Cerrado");
        var stockTornillo = await OkAsync<List<ExistenciaResp>>(api.GetAsync($"/inventario/stock/producto/{tornillo}"));
        stockTornillo.Single().Cantidad.Should().Be(87m, "90 que quedaban tras la venta − 3 que faltaban al contar");
        (await OkAsync<List<ExistenciaResp>>(api.GetAsync($"/inventario/series/{taladro}"))).Select(s => s.Lote).Should().Equal("TP-002", "TP-009");
        var movs = await OkAsync<List<MovimientoResp>>(api.GetAsync($"/inventario/movimientos/producto/{tornillo}"));
        movs.Should().ContainSingle(m => m.Tipo == "Ajuste" && m.Cantidad == -3m && m.Motivo == $"Recuento {rec.Codigo}");
        (await FalloAsync(api.PostAsync(new Uri($"/inventario/recuentos/{rec.Id}/anular", UriKind.Relative), null), HttpStatusCode.Conflict)).Should().Be("recuento.no_abierto");
        (await FalloAsync(api.PutAsJsonAsync($"/inventario/recuentos/{rec.Id}/conteos", new { Lineas = new[] { new { ProductoId = tornillo, Cantidad = (decimal?)1m } } }),
            HttpStatusCode.Conflict)).Should().Be("recuento.no_abierto");

        // Un recuento de la tienda que no se cuenta: se anula; y cerrar sin contar nada no se puede.
        var otro = await OkAsync<RecuentoResp>(api.PostAsJsonAsync("/inventario/recuentos", new { AlmacenId = a2 }));
        (await FalloAsync(api.PostAsJsonAsync($"/inventario/recuentos/{otro.Id}/cerrar", new { }), HttpStatusCode.BadRequest)).Should().Be("recuento.vacio");
        (await api.PostAsync(new Uri($"/inventario/recuentos/{otro.Id}/anular", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await OkAsync<List<RecuentoResp>>(api.GetAsync("/inventario/recuentos"))).Select(r => r.Estado).Should().Equal("Anulado", "Cerrado");
    }
}
