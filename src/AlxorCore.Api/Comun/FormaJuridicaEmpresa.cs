using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Comun;

/// <summary>Si la empresa es una sociedad o entidad, por su NIF: su contabilidad empieza en modo Completo.</summary>
public sealed class FormaJuridicaPorNif : IFormaJuridicaEmpresa
{
    private readonly IConsultaEmpresas _empresas;

    public FormaJuridicaPorNif(IConsultaEmpresas empresas) => _empresas = empresas;

    public async Task<bool> EsPersonaJuridicaAsync(Guid empresaId, CancellationToken ct = default) =>
        ModosContables.EsPersonaJuridica((await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.Nif);
}
