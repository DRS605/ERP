using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace AlxorCore.Organizacion.Infraestructura.Persistencia;

internal sealed class RepositorioEmpresas : IRepositorioEmpresas, IConsultaEmpresas
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioEmpresas(OrganizacionDbContext contexto) => _contexto = contexto;

    public Task<Empresa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Empresas.SingleOrDefaultAsync(e => e.Id == id, ct);

    public Task<bool> ExisteNifAsync(string nif, CancellationToken ct = default)
    {
        var vo = Nif.Rehidratar(nif);
        return _contexto.Empresas.AnyAsync(e => e.Nif == vo, ct);
    }

    public Task<Guid?> ObtenerGrupoIdAsync(Guid empresaId, CancellationToken ct = default) =>
        _contexto.Empresas.Where(e => e.Id == empresaId).Select(e => (Guid?)e.GrupoId).SingleOrDefaultAsync(ct);

    public void Agregar(Empresa empresa) => _contexto.Empresas.Add(empresa);

    public async Task<EmpresaDto?> ObtenerAsync(Guid empresaId, CancellationToken ct = default)
    {
        var empresa = await _contexto.Empresas.SingleOrDefaultAsync(e => e.Id == empresaId, ct).ConfigureAwait(false);
        return empresa is null ? null : EmpresaDto.Desde(empresa);
    }
}

internal sealed class RepositorioGrupos : IRepositorioGrupos
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioGrupos(OrganizacionDbContext contexto) => _contexto = contexto;

    public Task<Grupo?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Grupos.SingleOrDefaultAsync(g => g.Id == id, ct);

    public void Agregar(Grupo grupo) => _contexto.Grupos.Add(grupo);
}

internal sealed class RepositorioMembresias : IRepositorioMembresias
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioMembresias(OrganizacionDbContext contexto) => _contexto = contexto;

    public Task<Membresia?> ObtenerAsync(Guid usuarioId, Guid empresaId, CancellationToken ct = default) =>
        _contexto.Membresias.SingleOrDefaultAsync(m => m.UsuarioId == usuarioId && m.EmpresaId == empresaId, ct);

    public void Agregar(Membresia membresia) => _contexto.Membresias.Add(membresia);

    public async Task<IReadOnlyList<Membresia>> ListarPorEmpresaAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Membresias.Where(m => m.EmpresaId == empresaId).OrderBy(m => m.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioSeries : IRepositorioSeries
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioSeries(OrganizacionDbContext contexto) => _contexto = contexto;

    public void Agregar(SerieNumeracion serie) => _contexto.Series.Add(serie);

    public async Task<IReadOnlyList<SerieNumeracion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Series.Where(s => s.EmpresaId == empresaId).OrderBy(s => s.Prefijo).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> ExisteAsync(Guid empresaId, TipoDocumento tipo, int ejercicio, string prefijo, CancellationToken ct = default) =>
        _contexto.Series.AnyAsync(
            s => s.EmpresaId == empresaId && s.TipoDocumento == tipo && s.Ejercicio == ejercicio && s.Prefijo == prefijo,
            ct);
}

internal sealed class RepositorioAsignacionesSerie : IRepositorioAsignacionesSerie, IResolverSerie
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioAsignacionesSerie(OrganizacionDbContext contexto) => _contexto = contexto;

    public void Agregar(AsignacionSerie asignacion) => _contexto.AsignacionesSerie.Add(asignacion);

    public Task<AsignacionSerie?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.AsignacionesSerie.SingleOrDefaultAsync(a => a.Id == id, ct);

    public void Eliminar(AsignacionSerie asignacion) => _contexto.AsignacionesSerie.Remove(asignacion);

    public async Task<IReadOnlyList<AsignacionSerie>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.AsignacionesSerie.Where(a => a.EmpresaId == empresaId)
            .OrderBy(a => a.TipoDocumento).ThenBy(a => a.Ambito).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> ExisteAsync(Guid empresaId, TipoDocumento tipo, AmbitoSerie ambito, Guid terceroId, CancellationToken ct = default) =>
        _contexto.AsignacionesSerie.AnyAsync(
            a => a.EmpresaId == empresaId && a.TipoDocumento == tipo && a.Ambito == ambito && a.TerceroId == terceroId, ct);

    public async Task<string?> ResolverPrefijoAsync(Guid empresaId, TipoDocumento tipoDocumento, Guid? terceroId, CancellationToken ct = default)
    {
        // La serie del tercero (si tiene una asignada) tiene prioridad sobre la de la empresa.
        if (terceroId is { } id && id != Guid.Empty)
        {
            var especifica = await _contexto.AsignacionesSerie.AsNoTracking()
                .Where(a => a.EmpresaId == empresaId && a.TipoDocumento == tipoDocumento && a.TerceroId == id)
                .Select(a => a.Prefijo)
                .FirstOrDefaultAsync(ct).ConfigureAwait(false);
            if (especifica is not null)
            {
                return especifica;
            }
        }

        return await _contexto.AsignacionesSerie.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.TipoDocumento == tipoDocumento && a.Ambito == AmbitoSerie.Empresa)
            .Select(a => a.Prefijo)
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Consultas de lectura del módulo Organización (join membresía-empresa).</summary>
internal sealed class ConsultasOrganizacion : IConsultasOrganizacion
{
    private readonly OrganizacionDbContext _contexto;

    public ConsultasOrganizacion(OrganizacionDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<EmpresaResumen>> ListarEmpresasDeUsuarioAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var filas = await (
            from m in _contexto.Membresias
            where m.UsuarioId == usuarioId && m.Estado == EstadoMembresia.Activa
            join e in _contexto.Empresas on m.EmpresaId equals e.Id
            select new { e.Id, e.Nif, e.RazonSocial, m.RolCodigo })
            .ToListAsync(ct).ConfigureAwait(false);

        return filas
            .Select(f => new EmpresaResumen(f.Id, f.Nif.Valor, f.RazonSocial, f.RolCodigo))
            .ToList();
    }
}

internal sealed class RepositorioFormasPago : IRepositorioFormasPago, IConsultaFormasPago
{
    private readonly OrganizacionDbContext _contexto;

    public RepositorioFormasPago(OrganizacionDbContext contexto) => _contexto = contexto;

    public void Agregar(FormaPago forma) => _contexto.FormasPago.Add(forma);

    public Task<FormaPago?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.FormasPago.SingleOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<FormaPago>> ListarAsync(Guid empresaId, bool incluirInactivas = false, CancellationToken ct = default) =>
        await _contexto.FormasPago
            .Where(f => f.EmpresaId == empresaId && (incluirInactivas || f.Activo))
            .OrderBy(f => f.Nombre).ToListAsync(ct).ConfigureAwait(false);

    async Task<FormaPagoDto?> IConsultaFormasPago.ObtenerAsync(Guid formaPagoId, CancellationToken ct)
    {
        var forma = await _contexto.FormasPago.AsNoTracking().SingleOrDefaultAsync(f => f.Id == formaPagoId, ct).ConfigureAwait(false);
        return forma is null ? null : FormaPagoDto.Desde(forma);
    }

    async Task<IReadOnlyList<FormaPagoDto>> IConsultaFormasPago.ListarAsync(Guid empresaId, bool incluirInactivas, CancellationToken ct) =>
        await _contexto.FormasPago.AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && (incluirInactivas || f.Activo))
            .OrderBy(f => f.Nombre).Select(f => FormaPagoDto.Desde(f)).ToListAsync(ct).ConfigureAwait(false);
}
