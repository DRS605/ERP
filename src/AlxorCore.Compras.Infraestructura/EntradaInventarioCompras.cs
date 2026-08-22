using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Inventario.Aplicacion;

namespace AlxorCore.Compras.Infraestructura;

/// <summary>
/// Adaptador de <see cref="IEntradaInventarioCompras"/> sobre el módulo Inventario: al recibir un
/// albarán, registra la entrada de la mercancía en el almacén indicado, resolviendo la ubicación
/// por defecto por proveedor+almacén (o, si no hay, la general del almacén).
///
/// La cantidad recibida llega en la <b>unidad de compra</b> del pedido (p. ej. cajas); aquí se
/// convierte a <b>unidades base</b> con el factor de compra del artículo (Catálogo), porque el stock
/// —y, a futuro, el consumo de producción— siempre se lleva en la unidad base canónica.
/// </summary>
internal sealed class EntradaInventarioCompras : IEntradaInventarioCompras
{
    private readonly MovimientosInventario _movimientos;
    private readonly UbicacionesPorDefecto _ubicaciones;
    private readonly IConsultaProductos _productos;

    public EntradaInventarioCompras(MovimientosInventario movimientos, UbicacionesPorDefecto ubicaciones, IConsultaProductos productos)
    {
        _movimientos = movimientos;
        _ubicaciones = ubicaciones;
        _productos = productos;
    }

    public async Task RegistrarEntradaAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId,
        decimal cantidad, string? referencia, DateOnly fecha, string? lote, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
        var cantidadBase = (producto is not null && producto.FactorCompra > 0m)
            ? Math.Round(cantidad * producto.FactorCompra, 3, MidpointRounding.AwayFromZero)
            : cantidad;

        var ubicacionId = await _ubicaciones.ResolverAsync(empresaId, productoId, almacenId, proveedorId, ct).ConfigureAwait(false);
        await _movimientos.EntradaAsync(empresaId,
            new MovimientoComando(productoId, almacenId, cantidadBase, ubicacionId, fecha, "Recepción de compra", referencia, string.IsNullOrWhiteSpace(lote) ? null : lote.Trim()), ct)
            .ConfigureAwait(false);
    }
}
