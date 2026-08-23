using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Dominio;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Organizacion.Tests.Dominio;

/// <summary>Pruebas de la plantilla de documentos configurable de la empresa.</summary>
public class EmpresaPlantillaTests
{
    private static readonly IReloj Reloj = new RelojFijo();

    // PNG 1×1 válido (empieza por la firma \x89PNG).
    private static readonly byte[] PngValido = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+M8AAAMBAQDJ/pLvAAAAAElFTkSuQmCC");

    private static Empresa NuevaEmpresa() =>
        Empresa.Crear(Guid.NewGuid(), Nif.Crear("B12345674").Valor, "Mi Empresa SL", Direccion.Vacia, RegimenIva.General, Reloj).Valor;

    [Fact]
    public void Guarda_contacto_color_pie_y_logo_validos()
    {
        var e = NuevaEmpresa();
        var r = e.EstablecerPlantillaDocumento("911234567", "www.miempresa.es", "hola@miempresa.es", "#0EA5B7", "Gracias por su confianza", PngValido, Reloj);

        r.EsCorrecto.Should().BeTrue();
        e.Telefono.Should().Be("911234567");
        e.Web.Should().Be("www.miempresa.es");
        e.EmailContacto.Should().Be("hola@miempresa.es");
        e.ColorPrincipal.Should().Be("#0EA5B7");
        e.TextoPie.Should().Be("Gracias por su confianza");
        e.LogoPng.Should().BeEquivalentTo(PngValido);
    }

    [Fact]
    public void Un_color_mal_formado_se_rechaza()
    {
        var e = NuevaEmpresa();
        e.EstablecerPlantillaDocumento(null, null, null, "azul", null, null, Reloj).EsFallo.Should().BeTrue();
        e.EstablecerPlantillaDocumento(null, null, null, "#12", null, null, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Un_logo_que_no_es_png_se_rechaza()
    {
        var e = NuevaEmpresa();
        var noPng = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // «%PDF-»
        e.EstablecerPlantillaDocumento(null, null, null, null, null, noPng, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Un_logo_demasiado_grande_se_rechaza()
    {
        var e = NuevaEmpresa();
        var grande = new byte[Empresa.TamanoMaximoLogoBytes + 1];
        // Firma PNG para que solo falle por tamaño.
        Array.Copy(PngValido, grande, 8);
        e.EstablecerPlantillaDocumento(null, null, null, null, null, grande, Reloj).EsFallo.Should().BeTrue();
    }

    [Fact]
    public void Un_array_vacio_quita_el_logo()
    {
        var e = NuevaEmpresa();
        e.EstablecerPlantillaDocumento(null, null, null, null, null, PngValido, Reloj);
        e.LogoPng.Should().NotBeNull();

        e.EstablecerPlantillaDocumento(null, null, null, null, null, Array.Empty<byte>(), Reloj);
        e.LogoPng.Should().BeNull();
    }

    [Fact]
    public void Un_logo_null_no_toca_el_existente()
    {
        var e = NuevaEmpresa();
        e.EstablecerPlantillaDocumento(null, null, null, null, null, PngValido, Reloj);
        e.EstablecerPlantillaDocumento("911", null, null, null, null, null, Reloj);
        e.LogoPng.Should().NotBeNull(); // sigue el logo anterior
        e.Telefono.Should().Be("911");
    }
}
