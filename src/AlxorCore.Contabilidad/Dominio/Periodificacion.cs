using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Qué se periodifica: un gasto (6xx, contra 480) o un ingreso (7xx, contra 485).</summary>
public enum TipoPeriodificacion
{
    Gasto = 1,
    Ingreso = 2,
}

public enum EstadoPeriodificacion
{
    /// <summary>Se van generando sus cuotas.</summary>
    Activa = 1,

    /// <summary>Todas sus cuotas están contabilizadas.</summary>
    Terminada = 2,

    /// <summary>Se terminó antes: lo que faltaba se llevó a resultados de una vez.</summary>
    Cancelada = 3,

    /// <summary>Se anularon todos sus asientos.</summary>
    Anulada = 4,
}

/// <summary>Cuota mensual de una periodificación: su importe y, cuando se contabiliza, su asiento.</summary>
public sealed class CuotaPeriodificacion
{
    private CuotaPeriodificacion() { }

    internal CuotaPeriodificacion(Guid id, int ejercicio, int mes, decimal importe)
    {
        Id = id;
        Ejercicio = ejercicio;
        Mes = mes;
        Importe = importe;
    }

    public Guid Id { get; private set; }

    public int Ejercicio { get; private set; }

    public int Mes { get; private set; }

    public decimal Importe { get; private set; }

    public Guid? AsientoId { get; private set; }

    /// <summary>Último día del mes: la fecha de su asiento.</summary>
    public DateOnly Fecha => new DateOnly(Ejercicio, Mes, 1).AddMonths(1).AddDays(-1);

    internal void Contabilizar(Guid asientoId) => AsientoId = asientoId;
}

/// <summary>
/// Periodificación (el <c>AsientosPeriodificacion</c> de Hispatec): un gasto o un ingreso que corresponde a varios meses
/// (un seguro anual, una suscripción, un alquiler cobrado por adelantado) se lleva a resultados mes a mes.
/// <list type="bullet">
/// <item>Al darla de alta, opcionalmente, un asiento de reclasificación saca el importe de la cuenta de resultados y lo
/// deja en la de periodificación (480 gastos anticipados, 485 ingresos anticipados).</item>
/// <item>Cada mes, su cuota pasa de la cuenta de periodificación a la de resultados (asiento al último día del mes).</item>
/// </list>
/// La última cuota recoge el redondeo, de modo que la suma de las cuotas es el importe.
/// </summary>
public sealed class Periodificacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudDescripcion = 200;
    public const int MesesMaximos = 120;
    public const string CuentaGastosAnticipados = "480";
    public const string CuentaIngresosAnticipados = "485";

    private readonly List<CuotaPeriodificacion> _cuotas = [];

    private Periodificacion(Guid id) : base(id, Guid.Empty) { Descripcion = null!; CuentaResultado = null!; CuentaPeriodificacion = null!; }

    private Periodificacion(Guid id, Guid empresaId) : base(id, empresaId) { Descripcion = null!; CuentaResultado = null!; CuentaPeriodificacion = null!; }

    public string Descripcion { get; private set; }

    public TipoPeriodificacion Tipo { get; private set; }

    /// <summary>Cuenta de gasto (6xx) o de ingreso (7xx) a la que va cada cuota.</summary>
    public string CuentaResultado { get; private set; }

    /// <summary>Cuenta de balance donde espera lo no imputado (480 / 485).</summary>
    public string CuentaPeriodificacion { get; private set; }

    public decimal Importe { get; private set; }

    /// <summary>Fecha del gasto o ingreso (la del asiento de reclasificación).</summary>
    public DateOnly Fecha { get; private set; }

    public int EjercicioInicio { get; private set; }

    public int MesInicio { get; private set; }

    public int Meses { get; private set; }

    public EstadoPeriodificacion Estado { get; private set; }

    /// <summary>Asiento de reclasificación del alta (null si el importe ya estaba en la cuenta de periodificación).</summary>
    public Guid? AsientoReclasificacionId { get; private set; }

    /// <summary>Asiento con que se canceló (lo pendiente, a resultados de una vez).</summary>
    public Guid? AsientoCancelacionId { get; private set; }

    public IReadOnlyList<CuotaPeriodificacion> Cuotas => _cuotas;

    public decimal Imputado => Redondeo.Dos(_cuotas.Where(c => c.AsientoId is not null).Sum(c => c.Importe));

    public decimal Pendiente => Redondeo.Dos(Importe - Imputado);

    public bool TieneAsientos => AsientoReclasificacionId is not null || AsientoCancelacionId is not null || _cuotas.Any(c => c.AsientoId is not null);

    public static Resultado<Periodificacion> Crear(Guid empresaId, string? descripcion, TipoPeriodificacion tipo, string? cuentaResultado,
        string? cuentaPeriodificacion, decimal importe, DateOnly fecha, int ejercicioInicio, int mesInicio, int meses)
    {
        var d = (descripcion ?? string.Empty).Trim();
        if (d.Length is 0 or > LongitudDescripcion)
        {
            return Fallo("periodificacion.descripcion", $"La descripción es obligatoria (hasta {LongitudDescripcion} caracteres).");
        }

        if (!Enum.IsDefined(tipo))
        {
            return Fallo("periodificacion.tipo", "El tipo es Gasto o Ingreso.");
        }

        var resultado = (cuentaResultado ?? string.Empty).Trim();
        var grupo = tipo == TipoPeriodificacion.Gasto ? '6' : '7';
        if (resultado.Length is < 3 or > 12 || !resultado.All(char.IsAsciiDigit) || resultado[0] != grupo)
        {
            return Fallo("periodificacion.cuenta_resultado", $"La cuenta de {(tipo == TipoPeriodificacion.Gasto ? "gasto" : "ingreso")} es del grupo {grupo} (3 a 12 dígitos).");
        }

        var periodo = string.IsNullOrWhiteSpace(cuentaPeriodificacion)
            ? (tipo == TipoPeriodificacion.Gasto ? CuentaGastosAnticipados : CuentaIngresosAnticipados)
            : cuentaPeriodificacion.Trim();
        if (periodo.Length is < 3 or > 12 || !periodo.All(char.IsAsciiDigit) || periodo[0] is '6' or '7')
        {
            return Fallo("periodificacion.cuenta_periodificacion", "La cuenta de periodificación es de balance (3 a 12 dígitos, no de los grupos 6 ni 7).");
        }

        if (importe <= 0m || decimal.Round(importe, 2) != importe)
        {
            return Fallo("periodificacion.importe", "El importe es positivo, con 2 decimales como mucho.");
        }

        if (meses is < 2 or > MesesMaximos)
        {
            return Fallo("periodificacion.meses", $"Se reparte en 2 a {MesesMaximos} meses.");
        }

        if (mesInicio is < 1 or > 12 || ejercicioInicio is < 2000 or > 2100)
        {
            return Fallo("periodificacion.inicio", "Indica el mes en que empieza.");
        }

        var p = new Periodificacion(Guid.NewGuid(), empresaId)
        {
            Descripcion = d, Tipo = tipo, CuentaResultado = resultado, CuentaPeriodificacion = periodo, Importe = importe, Fecha = fecha,
            EjercicioInicio = ejercicioInicio, MesInicio = mesInicio, Meses = meses, Estado = EstadoPeriodificacion.Activa,
        };
        var cuota = Redondeo.Dos(importe / meses);
        var inicio = new DateOnly(ejercicioInicio, mesInicio, 1);
        for (var i = 0; i < meses; i++)
        {
            var mes = inicio.AddMonths(i);
            var importeCuota = i == meses - 1 ? Redondeo.Dos(importe - (cuota * (meses - 1))) : cuota;
            p._cuotas.Add(new CuotaPeriodificacion(Guid.NewGuid(), mes.Year, mes.Month, importeCuota));
        }

        return Resultado.Ok(p);
    }

    public void MarcarReclasificada(Guid asientoId) => AsientoReclasificacionId = asientoId;

    /// <summary>Anota el asiento de una cuota; con la última, la periodificación termina.</summary>
    public Resultado ContabilizarCuota(Guid cuotaId, Guid asientoId)
    {
        var c = _cuotas.FirstOrDefault(x => x.Id == cuotaId);
        if (Estado != EstadoPeriodificacion.Activa || c is null || c.AsientoId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("periodificacion.cuota", "Esa cuota no está pendiente."));
        }

        c.Contabilizar(asientoId);
        if (_cuotas.All(x => x.AsientoId is not null))
        {
            Estado = EstadoPeriodificacion.Terminada;
        }

        return Resultado.Ok();
    }

    /// <summary>Termina antes: lo pendiente va a resultados con el asiento indicado.</summary>
    public Resultado Cancelar(Guid asientoId)
    {
        if (Estado != EstadoPeriodificacion.Activa)
        {
            return Resultado.Fallo(Error.Conflicto("periodificacion.estado", "Solo se cancela una periodificación activa."));
        }

        AsientoCancelacionId = asientoId;
        Estado = EstadoPeriodificacion.Cancelada;
        return Resultado.Ok();
    }

    public Resultado Anular()
    {
        if (Estado == EstadoPeriodificacion.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("periodificacion.anulada", "La periodificación ya está anulada."));
        }

        Estado = EstadoPeriodificacion.Anulada;
        return Resultado.Ok();
    }

    /// <summary>Asientos generados por la periodificación (reclasificación, cuotas y cancelación).</summary>
    public IEnumerable<Guid> Asientos() =>
        new[] { AsientoReclasificacionId }.Concat(_cuotas.Select(c => c.AsientoId)).Append(AsientoCancelacionId).Where(a => a is not null).Select(a => a!.Value);

    private static Resultado<Periodificacion> Fallo(string codigo, string mensaje) => Resultado.Fallo<Periodificacion>(Error.Validacion(codigo, mensaje));
}
