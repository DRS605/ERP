using AlxorCore.Agro.Aplicacion;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// PDF de los documentos comerciales que no son factura: albarán de venta (valorado o sin precios), pedido de venta,
/// pedido de compra y liquidación al agricultor (la hoja que recibe el agricultor, como la de Hispatec). Cada uno se
/// traduce a un <see cref="DocumentoImpreso"/> y lo maqueta el generador común con la plantilla de la empresa.
/// </summary>
public sealed class ImpresosComerciales
{
    private readonly IGeneradorPdfDocumento _generador;
    private readonly IConsultaEmpresas _empresas;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProveedores _proveedores;
    private readonly ConsultarAlbaranesVenta _albaranes;
    private readonly ObtenerPedidoVenta _pedidosVenta;
    private readonly ObtenerPedido _pedidosCompra;
    private readonly LiquidacionesAgro _liquidaciones;
    private readonly IRepositorioAgro _agro;
    private readonly OrdenesCargaAgro? _ordenes;

    public ImpresosComerciales(IGeneradorPdfDocumento generador, IConsultaEmpresas empresas, IConsultaClientes clientes, IConsultaProveedores proveedores,
        ConsultarAlbaranesVenta albaranes, ObtenerPedidoVenta pedidosVenta, ObtenerPedido pedidosCompra, LiquidacionesAgro liquidaciones, IRepositorioAgro agro,
        OrdenesCargaAgro? ordenes = null)
    {
        _ordenes = ordenes;
        _generador = generador;
        _empresas = empresas;
        _clientes = clientes;
        _proveedores = proveedores;
        _albaranes = albaranes;
        _pedidosVenta = pedidosVenta;
        _pedidosCompra = pedidosCompra;
        _liquidaciones = liquidaciones;
        _agro = agro;
    }

    /// <summary>Albarán de venta; <paramref name="valorado"/> = false lo imprime sin precios (el que viaja con la mercancía).</summary>
    public async Task<Resultado<DocumentoPdf>> AlbaranAsync(Guid empresaId, Guid id, bool valorado, CancellationToken ct = default)
    {
        var a = await _albaranes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        var lineas = a.Lineas.OrderBy(l => l.Orden).Select(l => new LineaImpresa(l.Descripcion, l.Cantidad, null,
            valorado && l.PrecioFijado ? l.PrecioUnitario : null, l.PorcentajeDescuento, valorado && l.PrecioFijado ? l.Base : null,
            l.PrecioFijado || !valorado ? null : "Precio por fijar")).ToList();
        var datos = new List<(string, string)>();
        if (!string.IsNullOrWhiteSpace(a.Referencia)) datos.Add(("Referencia", a.Referencia));
        if (a.Anulado) datos.Add(("Estado", "ANULADO"));
        var totales = valorado ? new List<TotalImpreso> { new("Base", a.Base, Destacado: true) } : [];
        var doc = new DocumentoImpreso("Albarán", a.NumeroCompleto, a.Fecha, await ClienteAsync(a.ClienteId, a.ClienteNombre, ct).ConfigureAwait(false),
            lineas, totales, datos, a.Observaciones, valorado ? "Importes sin impuestos: el IVA se aplica en la factura." : null, Valorado: valorado);
        return await PdfAsync(empresaId, doc, $"albaran-{a.NumeroCompleto}", ct).ConfigureAwait(false);
    }

    public async Task<Resultado<DocumentoPdf>> PedidoVentaAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var p = await _pedidosVenta.EjecutarAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("pedidoventa.no_encontrado", "El pedido no existe."));
        }

        var lineas = p.Lineas.Select(l => new LineaImpresa(l.Descripcion, l.Cantidad, null, l.PrecioUnitario, l.PorcentajeDescuento, l.Base,
            l.CantidadServida > 0 ? $"Servido: {Redondeo.Formatear(l.CantidadServida, 3)}" : null)).ToList();
        var doc = new DocumentoImpreso("Pedido de venta", p.NumeroCompleto, p.Fecha, await ClienteAsync(p.ClienteId, p.ClienteNombre, ct).ConfigureAwait(false),
            lineas, [new TotalImpreso("Base", Redondeo.Dos(p.Lineas.Sum(l => l.Base)), Destacado: true)], [("Estado", p.Estado)],
            Leyenda: "Confirmación de pedido. Importes sin impuestos.");
        return await PdfAsync(empresaId, doc, $"pedido-{p.NumeroCompleto}", ct).ConfigureAwait(false);
    }

    public async Task<Resultado<DocumentoPdf>> PedidoCompraAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var p = await _pedidosCompra.EjecutarAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("pedidocompra.no_encontrado", "El pedido no existe."));
        }

        var proveedor = p.ProveedorId is { } pid ? await _proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false) : null;
        var tercero = new TerceroImpreso("Proveedor", proveedor?.Nombre ?? p.ProveedorTexto, proveedor?.NifFiscal,
            proveedor is null ? null : Direccion(proveedor.Calle, proveedor.CodigoPostal, proveedor.Poblacion, proveedor.Provincia));
        var lineas = p.Lineas.Select(l => new LineaImpresa(l.Descripcion, l.Cantidad, null, l.PrecioUnitario, null, l.Importe)).ToList();
        var doc = new DocumentoImpreso("Pedido de compra", p.NumeroCompleto, p.Fecha, tercero, lineas,
            [new TotalImpreso("Total", p.Total, Destacado: true)], [("Estado", p.Estado)], Leyenda: "Rogamos confirmen la recepción de este pedido.");
        return await PdfAsync(empresaId, doc, $"pedido-compra-{p.NumeroCompleto}", ct).ConfigureAwait(false);
    }

    /// <summary>Liquidación al agricultor: entregas con kilos, precio e importe, descuentos, impuesto (o compensación REAGP), retención y líquido.</summary>
    public async Task<Resultado<DocumentoPdf>> LiquidacionAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var l = await _liquidaciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("liquidacion.no_encontrada", "La liquidación no existe."));
        }

        var agricultor = await _agro.AgricultorAsync(l.AgricultorId, ct).ConfigureAwait(false);
        var proveedor = agricultor is null ? null : await _proveedores.ObtenerAsync(agricultor.ProveedorId, ct).ConfigureAwait(false);
        var tercero = new TerceroImpreso("Agricultor", proveedor?.Nombre ?? l.Agricultor ?? "—", proveedor?.NifFiscal,
            proveedor is null ? null : Direccion(proveedor.Calle, proveedor.CodigoPostal, proveedor.Poblacion, proveedor.Provincia));
        var lineas = l.Lineas.Select(x => new LineaImpresa($"{x.Recepcion} · {x.FechaRecepcion:dd/MM/yyyy}{(x.Categoria is null ? "" : " · " + x.Categoria)}",
            x.Kilos, "kg", x.PrecioKg, null, x.Importe)).ToList();
        var reagp = string.Equals(l.Regimen, "Reagp", StringComparison.OrdinalIgnoreCase);
        var totales = new List<TotalImpreso> { new($"Fruta ({Redondeo.Formatear(l.Kilos, l.Kilos == decimal.Round(l.Kilos) ? 0 : 3)} kg)", l.Bruto) };
        totales.AddRange(l.Descuentos.Select(d => new TotalImpreso(d.Nombre, -d.Importe)));
        totales.Add(new TotalImpreso("Base", l.BaseImponible));
        totales.Add(new TotalImpreso(reagp ? $"Compensación REAGP ({Redondeo.Formatear(l.PorcentajeImpuesto)} %)" : $"Impuesto ({Redondeo.Formatear(l.PorcentajeImpuesto)} %)", l.CuotaImpuesto));
        if (l.Retencion != 0) totales.Add(new TotalImpreso($"Retención IRPF ({Redondeo.Formatear(l.PorcentajeRetencion)} %)", -l.Retencion));
        totales.Add(new TotalImpreso("Líquido a pagar", l.APagar, Destacado: true));
        var doc = new DocumentoImpreso(reagp ? "Liquidación · recibo de compensación" : "Liquidación", l.Numero ?? "Borrador", l.Fecha, tercero, lineas, totales,
            [("Periodo", $"{l.Desde:dd/MM/yyyy} – {l.Hasta:dd/MM/yyyy}"), ("Estado", l.Estado)],
            Leyenda: reagp
                ? "Régimen Especial de la Agricultura, Ganadería y Pesca (art. 124 y ss. LIVA). Autofactura emitida por el destinatario por cuenta del agricultor."
                : "Autofactura emitida por el destinatario por cuenta del proveedor (art. 5 del Reglamento de facturación).",
            TituloCantidad: "Kilos");
        return await PdfAsync(empresaId, doc, $"liquidacion-{l.Numero ?? "borrador"}", ct).ConfigureAwait(false);
    }

    /// <summary>Hoja de carga: palés por línea de pedido con su SSCC y posición, y el esquema del camión si se indicaron filas y columnas.</summary>
    public async Task<Resultado<DocumentoPdf>> HojaCargaAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var o = _ordenes is null ? null : await _ordenes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("ordencarga.no_encontrada", "La orden de carga no existe."));
        }

        var lineas = new List<LineaImpresa>();
        foreach (var l in o.Lineas)
        {
            var pales = o.Cargados.Where(c => c.LineaId == l.Id).ToList();
            lineas.Add(new LineaImpresa($"{l.Orden}. {l.Pedido} · {l.Cliente} · {l.Descripcion}", l.PalesPrevistos, "palés", null, null, null,
                pales.Count == 0 ? "Sin cargar" : string.Join(" · ", pales.Select(c => c.Sscc + (c.Fila is { } f ? $" ({f}-{c.Columna})" : "")))));
        }

        var datos = new List<(string, string)> { ("Estado", o.Estado), ("Fecha de carga", o.FechaCarga.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)) };
        if (o.Muelle is { } m) datos.Add(("Muelle", m));
        if (o.Matricula is { } mat) datos.Add(("Matrícula", mat));
        if (o.Conductor is { } con) datos.Add(("Conductor", con));
        if (o.TemperaturaConsigna is { } t) datos.Add(("Temperatura", $"{Redondeo.Formatear(t, 1)} ºC"));
        datos.Add(("Palés cargados", $"{o.Cargados.Count} · {Redondeo.Formatear(o.Cargados.Sum(c => c.Kilos), 0)} kg"));
        string? esquema = null;
        if (o.Filas is { } filas && o.Columnas is { } columnas)
        {
            var filasTexto = Enumerable.Range(1, filas).Select(f => $"Fila {f}: " + string.Join(" | ", Enumerable.Range(1, columnas)
                .Select(c => o.Cargados.FirstOrDefault(p => p.Fila == f && p.Columna == c)?.Sscc[^6..] ?? "—")));
            esquema = "Distribución del camión (últimas 6 cifras del SSCC; fila 1 junto a la cabina): " + string.Join("  ·  ", filasTexto);
        }

        var tercero = new TerceroImpreso("Destinatarios", string.Join(", ", o.Lineas.Select(l => l.Cliente).Distinct()), null, null);
        var doc = new DocumentoImpreso("Hoja de carga", o.Numero, o.FechaCarga, tercero, lineas,
            [], datos,
            o.Observaciones, esquema, TituloCantidad: "Previstos", Valorado: false);
        return await PdfAsync(empresaId, doc, $"hoja-carga-{o.Numero}", ct).ConfigureAwait(false);
    }

    private async Task<TerceroImpreso> ClienteAsync(Guid clienteId, string nombre, CancellationToken ct)
    {
        var c = clienteId == Guid.Empty ? null : await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
        return new TerceroImpreso("Cliente", c?.Nombre ?? nombre, c?.NifFiscal, c is null ? null : Direccion(c.Calle, c.CodigoPostal, c.Poblacion, c.Provincia));
    }

    private static string? Direccion(string? calle, string? cp, string? poblacion, string? provincia)
    {
        var loc = string.Join(" ", new[] { cp, poblacion }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(provincia) && !string.IsNullOrWhiteSpace(loc)) loc += $" ({provincia})";
        var partes = new[] { calle, loc }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return partes.Count == 0 ? null : string.Join(", ", partes);
    }

    private async Task<Resultado<DocumentoPdf>> PdfAsync(Guid empresaId, DocumentoImpreso doc, string nombre, CancellationToken ct)
    {
        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        return Resultado.Ok(new DocumentoPdf($"{nombre.Replace('/', '-').Replace(' ', '-')}.pdf", _generador.Generar(doc, empresa)));
    }
}
