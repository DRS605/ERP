using System.Globalization;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Analisis.Aplicacion;

/// <summary>Ejecuta una sentencia de solo lectura (con la RLS de la empresa activa) y devuelve sus filas.</summary>
public interface IEjecutorAnalisis
{
    Task<IReadOnlyList<object?[]>> EjecutarAsync(SentenciaSql sentencia, CancellationToken ct = default);
}

/// <summary>Qué conjuntos puede ver quien consulta (según los módulos contratados).</summary>
public interface IPermisosAnalisis
{
    bool Puede(DatasetAnalisis dataset);
}

/// <summary>Dimensión o medida tal y como se enseña en el diseñador de informes.</summary>
public sealed record CampoDto(string Clave, string Nombre, string Grupo, TipoDato Tipo, bool Aditiva = true, string? Descripcion = null);

public sealed record DatasetDto(string Clave, string Nombre, string Descripcion, IReadOnlyList<CampoDto> Dimensiones, IReadOnlyList<CampoDto> Medidas,
    IReadOnlyList<string> FilasDefecto, IReadOnlyList<string> MedidasDefecto, bool Detalle, string? VistaDocumento);

public sealed record ValorDimensionDto(string? Valor, decimal Registros);

public sealed record DetalleAnalisisDto(IReadOnlyList<ColumnaResultado> Columnas, IReadOnlyList<IReadOnlyList<object?>> Filas, IReadOnlyList<string?> Ids, string? VistaDocumento, bool Truncado);

/// <summary>
/// Motor de análisis: valida la consulta contra el catálogo, la ejecuta (y la del periodo de comparación), y
/// monta el árbol de filas con sus subtotales, la tabla dinámica por columnas, el orden y los N primeros.
/// </summary>
public sealed class MotorAnalisis
{
    private readonly IEjecutorAnalisis _ejecutor;
    private readonly IPermisosAnalisis _permisos;

    public MotorAnalisis(IEjecutorAnalisis ejecutor, IPermisosAnalisis permisos)
    {
        _ejecutor = ejecutor;
        _permisos = permisos;
    }

    public IReadOnlyList<DatasetDto> Catalogo() =>
        CatalogoDatasets.Todos.Where(_permisos.Puede).Select(d => new DatasetDto(
            d.Clave, d.Nombre, d.Descripcion,
            d.DimensionesCompletas.Select(x => new CampoDto(x.Clave, x.Nombre, x.Grupo, x.Grupo == "Fecha" ? TipoDato.Fecha : TipoDato.Texto)).ToList(),
            d.Medidas.Select(m => new CampoDto(m.Clave, m.Nombre, "Medidas", m.Tipo, m.Aditiva, m.Descripcion)).ToList(),
            d.FilasDefecto, d.MedidasDefecto, d.Detalle.Count > 0, d.VistaDocumento)).ToList();

    private Resultado<DatasetAnalisis> Dataset(string? clave) =>
        CatalogoDatasets.Buscar(clave) is { } ds && _permisos.Puede(ds)
            ? Resultado.Ok(ds)
            : Resultado.Fallo<DatasetAnalisis>(Error.NoEncontrado("analisis.dataset", $"No existe el conjunto de datos «{clave}» o no está contratado."));

    public async Task<Resultado<ResultadoAnalisis>> ConsultarAsync(ConsultaAnalisis consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        var ds = Dataset(consulta.Dataset);
        if (ds.EsFallo)
        {
            return Resultado.Fallo<ResultadoAnalisis>(ds.Error);
        }

        var plan = ConstructorSql.Validar(consulta, ds.Valor);
        if (plan.EsFallo)
        {
            return Resultado.Fallo<ResultadoAnalisis>(plan.Error);
        }

        var p = plan.Valor;
        var filas = await _ejecutor.EjecutarAsync(ConstructorSql.Agrupado(p, consulta.Desde, consulta.Hasta, consulta.Filtros, consulta.FiltrosMedida), ct).ConfigureAwait(false);
        var truncado = filas.Count > ConstructorSql.MaxFilasResultado;
        var actual = Leer(p, truncado ? filas.Take(ConstructorSql.MaxFilasResultado).ToList() : filas);

        (DateOnly Desde, DateOnly Hasta)? anterior = null;
        Dictionary<string, decimal?[]>? previos = null;
        if (!string.IsNullOrWhiteSpace(consulta.Comparar) && consulta.Desde is { } d && consulta.Hasta is { } h)
        {
            anterior = consulta.Comparar == "anio_anterior"
                ? (d.AddYears(-1), h.AddYears(-1))
                : (d.AddDays(-(h.DayNumber - d.DayNumber + 1)), d.AddDays(-1));
            // Las dimensiones de fecha del periodo anterior se calculan sobre la fecha desplazada al periodo actual:
            // así «marzo 2025» se compara con «marzo 2026» (y el día 1 del periodo anterior con el día 1 del actual).
            var desplazamiento = consulta.Comparar == "anio_anterior" ? "interval '1 year'" : $"interval '{h.DayNumber - d.DayNumber + 1} days'";
            var desplazado = ds.Valor with { ColumnaFecha = $"(({ds.Valor.ColumnaFecha}) + {desplazamiento})::date" };
            var sinColumna = p with
            {
                Columna = null,
                Filas = p.Filas.Select(f => f.Grupo == "Fecha" ? desplazado.BuscarDimension(f.Clave)! : f).ToList(),
            };
            var filasPrevias = await _ejecutor.EjecutarAsync(ConstructorSql.Agrupado(sinColumna, anterior.Value.Desde, anterior.Value.Hasta, consulta.Filtros, consulta.FiltrosMedida), ct).ConfigureAwait(false);
            previos = Leer(sinColumna, filasPrevias).Where(n => n.Columna is null).ToDictionary(n => n.Clave, n => n.Medidas);
        }

        // Valores de la columna (tabla dinámica), en orden.
        var valoresColumna = p.Columna is null
            ? []
            : actual.Where(n => n.ConColumna).Select(n => n.Columna).Distinct().OrderBy(v => v ?? "￿", StringComparer.Ordinal).ToList();
        if (valoresColumna.Count > ConstructorSql.MaxValoresColumna)
        {
            return Resultado.Fallo<ResultadoAnalisis>(Error.Validacion("analisis.columnas",
                $"La dimensión de columnas tiene {valoresColumna.Count} valores (máximo {ConstructorSql.MaxValoresColumna}): filtra o elige otra."));
        }

        var indiceColumna = valoresColumna.Select((v, i) => (v, i)).ToDictionary(x => x.v ?? "\u0000", x => x.i);
        var nodos = actual.Where(n => !n.ConColumna).ToDictionary(n => n.Clave);
        var celdas = new Dictionary<string, decimal?[][]>();
        foreach (var n in actual.Where(n => n.ConColumna))
        {
            if (!celdas.TryGetValue(n.Clave, out var c))
            {
                c = new decimal?[valoresColumna.Count][];
                celdas[n.Clave] = c;
            }

            c[indiceColumna[n.Columna ?? "\u0000"]] = n.Medidas;
        }

        // Orden: por la medida elegida (descendente por defecto) o, en dimensiones de fecha/código, por su valor.
        var orden = consulta.OrdenarPor is { } o ? p.Medidas.Select(m => m.Clave).ToList().IndexOf(o) : -1;
        var ordenarPorValor = orden < 0 && consulta.OrdenarPor is { } od && p.Filas.Any(x => x.Clave == od);
        var resultado = new List<FilaAnalisis>();
        var hijosDe = nodos.Values.Where(n => n.Nivel > 0).GroupBy(n => Nodo.ClaveDe(n.Nivel - 1, n.Claves.Take(n.Nivel - 1))).ToDictionary(g => g.Key, g => g.ToList());

        FilaAnalisis Fila(Nodo n, bool resto = false) => new(
            n.Nivel, n.Claves, n.Medidas,
            previos is null ? null : previos.TryGetValue(n.Clave, out var prev) ? prev : new decimal?[p.Medidas.Count],
            p.Columna is null ? null : (celdas.TryGetValue(n.Clave, out var c) ? c : new decimal?[valoresColumna.Count][]).Select(x => (IReadOnlyList<decimal?>)(x ?? new decimal?[p.Medidas.Count])).ToList(),
            resto);

        void Recorrer(Nodo padre)
        {
            var nivel = padre.Nivel + 1;
            if (nivel > p.Filas.Count)
            {
                return;
            }

            var hijos = hijosDe.GetValueOrDefault(padre.Clave) ?? [];
            var dimension = p.Filas[nivel - 1];
            IEnumerable<Nodo> ordenados;
            if (ordenarPorValor ? consulta.OrdenarPor == dimension.Clave : orden < 0 && dimension.Ordenable)
            {
                ordenados = hijos.OrderBy(n => n.Claves[^1] ?? "￿", StringComparer.Ordinal);
                if (ordenarPorValor && !consulta.Ascendente)
                {
                    ordenados = ordenados.Reverse();
                }
            }
            else
            {
                var k = orden < 0 ? 0 : orden;
                ordenados = consulta.Ascendente && orden >= 0
                    ? hijos.OrderBy(n => n.Medidas[k] ?? decimal.MaxValue).ThenBy(n => n.Claves[^1], StringComparer.CurrentCulture)
                    : hijos.OrderByDescending(n => n.Medidas[k] ?? decimal.MinValue).ThenBy(n => n.Claves[^1], StringComparer.CurrentCulture);
            }

            var lista = ordenados.ToList();
            if (nivel == 1 && consulta.Limite is { } limite && lista.Count > limite)
            {
                var resto = lista.Skip(limite).ToList();
                lista = lista.Take(limite).ToList();
                foreach (var n in lista)
                {
                    resultado.Add(Fila(n));
                    Recorrer(n);
                }

                // «Resto»: lo que no entra en los N primeros (solo en las medidas que se pueden sumar).
                var medidasResto = p.Medidas.Select((m, i) => m.Aditiva ? Suma(resto.Select(r => r.Medidas[i])) : null).ToArray();
                var anterioresResto = previos is null ? null : p.Medidas.Select((m, i) => m.Aditiva ? Suma(resto.Select(r => previos.TryGetValue(r.Clave, out var pr) ? pr[i] : null)) : null).ToArray();
                var celdasResto = p.Columna is null ? null : valoresColumna.Select((_, ci) => (IReadOnlyList<decimal?>)p.Medidas.Select((m, i) => m.Aditiva ? Suma(resto.Select(r => celdas.TryGetValue(r.Clave, out var c) ? c[ci]?[i] : null)) : null).ToArray()).ToList();
                resultado.Add(new FilaAnalisis(1, [$"Resto ({resto.Count})"], medidasResto, anterioresResto, celdasResto, true));
                return;
            }

            foreach (var n in lista)
            {
                resultado.Add(Fila(n));
                Recorrer(n);
            }
        }

        var raiz = nodos.GetValueOrDefault(Nodo.ClaveDe(0, [])) ?? new Nodo(0, [], new decimal?[p.Medidas.Count], null, false);
        resultado.Add(Fila(raiz));
        Recorrer(raiz);

        var titulo = $"{ds.Valor.Nombre}: {string.Join(" · ", p.Medidas.Select(m => m.Nombre))}"
            + (p.Filas.Count > 0 ? $" por {string.Join(", ", p.Filas.Select(f => f.Nombre.ToLower(CultureInfo.CurrentCulture)))}" : string.Empty)
            + (p.Columna is { } col ? $" y {col.Nombre.ToLower(CultureInfo.CurrentCulture)}" : string.Empty);
        return Resultado.Ok(new ResultadoAnalisis(
            ds.Valor.Clave, titulo, consulta.Desde, consulta.Hasta, anterior?.Desde, anterior?.Hasta,
            p.Filas.Select(f => new ColumnaResultado(f.Clave, f.Nombre, f.Grupo == "Fecha" ? TipoDato.Fecha : TipoDato.Texto)).ToList(),
            p.Medidas.Select(m => new ColumnaResultado(m.Clave, m.Nombre, m.Tipo, m.Aditiva)).ToList(),
            p.Columna is { } c2 ? new ColumnaResultado(c2.Clave, c2.Nombre, c2.Grupo == "Fecha" ? TipoDato.Fecha : TipoDato.Texto) : null,
            valoresColumna, resultado, truncado));
    }

    public async Task<Resultado<IReadOnlyList<ValorDimensionDto>>> ValoresAsync(string dataset, string dimension, string? texto, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var ds = Dataset(dataset);
        if (ds.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<ValorDimensionDto>>(ds.Error);
        }

        if (ds.Valor.BuscarDimension(dimension) is not { } dim)
        {
            return Resultado.Fallo<IReadOnlyList<ValorDimensionDto>>(Error.Validacion("analisis.dimension", $"La dimensión «{dimension}» no existe."));
        }

        var filas = await _ejecutor.EjecutarAsync(ConstructorSql.Valores(ds.Valor, dim, texto, desde, hasta, 200), ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<ValorDimensionDto>>(filas.Select(f => new ValorDimensionDto(f[0] as string, Decimal(f[1]) ?? 0m)).ToList());
    }

    public async Task<Resultado<DetalleAnalisisDto>> DetalleAsync(string dataset, IReadOnlyList<FiltroAnalisis>? filtros, DateOnly? desde, DateOnly? hasta, int limite, CancellationToken ct = default)
    {
        var ds = Dataset(dataset);
        if (ds.EsFallo)
        {
            return Resultado.Fallo<DetalleAnalisisDto>(ds.Error);
        }

        var validado = ConstructorSql.Validar(new ConsultaAnalisis(dataset, [], null, [ds.Valor.MedidasDefecto[0]], filtros, desde, hasta), ds.Valor);
        if (validado.EsFallo)
        {
            return Resultado.Fallo<DetalleAnalisisDto>(validado.Error);
        }

        limite = Math.Clamp(limite, 1, 5000);
        var filas = await _ejecutor.EjecutarAsync(ConstructorSql.Detalle(ds.Valor, desde, hasta, filtros, limite), ct).ConfigureAwait(false);
        var n = ds.Valor.Detalle.Count;
        var visibles = filas.Take(limite).ToList();
        return Resultado.Ok(new DetalleAnalisisDto(
            ds.Valor.Detalle.Select((c, i) => new ColumnaResultado("x" + i.ToString(CultureInfo.InvariantCulture), c.Nombre, c.Tipo)).ToList(),
            visibles.Select(f => (IReadOnlyList<object?>)f.Take(n).Select(Normalizar).ToList()).ToList(),
            visibles.Select(f => f[n] as string).ToList(),
            ds.Valor.VistaDocumento, filas.Count > limite));
    }

    private static object? Normalizar(object? v) => v switch
    {
        null or DBNull => null,
        DateTime dt => DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        decimal or int or long or short or double => Convert.ToDecimal(v, CultureInfo.InvariantCulture),
        _ => v.ToString(),
    };

    private static decimal? Suma(IEnumerable<decimal?> valores)
    {
        var lista = valores.Where(v => v is not null).ToList();
        return lista.Count == 0 ? null : lista.Sum();
    }

    private static decimal? Decimal(object? v) => v is null or DBNull ? null : Convert.ToDecimal(v, CultureInfo.InvariantCulture);

    /// <summary>Un grupo del resultado: su nivel (dimensiones de fila no agregadas), sus claves y sus medidas.</summary>
    private sealed record Nodo(int Nivel, IReadOnlyList<string?> Claves, decimal?[] Medidas, string? Columna, bool ConColumna)
    {
        public string Clave => ClaveDe(Nivel, Claves);

        public static string ClaveDe(int nivel, IEnumerable<string?> claves) =>
            nivel.ToString(CultureInfo.InvariantCulture) + "|" + string.Join("\u001f", claves.Select(c => c ?? "\u0000"));
    }

    /// <summary>
    /// Lee las filas del SELECT agrupado: d0..dn-1, [c], m0..mk, g. En la máscara g (GROUPING), el bit más alto es
    /// d0 y el más bajo la columna: un 1 indica que está agregada. Los niveles son prefijos de las dimensiones.
    /// </summary>
    private static List<Nodo> Leer(ConstructorSql.Plan p, IReadOnlyList<object?[]> filas)
    {
        var nd = p.Filas.Count;
        var conCol = p.Columna is not null;
        var inicioMedidas = nd + (conCol ? 1 : 0);
        var bits = nd + (conCol ? 1 : 0);
        var lista = new List<Nodo>(filas.Count);
        foreach (var f in filas)
        {
            var g = Convert.ToInt32(f[inicioMedidas + p.Medidas.Count], CultureInfo.InvariantCulture);
            var nivel = 0;
            for (var i = 0; i < nd; i++)
            {
                if ((g & (1 << (bits - 1 - i))) == 0)
                {
                    nivel = i + 1;
                }
            }

            var columnaAgregada = !conCol || (g & 1) == 1;
            var claves = Enumerable.Range(0, nivel).Select(i => f[i] as string).ToList();
            var medidas = Enumerable.Range(0, p.Medidas.Count).Select(i => Decimal(f[inicioMedidas + i])).ToArray();
            lista.Add(new Nodo(nivel, claves, medidas, columnaAgregada ? null : f[nd] as string, !columnaAgregada));
        }

        return lista;
    }
}
