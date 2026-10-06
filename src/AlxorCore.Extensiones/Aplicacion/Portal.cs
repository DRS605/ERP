using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Extensiones.Aplicacion;

public interface IRepositorioPortal
{
    void Agregar(AccesoPortal acceso);

    Task<IReadOnlyList<AccesoPortal>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<AccesoPortal?> ObtenerAsync(Guid id, CancellationToken ct = default);
}

public sealed record DatosAccesoPortal(TipoPortal Tipo, Guid TerceroId, string? Nombre, DateOnly? Caduca = null);

public sealed record AccesoPortalDto(Guid Id, string Tipo, Guid TerceroId, string Nombre, DateTimeOffset CreadoEn, DateOnly? Caduca, DateTimeOffset? RevocadoEn,
    DateTimeOffset? UltimoAcceso, int Accesos, bool Vigente)
{
    public static AccesoPortalDto De(AccesoPortal a, DateOnly hoy) =>
        new(a.Id, a.Tipo.ToString(), a.TerceroId, a.Nombre, a.CreadoEn, a.Caduca, a.RevocadoEn, a.UltimoAcceso, a.Accesos, a.VigenteEl(hoy));
}

/// <summary>El acceso recién creado o regenerado con su clave (la única vez que se ve).</summary>
public sealed record ClaveAccesoPortalDto(AccesoPortalDto Acceso, string Clave);

/// <summary>
/// Accesos al portal de agricultores y clientes. La clave que se entrega es «empresa.acceso.secreto»: la empresa y el
/// acceso permiten encontrarlo (con la seguridad por filas de la empresa) y el secreto lo valida.
/// </summary>
public sealed class AccesosPortal
{
    private readonly IRepositorioPortal _repo;
    private readonly IUnidadDeTrabajoExtensiones _unidad;
    private readonly IReloj _reloj;

    public AccesosPortal(IRepositorioPortal repo, IUnidadDeTrabajoExtensiones unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public static string Clave(AccesoPortal a, string secreto) => $"{a.EmpresaId:N}.{a.Id:N}.{secreto}";

    /// <summary>Separa la clave en empresa, acceso y secreto.</summary>
    public static (Guid Empresa, Guid Acceso, string Secreto)? Partes(string? clave)
    {
        var p = (clave ?? string.Empty).Trim().Split('.');
        return p.Length == 3 && Guid.TryParseExact(p[0], "N", out var e) && Guid.TryParseExact(p[1], "N", out var a) && p[2].Length >= 20 ? (e, a, p[2]) : null;
    }

    public async Task<IReadOnlyList<AccesoPortalDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(a => a.Tipo).ThenBy(a => a.Nombre, StringComparer.CurrentCulture)
            .Select(a => AccesoPortalDto.De(a, Hoy)).ToList();

    public async Task<Resultado<ClaveAccesoPortalDto>> CrearAsync(Guid empresaId, DatosAccesoPortal d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.Tipo == TipoPortal.TerminalPlanta)
        {
            // Un terminal no es de ningún tercero: se identifica a sí mismo.
            d = d with { TerceroId = Guid.NewGuid() };
        }

        if ((await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(a => a.Tipo == d.Tipo && a.TerceroId == d.TerceroId && a.VigenteEl(Hoy)))
        {
            return Resultado.Fallo<ClaveAccesoPortalDto>(Error.Conflicto("portal.duplicado", "Ya tiene un acceso vigente: regenera su clave o revócalo."));
        }

        var r = AccesoPortal.Crear(empresaId, d.Tipo, d.TerceroId, d.Nombre, d.Caduca, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ClaveAccesoPortalDto>(r.Error);
        }

        _repo.Agregar(r.Valor.Acceso);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ClaveAccesoPortalDto(AccesoPortalDto.De(r.Valor.Acceso, Hoy), Clave(r.Valor.Acceso, r.Valor.Secreto)));
    }

    public async Task<Resultado<ClaveAccesoPortalDto>> RegenerarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<ClaveAccesoPortalDto>(Error.NoEncontrado("portal.no_encontrado", "El acceso no existe."));
        }

        var s = a.Regenerar();
        if (s.EsFallo)
        {
            return Resultado.Fallo<ClaveAccesoPortalDto>(s.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ClaveAccesoPortalDto(AccesoPortalDto.De(a, Hoy), Clave(a, s.Valor)));
    }

    public async Task<Resultado> RevocarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("portal.no_encontrado", "El acceso no existe."));
        }

        var r = a.Revocar(_reloj);
        if (r.EsFallo)
        {
            return r;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Valida la clave (con la empresa del acceso ya fijada en el contexto) y anota el acceso.</summary>
    public async Task<Resultado<AccesoPortal>> EntrarAsync(Guid accesoId, string secreto, CancellationToken ct = default)
    {
        var a = await _repo.ObtenerAsync(accesoId, ct).ConfigureAwait(false);
        if (a is null || !a.Valida(secreto, Hoy))
        {
            return Resultado.Fallo<AccesoPortal>(Error.NoAutenticado("portal.clave", "El enlace no es válido, ha caducado o se ha revocado."));
        }

        a.RegistrarAcceso(_reloj);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(a);
    }

    /// <summary>El acceso de la sesión, si sigue vigente (se comprueba en cada petición del portal).</summary>
    public async Task<AccesoPortal?> VigenteAsync(Guid accesoId, CancellationToken ct = default) =>
        await _repo.ObtenerAsync(accesoId, ct).ConfigureAwait(false) is { } a && a.VigenteEl(Hoy) ? a : null;
}
