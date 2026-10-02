using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Datos para emitir un ticket (factura simplificada) desde el TPV.</summary>
public sealed record EmitirTicketComando(
    IReadOnlyList<LineaComando> Lineas,
    Guid? ClienteId = null,
    string? Serie = null,
    DateOnly? FechaEmision = null,
    Guid? FormaPagoId = null);

/// <summary>
/// Caso de uso del TPV: emite un <b>ticket</b> (factura simplificada). Reutiliza la resolución de
/// líneas y la numeración correlativa; el destinatario es opcional (cliente de contado) y el importe
/// no puede superar el tope de la factura simplificada. Por defecto usa la serie <c>T</c>.
/// </summary>
public sealed class EmitirTicket
{
    /// <summary>Serie por defecto de los tickets.</summary>
    public const string SeriePorDefecto = "T";

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
    private readonly IResolverIvaEmpresa _resolverIva;
    private readonly IReloj _reloj;

    public EmitirTicket(
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
        IResolverIvaEmpresa resolverIva,
        IReloj reloj)
    {
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
        _resolverIva = resolverIva;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, EmitirTicketComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        if (comando.Lineas is null || comando.Lineas.Count == 0)
        {
            return Resultado.Fallo<FacturaDto>(Error.Validacion("ticket.sin_lineas", "El ticket debe tener al menos una línea."));
        }

        // Destinatario opcional: si se indica cliente se congelan sus datos; si no, "cliente de contado".
        var cliente = ClienteFacturado.Contado;
        string? tipoTercero = null;
        Guid? formaPagoDefectoId = null;
        if (comando.ClienteId is not null)
        {
            var datos = await _clientes.ObtenerAsync(comando.ClienteId.Value, ct).ConfigureAwait(false);
            if (datos is null)
            {
                return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
            }

            cliente = new ClienteFacturado(
                datos.Id, datos.Nombre, datos.NifFiscal, datos.Calle, datos.CodigoPostal, datos.Poblacion, datos.Provincia, datos.Pais, datos.ActividadNegocioId);
            tipoTercero = datos.Tipo;
            formaPagoDefectoId = datos.FormaPagoDefectoId;
        }

        var formaPagoId = comando.FormaPagoId ?? formaPagoDefectoId;
        FormaPagoDto? formaPago = formaPagoId is { } fpid
            ? await _formasPago.ObtenerAsync(fpid, ct).ConfigureAwait(false)
            : null;

        var impuesto = EmitirFactura.ImpuestoDeLaOperacion(await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false), comando.Lineas.Select(l => l.CodigoIva));
        var resolucion = await ResolucionLineasFactura.ResolverAsync(comando.Lineas, _productos, ct, false, empresaId, _resolverIva, impuestoEmpresa: impuesto).ConfigureAwait(false);
        if (resolucion.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(resolucion.Error);
        }

        var mencionFiscal = await ResolucionLineasFactura.MencionFiscalAsync(empresaId, resolucion.Valor, _resolverIva, ct).ConfigureAwait(false);
        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = comando.FechaEmision ?? hoy;
        var serie = comando.Serie;
        if (string.IsNullOrWhiteSpace(serie))
        {
            serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.Ticket, comando.ClienteId, ct).ConfigureAwait(false);
        }

        serie = string.IsNullOrWhiteSpace(serie) ? SeriePorDefecto : serie;

        // Número correlativo SIN huecos: se calcula (último + 1) dentro de la misma transacción que
        // guarda la factura y bajo un bloqueo por empresa, de modo que si la emisión falla no se pierde
        // ningún número y dos emisiones simultáneas no se pisan (ni el número ni la cadena VeriFactu).
        var numero = await _facturas.ReservarNumeroAsync(empresaId, serie, fecha, ct).ConfigureAwait(false);
        if (numero.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(numero.Error);
        }

        var numeroFactura = numero.Valor;
        var ticket = Factura.EmitirSimplificada(empresaId, numeroFactura, fecha, cliente, resolucion.Valor, _reloj);
        if (ticket.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(ticket.Error);
        }

        ticket.Valor.EstablecerMencionFiscal(mencionFiscal);
        ticket.Valor.EstablecerImpuesto(impuesto);
        await RegistroVerifactu.AplicarAsync(empresaId, ticket.Valor, _empresas, _facturas, _reloj, ct).ConfigureAwait(false);
        _facturas.Agregar(ticket.Valor);

        // Bandeja de salida (outbox): la contabilización del ticket se encola en la MISMA transacción
        // que el ticket, garantizando atomicidad (igual que una factura ordinaria).
        var t = ticket.Valor;
        var codigoIva = t.Lineas.Count > 0 ? t.Lineas[0].CodigoIva : "IVA21";
        var productoId = t.Lineas.FirstOrDefault(l => l.ProductoId is not null)?.ProductoId;
        string? familia = null;
        if (productoId is { } pid)
        {
            var producto = await _productos.ObtenerAsync(pid, ct).ConfigureAwait(false);
            familia = producto?.Familia;
        }

        _encolarSalida.Contabilizacion(empresaId, new DocumentoContabilizable(
            SentidoContable.Venta, "Ticket", t.Id, t.NumeroCompleto, comando.ClienteId, t.ClienteNombre,
            t.FechaEmision, t.BaseImponible, codigoIva, t.CuotaIva, t.PorcentajeIrpf, t.RetencionIrpf, t.Total, productoId, familia, tipoTercero,
            ActividadNegocioId: t.ActividadNegocioId, Lineas: EmitirFactura.LineasConCuenta(t, await EmitirFactura.ProductosAsync(_productos, t, ct).ConfigureAwait(false))));

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _despacharSalida.EjecutarAsync(ct: ct).ConfigureAwait(false);

        var lineasVenta = t.Lineas
            .Where(l => l.ProductoId is not null)
            .Select(l => new LineaVenta(l.ProductoId!.Value, l.Cantidad))
            .ToList();
        if (lineasVenta.Count > 0)
        {
            await _stock.DescontarVentaAsync(empresaId, lineasVenta, ct).ConfigureAwait(false);
        }

        if (formaPago?.RegistrarPagoAutomatico == true)
        {
            await _pagos.RegistrarCobroTotalAsync(empresaId, t.Id, t.Total, t.FechaEmision, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(FacturaDto.Desde(t));
    }
}
