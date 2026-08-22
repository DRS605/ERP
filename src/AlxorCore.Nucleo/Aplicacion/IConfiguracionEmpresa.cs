using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Consulta de parámetros de empresa transversales (los fija la implantación). Permite a cualquier
/// módulo conocer, por ejemplo, el método de valoración sin depender del módulo Organización.
/// </summary>
public interface IConfiguracionEmpresa
{
    /// <summary>Método de valoración elegido por la empresa (por defecto, precio estándar).</summary>
    Task<MetodoValoracion> MetodoValoracionAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>
/// Valoración de artículos: devuelve el coste unitario de un artículo según el método de la empresa.
/// La implementa el módulo Inventario (conoce las entradas con coste) y la consumen Proyectos y
/// Producción.
/// </summary>
public interface IValoracionArticulos
{
    /// <summary>Coste unitario del artículo con el método indicado (o el de la empresa si se omite).</summary>
    Task<decimal> ValorUnitarioAsync(Guid empresaId, Guid productoId, MetodoValoracion? metodo = null, CancellationToken ct = default);
}
