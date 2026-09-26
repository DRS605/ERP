using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de las tarifas de precios de venta: definición, asignación a clientes y aplicación en facturas y presupuestos.</summary>
public sealed class TarifasEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public TarifasEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal PorcentajeDescuento, decimal Base);
    private sealed record FacturaResp(Guid Id, decimal BaseImponible, List<LineaResp> Lineas);
    private sealed record PrecioResp(decimal PrecioUnitario, decimal PorcentajeDescuento, string Origen);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record SeleccionResp(string Token);

    /// <summary>Empresa con familia Bebidas > Refrescos, un refresco a 10 € y un cliente con la tarifa MAYOR.</summary>
    private async Task<(HttpClient C, Guid Cliente, Guid Producto, Guid Tarifa)> PrepararAsync()
    {
        var (c, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var bebidas = (await (await c.PostAsJsonAsync("/familias", new { Nombre = "Bebidas" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var refrescos = (await (await c.PostAsJsonAsync("/familias", new { Nombre = "Refrescos", PadreId = bebidas })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var producto = (await (await c.PostAsJsonAsync("/productos", new
            { Nombre = "Refresco 33 cl", PrecioUnitario = 10m, Tipo = "Bien", CodigoIva = "IVA21", FamiliaId = refrescos }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;
        var cliente = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Bar Central SL", NifFiscal = "B12345674" }))
            .Content.ReadFromJsonAsync<IdResp>())!.Id;

        var crear = await c.PostAsJsonAsync("/tarifas", new
        {
            Codigo = "mayor",
            Nombre = "Mayoristas",
            Lineas = new object[]
            {
                new { PorcentajeDescuento = 5m },                                        // general
                new { FamiliaId = bebidas, PorcentajeDescuento = 10m },                  // la familia (y subfamilias)
                new { ProductoId = producto, CantidadMinima = 100m, Precio = 8.5m },     // escalado del producto
            },
        });
        crear.StatusCode.Should().Be(HttpStatusCode.Created, await crear.Content.ReadAsStringAsync());
        var tarifa = (await crear.Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.PutAsJsonAsync($"/clientes/{cliente}/tarifa", new { TarifaId = tarifa })).StatusCode.Should().Be(HttpStatusCode.OK);
        return (c, cliente, producto, tarifa);
    }

    [Fact]
    public async Task La_factura_sin_precio_toma_precio_y_descuento_de_la_tarifa_del_cliente()
    {
        var (c, cliente, producto, _) = await PrepararAsync();
        var resp = await c.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new object[]
            {
                new { ProductoId = producto, Cantidad = 10m },            // familia: 10 € −10 %
                new { ProductoId = producto, Cantidad = 120m },           // escalado: 8,50 €
                new { ProductoId = producto, Cantidad = 5m, PrecioUnitario = 11m }, // precio escrito: se respeta
            },
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        var f = (await resp.Content.ReadFromJsonAsync<FacturaResp>())!;
        f.Lineas.Select(l => (l.PrecioUnitario, l.PorcentajeDescuento, l.Base)).Should().Equal(
            (10m, 10m, 90m), (8.5m, 0m, 1020m), (11m, 0m, 55m));
    }

    [Fact]
    public async Task La_consulta_de_precio_explica_de_donde_sale()
    {
        var (c, cliente, producto, _) = await PrepararAsync();
        var p = await c.GetFromJsonAsync<PrecioResp>($"/precios?clienteId={cliente}&productoId={producto}&cantidad=150");
        p.Should().Be(new PrecioResp(8.5m, 0m, "Tarifa MAYOR (precio del producto, desde 100 uds)"));

        var sinTarifa = (await (await c.PostAsJsonAsync("/clientes", new { Nombre = "Particular" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        (await c.GetFromJsonAsync<PrecioResp>($"/precios?clienteId={sinTarifa}&productoId={producto}"))
            .Should().Be(new PrecioResp(10m, 0m, "Precio del producto"));
    }

    [Fact]
    public async Task El_presupuesto_tambien_aplica_la_tarifa()
    {
        var (c, cliente, producto, _) = await PrepararAsync();
        var resp = await c.PostAsJsonAsync("/presupuestos", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Cantidad = 2m } } });
        resp.StatusCode.Should().Be(HttpStatusCode.Created, await resp.Content.ReadAsStringAsync());
        (await resp.Content.ReadAsStringAsync()).Should().Contain("\"porcentajeDescuento\":10");
    }

    [Fact]
    public async Task Rechaza_tarifas_incoherentes_o_con_referencias_que_no_existen()
    {
        var (c, _, _, _) = await PrepararAsync();
        var duplicada = await c.PostAsJsonAsync("/tarifas", new { Codigo = "MAYOR", Nombre = "Otra", Lineas = new[] { new { PorcentajeDescuento = 1m } } });
        duplicada.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var fantasma = await c.PostAsJsonAsync("/tarifas", new { Codigo = "X", Nombre = "X", Lineas = new[] { new { ProductoId = Guid.NewGuid(), Precio = 1m } } });
        fantasma.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await fantasma.Content.ReadFromJsonAsync<ProblemaResp>())!.Title.Should().Be("El producto de la línea 1 no existe.");

        (await c.PutAsJsonAsync($"/clientes/{Guid.NewGuid()}/tarifa", new { TarifaId = Guid.NewGuid() })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Las_tarifas_son_del_modulo_ventas()
    {
        var (c, empresaId) = await Ayudas.ConEmpresaAsync(_fabrica);
        await c.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "start" });
        var sel = await (await c.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>();
        c.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", sel!.Token);
        (await c.GetAsync("/tarifas")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
