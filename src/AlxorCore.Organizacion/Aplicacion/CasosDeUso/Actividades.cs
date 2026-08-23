using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Vista de una actividad de negocio.</summary>
public sealed record ActividadNegocioDto(Guid Id, string Nombre, bool Activa)
{
    public static ActividadNegocioDto Desde(ActividadNegocio a) => new(a.Id, a.Nombre, a.Activa);
}

/// <summary>Visibilidad concedida a un usuario en un área: lista de actividades permitidas.</summary>
public sealed record VisibilidadAreaDto(AreaVisibilidad Area, IReadOnlyList<Guid> Actividades);

// ---------------------------------------------------------------------------
// Puertos
// ---------------------------------------------------------------------------

/// <summary>Repositorio de actividades de negocio (por grupo).</summary>
public interface IRepositorioActividades
{
    void Agregar(ActividadNegocio actividad);

    Task<ActividadNegocio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ActividadNegocio>> ListarAsync(Guid grupoId, CancellationToken ct = default);
}

/// <summary>Repositorio de reglas de visibilidad de actividades por usuario y área.</summary>
public interface IRepositorioVisibilidad
{
    void Agregar(VisibilidadActividad visibilidad);

    void Eliminar(VisibilidadActividad visibilidad);

    Task<IReadOnlyList<VisibilidadActividad>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken ct = default);

    Task<IReadOnlyList<VisibilidadActividad>> ListarPorUsuarioYAreaAsync(Guid usuarioId, AreaVisibilidad area, CancellationToken ct = default);
}

/// <summary>
/// Consulta transversal de visibilidad: qué actividades puede ver un usuario en un área. Devuelve
/// <c>null</c> cuando el usuario no tiene ninguna regla en esa área (ve <b>todas</b>), o el conjunto
/// permitido en caso contrario. La usan los listados de maestros para filtrar por pantalla.
/// </summary>
public interface IConsultaVisibilidad
{
    Task<IReadOnlyCollection<Guid>?> ActividadesPermitidasAsync(Guid usuarioId, AreaVisibilidad area, CancellationToken ct = default);
}

// ---------------------------------------------------------------------------
// Casos de uso — Actividades
// ---------------------------------------------------------------------------

/// <summary>Caso de uso: crear una actividad de negocio en el grupo activo.</summary>
public sealed class CrearActividad
{
    private readonly IRepositorioActividades _actividades;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public CrearActividad(IRepositorioActividades actividades, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _actividades = actividades;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<ActividadNegocioDto>> EjecutarAsync(Guid grupoId, string? nombre, CancellationToken ct = default)
    {
        var actividad = ActividadNegocio.Crear(grupoId, nombre, _reloj);
        if (actividad.EsFallo)
        {
            return Resultado.Fallo<ActividadNegocioDto>(actividad.Error);
        }

        _actividades.Agregar(actividad.Valor);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ActividadNegocioDto.Desde(actividad.Valor));
    }
}

/// <summary>Caso de uso: listar las actividades de negocio del grupo activo.</summary>
public sealed class ListarActividades
{
    private readonly IRepositorioActividades _actividades;

    public ListarActividades(IRepositorioActividades actividades) => _actividades = actividades;

    public async Task<IReadOnlyList<ActividadNegocioDto>> EjecutarAsync(Guid grupoId, CancellationToken ct = default) =>
        (await _actividades.ListarAsync(grupoId, ct).ConfigureAwait(false)).Select(ActividadNegocioDto.Desde).ToList();
}

/// <summary>Caso de uso: renombrar o activar/desactivar una actividad de negocio.</summary>
public sealed class ActualizarActividad
{
    private readonly IRepositorioActividades _actividades;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public ActualizarActividad(IRepositorioActividades actividades, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _actividades = actividades;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado> EjecutarAsync(Guid id, string? nombre, bool activa, CancellationToken ct = default)
    {
        var actividad = await _actividades.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (actividad is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("actividad.no_encontrada", "La actividad no existe."));
        }

        var renombrada = actividad.Renombrar(nombre, _reloj);
        if (renombrada.EsFallo)
        {
            return renombrada;
        }

        actividad.FijarActiva(activa, _reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

// ---------------------------------------------------------------------------
// Casos de uso — Visibilidad
// ---------------------------------------------------------------------------

/// <summary>Caso de uso: fijar (reemplazar) las actividades que un usuario ve en un área.</summary>
public sealed class FijarVisibilidadUsuario
{
    private readonly IRepositorioVisibilidad _visibilidad;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public FijarVisibilidadUsuario(IRepositorioVisibilidad visibilidad, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _visibilidad = visibilidad;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado> EjecutarAsync(Guid grupoId, Guid usuarioId, AreaVisibilidad area, IReadOnlyCollection<Guid> actividades, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(actividades);

        // Se reemplaza el conjunto del usuario en el área: se borran las reglas previas y se crean las nuevas.
        var previas = await _visibilidad.ListarPorUsuarioYAreaAsync(usuarioId, area, ct).ConfigureAwait(false);
        foreach (var v in previas)
        {
            _visibilidad.Eliminar(v);
        }

        foreach (var actividadId in actividades.Distinct())
        {
            _visibilidad.Agregar(VisibilidadActividad.Crear(grupoId, usuarioId, area, actividadId, _reloj));
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Caso de uso: consultar la visibilidad de un usuario (actividades por área).</summary>
public sealed class ConsultarVisibilidadUsuario
{
    private readonly IRepositorioVisibilidad _visibilidad;

    public ConsultarVisibilidadUsuario(IRepositorioVisibilidad visibilidad) => _visibilidad = visibilidad;

    public async Task<IReadOnlyList<VisibilidadAreaDto>> EjecutarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var reglas = await _visibilidad.ListarPorUsuarioAsync(usuarioId, ct).ConfigureAwait(false);
        return reglas
            .GroupBy(r => r.Area)
            .Select(g => new VisibilidadAreaDto(g.Key, g.Select(r => r.ActividadNegocioId).ToList()))
            .ToList();
    }
}
