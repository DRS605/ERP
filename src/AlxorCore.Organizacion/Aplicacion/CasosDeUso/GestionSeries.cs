using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Datos para crear una serie de numeración.</summary>
public sealed record CrearSerieComando(TipoDocumento TipoDocumento, int Ejercicio, string Prefijo);

/// <summary>Caso de uso: crear una serie de numeración para la empresa activa.</summary>
public sealed class CrearSerie
{
    private readonly IRepositorioSeries _series;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearSerie(IRepositorioSeries series, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _series = series;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<SerieDto>> EjecutarAsync(Guid empresaId, CrearSerieComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var prefijo = (comando.Prefijo ?? string.Empty).Trim().ToUpperInvariant();
        if (await _series.ExisteAsync(empresaId, comando.TipoDocumento, comando.Ejercicio, prefijo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<SerieDto>(Error.Conflicto("serie.duplicada", "Ya existe una serie con ese prefijo y ejercicio."));
        }

        var serie = SerieNumeracion.Crear(empresaId, comando.TipoDocumento, comando.Ejercicio, prefijo, _reloj);
        if (serie.EsFallo)
        {
            return Resultado.Fallo<SerieDto>(serie.Error);
        }

        _series.Agregar(serie.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SerieDto.Desde(serie.Valor));
    }
}

/// <summary>Caso de uso: listar las series de la empresa activa.</summary>
public sealed class ListarSeries
{
    private readonly IRepositorioSeries _series;

    public ListarSeries(IRepositorioSeries series) => _series = series;

    public async Task<IReadOnlyList<SerieDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var series = await _series.ListarAsync(empresaId, ct).ConfigureAwait(false);
        return series.Select(SerieDto.Desde).ToList();
    }
}

/// <summary>
/// Elimina una serie creada por error: solo si todavía no ha numerado ningún documento (una serie usada no se borra:
/// la numeración correlativa sin huecos es obligatoria en las facturas).
/// </summary>
public sealed class EliminarSerie
{
    private readonly IRepositorioSeries _series;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;

    public EliminarSerie(IRepositorioSeries series, IUnidadDeTrabajoOrganizacion unidadDeTrabajo)
    {
        _series = series;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Resultado<BajaDto>> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var serie = await _series.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (serie is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("serie.no_encontrada", "La serie no existe."));
        }

        if (serie.SiguienteNumero > 1)
        {
            return Resultado.Fallo<BajaDto>(Error.Conflicto("serie.en_uso",
                $"La serie {serie.Prefijo} ya ha numerado {serie.SiguienteNumero - 1} documento(s): no se puede eliminar."));
        }

        _series.Eliminar(serie);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }
}
