using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Una línea (apunte) de un asiento: carga en el debe o abono en el haber de una cuenta.</summary>
public sealed class Apunte
{
    private Apunte()
    {
        CuentaCodigo = null!;
    }

    internal Apunte(Guid id, string cuentaCodigo, string? concepto, decimal debe, decimal haber)
    {
        Id = id;
        CuentaCodigo = cuentaCodigo;
        Concepto = concepto;
        Debe = debe;
        Haber = haber;
    }

    public Guid Id { get; private set; }

    public string CuentaCodigo { get; private set; }

    public string? Concepto { get; private set; }

    public decimal Debe { get; private set; }

    public decimal Haber { get; private set; }
}

/// <summary>Datos de una línea para componer un asiento.</summary>
public sealed record LineaAsiento(string CuentaCodigo, decimal Debe, decimal Haber, string? Concepto = null);

/// <summary>
/// Asiento contable por partida doble. Invariante fundamental: la suma del debe es igual a la
/// suma del haber (el asiento "cuadra"). Es inmutable una vez creado. Lleva número correlativo
/// por empresa y ejercicio.
/// </summary>
public sealed class Asiento : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaConcepto = 300;
    private readonly List<Apunte> _apuntes = new();

    private Asiento(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
        Origen = null!;
    }

    private Asiento(Guid id, Guid empresaId, int ejercicio, int numero, DateOnly fecha, string concepto, string origen, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        Fecha = fecha;
        Concepto = concepto;
        Origen = origen;
        CreadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Concepto { get; private set; }

    /// <summary>Cómo se generó el asiento (p. ej. «Manual» o «Compra»).</summary>
    public string Origen { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<Apunte> Apuntes => _apuntes;

    public decimal TotalDebe => Redondeo.Dos(_apuntes.Sum(a => a.Debe));

    public decimal TotalHaber => Redondeo.Dos(_apuntes.Sum(a => a.Haber));

    public static Resultado<Asiento> Crear(Guid empresaId, int ejercicio, int numero, DateOnly fecha,
        string? concepto, string? origen, IReadOnlyList<LineaAsiento> lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);

        if (string.IsNullOrWhiteSpace(concepto))
        {
            return Resultado.Fallo<Asiento>(Error.Validacion("asiento.concepto_vacio", "El concepto del asiento es obligatorio."));
        }

        if (lineas.Count < 2)
        {
            return Resultado.Fallo<Asiento>(Error.Validacion("asiento.pocas_lineas", "Un asiento necesita al menos dos apuntes."));
        }

        foreach (var linea in lineas)
        {
            if (string.IsNullOrWhiteSpace(linea.CuentaCodigo))
            {
                return Resultado.Fallo<Asiento>(Error.Validacion("asiento.cuenta_vacia", "Cada apunte debe indicar una cuenta."));
            }

            if (linea.Debe < 0m || linea.Haber < 0m)
            {
                return Resultado.Fallo<Asiento>(Error.Validacion("asiento.importe_negativo", "Los importes no pueden ser negativos."));
            }

            var tieneDebe = linea.Debe > 0m;
            var tieneHaber = linea.Haber > 0m;
            if (tieneDebe == tieneHaber)
            {
                return Resultado.Fallo<Asiento>(Error.Validacion("asiento.apunte_invalido", "Cada apunte debe cargar en el debe O abonar en el haber (no ambos ni ninguno)."));
            }
        }

        var totalDebe = Redondeo.Dos(lineas.Sum(l => l.Debe));
        var totalHaber = Redondeo.Dos(lineas.Sum(l => l.Haber));
        if (totalDebe <= 0m)
        {
            return Resultado.Fallo<Asiento>(Error.Validacion("asiento.importe_cero", "El asiento no puede tener importe cero."));
        }

        if (totalDebe != totalHaber)
        {
            return Resultado.Fallo<Asiento>(Error.Validacion("asiento.descuadrado", $"El asiento no cuadra: debe {totalDebe} ≠ haber {totalHaber}."));
        }

        var asiento = new Asiento(Guid.NewGuid(), empresaId, ejercicio, numero, fecha, concepto.Trim(),
            string.IsNullOrWhiteSpace(origen) ? "Manual" : origen.Trim(), reloj.AhoraUtc);
        foreach (var linea in lineas)
        {
            asiento._apuntes.Add(new Apunte(Guid.NewGuid(), linea.CuentaCodigo.Trim(), string.IsNullOrWhiteSpace(linea.Concepto) ? null : linea.Concepto.Trim(),
                Redondeo.Dos(linea.Debe), Redondeo.Dos(linea.Haber)));
        }

        asiento.RegistrarEvento(new AsientoRegistrado(asiento.Id, empresaId, ejercicio, numero, totalDebe, reloj.AhoraUtc));
        return Resultado.Ok(asiento);
    }
}

/// <summary>Se registró un asiento contable.</summary>
public sealed record AsientoRegistrado(Guid AsientoId, Guid EmpresaId, int Ejercicio, int Numero, decimal Total, DateTimeOffset OcurridoEn) : IEventoDominio;
