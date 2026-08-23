using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Informes.Aplicacion;

// ---------------------------------------------------------------------------
// Informes de gestión: análisis comercial (ventas por cliente/artículo,
// compras por proveedor), rotación de existencias y comparativa mensual.
// Todos agregan en memoria los datos que ya exponen los puertos de lectura de
// los demás módulos; no añaden persistencia.
// ---------------------------------------------------------------------------

/// <summary>Estados de factura que NO cuentan como venta (anuladas o sustituidas por rectificativa).</summary>
internal static class EstadosVenta
{
    public static bool Cuenta(string estado) =>
        !string.Equals(estado, "Anulada", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(estado, "Rectificada", StringComparison.OrdinalIgnoreCase);
}

/// <summary>Ventas de un cliente en el periodo.</summary>
public sealed record VentaClienteDto(Guid? ClienteId, string Cliente, int NumFacturas, decimal Base, decimal Cuota, decimal Total);

/// <summary>Ranking de ventas por cliente del periodo, ordenado por total facturado (descendente).</summary>
public sealed record VentasPorClienteDto(DateOnly Desde, DateOnly Hasta, decimal Total, IReadOnlyList<VentaClienteDto> Clientes);

/// <summary>Caso de uso: ventas agregadas por cliente en un periodo (excluye anuladas y rectificadas).</summary>
public sealed class GenerarVentasPorCliente
{
    private readonly IConsultaFacturas _facturas;

    public GenerarVentasPorCliente(IConsultaFacturas facturas) => _facturas = facturas;

    public async Task<VentasPorClienteDto> EjecutarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var facturas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta && EstadosVenta.Cuenta(f.Estado))
            .ToList();

        var clientes = facturas
            .GroupBy(f => new { f.ClienteId, f.ClienteNombre })
            .Select(g => new VentaClienteDto(
                g.Key.ClienteId, g.Key.ClienteNombre, g.Count(),
                Redondeo.Dos(g.Sum(f => f.BaseImponible)), Redondeo.Dos(g.Sum(f => f.CuotaIva)), Redondeo.Dos(g.Sum(f => f.Total))))
            .OrderByDescending(c => c.Total)
            .ToList();

        return new VentasPorClienteDto(desde, hasta, Redondeo.Dos(clientes.Sum(c => c.Total)), clientes);
    }
}

/// <summary>Ventas de un artículo (o concepto) en el periodo, con margen.</summary>
public sealed record VentaArticuloDto(Guid? ProductoId, string Descripcion, decimal Unidades, decimal Ingresos, decimal Coste, decimal Margen);

/// <summary>Ranking de ventas por artículo del periodo, ordenado por ingresos (descendente).</summary>
public sealed record VentasPorArticuloDto(DateOnly Desde, DateOnly Hasta, decimal Ingresos, decimal Margen, IReadOnlyList<VentaArticuloDto> Articulos);

/// <summary>Caso de uso: ventas agregadas por artículo en un periodo (unidades, ingresos, coste y margen).</summary>
public sealed class GenerarVentasPorArticulo
{
    private readonly IConsultaFacturas _facturas;

    public GenerarVentasPorArticulo(IConsultaFacturas facturas) => _facturas = facturas;

    public async Task<VentasPorArticuloDto> EjecutarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var lineas = await _facturas.ListarLineasMargenAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);

        var articulos = lineas
            .GroupBy(l => l.ProductoId is { } id ? "p:" + id : "c:" + l.Descripcion)
            .Select(g =>
            {
                var ingresos = Redondeo.Dos(g.Sum(l => l.Ingreso));
                var coste = Redondeo.Dos(g.Sum(l => l.Coste));
                return new VentaArticuloDto(g.First().ProductoId, g.First().Descripcion, g.Sum(l => l.Cantidad), ingresos, coste, Redondeo.Dos(ingresos - coste));
            })
            .OrderByDescending(a => a.Ingresos)
            .ToList();

        return new VentasPorArticuloDto(desde, hasta, Redondeo.Dos(articulos.Sum(a => a.Ingresos)), Redondeo.Dos(articulos.Sum(a => a.Margen)), articulos);
    }
}

/// <summary>Compras a un proveedor en el periodo.</summary>
public sealed record CompraProveedorDto(Guid? ProveedorId, string Proveedor, int NumGastos, decimal Base, decimal Total);

/// <summary>Ranking de compras por proveedor del periodo, ordenado por total (descendente).</summary>
public sealed record ComprasPorProveedorDto(DateOnly Desde, DateOnly Hasta, decimal Total, IReadOnlyList<CompraProveedorDto> Proveedores);

/// <summary>Caso de uso: compras/gastos agregados por proveedor en un periodo (excluye anulados).</summary>
public sealed class GenerarComprasPorProveedor
{
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaProveedores _proveedores;

    public GenerarComprasPorProveedor(IConsultaGastos gastos, IConsultaProveedores proveedores)
    {
        _gastos = gastos;
        _proveedores = proveedores;
    }

    public async Task<ComprasPorProveedorDto> EjecutarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(g => g.Fecha >= desde && g.Fecha <= hasta && !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var nombres = (await _proveedores.ListarAsync(empresaId, incluirInactivos: true, ct: ct).ConfigureAwait(false))
            .ToDictionary(p => p.Id, p => p.Nombre);

        var proveedores = gastos
            .GroupBy(g => g.ProveedorId)
            .Select(g =>
            {
                var nombre = g.Key is { } id && nombres.TryGetValue(id, out var n) ? n
                    : (g.Select(x => x.ProveedorTexto).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? "(sin proveedor)");
                return new CompraProveedorDto(g.Key, nombre, g.Count(), Redondeo.Dos(g.Sum(x => x.BaseImponible)), Redondeo.Dos(g.Sum(x => x.Total)));
            })
            .OrderByDescending(p => p.Total)
            .ToList();

        return new ComprasPorProveedorDto(desde, hasta, Redondeo.Dos(proveedores.Sum(p => p.Total)), proveedores);
    }
}

/// <summary>Rotación de un artículo: unidades vendidas frente a existencias.</summary>
public sealed record RotacionArticuloDto(Guid ProductoId, string Nombre, decimal UnidadesVendidas, decimal StockActual, decimal Rotacion, decimal? DiasCobertura);

/// <summary>Informe de rotación de existencias del periodo.</summary>
public sealed record RotacionStockDto(DateOnly Desde, DateOnly Hasta, IReadOnlyList<RotacionArticuloDto> Articulos);

/// <summary>
/// Caso de uso: rotación de existencias. Cruza las unidades vendidas por artículo (líneas de factura
/// del periodo) con el stock actual de los artículos con control de existencias. <b>Rotación</b> =
/// unidades vendidas ÷ stock actual; <b>días de cobertura</b> = stock actual ÷ ventas diarias.
/// </summary>
public sealed class GenerarRotacionStock
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaProductos _productos;

    public GenerarRotacionStock(IConsultaFacturas facturas, IConsultaProductos productos)
    {
        _facturas = facturas;
        _productos = productos;
    }

    public async Task<RotacionStockDto> EjecutarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var lineas = await _facturas.ListarLineasMargenAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var productos = await _productos.ListarAsync(empresaId, incluirInactivos: false, ct).ConfigureAwait(false);

        var vendidasPorProducto = lineas
            .Where(l => l.ProductoId is not null)
            .GroupBy(l => l.ProductoId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));

        var dias = Math.Max(1, hasta.DayNumber - desde.DayNumber + 1);

        var articulos = productos
            .Where(p => p.ControlarStock)
            .Select(p =>
            {
                var vendidas = vendidasPorProducto.TryGetValue(p.Id, out var v) ? v : 0m;
                var stock = p.Stock;
                var rotacion = stock > 0m ? Redondeo.Dos(vendidas / stock) : 0m;
                decimal? cobertura = vendidas > 0m ? Redondeo.Dos(stock / (vendidas / dias)) : null;
                return new RotacionArticuloDto(p.Id, p.Nombre, vendidas, stock, rotacion, cobertura);
            })
            .OrderByDescending(a => a.UnidadesVendidas)
            .ToList();

        return new RotacionStockDto(desde, hasta, articulos);
    }
}

/// <summary>Cifras de un mes: ventas, gastos y resultado (bases imponibles).</summary>
public sealed record MesComparativaDto(int Mes, decimal Ventas, decimal Gastos, decimal Resultado);

/// <summary>Comparativa mensual de un ejercicio: ventas, gastos y resultado por mes, con totales.</summary>
public sealed record ComparativaAnualDto(int Ejercicio, decimal Ventas, decimal Gastos, decimal Resultado, IReadOnlyList<MesComparativaDto> Meses);

/// <summary>
/// Caso de uso: comparativa mes a mes de un ejercicio (ventas de facturas emitidas frente a gastos, en
/// base imponible, y su resultado). Útil para ver la evolución y la estacionalidad del negocio.
/// </summary>
public sealed class GenerarComparativaMensual
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;

    public GenerarComparativaMensual(IConsultaFacturas facturas, IConsultaGastos gastos)
    {
        _facturas = facturas;
        _gastos = gastos;
    }

    public async Task<ComparativaAnualDto> EjecutarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var facturas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(f => f.FechaEmision.Year == ejercicio && EstadosVenta.Cuenta(f.Estado))
            .ToList();
        var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(g => g.Fecha.Year == ejercicio && !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var meses = new List<MesComparativaDto>(12);
        for (var mes = 1; mes <= 12; mes++)
        {
            var ventas = Redondeo.Dos(facturas.Where(f => f.FechaEmision.Month == mes).Sum(f => f.BaseImponible));
            var gasto = Redondeo.Dos(gastos.Where(g => g.Fecha.Month == mes).Sum(g => g.BaseImponible));
            meses.Add(new MesComparativaDto(mes, ventas, gasto, Redondeo.Dos(ventas - gasto)));
        }

        var totalVentas = Redondeo.Dos(meses.Sum(m => m.Ventas));
        var totalGastos = Redondeo.Dos(meses.Sum(m => m.Gastos));
        return new ComparativaAnualDto(ejercicio, totalVentas, totalGastos, Redondeo.Dos(totalVentas - totalGastos), meses);
    }
}
