using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Organizacion.Infraestructura.Persistencia;

internal sealed class RepositorioInstalaciones : IRepositorioInstalaciones
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioInstalaciones(OrganizacionDbContext contexto) => _contexto = contexto;

    public Task<Instalacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Instalaciones.SingleOrDefaultAsync(i => i.Id == id, ct);

    public Task<Instalacion?> ObtenerPorClaveAsync(string clave, CancellationToken ct = default) =>
        _contexto.Instalaciones.SingleOrDefaultAsync(i => i.Clave == clave, ct);

    public async Task<IReadOnlyList<Instalacion>> ListarAsync(CancellationToken ct = default) =>
        await _contexto.Instalaciones.OrderBy(i => i.Nombre).ToListAsync(ct).ConfigureAwait(false);

    public void Agregar(Instalacion instalacion) => _contexto.Instalaciones.Add(instalacion);
}
