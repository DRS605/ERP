namespace AlxorCore.Identidad.Aplicacion.Modelos;

/// <summary>
/// Resultado de un inicio de sesión. Si el usuario tiene la verificación en dos pasos activa y no ha
/// aportado (o no es válido) el código, <see cref="Requiere2fa"/> es verdadero y no se emite token:
/// el cliente debe repetir el login incluyendo el código.
/// </summary>
public sealed record ResultadoAutenticacion(string Token, DateTimeOffset ExpiraEn, PerfilUsuario Usuario, bool Requiere2fa = false)
{
    /// <summary>Resultado que indica que hace falta el segundo factor para completar el inicio de sesión.</summary>
    public static ResultadoAutenticacion RetoDobleFactor(PerfilUsuario usuario) =>
        new(string.Empty, default, usuario, Requiere2fa: true);
}
