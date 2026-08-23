using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Integraciones.Dominio;

/// <summary>
/// Clave de API de una empresa: credencial para la <b>API pública</b>, independiente del token JWT
/// de la interfaz. El secreto se muestra <b>una sola vez</b> al crearla; en la base solo se guarda su
/// hash (SHA-256), de modo que un volcado de la tabla no revela ninguna clave utilizable.
/// </summary>
public sealed class ClaveApi : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 80;

    private ClaveApi(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        Prefijo = null!;
        HashSecreto = null!;
    }

    private ClaveApi(Guid id, Guid empresaId, string nombre, string prefijo, string hashSecreto, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Nombre = nombre;
        Prefijo = prefijo;
        HashSecreto = hashSecreto;
        Activa = true;
        CreadoEn = ahora;
    }

    public string Nombre { get; private set; }

    /// <summary>Primeros caracteres de la clave, para identificarla en listados sin revelar el secreto.</summary>
    public string Prefijo { get; private set; }

    /// <summary>Hash SHA-256 (hex) del secreto completo. Nunca se guarda el secreto en claro.</summary>
    public string HashSecreto { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? UltimoUsoEn { get; private set; }

    public DateTimeOffset? RevocadaEn { get; private set; }

    /// <summary>Crea una clave de API y devuelve, junto al agregado, el <b>secreto en claro</b> (se muestra una única vez).</summary>
    public static Resultado<(ClaveApi Clave, string Secreto)> Crear(Guid empresaId, string? nombre, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<(ClaveApi, string)>(Error.Validacion("clave_api.nombre", "La clave necesita un nombre."));
        }

        var limpio = nombre.Trim();
        if (limpio.Length > LongitudMaximaNombre)
        {
            limpio = limpio[..LongitudMaximaNombre];
        }

        var secreto = GenerarSecreto();
        var clave = new ClaveApi(Guid.NewGuid(), empresaId, limpio, secreto[..11], Hash(secreto), reloj.AhoraUtc);
        return Resultado.Ok((clave, secreto));
    }

    /// <summary>Marca la clave como usada (última vez). No se persiste en caliente en cada petición si no interesa.</summary>
    public void RegistrarUso(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        UltimoUsoEn = reloj.AhoraUtc;
    }

    /// <summary>Revoca la clave: deja de ser válida para autenticar.</summary>
    public void Revocar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Activa)
        {
            return;
        }

        Activa = false;
        RevocadaEn = reloj.AhoraUtc;
    }

    /// <summary>Hash SHA-256 en hexadecimal de un secreto (para guardar y para comparar al autenticar).</summary>
    public static string Hash(string secreto)
    {
        ArgumentNullException.ThrowIfNull(secreto);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(secreto));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string GenerarSecreto()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var cuerpo = Convert.ToBase64String(bytes)
            .Replace("+", "A", StringComparison.Ordinal)
            .Replace("/", "B", StringComparison.Ordinal)
            .Replace("=", string.Empty, StringComparison.Ordinal);
        return "ak_" + cuerpo;
    }
}

/// <summary>Estado de una entrega de webhook.</summary>
public enum EstadoEntrega
{
    Pendiente = 1,
    Entregada = 2,
    Fallida = 3,
}

/// <summary>
/// Suscripción a <b>webhooks</b>: una URL de la empresa que recibirá, mediante POST firmado (HMAC),
/// los eventos de dominio a los que se suscribe (p. ej. <c>factura.emitida</c>). El secreto de firma
/// se genera al crearla y se comparte con quien integra para que verifique la autenticidad.
/// </summary>
public sealed class SuscripcionWebhook : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaUrl = 400;

    private SuscripcionWebhook(Guid id)
        : base(id, Guid.Empty)
    {
        Url = null!;
        Secreto = null!;
        Eventos = null!;
    }

    private SuscripcionWebhook(Guid id, Guid empresaId, string url, string secreto, string eventos, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Url = url;
        Secreto = secreto;
        Eventos = eventos;
        Activa = true;
        CreadoEn = ahora;
    }

    public string Url { get; private set; }

    /// <summary>Secreto compartido para firmar (HMAC-SHA256) el cuerpo de cada entrega.</summary>
    public string Secreto { get; private set; }

    /// <summary>Eventos suscritos, separados por comas (p. ej. <c>factura.emitida,cobro.registrado</c>).</summary>
    public string Eventos { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>Crea una suscripción validando la URL y la lista de eventos. Genera el secreto de firma.</summary>
    public static Resultado<SuscripcionWebhook> Crear(Guid empresaId, string? url, IReadOnlyCollection<string>? eventos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Resultado.Fallo<SuscripcionWebhook>(Error.Validacion("webhook.url", "La URL del webhook debe ser http(s) absoluta."));
        }

        if (url.Trim().Length > LongitudMaximaUrl)
        {
            return Resultado.Fallo<SuscripcionWebhook>(Error.Validacion("webhook.url_larga", "La URL es demasiado larga."));
        }

        var limpios = (eventos ?? Array.Empty<string>())
            .Select(e => (e ?? string.Empty).Trim().ToLowerInvariant())
            .Where(e => e.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (limpios.Count == 0)
        {
            return Resultado.Fallo<SuscripcionWebhook>(Error.Validacion("webhook.sin_eventos", "Suscribe al menos un evento."));
        }

        var invalidos = limpios.Where(e => !EventosIntegracion.Todos.Contains(e)).ToList();
        if (invalidos.Count > 0)
        {
            return Resultado.Fallo<SuscripcionWebhook>(Error.Validacion("webhook.evento_desconocido", $"Evento(s) no válido(s): {string.Join(", ", invalidos)}."));
        }

        var secreto = "whsec_" + Convert.ToHexString(RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
        var suscripcion = new SuscripcionWebhook(Guid.NewGuid(), empresaId, uri.ToString(), secreto, string.Join(',', limpios), reloj.AhoraUtc);
        return Resultado.Ok(suscripcion);
    }

    public bool Suscrito(string evento) =>
        Eventos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Contains(evento, StringComparer.Ordinal);

    public void Desactivar() => Activa = false;
}

/// <summary>
/// Intento de <b>entrega</b> de un evento a una suscripción de webhook. Guarda el cuerpo, el estado y
/// el contador de reintentos con reprogramación (backoff exponencial). Es el «buzón de salida» de
/// webhooks: un proceso en segundo plano lo recorre, envía y marca cada entrega.
/// </summary>
public sealed class EntregaWebhook : RaizAgregadoEmpresa<Guid>
{
    /// <summary>Número máximo de intentos antes de dar la entrega por fallida definitiva.</summary>
    public const int MaximoIntentos = 5;

    private EntregaWebhook(Guid id)
        : base(id, Guid.Empty)
    {
        Url = null!;
        SecretoFirma = null!;
        Evento = null!;
        Payload = null!;
    }

    private EntregaWebhook(Guid id, Guid empresaId, Guid suscripcionId, string url, string secretoFirma, string evento, string payload, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        SuscripcionId = suscripcionId;
        Url = url;
        SecretoFirma = secretoFirma;
        Evento = evento;
        Payload = payload;
        Estado = EstadoEntrega.Pendiente;
        Intentos = 0;
        ProximoIntento = ahora;
        CreadoEn = ahora;
    }

    public Guid SuscripcionId { get; private set; }

    public string Url { get; private set; }

    /// <summary>Secreto de firma copiado de la suscripción (para no depender de ella al enviar).</summary>
    public string SecretoFirma { get; private set; }

    public string Evento { get; private set; }

    public string Payload { get; private set; }

    public EstadoEntrega Estado { get; private set; }

    public int Intentos { get; private set; }

    public DateTimeOffset ProximoIntento { get; private set; }

    public string? UltimaRespuesta { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? EntregadaEn { get; private set; }

    public static EntregaWebhook Crear(Guid empresaId, SuscripcionWebhook suscripcion, string evento, string payload, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(suscripcion);
        ArgumentNullException.ThrowIfNull(reloj);
        return new EntregaWebhook(Guid.NewGuid(), empresaId, suscripcion.Id, suscripcion.Url, suscripcion.Secreto, evento, payload, reloj.AhoraUtc);
    }

    /// <summary>Firma HMAC-SHA256 (hex) del cuerpo con el secreto de la suscripción. La cabecera <c>X-Alxor-Firma</c> la transporta.</summary>
    public string Firma()
    {
        var mac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretoFirma));
        return Convert.ToHexString(mac.ComputeHash(Encoding.UTF8.GetBytes(Payload))).ToLowerInvariant();
    }

    public void MarcarEntregada(int codigoHttp, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Estado = EstadoEntrega.Entregada;
        Intentos++;
        UltimaRespuesta = "HTTP " + codigoHttp.ToString(CultureInfo.InvariantCulture);
        EntregadaEn = reloj.AhoraUtc;
    }

    /// <summary>Registra un fallo: incrementa el intento y reprograma con backoff, o marca fallida al agotar los intentos.</summary>
    public void RegistrarFallo(string descripcion, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Intentos++;
        UltimaRespuesta = (descripcion ?? string.Empty).Length > 300 ? descripcion![..300] : descripcion;
        if (Intentos >= MaximoIntentos)
        {
            Estado = EstadoEntrega.Fallida;
            return;
        }

        // Backoff exponencial: 1, 2, 4, 8 minutos…
        var minutos = Math.Pow(2, Intentos - 1);
        ProximoIntento = reloj.AhoraUtc.AddMinutes(minutos);
    }
}

/// <summary>Catálogo de eventos públicos que se pueden entregar por webhook (nombre estable de la API).</summary>
public static class EventosIntegracion
{
    public const string FacturaEmitida = "factura.emitida";
    public const string CobroRegistrado = "cobro.registrado";
    public const string GastoRegistrado = "gasto.registrado";
    public const string ClienteCreado = "cliente.creado";
    public const string ProveedorCreado = "proveedor.creado";
    public const string ProductoCreado = "producto.creado";

    public static readonly IReadOnlySet<string> Todos = new HashSet<string>(StringComparer.Ordinal)
    {
        FacturaEmitida, CobroRegistrado, GastoRegistrado, ClienteCreado, ProveedorCreado, ProductoCreado,
    };
}
