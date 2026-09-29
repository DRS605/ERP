using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>IGIC devengado de un tipo en el trimestre.</summary>
public sealed record DevengoIgicDto(string Codigo, decimal Porcentaje, decimal Base, decimal Cuota);

/// <summary>
/// Borrador del <b>modelo 420</b>: autoliquidación trimestral del IGIC ante la Agencia Tributaria
/// Canaria. IGIC devengado por tipo (facturas emitidas con IGIC) menos IGIC deducible (gastos con IGIC,
/// con la prorrata aplicada si la hay). Es una ayuda para preparar la declaración, no su envío.
/// </summary>
public sealed record Modelo420Dto(
    int Anio, int Trimestre, DateOnly Desde, DateOnly Hasta,
    IReadOnlyList<DevengoIgicDto> Devengado,
    decimal DevengadoBase, decimal DevengadoCuota,
    decimal SoportadoBase, decimal SoportadoCuota,
    int PorcentajeProrrata, decimal DeducibleCuota, decimal RegularizacionProrrata,
    decimal Resultado, decimal RegularizacionBienesInversion = 0m);

/// <summary>Caso de uso: calcula el borrador del modelo 420 de un trimestre.</summary>
public sealed class GenerarModelo420
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly CalcularProrrata _prorrata;
    private readonly RegularizarBienesInversion? _bienes;

    public GenerarModelo420(IConsultaFacturas facturas, IConsultaGastos gastos, CalcularProrrata prorrata, RegularizarBienesInversion? bienes = null)
    {
        _facturas = facturas;
        _gastos = gastos;
        _prorrata = prorrata;
        _bienes = bienes;
    }

    public async Task<Modelo420Dto> EjecutarAsync(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        var (desde, hasta) = Periodos.Trimestre(anio, trimestre);

        var devengado = (await _facturas.DesgloseImpuestoAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .Where(d => d.Impuesto == TipoImpuesto.Igic)
            .Select(d => new DevengoIgicDto(d.CodigoIva, d.Porcentaje, Redondeo.Dos(d.Base), Redondeo.Dos(d.Cuota)))
            .ToList();

        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var soportado = CuotasSoportadas.De(gastos.Where(g => g.Fecha >= desde && g.Fecha <= hasta), TipoImpuesto.Igic);
        var deduccion = await _prorrata.DeduccionAsync(empresaId, anio, trimestre, TipoImpuesto.Igic, soportado, ct).ConfigureAwait(false);

        var devCuota = Redondeo.Dos(devengado.Sum(d => d.Cuota));
        // La regularización de los bienes de inversión va en el último periodo del año.
        var bienes = trimestre == 4 && _bienes is not null ? (await _bienes.EjecutarAsync(empresaId, anio, TipoImpuesto.Igic, ct).ConfigureAwait(false)).Total : 0m;
        return new Modelo420Dto(
            anio, trimestre, desde, hasta, devengado, Redondeo.Dos(devengado.Sum(d => d.Base)), devCuota,
            soportado.Base, soportado.Total, deduccion.Porcentaje, deduccion.Deducible, deduccion.Regularizacion,
            Redondeo.Dos(devCuota - deduccion.Deducible - deduccion.Regularizacion - bienes), bienes);
    }
}

/// <summary>Trimestre del resumen anual del IGIC: su resultado en el 420.</summary>
public sealed record TrimestreModelo425Dto(int Trimestre, decimal DevengadoCuota, decimal DeducibleCuota, decimal Resultado);

/// <summary>
/// Borrador del <b>modelo 425</b>: declaración-resumen anual del IGIC. Suma los cuatro 420 del año: IGIC devengado por
/// tipo, soportado, deducible (con la prorrata y su regularización del cuarto trimestre) y el resultado de cada
/// trimestre, que tiene que coincidir con lo autoliquidado. Es una ayuda para prepararla, no su envío.
/// </summary>
public sealed record Modelo425Dto(
    int Anio,
    IReadOnlyList<DevengoIgicDto> Devengado,
    decimal DevengadoBase, decimal DevengadoCuota,
    decimal SoportadoBase, decimal SoportadoCuota,
    int PorcentajeProrrata, decimal DeducibleCuota, decimal RegularizacionProrrata,
    decimal Resultado,
    IReadOnlyList<TrimestreModelo425Dto> Trimestres,
    decimal RegularizacionBienesInversion = 0m);

/// <summary>Caso de uso: calcula el borrador del modelo 425 de un año con los cuatro 420.</summary>
public sealed class GenerarModelo425
{
    private readonly GenerarModelo420 _trimestral;

    public GenerarModelo425(GenerarModelo420 trimestral) => _trimestral = trimestral;

    public async Task<Modelo425Dto> EjecutarAsync(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var trimestres = new List<Modelo420Dto>();
        for (var t = 1; t <= 4; t++)
        {
            trimestres.Add(await _trimestral.EjecutarAsync(empresaId, anio, t, ct).ConfigureAwait(false));
        }

        var devengado = trimestres.SelectMany(t => t.Devengado).GroupBy(d => (d.Codigo, d.Porcentaje))
            .Select(g => new DevengoIgicDto(g.Key.Codigo, g.Key.Porcentaje, Redondeo.Dos(g.Sum(d => d.Base)), Redondeo.Dos(g.Sum(d => d.Cuota))))
            .OrderByDescending(d => d.Porcentaje).ToList();
        return new Modelo425Dto(anio, devengado, Redondeo.Dos(devengado.Sum(d => d.Base)), Redondeo.Dos(devengado.Sum(d => d.Cuota)),
            Redondeo.Dos(trimestres.Sum(t => t.SoportadoBase)), Redondeo.Dos(trimestres.Sum(t => t.SoportadoCuota)), trimestres[3].PorcentajeProrrata,
            Redondeo.Dos(trimestres.Sum(t => t.DeducibleCuota)), Redondeo.Dos(trimestres.Sum(t => t.RegularizacionProrrata)),
            Redondeo.Dos(trimestres.Sum(t => t.Resultado)),
            trimestres.Select(t => new TrimestreModelo425Dto(t.Trimestre, t.DevengadoCuota, t.DeducibleCuota + t.RegularizacionProrrata + t.RegularizacionBienesInversion, t.Resultado)).ToList(),
            trimestres[3].RegularizacionBienesInversion);
    }
}
