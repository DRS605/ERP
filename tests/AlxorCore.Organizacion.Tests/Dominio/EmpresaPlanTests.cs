using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Modulos;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Organizacion.Dominio.Eventos;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

/// <summary>Pruebas del plan contratado (edición + módulos adicionales) de la empresa.</summary>
public class EmpresaPlanTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    private static Empresa NuevaEmpresa() =>
        Empresa.Crear(Guid.NewGuid(), Nif.Crear("B12345674").Valor, "Mi Empresa SL", Direccion.Vacia, RegimenIva.General, Reloj).Valor;

    [Fact]
    public void Por_defecto_la_empresa_tiene_la_edicion_completa()
    {
        var e = NuevaEmpresa();
        e.Plan.Edicion.Should().Be(CatalogoModulos.EdicionCompleta);
        e.Plan.ModulosActivos.Should().HaveCount(CatalogoModulos.Modulos.Count(m => !m.Vertical), "los módulos sectoriales se contratan aparte");
    }

    [Fact]
    public void Start_no_incluye_ningun_modulo_contratable()
    {
        var e = NuevaEmpresa();
        e.CambiarPlan(CatalogoModulos.EdicionStart, null, Reloj).EsCorrecto.Should().BeTrue();
        e.Plan.ModulosActivos.Should().BeEmpty();
        e.Plan.Incluye(CatalogoModulos.Contabilidad).Should().BeFalse();
    }

    [Fact]
    public void Gestion_para_empresas_generalistas_incluye_ventas_compras_e_inventario_pero_no_contabilidad()
    {
        var e = NuevaEmpresa();
        e.CambiarPlan(CatalogoModulos.EdicionGestion, null, Reloj).EsCorrecto.Should().BeTrue();
        e.Plan.ModulosActivos.Should().Contain([CatalogoModulos.Ventas, CatalogoModulos.Compras, CatalogoModulos.Inventario]);
        e.Plan.Incluye(CatalogoModulos.Contabilidad).Should().BeFalse();
    }

    [Fact]
    public void Se_pueden_contratar_modulos_sueltos_sobre_una_edicion()
    {
        var e = NuevaEmpresa();
        e.CambiarPlan(CatalogoModulos.EdicionGestion, [CatalogoModulos.Contabilidad, CatalogoModulos.Ventas], Reloj).EsCorrecto.Should().BeTrue();
        e.Plan.Incluye(CatalogoModulos.Contabilidad).Should().BeTrue();
        // Ventas ya venía en la edición: no se guarda como adicional.
        e.ModulosAdicionales.Should().Equal(CatalogoModulos.Contabilidad);
    }

    [Fact]
    public void Un_modulo_sin_su_dependencia_se_rechaza_explicando_cual_falta()
    {
        var e = NuevaEmpresa();
        var r = e.CambiarPlan(CatalogoModulos.EdicionStart, [CatalogoModulos.Produccion, CatalogoModulos.Inmovilizado], Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("plan.dependencia");
        r.Error.Mensaje.Should().Contain("Producción necesita Inventario").And.Contain("Inmovilizado necesita Contabilidad");
        e.Plan.Edicion.Should().Be(CatalogoModulos.EdicionCompleta);   // no ha cambiado
    }

    [Fact]
    public void Ediciones_y_modulos_desconocidos_se_rechazan()
    {
        var e = NuevaEmpresa();
        e.CambiarPlan("premium", null, Reloj).Error.Codigo.Should().Be("plan.edicion_desconocida");
        e.CambiarPlan(CatalogoModulos.EdicionStart, ["nominas"], Reloj).Error.Codigo.Should().Be("plan.modulo_desconocido");
    }

    [Fact]
    public void Cambiar_de_plan_registra_un_evento_con_los_modulos_activos()
    {
        var e = NuevaEmpresa();
        e.CambiarPlan(CatalogoModulos.EdicionFinanzas, null, Reloj);
        var evento = e.EventosDominio.OfType<PlanEmpresaCambiado>().Single();
        evento.EdicionAnterior.Should().Be(CatalogoModulos.EdicionCompleta);
        evento.EdicionNueva.Should().Be(CatalogoModulos.EdicionFinanzas);
        evento.ModulosActivos.Should().Contain(CatalogoModulos.Contabilidad);
    }

    [Fact]
    public void Cada_edicion_del_catalogo_es_coherente_con_sus_dependencias()
    {
        foreach (var edicion in CatalogoModulos.Ediciones)
        {
            CatalogoModulos.Resolver(edicion.Codigo, null).EsCorrecto.Should().BeTrue($"la edición {edicion.Codigo} debe resolverse sin errores");
        }
    }
}
