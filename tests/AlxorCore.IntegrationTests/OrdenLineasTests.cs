using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Las líneas de los documentos vuelven en el orden en que se escribieron (la base de datos no lo garantiza sola).</summary>
public sealed class OrdenLineasTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public OrdenLineasTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record LineaResp(string Descripcion);
    private sealed record DocResp(Guid Id, List<LineaResp> Lineas);

    private static readonly string[] Textos = ["Primera", "Segunda", "Tercera", "Cuarta", "Quinta", "Sexta", "Séptima", "Octava"];

    [Fact]
    public async Task Facturas_presupuestos_y_pedidos_conservan_el_orden_de_las_lineas()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Cliente SL", NifFiscal = "B12345674" })).Content.ReadFromJsonAsync<IdResp>())!.Id;
        var lineas = Textos.Select((t, i) => new { Descripcion = t, Cantidad = 1m + i, PrecioUnitario = 10m, CodigoIva = "IVA21" }).ToArray();

        async Task Comprobar(string alta, object cuerpo, Func<Guid, string> ruta)
        {
            var r = await api.PostAsJsonAsync(alta, cuerpo);
            r.IsSuccessStatusCode.Should().BeTrue($"{alta}: {await r.Content.ReadAsStringAsync()}");
            var id = (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
            var doc = (await api.GetFromJsonAsync<DocResp>(ruta(id)))!;
            doc.Lineas.Select(l => l.Descripcion).Should().Equal(Textos, $"{alta} devuelve las líneas en su orden");
        }

        await Comprobar("/facturas", new { ClienteId = cliente, Lineas = lineas }, id => $"/facturas/{id}");
        await Comprobar("/presupuestos", new { ClienteId = cliente, Lineas = lineas }, id => $"/presupuestos/{id}");
        await Comprobar("/pedidos-venta", new { ClienteId = cliente, Lineas = lineas }, id => $"/pedidos-venta/{id}");
        await Comprobar("/compras/pedidos", new { ProveedorTexto = "Proveedor SL", Lineas = Textos.Select(t => new { Descripcion = t, Cantidad = 1m, PrecioUnitario = 2m }).ToArray() },
            id => $"/compras/pedidos/{id}");
    }
}
