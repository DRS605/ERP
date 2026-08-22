using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

public class AsignacionSerieTests
{
    private static readonly IReloj Reloj = new RelojFijo();
    private static readonly Guid Empresa = Guid.NewGuid();

    [Fact]
    public void Ambito_empresa_no_necesita_tercero_y_normaliza_prefijo()
    {
        var r = AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Empresa, null, " fa ", Reloj);

        r.EsCorrecto.Should().BeTrue();
        r.Valor.Ambito.Should().Be(AmbitoSerie.Empresa);
        r.Valor.TerceroId.Should().Be(Guid.Empty);
        r.Valor.Prefijo.Should().Be("FA");
    }

    [Fact]
    public void Ambito_cliente_exige_tercero()
    {
        AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Cliente, null, "CLI", Reloj).EsFallo.Should().BeTrue();
        AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Cliente, Guid.Empty, "CLI", Reloj).EsFallo.Should().BeTrue();
        AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Cliente, Guid.NewGuid(), "CLI", Reloj).EsCorrecto.Should().BeTrue();
    }

    [Fact]
    public void Rechaza_prefijo_vacio_o_demasiado_largo()
    {
        AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Empresa, null, "  ", Reloj).EsFallo.Should().BeTrue();
        AsignacionSerie.Crear(Empresa, TipoDocumento.Factura, AmbitoSerie.Empresa, null, new string('X', 11), Reloj).EsFallo.Should().BeTrue();
    }
}
