using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Inventario.Dominio;

public enum EstadoRecuento
{
    /// <summary>Abierto: se va contando.</summary>
    Abierto = 1,

    /// <summary>Cerrado: las diferencias se regularizaron con ajustes.</summary>
    Cerrado = 2,

    /// <summary>Anulado sin regularizar.</summary>
    Anulado = 3,
}

/// <summary>
/// Inventario físico de un almacén (o de una ubicación): al abrirlo se congela el stock teórico de cada artículo, lote
/// y ubicación; se anota lo contado y, al cerrarlo, se regulariza la diferencia (contado − teórico congelado) sobre el
/// stock de ese momento. Así lo que se mueve mientras se cuenta no se pierde ni se duplica.
/// </summary>
public sealed class RecuentoInventario : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudDescripcion = 200;
    public const int MaximoLineas = 20_000;

    private readonly List<LineaRecuento> _lineas = [];

    private RecuentoInventario(Guid id) : base(id, Guid.Empty) => Codigo = null!;

    private RecuentoInventario(Guid id, Guid empresaId) : base(id, empresaId) => Codigo = null!;

    public string Codigo { get; private set; }

    public Guid AlmacenId { get; private set; }

    /// <summary>Solo esta ubicación (null: todo el almacén).</summary>
    public Guid? UbicacionId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Descripcion { get; private set; }

    public EstadoRecuento Estado { get; private set; }

    public DateTimeOffset AbiertoEn { get; private set; }

    public DateTimeOffset? CerradoEn { get; private set; }

    /// <summary>Al cerrar, lo no contado se dio por cero (si no, se dejó como estaba).</summary>
    public bool NoContadosACero { get; private set; }

    public IReadOnlyList<LineaRecuento> Lineas => _lineas;

    public static Resultado<RecuentoInventario> Abrir(Guid empresaId, string codigo, Guid almacenId, Guid? ubicacionId, DateOnly fecha, string? descripcion,
        IEnumerable<(Guid ProductoId, Guid? UbicacionId, string? Lote, decimal Teorico)> teoricos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(teoricos);
        ArgumentNullException.ThrowIfNull(reloj);
        var r = new RecuentoInventario(Guid.NewGuid(), empresaId)
        {
            Codigo = codigo, AlmacenId = almacenId, UbicacionId = ubicacionId, Fecha = fecha, Estado = EstadoRecuento.Abierto, AbiertoEn = reloj.AhoraUtc,
            Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim()[..Math.Min(descripcion.Trim().Length, LongitudDescripcion)],
        };
        foreach (var t in teoricos)
        {
            r._lineas.Add(new LineaRecuento(Guid.NewGuid(), t.ProductoId, t.UbicacionId, Normalizar(t.Lote), t.Teorico, añadida: false));
        }

        return r._lineas.Count > MaximoLineas
            ? Resultado.Fallo<RecuentoInventario>(Error.Validacion("recuento.demasiadas_lineas", $"Más de {MaximoLineas} líneas: divide el recuento por ubicaciones."))
            : Resultado.Ok(r);
    }

    /// <summary>Anota lo contado de un artículo, lote y ubicación; si no estaba en el teórico, se añade (apareció en el almacén).</summary>
    public Resultado<LineaRecuento> Contar(Guid productoId, Guid? ubicacionId, string? lote, decimal? cantidad)
    {
        if (Estado != EstadoRecuento.Abierto)
        {
            return Resultado.Fallo<LineaRecuento>(Error.Conflicto("recuento.no_abierto", "El recuento ya está cerrado o anulado."));
        }

        if (cantidad < 0m)
        {
            return Resultado.Fallo<LineaRecuento>(Error.Validacion("recuento.cantidad", "Lo contado no puede ser negativo."));
        }

        if (UbicacionId is { } u && ubicacionId != u)
        {
            return Resultado.Fallo<LineaRecuento>(Error.Validacion("recuento.ubicacion", "El recuento es solo de una ubicación."));
        }

        lote = Normalizar(lote);
        var linea = _lineas.FirstOrDefault(l => l.ProductoId == productoId && l.UbicacionId == ubicacionId && l.Lote == lote);
        if (linea is null)
        {
            if (cantidad is null)
            {
                return Resultado.Fallo<LineaRecuento>(Error.NoEncontrado("recuento.linea", "Ese artículo no está en el recuento."));
            }

            linea = new LineaRecuento(Guid.NewGuid(), productoId, ubicacionId, lote, 0m, añadida: true);
            _lineas.Add(linea);
        }

        linea.Anotar(cantidad.HasValue ? Math.Round(cantidad.Value, 3, MidpointRounding.AwayFromZero) : null);
        return Resultado.Ok(linea);
    }

    /// <summary>Cierra el recuento y devuelve las diferencias a regularizar (contado − teórico) de cada línea.</summary>
    public Resultado<IReadOnlyList<(LineaRecuento Linea, decimal Diferencia)>> Cerrar(bool noContadosACero, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoRecuento.Abierto)
        {
            return Resultado.Fallo<IReadOnlyList<(LineaRecuento, decimal)>>(Error.Conflicto("recuento.no_abierto", "El recuento ya está cerrado o anulado."));
        }

        if (!noContadosACero && _lineas.All(l => l.Contado is null))
        {
            return Resultado.Fallo<IReadOnlyList<(LineaRecuento, decimal)>>(Error.Validacion("recuento.vacio", "No se ha contado nada."));
        }

        var diferencias = new List<(LineaRecuento, decimal)>();
        foreach (var l in _lineas)
        {
            var contado = l.Contado ?? (noContadosACero ? 0m : l.Teorico);
            l.Regularizar(contado);
            if (l.Diferencia != 0m)
            {
                diferencias.Add((l, l.Diferencia!.Value));
            }
        }

        Estado = EstadoRecuento.Cerrado;
        CerradoEn = reloj.AhoraUtc;
        NoContadosACero = noContadosACero;
        return Resultado.Ok<IReadOnlyList<(LineaRecuento, decimal)>>(diferencias);
    }

    public Resultado Anular()
    {
        if (Estado != EstadoRecuento.Abierto)
        {
            return Resultado.Fallo(Error.Conflicto("recuento.no_abierto", "Solo se anula un recuento abierto."));
        }

        Estado = EstadoRecuento.Anulado;
        return Resultado.Ok();
    }

    private static string? Normalizar(string? lote) => string.IsNullOrWhiteSpace(lote) ? null : lote.Trim();
}

public sealed class LineaRecuento : EntidadBase<Guid>
{
    private LineaRecuento(Guid id) : base(id) { }

    internal LineaRecuento(Guid id, Guid productoId, Guid? ubicacionId, string? lote, decimal teorico, bool añadida) : base(id)
    {
        ProductoId = productoId;
        UbicacionId = ubicacionId;
        Lote = lote;
        Teorico = teorico;
        Añadida = añadida;
    }

    public Guid ProductoId { get; private set; }

    public Guid? UbicacionId { get; private set; }

    public string? Lote { get; private set; }

    /// <summary>Stock teórico al abrir el recuento.</summary>
    public decimal Teorico { get; private set; }

    /// <summary>Lo contado (null: sin contar).</summary>
    public decimal? Contado { get; private set; }

    /// <summary>No estaba en el teórico: apareció al contar.</summary>
    public bool Añadida { get; private set; }

    /// <summary>Diferencia regularizada al cerrar (contado − teórico).</summary>
    public decimal? Diferencia { get; private set; }

    internal void Anotar(decimal? contado) => Contado = contado;

    internal void Regularizar(decimal contado)
    {
        Contado ??= contado;
        Diferencia = Math.Round(contado - Teorico, 3, MidpointRounding.AwayFromZero);
    }
}
