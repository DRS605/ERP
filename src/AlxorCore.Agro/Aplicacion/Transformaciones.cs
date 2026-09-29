using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record DatosReglaTransformacion(Guid ProductoOrigenId, Guid ProductoDestinoId, decimal? MermaMaximaPct = null);

public sealed record ReglaTransformacionDto(Guid Id, Guid ProductoOrigenId, Guid ProductoDestinoId, decimal? MermaMaximaPct);

/// <summary>Una línea del repaletizado: de qué palé se saca y, si no se indica, todo su contenido (o todo el de una partida).</summary>
public sealed record DatosLineaRepaletizado(Guid OrigenPaleId, Guid? PartidaId = null, decimal? Kilos = null);

/// <summary>Repaletizado a un palé abierto existente o, sin destino, a uno nuevo; <paramref name="CerrarDestino"/> lo cierra al acabar.</summary>
public sealed record DatosRepaletizado(IReadOnlyList<DatosLineaRepaletizado> Lineas, Guid? DestinoPaleId = null, string? TipoPale = null, DateOnly? Fecha = null,
    string? Motivo = null, bool CerrarDestino = false);

public sealed record LineaRepaletizadoDto(Guid OrigenPaleId, Guid PartidaId, decimal Kilos, int Cajas);

public sealed record RepaletizadoDto(Guid Id, DateOnly Fecha, Guid DestinoPaleId, string? DestinoSscc, decimal Kilos, string? Motivo, Guid? UsuarioId, IReadOnlyList<LineaRepaletizadoDto> Lineas);

/// <summary>
/// Transformaciones: qué producto puede salir de cuál en la confección (con su merma máxima) y el repaletizado, que pasa
/// kilos de unos palés a otro sin cambiar de partida, en una sola operación registrada palé a palé.
/// </summary>
public sealed class TransformacionesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    public TransformacionesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<ReglaTransformacionDto>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ReglasTransformacionAsync(empresaId, ct).ConfigureAwait(false)).Select(Dto).ToList();

    public async Task<Resultado<ReglaTransformacionDto>> CrearReglaAsync(Guid empresaId, DatosReglaTransformacion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((await _repo.ReglasTransformacionAsync(empresaId, ct).ConfigureAwait(false)).Any(r => r.ProductoOrigenId == d.ProductoOrigenId && r.ProductoDestinoId == d.ProductoDestinoId))
        {
            return Resultado.Fallo<ReglaTransformacionDto>(Error.Conflicto("regla.repetida", "Esa transformación ya está dada de alta."));
        }

        var r = ReglaTransformacion.Crear(empresaId, d.ProductoOrigenId, d.ProductoDestinoId, d.MermaMaximaPct);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ReglaTransformacionDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(r.Valor));
    }

    public async Task<Resultado<ReglaTransformacionDto>> CambiarReglaAsync(Guid id, DatosReglaTransformacion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var r = await _repo.ReglaTransformacionAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<ReglaTransformacionDto>(NoExiste());
        }

        var c = r.Cambiar(d.MermaMaximaPct);
        if (c.EsFallo)
        {
            return Resultado.Fallo<ReglaTransformacionDto>(c.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(r));
    }

    /// <summary>Quita una regla (afecta a los partes que se validen después; los validados no cambian).</summary>
    public async Task<Resultado> EliminarReglaAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ReglaTransformacionAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Repaletiza: saca de cada palé de origen (abierto o cerrado, no expedido) los kilos indicados de una partida, o todo su
    /// contenido, y los pone en el palé de destino (uno abierto o uno nuevo). La partida no cambia, así que la traza sigue
    /// entera, y el repaletizado queda registrado con sus aristas palé → palé.
    /// </summary>
    public async Task<Resultado<RepaletizadoDto>> RepaletizarAsync(Guid empresaId, DatosRepaletizado d, Guid? usuarioId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.Lineas is null || d.Lineas.Count == 0)
        {
            return Resultado.Fallo<RepaletizadoDto>(Error.Validacion("repaletizado.lineas", "Indica los palés de origen."));
        }

        Pale destino;
        if (d.DestinoPaleId is { } did)
        {
            var existente = await _repo.PaleAsync(did, ct).ConfigureAwait(false);
            if (existente is null || existente.Estado != EstadoPale.Abierto)
            {
                return Resultado.Fallo<RepaletizadoDto>(Error.Conflicto("repaletizado.destino", "El palé de destino tiene que existir y estar abierto."));
            }

            destino = existente;
        }
        else
        {
            var nuevo = await PalesAgro.NuevosPalesAsync(_repo, _unidad, _reloj, empresaId, 1, d.TipoPale, null, ct).ConfigureAwait(false);
            if (nuevo.EsFallo)
            {
                return Resultado.Fallo<RepaletizadoDto>(nuevo.Error);
            }

            destino = nuevo.Valor[0];
        }

        var lineas = new List<(Guid OrigenPaleId, Guid PartidaId, decimal Kilos, int Cajas)>();
        foreach (var l in d.Lineas)
        {
            var origen = await _repo.PaleAsync(l.OrigenPaleId, ct).ConfigureAwait(false);
            if (origen is null || origen.Estado == EstadoPale.Expedido || origen.Id == destino.Id)
            {
                return Resultado.Fallo<RepaletizadoDto>(Error.Conflicto("repaletizado.origen", "Un palé de origen no existe, ya salió o es el de destino."));
            }

            var contenido = (await _repo.ContenidoPaleAsync(origen.Id, ct).ConfigureAwait(false)).Where(c => c.Kilos > 0m && (l.PartidaId is null || c.PartidaId == l.PartidaId)).ToList();
            if (contenido.Count == 0)
            {
                return Resultado.Fallo<RepaletizadoDto>(Error.Conflicto("repaletizado.vacio", $"El palé {origen.Sscc} no lleva {(l.PartidaId is null ? "nada" : "esa partida")}."));
            }

            if (l.Kilos is { } kilos)
            {
                if (l.PartidaId is null || contenido.Count != 1 || kilos <= 0m || kilos > contenido[0].Kilos || decimal.Round(kilos, 3) != kilos)
                {
                    return Resultado.Fallo<RepaletizadoDto>(Error.Validacion("repaletizado.kilos",
                        $"Para pasar parte de un palé indica la partida y hasta {Redondeo.Formatear(contenido.Sum(c => c.Kilos), 3)} kg (3 decimales)."));
                }

                var cajas = contenido[0].Cajas > 0 && kilos == contenido[0].Kilos ? contenido[0].Cajas : 0;
                lineas.Add((origen.Id, contenido[0].PartidaId, kilos, cajas));
            }
            else
            {
                lineas.AddRange(contenido.Select(c => (origen.Id, c.PartidaId, c.Kilos, Math.Max(0, c.Cajas))));
            }
        }

        var fecha = d.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var rep = Repaletizado.Crear(empresaId, fecha, destino.Id, lineas, d.Motivo, usuarioId, _reloj.AhoraUtc);
        if (rep.EsFallo)
        {
            return Resultado.Fallo<RepaletizadoDto>(rep.Error);
        }

        foreach (var l in rep.Valor.Lineas)
        {
            _repo.Agregar(MovimientoPartida.Crear(empresaId, l.PartidaId, fecha, TipoMovimientoPartida.Paletizado, -l.Kilos, l.OrigenPaleId, "Repaletizado", rep.Valor.Id,
                "Repaletizado", _reloj, -l.Cajas).Valor);
            _repo.Agregar(MovimientoPartida.Crear(empresaId, l.PartidaId, fecha, TipoMovimientoPartida.Paletizado, l.Kilos, destino.Id, "Repaletizado", rep.Valor.Id,
                "Repaletizado", _reloj, l.Cajas).Valor);
        }

        _repo.Agregar(rep.Valor);
        if (d.CerrarDestino)
        {
            destino.Cerrar();
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(rep.Valor, destino.Sscc));
    }

    public async Task<IReadOnlyList<RepaletizadoDto>> RepaletizadosAsync(Guid empresaId, Guid? paleId, CancellationToken ct = default)
    {
        var lista = await _repo.RepaletizadosAsync(empresaId, paleId, ct).ConfigureAwait(false);
        var pales = (await _repo.PalesAsync(lista.Select(r => r.DestinoPaleId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id, p => p.Sscc);
        return lista.Select(r => Dto(r, pales.GetValueOrDefault(r.DestinoPaleId))).ToList();
    }

    private static RepaletizadoDto Dto(Repaletizado r, string? sscc) => new(r.Id, r.Fecha, r.DestinoPaleId, sscc, r.Kilos, r.Motivo, r.UsuarioId,
        r.Lineas.Select(l => new LineaRepaletizadoDto(l.OrigenPaleId, l.PartidaId, l.Kilos, l.Cajas)).ToList());

    private static ReglaTransformacionDto Dto(ReglaTransformacion r) => new(r.Id, r.ProductoOrigenId, r.ProductoDestinoId, r.MermaMaximaPct);

    private static Error NoExiste() => Error.NoEncontrado("regla.no_encontrada", "La regla de transformación no existe.");
}
