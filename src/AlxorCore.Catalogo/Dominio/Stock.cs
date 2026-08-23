using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>Tipo de movimiento de stock (existencias) de un producto.</summary>
public enum TipoMovimientoStock
{
    /// <summary>Entrada de mercancía (compra, reposición).</summary>
    Entrada = 1,

    /// <summary>Salida manual (merma, autoconsumo, rotura).</summary>
    Salida = 2,

    /// <summary>Ajuste a un stock contado (recuento de inventario).</summary>
    Ajuste = 3,

    /// <summary>Salida automática por una venta (factura o ticket).</summary>
    Venta = 4,
}

/// <summary>
/// Movimiento de existencias de un producto. Es un registro inmutable (histórico): guarda la
/// variación aplicada (<see cref="Cantidad"/>, con signo) y el stock resultante tras aplicarla.
/// </summary>
public sealed class MovimientoStock : RaizAgregadoEmpresa<Guid>
{
    private MovimientoStock(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private MovimientoStock(Guid id, Guid empresaId, Guid productoId, TipoMovimientoStock tipo, decimal cantidad, decimal stockResultante, string? motivo, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        Tipo = tipo;
        Cantidad = cantidad;
        StockResultante = stockResultante;
        Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        CreadoEn = ahora;
    }

    public Guid ProductoId { get; private set; }

    public TipoMovimientoStock Tipo { get; private set; }

    /// <summary>Variación aplicada al stock, con signo (positiva = entrada, negativa = salida).</summary>
    public decimal Cantidad { get; private set; }

    /// <summary>Stock del producto después de aplicar este movimiento.</summary>
    public decimal StockResultante { get; private set; }

    public string? Motivo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    internal static MovimientoStock Registrar(
        Guid empresaId, Guid productoId, TipoMovimientoStock tipo, decimal cantidad, decimal stockResultante, string? motivo, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), empresaId, productoId, tipo, cantidad, stockResultante, motivo, ahora);
}

/// <summary>
/// Existencias «simples» de un artículo <b>en una empresa</b>. Como el catálogo (artículo) se comparte
/// por grupo pero el stock es propio de cada empresa, esta entidad lleva la cantidad por empresa +
/// artículo. Es el modo de stock básico; el control por almacenes/ubicaciones vive en el módulo de
/// Inventario. Una fila por empresa y artículo.
/// </summary>
public sealed class ExistenciaSimple : RaizAgregadoEmpresa<Guid>
{
    private ExistenciaSimple(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ExistenciaSimple(Guid id, Guid empresaId, Guid productoId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        Cantidad = 0m;
        ActualizadoEn = ahora;
    }

    public Guid ProductoId { get; private set; }

    /// <summary>Existencias actuales del artículo en la empresa.</summary>
    public decimal Cantidad { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static ExistenciaSimple Crear(Guid empresaId, Guid productoId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new ExistenciaSimple(Guid.NewGuid(), empresaId, productoId, reloj.AhoraUtc);
    }

    /// <summary>
    /// Aplica un movimiento de existencias y actualiza la cantidad. Un
    /// <see cref="TipoMovimientoStock.Ajuste"/> fija la cantidad al valor contado; el resto suman o
    /// restan la cantidad indicada. Devuelve el movimiento inmutable (histórico) resultante.
    /// </summary>
    public Resultado<MovimientoStock> Aplicar(TipoMovimientoStock tipo, decimal cantidad, string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (cantidad < 0)
        {
            return Resultado.Fallo<MovimientoStock>(Error.Validacion("stock.cantidad_negativa", "La cantidad no puede ser negativa."));
        }

        var delta = tipo switch
        {
            TipoMovimientoStock.Entrada => cantidad,
            TipoMovimientoStock.Salida => -cantidad,
            TipoMovimientoStock.Venta => -cantidad,
            TipoMovimientoStock.Ajuste => cantidad - Cantidad,
            _ => 0m,
        };

        Cantidad += delta;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok(MovimientoStock.Registrar(EmpresaId, ProductoId, tipo, delta, Cantidad, motivo, reloj.AhoraUtc));
    }
}
