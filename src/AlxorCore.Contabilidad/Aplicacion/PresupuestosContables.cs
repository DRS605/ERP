using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Persistencia de los presupuestos contables.</summary>
public interface IRepositorioPresupuestosContables
{
    Task<IReadOnlyList<PresupuestoContable>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<PresupuestoContable?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(PresupuestoContable presupuesto);
}

public sealed record LineaPresupuestoDto(
    Guid Id, string CuentaCodigo, Guid? CentroId, Guid? PartidaId, NaturalezaAnalitica Naturaleza, IReadOnlyList<decimal> Importes, decimal Total);

public sealed record PresupuestoContableDto(
    Guid Id, string Codigo, string Nombre, DateOnly Desde, DateOnly Hasta, int Meses, EstadoPresupuesto Estado,
    decimal TotalIngresos, decimal TotalGastos, decimal Resultado, IReadOnlyList<LineaPresupuestoDto> Lineas)
{
    public static PresupuestoContableDto De(PresupuestoContable p)
    {
        ArgumentNullException.ThrowIfNull(p);
        var ingresos = p.Lineas.Where(l => l.Naturaleza == NaturalezaAnalitica.Ingreso).Sum(l => l.Total);
        var gastos = p.Lineas.Where(l => l.Naturaleza == NaturalezaAnalitica.Gasto).Sum(l => l.Total);
        return new(p.Id, p.Codigo, p.Nombre, p.Desde, p.Hasta, p.Meses, p.Estado, ingresos, gastos, ingresos - gastos,
            p.Lineas.OrderBy(l => l.CuentaCodigo, StringComparer.Ordinal)
                .Select(l => new LineaPresupuestoDto(l.Id, l.CuentaCodigo, l.CentroId, l.PartidaId, l.Naturaleza, l.Importes, l.Total)).ToList());
    }
}

public sealed record CrearPresupuestoComando(string? Codigo, string? Nombre, DateOnly Desde, int Meses = 12, IReadOnlyList<DatosLineaPresupuesto>? Lineas = null);

public sealed record CopiarPresupuestoComando(string? Codigo, string? Nombre, decimal IncrementoPorcentaje = 0m, DateOnly? Desde = null);

/// <summary>Generar un presupuesto a partir del real de otro periodo (por ejemplo, el año pasado + 3 %).</summary>
public sealed record PresupuestoDesdeRealComando(
    string? Codigo, string? Nombre, DateOnly Desde, int Meses, DateOnly OrigenDesde, decimal IncrementoPorcentaje = 0m, bool PorCentro = false);

/// <summary>Casos de uso de los presupuestos contables.</summary>
public sealed class GestionPresupuestosContables
{
    private readonly IRepositorioPresupuestosContables _presupuestos;
    private readonly IRepositorioAnalitica _analitica;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public GestionPresupuestosContables(IRepositorioPresupuestosContables presupuestos, IRepositorioAnalitica analitica, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _presupuestos = presupuestos;
        _analitica = analitica;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<PresupuestoContableDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _presupuestos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .OrderByDescending(p => p.Desde).ThenBy(p => p.Codigo, StringComparer.Ordinal).Select(PresupuestoContableDto.De).ToList();

    public async Task<Resultado<PresupuestoContableDto>> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _presupuestos.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p
            ? Resultado.Ok(PresupuestoContableDto.De(p))
            : NoExiste();

    public async Task<Resultado<PresupuestoContableDto>> CrearAsync(Guid empresaId, CrearPresupuestoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return await GuardarNuevoAsync(empresaId, comando.Codigo, comando.Nombre, comando.Desde, comando.Meses, comando.Lineas ?? [], ct).ConfigureAwait(false);
    }

    public async Task<Resultado<PresupuestoContableDto>> FijarLineasAsync(Guid id, IReadOnlyList<DatosLineaPresupuesto> lineas, CancellationToken ct = default)
    {
        var p = await _presupuestos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return NoExiste();
        }

        var error = await ReferenciasAsync(lineas, ct).ConfigureAwait(false);
        var r = error is null ? p.FijarLineas(lineas) : Resultado.Fallo(error);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PresupuestoContableDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PresupuestoContableDto.De(p));
    }

    public async Task<Resultado<PresupuestoContableDto>> AprobarAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _presupuestos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return NoExiste();
        }

        var r = p.Aprobar();
        if (r.EsFallo)
        {
            return Resultado.Fallo<PresupuestoContableDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PresupuestoContableDto.De(p));
    }

    /// <summary>Copia un presupuesto (una versión nueva, en borrador), con un incremento opcional.</summary>
    public async Task<Resultado<PresupuestoContableDto>> CopiarAsync(Guid empresaId, Guid id, CopiarPresupuestoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var origen = await _presupuestos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (origen is null)
        {
            return NoExiste();
        }

        var factor = 1m + (comando.IncrementoPorcentaje / 100m);
        var lineas = origen.Lineas
            .Select(l => new DatosLineaPresupuesto(l.CuentaCodigo, l.CentroId, l.PartidaId, l.Importes.Select(i => Math.Max(0m, decimal.Round(i * factor, 2, MidpointRounding.AwayFromZero))).ToList()))
            .ToList();
        return await GuardarNuevoAsync(empresaId, comando.Codigo, comando.Nombre, comando.Desde ?? origen.Desde, origen.Meses, lineas, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Genera un presupuesto con el real de otro periodo de la misma duración, mes a mes, más un
    /// incremento. Por cuenta, o por cuenta, centro y partida (con lo imputado en la analítica).
    /// </summary>
    public async Task<Resultado<PresupuestoContableDto>> DesdeRealAsync(Guid empresaId, PresupuestoDesdeRealComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.Meses is < 1 or > PresupuestoContable.MesesMaximos)
        {
            return Resultado.Fallo<PresupuestoContableDto>(Error.Validacion("presupuesto.meses", $"El presupuesto abarca de 1 a {PresupuestoContable.MesesMaximos} meses."));
        }

        var inicio = new DateOnly(comando.OrigenDesde.Year, comando.OrigenDesde.Month, 1);
        var fin = inicio.AddMonths(comando.Meses).AddDays(-1);
        int Mes(DateOnly f) => ((f.Year - inicio.Year) * 12) + f.Month - inicio.Month;

        IEnumerable<(string Cuenta, Guid? Centro, Guid? Partida, int Mes, decimal Importe)> real = comando.PorCentro
            ? (await _analitica.ImputacionesAsync(empresaId, inicio, fin, ct).ConfigureAwait(false))
                .Select(i => (i.CuentaCodigo, (Guid?)i.CentroId, i.PartidaId, Mes(i.Fecha), i.Importe))
            : (await _analitica.ApuntesAsync(empresaId, inicio, fin, ct).ConfigureAwait(false))
                .Select(a => (a.CuentaCodigo, (Guid?)null, (Guid?)null, Mes(a.Fecha), a.Importe));

        var factor = 1m + (comando.IncrementoPorcentaje / 100m);
        var lineas = real
            .GroupBy(r => (r.Cuenta, r.Centro, r.Partida))
            .Select(g =>
            {
                var importes = new decimal[comando.Meses];
                foreach (var r in g)
                {
                    importes[r.Mes] += r.Importe;
                }

                return new DatosLineaPresupuesto(g.Key.Cuenta, g.Key.Centro, g.Key.Partida,
                    importes.Select(i => Math.Max(0m, decimal.Round(i * factor, 2, MidpointRounding.AwayFromZero))).ToList());
            })
            .Where(l => l.Importes!.Any(i => i != 0m))
            .ToList();

        return await GuardarNuevoAsync(empresaId, comando.Codigo, comando.Nombre, comando.Desde, comando.Meses, lineas, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<PresupuestoContableDto>> GuardarNuevoAsync(
        Guid empresaId, string? codigo, string? nombre, DateOnly desde, int meses, IReadOnlyList<DatosLineaPresupuesto> lineas, CancellationToken ct)
    {
        var existentes = await _presupuestos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        if (codigo is not null && existentes.Any(p => string.Equals(p.Codigo, codigo.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<PresupuestoContableDto>(Error.Conflicto("presupuesto.codigo_repetido", $"Ya existe el presupuesto {codigo.Trim().ToUpperInvariant()}."));
        }

        var nuevo = PresupuestoContable.Crear(empresaId, codigo, nombre, desde, meses, _reloj.AhoraUtc);
        if (nuevo.EsFallo)
        {
            return Resultado.Fallo<PresupuestoContableDto>(nuevo.Error);
        }

        var error = await ReferenciasAsync(lineas, ct).ConfigureAwait(false);
        var r = error is null ? nuevo.Valor.FijarLineas(lineas) : Resultado.Fallo(error);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PresupuestoContableDto>(r.Error);
        }

        _presupuestos.Agregar(nuevo.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PresupuestoContableDto.De(nuevo.Valor));
    }

    private async Task<Error?> ReferenciasAsync(IReadOnlyList<DatosLineaPresupuesto> lineas, CancellationToken ct)
    {
        if (lineas.All(l => l.CentroId is null && l.PartidaId is null))
        {
            return null;
        }

        var centros = (await _analitica.CentrosAsync(ct).ConfigureAwait(false)).Select(c => c.Id).ToHashSet();
        var partidas = (await _analitica.PartidasAsync(ct).ConfigureAwait(false)).Select(p => p.Id).ToHashSet();
        return lineas.Any(l => (l.CentroId is { } c && !centros.Contains(c)) || (l.PartidaId is { } p && !partidas.Contains(p)))
            ? Error.Validacion("presupuesto.analitica", "Algún centro o partida de las líneas no existe.")
            : null;
    }

    private static Resultado<PresupuestoContableDto> NoExiste() =>
        Resultado.Fallo<PresupuestoContableDto>(Error.NoEncontrado("presupuesto.no_encontrado", "El presupuesto no existe."));
}

/// <summary>Seguimiento de una línea: presupuesto frente a real, mes a mes y acumulado.</summary>
public sealed record SeguimientoLineaDto(
    string CuentaCodigo, Guid? CentroId, Guid? PartidaId, NaturalezaAnalitica Naturaleza,
    IReadOnlyList<decimal> PresupuestoMensual, IReadOnlyList<decimal> RealMensual,
    decimal PresupuestoAcumulado, decimal RealAcumulado, decimal Desviacion, decimal? PorcentajeEjecucion, bool Favorable, decimal PresupuestoTotal);

/// <summary>Seguimiento de un presupuesto hasta un mes.</summary>
public sealed record SeguimientoPresupuestoDto(
    Guid PresupuestoId, string Codigo, DateOnly Desde, DateOnly HastaSeguimiento, int MesesTranscurridos,
    IReadOnlyList<SeguimientoLineaDto> Lineas,
    decimal IngresosPresupuesto, decimal IngresosReal, decimal GastosPresupuesto, decimal GastosReal,
    decimal ResultadoPresupuesto, decimal ResultadoReal, IReadOnlyList<string> Avisos);

/// <summary>
/// Caso de uso: compara el presupuesto con el real hasta un mes. Sin centro ni partida, el real sale de
/// los apuntes de la cuenta (o grupo); con ellos, de lo imputado en la analítica a ese centro (y sus
/// hijos) y partida. Una desviación es favorable si se gasta menos o se ingresa más de lo previsto.
/// </summary>
public sealed class SeguimientoPresupuesto
{
    private readonly IRepositorioPresupuestosContables _presupuestos;
    private readonly IRepositorioAnalitica _analitica;

    public SeguimientoPresupuesto(IRepositorioPresupuestosContables presupuestos, IRepositorioAnalitica analitica)
    {
        _presupuestos = presupuestos;
        _analitica = analitica;
    }

    public async Task<Resultado<SeguimientoPresupuestoDto>> EjecutarAsync(Guid empresaId, Guid id, DateOnly? hasta = null, CancellationToken ct = default)
    {
        var p = await _presupuestos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<SeguimientoPresupuestoDto>(Error.NoEncontrado("presupuesto.no_encontrado", "El presupuesto no existe."));
        }

        var corte = hasta is { } h ? (h < p.Desde ? p.Desde : h > p.Hasta ? p.Hasta : h) : p.Hasta;
        var transcurridos = (p.MesDe(corte) ?? (p.Meses - 1)) + 1;
        var finCorte = p.Desde.AddMonths(transcurridos).AddDays(-1);

        var apuntes = await _analitica.ApuntesAsync(empresaId, p.Desde, finCorte, ct).ConfigureAwait(false);
        var imputaciones = p.Lineas.Any(l => l.CentroId is not null || l.PartidaId is not null)
            ? await _analitica.ImputacionesAsync(empresaId, p.Desde, finCorte, ct).ConfigureAwait(false)
            : [];
        var centros = (await _analitica.CentrosAsync(ct).ConfigureAwait(false)).ToLookup(c => c.PadreId, c => c.Id);
        var partidas = (await _analitica.PartidasAsync(ct).ConfigureAwait(false)).ToLookup(x => x.PadreId, x => x.Id);

        var lineas = new List<SeguimientoLineaDto>();
        foreach (var l in p.Lineas.OrderBy(l => l.CuentaCodigo, StringComparer.Ordinal))
        {
            var real = new decimal[p.Meses];
            if (l.CentroId is null && l.PartidaId is null)
            {
                foreach (var a in apuntes.Where(a => a.CuentaCodigo.StartsWith(l.CuentaCodigo, StringComparison.Ordinal)))
                {
                    real[p.MesDe(a.Fecha)!.Value] += a.Importe;
                }
            }
            else
            {
                var enCentro = l.CentroId is { } c ? Subarbol(centros, c) : null;
                var enPartida = l.PartidaId is { } pa ? Subarbol(partidas, pa) : null;
                foreach (var i in imputaciones.Where(i => i.CuentaCodigo.StartsWith(l.CuentaCodigo, StringComparison.Ordinal)
                             && (enCentro is null || enCentro.Contains(i.CentroId))
                             && (enPartida is null || (i.PartidaId is { } ip && enPartida.Contains(ip)))))
                {
                    real[p.MesDe(i.Fecha)!.Value] += i.Importe;
                }
            }

            var presupAcum = l.Importes.Take(transcurridos).Sum();
            var realAcum = real.Take(transcurridos).Sum();
            var desviacion = realAcum - presupAcum;
            var favorable = l.Naturaleza == NaturalezaAnalitica.Gasto ? desviacion <= 0m : desviacion >= 0m;
            lineas.Add(new SeguimientoLineaDto(l.CuentaCodigo, l.CentroId, l.PartidaId, l.Naturaleza, l.Importes, real,
                presupAcum, realAcum, desviacion, presupAcum == 0m ? null : Math.Round(realAcum * 100m / presupAcum, 2), favorable, l.Total));
        }

        decimal Suma(NaturalezaAnalitica n, Func<SeguimientoLineaDto, decimal> f) => lineas.Where(x => x.Naturaleza == n).Sum(f);
        var ip2 = Suma(NaturalezaAnalitica.Ingreso, x => x.PresupuestoAcumulado);
        var ir = Suma(NaturalezaAnalitica.Ingreso, x => x.RealAcumulado);
        var gp = Suma(NaturalezaAnalitica.Gasto, x => x.PresupuestoAcumulado);
        var gr = Suma(NaturalezaAnalitica.Gasto, x => x.RealAcumulado);
        return Resultado.Ok(new SeguimientoPresupuestoDto(p.Id, p.Codigo, p.Desde, finCorte, transcurridos, lineas,
            ip2, ir, gp, gr, ip2 - gp, ir - gr, Solapes(p)));
    }

    /// <summary>Un centro o partida y todos sus descendientes.</summary>
    private static HashSet<Guid> Subarbol(ILookup<Guid?, Guid> hijos, Guid raiz)
    {
        var resultado = new HashSet<Guid> { raiz };
        var pendientes = new Stack<Guid>([raiz]);
        while (pendientes.TryPop(out var actual))
        {
            foreach (var hijo in hijos[actual].Where(resultado.Add))
            {
                pendientes.Push(hijo);
            }
        }

        return resultado;
    }

    /// <summary>Avisa si dos líneas cuentan el mismo real (por ejemplo, «62» y «629» sin centro): se sumaría dos veces.</summary>
    private static List<string> Solapes(PresupuestoContable p) =>
        p.Lineas.SelectMany(a => p.Lineas.Where(b => a != b && a.CentroId == b.CentroId && a.PartidaId == b.PartidaId
                && b.CuentaCodigo.Length > a.CuentaCodigo.Length && b.CuentaCodigo.StartsWith(a.CuentaCodigo, StringComparison.Ordinal))
            .Select(b => $"La línea {a.CuentaCodigo} incluye a la {b.CuentaCodigo}: su real se cuenta en las dos."))
        .ToList();
}
