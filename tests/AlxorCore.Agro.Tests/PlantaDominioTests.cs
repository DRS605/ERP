using AlxorCore.Agro.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

public sealed class PlantaDominioTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly DateOnly Dia = new(2026, 3, 10);
    private static readonly DateTimeOffset Ahora = new(2026, 3, 10, 8, 0, 0, TimeSpan.Zero);

    private static LineaPlanta Calibradora(Guid categoria) => LineaPlanta.Crear(Empresa, new DatosLineaPlanta("cal", "Calibradora", TipoLineaPlanta.Calibradora, 8_000m,
        Salidas: [new DatosSalidaCalibradora(1, "3", categoria), new DatosSalidaCalibradora(2, "3", categoria), new DatosSalidaCalibradora(3, Destrio: true)])).Valor;

    [Fact]
    public void La_linea_valida_capacidad_turnos_y_salidas()
    {
        LineaPlanta.Crear(Empresa, new DatosLineaPlanta("L1", "Línea", TipoLineaPlanta.Confeccion, 1_000m, 9m, 3)).Error.Codigo.Should().Be("linea_planta.capacidad");
        LineaPlanta.Crear(Empresa, new DatosLineaPlanta("L1", "Línea", TipoLineaPlanta.Confeccion, 1_000m, Salidas: [new DatosSalidaCalibradora(1, "3")]))
            .Error.Codigo.Should().Be("linea_planta.salidas");
        LineaPlanta.Crear(Empresa, new DatosLineaPlanta("CAL", "Cal", TipoLineaPlanta.Calibradora, 1_000m, Salidas: [new DatosSalidaCalibradora(1)]))
            .Error.Codigo.Should().Be("linea_planta.salidas");
        var l = LineaPlanta.Crear(Empresa, new DatosLineaPlanta(" l1 ", "Línea", TipoLineaPlanta.Confeccion, 1_250m, 7.5m, 2)).Valor;
        l.Codigo.Should().Be("L1");
        l.CapacidadDia.Should().Be(18_750m);
    }

    [Fact]
    public void El_calibrado_suma_las_salidas_iguales_y_da_la_muestra_por_categoria()
    {
        var extra = Guid.NewGuid();
        var linea = Calibradora(extra);
        var c = Calibrado.Crear(Empresa, linea, Guid.NewGuid(), 1_000m,
            new DatosCalibrado(Dia, [new DatosLineaCalibrado(400m, 1, Piezas: 2_000), new DatosLineaCalibrado(200m, 2, Piezas: 1_000), new DatosLineaCalibrado(50m, 3)]), Ahora).Valor;
        c.Lineas.Should().HaveCount(2, "las salidas 1 y 2 recogen lo mismo");
        c.Lineas.Single(l => l.Calibre == "3").Should().Match<LineaCalibrado>(l => l.Kilos == 600m && l.Piezas == 3_000 && l.GramosPieza == 200m);
        c.KilosEntrada.Should().Be(650m, "sin kilos de entrada, entra lo que sale");
        c.PorCategoria().Should().BeEquivalentTo([(extra, 600m)]);

        c.Confirmar(null).EsCorrecto.Should().BeTrue();
        c.Cambiar(linea, 1_000m, new DatosCalibrado(Dia, [new DatosLineaCalibrado(1m, 1)])).Error.Codigo.Should().Be("calibrado.no_borrador");
        c.Anular("repetido").EsCorrecto.Should().BeTrue();
        c.Anular("otra vez").Error.Codigo.Should().Be("calibrado.anulado");

        var confeccion = LineaPlanta.Crear(Empresa, new DatosLineaPlanta("L1", "Línea", TipoLineaPlanta.Confeccion, 1_000m)).Valor;
        Calibrado.Crear(Empresa, confeccion, Guid.NewGuid(), 1_000m, new DatosCalibrado(Dia, [new DatosLineaCalibrado(1m, Calibre: "3")]), Ahora)
            .Error.Codigo.Should().Be("calibrado.linea");
    }

    [Fact]
    public void La_orden_solo_se_mueve_planificada_y_sigue_sus_transiciones()
    {
        var linea = LineaPlanta.Crear(Empresa, new DatosLineaPlanta("L1", "Línea", TipoLineaPlanta.Confeccion, 1_000m, 8m, 2)).Valor;
        var o = OrdenLinea.Crear(Empresa, linea, new DatosOrdenLinea(Dia, Guid.NewGuid(), 5_000m, 2), Ahora).Valor;
        o.Cambiar(linea, new DatosOrdenLinea(Dia, o.ProductoId, 5_000m, 3)).Error.Codigo.Should().Be("orden_linea.turno");
        o.Iniciar().EsCorrecto.Should().BeTrue();
        o.Cambiar(linea, new DatosOrdenLinea(Dia.AddDays(1), o.ProductoId, 5_000m)).Error.Codigo.Should().Be("orden_linea.no_planificada");
        o.Terminar(Guid.NewGuid()).EsCorrecto.Should().BeTrue();
        o.Cancelar().Error.Codigo.Should().Be("orden_linea.estado");
        o.Reabrir().EsCorrecto.Should().BeTrue();
        o.ParteConfeccionId.Should().BeNull();

        linea.FijarActiva(false);
        OrdenLinea.Crear(Empresa, linea, new DatosOrdenLinea(Dia, Guid.NewGuid(), 1m), Ahora).Error.Codigo.Should().Be("orden_linea.linea_baja");
        ParadaLinea.Crear(Empresa, linea, new DatosParada(Dia, 481m, MotivoParada.Averia)).Error.Codigo.Should().Be("parada.minutos");
    }
}
