using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Contabilidad.Tests;

public sealed class RelojFijo : IReloj
{
    public DateTimeOffset AhoraUtc { get; init; } = new(2026, 8, 21, 10, 0, 0, TimeSpan.Zero);
}

public sealed class AsientoTests
{
    private static readonly Guid Empresa = Guid.NewGuid();
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Fecha = new(2026, 8, 1);

    [Fact]
    public void Asiento_cuadrado_se_crea()
    {
        var lineas = new List<LineaAsiento>
        {
            new("629", 100m, 0m),
            new("472", 21m, 0m),
            new("400", 0m, 121m),
        };
        var r = Asiento.Crear(Empresa, 2026, 1, Fecha, "Compra material", "Compra", lineas, Reloj);
        r.EsCorrecto.Should().BeTrue();
        r.Valor.TotalDebe.Should().Be(121m);
        r.Valor.TotalHaber.Should().Be(121m);
        r.Valor.Apuntes.Should().HaveCount(3);
        r.Valor.EventosDominio.Should().ContainSingle(e => e is AsientoRegistrado);
    }

    [Fact]
    public void Asiento_descuadrado_falla()
    {
        var lineas = new List<LineaAsiento> { new("629", 100m, 0m), new("400", 0m, 90m) };
        var r = Asiento.Crear(Empresa, 2026, 1, Fecha, "Mal", "Manual", lineas, Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("asiento.descuadrado");
    }

    [Fact]
    public void Apunte_con_debe_y_haber_a_la_vez_falla()
    {
        var lineas = new List<LineaAsiento> { new("629", 100m, 100m), new("400", 0m, 100m) };
        var r = Asiento.Crear(Empresa, 2026, 1, Fecha, "Mal", "Manual", lineas, Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("asiento.apunte_invalido");
    }

    [Fact]
    public void Asiento_con_una_sola_linea_falla()
    {
        var r = Asiento.Crear(Empresa, 2026, 1, Fecha, "Solo una", "Manual", new List<LineaAsiento> { new("570", 100m, 0m) }, Reloj);
        r.EsFallo.Should().BeTrue();
        r.Error.Codigo.Should().Be("asiento.pocas_lineas");
    }

    [Fact]
    public void Cuenta_codigo_no_numerico_falla()
    {
        Cuenta.Crear(Empresa, "ABC", "Mala").EsFallo.Should().BeTrue();
        Cuenta.Crear(Empresa, "600", "Compras").EsCorrecto.Should().BeTrue();
    }
}
