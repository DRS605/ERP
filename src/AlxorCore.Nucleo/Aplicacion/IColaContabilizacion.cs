namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>Sentido contable de un documento: venta (ingreso), compra (gasto), cobro o pago.</summary>
public enum SentidoContable
{
    Venta = 1,
    Compra = 2,

    /// <summary>Cobro de un cliente: tesorería (Debe) contra la cuenta del cliente (Haber).</summary>
    Cobro = 3,

    /// <summary>Pago a un proveedor: la cuenta del proveedor (Debe) contra tesorería (Haber).</summary>
    Pago = 4,
}

/// <summary>
/// Datos de un documento que debe contabilizarse (factura emitida, gasto, factura recibida, factura
/// de compra). Los módulos de negocio los envían a la <see cref="IColaContabilizacion"/> sin conocer
/// el módulo de Contabilidad. Con <c>Anulacion</c>, el documento anula otro ya encolado: se contabiliza con
/// el asiento inverso (contraasiento), con los mismos importes en positivo.
/// En cobros y pagos, <c>Total</c> es el importe; <c>CuentaTesoreria</c> la cuenta de caja o banco (572 por defecto)
/// y <c>CuentaTercero</c> sustituye a la del tercero cuando no es la de clientes o proveedores (438 en anticipos).
/// </summary>
public sealed record DocumentoContabilizable(
    SentidoContable Sentido,
    string OrigenTipo,
    Guid OrigenId,
    string Referencia,
    Guid? TerceroId,
    string TerceroNombre,
    DateOnly FechaDocumento,
    decimal BaseImponible,
    string CodigoIva,
    decimal CuotaIva,
    decimal PorcentajeIrpf,
    decimal RetencionIrpf,
    decimal Total,
    Guid? ProductoId = null,
    string? Familia = null,
    string? TipoTercero = null,
    string? Afectacion = null,
    Guid? ActividadNegocioId = null,
    bool Anulacion = false,
    string? CuentaTesoreria = null,
    string? CuentaTercero = null,
    IReadOnlyList<LineaContable>? Lineas = null);

/// <summary>
/// Línea de una factura recibida con varias bases: su base, tipo e impuesto, la parte deducible (antes de la prorrata),
/// el recargo de equivalencia soportado, si la cuota la autoliquida la empresa (inversión del sujeto pasivo o adquisición
/// intracomunitaria: se carga en la 472 y se abona en la 477) y la cuenta de gasto propia de la línea (null: la regla).
/// </summary>
public sealed record LineaContable(
    decimal Base,
    string CodigoIva,
    decimal Cuota,
    decimal CuotaDeducible,
    decimal Recargo = 0m,
    bool Autoliquidada = false,
    string? CuentaGasto = null);

/// <summary>
/// Cola de contabilización: recibe los documentos contabilizables y los deja <b>pendientes</b> de que
/// el contable los revise y confirme. Si la empresa tiene activada la contabilización automática, los
/// contabiliza en el acto. La implementa el módulo Contabilidad; la consumen Facturación, Gastos,
/// Recepción y Compras.
/// </summary>
public interface IColaContabilizacion
{
    Task EncolarAsync(Guid empresaId, DocumentoContabilizable documento, CancellationToken ct = default);
}
