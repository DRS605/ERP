using System.Text.Json;
using System.Text.Json.Serialization;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Repositorio de la bandeja de salida (outbox).</summary>
public interface IRepositorioSalida
{
    void Agregar(MensajeSalida mensaje);

    /// <summary>Mensajes pendientes de despachar (de la empresa activa), en orden de creación.</summary>
    Task<IReadOnlyList<MensajeSalida>> PendientesAsync(int maximo, CancellationToken ct = default);
}

/// <summary>Serialización compartida de la bandeja de salida (enum como texto).</summary>
public static class SalidaJson
{
    public static readonly JsonSerializerOptions Opciones = new()
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static string Serializar(DocumentoContabilizable doc) => JsonSerializer.Serialize(doc, Opciones);

    public static DocumentoContabilizable? DeserializarContabilizacion(string carga) =>
        JsonSerializer.Deserialize<DocumentoContabilizable>(carga, Opciones);
}

/// <summary>
/// Encola en la bandeja de salida (outbox) el documento a contabilizar, dentro de la unidad de trabajo
/// del que lo llama (misma transacción que la factura). No lo despacha: eso lo hace el despachador.
/// </summary>
public sealed class EncolarSalida
{
    private readonly IRepositorioSalida _salida;
    private readonly IReloj _reloj;

    public EncolarSalida(IRepositorioSalida salida, IReloj reloj)
    {
        _salida = salida;
        _reloj = reloj;
    }

    public void Contabilizacion(Guid empresaId, DocumentoContabilizable documento)
    {
        ArgumentNullException.ThrowIfNull(documento);
        _salida.Agregar(MensajeSalida.Crear(empresaId, MensajeSalida.TipoContabilizacion, SalidaJson.Serializar(documento), _reloj.AhoraUtc));
    }
}

/// <summary>
/// Despacha los mensajes pendientes de la bandeja de salida: ejecuta su efecto (encolar la
/// contabilización) y los marca como procesados. Es idempotente y con reintento: si el efecto falla,
/// el mensaje queda pendiente y se reintentará. Se invoca tras confirmar la operación de origen y,
/// además, puede reejecutarse para recuperar mensajes atascados.
/// </summary>
public sealed class DespacharSalida
{
    private readonly IRepositorioSalida _salida;
    private readonly IColaContabilizacion _cola;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    public DespacharSalida(IRepositorioSalida salida, IColaContabilizacion cola, IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
        _salida = salida;
        _cola = cola;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Despacha hasta <paramref name="maximo"/> mensajes pendientes. Devuelve cuántos se procesaron.</summary>
    public async Task<int> EjecutarAsync(int maximo = 100, CancellationToken ct = default)
    {
        var pendientes = await _salida.PendientesAsync(maximo, ct).ConfigureAwait(false);
        var procesados = 0;
        foreach (var mensaje in pendientes)
        {
            try
            {
                if (mensaje.Tipo == MensajeSalida.TipoContabilizacion)
                {
                    var doc = SalidaJson.DeserializarContabilizacion(mensaje.Carga);
                    if (doc is not null)
                    {
                        await _cola.EncolarAsync(mensaje.EmpresaId, doc, ct).ConfigureAwait(false);
                    }
                }

                mensaje.MarcarProcesado(_reloj.AhoraUtc);
                procesados++;
            }
            catch (Exception ex)
            {
                mensaje.RegistrarError(ex.Message);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return procesados;
    }
}
