using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Compras.Dominio;

/// <summary>Línea de un albarán de recepción (referencia a la línea del pedido).</summary>
public sealed class LineaAlbaran
{
    private LineaAlbaran() { Descripcion = null!; }

    internal LineaAlbaran(Guid id, Guid lineaPedidoId, Guid? productoId, string descripcion, decimal cantidad)
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
/// Albarán de recepción de mercancía contra un pedido de compra. Registra qué y cuánto se recibió;
/// al crearse actualiza las cantidades recibidas del pedido. Tercer eslabón de la cadena.
/// </summary>
public sealed class AlbaranCompra : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaAlbaran> _lineas = new();

    private AlbaranCompra(Guid id) : base(id, Guid.Empty) { }

    private AlbaranCompra(Guid id, Guid empresaId, Guid pedidoId, int numero, DateOnly fecha, string? referencia, string? serie, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        PedidoId = pedidoId;
        Numero = numero;
        Fecha = fecha;
        Referencia = referencia;
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        CreadoEn = ahora;
    }

    public Guid PedidoId { get; private set; }

    /// <summary>Número correlativo del albarán de recepción (por empresa y ejercicio).</summary>
    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Prefijo de serie asignado (opcional); si es nulo, el número se muestra sin prefijo.</summary>
    public string? Serie { get; private set; }

    /// <summary>Número mostrable: con prefijo de serie si lo hay, o el número correlativo a secas.</summary>
    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Fecha.Year}/{Numero:D5}" : Numero.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Referencia del albarán del proveedor (opcional).</summary>
    public string? Referencia { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaAlbaran> Lineas => _lineas;

    public static Resultado<AlbaranCompra> Crear(Guid empresaId, Guid pedidoId, int numero, DateOnly fecha, string? referencia,
        IReadOnlyList<(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad)> lineas, IReloj reloj, string? serie = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<AlbaranCompra>(Error.Validacion("albaran.sin_lineas", "El albarán necesita al menos una línea recibida."));
        }

        var albaran = new AlbaranCompra(Guid.NewGuid(), empresaId, pedidoId, numero, fecha, referencia?.Trim(), serie, reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            albaran._lineas.Add(new LineaAlbaran(Guid.NewGuid(), l.LineaPedidoId, l.ProductoId, l.Descripcion?.Trim() ?? string.Empty, l.Cantidad));
        }

        return Resultado.Ok(albaran);
    }
}
