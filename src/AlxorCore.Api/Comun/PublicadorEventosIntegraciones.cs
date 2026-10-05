using System.Text.Json;
using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using Microsoft.Extensions.DependencyInjection;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Publicador de eventos de dominio que, además de dejar traza en el log, <b>alimenta las
/// integraciones</b>: por cada evento con nombre público (p. ej. <c>factura.emitida</c>) encola una
/// entrega de webhook por cada suscripción activa de la empresa. Es la pieza que conecta el modelo de
/// dominio con la API y los webhooks sin acoplar los módulos emisores a las integraciones.
/// </summary>
public sealed class PublicadorEventosIntegraciones : IPublicadorEventos
{
    private static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IContextoEmpresa _contexto;
    private readonly IServiceProvider _servicios;
    private readonly ILogger<PublicadorEventosIntegraciones> _log;

    // La cola se resuelve de forma perezosa (no por constructor) para evitar un ciclo de dependencias:
    // este publicador lo inyectan los DbContext, y la cola depende a su vez del DbContext de Integraciones.
    public PublicadorEventosIntegraciones(IContextoEmpresa contexto, IServiceProvider servicios, ILogger<PublicadorEventosIntegraciones> log)
    {
        _contexto = contexto;
        _servicios = servicios;
        _log = log;
    }

    public async Task PublicarAsync(IReadOnlyCollection<IEventoDominio> eventos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(eventos);

        var empresaId = _contexto.EmpresaId;
        IColaWebhooks? cola = null;
        foreach (var evento in eventos)
        {
            _log.LogInformation("Evento de dominio: {Evento}", evento.GetType().Name);

            // Facturas a empresas del grupo: su espejo en la bandeja de la receptora (tras confirmar la factura).
            if (empresaId is { } emisora && evento is AlxorCore.Facturacion.Dominio.FacturaEmitida or AlxorCore.Facturacion.Dominio.FacturaAnulada
                    or AlxorCore.Facturacion.Dominio.AlbaranVentaEmitido or AlxorCore.Facturacion.Dominio.AlbaranVentaAnulado)
            {
                await _servicios.GetRequiredService<OperacionesIntragrupo>().ProcesarEventoAsync(emisora, evento, ct).ConfigureAwait(false);
            }

            // Reglas de alerta de evento (un fallo aquí no tumba la operación, que ya está guardada).
            if (empresaId is { } empresa && AlxorCore.Extensiones.Dominio.ReglaAlerta.Eventos.ContainsKey(evento.GetType().Name)
                && EventosAlertas.Describir(evento) is { } alerta)
            {
                try
                {
                    await _servicios.GetRequiredService<AlxorCore.Extensiones.Aplicacion.AlertasEmpresa>().RegistrarEventoAsync(empresa, evento.GetType().Name, alerta.Titulo,
                        alerta.Detalle, alerta.Entidad, alerta.EntidadId, alerta.EventoId, ct).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _log.LogWarning(ex, "No se pudo registrar la alerta del evento {Evento}", evento.GetType().Name);
                }
            }

            var nombre = NombrePublico(evento.GetType().Name);
            if (nombre is null || empresaId is null)
            {
                continue;
            }

            var payload = JsonSerializer.Serialize(
                new { evento = nombre, empresaId = empresaId.Value, ocurridoEn = evento.OcurridoEn, datos = (object)evento },
                Json);

            cola ??= _servicios.GetRequiredService<IColaWebhooks>();
            await cola.EncolarAsync(empresaId.Value, nombre, payload, ct).ConfigureAwait(false);
        }
    }

    /// <summary>Traduce el nombre del evento de dominio a su nombre público estable, o null si no se expone.</summary>
    private static string? NombrePublico(string tipoEvento) => tipoEvento switch
    {
        "FacturaEmitida" => EventosIntegracion.FacturaEmitida,
        "MovimientoRegistrado" => EventosIntegracion.CobroRegistrado,
        "GastoRegistrado" => EventosIntegracion.GastoRegistrado,
        "ClienteCreado" => EventosIntegracion.ClienteCreado,
        "ProveedorCreado" => EventosIntegracion.ProveedorCreado,
        "ProductoCreado" => EventosIntegracion.ProductoCreado,
        _ => null,
    };
}
