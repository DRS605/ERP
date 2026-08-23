using AlxorCore.Identidad.Aplicacion.Modelos;
using AlxorCore.Identidad.Aplicacion.Puertos;
using AlxorCore.Nucleo.Seguridad;
using AlxorCore.Identidad.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Identidad.Aplicacion.CasosDeUso;

/// <summary>Credenciales para iniciar sesión. <paramref name="Codigo"/> es el segundo factor (2FA), opcional.</summary>
public sealed record IniciarSesionComando(string Email, string Contrasena, string? Codigo = null);

/// <summary>
/// Caso de uso: autenticación por correo y contraseña. Ante credenciales incorrectas devuelve
/// siempre el mismo error genérico para no revelar si el correo existe (evita enumeración).
/// </summary>
public sealed class IniciarSesion
{
    private static readonly Error CredencialesInvalidas =
        Error.NoAutenticado("auth.credenciales_invalidas", "El correo o la contraseña no son correctos.");

    private readonly IRepositorioUsuarios _usuarios;
    private readonly IHasherContrasena _hasher;
    private readonly IProveedorTokens _tokens;
    private readonly IUnidadDeTrabajoIdentidad _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public IniciarSesion(IRepositorioUsuarios usuarios, IHasherContrasena hasher, IProveedorTokens tokens, IUnidadDeTrabajoIdentidad unidadDeTrabajo, IReloj reloj)
    {
        _usuarios = usuarios;
        _hasher = hasher;
        _tokens = tokens;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ResultadoAutenticacion>> EjecutarAsync(IniciarSesionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var email = Email.Crear(comando.Email);
        if (email.EsFallo)
        {
            return Resultado.Fallo<ResultadoAutenticacion>(CredencialesInvalidas);
        }

        var usuario = await _usuarios.ObtenerPorEmailAsync(email.Valor, ct).ConfigureAwait(false);
        if (usuario is null || !_hasher.Verificar(usuario.HashContrasena.Valor, comando.Contrasena ?? string.Empty))
        {
            return Resultado.Fallo<ResultadoAutenticacion>(CredencialesInvalidas);
        }

        if (!usuario.PuedeAutenticarse)
        {
            return Resultado.Fallo<ResultadoAutenticacion>(
                Error.Prohibido("auth.cuenta_suspendida", "La cuenta está suspendida."));
        }

        // Verificación en dos pasos: si está activa, exige un segundo factor válido (TOTP o código de recuperación).
        if (usuario.DobleFactorActivo)
        {
            if (string.IsNullOrWhiteSpace(comando.Codigo))
            {
                return Resultado.Ok(ResultadoAutenticacion.RetoDobleFactor(PerfilUsuario.Desde(usuario)));
            }

            if (!usuario.VerificarSegundoFactor(comando.Codigo, _reloj))
            {
                return Resultado.Fallo<ResultadoAutenticacion>(
                    Error.NoAutenticado("auth.2fa_invalido", "El código de verificación no es correcto."));
            }

            // Un código de recuperación consumido modifica al usuario: se persiste.
            await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        var identidad = new IdentidadUsuario(usuario.Id, usuario.Email.Valor, usuario.Nombre, usuario.EmailVerificado);
        var token = _tokens.GenerarToken(identidad);
        var resultado = new ResultadoAutenticacion(token.Token, token.ExpiraEn, PerfilUsuario.Desde(usuario));
        return Resultado.Ok(resultado);
    }
}
