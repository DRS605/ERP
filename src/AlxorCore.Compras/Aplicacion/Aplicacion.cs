using AlxorCore.Compras.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Recepcion.Aplicacion;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Compras.Aplicacion;

// ---------------------------------------------------------------------------- DTOs
public sealed record LineaSolicitudDto(Guid Id, string Descripcion, decimal Cantidad);

public sealed record SolicitudDto(Guid Id, string Estado, string? ProveedorSugerido, string? Notas,
    DateTimeOffset CreadoEn, IReadOnlyList<LineaSolicitudDto> Lineas)
{
    public static SolicitudDto Desde(SolicitudCompra s) => new(s.Id, s.Estado.ToString(), s.ProveedorSugerido, s.Notas,
        s.CreadoEn, s.Lineas.Select(l => new LineaSolicitudDto(l.Id, l.Descripcion, l.Cantidad)).ToList());
}

public sealed record LineaPedidoDto(Guid Id, string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    decimal Importe, decimal CantidadRecibida, decimal CantidadFacturada, decimal PendienteRecibir);

public sealed record PedidoDto(Guid Id, string Estado, Guid? ProveedorId, string ProveedorTexto, DateOnly Fecha,
    Guid? SolicitudOrigenId, decimal Total, bool RecibidoCompleto, IReadOnlyList<LineaPedidoDto> Lineas)
{
    public static PedidoDto Desde(PedidoCompra p) => new(p.Id, p.Estado.ToString(), p.ProveedorId, p.ProveedorTexto,
        p.Fecha, p.SolicitudOrigenId, p.Total, p.RecibidoCompleto,
        p.Lineas.Select(l => new LineaPedidoDto(l.Id, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.Importe,
            l.CantidadRecibida, l.CantidadFacturada, l.PendienteRecibir)).ToList());
}

public sealed record LineaAlbaranDto(Guid LineaPedidoId, string Descripcion, decimal Cantidad);

public sealed record AlbaranDto(Guid Id, Guid PedidoId, DateOnly Fecha, string? Referencia, IReadOnlyList<LineaAlbaranDto> Lineas)
{
    public static AlbaranDto Desde(AlbaranCompra a) => new(a.Id, a.PedidoId, a.Fecha, a.Referencia,
        a.Lineas.Select(l => new LineaAlbaranDto(l.LineaPedidoId, l.Descripcion, l.Cantidad)).ToList());
}

// ---------------------------------------------------------------------------- Puertos
public interface IRepositorioSolicitudes
{
    Task<SolicitudCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(SolicitudCompra solicitud);
    Task<IReadOnlyList<SolicitudDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<SolicitudDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
}

public interface IRepositorioPedidos
{
    Task<PedidoCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(PedidoCompra pedido);
    Task<IReadOnlyList<PedidoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<PedidoDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
}

public interface IRepositorioAlbaranes
{
    void Agregar(AlbaranCompra albaran);
    Task<IReadOnlyList<AlbaranDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default);
}

public interface IUnidadDeTrabajoCompras : IUnidadDeTrabajo;

// ---------------------------------------------------------------------------- Comandos
public sealed record LineaSolicitudComando(string Descripcion, decimal Cantidad);
public sealed record CrearSolicitudComando(IReadOnlyList<LineaSolicitudComando> Lineas, string? ProveedorSugerido = null, string? Notas = null);

public sealed record LineaPedidoComando(string Descripcion, decimal Cantidad, decimal PrecioUnitario);
public sealed record CrearPedidoComando(string? ProveedorTexto, IReadOnlyList<LineaPedidoComando> Lineas,
    Guid? ProveedorId = null, DateOnly? Fecha = null, Guid? SolicitudOrigenId = null);

public sealed record RecepcionLineaComando(Guid LineaPedidoId, decimal Cantidad);
public sealed record RecibirMercanciaComando(IReadOnlyList<RecepcionLineaComando> Lineas, DateOnly? Fecha = null, string? Referencia = null);

public sealed record FacturarPedidoComando(string? CodigoIva = "IVA21", decimal PorcentajeIrpf = 0m);

// ---------------------------------------------------------------------------- Solicitudes
public sealed class CrearSolicitud
{
    private readonly IRepositorioSolicitudes _repo;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;

    public CrearSolicitud(IRepositorioSolicitudes repo, IUnidadDeTrabajoCompras unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<SolicitudDto>> EjecutarAsync(Guid empresaId, CrearSolicitudComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var lineas = (comando.Lineas ?? Array.Empty<LineaSolicitudComando>()).Select(l => (l.Descripcion, l.Cantidad)).ToList();
        var solicitud = SolicitudCompra.Crear(empresaId, comando.ProveedorSugerido, comando.Notas, lineas, _reloj);
        if (solicitud.EsFallo)
        {
            return Resultado.Fallo<SolicitudDto>(solicitud.Error);
        }

        _repo.Agregar(solicitud.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SolicitudDto.Desde(solicitud.Valor));
    }
}

public sealed class DecidirSolicitud
{
    private readonly IRepositorioSolicitudes _repo;
    private readonly IUnidadDeTrabajoCompras _unidad;

    public DecidirSolicitud(IRepositorioSolicitudes repo, IUnidadDeTrabajoCompras unidad) { _repo = repo; _unidad = unidad; }

    public async Task<Resultado<SolicitudDto>> AprobarAsync(Guid id, CancellationToken ct = default)
        => await CambiarAsync(id, s => s.Aprobar(), ct).ConfigureAwait(false);

    public async Task<Resultado<SolicitudDto>> RechazarAsync(Guid id, string? motivo, CancellationToken ct = default)
        => await CambiarAsync(id, s => s.Rechazar(motivo), ct).ConfigureAwait(false);

    private async Task<Resultado<SolicitudDto>> CambiarAsync(Guid id, Func<SolicitudCompra, Resultado> accion, CancellationToken ct)
    {
        var solicitud = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (solicitud is null)
        {
            return Resultado.Fallo<SolicitudDto>(Error.NoEncontrado("solicitud.no_encontrada", "No se encontró la solicitud."));
        }

        var r = accion(solicitud);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SolicitudDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SolicitudDto.Desde(solicitud));
    }
}

public sealed class ListarSolicitudes
{
    private readonly IRepositorioSolicitudes _repo;
    public ListarSolicitudes(IRepositorioSolicitudes repo) => _repo = repo;
    public Task<IReadOnlyList<SolicitudDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);
}

// ---------------------------------------------------------------------------- Pedidos
public sealed class CrearPedido
{
    private readonly IRepositorioPedidos _pedidos;
    private readonly IRepositorioSolicitudes _solicitudes;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;

    public CrearPedido(IRepositorioPedidos pedidos, IRepositorioSolicitudes solicitudes, IConsultaProveedores proveedores, IUnidadDeTrabajoCompras unidad, IReloj reloj)
    {
        _pedidos = pedidos; _solicitudes = solicitudes; _proveedores = proveedores; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<PedidoDto>> EjecutarAsync(Guid empresaId, CrearPedidoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var proveedorTexto = comando.ProveedorTexto;
        if (comando.ProveedorId is { } provId)
        {
            var proveedor = await _proveedores.ObtenerAsync(provId, ct).ConfigureAwait(false);
            if (proveedor is null)
            {
                return Resultado.Fallo<PedidoDto>(Error.Validacion("pedido.proveedor_desconocido", "El proveedor indicado no existe."));
            }

            proveedorTexto = proveedor.Nombre;
        }

        SolicitudCompra? solicitud = null;
        if (comando.SolicitudOrigenId is { } solId)
        {
            solicitud = await _solicitudes.ObtenerPorIdAsync(solId, ct).ConfigureAwait(false);
            if (solicitud is null)
            {
                return Resultado.Fallo<PedidoDto>(Error.NoEncontrado("solicitud.no_encontrada", "No se encontró la solicitud de origen."));
            }
        }

        var lineas = (comando.Lineas ?? Array.Empty<LineaPedidoComando>()).Select(l => (l.Descripcion, l.Cantidad, l.PrecioUnitario)).ToList();
        var pedido = PedidoCompra.Crear(empresaId, comando.ProveedorId, proveedorTexto, comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), comando.SolicitudOrigenId, lineas, _reloj);
        if (pedido.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(pedido.Error);
        }

        if (solicitud is not null)
        {
            var conv = solicitud.MarcarConvertida();
            if (conv.EsFallo)
            {
                return Resultado.Fallo<PedidoDto>(conv.Error);
            }
        }

        _pedidos.Agregar(pedido.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoDto.Desde(pedido.Valor));
    }
}

public sealed class DecidirPedido
{
    private readonly IRepositorioPedidos _repo;
    private readonly IUnidadDeTrabajoCompras _unidad;

    public DecidirPedido(IRepositorioPedidos repo, IUnidadDeTrabajoCompras unidad) { _repo = repo; _unidad = unidad; }

    public Task<Resultado<PedidoDto>> ConfirmarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Confirmar(), ct);
    public Task<Resultado<PedidoDto>> CancelarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Cancelar(), ct);

    private async Task<Resultado<PedidoDto>> CambiarAsync(Guid id, Func<PedidoCompra, Resultado> accion, CancellationToken ct)
    {
        var pedido = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<PedidoDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        var r = accion(pedido);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoDto.Desde(pedido));
    }
}

public sealed class ListarPedidos
{
    private readonly IRepositorioPedidos _repo;
    public ListarPedidos(IRepositorioPedidos repo) => _repo = repo;
    public Task<IReadOnlyList<PedidoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);
}

public sealed class ObtenerPedido
{
    private readonly IRepositorioPedidos _repo;
    public ObtenerPedido(IRepositorioPedidos repo) => _repo = repo;
    public Task<PedidoDto?> EjecutarAsync(Guid id, CancellationToken ct = default) => _repo.ObtenerDtoAsync(id, ct);
}

// ---------------------------------------------------------------------------- Recepción (albarán)
public sealed class RecibirMercancia
{
    private readonly IRepositorioPedidos _pedidos;
    private readonly IRepositorioAlbaranes _albaranes;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;

    public RecibirMercancia(IRepositorioPedidos pedidos, IRepositorioAlbaranes albaranes, IUnidadDeTrabajoCompras unidad, IReloj reloj)
    {
        _pedidos = pedidos; _albaranes = albaranes; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, RecibirMercanciaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<AlbaranDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        var recepciones = (comando.Lineas ?? Array.Empty<RecepcionLineaComando>()).Select(l => (l.LineaPedidoId, l.Cantidad)).ToList();
        if (recepciones.Count == 0)
        {
            return Resultado.Fallo<AlbaranDto>(Error.Validacion("albaran.sin_lineas", "Indica qué se ha recibido."));
        }

        var registro = pedido.RegistrarRecepcion(recepciones);
        if (registro.EsFallo)
        {
            return Resultado.Fallo<AlbaranDto>(registro.Error);
        }

        var lineasAlbaran = recepciones.Select(r =>
        {
            var lp = pedido.Lineas.Single(l => l.Id == r.LineaPedidoId);
            return (r.LineaPedidoId, lp.Descripcion, r.Cantidad);
        }).ToList();

        var albaran = AlbaranCompra.Crear(empresaId, pedidoId, comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), comando.Referencia, lineasAlbaran, _reloj);
        if (albaran.EsFallo)
        {
            return Resultado.Fallo<AlbaranDto>(albaran.Error);
        }

        _albaranes.Agregar(albaran.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranDto.Desde(albaran.Valor));
    }
}

public sealed class ListarAlbaranesPedido
{
    private readonly IRepositorioAlbaranes _repo;
    public ListarAlbaranesPedido(IRepositorioAlbaranes repo) => _repo = repo;
    public Task<IReadOnlyList<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default) => _repo.ListarPorPedidoAsync(empresaId, pedidoId, ct);
}

// ---------------------------------------------------------------------------- Facturación del pedido
/// <summary>
/// Factura un pedido: marca el pedido como facturado y lo contabiliza a través del puerto
/// <see cref="IContabilizador"/> (crea el gasto con IVA soportado y, en modo Completo, el asiento).
/// </summary>
public sealed class FacturarPedido
{
    private readonly IRepositorioPedidos _pedidos;
    private readonly IContabilizador _contabilizador;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;

    public FacturarPedido(IRepositorioPedidos pedidos, IContabilizador contabilizador, IUnidadDeTrabajoCompras unidad, IReloj reloj)
    {
        _pedidos = pedidos; _contabilizador = contabilizador; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<PedidoDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, FacturarPedidoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<PedidoDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        var facturado = pedido.Facturar();
        if (facturado.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(facturado.Error);
        }

        var concepto = $"Compra a {pedido.ProveedorTexto}";
        var datos = new DatosContabilizacion(pedido.ProveedorId, pedido.ProveedorTexto, concepto,
            DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), facturado.Valor,
            string.IsNullOrWhiteSpace(comando.CodigoIva) ? "IVA21" : comando.CodigoIva!, comando.PorcentajeIrpf);

        var contabilizado = await _contabilizador.ContabilizarAsync(empresaId, datos, ct).ConfigureAwait(false);
        if (contabilizado.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(contabilizado.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoDto.Desde(pedido));
    }
}
