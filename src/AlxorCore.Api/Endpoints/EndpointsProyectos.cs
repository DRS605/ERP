using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Proyectos.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Proyectos (imputación de costes y presupuesto vs. real).</summary>
public static class EndpointsProyectos
{
    public static IEndpointRouteBuilder MapearProyectos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/proyectos").WithTags("Proyectos");
        g.MapGet("", ListarAsync).WithSummary("Lista los proyectos con su coste real y desviación.").RequierePermiso(Permisos.ProyectoLeer);
        g.MapGet("/{id:guid}", ObtenerAsync).WithSummary("Obtiene el detalle de un proyecto (imputaciones y subtotales).").RequierePermiso(Permisos.ProyectoLeer);
        g.MapPost("", CrearAsync).WithSummary("Crea un proyecto.").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPut("/{id:guid}", ActualizarAsync).WithSummary("Actualiza los datos del proyecto (nombre, cliente, presupuesto).").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/cerrar", CerrarAsync).WithSummary("Cierra el proyecto.").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/reabrir", ReabrirAsync).WithSummary("Reabre el proyecto.").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/cancelar", CancelarAsync).WithSummary("Cancela el proyecto.").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/mano-obra", ManoObraAsync).WithSummary("Imputa mano de obra (persona × horas × tarifa).").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/material", MaterialAsync).WithSummary("Imputa material (valorado según el método de la empresa).").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapPost("/{id:guid}/gasto", GastoAsync).WithSummary("Imputa un gasto directo.").RequierePermiso(Permisos.ProyectoGestionar);
        g.MapDelete("/{id:guid}/imputaciones/{imputacionId:guid}", EliminarImputacionAsync).WithSummary("Elimina una imputación.").RequierePermiso(Permisos.ProyectoGestionar);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static async Task<IResult> ListarAsync(IContextoEmpresa c, ListarProyectos caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.EjecutarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> ObtenerAsync(Guid id, ObtenerProyecto caso, CancellationToken ct)
    {
        var dto = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return dto is null ? ResultadosHttp.AProblema(Error.NoEncontrado("proyecto.no_encontrado", "No se encontró el proyecto.")) : Results.Ok(dto);
    }

    private static async Task<IResult> CrearAsync(DatosProyecto datos, IContextoEmpresa c, CrearProyecto caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(c.EmpresaId.Value, datos, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/proyectos/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, DatosProyecto datos, ActualizarProyecto caso, CancellationToken ct)
        => (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CerrarAsync(Guid id, CambiarEstadoProyecto caso, CancellationToken ct)
        => (await caso.CerrarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ReabrirAsync(Guid id, CambiarEstadoProyecto caso, CancellationToken ct)
        => (await caso.ReabrirAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CancelarAsync(Guid id, CambiarEstadoProyecto caso, CancellationToken ct)
        => (await caso.CancelarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ManoObraAsync(Guid id, ImputarManoObraComando comando, IContextoEmpresa c, ImputarCostes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.ManoObraAsync(c.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> MaterialAsync(Guid id, ImputarMaterialComando comando, IContextoEmpresa c, ImputarCostes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.MaterialAsync(c.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> GastoAsync(Guid id, ImputarGastoComando comando, IContextoEmpresa c, ImputarCostes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.GastoAsync(c.EmpresaId.Value, id, comando, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> EliminarImputacionAsync(Guid id, Guid imputacionId, ImputarCostes caso, CancellationToken ct)
        => (await caso.EliminarAsync(id, imputacionId, ct).ConfigureAwait(false)).AOk();
}
