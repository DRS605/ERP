using System.Security.Claims;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Volcados de palots en la planta. El terminal de la línea (app /planta, que funciona sin conexión) entra con un acceso
/// de tipo terminal del portal: descarga los datos del día para trabajar sin red y manda su cola de lecturas cuando la
/// tiene. En el ERP se consultan, se anulan y se pasan a un parte de confección.
/// </summary>
public static class EndpointsVolcados
{
    public sealed record PeticionVolcados(IReadOnlyList<DatosVolcado>? Volcados);

    public sealed record PeticionAnular(string? Motivo);

    public sealed record LineaTerminalDto(Guid Id, string Codigo, string Nombre);

    public sealed record OrdenTerminalDto(Guid Id, Guid LineaId, DateOnly Fecha, int Turno, int Secuencia, string Producto, decimal Kilos, string Estado);

    public sealed record PaleTerminalDto(string Sscc, string Producto, string? Agricultor, string? Partida, decimal Kilos);

    public sealed record DatosTerminalDto(string Terminal, DateOnly Hoy, DateTimeOffset GeneradoEn, IReadOnlyList<LineaTerminalDto> Lineas, IReadOnlyList<OrdenTerminalDto> Ordenes,
        IReadOnlyList<PaleTerminalDto> Pales);

    public static IEndpointRouteBuilder MapearVolcados(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        // Terminal de la planta (sesión del portal de tipo terminal).
        var t = rutas.MapGroup("/portal/planta").WithTags("Planta · terminal");
        t.MapGet("/datos", (ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, PlantaAgro planta, PalesAgro pales, RecepcionesAgro recepciones,
                MaestrosAgro maestros, IConsultaProductos productos, IReloj reloj, CancellationToken ct) =>
                EndpointsPortal.ConSesion(u, accesos, TipoPortal.TerminalPlanta, ct: ct, accion: async (_, acceso) =>
                {
                    var e = c.EmpresaId!.Value;
                    var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                    var nombres = new Dictionary<Guid, string>();
                    async Task<string> Producto(Guid id)
                    {
                        if (!nombres.TryGetValue(id, out var n))
                        {
                            nombres[id] = n = (await productos.ObtenerAsync(id, ct).ConfigureAwait(false))?.Nombre ?? "?";
                        }

                        return n;
                    }

                    var lineas = (await planta.LineasAsync(e, ct).ConfigureAwait(false)).Where(l => l.Activa).OrderBy(l => l.Codigo, StringComparer.Ordinal)
                        .Select(l => new LineaTerminalDto(l.Id, l.Codigo, l.Nombre)).ToList();
                    var ordenes = new List<OrdenTerminalDto>();
                    foreach (var o in (await planta.OrdenesAsync(e, hoy.AddDays(-1), hoy.AddDays(1), null, ct).ConfigureAwait(false))
                                 .Where(o => o.Estado is "Planificada" or "EnCurso").OrderBy(o => o.Fecha).ThenBy(o => o.Turno).ThenBy(o => o.Secuencia))
                    {
                        ordenes.Add(new OrdenTerminalDto(o.Id, o.LineaId, o.Fecha, o.Turno, o.Secuencia, await Producto(o.ProductoId).ConfigureAwait(false), o.Kilos, o.Estado));
                    }

                    var partidas = (await recepciones.ExistenciasAsync(e, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
                    var agricultores = (await maestros.AgricultoresAsync(e, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
                    var volcables = new List<PaleTerminalDto>();
                    foreach (var p in (await pales.ListarAsync(e, EstadoPale.Cerrado, ct).ConfigureAwait(false))
                                 .Concat(await pales.ListarAsync(e, EstadoPale.Abierto, ct).ConfigureAwait(false)))
                    {
                        var principal = p.Contenido.Where(x => x.Kilos > 0m).OrderByDescending(x => x.Kilos).FirstOrDefault();
                        if (principal is null)
                        {
                            continue;
                        }

                        var partida = partidas.GetValueOrDefault(principal.PartidaId);
                        volcables.Add(new PaleTerminalDto(p.Sscc, await Producto(principal.ProductoId).ConfigureAwait(false),
                            partida?.AgricultorId is { } a ? agricultores.GetValueOrDefault(a) : null, principal.Partida, p.Contenido.Sum(x => x.Kilos)));
                    }

                    return Results.Ok(new DatosTerminalDto(acceso.Nombre, hoy, reloj.AhoraUtc, lineas, ordenes, volcables));
                }))
            .WithSummary("Datos para trabajar sin conexión: líneas, órdenes de ayer a mañana y palés que se pueden volcar.").RequireAuthorization();
        t.MapPost("/volcados", (PeticionVolcados p, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, VolcadosPlanta volcados, CancellationToken ct) =>
                EndpointsPortal.ConSesion(u, accesos, TipoPortal.TerminalPlanta, ct: ct, accion: async (_, acceso) =>
                    (await volcados.RegistrarAsync(c.EmpresaId!.Value, p.Volcados, acceso.Nombre, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Manda la cola de lecturas del terminal: cada una vuelve registrada, ya registrada (reenvío) o con su error.").RequireAuthorization();
        t.MapGet("/volcados", (DateOnly? fecha, Guid? lineaId, ClaimsPrincipal u, IContextoEmpresa c, AccesosPortal accesos, VolcadosPlanta volcados, IReloj reloj,
                CancellationToken ct) =>
                EndpointsPortal.ConSesion(u, accesos, TipoPortal.TerminalPlanta, ct: ct, accion: async (_, _) =>
                {
                    var dia = fecha ?? DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                    return Results.Ok(await volcados.ListarAsync(c.EmpresaId!.Value, dia, dia, lineaId, ct).ConfigureAwait(false));
                }))
            .WithSummary("Volcados de un día (de todos los terminales).").RequireAuthorization();

        // ERP.
        var g = rutas.MapGroup("/agro/planta/volcados").WithTags("Agro");
        g.MapGet("", async (DateOnly? desde, DateOnly? hasta, Guid? lineaId, IContextoEmpresa c, VolcadosPlanta volcados, IReloj reloj, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                return Results.Ok(await volcados.ListarAsync(e, desde ?? hoy, hasta ?? desde ?? hoy, lineaId, ct).ConfigureAwait(false));
            })
            .WithSummary("Volcados de palots por día y línea.").RequierePermiso(Permisos.AgroLeer);
        g.MapPost("", async (PeticionVolcados p, HttpContext http, IContextoEmpresa c, VolcadosPlanta volcados, CancellationToken ct) =>
            {
                if (c.EmpresaId is not { } e)
                {
                    return SinEmpresa();
                }

                var yo = http.User.ObtenerIdentidad();
                return (await volcados.RegistrarAsync(e, p.Volcados, string.IsNullOrWhiteSpace(yo?.Nombre) ? yo?.Email ?? "ERP" : yo.Nombre, ct).ConfigureAwait(false)).AOk();
            })
            .WithSummary("Registra volcados desde el ERP (lector en el puesto).").RequiereAlgunPermiso(Permisos.AgroGestionar, Permisos.AgroConfeccionar);
        g.MapPost("/{id:guid}/anular", async (Guid id, PeticionAnular? p, VolcadosPlanta volcados, CancellationToken ct) =>
                (await volcados.AnularAsync(id, p?.Motivo, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Anula un volcado pendiente (lectura errónea).").RequiereAlgunPermiso(Permisos.AgroGestionar, Permisos.AgroConfeccionar);
        g.MapPost("/parte", async (DatosParteVolcados d, IContextoEmpresa c, VolcadosPlanta volcados, CancellationToken ct) =>
                c.EmpresaId is { } e
                    ? (await volcados.GenerarParteAsync(e, d, ct).ConfigureAwait(false)) is var r && r.EsCorrecto
                        ? Results.Created($"/agro/partes/{r.Valor.Id}", r.Valor)
                        : ResultadosHttp.AProblema(r.Error)
                    : SinEmpresa())
            .WithSummary("Parte de confección en borrador con el consumo de los palés volcados (pendientes) de una línea y día.")
            .RequiereAlgunPermiso(Permisos.AgroGestionar, Permisos.AgroConfeccionar);
        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
