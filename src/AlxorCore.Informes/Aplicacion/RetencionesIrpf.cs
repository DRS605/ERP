using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Un perceptor de rendimientos con retención de IRPF practicada por la empresa (normalmente un
/// proveedor autónomo/profesional), con el total de percepciones y de retenciones del periodo.
/// </summary>
public sealed record PerceptorRetencionDto(
    string Clave,
    Guid? ProveedorId,
    string Nombre,
    string? Nif,
    string? Provincia,
    decimal BasePercepciones,
    decimal Retenciones);

/// <summary>
/// Resumen del <b>modelo 111</b> (retenciones e ingresos a cuenta del IRPF, autoliquidación
/// trimestral). Recoge las retenciones que la empresa ha practicado a sus perceptores en el
/// trimestre. Con los datos del ERP se cubren las <b>retenciones a profesionales/actividades
/// económicas</b> (las soportadas en gastos con retención); las de rendimientos del trabajo
/// (nóminas) quedarían fuera al no gestionarse aquí.
/// </summary>
public sealed record Modelo111Dto(
    int Anio,
    int Trimestre,
    int NumeroPerceptores,
    decimal BasePercepciones,
    decimal Retenciones,
    decimal TotalAIngresar);

/// <summary>
/// Resumen del <b>modelo 190</b> (resumen anual de retenciones e ingresos a cuenta del IRPF): el
/// detalle, perceptor a perceptor, de las retenciones practicadas durante el ejercicio (la suma de
/// los cuatro modelos 111). Los perceptores sin NIF (gastos sin proveedor en la ficha) se listan en
/// <see cref="PerceptoresSinNif"/> y no pueden incluirse en el fichero oficial.
/// </summary>
public sealed record Modelo190Dto(
    int Anio,
    int NumeroPerceptores,
    decimal TotalPercepciones,
    decimal TotalRetenciones,
    IReadOnlyList<PerceptorRetencionDto> Perceptores,
    IReadOnlyList<PerceptorRetencionDto> PerceptoresSinNif);

/// <summary>
/// Caso de uso: calcula los modelos 111 (trimestral) y 190 (anual) de retenciones de IRPF a partir
/// de los gastos con retención y del maestro de proveedores. Es una <b>ayuda</b> para preparar la
/// declaración con la gestoría.
/// </summary>
public sealed class GenerarRetencionesIrpf
{
    /// <summary>
    /// Clave del modelo 190 para rendimientos de actividades económicas (actividades profesionales):
    /// es la que corresponde a las retenciones que un autónomo/pyme practica a otros profesionales.
    /// </summary>
    public const string ClaveActividadesProfesionales = "G";

    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;

    public GenerarRetencionesIrpf(IConsultaGastos gastos, IConsultaProveedores proveedores)
    {
        _gastos = gastos;
        _proveedores = proveedores;
    }

    public async Task<Modelo111Dto> Modelo111Async(Guid empresaId, int anio, int trimestre, CancellationToken ct = default)
    {
        var (desde, hasta) = RangoTrimestre(anio, trimestre);
        var perceptores = await PerceptoresAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var todos = perceptores.ConNif.Concat(perceptores.SinNif).ToList();

        var basePercepciones = Redondeo.Dos(todos.Sum(p => p.BasePercepciones));
        var retenciones = Redondeo.Dos(todos.Sum(p => p.Retenciones));
        return new Modelo111Dto(anio, trimestre, todos.Count, basePercepciones, retenciones, retenciones);
    }

    public async Task<Modelo190Dto> Modelo190Async(Guid empresaId, int anio, CancellationToken ct = default)
    {
        var desde = new DateOnly(anio, 1, 1);
        var hasta = new DateOnly(anio, 12, 31);
        var perceptores = await PerceptoresAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);

        var totalPercepciones = Redondeo.Dos(perceptores.ConNif.Concat(perceptores.SinNif).Sum(p => p.BasePercepciones));
        var totalRetenciones = Redondeo.Dos(perceptores.ConNif.Concat(perceptores.SinNif).Sum(p => p.Retenciones));
        var numero = perceptores.ConNif.Count + perceptores.SinNif.Count;
        return new Modelo190Dto(anio, numero, totalPercepciones, totalRetenciones, perceptores.ConNif, perceptores.SinNif);
    }

    /// <summary>Agrupa los gastos con retención del periodo por perceptor, resolviendo NIF y datos del maestro.</summary>
    private async Task<(List<PerceptorRetencionDto> ConNif, List<PerceptorRetencionDto> SinNif)> PerceptoresAsync(
        Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var proveedores = await _proveedores.ListarAsync(empresaId, true, ct).ConfigureAwait(false);
        var mapa = proveedores.ToDictionary(p => p.Id);

        var conRetencion = gastos
            .Where(g => g.Estado != "Anulado" && g.RetencionIrpf > 0m && g.Fecha >= desde && g.Fecha <= hasta)
            .ToList();

        var lineas = conRetencion
            .GroupBy(g => g.ProveedorId is { } id ? "id:" + id : "txt:" + (g.ProveedorTexto ?? "Sin proveedor"))
            .Select(grupo =>
            {
                var primero = grupo.First();
                string nombre;
                string? nif;
                string? provincia = null;
                if (primero.ProveedorId is { } id && mapa.TryGetValue(id, out var prov))
                {
                    nombre = prov.Nombre;
                    nif = string.IsNullOrWhiteSpace(prov.NifFiscal) ? null : prov.NifFiscal;
                    provincia = string.IsNullOrWhiteSpace(prov.Provincia) ? null : prov.Provincia;
                }
                else
                {
                    nombre = primero.ProveedorTexto ?? "Sin proveedor";
                    nif = null;
                }

                return new PerceptorRetencionDto(
                    ClaveActividadesProfesionales,
                    primero.ProveedorId,
                    nombre,
                    nif,
                    provincia,
                    Redondeo.Dos(grupo.Sum(x => x.BaseImponible)),
                    Redondeo.Dos(grupo.Sum(x => x.RetencionIrpf)));
            })
            .OrderByDescending(p => p.Retenciones)
            .ToList();

        var conNif = lineas.Where(l => l.Nif is { Length: > 0 }).ToList();
        var sinNif = lineas.Where(l => l.Nif is not { Length: > 0 }).ToList();
        return (conNif, sinNif);
    }

    private static (DateOnly Desde, DateOnly Hasta) RangoTrimestre(int anio, int trimestre)
    {
        var mesInicio = ((trimestre - 1) * 3) + 1;
        var desde = new DateOnly(anio, mesInicio, 1);
        var hasta = desde.AddMonths(3).AddDays(-1);
        return (desde, hasta);
    }
}
