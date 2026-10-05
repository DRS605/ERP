using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Vivero.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Viveros (módulo <c>vivero</c>): ajustes del pasaporte fitosanitario, lotes de planta con fases, bajas y paso a
/// existencias, encargos con reserva y entrega con albarán, pasaporte y libro del vivero.
/// </summary>
public static class EndpointsVivero
{
    public sealed record PeticionMotivoVivero(string? Motivo);

    public sealed record PeticionReserva(Guid LoteId);

    public sealed record PeticionServir(DateOnly? Fecha = null);

    public static IEndpointRouteBuilder MapearVivero(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/vivero").WithTags("Vivero");

        g.MapGet("/configuracion", (IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.ConfiguracionAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Código de registro del operador (ROPVEG) y país de origen para el pasaporte fitosanitario.").RequierePermiso(Permisos.ViveroLeer);
        g.MapPut("/configuracion", (DatosConfiguracionVivero d, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.FijarConfiguracionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija el código de registro del operador y el país de origen.").RequierePermiso(Permisos.ViveroGestionar);

        // Lotes de planta.
        g.MapGet("/lotes", (bool? incluirTerminados, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.LotesAsync(e, incluirTerminados ?? false, ct).ConfigureAwait(false))))
            .WithSummary("Lotes de planta en curso (o todos) con plantas vivas, reservadas y disponibles.").RequierePermiso(Permisos.ViveroLeer);
        g.MapGet("/lotes/{id:guid}", async (Guid id, GestionVivero v, CancellationToken ct) =>
                await v.LoteAsync(id, ct).ConfigureAwait(false) is { } l ? Results.Ok(l)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("lote_planta.no_encontrado", "El lote no existe.")))
            .WithSummary("Un lote con su libro de movimientos.").RequierePermiso(Permisos.ViveroLeer);
        g.MapPost("/lotes", (DatosLotePlanta d, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await v.CrearLoteAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/vivero/lotes/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Siembra un lote: especie, variedad, portainjerto, artículo, plantas, fase y ubicación.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPut("/lotes/{id:guid}", async (Guid id, DatosCambioLote d, GestionVivero v, CancellationToken ct) => (await v.CambiarLoteAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia variedad, portainjerto, origen del material, fecha prevista u observaciones.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/lotes/{id:guid}/avanzar", async (Guid id, DatosAvanceLote d, GestionVivero v, CancellationToken ct) => (await v.AvanzarAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Avanza de fase (nunca atrás) o traslada el lote a otra ubicación.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/lotes/{id:guid}/bajas", async (Guid id, DatosSalidaLote d, GestionVivero v, CancellationToken ct) => (await v.BajaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Baja de plantas (mortandad, descarte…) con el motivo.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/lotes/{id:guid}/existencias", (Guid id, DatosSalidaLote d, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.PasarAExistenciasAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Pasa plantas listas (sin reservar) a las existencias del artículo.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/lotes/{id:guid}/movimientos/{movimientoId:guid}/anular", (Guid id, Guid movimientoId, DatosAnulacionMovimiento d, IContextoEmpresa c, GestionVivero v,
                CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.AnularMovimientoAsync(e, id, movimientoId, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Anula una baja o un paso a existencias: las plantas vuelven al lote.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/lotes/{id:guid}/anular", async (Guid id, GestionVivero v, CancellationToken ct) => (await v.AnularLoteAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un lote que no ha tenido bajas, entregas ni ventas.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapGet("/lotes/{id:guid}/pasaporte", (Guid id, Guid? encargoId, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.PasaporteAsync(e, id, encargoId, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Datos del pasaporte fitosanitario UE del lote (A especie, B registro, C trazabilidad, D origen).").RequierePermiso(Permisos.ViveroLeer);
        g.MapGet("/libro", (DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.LibroAsync(e, desde, hasta, ct).ConfigureAwait(false))))
            .WithSummary("Libro del vivero: siembras, cambios, bajas, entregas y pasos a existencias de todos los lotes.").RequierePermiso(Permisos.ViveroLeer);

        // Encargos.
        g.MapGet("/encargos", (IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await v.EncargosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Encargos de planta de los clientes.").RequierePermiso(Permisos.ViveroLeer);
        g.MapPost("/encargos", (DatosEncargo d, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e =>
                {
                    var r = await v.CrearEncargoAsync(e, d, ct).ConfigureAwait(false);
                    return r.EsCorrecto ? Results.Created($"/vivero/encargos/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error);
                }))
            .WithSummary("Encargo de un cliente (artículo, plantas, fecha de entrega y precio), con su lote si ya se sabe.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/encargos/{id:guid}/reservar", async (Guid id, PeticionReserva p, GestionVivero v, CancellationToken ct) =>
                (await v.ReservarAsync(id, p.LoteId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reserva al encargo las plantas de un lote del mismo artículo.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/encargos/{id:guid}/liberar", async (Guid id, GestionVivero v, CancellationToken ct) => (await v.LiberarAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Quita la reserva del encargo.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/encargos/{id:guid}/servir", (Guid id, PeticionServir? p, IContextoEmpresa c, GestionVivero v, CancellationToken ct) =>
                ConEmpresa(c, async e => (await v.ServirAsync(e, id, p?.Fecha, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Entrega el encargo: las plantas salen del lote (listo) con un albarán al cliente.").RequierePermiso(Permisos.ViveroGestionar);
        g.MapPost("/encargos/{id:guid}/anular", async (Guid id, PeticionMotivoVivero? p, GestionVivero v, CancellationToken ct) =>
                (await v.AnularEncargoAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula el encargo; servido, anula su albarán y las plantas vuelven al lote.").RequierePermiso(Permisos.ViveroGestionar);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
