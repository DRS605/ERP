using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Produccion.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Producción (órdenes de fabricación).</summary>
public static class EndpointsProduccion
{
    public static IEndpointRouteBuilder MapearProduccion(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/produccion/ordenes").WithTags("Producción");
        g.MapGet("", ListarAsync).WithSummary("Lista las órdenes de fabricación.").RequierePermiso(Permisos.ProduccionLeer);
        g.MapGet("/{id:guid}", ObtenerAsync).WithSummary("Obtiene una orden de fabricación.").RequierePermiso(Permisos.ProduccionLeer);
        g.MapPost("", CrearAsync).WithSummary("Crea una orden de fabricación (planifica la lista de materiales).").RequierePermiso(Permisos.ProduccionGestionar);
        g.MapPost("/{id:guid}/iniciar", IniciarAsync).WithSummary("Inicia la fabricación.").RequierePermiso(Permisos.ProduccionGestionar);
        g.MapPost("/{id:guid}/terminar", TerminarAsync).WithSummary("Termina la orden: consume componentes y produce el artículo.").RequierePermiso(Permisos.ProduccionGestionar);
        g.MapPost("/{id:guid}/cancelar", CancelarAsync).WithSummary("Cancela la orden.").RequierePermiso(Permisos.ProduccionGestionar);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static async Task<IResult> ListarAsync(IContextoEmpresa c, ListarOrdenes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.EjecutarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerOrden caso, CancellationToken ct)
    {
        var dto = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return dto is null ? ResultadosHttp.AProblema(Error.NoEncontrado("orden.no_encontrada", "No se encontró la orden.")) : Results.Ok(dto);
    }

    private static async Task<IResult> CrearAsync(CrearOrdenComando comando, IContextoEmpresa c, CrearOrden caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(c.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/produccion/ordenes/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> IniciarAsync(Guid id, IContextoEmpresa c, DecidirOrden caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.IniciarAsync(c.EmpresaId.Value, id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> TerminarAsync(Guid id, IContextoEmpresa c, DecidirOrden caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.TerminarAsync(c.EmpresaId.Value, id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CancelarAsync(Guid id, IContextoEmpresa c, DecidirOrden caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.CancelarAsync(c.EmpresaId.Value, id, ct).ConfigureAwait(false)).AOk();
}
