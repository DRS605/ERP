using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

// ----------------------------------------------------------------------------- Comandos
/// <summary>
/// Línea de un albarán directo. Con artículo y sin precio, el de la tarifa del cliente (o el del artículo). Con
/// <c>PrecioPorFijar</c>, el precio indicado (o el de tarifa) es solo una estimación y el albarán queda por valorar.
/// </summary>
public sealed record LineaAlbaranComando(decimal Cantidad, string? Descripcion = null, decimal? PrecioUnitario = null, string? CodigoIva = null,
    decimal PorcentajeDescuento = 0m, Guid? ProductoId = null, bool PrecioPorFijar = false, IReadOnlyList<ConceptoSolicitado>? Conceptos = null,
    decimal? Bultos = null, decimal? Pales = null);

/// <summary>Albarán directo. Sin conceptos en una línea se ponen los automáticos del cliente y el artículo (sus reglas).</summary>
public sealed record CrearAlbaranVentaComando(Guid ClienteId, IReadOnlyList<LineaAlbaranComando> Lineas, DateOnly? Fecha = null, string? Referencia = null,
    string? Observaciones = null, IReadOnlyList<ConceptoSolicitado>? ConceptosDocumento = null, AlxorCore.Nucleo.Comun.TipoImpuesto? Impuesto = null, Guid? CentroId = null);

/// <summary>Precio definitivo de una línea (por su número de orden en el albarán).</summary>
public sealed record PrecioLineaAlbaranComando(int Orden, decimal PrecioUnitario, decimal? PorcentajeDescuento = null);

public sealed record ValorarAlbaranVentaComando(IReadOnlyList<PrecioLineaAlbaranComando> Lineas);

public sealed record FacturarAlbaranesComando(IReadOnlyList<Guid> AlbaranIds, DateOnly? FechaEmision = null, Guid? FormaPagoId = null, int? DiasVencimiento = null);

/// <summary>
/// Facturación masiva: los albaranes valorados y pendientes hasta una fecha (de un cliente o de todos), una factura por
/// cliente o una por albarán.
/// </summary>
public sealed record FacturacionMasivaAlbaranesComando(DateOnly Hasta, Guid? ClienteId = null, DateOnly? Desde = null, bool UnaFacturaPorAlbaran = false,
    DateOnly? FechaEmision = null);

public sealed record FacturaGeneradaDto(Guid FacturaId, string Numero, Guid ClienteId, string ClienteNombre, int Albaranes, decimal Total);

public sealed record ErrorFacturacionDto(Guid ClienteId, string ClienteNombre, string Codigo, string Mensaje);

public sealed record ResultadoFacturacionMasivaDto(IReadOnlyList<FacturaGeneradaDto> Facturas, IReadOnlyList<ErrorFacturacionDto> Errores,
    IReadOnlyList<AlbaranVentaDto> SinValorar);

// ----------------------------------------------------------------------------- Apoyo
/// <summary>Salida y devolución de existencias de un albarán, y las líneas de factura que lo recogen.</summary>
internal static class AlbaranesVentaStock
{
    private static List<LineaVenta> Lineas(AlbaranVenta a) =>
        a.Lineas.Where(l => l.ProductoId is not null).Select(l => new LineaVenta(l.ProductoId!.Value, l.Cantidad)).ToList();

    /// <summary>Saca la mercancía del almacén (mejor esfuerzo: el albarán ya está guardado).</summary>
    public static async Task SacarAsync(IStockVentas? stock, Guid empresaId, AlbaranVenta albaran, CancellationToken ct)
    {
        var lineas = Lineas(albaran);
        if (stock is not null && lineas.Count > 0)
        {
            await stock.DescontarVentaAsync(empresaId, lineas, albaran.CentroId, ct).ConfigureAwait(false);
        }
    }

    /// <summary>Devuelve al almacén la mercancía de un albarán anulado, si la había sacado.</summary>
    public static async Task DevolverAsync(IStockVentas? stock, AlbaranVenta albaran, CancellationToken ct)
    {
        var lineas = Lineas(albaran);
        if (stock is not null && albaran.StockDescontado && lineas.Count > 0)
        {
            await stock.DevolverVentaAsync(albaran.EmpresaId, lineas, $"Anulación del albarán {albaran.NumeroCompleto}", albaran.CentroId, ct).ConfigureAwait(false);
        }
    }

    /// <summary>Líneas de la factura que recoge el albarán: su precio, sus conceptos y la referencia al albarán en la descripción.</summary>
    public static IEnumerable<LineaComando> LineasFactura(AlbaranVenta a) => LineasFactura(a, new Dictionary<int, decimal>());

    /// <summary>
    /// Líneas de la factura con lo devuelto (devoluciones pendientes de abono) ya descontado; una línea devuelta entera no
    /// sale. Los conceptos de la línea (cargos y abonos) se copian tal como se aplicaron en el albarán.
    /// </summary>
    public static IEnumerable<LineaComando> LineasFactura(AlbaranVenta a, IReadOnlyDictionary<int, decimal> devuelto) => a.Lineas
        .Select(l => (Linea: l, Cantidad: l.Cantidad - (devuelto.TryGetValue(l.Orden, out var d) ? d : 0m)))
        .Where(x => x.Cantidad > 0m)
        .Select(x => new LineaComando(
            x.Cantidad, $"Alb. {a.NumeroCompleto} · {x.Linea.Descripcion}", x.Linea.PrecioUnitario, x.Linea.CodigoIva, x.Linea.PorcentajeDescuento, x.Linea.ProductoId,
            ConceptosCopiados: x.Linea.Conceptos, AlbaranVentaId: a.Id, SinSalidaStock: a.StockDescontado, CosteUnitario: x.Linea.CosteUnitario));

    /// <summary>
    /// Conceptos de una línea de pedido para la parte entregada: los porcentajes sobre la nueva base (en su orden, con la
    /// cascada), los de bulto o palé por los bultos y palés entregados (si se indican; si no, en proporción) y el resto en
    /// proporción a la cantidad entregada.
    /// </summary>
    public static List<ConceptoAplicado> ConceptosParciales(IReadOnlyList<ConceptoAplicado> conceptos, decimal cantidadPedida, decimal cantidadEntregada, decimal baseBruta,
        decimal? bultos = null, decimal? pales = null)
    {
        var resultado = new List<ConceptoAplicado>();
        var proporcion = cantidadPedida == 0m ? 1m : cantidadEntregada / cantidadPedida;
        foreach (var c in conceptos)
        {
            var baseConcepto = c.Cascada ? baseBruta + ConceptosLinea.SumaPrecio(resultado) : baseBruta;
            if (ConceptosLinea.PorUnidadesLogisticas(c.Calculo))
            {
                var unidades = (c.Calculo == CalculoConcepto.PorBulto ? bultos : pales) ?? decimal.Round((c.Unidades ?? 0m) * proporcion, 3);
                resultado.Add(c with { Unidades = unidades, Importe = ConceptosLinea.Calcular(c.Calculo, c.Sentido, c.Valor, 0m, 0m, null, unidades) });
                continue;
            }

            resultado.Add(c with
            {
                Importe = c.Calculo switch
                {
                    CalculoConcepto.Porcentaje => ConceptosLinea.Calcular(c.Calculo, c.Sentido, c.Valor, baseConcepto, 0m, null),
                    CalculoConcepto.PorUnidad => ConceptosLinea.Calcular(c.Calculo, c.Sentido, c.Valor, 0m, cantidadEntregada, null),
                    _ => AlxorCore.Nucleo.Comun.Redondeo.Dos(c.Importe * proporcion),
                },
            });
        }

        return resultado;
    }
}

// ----------------------------------------------------------------------------- Albarán directo
/// <summary>Caso de uso: albarán de venta directo, sin pedido. Saca la mercancía del almacén.</summary>
public sealed class CrearAlbaranVenta
{
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProductos _productos;
    private readonly IResolverPrecioVenta _precios;
    private readonly IResolverSerie _resolverSerie;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IStockVentas _stock;
    private readonly IReloj _reloj;
    private readonly IResolverConceptos? _conceptos;
    private readonly AlxorCore.Organizacion.Aplicacion.Puertos.IConsultaEmpresas? _empresas;

    public CrearAlbaranVenta(IRepositorioAlbaranesVenta albaranes, IConsultaClientes clientes, IConsultaProductos productos, IResolverPrecioVenta precios,
        IResolverSerie resolverSerie, IUnidadDeTrabajoFacturacion unidad, IStockVentas stock, IReloj reloj, IResolverConceptos? conceptos = null,
        AlxorCore.Organizacion.Aplicacion.Puertos.IConsultaEmpresas? empresas = null)
    {
        _conceptos = conceptos;
        _empresas = empresas;
        _albaranes = albaranes;
        _clientes = clientes;
        _productos = productos;
        _precios = precios;
        _resolverSerie = resolverSerie;
        _unidad = unidad;
        _stock = stock;
        _reloj = reloj;
    }

    public async Task<Resultado<AlbaranVentaDto>> EjecutarAsync(Guid empresaId, CrearAlbaranVentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var cliente = await _clientes.ObtenerAsync(comando.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var empresa = _empresas is null ? null : await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var territorio = TerritorioOperacion.Resolver(empresa, comando.Impuesto, (comando.Lineas ?? []).Select(l => l.CodigoIva));
        if (territorio.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(territorio.Error);
        }

        var lineas = new List<NuevaLineaAlbaran>();
        foreach (var l in comando.Lineas ?? [])
        {
            ProductoDto? productoLinea = null;
            var descripcion = l.Descripcion;
            var precio = l.PrecioUnitario;
            var descuento = l.PorcentajeDescuento;
            var codigoIva = l.CodigoIva;
            if (l.ProductoId is { } productoId)
            {
                var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
                if (producto is null)
                {
                    return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("producto.no_encontrado", "El artículo de una línea no existe."));
                }

                descripcion ??= producto.Nombre;
                productoLinea = producto;
                if (precio is null && await _precios.ResolverAsync(cliente.TarifaId, productoId, l.Cantidad, fecha, ct).ConfigureAwait(false) is { } tarifa)
                {
                    precio = tarifa.PrecioUnitario;
                    descuento = tarifa.PorcentajeDescuento;
                }

                precio ??= producto.PrecioUnitario;
            }

            if (precio is null && !l.PrecioPorFijar)
            {
                return Resultado.Fallo<AlbaranVentaDto>(Error.Validacion("albaranventa.linea_sin_precio",
                    "Indica el precio de cada línea o márcala con precio por fijar."));
            }

            // El tipo de la línea con el impuesto del albarán (el del artículo, en Canarias su IGIC).
            if (codigoIva is not null || productoLinea is not null || empresa is { OperaEnAmbosTerritorios: true })
            {
                var tipo = TerritorioOperacion.CodigoLinea(territorio.Valor, codigoIva, productoLinea);
                if (tipo.EsFallo)
                {
                    return Resultado.Fallo<AlbaranVentaDto>(tipo.Error);
                }

                codigoIva = tipo.Valor;
            }

            lineas.Add(new NuevaLineaAlbaran(null, l.ProductoId, descripcion ?? string.Empty, l.Cantidad, precio ?? 0m, descuento, codigoIva, l.PrecioPorFijar));
        }

        // Cargos y abonos: los pedidos en cada línea o los automáticos del cliente (reglas por cliente, tipo y artículo), y los del documento.
        if (_conceptos is not null && lineas.Count > 0)
        {
            var entrada = lineas.Select((l, i) => new LineaConceptos(l.ProductoId, l.Cantidad,
                AlxorCore.Nucleo.Comun.Redondeo.Dos(l.Cantidad * (l.PrecioUnitario ?? 0m) * (1m - l.PorcentajeDescuento / 100m)), comando.Lineas![i].Conceptos,
                Bultos: comando.Lineas[i].Bultos, Pales: comando.Lineas[i].Pales)).ToList();
            var r = await _conceptos.ResolverAsync(AmbitoConcepto.Ventas, cliente.Id, entrada, comando.ConceptosDocumento, true,
                new ContextoConceptos(cliente.Tipo, fecha), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<AlbaranVentaDto>(r.Error);
            }

            lineas = lineas.Select((l, i) => l with { Conceptos = r.Valor[i] }).ToList();
        }

        var numero = await _albaranes.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.AlbaranVenta, cliente.Id, comando.CentroId, null, ct).ConfigureAwait(false);
        var albaran = AlbaranVenta.Crear(empresaId, null, cliente.Id, cliente.Nombre, numero, fecha, comando.Referencia, lineas, _reloj, serie, comando.Observaciones);
        if (albaran.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(albaran.Error);
        }

        albaran.Valor.AsignarCentro(comando.CentroId);

        _albaranes.Agregar(albaran.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await AlbaranesVentaStock.SacarAsync(_stock, empresaId, albaran.Valor, ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranVentaDto.Desde(albaran.Valor));
    }
}

// ----------------------------------------------------------------------------- Valoración
/// <summary>Caso de uso: fijar los precios de un albarán entregado a precio por fijar (valoración a posteriori).</summary>
public sealed class ValorarAlbaranVenta
{
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IRepositorioLiquidacionesComision? _comision;

    public ValorarAlbaranVenta(IRepositorioAlbaranesVenta albaranes, IUnidadDeTrabajoFacturacion unidad, IRepositorioLiquidacionesComision? comision = null)
    {
        _albaranes = albaranes;
        _unidad = unidad;
        _comision = comision;
    }

    public async Task<Resultado<AlbaranVentaDto>> EjecutarAsync(Guid albaranId, ValorarAlbaranVentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var albaran = await _albaranes.ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
        if (albaran is null)
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe."));
        }

        // Lo que valora una liquidación de venta en comisión no se valora a mano: se cambia la liquidación.
        if (_comision is not null && comando.Lineas is { Count: > 0 } pedidas)
        {
            var enComision = (await _comision.VivasDeAlbaranesAsync([albaranId], ct).ConfigureAwait(false))
                .SelectMany(l => l.Lineas.Where(x => x.AlbaranVentaId == albaranId).Select(x => (x.OrdenAlbaran, Liquidacion: l.NumeroCompleto ?? "en borrador"))).ToList();
            if (enComision.FirstOrDefault(x => pedidas.Any(p => p.Orden == x.OrdenAlbaran)) is { Liquidacion: not null } ocupada)
            {
                return Resultado.Fallo<AlbaranVentaDto>(Error.Conflicto("albaranventa.en_liquidacion_comision",
                    $"La línea {ocupada.OrdenAlbaran} está en la liquidación de venta en comisión {ocupada.Liquidacion}: se valora desde ella."));
            }
        }

        var r = albaran.Valorar((comando.Lineas ?? []).Select(l => (l.Orden, l.PrecioUnitario, l.PorcentajeDescuento)).ToList());
        if (r.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranVentaDto.Desde(albaran));
    }
}

// ----------------------------------------------------------------------------- Facturación
/// <summary>
/// Caso de uso: facturar albaranes de venta. Varios albaranes del mismo cliente van en una sola factura (una línea por
/// cada línea de albarán, con su referencia); la mercancía ya salió con el albarán, así que la factura no la mueve. Si
/// el albarán viene de un pedido, se anota lo facturado en el pedido.
/// </summary>
public sealed class FacturarAlbaranesVenta
{
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly EmitirFactura _emitir;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IRepositorioDevolucionesVenta? _devoluciones;
    private readonly IReloj? _reloj;

    public FacturarAlbaranesVenta(IRepositorioAlbaranesVenta albaranes, IRepositorioPedidosVenta pedidos, EmitirFactura emitir, IUnidadDeTrabajoFacturacion unidad,
        IRepositorioDevolucionesVenta? devoluciones = null, IReloj? reloj = null)
    {
        _devoluciones = devoluciones;
        _reloj = reloj;
        _albaranes = albaranes;
        _pedidos = pedidos;
        _emitir = emitir;
        _unidad = unidad;
    }

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, FacturarAlbaranesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var ids = (comando.AlbaranIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("albaranventa.ninguno", "Elige al menos un albarán para facturar."));
        }

        var albaranes = await _albaranes.ObtenerVariosAsync(ids, ct).ConfigureAwait(false);
        if (albaranes.Count != ids.Count)
        {
            return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("albaranventa.no_encontrado", "Algún albarán no existe."));
        }

        return await FacturarAsync(empresaId, albaranes, comando.FechaEmision, comando.FormaPagoId, comando.DiasVencimiento, ct).ConfigureAwait(false);
    }

    /// <summary>Factura masiva de los albaranes pendientes y valorados hasta una fecha.</summary>
    public async Task<ResultadoFacturacionMasivaDto> MasivaAsync(Guid empresaId, FacturacionMasivaAlbaranesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var filtro = new FiltroAlbaranesVenta(comando.ClienteId, null, comando.Desde, comando.Hasta);
        var candidatos = (await _albaranes.ListarAsync(empresaId, filtro, ct).ConfigureAwait(false))
            .Where(a => a.Estado is nameof(EstadoAlbaranVenta.PendienteFacturar) or nameof(EstadoAlbaranVenta.PendienteValorar)).ToList();
        var sinValorar = candidatos.Where(a => a.Estado == nameof(EstadoAlbaranVenta.PendienteValorar)).ToList();
        var listos = candidatos.Where(a => a.Estado == nameof(EstadoAlbaranVenta.PendienteFacturar)).ToList();

        var grupos = comando.UnaFacturaPorAlbaran
            ? listos.OrderBy(a => a.ClienteNombre, StringComparer.CurrentCulture).ThenBy(a => a.Fecha).ThenBy(a => a.Numero).Select(a => (IReadOnlyList<AlbaranVentaDto>)[a]).ToList()
            : listos.GroupBy(a => (a.ClienteId, a.CentroId)).OrderBy(g => g.First().ClienteNombre, StringComparer.CurrentCulture).Select(g => (IReadOnlyList<AlbaranVentaDto>)g.ToList()).ToList();

        var facturas = new List<FacturaGeneradaDto>();
        var errores = new List<ErrorFacturacionDto>();
        foreach (var grupo in grupos)
        {
            var albaranes = await _albaranes.ObtenerVariosAsync(grupo.Select(a => a.Id).ToList(), ct).ConfigureAwait(false);
            var r = await FacturarAsync(empresaId, albaranes, comando.FechaEmision, null, null, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add(new ErrorFacturacionDto(grupo[0].ClienteId, grupo[0].ClienteNombre, r.Error.Codigo, r.Error.Mensaje));
            }
            else
            {
                facturas.Add(new FacturaGeneradaDto(r.Valor.Id, r.Valor.NumeroCompleto, grupo[0].ClienteId, r.Valor.ClienteNombre, grupo.Count, r.Valor.Total));
            }
        }

        return new ResultadoFacturacionMasivaDto(facturas, errores, sinValorar);
    }

    private async Task<Resultado<FacturaDto>> FacturarAsync(Guid empresaId, IReadOnlyList<AlbaranVenta> albaranes, DateOnly? fechaEmision, Guid? formaPagoId,
        int? diasVencimiento, CancellationToken ct)
    {
        if (albaranes.Select(a => a.ClienteId).Distinct().Count() > 1)
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("albaranventa.varios_clientes", "Solo se facturan juntos albaranes del mismo cliente."));
        }

        foreach (var a in albaranes)
        {
            if (a.PuedeFacturarse() is { EsFallo: true } no)
            {
                return Resultado.Fallo<FacturaDto>(no.Error);
            }
        }

        var ordenados = albaranes.OrderBy(a => a.Fecha).ThenBy(a => a.Numero).ToList();
        var centros = ordenados.Select(a => a.CentroId).Distinct().ToList();
        if (centros.Count > 1)
        {
            return Resultado.Fallo<FacturaDto>(Error.Conflicto("albaranventa.centros_distintos", "Los albaranes son de centros distintos: factúralos por separado."));
        }

        // Lo devuelto y aún sin abonar se descuenta en la propia factura del albarán.
        var devoluciones = _devoluciones is null ? []
            : (await _devoluciones.DeAlbaranesAsync(ordenados.Select(a => a.Id).ToList(), ct).ConfigureAwait(false))
                .Where(d => d.Estado == EstadoDevolucionVenta.Registrada).ToList();
        var lineas = ordenados.SelectMany(a => AlbaranesVentaStock.LineasFactura(a, DevolucionesAlbaran.Devuelto(devoluciones.Where(d => d.AlbaranId == a.Id)))).ToList();
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<FacturaDto>(Error.Conflicto("albaranventa.devuelto_entero", "Todo lo entregado se ha devuelto: no hay nada que facturar."));
        }

        // La fecha de operación es la de la última entrega (art. 75 LIVA: el devengo es la puesta a disposición).
        var comando = new EmitirFacturaComando(ordenados[0].ClienteId, lineas, fechaEmision, ordenados[^1].Fecha, DiasVencimiento: diasVencimiento, FormaPagoId: formaPagoId, CentroId: centros[0]);
        var factura = await _emitir.EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
        if (factura.EsFallo)
        {
            return factura;
        }

        foreach (var d in devoluciones)
        {
            d.Abonar(FormaAbonoDevolucion.EnFacturaDelAlbaran, factura.Valor.Id, _reloj ?? RelojSistemaDevoluciones.Instancia);
        }

        foreach (var a in ordenados)
        {
            a.Facturar(factura.Valor.Id);
            if (a.PedidoId is { } pedidoId && await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false) is { } pedido)
            {
                pedido.FacturarEntrega(factura.Valor.Id, a.Lineas.Where(l => l.LineaPedidoId is not null).Select(l => (l.LineaPedidoId!.Value, l.Cantidad)).ToList());
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return factura;
    }
}

/// <summary>Listado de albaranes de venta con filtro (cliente, estado y fechas).</summary>
public sealed class ConsultarAlbaranesVenta
{
    private readonly IRepositorioAlbaranesVenta _albaranes;

    public ConsultarAlbaranesVenta(IRepositorioAlbaranesVenta albaranes) => _albaranes = albaranes;

    public Task<IReadOnlyList<AlbaranVentaDto>> ListarAsync(Guid empresaId, FiltroAlbaranesVenta filtro, CancellationToken ct = default) =>
        _albaranes.ListarAsync(empresaId, filtro, ct);

    public async Task<AlbaranVentaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _albaranes.ObtenerPorIdAsync(id, ct).ConfigureAwait(false) is { } a ? AlbaranVentaDto.Desde(a) : null;
}

/// <summary>
/// Al anular una factura, los albaranes que recogía vuelven a estar pendientes de facturar (y su pedido, a servido). Lo
/// usa <see cref="AnularFactura"/> antes de guardar.
/// </summary>
public sealed class LiberarAlbaranesFactura
{
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly IRepositorioDevolucionesVenta? _devoluciones;

    public LiberarAlbaranesFactura(IRepositorioAlbaranesVenta albaranes, IRepositorioPedidosVenta pedidos, IRepositorioDevolucionesVenta? devoluciones = null)
    {
        _albaranes = albaranes;
        _pedidos = pedidos;
        _devoluciones = devoluciones;
    }

    public async Task EjecutarAsync(Guid facturaId, CancellationToken ct = default)
    {
        if (_devoluciones is not null)
        {
            foreach (var d in await _devoluciones.DeFacturaAsync(facturaId, ct).ConfigureAwait(false))
            {
                d.LiberarFactura(facturaId);
            }
        }

        foreach (var a in await _albaranes.DeFacturaAsync(facturaId, ct).ConfigureAwait(false))
        {
            a.LiberarFactura(facturaId);
            if (a.PedidoId is { } pedidoId && await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false) is { } pedido)
            {
                pedido.DeshacerFacturacion(facturaId, a.Lineas.Where(l => l.LineaPedidoId is not null).Select(l => (l.LineaPedidoId!.Value, l.Cantidad)).ToList());
            }
        }
    }
}

/// <summary>Reloj de respaldo cuando el caso de uso se construye sin reloj (pruebas unitarias antiguas).</summary>
internal sealed class RelojSistemaDevoluciones : IReloj
{
    public static readonly RelojSistemaDevoluciones Instancia = new();

    public DateTimeOffset AhoraUtc => DateTimeOffset.UtcNow;
}
