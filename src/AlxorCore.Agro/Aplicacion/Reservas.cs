using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

/// <summary>Reserva de palés para una línea del pedido. Con <c>Forzar</c> se admite reservar más de lo pendiente de servir.</summary>
public sealed record ReservarPalesComando(Guid PedidoVentaId, Guid LineaPedidoId, IReadOnlyList<Guid> PaleIds, bool Forzar = false);

public sealed record ReservaPaleDto(Guid Id, Guid PaleId, string Sscc, Guid PedidoVentaId, Guid LineaPedidoId, decimal Kilos, int Cajas, string Estado,
    DateTimeOffset CreadaEn, DateOnly? ConsumidaEl, string? MotivoAnulacion);

/// <summary>Una línea del pedido con lo pedido, servido y reservado (en la unidad del artículo: kilos o cajas).</summary>
public sealed record LineaReservasDto(Guid LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad, decimal Servida, decimal Reservado,
    decimal PendienteReservar, int PalesReservados);

public sealed record ReservasPedidoDto(Guid PedidoVentaId, string Numero, string Cliente, IReadOnlyList<LineaReservasDto> Lineas, IReadOnlyList<ReservaPaleDto> Reservas,
    IReadOnlyList<PaleDisponibleDto> Disponibles);

/// <summary>Palé cerrado y sin reserva activa de uno de los artículos del pedido.</summary>
public sealed record PaleDisponibleDto(Guid PaleId, string Sscc, Guid ProductoId, decimal Kilos, int Cajas);

/// <summary>
/// Reservas de palés a líneas de pedidos de venta, como en Hispatec: un palé cerrado del artículo de la línea queda
/// apartado para ese pedido (una sola reserva activa por palé) y no sale con otro; la expedición del pedido la consume y,
/// si se anula, vuelve a estar activa. No se reserva más de lo pendiente de servir salvo que se fuerce.
/// </summary>
public sealed class ReservasPales
{
    private readonly IRepositorioReservas _reservas;
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;
    private readonly IDocumentosExpedicion? _documentos;

    public ReservasPales(IRepositorioReservas reservas, IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProductos productos, IReloj reloj,
        IDocumentosExpedicion? documentos = null)
    {
        _reservas = reservas;
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
        _documentos = documentos;
    }

    public async Task<Resultado<ReservasPedidoDto>> DePedidoAsync(Guid empresaId, Guid pedidoVentaId, CancellationToken ct = default)
    {
        var pedido = _documentos is null ? null : await _documentos.PedidoParaReservasAsync(pedidoVentaId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<ReservasPedidoDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido de venta."));
        }

        var reservas = await _reservas.DePedidoAsync(pedidoVentaId, ct).ConfigureAwait(false);
        var dtos = await DtosAsync(reservas, ct).ConfigureAwait(false);
        var lineas = new List<LineaReservasDto>();
        foreach (var l in pedido.Lineas)
        {
            var porKilos = await PorKilosAsync(l.ProductoId, ct).ConfigureAwait(false);
            var activas = dtos.Where(r => r.LineaPedidoId == l.Id && r.Estado == nameof(EstadoReservaPale.Activa)).ToList();
            var reservado = activas.Sum(r => porKilos ? r.Kilos : r.Cajas);
            lineas.Add(new LineaReservasDto(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.Servida, reservado, Math.Max(0m, l.Cantidad - l.Servida - reservado), activas.Count));
        }

        // Palés cerrados, sin reserva activa, de los artículos del pedido.
        var productos = pedido.Lineas.Where(l => l.ProductoId is not null).Select(l => l.ProductoId!.Value).ToHashSet();
        var cerrados = await _repo.PalesAsync(empresaId, EstadoPale.Cerrado, ct).ConfigureAwait(false);
        var reservados = (await _reservas.ActivasDePalesAsync(cerrados.Select(p => p.Id).ToList(), ct).ConfigureAwait(false)).Select(r => r.PaleId).ToHashSet();
        var disponibles = new List<PaleDisponibleDto>();
        foreach (var p in cerrados.Where(p => !reservados.Contains(p.Id)))
        {
            var (producto, kilos, cajas) = await ContenidoAsync(p.Id, ct).ConfigureAwait(false);
            if (producto is { } pr && productos.Contains(pr))
            {
                disponibles.Add(new PaleDisponibleDto(p.Id, p.Sscc, pr, kilos, cajas));
            }
        }

        return Resultado.Ok(new ReservasPedidoDto(pedido.Id, pedido.Numero, pedido.Cliente, lineas, dtos, disponibles));
    }

    public async Task<Resultado<IReadOnlyList<ReservaPaleDto>>> ReservarAsync(Guid empresaId, ReservarPalesComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var pedido = _documentos is null ? null : await _documentos.PedidoParaReservasAsync(comando.PedidoVentaId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido de venta."));
        }

        if (!pedido.Abierto)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Conflicto("reserva.pedido_cerrado", "Solo se reservan palés para pedidos confirmados y sin facturar."));
        }

        var linea = pedido.Lineas.SingleOrDefault(l => l.Id == comando.LineaPedidoId);
        if (linea?.ProductoId is not { } producto)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Validacion("reserva.linea", "La línea no es del pedido o no lleva artículo."));
        }

        var ids = (comando.PaleIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Validacion("reserva.sin_pales", "Elige los palés que reservas."));
        }

        var pales = await _repo.PalesAsync(ids, ct).ConfigureAwait(false);
        if (pales.Count != ids.Count)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.NoEncontrado("pale.no_encontrado", "Algún palé no existe."));
        }

        var activas = (await _reservas.ActivasDePalesAsync(ids, ct).ConfigureAwait(false)).ToDictionary(r => r.PaleId);
        var porKilos = await PorKilosAsync(producto, ct).ConfigureAwait(false);
        var nuevas = new List<ReservaPale>();
        decimal cantidadNueva = 0m;
        foreach (var p in pales)
        {
            if (p.Estado != EstadoPale.Cerrado)
            {
                return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Conflicto("reserva.pale_no_cerrado", $"{p.Sscc}: solo se reservan palés cerrados y sin expedir."));
            }

            if (activas.TryGetValue(p.Id, out var otra))
            {
                return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Conflicto("reserva.pale_reservado",
                    otra.PedidoVentaId == pedido.Id ? $"{p.Sscc} ya está reservado para este pedido." : $"{p.Sscc} ya está reservado para otro pedido."));
            }

            var (productoPale, kilos, cajas) = await ContenidoAsync(p.Id, ct).ConfigureAwait(false);
            if (productoPale != producto)
            {
                return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Validacion("reserva.producto", $"{p.Sscc} no es del artículo de la línea."));
            }

            cantidadNueva += porKilos ? kilos : cajas;
            nuevas.Add(ReservaPale.Crear(empresaId, p.Id, pedido.Id, linea.Id, kilos, cajas, _reloj));
        }

        var yaReservado = 0m;
        foreach (var r in (await _reservas.DePedidoAsync(pedido.Id, ct).ConfigureAwait(false)).Where(r => r.LineaPedidoId == linea.Id && r.Estado == EstadoReservaPale.Activa))
        {
            yaReservado += porKilos ? r.Kilos : r.Cajas;
        }

        var pendiente = linea.Cantidad - linea.Servida - yaReservado;
        if (!comando.Forzar && cantidadNueva > pendiente)
        {
            return Resultado.Fallo<IReadOnlyList<ReservaPaleDto>>(Error.Conflicto("reserva.supera_pendiente",
                $"Los palés suman {cantidadNueva:0.###} {(porKilos ? "kg" : "cajas")} y la línea solo tiene {Math.Max(0m, pendiente):0.###} pendientes de reservar."));
        }

        foreach (var r in nuevas)
        {
            _reservas.Agregar(r);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<ReservaPaleDto>>(await DtosAsync(nuevas, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<ReservaPaleDto>> AnularAsync(Guid reservaId, string? motivo, CancellationToken ct = default)
    {
        var r = await _reservas.ReservaAsync(reservaId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<ReservaPaleDto>(Error.NoEncontrado("reserva.no_encontrada", "La reserva no existe."));
        }

        var anulada = r.Anular(motivo);
        if (anulada.EsFallo)
        {
            return Resultado.Fallo<ReservaPaleDto>(anulada.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync([r], ct).ConfigureAwait(false))[0]);
    }

    /// <summary>Reservas activas de la empresa (para marcar los palés reservados en la rejilla).</summary>
    public async Task<IReadOnlyList<ReservaPaleDto>> ActivasAsync(Guid empresaId, CancellationToken ct = default) =>
        await DtosAsync(await _reservas.ActivasAsync(empresaId, ct).ConfigureAwait(false), ct).ConfigureAwait(false);

    private async Task<bool> PorKilosAsync(Guid? productoId, CancellationToken ct) =>
        productoId is { } p && await _productos.ObtenerAsync(p, ct).ConfigureAwait(false) is { } producto
        && string.Equals(producto.Unidad, "kg", StringComparison.OrdinalIgnoreCase);

    /// <summary>Artículo (si el palé tiene uno solo), kilos y cajas del palé.</summary>
    private async Task<(Guid? Producto, decimal Kilos, int Cajas)> ContenidoAsync(Guid paleId, CancellationToken ct)
    {
        var contenido = (await _repo.ContenidoPaleAsync(paleId, ct).ConfigureAwait(false)).Where(c => c.Kilos > 0m).ToList();
        var partidas = await _repo.PartidasAsync(contenido.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false);
        var productos = partidas.Select(p => p.ProductoId).Distinct().ToList();
        return (productos.Count == 1 ? productos[0] : null, contenido.Sum(c => c.Kilos), contenido.Sum(c => Math.Max(c.Cajas, 0)));
    }

    private async Task<List<ReservaPaleDto>> DtosAsync(IEnumerable<ReservaPale> reservas, CancellationToken ct)

    {
        var lista = reservas.ToList();
        var pales = (await _repo.PalesAsync(lista.Select(r => r.PaleId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var resultado = new List<ReservaPaleDto>();
        foreach (var r in lista)
        {
            resultado.Add(new ReservaPaleDto(r.Id, r.PaleId, pales.GetValueOrDefault(r.PaleId)?.Sscc ?? "?", r.PedidoVentaId, r.LineaPedidoId, r.Kilos, r.Cajas, r.Estado.ToString(),
                r.CreadaEn, r.ConsumidaEl, r.MotivoAnulacion));
        }

        return resultado;
    }
}
