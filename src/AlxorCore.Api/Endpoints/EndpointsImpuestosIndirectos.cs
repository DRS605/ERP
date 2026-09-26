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
