using AlxorCore.Api.Comun;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Presupuestos contables (módulo «analitica»): por cuenta, centro y partida, con seguimiento frente al real.</summary>
public static class EndpointsPresupuestosContables
{
    public sealed record PeticionLineas(IReadOnlyList<DatosLineaPresupuesto> Lineas);

    public static IEndpointRouteBuilder MapearPresupuestosContables(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/contabilidad/presupuestos").WithTags("Presupuestos contables");

        g.MapGet("", async (IContextoEmpresa contexto, GestionPresupuestosContables caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)))
            .WithSummary("Presupuestos contables de la empresa.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapGet("/{id:guid}", async (Guid id, GestionPresupuestosContables caso, CancellationToken ct) =>
                (await caso.ObtenerAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Un presupuesto con sus líneas mes a mes.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("", async (CrearPresupuestoComando comando, IContextoEmpresa contexto, GestionPresupuestosContables caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Creado(await caso.CrearAsync(e, comando, ct).ConfigureAwait(false)))
            .WithSummary("Crea un presupuesto (de 1 a 24 meses desde un mes). Cada línea: importes mensuales o un total a repartir.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPost("/desde-real", async (PresupuestoDesdeRealComando comando, IContextoEmpresa contexto, GestionPresupuestosContables caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Creado(await caso.DesdeRealAsync(e, comando, ct).ConfigureAwait(false)))
            .WithSummary("Genera un presupuesto con el real de otro periodo mes a mes más un incremento (por cuenta o por centro y partida).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/{id:guid}/lineas", async (Guid id, PeticionLineas peticion, GestionPresupuestosContables caso, CancellationToken ct) =>
                (await caso.FijarLineasAsync(id, peticion.Lineas ?? [], ct).ConfigureAwait(false)).AOk())
            .WithSummary("Sustituye las líneas (solo en borrador).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPost("/{id:guid}/aprobar", async (Guid id, GestionPresupuestosContables caso, CancellationToken ct) =>
                (await caso.AprobarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Aprueba el presupuesto: queda congelado como referencia del seguimiento.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPost("/{id:guid}/copiar", async (Guid id, CopiarPresupuestoComando comando, IContextoEmpresa contexto, GestionPresupuestosContables caso, CancellationToken ct) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : Creado(await caso.CopiarAsync(e, id, comando, ct).ConfigureAwait(false)))
            .WithSummary("Copia un presupuesto a una versión nueva en borrador, con un incremento opcional.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapGet("/{id:guid}/seguimiento", async (Guid id, IContextoEmpresa contexto, SeguimientoPresupuesto caso, CancellationToken ct, DateOnly? hasta = null) =>
                contexto.EmpresaId is not { } e ? SinEmpresa() : (await caso.EjecutarAsync(e, id, hasta, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Presupuesto frente a real hasta un mes: por mes y acumulado, desviación y si es favorable.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        return rutas;
    }

    private static IResult SinEmpresa() =>
        ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado(Resultado<PresupuestoContableDto> r) =>
        r.EsCorrecto ? Results.Created($"/contabilidad/presupuestos/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
}
