using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>
/// Eliminar o dar de baja artículos y tarifas. Un artículo solo se elimina si no se ha usado en ninguna empresa del
/// grupo (con él se borran sus variantes de atributos y su composición); si ya se usó, se da de baja. Una tarifa se
/// elimina si ningún cliente la tiene asignada.
/// </summary>
public sealed class BajasCatalogo
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioTarifas _tarifas;
    private readonly IComprobadorUso _uso;
    private readonly IUnidadDeTrabajoCatalogo _unidad;
    private readonly IReloj _reloj;

    public BajasCatalogo(IRepositorioProductos productos, IRepositorioTarifas tarifas, IComprobadorUso uso, IUnidadDeTrabajoCatalogo unidad, IReloj reloj)
    {
        _productos = productos;
        _tarifas = tarifas;
        _uso = uso;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<BajaDto>> EliminarProductoAsync(Guid id, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        var uso = await _uso.BuscarUsoAsync(TiposRegistro.Producto, id, ct).ConfigureAwait(false);
        if (uso is not null)
        {
            return Resultado.Fallo<BajaDto>(Bajas.EnUso("producto", "el artículo", uso));
        }

        _productos.Eliminar(producto);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    public async Task<Resultado<BajaDto>> CambiarEstadoProductoAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        if (activo)
        {
            producto.Reactivar(_reloj);
        }
        else
        {
            producto.Desactivar(_reloj);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, false, producto.Activo));
    }

    public async Task<Resultado<BajaDto>> EliminarTarifaAsync(Guid id, CancellationToken ct = default)
    {
        var tarifa = await _tarifas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (tarifa is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("tarifa.no_encontrada", "La tarifa no existe."));
        }

        var uso = await _uso.BuscarUsoAsync(TiposRegistro.Tarifa, id, ct).ConfigureAwait(false);
        if (uso is not null)
        {
            return Resultado.Fallo<BajaDto>(Error.Conflicto("tarifa" + Bajas.SufijoEnUso,
                $"No se puede eliminar la tarifa porque hay {uso}. Quítasela primero."));
        }

        _tarifas.Eliminar(tarifa);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }
}
