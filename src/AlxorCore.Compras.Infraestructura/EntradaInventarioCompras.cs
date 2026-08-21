using AlxorCore.Compras.Aplicacion;
using AlxorCore.Inventario.Aplicacion;

namespace AlxorCore.Compras.Infraestructura;

/// <summary>
/// Adaptador de <see cref="IEntradaInventarioCompras"/> sobre el módulo Inventario: al recibir un
/// albarán, registra la entrada de la mercancía en el almacén indicado, resolviendo la ubicación
/// por defecto por proveedor+almacén (o, si no hay, la general del almacén).
/// </summary>
internal sealed class EntradaInventarioCompras : IEntradaInventarioCompras
{
    private readonly MovimientosInventario _movimientos;
    private readonly UbicacionesPorDefecto _ubicaciones;

    public EntradaInventarioCompras(MovimientosInventario movimientos, UbicacionesPorDefecto ubicaciones)
    {
        _movimientos = movimientos;
        _ubicaciones = ubicaciones;
    }

    public async Task RegistrarEntradaAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId,
        decimal cantidad, string? referencia, DateOnly fecha, CancellationToken ct = default)
    {
        var ubicacionId = await _ubicaciones.ResolverAsync(empresaId, productoId, almacenId, proveedorId, ct).ConfigureAwait(false);
        await _movimientos.EntradaAsync(empresaId,
            new MovimientoComando(productoId, almacenId, cantidad, ubicacionId, fecha, "Recepción de compra", referencia), ct)
            .ConfigureAwait(false);
    }
}
