using AlxorCore.Nucleo.Comun;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public sealed record EntradaConsumo(Guid PartidaId, decimal Kilos, Guid? PaleId = null);

public sealed record EntradaManoObra(string? Descripcion, string Categoria, TipoHora TipoHora = TipoHora.Normal, decimal Horas = 0m, decimal? Piezas = null);

public sealed record EntradaMaquina(string? Descripcion, string Categoria, decimal Horas);

public sealed record EntradaMaterial(Guid ProductoId, decimal Cantidad);

public sealed record EntradaSalida(Guid ProductoId, decimal Kilos, decimal Factor = 1m, string? Calibre = null, Guid? CategoriaId = null, Guid? PaleId = null);

public sealed record DatosParteConfeccion(
    DateOnly Fecha, Guid? CampanaId = null, string? Descripcion = null, Guid? CentroAnaliticoId = null, decimal PorcentajeIndirectos = 0m,
    RepartoCoste Reparto = RepartoCoste.PorKilos, IReadOnlyList<EntradaConsumo>? Consumos = null, IReadOnlyList<EntradaManoObra>? ManoObra = null,
    IReadOnlyList<EntradaMaquina>? Maquinas = null, IReadOnlyList<EntradaMaterial>? Materiales = null, IReadOnlyList<EntradaSalida>? Salidas = null);

public sealed record ConsumoDto(Guid PartidaId, string? Partida, Guid? PaleId, decimal Kilos, decimal CosteKg, decimal Coste);

public sealed record ManoObraDto(string Descripcion, string Categoria, string TipoHora, decimal Horas, decimal? Piezas, Guid? TarifaId, decimal CosteUnitario, decimal Coste);

public sealed record MaquinaDto(string Descripcion, string Categoria, decimal Horas, Guid? TarifaId, decimal CosteUnitario, decimal Coste);

public sealed record MaterialDto(Guid ProductoId, string Nombre, decimal Cantidad, decimal CosteUnitario, decimal Coste);

public sealed record SalidaDto(int NumeroLinea, Guid ProductoId, string Nombre, decimal Kilos, decimal Factor, string? Calibre, Guid? CategoriaId, Guid? PaleId, Guid? PartidaId, decimal Coste, decimal CosteKg);

public sealed record ParteDto(
    Guid Id, string? Numero, DateOnly Fecha, Guid? CampanaId, string? Descripcion, Guid? CentroAnaliticoId, decimal PorcentajeIndirectos, string Reparto,
    string Estado, decimal KilosConsumidos, decimal KilosObtenidos, decimal Merma, decimal CosteFruta, decimal CosteMateriales, decimal CosteManoObra,
    decimal CosteMaquinaria, decimal CosteIndirectos, decimal CosteTotal, IReadOnlyList<ConsumoDto> Consumos, IReadOnlyList<ManoObraDto> ManoObra,
    IReadOnlyList<MaquinaDto> Maquinas, IReadOnlyList<MaterialDto> Materiales, IReadOnlyList<SalidaDto> Salidas, IReadOnlyList<ErrorDto> Errores);

public sealed record ParteResumenDto(Guid Id, string? Numero, DateOnly Fecha, string? Descripcion, string Estado, decimal KilosConsumidos, decimal KilosObtenidos, decimal CosteTotal);

public sealed record PaleDto(
    Guid Id, string Sscc, string? Tipo, string Estado, Guid? ClienteId, DateOnly? FechaExpedicion, string? ReferenciaExpedicion, decimal Kilos,
    IReadOnlyList<ContenidoPaleDto> Contenido);

public sealed record ContenidoPaleDto(Guid PartidaId, string? Partida, Guid ProductoId, decimal Kilos);

public sealed record DatosPale(string? Tipo = null);

public sealed record DatosMoverKilos(Guid PartidaId, decimal Kilos, Guid? DesdePaleId = null, DateOnly? Fecha = null);

public sealed record DatosExpedicion(IReadOnlyList<Guid> PaleIds, Guid? ClienteId = null, DateOnly? Fecha = null, string? Referencia = null);

/// <summary>Partes de confección: valoración, validación (consumos, salidas y genealogía) y anulación.</summary>
public sealed class ConfeccionAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;

    public ConfeccionAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProductos productos, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<ParteResumenDto>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        (await _repo.PartesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)).OrderByDescending(p => p.Fecha).ThenByDescending(p => p.Numero ?? int.MaxValue)
            .Select(p => new ParteResumenDto(p.Id, p.NumeroCompleto, p.Fecha, p.Descripcion, p.Estado.ToString(), p.KilosConsumidos, p.KilosObtenidos, p.CosteTotal)).ToList();

    public async Task<ParteDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        return p is null ? null : await DtoAsync(p, [], ct).ConfigureAwait(false);
    }

    public async Task<Resultado<ParteDto>> CrearAsync(Guid empresaId, DatosParteConfeccion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var parte = ParteConfeccion.Crear(empresaId, datos.Fecha, _reloj);
        var r = await FijarAsync(parte, datos, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ParteDto>(r.Error);
        }

        _repo.Agregar(parte);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(parte, [], ct).ConfigureAwait(false));
    }

    public async Task<Resultado<ParteDto>> ActualizarAsync(Guid id, DatosParteConfeccion datos, CancellationToken ct = default)
    {
        var parte = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        if (parte is null)
        {
            return NoEncontrado<ParteDto>();
        }

        var r = await FijarAsync(parte, datos, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ParteDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(parte, [], ct).ConfigureAwait(false));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var parte = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        if (parte is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("parte.no_encontrado", "El parte no existe."));
        }

        if (parte.Estado != EstadoParte.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("parte.no_borrador", "Solo se elimina un borrador; un parte validado se anula."));
        }

        _repo.Eliminar(parte);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Valora el borrador sin guardarlo: el coste que saldría, o todo lo que falta para validarlo.</summary>
    public async Task<Resultado<ParteDto>> ValorarAsync(Guid id, CancellationToken ct = default)
    {
        var parte = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        if (parte is null)
        {
            return NoEncontrado<ParteDto>();
        }

        var errores = await ComprobarAsync(parte, ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(parte, errores, ct).ConfigureAwait(false));
    }

    /// <summary>
    /// Valida el parte: consume los kilos de sus partidas (sueltos o de un palé), crea una partida por salida con
    /// su coste por kilo (en su palé, si se indica), registra la genealogía y lo numera sin huecos.
    /// </summary>
    public async Task<Resultado<ParteDto>> ValidarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        await _unidad.BloquearAsync(RecepcionesAgro.ClaveNumeracion(empresaId, ParteConfeccion.Serie), ct).ConfigureAwait(false);
        var parte = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        if (parte is null)
        {
            return NoEncontrado<ParteDto>();
        }

        var errores = await ComprobarAsync(parte, ct).ConfigureAwait(false);
        if (errores.Count > 0)
        {
            return Resultado.Fallo<ParteDto>(Valoracion.Resumen(errores));
        }

        var numero = await _repo.UltimoNumeroAsync(empresaId, ParteConfeccion.Serie, parte.Ejercicio, ct).ConfigureAwait(false) + 1;
        var codigo = $"{ParteConfeccion.Serie}-{parte.Ejercicio}-{numero:D6}";
        foreach (var c in parte.Consumos)
        {
            _repo.Agregar(MovimientoPartida.Crear(empresaId, c.PartidaId, parte.Fecha, TipoMovimientoPartida.Consumo, -c.Kilos, c.PaleId, "ParteConfeccion", parte.Id,
                $"Confección {codigo}", _reloj).Valor);
        }

        var partidas = new Dictionary<Guid, Guid>();
        var consumidas = parte.Consumos.GroupBy(c => c.PartidaId).Select(g => (Partida: g.Key, Kilos: g.Sum(c => c.Kilos))).ToList();
        foreach (var s in parte.Salidas)
        {
            var p = Partida.DeConfeccion(empresaId, $"{codigo}/{s.NumeroLinea}", s.ProductoId, parte.Fecha, s.Kilos, parte.Id, parte.CampanaId, s.Calibre, s.CosteKg, _reloj);
            _repo.Agregar(p);
            partidas[s.Id] = p.Id;
            _repo.Agregar(MovimientoPartida.Crear(empresaId, p.Id, parte.Fecha, TipoMovimientoPartida.Entrada, s.Kilos, s.PaleId, "ParteConfeccion", parte.Id,
                $"Confección {codigo}", _reloj).Valor);
            foreach (var (origen, kilos) in consumidas)
            {
                _repo.Agregar(Genealogia.Crear(empresaId, parte.Id, origen, p.Id, kilos));
            }
        }

        var validado = parte.Validar(numero, partidas, _reloj);
        if (validado.EsFallo)
        {
            return Resultado.Fallo<ParteDto>(validado.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(parte, [], ct).ConfigureAwait(false));
    }

    /// <summary>Anula un parte validado si sus salidas no se han movido: devuelve los kilos a sus partidas de origen.</summary>
    public async Task<Resultado<ParteDto>> AnularAsync(Guid id, CancellationToken ct = default)
    {
        var parte = await _repo.ParteAsync(id, ct).ConfigureAwait(false);
        if (parte is null)
        {
            return NoEncontrado<ParteDto>();
        }

        var salidas = parte.Salidas.Where(s => s.PartidaId is not null).Select(s => s.PartidaId!.Value).ToList();
        if (await _repo.TieneMovimientosPosterioresAsync(salidas, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<ParteDto>(Error.Conflicto("parte.salidas_usadas", "Sus salidas ya se han paletizado, consumido o expedido: deshaz antes esos movimientos."));
        }

        var r = parte.Anular();
        if (r.EsFallo)
        {
            return Resultado.Fallo<ParteDto>(r.Error);
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var c in parte.Consumos)
        {
            _repo.Agregar(MovimientoPartida.Crear(parte.EmpresaId, c.PartidaId, hoy, TipoMovimientoPartida.Anulacion, c.Kilos, c.PaleId, "ParteConfeccion", parte.Id,
                $"Anulación de {parte.NumeroCompleto}", _reloj).Valor);
        }

        foreach (var p in await _repo.PartidasAsync(salidas, ct).ConfigureAwait(false))
        {
            p.Anular();
            var salida = parte.Salidas.First(s => s.PartidaId == p.Id);
            _repo.Agregar(MovimientoPartida.Crear(parte.EmpresaId, p.Id, hoy, TipoMovimientoPartida.Anulacion, -salida.Kilos, salida.PaleId, "ParteConfeccion", parte.Id,
                $"Anulación de {parte.NumeroCompleto}", _reloj).Valor);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(parte, [], ct).ConfigureAwait(false));
    }

    private async Task<Resultado> FijarAsync(ParteConfeccion parte, DatosParteConfeccion datos, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var materiales = new List<DatosMaterial>();
        foreach (var m in datos.Materiales ?? [])
        {
            var p = await _productos.ObtenerAsync(m.ProductoId, ct).ConfigureAwait(false);
            if (p is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("producto.no_encontrado", "Un material no existe en el catálogo."));
            }

            materiales.Add(new DatosMaterial(p.Id, p.Nombre, m.Cantidad));
        }

        var salidas = new List<DatosSalida>();
        foreach (var s in datos.Salidas ?? [])
        {
            var p = await _productos.ObtenerAsync(s.ProductoId, ct).ConfigureAwait(false);
            if (p is null)
            {
                return Resultado.Fallo(Error.NoEncontrado("producto.no_encontrado", "Un producto obtenido no existe en el catálogo."));
            }

            if (!RecepcionesAgro.EsKilo(p.Unidad))
            {
                return Resultado.Fallo(Error.Validacion("parte.unidad", $"{p.Nombre} se mide en «{p.Unidad}»: las salidas de confección van en kilos."));
            }

            salidas.Add(new DatosSalida(p.Id, p.Nombre, s.Kilos, s.Factor, s.Calibre, s.CategoriaId, s.PaleId));
        }

        if ((datos.ManoObra ?? []).Any(m => string.IsNullOrWhiteSpace(m.Categoria)) || (datos.Maquinas ?? []).Any(m => string.IsNullOrWhiteSpace(m.Categoria)))
        {
            return Resultado.Fallo(Error.Validacion("parte.categoria", "Indica la categoría de cada línea de mano de obra y de maquinaria."));
        }

        return parte.Fijar(new DatosParte(datos.Fecha, datos.CampanaId, datos.Descripcion, datos.CentroAnaliticoId, datos.PorcentajeIndirectos, datos.Reparto,
            (datos.Consumos ?? []).Select(c => new DatosConsumo(c.PartidaId, c.Kilos, c.PaleId)).ToList(),
            (datos.ManoObra ?? []).Select(m => new DatosManoObra(m.Descripcion ?? string.Empty, m.Categoria, m.TipoHora, m.Horas, m.Piezas)).ToList(),
            (datos.Maquinas ?? []).Select(m => new DatosMaquina(m.Descripcion ?? string.Empty, m.Categoria, m.Horas)).ToList(),
            materiales, salidas));
    }

    /// <summary>Valora el parte y comprueba partidas, saldos y palés. Devuelve todos los errores.</summary>
    private async Task<IReadOnlyList<Error>> ComprobarAsync(ParteConfeccion parte, CancellationToken ct)
    {
        var errores = new List<Error>();
        var partidas = (await _repo.PartidasAsync(parte.Consumos.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var saldos = await _repo.SaldosAsync(partidas.Keys.ToList(), ct).ConfigureAwait(false);
        foreach (var g in parte.Consumos.GroupBy(c => (c.PartidaId, c.PaleId)))
        {
            if (!partidas.TryGetValue(g.Key.PartidaId, out var p) || p.Anulada)
            {
                errores.Add(Error.Validacion("parte.partida", "Una partida consumida no existe o está anulada."));
                continue;
            }

            var disponible = saldos.Where(s => s.PartidaId == p.Id && s.PaleId == g.Key.PaleId).Sum(s => s.Kilos);
            var pedido = g.Sum(c => c.Kilos);
            if (pedido > disponible)
            {
                errores.Add(Error.Conflicto("parte.saldo_insuficiente", $"La partida {p.Codigo} solo tiene {Redondeo.Formatear(disponible, 3)} kg {(g.Key.PaleId is null ? "sueltos" : "en ese palé")} y se consumen {Redondeo.Formatear(pedido, 3)}."));
            }
        }

        var palesSalida = parte.Salidas.Where(s => s.PaleId is not null).Select(s => s.PaleId!.Value).Distinct().ToList();
        if (palesSalida.Count > 0)
        {
            var pales = await _repo.PalesAsync(palesSalida, ct).ConfigureAwait(false);
            if (pales.Count != palesSalida.Count || pales.Any(p => p.Estado != EstadoPale.Abierto))
            {
                errores.Add(Error.Conflicto("parte.pale", "Las salidas solo van a palés abiertos."));
            }
        }

        // Coste de la fruta: el liquidado si ya se liquidó; si no, el precio estimado de la recepción.
        var recepciones = (await _repo.RecepcionesAsync(partidas.Values.Where(p => p.RecepcionId is not null).Select(p => p.RecepcionId!.Value).Distinct().ToList(), ct)
            .ConfigureAwait(false)).SelectMany(r => r.Lineas).ToDictionary(l => l.Id);
        decimal? CosteKg(Guid partidaId) =>
            partidas.TryGetValue(partidaId, out var p)
                ? p.CosteKg ?? (p.LineaRecepcionId is { } l && recepciones.TryGetValue(l, out var linea) ? linea.PrecioEstimadoKg : null)
                : null;

        var costesMaterial = new Dictionary<Guid, decimal?>();
        foreach (var m in parte.Materiales.Select(m => m.ProductoId).Distinct())
        {
            var producto = await _productos.ObtenerAsync(m, ct).ConfigureAwait(false);
            costesMaterial[m] = producto is { PrecioCompra: > 0m } ? producto.PrecioCompra : null;
        }

        var tarifas = await _repo.TarifasAsync(parte.EmpresaId, ct).ConfigureAwait(false);
        errores.AddRange(parte.Valorar(tarifas, CosteKg, m => costesMaterial.GetValueOrDefault(m)));
        return errores;
    }

    private async Task<ParteDto> DtoAsync(ParteConfeccion p, IReadOnlyList<Error> errores, CancellationToken ct)
    {
        var codigos = (await _repo.PartidasAsync(p.Consumos.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Codigo);
        return new ParteDto(p.Id, p.NumeroCompleto, p.Fecha, p.CampanaId, p.Descripcion, p.CentroAnaliticoId, p.PorcentajeIndirectos, p.Reparto.ToString(), p.Estado.ToString(),
            p.KilosConsumidos, p.KilosObtenidos, p.Merma, p.CosteFruta, p.CosteMateriales, p.CosteManoObra, p.CosteMaquinaria, p.CosteIndirectos, p.CosteTotal,
            p.Consumos.Select(c => new ConsumoDto(c.PartidaId, codigos.GetValueOrDefault(c.PartidaId), c.PaleId, c.Kilos, c.CosteKg, c.Coste)).ToList(),
            p.ManoObra.Select(m => new ManoObraDto(m.Descripcion, m.Categoria, m.TipoHora.ToString(), m.Horas, m.Piezas, m.TarifaId, m.CosteUnitario, m.Coste)).ToList(),
            p.Maquinas.Select(m => new MaquinaDto(m.Descripcion, m.Categoria, m.Horas, m.TarifaId, m.CosteUnitario, m.Coste)).ToList(),
            p.Materiales.Select(m => new MaterialDto(m.ProductoId, m.Nombre, m.Cantidad, m.CosteUnitario, m.Coste)).ToList(),
            p.Salidas.OrderBy(s => s.NumeroLinea).Select(s => new SalidaDto(s.NumeroLinea, s.ProductoId, s.Nombre, s.Kilos, s.Factor, s.Calibre, s.CategoriaId, s.PaleId,
                s.PartidaId, s.Coste, s.CosteKg)).ToList(),
            errores.Select(e => new ErrorDto(e.Codigo, e.Mensaje)).ToList());
    }

    private static Resultado<T> NoEncontrado<T>() => Resultado.Fallo<T>(Error.NoEncontrado("parte.no_encontrado", "El parte no existe."));
}

/// <summary>Palés SSCC: alta con numeración GS1, paletizado, cierre y expedición.</summary>
public sealed class PalesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaClientes _clientes;
    private readonly IReloj _reloj;

    public PalesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaClientes clientes, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _clientes = clientes;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<PaleDto>> ListarAsync(Guid empresaId, EstadoPale? estado, CancellationToken ct = default)
    {
        var pales = await _repo.PalesAsync(empresaId, estado, ct).ConfigureAwait(false);
        var lista = new List<PaleDto>();
        foreach (var p in pales.OrderByDescending(p => p.CreadoEn))
        {
            lista.Add(await DtoAsync(p, ct).ConfigureAwait(false));
        }

        return lista;
    }

    public async Task<PaleDto?> ObtenerAsync(Guid empresaId, string idOSscc, CancellationToken ct = default)
    {
        var pale = Guid.TryParse(idOSscc, out var id)
            ? await _repo.PaleAsync(id, ct).ConfigureAwait(false)
            : await _repo.PalePorSsccAsync(empresaId, idOSscc.Trim(), ct).ConfigureAwait(false);
        return pale is null ? null : await DtoAsync(pale, ct).ConfigureAwait(false);
    }

    /// <summary>Da de alta un palé con el siguiente SSCC de la empresa (extensión + prefijo GS1 + serie + control).</summary>
    public async Task<Resultado<PaleDto>> CrearAsync(Guid empresaId, DatosPale datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        await _unidad.BloquearAsync(RecepcionesAgro.ClaveNumeracion(empresaId, "SSCC"), ct).ConfigureAwait(false);
        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false) ?? ConfiguracionAgro.Crear(empresaId);
        var serie = await _repo.PalesCreadosAsync(empresaId, ct).ConfigureAwait(false) + 1;
        var sscc = config.Sscc(serie);
        if (sscc.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(sscc.Error);
        }

        var pale = Pale.Crear(empresaId, sscc.Valor, datos.Tipo, _reloj);
        if (pale.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(pale.Error);
        }

        _repo.Agregar(pale.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(pale.Valor, ct).ConfigureAwait(false));
    }

    /// <summary>Pone kilos de una partida en el palé (sueltos o desde otro palé abierto).</summary>
    public async Task<Resultado<PaleDto>> PaletizarAsync(Guid paleId, DatosMoverKilos datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return NoEncontrado<PaleDto>();
        }

        var r = await MoverAsync(pale.EmpresaId, datos.PartidaId, datos.Kilos, datos.DesdePaleId, pale, datos.Fecha, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<PaleDto>(r.Error) : Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    /// <summary>Saca kilos de una partida del palé (vuelven a quedar sueltos).</summary>
    public async Task<Resultado<PaleDto>> DespaletizarAsync(Guid paleId, DatosMoverKilos datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return NoEncontrado<PaleDto>();
        }

        if (pale.Estado != EstadoPale.Abierto)
        {
            return Resultado.Fallo<PaleDto>(Error.Conflicto("pale.no_abierto", "Solo se saca mercancía de un palé abierto."));
        }

        var r = await MoverAsync(pale.EmpresaId, datos.PartidaId, datos.Kilos, pale.Id, null, datos.Fecha, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<PaleDto>(r.Error) : Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<PaleDto>> CerrarAsync(Guid paleId, CancellationToken ct = default)
    {
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return NoEncontrado<PaleDto>();
        }

        if ((await _repo.ContenidoPaleAsync(pale.Id, ct).ConfigureAwait(false)).All(c => c.Kilos <= 0m))
        {
            return Resultado.Fallo<PaleDto>(Error.Conflicto("pale.vacio", "El palé está vacío."));
        }

        var r = pale.Cerrar();
        if (r.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<PaleDto>> ReabrirAsync(Guid paleId, CancellationToken ct = default)
    {
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return NoEncontrado<PaleDto>();
        }

        var r = pale.Reabrir();
        if (r.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    /// <summary>Expide palés cerrados a un cliente: da salida a todo su contenido y quedan como expedidos.</summary>
    public async Task<Resultado<IReadOnlyList<PaleDto>>> ExpedirAsync(Guid empresaId, DatosExpedicion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (datos.PaleIds is null || datos.PaleIds.Count == 0)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("expedicion.sin_pales", "Indica los palés que salen."));
        }

        if (datos.ClienteId is { } clienteId && await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var fecha = datos.Fecha ?? Hoy;
        var pales = await _repo.PalesAsync(datos.PaleIds.Distinct().ToList(), ct).ConfigureAwait(false);
        if (pales.Count != datos.PaleIds.Distinct().Count())
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("pale.no_encontrado", "Algún palé no existe."));
        }

        foreach (var pale in pales)
        {
            var r = pale.Expedir(datos.ClienteId, fecha, datos.Referencia);
            if (r.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto(r.Error.Codigo, $"{pale.Sscc}: {r.Error.Mensaje}"));
            }

            foreach (var c in (await _repo.ContenidoPaleAsync(pale.Id, ct).ConfigureAwait(false)).Where(c => c.Kilos > 0m))
            {
                _repo.Agregar(MovimientoPartida.Crear(empresaId, c.PartidaId, fecha, TipoMovimientoPartida.Expedicion, -c.Kilos, pale.Id, "Expedicion", pale.Id,
                    $"Expedición {datos.Referencia}".Trim(), _reloj).Valor);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var lista = new List<PaleDto>();
        foreach (var p in pales)
        {
            lista.Add(await DtoAsync(p, ct).ConfigureAwait(false));
        }

        return Resultado.Ok<IReadOnlyList<PaleDto>>(lista);
    }

    private async Task<Resultado> MoverAsync(Guid empresaId, Guid partidaId, decimal kilos, Guid? desdePaleId, Pale? hacia, DateOnly? fecha, CancellationToken ct)
    {
        if (hacia is { Estado: not EstadoPale.Abierto })
        {
            return Resultado.Fallo(Error.Conflicto("pale.no_abierto", "El palé no está abierto."));
        }

        var partida = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (partida is null || partida.Anulada)
        {
            return Resultado.Fallo(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        if (desdePaleId is { } origenId && origenId != hacia?.Id)
        {
            var origen = await _repo.PaleAsync(origenId, ct).ConfigureAwait(false);
            if (origen is null || origen.Estado != EstadoPale.Abierto)
            {
                return Resultado.Fallo(Error.Conflicto("pale.no_abierto", "El palé de origen no existe o no está abierto."));
            }
        }

        if (kilos <= 0m || decimal.Round(kilos, 3) != kilos || desdePaleId == hacia?.Id)
        {
            return Resultado.Fallo(Error.Validacion("partida.kilos", "Indica los kilos (positivos, hasta 3 decimales) y un origen distinto del destino."));
        }

        var disponible = (await _repo.SaldosAsync([partidaId], ct).ConfigureAwait(false)).Where(s => s.PaleId == desdePaleId).Sum(s => s.Kilos);
        if (kilos > disponible)
        {
            return Resultado.Fallo(Error.Conflicto("partida.saldo_insuficiente", $"La partida {partida.Codigo} solo tiene {Redondeo.Formatear(disponible, 3)} kg {(desdePaleId is null ? "sueltos" : "en ese palé")}."));
        }

        var dia = fecha ?? Hoy;
        _repo.Agregar(MovimientoPartida.Crear(empresaId, partidaId, dia, TipoMovimientoPartida.Paletizado, -kilos, desdePaleId, "Pale", hacia?.Id ?? desdePaleId, null, _reloj).Valor);
        _repo.Agregar(MovimientoPartida.Crear(empresaId, partidaId, dia, TipoMovimientoPartida.Paletizado, kilos, hacia?.Id, "Pale", hacia?.Id ?? desdePaleId, null, _reloj).Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<PaleDto> DtoAsync(Pale p, CancellationToken ct)
    {
        var contenido = await _repo.ContenidoPaleAsync(p.Id, ct).ConfigureAwait(false);
        if (p.Estado == EstadoPale.Expedido)
        {
            // Expedido: se muestra lo que llevaba al salir.
            var salidas = (await _repo.MovimientosAsync(contenido.Select(c => c.PartidaId).ToList(), ct).ConfigureAwait(false))
                .Where(m => m.PaleId == p.Id && m.Tipo == TipoMovimientoPartida.Expedicion)
                .GroupBy(m => m.PartidaId).Select(g => new SaldoPartida(g.Key, p.Id, -g.Sum(m => m.Kilos))).ToList();
            contenido = salidas;
        }

        var partidas = (await _repo.PartidasAsync(contenido.Select(c => c.PartidaId).ToList(), ct).ConfigureAwait(false)).ToDictionary(x => x.Id);
        var lineas = contenido.Where(c => c.Kilos > 0m)
            .Select(c => new ContenidoPaleDto(c.PartidaId, partidas.GetValueOrDefault(c.PartidaId)?.Codigo, partidas.GetValueOrDefault(c.PartidaId)?.ProductoId ?? Guid.Empty, c.Kilos))
            .ToList();
        return new PaleDto(p.Id, p.Sscc, p.Tipo, p.Estado.ToString(), p.ClienteId, p.FechaExpedicion, p.ReferenciaExpedicion, lineas.Sum(l => l.Kilos), lineas);
    }

    private static Resultado<T> NoEncontrado<T>() => Resultado.Fallo<T>(Error.NoEncontrado("pale.no_encontrado", "El palé no existe."));
}
