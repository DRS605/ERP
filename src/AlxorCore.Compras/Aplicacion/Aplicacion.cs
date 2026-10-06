using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Compras.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;
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

public sealed record LineaPedidoDto(Guid Id, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario,
    decimal Importe, decimal CantidadRecibida, decimal CantidadFacturada, decimal PendienteRecibir, IReadOnlyList<ConceptoAplicado>? Conceptos = null, decimal ImporteConceptos = 0m, decimal CosteConceptos = 0m, decimal CosteUnitarioEntrada = 0m);

public sealed record PedidoDto(Guid Id, string Estado, int Ejercicio, int Numero, string NumeroCompleto, Guid? ProveedorId, string ProveedorTexto, DateOnly Fecha,
    Guid? SolicitudOrigenId, decimal Total, bool RecibidoCompleto, IReadOnlyList<LineaPedidoDto> Lineas, Guid? EmpresaOrigenId = null, Guid? PedidoVentaOrigenId = null)
{
    public static PedidoDto Desde(PedidoCompra p) => new(p.Id, p.Estado.ToString(), p.Ejercicio, p.Numero, p.NumeroCompleto, p.ProveedorId, p.ProveedorTexto,
        p.Fecha, p.SolicitudOrigenId, p.Total, p.RecibidoCompleto,
        p.Lineas.Select(l => new LineaPedidoDto(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.Importe,
            l.CantidadRecibida, l.CantidadFacturada, l.PendienteRecibir, l.Conceptos, l.ImporteConceptos, l.CosteConceptos, l.CosteUnitarioEntrada)).ToList(),
        p.EmpresaOrigenId, p.PedidoVentaOrigenId);
}

public sealed record LineaAlbaranDto(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad);

public sealed record AlbaranDto(Guid Id, Guid PedidoId, int Numero, string NumeroCompleto, DateOnly Fecha, string? Referencia, IReadOnlyList<LineaAlbaranDto> Lineas,
    Guid? AlmacenId = null, bool Anulado = false, string? MotivoAnulacion = null, Guid? AlbaranVentaOrigenId = null)
{
    public static AlbaranDto Desde(AlbaranCompra a) => new(a.Id, a.PedidoId, a.Numero, a.NumeroCompleto, a.Fecha, a.Referencia,
        a.Lineas.Select(l => new LineaAlbaranDto(l.LineaPedidoId, l.ProductoId, l.Descripcion, l.Cantidad)).ToList(), a.AlmacenId, a.AnuladoEn is not null, a.MotivoAnulacion, a.AlbaranVentaOrigenId);
}

// ---------------------------------------------------------------------------- Puertos
public interface IRepositorioSolicitudes
{
    Task<SolicitudCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(SolicitudCompra solicitud);
    void Eliminar(SolicitudCompra solicitud);
    Task<IReadOnlyList<SolicitudDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<SolicitudDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
}

public interface IRepositorioPedidos
{
    Task<PedidoCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(PedidoCompra pedido);
    Task<IReadOnlyList<PedidoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<PedidoDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, Guid? proveedorId, CancellationToken ct = default);
    /// <summary>Pedido vivo (no cancelado) espejo de un pedido de venta de otra empresa del grupo.</summary>
    Task<PedidoCompra?> PorPedidoVentaOrigenAsync(Guid empresaId, Guid pedidoVentaId, CancellationToken ct = default) => Task.FromResult<PedidoCompra?>(null);
}

public interface IRepositorioAlbaranes
{
    void Agregar(AlbaranCompra albaran);
    Task<IReadOnlyList<AlbaranDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
    Task<AlbaranCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) => Task.FromResult<AlbaranCompra?>(null);
    Task<IReadOnlyList<AlbaranCompra>> DePedidoAsync(Guid pedidoId, CancellationToken ct = default) => Task.FromResult<IReadOnlyList<AlbaranCompra>>([]);
    Task<AlbaranCompra?> PorAlbaranVentaOrigenAsync(Guid empresaId, Guid albaranVentaId, CancellationToken ct = default) => Task.FromResult<AlbaranCompra?>(null);

    /// <summary>Albaranes de recepción no anulados con fecha en el periodo (para Intrastat).</summary>
    Task<IReadOnlyList<AlbaranCompra>> EnPeriodoAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<AlbaranCompra>>([]);
}

public interface IRepositorioAlmacenesTraspaso
{
    Task<IReadOnlyList<AlmacenTraspaso>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<AlmacenTraspaso?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    void Agregar(AlmacenTraspaso almacen);
    void Eliminar(AlmacenTraspaso almacen);
}

public interface IUnidadDeTrabajoCompras : IUnidadDeTrabajo;

/// <summary>
/// Puerto para la entrada automática en el almacén al recibir un albarán. Lo implementa la
/// infraestructura sobre el módulo Inventario (resuelve la ubicación por defecto por proveedor+almacén
/// o solo almacén). Mantiene el proyecto de aplicación de Compras sin dependencia del módulo Inventario.
/// </summary>
public interface IEntradaInventarioCompras
{
    Task RegistrarEntradaAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId,
        decimal cantidad, string? referencia, DateOnly fecha, string? lote, decimal costeUnitarioCompra, CancellationToken ct = default);

    /// <summary>Salida del almacén al anular una recepción (deshace la entrada; falla si ya no hay esas existencias).</summary>
    Task<Resultado> RegistrarSalidaAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, decimal cantidad, string? referencia, DateOnly fecha,
        CancellationToken ct = default) => Task.FromResult(Resultado.Ok());

    /// <summary>Salida del almacén por una devolución al proveedor (con su lote o número de serie).</summary>
    Task<Resultado> DevolverAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, decimal cantidad, string? lote, string? referencia, DateOnly fecha,
        CancellationToken ct = default) => RegistrarSalidaAsync(empresaId, productoId, almacenId, proveedorId, cantidad, referencia, fecha, ct);
}

// ---------------------------------------------------------------------------- Comandos
public sealed record LineaSolicitudComando(string Descripcion, decimal Cantidad);
public sealed record CrearSolicitudComando(IReadOnlyList<LineaSolicitudComando> Lineas, string? ProveedorSugerido = null, string? Notas = null);

public sealed record LineaPedidoComando(string Descripcion, decimal Cantidad, decimal PrecioUnitario, Guid? ProductoId = null,
    IReadOnlyList<ConceptoSolicitado>? Conceptos = null);
public sealed record CrearPedidoComando(string? ProveedorTexto, IReadOnlyList<LineaPedidoComando> Lineas,
    Guid? ProveedorId = null, DateOnly? Fecha = null, Guid? SolicitudOrigenId = null, IReadOnlyList<ConceptoSolicitado>? ConceptosDocumento = null);

public sealed record RecepcionLineaComando(Guid LineaPedidoId, decimal Cantidad, string? Lote = null);

/// <summary>
/// Recepción de mercancía contra un pedido. Si se indica <see cref="AlmacenId"/>, las líneas con
/// artículo del catálogo generan una entrada automática de inventario (ubicación por defecto por
/// proveedor+almacén o solo almacén).
/// </summary>
public sealed record RecibirMercanciaComando(IReadOnlyList<RecepcionLineaComando> Lineas, DateOnly? Fecha = null,
    string? Referencia = null, Guid? AlmacenId = null);

public sealed record FacturarPedidoComando(string? CodigoIva = "IVA21", decimal PorcentajeIrpf = 0m, string? NumeroFactura = null, DateOnly? FechaFactura = null);

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

    public async Task<Resultado<SolicitudDto>> ModificarAsync(Guid id, CrearSolicitudComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var solicitud = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (solicitud is null)
        {
            return Resultado.Fallo<SolicitudDto>(Error.NoEncontrado("solicitud.no_encontrada", "No se encontró la solicitud."));
        }

        var lineas = (comando.Lineas ?? Array.Empty<LineaSolicitudComando>()).Select(l => (l.Descripcion, l.Cantidad)).ToList();
        var r = solicitud.Modificar(comando.ProveedorSugerido, comando.Notas, lineas);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SolicitudDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(SolicitudDto.Desde(solicitud));
    }

    /// <summary>Elimina una solicitud en borrador o rechazada (las aprobadas o convertidas quedan como histórico).</summary>
    public async Task<Resultado<BajaDto>> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var solicitud = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (solicitud is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("solicitud.no_encontrada", "No se encontró la solicitud."));
        }

        if (!solicitud.Editable)
        {
            return Resultado.Fallo<BajaDto>(Error.Conflicto("solicitud.no_eliminable", "Una solicitud aprobada o convertida en pedido no se elimina."));
        }

        _repo.Eliminar(solicitud);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
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
    private readonly IResolverSerie _resolverSerie;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;
    private readonly IResolverConceptos? _conceptos;

    public CrearPedido(IRepositorioPedidos pedidos, IRepositorioSolicitudes solicitudes, IConsultaProveedores proveedores, IResolverSerie resolverSerie, IUnidadDeTrabajoCompras unidad, IReloj reloj,
        IResolverConceptos? conceptos = null)
    {
        _pedidos = pedidos; _solicitudes = solicitudes; _proveedores = proveedores; _resolverSerie = resolverSerie; _unidad = unidad; _reloj = reloj; _conceptos = conceptos;
    }

    /// <summary>Pone los conceptos de línea pedidos (o los automáticos del proveedor y el artículo) y los del documento.</summary>
    private async Task<Error?> PonerConceptosAsync(PedidoCompra pedido, CrearPedidoComando comando, CancellationToken ct)
    {
        if (_conceptos is null)
        {
            return null;
        }

        var lineas = comando.Lineas ?? [];
        var entrada = pedido.Lineas.Select((l, i) => new LineaConceptos(l.ProductoId, l.Cantidad, l.ImporteBruto, i < lineas.Count ? lineas[i].Conceptos : null)).ToList();
        var proveedor = pedido.ProveedorId is { } pid ? await _proveedores.ObtenerAsync(pid, ct).ConfigureAwait(false) : null;
        var r = await _conceptos.ResolverAsync(AmbitoConcepto.Compras, pedido.ProveedorId, entrada, comando.ConceptosDocumento, true,
            new ContextoConceptos(proveedor?.Tipo, pedido.Fecha), ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return r.Error;
        }

        var puestos = pedido.PonerConceptos(r.Valor);
        return puestos.EsFallo ? puestos.Error : null;
    }

    public Task<Resultado<PedidoDto>> EjecutarAsync(Guid empresaId, CrearPedidoComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, false, ct);

    /// <summary>Calcula el pedido tal como quedaría (conceptos, importes y coste de entrada) sin numerarlo ni guardarlo.</summary>
    public Task<Resultado<PedidoDto>> SimularAsync(Guid empresaId, CrearPedidoComando comando, CancellationToken ct = default) =>
        EjecutarInternoAsync(empresaId, comando, true, ct);

    private async Task<Resultado<PedidoDto>> EjecutarInternoAsync(Guid empresaId, CrearPedidoComando comando, bool simular, CancellationToken ct)
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

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = simular ? 0 : await _pedidos.SiguienteNumeroAsync(empresaId, fecha.Year, comando.ProveedorId, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.PedidoCompra, comando.ProveedorId, ct).ConfigureAwait(false);
        var lineas = (comando.Lineas ?? Array.Empty<LineaPedidoComando>())
            .Select(l => (l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario)).ToList();
        var pedido = PedidoCompra.Crear(empresaId, comando.ProveedorId, proveedorTexto, fecha, numero, comando.SolicitudOrigenId, lineas, _reloj, serie);
        if (pedido.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(pedido.Error);
        }

        if (await PonerConceptosAsync(pedido.Valor, comando, ct).ConfigureAwait(false) is { } errorConceptos)
        {
            return Resultado.Fallo<PedidoDto>(errorConceptos);
        }

        if (simular)
        {
            return Resultado.Ok(PedidoDto.Desde(pedido.Valor));
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

    /// <summary>Modifica fecha y líneas de un pedido sin recepciones ni factura (el proveedor no cambia).</summary>
    public async Task<Resultado<PedidoDto>> ModificarAsync(Guid pedidoId, CrearPedidoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<PedidoDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        if (pedido.EsTraspasoIntragrupo)
        {
            return Resultado.Fallo<PedidoDto>(ErroresTraspaso.PedidoIntragrupo);
        }

        if (comando.ProveedorId is { } p && p != pedido.ProveedorId)
        {
            return Resultado.Fallo<PedidoDto>(Error.Validacion("pedido.cambio_proveedor",
                "El proveedor de un pedido no se cambia (su número es de la serie del proveedor): cancélalo y crea otro."));
        }

        var lineas = (comando.Lineas ?? Array.Empty<LineaPedidoComando>())
            .Select(l => (l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario)).ToList();
        var r = pedido.Modificar(comando.Fecha ?? pedido.Fecha, lineas);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(r.Error);
        }

        if (await PonerConceptosAsync(pedido, comando, ct).ConfigureAwait(false) is { } errorConceptos)
        {
            return Resultado.Fallo<PedidoDto>(errorConceptos);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoDto.Desde(pedido));
    }
}

public sealed class DecidirPedido
{
    private readonly IRepositorioPedidos _repo;
    private readonly IUnidadDeTrabajoCompras _unidad;

    public DecidirPedido(IRepositorioPedidos repo, IUnidadDeTrabajoCompras unidad) { _repo = repo; _unidad = unidad; }

    public Task<Resultado<PedidoDto>> ConfirmarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Confirmar(), ct);
    public Task<Resultado<PedidoDto>> CancelarAsync(Guid id, CancellationToken ct = default) =>
        CambiarAsync(id, p => p.EsTraspasoIntragrupo ? Resultado.Fallo(ErroresTraspaso.PedidoIntragrupo) : p.Cancelar(), ct);

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
    private readonly IResolverSerie _resolverSerie;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;
    private readonly IEntradaInventarioCompras? _inventario;

    public RecibirMercancia(IRepositorioPedidos pedidos, IRepositorioAlbaranes albaranes, IResolverSerie resolverSerie, IUnidadDeTrabajoCompras unidad,
        IReloj reloj, IEntradaInventarioCompras? inventario = null)
    {
        _pedidos = pedidos; _albaranes = albaranes; _resolverSerie = resolverSerie; _unidad = unidad; _reloj = reloj; _inventario = inventario;
    }

    public Task<Resultado<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, RecibirMercanciaComando comando, CancellationToken ct = default) =>
        EjecutarAsync(empresaId, pedidoId, comando, null, ct);

    /// <param name="albaranVentaOrigenId">Albarán de venta de otra empresa del grupo que origina esta recepción (traspaso).</param>
    internal async Task<Resultado<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid pedidoId, RecibirMercanciaComando comando, Guid? albaranVentaOrigenId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<AlbaranDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        if (pedido.EsTraspasoIntragrupo && albaranVentaOrigenId is null)
        {
            return Resultado.Fallo<AlbaranDto>(ErroresTraspaso.PedidoIntragrupo);
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

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _albaranes.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var serie = await _resolverSerie.ResolverPrefijoAsync(empresaId, TipoDocumento.AlbaranCompra, pedido.ProveedorId, ct).ConfigureAwait(false);

        var lineasAlbaran = recepciones.Select(r =>
        {
            var lp = pedido.Lineas.Single(l => l.Id == r.LineaPedidoId);
            return (r.LineaPedidoId, lp.ProductoId, lp.Descripcion, r.Cantidad);
        }).ToList();

        var albaran = AlbaranCompra.Crear(empresaId, pedidoId, numero, fecha, comando.Referencia, lineasAlbaran, _reloj, serie,
            _inventario is not null ? comando.AlmacenId : null, albaranVentaOrigenId);
        if (albaran.EsFallo)
        {
            return Resultado.Fallo<AlbaranDto>(albaran.Error);
        }

        _albaranes.Agregar(albaran.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);

        // Entrada automática en inventario: solo si se indicó almacén y la línea tiene artículo del catálogo.
        if (comando.AlmacenId is { } almacenId && _inventario is not null)
        {
            var referencia = $"Albarán {numero}";
            foreach (var r in recepciones)
            {
                var lp = pedido.Lineas.Single(l => l.Id == r.LineaPedidoId);
                if (lp.ProductoId is not { } productoId)
                {
                    continue;
                }

                var lote = comando.Lineas!.First(x => x.LineaPedidoId == r.LineaPedidoId).Lote;
                await _inventario.RegistrarEntradaAsync(empresaId, productoId, almacenId, pedido.ProveedorId,
                    r.Cantidad, referencia, fecha, lote, lp.CosteUnitarioEntrada, ct).ConfigureAwait(false);
            }
        }

        return Resultado.Ok(AlbaranDto.Desde(albaran.Valor));
    }
}

/// <summary>
/// Caso de uso: anular un albarán de compra (la recepción no se hizo o se registró mal). Lo recibido vuelve a quedar
/// pendiente en el pedido y, si entró en un almacén, sale de él. Si las existencias ya se han usado, no se anula.
/// </summary>
public sealed class AnularAlbaranCompra
{
    private readonly IRepositorioPedidos _pedidos;
    private readonly IRepositorioAlbaranes _albaranes;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;
    private readonly IEntradaInventarioCompras? _inventario;

    public AnularAlbaranCompra(IRepositorioPedidos pedidos, IRepositorioAlbaranes albaranes, IUnidadDeTrabajoCompras unidad, IReloj reloj, IEntradaInventarioCompras? inventario = null)
    {
        _pedidos = pedidos; _albaranes = albaranes; _unidad = unidad; _reloj = reloj; _inventario = inventario;
    }

    public Task<Resultado<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid? pedidoId, Guid albaranId, string? motivo, CancellationToken ct = default) =>
        EjecutarAsync(empresaId, pedidoId, albaranId, motivo, desdeTraspaso: false, ct);

    /// <param name="desdeTraspaso">La anulación viene del albarán de venta de origen (traspaso intragrupo).</param>
    public async Task<Resultado<AlbaranDto>> EjecutarAsync(Guid empresaId, Guid? pedidoId, Guid albaranId, string? motivo, bool desdeTraspaso, CancellationToken ct = default)
    {
        var albaran = await _albaranes.ObtenerPorIdAsync(albaranId, ct).ConfigureAwait(false);
        if (albaran is null || (pedidoId is { } p && albaran.PedidoId != p))
        {
            return Resultado.Fallo<AlbaranDto>(Error.NoEncontrado("albaran.no_encontrado", "El albarán no existe en ese pedido."));
        }

        var pedido = await _pedidos.ObtenerPorIdAsync(albaran.PedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<AlbaranDto>(Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido."));
        }

        if (pedido.EsTraspasoIntragrupo && !desdeTraspaso)
        {
            return Resultado.Fallo<AlbaranDto>(Error.Conflicto("albaran.intragrupo",
                "Este albarán es un traspaso de otra empresa del grupo: se anula anulando su albarán de venta en la empresa de origen."));
        }

        var anulado = albaran.Anular(motivo, _reloj);
        if (anulado.EsFallo)
        {
            return Resultado.Fallo<AlbaranDto>(anulado.Error);
        }

        var deshecho = pedido.DeshacerRecepcion(albaran.Lineas.Select(l => (l.LineaPedidoId, l.Cantidad)).ToList());
        if (deshecho.EsFallo)
        {
            return Resultado.Fallo<AlbaranDto>(deshecho.Error);
        }

        // Salida del almacén de lo que entró; si una falla (ya no hay existencias), se deshacen las anteriores.
        if (albaran.AlmacenId is { } almacen && _inventario is not null)
        {
            var fecha = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
            var referencia = $"Anulación del albarán {albaran.NumeroCompleto}";
            var hechas = new List<(Guid Producto, decimal Cantidad)>();
            foreach (var l in albaran.Lineas.Where(l => l.ProductoId is not null))
            {
                var r = await _inventario.RegistrarSalidaAsync(empresaId, l.ProductoId!.Value, almacen, pedido.ProveedorId, l.Cantidad, referencia, fecha, ct).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    foreach (var (producto, cantidad) in hechas)
                    {
                        await _inventario.RegistrarEntradaAsync(empresaId, producto, almacen, pedido.ProveedorId, cantidad, referencia, fecha, null,
                            pedido.Lineas.First(x => x.ProductoId == producto).CosteUnitarioEntrada, CancellationToken.None).ConfigureAwait(false);
                    }

                    return Resultado.Fallo<AlbaranDto>(Error.Conflicto("albaran.existencias_usadas",
                        $"No se puede anular: ya no están en el almacén las existencias de «{l.Descripcion}» ({r.Error.Mensaje})."));
                }

                hechas.Add((l.ProductoId.Value, l.Cantidad));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AlbaranDto.Desde(albaran));
    }
}

internal static class ErroresTraspaso
{
    public static readonly Error PedidoIntragrupo = Error.Conflicto("pedido.intragrupo",
        "Es un traspaso de otra empresa del grupo: se recibe, se anula y se factura desde la empresa de origen.");
}

/// <summary>Línea del pedido de venta de origen de un traspaso entre empresas del grupo (precio neto de descuento).</summary>
public sealed record LineaTraspaso(Guid LineaVentaId, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario);

/// <summary>Lo entregado en un albarán de venta, por línea del pedido de venta.</summary>
public sealed record EntregaTraspaso(Guid LineaVentaId, decimal Cantidad);

/// <summary>Mercancía que otra empresa del grupo ha entregado a esta (su albarán de venta, contra su pedido de venta).</summary>
public sealed record DatosTraspaso(Guid EmpresaOrigenId, string EmpresaOrigenNombre, Guid PedidoVentaId, Guid AlbaranVentaId, string NumeroAlbaranVenta,
    Guid? ProveedorId, DateOnly Fecha, Guid? AlmacenId, IReadOnlyList<LineaTraspaso> LineasPedido, IReadOnlyList<EntregaTraspaso> Entregas);

/// <summary>
/// Traspaso de existencias entre empresas del grupo. El pedido de venta de una empresa a otra del grupo tiene en la
/// receptora su pedido de compra espejo (confirmado, mismas líneas y precio neto), que nace con la primera entrega;
/// cada albarán de venta es un albarán de recepción de ese pedido, con la entrada en su almacén. Las entregas parciales
/// se acumulan en el mismo pedido. El pedido no se factura desde compras: la factura llega a la bandeja de facturas
/// recibidas. Es idempotente por albarán de origen.
/// </summary>
public sealed class TraspasoIntragrupoCompras
{
    private readonly CrearPedido _crear;
    private readonly RecibirMercancia _recibir;
    private readonly AnularAlbaranCompra _anular;
    private readonly IRepositorioPedidos _pedidos;
    private readonly IRepositorioAlbaranes _albaranes;
    private readonly IUnidadDeTrabajoCompras _unidad;

    public TraspasoIntragrupoCompras(CrearPedido crear, RecibirMercancia recibir, AnularAlbaranCompra anular, IRepositorioPedidos pedidos,
        IRepositorioAlbaranes albaranes, IUnidadDeTrabajoCompras unidad)
    {
        _crear = crear; _recibir = recibir; _anular = anular; _pedidos = pedidos; _albaranes = albaranes; _unidad = unidad;
    }

    public async Task<Resultado<PedidoDto>> RecibirAsync(Guid empresaId, DatosTraspaso datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _albaranes.PorAlbaranVentaOrigenAsync(empresaId, datos.AlbaranVentaId, ct).ConfigureAwait(false) is { } hecho)
        {
            return Resultado.Ok(PedidoDto.Desde((await _pedidos.ObtenerPorIdAsync(hecho.PedidoId, ct).ConfigureAwait(false))!));
        }

        var pedido = await _pedidos.PorPedidoVentaOrigenAsync(empresaId, datos.PedidoVentaId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            var creado = await _crear.EjecutarAsync(empresaId, new CrearPedidoComando(datos.EmpresaOrigenNombre,
                datos.LineasPedido.Select(l => new LineaPedidoComando(l.Descripcion, l.Cantidad, l.PrecioUnitario, l.ProductoId)).ToList(), datos.ProveedorId, datos.Fecha),
                ct).ConfigureAwait(false);
            if (creado.EsFallo)
            {
                return creado;
            }

            pedido = (await _pedidos.ObtenerPorIdAsync(creado.Valor.Id, ct).ConfigureAwait(false))!;
            var marcado = pedido.MarcarTraspasoIntragrupo(datos.EmpresaOrigenId, datos.PedidoVentaId, datos.LineasPedido.Select(l => l.LineaVentaId).ToList());
            var confirmado = marcado.EsCorrecto ? pedido.Confirmar() : marcado;
            if (confirmado.EsFallo)
            {
                return Resultado.Fallo<PedidoDto>(confirmado.Error);
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        var lineas = new List<RecepcionLineaComando>();
        foreach (var e in datos.Entregas)
        {
            var linea = pedido.Lineas.FirstOrDefault(l => l.LineaVentaOrigenId == e.LineaVentaId);
            if (linea is null)
            {
                return Resultado.Fallo<PedidoDto>(Error.Conflicto("traspaso.linea_sin_espejo",
                    $"El pedido de compra {pedido.NumeroCompleto} no tiene la línea del pedido de venta que se entrega."));
            }

            lineas.Add(new RecepcionLineaComando(linea.Id, e.Cantidad));
        }

        var recibido = await _recibir.EjecutarAsync(empresaId, pedido.Id, new RecibirMercanciaComando(lineas, datos.Fecha,
            $"Albarán {datos.NumeroAlbaranVenta} de {datos.EmpresaOrigenNombre}", datos.AlmacenId), datos.AlbaranVentaId, ct).ConfigureAwait(false);
        return recibido.EsFallo ? Resultado.Fallo<PedidoDto>(recibido.Error) : Resultado.Ok(PedidoDto.Desde(pedido));
    }

    /// <summary>
    /// El albarán de venta de origen se anuló: se anula su albarán de recepción (sale del almacén y vuelve a quedar
    /// pendiente). Si el pedido se queda sin recepciones vivas, se cancela; la siguiente entrega abre otro.
    /// </summary>
    /// <summary>Lo que tiene que salir del almacén de la receptora si se anula el traspaso del albarán de venta (vacío si no entró nada).</summary>
    public async Task<IReadOnlyList<(Guid AlmacenId, Guid ProductoId, string Descripcion, decimal Cantidad)>> SalidasAlAnularAsync(Guid empresaId,
        Guid albaranVentaId, CancellationToken ct = default)
    {
        var albaran = await _albaranes.PorAlbaranVentaOrigenAsync(empresaId, albaranVentaId, ct).ConfigureAwait(false);
        if (albaran is null || albaran.AnuladoEn is not null || albaran.AlmacenId is not { } almacen)
        {
            return [];
        }

        return albaran.Lineas.Where(l => l.ProductoId is not null).GroupBy(l => l.ProductoId!.Value)
            .Select(g => (almacen, g.Key, g.First().Descripcion, g.Sum(l => l.Cantidad))).ToList();
    }

    public async Task<Resultado<PedidoDto?>> AnularAsync(Guid empresaId, Guid albaranVentaId, string motivo, CancellationToken ct = default)
    {
        var albaran = await _albaranes.PorAlbaranVentaOrigenAsync(empresaId, albaranVentaId, ct).ConfigureAwait(false);
        if (albaran is null)
        {
            return Resultado.Ok<PedidoDto?>(null);
        }

        var pedido = (await _pedidos.ObtenerPorIdAsync(albaran.PedidoId, ct).ConfigureAwait(false))!;
        if (albaran.AnuladoEn is null)
        {
            var r = await _anular.EjecutarAsync(empresaId, pedido.Id, albaran.Id, motivo, desdeTraspaso: true, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<PedidoDto?>(r.Error);
            }
        }

        if (pedido.Estado is not EstadoPedido.Cancelado && pedido.Lineas.All(l => l.CantidadRecibida == 0m))
        {
            var cancelado = pedido.Cancelar();
            if (cancelado.EsFallo)
            {
                return Resultado.Fallo<PedidoDto?>(cancelado.Error);
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return Resultado.Ok<PedidoDto?>(PedidoDto.Desde(pedido));
    }
}

/// <summary>
/// Almacén de entrada de los traspasos desde otras empresas del grupo: uno por empresa de origen, uno general para las
/// demás, o ninguno (entra en el almacén activo de código más bajo).
/// </summary>
public sealed class AlmacenesTraspaso
{
    private readonly IRepositorioAlmacenesTraspaso _repo;
    private readonly IUnidadDeTrabajoCompras _unidad;

    public AlmacenesTraspaso(IRepositorioAlmacenesTraspaso repo, IUnidadDeTrabajoCompras unidad) { _repo = repo; _unidad = unidad; }

    public Task<IReadOnlyList<AlmacenTraspaso>> ListarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);

    /// <summary>
    /// Almacén configurado para una empresa de origen (o el general). <c>Configurado</c> falso si no hay nada
    /// configurado; con <c>Configurado</c> y almacén nulo, el traspaso no da entrada en inventario.
    /// </summary>
    public async Task<(bool Configurado, Guid? AlmacenId)> ResolverAsync(Guid empresaId, Guid empresaOrigenId, CancellationToken ct = default)
    {
        var lista = await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var fila = lista.FirstOrDefault(a => a.EmpresaOrigenId == empresaOrigenId) ?? lista.FirstOrDefault(a => a.EmpresaOrigenId is null);
        return fila is null ? (false, null) : (true, fila.AlmacenId);
    }

    /// <summary>Fija el almacén (nulo: sin entrada en inventario) para una empresa de origen o, sin ella, para todas las demás.</summary>
    public async Task<AlmacenTraspaso> FijarAsync(Guid empresaId, Guid? empresaOrigenId, Guid? almacenId, CancellationToken ct = default)
    {
        var fila = (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(a => a.EmpresaOrigenId == empresaOrigenId);
        if (fila is null)
        {
            fila = AlmacenTraspaso.Crear(empresaId, empresaOrigenId, almacenId);
            _repo.Agregar(fila);
        }
        else
        {
            fila.CambiarAlmacen(almacenId);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return fila;
    }

    public async Task<Resultado> QuitarAsync(Guid id, CancellationToken ct = default)
    {
        var fila = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (fila is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("almacen_traspaso.no_encontrado", "No existe esa configuración."));
        }

        _repo.Eliminar(fila);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
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
    private readonly GestionDevolucionesCompra? _devoluciones;

    public FacturarPedido(IRepositorioPedidos pedidos, IContabilizador contabilizador, IUnidadDeTrabajoCompras unidad, IReloj reloj,
        GestionDevolucionesCompra? devoluciones = null)
    {
        _pedidos = pedidos; _contabilizador = contabilizador; _unidad = unidad; _reloj = reloj; _devoluciones = devoluciones;
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

        // Lo devuelto antes de facturar no se factura: la factura sale sin ello.
        var devuelto = _devoluciones is null ? 0m : await _devoluciones.DescontarEnFacturaAsync(pedido.Id, ct).ConfigureAwait(false);
        var importe = Redondeo.Dos(facturado.Valor - devuelto);
        var concepto = $"Compra a {pedido.ProveedorTexto}";
        // Los conceptos con cuenta propia (portes pagados a la 624, por ejemplo) van en su propia línea de la factura
        // recibida, a su cuenta; el resto de la línea, a la cuenta de compras.
        static bool Propio(ConceptoAplicado c) => c.Efecto == EfectoConcepto.Precio && !string.IsNullOrWhiteSpace(c.CuentaContable);
        IReadOnlyList<(decimal Base, string? Cuenta, string? Descripcion)>? lineas = null;
        if (pedido.Lineas.Any(l => l.Conceptos.Any(Propio)))
        {
            var propios = pedido.Lineas.SelectMany(l => l.Conceptos.Where(Propio)).GroupBy(c => (c.CuentaContable, c.Nombre))
                .Select(g => (Redondeo.Dos(g.Sum(c => c.Importe)), (string?)g.Key.CuentaContable, (string?)g.Key.Nombre)).ToList();
            var resto = Redondeo.Dos(importe - propios.Sum(p => p.Item1));
            lineas = [(resto, null, concepto), .. propios];
        }

        var datos = new DatosContabilizacion(pedido.ProveedorId, pedido.ProveedorTexto, concepto,
            DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), importe,
            string.IsNullOrWhiteSpace(comando.CodigoIva) ? "IVA21" : comando.CodigoIva!, comando.PorcentajeIrpf, comando.NumeroFactura, comando.FechaFactura, lineas);

        var contabilizado = await _contabilizador.ContabilizarAsync(empresaId, datos, ct).ConfigureAwait(false);
        if (contabilizado.EsFallo)
        {
            return Resultado.Fallo<PedidoDto>(contabilizado.Error);
        }

        pedido.AnotarFactura(contabilizado.Valor.GastoId, datos.CodigoIva, comando.PorcentajeIrpf, comando.NumeroFactura, comando.FechaFactura);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PedidoDto.Desde(pedido));
    }
}
