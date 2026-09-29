using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Logistica.Tests;

public sealed class Gs1Tests
{
    private const string Ean = "8412345678905";
    private const string Gtin = "08412345678905";

    [Fact]
    public void Digito_de_control_y_validez()
    {
        Gs1.DigitoControl("841234567890").Should().Be(5);
        Gs1.EsGtinValido(Ean).Should().BeTrue();
        Gs1.EsGtinValido("8412345678904").Should().BeFalse();
        Gs1.Gtin14(Ean).Should().Be(Gtin);
    }

    [Fact]
    public void Sscc_con_extension_prefijo_y_serie()
    {
        var sscc = Gs1.Sscc(3, "8400000", 42);
        sscc.Should().HaveLength(18).And.StartWith("38400000").And.Contain("0000000042"[1..]);
        Gs1.EsSsccValido(sscc).Should().BeTrue();
    }

    [Fact]
    public void Lee_el_texto_con_parentesis()
    {
        var l = Gs1.Leer($"(02){Gtin}(17)261231(37)48(10)L-77")!;
        l.Gtin.Should().Be(Gtin);
        l.Caducidad.Should().Be(new DateOnly(2026, 12, 31));
        l.Cantidad.Should().Be(48);
        l.Lote.Should().Be("L-77");
    }

    [Fact]
    public void Lee_lo_que_envia_una_pistola_con_fnc1_y_simbologia()
    {
        var sep = '\u001d';
        var l = Gs1.Leer($"]C101{Gtin}10LOTE1{sep}17270115")!;
        l.Gtin.Should().Be(Gtin);
        l.Lote.Should().Be("LOTE1");
        l.Caducidad.Should().Be(new DateOnly(2027, 1, 15));

        Gs1.Leer($"01{Gtin}10LOTE1<GS>3102012345")!.PesoNetoKg.Should().Be(123.45m);
        Gs1.Leer($"01{Gtin}10LOTE1~117270115")!.Caducidad.Should().Be(new DateOnly(2027, 1, 15));
    }

    [Fact]
    public void Lee_un_sscc_suelto_o_con_su_ia_y_un_ean_suelto()
    {
        var sscc = Gs1.Sscc(0, "8400000", 7)!;
        Gs1.Leer(sscc)!.Sscc.Should().Be(sscc);
        Gs1.Leer("00" + sscc)!.Sscc.Should().Be(sscc);
        Gs1.Leer(Ean)!.Gtin.Should().Be(Gtin);
        Gs1.Leer("   ").Should().BeNull();
    }

    [Fact]
    public void Codifica_y_vuelve_a_leer()
    {
        var elementos = new List<(string, string)> { ("02", Gtin), ("10", "A1"), ("37", "12"), Gs1.Peso("310", 250.5m) };
        var l = Gs1.Leer(Gs1.Codificar(elementos))!;
        l.Lote.Should().Be("A1");
        l.Cantidad.Should().Be(12);
        l.PesoNetoKg.Should().Be(250.5m);
        Gs1.Legible(elementos).Should().Contain("(10) A1");
    }
}

public sealed class PaletizadoTests
{
    private sealed class Reloj : IReloj
    {
        public DateTimeOffset AhoraUtc { get; } = new(2026, 9, 29, 8, 0, 0, TimeSpan.Zero);
    }

    private static readonly Guid Empresa = Guid.NewGuid();

    private static TipoSoporte Europale() => TipoSoporte.Crear(Empresa, "EUR", "Europalé", 1200, 800, 144, 25m, 1500m, null).Valor;

    private static FichaLogistica Ficha(int? alto = 200, decimal? bruto = 6.5m) =>
        FichaLogistica.Crear(Empresa, Guid.NewGuid(), new DatosFichaLogistica("8412345678905", null, 6, 1m, bruto, 400, 300, alto, 8, 5, null, 1200, 400m)).Valor;

    [Fact]
    public void Calcula_palés_completos_y_pico_con_pesos_y_altura()
    {
        var ficha = Ficha(alto: 220);
        var mosaico = new Mosaico(8, 5, Europale(), null, null, null, "ficha");
        var c = CalculadoraPaletizado.Calcular(ficha, mosaico, 95);

        c.CajasPorPale.Should().Be(40);
        c.PalesCompletos.Should().Be(2);
        c.Pales.Should().Be(3);
        c.Pico!.Cajas.Should().Be(15);
        c.Pico.CapasCompletas.Should().Be(1);
        c.Pico.CajasCapaIncompleta.Should().Be(7);
        c.PaleCompleto!.PesoBrutoKg.Should().Be(40 * 6.5m + 25m);
        c.PaleCompleto.PesoNetoKg.Should().Be(240m);
        c.PaleCompleto.AlturaMm.Should().Be(144 + 5 * 220);
        c.UnidadesTotales.Should().Be(570m);
        c.PesoBrutoTotalKg.Should().Be(2 * 285m + 15 * 6.5m + 25m);
        c.Avisos.Should().ContainSingle(a => a.Contains("mm", StringComparison.Ordinal));
    }

    [Fact]
    public void Avisa_si_supera_el_peso_o_faltan_datos()
    {
        var mosaico = new Mosaico(8, 5, Europale(), null, 200m, null, "plantilla");
        CalculadoraPaletizado.Calcular(Ficha(alto: 100), mosaico, 40).Avisos.Should().ContainSingle(a => a.Contains("kg", StringComparison.Ordinal));
        CalculadoraPaletizado.Calcular(Ficha(alto: null), new Mosaico(8, 5, null, null, null, null, "ficha"), 10).Avisos
            .Should().Contain(a => a.Contains("aproximados", StringComparison.Ordinal));
        CalculadoraPaletizado.Cajas(Ficha(), 13m).Should().Be(3);
    }

    [Fact]
    public void Una_unidad_se_cierra_sola_al_completarse_y_no_admite_mas()
    {
        var reloj = new Reloj();
        var sscc = Gs1.Sscc(0, "8400000", 1)!;
        var u = UnidadLogistica.Crear(Empresa, sscc, TipoUnidadLogistica.Pale, OrigenUnidadLogistica.Almacen, Guid.NewGuid(), null, Europale(), reloj, cajasCompleta: 10);
        var producto = Guid.NewGuid();

        u.Poner(new ContenidoUnidad(producto, "L1", null, 6, 36m, 36m, 39m), null, reloj).EsCorrecto.Should().BeTrue();
        u.Estado.Should().Be(EstadoUnidadLogistica.Abierta);
        u.Poner(new ContenidoUnidad(producto, "L1", null, -7, -42m, -42m, -45.5m), null, reloj).EsCorrecto.Should().BeFalse();
        u.Poner(new ContenidoUnidad(producto, "L2", null, 4, 24m, 24m, 26m), null, reloj).EsCorrecto.Should().BeTrue();

        u.Estado.Should().Be(EstadoUnidadLogistica.Cerrada);
        u.Cajas.Should().Be(10);
        u.PesoBrutoKg.Should().Be(25m + 39m + 26m);
        u.Poner(new ContenidoUnidad(producto, "L2", null, 1, 6m, 6m, 6.5m), null, reloj).EsCorrecto.Should().BeFalse();
    }
}
