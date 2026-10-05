using AlxorCore.Agro.Aplicacion;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Subasta hortofrutícola (alhóndiga, módulo <c>subasta</c>, sobre agro): sesiones, lotes de las partidas, pujas,
/// adjudicación, cierre con los albaranes de los compradores, anulación y ventas por comprador o agricultor.
/// </summary>
public static class EndpointsSubasta
{
    public sealed record PeticionMotivoSubasta(string? Motivo);

    public static IEndpointRouteBuilder MapearSubasta(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/subasta").WithTags("Subasta");

        g.MapGet("/sesiones", (DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.ListarAsync(e, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Sesiones de subasta (filtro por fechas).").RequierePermiso(Permisos.SubastaLeer);
        g.MapGet("/sesiones/{id:guid}", async (Guid id, SubastasAgro s, CancellationToken ct) =>
                await s.ObtenerAsync(id, ct).ConfigureAwait(false) is { } sesion ? Results.Ok(sesion)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("subasta.no_encontrada", "La sesión de subasta no existe.")))
            .WithSummary("Sesión con sus lotes, pujas y totales por comprador y por agricultor.").RequierePermiso(Permisos.SubastaLeer);
        g.MapPost("/sesiones", (DatosSesionSubasta d, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await s.CrearAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/subasta/sesiones/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Abre una sesión de subasta (a la baja o al alza).").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPut("/sesiones/{id:guid}", async (Guid id, DatosSesionSubasta d, SubastasAgro s, CancellationToken ct) => (await s.CambiarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia la fecha, el tipo (sin pujas) o las observaciones de una sesión abierta.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapGet("/partidas", (Guid? productoId, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.SubastablesAsync(e, productoId, ct).ConfigureAwait(false))))
            .WithSummary("Partidas con kilos sueltos que aún no están en un lote vivo.").RequierePermiso(Permisos.SubastaLeer);
        g.MapPost("/sesiones/{id:guid}/lotes", async (Guid id, DatosLoteSubasta d, SubastasAgro s, CancellationToken ct) =>
                (await s.AgregarLoteAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Añade un lote con kilos sueltos de una partida (sin kilos, todo lo disponible).").RequierePermiso(Permisos.SubastaGestionar);
        g.MapDelete("/sesiones/{id:guid}/lotes/{loteId:guid}", async (Guid id, Guid loteId, SubastasAgro s, CancellationToken ct) =>
                (await s.QuitarLoteAsync(id, loteId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita un lote sin pujas ni adjudicación.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/lotes/{loteId:guid}/pujas", async (Guid id, Guid loteId, DatosPujaSubasta d, SubastasAgro s, CancellationToken ct) =>
                (await s.PujarAsync(id, loteId, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Puja al alza de un comprador (por encima de la mejor y del precio de salida).").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/lotes/{loteId:guid}/adjudicar", async (Guid id, Guid loteId, DatosAdjudicacionSubasta? d, SubastasAgro s, CancellationToken ct) =>
                (await s.AdjudicarAsync(id, loteId, d ?? new DatosAdjudicacionSubasta(), ct).ConfigureAwait(false)).AOk())
            .WithSummary("Adjudica el lote: a la baja, al comprador y precio del reloj; al alza, sin datos, a la mejor puja.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/lotes/{loteId:guid}/desierto", async (Guid id, Guid loteId, SubastasAgro s, CancellationToken ct) =>
                (await s.DesiertoAsync(id, loteId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Deja el lote desierto (sin comprador): la fruta sigue en la partida.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/lotes/{loteId:guid}/deshacer", async (Guid id, Guid loteId, SubastasAgro s, CancellationToken ct) =>
                (await s.DeshacerAsync(id, loteId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Vuelve a dejar pendiente un lote adjudicado o desierto de una sesión abierta.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/cerrar", (Guid id, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.CerrarAsync(e, id, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cierra la sesión: un albarán de venta por comprador y la salida de los kilos de sus partidas.").RequierePermiso(Permisos.SubastaGestionar);
        g.MapPost("/sesiones/{id:guid}/anular", (Guid id, PeticionMotivoSubasta? p, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e => (await s.AnularAsync(e, id, p?.Motivo, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Anula la sesión; cerrada, anula sus albaranes y devuelve los kilos (no si ya está liquidada al agricultor).")
            .RequierePermiso(Permisos.SubastaGestionar);
        g.MapGet("/ventas", (DateOnly? desde, DateOnly? hasta, Guid? compradorId, Guid? agricultorId, IContextoEmpresa c, SubastasAgro s, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await s.VentasAsync(e, desde, hasta, compradorId, agricultorId, ct).ConfigureAwait(false))))
            .WithSummary("Lotes vendidos en sesiones cerradas, por comprador o por agricultor.").RequierePermiso(Permisos.SubastaLeer);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
