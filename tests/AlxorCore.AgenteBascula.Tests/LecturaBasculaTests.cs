using AlxorCore.AgenteBascula;
using FluentAssertions;
using Xunit;

namespace AlxorCore.AgenteBascula.Tests;

public sealed class LecturaBasculaTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 29, 8, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("ST,GS,+0012340kg", 12340, true)]
    [InlineData("US,GS,+0012300kg", 12300, false)]
    [InlineData("ST,NT,-0000050kg", -50, true)]
    [InlineData("S S     12.345 kg", 12.345, true)]
    [InlineData("S D     12.300 kg", 12.3, false)]
    [InlineData("\u0002  8.540kg\u0003", 8.54, true)]
    [InlineData("  1250 g", 1.25, true)]
    [InlineData("P 2,5 t", 2500, true)]
    [InlineData("0001234", 1234, true)]
    public void Interpreta_las_tramas_habituales(string trama, double kilos, bool estable)
    {
        var p = LecturaBascula.Interpretar(trama, Ahora);
        p.Should().NotBeNull();
        p!.Kilos.Should().Be((decimal)kilos);
        p.Estable.Should().Be(estable);
    }

    [Theory]
    [InlineData("")]
    [InlineData("OL,GS,+9999999kg")]
    [InlineData("S +")]
    [InlineData("S I")]
    [InlineData("ERR 03")]
    [InlineData("---")]
    public void Sin_peso_valido_no_hay_pesada(string trama) => LecturaBascula.Interpretar(trama, Ahora).Should().BeNull();
}
