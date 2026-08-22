using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  Cuenta de Pérdidas y Ganancias (analítica por grupos 6 y 7)
// --------------------------------------------------------------------------------------------

/// <summary>Fila de un informe contable agregado: cuenta (o epígrafe) e importe.</summary>
public sealed record LineaInformeDto(string Codigo, string Nombre, decimal Importe);

/// <summary>Cuenta de resultados: gastos (grupo 6), ingresos (grupo 7) y resultado del ejercicio.</summary>
public sealed record PerdidasGananciasDto(
    int Ejercicio, IReadOnlyList<LineaInformeDto> Ingresos, IReadOnlyList<LineaInformeDto> Gastos,
    decimal TotalIngresos, decimal TotalGastos, decimal Resultado);

/// <summary>Calcula la cuenta de Pérdidas y Ganancias del ejercicio a partir del libro diario.</summary>
public sealed class GenerarPerdidasGanancias
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;

    public GenerarPerdidasGanancias(IRepositorioAsientos asientos, IRepositorioCuentas cuentas)
    {
        _asientos = asientos;
        _cuentas = cuentas;
    }

    public async Task<PerdidasGananciasDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var saldos = await SaldosContables.CalcularAsync(empresaId, ejercicio, _asientos, _cuentas, ct).ConfigureAwait(false);

        // Ingresos (grupo 7): saldo acreedor (haber − debe). Gastos (grupo 6): saldo deudor (debe − haber).
        var ingresos = saldos.Where(s => s.Grupo == 7).Select(s => new LineaInformeDto(s.Codigo, s.Nombre, Redondeo.Dos(s.Haber - s.Debe)))
            .Where(l => l.Importe != 0m).OrderBy(l => l.Codigo, StringComparer.Ordinal).ToList();
        var gastos = saldos.Where(s => s.Grupo == 6).Select(s => new LineaInformeDto(s.Codigo, s.Nombre, Redondeo.Dos(s.Debe - s.Haber)))
            .Where(l => l.Importe != 0m).OrderBy(l => l.Codigo, StringComparer.Ordinal).ToList();

        var totalIngresos = Redondeo.Dos(ingresos.Sum(l => l.Importe));
        var totalGastos = Redondeo.Dos(gastos.Sum(l => l.Importe));
        return new PerdidasGananciasDto(ejercicio, ingresos, gastos, totalIngresos, totalGastos, Redondeo.Dos(totalIngresos - totalGastos));
    }
}

// --------------------------------------------------------------------------------------------
//  Balance de situación (clasificado por masas patrimoniales)
// --------------------------------------------------------------------------------------------

/// <summary>Balance de situación por masas: activo frente a patrimonio neto + pasivo.</summary>
public sealed record BalanceSituacionDto(
    int Ejercicio,
    IReadOnlyList<LineaInformeDto> ActivoNoCorriente, IReadOnlyList<LineaInformeDto> ActivoCorriente,
    IReadOnlyList<LineaInformeDto> PatrimonioNeto, IReadOnlyList<LineaInformeDto> Pasivo,
    decimal TotalActivo, decimal TotalPatrimonioNetoYPasivo, bool Cuadra);

/// <summary>Calcula el balance de situación clasificado por masas patrimoniales del PGC.</summary>
public sealed class GenerarBalanceSituacion
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;

    public GenerarBalanceSituacion(IRepositorioAsientos asientos, IRepositorioCuentas cuentas)
    {
        _asientos = asientos;
        _cuentas = cuentas;
    }

    public async Task<BalanceSituacionDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var saldos = await SaldosContables.CalcularAsync(empresaId, ejercicio, _asientos, _cuentas, ct).ConfigureAwait(false);

        var activoNoCorriente = new List<LineaInformeDto>();
        var activoCorriente = new List<LineaInformeDto>();
        var patrimonioNeto = new List<LineaInformeDto>();
        var pasivo = new List<LineaInformeDto>();

        foreach (var s in saldos)
        {
            var neto = Redondeo.Dos(s.Debe - s.Haber); // saldo deudor positivo, acreedor negativo
            if (neto == 0m || s.Grupo is 6 or 7)
            {
                continue; // las cuentas de gestión no van al balance
            }

            var masa = ClasificarMasa(s.Codigo, s.Grupo, neto);
            var esActivo = masa is Masa.ActivoNoCorriente or Masa.ActivoCorriente;

            // Importe con signo: el activo se muestra por su saldo deudor (neto) —así una cuenta
            // correctora como la amortización acumulada (28x, acreedora) minora el activo— y el
            // patrimonio neto y el pasivo por su saldo acreedor (−neto). Con este convenio el balance
            // cuadra por construcción (Σ activo = Σ pn+pasivo + resultado).
            var importe = esActivo ? neto : -neto;
            var linea = new LineaInformeDto(s.Codigo, s.Nombre, importe);
            switch (masa)
            {
                case Masa.ActivoNoCorriente: activoNoCorriente.Add(linea); break;
                case Masa.ActivoCorriente: activoCorriente.Add(linea); break;
                case Masa.PatrimonioNeto: patrimonioNeto.Add(linea); break;
                case Masa.Pasivo: pasivo.Add(linea); break;
            }
        }

        // Resultado del ejercicio aún no regularizado: se muestra como línea de patrimonio neto para
        // que el balance cuadre también antes del cierre (como hace cualquier software contable).
        if (!saldos.Any(s => s.Codigo == PlanBasico.CuentaResultado && Redondeo.Dos(s.Debe - s.Haber) != 0m))
        {
            var ingresos = Redondeo.Dos(saldos.Where(s => s.Grupo == 7).Sum(s => s.Haber - s.Debe));
            var gastos = Redondeo.Dos(saldos.Where(s => s.Grupo == 6).Sum(s => s.Debe - s.Haber));
            var resultado = Redondeo.Dos(ingresos - gastos);
            if (resultado != 0m)
            {
                patrimonioNeto.Add(new LineaInformeDto(PlanBasico.CuentaResultado, "Resultado del ejercicio (provisional)", resultado));
            }
        }

        var totalActivo = Redondeo.Dos(activoNoCorriente.Concat(activoCorriente).Sum(l => l.Importe));
        var totalPnPasivo = Redondeo.Dos(patrimonioNeto.Concat(pasivo).Sum(l => l.Importe));
        return new BalanceSituacionDto(
            ejercicio,
            Ordenar(activoNoCorriente), Ordenar(activoCorriente), Ordenar(patrimonioNeto), Ordenar(pasivo),
            totalActivo, totalPnPasivo, totalActivo == totalPnPasivo);
    }

    private static List<LineaInformeDto> Ordenar(List<LineaInformeDto> l) => l.OrderBy(x => x.Codigo, StringComparer.Ordinal).ToList();

    private enum Masa { ActivoNoCorriente, ActivoCorriente, PatrimonioNeto, Pasivo }

    /// <summary>
    /// Clasifica una cuenta en su masa patrimonial. Simplificación por grupo/subgrupo del PGC: es una
    /// clasificación analítica, no el modelo oficial con todos los epígrafes.
    /// </summary>
    private static Masa ClasificarMasa(string codigo, int grupo, decimal netoDeudor)
    {
        switch (grupo)
        {
            case 1: // Financiación básica: patrimonio neto (10-13) y pasivo no corriente (14-17).
                return codigo.StartsWith("129", StringComparison.Ordinal) || EmpiezaPorAlguno(codigo, "10", "11", "12", "13")
                    ? Masa.PatrimonioNeto : Masa.Pasivo;
            case 2: // Inmovilizado.
                return Masa.ActivoNoCorriente;
            case 3: // Existencias.
                return Masa.ActivoCorriente;
            case 5: // Cuentas financieras (tesorería y similares).
                return netoDeudor >= 0m ? Masa.ActivoCorriente : Masa.Pasivo;
            case 4: // Acreedores y deudores: por el signo del saldo.
            default:
                return netoDeudor >= 0m ? Masa.ActivoCorriente : Masa.Pasivo;
        }
    }

    private static bool EmpiezaPorAlguno(string codigo, params string[] prefijos)
    {
        foreach (var p in prefijos)
        {
            if (codigo.StartsWith(p, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

// --------------------------------------------------------------------------------------------
//  Saldos contables (agregación reutilizable)
// --------------------------------------------------------------------------------------------

/// <summary>Saldo acumulado de una cuenta en un ejercicio.</summary>
public sealed record SaldoContable(string Codigo, string Nombre, int Grupo, decimal Debe, decimal Haber);

/// <summary>Calcula el saldo (debe/haber acumulado) de cada cuenta con movimiento en el ejercicio.</summary>
internal static class SaldosContables
{
    public static async Task<IReadOnlyList<SaldoContable>> CalcularAsync(
        Guid empresaId, int ejercicio, IRepositorioAsientos asientos, IRepositorioCuentas cuentas, CancellationToken ct)
    {
        // La suma por cuenta la hace la base de datos (GROUP BY): no se cargan los apuntes en memoria.
        var agregados = await asientos.SaldosAgregadosAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var plan = await cuentas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var nombres = plan.ToDictionary(c => c.Codigo, c => c.Nombre, StringComparer.Ordinal);

        return agregados.Select(s => new SaldoContable(
            s.CuentaCodigo, nombres.GetValueOrDefault(s.CuentaCodigo, "—"),
            s.CuentaCodigo.Length > 0 && char.IsDigit(s.CuentaCodigo[0]) ? s.CuentaCodigo[0] - '0' : 0,
            Redondeo.Dos(s.Debe), Redondeo.Dos(s.Haber))).ToList();
    }
}

// --------------------------------------------------------------------------------------------
//  Cierre de ejercicio (regularización → cierre → apertura del siguiente)
// --------------------------------------------------------------------------------------------

/// <summary>Resultado del cierre de ejercicio: los asientos generados y el resultado.</summary>
public sealed record CierreEjercicioDto(int Ejercicio, decimal Resultado, Guid RegularizacionId, Guid CierreId, Guid AperturaId);

/// <summary>
/// Cierra un ejercicio contable: (1) regulariza los grupos 6 y 7 contra la cuenta 129 (resultado),
/// (2) genera el asiento de cierre que salda todas las cuentas patrimoniales y (3) reabre esos saldos
/// en el ejercicio siguiente (asiento de apertura a 1 de enero). No se puede cerrar dos veces.
/// </summary>
public sealed class CerrarEjercicio
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public CerrarEjercicio(IRepositorioAsientos asientos, IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _asientos = asientos;
        _cuentas = cuentas;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<CierreEjercicioDto>> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        if (await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<CierreEjercicioDto>(Error.Conflicto("cierre.ya_cerrado", $"El ejercicio {ejercicio} ya está cerrado."));
        }

        var saldos = await SaldosContables.CalcularAsync(empresaId, ejercicio, _asientos, _cuentas, ct).ConfigureAwait(false);
        if (saldos.Count == 0)
        {
            return Resultado.Fallo<CierreEjercicioDto>(Error.Validacion("cierre.sin_movimientos", $"El ejercicio {ejercicio} no tiene movimientos que cerrar."));
        }

        var finAnio = new DateOnly(ejercicio, 12, 31);

        // 1) Regularización: saldar 6 (gasto, deudor) y 7 (ingreso, acreedor) contra 129.
        var lineasReg = new List<LineaAsiento>();
        decimal ingresos = 0m, gastos = 0m;
        foreach (var s in saldos.Where(s => s.Grupo is 6 or 7))
        {
            var neto = Redondeo.Dos(s.Debe - s.Haber);
            if (neto == 0m)
            {
                continue;
            }

            if (s.Grupo == 7)
            {
                // Ingreso: saldo acreedor → se salda cargándolo (Debe) por su saldo acreedor.
                var acreedor = Redondeo.Dos(-neto);
                lineasReg.Add(new LineaAsiento(s.Codigo, acreedor, 0m, "Regularización"));
                ingresos += acreedor;
            }
            else
            {
                // Gasto: saldo deudor → se salda abonándolo (Haber) por su saldo deudor.
                lineasReg.Add(new LineaAsiento(s.Codigo, 0m, neto, "Regularización"));
                gastos += neto;
            }
        }

        var resultado = Redondeo.Dos(ingresos - gastos);
        if (resultado >= 0m)
        {
            lineasReg.Add(new LineaAsiento(PlanBasico.CuentaResultado, 0m, resultado, "Beneficio del ejercicio"));
        }
        else
        {
            lineasReg.Add(new LineaAsiento(PlanBasico.CuentaResultado, Redondeo.Dos(-resultado), 0m, "Pérdida del ejercicio"));
        }

        var numReg = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var regularizacion = Asiento.Crear(empresaId, ejercicio, numReg, finAnio, "Regularización de existencias, gastos e ingresos", "Regularizacion", lineasReg, _reloj);
        if (regularizacion.EsFallo)
        {
            return Resultado.Fallo<CierreEjercicioDto>(regularizacion.Error);
        }

        _asientos.Agregar(regularizacion.Valor);

        // 2) Cierre: saldar todas las cuentas patrimoniales (grupos 1-5) con su saldo, incluida la 129.
        var lineasCierre = new List<LineaAsiento>();
        var saldosPatrimonio = new List<(string Codigo, decimal Neto)>();
        foreach (var s in saldos.Where(s => s.Grupo is not (6 or 7)))
        {
            var neto = Redondeo.Dos(s.Debe - s.Haber);
            if (neto != 0m)
            {
                saldosPatrimonio.Add((s.Codigo, neto));
            }
        }

        // La 129 recoge el resultado (acreedor si beneficio); añádela si no estaba con saldo.
        if (!saldosPatrimonio.Any(x => x.Codigo == PlanBasico.CuentaResultado) && resultado != 0m)
        {
            saldosPatrimonio.Add((PlanBasico.CuentaResultado, Redondeo.Dos(-resultado)));
        }

        foreach (var (codigo, neto) in saldosPatrimonio)
        {
            // Saldo deudor (neto>0) se abona (Haber); saldo acreedor (neto<0) se carga (Debe).
            if (neto > 0m)
            {
                lineasCierre.Add(new LineaAsiento(codigo, 0m, neto, "Cierre"));
            }
            else
            {
                lineasCierre.Add(new LineaAsiento(codigo, Redondeo.Dos(-neto), 0m, "Cierre"));
            }
        }

        // SiguienteNumeroAsync ya tiene en cuenta la regularización recién añadida (aunque no esté guardada),
        // así que devuelve el número correcto sin necesidad de sumar uno a mano.
        var numCierre = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var cierre = Asiento.Crear(empresaId, ejercicio, numCierre, finAnio, "Cierre del ejercicio", "Cierre", lineasCierre, _reloj);
        if (cierre.EsFallo)
        {
            return Resultado.Fallo<CierreEjercicioDto>(cierre.Error);
        }

        _asientos.Agregar(cierre.Valor);

        // 3) Apertura del ejercicio siguiente (asiento inverso del cierre a 1 de enero).
        var lineasApertura = saldosPatrimonio.Select(x => x.Neto > 0m
            ? new LineaAsiento(x.Codigo, x.Neto, 0m, "Apertura")
            : new LineaAsiento(x.Codigo, 0m, Redondeo.Dos(-x.Neto), "Apertura")).ToList();

        var siguiente = ejercicio + 1;
        var numApertura = await _asientos.SiguienteNumeroAsync(empresaId, siguiente, ct).ConfigureAwait(false);
        var apertura = Asiento.Crear(empresaId, siguiente, numApertura, new DateOnly(siguiente, 1, 1), "Apertura del ejercicio", "Apertura", lineasApertura, _reloj);
        if (apertura.EsFallo)
        {
            return Resultado.Fallo<CierreEjercicioDto>(apertura.Error);
        }

        _asientos.Agregar(apertura.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);

        return Resultado.Ok(new CierreEjercicioDto(ejercicio, resultado, regularizacion.Valor.Id, cierre.Valor.Id, apertura.Valor.Id));
    }
}
