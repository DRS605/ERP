using AlxorCore.Contabilidad.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class FinanciacionTests
{
    private static readonly DateOnly Primera = new(2026, 4, 15);

    [Fact]
    public void El_sistema_frances_tiene_cuota_constante_y_devuelve_todo_el_capital()
    {
        var cuadro = CalculadoraPrestamo.Calcular(120_000m, 3m, PeriodicidadCuota.Mensual, 60, 0, Primera, SistemaAmortizacionPrestamo.Frances, 0m);

        cuadro.Should().HaveCount(60);
        cuadro.Take(59).Should().OnlyContain(c => c.Cuota == 2_156.24m);
        cuadro[0].Intereses.Should().Be(300m, "120.000 × 3 % / 12");
        cuadro.Sum(c => c.Capital).Should().Be(120_000m);
        cuadro[^1].Pendiente.Should().Be(0m);
        cuadro[^1].Fecha.Should().Be(new DateOnly(2031, 3, 15));
        cuadro.Zip(cuadro.Skip(1)).Should().OnlyContain(p => p.Second.Capital >= p.First.Capital, "el capital crece y los intereses bajan");
    }

    [Fact]
    public void Con_capital_constante_y_carencia_las_primeras_cuotas_solo_pagan_intereses()
    {
        var cuadro = CalculadoraPrestamo.Calcular(12_000m, 6m, PeriodicidadCuota.Trimestral, 6, 2, Primera, SistemaAmortizacionPrestamo.CapitalConstante, 0m);

        cuadro.Select(c => c.Capital).Should().Equal(0m, 0m, 3_000m, 3_000m, 3_000m, 3_000m);
        cuadro[0].Intereses.Should().Be(180m, "12.000 × 6 % × 3/12");
        cuadro[3].Intereses.Should().Be(135m, "quedan 9.000");
        cuadro.Select(c => c.Fecha.Month).Should().Equal(4, 7, 10, 1, 4, 7);
    }

    [Fact]
    public void El_leasing_deja_la_opcion_de_compra_un_periodo_despues_de_la_ultima_cuota()
    {
        var cuadro = CalculadoraPrestamo.Calcular(30_000m, 3m, PeriodicidadCuota.Mensual, 36, 0, Primera, SistemaAmortizacionPrestamo.Frances, 1_000m);

        cuadro.Should().HaveCount(37);
        cuadro[0].Cuota.Should().Be(845.86m);
        cuadro[35].Pendiente.Should().Be(1_000m);
        cuadro[^1].Should().Be(new CuotaCuadro(37, new DateOnly(2029, 4, 15), 0m, 1_000m, 0m, OpcionCompra: true));
        cuadro.Sum(c => c.Capital).Should().Be(30_000m);
    }

    [Fact]
    public void Una_revision_o_un_anticipo_recalculan_la_cuota_de_lo_que_queda_sin_cambiar_el_plazo()
    {
        var base_ = CalculadoraPrestamo.Calcular(120_000m, 3m, PeriodicidadCuota.Mensual, 60, 0, Primera, SistemaAmortizacionPrestamo.Frances, 0m);
        var revisado = CalculadoraPrestamo.Calcular(120_000m, 3m, PeriodicidadCuota.Mensual, 60, 0, Primera, SistemaAmortizacionPrestamo.Frances, 0m,
            [new RevisionCuadro(13, 4m)]);
        revisado.Take(12).Should().Equal(base_.Take(12));
        revisado[12].Cuota.Should().BeGreaterThan(base_[12].Cuota);
        revisado[12].Intereses.Should().Be(decimal.Round(base_[11].Pendiente * 0.04m / 12m, 2, MidpointRounding.AwayFromZero));
        revisado.Should().HaveCount(60);
        revisado.Sum(c => c.Capital).Should().Be(120_000m);

        var anticipado = CalculadoraPrestamo.Calcular(120_000m, 3m, PeriodicidadCuota.Mensual, 60, 0, Primera, SistemaAmortizacionPrestamo.Frances, 0m,
            anticipos: [new AnticipoCuadro(12, 20_000m)]);
        anticipado.Should().HaveCount(60);
        anticipado[12].Cuota.Should().BeLessThan(base_[12].Cuota);
        anticipado.Sum(c => c.Capital).Should().Be(100_000m, "los 20.000 anticipados no van en cuotas");

        var cancelado = CalculadoraPrestamo.Calcular(120_000m, 3m, PeriodicidadCuota.Mensual, 60, 0, Primera, SistemaAmortizacionPrestamo.Frances, 0m,
            anticipos: [new AnticipoCuadro(12, base_[11].Pendiente)]);
        cancelado.Should().HaveCount(12);
    }

    [Fact]
    public void La_operacion_reparte_el_capital_pendiente_entre_corto_y_largo_plazo_por_el_ano_de_cada_cuota()
    {
        var op = OperacionFinanciacion.Crear(Guid.NewGuid(), TipoFinanciacion.Prestamo, "p-1", "Préstamo ICO", "Banco", new DateOnly(2026, 3, 15), 120_000m, 3m,
            SistemaAmortizacionPrestamo.Frances, PeriodicidadCuota.Mensual, 60, 0, null, 0m, 0m, null, 0m, null, null, null, null, null, null, null).Valor;

        op.Codigo.Should().Be("P-1");
        op.FechaPrimeraCuota.Should().Be(Primera, "por defecto, un periodo después de la formalización");
        op.CuentaLargoPlazo.Should().Be("170");
        op.CuentaCortoPlazo.Should().Be("520");
        var (corto, largo) = op.Reparto();
        corto.Should().Be(op.Cuadro().Where(c => c.Fecha.Year == 2026).Sum(c => c.Capital), "al formalizar, el corto plazo son las cuotas de este año");
        (corto + largo).Should().Be(120_000m);

        op.AnotarReclasificacion(2026, 0m, null);
        op.HorizonteCortoPlazo.Should().Be(2027);
        op.Reparto().Corto.Should().Be(op.Cuadro().Where(c => c.Fecha.Year <= 2027).Sum(c => c.Capital));
    }

    [Fact]
    public void Las_condiciones_se_validan_por_tipo()
    {
        OperacionFinanciacion Crear(TipoFinanciacion tipo, decimal residual = 0m, decimal iva = 0m, string? activo = null, DateOnly? vto = null) =>
            OperacionFinanciacion.Crear(Guid.NewGuid(), tipo, "X", "X", null, new DateOnly(2026, 1, 1), 10_000m, 3m, SistemaAmortizacionPrestamo.Frances,
                PeriodicidadCuota.Mensual, 12, 0, null, residual, iva, vto, 0m, null, null, null, null, null, null, activo) is { EsCorrecto: true } r ? r.Valor : null!;

        Crear(TipoFinanciacion.Prestamo, residual: 100m).Should().BeNull("el valor residual es del leasing");
        Crear(TipoFinanciacion.Leasing, iva: 21m).Should().BeNull("el leasing necesita la cuenta del bien");
        Crear(TipoFinanciacion.Leasing, iva: 21m, activo: "218").CuentaLargoPlazo.Should().Be("174");
        Crear(TipoFinanciacion.Poliza).Should().BeNull("la póliza necesita vencimiento");
        Crear(TipoFinanciacion.Poliza, vto: new DateOnly(2027, 1, 1)).CuentaCortoPlazo.Should().Be("5201");
    }
}
