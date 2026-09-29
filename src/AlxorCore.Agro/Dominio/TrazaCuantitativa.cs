namespace AlxorCore.Agro.Dominio;

/// <summary>Destino de unos kilos de una compra: expedidos, en existencias, merma de confección o ajuste.</summary>
public enum DestinoKilos
{
    Expedido = 1,
    Existencias = 2,
    Merma = 3,
    Ajuste = 4,
}

/// <summary>Kilos de una partida de compra (de recepción) que acabaron en un sitio, vistos desde la partida en que están.</summary>
public sealed record TramoTraza(DestinoKilos Destino, Guid PartidaId, Guid? PaleId, Guid? ParteId, DateOnly? Fecha, decimal Kilos);

/// <summary>
/// Traza cuantitativa: cuántos kilos de cada compra (partida de recepción) van en cada partida, palé y venta, con la
/// genealogía de los partes de confección. Lo consumido de un origen se reparte entre sus salidas (<see cref="Genealogia"/>),
/// así que una partida confeccionada «es» una mezcla de sus orígenes en la proporción de lo que se consumió de cada uno,
/// y la merma del parte también. Con eso:
/// <list type="bullet">
/// <item><b>hacia atrás</b>, los kilos de un palé o una venta se reparten entre las compras de las que salieron;</item>
/// <item><b>hacia delante</b>, los kilos de una compra se reparten entre lo expedido, lo que queda en existencias, la
/// merma y los ajustes, y la suma es lo que entró: el balance de masas cierra.</item>
/// </list>
/// </summary>
public sealed class MotorTraza
{
    private readonly Dictionary<Guid, Partida> _partidas;
    private readonly Dictionary<Guid, List<Genealogia>> _entradasDe;
    private readonly Dictionary<Guid, List<MovimientoPartida>> _movimientos;
    private readonly Dictionary<Guid, IReadOnlyDictionary<Guid, decimal>> _composicion = [];

    public MotorTraza(IEnumerable<Partida> partidas, IEnumerable<Genealogia> genealogia, IEnumerable<MovimientoPartida> movimientos)
    {
        ArgumentNullException.ThrowIfNull(partidas);
        ArgumentNullException.ThrowIfNull(genealogia);
        ArgumentNullException.ThrowIfNull(movimientos);
        // Un parte anulado no cuenta: su salida se anuló y lo consumido volvió a los orígenes.
        _partidas = partidas.GroupBy(p => p.Id).Select(g => g.First()).Where(p => !(p.Anulada && p.Origen == OrigenPartida.Confeccion)).ToDictionary(p => p.Id);
        _entradasDe = genealogia.Where(g => _partidas.ContainsKey(g.DestinoId)).GroupBy(g => g.DestinoId)
            .ToDictionary(g => g.Key, g => g.GroupBy(x => x.Id).Select(x => x.First()).ToList());
        var anulados = genealogia.Where(g => !_partidas.ContainsKey(g.DestinoId)).Select(g => g.ParteId).ToHashSet();
        _movimientos = movimientos.Where(m => !(m.DocumentoId is { } d && anulados.Contains(d) && m.DocumentoTipo == "ParteConfeccion"))
            .GroupBy(m => m.PartidaId).ToDictionary(g => g.Key, g => g.GroupBy(x => x.Id).Select(x => x.First()).ToList());
    }

    /// <summary>
    /// De qué compras (partidas de recepción) está hecha la partida, en fracción de su fruta (suma 1). Una partida de
    /// recepción es entera de sí misma; una confeccionada, la mezcla de sus orígenes según lo consumido de cada uno.
    /// </summary>
    public IReadOnlyDictionary<Guid, decimal> Composicion(Guid partidaId) => Composicion(partidaId, 0);

    private IReadOnlyDictionary<Guid, decimal> Composicion(Guid partidaId, int profundidad)
    {
        if (_composicion.TryGetValue(partidaId, out var hecha))
        {
            return hecha;
        }

        var entradas = _entradasDe.GetValueOrDefault(partidaId);
        IReadOnlyDictionary<Guid, decimal> resultado;
        if (entradas is null || entradas.Count == 0 || profundidad > 200)
        {
            resultado = new Dictionary<Guid, decimal> { [partidaId] = 1m };
        }
        else
        {
            var total = entradas.Sum(e => e.KilosOrigen);
            var mezcla = new Dictionary<Guid, decimal>();
            foreach (var e in entradas)
            {
                var peso = total == 0m ? 1m / entradas.Count : e.KilosOrigen / total;
                foreach (var (compra, fraccion) in Composicion(e.OrigenId, profundidad + 1))
                {
                    mezcla[compra] = mezcla.GetValueOrDefault(compra) + peso * fraccion;
                }
            }

            resultado = mezcla;
        }

        _composicion[partidaId] = resultado;
        return resultado;
    }

    /// <summary>Kilos de cada compra en unos kilos de una partida (redondeado a gramos; el último se lleva el resto).</summary>
    public IReadOnlyList<(Guid Compra, decimal Kilos)> Repartir(Guid partidaId, decimal kilos)
    {
        var comp = Composicion(partidaId).OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();
        var lista = new List<(Guid, decimal)>();
        var resto = kilos;
        for (var i = 0; i < comp.Count; i++)
        {
            var parte = i == comp.Count - 1 ? resto : Math.Round(kilos * comp[i].Value, 3, MidpointRounding.AwayFromZero);
            resto -= parte;
            lista.Add((comp[i].Key, parte));
        }

        return lista;
    }

    /// <summary>
    /// A dónde fueron los kilos de una compra: por cada partida que lleva fruta suya (ella misma y sus confeccionadas),
    /// su parte de lo expedido, de lo que queda (por palé), de la merma de su parte y de los ajustes. Lo consumido en un
    /// parte no se cuenta aquí: sigue en las partidas que salieron de él.
    /// </summary>
    public IReadOnlyList<TramoTraza> Destinos(Guid compraId)
    {
        var tramos = new List<TramoTraza>();
        foreach (var partida in _partidas.Values)
        {
            var fraccion = Composicion(partida.Id).GetValueOrDefault(compraId);
            if (fraccion == 0m)
            {
                continue;
            }

            var movimientos = _movimientos.GetValueOrDefault(partida.Id) ?? [];
            foreach (var m in movimientos.Where(m => m.Tipo == TipoMovimientoPartida.Expedicion))
            {
                tramos.Add(new TramoTraza(DestinoKilos.Expedido, partida.Id, m.PaleId, null, m.Fecha, -m.Kilos * fraccion));
            }

            // Lo que la expedición anulada devolvió: vuelve a existencias (se resta de lo expedido de ese palé).
            foreach (var m in movimientos.Where(m => m.Tipo == TipoMovimientoPartida.Anulacion && m.DocumentoTipo == "Expedicion"))
            {
                tramos.Add(new TramoTraza(DestinoKilos.Expedido, partida.Id, m.PaleId, null, m.Fecha, -m.Kilos * fraccion));
            }

            foreach (var m in movimientos.Where(m => m.Tipo == TipoMovimientoPartida.Ajuste || (m.Tipo == TipoMovimientoPartida.Anulacion && m.DocumentoTipo != "Expedicion")))
            {
                tramos.Add(new TramoTraza(DestinoKilos.Ajuste, partida.Id, m.PaleId, null, m.Fecha, -m.Kilos * fraccion));
            }

            foreach (var g in movimientos.GroupBy(m => m.PaleId))
            {
                var saldo = g.Sum(m => m.Kilos);
                if (saldo != 0m)
                {
                    tramos.Add(new TramoTraza(DestinoKilos.Existencias, partida.Id, g.Key, null, null, saldo * fraccion));
                }
            }

            // Merma del parte que creó esta partida: lo que entró de sus orígenes y no salió en ella.
            if (_entradasDe.GetValueOrDefault(partida.Id) is { Count: > 0 } entradas)
            {
                var merma = entradas.Sum(e => e.KilosOrigen) - partida.KilosIniciales;
                if (merma != 0m)
                {
                    tramos.Add(new TramoTraza(DestinoKilos.Merma, partida.Id, null, entradas[0].ParteId, partida.Fecha, merma * fraccion));
                }
            }
        }

        return tramos.Select(t => t with { Kilos = Math.Round(t.Kilos, 3, MidpointRounding.AwayFromZero) }).Where(t => t.Kilos != 0m).ToList();
    }

    /// <summary>Kilos que entraron de una compra: su entrada y sus rectificaciones.</summary>
    public decimal Entrados(Guid compraId) =>
        (_movimientos.GetValueOrDefault(compraId) ?? []).Where(m => m.Tipo is TipoMovimientoPartida.Entrada or TipoMovimientoPartida.Rectificacion).Sum(m => m.Kilos);

    /// <summary>
    /// Descuadres de la genealogía: partidas de las que se consumió más o menos de lo que su genealogía reparte, o
    /// confeccionadas con más kilos de los que les entraron. Con los datos de ALXOR no los hay (lo impide la base de
    /// datos); sirven para revisar datos migrados.
    /// </summary>
    public IReadOnlyList<(Guid PartidaId, string Motivo, decimal Diferencia)> Descuadres(IEnumerable<Genealogia> genealogia)
    {
        ArgumentNullException.ThrowIfNull(genealogia);
        var salidas = genealogia.GroupBy(g => g.OrigenId).ToDictionary(g => g.Key, g => g.Sum(x => x.KilosOrigen));
        var lista = new List<(Guid, string, decimal)>();
        foreach (var p in _partidas.Values)
        {
            var consumido = -(_movimientos.GetValueOrDefault(p.Id) ?? []).Where(m => m.Tipo == TipoMovimientoPartida.Consumo).Sum(m => m.Kilos);
            var repartido = salidas.GetValueOrDefault(p.Id);
            if (consumido != repartido)
            {
                lista.Add((p.Id, "Lo consumido en partes no coincide con lo que reparte su genealogía.", consumido - repartido));
            }

            if (_entradasDe.GetValueOrDefault(p.Id) is { Count: > 0 } entradas && p.KilosIniciales > entradas.Sum(e => e.KilosOrigen))
            {
                lista.Add((p.Id, "Salieron más kilos de los que entraron en su parte.", p.KilosIniciales - entradas.Sum(e => e.KilosOrigen)));
            }
        }

        return lista;
    }
}
