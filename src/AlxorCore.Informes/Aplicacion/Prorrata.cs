using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>Cuotas soportadas de un periodo, repartidas por la afectación de cada gasto.</summary>
public sealed record CuotasSoportadas(decimal Base, decimal Total, decimal Comun, decimal ConDerecho, decimal SinDerecho)
{
    public static CuotasSoportadas Cero { get; } = new(0m, 0m, 0m, 0m, 0m);

    /// <summary>Suma los gastos registrados (no anulados) del impuesto indicado.</summary>
    public static CuotasSoportadas De(IEnumerable<GastoDto> gastos, TipoImpuesto impuesto)
    {
        // Por tipo de cada factura: base de las líneas con cuota y cuota deducible (la parte que la línea deja deducir,
        // antes de la prorrata; incluye la autoliquidada por inversión del sujeto pasivo o intracomunitaria).
        var lista = gastos.Where(g => Soportado.Cuenta(g, impuesto)).ToList();
        decimal Cuota(AfectacionIva a) => Redondeo.Dos(lista.Where(g => g.Afectacion == a).Sum(g => g.DesgloseIva.Sum(d => d.CuotaDeducible)));
        return new CuotasSoportadas(
            Redondeo.Dos(lista.Sum(g => g.DesgloseIva.Where(d => d.Cuota != 0m).Sum(d => d.Base))), Redondeo.Dos(lista.Sum(g => g.DesgloseIva.Sum(d => d.CuotaDeducible))),
            Cuota(AfectacionIva.Comun), Cuota(AfectacionIva.ConDerecho), Cuota(AfectacionIva.SinDerecho));
    }

    public static CuotasSoportadas operator +(CuotasSoportadas a, CuotasSoportadas b)
    {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return new(a.Base + b.Base, a.Total + b.Total, a.Comun + b.Comun, a.ConDerecho + b.ConDerecho, a.SinDerecho + b.SinDerecho);
    }
}

/// <summary>Qué gastos cuentan como impuesto soportado de un impuesto.</summary>
internal static class Soportado
{
    public static bool Cuenta(GastoDto g, TipoImpuesto impuesto) =>
        !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase) && Impuesto.TipoDeCodigo(g.CodigoIva) == impuesto;
}

/// <summary>Reglas de cálculo de la prorrata (arts. 104 y 106 Ley 37/1992; igual en el IGIC).</summary>
public static class ReglaProrrata
{
    /// <summary>
    /// Cuota deducible: sin prorrata, toda; con la general, el porcentaje de toda; con la especial, entera
    /// la de uso exclusivo en operaciones con derecho, nada la de uso en exentas y el porcentaje de la común.
    /// </summary>
    public static decimal Deducible(RegimenProrrata? regimen, int porcentaje, CuotasSoportadas cuotas)
    {
        ArgumentNullException.ThrowIfNull(cuotas);
        return regimen switch
        {
            null => cuotas.Total,
            RegimenProrrata.General => Redondeo.Dos(cuotas.Total * porcentaje / 100m),
            _ => Redondeo.Dos(cuotas.ConDerecho + (cuotas.Comun * porcentaje / 100m)),
        };
    }

    /// <summary>
    /// Porcentaje definitivo: operaciones con derecho a deducir entre el total (con y sin derecho),
    /// redondeado a la unidad <b>superior</b> (art. 104.Dos). Sin operaciones, 100.
    /// </summary>
    public static int Porcentaje(decimal baseConDerecho, decimal baseSinDerecho)
    {
        var total = baseConDerecho + baseSinDerecho;
        if (total <= 0m)
        {
            return 100;
        }

        var p = (int)Math.Ceiling(Math.Max(0m, baseConDerecho) * 100m / total);
        return Math.Clamp(p, 0, 100);
    }

    /// <summary>Derecho a deducción que da una venta según la clase de su tipo de impuesto.</summary>
    public static DerechoDeduccion Derecho(ClaseIva clase) => clase switch
    {
        ClaseIva.Exento or ClaseIva.OroInversion => DerechoDeduccion.SinDerecho,
        ClaseIva.NoSujeto => DerechoDeduccion.Excluida,
        _ => DerechoDeduccion.ConDerecho,
    };
}

/// <summary>Derecho a deducir que generan las ventas (para el porcentaje de prorrata).</summary>
public enum DerechoDeduccion
{
    /// <summary>Sujetas y no exentas, exportaciones, entregas intracomunitarias, ISP…: cuentan arriba y abajo.</summary>
    ConDerecho = 1,

    /// <summary>Exentas sin derecho (art. 20 LIVA): solo cuentan en el denominador.</summary>
    SinDerecho = 2,

    /// <summary>No sujetas: no cuentan.</summary>
    Excluida = 3,
}

/// <summary>Resultado del cálculo de la prorrata de un ejercicio.</summary>
public sealed record ProrrataCalculadaDto(
    int Ejercicio,
    TipoImpuesto Impuesto,
    RegimenProrrata? Regimen,
    int PorcentajeProvisional,
    int PorcentajeDefinitivo,
    decimal BaseConDerecho,
    decimal BaseSinDerecho,
    decimal BaseExcluida,
    CuotasSoportadas Soportado,
    decimal DeducibleProvisional,
    decimal DeducibleDefinitivo,
    decimal Regularizacion,
    decimal DeducibleGeneral,
    decimal DeducibleEspecial,
    bool EspecialObligatoria,
    string? Aviso);

/// <summary>
/// Caso de uso: calcula la prorrata de un ejercicio (porcentaje definitivo, deducción con el provisional
/// y con el definitivo, regularización) y la aplica a las autoliquidaciones trimestrales.
/// </summary>
public sealed class CalcularProrrata
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProrrata _prorratas;
    private readonly IResolverIvaEmpresa _tipos;

    public CalcularProrrata(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaProrrata prorratas, IResolverIvaEmpresa tipos)
    {
        _facturas = facturas;
        _gastos = gastos;
        _prorratas = prorratas;
        _tipos = tipos;
    }

    public async Task<ProrrataCalculadaDto> EjecutarAsync(Guid empresaId, int ejercicio, TipoImpuesto impuesto, CancellationToken ct = default)
    {
        var config = await _prorratas.ObtenerAsync(empresaId, ejercicio, impuesto, ct).ConfigureAwait(false);
        var (conDerecho, sinDerecho, excluida) = await BasesVentasAsync(empresaId, ejercicio, impuesto, ct).ConfigureAwait(false);
        var definitivo = ReglaProrrata.Porcentaje(conDerecho, sinDerecho);

        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var anual = CuotasSoportadas.De(gastos.Where(g => g.Fecha.Year == ejercicio), impuesto);

        var regimen = config?.Regimen;
        var provisional = config?.PorcentajeProvisional ?? 100;
        decimal deducidoProvisional = 0m;
        for (var t = 1; t <= 4; t++)
        {
            var (desde, hasta) = Periodos.Trimestre(ejercicio, t);
            deducidoProvisional += ReglaProrrata.Deducible(regimen, provisional,
                CuotasSoportadas.De(gastos.Where(g => g.Fecha >= desde && g.Fecha <= hasta), impuesto));
        }

        var deducibleDefinitivo = ReglaProrrata.Deducible(regimen, definitivo, anual);
        var general = ReglaProrrata.Deducible(RegimenProrrata.General, definitivo, anual);
        var especial = ReglaProrrata.Deducible(RegimenProrrata.Especial, definitivo, anual);

        // Art. 103.Dos.2º: la especial es obligatoria si la general deduce un 10 % más o más que ella.
        var especialObligatoria = regimen == RegimenProrrata.General && especial > 0m && general >= especial * 1.10m;
        string? aviso = null;
        if (regimen is null && sinDerecho > 0m)
        {
            aviso = $"La empresa tiene ventas exentas sin derecho a deducción ({Redondeo.Formatear(sinDerecho)} €): debe aplicar prorrata. Con las ventas del año, el porcentaje sería {definitivo} %.";
        }
        else if (especialObligatoria)
        {
            aviso = $"Con la prorrata general se deducen {Redondeo.Formatear(general)} €, un 10 % o más que con la especial ({Redondeo.Formatear(especial)} €): la prorrata especial es obligatoria (art. 103.Dos.2º Ley 37/1992).";
        }

        return new ProrrataCalculadaDto(
            ejercicio, impuesto, regimen, provisional, definitivo, conDerecho, sinDerecho, excluida, anual,
            Redondeo.Dos(deducidoProvisional), deducibleDefinitivo,
            regimen is null ? 0m : Redondeo.Dos(deducibleDefinitivo - deducidoProvisional),
            general, especial, especialObligatoria, aviso);
    }

    /// <summary>
    /// Deducción de un trimestre: la cuota soportada con el porcentaje provisional y, en el cuarto
    /// trimestre, la regularización del año con el porcentaje definitivo.
    /// </summary>
    public async Task<DeduccionTrimestre> DeduccionAsync(
        Guid empresaId, int anio, int trimestre, TipoImpuesto impuesto, CuotasSoportadas cuotasTrimestre, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cuotasTrimestre);
        var config = await _prorratas.ObtenerAsync(empresaId, anio, impuesto, ct).ConfigureAwait(false);
        if (config is null)
        {
            return new DeduccionTrimestre(null, 100, cuotasTrimestre.Total, 0m);
        }

        var deducible = ReglaProrrata.Deducible(config.Regimen, config.PorcentajeProvisional, cuotasTrimestre);
        var regularizacion = 0m;
        if (trimestre == 4)
        {
            regularizacion = (await EjecutarAsync(empresaId, anio, impuesto, ct).ConfigureAwait(false)).Regularizacion;
        }

        return new DeduccionTrimestre(config.Regimen, config.PorcentajeProvisional, deducible, regularizacion);
    }

    private async Task<(decimal ConDerecho, decimal SinDerecho, decimal Excluida)> BasesVentasAsync(
        Guid empresaId, int ejercicio, TipoImpuesto impuesto, CancellationToken ct)
    {
        var desglose = await _facturas.DesgloseImpuestoAsync(empresaId, new DateOnly(ejercicio, 1, 1), new DateOnly(ejercicio, 12, 31), ct)
            .ConfigureAwait(false);

        decimal con = 0m, sin = 0m, excluida = 0m;
        foreach (var d in desglose.Where(d => d.Impuesto == impuesto))
        {
            var clase = (await _tipos.ResolverAsync(empresaId, d.CodigoIva, ct).ConfigureAwait(false))?.Clase
                ?? (string.Equals(d.CodigoIva, Impuesto.IvaExento.Codigo, StringComparison.OrdinalIgnoreCase) ? ClaseIva.Exento : ClaseIva.Ordinario);
            switch (ReglaProrrata.Derecho(clase))
            {
                case DerechoDeduccion.ConDerecho: con += d.Base; break;
                case DerechoDeduccion.SinDerecho: sin += d.Base; break;
                default: excluida += d.Base; break;
            }
        }

        return (Redondeo.Dos(con), Redondeo.Dos(sin), Redondeo.Dos(excluida));
    }
}

/// <summary>Deducción del impuesto soportado en un trimestre, con la prorrata aplicada.</summary>
public sealed record DeduccionTrimestre(RegimenProrrata? Regimen, int Porcentaje, decimal Deducible, decimal Regularizacion);

/// <summary>Periodos de las autoliquidaciones.</summary>
public static class Periodos
{
    public static (DateOnly Desde, DateOnly Hasta) Trimestre(int anio, int trimestre)
    {
        if (trimestre is < 1 or > 4)
        {
            throw new ArgumentOutOfRangeException(nameof(trimestre), trimestre, "El trimestre debe estar entre 1 y 4.");
        }

        var desde = new DateOnly(anio, ((trimestre - 1) * 3) + 1, 1);
        return (desde, desde.AddMonths(3).AddDays(-1));
    }
}
