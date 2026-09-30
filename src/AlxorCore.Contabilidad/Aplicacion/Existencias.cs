using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Stock de un artículo a una fecha, valorado con el método de la empresa.</summary>
public sealed record ExistenciaValorada(Guid ProductoId, string Nombre, string? Familia, decimal Cantidad, decimal CosteUnitario, decimal Valor);

/// <summary>Puerto hacia el inventario: las existencias valoradas a una fecha (lo implementa la API).</summary>
public interface IValoracionExistencias
{
    Task<IReadOnlyList<ExistenciaValorada>> ValorarAsync(Guid empresaId, DateOnly fecha, CancellationToken ct = default);
}

public interface IRepositorioCuentasExistencias
{
    Task<IReadOnlyList<CuentaExistencias>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(CuentaExistencias cuenta);

    void Eliminar(CuentaExistencias cuenta);
}

public sealed record CuentaExistenciasDto(string? Familia, string CuentaStock, string CuentaVariacion);

/// <summary>Una cuenta de existencias en la regularización: lo que tenía (inicial), lo que hay (final) y la variación.</summary>
public sealed record LineaRegularizacionExistenciasDto(string CuentaStock, string CuentaVariacion, decimal Inicial, decimal Final, decimal Calculado, decimal Variacion,
    IReadOnlyList<ExistenciaValorada> Articulos);

/// <summary>Regularización de existencias de un ejercicio: su previsión o la contabilizada (con su asiento).</summary>
public sealed record RegularizacionExistenciasDto(int Ejercicio, DateOnly Fecha, bool Contabilizada, Guid? AsientoId, int? NumeroAsiento, bool EjercicioCerrado,
    decimal Inicial, decimal Final, IReadOnlyList<LineaRegularizacionExistenciasDto> Lineas);

/// <summary>Valor final corregido a mano para una cuenta (recuento físico, deterioro de valor…).</summary>
public sealed record AjusteExistencias(string CuentaStock, decimal Final);

/// <summary>
/// Regularización de existencias al cierre del ejercicio (PGC, grupo 3): la existencia inicial (el saldo de cada
/// cuenta 3xx en el ejercicio) se da de baja contra su cuenta de variación (61x/71x) y la final (el stock del
/// inventario a 31 de diciembre, valorado con el método de la empresa, o el valor que se indique) se da de alta.
/// El asiento, de origen <c>Existencias</c>, va al diario de cierre y se hace antes de cerrar el ejercicio; se anula
/// con un contraasiento para repetirla.
/// </summary>
public sealed class RegularizacionExistencias
{
    public const string Origen = "Existencias";
    private const string ConceptoFinal = "Existencia final";

    private readonly IRepositorioCuentasExistencias _cuentasExistencias;
    private readonly IValoracionExistencias _valoracion;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public RegularizacionExistencias(IRepositorioCuentasExistencias cuentasExistencias, IValoracionExistencias valoracion, IRepositorioAsientos asientos,
        IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _cuentasExistencias = cuentasExistencias; _valoracion = valoracion; _asientos = asientos; _cuentas = cuentas; _unidad = unidad; _reloj = reloj;
    }

    public async Task<IReadOnlyList<CuentaExistenciasDto>> CuentasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = (await _cuentasExistencias.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Select(c => new CuentaExistenciasDto(c.Familia, c.CuentaStock, c.CuentaVariacion)).ToList();
        if (!lista.Any(c => c.Familia is null))
        {
            lista.Insert(0, new CuentaExistenciasDto(null, CuentaExistencias.CuentaPorDefecto, CuentaExistencias.VariacionDe(CuentaExistencias.CuentaPorDefecto)));
        }

        return lista.OrderBy(c => c.Familia is not null).ThenBy(c => c.Familia, StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Sustituye la asignación de cuentas (una por familia y, sin familia, la de por defecto).</summary>
    public async Task<Resultado<IReadOnlyList<CuentaExistenciasDto>>> FijarCuentasAsync(Guid empresaId, IReadOnlyList<CuentaExistenciasDto> cuentas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cuentas);
        var nuevas = new List<CuentaExistencias>();
        foreach (var c in cuentas)
        {
            var r = CuentaExistencias.Crear(empresaId, c.Familia, c.CuentaStock, c.CuentaVariacion);
            if (r.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<CuentaExistenciasDto>>(r.Error);
            }

            if (nuevas.Any(n => string.Equals(n.Familia, r.Valor.Familia, StringComparison.OrdinalIgnoreCase)))
            {
                return Resultado.Fallo<IReadOnlyList<CuentaExistenciasDto>>(Error.Validacion("existencias.familia_repetida",
                    r.Valor.Familia is null ? "Solo hay una cuenta por defecto." : $"La familia {r.Valor.Familia} está dos veces."));
            }

            nuevas.Add(r.Valor);
        }

        foreach (var vieja in await _cuentasExistencias.ListarAsync(empresaId, ct).ConfigureAwait(false))
        {
            _cuentasExistencias.Eliminar(vieja);
        }

        foreach (var n in nuevas)
        {
            _cuentasExistencias.Agregar(n);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await CuentasAsync(empresaId, ct).ConfigureAwait(false));
    }

    /// <summary>La regularización contabilizada del ejercicio o, si no la hay, lo que se contabilizaría.</summary>
    public async Task<RegularizacionExistenciasDto> ConsultarAsync(Guid empresaId, int ejercicio, IReadOnlyList<AjusteExistencias>? ajustes = null, CancellationToken ct = default)
    {
        var fecha = new DateOnly(ejercicio, 12, 31);
        var cerrado = await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var todos = await _asientos.TodosAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var vigente = Vigente(todos);
        var mapa = await CuentasAsync(empresaId, ct).ConfigureAwait(false);
        var porDefecto = mapa.First(c => c.Familia is null);

        // Existencia inicial: el saldo de cada cuenta de existencias en el ejercicio, sin la propia regularización ni
        // el cierre (que los salda).
        var excluidos = todos.Where(a => a.Origen is Origen or "Cierre" || (a.AnulaAsientoId is { } x && todos.Any(o => o.Id == x && o.Origen == Origen)))
            .Select(a => a.Id).ToHashSet();
        var inicial = todos.Where(a => !excluidos.Contains(a.Id)).SelectMany(a => a.Apuntes)
            .Where(p => EsExistencias(p.CuentaCodigo))
            .GroupBy(p => p.CuentaCodigo)
            .ToDictionary(g => g.Key, g => Redondeo.Dos(g.Sum(p => p.Debe - p.Haber)));

        var articulos = await _valoracion.ValorarAsync(empresaId, fecha, ct).ConfigureAwait(false);
        var porCuenta = articulos.Where(a => a.Valor != 0m)
            .GroupBy(a => (mapa.FirstOrDefault(m => m.Familia is not null && string.Equals(m.Familia, a.Familia, StringComparison.OrdinalIgnoreCase)) ?? porDefecto).CuentaStock)
            .ToDictionary(g => g.Key, g => g.ToList());

        var codigos = inicial.Keys.Concat(porCuenta.Keys).Concat((ajustes ?? []).Select(a => a.CuentaStock.Trim())).Distinct().Order(StringComparer.Ordinal).ToList();
        var lineas = new List<LineaRegularizacionExistenciasDto>();
        foreach (var codigo in codigos)
        {
            var ini = inicial.GetValueOrDefault(codigo);
            var arts = porCuenta.GetValueOrDefault(codigo) ?? [];
            var calculado = Redondeo.Dos(arts.Sum(a => a.Valor));
            var final = ajustes?.FirstOrDefault(a => a.CuentaStock.Trim() == codigo) is { } aj ? Redondeo.Dos(aj.Final) : calculado;
            if (vigente is not null)
            {
                // Contabilizada: lo que dice el asiento (el debe de la cuenta de existencias es la final).
                final = Redondeo.Dos(vigente.Apuntes.Where(p => p.CuentaCodigo == codigo && p.Concepto == ConceptoFinal).Sum(p => p.Debe - p.Haber));
            }

            if (ini == 0m && final == 0m && calculado == 0m)
            {
                continue;
            }

            var variacion = mapa.FirstOrDefault(m => m.CuentaStock == codigo)?.CuentaVariacion ?? CuentaExistencias.VariacionDe(codigo);
            lineas.Add(new LineaRegularizacionExistenciasDto(codigo, variacion, ini, final, calculado, Redondeo.Dos(final - ini),
                arts.OrderByDescending(a => a.Valor).ToList()));
        }

        return new RegularizacionExistenciasDto(ejercicio, fecha, vigente is not null, vigente?.Id, vigente?.Numero, cerrado,
            Redondeo.Dos(lineas.Sum(l => l.Inicial)), Redondeo.Dos(lineas.Sum(l => l.Final)), lineas);
    }

    /// <summary>Contabiliza la regularización del ejercicio a 31 de diciembre.</summary>
    public async Task<Resultado<RegularizacionExistenciasDto>> ContabilizarAsync(Guid empresaId, int ejercicio, IReadOnlyList<AjusteExistencias>? ajustes, CancellationToken ct = default)
    {
        if (ejercicio is < 2000 or > 2100)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Validacion("existencias.ejercicio", "Ejercicio no válido."));
        }

        if (await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Conflicto("asiento.ejercicio_cerrado",
                $"El ejercicio {ejercicio} está cerrado: la regularización de existencias se hace antes del cierre."));
        }

        foreach (var a in ajustes ?? [])
        {
            if (a.Final < 0m || !EsExistencias(a.CuentaStock?.Trim() ?? string.Empty))
            {
                return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Validacion("existencias.ajuste", "Cada ajuste es una cuenta del grupo 3 con un valor no negativo."));
            }
        }

        var previa = await ConsultarAsync(empresaId, ejercicio, ajustes, ct).ConfigureAwait(false);
        if (previa.Contabilizada)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Conflicto("existencias.ya_regularizado",
                $"Las existencias de {ejercicio} ya están regularizadas (asiento {previa.NumeroAsiento}): anúlala para repetirla."));
        }

        // Inicial: variación al debe, existencias al haber. Final: existencias al debe, variación al haber.
        var lineas = new List<LineaAsiento>();
        foreach (var l in previa.Lineas)
        {
            Apunte(lineas, l.CuentaVariacion, l.CuentaStock, l.Inicial, "Existencia inicial");
            Apunte(lineas, l.CuentaStock, l.CuentaVariacion, l.Final, ConceptoFinal);
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Validacion("existencias.sin_existencias",
                $"No hay existencias que regularizar en {ejercicio}: ni saldo en el grupo 3 ni stock valorado a 31/12."));
        }

        await AsegurarCuentasAsync(empresaId, previa.Lineas.SelectMany(l => new[] { l.CuentaStock, l.CuentaVariacion }).Distinct().ToList(), ct).ConfigureAwait(false);
        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var asiento = Asiento.Crear(empresaId, ejercicio, numero, previa.Fecha, $"Regularización de existencias {ejercicio}", Origen, lineas, _reloj);
        if (asiento.EsFallo)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(asiento.Error);
        }

        _asientos.Agregar(asiento.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await ConsultarAsync(empresaId, ejercicio, null, ct).ConfigureAwait(false));
    }

    /// <summary>Anula la regularización contabilizada con un contraasiento (a 31/12, con el mes abierto).</summary>
    public async Task<Resultado<RegularizacionExistenciasDto>> AnularAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        if (await _asientos.TieneCierreAsync(empresaId, ejercicio, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {ejercicio} está cerrado."));
        }

        var vigente = Vigente(await _asientos.TodosAsync(empresaId, ejercicio, ct).ConfigureAwait(false));
        if (vigente is null || await _asientos.ObtenerAsync(vigente.Id, ct).ConfigureAwait(false) is not { } original)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(Error.NoEncontrado("existencias.no_regularizado", $"Las existencias de {ejercicio} no están regularizadas."));
        }

        if (await PeriodosContables.ComprobarAsync(_asientos, empresaId, original.Fecha, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(error);
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, ejercicio, ct).ConfigureAwait(false);
        var contra = Asiento.CrearAnulacion(original, numero, original.Fecha, _reloj);
        if (contra.EsFallo)
        {
            return Resultado.Fallo<RegularizacionExistenciasDto>(contra.Error);
        }

        _asientos.Agregar(contra.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await ConsultarAsync(empresaId, ejercicio, null, ct).ConfigureAwait(false));
    }

    private static AsientoDto? Vigente(IReadOnlyList<AsientoDto> todos) =>
        todos.Where(a => a.Origen == Origen && a.AnuladoPorId is null && !todos.Any(o => o.AnulaAsientoId == a.Id)).MaxBy(a => a.Numero);

    private static bool EsExistencias(string codigo) =>
        codigo.Length >= 3 && codigo[0] == '3' && !codigo.StartsWith("39", StringComparison.Ordinal) && codigo.All(char.IsAsciiDigit);

    private static void Apunte(List<LineaAsiento> lineas, string debe, string haber, decimal importe, string concepto)
    {
        if (importe == 0m)
        {
            return;
        }

        // Un saldo negativo (stock valorado en negativo o una cuenta acreedora) invierte el apunte.
        var (d, h, i) = importe > 0m ? (debe, haber, importe) : (haber, debe, -importe);
        lineas.Add(new LineaAsiento(d, i, 0m, concepto));
        lineas.Add(new LineaAsiento(h, 0m, i, concepto));
    }

    private async Task AsegurarCuentasAsync(Guid empresaId, IReadOnlyList<string> codigos, CancellationToken ct)
    {
        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var existentes = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        foreach (var c in codigos.Where(c => !existentes.Contains(c)))
        {
            if (Cuenta.Crear(empresaId, c, CuentaExistencias.NombreDe(c)) is { EsCorrecto: true } cuenta)
            {
                _cuentas.Agregar(cuenta.Valor);
            }
        }
    }
}
