using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Facturacion.Tests;

public class AnulacionTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly DateOnly Fecha = new(2026, 1, 15);
    private static readonly ClienteFacturado Cliente =
        new(Guid.NewGuid(), "Cliente SL", "B12345674", "Calle 1", "28001", "Madrid", "Madrid", "ES");

    private static Factura Emitida() =>
        Factura.Emitir(Guid.NewGuid(), new NumeroFactura("FA", 2026, 1), Fecha, Fecha, Cliente,
            [new NuevaLinea("Servicio", 1m, 100m, "IVA21", 21m)], 0m, Reloj).Valor;

    [Fact]
    public void Anular_cambia_estado_guarda_motivo_y_genera_huella_de_anulacion_sin_borrar()
    {
        var f = Emitida();
        var total = f.Total;

        var r = f.Anular("Factura duplicada", "B12345674", "HUELLAPREVIA", Reloj.AhoraUtc);

        r.EsCorrecto.Should().BeTrue();
        f.Estado.Should().Be(EstadoFactura.Anulada);
        f.MotivoAnulacion.Should().Be("Factura duplicada");
        f.HuellaAnulacion.Should().NotBeNullOrEmpty();
        f.FechaHoraAnulacion.Should().NotBeNull();
        f.Total.Should().Be(total); // no se modifica el importe
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Anular_exige_motivo(string? motivo)
    {
        Emitida().Anular(motivo, "B12345674", null, Reloj.AhoraUtc).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void No_se_puede_anular_una_factura_ya_rectificada_ni_ya_anulada()
    {
        var f = Emitida();
        f.MarcarRectificada();
        f.Anular("motivo", "B12345674", null, Reloj.AhoraUtc).EsFallo.Should().BeTrue();

        var g = Emitida();
        g.Anular("primera", "B12345674", null, Reloj.AhoraUtc).EsCorrecto.Should().BeTrue();
        g.Anular("segunda", "B12345674", null, Reloj.AhoraUtc).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void La_huella_de_anulacion_encadena_la_anterior()
    {
        var a = Emitida();
        var b = Emitida();
        a.Anular("m", "B12345674", "PREVIA-1", Reloj.AhoraUtc);
        b.Anular("m", "B12345674", "PREVIA-2", Reloj.AhoraUtc);
        // Distinta huella anterior ⇒ distinta huella de anulación.
        a.HuellaAnulacion.Should().NotBe(b.HuellaAnulacion);
    }
}
