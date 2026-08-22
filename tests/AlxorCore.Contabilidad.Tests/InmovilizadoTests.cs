using AlxorCore.Contabilidad.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class InmovilizadoTests
{
    private static readonly Guid Empresa = Guid.NewGuid();

    // ---- Calculadora de cuotas anuales ----

    [Fact]
    public void Lineal_reparte_la_base_en_cuotas_iguales()
    {
        var cuotas = CalculadoraAmortizacion.CuotasAnuales(9000m, new PlanAmortizacion(MetodoAmortizacion.Lineal, 3));
        cuotas.Should().Equal(3000m, 3000m, 3000m);
        cuotas.Sum().Should().Be(9000m);
    }

    [Fact]
    public void Digitos_reparte_de_forma_decreciente_por_suma_de_digitos()
    {
        var cuotas = CalculadoraAmortizacion.CuotasAnuales(9000m, new PlanAmortizacion(MetodoAmortizacion.Digitos, 3));
        // Σdígitos = 6; año1 = 9000·3/6, año2 = 9000·2/6, año3 = 9000·1/6.
        cuotas.Should().Equal(4500m, 3000m, 1500m);
        cuotas.Sum().Should().Be(9000m);
    }

    [Fact]
    public void Degresivo_aplica_porcentaje_sobre_el_saldo_pendiente_y_el_ultimo_ano_cierra()
    {
        var cuotas = CalculadoraAmortizacion.CuotasAnuales(9000m, new PlanAmortizacion(MetodoAmortizacion.Degresivo, 3, 50m));
        // 50% sobre pendiente: 4500, luego 2250, y el último año amortiza el resto (2250).
        cuotas.Should().Equal(4500m, 2250m, 2250m);
        cuotas.Sum().Should().Be(9000m);
    }

    [Fact]
    public void Todos_los_metodos_suman_exactamente_la_base_aunque_haya_redondeo()
    {
        foreach (var metodo in new[] { MetodoAmortizacion.Lineal, MetodoAmortizacion.Digitos, MetodoAmortizacion.Degresivo })
        {
            var cuotas = CalculadoraAmortizacion.CuotasAnuales(10000m, new PlanAmortizacion(metodo, 7, 30m));
            cuotas.Sum().Should().Be(10000m, "el método {0} debe amortizar la base completa", metodo);
        }
    }

    [Fact]
    public void El_plan_mensual_prorratea_desde_la_puesta_en_funcionamiento()
    {
        var cuotas = CalculadoraAmortizacion.CuotasAnuales(12000m, new PlanAmortizacion(MetodoAmortizacion.Lineal, 1));
        // Alta el 1 de abril de 2026: 9 meses en 2026, 3 meses en 2027.
        var contable2026 = CalculadoraAmortizacion.ImporteDeEjercicio(new DateOnly(2026, 4, 1), cuotas, 2026);
        var contable2027 = CalculadoraAmortizacion.ImporteDeEjercicio(new DateOnly(2026, 4, 1), cuotas, 2027);
        contable2026.Should().Be(9000m);  // 9 · 1000
        contable2027.Should().Be(3000m);  // 3 · 1000
        (contable2026 + contable2027).Should().Be(12000m);
    }

    // ---- Impuesto diferido ----

    [Fact]
    public void Impuesto_diferido_crea_pasivo_cuando_la_amortizacion_fiscal_supera_a_la_contable()
    {
        // Diferencia del año = fiscal − contable = +1000; tipo 25% → 250 al pasivo 479.
        var patas = CalculadoraAmortizacion.AsientoImpuestoDiferido(0m, 1000m, 0.25m, "479", "4740", "6301");
        patas.Should().Contain(p => p.CuentaCodigo == "479" && p.Haber == 250m);
        patas.Should().Contain(p => p.CuentaCodigo == "6301" && p.Debe == 250m);
        patas.Sum(p => p.Debe).Should().Be(patas.Sum(p => p.Haber));
    }

    [Fact]
    public void Impuesto_diferido_revierte_el_pasivo_en_anos_posteriores()
    {
        // Acumulada previa +1000 (pasivo 250 vivo). Este año contable>fiscal en 400 → diferencia −400.
        var patas = CalculadoraAmortizacion.AsientoImpuestoDiferido(1000m, -400m, 0.25m, "479", "4740", "6301");
        // El pasivo baja de 250 a 150 → se carga 479 por 100; contrapartida en 6301.
        patas.Should().Contain(p => p.CuentaCodigo == "479" && p.Debe == 100m);
        patas.Should().Contain(p => p.CuentaCodigo == "6301" && p.Haber == 100m);
        patas.Sum(p => p.Debe).Should().Be(patas.Sum(p => p.Haber));
    }

    [Fact]
    public void Sin_diferencia_no_hay_asiento_de_impuesto_diferido()
    {
        CalculadoraAmortizacion.AsientoImpuestoDiferido(500m, 0m, 0.25m, "479", "4740", "6301").Should().BeEmpty();
    }

    // ---- Entidad Inmovilizado ----

    private static Inmovilizado Crear(decimal valor = 12000m, decimal residual = 0m) =>
        Inmovilizado.Crear(Empresa, "INM-1", "Furgoneta", "218", "281", "681",
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1), valor, residual,
            PeriodicidadAmortizacion.Anual,
            new PlanAmortizacion(MetodoAmortizacion.Lineal, 4),
            new PlanAmortizacion(MetodoAmortizacion.Lineal, 2)).Valor;

    [Fact]
    public void Crear_calcula_base_amortizable_y_estado_inicial()
    {
        var inmo = Crear(12000m, 2000m);
        inmo.BaseAmortizable.Should().Be(10000m);
        inmo.Estado.Should().Be(EstadoInmovilizado.Activo);
        inmo.ValorNetoContable.Should().Be(12000m);
    }

    [Fact]
    public void El_valor_residual_no_puede_igualar_o_superar_la_adquisicion()
    {
        Inmovilizado.Crear(Empresa, "INM-2", "X", "218", "281", "681",
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 1), 1000m, 1000m,
            PeriodicidadAmortizacion.Anual,
            new PlanAmortizacion(MetodoAmortizacion.Lineal, 4),
            new PlanAmortizacion(MetodoAmortizacion.Lineal, 4)).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Registrar_dotaciones_acumula_amortizacion_y_reduce_el_valor_neto()
    {
        var inmo = Crear();
        inmo.RegistrarDotacion(2026, 0, new DateOnly(2026, 12, 31), 3000m, Guid.NewGuid());
        inmo.AmortizacionAcumulada.Should().Be(3000m);
        inmo.ValorNetoContable.Should().Be(9000m);
        inmo.DotacionExiste(2026, 0).Should().BeTrue();
        inmo.DotacionExiste(2027, 0).Should().BeFalse();
    }

    [Fact]
    public void No_se_puede_enajenar_ni_dar_de_baja_dos_veces()
    {
        var inmo = Crear();
        inmo.Enajenar(new DateOnly(2027, 6, 1), 5000m).EsCorrecto.Should().BeTrue();
        inmo.Estado.Should().Be(EstadoInmovilizado.Enajenado);
        inmo.DarDeBaja(new DateOnly(2027, 7, 1)).EsFallo.Should().BeTrue();
    }
}
