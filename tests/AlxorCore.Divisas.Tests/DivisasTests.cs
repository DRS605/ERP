using AlxorCore.Divisas.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Divisas.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);
}

public class CalculadoraDivisaTests
{
    [Theory]
    [InlineData(100, 0.92, 92.00)]     // 100 USD × 0,92 €/USD
    [InlineData(1000, 1.15, 1150.00)]  // 1000 GBP × 1,15
    public void Convierte_a_euros(decimal importe, decimal tasa, decimal esperado) =>
        CalculadoraDivisa.AEuros(importe, tasa).Should().Be(esperado);

    [Fact]
    public void Convierte_de_euros_a_divisa()
    {
        CalculadoraDivisa.DeEuros(92m, 0.92m).Should().Be(100m);
        CalculadoraDivisa.DeEuros(100m, 0m).Should().Be(0m); // tasa 0 → 0 (defensivo)
    }

    [Fact]
    public void Diferencia_de_activo_que_se_aprecia_es_ingreso_768()
    {
        // 1000 USD a cobrar; la divisa sube de 0,90 a 0,95 € ⇒ cobras más euros ⇒ ingreso.
        var r = CalculadoraDivisa.Diferencia(1000m, 0.90m, 0.95m, esActivo: true);
        r.DiferenciaEur.Should().Be(50.00m);
        r.EsIngreso.Should().BeTrue();
        r.Cuenta.Should().Be(CalculadoraDivisa.CuentaDiferenciasPositivas);
    }

    [Fact]
    public void Diferencia_de_activo_que_se_deprecia_es_gasto_668()
    {
        var r = CalculadoraDivisa.Diferencia(1000m, 0.90m, 0.85m, esActivo: true);
        r.DiferenciaEur.Should().Be(-50.00m);
        r.EsIngreso.Should().BeFalse();
        r.Cuenta.Should().Be(CalculadoraDivisa.CuentaDiferenciasNegativas);
    }

    [Fact]
    public void Diferencia_de_pasivo_es_el_inverso_del_activo()
    {
        // 1000 USD a pagar; la divisa sube ⇒ debes más euros ⇒ pérdida (668).
        var r = CalculadoraDivisa.Diferencia(1000m, 0.90m, 0.95m, esActivo: false);
        r.DiferenciaEur.Should().Be(-50.00m);
        r.Cuenta.Should().Be(CalculadoraDivisa.CuentaDiferenciasNegativas);
    }
}

public class TipoCambioTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Crea_un_tipo_de_cambio_valido_normalizando_el_codigo()
    {
        var r = TipoCambio.Crear(Guid.NewGuid(), "usd", new DateOnly(2026, 3, 1), 0.92m, Reloj);
        r.EsCorrecto.Should().BeTrue();
        r.Valor.Divisa.Should().Be("USD");
        r.Valor.TasaEur.Should().Be(0.92m);
    }

    [Fact]
    public void El_euro_no_admite_tipo_de_cambio()
    {
        TipoCambio.Crear(Guid.NewGuid(), "EUR", new DateOnly(2026, 3, 1), 1m, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Rechaza_divisa_desconocida_y_tasa_no_positiva()
    {
        TipoCambio.Crear(Guid.NewGuid(), "XXX", new DateOnly(2026, 3, 1), 1m, Reloj).EsFallo.Should().BeTrue();
        TipoCambio.Crear(Guid.NewGuid(), "USD", new DateOnly(2026, 3, 1), 0m, Reloj).EsFallo.Should().BeTrue();
        TipoCambio.Crear(Guid.NewGuid(), "USD", new DateOnly(2026, 3, 1), -1m, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Catalogo_reconoce_divisas_habituales()
    {
        DivisasConocidas.EsConocida("USD").Should().BeTrue();
        DivisasConocidas.EsConocida("xxx").Should().BeFalse();
        DivisasConocidas.EsEuro("eur").Should().BeTrue();
        DivisasConocidas.Normalizar(" gbp ").Should().Be("GBP");
    }
}
