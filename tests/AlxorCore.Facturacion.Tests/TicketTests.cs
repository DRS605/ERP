using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Facturacion.Tests;

public class TicketTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Fecha = new(2026, 1, 15);

    private static NumeroFactura Numero() => new("T", 2026, 1);
    private static NuevaLinea Linea(decimal cantidad, decimal precio, decimal iva = 21m) =>
        new("Artículo", cantidad, precio, "IVA21", iva);

    [Fact]
    public void EmitirSimplificada_sin_cliente_usa_contado_y_marca_tipo()
    {
        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [Linea(2m, 10m)], Reloj);

        t.EsCorrecto.Should().BeTrue();
        t.Valor.TipoFactura.Should().Be(TipoFactura.Simplificada);
        t.Valor.ClienteId.Should().BeNull();
        t.Valor.ClienteNombre.Should().Be("Cliente de contado");
        t.Valor.NumeroCompleto.Should().Be("T2026/000001");
    }

    [Fact]
    public void EmitirSimplificada_calcula_base_iva_y_total_sin_irpf()
    {
        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [Linea(2m, 10m)], Reloj).Valor;

        t.BaseImponible.Should().Be(20m);
        t.CuotaIva.Should().Be(4.20m);
        t.RetencionIrpf.Should().Be(0m);
        t.Total.Should().Be(24.20m);
    }

    [Fact]
    public void EmitirSimplificada_con_iva_incluido_el_total_cuadra_con_los_precios_de_estanteria()
    {
        // Precios de venta CON IVA (lo que ve el cliente): 1,35 (10%) + 1,00 (21%) + 1,15 (21%) = 3,50.
        // El precio unitario es la base (1,35/1,10=1,2273 -> 1,23 redondeado, etc.). El total debe ser
        // exactamente 3,50, no 3,51 (el "céntimo fantasma" del cálculo de abajo a arriba).
        NuevaLinea LineaInc(decimal precioBase, decimal iva, string codigo) =>
            new("Artículo", 1m, precioBase, codigo, iva, PreciosConIvaIncluido: true);

        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [LineaInc(1.23m, 10m, "IVA10"), LineaInc(0.83m, 21m, "IVA21"), LineaInc(0.95m, 21m, "IVA21")], Reloj).Valor;

        t.BaseImponible.Should().Be(3.01m);
        t.CuotaIva.Should().Be(0.49m);
        t.Total.Should().Be(3.50m);
    }

    [Fact]
    public void EmitirSimplificada_con_iva_incluido_dos_unidades_del_mismo_precio_no_descuadran()
    {
        // 2 uds. de un artículo cuyo precio de estantería es 1,35 € (base 1,23 + 10%): el total debe ser
        // 2,70 €, no 2,71 €. El precio unitario con IVA se redondea antes de multiplicar por la cantidad.
        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [new("Aceitunas", 2m, 1.23m, "IVA10", 10m, PreciosConIvaIncluido: true)], Reloj).Valor;

        t.Total.Should().Be(2.70m);
        t.BaseImponible.Should().Be(2.45m);
        t.CuotaIva.Should().Be(0.25m);
    }

    [Fact]
    public void EmitirSimplificada_una_linea_con_iva_incluido_no_cambia_el_precio_de_venta()
    {
        // Un solo artículo a 1,00 € con IVA 21%: el ticket debe seguir siendo 1,00 € (base 0,83 + IVA 0,17).
        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [new("Refresco", 1m, 0.826446m, "IVA21", 21m, PreciosConIvaIncluido: true)], Reloj).Valor;

        t.Total.Should().Be(1.00m);
        t.BaseImponible.Should().Be(0.83m);
        t.CuotaIva.Should().Be(0.17m);
    }

    [Fact]
    public void EmitirSimplificada_rechaza_importe_por_encima_del_tope()
    {
        // 3.000 € de base + IVA supera con creces el tope de 3.000 € de total.
        var t = Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado,
            [Linea(1m, 3000m)], Reloj);

        t.EsFallo.Should().BeTrue();
        t.Error.Codigo.Should().Be("ticket.importe_excedido");
    }

    [Fact]
    public void EmitirSimplificada_rechaza_sin_lineas()
    {
        Factura.EmitirSimplificada(Guid.NewGuid(), Numero(), Fecha, ClienteFacturado.Contado, [], Reloj)
            .EsFallo.Should().BeTrue();
    }
}
