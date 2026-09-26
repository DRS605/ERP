using AlxorCore.Contabilidad.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

/// <summary>Motor de la contabilidad analítica: reparto al céntimo, reglas y claves.</summary>
public class AnaliticaTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly Guid A = Guid.NewGuid();
    private static readonly Guid B = Guid.NewGuid();
    private static readonly Guid C = Guid.NewGuid();

    [Theory]
    [InlineData(100, new[] { 33.33, 33.33, 33.34 }, new[] { 33.33, 33.33, 33.34 })]
    [InlineData(10, new[] { 33.33, 33.33, 33.34 }, new[] { 3.33, 3.33, 3.34 })]
    [InlineData(0.01, new[] { 50.0, 50.0 }, new[] { 0.01, 0.00 })]
    [InlineData(-100, new[] { 60.0, 40.0 }, new[] { -60.0, -40.0 })]
    [InlineData(1000, new[] { 50.0 }, new[] { 500.0 })]
    public void El_reparto_es_exacto_al_centimo(double importe, double[] porcentajes, double[] esperado)
    {
        var partes = MotorAnalitico.Repartir((decimal)importe, porcentajes.Select(p => (decimal)p).ToList());
        partes.Should().Equal(esperado.Select(e => (decimal)e));
        partes.Sum().Should().Be(decimal.Round((decimal)importe * porcentajes.Sum(p => (decimal)p) / 100m, 2));
    }

    [Fact]
    public void El_reparto_de_un_centimo_impar_no_pierde_nada()
    {
        var partes = MotorAnalitico.Repartir(100.01m, [33.33m, 33.33m, 33.34m]);
        partes.Sum().Should().Be(100.01m);
    }

    [Theory]
    [InlineData("6290000", 100, 0, 100)]
    [InlineData("629", 0, 30, -30)]      // abono en un gasto: menos coste
    [InlineData("705", 0, 250, 250)]
    [InlineData("708", 40, 0, -40)]      // devolución de ventas: menos ingreso
    public void El_importe_natural_da_el_signo_de_gestion(string cuenta, decimal debe, decimal haber, decimal esperado) =>
        MotorAnalitico.ImporteNatural(cuenta, debe, haber).Should().Be(esperado);

    [Theory]
    [InlineData("629", NaturalezaAnalitica.Gasto)]
    [InlineData("700", NaturalezaAnalitica.Ingreso)]
    [InlineData("572", null)]
    [InlineData("", null)]
    public void Solo_se_imputan_gastos_e_ingresos(string cuenta, NaturalezaAnalitica? esperado) =>
        MotorAnalitico.NaturalezaDe(cuenta).Should().Be(esperado);

    [Fact]
    public void Gana_la_regla_mas_concreta_y_la_prioridad_manda()
    {
        var proveedor = Guid.NewGuid();
        var porCuenta = Regla(new(null, "629", null, null, null, null, null, A, null, null));
        var porCuentaLarga = Regla(new(null, "6291", null, null, null, null, null, B, null, null));
        var porProveedor = Regla(new(null, null, proveedor, null, null, null, null, C, null, null));
        var reglas = new[] { porCuenta, porCuentaLarga, porProveedor };

        MotorAnalitico.ReglaQueAplica(reglas, new ContextoImputacion("6290001", Hoy)).Should().Be(porCuenta);
        MotorAnalitico.ReglaQueAplica(reglas, new ContextoImputacion("6291001", Hoy)).Should().Be(porCuentaLarga);
        MotorAnalitico.ReglaQueAplica(reglas, new ContextoImputacion("6291001", Hoy, proveedor)).Should().Be(porProveedor, "el tercero es más concreto que la cuenta");
        MotorAnalitico.ReglaQueAplica(reglas, new ContextoImputacion("700", Hoy)).Should().BeNull();

        var prioritaria = Regla(new(null, "6", null, null, null, null, null, A, null, null, Prioridad: 10));
        MotorAnalitico.ReglaQueAplica([.. reglas, prioritaria], new ContextoImputacion("6291001", Hoy, proveedor)).Should().Be(prioritaria);
    }

    [Fact]
    public void Una_regla_solo_aplica_en_su_vigencia()
    {
        var regla = Regla(new(null, "62", null, null, null, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31), A, null, null));
        regla.Cumple(new ContextoImputacion("629", new DateOnly(2026, 8, 31))).Should().BeFalse();
        regla.Cumple(new ContextoImputacion("629", new DateOnly(2027, 3, 1))).Should().BeTrue();
    }

    [Fact]
    public void Una_regla_necesita_criterio_y_un_solo_destino()
    {
        ReglaAnalitica.Crear(Empresa, new(null, null, null, null, null, null, null, A, null, null)).Error.Codigo.Should().Be("regla.sin_criterio");
        ReglaAnalitica.Crear(Empresa, new(null, "62", null, null, null, null, null, A, Guid.NewGuid(), null)).Error.Codigo.Should().Be("regla.destino");
        ReglaAnalitica.Crear(Empresa, new(null, "57", null, null, null, null, null, A, null, null)).Error.Codigo.Should().Be("regla.cuenta");
    }

    [Fact]
    public void Una_clave_suma_cien_sin_repetir_centros()
    {
        ClaveReparto.ValidarReparto([new(A, 60m), new(B, 40m)]).Should().BeNull();
        ClaveReparto.ValidarReparto([new(A, 60m), new(B, 30m)])!.Mensaje.Should().Be("Los porcentajes deben sumar 100 (suman 90).");
        ClaveReparto.ValidarReparto([new(A, 50m), new(A, 50m)])!.Mensaje.Should().Contain("dos veces");
        ClaveReparto.ValidarReparto([new(A, 33.333m), new(B, 66.667m)])!.Mensaje.Should().Contain("dos decimales");
    }

    [Fact]
    public void Un_centro_no_depende_de_si_mismo()
    {
        var centro = CentroAnalitico.Crear(Guid.NewGuid(), "adm", "Administración", TipoCentroAnalitico.Departamento, null).Valor;
        centro.Codigo.Should().Be("ADM");
        centro.Actualizar("Administración", TipoCentroAnalitico.Departamento, centro.Id, true).Error.Codigo.Should().Be("centro.padre");
    }

    private static readonly DateOnly Hoy = new(2026, 9, 26);

    private static ReglaAnalitica Regla(DatosReglaAnalitica datos) => ReglaAnalitica.Crear(Empresa, datos).Valor;
}
