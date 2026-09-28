using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Anticipo facturado que se quiere descontar en una factura (sin base: todo lo que quede).</summary>
public sealed record DescuentoAnticipoSolicitado(Guid AnticipoId, decimal? Base = null);

/// <summary>
/// Anticipos facturados en las facturas finales (lo resuelve Tesorería): las líneas negativas que los descuentan
/// (base e impuesto del anticipo, a la cuenta 438), su anotación al emitir y lo que pasa al anular una factura.
/// </summary>
public interface IAnticiposFactura
{
    /// <summary>Líneas de descuento de los anticipos pedidos, sin pasar en total de <paramref name="baseMaxima"/>.</summary>
    Task<Resultado<IReadOnlyList<LineaComando>>> LineasDescuentoAsync(Guid clienteId, IReadOnlyList<DescuentoAnticipoSolicitado> solicitados, decimal baseMaxima, CancellationToken ct = default);

    /// <summary>La factura se ha emitido: anota en cada anticipo lo que descuenta.</summary>
    Task<Resultado> AnotarAsync(Factura factura, CancellationToken ct = default);

    /// <summary>Error si la factura no se puede anular por sus anticipos (p. ej. la de un anticipo ya descontado).</summary>
    Task<Error?> ComprobarAnulacionAsync(Factura factura, CancellationToken ct = default);

    /// <summary>La factura se ha anulado: sus anticipos descontados vuelven a quedar disponibles, y si era la de un anticipo, este se anula.</summary>
    Task FacturaAnuladaAsync(Factura factura, CancellationToken ct = default);
}
