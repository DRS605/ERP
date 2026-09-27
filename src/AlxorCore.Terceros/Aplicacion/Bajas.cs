using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Terceros.Aplicacion;

/// <summary>
/// Eliminar o dar de baja clientes y proveedores. Solo se elimina un tercero que no se ha usado en ninguna empresa
/// del grupo; uno usado se da de baja (deja de ofrecerse en las altas nuevas) y se puede reactivar.
/// </summary>
public sealed class BajasTerceros
{
    private readonly IRepositorioClientes _clientes;
    private readonly IRepositorioProveedores _proveedores;
    private readonly IComprobadorUso _uso;
    private readonly IUnidadDeTrabajoTerceros _unidad;
    private readonly IReloj _reloj;

    public BajasTerceros(IRepositorioClientes clientes, IRepositorioProveedores proveedores, IComprobadorUso uso, IUnidadDeTrabajoTerceros unidad, IReloj reloj)
    {
        _clientes = clientes;
        _proveedores = proveedores;
        _uso = uso;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<BajaDto>> EliminarClienteAsync(Guid id, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var uso = await _uso.BuscarUsoAsync(TiposRegistro.Cliente, id, ct).ConfigureAwait(false);
        if (uso is not null)
        {
            return Resultado.Fallo<BajaDto>(Bajas.EnUso("cliente", "el cliente", uso));
        }

        _clientes.Eliminar(cliente);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    public async Task<Resultado<BajaDto>> CambiarEstadoClienteAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        if (activo)
        {
            cliente.Reactivar(_reloj);
        }
        else
        {
            cliente.Desactivar(_reloj);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, false, cliente.Activo));
    }

    public async Task<Resultado<BajaDto>> EliminarProveedorAsync(Guid id, CancellationToken ct = default)
    {
        var proveedor = await _proveedores.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
        }

        var uso = await _uso.BuscarUsoAsync(TiposRegistro.Proveedor, id, ct).ConfigureAwait(false);
        if (uso is not null)
        {
            return Resultado.Fallo<BajaDto>(Bajas.EnUso("proveedor", "el proveedor", uso));
        }

        _proveedores.Eliminar(proveedor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    public async Task<Resultado<BajaDto>> CambiarEstadoProveedorAsync(Guid id, bool activo, CancellationToken ct = default)
    {
        var proveedor = await _proveedores.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
        }

        if (activo)
        {
            proveedor.Reactivar(_reloj);
        }
        else
        {
            proveedor.Desactivar(_reloj);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, false, proveedor.Activo));
    }
}
