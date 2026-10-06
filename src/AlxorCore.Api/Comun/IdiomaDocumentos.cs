using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Idioma de los documentos del cliente o proveedor (su ficha) y nombres traducidos de los artículos (Catálogo).</summary>
public sealed class IdiomaDocumentos : IIdiomaDocumentos
{
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaTraduccionesArticulos _articulos;

    public IdiomaDocumentos(IConsultaClientes clientes, IConsultaProveedores proveedores, IConsultaTraduccionesArticulos articulos)
    {
        _clientes = clientes;
        _proveedores = proveedores;
        _articulos = articulos;
    }

    public async Task<string> DeClienteAsync(Guid? clienteId, CancellationToken ct = default) =>
        IdiomasDocumento.Efectivo(clienteId is { } id && id != Guid.Empty ? (await _clientes.ObtenerAsync(id, ct).ConfigureAwait(false))?.Idioma : null);

    public async Task<string> DeProveedorAsync(Guid? proveedorId, CancellationToken ct = default) =>
        IdiomasDocumento.Efectivo(proveedorId is { } id && id != Guid.Empty ? (await _proveedores.ObtenerAsync(id, ct).ConfigureAwait(false))?.Idioma : null);

    public async Task<IReadOnlyDictionary<Guid, NombreTraducido>> ArticulosAsync(IReadOnlyCollection<Guid> productoIds, string idioma, CancellationToken ct = default) =>
        (await _articulos.NombresEnIdiomaAsync(productoIds, idioma, ct).ConfigureAwait(false))
            .ToDictionary(kv => kv.Key, kv => new NombreTraducido(kv.Value.Nombre, kv.Value.Traducido));
}
