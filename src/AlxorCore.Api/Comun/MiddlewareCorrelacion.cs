namespace AlxorCore.Api.Comun;

/// <summary>
/// Asigna a cada petición un <b>identificador de correlación</b>: toma el de la cabecera
/// <c>X-Correlation-Id</c> si llega (para poder seguir una operación entre servicios) o genera uno.
/// Lo añade al ámbito de logging (así aparece en todas las trazas de la petición) y lo devuelve en la
/// respuesta, facilitando el diagnóstico en producción.
/// </summary>
public sealed class MiddlewareCorrelacion
{
    private const string Cabecera = "X-Correlation-Id";
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<MiddlewareCorrelacion> _log;

    public MiddlewareCorrelacion(RequestDelegate siguiente, ILogger<MiddlewareCorrelacion> log)
    {
        _siguiente = siguiente;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        ArgumentNullException.ThrowIfNull(contexto);

        var entrante = contexto.Request.Headers[Cabecera].ToString();
        var correlacion = string.IsNullOrWhiteSpace(entrante) ? Guid.NewGuid().ToString("N") : entrante.Trim();
        contexto.TraceIdentifier = correlacion;
        contexto.Response.Headers[Cabecera] = correlacion;

        using (_log.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlacion }))
        {
            await _siguiente(contexto).ConfigureAwait(false);
        }
    }
}
