using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Logistica.Aplicacion;
using AlxorCore.Produccion.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Existencias por lote de un artículo en un almacén (Inventario), con la caducidad de cada lote.</summary>
public sealed class ExistenciasLogisticaInventario : IExistenciasLogistica
{
    private readonly ConsultasInventario _consultas;
    private readonly GestionAlmacenes _almacenes;
    private readonly LotesArticulos _lotes;

    public ExistenciasLogisticaInventario(ConsultasInventario consultas, GestionAlmacenes almacenes, LotesArticulos lotes)
    {
        _consultas = consultas;
        _almacenes = almacenes;
        _lotes = lotes;
    }

    public async Task<IReadOnlyList<ExistenciaLote>> DeProductoAsync(Guid empresaId, Guid productoId, Guid almacenId, CancellationToken ct = default)
    {
        var existencias = (await _consultas.StockDeProductoAsync(empresaId, productoId, ct).ConfigureAwait(false)).Where(e => e.AlmacenId == almacenId && e.Cantidad != 0m).ToList();
        var caducidades = (await _lotes.ListarAsync(empresaId, productoId, ct).ConfigureAwait(false)).ToDictionary(l => l.Codigo, l => l.FechaCaducidad, StringComparer.Ordinal);
        return existencias.Select(e => new ExistenciaLote(e.AlmacenId, e.Lote, e.Cantidad, e.Lote is null ? null : caducidades.GetValueOrDefault(e.Lote))).ToList();
    }

    public async Task<bool> AlmacenExisteAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default) =>
        (await _almacenes.ListarAlmacenesAsync(empresaId, ct).ConfigureAwait(false)).Any(a => a.Id == almacenId);
}

/// <summary>Nombre y unidad de los artículos del catálogo.</summary>
public sealed class ArticulosLogisticaCatalogo : IArticulosLogistica
{
    private readonly IConsultaProductos _productos;

    public ArticulosLogisticaCatalogo(IConsultaProductos productos) => _productos = productos;

    public async Task<IReadOnlyDictionary<Guid, ArticuloLogistica>> ObtenerAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(ids);
        var resultado = new Dictionary<Guid, ArticuloLogistica>();
        foreach (var id in ids.Distinct())
        {
            if (await _productos.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p)
            {
                resultado[id] = new ArticuloLogistica(p.Id, p.Nombre, p.Unidad);
            }
        }

        return resultado;
    }
}

/// <summary>Lo pendiente de servir de un pedido de venta.</summary>
public sealed class PedidosLogisticaFacturacion : IPedidosLogistica
{
    private readonly ObtenerPedidoVenta _pedidos;

    public PedidosLogisticaFacturacion(ObtenerPedidoVenta pedidos) => _pedidos = pedidos;

    public async Task<PedidoLogistica?> ObtenerAsync(Guid pedidoVentaId, CancellationToken ct = default) =>
        await _pedidos.EjecutarAsync(pedidoVentaId, ct).ConfigureAwait(false) is { } p
            ? new PedidoLogistica(p.Id, p.NumeroCompleto, p.ClienteId, p.ClienteNombre,
                p.Lineas.Where(l => l.ProductoId is not null).Select(l => new LineaPedidoLogistica(l.Id, l.ProductoId!.Value, l.Descripcion, l.PendienteServir)).ToList())
            : null;
}

/// <summary>Órdenes de fabricación (Producción): lo fabricado y su lote.</summary>
public sealed class FabricacionLogisticaProduccion : IFabricacionLogistica
{
    private readonly IRepositorioOrdenes _ordenes;

    public FabricacionLogisticaProduccion(IRepositorioOrdenes ordenes) => _ordenes = ordenes;

    public async Task<OrdenFabricacionLogistica?> ObtenerAsync(Guid ordenId, CancellationToken ct = default) =>
        await _ordenes.ObtenerDtoAsync(ordenId, ct).ConfigureAwait(false) is { } o
            ? new OrdenFabricacionLogistica(o.Id, $"OF-{o.Ejercicio}-{o.Numero:D6}", o.ProductoId, o.Cantidad, o.Estado == "Terminada", o.Lote, o.FechaCaducidad,
                o.TerminadaEn is { } t ? DateOnly.FromDateTime(t.UtcDateTime) : null)
            : null;
}
