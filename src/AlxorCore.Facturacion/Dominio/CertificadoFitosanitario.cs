using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Qué documento fitosanitario acompaña la expedición.</summary>
public enum TipoCertificadoFitosanitario
{
    /// <summary>Certificado fitosanitario de exportación (a países fuera de la UE; en España lo expide la inspección del MAPA).</summary>
    Exportacion = 1,

    /// <summary>Certificado de reexportación (mercancía importada que se vuelve a exportar).</summary>
    Reexportacion = 2,

    /// <summary>Pasaporte fitosanitario (vegetales dentro de la UE que lo requieren).</summary>
    PasaporteFitosanitario = 3,
}

/// <summary>
/// Certificado fitosanitario de una expedición (su carta de porte): número, fecha, país de destino, organismo que lo
/// expide y, opcionalmente, el documento escaneado. Se cita en los documentos que acompañan a la carta de porte y en los datos de aduana.
/// </summary>
public sealed class CertificadoFitosanitario : RaizAgregadoEmpresa<Guid>
{
    public const int TamanoMaximoDocumento = 5 * 1024 * 1024;

    private CertificadoFitosanitario(Guid id) : base(id, Guid.Empty) { Numero = null!; }

    private CertificadoFitosanitario(Guid id, Guid empresaId, Guid cartaPorteId) : base(id, empresaId) { Numero = null!; CartaPorteId = cartaPorteId; }

    public Guid CartaPorteId { get; private set; }

    public TipoCertificadoFitosanitario Tipo { get; private set; }

    public string Numero { get; private set; }

    public DateOnly FechaEmision { get; private set; }

    public string? PaisDestino { get; private set; }

    /// <summary>Organismo que lo expide (p. ej. «MAPA · Inspección fitosanitaria de Valencia»).</summary>
    public string? Organismo { get; private set; }

    /// <summary>Mercancía que ampara.</summary>
    public string? Mercancia { get; private set; }

    public string? Observaciones { get; private set; }

    public string? DocumentoNombre { get; private set; }

    public string? DocumentoTipo { get; private set; }

    public byte[]? Documento { get; private set; }

    public static Resultado<CertificadoFitosanitario> Crear(Guid empresaId, Guid cartaPorteId, TipoCertificadoFitosanitario tipo, string? numero, DateOnly fechaEmision,
        string? paisDestino, string? organismo, string? mercancia, string? observaciones)
    {
        var c = new CertificadoFitosanitario(Guid.NewGuid(), empresaId, cartaPorteId);
        var r = c.Modificar(tipo, numero, fechaEmision, paisDestino, organismo, mercancia, observaciones);
        return r.EsFallo ? Resultado.Fallo<CertificadoFitosanitario>(r.Error) : Resultado.Ok(c);
    }

    public Resultado Modificar(TipoCertificadoFitosanitario tipo, string? numero, DateOnly fechaEmision, string? paisDestino, string? organismo, string? mercancia,
        string? observaciones)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo(Error.Validacion("certificado.tipo", "El tipo es Exportacion, Reexportacion o PasaporteFitosanitario."));
        }

        var n = Transportista.Texto(numero, 40);
        if (n is null)
        {
            return Resultado.Fallo(Error.Validacion("certificado.numero", "El número del certificado es obligatorio (hasta 40 caracteres)."));
        }

        var pais = Paises.Codigo(paisDestino);
        if (!string.IsNullOrWhiteSpace(paisDestino) && pais is null)
        {
            return Resultado.Fallo(Error.Validacion("certificado.pais", "Indica el país de destino con su código de dos letras."));
        }

        Tipo = tipo;
        Numero = n;
        FechaEmision = fechaEmision;
        PaisDestino = pais;
        Organismo = Transportista.Texto(organismo, 120);
        Mercancia = Transportista.Texto(mercancia, 200);
        Observaciones = Transportista.Texto(observaciones, 300);
        return Resultado.Ok();
    }

    /// <summary>Adjunta el documento escaneado (PDF o imagen, hasta 5 MB); sin contenido, lo quita.</summary>
    public Resultado Adjuntar(string? nombre, string? tipoContenido, byte[]? contenido)
    {
        if (contenido is null || contenido.Length == 0)
        {
            DocumentoNombre = DocumentoTipo = null;
            Documento = null;
            return Resultado.Ok();
        }

        if (contenido.Length > TamanoMaximoDocumento)
        {
            return Resultado.Fallo(Error.Validacion("certificado.documento", "El documento no puede pasar de 5 MB."));
        }

        var t = (tipoContenido ?? string.Empty).Trim().ToLowerInvariant();
        if (t is not ("application/pdf" or "image/jpeg" or "image/png"))
        {
            return Resultado.Fallo(Error.Validacion("certificado.documento", "El documento es un PDF o una imagen (JPEG o PNG)."));
        }

        DocumentoNombre = Transportista.Texto(nombre, 150) ?? "certificado";
        DocumentoTipo = t;
        Documento = contenido;
        return Resultado.Ok();
    }
}
