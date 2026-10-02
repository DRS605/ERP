using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

public sealed class PerfilFiscalTests
{
    [Fact]
    public void Gran_empresa_y_redeme_obligan_a_declarar_cada_mes_y_al_sii()
    {
        new PerfilFiscal { GranEmpresa = true }.Validar().Error.Codigo.Should().Be("perfil.periodicidad");
        new PerfilFiscal { Redeme = true, Periodicidad = PeriodicidadImpuesto.Mensual }.Validar().Error.Codigo.Should().Be("perfil.sii_obligatorio");
        new PerfilFiscal { Sii = true }.Validar().Error.Codigo.Should().Be("perfil.fecha_sii");
        new PerfilFiscal { GranEmpresa = true, Periodicidad = PeriodicidadImpuesto.Mensual, Sii = true, FechaAltaSii = new DateOnly(2026, 1, 1) }
            .Validar().EsCorrecto.Should().BeTrue();
    }

    [Fact]
    public void Valida_cnae_mes_y_modelos_y_normaliza()
    {
        new PerfilFiscal { Cnae = "46.3" }.Validar().Error.Codigo.Should().Be("perfil.cnae");
        new PerfilFiscal { MesInicioEjercicio = 13 }.Validar().Error.Codigo.Should().Be("perfil.mes_ejercicio");
        new PerfilFiscal { Modelos = ["999"] }.Validar().Error.Codigo.Should().Be("perfil.modelo");
        var ok = new PerfilFiscal
        {
            Cnae = "46.31", NombreComercial = "  Frutas Levante  ", Modelos = ["347", "303", "303"],
            Administradores = [new("Ana García", "12345678z", "Administradora única"), new("  ", null, null)],
        }.Validar().Valor;
        ok.Cnae.Should().Be("4631");
        ok.NombreComercial.Should().Be("Frutas Levante");
        ok.Modelos.Should().Equal("303", "347");
        ok.Administradores.Should().ContainSingle().Which.Nif.Should().Be("12345678Z");
    }

    [Fact]
    public void Propone_los_modelos_segun_forma_juridica_territorio_y_sii()
    {
        PerfilFiscal.Sugeridos("B12345678", TerritorioFiscal.Comun, false).Should().Equal("303", "390", "347", "111", "190", "200", "202");
        PerfilFiscal.Sugeridos("B12345678", TerritorioFiscal.Comun, true).Should().Equal("303", "111", "190", "200", "202");
        PerfilFiscal.Sugeridos("12345678Z", TerritorioFiscal.Canarias, false).Should().Equal("420", "425", "347", "111", "190", "130");
        PerfilFiscal.Sugeridos("E12345678", TerritorioFiscal.Comun, false).Should().Contain("184").And.NotContain("200");
    }

    [Fact]
    public void Calendario_trimestral_y_mensual_con_los_plazos_generales()
    {
        var t = new PerfilFiscal { Modelos = ["303", "111", "390", "347", "200", "202"] }.Calendario(2027);
        t.Where(v => v.Modelo == "303").Select(v => (v.Periodo, v.Hasta)).Should().Equal(
            ("4T 2026", new DateOnly(2027, 2, 1)),   // 30/01/2027 es sábado: pasa al lunes
            ("1T 2027", new DateOnly(2027, 4, 20)),
            ("2T 2027", new DateOnly(2027, 7, 20)),
            ("3T 2027", new DateOnly(2027, 10, 20)));
        t.Single(v => v.Modelo == "111" && v.Periodo == "4T 2026").Hasta.Should().Be(new DateOnly(2027, 1, 20));
        t.Single(v => v.Modelo == "347").Hasta.Should().Be(new DateOnly(2027, 3, 1));   // 28/02/2027 es domingo
        t.Single(v => v.Modelo == "200").Hasta.Should().Be(new DateOnly(2027, 7, 26));  // 25/07/2027 es domingo
        t.Count(v => v.Modelo == "202").Should().Be(3);

        var m = new PerfilFiscal { Periodicidad = PeriodicidadImpuesto.Mensual, Modelos = ["303", "111"] }.Calendario(2026);
        m.Count(v => v.Modelo == "303").Should().Be(12);
        m.Single(v => v.Modelo == "303" && v.Periodo == "julio 2026").Hasta.Should().Be(new DateOnly(2026, 9, 30));
        m.Single(v => v.Modelo == "111" && v.Periodo == "julio 2026").Hasta.Should().Be(new DateOnly(2026, 9, 21));   // 20/09/2026 domingo
        m.Single(v => v.Modelo == "303" && v.Periodo == "diciembre 2025").Hasta.Should().Be(new DateOnly(2026, 1, 30));
    }

    [Fact]
    public void Sociedades_con_ejercicio_partido()
    {
        var c = new PerfilFiscal { MesInicioEjercicio = 7, Modelos = ["200"] }.Calendario(2027);
        // Cierre 30/06/2026: los 25 días siguientes a los seis meses, del 1 al 25 de enero de 2027.
        c.Should().ContainSingle().Which.Should().Be(new VencimientoFiscal("200", "Impuesto sobre sociedades", "cierre 30/06/2026",
            new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 25)));
    }
}
