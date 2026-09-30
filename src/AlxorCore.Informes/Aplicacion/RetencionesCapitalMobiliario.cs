using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>Rendimiento del capital mobiliario pagado a un perceptor, con su retención (retornos e intereses a socios).</summary>
public sealed record RendimientoCapitalDto(Guid? PerceptorId, string Nombre, string? Nif, string? Provincia, decimal Integro, decimal Retencion);

/// <summary>
/// Puerto hacia quien paga rendimientos del capital mobiliario: hoy, los retornos cooperativos y los intereses de las
/// aportaciones de la cooperativa (lo implementa la API). Sin módulo que los pague, no hay rendimientos.
/// </summary>
public interface IRendimientosCapitalMobiliario
{
    Task<IReadOnlyList<RendimientoCapitalDto>> RendimientosAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
}

public sealed class SinRendimientosCapitalMobiliario : IRendimientosCapitalMobiliario
{
    public Task<IReadOnlyList<RendimientoCapitalDto>> RendimientosAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<RendimientoCapitalDto>>([]);
}

/// <summary>
/// Modelos 123 (retenciones del trimestre sobre rendimientos del capital mobiliario) y 193 (su resumen anual por
/// perceptor), a partir de los retornos e intereses pagados a los socios con retención. Ayuda para la gestoría: la
/// clave y la naturaleza de cada percepción del 193 se revisan antes de presentarlo.
/// </summary>
public sealed class GenerarRetencionesCapital
{
    /// <summary>Clave A del 193: rendimientos por participación en fondos propios de entidades (retornos cooperativos).</summary>
    public const string ClaveParticipacion = "A";

    private readonly IRendimientosCapitalMobiliario _rendimientos;

    public GenerarRetencionesCapital(IRendimientosCapitalMobiliario rendimientos) => _rendimientos = rendimientos;

    public async Task<ResumenRetencionesDto> Modelo123Async(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        var (desde, hasta) = GenerarRetencionesIrpf.RangoTrimestre(anio, trimestre);
        var p = await PerceptoresAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        return ResumenRetencionesDto.De("123", anio, trimestre, p);
    }

    public async Task<AnualRetencionesDto> Modelo193Async(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var p = await PerceptoresAsync(empresaId, new DateOnly(anio, 1, 1), new DateOnly(anio, 12, 31), ct).ConfigureAwait(false);
        return AnualRetencionesDto.De("193", anio, p.Where(x => x.Nif is { Length: > 0 }).ToList(), p.Where(x => x.Nif is not { Length: > 0 }).ToList());
    }

    private async Task<List<PerceptorRetencionDto>> PerceptoresAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct) =>
        (await _rendimientos.RendimientosAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .GroupBy(r => r.PerceptorId?.ToString() ?? r.Nif ?? r.Nombre)
            .Select(g => new PerceptorRetencionDto(ClaveParticipacion, g.First().PerceptorId, g.First().Nombre, g.First().Nif, g.First().Provincia,
                Redondeo.Dos(g.Sum(r => r.Integro)), Redondeo.Dos(g.Sum(r => r.Retencion))))
            .Where(p => p.BasePercepciones != 0m || p.Retenciones != 0m)
            .OrderByDescending(p => p.Retenciones).ThenBy(p => p.Nombre, StringComparer.OrdinalIgnoreCase)
            .ToList();
}
