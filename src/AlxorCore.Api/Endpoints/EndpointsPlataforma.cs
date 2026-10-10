using System.Security.Claims;
using AlxorCore.Api.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Endpoints;

/// <summary>
/// Endpoints de la <b>plataforma</b> (panel de instalaciones vendidas, sólo para el administrador de
/// Map Technology) y la validación pública de licencias que usan las instalaciones de los clientes.
/// El administrador se define en configuración: <c>Plataforma:Administradores</c> (lista de correos).
/// </summary>
public static class EndpointsPlataforma
{
    private sealed record EstadoPeticion(EstadoInstalacion Estado);

    public static IEndpointRouteBuilder MapearPlataforma(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var plat = rutas.MapGroup("/plataforma").WithTags("Plataforma");

        plat.MapGet("/soy-admin", (ClaimsPrincipal usuario, IConfiguration cfg) =>
                Results.Ok(new { esAdmin = EsAdmin(usuario, cfg) }))
            .WithSummary("Indica si el usuario autenticado es administrador de la plataforma.")
            .RequireAuthorization();

        plat.MapGet("/instalaciones", async (ClaimsPrincipal usuario, IConfiguration cfg, ListarInstalaciones caso, CancellationToken ct) =>
                !EsAdmin(usuario, cfg) ? Prohibido() : Results.Ok(await caso.EjecutarAsync(ct).ConfigureAwait(false)))
            .WithSummary("Lista las instalaciones vendidas.")
            .RequireAuthorization();

        plat.MapGet("/resumen", async (ClaimsPrincipal usuario, IConfiguration cfg, ResumenPlataforma caso, CancellationToken ct) =>
                !EsAdmin(usuario, cfg) ? Prohibido() : Results.Ok(await caso.EjecutarAsync(ct).ConfigureAwait(false)))
            .WithSummary("Resumen del panel: recuentos por estado e ingresos recurrentes (MRR).")
            .RequireAuthorization();

        plat.MapPost("/instalaciones", async (DatosInstalacion datos, ClaimsPrincipal usuario, IConfiguration cfg, CrearInstalacion caso, CancellationToken ct) =>
                !EsAdmin(usuario, cfg) ? Prohibido() : (await caso.EjecutarAsync(datos, ct).ConfigureAwait(false)).ACreado("/plataforma/instalaciones"))
            .WithSummary("Da de alta una instalación (genera su clave de licencia).")
            .RequireAuthorization();

        plat.MapPut("/instalaciones/{id:guid}", async (Guid id, DatosInstalacion datos, ClaimsPrincipal usuario, IConfiguration cfg, ActualizarInstalacion caso, CancellationToken ct) =>
                !EsAdmin(usuario, cfg) ? Prohibido() : (await caso.EjecutarAsync(id, datos, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Edita una instalación.")
            .RequireAuthorization();

        plat.MapPost("/instalaciones/{id:guid}/estado", async (Guid id, EstadoPeticion peticion, ClaimsPrincipal usuario, IConfiguration cfg, CambiarEstadoInstalacion caso, CancellationToken ct) =>
                !EsAdmin(usuario, cfg) ? Prohibido() : (await caso.EjecutarAsync(id, peticion.Estado, ct).ConfigureAwait(false)).AOk())
            .WithSummary("Cambia el estado de una instalación (activar, suspender por impago, baja…).")
            .RequireAuthorization();

        // Validación pública de licencia: la usan las instalaciones de los clientes para saber si
        // siguen activas. No requiere autenticación; sólo responde según la clave.
        rutas.MapGet("/licencia/verificar", async (string? clave, VerificarLicencia caso, CancellationToken ct) =>
                Results.Ok(await caso.EjecutarAsync(clave, ct).ConfigureAwait(false)))
            .WithTags("Plataforma")
            .WithSummary("Valida una clave de licencia y devuelve si la instalación está activa.")
            .AllowAnonymous();

        return rutas;
    }

    private static IResult Prohibido() =>
        ResultadosHttp.AProblema(Error.Prohibido("plataforma.solo_admin", "Sólo el administrador de la plataforma puede acceder."));

    private static bool EsAdmin(ClaimsPrincipal usuario, IConfiguration cfg)
    {
        var email = usuario.ObtenerIdentidad()?.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var admins = cfg.GetSection("Plataforma:Administradores").Get<string[]>() ?? Array.Empty<string>();
        return admins.Any(a => string.Equals(a?.Trim(), email, StringComparison.OrdinalIgnoreCase));
    }
}
