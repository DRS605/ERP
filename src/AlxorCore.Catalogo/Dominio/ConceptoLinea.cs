using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>Datos de una asignación automática de un concepto (para crear o sustituir las asignaciones).</summary>
public sealed record DatosAsignacionConcepto(Guid? TerceroId = null, Guid? FamiliaId = null, Guid? ProductoId = null, decimal? Valor = null);

/// <summary>
/// Cuándo se pone solo un concepto en las líneas: para un cliente o proveedor (o cualquiera, si no se indica),
/// y para un artículo, una familia (con sus subfamilias) o todos. <see cref="Valor"/> sustituye al valor por
/// defecto del concepto (p. ej. otro porcentaje de comisión para un cliente).
/// </summary>
public sealed class AsignacionConcepto
{
    private AsignacionConcepto()
    {
    }

    internal AsignacionConcepto(DatosAsignacionConcepto d)
    {
        Id = Guid.NewGuid();
        TerceroId = d.TerceroId;
        FamiliaId = d.ProductoId is null ? d.FamiliaId : null;
        ProductoId = d.ProductoId;
        Valor = d.Valor is { } v ? Math.Round(v, 4, MidpointRounding.AwayFromZero) : null;
    }

    public Guid Id { get; private set; }

    public Guid? TerceroId { get; private set; }

    public Guid? FamiliaId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public decimal? Valor { get; private set; }

    /// <summary>
    /// Si la asignación vale para el tercero y el artículo dados, cuánto de específica es (más alto, más
    /// específica): artículo &gt; familia (la más cercana) &gt; todos, y a igualdad, la del tercero. Null si no vale.
    /// </summary>
    internal int? Encaje(Guid? terceroId, Guid? productoId, IReadOnlyList<Guid> familias)
    {
        if (TerceroId is { } t && t != terceroId)
        {
            return null;
        }

        int nivel;
        if (ProductoId is { } p)
        {
            if (p != productoId)
            {
                return null;
            }

            nivel = 1000;
        }
        else if (FamiliaId is { } f)
        {
            var i = familias.ToList().IndexOf(f);
            if (i < 0)
            {
                return null;
            }

            nivel = 500 - i;
        }
        else
        {
            nivel = 0;
        }

        return nivel * 2 + (TerceroId is null ? 0 : 1);
    }
}

/// <summary>
/// Concepto de línea: un recargo, bonificación o coste que se pone en las líneas de los documentos de venta o de
/// compra. Si su efecto es <see cref="EfectoConcepto.Precio"/>, cambia el importe de la línea (portes cobrados,
/// rappel, recargo por envase…); si es <see cref="EfectoConcepto.Coste"/>, no toca el documento y solo suma o
/// resta al coste (comisión del comercial, portes pagados al transportista, aranceles…). Se comparte en el grupo,
/// como los artículos y las tarifas. Las <see cref="Asignaciones"/> lo ponen solo en las líneas que encajan.
/// </summary>
public sealed class ConceptoLinea : RaizAgregadoGrupo<Guid>
{
    public const int LongitudMaximaCodigo = 20;
    public const int LongitudMaximaNombre = 100;

    private readonly List<AsignacionConcepto> _asignaciones = [];

    private ConceptoLinea(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private ConceptoLinea(Guid id, Guid grupoId, string codigo, DateTimeOffset ahora)
        : base(id, grupoId)
    {
        Codigo = codigo;
        Nombre = string.Empty;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Texto que se imprime en el documento (si es null, el nombre).</summary>
    public string? TextoDocumento { get; private set; }

    public AmbitoConcepto Ambito { get; private set; }

    public EfectoConcepto Efecto { get; private set; }

    public SentidoConcepto Sentido { get; private set; }

    public CalculoConcepto Calculo { get; private set; }

    /// <summary>Valor por defecto: el porcentaje, el importe por unidad o por kilo, o el importe fijo.</summary>
    public decimal Valor { get; private set; }

    /// <summary>Cómo se reparte un importe fijo puesto al documento entero.</summary>
    public RepartoConcepto Reparto { get; private set; }

    public bool Activo { get; private set; }

    public IReadOnlyList<AsignacionConcepto> Asignaciones => _asignaciones;

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public bool ValeEn(AmbitoConcepto ambito) => Ambito == AmbitoConcepto.Ambos || Ambito == ambito;

    public static Resultado<ConceptoLinea> Crear(Guid grupoId, string? codigo, DatosConcepto datos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var c = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (c.Length == 0 || c.Length > LongitudMaximaCodigo)
        {
            return Resultado.Fallo<ConceptoLinea>(Error.Validacion("concepto.codigo", $"El código es obligatorio y de hasta {LongitudMaximaCodigo} caracteres."));
        }

        var concepto = new ConceptoLinea(Guid.NewGuid(), grupoId, c, reloj.AhoraUtc);
        var r = concepto.Actualizar(datos, true, reloj);
        return r.EsFallo ? Resultado.Fallo<ConceptoLinea>(r.Error) : Resultado.Ok(concepto);
    }

    /// <summary>Cambia la definición y sustituye todas las asignaciones.</summary>
    public Resultado Actualizar(DatosConcepto datos, bool activo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(datos);
        ArgumentNullException.ThrowIfNull(reloj);

        var nombre = (datos.Nombre ?? string.Empty).Trim();
        if (nombre.Length == 0 || nombre.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo(Error.Validacion("concepto.nombre", $"El nombre es obligatorio y de hasta {LongitudMaximaNombre} caracteres."));
        }

        var texto = string.IsNullOrWhiteSpace(datos.TextoDocumento) ? null : datos.TextoDocumento.Trim();
        if (texto is { Length: > LongitudMaximaNombre })
        {
            return Resultado.Fallo(Error.Validacion("concepto.texto", $"El texto del documento es de hasta {LongitudMaximaNombre} caracteres."));
        }

        if (!Enum.IsDefined(datos.Ambito) || !Enum.IsDefined(datos.Efecto) || !Enum.IsDefined(datos.Sentido) || !Enum.IsDefined(datos.Calculo) || !Enum.IsDefined(datos.Reparto))
        {
            return Resultado.Fallo(Error.Validacion("concepto.tipo", "El ámbito, el efecto, el sentido, el cálculo o el reparto no son válidos."));
        }

        if (ErrorValor(datos.Valor, datos.Calculo) is { } error)
        {
            return Resultado.Fallo(error);
        }

        var asignaciones = new List<AsignacionConcepto>();
        foreach (var (a, i) in (datos.Asignaciones ?? []).Select((a, i) => (a, i + 1)))
        {
            if (a.Valor is { } v && ErrorValor(v, datos.Calculo) is { } e)
            {
                return Resultado.Fallo(Error.Validacion(e.Codigo, $"Asignación {i}: {e.Mensaje}"));
            }

            var nueva = new AsignacionConcepto(a);
            if (asignaciones.Any(o => o.TerceroId == nueva.TerceroId && o.FamiliaId == nueva.FamiliaId && o.ProductoId == nueva.ProductoId))
            {
                return Resultado.Fallo(Error.Validacion("concepto.asignacion_repetida", $"La asignación {i} repite el tercero y el artículo o la familia de otra."));
            }

            asignaciones.Add(nueva);
        }

        Nombre = nombre;
        TextoDocumento = texto;
        Ambito = datos.Ambito;
        Efecto = datos.Efecto;
        Sentido = datos.Sentido;
        Calculo = datos.Calculo;
        Valor = Math.Round(datos.Valor, 4, MidpointRounding.AwayFromZero);
        Reparto = datos.Reparto;
        Activo = activo;
        _asignaciones.Clear();
        _asignaciones.AddRange(asignaciones);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public void Desactivar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Activo = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>La asignación más específica que vale para el tercero y el artículo, o null si ninguna.</summary>
    public AsignacionConcepto? AsignacionPara(Guid? terceroId, Guid? productoId, IReadOnlyList<Guid> familias) =>
        _asignaciones.Select(a => (a, Encaje: a.Encaje(terceroId, productoId, familias)))
            .Where(x => x.Encaje is not null)
            .OrderByDescending(x => x.Encaje)
            .Select(x => x.a)
            .FirstOrDefault();

    public static Error? ErrorValor(decimal valor, CalculoConcepto calculo)
    {
        if (valor < 0m)
        {
            return Error.Validacion("concepto.valor", "El valor no puede ser negativo: el sentido (suma o resta) lo decide el concepto.");
        }

        return calculo == CalculoConcepto.Porcentaje && valor > 100m
            ? Error.Validacion("concepto.valor", "El porcentaje no puede pasar de 100.")
            : null;
    }
}

/// <summary>Definición de un concepto de línea (alta y modificación).</summary>
public sealed record DatosConcepto(
    string? Nombre,
    AmbitoConcepto Ambito,
    EfectoConcepto Efecto,
    SentidoConcepto Sentido,
    CalculoConcepto Calculo,
    decimal Valor,
    RepartoConcepto Reparto = RepartoConcepto.PorImporte,
    string? TextoDocumento = null,
    IReadOnlyList<DatosAsignacionConcepto>? Asignaciones = null);
