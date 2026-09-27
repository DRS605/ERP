using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Aplicacion;

public sealed record NodoTrazaDto(Guid PartidaId, string Codigo, Guid ProductoId, string Origen, DateOnly Fecha, int Nivel, decimal KilosIniciales, bool Anulada,
    string? Parte, IReadOnlyList<Guid> Relacionadas);

public sealed record OrigenCampoDto(
    Guid PartidaId, string Partida, Guid RecepcionId, string? Recepcion, DateOnly FechaRecepcion, Guid? AgricultorId, string? Agricultor, Guid? ParcelaId,
    string? Parcela, string? ReferenciaSigpac, DateOnly? FechaRecoleccion, decimal Kilos);

public sealed record DestinoClienteDto(Guid PaleId, string Sscc, string Estado, Guid? ClienteId, DateOnly? FechaExpedicion, string? Referencia, Guid PartidaId, string Partida, decimal Kilos);

/// <summary>Traza de una partida o palé: el árbol de partidas, su origen en el campo y su destino en los clientes.</summary>
public sealed record TrazaDto(string Sentido, IReadOnlyList<NodoTrazaDto> Partidas, IReadOnlyList<OrigenCampoDto> Origenes, IReadOnlyList<DestinoClienteDto> Destinos);

/// <summary>
/// Trazabilidad (Reglamento CE 178/2002, «un paso atrás y un paso adelante», y hasta el final): desde un palé o
/// una partida, hacia atrás hasta la parcela y el agricultor, y hacia delante hasta los palés expedidos y sus
/// clientes. Recorre la genealogía de los partes de confección a cualquier profundidad.
/// </summary>
public sealed class TrazabilidadAgro
{
    private const int ProfundidadMaxima = 50;
    private readonly IRepositorioAgro _repo;

    public TrazabilidadAgro(IRepositorioAgro repo) => _repo = repo;

    public async Task<Resultado<TrazaDto>> HaciaAtrasAsync(Guid empresaId, Guid? partidaId, string? sscc, CancellationToken ct = default)
    {
        var inicio = await InicioAsync(empresaId, partidaId, sscc, ct).ConfigureAwait(false);
        if (inicio.EsFallo)
        {
            return Resultado.Fallo<TrazaDto>(inicio.Error);
        }

        var nodos = new Dictionary<Guid, (int Nivel, List<Guid> Origenes)>();
        var frontera = inicio.Valor.ToList();
        foreach (var id in frontera)
        {
            nodos[id] = (0, []);
        }

        for (var nivel = 1; frontera.Count > 0 && nivel <= ProfundidadMaxima; nivel++)
        {
            var enlaces = await _repo.OrigenesAsync(frontera, ct).ConfigureAwait(false);
            var siguientes = new List<Guid>();
            foreach (var e in enlaces)
            {
                if (!nodos[e.DestinoId].Origenes.Contains(e.OrigenId))
                {
                    nodos[e.DestinoId].Origenes.Add(e.OrigenId);
                }

                if (!nodos.ContainsKey(e.OrigenId))
                {
                    nodos[e.OrigenId] = (nivel, []);
                    siguientes.Add(e.OrigenId);
                }
            }

            frontera = siguientes;
        }

        var partidas = (await _repo.PartidasAsync(nodos.Keys.ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var recepciones = (await _repo.RecepcionesAsync(partidas.Values.Where(p => p.RecepcionId is not null).Select(p => p.RecepcionId!.Value).Distinct().ToList(), ct)
            .ConfigureAwait(false)).ToDictionary(r => r.Id);
        var agricultores = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id);
        var parcelas = (await _repo.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var origenes = partidas.Values.Where(p => p.Origen == OrigenPartida.Recepcion && p.RecepcionId is not null)
            .Select(p =>
            {
                var r = recepciones.GetValueOrDefault(p.RecepcionId!.Value);
                var linea = r?.Lineas.FirstOrDefault(l => l.Id == p.LineaRecepcionId);
                var parcela = p.ParcelaId is { } pid ? parcelas.GetValueOrDefault(pid) : null;
                return new OrigenCampoDto(p.Id, p.Codigo, p.RecepcionId!.Value, r?.NumeroCompleto, p.Fecha, p.AgricultorId,
                    p.AgricultorId is { } a ? agricultores.GetValueOrDefault(a)?.Nombre : null, p.ParcelaId, parcela?.Codigo, parcela?.ReferenciaSigpac,
                    linea?.FechaRecoleccion, p.KilosIniciales);
            })
            .OrderBy(o => o.FechaRecepcion).ThenBy(o => o.Partida, StringComparer.Ordinal).ToList();

        return Resultado.Ok(new TrazaDto("Atras", await NodosAsync(partidas, nodos.ToDictionary(n => n.Key, n => (n.Value.Nivel, (IReadOnlyList<Guid>)n.Value.Origenes)), ct).ConfigureAwait(false),
            origenes, []));
    }

    public async Task<Resultado<TrazaDto>> HaciaAdelanteAsync(Guid empresaId, Guid? partidaId, Guid? recepcionId, CancellationToken ct = default)
    {
        IReadOnlyList<Guid> inicio;
        if (recepcionId is { } rid)
        {
            inicio = (await _repo.PartidasDeRecepcionAsync(rid, ct).ConfigureAwait(false)).Select(p => p.Id).ToList();
            if (inicio.Count == 0)
            {
                return Resultado.Fallo<TrazaDto>(Error.NoEncontrado("traza.sin_partidas", "La recepción no existe o no está confirmada."));
            }
        }
        else
        {
            var r = await InicioAsync(empresaId, partidaId, null, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<TrazaDto>(r.Error);
            }

            inicio = r.Valor;
        }

        var nodos = inicio.ToDictionary(id => id, _ => (Nivel: 0, Destinos: new List<Guid>()));
        var frontera = inicio.ToList();
        for (var nivel = 1; frontera.Count > 0 && nivel <= ProfundidadMaxima; nivel++)
        {
            var enlaces = await _repo.DestinosAsync(frontera, ct).ConfigureAwait(false);
            var siguientes = new List<Guid>();
            foreach (var e in enlaces)
            {
                if (!nodos[e.OrigenId].Destinos.Contains(e.DestinoId))
                {
                    nodos[e.OrigenId].Destinos.Add(e.DestinoId);
                }

                if (!nodos.ContainsKey(e.DestinoId))
                {
                    nodos[e.DestinoId] = (nivel, []);
                    siguientes.Add(e.DestinoId);
                }
            }

            frontera = siguientes;
        }

        var partidas = (await _repo.PartidasAsync(nodos.Keys.ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var movimientos = await _repo.MovimientosAsync(nodos.Keys.ToList(), ct).ConfigureAwait(false);
        // La salida de cada expedición y, si se anuló, su vuelta (mismo tipo de documento): cuenta lo que salió de verdad.
        var expediciones = movimientos.Where(m => m.DocumentoTipo == PalesAgro.DocumentoExpedicion && m.PaleId is not null).ToList();
        var enPale = movimientos.Where(m => m.PaleId is not null).GroupBy(m => (m.PaleId!.Value, m.PartidaId))
            .Select(g => (Pale: g.Key.Value, Partida: g.Key.PartidaId, Saldo: g.Sum(m => m.Kilos))).Where(x => x.Saldo > 0m).ToList();
        var pales = (await _repo.PalesAsync(expediciones.Select(m => m.PaleId!.Value).Concat(enPale.Select(x => x.Pale)).Distinct().ToList(), ct).ConfigureAwait(false))
            .ToDictionary(p => p.Id);
        var destinos = expediciones.GroupBy(m => (m.PaleId!.Value, m.PartidaId)).Select(g => (Pale: g.Key.Value, Partida: g.Key.PartidaId, Kilos: -g.Sum(m => m.Kilos)))
            .Where(x => x.Kilos > 0m)
            .Concat(enPale.Select(x => (x.Pale, x.Partida, Kilos: x.Saldo)))
            .Select(x =>
            {
                var p = pales[x.Pale];
                return new DestinoClienteDto(p.Id, p.Sscc, p.Estado.ToString(), p.ClienteId, p.FechaExpedicion, p.ReferenciaExpedicion, x.Partida,
                    partidas.GetValueOrDefault(x.Partida)?.Codigo ?? "?", x.Kilos);
            })
            .OrderBy(d => d.FechaExpedicion ?? DateOnly.MaxValue).ThenBy(d => d.Sscc, StringComparer.Ordinal).ToList();

        return Resultado.Ok(new TrazaDto("Adelante", await NodosAsync(partidas, nodos.ToDictionary(n => n.Key, n => (n.Value.Nivel, (IReadOnlyList<Guid>)n.Value.Destinos)), ct).ConfigureAwait(false),
            [], destinos));
    }

    private async Task<Resultado<IReadOnlyList<Guid>>> InicioAsync(Guid empresaId, Guid? partidaId, string? sscc, CancellationToken ct)
    {
        if (partidaId is { } id)
        {
            return await _repo.PartidaAsync(id, ct).ConfigureAwait(false) is null
                ? Resultado.Fallo<IReadOnlyList<Guid>>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe."))
                : Resultado.Ok<IReadOnlyList<Guid>>([id]);
        }

        if (!string.IsNullOrWhiteSpace(sscc))
        {
            var pale = await _repo.PalePorSsccAsync(empresaId, sscc.Trim(), ct).ConfigureAwait(false);
            if (pale is null)
            {
                return Resultado.Fallo<IReadOnlyList<Guid>>(Error.NoEncontrado("pale.no_encontrado", "No hay ningún palé con ese SSCC."));
            }

            var contenido = await _repo.ContenidoPaleAsync(pale.Id, ct).ConfigureAwait(false);
            return Resultado.Ok<IReadOnlyList<Guid>>(contenido.Select(c => c.PartidaId).Distinct().ToList());
        }

        return Resultado.Fallo<IReadOnlyList<Guid>>(Error.Validacion("traza.inicio", "Indica una partida, un SSCC o una recepción."));
    }

    private async Task<IReadOnlyList<NodoTrazaDto>> NodosAsync(Dictionary<Guid, Partida> partidas, Dictionary<Guid, (int Nivel, IReadOnlyList<Guid> Relacionadas)> nodos, CancellationToken ct)
    {
        var partes = new Dictionary<Guid, string?>();
        foreach (var parteId in partidas.Values.Where(p => p.ParteConfeccionId is not null).Select(p => p.ParteConfeccionId!.Value).Distinct())
        {
            partes[parteId] = (await _repo.ParteAsync(parteId, ct).ConfigureAwait(false))?.NumeroCompleto;
        }

        return nodos.Where(n => partidas.ContainsKey(n.Key))
            .Select(n =>
            {
                var p = partidas[n.Key];
                return new NodoTrazaDto(p.Id, p.Codigo, p.ProductoId, p.Origen.ToString(), p.Fecha, n.Value.Nivel, p.KilosIniciales, p.Anulada,
                    p.ParteConfeccionId is { } pc ? partes.GetValueOrDefault(pc) : null, n.Value.Relacionadas);
            })
            .OrderBy(n => n.Nivel).ThenBy(n => n.Codigo, StringComparer.Ordinal).ToList();
    }
}

public sealed record EntregasAgricultorDto(
    Guid AgricultorId, string Agricultor, int Entregas, decimal KilosEntregados, decimal KilosLiquidados, decimal KilosPendientes, decimal ImporteBruto,
    decimal BaseLiquidada, decimal APagar, decimal PrecioMedioKg);

public sealed record CosteParcelaDto(
    Guid ParcelaId, string Codigo, string Nombre, Guid AgricultorId, string? Agricultor, decimal? SuperficieHa, decimal KilosRecibidos, decimal? KilosPorHa,
    decimal ImporteLiquidado, decimal? PrecioMedioLiquidado, Guid? CentroAnaliticoId, decimal? CosteCultivo, decimal? CosteCultivoPorKilo, decimal? IngresosCentro);

public sealed record CosteConfeccionDto(Guid ProductoId, decimal KilosObtenidos, decimal CosteTotal, decimal CosteKg, int Partes);

public sealed record InformeCampanaDto(
    Guid CampanaId, string Campana, DateOnly Desde, DateOnly Hasta, bool AnaliticaDisponible, decimal KilosRecibidos, decimal KilosLiquidados, decimal ImporteLiquidado,
    IReadOnlyList<EntregasAgricultorDto> Agricultores, IReadOnlyList<CosteParcelaDto> Parcelas, IReadOnlyList<CosteConfeccionDto> Confeccion);

/// <summary>
/// Informe de campaña: entregas y liquidaciones por agricultor, producción y coste por kilo de cada parcela
/// (con el coste imputado en analítica a su centro) y coste por kilo de la confección por producto.
/// </summary>
public sealed class InformesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly ICosteAnalitico _analitica;

    public InformesAgro(IRepositorioAgro repo, ICosteAnalitico analitica)
    {
        _repo = repo;
        _analitica = analitica;
    }

    public async Task<Resultado<InformeCampanaDto>> CampanaAsync(Guid empresaId, Guid campanaId, CancellationToken ct = default)
    {
        var campana = await _repo.CampanaAsync(campanaId, ct).ConfigureAwait(false);
        if (campana is null)
        {
            return Resultado.Fallo<InformeCampanaDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        var recepciones = (await _repo.RecepcionesAsync(empresaId, campana.Desde, campana.Hasta, null, ct).ConfigureAwait(false))
            .Where(r => r.Estado == EstadoRecepcion.Confirmada && r.CampanaId == campanaId).ToList();
        var liquidaciones = (await _repo.LiquidacionesAsync(empresaId, null, ct).ConfigureAwait(false))
            .Where(l => l.CampanaId == campanaId && l.Estado == EstadoLiquidacion.Emitida).ToList();
        var agricultores = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id);
        var parcelas = await _repo.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false);
        var lineasLiquidadas = liquidaciones.SelectMany(l => l.Lineas).ToList();

        var porAgricultor = recepciones.GroupBy(r => r.AgricultorId).Select(g =>
        {
            var liq = liquidaciones.Where(l => l.AgricultorId == g.Key).ToList();
            var kilos = g.Sum(r => r.NetoKg);
            var liquidados = liq.Sum(l => l.Kilos);
            var bruto = liq.Sum(l => l.Bruto);
            return new EntregasAgricultorDto(g.Key, agricultores.GetValueOrDefault(g.Key)?.Nombre ?? "?", g.Count(), kilos, liquidados, kilos - liquidados, bruto,
                liq.Sum(l => l.BaseImponible), liq.Sum(l => l.APagar), liquidados == 0m ? 0m : decimal.Round(bruto / liquidados, 4));
        }).OrderByDescending(a => a.KilosEntregados).ToList();

        var costes = await _analitica.PorCentroAsync(empresaId, campana.Desde, campana.Hasta, ct).ConfigureAwait(false);
        var lineasRecepcion = recepciones.SelectMany(r => r.Lineas).ToList();
        var porParcela = parcelas.Select(p =>
        {
            var lineas = lineasRecepcion.Where(l => l.ParcelaId == p.Id).ToList();
            var kilos = lineas.Sum(l => l.NetoKg ?? 0m);
            var ids = lineas.Select(l => l.Id).ToHashSet();
            var liquidadas = lineasLiquidadas.Where(l => ids.Contains(l.LineaRecepcionId)).ToList();
            var importe = liquidadas.Sum(l => l.Importe);
            var kilosLiquidados = liquidadas.Sum(l => l.Kilos);
            (decimal Gastos, decimal Ingresos)? centro = p.CentroAnaliticoId is { } c && costes is not null && costes.TryGetValue(c, out var v) ? v : null;
            return new CosteParcelaDto(p.Id, p.Codigo, p.Nombre, p.AgricultorId, agricultores.GetValueOrDefault(p.AgricultorId)?.Nombre, p.SuperficieHa, kilos,
                p.SuperficieHa is > 0m && kilos > 0m ? decimal.Round(kilos / p.SuperficieHa.Value, 2) : null, importe,
                kilosLiquidados == 0m ? null : decimal.Round(importe / kilosLiquidados, 4), p.CentroAnaliticoId, centro?.Gastos,
                centro is { } cc && kilos > 0m ? decimal.Round(cc.Gastos / kilos, 4) : null, centro?.Ingresos);
        }).Where(p => p.KilosRecibidos > 0m || p.CosteCultivo is > 0m).OrderBy(p => p.Codigo, StringComparer.Ordinal).ToList();

        var partes = (await _repo.PartesAsync(empresaId, campana.Desde, campana.Hasta, ct).ConfigureAwait(false)).Where(p => p.Estado == EstadoParte.Validado).ToList();
        var confeccion = partes.SelectMany(p => p.Salidas.Select(s => (Parte: p.Id, Salida: s))).GroupBy(x => x.Salida.ProductoId).Select(g =>
        {
            var kilos = g.Sum(x => x.Salida.Kilos);
            var coste = g.Sum(x => x.Salida.Coste);
            return new CosteConfeccionDto(g.Key, kilos, coste, kilos == 0m ? 0m : decimal.Round(coste / kilos, 4), g.Select(x => x.Parte).Distinct().Count());
        }).OrderByDescending(c => c.KilosObtenidos).ToList();

        return Resultado.Ok(new InformeCampanaDto(campana.Id, campana.Codigo, campana.Desde, campana.Hasta, costes is not null, recepciones.Sum(r => r.NetoKg),
            liquidaciones.Sum(l => l.Kilos), liquidaciones.Sum(l => l.Bruto), porAgricultor, porParcela, confeccion));
    }
}
