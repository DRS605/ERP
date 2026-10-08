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
    private readonly AlxorCore.Tesoreria.Aplicacion.LiquidacionesPagos? _liquidacionesPagos;
    private readonly EnvasesTerceros? _envases;
    private readonly IIdiomaDocumentos? _idiomas;

    public ImpresosComerciales(IGeneradorPdfDocumento generador, IConsultaEmpresas empresas, IConsultaClientes clientes, IConsultaProveedores proveedores,
        ConsultarAlbaranesVenta albaranes, ObtenerPedidoVenta pedidosVenta, ObtenerPedido pedidosCompra, LiquidacionesAgro liquidaciones, IRepositorioAgro agro,
        OrdenesCargaAgro? ordenes = null, AlxorCore.Tesoreria.Aplicacion.LiquidacionesPagos? liquidacionesPagos = null, EnvasesTerceros? envases = null,
        IIdiomaDocumentos? idiomas = null)
    {
        _idiomas = idiomas;
        _envases = envases;
        _ordenes = ordenes;
        _liquidacionesPagos = liquidacionesPagos;
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
    public async Task<Resultado<DocumentoPdf>> AlbaranAsync(Guid empresaId, Guid id, bool valorado, string? idioma, CancellationToken ct = default)
    {
        var a = await _albaranes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        var i = await IdiomaAsync(idioma, () => _idiomas!.DeClienteAsync(a.ClienteId, ct)).ConfigureAwait(false);
        string T(string clave) => TextosImpreso.T(i, clave);
        var nombres = await TraduccionLineas.NombresAsync(_idiomas, a.Lineas.Select(l => l.ProductoId), i, ct).ConfigureAwait(false);
        var lineas = a.Lineas.OrderBy(l => l.Orden).Select(l => new LineaImpresa(TraduccionLineas.Descripcion(l.Descripcion, l.ProductoId, nombres), l.Cantidad, null,
            valorado && l.PrecioFijado ? l.PrecioUnitario : null, l.PorcentajeDescuento, valorado && l.PrecioFijado ? l.Base : null,
            l.PrecioFijado || !valorado ? null : T("Precio por fijar"))).ToList();
        var datos = new List<(string, string)>();
        if (!string.IsNullOrWhiteSpace(a.Referencia)) datos.Add((T("Referencia"), a.Referencia));
        if (a.Anulado) datos.Add((T("Estado"), T("ANULADO")));
        var totales = valorado ? new List<TotalImpreso> { new(T("Base"), a.Base, Destacado: true) } : [];
        var doc = new DocumentoImpreso(T("Albarán"), a.NumeroCompleto, a.Fecha, await ClienteAsync(a.ClienteId, a.ClienteNombre, ct, i).ConfigureAwait(false),
            lineas, totales, datos, a.Observaciones, valorado ? T("Leyenda albarán") : null, Valorado: valorado, Idioma: i, Moneda: a.Moneda);
        return await PdfAsync(empresaId, doc, $"albaran-{a.NumeroCompleto}", ct).ConfigureAwait(false);
    }

    public async Task<Resultado<DocumentoPdf>> PedidoVentaAsync(Guid empresaId, Guid id, string? idioma, CancellationToken ct = default)
    {
        var p = await _pedidosVenta.EjecutarAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("pedidoventa.no_encontrado", "El pedido no existe."));
        }

        var i = await IdiomaAsync(idioma, () => _idiomas!.DeClienteAsync(p.ClienteId, ct)).ConfigureAwait(false);
        string T(string clave) => TextosImpreso.T(i, clave);
        var nombres = await TraduccionLineas.NombresAsync(_idiomas, p.Lineas.Select(l => l.ProductoId), i, ct).ConfigureAwait(false);
        var lineas = p.Lineas.Select(l => new LineaImpresa(TraduccionLineas.Descripcion(l.Descripcion, l.ProductoId, nombres), l.Cantidad, null, l.PrecioUnitario,
            l.PorcentajeDescuento, l.Base, l.CantidadServida > 0 ? $"{T("Servido")}: {TextosImpreso.Numero(i, l.CantidadServida, 3)}" : null)).ToList();
        // El estado (borrador, confirmado…) solo sale en castellano: es un dato interno.
        var doc = new DocumentoImpreso(T("Pedido de venta"), p.NumeroCompleto, p.Fecha, await ClienteAsync(p.ClienteId, p.ClienteNombre, ct, i).ConfigureAwait(false),
            lineas, [new TotalImpreso(T("Base"), Redondeo.Dos(p.Lineas.Sum(l => l.Base)), Destacado: true)], i == IdiomasDocumento.Castellano ? [("Estado", p.Estado)] : [],
            Leyenda: T("Leyenda pedido venta"), Idioma: i, Moneda: p.Moneda);
        return await PdfAsync(empresaId, doc, $"pedido-{p.NumeroCompleto}", ct).ConfigureAwait(false);
    }

    public async Task<Resultado<DocumentoPdf>> PedidoCompraAsync(Guid empresaId, Guid id, string? idioma, CancellationToken ct = default)
    {
        var p = await _pedidosCompra.EjecutarAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("pedidocompra.no_encontrado", "El pedido no existe."));
        }

        var proveedor = p.ProveedorId is { } pid ? await _proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false) : null;
        var i = IdiomasDocumento.Efectivo(idioma ?? proveedor?.Idioma);
        string T(string clave) => TextosImpreso.T(i, clave);
        var tercero = new TerceroImpreso(T("Proveedor"), proveedor?.Nombre ?? p.ProveedorTexto, proveedor?.NifFiscal,
            proveedor is null ? null : Direccion(proveedor.Calle, proveedor.CodigoPostal, proveedor.Poblacion, proveedor.Provincia));
        var nombres = await TraduccionLineas.NombresAsync(_idiomas, p.Lineas.Select(l => l.ProductoId), i, ct).ConfigureAwait(false);
        var lineas = p.Lineas.Select(l => new LineaImpresa(TraduccionLineas.Descripcion(l.Descripcion, l.ProductoId, nombres), l.Cantidad, null, l.PrecioUnitario, null, l.Importe)).ToList();
        var doc = new DocumentoImpreso(T("Pedido de compra"), p.NumeroCompleto, p.Fecha, tercero, lineas,
            p.Suplidos == 0m ? [new TotalImpreso(T("Total"), p.Total, Destacado: true)]
                : [new TotalImpreso(T("Base"), p.Total), new TotalImpreso(T("Suplidos"), p.Suplidos), new TotalImpreso(T("Total"), Redondeo.Dos(p.Total + p.Suplidos), Destacado: true)],
            i == IdiomasDocumento.Castellano ? [("Estado", p.Estado)] : [], Leyenda: T("Leyenda pedido compra"), Idioma: i);
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

    /// <summary>
    /// Liquidación de pagos al proveedor o agricultor (el impreso de <c>LiquidacionImprimir</c> de Hispatec): facturas
    /// liquidadas, entregas a cuenta canceladas, cobros compensados, intereses, retención y el líquido con su forma de pago.
    /// </summary>
    public async Task<Resultado<DocumentoPdf>> LiquidacionPagosAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var r = _liquidacionesPagos is null ? null : await _liquidacionesPagos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null || r.EsFallo)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("liquidacionpagos.no_encontrada", "La liquidación de pagos no existe."));
        }

        var l = r.Valor;
        var proveedor = await _proveedores.ObtenerAsync(l.ProveedorId, ct).ConfigureAwait(false);
        var tercero = new TerceroImpreso("Proveedor", proveedor?.Nombre ?? l.Proveedor ?? "—", proveedor?.NifFiscal,
            proveedor is null ? null : Direccion(proveedor.Calle, proveedor.CodigoPostal, proveedor.Poblacion, proveedor.Provincia));
        static string Tipo(string t) => t switch
        {
            "EntregaCuenta" => "Entrega a cuenta cancelada",
            "Compensacion" => "Compensado con sus facturas de cliente",
            "CobroCompensado" => "Cobro compensado",
            "Intereses" => "Intereses de entregas a cuenta",
            "Retencion" => "Retención en el pago",
            "Pagare" => "Pagado con pagaré",
            _ => "Pagado",
        };
        var lineas = l.Lineas.Select(x => new LineaImpresa($"{x.Documento} · {Tipo(x.Tipo)}", 1m, null, null, null, x.Tipo == "CobroCompensado" ? x.Importe : -x.Importe))
            .Prepend(new LineaImpresa("Facturas pendientes liquidadas", 1m, null, null, null, l.APagar)).ToList();
        var totales = new List<TotalImpreso> { new("Facturas liquidadas", l.APagar) };
        if (l.EntregasCanceladas != 0) totales.Add(new TotalImpreso("Entregas a cuenta", -l.EntregasCanceladas));
        if (l.Compensado != 0) totales.Add(new TotalImpreso("Compensado", -l.Compensado));
        if (l.Intereses != 0) totales.Add(new TotalImpreso("Intereses de entregas", -l.Intereses));
        if (l.Retencion != 0) totales.Add(new TotalImpreso($"Retención ({Redondeo.Formatear(l.PorcentajeRetencion)} %)", -l.Retencion));
        totales.Add(new TotalImpreso("Líquido", l.Liquido, Destacado: true));
        var forma = l.FormaPago switch
        {
            "Directo" => "Pagado por banco",
            "Remesa" => "Por transferencia (remesa)",
            "Pagare" => "Pagaré a fecha",
            _ => "Pendiente de pago",
        };
        var doc = new DocumentoImpreso("Liquidación de pagos", l.Numero, l.Fecha, tercero, lineas, totales,
            [("Facturas hasta", l.Hasta.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture)), ("Forma de pago", forma), ("Estado", l.Estado)],
            Leyenda: $"Líquido a percibir: {Redondeo.Formatear(l.Liquido)} €.", TituloCantidad: " ");
        return await PdfAsync(empresaId, doc, $"liquidacion-pagos-{l.Numero}", ct).ConfigureAwait(false);
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

    /// <summary>Justificante de un movimiento de envases: lo entregado y lo recogido, para firmar el tercero.</summary>
    public async Task<Resultado<DocumentoPdf>> JustificanteEnvasesAsync(Guid empresaId, Guid movimientoId, CancellationToken ct = default)
    {
        var m = _envases is null ? null : await _envases.ObtenerMovimientoAsync(movimientoId, ct).ConfigureAwait(false);
        if (m is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("envases.movimiento", "El movimiento de envases no existe."));
        }

        var cuenta = (await _envases!.CuentasAsync(empresaId, null, ct).ConfigureAwait(false)).FirstOrDefault(c => c.Id == m.CuentaSolicitadaId)
            ?? (await _envases.CuentasAsync(empresaId, null, ct).ConfigureAwait(false)).FirstOrDefault(c => c.Id == m.CuentaId);
        var tercero = await TerceroEnvasesAsync(cuenta, m.Cuenta, ct).ConfigureAwait(false);
        var lineas = m.Lineas.Select(l => new LineaImpresa(l.Envase, Math.Abs(l.Cantidad), "uds", Detalle: l.Cantidad > 0 ? "Entregado" : "Recogido")).ToList();
        var datos = new List<(string, string)> { ("Origen", m.Origen), ("Neto", $"{m.Lineas.Sum(l => l.Cantidad):+#;-#;0}") };
        if (m.Matricula is { } mat) datos.Add(("Matrícula", mat));
        if (m.Anulado) datos.Add(("Estado", "Anulado"));
        if (m.CuentaId != m.CuentaSolicitadaId) datos.Add(("Se lleva en", m.Cuenta));
        var doc = new DocumentoImpreso("Justificante de envases", m.Numero, m.Fecha, tercero, lineas, [], datos, m.Observaciones,
            "Entregado: envases que se lleva el tercero. Recogido: envases que devuelve. Firma del tercero:", TituloCantidad: "Envases", Valorado: false);
        return await PdfAsync(empresaId, doc, $"envases-{m.Numero}", ct).ConfigureAwait(false);
    }

    /// <summary>Extracto de envases de una cuenta en PDF: saldo inicial, movimientos con el acumulado y saldo final por envase.</summary>
    public async Task<Resultado<DocumentoPdf>> ExtractoEnvasesAsync(Guid empresaId, Guid cuentaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        if (_envases is null)
        {
            return Resultado.Fallo<DocumentoPdf>(Error.NoEncontrado("envases.cuenta", "La cuenta de envases no existe."));
        }

        var r = await _envases.ExtractoAsync(empresaId, cuentaId, desde, hasta, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DocumentoPdf>(r.Error);
        }

        var e = r.Valor;
        var tercero = await TerceroEnvasesAsync(e.Cuenta, e.Cuenta.Nombre, ct).ConfigureAwait(false);
        var lineas = new List<LineaImpresa> { new("Saldo inicial", e.SaldoInicial, "uds", Detalle: string.Join(" · ", e.SaldoInicialPorEnvase.Select(s => $"{s.Envase}: {s.Saldo}"))) };
        lineas.AddRange(e.Movimientos.Select(x => new LineaImpresa(
            $"{x.Movimiento.Fecha:dd/MM/yyyy} · {x.Movimiento.Numero} · {x.Movimiento.Origen}{(x.Movimiento.Anulado ? " (anulado)" : "")}", x.Neto, "uds",
            Detalle: string.Join(" · ", x.Movimiento.Lineas.Select(l => $"{l.Envase} {l.Cantidad:+#;-#;0}")) + $" · acumulado {x.Acumulado}")));
        var datos = new List<(string, string)>
        {
            ("Desde", e.Desde?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "el inicio"),
            ("Hasta", e.Hasta?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture) ?? "hoy"),
            ("Saldo final", e.SaldoFinal.ToString(System.Globalization.CultureInfo.InvariantCulture)),
        };
        var doc = new DocumentoImpreso("Extracto de envases", e.Cuenta.Nombre, e.Hasta ?? DateOnly.FromDateTime(DateTime.UtcNow), tercero, lineas, [], datos,
            null, "Saldo final por envase: " + (e.SaldoFinalPorEnvase.Count == 0 ? "sin envases." : string.Join(" · ", e.SaldoFinalPorEnvase.Select(s => $"{s.Envase}: {s.Saldo}"))),
            TituloCantidad: "Envases", Valorado: false);
        return await PdfAsync(empresaId, doc, $"extracto-envases-{e.Cuenta.Nombre}", ct).ConfigureAwait(false);
    }

    private async Task<TerceroImpreso> TerceroEnvasesAsync(CuentaEnvasesDto? cuenta, string nombre, CancellationToken ct)
    {
        switch (cuenta?.Tipo)
        {
            case "Cliente":
                return await ClienteAsync(cuenta.TerceroId, nombre, ct).ConfigureAwait(false);
            case "Proveedor" or "Pool":
                var p = await _proveedores.ObtenerAsync(cuenta.TerceroId, ct).ConfigureAwait(false);
                return new TerceroImpreso(cuenta.Tipo == "Pool" ? "Pool" : "Proveedor", p?.Nombre ?? nombre, p?.NifFiscal,
                    p is null ? null : Direccion(p.Calle, p.CodigoPostal, p.Poblacion, p.Provincia));
            default:
                return new TerceroImpreso(cuenta?.Tipo ?? "Tercero", nombre);
        }
    }

    /// <summary>El idioma pedido o, si no, el del tercero (castellano sin puerto de idiomas).</summary>
    private async Task<string> IdiomaAsync(string? pedido, Func<Task<string>> delTercero) =>
        IdiomasDocumento.Efectivo(pedido ?? (_idiomas is null ? null : await delTercero().ConfigureAwait(false)));

    private async Task<TerceroImpreso> ClienteAsync(Guid clienteId, string nombre, CancellationToken ct, string? idioma = null)
    {
        var c = clienteId == Guid.Empty ? null : await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
        return new TerceroImpreso(TextosImpreso.T(idioma, "Cliente"), c?.Nombre ?? nombre, c?.NifFiscal, c is null ? null : Direccion(c.Calle, c.CodigoPostal, c.Poblacion, c.Provincia));
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

        return Resultado.Ok(new DocumentoPdf($"{nombre.Replace('/', '-').Replace(' ', '-')}.pdf", _generador.Generar(doc, empresa), IdiomasDocumento.Efectivo(doc.Idioma), doc.Numero));
    }
}
