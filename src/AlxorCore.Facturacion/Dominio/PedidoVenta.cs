using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Estado de un pedido de venta a cliente.</summary>
public enum EstadoPedidoVenta
{
    Borrador = 1,
    Confirmado = 2,

    /// <summary>Se ha entregado mercancía (parcial o total; ver <see cref="PedidoVenta.ServidoCompleto"/>).</summary>
    Servido = 3,
    Facturado = 4,
    Cancelado = 5,
}

/// <summary>Línea de un pedido de venta, con seguimiento de lo servido y lo facturado.</summary>
public sealed class LineaPedidoVenta
{
    /// <summary>Número de la línea en el documento (1, 2, 3…).</summary>
    public int Orden { get; internal set; }

    private LineaPedidoVenta()
    {
        Descripcion = null!;
        CodigoIva = null!;
    }

    internal LineaPedidoVenta(Guid id, Guid? productoId, string descripcion, decimal cantidad, decimal precioUnitario, decimal porcentajeDescuento, string codigoIva)
    {
        Id = id;
        ProductoId = productoId;
        Descripcion = descripcion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        PorcentajeDescuento = porcentajeDescuento;
        CodigoIva = codigoIva;
    }

    public Guid Id { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    public decimal PorcentajeDescuento { get; private set; }

    public string CodigoIva { get; private set; }

    public decimal CantidadServida { get; private set; }

    public decimal CantidadFacturada { get; private set; }

    /// <summary>Conceptos de línea aplicados.</summary>
    public IReadOnlyList<ConceptoAplicado> Conceptos { get; private set; } = [];

    public decimal ImporteConceptos { get; private set; }

    public decimal CosteConceptos { get; private set; }

    /// <summary>Importe de la línea con descuento, antes de conceptos.</summary>
    public decimal BaseBruta => Redondeo.Dos(Cantidad * PrecioUnitario * (1m - PorcentajeDescuento / 100m));

    /// <summary>Base imponible de la línea (con descuento y conceptos que cambian el importe).</summary>
    public decimal Base => BaseBruta + ImporteConceptos;

    internal void PonerConceptos(IReadOnlyList<ConceptoAplicado> conceptos)
    {
        Conceptos = conceptos.ToList();
        ImporteConceptos = ConceptosLinea.SumaPrecio(Conceptos);
        CosteConceptos = ConceptosLinea.SumaCoste(Conceptos);
    }

    public decimal PendienteServir => Cantidad - CantidadServida;

    internal void Servir(decimal cantidad) => CantidadServida = Math.Round(CantidadServida + cantidad, 3, MidpointRounding.AwayFromZero);

    internal void DeshacerServido(decimal cantidad) => CantidadServida = Math.Max(0m, Math.Round(CantidadServida - cantidad, 3, MidpointRounding.AwayFromZero));

    internal void Facturar() => CantidadFacturada = Cantidad;

    internal void FacturarCantidad(decimal cantidad) => CantidadFacturada = Math.Round(Math.Min(Cantidad, CantidadFacturada + cantidad), 3, MidpointRounding.AwayFromZero);

    internal void DeshacerFacturado(decimal cantidad) => CantidadFacturada = Math.Round(Math.Max(0m, CantidadFacturada - cantidad), 3, MidpointRounding.AwayFromZero);
}

/// <summary>
/// Pedido de venta a un cliente. Se confirma, se va sirviendo (albaranes de entrega) y finalmente se
/// factura (generando una factura real con toda su maquinaria fiscal). Congela el nombre del cliente.
/// Segundo eslabón de la cadena de ventas (presupuesto → pedido → albarán → factura).
/// </summary>
public sealed class PedidoVenta : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTexto = 200;
    private readonly List<LineaPedidoVenta> _lineas = new();

    private PedidoVenta(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
    }

    private PedidoVenta(Guid id, Guid empresaId, Guid clienteId, string clienteNombre, DateOnly fecha,
        int ejercicio, int numero, string? serie, Guid? presupuestoOrigenId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Fecha = fecha;
        Ejercicio = ejercicio;
        Numero = numero;
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        PresupuestoOrigenId = presupuestoOrigenId;
        Estado = EstadoPedidoVenta.Borrador;
        CreadoEn = ahora;
    }

    public Guid ClienteId { get; private set; }

    /// <summary>Centro de la empresa donde se hace el documento (null: sin centro).</summary>
    public Guid? CentroId { get; private set; }

    /// <summary>Fija el centro del documento (al crearlo; los que nacen de otro heredan el suyo).</summary>
    public void AsignarCentro(Guid? centroId) => CentroId = centroId == Guid.Empty ? null : centroId;

    /// <summary>Divisa del documento (ISO 4217); null = euros. Sus precios e importes van en esa divisa.</summary>
    public string? Moneda { get; private set; }

    public void EstablecerMoneda(string? moneda) => Moneda = moneda;

    public string ClienteNombre { get; private set; }

    public DateOnly Fecha { get; private set; }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string? Serie { get; private set; }

    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Ejercicio}/{Numero:D5}" : Numero.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public Guid? PresupuestoOrigenId { get; private set; }

    public EstadoPedidoVenta Estado { get; private set; }

    public Guid? FacturaId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>Líneas en su orden (la base de datos no garantiza el orden en que las devuelve).</summary>
    public IReadOnlyList<LineaPedidoVenta> Lineas => _lineas.OrderBy(l => l.Orden).ToList().AsReadOnly();

    /// <summary>Añade una línea con el número siguiente.</summary>
    private void AgregarLinea(LineaPedidoVenta linea)
    {
        linea.Orden = _lineas.Count + 1;
        _lineas.Add(linea);
    }

    public decimal Total => Redondeo.Dos(_lineas.Sum(l => l.Base));

    /// <summary>Suplidos y fianzas de las líneas: fuera de la base y sin impuesto (la factura los suma al total).</summary>
    public decimal Suplidos => Redondeo.Dos(_lineas.Sum(l => ConceptosLinea.SumaSuplidos(l.Conceptos)));

    /// <summary>Pone los conceptos de cada línea (en el orden de las líneas) mientras se puede modificar el pedido.</summary>
    public Resultado PonerConceptos(IReadOnlyList<IReadOnlyList<ConceptoAplicado>> conceptos)
    {
        ArgumentNullException.ThrowIfNull(conceptos);
        if (conceptos.Count != _lineas.Count)
        {
            return Resultado.Fallo(Error.Validacion("pedidoventa.conceptos", "Hay que dar los conceptos de cada línea."));
        }

        if (_lineas.Zip(conceptos).Any(x => x.First.BaseBruta + ConceptosLinea.SumaPrecio(x.Second) < 0m))
        {
            return Resultado.Fallo(Error.Validacion("pedidoventa.linea_negativa", "Los conceptos de línea no pueden dejar una línea con importe negativo."));
        }

        foreach (var (l, c) in _lineas.Zip(conceptos))
        {
            l.PonerConceptos(c);
        }

        return Resultado.Ok();
    }

    public bool ServidoCompleto => _lineas.Count > 0 && _lineas.All(l => l.CantidadServida >= l.Cantidad);

    public static Resultado<PedidoVenta> Crear(Guid empresaId, Guid clienteId, string? clienteNombre, DateOnly fecha,
        int numero, Guid? presupuestoOrigenId,
        IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Descuento, string CodigoIva)> lineas,
        IReloj reloj, string? serie = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (Validar(clienteId, clienteNombre, lineas) is { } error)
        {
            return Resultado.Fallo<PedidoVenta>(error);
        }

        var pedido = new PedidoVenta(Guid.NewGuid(), empresaId, clienteId, clienteNombre!.Trim(), fecha, fecha.Year, numero, serie, presupuestoOrigenId, reloj.AhoraUtc);
        pedido.PonerLineas(lineas);
        return Resultado.Ok(pedido);
    }

    /// <summary>
    /// Modifica cliente, fecha y líneas de un pedido que aún no se ha entregado ni facturado (en borrador o
    /// confirmado). El número no cambia, así que la fecha tiene que seguir en su ejercicio.
    /// </summary>
    public Resultado Modificar(Guid clienteId, string? clienteNombre, DateOnly fecha,
        IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Descuento, string CodigoIva)> lineas)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (Estado is not (EstadoPedidoVenta.Borrador or EstadoPedidoVenta.Confirmado) || _lineas.Any(l => l.CantidadServida > 0m))
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.no_modificable",
                "Solo se modifica un pedido sin entregas ni factura. Si ya se entregó, cancela lo pendiente o haz otro pedido."));
        }

        if (fecha.Year != Ejercicio)
        {
            return Resultado.Fallo(Error.Validacion("pedidoventa.fecha_ejercicio", $"La fecha debe ser de {Ejercicio}, el ejercicio de su número."));
        }

        if (Validar(clienteId, clienteNombre, lineas) is { } error)
        {
            return Resultado.Fallo(error);
        }

        ClienteId = clienteId;
        ClienteNombre = clienteNombre!.Trim();
        Fecha = fecha;
        _lineas.Clear();
        PonerLineas(lineas);
        return Resultado.Ok();
    }

    private void PonerLineas(IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Descuento, string CodigoIva)> lineas)
    {
        foreach (var l in lineas)
        {
            AgregarLinea(new LineaPedidoVenta(Guid.NewGuid(), l.ProductoId, l.Descripcion.Trim(), l.Cantidad,
                Redondeo.Dos(l.Precio), l.Descuento, string.IsNullOrWhiteSpace(l.CodigoIva) ? "IVA21" : l.CodigoIva.Trim()));
        }
    }

    private static Error? Validar(Guid clienteId, string? clienteNombre,
        IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Descuento, string CodigoIva)> lineas)
    {
        if (clienteId == Guid.Empty || string.IsNullOrWhiteSpace(clienteNombre))
        {
            return Error.Validacion("pedidoventa.cliente_vacio", "El cliente es obligatorio.");
        }

        if (lineas.Count == 0)
        {
            return Error.Validacion("pedidoventa.sin_lineas", "El pedido necesita al menos una línea.");
        }

        foreach (var l in lineas)
        {
            if (string.IsNullOrWhiteSpace(l.Descripcion))
            {
                return Error.Validacion("pedidoventa.descripcion_vacia", "Cada línea necesita una descripción.");
            }

            if (l.Cantidad <= 0m || l.Precio < 0m)
            {
                return Error.Validacion("pedidoventa.linea_invalida", "Cantidad > 0 y precio ≥ 0.");
            }
        }

        return null;
    }

    public Resultado Confirmar()
    {
        if (Estado is not EstadoPedidoVenta.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.estado", "Solo se confirma un pedido en borrador."));
        }

        Estado = EstadoPedidoVenta.Confirmado;
        return Resultado.Ok();
    }

    /// <summary>Registra la entrega de cantidades (desde un albarán de venta).</summary>
    public Resultado RegistrarEntrega(IReadOnlyList<(Guid LineaId, decimal Cantidad)> entregas)
    {
        ArgumentNullException.ThrowIfNull(entregas);
        if (Estado is not (EstadoPedidoVenta.Confirmado or EstadoPedidoVenta.Servido))
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.no_confirmado", "Confirma el pedido antes de entregar mercancía."));
        }

        foreach (var (lineaId, cantidad) in entregas)
        {
            var linea = _lineas.SingleOrDefault(l => l.Id == lineaId);
            if (linea is null)
            {
                return Resultado.Fallo(Error.Validacion("pedidoventa.linea_desconocida", "Una línea entregada no pertenece al pedido."));
            }

            if (cantidad <= 0m)
            {
                return Resultado.Fallo(Error.Validacion("pedidoventa.entrega_invalida", "La cantidad entregada debe ser mayor que cero."));
            }

            if (linea.CantidadServida + cantidad > linea.Cantidad)
            {
                return Resultado.Fallo(Error.Validacion("pedidoventa.sobre_entrega", $"No puedes entregar más de lo pedido en «{linea.Descripcion}»."));
            }
        }

        foreach (var (lineaId, cantidad) in entregas)
        {
            _lineas.Single(l => l.Id == lineaId).Servir(cantidad);
        }

        Estado = EstadoPedidoVenta.Servido;
        return Resultado.Ok();
    }

    /// <summary>
    /// Deshace una entrega (se anuló su albarán): las cantidades vuelven a quedar pendientes. Un pedido ya facturado no
    /// se toca: la corrección es una factura rectificativa.
    /// </summary>
    public Resultado DeshacerEntrega(IReadOnlyList<(Guid LineaId, decimal Cantidad)> entregas)
    {
        ArgumentNullException.ThrowIfNull(entregas);
        if (Estado is EstadoPedidoVenta.Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.facturado", "El pedido ya está facturado: corrige con una factura rectificativa."));
        }

        foreach (var (lineaId, cantidad) in entregas)
        {
            _lineas.SingleOrDefault(l => l.Id == lineaId)?.DeshacerServido(cantidad);
        }

        if (Estado is EstadoPedidoVenta.Servido && _lineas.All(l => l.CantidadServida == 0m))
        {
            Estado = EstadoPedidoVenta.Confirmado;
        }

        return Resultado.Ok();
    }

    /// <summary>Marca el pedido como facturado, enlazando la factura generada.</summary>
    public Resultado Facturar(Guid facturaId)
    {
        if (Estado is EstadoPedidoVenta.Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.ya_facturado", "El pedido ya está facturado."));
        }

        if (Estado is EstadoPedidoVenta.Cancelado)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.cancelado", "El pedido está cancelado."));
        }

        if (Estado is EstadoPedidoVenta.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.no_confirmado", "Confirma el pedido antes de facturarlo."));
        }

        foreach (var l in _lineas)
        {
            l.Facturar();
        }

        Estado = EstadoPedidoVenta.Facturado;
        FacturaId = facturaId;
        return Resultado.Ok();
    }

    /// <summary>
    /// Se ha facturado un albarán del pedido: suma lo facturado de sus líneas; cuando todo el pedido está facturado, pasa a
    /// <c>Facturado</c> con esa factura.
    /// </summary>
    public void FacturarEntrega(Guid facturaId, IReadOnlyList<(Guid LineaId, decimal Cantidad)> entregas)
    {
        ArgumentNullException.ThrowIfNull(entregas);
        foreach (var (lineaId, cantidad) in entregas)
        {
            _lineas.SingleOrDefault(l => l.Id == lineaId)?.FacturarCantidad(cantidad);
        }

        if (Estado is not EstadoPedidoVenta.Cancelado && _lineas.All(l => l.CantidadFacturada >= l.Cantidad))
        {
            Estado = EstadoPedidoVenta.Facturado;
            FacturaId = facturaId;
        }
    }

    /// <summary>Se anuló la factura de un albarán del pedido: lo facturado vuelve atrás y el pedido, a servido.</summary>
    public void DeshacerFacturacion(Guid facturaId, IReadOnlyList<(Guid LineaId, decimal Cantidad)> entregas)
    {
        ArgumentNullException.ThrowIfNull(entregas);
        foreach (var (lineaId, cantidad) in entregas)
        {
            _lineas.SingleOrDefault(l => l.Id == lineaId)?.DeshacerFacturado(cantidad);
        }

        if (Estado is EstadoPedidoVenta.Facturado && _lineas.Any(l => l.CantidadFacturada < l.Cantidad))
        {
            Estado = _lineas.Any(l => l.CantidadServida > 0m) ? EstadoPedidoVenta.Servido : EstadoPedidoVenta.Confirmado;
            if (FacturaId == facturaId)
            {
                FacturaId = null;
            }
        }
    }

    public Resultado Cancelar()
    {
        if (Estado is EstadoPedidoVenta.Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("pedidoventa.facturado", "No puedes cancelar un pedido facturado."));
        }

        Estado = EstadoPedidoVenta.Cancelado;
        return Resultado.Ok();
    }
}
