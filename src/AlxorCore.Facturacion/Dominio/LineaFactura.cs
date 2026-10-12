using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>
/// Línea de una factura. Todos sus importes se calculan y se "congelan" al emitir. Pertenece al
/// agregado <see cref="Factura"/> y no se modifica de forma independiente.
/// </summary>
public sealed class LineaFactura : EntidadBase<Guid>
{
    private LineaFactura(Guid id)
        : base(id)
    {
        Descripcion = null!;
        CodigoIva = null!;
    }

    internal LineaFactura(Guid empresaId, NuevaLinea datos)
        : base(Guid.NewGuid())
    {
        EmpresaId = empresaId;
        ProductoId = datos.ProductoId;
        Descripcion = datos.Descripcion.Trim();
        Cantidad = datos.Cantidad;
        PrecioUnitario = datos.PrecioUnitario;
        CosteUnitario = datos.CosteUnitario;
        PorcentajeDescuento = datos.PorcentajeDescuento;
        CodigoIva = datos.CodigoIva;
        PorcentajeIva = datos.PorcentajeIva;
        PorcentajeRecargo = datos.PorcentajeRecargo;

        if (datos.PreciosConIvaIncluido)
        {
            // Tickets de TPV: el precio de venta es CON IVA (precio de estantería). El importe bruto de
            // la línea es el que manda y se "desglosa" hacia atrás, de modo que la suma de los totales de
            // línea coincide exactamente con lo que paga el cliente (sin el céntimo de descuadre).
            var brutoUnitario = PrecioUnitario * (1 + (PorcentajeIva / 100m));
            var brutoLinea = Redondeo.Dos(Cantidad * brutoUnitario * (1 - (PorcentajeDescuento / 100m)));
            Base = Redondeo.Dos(brutoLinea / (1 + (PorcentajeIva / 100m)));
            CuotaIva = brutoLinea - Base;
            CuotaRecargo = Redondeo.Dos(Base * PorcentajeRecargo / 100m);
        }
        else
        {
            Base = Redondeo.Dos(Cantidad * PrecioUnitario * (1 - (PorcentajeDescuento / 100m)));
            CuotaIva = Redondeo.Dos(Base * PorcentajeIva / 100m);
            CuotaRecargo = Redondeo.Dos(Base * PorcentajeRecargo / 100m);
        }
    }

    /// <summary>Empresa (para el aislamiento multiempresa de la tabla de líneas).</summary>
    public Guid EmpresaId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    /// <summary>Coste/precio de compra unitario congelado al emitir (para el margen). 0 si no se conoce.</summary>
    public decimal CosteUnitario { get; private set; }

    public decimal PorcentajeDescuento { get; private set; }

    public string CodigoIva { get; private set; }

    public decimal PorcentajeIva { get; private set; }

    /// <summary>Porcentaje de recargo de equivalencia (0 si no aplica).</summary>
    public decimal PorcentajeRecargo { get; private set; }

    /// <summary>Base imponible de la línea (cantidad × precio − descuento).</summary>
    public decimal Base { get; private set; }

    /// <summary>Cuota de IVA de la línea.</summary>
    public decimal CuotaIva { get; private set; }

    /// <summary>Cuota de recargo de equivalencia de la línea (0 si no aplica).</summary>
    public decimal CuotaRecargo { get; private set; }

    /// <summary>Coste total de la línea (coste unitario × cantidad).</summary>
    public decimal CosteTotal => Redondeo.Dos(CosteUnitario * Cantidad);

    /// <summary>Margen comercial de la línea (base de venta − coste total).</summary>
    public decimal Margen => Redondeo.Dos(Base - CosteTotal);
}
