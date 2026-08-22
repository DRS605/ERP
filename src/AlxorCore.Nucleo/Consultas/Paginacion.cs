namespace AlxorCore.Nucleo.Consultas;

/// <summary>
/// Parámetros de paginación de una consulta de listado, ya <b>normalizados</b> (página ≥ 1 y tamaño
/// acotado). Se construye siempre con <see cref="Normalizar"/> para evitar valores inválidos o
/// peticiones de páginas gigantes que tumbarían la base de datos.
/// </summary>
public sealed record Paginacion
{
    /// <summary>Tamaño de página por defecto cuando el cliente no indica uno.</summary>
    public const int TamanoDefecto = 25;

    /// <summary>Tamaño de página máximo permitido (tope de seguridad).</summary>
    public const int TamanoMaximo = 200;

    private Paginacion(int pagina, int tamanoPagina)
    {
        Pagina = pagina;
        TamanoPagina = tamanoPagina;
    }

    /// <summary>Número de página, empezando en 1.</summary>
    public int Pagina { get; }

    /// <summary>Filas por página.</summary>
    public int TamanoPagina { get; }

    /// <summary>Cuántas filas hay que saltar para llegar a esta página (para <c>Skip</c>).</summary>
    public int Saltar => (Pagina - 1) * TamanoPagina;

    /// <summary>Normaliza los parámetros recibidos del cliente: página ≥ 1 y tamaño en [1, máximo].</summary>
    public static Paginacion Normalizar(int? pagina, int? tamanoPagina)
    {
        var p = pagina is null or < 1 ? 1 : pagina.Value;
        var t = tamanoPagina switch
        {
            null or < 1 => TamanoDefecto,
            > TamanoMaximo => TamanoMaximo,
            _ => tamanoPagina.Value,
        };
        return new Paginacion(p, t);
    }
}

/// <summary>
/// Una página de resultados de una consulta de listado, con el <b>total</b> de filas que cumplen el
/// filtro (para que la interfaz pueda mostrar el paginador). Los <see cref="Elementos"/> son solo los
/// de la página pedida.
/// </summary>
public sealed record PaginaResultado<T>(IReadOnlyList<T> Elementos, int Total, int Pagina, int TamanoPagina)
{
    /// <summary>Número total de páginas dado el total de filas y el tamaño de página.</summary>
    public int TotalPaginas => TamanoPagina <= 0 ? 0 : (int)Math.Ceiling(Total / (double)TamanoPagina);

    /// <summary>¿Hay una página siguiente?</summary>
    public bool HayMas => (long)Pagina * TamanoPagina < Total;

    public static PaginaResultado<T> Crear(IReadOnlyList<T> elementos, int total, Paginacion paginacion)
    {
        ArgumentNullException.ThrowIfNull(paginacion);
        return new PaginaResultado<T>(elementos, total, paginacion.Pagina, paginacion.TamanoPagina);
    }

    /// <summary>Página vacía (0 resultados) conservando los parámetros de paginación.</summary>
    public static PaginaResultado<T> Vacia(Paginacion paginacion)
    {
        ArgumentNullException.ThrowIfNull(paginacion);
        return new PaginaResultado<T>(Array.Empty<T>(), 0, paginacion.Pagina, paginacion.TamanoPagina);
    }
}
