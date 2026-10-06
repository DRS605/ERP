using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Compras.Dominio;

public enum EstadoDevolucionCompra
{
    /// <summary>Devuelta al proveedor, pendiente de su abono.</summary>
    Registrada = 1,

    /// <summary>El pedido aún no estaba facturado: la factura salió ya sin lo devuelto.</summary>
    EnFactura = 2,

    /// <summary>El proveedor la abonó con su factura rectificativa.</summary>
    Abonada = 3,

    /// <summary>Sin abono (por ejemplo, el proveedor repone la mercancía).</summary>
    SinAbono = 4,

    Anulada = 5,
}

public sealed class LineaDevolucionCompra
{
    private LineaDevolucionCompra() => Descripcion = null!;

    internal LineaDevolucionCompra(Guid id, Guid lineaPedidoId, Guid? productoId, string descripcion, decimal cantidad, decimal precioUnitario, string? lote)
    {
        Id = id;
        LineaPedidoId = lineaPedidoId;
        ProductoId = productoId;
        Descripcion = descripcion;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Lote = lote;
    }

    public Guid Id { get; private set; }

    public Guid LineaPedidoId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    /// <summary>Precio por unidad de la línea del pedido, con sus conceptos (lo que se pagó por cada unidad).</summary>
    public decimal PrecioUnitario { get; private set; }

    public string? Lote { get; private set; }

    public decimal Base => Redondeo.Dos(Cantidad * PrecioUnitario);
}

/// <summary>
/// Devolución de mercancía recibida al proveedor: sale del almacén y queda pendiente de su abono. Si el pedido aún no
/// estaba facturado, la factura del pedido sale ya sin lo devuelto; si lo estaba, se registra el abono del proveedor
/// (factura rectificativa recibida, en negativo, que rectifica la del pedido).
/// </summary>
public sealed class DevolucionCompra : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMotivo = 200;

    private readonly List<LineaDevolucionCompra> _lineas = [];

    private DevolucionCompra(Guid id) : base(id, Guid.Empty) => ProveedorTexto = null!;

    private DevolucionCompra(Guid id, Guid empresaId) : base(id, empresaId) => ProveedorTexto = null!;

    public Guid PedidoId { get; private set; }

    public Guid? ProveedorId { get; private set; }

    public string ProveedorTexto { get; private set; }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"DVC-{Ejercicio}-{Numero:D5}";

    public DateOnly Fecha { get; private set; }

    public string? Motivo { get; private set; }

    /// <summary>Almacén del que salió la mercancía (null: no se movió stock).</summary>
    public Guid? AlmacenId { get; private set; }

    public EstadoDevolucionCompra Estado { get; private set; }

    /// <summary>Gasto del abono del proveedor (rectificativa recibida).</summary>
    public Guid? GastoAbonoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaDevolucionCompra> Lineas => _lineas;

    public decimal Base => Redondeo.Dos(_lineas.Sum(l => l.Base));

    /// <summary>Cuenta para lo ya devuelto de las líneas del pedido (todo menos las anuladas).</summary>
    public bool Viva => Estado != EstadoDevolucionCompra.Anulada;

    /// <summary>
    /// Crea la devolución: cada cantidad no puede pasar de lo recibido en esa línea menos lo ya devuelto.
    /// </summary>
    public static Resultado<DevolucionCompra> Crear(Guid empresaId, PedidoCompra pedido, int numero, DateOnly fecha, string? motivo, Guid? almacenId,
        IReadOnlyList<(Guid LineaPedidoId, decimal Cantidad, string? Lote)> lineas, IReadOnlyDictionary<Guid, decimal> yaDevuelto, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(yaDevuelto);
        ArgumentNullException.ThrowIfNull(reloj);
        if (pedido.EsTraspasoIntragrupo)
        {
            return Resultado.Fallo<DevolucionCompra>(Error.Conflicto("devolucion_compra.intragrupo",
                "Es un traspaso de otra empresa del grupo: la devolución se hace desde la empresa de origen."));
        }

        var validas = lineas.Where(l => l.Cantidad != 0m).ToList();
        if (validas.Count == 0)
        {
            return Resultado.Fallo<DevolucionCompra>(Error.Validacion("devolucion_compra.sin_lineas", "Indica qué se devuelve."));
        }

        var d = new DevolucionCompra(Guid.NewGuid(), empresaId)
        {
            PedidoId = pedido.Id, ProveedorId = pedido.ProveedorId, ProveedorTexto = pedido.ProveedorTexto, Ejercicio = fecha.Year, Numero = numero, Fecha = fecha,
            AlmacenId = almacenId, Estado = EstadoDevolucionCompra.Registrada, CreadoEn = reloj.AhoraUtc,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, LongitudMotivo)],
        };
        foreach (var grupo in validas.GroupBy(l => l.LineaPedidoId))
        {
            var lp = pedido.Lineas.FirstOrDefault(l => l.Id == grupo.Key);
            if (lp is null)
            {
                return Resultado.Fallo<DevolucionCompra>(Error.Validacion("devolucion_compra.linea", "La línea no es de este pedido."));
            }

            var cantidad = grupo.Sum(l => l.Cantidad);
            if (grupo.Any(l => l.Cantidad < 0m))
            {
                return Resultado.Fallo<DevolucionCompra>(Error.Validacion("devolucion_compra.cantidad", "Las cantidades devueltas son positivas."));
            }

            var devolvible = lp.CantidadRecibida - yaDevuelto.GetValueOrDefault(lp.Id);
            if (cantidad > devolvible)
            {
                return Resultado.Fallo<DevolucionCompra>(Error.Validacion("devolucion_compra.excede",
                    $"De «{lp.Descripcion}» solo se pueden devolver {devolvible:0.###} (recibido {lp.CantidadRecibida:0.###}, ya devuelto {yaDevuelto.GetValueOrDefault(lp.Id):0.###})."));
            }

            var precio = lp.Cantidad == 0m ? lp.PrecioUnitario : Math.Round(lp.Importe / lp.Cantidad, 4, MidpointRounding.AwayFromZero);
            foreach (var l in grupo)
            {
                d._lineas.Add(new LineaDevolucionCompra(Guid.NewGuid(), lp.Id, lp.ProductoId, lp.Descripcion, l.Cantidad, precio,
                    string.IsNullOrWhiteSpace(l.Lote) ? null : l.Lote.Trim()));
            }
        }

        return Resultado.Ok(d);
    }

    /// <summary>Descontada al facturar el pedido.</summary>
    public void DescontarEnFactura()
    {
        if (Estado == EstadoDevolucionCompra.Registrada)
        {
            Estado = EstadoDevolucionCompra.EnFactura;
        }
    }

    public Resultado Abonar(Guid gastoAbonoId)
    {
        if (Estado != EstadoDevolucionCompra.Registrada)
        {
            return Resultado.Fallo(Error.Conflicto("devolucion_compra.no_pendiente", "La devolución ya no está pendiente de abono."));
        }

        Estado = EstadoDevolucionCompra.Abonada;
        GastoAbonoId = gastoAbonoId;
        return Resultado.Ok();
    }

    public Resultado CerrarSinAbono()
    {
        if (Estado != EstadoDevolucionCompra.Registrada)
        {
            return Resultado.Fallo(Error.Conflicto("devolucion_compra.no_pendiente", "La devolución ya no está pendiente de abono."));
        }

        Estado = EstadoDevolucionCompra.SinAbono;
        return Resultado.Ok();
    }

    public Resultado Anular()
    {
        if (Estado != EstadoDevolucionCompra.Registrada)
        {
            return Resultado.Fallo(Error.Conflicto("devolucion_compra.no_anulable",
                Estado == EstadoDevolucionCompra.Anulada ? "La devolución ya está anulada." : "La devolución ya está abonada o en la factura: anula antes el abono."));
        }

        Estado = EstadoDevolucionCompra.Anulada;
        return Resultado.Ok();
    }
}
