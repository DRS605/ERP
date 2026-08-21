using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Recepcion.Aplicacion;
using AlxorCore.Recepcion.Infraestructura;
using Microsoft.Extensions.Options;

namespace AlxorCore.Api.Servicios;

/// <summary>
/// Proceso en segundo plano que revisa periódicamente el buzón de correo de facturas de
/// proveedor y da de alta las nuevas en la bandeja de entrada de la empresa configurada. No
/// contabiliza nada: eso exige validación humana. Está apagado salvo que el buzón esté
/// configurado (<see cref="OpcionesBuzon.Activo"/> + credenciales).
/// </summary>
public sealed class ServicioBuzonProveedores : BackgroundService
{
    private readonly IServiceScopeFactory _ambitos;
    private readonly ILogger<ServicioBuzonProveedores> _log;
    private readonly OpcionesBuzon _opciones;

    public ServicioBuzonProveedores(
        IServiceScopeFactory ambitos,
        ILogger<ServicioBuzonProveedores> log,
        IOptions<OpcionesBuzon> opciones)
    {
        _ambitos = ambitos;
        _log = log;
        _opciones = opciones.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opciones.Configurado || _opciones.EmpresaId is null)
        {
            return;
        }

        var intervalo = TimeSpan.FromSeconds(Math.Max(60, _opciones.IntervaloSegundos));
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken).ConfigureAwait(false);
            using var temporizador = new PeriodicTimer(intervalo);
            do
            {
                await ProcesarAsync(_opciones.EmpresaId.Value, stoppingToken).ConfigureAwait(false);
            }
            while (await temporizador.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Apagado normal.
        }
    }

    private async Task ProcesarAsync(Guid empresaId, CancellationToken ct)
    {
        try
        {
            using var ambito = _ambitos.CreateScope();
            ambito.ServiceProvider.GetRequiredService<IContextoEmpresaMutable>().Fijar(empresaId);
            var caso = ambito.ServiceProvider.GetRequiredService<ProcesarBuzon>();
            var altas = await caso.EjecutarAsync(empresaId, ct).ConfigureAwait(false);
            if (altas > 0)
            {
                _log.LogInformation("Buzón de facturas: {Altas} factura(s) nueva(s) en la bandeja de entrada.", altas);
            }
        }
#pragma warning disable CA1031 // Un fallo del buzón no debe tumbar el proceso.
        catch (Exception ex)
#pragma warning restore CA1031
        {
            _log.LogError(ex, "Fallo al procesar el buzón de facturas de proveedor.");
        }
    }
}
