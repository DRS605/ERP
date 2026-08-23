using System.Globalization;
using System.Net.Http;
using System.Text;
using AlxorCore.Integraciones.Aplicacion;

namespace AlxorCore.Integraciones.Infraestructura;

/// <summary>
/// Adaptador que entrega un webhook por HTTP POST. Envía el cuerpo JSON con las cabeceras de firma
/// (<c>X-Alxor-Firma</c>, HMAC-SHA256 en hex), evento y entrega, para que el receptor verifique la
/// autenticidad. Un código 2xx se considera entrega correcta; cualquier otro, un fallo reintentable.
/// </summary>
internal sealed class ClienteHttpWebhook : IClienteHttpWebhook
{
    private readonly IHttpClientFactory _fabrica;

    public ClienteHttpWebhook(IHttpClientFactory fabrica) => _fabrica = fabrica;

    public async Task<ResultadoEnvioWebhook> EnviarAsync(string url, string evento, Guid entregaId, string firma, string payload, CancellationToken ct = default)
    {
        var cliente = _fabrica.CreateClient("webhooks");
        using var contenido = new StringContent(payload, Encoding.UTF8, "application/json");
        using var peticion = new HttpRequestMessage(HttpMethod.Post, url) { Content = contenido };
        peticion.Headers.TryAddWithoutValidation("X-Alxor-Firma", firma);
        peticion.Headers.TryAddWithoutValidation("X-Alxor-Evento", evento);
        peticion.Headers.TryAddWithoutValidation("X-Alxor-Entrega", entregaId.ToString("N"));

        using var respuesta = await cliente.SendAsync(peticion, ct).ConfigureAwait(false);
        var codigo = (int)respuesta.StatusCode;
        return new ResultadoEnvioWebhook(respuesta.IsSuccessStatusCode, codigo, respuesta.ReasonPhrase ?? codigo.ToString(CultureInfo.InvariantCulture));
    }
}
