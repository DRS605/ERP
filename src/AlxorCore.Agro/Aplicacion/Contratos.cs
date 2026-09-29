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
public sealed record SaldoPartida(Guid PartidaId, Guid? PaleId, decimal Kilos, int Cajas = 0);

/// <summary>Reservas de palés a líneas de pedidos de venta.</summary>
public interface IRepositorioReservas
{
    void Agregar(ReservaPale reserva);
    Task<ReservaPale?> ReservaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaPale>> DePedidoAsync(Guid pedidoVentaId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaPale>> ActivasDePalesAsync(IReadOnlyCollection<Guid> paleIds, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaPale>> DePaleAsync(Guid paleId, CancellationToken ct = default);
    Task<IReadOnlyList<ReservaPale>> ActivasAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Cuentas de envases por tercero y su libro de movimientos.</summary>
public interface IRepositorioEnvases
{
    void Agregar(object entidad);
    Task<CuentaEnvases?> CuentaAsync(Guid id, CancellationToken ct = default);
    Task<CuentaEnvases?> CuentaDeAsync(Guid empresaId, TipoCuentaEnvases tipo, Guid terceroId, CancellationToken ct = default);
    Task<IReadOnlyList<CuentaEnvases>> CuentasAsync(Guid empresaId, CancellationToken ct = default);
    Task<IReadOnlyList<(Guid CuentaId, Guid EnvaseProductoId, int Saldo)>> SaldosAsync(Guid empresaId, DateOnly? hasta, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoEnvases>> MovimientosAsync(Guid empresaId, Guid? cuentaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<MovimientoEnvases?> MovimientoAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<MovimientoEnvases>> DeDocumentoAsync(Guid documentoId, CancellationToken ct = default);
    Task<bool> AnuladoAsync(Guid movimientoId, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
    Task<ConfiguracionEnvases?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Fecha del último movimiento de cada cuenta (la que cuenta el saldo).</summary>
    Task<IReadOnlyDictionary<Guid, DateOnly>> UltimosMovimientosAsync(Guid empresaId, CancellationToken ct = default);
}

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
    Task<IReadOnlyList<RendimientoConfeccion>> RendimientosAsync(Guid empresaId, CancellationToken ct = default);
    Task<RendimientoConfeccion?> RendimientoAsync(Guid id, CancellationToken ct = default);
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
    Task<IReadOnlyList<Pale>> PalesDeCartaPorteAsync(Guid cartaPorteId, CancellationToken ct = default);
    Task<IReadOnlyList<Pale>> PalesDeAlbaranAsync(Guid albaranId, CancellationToken ct = default);
    Task<IReadOnlyList<PlantillaPale>> PlantillasPaleAsync(Guid empresaId, CancellationToken ct = default);
    Task<PlantillaPale?> PlantillaPaleAsync(Guid id, CancellationToken ct = default);
    Task<bool> PlantillaPaleEnUsoAsync(Guid plantillaId, CancellationToken ct = default);

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

/// <summary>Carta de porte de una expedición de palés: destinatario, transporte y una línea por producto.</summary>
public sealed record CartaPorteExpedicion(Guid ClienteId, DateOnly Fecha, string? Transportista, string? Matricula, string? LugarOrigen, string? LugarDestino,
    string? Observaciones, IReadOnlyList<(string Descripcion, int Bultos, decimal Kilos)> Lineas, Guid? AlbaranId = null,
    Guid? TransportistaId = null, Guid? VehiculoId = null, decimal? TemperaturaConsigna = null, string? Termografo = null,
    IReadOnlyList<(Guid? ProductoId, int Pales)>? Detalle = null);

/// <summary>
/// Documentos de transporte de una expedición: emite la carta de porte (y la anula si la expedición no se completa o se
/// deshace). Lo implementa la API sobre el módulo de facturación.
/// </summary>
public interface IDocumentosExpedicion
{
    Task<Resultado<(Guid Id, string Numero)>> EmitirCartaPorteAsync(Guid empresaId, CartaPorteExpedicion carta, CancellationToken ct = default);

    Task<Resultado> AnularCartaPorteAsync(Guid cartaPorteId, string motivo, CancellationToken ct = default);

    /// <summary>Cliente del pedido de venta (null si el pedido no existe).</summary>
    Task<Guid?> ClienteDePedidoAsync(Guid pedidoVentaId, CancellationToken ct = default) => Task.FromResult<Guid?>(null);

    /// <summary>Líneas del pedido de venta (null si no existe): artículo, cantidad pedida y servida, y si está en un estado que admite reservas.</summary>
    Task<PedidoParaReservas?> PedidoParaReservasAsync(Guid pedidoVentaId, CancellationToken ct = default) => Task.FromResult<PedidoParaReservas?>(null);

    /// <summary>Pedidos confirmados con algo pendiente de servir (para montar órdenes de carga).</summary>
    Task<IReadOnlyList<PedidoParaReservas>> PedidosPendientesAsync(Guid empresaId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<PedidoParaReservas>>([]);

    /// <summary>Albarán de venta del pedido con lo expedido (kilos o cajas según la unidad del artículo).</summary>
    Task<Resultado<(Guid Id, string Numero)>> EmitirAlbaranAsync(Guid empresaId, AlbaranExpedicion albaran, CancellationToken ct = default) =>
        Task.FromResult(Resultado.Fallo<(Guid, string)>(Error.Validacion("expedicion.sin_albaran", "No se pueden emitir albaranes.")));

    Task<Resultado> AnularAlbaranAsync(Guid albaranId, string motivo, CancellationToken ct = default) => Task.FromResult(Resultado.Ok());

    /// <summary>
    /// Vuelta de solo algunos palés de un albarán: una devolución de venta con lo que traen (kilos o cajas según la unidad
    /// del artículo), que corrige el albarán sin anularlo. Devuelve el número de la devolución.
    /// </summary>
    Task<Resultado<string?>> DevolverParcialAsync(Guid empresaId, Guid albaranId, IReadOnlyList<(Guid ProductoId, decimal Kilos, int Cajas)> lineas, string motivo,
        CancellationToken ct = default) => Task.FromResult(Resultado.Ok<string?>(null));

    /// <summary>Anula las devoluciones que dejaron las vueltas parciales de palés del albarán (antes de anularlo porque han vuelto todos).</summary>
    Task<Resultado> AnularDevolucionesDeVueltaAsync(Guid empresaId, Guid albaranId, CancellationToken ct = default) => Task.FromResult(Resultado.Ok());

    /// <summary>Albarán de venta directo (sin pedido): los envases que se le facturan a un cliente. Sin precio, el de su tarifa o el del artículo.</summary>
    Task<Resultado<(Guid Id, string Numero)>> EmitirAlbaranDirectoAsync(Guid empresaId, Guid clienteId, DateOnly fecha, string? referencia,
        IReadOnlyList<(Guid ProductoId, decimal Cantidad, decimal? Precio)> lineas, CancellationToken ct = default) =>
        Task.FromResult(Resultado.Fallo<(Guid, string)>(Error.Validacion("expedicion.sin_albaran", "No se pueden emitir albaranes.")));
}

/// <summary>Pedido de venta visto desde las reservas de palés.</summary>
public sealed record PedidoParaReservas(Guid Id, string Numero, Guid ClienteId, string Cliente, bool Abierto, IReadOnlyList<LineaPedidoParaReservas> Lineas);

public sealed record LineaPedidoParaReservas(Guid Id, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Servida);

/// <summary>Albarán de venta de una expedición: pedido y, por producto, los kilos y las cajas expedidos.</summary>
/// <summary>Albarán de lo expedido. <paramref name="CosteKg"/>: coste por kilo de cada producto según sus palés (el de las partidas cargadas).</summary>
public sealed record AlbaranExpedicion(Guid PedidoVentaId, DateOnly Fecha, string? Referencia, IReadOnlyList<(Guid ProductoId, decimal Kilos, int Cajas)> Lineas,
    IReadOnlyDictionary<Guid, decimal>? CosteKg = null);

/// <summary>Gastos e ingresos imputados en analítica a unos centros (con sus descendientes) en unas fechas.</summary>
public interface ICosteAnalitico
{
    /// <summary>Null si la empresa no tiene el módulo de analítica.</summary>
    Task<IReadOnlyDictionary<Guid, (decimal Gastos, decimal Ingresos)>?> PorCentroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
}

/// <summary>Movimiento de existencias en el inventario del almacén: + entrada, − salida.</summary>
public sealed record MovimientoInventarioAgro(Guid ProductoId, decimal Cantidad, string Motivo);

/// <summary>
/// Inventario del almacén (Catálogo e Inventario): Agro refleja en él lo que mueven sus partidas y sus envases. Los
/// artículos sin control de stock se ignoran. Devuelve los avisos (p. ej. una salida sin existencias en el almacén).
/// </summary>
public interface IInventarioAgro
{
    Task<IReadOnlyList<string>> MoverAsync(Guid empresaId, IReadOnlyList<MovimientoInventarioAgro> movimientos, CancellationToken ct = default);
}

/// <summary>Impuesto indirecto de la empresa (IVA, o IGIC en Canarias): decide el impuesto de las autofacturas a agricultores.</summary>
public interface IImpuestoEmpresaAgro
{
    Task<AlxorCore.Nucleo.Comun.TipoImpuesto> ImpuestoAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Ventas del artículo en unas fechas (kilos y su importe a precio de venta, sin conceptos): base de la liquidación a resultas.</summary>
public interface IVentasAgro
{
    Task<(decimal Kilos, decimal Importe)> VentasAsync(Guid productoId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);
}
