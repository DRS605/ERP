using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Método de cálculo de la cuota de amortización.</summary>
public enum MetodoAmortizacion
{
    /// <summary>Cuota constante = base amortizable / vida útil.</summary>
    Lineal,

    /// <summary>Porcentaje constante sobre el valor pendiente (amortización decreciente).</summary>
    Degresivo,

    /// <summary>Suma de dígitos (decreciente): año k reparte base × (n−k+1) / Σdígitos.</summary>
    Digitos,
}

/// <summary>Con qué frecuencia se genera la dotación (y su asiento) de un inmovilizado.</summary>
public enum PeriodicidadAmortizacion
{
    /// <summary>Un asiento por mes (dotación mensual prorrateada).</summary>
    Mensual,

    /// <summary>Un único asiento anual (al 31/12).</summary>
    Anual,
}

/// <summary>Situación del inmovilizado en su ciclo de vida.</summary>
public enum EstadoInmovilizado
{
    Activo,
    DadoDeBaja,
    Enajenado,
}

/// <summary>
/// Parámetros de un plan de amortización (método, vida útil y —solo para el degresivo— el
/// porcentaje anual sobre el saldo pendiente). Se usa por separado para el plan contable y el fiscal.
/// </summary>
public sealed record PlanAmortizacion(MetodoAmortizacion Metodo, int VidaUtilAnios, decimal PorcentajeDegresivo = 0m);

/// <summary>
/// Calcula la amortización: cuotas anuales por método y su reparto mensual desde la puesta en
/// funcionamiento. Es una función pura (sin estado ni dependencias), fácil de probar.
/// </summary>
public static class CalculadoraAmortizacion
{
    /// <summary>
    /// Cuotas anuales (una por año de vida útil) que suman exactamente la base amortizable. El último
    /// año absorbe el redondeo para que Σcuotas = base.
    /// </summary>
    public static IReadOnlyList<decimal> CuotasAnuales(decimal baseAmortizable, PlanAmortizacion plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var n = plan.VidaUtilAnios;
        var cuotas = new decimal[Math.Max(n, 0)];
        if (n <= 0 || baseAmortizable <= 0m)
        {
            return cuotas;
        }

        switch (plan.Metodo)
        {
            case MetodoAmortizacion.Lineal:
                var anual = Redondeo.Dos(baseAmortizable / n);
                for (var i = 0; i < n; i++)
                {
                    cuotas[i] = anual;
                }

                break;

            case MetodoAmortizacion.Digitos:
                var sumaDigitos = n * (n + 1) / 2m;
                for (var i = 0; i < n; i++)
                {
                    cuotas[i] = Redondeo.Dos(baseAmortizable * (n - i) / sumaDigitos);
                }

                break;

            case MetodoAmortizacion.Degresivo:
                // Porcentaje anual sobre el saldo pendiente; si no se indica, doble del lineal (2/n).
                var tasa = plan.PorcentajeDegresivo > 0m ? plan.PorcentajeDegresivo / 100m : 2m / n;
                var pendiente = baseAmortizable;
                for (var i = 0; i < n - 1; i++)
                {
                    var cuota = Redondeo.Dos(pendiente * tasa);
                    if (cuota > pendiente)
                    {
                        cuota = pendiente;
                    }

                    cuotas[i] = cuota;
                    pendiente = Redondeo.Dos(pendiente - cuota);
                }

                cuotas[n - 1] = pendiente; // el último año amortiza el resto pendiente
                break;
        }

        // Ajuste final: garantizar Σcuotas == base amortizable (corrige céntimos de redondeo).
        var resto = Redondeo.Dos(baseAmortizable - cuotas.Sum());
        if (resto != 0m)
        {
            cuotas[n - 1] = Redondeo.Dos(cuotas[n - 1] + resto);
        }

        return cuotas;
    }

    /// <summary>Una cuota mensual del plan: a qué año/mes natural corresponde y su importe.</summary>
    public readonly record struct CuotaMensual(int Ejercicio, int Mes, decimal Importe);

    /// <summary>
    /// Reparte las cuotas anuales mes a mes desde la puesta en funcionamiento (12 meses por año de
    /// vida). Cada mes recibe cuotaAnual/12; el último mes de cada año de vida absorbe el redondeo.
    /// El resultado ya está agrupado por año/mes natural (útil para prorratear el primer y último año).
    /// </summary>
    public static IReadOnlyList<CuotaMensual> PlanMensual(DateOnly fechaAlta, IReadOnlyList<decimal> cuotasAnuales)
    {
        ArgumentNullException.ThrowIfNull(cuotasAnuales);
        var meses = new List<CuotaMensual>();
        for (var anio = 0; anio < cuotasAnuales.Count; anio++)
        {
            var cuota = cuotasAnuales[anio];
            var mensual = Redondeo.Dos(cuota / 12m);
            var acumulado = 0m;
            for (var m = 0; m < 12; m++)
            {
                var fecha = fechaAlta.AddYears(anio).AddMonths(m);
                var importe = m < 11 ? mensual : Redondeo.Dos(cuota - acumulado);
                acumulado = Redondeo.Dos(acumulado + importe);
                if (importe != 0m)
                {
                    meses.Add(new CuotaMensual(fecha.Year, fecha.Month, importe));
                }
            }
        }

        return meses;
    }

    /// <summary>Importe amortizable de un año natural concreto (suma de sus cuotas mensuales).</summary>
    public static decimal ImporteDeEjercicio(DateOnly fechaAlta, IReadOnlyList<decimal> cuotasAnuales, int ejercicio) =>
        Redondeo.Dos(PlanMensual(fechaAlta, cuotasAnuales).Where(c => c.Ejercicio == ejercicio).Sum(c => c.Importe));

    /// <summary>Una pata de asiento de impuesto diferido (cuenta + debe/haber).</summary>
    public readonly record struct PataImpuestoDiferido(string CuentaCodigo, decimal Debe, decimal Haber);

    /// <summary>
    /// Calcula las patas del asiento de impuesto diferido al pasar de una diferencia temporaria
    /// acumulada <paramref name="acumuladaAntes"/> (fiscal − contable) a
    /// <paramref name="acumuladaAntes"/> + <paramref name="diferenciaEjercicio"/>. Rutea el efecto a la
    /// cuenta 479 (pasivo por diferencia imponible) mientras la acumulada es positiva y a la 4740
    /// (activo por diferencia deducible) mientras es negativa, partiendo el asiento si cruza el cero.
    /// La contrapartida es la 6301 (impuesto diferido). Devuelve lista vacía si no hay efecto.
    /// </summary>
    public static IReadOnlyList<PataImpuestoDiferido> AsientoImpuestoDiferido(
        decimal acumuladaAntes, decimal diferenciaEjercicio, decimal tipoImpositivo,
        string cuentaPasivo, string cuentaActivo, string cuentaImpuesto)
    {
        var despues = acumuladaAntes + diferenciaEjercicio;

        // Saldos objetivo (en importe de impuesto) de cada cuenta según el signo de la acumulada.
        var pasivoAntes = Redondeo.Dos(Math.Max(acumuladaAntes, 0m) * tipoImpositivo);
        var pasivoDespues = Redondeo.Dos(Math.Max(despues, 0m) * tipoImpositivo);
        var activoAntes = Redondeo.Dos(Math.Max(-acumuladaAntes, 0m) * tipoImpositivo);
        var activoDespues = Redondeo.Dos(Math.Max(-despues, 0m) * tipoImpositivo);

        var deltaPasivo = Redondeo.Dos(pasivoDespues - pasivoAntes); // 479 es de saldo acreedor
        var deltaActivo = Redondeo.Dos(activoDespues - activoAntes); // 4740 es de saldo deudor

        var patas = new List<PataImpuestoDiferido>();
        if (deltaPasivo > 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaPasivo, 0m, deltaPasivo));
        }
        else if (deltaPasivo < 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaPasivo, -deltaPasivo, 0m));
        }

        if (deltaActivo > 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaActivo, deltaActivo, 0m));
        }
        else if (deltaActivo < 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaActivo, 0m, -deltaActivo));
        }

        if (patas.Count == 0)
        {
            return patas;
        }

        // Contrapartida en 6301 para que el asiento cuadre.
        var debe = Redondeo.Dos(patas.Sum(p => p.Debe));
        var haber = Redondeo.Dos(patas.Sum(p => p.Haber));
        var balance = Redondeo.Dos(debe - haber);
        if (balance > 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaImpuesto, 0m, balance));
        }
        else if (balance < 0m)
        {
            patas.Add(new PataImpuestoDiferido(cuentaImpuesto, -balance, 0m));
        }

        return patas;
    }
}

/// <summary>Una dotación de amortización contabilizada de un inmovilizado (idempotencia por año/mes).</summary>
public sealed class DotacionAmortizacion
{
    private DotacionAmortizacion()
    {
    }

    internal DotacionAmortizacion(Guid id, int ejercicio, int mes, DateOnly fecha, decimal importe, Guid asientoId)
    {
        Id = id;
        Ejercicio = ejercicio;
        Mes = mes;
        Fecha = fecha;
        Importe = importe;
        AsientoId = asientoId;
    }

    public Guid Id { get; private set; }

    public int Ejercicio { get; private set; }

    /// <summary>Mes natural (1–12) para periodicidad mensual; 0 para la dotación anual.</summary>
    public int Mes { get; private set; }

    public DateOnly Fecha { get; private set; }

    public decimal Importe { get; private set; }

    public Guid AsientoId { get; private set; }
}

/// <summary>Ajuste por impuesto diferido de un ejercicio (diferencia contable-fiscal). Idempotente por año.</summary>
public sealed class AjusteFiscalAmortizacion
{
    private AjusteFiscalAmortizacion()
    {
    }

    internal AjusteFiscalAmortizacion(Guid id, int ejercicio, decimal diferenciaTemporaria, decimal tipoImpositivo, Guid asientoId)
    {
        Id = id;
        Ejercicio = ejercicio;
        DiferenciaTemporaria = diferenciaTemporaria;
        TipoImpositivo = tipoImpositivo;
        AsientoId = asientoId;
    }

    public Guid Id { get; private set; }

    public int Ejercicio { get; private set; }

    /// <summary>Diferencia temporaria del ejercicio: amortización fiscal − contable.</summary>
    public decimal DiferenciaTemporaria { get; private set; }

    public decimal TipoImpositivo { get; private set; }

    public Guid AsientoId { get; private set; }
}

/// <summary>
/// Elemento de inmovilizado (activo fijo) con amortización contable y fiscal independientes. Lleva su
/// cuadro de dotaciones contabilizadas y los ajustes por impuesto diferido. Se da de baja o se enajena.
/// </summary>
public sealed class Inmovilizado : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaCodigo = 40;
    public const int LongitudMaximaDescripcion = 200;

    private readonly List<DotacionAmortizacion> _dotaciones = new();
    private readonly List<AjusteFiscalAmortizacion> _ajustesFiscales = new();

    private Inmovilizado(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Descripcion = null!;
        CuentaActivo = null!;
        CuentaAmortizacion = null!;
        CuentaDotacion = null!;
        Contable = null!;
        Fiscal = null!;
    }

    private Inmovilizado(Guid id, Guid empresaId, string codigo, string descripcion, string cuentaActivo,
        string cuentaAmortizacion, string cuentaDotacion, DateOnly fechaAdquisicion, DateOnly fechaAlta,
        decimal valorAdquisicion, decimal valorResidual, PeriodicidadAmortizacion periodicidad,
        PlanAmortizacion contable, PlanAmortizacion fiscal)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Descripcion = descripcion;
        CuentaActivo = cuentaActivo;
        CuentaAmortizacion = cuentaAmortizacion;
        CuentaDotacion = cuentaDotacion;
        FechaAdquisicion = fechaAdquisicion;
        FechaAlta = fechaAlta;
        ValorAdquisicion = valorAdquisicion;
        ValorResidual = valorResidual;
        Periodicidad = periodicidad;
        Contable = contable;
        Fiscal = fiscal;
        Estado = EstadoInmovilizado.Activo;
    }

    public string Codigo { get; private set; }

    public string Descripcion { get; private set; }

    /// <summary>Cuenta del activo (grupo 2, p. ej. 213 Maquinaria).</summary>
    public string CuentaActivo { get; private set; }

    /// <summary>Cuenta de amortización acumulada (28x, saldo acreedor: minora el activo).</summary>
    public string CuentaAmortizacion { get; private set; }

    /// <summary>Cuenta de dotación (gasto 68x).</summary>
    public string CuentaDotacion { get; private set; }

    public DateOnly FechaAdquisicion { get; private set; }

    /// <summary>Puesta en funcionamiento: fecha de inicio de la amortización.</summary>
    public DateOnly FechaAlta { get; private set; }

    public decimal ValorAdquisicion { get; private set; }

    public decimal ValorResidual { get; private set; }

    public PeriodicidadAmortizacion Periodicidad { get; private set; }

    public PlanAmortizacion Contable { get; private set; }

    public PlanAmortizacion Fiscal { get; private set; }

    public EstadoInmovilizado Estado { get; private set; }

    public DateOnly? FechaBaja { get; private set; }

    public decimal? ValorEnajenacion { get; private set; }

    public IReadOnlyList<DotacionAmortizacion> Dotaciones => _dotaciones;

    public IReadOnlyList<AjusteFiscalAmortizacion> AjustesFiscales => _ajustesFiscales;

    /// <summary>Base amortizable = valor de adquisición − valor residual.</summary>
    public decimal BaseAmortizable => Redondeo.Dos(ValorAdquisicion - ValorResidual);

    public decimal AmortizacionAcumulada => Redondeo.Dos(_dotaciones.Sum(d => d.Importe));

    public decimal ValorNetoContable => Redondeo.Dos(ValorAdquisicion - AmortizacionAcumulada);

    public decimal DiferenciaTemporariaAcumulada => Redondeo.Dos(_ajustesFiscales.Sum(a => a.DiferenciaTemporaria));

    public static Resultado<Inmovilizado> Crear(Guid empresaId, string? codigo, string? descripcion,
        string? cuentaActivo, string? cuentaAmortizacion, string? cuentaDotacion,
        DateOnly fechaAdquisicion, DateOnly fechaAlta, decimal valorAdquisicion, decimal valorResidual,
        PeriodicidadAmortizacion periodicidad, PlanAmortizacion contable, PlanAmortizacion fiscal)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.codigo_vacio", "El código del inmovilizado es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.descripcion_vacia", "La descripción es obligatoria."));
        }

        if (string.IsNullOrWhiteSpace(cuentaActivo) || string.IsNullOrWhiteSpace(cuentaAmortizacion) || string.IsNullOrWhiteSpace(cuentaDotacion))
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.cuentas", "Las cuentas de activo, amortización acumulada y dotación son obligatorias."));
        }

        if (valorAdquisicion <= 0m)
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.valor", "El valor de adquisición debe ser positivo."));
        }

        if (valorResidual < 0m || valorResidual >= valorAdquisicion)
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.residual", "El valor residual debe estar entre 0 y el valor de adquisición."));
        }

        if (fechaAlta < fechaAdquisicion)
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.fecha_alta", "La puesta en funcionamiento no puede ser anterior a la adquisición."));
        }

        ArgumentNullException.ThrowIfNull(contable);
        ArgumentNullException.ThrowIfNull(fiscal);
        if (contable.VidaUtilAnios <= 0 || fiscal.VidaUtilAnios <= 0)
        {
            return Resultado.Fallo<Inmovilizado>(Error.Validacion("inmovilizado.vida_util", "La vida útil (contable y fiscal) debe ser de al menos un año."));
        }

        return Resultado.Ok(new Inmovilizado(Guid.NewGuid(), empresaId, codigo.Trim(), descripcion.Trim(),
            cuentaActivo.Trim(), cuentaAmortizacion.Trim(), cuentaDotacion.Trim(), fechaAdquisicion, fechaAlta,
            Redondeo.Dos(valorAdquisicion), Redondeo.Dos(valorResidual), periodicidad, contable, fiscal));
    }

    /// <summary>Cuotas anuales del plan contable.</summary>
    public IReadOnlyList<decimal> CuotasAnualesContable() => CalculadoraAmortizacion.CuotasAnuales(BaseAmortizable, Contable);

    /// <summary>Cuotas anuales del plan fiscal.</summary>
    public IReadOnlyList<decimal> CuotasAnualesFiscal() => CalculadoraAmortizacion.CuotasAnuales(BaseAmortizable, Fiscal);

    public bool DotacionExiste(int ejercicio, int mes) => _dotaciones.Any(d => d.Ejercicio == ejercicio && d.Mes == mes);

    public bool AjusteFiscalExiste(int ejercicio) => _ajustesFiscales.Any(a => a.Ejercicio == ejercicio);

    public void RegistrarDotacion(int ejercicio, int mes, DateOnly fecha, decimal importe, Guid asientoId) =>
        _dotaciones.Add(new DotacionAmortizacion(Guid.NewGuid(), ejercicio, mes, fecha, Redondeo.Dos(importe), asientoId));

    public void RegistrarAjusteFiscal(int ejercicio, decimal diferenciaTemporaria, decimal tipoImpositivo, Guid asientoId) =>
        _ajustesFiscales.Add(new AjusteFiscalAmortizacion(Guid.NewGuid(), ejercicio, Redondeo.Dos(diferenciaTemporaria), tipoImpositivo, asientoId));

    public Resultado DarDeBaja(DateOnly fecha)
    {
        if (Estado != EstadoInmovilizado.Activo)
        {
            return Resultado.Fallo(Error.Conflicto("inmovilizado.no_activo", "El inmovilizado ya está dado de baja o enajenado."));
        }

        Estado = EstadoInmovilizado.DadoDeBaja;
        FechaBaja = fecha;
        return Resultado.Ok();
    }

    public Resultado Enajenar(DateOnly fecha, decimal valorEnajenacion)
    {
        if (Estado != EstadoInmovilizado.Activo)
        {
            return Resultado.Fallo(Error.Conflicto("inmovilizado.no_activo", "El inmovilizado ya está dado de baja o enajenado."));
        }

        if (valorEnajenacion < 0m)
        {
            return Resultado.Fallo(Error.Validacion("inmovilizado.enajenacion_negativa", "El importe de la enajenación no puede ser negativo."));
        }

        Estado = EstadoInmovilizado.Enajenado;
        FechaBaja = fecha;
        ValorEnajenacion = Redondeo.Dos(valorEnajenacion);
        return Resultado.Ok();
    }
}
