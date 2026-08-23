using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Tiempo;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Catalogo.Tests;

public sealed class TipoIvaTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    [Fact]
    public void Ordinario_repercute_el_porcentaje()
    {
        var t = TipoIva.Crear(Empresa, "IVA21", "IVA general", 21m, 5.2m, ClaseIva.Ordinario, null, Reloj).Valor;
        t.Clase.Repercute().Should().BeTrue();
        t.PorcentajeRepercutido.Should().Be(21m);
    }

    [Theory]
    [InlineData(ClaseIva.Exento)]
    [InlineData(ClaseIva.NoSujeto)]
    [InlineData(ClaseIva.InversionSujetoPasivo)]
    [InlineData(ClaseIva.Intracomunitario)]
    [InlineData(ClaseIva.Viajeros)]
    [InlineData(ClaseIva.BienesUsados)]
    [InlineData(ClaseIva.AgenciasViajes)]
    [InlineData(ClaseIva.OroInversion)]
    [InlineData(ClaseIva.Exportacion)]
    public void Las_clases_sin_repercusion_no_llevan_porcentaje(ClaseIva clase)
    {
        // Con porcentaje > 0 falla…
        TipoIva.Crear(Empresa, "X", "X", 21m, 0m, clase, "mención", Reloj).EsFallo.Should().BeTrue();

        // …y con 0 el repercutido es 0.
        var t = TipoIva.Crear(Empresa, "X", "X", 0m, 0m, clase, "mención", Reloj).Valor;
        t.Clase.Repercute().Should().BeFalse();
        t.PorcentajeRepercutido.Should().Be(0m);
    }

    [Fact]
    public void Importacion_repercute()
    {
        var t = TipoIva.Crear(Empresa, "IMP", "Importación 21", 21m, 0m, ClaseIva.Importacion, null, Reloj).Valor;
        t.Clase.Repercute().Should().BeTrue();
        t.PorcentajeRepercutido.Should().Be(21m);
    }

    [Fact]
    public void El_codigo_se_normaliza_en_mayusculas()
    {
        TipoIva.Crear(Empresa, " isp ", "Inversión", 0m, 0m, ClaseIva.InversionSujetoPasivo, "m", Reloj).Valor
            .Codigo.Should().Be("ISP");
    }

    [Fact]
    public void El_criterio_de_caja_repercute_iva_ordinario()
    {
        // El RECC difiere el devengo al cobro, pero repercute IVA al porcentaje normal.
        var t = TipoIva.Crear(Empresa, "CAJA21", "Criterio de caja", 21m, 5.2m, ClaseIva.CriterioCaja, "mención RECC", Reloj).Valor;
        t.Clase.Repercute().Should().BeTrue();
        t.PorcentajeRepercutido.Should().Be(21m);
    }

    [Theory]
    [InlineData(ClaseIva.AgriculturaCompensacion, 12)]
    [InlineData(ClaseIva.VentanillaUnicaOSS, 20)]
    public void La_compensacion_reagp_y_el_iva_de_destino_oss_se_repercuten(ClaseIva clase, int porcentaje)
    {
        // REAGP: la compensación a tanto alzado se añade al importe; OSS: IVA del país de destino.
        var t = TipoIva.Crear(Empresa, "X", "X", porcentaje, 0m, clase, "mención", Reloj).Valor;
        t.Clase.Repercute().Should().BeTrue();
        t.PorcentajeRepercutido.Should().Be(porcentaje);
    }

    [Fact]
    public void El_conjunto_predeterminado_incluye_las_casuisticas_y_regimenes_especiales()
    {
        var codigos = TipoIva.Predeterminados.Select(p => p.Codigo).ToList();
        codigos.Should().Contain(new[] { "IVA21", "IVA10", "IVA4", "IVA0", "NOSUJETO", "ISP", "INTRA", "IMPORT21" });

        // Regímenes especiales: exportación, viajeros, REBU, agencias, oro, criterio de caja y REAGP.
        codigos.Should().Contain(new[] { "EXPORT", "VIAJEROS", "REBU", "AGENCIAS", "ORO", "CAJA21", "REAGP12", "REAGP105" });

        // Todos los predeterminados sin repercusión llevan mención legal.
        foreach (var p in TipoIva.Predeterminados.Where(p => !p.Clase.Repercute()))
        {
            p.Mencion.Should().NotBeNullOrWhiteSpace($"la clase {p.Clase} debe llevar mención");
        }
    }
}
