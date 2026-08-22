namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Consulta el <b>riesgo vivo</b> de un tercero: el importe pendiente de cobro (cliente) o de pago
/// (proveedor) de sus documentos. Lo implementa Tesorería (que conoce los saldos) y lo consumen
/// Facturación y Gastos para comparar con el límite de riesgo del tercero al emitir/registrar.
/// </summary>
public interface IConsultaRiesgo
{
    /// <summary>Suma pendiente de cobro de las facturas del cliente (sin incluir el documento en curso).</summary>
    Task<decimal> RiesgoVivoClienteAsync(Guid empresaId, Guid clienteId, CancellationToken ct = default);

    /// <summary>Suma pendiente de pago de los gastos del proveedor (sin incluir el documento en curso).</summary>
    Task<decimal> RiesgoVivoProveedorAsync(Guid empresaId, Guid proveedorId, CancellationToken ct = default);
}
