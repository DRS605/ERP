using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Partida del modelo de depósito: su clave, el texto, el nivel de sangría y los importes del ejercicio y el anterior.</summary>
public sealed record PartidaDepositoDto(string Clave, string Concepto, int Nivel, decimal Actual, decimal Anterior);

/// <summary>
/// Balance y cuenta de pérdidas y ganancias en el formato abreviado de los modelos de depósito de cuentas en el Registro
/// Mercantil (Orden JUS), con sus claves de partida y la comparación con el ejercicio anterior.
/// </summary>
public sealed record ModeloDepositoDto(int Ejercicio, IReadOnlyList<PartidaDepositoDto> Balance, IReadOnlyList<PartidaDepositoDto> PerdidasGanancias,
    decimal TotalActivo, decimal TotalPatrimonioNetoPasivo, bool Cuadra, IReadOnlyList<string> CuentasSinClasificar);

/// <summary>
/// Cuentas anuales abreviadas con las claves de los modelos oficiales de depósito (balance 10000-32000, pérdidas y
/// ganancias 40100-49500), del ejercicio y del anterior, a partir de los saldos sin regularización ni cierre. Cada cuenta
/// va a su partida por su grupo del PGC; las que no encajan se listan para revisarlas. El depósito se presenta con el
/// programa del Registro (D2): estas cifras son las que se copian en él.
/// </summary>
public sealed class GenerarModeloDeposito
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;

    public GenerarModeloDeposito(IRepositorioAsientos asientos, IRepositorioCuentas cuentas)
    {
        _asientos = asientos; _cuentas = cuentas;
    }

    // Partida → prefijos de cuenta. Las correctoras («-») restan solas por su saldo; con «d:»/«a:» la cuenta solo va a la
    // partida si su saldo es deudor/acreedor (tesorería o cuentas de relación que pueden cambiar de lado).
    private static readonly (string Clave, string Concepto, int Nivel, string[] Cuentas)[] Activo =
    [
        ("11100", "I. Inmovilizado intangible", 2, ["20", "-280", "-290"]),
        ("11200", "II. Inmovilizado material", 2, ["21", "23", "-281", "-291"]),
        ("11300", "III. Inversiones inmobiliarias", 2, ["22", "-282", "-292"]),
        ("11400", "IV. Inversiones en empresas del grupo y asociadas a largo plazo", 2, ["240", "241", "242", "-293"]),
        ("11500", "V. Inversiones financieras a largo plazo", 2, ["25", "26", "243", "244", "245", "246", "247", "248", "249", "-294", "-295", "-296", "-297", "-298"]),
        ("11600", "VI. Activos por impuesto diferido", 2, ["474"]),
        ("12100", "I. Activos no corrientes mantenidos para la venta", 2, ["580", "581", "582", "583", "584", "-599"]),
        ("12200", "II. Existencias", 2, ["30", "31", "32", "33", "34", "35", "36", "407", "-39"]),
        ("12300", "III. Deudores comerciales y otras cuentas a cobrar", 2, ["d:43", "d:44", "d:460", "d:470", "d:471", "d:472", "d:473", "d:5531", "d:5533", "-490", "-493"]),
        ("12400", "IV. Inversiones en empresas del grupo y asociadas a corto plazo", 2, ["530", "531", "532", "533", "534", "535", "-593", "-594"]),
        ("12500", "V. Inversiones financieras a corto plazo", 2, ["536", "537", "538", "539", "54", "d:551", "d:552", "d:5590", "565", "566", "-595", "-596", "-597", "-598"]),
        ("12600", "VI. Periodificaciones a corto plazo", 2, ["480", "567"]),
        ("12700", "VII. Efectivo y otros activos líquidos equivalentes", 2, ["d:57"]),
    ];

    private static readonly (string Clave, string Concepto, int Nivel, string[] Cuentas)[] Pasivo =
    [
        ("21100", "I. Capital", 3, ["100", "101", "102", "-103", "-104"]),
        ("21200", "II. Prima de emisión", 3, ["110"]),
        ("21300", "III. Reservas", 3, ["111", "112", "113", "114", "115", "119"]),
        ("21400", "IV. (Acciones y participaciones en patrimonio propias)", 3, ["-108", "-109"]),
        ("21500", "V. Resultados de ejercicios anteriores", 3, ["120", "-121", "129"]),
        ("21600", "VI. Otras aportaciones de socios", 3, ["118"]),
        ("21800", "VIII. (Dividendo a cuenta)", 3, ["-557"]),
        ("23000", "A-3) Subvenciones, donaciones y legados recibidos", 2, ["130", "131", "132"]),
        ("31100", "I. Provisiones a largo plazo", 2, ["14"]),
        ("31200", "II. Deudas a largo plazo", 2, ["17", "180", "185", "189"]),
        ("31300", "III. Deudas con empresas del grupo y asociadas a largo plazo", 2, ["160", "161", "162", "163"]),
        ("31400", "IV. Pasivos por impuesto diferido", 2, ["479"]),
        ("31500", "V. Periodificaciones a largo plazo", 2, ["181"]),
        ("32200", "II. Provisiones a corto plazo", 2, ["499", "529"]),
        ("32300", "III. Deudas a corto plazo", 2, ["50", "515", "516", "517", "518", "519", "52", "a:551", "a:552", "a:555", "a:5595", "560", "561", "a:57"]),
        ("32400", "IV. Deudas con empresas del grupo y asociadas a corto plazo", 2, ["510", "511", "512", "513", "514"]),
        ("32500", "V. Acreedores comerciales y otras cuentas a pagar", 2, ["40", "a:41", "438", "a:465", "a:466", "a:475", "a:476", "a:477", "a:43", "a:44", "a:460", "a:470", "a:471", "a:472", "a:473", "-406"]),
        ("32600", "VI. Periodificaciones a corto plazo", 2, ["485", "568"]),
    ];

    // Pérdidas y ganancias: importes con signo (ingreso +, gasto −).
    private static readonly (string Clave, string Concepto, string[] Cuentas)[] Explotacion =
    [
        ("40100", "1. Importe neto de la cifra de negocios", ["70"]),
        ("40200", "2. Variación de existencias de productos terminados y en curso de fabricación", ["71", "6930", "7930"]),
        ("40300", "3. Trabajos realizados por la empresa para su activo", ["73"]),
        ("40400", "4. Aprovisionamientos", ["600", "601", "602", "606", "607", "608", "609", "61", "6931", "6932", "6933", "7931", "7932", "7933"]),
        ("40500", "5. Otros ingresos de explotación", ["75", "740", "747"]),
        ("40600", "6. Gastos de personal", ["64", "7950", "7957"]),
        ("40700", "7. Otros gastos de explotación", ["62", "631", "634", "636", "639", "65", "694", "695", "794", "7954"]),
        ("40800", "8. Amortización del inmovilizado", ["68"]),
        ("40900", "9. Imputación de subvenciones de inmovilizado no financiero y otras", ["746"]),
        ("41000", "10. Excesos de provisiones", ["7951", "7952", "7955", "7956"]),
        ("41100", "11. Deterioro y resultado por enajenaciones del inmovilizado", ["670", "671", "672", "690", "691", "692", "770", "771", "772", "790", "791", "792"]),
        ("41300", "12. Otros resultados", ["678", "778"]),
    ];

    private static readonly (string Clave, string Concepto, string[] Cuentas)[] Financiero =
    [
        ("41400", "13. Ingresos financieros", ["760", "761", "762", "767", "769"]),
        ("41500", "14. Gastos financieros", ["660", "661", "662", "664", "665", "669"]),
        ("41600", "15. Variación de valor razonable en instrumentos financieros", ["663", "763"]),
        ("41700", "16. Diferencias de cambio", ["668", "768"]),
        ("41800", "17. Deterioro y resultado por enajenaciones de instrumentos financieros", ["666", "667", "673", "675", "696", "697", "698", "699", "766", "773", "775", "796", "797", "798", "799"]),
    ];

    private static readonly string[] Impuesto = ["6300", "6301", "633", "638", "630"];

    public async Task<ModeloDepositoDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var actual = await SaldosAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var anterior = await SaldosAsync(empresaId, ejercicio - 1, ct).ConfigureAwait(false);

        // Cada cuenta va a una sola partida: la del prefijo más largo que casa (y cuyo lado, si lo pide, coincide).
        var reglas = Activo.Select(x => (x.Clave, x.Cuentas)).Concat(Pasivo.Select(x => (x.Clave, x.Cuentas)))
            .Concat(Explotacion.Select(x => (x.Clave, x.Cuentas))).Concat(Financiero.Select(x => (x.Clave, x.Cuentas)))
            .Append((Clave: "41900", Cuentas: Impuesto))
            .SelectMany(x => x.Item2.Select(c => c.TrimStart('-')).Select(c => (x.Clave, Lado: c.Length > 2 && c[1] == ':' ? c[0] : ' ', Pref: c.Length > 2 && c[1] == ':' ? c[2..] : c)))
            .ToList();
        var sinClasificar = new SortedSet<string>(StringComparer.Ordinal);
        Dictionary<string, decimal> Asignar(IReadOnlyDictionary<string, decimal> saldos)
        {
            var porPartida = new Dictionary<string, decimal>(StringComparer.Ordinal);
            foreach (var (codigo, neto) in saldos)
            {
                var regla = reglas.Where(r => codigo.StartsWith(r.Pref, StringComparison.Ordinal) && (r.Lado == ' ' || (r.Lado == 'd' ? neto > 0m : neto < 0m)))
                    .OrderByDescending(r => r.Pref.Length).Select(r => r.Clave).FirstOrDefault();
                if (regla is null)
                {
                    sinClasificar.Add(codigo);
                    // Una cuenta de gestión sin partida va a otros resultados, para que la PyG cuadre con los saldos.
                    regla = codigo[0] is '6' or '7' ? "41300" : null;
                }

                if (regla is not null)
                {
                    porPartida[regla] = porPartida.GetValueOrDefault(regla) + neto;
                }
            }

            return porPartida;
        }

        var a = Asignar(actual);
        var n1 = Asignar(anterior);
        // Activo con su saldo deudor; patrimonio neto, pasivo y PyG con el acreedor (ingreso +, gasto −).
        PartidaDepositoDto P(string clave, string concepto, int nivel, bool acreedor) =>
            new(clave, concepto, nivel, Redondeo.Dos((acreedor ? -1m : 1m) * a.GetValueOrDefault(clave)), Redondeo.Dos((acreedor ? -1m : 1m) * n1.GetValueOrDefault(clave)));

        var pyg = new List<PartidaDepositoDto>();
        var expl = Explotacion.Select(x => P(x.Clave, x.Concepto, 1, true)).ToList();
        var resExpl = Total("49100", "A) RESULTADO DE EXPLOTACIÓN (1 a 12)", 0, expl);
        var fin = Financiero.Select(x => P(x.Clave, x.Concepto, 1, true)).ToList();
        var resFin = Total("49200", "B) RESULTADO FINANCIERO (13 a 17)", 0, fin);
        var antes = Total("49300", "C) RESULTADO ANTES DE IMPUESTOS (A + B)", 0, [resExpl, resFin]);
        var imp = P("41900", "18. Impuestos sobre beneficios", 1, true);
        var resultado = Total("49500", "D) RESULTADO DEL EJERCICIO (C + 18)", 0, [antes, imp]);
        pyg.AddRange(expl);
        pyg.Add(resExpl);
        pyg.AddRange(fin);
        pyg.AddRange([resFin, antes, imp, resultado]);

        var activoNc = Activo.Where(x => x.Clave.StartsWith("11", StringComparison.Ordinal)).Select(x => P(x.Clave, x.Concepto, x.Nivel, false)).ToList();
        var activoC = Activo.Where(x => x.Clave.StartsWith("12", StringComparison.Ordinal)).Select(x => P(x.Clave, x.Concepto, x.Nivel, false)).ToList();
        var tanc = Total("11000", "A) ACTIVO NO CORRIENTE", 1, activoNc);
        var tac = Total("12000", "B) ACTIVO CORRIENTE", 1, activoC);
        var totalActivo = Total("10000", "TOTAL ACTIVO (A + B)", 0, [tanc, tac]);

        var fondos = Pasivo.Where(x => x.Clave.StartsWith("21", StringComparison.Ordinal)).Select(x => P(x.Clave, x.Concepto, x.Nivel, true)).ToList();
        // VII: el resultado de la PyG (sin regularizar). Lo que traiga la 129 es de ejercicios anteriores sin aplicar (V).
        fondos.Insert(fondos.FindIndex(f => f.Clave == "21800"), new PartidaDepositoDto("21700", "VII. Resultado del ejercicio", 3, resultado.Actual, resultado.Anterior));
        var fp = Total("21000", "A-1) Fondos propios", 2, fondos);
        var subv = P("23000", "A-3) Subvenciones, donaciones y legados recibidos", 2, true);
        var pn = Total("20000", "A) PATRIMONIO NETO", 1, [fp, subv]);
        var pnc = Pasivo.Where(x => x.Clave.StartsWith("31", StringComparison.Ordinal)).Select(x => P(x.Clave, x.Concepto, x.Nivel, true)).ToList();
        var pc = Pasivo.Where(x => x.Clave.StartsWith("32", StringComparison.Ordinal)).Select(x => P(x.Clave, x.Concepto, x.Nivel, true)).ToList();
        var tpnc = Total("31000", "B) PASIVO NO CORRIENTE", 1, pnc);
        var tpc = Total("32000", "C) PASIVO CORRIENTE", 1, pc);
        var totalPasivo = Total("30000", "TOTAL PATRIMONIO NETO Y PASIVO (A + B + C)", 0, [pn, tpnc, tpc]);

        var balance = new List<PartidaDepositoDto> { tanc };
        balance.AddRange(activoNc);
        balance.Add(tac);
        balance.AddRange(activoC);
        balance.AddRange([totalActivo, pn, fp]);
        balance.AddRange(fondos);
        balance.AddRange([subv, tpnc]);
        balance.AddRange(pnc);
        balance.Add(tpc);
        balance.AddRange(pc);
        balance.Add(totalPasivo);

        return new ModeloDepositoDto(ejercicio, Limpiar(balance), Limpiar(pyg), totalActivo.Actual, totalPasivo.Actual,
            totalActivo.Actual == totalPasivo.Actual && totalActivo.Anterior == totalPasivo.Anterior, sinClasificar.ToList());
    }

    private async Task<IReadOnlyDictionary<string, decimal>> SaldosAsync(Guid empresaId, int ejercicio, CancellationToken ct) =>
        (await _asientos.SaldosAntesDelCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false))
            .GroupBy(s => s.CuentaCodigo).ToDictionary(g => g.Key, g => Redondeo.Dos(g.Sum(s => s.Debe - s.Haber)))
            .Where(x => x.Value != 0m).ToDictionary(x => x.Key, x => x.Value);

    private static PartidaDepositoDto Total(string clave, string concepto, int nivel, IReadOnlyCollection<PartidaDepositoDto> partes) =>
        new(clave, concepto, nivel, Redondeo.Dos(partes.Sum(p => p.Actual)), Redondeo.Dos(partes.Sum(p => p.Anterior)));

    // Los totales se quedan aunque sean cero; las partidas vacías en los dos ejercicios, no.
    private static List<PartidaDepositoDto> Limpiar(List<PartidaDepositoDto> partidas) =>
        partidas.Where(p => p.Actual != 0m || p.Anterior != 0m || p.Clave.EndsWith("000", StringComparison.Ordinal) || p.Clave.StartsWith("49", StringComparison.Ordinal)).ToList();
}
