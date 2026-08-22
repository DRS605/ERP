using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Proyectos.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Proyectos.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);
}

public class ProyectoTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateOnly Fecha = new(2026, 3, 1);

    private static Proyecto Nuevo(decimal presupuesto = 1000m) =>
        Proyecto.Crear(Empresa, 1, "Reforma local", null, null, presupuesto, Fecha, Reloj).Valor;

    [Fact]
    public void Crear_arranca_abierto_sin_coste()
    {
        var p = Nuevo(1000m);
        p.Estado.Should().Be(EstadoProyecto.Abierto);
        p.Ejercicio.Should().Be(2026);
        p.CosteReal.Should().Be(0m);
        p.Desviacion.Should().Be(1000m);
    }

    [Fact]
    public void Imputaciones_suman_coste_real_y_subtotales_por_tipo()
    {
        var p = Nuevo(1000m);
        var persona = Guid.NewGuid();
        var articulo = Guid.NewGuid();

        p.ImputarManoObra(persona, "Ana", 10m, 20m, Fecha, Reloj).EsCorrecto.Should().BeTrue();   // 200
        p.ImputarMaterial(articulo, "Cemento", 5m, 12.5m, Fecha, Reloj).EsCorrecto.Should().BeTrue(); // 62.5
        p.ImputarGasto("Dietas", 37.5m, Fecha, Reloj).EsCorrecto.Should().BeTrue();               // 37.5

        p.CosteManoObra.Should().Be(200m);
        p.CosteMateriales.Should().Be(62.5m);
        p.CosteGastos.Should().Be(37.5m);
        p.CosteReal.Should().Be(300m);
        p.Desviacion.Should().Be(700m);          // 1000 − 300
    }

    [Fact]
    public void Desviacion_negativa_si_se_supera_el_presupuesto()
    {
        var p = Nuevo(100m);
        p.ImputarGasto("Material", 150m, Fecha, Reloj);
        p.Desviacion.Should().Be(-50m);
    }

    [Fact]
    public void Rechaza_imputaciones_invalidas()
    {
        var p = Nuevo();
        p.ImputarManoObra(Guid.NewGuid(), "Ana", 0m, 20m, Fecha, Reloj).EsFallo.Should().BeTrue();    // horas 0
        p.ImputarMaterial(Guid.NewGuid(), "X", -1m, 5m, Fecha, Reloj).EsFallo.Should().BeTrue();       // cantidad < 0
        p.ImputarGasto("  ", 10m, Fecha, Reloj).EsFallo.Should().BeTrue();                             // concepto vacío
    }

    [Fact]
    public void No_se_imputa_ni_edita_un_proyecto_cerrado_y_reabrir_lo_permite()
    {
        var p = Nuevo();
        p.Cerrar(Reloj);
        p.Estado.Should().Be(EstadoProyecto.Cerrado);
        p.ImputarGasto("Tarde", 10m, Fecha, Reloj).EsFallo.Should().BeTrue();

        p.Reabrir(Reloj);
        p.ImputarGasto("Ahora sí", 10m, Fecha, Reloj).EsCorrecto.Should().BeTrue();
    }

    [Fact]
    public void Eliminar_imputacion_recalcula_el_coste()
    {
        var p = Nuevo(500m);
        var r = p.ImputarGasto("Borrable", 100m, Fecha, Reloj);
        p.CosteReal.Should().Be(100m);

        p.EliminarImputacion(r.Valor.Id, Reloj).EsCorrecto.Should().BeTrue();
        p.CosteReal.Should().Be(0m);
        p.EliminarImputacion(Guid.NewGuid(), Reloj).EsFallo.Should().BeTrue();  // inexistente
    }

    [Fact]
    public void Proyecto_cancelado_no_se_edita_ni_reabre()
    {
        var p = Nuevo();
        p.Cancelar(Reloj);
        p.ActualizarDatos("Otro", null, null, 50m, Reloj).EsFallo.Should().BeTrue();
        p.Reabrir(Reloj).EsFallo.Should().BeTrue();
    }
}
