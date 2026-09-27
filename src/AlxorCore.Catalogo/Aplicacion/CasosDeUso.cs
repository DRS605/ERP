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
    SeguimientoArticulo Seguimiento = SeguimientoArticulo.Ninguno,
    string? Familia = null,
    Guid? FamiliaId = null,
    Guid? ActividadNegocioId = null,
    decimal? PesoKg = null,
    string? CodigoArancelario = null,
    string? PaisOrigen = null);

/// <summary>Caso de uso: crear un producto en la empresa activa.</summary>
public sealed class CrearProducto
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly IRepositorioExistenciasSimples _existencias;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IConsultaFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearProducto(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, IRepositorioExistenciasSimples existencias, IRepositorioMovimientosStock movimientos, IConsultaFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _historico = historico;
        _existencias = existencias;
        _movimientos = movimientos;
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    /// <summary>
    /// Crea el artículo en el grupo (catálogo compartido). Si lleva control de stock y se indica un
    /// stock inicial, se registra en la <b>empresa</b> activa (<paramref name="empresaId"/>), pues las
    /// existencias son por empresa.
    /// </summary>
    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid grupoId, Guid empresaId, DatosProducto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var producto = Producto.Crear(grupoId, datos.Referencia, datos.Nombre, datos.Tipo, datos.PrecioUnitario, datos.PrecioCompra, datos.CodigoIva, datos.Unidad, _reloj, datos.ProveedorHabitualId, datos.ControlarStock, datos.UnidadCompra, datos.FactorCompra, datos.UnidadVenta, datos.FactorVenta, datos.Seguimiento);
        if (producto.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(producto.Error);
        }

        var familia = await ResolverFamilia.AplicarAsync(_familias, producto.Valor, datos, ct).ConfigureAwait(false);
        if (familia.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(familia.Error);
        }

        producto.Valor.EstablecerActividad(datos.ActividadNegocioId);
        if (producto.Valor.EstablecerPeso(datos.PesoKg) is { EsFallo: true } peso)
        {
            return Resultado.Fallo<ProductoDto>(peso.Error);
        }

        if (producto.Valor.EstablecerComercioExterior(datos.CodigoArancelario, datos.PaisOrigen) is { EsFallo: true } exterior)
        {
            return Resultado.Fallo<ProductoDto>(exterior.Error);
        }

        _productos.Agregar(producto.Valor);
        _historico.Agregar(HistoricoPrecio.Registrar(grupoId, producto.Valor.Id, producto.Valor.PrecioUnitario, producto.Valor.PrecioCompra, _reloj.AhoraUtc));

        var stock = 0m;
        if (datos.ControlarStock && datos.StockInicial != 0m)
        {
            var existencia = ExistenciaSimple.Crear(empresaId, producto.Valor.Id, _reloj);
            var movimiento = existencia.Aplicar(TipoMovimientoStock.Entrada, datos.StockInicial, "Stock inicial", _reloj);
            if (movimiento.EsCorrecto)
            {
                _existencias.Agregar(existencia);
                _movimientos.Agregar(movimiento.Valor);
                stock = existencia.Cantidad;
            }
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(producto.Valor, stock));
    }
}

/// <summary>Caso de uso: actualizar un producto.</summary>
public sealed class ActualizarProducto
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly IConsultaFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarProducto(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, IConsultaFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _historico = historico;
        _familias = familias;
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

        var familia = await ResolverFamilia.AplicarAsync(_familias, producto, datos, ct).ConfigureAwait(false);
        if (familia.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(familia.Error);
        }

        producto.EstablecerActividad(datos.ActividadNegocioId);
        if (producto.EstablecerPeso(datos.PesoKg) is { EsFallo: true } peso)
        {
            return Resultado.Fallo<ProductoDto>(peso.Error);
        }

        if (producto.EstablecerComercioExterior(datos.CodigoArancelario, datos.PaisOrigen) is { EsFallo: true } exterior)
        {
            return Resultado.Fallo<ProductoDto>(exterior.Error);
        }


        // Solo dejamos rastro en el histórico si algún precio cambió.
        if (producto.PrecioUnitario != precioVentaAnterior || producto.PrecioCompra != precioCompraAnterior)
        {
            _historico.Agregar(HistoricoPrecio.Registrar(producto.GrupoId, producto.Id, producto.PrecioUnitario, producto.PrecioCompra, _reloj.AhoraUtc));
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

    public Task<IReadOnlyList<ProductoDto>> EjecutarAsync(Guid grupoId, IReadOnlyCollection<Guid>? actividadesPermitidas = null, bool incluirBajas = false, CancellationToken ct = default) =>
        _consulta.ListarAsync(grupoId, incluirBajas, actividadesPermitidas, ct);
}

/// <summary>Caso de uso: buscar productos con filtros (texto, familia, activos) y paginación en servidor.</summary>
public sealed class BuscarProductos
{
    private readonly IConsultaProductos _consulta;

    public BuscarProductos(IConsultaProductos consulta) => _consulta = consulta;

    public Task<Nucleo.Consultas.PaginaResultado<ProductoDto>> EjecutarAsync(Guid empresaId, FiltroProductos filtro, Nucleo.Consultas.Paginacion paginacion, CancellationToken ct = default) =>
        _consulta.BuscarAsync(empresaId, filtro, paginacion, ct);
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

/// <summary>
/// Datos para definir la lista de materiales de un artículo compuesto: sus componentes, si es de fabricación o un kit
/// de venta, y si su precio sale de los componentes (con un ajuste en %).
/// </summary>
public sealed record DatosComposicion(IReadOnlyList<ComponenteComando> Componentes, TipoComposicion Tipo = TipoComposicion.Fabricacion,
    bool PrecioSegunComponentes = false, decimal AjustePrecio = 0m);

/// <summary>Caso de uso: definir (o quitar) la lista de materiales de un artículo.</summary>
public sealed class DefinirComposicion
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly ArbolComposiciones _arbol;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public DefinirComposicion(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, ArbolComposiciones arbol, IUnidadDeTrabajoCatalogo unidadDeTrabajo,
        IReloj reloj)
    {
        _productos = productos; _historico = historico; _arbol = arbol; _unidadDeTrabajo = unidadDeTrabajo; _reloj = reloj;
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
            return Resultado.Ok(await _arbol.ComposicionAsync(producto, ct).ConfigureAwait(false));
        }

        // Los componentes deben existir en el catálogo, sin ciclos (A lleva B y B lleva A) ni más niveles de la cuenta.
        foreach (var c in componentes)
        {
            if (await _productos.ObtenerPorIdAsync(c.ComponenteId, ct).ConfigureAwait(false) is null)
            {
                return Resultado.Fallo<ComposicionDto>(Error.Validacion("composicion.componente_desconocido", "Algún componente no existe en el catálogo."));
            }
        }

        if (await _arbol.CreariaCicloAsync(productoId, componentes.Select(c => c.ComponenteId), ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ComposicionDto>(Error.Validacion("composicion.ciclo",
                "Algún componente ya lleva, dentro de su composición, a este mismo artículo: se formaría un ciclo."));
        }

        if (await _arbol.NivelesAsync(componentes.Select(c => c.ComponenteId), ct).ConfigureAwait(false) > ArbolComposiciones.MaximoNiveles)
        {
            return Resultado.Fallo<ComposicionDto>(Error.Validacion("composicion.niveles", $"La composición tendría más de {ArbolComposiciones.MaximoNiveles} niveles."));
        }

        var r = producto.DefinirComposicion(componentes.Select(c => (c.ComponenteId, c.Cantidad)).ToList(), _reloj, datos.Tipo, datos.PrecioSegunComponentes, datos.AjustePrecio);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ComposicionDto>(r.Error);
        }

        await AplicarPrecioAsync(producto, ct).ConfigureAwait(false);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await _arbol.ComposicionAsync(producto, ct).ConfigureAwait(false));
    }

    /// <summary>Recalcula el precio de los compuestos «según componentes» del grupo (tras cambiar precios de componentes).</summary>
    public async Task<int> RecalcularPreciosAsync(Guid grupoId, CancellationToken ct = default)
    {
        var cambiados = 0;
        foreach (var p in (await _productos.CompuestosAsync(grupoId, ct).ConfigureAwait(false)).Where(x => x.PrecioSegunComponentes))
        {
            cambiados += await AplicarPrecioAsync(p, ct).ConfigureAwait(false) ? 1 : 0;
        }

        if (cambiados > 0)
        {
            await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return cambiados;
    }

    private async Task<bool> AplicarPrecioAsync(Producto producto, CancellationToken ct)
    {
        if (!producto.PrecioSegunComponentes)
        {
            return false;
        }

        var anterior = producto.PrecioUnitario;
        producto.FijarPrecioSegunComponentes(await _arbol.PrecioSegunComponentesAsync(producto, ct).ConfigureAwait(false), _reloj);
        if (producto.PrecioUnitario == anterior)
        {
            return false;
        }

        _historico.Agregar(HistoricoPrecio.Registrar(producto.GrupoId, producto.Id, producto.PrecioUnitario, producto.PrecioCompra, _reloj.AhoraUtc));
        return true;
    }
}

/// <summary>Caso de uso: obtener la lista de materiales (escandallo) de un artículo, y las consultas inversas.</summary>
public sealed class ObtenerComposicion
{
    private readonly IRepositorioProductos _productos;
    private readonly ArbolComposiciones _arbol;

    public ObtenerComposicion(IRepositorioProductos productos, ArbolComposiciones arbol)
    {
        _productos = productos; _arbol = arbol;
    }

    public async Task<Resultado<ComposicionDto>> EjecutarAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        return producto is null
            ? Resultado.Fallo<ComposicionDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."))
            : Resultado.Ok(await _arbol.ComposicionAsync(producto, ct).ConfigureAwait(false));
    }

    /// <summary>Dónde se usa el artículo (compuestos que lo llevan, en cualquier nivel).</summary>
    public async Task<Resultado<IReadOnlyList<UsoComponenteDto>>> UsosAsync(Guid productoId, CancellationToken ct = default) =>
        await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false) is null
            ? Resultado.Fallo<IReadOnlyList<UsoComponenteDto>>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."))
            : Resultado.Ok(await _arbol.UsosAsync(productoId, ct).ConfigureAwait(false));

    /// <summary>Compuestos que ya tienen exactamente esta lista de materiales (para no crear uno repetido).</summary>
    public async Task<IReadOnlyList<CompuestoIgualDto>> IgualesAsync(Guid grupoId, IReadOnlyList<ComponenteComando> componentes, CancellationToken ct = default) =>
        (await _arbol.IgualesAsync(grupoId, (componentes ?? []).Select(c => (c.ComponenteId, c.Cantidad)).ToList(), ct).ConfigureAwait(false))
            .Select(x => new CompuestoIgualDto(x.Id, x.Nombre)).ToList();
}

/// <summary>Un compuesto con la misma lista de materiales.</summary>
public sealed record CompuestoIgualDto(Guid Id, string Nombre);

/// <summary>Un eje de la variante (p. ej. Talla=M).</summary>
public sealed record AtributoComando(string Nombre, string Valor);

/// <summary>Datos para crear una variante de un artículo plantilla.</summary>
public sealed record DatosVariante(IReadOnlyList<AtributoComando> Atributos, string? Referencia = null, decimal? PrecioUnitario = null, decimal? PrecioCompra = null);

/// <summary>Caso de uso: crear una variante (talla/color/…) de un artículo, que pasa a ser plantilla.</summary>
public sealed class CrearVariante
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioHistoricoPrecios _historico;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearVariante(IRepositorioProductos productos, IRepositorioHistoricoPrecios historico, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos; _historico = historico; _unidadDeTrabajo = unidadDeTrabajo; _reloj = reloj;
    }

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid padreId, DatosVariante datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var padre = await _productos.ObtenerPorIdAsync(padreId, ct).ConfigureAwait(false);
        if (padre is null)
        {
            return Resultado.Fallo<ProductoDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        if (padre.EsVariante)
        {
            return Resultado.Fallo<ProductoDto>(Error.Validacion("variante.padre_es_variante", "No se pueden crear variantes de otra variante."));
        }

        var atributos = (datos.Atributos ?? Array.Empty<AtributoComando>())
            .Where(a => !string.IsNullOrWhiteSpace(a.Nombre) && !string.IsNullOrWhiteSpace(a.Valor)).ToList();
        if (atributos.Count == 0)
        {
            return Resultado.Fallo<ProductoDto>(Error.Validacion("variante.sin_atributos", "Indica al menos un atributo (p. ej. Talla o Color)."));
        }

        var sufijo = string.Join(" / ", atributos.Select(a => a.Valor.Trim()));
        var nombre = $"{padre.Nombre} {sufijo}";
        var precio = datos.PrecioUnitario ?? padre.PrecioUnitario;
        var precioCompra = datos.PrecioCompra ?? padre.PrecioCompra;

        var variante = Producto.Crear(padre.GrupoId, datos.Referencia, nombre, padre.Tipo, precio, precioCompra, padre.CodigoIva, padre.Unidad, _reloj,
            padre.ProveedorHabitualId, padre.ControlarStock, padre.UnidadCompra, padre.FactorCompra, padre.UnidadVenta, padre.FactorVenta, padre.Seguimiento);
        if (variante.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(variante.Error);
        }

        variante.Valor.AsignarComoVariante(padre.Id, atributos.Select(a => (a.Nombre, a.Valor)).ToList());
        padre.MarcarPlantilla(true, _reloj);

        _productos.Agregar(variante.Valor);
        _historico.Agregar(HistoricoPrecio.Registrar(padre.GrupoId, variante.Valor.Id, precio, precioCompra, _reloj.AhoraUtc));
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(variante.Valor));
    }
}

/// <summary>Caso de uso: listar las variantes de un artículo plantilla.</summary>
public sealed class ListarVariantes
{
    private readonly IConsultaProductos _consulta;

    public ListarVariantes(IConsultaProductos consulta) => _consulta = consulta;

    public Task<IReadOnlyList<ProductoDto>> EjecutarAsync(Guid padreId, CancellationToken ct = default) =>
        _consulta.ListarVariantesAsync(padreId, ct);
}

/// <summary>Caso de uso: listar el catálogo estatal de tipos del impuesto indirecto (IVA, o IGIC en Canarias).</summary>
public static class ListarImpuestos
{
    public static IReadOnlyList<ImpuestoDto> Ejecutar(TipoImpuesto impuesto = TipoImpuesto.Iva) => Impuesto.De(impuesto).Select(ImpuestoDto.Desde).ToList();
}

/// <summary>Datos de un movimiento de stock manual.</summary>
public sealed record DatosMovimientoStock(TipoMovimientoStock Tipo, decimal Cantidad, string? Motivo = null);

/// <summary>Caso de uso: registrar un movimiento de stock (entrada, salida o ajuste) de un producto en la empresa activa.</summary>
public sealed class RegistrarMovimientoStock
{
    private readonly IRepositorioProductos _productos;
    private readonly IRepositorioExistenciasSimples _existencias;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public RegistrarMovimientoStock(IRepositorioProductos productos, IRepositorioExistenciasSimples existencias, IRepositorioMovimientosStock movimientos, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _productos = productos;
        _existencias = existencias;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ProductoDto>> EjecutarAsync(Guid empresaId, Guid productoId, DatosMovimientoStock datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<ProductoDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        if (!producto.ControlarStock)
        {
            return Resultado.Fallo<ProductoDto>(Error.Conflicto("producto.sin_control_stock", "Este artículo no lleva control de stock."));
        }

        var existencia = await _existencias.ObtenerPorProductoAsync(productoId, ct).ConfigureAwait(false);
        var nueva = existencia is null;
        existencia ??= ExistenciaSimple.Crear(empresaId, productoId, _reloj);

        var movimiento = existencia.Aplicar(datos.Tipo, datos.Cantidad, datos.Motivo, _reloj);
        if (movimiento.EsFallo)
        {
            return Resultado.Fallo<ProductoDto>(movimiento.Error);
        }

        if (nueva)
        {
            _existencias.Agregar(existencia);
        }

        _movimientos.Agregar(movimiento.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProductoDto.Desde(producto, existencia.Cantidad));
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
    private readonly IRepositorioExistenciasSimples _existencias;
    private readonly IRepositorioMovimientosStock _movimientos;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    private readonly ArbolComposiciones _arbol;

    public StockVentas(IRepositorioProductos productos, IRepositorioExistenciasSimples existencias, IRepositorioMovimientosStock movimientos, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj,
        ArbolComposiciones arbol)
    {
        _arbol = arbol;
        _productos = productos;
        _existencias = existencias;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task DescontarVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);

        // Un kit no tiene existencias propias: se descuentan sus componentes (en todos los niveles de kits). Las
        // cantidades se agrupan por artículo, así una misma existencia se mueve una sola vez.
        var aDescontar = new Dictionary<Guid, (decimal Cantidad, string Motivo)>();
        foreach (var linea in lineas)
        {
            var producto = await _productos.ObtenerPorIdAsync(linea.ProductoId, ct).ConfigureAwait(false);
            if (producto is { EsCompuesto: true, Composicion: TipoComposicion.Kit })
            {
                foreach (var (componenteId, cantidad) in await _arbol.ExplosionAsync(producto, linea.Cantidad, true, ct).ConfigureAwait(false))
                {
                    var previo = aDescontar.GetValueOrDefault(componenteId, (0m, $"Venta (kit {producto.Nombre})"));
                    aDescontar[componenteId] = (previo.Item1 + cantidad, previo.Item2);
                }
            }
            else
            {
                var previo = aDescontar.GetValueOrDefault(linea.ProductoId, (0m, "Venta"));
                aDescontar[linea.ProductoId] = (previo.Item1 + linea.Cantidad, previo.Item2);
            }
        }

        var afectados = false;
        foreach (var (productoId, (cantidadVendida, motivo)) in aDescontar)
        {
            var producto = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
            if (producto is null || !producto.ControlarStock || cantidadVendida <= 0m)
            {
                continue;
            }

            var existencia = await _existencias.ObtenerPorProductoAsync(productoId, ct).ConfigureAwait(false);
            var nueva = existencia is null;
            existencia ??= ExistenciaSimple.Crear(empresaId, productoId, _reloj);

            var movimiento = existencia.Aplicar(TipoMovimientoStock.Venta, cantidadVendida, motivo, _reloj);
            if (movimiento.EsCorrecto)
            {
                if (nueva)
                {
                    _existencias.Agregar(existencia);
                }

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
