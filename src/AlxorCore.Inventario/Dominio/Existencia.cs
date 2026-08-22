using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Inventario.Dominio;

/// <summary>Tipo de movimiento de inventario.</summary>
public enum TipoMovimientoInventario
{
    Entrada = 1,
    Salida = 2,
    Ajuste = 3,
    TraspasoSalida = 4,
    TraspasoEntrada = 5,
}

/// <summary>
/// Existencia (stock) de un artículo en un almacén y, opcionalmente, en una ubicación concreta.
/// La cantidad no puede quedar negativa.
/// </summary>
public sealed class Existencia : RaizAgregadoEmpresa<Guid>
{
    private Existencia(Guid id) : base(id, Guid.Empty) { }

    private Existencia(Guid id, Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, string? lote)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        AlmacenId = almacenId;
        UbicacionId = ubicacionId;
        Lote = lote;
        Cantidad = 0m;
    }

    public Guid ProductoId { get; private set; }

    public Guid AlmacenId { get; private set; }

    public Guid? UbicacionId { get; private set; }

    /// <summary>Lote o número de serie al que pertenece esta existencia (null si el artículo no se traza así).</summary>
    public string? Lote { get; private set; }

    public decimal Cantidad { get; private set; }

    public static Existencia Nueva(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, string? lote = null) =>
        new(Guid.NewGuid(), empresaId, productoId, almacenId, ubicacionId, lote);

    public void Aumentar(decimal cantidad) => Cantidad = Math.Round(Cantidad + cantidad, 3, MidpointRounding.AwayFromZero);

    public Resultado Disminuir(decimal cantidad)
    {
        if (cantidad > Cantidad)
        {
            return Resultado.Fallo(Error.Validacion("existencia.insuficiente", $"No hay stock suficiente (disponible {Cantidad})."));
        }

        Cantidad = Math.Round(Cantidad - cantidad, 3, MidpointRounding.AwayFromZero);
        return Resultado.Ok();
    }

    public void Fijar(decimal cantidad) => Cantidad = Math.Round(cantidad, 3, MidpointRounding.AwayFromZero);
}

/// <summary>Registro (histórico) de un movimiento de inventario. Alimenta la trazabilidad.</summary>
public sealed class MovimientoInventario : RaizAgregadoEmpresa<Guid>
{
    private MovimientoInventario(Guid id) : base(id, Guid.Empty) { }

    private MovimientoInventario(Guid id, Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId,
        TipoMovimientoInventario tipo, decimal cantidad, DateOnly fecha, string? motivo, string? referencia, string? lote, decimal? costeUnitario, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        AlmacenId = almacenId;
        UbicacionId = ubicacionId;
        Tipo = tipo;
        Cantidad = cantidad;
        Fecha = fecha;
        Motivo = motivo;
        Referencia = referencia;
        Lote = lote;
        CosteUnitario = costeUnitario;
        CreadoEn = ahora;
    }

    public Guid ProductoId { get; private set; }

    public Guid AlmacenId { get; private set; }

    public Guid? UbicacionId { get; private set; }

    /// <summary>Lote o número de serie afectado por el movimiento (null si no aplica). Base de la trazabilidad.</summary>
    public string? Lote { get; private set; }

    /// <summary>Coste unitario de la entrada (para PMP/FIFO/última compra). Null en salidas o si no se conoce.</summary>
    public decimal? CosteUnitario { get; private set; }

    public TipoMovimientoInventario Tipo { get; private set; }

    /// <summary>Cantidad con signo (+ entra, − sale).</summary>
    public decimal Cantidad { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Motivo { get; private set; }

    public string? Referencia { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static MovimientoInventario Registrar(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId,
        TipoMovimientoInventario tipo, decimal cantidadConSigno, DateOnly fecha, string? motivo, string? referencia, IReloj reloj, string? lote = null, decimal? costeUnitario = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new MovimientoInventario(Guid.NewGuid(), empresaId, productoId, almacenId, ubicacionId, tipo,
            cantidadConSigno, fecha, string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim(),
            string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim(),
            string.IsNullOrWhiteSpace(lote) ? null : lote.Trim(), costeUnitario, reloj.AhoraUtc);
    }
}
