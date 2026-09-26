using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Gastos.Dominio;
using AlxorCore.Informes.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador del puerto de Contabilidad <see cref="IDeduccionImpuesto"/>: aplica al contabilizar una
/// compra la prorrata del ejercicio (Organización) con las mismas reglas que el 303/420 (Informes).
/// </summary>
public sealed class DeduccionImpuestoProrrata : IDeduccionImpuesto
{
    private readonly IConsultaProrrata _prorratas;

    public DeduccionImpuestoProrrata(IConsultaProrrata prorratas) => _prorratas = prorratas;

    public async Task<decimal> CuotaDeducibleAsync(Guid empresaId, int ejercicio, decimal cuota, string? afectacion, CancellationToken ct = default)
    {
        var prorrata = await _prorratas.ObtenerAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        if (prorrata is null)
        {
            return cuota;
        }

        var destino = Enum.TryParse<AfectacionIva>(afectacion, out var a) ? a : AfectacionIva.Comun;
        var cuotas = new CuotasSoportadas(0m, cuota,
            destino == AfectacionIva.Comun ? cuota : 0m,
            destino == AfectacionIva.ConDerecho ? cuota : 0m,
            destino == AfectacionIva.SinDerecho ? cuota : 0m);
        return ReglaProrrata.Deducible(prorrata.Regimen, prorrata.PorcentajeProvisional, cuotas);
    }
}
