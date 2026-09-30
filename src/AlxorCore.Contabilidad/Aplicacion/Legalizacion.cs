using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Fila del balance de comprobación (sumas y saldos) a una fecha.</summary>
public sealed record SumasSaldosDto(string Cuenta, string Nombre, decimal SumaDebe, decimal SumaHaber, decimal SaldoDeudor, decimal SaldoAcreedor);

/// <summary>Balance de comprobación al final de un trimestre.</summary>
public sealed record ComprobacionTrimestreDto(int Trimestre, DateOnly Hasta, IReadOnlyList<SumasSaldosDto> Cuentas);

/// <summary>
/// Lo que llevan los libros que se legalizan en el Registro Mercantil (art. 25-28 del Código de Comercio): el diario del
/// ejercicio y el de inventarios y cuentas anuales (balances de comprobación trimestrales, el inventario de cierre y las
/// cuentas anuales). La API lo convierte en PDF y lo empaqueta para Legalia.
/// </summary>
public sealed record LibrosLegalizacionDto(int Ejercicio, IReadOnlyList<AsientoDto> Diario, IReadOnlyDictionary<string, string> NombresCuentas,
    IReadOnlyList<ComprobacionTrimestreDto> Trimestres, IReadOnlyList<SumasSaldosDto> Inventario, ModeloDepositoDto CuentasAnuales, bool EjercicioCerrado);

public sealed class LibrosLegalizacion
{
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly GenerarModeloDeposito _deposito;

    public LibrosLegalizacion(IRepositorioAsientos asientos, IRepositorioCuentas cuentas, GenerarModeloDeposito deposito)
    {
        _asientos = asientos; _cuentas = cuentas; _deposito = deposito;
    }

    public async Task<LibrosLegalizacionDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var diario = (await _asientos.TodosAsync(empresaId, ejercicio, ct).ConfigureAwait(false)).OrderBy(a => a.Numero).ToList();
        var nombres = (await _cuentas.ListarAsync(empresaId, ct).ConfigureAwait(false)).GroupBy(c => c.Codigo).ToDictionary(g => g.Key, g => g.First().Nombre, StringComparer.Ordinal);

        // Comprobación de cada trimestre: lo registrado hasta su último día, sin la regularización ni el cierre del ejercicio.
        var gestion = diario.Where(a => a.Origen is not ("Regularizacion" or "Cierre")).ToList();
        var trimestres = new List<ComprobacionTrimestreDto>();
        for (var t = 1; t <= 4; t++)
        {
            var hasta = new DateOnly(ejercicio, t * 3, 1).AddMonths(1).AddDays(-1);
            trimestres.Add(new ComprobacionTrimestreDto(t, hasta, Sumas(gestion.Where(a => a.Fecha <= hasta), nombres)));
        }

        // Inventario de cierre: el saldo de cada cuenta de balance a 31/12, antes del asiento de cierre.
        var inventario = Sumas(gestion.Concat(diario.Where(a => a.Origen == "Regularizacion")), nombres)
            .Where(s => s.Cuenta[0] is not ('6' or '7') && (s.SaldoDeudor != 0m || s.SaldoAcreedor != 0m)).ToList();

        var cuentas = await _deposito.EjecutarAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var cerrado = await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        return new LibrosLegalizacionDto(ejercicio, diario, nombres, trimestres, inventario, cuentas, cerrado);
    }

    private static List<SumasSaldosDto> Sumas(IEnumerable<AsientoDto> asientos, IReadOnlyDictionary<string, string> nombres) =>
        asientos.SelectMany(a => a.Apuntes).GroupBy(p => p.CuentaCodigo).OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g =>
            {
                var d = Redondeo.Dos(g.Sum(p => p.Debe));
                var h = Redondeo.Dos(g.Sum(p => p.Haber));
                return new SumasSaldosDto(g.Key, nombres.GetValueOrDefault(g.Key, "—"), d, h, d > h ? Redondeo.Dos(d - h) : 0m, h > d ? Redondeo.Dos(h - d) : 0m);
            }).ToList();
}
