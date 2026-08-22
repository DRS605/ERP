using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.Puertos;

/// <summary>Repositorio de empresas (tenants).</summary>
public interface IRepositorioEmpresas
{
    Task<Empresa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteNifAsync(string nif, CancellationToken ct = default);

    void Agregar(Empresa empresa);
}

/// <summary>Repositorio de membresías.</summary>
public interface IRepositorioMembresias
{
    Task<Membresia?> ObtenerAsync(Guid usuarioId, Guid empresaId, CancellationToken ct = default);

    void Agregar(Membresia membresia);

    /// <summary>Membresías (activas y revocadas) de una empresa.</summary>
    Task<IReadOnlyList<Membresia>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Repositorio de series de numeración.</summary>
public interface IRepositorioSeries
{
    void Agregar(SerieNumeracion serie);

    Task<IReadOnlyList<SerieNumeracion>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<bool> ExisteAsync(Guid empresaId, TipoDocumento tipo, int ejercicio, string prefijo, CancellationToken ct = default);
}

/// <summary>Repositorio de asignaciones de serie (empresa/cliente/proveedor por tipo de documento).</summary>
public interface IRepositorioAsignacionesSerie
{
    void Agregar(AsignacionSerie asignacion);

    Task<AsignacionSerie?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Eliminar(AsignacionSerie asignacion);

    Task<IReadOnlyList<AsignacionSerie>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<bool> ExisteAsync(Guid empresaId, TipoDocumento tipo, AmbitoSerie ambito, Guid terceroId, CancellationToken ct = default);
}

/// <summary>
/// Resuelve qué serie (prefijo) usar al emitir un documento: la del tercero si tiene una asignada,
/// si no la serie por defecto de la empresa; null si no hay ninguna asignación. La consumen otros
/// módulos (Facturación, Compras) al numerar.
/// </summary>
public interface IResolverSerie
{
    Task<string?> ResolverPrefijoAsync(Guid empresaId, TipoDocumento tipoDocumento, Guid? terceroId, CancellationToken ct = default);
}
