using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Aprobaciones.Aplicacion;
using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Aprobaciones (reglas por umbral y solicitudes con SoD).</summary>
public static class EndpointsAprobaciones
{
    public static IEndpointRouteBuilder MapearAprobaciones(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var g = rutas.MapGroup("/aprobaciones").WithTags("Aprobaciones");

        g.MapGet("/reglas", ListarReglasAsync)
            .WithSummary("Lista las reglas de aprobación (por tipo de documento y umbral).")
            .RequireAuthorization();

        g.MapPut("/reglas", ConfigurarReglaAsync)
            .WithSummary("Configura (crea o actualiza) la regla de aprobación de un tipo de documento.")
            .RequierePermiso(Permisos.AprobacionConfigurar);

        g.MapGet("/requiere", RequiereAsync)
            .WithSummary("Indica si una operación de un tipo e importe requiere aprobación.")
            .RequireAuthorization();

        g.MapGet("/solicitudes", ListarSolicitudesAsync)
            .WithSummary("Lista las solicitudes de aprobación (opcionalmente por estado).")
            .RequireAuthorization();

        g.MapPost("/solicitudes", CrearSolicitudAsync)
            .WithSummary("Abre una solicitud de aprobación (el solicitante es el usuario actual).")
            .RequireAuthorization();

        g.MapPost("/solicitudes/{id:guid}/aprobar", AprobarAsync)
            .WithSummary("Aprueba una solicitud (no puede ser el propio solicitante: segregación de funciones).")
            .RequierePermiso(Permisos.AprobacionAprobar);

        g.MapPost("/solicitudes/{id:guid}/rechazar", RechazarAsync)
            .WithSummary("Rechaza una solicitud con un motivo (no puede ser el propio solicitante).")
            .RequierePermiso(Permisos.AprobacionAprobar);

        return rutas;
    }

    /// <summary>Cuerpo para configurar una regla de aprobación.</summary>
    public sealed record PeticionRegla(string TipoDocumento, decimal UmbralImporte, bool Activa = true);

    /// <summary>Cuerpo para rechazar una solicitud.</summary>
    public sealed record PeticionRechazo(string? Motivo);

    private static async Task<IResult> ListarReglasAsync(IContextoEmpresa contexto, ListarReglas caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> ConfigurarReglaAsync(PeticionRegla peticion, IContextoEmpresa contexto, ConfigurarRegla caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return (await caso.EjecutarAsync(contexto.EmpresaId.Value, peticion.TipoDocumento, peticion.UmbralImporte, peticion.Activa, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> RequiereAsync(IContextoEmpresa contexto, ConsultarRequiere caso, string tipoDocumento, decimal importe, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var requiere = await caso.EjecutarAsync(contexto.EmpresaId.Value, tipoDocumento, importe, ct).ConfigureAwait(false);
        return Results.Ok(new { requiere });
    }

    private static async Task<IResult> ListarSolicitudesAsync(IContextoEmpresa contexto, ListarSolicitudes caso, string? estado, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        return Results.Ok(await caso.EjecutarAsync(contexto.EmpresaId.Value, estado, ct).ConfigureAwait(false));
    }

    private static async Task<IResult> CrearSolicitudAsync(CrearSolicitudComando comando, ClaimsPrincipal usuario, IContextoEmpresa contexto, CrearSolicitud caso, CancellationToken ct)
    {
        if (contexto.EmpresaId is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("empresa.no_seleccionada", "Selecciona una empresa primero."));
        }

        var solicitante = usuario.ObtenerUsuarioId();
        if (solicitante is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("usuario.no_identificado", "No se ha podido identificar al usuario."));
        }

        var r = await caso.EjecutarAsync(contexto.EmpresaId.Value, solicitante.Value, comando, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.ACreado($"/aprobaciones/solicitudes/{r.Valor.Id}") : ResultadosHttp.AProblema(r.Error);
    }

    private static async Task<IResult> AprobarAsync(Guid id, ClaimsPrincipal usuario, ResolverSolicitud caso, CancellationToken ct)
    {
        var aprobador = usuario.ObtenerUsuarioId();
        if (aprobador is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("usuario.no_identificado", "No se ha podido identificar al usuario."));
        }

        return (await caso.AprobarAsync(id, aprobador.Value, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> RechazarAsync(Guid id, PeticionRechazo peticion, ClaimsPrincipal usuario, ResolverSolicitud caso, CancellationToken ct)
    {
        var aprobador = usuario.ObtenerUsuarioId();
        if (aprobador is null)
        {
            return ResultadosHttp.AProblema(Error.Validacion("usuario.no_identificado", "No se ha podido identificar al usuario."));
        }

        return (await caso.RechazarAsync(id, aprobador.Value, peticion.Motivo, ct).ConfigureAwait(false)).AOk();
    }
}
