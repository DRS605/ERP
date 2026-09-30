using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Cómo se valora la venta en comisión.</summary>
public enum ModoLiquidacionComision
{
    /// <summary>El albarán se valora al precio neto (bruto menos comisión y gastos): se factura el líquido.</summary>
    PrecioNeto = 1,

    /// <summary>El albarán se valora al precio bruto de venta y la comisión y los gastos son una factura del comisionista.</summary>
    BrutoConFacturaGastos = 2,
}

public enum EstadoLiquidacionComision
{
    Borrador = 1,
    Confirmada = 2,
    Anulada = 3,
}

/// <summary>Lo que el comisionista descuenta de la venta.</summary>
public enum TipoGastoComision
{
    Comision = 1,
    Transporte = 2,
    Aduanas = 3,
    Manipulacion = 4,
    Frio = 5,
    Publicidad = 6,
    Otros = 9,
}

/// <summary>
/// Liquidación de la venta en comisión (<i>account sale</i>): el cliente vende la mercancía que se le envió a precio por
/// fijar y comunica lo vendido de cada albarán, el precio bruto al que lo vendió y lo que descuenta (su comisión, portes,
/// aduanas, manipulación…). Al confirmarla, los albaranes quedan valorados: al precio neto, o al bruto con una factura
/// de gastos del comisionista. Lo que se envió y no se vendió (merma, destrío en destino) baja el precio de lo enviado.
/// </summary>
public sealed class LiquidacionComision : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "LC";

    private readonly List<LineaLiquidacionComision> _lineas = [];
    private readonly List<GastoLiquidacionComision> _gastos = [];

    private LiquidacionComision(Guid id) : base(id, Guid.Empty) { ClienteNombre = null!; }

    private LiquidacionComision(Guid id, Guid empresaId) : base(id, empresaId) { ClienteNombre = null!; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int? Numero { get; private set; }

    public string? NumeroCompleto => Numero is { } n ? $"{Serie}-{Fecha.Year}-{n:D6}" : null;

    /// <summary>Número de la liquidación del cliente (su <i>account sale</i>).</summary>
    public string? ReferenciaCliente { get; private set; }

    public ModoLiquidacionComision Modo { get; private set; }

    public EstadoLiquidacionComision Estado { get; private set; }

    /// <summary>Proveedor que factura los gastos (la ficha de proveedor del comisionista), en el modo bruto.</summary>
    public Guid? ProveedorId { get; private set; }

    public string? CodigoIvaGastos { get; private set; }

    /// <summary>Factura de gastos del comisionista generada al confirmar (modo bruto).</summary>
    public Guid? GastoId { get; private set; }

    public string? Observaciones { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaLiquidacionComision> Lineas => _lineas;

    public IReadOnlyList<GastoLiquidacionComision> Gastos => _gastos;

    public decimal ImporteBruto => Redondeo.Dos(_lineas.Sum(l => l.ImporteBruto));

    public decimal TotalGastos => Redondeo.Dos(_gastos.Sum(g => g.Importe));

    public decimal ImporteNeto => Redondeo.Dos(ImporteBruto - TotalGastos);

    public static Resultado<LiquidacionComision> Crear(Guid empresaId, Guid clienteId, string clienteNombre, DatosLiquidacionComision d, DateTimeOffset ahora)
    {
        var l = new LiquidacionComision(Guid.NewGuid(), empresaId)
        {
            ClienteId = clienteId, ClienteNombre = clienteNombre, Estado = EstadoLiquidacionComision.Borrador, CreadoEn = ahora,
        };
        var r = l.Cambiar(d);
        return r.EsFallo ? Resultado.Fallo<LiquidacionComision>(r.Error) : Resultado.Ok(l);
    }

    /// <summary>Fija los datos de la liquidación. Cada línea trae los datos del albarán (<see cref="LineaAlbaranLiquidable"/>).</summary>
    public Resultado Cambiar(DatosLiquidacionComision d)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (Estado != EstadoLiquidacionComision.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_comision.no_borrador", "Solo se cambia una liquidación en borrador."));
        }

        if (!Enum.IsDefined(d.Modo))
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.modo", "La liquidación se valora a precio neto o a bruto con factura de gastos."));
        }

        if (d.Lineas is not { Count: > 0 })
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.sin_lineas", "La liquidación necesita al menos una línea de albarán."));
        }

        if (d.Lineas.GroupBy(l => (l.Albaran.AlbaranId, l.Albaran.Orden)).Any(g => g.Count() > 1))
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.linea_repetida", "Una línea de albarán está dos veces."));
        }

        var lineas = new List<LineaLiquidacionComision>();
        foreach (var l in d.Lineas)
        {
            if (l.Albaran.ClienteId != ClienteId)
            {
                return Resultado.Fallo(Error.Validacion("liquidacion_comision.otro_cliente", $"El albarán {l.Albaran.AlbaranNumero} es de otro cliente."));
            }

            if (l.CantidadVendida < 0m || l.CantidadVendida > l.Albaran.Cantidad || l.PrecioBruto < 0m)
            {
                return Resultado.Fallo(Error.Validacion("liquidacion_comision.cantidad",
                    $"Albarán {l.Albaran.AlbaranNumero}, línea {l.Albaran.Orden}: se vende entre 0 y lo enviado ({l.Albaran.Cantidad:0.###}) y el precio no es negativo."));
            }

            lineas.Add(new LineaLiquidacionComision(Guid.NewGuid(), l.Albaran, Math.Round(l.CantidadVendida, 3, MidpointRounding.AwayFromZero),
                Math.Round(l.PrecioBruto, 4, MidpointRounding.AwayFromZero)));
        }

        var bruto = Redondeo.Dos(lineas.Sum(l => l.ImporteBruto));
        var gastos = new List<GastoLiquidacionComision>();
        foreach (var g in d.Gastos ?? [])
        {
            if (!Enum.IsDefined(g.Tipo) || g.Porcentaje is < 0m or > 100m || g.Importe is < 0m || (g.Porcentaje is null) == (g.Importe is null))
            {
                return Resultado.Fallo(Error.Validacion("liquidacion_comision.gasto", "Cada gasto lleva un porcentaje sobre el bruto (0-100) o un importe, no negativos."));
            }

            gastos.Add(new GastoLiquidacionComision(Guid.NewGuid(), g.Tipo, Texto(g.Descripcion, 120), g.Porcentaje,
                g.Porcentaje is { } p ? Redondeo.Dos(bruto * p / 100m) : Redondeo.Dos(g.Importe!.Value)));
        }

        if (d.Modo == ModoLiquidacionComision.BrutoConFacturaGastos && gastos.Sum(g => g.Importe) > 0m && d.ProveedorId is null)
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.proveedor", "A bruto, los gastos los factura el comisionista: indica su ficha de proveedor."));
        }

        // Los gastos se reparten entre las líneas por su importe bruto, al céntimo (la última recoge el redondeo).
        var total = Redondeo.Dos(gastos.Sum(g => g.Importe));
        var repartido = 0m;
        for (var i = 0; i < lineas.Count; i++)
        {
            var parte = i == lineas.Count - 1 ? Redondeo.Dos(total - repartido)
                : bruto == 0m ? 0m : Redondeo.Dos(total * lineas[i].ImporteBruto / bruto);
            lineas[i].AsignarGastos(parte, d.Modo);
            repartido += parte;
        }

        if (d.Modo == ModoLiquidacionComision.PrecioNeto && lineas.Any(l => l.ImporteNeto < 0m))
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.neto_negativo",
                "Los gastos superan lo vendido en alguna línea: a precio neto el albarán quedaría en negativo. Liquídala a bruto con factura de gastos."));
        }

        Fecha = d.Fecha;
        ReferenciaCliente = Texto(d.ReferenciaCliente, 60);
        Modo = d.Modo;
        ProveedorId = d.Modo == ModoLiquidacionComision.BrutoConFacturaGastos ? d.ProveedorId : null;
        CodigoIvaGastos = Texto(d.CodigoIvaGastos, 20);
        Observaciones = Texto(d.Observaciones, 500);
        _lineas.Clear();
        _lineas.AddRange(lineas);
        _gastos.Clear();
        _gastos.AddRange(gastos);
        return Resultado.Ok();
    }

    public Resultado Confirmar(int numero, Guid? gastoId)
    {
        if (Estado != EstadoLiquidacionComision.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_comision.no_borrador", "La liquidación ya está confirmada o anulada."));
        }

        Numero = numero;
        GastoId = gastoId;
        Estado = EstadoLiquidacionComision.Confirmada;
        return Resultado.Ok();
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado != EstadoLiquidacionComision.Confirmada)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_comision.no_confirmada", "Solo se anula una liquidación confirmada; el borrador se elimina."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("liquidacion_comision.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoLiquidacionComision.Anulada;
        MotivoAnulacion = Texto(motivo, 200);
        return Resultado.Ok();
    }

    internal static string? Texto(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
}

/// <summary>Una línea de albarán que se liquida, tal como está en el albarán (con su precio estimado, que se recupera al anular).</summary>
public sealed record LineaAlbaranLiquidable(Guid AlbaranId, string AlbaranNumero, DateOnly AlbaranFecha, Guid ClienteId, int Orden, Guid? ProductoId, string Descripcion,
    decimal Cantidad, decimal PrecioEstimado, decimal DescuentoAnterior);

public sealed record DatosLineaComision(LineaAlbaranLiquidable Albaran, decimal CantidadVendida, decimal PrecioBruto);

public sealed record DatosGastoComision(TipoGastoComision Tipo, decimal? Porcentaje = null, decimal? Importe = null, string? Descripcion = null);

public sealed record DatosLiquidacionComision(DateOnly Fecha, ModoLiquidacionComision Modo, IReadOnlyList<DatosLineaComision> Lineas, IReadOnlyList<DatosGastoComision>? Gastos = null,
    string? ReferenciaCliente = null, Guid? ProveedorId = null, string? CodigoIvaGastos = null, string? Observaciones = null);

public sealed class LineaLiquidacionComision : EntidadBase<Guid>
{
    private LineaLiquidacionComision(Guid id) : base(id) { AlbaranNumero = null!; Descripcion = null!; }

    internal LineaLiquidacionComision(Guid id, LineaAlbaranLiquidable a, decimal vendida, decimal precioBruto) : base(id)
    {
        AlbaranVentaId = a.AlbaranId;
        AlbaranNumero = a.AlbaranNumero;
        AlbaranFecha = a.AlbaranFecha;
        OrdenAlbaran = a.Orden;
        ProductoId = a.ProductoId;
        Descripcion = a.Descripcion;
        CantidadEnviada = a.Cantidad;
        PrecioEstimado = a.PrecioEstimado;
        DescuentoAnterior = a.DescuentoAnterior;
        CantidadVendida = vendida;
        PrecioBruto = precioBruto;
        ImporteBruto = Redondeo.Dos(vendida * precioBruto);
    }

    public Guid AlbaranVentaId { get; private set; }

    public string AlbaranNumero { get; private set; }

    public DateOnly AlbaranFecha { get; private set; }

    public int OrdenAlbaran { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal CantidadEnviada { get; private set; }

    public decimal CantidadVendida { get; private set; }

    public decimal PrecioBruto { get; private set; }

    public decimal ImporteBruto { get; private set; }

    public decimal Gastos { get; private set; }

    public decimal ImporteNeto => Redondeo.Dos(ImporteBruto - Gastos);

    /// <summary>Precio por unidad enviada que queda en el albarán (el neto o el bruto, según el modo).</summary>
    public decimal PrecioAlbaran { get; private set; }

    /// <summary>Precio estimado del albarán antes de liquidar, que vuelve si se anula.</summary>
    public decimal PrecioEstimado { get; private set; }

    public decimal DescuentoAnterior { get; private set; }

    /// <summary>Lo enviado que no se vendió (merma, destrío en destino).</summary>
    public decimal Merma => CantidadEnviada - CantidadVendida;

    internal void AsignarGastos(decimal gastos, ModoLiquidacionComision modo)
    {
        Gastos = gastos;
        var importe = modo == ModoLiquidacionComision.PrecioNeto ? ImporteNeto : ImporteBruto;
        PrecioAlbaran = CantidadEnviada == 0m ? 0m : Math.Round(Math.Max(importe, 0m) / CantidadEnviada, 4, MidpointRounding.AwayFromZero);
    }
}

public sealed class GastoLiquidacionComision : EntidadBase<Guid>
{
    private GastoLiquidacionComision(Guid id) : base(id) { }

    internal GastoLiquidacionComision(Guid id, TipoGastoComision tipo, string? descripcion, decimal? porcentaje, decimal importe) : base(id)
    {
        Tipo = tipo;
        Descripcion = descripcion;
        Porcentaje = porcentaje;
        Importe = importe;
    }

    public TipoGastoComision Tipo { get; private set; }

    public string? Descripcion { get; private set; }

    public decimal? Porcentaje { get; private set; }

    public decimal Importe { get; private set; }
}
