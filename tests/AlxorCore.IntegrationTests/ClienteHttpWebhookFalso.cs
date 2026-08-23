using System.Collections.Concurrent;
using AlxorCore.Integraciones.Aplicacion;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Cliente HTTP de webhooks falso para las pruebas: no sale a la red, registra cada envío y devuelve
/// éxito (HTTP 200). Permite ejercitar la entrega extremo a extremo de forma determinista.
/// </summary>
public sealed class ClienteHttpWebhookFalso : IClienteHttpWebhook
{
    public sealed record Envio(string Url, string Evento, Guid EntregaId, string Firma, string Payload);

    public ConcurrentBag<Envio> Enviados { get; } = [];

    public Task<ResultadoEnvioWebhook> EnviarAsync(string url, string evento, Guid entregaId, string firma, string payload, CancellationToken ct = default)
    {
        Enviados.Add(new Envio(url, evento, entregaId, firma, payload));
        return Task.FromResult(new ResultadoEnvioWebhook(true, 200, "OK"));
    }
}
