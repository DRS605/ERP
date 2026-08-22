using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Datos para asignar una serie a un tipo de documento (empresa por defecto o un tercero).</summary>
public sealed record AsignarSerieComando(TipoDocumento TipoDocumento, AmbitoSerie Ambito, Guid? TerceroId, string Prefijo);

/// <summary>Caso de uso: asignar una serie (crea la asignación) para la empresa activa.</summary>
public sealed class AsignarSerie
{
    private readonly IRepositorioAsignacionesSerie _asignaciones;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public AsignarSerie(IRepositorioAsignacionesSerie asignaciones, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _asignaciones = asignaciones;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<AsignacionSerieDto>> EjecutarAsync(Guid empresaId, AsignarSerieComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var asignacion = AsignacionSerie.Crear(empresaId, comando.TipoDocumento, comando.Ambito, comando.TerceroId, comando.Prefijo, _reloj);
        if (asignacion.EsFallo)
        {
            return Resultado.Fallo<AsignacionSerieDto>(asignacion.Error);
        }

        var terceroId = asignacion.Valor.TerceroId;
        if (await _asignaciones.ExisteAsync(empresaId, comando.TipoDocumento, comando.Ambito, terceroId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<AsignacionSerieDto>(Error.Conflicto("serie.asignacion_duplicada", "Ya existe una serie asignada para ese documento y ámbito."));
        }

        _asignaciones.Agregar(asignacion.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AsignacionSerieDto.Desde(asignacion.Valor));
    }
}

/// <summary>Caso de uso: eliminar una asignación de serie.</summary>
public sealed class EliminarAsignacionSerie
{
    private readonly IRepositorioAsignacionesSerie _asignaciones;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;

    public EliminarAsignacionSerie(IRepositorioAsignacionesSerie asignaciones, IUnidadDeTrabajoOrganizacion unidadDeTrabajo)
    {
        _asignaciones = asignaciones;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Resultado> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var asignacion = await _asignaciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (asignacion is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("serie.asignacion_no_encontrada", "No se encontró la asignación de serie."));
        }

        _asignaciones.Eliminar(asignacion);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Caso de uso: listar las asignaciones de serie de la empresa activa.</summary>
public sealed class ListarAsignacionesSerie
{
    private readonly IRepositorioAsignacionesSerie _asignaciones;

    public ListarAsignacionesSerie(IRepositorioAsignacionesSerie asignaciones) => _asignaciones = asignaciones;

    public async Task<IReadOnlyList<AsignacionSerieDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var asignaciones = await _asignaciones.ListarAsync(empresaId, ct).ConfigureAwait(false);
        return asignaciones.Select(AsignacionSerieDto.Desde).ToList();
    }
}
