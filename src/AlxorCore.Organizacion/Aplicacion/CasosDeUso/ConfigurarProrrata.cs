using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Configuración de la prorrata de un ejercicio. <c>Regimen = null</c> la quita (se deduce el 100 %).</summary>
/// <remarks>Sin <c>Impuesto</c>, la del impuesto del territorio de la empresa (IVA, o IGIC en Canarias).</remarks>
public sealed record ConfigurarProrrataComando(RegimenProrrata? Regimen, int PorcentajeProvisional, AlxorCore.Nucleo.Comun.TipoImpuesto? Impuesto = null);

/// <summary>Caso de uso: fijar (o quitar) la prorrata de un ejercicio de la empresa activa.</summary>
public sealed class ConfigurarProrrata
{
    private readonly IRepositorioProrratas _prorratas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IRepositorioEmpresas _empresas;

    public ConfigurarProrrata(IRepositorioProrratas prorratas, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IRepositorioEmpresas empresas)
    {
        _prorratas = prorratas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _empresas = empresas;
    }

    public async Task<Resultado<ProrrataDto?>> EjecutarAsync(Guid empresaId, int ejercicio, ConfigurarProrrataComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var impuesto = comando.Impuesto
            ?? (await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false))?.TerritorioFiscal.ImpuestoIndirecto()
            ?? AlxorCore.Nucleo.Comun.TipoImpuesto.Iva;
        var actual = await _prorratas.ObtenerAsync(empresaId, ejercicio, impuesto, ct).ConfigureAwait(false);

        if (comando.Regimen is not { } regimen)
        {
            if (actual is not null)
            {
                _prorratas.Eliminar(actual);
                await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
            }

            return Resultado.Ok<ProrrataDto?>(null);
        }

        if (actual is null)
        {
            var nueva = ProrrataEjercicio.Crear(empresaId, ejercicio, regimen, comando.PorcentajeProvisional, impuesto);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<ProrrataDto?>(nueva.Error);
            }

            actual = nueva.Valor;
            _prorratas.Agregar(actual);
        }
        else
        {
            var cambio = actual.Cambiar(regimen, comando.PorcentajeProvisional);
            if (cambio.EsFallo)
            {
                return Resultado.Fallo<ProrrataDto?>(cambio.Error);
            }
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<ProrrataDto?>(new ProrrataDto(actual.Ejercicio, actual.Regimen, actual.PorcentajeProvisional, actual.Impuesto));
    }
}
