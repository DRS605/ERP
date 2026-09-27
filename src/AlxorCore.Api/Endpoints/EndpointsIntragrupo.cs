using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;

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

        g.MapGet("/consolidado", async (int? ejercicio, IContextoEmpresa contexto, ClaimsPrincipal usuario, ConsolidacionGrupo consolidacion, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : Results.Ok(await consolidacion.ConsolidarAsync(empresa, u, ejercicio ?? DateTime.UtcNow.Year, ct).ConfigureAwait(false)))
            .WithSummary("Consolidación del grupo: saldos de todas las empresas por cuenta, con las ventas, compras y saldos intragrupo eliminados.")
            .RequierePermiso(Permisos.ContabilidadLeer);

        g.MapGet("/perimetro", async (IContextoEmpresa contexto, ConfiguracionConsolidacion config, CancellationToken ct) =>
                contexto.GrupoId is { } grupo ? Results.Ok(await config.PerimetroAsync(grupo, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Participación y método de consolidación de cada empresa del grupo (sin fijar: global al 100 %).")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPut("/perimetro/{empresaId:guid}", async (Guid empresaId, DatosPerimetro datos, IContextoEmpresa contexto, ConfiguracionConsolidacion config, CancellationToken ct) =>
                contexto.GrupoId is { } grupo ? (await config.FijarPerimetroAsync(grupo, empresaId, datos, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Fija la participación (%) y el método (Global, Proporcional o Excluida) de una empresa del grupo.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapGet("/correspondencias", async (IContextoEmpresa contexto, ConfiguracionConsolidacion config, CancellationToken ct) =>
                contexto.GrupoId is { } grupo ? Results.Ok(await config.CorrespondenciasAsync(grupo, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Correspondencias de cuentas recíprocas entre empresas del grupo.")
            .RequierePermiso(Permisos.ContabilidadLeer);
        g.MapPost("/correspondencias", async (DatosCorrespondencia datos, IContextoEmpresa contexto, ConfiguracionConsolidacion config, CancellationToken ct) =>
            {
                if (contexto.GrupoId is not { } grupo)
                {
                    return SinEmpresa();
                }

                var r = await config.CrearCorrespondenciaAsync(grupo, datos, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/intragrupo/correspondencias/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Empareja una cuenta de una empresa con la recíproca de otra (se eliminan en la consolidación).")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapPut("/correspondencias/{id:guid}", async (Guid id, DatosCorrespondencia datos, IContextoEmpresa contexto, ConfiguracionConsolidacion config, CancellationToken ct) =>
                contexto.GrupoId is { } grupo ? (await config.ActualizarCorrespondenciaAsync(grupo, id, datos, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Modifica una correspondencia de cuentas.")
            .RequierePermiso(Permisos.ContabilidadGestionar);
        g.MapDelete("/correspondencias/{id:guid}", async (Guid id, ConfiguracionConsolidacion config, CancellationToken ct) =>
                (await config.EliminarCorrespondenciaAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Elimina una correspondencia de cuentas.")
            .RequierePermiso(Permisos.ContabilidadGestionar);

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

        g.MapPost("/albaranes/{id:guid}/traspasar", async (Guid id, IContextoEmpresa contexto, OperacionesIntragrupo op, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresa)
                {
                    return SinEmpresa();
                }

                var r = await op.TraspasarAlbaranAsync(empresa, id, ct).ConfigureAwait(false);
                return r.EsFallo ? ResultadosHttp.AProblema(r.Error)
                    : r.Valor is null ? ResultadosHttp.AProblema(Error.Validacion("intragrupo.no_intragrupo", "El cliente del albarán no está enlazado con otra empresa del grupo."))
                    : Results.Ok(new { PedidoCompraId = r.Valor });
            })
            .WithSummary("Vuelve a enviar el traspaso de un albarán de venta a la empresa del grupo (pedido y recepción en su almacén; es idempotente).")
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

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
