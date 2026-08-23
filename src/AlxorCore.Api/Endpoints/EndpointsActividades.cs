using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Endpoints REST de <b>actividades de negocio</b> (clasificación transversal del grupo) y de la
/// <b>visibilidad</b> por usuario y pantalla. Las actividades pertenecen al grupo (holding) y son
/// compartidas por todas sus empresas.
/// </summary>
public static class EndpointsActividades
{
    public static IEndpointRouteBuilder MapearActividades(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/actividades").WithTags("Actividades de negocio");

        g.MapGet("", ListarAsync)
            .WithSummary("Lista las actividades de negocio del grupo activo.")
            .RequireAuthorization();

        g.MapPost("", CrearAsync)
            .WithSummary("Crea una actividad de negocio en el grupo activo.")
            .RequierePermiso(Permisos.ActividadGestionar);

        g.MapPut("/{id:guid}", ActualizarAsync)
            .WithSummary("Renombra o activa/desactiva una actividad de negocio.")
            .RequierePermiso(Permisos.ActividadGestionar);

        g.MapGet("/visibilidad/{usuarioId:guid}", ConsultarVisibilidadAsync)
            .WithSummary("Consulta las actividades que un usuario puede ver por área (pantalla).")
            .RequierePermiso(Permisos.ActividadGestionar);

        g.MapPut("/visibilidad/{usuarioId:guid}", FijarVisibilidadAsync)
            .WithSummary("Fija (reemplaza) las actividades que un usuario ve en un área.")
            .RequierePermiso(Permisos.ActividadGestionar);

        return rutas;
    }

    /// <summary>Cuerpo para crear una actividad de negocio.</summary>
    public sealed record CrearActividadPeticion(string? Nombre);

    /// <summary>Cuerpo para actualizar una actividad de negocio.</summary>
    public sealed record ActualizarActividadPeticion(string? Nombre, bool Activa);

    /// <summary>Cuerpo para fijar la visibilidad de un usuario en un área concreta.</summary>
    public sealed record FijarVisibilidadPeticion(AreaVisibilidad Area, IReadOnlyList<Guid>? Actividades);

    private static async Task<IResult> ListarAsync(IContextoEmpresa contexto, ListarActividades caso, CancellationToken ct)
    {
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.GrupoId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearAsync(CrearActividadPeticion peticion, IContextoEmpresa contexto, CrearActividad caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var r = await caso.EjecutarAsync(contexto.GrupoId.Value, peticion.Nombre, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/actividades/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> ActualizarAsync(Guid id, ActualizarActividadPeticion peticion, ActualizarActividad caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        return (await caso.EjecutarAsync(id, peticion.Nombre, peticion.Activa, ct).ConfigureAwait(false)).ASinContenido();
    }

    private static async Task<IResult> ConsultarVisibilidadAsync(Guid usuarioId, ConsultarVisibilidadUsuario caso, CancellationToken ct) =>
        Results.Ok(await caso.EjecutarAsync(usuarioId, ct).ConfigureAwait(false));

    private static async Task<IResult> FijarVisibilidadAsync(Guid usuarioId, FijarVisibilidadPeticion peticion, IContextoEmpresa contexto, FijarVisibilidadUsuario caso, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(peticion);
        if (contexto.GrupoId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var actividades = peticion.Actividades ?? Array.Empty<Guid>();
        return (await caso.EjecutarAsync(contexto.GrupoId.Value, usuarioId, peticion.Area, actividades, ct).ConfigureAwait(false)).ASinContenido();
    }
}
