using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Parámetro: cómo actúa la empresa ante el exceso del límite de riesgo (avisar o bloquear).</summary>
public sealed record ControlRiesgoComando(ControlRiesgo ControlRiesgo);

/// <summary>Caso de uso: fijar el control de riesgo de la empresa activa.</summary>
public sealed class ActualizarControlRiesgo
{
    private readonly IRepositorioEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarControlRiesgo(IRepositorioEmpresas empresas, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<EmpresaDto>> EjecutarAsync(Guid empresaId, ControlRiesgoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<EmpresaDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        empresa.EstablecerControlRiesgo(comando.ControlRiesgo, _reloj);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EmpresaDto.Desde(empresa));
    }
}
