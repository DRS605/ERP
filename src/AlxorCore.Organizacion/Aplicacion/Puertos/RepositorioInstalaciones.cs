using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.Puertos;

/// <summary>Acceso a las instalaciones vendidas (panel de la plataforma).</summary>
public interface IRepositorioInstalaciones
{
    Task<Instalacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Busca una instalación por su clave de licencia (para validar al cliente).</summary>
    Task<Instalacion?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default);

    Task<IReadOnlyList<Instalacion>> ListarAsync(CancellationToken ct = default);

    void Agregar(Instalacion instalacion);
}
