using System.Globalization;
using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioPlanta
{
    Task<IReadOnlyList<LineaPlanta>> LineasAsync(Guid empresaId, CancellationToken ct = default);

    Task<LineaPlanta?> LineaAsync(Guid id, CancellationToken ct = default);

    Task<bool> LineaEnUsoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Calibrado>> CalibradosAsync(Guid empresaId, Guid? partidaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);

    Task<Calibrado?> CalibradoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<OrdenLinea>> OrdenesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<OrdenLinea?> OrdenAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ParadaLinea>> ParadasAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<ParadaLinea?> ParadaAsync(Guid id, CancellationToken ct = default);

    void Agregar(object entidad);

    void Eliminar(object entidad);
}

public sealed record SalidaCalibradoraDto(int Numero, string? Calibre, Guid? CategoriaId, string? Categoria, bool Destrio);

public sealed record LineaPlantaDto(Guid Id, string Codigo, string Nombre, string Tipo, decimal CapacidadKgHora, decimal HorasTurno, int Turnos, decimal CapacidadDia,
    Guid? CentroAnaliticoId, bool Activa, IReadOnlyList<SalidaCalibradoraDto> Salidas);

public sealed record LineaCalibradoDto(string? Calibre, Guid? CategoriaId, string? Categoria, bool Destrio, decimal Kilos, decimal Porcentaje, int? Piezas, decimal? GramosPieza);

public sealed record CalibradoDto(Guid Id, Guid LineaId, string Linea, Guid PartidaId, string Partida, Guid ProductoId, Guid? AgricultorId, DateOnly Fecha, decimal KilosEntrada,
    decimal KilosSalida, decimal Merma, string Estado, string? Referencia, Guid? ClasificacionId, string? MotivoAnulacion, IReadOnlyList<LineaCalibradoDto> Lineas);

/// <summary>Kilos de un calibre y categoría en el periodo, sobre el total calibrado, con el peso medio del fruto.</summary>
public sealed record CalibreResumenDto(string? Calibre, string? Categoria, bool Destrio, decimal Kilos, decimal Porcentaje, decimal? GramosPieza);

public sealed record ResumenCalibresDto(DateOnly Desde, DateOnly Hasta, Guid? ProductoId, Guid? AgricultorId, int Calibrados, decimal KilosEntrada, decimal KilosSalida,
    IReadOnlyList<CalibreResumenDto> Calibres);

public sealed record OrdenLineaDto(Guid Id, Guid LineaId, string Linea, DateOnly Fecha, int Turno, int Secuencia, Guid ProductoId, decimal Kilos, int? Cajas, decimal HorasPrevistas,
    Guid? ClienteId, Guid? PedidoVentaId, Guid? LineaPlanId, string? Notas, string Estado, Guid? ParteConfeccionId, decimal? KilosReales);

public sealed record ParadaDto(Guid Id, Guid LineaId, string Linea, DateOnly Fecha, int Turno, decimal Minutos, string Motivo, string? Notas);

/// <summary>
/// Carga de una línea un día: la capacidad (menos las paradas), lo planificado y su ocupación, y lo real de las órdenes
/// terminadas con su parte. La disponibilidad es el tiempo sin paradas; el rendimiento, lo real frente a lo que la línea
/// podía hacer en ese tiempo.
/// </summary>
public sealed record CargaDiaDto(Guid LineaId, string Linea, DateOnly Fecha, decimal HorasTurnos, decimal MinutosParada, decimal Capacidad, decimal Planificado, decimal HorasPlanificadas,
    decimal? Ocupacion, bool Sobrecarga, decimal Real, decimal? Disponibilidad, decimal? Rendimiento, int Ordenes, int Pendientes);

public sealed record CargaPlantaDto(DateOnly Desde, DateOnly Hasta, IReadOnlyList<CargaDiaDto> Dias, decimal Capacidad, decimal Planificado, decimal Real);

public sealed record DatosConfirmarCalibrado(bool Clasificar = true, bool Definitiva = false);

public sealed record DatosOrdenesDesdePlan(DateOnly Desde, DateOnly Hasta, Guid? PlanId = null, Guid? CampanaId = null);

/// <summary>
/// Planta de confección: las líneas (calibradoras, confección y envasado) con su capacidad y sus turnos; los calibrados
/// de cada partida, que pueden pasar a su clasificación; las órdenes de trabajo por línea, día y turno, con la carga frente
/// a la capacidad; y las paradas.
/// </summary>
public sealed class PlantaAgro
{
    private readonly IRepositorioPlanta _planta;
    private readonly IRepositorioAgro _repo;
    private readonly LiquidacionesAgro _liquidaciones;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    public PlantaAgro(IRepositorioPlanta planta, IRepositorioAgro repo, LiquidacionesAgro liquidaciones, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _planta = planta; _repo = repo; _liquidaciones = liquidaciones; _unidad = unidad; _reloj = reloj;
    }

    // ------------------------------------------------------------------ Líneas
    public async Task<IReadOnlyList<LineaPlantaDto>> LineasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var categorias = await CategoriasAsync(empresaId, ct).ConfigureAwait(false);
        return (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(l => l.Activa).ThenBy(l => l.Tipo).ThenBy(l => l.Codigo, StringComparer.Ordinal)
            .Select(l => Dto(l, categorias)).ToList();
    }

    public async Task<Resultado<LineaPlantaDto>> CrearLineaAsync(Guid empresaId, DatosLineaPlanta d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var l = LineaPlanta.Crear(empresaId, d);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LineaPlantaDto>(l.Error);
        }

        if (await ComprobarLineaAsync(empresaId, l.Valor, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<LineaPlantaDto>(error);
        }

        _planta.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l.Valor, await CategoriasAsync(empresaId, ct).ConfigureAwait(false)));
    }

    public async Task<Resultado<LineaPlantaDto>> CambiarLineaAsync(Guid empresaId, Guid id, DatosLineaPlanta d, bool? activa, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await LineaDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return Resultado.Fallo<LineaPlantaDto>(LineaNoExiste());
        }

        if (l.Tipo != d.Tipo && await _planta.LineaEnUsoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<LineaPlantaDto>(Error.Conflicto("linea_planta.en_uso", "La línea ya tiene calibrados u órdenes: no cambia de tipo."));
        }

        var r = l.Cambiar(d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LineaPlantaDto>(r.Error);
        }

        if (activa is { } a)
        {
            l.FijarActiva(a);
        }

        if (await ComprobarLineaAsync(empresaId, l, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<LineaPlantaDto>(error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l, await CategoriasAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>Elimina una línea sin uso; la que tiene calibrados, órdenes o paradas se da de baja.</summary>
    public async Task<Resultado> EliminarLineaAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await LineaDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return Resultado.Fallo(LineaNoExiste());
        }

        if (await _planta.LineaEnUsoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("linea_planta.en_uso", "La línea tiene calibrados, órdenes o paradas: dala de baja."));
        }

        _planta.Eliminar(l);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Calibrados
    public async Task<IReadOnlyList<CalibradoDto>> CalibradosAsync(Guid empresaId, Guid? partidaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var lista = await _planta.CalibradosAsync(empresaId, partidaId, desde, hasta, ct).ConfigureAwait(false);
        return await DtosAsync(empresaId, lista, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<CalibradoDto>> CrearCalibradoAsync(Guid empresaId, Guid lineaId, Guid partidaId, DatosCalibrado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await LineaDeAsync(empresaId, lineaId, ct).ConfigureAwait(false) is not { } linea)
        {
            return Resultado.Fallo<CalibradoDto>(LineaNoExiste());
        }

        if (await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false) is not { } partida || partida.EmpresaId != empresaId || partida.Anulada)
        {
            return Resultado.Fallo<CalibradoDto>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        if (d.Fecha < partida.Fecha)
        {
            return Resultado.Fallo<CalibradoDto>(Error.Validacion("calibrado.fecha", $"La partida entró el {partida.Fecha:dd/MM/yyyy}: no se calibra antes."));
        }

        if (await ComprobarCategoriasAsync(empresaId, d, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<CalibradoDto>(error);
        }

        var c = Calibrado.Crear(empresaId, linea, partidaId, partida.KilosIniciales, d, _reloj.AhoraUtc);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CalibradoDto>(c.Error);
        }

        _planta.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(empresaId, [c.Valor], ct).ConfigureAwait(false))[0]);
    }

    public async Task<Resultado<CalibradoDto>> CambiarCalibradoAsync(Guid empresaId, Guid id, DatosCalibrado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await CalibradoDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } c)
        {
            return Resultado.Fallo<CalibradoDto>(CalibradoNoExiste());
        }

        var linea = (await _planta.LineaAsync(c.LineaId, ct).ConfigureAwait(false))!;
        var partida = (await _repo.PartidaAsync(c.PartidaId, ct).ConfigureAwait(false))!;
        if (await ComprobarCategoriasAsync(empresaId, d, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<CalibradoDto>(error);
        }

        var r = c.Cambiar(linea, partida.KilosIniciales, d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CalibradoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(empresaId, [c], ct).ConfigureAwait(false))[0]);
    }

    /// <summary>
    /// Confirma el calibrado y, si se pide, lo pasa a la clasificación de la partida (kilos por categoría como muestra),
    /// provisional o definitiva. Así el agricultor se liquida por lo que dio la calibradora.
    /// </summary>
    public async Task<Resultado<CalibradoDto>> ConfirmarCalibradoAsync(Guid empresaId, Guid id, DatosConfirmarCalibrado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await CalibradoDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } c)
        {
            return Resultado.Fallo<CalibradoDto>(CalibradoNoExiste());
        }

        Guid? clasificacion = null;
        if (d.Clasificar)
        {
            var muestra = c.PorCategoria();
            if (muestra.Count == 0)
            {
                return Resultado.Fallo<CalibradoDto>(Error.Validacion("calibrado.sin_categorias",
                    "Ninguna salida tiene categoría: no hay clasificación que pasar. Confírmalo sin clasificar o asigna categorías."));
            }

            var r = await _liquidaciones.ClasificarAsync(empresaId, c.PartidaId, new DatosClasificacion(
                muestra.Select(m => new DatosLineaClasificacion(m.CategoriaId, m.Kilos)).ToList(), d.Definitiva, c.Fecha,
                $"Calibrado{(c.Referencia is null ? string.Empty : " " + c.Referencia)}"), ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<CalibradoDto>(r.Error);
            }

            clasificacion = r.Valor.Id;
        }

        var ok = c.Confirmar(clasificacion);
        if (ok.EsFallo)
        {
            return Resultado.Fallo<CalibradoDto>(ok.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(empresaId, [c], ct).ConfigureAwait(false))[0]);
    }

    /// <summary>Anula el calibrado. La clasificación que generó no se toca: se sustituye desde la clasificación de la partida.</summary>
    public async Task<Resultado<CalibradoDto>> AnularCalibradoAsync(Guid empresaId, Guid id, string? motivo, CancellationToken ct = default)
    {
        if (await CalibradoDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } c)
        {
            return Resultado.Fallo<CalibradoDto>(CalibradoNoExiste());
        }

        var r = c.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CalibradoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await DtosAsync(empresaId, [c], ct).ConfigureAwait(false))[0]);
    }

    /// <summary>
    /// Lee el fichero de resultados de la calibradora (CSV con «;» o «,»): una fila por salida con su número (o su calibre y
    /// categoría), los kilos y, si los trae, las piezas. La cabecera dice qué es cada columna (salida/canal, calibre,
    /// categoría, kilos/peso, piezas/frutos); sin cabecera se lee «salida;kilos;piezas».
    /// </summary>
    public async Task<Resultado<IReadOnlyList<DatosLineaCalibrado>>> LeerFicheroAsync(Guid empresaId, string? contenido, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contenido))
        {
            return Resultado.Fallo<IReadOnlyList<DatosLineaCalibrado>>(Error.Validacion("calibrado.fichero", "El fichero está vacío."));
        }

        var categorias = (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false))
            .GroupBy(c => c.Nombre.Trim().ToUpperInvariant()).ToDictionary(g => g.Key, g => g.First().Id);
        var filas = contenido.Replace("\r", string.Empty, StringComparison.Ordinal).Split('\n').Where(f => f.Trim().Length > 0).ToList();
        var sep = filas[0].Contains(';', StringComparison.Ordinal) ? ';' : ',';
        string[] Celdas(string f) => f.Split(sep).Select(x => x.Trim().Trim('"')).ToArray();

        var cabecera = Celdas(filas[0]).Select(x => x.ToUpperInvariant()).ToArray();
        int Col(params string[] nombres) => Array.FindIndex(cabecera, h => nombres.Any(n => h.StartsWith(n, StringComparison.Ordinal)));
        var tieneCabecera = cabecera.Any(h => h.Length > 0 && !char.IsDigit(h[0]));
        var (cSalida, cCalibre, cCategoria, cKilos, cPiezas) = tieneCabecera
            ? (Col("SALIDA", "CANAL", "SAL"), Col("CALIBRE"), Col("CATEG"), Col("KILOS", "KG", "PESO"), Col("PIEZAS", "FRUTOS", "UNID"))
            : (0, -1, -1, 1, 2);
        if (cKilos < 0 || (cSalida < 0 && cCalibre < 0 && cCategoria < 0))
        {
            return Resultado.Fallo<IReadOnlyList<DatosLineaCalibrado>>(Error.Validacion("calibrado.fichero",
                "El fichero necesita una columna de kilos y otra de salida, calibre o categoría."));
        }

        var lineas = new List<DatosLineaCalibrado>();
        var n = 0;
        foreach (var fila in filas.Skip(tieneCabecera ? 1 : 0))
        {
            n++;
            var c = Celdas(fila);
            string? Celda(int i) => i >= 0 && i < c.Length && c[i].Length > 0 ? c[i] : null;
            if (!decimal.TryParse(Celda(cKilos)?.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out var kilos))
            {
                return Resultado.Fallo<IReadOnlyList<DatosLineaCalibrado>>(Error.Validacion("calibrado.fichero", $"Fila {n}: los kilos no son un número."));
            }

            int? salida = int.TryParse(Celda(cSalida), NumberStyles.Integer, CultureInfo.InvariantCulture, out var s) ? s : null;
            int? piezas = int.TryParse(Celda(cPiezas), NumberStyles.Integer, CultureInfo.InvariantCulture, out var p) ? p : null;
            Guid? categoria = null;
            if (Celda(cCategoria) is { } nombre)
            {
                if (!categorias.TryGetValue(nombre.ToUpperInvariant(), out var cat))
                {
                    return Resultado.Fallo<IReadOnlyList<DatosLineaCalibrado>>(Error.Validacion("calibrado.fichero", $"Fila {n}: la categoría «{nombre}» no existe."));
                }

                categoria = cat;
            }

            if (kilos != 0m)
            {
                lineas.Add(new DatosLineaCalibrado(kilos, salida, Celda(cCalibre), categoria, Piezas: piezas));
            }
        }

        return lineas.Count == 0
            ? Resultado.Fallo<IReadOnlyList<DatosLineaCalibrado>>(Error.Validacion("calibrado.fichero", "El fichero no tiene ninguna salida con kilos."))
            : Resultado.Ok<IReadOnlyList<DatosLineaCalibrado>>(lineas);
    }

    /// <summary>Reparto por calibre y categoría de lo calibrado en un periodo (confirmado), de un producto o un agricultor.</summary>
    public async Task<ResumenCalibresDto> ResumenCalibresAsync(Guid empresaId, DateOnly desde, DateOnly hasta, Guid? productoId, Guid? agricultorId, CancellationToken ct = default)
    {
        var lista = (await _planta.CalibradosAsync(empresaId, null, desde, hasta, ct).ConfigureAwait(false)).Where(c => c.Estado == EstadoCalibrado.Confirmado).ToList();
        var partidas = (await _repo.PartidasAsync(lista.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        lista = lista.Where(c => partidas.TryGetValue(c.PartidaId, out var p) && (productoId is null || p.ProductoId == productoId) && (agricultorId is null || p.AgricultorId == agricultorId))
            .ToList();
        var categorias = await CategoriasAsync(empresaId, ct).ConfigureAwait(false);
        var salida = lista.Sum(c => c.KilosSalida);
        var calibres = lista.SelectMany(c => c.Lineas).GroupBy(l => (l.Calibre, l.CategoriaId, l.Destrio))
            .Select(g =>
            {
                var kilos = g.Sum(l => l.Kilos);
                var piezas = g.Where(l => l.Piezas is > 0).ToList();
                return new CalibreResumenDto(g.Key.Calibre, g.Key.CategoriaId is { } id ? categorias.GetValueOrDefault(id) : null, g.Key.Destrio, Redondeo.Dos(kilos),
                    Pct(kilos, salida) ?? 0m, piezas.Count > 0 ? Math.Round(piezas.Sum(l => l.Kilos) * 1000m / piezas.Sum(l => l.Piezas!.Value), 1, MidpointRounding.AwayFromZero) : null);
            })
            .OrderBy(c => c.Destrio).ThenBy(c => c.Calibre, StringComparer.Ordinal).ThenBy(c => c.Categoria, StringComparer.Ordinal).ToList();
        return new ResumenCalibresDto(desde, hasta, productoId, agricultorId, lista.Count, Redondeo.Dos(lista.Sum(c => c.KilosEntrada)), Redondeo.Dos(salida), calibres);
    }

    // ------------------------------------------------------------------ Órdenes
    public async Task<IReadOnlyList<OrdenLineaDto>> OrdenesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, Guid? lineaId, CancellationToken ct = default)
    {
        var lineas = (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(l => l.Id);
        var ordenes = (await _planta.OrdenesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)).Where(o => lineaId is null || o.LineaId == lineaId).ToList();
        var reales = await RealesAsync(ordenes, ct).ConfigureAwait(false);
        return ordenes.OrderBy(o => o.Fecha).ThenBy(o => lineas.GetValueOrDefault(o.LineaId)?.Codigo, StringComparer.Ordinal).ThenBy(o => o.Turno).ThenBy(o => o.Secuencia)
            .Select(o => Dto(o, lineas.GetValueOrDefault(o.LineaId), reales)).ToList();
    }

    public async Task<Resultado<OrdenLineaDto>> CrearOrdenAsync(Guid empresaId, Guid lineaId, DatosOrdenLinea d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await LineaDeAsync(empresaId, lineaId, ct).ConfigureAwait(false) is not { } linea)
        {
            return Resultado.Fallo<OrdenLineaDto>(LineaNoExiste());
        }

        var o = OrdenLinea.Crear(empresaId, linea, d with { Secuencia = d.Secuencia ?? await SiguienteSecuenciaAsync(empresaId, linea.Id, d.Fecha, d.Turno, ct).ConfigureAwait(false) },
            _reloj.AhoraUtc);
        if (o.EsFallo)
        {
            return Resultado.Fallo<OrdenLineaDto>(o.Error);
        }

        _planta.Agregar(o.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(o.Valor, linea, new Dictionary<Guid, decimal>()));
    }

    /// <summary>Cambia o mueve una orden planificada (a otra línea, día, turno o posición).</summary>
    public async Task<Resultado<OrdenLineaDto>> CambiarOrdenAsync(Guid empresaId, Guid id, Guid lineaId, DatosOrdenLinea d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await OrdenDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } o)
        {
            return Resultado.Fallo<OrdenLineaDto>(OrdenNoExiste());
        }

        if (await LineaDeAsync(empresaId, lineaId, ct).ConfigureAwait(false) is not { } linea)
        {
            return Resultado.Fallo<OrdenLineaDto>(LineaNoExiste());
        }

        var r = o.Cambiar(linea, d with { Secuencia = d.Secuencia ?? o.Secuencia });
        if (r.EsFallo)
        {
            return Resultado.Fallo<OrdenLineaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(o, linea, new Dictionary<Guid, decimal>()));
    }

    public Task<Resultado<OrdenLineaDto>> IniciarOrdenAsync(Guid empresaId, Guid id, CancellationToken ct = default) =>
        TransicionAsync(empresaId, id, o => o.Iniciar(), ct);

    public Task<Resultado<OrdenLineaDto>> CancelarOrdenAsync(Guid empresaId, Guid id, CancellationToken ct = default) =>
        TransicionAsync(empresaId, id, o => o.Cancelar(), ct);

    public Task<Resultado<OrdenLineaDto>> ReabrirOrdenAsync(Guid empresaId, Guid id, CancellationToken ct = default) =>
        TransicionAsync(empresaId, id, o => o.Reabrir(), ct);

    /// <summary>Termina la orden y la enlaza con su parte de confección (validado, del mismo día o posterior), que da lo real.</summary>
    public async Task<Resultado<OrdenLineaDto>> TerminarOrdenAsync(Guid empresaId, Guid id, Guid? parteId, CancellationToken ct = default)
    {
        if (await OrdenDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } o)
        {
            return Resultado.Fallo<OrdenLineaDto>(OrdenNoExiste());
        }

        if (parteId is { } p)
        {
            var parte = await _repo.ParteAsync(p, ct).ConfigureAwait(false);
            if (parte is null || parte.EmpresaId != empresaId)
            {
                return Resultado.Fallo<OrdenLineaDto>(Error.NoEncontrado("parte.no_encontrado", "El parte de confección no existe."));
            }

            if (parte.Estado != EstadoParte.Validado)
            {
                return Resultado.Fallo<OrdenLineaDto>(Error.Validacion("orden_linea.parte_no_validado", "El parte de confección no está validado."));
            }

            if (parte.Fecha < o.Fecha)
            {
                return Resultado.Fallo<OrdenLineaDto>(Error.Validacion("orden_linea.parte_fecha", "El parte es anterior a la orden."));
            }

            var otras = (await _planta.OrdenesAsync(empresaId, parte.Fecha.AddDays(-60), parte.Fecha, ct).ConfigureAwait(false)).Where(x => x.ParteConfeccionId == p && x.Id != o.Id);
            if (otras.Any())
            {
                return Resultado.Fallo<OrdenLineaDto>(Error.Conflicto("orden_linea.parte_en_uso", "Ese parte ya cierra otra orden."));
            }
        }

        return await TransicionAsync(empresaId, id, x => x.Terminar(parteId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado> EliminarOrdenAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await OrdenDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } o)
        {
            return Resultado.Fallo(OrdenNoExiste());
        }

        if (o.Estado != EstadoOrdenLinea.Planificada)
        {
            return Resultado.Fallo(Error.Conflicto("orden_linea.no_planificada", "Solo se elimina una orden planificada; la empezada se cancela."));
        }

        _planta.Eliminar(o);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Genera las órdenes de un periodo desde el plan de producción aprobado (o el indicado): cada línea del plan con la
    /// línea de confección de una línea de la planta reparte sus kilos por igual entre sus días. No repite los días que ya
    /// tienen su orden.
    /// </summary>
    public async Task<Resultado<IReadOnlyList<OrdenLineaDto>>> DesdePlanAsync(Guid empresaId, DatosOrdenesDesdePlan d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.Hasta < d.Desde || d.Hasta.DayNumber - d.Desde.DayNumber > 92)
        {
            return Resultado.Fallo<IReadOnlyList<OrdenLineaDto>>(Error.Validacion("orden_linea.periodo", "Indica un periodo de hasta tres meses."));
        }

        var planes = await _repo.PlanesAsync(empresaId, ct).ConfigureAwait(false);
        var plan = d.PlanId is { } pid
            ? planes.FirstOrDefault(p => p.Id == pid)
            : planes.Where(p => p.Tipo == TipoPlan.Produccion && p.Estado == EstadoPlan.Aprobado && (d.CampanaId is null || p.CampanaId == d.CampanaId)).MaxBy(p => p.AprobadoEn);
        if (plan is null || plan.Tipo != TipoPlan.Produccion)
        {
            return Resultado.Fallo<IReadOnlyList<OrdenLineaDto>>(Error.NoEncontrado("plan.no_encontrado", "No hay plan de producción aprobado del que partir."));
        }

        var lineas = (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).Where(l => l.Activa).ToDictionary(l => l.Codigo, StringComparer.OrdinalIgnoreCase);
        var existentes = (await _planta.OrdenesAsync(empresaId, d.Desde, d.Hasta, ct).ConfigureAwait(false))
            .Where(o => o.LineaPlanId is not null).Select(o => (o.LineaPlanId!.Value, o.Fecha)).ToHashSet();
        var nuevas = new List<(OrdenLinea Orden, LineaPlanta Linea)>();
        foreach (var lp in plan.Lineas.Where(x => x.LineaConfeccion is not null))
        {
            if (!lineas.TryGetValue(lp.LineaConfeccion!, out var linea))
            {
                continue;
            }

            var dias = lp.Hasta.DayNumber - lp.Desde.DayNumber + 1;
            for (var f = Max(lp.Desde, d.Desde); f <= Min(lp.Hasta, d.Hasta); f = f.AddDays(1))
            {
                if (existentes.Contains((lp.Id, f)))
                {
                    continue;
                }

                var o = OrdenLinea.Crear(empresaId, linea, new DatosOrdenLinea(f, lp.ProductoId, Redondeo.Dos(lp.Kilos / dias), 1,
                    await SiguienteSecuenciaAsync(empresaId, linea.Id, f, 1, ct).ConfigureAwait(false) + nuevas.Count(n => n.Linea.Id == linea.Id && n.Orden.Fecha == f),
                    lp.Cajas is { } cj ? (int)Math.Round((decimal)cj / dias, MidpointRounding.AwayFromZero) : null, lp.ClienteId, Notas: lp.Notas), _reloj.AhoraUtc, lp.Id);
                if (o.EsFallo)
                {
                    return Resultado.Fallo<IReadOnlyList<OrdenLineaDto>>(o.Error);
                }

                _planta.Agregar(o.Valor);
                nuevas.Add((o.Valor, linea));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var vacio = new Dictionary<Guid, decimal>();
        return Resultado.Ok<IReadOnlyList<OrdenLineaDto>>(nuevas.Select(n => Dto(n.Orden, n.Linea, vacio)).ToList());
    }

    // ------------------------------------------------------------------ Paradas
    public async Task<IReadOnlyList<ParadaDto>> ParadasAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var lineas = (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(l => l.Id);
        return (await _planta.ParadasAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)).OrderBy(p => p.Fecha).ThenBy(p => p.Turno)
            .Select(p => Dto(p, lineas.GetValueOrDefault(p.LineaId))).ToList();
    }

    public async Task<Resultado<ParadaDto>> CrearParadaAsync(Guid empresaId, Guid lineaId, DatosParada d, CancellationToken ct = default)
    {
        if (await LineaDeAsync(empresaId, lineaId, ct).ConfigureAwait(false) is not { } linea)
        {
            return Resultado.Fallo<ParadaDto>(LineaNoExiste());
        }

        var p = ParadaLinea.Crear(empresaId, linea, d);
        if (p.EsFallo)
        {
            return Resultado.Fallo<ParadaDto>(p.Error);
        }

        if (await ExcedeTurnoAsync(empresaId, linea, d, null, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<ParadaDto>(error);
        }

        _planta.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p.Valor, linea));
    }

    public async Task<Resultado<ParadaDto>> CambiarParadaAsync(Guid empresaId, Guid id, DatosParada d, CancellationToken ct = default)
    {
        if (await _planta.ParadaAsync(id, ct).ConfigureAwait(false) is not { } p || p.EmpresaId != empresaId)
        {
            return Resultado.Fallo<ParadaDto>(ParadaNoExiste());
        }

        var linea = (await _planta.LineaAsync(p.LineaId, ct).ConfigureAwait(false))!;
        if (await ExcedeTurnoAsync(empresaId, linea, d, p.Id, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<ParadaDto>(error);
        }

        var r = p.Cambiar(linea, d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ParadaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p, linea));
    }

    public async Task<Resultado> EliminarParadaAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await _planta.ParadaAsync(id, ct).ConfigureAwait(false) is not { } p || p.EmpresaId != empresaId)
        {
            return Resultado.Fallo(ParadaNoExiste());
        }

        _planta.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Carga
    /// <summary>Carga de cada línea activa (o con órdenes) cada día del periodo.</summary>
    public async Task<Resultado<CargaPlantaDto>> CargaAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        if (hasta < desde || hasta.DayNumber - desde.DayNumber > 92)
        {
            return Resultado.Fallo<CargaPlantaDto>(Error.Validacion("carga.periodo", "Indica un periodo de hasta tres meses."));
        }

        var lineas = await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false);
        var ordenes = (await _planta.OrdenesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)).Where(o => o.Estado != EstadoOrdenLinea.Cancelada).ToList();
        var paradas = await _planta.ParadasAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var reales = await RealesAsync(ordenes, ct).ConfigureAwait(false);
        var dias = new List<CargaDiaDto>();
        foreach (var l in lineas.Where(l => l.Tipo != TipoLineaPlanta.Calibradora && (l.Activa || ordenes.Any(o => o.LineaId == l.Id))).OrderBy(l => l.Codigo, StringComparer.Ordinal))
        {
            for (var f = desde; f <= hasta; f = f.AddDays(1))
            {
                var os = ordenes.Where(o => o.LineaId == l.Id && o.Fecha == f).ToList();
                var parada = paradas.Where(p => p.LineaId == l.Id && p.Fecha == f).Sum(p => p.Minutos);
                var horas = l.HorasTurno * l.Turnos;
                var disponibles = horas - (parada / 60m);
                var capacidad = Redondeo.Dos(l.CapacidadKgHora * disponibles);
                var planificado = Redondeo.Dos(os.Sum(o => o.Kilos));
                var real = Redondeo.Dos(os.Sum(o => reales.GetValueOrDefault(o.Id)));
                var terminadas = os.Where(o => o.Estado == EstadoOrdenLinea.Terminada && o.ParteConfeccionId is not null).ToList();
                dias.Add(new CargaDiaDto(l.Id, l.Codigo, f, horas, parada, capacidad, planificado, Math.Round(planificado / l.CapacidadKgHora, 2, MidpointRounding.AwayFromZero),
                    Pct(planificado, capacidad), planificado > capacidad, real, Pct(disponibles, horas),
                    terminadas.Count > 0 && capacidad > 0m ? Pct(real, capacidad) : null, os.Count, os.Count(o => o.Estado is EstadoOrdenLinea.Planificada or EstadoOrdenLinea.EnCurso)));
            }
        }

        return Resultado.Ok(new CargaPlantaDto(desde, hasta, dias, Redondeo.Dos(dias.Sum(x => x.Capacidad)), Redondeo.Dos(dias.Sum(x => x.Planificado)), Redondeo.Dos(dias.Sum(x => x.Real))));
    }

    // ------------------------------------------------------------------ Apoyo
    private async Task<Error?> ComprobarLineaAsync(Guid empresaId, LineaPlanta linea, CancellationToken ct)
    {
        var lineas = await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false);
        if (lineas.Any(x => x.Id != linea.Id && string.Equals(x.Codigo, linea.Codigo, StringComparison.OrdinalIgnoreCase)))
        {
            return Error.Conflicto("linea_planta.codigo_repetido", $"Ya hay una línea {linea.Codigo}.");
        }

        var categorias = await CategoriasAsync(empresaId, ct).ConfigureAwait(false);
        return linea.Salidas.Any(s => s.CategoriaId is { } c && !categorias.ContainsKey(c))
            ? Error.NoEncontrado("categoria.no_encontrada", "Alguna categoría de las salidas no existe.")
            : null;
    }

    private async Task<Error?> ComprobarCategoriasAsync(Guid empresaId, DatosCalibrado d, CancellationToken ct)
    {
        var categorias = await CategoriasAsync(empresaId, ct).ConfigureAwait(false);
        return d.Lineas?.Any(l => l.CategoriaId is { } c && !categorias.ContainsKey(c)) == true
            ? Error.NoEncontrado("categoria.no_encontrada", "Alguna categoría no existe.")
            : null;
    }

    private async Task<Error?> ExcedeTurnoAsync(Guid empresaId, LineaPlanta linea, DatosParada d, Guid? excepto, CancellationToken ct)
    {
        var ya = (await _planta.ParadasAsync(empresaId, d.Fecha, d.Fecha, ct).ConfigureAwait(false))
            .Where(p => p.LineaId == linea.Id && p.Turno == d.Turno && p.Id != excepto).Sum(p => p.Minutos);
        return ya + d.Minutos > linea.HorasTurno * 60m
            ? Error.Validacion("parada.minutos", $"Las paradas del turno {d.Turno} pasarían de su duración ({linea.HorasTurno * 60m:0} minutos).")
            : null;
    }

    private async Task<int> SiguienteSecuenciaAsync(Guid empresaId, Guid lineaId, DateOnly fecha, int turno, CancellationToken ct) =>
        (await _planta.OrdenesAsync(empresaId, fecha, fecha, ct).ConfigureAwait(false)).Where(o => o.LineaId == lineaId && o.Turno == turno)
        .Select(o => o.Secuencia + 1).DefaultIfEmpty(1).Max();

    /// <summary>Kilos obtenidos en el parte de cada orden terminada.</summary>
    private async Task<Dictionary<Guid, decimal>> RealesAsync(IReadOnlyCollection<OrdenLinea> ordenes, CancellationToken ct)
    {
        var reales = new Dictionary<Guid, decimal>();
        foreach (var o in ordenes.Where(o => o.Estado == EstadoOrdenLinea.Terminada && o.ParteConfeccionId is not null))
        {
            if (await _repo.ParteAsync(o.ParteConfeccionId!.Value, ct).ConfigureAwait(false) is { Estado: EstadoParte.Validado } parte)
            {
                reales[o.Id] = parte.KilosObtenidos;
            }
        }

        return reales;
    }

    private async Task<Dictionary<Guid, string>> CategoriasAsync(Guid empresaId, CancellationToken ct) =>
        (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre);

    private async Task<IReadOnlyList<CalibradoDto>> DtosAsync(Guid empresaId, IReadOnlyList<Calibrado> lista, CancellationToken ct)
    {
        var lineas = (await _planta.LineasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(l => l.Id);
        var partidas = (await _repo.PartidasAsync(lista.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var categorias = await CategoriasAsync(empresaId, ct).ConfigureAwait(false);
        return lista.OrderByDescending(c => c.Fecha).ThenByDescending(c => c.CreadoEn).Select(c =>
        {
            var p = partidas.GetValueOrDefault(c.PartidaId);
            return new CalibradoDto(c.Id, c.LineaId, lineas.GetValueOrDefault(c.LineaId)?.Codigo ?? "—", c.PartidaId, p?.Codigo ?? "—", p?.ProductoId ?? Guid.Empty, p?.AgricultorId,
                c.Fecha, c.KilosEntrada, c.KilosSalida, c.Merma, c.Estado.ToString(), c.Referencia, c.ClasificacionId, c.MotivoAnulacion,
                c.Lineas.OrderBy(l => l.Destrio).ThenBy(l => l.Calibre, StringComparer.Ordinal).Select(l => new LineaCalibradoDto(l.Calibre, l.CategoriaId,
                    l.CategoriaId is { } id ? categorias.GetValueOrDefault(id) : null, l.Destrio, l.Kilos, Pct(l.Kilos, c.KilosSalida) ?? 0m, l.Piezas, l.GramosPieza)).ToList());
        }).ToList();
    }

    private async Task<Resultado<OrdenLineaDto>> TransicionAsync(Guid empresaId, Guid id, Func<OrdenLinea, Resultado> paso, CancellationToken ct)
    {
        if (await OrdenDeAsync(empresaId, id, ct).ConfigureAwait(false) is not { } o)
        {
            return Resultado.Fallo<OrdenLineaDto>(OrdenNoExiste());
        }

        var r = paso(o);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OrdenLineaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var linea = await _planta.LineaAsync(o.LineaId, ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(o, linea, await RealesAsync([o], ct).ConfigureAwait(false)));
    }

    private async Task<LineaPlanta?> LineaDeAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _planta.LineaAsync(id, ct).ConfigureAwait(false) is { } l && l.EmpresaId == empresaId ? l : null;

    private async Task<Calibrado?> CalibradoDeAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _planta.CalibradoAsync(id, ct).ConfigureAwait(false) is { } c && c.EmpresaId == empresaId ? c : null;

    private async Task<OrdenLinea?> OrdenDeAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _planta.OrdenAsync(id, ct).ConfigureAwait(false) is { } o && o.EmpresaId == empresaId ? o : null;

    private static LineaPlantaDto Dto(LineaPlanta l, IReadOnlyDictionary<Guid, string> categorias) =>
        new(l.Id, l.Codigo, l.Nombre, l.Tipo.ToString(), l.CapacidadKgHora, l.HorasTurno, l.Turnos, l.CapacidadDia, l.CentroAnaliticoId, l.Activa,
            l.Salidas.Select(s => new SalidaCalibradoraDto(s.Numero, s.Calibre, s.CategoriaId, s.CategoriaId is { } c ? categorias.GetValueOrDefault(c) : null, s.Destrio)).ToList());

    private static OrdenLineaDto Dto(OrdenLinea o, LineaPlanta? l, Dictionary<Guid, decimal> reales) =>
        new(o.Id, o.LineaId, l?.Codigo ?? "—", o.Fecha, o.Turno, o.Secuencia, o.ProductoId, o.Kilos, o.Cajas,
            l is { CapacidadKgHora: > 0m } ? Math.Round(o.Kilos / l.CapacidadKgHora, 2, MidpointRounding.AwayFromZero) : 0m,
            o.ClienteId, o.PedidoVentaId, o.LineaPlanId, o.Notas, o.Estado.ToString(), o.ParteConfeccionId, reales.TryGetValue(o.Id, out var r) ? r : null);

    private static ParadaDto Dto(ParadaLinea p, LineaPlanta? l) => new(p.Id, p.LineaId, l?.Codigo ?? "—", p.Fecha, p.Turno, p.Minutos, p.Motivo.ToString(), p.Notas);

    private static decimal? Pct(decimal parte, decimal total) => total == 0m ? null : Math.Round(parte * 100m / total, 1, MidpointRounding.AwayFromZero);

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

    private static DateOnly Min(DateOnly a, DateOnly b) => a < b ? a : b;

    private static Error LineaNoExiste() => Error.NoEncontrado("linea_planta.no_encontrada", "La línea no existe.");

    private static Error CalibradoNoExiste() => Error.NoEncontrado("calibrado.no_encontrado", "El calibrado no existe.");

    private static Error OrdenNoExiste() => Error.NoEncontrado("orden_linea.no_encontrada", "La orden no existe.");

    private static Error ParadaNoExiste() => Error.NoEncontrado("parada.no_encontrada", "La parada no existe.");
}
