using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>Datos de un producto para crear o actualizar.</summary>
public sealed record DatosProducto(
    string Nombre,
    decimal PrecioUnitario,
    string? Referencia = null,
    TipoProducto Tipo = TipoProducto.Servicio,
    string? CodigoIva = null,
    string? Unidad = null,
    decimal PrecioCompra = 0m,
    Guid? ProveedorHabitualId = null,
    bool ControlarStock = false,
    decimal StockInicial = 0m,
    string? UnidadCompra = null,
    decimal FactorCompra = 1m,
    string? UnidadVenta = null,
    decimal FactorVenta = 1m,
    SeguimientoArticulo Seguimiento = SeguimientoArticulo.Ninguno);

/// <summary>Caso de uso: crear un producto en la empresa activa.</summary>
public sealed class CrearProducto
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearProducto(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _historico = historico;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid empresaId, DatosProducto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var producto = Producto.Crear(empresaId, datos.Referencia, datos.Nombre, datos.Tipo, datos.PrecioUnitario, datos.PrecioCompra, datos.CodigoIva, datos.Unidad, _reloj, datos.ProveedorHabitualId, datos.ControlarStock, datos.StockInicial, datos.UnidadCompra, datos.FactorCompra, datos.UnidadVenta, datos.FactorVenta, datos.Seguimiento);
        if (producto.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(producto.Error);
        }

        _productos.Agregar(producto.Valor);
        _historico.Agregar(HistoricoPrecio.Registrar(empresaId, producto.Valor.Id, producto.Valor.PrecioUnitario, producto.Valor.PrecioCompra, _reloj.AhoraUtc));
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(producto.Valor));
    }
}

/// <summary>Caso de uso: actualizar un producto.</summary>
public sealed class ActualizarProducto
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarProducto(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _historico = historico;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid productoId, DatosProducto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<ProductoDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        var precioVentaAnterior = producto.PrecioUnitario;
        var precioCompraAnterior = producto.PrecioCompra;

        var r = producto.Actualizar(datos.Referencia, datos.Nombre, datos.Tipo, datos.PrecioUnitario, datos.PrecioCompra, datos.CodigoIva, datos.Unidad, _reloj, datos.ProveedorHabitualId, datos.ControlarStock, datos.UnidadCompra, datos.FactorCompra, datos.UnidadVenta, datos.FactorVenta, datos.Seguimiento);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(r.Error);
        }

        // Solo dejamos rastro en el histórico si algún precio cambió.
        if (producto.PrecioUnitario != precioVentaAnterior || producto.PrecioCompra != precioCompraAnterior)
        {
            _historico.Agregar(HistoricoPrecio.Registrar(producto.EmpresaId, producto.Id, producto.PrecioUnitario, producto.PrecioCompra, _reloj.AhoraUtc));
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(producto));
    }
}

/// <summary>Caso de uso: listar el histórico de precios de un producto (más reciente primero).</summary>
public sealed class ListarHistoricoPrecios
{
    private readonly IConsultaHistoricoPrecios _consulta;

    public ListarHistoricoPrecios(IConsultaHistoricoPrecios consulta) => _consulta = consulta;

    public Task<IReadOnlyList<HistoricoPrecioDto>> EjecutarAsync(Guid productoId, CancellationToken ct = default) =>
        _consulta.ListarPorProductoAsync(productoId, ct);
}

/// <summary>Caso de uso: listar los productos de la empresa activa.</summary>
public sealed class ListarProductos
{
    private readonly IConsultaProductos _consulta;

    public ListarProductos(IConsultaProductos consulta) => _consulta = consulta;

    public Task<IReadOnlyList<ProductoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, incluirInactivos: false, ct);
}

/// <summary>Caso de uso: obtener un producto por su identificador.</summary>
public sealed class ObtenerProducto
{
    private readonly IConsultaProductos _consulta;

    public ObtenerProducto(IConsultaProductos consulta) => _consulta = consulta;

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await _consulta.ObtenerAsync(productoId, ct).ConfigureAwait(false);
        return producto is null
            ? Resultado.Fallo<ProductoDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."))
            : Resultado.Ok(producto);
    }
}

/// <summary>Una línea de la lista de materiales al definirla.</summary>
public sealed record ComponenteComando(Guid ComponenteId, decimal Cantidad);

/// <summary>Datos para definir la lista de materiales de un artículo compuesto.</summary>
public sealed record DatosComposicion(IReadOnlyList<ComponenteComando> Componentes);

/// <summary>Caso de uso: definir (o quitar) la lista de materiales de un artículo.</summary>
public sealed class DefinirComposicion
{
    private readonly IRepositorioProductos _productos;
    private readonly IConsultaProductos _consulta;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public DefinirComposicion(IRepositorioProductos productos, IConsultaProductos consulta, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos; _consulta = consulta; _unidadDeTrabajo = unidadDeTrabajo; _reloj = reloj;
    }

    public async Task<Resultado<ComposicionDto>> EjecutarAsync(Guid productoId, DatosComposicion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<ComposicionDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        var componentes = datos.Componentes ?? Array.Empty<ComponenteComando>();
        if (componentes.Count == 0)
        {
            producto.QuitarComposicion(_reloj);
            await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return await ComponerDtoAsync(producto, ct).ConfigureAwait(false);
        }

        // Los componentes deben existir en el catálogo de la empresa.
        foreach (var c in componentes)
        {
            var existe = await _consulta.ObtenerAsync(c.ComponenteId, ct).ConfigureAwait(false);
            if (existe is null)
            {
                return Resultado.Fallo<ComposicionDto>(Error.Validacion("composicion.componente_desconocido", "Algún componente no existe en el catálogo."));
            }
        }

        var r = producto.DefinirComposicion(componentes.Select(c => (c.ComponenteId, c.Cantidad)).ToList(), _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ComposicionDto>(r.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return await ComponerDtoAsync(producto, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<ComposicionDto>> ComponerDtoAsync(Producto producto, CancellationToken ct)
    {
        var lineas = new List<ComponenteDto>();
        decimal total = 0m;
        foreach (var c in producto.Componentes)
        {
            var comp = await _consulta.ObtenerAsync(c.ComponenteId, ct).ConfigureAwait(false);
            var coste = comp?.PrecioCompra ?? 0m;
            var costeLinea = Redondeo.Dos(coste * c.Cantidad);
            total += costeLinea;
            lineas.Add(new ComponenteDto(c.ComponenteId, comp?.Nombre ?? "(desconocido)", c.Cantidad, comp?.Unidad ?? "ud", coste, costeLinea));
        }

        return Resultado.Ok(new ComposicionDto(producto.Id, producto.EsCompuesto, Redondeo.Dos(total), lineas));
    }
}

/// <summary>Caso de uso: obtener la lista de materiales (escandallo) de un artículo.</summary>
public sealed class ObtenerComposicion
{
    private readonly IRepositorioProductos _productos;
    private readonly IConsultaProductos _consulta;

    public ObtenerComposicion(IRepositorioProductos productos, IConsultaProductos consulta)
    {
        _productos = productos; _consulta = consulta;
    }

    public async Task<Resultado<ComposicionDto>> EjecutarAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<ComposicionDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        var lineas = new List<ComponenteDto>();
        decimal total = 0m;
        foreach (var c in producto.Componentes)
        {
            var comp = await _consulta.ObtenerAsync(c.ComponenteId, ct).ConfigureAwait(false);
            var coste = comp?.PrecioCompra ?? 0m;
            var costeLinea = Redondeo.Dos(coste * c.Cantidad);
            total += costeLinea;
            lineas.Add(new ComponenteDto(c.ComponenteId, comp?.Nombre ?? "(desconocido)", c.Cantidad, comp?.Unidad ?? "ud", coste, costeLinea));
        }

        return Resultado.Ok(new ComposicionDto(producto.Id, producto.EsCompuesto, Redondeo.Dos(total), lineas));
    }
}

/// <summary>Caso de uso: listar el catálogo de tipos de IVA disponibles.</summary>
public static class ListarImpuestos
{
    public static IReadOnlyList<ImpuestoDto> Ejecutar() => Impuesto.TodosIva.Select(ImpuestoDto.Desde).ToList();
}

/// <summary>Datos de un movimiento de stock manual.</summary>
public sealed record DatosMovimientoStock(TipoMovimientoStock Tipo, decimal Cantidad, string? Motivo = null);

/// <summary>Caso de uso: registrar un movimiento de stock (entrada, salida o ajuste) de un producto.</summary>
public sealed class RegistrarMovimientoStock
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public RegistrarMovimientoStock(IRepositorioProductos productos, IRepositorioMovimientosStock movimientos, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid productoId, DatosMovimientoStock datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<ProductoDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        var movimiento = producto.RegistrarMovimientoStock(datos.Tipo, datos.Cantidad, datos.Motivo, _reloj);
        if (movimiento.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(movimiento.Error);
        }

        _movimientos.Agregar(movimiento.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(producto));
    }
}

/// <summary>Caso de uso: listar los movimientos de stock de un producto (más reciente primero).</summary>
public sealed class ListarMovimientosStock
{
    private readonly IConsultaMovimientosStock _consulta;

    public ListarMovimientosStock(IConsultaMovimientosStock consulta) => _consulta = consulta;

    public Task<IReadOnlyList<MovimientoStockDto>> EjecutarAsync(Guid productoId, CancellationToken ct = default) =>
        _consulta.ListarPorProductoAsync(productoId, ct);
}

/// <summary>
/// Descuenta existencias al vender (puerto <see cref="IStockVentas"/>). Recorre las líneas con
/// producto asociado y, para las que llevan control de stock, registra un movimiento de venta.
/// Los productos sin control (servicios) se ignoran silenciosamente.
/// </summary>
public sealed class StockVentas : IStockVentas
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public StockVentas(IRepositorioProductos productos, IRepositorioMovimientosStock movimientos, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task DescontarVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);

        var afectados = false;
        foreach (var linea in lineas)
        {
            var producto = await _productos.ObtenerPorIdAsync(linea.ProductoId, ct).ConfigureAwait(false);
            if (producto is null || !producto.ControlarStock)
            {
                continue;
            }

            var movimiento = producto.RegistrarMovimientoStock(TipoMovimientoStock.Venta, linea.Cantidad, "Venta", _reloj);
            if (movimiento.EsCorrecto)
            {
                _movimientos.Agregar(movimiento.Valor);
                afectados = true;
            }
        }

        if (afectados)
        {
            await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
    }
}
