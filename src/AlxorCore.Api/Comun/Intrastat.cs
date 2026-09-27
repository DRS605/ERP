using System.Text;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Flujo de Intrastat: expediciones (salidas a otro Estado miembro) o introducciones (llegadas).</summary>
public enum FlujoIntrastat
{
    Expedicion = 1,
    Introduccion = 2,
}

/// <summary>
/// Línea de la declaración Intrastat, con los campos de la declaración estadística: Estado miembro, provincia, condiciones
/// de entrega (Incoterm), naturaleza de la transacción, modalidad de transporte, código de mercancías (NC, 8 dígitos),
/// país de origen, régimen estadístico, masa neta, importe facturado y valor estadístico, y el NIF-IVA de la
/// contraparte (obligatorio en las expediciones).
/// </summary>
public sealed record LineaIntrastatDto(string EstadoMiembro, string? Provincia, string? CondicionesEntrega, string NaturalezaTransaccion, string ModalidadTransporte,
    string? CodigoMercancias, string? PaisOrigen, string RegimenEstadistico, decimal? MasaNetaKg, decimal ImporteFacturado, decimal ValorEstadistico, string? NifContraparte,
    IReadOnlyList<string> Documentos);

public sealed record IntrastatDto(int Anio, int Mes, string Flujo, IReadOnlyList<LineaIntrastatDto> Lineas, decimal TotalImporte, decimal? TotalMasaNetaKg,
    decimal AcumuladoAnual, decimal Umbral, bool SuperaUmbral, IReadOnlyList<string> Avisos);

/// <summary>
/// Intrastat (declaración estadística del comercio de bienes entre Estados miembros de la UE), mensual:
/// <list type="bullet">
/// <item><b>expediciones</b>: las facturas emitidas del mes con IVA intracomunitario (entregas exentas del art. 25) a
/// clientes de otro Estado miembro;</item>
/// <item><b>introducciones</b>: los albaranes de compra del mes de proveedores de otro Estado miembro (la llegada de la
/// mercancía), valorados al precio del pedido.</item>
/// </list>
/// Agrupa por los campos de la declaración y avisa de lo que falta (código NC, NIF-IVA, peso). El umbral de obligación
/// es anual por flujo; se muestra el acumulado del año hasta el mes.
/// </summary>
public sealed class Intrastat
{
    /// <summary>Umbral de obligación de declarar en España, por flujo (euros al año).</summary>
    public const decimal UmbralAnual = 400_000m;

    private readonly IConsultaFacturasPorIva _porIva;
    private readonly IConsultaFacturas _facturas;
    private readonly IRepositorioTiposIva _tiposIva;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IConsultaProductos _productos;
    private readonly IConsultaCartasPorte _cartas;
    private readonly IConsultaEmpresas _empresas;
    private readonly IRepositorioAlbaranes _albaranesCompra;
    private readonly IRepositorioPedidos _pedidosCompra;

    public Intrastat(IConsultaFacturasPorIva porIva, IConsultaFacturas facturas, IRepositorioTiposIva tiposIva, IConsultaClientes clientes, IConsultaProveedores proveedores,
        IConsultaProductos productos, IConsultaCartasPorte cartas, IConsultaEmpresas empresas, IRepositorioAlbaranes albaranesCompra, IRepositorioPedidos pedidosCompra)
    {
        _porIva = porIva; _facturas = facturas; _tiposIva = tiposIva; _clientes = clientes; _proveedores = proveedores; _productos = productos; _cartas = cartas;
        _empresas = empresas; _albaranesCompra = albaranesCompra; _pedidosCompra = pedidosCompra;
    }

    private sealed record Movimiento(string EstadoMiembro, string? Incoterm, string Modo, string? Nc, string? PaisOrigen, decimal? Masa, decimal Importe, string? Nif, string Documento);

    public async Task<IntrastatDto> DeclaracionAsync(Guid empresaId, int anio, int mes, FlujoIntrastat flujo, CancellationToken ct = default)
    {
        var desde = new DateOnly(anio, mes, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);
        var avisos = new List<string>();
        var movimientos = await MovimientosAsync(empresaId, desde, hasta, flujo, avisos, ct).ConfigureAwait(false);
        var acumulado = (await MovimientosAsync(empresaId, new DateOnly(anio, 1, 1), hasta, flujo, [], ct).ConfigureAwait(false)).Sum(m => m.Importe);

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var provincia = Provincia(empresa?.CodigoPostal);
        if (provincia is null)
        {
            avisos.Add("La empresa no tiene código postal español: falta la provincia.");
        }

        var lineas = movimientos
            .GroupBy(m => (m.EstadoMiembro, m.Incoterm, m.Modo, m.Nc, m.PaisOrigen, m.Nif))
            .Select(g =>
            {
                var importe = Redondeo.Dos(g.Sum(m => m.Importe));
                decimal? masa = g.All(m => m.Masa is not null) ? Math.Round(g.Sum(m => m.Masa!.Value), 0, MidpointRounding.AwayFromZero) : null;
                // Valor estadístico: el facturado (sin ajuste por los gastos de transporte hasta la frontera).
                return new LineaIntrastatDto(g.Key.EstadoMiembro, provincia, g.Key.Incoterm, "11", g.Key.Modo, g.Key.Nc, g.Key.PaisOrigen, "1", masa, importe, importe,
                    flujo == FlujoIntrastat.Expedicion ? g.Key.Nif : null, g.Select(m => m.Documento).Distinct(StringComparer.Ordinal).ToList());
            })
            .OrderBy(l => l.EstadoMiembro, StringComparer.Ordinal).ThenBy(l => l.CodigoMercancias ?? "~", StringComparer.Ordinal).ToList();

        return new IntrastatDto(anio, mes, flujo.ToString(), lineas, Redondeo.Dos(lineas.Sum(l => l.ImporteFacturado)),
            lineas.All(l => l.MasaNetaKg is not null) ? lineas.Sum(l => l.MasaNetaKg!.Value) : null, Redondeo.Dos(acumulado), UmbralAnual, acumulado >= UmbralAnual,
            avisos.Distinct(StringComparer.Ordinal).ToList());
    }

    private async Task<List<Movimiento>> MovimientosAsync(Guid empresaId, DateOnly desde, DateOnly hasta, FlujoIntrastat flujo, List<string> avisos, CancellationToken ct) =>
        flujo == FlujoIntrastat.Expedicion
            ? await ExpedicionesAsync(empresaId, desde, hasta, avisos, ct).ConfigureAwait(false)
            : await IntroduccionesAsync(empresaId, desde, hasta, avisos, ct).ConfigureAwait(false);

    private async Task<List<Movimiento>> ExpedicionesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, List<string> avisos, CancellationToken ct)
    {
        var codigos = (await _tiposIva.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(t => t.Clase == ClaseIva.Intracomunitario).Select(t => t.Codigo)
            .Append("INTRA").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var resultado = new List<Movimiento>();
        foreach (var fila in (await _porIva.ConCodigosAsync(empresaId, codigos, ct).ConfigureAwait(false)).Where(f => f.Fecha >= desde && f.Fecha <= hasta))
        {
            var f = await _facturas.ObtenerAsync(fila.Id, ct).ConfigureAwait(false);
            if (f is null || f.Tipo != nameof(TipoFactura.Ordinaria))
            {
                if (f is not null)
                {
                    avisos.Add($"{f.NumeroCompleto} es rectificativa: las devoluciones van con otra naturaleza de la transacción (no se incluye).");
                }

                continue;
            }

            var cliente = f.ClienteId is { } c ? await _clientes.ObtenerAsync(c, ct).ConfigureAwait(false) : null;
            var pais = Paises.Codigo(cliente?.Pais ?? f.ClientePais);
            if (pais is null || pais == "ES" || !Paises.UnionEuropea.Contains(pais))
            {
                avisos.Add($"{f.NumeroCompleto}: el cliente no es de otro Estado miembro ({pais ?? "sin país"}).");
                continue;
            }

            var nif = cliente?.NifIva;
            if (string.IsNullOrWhiteSpace(nif))
            {
                avisos.Add($"{f.NumeroCompleto}: el cliente no tiene NIF-IVA (obligatorio en las expediciones).");
            }

            var carta = (await _cartas.DeFacturaAsync(f.Id, ct).ConfigureAwait(false)).FirstOrDefault(x => !x.Anulada);
            var modo = Modalidad(carta?.Transporte?.Modo);
            var incoterm = carta?.Transporte?.Incoterm ?? cliente?.Incoterm;
            foreach (var l in f.Lineas.Where(l => codigos.Contains(l.CodigoIva)))
            {
                var (nc, origen, masa) = await MercanciaAsync(l.ProductoId, l.Cantidad, l.Descripcion, f.NumeroCompleto, avisos, ct).ConfigureAwait(false);
                resultado.Add(new Movimiento(pais, incoterm, modo, nc, origen, masa, l.Base, nif, f.NumeroCompleto));
            }
        }

        return resultado;
    }

    private async Task<List<Movimiento>> IntroduccionesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, List<string> avisos, CancellationToken ct)
    {
        var resultado = new List<Movimiento>();
        var proveedores = new Dictionary<Guid, ProveedorDto?>();
        foreach (var a in await _albaranesCompra.EnPeriodoAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
        {
            var pedido = await _pedidosCompra.ObtenerPorIdAsync(a.PedidoId, ct).ConfigureAwait(false);
            if (pedido?.ProveedorId is not { } pid)
            {
                continue;
            }

            if (!proveedores.TryGetValue(pid, out var proveedor))
            {
                proveedor = await _proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false);
                proveedores[pid] = proveedor;
            }

            var pais = Paises.Codigo(proveedor?.Pais);
            if (pais is null || pais == "ES" || !Paises.UnionEuropea.Contains(pais))
            {
                continue;
            }

            foreach (var l in a.Lineas)
            {
                var precio = pedido.Lineas.FirstOrDefault(x => x.Id == l.LineaPedidoId)?.PrecioUnitario ?? 0m;
                var (nc, origen, masa) = await MercanciaAsync(l.ProductoId, l.Cantidad, l.Descripcion, $"Albarán {a.NumeroCompleto}", avisos, ct).ConfigureAwait(false);
                resultado.Add(new Movimiento(pais, null, "3", nc, origen ?? pais, masa, Redondeo.Dos(precio * l.Cantidad), proveedor?.NifIva, $"Albarán de compra {a.NumeroCompleto}"));
            }
        }

        return resultado;
    }

    /// <summary>Código NC (los 8 primeros dígitos del arancelario), país de origen y masa neta de una línea.</summary>
    private async Task<(string? Nc, string? Origen, decimal? Masa)> MercanciaAsync(Guid? productoId, decimal cantidad, string descripcion, string documento,
        List<string> avisos, CancellationToken ct)
    {
        var p = productoId is { } id ? await _productos.ObtenerAsync(id, ct).ConfigureAwait(false) : null;
        var nc = p?.CodigoArancelario is { Length: >= 8 } c ? c[..8] : null;
        if (nc is null)
        {
            avisos.Add($"{documento}: «{descripcion}» no tiene código arancelario.");
        }

        decimal? masa = p is null ? null
            : string.Equals(p.Unidad, "kg", StringComparison.OrdinalIgnoreCase) ? cantidad
            : p.PesoKg is { } pk ? pk * cantidad : null;
        if (masa is null)
        {
            avisos.Add($"{documento}: «{descripcion}» no tiene peso (ficha del artículo).");
        }

        return (nc, p?.PaisOrigen, masa);
    }

    /// <summary>Modalidad de transporte de Intrastat: 1 marítimo, 2 ferrocarril, 3 carretera, 4 aéreo.</summary>
    private static string Modalidad(ModoTransporte? modo) => modo switch
    {
        ModoTransporte.Maritimo => "1",
        ModoTransporte.Ferrocarril => "2",
        ModoTransporte.Aereo => "4",
        _ => "3",
    };

    /// <summary>Provincia (código INE de dos dígitos): los dos primeros del código postal español.</summary>
    public static string? Provincia(string? codigoPostal)
    {
        var cp = (codigoPostal ?? string.Empty).Trim();
        return cp.Length == 5 && cp.All(char.IsAsciiDigit) && int.Parse(cp[..2], System.Globalization.CultureInfo.InvariantCulture) is >= 1 and <= 52 ? cp[..2] : null;
    }

    /// <summary>La declaración en CSV (punto y coma, coma decimal), una fila por línea.</summary>
    public static string Csv(IntrastatDto d)
    {
        ArgumentNullException.ThrowIfNull(d);
        var n = Redondeo.NumeroEspanol;
        var sb = new StringBuilder();
        sb.AppendLine(d.Flujo == nameof(FlujoIntrastat.Expedicion)
            ? "Estado miembro destino;Provincia origen;Condiciones entrega;Naturaleza transacción;Modalidad transporte;Código mercancías;País origen;Régimen estadístico;Masa neta kg;Unidades suplementarias;Importe facturado;Valor estadístico;NIF-IVA destinatario"
            : "Estado miembro procedencia;Provincia destino;Condiciones entrega;Naturaleza transacción;Modalidad transporte;Código mercancías;País origen;Régimen estadístico;Masa neta kg;Unidades suplementarias;Importe facturado;Valor estadístico");
        foreach (var l in d.Lineas)
        {
            var campos = new List<string?>
            {
                l.EstadoMiembro, l.Provincia, l.CondicionesEntrega, l.NaturalezaTransaccion, l.ModalidadTransporte, l.CodigoMercancias, l.PaisOrigen, l.RegimenEstadistico,
                l.MasaNetaKg?.ToString("0", n), string.Empty, l.ImporteFacturado.ToString("0.00", n), l.ValorEstadistico.ToString("0.00", n),
            };
            if (d.Flujo == nameof(FlujoIntrastat.Expedicion))
            {
                campos.Add(l.NifContraparte);
            }

            sb.AppendLine(string.Join(';', campos.Select(c => c ?? string.Empty)));
        }

        return sb.ToString();
    }
}
