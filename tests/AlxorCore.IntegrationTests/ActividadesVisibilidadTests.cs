using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Pruebas de las actividades de negocio (clasificación por grupo) y de la visibilidad por usuario
/// y pantalla: un maestro clasificado en una actividad solo lo ven los usuarios con esa actividad
/// concedida en el área; los maestros sin actividad los ve todo el mundo.
/// </summary>
public sealed class ActividadesVisibilidadTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ActividadesVisibilidadTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record ActividadResp(Guid Id, string Nombre, bool Activa);
    private sealed record ClienteResp(Guid Id, string Nombre);
    private sealed record PerfilResp(Guid Id);

    private static async Task<Guid> UsuarioActualAsync(HttpClient cli) =>
        (await cli.GetFromJsonAsync<PerfilResp>("/auth/perfil"))!.Id;

    [Fact]
    public async Task Crear_listar_y_actualizar_actividad()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        var creada = await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Distribución" }))
            .Content.ReadFromJsonAsync<ActividadResp>();
        creada!.Nombre.Should().Be("Distribución");
        creada.Activa.Should().BeTrue();

        var lista = await cli.GetFromJsonAsync<List<ActividadResp>>("/actividades");
        lista!.Should().ContainSingle(a => a.Id == creada.Id);

        var actualizar = await cli.PutAsJsonAsync($"/actividades/{creada.Id}", new { Nombre = "Mayorista", Activa = false });
        actualizar.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var relista = await cli.GetFromJsonAsync<List<ActividadResp>>("/actividades");
        relista!.Single(a => a.Id == creada.Id).Nombre.Should().Be("Mayorista");
        relista.Single(a => a.Id == creada.Id).Activa.Should().BeFalse();
    }

    [Fact]
    public async Task Cliente_sin_actividad_siempre_es_visible_aunque_haya_reglas()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = await UsuarioActualAsync(cli);

        var actividad = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Retail" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        // Cliente clasificado en la actividad y cliente sin actividad.
        var conActividad = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Cliente Retail", ActividadNegocioId = actividad })).Content.ReadFromJsonAsync<ClienteResp>())!.Id;
        var sinActividad = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Cliente General" })).Content.ReadFromJsonAsync<ClienteResp>())!.Id;

        // Sin reglas para el usuario: ve TODOS.
        var todos = await cli.GetFromJsonAsync<List<ClienteResp>>("/clientes");
        todos!.Select(c => c.Id).Should().Contain(new[] { conActividad, sinActividad });

        // Se conceden reglas al usuario en Ventas pero SIN la actividad «Retail».
        var otra = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Horeca" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var fijar = await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}",
            new { Area = "Ventas", Actividades = new[] { otra } });
        fijar.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Ahora ve el cliente sin actividad (siempre visible) pero NO el de la actividad no concedida.
        var visibles = await cli.GetFromJsonAsync<List<ClienteResp>>("/clientes");
        visibles!.Select(c => c.Id).Should().Contain(sinActividad);
        visibles.Select(c => c.Id).Should().NotContain(conActividad);
    }

    [Fact]
    public async Task Actividades_visibles_devuelve_solo_las_accesibles_del_area()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = await UsuarioActualAsync(cli);

        var a = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Accesible" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var b = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Oculta" })).Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        // Sin reglas: se ven todas.
        var todas = await cli.GetFromJsonAsync<List<ActividadResp>>("/actividades/visibles?area=Ventas");
        todas!.Select(x => x.Id).Should().Contain(new[] { a, b });

        // Con regla que solo concede A en Ventas: /visibles?area=Ventas devuelve solo A.
        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}", new { Area = "Ventas", Actividades = new[] { a } });
        var visibles = await cli.GetFromJsonAsync<List<ActividadResp>>("/actividades/visibles?area=Ventas");
        visibles!.Select(x => x.Id).Should().Contain(a).And.NotContain(b);

        // En Compras (sin reglas) siguen viéndose todas.
        var compras = await cli.GetFromJsonAsync<List<ActividadResp>>("/actividades/visibles?area=Compras");
        compras!.Select(x => x.Id).Should().Contain(new[] { a, b });
    }

    [Fact]
    public async Task Conceder_la_actividad_hace_visible_el_cliente()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = await UsuarioActualAsync(cli);

        var actividad = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Exportación" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;
        var cliente = (await (await cli.PostAsJsonAsync("/clientes",
            new { Nombre = "Cliente Export", ActividadNegocioId = actividad })).Content.ReadFromJsonAsync<ClienteResp>())!.Id;

        // Concedemos la actividad al usuario en Ventas.
        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}",
            new { Area = "Ventas", Actividades = new[] { actividad } });

        var visibles = await cli.GetFromJsonAsync<List<ClienteResp>>("/clientes");
        visibles!.Select(c => c.Id).Should().Contain(cliente);
    }

    [Fact]
    public async Task La_visibilidad_de_ventas_no_afecta_a_compras()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var usuario = await UsuarioActualAsync(cli);

        var actividad = (await (await cli.PostAsJsonAsync("/actividades", new { Nombre = "Ferretería" }))
            .Content.ReadFromJsonAsync<ActividadResp>())!.Id;

        // Proveedor clasificado en la actividad.
        var proveedor = await (await cli.PostAsJsonAsync("/proveedores",
            new { Nombre = "Proveedor Ferretería", ActividadNegocioId = actividad })).Content.ReadFromJsonAsync<ClienteResp>();

        // Reglas SOLO en Ventas (no en Compras) => en Compras el usuario ve todo.
        await cli.PutAsJsonAsync($"/actividades/visibilidad/{usuario}",
            new { Area = "Ventas", Actividades = Array.Empty<Guid>() });

        var proveedores = await cli.GetFromJsonAsync<List<ClienteResp>>("/proveedores");
        proveedores!.Select(p => p.Id).Should().Contain(proveedor!.Id);
    }
}
