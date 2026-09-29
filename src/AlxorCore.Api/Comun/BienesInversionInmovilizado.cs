using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Informes.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Los bienes de inversión para la regularización de la prorrata: los inmovilizados con impuesto soportado.</summary>
public sealed class BienesInversionInmovilizado : IConsultaBienesInversion
{
    private readonly IRepositorioInmovilizado _inmovilizados;

    public BienesInversionInmovilizado(IRepositorioInmovilizado inmovilizados) => _inmovilizados = inmovilizados;

    public async Task<IReadOnlyList<BienInversion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _inmovilizados.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(i => i.CuotaImpuestoSoportada is not null)
            .Select(i => new BienInversion(i.Id, i.Codigo, i.Descripcion, i.FechaAdquisicion, i.FechaAlta, i.CuotaImpuestoSoportada!.Value,
                i.PorcentajeDeduccionInicial ?? 100, i.AniosRegularizacion, i.FechaBaja, i.EnPeriodoRegularizacion, i.Regularizacion))
            .ToList();
}
