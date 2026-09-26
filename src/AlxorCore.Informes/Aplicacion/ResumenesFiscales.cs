using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Resumen del <b>modelo 303</b> (autoliquidación trimestral de IVA): IVA devengado
/// (repercutido en las facturas emitidas del trimestre) menos IVA deducible (soportado en los
/// gastos del trimestre).
/// </summary>
/// <remarks>
/// Con prorrata, <see cref="IvaDeducibleCuota"/> es la parte deducible del IVA soportado
/// (<see cref="IvaSoportadoCuota"/>) con el porcentaje provisional, y en el cuarto trimestre se añade la
/// <see cref="RegularizacionProrrata"/> del año con el definitivo. Solo cuenta el IVA: las facturas y
/// gastos con IGIC van al modelo 420.
/// </remarks>
public sealed record Modelo303Dto(
    int Anio, int Trimestre, DateOnly Desde, DateOnly Hasta,
    decimal IvaDevengadoBase, decimal IvaDevengadoCuota,
    decimal IvaDeducibleBase, decimal IvaDeducibleCuota,
    decimal Resultado,
    decimal IvaSoportadoCuota = 0m,
    int PorcentajeProrrata = 100,
    decimal RegularizacionProrrata = 0m,
    decimal CompensacionesReagpBase = 0m,
    decimal CompensacionesReagpCuota = 0m);

/// <summary>
/// Resumen del <b>modelo 130</b> (pago fraccionado del IRPF en estimación directa). Es
/// <b>acumulativo</b> desde el 1 de enero: sobre el rendimiento neto acumulado (ingresos − gastos)
/// se aplica el 20 %, del que se descuentan las retenciones soportadas y los pagos fraccionados de
/// los trimestres anteriores del mismo ejercicio.
/// </summary>
public sealed record Modelo130Dto(
    int Anio, int Trimestre, DateOnly Desde, DateOnly Hasta,
    decimal IngresosAcumulados, decimal GastosAcumulados, decimal RendimientoAcumulado,
    decimal PagoFraccionadoBruto, decimal RetencionesAcumuladas, decimal PagosAnteriores,
    decimal Resultado);

/// <summary>Resumen fiscal de un trimestre: modelos 303 (IVA) y 130 (IRPF).</summary>
public sealed record ResumenTrimestralDto(Modelo303Dto Modelo303, Modelo130Dto Modelo130);

/// <summary>
/// Caso de uso: calcula los resúmenes fiscales de un trimestre (303 y 130) a partir de las facturas
/// emitidas y los gastos registrados. Es una <b>ayuda informativa</b> para preparar la
/// autoliquidación con la gestoría, no un envío oficial a la AEAT.
/// </summary>
public sealed class GenerarResumenesFiscales
{
    private const decimal PorcentajePagoFraccionado = 0.20m;

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly CalcularProrrata? _prorrata;

    public GenerarResumenesFiscales(IConsultaFacturas facturas, IConsultaGastos gastos, CalcularProrrata? prorrata = null)
    {
        _facturas = facturas;
        _gastos = gastos;
        _prorrata = prorrata;
    }

    public async Task<ResumenTrimestralDto> EjecutarAsync(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        if (trimestre is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(trimestre), trimestre, "El trimestre debe estar entre 1 y 4.");
        }

        var facturas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var todos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);

        // Solo cuentan las facturas realmente emitidas (se excluyen las anuladas y las ya
        // sustituidas por una rectificativa, que aportaría los importes corregidos) y los gastos
        // no anulados.
        var emitidas = facturas.Where(f => f.Estado == "Emitida").ToList();
        var gastos = todos.Where(g => !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase)).ToList();

        return new ResumenTrimestralDto(
            await Calcular303Async(empresaId, anio, trimestre, emitidas, gastos, ct).ConfigureAwait(false),
            Calcular130(anio, trimestre, emitidas, gastos));
    }

    private async Task<Modelo303Dto> Calcular303Async(
        Guid empresaId, int anio, int trimestre, IReadOnlyList<FacturaResumen> facturas, IReadOnlyList<GastoDto> gastos, CancellationToken ct)
    {
        var (desde, hasta) = Periodos.Trimestre(anio, trimestre);

        var devengado = facturas.Where(f => f.Impuesto == TipoImpuesto.Iva && f.FechaEmision >= desde && f.FechaEmision <= hasta).ToList();
        var soportado = CuotasSoportadas.De(gastos.Where(g => g.Fecha >= desde && g.Fecha <= hasta), TipoImpuesto.Iva);

        var devBase = Redondeo.Dos(devengado.Sum(f => f.BaseImponible));
        var devCuota = Redondeo.Dos(devengado.Sum(f => f.CuotaIva));
        var deduccion = _prorrata is null
            ? new DeduccionTrimestre(null, 100, soportado.Total, 0m)
            : await _prorrata.DeduccionAsync(empresaId, anio, trimestre, TipoImpuesto.Iva, soportado, ct).ConfigureAwait(false);

        // Compensaciones pagadas a agricultores en REAGP (casillas 42-43): son parte de lo soportado.
        var reagp = gastos.Where(g => g.Fecha >= desde && g.Fecha <= hasta && g.CodigoIva.StartsWith("REAGP", StringComparison.OrdinalIgnoreCase)).ToList();

        return new Modelo303Dto(anio, trimestre, desde, hasta, devBase, devCuota, soportado.Base, deduccion.Deducible,
            Redondeo.Dos(devCuota - deduccion.Deducible - deduccion.Regularizacion),
            soportado.Total, deduccion.Porcentaje, deduccion.Regularizacion,
            Redondeo.Dos(reagp.Sum(g => g.BaseImponible)), Redondeo.Dos(reagp.Sum(g => g.CuotaIva)));
    }

    private static Modelo130Dto Calcular130(int anio, int trimestre, IReadOnlyList<FacturaResumen> facturas, IReadOnlyList<GastoDto> gastos)
    {
        // El 130 es acumulativo: para calcular los "pagos anteriores" hay que recorrer trimestre a
        // trimestre desde el primero hasta el pedido, arrastrando lo ya ingresado.
        decimal pagosAnteriores = 0m;
        Modelo130Dto? actual = null;

        for (var t = 1; t <= trimestre; t++)
        {
            var (_, hasta) = Periodos.Trimestre(anio, t);
            var inicioAnio = new DateOnly(anio, 1, 1);

            var ingresos = Redondeo.Dos(facturas.Where(f => f.FechaEmision >= inicioAnio && f.FechaEmision <= hasta).Sum(f => f.BaseImponible));
            var gastosAcum = Redondeo.Dos(gastos.Where(g => g.Fecha >= inicioAnio && g.Fecha <= hasta).Sum(g => g.BaseImponible));
            var retenciones = Redondeo.Dos(facturas.Where(f => f.FechaEmision >= inicioAnio && f.FechaEmision <= hasta).Sum(f => f.RetencionIrpf));

            var rendimiento = Redondeo.Dos(ingresos - gastosAcum);
            var bruto = Redondeo.Dos(Math.Max(0m, rendimiento * PorcentajePagoFraccionado));
            var resultado = Redondeo.Dos(Math.Max(0m, bruto - retenciones - pagosAnteriores));

            var (desdeT, hastaT) = Periodos.Trimestre(anio, t);
            actual = new Modelo130Dto(anio, t, desdeT, hastaT, ingresos, gastosAcum, rendimiento, bruto, retenciones, Redondeo.Dos(pagosAnteriores), resultado);
            pagosAnteriores += resultado;
        }

        return actual!;
    }
}
