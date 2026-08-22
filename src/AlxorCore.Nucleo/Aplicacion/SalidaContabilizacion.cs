using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Serialización compartida de la carga de los mensajes de la bandeja de salida (outbox) que encolan
/// una contabilización. La reutilizan todos los módulos que tienen su propio outbox (Facturación,
/// Gastos…) para no duplicar el formato.
/// </summary>
public static class SalidaJson
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serializar(DocumentoContabilizable documento) => JsonSerializer.Serialize(documento, Opciones);

    public static DocumentoContabilizable? DeserializarContabilizacion(string carga) =>
        JsonSerializer.Deserialize<DocumentoContabilizable>(carga, Opciones);
}
