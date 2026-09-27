using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Comprobación de los meses cerrados antes de registrar un asiento (la base de datos lo garantiza además).</summary>
public static class PeriodosContables
{
    public static Error? Cerrado(DateOnly? cerradoHasta, DateOnly fecha) =>
        cerradoHasta is { } h && fecha <= h
            ? Error.Conflicto("asiento.periodo_cerrado",
                $"El mes de {fecha:MM/yyyy} está cerrado (cerrado hasta el {h:dd/MM/yyyy}): usa una fecha posterior o reabre el mes.")
            : null;

    public static async Task<Error?> ComprobarAsync(IRepositorioAsientos asientos, Guid empresaId, DateOnly fecha, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(asientos);
        return Cerrado(await asientos.CerradoHastaAsync(empresaId, ct).ConfigureAwait(false), fecha);
    }
}

/// <summary>Diario (de sistema o propio) con lo que recoge.</summary>
public sealed record DiarioDto(Guid? Id, string Codigo, string Nombre, bool DeSistema, bool Activo, IReadOnlyList<string> Origenes, int Asientos);

/// <summary>Datos de un diario propio.</summary>
public sealed record DatosDiario(string? Codigo, string? Nombre, IReadOnlyList<string>? Origenes, bool Activo = true);

public interface IRepositorioDiarios
{
    Task<IReadOnlyList<DiarioContable>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<DiarioContable?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(DiarioContable diario);

    void Eliminar(DiarioContable diario);

    /// <summary>Número de asientos de cada diario (todos los ejercicios).</summary>
    Task<IReadOnlyDictionary<string, int>> AsientosPorDiarioAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>¿Existe el diario (de sistema, o propio y activo)?</summary>
    async Task<bool> ExisteActivoAsync(Guid empresaId, string codigo, CancellationToken ct = default) =>
        DiariosContables.EsDeSistema(codigo)
        || (await ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(d => d.Activo && string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Diarios de asientos: los de sistema y los propios de la empresa, que pueden recoger los asientos de algunos orígenes
/// (cada origen, en un solo diario propio).
/// </summary>
public sealed class GestionDiarios
{
    private readonly IRepositorioDiarios _repo;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public GestionDiarios(IRepositorioDiarios repo, IUnidadDeTrabajoContabilidad unidad) { _repo = repo; _unidad = unidad; }

    public async Task<IReadOnlyList<DiarioDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var propios = await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var usos = await _repo.AsientosPorDiarioAsync(empresaId, ct).ConfigureAwait(false);
        var tomados = propios.Where(p => p.Activo).SelectMany(p => p.Origenes).ToHashSet(StringComparer.Ordinal);
        return DiariosContables.Sistema
            .Select(d => new DiarioDto(null, d.Codigo, d.Nombre, true, true, d.Origenes.Where(o => !tomados.Contains(o)).ToList(), usos.GetValueOrDefault(d.Codigo)))
            .Concat(propios.OrderBy(p => p.Codigo, StringComparer.Ordinal).Select(p => Dto(p, usos)))
            .ToList();
    }

    public async Task<Resultado<DiarioDto>> CrearAsync(Guid empresaId, DatosDiario datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var propios = await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var d = DiarioContable.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Origenes);
        if (d.EsFallo)
        {
            return Resultado.Fallo<DiarioDto>(d.Error);
        }

        if (propios.Any(p => string.Equals(p.Codigo, d.Valor.Codigo, StringComparison.Ordinal)))
        {
            return Resultado.Fallo<DiarioDto>(Error.Conflicto("diario.duplicado", $"Ya hay un diario «{d.Valor.Codigo}»."));
        }

        if (OrigenRepetido(propios, d.Valor) is { } error)
        {
            return Resultado.Fallo<DiarioDto>(error);
        }

        _repo.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(d.Valor, new Dictionary<string, int>()));
    }

    public async Task<Resultado<DiarioDto>> ModificarAsync(Guid empresaId, Guid id, DatosDiario datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var d = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo<DiarioDto>(Error.NoEncontrado("diario.no_encontrado", "El diario no existe."));
        }

        var r = d.Modificar(datos.Nombre, datos.Origenes, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DiarioDto>(r.Error);
        }

        if (OrigenRepetido(await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false), d) is { } error)
        {
            return Resultado.Fallo<DiarioDto>(error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(d, await _repo.AsientosPorDiarioAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>Elimina un diario sin asientos (el que tiene asientos se da de baja).</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var d = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null || d.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("diario.no_encontrado", "El diario no existe."));
        }

        if ((await _repo.AsientosPorDiarioAsync(empresaId, ct).ConfigureAwait(false)).GetValueOrDefault(d.Codigo) > 0)
        {
            return Resultado.Fallo(Error.Conflicto("diario.en_uso", $"El diario «{d.Codigo}» tiene asientos: dalo de baja en lugar de eliminarlo."));
        }

        _repo.Eliminar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private static Error? OrigenRepetido(IReadOnlyList<DiarioContable> propios, DiarioContable d) =>
        !d.Activo ? null
        : propios.Where(p => p.Id != d.Id && p.Activo).SelectMany(p => p.Origenes.Where(d.Origenes.Contains).Select(o => (p.Codigo, Origen: o))).FirstOrDefault() is { Codigo: not null } x
            ? Error.Conflicto("diario.origen_repetido", $"Los asientos de «{x.Origen}» ya van al diario «{x.Codigo}».")
            : null;

    private static DiarioDto Dto(DiarioContable d, IReadOnlyDictionary<string, int> usos) =>
        new(d.Id, d.Codigo, d.Nombre, false, d.Activo, d.Origenes, usos.GetValueOrDefault(d.Codigo));
}

/// <summary>Un mes del ejercicio: si está cerrado, sus asientos y lo que queda pendiente de contabilizar con esa fecha.</summary>
public sealed record MesContableDto(int Mes, DateOnly Desde, DateOnly Hasta, bool Cerrado, int Asientos, int Pendientes, int Periodificaciones = 0);

public sealed record PeriodosContablesDto(int Ejercicio, DateOnly? CerradoHasta, IReadOnlyList<MesContableDto> Meses);

/// <summary>
/// Cierre mensual: una vez cerrado un mes (normalmente al presentar el IVA), no se registran asientos con fecha de ese
/// mes ni anteriores. Se cierra hasta un mes, sin huecos, y se reabre desde un mes.
/// </summary>
public sealed class CierreMensual
{
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioDocumentosPendientes _pendientes;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IRepositorioPeriodificaciones? _periodificaciones;

    public CierreMensual(IRepositorioConfigContabilidad config, IRepositorioAsientos asientos, IRepositorioDocumentosPendientes pendientes, IUnidadDeTrabajoContabilidad unidad,
        IRepositorioPeriodificaciones? periodificaciones = null)
    {
        _config = config; _asientos = asientos; _pendientes = pendientes; _unidad = unidad; _periodificaciones = periodificaciones;
    }

    /// <summary>Fechas de las cuotas de periodificación sin contabilizar (de las periodificaciones activas).</summary>
    private async Task<IReadOnlyList<DateOnly>> CuotasPendientesAsync(Guid empresaId, CancellationToken ct) =>
        _periodificaciones is null ? []
            : (await _periodificaciones.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Estado == EstadoPeriodificacion.Activa)
                .SelectMany(p => p.Cuotas.Where(c => c.AsientoId is null).Select(c => c.Fecha)).ToList();

    public async Task<PeriodosContablesDto> EstadoAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var cerrado = (await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.CerradoHasta;
        var asientos = await _asientos.DiarioAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var pendientes = (await _pendientes.ListarPendientesAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.FechaRegistro.Year == ejercicio).ToList();
        var cuotas = (await CuotasPendientesAsync(empresaId, ct).ConfigureAwait(false)).Where(f => f.Year == ejercicio).ToList();
        var meses = Enumerable.Range(1, 12).Select(m =>
        {
            var desde = new DateOnly(ejercicio, m, 1);
            var hasta = desde.AddMonths(1).AddDays(-1);
            return new MesContableDto(m, desde, hasta, cerrado is { } h && hasta <= h, asientos.Count(a => a.Fecha.Month == m),
                pendientes.Count(p => p.FechaRegistro.Month == m), cuotas.Count(f => f.Month == m));
        }).ToList();
        return new PeriodosContablesDto(ejercicio, cerrado, meses);
    }

    /// <summary>
    /// Cierra hasta el final del mes. Con documentos pendientes de contabilizar en los meses que se cierran, no cierra
    /// salvo que se fuerce (se quedarían pendientes y habría que contabilizarlos con otra fecha).
    /// </summary>
    public async Task<Resultado<PeriodosContablesDto>> CerrarAsync(Guid empresaId, int anio, int mes, bool forzar, CancellationToken ct = default)
    {
        var config = await ObtenerConfigAsync(empresaId, ct).ConfigureAwait(false);
        if (mes is >= 1 and <= 12 && !forzar)
        {
            var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
            var n = (await _pendientes.ListarPendientesAsync(empresaId, ct).ConfigureAwait(false))
                .Count(p => p.FechaRegistro <= fin && (config.CerradoHasta is not { } h || p.FechaRegistro > h));
            if (n > 0)
            {
                return Resultado.Fallo<PeriodosContablesDto>(Error.Conflicto("periodo.pendientes",
                    $"Hay {n} documento(s) pendiente(s) de contabilizar con fecha hasta el {fin:dd/MM/yyyy}: contabilízalos antes, o cierra igualmente."));
            }

            var cuotas = (await CuotasPendientesAsync(empresaId, ct).ConfigureAwait(false)).Count(f => f <= fin && (config.CerradoHasta is not { } h2 || f > h2));
            if (cuotas > 0)
            {
                return Resultado.Fallo<PeriodosContablesDto>(Error.Conflicto("periodo.pendientes",
                    $"Hay {cuotas} cuota(s) de periodificación sin contabilizar hasta el {fin:dd/MM/yyyy}: genéralas antes, o cierra igualmente."));
            }
        }

        var r = config.CerrarHasta(anio, mes);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PeriodosContablesDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await EstadoAsync(empresaId, anio, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<PeriodosContablesDto>> ReabrirAsync(Guid empresaId, int anio, int mes, CancellationToken ct = default)
    {
        if (await _asientos.TieneCierreAsync(empresaId, anio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<PeriodosContablesDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {anio} está cerrado: sus meses no se reabren."));
        }

        var config = await ObtenerConfigAsync(empresaId, ct).ConfigureAwait(false);
        var r = config.ReabrirDesde(anio, mes);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PeriodosContablesDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await EstadoAsync(empresaId, anio, ct).ConfigureAwait(false));
    }

    private async Task<ConfiguracionContabilidad> ObtenerConfigAsync(Guid empresaId, CancellationToken ct)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null)
        {
            config = new ConfiguracionContabilidad(empresaId, ModoContabilidad.Simple);
            _config.Agregar(config);
        }

        return config;
    }
}
