using AlxorCore.Api.Comun;
using AlxorCore.Bodega.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Bodegas (módulo <c>bodega</c>): depósitos, entradas de uva, precios y liquidación de la uva, operaciones de bodega
/// (elaboración, trasiego, coupage, merma, embotellado, granel) con su anulación, declaración de existencias y trazabilidad.
/// </summary>
public static class EndpointsBodega
{
    public sealed record PeticionMotivoBodega(string? Motivo);

    public static IEndpointRouteBuilder MapearBodega(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/bodega").WithTags("Bodega");

        // Depósitos.
        g.MapGet("/depositos", (IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await b.DepositosAsync(e, ct).ConfigureAwait(false))))
            .WithSummary("Depósitos con lo que contienen: litros, producto, calificación y composición por variedad y añada.").RequierePermiso(Permisos.BodegaLeer);
        g.MapGet("/depositos/{id:guid}", async (Guid id, UvaYDepositosBodega b, CancellationToken ct) =>
                await b.DepositoAsync(id, ct).ConfigureAwait(false) is { } d ? Results.Ok(d) : NoEncontrado("deposito"))
            .WithSummary("Un depósito con su contenido.").RequierePermiso(Permisos.BodegaLeer);
        g.MapPost("/depositos", (DatosDeposito d, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await b.CrearDepositoAsync(e, d, ct).ConfigureAwait(false), r => $"depositos/{r.Id}")))
            .WithSummary("Da de alta un depósito (código, nombre, tipo y capacidad en litros).").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPut("/depositos/{id:guid}", async (Guid id, DatosDeposito d, UvaYDepositosBodega b, CancellationToken ct) =>
                (await b.CambiarDepositoAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia nombre, tipo, capacidad (no por debajo de lo que tiene) o la baja (vacío).").RequierePermiso(Permisos.BodegaGestionar);
        g.MapDelete("/depositos/{id:guid}", async (Guid id, UvaYDepositosBodega b, CancellationToken ct) =>
                (await b.EliminarDepositoAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Borra un depósito vacío y sin operaciones.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapGet("/depositos/{id:guid}/operaciones", (Guid id, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await o.ListarAsync(e, null, null, id, ct).ConfigureAwait(false))))
            .WithSummary("Historial de operaciones del depósito.").RequierePermiso(Permisos.BodegaLeer);
        g.MapGet("/depositos/{id:guid}/origen", async (Guid id, OperacionesBodega o, CancellationToken ct) => (await o.OrigenAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Trazabilidad: de qué entradas de uva (viticultor, variedad, parcela) y por qué depósitos viene lo que contiene.").RequierePermiso(Permisos.BodegaLeer);

        // Uva.
        g.MapGet("/uva", (DateOnly? desde, DateOnly? hasta, Guid? viticultorId, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await b.EntradasAsync(e, desde, hasta, viticultorId, ct).ConfigureAwait(false))))
            .WithSummary("Entradas de uva (filtros por fechas y viticultor).").RequierePermiso(Permisos.BodegaLeer);
        g.MapPost("/uva", (DatosEntradaUva d, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await b.CrearEntradaAsync(e, d, ct).ConfigureAwait(false), r => $"uva/{r.Id}")))
            .WithSummary("Entrada de uva: viticultor, variedad, kilos, grado Baumé, parcela y calificación.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPut("/uva/{id:guid}", async (Guid id, DatosEntradaUva d, UvaYDepositosBodega b, CancellationToken ct) =>
                (await b.CorregirEntradaAsync(id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Corrige una entrada aún sin elaborar ni liquidar.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/uva/{id:guid}/anular", async (Guid id, PeticionMotivoBodega? p, UvaYDepositosBodega b, CancellationToken ct) =>
                (await b.AnularEntradaAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula una entrada sin elaborar ni liquidar.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapGet("/precios-uva", (int? anada, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await b.PreciosAsync(e, anada, ct).ConfigureAwait(false))))
            .WithSummary("Precios de la uva por variedad y añada.").RequierePermiso(Permisos.BodegaLeer);
        g.MapPut("/precios-uva", (DatosPrecioUva d, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => (await b.FijarPrecioAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Fija el precio de una variedad en una añada: precio por kilo al grado de referencia y el porcentaje por grado.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapDelete("/precios-uva/{id:guid}", async (Guid id, UvaYDepositosBodega b, CancellationToken ct) => (await b.EliminarPrecioAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita un precio de la uva (las liquidaciones hechas guardan el suyo).").RequierePermiso(Permisos.BodegaGestionar);
        g.MapGet("/liquidaciones", (Guid? viticultorId, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await b.LiquidacionesAsync(e, viticultorId, ct).ConfigureAwait(false))))
            .WithSummary("Liquidaciones de la uva a los viticultores.").RequierePermiso(Permisos.BodegaLeer);
        g.MapPost("/liquidaciones/simular", (DatosLiquidacionUva d, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => (await b.SimularLiquidacionAsync(e, d, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Calcula la liquidación de la uva sin guardarla.").RequierePermiso(Permisos.BodegaLeer);
        g.MapPost("/liquidaciones", (DatosLiquidacionUva d, IContextoEmpresa c, UvaYDepositosBodega b, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await b.CrearLiquidacionAsync(e, d, ct).ConfigureAwait(false), r => $"liquidaciones/{r.Id}")))
            .WithSummary("Liquida la uva pendiente del viticultor y registra la autofactura (compensación REAGP o IVA, y retención).").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/liquidaciones/{id:guid}/anular", async (Guid id, PeticionMotivoBodega? p, UvaYDepositosBodega b, CancellationToken ct) =>
                (await b.AnularLiquidacionAsync(id, p?.Motivo, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anula la liquidación y su autofactura (si no está pagada); la uva vuelve a estar pendiente.").RequierePermiso(Permisos.BodegaGestionar);

        // Operaciones.
        g.MapGet("/operaciones", (DateOnly? desde, DateOnly? hasta, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await o.ListarAsync(e, desde, hasta, null, ct).ConfigureAwait(false))))
            .WithSummary("Operaciones de bodega (filtro por fechas).").RequierePermiso(Permisos.BodegaLeer);
        g.MapGet("/operaciones/{id:guid}", async (Guid id, OperacionesBodega o, CancellationToken ct) =>
                await o.ObtenerAsync(id, ct).ConfigureAwait(false) is { } op ? Results.Ok(op) : NoEncontrado("operacion_bodega"))
            .WithSummary("Una operación con sus movimientos.").RequierePermiso(Permisos.BodegaLeer);
        g.MapPost("/operaciones/elaboracion", (DatosElaboracion d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.ElaborarAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Elabora entradas de uva: los litros obtenidos entran en un depósito con su composición.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/trasiego", (DatosTrasiego d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.TrasegarAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Trasiego de un depósito a otro (todo o parte), con su merma.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/coupage", (DatosCoupage d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.CoupageAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Coupage: mezcla los litros de varios depósitos en uno.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/merma", (DatosMerma d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.MermaAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Merma de un depósito (evaporación, lías…), con el motivo.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/embotellado", (DatosEmbotellado d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.EmbotellarAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Embotellado: salen del depósito las botellas × su formato y entran en las existencias del artículo.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/granel", (DatosSalidaGranel d, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Creado(await o.SalidaGranelAsync(e, d, ct).ConfigureAwait(false), r => $"operaciones/{r.Id}")))
            .WithSummary("Venta a granel: salen los litros y se emite el albarán al cliente.").RequierePermiso(Permisos.BodegaGestionar);
        g.MapPost("/operaciones/{id:guid}/anular", (Guid id, PeticionMotivoBodega? p, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => (await o.AnularAsync(e, id, p?.Motivo, ct).ConfigureAwait(false)).AOk()))
            .WithSummary("Deshace la operación si es la última de sus depósitos (vuelven a como estaban).").RequierePermiso(Permisos.BodegaGestionar);
        g.MapGet("/declaracion", (int anio, int mes, IContextoEmpresa c, OperacionesBodega o, CancellationToken ct) =>
                ConEmpresa(c, async e => Results.Ok(await o.DeclaracionAsync(e, anio, mes, ct).ConfigureAwait(false))))
            .WithSummary("Existencias y movimientos del mes por producto y calificación (base de la declaración mensual, INFOVI).").RequierePermiso(Permisos.BodegaLeer);
        return rutas;
    }

    private static async Task<IResult> ConEmpresa(IContextoEmpresa contexto, Func<Guid, Task<IResult>> accion) =>
        contexto.EmpresaId is { } empresa
            ? await accion(empresa).ConfigureAwait(false)
            : ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static IResult Creado<T>(Resultado<T> r, Func<T, string> ruta) =>
        r.EsCorrecto ? Results.Created($"/bodega/{ruta(r.Valor)}", r.Valor) : ResultadosHttp.AProblema(r.Error);

    private static IResult NoEncontrado(string que) => ResultadosHttp.AProblema(Error.NoEncontrado($"{que}.no_encontrado", "No existe."));
}
