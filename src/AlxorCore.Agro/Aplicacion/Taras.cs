using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Aplicacion;

public sealed record DatosTara(Guid EnvaseProductoId, decimal TaraKg, DateOnly Desde, DateOnly? Hasta = null, string? Observaciones = null);

public sealed record TaraDto(Guid Id, Guid EnvaseProductoId, decimal TaraKg, DateOnly Desde, DateOnly? Hasta, string? Observaciones, bool Usada);

/// <summary>
/// Taras de los envases, versionadas por fecha. Dar de alta una versión nueva cierra la anterior el día antes, y una
/// versión ya aplicada en alguna pesada no cambia ni se borra: las pesadas antiguas conservan la tara con que se pesaron.
/// </summary>
public sealed class TarasAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;

    public TarasAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad)
    {
        _repo = repo;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<TaraDto>> ListarAsync(Guid empresaId, Guid? envaseProductoId, CancellationToken ct = default)
    {
        var taras = await _repo.TarasAsync(empresaId, envaseProductoId, ct).ConfigureAwait(false);
        var usadas = await _repo.TarasUsadasAsync(taras.Select(t => t.Id).ToList(), ct).ConfigureAwait(false);
        return taras.Select(t => Dto(t, usadas.Contains(t.Id))).ToList();
    }

    /// <summary>La tara vigente de un envase en una fecha, si la hay.</summary>
    public async Task<TaraEnvase?> VigenteAsync(Guid empresaId, Guid envaseProductoId, DateOnly fecha, CancellationToken ct = default) =>
        (await _repo.TarasAsync(empresaId, envaseProductoId, ct).ConfigureAwait(false)).FirstOrDefault(t => t.VigenteEl(fecha));

    /// <summary>Da de alta una versión. Si hay una abierta anterior, se cierra el día antes de esta.</summary>
    public async Task<Resultado<TaraDto>> CrearAsync(Guid empresaId, DatosTara d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        await _unidad.BloquearAsync($"alxor.agro.tara:{empresaId}:{d.EnvaseProductoId}", ct).ConfigureAwait(false);
        var existentes = await _repo.TarasAsync(empresaId, d.EnvaseProductoId, ct).ConfigureAwait(false);
        foreach (var anterior in existentes.Where(t => t.Hasta is null && t.Desde < d.Desde))
        {
            var r = anterior.CerrarAntesDe(d.Desde);
            if (r.EsFallo)
            {
                return Resultado.Fallo<TaraDto>(r.Error);
            }
        }

        var hasta = d.Hasta ?? existentes.Where(t => t.Desde > d.Desde).Select(t => (DateOnly?)t.Desde.AddDays(-1)).Min();
        if (existentes.Any(t => t.Desde <= (hasta ?? DateOnly.MaxValue) && (t.Hasta ?? DateOnly.MaxValue) >= d.Desde))
        {
            return Resultado.Fallo<TaraDto>(Error.Conflicto("tara.solapada", "Ya hay una tara de ese envase en esas fechas."));
        }

        var tara = TaraEnvase.Crear(empresaId, d.EnvaseProductoId, d.TaraKg, d.Desde, hasta, d.Observaciones);
        if (tara.EsFallo)
        {
            return Resultado.Fallo<TaraDto>(tara.Error);
        }

        _repo.Agregar(tara.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(tara.Valor, false));
    }

    /// <summary>Corrige una versión que aún no se ha aplicado. Si ya se aplicó, se da de alta otra versión desde la fecha que toque.</summary>
    public async Task<Resultado<TaraDto>> ActualizarAsync(Guid id, DatosTara d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var tara = await _repo.TaraAsync(id, ct).ConfigureAwait(false);
        if (tara is null)
        {
            return Resultado.Fallo<TaraDto>(NoExiste());
        }

        if ((await _repo.TarasUsadasAsync([id], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<TaraDto>(Error.Conflicto("tara.aplicada",
                "Esa tara ya se aplicó en pesadas: no cambia. Da de alta una versión nueva desde la fecha en que cambia."));
        }

        var otras = (await _repo.TarasAsync(tara.EmpresaId, tara.EnvaseProductoId, ct).ConfigureAwait(false)).Where(t => t.Id != id);
        if (otras.Any(t => t.Desde <= (d.Hasta ?? DateOnly.MaxValue) && (t.Hasta ?? DateOnly.MaxValue) >= d.Desde))
        {
            return Resultado.Fallo<TaraDto>(Error.Conflicto("tara.solapada", "Ya hay una tara de ese envase en esas fechas."));
        }

        var r = tara.Actualizar(d.TaraKg, d.Desde, d.Hasta, d.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TaraDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(tara, false));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var tara = await _repo.TaraAsync(id, ct).ConfigureAwait(false);
        if (tara is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        if ((await _repo.TarasUsadasAsync([id], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo(Error.Conflicto("tara.aplicada", "Esa tara ya se aplicó en pesadas: no se borra."));
        }

        _repo.Eliminar(tara);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private static TaraDto Dto(TaraEnvase t, bool usada) => new(t.Id, t.EnvaseProductoId, t.TaraKg, t.Desde, t.Hasta, t.Observaciones, usada);

    private static Error NoExiste() => Error.NoEncontrado("tara.no_encontrada", "La tara no existe.");
}
