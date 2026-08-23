using AlxorCore.Identidad.Dominio.Eventos;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Identidad.Dominio;

/// <summary>
/// Usuario de la plataforma. Es una identidad global (no pertenece a una empresa): un mismo
/// usuario podrá operar en varias empresas a través de sus membresías (módulo Organización).
/// Raíz de agregado responsable de sus invariantes de estado y credenciales.
/// </summary>
public sealed class Usuario : RaizAgregado<Guid>
{
    public const int LongitudMaximaNombre = 120;

    // Constructor privado para EF Core (rehidratación desde la base de datos).
    private Usuario(Guid id)
        : base(id)
    {
        Email = null!;
        Nombre = null!;
        HashContrasena = null!;
    }

    private Usuario(Guid id, Email email, string nombre, HashContrasena hash, DateTimeOffset ahora)
        : base(id)
    {
        Email = email;
        Nombre = nombre;
        HashContrasena = hash;
        Estado = EstadoUsuario.Activo;
        EmailVerificado = false;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Correo electrónico (identificador de acceso, único en la plataforma).</summary>
    public Email Email { get; private set; }

    /// <summary>Nombre visible del usuario.</summary>
    public string Nombre { get; private set; }

    /// <summary>Contraseña cifrada.</summary>
    public HashContrasena HashContrasena { get; private set; }

    /// <summary>Estado de la cuenta.</summary>
    public EstadoUsuario Estado { get; private set; }

    /// <summary>Indica si el usuario ha verificado su correo.</summary>
    public bool EmailVerificado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    // --- Tokens de cuenta (solo se almacena el hash) ---
    public string? TokenVerificacionHash { get; private set; }
    public DateTimeOffset? TokenVerificacionExpira { get; private set; }
    public string? TokenRestablecimientoHash { get; private set; }
    public DateTimeOffset? TokenRestablecimientoExpira { get; private set; }

    // --- Verificación en dos pasos (2FA / TOTP) ---

    /// <summary>El usuario tiene la verificación en dos pasos activa.</summary>
    public bool DobleFactorActivo { get; private set; }

    /// <summary>Secreto TOTP en Base32. Se guarda desde que se prepara el 2FA (aunque aún no esté activo).</summary>
    public string? DobleFactorSecreto { get; private set; }

    /// <summary>Hashes (SHA-256) de los códigos de recuperación pendientes, separados por comas.</summary>
    public string? CodigosRecuperacion { get; private set; }

    /// <summary>Indica si el usuario puede autenticarse en este momento.</summary>
    public bool PuedeAutenticarse => Estado == EstadoUsuario.Activo;

    /// <summary>Número de códigos de recuperación de 2FA aún sin usar.</summary>
    public int CodigosRecuperacionPendientes =>
        string.IsNullOrEmpty(CodigosRecuperacion) ? 0 : CodigosRecuperacion.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Registra un nuevo usuario ya validado. El correo se activa de inmediato para no añadir
    /// fricción al alta; la verificación de correo se solicita aparte y no bloquea el uso.
    /// </summary>
    public static Resultado<Usuario> Registrar(Email email, string? nombre, HashContrasena hash, IReloj reloj)
    {
        var nombreNormalizado = (nombre ?? string.Empty).Trim();

        if (nombreNormalizado.Length == 0)
        {
            return Resultado.Fallo<Usuario>(Error.Validacion("usuario.nombre_vacio", "El nombre es obligatorio."));
        }

        if (nombreNormalizado.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo<Usuario>(Error.Validacion("usuario.nombre_largo", "El nombre es demasiado largo."));
        }

        var usuario = new Usuario(Guid.NewGuid(), email, nombreNormalizado, hash, reloj.AhoraUtc);
        usuario.RegistrarEvento(new UsuarioRegistrado(usuario.Id, email.Valor, reloj.AhoraUtc));
        return Resultado.Ok(usuario);
    }

    /// <summary>Marca el correo como verificado. Idempotente.</summary>
    public void VerificarEmail(IReloj reloj)
    {
        if (EmailVerificado)
        {
            return;
        }

        EmailVerificado = true;
        TokenVerificacionHash = null;
        TokenVerificacionExpira = null;
        Tocar(reloj);
        RegistrarEvento(new EmailUsuarioVerificado(Id, reloj.AhoraUtc));
    }

    /// <summary>Emite un token de verificación de correo (se almacena su hash) con su caducidad.</summary>
    public void EmitirTokenVerificacion(string token, DateTimeOffset expira, IReloj reloj)
    {
        TokenVerificacionHash = TokenCuenta.Hash(token);
        TokenVerificacionExpira = expira;
        Tocar(reloj);
    }

    /// <summary>Confirma el correo comprobando el token y su caducidad.</summary>
    public Resultado ConfirmarEmailConToken(string token, IReloj reloj)
    {
        if (EmailVerificado)
        {
            return Resultado.Ok();
        }

        if (TokenVerificacionHash is null || TokenVerificacionHash != TokenCuenta.Hash(token))
        {
            return Resultado.Fallo(Error.Validacion("verificacion.token_invalido", "El enlace de verificación no es válido."));
        }

        if (TokenVerificacionExpira is not null && TokenVerificacionExpira < reloj.AhoraUtc)
        {
            return Resultado.Fallo(Error.Validacion("verificacion.token_caducado", "El enlace de verificación ha caducado."));
        }

        VerificarEmail(reloj);
        return Resultado.Ok();
    }

    /// <summary>Emite un token de restablecimiento de contraseña (se almacena su hash) con su caducidad.</summary>
    public void EmitirTokenRestablecimiento(string token, DateTimeOffset expira, IReloj reloj)
    {
        TokenRestablecimientoHash = TokenCuenta.Hash(token);
        TokenRestablecimientoExpira = expira;
        Tocar(reloj);
    }

    /// <summary>Restablece la contraseña comprobando el token y su caducidad. El token se consume.</summary>
    public Resultado RestablecerConToken(string token, HashContrasena nuevoHash, IReloj reloj)
    {
        if (TokenRestablecimientoHash is null || TokenRestablecimientoHash != TokenCuenta.Hash(token))
        {
            return Resultado.Fallo(Error.Validacion("restablecimiento.token_invalido", "El enlace de restablecimiento no es válido."));
        }

        if (TokenRestablecimientoExpira is not null && TokenRestablecimientoExpira < reloj.AhoraUtc)
        {
            return Resultado.Fallo(Error.Validacion("restablecimiento.token_caducado", "El enlace de restablecimiento ha caducado."));
        }

        TokenRestablecimientoHash = null;
        TokenRestablecimientoExpira = null;
        CambiarContrasena(nuevoHash, reloj);
        return Resultado.Ok();
    }

    /// <summary>Sustituye la contraseña por un nuevo hash.</summary>
    public void CambiarContrasena(HashContrasena nuevoHash, IReloj reloj)
    {
        HashContrasena = nuevoHash;
        Tocar(reloj);
        RegistrarEvento(new ContrasenaUsuarioCambiada(Id, reloj.AhoraUtc));
    }

    /// <summary>Suspende la cuenta. Idempotente.</summary>
    public void Suspender(IReloj reloj)
    {
        if (Estado == EstadoUsuario.Suspendido)
        {
            return;
        }

        Estado = EstadoUsuario.Suspendido;
        Tocar(reloj);
        RegistrarEvento(new UsuarioSuspendido(Id, reloj.AhoraUtc));
    }

    /// <summary>Reactiva una cuenta suspendida. Idempotente.</summary>
    public void Reactivar(IReloj reloj)
    {
        if (Estado == EstadoUsuario.Activo)
        {
            return;
        }

        Estado = EstadoUsuario.Activo;
        Tocar(reloj);
        RegistrarEvento(new UsuarioReactivado(Id, reloj.AhoraUtc));
    }

    /// <summary>Número de códigos de recuperación que se generan al activar el 2FA.</summary>
    public const int NumeroCodigosRecuperacion = 8;

    /// <summary>
    /// Prepara la verificación en dos pasos: genera y guarda un secreto TOTP <b>sin activarlo</b>
    /// todavía. Devuelve el secreto para mostrarlo (QR/manual). La activación se confirma después con
    /// <see cref="ActivarDobleFactor"/> tras validar un código, evitando bloquearse por un secreto mal copiado.
    /// </summary>
    public string PrepararDobleFactor(IReloj reloj)
    {
        var secreto = Totp.GenerarSecreto();
        DobleFactorSecreto = secreto;
        DobleFactorActivo = false;
        CodigosRecuperacion = null;
        Tocar(reloj);
        return secreto;
    }

    /// <summary>
    /// Activa el 2FA comprobando un código TOTP contra el secreto preparado. Al activarse genera los
    /// <b>códigos de recuperación</b> (se guardan sus hashes) y los devuelve en claro para mostrarlos
    /// una única vez.
    /// </summary>
    public Resultado<IReadOnlyList<string>> ActivarDobleFactor(string? codigo, IReloj reloj)
    {
        if (DobleFactorActivo)
        {
            return Resultado.Fallo<IReadOnlyList<string>>(Error.Conflicto("2fa.ya_activo", "La verificación en dos pasos ya está activa."));
        }

        if (string.IsNullOrEmpty(DobleFactorSecreto))
        {
            return Resultado.Fallo<IReadOnlyList<string>>(Error.Validacion("2fa.no_preparado", "Primero hay que preparar la verificación en dos pasos."));
        }

        if (!Totp.Verificar(DobleFactorSecreto, codigo, reloj.AhoraUtc))
        {
            return Resultado.Fallo<IReadOnlyList<string>>(Error.Validacion("2fa.codigo_invalido", "El código no es correcto. Revisa la hora de tu dispositivo."));
        }

        var codigos = GenerarCodigosRecuperacion();
        CodigosRecuperacion = string.Join(',', codigos.Select(TokenCuenta.Hash));
        DobleFactorActivo = true;
        Tocar(reloj);
        RegistrarEvento(new DobleFactorActivado(Id, reloj.AhoraUtc));
        return Resultado.Ok<IReadOnlyList<string>>(codigos);
    }

    /// <summary>Desactiva el 2FA y borra el secreto y los códigos de recuperación. Idempotente.</summary>
    public void DesactivarDobleFactor(IReloj reloj)
    {
        if (!DobleFactorActivo && DobleFactorSecreto is null)
        {
            return;
        }

        var estaba = DobleFactorActivo;
        DobleFactorActivo = false;
        DobleFactorSecreto = null;
        CodigosRecuperacion = null;
        Tocar(reloj);
        if (estaba)
        {
            RegistrarEvento(new DobleFactorDesactivado(Id, reloj.AhoraUtc));
        }
    }

    /// <summary>
    /// Verifica el segundo factor en el inicio de sesión: admite un código TOTP válido o uno de los
    /// códigos de recuperación (que se <b>consume</b>). Devuelve verdadero si el código es aceptado.
    /// </summary>
    public bool VerificarSegundoFactor(string? codigo, IReloj reloj)
    {
        if (!DobleFactorActivo || string.IsNullOrWhiteSpace(codigo))
        {
            return false;
        }

        if (DobleFactorSecreto is not null && Totp.Verificar(DobleFactorSecreto, codigo, reloj.AhoraUtc))
        {
            return true;
        }

        return ConsumirCodigoRecuperacion(codigo, reloj);
    }

    private bool ConsumirCodigoRecuperacion(string codigo, IReloj reloj)
    {
        if (string.IsNullOrEmpty(CodigosRecuperacion))
        {
            return false;
        }

        var objetivo = TokenCuenta.Hash(codigo.Trim().ToUpperInvariant());
        var restantes = CodigosRecuperacion.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!restantes.Remove(objetivo))
        {
            return false;
        }

        CodigosRecuperacion = restantes.Count == 0 ? string.Empty : string.Join(',', restantes);
        Tocar(reloj);
        return true;
    }

    private static List<string> GenerarCodigosRecuperacion()
    {
        var codigos = new List<string>(NumeroCodigosRecuperacion);
        for (var i = 0; i < NumeroCodigosRecuperacion; i++)
        {
            // 10 caracteres hex en dos grupos, p. ej. "3F9A2-BC71D".
            var bruto = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(5)).ToUpperInvariant();
            codigos.Add($"{bruto[..5]}-{bruto[5..]}");
        }

        return codigos;
    }

    private void Tocar(IReloj reloj) => ActualizadoEn = reloj.AhoraUtc;
}
