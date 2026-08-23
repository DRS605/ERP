using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Api.Servicios;

/// <summary>Ajustes del proceso de entrega de webhooks en segundo plano.</summary>
public sealed class OpcionesWebhooks
{
    public const string Seccion = "Webhooks";

    /// <summary>Si está activo el proceso en segundo plano.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Espera inicial antes de la primera pasada tras arrancar.</summary>
    public TimeSpan RetardoInicial { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>Cada cuánto se revisan las entregas pendientes.</summary>
    public TimeSpan Intervalo { get; set; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Proceso en segundo plano que entrega los <b>webhooks pendientes</b> de todas las empresas. En cada
/// pasada descubre las empresas con entregas vencidas y, para cada una, abre su ámbito con la empresa
/// fijada (aislamiento multiempresa) y ejecuta el envío con reintentos y backoff. Tolerante a fallos:
/// un error en una empresa no detiene al resto.
/// </summary>
public sealed class ServicioWebhooks : BackgroundService
{
    private readonly IServiceScopeFactory _ambitos;
    private readonly IReloj _reloj;
    private readonly ILogger<ServicioWebhooks> _log;
    private readonly OpcionesWebhooks _opciones;

    public ServicioWebhooks(
        IServiceScopeFactory ambitos,
        IReloj reloj,
        ILogger<ServicioWebhooks> log,
        Microsoft.Extensions.Options.IOptions<OpcionesWebhooks> opciones)
    {
        _ambitos = ambitos;
        _reloj = reloj;
        _log = log;
        _opciones = opciones.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opciones.Activo)
        {
            return;
        }

        try
        {
            await Task.Delay(_opciones.RetardoInicial, stoppingToken).ConfigureAwait(false);
            using var temporizador = new PeriodicTimer(_opciones.Intervalo);
            do
            {
                await ProcesarTodasLasEmpresasAsync(stoppingToken).ConfigureAwait(false);
            }
            while (await temporizador.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Apagado normal.
        }
    }

    private async Task ProcesarTodasLasEmpresasAsync(CancellationToken ct)
    {
        IReadOnlyList<Guid> empresas;
        using (var ambito = _ambitos.CreateScope())
        {
            var repositorio = ambito.ServiceProvider.GetRequiredService<IRepositorioEntregas>();
            empresas = await repositorio.EmpresasConPendientesAsync(_reloj.AhoraUtc, ct).ConfigureAwait(false);
        }

        foreach (var empresaId in empresas)
        {
            try
            {
                using var ambito = _ambitos.CreateScope();
                ambito.ServiceProvider.GetRequiredService<IContextoEmpresaMutable>().Fijar(empresaId);
                var caso = ambito.ServiceProvider.GetRequiredService<ProcesarEntregasWebhook>();
                await caso.EjecutarAsync(empresaId, ct).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Un fallo en una empresa no debe tumbar el proceso ni afectar a las demás.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _log.LogError(ex, "Fallo al entregar webhooks de la empresa {EmpresaId}.", empresaId);
            }
        }
    }
}
