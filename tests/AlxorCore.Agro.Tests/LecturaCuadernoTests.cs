using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Agro.Tests;

public sealed class LecturaCuadernoTests
{
    [Theory]
    [InlineData("(01)08412345678905(10)REC-2026-000001/1", "REC-2026-000001/1")]
    [InlineData("(01)08412345678905(10)REC-2026-000001/1(21)77", "REC-2026-000001/1")]
    [InlineData("]C10108412345678905172612311012AB\u001d2177", "12AB")]
    [InlineData("01084123456789051012AB", "12AB")]
    [InlineData(" REC-2026-000004/2 ", "REC-2026-000004/2")]
    public void El_lote_de_la_etiqueta_es_el_codigo_de_la_partida(string lectura, string lote) =>
        PalesAgro.LoteDeEtiqueta(lectura).Should().Be(lote);

    [Fact]
    public void El_plazo_de_seguridad_cuenta_desde_el_dia_del_tratamiento()
    {
        var t = TratamientoParcela.Crear(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 3, 1), "Abamectina", 7, new RelojFijo()).Valor;
        t.RecolectableDesde.Should().Be(new DateOnly(2026, 3, 8));
        t.DentroDePlazo(new DateOnly(2026, 2, 28)).Should().BeFalse("antes del tratamiento");
        t.DentroDePlazo(new DateOnly(2026, 3, 7)).Should().BeTrue();
        t.DentroDePlazo(new DateOnly(2026, 3, 8)).Should().BeFalse();
        t.Anular("error").EsCorrecto.Should().BeTrue();
        t.DentroDePlazo(new DateOnly(2026, 3, 7)).Should().BeFalse("anulado");
        TratamientoParcela.Crear(Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 3, 1), " ", 7, new RelojFijo()).Error.Codigo.Should().Be("tratamiento.producto");
    }
}
