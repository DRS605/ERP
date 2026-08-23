using AlxorCore.Api.Comun;
using AlxorCore.Api.Contratos;
using AlxorCore.Identidad.Aplicacion.CasosDeUso;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Endpoints;

/// <summary>Endpoints REST del módulo Identidad (autenticación y perfil).</summary>
public static class EndpointsIdentidad
{
    public static IEndpointRouteBuilder MapearIdentidad(this IEndpointRouteBuilder rutas)
    {
        ArgumentNullException.ThrowIfNull(rutas);

        var grupo = rutas.MapGroup("/auth").WithTags("Autenticación");

        grupo.MapPost("/registro", RegistrarAsync)
            .WithName("RegistrarUsuario")
            .WithSummary("Crea una cuenta de usuario.")
            .AllowAnonymous();

        grupo.MapPost("/login", LoginAsync)
            .WithName("IniciarSesion")
            .WithSummary("Inicia sesión y devuelve un token de acceso.")
            .RequireRateLimiting(AlxorCore.Api.Comun.OpcionesSeguridad.PoliticaAuth)
            .AllowAnonymous();

        grupo.MapGet("/perfil", PerfilAsync)
            .WithName("ObtenerPerfil")
            .WithSummary("Devuelve el perfil del usuario autenticado.")
            .RequireAuthorization();

        grupo.MapPost("/verificar-email", VerificarEmailAsync)
            .WithName("VerificarEmail")
            .WithSummary("Verifica el correo con el token del enlace.")
            .AllowAnonymous();

        grupo.MapPost("/recuperar", RecuperarAsync)
            .WithName("RecuperarContrasena")
            .WithSummary("Solicita un enlace de restablecimiento de contraseña.")
            .RequireRateLimiting(AlxorCore.Api.Comun.OpcionesSeguridad.PoliticaAuth)
            .AllowAnonymous();

        grupo.MapPost("/restablecer", RestablecerAsync)
            .WithName("RestablecerContrasena")
            .WithSummary("Fija una nueva contraseña con el token del enlace.")
            .AllowAnonymous();

        // Verificación en dos pasos (2FA / TOTP) — requieren usuario autenticado.
        grupo.MapGet("/2fa", EstadoDobleFactorAsync)
            .WithSummary("Estado del 2FA del usuario autenticado.")
            .RequireAuthorization();

        grupo.MapPost("/2fa/preparar", PrepararDobleFactorAsync)
            .WithSummary("Prepara el 2FA: devuelve el secreto y el URI otpauth (aún no queda activo).")
            .RequireAuthorization();

        grupo.MapPost("/2fa/activar", ActivarDobleFactorAsync)
            .WithSummary("Activa el 2FA validando un código; devuelve los códigos de recuperación (una sola vez).")
            .RequireAuthorization();

        grupo.MapPost("/2fa/desactivar", DesactivarDobleFactorAsync)
            .WithSummary("Desactiva el 2FA del usuario.")
            .RequireAuthorization();

        return rutas;
    }

    private static async Task<IResult> RegistrarAsync(
        RegistroPeticion peticion,
        RegistrarUsuario casoDeUso,
        IHostEnvironment entorno,
        CancellationToken ct)
    {
        var resultado = await casoDeUso
            .EjecutarAsync(new RegistrarUsuarioComando(peticion.Email, peticion.Nombre, peticion.Contrasena), ct)
            .ConfigureAwait(false);

        if (resultado.EsFallo)
        {
            return ResultadosHttp.AProblema(resultado.Error);
        }

        // El token del enlace de verificación solo se expone fuera de producción (en producción va solo por correo).
        var cuerpo = entorno.IsProduction()
            ? (object)resultado.Valor.Perfil
            : new { resultado.Valor.Perfil, resultado.Valor.TokenVerificacion };
        return Results.Created("/auth/perfil", cuerpo);
    }

    private static async Task<IResult> LoginAsync(
        LoginPeticion peticion,
        IniciarSesion casoDeUso,
        CancellationToken ct)
    {
        var resultado = await casoDeUso
            .EjecutarAsync(new IniciarSesionComando(peticion.Email, peticion.Contrasena, peticion.Codigo), ct)
            .ConfigureAwait(false);

        return resultado.AOk();
    }

    private static async Task<IResult> EstadoDobleFactorAsync(
        System.Security.Claims.ClaimsPrincipal usuario, ConsultarDobleFactor casoDeUso, CancellationToken ct)
    {
        var id = usuario.ObtenerUsuarioId();
        return id is null
            ? ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."))
            : (await casoDeUso.EjecutarAsync(id.Value, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> PrepararDobleFactorAsync(
        System.Security.Claims.ClaimsPrincipal usuario, PrepararDobleFactor casoDeUso, CancellationToken ct)
    {
        var id = usuario.ObtenerUsuarioId();
        return id is null
            ? ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."))
            : (await casoDeUso.EjecutarAsync(id.Value, ct).ConfigureAwait(false)).AOk();
    }

    private static async Task<IResult> ActivarDobleFactorAsync(
        ActivarDobleFactorPeticion peticion, System.Security.Claims.ClaimsPrincipal usuario, ActivarDobleFactor casoDeUso, CancellationToken ct)
    {
        var id = usuario.ObtenerUsuarioId();
        if (id is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        var resultado = await casoDeUso.EjecutarAsync(id.Value, peticion.Codigo, ct).ConfigureAwait(false);
        return resultado.EsCorrecto
            ? Results.Ok(new { codigosRecuperacion = resultado.Valor })
            : ResultadosHttp.AProblema(resultado.Error);
    }

    private static async Task<IResult> DesactivarDobleFactorAsync(
        System.Security.Claims.ClaimsPrincipal usuario, DesactivarDobleFactor casoDeUso, CancellationToken ct)
    {
        var id = usuario.ObtenerUsuarioId();
        return id is null
            ? ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."))
            : (await casoDeUso.EjecutarAsync(id.Value, ct).ConfigureAwait(false)).ASinContenido();
    }

    private static async Task<IResult> PerfilAsync(
        System.Security.Claims.ClaimsPrincipal usuario,
        ObtenerPerfil casoDeUso,
        CancellationToken ct)
    {
        var usuarioId = usuario.ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return ResultadosHttp.AProblema(Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario."));
        }

        var resultado = await casoDeUso.EjecutarAsync(usuarioId.Value, ct).ConfigureAwait(false);
        return resultado.AOk();
    }

    private static async Task<IResult> VerificarEmailAsync(
        VerificarEmailPeticion peticion,
        VerificarEmail casoDeUso,
        CancellationToken ct)
    {
        var resultado = await casoDeUso.EjecutarAsync(peticion.Token, ct).ConfigureAwait(false);
        return resultado.ASinContenido();
    }

    private static async Task<IResult> RecuperarAsync(
        RecuperarPeticion peticion,
        RecuperarContrasena casoDeUso,
        IHostEnvironment entorno,
        CancellationToken ct)
    {
        var token = await casoDeUso.EjecutarAsync(peticion.Email, ct).ConfigureAwait(false);

        // Respuesta uniforme (no revela si el correo existe). Fuera de producción devuelve el token para pruebas.
        var mensaje = "Si el correo corresponde a una cuenta, te hemos enviado un enlace para restablecer la contraseña.";
        return entorno.IsProduction() || token is null
            ? Results.Ok(new { mensaje })
            : Results.Ok(new { mensaje, token });
    }

    private static async Task<IResult> RestablecerAsync(
        RestablecerPeticion peticion,
        RestablecerContrasena casoDeUso,
        CancellationToken ct)
    {
        var resultado = await casoDeUso
            .EjecutarAsync(new RestablecerContrasenaComando(peticion.Token, peticion.NuevaContrasena), ct)
            .ConfigureAwait(false);
        return resultado.ASinContenido();
    }
}
