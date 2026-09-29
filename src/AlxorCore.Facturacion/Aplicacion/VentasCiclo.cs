using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

// ----------------------------------------------------------------------------- DTOs
public sealed record LineaPedidoVentaDto(Guid Id, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    decimal PorcentajeDescuento, string CodigoIva, decimal Base, decimal CantidadServida, decimal CantidadFacturada, decimal PendienteServir, IReadOnlyList<ConceptoAplicado>? Conceptos = null, decimal ImporteConceptos = 0m, decimal CosteConceptos = 0m);

public sealed record PedidoVentaDto(Guid Id, string Estado, int Ejercicio, int Numero, string NumeroCompleto, Guid ClienteId, string ClienteNombre,
    DateOnly Fecha, Guid? PresupuestoOrigenId, Guid? FacturaId, decimal Total, bool ServidoCompleto, IReadOnlyList<LineaPedidoVentaDto> Lineas, decimal Suplidos = 0m)
{
    public static PedidoVentaDto Desde(PedidoVenta p) => new(p.Id, p.Estado.ToString(), p.Ejercicio, p.Numero, p.NumeroCompleto, p.ClienteId, p.ClienteNombre,
        p.Fecha, p.PresupuestoOrigenId, p.FacturaId, p.Total, p.ServidoCompleto,
        p.Lineas.Select(l => new LineaPedidoVentaDto(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento,
            l.CodigoIva, l.Base, l.CantidadServida, l.CantidadFacturada, l.PendienteServir, l.Conceptos, l.ImporteConceptos, l.CosteConceptos)).ToList(), p.Suplidos);
}

public sealed record LineaAlbaranVentaDto(Guid? LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad,
    int Orden = 0, decimal PrecioUnitario = 0m, decimal PorcentajeDescuento = 0m, string CodigoIva = "IVA21", bool PrecioFijado = true, decimal Base = 0m,
    IReadOnlyList<ConceptoAplicado>? Conceptos = null, decimal ImporteConceptos = 0m, decimal CosteConceptos = 0m);

public sealed record AlbaranVentaDto(Guid Id, Guid? PedidoId, int Numero, string NumeroCompleto, DateOnly Fecha, string? Referencia, IReadOnlyList<LineaAlbaranVentaDto> Lineas,
    bool Anulado = false, string? MotivoAnulacion = null, Guid ClienteId = default, string ClienteNombre = "", string Estado = "PendienteFacturar",
    decimal Base = 0m, Guid? FacturaId = null, string? Observaciones = null)
{
    public static AlbaranVentaDto Desde(AlbaranVenta a) => new(a.Id, a.PedidoId, a.Numero, a.NumeroCompleto, a.Fecha, a.Referencia,
        a.Lineas.Select(l => new LineaAlbaranVentaDto(l.LineaPedidoId, l.ProductoId, l.Descripcion, l.Cantidad, l.Orden, l.PrecioUnitario, l.PorcentajeDescuento,
            l.CodigoIva, l.PrecioFijado, l.Base, l.Conceptos, l.ImporteConceptos, l.CosteConceptos)).ToList(),
        a.AnuladoEn is not null, a.MotivoAnulacion, a.ClienteId, a.ClienteNombre, a.Estado.ToString(), a.Base, a.FacturaId, a.Observaciones);
}

/// <summary>Filtro del listado de albaranes de venta.</summary>
public sealed record FiltroAlbaranesVenta(Guid? ClienteId = null, EstadoAlbaranVenta? Estado = null, DateOnly? Desde = null, DateOnly? Hasta = null, Guid? FacturaId = null);

// ----------------------------------------------------------------------------- Puertos
public interface IRepositorioPedidosVenta
{
    Task<PedidoVenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(PedidoVenta pedido);
    Task<IReadOnlyList<PedidoVentaDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<PedidoVentaDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

public interface IRepositorioAlbaranesVenta
{
    void Agregar(AlbaranVenta albaran);
    Task<AlbaranVenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<AlbaranVenta?>(null);
    Task<IReadOnlyList<AlbaranVentaDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);

    /// <summary>Albaranes de la empresa según el filtro (sin seguimiento), del más reciente al más antiguo.</summary>
    Task<IReadOnlyList<AlbaranVentaDto>> ListarAsync(Guid empresaId, FiltroAlbaranesVenta filtro, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlbaranVentaDto>>([]);

    /// <summary>Albaranes (con seguimiento, para modificarlos) por sus identificadores.</summary>
    Task<IReadOnlyList<AlbaranVenta>> ObtenerVariosAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlbaranVenta>>([]);

    /// <summary>Albaranes (con seguimiento) que recoge una factura.</summary>
    Task<IReadOnlyList<AlbaranVenta>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlbaranVenta>>([]);

    /// <summary>Albaranes (con seguimiento) de un pedido.</summary>
    Task<IReadOnlyList<AlbaranVenta>> DePedidoAsync(Guid pedidoId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlbaranVenta>>([]);
}

// ----------------------------------------------------------------------------- Comandos
public sealed record LineaPedidoVentaComando(string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    string? CodigoIva = "IVA21", decimal PorcentajeDescuento = 0m, Guid? ProductoId = null, IReadOnlyList<ConceptoSolicitado>? Conceptos = null);

public sealed record CrearPedidoVentaComando(Guid ClienteId, IReadOnlyList<LineaPedidoVentaComando> Lineas, DateOnly? Fecha = null,
    IReadOnlyList<ConceptoSolicitado>? ConceptosDocumento = null);

public sealed record CrearPedidoDesdePresupuestoComando(Guid PresupuestoId, DateOnly? Fecha = null);

/// <summary>Lo que se entrega de una línea del pedido; con bultos o palés, los conceptos por bulto o palé van por lo real.</summary>
public sealed record EntregaLineaComando(Guid LineaPedidoId, decimal Cantidad, decimal? CosteUnitario = null, decimal? Bultos = null, decimal? Pales = null);

public sealed record EntregarPedidoComando(IReadOnlyList<EntregaLineaComando> Lineas, DateOnly? Fecha = null, string? Referencia = null);

public sealed record FacturarPedidoVentaComando(DateOnly? FechaEmision = null, int? DiasVencimiento = null, Guid? FormaPagoId = null);

// ----------------------------------------------------------------------------- Pedidos
public sealed class CrearPedidoVenta
{
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly IRepositorioPresupuestos _presupuestos;
    private readonly IConsultaClientes _clientes;
    private readonly IResolverSerie _resolverSerie;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;
    private readonly IResolverConceptos? _conceptos;

    public CrearPedidoVenta(IRepositorioPedidosVenta pedidos, IRepositorioPresupuestos presupuestos, IConsultaClientes clientes,
        IResolverSerie resolverSerie, IUnidadDeTrabajoFacturacion unidad, IReloj reloj, IResolverConceptos? conceptos = null)
    {
        _conceptos = conceptos;
        _pedidos = pedidos;
        _presupuestos = presupuestos;
        _clientes = clientes;
        _resolverSerie = resolverSerie;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<PedidoVentaDto>> EjecutarAsync(Guid empresaId, CrearPedidoVentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var cliente = await _clientes.ObtenerAsync(comando.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<PedidoVentaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var lineas = (comando.Lineas ?? Array.Empty<LineaPedidoVentaComando>())
            .Select(l => ((Guid?)l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento, l.CodigoIva ?? "IVA21")).ToList();

        var solicitados = (comando.Lineas ?? []).Select(l => (l.Conceptos, (IReadOnlyList<ConceptoAplicado>?)null)).ToList();
        return await CrearInternoAsync(empresaId, comando.ClienteId, cliente.Nombre, comando.Fecha, null, lineas, solicitados, comando.ConceptosDocumento, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<PedidoVentaDto>> DesdePresupuestoAsync(Guid empresaId, CrearPedidoDesdePresupuestoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var presupuesto = await _presupuestos.ObtenerPorIdAsync(comando.PresupuestoId, ct).ConfigureAwait(false);
        if (presupuesto is null)
        {
            return Resultado.Fallo<PedidoVentaDto>(Error.NoEncontrado("presupuesto.no_encontrado", "No se encontró el presupuesto."));
        }

        var lineas = presupuesto.Lineas
            .Select(l => ((Guid?)l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento, l.CodigoIva)).ToList();
        var copiados = presupuesto.Lineas.Select(l => ((IReadOnlyList<ConceptoSolicitado>?)null, (IReadOnlyList<ConceptoAplicado>?)l.Conceptos)).ToList();
        return await CrearInternoAsync(empresaId, presupuesto.ClienteId, presupuesto.ClienteNombre, comando.Fecha, presupuesto.Id, lineas, copiados, null, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<PedidoVentaDto>> CrearInternoAsync(Guid empresaId, Guid clienteId, string clienteNombre, DateOnly? fechaOpt, Guid? presupuestoId,
        List<(Guid?, string, decimal, decimal, decimal, string)> lineas,
        IReadOnlyList<(IReadOnlyList<ConceptoSolicitado>? Pedidos, IReadOnlyList<ConceptoAplicado>? Copiados)> conceptos,
        IReadOnlyList<ConceptoSolicitado>? documento, CancellationToken ct)
    {
        var fecha = fechaOpt ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _pedidos.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.PedidoVenta, clienteId, ct).ConfigureAwait(false);
        var pedido = PedidoVenta.Crear(empresaId, clienteId, clienteNombre, fecha, numero, presupuestoId, lineas, _reloj, serie);
        if (pedido.EsFallo)
        {
            return Resultado.Fallo<PedidoVentaDto>(pedido.Error);
        }

        if (await PonerConceptosAsync(pedido.Valor, conceptos, documento, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<PedidoVentaDto>(error);
        }

        _pedidos.Agregar(pedido.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoVentaDto.Desde(pedido.Valor));
    }

    /// <summary>Modifica un pedido aún sin entregas ni factura (cliente, fecha y líneas).</summary>
    public async Task<Resultado<PedidoVentaDto>> ModificarAsync(Guid pedidoId, CrearPedidoVentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<PedidoVentaDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "El pedido no existe."));
        }

        var cliente = await _clientes.ObtenerAsync(comando.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<PedidoVentaDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var lineas = (comando.Lineas ?? Array.Empty<LineaPedidoVentaComando>())
            .Select(l => ((Guid?)l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento, l.CodigoIva ?? "IVA21")).ToList();
        var r = pedido.Modificar(comando.ClienteId, cliente.Nombre, comando.Fecha ?? pedido.Fecha, lineas);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PedidoVentaDto>(r.Error);
        }

        var solicitados = (comando.Lineas ?? []).Select(l => (l.Conceptos, (IReadOnlyList<ConceptoAplicado>?)null)).ToList();
        if (await PonerConceptosAsync(pedido, solicitados, comando.ConceptosDocumento, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<PedidoVentaDto>(error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoVentaDto.Desde(pedido));
    }

    private async Task<Error?> PonerConceptosAsync(PedidoVenta pedido,
        IReadOnlyList<(IReadOnlyList<ConceptoSolicitado>? Pedidos, IReadOnlyList<ConceptoAplicado>? Copiados)> conceptos,
        IReadOnlyList<ConceptoSolicitado>? documento, CancellationToken ct)
    {
        if (_conceptos is null)
        {
            return null;
        }

        var entrada = pedido.Lineas.Select((l, i) => new LineaConceptos(l.ProductoId, l.Cantidad, l.BaseBruta,
            i < conceptos.Count ? conceptos[i].Pedidos : null, i < conceptos.Count ? conceptos[i].Copiados : null)).ToList();
        var cliente = await _clientes.ObtenerAsync(pedido.ClienteId, ct).ConfigureAwait(false);
        var r = await _conceptos.ResolverAsync(AmbitoConcepto.Ventas, pedido.ClienteId, entrada, documento, true, new ContextoConceptos(cliente?.Tipo, pedido.Fecha), ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return r.Error;
        }

        var puestos = pedido.PonerConceptos(r.Valor);
        return puestos.EsFallo ? puestos.Error : null;
    }
}

public sealed class DecidirPedidoVenta
{
    private readonly IRepositorioPedidosVenta _repo;
    private readonly IUnidadDeTrabajoFacturacion _unidad;

    public DecidirPedidoVenta(IRepositorioPedidosVenta repo, IUnidadDeTrabajoFacturacion unidad)
    {
        _repo = repo;
        _unidad = unidad;
    }

    public Task<Resultado<PedidoVentaDto>> ConfirmarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Confirmar(), ct);

    public Task<Resultado<PedidoVentaDto>> CancelarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Cancelar(), ct);

    private async Task<Resultado<PedidoVentaDto>> CambiarAsync(Guid id, Func<PedidoVenta, Resultado> accion, CancellationToken ct)
    {
        var pedido = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<PedidoVentaDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido."));
        }

        var r = accion(pedido);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PedidoVentaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoVentaDto.Desde(pedido));
    }
}

public sealed class ListarPedidosVenta
{
    private readonly IRepositorioPedidosVenta _repo;
    public ListarPedidosVenta(IRepositorioPedidosVenta repo) => _repo = repo;
    public Task<IReadOnlyList<PedidoVentaDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);
}

public sealed class ObtenerPedidoVenta
{
    private readonly IRepositorioPedidosVenta _repo;
    public ObtenerPedidoVenta(IRepositorioPedidosVenta repo) => _repo = repo;
    public Task<PedidoVentaDto?> EjecutarAsync(Guid id, CancellationToken ct = default) => _repo.ObtenerDtoAsync(id, ct);
}

// ----------------------------------------------------------------------------- Entrega (albarán de venta)
public sealed class EntregarPedido
{
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IResolverSerie _resolverSerie;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    private readonly IStockVentas? _stock;

    public EntregarPedido(IRepositorioPedidosVenta pedidos, IRepositorioAlbaranesVenta albaranes, IResolverSerie resolverSerie, IUnidadDeTrabajoFacturacion unidad, IReloj reloj,
        IStockVentas? stock = null)
    {
        _stock = stock;
        _pedidos = pedidos;
        _albaranes = albaranes;
        _resolverSerie = resolverSerie;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AlbaranVentaDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, EntregarPedidoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido."));
        }

        var entregas = (comando.Lineas ?? Array.Empty<EntregaLineaComando>()).Select(l => (l.LineaPedidoId, l.Cantidad)).ToList();
        if (entregas.Count == 0)
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.Validacion("albaranventa.sin_lineas", "Indica qué se ha entregado."));
        }

        var registro = pedido.RegistrarEntrega(entregas);
        if (registro.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(registro.Error);
        }

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _albaranes.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.AlbaranVenta, pedido.ClienteId, ct).ConfigureAwait(false);

        var costes = (comando.Lineas ?? []).Select(l => l.CosteUnitario).ToList();
        var lineasAlbaran = entregas.Select((e, i) =>
        {
            var lp = pedido.Lineas.Single(l => l.Id == e.LineaPedidoId);
            var bruta = Redondeo.Dos(e.Cantidad * lp.PrecioUnitario * (1m - lp.PorcentajeDescuento / 100m));
            return new NuevaLineaAlbaran(e.LineaPedidoId, lp.ProductoId, lp.Descripcion, e.Cantidad, lp.PrecioUnitario, lp.PorcentajeDescuento, lp.CodigoIva,
                Conceptos: AlbaranesVentaStock.ConceptosParciales(lp.Conceptos, lp.Cantidad, e.Cantidad, bruta, (comando.Lineas ?? [])[i].Bultos, (comando.Lineas ?? [])[i].Pales), CosteUnitario: costes[i]);
        }).ToList();

        var albaran = AlbaranVenta.Crear(empresaId, pedidoId, pedido.ClienteId, pedido.ClienteNombre, numero, fecha, comando.Referencia, lineasAlbaran, _reloj, serie);
        if (albaran.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(albaran.Error);
        }

        _albaranes.Agregar(albaran.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await AlbaranesVentaStock.SacarAsync(_stock, empresaId, albaran.Valor, ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranVentaDto.Desde(albaran.Valor));
    }
}

/// <summary>
/// Caso de uso: anular un albarán de venta (la entrega no se hizo o se registró mal). Sus cantidades vuelven a quedar
/// pendientes de servir; si el pedido ya está facturado, no se anula (se corrige con una rectificativa).
/// </summary>
public sealed class AnularAlbaranVenta
{
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    private readonly IStockVentas? _stock;
    private readonly IRepositorioDevolucionesVenta? _devoluciones;

    public AnularAlbaranVenta(IRepositorioPedidosVenta pedidos, IRepositorioAlbaranesVenta albaranes, IUnidadDeTrabajoFacturacion unidad, IReloj reloj,
        IStockVentas? stock = null, IRepositorioDevolucionesVenta? devoluciones = null)
    {
        _devoluciones = devoluciones;
        _stock = stock;
        _pedidos = pedidos;
        _albaranes = albaranes;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<Resultado<AlbaranVentaDto>> EjecutarAsync(Guid? pedidoId, Guid albaranId, string? motivo, CancellationToken ct = default)
    {
        var albaran = await _albaranes.ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
        if (albaran is null || (pedidoId is { } p && albaran.PedidoId != p))
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("albaranventa.no_encontrado", "El albarán no existe en ese pedido."));
        }

        PedidoVenta? pedido = null;
        if (albaran.PedidoId is { } pedidoAlbaran)
        {
            pedido = await _pedidos.ObtenerPorIdAsync(pedidoAlbaran, ct).ConfigureAwait(false);
            if (pedido is null)
            {
                return Resultado.Fallo<AlbaranVentaDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido."));
            }
        }

        if (_devoluciones is not null
            && (await _devoluciones.DeAlbaranesAsync([albaran.Id], ct).ConfigureAwait(false)).Any(d => d.Estado != EstadoDevolucionVenta.Anulada))
        {
            return Resultado.Fallo<AlbaranVentaDto>(Error.Conflicto("albaranventa.con_devoluciones", "El albarán tiene devoluciones: anúlalas antes."));
        }

        var anulado = albaran.Anular(motivo, _reloj);
        if (anulado.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(anulado.Error);
        }

        if (pedido is not null)
        {
            var deshecho = pedido.DeshacerEntrega(albaran.Lineas.Where(l => l.LineaPedidoId is not null).Select(l => (l.LineaPedidoId!.Value, l.Cantidad)).ToList());
            if (deshecho.EsFallo)
            {
                return Resultado.Fallo<AlbaranVentaDto>(deshecho.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await AlbaranesVentaStock.DevolverAsync(_stock, albaran, ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranVentaDto.Desde(albaran));
    }
}

public sealed class ListarAlbaranesVenta
{
    private readonly IRepositorioAlbaranesVenta _repo;
    public ListarAlbaranesVenta(IRepositorioAlbaranesVenta repo) => _repo = repo;
    public Task<IReadOnlyList<AlbaranVentaDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default) => _repo.ListarPorPedidoAsync(empresaId, pedidoId, ct);
}

// ----------------------------------------------------------------------------- Facturación del pedido
/// <summary>
/// Factura un pedido de venta: genera una <b>factura real</b> con toda su maquinaria (numeración
/// correlativa, VeriFactu, salida de existencias, vencimientos y contabilización) reutilizando
/// <see cref="EmitirFactura"/>, y enlaza la factura al pedido.
/// </summary>
public sealed class FacturarPedidoVenta
{
    private readonly IRepositorioPedidosVenta _pedidos;
    private readonly EmitirFactura _emitir;
    private readonly IUnidadDeTrabajoFacturacion _unidad;

    private readonly IRepositorioAlbaranesVenta? _albaranes;

    public FacturarPedidoVenta(IRepositorioPedidosVenta pedidos, EmitirFactura emitir, IUnidadDeTrabajoFacturacion unidad, IRepositorioAlbaranesVenta? albaranes = null)
    {
        _albaranes = albaranes;
        _pedidos = pedidos;
        _emitir = emitir;
        _unidad = unidad;
    }

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, FacturarPedidoVentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido."));
        }

        if (pedido.Estado is EstadoPedidoVenta.Facturado)
        {
            return Resultado.Fallo<FacturaDto>(Error.Conflicto("pedidoventa.ya_facturado", "El pedido ya está facturado."));
        }

        if (pedido.Estado is EstadoPedidoVenta.Borrador)
        {
            return Resultado.Fallo<FacturaDto>(Error.Conflicto("pedidoventa.no_confirmado", "Confirma el pedido antes de facturarlo."));
        }

        if (pedido.Estado is EstadoPedidoVenta.Cancelado)
        {
            return Resultado.Fallo<FacturaDto>(Error.Conflicto("pedidoventa.cancelado", "El pedido está cancelado."));
        }

        // Con albaranes, la factura recoge los pendientes (su mercancía ya salió) y lo que quede sin servir; sin albaranes, el pedido entero.
        var albaranes = _albaranes is null ? [] : (await _albaranes.DePedidoAsync(pedido.Id, ct).ConfigureAwait(false)).Where(a => a.AnuladoEn is null).ToList();
        var pendientes = albaranes.Where(a => a.FacturaId is null).OrderBy(a => a.Fecha).ThenBy(a => a.Numero).ToList();
        foreach (var a in pendientes)
        {
            if (a.PuedeFacturarse() is { EsFallo: true } no)
            {
                return Resultado.Fallo<FacturaDto>(no.Error);
            }
        }

        List<LineaComando> lineas;
        if (albaranes.Count == 0)
        {
            lineas = pedido.Lineas.Select(l => new LineaComando(
                l.Cantidad, l.Descripcion, l.PrecioUnitario, l.CodigoIva, l.PorcentajeDescuento, l.ProductoId, ConceptosCopiados: l.Conceptos)).ToList();
        }
        else
        {
            lineas = [.. pendientes.SelectMany(AlbaranesVentaStock.LineasFactura),
                .. pedido.Lineas.Where(l => l.Cantidad - l.CantidadServida > 0m).Select(l => new LineaComando(
                    l.Cantidad - l.CantidadServida, l.Descripcion, l.PrecioUnitario, l.CodigoIva, l.PorcentajeDescuento, l.ProductoId))];
            if (lineas.Count == 0)
            {
                return Resultado.Fallo<FacturaDto>(Error.Conflicto("pedidoventa.albaranes_facturados", "Los albaranes del pedido ya están facturados."));
            }
        }

        var comandoFactura = new EmitirFacturaComando(
            pedido.ClienteId, lineas, comando.FechaEmision, null, null, null, comando.DiasVencimiento, false, comando.FormaPagoId);

        var factura = await _emitir.EjecutarAsync(empresaId, comandoFactura, ct).ConfigureAwait(false);
        if (factura.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(factura.Error);
        }

        var marcado = pedido.Facturar(factura.Valor.Id);
        if (marcado.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(marcado.Error);
        }

        foreach (var a in pendientes)
        {
            a.Facturar(factura.Valor.Id);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(factura.Valor);
    }
}
