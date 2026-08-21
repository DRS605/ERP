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

    internal LineaPedido(Guid id, string descripcion, decimal cantidad, decimal precioUnitario)
    {
        Id = id;
        Descripcion = descripcion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }

    public Guid Id { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    public decimal CantidadRecibida { get; private set; }

    public decimal CantidadFacturada { get; private set; }

    public decimal Importe => Redondeo.Dos(Cantidad * PrecioUnitario);

    public decimal PendienteRecibir => Cantidad - CantidadRecibida;

    internal void Recibir(decimal cantidad) => CantidadRecibida = Math.Round(CantidadRecibida + cantidad, 3, MidpointRounding.AwayFromZero);

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
        Guid? solicitudOrigenId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProveedorId = proveedorId;
        ProveedorTexto = proveedorTexto;
        Fecha = fecha;
        SolicitudOrigenId = solicitudOrigenId;
        Estado = EstadoPedido.Borrador;
        CreadoEn = ahora;
    }

    public Guid? ProveedorId { get; private set; }

    public string ProveedorTexto { get; private set; }

    public DateOnly Fecha { get; private set; }

    public Guid? SolicitudOrigenId { get; private set; }

    public EstadoPedido Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaPedido> Lineas => _lineas;

    public decimal Total => Redondeo.Dos(_lineas.Sum(l => l.Importe));

    /// <summary>Todas las líneas se han recibido por completo.</summary>
    public bool RecibidoCompleto => _lineas.Count > 0 && _lineas.All(l => l.CantidadRecibida >= l.Cantidad);

    public static Resultado<PedidoCompra> Crear(Guid empresaId, Guid? proveedorId, string? proveedorTexto,
        DateOnly fecha, Guid? solicitudOrigenId, IReadOnlyList<(string Descripcion, decimal Cantidad, decimal Precio)> lineas, IReloj reloj)
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

        var pedido = new PedidoCompra(Guid.NewGuid(), empresaId, proveedorId, proveedorTexto.Trim(), fecha, solicitudOrigenId, reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            pedido._lineas.Add(new LineaPedido(Guid.NewGuid(), l.Descripcion.Trim(), l.Cantidad, Redondeo.Dos(l.Precio)));
        }

        return Resultado.Ok(pedido);
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
