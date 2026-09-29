using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioAutoevaluaciones
{
    void Agregar(ListaControl lista);

    void Agregar(Autoevaluacion evaluacion);

    void Eliminar(ListaControl lista);

    void Eliminar(Autoevaluacion evaluacion);

    Task<ListaControl?> ListaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ListaControl>> ListasAsync(Guid empresaId, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default);

    Task<bool> ListaUsadaAsync(Guid listaId, CancellationToken ct = default);

    Task<Autoevaluacion?> EvaluacionAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Autoevaluacion>> EvaluacionesAsync(Guid empresaId, Guid? agricultorId, int? anio, CancellationToken ct = default);
}

public sealed record DatosPuntoControl(string? Codigo, string? Texto, NivelPuntoControl Nivel);

public sealed record DatosListaControl(string? Codigo, string? Nombre, string? Version, IReadOnlyList<DatosPuntoControl>? Puntos, bool Activa = true);

public sealed record PuntoControlDto(string Codigo, string Texto, string Nivel);

public sealed record ListaControlDto(Guid Id, string Codigo, string Nombre, string? Version, bool Activa, IReadOnlyList<PuntoControlDto> Puntos);

public sealed record DatosAutoevaluacion(Guid ListaControlId, Guid? AgricultorId, TipoAutoevaluacion Tipo, DateOnly Fecha, string? Auditor);

public sealed record DatosRespuesta(string Codigo, ResultadoPunto? Resultado, string? Comentario = null, string? AccionCorrectiva = null, DateOnly? FechaLimite = null);

public sealed record DatosRespuestas(IReadOnlyList<DatosRespuesta>? Respuestas, string? Auditor = null, string? Observaciones = null);

public sealed record RespuestaDto(string Codigo, string Texto, string Nivel, string? Resultado, string? Comentario, string? AccionCorrectiva, DateOnly? FechaLimite);

public sealed record AutoevaluacionDto(Guid Id, Guid ListaControlId, string Lista, Guid? AgricultorId, string? Agricultor, string Tipo, DateOnly Fecha, string Auditor,
    string? Observaciones, bool Cerrada, int Puntos, int Respondidos, int NoCumple, decimal CumplimientoMayores, decimal CumplimientoMenores, bool Supera,
    IReadOnlyList<RespuestaDto> Respuestas);

/// <summary>
/// GlobalG.A.P.: listas de puntos de control y autoevaluaciones o auditorías internas del productor o de la empresa.
/// Cada evaluación copia los puntos de su lista; se responden, se cierran (con justificación de los que no aplican y
/// acción correctiva de los que no se cumplen) y ya no cambian.
/// </summary>
public sealed class AutoevaluacionesAgro
{
    private readonly IRepositorioAutoevaluaciones _repo;
    private readonly IRepositorioAgro _agro;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    public AutoevaluacionesAgro(IRepositorioAutoevaluaciones repo, IRepositorioAgro agro, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _repo = repo;
        _agro = agro;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<ListaControlDto>> ListasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListasAsync(empresaId, ct).ConfigureAwait(false)).Select(Dto).ToList();

    public async Task<Resultado<ListaControlDto>> CrearListaAsync(Guid empresaId, DatosListaControl d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (!string.IsNullOrWhiteSpace(d.Codigo) && await _repo.ExisteCodigoAsync(empresaId, d.Codigo.Trim().ToUpperInvariant(), ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ListaControlDto>(Error.Conflicto("lista_control.codigo_repetido", "Ya hay una lista con ese código."));
        }

        var l = ListaControl.Crear(empresaId, d.Codigo, d.Nombre, d.Version, Puntos(d));
        if (l.EsFallo)
        {
            return Resultado.Fallo<ListaControlDto>(l.Error);
        }

        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l.Valor));
    }

    public async Task<Resultado<ListaControlDto>> ActualizarListaAsync(Guid id, DatosListaControl d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var l = await _repo.ListaAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo<ListaControlDto>(Error.NoEncontrado("lista_control.no_encontrada", "La lista de control no existe."));
        }

        var r = l.Actualizar(d.Nombre, d.Version, d.Activa, Puntos(d));
        if (r.EsFallo)
        {
            return Resultado.Fallo<ListaControlDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l));
    }

    /// <summary>Elimina la lista si no tiene evaluaciones; si las tiene, la da de baja. Devuelve si se eliminó.</summary>
    public async Task<Resultado<bool>> EliminarListaAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.ListaAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo<bool>(Error.NoEncontrado("lista_control.no_encontrada", "La lista de control no existe."));
        }

        var usada = await _repo.ListaUsadaAsync(id, ct).ConfigureAwait(false);
        if (usada)
        {
            l.DarDeBaja();
        }
        else
        {
            _repo.Eliminar(l);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(!usada);
    }

    public async Task<IReadOnlyList<AutoevaluacionDto>> ListarAsync(Guid empresaId, Guid? agricultorId, int? anio, CancellationToken ct = default)
    {
        var lista = await _repo.EvaluacionesAsync(empresaId, agricultorId, anio, ct).ConfigureAwait(false);
        var nombres = new Dictionary<Guid, string?>();
        foreach (var a in lista.Where(a => a.AgricultorId is not null).Select(a => a.AgricultorId!.Value).Distinct())
        {
            nombres[a] = (await _agro.AgricultorAsync(a, ct).ConfigureAwait(false))?.Nombre;
        }

        return lista.Select(a => Dto(a, a.AgricultorId is { } g ? nombres.GetValueOrDefault(g) : null)).ToList();
    }

    public async Task<AutoevaluacionDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.EvaluacionAsync(id, ct).ConfigureAwait(false);
        return a is null ? null : await DtoAsync(a, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<AutoevaluacionDto>> AbrirAsync(Guid empresaId, DatosAutoevaluacion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var lista = await _repo.ListaAsync(d.ListaControlId, ct).ConfigureAwait(false);
        if (lista is null)
        {
            return Resultado.Fallo<AutoevaluacionDto>(Error.NoEncontrado("lista_control.no_encontrada", "La lista de control no existe."));
        }

        if (d.AgricultorId is { } ag && await _agro.AgricultorAsync(ag, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<AutoevaluacionDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        var a = Autoevaluacion.Abrir(empresaId, lista, d.AgricultorId, d.Tipo, d.Fecha, d.Auditor, _reloj);
        if (a.EsFallo)
        {
            return Resultado.Fallo<AutoevaluacionDto>(a.Error);
        }

        _repo.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(a.Valor, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<AutoevaluacionDto>> ResponderAsync(Guid id, DatosRespuestas d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var a = await _repo.EvaluacionAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AutoevaluacionDto>(Error.NoEncontrado("autoevaluacion.no_encontrada", "La evaluación no existe."));
        }

        var r = a.Responder((d.Respuestas ?? []).Select(x => (x.Codigo, x.Resultado, x.Comentario, x.AccionCorrectiva, x.FechaLimite)).ToList(), d.Auditor, d.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AutoevaluacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(a, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<AutoevaluacionDto>> CerrarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.EvaluacionAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<AutoevaluacionDto>(Error.NoEncontrado("autoevaluacion.no_encontrada", "La evaluación no existe."));
        }

        var r = a.Cerrar(_reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<AutoevaluacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(a, ct).ConfigureAwait(false));
    }

    /// <summary>Elimina una evaluación abierta (una cerrada es el registro de la auditoría y no se borra).</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.EvaluacionAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("autoevaluacion.no_encontrada", "La evaluación no existe."));
        }

        if (a.Cerrada)
        {
            return Resultado.Fallo(Error.Conflicto("autoevaluacion.cerrada", "La evaluación está cerrada: es el registro de la auditoría y no se borra."));
        }

        _repo.Eliminar(a);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private static List<(string?, string?, NivelPuntoControl)> Puntos(DatosListaControl d) =>
        (d.Puntos ?? []).Select(p => (p.Codigo, p.Texto, p.Nivel)).ToList();

    private static ListaControlDto Dto(ListaControl l) => new(l.Id, l.Codigo, l.Nombre, l.Version, l.Activa,
        l.Puntos.OrderBy(p => p.Orden).Select(p => new PuntoControlDto(p.Codigo, p.Texto, p.Nivel.ToString())).ToList());

    private async Task<AutoevaluacionDto> DtoAsync(Autoevaluacion a, CancellationToken ct) =>
        Dto(a, a.AgricultorId is { } g ? (await _agro.AgricultorAsync(g, ct).ConfigureAwait(false))?.Nombre : null);

    private static AutoevaluacionDto Dto(Autoevaluacion a, string? agricultor)
    {
        var respuestas = a.Respuestas.OrderBy(r => r.Orden).Select(r => new RespuestaDto(r.Codigo, r.Texto, r.Nivel.ToString(), r.Resultado?.ToString(), r.Comentario,
            r.AccionCorrectiva, r.FechaLimite)).ToList();
        return new AutoevaluacionDto(a.Id, a.ListaControlId, a.ListaCodigo, a.AgricultorId, agricultor, a.Tipo.ToString(), a.Fecha, a.Auditor, a.Observaciones, a.Cerrada,
            respuestas.Count, respuestas.Count(r => r.Resultado is not null), a.Respuestas.Count(r => r.Resultado == ResultadoPunto.NoCumple),
            a.Cumplimiento(NivelPuntoControl.Mayor), a.Cumplimiento(NivelPuntoControl.Menor), a.Supera, respuestas);
    }
}
