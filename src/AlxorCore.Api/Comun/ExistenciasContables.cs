using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Inventario.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Las existencias del inventario a una fecha, con la familia de cada artículo, para la regularización contable.</summary>
public sealed class ValoracionExistenciasInventario : IValoracionExistencias
{
    private readonly IInformeValoracion _valoracion;
    private readonly IConsultaProductos _productos;

    public ValoracionExistenciasInventario(IInformeValoracion valoracion, IConsultaProductos productos)
    {
        _valoracion = valoracion; _productos = productos;
    }

    public async Task<IReadOnlyList<ExistenciaValorada>> ValorarAsync(Guid empresaId, DateOnly fecha, CancellationToken ct = default)
    {
        var v = await _valoracion.AFechaAsync(empresaId, fecha, ct).ConfigureAwait(false);
        var lista = new List<ExistenciaValorada>(v.Lineas.Count);
        foreach (var l in v.Lineas)
        {
            var p = await _productos.ObtenerAsync(l.ProductoId, ct).ConfigureAwait(false);
            lista.Add(new ExistenciaValorada(l.ProductoId, l.Nombre, p?.Familia, l.Cantidad, l.CosteUnitario, l.Valor));
        }

        return lista;
    }
}
