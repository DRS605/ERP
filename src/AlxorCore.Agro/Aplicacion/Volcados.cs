using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioVolcados
{
    Task<IReadOnlyList<VolcadoPalot>> ListarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, Guid? lineaId, CancellationToken ct = default);

    Task<VolcadoPalot?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<VolcadoPalot?> PorClaveAsync(Guid empresaId, string clave, CancellationToken ct = default);

    /// <summary>Volcados vivos de un palé: pendientes o en un parte.</summary>
    Task<IReadOnlyList<VolcadoPalot>> VivosDelPaleAsync(Guid paleId, CancellationToken ct = default);

    Task<IReadOnlyList<VolcadoPalot>> DelParteAsync(Guid parteId, CancellationToken ct = default);

    void Agregar(VolcadoPalot volcado);
}

/// <summary>Una lectura del terminal: la clave la pone el terminal y hace que reenviarla no la duplique.</summary>
public sealed record DatosVolcado(string? Clave, string? Sscc, Guid LineaId, Guid? OrdenId, DateTimeOffset? VolcadoEn);

/// <summary>Cómo acabó una lectura: «registrado», «ya_registrado» (reenvío de la misma clave) o «error» con su código.</summary>
public sealed record ResultadoVolcadoDto(string? Clave, string Estado, VolcadoDto? Volcado, string? Codigo, string? Mensaje);

public sealed record VolcadoDto(Guid Id, string Clave, Guid LineaId, string? Linea, Guid? OrdenLineaId, Guid PaleId, string Sscc, decimal Kilos, Guid? ProductoId,
    string? Producto, string? Agricultor, DateTimeOffset VolcadoEn, DateOnly Fecha, DateTimeOffset RegistradoEn, string Terminal, string Estado, Guid? ParteConfeccionId,
    string? MotivoAnulacion);

public sealed record DatosParteVolcados(Guid LineaId, DateOnly Fecha, Guid? OrdenId = null);

/// <summary>
/// Volcados de palots en las líneas de la planta: los registra (uno a uno o en lote, desde la cola del terminal sin
/// conexión), los anula y los pasa a un parte de confección en borrador con el consumo de cada palé.
/// </summary>
public sealed class VolcadosPlanta
{
    /// <summary>Máximo de lecturas por envío del terminal.</summary>
    public const int MaximoLote = 500;

    private readonly IRepositorioVolcados _volcados;
    private readonly IRepositorioPlanta _planta;
    private readonly IRepositorioAgro _repo;
    private readonly PalesAgro _pales;
    private readonly ConfeccionAgro _confeccion;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos _productos;

    public VolcadosPlanta(IRepositorioVolcados volcados, IRepositorioPlanta planta, IRepositorioAgro repo, PalesAgro pales, ConfeccionAgro confeccion,
        IUnidadDeTrabajoAgro unidad, IReloj reloj, AlxorCore.Catalogo.Aplicacion.IConsultaProductos productos)
    {
        _volcados = volcados; _planta = planta; _repo = repo; _pales = pales; _confeccion = confeccion; _unidad = unidad; _reloj = reloj; _productos = productos;
    }

    public async Task<IReadOnlyList<VolcadoDto>> ListarAsync(Guid empresaId, DateOnly desde, DateOnly hasta, Guid? lineaId, CancellationToken ct = default)
    {
        var lista = await _volcados.ListarAsync(empresaId, desde, hasta, lineaId, ct).ConfigureAwait(false);
        await LiberarHuerfanosAsync(lista, ct).ConfigureAwait(false);
        return await DtosAsync(empresaId, lista, ct).ConfigureAwait(false);
    }

    /// <summary>Registra un lote de lecturas; cada una se guarda (o falla) por su cuenta, en el orden en que se leyeron.</summary>
    public async Task<Resultado<IReadOnlyList<ResultadoVolcadoDto>>> RegistrarAsync(Guid empresaId, IReadOnlyList<DatosVolcado>? lecturas, string terminal,
        CancellationToken ct = default)
    {
        if (lecturas is null || lecturas.Count == 0 || lecturas.Count > MaximoLote)
        {
            return Resultado.Fallo<IReadOnlyList<ResultadoVolcadoDto>>(Error.Validacion("volcado.lote", $"Envía entre 1 y {MaximoLote} lecturas."));
        }

        var resultados = new List<ResultadoVolcadoDto>();
        foreach (var l in lecturas.OrderBy(l => l.VolcadoEn ?? DateTimeOffset.MaxValue))
        {
            resultados.Add(await RegistrarUnoAsync(empresaId, l, terminal, ct).ConfigureAwait(false));
        }

        return Resultado.Ok<IReadOnlyList<ResultadoVolcadoDto>>(resultados);
    }

    private async Task<ResultadoVolcadoDto> RegistrarUnoAsync(Guid empresaId, DatosVolcado l, string terminal, CancellationToken ct)
    {
        ResultadoVolcadoDto Fallo(Error e) => new(l.Clave, "error", null, e.Codigo, e.Mensaje);

        if (!string.IsNullOrWhiteSpace(l.Clave) && await _volcados.PorClaveAsync(empresaId, l.Clave.Trim(), ct).ConfigureAwait(false) is { } previo)
        {
            return new ResultadoVolcadoDto(l.Clave, "ya_registrado", (await DtosAsync(empresaId, [previo], ct).ConfigureAwait(false))[0], null, null);
        }

        var sscc = EtiquetaCampo.SsccDeLectura(l.Sscc);
        if (sscc is null)
        {
            return Fallo(Error.Validacion("volcado.lectura", $"«{l.Sscc}» no es la etiqueta SSCC de un palé."));
        }

        var linea = await _planta.LineaAsync(l.LineaId, ct).ConfigureAwait(false);
        if (linea is null || linea.EmpresaId != empresaId)
        {
            return Fallo(Error.NoEncontrado("volcado.linea", "La línea no existe."));
        }

        OrdenLinea? orden = null;
        if (l.OrdenId is { } ordenId && (orden = await _planta.OrdenAsync(ordenId, ct).ConfigureAwait(false)) is null)
        {
            return Fallo(Error.NoEncontrado("volcado.orden", "La orden no existe."));
        }

        var pale = await _pales.ObtenerAsync(empresaId, sscc, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return Fallo(Error.NoEncontrado("volcado.pale_desconocido", $"No hay ningún palé con el SSCC {sscc}."));
        }

        if (pale.Estado == nameof(EstadoPale.Expedido))
        {
            return Fallo(Error.Conflicto("volcado.pale_expedido", $"El palé {sscc} ya se expidió."));
        }

        var kilos = pale.Contenido.Sum(c => c.Kilos);
        if (kilos <= 0m)
        {
            return Fallo(Error.Conflicto("volcado.pale_vacio", $"El palé {sscc} no lleva kilos en ninguna partida."));
        }

        // Un palé ya volcado (pendiente o en un parte vivo) no se vuelve a volcar.
        var previos = await _volcados.VivosDelPaleAsync(pale.Id, ct).ConfigureAwait(false);
        await LiberarHuerfanosAsync(previos, ct).ConfigureAwait(false);
        if (previos.Count > 0 && previos[0] is var otro)
        {
            return Fallo(Error.Conflicto("volcado.repetido", $"El palé {sscc} ya se volcó el {otro.VolcadoEn:dd/MM HH:mm} ({otro.Terminal})."));
        }

        var v = VolcadoPalot.Registrar(empresaId, l.Clave, linea, orden, pale.Id, sscc, kilos, l.VolcadoEn ?? _reloj.AhoraUtc, terminal, _reloj);
        if (v.EsFallo)
        {
            return Fallo(v.Error);
        }

        _volcados.Agregar(v.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return new ResultadoVolcadoDto(l.Clave, "registrado", (await DtosAsync(empresaId, [v.Valor], ct).ConfigureAwait(false))[0], null, null);
    }

    public async Task<Resultado> AnularAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var v = await _volcados.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (v is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("volcado.no_encontrado", "El volcado no existe."));
        }

        await LiberarHuerfanosAsync([v], ct).ConfigureAwait(false);
        var r = v.Anular(motivo);
        if (r.EsFallo)
        {
            return r;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Parte de confección en borrador con el consumo de los palés volcados (pendientes) de una línea en un día y, si se
    /// indica, de una orden: consume lo que lleve cada palé, por partida. Las salidas y la mano de obra se completan en el parte.
    /// </summary>
    public async Task<Resultado<ParteDto>> GenerarParteAsync(Guid empresaId, DatosParteVolcados d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var linea = await _planta.LineaAsync(d.LineaId, ct).ConfigureAwait(false);
        if (linea is null)
        {
            return Resultado.Fallo<ParteDto>(Error.NoEncontrado("volcado.linea", "La línea no existe."));
        }

        var lista = (await _volcados.ListarAsync(empresaId, d.Fecha, d.Fecha, d.LineaId, ct).ConfigureAwait(false)).ToList();
        await LiberarHuerfanosAsync(lista, ct).ConfigureAwait(false);
        var pendientes = lista.Where(v => v.Estado == EstadoVolcado.Registrado && (d.OrdenId is null || v.OrdenLineaId == d.OrdenId)).OrderBy(v => v.VolcadoEn).ToList();
        if (pendientes.Count == 0)
        {
            return Resultado.Fallo<ParteDto>(Error.Validacion("volcado.sin_pendientes", "No hay volcados pendientes de esa línea y día."));
        }

        var consumos = new List<EntradaConsumo>();
        foreach (var v in pendientes)
        {
            var pale = await _pales.ObtenerAsync(empresaId, v.PaleId.ToString(), ct).ConfigureAwait(false);
            consumos.AddRange((pale?.Contenido ?? []).Where(c => c.Kilos > 0m).Select(c => new EntradaConsumo(c.PartidaId, c.Kilos, v.PaleId)));
        }

        if (consumos.Count == 0)
        {
            return Resultado.Fallo<ParteDto>(Error.Conflicto("volcado.sin_kilos", "Los palés volcados ya no llevan kilos: se consumieron en otro parte."));
        }

        var orden = d.OrdenId is { } o ? await _planta.OrdenAsync(o, ct).ConfigureAwait(false) : null;
        var parte = await _confeccion.CrearAsync(empresaId, new DatosParteConfeccion(d.Fecha,
            Descripcion: $"Volcados {linea.Codigo} {d.Fecha:dd/MM/yyyy}{(orden is null ? string.Empty : $" · orden {orden.Secuencia}")} ({pendientes.Count} palés)",
            CentroAnaliticoId: linea.CentroAnaliticoId, Consumos: consumos), ct).ConfigureAwait(false);
        if (parte.EsFallo)
        {
            return parte;
        }

        foreach (var v in pendientes)
        {
            v.PasarAParte(parte.Valor.Id);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return parte;
    }

    /// <summary>Los volcados de un parte que se borró o se anuló vuelven a quedar pendientes.</summary>
    private async Task LiberarHuerfanosAsync(IReadOnlyList<VolcadoPalot> lista, CancellationToken ct)
    {
        var cambios = false;
        foreach (var grupo in lista.Where(v => v.Estado == EstadoVolcado.EnParte).GroupBy(v => v.ParteConfeccionId!.Value))
        {
            var parte = await _repo.ParteAsync(grupo.Key, ct).ConfigureAwait(false);
            if (parte is null || parte.Estado == EstadoParte.Anulado)
            {
                foreach (var v in grupo)
                {
                    v.Liberar();
                }

                cambios = true;
            }
        }

        if (cambios)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
    }

    private async Task<IReadOnlyList<VolcadoDto>> DtosAsync(Guid empresaId, IReadOnlyList<VolcadoPalot> lista, CancellationToken ct)
    {
        var lineas = (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(l => l.Id, l => l.Codigo);
        var resultado = new List<VolcadoDto>();
        var cache = new Dictionary<Guid, (Guid? Producto, string? NombreProducto, string? Agricultor)>();
        foreach (var v in lista.OrderByDescending(v => v.VolcadoEn))
        {
            if (!cache.TryGetValue(v.PaleId, out var info))
            {
                info = await OrigenAsync(v.PaleId, ct).ConfigureAwait(false);
                cache[v.PaleId] = info;
            }

            resultado.Add(new VolcadoDto(v.Id, v.ClaveTerminal, v.LineaId, lineas.GetValueOrDefault(v.LineaId), v.OrdenLineaId, v.PaleId, v.Sscc, v.Kilos, info.Producto,
                info.NombreProducto, info.Agricultor, v.VolcadoEn, v.Fecha, v.RegistradoEn, v.Terminal, v.Estado.ToString(), v.ParteConfeccionId, v.MotivoAnulacion));
        }

        return resultado;
    }

    /// <summary>Producto y agricultor del palé: los de la partida con más kilos que llevó (aunque ya esté consumido).</summary>
    private async Task<(Guid? Producto, string? NombreProducto, string? Agricultor)> OrigenAsync(Guid paleId, CancellationToken ct)
    {
        var partidaId = (await _repo.MovimientosDePaleAsync(paleId, ct).ConfigureAwait(false)).Where(m => m.Kilos > 0m).GroupBy(m => m.PartidaId)
            .OrderByDescending(g => g.Sum(m => m.Kilos)).Select(g => (Guid?)g.Key).FirstOrDefault();
        var partida = partidaId is { } p ? await _repo.PartidaAsync(p, ct).ConfigureAwait(false) : null;
        if (partida is null)
        {
            return (null, null, null);
        }

        var agricultor = partida.AgricultorId is { } a ? await _repo.AgricultorAsync(a, ct).ConfigureAwait(false) : null;
        return (partida.ProductoId, (await _productos.ObtenerAsync(partida.ProductoId, ct).ConfigureAwait(false))?.Nombre, agricultor?.Nombre);
    }
}
