using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Informes.Dominio;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// AEAT simulada para las pruebas: responde a cada factura del envío como Correcto, salvo las que llevan «RECHAZO» en el
/// número (Incorrecto, error 1117) o «AVISO» (AceptadoConErrores). Guarda los sobres recibidos para inspeccionarlos.
/// </summary>
public sealed class TransporteSiiFalso : ITransporteSii
{
    public static ConcurrentQueue<(TipoLibroSii Libro, EntornoSii Entorno, string Sobre)> Recibidos { get; } = new();

    /// <summary>Destino de cada sobre (AEAT o ATC), por su contenido.</summary>
    public static ConcurrentQueue<(Uri Destino, string Sobre)> Destinos { get; } = new();

    public Task<RespuestaTransporteSii> EnviarAsync(Uri destino, TipoLibroSii libro, EntornoSii entorno, X509Certificate2 certificado, string sobreSoap,
        CancellationToken ct = default)
    {
        Recibidos.Enqueue((libro, entorno, sobreSoap));
        Destinos.Enqueue((destino, sobreSoap));
        var doc = XDocument.Parse(sobreSoap);
        var baja = doc.Descendants().Any(e => e.Name.LocalName.StartsWith("BajaLR", StringComparison.Ordinal));
        var numeros = doc.Descendants().Where(e => e.Name.LocalName == "IDFactura")
            .Select(e => e.Elements().First(x => x.Name.LocalName == "NumSerieFacturaEmisor").Value).ToList();
        var lineas = numeros.Select(n =>
        {
            var (estado, codigo) = n.Contains("RECHAZO", StringComparison.Ordinal) ? ("Incorrecto", "<CodigoErrorRegistro>1117</CodigoErrorRegistro><DescripcionErrorRegistro>El NIF no está identificado</DescripcionErrorRegistro>")
                : n.Contains("AVISO", StringComparison.Ordinal) ? ("AceptadoConErrores", "<CodigoErrorRegistro>2011</CodigoErrorRegistro><DescripcionErrorRegistro>Aviso de prueba</DescripcionErrorRegistro>")
                : ("Correcto", string.Empty);
            return $"<siiR:RespuestaLinea><siiR:IDFactura><sii:IDEmisorFactura><sii:NIF>B00000000</sii:NIF></sii:IDEmisorFactura><sii:NumSerieFacturaEmisor>{n}</sii:NumSerieFacturaEmisor></siiR:IDFactura><siiR:EstadoRegistro>{estado}</siiR:EstadoRegistro>{codigo.Replace("<", "<siiR:", StringComparison.Ordinal).Replace("<siiR:/", "</siiR:", StringComparison.Ordinal)}</siiR:RespuestaLinea>";
        }).ToList();
        var incorrectas = numeros.Count(n => n.Contains("RECHAZO", StringComparison.Ordinal));
        var global = incorrectas == 0 ? "Correcto" : incorrectas == numeros.Count ? "Incorrecto" : "ParcialmenteCorrecto";
        var raiz = baja ? (libro == TipoLibroSii.Emitidas ? "RespuestaLRBajaFEmitidas" : "RespuestaLRBajaFRecibidas") : (libro == TipoLibroSii.Emitidas ? "RespuestaLRFEmitidas" : "RespuestaLRFRecibidas");
        var cuerpo = $"""
            <env:Envelope xmlns:env="http://schemas.xmlsoap.org/soap/envelope/"><env:Body>
            <siiR:{raiz} xmlns:siiR="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/RespuestaSuministro.xsd" xmlns:sii="https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroInformacion.xsd">
            <siiR:CSV>CSVPRUEBA{Recibidos.Count:D4}</siiR:CSV><siiR:EstadoEnvio>{global}</siiR:EstadoEnvio>{string.Concat(lineas)}
            </siiR:{raiz}></env:Body></env:Envelope>
            """;
        return Task.FromResult(new RespuestaTransporteSii(200, cuerpo, null));
    }
}
