namespace AlxorCore.Api.Contratos;

/// <summary>Cuerpo de la petición de registro.</summary>
public sealed record RegistroPeticion(string Email, string Nombre, string Contrasena);

/// <summary>Cuerpo de la petición de inicio de sesión. <c>Codigo</c> es el segundo factor (2FA), opcional.</summary>
public sealed record LoginPeticion(string Email, string Contrasena, string? Codigo = null);

/// <summary>Cuerpo para activar el 2FA con el código que muestra la app de autenticación.</summary>
public sealed record ActivarDobleFactorPeticion(string Codigo);

/// <summary>Cuerpo para verificar el correo con el token del enlace.</summary>
public sealed record VerificarEmailPeticion(string Token);

/// <summary>Cuerpo para solicitar el restablecimiento de contraseña.</summary>
public sealed record RecuperarPeticion(string Email);

/// <summary>Cuerpo para fijar la nueva contraseña con el token del enlace.</summary>
public sealed record RestablecerPeticion(string Token, string NuevaContrasena);
