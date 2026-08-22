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
    decimal? CosteUnitario = null);

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
    Guid? FormaPagoId = null);

/// <summary>
/// Caso de uso estrella: emitir una factura. Compone cliente (Terceros), productos/impuestos
/// (Catálogo) y numeración correlativa (Organización). El número se asigna de forma atómica
/// <b>después</b> de validar todo, para minimizar el riesgo de huecos (invariante F1).
/// </summary>
public sealed class EmitirFactura
{
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaProductos _productos;
    private readonly IServicioNumeracion _numeracion;
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
    private readonly IReloj _reloj;

    public EmitirFactura(
        IConsultaClientes clientes,
        IConsultaProductos productos,
        IServicioNumeracion numeracion,
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
        IReloj reloj)
    {
        _clientes = clientes;
        _productos = productos;
        _numeracion = numeracion;
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

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, EmitirFacturaComando comando, CancellationToken ct = default)
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

        var resolucion = await ResolucionLineasFactura.ResolverAsync(comando.Lineas, _productos, ct, comando.RecargoEquivalencia).ConfigureAwait(false);
        if (resolucion.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(resolucion.Error);
        }

        var lineas = resolucion.Valor;
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

        var clienteFacturado = new ClienteFacturado(
            cliente.Id, cliente.Nombre, cliente.NifFiscal,
            cliente.Calle, cliente.CodigoPostal, cliente.Poblacion, cliente.Provincia, cliente.Pais);

        // Serie: si no se indica una explícita, se resuelve la asignada al cliente (o la de la empresa).
        var serie = comando.Serie;
        if (string.IsNullOrWhiteSpace(serie))
        {
            serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.Factura, cliente.Id, ct).ConfigureAwait(false);
        }

        // Control de riesgo del cliente: se comprueba ANTES de numerar (para no consumir número si se
        // bloquea). El total proyectado se calcula con una factura provisional (número 0) que se descarta.
        string? avisoRiesgo = null;
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
        var numero = await _numeracion.SiguienteAsync(empresaId, TipoDocumento.Factura, fechaEmision.Year, serie, ct).ConfigureAwait(false);
        if (numero.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(numero.Error);
        }

        var numeroFactura = new NumeroFactura(numero.Valor.Prefijo, numero.Valor.Ejercicio, numero.Valor.Numero);
        var factura = Factura.Emitir(empresaId, numeroFactura, fechaEmision, fechaOperacion, clienteFacturado, lineas, porcentajeIrpf, _reloj, fechaVencimiento);
        if (factura.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(factura.Error);
        }

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
            f.FechaEmision, f.BaseImponible, codigoIva, f.CuotaIva, f.PorcentajeIrpf, f.RetencionIrpf, f.Total, productoId, familia, cliente.Tipo));

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        // Ya confirmada la factura, despacha la bandeja de salida (encola la contabilización). Si fallara,
        // el mensaje queda pendiente y se reintenta; la factura ya está a salvo.
        await _despacharSalida.EjecutarAsync(ct: ct).ConfigureAwait(false);

        // Descuento de existencias de los artículos con control de stock (mejor esfuerzo; la factura ya
        // está emitida y es la verdad fiscal).
        var lineasVenta = f.Lineas
            .Where(l => l.ProductoId is not null)
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
        IReadOnlyList<LineaComando> lineas, IConsultaProductos productos, CancellationToken ct, bool recargoEquivalencia = false)
    {
        var resueltas = new List<NuevaLinea>(lineas.Count);
        foreach (var linea in lineas)
        {
            string? descripcion = linea.Descripcion;
            decimal? precio = linea.PrecioUnitario;
            string? codigoIva = linea.CodigoIva;
            decimal? coste = linea.CosteUnitario;

            if (linea.ProductoId is not null)
            {
                var producto = await productos.ObtenerAsync(linea.ProductoId.Value, ct).ConfigureAwait(false);
                if (producto is null)
                {
                    return Resultado.Fallo<List<NuevaLinea>>(Error.NoEncontrado("producto.no_encontrado", "El producto de una línea no existe."));
                }

                descripcion ??= producto.Nombre;
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

            var impuesto = Impuesto.PorCodigoImpuesto(codigoIva ?? Impuesto.IvaGeneral.Codigo);
            if (impuesto.EsFallo)
            {
                return Resultado.Fallo<List<NuevaLinea>>(impuesto.Error);
            }

            var porcentajeRecargo = recargoEquivalencia ? Impuesto.RecargoEquivalencia(impuesto.Valor.Porcentaje) : 0m;
            resueltas.Add(new NuevaLinea(
                descripcion, linea.Cantidad, precio.Value, impuesto.Valor.Codigo, impuesto.Valor.Porcentaje, linea.PorcentajeDescuento, linea.ProductoId, coste ?? 0m, porcentajeRecargo));
        }

        return Resultado.Ok(resueltas);
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
