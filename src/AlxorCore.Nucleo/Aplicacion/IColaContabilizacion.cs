namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>Sentido contable de un documento: venta (ingreso) o compra (gasto).</summary>
public enum SentidoContable
{
    Venta = 1,
    Compra = 2,
}

/// <summary>
/// Datos de un documento que debe contabilizarse (factura emitida, gasto, factura recibida, factura
/// de compra). Los módulos de negocio los envían a la <see cref="IColaContabilizacion"/> sin conocer
/// el módulo de Contabilidad.
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
    string? TipoTercero = null);

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
