using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Fase 3: el catálogo (artículos) se comparte por grupo (definición compartida), pero las
/// existencias son por empresa. Además, los artículos se clasifican por actividad de negocio y su
/// visibilidad se controla por usuario en el área «Artículos».
/// </summary>
public sealed class CatalogoCompartidoTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CatalogoCompartidoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record GrupoResp(Guid Id);
    private sealed record EmpresaResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ActividadResp(Guid Id, string Nombre, bool Activa);
    private sealed record ProductoResp(Guid Id, string Nombre, bool ControlarStock, decimal Stock, Guid? ActividadNegocioId);
    private sealed record PerfilResp(Guid Id);

    private static async Task SeleccionarAsync(HttpClient cli, Guid empresaId)
    {
        var sel = await (await cli.PostAsync(new Uri($"/empresas/{empresaId}/seleccionar", UriKind.Relative), null))
            .Content.ReadFromJsonAsync<SeleccionResp>();
        cli.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sel!.Token);
    }

    private static async Task<Guid> UsuarioActualAsync(HttpClient cli) =>
        (await cli.GetFromJsonAsync<PerfilResp>("/auth/perfil"))!.Id;

    [Fact]
    public async Task Dos_empresas_del_mismo_grupo_comparten_el_articulo_pero_el_stock_es_por_empresa()
    {
        // Empresa A con un artículo con stock inicial.
        var (cli, empresaA) = await Ayudas.ConEmpresaAsync(_fabrica);
        var grupo = (await cli.GetFromJsonAsync<GrupoResp>("/grupos/actual"))!.Id;
        var creado = await (await cli.PostAsJsonAsync("/productos", new
        {
            Nombre = "Café 1kg",
            PrecioUnitario = 10m,
            Tipo = "Bien",
            CodigoIva = "IVA21",
            ControlarStock = true,
            StockInicial = 20m,
        })).Content.ReadFromJsonAsync<ProductoResp>();
        creado!.Stock.Should().Be(20m);

        // Empresa B en el MISMO grupo.
        var empresaB = (await (await cli.PostAsJsonAsync("/empresas", new { Nif = Ayudas.GenerarNif(), RazonSocial = "Empresa B SL", GrupoId = grupo }))
            .Content.ReadFromJsonAsync<EmpresaResp>())!.Id;
        await SeleccionarAsync(cli, empresaB);

        // Desde B se ve el mismo artículo (catálogo compartido), pero con stock 0 (existencias por empresa).
        var productosB = await cli.GetFromJsonAsync<List<ProductoResp>>("/productos");
        var enB = productosB!.Single(p => p.Id == creado.Id);
        enB.Stock.Should().Be(0m);

        // Un movimiento de entrada en B no afecta al stock de A.
        await cli.PostAsJsonAsync($"/productos/{creado.Id}/stock", new { Tipo = "Entrada", Cantidad = 5m });
        (await cli.GetFromJsonAsync<ProductoResp>($"/productos/{creado.Id}"))!.Stock.Should().Be(5m);

        await SeleccionarAsync(cli, empresaA);
        (await cli.GetFromJsonAsync<ProductoResp>($"/productos/{creado.Id}"))!.Stock.Should().Be(20m);
    }

    [Fact]
    public async Task La_visibilidad_por_actividad_filtra_los_articulos()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = await UsuarioActualAsync(cli);

        var actividad = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Hostelería" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        var conActividad = (await (await cli.PostAsJsonAsync("/productos",
            new { Nombre = "Vaso sidra", PrecioUnitario = 1m, CodigoIva = "IVA21", ActividadNegocioId = actividad }))
            .Content.ReadFromJsonAsync<ProductoResp>())!.Id;
        var sinActividad = (await (await cli.PostAsJsonAsync("/productos",
            new { Nombre = "Genérico", PrecioUnitario = 1m, CodigoIva = "IVA21" }))
            .Content.ReadFromJsonAsync<ProductoResp>())!.Id;

        // Sin reglas: se ven todos.
        (await cli.GetFromJsonAsync<List<ProductoResp>>("/productos"))!.Select(p => p.Id)
            .Should().Contain(new[] { conActividad, sinActividad });

        // Regla en Artículos con OTRA actividad: solo se ve el artículo sin actividad.
        var otra = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Taller" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var fijar = await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}",
            new { Area = "Articulos", Actividades = new[] { otra } });
        fijar.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var visibles = (await cli.GetFromJsonAsync<List<ProductoResp>>("/productos"))!.Select(p => p.Id).ToList();
        visibles.Should().Contain(sinActividad);
        visibles.Should().NotContain(conActividad);

        // Al conceder la actividad, el artículo vuelve a verse.
        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}",
            new { Area = "Articulos", Actividades = new[] { otra, actividad } });
        (await cli.GetFromJsonAsync<List<ProductoResp>>("/productos"))!.Select(p => p.Id).Should().Contain(conActividad);
    }
}
