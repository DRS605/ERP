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
    decimal Resultado);

/// <summary>Caso de uso: calcula el borrador del modelo 420 de un trimestre.</summary>
public sealed class GenerarModelo420
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly CalcularProrrata _prorrata;

    public GenerarModelo420(IConsultaFacturas facturas, IConsultaGastos gastos, CalcularProrrata prorrata)
    {
        _facturas = facturas;
        _gastos = gastos;
        _prorrata = prorrata;
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
        return new Modelo420Dto(
            anio, trimestre, desde, hasta, devengado, Redondeo.Dos(devengado.Sum(d => d.Base)), devCuota,
            soportado.Base, soportado.Total, deduccion.Porcentaje, deduccion.Deducible, deduccion.Regularizacion,
            Redondeo.Dos(devCuota - deduccion.Deducible - deduccion.Regularizacion));
    }
}
