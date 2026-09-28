using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Integraciones.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Integraciones.Tests;

public sealed class EdifactTests
{
    private static readonly DateTimeOffset Ahora = new(2026, 9, 28, 8, 30, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("8412345000010", true)]
    [InlineData("8412345000011", false)]
    [InlineData("841234500001", false)]
    [InlineData("84123450000A0", false)]
    public void El_GLN_se_valida_con_su_digito_de_control(string gln, bool valido) => CodigosGs1.EsGlnValido(gln).Should().Be(valido);

    [Fact]
    public void El_GTIN_admite_8_12_13_y_14_digitos()
    {
        CodigosGs1.EsGtinValido("8437000000013").Should().BeTrue();
        CodigosGs1.EsGtinValido("96385074").Should().BeTrue();
        CodigosGs1.EsGtinValido("8437000000014").Should().BeFalse();
    }

    [Fact]
    public void Los_caracteres_reservados_se_escapan_y_se_leen_de_vuelta()
    {
        Edifact.Esc("A+B:C'D?E").Should().Be("A?+B?:C?'D??E");
        var orders = "UNA:+.? 'UNH+1+ORDERS:D:96A:UN:EAN008'BGM+220+PO?+1+9'DTM+137:20260928:102'DTM+2:20261001:102'NAD+BY+8412345000027::9'"
            + "LIN+1++8437000000013:EN'IMD+F++:::Tomate ?+ calibre?: M'QTY+21:12.5'PRI+AAA:1.15'LIN+2'PIA+5+ABC:SA'QTY+21:3'UNT+11+1'";
        var p = Edifact.LeerOrders(orders);
        p.Numero.Should().Be("PO+1");
        p.Fecha.Should().Be(new DateOnly(2026, 9, 28));
        p.FechaEntrega.Should().Be(new DateOnly(2026, 10, 1));
        p.GlnComprador.Should().Be("8412345000027");
        p.Lineas.Should().HaveCount(2);
        p.Lineas[0].Should().Match<LineaEdi>(l => l.Gtin == "8437000000013" && l.Descripcion == "Tomate + calibre: M" && l.Cantidad == 12.5m && l.Precio == 1.15m);
        p.Lineas[1].Should().Match<LineaEdi>(l => l.Gtin == null && l.CodigoInterno == "ABC" && l.Cantidad == 3m);
    }

    [Fact]
    public void Un_mensaje_de_otro_tipo_no_se_lee_como_ORDERS() =>
        FluentActions.Invoking(() => Edifact.LeerOrders("UNH+1+INVOIC:D:96A:UN'BGM+380+1'")).Should().Throw<FormatException>();

    [Fact]
    public void El_DESADV_lleva_un_nivel_por_pale_con_su_SSCC_y_cuenta_sus_segmentos()
    {
        var d = new DatosDesadv("A-7", new DateOnly(2026, 9, 28), "8412345000010", "8412345000027", "8412345000034", "PO-1",
            [new PaleEdi("384123450000000019", [new LineaEdi("8437000000013", "", 600m)]), new PaleEdi("384123450000000026", [new LineaEdi("8437000000013", "", 400m)])], []);
        var m = Edifact.Desadv(d, "REF1", Ahora);
        m.Should().StartWith("UNA:+.? 'UNB+UNOC:3+8412345000010:14+8412345000027:14+260928:0830+REF1'")
            .And.Contain("PAC+2++201").And.Contain("CPS+2+1").And.Contain("GIN+BJ+384123450000000019").And.Contain("CPS+3+1").And.Contain("QTY+12:400")
            .And.Contain("CNT+2:2").And.EndWith("UNZ+1+REF1'");
        var segmentos = m.Split('\'').SkipWhile(s => !s.StartsWith("UNH", StringComparison.Ordinal)).TakeWhile(s => !s.StartsWith("UNZ", StringComparison.Ordinal)).ToList();
        m.Should().Contain($"UNT+{segmentos.Count}+1'");
    }

    [Fact]
    public void El_RECADV_da_lo_recibido_y_lo_aceptado_por_articulo()
    {
        var r = Edifact.LeerRecadv("UNH+1+RECADV:D:96A:UN:EAN003'BGM+632+R1+9'RFF+AAK:A?/7'RFF+ON:PO-1'LIN+1++8437000000013:EN'QTY+194:980'QTY+46:950'LIN+2++96385074:EN'QTY+12:5'UNT+9+1'");
        r.Albaran.Should().Be("A/7");
        r.PedidoCliente.Should().Be("PO-1");
        r.Lineas.Should().Equal(("8437000000013", 980m, (decimal?)950m), ("96385074", 5m, (decimal?)null));
    }
}
