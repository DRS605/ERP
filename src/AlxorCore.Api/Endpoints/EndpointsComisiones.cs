using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Endpoints;

/// <summary>Comisiones de agentes y vendedores: agentes, su cartera de clientes, reglas, cálculo y liquidaciones.</summary>
public static class EndpointsComisiones
{
    public static IEndpointRouteBuilder MapearComisiones(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/comisiones").WithTags("Comisiones");
        g.MapGet("/agentes", (IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) => Con(c, async e => Results.Ok(await s.AgentesAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Agentes comerciales y vendedores con su comisión y número de clientes.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/agentes", (DatosAgente d, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => (await s.GuardarAgenteAsync(e, null, d, ct).ConfigureAwait(false)) is var r && r.EsCorrecto
                    ? Results.Created($"/comisiones/agentes/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error)))
            .WithSummary("Da de alta un agente (externo, enlazado a su proveedor, o vendedor de la casa).").RequierePermiso(Permisos.FacturaEmitir);
        g.MapPut("/agentes/{id:guid}", (Guid id, DatosAgente d, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => (await s.GuardarAgenteAsync(e, id, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Cambia un agente (o lo da de baja con activo = false).").RequierePermiso(Permisos.FacturaEmitir);
        g.MapGet("/asignaciones", (IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) => Con(c, async e => Results.Ok(await s.AsignacionesAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Agente de cada cliente con su fecha de inicio (el historial).").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/asignaciones", (DatosAsignacionAgente d, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => (await s.AsignarAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Asigna el agente de un cliente desde una fecha (sin agente: se le quita).").RequierePermiso(Permisos.FacturaEmitir);
        g.MapGet("/reglas", (IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) => Con(c, async e => Results.Ok(await s.ReglasAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Comisiones particulares por familia de artículos y por cliente.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/reglas", (DatosReglaComision d, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => (await s.CrearReglaAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Comisión particular de un agente para una familia, un cliente o los dos.").RequierePermiso(Permisos.FacturaEmitir);
        g.MapDelete("/reglas/{id:guid}", async (Guid id, ComisionesAgentes s, CancellationToken ct) => (await s.EliminarReglaAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita una regla de comisión.").RequierePermiso(Permisos.FacturaEmitir);
        g.MapGet("/calculo/{agenteId:guid}", (Guid agenteId, DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, ComisionesAgentes s, IReloj reloj, CancellationToken ct) =>
                Con(c, async e =>
                {
                    var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
                    return (await s.CalcularAsync(e, agenteId, desde ?? new DateOnly(hoy.Year, hoy.Month, 1), hasta ?? hoy, ct).ConfigureAwait(false)).AOk();
                }))
            .WithSummary("Comisiones pendientes de liquidar de un agente en el periodo (por defecto, el mes en curso).").RequierePermiso(Permisos.FacturaLeer);
        g.MapGet("/liquidaciones", (Guid? agenteId, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => Results.Ok(await s.LiquidacionesAsync(e, agenteId, ct).ConfigureAwait(false))))
            .WithSummary("Liquidaciones de comisiones.").RequierePermiso(Permisos.FacturaLeer);
        g.MapPost("/liquidaciones", (DatosLiquidacionAgente d, IContextoEmpresa c, ComisionesAgentes s, CancellationToken ct) =>
                Con(c, async e => (await s.EmitirAsync(e, d, ct).ConfigureAwait(false)) is var r && r.EsCorrecto
                    ? Results.Created($"/comisiones/liquidaciones/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error)))
            .WithSummary("Liquida las comisiones pendientes del periodo (y, si el agente es externo, registra su factura).").RequierePermiso(Permisos.FacturaEmitir);
        g.MapPost("/liquidaciones/{id:guid}/anular", async (Guid id, ComisionesAgentes s, CancellationToken ct) => (await s.AnularAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Anula una liquidación (y la factura del agente): sus comisiones vuelven a estar pendientes.").RequierePermiso(Permisos.FacturaEmitir);
        return rutas;
    }

    private static async Task<IResult> Con(IContextoEmpresa c, Func<Guid, Task<IResult>> accion) =>
        c.EmpresaId is { } e ? await accion(e).ConfigureAwait(false) : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
