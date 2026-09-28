using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Endpoints;

/// <summary>Motivo de la anulación de una entrega a cuenta o de una liquidación de pagos.</summary>
public sealed record PeticionAnularPagos(string? Motivo);

/// <summary>Entregas a cuenta a proveedores y agricultores, y liquidaciones de pagos (individuales y masivas).</summary>
public static class EndpointsLiquidacionesPagos
{
    public static IEndpointRouteBuilder MapearLiquidacionesPagos(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var entregas = rutas.MapGroup("/pagos/entregas-cuenta").WithTags("Tesorería");
        entregas.MapGet("/", async (Guid? proveedorId, bool? pendientes, LiquidacionesPagos s, CancellationToken ct) =>
                Results.Ok(await s.EntregasAsync(proveedorId, pendientes == true, ct).ConfigureAwait(false)))
            .WithSummary("Entregas a cuenta a proveedores (sin IVA), con lo cancelado y lo pendiente.")
            .RequierePermiso(Permisos.GastoLeer);
        entregas.MapPost("/", async (DatosEntregaCuenta datos, IContextoEmpresa contexto, LiquidacionesPagos s, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa()
                    : Creado(await s.RegistrarEntregaAsync(empresa, datos, ct).ConfigureAwait(false), e => $"/pagos/entregas-cuenta/{e.Id}"))
            .WithSummary("Registra una entrega a cuenta a un proveedor o agricultor (asiento 407 a tesorería).")
            .RequierePermiso(Permisos.PagoRegistrar);
        entregas.MapPost("/{id:guid}/aplicar", async (Guid id, AplicarEntregaComando comando, IContextoEmpresa contexto, LiquidacionesPagos s, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await s.AplicarEntregaAsync(empresa, id, comando, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cancela (parte de) la entrega contra una factura del mismo proveedor (pago 400 a 407).")
            .RequierePermiso(Permisos.PagoRegistrar);
        entregas.MapPost("/{id:guid}/anular", async (Guid id, PeticionAnularPagos? peticion, LiquidacionesPagos s, CancellationToken ct) =>
                (await s.AnularEntregaAsync(id, peticion?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una entrega a cuenta sin nada cancelado (con su contraasiento).")
            .RequierePermiso(Permisos.PagoRegistrar);

        var liquidaciones = rutas.MapGroup("/pagos/liquidaciones").WithTags("Tesorería");
        liquidaciones.MapGet("/", async (Guid? proveedorId, Guid? loteId, LiquidacionesPagos s, CancellationToken ct) =>
                Results.Ok(await s.ListarAsync(proveedorId, loteId, ct).ConfigureAwait(false)))
            .WithSummary("Liquidaciones de pagos emitidas y anuladas.")
            .RequierePermiso(Permisos.GastoLeer);
        liquidaciones.MapGet("/pendientes", async (DateOnly? hasta, bool? agricultores, IPendientesLiquidacionPagos p, CancellationToken ct) =>
                Results.Ok(await p.ProveedoresAsync(hasta ?? DateOnly.FromDateTime(DateTime.Today), agricultores == true, ct).ConfigureAwait(false)))
            .WithSummary("Proveedores con facturas pendientes de pago hasta la fecha: lo pendiente, lo que deben como clientes y sus entregas a cuenta.")
            .RequierePermiso(Permisos.GastoLeer);
        liquidaciones.MapGet("/{id:guid}", async (Guid id, LiquidacionesPagos s, CancellationToken ct) => (await s.ObtenerAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Detalle de una liquidación de pagos.")
            .RequierePermiso(Permisos.GastoLeer);
        liquidaciones.MapPost("/previsualizar", async (DatosLiquidacionPagos datos, LiquidacionesPagos s, CancellationToken ct) =>
                (await s.PrevisualizarAsync(datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Calcula la liquidación de un proveedor sin registrar nada.")
            .RequierePermiso(Permisos.GastoLeer);
        liquidaciones.MapPost("/", async (DatosLiquidacionPagos datos, IContextoEmpresa contexto, LiquidacionesPagos s, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa()
                    : Creado(await s.EmitirAsync(empresa, datos, ct).ConfigureAwait(false), l => $"/pagos/liquidaciones/{l.Id}"))
            .WithSummary("Emite la liquidación de pagos de un proveedor: cancela entregas, compensa con lo que debe como cliente y paga el líquido.")
            .RequierePermiso(Permisos.PagoRegistrar);
        liquidaciones.MapPost("/masiva", async (DatosLiquidacionMasiva datos, IContextoEmpresa contexto, LiquidacionesPagos s, CancellationToken ct) =>
                contexto.EmpresaId is not { } empresa ? SinEmpresa() : (await s.MasivaAsync(empresa, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Liquida a todos los proveedores (o solo agricultores) con facturas pendientes hasta la fecha, con una remesa de transferencias.")
            .RequierePermiso(Permisos.PagoRegistrar);
        liquidaciones.MapPost("/{id:guid}/anular", async (Guid id, PeticionAnularPagos? peticion, LiquidacionesPagos s, CancellationToken ct) =>
                (await s.AnularAsync(id, peticion?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula la liquidación: deshace sus pagos, compensaciones y cancelaciones de entregas.")
            .RequierePermiso(Permisos.PagoRegistrar);

        return rutas;
    }

    private static IResult Creado<T>(Resultado<T> r, Func<T, string> ubicacion) =>
        r.EsFallo ? ResultadosHttp.AProblema(r.Error) : Results.Created(ubicacion(r.Valor), r.Valor);

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
}
