using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Informes.Aplicacion;

// ---------------------------------------------------------------------------
// Informes de cartera: extracto de un tercero (documentos y su pendiente) y
// aging de cobros (antigüedad del saldo pendiente por tramos de vencimiento).
// Cruzan los documentos (Facturación/Gastos) con lo liquidado en Tesorería.
// ---------------------------------------------------------------------------

/// <summary>Una línea del extracto de un tercero (un documento con su total, liquidado y pendiente).</summary>
public sealed record LineaExtractoDto(DateOnly Fecha, string Documento, decimal Total, decimal Liquidado, decimal Pendiente, string Estado);

/// <summary>Extracto de cuenta de un cliente (facturas y cobros) o proveedor (gastos y pagos).</summary>
public sealed record ExtractoTerceroDto(
    string Tipo, Guid TerceroId, string Nombre, DateOnly Desde, DateOnly Hasta,
    decimal TotalDocumentos, decimal TotalLiquidado, decimal Pendiente, IReadOnlyList<LineaExtractoDto> Lineas);

/// <summary>
/// Caso de uso: extracto de cuenta de un tercero. Para un <b>cliente</b> lista sus facturas del periodo
/// con lo cobrado y el pendiente; para un <b>proveedor</b>, sus gastos con lo pagado. El pendiente sale
/// de cruzar el total del documento con lo liquidado en Tesorería.
/// </summary>
public sealed class GenerarExtractoTercero
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaTesoreria _tesoreria;

    public GenerarExtractoTercero(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaTesoreria tesoreria)
    {
        _facturas = facturas;
        _gastos = gastos;
        _tesoreria = tesoreria;
    }

    public async Task<ExtractoTerceroDto> EjecutarAsync(Guid empresaId, string tipo, Guid terceroId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var esProveedor = string.Equals(tipo, "Proveedor", StringComparison.OrdinalIgnoreCase);
        var lineas = new List<LineaExtractoDto>();
        var nombre = "";

        if (esProveedor)
        {
            var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(g => g.ProveedorId == terceroId && g.Fecha >= desde && g.Fecha <= hasta)
                .OrderBy(g => g.Fecha).ToList();
            var liquidado = await _tesoreria.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Gasto, gastos.Select(g => g.Id).ToList(), ct).ConfigureAwait(false);
            nombre = gastos.Select(g => g.ProveedorTexto).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? "";
            foreach (var g in gastos)
            {
                var pagado = liquidado.TryGetValue(g.Id, out var l) ? l : 0m;
                lineas.Add(new LineaExtractoDto(g.Fecha, g.Concepto, g.Total, Redondeo.Dos(pagado), Redondeo.Dos(g.Total - pagado), g.Estado));
            }
        }
        else
        {
            var facturas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(f => f.ClienteId == terceroId && f.FechaEmision >= desde && f.FechaEmision <= hasta)
                .OrderBy(f => f.FechaEmision).ThenBy(f => f.NumeroCompleto).ToList();
            var liquidado = await _tesoreria.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Factura, facturas.Select(f => f.Id).ToList(), ct).ConfigureAwait(false);
            nombre = facturas.Select(f => f.ClienteNombre).FirstOrDefault() ?? "";
            foreach (var f in facturas)
            {
                var cobrado = liquidado.TryGetValue(f.Id, out var l) ? l : 0m;
                lineas.Add(new LineaExtractoDto(f.FechaEmision, f.NumeroCompleto, f.Total, Redondeo.Dos(cobrado), Redondeo.Dos(f.Total - cobrado), f.Estado));
            }
        }

        return new ExtractoTerceroDto(
            esProveedor ? "Proveedor" : "Cliente", terceroId, nombre, desde, hasta,
            Redondeo.Dos(lineas.Sum(l => l.Total)), Redondeo.Dos(lineas.Sum(l => l.Liquidado)), Redondeo.Dos(lineas.Sum(l => l.Pendiente)), lineas);
    }
}

/// <summary>Tramo de antigüedad del saldo pendiente de cobro.</summary>
public sealed record TramoAgingDto(string Tramo, int NumDocumentos, decimal Importe);

/// <summary>Aging (antigüedad) de la cartera de cobro pendiente, clasificada por tramos de vencimiento.</summary>
public sealed record AgingCarteraDto(DateOnly ALaFecha, decimal TotalPendiente, IReadOnlyList<TramoAgingDto> Tramos);

/// <summary>
/// Caso de uso: aging de la <b>cartera de cobro</b>. Toma las facturas con saldo pendiente (total −
/// cobrado &gt; 0) y las clasifica según los días transcurridos entre su fecha de vencimiento y hoy:
/// «Por vencer», «1-30», «31-60», «61-90» y «Más de 90» días vencidos. Es la foto de qué se debe y
/// desde cuándo, para priorizar el recobro.
/// </summary>
public sealed class GenerarAgingCartera
{
    private static readonly string[] Orden = ["Por vencer", "1-30 días", "31-60 días", "61-90 días", "Más de 90 días"];

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaTesoreria _tesoreria;
    private readonly IReloj _reloj;

    public GenerarAgingCartera(IConsultaFacturas facturas, IConsultaTesoreria tesoreria, IReloj reloj)
    {
        _facturas = facturas;
        _tesoreria = tesoreria;
        _reloj = reloj;
    }

    public async Task<AgingCarteraDto> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var facturas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(f => EstadosVenta.Cuenta(f.Estado))
            .ToList();
        var liquidado = await _tesoreria.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Factura, facturas.Select(f => f.Id).ToList(), ct).ConfigureAwait(false);

        var acumulado = new Dictionary<string, TramoAgingDto>();
        foreach (var f in facturas)
        {
            var cobrado = liquidado.TryGetValue(f.Id, out var l) ? l : 0m;
            var pendiente = Redondeo.Dos(f.Total - cobrado);
            if (pendiente <= 0m)
            {
                continue;
            }

            var tramo = Clasificar(hoy.DayNumber - f.FechaVencimiento.DayNumber);
            var prev = acumulado.TryGetValue(tramo, out var v) ? v : new TramoAgingDto(tramo, 0, 0m);
            acumulado[tramo] = prev with { NumDocumentos = prev.NumDocumentos + 1, Importe = Redondeo.Dos(prev.Importe + pendiente) };
        }

        var tramos = Orden
            .Select(t => acumulado.TryGetValue(t, out var v) ? v : new TramoAgingDto(t, 0, 0m))
            .ToList();

        return new AgingCarteraDto(hoy, Redondeo.Dos(tramos.Sum(t => t.Importe)), tramos);
    }

    private static string Clasificar(int diasVencido) => diasVencido switch
    {
        <= 0 => "Por vencer",
        <= 30 => "1-30 días",
        <= 60 => "31-60 días",
        <= 90 => "61-90 días",
        _ => "Más de 90 días",
    };
}
