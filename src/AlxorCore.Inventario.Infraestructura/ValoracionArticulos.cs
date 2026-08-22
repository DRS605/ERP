using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Inventario.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Inventario.Infraestructura;

/// <summary>
/// Valoración de artículos según el método de la empresa (estándar, última compra, PMP o FIFO).
/// - Estándar: coste de la ficha del artículo (Catálogo).
/// - Última compra: coste de la última entrada con coste.
/// - PMP: media ponderada de las entradas con coste.
/// - FIFO: coste de las capas de entrada más antiguas que cubren las existencias actuales.
/// Si no hay datos de coste de entradas, cae al coste estándar del artículo.
/// </summary>
internal sealed class ValoracionArticulos : IValoracionArticulos, IInformeValoracion
{
    private readonly InventarioDbContext _ctx;
    private readonly IConsultaProductos _productos;
    private readonly IConfiguracionEmpresa _configuracion;

    public ValoracionArticulos(InventarioDbContext ctx, IConsultaProductos productos, IConfiguracionEmpresa configuracion)
    {
        _ctx = ctx; _productos = productos; _configuracion = configuracion;
    }

    public async Task<decimal> ValorUnitarioAsync(Guid empresaId, Guid productoId, MetodoValoracion? metodo = null, CancellationToken ct = default)
    {
        var m = metodo ?? await _configuracion.MetodoValoracionAsync(empresaId, ct).ConfigureAwait(false);
        var estandar = await CosteEstandarAsync(productoId, ct).ConfigureAwait(false);

        if (m == MetodoValoracion.Estandar)
        {
            return estandar;
        }

        var entradas = await _ctx.Movimientos.AsNoTracking()
            .Where(x => x.EmpresaId == empresaId && x.ProductoId == productoId && x.Tipo == TipoMovimientoInventario.Entrada && x.CosteUnitario != null && x.Cantidad > 0m)
            .OrderBy(x => x.Fecha).ThenBy(x => x.CreadoEn)
            .Select(x => new { x.Cantidad, Coste = x.CosteUnitario!.Value })
            .ToListAsync(ct).ConfigureAwait(false);

        if (entradas.Count == 0)
        {
            return estandar;
        }

        switch (m)
        {
            case MetodoValoracion.UltimaCompra:
                return entradas[^1].Coste;

            case MetodoValoracion.Pmp:
            {
                var cant = entradas.Sum(e => e.Cantidad);
                if (cant <= 0m)
                {
                    return estandar;
                }

                return Redondeo.Dos(entradas.Sum(e => e.Cantidad * e.Coste) / cant);
            }

            case MetodoValoracion.Fifo:
            {
                // Existencias actuales del artículo (todas las ubicaciones/lotes).
                var onHand = await _ctx.Existencias.AsNoTracking()
                    .Where(e => e.EmpresaId == empresaId && e.ProductoId == productoId)
                    .SumAsync(e => (decimal?)e.Cantidad, ct).ConfigureAwait(false) ?? 0m;
                if (onHand <= 0m)
                {
                    return entradas[^1].Coste; // sin stock: referencia = última compra
                }

                // Las existencias actuales corresponden a las capas de entrada más recientes (FIFO:
                // lo antiguo ya salió). El coste unitario FIFO del stock es el de la capa más antigua
                // que aún queda: recorremos desde la más reciente hacia atrás acumulando `onHand`.
                var restante = onHand;
                var costeCapa = entradas[^1].Coste;
                for (var i = entradas.Count - 1; i >= 0 && restante > 0m; i--)
                {
                    costeCapa = entradas[i].Coste;
                    restante -= entradas[i].Cantidad;
                }

                return costeCapa;
            }

            default:
                return estandar;
        }
    }

    public async Task<ValoracionInventarioDto> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var metodo = await _configuracion.MetodoValoracionAsync(empresaId, ct).ConfigureAwait(false);

        // Existencias por artículo (sumando ubicaciones y lotes).
        var stock = await _ctx.Existencias.AsNoTracking()
            .Where(e => e.EmpresaId == empresaId)
            .GroupBy(e => e.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .Where(x => x.Cantidad != 0m)
            .ToListAsync(ct).ConfigureAwait(false);

        var lineas = new List<ValoracionLineaDto>();
        decimal total = 0m;
        foreach (var s in stock)
        {
            var coste = await ValorUnitarioAsync(empresaId, s.ProductoId, metodo, ct).ConfigureAwait(false);
            var prod = await _productos.ObtenerAsync(s.ProductoId, ct).ConfigureAwait(false);
            var valor = Redondeo.Dos(coste * s.Cantidad);
            total += valor;
            lineas.Add(new ValoracionLineaDto(s.ProductoId, prod?.Nombre ?? "(artículo)", s.Cantidad, coste, valor));
        }

        return new ValoracionInventarioDto(metodo.ToString(), Redondeo.Dos(total), lineas.OrderByDescending(l => l.Valor).ToList());
    }

    private async Task<decimal> CosteEstandarAsync(Guid productoId, CancellationToken ct)
    {
        var prod = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
        return prod?.PrecioCompra ?? 0m;
    }
}
