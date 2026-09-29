using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Aplicacion;

/// <summary>Kilos de una compra (partida de recepción) en lo trazado, con su agricultor, parcela y recepción.</summary>
public sealed record CompraKilosDto(Guid PartidaId, string Partida, Guid? RecepcionId, string? Recepcion, DateOnly Fecha, Guid? AgricultorId, string? Agricultor,
    Guid? ParcelaId, string? Parcela, string? ReferenciaSigpac, Guid ProductoId, decimal Kilos, decimal Porcentaje);

/// <summary>Lo trazado: kilos de una partida en un palé (o suelta).</summary>
public sealed record ContenidoTrazadoDto(Guid PartidaId, string Partida, Guid ProductoId, Guid? PaleId, string? Sscc, decimal Kilos);

/// <summary>Hacia atrás: de qué compras salieron los kilos de un palé, una venta o una partida.</summary>
public sealed record ComposicionDto(string Trazado, decimal Kilos, IReadOnlyList<ContenidoTrazadoDto> Contenido, IReadOnlyList<CompraKilosDto> Compras);

public sealed record VentaKilosDto(Guid PaleId, string Sscc, Guid? ClienteId, DateOnly? Fecha, string? Referencia, Guid? AlbaranId, Guid PartidaId, string Partida, Guid ProductoId,
    decimal Kilos);

public sealed record ExistenciaKilosDto(Guid PartidaId, string Partida, Guid ProductoId, Guid? PaleId, string? Sscc, decimal Kilos);

public sealed record MermaKilosDto(Guid? ParteId, string? Parte, Guid PartidaId, string Partida, DateOnly? Fecha, decimal Kilos);

/// <summary>
/// Hacia delante: a dónde fueron los kilos de una compra. Entrados = expedidos + en existencias + merma + ajustes; si
/// no cuadra, <see cref="Descuadre"/> dice cuánto falta o sobra.
/// </summary>
public sealed record BalanceCompraDto(Guid PartidaId, string Partida, Guid? RecepcionId, string? Recepcion, DateOnly Fecha, Guid? AgricultorId, string? Agricultor,
    Guid ProductoId, decimal Entrados, decimal Expedidos, decimal Existencias, decimal Merma, decimal Ajustes, decimal Descuadre,
    IReadOnlyList<VentaKilosDto> Ventas, IReadOnlyList<ExistenciaKilosDto> EnExistencias, IReadOnlyList<MermaKilosDto> Mermas, IReadOnlyList<ExistenciaKilosDto> EnAjustes);

public sealed record DescuadreDto(Guid PartidaId, string Partida, string Motivo, decimal Kilos);

/// <summary>
/// Traza compra ↔ venta con kilos: de cada venta o palé, cuántos kilos vienen de cada compra (agricultor, parcela,
/// recepción), y de cada compra, cuántos kilos se vendieron a cada cliente, cuántos quedan y cuántos se mermaron. Se
/// calcula en cada consulta sobre el libro de movimientos y la genealogía, así que siempre está al día.
/// </summary>
public sealed class TrazaCompraVentaAgro
{
    private const int ProfundidadMaxima = 60;
    private readonly IRepositorioAgro _repo;

    public TrazaCompraVentaAgro(IRepositorioAgro repo) => _repo = repo;

    /// <summary>De qué compras salió lo trazado: un palé (por SSCC o id), un albarán de venta, lo vendido a un cliente en un periodo o una partida.</summary>
    public async Task<Resultado<ComposicionDto>> ComposicionAsync(Guid empresaId, string? sscc, Guid? albaranId, Guid? clienteId, DateOnly? desde, DateOnly? hasta, Guid? partidaId,
        CancellationToken ct = default)
    {
        var contenido = new List<(Guid PartidaId, Guid? PaleId, decimal Kilos)>();
        string trazado;
        if (!string.IsNullOrWhiteSpace(sscc) || albaranId is not null || clienteId is not null)
        {
            IReadOnlyList<Pale> pales;
            if (!string.IsNullOrWhiteSpace(sscc))
            {
                var pale = Guid.TryParse(sscc, out var id) ? await _repo.PaleAsync(id, ct).ConfigureAwait(false) : await _repo.PalePorSsccAsync(empresaId, sscc.Trim(), ct).ConfigureAwait(false);
                if (pale is null || pale.EmpresaId != empresaId)
                {
                    return Resultado.Fallo<ComposicionDto>(Error.NoEncontrado("pale.no_encontrado", "El palé no existe."));
                }

                pales = [pale];
                trazado = $"Palé {pale.Sscc}";
            }
            else if (albaranId is { } alb)
            {
                pales = await _repo.PalesDeAlbaranAsync(alb, ct).ConfigureAwait(false);
                trazado = $"Albarán {(pales.Count > 0 ? pales[0].ReferenciaExpedicion : null) ?? alb.ToString()}";
            }
            else
            {
                pales = (await _repo.PalesAsync(empresaId, EstadoPale.Expedido, ct).ConfigureAwait(false))
                    .Where(p => p.ClienteId == clienteId && (desde is null || p.FechaExpedicion >= desde) && (hasta is null || p.FechaExpedicion <= hasta)).ToList();
                trazado = $"Ventas al cliente {(desde is null ? "" : $"desde el {desde:dd/MM/yyyy} ")}{(hasta is null ? "" : $"hasta el {hasta:dd/MM/yyyy}")}".Trim();
            }

            foreach (var pale in pales)
            {
                var movs = await _repo.MovimientosDePaleAsync(pale.Id, ct).ConfigureAwait(false);
                // Expedido: lo que salió (neto de anulaciones); si no, lo que lleva.
                var porPartida = pale.Estado == EstadoPale.Expedido
                    ? movs.Where(m => m.DocumentoTipo == PalesAgro.DocumentoExpedicion).GroupBy(m => m.PartidaId).Select(g => (g.Key, -g.Sum(m => m.Kilos)))
                    : movs.GroupBy(m => m.PartidaId).Select(g => (g.Key, g.Sum(m => m.Kilos)));
                contenido.AddRange(porPartida.Where(x => x.Item2 > 0m).Select(x => (x.Key, (Guid?)pale.Id, x.Item2)));
            }
        }
        else if (partidaId is { } pid)
        {
            var partida = await _repo.PartidaAsync(pid, ct).ConfigureAwait(false);
            if (partida is null || partida.EmpresaId != empresaId)
            {
                return Resultado.Fallo<ComposicionDto>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe."));
            }

            contenido.Add((pid, null, partida.KilosIniciales));
            trazado = $"Partida {partida.Codigo}";
        }
        else
        {
            return Resultado.Fallo<ComposicionDto>(Error.Validacion("traza.sin_inicio", "Indica un palé, un albarán, un cliente o una partida."));
        }

        var motor = await MotorAsync(contenido.Select(c => c.PartidaId).Distinct().ToList(), hacia: -1, ct).ConfigureAwait(false);
        var kilosPorCompra = new Dictionary<Guid, decimal>();
        foreach (var (partida, _, kilos) in contenido)
        {
            foreach (var (compra, k) in motor.Motor.Repartir(partida, kilos))
            {
                kilosPorCompra[compra] = kilosPorCompra.GetValueOrDefault(compra) + k;
            }
        }

        var total = contenido.Sum(c => c.Kilos);
        var info = await InfoAsync(empresaId, motor.Partidas, ct).ConfigureAwait(false);
        var pales2 = (await _repo.PalesAsync(contenido.Where(c => c.PaleId is not null).Select(c => c.PaleId!.Value).Distinct().ToList(), ct).ConfigureAwait(false))
            .ToDictionary(p => p.Id);
        var compras = kilosPorCompra.Where(x => x.Value != 0m).OrderByDescending(x => x.Value).Select(x =>
        {
            var p = motor.Partidas[x.Key];
            var (recepcion, agricultor, parcela, sigpac) = info(p);
            return new CompraKilosDto(p.Id, p.Codigo, p.RecepcionId, recepcion, p.Fecha, p.AgricultorId, agricultor, p.ParcelaId, parcela, sigpac, p.ProductoId, x.Value,
                total == 0m ? 0m : Math.Round(x.Value * 100m / total, 2, MidpointRounding.AwayFromZero));
        }).ToList();
        return Resultado.Ok(new ComposicionDto(trazado, total,
            contenido.Select(c => new ContenidoTrazadoDto(c.PartidaId, motor.Partidas[c.PartidaId].Codigo, motor.Partidas[c.PartidaId].ProductoId, c.PaleId,
                c.PaleId is { } pa ? pales2.GetValueOrDefault(pa)?.Sscc : null, c.Kilos)).ToList(), compras));
    }

    /// <summary>A dónde fueron los kilos de una compra (una partida de recepción) o de todas las líneas de una recepción.</summary>
    public async Task<Resultado<IReadOnlyList<BalanceCompraDto>>> BalanceAsync(Guid empresaId, Guid? partidaId, Guid? recepcionId, CancellationToken ct = default)
    {
        IReadOnlyList<Partida> compras;
        if (partidaId is { } pid)
        {
            var p = await _repo.PartidaAsync(pid, ct).ConfigureAwait(false);
            compras = p is null || p.EmpresaId != empresaId ? [] : [p];
        }
        else if (recepcionId is { } rid)
        {
            compras = await _repo.PartidasDeRecepcionAsync(rid, ct).ConfigureAwait(false);
        }
        else
        {
            return Resultado.Fallo<IReadOnlyList<BalanceCompraDto>>(Error.Validacion("traza.sin_inicio", "Indica la partida o la recepción."));
        }

        if (compras.Count == 0)
        {
            return Resultado.Fallo<IReadOnlyList<BalanceCompraDto>>(Error.NoEncontrado("partida.no_encontrada", "No hay partidas que trazar."));
        }

        var motor = await MotorAsync(compras.Select(c => c.Id).ToList(), hacia: 1, ct).ConfigureAwait(false);
        var info = await InfoAsync(empresaId, motor.Partidas, ct).ConfigureAwait(false);
        var todosPales = motor.Movimientos.Where(m => m.PaleId is not null).Select(m => m.PaleId!.Value).Distinct().ToList();
        var pales = (await _repo.PalesAsync(todosPales, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var partes = new Dictionary<Guid, string?>();
        var lista = new List<BalanceCompraDto>();
        foreach (var compra in compras)
        {
            var tramos = motor.Motor.Destinos(compra.Id);
            foreach (var parte in tramos.Where(t => t.ParteId is not null).Select(t => t.ParteId!.Value).Distinct().Where(x => !partes.ContainsKey(x)))
            {
                partes[parte] = (await _repo.ParteAsync(parte, ct).ConfigureAwait(false))?.NumeroCompleto;
            }

            string Codigo(Guid id) => motor.Partidas.GetValueOrDefault(id)?.Codigo ?? "";
            Guid Producto(Guid id) => motor.Partidas.GetValueOrDefault(id)?.ProductoId ?? Guid.Empty;
            var ventas = tramos.Where(t => t.Destino == DestinoKilos.Expedido && t.PaleId is not null).GroupBy(t => (t.PaleId!.Value, t.PartidaId))
                .Select(g =>
                {
                    var pale = pales.GetValueOrDefault(g.Key.Item1);
                    return new VentaKilosDto(g.Key.Item1, pale?.Sscc ?? "", pale?.ClienteId, pale?.FechaExpedicion, pale?.ReferenciaExpedicion, pale?.AlbaranId, g.Key.PartidaId,
                        Codigo(g.Key.PartidaId), Producto(g.Key.PartidaId), g.Sum(t => t.Kilos));
                }).Where(v => v.Kilos != 0m).OrderBy(v => v.Fecha).ThenBy(v => v.Sscc).ToList();
            var existencias = tramos.Where(t => t.Destino == DestinoKilos.Existencias).Select(t => new ExistenciaKilosDto(t.PartidaId, Codigo(t.PartidaId), Producto(t.PartidaId),
                t.PaleId, t.PaleId is { } pa ? pales.GetValueOrDefault(pa)?.Sscc : null, t.Kilos)).ToList();
            var ajustes = tramos.Where(t => t.Destino == DestinoKilos.Ajuste).GroupBy(t => (t.PartidaId, t.PaleId)).Select(g => new ExistenciaKilosDto(g.Key.PartidaId,
                Codigo(g.Key.PartidaId), Producto(g.Key.PartidaId), g.Key.PaleId, g.Key.PaleId is { } pa ? pales.GetValueOrDefault(pa)?.Sscc : null, g.Sum(t => t.Kilos)))
                .Where(a => a.Kilos != 0m).ToList();
            var mermas = tramos.Where(t => t.Destino == DestinoKilos.Merma).Select(t => new MermaKilosDto(t.ParteId, t.ParteId is { } x ? partes.GetValueOrDefault(x) : null,
                t.PartidaId, Codigo(t.PartidaId), t.Fecha, t.Kilos)).ToList();
            var entrados = motor.Motor.Entrados(compra.Id);
            var expedidos = ventas.Sum(v => v.Kilos);
            var enStock = existencias.Sum(e => e.Kilos);
            var merma = mermas.Sum(m => m.Kilos);
            var ajuste = ajustes.Sum(a => a.Kilos);
            var (recepcion, agricultor, _, _) = info(compra);
            lista.Add(new BalanceCompraDto(compra.Id, compra.Codigo, compra.RecepcionId, recepcion, compra.Fecha, compra.AgricultorId, agricultor, compra.ProductoId, entrados,
                expedidos, enStock, merma, ajuste, entrados - expedidos - enStock - merma - ajuste, ventas, existencias, mermas, ajustes));
        }

        return Resultado.Ok<IReadOnlyList<BalanceCompraDto>>(lista);
    }

    /// <summary>
    /// Descuadres de una campaña: compras cuyo balance no cierra (más de 10 g por partida implicada, por redondeos) y
    /// genealogías que no reparten lo consumido. Con los datos de ALXOR debe salir vacío; sirve para datos migrados.
    /// </summary>
    public async Task<IReadOnlyList<DescuadreDto>> DescuadresAsync(Guid empresaId, Guid campanaId, CancellationToken ct = default)
    {
        var compras = (await _repo.PartidasDeCampanaAsync(empresaId, campanaId, ct).ConfigureAwait(false)).Where(p => p.Origen == OrigenPartida.Recepcion).ToList();
        if (compras.Count == 0)
        {
            return [];
        }

        var motor = await MotorAsync(compras.Select(c => c.Id).ToList(), hacia: 1, ct).ConfigureAwait(false);
        var lista = motor.Motor.Descuadres(motor.Genealogia).Select(d => new DescuadreDto(d.PartidaId, motor.Partidas[d.PartidaId].Codigo, d.Motivo, d.Diferencia)).ToList();
        foreach (var compra in compras)
        {
            var tramos = motor.Motor.Destinos(compra.Id);
            var diferencia = motor.Motor.Entrados(compra.Id) - tramos.Sum(t => t.Kilos);
            if (Math.Abs(diferencia) > 0.01m * Math.Max(1, tramos.Select(t => t.PartidaId).Distinct().Count()))
            {
                lista.Add(new DescuadreDto(compra.Id, compra.Codigo, "El balance de la compra no cierra: entrados ≠ expedidos + existencias + merma + ajustes.", diferencia));
            }
        }

        return lista;
    }

    /// <summary>
    /// Carga las partidas relacionadas (hacia atrás, sus orígenes; hacia delante, sus descendientes y los orígenes de
    /// estos, porque una confeccionada mezcla compras), con su genealogía y sus movimientos.
    /// </summary>
    private async Task<(MotorTraza Motor, Dictionary<Guid, Partida> Partidas, IReadOnlyList<Genealogia> Genealogia, IReadOnlyList<MovimientoPartida> Movimientos)> MotorAsync(
        IReadOnlyList<Guid> inicio, int hacia, CancellationToken ct)
    {
        var ids = inicio.ToHashSet();
        var genealogia = new Dictionary<Guid, Genealogia>();
        if (hacia > 0)
        {
            var frontera = inicio.ToList();
            for (var n = 0; frontera.Count > 0 && n < ProfundidadMaxima; n++)
            {
                var hijos = await _repo.DestinosAsync(frontera, ct).ConfigureAwait(false);
                foreach (var g in hijos)
                {
                    genealogia[g.Id] = g;
                }

                frontera = hijos.Select(g => g.DestinoId).Where(ids.Add).ToList();
            }
        }

        // Hacia atrás desde todo lo cargado, hasta las compras.
        var atras = ids.ToList();
        for (var n = 0; atras.Count > 0 && n < ProfundidadMaxima; n++)
        {
            var padres = await _repo.OrigenesAsync(atras, ct).ConfigureAwait(false);
            foreach (var g in padres)
            {
                genealogia[g.Id] = g;
            }

            atras = padres.Select(g => g.OrigenId).Where(ids.Add).ToList();
        }

        var partidas = (await _repo.PartidasAsync(ids.ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var movimientos = await _repo.MovimientosAsync(ids.ToList(), ct).ConfigureAwait(false);
        var lista = genealogia.Values.ToList();
        return (new MotorTraza(partidas.Values, lista, movimientos), partidas, lista, movimientos);
    }

    private async Task<Func<Partida, (string? Recepcion, string? Agricultor, string? Parcela, string? Sigpac)>> InfoAsync(Guid empresaId, Dictionary<Guid, Partida> partidas,
        CancellationToken ct)
    {
        var recepciones = (await _repo.RecepcionesAsync(partidas.Values.Where(p => p.RecepcionId is not null).Select(p => p.RecepcionId!.Value).Distinct().ToList(), ct)
            .ConfigureAwait(false)).ToDictionary(r => r.Id);
        var agricultores = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id);
        var parcelas = (await _repo.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        return p => (p.RecepcionId is { } r ? recepciones.GetValueOrDefault(r)?.NumeroCompleto : null,
            p.AgricultorId is { } a ? agricultores.GetValueOrDefault(a)?.Nombre : null,
            p.ParcelaId is { } pa ? parcelas.GetValueOrDefault(pa)?.Codigo : null,
            p.ParcelaId is { } pb ? parcelas.GetValueOrDefault(pb)?.ReferenciaSigpac : null);
    }
}
