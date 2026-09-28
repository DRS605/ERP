using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Línea de la factura a emitir. Si se indica <see cref="ProductoId"/>, se toman sus datos por defecto.</summary>
public sealed record LineaComando(
    decimal Cantidad,
    string? Descripcion = null,
    decimal? PrecioUnitario = null,
    string? CodigoIva = null,
    decimal PorcentajeDescuento = 0m,
    Guid? ProductoId = null,
    decimal? CosteUnitario = null,
    IReadOnlyList<ConceptoSolicitado>? Conceptos = null,
    IReadOnlyList<ConceptoAplicado>? ConceptosCopiados = null,
    string? CuentaContable = null,
    Guid? AnticipoId = null,
    Guid? AlbaranVentaId = null,
    bool SinSalidaStock = false);

/// <summary>Datos para emitir una factura. <c>DiasVencimiento</c> es el plazo de pago (0 = contado).</summary>
public sealed record EmitirFacturaComando(
    Guid ClienteId,
    IReadOnlyList<LineaComando> Lineas,
    DateOnly? FechaEmision = null,
    DateOnly? FechaOperacion = null,
    decimal? PorcentajeIrpf = null,
    string? Serie = null,
    int? DiasVencimiento = null,
    bool RecargoEquivalencia = false,
    Guid? FormaPagoId = null,
    Guid? ActividadNegocioId = null,
    IReadOnlyList<ConceptoSolicitado>? ConceptosDocumento = null,
    IReadOnlyList<DescuentoAnticipoSolicitado>? DescontarAnticipos = null);

/// <summary>
/// Caso de uso estrella: emitir una factura. Compone cliente (Terceros), productos/impuestos
/// (Catálogo) y numeración correlativa (Organización). El número se asigna de forma atómica
/// <b>después</b> de validar todo, para minimizar el riesgo de huecos (invariante F1).
/// </summary>
public sealed class EmitirFactura
{
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProductos _productos;
    private readonly IResolverSerie _resolverSerie;
    private readonly IRepositorioFacturas _facturas;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoFacturacion _unidadDeTrabajo;
    private readonly IStockVentas _stock;
    private readonly EncolarSalida _encolarSalida;
    private readonly DespacharSalida _despacharSalida;
    private readonly IConsultaFormasPago _formasPago;
    private readonly IPagosAutomaticos _pagos;
    private readonly IConsultaRiesgo _riesgo;
    private readonly IResolverIvaEmpresa _resolverIva;
    private readonly IResolverPrecioVenta _precios;
    private readonly IReloj _reloj;
    private readonly IResolverConceptos? _conceptos;
    private readonly IAnticiposFactura? _anticipos;

    public EmitirFactura(
        IConsultaClientes clientes,
        IConsultaProductos productos,
        IResolverSerie resolverSerie,
        IRepositorioFacturas facturas,
        IConsultaEmpresas empresas,
        IUnidadDeTrabajoFacturacion unidadDeTrabajo,
        IStockVentas stock,
        EncolarSalida encolarSalida,
        DespacharSalida despacharSalida,
        IConsultaFormasPago formasPago,
        IPagosAutomaticos pagos,
        IConsultaRiesgo riesgo,
        IResolverIvaEmpresa resolverIva,
        IResolverPrecioVenta precios,
        IReloj reloj,
        IResolverConceptos? conceptos = null,
        IAnticiposFactura? anticipos = null)
    {
        _conceptos = conceptos;
        _anticipos = anticipos;
        _resolverIva = resolverIva;
        _precios = precios;
        _clientes = clientes;
        _productos = productos;
        _resolverSerie = resolverSerie;
        _facturas = facturas;
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _stock = stock;
        _encolarSalida = encolarSalida;
        _despacharSalida = despacharSalida;
        _formasPago = formasPago;
        _pagos = pagos;
        _riesgo = riesgo;
        _reloj = reloj;
    }

    /// <summary>
    /// Líneas para la contabilización cuando alguna lleva su propia cuenta (anticipos: 438) o algún concepto de importe la
    /// tiene (portes cobrados a la 759, envases…): la parte del concepto va a su cuenta y el resto de la línea a la suya o
    /// a la de ventas. Si nada lleva cuenta, null y el asiento va entero a la cuenta de ventas de la regla.
    /// </summary>
    internal static IReadOnlyList<LineaContable>? LineasConCuenta(Factura f)
    {
        static IEnumerable<ConceptoAplicado> Propios(LineaFactura l) =>
            l.Conceptos.Where(c => c.Efecto == EfectoConcepto.Precio && !string.IsNullOrWhiteSpace(c.CuentaContable));
        if (!f.Lineas.Any(l => l.CuentaContable is not null || Propios(l).Any()))
        {
            return null;
        }

        var lineas = new List<LineaContable>();
        foreach (var l in f.Lineas)
        {
            var propios = Propios(l).ToList();
            lineas.Add(new LineaContable(l.Base - propios.Sum(c => c.Importe), l.CodigoIva, l.CuotaIva, l.CuotaIva, l.CuotaRecargo, CuentaGasto: l.CuentaContable));
            lineas.AddRange(propios.Select(c => new LineaContable(c.Importe, l.CodigoIva, 0m, 0m, 0m, CuentaGasto: c.CuentaContable)));
        }

        return lineas;
    }

    public Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, EmitirFacturaComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, false, ct);

    /// <summary>
    /// Calcula la factura tal como se emitiría (precios de tarifa, conceptos de línea, impuestos, recargo, IRPF y aviso
    /// de riesgo) sin numerarla ni guardarla: lo usa la pantalla para enseñar los importes exactos mientras se edita.
    /// </summary>
    public Task<Resultado<FacturaDto>> SimularAsync(Guid empresaId, EmitirFacturaComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, true, ct);

    private async Task<Resultado<FacturaDto>> EjecutarInternoAsync(Guid empresaId, EmitirFacturaComando comando, bool simular, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);

        if (comando.Lineas is null || comando.Lineas.Count == 0)
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("factura.sin_lineas", "La factura debe tener al menos una línea."));
        }

        var cliente = await _clientes.ObtenerAsync(comando.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var fechaPrecio = comando.FechaOperacion ?? comando.FechaEmision ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var impuesto = (await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false))?.ImpuestoIndirecto ?? TipoImpuesto.Iva;
        var resolucion = await ResolucionLineasFactura.ResolverAsync(comando.Lineas, _productos, ct, comando.RecargoEquivalencia, empresaId, _resolverIva,
            (producto, cantidad, c) => _precios.ResolverAsync(cliente.TarifaId, producto, cantidad, fechaPrecio, c), impuesto).ConfigureAwait(false);
        if (resolucion.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(resolucion.Error);
        }

        var conConceptos = await ResolucionLineasFactura.AplicarConceptosAsync(_conceptos, cliente.Id, comando.Lineas, resolucion.Valor, comando.ConceptosDocumento, true,
            new ContextoConceptos(cliente.Tipo, fechaPrecio), ct)
            .ConfigureAwait(false);
        if (conConceptos.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(conConceptos.Error);
        }

        var lineas = conConceptos.Valor;

        // Anticipos facturados que se descuentan: líneas negativas con su base e impuesto (cuenta 438), como mucho la base de la factura.
        if (comando.DescontarAnticipos is { Count: > 0 } descontar)
        {
            if (_anticipos is null)
            {
                return Resultado.Fallo<FacturaDto>(Error.Validacion("factura.anticipos", "No se pueden descontar anticipos en esta instalación."));
            }

            var baseFactura = lineas.Sum(l => LineaFactura.CalcularBaseBruta(l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento) + ConceptosLinea.SumaPrecio(l.Conceptos));
            var descuentos = await _anticipos.LineasDescuentoAsync(cliente.Id, descontar, baseFactura, ct).ConfigureAwait(false);
            if (descuentos.EsFallo)
            {
                return Resultado.Fallo<FacturaDto>(descuentos.Error);
            }

            var resueltos = await ResolucionLineasFactura.ResolverAsync(descuentos.Valor, _productos, ct, comando.RecargoEquivalencia, empresaId, _resolverIva,
                (producto, cantidad, c) => _precios.ResolverAsync(cliente.TarifaId, producto, cantidad, fechaPrecio, c), impuesto).ConfigureAwait(false);
            if (resueltos.EsFallo)
            {
                return Resultado.Fallo<FacturaDto>(resueltos.Error);
            }

            lineas = [.. lineas, .. resueltos.Valor];
        }

        var mencionFiscal = await ResolucionLineasFactura.MencionFiscalAsync(empresaId, lineas, _resolverIva, ct).ConfigureAwait(false);
        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fechaEmision = comando.FechaEmision ?? hoy;
        var fechaOperacion = comando.FechaOperacion ?? fechaEmision;

        // Forma de pago: la indicada o la habitual del cliente. Determina el vencimiento y si el cobro
        // se registra en el acto (documento «ya pagado»).
        var formaPagoId = comando.FormaPagoId ?? cliente.FormaPagoDefectoId;
        FormaPagoDto? formaPago = formaPagoId is { } fpid
            ? await _formasPago.ObtenerAsync(fpid, ct).ConfigureAwait(false)
            : null;
        var diasVencimiento = formaPago is not null
            ? (formaPago.GeneraVencimiento ? formaPago.DiasVencimiento : 0)
            : Math.Max(0, comando.DiasVencimiento ?? 0);
        var fechaVencimiento = fechaEmision.AddDays(diasVencimiento);
        var porcentajeIrpf = comando.PorcentajeIrpf ?? cliente.PorcentajeIrpfDefecto;

        // La actividad de negocio se hereda del cliente, salvo que se indique una en el comando
        // (el acceso del usuario a esa actividad lo valida la capa de API antes de llegar aquí).
        var clienteFacturado = new ClienteFacturado(
            cliente.Id, cliente.Nombre, cliente.NifFiscal,
            cliente.Calle, cliente.CodigoPostal, cliente.Poblacion, cliente.Provincia, cliente.Pais,
            comando.ActividadNegocioId ?? cliente.ActividadNegocioId);

        // Serie: si no se indica una explícita, se resuelve la asignada al cliente (o la de la empresa).
        var serie = comando.Serie;
        if (string.IsNullOrWhiteSpace(serie))
        {
            serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.Factura, cliente.Id, ct).ConfigureAwait(false);
        }

        // Control de riesgo del cliente: se comprueba ANTES de numerar (para no consumir número si se
        // bloquea). El total proyectado se calcula con una factura provisional (número 0) que se descarta.
        string? avisoRiesgo = null;
        if (simular)
        {
            var prefijo = string.IsNullOrWhiteSpace(serie) ? "FA" : serie!;
            var borrador = Factura.Emitir(empresaId, new NumeroFactura(prefijo, fechaEmision.Year, 0), fechaEmision, fechaOperacion, clienteFacturado, lineas, porcentajeIrpf, _reloj, fechaVencimiento);
            if (borrador.EsFallo)
            {
                return Resultado.Fallo<FacturaDto>(borrador.Error);
            }

            borrador.Valor.EstablecerMencionFiscal(mencionFiscal);
            borrador.Valor.EstablecerImpuesto(impuesto);
            if (cliente.LimiteRiesgo is { } limite)
            {
                var vivo = await _riesgo.RiesgoVivoClienteAsync(empresaId, cliente.Id, ct).ConfigureAwait(false);
                if (vivo + borrador.Valor.Total > limite)
                {
                    avisoRiesgo = $"El cliente supera su límite de riesgo ({limite:F2} €): riesgo vivo {vivo:F2} € + este documento {borrador.Valor.Total:F2} €.";
                }
            }

            return Resultado.Ok(FacturaDto.Desde(borrador.Valor) with { AvisoRiesgo = avisoRiesgo });
        }

        if (cliente.LimiteRiesgo is { } limiteRiesgo)
        {
            var prefijoProvisional = string.IsNullOrWhiteSpace(serie) ? "FA" : serie!;
            var provisional = Factura.Emitir(empresaId, new NumeroFactura(prefijoProvisional, fechaEmision.Year, 0), fechaEmision, fechaOperacion, clienteFacturado, lineas, porcentajeIrpf, _reloj, fechaVencimiento);
            if (provisional.EsFallo)
            {
                return Resultado.Fallo<FacturaDto>(provisional.Error);
            }

            var totalProyectado = provisional.Valor.Total;
            var riesgoVivo = await _riesgo.RiesgoVivoClienteAsync(empresaId, cliente.Id, ct).ConfigureAwait(false);
            if (riesgoVivo + totalProyectado > limiteRiesgo)
            {
                var emp = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
                if ((emp?.ControlRiesgo ?? ControlRiesgo.Aviso) == ControlRiesgo.Bloqueo)
                {
                    return Resultado.Fallo<FacturaDto>(Error.Conflicto("riesgo.superado",
                        $"El cliente supera su límite de riesgo ({limiteRiesgo:F2} €): riesgo vivo {riesgoVivo:F2} € + esta factura {totalProyectado:F2} €."));
                }

                avisoRiesgo = $"El cliente supera su límite de riesgo ({limiteRiesgo:F2} €). Riesgo tras esta factura: {riesgoVivo + totalProyectado:F2} €.";
            }
        }

        // La numeración es lo último antes de crear y guardar (minimiza huecos).
        // Número correlativo SIN huecos: se calcula (último + 1) dentro de la misma transacción que
        // guarda la factura y bajo un bloqueo por empresa, de modo que si la emisión falla no se pierde
        // ningún número y dos emisiones simultáneas no se pisan (ni el número ni la cadena VeriFactu).
        var numero = await _facturas.ReservarNumeroAsync(empresaId, serie, fechaEmision, ct).ConfigureAwait(false);
        if (numero.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(numero.Error);
        }

        var numeroFactura = numero.Valor;
        var factura = Factura.Emitir(empresaId, numeroFactura, fechaEmision, fechaOperacion, clienteFacturado, lineas, porcentajeIrpf, _reloj, fechaVencimiento);
        if (factura.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(factura.Error);
        }

        factura.Valor.EstablecerMencionFiscal(mencionFiscal);
        factura.Valor.EstablecerImpuesto(impuesto);
        await RegistroVerifactu.AplicarAsync(empresaId, factura.Valor, _empresas, _facturas, _reloj, ct).ConfigureAwait(false);
        _facturas.Agregar(factura.Valor);

        // Bandeja de salida (outbox): el documento a contabilizar se encola en la MISMA transacción que
        // la factura, de modo que "emitir factura" y "encolar su contabilización" son atómicos (ninguna
        // factura queda sin su documento a contabilizar, ni al revés). La familia (del primer artículo)
        // y el tipo de cliente permiten aplicar las reglas de cuenta.
        var f = factura.Valor;
        var codigoIva = f.Lineas.Count > 0 ? f.Lineas[0].CodigoIva : "IVA21";
        var productoId = f.Lineas.FirstOrDefault(l => l.ProductoId is not null)?.ProductoId;
        string? familia = null;
        if (productoId is { } pid)
        {
            var producto = await _productos.ObtenerAsync(pid, ct).ConfigureAwait(false);
            familia = producto?.Familia;
        }

        _encolarSalida.Contabilizacion(empresaId, new DocumentoContabilizable(
            SentidoContable.Venta, "FacturaVenta", f.Id, f.NumeroCompleto, f.ClienteId, f.ClienteNombre,
            f.FechaEmision, f.BaseImponible, codigoIva, f.CuotaIva, f.PorcentajeIrpf, f.RetencionIrpf, f.Total, productoId, familia, cliente.Tipo,
            ActividadNegocioId: f.ActividadNegocioId, Lineas: LineasConCuenta(f)));

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        // Los anticipos descontados quedan anotados (ya no están disponibles para otra factura).
        if (_anticipos is not null && f.Lineas.Any(l => l.AnticipoId is not null))
        {
            await _anticipos.AnotarAsync(f, ct).ConfigureAwait(false);
        }

        // Ya confirmada la factura, despacha la bandeja de salida (encola la contabilización). Si fallara,
        // el mensaje queda pendiente y se reintenta; la factura ya está a salvo.
        await _despacharSalida.EjecutarAsync(ct: ct).ConfigureAwait(false);

        // Descuento de existencias de los artículos con control de stock (mejor esfuerzo; la factura ya
        // está emitida y es la verdad fiscal). Las líneas de albaranes que ya sacaron la mercancía no la mueven otra vez.
        var yaSalieron = comando.Lineas.Where(l => l.SinSalidaStock && l.AlbaranVentaId is not null).Select(l => l.AlbaranVentaId!.Value).ToHashSet();
        var lineasVenta = f.Lineas
            .Where(l => l.ProductoId is not null && !(l.AlbaranVentaId is { } alb && yaSalieron.Contains(alb)))
            .Select(l => new LineaVenta(l.ProductoId!.Value, l.Cantidad))
            .ToList();
        if (lineasVenta.Count > 0)
        {
            await _stock.DescontarVentaAsync(empresaId, lineasVenta, ct).ConfigureAwait(false);
        }

        // Si la forma de pago registra el pago automáticamente, se cobra el total en el acto (queda
        // saldada, fuera de la cartera). Se omite si el pago se registra aparte.
        if (formaPago?.RegistrarPagoAutomatico == true)
        {
            await _pagos.RegistrarCobroTotalAsync(empresaId, f.Id, f.Total, f.FechaEmision, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(FacturaDto.Desde(f) with { AvisoRiesgo = avisoRiesgo });
    }
}

/// <summary>
/// Genera el registro VeriFactu de una factura recién emitida: obtiene la huella del registro
/// anterior de la empresa (encadenamiento) y el NIF del emisor, y calcula la huella. Lo comparten la
/// emisión de facturas y de tickets.
/// </summary>
/// <remarks>
/// La huella anterior se lee justo antes de calcular; se asume emisión secuencial por empresa (misma
/// premisa que la numeración). Endurecer la cadena con un bloqueo por empresa ante emisión concurrente
/// es una mejora futura documentada.
/// </remarks>
internal static class RegistroVerifactu
{
    public static async Task AplicarAsync(
        Guid empresaId, Factura factura, IConsultaEmpresas empresas, IRepositorioFacturas facturas, IReloj reloj, CancellationToken ct)
    {
        var emisor = await empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var huellaAnterior = await facturas.UltimaHuellaAsync(empresaId, ct).ConfigureAwait(false);
        factura.RegistrarVerifactu(emisor?.Nif ?? string.Empty, huellaAnterior, reloj.AhoraUtc);
    }
}

/// <summary>
/// Resuelve las líneas de comando a líneas de dominio (<see cref="NuevaLinea"/>), tomando los datos
/// por defecto del producto cuando se indica <c>ProductoId</c>. Lo comparten la emisión de facturas
/// y de tickets.
/// </summary>
internal static class ResolucionLineasFactura
{
    public static async Task<Resultado<List<NuevaLinea>>> ResolverAsync(
        IReadOnlyList<LineaComando> lineas, IConsultaProductos productos, CancellationToken ct, bool recargoEquivalencia = false,
        Guid? empresaId = null, IResolverIvaEmpresa? resolverIva = null,
        Func<Guid, decimal, CancellationToken, Task<PrecioVentaDto?>>? precioTarifa = null,
        TipoImpuesto? impuestoEmpresa = null)
    {
        var resueltas = new List<NuevaLinea>(lineas.Count);
        foreach (var linea in lineas)
        {
            string? descripcion = linea.Descripcion;
            decimal? precio = linea.PrecioUnitario;
            string? codigoIva = linea.CodigoIva;
            decimal? coste = linea.CosteUnitario;
            var descuento = linea.PorcentajeDescuento;

            if (linea.ProductoId is not null)
            {
                var producto = await productos.ObtenerAsync(linea.ProductoId.Value, ct).ConfigureAwait(false);
                if (producto is null)
                {
                    return Resultado.Fallo<List<NuevaLinea>>(Error.NoEncontrado("producto.no_encontrado", "El producto de una línea no existe."));
                }

                descripcion ??= producto.Nombre;

                // Sin precio en la línea: manda la tarifa del cliente (precio y descuento). Con precio, se respeta.
                if (precio is null && precioTarifa is not null
                    && await precioTarifa(producto.Id, linea.Cantidad, ct).ConfigureAwait(false) is { } tarifa)
                {
                    precio = tarifa.PrecioUnitario;
                    descuento = tarifa.PorcentajeDescuento;
                }

                precio ??= producto.PrecioUnitario;
                codigoIva ??= producto.CodigoIva;
                coste ??= producto.PrecioCompra;
            }

            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return Resultado.Fallo<List<NuevaLinea>>(Error.Validacion("factura.linea_sin_descripcion", "Cada línea necesita una descripción."));
            }

            if (precio is null)
            {
                return Resultado.Fallo<List<NuevaLinea>>(Error.Validacion("factura.linea_sin_precio", "Cada línea necesita un precio."));
            }

            // Preferimos el catálogo de IVA de la empresa (con todas las casuísticas: exención, ISP, no
            // sujeto, importación, intracomunitario). Si no está configurado, se usa el catálogo estatal.
            // Sin código, el tipo general del impuesto de la empresa (IVA 21 %, o IGIC 7 % en Canarias).
            var codigo = codigoIva ?? (impuestoEmpresa == TipoImpuesto.Igic ? Impuesto.IgicGeneral.Codigo : Impuesto.IvaGeneral.Codigo);
            string codigoResuelto;
            decimal porcentaje;
            decimal porcentajeRecargo;
            TipoImpuesto impuestoLinea;
            if (empresaId is { } emp && resolverIva is not null &&
                await resolverIva.ResolverAsync(emp, codigo, ct).ConfigureAwait(false) is { } iva)
            {
                codigoResuelto = iva.Codigo;
                porcentaje = iva.PorcentajeRepercutido; // 0 en clases sin repercusión (exento, ISP, no sujeto…)
                porcentajeRecargo = recargoEquivalencia ? iva.RecargoEquivalencia : 0m;
                impuestoLinea = iva.Impuesto;
            }
            else
            {
                var impuesto = Impuesto.PorCodigoImpuesto(codigo);
                if (impuesto.EsFallo)
                {
                    return Resultado.Fallo<List<NuevaLinea>>(impuesto.Error);
                }

                codigoResuelto = impuesto.Valor.Codigo;
                porcentaje = impuesto.Valor.Porcentaje;
                porcentajeRecargo = recargoEquivalencia ? Impuesto.RecargoEquivalencia(impuesto.Valor.Porcentaje) : 0m;
                impuestoLinea = impuesto.Valor.Tipo;
            }

            // Una factura lleva el impuesto del territorio de la empresa: IVA o IGIC, nunca mezclados.
            if (impuestoEmpresa is { } esperado && impuestoLinea != esperado)
            {
                return Resultado.Fallo<List<NuevaLinea>>(Error.Validacion("factura.impuesto_territorio",
                    $"El tipo {codigoResuelto} es de {impuestoLinea.Siglas()}, pero la empresa tributa por {esperado.Siglas()}" +
                    (esperado == TipoImpuesto.Igic ? " (Canarias)." : ".")));
            }

            if (impuestoLinea == TipoImpuesto.Igic)
            {
                porcentajeRecargo = 0m; // el IGIC no tiene recargo de equivalencia
            }

            resueltas.Add(new NuevaLinea(
                descripcion, linea.Cantidad, precio.Value, codigoResuelto, porcentaje, descuento, linea.ProductoId, coste ?? 0m, porcentajeRecargo,
                CuentaContable: linea.CuentaContable, AnticipoId: linea.AnticipoId, AlbaranVentaId: linea.AlbaranVentaId));
        }

        return Resultado.Ok(resueltas);
    }

    /// <summary>
    /// Pone los conceptos de línea (los pedidos, los copiados del documento de origen o, si <paramref name="automaticos"/>,
    /// los que se ponen solos al cliente y al artículo) y los repartidos del documento. Sin resolutor, no cambia nada.
    /// </summary>
    public static async Task<Resultado<List<NuevaLinea>>> AplicarConceptosAsync(
        IResolverConceptos? conceptos, Guid? clienteId, IReadOnlyList<LineaComando> comandos, List<NuevaLinea> lineas,
        IReadOnlyList<ConceptoSolicitado>? documento, bool automaticos, ContextoConceptos? contexto, CancellationToken ct)
    {
        if (conceptos is null)
        {
            return Resultado.Ok(lineas);
        }

        var entrada = lineas.Select((l, i) => new LineaConceptos(l.ProductoId, l.Cantidad,
            LineaFactura.CalcularBaseBruta(l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento), comandos[i].Conceptos, comandos[i].ConceptosCopiados)).ToList();
        var r = await conceptos.ResolverAsync(AmbitoConcepto.Ventas, clienteId, entrada, documento, automaticos, contexto, ct).ConfigureAwait(false);
        return r.EsFallo
            ? Resultado.Fallo<List<NuevaLinea>>(r.Error)
            : Resultado.Ok(lineas.Select((l, i) => r.Valor[i].Count == 0 ? l : l with { Conceptos = r.Valor[i] }).ToList());
    }

    /// <summary>
    /// Reúne las menciones legales de los tipos de IVA usados en las líneas (exención, ISP, no sujeto,
    /// intracomunitario…) para estamparlas en la factura. Devuelve null si ninguna línea las requiere.
    /// </summary>
    public static async Task<string?> MencionFiscalAsync(
        Guid empresaId, IEnumerable<NuevaLinea> lineas, IResolverIvaEmpresa resolverIva, CancellationToken ct)
    {
        var menciones = new List<string>();
        foreach (var codigo in lineas.Select(l => l.CodigoIva).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var iva = await resolverIva.ResolverAsync(empresaId, codigo, ct).ConfigureAwait(false);
            if (iva?.MencionFactura is { Length: > 0 } m && !menciones.Contains(m, StringComparer.Ordinal))
            {
                menciones.Add(m);
            }
        }

        return menciones.Count == 0 ? null : string.Join(" · ", menciones);
    }
}

/// <summary>Caso de uso: listar las facturas de la empresa activa.</summary>
public sealed class ListarFacturas
{
    private readonly IConsultaFacturas _consulta;

    public ListarFacturas(IConsultaFacturas consulta) => _consulta = consulta;

    public Task<IReadOnlyList<FacturaResumen>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, ct);
}

/// <summary>Caso de uso: buscar facturas con filtros y paginación (en servidor).</summary>
public sealed class BuscarFacturas
{
    private readonly IConsultaFacturas _consulta;

    public BuscarFacturas(IConsultaFacturas consulta) => _consulta = consulta;

    public Task<Nucleo.Consultas.PaginaResultado<FacturaResumen>> EjecutarAsync(Guid empresaId, FiltroFacturas filtro, Nucleo.Consultas.Paginacion paginacion, CancellationToken ct = default) =>
        _consulta.BuscarAsync(empresaId, filtro, paginacion, ct);
}

/// <summary>Caso de uso: obtener una factura por su identificador.</summary>
public sealed class ObtenerFactura
{
    private readonly IConsultaFacturas _consulta;

    public ObtenerFactura(IConsultaFacturas consulta) => _consulta = consulta;

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _consulta.ObtenerAsync(facturaId, ct).ConfigureAwait(false);
        return factura is null
            ? Resultado.Fallo<FacturaDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."))
            : Resultado.Ok(factura);
    }
}
