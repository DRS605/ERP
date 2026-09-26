using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Parámetro: edición y módulos adicionales que contrata la empresa.</summary>
public sealed record CambiarPlanComando(string Edicion, IReadOnlyList<string>? ModulosAdicionales);

/// <summary>Caso de uso: consultar el plan contratado de la empresa activa.</summary>
public sealed class ConsultarPlan
{
    private readonly IRepositorioEmpresas _empresas;

    public ConsultarPlan(IRepositorioEmpresas empresas) => _empresas = empresas;

    public async Task<Resultado<PlanDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        return empresa is null
            ? Resultado.Fallo<PlanDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."))
            : Resultado.Ok(PlanDto.Desde(empresa.Plan));
    }
}

/// <summary>
/// Caso de uso: cambiar el plan contratado de la empresa activa. El cambio se aplica a los tokens
/// que se emitan a partir de ahora (al volver a seleccionar la empresa).
/// </summary>
public sealed class CambiarPlan
{
    private readonly IRepositorioEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CambiarPlan(IRepositorioEmpresas empresas, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<PlanDto>> EjecutarAsync(Guid empresaId, CambiarPlanComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<PlanDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        var cambio = empresa.CambiarPlan(comando.Edicion, comando.ModulosAdicionales, _reloj);
        if (cambio.EsFallo)
        {
            return Resultado.Fallo<PlanDto>(cambio.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlanDto.Desde(empresa.Plan));
    }
}
