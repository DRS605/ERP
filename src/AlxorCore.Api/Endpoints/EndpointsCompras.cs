using AlxorCore.Api.Comun;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Motivo de rechazo de una solicitud.</summary>
public sealed record RechazarSolicitudPeticion(string? Motivo = null);

/// <summary>Endpoints REST del módulo Compras (solicitud → pedido → albarán → factura).</summary>
public static class EndpointsCompras
{
    public static IEndpointRouteBuilder MapearCompras(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var sol = rutas.MapGroup("/compras/solicitudes").WithTags("Compras · Solicitudes");
        sol.MapGet("", ListarSolicitudesAsync).WithSummary("Lista las solicitudes de compra.").RequierePermiso(Permisos.CompraLeer);
        sol.MapPost("", CrearSolicitudAsync).WithSummary("Crea una solicitud de compra.").RequierePermiso(Permisos.CompraGestionar);
        sol.MapPost("/{id:guid}/aprobar", AprobarSolicitudAsync).WithSummary("Aprueba una solicitud.").RequierePermiso(Permisos.CompraGestionar);
        sol.MapPost("/{id:guid}/rechazar", RechazarSolicitudAsync).WithSummary("Rechaza una solicitud.").RequierePermiso(Permisos.CompraGestionar);

        var ped = rutas.MapGroup("/compras/pedidos").WithTags("Compras · Pedidos");
        ped.MapGet("", ListarPedidosAsync).WithSummary("Lista los pedidos de compra.").RequierePermiso(Permisos.CompraLeer);
        ped.MapGet("/{id:guid}", ObtenerPedidoAsync).WithSummary("Obtiene un pedido.").RequierePermiso(Permisos.CompraLeer);
        ped.MapGet("/{id:guid}/albaranes", AlbaranesAsync).WithSummary("Albaranes de recepción del pedido.").RequierePermiso(Permisos.CompraLeer);
        ped.MapPost("", CrearPedidoAsync).WithSummary("Crea un pedido de compra (opcionalmente desde una solicitud).").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/confirmar", ConfirmarPedidoAsync).WithSummary("Confirma un pedido.").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/recibir", RecibirAsync).WithSummary("Registra un albarán de recepción.").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/facturar", FacturarAsync).WithSummary("Factura el pedido (genera el gasto y, en modo Completo, el asiento).").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/cancelar", CancelarPedidoAsync).WithSummary("Cancela un pedido.").RequierePermiso(Permisos.CompraGestionar);

        return rutas;
    }

    private static Guid? Empresa(IContextoEmpresa c) => c.EmpresaId;

    private static async Task<IResult> ListarSolicitudesAsync(IContextoEmpresa contexto, ListarSolicitudes caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId!.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearSolicitudAsync(CrearSolicitudComando comando, IContextoEmpresa contexto, CrearSolicitud caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(contexto.EmpresaId!.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/compras/solicitudes") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> AprobarSolicitudAsync(Guid id, DecidirSolicitud caso, CancellationToken ct)
        => (await caso.AprobarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> RechazarSolicitudAsync(Guid id, RechazarSolicitudPeticion peticion, DecidirSolicitud caso, CancellationToken ct)
        => (await caso.RechazarAsync(id, peticion?.Motivo, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ListarPedidosAsync(IContextoEmpresa contexto, ListarPedidos caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId!.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerPedidoAsync(Guid id, ObtenerPedido caso, CancellationToken ct)
    {
        var dto = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return dto is null ? ResultadosHttp.AProblema(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido.")) : Results.Ok(dto);
    }

    private static async Task<IResult> AlbaranesAsync(Guid id, IContextoEmpresa contexto, ListarAlbaranesPedido caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId!.Value, id, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearPedidoAsync(CrearPedidoComando comando, IContextoEmpresa contexto, CrearPedido caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(contexto.EmpresaId!.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/compras/pedidos/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ConfirmarPedidoAsync(Guid id, DecidirPedido caso, CancellationToken ct)
        => (await caso.ConfirmarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> CancelarPedidoAsync(Guid id, DecidirPedido caso, CancellationToken ct)
        => (await caso.CancelarAsync(id, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> RecibirAsync(Guid id, RecibirMercanciaComando comando, IContextoEmpresa contexto, RecibirMercancia caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        var r = await caso.EjecutarAsync(contexto.EmpresaId!.Value, id, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/compras/pedidos/{id}/albaranes") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> FacturarAsync(Guid id, FacturarPedidoComando comando, IContextoEmpresa contexto, FacturarPedido caso, CancellationToken ct)
    {
        if (Empresa(contexto) is null) return SinEmpresa();
        return (await caso.EjecutarAsync(contexto.EmpresaId!.Value, id, comando, ct).ConfigureAwait(false)).AOk();
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
