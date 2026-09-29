using AlxorCore.Agro.Aplicacion;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador de <see cref="IInventarioAgro"/> sobre el movimiento de stock de la ficha del artículo: va a la existencia
/// del artículo o, si la empresa trabaja con almacenes, a su almacén principal. Los artículos sin control de stock se
/// ignoran; cualquier otro fallo vuelve como aviso.
/// </summary>
public sealed class InventarioAgro : IInventarioAgro
{
    private readonly RegistrarMovimientoStock _movimiento;
    private readonly IConsultaProductos _productos;
    private readonly AlxorCore.Inventario.Aplicacion.MovimientosInventario? _almacen;
    private readonly AlxorCore.Inventario.Aplicacion.LotesArticulos? _lotes;

    public InventarioAgro(RegistrarMovimientoStock movimiento, IConsultaProductos productos, AlxorCore.Inventario.Aplicacion.MovimientosInventario? almacen = null,
        AlxorCore.Inventario.Aplicacion.LotesArticulos? lotes = null)
    {
        _movimiento = movimiento;
        _productos = productos;
        _almacen = almacen;
        _lotes = lotes;
    }

    public async Task<AlxorCore.Nucleo.Resultados.Resultado> ConsumirAsync(Guid empresaId, Guid productoId, Guid almacenId, string? lote, decimal cantidad, DateOnly fecha,
        string referencia, CancellationToken ct = default)
    {
        if (_almacen is null)
        {
            return AlxorCore.Nucleo.Resultados.Resultado.Fallo(AlxorCore.Nucleo.Resultados.Error.Validacion("inventario.sin_almacenes", "La empresa no trabaja con almacenes."));
        }

        var r = await _almacen.SalidaAsync(empresaId, new AlxorCore.Inventario.Aplicacion.MovimientoComando(productoId, almacenId, cantidad, Fecha: fecha, Motivo: "Tratamiento",
            Referencia: referencia, Lote: string.IsNullOrWhiteSpace(lote) ? null : lote.Trim()), ct).ConfigureAwait(false);
        return r.EsFallo ? AlxorCore.Nucleo.Resultados.Resultado.Fallo(r.Error) : AlxorCore.Nucleo.Resultados.Resultado.Ok();
    }

    public async Task<AlxorCore.Nucleo.Resultados.Resultado> DevolverAsync(Guid empresaId, Guid productoId, Guid almacenId, string? lote, decimal cantidad, DateOnly fecha,
        string referencia, CancellationToken ct = default)
    {
        if (_almacen is null)
        {
            return AlxorCore.Nucleo.Resultados.Resultado.Ok();
        }

        var r = await _almacen.EntradaAsync(empresaId, new AlxorCore.Inventario.Aplicacion.MovimientoComando(productoId, almacenId, cantidad, Fecha: fecha, Motivo: "Tratamiento anulado",
            Referencia: referencia, Lote: string.IsNullOrWhiteSpace(lote) ? null : lote.Trim()), ct).ConfigureAwait(false);
        return r.EsFallo ? AlxorCore.Nucleo.Resultados.Resultado.Fallo(r.Error) : AlxorCore.Nucleo.Resultados.Resultado.Ok();
    }

    public async Task<DateOnly?> CaducidadLoteAsync(Guid empresaId, Guid productoId, string lote, CancellationToken ct = default) =>
        _lotes is null ? null : (await _lotes.ObtenerAsync(empresaId, productoId, lote, ct).ConfigureAwait(false))?.FechaCaducidad;

    public async Task<IReadOnlyList<string>> MoverAsync(Guid empresaId, IReadOnlyList<MovimientoInventarioAgro> movimientos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(movimientos);
        var avisos = new List<string>();
        foreach (var m in movimientos.Where(m => m.Cantidad != 0m))
        {
            var r = await _movimiento.EjecutarAsync(empresaId, m.ProductoId,
                new DatosMovimientoStock(m.Cantidad > 0m ? TipoMovimientoStock.Entrada : TipoMovimientoStock.Salida, Math.Abs(m.Cantidad), m.Motivo), ct).ConfigureAwait(false);
            if (r.EsFallo && r.Error.Codigo != "producto.sin_control_stock")
            {
                var nombre = (await _productos.ObtenerAsync(m.ProductoId, ct).ConfigureAwait(false))?.Nombre ?? "un artículo";
                avisos.Add($"Inventario de {nombre}: {r.Error.Mensaje}");
            }
        }

        return avisos;
    }
}

/// <summary>Existencias de un artículo en todos los almacenes (para los avisos de fitosanitarios retirados).</summary>
public sealed class ExistenciasFito : AlxorCore.Agro.Aplicacion.IExistenciasFito
{
    private readonly AlxorCore.Inventario.Aplicacion.ConsultasInventario _consultas;

    public ExistenciasFito(AlxorCore.Inventario.Aplicacion.ConsultasInventario consultas) => _consultas = consultas;

    public async Task<decimal> ExistenciasAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        (await _consultas.StockDeProductoAsync(empresaId, productoId, ct).ConfigureAwait(false)).Sum(e => e.Cantidad);
}
