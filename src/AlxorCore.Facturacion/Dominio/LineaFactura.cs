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
        // Se redondea a la precisión con la que se guarda cada columna ANTES de calcular: así la línea
        // guardada reproduce exactamente su base y su cuota (la base de datos lo comprueba).
        Cantidad = Math.Round(datos.Cantidad, 3, MidpointRounding.AwayFromZero);
        PrecioUnitario = Math.Round(datos.PrecioUnitario, 4, MidpointRounding.AwayFromZero);
        CosteUnitario = Math.Round(datos.CosteUnitario, 4, MidpointRounding.AwayFromZero);
        PorcentajeDescuento = Redondeo.Dos(datos.PorcentajeDescuento);
        CodigoIva = datos.CodigoIva;
        PorcentajeIva = Redondeo.Dos(datos.PorcentajeIva);
        PorcentajeRecargo = Redondeo.Dos(datos.PorcentajeRecargo);

        Conceptos = datos.Conceptos?.ToList() ?? [];
        var enDivisa = datos.PrecioDivisa is not null && datos.TasaCambio is not null;
        if (enDivisa && Conceptos.Any(c => c.ImporteDivisa is null))
        {
            // Conceptos calculados en la divisa: su importe pasa a la divisa y queda su contravalor en euros.
            Conceptos = ConceptosLinea.AEuros(Conceptos, datos.TasaCambio!.Value);
        }

        ImporteConceptos = ConceptosLinea.SumaPrecio(Conceptos);
        CosteConceptos = ConceptosLinea.SumaCoste(Conceptos);
        SuplidosConceptos = ConceptosLinea.SumaSuplidos(Conceptos);

        if (datos.PrecioDivisa is { } precioDivisa && datos.TasaCambio is { } tasa)
        {
            // Factura en divisa: manda el importe en la divisa (el pactado); los euros son su contravalor al tipo de la
            // factura. El precio en euros es orientativo (la base no sale de él, sino de la base en divisa). Los conceptos
            // suman su importe en la divisa a la base en divisa y su contravalor a la de euros.
            PrecioDivisa = Math.Round(precioDivisa, 4, MidpointRounding.AwayFromZero);
            var brutaDivisa = CalcularBaseBruta(Cantidad, PrecioDivisa.Value, PorcentajeDescuento);
            BaseDivisa = brutaDivisa + ConceptosLinea.SumaPrecioDivisa(Conceptos);
            PrecioUnitario = Math.Round(PrecioDivisa.Value * tasa, 4, MidpointRounding.AwayFromZero);
            Base = Redondeo.Dos(brutaDivisa * tasa) + ImporteConceptos;
        }
        else
        {
            Base = CalcularBaseBruta(Cantidad, PrecioUnitario, PorcentajeDescuento) + ImporteConceptos;
        }

        CuotaIva = Redondeo.Dos(Base * PorcentajeIva / 100m);
        CuotaRecargo = Redondeo.Dos(Base * PorcentajeRecargo / 100m);
        CuentaContable = string.IsNullOrWhiteSpace(datos.CuentaContable) ? null : datos.CuentaContable.Trim();
        AnticipoId = datos.AnticipoId;
        AlbaranVentaId = datos.AlbaranVentaId;
        EnvaseProductoId = datos.EnvaseProductoId;
    }

    /// <summary>
    /// Cuenta de la base de esta línea, si no es la de ventas de la regla: 438 en la factura de un anticipo y en la línea
    /// que lo descuenta de la factura final.
    /// </summary>
    public string? CuentaContable { get; private set; }

    /// <summary>Factura en divisa: precio unitario en la divisa (null en euros).</summary>
    public decimal? PrecioDivisa { get; private set; }

    /// <summary>Factura en divisa: base de la línea en la divisa (cantidad × precio − descuento + conceptos de importe).</summary>
    public decimal? BaseDivisa { get; private set; }

    /// <summary>Factura en divisa: suplidos de la línea en la divisa (no se guarda: sale de sus conceptos).</summary>
    public decimal SuplidosDivisa => ConceptosLinea.SumaSuplidosDivisa(Conceptos);

    /// <summary>Anticipo facturado que descuenta esta línea (base e impuesto en negativo) en la factura final.</summary>
    public Guid? AnticipoId { get; private set; }

    /// <summary>Albarán de venta que factura esta línea (la salida de existencias la hizo el albarán).</summary>
    public Guid? AlbaranVentaId { get; private set; }

    /// <summary>Envase de la línea (un artículo): decide las reglas de conceptos por envase.</summary>
    public Guid? EnvaseProductoId { get; private set; }

    /// <summary>Número de la línea en la factura (1, 2, 3…): el orden en que se emitió, en pantalla, PDF y registros.</summary>
    public int Orden { get; internal set; }

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

    /// <summary>Conceptos de línea aplicados (copia de cada concepto con su importe).</summary>
    public IReadOnlyList<ConceptoAplicado> Conceptos { get; private set; } = [];

    /// <summary>Suma de los conceptos que cambian el importe (forma parte de la base).</summary>
    public decimal ImporteConceptos { get; private set; }

    /// <summary>Suma de los conceptos después de la base (suplidos, fianzas): van al total de la factura, sin impuesto.</summary>
    public decimal SuplidosConceptos { get; private set; }

    /// <summary>Suma de los conceptos que solo cambian el coste (no está en la factura).</summary>
    public decimal CosteConceptos { get; private set; }

    /// <summary>Importe de la línea antes de conceptos: cantidad × precio − descuento, redondeado como se guarda.</summary>
    public static decimal CalcularBaseBruta(decimal cantidad, decimal precio, decimal porcentajeDescuento) =>
        Redondeo.Dos(Math.Round(cantidad, 3, MidpointRounding.AwayFromZero) * Math.Round(precio, 4, MidpointRounding.AwayFromZero)
            * (1 - (Redondeo.Dos(porcentajeDescuento) / 100m)));

    /// <summary>Base imponible de la línea (cantidad × precio − descuento + conceptos que cambian el importe).</summary>
    public decimal Base { get; private set; }

    /// <summary>Cuota de IVA de la línea.</summary>
    public decimal CuotaIva { get; private set; }

    /// <summary>Cuota de recargo de equivalencia de la línea (0 si no aplica).</summary>
    public decimal CuotaRecargo { get; private set; }

    /// <summary>Coste total de la línea (coste unitario × cantidad + conceptos de coste).</summary>
    public decimal CosteTotal => Redondeo.Dos(CosteUnitario * Cantidad) + CosteConceptos;

    /// <summary>Margen comercial de la línea (base de venta − coste total).</summary>
    public decimal Margen => Redondeo.Dos(Base - CosteTotal);
}
