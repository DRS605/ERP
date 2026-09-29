using AlxorCore.Nucleo.Autorizacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Identidad.Tests.Dominio;

public class RolTests
{
    [Fact]
    public void Propietario_concede_todos_los_permisos()
    {
        foreach (var permiso in Permisos.Todos)
        {
            Rol.Propietario.Concede(permiso).Should().BeTrue($"el propietario debe tener {permiso}");
        }
    }

    [Fact]
    public void SoloLectura_no_puede_emitir_facturas_pero_puede_leer_y_exportar()
    {
        Rol.SoloLectura.Concede(Permisos.FacturaEmitir).Should().BeFalse();
        Rol.SoloLectura.Concede(Permisos.FacturaLeer).Should().BeTrue();
        Rol.SoloLectura.Concede(Permisos.DatosExportar).Should().BeTrue();
    }

    [Fact]
    public void Usuario_opera_pero_no_gestiona_usuarios_ni_ajustes()
    {
        Rol.Usuario.Concede(Permisos.FacturaEmitir).Should().BeTrue();
        Rol.Usuario.Concede(Permisos.UsuarioGestionar).Should().BeFalse();
        Rol.Usuario.Concede(Permisos.EmpresaAjustes).Should().BeFalse();
    }

    [Theory]
    [InlineData("propietario")]
    [InlineData("usuario")]
    [InlineData("solo_lectura")]
    public void PorCodigoRol_resuelve_roles_conocidos(string codigo)
    {
        Rol.PorCodigoRol(codigo).EsCorrecto.Should().BeTrue();
    }

    [Fact]
    public void PorCodigoRol_falla_con_rol_desconocido()
    {
        Rol.PorCodigoRol("inexistente").EsFallo.Should().BeTrue();
    }

    [Fact]
    public void El_catalogo_describe_todos_los_permisos_y_las_plantillas_solo_usan_permisos_que_existen()
    {
        CatalogoPermisos.Todos.Select(p => p.Codigo).Should().BeEquivalentTo(Permisos.Todos, "cada permiso necesita su descripción en la pantalla de roles");
        CatalogoPermisos.Plantillas.SelectMany(p => p.Permisos).Should().OnlyContain(p => Permisos.Todos.Contains(p));
        CatalogoPermisos.Plantillas.Single(p => p.Codigo == "direccion").Permisos.Should().NotContain([Permisos.UsuarioGestionar, Permisos.EmpresaAjustes]);
    }

    [Fact]
    public void Un_rol_propio_solo_da_permisos_del_catalogo()
    {
        var rol = Rol.Propio("rol_" + Guid.NewGuid().ToString("N"), "Báscula", [Permisos.AgroLeer, Permisos.AgroRecepcionar, "agro.volar"]);
        rol.PermisosConcedidos.Should().BeEquivalentTo([Permisos.AgroLeer, Permisos.AgroRecepcionar]);
        Rol.EsPropio(rol.Codigo).Should().BeTrue();
        Rol.EsPropio("usuario").Should().BeFalse();
    }
}
