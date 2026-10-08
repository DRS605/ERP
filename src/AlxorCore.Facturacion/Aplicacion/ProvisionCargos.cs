using System.Security.Cryptography;
using System.Text;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Cargos con acreedor de los albaranes ya liquidados en una factura del acreedor no anulada.</summary>
public interface ICargosAcreedorLiquidados
{
    Task<IReadOnlyList<(Guid AlbaranId, Guid ConceptoId)>> DeAlbaranesAsync(IReadOnlyCollection<Guid> albaranes, CancellationToken ct = default);
}

/// <summary>
/// Provisión del coste con acreedor (portes al transportista, comisión del comisionista…): al emitir la factura de venta,
/// el coste se contabiliza ya, contra «Acreedores, facturas pendientes de recibir» (4009), con su cuenta de gasto (la del
/// concepto o, sin ella, 629). Cuando llega la factura del acreedor, sus cargos provisionados cancelan la 4009 en vez de
/// cargar otra vez el gasto. Un cargo del albarán que ya se liquidó antes de facturarlo no se provisiona.
/// </summary>
public static class ProvisionCargos
{
    public const string Origen = "ProvisionCargo";
    public const string CuentaProvision = "4009";
    public const string CuentaGastoPorDefecto = "629";

    /// <summary>Si un concepto aplicado es un coste con acreedor (lo que se provisiona).</summary>
    public static bool Provisionable(ConceptoAplicado c) =>
        c.Efecto == EfectoConcepto.Coste && c.AcreedorId is not null && c.Importe != 0m;

    /// <summary>Marca como provisionados los costes con acreedor de las líneas, salvo los del albarán ya liquidados.</summary>
    public static async Task<List<NuevaLinea>> MarcarAsync(ICargosAcreedorLiquidados? liquidados, List<NuevaLinea> lineas, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (!lineas.Any(l => (l.Conceptos ?? []).Any(Provisionable)))
        {
            return lineas;
        }

        var albaranes = lineas.Where(l => l.AlbaranVentaId is not null).Select(l => l.AlbaranVentaId!.Value).Distinct().ToList();
        var yaLiquidados = liquidados is null || albaranes.Count == 0
            ? []
            : (await liquidados.DeAlbaranesAsync(albaranes, ct).ConfigureAwait(false)).ToHashSet();
        return lineas.Select(l => (l.Conceptos ?? []).Any(Provisionable)
            ? l with
            {
                Conceptos = l.Conceptos!.Select(c => Provisionable(c) && !(l.AlbaranVentaId is { } a && yaLiquidados.Contains((a, c.ConceptoId)))
                    ? c with { Provisionado = true }
                    : c).ToList(),
            }
            : l).ToList();
    }

    /// <summary>Asientos de provisión de la factura: uno por acreedor y cuenta de gasto (gasto al debe, 4009 al haber).</summary>
    public static IEnumerable<DocumentoContabilizable> Documentos(Factura f)
    {
        ArgumentNullException.ThrowIfNull(f);
        return f.Lineas.SelectMany(l => l.Conceptos).Where(c => c.Provisionado)
            .GroupBy(c => (Acreedor: c.AcreedorId!.Value, Cuenta: string.IsNullOrWhiteSpace(c.CuentaContable) ? CuentaGastoPorDefecto : c.CuentaContable!))
            .Select(g => (g.Key, Importe: Redondeo.Dos(g.Sum(c => c.Importe)), Nombres: string.Join(", ", g.Select(c => c.Nombre).Distinct())))
            .Where(x => x.Importe != 0m)
            .Select(x => new DocumentoContabilizable(
                SentidoContable.Cobro, Origen, Derivado(f.Id, $"{x.Key.Acreedor:N}:{x.Key.Cuenta}"), Recortar($"Provisión {x.Nombres} · {f.NumeroCompleto}"),
                x.Key.Acreedor, string.Empty, f.FechaEmision, 0m, string.Empty, 0m, 0m, 0m, Math.Abs(x.Importe),
                Anulacion: x.Importe < 0m, CuentaTesoreria: x.Key.Cuenta, CuentaTercero: CuentaProvision));
    }

    private static string Recortar(string texto) => texto.Length > 80 ? texto[..80] : texto;

    /// <summary>Id determinista de un asiento derivado de un documento (el mismo siempre para el mismo documento y etiqueta).</summary>
    private static Guid Derivado(Guid id, string etiqueta)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{id:N}|{etiqueta}"));
        var g = bytes.AsSpan(0, 16).ToArray();
        g[7] = (byte)((g[7] & 0x0F) | 0x50);
        g[8] = (byte)((g[8] & 0x3F) | 0x80);
        return new Guid(g);
    }
}
