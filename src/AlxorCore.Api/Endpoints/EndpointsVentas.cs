using AlxorCore.Api.Comun;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints del ciclo de venta: pedido de venta y albarán de entrega.</summary>
public static class EndpointsVentas
{
    public static IEndpointRouteBuilder MapearVentas(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var pedidos = rutas.MapGroup("/pedidos-venta").WithTags("Ventas");

        pedidos.MapGet("", ListarAsync)
            .WithSummary("Lista los pedidos de venta de la empresa.")
            .RequierePermiso(Permisos.FacturaLeer);

        pedidos.MapGet("/{id:guid}", ObtenerAsync)
            .WithSummary("Obtiene un pedido de venta con sus líneas.")
            .RequierePermiso(Permisos.FacturaLeer);

        pedidos.MapPost("", CrearAsync)
            .WithSummary("Crea un pedido de venta.")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPut("/{id:guid}", async (Guid id, CrearPedidoVentaComando comando, CrearPedidoVenta caso, CancellationToken ct) =>
                (await caso.ModificarAsync(id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica un pedido de venta sin entregas ni factura (cliente, fecha y líneas).")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPost("/desde-presupuesto", DesdePresupuestoAsync)
            .WithSummary("Crea un pedido de venta a partir de un presupuesto.")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPost("/{id:guid}/confirmar", ConfirmarAsync)
            .WithSummary("Confirma un pedido de venta en borrador.")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPost("/{id:guid}/cancelar", CancelarAsync)
            .WithSummary("Cancela un pedido de venta.")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPost("/{id:guid}/entregar", EntregarAsync)
            .WithSummary("Registra un albarán de entrega contra el pedido.")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapPost("/{id:guid}/albaranes/{albaranId:guid}/anular", async (Guid id, Guid albaranId, AnularAlbaranPeticion? peticion, AnularAlbaranVenta caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, albaranId, peticion?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un albarán de entrega: sus cantidades vuelven a quedar pendientes de servir (si el pedido no está facturado).")
            .RequierePermiso(Permisos.FacturaEmitir);

        pedidos.MapGet("/{id:guid}/albaranes", AlbaranesAsync)
            .WithSummary("Lista los albaranes de entrega del pedido.")
            .RequierePermiso(Permisos.FacturaLeer);

        pedidos.MapPost("/{id:guid}/facturar", FacturarAsync)
            .WithSummary("Factura el pedido: genera la factura real y la enlaza.")
            .RequierePermiso(Permisos.FacturaEmitir);

        MapearAlbaranes(rutas);
        return rutas;
    }

    /// <summary>
    /// Albaranes de venta como documento central: directos o de pedido, con salida de stock al emitirse, valoración a
    /// posteriori y facturación de varios albaranes en una factura o masiva por cliente.
    /// </summary>
    private static void MapearAlbaranes(IEndpointRouteBuilder rutas)
    {
        var albaranes = rutas.MapGroup("/albaranes-venta").WithTags("Ventas");

        albaranes.MapGet("", async (Guid? clienteId, string? estado, DateOnly? desde, DateOnly? hasta, IContextoEmpresa contexto, ConsultarAlbaranesVenta caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is null)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                AlxorCore.Facturacion.Dominio.EstadoAlbaranVenta? filtroEstado = null;
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    if (!Enum.TryParse<AlxorCore.Facturacion.Dominio.EstadoAlbaranVenta>(estado, true, out var e))
                    {
                        return ResultadosHttp.AProblema(Error.Validacion("albaranventa.estado", "Estado no válido: PendienteValorar, PendienteFacturar, Facturado o Anulado."));
                    }

                    filtroEstado = e;
                }

                return Results.Ok(await caso.ListarAsync(contexto.EmpresaId.Value, new FiltroAlbaranesVenta(clienteId, filtroEstado, desde, hasta), ct).ConfigureAwait(false));
            })
            .WithSummary("Albaranes de venta (filtro por cliente, estado y fechas).")
            .RequierePermiso(Permisos.FacturaLeer);

        albaranes.MapGet("/{id:guid}", async (Guid id, ConsultarAlbaranesVenta caso, CancellationToken ct) =>
                await caso.ObtenerAsync(id, ct).ConfigureAwait(false) is { } a ? Results.Ok(a) : Results.NotFound())
            .WithSummary("Albarán de venta con sus líneas.")
            .RequierePermiso(Permisos.FacturaLeer);

        albaranes.MapPost("", async (CrearAlbaranVentaComando comando, IContextoEmpresa contexto, CrearAlbaranVenta caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is null)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/albaranes-venta/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Albarán directo (sin pedido): saca la mercancía del almacén.")
            .RequierePermiso(Permisos.FacturaEmitir);

        albaranes.MapPut("/{id:guid}/valorar", async (Guid id, ValorarAlbaranVentaComando comando, ValorarAlbaranVenta caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Fija los precios de un albarán entregado a precio por fijar.")
            .RequierePermiso(Permisos.FacturaEmitir);

        albaranes.MapPost("/{id:guid}/anular", async (Guid id, AnularAlbaranPeticion? peticion, AnularAlbaranVenta caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(null, id, peticion?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula un albarán sin facturar: la mercancía vuelve al almacén.")
            .RequierePermiso(Permisos.FacturaEmitir);

        albaranes.MapPost("/facturar", async (FacturarAlbaranesComando comando, IContextoEmpresa contexto, FacturarAlbaranesVenta caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is null)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
                return r.EsCorrecto ? r.ACreado($"/facturas/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
            })
            .WithSummary("Factura uno o varios albaranes del mismo cliente en una factura.")
            .RequierePermiso(Permisos.FacturaEmitir);

        albaranes.MapPost("/facturacion-masiva", async (FacturacionMasivaAlbaranesComando comando, IContextoEmpresa contexto, FacturarAlbaranesVenta caso, CancellationToken ct) =>
            {
                if (contexto.EmpresaId is null)
                {
                    return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
                }

                return Results.Ok(await caso.MasivaAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false));
            })
            .WithSummary("Factura los albaranes valorados pendientes hasta una fecha: una factura por cliente (o por albarán).")
            .RequierePermiso(Permisos.FacturaEmitir);
    }

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarPedidosVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ObtenerAsync(Guid id, IContextoEmpresa contexto, ObtenerPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var dto = await caso.EjecutarAsync(id, ct).ConfigureAwait(false);
        return dto is null ? Results.NotFound() : Results.Ok(dto);
    }

    private static async Task<IResult> CrearAsync(CrearPedidoVentaComando comando, IContextoEmpresa contexto, CrearPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/pedidos-venta/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> DesdePresupuestoAsync(CrearPedidoDesdePresupuestoComando comando, IContextoEmpresa contexto, CrearPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.DesdePresupuestoAsync(contexto.EmpresaId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/pedidos-venta/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ConfirmarAsync(Guid id, IContextoEmpresa contexto, DecidirPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.ConfirmarAsync(id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok(r.Valor) : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> CancelarAsync(Guid id, IContextoEmpresa contexto, DecidirPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.CancelarAsync(id, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok(r.Valor) : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> EntregarAsync(Guid id, EntregarPedidoComando comando, IContextoEmpresa contexto, EntregarPedido caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok(r.Valor) : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> AlbaranesAsync(Guid id, IContextoEmpresa contexto, ListarAlbaranesVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, id, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> FacturarAsync(Guid id, FacturarPedidoVentaComando comando, IContextoEmpresa contexto, FacturarPedidoVenta caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, id, comando ?? new FacturarPedidoVentaComando(), ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/facturas/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }
}

/// <summary>Cuerpo de la anulación de un albarán de venta.</summary>
public sealed record AnularAlbaranPeticion(string? Motivo);
