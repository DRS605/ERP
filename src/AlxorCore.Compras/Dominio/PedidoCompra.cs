using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Compras.Dominio;

/// <summary>Estado de un pedido de compra a proveedor.</summary>
public enum EstadoPedido
{
    Borrador = 1,
    Confirmado = 2,

    /// <summary>Se ha recibido mercancía (parcial o total; ver <see cref="PedidoCompra.RecibidoCompleto"/>).</summary>
    Recibido = 3,
    Facturado = 4,
    Cancelado = 5,
}

/// <summary>Línea de un pedido de compra, con seguimiento de lo recibido y lo facturado.</summary>
public sealed class LineaPedido
{
    private LineaPedido() { Descripcion = null!; }

    internal LineaPedido(Guid id, Guid? productoId, string descripcion, decimal cantidad, decimal precioUnitario)
    {
        Id = id;
        ProductoId = productoId;
        Descripcion = descripcion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }

    public Guid Id { get; private set; }

    /// <summary>Artículo del catálogo (opcional; permite la entrada automática en inventario).</summary>
    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    public decimal CantidadRecibida { get; private set; }

    public decimal CantidadFacturada { get; private set; }

    /// <summary>Línea del pedido de venta de la empresa del grupo de la que es espejo (traspaso intragrupo).</summary>
    public Guid? LineaVentaOrigenId { get; internal set; }

    /// <summary>Importe de la línea antes de conceptos.</summary>
    public decimal ImporteBruto => Redondeo.Dos(Cantidad * PrecioUnitario);

    /// <summary>Conceptos de línea aplicados (copia de cada concepto con su importe).</summary>
    public IReadOnlyList<ConceptoAplicado> Conceptos { get; private set; } = [];

    /// <summary>Suma de los conceptos que cambian el importe a pagar al proveedor.</summary>
    public decimal ImporteConceptos { get; private set; }

    /// <summary>Suma de los conceptos que solo cambian el coste (portes a otro transportista, aranceles…).</summary>
    public decimal CosteConceptos { get; private set; }

    /// <summary>Importe de la línea (el que factura el proveedor): bruto + conceptos que cambian el importe.</summary>
    public decimal Importe => ImporteBruto + ImporteConceptos;

    /// <summary>Coste unitario con que entra en almacén: importe y conceptos de coste repartidos por unidad.</summary>
    public decimal CosteUnitarioEntrada => Cantidad == 0m ? PrecioUnitario : Math.Round((Importe + CosteConceptos) / Cantidad, 4, MidpointRounding.AwayFromZero);

    internal void PonerConceptos(IReadOnlyList<ConceptoAplicado> conceptos)
    {
        Conceptos = conceptos.ToList();
        ImporteConceptos = ConceptosLinea.SumaPrecio(Conceptos);
        CosteConceptos = ConceptosLinea.SumaCoste(Conceptos);
    }

    public decimal PendienteRecibir => Cantidad - CantidadRecibida;

    internal void Recibir(decimal cantidad) => CantidadRecibida = Math.Round(CantidadRecibida + cantidad, 3, MidpointRounding.AwayFromZero);

    internal void DeshacerRecepcion(decimal cantidad) => CantidadRecibida = Math.Max(0m, Math.Round(CantidadRecibida - cantidad, 3, MidpointRounding.AwayFromZero));

    internal void Facturar() => CantidadFacturada = Cantidad;
}

/// <summary>
/// Pedido de compra a un proveedor. Se confirma, se va recibiendo (albaranes) y finalmente se
/// factura. Congela el nombre del proveedor. Segundo eslabón de la cadena de compras.
/// </summary>
public sealed class PedidoCompra : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTexto = 200;
    private readonly List<LineaPedido> _lineas = new();

    private PedidoCompra(Guid id) : base(id, Guid.Empty) { ProveedorTexto = null!; }

    private PedidoCompra(Guid id, Guid empresaId, Guid? proveedorId, string proveedorTexto, DateOnly fecha,
        int ejercicio, int numero, string? serie, Guid? solicitudOrigenId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProveedorId = proveedorId;
        ProveedorTexto = proveedorTexto;
        Fecha = fecha;
        Ejercicio = ejercicio;
        Numero = numero;
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        SolicitudOrigenId = solicitudOrigenId;
        Estado = EstadoPedido.Borrador;
        CreadoEn = ahora;
    }

    public Guid? ProveedorId { get; private set; }

    public string ProveedorTexto { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Ejercicio (año) del pedido.</summary>
    public int Ejercicio { get; private set; }

    /// <summary>Número correlativo del pedido, por empresa · ejercicio · proveedor (serie por proveedor y año).</summary>
    public int Numero { get; private set; }

    /// <summary>Prefijo de serie asignado (opcional); si es nulo, el número se muestra sin prefijo.</summary>
    public string? Serie { get; private set; }

    /// <summary>Número mostrable: con prefijo de serie si lo hay, o el número correlativo a secas.</summary>
    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Ejercicio}/{Numero:D5}" : Numero.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public Guid? SolicitudOrigenId { get; private set; }

    /// <summary>Empresa del grupo que vende (traspaso intragrupo): el pedido nace de su albarán de venta.</summary>
    public Guid? EmpresaOrigenId { get; private set; }

    /// <summary>Pedido de venta de la empresa del grupo del que este pedido es espejo (traspaso de existencias).</summary>
    public Guid? PedidoVentaOrigenId { get; private set; }

    /// <summary>Es un traspaso desde otra empresa del grupo: se recibe, anula y factura desde la empresa de origen.</summary>
    public bool EsTraspasoIntragrupo => PedidoVentaOrigenId is not null;

    /// <summary>
    /// Marca el pedido (en borrador) como espejo del pedido de venta de otra empresa del grupo; cada línea queda
    /// enlazada, en orden, con su línea del pedido de venta.
    /// </summary>
    public Resultado MarcarTraspasoIntragrupo(Guid empresaOrigenId, Guid pedidoVentaId, IReadOnlyList<Guid> lineasVenta)
    {
        ArgumentNullException.ThrowIfNull(lineasVenta);
        if (Estado is not EstadoPedido.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("pedido.estado", "Solo un pedido en borrador se marca como traspaso."));
        }

        if (lineasVenta.Count != _lineas.Count)
        {
            return Resultado.Fallo(Error.Validacion("pedido.lineas_traspaso", "Cada línea del pedido debe corresponder a una línea del pedido de venta."));
        }

        EmpresaOrigenId = empresaOrigenId;
        PedidoVentaOrigenId = pedidoVentaId;
        for (var i = 0; i < _lineas.Count; i++)
        {
            _lineas[i].LineaVentaOrigenId = lineasVenta[i];
        }

        return Resultado.Ok();
    }

    /// <summary>Deshace una recepción (se anuló su albarán). Un pedido facturado no se toca.</summary>
    public Resultado DeshacerRecepcion(IReadOnlyList<(Guid LineaId, decimal Cantidad)> recepciones)
    {
        ArgumentNullException.ThrowIfNull(recepciones);
        if (Estado is EstadoPedido.Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("pedido.facturado", "El pedido ya está facturado: no se deshace su recepción."));
        }

        foreach (var (lineaId, cantidad) in recepciones)
        {
            _lineas.SingleOrDefault(l => l.Id == lineaId)?.DeshacerRecepcion(cantidad);
        }

        if (Estado is EstadoPedido.Recibido && _lineas.All(l => l.CantidadRecibida == 0m))
        {
            Estado = EstadoPedido.Confirmado;
        }

        return Resultado.Ok();
    }

    public EstadoPedido Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaPedido> Lineas => _lineas;

    public decimal Total => Redondeo.Dos(_lineas.Sum(l => l.Importe));

    /// <summary>Pone los conceptos de cada línea (en el orden de las líneas) mientras el pedido no tiene recepciones.</summary>
    public Resultado PonerConceptos(IReadOnlyList<IReadOnlyList<ConceptoAplicado>> conceptos)
    {
        ArgumentNullException.ThrowIfNull(conceptos);
        if (conceptos.Count != _lineas.Count)
        {
            return Resultado.Fallo(Error.Validacion("pedido.conceptos", "Hay que dar los conceptos de cada línea."));
        }

        if (_lineas.Any(l => l.CantidadRecibida > 0m || l.CantidadFacturada > 0m))
        {
            return Resultado.Fallo(Error.Conflicto("pedido.no_modificable", "Solo se cambian los conceptos de un pedido sin recepciones ni factura."));
        }

        if (_lineas.Zip(conceptos).Any(x => x.First.ImporteBruto + ConceptosLinea.SumaPrecio(x.Second) < 0m))
        {
            return Resultado.Fallo(Error.Validacion("pedido.linea_negativa", "Los conceptos de línea no pueden dejar una línea con importe negativo."));
        }

        foreach (var (l, c) in _lineas.Zip(conceptos))
        {
            l.PonerConceptos(c);
        }

        return Resultado.Ok();
    }

    /// <summary>Todas las líneas se han recibido por completo.</summary>
    public bool RecibidoCompleto => _lineas.Count > 0 && _lineas.All(l => l.CantidadRecibida >= l.Cantidad);

    public static Resultado<PedidoCompra> Crear(Guid empresaId, Guid? proveedorId, string? proveedorTexto,
        DateOnly fecha, int numero, Guid? solicitudOrigenId, IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio)> lineas, IReloj reloj, string? serie = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (string.IsNullOrWhiteSpace(proveedorTexto))
        {
            return Resultado.Fallo<PedidoCompra>(Error.Validacion("pedido.proveedor_vacio", "El proveedor es obligatorio."));
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<PedidoCompra>(Error.Validacion("pedido.sin_lineas", "El pedido necesita al menos una línea."));
        }

        foreach (var l in lineas)
        {
            if (string.IsNullOrWhiteSpace(l.Descripcion))
            {
                return Resultado.Fallo<PedidoCompra>(Error.Validacion("pedido.descripcion_vacia", "Cada línea necesita una descripción."));
            }

            if (l.Cantidad <= 0m || l.Precio < 0m)
            {
                return Resultado.Fallo<PedidoCompra>(Error.Validacion("pedido.linea_invalida", "Cantidad > 0 y precio ≥ 0."));
            }
        }

        var pedido = new PedidoCompra(Guid.NewGuid(), empresaId, proveedorId, proveedorTexto.Trim(), fecha, fecha.Year, numero, serie, solicitudOrigenId, reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            pedido._lineas.Add(new LineaPedido(Guid.NewGuid(), l.ProductoId, l.Descripcion.Trim(), l.Cantidad, Redondeo.Dos(l.Precio)));
        }

        return Resultado.Ok(pedido);
    }

    /// <summary>
    /// Modifica fecha y líneas de un pedido aún sin recepciones ni factura. El proveedor no cambia (el número es de
    /// la serie del proveedor) y la fecha sigue en su ejercicio.
    /// </summary>
    public Resultado Modificar(DateOnly fecha, IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio)> lineas)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (Estado is not (EstadoPedido.Borrador or EstadoPedido.Confirmado) || _lineas.Any(l => l.CantidadRecibida > 0m || l.CantidadFacturada > 0m))
        {
            return Resultado.Fallo(Error.Conflicto("pedido.no_modificable", "Solo se modifica un pedido sin recepciones ni factura."));
        }

        if (fecha.Year != Ejercicio)
        {
            return Resultado.Fallo(Error.Validacion("pedido.fecha_ejercicio", $"La fecha debe ser de {Ejercicio}, el ejercicio de su número."));
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("pedido.sin_lineas", "El pedido necesita al menos una línea."));
        }

        if (lineas.Any(l => string.IsNullOrWhiteSpace(l.Descripcion)))
        {
            return Resultado.Fallo(Error.Validacion("pedido.descripcion_vacia", "Cada línea necesita una descripción."));
        }

        if (lineas.Any(l => l.Cantidad <= 0m || l.Precio < 0m))
        {
            return Resultado.Fallo(Error.Validacion("pedido.linea_invalida", "Cantidad > 0 y precio ≥ 0."));
        }

        Fecha = fecha;
        _lineas.Clear();
        foreach (var l in lineas)
        {
            _lineas.Add(new LineaPedido(Guid.NewGuid(), l.ProductoId, l.Descripcion.Trim(), l.Cantidad, Redondeo.Dos(l.Precio)));
        }

        return Resultado.Ok();
    }

    public Resultado Confirmar()
    {
        if (Estado is not EstadoPedido.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("pedido.estado", "Solo se confirma un pedido en borrador."));
        }

        Estado = EstadoPedido.Confirmado;
        return Resultado.Ok();
    }

    /// <summary>Registra la recepción de cantidades (desde un albarán).</summary>
    public Resultado RegistrarRecepcion(IReadOnlyList<(Guid LineaId, decimal Cantidad)> recepciones)
    {
        ArgumentNullException.ThrowIfNull(recepciones);
        if (Estado is not (EstadoPedido.Confirmado or EstadoPedido.Recibido))
        {
            return Resultado.Fallo(Error.Conflicto("pedido.no_confirmado", "Confirma el pedido antes de recibir mercancía."));
        }

        foreach (var (lineaId, cantidad) in recepciones)
        {
            var linea = _lineas.SingleOrDefault(l => l.Id == lineaId);
            if (linea is null)
            {
                return Resultado.Fallo(Error.Validacion("pedido.linea_desconocida", "Una línea recibida no pertenece al pedido."));
            }

            if (cantidad <= 0m)
            {
                return Resultado.Fallo(Error.Validacion("pedido.recepcion_invalida", "La cantidad recibida debe ser mayor que cero."));
            }

            if (linea.CantidadRecibida + cantidad > linea.Cantidad)
            {
                return Resultado.Fallo(Error.Validacion("pedido.sobre_recepcion", $"No puedes recibir más de lo pedido en «{linea.Descripcion}»."));
            }
        }

        foreach (var (lineaId, cantidad) in recepciones)
        {
            _lineas.Single(l => l.Id == lineaId).Recibir(cantidad);
        }

        Estado = EstadoPedido.Recibido;
        return Resultado.Ok();
    }

    /// <summary>Marca el pedido como facturado (genera un gasto por el total). Devuelve el total.</summary>
    public Resultado<decimal> Facturar()
    {
        if (Estado is EstadoPedido.Facturado)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("pedido.ya_facturado", "El pedido ya está facturado."));
        }

        if (Estado is not (EstadoPedido.Confirmado or EstadoPedido.Recibido))
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("pedido.no_confirmado", "Confirma el pedido antes de facturarlo."));
        }

        if (EsTraspasoIntragrupo)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("pedido.intragrupo",
                "Es un traspaso de otra empresa del grupo: su factura llega a la bandeja de facturas recibidas; contabilízala allí."));
        }

        foreach (var l in _lineas)
        {
            l.Facturar();
        }

        Estado = EstadoPedido.Facturado;
        return Resultado.Ok(Total);
    }

    public Resultado Cancelar()
    {
        if (Estado is EstadoPedido.Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("pedido.facturado", "No puedes cancelar un pedido facturado."));
        }

        Estado = EstadoPedido.Cancelado;
        return Resultado.Ok();
    }
}
