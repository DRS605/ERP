using AlxorCore.Inventario.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Inventario.Tests;

public sealed class ExistenciaTests
{
    private static readonly Guid Emp = Guid.NewGuid();

    [Fact]
    public void Aumentar_y_disminuir()
    {
        var e = Existencia.Nueva(Emp, Guid.NewGuid(), Guid.NewGuid(), null);
        e.Aumentar(10m);
        e.Cantidad.Should().Be(10m);
        e.Disminuir(3m).EsCorrecto.Should().BeTrue();
        e.Cantidad.Should().Be(7m);
    }

    [Fact]
    public void No_deja_stock_negativo()
    {
        var e = Existencia.Nueva(Emp, Guid.NewGuid(), Guid.NewGuid(), null);
        e.Aumentar(5m);
        var r = e.Disminuir(8m);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("existencia.insuficiente");
        e.Cantidad.Should().Be(5m);
    }

    [Fact]
    public void Fijar_ajusta_a_la_cantidad_contada()
    {
        var e = Existencia.Nueva(Emp, Guid.NewGuid(), Guid.NewGuid(), null);
        e.Aumentar(10m);
        e.Fijar(7.5m);
        e.Cantidad.Should().Be(7.5m);
    }

    [Fact]
    public void La_existencia_guarda_el_lote()
    {
        var e = Existencia.Nueva(Emp, Guid.NewGuid(), Guid.NewGuid(), null, "L-2026-01");
        e.Lote.Should().Be("L-2026-01");
        e.Aumentar(4m);
        e.Cantidad.Should().Be(4m);
    }

    [Fact]
    public void Almacen_y_ubicacion_validan()
    {
        Almacen.Crear(Emp, "", "Central").EsFallo.Should().BeTrue();
        Almacen.Crear(Emp, "ALM1", "Central").EsCorrecto.Should().BeTrue();
        Ubicacion.Crear(Emp, Guid.NewGuid(), "", null).EsFallo.Should().BeTrue();
        var u = Ubicacion.Crear(Emp, Guid.NewGuid(), "A-3", null).Valor;
        u.Nombre.Should().Be("A-3");
    }
}
