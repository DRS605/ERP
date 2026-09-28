using AlxorCore.Api.Comun;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Devoluciones de venta y reclamaciones sobre ventas (con su maestro de conceptos e informe).</summary>
public static class EndpointsDevoluciones
{
    public sealed record PeticionAbono(DateOnly? Fecha);

    public sealed record PeticionMotivo(string? Motivo);

    public sealed record PeticionTramitar(string? Responsable);

    public sealed record PeticionActivo(bool Activo);

    public static IEndpointRouteBuilder MapearDevoluciones(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        // ------------------------------------------------------------------ devoluciones
        var d = rutas.MapGroup("/devoluciones-venta").WithTags("Ventas");
        d.MapGet("", (Guid? clienteId, Guid? albaranId, EstadoDevolucionVenta? estado, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, GestionDevolucionesVenta g,
                CancellationToken ct) => ConEmpresa(c, async e => Results.Ok(await g.ListarAsync(e, new FiltroDevoluciones(clienteId, albaranId, estado, desde, hasta), ct).ConfigureAwait(false))))
            .WithSummary("Devoluciones de venta (filtro por cliente, albarán, estado y fechas).").RequierePermiso(Permisos.FacturaLeer);
        d.MapGet("/{id:guid}", async (Guid id, GestionDevolucionesVenta g, CancellationToken ct) =>
                await g.ObtenerAsync(id, ct).ConfigureAwait(false) is { } x ? Results.Ok(x) : Results.NotFound())
            .WithSummary("Devolución de venta con sus líneas.").RequierePermiso(Permisos.FacturaLeer);
        d.MapGet("/devolubles/{albaranId:guid}", async (Guid albaranId, GestionDevolucionesVenta g, CancellationToken ct) =>
                (await g.DevolublesAsync(albaranId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Lo entregado, lo devuelto y lo que aún se puede devolver de cada línea del albarán.").RequierePermiso(Permisos.FacturaLeer);
        d.MapPost("", (CrearDevolucionComando cmd, IContextoEmpresa c, GestionDevolucionesVenta g, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await g.CrearAsync(e, cmd, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/devoluciones-venta/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Registra una devolución sobre un albarán: la mercancía que vuelve en buen estado entra en el almacén.").RequierePermiso(Permisos.FacturaEmitir);
        d.MapPost("/{id:guid}/abonar", (Guid id, PeticionAbono? p, IContextoEmpresa c, GestionDevolucionesVenta g, CancellationToken ct) =>
                ConEmpresa(c, async e => (await g.AbonarAsync(e, id, p?.Fecha, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Abona la devolución de un albarán facturado con una rectificativa de su factura.").RequierePermiso(Permisos.FacturaEmitir);
        d.MapPost("/{id:guid}/cerrar-sin-abono", async (Guid id, GestionDevolucionesVenta g, CancellationToken ct) =>
                (await g.CerrarSinAbonoAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra la devolución sin abono (mercancía repuesta).").RequierePermiso(Permisos.FacturaEmitir);
        d.MapPost("/{id:guid}/anular", (Guid id, PeticionMotivo? p, IContextoEmpresa c, GestionDevolucionesVenta g, CancellationToken ct) =>
                ConEmpresa(c, async e => (await g.AnularAsync(e, id, p?.Motivo, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Anula una devolución no abonada: lo que reingresó vuelve a salir del almacén.").RequierePermiso(Permisos.FacturaEmitir);

        // ------------------------------------------------------------------ reclamaciones
        var r = rutas.MapGroup("/reclamaciones").WithTags("Ventas");
        r.MapGet("/conceptos", async (GestionReclamaciones g, CancellationToken ct) => Results.Ok(await g.ConceptosAsync(ct).ConfigureAwait(false)))
            .WithSummary("Conceptos para reclamaciones.").RequierePermiso(Permisos.FacturaLeer);
        r.MapPost("/conceptos", (GuardarConceptoReclamacion p, IContextoEmpresa c, GestionReclamaciones g, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var x = await g.GuardarConceptoAsync(e, null, p, ct).ConfigureAwait(false);
                    return x.EsCorrecto ? Results.Created($"/reclamaciones/conceptos/{x.Valor.Id}", x.Valor) : ResultadosHttp.AProblema(x.Error);
                }))
            .WithSummary("Crea un concepto de reclamación.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPut("/conceptos/{id:guid}", (Guid id, GuardarConceptoReclamacion p, IContextoEmpresa c, GestionReclamaciones g, CancellationToken ct) =>
                ConEmpresa(c, async e => await g.ConceptoExisteAsync(id, ct).ConfigureAwait(false)
                    ? (await g.GuardarConceptoAsync(e, id, p, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.NoEncontrado("reclamacion.concepto_no_encontrado", "El concepto no existe."))))
            .WithSummary("Cambia un concepto de reclamación.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/conceptos/{id:guid}/baja", async (Guid id, GestionReclamaciones g, CancellationToken ct) =>
                (await g.ActivarConceptoAsync(id, false, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Da de baja un concepto (las reclamaciones que lo usan lo conservan).").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/conceptos/{id:guid}/reactivar", async (Guid id, GestionReclamaciones g, CancellationToken ct) =>
                (await g.ActivarConceptoAsync(id, true, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Reactiva un concepto dado de baja.").RequierePermiso(Permisos.FacturaEmitir);

        r.MapGet("", (Guid? clienteId, EstadoReclamacion? estado, Guid? conceptoId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, GestionReclamaciones g, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await g.ListarAsync(e, new FiltroReclamaciones(clienteId, estado, conceptoId, desde, hasta), ct).ConfigureAwait(false))))
            .WithSummary("Reclamaciones sobre ventas.").RequierePermiso(Permisos.FacturaLeer);
        r.MapGet("/informe", (Guid? clienteId, Guid? conceptoId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, GestionReclamaciones g, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await g.InformeAsync(e, new FiltroReclamaciones(clienteId, null, conceptoId, desde, hasta), ct).ConfigureAwait(false))))
            .WithSummary("Informe de reclamaciones por concepto y por cliente.").RequierePermiso(Permisos.FacturaLeer);
        r.MapGet("/{id:guid}", async (Guid id, GestionReclamaciones g, CancellationToken ct) =>
                await g.ObtenerAsync(id, ct).ConfigureAwait(false) is { } x ? Results.Ok(x) : Results.NotFound())
            .WithSummary("Reclamación.").RequierePermiso(Permisos.FacturaLeer);
        r.MapPost("", (GuardarReclamacion p, IContextoEmpresa c, GestionReclamaciones g, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var x = await g.CrearAsync(e, p, ct).ConfigureAwait(false);
                    return x.EsCorrecto ? Results.Created($"/reclamaciones/{x.Valor.Id}", x.Valor) : ResultadosHttp.AProblema(x.Error);
                }))
            .WithSummary("Registra una reclamación del cliente.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPut("/{id:guid}", async (Guid id, GuardarReclamacion p, GestionReclamaciones g, CancellationToken ct) =>
                (await g.CambiarAsync(id, p, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia una reclamación abierta o en trámite.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/{id:guid}/tramitar", async (Guid id, PeticionTramitar? p, GestionReclamaciones g, CancellationToken ct) =>
                (await g.TramitarAsync(id, p?.Responsable, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Pasa la reclamación a trámite con su responsable.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/{id:guid}/resolver", async (Guid id, ResolverReclamacion p, GestionReclamaciones g, CancellationToken ct) =>
                (await g.ResolverAsync(id, p, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Resuelve la reclamación: aceptada, parcial o rechazada, con lo reconocido y la devolución o rectificativa.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/{id:guid}/reabrir", async (Guid id, GestionReclamaciones g, CancellationToken ct) =>
                (await g.ReabrirAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reabre una reclamación resuelta.").RequierePermiso(Permisos.FacturaEmitir);
        r.MapPost("/{id:guid}/anular", async (Guid id, GestionReclamaciones g, CancellationToken ct) =>
                (await g.AnularAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una reclamación registrada por error.").RequierePermiso(Permisos.FacturaEmitir);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } e
            ? await accion(e).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
