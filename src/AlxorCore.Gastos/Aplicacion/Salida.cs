using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Gastos.Aplicacion;

/// <summary>Repositorio de la bandeja de salida (outbox) del módulo Gastos.</summary>
public interface IRepositorioSalidaGastos
{
    void Agregar(MensajeSalida mensaje);

    Task<IReadOnlyList<MensajeSalida>> PendientesAsync(int maximo, CancellationToken ct = default);
}

/// <summary>Encola en la bandeja de salida (dentro de la unidad de trabajo del gasto) su contabilización.</summary>
public sealed class EncolarSalidaGastos
{
    private readonly IRepositorioSalidaGastos _salida;
    private readonly IReloj _reloj;

    public EncolarSalidaGastos(IRepositorioSalidaGastos salida, IReloj reloj)
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

/// <summary>Despacha los mensajes pendientes de la bandeja de salida de Gastos (idempotente, con reintento).</summary>
public sealed class DespacharSalidaGastos
{
    private readonly IRepositorioSalidaGastos _salida;
    private readonly IColaContabilizacion _cola;
    private readonly IUnidadDeTrabajoGastos _unidad;
    private readonly IReloj _reloj;

    public DespacharSalidaGastos(IRepositorioSalidaGastos salida, IColaContabilizacion cola, IUnidadDeTrabajoGastos unidad, IReloj reloj)
    {
        _salida = salida;
        _cola = cola;
        _unidad = unidad;
        _reloj = reloj;
    }

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
