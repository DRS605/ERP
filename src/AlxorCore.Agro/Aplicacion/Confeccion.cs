using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Agro.Aplicacion;

public sealed record EntradaConsumo(Guid PartidaId, decimal Kilos, Guid? PaleId = null);

public sealed record EntradaManoObra(string? Descripcion, string Categoria, TipoHora TipoHora = TipoHora.Normal, decimal Horas = 0m, decimal? Piezas = null,
    bool Confeccion = true);

public sealed record EntradaMaquina(string? Descripcion, string Categoria, decimal Horas);

public sealed record EntradaMaterial(Guid ProductoId, decimal Cantidad);

public sealed record EntradaSalida(Guid ProductoId, decimal Kilos, decimal Factor = 1m, string? Calibre = null, Guid? CategoriaId = null, Guid? PaleId = null,
    int? Cajas = null, Guid? EnvaseProductoId = null, string? MotivoDescalificacion = null);

public sealed record DatosParteConfeccion(
    DateOnly Fecha, Guid? CampanaId = null, string? Descripcion = null, Guid? CentroAnaliticoId = null, decimal PorcentajeIndirectos = 0m,
    RepartoCoste Reparto = RepartoCoste.PorKilos, IReadOnlyList<EntradaConsumo>? Consumos = null, IReadOnlyList<EntradaManoObra>? ManoObra = null,
    IReadOnlyList<EntradaMaquina>? Maquinas = null, IReadOnlyList<EntradaMaterial>? Materiales = null, IReadOnlyList<EntradaSalida>? Salidas = null);

public sealed record ConsumoDto(Guid PartidaId, string? Partida, Guid? PaleId, decimal Kilos, decimal CosteKg, decimal Coste);

public sealed record ManoObraDto(string Descripcion, string Categoria, string TipoHora, decimal Horas, decimal? Piezas, Guid? TarifaId, decimal CosteUnitario, decimal Coste,
    bool Confeccion = true);

public sealed record MaquinaDto(string Descripcion, string Categoria, decimal Horas, Guid? TarifaId, decimal CosteUnitario, decimal Coste);

public sealed record MaterialDto(Guid ProductoId, string Nombre, decimal Cantidad, decimal CosteUnitario, decimal Coste);

public sealed record SalidaDto(int NumeroLinea, Guid ProductoId, string Nombre, decimal Kilos, decimal Factor, string? Calibre, Guid? CategoriaId, Guid? PaleId, Guid? PartidaId, decimal Coste, decimal CosteKg,
    int? Cajas = null, Guid? EnvaseProductoId = null, decimal? SegundosTeoricos = null, decimal CosteConfeccion = 0m, string? MotivoDescalificacion = null);

public sealed record ParteDto(
    Guid Id, string? Numero, DateOnly Fecha, Guid? CampanaId, string? Descripcion, Guid? CentroAnaliticoId, decimal PorcentajeIndirectos, string Reparto,
    string Estado, decimal KilosConsumidos, decimal KilosObtenidos, decimal Merma, decimal CosteFruta, decimal CosteMateriales, decimal CosteManoObra,
    decimal CosteMaquinaria, decimal CosteIndirectos, decimal CosteTotal, IReadOnlyList<ConsumoDto> Consumos, IReadOnlyList<ManoObraDto> ManoObra,
    IReadOnlyList<MaquinaDto> Maquinas, IReadOnlyList<MaterialDto> Materiales, IReadOnlyList<SalidaDto> Salidas, IReadOnlyList<ErrorDto> Errores);

public sealed record ParteResumenDto(Guid Id, string? Numero, DateOnly Fecha, string? Descripcion, string Estado, decimal KilosConsumidos, decimal KilosObtenidos, decimal CosteTotal);

public sealed record PaleDto(
    Guid Id, string Sscc, string? Tipo, string Estado, Guid? ClienteId, DateOnly? FechaExpedicion, string? ReferenciaExpedicion, decimal Kilos,
    IReadOnlyList<ContenidoPaleDto> Contenido, Guid? PlantillaId = null, int Cajas = 0, int? CajasPorPale = null, Guid? CartaPorteId = null, Guid? AlbaranId = null);

public sealed record ContenidoPaleDto(Guid PartidaId, string? Partida, Guid ProductoId, decimal Kilos, int Cajas = 0);

public sealed record DatosPale(string? Tipo = null, Guid? PlantillaId = null);

public sealed record DatosMoverKilos(Guid PartidaId, decimal Kilos, Guid? DesdePaleId = null, DateOnly? Fecha = null);

/// <summary>Cajas de una partida que se ponen en el palé (positivas) o se sacan de él (negativas).</summary>
public sealed record DatosCajas(Guid PartidaId, int Cajas, DateOnly? Fecha = null);

/// <summary>Lectura del escáner en el punto de paletizado: la etiqueta de la caja (GS1-128 con el lote en el AI 10, o el código de la partida).</summary>
public sealed record LecturaCaja(string Codigo, int Cajas = 1);

/// <summary>
/// Montaje rápido: con una plantilla y una partida, monta de una vez <see cref="NumeroPales"/> palés completos, o reparte
/// <see cref="Cajas"/> cajas (el último palé queda abierto si no se completa). Sin ninguno de los dos, monta todos los
/// palés completos que dan los kilos sueltos de la partida.
/// </summary>
public sealed record DatosMontaje(Guid PlantillaId, Guid PartidaId, int? NumeroPales = null, int? Cajas = null, DateOnly? Fecha = null);

/// <summary>
/// Expedición de palés cerrados. Con <see cref="CartaPorte"/> se emite además la carta de porte (una línea por producto,
/// con sus cajas y kilos), que queda enlazada a los palés.
/// </summary>
public sealed record DatosExpedicion(IReadOnlyList<Guid> PaleIds, Guid? ClienteId = null, DateOnly? Fecha = null, string? Referencia = null,
    bool CartaPorte = false, string? Transportista = null, string? Matricula = null, string? LugarOrigen = null, string? LugarDestino = null, string? Observaciones = null,
    Guid? PedidoVentaId = null, Guid? TransportistaId = null, Guid? VehiculoId = null, decimal? TemperaturaConsigna = null, string? Termografo = null);

/// <summary>Lo que se imprime en la etiqueta del palé.</summary>
public sealed record EtiquetaPaleDto(string Sscc, string? Producto, string? Marca, string? TipoPale, int Cajas, decimal Kilos, string? Lote, DateOnly Fecha, string? Destinatario);

public sealed record PlantillaPaleDto(Guid Id, string Codigo, string Nombre, string? TipoPale, Guid? ProductoId, string? Marca, int CajasPorPale, decimal KilosPorCaja,
    int? Filas, int? Columnas, int? CajasPorCapa, int? Capas, decimal KilosPorPale, Guid? ClienteId, bool Activa, Guid? EnvaseProductoId = null, Guid? PaleProductoId = null)
{
    public static PlantillaPaleDto De(PlantillaPale p) => new(p.Id, p.Codigo, p.Nombre, p.TipoPale, p.ProductoId, p.Marca, p.CajasPorPale, p.KilosPorCaja,
        p.Filas, p.Columnas, p.CajasPorCapa, p.Capas, p.KilosPorPale, p.ClienteId, p.Activa, p.EnvaseProductoId, p.PaleProductoId);
}

public sealed record DatosPlantilla(string? Codigo, string? Nombre, int CajasPorPale, decimal KilosPorCaja, string? TipoPale = null, Guid? ProductoId = null,
    string? Marca = null, int? Filas = null, int? Columnas = null, Guid? ClienteId = null, bool Activa = true, Guid? EnvaseProductoId = null, Guid? PaleProductoId = null)
{
    public DatosPlantillaPale Datos => new(CajasPorPale, KilosPorCaja, TipoPale, ProductoId, Marca, Filas, Columnas, ClienteId, EnvaseProductoId, PaleProductoId);
}

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
        var certificaciones = await CertificacionesSalidasAsync(parte, await _repo.PartidasAsync(consumidas.Select(c => c.Partida).ToList(), ct).ConfigureAwait(false), ct)
            .ConfigureAwait(false);
        foreach (var s in parte.Salidas)
        {
            var (certs, quitadas) = certificaciones[s.Id].Valor;
            var p = Partida.DeConfeccion(empresaId, $"{codigo}/{s.NumeroLinea}", s.ProductoId, parte.Fecha, s.Kilos, parte.Id, parte.CampanaId, s.Calibre, s.CosteKg, _reloj,
                certs);
            _repo.Agregar(p);
            if (quitadas != Certificaciones.Ninguna)
            {
                _repo.Agregar(DescalificacionPartida.Crear(empresaId, p.Id, quitadas, s.MotivoDescalificacion!, "ParteConfeccion", parte.Id, null, _reloj.AhoraUtc));
            }

            partidas[s.Id] = p.Id;
            _repo.Agregar(MovimientoPartida.Crear(empresaId, p.Id, parte.Fecha, TipoMovimientoPartida.Entrada, s.Kilos, s.PaleId, "ParteConfeccion", parte.Id,
                $"Confección {codigo}", _reloj).Valor);
        }

        foreach (var (origen, destino, kilos) in Genealogia.Repartir(consumidas, parte.Salidas.Select(s => (partidas[s.Id], s.Kilos)).ToList()))
        {
            _repo.Agregar(Genealogia.Crear(empresaId, parte.Id, origen, destino, kilos));
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

            salidas.Add(new DatosSalida(p.Id, p.Nombre, s.Kilos, s.Factor, s.Calibre, s.CategoriaId, s.PaleId, s.Cajas, s.EnvaseProductoId, s.MotivoDescalificacion));
        }

        if ((datos.ManoObra ?? []).Any(m => string.IsNullOrWhiteSpace(m.Categoria)) || (datos.Maquinas ?? []).Any(m => string.IsNullOrWhiteSpace(m.Categoria)))
        {
            return Resultado.Fallo(Error.Validacion("parte.categoria", "Indica la categoría de cada línea de mano de obra y de maquinaria."));
        }

        return parte.Fijar(new DatosParte(datos.Fecha, datos.CampanaId, datos.Descripcion, datos.CentroAnaliticoId, datos.PorcentajeIndirectos, datos.Reparto,
            (datos.Consumos ?? []).Select(c => new DatosConsumo(c.PartidaId, c.Kilos, c.PaleId)).ToList(),
            (datos.ManoObra ?? []).Select(m => new DatosManoObra(m.Descripcion ?? string.Empty, m.Categoria, m.TipoHora, m.Horas, m.Piezas, m.Confeccion)).ToList(),
            (datos.Maquinas ?? []).Select(m => new DatosMaquina(m.Descripcion ?? string.Empty, m.Categoria, m.Horas)).ToList(),
            materiales, salidas));
    }

    /// <summary>Valora el parte y comprueba partidas, saldos y palés. Devuelve todos los errores.</summary>
    /// <summary>
    /// Certificaciones de cada salida: las comunes a las partidas consumidas (ecológico con convencional da convencional),
    /// ajustadas a lo que declara el artículo que sale (ver <see cref="ReglasCertificacion.Aplicar"/>).
    /// </summary>
    private async Task<Dictionary<Guid, Resultado<(Certificaciones Resultado, Certificaciones Quitadas)>>> CertificacionesSalidasAsync(ParteConfeccion parte,
        IEnumerable<Partida> consumidas, CancellationToken ct)
    {
        var comunes = ReglasCertificacion.Comunes(consumidas.Select(p => p.Certificaciones));
        var resultado = new Dictionary<Guid, Resultado<(Certificaciones, Certificaciones)>>();
        foreach (var s in parte.Salidas)
        {
            var declaracion = await _repo.DeclaracionAsync(parte.EmpresaId, s.ProductoId, ct).ConfigureAwait(false);
            var r = ReglasCertificacion.Aplicar(comunes, declaracion?.Exige, s.MotivoDescalificacion, s.Nombre);
            resultado[s.Id] = r.EsFallo ? Resultado.Fallo<(Certificaciones, Certificaciones)>(Error.Conflicto(r.Error.Codigo, $"Salida {s.NumeroLinea}: {r.Error.Mensaje}")) : r;
        }

        return resultado;
    }

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

            if (parte.Fecha < p.Fecha)
            {
                errores.Add(Error.Conflicto("parte.fecha_anterior", $"La partida {p.Codigo} entró el {p.Fecha:dd/MM/yyyy}: no se puede consumir el {parte.Fecha:dd/MM/yyyy}."));
            }

            var disponible = saldos.Where(s => s.PartidaId == p.Id && s.PaleId == g.Key.PaleId).Sum(s => s.Kilos);
            var pedido = g.Sum(c => c.Kilos);
            if (pedido > disponible)
            {
                errores.Add(Error.Conflicto("parte.saldo_insuficiente", $"La partida {p.Codigo} solo tiene {Redondeo.Formatear(disponible, 3)} kg {(g.Key.PaleId is null ? "sueltos" : "en ese palé")} y se consumen {Redondeo.Formatear(pedido, 3)}."));
            }
        }

        foreach (var c in await CertificacionesSalidasAsync(parte, partidas.Values, ct).ConfigureAwait(false))
        {
            if (c.Value.EsFallo)
            {
                errores.Add(c.Value.Error);
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
        // Rendimiento de confección: el del producto y el envase de la salida o, si no hay, el del producto sin envase.
        var rendimientos = await _repo.RendimientosAsync(parte.EmpresaId, ct).ConfigureAwait(false);
        decimal? Rendimiento(Guid producto, Guid? envase) =>
            (rendimientos.FirstOrDefault(r => r.ProductoId == producto && envase is not null && r.EnvaseProductoId == envase)
             ?? rendimientos.FirstOrDefault(r => r.ProductoId == producto && r.EnvaseProductoId is null))?.CajasHora;
        errores.AddRange(parte.Valorar(tarifas, CosteKg, m => costesMaterial.GetValueOrDefault(m), Rendimiento));
        return errores;
    }

    private async Task<ParteDto> DtoAsync(ParteConfeccion p, IReadOnlyList<Error> errores, CancellationToken ct)
    {
        var codigos = (await _repo.PartidasAsync(p.Consumos.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Codigo);
        return new ParteDto(p.Id, p.NumeroCompleto, p.Fecha, p.CampanaId, p.Descripcion, p.CentroAnaliticoId, p.PorcentajeIndirectos, p.Reparto.ToString(), p.Estado.ToString(),
            p.KilosConsumidos, p.KilosObtenidos, p.Merma, p.CosteFruta, p.CosteMateriales, p.CosteManoObra, p.CosteMaquinaria, p.CosteIndirectos, p.CosteTotal,
            p.Consumos.Select(c => new ConsumoDto(c.PartidaId, codigos.GetValueOrDefault(c.PartidaId), c.PaleId, c.Kilos, c.CosteKg, c.Coste)).ToList(),
            p.ManoObra.Select(m => new ManoObraDto(m.Descripcion, m.Categoria, m.TipoHora.ToString(), m.Horas, m.Piezas, m.TarifaId, m.CosteUnitario, m.Coste, m.Confeccion)).ToList(),
            p.Maquinas.Select(m => new MaquinaDto(m.Descripcion, m.Categoria, m.Horas, m.TarifaId, m.CosteUnitario, m.Coste)).ToList(),
            p.Materiales.Select(m => new MaterialDto(m.ProductoId, m.Nombre, m.Cantidad, m.CosteUnitario, m.Coste)).ToList(),
            p.Salidas.OrderBy(s => s.NumeroLinea).Select(s => new SalidaDto(s.NumeroLinea, s.ProductoId, s.Nombre, s.Kilos, s.Factor, s.Calibre, s.CategoriaId, s.PaleId,
                s.PartidaId, s.Coste, s.CosteKg, s.Cajas, s.EnvaseProductoId, s.SegundosTeoricos, s.CosteConfeccion, s.MotivoDescalificacion)).ToList(),
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
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;
    private readonly IDocumentosExpedicion? _documentos;
    private readonly EnvasesTerceros? _envases;
    private readonly IRepositorioReservas? _reservas;

    public PalesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaClientes clientes, IConsultaProductos productos, IReloj reloj,
        IDocumentosExpedicion? documentos = null, EnvasesTerceros? envases = null, IRepositorioReservas? reservas = null)
    {
        _reservas = reservas;
        _envases = envases;
        _repo = repo;
        _unidad = unidad;
        _clientes = clientes;
        _productos = productos;
        _reloj = reloj;
        _documentos = documentos;
    }

    /// <summary>Máximo de palés que se montan en una sola operación.</summary>
    public const int MaximoPalesMontaje = 200;

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
        PlantillaPale? plantilla = null;
        if (datos.PlantillaId is { } plantillaId)
        {
            plantilla = await _repo.PlantillaPaleAsync(plantillaId, ct).ConfigureAwait(false);
            if (plantilla is null || !plantilla.Activa)
            {
                return Resultado.Fallo<PaleDto>(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla de palé no existe o está desactivada."));
            }
        }

        var pales = await NuevosPalesAsync(empresaId, 1, datos.Tipo ?? plantilla?.TipoPale, plantilla?.Id, ct).ConfigureAwait(false);
        if (pales.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(pales.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(pales.Valor[0], ct).ConfigureAwait(false));
    }

    /// <summary>Datos de la etiqueta logística del palé (producto, marca, cajas, peso neto, lote y destinatario).</summary>
    public async Task<Resultado<EtiquetaPaleDto>> EtiquetaAsync(Guid empresaId, string idOSscc, CancellationToken ct = default)
    {
        var dto = await ObtenerAsync(empresaId, idOSscc, ct).ConfigureAwait(false);
        if (dto is null)
        {
            return NoEncontrado<EtiquetaPaleDto>();
        }

        if (dto.Kilos <= 0m)
        {
            return Resultado.Fallo<EtiquetaPaleDto>(Error.Conflicto("pale.vacio", "El palé está vacío."));
        }

        var plantilla = dto.PlantillaId is { } pid ? await _repo.PlantillaPaleAsync(pid, ct).ConfigureAwait(false) : null;
        var productos = dto.Contenido.Select(c => c.ProductoId).Distinct().ToList();
        var producto = productos.Count == 1 ? (await _productos.ObtenerAsync(productos[0], ct).ConfigureAwait(false))?.Nombre : "Varios productos";
        var lotes = dto.Contenido.Select(c => c.Partida).Distinct().ToList();
        var clienteId = dto.ClienteId ?? plantilla?.ClienteId;
        var cliente = clienteId is { } cid ? (await _clientes.ObtenerAsync(cid, ct).ConfigureAwait(false))?.Nombre : null;
        return Resultado.Ok(new EtiquetaPaleDto(dto.Sscc, producto, plantilla?.Marca, dto.Tipo, dto.Cajas, dto.Kilos, lotes.Count == 1 ? lotes[0] : "VARIOS",
            dto.FechaExpedicion ?? Hoy, cliente));
    }

    public async Task<IReadOnlyList<PlantillaPaleDto>> PlantillasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.PlantillasPaleAsync(empresaId, ct).ConfigureAwait(false)).Select(PlantillaPaleDto.De).ToList();

    public async Task<Resultado<PlantillaPaleDto>> CrearPlantillaAsync(Guid empresaId, DatosPlantilla datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = PlantillaPale.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Datos);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PlantillaPaleDto>(p.Error);
        }

        if ((await _repo.PlantillasPaleAsync(empresaId, ct).ConfigureAwait(false)).Any(x => x.Codigo == p.Valor.Codigo))
        {
            return Resultado.Fallo<PlantillaPaleDto>(Error.Conflicto("plantilla.codigo_duplicado", $"Ya hay una plantilla {p.Valor.Codigo}."));
        }

        var error = await ValidarReferenciasAsync(datos, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<PlantillaPaleDto>(error);
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlantillaPaleDto.De(p.Valor));
    }

    public async Task<Resultado<PlantillaPaleDto>> ActualizarPlantillaAsync(Guid id, DatosPlantilla datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = await _repo.PlantillaPaleAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<PlantillaPaleDto>(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla de palé no existe."));
        }

        var error = await ValidarReferenciasAsync(datos, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<PlantillaPaleDto>(error);
        }

        var r = p.Fijar(datos.Nombre, datos.Datos, datos.Activa, await _repo.PlantillaPaleEnUsoAsync(id, ct).ConfigureAwait(false));
        if (r.EsFallo)
        {
            return Resultado.Fallo<PlantillaPaleDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlantillaPaleDto.De(p));
    }

    /// <summary>Elimina una plantilla con la que no se ha montado ningún palé; si ya se usó, se desactiva.</summary>
    public async Task<Resultado<BajaDto>> EliminarPlantillaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PlantillaPaleAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla de palé no existe."));
        }

        if (await _repo.PlantillaPaleEnUsoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<BajaDto>(Bajas.EnUso("plantilla", $"la plantilla {p.Codigo}", "palés montados"));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    private async Task<Error?> ValidarReferenciasAsync(DatosPlantilla datos, CancellationToken ct)
    {
        if (datos.ProductoId is { } producto && await _productos.ObtenerAsync(producto, ct).ConfigureAwait(false) is null)
        {
            return Error.NoEncontrado("producto.no_encontrado", "El producto no existe.");
        }

        return datos.ClienteId is { } cliente && await _clientes.ObtenerAsync(cliente, ct).ConfigureAwait(false) is null
            ? Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe.")
            : null;
    }

    /// <summary>Da de alta <paramref name="cantidad"/> palés con SSCC correlativos (sin guardar).</summary>
    private Task<Resultado<IReadOnlyList<Pale>>> NuevosPalesAsync(Guid empresaId, int cantidad, string? tipo, Guid? plantillaId, CancellationToken ct) =>
        NuevosPalesAsync(_repo, _unidad, _reloj, empresaId, cantidad, tipo, plantillaId, ct);

    /// <summary>
    /// Da de alta <paramref name="cantidad"/> palés con SSCC correlativos (sin guardar), bajo el bloqueo de la numeración.
    /// <paramref name="crear"/> construye cada palé con su SSCC y su posición (por defecto, un palé abierto).
    /// </summary>
    internal static async Task<Resultado<IReadOnlyList<Pale>>> NuevosPalesAsync(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IReloj reloj, Guid empresaId, int cantidad,
        string? tipo, Guid? plantillaId, CancellationToken ct, Func<string, int, Resultado<Pale>>? crear = null)
    {
        await unidad.BloquearAsync(RecepcionesAgro.ClaveNumeracion(empresaId, "SSCC"), ct).ConfigureAwait(false);
        var config = await repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false) ?? ConfiguracionAgro.Crear(empresaId);
        var serie = await repo.PalesCreadosAsync(empresaId, ct).ConfigureAwait(false);
        var lista = new List<Pale>();
        for (var i = 1; i <= cantidad; i++)
        {
            var sscc = config.Sscc(serie + i);
            if (sscc.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<Pale>>(sscc.Error);
            }

            var pale = crear is null ? Pale.Crear(empresaId, sscc.Valor, tipo, reloj, plantillaId) : crear(sscc.Valor, i - 1);
            if (pale.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<Pale>>(pale.Error);
            }

            repo.Agregar(pale.Valor);
            lista.Add(pale.Valor);
        }

        return Resultado.Ok<IReadOnlyList<Pale>>(lista);
    }

    /// <summary>
    /// Pone cajas de una partida en un palé montado con plantilla (o las saca, con cajas negativas). Los kilos son las
    /// cajas por los kilos por caja de la plantilla, y el palé se cierra solo al completar sus cajas.
    /// </summary>
    /// <summary>
    /// Lectura de una caja con el escáner: el lote de la etiqueta es el código de la partida, y la caja se añade al palé como
    /// si se indicara a mano (con la plantilla del palé, sus kilos por caja).
    /// </summary>
    public async Task<Resultado<PaleDto>> LeerCajaAsync(Guid empresaId, Guid paleId, LecturaCaja datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var lote = LoteDeEtiqueta(datos.Codigo);
        if (lote.Length == 0)
        {
            return Resultado.Fallo<PaleDto>(Error.Validacion("lectura.vacia", "La lectura no trae ningún código."));
        }

        var partida = (await _repo.PartidasConSaldoAsync(empresaId, ct).ConfigureAwait(false))
            .FirstOrDefault(p => string.Equals(p.Codigo, lote, StringComparison.OrdinalIgnoreCase));
        if (partida is null)
        {
            return Resultado.Fallo<PaleDto>(Error.NoEncontrado("lectura.partida", $"No hay ninguna partida con existencias con el lote {lote}."));
        }

        return await CajasAsync(paleId, new DatosCajas(partida.Id, datos.Cajas <= 0 ? 1 : datos.Cajas), ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Lote de una etiqueta leída: el AI (10) de un GS1-128, legible «(01)…(10)LOTE» o en bruto («01» + GTIN, fechas
    /// opcionales y «10» + lote hasta el separador FNC1), o el propio código si no es GS1.
    /// </summary>
    public static string LoteDeEtiqueta(string? codigo)
    {
        var c = (codigo ?? string.Empty).Trim();
        if (c.StartsWith("]C1", StringComparison.Ordinal) || c.StartsWith("]d2", StringComparison.Ordinal))
        {
            c = c[3..];
        }

        var legible = c.IndexOf("(10)", StringComparison.Ordinal);
        if (legible >= 0)
        {
            var resto = c[(legible + 4)..];
            var fin = resto.IndexOf('(', StringComparison.Ordinal);
            return (fin >= 0 ? resto[..fin] : resto).Trim();
        }

        if (c.Length > 18 && c.StartsWith("01", StringComparison.Ordinal) && c[2..16].All(char.IsAsciiDigit))
        {
            var i = 16;
            while (i + 8 <= c.Length && c.Substring(i, 2) is "11" or "13" or "15" or "17" && c.Substring(i + 2, 6).All(char.IsAsciiDigit))
            {
                i += 8;
            }

            if (i + 2 < c.Length && c.Substring(i, 2) == "10")
            {
                var resto = c[(i + 2)..];
                var fin = resto.IndexOf('\u001d', StringComparison.Ordinal);
                return fin >= 0 ? resto[..fin] : resto;
            }
        }

        return c;
    }

    public async Task<Resultado<PaleDto>> CajasAsync(Guid paleId, DatosCajas datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return NoEncontrado<PaleDto>();
        }

        var plantilla = pale.PlantillaId is { } pid ? await _repo.PlantillaPaleAsync(pid, ct).ConfigureAwait(false) : null;
        if (plantilla is null)
        {
            return Resultado.Fallo<PaleDto>(Error.Validacion("pale.sin_plantilla", "El palé no se montó con plantilla: se paletiza por kilos."));
        }

        if (pale.Estado != EstadoPale.Abierto)
        {
            return Resultado.Fallo<PaleDto>(Error.Conflicto("pale.no_abierto", "El palé no está abierto (reábrelo para cambiar sus cajas)."));
        }

        if (datos.Cajas == 0)
        {
            return Resultado.Fallo<PaleDto>(Error.Validacion("pale.cajas", "Indica cuántas cajas se ponen (o, en negativo, se sacan)."));
        }

        var contenido = await _repo.ContenidoPaleAsync(pale.Id, ct).ConfigureAwait(false);
        var llevadas = contenido.Sum(c => c.Cajas);
        if (datos.Cajas > 0 && llevadas + datos.Cajas > plantilla.CajasPorPale)
        {
            return Resultado.Fallo<PaleDto>(Error.Conflicto("pale.completo",
                $"El palé lleva {llevadas} de {plantilla.CajasPorPale} cajas: caben {plantilla.CajasPorPale - llevadas} más."));
        }

        if (datos.Cajas < 0 && -datos.Cajas > contenido.Where(c => c.PartidaId == datos.PartidaId).Sum(c => c.Cajas))
        {
            return Resultado.Fallo<PaleDto>(Error.Conflicto("pale.cajas", "El palé no lleva tantas cajas de esa partida."));
        }

        var kilos = Math.Abs(datos.Cajas) * plantilla.KilosPorCaja;
        var r = datos.Cajas > 0
            ? await MoverAsync(pale.EmpresaId, datos.PartidaId, kilos, null, pale, datos.Fecha, ct, datos.Cajas, plantilla).ConfigureAwait(false)
            : await MoverAsync(pale.EmpresaId, datos.PartidaId, kilos, pale.Id, null, datos.Fecha, ct, -datos.Cajas).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(r.Error);
        }

        if (llevadas + datos.Cajas == plantilla.CajasPorPale)
        {
            pale.Cerrar();
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    /// <summary>Monta de una vez los palés de una partida con una plantilla (ver <see cref="DatosMontaje"/>).</summary>
    public async Task<Resultado<IReadOnlyList<PaleDto>>> MontarAsync(Guid empresaId, DatosMontaje datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var plantilla = await _repo.PlantillaPaleAsync(datos.PlantillaId, ct).ConfigureAwait(false);
        if (plantilla is null || !plantilla.Activa)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("plantilla.no_encontrada", "La plantilla de palé no existe o está desactivada."));
        }

        var partida = await _repo.PartidaAsync(datos.PartidaId, ct).ConfigureAwait(false);
        if (partida is null || partida.Anulada)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        if (plantilla.ProductoId is { } producto && producto != partida.ProductoId)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("plantilla.producto", $"La plantilla {plantilla.Codigo} es para otro producto."));
        }

        if (datos.NumeroPales is < 1 || datos.Cajas is < 1 || (datos.NumeroPales is not null && datos.Cajas is not null))
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("montaje.cantidad", "Indica el número de palés o las cajas (una de las dos, positiva)."));
        }

        var sueltos = (await _repo.SaldosAsync([partida.Id], ct).ConfigureAwait(false)).Where(x => x.PaleId is null).Sum(x => x.Kilos);
        var cajas = datos.Cajas ?? (datos.NumeroPales * plantilla.CajasPorPale)
            ?? (int)Math.Floor(sueltos / plantilla.KilosPorPale) * plantilla.CajasPorPale;
        if (cajas == 0)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("montaje.sin_kilos",
                $"La partida {partida.Codigo} tiene {Redondeo.Formatear(sueltos, 3)} kg sueltos: no llega a un palé completo ({Redondeo.Formatear(plantilla.KilosPorPale, 3)} kg)."));
        }

        var kilos = cajas * plantilla.KilosPorCaja;
        if (kilos > sueltos)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("partida.saldo_insuficiente",
                $"{cajas} cajas son {Redondeo.Formatear(kilos, 3)} kg y la partida {partida.Codigo} solo tiene {Redondeo.Formatear(sueltos, 3)} kg sueltos."));
        }

        var numero = (cajas + plantilla.CajasPorPale - 1) / plantilla.CajasPorPale;
        if (numero > MaximoPalesMontaje)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("montaje.maximo", $"Se montan como mucho {MaximoPalesMontaje} palés de una vez."));
        }

        var pales = await NuevosPalesAsync(empresaId, numero, plantilla.TipoPale, plantilla.Id, ct).ConfigureAwait(false);
        if (pales.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(pales.Error);
        }

        var dia = datos.Fecha ?? Hoy;
        var restantes = cajas;
        foreach (var pale in pales.Valor)
        {
            var enEste = Math.Min(restantes, plantilla.CajasPorPale);
            restantes -= enEste;
            var kg = enEste * plantilla.KilosPorCaja;
            _repo.Agregar(MovimientoPartida.Crear(empresaId, partida.Id, dia, TipoMovimientoPartida.Paletizado, -kg, null, "Pale", pale.Id, null, _reloj, -enEste).Valor);
            _repo.Agregar(MovimientoPartida.Crear(empresaId, partida.Id, dia, TipoMovimientoPartida.Paletizado, kg, pale.Id, "Pale", pale.Id, null, _reloj, enEste).Valor);
        }

        // La base de datos comprueba al confirmar que se paletiza en palés abiertos: primero se montan y después se
        // cierran los completos (si el cierre fallase, quedan montados y abiertos, listos para cerrar).
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var completos = pales.Valor.Take(cajas / plantilla.CajasPorPale).ToList();
        foreach (var pale in completos)
        {
            pale.Cerrar();
        }

        if (completos.Count > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
        var lista = new List<PaleDto>();
        foreach (var p in pales.Valor)
        {
            lista.Add(await DtoAsync(p, ct).ConfigureAwait(false));
        }

        return Resultado.Ok<IReadOnlyList<PaleDto>>(lista);
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

        if (pale.PlantillaId is not null)
        {
            return Resultado.Fallo<PaleDto>(PorCajas());
        }

        var r = await MoverAsync(pale.EmpresaId, datos.PartidaId, datos.Kilos, datos.DesdePaleId, pale, datos.Fecha, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<PaleDto>(r.Error) : Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    private static Error PorCajas() => Error.Validacion("pale.por_cajas", "Este palé se montó con plantilla: se ponen y se sacan cajas, no kilos.");

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

        if (pale.PlantillaId is not null)
        {
            return Resultado.Fallo<PaleDto>(PorCajas());
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

        // Con pedido de venta, el cliente es el del pedido.
        if (datos.PedidoVentaId is { } pedidoId)
        {
            var delPedido = _documentos is null ? null : await _documentos.ClienteDePedidoAsync(pedidoId, ct).ConfigureAwait(false);
            if (delPedido is null)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido de venta."));
            }

            if (datos.ClienteId is { } indicado && indicado != delPedido)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("expedicion.cliente_pedido", "El cliente no es el del pedido."));
            }

            datos = datos with { ClienteId = delPedido };
        }

        if (datos.ClienteId is { } clienteId && await _clientes.ObtenerAsync(clienteId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        if (datos.CartaPorte && (datos.ClienteId is null || _documentos is null))
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Validacion("expedicion.carta_sin_cliente", "Para emitir la carta de porte indica el cliente."));
        }

        var fecha = datos.Fecha ?? Hoy;
        var cargado = new List<(Guid PaleId, SaldoPartida Contenido)>();
        var pales = await _repo.PalesAsync(datos.PaleIds.Distinct().ToList(), ct).ConfigureAwait(false);
        if (pales.Count != datos.PaleIds.Distinct().Count())
        {
            return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.NoEncontrado("pale.no_encontrado", "Algún palé no existe."));
        }

        // Un palé reservado solo sale con su pedido; con él, la reserva queda consumida.
        if (_reservas is not null)
        {
            foreach (var reserva in await _reservas.ActivasDePalesAsync(pales.Select(p => p.Id).ToList(), ct).ConfigureAwait(false))
            {
                if (reserva.PedidoVentaId != datos.PedidoVentaId)
                {
                    var sscc = pales.First(p => p.Id == reserva.PaleId).Sscc;
                    return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("pale.reservado", $"{sscc} está reservado para otro pedido: anula antes la reserva."));
                }

                reserva.Consumir(fecha);
            }
        }

        foreach (var pale in pales)
        {
            // Nada sale antes de haber entrado, y cada partida sale con las certificaciones que exige su artículo.
            var contenido = (await _repo.ContenidoPaleAsync(pale.Id, ct).ConfigureAwait(false)).Where(c => c.Kilos > 0m).ToList();
            if (contenido.Count == 0)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("expedicion.pale_vacio", $"{pale.Sscc} está vacío: no lleva kilos que expedir."));
            }

            foreach (var partida in await _repo.PartidasAsync(contenido.Select(c => c.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false))
            {
                if (fecha < partida.Fecha)
                {
                    return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("expedicion.fecha_anterior",
                        $"{pale.Sscc}: la partida {partida.Codigo} es del {partida.Fecha:dd/MM/yyyy} y no puede salir el {fecha:dd/MM/yyyy}."));
                }

                if (await _repo.DeclaracionAsync(empresaId, partida.ProductoId, ct).ConfigureAwait(false) is { } declaracion
                    && (declaracion.Exige & ~partida.Certificaciones) is var falta and not Certificaciones.Ninguna)
                {
                    return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto("certificacion.falta",
                        $"{pale.Sscc}: la partida {partida.Codigo} no es {ReglasCertificacion.Texto(falta)} y su artículo se vende como tal."));
                }
            }

            var r = pale.Expedir(datos.ClienteId, fecha, datos.Referencia);
            if (r.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(Error.Conflicto(r.Error.Codigo, $"{pale.Sscc}: {r.Error.Mensaje}"));
            }

            foreach (var c in contenido)
            {
                _repo.Agregar(MovimientoPartida.Crear(empresaId, c.PartidaId, fecha, TipoMovimientoPartida.Expedicion, -c.Kilos, pale.Id, "Expedicion", pale.Id,
                    $"Expedición {datos.Referencia}".Trim(), _reloj, c.Cajas > 0 ? -c.Cajas : 0).Valor);
                cargado.Add((pale.Id, c));
            }
        }

        // Albarán del pedido (primero: comprueba que lo expedido cabe en lo pendiente de servir).
        Guid? albaranId = null;
        if (datos.PedidoVentaId is { } pedido)
        {
            var partidasCargadas = (await _repo.PartidasAsync(cargado.Select(c => c.Contenido.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
            var lineas = cargado.GroupBy(c => partidasCargadas.GetValueOrDefault(c.Contenido.PartidaId)?.ProductoId ?? Guid.Empty)
                .Select(g => (g.Key, g.Sum(c => c.Contenido.Kilos), g.Sum(c => Math.Max(c.Contenido.Cajas, 0)))).ToList();
            // Coste por kilo de cada producto expedido: el de sus partidas (confeccionadas o liquidadas), si todas lo tienen.
            var costeKg = cargado.GroupBy(c => partidasCargadas.GetValueOrDefault(c.Contenido.PartidaId)?.ProductoId ?? Guid.Empty)
                .Where(g => g.All(c => partidasCargadas.GetValueOrDefault(c.Contenido.PartidaId)?.CosteKg is not null) && g.Sum(c => c.Contenido.Kilos) > 0m)
                .ToDictionary(g => g.Key, g => decimal.Round(g.Sum(c => c.Contenido.Kilos * partidasCargadas[c.Contenido.PartidaId].CosteKg!.Value) / g.Sum(c => c.Contenido.Kilos), 6));
            var palesPorProducto = cargado.GroupBy(c => partidasCargadas.GetValueOrDefault(c.Contenido.PartidaId)?.ProductoId ?? Guid.Empty)
                .ToDictionary(g => g.Key, g => g.Select(c => c.PaleId).Distinct().Count());
            var albaran = await _documentos!.EmitirAlbaranAsync(empresaId, new AlbaranExpedicion(pedido, fecha, datos.Referencia, lineas, costeKg, palesPorProducto), ct)
                .ConfigureAwait(false);
            if (albaran.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<PaleDto>>(albaran.Error);
            }

            albaranId = albaran.Valor.Id;
            foreach (var pale in pales)
            {
                pale.AsignarAlbaran(albaran.Valor.Id, albaran.Valor.Numero);
            }
        }

        Guid? cartaId = null;
        try
        {
            if (datos.CartaPorte)
            {
                var carta = await EmitirCartaPorteAsync(empresaId, datos, fecha, cargado, ct, albaranId).ConfigureAwait(false);
                if (carta.EsFallo)
                {
                    await DeshacerDocumentosAsync(albaranId, null).ConfigureAwait(false);
                    return Resultado.Fallo<IReadOnlyList<PaleDto>>(carta.Error);
                }

                cartaId = carta.Valor.Id;
                foreach (var pale in pales)
                {
                    pale.AsignarCartaPorte(carta.Valor.Id, carta.Valor.Numero);
                }
            }

            // Envases retornables: las cajas y el palé de la plantilla se entregan al cliente (o a su transportista).
            if (_envases is not null && datos.ClienteId is { } destinatario)
            {
                var entregas = new List<(Guid, Guid?, int, Guid?)>();
                foreach (var pale in pales)
                {
                    var plantilla = pale.PlantillaId is { } pid ? await _repo.PlantillaPaleAsync(pid, ct).ConfigureAwait(false) : null;
                    if (plantilla is { EnvaseProductoId: not null } or { PaleProductoId: not null })
                    {
                        entregas.Add((pale.Id, plantilla.EnvaseProductoId, cargado.Where(c => c.PaleId == pale.Id).Sum(c => Math.Max(c.Contenido.Cajas, 0)), plantilla.PaleProductoId));
                    }
                }

                var envases = await _envases.EntregarEnExpedicionAsync(empresaId, destinatario, entregas, fecha, datos.TransportistaId, datos.Matricula, datos.Referencia, ct)
                    .ConfigureAwait(false);
                if (envases.EsFallo)
                {
                    await DeshacerDocumentosAsync(albaranId, cartaId).ConfigureAwait(false);
                    return Resultado.Fallo<IReadOnlyList<PaleDto>>(envases.Error);
                }
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
        catch when (albaranId is not null || cartaId is not null)
        {
            // La expedición no se guardó: el albarán y la carta ya emitidos se anulan (sus números quedan usados).
            await DeshacerDocumentosAsync(albaranId, cartaId).ConfigureAwait(false);
            throw;
        }

        var lista = new List<PaleDto>();
        foreach (var p in pales)
        {
            lista.Add(await DtoAsync(p, ct).ConfigureAwait(false));
        }

        return Resultado.Ok<IReadOnlyList<PaleDto>>(lista);
    }

    /// <summary>Anula la expedición de un palé: devuelve sus kilos a las partidas (dentro del palé) y lo deja cerrado.</summary>
    public async Task<Resultado<PaleDto>> AnularExpedicionAsync(Guid empresaId, Guid paleId, CancellationToken ct = default)
    {
        var pale = await _repo.PaleAsync(paleId, ct).ConfigureAwait(false);
        if (pale is null)
        {
            return Resultado.Fallo<PaleDto>(Error.NoEncontrado("pale.no_encontrado", "El palé no existe."));
        }

        var salida = (await _repo.MovimientosDePaleAsync(paleId, ct).ConfigureAwait(false))
            .Where(m => m.DocumentoTipo == DocumentoExpedicion).GroupBy(m => m.PartidaId)
            .Select(g => (Partida: g.Key, Kilos: -g.Sum(m => m.Kilos), Cajas: -g.Sum(m => m.Cajas))).Where(x => x.Kilos > 0m).ToList();
        var carta = pale.CartaPorteId;
        var albaranPale = pale.AlbaranId;
        var r = pale.AnularExpedicion();
        if (r.EsFallo)
        {
            return Resultado.Fallo<PaleDto>(r.Error);
        }

        foreach (var (partida, kilos, cajas) in salida)
        {
            _repo.Agregar(MovimientoPartida.Crear(empresaId, partida, Hoy, TipoMovimientoPartida.Anulacion, kilos, pale.Id, DocumentoExpedicion, pale.Id,
                "Anulación de la expedición", _reloj, Math.Max(cajas, 0)).Valor);
        }

        // Su reserva, si la tenía, vuelve a estar activa.
        foreach (var reserva in _reservas is null ? [] : await _reservas.DePaleAsync(pale.Id, ct).ConfigureAwait(false))
        {
            reserva.Reactivar();
        }

        // Los envases entregados con el palé vuelven (contramovimiento en la misma cuenta).
        if (_envases is not null && await _envases.AnularDeDocumentoAsync(pale.Id, "expedición anulada", ct).ConfigureAwait(false) is { EsFallo: true } fallo)
        {
            return Resultado.Fallo<PaleDto>(fallo.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);

        // Si ya no queda ningún palé expedido con esa carta de porte, la carta se anula (el transporte no se hizo).
        if (carta is { } cartaId && _documentos is not null
            && (await _repo.PalesDeCartaPorteAsync(cartaId, ct).ConfigureAwait(false)).All(p => p.Estado != EstadoPale.Expedido))
        {
            await _documentos.AnularCartaPorteAsync(cartaId, "Expedición anulada.", ct).ConfigureAwait(false);
        }

        // Igual con el albarán: al volver todos sus palés se anula, y lo servido vuelve a quedar pendiente en el pedido. Si
        // vuelven solo algunos, una devolución de venta con lo que traen corrige el albarán.
        if (albaranPale is { } alb && _documentos is not null)
        {
            if ((await _repo.PalesDeAlbaranAsync(alb, ct).ConfigureAwait(false)).All(p => p.Estado != EstadoPale.Expedido))
            {
                if ((await _documentos.AnularDevolucionesDeVueltaAsync(empresaId, alb, ct).ConfigureAwait(false)).EsCorrecto)
                {
                    await _documentos.AnularAlbaranAsync(alb, "Expedición anulada.", ct).ConfigureAwait(false);
                }
            }
            else
            {
                var contenido = new List<(Guid ProductoId, decimal Kilos, int Cajas)>();
                foreach (var (partida, kilos, cajas) in salida)
                {
                    if (await _repo.PartidaAsync(partida, ct).ConfigureAwait(false) is { } p)
                    {
                        contenido.Add((p.ProductoId, kilos, Math.Max(cajas, 0)));
                    }
                }

                var porProducto = contenido.GroupBy(c => c.ProductoId).Select(g => (g.Key, g.Sum(c => c.Kilos), g.Sum(c => c.Cajas))).ToList();
                var devolucion = await _documentos.DevolverParcialAsync(empresaId, alb, porProducto, $"Vuelta del palé {pale.Sscc}", ct).ConfigureAwait(false);
                if (devolucion.EsFallo)
                {
                    return Resultado.Fallo<PaleDto>(Error.Conflicto(devolucion.Error.Codigo,
                        $"El palé ha vuelto, pero no se pudo corregir el albarán: {devolucion.Error.Mensaje} Regístralo como devolución del albarán."));
                }
            }
        }

        return Resultado.Ok(await DtoAsync(pale, ct).ConfigureAwait(false));
    }

    /// <summary>Tipo de documento de los movimientos de una expedición (la salida y, si se anula, su vuelta).</summary>
    public const string DocumentoExpedicion = "Expedicion";

    private async Task<Resultado> MoverAsync(Guid empresaId, Guid partidaId, decimal kilos, Guid? desdePaleId, Pale? hacia, DateOnly? fecha, CancellationToken ct,
        int cajas = 0, PlantillaPale? plantilla = null)
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

        if (plantilla?.ProductoId is { } producto && producto != partida.ProductoId)
        {
            return Resultado.Fallo(Error.Validacion("plantilla.producto", $"La plantilla {plantilla.Codigo} es para otro producto."));
        }

        if (desdePaleId is { } origenId && origenId != hacia?.Id)
        {
            var origen = await _repo.PaleAsync(origenId, ct).ConfigureAwait(false);
            if (origen is null || origen.Estado != EstadoPale.Abierto)
            {
                return Resultado.Fallo(Error.Conflicto("pale.no_abierto", "El palé de origen no existe o no está abierto."));
            }

            if (origen.PlantillaId is not null && cajas == 0)
            {
                return Resultado.Fallo(PorCajas());
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
        _repo.Agregar(MovimientoPartida.Crear(empresaId, partidaId, dia, TipoMovimientoPartida.Paletizado, -kilos, desdePaleId, "Pale", hacia?.Id ?? desdePaleId, null, _reloj, -cajas).Valor);
        _repo.Agregar(MovimientoPartida.Crear(empresaId, partidaId, dia, TipoMovimientoPartida.Paletizado, kilos, hacia?.Id, "Pale", hacia?.Id ?? desdePaleId, null, _reloj, cajas).Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task DeshacerDocumentosAsync(Guid? albaranId, Guid? cartaId)
    {
        const string motivo = "La expedición no se completó.";
        if (albaranId is { } a)
        {
            await _documentos!.AnularAlbaranAsync(a, motivo, CancellationToken.None).ConfigureAwait(false);
        }

        if (cartaId is { } c)
        {
            await _documentos!.AnularCartaPorteAsync(c, motivo, CancellationToken.None).ConfigureAwait(false);
        }
    }

    private async Task<Resultado<(Guid Id, string Numero)>> EmitirCartaPorteAsync(Guid empresaId, DatosExpedicion datos, DateOnly fecha,
        IReadOnlyList<(Guid PaleId, SaldoPartida Contenido)> cargado, CancellationToken ct, Guid? albaranId = null)
    {
        var partidas = (await _repo.PartidasAsync(cargado.Select(c => c.Contenido.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        var lineas = new List<(string, int, decimal)>();
        var detalle = new List<(Guid?, int)>();
        foreach (var grupo in cargado.GroupBy(c => partidas.GetValueOrDefault(c.Contenido.PartidaId)?.ProductoId ?? Guid.Empty))
        {
            var nombre = (await _productos.ObtenerAsync(grupo.Key, ct).ConfigureAwait(false))?.Nombre ?? "Mercancía";
            var pales = grupo.Select(c => c.PaleId).Distinct().Count();
            var cajas = grupo.Sum(c => Math.Max(c.Contenido.Cajas, 0));
            lineas.Add(($"{nombre} · {pales} palé(s)", cajas > 0 ? cajas : pales, grupo.Sum(c => c.Contenido.Kilos)));
            detalle.Add((grupo.Key == Guid.Empty ? null : grupo.Key, pales));
        }

        return await _documentos!.EmitirCartaPorteAsync(empresaId, new CartaPorteExpedicion(datos.ClienteId!.Value, fecha, datos.Transportista, datos.Matricula,
            datos.LugarOrigen, datos.LugarDestino, datos.Observaciones ?? datos.Referencia, lineas, albaranId,
            datos.TransportistaId, datos.VehiculoId, datos.TemperaturaConsigna, datos.Termografo, detalle), ct).ConfigureAwait(false);
    }

    private async Task<PaleDto> DtoAsync(Pale p, CancellationToken ct)
    {
        var contenido = await _repo.ContenidoPaleAsync(p.Id, ct).ConfigureAwait(false);
        if (p.Estado == EstadoPale.Expedido)
        {
            // Expedido: se muestra lo que llevaba al salir.
            var salidas = (await _repo.MovimientosDePaleAsync(p.Id, ct).ConfigureAwait(false))
                .Where(m => m.DocumentoTipo == DocumentoExpedicion)
                .GroupBy(m => m.PartidaId).Select(g => new SaldoPartida(g.Key, p.Id, -g.Sum(m => m.Kilos), -g.Sum(m => m.Cajas))).ToList();
            contenido = salidas;
        }

        var plantilla = p.PlantillaId is { } pid ? await _repo.PlantillaPaleAsync(pid, ct).ConfigureAwait(false) : null;
        var partidas = (await _repo.PartidasAsync(contenido.Select(c => c.PartidaId).ToList(), ct).ConfigureAwait(false)).ToDictionary(x => x.Id);
        var lineas = contenido.Where(c => c.Kilos > 0m)
            .Select(c => new ContenidoPaleDto(c.PartidaId, partidas.GetValueOrDefault(c.PartidaId)?.Codigo, partidas.GetValueOrDefault(c.PartidaId)?.ProductoId ?? Guid.Empty, c.Kilos,
                Math.Max(c.Cajas, 0)))
            .ToList();
        return new PaleDto(p.Id, p.Sscc, p.Tipo, p.Estado.ToString(), p.ClienteId, p.FechaExpedicion, p.ReferenciaExpedicion, lineas.Sum(l => l.Kilos), lineas,
            p.PlantillaId, lineas.Sum(l => l.Cajas), plantilla?.CajasPorPale, p.CartaPorteId, p.AlbaranId);
    }

    private static Resultado<T> NoEncontrado<T>() => Resultado.Fallo<T>(Error.NoEncontrado("pale.no_encontrado", "El palé no existe."));
}
