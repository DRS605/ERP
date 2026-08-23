using AlxorCore.Aprobaciones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Aprobaciones.Aplicacion;

/// <summary>Vista de una regla de aprobación.</summary>
public sealed record ReglaAprobacionDto(Guid Id, string TipoDocumento, decimal UmbralImporte, bool Activa)
{
    public static ReglaAprobacionDto Desde(ReglaAprobacion r) => new(r.Id, r.TipoDocumento, r.UmbralImporte, r.Activa);
}

/// <summary>Vista de una solicitud de aprobación.</summary>
public sealed record SolicitudAprobacionDto(
    Guid Id, string TipoDocumento, Guid DocumentoId, string Referencia, decimal Importe,
    Guid SolicitanteUsuarioId, string Estado, Guid? AprobadorUsuarioId, string? Motivo,
    DateTimeOffset CreadoEn, DateTimeOffset? ResueltaEn)
{
    public static SolicitudAprobacionDto Desde(SolicitudAprobacion s) => new(
        s.Id, s.TipoDocumento, s.DocumentoId, s.Referencia, s.Importe, s.SolicitanteUsuarioId,
        s.Estado.ToString(), s.AprobadorUsuarioId, s.Motivo, s.CreadoEn, s.ResueltaEn);
}

/// <summary>Unidad de trabajo del módulo Aprobaciones.</summary>
public interface IUnidadDeTrabajoAprobaciones : IUnidadDeTrabajo;

/// <summary>Repositorio de reglas de aprobación.</summary>
public interface IRepositorioReglas
{
    Task<ReglaAprobacion?> ObtenerPorTipoAsync(Guid empresaId, string tipoDocumento, CancellationToken ct = default);

    void Agregar(ReglaAprobacion regla);

    Task<IReadOnlyList<ReglaAprobacion>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Repositorio de solicitudes de aprobación.</summary>
public interface IRepositorioSolicitudes
{
    Task<SolicitudAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(SolicitudAprobacion solicitud);

    Task<IReadOnlyList<SolicitudAprobacion>> ListarAsync(Guid empresaId, EstadoAprobacion? estado, CancellationToken ct = default);
}

/// <summary>Caso de uso: configurar (crear o actualizar) la regla de aprobación de un tipo de documento.</summary>
public sealed class ConfigurarRegla
{
    private readonly IRepositorioReglas _reglas;
    private readonly IUnidadDeTrabajoAprobaciones _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ConfigurarRegla(IRepositorioReglas reglas, IUnidadDeTrabajoAprobaciones unidadDeTrabajo, IReloj reloj)
    {
        _reglas = reglas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ReglaAprobacionDto>> EjecutarAsync(Guid empresaId, string? tipoDocumento, decimal umbralImporte, bool activa, CancellationToken ct = default)
    {
        var tipo = ReglaAprobacion.NormalizarTipo(tipoDocumento);
        if (tipo is not null)
        {
            var existente = await _reglas.ObtenerPorTipoAsync(empresaId, tipo, ct).ConfigureAwait(false);
            if (existente is not null)
            {
                var act = existente.Actualizar(umbralImporte, activa, _reloj);
                if (act.EsFallo)
                {
                    return Resultado.Fallo<ReglaAprobacionDto>(act.Error);
                }

                await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
                return Resultado.Ok(ReglaAprobacionDto.Desde(existente));
            }
        }

        var regla = ReglaAprobacion.Crear(empresaId, tipoDocumento, umbralImporte, activa, _reloj);
        if (regla.EsFallo)
        {
            return Resultado.Fallo<ReglaAprobacionDto>(regla.Error);
        }

        _reglas.Agregar(regla.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaAprobacionDto.Desde(regla.Valor));
    }
}

/// <summary>Caso de uso: listar las reglas de aprobación de la empresa.</summary>
public sealed class ListarReglas
{
    private readonly IRepositorioReglas _reglas;

    public ListarReglas(IRepositorioReglas reglas) => _reglas = reglas;

    public async Task<IReadOnlyList<ReglaAprobacionDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _reglas.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(ReglaAprobacionDto.Desde).ToList();
}

/// <summary>Datos para abrir una solicitud de aprobación.</summary>
public sealed record CrearSolicitudComando(string TipoDocumento, Guid DocumentoId, string Referencia, decimal Importe);

/// <summary>Caso de uso: abrir una solicitud de aprobación (el solicitante es el usuario actual).</summary>
public sealed class CrearSolicitud
{
    private readonly IRepositorioSolicitudes _solicitudes;
    private readonly IUnidadDeTrabajoAprobaciones _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearSolicitud(IRepositorioSolicitudes solicitudes, IUnidadDeTrabajoAprobaciones unidadDeTrabajo, IReloj reloj)
    {
        _solicitudes = solicitudes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<SolicitudAprobacionDto>> EjecutarAsync(Guid empresaId, Guid solicitanteUsuarioId, CrearSolicitudComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var solicitud = SolicitudAprobacion.Crear(empresaId, comando.TipoDocumento, comando.DocumentoId, comando.Referencia, comando.Importe, solicitanteUsuarioId, _reloj);
        if (solicitud.EsFallo)
        {
            return Resultado.Fallo<SolicitudAprobacionDto>(solicitud.Error);
        }

        _solicitudes.Agregar(solicitud.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SolicitudAprobacionDto.Desde(solicitud.Valor));
    }
}

/// <summary>Caso de uso: resolver (aprobar o rechazar) una solicitud, aplicando la segregación de funciones.</summary>
public sealed class ResolverSolicitud
{
    private readonly IRepositorioSolicitudes _solicitudes;
    private readonly IUnidadDeTrabajoAprobaciones _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ResolverSolicitud(IRepositorioSolicitudes solicitudes, IUnidadDeTrabajoAprobaciones unidadDeTrabajo, IReloj reloj)
    {
        _solicitudes = solicitudes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public Task<Resultado<SolicitudAprobacionDto>> AprobarAsync(Guid solicitudId, Guid aprobadorUsuarioId, CancellationToken ct = default) =>
        ResolverAsync(solicitudId, s => s.Aprobar(aprobadorUsuarioId, _reloj), ct);

    public Task<Resultado<SolicitudAprobacionDto>> RechazarAsync(Guid solicitudId, Guid aprobadorUsuarioId, string? motivo, CancellationToken ct = default) =>
        ResolverAsync(solicitudId, s => s.Rechazar(aprobadorUsuarioId, motivo, _reloj), ct);

    private async Task<Resultado<SolicitudAprobacionDto>> ResolverAsync(Guid solicitudId, Func<SolicitudAprobacion, Resultado> accion, CancellationToken ct)
    {
        var solicitud = await _solicitudes.ObtenerPorIdAsync(solicitudId, ct).ConfigureAwait(false);
        if (solicitud is null)
        {
            return Resultado.Fallo<SolicitudAprobacionDto>(Error.NoEncontrado("aprobacion.no_encontrada", "La solicitud no existe."));
        }

        var r = accion(solicitud);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SolicitudAprobacionDto>(r.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SolicitudAprobacionDto.Desde(solicitud));
    }
}

/// <summary>Caso de uso: listar solicitudes de aprobación, opcionalmente por estado.</summary>
public sealed class ListarSolicitudes
{
    private readonly IRepositorioSolicitudes _solicitudes;

    public ListarSolicitudes(IRepositorioSolicitudes solicitudes) => _solicitudes = solicitudes;

    public async Task<IReadOnlyList<SolicitudAprobacionDto>> EjecutarAsync(Guid empresaId, string? estado, CancellationToken ct = default)
    {
        EstadoAprobacion? filtro = Enum.TryParse<EstadoAprobacion>(estado, ignoreCase: true, out var e) ? e : null;
        var lista = await _solicitudes.ListarAsync(empresaId, filtro, ct).ConfigureAwait(false);
        return lista.Select(SolicitudAprobacionDto.Desde).ToList();
    }
}

/// <summary>Caso de uso: ¿requiere aprobación una operación de este tipo e importe?</summary>
public sealed class ConsultarRequiere
{
    private readonly IRepositorioReglas _reglas;

    public ConsultarRequiere(IRepositorioReglas reglas) => _reglas = reglas;

    public async Task<bool> EjecutarAsync(Guid empresaId, string? tipoDocumento, decimal importe, CancellationToken ct = default)
    {
        var tipo = ReglaAprobacion.NormalizarTipo(tipoDocumento);
        if (tipo is null)
        {
            return false;
        }

        var regla = await _reglas.ObtenerPorTipoAsync(empresaId, tipo, ct).ConfigureAwait(false);
        return regla is not null && regla.Requiere(importe);
    }
}

/// <summary>Implementación del puerto transversal <see cref="IFlujoAprobaciones"/>.</summary>
public sealed class FlujoAprobaciones : IFlujoAprobaciones
{
    private readonly ConsultarRequiere _requiere;
    private readonly CrearSolicitud _crear;

    public FlujoAprobaciones(ConsultarRequiere requiere, CrearSolicitud crear)
    {
        _requiere = requiere;
        _crear = crear;
    }

    public Task<bool> RequiereAprobacionAsync(Guid empresaId, string tipoDocumento, decimal importe, CancellationToken ct = default) =>
        _requiere.EjecutarAsync(empresaId, tipoDocumento, importe, ct);

    public async Task<Guid> AbrirSolicitudAsync(Guid empresaId, string tipoDocumento, Guid documentoId, string referencia, decimal importe, Guid solicitanteUsuarioId, CancellationToken ct = default)
    {
        var r = await _crear.EjecutarAsync(empresaId, solicitanteUsuarioId, new CrearSolicitudComando(tipoDocumento, documentoId, referencia, importe), ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.Valor.Id : Guid.Empty;
    }
}
