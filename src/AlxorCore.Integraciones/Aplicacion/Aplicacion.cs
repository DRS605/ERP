using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Integraciones.Aplicacion;

// ---------------------------------------------------------------------------
// DTOs
// ---------------------------------------------------------------------------

/// <summary>Vista de una clave de API (sin el secreto, que solo se muestra al crearla).</summary>
public sealed record ClaveApiDto(Guid Id, string Nombre, string Prefijo, bool Activa, DateTimeOffset CreadoEn, DateTimeOffset? UltimoUsoEn)
{
    public static ClaveApiDto Desde(ClaveApi c) => new(c.Id, c.Nombre, c.Prefijo, c.Activa, c.CreadoEn, c.UltimoUsoEn);
}

/// <summary>Clave recién creada: incluye el secreto en claro, que se muestra una única vez.</summary>
public sealed record ClaveApiCreadaDto(Guid Id, string Nombre, string Prefijo, string Secreto);

/// <summary>Vista de una suscripción de webhook (el secreto solo se devuelve al crearla).</summary>
public sealed record SuscripcionWebhookDto(Guid Id, string Url, IReadOnlyList<string> Eventos, bool Activa, DateTimeOffset CreadoEn)
{
    public static SuscripcionWebhookDto Desde(SuscripcionWebhook s) =>
        new(s.Id, s.Url, s.Eventos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), s.Activa, s.CreadoEn);
}

/// <summary>Suscripción recién creada: incluye el secreto de firma (mostrar una única vez).</summary>
public sealed record SuscripcionWebhookCreadaDto(Guid Id, string Url, IReadOnlyList<string> Eventos, string Secreto);

/// <summary>Vista de un intento de entrega de webhook.</summary>
public sealed record EntregaWebhookDto(
    Guid Id, Guid SuscripcionId, string Evento, string Estado, int Intentos,
    DateTimeOffset ProximoIntento, string? UltimaRespuesta, DateTimeOffset CreadoEn, DateTimeOffset? EntregadaEn)
{
    public static EntregaWebhookDto Desde(EntregaWebhook e) => new(
        e.Id, e.SuscripcionId, e.Evento, e.Estado.ToString(), e.Intentos, e.ProximoIntento, e.UltimaRespuesta, e.CreadoEn, e.EntregadaEn);
}

// ---------------------------------------------------------------------------
// Puertos
// ---------------------------------------------------------------------------

/// <summary>Unidad de trabajo del módulo Integraciones.</summary>
public interface IUnidadDeTrabajoIntegraciones : IUnidadDeTrabajo;

/// <summary>Repositorio de claves de API.</summary>
public interface IRepositorioClavesApi
{
    void Agregar(ClaveApi clave);

    Task<ClaveApi?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ClaveApi>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Busca una clave por el hash de su secreto <b>sin filtrar por empresa</b> (autenticación previa al tenant).</summary>
    Task<ClaveApi?> ObtenerPorHashAsync(string hashSecreto, CancellationToken ct = default);
}

/// <summary>Repositorio de suscripciones de webhook.</summary>
public interface IRepositorioSuscripciones
{
    void Agregar(SuscripcionWebhook suscripcion);

    Task<SuscripcionWebhook?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<SuscripcionWebhook>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<IReadOnlyList<SuscripcionWebhook>> ListarActivasPorEventoAsync(Guid empresaId, string evento, CancellationToken ct = default);
}

/// <summary>Repositorio del buzón de salida de entregas de webhook.</summary>
public interface IRepositorioEntregas
{
    void Agregar(EntregaWebhook entrega);

    Task<IReadOnlyList<EntregaWebhook>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Entregas pendientes de la empresa cuyo próximo intento ya venció.</summary>
    Task<IReadOnlyList<EntregaWebhook>> PendientesAsync(Guid empresaId, DateTimeOffset ahora, CancellationToken ct = default);

    /// <summary>Empresas con alguna entrega pendiente y vencida (para el proceso en segundo plano; sin filtro de empresa).</summary>
    Task<IReadOnlyList<Guid>> EmpresasConPendientesAsync(DateTimeOffset ahora, CancellationToken ct = default);
}

/// <summary>Resultado del envío HTTP de un webhook.</summary>
public sealed record ResultadoEnvioWebhook(bool Correcto, int CodigoHttp, string Detalle);

/// <summary>Puerto del cliente HTTP que entrega los webhooks (adaptador en infraestructura; sustituible en pruebas).</summary>
public interface IClienteHttpWebhook
{
    Task<ResultadoEnvioWebhook> EnviarAsync(string url, string evento, Guid entregaId, string firma, string payload, CancellationToken ct = default);
}

/// <summary>Cola de webhooks: encola una entrega por cada suscripción activa al evento. La usa el publicador de eventos.</summary>
public interface IColaWebhooks
{
    Task EncolarAsync(Guid empresaId, string evento, string payload, CancellationToken ct = default);
}

// ---------------------------------------------------------------------------
// Casos de uso — Claves de API
// ---------------------------------------------------------------------------

/// <summary>Crea una clave de API para la empresa activa y devuelve el secreto (una sola vez).</summary>
public sealed class CrearClaveApi
{
    private readonly IRepositorioClavesApi _claves;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public CrearClaveApi(IRepositorioClavesApi claves, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _claves = claves;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<ClaveApiCreadaDto>> EjecutarAsync(Guid empresaId, string? nombre, CancellationToken ct = default)
    {
        var creada = ClaveApi.Crear(empresaId, nombre, _reloj);
        if (creada.EsFallo)
        {
            return Resultado.Fallo<ClaveApiCreadaDto>(creada.Error);
        }

        var (clave, secreto) = creada.Valor;
        _claves.Agregar(clave);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ClaveApiCreadaDto(clave.Id, clave.Nombre, clave.Prefijo, secreto));
    }
}

/// <summary>Lista las claves de API de la empresa activa.</summary>
public sealed class ListarClavesApi
{
    private readonly IRepositorioClavesApi _claves;

    public ListarClavesApi(IRepositorioClavesApi claves) => _claves = claves;

    public async Task<IReadOnlyList<ClaveApiDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _claves.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(ClaveApiDto.Desde).ToList();
}

/// <summary>Revoca una clave de API.</summary>
public sealed class RevocarClaveApi
{
    private readonly IRepositorioClavesApi _claves;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public RevocarClaveApi(IRepositorioClavesApi claves, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _claves = claves;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado> EjecutarAsync(Guid claveId, CancellationToken ct = default)
    {
        var clave = await _claves.ObtenerPorIdAsync(claveId, ct).ConfigureAwait(false);
        if (clave is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("clave_api.no_encontrada", "La clave no existe."));
        }

        clave.Revocar(_reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Autentica una clave de API por su secreto y devuelve la empresa a la que pertenece.</summary>
public sealed class AutenticarClaveApi
{
    private readonly IRepositorioClavesApi _claves;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public AutenticarClaveApi(IRepositorioClavesApi claves, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _claves = claves;
        _uow = uow;
        _reloj = reloj;
    }

    /// <summary>Devuelve la empresa de la clave válida y activa, o null si el secreto no corresponde a ninguna clave utilizable.</summary>
    public async Task<Guid?> EjecutarAsync(string? secreto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(secreto))
        {
            return null;
        }

        var clave = await _claves.ObtenerPorHashAsync(ClaveApi.Hash(secreto.Trim()), ct).ConfigureAwait(false);
        if (clave is null || !clave.Activa)
        {
            return null;
        }

        clave.RegistrarUso(_reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return clave.EmpresaId;
    }
}

// ---------------------------------------------------------------------------
// Casos de uso — Webhooks
// ---------------------------------------------------------------------------

/// <summary>Crea una suscripción de webhook y devuelve el secreto de firma (una sola vez).</summary>
public sealed class CrearSuscripcionWebhook
{
    private readonly IRepositorioSuscripciones _suscripciones;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public CrearSuscripcionWebhook(IRepositorioSuscripciones suscripciones, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _suscripciones = suscripciones;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<SuscripcionWebhookCreadaDto>> EjecutarAsync(Guid empresaId, string? url, IReadOnlyCollection<string>? eventos, CancellationToken ct = default)
    {
        var creada = SuscripcionWebhook.Crear(empresaId, url, eventos, _reloj);
        if (creada.EsFallo)
        {
            return Resultado.Fallo<SuscripcionWebhookCreadaDto>(creada.Error);
        }

        var s = creada.Valor;
        _suscripciones.Agregar(s);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new SuscripcionWebhookCreadaDto(
            s.Id, s.Url, s.Eventos.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries), s.Secreto));
    }
}

/// <summary>Lista las suscripciones de webhook de la empresa activa.</summary>
public sealed class ListarSuscripcionesWebhook
{
    private readonly IRepositorioSuscripciones _suscripciones;

    public ListarSuscripcionesWebhook(IRepositorioSuscripciones suscripciones) => _suscripciones = suscripciones;

    public async Task<IReadOnlyList<SuscripcionWebhookDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _suscripciones.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(SuscripcionWebhookDto.Desde).ToList();
}

/// <summary>Elimina (desactiva) una suscripción de webhook.</summary>
public sealed class EliminarSuscripcionWebhook
{
    private readonly IRepositorioSuscripciones _suscripciones;
    private readonly IUnidadDeTrabajoIntegraciones _uow;

    public EliminarSuscripcionWebhook(IRepositorioSuscripciones suscripciones, IUnidadDeTrabajoIntegraciones uow)
    {
        _suscripciones = suscripciones;
        _uow = uow;
    }

    public async Task<Resultado> EjecutarAsync(Guid suscripcionId, CancellationToken ct = default)
    {
        var s = await _suscripciones.ObtenerPorIdAsync(suscripcionId, ct).ConfigureAwait(false);
        if (s is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("webhook.no_encontrado", "La suscripción no existe."));
        }

        s.Desactivar();
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Lista las entregas de webhook de la empresa activa (buzón de salida).</summary>
public sealed class ListarEntregasWebhook
{
    private readonly IRepositorioEntregas _entregas;

    public ListarEntregasWebhook(IRepositorioEntregas entregas) => _entregas = entregas;

    public async Task<IReadOnlyList<EntregaWebhookDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _entregas.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(EntregaWebhookDto.Desde).ToList();
}

/// <summary>
/// Cola de webhooks: al recibir un evento, crea una entrega pendiente por cada suscripción activa a
/// ese evento. Persiste con su propia unidad de trabajo (se invoca tras confirmar la operación origen).
/// </summary>
public sealed class ColaWebhooks : IColaWebhooks
{
    private readonly IRepositorioSuscripciones _suscripciones;
    private readonly IRepositorioEntregas _entregas;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public ColaWebhooks(IRepositorioSuscripciones suscripciones, IRepositorioEntregas entregas, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _suscripciones = suscripciones;
        _entregas = entregas;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task EncolarAsync(Guid empresaId, string evento, string payload, CancellationToken ct = default)
    {
        var suscripciones = await _suscripciones.ListarActivasPorEventoAsync(empresaId, evento, ct).ConfigureAwait(false);
        if (suscripciones.Count == 0)
        {
            return;
        }

        foreach (var s in suscripciones)
        {
            _entregas.Agregar(EntregaWebhook.Crear(empresaId, s, evento, payload, _reloj));
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Procesa las entregas pendientes de una empresa: las envía y marca su resultado (con reintentos/backoff).</summary>
public sealed class ProcesarEntregasWebhook
{
    private readonly IRepositorioEntregas _entregas;
    private readonly IClienteHttpWebhook _cliente;
    private readonly IUnidadDeTrabajoIntegraciones _uow;
    private readonly IReloj _reloj;

    public ProcesarEntregasWebhook(IRepositorioEntregas entregas, IClienteHttpWebhook cliente, IUnidadDeTrabajoIntegraciones uow, IReloj reloj)
    {
        _entregas = entregas;
        _cliente = cliente;
        _uow = uow;
        _reloj = reloj;
    }

    /// <summary>Envía las entregas pendientes vencidas de la empresa. Devuelve cuántas se entregaron con éxito.</summary>
    public async Task<int> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var pendientes = await _entregas.PendientesAsync(empresaId, _reloj.AhoraUtc, ct).ConfigureAwait(false);
        if (pendientes.Count == 0)
        {
            return 0;
        }

        var entregadas = 0;
        foreach (var entrega in pendientes)
        {
            ResultadoEnvioWebhook resultado;
            try
            {
                resultado = await _cliente.EnviarAsync(entrega.Url, entrega.Evento, entrega.Id, entrega.Firma(), entrega.Payload, ct).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Un fallo de red no debe detener el resto de entregas.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                resultado = new ResultadoEnvioWebhook(false, 0, ex.Message);
            }

            if (resultado.Correcto)
            {
                entrega.MarcarEntregada(resultado.CodigoHttp, _reloj);
                entregadas++;
            }
            else
            {
                entrega.RegistrarFallo($"HTTP {resultado.CodigoHttp}: {resultado.Detalle}", _reloj);
            }
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return entregadas;
    }
}
