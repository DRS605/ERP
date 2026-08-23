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
    Guid? ActividadNegocioId = null)
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
            p.ProductoPadreId, p.EsPlantilla, p.ResumenVariante, p.Familia, p.FamiliaId, p.ActividadNegocioId);
    }
}

/// <summary>Componente de la lista de materiales, enriquecido con nombre y coste.</summary>
public sealed record ComponenteDto(Guid ComponenteId, string Nombre, decimal Cantidad, string Unidad, decimal CosteUnitario, decimal CosteLinea);

/// <summary>Lista de materiales de un artículo compuesto, con el coste agregado (escandallo).</summary>
public sealed record ComposicionDto(Guid ProductoId, bool EsCompuesto, decimal CosteTotal, IReadOnlyList<ComponenteDto> Componentes);

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

    void Agregar(Producto producto);
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

    Task<IReadOnlyList<ProductoDto>> ListarVariantesAsync(Guid padreId, CancellationToken ct = default);
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
}

/// <summary>Unidad de trabajo del módulo Catálogo.</summary>
public interface IUnidadDeTrabajoCatalogo : IUnidadDeTrabajo;
