using AlxorCore.Nucleo.Resultados;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>Vista de un producto (incluye el porcentaje de IVA resuelto del catálogo).</summary>
public sealed record ProductoDto(
    Guid Id,
    string? Referencia,
    string Nombre,
    TipoProducto Tipo,
    decimal PrecioUnitario,
    string CodigoIva,
    decimal PorcentajeIva,
    string Unidad,
    bool Activo,
    decimal PrecioCompra,
    Guid? ProveedorHabitualId,
    bool ControlarStock,
    decimal Stock,
    string? UnidadCompra,
    decimal FactorCompra,
    string? UnidadVenta,
    decimal FactorVenta,
    decimal PrecioCompraPorUnidadCompra,
    decimal PrecioVentaPorUnidadVenta,
    SeguimientoArticulo Seguimiento,
    bool EsCompuesto,
    Guid? ProductoPadreId,
    bool EsPlantilla,
    string Variante,
    string? Familia,
    Guid? FamiliaId,
    Guid? ActividadNegocioId = null,
    string TipoComposicion = "Fabricacion",
    decimal? PesoKg = null,
    string? CodigoArancelario = null,
    string? PaisOrigen = null,
    string? CodigoIgic = null,
    IReadOnlyList<TraduccionArticuloDto>? Traducciones = null)
{
    /// <summary>
    /// Construye el DTO. Las existencias (<paramref name="stock"/>) son por empresa (el catálogo se
    /// comparte por grupo), por lo que se pasan aparte; 0 cuando no procede o no hay existencias.
    /// </summary>
    public static ProductoDto Desde(Producto p, decimal stock = 0m)
    {
        ArgumentNullException.ThrowIfNull(p);
        var porcentaje = Impuesto.PorCodigoImpuesto(p.CodigoIva).Valor.Porcentaje;
        return new ProductoDto(p.Id, p.Referencia, p.Nombre, p.Tipo, p.PrecioUnitario, p.CodigoIva, porcentaje, p.Unidad, p.Activo, p.PrecioCompra, p.ProveedorHabitualId, p.ControlarStock, stock,
            p.UnidadCompra, p.FactorCompra, p.UnidadVenta, p.FactorVenta, p.PrecioCompraPorUnidadCompra, p.PrecioVentaPorUnidadVenta, p.Seguimiento, p.EsCompuesto,
            p.ProductoPadreId, p.EsPlantilla, p.ResumenVariante, p.Familia, p.FamiliaId, p.ActividadNegocioId, p.Composicion.ToString(), p.PesoKg,
            p.CodigoArancelario, p.PaisOrigen, p.CodigoIgic, p.Traducciones.Select(t => new TraduccionArticuloDto(t.Idioma, t.Nombre)).ToList());
    }
}

/// <summary>
/// Componente de la lista de materiales, con nombre, coste, precio y peso. Si es a su vez compuesto, lleva su propia
/// lista (<see cref="Componentes"/>): el árbol completo, con el coste y el peso calculados de abajo arriba.
/// </summary>
public sealed record ComponenteDto(Guid ComponenteId, string Nombre, decimal Cantidad, string Unidad, decimal CosteUnitario, decimal CosteLinea,
    bool EsCompuesto = false, string? TipoComposicion = null, decimal PrecioUnitario = 0m, decimal? PesoKg = null, IReadOnlyList<ComponenteDto>? Componentes = null);

/// <summary>Necesidad de un material básico (sin composición) para una unidad del compuesto, sumando todos los niveles.</summary>
public sealed record NecesidadDto(Guid ProductoId, string Nombre, string Unidad, decimal Cantidad, bool ControlarStock, decimal Stock);

/// <summary>
/// Lista de materiales de un artículo compuesto (escandallo): árbol de varios niveles, coste, precio según
/// componentes, peso calculado, explosión en materiales básicos y, en un kit, cuántos se pueden vender con las
/// existencias de sus componentes. <see cref="CompuestosIguales"/> avisa de otros compuestos con la misma lista.
/// </summary>
public sealed record ComposicionDto(Guid ProductoId, bool EsCompuesto, decimal CosteTotal, IReadOnlyList<ComponenteDto> Componentes,
    string TipoComposicion = "Fabricacion", bool PrecioSegunComponentes = false, decimal AjustePrecio = 0m, decimal PrecioComponentes = 0m,
    decimal? PrecioCalculado = null, decimal? PesoKg = null, int Niveles = 0, IReadOnlyList<NecesidadDto>? Explosion = null, decimal? DisponibleKit = null,
    IReadOnlyList<string>? CompuestosIguales = null);

/// <summary>Un compuesto que usa el artículo (directamente, nivel 1, o dentro de otro compuesto).</summary>
public sealed record UsoComponenteDto(Guid ProductoId, string Nombre, string TipoComposicion, int Nivel, decimal CantidadPorUnidad);

/// <summary>Fila del histórico de movimientos de stock de un producto.</summary>
public sealed record MovimientoStockDto(DateTimeOffset Fecha, string Tipo, decimal Cantidad, decimal StockResultante, string? Motivo)
{
    public static MovimientoStockDto Desde(MovimientoStock m) => new(m.CreadoEn, m.Tipo.ToString(), m.Cantidad, m.StockResultante, m.Motivo);
}

/// <summary>Fila del histórico de precios de un producto.</summary>
public sealed record HistoricoPrecioDto(DateTimeOffset RegistradoEn, decimal PrecioVenta, decimal PrecioCompra)
{
    public static HistoricoPrecioDto Desde(HistoricoPrecio h) => new(h.RegistradoEn, h.PrecioVenta, h.PrecioCompra);
}

/// <summary>Vista de un tipo de impuesto del catálogo.</summary>
public sealed record ImpuestoDto(string Codigo, string Nombre, TipoImpuesto Tipo, decimal Porcentaje)
{
    public static ImpuestoDto Desde(Impuesto i) => new(i.Codigo, i.Nombre, i.Tipo, i.Porcentaje);
}

/// <summary>Repositorio de productos (escritura).</summary>
public interface IRepositorioProductos
{
    Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Compuestos que llevan el artículo como componente directo.</summary>
    Task<IReadOnlyList<Producto>> CompuestosConComponenteAsync(Guid componenteId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);

    /// <summary>Compuestos del grupo (para buscar uno con la misma lista de materiales).</summary>
    Task<IReadOnlyList<Producto>> CompuestosAsync(Guid grupoId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<Producto>>([]);

    void Agregar(Producto producto);

    void Eliminar(Producto producto);
}

/// <summary>
/// Filtros de búsqueda de productos en servidor (todos opcionales). <paramref name="Texto"/> busca en
/// el nombre y la referencia; <paramref name="FamiliaId"/> filtra por familia del catálogo.
/// </summary>
public sealed record FiltroProductos(
    string? Texto = null,
    Guid? FamiliaId = null,
    bool IncluirInactivos = false,
    IReadOnlyCollection<Guid>? ActividadesPermitidas = null);

/// <summary>Consultas de lectura de productos (las usan la API y Facturación).</summary>
public interface IConsultaProductos
{
    Task<ProductoDto?> ObtenerAsync(Guid productoId, CancellationToken ct = default);

    Task<IReadOnlyList<ProductoDto>> ListarAsync(Guid grupoId, bool incluirInactivos = false, IReadOnlyCollection<Guid>? actividadesPermitidas = null, CancellationToken ct = default);

    /// <summary>Búsqueda paginada y filtrada de productos (el filtrado ocurre en la base de datos).</summary>
    Task<PaginaResultado<ProductoDto>> BuscarAsync(Guid empresaId, FiltroProductos filtro, Paginacion paginacion, CancellationToken ct = default);

    /// <summary>Identificadores de todos los artículos del filtro (sin paginar), para calcular totales en el servidor.</summary>
    Task<IReadOnlyList<Guid>> IdsFiltradosAsync(Guid grupoId, FiltroProductos filtro, CancellationToken ct = default);

    Task<IReadOnlyList<ProductoDto>> ListarVariantesAsync(Guid padreId, CancellationToken ct = default);
}

/// <summary>Nombre de un artículo en otro idioma.</summary>
public sealed record TraduccionArticuloDto(string Idioma, string Nombre);

/// <summary>Nombre del artículo en castellano y en el idioma pedido (el mismo si no tiene traducción).</summary>
public sealed record NombreArticuloIdioma(string Nombre, string Traducido);

/// <summary>Nombres traducidos de los artículos (para imprimir documentos en otro idioma).</summary>
public interface IConsultaTraduccionesArticulos
{
    Task<IReadOnlyDictionary<Guid, NombreArticuloIdioma>> NombresEnIdiomaAsync(IReadOnlyCollection<Guid> productoIds, string idioma, CancellationToken ct = default);
}

/// <summary>Consulta y cambio de las traducciones del nombre de un artículo.</summary>
public sealed class TraduccionesArticulos
{
    private readonly IRepositorioProductos _productos;
    private readonly IUnidadDeTrabajoCatalogo _unidad;
    private readonly AlxorCore.Nucleo.Tiempo.IReloj _reloj;

    public TraduccionesArticulos(IRepositorioProductos productos, IUnidadDeTrabajoCatalogo unidad, AlxorCore.Nucleo.Tiempo.IReloj reloj)
    {
        _productos = productos;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<IReadOnlyList<TraduccionArticuloDto>>> ListarAsync(Guid productoId, CancellationToken ct = default) =>
        await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false) is { } p
            ? Resultado.Ok<IReadOnlyList<TraduccionArticuloDto>>(p.Traducciones.Select(t => new TraduccionArticuloDto(t.Idioma, t.Nombre)).ToList())
            : Resultado.Fallo<IReadOnlyList<TraduccionArticuloDto>>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));

    public async Task<Resultado<IReadOnlyList<TraduccionArticuloDto>>> FijarAsync(Guid productoId, IReadOnlyList<TraduccionArticuloDto> traducciones, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(traducciones);
        var p = await _productos.ObtenerPorIdAsync(productoId, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<IReadOnlyList<TraduccionArticuloDto>>(Error.NoEncontrado("producto.no_encontrado", "El artículo no existe."));
        }

        var r = p.FijarTraducciones(traducciones.Select(t => ((string?)t.Idioma, (string?)t.Nombre)).ToList(), _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<TraduccionArticuloDto>>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<TraduccionArticuloDto>>(p.Traducciones.Select(t => new TraduccionArticuloDto(t.Idioma, t.Nombre)).ToList());
    }
}

/// <summary>Repositorio del histórico de precios (solo escritura: se añaden filas).</summary>
public interface IRepositorioHistoricoPrecios
{
    void Agregar(HistoricoPrecio historico);
}

/// <summary>Consulta del histórico de precios de un producto.</summary>
public interface IConsultaHistoricoPrecios
{
    Task<IReadOnlyList<HistoricoPrecioDto>> ListarPorProductoAsync(Guid productoId, CancellationToken ct = default);
}

/// <summary>Repositorio de movimientos de stock (solo escritura: se añaden filas).</summary>
public interface IRepositorioMovimientosStock
{
    void Agregar(MovimientoStock movimiento);
}

/// <summary>
/// Repositorio de existencias «simples» por empresa. El aislamiento por empresa lo garantiza el
/// filtro global (la entidad es por empresa), así que se busca solo por artículo.
/// </summary>
public interface IRepositorioExistenciasSimples
{
    Task<ExistenciaSimple?> ObtenerPorProductoAsync(Guid productoId, CancellationToken ct = default);

    void Agregar(ExistenciaSimple existencia);
}

/// <summary>Consulta del histórico de movimientos de stock de un producto.</summary>
public interface IConsultaMovimientosStock
{
    Task<IReadOnlyList<MovimientoStockDto>> ListarPorProductoAsync(Guid productoId, CancellationToken ct = default);
}

/// <summary>Una línea vendida que puede descontar existencias (producto + cantidad).</summary>
public sealed record LineaVenta(Guid ProductoId, decimal Cantidad);

/// <summary>
/// Puerto que descuenta existencias al vender. Lo usa Facturación tras emitir una factura o ticket,
/// sin conocer los detalles del módulo Catálogo. Es tolerante: ignora productos sin control de stock.
/// </summary>
public interface IStockVentas
{
    Task DescontarVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, CancellationToken ct = default);

    /// <summary>Vuelve a meter en el almacén lo que salió por una venta que se deshace (un albarán anulado).</summary>
    Task DevolverVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, string motivo, CancellationToken ct = default) => Task.CompletedTask;

    /// <summary>Venta de un centro: sale primero de su almacén habitual.</summary>
    Task DescontarVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, Guid? centroId, CancellationToken ct = default) =>
        DescontarVentaAsync(empresaId, lineas, ct);

    /// <summary>Venta de un centro que se deshace: vuelve a su almacén habitual.</summary>
    Task DevolverVentaAsync(Guid empresaId, IReadOnlyList<LineaVenta> lineas, string motivo, Guid? centroId, CancellationToken ct = default) =>
        DevolverVentaAsync(empresaId, lineas, motivo, ct);
}

/// <summary>
/// Existencias por almacén (módulo Inventario). Cuando la empresa trabaja con almacenes, los movimientos de la ficha del
/// artículo van al almacén principal y la existencia simple solo refleja el total.
/// </summary>
public interface IExistenciasAlmacen
{
    Task<bool> TrabajaConAlmacenesAsync(Guid empresaId, CancellationToken ct = default);

    Task<Resultado> MovimientoAsync(Guid empresaId, Guid productoId, TipoMovimientoStock tipo, decimal cantidad, string? motivo, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Catálogo.</summary>
public interface IUnidadDeTrabajoCatalogo : IUnidadDeTrabajo;
