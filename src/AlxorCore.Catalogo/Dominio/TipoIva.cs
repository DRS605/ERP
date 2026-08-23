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

    /// <summary>
    /// Régimen especial de viajeros: exención en exportación de bienes en el equipaje personal de
    /// viajeros no residentes en la UE (art. 21.2º LIVA), con devolución vía DIVA. No repercute cuota.
    /// </summary>
    /// <remarks>
    /// La mecánica completa (datos del viajero no residente, documento DIVA y devolución del IVA) se
    /// implementará con la puesta en marcha fiscal; aquí se modela la exención y su mención en factura.
    /// </remarks>
    Viajeros = 7,

    /// <summary>
    /// Régimen especial de bienes usados, objetos de arte, antigüedades y objetos de colección
    /// (REBU, art. 135 LIVA): el IVA se calcula sobre el <b>margen</b> y no se desglosa al comprador.
    /// </summary>
    /// <remarks>
    /// El cálculo sobre el margen se incorporará con el bloque fiscal; por ahora se modela como una
    /// operación sin cuota desglosada y con su mención obligatoria.
    /// </remarks>
    BienesUsados = 8,

    /// <summary>
    /// Régimen especial de las agencias de viajes (art. 141 LIVA): IVA sobre el margen, sin desglose
    /// al cliente. No repercute cuota desglosada; requiere mención.
    /// </summary>
    AgenciasViajes = 9,

    /// <summary>Oro de inversión exento (art. 140 bis LIVA). No repercute cuota; requiere mención.</summary>
    OroInversion = 10,

    /// <summary>
    /// Régimen especial del criterio de caja (RECC, art. 163 decies y ss. LIVA): <b>repercute</b> IVA
    /// al porcentaje ordinario, pero el devengo se difiere al momento del cobro. Requiere mención.
    /// </summary>
    /// <remarks>
    /// El diferimiento del devengo al cobro (a efectos de los libros y del modelo 303) se tratará con
    /// el bloque fiscal; aquí se modela la repercusión normal y su mención en factura.
    /// </remarks>
    CriterioCaja = 11,

    /// <summary>
    /// Régimen especial de la agricultura, ganadería y pesca (REAGP, art. 124-134 bis LIVA). El
    /// titular no repercute IVA: el destinatario satisface una <b>compensación a tanto alzado</b>
    /// (12 % agrícola/forestal, 10,5 % ganadera/pesquera) que sí se añade al importe. Requiere mención.
    /// </summary>
    /// <remarks>
    /// La compensación se modela como un porcentaje repercutido (funciona como importe añadido a la
    /// base); su tratamiento diferenciado en libros y modelo 303 llega con el bloque fiscal.
    /// </remarks>
    AgriculturaCompensacion = 12,

    /// <summary>
    /// Exportación de bienes fuera de la UE, exenta (art. 21 LIVA). No repercute cuota; requiere
    /// mención. (El régimen de viajeros es un subcaso con devolución al viajero; esta clase es la
    /// exportación general.)
    /// </summary>
    Exportacion = 13,

    /// <summary>
    /// Ventanilla única — régimen de la Unión/OSS (art. 163 unvicies y ss. LIVA): en ventas B2C
    /// intracomunitarias a distancia se <b>repercute el IVA del país de destino</b>. El porcentaje se
    /// configura según el país; requiere mención.
    /// </summary>
    /// <remarks>
    /// La declaración por ventanilla única (modelo 369) y la selección automática del tipo por país
    /// llegan con el bloque fiscal; aquí se modela la repercusión del tipo indicado y su mención.
    /// </remarks>
    VentanillaUnicaOSS = 14,
}

/// <summary>Utilidades de la clase de IVA.</summary>
public static class ClaseIvaExtensiones
{
    /// <summary>
    /// ¿La clase repercute un porcentaje en la factura emitida (IVA ordinario, IVA de destino OSS, o
    /// compensación a tanto alzado del REAGP)? Las clases exentas/no sujetas devuelven <c>false</c>.
    /// </summary>
    public static bool Repercute(this ClaseIva clase) =>
        clase is ClaseIva.Ordinario or ClaseIva.Importacion or ClaseIva.CriterioCaja
            or ClaseIva.AgriculturaCompensacion or ClaseIva.VentanillaUnicaOSS;
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

        // En las clases sin repercusión el porcentaje debe ser 0 (no se factura cuota desglosada).
        if (!clase.Repercute() && porcentaje != 0m)
        {
            return Error.Validacion("tipoiva.clase_sin_porcentaje", "Las operaciones exentas, no sujetas, con inversión del sujeto pasivo, intracomunitarias, de exportación, de viajeros, bienes usados, agencias de viajes u oro de inversión no llevan porcentaje de IVA desglosado.");
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
        ("EXPORT", "Exportación exenta", 0m, 0m, ClaseIva.Exportacion, "Operación exenta. Exportación de bienes (art. 21 Ley 37/1992)."),
        ("VIAJEROS", "Régimen especial de viajeros", 0m, 0m, ClaseIva.Viajeros, "Exención en exportación en régimen de viajeros (art. 21.2º Ley 37/1992)."),
        ("REBU", "Bienes usados (margen)", 0m, 0m, ClaseIva.BienesUsados, "Régimen especial de los bienes usados, objetos de arte, antigüedades y objetos de colección (art. 135 Ley 37/1992)."),
        ("AGENCIAS", "Agencias de viajes (margen)", 0m, 0m, ClaseIva.AgenciasViajes, "Régimen especial de las agencias de viajes (art. 141 Ley 37/1992)."),
        ("ORO", "Oro de inversión exento", 0m, 0m, ClaseIva.OroInversion, "Operación exenta. Oro de inversión (art. 140 bis Ley 37/1992)."),
        ("CAJA21", "Criterio de caja (21%)", 21m, 5.2m, ClaseIva.CriterioCaja, "Régimen especial del criterio de caja (art. 163 decies y siguientes Ley 37/1992)."),
        ("REAGP12", "REAGP agrícola/forestal (comp. 12%)", 12m, 0m, ClaseIva.AgriculturaCompensacion, "Compensación a tanto alzado del régimen especial de la agricultura, ganadería y pesca (art. 130 Ley 37/1992)."),
        ("REAGP105", "REAGP ganadera/pesquera (comp. 10,5%)", 10.5m, 0m, ClaseIva.AgriculturaCompensacion, "Compensación a tanto alzado del régimen especial de la agricultura, ganadería y pesca (art. 130 Ley 37/1992)."),
    };
}
