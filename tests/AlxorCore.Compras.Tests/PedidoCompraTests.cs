using AlxorCore.Compras.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Compras.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);
}

public sealed class PedidoCompraTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Fecha = new(2026, 8, 1);

    private static PedidoCompra Pedido() =>
        PedidoCompra.Crear(Empresa, null, "Suministros Ebro SL", Fecha, null,
            new[] { ("Tornillos", 10m, 2m), ("Tuercas", 20m, 1m) }, Reloj).Valor;

    [Fact]
    public void Crear_calcula_el_total()
    {
        var p = Pedido();
        p.Total.Should().Be(40m); // 10*2 + 20*1
        p.Estado.Should().Be(EstadoPedido.Borrador);
    }

    [Fact]
    public void No_se_recibe_sin_confirmar()
    {
        var p = Pedido();
        var linea = p.Lineas[0].Id;
        p.RegistrarRecepcion(new[] { (linea, 5m) }).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Recepcion_parcial_y_total()
    {
        var p = Pedido();
        p.Confirmar().EsCorrecto.Should().BeTrue();
        var l1 = p.Lineas[0].Id;
        var l2 = p.Lineas[1].Id;

        p.RegistrarRecepcion(new[] { (l1, 4m) }).EsCorrecto.Should().BeTrue();
        p.Estado.Should().Be(EstadoPedido.Recibido);
        p.RecibidoCompleto.Should().BeFalse();

        // No se puede recibir más de lo pedido.
        p.RegistrarRecepcion(new[] { (l1, 100m) }).EsFallo.Should().BeTrue();

        p.RegistrarRecepcion(new[] { (l1, 6m), (l2, 20m) }).EsCorrecto.Should().BeTrue();
        p.RecibidoCompleto.Should().BeTrue();
    }

    [Fact]
    public void Facturar_marca_facturado_y_devuelve_el_total()
    {
        var p = Pedido();
        p.Confirmar();
        var r = p.Facturar();
        r.EsCorrecto.Should().BeTrue();
        r.Valor.Should().Be(40m);
        p.Estado.Should().Be(EstadoPedido.Facturado);
        p.Facturar().EsFallo.Should().BeTrue(); // no dos veces
        p.Cancelar().EsFallo.Should().BeTrue(); // no cancelar facturado
    }

    [Fact]
    public void Solicitud_aprobar_y_convertir()
    {
        var s = SolicitudCompra.Crear(Empresa, "Ebro", null, new[] { ("Material", 5m) }, Reloj).Valor;
        s.Estado.Should().Be(EstadoSolicitud.Borrador);
        s.MarcarConvertida().EsFallo.Should().BeTrue(); // hay que aprobar antes
        s.Aprobar().EsCorrecto.Should().BeTrue();
        s.MarcarConvertida().EsCorrecto.Should().BeTrue();
        s.Estado.Should().Be(EstadoSolicitud.Convertida);
    }
}
