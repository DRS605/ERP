using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Infraestructura.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Organizacion.Infraestructura;

/// <summary>
/// Implementa el puerto transversal <see cref="IConfiguracionEmpresa"/> leyendo los parámetros de la
/// empresa (p. ej. el método de valoración). Permite a otros módulos conocerlos sin depender de
/// Organización.
/// </summary>
internal sealed class ConfiguracionEmpresaOrganizacion : IConfiguracionEmpresa
{
    private readonly OrganizacionDbContext _contexto;

    public ConfiguracionEmpresaOrganizacion(OrganizacionDbContext contexto) => _contexto = contexto;

    public async Task<MetodoValoracion> MetodoValoracionAsync(Guid empresaId, CancellationToken ct = default)
    {
        var metodo = await _contexto.Empresas.AsNoTracking()
            .Where(e => e.Id == empresaId)
            .Select(e => (MetodoValoracion?)e.MetodoValoracion)
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return metodo ?? MetodoValoracion.Estandar;
    }
}
