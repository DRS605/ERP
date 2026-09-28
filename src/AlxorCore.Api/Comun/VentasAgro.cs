using System.Globalization;
using AlxorCore.Agro.Aplicacion;
using AlxorCore.Analisis.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Ventas de un artículo para la liquidación a resultas: kilos e importe a precio de venta (con su descuento, sin
/// conceptos) de los albaranes valorados no anulados y de las facturas vivas sin albarán, en las fechas. SQL de solo
/// lectura, limitado a la empresa activa.
/// </summary>
public sealed class VentasAgro : IVentasAgro
{
    private const string Sql = """
        WITH v AS (
            SELECT l.cantidad, l.cantidad * l.precio_unitario * (1 - coalesce(l.descuento, 0) / 100) AS importe
            FROM facturacion.linea_factura l JOIN facturacion.factura f ON f.id = l.factura_id
            WHERE f.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid
              AND f.estado NOT IN ('Anulada', 'Rectificada') AND l.albaran_venta_id IS NULL AND l.producto_id = @producto
              AND f.fecha_emision BETWEEN @desde AND @hasta AND f.total > 0
            UNION ALL
            SELECT l.cantidad, l.cantidad * l.precio_unitario * (1 - coalesce(l.porcentaje_descuento, 0) / 100)
            FROM facturacion.linea_albaran_venta l JOIN facturacion.albaran_venta a ON a.id = l.albaran_venta_id
            WHERE a.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid
              AND a.anulado_en IS NULL AND l.precio_fijado AND l.producto_id = @producto AND a.fecha BETWEEN @desde AND @hasta)
        SELECT coalesce(sum(cantidad), 0), coalesce(sum(importe), 0) FROM v
        """;

    private readonly IEjecutorAnalisis _ejecutor;

    public VentasAgro(IEjecutorAnalisis ejecutor) => _ejecutor = ejecutor;

    public async Task<(decimal Kilos, decimal Importe)> VentasAsync(Guid productoId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var filas = await _ejecutor.EjecutarAsync(new SentenciaSql(Sql, [new("producto", productoId), new("desde", desde), new("hasta", hasta)]), ct).ConfigureAwait(false);
        var f = filas.Single();
        return (Convert.ToDecimal(f[0], CultureInfo.InvariantCulture), Convert.ToDecimal(f[1], CultureInfo.InvariantCulture));
    }
}
