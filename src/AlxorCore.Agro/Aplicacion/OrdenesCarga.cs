using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioOrdenesCarga
{
    void Agregar(OrdenCarga orden);

    Task<OrdenCarga?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OrdenCarga>> ListarAsync(Guid empresaId, EstadoOrdenCarga? estado, CancellationToken ct = default);

    /// <summary>Órdenes sin finalizar ni anular (para no cargar dos veces un palé y contar lo previsto en otras órdenes).</summary>
    Task<IReadOnlyList<OrdenCarga>> AbiertasAsync(Guid empresaId, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Contratos
public sealed record DatosOrdenCarga(DateOnly FechaCarga, string? Muelle = null, Guid? TransportistaId = null, Guid? VehiculoId = null, string? Matricula = null,
    string? Conductor = null, decimal? TemperaturaConsigna = null, int? Filas = null, int? Columnas = null, bool CartaPorte = false, string? Observaciones = null,
    bool Propuesta = false);

public sealed record LineaOrdenCargaComando(Guid PedidoVentaId, Guid LineaPedidoId, int Pales, int? Fila = null, int? Columna = null);

public sealed record CargarPaleComando(string Pale, Guid? LineaId = null, int? Fila = null, int? Columna = null);

public sealed record LineaOrdenCargaDto(Guid Id, int Orden, Guid PedidoVentaId, string Pedido, string Cliente, Guid LineaPedidoId, Guid? ProductoId, string Descripcion,
    int PalesPrevistos, int PalesCargados, decimal KilosCargados, int? Fila, int? Columna);

public sealed record PaleCargadoDto(Guid PaleId, string Sscc, Guid LineaId, decimal Kilos, int? Fila, int? Columna, DateTimeOffset CargadoEn, Guid? AlbaranId);

public sealed record OrdenCargaDto(Guid Id, string Numero, DateOnly FechaCarga, string Estado, string? Muelle, Guid? TransportistaId, Guid? VehiculoId, string? Matricula,
    string? Conductor, decimal? TemperaturaConsigna, int? Filas, int? Columnas, bool CartaPorte, string? Observaciones, IReadOnlyList<LineaOrdenCargaDto> Lineas,
    IReadOnlyList<PaleCargadoDto> Cargados, IReadOnlyList<Guid> Albaranes, string? MotivoAnulacion);

/// <summary>Línea en firme pendiente de servir, para montar órdenes: lo pedido, lo servido, lo reservado y lo previsto en otras órdenes.</summary>
public sealed record LineaPendienteCargaDto(Guid PedidoVentaId, string Pedido, Guid ClienteId, string Cliente, Guid LineaPedidoId, Guid? ProductoId, string Descripcion,
    decimal Cantidad, decimal Servida, decimal Pendiente, int PalesReservados, decimal KilosReservados, int PalesEnOrdenes);

/// <summary>Resultado de finalizar: la orden y, si algún pedido no pudo expedirse, por qué (el resto ya salió).</summary>
public sealed record FinalizacionOrdenCargaDto(OrdenCargaDto Orden, IReadOnlyList<string> Errores);

/// <summary>
/// Órdenes de carga ligeras sobre la expedición de palés (Hispatec: órdenes de carga). Se montan con líneas de pedidos en
/// firme pendientes de servir; al cargar se lee el palé, que se valida contra la línea (su artículo) y su reserva —un
/// palé reservado a otro pedido no entra—; al finalizar, los palés cargados se expiden por pedido con la expedición de
/// siempre: un albarán por pedido (y carta de porte si se pide) y el movimiento de envases.
/// </summary>
public sealed class OrdenesCargaAgro
{
    private readonly IRepositorioOrdenesCarga _ordenes;
    private readonly PalesAgro _pales;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;
    private readonly IDocumentosExpedicion? _documentos;
    private readonly IRepositorioReservas? _reservas;

    public OrdenesCargaAgro(IRepositorioOrdenesCarga ordenes, PalesAgro pales, IUnidadDeTrabajoAgro unidad, IReloj reloj, IDocumentosExpedicion? documentos = null,
        IRepositorioReservas? reservas = null)
    {
        _ordenes = ordenes;
        _pales = pales;
        _unidad = unidad;
        _reloj = reloj;
        _documentos = documentos;
        _reservas = reservas;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<OrdenCargaDto>> ListarAsync(Guid empresaId, EstadoOrdenCarga? estado, CancellationToken ct = default)
    {
        var lista = new List<OrdenCargaDto>();
        foreach (var o in await _ordenes.ListarAsync(empresaId, estado, ct).ConfigureAwait(false))
        {
            lista.Add(await DtoAsync(o, ct).ConfigureAwait(false));
        }

        return lista;
    }

    public async Task<OrdenCargaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _ordenes.ObtenerAsync(id, ct).ConfigureAwait(false) is { } o ? await DtoAsync(o, ct).ConfigureAwait(false) : null;

    public async Task<Resultado<OrdenCargaDto>> CrearAsync(Guid empresaId, DatosOrdenCarga datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        await _unidad.BloquearAsync($"agro:ordencarga:{empresaId}:{datos.FechaCarga.Year}", ct).ConfigureAwait(false);
        var numero = await _ordenes.SiguienteNumeroAsync(empresaId, datos.FechaCarga.Year, ct).ConfigureAwait(false);
        var orden = OrdenCarga.Crear(empresaId, numero, datos.FechaCarga, datos.Propuesta, _reloj);
        var r = Aplicar(orden, datos);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OrdenCargaDto>(r.Error);
        }

        _ordenes.Agregar(orden);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(orden, ct).ConfigureAwait(false));
    }

    public Task<Resultado<OrdenCargaDto>> CambiarAsync(Guid id, DatosOrdenCarga datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        return ConAsync(id, o => Task.FromResult(Aplicar(o, datos)), ct);
    }

    public Task<Resultado<OrdenCargaDto>> LiberarAsync(Guid id, CancellationToken ct = default) => ConAsync(id, o => Task.FromResult(o.Liberar()), ct);

    public Task<Resultado<OrdenCargaDto>> AnularAsync(Guid id, string? motivo, CancellationToken ct = default) => ConAsync(id, o => Task.FromResult(o.Anular(motivo)), ct);

    public Task<Resultado<OrdenCargaDto>> QuitarLineaAsync(Guid id, Guid lineaId, CancellationToken ct = default) =>
        ConAsync(id, o => Task.FromResult(o.QuitarLinea(lineaId)), ct);

    public Task<Resultado<OrdenCargaDto>> DescargarAsync(Guid id, Guid paleId, CancellationToken ct = default) =>
        ConAsync(id, o => Task.FromResult(o.Descargar(paleId)), ct);

    public Task<Resultado<OrdenCargaDto>> AgregarLineaAsync(Guid id, LineaOrdenCargaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return ConAsync(id, async o =>
        {
            var pedido = _documentos is null ? null : await _documentos.PedidoParaReservasAsync(comando.PedidoVentaId, ct).ConfigureAwait(false);
            var linea = pedido?.Lineas.SingleOrDefault(l => l.Id == comando.LineaPedidoId);
            if (pedido is null || linea is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("ordencarga.linea_pedido", "No se encontró la línea del pedido de venta."));
            }

            if (!pedido.Abierto || linea.Servida >= linea.Cantidad)
            {
                return Resultado.Fallo(Error.Conflicto("ordencarga.linea_servida", "Solo se cargan líneas de pedidos confirmados con algo pendiente de servir."));
            }

            var r = o.AgregarLinea(pedido.Id, linea.Id, linea.ProductoId, linea.Descripcion, comando.Pales, comando.Fila, comando.Columna);
            return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
        }, ct);
    }

    /// <summary>Carga un palé (por su SSCC o su id): cerrado, no cargado en otra orden abierta y de la línea que toca.</summary>
    public Task<Resultado<OrdenCargaDto>> CargarAsync(Guid empresaId, Guid id, CargarPaleComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        return ConAsync(id, async o =>
        {
            var pale = await _pales.ObtenerAsync(empresaId, (comando.Pale ?? string.Empty).Trim(), ct).ConfigureAwait(false);
            if (pale is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("pale.no_encontrado", "No hay ningún palé con ese SSCC."));
            }

            if (pale.Estado != nameof(EstadoPale.Cerrado))
            {
                return Resultado.Fallo(Error.Conflicto("ordencarga.pale_no_cerrado", $"{pale.Sscc} no está cerrado (está {pale.Estado.ToLowerInvariant()})."));
            }

            var otra = (await _ordenes.AbiertasAsync(empresaId, ct).ConfigureAwait(false))
                .FirstOrDefault(x => x.Id != o.Id && x.Cargados.Any(c => c.PaleId == pale.Id && !c.EstaExpedido));
            if (otra is not null)
            {
                return Resultado.Fallo(Error.Conflicto("ordencarga.pale_en_otra", $"{pale.Sscc} ya está cargado en la orden {otra.NumeroCompleto}."));
            }

            // La línea: la de su reserva (que tiene que estar en la orden), la indicada o la única del artículo del palé con hueco.
            var activas = _reservas is null ? [] : await _reservas.ActivasDePalesAsync([pale.Id], ct).ConfigureAwait(false);
            var reserva = activas.Count > 0 ? activas[0] : null;
            var productos = pale.Contenido.Select(c => c.ProductoId).Distinct().ToList();
            LineaOrdenCarga? linea;
            if (reserva is not null)
            {
                linea = o.Lineas.FirstOrDefault(l => l.LineaPedidoId == reserva.LineaPedidoId);
                if (linea is null)
                {
                    return Resultado.Fallo(Error.Conflicto("ordencarga.pale_reservado",
                        $"{pale.Sscc} está reservado para una línea de pedido que no está en esta orden."));
                }

                if (comando.LineaId is { } pedida && pedida != linea.Id)
                {
                    return Resultado.Fallo(Error.Conflicto("ordencarga.pale_reservado", $"{pale.Sscc} está reservado para la línea {linea.Orden}."));
                }
            }
            else if (comando.LineaId is { } lineaId)
            {
                linea = o.Lineas.FirstOrDefault(l => l.Id == lineaId);
                if (linea is null)
                {
                    return Resultado.Fallo(Error.NoEncontrado("ordencarga.linea", "La línea no está en la orden."));
                }
            }
            else
            {
                var candidatas = o.Lineas.Where(l => l.ProductoId is { } p && productos.Contains(p) && o.Cargados.Count(c => c.LineaId == l.Id) < l.PalesPrevistos).ToList();
                if (candidatas.Count != 1)
                {
                    return Resultado.Fallo(Error.Validacion("ordencarga.elige_linea", candidatas.Count == 0
                        ? $"Ninguna línea con hueco es del artículo de {pale.Sscc}."
                        : $"{pale.Sscc} vale para varias líneas: indica cuál."));
                }

                linea = candidatas[0];
            }

            if (linea.ProductoId is { } producto && !productos.Contains(producto))
            {
                return Resultado.Fallo(Error.Conflicto("ordencarga.pale_otro_articulo", $"{pale.Sscc} no lleva el artículo de la línea {linea.Orden}."));
            }

            var r = o.Cargar(linea.Id, pale.Id, pale.Sscc, pale.Kilos, comando.Fila, comando.Columna, _reloj);
            return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
        }, ct);
    }

    /// <summary>
    /// Finaliza la orden: expide los palés cargados de cada pedido (albarán, carta de porte si se pidió y envases). Si un
    /// pedido falla, los demás ya salieron y la orden sigue en carga con lo pendiente; se puede volver a finalizar.
    /// </summary>
    public async Task<Resultado<FinalizacionOrdenCargaDto>> FinalizarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var o = await _ordenes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return Resultado.Fallo<FinalizacionOrdenCargaDto>(NoEncontrada());
        }

        var puede = o.PuedeFinalizarse();
        if (puede.EsFallo)
        {
            return Resultado.Fallo<FinalizacionOrdenCargaDto>(puede.Error);
        }

        var errores = new List<string>();
        var lineas = o.Lineas.ToDictionary(l => l.Id);
        foreach (var grupo in o.Cargados.Where(c => !c.EstaExpedido).GroupBy(c => lineas[c.LineaId].PedidoVentaId).ToList())
        {
            var r = await _pales.ExpedirAsync(empresaId, new DatosExpedicion(grupo.Select(c => c.PaleId).ToList(), Fecha: Hoy, Referencia: o.NumeroCompleto,
                CartaPorte: o.CartaPorte, Matricula: o.Matricula, Observaciones: o.Observaciones, PedidoVentaId: grupo.Key, TransportistaId: o.TransportistaId,
                VehiculoId: o.VehiculoId, TemperaturaConsigna: o.TemperaturaConsigna), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                errores.Add(r.Error.Mensaje);
                continue;
            }

            o.Expedidos(grupo.Key, r.Valor.Select(p => p.AlbaranId).FirstOrDefault(a => a is not null), _reloj);
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        if (errores.Count > 0 && o.Cargados.All(c => !c.EstaExpedido))
        {
            return Resultado.Fallo<FinalizacionOrdenCargaDto>(Error.Conflicto("ordencarga.no_expedida", string.Join(" ", errores)));
        }

        return Resultado.Ok(new FinalizacionOrdenCargaDto(await DtoAsync(o, ct).ConfigureAwait(false), errores));
    }

    /// <summary>Líneas en firme pendientes de servir, con lo reservado y lo ya previsto en órdenes abiertas.</summary>
    public async Task<IReadOnlyList<LineaPendienteCargaDto>> PendientesAsync(Guid empresaId, Guid? clienteId, CancellationToken ct = default)
    {
        if (_documentos is null)
        {
            return [];
        }

        var enOrdenes = (await _ordenes.AbiertasAsync(empresaId, ct).ConfigureAwait(false)).SelectMany(o => o.Lineas)
            .GroupBy(l => l.LineaPedidoId).ToDictionary(g => g.Key, g => g.Sum(l => l.PalesPrevistos));
        var lista = new List<LineaPendienteCargaDto>();
        foreach (var p in await _documentos.PedidosPendientesAsync(empresaId, ct).ConfigureAwait(false))
        {
            if (clienteId is { } c && p.ClienteId != c)
            {
                continue;
            }

            var reservas = _reservas is null ? [] : (await _reservas.DePedidoAsync(p.Id, ct).ConfigureAwait(false)).Where(r => r.Estado == EstadoReservaPale.Activa).ToList();
            foreach (var l in p.Lineas.Where(l => l.Servida < l.Cantidad))
            {
                var suyas = reservas.Where(r => r.LineaPedidoId == l.Id).ToList();
                lista.Add(new LineaPendienteCargaDto(p.Id, p.Numero, p.ClienteId, p.Cliente, l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.Servida, l.Cantidad - l.Servida,
                    suyas.Count, suyas.Sum(r => r.Kilos), enOrdenes.GetValueOrDefault(l.Id)));
            }
        }

        return lista;
    }

    private static Resultado Aplicar(OrdenCarga o, DatosOrdenCarga d) => o.Datos(d.FechaCarga, d.Muelle, d.TransportistaId, d.VehiculoId, d.Matricula, d.Conductor,
        d.TemperaturaConsigna, d.Filas, d.Columnas, d.CartaPorte, d.Observaciones);

    private async Task<Resultado<OrdenCargaDto>> ConAsync(Guid id, Func<OrdenCarga, Task<Resultado>> accion, CancellationToken ct)
    {
        var o = await _ordenes.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return Resultado.Fallo<OrdenCargaDto>(NoEncontrada());
        }

        var r = await accion(o).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OrdenCargaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(o, ct).ConfigureAwait(false));
    }

    private async Task<OrdenCargaDto> DtoAsync(OrdenCarga o, CancellationToken ct)
    {
        var pedidos = new Dictionary<Guid, PedidoParaReservas?>();
        foreach (var pid in o.Lineas.Select(l => l.PedidoVentaId).Distinct())
        {
            pedidos[pid] = _documentos is null ? null : await _documentos.PedidoParaReservasAsync(pid, ct).ConfigureAwait(false);
        }

        var lineas = o.Lineas.Select(l =>
        {
            var cargados = o.Cargados.Where(c => c.LineaId == l.Id).ToList();
            var p = pedidos.GetValueOrDefault(l.PedidoVentaId);
            return new LineaOrdenCargaDto(l.Id, l.Orden, l.PedidoVentaId, p?.Numero ?? "?", p?.Cliente ?? "?", l.LineaPedidoId, l.ProductoId, l.Descripcion, l.PalesPrevistos,
                cargados.Count, cargados.Sum(c => c.Kilos), l.Fila, l.Columna);
        }).ToList();
        return new OrdenCargaDto(o.Id, o.NumeroCompleto, o.FechaCarga, o.Estado.ToString(), o.Muelle, o.TransportistaId, o.VehiculoId, o.Matricula, o.Conductor,
            o.TemperaturaConsigna, o.Filas, o.Columnas, o.CartaPorte, o.Observaciones, lineas,
            o.Cargados.Select(c => new PaleCargadoDto(c.PaleId, c.Sscc, c.LineaId, c.Kilos, c.Fila, c.Columna, c.CargadoEn, c.AlbaranId)).ToList(),
            o.Cargados.Where(c => c.AlbaranId is { } a && a != Guid.Empty).Select(c => c.AlbaranId!.Value).Distinct().ToList(), o.MotivoAnulacion);
    }

    private static Error NoEncontrada() => Error.NoEncontrado("ordencarga.no_encontrada", "La orden de carga no existe.");
}
