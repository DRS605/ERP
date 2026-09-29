using AlxorCore.Agro.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Comun;

/// <summary>Adaptador de <see cref="IImpuestoEmpresaAgro"/>: el impuesto indirecto del territorio fiscal de la empresa.</summary>
public sealed class ImpuestoEmpresaAgro : IImpuestoEmpresaAgro
{
    private readonly IConsultaEmpresas _empresas;

    public ImpuestoEmpresaAgro(IConsultaEmpresas empresas) => _empresas = empresas;

    public async Task<TipoImpuesto> ImpuestoAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
}
