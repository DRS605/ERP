using AlxorCore.Api.Comun;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Impuestos indirectos (base, en todas las ediciones): prorrata del IVA/IGIC soportado y borrador del
/// modelo 420 (IGIC, Canarias). El 303 sigue en <c>/informes/resumen-trimestral</c>.
/// </summary>
public static class EndpointsImpuestosIndirectos
{
    public static IEndpointRouteBuilder MapearImpuestosIndirectos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/impuestos").WithTags("Impuestos");

        g.MapGet("/prorrata", ProrrataAsync)
            .WithSummary("Prorrata del ejercicio: porcentaje provisional y definitivo (con las ventas del año), deducción y regularización, y aviso si la especial es obligatoria.")
            .RequierePermiso(Permisos.InformeLeer);
        g.MapPut("/prorrata/{ejercicio:int}", ConfigurarProrrataAsync)
            .WithSummary("Fija la prorrata del ejercicio (Regimen: General o Especial; PorcentajeProvisional 0-100). Regimen nulo la quita.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapPost("/prorrata/{ejercicio:int}/regularizar", RegularizarProrrataAsync)
            .WithSummary("Asiento de la regularización anual de la prorrata a 31/12: 472 a 639 si se deduce más, 634 a 472 si menos. Una sola vez por ejercicio.")
            .RequierePermiso(Permisos.EmpresaAjustes);
        g.MapGet("/modelo-425", Modelo425Async)
            .WithSummary("Borrador del modelo 425 (resumen anual del IGIC): los cuatro 420 del año sumados, por tipo, con la prorrata.")
            .RequierePermiso(Permisos.InformeLeer);
        g.MapGet("/modelo-420", Modelo420Async)
            .WithSummary("Borrador del modelo 420 (IGIC, Canarias): devengado por tipo, deducible con prorrata y resultado del trimestre.")
            .RequierePermiso(Permisos.InformeLeer);

        return rutas;
    }

    private static Error SinEmpresa() => Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.");

    private static async Task<IResult> ProrrataAsync(
        IContextoEmpresa contexto, IConsultaEmpresas empresas, CalcularProrrata caso, CancellationToken ct, int? ejercicio = null)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return ResultadosHttp.AProblema(SinEmpresa());
        }

        var impuesto = (await empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        var anio = ejercicio ?? DateTime.UtcNow.Year;
        return Results.Ok(await caso.EjecutarAsync(empresaId, anio, impuesto, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ConfigurarProrrataAsync(
        int ejercicio, ConfigurarProrrataComando comando, IContextoEmpresa contexto, ConfigurarProrrata caso, CancellationToken ct) =>
        contexto.EmpresaId is not { } empresaId
            ? ResultadosHttp.AProblema(SinEmpresa())
            : (await caso.EjecutarAsync(empresaId, ejercicio, comando, ct).ConfigureAwait(false)).AOk();

    /// <summary>Concepto del asiento de la regularización de la prorrata de un ejercicio (también sirve para no repetirlo).</summary>
    public static string ConceptoRegularizacion(int ejercicio, TipoImpuesto impuesto) => $"Regularización de la prorrata {ejercicio} ({impuesto.Siglas()})";

    private static async Task<IResult> RegularizarProrrataAsync(int ejercicio, IContextoEmpresa contexto, IConsultaEmpresas empresas, CalcularProrrata prorrata,
        AlxorCore.Contabilidad.Aplicacion.CrearAsiento crear, AlxorCore.Contabilidad.Aplicacion.IRepositorioAsientos asientos, CancellationToken ct)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return ResultadosHttp.AProblema(SinEmpresa());
        }

        var impuesto = (await empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        var calculo = await prorrata.EjecutarAsync(empresaId, ejercicio, impuesto, ct).ConfigureAwait(false);
        if (calculo.Regimen is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("prorrata.sin_configurar", $"El ejercicio {ejercicio} no tiene prorrata configurada."));
        }

        if (calculo.Regularizacion == 0m)
        {
            return ResultadosHttp.AProblema(Error.Validacion("prorrata.sin_regularizacion",
                $"El porcentaje definitivo ({calculo.PorcentajeDefinitivo} %) no cambia lo deducido: no hay nada que regularizar."));
        }

        var concepto = ConceptoRegularizacion(ejercicio, impuesto);
        if ((await asientos.DiarioAsync(empresaId, ejercicio, ct).ConfigureAwait(false)).Any(a => a.Concepto == concepto && a.AnuladoPorId is null && a.AnulaAsientoId is null))
        {
            return ResultadosHttp.AProblema(Error.Conflicto("prorrata.regularizada", $"La prorrata de {ejercicio} ya está regularizada en contabilidad (anula ese asiento para rehacerlo)."));
        }

        // Más deducible con el definitivo: la Hacienda nos debe (472) y es un ingreso (639); menos: gasto (634) contra la 472.
        var importe = Math.Abs(calculo.Regularizacion);
        IReadOnlyList<AlxorCore.Contabilidad.Aplicacion.LineaAsientoComando> lineas = calculo.Regularizacion > 0m
            ? [new("472", importe, 0m, concepto), new("639", 0m, importe, "Ajustes positivos en la imposición indirecta")]
            : [new("634", importe, 0m, "Ajustes negativos en la imposición indirecta"), new("472", 0m, importe, concepto)];
        var r = await crear.EjecutarAsync(empresaId, new AlxorCore.Contabilidad.Aplicacion.CrearAsientoComando(new DateOnly(ejercicio, 12, 31), concepto, lineas), ct)
            .ConfigureAwait(false);
        return r.EsFallo ? ResultadosHttp.AProblema(r.Error) : Results.Ok(r.Valor);
    }

    private static async Task<IResult> Modelo425Async(IContextoEmpresa contexto, GenerarModelo425 caso, CancellationToken ct, int? anio = null) =>
        contexto.EmpresaId is not { } empresaId
            ? ResultadosHttp.AProblema(SinEmpresa())
            : Results.Ok(await caso.EjecutarAsync(empresaId, anio ?? DateTime.UtcNow.Year, ct).ConfigureAwait(false));

    private static async Task<IResult> Modelo420Async(
        IContextoEmpresa contexto, GenerarModelo420 caso, CancellationToken ct, int? anio = null, int trimestre = 1)
    {
        if (contexto.EmpresaId is not { } empresaId)
        {
            return ResultadosHttp.AProblema(SinEmpresa());
        }

        if (trimestre is < 1 or > 4)
        {
            return ResultadosHttp.AProblema(Error.Validacion("modelo420.trimestre", "El trimestre debe estar entre 1 y 4."));
        }

        return Results.Ok(await caso.EjecutarAsync(empresaId, anio ?? DateTime.UtcNow.Year, trimestre, ct).ConfigureAwait(false));
    }
}
