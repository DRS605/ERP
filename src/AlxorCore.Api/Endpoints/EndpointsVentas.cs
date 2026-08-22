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

        pedidos.MapGet("/{id:guid}/albaranes", AlbaranesAsync)
            .WithSummary("Lista los albaranes de entrega del pedido.")
            .RequierePermiso(Permisos.FacturaLeer);

        pedidos.MapPost("/{id:guid}/facturar", FacturarAsync)
            .WithSummary("Factura el pedido: genera la factura real y la enlaza.")
            .RequierePermiso(Permisos.FacturaEmitir);

        return rutas;
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
