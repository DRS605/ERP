using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Inventario.Aplicacion;

namespace AlxorCore.Inventario.Infraestructura;

/// <summary>
/// Implementa <see cref="IConsultaComposicion"/> sobre el módulo Catálogo: obtiene la lista de
/// materiales de un artículo compuesto para poder montarlo (consumir componentes y producir el
/// compuesto). Devuelve null si el artículo no es compuesto.
/// </summary>
internal sealed class ConsultaComposicionCatalogo : IConsultaComposicion
{
    private readonly ObtenerComposicion _composicion;

    public ConsultaComposicionCatalogo(ObtenerComposicion composicion) => _composicion = composicion;

    public async Task<IReadOnlyList<(Guid ComponenteId, decimal Cantidad)>?> ObtenerComponentesAsync(Guid productoId, CancellationToken ct = default)
    {
        var r = await _composicion.EjecutarAsync(productoId, ct).ConfigureAwait(false);
        // Un kit de venta no se monta: no tiene existencias propias, se descuentan sus componentes al venderlo.
        if (r.EsFallo || !r.Valor.EsCompuesto || r.Valor.TipoComposicion == "Kit")
        {
            return null;
        }

        return r.Valor.Componentes.Select(c => (c.ComponenteId, c.Cantidad)).ToList();
    }
}
