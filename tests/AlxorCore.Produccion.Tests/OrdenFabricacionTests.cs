using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Produccion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Produccion.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);
}

public class OrdenFabricacionTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateOnly Fecha = new(2026, 8, 1);

    private static OrdenFabricacion Orden(decimal cantidad = 10m) =>
        OrdenFabricacion.Crear(Empresa, 1, Guid.NewGuid(), "Conjunto", cantidad, Guid.NewGuid(), Fecha,
            new[] { (Guid.NewGuid(), "Pieza A", 2m), (Guid.NewGuid(), "Pieza B", 3m) }, Reloj).Valor;

    [Fact]
    public void Crear_explosiona_la_lista_de_materiales()
    {
        var o = Orden(10m);
        o.Estado.Should().Be(EstadoOrdenFabricacion.Planificada);
        o.Ejercicio.Should().Be(2026);
        o.Componentes.Should().HaveCount(2);
        o.Componentes.Single(c => c.Nombre == "Pieza A").CantidadTotal.Should().Be(20m); // 2 × 10
        o.Componentes.Single(c => c.Nombre == "Pieza B").CantidadTotal.Should().Be(30m); // 3 × 10
    }

    [Fact]
    public void Rechaza_cantidad_no_positiva_y_sin_componentes()
    {
        OrdenFabricacion.Crear(Empresa, 1, Guid.NewGuid(), "X", 0m, Guid.NewGuid(), Fecha, new[] { (Guid.NewGuid(), "A", 1m) }, Reloj).EsFallo.Should().BeTrue();
        OrdenFabricacion.Crear(Empresa, 1, Guid.NewGuid(), "X", 5m, Guid.NewGuid(), Fecha, Array.Empty<(Guid, string, decimal)>(), Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Ciclo_de_estados_planificada_encurso_terminada()
    {
        var o = Orden();
        o.Terminar(Reloj.AhoraUtc).EsCorrecto.Should().BeTrue(); // se puede terminar directamente
        o.Estado.Should().Be(EstadoOrdenFabricacion.Terminada);
        o.TerminadaEn.Should().NotBeNull();
        o.Terminar(Reloj.AhoraUtc).EsFallo.Should().BeTrue(); // no dos veces
        o.Cancelar().EsFallo.Should().BeTrue();               // no cancelar terminada
    }

    [Fact]
    public void Iniciar_solo_desde_planificada()
    {
        var o = Orden();
        o.Iniciar().EsCorrecto.Should().BeTrue();
        o.Estado.Should().Be(EstadoOrdenFabricacion.EnCurso);
        o.Iniciar().EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Cancelar_una_planificada()
    {
        var o = Orden();
        o.Cancelar().EsCorrecto.Should().BeTrue();
        o.Estado.Should().Be(EstadoOrdenFabricacion.Cancelada);
        o.Terminar(Reloj.AhoraUtc).EsFallo.Should().BeTrue();
    }
}
