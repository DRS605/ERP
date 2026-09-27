using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Aplicacion.Puertos;

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

        g.MapPost("/albaranes/{id:guid}/deshacer-traspaso", async (Guid id, IContextoEmpresa contexto, OperacionesIntragrupo op, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa()
                    : (await op.AnularTraspasoAsync(empresa, id, "Albarán de venta anulado.", ct).ConfigureAwait(false)) is { EsFallo: true } r
                        ? ResultadosHttp.AProblema(r.Error) : Results.NoContent())
            .WithSummary("Repite, en la empresa receptora, la anulación del traspaso de un albarán de venta ya anulado (p. ej. tras regularizar su almacén).")
            .RequierePermiso(Permisos.FacturaEmitir);

        g.MapGet("/traspasos/almacenes", async (IContextoEmpresa contexto, AlmacenesTraspaso almacenes, CancellationToken ct) =>
                contexto.EmpresaId is { } empresa
                    ? Results.Ok((await almacenes.ListarAsync(empresa, ct).ConfigureAwait(false)).Select(a => new AlmacenTraspasoDto(a.Id, a.EmpresaOrigenId, a.AlmacenId)))
                    : SinEmpresa())
            .WithSummary("Almacén de entrada de los traspasos de otras empresas del grupo (por empresa de origen o general).")
            .RequierePermiso(Permisos.CompraLeer);
        g.MapPut("/traspasos/almacenes", async (DatosAlmacenTraspaso datos, IContextoEmpresa contexto, AlmacenesTraspaso almacenes,
                AlxorCore.Inventario.Aplicacion.GestionAlmacenes gestion, IConsultaEmpresas empresas, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is not { } empresa || contexto.GrupoId is not { } grupo)
                {
                    return SinEmpresa();
                }

                if (datos.EmpresaOrigenId is { } origen && (origen == empresa
                    || !(await empresas.EmpresasDelGrupoAsync(grupo, ct).ConfigureAwait(false)).Any(e => e.Id == origen)))
                {
                    return ResultadosHttp.AProblema(Error.Validacion("almacen_traspaso.origen", "La empresa de origen debe ser otra empresa del grupo."));
                }

                if (datos.AlmacenId is { } alm && !(await gestion.ListarAlmacenesAsync(empresa, ct).ConfigureAwait(false)).Any(a => a.Id == alm && a.Activo))
                {
                    return ResultadosHttp.AProblema(Error.Validacion("almacen_traspaso.almacen", "El almacén no existe o no está activo."));
                }

                var a = await almacenes.FijarAsync(empresa, datos.EmpresaOrigenId, datos.AlmacenId, ct).ConfigureAwait(false);
                return Results.Ok(new AlmacenTraspasoDto(a.Id, a.EmpresaOrigenId, a.AlmacenId));
            })
            .WithSummary("Fija el almacén de entrada de los traspasos (sin almacén: se recibe sin entrada en inventario).")
            .RequierePermiso(Permisos.CompraGestionar);
        g.MapDelete("/traspasos/almacenes/{id:guid}", async (Guid id, AlmacenesTraspaso almacenes, CancellationToken ct) =>
                (await almacenes.QuitarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita la configuración: vuelve al almacén activo de código más bajo.")
            .RequierePermiso(Permisos.CompraGestionar);

        g.MapPost("/facturas/{id:guid}/liquidar", async (Guid id, LiquidarIntragrupoPeticion? peticion, IContextoEmpresa contexto, ClaimsPrincipal usuario,
                OperacionesIntragrupo op, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa || usuario.ObtenerUsuarioId() is not { } u
                    ? ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."))
                    : (await op.LiquidarAsync(empresa, u, id, peticion ?? new LiquidarIntragrupoPeticion(), ct).ConfigureAwait(false)).AOk())
            .WithSummary("Liquida una factura intragrupo: cobro en esta empresa y pago del gasto en la receptora, por el mismo importe.")
            .RequierePermiso(Permisos.CobroRegistrar);

        return rutas;
    }

    public sealed record DatosAlmacenTraspaso(Guid? EmpresaOrigenId, Guid? AlmacenId);

    public sealed record AlmacenTraspasoDto(Guid Id, Guid? EmpresaOrigenId, Guid? AlmacenId);

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
