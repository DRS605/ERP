using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Aprobaciones.Dominio;

/// <summary>Estado de una solicitud de aprobación.</summary>
public enum EstadoAprobacion
{
    /// <summary>A la espera de que un aprobador (distinto del solicitante) la resuelva.</summary>
    Pendiente = 1,

    /// <summary>Aprobada: la operación puede continuar.</summary>
    Aprobada = 2,

    /// <summary>Rechazada: la operación queda vetada (con motivo).</summary>
    Rechazada = 3,
}

/// <summary>Se ha resuelto (aprobado o rechazado) una solicitud de aprobación.</summary>
public sealed record SolicitudResuelta(Guid SolicitudId, Guid EmpresaId, EstadoAprobacion Estado, Guid AprobadorUsuarioId, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>
/// Regla de aprobación de la empresa: a partir de qué <b>importe</b> un documento de cierto
/// <b>tipo</b> requiere aprobación. Un umbral de 0 con la regla activa exige aprobar siempre; regla
/// inactiva = no se exige aprobación para ese tipo.
/// </summary>
public sealed class ReglaAprobacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTipo = 40;

    private ReglaAprobacion(Guid id)
        : base(id, Guid.Empty)
    {
        TipoDocumento = null!;
    }

    private ReglaAprobacion(Guid id, Guid empresaId, string tipoDocumento, decimal umbralImporte, bool activa, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        TipoDocumento = tipoDocumento;
        UmbralImporte = umbralImporte;
        Activa = activa;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Tipo de documento al que aplica (p. ej. «Factura», «Gasto», «Pago»).</summary>
    public string TipoDocumento { get; private set; }

    /// <summary>Importe a partir del cual (≥) se exige aprobación.</summary>
    public decimal UmbralImporte { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<ReglaAprobacion> Crear(Guid empresaId, string? tipoDocumento, decimal umbralImporte, bool activa, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var tipo = NormalizarTipo(tipoDocumento);
        if (tipo is null)
        {
            return Resultado.Fallo<ReglaAprobacion>(Error.Validacion("aprobacion.tipo_vacio", "Indica el tipo de documento de la regla."));
        }

        if (umbralImporte < 0m)
        {
            return Resultado.Fallo<ReglaAprobacion>(Error.Validacion("aprobacion.umbral_negativo", "El umbral no puede ser negativo."));
        }

        return Resultado.Ok(new ReglaAprobacion(Guid.NewGuid(), empresaId, tipo, umbralImporte, activa, reloj.AhoraUtc));
    }

    public Resultado Actualizar(decimal umbralImporte, bool activa, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (umbralImporte < 0m)
        {
            return Resultado.Fallo(Error.Validacion("aprobacion.umbral_negativo", "El umbral no puede ser negativo."));
        }

        UmbralImporte = umbralImporte;
        Activa = activa;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>¿Un documento de este importe requiere aprobación con esta regla?</summary>
    public bool Requiere(decimal importe) => Activa && importe >= UmbralImporte;

    internal static string? NormalizarTipo(string? tipo)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return null;
        }

        var t = tipo.Trim();
        return t.Length > LongitudMaximaTipo ? t[..LongitudMaximaTipo] : t;
    }
}

/// <summary>
/// Solicitud de aprobación de una operación (documento) que ha superado el umbral. La resuelve un
/// aprobador que, por <b>segregación de funciones</b>, no puede ser el mismo usuario que la solicitó.
/// </summary>
public sealed class SolicitudAprobacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaReferencia = 120;
    public const int LongitudMaximaMotivo = 300;

    private SolicitudAprobacion(Guid id)
        : base(id, Guid.Empty)
    {
        TipoDocumento = null!;
        Referencia = null!;
    }

    private SolicitudAprobacion(Guid id, Guid empresaId, string tipoDocumento, Guid documentoId, string referencia, decimal importe, Guid solicitanteUsuarioId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        Referencia = referencia;
        Importe = importe;
        SolicitanteUsuarioId = solicitanteUsuarioId;
        Estado = EstadoAprobacion.Pendiente;
        CreadoEn = ahora;
    }

    public string TipoDocumento { get; private set; }

    /// <summary>Documento de origen (puede ser <see cref="Guid.Empty"/> si es una solicitud manual).</summary>
    public Guid DocumentoId { get; private set; }

    public string Referencia { get; private set; }

    public decimal Importe { get; private set; }

    /// <summary>Usuario que creó la solicitud (no puede aprobarla él mismo).</summary>
    public Guid SolicitanteUsuarioId { get; private set; }

    public EstadoAprobacion Estado { get; private set; }

    public Guid? AprobadorUsuarioId { get; private set; }

    public string? Motivo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? ResueltaEn { get; private set; }

    public static Resultado<SolicitudAprobacion> Crear(Guid empresaId, string? tipoDocumento, Guid documentoId, string? referencia, decimal importe, Guid solicitanteUsuarioId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var tipo = ReglaAprobacion.NormalizarTipo(tipoDocumento);
        if (tipo is null)
        {
            return Resultado.Fallo<SolicitudAprobacion>(Error.Validacion("aprobacion.tipo_vacio", "Indica el tipo de documento."));
        }

        var refe = (referencia ?? string.Empty).Trim();
        if (refe.Length == 0)
        {
            return Resultado.Fallo<SolicitudAprobacion>(Error.Validacion("aprobacion.referencia_vacia", "Indica una referencia para la solicitud."));
        }

        if (refe.Length > LongitudMaximaReferencia)
        {
            refe = refe[..LongitudMaximaReferencia];
        }

        if (solicitanteUsuarioId == Guid.Empty)
        {
            return Resultado.Fallo<SolicitudAprobacion>(Error.Validacion("aprobacion.solicitante", "No se ha identificado al solicitante."));
        }

        return Resultado.Ok(new SolicitudAprobacion(Guid.NewGuid(), empresaId, tipo, documentoId, refe, importe, solicitanteUsuarioId, reloj.AhoraUtc));
    }

    public Resultado Aprobar(Guid aprobadorUsuarioId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = ValidarResolucion(aprobadorUsuarioId);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Estado = EstadoAprobacion.Aprobada;
        AprobadorUsuarioId = aprobadorUsuarioId;
        ResueltaEn = reloj.AhoraUtc;
        RegistrarEvento(new SolicitudResuelta(Id, EmpresaId, Estado, aprobadorUsuarioId, reloj.AhoraUtc));
        return Resultado.Ok();
    }

    public Resultado Rechazar(Guid aprobadorUsuarioId, string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = ValidarResolucion(aprobadorUsuarioId);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        var m = (motivo ?? string.Empty).Trim();
        Estado = EstadoAprobacion.Rechazada;
        AprobadorUsuarioId = aprobadorUsuarioId;
        Motivo = m.Length == 0 ? null : (m.Length > LongitudMaximaMotivo ? m[..LongitudMaximaMotivo] : m);
        ResueltaEn = reloj.AhoraUtc;
        RegistrarEvento(new SolicitudResuelta(Id, EmpresaId, Estado, aprobadorUsuarioId, reloj.AhoraUtc));
        return Resultado.Ok();
    }

    private Error? ValidarResolucion(Guid aprobadorUsuarioId)
    {
        if (Estado != EstadoAprobacion.Pendiente)
        {
            return Error.Conflicto("aprobacion.ya_resuelta", "La solicitud ya está resuelta.");
        }

        if (aprobadorUsuarioId == Guid.Empty)
        {
            return Error.Validacion("aprobacion.aprobador", "No se ha identificado al aprobador.");
        }

        // Segregación de funciones: quien solicita no puede aprobar su propia solicitud.
        if (aprobadorUsuarioId == SolicitanteUsuarioId)
        {
            return Error.Conflicto("aprobacion.segregacion", "Por segregación de funciones, no puedes aprobar tu propia solicitud.");
        }

        return null;
    }
}
