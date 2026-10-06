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
        sol.MapPut("/{id:guid}", async (Guid id, CrearSolicitudComando cmd, CrearSolicitud caso, CancellationToken ct) =>
            (await caso.ModificarAsync(id, cmd, ct).ConfigureAwait(false)).AOk()).WithSummary("Modifica una solicitud en borrador o rechazada.").RequierePermiso(Permisos.CompraGestionar);
        sol.MapDelete("/{id:guid}", async (Guid id, CrearSolicitud caso, CancellationToken ct) =>
            (await caso.EliminarAsync(id, ct).ConfigureAwait(false)).AOk()).WithSummary("Elimina una solicitud en borrador o rechazada.").RequierePermiso(Permisos.CompraGestionar);
        sol.MapPost("/{id:guid}/aprobar", AprobarSolicitudAsync).WithSummary("Aprueba una solicitud.").RequierePermiso(Permisos.CompraGestionar);
        sol.MapPost("/{id:guid}/rechazar", RechazarSolicitudAsync).WithSummary("Rechaza una solicitud.").RequierePermiso(Permisos.CompraGestionar);

        var ped = rutas.MapGroup("/compras/pedidos").WithTags("Compras · Pedidos");
        ped.MapGet("", ListarPedidosAsync).WithSummary("Lista los pedidos de compra.").RequierePermiso(Permisos.CompraLeer);
        ped.MapGet("/{id:guid}", ObtenerPedidoAsync).WithSummary("Obtiene un pedido.").RequierePermiso(Permisos.CompraLeer);
        ped.MapPost("/{id:guid}/albaranes/{albaranId:guid}/anular", async (Guid id, Guid albaranId, AnularAlbaranPeticion? peticion, IContextoEmpresa contexto, AnularAlbaranCompra caso, CancellationToken ct) =>
                contexto.EmpresaId is { } e
                    ? (await caso.EjecutarAsync(e, id, albaranId, peticion?.Motivo, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithSummary("Anula un albarán de recepción: lo recibido vuelve a quedar pendiente y sale del almacén (si las existencias siguen ahí).")
            .RequierePermiso(Permisos.CompraGestionar);
        ped.MapGet("/{id:guid}/albaranes", AlbaranesAsync).WithSummary("Albaranes de recepción del pedido.").RequierePermiso(Permisos.CompraLeer);
        ped.MapPost("/simular", async (CrearPedidoComando cmd, IContextoEmpresa c, CrearPedido caso, CancellationToken ct) =>
                c.EmpresaId is { } e ? (await caso.SimularAsync(e, cmd, ct).ConfigureAwait(false)).AOk()
                    : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero.")))
            .WithSummary("Calcula un pedido de compra sin guardarlo (conceptos, importes y coste de entrada).").RequierePermiso(Permisos.CompraLeer);
        ped.MapPost("", CrearPedidoAsync).WithSummary("Crea un pedido de compra (opcionalmente desde una solicitud).").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPut("/{id:guid}", async (Guid id, CrearPedidoComando cmd, CrearPedido caso, CancellationToken ct) =>
            (await caso.ModificarAsync(id, cmd, ct).ConfigureAwait(false)).AOk()).WithSummary("Modifica fecha y líneas de un pedido sin recepciones ni factura.").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/confirmar", ConfirmarPedidoAsync).WithSummary("Confirma un pedido.").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/recibir", RecibirAsync).WithSummary("Registra un albarán de recepción.").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/facturar", FacturarAsync).WithSummary("Factura el pedido (genera el gasto y, en modo Completo, el asiento).").RequierePermiso(Permisos.CompraGestionar);
        ped.MapPost("/{id:guid}/cancelar", CancelarPedidoAsync).WithSummary("Cancela un pedido.").RequierePermiso(Permisos.CompraGestionar);

        // Propuesta de compra (stock mínimo y necesidades de fabricación).
        rutas.MapGet("/compras/propuesta", async (IContextoEmpresa c, PropuestaCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? Results.Ok(await caso.CalcularAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithTags("Compras · Propuesta").WithSummary("Qué comprar: artículos bajo su mínimo y componentes que faltan para las órdenes de fabricación.")
            .RequierePermiso(Permisos.CompraLeer);
        rutas.MapPost("/compras/propuesta/pedidos", async (DatosGenerarPedidos d, IContextoEmpresa c, PropuestaCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? (await caso.GenerarPedidosAsync(e, d, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithTags("Compras · Propuesta").WithSummary("Convierte las líneas elegidas de la propuesta en pedidos de compra en borrador, uno por proveedor.")
            .RequierePermiso(Permisos.CompraGestionar);

        // Devoluciones a proveedor.
        var dev = rutas.MapGroup("/compras/devoluciones").WithTags("Compras · Devoluciones");
        dev.MapGet("", async (IContextoEmpresa c, GestionDevolucionesCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? Results.Ok(await caso.ListarAsync(e, ct).ConfigureAwait(false)) : SinEmpresa())
            .WithSummary("Devoluciones de mercancía a proveedores.").RequierePermiso(Permisos.CompraLeer);
        dev.MapGet("/{id:guid}", async (Guid id, GestionDevolucionesCompra caso, CancellationToken ct) =>
                await caso.ObtenerAsync(id, ct).ConfigureAwait(false) is { } d ? Results.Ok(d)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("devolucion_compra.no_encontrada", "La devolución no existe.")))
            .WithSummary("Una devolución a proveedor.").RequierePermiso(Permisos.CompraLeer);
        dev.MapGet("/devolubles/{pedidoId:guid}", async (Guid pedidoId, GestionDevolucionesCompra caso, CancellationToken ct) =>
                (await caso.DevolublesAsync(pedidoId, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Lo que aún se puede devolver de cada línea recibida del pedido.").RequierePermiso(Permisos.CompraLeer);
        dev.MapPost("", async (CrearDevolucionCompraComando cmd, IContextoEmpresa c, GestionDevolucionesCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? (await caso.CrearAsync(e, cmd, ct).ConfigureAwait(false)) is var r && r.EsCorrecto
                    ? Results.Created($"/compras/devoluciones/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error) : SinEmpresa())
            .WithSummary("Devuelve mercancía recibida al proveedor (sale del almacén y queda pendiente de abono).").RequierePermiso(Permisos.CompraGestionar);
        dev.MapPost("/{id:guid}/abonar", async (Guid id, AbonarDevolucionCompraComando cmd, IContextoEmpresa c, GestionDevolucionesCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? (await caso.AbonarAsync(e, id, cmd, ct).ConfigureAwait(false)).AOk() : SinEmpresa())
            .WithSummary("Registra el abono del proveedor: factura rectificativa recibida en negativo que rectifica la del pedido.").RequierePermiso(Permisos.CompraGestionar);
        dev.MapPost("/{id:guid}/cerrar-sin-abono", async (Guid id, GestionDevolucionesCompra caso, CancellationToken ct) =>
                (await caso.CerrarSinAbonoAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra la devolución sin abono (el proveedor repone la mercancía).").RequierePermiso(Permisos.CompraGestionar);
        dev.MapPost("/{id:guid}/anular", async (Guid id, IContextoEmpresa c, GestionDevolucionesCompra caso, CancellationToken ct) =>
                Empresa(c) is { } e ? (await caso.AnularAsync(e, id, ct).ConfigureAwait(false)).ASinContenido() : SinEmpresa())
            .WithSummary("Anula una devolución pendiente: la mercancía vuelve a entrar en el almacén.").RequierePermiso(Permisos.CompraGestionar);

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
