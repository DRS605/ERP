using System.Globalization;
using System.Text;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Analisis.Aplicacion;

/// <summary>
/// Filtro sobre una dimensión. Operadores: <c>en</c> (alguno de los valores), <c>no_en</c>, <c>contiene</c>,
/// <c>empieza</c>, <c>vacio</c>, <c>no_vacio</c>, <c>desde</c> / <c>hasta</c> (comparación de texto: vale para
/// fechas y periodos como «2026-03»).
/// </summary>
public sealed record FiltroAnalisis(string Dimension, string Operador, IReadOnlyList<string>? Valores = null);

/// <summary>Filtro sobre el resultado de una medida (p. ej. clientes con más de 10.000 € de base): <c>mayor</c>, <c>menor</c>, <c>entre</c>.</summary>
public sealed record FiltroMedida(string Medida, string Operador, decimal Valor, decimal? Hasta = null);

/// <summary>
/// Lo que se quiere ver: dimensiones en filas (jerarquía, con subtotales), opcionalmente una dimensión en columnas
/// (tabla dinámica), las medidas, filtros, periodo, comparación con el periodo anterior y los N primeros.
/// </summary>
public sealed record ConsultaAnalisis(
    string Dataset,
    IReadOnlyList<string>? Filas = null,
    string? Columna = null,
    IReadOnlyList<string>? Medidas = null,
    IReadOnlyList<FiltroAnalisis>? Filtros = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    string? Comparar = null,
    string? OrdenarPor = null,
    bool Ascendente = false,
    int? Limite = null,
    IReadOnlyList<FiltroMedida>? FiltrosMedida = null);

/// <summary>Cabecera de una columna del resultado.</summary>
public sealed record ColumnaResultado(string Clave, string Nombre, TipoDato Tipo, bool Aditiva = true);

/// <summary>
/// Fila del resultado: <see cref="Nivel"/> 0 es el total general; 1..n, los subtotales de cada dimensión (n, el
/// detalle). <see cref="Claves"/> tiene los valores de las dimensiones hasta su nivel. <see cref="Valores"/> son las
/// medidas (el total de la fila si hay columnas), <see cref="Celdas"/> las medidas por valor de la columna, y
/// <see cref="Anteriores"/> las del periodo de comparación.
/// </summary>
public sealed record FilaAnalisis(
    int Nivel,
    IReadOnlyList<string?> Claves,
    IReadOnlyList<decimal?> Valores,
    IReadOnlyList<decimal?>? Anteriores = null,
    IReadOnlyList<IReadOnlyList<decimal?>>? Celdas = null,
    bool Resto = false);

public sealed record ResultadoAnalisis(
    string Dataset,
    string Titulo,
    DateOnly? Desde,
    DateOnly? Hasta,
    DateOnly? DesdeAnterior,
    DateOnly? HastaAnterior,
    IReadOnlyList<ColumnaResultado> Dimensiones,
    IReadOnlyList<ColumnaResultado> Medidas,
    ColumnaResultado? Columna,
    IReadOnlyList<string?> ValoresColumna,
    IReadOnlyList<FilaAnalisis> Filas,
    bool Truncado);

/// <summary>Sentencia SQL con sus parámetros (los valores del usuario nunca van en el texto).</summary>
public sealed record SentenciaSql(string Texto, IReadOnlyList<KeyValuePair<string, object>> Parametros);

/// <summary>
/// Construye el SQL de una consulta: SELECT de las dimensiones (como texto) y medidas, con GROUPING SETS para los
/// subtotales de cada nivel (y de la columna, si la hay), sobre el conjunto de datos y sus filtros.
/// </summary>
public static class ConstructorSql
{
    public const int MaxDimensionesFila = 4;
    public const int MaxMedidas = 12;
    public const int MaxValoresColumna = 60;
    public const int MaxFilasResultado = 20000;

    /// <summary>Consulta ya validada contra el conjunto de datos.</summary>
    internal sealed record Plan(DatasetAnalisis Dataset, IReadOnlyList<Dimension> Filas, Dimension? Columna, IReadOnlyList<Medida> Medidas);

    internal static Resultado<Plan> Validar(ConsultaAnalisis consulta, DatasetAnalisis dataset)
    {
        var filas = new List<Dimension>();
        foreach (var clave in (consulta.Filas is { Count: > 0 } f ? f : dataset.FilasDefecto).Distinct())
        {
            if (dataset.BuscarDimension(clave) is not { } d)
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.dimension", $"La dimensión «{clave}» no existe en {dataset.Nombre}."));
            }

            filas.Add(d);
        }

        if (filas.Count > MaxDimensionesFila)
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.dimensiones", $"Como mucho {MaxDimensionesFila} dimensiones en filas."));
        }

        Dimension? columna = null;
        if (!string.IsNullOrWhiteSpace(consulta.Columna))
        {
            columna = dataset.BuscarDimension(consulta.Columna);
            if (columna is null)
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.dimension", $"La dimensión «{consulta.Columna}» no existe en {dataset.Nombre}."));
            }

            if (filas.Any(d => d.Clave == columna.Clave))
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.columna", "La dimensión de columnas no puede estar también en filas."));
            }
        }

        var medidas = new List<Medida>();
        foreach (var clave in (consulta.Medidas is { Count: > 0 } m ? m : dataset.MedidasDefecto).Distinct())
        {
            if (dataset.BuscarMedida(clave) is not { } medida)
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.medida", $"La medida «{clave}» no existe en {dataset.Nombre}."));
            }

            medidas.Add(medida);
        }

        if (medidas.Count is 0 or > MaxMedidas)
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.medidas", $"Elige entre 1 y {MaxMedidas} medidas."));
        }

        foreach (var filtro in consulta.Filtros ?? [])
        {
            if (dataset.BuscarDimension(filtro.Dimension) is null)
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.filtro", $"No se puede filtrar por «{filtro.Dimension}»."));
            }

            if (!Operadores.Contains(filtro.Operador))
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.filtro", $"Operador de filtro no válido: «{filtro.Operador}»."));
            }

            if (filtro.Operador is not ("vacio" or "no_vacio") && (filtro.Valores is null || filtro.Valores.Count == 0 || filtro.Valores.Count > 500))
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.filtro", $"El filtro de «{filtro.Dimension}» necesita entre 1 y 500 valores."));
            }
        }

        foreach (var fm in consulta.FiltrosMedida ?? [])
        {
            if (dataset.BuscarMedida(fm.Medida) is null || fm.Operador is not ("mayor" or "menor" or "entre") || (fm.Operador == "entre" && fm.Hasta is null))
            {
                return Resultado.Fallo<Plan>(Error.Validacion("analisis.filtro_medida", $"Filtro de medida no válido: «{fm.Medida}»."));
            }
        }

        if (consulta.Desde is { } desde && consulta.Hasta is { } hasta && desde > hasta)
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.periodo", "La fecha inicial es posterior a la final."));
        }

        if (consulta.Comparar is not (null or "" or "anio_anterior" or "periodo_anterior"))
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.comparar", "Se compara con «anio_anterior» o «periodo_anterior»."));
        }

        if (!string.IsNullOrWhiteSpace(consulta.Comparar) && (consulta.Desde is null || consulta.Hasta is null))
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.comparar", "Para comparar hay que indicar el periodo (desde y hasta)."));
        }

        if (consulta.Limite is < 1 or > 1000)
        {
            return Resultado.Fallo<Plan>(Error.Validacion("analisis.limite", "El límite va de 1 a 1.000."));
        }

        return Resultado.Ok(new Plan(dataset, filas, columna, medidas));
    }

    private static readonly HashSet<string> Operadores = ["en", "no_en", "contiene", "empieza", "vacio", "no_vacio", "desde", "hasta"];

    /// <summary>Condiciones WHERE (base, periodo y filtros de dimensión) con sus parámetros.</summary>
    internal static string Donde(DatasetAnalisis ds, DateOnly? desde, DateOnly? hasta, IReadOnlyList<FiltroAnalisis>? filtros, List<KeyValuePair<string, object>> p)
    {
        var w = new StringBuilder($"({ds.CondicionBase})");
        if (desde is { } d)
        {
            w.Append(CultureInfo.InvariantCulture, $" AND {ds.ColumnaFecha} >= {Param(p, d)}");
        }

        if (hasta is { } h)
        {
            w.Append(CultureInfo.InvariantCulture, $" AND {ds.ColumnaFecha} <= {Param(p, h)}");
        }

        foreach (var f in filtros ?? [])
        {
            var expr = $"(({ds.BuscarDimension(f.Dimension)!.Sql})::text)";
            var valores = (f.Valores ?? []).Select(v => v ?? string.Empty).ToArray();
            var sinValor = valores.Any(v => v.Length == 0);
            var conValor = valores.Where(v => v.Length > 0).ToArray();
            w.Append(" AND ");
            switch (f.Operador)
            {
                case "en":
                    w.Append(sinValor
                        ? $"({expr} IS NULL OR {expr} = '' OR {expr} = ANY({Param(p, conValor)}))"
                        : $"{expr} = ANY({Param(p, conValor)})");
                    break;
                case "no_en":
                    w.Append(sinValor
                        ? $"({expr} IS NOT NULL AND {expr} <> '' AND NOT ({expr} = ANY({Param(p, conValor)})))"
                        : $"({expr} IS NULL OR NOT ({expr} = ANY({Param(p, conValor)})))");
                    break;
                case "contiene":
                    w.Append(CultureInfo.InvariantCulture, $"({string.Join(" OR ", conValor.Select(v => $"{expr} ILIKE {Param(p, "%" + Escapar(v) + "%")}"))})");
                    break;
                case "empieza":
                    w.Append(CultureInfo.InvariantCulture, $"({string.Join(" OR ", conValor.Select(v => $"{expr} ILIKE {Param(p, Escapar(v) + "%")}"))})");
                    break;
                case "vacio":
                    w.Append(CultureInfo.InvariantCulture, $"({expr} IS NULL OR {expr} = '')");
                    break;
                case "no_vacio":
                    w.Append(CultureInfo.InvariantCulture, $"({expr} IS NOT NULL AND {expr} <> '')");
                    break;
                case "desde":
                    w.Append(CultureInfo.InvariantCulture, $"{expr} >= {Param(p, valores[0])}");
                    break;
                default:
                    w.Append(CultureInfo.InvariantCulture, $"{expr} <= {Param(p, valores[0])}");
                    break;
            }
        }

        return w.ToString();
    }

    /// <summary>
    /// SELECT agrupado. Columnas: d0..dn (texto), c (columna), m0..mk y g (máscara de GROUPING: qué dimensiones
    /// están agregadas). Con columnas se piden los niveles de fila × (con columna, sin columna).
    /// </summary>
    internal static SentenciaSql Agrupado(Plan plan, DateOnly? desde, DateOnly? hasta, IReadOnlyList<FiltroAnalisis>? filtros, IReadOnlyList<FiltroMedida>? filtrosMedida)
    {
        var p = new List<KeyValuePair<string, object>>();
        var ds = plan.Dataset;
        var dims = plan.Filas.Select((d, i) => $"(({d.Sql})::text) AS d{i}").ToList();
        var agrupables = plan.Filas.Select(d => $"(({d.Sql})::text)").ToList();
        if (plan.Columna is { } col)
        {
            dims.Add($"(({col.Sql})::text) AS c");
        }

        var medidas = plan.Medidas.Select((m, i) => $"({m.Sql})::numeric AS m{i}").ToList();
        var expresionColumna = plan.Columna is { } cc ? $"(({cc.Sql})::text)" : null;
        var todas = agrupables.Concat(expresionColumna is null ? [] : [expresionColumna]).ToList();
        var mascara = todas.Count == 0 ? "0" : $"GROUPING({string.Join(", ", todas)})";

        // Niveles: (d0..dn), (d0..dn-1), …, () — y cada uno con y sin la columna.
        var niveles = Enumerable.Range(0, plan.Filas.Count + 1).Reverse().Select(n => agrupables.Take(n).ToList()).ToList();
        var conjuntos = plan.Columna is null
            ? niveles
            : niveles.SelectMany(n => new[] { n.Append(expresionColumna!).ToList(), n }).ToList();
        var grouping = todas.Count == 0 ? string.Empty : $" GROUP BY GROUPING SETS ({string.Join(", ", conjuntos.Select(c => $"({string.Join(", ", c)})"))})";

        var donde = Donde(ds, desde, hasta, filtros, p) + FiltrosMedidaSql(plan, desde, hasta, filtros, filtrosMedida, p);
        var sql = $"""
            SELECT {string.Join(", ", dims.Concat(medidas).Append($"{mascara} AS g"))}
            FROM {ds.Desde}
            WHERE {donde}{grouping}
            LIMIT {MaxFilasResultado + 1}
            """;
        return new SentenciaSql(sql, p);
    }

    /// <summary>
    /// Filtros de medida: se quedan solo los registros cuyo grupo de detalle (todas las dimensiones de fila) cumple la
    /// condición, así los subtotales y el total son los de lo que se ve (p. ej. solo clientes de más de 10.000 €).
    /// </summary>
    private static string FiltrosMedidaSql(Plan plan, DateOnly? desde, DateOnly? hasta, IReadOnlyList<FiltroAnalisis>? filtros, IReadOnlyList<FiltroMedida>? filtrosMedida, List<KeyValuePair<string, object>> p)
    {
        if (filtrosMedida is not { Count: > 0 } || plan.Filas.Count == 0)
        {
            return string.Empty;
        }

        var ds = plan.Dataset;
        var claves = plan.Filas.Select(d => $"coalesce((({d.Sql})::text), '~sin valor~')").ToList();
        var condiciones = filtrosMedida.Select(f =>
        {
            var m = $"({ds.BuscarMedida(f.Medida)!.Sql})";
            return f.Operador switch
            {
                "mayor" => $"{m} > {Param(p, f.Valor)}",
                "menor" => $"{m} < {Param(p, f.Valor)}",
                _ => $"{m} BETWEEN {Param(p, f.Valor)} AND {Param(p, f.Hasta!.Value)}",
            };
        }).ToList();
        var interior = $"SELECT {string.Join(", ", claves)} FROM {ds.Desde} WHERE {Donde(ds, desde, hasta, filtros, p)} GROUP BY {string.Join(", ", claves.Select((_, i) => (i + 1).ToString(CultureInfo.InvariantCulture)))} HAVING {string.Join(" AND ", condiciones)}";
        return $" AND ({string.Join(", ", claves)}) IN ({interior})";
    }

    /// <summary>Valores distintos de una dimensión (para el selector de filtros), los más frecuentes primero.</summary>
    internal static SentenciaSql Valores(DatasetAnalisis ds, Dimension dim, string? texto, DateOnly? desde, DateOnly? hasta, int limite)
    {
        var p = new List<KeyValuePair<string, object>>();
        var expr = $"(({dim.Sql})::text)";
        var donde = Donde(ds, desde, hasta, null, p);
        if (!string.IsNullOrWhiteSpace(texto))
        {
            donde += $" AND {expr} ILIKE {Param(p, "%" + Escapar(texto.Trim()) + "%")}";
        }

        var orden = dim.Ordenable ? "v" : "count(*) DESC, v";
        return new SentenciaSql($"SELECT {expr} AS v, count(*)::numeric AS n FROM {ds.Desde} WHERE {donde} GROUP BY 1 ORDER BY {orden} LIMIT {limite}", p);
    }

    /// <summary>Registros de detalle que forman una cifra (profundizar), con el enlace al documento si lo hay.</summary>
    internal static SentenciaSql Detalle(DatasetAnalisis ds, DateOnly? desde, DateOnly? hasta, IReadOnlyList<FiltroAnalisis>? filtros, int limite)
    {
        var p = new List<KeyValuePair<string, object>>();
        var columnas = ds.Detalle.Select((c, i) => $"{c.Sql} AS x{i}").ToList();
        columnas.Add(ds.IdDocumentoSql is null ? "NULL::text AS id" : $"({ds.IdDocumentoSql})::text AS id");
        return new SentenciaSql($"SELECT {string.Join(", ", columnas)} FROM {ds.Desde} WHERE {Donde(ds, desde, hasta, filtros, p)} ORDER BY {ds.ColumnaFecha} DESC LIMIT {limite + 1}", p);
    }

    private static string Param(List<KeyValuePair<string, object>> p, object valor)
    {
        var nombre = $"p{p.Count.ToString(CultureInfo.InvariantCulture)}";
        p.Add(new(nombre, valor));
        return "@" + nombre;
    }

    private static string Escapar(string v) => v.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);
}
