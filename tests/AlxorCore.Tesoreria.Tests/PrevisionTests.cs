using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Tesoreria.Tests;

public class PrevisionTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    [Fact]
    public void Crear_prevision_valida()
    {
        var r = PrevisionTesoreria.Crear(Guid.NewGuid(), SentidoPrevision.Ingreso, "  Subvención  ", 1200.5m, new DateOnly(2026, 3, 1), Reloj);
        r.EsCorrecto.Should().BeTrue();
        r.Valor.Sentido.Should().Be(SentidoPrevision.Ingreso);
        r.Valor.Concepto.Should().Be("Subvención");
        r.Valor.Importe.Should().Be(1200.5m);
    }

    [Fact]
    public void Rechaza_concepto_vacio_e_importe_no_positivo()
    {
        PrevisionTesoreria.Crear(Guid.NewGuid(), SentidoPrevision.Gasto, "  ", 10m, new DateOnly(2026, 3, 1), Reloj).EsFallo.Should().BeTrue();
        PrevisionTesoreria.Crear(Guid.NewGuid(), SentidoPrevision.Gasto, "Alquiler", 0m, new DateOnly(2026, 3, 1), Reloj).EsFallo.Should().BeTrue();
        PrevisionTesoreria.Crear(Guid.NewGuid(), SentidoPrevision.Gasto, "Alquiler", -5m, new DateOnly(2026, 3, 1), Reloj).EsFallo.Should().BeTrue();
    }
}
