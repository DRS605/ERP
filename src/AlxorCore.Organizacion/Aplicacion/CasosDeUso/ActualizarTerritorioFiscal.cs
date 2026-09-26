using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Parámetro: territorio fiscal de la empresa (Península y Baleares → IVA; Canarias → IGIC).</summary>
public sealed record TerritorioFiscalComando(TerritorioFiscal TerritorioFiscal);

/// <summary>Caso de uso: fijar el territorio fiscal de la empresa activa.</summary>
public sealed class ActualizarTerritorioFiscal
{
    private readonly IRepositorioEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarTerritorioFiscal(IRepositorioEmpresas empresas, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<EmpresaDto>> EjecutarAsync(Guid empresaId, TerritorioFiscalComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (!Enum.IsDefined(comando.TerritorioFiscal))
        {
            return Resultado.Fallo<EmpresaDto>(Error.Validacion("empresa.territorio_invalido", "El territorio fiscal no es válido (Comun o Canarias)."));
        }

        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<EmpresaDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        empresa.EstablecerTerritorioFiscal(comando.TerritorioFiscal, _reloj);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EmpresaDto.Desde(empresa));
    }
}
