using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Facturacion.Tests;

public class CartaPorteTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    private static (string?, int, decimal)[] DosLineas() =>
        new[] { ("Palés", 10, 250.5m), ("Cajas", 5, 30m) };

    [Fact]
    public void Crear_calcula_totales_de_bultos_y_peso()
    {
        var r = CartaPorte.Crear(Empresa, null, 2026, 1, new DateOnly(2026, 3, 1),
            "Remitente SL", "B1", null, "Destinatario SL", "B2",
            "Transportista SA", "A1", "1234 ABC", "Origen", "Destino", null, null, null, DosLineas(), Reloj);

        r.EsCorrecto.Should().BeTrue();
        r.Valor.TotalBultos.Should().Be(15);
        r.Valor.TotalPesoKg.Should().Be(280.5m);
        r.Valor.NumeroCompleto.Should().Be("2026/00001");
        r.Valor.Lineas.Should().HaveCount(2);
    }

    [Fact]
    public void Crear_sin_destinatario_falla()
    {
        CartaPorte.Crear(Empresa, null, 2026, 1, new DateOnly(2026, 3, 1),
            "Rem", null, null, "  ", null, null, null, null, "O", "D", null, null, null, DosLineas(), Reloj)
            .EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Crear_sin_lineas_falla()
    {
        CartaPorte.Crear(Empresa, null, 2026, 1, new DateOnly(2026, 3, 1),
            "Rem", null, null, "Dest", null, null, null, null, "O", "D", null, null, null,
            Array.Empty<(string?, int, decimal)>(), Reloj)
            .EsFallo.Should().BeTrue();
    }

    [Fact]
    public void La_serie_forma_parte_del_numero_completo()
    {
        var r = CartaPorte.Crear(Empresa, "cp", 2026, 7, new DateOnly(2026, 3, 1),
            "Rem", null, null, "Dest", null, null, null, null, "O", "D", null, null, null, DosLineas(), Reloj);
        r.Valor.NumeroCompleto.Should().Be("CP2026/00007");
    }
}
