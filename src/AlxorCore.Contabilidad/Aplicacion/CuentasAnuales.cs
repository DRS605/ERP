using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  DTOs
// --------------------------------------------------------------------------------------------

/// <summary>Epígrafe (renglón) de un estado financiero normalizado.</summary>
public sealed record EpigrafeDto(string Concepto, decimal Importe);

/// <summary>Balance de situación normalizado (PGC-Pymes, abreviado) por masas y epígrafes.</summary>
public sealed record BalanceNormalizadoDto(
    IReadOnlyList<EpigrafeDto> ActivoNoCorriente, IReadOnlyList<EpigrafeDto> ActivoCorriente, decimal TotalActivo,
    IReadOnlyList<EpigrafeDto> PatrimonioNeto, IReadOnlyList<EpigrafeDto> PasivoNoCorriente, IReadOnlyList<EpigrafeDto> PasivoCorriente,
    decimal TotalPatrimonioNetoYPasivo, bool Cuadra);

/// <summary>Cuenta de Pérdidas y Ganancias normalizada (PGC-Pymes) con subtotales.</summary>
public sealed record PyGNormalizadaDto(
    IReadOnlyList<EpigrafeDto> Explotacion, decimal ResultadoExplotacion,
    IReadOnlyList<EpigrafeDto> Financiero, decimal ResultadoFinanciero,
    decimal ResultadoAntesImpuestos, decimal ImpuestoSociedades, decimal ResultadoEjercicio);

/// <summary>Cuentas anuales del ejercicio: balance normalizado y PyG normalizada.</summary>
public sealed record CuentasAnualesDto(int Ejercicio, BalanceNormalizadoDto Balance, PyGNormalizadaDto PerdidasGanancias);

/// <summary>Liquidación del Impuesto sobre Sociedades (modelo 200), calculada desde la contabilidad.</summary>
public sealed record LiquidacionSociedadesDto(
    int Ejercicio, decimal ResultadoContableAntesImpuestos,
    decimal AjustesAumentos, decimal AjustesDisminuciones, decimal BaseImponible,
    decimal TipoGravamen, decimal CuotaIntegra, decimal Deducciones, decimal CuotaLiquida,
    decimal RetencionesYPagosACuenta, decimal CuotaDiferencial);

// --------------------------------------------------------------------------------------------
//  Cálculo
// --------------------------------------------------------------------------------------------

/// <summary>
/// Genera las Cuentas Anuales normalizadas (PGC-Pymes abreviado) y la liquidación del Impuesto de
/// Sociedades a partir de los saldos contables del ejercicio. Es una presentación de la misma
/// información contable; el mapeo de cuentas a epígrafes es una aproximación del modelo abreviado
/// (mejor esfuerzo, a validar con la gestoría), no el modelo oficial con todos los renglones.
/// </summary>
public sealed class GenerarCuentasAnuales
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;

    public GenerarCuentasAnuales(IRepositorioAsientos asientos, IRepositorioCuentas cuentas)
    {
        _asientos = asientos;
        _cuentas = cuentas;
    }

    public async Task<CuentasAnualesDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var saldos = await SaldosContables.CalcularAsync(empresaId, ejercicio, _asientos, _cuentas, ct).ConfigureAwait(false);
        return new CuentasAnualesDto(ejercicio, Balance(saldos), PyG(saldos));
    }

    public async Task<LiquidacionSociedadesDto> LiquidacionAsync(Guid empresaId, int ejercicio,
        decimal tipoGravamen, decimal ajustesAumentos, decimal ajustesDisminuciones, decimal deducciones, decimal? retencionesYPagos, CancellationToken ct = default)
    {
        var saldos = await SaldosContables.CalcularAsync(empresaId, ejercicio, _asientos, _cuentas, ct).ConfigureAwait(false);

        var ingresos = Acreedor(saldos, "7");
        var gastosSinImpuesto = Deudor(saldos, "6", excluyendo: "630");
        var resultadoAntes = Redondeo.Dos(ingresos - gastosSinImpuesto);

        var baseImponible = Redondeo.Dos(resultadoAntes + ajustesAumentos - ajustesDisminuciones);
        var cuotaIntegra = Redondeo.Dos(Math.Max(baseImponible, 0m) * tipoGravamen);
        var cuotaLiquida = Redondeo.Dos(Math.Max(cuotaIntegra - deducciones, 0m));
        // Retenciones y pagos a cuenta soportados: saldo deudor de la 473 si no se indica un valor.
        var retenciones = retencionesYPagos ?? Deudor(saldos, "473");
        var diferencial = Redondeo.Dos(cuotaLiquida - retenciones);

        return new LiquidacionSociedadesDto(ejercicio, resultadoAntes, Redondeo.Dos(ajustesAumentos), Redondeo.Dos(ajustesDisminuciones),
            baseImponible, tipoGravamen, cuotaIntegra, Redondeo.Dos(deducciones), cuotaLiquida, Redondeo.Dos(retenciones), diferencial);
    }

    // ---- Balance normalizado ----
    private static BalanceNormalizadoDto Balance(IReadOnlyList<SaldoContable> s)
    {
        // Activo (saldo deudor, positivo; las correctoras 28x/29x restan por su signo).
        var activoNoCorriente = new List<EpigrafeDto>
        {
            new("I. Inmovilizado intangible", NetoDeudor(s, incluir: new[] { "20" }, correctoras: new[] { "280", "290" })),
            new("II. Inmovilizado material", NetoDeudor(s, incluir: new[] { "21" }, correctoras: new[] { "281", "291" })),
            new("III. Inversiones inmobiliarias", NetoDeudor(s, incluir: new[] { "22" }, correctoras: new[] { "282", "292" })),
            new("IV. Inversiones financieras a largo plazo", NetoDeudor(s, incluir: new[] { "24", "25", "26" }, correctoras: Array.Empty<string>())),
        };
        var activoCorriente = new List<EpigrafeDto>
        {
            new("I. Existencias", NetoDeudor(s, incluir: new[] { "3" }, correctoras: new[] { "39" })),
            new("II. Deudores comerciales y otras cuentas a cobrar", SoloDeudor(s, "43", "44", "460", "470", "471", "472", "473", "474")),
            new("III. Inversiones financieras a corto plazo", NetoDeudor(s, incluir: new[] { "53", "54" }, correctoras: new[] { "59" })),
            new("IV. Efectivo y otros activos líquidos", SoloDeudor(s, "57")),
        };
        var totalActivo = TotalActivo(s);

        // Patrimonio neto y pasivo (saldo acreedor, positivo).
        var resultado = Redondeo.Dos(Acreedor(s, "7") - Deudor(s, "6"));
        var patrimonioNeto = new List<EpigrafeDto>
        {
            new("I. Capital", Acreedor(s, "100", "101", "102")),
            new("II. Reservas y prima de emisión", Acreedor(s, "110", "111", "112", "113", "114", "115", "119")),
            new("III. Resultados de ejercicios anteriores", Acreedor(s, "120", "121")),
            new("IV. Resultado del ejercicio", resultado),
        };
        var pasivoNoCorriente = new List<EpigrafeDto>
        {
            new("I. Deudas a largo plazo", Acreedor(s, "16", "17", "18")),
            new("II. Pasivos por impuesto diferido", Acreedor(s, "479")),
        };
        var pasivoCorriente = new List<EpigrafeDto>
        {
            new("I. Deudas a corto plazo", Acreedor(s, "50", "51", "52", "55")),
            new("II. Acreedores comerciales y otras cuentas a pagar", SoloAcreedor(s, "40", "41", "465", "475", "476", "477")),
        };
        var totalPnPasivo = Redondeo.Dos(TotalPasivoPn(s) + resultado);

        return new BalanceNormalizadoDto(
            Limpiar(activoNoCorriente), Limpiar(activoCorriente), totalActivo,
            Limpiar(patrimonioNeto), Limpiar(pasivoNoCorriente), Limpiar(pasivoCorriente),
            totalPnPasivo, totalActivo == totalPnPasivo);
    }

    // ---- PyG normalizada ----
    private static PyGNormalizadaDto PyG(IReadOnlyList<SaldoContable> s)
    {
        var cifraNegocios = Acreedor(s, "70");
        var aprovisionamientos = Deudor(s, "60", "61", "607");
        var otrosIngresos = Acreedor(s, "74", "75");
        var gastosPersonal = Deudor(s, "64");
        var otrosGastos = Deudor(s, "62", "631", "634", "636", "639", "65", "694", "695", "699");
        var amortizacion = Deudor(s, "68");
        var resultadoExplotacion = Redondeo.Dos(cifraNegocios + otrosIngresos - aprovisionamientos - gastosPersonal - otrosGastos - amortizacion);

        var explotacion = new List<EpigrafeDto>
        {
            new("1. Importe neto de la cifra de negocios", cifraNegocios),
            new("2. Aprovisionamientos", -aprovisionamientos),
            new("3. Otros ingresos de explotación", otrosIngresos),
            new("4. Gastos de personal", -gastosPersonal),
            new("5. Otros gastos de explotación", -otrosGastos),
            new("6. Amortización del inmovilizado", -amortizacion),
        };

        var ingresosFinancieros = Acreedor(s, "76");
        var gastosFinancieros = Deudor(s, "66");
        var resultadoFinanciero = Redondeo.Dos(ingresosFinancieros - gastosFinancieros);
        var financiero = new List<EpigrafeDto>
        {
            new("7. Ingresos financieros", ingresosFinancieros),
            new("8. Gastos financieros", -gastosFinancieros),
        };

        var antesImpuestos = Redondeo.Dos(resultadoExplotacion + resultadoFinanciero);
        var impuesto = Deudor(s, "630"); // impuesto sobre beneficios (corriente + diferido)
        var resultadoEjercicio = Redondeo.Dos(antesImpuestos - impuesto);

        return new PyGNormalizadaDto(explotacion, resultadoExplotacion, financiero, resultadoFinanciero, antesImpuestos, impuesto, resultadoEjercicio);
    }

    // ---- Helpers de agregación ----
    private static bool Empieza(string codigo, string prefijo) => codigo.StartsWith(prefijo, StringComparison.Ordinal);

    private static bool EmpiezaAlguno(string codigo, string[] prefijos) => prefijos.Any(p => Empieza(codigo, p));

    /// <summary>Suma de saldos acreedores (haber − debe) de las cuentas cuyo código empieza por alguno de los prefijos.</summary>
    private static decimal Acreedor(IReadOnlyList<SaldoContable> s, params string[] prefijos) =>
        Redondeo.Dos(s.Where(x => EmpiezaAlguno(x.Codigo, prefijos)).Sum(x => x.Haber - x.Debe));

    /// <summary>Suma de saldos deudores (debe − haber), opcionalmente excluyendo un prefijo.</summary>
    private static decimal Deudor(IReadOnlyList<SaldoContable> s, params string[] prefijos) => Deudor(s, prefijos, null);

    private static decimal Deudor(IReadOnlyList<SaldoContable> s, string[] prefijos, string? excluyendo) =>
        Redondeo.Dos(s.Where(x => EmpiezaAlguno(x.Codigo, prefijos) && (excluyendo is null || !Empieza(x.Codigo, excluyendo))).Sum(x => x.Debe - x.Haber));

    private static decimal Deudor(IReadOnlyList<SaldoContable> s, string prefijo, string excluyendo) =>
        Deudor(s, new[] { prefijo }, excluyendo);

    /// <summary>Neto deudor de las cuentas indicadas menos sus correctoras (28x/29x).</summary>
    private static decimal NetoDeudor(IReadOnlyList<SaldoContable> s, string[] incluir, string[] correctoras)
    {
        var bruto = s.Where(x => EmpiezaAlguno(x.Codigo, incluir) && !EmpiezaAlguno(x.Codigo, correctoras)).Sum(x => x.Debe - x.Haber);
        var correc = s.Where(x => EmpiezaAlguno(x.Codigo, correctoras)).Sum(x => x.Haber - x.Debe);
        return Redondeo.Dos(bruto - correc);
    }

    /// <summary>Suma solo de las cuentas con saldo deudor (>0) de los prefijos (deudores del grupo 4/5).</summary>
    private static decimal SoloDeudor(IReadOnlyList<SaldoContable> s, params string[] prefijos) =>
        Redondeo.Dos(s.Where(x => EmpiezaAlguno(x.Codigo, prefijos)).Select(x => x.Debe - x.Haber).Where(n => n > 0m).Sum());

    /// <summary>Suma solo de las cuentas con saldo acreedor (>0) de los prefijos (acreedores del grupo 4/5).</summary>
    private static decimal SoloAcreedor(IReadOnlyList<SaldoContable> s, params string[] prefijos) =>
        Redondeo.Dos(s.Where(x => EmpiezaAlguno(x.Codigo, prefijos)).Select(x => x.Haber - x.Debe).Where(n => n > 0m).Sum());

    /// <summary>Total activo: neto deudor de todas las cuentas de activo (grupos 2, 3 y deudores de 4 y 5).</summary>
    private static decimal TotalActivo(IReadOnlyList<SaldoContable> s) => Redondeo.Dos(s.Where(EsActivo).Sum(x => x.Debe - x.Haber));

    /// <summary>Total patrimonio neto y pasivo (sin el resultado del ejercicio, que se añade aparte).</summary>
    private static decimal TotalPasivoPn(IReadOnlyList<SaldoContable> s) => Redondeo.Dos(s.Where(x => !EsActivo(x) && x.Grupo is not (6 or 7)).Sum(x => x.Haber - x.Debe));

    private static bool EsActivo(SaldoContable x)
    {
        var neto = x.Debe - x.Haber;
        return x.Grupo switch
        {
            2 or 3 => true,          // inmovilizado y existencias
            4 or 5 => neto >= 0m,    // deudores (saldo deudor) → activo; acreedores → pasivo
            _ => false,               // grupo 1 → patrimonio neto / pasivo; 6 y 7 se excluyen fuera
        };
    }

    private static List<EpigrafeDto> Limpiar(List<EpigrafeDto> lineas) => lineas.Where(l => l.Importe != 0m).ToList();
}
