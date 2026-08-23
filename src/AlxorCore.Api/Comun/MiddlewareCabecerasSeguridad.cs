namespace AlxorCore.Api.Comun;

/// <summary>
/// Añade cabeceras de seguridad HTTP a todas las respuestas: evita el <i>sniffing</i> de tipos,
/// el enmarcado en <c>iframe</c> (clickjacking), fuga de <i>referer</i> y capacidades del navegador
/// no necesarias. No fija una CSP estricta porque la interfaz clásica usa scripts y estilos en línea;
/// la SPA (sin código en línea) podrá adoptar una CSP restrictiva más adelante.
/// </summary>
public sealed class MiddlewareCabecerasSeguridad
{
    private readonly RequestDelegate _siguiente;

    public MiddlewareCabecerasSeguridad(RequestDelegate siguiente) => _siguiente = siguiente;

    public Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);

        var cabeceras = contexto.Response.Headers;
        cabeceras["X-Content-Type-Options"] = "nosniff";
        cabeceras["X-Frame-Options"] = "DENY";
        cabeceras["Referrer-Policy"] = "strict-origin-when-cross-origin";
        cabeceras["Cross-Origin-Opener-Policy"] = "same-origin";
        // El TPV usa la cámara del propio origen para escanear; el resto de capacidades se deshabilitan.
        cabeceras["Permissions-Policy"] = "camera=(self), microphone=(), geolocation=(), payment=()";

        return _siguiente(contexto);
    }
}
