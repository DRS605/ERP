using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>Bien de inversión con impuesto soportado sujeto a regularización (art. 107 LIVA; igual en el IGIC).</summary>
/// <param name="Regularizar">Cálculo del bien: del ejercicio y el porcentaje definitivo, lo que se regulariza.</param>
public sealed record BienInversion(Guid Id, string Codigo, string Descripcion, DateOnly FechaAdquisicion, DateOnly InicioUtilizacion, decimal Cuota,
    int PorcentajeInicial, int Anios, DateOnly? FechaBaja, Func<int, bool> EnPeriodo, Func<int, int, decimal> Regularizar);

/// <summary>Puerto: los bienes de inversión de la empresa (del inmovilizado de contabilidad).</summary>
public interface IConsultaBienesInversion
{
    Task<IReadOnlyList<BienInversion>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

public sealed record LineaRegularizacionBienDto(Guid InmovilizadoId, string Codigo, string Descripcion, DateOnly FechaAdquisicion, int AnioDelPeriodo, int Anios,
    decimal Cuota, int PorcentajeInicial, int PorcentajeDefinitivo, int Diferencia, decimal Regularizacion);

/// <summary>Regularización de los bienes de inversión de un ejercicio: va a la casilla 43 del 303 (o al 420) del último periodo.</summary>
public sealed record RegularizacionBienesInversionDto(int Ejercicio, int PorcentajeDefinitivo, IReadOnlyList<LineaRegularizacionBienDto> Bienes, decimal Total);

/// <summary>
/// Caso de uso: regularización anual de los bienes de inversión (art. 107 LIVA). Con el porcentaje definitivo del
/// ejercicio, cada bien en su periodo (5 años, 10 los inmuebles) ajusta la quinta o décima parte de su cuota si la
/// diferencia con el porcentaje del año de compra pasa de 10 puntos.
/// </summary>
public sealed class RegularizarBienesInversion
{
    private readonly IConsultaBienesInversion _bienes;
    private readonly CalcularProrrata _prorrata;

    public RegularizarBienesInversion(IConsultaBienesInversion bienes, CalcularProrrata prorrata)
    {
        _bienes = bienes;
        _prorrata = prorrata;
    }

    public async Task<RegularizacionBienesInversionDto> EjecutarAsync(Guid empresaId, int ejercicio, TipoImpuesto impuesto, CancellationToken ct = default)
    {
        var bienes = (await _bienes.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(b => b.EnPeriodo(ejercicio)).ToList();
        if (bienes.Count == 0)
        {
            return new RegularizacionBienesInversionDto(ejercicio, 100, [], 0m);
        }

        var definitivo = (await _prorrata.EjecutarAsync(empresaId, ejercicio, impuesto, ct).ConfigureAwait(false)).PorcentajeDefinitivo;
        var lineas = bienes.OrderBy(b => b.Codigo, StringComparer.Ordinal).Select(b => new LineaRegularizacionBienDto(b.Id, b.Codigo, b.Descripcion, b.FechaAdquisicion,
            ejercicio - b.InicioUtilizacion.Year + 1, b.Anios, b.Cuota, b.PorcentajeInicial, definitivo, definitivo - b.PorcentajeInicial,
            b.Regularizar(ejercicio, definitivo))).ToList();
        return new RegularizacionBienesInversionDto(ejercicio, definitivo, lineas, Redondeo.Dos(lineas.Sum(l => l.Regularizacion)));
    }
}
