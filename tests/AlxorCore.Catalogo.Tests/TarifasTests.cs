using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Catalogo.Tests;

/// <summary>Pruebas de la regla de precios de las tarifas de venta.</summary>
public class TarifasTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Producto = Guid.NewGuid();
    private static readonly Guid Bebidas = Guid.NewGuid();
    private static readonly Guid Refrescos = Guid.NewGuid();     // subfamilia de Bebidas
    private static readonly DateOnly Hoy = new(2026, 9, 26);

    private static Tarifa Nueva(params DatosLineaTarifa[] lineas) =>
        Tarifa.Crear(Guid.NewGuid(), "mayor", "Mayoristas", lineas, Reloj).Valor;

    /// <summary>Producto de 10 € de la subfamilia Refrescos (que cuelga de Bebidas).</summary>
    private static SolicitudPrecio Pedir(decimal cantidad = 1, DateOnly? fecha = null) =>
        new(Producto, [Refrescos, Bebidas], 10m, cantidad, fecha ?? Hoy);

    [Fact]
    public void Un_descuento_general_se_aplica_a_cualquier_producto_con_su_precio()
    {
        var p = Nueva(new DatosLineaTarifa(PorcentajeDescuento: 5)).Resolver(Pedir());
        p.Should().Be(new PrecioTarifa(10m, 5m, "Tarifa MAYOR (general)"));
    }

    [Fact]
    public void Gana_lo_mas_especifico_producto_antes_que_familia_y_familia_cercana_antes_que_lejana()
    {
        var t = Nueva(
            new DatosLineaTarifa(PorcentajeDescuento: 5),
            new DatosLineaTarifa(FamiliaId: Bebidas, PorcentajeDescuento: 10),
            new DatosLineaTarifa(FamiliaId: Refrescos, PorcentajeDescuento: 12));
        t.Resolver(Pedir())!.PorcentajeDescuento.Should().Be(12m);          // la subfamilia es la más cercana

        var conPrecio = Nueva(
            new DatosLineaTarifa(FamiliaId: Refrescos, PorcentajeDescuento: 12),
            new DatosLineaTarifa(ProductoId: Producto, Precio: 8.5m));
        conPrecio.Resolver(Pedir()).Should().Be(new PrecioTarifa(8.5m, 0m, "Tarifa MAYOR (precio del producto)"));
    }

    [Fact]
    public void El_descuento_de_una_familia_alcanza_a_sus_subfamilias()
    {
        Nueva(new DatosLineaTarifa(FamiliaId: Bebidas, PorcentajeDescuento: 10)).Resolver(Pedir())!.PorcentajeDescuento.Should().Be(10m);
    }

    [Fact]
    public void Escalado_por_cantidad_aplica_el_tramo_mayor_alcanzado()
    {
        var t = Nueva(
            new DatosLineaTarifa(ProductoId: Producto, Precio: 9.5m),
            new DatosLineaTarifa(ProductoId: Producto, CantidadMinima: 100, Precio: 9m),
            new DatosLineaTarifa(ProductoId: Producto, CantidadMinima: 500, Precio: 8.25m));
        t.Resolver(Pedir(10))!.PrecioUnitario.Should().Be(9.5m);
        t.Resolver(Pedir(100))!.PrecioUnitario.Should().Be(9m);
        t.Resolver(Pedir(750)).Should().Be(new PrecioTarifa(8.25m, 0m, "Tarifa MAYOR (precio del producto, desde 500 uds)"));
    }

    [Fact]
    public void Solo_cuentan_las_lineas_vigentes_y_la_tarifa_activa()
    {
        var t = Nueva(new DatosLineaTarifa(PorcentajeDescuento: 15, Desde: new DateOnly(2026, 11, 1), Hasta: new DateOnly(2026, 11, 30)));
        t.Resolver(Pedir(fecha: new DateOnly(2026, 10, 31))).Should().BeNull();
        t.Resolver(Pedir(fecha: new DateOnly(2026, 11, 30)))!.PorcentajeDescuento.Should().Be(15m);   // extremo incluido

        t.Actualizar("Mayoristas", false, [new DatosLineaTarifa(PorcentajeDescuento: 5)], Reloj);
        t.Resolver(Pedir()).Should().BeNull();
    }

    [Fact]
    public void Rechaza_lineas_incoherentes_con_un_mensaje_que_dice_cual()
    {
        Tarifa.Crear(Guid.NewGuid(), "X", "X", [new DatosLineaTarifa(ProductoId: Producto, FamiliaId: Bebidas, PorcentajeDescuento: 5)], Reloj)
            .Error.Codigo.Should().Be("tarifa.linea_ambito");
        Tarifa.Crear(Guid.NewGuid(), "X", "X", [new DatosLineaTarifa(PorcentajeDescuento: 120)], Reloj)
            .Error.Mensaje.Should().Be("La línea 1 tiene un descuento fuera de 0-100 %.");
        Tarifa.Crear(Guid.NewGuid(), "X", "X", [new DatosLineaTarifa(ProductoId: Producto)], Reloj)
            .Error.Codigo.Should().Be("tarifa.linea_vacia");
        Tarifa.Crear(Guid.NewGuid(), "X", "X", [new DatosLineaTarifa(Precio: 1, Desde: new DateOnly(2026, 2, 1), Hasta: new DateOnly(2026, 1, 1))], Reloj)
            .Error.Codigo.Should().Be("tarifa.linea_fechas");
    }

    [Fact]
    public void No_admite_dos_lineas_para_lo_mismo_con_fechas_solapadas()
    {
        var r = Tarifa.Crear(Guid.NewGuid(), "X", "X",
        [
            new DatosLineaTarifa(ProductoId: Producto, Precio: 9, Desde: new DateOnly(2026, 1, 1), Hasta: new DateOnly(2026, 6, 30)),
            new DatosLineaTarifa(ProductoId: Producto, Precio: 8, Desde: new DateOnly(2026, 6, 1)),
        ], Reloj);
        r.Error.Mensaje.Should().Be("La línea 2 repite ámbito y cantidad mínima de la línea 1 con fechas que se solapan.");

        // Sin solape (una detrás de otra) sí vale: así se programa un cambio de precio.
        Tarifa.Crear(Guid.NewGuid(), "X", "X",
        [
            new DatosLineaTarifa(ProductoId: Producto, Precio: 9, Hasta: new DateOnly(2026, 6, 30)),
            new DatosLineaTarifa(ProductoId: Producto, Precio: 8, Desde: new DateOnly(2026, 7, 1)),
        ], Reloj).EsCorrecto.Should().BeTrue();
    }
}
