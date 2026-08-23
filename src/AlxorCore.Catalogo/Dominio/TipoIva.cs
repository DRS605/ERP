using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>
/// Clase o naturaleza de una operación a efectos de IVA. Determina si el tipo <b>repercute</b> cuota
/// y qué mención legal debe figurar en la factura.
/// </summary>
public enum ClaseIva
{
    /// <summary>Sujeta y no exenta: repercute IVA al porcentaje indicado (21/10/4…).</summary>
    Ordinario = 1,

    /// <summary>Sujeta pero exenta (p. ej. art. 20 LIVA). No repercute cuota; requiere mención.</summary>
    Exento = 2,

    /// <summary>No sujeta a IVA (p. ej. art. 7 LIVA). No repercute cuota; requiere mención.</summary>
    NoSujeto = 3,

    /// <summary>Inversión del sujeto pasivo (art. 84 LIVA): el destinatario autoliquida. No repercute.</summary>
    InversionSujetoPasivo = 4,

    /// <summary>IVA de importación (se aplica el porcentaje correspondiente al despacho aduanero).</summary>
    Importacion = 5,

    /// <summary>Entrega/adquisición intracomunitaria exenta (art. 25 LIVA). No repercute; requiere mención.</summary>
    Intracomunitario = 6,
}

/// <summary>Utilidades de la clase de IVA.</summary>
public static class ClaseIvaExtensiones
{
    /// <summary>¿La clase repercute cuota de IVA (aplica el porcentaje) en la factura emitida?</summary>
    public static bool Repercute(this ClaseIva clase) => clase is ClaseIva.Ordinario or ClaseIva.Importacion;
}

/// <summary>
/// <b>Tipo de IVA configurable por empresa</b>. Sustituye/extiende el catálogo estatal fijo por uno
/// editable por cada empresa, con todas las casuísticas (exenciones, no sujeción, inversión del
/// sujeto pasivo, importación, intracomunitario). Las facturas siguen guardando el porcentaje
/// aplicado (snapshot), por lo que editar un tipo no altera las facturas ya emitidas.
/// </summary>
public sealed class TipoIva : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaCodigo = 15;
    public const int LongitudMaximaNombre = 100;
    public const int LongitudMaximaMencion = 300;
    public const decimal PorcentajeMaximo = 100m;

    private TipoIva(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private TipoIva(Guid id, Guid empresaId, string codigo, string nombre, decimal porcentaje, decimal recargoEquivalencia, ClaseIva clase, string? mencionFactura, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Porcentaje = porcentaje;
        RecargoEquivalencia = recargoEquivalencia;
        Clase = clase;
        MencionFactura = mencionFactura;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Código estable dentro de la empresa (p. ej. <c>IVA21</c>, <c>ISP</c>, <c>EXENTO</c>).</summary>
    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Porcentaje de IVA. Solo se repercute si la <see cref="Clase"/> repercute.</summary>
    public decimal Porcentaje { get; private set; }

    /// <summary>Porcentaje de recargo de equivalencia asociado (0 si no aplica).</summary>
    public decimal RecargoEquivalencia { get; private set; }

    public ClaseIva Clase { get; private set; }

    /// <summary>Mención legal a imprimir en la factura (obligatoria en exenciones, ISP, etc.). Null = ninguna.</summary>
    public string? MencionFactura { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    /// <summary>Porcentaje que realmente se repercute en la factura (0 en clases sin repercusión).</summary>
    public decimal PorcentajeRepercutido => Clase.Repercute() ? Porcentaje : 0m;

    public static Resultado<TipoIva> Crear(Guid empresaId, string? codigo, string? nombre, decimal porcentaje, decimal recargoEquivalencia, ClaseIva clase, string? mencionFactura, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(ref codigo, ref nombre, porcentaje, recargoEquivalencia, clase);
        if (error is not null)
        {
            return Resultado.Fallo<TipoIva>(error);
        }

        return Resultado.Ok(new TipoIva(Guid.NewGuid(), empresaId, codigo!, nombre!, porcentaje, recargoEquivalencia, clase, Recortar(mencionFactura, LongitudMaximaMencion), reloj.AhoraUtc));
    }

    public Resultado Actualizar(string? nombre, decimal porcentaje, decimal recargoEquivalencia, ClaseIva clase, string? mencionFactura, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var codigo = Codigo; // el código no se cambia una vez creado (lo referencian artículos y facturas)
        var error = Validar(ref codigo, ref nombre, porcentaje, recargoEquivalencia, clase);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!;
        Porcentaje = porcentaje;
        RecargoEquivalencia = recargoEquivalencia;
        Clase = clase;
        MencionFactura = Recortar(mencionFactura, LongitudMaximaMencion);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public void FijarActivo(bool activo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Activo = activo;
        ActualizadoEn = reloj.AhoraUtc;
    }

    private static Error? Validar(ref string? codigo, ref string? nombre, decimal porcentaje, decimal recargo, ClaseIva clase)
    {
        codigo = Recortar(codigo, LongitudMaximaCodigo)?.ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Error.Validacion("tipoiva.codigo_vacio", "El código del tipo de IVA es obligatorio.");
        }

        nombre = Recortar(nombre, LongitudMaximaNombre);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("tipoiva.nombre_vacio", "El nombre del tipo de IVA es obligatorio.");
        }

        if (porcentaje is < 0m or > PorcentajeMaximo || recargo is < 0m or > PorcentajeMaximo)
        {
            return Error.Validacion("tipoiva.porcentaje_invalido", "El porcentaje no es válido.");
        }

        // En las clases sin repercusión el porcentaje debe ser 0 (no se factura cuota).
        if (!clase.Repercute() && porcentaje != 0m)
        {
            return Error.Validacion("tipoiva.clase_sin_porcentaje", "Las operaciones exentas, no sujetas, con inversión del sujeto pasivo o intracomunitarias no llevan porcentaje de IVA.");
        }

        return null;
    }

    private static string? Recortar(string? valor, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length > max ? limpio[..max] : limpio;
    }

    /// <summary>Conjunto estándar español con el que se siembra cada empresa (editable después).</summary>
    public static IReadOnlyList<(string Codigo, string Nombre, decimal Porcentaje, decimal Recargo, ClaseIva Clase, string? Mencion)> Predeterminados { get; } = new[]
    {
        ("IVA21", "IVA general (21%)", 21m, 5.2m, ClaseIva.Ordinario, (string?)null),
        ("IVA10", "IVA reducido (10%)", 10m, 1.4m, ClaseIva.Ordinario, null),
        ("IVA4", "IVA superreducido (4%)", 4m, 0.5m, ClaseIva.Ordinario, null),
        ("IVA0", "Exento / 0%", 0m, 0m, ClaseIva.Exento, "Operación exenta de IVA (art. 20 Ley 37/1992)."),
        ("NOSUJETO", "No sujeto a IVA", 0m, 0m, ClaseIva.NoSujeto, "Operación no sujeta a IVA (art. 7 Ley 37/1992)."),
        ("ISP", "Inversión del sujeto pasivo", 0m, 0m, ClaseIva.InversionSujetoPasivo, "Inversión del sujeto pasivo (art. 84.Uno.2º Ley 37/1992)."),
        ("INTRA", "Entrega intracomunitaria exenta", 0m, 0m, ClaseIva.Intracomunitario, "Entrega intracomunitaria exenta (art. 25 Ley 37/1992)."),
        ("IMPORT21", "IVA importación (21%)", 21m, 0m, ClaseIva.Importacion, null),
    };
}
