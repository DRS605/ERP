using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Aplicacion;

/// <summary>Unidad de trabajo del módulo agro, con bloqueo para las numeraciones sin huecos.</summary>
public interface IUnidadDeTrabajoAgro : IUnidadDeTrabajo
{
    /// <summary>Bloqueo transaccional (hasta guardar) con el que «leer el último número y guardar el siguiente» no se cruza.</summary>
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

/// <summary>Saldo de una partida en un palé (null: kilos sueltos).</summary>
public sealed record SaldoPartida(Guid PartidaId, Guid? PaleId, decimal Kilos);

/// <summary>Persistencia del módulo agro.</summary>
public interface IRepositorioAgro
{
    void Agregar(object entidad);

    void Eliminar(object entidad);

    // Maestros
    Task<IReadOnlyList<Campana>> CampanasAsync(Guid empresaId, CancellationToken ct = default);
    Task<Campana?> CampanaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Agricultor>> AgricultoresAsync(Guid empresaId, CancellationToken ct = default);
    Task<Agricultor?> AgricultorAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExisteAgricultorAsync(Guid empresaId, Guid proveedorId, CancellationToken ct = default);
    Task<IReadOnlyList<Parcela>> ParcelasAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default);
    Task<Parcela?> ParcelaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Categoria>> CategoriasAsync(Guid empresaId, CancellationToken ct = default);
    Task<Categoria?> CategoriaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ArticuloCampana>> ArticulosCampanaAsync(Guid campanaId, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioLiquidacion>> PreciosAsync(Guid campanaId, CancellationToken ct = default);
    Task<PrecioLiquidacion?> PrecioAsync(Guid id, CancellationToken ct = default);
    Task<bool> PrecioEnUsoAsync(Guid precioId, CancellationToken ct = default);
    Task<IReadOnlyList<ConceptoLiquidacion>> ConceptosAsync(Guid empresaId, CancellationToken ct = default);
    Task<ConceptoLiquidacion?> ConceptoAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<TarifaCoste>> TarifasAsync(Guid empresaId, CancellationToken ct = default);
    Task<TarifaCoste?> TarifaAsync(Guid id, CancellationToken ct = default);
    Task<ConfiguracionAgro?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default);

    // Recepciones y partidas
    Task<Recepcion?> RecepcionAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Recepcion>> RecepcionesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? agricultorId, CancellationToken ct = default);
    Task<IReadOnlyList<Recepcion>> RecepcionesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<int> UltimoNumeroAsync(Guid empresaId, string serie, int ejercicio, CancellationToken ct = default);
    Task<Partida?> PartidaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Partida>> PartidasAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Partida>> PartidasDeRecepcionAsync(Guid recepcionId, CancellationToken ct = default);
    Task<IReadOnlyList<Partida>> PartidasConSaldoAsync(Guid empresaId, CancellationToken ct = default);
    Task<IReadOnlyList<SaldoPartida>> SaldosAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);
    Task<IReadOnlyList<SaldoPartida>> ContenidoPaleAsync(Guid paleId, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoPartida>> MovimientosDePaleAsync(Guid paleId, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoPartida>> MovimientosAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);

    /// <summary>¿Alguna de las partidas tiene movimientos aparte de su entrada?</summary>
    Task<bool> TieneMovimientosPosterioresAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid EnvaseProductoId, int Saldo)>> SaldoEnvasesAsync(Guid agricultorId, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoEnvase>> MovimientosEnvaseAsync(Guid agricultorId, CancellationToken ct = default);

    // Palés
    Task<Pale?> PaleAsync(Guid id, CancellationToken ct = default);
    Task<Pale?> PalePorSsccAsync(Guid empresaId, string sscc, CancellationToken ct = default);
    Task<IReadOnlyList<Pale>> PalesAsync(Guid empresaId, EstadoPale? estado, CancellationToken ct = default);
    Task<IReadOnlyList<Pale>> PalesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default);
    Task<int> PalesCreadosAsync(Guid empresaId, CancellationToken ct = default);

    // Clasificación y liquidación
    Task<IReadOnlyList<ClasificacionPartida>> ClasificacionesAsync(Guid partidaId, CancellationToken ct = default);
    Task<ClasificacionPartida?> ClasificacionAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ClasificacionPartida>> DefinitivasAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);
    Task<Liquidacion?> LiquidacionAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Liquidacion>> LiquidacionesAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default);

    /// <summary>Líneas de recepción que están en una liquidación no anulada (borrador o emitida).</summary>
    Task<IReadOnlySet<Guid>> LineasEnLiquidacionAsync(IReadOnlyCollection<Guid> lineaRecepcionIds, CancellationToken ct = default);

    /// <summary>Partidas que están en una liquidación no anulada.</summary>
    Task<bool> PartidasEnLiquidacionAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default);

    // Confección y trazabilidad
    Task<ParteConfeccion?> ParteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ParteConfeccion>> PartesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<Genealogia>> OrigenesAsync(IReadOnlyCollection<Guid> destinoIds, CancellationToken ct = default);
    Task<IReadOnlyList<Genealogia>> DestinosAsync(IReadOnlyCollection<Guid> origenIds, CancellationToken ct = default);
}

/// <summary>Datos de la autofactura de una liquidación, para registrarla como gasto.</summary>
public sealed record AutofacturaAgro(Guid ProveedorId, string Concepto, DateOnly Fecha, decimal BaseImponible, string CodigoImpuesto, decimal PorcentajeRetencion);

/// <summary>
/// Registro de la autofactura de una liquidación en Gastos (y, por la cola, en contabilidad y los libros de
/// IVA), y su anulación si no está pagada. Lo implementa la API sobre los módulos Gastos y Tesorería.
/// </summary>
public interface IAutofacturas
{
    Task<Resultado<(Guid GastoId, decimal Total)>> RegistrarAsync(Guid empresaId, AutofacturaAgro autofactura, CancellationToken ct = default);

    Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default);
}

/// <summary>Gastos e ingresos imputados en analítica a unos centros (con sus descendientes) en unas fechas.</summary>
public interface ICosteAnalitico
{
    /// <summary>Null si la empresa no tiene el módulo de analítica.</summary>
    Task<IReadOnlyDictionary<Guid, (decimal Gastos, decimal Ingresos)>?> PorCentroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
}
