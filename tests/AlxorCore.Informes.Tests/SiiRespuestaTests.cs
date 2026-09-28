using AlxorCore.Informes.Aplicacion;
using FluentAssertions;
using Xunit;

namespace AlxorCore.Informes.Tests;

/// <summary>Lectura de la respuesta del SII y claves del 190.</summary>
public class SiiRespuestaTests
{
    [Fact]
    public void Lee_el_estado_el_csv_y_cada_linea()
    {
        const string cuerpo = """
            <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/"><env:Body>
            <siiR:RespuestaLRFEmitidas xmlns:siiR="urn:r" xmlns:sii="urn:s">
              <siiR:CSV>A1B2C3</siiR:CSV><siiR:EstadoEnvio>ParcialmenteCorrecto</siiR:EstadoEnvio>
              <siiR:RespuestaLinea><siiR:IDFactura><sii:NumSerieFacturaEmisor>FA2026/1</sii:NumSerieFacturaEmisor></siiR:IDFactura><siiR:EstadoRegistro>Correcto</siiR:EstadoRegistro></siiR:RespuestaLinea>
              <siiR:RespuestaLinea><siiR:IDFactura><sii:NumSerieFacturaEmisor>FA2026/2</sii:NumSerieFacturaEmisor></siiR:IDFactura><siiR:EstadoRegistro>Incorrecto</siiR:EstadoRegistro>
                <siiR:CodigoErrorRegistro>1100</siiR:CodigoErrorRegistro><siiR:DescripcionErrorRegistro>Valor no permitido</siiR:DescripcionErrorRegistro></siiR:RespuestaLinea>
            </siiR:RespuestaLRFEmitidas></env:Body></env:Envelope>
            """;
        var r = RespuestaSii.Leer(cuerpo);
        r.Fallo.Should().BeNull();
        r.EstadoEnvio.Should().Be("ParcialmenteCorrecto");
        r.Csv.Should().Be("A1B2C3");
        r.Lineas.Should().HaveCount(2);
        r.Lineas[1].Should().Be(new RespuestaSii.Linea("FA2026/2", "Incorrecto", "1100", "Valor no permitido"));
    }

    [Fact]
    public void Un_fault_o_un_cuerpo_ilegible_es_un_fallo_global()
    {
        RespuestaSii.Leer("""<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><s:Fault><faultstring>Codigo[4102].El XML no cumple el esquema</faultstring></s:Fault></s:Body></s:Envelope>""")
            .Fallo.Should().Contain("4102");
        RespuestaSii.Leer("<html>no es xml").Fallo.Should().NotBeNull();
        RespuestaSii.Leer(null).Fallo.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1000, 150, "G", "01")]
    [InlineData(1000, 70, "G", "01")]
    [InlineData(10000, 200, "H", "02")]
    [InlineData(10000, 100, "H", "01")]
    public void La_clave_del_190_depende_del_tipo_de_retencion(decimal baseImponible, decimal retencion, string clave, string subclave) =>
        GenerarRetencionesIrpf.ClaveDe(baseImponible, retencion).Should().Be((clave, subclave));
}
