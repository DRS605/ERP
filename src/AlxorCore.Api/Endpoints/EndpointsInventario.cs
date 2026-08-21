using AlxorCore.Api.Comun;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Inventario (multi-almacén, ubicaciones, movimientos).</summary>
public static class EndpointsInventario
{
    public static IEndpointRouteBuilder MapearInventario(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);
        var g = rutas.MapGroup("/inventario").WithTags("Inventario");

        g.MapGet("/almacenes", ListarAlmacenesAsync).WithSummary("Lista los almacenes.").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/almacenes", CrearAlmacenAsync).WithSummary("Crea un almacén.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapGet("/ubicaciones", ListarUbicacionesAsync).WithSummary("Lista las ubicaciones (opcional por almacén).").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/ubicaciones", CrearUbicacionAsync).WithSummary("Crea una ubicación en un almacén.").RequierePermiso(Permisos.InventarioGestionar);

        g.MapGet("/stock/producto/{productoId:guid}", StockProductoAsync).WithSummary("Existencias de un artículo por almacén/ubicación.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/stock/almacen/{almacenId:guid}", StockAlmacenAsync).WithSummary("Existencias de un almacén.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/movimientos/producto/{productoId:guid}", MovimientosAsync).WithSummary("Movimientos (trazabilidad) de un artículo.").RequierePermiso(Permisos.InventarioLeer);

        g.MapPost("/entrada", EntradaAsync).WithSummary("Registra una entrada de stock.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/salida", SalidaAsync).WithSummary("Registra una salida de stock.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/ajuste", AjusteAsync).WithSummary("Ajusta el stock por recuento.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/traspaso", TraspasoAsync).WithSummary("Traspasa stock entre almacenes/ubicaciones.").RequierePermiso(Permisos.InventarioGestionar);

        g.MapGet("/ubicacion-defecto/producto/{productoId:guid}", ListarUbiDefAsync).WithSummary("Ubicaciones por defecto de un artículo.").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/ubicacion-defecto", FijarUbiDefAsync).WithSummary("Fija la ubicación por defecto (por almacén o por proveedor+almacén).").RequierePermiso(Permisos.InventarioGestionar);

        return rutas;
    }

    private static IResult SinEmpresa() => ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));

    private static async Task<IResult> ListarAlmacenesAsync(IContextoEmpresa c, GestionAlmacenes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAlmacenesAsync(c.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> CrearAlmacenAsync(CrearAlmacenComando cmd, IContextoEmpresa c, GestionAlmacenes caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.CrearAlmacenAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/inventario/almacenes") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ListarUbicacionesAsync(Guid? almacenId, IContextoEmpresa c, GestionAlmacenes caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarUbicacionesAsync(c.EmpresaId.Value, almacenId, ct).ConfigureAwait(false));

    private static async Task<IResult> CrearUbicacionAsync(CrearUbicacionComando cmd, IContextoEmpresa c, GestionAlmacenes caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.CrearUbicacionAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado("/inventario/ubicaciones") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> StockProductoAsync(Guid productoId, IContextoEmpresa c, ConsultasInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.StockDeProductoAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false));

    private static async Task<IResult> StockAlmacenAsync(Guid almacenId, IContextoEmpresa c, ConsultasInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.StockDeAlmacenAsync(c.EmpresaId.Value, almacenId, ct).ConfigureAwait(false));

    private static async Task<IResult> MovimientosAsync(Guid productoId, IContextoEmpresa c, ConsultasInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.MovimientosDeProductoAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false));

    private static async Task<IResult> EntradaAsync(MovimientoComando cmd, IContextoEmpresa c, MovimientosInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.EntradaAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> SalidaAsync(MovimientoComando cmd, IContextoEmpresa c, MovimientosInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.SalidaAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> AjusteAsync(MovimientoComando cmd, IContextoEmpresa c, MovimientosInventario caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.AjustarAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> TraspasoAsync(TraspasoComando cmd, IContextoEmpresa c, MovimientosInventario caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.TraspasarAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok() : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ListarUbiDefAsync(Guid productoId, IContextoEmpresa c, UbicacionesPorDefecto caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false));

    private static async Task<IResult> FijarUbiDefAsync(UbicacionDefectoComando cmd, IContextoEmpresa c, UbicacionesPorDefecto caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.FijarAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok() : ResultadosHttp.AProblema(r.Error);
    }
}
