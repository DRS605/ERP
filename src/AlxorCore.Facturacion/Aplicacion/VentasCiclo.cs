using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

// ----------------------------------------------------------------------------- DTOs
public sealed record LineaPedidoVentaDto(Guid Id, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    decimal PorcentajeDescuento, string CodigoIva, decimal Base, decimal CantidadServida, decimal CantidadFacturada, decimal PendienteServir);

public sealed record PedidoVentaDto(Guid Id, string Estado, int Ejercicio, int Numero, string NumeroCompleto, Guid ClienteId, string ClienteNombre,
    DateOnly Fecha, Guid? PresupuestoOrigenId, Guid? FacturaId, decimal Total, bool ServidoCompleto, IReadOnlyList<LineaPedidoVentaDto> Lineas)
{
    public static PedidoVentaDto Desde(PedidoVenta p) => new(p.Id, p.Estado.ToString(), p.Ejercicio, p.Numero, p.NumeroCompleto, p.ClienteId, p.ClienteNombre,
        p.Fecha, p.PresupuestoOrigenId, p.FacturaId, p.Total, p.ServidoCompleto,
        p.Lineas.Select(l => new LineaPedidoVentaDto(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.PorcentajeDescuento,
            l.CodigoIva, l.Base, l.CantidadServida, l.CantidadFacturada, l.PendienteServir)).ToList());
}

public sealed record LineaAlbaranVentaDto(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad);

public sealed record AlbaranVentaDto(Guid Id, Guid PedidoId, int Numero, string NumeroCompleto, DateOnly Fecha, string? Referencia, IReadOnlyList<LineaAlbaranVentaDto> Lineas)
{
    public static AlbaranVentaDto Desde(AlbaranVenta a) => new(a.Id, a.PedidoId, a.Numero, a.NumeroCompleto, a.Fecha, a.Referencia,
        a.Lineas.Select(l => new LineaAlbaranVentaDto(l.LineaPedidoId, l.ProductoId, l.Descripcion, l.Cantidad)).ToList());
}

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
    Task<IReadOnlyList<AlbaranVentaDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Comandos
public sealed record LineaPedidoVentaComando(string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    string? CodigoIva = "IVA21", decimal PorcentajeDescuento = 0m, Guid? ProductoId = null);

public sealed record CrearPedidoVentaComando(Guid ClienteId, IReadOnlyList<LineaPedidoVentaComando> Lineas, DateOnly? Fecha = null);

public sealed record CrearPedidoDesdePresupuestoComando(Guid PresupuestoId, DateOnly? Fecha = null);

public sealed record EntregaLineaComando(Guid LineaPedidoId, decimal Cantidad);

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

    public CrearPedidoVenta(IRepositorioPedidosVenta pedidos, IRepositorioPresupuestos presupuestos, IConsultaClientes clientes,
        IResolverSerie resolverSerie, IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
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

        return await CrearInternoAsync(empresaId, comando.ClienteId, cliente.Nombre, comando.Fecha, null, lineas, ct).ConfigureAwait(false);
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
        return await CrearInternoAsync(empresaId, presupuesto.ClienteId, presupuesto.ClienteNombre, comando.Fecha, presupuesto.Id, lineas, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<PedidoVentaDto>> CrearInternoAsync(Guid empresaId, Guid clienteId, string clienteNombre, DateOnly? fechaOpt, Guid? presupuestoId,
        List<(Guid?, string, decimal, decimal, decimal, string)> lineas, CancellationToken ct)
    {
        var fecha = fechaOpt ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _pedidos.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.PedidoVenta, clienteId, ct).ConfigureAwait(false);
        var pedido = PedidoVenta.Crear(empresaId, clienteId, clienteNombre, fecha, numero, presupuestoId, lineas, _reloj, serie);
        if (pedido.EsFallo)
        {
            return Resultado.Fallo<PedidoVentaDto>(pedido.Error);
        }

        _pedidos.Agregar(pedido.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoVentaDto.Desde(pedido.Valor));
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

    public EntregarPedido(IRepositorioPedidosVenta pedidos, IRepositorioAlbaranesVenta albaranes, IResolverSerie resolverSerie, IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
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

        var lineasAlbaran = entregas.Select(e =>
        {
            var lp = pedido.Lineas.Single(l => l.Id == e.LineaPedidoId);
            return (e.LineaPedidoId, lp.ProductoId, lp.Descripcion, e.Cantidad);
        }).ToList();

        var albaran = AlbaranVenta.Crear(empresaId, pedidoId, pedido.ClienteId, pedido.ClienteNombre, numero, fecha, comando.Referencia, lineasAlbaran, _reloj, serie);
        if (albaran.EsFallo)
        {
            return Resultado.Fallo<AlbaranVentaDto>(albaran.Error);
        }

        _albaranes.Agregar(albaran.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranVentaDto.Desde(albaran.Valor));
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

    public FacturarPedidoVenta(IRepositorioPedidosVenta pedidos, EmitirFactura emitir, IUnidadDeTrabajoFacturacion unidad)
    {
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

        var lineas = pedido.Lineas.Select(l => new LineaComando(
            l.Cantidad, l.Descripcion, l.PrecioUnitario, l.CodigoIva, l.PorcentajeDescuento, l.ProductoId)).ToList();

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

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(factura.Valor);
    }
}
