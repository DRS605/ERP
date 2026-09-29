using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public interface IRepositorioFitosanitarios
{
    void Agregar(ProductoFitosanitario producto);

    void Agregar(CambioFitosanitario cambio);

    void Eliminar(ProductoFitosanitario producto);

    Task<ProductoFitosanitario?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ProductoFitosanitario>> ListarAsync(Guid empresaId, string? buscar, CancellationToken ct = default);

    /// <summary>Todos los productos de la empresa con seguimiento (para aplicar una carga del registro).</summary>
    Task<IReadOnlyList<ProductoFitosanitario>> TodosAsync(Guid empresaId, CancellationToken ct = default);

    Task<CambioFitosanitario?> CambioAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CambioFitosanitario>> CambiosAsync(Guid empresaId, DateOnly? desde, bool soloPendientes, CancellationToken ct = default);

    Task<bool> UsadoAsync(Guid fitosanitarioId, CancellationToken ct = default);
}

public sealed record DatosMateriaActiva(string? Nombre, string? Riqueza = null);

public sealed record DatosUsoFito(string? Cultivo, string? Plaga, decimal? DosisMinima = null, decimal? DosisMaxima = null, string? UnidadDosis = null,
    int? PlazoSeguridadDias = null, int? Aplicaciones = null, string? Observaciones = null);

public sealed record DatosFitosanitario(string? NumeroRegistro, string? Nombre, string? Titular = null, EstadoFitosanitario Estado = EstadoFitosanitario.Autorizado,
    DateOnly? FechaCaducidad = null, DateOnly? FechaLimiteVenta = null, DateOnly? FechaLimiteUso = null, IReadOnlyList<DatosMateriaActiva>? MateriasActivas = null,
    IReadOnlyList<DatosUsoFito>? Usos = null, Guid? ProductoId = null, DateOnly? FechaCancelacion = null, string? Formulado = null)
{
    public DatosProductoFito Dominio() => new(NumeroRegistro, Nombre, Titular, Estado, FechaCaducidad, FechaLimiteVenta, FechaLimiteUso,
        (MateriasActivas ?? []).Select(m => (m.Nombre, m.Riqueza)).ToList(),
        (Usos ?? []).Select(u => (u.Cultivo, u.Plaga, u.DosisMinima, u.DosisMaxima, u.UnidadDosis, u.PlazoSeguridadDias, u.Aplicaciones, u.Observaciones)).ToList(),
        FechaCancelacion, Formulado);
}

/// <summary>Una carga del registro: los productos tal como vienen. <c>Completa</c>: si es el registro entero, lo que no venga se da por retirado.</summary>
public sealed record CargaRegistroFito(IReadOnlyList<DatosFitosanitario>? Productos, bool Completa = false, string? Origen = null);

public sealed record UsoFitoDto(Guid Id, string Cultivo, string Plaga, decimal? DosisMinima, decimal? DosisMaxima, string? UnidadDosis, int? PlazoSeguridadDias,
    int? Aplicaciones, string? Observaciones);

public sealed record FitosanitarioDto(Guid Id, string NumeroRegistro, string Nombre, string? Titular, string Estado, DateOnly? FechaCaducidad, DateOnly? FechaLimiteVenta,
    DateOnly? FechaLimiteUso, bool AplicableHoy, Guid? ProductoId, IReadOnlyList<DatosMateriaActiva> MateriasActivas, IReadOnlyList<UsoFitoDto> Usos, DateTimeOffset ActualizadoEn,
    DateOnly? FechaCancelacion = null, string? Formulado = null);

public sealed record ResultadoCargaFitoDto(int Productos, int Altas, int ConCambios, int Retirados, int Errores, IReadOnlyList<string> Detalle);

/// <summary>A quién afecta un cambio: existencias del artículo enlazado y tratamientos recientes con el producto.</summary>
public sealed record AfectadoFitoDto(string Tipo, string Descripcion, Guid? AgricultorId = null, DateOnly? Fecha = null, decimal? Cantidad = null);

public sealed record CambioFitoDto(Guid Id, Guid FitosanitarioId, string NumeroRegistro, string Producto, string Tipo, string Detalle, DateTimeOffset DetectadoEn,
    bool Revisado, bool Restrictivo, IReadOnlyList<AfectadoFitoDto> Afectados);

/// <summary>Existencias de un artículo en el almacén (para avisar a quien tiene un fitosanitario retirado).</summary>
public interface IExistenciasFito
{
    Task<decimal> ExistenciasAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);
}

/// <summary>
/// Registro Oficial de Productos Fitosanitarios en la empresa: productos con sus materias activas y usos autorizados,
/// cargados de los ficheros del ministerio (o dados de alta a mano). Cada carga compara con lo que había y anota los
/// cambios; los que restringen (retirada, fechas, materias activas, usos retirados o modificados) salen como avisos
/// con quién tiene el producto en el almacén y quién lo ha aplicado en el último año.
/// </summary>
public sealed class RegistroFitosanitarios
{
    private readonly IRepositorioFitosanitarios _repo;
    private readonly IRepositorioCuaderno _cuaderno;
    private readonly IRepositorioAgro _agro;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;
    private readonly IExistenciasFito? _existencias;

    public RegistroFitosanitarios(IRepositorioFitosanitarios repo, IRepositorioCuaderno cuaderno, IRepositorioAgro agro, IUnidadDeTrabajoAgro unidad, IReloj reloj,
        IExistenciasFito? existencias = null)
    {
        _repo = repo;
        _cuaderno = cuaderno;
        _agro = agro;
        _unidad = unidad;
        _reloj = reloj;
        _existencias = existencias;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<FitosanitarioDto>> ListarAsync(Guid empresaId, string? buscar, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, buscar, ct).ConfigureAwait(false)).Select(Dto).ToList();

    public async Task<FitosanitarioDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p ? Dto(p) : null;

    public async Task<Resultado<FitosanitarioDto>> CrearAsync(Guid empresaId, DatosFitosanitario d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var numero = ProductoFitosanitario.Normalizar(d.NumeroRegistro);
        if ((await _repo.TodosAsync(empresaId, ct).ConfigureAwait(false)).Any(p => p.NumeroRegistro == numero))
        {
            return Resultado.Fallo<FitosanitarioDto>(Error.Conflicto("fitosanitario.repetido", $"Ya está el número de registro {numero}."));
        }

        var p = ProductoFitosanitario.Crear(empresaId, d.Dominio(), _reloj);
        if (p.EsFallo)
        {
            return Resultado.Fallo<FitosanitarioDto>(p.Error);
        }

        p.Valor.EnlazarArticulo(d.ProductoId);
        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p.Valor));
    }

    /// <summary>Cambia los datos a mano: los cambios que importan también quedan anotados (y avisan).</summary>
    public async Task<Resultado<FitosanitarioDto>> ActualizarAsync(Guid id, DatosFitosanitario d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<FitosanitarioDto>(Error.NoEncontrado("fitosanitario.no_encontrado", "El producto no está en el registro."));
        }

        var r = p.Aplicar(d.Dominio(), _reloj, out var cambios);
        if (r.EsFallo)
        {
            return Resultado.Fallo<FitosanitarioDto>(r.Error);
        }

        p.EnlazarArticulo(d.ProductoId);
        foreach (var (tipo, detalle) in cambios)
        {
            _repo.Agregar(CambioFitosanitario.Nuevo(p, tipo, detalle, _reloj));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(p));
    }

    /// <summary>Elimina un producto que no se ha usado en ningún tratamiento.</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("fitosanitario.no_encontrado", "El producto no está en el registro."));
        }

        if (await _repo.UsadoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("fitosanitario.en_uso", "El producto está en tratamientos del cuaderno de campo: no se elimina."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Aplica una carga del registro: da de alta lo nuevo, actualiza lo que cambia y anota cada cambio. En una carga
    /// completa, los productos que no vienen (y seguían autorizados) pasan a cancelados, sin tocar sus fechas.
    /// El enlace con el artículo del almacén se conserva.
    /// </summary>
    public async Task<ResultadoCargaFitoDto> CargarAsync(Guid empresaId, CargaRegistroFito carga, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(carga);
        var existentes = (await _repo.TodosAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(p => p.NumeroRegistro, StringComparer.Ordinal);
        var vistos = new HashSet<string>(StringComparer.Ordinal);
        int altas = 0, cambiados = 0, retirados = 0, errores = 0;
        var detalle = new List<string>();
        foreach (var d in carga.Productos ?? [])
        {
            var numero = ProductoFitosanitario.Normalizar(d.NumeroRegistro);
            if (!vistos.Add(numero))
            {
                continue;
            }

            if (existentes.TryGetValue(numero, out var p))
            {
                var r = p.Aplicar(d.Dominio() with { }, _reloj, out var cambios);
                if (r.EsFallo)
                {
                    errores++;
                    detalle.Add($"{numero}: {r.Error.Mensaje}");
                    continue;
                }

                if (cambios.Count > 0)
                {
                    cambiados++;
                    foreach (var (tipo, texto) in cambios)
                    {
                        _repo.Agregar(CambioFitosanitario.Nuevo(p, tipo, texto, _reloj));
                    }
                }
            }
            else
            {
                var nuevo = ProductoFitosanitario.Crear(empresaId, d.Dominio(), _reloj);
                if (nuevo.EsFallo)
                {
                    errores++;
                    detalle.Add($"{(numero.Length > 0 ? numero : "(sin número)")}: {nuevo.Error.Mensaje}");
                    continue;
                }

                _repo.Agregar(nuevo.Valor);
                altas++;
                // En el alta de una carga sobre un registro ya cargado, el producto nuevo también se avisa.
                if (existentes.Count > 0)
                {
                    _repo.Agregar(CambioFitosanitario.Nuevo(nuevo.Valor, TipoCambioFito.Alta, "Nuevo producto en el registro.", _reloj));
                }
            }
        }

        if (carga.Completa)
        {
            foreach (var p in existentes.Values.Where(p => !vistos.Contains(p.NumeroRegistro) && p.Estado == EstadoFitosanitario.Autorizado))
            {
                var datos = new DatosProductoFito(p.NumeroRegistro, p.Nombre, p.Titular, EstadoFitosanitario.Cancelado, p.FechaCaducidad, p.FechaLimiteVenta, p.FechaLimiteUso,
                    p.MateriasActivas.Select(m => ((string?)m.Nombre, m.Riqueza)).ToList(),
                    p.Usos.Select(u => ((string?)u.Cultivo, (string?)u.Plaga, u.DosisMinima, u.DosisMaxima, u.UnidadDosis, u.PlazoSeguridadDias, u.Aplicaciones, u.Observaciones)).ToList(),
                    p.FechaCancelacion ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), p.Formulado);
                p.Aplicar(datos, _reloj, out _);
                _repo.Agregar(CambioFitosanitario.Nuevo(p, TipoCambioFito.Retirado, "Ya no figura en el registro: se da por cancelado.", _reloj));
                retirados++;
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return new ResultadoCargaFitoDto(vistos.Count, altas, cambiados, retirados, errores, detalle);
    }

    /// <summary>Cambios detectados (los avisos), con quién tiene el producto en el almacén y quién lo ha aplicado en el último año.</summary>
    public async Task<IReadOnlyList<CambioFitoDto>> CambiosAsync(Guid empresaId, DateOnly? desde, bool soloPendientes, CancellationToken ct = default)
    {
        var cambios = await _repo.CambiosAsync(empresaId, desde, soloPendientes, ct).ConfigureAwait(false);
        if (cambios.Count == 0)
        {
            return [];
        }

        var parcelas = (await _agro.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var recientes = (await _cuaderno.ListarAsync(empresaId, null, Hoy.AddYears(-1), null, ct).ConfigureAwait(false)).Where(t => !t.Anulado).ToList();
        var productos = new Dictionary<Guid, ProductoFitosanitario?>();
        var lista = new List<CambioFitoDto>();
        foreach (var c in cambios)
        {
            if (!productos.TryGetValue(c.FitosanitarioId, out var p))
            {
                p = await _repo.ObtenerAsync(c.FitosanitarioId, ct).ConfigureAwait(false);
                productos[c.FitosanitarioId] = p;
            }

            var afectados = new List<AfectadoFitoDto>();
            if (c.Restrictivo)
            {
                if (p?.ProductoId is { } articulo && _existencias is not null
                    && await _existencias.ExistenciasAsync(empresaId, articulo, ct).ConfigureAwait(false) is var stock && stock > 0m)
                {
                    afectados.Add(new AfectadoFitoDto("Almacén", $"Hay {stock:0.###} en el almacén.", Cantidad: stock));
                }

                foreach (var t in recientes.Where(t => t.FitosanitarioId == c.FitosanitarioId).OrderByDescending(t => t.Fecha))
                {
                    var parcela = parcelas.GetValueOrDefault(t.ParcelaId);
                    afectados.Add(new AfectadoFitoDto("Tratamiento", $"Parcela {parcela?.Codigo ?? "?"} el {t.Fecha:dd/MM/yyyy}{(t.Cultivo is null ? "" : $" ({t.Cultivo} / {t.Motivo})")}.",
                        parcela?.AgricultorId, t.Fecha));
                }
            }

            lista.Add(new CambioFitoDto(c.Id, c.FitosanitarioId, c.NumeroRegistro, c.Producto, c.Tipo.ToString(), c.Detalle, c.DetectadoEn, c.Revisado, c.Restrictivo, afectados));
        }

        return lista;
    }

    public async Task<Resultado> MarcarRevisadoAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.CambioAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("cambio_fitosanitario.no_encontrado", "El aviso no existe."));
        }

        c.MarcarRevisado();
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private FitosanitarioDto Dto(ProductoFitosanitario p) => new(p.Id, p.NumeroRegistro, p.Nombre, p.Titular, p.Estado.ToString(), p.FechaCaducidad, p.FechaLimiteVenta,
        p.FechaLimiteUso, p.AplicableEl(Hoy), p.ProductoId, p.MateriasActivas.Select(m => new DatosMateriaActiva(m.Nombre, m.Riqueza)).ToList(),
        p.Usos.OrderBy(u => u.Cultivo, StringComparer.Ordinal).ThenBy(u => u.Plaga, StringComparer.Ordinal)
            .Select(u => new UsoFitoDto(u.Id, u.Cultivo, u.Plaga, u.DosisMinima, u.DosisMaxima, u.UnidadDosis, u.PlazoSeguridadDias, u.Aplicaciones, u.Observaciones)).ToList(),
        p.ActualizadoEn, p.FechaCancelacion, p.Formulado);
}

public sealed record TratamientoLoteDto(Guid TratamientoId, DateOnly Fecha, Guid ParcelaId, string Parcela, Guid AgricultorId, string? Agricultor, string Producto,
    string? NumeroRegistro, string? Lote, decimal? Cantidad, int PlazoSeguridadDias);

public sealed record PartidaTratadaDto(Guid PartidaId, Guid RecepcionId, string? Recepcion, DateOnly FechaRecoleccion, string Parcela, decimal NetoKg, Guid TratamientoId);

/// <summary>Traza de un lote de fitosanitario: dónde se aplicó, qué fruta salió de esas parcelas después y a qué clientes llegó.</summary>
public sealed record TrazaLoteFitoDto(Guid ArticuloId, string Lote, IReadOnlyList<TratamientoLoteDto> Tratamientos, IReadOnlyList<PartidaTratadaDto> Partidas,
    IReadOnlyList<DestinoClienteDto> Destinos);

/// <summary>
/// Trazabilidad de los fitosanitarios: de un lote aplicado a las parcelas, a las partidas recolectadas en ellas
/// desde el tratamiento (hasta un año después) y a los palés y clientes; y, al revés, de una partida o un palé a los
/// tratamientos que recibieron sus parcelas en el año anterior a la recolección.
/// </summary>
public sealed class TrazabilidadFitosanitarios
{
    private readonly IRepositorioCuaderno _cuaderno;
    private readonly IRepositorioAgro _agro;
    private readonly TrazabilidadAgro _traza;

    public TrazabilidadFitosanitarios(IRepositorioCuaderno cuaderno, IRepositorioAgro agro, TrazabilidadAgro traza)
    {
        _cuaderno = cuaderno;
        _agro = agro;
        _traza = traza;
    }

    public async Task<TrazaLoteFitoDto> DeLoteAsync(Guid empresaId, Guid articuloId, string lote, CancellationToken ct = default)
    {
        var codigo = (lote ?? string.Empty).Trim();
        var parcelas = (await _agro.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var tratamientos = (await _cuaderno.ListarAsync(empresaId, null, null, null, ct).ConfigureAwait(false))
            .Where(t => !t.Anulado && t.ArticuloId == articuloId && string.Equals(t.Lote, codigo, StringComparison.OrdinalIgnoreCase)).ToList();
        var nombres = new Dictionary<Guid, string?>();
        var lista = new List<TratamientoLoteDto>();
        foreach (var t in tratamientos)
        {
            var p = parcelas.GetValueOrDefault(t.ParcelaId);
            lista.Add(new TratamientoLoteDto(t.Id, t.Fecha, t.ParcelaId, p?.Codigo ?? "?", p?.AgricultorId ?? Guid.Empty,
                p is null ? null : await NombreAsync(nombres, p.AgricultorId, ct).ConfigureAwait(false), t.Producto, t.NumeroRegistro, t.Lote, t.CantidadConsumida, t.PlazoSeguridadDias));
        }

        var partidas = new List<PartidaTratadaDto>();
        if (tratamientos.Count > 0)
        {
            var desde = tratamientos.Min(t => t.Fecha);
            var hasta = tratamientos.Max(t => t.Fecha).AddYears(1);
            foreach (var r in (await _agro.RecepcionesAsync(empresaId, desde, hasta, null, ct).ConfigureAwait(false)).Where(r => r.Estado != EstadoRecepcion.Anulada))
            {
                foreach (var l in r.Lineas.Where(l => l.ParcelaId is not null && l.PartidaId is not null))
                {
                    var dia = l.FechaRecoleccion ?? r.Fecha;
                    var t = tratamientos.Where(t => t.ParcelaId == l.ParcelaId && dia >= t.Fecha && dia <= t.Fecha.AddYears(1)).OrderByDescending(t => t.Fecha).FirstOrDefault();
                    if (t is not null)
                    {
                        partidas.Add(new PartidaTratadaDto(l.PartidaId!.Value, r.Id, r.NumeroCompleto, dia, parcelas.GetValueOrDefault(l.ParcelaId!.Value)?.Codigo ?? "?",
                            l.NetoKg ?? r.NetoDe(l.Id), t.Id));
                    }
                }
            }
        }

        var destinos = new Dictionary<Guid, DestinoClienteDto>();
        foreach (var partida in partidas.Select(p => p.PartidaId).Distinct())
        {
            var traza = await _traza.HaciaAdelanteAsync(empresaId, partida, null, ct).ConfigureAwait(false);
            if (traza.EsCorrecto)
            {
                foreach (var d in traza.Valor.Destinos)
                {
                    destinos.TryAdd(d.PaleId, d);
                }
            }
        }

        return new TrazaLoteFitoDto(articuloId, codigo, lista.OrderBy(t => t.Fecha).ToList(), partidas.OrderBy(p => p.FechaRecoleccion).ToList(), destinos.Values.ToList());
    }

    /// <summary>Tratamientos de las parcelas de origen de una partida o palé en el año anterior a cada recolección (con su lote).</summary>
    public async Task<Resultado<IReadOnlyList<TratamientoLoteDto>>> DePartidaAsync(Guid empresaId, Guid? partidaId, string? sscc, CancellationToken ct = default)
    {
        var traza = await _traza.HaciaAtrasAsync(empresaId, partidaId, sscc, ct).ConfigureAwait(false);
        if (traza.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<TratamientoLoteDto>>(traza.Error);
        }

        var parcelas = (await _agro.ParcelasAsync(empresaId, null, ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var origenes = traza.Valor.Origenes.Where(o => o.ParcelaId is not null).ToList();
        var nombres = new Dictionary<Guid, string?>();
        var lista = new List<TratamientoLoteDto>();
        if (origenes.Count > 0)
        {
            var todos = await _cuaderno.ListarAsync(empresaId, origenes.Select(o => o.ParcelaId!.Value).Distinct().ToList(), null, null, ct).ConfigureAwait(false);
            foreach (var t in todos.Where(t => !t.Anulado && origenes.Any(o => o.ParcelaId == t.ParcelaId
                && (o.FechaRecoleccion ?? o.FechaRecepcion) >= t.Fecha && (o.FechaRecoleccion ?? o.FechaRecepcion) <= t.Fecha.AddYears(1))))
            {
                var p = parcelas.GetValueOrDefault(t.ParcelaId);
                lista.Add(new TratamientoLoteDto(t.Id, t.Fecha, t.ParcelaId, p?.Codigo ?? "?", p?.AgricultorId ?? Guid.Empty,
                    p is null ? null : await NombreAsync(nombres, p.AgricultorId, ct).ConfigureAwait(false), t.Producto, t.NumeroRegistro, t.Lote, t.CantidadConsumida, t.PlazoSeguridadDias));
            }
        }

        return Resultado.Ok<IReadOnlyList<TratamientoLoteDto>>(lista.OrderBy(t => t.Fecha).ToList());
    }

    private async Task<string?> NombreAsync(Dictionary<Guid, string?> cache, Guid agricultorId, CancellationToken ct)
    {
        if (!cache.TryGetValue(agricultorId, out var n))
        {
            n = (await _agro.AgricultorAsync(agricultorId, ct).ConfigureAwait(false))?.Nombre;
            cache[agricultorId] = n;
        }

        return n;
    }
}
