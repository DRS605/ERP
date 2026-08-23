using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

public sealed class ActividadNegocioTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Crear_con_nombre_valido_queda_activa()
    {
        var r = ActividadNegocio.Crear(Guid.NewGuid(), "  Distribución  ", Reloj);

        r.EsCorrecto.Should().BeTrue();
        r.Valor.Nombre.Should().Be("Distribución");
        r.Valor.Activa.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_sin_nombre_falla(string? nombre)
    {
        ActividadNegocio.Crear(Guid.NewGuid(), nombre, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Crear_con_nombre_demasiado_largo_falla()
    {
        var largo = new string('x', ActividadNegocio.LongitudMaximaNombre + 1);
        ActividadNegocio.Crear(Guid.NewGuid(), largo, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Renombrar_cambia_el_nombre()
    {
        var actividad = ActividadNegocio.Crear(Guid.NewGuid(), "Antigua", Reloj).Valor;

        actividad.Renombrar("Nueva", Reloj).EsCorrecto.Should().BeTrue();
        actividad.Nombre.Should().Be("Nueva");
    }

    [Fact]
    public void Renombrar_con_valor_invalido_falla_y_no_muta()
    {
        var actividad = ActividadNegocio.Crear(Guid.NewGuid(), "Original", Reloj).Valor;

        actividad.Renombrar("   ", Reloj).EsFallo.Should().BeTrue();
        actividad.Nombre.Should().Be("Original");
    }

    [Fact]
    public void FijarActiva_desactiva_y_reactiva()
    {
        var actividad = ActividadNegocio.Crear(Guid.NewGuid(), "Servicios", Reloj).Valor;

        actividad.FijarActiva(false, Reloj);
        actividad.Activa.Should().BeFalse();

        actividad.FijarActiva(true, Reloj);
        actividad.Activa.Should().BeTrue();
    }

    [Fact]
    public void VisibilidadActividad_conserva_area_y_usuario()
    {
        var grupo = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        var actividad = Guid.NewGuid();

        var v = VisibilidadActividad.Crear(grupo, usuario, AreaVisibilidad.Compras, actividad, Reloj);

        v.GrupoId.Should().Be(grupo);
        v.UsuarioId.Should().Be(usuario);
        v.Area.Should().Be(AreaVisibilidad.Compras);
        v.ActividadNegocioId.Should().Be(actividad);
    }
}
