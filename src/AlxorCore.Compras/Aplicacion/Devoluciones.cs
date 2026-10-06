using AlxorCore.Compras.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Recepcion.Aplicacion;

namespace AlxorCore.Compras.Aplicacion;

public interface IRepositorioDevolucionesCompra
{
    void Agregar(DevolucionCompra devolucion);
    Task<DevolucionCompra?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DevolucionCompra>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<IReadOnlyList<DevolucionCompra>> DePedidoAsync(Guid pedidoId, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

public sealed record LineaDevolucionCompraComando(Guid LineaPedidoId, decimal Cantidad, string? Lote = null);

public sealed record CrearDevolucionCompraComando(Guid PedidoId, IReadOnlyList<LineaDevolucionCompraComando>? Lineas, DateOnly? Fecha = null, string? Motivo = null,
    Guid? AlmacenId = null);

/// <summary>Abono del proveedor: su número de factura rectificativa y, si cambian, el IVA y la retención (si no, los de la factura del pedido).</summary>
public sealed record AbonarDevolucionCompraComando(string? NumeroAbono, DateOnly? FechaAbono = null, string? CodigoIva = null, decimal? PorcentajeIrpf = null,
    string? NumeroRectificado = null, DateOnly? FechaRectificada = null);

public sealed record LineaDevolucionCompraDto(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal PrecioUnitario, decimal Base, string? Lote);

public sealed record DevolucionCompraDto(Guid Id, string Numero, Guid PedidoId, string? Pedido, Guid? ProveedorId, string Proveedor, DateOnly Fecha, string? Motivo,
    Guid? AlmacenId, string Estado, decimal Base, Guid? GastoAbonoId, IReadOnlyList<LineaDevolucionCompraDto> Lineas)
{
    public static DevolucionCompraDto De(DevolucionCompra d, string? pedido) =>
        new(d.Id, d.NumeroCompleto, d.PedidoId, pedido, d.ProveedorId, d.ProveedorTexto, d.Fecha, d.Motivo, d.AlmacenId, d.Estado.ToString(), d.Base, d.GastoAbonoId,
            d.Lineas.Select(l => new LineaDevolucionCompraDto(l.LineaPedidoId, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.Base, l.Lote)).ToList());
}

public sealed record DevolubleCompraDto(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Recibido, decimal Devuelto, decimal Devolvible, decimal PrecioUnitario);

/// <summary>
/// Devoluciones de mercancía al proveedor: sacan del almacén lo devuelto y quedan pendientes de su abono. Si el pedido
/// aún no está facturado, su factura saldrá sin lo devuelto; si ya lo está, el abono del proveedor se registra como
/// factura rectificativa recibida (gasto en negativo que rectifica el del pedido) y se compensa en tesorería.
/// </summary>
public sealed class GestionDevolucionesCompra
{
    private readonly IRepositorioDevolucionesCompra _devoluciones;
    private readonly IRepositorioPedidos _pedidos;
    private readonly IContabilizador _contabilizador;
    private readonly IUnidadDeTrabajoCompras _unidad;
    private readonly IReloj _reloj;
    private readonly IEntradaInventarioCompras? _inventario;

    public GestionDevolucionesCompra(IRepositorioDevolucionesCompra devoluciones, IRepositorioPedidos pedidos, IContabilizador contabilizador, IUnidadDeTrabajoCompras unidad,
        IReloj reloj, IEntradaInventarioCompras? inventario = null)
    {
        _devoluciones = devoluciones; _pedidos = pedidos; _contabilizador = contabilizador; _unidad = unidad; _reloj = reloj; _inventario = inventario;
    }

    public async Task<IReadOnlyList<DevolucionCompraDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _devoluciones.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var pedidos = (await _pedidos.ListarAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(p => p.Id, p => p.NumeroCompleto);
        return lista.OrderByDescending(d => d.Fecha).ThenByDescending(d => d.Numero).Select(d => DevolucionCompraDto.De(d, pedidos.GetValueOrDefault(d.PedidoId))).ToList();
    }

    public async Task<DevolucionCompraDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        return d is null ? null : DevolucionCompraDto.De(d, (await _pedidos.ObtenerPorIdAsync(d.PedidoId, ct).ConfigureAwait(false))?.NumeroCompleto);
    }

    /// <summary>Lo que aún se puede devolver de cada línea del pedido.</summary>
    public async Task<Resultado<IReadOnlyList<DevolubleCompraDto>>> DevolublesAsync(Guid pedidoId, CancellationToken ct = default)
    {
        var pedido = await _pedidos.ObtenerPorIdAsync(pedidoId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<IReadOnlyList<DevolubleCompraDto>>(PedidoNoEncontrado());
        }

        var devuelto = await DevueltoAsync(pedidoId, ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<DevolubleCompraDto>>(pedido.Lineas.Where(l => l.CantidadRecibida > 0m).Select(l => new DevolubleCompraDto(l.Id, l.ProductoId,
            l.Descripcion, l.CantidadRecibida, devuelto.GetValueOrDefault(l.Id), l.CantidadRecibida - devuelto.GetValueOrDefault(l.Id),
            l.Cantidad == 0m ? l.PrecioUnitario : Math.Round(l.Importe / l.Cantidad, 4, MidpointRounding.AwayFromZero))).ToList());
    }

    public async Task<Resultado<DevolucionCompraDto>> CrearAsync(Guid empresaId, CrearDevolucionCompraComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var pedido = await _pedidos.ObtenerPorIdAsync(c.PedidoId, ct).ConfigureAwait(false);
        if (pedido is null || pedido.EmpresaId != empresaId)
        {
            return Resultado.Fallo<DevolucionCompraDto>(PedidoNoEncontrado());
        }

        var fecha = c.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _devoluciones.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var almacen = _inventario is null ? null : c.AlmacenId;
        var r = DevolucionCompra.Crear(empresaId, pedido, numero, fecha, c.Motivo, almacen,
            (c.Lineas ?? []).Select(l => (l.LineaPedidoId, l.Cantidad, l.Lote)).ToList(), await DevueltoAsync(pedido.Id, ct).ConfigureAwait(false), _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DevolucionCompraDto>(r.Error);
        }

        var d = r.Valor;
        // Sale del almacén lo devuelto; si alguna salida falla (no quedan existencias), se deshacen las anteriores.
        if (almacen is { } almacenId)
        {
            var hechas = new List<LineaDevolucionCompra>();
            foreach (var l in d.Lineas.Where(l => l.ProductoId is not null))
            {
                var salida = await _inventario!.DevolverAsync(empresaId, l.ProductoId!.Value, almacenId, pedido.ProveedorId, l.Cantidad, l.Lote, d.NumeroCompleto, fecha, ct)
                    .ConfigureAwait(false);
                if (salida.EsFallo)
                {
                    await ReingresarAsync(empresaId, pedido, d, hechas, $"Deshacer {d.NumeroCompleto}", ct).ConfigureAwait(false);
                    return Resultado.Fallo<DevolucionCompraDto>(Error.Conflicto("devolucion_compra.sin_existencias",
                        $"No hay existencias de «{l.Descripcion}»{(l.Lote is null ? "" : $" ({l.Lote})")} en el almacén para devolverlas: {salida.Error.Mensaje}"));
                }

                hechas.Add(l);
            }
        }

        _devoluciones.Agregar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DevolucionCompraDto.De(d, pedido.NumeroCompleto));
    }

    /// <summary>Registra el abono del proveedor como factura rectificativa recibida (gasto en negativo que rectifica la del pedido).</summary>
    public async Task<Resultado<DevolucionCompraDto>> AbonarAsync(Guid empresaId, Guid id, AbonarDevolucionCompraComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DevolucionCompraDto>(NoEncontrada());
        }

        if (d.Estado != EstadoDevolucionCompra.Registrada)
        {
            return Resultado.Fallo<DevolucionCompraDto>(Error.Conflicto("devolucion_compra.no_pendiente", "La devolución ya no está pendiente de abono."));
        }

        var pedido = (await _pedidos.ObtenerPorIdAsync(d.PedidoId, ct).ConfigureAwait(false))!;
        if (pedido.Estado != EstadoPedido.Facturado)
        {
            return Resultado.Fallo<DevolucionCompraDto>(Error.Conflicto("devolucion_compra.pedido_sin_facturar",
                "El pedido aún no está facturado: lo devuelto se descontará solo de su factura."));
        }

        var motivo = $"Devolución {d.NumeroCompleto}{(d.Motivo is null ? "" : $": {d.Motivo}")}";
        var datos = new DatosContabilizacion(pedido.ProveedorId, pedido.ProveedorTexto, $"Abono de la devolución {d.NumeroCompleto} (pedido {pedido.NumeroCompleto})",
            c.FechaAbono ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), -d.Base,
            string.IsNullOrWhiteSpace(c.CodigoIva) ? pedido.CodigoIvaFactura ?? "IVA21" : c.CodigoIva!, c.PorcentajeIrpf ?? pedido.PorcentajeIrpfFactura,
            string.IsNullOrWhiteSpace(c.NumeroAbono) ? null : c.NumeroAbono.Trim(), c.FechaAbono,
            Rectificacion: new DatosRectificacionRecibida(pedido.GastoId, c.NumeroRectificado ?? pedido.NumeroFacturaProveedor, c.FechaRectificada ?? pedido.FechaFacturaProveedor,
                motivo));
        var gasto = await _contabilizador.ContabilizarAsync(empresaId, datos, ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<DevolucionCompraDto>(gasto.Error);
        }

        d.Abonar(gasto.Valor.GastoId);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DevolucionCompraDto.De(d, pedido.NumeroCompleto));
    }

    public async Task<Resultado<DevolucionCompraDto>> CerrarSinAbonoAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<DevolucionCompraDto>(NoEncontrada());
        }

        var r = d.CerrarSinAbono();
        if (r.EsFallo)
        {
            return Resultado.Fallo<DevolucionCompraDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(DevolucionCompraDto.De(d, null));
    }

    /// <summary>Anula una devolución pendiente (no se llegó a devolver): la mercancía vuelve a entrar en el almacén.</summary>
    public async Task<Resultado> AnularAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var d = await _devoluciones.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo(NoEncontrada());
        }

        var r = d.Anular();
        if (r.EsFallo)
        {
            return r;
        }

        var pedido = (await _pedidos.ObtenerPorIdAsync(d.PedidoId, ct).ConfigureAwait(false))!;
        await ReingresarAsync(empresaId, pedido, d, d.Lineas, $"Anulación {d.NumeroCompleto}", ct).ConfigureAwait(false);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Base de las devoluciones pendientes del pedido, que se descuentan al facturarlo (y quedan en la factura).</summary>
    internal async Task<decimal> DescontarEnFacturaAsync(Guid pedidoId, CancellationToken ct)
    {
        var base_ = 0m;
        foreach (var d in (await _devoluciones.DePedidoAsync(pedidoId, ct).ConfigureAwait(false)).Where(d => d.Estado == EstadoDevolucionCompra.Registrada))
        {
            base_ += d.Base;
            d.DescontarEnFactura();
        }

        return Redondeo.Dos(base_);
    }

    private async Task<Dictionary<Guid, decimal>> DevueltoAsync(Guid pedidoId, CancellationToken ct) =>
        (await _devoluciones.DePedidoAsync(pedidoId, ct).ConfigureAwait(false)).Where(d => d.Viva).SelectMany(d => d.Lineas)
            .GroupBy(l => l.LineaPedidoId).ToDictionary(g => g.Key, g => g.Sum(l => l.Cantidad));

    private async Task ReingresarAsync(Guid empresaId, PedidoCompra pedido, DevolucionCompra d, IEnumerable<LineaDevolucionCompra> lineas, string referencia,
        CancellationToken ct)
    {
        if (d.AlmacenId is not { } almacenId || _inventario is null)
        {
            return;
        }

        foreach (var l in lineas.Where(l => l.ProductoId is not null))
        {
            var lp = pedido.Lineas.FirstOrDefault(x => x.Id == l.LineaPedidoId);
            await _inventario.RegistrarEntradaAsync(empresaId, l.ProductoId!.Value, almacenId, pedido.ProveedorId, l.Cantidad, referencia, DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime),
                l.Lote, lp?.CosteUnitarioEntrada ?? l.PrecioUnitario, ct).ConfigureAwait(false);
        }
    }

    private static Error PedidoNoEncontrado() => Error.NoEncontrado("pedido.no_encontrado", "No se encontró el pedido.");

    private static Error NoEncontrada() => Error.NoEncontrado("devolucion_compra.no_encontrada", "La devolución no existe.");
}
