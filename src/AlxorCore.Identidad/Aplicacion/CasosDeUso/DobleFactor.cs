using AlxorCore.Identidad.Aplicacion.Puertos;
using AlxorCore.Identidad.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Identidad.Aplicacion.CasosDeUso;

/// <summary>Datos para configurar la app de autenticación al preparar el 2FA.</summary>
public sealed record PreparacionDobleFactor(string Secreto, string UriOtpauth);

/// <summary>Estado del 2FA de un usuario.</summary>
public sealed record EstadoDobleFactor(bool Activo, int CodigosRecuperacionPendientes);

/// <summary>Emisor que aparece en la app de autenticación (Google Authenticator, etc.).</summary>
internal static class Emisor2fa
{
    public const string Nombre = "ALXOR Core";
}

/// <summary>Caso de uso: preparar el 2FA (genera el secreto y el URI; aún no queda activo).</summary>
public sealed class PrepararDobleFactor
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajoIdentidad _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public PrepararDobleFactor(IRepositorioUsuarios usuarios, IUnidadDeTrabajoIdentidad unidadDeTrabajo, IReloj reloj)
    {
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<PreparacionDobleFactor>> EjecutarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Resultado.Fallo<PreparacionDobleFactor>(Error.NoEncontrado("usuario.no_encontrado", "El usuario no existe."));
        }

        if (usuario.DobleFactorActivo)
        {
            return Resultado.Fallo<PreparacionDobleFactor>(Error.Conflicto("2fa.ya_activo", "La verificación en dos pasos ya está activa."));
        }

        var secreto = usuario.PrepararDobleFactor(_reloj);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new PreparacionDobleFactor(secreto, Totp.ConstruirUri(secreto, Emisor2fa.Nombre, usuario.Email.Valor)));
    }
}

/// <summary>Caso de uso: activar el 2FA validando un código; devuelve los códigos de recuperación.</summary>
public sealed class ActivarDobleFactor
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajoIdentidad _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActivarDobleFactor(IRepositorioUsuarios usuarios, IUnidadDeTrabajoIdentidad unidadDeTrabajo, IReloj reloj)
    {
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<IReadOnlyList<string>>> EjecutarAsync(Guid usuarioId, string? codigo, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Resultado.Fallo<IReadOnlyList<string>>(Error.NoEncontrado("usuario.no_encontrado", "El usuario no existe."));
        }

        var resultado = usuario.ActivarDobleFactor(codigo, _reloj);
        if (resultado.EsFallo)
        {
            return resultado;
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return resultado;
    }
}

/// <summary>Caso de uso: desactivar el 2FA del usuario.</summary>
public sealed class DesactivarDobleFactor
{
    private readonly IRepositorioUsuarios _usuarios;
    private readonly IUnidadDeTrabajoIdentidad _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public DesactivarDobleFactor(IRepositorioUsuarios usuarios, IUnidadDeTrabajoIdentidad unidadDeTrabajo, IReloj reloj)
    {
        _usuarios = usuarios;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado> EjecutarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct).ConfigureAwait(false);
        if (usuario is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("usuario.no_encontrado", "El usuario no existe."));
        }

        usuario.DesactivarDobleFactor(_reloj);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Caso de uso: consultar el estado del 2FA del usuario.</summary>
public sealed class ConsultarDobleFactor
{
    private readonly IRepositorioUsuarios _usuarios;

    public ConsultarDobleFactor(IRepositorioUsuarios usuarios) => _usuarios = usuarios;

    public async Task<Resultado<EstadoDobleFactor>> EjecutarAsync(Guid usuarioId, CancellationToken ct = default)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(usuarioId, ct).ConfigureAwait(false);
        return usuario is null
            ? Resultado.Fallo<EstadoDobleFactor>(Error.NoEncontrado("usuario.no_encontrado", "El usuario no existe."))
            : Resultado.Ok(new EstadoDobleFactor(usuario.DobleFactorActivo, usuario.CodigosRecuperacionPendientes));
    }
}
