using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Inventario.Infraestructura;

/// <summary>
/// Una sola existencia por artículo. Cuando la empresa trabaja con almacenes, la verdad es el inventario por almacén y
/// la existencia de la ficha del artículo (Catálogo) refleja su total: se actualiza tras cada movimiento de almacén.
/// </summary>
internal sealed class ReflejoExistenciasCatalogo : IAvisoExistencias
{
    private readonly InventarioDbContext _inventario;
    private readonly IRepositorioExistenciasSimples _simples;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IUnidadDeTrabajoCatalogo _unidad;
    private readonly IReloj _reloj;

    public ReflejoExistenciasCatalogo(InventarioDbContext inventario, IRepositorioExistenciasSimples simples, IRepositorioMovimientosStock movimientos, IUnidadDeTrabajoCatalogo unidad, IReloj reloj)
    {
        _inventario = inventario;
        _simples = simples;
        _movimientos = movimientos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task CambiaronAsync(Guid empresaId, IReadOnlyCollection<Guid> productos, CancellationToken ct = default)
    {
        var cambio = false;
        foreach (var productoId in productos.Distinct())
        {
            var total = await _inventario.Existencias.Where(e => e.EmpresaId == empresaId && e.ProductoId == productoId).SumAsync(e => e.Cantidad, ct).ConfigureAwait(false);
            var simple = await _simples.ObtenerPorProductoAsync(productoId, ct).ConfigureAwait(false);
            if (simple is null)
            {
                simple = ExistenciaSimple.Crear(empresaId, productoId, _reloj);
                _simples.Agregar(simple);
            }

            if (simple.Reflejar(total, "Existencias de los almacenes", _reloj) is { } movimiento)
            {
                _movimientos.Agregar(movimiento);
                cambio = true;
            }
        }

        if (cambio)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
    }
}

/// <summary>
/// Salida de existencias al vender: si la empresa trabaja con almacenes, sale del inventario (almacén principal primero,
/// por lotes); si no, de la existencia simple del artículo, como siempre.
/// </summary>
internal sealed class StockVentasPorAlmacen : IStockVentas
{
    private readonly StockVentas _catalogo;
    private readonly MovimientosInventario _movimientos;
    private readonly IReloj _reloj;

    public StockVentasPorAlmacen(StockVentas catalogo, MovimientosInventario movimientos, IReloj reloj)
    {
        _catalogo = catalogo;
        _movimientos = movimientos;
        _reloj = reloj;
    }

    public async Task DescontarVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, CancellationToken ct = default)
    {
        if ((await _movimientos.AlmacenesActivosAsync(empresaId, ct).ConfigureAwait(false)).Count == 0)
        {
            await _catalogo.DescontarVentaAsync(empresaId, lineas, ct).ConfigureAwait(false);
            return;
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var (productoId, (cantidad, motivo)) in await _catalogo.SalidasAsync(lineas, ct).ConfigureAwait(false))
        {
            await _movimientos.SalidaVentaAsync(empresaId, productoId, cantidad, motivo, null, hoy, ct).ConfigureAwait(false);
        }
    }

    public async Task DevolverVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, string motivo, CancellationToken ct = default)
    {
        var almacenes = await _movimientos.AlmacenesActivosAsync(empresaId, ct).ConfigureAwait(false);
        if (almacenes.Count == 0)
        {
            await _catalogo.DevolverVentaAsync(empresaId, lineas, motivo, ct).ConfigureAwait(false);
            return;
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var (productoId, (cantidad, _)) in await _catalogo.SalidasAsync(lineas, ct).ConfigureAwait(false))
        {
            await _movimientos.EntradaAsync(empresaId, new MovimientoComando(productoId, almacenes[0].Id, cantidad, Motivo: motivo, Fecha: hoy), ct).ConfigureAwait(false);
        }
    }
}

/// <summary>Movimientos hechos desde la ficha del artículo cuando la empresa trabaja con almacenes: van al almacén principal.</summary>
internal sealed class ExistenciasAlmacenCatalogo : IExistenciasAlmacen
{
    private readonly MovimientosInventario _movimientos;
    private readonly InventarioDbContext _inventario;

    public ExistenciasAlmacenCatalogo(MovimientosInventario movimientos, InventarioDbContext inventario)
    {
        _movimientos = movimientos;
        _inventario = inventario;
    }

    public async Task<bool> TrabajaConAlmacenesAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _movimientos.AlmacenesActivosAsync(empresaId, ct).ConfigureAwait(false)).Count > 0;

    public async Task<Resultado> MovimientoAsync(Guid empresaId, Guid productoId, TipoMovimientoStock tipo, decimal cantidad, string? motivo, CancellationToken ct = default)
    {
        var almacenes = await _movimientos.AlmacenesActivosAsync(empresaId, ct).ConfigureAwait(false);
        if (almacenes.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("inventario.sin_almacen", "La empresa no tiene almacenes."));
        }

        var principal = almacenes[0].Id;
        var comando = new MovimientoComando(productoId, principal, cantidad, Motivo: motivo ?? "Ficha del artículo");
        switch (tipo)
        {
            case TipoMovimientoStock.Entrada:
                return Quitar(await _movimientos.EntradaAsync(empresaId, comando, ct).ConfigureAwait(false));
            case TipoMovimientoStock.Salida:
            case TipoMovimientoStock.Venta:
                return Quitar(await _movimientos.SalidaAsync(empresaId, comando, ct).ConfigureAwait(false));
            default:
                // Recuento: solo si todo el artículo está en el almacén principal, sin ubicación ni lote.
                var repartido = await _inventario.Existencias.AnyAsync(e => e.EmpresaId == empresaId && e.ProductoId == productoId && e.Cantidad != 0m
                    && (e.AlmacenId != principal || e.UbicacionId != null || e.Lote != null), ct).ConfigureAwait(false);
                if (repartido)
                {
                    return Resultado.Fallo(Error.Conflicto("inventario.recuento_por_almacen",
                        "El artículo está en varios almacenes, ubicaciones o lotes: haz el recuento en Inventario, almacén por almacén."));
                }

                return Quitar(await _movimientos.AjustarAsync(empresaId, comando, ct).ConfigureAwait(false));
        }
    }

    private static Resultado Quitar<T>(Resultado<T> r) => r.EsCorrecto ? Resultado.Ok() : Resultado.Fallo(r.Error);
}
