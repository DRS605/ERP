using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>Datos de una línea de tarifa (para crear o sustituir las líneas).</summary>
public sealed record DatosLineaTarifa(
    Guid? ProductoId = null,
    Guid? FamiliaId = null,
    decimal CantidadMinima = 0m,
    decimal? Precio = null,
    decimal PorcentajeDescuento = 0m,
    DateOnly? Desde = null,
    DateOnly? Hasta = null);

/// <summary>
/// Línea de una tarifa. Su <b>ámbito</b> es un producto, una familia (y sus subfamilias) o, si no
/// indica ninguno, todos los productos. Fija un precio especial, un descuento, o ambos, a partir de
/// una cantidad mínima (escalado) y con vigencia opcional por fechas (ambos extremos incluidos).
/// </summary>
public sealed class LineaTarifa
{
    private LineaTarifa()
    {
    }

    internal LineaTarifa(DatosLineaTarifa d)
    {
        Id = Guid.NewGuid();
        ProductoId = d.ProductoId;
        FamiliaId = d.FamiliaId;
        CantidadMinima = d.CantidadMinima;
        Precio = d.Precio;
        PorcentajeDescuento = d.PorcentajeDescuento;
        Desde = d.Desde;
        Hasta = d.Hasta;
    }

    public Guid Id { get; private set; }

    public Guid? ProductoId { get; private set; }

    public Guid? FamiliaId { get; private set; }

    public decimal CantidadMinima { get; private set; }

    /// <summary>Precio especial. Null = se mantiene el precio del producto (solo descuento).</summary>
    public decimal? Precio { get; private set; }

    public decimal PorcentajeDescuento { get; private set; }

    public DateOnly? Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    public bool VigenteEn(DateOnly fecha) => (Desde is null || Desde <= fecha) && (Hasta is null || fecha <= Hasta);

    internal bool SeSolapaCon(LineaTarifa otra) =>
        ProductoId == otra.ProductoId && FamiliaId == otra.FamiliaId && CantidadMinima == otra.CantidadMinima
        && (Desde ?? DateOnly.MinValue) <= (otra.Hasta ?? DateOnly.MaxValue)
        && (otra.Desde ?? DateOnly.MinValue) <= (Hasta ?? DateOnly.MaxValue);
}

/// <summary>Lo que se pide a una tarifa: un producto (con sus familias, de la más cercana a la raíz), cantidad y fecha.</summary>
public sealed record SolicitudPrecio(Guid ProductoId, IReadOnlyList<Guid> Familias, decimal PrecioBase, decimal Cantidad, DateOnly Fecha);

/// <summary>Precio que da la tarifa y de dónde sale (para explicarlo al usuario).</summary>
public sealed record PrecioTarifa(decimal PrecioUnitario, decimal PorcentajeDescuento, string Origen);

/// <summary>
/// Tarifa de precios de venta, compartida por el grupo (como productos y familias). Se asigna a los
/// clientes. Al pedirle el precio de un producto se aplica <b>una sola línea</b>: la más específica
/// (producto &gt; familia más cercana &gt; general) y, entre las del mismo ámbito, la de mayor
/// cantidad mínima alcanzada (escalado por volumen). Solo cuentan las líneas vigentes en la fecha.
/// </summary>
public sealed class Tarifa : RaizAgregadoGrupo<Guid>
{
    public const int LongitudMaximaCodigo = 20;
    public const int LongitudMaximaNombre = 100;

    private readonly List<LineaTarifa> _lineas = [];

    private Tarifa(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Tarifa(Guid id, Guid grupoId, string codigo, string nombre, DateTimeOffset ahora)
        : base(id, grupoId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Activa = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activa { get; private set; }

    public IReadOnlyList<LineaTarifa> Lineas => _lineas;

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Tarifa> Crear(Guid grupoId, string? codigo, string? nombre, IEnumerable<DatosLineaTarifa>? lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var c = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (c.Length == 0 || c.Length > LongitudMaximaCodigo)
        {
            return Resultado.Fallo<Tarifa>(Error.Validacion("tarifa.codigo_invalido", $"El código es obligatorio y de hasta {LongitudMaximaCodigo} caracteres."));
        }

        var tarifa = new Tarifa(Guid.NewGuid(), grupoId, c, string.Empty, reloj.AhoraUtc);
        var r = tarifa.Actualizar(nombre, true, lineas, reloj);
        return r.EsFallo ? Resultado.Fallo<Tarifa>(r.Error) : Resultado.Ok(tarifa);
    }

    /// <summary>Cambia nombre, estado y sustituye todas las líneas (validadas en bloque).</summary>
    public Resultado Actualizar(string? nombre, bool activa, IEnumerable<DatosLineaTarifa>? lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var n = (nombre ?? string.Empty).Trim();
        if (n.Length == 0 || n.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo(Error.Validacion("tarifa.nombre_invalido", $"El nombre es obligatorio y de hasta {LongitudMaximaNombre} caracteres."));
        }

        var nuevas = new List<LineaTarifa>();
        foreach (var (d, i) in (lineas ?? []).Select((d, i) => (d, i + 1)))
        {
            var error = ValidarLinea(d, i);
            if (error is not null)
            {
                return Resultado.Fallo(error);
            }

            var linea = new LineaTarifa(d);
            if (nuevas.FirstOrDefault(o => o.SeSolapaCon(linea)) is { } choca)
            {
                return Resultado.Fallo(Error.Validacion("tarifa.linea_duplicada",
                    $"La línea {i} repite ámbito y cantidad mínima de la línea {nuevas.IndexOf(choca) + 1} con fechas que se solapan."));
            }

            nuevas.Add(linea);
        }

        Nombre = n;
        Activa = activa;
        _lineas.Clear();
        _lineas.AddRange(nuevas);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Precio que aplica la tarifa a la solicitud, o null si ninguna línea aplica (o la tarifa está inactiva).</summary>
    public PrecioTarifa? Resolver(SolicitudPrecio s)
    {
        ArgumentNullException.ThrowIfNull(s);
        if (!Activa)
        {
            return null;
        }

        // Especificidad: producto (1000) > familia (999 la más cercana, 998 su padre…) > general (0).
        int? Puntuacion(LineaTarifa l)
        {
            if (l.ProductoId is { } p)
            {
                return p == s.ProductoId ? 1000 : null;
            }

            if (l.FamiliaId is { } f)
            {
                var nivel = s.Familias.ToList().IndexOf(f);
                return nivel < 0 ? null : 999 - nivel;
            }

            return 0;
        }

        var mejor = _lineas
            .Where(l => l.VigenteEn(s.Fecha) && l.CantidadMinima <= s.Cantidad)
            .Select(l => (Linea: l, Puntos: Puntuacion(l)))
            .Where(x => x.Puntos is not null)
            .OrderByDescending(x => x.Puntos)
            .ThenByDescending(x => x.Linea.CantidadMinima)
            .Select(x => x.Linea)
            .FirstOrDefault();
        if (mejor is null)
        {
            return null;
        }

        var ambito = mejor.ProductoId is not null ? "precio del producto" : mejor.FamiliaId is not null ? "familia" : "general";
        var escalado = mejor.CantidadMinima > 0 ? $", desde {mejor.CantidadMinima:0.###} uds" : string.Empty;
        return new PrecioTarifa(mejor.Precio ?? s.PrecioBase, mejor.PorcentajeDescuento, $"Tarifa {Codigo} ({ambito}{escalado})");
    }

    private static Error? ValidarLinea(DatosLineaTarifa d, int i)
    {
        if (d.ProductoId is not null && d.FamiliaId is not null)
        {
            return Error.Validacion("tarifa.linea_ambito", $"La línea {i} debe ser de un producto o de una familia, no de ambos.");
        }

        if (d.CantidadMinima < 0)
        {
            return Error.Validacion("tarifa.linea_cantidad", $"La línea {i} tiene una cantidad mínima negativa.");
        }

        if (d.Precio is < 0)
        {
            return Error.Validacion("tarifa.linea_precio", $"La línea {i} tiene un precio negativo.");
        }

        if (d.PorcentajeDescuento is < 0 or > 100)
        {
            return Error.Validacion("tarifa.linea_descuento", $"La línea {i} tiene un descuento fuera de 0-100 %.");
        }

        if (d.Precio is null && d.PorcentajeDescuento == 0)
        {
            return Error.Validacion("tarifa.linea_vacia", $"La línea {i} no fija ni precio ni descuento.");
        }

        if (d.Desde is not null && d.Hasta is not null && d.Hasta < d.Desde)
        {
            return Error.Validacion("tarifa.linea_fechas", $"En la línea {i} la fecha final es anterior a la inicial.");
        }

        return null;
    }
}
