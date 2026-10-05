using AlxorCore.Bodega.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Bodega.Aplicacion;

public interface IUnidadDeTrabajoBodega : IUnidadDeTrabajo
{
    /// <summary>Bloqueo transaccional (hasta guardar) para numerar y para no cruzar operaciones sobre los mismos depósitos.</summary>
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

/// <summary>Movimiento del libro de la bodega (una línea de una operación viva).</summary>
public sealed record MovimientoLibroBodega(Guid OperacionId, DateOnly Fecha, TipoOperacionBodega Tipo, ClaseLineaBodega Clase, Guid DepositoId, decimal Litros,
    ProductoVinico Producto, string? Calificacion);

public interface IRepositorioBodega
{
    void Agregar(object entidad);

    void Eliminar(object entidad);

    Task<IReadOnlyList<Deposito>> DepositosAsync(Guid empresaId, CancellationToken ct = default);

    Task<IReadOnlyList<Deposito>> DepositosAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<Deposito?> DepositoAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteDepositoAsync(Guid empresaId, string codigo, CancellationToken ct = default);

    Task<bool> DepositoConOperacionesAsync(Guid depositoId, CancellationToken ct = default);

    Task<IReadOnlyList<EntradaUva>> EntradasAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? viticultorId, CancellationToken ct = default);

    Task<IReadOnlyList<EntradaUva>> EntradasAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);

    Task<EntradaUva?> EntradaAsync(Guid id, CancellationToken ct = default);

    /// <summary>Entradas elaboradas en estas operaciones.</summary>
    Task<IReadOnlyList<EntradaUva>> EntradasDeOperacionesAsync(IReadOnlyCollection<Guid> operacionIds, CancellationToken ct = default);

    Task<int> SiguienteNumeroEntradaAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    Task<IReadOnlyList<PrecioUva>> PreciosAsync(Guid empresaId, int? anada, CancellationToken ct = default);

    Task<PrecioUva?> PrecioAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OperacionBodega>> OperacionesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);

    Task<IReadOnlyList<OperacionBodega>> OperacionesDeDepositoAsync(Guid depositoId, CancellationToken ct = default);

    Task<OperacionBodega?> OperacionAsync(Guid id, CancellationToken ct = default);

    Task<int> SiguienteNumeroOperacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Líneas de las operaciones vivas hasta una fecha (incluida).</summary>
    Task<IReadOnlyList<MovimientoLibroBodega>> LibroAsync(Guid empresaId, DateOnly hasta, CancellationToken ct = default);

    Task<IReadOnlyList<LiquidacionUva>> LiquidacionesAsync(Guid empresaId, Guid? viticultorId, CancellationToken ct = default);

    Task<LiquidacionUva?> LiquidacionAsync(Guid id, CancellationToken ct = default);

    Task<int> SiguienteNumeroLiquidacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

/// <summary>Existencias del artículo embotellado (Catálogo e Inventario). Lo implementa la API.</summary>
public interface IExistenciasBodega
{
    Task<Resultado> EntrarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default);

    Task<Resultado> SacarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default);
}

/// <summary>Albarán de venta a granel (Facturación). Lo implementa la API.</summary>
public interface IVentasBodega
{
    Task<Resultado<(Guid Id, string Numero)>> AlbaranAsync(Guid empresaId, Guid clienteId, DateOnly fecha, string referencia, Guid productoId, decimal cantidad, decimal? precio,
        CancellationToken ct = default);

    Task<Resultado> AnularAlbaranAsync(Guid albaranId, string motivo, CancellationToken ct = default);
}

/// <summary>Autofactura de la uva comprada al viticultor (un gasto con la compensación REAGP o el IVA y la retención). Lo implementa la API.</summary>
public interface IAutofacturasBodega
{
    Task<Resultado<(Guid GastoId, decimal Total)>> RegistrarAsync(Guid empresaId, Guid proveedorId, string concepto, DateOnly fecha, decimal baseImponible, string codigoImpuesto,
        decimal porcentajeRetencion, CancellationToken ct = default);

    Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default);
}
