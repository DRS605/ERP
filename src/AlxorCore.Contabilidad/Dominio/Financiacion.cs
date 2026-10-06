using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Clase de operación de financiación.</summary>
public enum TipoFinanciacion
{
    /// <summary>Préstamo bancario: se recibe el capital y se devuelve en cuotas (170 / 520).</summary>
    Prestamo = 1,

    /// <summary>Arrendamiento financiero: se recibe un bien y se paga en cuotas con IVA, opción de compra al final (174 / 524).</summary>
    Leasing = 2,

    /// <summary>Póliza de crédito: un límite del que se dispone y se reintegra; se liquidan intereses por lo dispuesto (5201).</summary>
    Poliza = 3,
}

/// <summary>Cómo se devuelve el capital de un préstamo o leasing.</summary>
public enum SistemaAmortizacionPrestamo
{
    /// <summary>Cuota constante (capital creciente, intereses decrecientes).</summary>
    Frances = 1,

    /// <summary>Capital constante en cada cuota (cuota decreciente).</summary>
    CapitalConstante = 2,
}

/// <summary>Periodicidad de las cuotas; el valor es el número de meses entre cuotas.</summary>
public enum PeriodicidadCuota
{
    Mensual = 1,
    Trimestral = 3,
    Semestral = 6,
    Anual = 12,
}

public enum EstadoFinanciacion
{
    /// <summary>Con capital pendiente o dispuesto.</summary>
    Vigente = 1,

    /// <summary>Devuelta del todo (todas las cuotas pagadas o cancelada anticipadamente; póliza cerrada).</summary>
    Cancelada = 2,
}

/// <summary>Lo que ha pasado en una operación; los que llevan asiento se deshacen del último al primero.</summary>
public enum TipoEventoFinanciacion
{
    Formalizacion = 1,
    Cuota = 2,
    RevisionTipo = 3,
    AmortizacionAnticipada = 4,
    Reclasificacion = 5,
    Disposicion = 6,
    Reintegro = 7,
    LiquidacionIntereses = 8,
}

/// <summary>Hecho de una operación de financiación (formalización, cuota pagada, revisión, disposición…).</summary>
public sealed class EventoFinanciacion
{
    private EventoFinanciacion() { }

    internal EventoFinanciacion(int orden, TipoEventoFinanciacion tipo, DateOnly fecha)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Tipo = tipo;
        Fecha = fecha;
    }

    public Guid Id { get; private set; }

    /// <summary>Secuencia dentro de la operación (se deshace el de mayor orden).</summary>
    public int Orden { get; private set; }

    public TipoEventoFinanciacion Tipo { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Cuota: su número. Revisión: primera cuota con el nuevo tipo. Amortización anticipada: cuotas ya vencidas.</summary>
    public int? Numero { get; internal set; }

    /// <summary>Reclasificación: ejercicio a cuyo cierre se pasa a corto plazo el capital del siguiente.</summary>
    public int? Ejercicio { get; internal set; }

    /// <summary>Capital de la cuota, importe anticipado, dispuesto, reintegrado o reclasificado.</summary>
    public decimal Importe { get; internal set; }

    public decimal Intereses { get; internal set; }

    /// <summary>Comisión (de amortización anticipada o de no disponibilidad).</summary>
    public decimal Comision { get; internal set; }

    /// <summary>IVA de la cuota del leasing.</summary>
    public decimal Iva { get; internal set; }

    /// <summary>Revisión: nuevo tipo nominal anual (%).</summary>
    public decimal? TipoInteres { get; internal set; }

    /// <summary>Liquidación de intereses: primer día del periodo (la fecha es el último).</summary>
    public DateOnly? Desde { get; internal set; }

    public Guid? AsientoId { get; internal set; }
}

/// <summary>Fila del cuadro de amortización de un préstamo o leasing.</summary>
public sealed record CuotaCuadro(int Numero, DateOnly Fecha, decimal Intereses, decimal Capital, decimal Pendiente, bool OpcionCompra = false)
{
    public decimal Cuota => Redondeo.Dos(Intereses + Capital);
}

/// <summary>Revisión del tipo de interés a partir de una cuota.</summary>
public sealed record RevisionCuadro(int DesdeCuota, decimal TipoInteres);

/// <summary>Devolución anticipada de capital tras una cuota (reduce la cuota y mantiene el plazo).</summary>
public sealed record AnticipoCuadro(int TrasCuota, decimal Importe);

/// <summary>Cálculo puro del cuadro de amortización.</summary>
public static class CalculadoraPrestamo
{
    /// <summary>
    /// Cuadro con <paramref name="numeroCuotas"/> cuotas (las de carencia solo pagan intereses) y, si hay valor residual,
    /// una fila más de opción de compra un periodo después de la última. Las revisiones de tipo y los anticipos recalculan
    /// la cuota de lo que queda, manteniendo el plazo; la última cuota recoge el redondeo.
    /// </summary>
    public static IReadOnlyList<CuotaCuadro> Calcular(decimal capital, decimal tipoInteres, PeriodicidadCuota periodicidad, int numeroCuotas,
        int cuotasCarencia, DateOnly primeraCuota, SistemaAmortizacionPrestamo sistema, decimal valorResidual,
        IReadOnlyList<RevisionCuadro>? revisiones = null, IReadOnlyList<AnticipoCuadro>? anticipos = null)
    {
        var meses = (int)periodicidad;
        var filas = new List<CuotaCuadro>();
        var pendiente = capital;
        var tipo = tipoInteres;
        decimal? cuotaFija = null;
        for (var k = 1; k <= numeroCuotas; k++)
        {
            if (revisiones?.Where(r => r.DesdeCuota == k).Select(r => (decimal?)r.TipoInteres).LastOrDefault() is { } nuevo)
            {
                tipo = nuevo;
                cuotaFija = null;
            }

            var anticipado = anticipos?.Where(a => a.TrasCuota == k - 1).Sum(a => a.Importe) ?? 0m;
            if (anticipado > 0m)
            {
                pendiente = Redondeo.Dos(pendiente - anticipado);
                cuotaFija = null;
            }

            if (pendiente <= 0m)
            {
                break;
            }

            var i = tipo / 100m * meses / 12m;
            var intereses = Redondeo.Dos(pendiente * i);
            decimal amortizado;
            if (k <= cuotasCarencia)
            {
                amortizado = 0m;
            }
            else if (k == numeroCuotas)
            {
                amortizado = Redondeo.Dos(pendiente - valorResidual);
            }
            else
            {
                var restantes = numeroCuotas - k + 1;
                if (sistema == SistemaAmortizacionPrestamo.Frances)
                {
                    cuotaFija ??= CuotaFrancesa(pendiente, i, restantes, valorResidual);
                    amortizado = Redondeo.Dos(cuotaFija.Value - intereses);
                }
                else
                {
                    cuotaFija ??= Redondeo.Dos((pendiente - valorResidual) / restantes);
                    amortizado = cuotaFija.Value;
                }

                amortizado = Math.Clamp(amortizado, 0m, Math.Max(0m, pendiente - valorResidual));
            }

            pendiente = Redondeo.Dos(pendiente - amortizado);
            filas.Add(new CuotaCuadro(k, primeraCuota.AddMonths((k - 1) * meses), intereses, amortizado, pendiente));
        }

        if (pendiente > 0m && filas.Count == numeroCuotas)
        {
            // Opción de compra del leasing: el valor residual, sin intereses, un periodo después de la última cuota.
            filas.Add(new CuotaCuadro(numeroCuotas + 1, primeraCuota.AddMonths(numeroCuotas * meses), 0m, pendiente, 0m, OpcionCompra: true));
        }

        return filas;
    }

    /// <summary>Cuota constante que amortiza <paramref name="capital"/> hasta dejar <paramref name="residual"/> en <paramref name="n"/> periodos.</summary>
    public static decimal CuotaFrancesa(decimal capital, decimal i, int n, decimal residual = 0m)
    {
        if (n <= 0)
        {
            return capital;
        }

        if (i == 0m)
        {
            return Redondeo.Dos((capital - residual) / n);
        }

        var factor = (decimal)Math.Pow(1d + (double)i, n);
        return Redondeo.Dos((capital - (residual / factor)) * i * factor / (factor - 1m));
    }
}

/// <summary>Movimiento de una póliza de crédito: saldo dispuesto desde una fecha.</summary>
public sealed record TramoPoliza(DateOnly Desde, decimal Dispuesto);

/// <summary>
/// Operación de financiación de la empresa: préstamo, leasing o póliza de crédito. Guarda las condiciones y los hechos
/// (<see cref="EventoFinanciacion"/>); el cuadro de amortización se calcula de las condiciones, las revisiones de tipo y
/// los anticipos.
/// <para>El capital pendiente se reparte entre largo y corto plazo así: está en corto plazo el de las cuotas pendientes
/// que vencen hasta el 31/12 del <see cref="HorizonteCortoPlazo"/> (al formalizar, el año de la formalización; cada
/// reclasificación de cierre lo adelanta un año). Cada cuota se paga desde la cuenta en que está su capital.</para>
/// </summary>
public sealed class OperacionFinanciacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 20;
    public const int LongitudDescripcion = 200;
    public const int LongitudEntidad = 120;
    public const int CuotasMaximas = 600;

    private readonly List<EventoFinanciacion> _eventos = [];

    private OperacionFinanciacion(Guid id) : base(id, Guid.Empty)
    {
        Codigo = null!; Descripcion = null!; CuentaCortoPlazo = null!; CuentaIntereses = null!; CuentaTesoreria = null!; CuentaComisiones = null!;
    }

    private OperacionFinanciacion(Guid id, Guid empresaId) : base(id, empresaId)
    {
        Codigo = null!; Descripcion = null!; CuentaCortoPlazo = null!; CuentaIntereses = null!; CuentaTesoreria = null!; CuentaComisiones = null!;
    }

    public TipoFinanciacion Tipo { get; private set; }

    public string Codigo { get; private set; }

    public string Descripcion { get; private set; }

    /// <summary>Banco o entidad financiera.</summary>
    public string? Entidad { get; private set; }

    public EstadoFinanciacion Estado { get; private set; }

    public DateOnly FechaFormalizacion { get; private set; }

    /// <summary>Préstamo y leasing: capital (en el leasing, el valor al contado del bien, opción de compra incluida). Póliza: límite.</summary>
    public decimal Capital { get; private set; }

    /// <summary>Tipo nominal anual inicial (%).</summary>
    public decimal TipoInteres { get; private set; }

    public SistemaAmortizacionPrestamo Sistema { get; private set; }

    public PeriodicidadCuota Periodicidad { get; private set; }

    public int NumeroCuotas { get; private set; }

    /// <summary>Cuotas iniciales en que solo se pagan intereses (incluidas en <see cref="NumeroCuotas"/>).</summary>
    public int CuotasCarencia { get; private set; }

    public DateOnly? FechaPrimeraCuota { get; private set; }

    /// <summary>Leasing: opción de compra (se paga un periodo después de la última cuota).</summary>
    public decimal ValorResidual { get; private set; }

    /// <summary>Leasing: IVA de cada cuota (%).</summary>
    public decimal PorcentajeIva { get; private set; }

    /// <summary>Póliza: vencimiento (hay que devolver lo dispuesto).</summary>
    public DateOnly? FechaVencimiento { get; private set; }

    /// <summary>Póliza: comisión anual de no disponibilidad sobre lo no dispuesto (%).</summary>
    public decimal ComisionNoDisponible { get; private set; }

    /// <summary>Cuotas con fecha anterior se pagaron antes de llevar la operación aquí (sin asiento).</summary>
    public DateOnly? ContabilizarDesde { get; private set; }

    /// <summary>Deuda a largo plazo (170 préstamo, 174 leasing); la póliza no tiene.</summary>
    public string? CuentaLargoPlazo { get; private set; }

    /// <summary>Deuda a corto plazo (520 préstamo, 524 leasing, 5201 póliza).</summary>
    public string CuentaCortoPlazo { get; private set; }

    public string CuentaIntereses { get; private set; }

    /// <summary>Banco donde se recibe el dinero y se cargan las cuotas.</summary>
    public string CuentaTesoreria { get; private set; }

    /// <summary>Comisiones bancarias (amortización anticipada, no disponibilidad).</summary>
    public string CuentaComisiones { get; private set; }

    /// <summary>Leasing: cuenta del bien (2xx) que se da de alta al formalizar.</summary>
    public string? CuentaActivo { get; private set; }

    public IReadOnlyList<EventoFinanciacion> Eventos => _eventos;

    public bool EsPoliza => Tipo == TipoFinanciacion.Poliza;

    // ------------------------------------------------------------------ alta

    public static Resultado<OperacionFinanciacion> Crear(Guid empresaId, TipoFinanciacion tipo, string? codigo, string? descripcion, string? entidad,
        DateOnly fechaFormalizacion, decimal capital, decimal tipoInteres, SistemaAmortizacionPrestamo sistema, PeriodicidadCuota periodicidad,
        int numeroCuotas, int cuotasCarencia, DateOnly? fechaPrimeraCuota, decimal valorResidual, decimal porcentajeIva, DateOnly? fechaVencimiento,
        decimal comisionNoDisponible, DateOnly? contabilizarDesde, string? cuentaLargoPlazo, string? cuentaCortoPlazo, string? cuentaIntereses,
        string? cuentaTesoreria, string? cuentaComisiones, string? cuentaActivo)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Fallo("financiacion.tipo", "El tipo es Prestamo, Leasing o Poliza.");
        }

        var cod = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (cod.Length is 0 or > LongitudCodigo)
        {
            return Fallo("financiacion.codigo", $"El código es obligatorio (hasta {LongitudCodigo} caracteres).");
        }

        var desc = (descripcion ?? string.Empty).Trim();
        if (desc.Length is 0 or > LongitudDescripcion)
        {
            return Fallo("financiacion.descripcion", $"La descripción es obligatoria (hasta {LongitudDescripcion} caracteres).");
        }

        var ent = string.IsNullOrWhiteSpace(entidad) ? null : entidad.Trim();
        if (ent is { Length: > LongitudEntidad })
        {
            return Fallo("financiacion.entidad", "El nombre de la entidad es demasiado largo.");
        }

        if (capital <= 0m || decimal.Round(capital, 2) != capital)
        {
            return Fallo("financiacion.capital", tipo == TipoFinanciacion.Poliza ? "El límite es positivo, con 2 decimales como mucho." : "El capital es positivo, con 2 decimales como mucho.");
        }

        if (tipoInteres is < 0m or > 50m)
        {
            return Fallo("financiacion.tipo_interes", "El tipo de interés anual va de 0 a 50 %.");
        }

        var op = new OperacionFinanciacion(Guid.NewGuid(), empresaId)
        {
            Tipo = tipo, Codigo = cod, Descripcion = desc, Entidad = ent, Estado = EstadoFinanciacion.Vigente, FechaFormalizacion = fechaFormalizacion,
            Capital = capital, TipoInteres = tipoInteres, ContabilizarDesde = contabilizarDesde,
        };

        string? Cuenta(string? valor, string defecto) => string.IsNullOrWhiteSpace(valor) ? defecto : valor.Trim();
        if (tipo == TipoFinanciacion.Poliza)
        {
            if (fechaVencimiento is not { } vto || vto <= fechaFormalizacion)
            {
                return Fallo("financiacion.vencimiento", "La póliza necesita un vencimiento posterior a la formalización.");
            }

            if (comisionNoDisponible is < 0m or > 10m)
            {
                return Fallo("financiacion.comision", "La comisión de no disponibilidad va de 0 a 10 %.");
            }

            op.FechaVencimiento = vto;
            op.ComisionNoDisponible = comisionNoDisponible;
            op.Periodicidad = PeriodicidadCuota.Mensual;
            op.Sistema = SistemaAmortizacionPrestamo.Frances;
            op.CuentaCortoPlazo = Cuenta(cuentaCortoPlazo, "5201")!;
        }
        else
        {
            if (!Enum.IsDefined(sistema) || !Enum.IsDefined(periodicidad))
            {
                return Fallo("financiacion.sistema", "Indica el sistema (Frances o CapitalConstante) y la periodicidad (Mensual, Trimestral, Semestral o Anual).");
            }

            if (numeroCuotas is < 1 or > CuotasMaximas)
            {
                return Fallo("financiacion.cuotas", $"El número de cuotas va de 1 a {CuotasMaximas}.");
            }

            if (cuotasCarencia < 0 || cuotasCarencia >= numeroCuotas)
            {
                return Fallo("financiacion.carencia", "Las cuotas de carencia son menos que el número de cuotas.");
            }

            var primera = fechaPrimeraCuota ?? fechaFormalizacion.AddMonths((int)periodicidad);
            if (primera <= fechaFormalizacion)
            {
                return Fallo("financiacion.primera_cuota", "La primera cuota es posterior a la formalización.");
            }

            if (valorResidual < 0m || valorResidual >= capital || decimal.Round(valorResidual, 2) != valorResidual
                || (valorResidual > 0m && tipo != TipoFinanciacion.Leasing))
            {
                return Fallo("financiacion.valor_residual", "El valor residual (opción de compra) es del leasing y menor que el capital.");
            }

            if (porcentajeIva is < 0m or > 30m || (porcentajeIva > 0m && tipo != TipoFinanciacion.Leasing))
            {
                return Fallo("financiacion.iva", "El IVA de las cuotas es del leasing (0 a 30 %).");
            }

            op.Sistema = sistema;
            op.Periodicidad = periodicidad;
            op.NumeroCuotas = numeroCuotas;
            op.CuotasCarencia = cuotasCarencia;
            op.FechaPrimeraCuota = primera;
            op.ValorResidual = valorResidual;
            op.PorcentajeIva = porcentajeIva;
            var leasing = tipo == TipoFinanciacion.Leasing;
            op.CuentaLargoPlazo = Cuenta(cuentaLargoPlazo, leasing ? "174" : "170");
            op.CuentaCortoPlazo = Cuenta(cuentaCortoPlazo, leasing ? "524" : "520")!;
            if (leasing)
            {
                op.CuentaActivo = string.IsNullOrWhiteSpace(cuentaActivo) ? null : cuentaActivo.Trim();
                if (op.CuentaActivo is null || op.CuentaActivo[0] != '2')
                {
                    return Fallo("financiacion.cuenta_activo", "El leasing necesita la cuenta del bien (grupo 2, p. ej. 213 maquinaria o 218 elementos de transporte).");
                }
            }
        }

        op.CuentaIntereses = Cuenta(cuentaIntereses, "6623")!;
        op.CuentaTesoreria = Cuenta(cuentaTesoreria, "572")!;
        op.CuentaComisiones = Cuenta(cuentaComisiones, "626")!;
        foreach (var (c, grupos, texto) in new[]
        {
            (op.CuentaLargoPlazo, "1", "de largo plazo (grupo 1)"), (op.CuentaCortoPlazo, "5", "de corto plazo (grupo 5)"),
            (op.CuentaIntereses, "6", "de intereses (grupo 6)"), (op.CuentaTesoreria, "5", "de tesorería (grupo 5)"),
            (op.CuentaComisiones, "6", "de comisiones (grupo 6)"),
        })
        {
            if (c is not null && (c.Length is < 3 or > 12 || !c.All(char.IsAsciiDigit) || !c.StartsWith(grupos, StringComparison.Ordinal)))
            {
                return Fallo("financiacion.cuenta", $"La cuenta {texto} no es válida: {c}.");
            }
        }

        return Resultado.Ok(op);
    }

    // ------------------------------------------------------------------ cuadro y saldos

    public IReadOnlyList<RevisionCuadro> Revisiones => _eventos.Where(e => e.Tipo == TipoEventoFinanciacion.RevisionTipo)
        .OrderBy(e => e.Orden).Select(e => new RevisionCuadro(e.Numero!.Value, e.TipoInteres!.Value)).ToList();

    public IReadOnlyList<AnticipoCuadro> Anticipos => _eventos.Where(e => e.Tipo == TipoEventoFinanciacion.AmortizacionAnticipada)
        .OrderBy(e => e.Orden).Select(e => new AnticipoCuadro(e.Numero!.Value, e.Importe)).ToList();

    /// <summary>Cuadro vigente (con las revisiones y anticipos registrados). Vacío en una póliza.</summary>
    public IReadOnlyList<CuotaCuadro> Cuadro() => Cuadro(Revisiones, Anticipos);

    internal IReadOnlyList<CuotaCuadro> Cuadro(IReadOnlyList<RevisionCuadro> revisiones, IReadOnlyList<AnticipoCuadro> anticipos) =>
        EsPoliza ? [] : CalculadoraPrestamo.Calcular(Capital, TipoInteres, Periodicidad, NumeroCuotas, CuotasCarencia, FechaPrimeraCuota!.Value,
            Sistema, ValorResidual, revisiones, anticipos);

    public EventoFinanciacion? EventoCuota(int numero) => _eventos.FirstOrDefault(e => e.Tipo == TipoEventoFinanciacion.Cuota && e.Numero == numero);

    /// <summary>Cuota pagada antes de llevar la operación aquí (sin asiento).</summary>
    public bool EsPrevia(CuotaCuadro c) => ContabilizarDesde is { } d && c.Fecha < d;

    public bool Pagada(CuotaCuadro c) => EsPrevia(c) || EventoCuota(c.Numero) is not null;

    /// <summary>Último ejercicio cuyas cuotas tienen el capital en corto plazo.</summary>
    public int HorizonteCortoPlazo
    {
        get
        {
            var inicial = Math.Max(FechaFormalizacion.Year, ContabilizarDesde?.Year ?? 0);
            var reclasificado = _eventos.Where(e => e.Tipo == TipoEventoFinanciacion.Reclasificacion).Select(e => e.Ejercicio!.Value + 1).DefaultIfEmpty(0).Max();
            return Math.Max(inicial, reclasificado);
        }
    }

    /// <summary>Capital pendiente en corto y largo plazo según un cuadro.</summary>
    internal (decimal Corto, decimal Largo) Reparto(IReadOnlyList<CuotaCuadro> cuadro, int horizonte)
    {
        decimal corto = 0m, largo = 0m;
        foreach (var c in cuadro.Where(c => !Pagada(c)))
        {
            if (c.Fecha.Year <= horizonte)
            {
                corto += c.Capital;
            }
            else
            {
                largo += c.Capital;
            }
        }

        return (Redondeo.Dos(corto), Redondeo.Dos(largo));
    }

    public (decimal Corto, decimal Largo) Reparto() => EsPoliza ? (Dispuesto(), 0m) : Reparto(Cuadro(), HorizonteCortoPlazo);

    public decimal CapitalPendiente() => EsPoliza ? Dispuesto() : Redondeo.Dos(Cuadro().Where(c => !Pagada(c)).Sum(c => c.Capital));

    /// <summary>Póliza: lo dispuesto (a una fecha, incluida).</summary>
    public decimal Dispuesto(DateOnly? aFecha = null) => Redondeo.Dos(_eventos
        .Where(e => (e.Tipo is TipoEventoFinanciacion.Disposicion or TipoEventoFinanciacion.Reintegro) && (aFecha is null || e.Fecha <= aFecha))
        .Sum(e => e.Tipo == TipoEventoFinanciacion.Disposicion ? e.Importe : -e.Importe));

    /// <summary>Póliza: saldo dispuesto por tramos (cambia el día de cada movimiento).</summary>
    public IReadOnlyList<TramoPoliza> Tramos()
    {
        var tramos = new List<TramoPoliza> { new(FechaFormalizacion, 0m) };
        var saldo = 0m;
        foreach (var g in _eventos.Where(e => e.Tipo is TipoEventoFinanciacion.Disposicion or TipoEventoFinanciacion.Reintegro)
                     .OrderBy(e => e.Fecha).ThenBy(e => e.Orden).GroupBy(e => e.Fecha))
        {
            saldo = Redondeo.Dos(saldo + g.Sum(e => e.Tipo == TipoEventoFinanciacion.Disposicion ? e.Importe : -e.Importe));
            tramos.Add(new TramoPoliza(g.Key, saldo));
        }

        return tramos;
    }

    /// <summary>Póliza: el día siguiente a la última liquidación (o la formalización).</summary>
    public DateOnly InicioSiguienteLiquidacion => _eventos.Where(e => e.Tipo == TipoEventoFinanciacion.LiquidacionIntereses)
        .Select(e => (DateOnly?)e.Fecha.AddDays(1)).Max() ?? FechaFormalizacion;

    public EventoFinanciacion? UltimoEvento => _eventos.OrderBy(e => e.Orden).LastOrDefault();

    public bool TieneAsientos => _eventos.Any(e => e.AsientoId is not null);

    // ------------------------------------------------------------------ hechos

    internal EventoFinanciacion Anotar(TipoEventoFinanciacion tipo, DateOnly fecha, Guid? asientoId)
    {
        var e = new EventoFinanciacion(_eventos.Select(x => x.Orden).DefaultIfEmpty(0).Max() + 1, tipo, fecha) { AsientoId = asientoId };
        _eventos.Add(e);
        return e;
    }

    public void AnotarFormalizacion(Guid? asientoId) => Anotar(TipoEventoFinanciacion.Formalizacion, FechaFormalizacion, asientoId).Importe = Capital;

    public void AnotarCuota(CuotaCuadro c, decimal iva, Guid? asientoId)
    {
        var e = Anotar(TipoEventoFinanciacion.Cuota, c.Fecha, asientoId);
        e.Numero = c.Numero;
        e.Importe = c.Capital;
        e.Intereses = c.Intereses;
        e.Iva = iva;
        ActualizarEstado();
    }

    public void AnotarRevision(DateOnly fecha, int desdeCuota, decimal tipoInteres, Guid? asientoId)
    {
        var e = Anotar(TipoEventoFinanciacion.RevisionTipo, fecha, asientoId);
        e.Numero = desdeCuota;
        e.TipoInteres = tipoInteres;
    }

    public void AnotarAnticipo(DateOnly fecha, int trasCuota, decimal importe, decimal comision, Guid asientoId)
    {
        var e = Anotar(TipoEventoFinanciacion.AmortizacionAnticipada, fecha, asientoId);
        e.Numero = trasCuota;
        e.Importe = importe;
        e.Comision = comision;
        ActualizarEstado();
    }

    public void AnotarReclasificacion(int ejercicio, decimal importe, Guid? asientoId)
    {
        var e = Anotar(TipoEventoFinanciacion.Reclasificacion, new DateOnly(ejercicio, 12, 31), asientoId);
        e.Ejercicio = ejercicio;
        e.Importe = importe;
    }

    public void AnotarMovimientoPoliza(bool disposicion, DateOnly fecha, decimal importe, Guid asientoId) =>
        Anotar(disposicion ? TipoEventoFinanciacion.Disposicion : TipoEventoFinanciacion.Reintegro, fecha, asientoId).Importe = importe;

    public void AnotarLiquidacion(DateOnly desde, DateOnly hasta, decimal intereses, decimal comision, Guid? asientoId)
    {
        var e = Anotar(TipoEventoFinanciacion.LiquidacionIntereses, hasta, asientoId);
        e.Desde = desde;
        e.Intereses = intereses;
        e.Comision = comision;
    }

    /// <summary>Quita el último hecho (su asiento ya se ha anulado).</summary>
    public Resultado QuitarUltimo(Guid eventoId)
    {
        var ultimo = UltimoEvento;
        if (ultimo is null || ultimo.Id != eventoId)
        {
            return Resultado.Fallo(Error.Conflicto("financiacion.no_es_el_ultimo", "Solo se deshace el último hecho de la operación."));
        }

        if (ultimo.Tipo == TipoEventoFinanciacion.Formalizacion)
        {
            return Resultado.Fallo(Error.Conflicto("financiacion.formalizacion", "La formalización no se deshace: elimina la operación."));
        }

        _eventos.Remove(ultimo);
        ActualizarEstado();
        return Resultado.Ok();
    }

    public Resultado Renombrar(string? descripcion, string? entidad)
    {
        var desc = (descripcion ?? string.Empty).Trim();
        if (desc.Length is 0 or > LongitudDescripcion)
        {
            return Resultado.Fallo(Error.Validacion("financiacion.descripcion", $"La descripción es obligatoria (hasta {LongitudDescripcion} caracteres)."));
        }

        var ent = string.IsNullOrWhiteSpace(entidad) ? null : entidad.Trim();
        if (ent is { Length: > LongitudEntidad })
        {
            return Resultado.Fallo(Error.Validacion("financiacion.entidad", "El nombre de la entidad es demasiado largo."));
        }

        Descripcion = desc;
        Entidad = ent;
        return Resultado.Ok();
    }

    /// <summary>Cierra la póliza (sin nada dispuesto) o la vuelve a abrir.</summary>
    public Resultado CerrarPoliza(bool cerrar)
    {
        if (!EsPoliza)
        {
            return Resultado.Fallo(Error.Conflicto("financiacion.no_es_poliza", "Solo se cierra a mano una póliza; el préstamo se cancela al pagarlo."));
        }

        if (cerrar && Dispuesto() != 0m)
        {
            return Resultado.Fallo(Error.Conflicto("financiacion.dispuesto", "Antes de cerrar la póliza, reintegra lo dispuesto."));
        }

        Estado = cerrar ? EstadoFinanciacion.Cancelada : EstadoFinanciacion.Vigente;
        return Resultado.Ok();
    }

    private void ActualizarEstado()
    {
        if (!EsPoliza)
        {
            Estado = CapitalPendiente() <= 0m ? EstadoFinanciacion.Cancelada : EstadoFinanciacion.Vigente;
        }
    }

    private static Resultado<OperacionFinanciacion> Fallo(string codigo, string mensaje) =>
        Resultado.Fallo<OperacionFinanciacion>(Error.Validacion(codigo, mensaje));
}
