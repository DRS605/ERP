using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Dice si un registro maestro (cliente, artículo, almacén…) ya se ha usado en algún documento de cualquier
/// empresa del grupo. Los módulos se referencian entre sí por identificador, sin claves foráneas entre esquemas,
/// así que ningún módulo puede saberlo solo: lo implementa la API sobre el mapa de referencias de la base de datos.
/// </summary>
public interface IComprobadorUso
{
    /// <summary>Descripción del primer uso encontrado («facturas de venta», «pedidos de compra»…) o null si no se ha usado.</summary>
    Task<string?> BuscarUsoAsync(string tipoRegistro, Guid id, CancellationToken ct = default);
}

/// <summary>Tipos de registro maestro que se pueden eliminar o dar de baja.</summary>
public static class TiposRegistro
{
    public const string Cliente = "cliente";
    public const string Proveedor = "proveedor";
    public const string Producto = "producto";
    public const string Almacen = "almacen";
    public const string Ubicacion = "ubicacion";
    public const string Tarifa = "tarifa";
    public const string Serie = "serie";
    public const string CentroCoste = "centro_coste";
    public const string PartidaAnalitica = "partida_analitica";
    public const string ClaveReparto = "clave_reparto";
    public const string Campana = "campana";
    public const string Categoria = "categoria";
    public const string Parcela = "parcela";
    public const string Agricultor = "agricultor";
    public const string TarifaCoste = "tarifa_coste";
    public const string Actividad = "actividad_negocio";

    /// <summary>Centro de trabajo de la empresa (tienda, delegación…), no el centro de coste analítico.</summary>
    public const string Centro = "centro";

    /// <summary>Gasto generado por otro documento (se anula desde ese documento).</summary>
    public const string Gasto = "gasto";
}

/// <summary>Reglas comunes para eliminar un maestro: solo si no se ha usado; si se ha usado, se da de baja.</summary>
public static class Bajas
{
    /// <summary>Código de error (sufijo) que la interfaz reconoce para ofrecer la baja en lugar del borrado.</summary>
    public const string SufijoEnUso = ".en_uso";

    public static Error EnUso(string prefijo, string nombre, string uso) =>
        Error.Conflicto(prefijo + SufijoEnUso,
            $"No se puede eliminar {nombre} porque ya tiene {uso}. Puedes darlo de baja: dejará de ofrecerse en las altas nuevas y se conserva su histórico.");
}

/// <summary>Resultado de eliminar o dar de baja un maestro.</summary>
public sealed record BajaDto(Guid Id, bool Eliminado, bool Activo);
