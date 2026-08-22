namespace AlxorCore.Nucleo.Comun;

/// <summary>
/// Método de valoración de existencias/consumos, elegido a nivel de empresa en la implantación.
/// Se aplica en la valoración de inventario, en los consumos de proyectos y en los partes de
/// producción, para que el coste sea coherente en todo el ERP.
/// </summary>
public enum MetodoValoracion
{
    /// <summary>Precio estándar: el coste fijado en la ficha del artículo (precio de compra).</summary>
    Estandar = 1,

    /// <summary>Precio de la última compra registrada.</summary>
    UltimaCompra = 2,

    /// <summary>Precio medio ponderado (PMP) de las entradas.</summary>
    Pmp = 3,

    /// <summary>FIFO: primeras entradas, primeras salidas (valora con las capas de entrada más antiguas).</summary>
    Fifo = 4,
}
