using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Los listados de clientes, proveedores y artículos traen sus cifras (facturado, pendiente, vencido, comprado,
/// stock, ventas) y los totales de todo el filtro calculados en el servidor, no solo de la página.
/// </summary>
public sealed class CifrasMaestrosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CifrasMaestrosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record FacturaResp(Guid Id, decimal Total);
    private sealed record Elemento(Guid Id, string Nombre);
    private sealed record PaginaResp(List<Elemento> Elementos, int Total, int TotalPaginas, int Ejercicio, Dictionary<string, decimal> Totales, Dictionary<Guid, Dictionary<string, decimal>> Cifras);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    [Fact]
    public async Task Clientes_proveedores_y_articulos_totalizan_todo_el_filtro()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var año = DateTime.Today.Year;
        var articulo = await IdAsync(api, "/productos", new { Nombre = "Caja cartón", PrecioUnitario = 2m, PrecioCompra = 1.5m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        (await api.PostAsJsonAsync($"/productos/{articulo}/stock", new { Tipo = "Entrada", Cantidad = 100m })).EnsureSuccessStatusCode();

        // 30 clientes (dos páginas de 25), cada uno con una factura de 100 € de base (104 € con IVA 4 %).
        FacturaResp? primera = null;
        for (var i = 0; i < 30; i++)
        {
            var cliente = await IdAsync(api, "/clientes", new { Nombre = $"Cliente {i:D2}" });
            var r = await api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = articulo, Descripcion = "Caja", Cantidad = 50m, PrecioUnitario = 2m, CodigoIva = "IVA4" } } });
            r.IsSuccessStatusCode.Should().BeTrue(await r.Content.ReadAsStringAsync());
            primera ??= await r.Content.ReadFromJsonAsync<FacturaResp>();
        }

        (await api.PostAsJsonAsync("/cobros", new { FacturaId = primera!.Id, Importe = 104m })).EnsureSuccessStatusCode();

        var clientes = (await api.GetFromJsonAsync<PaginaResp>("/clientes/buscar?texto=Cliente&tamanoPagina=25"))!;
        clientes.Elementos.Should().HaveCount(25);
        clientes.TotalPaginas.Should().Be(2);
        clientes.Ejercicio.Should().Be(año);
        clientes.Totales["registros"].Should().Be(30m);
        clientes.Totales["facturas"].Should().Be(30m);
        clientes.Totales["facturado"].Should().Be(3000m, "la base de las 30 facturas, no solo las de la página");
        clientes.Totales["pendiente"].Should().Be(29 * 104m, "una está cobrada");
        clientes.Cifras.Should().HaveCount(25).And.OnlyContain(c => c.Value["facturado"] == 100m);

        // Anticipos por aplicar: un cliente sin facturas también sale con su anticipo.
        var conAnticipo = await IdAsync(api, "/clientes", new { Nombre = "Cliente con anticipo" });
        (await api.PostAsJsonAsync("/anticipos", new { ClienteId = conAnticipo, Importe = 250m })).EnsureSuccessStatusCode();
        var anticipos = (await api.GetFromJsonAsync<PaginaResp>("/clientes/buscar?texto=anticipo"))!;
        anticipos.Totales["anticipos"].Should().Be(250m);
        anticipos.Cifras[conAnticipo]["anticipos"].Should().Be(250m);

        // Filtro: solo los que contienen «Cliente 0» (00..09).
        var filtrados = (await api.GetFromJsonAsync<PaginaResp>("/clientes/buscar?texto=Cliente%200"))!;
        filtrados.Totales["registros"].Should().Be(10m);
        filtrados.Totales["facturado"].Should().Be(1000m);

        // Otro ejercicio: sin facturación, pero la deuda viva sigue.
        var anterior = (await api.GetFromJsonAsync<PaginaResp>($"/clientes/buscar?texto=Cliente&ejercicio={año - 1}"))!;
        anterior.Totales["facturado"].Should().Be(0m);
        anterior.Totales["pendiente"].Should().Be(29 * 104m);

        // Proveedores: comprado, pendiente y vencido según sus vencimientos.
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Envases SL" });
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        (await api.PostAsJsonAsync("/gastos", new
        {
            ProveedorId = proveedor, NumeroFactura = "E-1", FechaFactura = hoy.AddDays(-40).ToString("yyyy-MM-dd"), Fecha = hoy.AddDays(-40).ToString("yyyy-MM-dd"),
            Lineas = new[] { new { Base = 1000m, CodigoIva = "IVA21" } },
            Vencimientos = new[] { new { Fecha = hoy.AddDays(-10).ToString("yyyy-MM-dd"), Importe = 605m }, new { Fecha = hoy.AddDays(20).ToString("yyyy-MM-dd"), Importe = 605m } },
        })).EnsureSuccessStatusCode();
        var proveedores = (await api.GetFromJsonAsync<PaginaResp>("/proveedores/buscar"))!;
        proveedores.Totales["comprado"].Should().Be(hoy.AddDays(-40).Year == año ? 1000m : 0m);
        proveedores.Totales["pendiente"].Should().Be(1210m);
        proveedores.Totales["vencido"].Should().Be(605m, "solo el primer plazo ha vencido");

        // Artículos: stock, valor a precio de compra y ventas del ejercicio.
        var articulos = (await api.GetFromJsonAsync<PaginaResp>("/productos/buscar"))!;
        articulos.Totales["unidadesVendidas"].Should().Be(1500m);
        articulos.Totales["ventas"].Should().Be(3000m);
        var stock = articulos.Cifras[articulo]["stock"];
        articulos.Totales["valorStock"].Should().Be(Math.Round(stock * 1.5m, 2));
    }
}
