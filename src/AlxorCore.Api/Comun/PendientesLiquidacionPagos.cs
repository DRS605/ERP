using System.Globalization;
using AlxorCore.Analisis.Aplicacion;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Lo pendiente de la liquidación de pagos, con SQL fijo por la conexión de solo lectura del análisis (con la RLS de la
/// empresa y del grupo). El pendiente es el total menos lo pagado o cobrado en Tesorería. Entran solo facturas vivas
/// (registradas o emitidas, con total positivo) hasta la fecha de corte, y no las que ya van en una remesa viva. El
/// cliente «del mismo NIF» es la única ficha de cliente del grupo con el NIF del proveedor (sin espacios, guiones ni puntos).
/// </summary>
public sealed class PendientesLiquidacionPagos : IPendientesLiquidacionPagos
{
    private readonly IEjecutorAnalisis _ejecutor;

    public PendientesLiquidacionPagos(IEjecutorAnalisis ejecutor) => _ejecutor = ejecutor;

    // Además de la RLS, cada consulta se limita a la empresa y al grupo activos (la conexión de análisis puede tener un rol
    // que no la aplique, como el superusuario de desarrollo).
    private const string Empresa = "NULLIF(current_setting('app.empresa_actual', true), '')::uuid";
    private const string Grupo = "NULLIF(current_setting('app.grupo_actual', true), '')::uuid";

    private static string Nif(string columna) => $"upper(regexp_replace(coalesce({columna}, ''), '[^A-Za-z0-9]', '', 'g'))";

    private static readonly string GastosPendientes = $"""
        SELECT g.id, g.proveedor_id, coalesce(nullif(g.numero_factura, ''), g.concepto) AS documento, g.fecha, g.total,
               g.total - coalesce((SELECT sum(m.importe) FROM tesoreria.movimiento m WHERE m.tipo_documento = 'Gasto' AND m.documento_id = g.id), 0) AS pendiente
        FROM gastos.gasto g
        WHERE g.empresa_id = {Empresa} AND g.estado = 'Registrado' AND g.proveedor_id IS NOT NULL AND g.total > 0 AND g.fecha <= @hasta
          AND NOT EXISTS (SELECT 1 FROM tesoreria.linea_remesa lr WHERE lr.tipo_documento = 'Gasto' AND lr.documento_id = g.id AND lr.viva)
        """;

    private static readonly string FacturasPendientes = $"""
        SELECT f.id, f.cliente_id, f.numero_completo AS documento, f.fecha_emision AS fecha, f.total,
               f.total - coalesce((SELECT sum(m.importe) FROM tesoreria.movimiento m WHERE m.tipo_documento = 'Factura' AND m.documento_id = f.id), 0) AS pendiente
        FROM facturacion.factura f
        WHERE f.empresa_id = {Empresa} AND f.estado = 'Emitida' AND f.total > 0 AND f.fecha_emision <= @hasta
          AND NOT EXISTS (SELECT 1 FROM tesoreria.linea_remesa lr WHERE lr.tipo_documento = 'Factura' AND lr.documento_id = f.id AND lr.viva)
        """;

    private static string ClienteDe(string proveedor) => $"""
        (SELECT CASE WHEN count(*) = 1 THEN min(c.id::text) END FROM terceros.cliente c
         WHERE c.grupo_id = {Grupo} AND {Nif("c.nif_fiscal")} = {Nif(proveedor + ".nif_fiscal")} AND {Nif(proveedor + ".nif_fiscal")} <> '')
        """;

    public async Task<IReadOnlyList<DocumentoPendienteDto>> GastosAsync(Guid proveedorId, DateOnly hasta, CancellationToken ct = default) =>
        await DocumentosAsync($"SELECT id::text, documento, fecha, total, pendiente FROM ({GastosPendientes}) x WHERE proveedor_id = @tercero AND pendiente > 0 ORDER BY fecha, documento",
            proveedorId, hasta, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DocumentoPendienteDto>> FacturasAsync(Guid clienteId, DateOnly hasta, CancellationToken ct = default) =>
        await DocumentosAsync($"SELECT id::text, documento, fecha, total, pendiente FROM ({FacturasPendientes}) x WHERE cliente_id = @tercero AND pendiente > 0 ORDER BY fecha, documento",
            clienteId, hasta, ct).ConfigureAwait(false);

    public async Task<Guid?> ClienteMismoNifAsync(Guid proveedorId, CancellationToken ct = default)
    {
        var filas = await _ejecutor.EjecutarAsync(new SentenciaSql($"SELECT {ClienteDe("p")} FROM terceros.proveedor p WHERE p.id = @proveedor AND p.grupo_id = {Grupo}",
            [new("proveedor", proveedorId)]), ct).ConfigureAwait(false);
        return filas.Count == 1 && filas[0][0] is string s ? Guid.Parse(s) : null;
    }

    public async Task<IReadOnlyList<ProveedorPendienteDto>> ProveedoresAsync(DateOnly hasta, bool soloAgricultores, CancellationToken ct = default)
    {
        var sql = $"""
            WITH gas AS (
                SELECT proveedor_id, count(*)::int AS facturas, sum(pendiente) AS pendiente
                FROM ({GastosPendientes}) x WHERE pendiente > 0 GROUP BY proveedor_id),
            fac AS (
                SELECT cliente_id, sum(pendiente) AS pendiente FROM ({FacturasPendientes}) x WHERE pendiente > 0 GROUP BY cliente_id),
            ent AS (
                SELECT e.proveedor_id, sum(e.importe - coalesce((SELECT sum(c.importe) FROM tesoreria.cancelacion_entrega_cuenta c WHERE c.entrega_id = e.id), 0)) AS pendiente
                FROM tesoreria.entrega_cuenta_proveedor e WHERE e.empresa_id = {Empresa} AND e.anulada_en IS NULL AND e.fecha <= @hasta GROUP BY e.proveedor_id),
            base AS (
                SELECT p.id, p.nombre, p.nif_fiscal, p.iban,
                       EXISTS (SELECT 1 FROM agro.agricultor a WHERE a.proveedor_id = p.id AND a.empresa_id = {Empresa}) AS agricultor,
                       gas.facturas, gas.pendiente, {ClienteDe("p")} AS cliente_id
                FROM gas JOIN terceros.proveedor p ON p.id = gas.proveedor_id AND p.grupo_id = {Grupo})
            SELECT b.id::text, b.nombre, b.nif_fiscal, b.agricultor, b.facturas, b.pendiente, b.cliente_id, c.nombre, coalesce(fac.pendiente, 0),
                   coalesce(ent.pendiente, 0), coalesce(b.iban, '') <> ''
            FROM base b
            LEFT JOIN terceros.cliente c ON c.id::text = b.cliente_id
            LEFT JOIN fac ON fac.cliente_id::text = b.cliente_id
            LEFT JOIN ent ON ent.proveedor_id = b.id
            WHERE (NOT @agricultores OR b.agricultor)
            ORDER BY b.nombre
            """;
        var filas = await _ejecutor.EjecutarAsync(new SentenciaSql(sql, [new("hasta", hasta), new("agricultores", soloAgricultores)]), ct).ConfigureAwait(false);
        return filas.Select(f => new ProveedorPendienteDto(Guid.Parse((string)f[0]!), (string)f[1]!, f[2] as string, (bool)f[3]!, Convert.ToInt32(f[4], CultureInfo.InvariantCulture),
            Dec(f[5]), f[6] is string c ? Guid.Parse(c) : null, f[7] as string, Dec(f[8]), Dec(f[9]), (bool)f[10]!)).ToList();
    }

    private async Task<IReadOnlyList<DocumentoPendienteDto>> DocumentosAsync(string sql, Guid tercero, DateOnly hasta, CancellationToken ct)
    {
        var filas = await _ejecutor.EjecutarAsync(new SentenciaSql(sql, [new("tercero", tercero), new("hasta", hasta)]), ct).ConfigureAwait(false);
        return filas.Select(f => new DocumentoPendienteDto(Guid.Parse((string)f[0]!), (string)f[1]!, Fecha(f[2]), Dec(f[3]), Dec(f[4]))).ToList();
    }

    private static decimal Dec(object? v) => v is null ? 0m : Math.Round(Convert.ToDecimal(v, CultureInfo.InvariantCulture), 2);

    private static DateOnly Fecha(object? v) => v switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => throw new InvalidOperationException("Fecha no válida."),
    };
}
