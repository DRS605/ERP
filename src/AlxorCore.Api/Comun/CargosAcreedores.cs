using System.Globalization;
using AlxorCore.Analisis.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Un cargo con acreedor en un documento (pendiente o no de liquidar).</summary>
public sealed record CargoAcreedorDto(string Origen, Guid DocumentoId, string Numero, DateOnly Fecha, string Tercero, int LineaOrden, string Linea,
    Guid ConceptoId, string Codigo, string Concepto, string Efecto, decimal Importe, Guid AcreedorId, string AcreedorNombre, bool Provisionado = false,
    string? CuentaContable = null)
{
    /// <summary>Clave del cargo: origen, documento, línea y concepto.</summary>
    public string Clave => $"{Origen}:{DocumentoId:N}:{LineaOrden.ToString(CultureInfo.InvariantCulture)}:{ConceptoId:N}";
}

/// <summary>Liquidación de los cargos elegidos (o de todos los pendientes del acreedor en las fechas) en su factura.</summary>
public sealed record LiquidarAcreedorComando(Guid AcreedorId, string NumeroFactura, DateOnly? FechaFactura = null, DateOnly? Desde = null, DateOnly? Hasta = null,
    IReadOnlyList<string>? Claves = null, string? CodigoIva = "IVA21", decimal PorcentajeIrpf = 0m);

public sealed record LiquidacionAcreedorDto(Guid GastoId, string NumeroFactura, int Cargos, decimal Base, decimal Total);

/// <summary>
/// Cargos y abonos con acreedor (portes al transportista, comisión del comisionista…), como en Hispatec: cada concepto
/// de línea que lleva acreedor es una deuda con él, en euros. Se leen de las facturas de venta (las que no vienen de albaranes),
/// los albaranes de venta y los pedidos de compra vivos; los liquidados en una factura del acreedor no anulada dejan de
/// estar pendientes. La liquidación registra la factura del acreedor (un gasto, con su IVA y su retención) con una línea
/// por concepto, y anota qué cargos cubre.
/// </summary>
public sealed class CargosAcreedores
{
    // Además de la RLS, se limita a la empresa activa (la conexión de análisis puede tener un rol que no la aplique).
    private const string Sql = """
        WITH cargos AS (
            SELECT 'FacturaVenta' AS origen, f.id AS documento_id, f.numero_completo AS numero, f.fecha_emision AS fecha, f.cliente_nombre AS tercero,
                   l.orden, l.descripcion AS linea, e, 1::numeric AS tasa, coalesce((e ->> 'provisionado')::boolean, false) AS provisionado
            FROM facturacion.factura f
            JOIN facturacion.linea_factura l ON l.factura_id = f.id
            CROSS JOIN LATERAL jsonb_array_elements(l.conceptos) e
            WHERE f.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid AND f.estado <> 'Anulada' AND l.albaran_venta_id IS NULL
            UNION ALL
            SELECT 'AlbaranVenta', a.id,
                   CASE WHEN a.serie IS NOT NULL THEN a.serie || extract(year FROM a.fecha)::int || '/' || lpad(a.numero::text, 5, '0') ELSE a.numero::text END,
                   a.fecha, a.cliente_nombre, l.orden, l.descripcion, e,
                   -- Un albarán en divisa lleva sus conceptos en la divisa: se pasan a euros al tipo del día del albarán.
                   CASE WHEN a.moneda IS NULL THEN 1::numeric ELSE coalesce((SELECT t.tasa_eur FROM divisas.tipo_cambio t
                        WHERE t.empresa_id = a.empresa_id AND t.divisa = a.moneda AND t.fecha <= a.fecha ORDER BY t.fecha DESC LIMIT 1), 1) END,
                   -- Provisionado en la factura que recoge el albarán.
                   EXISTS (SELECT 1 FROM facturacion.linea_factura lf JOIN facturacion.factura fa ON fa.id = lf.factura_id
                           CROSS JOIN LATERAL jsonb_array_elements(lf.conceptos) x
                           WHERE lf.albaran_venta_id = a.id AND fa.estado <> 'Anulada' AND x ->> 'conceptoId' = e ->> 'conceptoId'
                             AND coalesce((x ->> 'provisionado')::boolean, false))
            FROM facturacion.albaran_venta a
            JOIN facturacion.linea_albaran_venta l ON l.albaran_venta_id = a.id
            CROSS JOIN LATERAL jsonb_array_elements(l.conceptos) e
            WHERE a.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid AND a.anulado_en IS NULL
            UNION ALL
            SELECT 'PedidoCompra', p.id,
                   CASE WHEN p.serie IS NOT NULL THEN p.serie || p.ejercicio || '/' || lpad(p.numero::text, 5, '0') ELSE p.numero::text END,
                   p.fecha, p.proveedor_texto, l.orden, l.descripcion, e, 1::numeric, false
            FROM compras.pedido_compra p
            JOIN compras.linea_pedido l ON l.pedido_id = p.id
            CROSS JOIN LATERAL jsonb_array_elements(l.conceptos) e
            WHERE p.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid AND p.estado <> 'Cancelado')
        SELECT c.origen, c.documento_id::text, c.numero, c.fecha, c.tercero, c.orden, c.linea,
               c.e ->> 'conceptoId', c.e ->> 'codigo', c.e ->> 'nombre', c.e ->> 'efecto', round((c.e ->> 'importe')::numeric * c.tasa, 2), c.e ->> 'acreedorId', c.provisionado, c.e ->> 'cuentaContable'
        FROM cargos c
        WHERE c.e ->> 'acreedorId' IS NOT NULL
          AND (@acreedor = '' OR c.e ->> 'acreedorId' = @acreedor)
          AND c.fecha BETWEEN @desde AND @hasta
          AND (@todos OR NOT EXISTS (
                SELECT 1 FROM gastos.cargo_acreedor_liquidado x JOIN gastos.gasto g ON g.id = x.gasto_id
                WHERE g.empresa_id = NULLIF(current_setting('app.empresa_actual', true), '')::uuid AND g.estado <> 'Anulado' AND x.origen = c.origen AND x.documento_id = c.documento_id
                  AND x.linea_orden = c.orden AND x.concepto_id::text = c.e ->> 'conceptoId'))
        ORDER BY c.fecha, c.numero, c.orden
        """;

    private readonly IEjecutorAnalisis _ejecutor;
    private readonly IConsultaProveedores _proveedores;
    private readonly RegistrarGasto _registrar;
    private readonly IRepositorioCargosAcreedor _liquidados;
    private readonly IUnidadDeTrabajoGastos _unidad;
    private readonly IReloj _reloj;

    public CargosAcreedores(IEjecutorAnalisis ejecutor, IConsultaProveedores proveedores, RegistrarGasto registrar, IRepositorioCargosAcreedor liquidados,
        IUnidadDeTrabajoGastos unidad, IReloj reloj)
    {
        _ejecutor = ejecutor;
        _proveedores = proveedores;
        _registrar = registrar;
        _liquidados = liquidados;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Cargos con acreedor entre dos fechas: los pendientes de liquidar o, con <paramref name="todos"/>, también los liquidados.</summary>
    public async Task<IReadOnlyList<CargoAcreedorDto>> ListarAsync(Guid? acreedorId, DateOnly? desde, DateOnly? hasta, bool todos = false, CancellationToken ct = default)
    {
        var sentencia = new SentenciaSql(Sql,
        [
            new("acreedor", acreedorId?.ToString() ?? string.Empty),
            new("desde", desde ?? new DateOnly(2000, 1, 1)),
            new("hasta", hasta ?? new DateOnly(2100, 12, 31)),
            new("todos", todos),
        ]);
        var filas = await _ejecutor.EjecutarAsync(sentencia, ct).ConfigureAwait(false);
        var nombres = new Dictionary<Guid, string>();
        var resultado = new List<CargoAcreedorDto>();
        foreach (var f in filas)
        {
            var acreedor = Guid.Parse((string)f[12]!);
            if (!nombres.TryGetValue(acreedor, out var nombre))
            {
                nombre = (await _proveedores.ObtenerAsync(acreedor, ct).ConfigureAwait(false))?.Nombre ?? "(acreedor dado de baja)";
                nombres[acreedor] = nombre;
            }

            resultado.Add(new CargoAcreedorDto((string)f[0]!, Guid.Parse((string)f[1]!), (string)f[2]!, Fecha(f[3]), (string?)f[4] ?? string.Empty,
                Convert.ToInt32(f[5], CultureInfo.InvariantCulture), (string?)f[6] ?? string.Empty, Guid.Parse((string)f[7]!), (string?)f[8] ?? string.Empty,
                (string?)f[9] ?? string.Empty, (string?)f[10] ?? string.Empty, Convert.ToDecimal(f[11], CultureInfo.InvariantCulture), acreedor, nombre,
                f[13] is bool provisionado && provisionado, (string?)f[14]));
        }

        return resultado;
    }

    public async Task<Resultado<LiquidacionAcreedorDto>> LiquidarAsync(Guid empresaId, LiquidarAcreedorComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (string.IsNullOrWhiteSpace(comando.NumeroFactura))
        {
            return Resultado.Fallo<LiquidacionAcreedorDto>(Error.Validacion("acreedor.numero_factura", "Indica el número de la factura del acreedor."));
        }

        if (await _proveedores.ObtenerAsync(comando.AcreedorId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<LiquidacionAcreedorDto>(Error.NoEncontrado("acreedor.no_encontrado", "El acreedor no existe."));
        }

        var pendientes = await ListarAsync(comando.AcreedorId, comando.Desde, comando.Hasta, false, ct).ConfigureAwait(false);
        var elegidos = comando.Claves is { Count: > 0 } claves
            ? pendientes.Where(c => claves.Contains(c.Clave, StringComparer.Ordinal)).ToList()
            : pendientes.ToList();
        if (comando.Claves is { Count: > 0 } pedidas && elegidos.Count != pedidas.Distinct(StringComparer.Ordinal).Count())
        {
            return Resultado.Fallo<LiquidacionAcreedorDto>(Error.Conflicto("acreedor.cargo_no_pendiente", "Algún cargo elegido ya está liquidado o no es de este acreedor."));
        }

        if (elegidos.Count == 0)
        {
            return Resultado.Fallo<LiquidacionAcreedorDto>(Error.Conflicto("acreedor.sin_cargos", "El acreedor no tiene cargos pendientes de liquidar en esas fechas."));
        }

        // Lo que se debe al acreedor es el importe del cargo (en valor absoluto: un abono en la venta también es un coste a pagarle).
        // El coste ya provisionado en la venta cancela la 4009; el resto va a la cuenta de gasto (6xx) del concepto o a la de gastos.
        var lineas = elegidos.GroupBy(c => (c.Codigo, c.Concepto, Cuenta: c.Provisionado ? AlxorCore.Facturacion.Aplicacion.ProvisionCargos.CuentaProvision
                : c.CuentaContable is { } cuenta && cuenta.StartsWith('6') ? cuenta : null))
            .Select(g => new LineaGastoComando(Math.Round(g.Sum(c => Math.Abs(c.Importe)), 2), comando.CodigoIva,
                $"{g.Key.Concepto} ({g.Count()} cargo{(g.Count() == 1 ? string.Empty : "s")})", CuentaGasto: g.Key.Cuenta)).ToList();
        var fecha = comando.FechaFactura ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var gasto = await _registrar.EjecutarAsync(empresaId, new RegistrarGastoComando(
            $"Liquidación de cargos {elegidos.Min(c => c.Fecha):dd/MM/yyyy}–{elegidos.Max(c => c.Fecha):dd/MM/yyyy}", lineas.Sum(l => l.Base), comando.AcreedorId,
            CodigoIva: comando.CodigoIva, PorcentajeIrpf: comando.PorcentajeIrpf, Fecha: fecha, NumeroFactura: comando.NumeroFactura.Trim(), FechaFactura: fecha,
            Lineas: lineas), ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<LiquidacionAcreedorDto>(gasto.Error);
        }

        foreach (var c in elegidos)
        {
            _liquidados.Agregar(new CargoAcreedorLiquidado(empresaId, gasto.Valor.Id, c.AcreedorId, c.Origen, c.DocumentoId, c.LineaOrden, c.ConceptoId, c.Codigo,
                Math.Abs(c.Importe), _reloj.AhoraUtc));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new LiquidacionAcreedorDto(gasto.Valor.Id, comando.NumeroFactura.Trim(), elegidos.Count, gasto.Valor.BaseImponible, gasto.Valor.Total));
    }

    private static DateOnly Fecha(object? valor) => valor switch
    {
        DateOnly d => d,
        DateTime dt => DateOnly.FromDateTime(dt),
        _ => DateOnly.Parse(Convert.ToString(valor, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture),
    };
}

/// <summary>Los cargos de los albaranes ya liquidados en la factura de su acreedor (para no provisionarlos al facturar el albarán).</summary>
public sealed class CargosLiquidadosFacturacion : AlxorCore.Facturacion.Aplicacion.ICargosAcreedorLiquidados
{
    private readonly IRepositorioCargosAcreedor _liquidados;

    public CargosLiquidadosFacturacion(IRepositorioCargosAcreedor liquidados) => _liquidados = liquidados;

    public Task<IReadOnlyList<(Guid AlbaranId, Guid ConceptoId)>> DeAlbaranesAsync(IReadOnlyCollection<Guid> albaranes, CancellationToken ct = default) =>
        _liquidados.LiquidadosAsync("AlbaranVenta", albaranes, ct);
}
