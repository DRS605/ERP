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
        if (r.EsFallo || !r.Valor.EsCompuesto)
        {
            return null;
        }

        return r.Valor.Componentes.Select(c => (c.ComponenteId, c.Cantidad)).ToList();
    }
}
