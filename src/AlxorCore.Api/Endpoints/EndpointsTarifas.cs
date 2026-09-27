using AlxorCore.Api.Comun;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Petición para asignar (o quitar, con null) la tarifa de un cliente.</summary>
public sealed record AsignarTarifaPeticion(Guid? TarifaId);

/// <summary>
/// Endpoints de tarifas de precios de venta (módulo Ventas): tarifas con precios especiales,
/// descuentos por producto, familia o generales, escalado por cantidad y vigencia; asignación a
/// clientes y consulta del precio que corresponde a una línea.
/// </summary>
public static class EndpointsTarifas
{
    public static IEndpointRouteBuilder MapearTarifas(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var tarifas = rutas.MapGroup("/tarifas").WithTags("Tarifas");

        tarifas.MapGet("", async (ConsultarTarifas caso, CancellationToken ct) => Results.Ok(await caso.ListarAsync(ct).ConfigureAwait(false)))
            .WithSummary("Lista las tarifas de precios del grupo.")
            .RequireAuthorization();

        tarifas.MapGet("/{id:guid}", async (Guid id, ConsultarTarifas caso, CancellationToken ct) =>
                (await caso.ObtenerAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Detalle de una tarifa con sus líneas.")
            .RequireAuthorization();

        tarifas.MapPost("", CrearAsync)
            .WithSummary("Crea una tarifa de precios.")
            .RequierePermiso(Permisos.ProductoGestionar);

        tarifas.MapPut("/{id:guid}", async (Guid id, ActualizarTarifaComando comando, ActualizarTarifa caso, CancellationToken ct) =>
                (await caso.EjecutarAsync(id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Actualiza nombre, estado y líneas de una tarifa (sustituye todas las líneas).")
            .RequierePermiso(Permisos.ProductoGestionar);

        tarifas.MapDelete("/{id:guid}", async (Guid id, BajasCatalogo caso, CancellationToken ct) =>
                (await caso.EliminarTarifaAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una tarifa que ningún cliente tiene asignada (para dejar de usarla sin borrarla, desactívala).")
            .RequierePermiso(Permisos.ProductoGestionar);

        rutas.MapGet("/precios", PrecioAsync)
            .WithTags("Tarifas")
            .WithSummary("Precio y descuento que corresponden a un producto para un cliente, cantidad y fecha (y de dónde salen).")
            .RequireAuthorization();

        rutas.MapPut("/clientes/{id:guid}/tarifa", AsignarAsync)
            .WithTags("Tarifas")
            .WithSummary("Asigna (o quita) la tarifa de precios de un cliente.")
            .RequierePermiso(Permisos.ClienteGestionar);

        return rutas;
    }

    private static async Task<IResult> CrearAsync(CrearTarifaComando comando, IContextoEmpresa contexto, CrearTarifa caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.GrupoId.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/tarifas/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> PrecioAsync(
        Guid clienteId, Guid productoId, decimal? cantidad, DateOnly? fecha,
        IConsultaClientes clientes, IConsultaProductos productos, IResolverPrecioVenta precios, IReloj reloj, CancellationToken ct)
    {
        var cliente = await clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
        var producto = await productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
        if (cliente is null || producto is null)
        {
            return ResultadosHttp.AProblema(Error.NoEncontrado("precio.referencia_no_encontrada", "El cliente o el producto no existen."));
        }

        var dia = fecha ?? DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
        var precio = await precios.ResolverAsync(cliente.TarifaId, productoId, cantidad ?? 1m, dia, ct).ConfigureAwait(false);
        return Results.Ok(precio ?? new PrecioVentaDto(producto.PrecioUnitario, 0m, "Precio del producto"));
    }

    private static async Task<IResult> AsignarAsync(
        Guid id, AsignarTarifaPeticion peticion, ConsultarTarifas tarifas, AsignarTarifaCliente caso, CancellationToken ct)
    {
        if (peticion.TarifaId is { } tarifaId && (await tarifas.ObtenerAsync(tarifaId, ct).ConfigureAwait(false)).EsFallo)
        {
            return ResultadosHttp.AProblema(Error.Validacion("tarifa.no_encontrada", "La tarifa no existe."));
        }

        return (await caso.EjecutarAsync(id, peticion.TarifaId, ct).ConfigureAwait(false)).AOk();
    }
}
