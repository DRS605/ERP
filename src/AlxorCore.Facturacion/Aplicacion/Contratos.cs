using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Base y cuota de un tipo de impuesto (IVA o IGIC) en un periodo.</summary>
public sealed record DesgloseImpuestoDto(TipoImpuesto Impuesto, string CodigoIva, decimal Porcentaje, decimal Base, decimal Cuota);

/// <summary>Vista de una línea de factura.</summary>
public sealed record LineaFacturaDto(
    string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal PorcentajeDescuento,
    string CodigoIva, decimal PorcentajeIva, decimal Base, decimal CuotaIva,
    decimal CosteUnitario, decimal Margen, decimal PorcentajeRecargo, decimal CuotaRecargo, Guid? ProductoId = null, IReadOnlyList<ConceptoAplicado>? Conceptos = null, decimal ImporteConceptos = 0m, decimal CosteConceptos = 0m);

/// <summary>Datos de una línea de venta para el cálculo de márgenes (informe de beneficio).</summary>
public sealed record LineaMargenDto(Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Ingreso, decimal Coste);

/// <summary>Vista de una factura.</summary>
public sealed record FacturaDto(
    Guid Id,
    string NumeroCompleto,
    DateOnly FechaEmision,
    DateOnly FechaOperacion,
    DateOnly FechaVencimiento,
    Guid? ClienteId,
    string ClienteNombre,
    string? ClienteNif,
    string ClienteCalle,
    string ClienteCodigoPostal,
    string ClientePoblacion,
    string ClienteProvincia,
    string ClientePais,
    decimal BaseImponible,
    decimal CuotaIva,
    decimal PorcentajeIrpf,
    decimal RetencionIrpf,
    bool RecargoEquivalencia,
    decimal RecargoTotal,
    decimal Total,
    string Estado,
    string Tipo,
    string? Huella,
    string? HuellaAnterior,
    DateTimeOffset? FechaHoraGenRegistro,
    Guid? RectificaFacturaId,
    string? MotivoRectificacion,
    string? MotivoAnulacion,
    IReadOnlyList<LineaFacturaDto> Lineas,
    string? AvisoRiesgo = null,
    string? MencionFiscal = null,
    TipoImpuesto Impuesto = TipoImpuesto.Iva)
{
    /// <summary>Siglas del impuesto para mostrar en documentos ("IVA" o "IGIC").</summary>
    public string SiglasImpuesto => Impuesto.Siglas();

    public static FacturaDto Desde(Factura f) => new(
        f.Id, f.NumeroCompleto, f.FechaEmision, f.FechaOperacion, f.FechaVencimiento, f.ClienteId, f.ClienteNombre, f.ClienteNif,
        f.ClienteCalle, f.ClienteCodigoPostal, f.ClientePoblacion, f.ClienteProvincia, f.Pais,
        f.BaseImponible, f.CuotaIva, f.PorcentajeIrpf, f.RetencionIrpf, f.RecargoEquivalencia, f.RecargoTotal, f.Total, f.Estado.ToString(), f.TipoFactura.ToString(), f.Huella, f.HuellaAnterior,
        f.FechaHoraGenRegistro,
        f.RectificaFacturaId, f.MotivoRectificacion, f.MotivoAnulacion,
        f.Lineas.Select(l => new LineaFacturaDto(
            l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento, l.CodigoIva, l.PorcentajeIva, l.Base, l.CuotaIva,
            l.CosteUnitario, l.Margen, l.PorcentajeRecargo, l.CuotaRecargo, l.ProductoId, l.Conceptos, l.ImporteConceptos, l.CosteConceptos)).ToList(),
        MencionFiscal: f.MencionFiscal,
        Impuesto: f.Impuesto);
}

/// <summary>Resumen de factura para listados y libros de IVA.</summary>
public sealed record FacturaResumen(
    Guid Id, string NumeroCompleto, DateOnly FechaEmision, DateOnly FechaVencimiento, string ClienteNombre,
    string? ClienteNif, decimal BaseImponible, decimal CuotaIva, decimal RetencionIrpf, decimal Total, string Estado, string Tipo,
    Guid? ClienteId, Guid? ActividadNegocioId = null, TipoImpuesto Impuesto = TipoImpuesto.Iva);

/// <summary>
/// Filtros de búsqueda de facturas en servidor. Todos son opcionales (null = no filtra por ese
/// criterio). <paramref name="Texto"/> busca en número, nombre y NIF del cliente; <paramref name="Serie"/> es el
/// prefijo de la serie; <paramref name="Ids"/> restringe a esos documentos (lo usa el filtro por estado de cobro).
/// <paramref name="Orden"/> ordena la página: <c>fecha</c> (por defecto), <c>numero</c>, <c>cliente</c>, <c>base</c>,
/// <c>impuestos</c> o <c>total</c>, con <paramref name="Descendente"/> (por defecto, los más recientes primero).
/// </summary>
public sealed record FiltroFacturas(
    string? Texto = null,
    string? Estado = null,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    decimal? ImporteMin = null,
    decimal? ImporteMax = null,
    Guid? ClienteId = null,
    string? Serie = null,
    IReadOnlyCollection<Guid>? Ids = null,
    string? Orden = null,
    bool Descendente = true);

/// <summary>Datos mínimos de cada factura de un listado filtrado (todas las páginas), para calcular totales y saldos.</summary>
public sealed record FacturaFiltrada(Guid Id, string Estado, DateOnly FechaVencimiento, decimal BaseImponible, decimal CuotaIva, decimal RetencionIrpf, decimal Total);

/// <summary>Repositorio de facturas (escritura).</summary>
public interface IRepositorioFacturas
{
    Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(Factura factura);

    /// <summary>Huella del último registro VeriFactu de la empresa (para el encadenamiento), o null si es el primero.</summary>
    Task<string?> UltimaHuellaAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>
    /// Reserva el siguiente número de la serie (último + 1, sin huecos) para una factura con fecha
    /// <paramref name="fecha"/>. Bloquea la numeración de la empresa hasta que se guarden los cambios.
    /// Falla si la fecha es anterior a la de la última factura de la serie (la numeración debe ser
    /// correlativa también en fechas).
    /// </summary>
    Task<Resultado<NumeroFactura>> ReservarNumeroAsync(Guid empresaId, string? serie, DateOnly fecha, CancellationToken ct = default);
}

/// <summary>Consultas de lectura de facturas (las usan la API, Tesorería e Informes).</summary>
public interface IConsultaFacturas
{
    Task<FacturaDto?> ObtenerAsync(Guid facturaId, CancellationToken ct = default);

    Task<IReadOnlyList<FacturaResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    /// <summary>Búsqueda paginada y filtrada de facturas (el filtrado ocurre en la base de datos).</summary>
    Task<PaginaResultado<FacturaResumen>> BuscarAsync(Guid empresaId, FiltroFacturas filtro, Paginacion paginacion, CancellationToken ct = default);

    /// <summary>Todas las facturas que cumplen el filtro (sin paginar), con sus importes: base de los totales del listado.</summary>
    Task<IReadOnlyList<FacturaFiltrada>> FiltradasAsync(Guid empresaId, FiltroFacturas filtro, CancellationToken ct = default) =>
        throw new NotSupportedException("Esta consulta de facturas no calcula totales de listado.");

    /// <summary>Líneas de las facturas emitidas en un periodo, para el cálculo de márgenes.</summary>
    Task<IReadOnlyList<LineaMargenDto>> ListarLineasMargenAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    /// <summary>
    /// Bases y cuotas de las facturas emitidas en el periodo, agrupadas por impuesto (IVA/IGIC), código
    /// y porcentaje: la base de las autoliquidaciones (303, 420) y del cálculo de la prorrata.
    /// </summary>
    Task<IReadOnlyList<DesgloseImpuestoDto>> DesgloseImpuestoAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<DesgloseImpuestoDto>>([]);

    /// <summary>Conceptos de línea de las facturas emitidas en el periodo (no anuladas), con su documento.</summary>
    Task<IReadOnlyList<ConceptoDocumentoDto>> ListarConceptosAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<ConceptoDocumentoDto>>([]);
}

/// <summary>Concepto de línea de un documento, para los informes.</summary>
public sealed record ConceptoDocumentoDto(Guid DocumentoId, string Documento, DateOnly Fecha, string Tercero, string Linea, ConceptoAplicado Concepto);

/// <summary>Unidad de trabajo del módulo Facturación.</summary>
public interface IUnidadDeTrabajoFacturacion : IUnidadDeTrabajo;
