using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Operaciones entre empresas del grupo: espejo de las facturas a clientes enlazados con otra empresa, cuadre recíproco y
/// liquidación (cobro en una empresa y pago en la otra).
/// </summary>
public static class EndpointsIntragrupo
{
    public static IEndpointRouteBuilder MapearIntragrupo(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/intragrupo").WithTags("Operaciones entre empresas");

        g.MapGet("/cuadre", async (int? ejercicio, IContextoEmpresa contexto, ClaimsPrincipal usuario, OperacionesIntragrupo op, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : Results.Ok(await op.CuadreAsync(empresa, u, ejercicio ?? DateTime.UtcNow.Year, ct).ConfigureAwait(false)))
            .WithSummary("Cuadre recíproco del ejercicio: lo facturado por cada empresa a otra del grupo frente a lo que esta ha contabilizado.")
            .RequierePermiso(Permisos.InformeLeer);

        g.MapPost("/facturas/{id:guid}/reflejar", async (Guid id, IContextoEmpresa contexto, OperacionesIntragrupo op, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresa)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await op.ReflejarFacturaAsync(empresa, id, ct).ConfigureAwait(false);
                return r.EsFallo ? ResultadosHttp.AProblema(r.Error)
                    : r.Valor is null ? ResultadosHttp.AProblema(Error.Validacion("intragrupo.no_intragrupo", "El cliente de la factura no está enlazado con otra empresa del grupo."))
                    : Results.Ok(r.Valor);
            })
            .WithSummary("Vuelve a dejar la factura en la bandeja de la empresa del grupo a la que se emitió (si no llegó; es idempotente).")
            .RequierePermiso(Permisos.FacturaEmitir);

        g.MapPost("/facturas/{id:guid}/liquidar", async (Guid id, LiquidarIntragrupoPeticion? peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario,
                OperacionesIntragrupo op, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : (await op.LiquidarAsync(empresa, u, id, peticion ?? new LiquidarIntragrupoPeticion(), ct).ConfigureAwait(false)).AOk())
            .WithSummary("Liquida una factura intragrupo: cobro en esta empresa y pago del gasto en la receptora, por el mismo importe.")
            .RequierePermiso(Permisos.CobroRegistrar);

        return rutas;
    }
}
