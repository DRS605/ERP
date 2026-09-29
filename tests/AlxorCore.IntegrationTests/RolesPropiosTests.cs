using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Roles propios de la empresa, por puesto de trabajo: se crean desde plantillas y dan solo sus permisos.</summary>
public sealed class RolesPropiosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public RolesPropiosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record RolResp(Guid? Id, string Codigo, string Nombre, bool Propio, List<string> Permisos, int Miembros);
    private sealed record PlantillaResp(string Codigo, string Nombre, List<string> Permisos);
    private sealed record PermisoResp(string Codigo, string Area, string Descripcion);
    private sealed record InvitarResp(Guid UsuarioId, bool Creado);
    private sealed record LoginResp(string Token);
    private sealed record SeleccionResp(string Token, List<string> Permisos);
    private sealed record ProblemaResp(string Codigo);

    [Fact]
    public async Task Un_rol_de_bascula_recepciona_pero_no_confecciona_ni_toca_maestros()
    {
        var (duenio, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await duenio.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();

        // El catálogo describe todos los permisos, y hay plantillas por puesto.
        (await duenio.GetFromJsonAsync<List<PermisoResp>>("/roles/permisos"))!.Should().Contain(p => p.Codigo == "agro.recepcionar" && p.Area == "Agro");
        (await duenio.GetFromJsonAsync<List<PlantillaResp>>("/roles/plantillas"))!.Select(p => p.Codigo)
            .Should().Contain(["bascula", "confeccion", "expedicion", "jefe_almacen", "calidad", "tecnico_campo", "administracion"]);

        var bascula = await (await duenio.PostAsJsonAsync("/roles", new { Plantilla = "bascula" })).Content.ReadFromJsonAsync<RolResp>();
        bascula!.Should().Match<RolResp>(r => r.Propio && r.Nombre == "Báscula" && r.Codigo.StartsWith("rol_") && r.Permisos.Contains("agro.recepcionar"));
        var repetido = await duenio.PostAsJsonAsync("/roles", new { Plantilla = "bascula" });
        (await repetido.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("rol.repetido");
        var malo = await duenio.PostAsJsonAsync("/roles", new { Nombre = "Raro", Permisos = new[] { "agro.volar" } });
        (await malo.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("rol.permiso_desconocido");

        // Un empleado se registra y el dueño lo añade con el rol de báscula.
        var email = Ayudas.EmailUnico();
        var empleado = _fabrica.CreateClient();
        (await empleado.PostAsJsonAsync("/auth/registro", new { Email = email, Nombre = "Pepe", Contrasena = "contrasena123" })).EnsureSuccessStatusCode();
        var invitado = await (await duenio.PostAsJsonAsync("/usuarios/invitar", new { Email = email, Rol = bascula.Codigo })).Content.ReadFromJsonAsync<InvitarResp>();
        invitado!.Creado.Should().BeFalse();
        (await duenio.GetFromJsonAsync<List<RolResp>>("/roles"))!.Single(r => r.Codigo == bascula.Codigo).Miembros.Should().Be(1);

        var login = await (await empleado.PostAsJsonAsync("/auth/login", new { Email = email, Contrasena = "contrasena123" })).Content.ReadFromJsonAsync<LoginResp>();
        empleado.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        var seleccion = await (await empleado.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>();
        seleccion!.Permisos.Should().BeEquivalentTo(["agro.leer", "agro.recepcionar"]);
        empleado.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", seleccion.Token);

        // Recepciona (se rechaza por datos, no por permiso), pero no confecciona, no da de alta campañas ni gestiona usuarios.
        (await empleado.PostAsJsonAsync("/agro/recepciones", new { AgricultorId = Guid.NewGuid(), Fecha = DateOnly.FromDateTime(DateTime.UtcNow) }))
            .StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        (await empleado.PostAsJsonAsync("/agro/partes", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await empleado.PostAsJsonAsync("/agro/campanas", new { })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await empleado.GetAsync(new Uri("/roles", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await empleado.GetAsync(new Uri("/agro/recepciones", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        // Un rol con miembros no se borra; al cambiarle los permisos, rigen al volver a entrar.
        (await (await duenio.DeleteAsync(new Uri($"/roles/{bascula.Id}", UriKind.Relative))).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("rol.en_uso");
        (await duenio.PutAsJsonAsync($"/roles/{bascula.Id}", new { Nombre = "Báscula y confección", Permisos = new[] { "agro.leer", "agro.recepcionar", "agro.confeccionar" } }))
            .EnsureSuccessStatusCode();
        empleado.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.Token);
        var otra = await (await empleado.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>();
        otra!.Permisos.Should().Contain("agro.confeccionar");

        // La base de datos: un miembro no puede tener un rol de otra empresa.
        var (_, ajena) = await Ayudas.ConEmpresaAsync(_fabrica);
        await using var c = new NpgsqlConnection(FabricaApiPruebas.CadenaConexion);
        await c.OpenAsync();
        await using var cmd = new NpgsqlCommand($"UPDATE organizacion.membresia SET empresa_id = '{ajena}' WHERE usuario_id = '{invitado.UsuarioId}' AND empresa_id = '{empresa}'", c);
        (await FluentActions.Awaiting(() => cmd.ExecuteNonQueryAsync()).Should().ThrowAsync<PostgresException>()).Which.Hint.Should().Be("rol.desconocido");

        // Revocado el miembro, el rol ya se puede borrar.
        (await duenio.PostAsync(new Uri($"/usuarios/{invitado.UsuarioId}/revocar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await duenio.DeleteAsync(new Uri($"/roles/{bascula.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
