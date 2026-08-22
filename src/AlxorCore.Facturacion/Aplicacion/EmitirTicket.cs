using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Dominio;
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
    private readonly IServicioNumeracion _numeracion;
    private readonly IResolverSerie _resolverSerie;
    private readonly IRepositorioFacturas _facturas;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoFacturacion _unidadDeTrabajo;
    private readonly IStockVentas _stock;
    private readonly IColaContabilizacion _cola;
    private readonly IConsultaFormasPago _formasPago;
    private readonly IPagosAutomaticos _pagos;
    private readonly IReloj _reloj;

    public EmitirTicket(
        IConsultaClientes clientes,
        IConsultaProductos productos,
        IServicioNumeracion numeracion,
        IResolverSerie resolverSerie,
        IRepositorioFacturas facturas,
        IConsultaEmpresas empresas,
        IUnidadDeTrabajoFacturacion unidadDeTrabajo,
        IStockVentas stock,
        IColaContabilizacion cola,
        IConsultaFormasPago formasPago,
        IPagosAutomaticos pagos,
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
        _cola = cola;
        _formasPago = formasPago;
        _pagos = pagos;
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
                datos.Id, datos.Nombre, datos.NifFiscal, datos.Calle, datos.CodigoPostal, datos.Poblacion, datos.Provincia, datos.Pais);
            tipoTercero = datos.Tipo;
            formaPagoDefectoId = datos.FormaPagoDefectoId;
        }

        var formaPagoId = comando.FormaPagoId ?? formaPagoDefectoId;
        FormaPagoDto? formaPago = formaPagoId is { } fpid
            ? await _formasPago.ObtenerAsync(fpid, ct).ConfigureAwait(false)
            : null;

        var resolucion = await ResolucionLineasFactura.ResolverAsync(comando.Lineas, _productos, ct).ConfigureAwait(false);
        if (resolucion.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(resolucion.Error);
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = comando.FechaEmision ?? hoy;
        var serie = comando.Serie;
        if (string.IsNullOrWhiteSpace(serie))
        {
            serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.Ticket, comando.ClienteId, ct).ConfigureAwait(false);
        }

        serie = string.IsNullOrWhiteSpace(serie) ? SeriePorDefecto : serie;

        var numero = await _numeracion.SiguienteAsync(empresaId, TipoDocumento.Factura, fecha.Year, serie, ct).ConfigureAwait(false);
        if (numero.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(numero.Error);
        }

        var numeroFactura = new NumeroFactura(numero.Valor.Prefijo, numero.Valor.Ejercicio, numero.Valor.Numero);
        var ticket = Factura.EmitirSimplificada(empresaId, numeroFactura, fecha, cliente, resolucion.Valor, _reloj);
        if (ticket.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(ticket.Error);
        }

        await RegistroVerifactu.AplicarAsync(empresaId, ticket.Valor, _empresas, _facturas, _reloj, ct).ConfigureAwait(false);
        _facturas.Agregar(ticket.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        var lineasVenta = ticket.Valor.Lineas
            .Where(l => l.ProductoId is not null)
            .Select(l => new LineaVenta(l.ProductoId!.Value, l.Cantidad))
            .ToList();
        if (lineasVenta.Count > 0)
        {
            await _stock.DescontarVentaAsync(empresaId, lineasVenta, ct).ConfigureAwait(false);
        }

        // Encola la venta para contabilizar (igual que una factura ordinaria): pendiente salvo
        // contabilización automática, y solo en modo Completo.
        var t = ticket.Valor;
        var codigoIva = t.Lineas.Count > 0 ? t.Lineas[0].CodigoIva : "IVA21";
        var productoId = t.Lineas.FirstOrDefault(l => l.ProductoId is not null)?.ProductoId;
        string? familia = null;
        if (productoId is { } pid)
        {
            var producto = await _productos.ObtenerAsync(pid, ct).ConfigureAwait(false);
            familia = producto?.Familia;
        }

        await _cola.EncolarAsync(empresaId, new DocumentoContabilizable(
            SentidoContable.Venta, "Ticket", t.Id, t.NumeroCompleto, comando.ClienteId, t.ClienteNombre,
            t.FechaEmision, t.BaseImponible, codigoIva, t.CuotaIva, t.PorcentajeIrpf, t.RetencionIrpf, t.Total, productoId, familia, tipoTercero), ct).ConfigureAwait(false);

        if (formaPago?.RegistrarPagoAutomatico == true)
        {
            await _pagos.RegistrarCobroTotalAsync(empresaId, t.Id, t.Total, t.FechaEmision, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(FacturaDto.Desde(t));
    }
}
