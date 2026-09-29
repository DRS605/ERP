using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Aplicacion;

namespace AlxorCore.Logistica.Aplicacion;

public interface IUnidadDeTrabajoLogistica : IUnidadDeTrabajo
{
    /// <summary>Bloqueo transaccional (hasta guardar) para numerar los SSCC sin cruces.</summary>
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

public interface IRepositorioLogistica
{
    Task<ConfiguracionLogistica?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default);

    void Agregar(ConfiguracionLogistica configuracion);

    Task<IReadOnlyList<TipoSoporte>> SoportesAsync(Guid empresaId, CancellationToken ct = default);

    Task<TipoSoporte?> SoporteAsync(Guid id, CancellationToken ct = default);

    void Agregar(TipoSoporte soporte);

    void Eliminar(TipoSoporte soporte);

    Task<bool> SoporteUsadoAsync(Guid soporteId, CancellationToken ct = default);

    Task<FichaLogistica?> FichaAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);

    Task<IReadOnlyList<FichaLogistica>> FichasAsync(Guid empresaId, CancellationToken ct = default);

    Task<FichaLogistica?> FichaPorGtinAsync(Guid empresaId, string gtin14, CancellationToken ct = default);

    void Agregar(FichaLogistica ficha);

    void Eliminar(FichaLogistica ficha);

    Task<IReadOnlyList<PlantillaPaletizado>> PlantillasAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default);

    Task<PlantillaPaletizado?> PlantillaAsync(Guid id, CancellationToken ct = default);

    void Agregar(PlantillaPaletizado plantilla);

    void Eliminar(PlantillaPaletizado plantilla);

    Task<bool> PlantillaUsadaAsync(Guid plantillaId, CancellationToken ct = default);

    Task<UnidadLogistica?> UnidadAsync(Guid id, CancellationToken ct = default);

    Task<UnidadLogistica?> UnidadPorSsccAsync(Guid empresaId, string sscc, CancellationToken ct = default);

    Task<IReadOnlyList<UnidadLogistica>> UnidadesAsync(Guid empresaId, FiltroUnidades filtro, CancellationToken ct = default);

    Task<IReadOnlyList<UnidadLogistica>> HijasAsync(Guid padreId, CancellationToken ct = default);

    /// <summary>Unidades del artículo ya paletizadas (abiertas o cerradas) en el almacén, por lote (sin la unidad indicada).</summary>
    Task<IReadOnlyDictionary<string, decimal>> PaletizadoAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? salvoUnidadId, CancellationToken ct = default);

    void Agregar(UnidadLogistica unidad);
}

public sealed record FiltroUnidades(EstadoUnidadLogistica? Estado = null, Guid? AlmacenId = null, Guid? ProductoId = null, string? Lote = null, Guid? PedidoVentaId = null,
    Guid? OrdenFabricacionId = null, bool SoloRaiz = false, int Maximo = 500);

/// <summary>Existencia de un artículo en un almacén por lote, con la caducidad del lote si se conoce.</summary>
public sealed record ExistenciaLote(Guid AlmacenId, string? Lote, decimal Cantidad, DateOnly? FechaCaducidad);

/// <summary>Existencias del inventario (lo implementa la API sobre el módulo Inventario).</summary>
public interface IExistenciasLogistica
{
    Task<IReadOnlyList<ExistenciaLote>> DeProductoAsync(Guid empresaId, Guid productoId, Guid almacenId, CancellationToken ct = default);

    Task<bool> AlmacenExisteAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default);
}

public sealed record ArticuloLogistica(Guid Id, string Nombre, string? Unidad);

/// <summary>Datos de los artículos del catálogo.</summary>
public interface IArticulosLogistica
{
    Task<IReadOnlyDictionary<Guid, ArticuloLogistica>> ObtenerAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
}

public sealed record LineaPedidoLogistica(Guid LineaId, Guid ProductoId, string Descripcion, decimal Pendiente);

public sealed record PedidoLogistica(Guid Id, string Numero, Guid ClienteId, string Cliente, IReadOnlyList<LineaPedidoLogistica> Lineas);

/// <summary>Pedidos de venta (para calcular sus palés).</summary>
public interface IPedidosLogistica
{
    Task<PedidoLogistica?> ObtenerAsync(Guid pedidoVentaId, CancellationToken ct = default);
}

public sealed record OrdenFabricacionLogistica(Guid Id, string Numero, Guid ProductoId, decimal Cantidad, bool Terminada, string? Lote, DateOnly? FechaCaducidad,
    DateOnly? FechaFabricacion);

/// <summary>Órdenes de fabricación (para paletizar lo fabricado).</summary>
public interface IFabricacionLogistica
{
    Task<OrdenFabricacionLogistica?> ObtenerAsync(Guid ordenId, CancellationToken ct = default);
}
