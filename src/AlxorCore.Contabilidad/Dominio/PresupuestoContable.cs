using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Estado de un presupuesto contable.</summary>
public enum EstadoPresupuesto
{
    /// <summary>Se puede editar.</summary>
    Borrador = 1,

    /// <summary>Congelado: es la referencia del seguimiento. Para cambiarlo, se copia a otra versión.</summary>
    Aprobado = 2,
}

/// <summary>Línea de presupuesto: una cuenta (o grupo de cuentas por prefijo), con centro y partida opcionales, mes a mes.</summary>
public sealed class LineaPresupuesto
{
    private LineaPresupuesto()
    {
        CuentaCodigo = null!;
        Importes = [];
    }

    internal LineaPresupuesto(string cuenta, Guid? centroId, Guid? partidaId, decimal[] importes)
    {
        Id = Guid.NewGuid();
        CuentaCodigo = cuenta;
        CentroId = centroId;
        PartidaId = partidaId;
        Importes = importes;
    }

    public Guid Id { get; private set; }

    /// <summary>Cuenta o prefijo de los grupos 6 o 7 («62» presupuesta todo el subgrupo 62).</summary>
    public string CuentaCodigo { get; private set; }

    public Guid? CentroId { get; private set; }

    public Guid? PartidaId { get; private set; }

    /// <summary>Importe de cada mes del presupuesto (en positivo: gasto previsto o ingreso previsto).</summary>
    public decimal[] Importes { get; private set; }

    public NaturalezaAnalitica Naturaleza => MotorAnalitico.NaturalezaDe(CuentaCodigo)!.Value;

    public decimal Total => Importes.Sum();
}

/// <summary>Datos de una línea: el importe de cada mes, o un total que se reparte por igual al céntimo.</summary>
public sealed record DatosLineaPresupuesto(string? CuentaCodigo, Guid? CentroId = null, Guid? PartidaId = null, IReadOnlyList<decimal>? Importes = null, decimal? Total = null);

/// <summary>
/// Presupuesto contable de un periodo (de 1 a 24 meses desde un mes cualquiera: un año natural o una
/// campaña de septiembre a agosto). Se compara con el real de la contabilidad (por cuenta) y de la
/// analítica (por centro y partida).
/// </summary>
public sealed class PresupuestoContable : RaizAgregadoEmpresa<Guid>
{
    public const int MesesMaximos = 24;

    private readonly List<LineaPresupuesto> _lineas = [];

    private PresupuestoContable(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private PresupuestoContable(Guid id, Guid empresaId, string codigo, string nombre, DateOnly desde, int meses, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Desde = desde;
        Meses = meses;
        Estado = EstadoPresupuesto.Borrador;
        CreadoEn = ahora;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Primer día del primer mes.</summary>
    public DateOnly Desde { get; private set; }

    public int Meses { get; private set; }

    public EstadoPresupuesto Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaPresupuesto> Lineas => _lineas.AsReadOnly();

    /// <summary>Último día del periodo.</summary>
    public DateOnly Hasta => Desde.AddMonths(Meses).AddDays(-1);

    /// <summary>Índice del mes (0 = primero) de una fecha del periodo, o null si cae fuera.</summary>
    public int? MesDe(DateOnly fecha)
    {
        var i = ((fecha.Year - Desde.Year) * 12) + fecha.Month - Desde.Month;
        return i >= 0 && i < Meses ? i : null;
    }

    public static Resultado<PresupuestoContable> Crear(Guid empresaId, string? codigo, string? nombre, DateOnly desde, int meses, DateTimeOffset ahora)
    {
        var error = MaestroAnalitico.Validar(ref codigo, ref nombre, "presupuesto");
        if (error is null && (meses is < 1 or > MesesMaximos))
        {
            error = Error.Validacion("presupuesto.meses", $"El presupuesto abarca de 1 a {MesesMaximos} meses.");
        }

        return error is not null
            ? Resultado.Fallo<PresupuestoContable>(error)
            : Resultado.Ok(new PresupuestoContable(Guid.NewGuid(), empresaId, codigo!, nombre!, new DateOnly(desde.Year, desde.Month, 1), meses, ahora));
    }

    /// <summary>Sustituye las líneas (solo en borrador).</summary>
    public Resultado FijarLineas(IReadOnlyList<DatosLineaPresupuesto> lineas)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (Estado == EstadoPresupuesto.Aprobado)
        {
            return Resultado.Fallo(Error.Conflicto("presupuesto.aprobado", "El presupuesto está aprobado: cópialo a una versión nueva para cambiarlo."));
        }

        var nuevas = new List<LineaPresupuesto>(lineas.Count);
        foreach (var l in lineas)
        {
            var cuenta = l.CuentaCodigo?.Trim();
            if (string.IsNullOrEmpty(cuenta) || cuenta.Length > 12 || !cuenta.All(char.IsAsciiDigit) || MotorAnalitico.NaturalezaDe(cuenta) is null)
            {
                return Resultado.Fallo(Error.Validacion("presupuesto.cuenta", $"La cuenta «{cuenta}» no es de gastos (6) ni de ingresos (7)."));
            }

            decimal[] importes;
            if (l.Importes is { } mensuales)
            {
                if (l.Total is not null || mensuales.Count != Meses)
                {
                    return Resultado.Fallo(Error.Validacion("presupuesto.importes", $"Indica los {Meses} importes mensuales o un total (uno de los dos)."));
                }

                importes = mensuales.Select(i => decimal.Round(i, 2, MidpointRounding.AwayFromZero)).ToArray();
            }
            else if (l.Total is { } total)
            {
                // Reparto por igual al céntimo (el resto va a los primeros meses).
                importes = MotorAnalitico.Repartir(total, Enumerable.Repeat(100m / Meses, Meses).ToList()).ToArray();
                importes[0] += decimal.Round(total, 2, MidpointRounding.AwayFromZero) - importes.Sum();
            }
            else
            {
                return Resultado.Fallo(Error.Validacion("presupuesto.importes", $"Indica los {Meses} importes mensuales o un total."));
            }

            if (importes.Any(i => i < 0m))
            {
                return Resultado.Fallo(Error.Validacion("presupuesto.importes", "Los importes presupuestados van en positivo (gasto o ingreso previsto)."));
            }

            if (nuevas.Any(n => n.CuentaCodigo == cuenta && n.CentroId == l.CentroId && n.PartidaId == l.PartidaId))
            {
                return Resultado.Fallo(Error.Validacion("presupuesto.duplicada", $"La cuenta {cuenta} aparece dos veces con el mismo centro y partida."));
            }

            nuevas.Add(new LineaPresupuesto(cuenta, l.CentroId, l.PartidaId, importes));
        }

        _lineas.Clear();
        _lineas.AddRange(nuevas);
        return Resultado.Ok();
    }

    public Resultado Aprobar()
    {
        if (_lineas.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("presupuesto.vacio", "Un presupuesto sin líneas no se puede aprobar."));
        }

        Estado = EstadoPresupuesto.Aprobado;
        return Resultado.Ok();
    }
}
