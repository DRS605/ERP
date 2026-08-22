using AlxorCore.Nucleo.Aplicacion;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>
/// Implementa el puerto <see cref="IPagosAutomaticos"/>: registra el cobro/pago total de un documento
/// cuando su forma de pago marca el pago automático. Reutiliza los casos de uso de cobro/pago (que
/// aplican los invariantes de saldo: no sobrepago, estado derivado).
/// </summary>
public sealed class PagosAutomaticos : IPagosAutomaticos
{
    private readonly RegistrarCobro _cobro;
    private readonly RegistrarPago _pago;

    public PagosAutomaticos(RegistrarCobro cobro, RegistrarPago pago)
    {
        _cobro = cobro;
        _pago = pago;
    }

    public Task RegistrarCobroTotalAsync(Guid empresaId, Guid facturaId, decimal importe, DateOnly fecha, CancellationToken ct = default) =>
        _cobro.EjecutarAsync(empresaId, new RegistrarCobroComando(facturaId, importe, fecha, "Contado"), ct);

    public Task RegistrarPagoTotalAsync(Guid empresaId, Guid gastoId, decimal importe, DateOnly fecha, CancellationToken ct = default) =>
        _pago.EjecutarAsync(empresaId, new RegistrarPagoComando(gastoId, importe, fecha, "Contado"), ct);
}
