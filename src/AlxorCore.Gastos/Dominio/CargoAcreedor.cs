using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Gastos.Dominio;

/// <summary>
/// Un cargo de un documento (concepto de línea con acreedor: portes de un transportista, comisión de un comisionista…)
/// liquidado en la factura del acreedor (un gasto). Si el gasto se anula, el cargo vuelve a estar pendiente. Solo se
/// inserta: lo liquidado no se cambia.
/// </summary>
public sealed class CargoAcreedorLiquidado : RaizAgregadoEmpresa<Guid>
{
    private CargoAcreedorLiquidado(Guid id)
        : base(id, Guid.Empty)
    {
        Origen = string.Empty;
        Codigo = string.Empty;
    }

    public CargoAcreedorLiquidado(Guid empresaId, Guid gastoId, Guid acreedorId, string origen, Guid documentoId, int lineaOrden, Guid conceptoId,
        string codigo, decimal importe, DateTimeOffset ahora)
        : base(Guid.NewGuid(), empresaId)
    {
        GastoId = gastoId;
        AcreedorId = acreedorId;
        Origen = origen;
        DocumentoId = documentoId;
        LineaOrden = lineaOrden;
        ConceptoId = conceptoId;
        Codigo = codigo;
        Importe = importe;
        CreadoEn = ahora;
    }

    public Guid GastoId { get; private set; }

    public Guid AcreedorId { get; private set; }

    /// <summary>FacturaVenta, AlbaranVenta o PedidoCompra.</summary>
    public string Origen { get; private set; }

    public Guid DocumentoId { get; private set; }

    public int LineaOrden { get; private set; }

    public Guid ConceptoId { get; private set; }

    public string Codigo { get; private set; }

    public decimal Importe { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }
}
