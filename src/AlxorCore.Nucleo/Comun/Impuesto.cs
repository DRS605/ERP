using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Nucleo.Comun;

/// <summary>Tipo de impuesto.</summary>
public enum TipoImpuesto
{
    /// <summary>IVA repercutido/soportado.</summary>
    Iva = 1,

    /// <summary>Retención de IRPF.</summary>
    Irpf = 2,

    /// <summary>Impuesto General Indirecto Canario: sustituye al IVA en Canarias.</summary>
    Igic = 3,
}

/// <summary>
/// Territorio fiscal de la empresa: decide qué impuesto indirecto repercute. Canarias está fuera del
/// territorio de aplicación del IVA y tributa por IGIC (Ley 20/1991), con sus propios tipos y su
/// autoliquidación (modelo 420, ante la Agencia Tributaria Canaria).
/// </summary>
public enum TerritorioFiscal
{
    /// <summary>Península y Baleares: IVA.</summary>
    Comun = 1,

    /// <summary>Canarias: IGIC.</summary>
    Canarias = 2,
}

/// <summary>Utilidades del territorio fiscal.</summary>
public static class TerritorioFiscalExtensiones
{
    /// <summary>Impuesto indirecto que se repercute en el territorio.</summary>
    public static TipoImpuesto ImpuestoIndirecto(this TerritorioFiscal territorio) =>
        territorio == TerritorioFiscal.Canarias ? TipoImpuesto.Igic : TipoImpuesto.Iva;

    /// <summary>Nombre corto del impuesto para documentos y pantallas ("IVA" o "IGIC").</summary>
    public static string Siglas(this TipoImpuesto impuesto) => impuesto == TipoImpuesto.Igic ? "IGIC" : "IVA";
}

/// <summary>
/// Catálogo de tipos de IVA españoles. Al ser tipos nacionales y estables, se modelan como
/// constantes de código (no como datos editables por empresa): mantiene la fiscalidad simple y
/// versionada. Las facturas guardan una copia del porcentaje aplicado (snapshot), por lo que un
/// cambio futuro de tipos no altera las facturas ya emitidas.
/// </summary>
public sealed class Impuesto
{
    public static readonly Impuesto IvaGeneral = new("IVA21", TipoImpuesto.Iva, 21m, "IVA general (21%)");
    public static readonly Impuesto IvaReducido = new("IVA10", TipoImpuesto.Iva, 10m, "IVA reducido (10%)");
    public static readonly Impuesto IvaSuperreducido = new("IVA4", TipoImpuesto.Iva, 4m, "IVA superreducido (4%)");
    public static readonly Impuesto IvaExento = new("IVA0", TipoImpuesto.Iva, 0m, "Exento / 0%");

    // IGIC (Canarias). Tipos vigentes de la Ley 4/2012 de medidas administrativas y fiscales de
    // Canarias; la empresa puede ajustarlos en su catálogo si cambian.
    public static readonly Impuesto IgicCero = new("IGIC0", TipoImpuesto.Igic, 0m, "IGIC tipo cero (0%)");
    public static readonly Impuesto IgicReducido = new("IGIC3", TipoImpuesto.Igic, 3m, "IGIC reducido (3%)");
    public static readonly Impuesto IgicGeneral = new("IGIC7", TipoImpuesto.Igic, 7m, "IGIC general (7%)");
    public static readonly Impuesto IgicIncrementado = new("IGIC95", TipoImpuesto.Igic, 9.5m, "IGIC incrementado (9,5%)");
    public static readonly Impuesto IgicIncrementado15 = new("IGIC15", TipoImpuesto.Igic, 15m, "IGIC incrementado (15%)");
    public static readonly Impuesto IgicEspecial = new("IGIC20", TipoImpuesto.Igic, 20m, "IGIC especial incrementado (20%)");

    private static readonly Dictionary<string, Impuesto> PorCodigo =
        new[] { IvaGeneral, IvaReducido, IvaSuperreducido, IvaExento, IgicCero, IgicReducido, IgicGeneral, IgicIncrementado, IgicIncrementado15, IgicEspecial }
            .ToDictionary(i => i.Codigo, StringComparer.OrdinalIgnoreCase);

    private Impuesto(string codigo, TipoImpuesto tipo, decimal porcentaje, string nombre)
    {
        Codigo = codigo;
        Tipo = tipo;
        Porcentaje = porcentaje;
        Nombre = nombre;
    }

    /// <summary>Código estable (p. ej. <c>IVA21</c>).</summary>
    public string Codigo { get; }

    public TipoImpuesto Tipo { get; }

    /// <summary>Porcentaje (p. ej. 21).</summary>
    public decimal Porcentaje { get; }

    /// <summary>Nombre legible en español.</summary>
    public string Nombre { get; }

    /// <summary>Todos los tipos de IVA disponibles.</summary>
    public static IReadOnlyCollection<Impuesto> TodosIva => PorCodigo.Values.Where(i => i.Tipo == TipoImpuesto.Iva).ToList();

    /// <summary>Tipos del impuesto indirecto indicado (IVA o IGIC).</summary>
    public static IReadOnlyCollection<Impuesto> De(TipoImpuesto tipo) => PorCodigo.Values.Where(i => i.Tipo == tipo).ToList();

    /// <summary>Impuesto al que pertenece un código del catálogo estatal (IVA si no se conoce).</summary>
    public static TipoImpuesto TipoDeCodigo(string? codigo) =>
        !string.IsNullOrWhiteSpace(codigo) && PorCodigo.TryGetValue(codigo, out var i) ? i.Tipo : TipoImpuesto.Iva;

    /// <summary>Resuelve un impuesto por su código.</summary>
    public static Resultado<Impuesto> PorCodigoImpuesto(string? codigo)
    {
        if (!string.IsNullOrWhiteSpace(codigo) && PorCodigo.TryGetValue(codigo, out var impuesto))
        {
            return Resultado.Ok(impuesto);
        }

        return Resultado.Fallo<Impuesto>(Error.Validacion("impuesto.desconocido", $"El impuesto «{codigo}» no existe."));
    }

    /// <summary>
    /// Porcentaje de <b>recargo de equivalencia</b> que corresponde a un tipo de IVA (régimen especial
    /// de minoristas): 21 % → 5,2 %; 10 % → 1,4 %; 4 % → 0,5 %; resto → 0 %.
    /// </summary>
    /// <summary>Recargo de equivalencia del tipo de IVA (el IGIC no tiene recargo de equivalencia).</summary>
    public static decimal RecargoEquivalencia(decimal porcentajeIva) => porcentajeIva switch
    {
        21m => 5.2m,
        10m => 1.4m,
        4m => 0.5m,
        _ => 0m,
    };
}
