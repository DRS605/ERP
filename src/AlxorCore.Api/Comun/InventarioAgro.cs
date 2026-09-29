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

    public InventarioAgro(RegistrarMovimientoStock movimiento, IConsultaProductos productos)
    {
        _movimiento = movimiento;
        _productos = productos;
    }

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
