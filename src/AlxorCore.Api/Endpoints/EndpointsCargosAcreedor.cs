using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Cargos y abonos con acreedor (transportistas, comisionistas): pendientes de liquidar y su liquidación en la factura del acreedor.</summary>
public static class EndpointsCargosAcreedor
{
    public static IEndpointRouteBuilder MapearCargosAcreedor(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var grupo = rutas.MapGroup("/gastos/cargos-acreedores").WithTags("Gastos");

        grupo.MapGet("", async (Guid? acreedorId, DateOnly? desde, DateOnly? hasta, bool? todos, CargosAcreedores caso, CancellationToken ct) =>
                Results.Ok(await caso.ListarAsync(acreedorId, desde, hasta, todos ?? false, ct).ConfigureAwait(false)))
            .WithSummary("Cargos con acreedor de facturas, albaranes de venta y pedidos de compra: los pendientes de liquidar (o todos, con todos=true).")
            .RequierePermiso(Permisos.GastoLeer);

        grupo.MapPost("/liquidar", async (LiquidarAcreedorComando comando, IContextoEmpresa contexto, CargosAcreedores caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresa)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await caso.LiquidarAsync(empresa, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/gastos/{r.Valor.GastoId}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Registra la factura del acreedor (gasto con IVA y retención) con los cargos elegidos o todos los pendientes de las fechas.")
            .RequierePermiso(Permisos.GastoGestionar);

        return rutas;
    }
}
