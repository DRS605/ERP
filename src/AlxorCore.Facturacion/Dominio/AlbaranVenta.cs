using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Línea de un albarán de venta (referencia a la línea del pedido).</summary>
public sealed class LineaAlbaranVenta
{
    private LineaAlbaranVenta()
    {
        Descripcion = null!;
    }

    internal LineaAlbaranVenta(Guid id, Guid lineaPedidoId, Guid? productoId, string descripcion, decimal cantidad)
    {
        Id = id;
        LineaPedidoId = lineaPedidoId;
        ProductoId = productoId;
        Descripcion = descripcion;
        Cantidad = cantidad;
    }

    public Guid Id { get; private set; }

    public Guid LineaPedidoId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }
}

/// <summary>
/// Albarán de entrega de un pedido de venta. Documenta qué y cuánto se entregó al cliente; al crearse
/// actualiza las cantidades servidas del pedido. La salida de existencias del inventario la realiza la
/// factura al emitirse (para no duplicar el movimiento), por lo que el albarán es un documento de
/// entrega. Tercer eslabón de la cadena de ventas.
/// </summary>
public sealed class AlbaranVenta : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaAlbaranVenta> _lineas = new();

    private AlbaranVenta(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
    }

    private AlbaranVenta(Guid id, Guid empresaId, Guid pedidoId, Guid clienteId, string clienteNombre, int numero, DateOnly fecha, string? referencia, string? serie, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        PedidoId = pedidoId;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Numero = numero;
        Fecha = fecha;
        Referencia = referencia;
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        CreadoEn = ahora;
    }

    public Guid PedidoId { get; private set; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Serie { get; private set; }

    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Fecha.Year}/{Numero:D5}" : Numero.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Referencia libre del albarán (opcional).</summary>
    public string? Referencia { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaAlbaranVenta> Lineas => _lineas;

    public static Resultado<AlbaranVenta> Crear(Guid empresaId, Guid pedidoId, Guid clienteId, string clienteNombre, int numero, DateOnly fecha, string? referencia,
        IReadOnlyList<(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad)> lineas, IReloj reloj, string? serie = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<AlbaranVenta>(Error.Validacion("albaranventa.sin_lineas", "El albarán necesita al menos una línea entregada."));
        }

        var albaran = new AlbaranVenta(Guid.NewGuid(), empresaId, pedidoId, clienteId, clienteNombre?.Trim() ?? string.Empty, numero, fecha, referencia?.Trim(), serie, reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            albaran._lineas.Add(new LineaAlbaranVenta(Guid.NewGuid(), l.LineaPedidoId, l.ProductoId, l.Descripcion?.Trim() ?? string.Empty, l.Cantidad));
        }

        return Resultado.Ok(albaran);
    }
}
