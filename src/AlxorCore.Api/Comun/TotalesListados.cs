using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Totales de <b>todo</b> el resultado filtrado de un listado paginado (no solo de la página): documentos,
/// base, impuestos, retenciones y total sin los anulados; pendiente y vencido de los documentos vivos.
/// </summary>
public sealed record TotalesListado(
    int Documentos, decimal BaseImponible, decimal Impuestos, decimal Retenciones, decimal Total,
    decimal Pendiente, decimal Vencido, int DocumentosVencidos);

/// <summary>
/// Página de un listado con los totales del resultado completo y el pendiente de cobro/pago de cada documento
/// de la página. Conserva los campos de <see cref="PaginaResultado{T}"/> (compatibilidad con los clientes existentes).
/// </summary>
public sealed record PaginaConTotales<T>(
    IReadOnlyList<T> Elementos, int Total, int Pagina, int TamanoPagina, int TotalPaginas, bool HayMas,
    TotalesListado Totales, IReadOnlyDictionary<Guid, decimal> Pendientes)
{
    public static PaginaConTotales<T> Desde(PaginaResultado<T> pagina, TotalesListado totales, IReadOnlyDictionary<Guid, decimal> pendientes)
    {
        ArgumentNullException.ThrowIfNull(pagina);
        return new PaginaConTotales<T>(pagina.Elementos, pagina.Total, pagina.Pagina, pagina.TamanoPagina, pagina.TotalPaginas, pagina.HayMas, totales, pendientes);
    }
}

/// <summary>Documento de un listado filtrado con sus importes (factura emitida o recibida).</summary>
public sealed record DocumentoListado(Guid Id, string Estado, DateOnly Vencimiento, decimal BaseImponible, decimal Impuestos, decimal Retenciones, decimal Total);

/// <summary>
/// Cálculo de totales y del filtro por estado de cobro/pago de los listados de facturas y gastos. El
/// pendiente sale de Tesorería (lo liquidado por cobros y pagos), igual que en las pantallas de cobros y pagos.
/// </summary>
public static class TotalesListados
{
    /// <summary>Valores admitidos del filtro <c>cobro</c>: pendiente (incluye vencidas), vencida o liquidada (cobrada/pagada).</summary>
    public static readonly IReadOnlyList<string> EstadosCobro = ["pendiente", "vencida", "cobrada", "pagada", "liquidada"];

    private static bool Anulado(string estado) => estado is "Anulada" or "Anulado";

    /// <summary>Vivo a efectos de cobro/pago: ni anulado ni sustituido por una rectificativa.</summary>
    private static bool Vivo(string estado) => !Anulado(estado) && estado != "Rectificada";

    /// <summary>Valida el filtro de estado de cobro (vacío = sin filtro).</summary>
    public static Error? ValidarCobro(string? cobro) =>
        string.IsNullOrWhiteSpace(cobro) || EstadosCobro.Contains(cobro.Trim().ToLowerInvariant())
            ? null
            : Error.Validacion("listado.cobro_invalido", "Estado de cobro no válido: usa pendiente, vencida o cobrada.");

    /// <summary>
    /// Pendiente de cada documento vivo (total − liquidado en Tesorería) y, si se pide <paramref name="cobro"/>,
    /// los identificadores de los que cumplen ese estado.
    /// </summary>
    public static async Task<(IReadOnlyDictionary<Guid, decimal> Pendientes, IReadOnlyCollection<Guid>? Filtrados)> SaldosAsync(
        IReadOnlyList<DocumentoListado> documentos, TipoDocumentoTesoreria tipo, string? cobro, DateOnly hoy, IConsultaTesoreria tesoreria, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(documentos);
        ArgumentNullException.ThrowIfNull(tesoreria);
        var liquidado = documentos.Count == 0
            ? new Dictionary<Guid, decimal>()
            : await tesoreria.LiquidadoPorDocumentosAsync(tipo, documentos.Select(d => d.Id).ToList(), ct).ConfigureAwait(false);
        var pendientes = documentos.ToDictionary(d => d.Id, d => Vivo(d.Estado) ? Math.Round(d.Total - liquidado.GetValueOrDefault(d.Id), 2) : 0m);

        if (string.IsNullOrWhiteSpace(cobro))
        {
            return (pendientes, null);
        }

        var c = cobro.Trim().ToLowerInvariant();
        var filtrados = documentos.Where(d => Vivo(d.Estado) && c switch
        {
            "pendiente" => pendientes[d.Id] > 0m,
            "vencida" => pendientes[d.Id] > 0m && d.Vencimiento < hoy,
            _ => pendientes[d.Id] <= 0m, // cobrada / pagada / liquidada
        }).Select(d => d.Id).ToList();
        return (pendientes, filtrados);
    }

    /// <summary>Totales de los documentos (ya filtrados).</summary>
    public static TotalesListado Calcular(IEnumerable<DocumentoListado> documentos, IReadOnlyDictionary<Guid, decimal> pendientes, DateOnly hoy)
    {
        ArgumentNullException.ThrowIfNull(pendientes);
        var sumables = documentos.Where(d => !Anulado(d.Estado)).ToList();
        var vencidos = sumables.Where(d => pendientes.GetValueOrDefault(d.Id) > 0m && d.Vencimiento < hoy).ToList();
        return new TotalesListado(
            sumables.Count, sumables.Sum(d => d.BaseImponible), sumables.Sum(d => d.Impuestos), sumables.Sum(d => d.Retenciones), sumables.Sum(d => d.Total),
            sumables.Sum(d => Math.Max(0m, pendientes.GetValueOrDefault(d.Id))), vencidos.Sum(d => pendientes.GetValueOrDefault(d.Id)), vencidos.Count);
    }
}
