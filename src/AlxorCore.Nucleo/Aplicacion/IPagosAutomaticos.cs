namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Puerto para registrar el cobro/pago total de un documento en el acto (formas de pago «ya pagada»).
/// Lo implementa Tesorería; lo consumen Facturación (ventas) y Gastos (compras) al emitir/registrar un
/// documento cuya forma de pago marca el registro automático del pago.
/// </summary>
public interface IPagosAutomaticos
{
    /// <summary>Registra el cobro total de una factura/ticket de venta en la fecha indicada.</summary>
    Task RegistrarCobroTotalAsync(Guid empresaId, Guid facturaId, decimal importe, DateOnly fecha, CancellationToken ct = default);

    /// <summary>Registra el pago total de un gasto/factura recibida en la fecha indicada.</summary>
    Task RegistrarPagoTotalAsync(Guid empresaId, Guid gastoId, decimal importe, DateOnly fecha, CancellationToken ct = default);
}
