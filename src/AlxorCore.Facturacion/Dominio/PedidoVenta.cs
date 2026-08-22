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

    /// <summary>Base imponible de la línea (con descuento aplicado).</summary>
    public decimal Base => Redondeo.Dos(Cantidad * PrecioUnitario * (1m - PorcentajeDescuento / 100m));

    public decimal PendienteServir => Cantidad - CantidadServida;

    internal void Servir(decimal cantidad) => CantidadServida = Math.Round(CantidadServida + cantidad, 3, MidpointRounding.AwayFromZero);

    internal void Facturar() => CantidadFacturada = Cantidad;
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

    public IReadOnlyList<LineaPedidoVenta> Lineas => _lineas;

    public decimal Total => Redondeo.Dos(_lineas.Sum(l => l.Base));

    public bool ServidoCompleto => _lineas.Count > 0 && _lineas.All(l => l.CantidadServida >= l.Cantidad);

    public static Resultado<PedidoVenta> Crear(Guid empresaId, Guid clienteId, string? clienteNombre, DateOnly fecha,
        int numero, Guid? presupuestoOrigenId,
        IReadOnlyList<(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Precio, decimal Descuento, string CodigoIva)> lineas,
        IReloj reloj, string? serie = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (clienteId == Guid.Empty || string.IsNullOrWhiteSpace(clienteNombre))
        {
            return Resultado.Fallo<PedidoVenta>(Error.Validacion("pedidoventa.cliente_vacio", "El cliente es obligatorio."));
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<PedidoVenta>(Error.Validacion("pedidoventa.sin_lineas", "El pedido necesita al menos una línea."));
        }

        foreach (var l in lineas)
        {
            if (string.IsNullOrWhiteSpace(l.Descripcion))
            {
                return Resultado.Fallo<PedidoVenta>(Error.Validacion("pedidoventa.descripcion_vacia", "Cada línea necesita una descripción."));
            }

            if (l.Cantidad <= 0m || l.Precio < 0m)
            {
                return Resultado.Fallo<PedidoVenta>(Error.Validacion("pedidoventa.linea_invalida", "Cantidad > 0 y precio ≥ 0."));
            }
        }

        var pedido = new PedidoVenta(Guid.NewGuid(), empresaId, clienteId, clienteNombre.Trim(), fecha, fecha.Year, numero, serie, presupuestoOrigenId, reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            pedido._lineas.Add(new LineaPedidoVenta(Guid.NewGuid(), l.ProductoId, l.Descripcion.Trim(), l.Cantidad,
                Redondeo.Dos(l.Precio), l.Descuento, string.IsNullOrWhiteSpace(l.CodigoIva) ? "IVA21" : l.CodigoIva.Trim()));
        }

        return Resultado.Ok(pedido);
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
