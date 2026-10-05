using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Logistica.Aplicacion;

public sealed record LineaUnidadDto(Guid ProductoId, string? Producto, string? Lote, DateOnly? FechaCaducidad, int Cajas, decimal Unidades, decimal PesoNetoKg, decimal PesoBrutoKg);

public sealed record UnidadLogisticaDto(Guid Id, string Sscc, string Tipo, string Estado, string Origen, Guid? SoporteId, Guid? PadreId, Guid AlmacenId, Guid? UbicacionId,
    Guid? ClienteId, Guid? PedidoVentaId, Guid? OrdenFabricacionId, Guid? PlantillaId, int Cajas, int? CajasCompleta, decimal PesoNetoKg, decimal PesoBrutoKg, int? AlturaMm,
    DateTimeOffset CreadaEn, DateTimeOffset? CerradaEn, DateOnly? FechaExpedicion, string? ReferenciaExpedicion, string? Observaciones, string? MotivoAnulacion,
    IReadOnlyList<LineaUnidadDto> Contenido, IReadOnlyList<string> Hijas);

public sealed record DatosCalculoPaletizado(Guid ProductoId, int? Cajas = null, decimal? Unidades = null, Guid? ClienteId = null, Guid? PlantillaId = null);

public sealed record CalculoPaletizadoDto(Guid ProductoId, string? Producto, string OrigenMosaico, Guid? PlantillaId, int CajasPorCapa, int Capas, string? Soporte,
    CalculoPaletizado Calculo);

public sealed record LineaCalculoPedidoDto(Guid LineaId, Guid ProductoId, string Descripcion, decimal Pendiente, CalculoPaletizadoDto? Calculo, string? Error);

public sealed record CalculoPedidoDto(Guid PedidoVentaId, string Numero, string Cliente, IReadOnlyList<LineaCalculoPedidoDto> Lineas, int Pales, int PalesCompletos, int Picos,
    decimal PesoBrutoKg);

public sealed record DatosUnidad(Guid AlmacenId, TipoUnidadLogistica Tipo = TipoUnidadLogistica.Pale, Guid? UbicacionId = null, Guid? SoporteId = null, Guid? PlantillaId = null,
    Guid? ProductoId = null, Guid? ClienteId = null, Guid? PedidoVentaId = null, string? Observaciones = null);

public sealed record DatosContenido(Guid ProductoId, string? Lote = null, int? Cajas = null, decimal? Unidades = null, DateOnly? FechaCaducidad = null);

/// <summary>Paletizar del almacén: cajas (o unidades) del artículo, del lote indicado o por caducidad (FEFO).</summary>
public sealed record DatosPaletizar(Guid ProductoId, Guid AlmacenId, int? Cajas = null, decimal? Unidades = null, string? Lote = null, Guid? PlantillaId = null,
    Guid? ClienteId = null, Guid? PedidoVentaId = null, Guid? UbicacionId = null, bool SoloCompletos = false, bool? MezclarLotes = null);

/// <summary>Paletizar lo fabricado en una orden: por defecto, todo lo que no esté ya paletizado.</summary>
public sealed record DatosPaletizarFabricacion(Guid OrdenFabricacionId, Guid AlmacenId, int? Cajas = null, Guid? PlantillaId = null, Guid? ClienteId = null,
    Guid? PedidoVentaId = null, Guid? UbicacionId = null);

public sealed record ResultadoPaletizadoDto(IReadOnlyList<UnidadLogisticaDto> Unidades, int Cajas, int Completas, int Abiertas, IReadOnlyList<string> Avisos);

public sealed record DatosMover(Guid AlmacenId, Guid? UbicacionId = null);

public sealed record DatosAsignar(Guid? ClienteId, Guid? PedidoVentaId);

public sealed record DatosMeter(string? PadreSscc, Guid? PadreId = null);

public sealed record DatosExpedir(DateOnly Fecha, string? Referencia = null);

/// <summary>Lo que se ha leído con el escáner: una unidad (SSCC) o un artículo (GTIN, con lote, caducidad y cantidad).</summary>
public sealed record LecturaLogisticaDto(string Tipo, UnidadLogisticaDto? Unidad, Guid? ProductoId, string? Producto, string? Gtin, bool EsCaja, string? Lote,
    DateOnly? FechaCaducidad, int? Cantidad, decimal? PesoNetoKg, IReadOnlyDictionary<string, string> Elementos, string? Aviso);

/// <summary>Datos de la etiqueta logística GS1 de una unidad.</summary>
public sealed record EtiquetaUnidadDto(string Sscc, string? Producto, string? Gtin, string? TipoSoporte, int Cajas, decimal PesoNetoKg, decimal PesoBrutoKg, string? Lote,
    DateOnly? FechaCaducidad, DateOnly Fecha, Guid? ClienteId, Guid? ProductoId = null);

/// <summary>
/// Paletización: el cálculo de los palés de una cantidad o de un pedido, el montaje de unidades logísticas desde el
/// almacén (por lote o por caducidad, FEFO) y desde lo fabricado, su ciclo (cerrar, abrir, anular, mover, asignar a un
/// pedido, meter cajas en un palé, expedir), la lectura de los códigos del escáner y los datos de la etiqueta.
/// Montar no mueve existencias: lo que está en unidades abiertas o cerradas del almacén no se puede volver a paletizar.
/// </summary>
public sealed class PaletizacionLogistica
{
    private readonly IRepositorioLogistica _repo;
    private readonly IUnidadDeTrabajoLogistica _unidad;
    private readonly MaestrosLogistica _maestros;
    private readonly IArticulosLogistica _articulos;
    private readonly IExistenciasLogistica _existencias;
    private readonly IReloj _reloj;
    private readonly IPedidosLogistica? _pedidos;
    private readonly IFabricacionLogistica? _fabricacion;

    public PaletizacionLogistica(IRepositorioLogistica repo, IUnidadDeTrabajoLogistica unidad, MaestrosLogistica maestros, IArticulosLogistica articulos,
        IExistenciasLogistica existencias, IReloj reloj, IPedidosLogistica? pedidos = null, IFabricacionLogistica? fabricacion = null)
    {
        _repo = repo;
        _unidad = unidad;
        _maestros = maestros;
        _articulos = articulos;
        _existencias = existencias;
        _reloj = reloj;
        _pedidos = pedidos;
        _fabricacion = fabricacion;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ cálculo
    public async Task<Resultado<CalculoPaletizadoDto>> CalcularAsync(Guid empresaId, DatosCalculoPaletizado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var ficha = await _repo.FichaAsync(empresaId, d.ProductoId, ct).ConfigureAwait(false);
        if (ficha is null)
        {
            return Resultado.Fallo<CalculoPaletizadoDto>(SinFicha());
        }

        var mosaico = await _maestros.MosaicoAsync(empresaId, ficha, d.ClienteId, d.PlantillaId, ct).ConfigureAwait(false);
        if (mosaico.EsFallo)
        {
            return Resultado.Fallo<CalculoPaletizadoDto>(mosaico.Error);
        }

        var cajas = d.Cajas ?? (d.Unidades is { } u ? CalculadoraPaletizado.Cajas(ficha, u) : mosaico.Valor.CajasPorPale);
        if (cajas < 0)
        {
            return Resultado.Fallo<CalculoPaletizadoDto>(Error.Validacion("paletizado.cantidad", "La cantidad no puede ser negativa."));
        }

        var nombre = (await _articulos.ObtenerAsync([d.ProductoId], ct).ConfigureAwait(false)).GetValueOrDefault(d.ProductoId)?.Nombre;
        return Resultado.Ok(new CalculoPaletizadoDto(d.ProductoId, nombre, mosaico.Valor.Origen, mosaico.Valor.PlantillaId, mosaico.Valor.CajasPorCapa, mosaico.Valor.Capas,
            mosaico.Valor.Soporte?.Nombre, CalculadoraPaletizado.Calcular(ficha, mosaico.Valor, cajas)));
    }

    /// <summary>Palés que necesita lo pendiente de servir de un pedido de venta, línea a línea, con las plantillas de su cliente.</summary>
    public async Task<Resultado<CalculoPedidoDto>> CalcularPedidoAsync(Guid empresaId, Guid pedidoVentaId, CancellationToken ct = default)
    {
        var pedido = _pedidos is null ? null : await _pedidos.ObtenerAsync(pedidoVentaId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<CalculoPedidoDto>(Error.NoEncontrado("pedidoventa.no_encontrado", "El pedido de venta no existe."));
        }

        var lineas = new List<LineaCalculoPedidoDto>();
        foreach (var l in pedido.Lineas.Where(l => l.Pendiente > 0m))
        {
            var c = await CalcularAsync(empresaId, new DatosCalculoPaletizado(l.ProductoId, Unidades: l.Pendiente, ClienteId: pedido.ClienteId), ct).ConfigureAwait(false);
            lineas.Add(new LineaCalculoPedidoDto(l.LineaId, l.ProductoId, l.Descripcion, l.Pendiente, c.EsCorrecto ? c.Valor : null, c.EsFallo ? c.Error.Mensaje : null));
        }

        var calculos = lineas.Where(l => l.Calculo is not null).Select(l => l.Calculo!.Calculo).ToList();
        return Resultado.Ok(new CalculoPedidoDto(pedido.Id, pedido.Numero, pedido.Cliente, lineas, calculos.Sum(c => c.Pales), calculos.Sum(c => c.PalesCompletos),
            calculos.Count(c => c.Pico is not null), Math.Round(calculos.Sum(c => c.PesoBrutoTotalKg), 3)));
    }

    // ------------------------------------------------------------------ montaje
    public async Task<Resultado<UnidadLogisticaDto>> CrearAsync(Guid empresaId, DatosUnidad d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (!Enum.IsDefined(d.Tipo))
        {
            return Resultado.Fallo<UnidadLogisticaDto>(Error.Validacion("unidad.tipo", "Tipo no válido: Pale, Caja o Contenedor."));
        }

        if (!await _existencias.AlmacenExisteAsync(empresaId, d.AlmacenId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<UnidadLogisticaDto>(Error.NoEncontrado("almacen.no_encontrado", "El almacén no existe."));
        }

        TipoSoporte? soporte = null;
        int? completa = null;
        if (d.ProductoId is { } productoId && await _repo.FichaAsync(empresaId, productoId, ct).ConfigureAwait(false) is { } ficha
            && await _maestros.MosaicoAsync(empresaId, ficha, d.ClienteId, d.PlantillaId, ct).ConfigureAwait(false) is { EsCorrecto: true } m)
        {
            soporte = m.Valor.Soporte;
            completa = d.Tipo == TipoUnidadLogistica.Pale ? m.Valor.CajasPorPale : null;
        }

        if (d.SoporteId is { } sid)
        {
            soporte = await _repo.SoporteAsync(sid, ct).ConfigureAwait(false);
            if (soporte is null)
            {
                return Resultado.Fallo<UnidadLogisticaDto>(Error.NoEncontrado("soporte.no_encontrado", "El soporte no existe."));
            }
        }

        var u = await NuevaAsync(empresaId, d.Tipo, OrigenUnidadLogistica.Manual, d.AlmacenId, d.UbicacionId, soporte, d.PlantillaId, completa, d.ClienteId, d.PedidoVentaId,
            null, d.Observaciones, ct).ConfigureAwait(false);
        if (u.EsFallo)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(u.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(u.Valor, ct).ConfigureAwait(false));
    }

    /// <summary>Pone (o quita, en negativo) contenido en una unidad abierta, comprobando lo que queda sin paletizar en su almacén.</summary>
    public async Task<Resultado<UnidadLogisticaDto>> PonerAsync(Guid unidadId, DatosContenido d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var u = await _repo.UnidadAsync(unidadId, ct).ConfigureAwait(false);
        if (u is null)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(NoExiste());
        }

        var ficha = await _repo.FichaAsync(u.EmpresaId, d.ProductoId, ct).ConfigureAwait(false);
        if (ficha is null)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(SinFicha());
        }

        var lote = string.IsNullOrWhiteSpace(d.Lote) ? null : d.Lote.Trim();
        if (ficha.GestionLotes && lote is null && (d.Cajas > 0 || d.Unidades > 0m))
        {
            return Resultado.Fallo<UnidadLogisticaDto>(Error.Validacion("unidad.lote", "El artículo se gestiona por lotes: indica el lote."));
        }

        var contenido = Contenido(ficha, d.Cajas, d.Unidades, lote, d.FechaCaducidad);
        if (contenido.EsFallo)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(contenido.Error);
        }

        if (contenido.Valor.Unidades > 0m)
        {
            var existencias = (await _existencias.DeProductoAsync(u.EmpresaId, d.ProductoId, u.AlmacenId, ct).ConfigureAwait(false)).Where(e => e.Lote == lote).ToList();
            var paletizado = (await _repo.PaletizadoAsync(u.EmpresaId, d.ProductoId, u.AlmacenId, null, ct).ConfigureAwait(false)).GetValueOrDefault(lote ?? string.Empty);
            var libre = existencias.Sum(e => e.Cantidad) - paletizado;
            if (contenido.Valor.Unidades > libre)
            {
                return Resultado.Fallo<UnidadLogisticaDto>(Error.Conflicto("unidad.sin_existencias",
                    $"En el almacén quedan {Math.Max(0m, libre):0.###} sin paletizar de ese artículo{(lote is null ? "" : $" y lote {lote}")}."));
            }

            contenido = Resultado.Ok(contenido.Valor with { FechaCaducidad = d.FechaCaducidad ?? existencias.FirstOrDefault()?.FechaCaducidad });
        }

        var altura = u.Contenido.All(l => l.ProductoId == d.ProductoId) ? await AlturaAsync(u, ficha, u.Cajas + contenido.Valor.Cajas, ct).ConfigureAwait(false) : null;
        var r = u.Poner(contenido.Valor, altura, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(r.Error);
        }

        await RecalcularPadreAsync(u, ct).ConfigureAwait(false);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(u, ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Monta palés con las cajas pedidas del artículo en el almacén. Sin lote, los toma por caducidad (FEFO; los que no
    /// tienen caducidad, al final). Cada palé completo se cierra; el último, si no se llena, queda abierto (o no se monta,
    /// con «solo completos»). Sin mezclar lotes, cada lote empieza palé. Falla si no hay existencias sin paletizar.
    /// </summary>
    public async Task<Resultado<ResultadoPaletizadoDto>> PaletizarAsync(Guid empresaId, DatosPaletizar d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var ficha = await _repo.FichaAsync(empresaId, d.ProductoId, ct).ConfigureAwait(false);
        if (ficha is null)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(SinFicha());
        }

        if (!await _existencias.AlmacenExisteAsync(empresaId, d.AlmacenId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.NoEncontrado("almacen.no_encontrado", "El almacén no existe."));
        }

        var mosaico = await _maestros.MosaicoAsync(empresaId, ficha, d.ClienteId, d.PlantillaId, ct).ConfigureAwait(false);
        if (mosaico.EsFallo)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(mosaico.Error);
        }

        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        var mezclar = d.MezclarLotes ?? config?.MezclarLotes ?? false;
        var paletizado = await _repo.PaletizadoAsync(empresaId, d.ProductoId, d.AlmacenId, null, ct).ConfigureAwait(false);
        var lotes = (await _existencias.DeProductoAsync(empresaId, d.ProductoId, d.AlmacenId, ct).ConfigureAwait(false))
            .GroupBy(e => e.Lote)
            .Select(g => (Lote: g.Key, Caducidad: g.Min(e => e.FechaCaducidad), Libre: g.Sum(e => e.Cantidad) - paletizado.GetValueOrDefault(g.Key ?? string.Empty)))
            .Where(x => x.Libre > 0m && (d.Lote is null || string.Equals(x.Lote, d.Lote.Trim(), StringComparison.Ordinal)))
            .OrderBy(x => x.Caducidad is null).ThenBy(x => x.Caducidad).ThenBy(x => x.Lote, StringComparer.Ordinal)
            .Select(x => (x.Lote, x.Caducidad, Cajas: (int)Math.Floor(x.Libre / ficha.UnidadesPorCaja))).Where(x => x.Cajas > 0).ToList();
        var disponibles = lotes.Sum(x => x.Cajas);
        var pedidas = d.Cajas ?? (d.Unidades is { } u ? CalculadoraPaletizado.Cajas(ficha, u) : disponibles);
        if (pedidas <= 0)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.Validacion("paletizado.cantidad", "No hay nada que paletizar: indica las cajas."));
        }

        if (pedidas > disponibles)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.Conflicto("paletizado.sin_existencias",
                $"Solo hay {disponibles} cajas sin paletizar{(d.Lote is null ? "" : $" del lote {d.Lote}")} en el almacén."));
        }

        var porPale = mosaico.Valor.CajasPorPale;
        if (d.SoloCompletos)
        {
            pedidas -= pedidas % porPale;
            if (pedidas == 0)
            {
                return Resultado.Fallo<ResultadoPaletizadoDto>(Error.Conflicto("paletizado.sin_completos", $"No llega a un palé completo ({porPale} cajas)."));
            }
        }

        var creadas = new List<UnidadLogistica>();
        UnidadLogistica? actual = null;
        var faltan = pedidas;
        foreach (var (lote, caducidad, cajasLote) in lotes)
        {
            if (faltan == 0)
            {
                break;
            }

            if (!mezclar)
            {
                actual = null;
            }

            var quedan = cajasLote;
            while (quedan > 0 && faltan > 0)
            {
                if (actual is null)
                {
                    var nueva = await NuevaAsync(empresaId, TipoUnidadLogistica.Pale, OrigenUnidadLogistica.Almacen, d.AlmacenId, d.UbicacionId, mosaico.Valor.Soporte,
                        mosaico.Valor.PlantillaId, porPale, d.ClienteId, d.PedidoVentaId, null, null, ct).ConfigureAwait(false);
                    if (nueva.EsFallo)
                    {
                        return Resultado.Fallo<ResultadoPaletizadoDto>(nueva.Error);
                    }

                    actual = nueva.Valor;
                    creadas.Add(actual);
                }

                var toma = Math.Min(Math.Min(porPale - actual.Cajas, quedan), faltan);
                var contenido = Contenido(ficha, toma, null, lote, caducidad).Valor;
                var r = actual.Poner(contenido, actual.Contenido.All(l => l.ProductoId == d.ProductoId)
                    ? CalculadoraPaletizado.Pale(ficha, mosaico.Valor, actual.Cajas + toma).AlturaMm : null, _reloj);
                if (r.EsFallo)
                {
                    return Resultado.Fallo<ResultadoPaletizadoDto>(r.Error);
                }

                quedan -= toma;
                faltan -= toma;
                if (actual.Estado == EstadoUnidadLogistica.Cerrada)
                {
                    actual = null;
                }
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var avisos = CalculadoraPaletizado.Calcular(ficha, mosaico.Valor, pedidas).Avisos.ToList();
        if (ficha.VidaMinimaEntregaDias is { } minimo && creadas.SelectMany(c => c.Contenido).Any(l => l.FechaCaducidad is { } cad && cad.DayNumber - Hoy.DayNumber < minimo))
        {
            avisos.Add($"Algún lote tiene menos de {minimo} días de vida: puede que el cliente no lo acepte.");
        }

        var dtos = new List<UnidadLogisticaDto>();
        foreach (var c in creadas)
        {
            dtos.Add(await DtoAsync(c, ct).ConfigureAwait(false));
        }

        return Resultado.Ok(new ResultadoPaletizadoDto(dtos, pedidas, creadas.Count(c => c.Estado == EstadoUnidadLogistica.Cerrada),
            creadas.Count(c => c.Estado == EstadoUnidadLogistica.Abierta), avisos));
    }

    /// <summary>
    /// Paletiza lo fabricado en una orden terminada, del lote con que se fabricó: por defecto todas las cajas que aún no
    /// estén en unidades de esa orden. Los palés quedan enlazados a la orden (trazabilidad hasta la fabricación).
    /// </summary>
    public async Task<Resultado<ResultadoPaletizadoDto>> PaletizarFabricacionAsync(Guid empresaId, DatosPaletizarFabricacion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var orden = _fabricacion is null ? null : await _fabricacion.ObtenerAsync(d.OrdenFabricacionId, ct).ConfigureAwait(false);
        if (orden is null)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.NoEncontrado("ordenfabricacion.no_encontrada", "La orden de fabricación no existe."));
        }

        if (!orden.Terminada)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.Conflicto("paletizado.orden_no_terminada",
                $"La orden {orden.Numero} no está terminada: lo fabricado entra en el almacén al terminarla."));
        }

        var ficha = await _repo.FichaAsync(empresaId, orden.ProductoId, ct).ConfigureAwait(false);
        if (ficha is null)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(SinFicha());
        }

        var yaCajas = (await _repo.UnidadesAsync(empresaId, new FiltroUnidades(OrdenFabricacionId: orden.Id), ct).ConfigureAwait(false))
            .Where(u => u.Estado != EstadoUnidadLogistica.Anulada).Sum(u => u.Contenido.Where(l => l.ProductoId == orden.ProductoId).Sum(l => l.Cajas));
        var fabricadas = (int)Math.Floor(orden.Cantidad / ficha.UnidadesPorCaja);
        var cajas = d.Cajas ?? fabricadas - yaCajas;
        if (cajas <= 0 || yaCajas + cajas > fabricadas)
        {
            return Resultado.Fallo<ResultadoPaletizadoDto>(Error.Conflicto("paletizado.fabricacion_completa",
                $"De la orden {orden.Numero} se fabricaron {fabricadas} cajas y ya hay {yaCajas} paletizadas."));
        }

        var r = await PaletizarAsync(empresaId, new DatosPaletizar(orden.ProductoId, d.AlmacenId, cajas, null, orden.Lote, d.PlantillaId, d.ClienteId, d.PedidoVentaId,
            d.UbicacionId, false, false), ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return r;
        }

        foreach (var dto in r.Valor.Unidades)
        {
            var u = (await _repo.UnidadAsync(dto.Id, ct).ConfigureAwait(false))!;
            u.MarcarFabricacion(orden.Id);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(r.Valor with { Unidades = r.Valor.Unidades.Select(x => x with { Origen = nameof(OrigenUnidadLogistica.Fabricacion), OrdenFabricacionId = orden.Id }).ToList() });
    }

    // ------------------------------------------------------------------ ciclo
    public Task<Resultado<UnidadLogisticaDto>> CerrarAsync(Guid id, CancellationToken ct = default) =>
        CambiarAsync(id, async u => u.Cerrar(_reloj, (await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false)).Any(h => h.Viva)), ct);

    public Task<Resultado<UnidadLogisticaDto>> AbrirAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, u => Task.FromResult(u.Abrir()), ct);

    /// <summary>Anula (desmonta) la unidad: su contenido queda libre en el almacén; las cajas que llevaba dentro, sueltas.</summary>
    public Task<Resultado<UnidadLogisticaDto>> AnularAsync(Guid id, string? motivo, CancellationToken ct = default) =>
        CambiarAsync(id, async u =>
        {
            var r = u.Anular(motivo);
            if (r.EsCorrecto)
            {
                foreach (var h in (await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false)).Where(h => h.Viva))
                {
                    h.MeterEn(null);
                }
            }

            return r;
        }, ct);

    public Task<Resultado<UnidadLogisticaDto>> MoverAsync(Guid id, DatosMover d, CancellationToken ct = default) =>
        CambiarAsync(id, async u =>
        {
            ArgumentNullException.ThrowIfNull(d);
            if (u.PadreId is not null)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.dentro", "La unidad va dentro de otra: mueve la de fuera."));
            }

            if (!await _existencias.AlmacenExisteAsync(u.EmpresaId, d.AlmacenId, ct).ConfigureAwait(false))
            {
                return Resultado.Fallo(Error.NoEncontrado("almacen.no_encontrado", "El almacén no existe."));
            }

            if (d.AlmacenId != u.AlmacenId)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.traspaso",
                    "Para llevarla a otro almacén, traspasa antes sus existencias en el inventario (el traspaso de palés entre almacenes llega con la expedición)."));
            }

            var r = u.Mover(d.AlmacenId, d.UbicacionId);
            foreach (var h in await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false))
            {
                h.Mover(d.AlmacenId, d.UbicacionId);
            }

            return r;
        }, ct);

    public Task<Resultado<UnidadLogisticaDto>> AsignarAsync(Guid id, DatosAsignar d, CancellationToken ct = default) =>
        CambiarAsync(id, u => Task.FromResult(u.AsignarPedido(d?.ClienteId, d?.PedidoVentaId)), ct);

    /// <summary>Mete la unidad (una caja con SSCC) en otra abierta (el palé), o la saca si no se indica.</summary>
    public Task<Resultado<UnidadLogisticaDto>> MeterAsync(Guid id, DatosMeter d, CancellationToken ct = default) =>
        CambiarAsync(id, async u =>
        {
            ArgumentNullException.ThrowIfNull(d);
            UnidadLogistica? padre = null;
            if (d.PadreId is { } pid)
            {
                padre = await _repo.UnidadAsync(pid, ct).ConfigureAwait(false);
            }
            else if (!string.IsNullOrWhiteSpace(d.PadreSscc))
            {
                padre = await _repo.UnidadPorSsccAsync(u.EmpresaId, Gs1.Leer(d.PadreSscc)?.Sscc ?? d.PadreSscc.Trim(), ct).ConfigureAwait(false);
            }

            if ((d.PadreId is not null || !string.IsNullOrWhiteSpace(d.PadreSscc)) && padre is null)
            {
                return Resultado.Fallo(NoExiste());
            }

            var anterior = u.PadreId is { } a ? await _repo.UnidadAsync(a, ct).ConfigureAwait(false) : null;
            var r = u.MeterEn(padre);
            if (r.EsCorrecto)
            {
                await RecalcularPadreAsync(u, ct, anterior).ConfigureAwait(false);
            }

            return r;
        }, ct);

    /// <summary>Marca la unidad (y lo que lleva dentro) como expedida. La salida de existencias la hace el albarán.</summary>
    public Task<Resultado<UnidadLogisticaDto>> ExpedirAsync(Guid id, DatosExpedir d, CancellationToken ct = default) =>
        CambiarAsync(id, async u =>
        {
            ArgumentNullException.ThrowIfNull(d);
            if (u.PadreId is not null)
            {
                return Resultado.Fallo(Error.Conflicto("unidad.dentro", "La unidad va dentro de otra: se expide la de fuera."));
            }

            var r = u.Expedir(d.Fecha, d.Referencia);
            if (r.EsCorrecto)
            {
                foreach (var h in (await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false)).Where(h => h.Viva))
                {
                    if (h.Estado == EstadoUnidadLogistica.Abierta)
                    {
                        h.Cerrar(_reloj);
                    }

                    h.Expedir(d.Fecha, d.Referencia);
                }
            }

            return r;
        }, ct);

    // ------------------------------------------------------------------ consultas
    public async Task<UnidadLogisticaDto?> ObtenerAsync(Guid empresaId, string idOSscc, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idOSscc);
        var u = Guid.TryParse(idOSscc, out var id)
            ? await _repo.UnidadAsync(id, ct).ConfigureAwait(false)
            : await _repo.UnidadPorSsccAsync(empresaId, Gs1.Leer(idOSscc)?.Sscc ?? idOSscc.Trim(), ct).ConfigureAwait(false);
        return u is null ? null : await DtoAsync(u, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UnidadLogisticaDto>> ListarAsync(Guid empresaId, FiltroUnidades filtro, CancellationToken ct = default)
    {
        var lista = await _repo.UnidadesAsync(empresaId, filtro, ct).ConfigureAwait(false);
        var articulos = await _articulos.ObtenerAsync(lista.SelectMany(u => u.Contenido).Select(l => l.ProductoId).Distinct().ToList(), ct).ConfigureAwait(false);
        return lista.Select(u => Dto(u, articulos, [])).ToList();
    }

    /// <summary>
    /// Interpreta lo leído con el escáner (pistola o cámara): un SSCC es una unidad logística; un GTIN (EAN-13 o ITF-14,
    /// solo o en un GS1-128 con lote, caducidad y cantidad) es un artículo con ficha logística.
    /// </summary>
    public async Task<LecturaLogisticaDto> LeerAsync(Guid empresaId, string? codigo, CancellationToken ct = default)
    {
        var lectura = Gs1.Leer(codigo);
        if (lectura is null)
        {
            return new LecturaLogisticaDto("Desconocido", null, null, null, null, false, null, null, null, null, new Dictionary<string, string>(),
                "No es un código GS1 que se entienda (SSCC, GTIN o GS1-128).");
        }

        if (lectura.Sscc is { } sscc)
        {
            var u = await _repo.UnidadPorSsccAsync(empresaId, sscc, ct).ConfigureAwait(false);
            return new LecturaLogisticaDto("Unidad", u is null ? null : await DtoAsync(u, ct).ConfigureAwait(false), null, null, lectura.Gtin, false, lectura.Lote,
                lectura.Caducidad, lectura.Cantidad, lectura.PesoNetoKg, lectura.Elementos, u is null ? $"El SSCC {sscc} no es de ninguna unidad de la empresa." : null);
        }

        if (lectura.Gtin is { } gtin)
        {
            var ficha = await _repo.FichaPorGtinAsync(empresaId, gtin, ct).ConfigureAwait(false);
            var nombre = ficha is null ? null : (await _articulos.ObtenerAsync([ficha.ProductoId], ct).ConfigureAwait(false)).GetValueOrDefault(ficha.ProductoId)?.Nombre;
            return new LecturaLogisticaDto("Articulo", null, ficha?.ProductoId, nombre, gtin, ficha?.GtinCaja == gtin, lectura.Lote, lectura.Caducidad, lectura.Cantidad,
                lectura.PesoNetoKg, lectura.Elementos, ficha is null ? $"El GTIN {gtin} no es de ningún artículo con ficha logística." : null);
        }

        return new LecturaLogisticaDto("Desconocido", null, null, null, null, false, lectura.Lote, lectura.Caducidad, lectura.Cantidad, lectura.PesoNetoKg, lectura.Elementos,
            "El código no lleva SSCC ni GTIN.");
    }

    /// <summary>Datos de la etiqueta GS1 de la unidad (el GTIN solo si lleva un único artículo, y el lote si es uno).</summary>
    public async Task<Resultado<EtiquetaUnidadDto>> EtiquetaAsync(Guid empresaId, string idOSscc, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idOSscc);
        var u = Guid.TryParse(idOSscc, out var id)
            ? await _repo.UnidadAsync(id, ct).ConfigureAwait(false)
            : await _repo.UnidadPorSsccAsync(empresaId, idOSscc.Trim(), ct).ConfigureAwait(false);
        if (u is null)
        {
            return Resultado.Fallo<EtiquetaUnidadDto>(NoExiste());
        }

        var productos = u.Contenido.Select(l => l.ProductoId).Distinct().ToList();
        var lotes = u.Contenido.Select(l => l.Lote).Distinct().ToList();
        string? producto = null;
        string? gtin = null;
        if (productos.Count == 1)
        {
            producto = (await _articulos.ObtenerAsync(productos, ct).ConfigureAwait(false)).GetValueOrDefault(productos[0])?.Nombre;
            var ficha = await _repo.FichaAsync(empresaId, productos[0], ct).ConfigureAwait(false);
            gtin = ficha?.GtinCaja ?? ficha?.Gtin;
        }
        else if (productos.Count > 1)
        {
            producto = "Palé mixto";
        }

        var soporte = u.SoporteId is { } sid ? (await _repo.SoporteAsync(sid, ct).ConfigureAwait(false))?.Nombre : null;
        var hijas = await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false);
        return Resultado.Ok(new EtiquetaUnidadDto(u.Sscc, producto, gtin, soporte, u.Cajas + hijas.Sum(h => h.Cajas), u.PesoNetoKg + hijas.Sum(h => h.PesoNetoKg), u.PesoBrutoKg,
            lotes.Count == 1 ? lotes[0] : null, u.Contenido.Min(l => l.FechaCaducidad), DateOnly.FromDateTime((u.CerradaEn ?? u.CreadaEn).UtcDateTime), u.ClienteId,
            productos.Count == 1 ? productos[0] : null));
    }

    // ------------------------------------------------------------------ auxiliares
    private async Task<Resultado<UnidadLogisticaDto>> CambiarAsync(Guid id, Func<UnidadLogistica, Task<Resultado>> accion, CancellationToken ct)
    {
        var u = await _repo.UnidadAsync(id, ct).ConfigureAwait(false);
        if (u is null)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(NoExiste());
        }

        var r = await accion(u).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<UnidadLogisticaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(u, ct).ConfigureAwait(false));
    }

    private async Task<Resultado<UnidadLogistica>> NuevaAsync(Guid empresaId, TipoUnidadLogistica tipo, OrigenUnidadLogistica origen, Guid almacenId, Guid? ubicacionId,
        TipoSoporte? soporte, Guid? plantillaId, int? completa, Guid? clienteId, Guid? pedidoVentaId, Guid? ordenId, string? observaciones, CancellationToken ct)
    {
        await _unidad.BloquearAsync($"logistica.sscc.{empresaId}", ct).ConfigureAwait(false);
        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        if (config is null)
        {
            config = ConfiguracionLogistica.Crear(empresaId);
            _repo.Agregar(config);
        }

        var sscc = config.SiguienteSscc();
        if (sscc.EsFallo)
        {
            return Resultado.Fallo<UnidadLogistica>(sscc.Error);
        }

        var u = UnidadLogistica.Crear(empresaId, sscc.Valor, tipo, origen, almacenId, ubicacionId, soporte, _reloj, plantillaId, completa, clienteId, pedidoVentaId, ordenId,
            observaciones);
        _repo.Agregar(u);
        return Resultado.Ok(u);
    }

    private static Resultado<ContenidoUnidad> Contenido(FichaLogistica ficha, int? cajas, decimal? unidades, string? lote, DateOnly? caducidad)
    {
        if (cajas is null && unidades is null)
        {
            return Resultado.Fallo<ContenidoUnidad>(Error.Validacion("unidad.cantidad", "Indica las cajas o las unidades."));
        }

        var u = unidades ?? cajas!.Value * (decimal)ficha.UnidadesPorCaja;
        var c = cajas ?? (int)(Math.Sign(u) * Math.Ceiling(Math.Abs(u) / ficha.UnidadesPorCaja));
        var neto = ficha.PesoNetoUnidadKg is { } pu ? Math.Round(pu * u, 3) : 0m;
        var embalaje = (ficha.PesoBrutoCajaKg ?? ficha.PesoNetoCajaKg ?? 0m) - (ficha.PesoNetoCajaKg ?? 0m);
        return Resultado.Ok(new ContenidoUnidad(ficha.ProductoId, lote, caducidad, c, u, neto, Math.Round(neto + embalaje * c, 3)));
    }

    private async Task<int?> AlturaAsync(UnidadLogistica u, FichaLogistica ficha, int cajas, CancellationToken ct)
    {
        var mosaico = await _maestros.MosaicoAsync(u.EmpresaId, ficha, u.ClienteId, u.PlantillaId, ct).ConfigureAwait(false);
        return mosaico.EsCorrecto ? CalculadoraPaletizado.Pale(ficha, mosaico.Valor with { Soporte = u.SoporteId is { } s ? await _repo.SoporteAsync(s, ct).ConfigureAwait(false) : null },
            cajas).AlturaMm : null;
    }

    private async Task RecalcularPadreAsync(UnidadLogistica u, CancellationToken ct, UnidadLogistica? anterior = null)
    {
        foreach (var padre in new[] { u.PadreId is { } p ? await _repo.UnidadAsync(p, ct).ConfigureAwait(false) : null, anterior }.Where(x => x is not null))
        {
            var hijas = (await _repo.HijasAsync(padre!.Id, ct).ConfigureAwait(false)).Where(h => h.Viva && h.PadreId == padre.Id).ToList();
            if (u.PadreId == padre.Id && hijas.All(h => h.Id != u.Id))
            {
                hijas.Add(u);
            }

            padre.FijarPesoHijas(hijas.Where(h => h.PadreId == padre.Id).Sum(h => h.PesoBrutoKg));
        }
    }

    private async Task<UnidadLogisticaDto> DtoAsync(UnidadLogistica u, CancellationToken ct)
    {
        var articulos = await _articulos.ObtenerAsync(u.Contenido.Select(l => l.ProductoId).Distinct().ToList(), ct).ConfigureAwait(false);
        var hijas = (await _repo.HijasAsync(u.Id, ct).ConfigureAwait(false)).Where(h => h.PadreId == u.Id).Select(h => h.Sscc).ToList();
        return Dto(u, articulos, hijas);
    }

    private static UnidadLogisticaDto Dto(UnidadLogistica u, IReadOnlyDictionary<Guid, ArticuloLogistica> articulos, IReadOnlyList<string> hijas) => new(u.Id, u.Sscc,
        u.Tipo.ToString(), u.Estado.ToString(), u.Origen.ToString(), u.SoporteId, u.PadreId, u.AlmacenId, u.UbicacionId, u.ClienteId, u.PedidoVentaId, u.OrdenFabricacionId,
        u.PlantillaId, u.Cajas, u.CajasCompleta, u.PesoNetoKg, u.PesoBrutoKg, u.AlturaMm, u.CreadaEn, u.CerradaEn, u.FechaExpedicion, u.ReferenciaExpedicion, u.Observaciones,
        u.MotivoAnulacion,
        u.Contenido.Select(l => new LineaUnidadDto(l.ProductoId, articulos.GetValueOrDefault(l.ProductoId)?.Nombre, l.Lote, l.FechaCaducidad, l.Cajas, l.Unidades, l.PesoNetoKg,
            l.PesoBrutoKg)).ToList(), hijas);

    private static Error SinFicha() => Error.Validacion("ficha_logistica.falta", "El artículo no tiene ficha logística (unidades por caja, pesos y mosaico).");

    private static Error NoExiste() => Error.NoEncontrado("unidad.no_encontrada", "La unidad logística no existe.");
}
