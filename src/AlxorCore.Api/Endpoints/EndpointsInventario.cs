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
        g.MapPut("/almacenes/{id:guid}", async (Guid id, CrearAlmacenComando cmd, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.ActualizarAlmacenAsync(id, cmd, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica el código y el nombre de un almacén.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapDelete("/almacenes/{id:guid}", async (Guid id, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.EliminarAlmacenAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina un almacén sin existencias ni movimientos (409 «almacen.en_uso»: darlo de baja).").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/almacenes/{id:guid}/baja", async (Guid id, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.CambiarEstadoAlmacenAsync(id, false, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Da de baja un almacén.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/almacenes/{id:guid}/alta", async (Guid id, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.CambiarEstadoAlmacenAsync(id, true, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Reactiva un almacén.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPut("/ubicaciones/{id:guid}", async (Guid id, CrearUbicacionComando cmd, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.ActualizarUbicacionAsync(id, cmd, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Modifica el código y el nombre de una ubicación.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapDelete("/ubicaciones/{id:guid}", async (Guid id, GestionAlmacenes caso, CancellationToken ct) =>
                (await caso.EliminarUbicacionAsync(id, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Elimina una ubicación sin existencias ni movimientos.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapGet("/ubicaciones", ListarUbicacionesAsync).WithSummary("Lista las ubicaciones (opcional por almacén).").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/ubicaciones", CrearUbicacionAsync).WithSummary("Crea una ubicación en un almacén.").RequierePermiso(Permisos.InventarioGestionar);

        g.MapGet("/stock/producto/{productoId:guid}", StockProductoAsync).WithSummary("Existencias de un artículo por almacén/ubicación.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/stock/almacen/{almacenId:guid}", StockAlmacenAsync).WithSummary("Existencias de un almacén.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/movimientos/producto/{productoId:guid}", MovimientosAsync).WithSummary("Movimientos (trazabilidad) de un artículo.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/trazabilidad/{productoId:guid}", TrazabilidadAsync).WithSummary("Trazabilidad de un lote o nº de serie: existencias e historial.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/valoracion", ValoracionAsync).WithSummary("Valoración de existencias con el método de la empresa (estándar/última compra/PMP/FIFO).").RequierePermiso(Permisos.InventarioLeer);

        g.MapGet("/lotes", async (Guid? productoId, IContextoEmpresa c, LotesArticulos caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false)))
            .WithSummary("Lotes de los artículos con su caducidad y existencias.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/lotes/caducan", async (int? dias, IContextoEmpresa c, LotesArticulos caso, AlxorCore.Nucleo.Tiempo.IReloj reloj, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.CaducanAsync(c.EmpresaId.Value,
                    DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime).AddDays(dias ?? 30), ct).ConfigureAwait(false)))
            .WithSummary("Lotes con existencias que caducan en los próximos días (30 por defecto), incluidos los caducados.").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/lotes", async (DatosLote d, IContextoEmpresa c, LotesArticulos caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.FijarAsync(c.EmpresaId.Value, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Da de alta un lote de un artículo o cambia sus fechas (caducidad, fabricación).").RequierePermiso(Permisos.InventarioGestionar);
        g.MapDelete("/lotes/{id:guid}", async (Guid id, LotesArticulos caso, CancellationToken ct) =>
                (await caso.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita las fechas de un lote (sus existencias y movimientos siguen).").RequierePermiso(Permisos.InventarioGestionar);

        g.MapPost("/entrada", EntradaAsync).WithSummary("Registra una entrada de stock.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/salida", SalidaAsync).WithSummary("Registra una salida de stock.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/ajuste", AjusteAsync).WithSummary("Ajusta el stock por recuento.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/traspaso", TraspasoAsync).WithSummary("Traspasa stock entre almacenes/ubicaciones.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/montaje", MontajeAsync).WithSummary("Monta un artículo compuesto: consume componentes y produce el compuesto.").RequierePermiso(Permisos.InventarioGestionar);

        // Números de serie.
        g.MapPost("/series/entrada", async (EntradaSeriesComando cmd, IContextoEmpresa c, MovimientosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.EntradaSeriesAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Entrada de varias unidades con su número de serie (entran todas o ninguna).").RequierePermiso(Permisos.InventarioGestionar);
        g.MapGet("/series/{productoId:guid}", async (Guid productoId, IContextoEmpresa c, ConsultasInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa()
                    : Results.Ok((await caso.StockDeProductoAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false))
                        .Where(e => e.Cantidad > 0m && e.Lote is not null).OrderBy(e => e.Lote, StringComparer.Ordinal).ToList()))
            .WithSummary("Números de serie (o lotes) de un artículo que hay en existencias, con su almacén y ubicación.").RequierePermiso(Permisos.InventarioLeer);

        // Stock mínimo y máximo (reaprovisionamiento).
        g.MapGet("/reaprovisionamiento", async (IContextoEmpresa c, ReglasReaprovisionamiento caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false)))
            .WithSummary("Reglas de stock mínimo y máximo de los artículos.").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/reaprovisionamiento", async (DatosReglaReaprovisionamiento d, IContextoEmpresa c, ReglasReaprovisionamiento caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.FijarAsync(c.EmpresaId.Value, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Fija el mínimo, el máximo y el múltiplo de compra de un artículo (en un almacén o en todos).").RequierePermiso(Permisos.InventarioGestionar);
        g.MapDelete("/reaprovisionamiento/{id:guid}", async (Guid id, ReglasReaprovisionamiento caso, CancellationToken ct) =>
                (await caso.EliminarAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Quita una regla de reaprovisionamiento.").RequierePermiso(Permisos.InventarioGestionar);

        // Inventario físico (recuentos).
        g.MapGet("/recuentos", async (IContextoEmpresa c, RecuentosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false)))
            .WithSummary("Recuentos de inventario físico.").RequierePermiso(Permisos.InventarioLeer);
        g.MapGet("/recuentos/{id:guid}", async (Guid id, IContextoEmpresa c, RecuentosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa()
                    : await caso.ObtenerAsync(c.EmpresaId.Value, id, ct).ConfigureAwait(false) is { } r ? Results.Ok(r)
                    : ResultadosHttp.AProblema(Error.NoEncontrado("recuento.no_encontrado", "El recuento no existe.")))
            .WithSummary("Recuento con sus líneas: teórico al abrir, contado y diferencia.").RequierePermiso(Permisos.InventarioLeer);
        g.MapPost("/recuentos", async (DatosRecuento d, IContextoEmpresa c, RecuentosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.AbrirAsync(c.EmpresaId.Value, d, ct).ConfigureAwait(false)) is var r && r.EsCorrecto
                    ? Results.Created($"/inventario/recuentos/{r.Valor.Id}", r.Valor) : ResultadosHttp.AProblema(r.Error))
            .WithSummary("Abre un recuento de un almacén (o ubicación, o artículos) congelando su stock teórico.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPut("/recuentos/{id:guid}/conteos", async (Guid id, DatosConteo d, IContextoEmpresa c, RecuentosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.ContarAsync(c.EmpresaId.Value, id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Anota lo contado (lo que no estaba en el teórico se añade).").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/recuentos/{id:guid}/cerrar", async (Guid id, DatosCierreRecuento? d, IContextoEmpresa c, RecuentosInventario caso, CancellationToken ct) =>
                c.EmpresaId is null ? SinEmpresa() : (await caso.CerrarAsync(c.EmpresaId.Value, id, d, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cierra el recuento y regulariza las diferencias con ajustes.").RequierePermiso(Permisos.InventarioGestionar);
        g.MapPost("/recuentos/{id:guid}/anular", async (Guid id, RecuentosInventario caso, CancellationToken ct) =>
                (await caso.AnularAsync(id, ct).ConfigureAwait(false)).ASinContenido())
            .WithSummary("Anula un recuento abierto sin regularizar nada.").RequierePermiso(Permisos.InventarioGestionar);

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

    private static async Task<IResult> TrazabilidadAsync(Guid productoId, string lote, IContextoEmpresa c, TrazabilidadLote caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        if (string.IsNullOrWhiteSpace(lote)) return ResultadosHttp.AProblema(Error.Validacion("trazabilidad.lote_vacio", "Indica el lote o número de serie."));
        return Results.Ok(await caso.ConsultarAsync(c.EmpresaId.Value, productoId, lote.Trim(), ct).ConfigureAwait(false));
    }

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

    private static async Task<IResult> ValoracionAsync(IContextoEmpresa c, IInformeValoracion caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.EjecutarAsync(c.EmpresaId.Value, ct).ConfigureAwait(false));

    private static async Task<IResult> MontajeAsync(MontajeComando cmd, IContextoEmpresa c, MontajeArticulo caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : (await caso.EjecutarAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false)).AOk();

    private static async Task<IResult> ListarUbiDefAsync(Guid productoId, IContextoEmpresa c, UbicacionesPorDefecto caso, CancellationToken ct)
        => c.EmpresaId is null ? SinEmpresa() : Results.Ok(await caso.ListarAsync(c.EmpresaId.Value, productoId, ct).ConfigureAwait(false));

    private static async Task<IResult> FijarUbiDefAsync(UbicacionDefectoComando cmd, IContextoEmpresa c, UbicacionesPorDefecto caso, CancellationToken ct)
    {
        if (c.EmpresaId is null) return SinEmpresa();
        var r = await caso.FijarAsync(c.EmpresaId.Value, cmd, ct).ConfigureAwait(false);
        return r.EsCorrecto ? Results.Ok() : ResultadosHttp.AProblema(r.Error);
    }
}
