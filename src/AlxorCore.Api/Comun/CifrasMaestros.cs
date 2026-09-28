using System.Globalization;
using AlxorCore.Analisis.Aplicacion;
using AlxorCore.Nucleo.Consultas;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Página de un maestro (clientes, proveedores, artículos) con sus cifras: las de cada fila de la página y los
/// totales de <b>todo</b> el filtro, calculados en el servidor. Conserva los campos de <see cref="PaginaResultado{T}"/>.
/// </summary>
public sealed record PaginaConCifras<T>(
    IReadOnlyList<T> Elementos, int Total, int Pagina, int TamanoPagina, int TotalPaginas, bool HayMas,
    int Ejercicio, IReadOnlyDictionary<string, decimal> Totales, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>> Cifras)
{
    public static PaginaConCifras<T> Desde(PaginaResultado<T> pagina, int ejercicio, (IReadOnlyDictionary<string, decimal> Totales, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>> Cifras) c)
    {
        ArgumentNullException.ThrowIfNull(pagina);
        return new(pagina.Elementos, pagina.Total, pagina.Pagina, pagina.TamanoPagina, pagina.TotalPaginas, pagina.HayMas, ejercicio, c.Totales, c.Cifras);
    }
}

/// <summary>
/// Cifras de los maestros para sus listados: facturado, pendiente, vencido y anticipos por aplicar de clientes; comprado, pendiente y
/// vencido de proveedores; stock, valor del stock y ventas de artículos. SQL fijo por la conexión de solo lectura
/// del análisis (con la RLS de la empresa): el pendiente es el total menos lo cobrado o pagado en Tesorería, como en
/// las pantallas de cobros y pagos, y solo de documentos vivos (sin anulados ni sustituidos).
/// </summary>
public sealed class CifrasMaestros
{
    private readonly IEjecutorAnalisis _ejecutor;

    public CifrasMaestros(IEjecutorAnalisis ejecutor) => _ejecutor = ejecutor;

    public static readonly string[] CamposClientes = ["facturas", "facturado", "pendiente", "vencido", "anticipos"];
    public static readonly string[] CamposProveedores = ["facturas", "comprado", "pendiente", "vencido"];
    public static readonly string[] CamposArticulos = ["stock", "valorStock", "unidadesVendidas", "ventas"];

    private const string Clientes = """
        WITH ids AS (SELECT unnest(@ids) AS id),
        fac AS (
            SELECT f.cliente_id AS id, f.base_imponible, f.fecha_emision, coalesce(f.fecha_vencimiento, f.fecha_emision) AS vto,
                   greatest(f.total - coalesce((SELECT sum(m.importe) FROM tesoreria.movimiento m
                                                WHERE m.tipo_documento = 'Factura' AND m.documento_id = f.id), 0), 0) AS pendiente
            FROM facturacion.factura f
            WHERE f.cliente_id = ANY(@ids) AND f.estado NOT IN ('Anulada', 'Rectificada')),
        sf AS (
            SELECT id,
                   count(*) FILTER (WHERE fecha_emision BETWEEN @desde AND @hasta)::numeric AS facturas,
                   coalesce(sum(base_imponible) FILTER (WHERE fecha_emision BETWEEN @desde AND @hasta), 0) AS facturado,
                   coalesce(sum(pendiente), 0) AS pendiente,
                   coalesce(sum(pendiente) FILTER (WHERE vto < @hoy), 0) AS vencido
            FROM fac GROUP BY id),
        an AS (
            SELECT a.cliente_id AS id, sum(a.importe - coalesce((SELECT sum(x.importe) FROM tesoreria.aplicacion_anticipo x WHERE x.anticipo_id = a.id), 0)) AS disponible
            FROM tesoreria.anticipo a
            WHERE a.cliente_id = ANY(@ids) AND a.anulado_en IS NULL
            GROUP BY a.cliente_id)
        SELECT ids.id::text, coalesce(sf.facturas, 0), coalesce(sf.facturado, 0), coalesce(sf.pendiente, 0), coalesce(sf.vencido, 0), coalesce(an.disponible, 0)
        FROM ids LEFT JOIN sf ON sf.id = ids.id LEFT JOIN an ON an.id = ids.id
        WHERE sf.id IS NOT NULL OR an.id IS NOT NULL
        """;

    // Vencido de un gasto: lo que ya debería estar pagado según sus vencimientos, menos lo pagado (entre 0 y el pendiente).
    private const string Proveedores = """
        WITH gas AS (
            SELECT g.proveedor_id AS id, g.base_imponible, g.fecha, g.total,
                   coalesce((SELECT sum(m.importe) FROM tesoreria.movimiento m WHERE m.tipo_documento = 'Gasto' AND m.documento_id = g.id), 0) AS pagado,
                   coalesce((SELECT sum(v.importe) FROM gastos.vencimiento_gasto v WHERE v.gasto_id = g.id AND v.fecha < @hoy), 0) AS exigible
            FROM gastos.gasto g
            WHERE g.proveedor_id = ANY(@ids) AND g.estado = 'Registrado')
        SELECT id::text,
               count(*) FILTER (WHERE fecha BETWEEN @desde AND @hasta)::numeric,
               coalesce(sum(base_imponible) FILTER (WHERE fecha BETWEEN @desde AND @hasta), 0),
               coalesce(sum(greatest(total - pagado, 0)), 0),
               coalesce(sum(least(greatest(total - pagado, 0), greatest(exigible - pagado, 0))), 0)
        FROM gas GROUP BY id
        """;

    private const string Articulos = """
        WITH ids AS (SELECT unnest(@ids) AS id),
        ven AS (
            SELECT l.producto_id AS id, sum(l.cantidad) AS unidades, sum(l.base) AS base
            FROM facturacion.linea_factura l JOIN facturacion.factura f ON f.id = l.factura_id
            WHERE l.producto_id = ANY(@ids) AND f.estado NOT IN ('Anulada', 'Rectificada') AND f.fecha_emision BETWEEN @desde AND @hasta
            GROUP BY l.producto_id)
        SELECT ids.id::text,
               coalesce(e.cantidad, 0),
               round(coalesce(e.cantidad, 0) * coalesce(p.precio_compra, 0), 2),
               coalesce(ven.unidades, 0),
               coalesce(ven.base, 0)
        FROM ids
        JOIN catalogo.producto p ON p.id = ids.id
        LEFT JOIN catalogo.existencia_simple e ON e.producto_id = ids.id
        LEFT JOIN ven ON ven.id = ids.id
        """;

    public Task<(IReadOnlyDictionary<string, decimal> Totales, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>> Cifras)> ClientesAsync(
        IReadOnlyList<Guid> todos, IEnumerable<Guid> pagina, int ejercicio, CancellationToken ct) => CalcularAsync(Clientes, CamposClientes, todos, pagina, ejercicio, ct);

    public Task<(IReadOnlyDictionary<string, decimal> Totales, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>> Cifras)> ProveedoresAsync(
        IReadOnlyList<Guid> todos, IEnumerable<Guid> pagina, int ejercicio, CancellationToken ct) => CalcularAsync(Proveedores, CamposProveedores, todos, pagina, ejercicio, ct);

    public Task<(IReadOnlyDictionary<string, decimal> Totales, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>> Cifras)> ArticulosAsync(
        IReadOnlyList<Guid> todos, IEnumerable<Guid> pagina, int ejercicio, CancellationToken ct) => CalcularAsync(Articulos, CamposArticulos, todos, pagina, ejercicio, ct);

    /// <summary>Ejercicio pedido o, si no es válido, el actual.</summary>
    public static int Ejercicio(int? ejercicio) => ejercicio is >= 2000 and <= 2100 ? ejercicio.Value : DateTime.Today.Year;

    private async Task<(IReadOnlyDictionary<string, decimal>, IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, decimal>>)> CalcularAsync(
        string sql, string[] campos, IReadOnlyList<Guid> todos, IEnumerable<Guid> pagina, int ejercicio, CancellationToken ct)
    {
        var totales = campos.ToDictionary(c => c, _ => 0m, StringComparer.Ordinal);
        var cifras = new Dictionary<Guid, IReadOnlyDictionary<string, decimal>>();
        if (todos.Count > 0)
        {
            var sentencia = new SentenciaSql(sql,
            [
                new("ids", todos.ToArray()),
                new("desde", new DateOnly(ejercicio, 1, 1)),
                new("hasta", new DateOnly(ejercicio, 12, 31)),
                new("hoy", DateOnly.FromDateTime(DateTime.Today)),
            ]);
            var enPagina = pagina.ToHashSet();
            foreach (var fila in await _ejecutor.EjecutarAsync(sentencia, ct).ConfigureAwait(false))
            {
                var valores = campos.Select((_, i) => Convert.ToDecimal(fila[i + 1] ?? 0m, CultureInfo.InvariantCulture)).ToArray();
                for (var i = 0; i < campos.Length; i++)
                {
                    totales[campos[i]] += valores[i];
                }

                var id = Guid.Parse((string)fila[0]!);
                if (enPagina.Contains(id))
                {
                    cifras[id] = campos.Select((c, i) => (c, valores[i])).ToDictionary(x => x.c, x => x.Item2, StringComparer.Ordinal);
                }
            }
        }

        totales["registros"] = todos.Count;
        return (totales, cifras);
    }
}
