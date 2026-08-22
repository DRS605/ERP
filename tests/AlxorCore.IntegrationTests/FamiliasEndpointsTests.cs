using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración de las familias de artículos (árbol de subfamilias y asignación).</summary>
public sealed class FamiliasEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FamiliasEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record FamiliaResp(Guid Id, string Nombre, string? Codigo, Guid? PadreId, bool Activo, string RutaCompleta, int Nivel);
    private sealed record FamiliaArbolResp(Guid Id, string Nombre, string? Codigo, bool Activo, List<FamiliaArbolResp> Hijos);
    private sealed record ProductoResp(Guid Id, string Nombre, string? Familia, Guid? FamiliaId);

    private static async Task<FamiliaResp> CrearAsync(HttpClient cliente, string nombre, Guid? padreId = null, string? codigo = null)
    {
        var r = await cliente.PostAsJsonAsync("/familias", new { Nombre = nombre, Codigo = codigo, PadreId = padreId });
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await r.Content.ReadFromJsonAsync<FamiliaResp>())!;
    }

    [Fact]
    public async Task Crea_subfamilias_y_calcula_la_ruta_completa()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var raiz = await CrearAsync(cliente, "Alimentación");
        var bebidas = await CrearAsync(cliente, "Bebidas", raiz.Id);
        var refrescos = await CrearAsync(cliente, "Refrescos", bebidas.Id);

        refrescos.RutaCompleta.Should().Be("Alimentación > Bebidas > Refrescos");
        refrescos.Nivel.Should().Be(2);
        refrescos.PadreId.Should().Be(bebidas.Id);
    }

    [Fact]
    public async Task El_arbol_anida_las_subfamilias_bajo_su_padre()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var raiz = await CrearAsync(cliente, "Ferretería");
        var tornillos = await CrearAsync(cliente, "Tornillería", raiz.Id);
        await CrearAsync(cliente, "Herramientas", raiz.Id);

        var arbol = await cliente.GetFromJsonAsync<List<FamiliaArbolResp>>("/familias/arbol");
        var ferreteria = arbol!.Single(f => f.Id == raiz.Id);
        ferreteria.Hijos.Should().HaveCount(2);
        ferreteria.Hijos.Should().Contain(h => h.Id == tornillos.Id);
    }

    [Fact]
    public async Task No_permite_mover_una_familia_dentro_de_su_subfamilia_ciclo()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var raiz = await CrearAsync(cliente, "A");
        var hija = await CrearAsync(cliente, "B", raiz.Id);

        // Intentar colgar la raíz de su propia hija → ciclo → 400.
        var r = await cliente.PutAsJsonAsync($"/familias/{raiz.Id}", new { Nombre = "A", PadreId = hija.Id, Activo = true });
        r.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Asignar_familia_a_un_articulo_sincroniza_el_texto_de_familia()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var raiz = await CrearAsync(cliente, "Alimentación");
        var bebidas = await CrearAsync(cliente, "Bebidas", raiz.Id);

        var crear = await cliente.PostAsJsonAsync("/productos", new { Nombre = "Agua 1L", PrecioUnitario = 1m, CodigoIva = "IVA21", FamiliaId = bebidas.Id });
        crear.StatusCode.Should().Be(HttpStatusCode.Created);
        var producto = (await crear.Content.ReadFromJsonAsync<ProductoResp>())!;

        producto.FamiliaId.Should().Be(bebidas.Id);
        producto.Familia.Should().Be("Bebidas"); // el texto se sincroniza con el nombre de la familia
    }

    [Fact]
    public async Task No_deja_eliminar_una_familia_con_subfamilias_ni_con_articulos()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var raiz = await CrearAsync(cliente, "Con hijos");
        var hija = await CrearAsync(cliente, "Hija", raiz.Id);

        // Con subfamilias → conflicto.
        (await cliente.DeleteAsync($"/familias/{raiz.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Asignamos un artículo a la hija → tampoco se puede borrar la hija.
        await cliente.PostAsJsonAsync("/productos", new { Nombre = "X", PrecioUnitario = 1m, CodigoIva = "IVA21", FamiliaId = hija.Id });
        (await cliente.DeleteAsync($"/familias/{hija.Id}")).StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Elimina_una_familia_hoja_sin_articulos()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var f = await CrearAsync(cliente, "Temporal");
        (await cliente.DeleteAsync($"/familias/{f.Id}")).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var lista = await cliente.GetFromJsonAsync<List<FamiliaResp>>("/familias");
        lista!.Should().NotContain(x => x.Id == f.Id);
    }
}
