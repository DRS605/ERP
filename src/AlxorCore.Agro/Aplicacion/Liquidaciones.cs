using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record LineaClasificacionDto(Guid CategoriaId, string? Categoria, decimal KgMuestra, decimal Porcentaje);

public sealed record ClasificacionDto(Guid Id, Guid PartidaId, DateOnly Fecha, string Estado, string? Observaciones, decimal KgMuestra, IReadOnlyList<LineaClasificacionDto> Lineas);

public sealed record DatosClasificacion(IReadOnlyList<DatosLineaClasificacion> Lineas, bool Definitiva = false, DateOnly? Fecha = null, string? Observaciones = null);

public sealed record DatosLineaClasificacion(Guid CategoriaId, decimal KgMuestra);

public sealed record DatosLiquidacion(Guid AgricultorId, Guid CampanaId, DateOnly Desde, DateOnly Hasta, DateOnly? Fecha = null);

public sealed record ErrorDto(string Codigo, string Mensaje);

public sealed record LineaLiquidacionDto(
    Guid LineaRecepcionId, Guid RecepcionId, string? Recepcion, Guid PartidaId, Guid? CategoriaId, string? Categoria, DateOnly FechaRecepcion,
    decimal Kilos, decimal PrecioKg, decimal Importe, Guid PrecioId);

public sealed record DescuentoLiquidacionDto(Guid ConceptoId, string Nombre, string Tipo, decimal Valor, decimal Base, decimal Importe);

public sealed record LiquidacionDto(
    Guid Id, string? Numero, Guid AgricultorId, string? Agricultor, Guid CampanaId, DateOnly Desde, DateOnly Hasta, DateOnly Fecha, string Regimen,
    string CodigoImpuesto, decimal PorcentajeImpuesto, decimal PorcentajeRetencion, decimal Kilos, decimal Bruto, decimal TotalDescuentos,
    decimal BaseImponible, decimal CuotaImpuesto, decimal Retencion, decimal TotalFactura, decimal APagar, string Estado, Guid? GastoId,
    string? MotivoAnulacion, IReadOnlyList<LineaLiquidacionDto> Lineas, IReadOnlyList<DescuentoLiquidacionDto> Descuentos);

public sealed record LiquidacionResumenDto(
    Guid Id, string? Numero, Guid AgricultorId, string? Agricultor, DateOnly Desde, DateOnly Hasta, DateOnly Fecha, decimal Kilos, decimal BaseImponible,
    decimal APagar, string Estado);

/// <summary>Resultado de una previsualización: la valoración, o todo lo que falta para poder liquidar.</summary>
public sealed record PrevisualizacionDto(bool Valida, IReadOnlyList<ErrorDto> Errores, LiquidacionDto? Liquidacion);

/// <summary>Clasificación de partidas y liquidaciones al agricultor (autofacturación).</summary>
public sealed class LiquidacionesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IAutofacturas _autofacturas;
    private readonly IReloj _reloj;

    public LiquidacionesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IAutofacturas autofacturas, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _autofacturas = autofacturas;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Clasificación
    public async Task<IReadOnlyList<ClasificacionDto>> ClasificacionesAsync(Guid empresaId, Guid partidaId, CancellationToken ct = default)
    {
        var nombres = (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre);
        return (await _repo.ClasificacionesAsync(partidaId, ct).ConfigureAwait(false)).OrderByDescending(c => c.CreadoEn).Select(c => ClasificacionDe(c, nombres)).ToList();
    }

    public async Task<Resultado<ClasificacionDto>> ClasificarAsync(Guid empresaId, Guid partidaId, DatosClasificacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var partida = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (partida is null || partida.Anulada)
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        var categorias = (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre);
        if (datos.Lineas.Any(l => !categorias.ContainsKey(l.CategoriaId)))
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("categoria.no_encontrada", "Alguna categoría no existe."));
        }

        var c = ClasificacionPartida.Crear(empresaId, partidaId, datos.Fecha ?? Hoy, datos.Lineas.Select(l => (l.CategoriaId, l.KgMuestra)).ToList(), datos.Observaciones, _reloj);
        if (c.EsFallo)
        {
            return Resultado.Fallo<ClasificacionDto>(c.Error);
        }

        _repo.Agregar(c.Valor);
        if (datos.Definitiva)
        {
            var r = await HacerDefinitivaAsync(c.Valor, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ClasificacionDto>(r.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ClasificacionDe(c.Valor, categorias));
    }

    public async Task<Resultado<ClasificacionDto>> DefinitivaAsync(Guid empresaId, Guid clasificacionId, CancellationToken ct = default)
    {
        var c = await _repo.ClasificacionAsync(clasificacionId, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<ClasificacionDto>(Error.NoEncontrado("clasificacion.no_encontrada", "La clasificación no existe."));
        }

        var r = await HacerDefinitivaAsync(c, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ClasificacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var nombres = (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(x => x.Id, x => x.Nombre);
        return Resultado.Ok(ClasificacionDe(c, nombres));
    }

    /// <summary>La hace definitiva y sustituye a la anterior, salvo que la partida ya esté en una liquidación.</summary>
    private async Task<Resultado> HacerDefinitivaAsync(ClasificacionPartida c, CancellationToken ct)
    {
        var anterior = (await _repo.DefinitivasAsync([c.PartidaId], ct).ConfigureAwait(false)).FirstOrDefault(x => x.Id != c.Id);
        if (anterior is not null)
        {
            if (await _repo.PartidasEnLiquidacionAsync([c.PartidaId], ct).ConfigureAwait(false))
            {
                return Resultado.Fallo(Error.Conflicto("clasificacion.liquidada", "La partida ya está en una liquidación: su clasificación definitiva no se puede sustituir."));
            }

            anterior.Sustituir();
        }

        return c.HacerDefinitiva();
    }

    // ------------------------------------------------------------------ Liquidaciones
    public async Task<IReadOnlyList<LiquidacionResumenDto>> ListarAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default)
    {
        var nombres = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return (await _repo.LiquidacionesAsync(empresaId, agricultorId, ct).ConfigureAwait(false))
            .OrderByDescending(l => l.Fecha).ThenByDescending(l => l.Numero ?? int.MaxValue)
            .Select(l => new LiquidacionResumenDto(l.Id, l.NumeroCompleto, l.AgricultorId, nombres.GetValueOrDefault(l.AgricultorId), l.Desde, l.Hasta, l.Fecha, l.Kilos,
                l.BaseImponible, l.APagar, l.Estado.ToString()))
            .ToList();
    }

    public async Task<LiquidacionDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        return l is null ? null : await DtoAsync(l, ct).ConfigureAwait(false);
    }

    /// <summary>Valora sin guardar nada: la liquidación que saldría, o la lista completa de lo que falta.</summary>
    public async Task<Resultado<PrevisualizacionDto>> PrevisualizarAsync(Guid empresaId, DatosLiquidacion datos, CancellationToken ct = default)
    {
        var preparada = await PrepararAsync(empresaId, datos, null, ct).ConfigureAwait(false);
        if (preparada.EsFallo)
        {
            return Resultado.Fallo<PrevisualizacionDto>(preparada.Error);
        }

        var (agricultor, valoracion, errores) = preparada.Valor;
        if (valoracion is null)
        {
            return Resultado.Ok(new PrevisualizacionDto(false, errores.Select(e => new ErrorDto(e.Codigo, e.Mensaje)).ToList(), null));
        }

        var borrador = Liquidacion.Crear(empresaId, agricultor, datos.CampanaId, datos.Desde, datos.Hasta, datos.Fecha ?? Hoy, agricultor.CodigoImpuesto, valoracion, _reloj);
        return borrador.EsFallo
            ? Resultado.Ok(new PrevisualizacionDto(false, [new ErrorDto(borrador.Error.Codigo, borrador.Error.Mensaje)], null))
            : Resultado.Ok(new PrevisualizacionDto(true, [], await DtoAsync(borrador.Valor, ct).ConfigureAwait(false)));
    }

    /// <summary>Crea la liquidación en borrador: reserva las entregas del periodo que no estén en otra.</summary>
    public async Task<Resultado<LiquidacionDto>> CrearAsync(Guid empresaId, DatosLiquidacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        await _unidad.BloquearAsync(RecepcionesAgro.ClaveNumeracion(empresaId, Liquidacion.Serie), ct).ConfigureAwait(false);
        var preparada = await PrepararAsync(empresaId, datos, null, ct).ConfigureAwait(false);
        if (preparada.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(preparada.Error);
        }

        var (agricultor, valoracion, errores) = preparada.Valor;
        if (valoracion is null)
        {
            return Resultado.Fallo<LiquidacionDto>(Valoracion.Resumen(errores));
        }

        var l = Liquidacion.Crear(empresaId, agricultor, datos.CampanaId, datos.Desde, datos.Hasta, datos.Fecha ?? Hoy, agricultor.CodigoImpuesto, valoracion, _reloj);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(l.Error);
        }

        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(l.Valor, ct).ConfigureAwait(false));
    }

    /// <summary>Vuelve a valorar un borrador con los precios, clasificaciones y descuentos actuales (mismas entregas).</summary>
    public async Task<Resultado<LiquidacionDto>> RecalcularAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return NoEncontrada<LiquidacionDto>();
        }

        var valoracion = await RevalorarAsync(empresaId, l, ct).ConfigureAwait(false);
        if (valoracion.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(valoracion.Error);
        }

        var r = l.Recalcular(valoracion.Valor);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(l, ct).ConfigureAwait(false));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("liquidacion.no_encontrada", "La liquidación no existe."));
        }

        if (l.Estado != EstadoLiquidacion.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.no_borrador", "Solo se elimina un borrador; una liquidación emitida se anula."));
        }

        _repo.Eliminar(l);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Emite la liquidación: comprueba que el agricultor autoriza la autofacturación y que la valoración sigue
    /// vigente, la numera sin huecos (serie LIQ) y registra la autofactura como gasto con su compensación o IVA
    /// y su retención. Si algo falla después de registrar el gasto, el gasto se anula.
    /// </summary>
    public async Task<Resultado<LiquidacionDto>> EmitirAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        await _unidad.BloquearAsync(RecepcionesAgro.ClaveNumeracion(empresaId, Liquidacion.Serie), ct).ConfigureAwait(false);
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return NoEncontrada<LiquidacionDto>();
        }

        if (l.Estado != EstadoLiquidacion.Borrador)
        {
            return Resultado.Fallo<LiquidacionDto>(Error.Conflicto("liquidacion.no_borrador", "La liquidación ya está emitida o anulada."));
        }

        var agricultor = (await _repo.AgricultorAsync(l.AgricultorId, ct).ConfigureAwait(false))!;
        if (agricultor.AutofacturacionDesde is not { } desde || desde > l.Fecha)
        {
            return Resultado.Fallo<LiquidacionDto>(Error.Conflicto("liquidacion.sin_autofacturacion",
                "El agricultor no ha autorizado la autofacturación en esa fecha (art. 5 del Reglamento de facturación): registra la fecha del acuerdo en su ficha."));
        }

        if (agricultor.Bloqueado)
        {
            return Resultado.Fallo<LiquidacionDto>(Error.Conflicto("agricultor.bloqueado", $"El agricultor está bloqueado: {agricultor.MotivoBloqueo}"));
        }

        var actual = await RevalorarAsync(empresaId, l, ct).ConfigureAwait(false);
        if (actual.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(actual.Error);
        }

        if (actual.Valor.APagar != l.APagar || actual.Valor.Base != l.BaseImponible || actual.Valor.Lineas.Count != l.Lineas.Count)
        {
            return Resultado.Fallo<LiquidacionDto>(Error.Conflicto("liquidacion.desactualizada",
                "Han cambiado precios, clasificaciones o descuentos desde que se calculó: recalcula la liquidación y revísala antes de emitirla."));
        }

        var numero = await _repo.UltimoNumeroAsync(empresaId, Liquidacion.Serie, l.Ejercicio, ct).ConfigureAwait(false) + 1;
        var numeroCompleto = $"{Liquidacion.Serie}-{l.Ejercicio}-{numero:D6}";
        var campana = await _repo.CampanaAsync(l.CampanaId, ct).ConfigureAwait(false);
        var gasto = await _autofacturas.RegistrarAsync(empresaId, new AutofacturaAgro(agricultor.ProveedorId,
            $"Liquidación {numeroCompleto} · campaña {campana?.Codigo} (autofactura)", l.Fecha, l.BaseImponible, l.CodigoImpuesto, l.PorcentajeRetencion), ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(gasto.Error);
        }

        try
        {
            if (gasto.Valor.Total != l.APagar)
            {
                throw new InvalidOperationException($"La autofactura ({gasto.Valor.Total}) no cuadra con la liquidación ({l.APagar}).");
            }

            l.Emitir(numero, gasto.Valor.GastoId, _reloj);

            // Coste por kilo de la fruta recibida: el importe liquidado de cada partida entre sus kilos.
            var partidas = await _repo.PartidasAsync(l.Lineas.Select(x => x.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false);
            foreach (var p in partidas)
            {
                var lineas = l.Lineas.Where(x => x.PartidaId == p.Id).ToList();
                var kilos = lineas.Sum(x => x.Kilos);
                p.FijarCoste(kilos == 0m ? null : decimal.Round(lineas.Sum(x => x.Importe) / kilos, 6));
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }
        catch
        {
            await _autofacturas.AnularAsync(gasto.Valor.GastoId, CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        return Resultado.Ok(await DtoAsync(l, ct).ConfigureAwait(false));
    }

    /// <summary>Anula una liquidación emitida y su autofactura (si no está pagada). Sus entregas quedan libres.</summary>
    public async Task<Resultado<LiquidacionDto>> AnularAsync(Guid id, string? motivo, CancellationToken ct = default)
    {
        var l = await _repo.LiquidacionAsync(id, ct).ConfigureAwait(false);
        if (l is null)
        {
            return NoEncontrada<LiquidacionDto>();
        }

        var r = l.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(r.Error);
        }

        var gasto = await _autofacturas.AnularAsync(l.GastoId!.Value, ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<LiquidacionDto>(gasto.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(l, ct).ConfigureAwait(false));
    }

    // ------------------------------------------------------------------ Valoración
    private async Task<Resultado<(Agricultor Agricultor, ValoracionLiquidacion? Valoracion, IReadOnlyList<Error> Errores)>> PrepararAsync(
        Guid empresaId, DatosLiquidacion datos, Liquidacion? existente, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var agricultor = await _repo.AgricultorAsync(datos.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor is null)
        {
            return Resultado.Fallo<(Agricultor, ValoracionLiquidacion?, IReadOnlyList<Error>)>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (await _repo.CampanaAsync(datos.CampanaId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<(Agricultor, ValoracionLiquidacion?, IReadOnlyList<Error>)>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        if (datos.Hasta < datos.Desde)
        {
            return Resultado.Fallo<(Agricultor, ValoracionLiquidacion?, IReadOnlyList<Error>)>(Error.Validacion("liquidacion.fechas", "El periodo termina antes de empezar."));
        }

        var lineas = await LineasPendientesAsync(empresaId, agricultor.Id, datos.CampanaId, datos.Desde, datos.Hasta, existente, ct).ConfigureAwait(false);
        var valoracion = await ValorarAsync(empresaId, datos.CampanaId, lineas, agricultor, ct).ConfigureAwait(false);
        return Resultado.Ok<(Agricultor, ValoracionLiquidacion?, IReadOnlyList<Error>)>((agricultor, valoracion.Valoracion, valoracion.Errores));
    }

    private async Task<Resultado<ValoracionLiquidacion>> RevalorarAsync(Guid empresaId, Liquidacion l, CancellationToken ct)
    {
        var agricultor = (await _repo.AgricultorAsync(l.AgricultorId, ct).ConfigureAwait(false))!;
        var lineas = await LineasPendientesAsync(empresaId, l.AgricultorId, l.CampanaId, l.Desde, l.Hasta, l, ct).ConfigureAwait(false);
        var ids = l.Lineas.Select(x => x.LineaRecepcionId).ToHashSet();
        var (valoracion, errores) = await ValorarAsync(empresaId, l.CampanaId, lineas.Where(x => ids.Contains(x.LineaRecepcionId)).ToList(), agricultor, ct).ConfigureAwait(false);
        return valoracion is null ? Resultado.Fallo<ValoracionLiquidacion>(Valoracion.Resumen(errores)) : Resultado.Ok(valoracion);
    }

    private async Task<(ValoracionLiquidacion? Valoracion, IReadOnlyList<Error> Errores)> ValorarAsync(
        Guid empresaId, Guid campanaId, IReadOnlyList<LineaALiquidar> lineas, Agricultor agricultor, CancellationToken ct)
    {
        var precios = await PreciosAsync(empresaId, campanaId, lineas.Select(l => l.PartidaId).ToList(), ct).ConfigureAwait(false);
        var conceptos = (await _repo.ConceptosAsync(empresaId, ct).ConfigureAwait(false)).Where(c => c.ValeParaAgricultor(agricultor.Id)).ToList();
        var porcentaje = AlxorCore.Nucleo.Comun.Impuesto.PorCodigoImpuesto(agricultor.CodigoImpuesto).Valor.Porcentaje;
        var r = Valoracion.Calcular(lineas, conceptos, porcentaje, agricultor.PorcentajeRetencion, precios, out var errores);
        return (r.EsCorrecto ? r.Valor : null, errores);
    }

    /// <summary>Entregas confirmadas del agricultor en la campaña y el periodo que no están en otra liquidación.</summary>
    private async Task<IReadOnlyList<LineaALiquidar>> LineasPendientesAsync(
        Guid empresaId, Guid agricultorId, Guid campanaId, DateOnly desde, DateOnly hasta, Liquidacion? propia, CancellationToken ct)
    {
        var recepciones = (await _repo.RecepcionesAsync(empresaId, desde, hasta, agricultorId, ct).ConfigureAwait(false))
            .Where(r => r.Estado == EstadoRecepcion.Confirmada && r.CampanaId == campanaId).ToList();
        var candidatas = recepciones.SelectMany(r => r.Lineas.Select(l => (r, l))).ToList();
        var ocupadas = await _repo.LineasEnLiquidacionAsync(candidatas.Select(x => x.l.Id).ToList(), ct).ConfigureAwait(false);
        var propias = propia?.Lineas.Select(x => x.LineaRecepcionId).ToHashSet() ?? [];
        return candidatas
            .Where(x => !ocupadas.Contains(x.l.Id) || propias.Contains(x.l.Id))
            .OrderBy(x => x.r.Fecha).ThenBy(x => x.r.Numero).ThenBy(x => x.l.NumeroLinea)
            .Select(x => new LineaALiquidar(x.l.Id, x.r.Id, x.l.PartidaId!.Value, $"{x.r.NumeroCompleto} línea {x.l.NumeroLinea} ({x.l.ProductoNombre})",
                x.l.ProductoId, x.r.Fecha, x.l.NetoKg ?? 0m, x.l.EnvaseProductoId, x.l.Envases ?? 0))
            .ToList();
    }

    private async Task<IPreciosLiquidacion> PreciosAsync(Guid empresaId, Guid campanaId, IReadOnlyCollection<Guid> partidaIds, CancellationToken ct)
    {
        var articulos = await _repo.ArticulosCampanaAsync(campanaId, ct).ConfigureAwait(false);
        var precios = await _repo.PreciosAsync(campanaId, ct).ConfigureAwait(false);
        var categorias = (await _repo.CategoriasAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(c => c.Id);
        var definitivas = await _repo.DefinitivasAsync(partidaIds, ct).ConfigureAwait(false);
        return new PreciosCampana(articulos, precios, definitivas, categorias);
    }

    private sealed class PreciosCampana : IPreciosLiquidacion
    {
        private readonly IReadOnlyList<ArticuloCampana> _articulos;
        private readonly IReadOnlyList<PrecioLiquidacion> _precios;
        private readonly Dictionary<Guid, IReadOnlyList<MuestraCategoria>> _clasificaciones;

        public PreciosCampana(IReadOnlyList<ArticuloCampana> articulos, IReadOnlyList<PrecioLiquidacion> precios, IReadOnlyList<ClasificacionPartida> definitivas,
            IReadOnlyDictionary<Guid, Categoria> categorias)
        {
            _articulos = articulos;
            _precios = precios;
            _clasificaciones = definitivas.ToDictionary(d => d.PartidaId, d => (IReadOnlyList<MuestraCategoria>)d.Lineas
                .Select(l => (l, c: categorias.GetValueOrDefault(l.CategoriaId)))
                .OrderBy(x => x.c?.Orden ?? int.MaxValue).ThenBy(x => x.c?.Codigo, StringComparer.Ordinal)
                .Select(x => new MuestraCategoria(x.l.CategoriaId, x.c?.Nombre ?? "?", x.l.KgMuestra)).ToList());
        }

        public MetodoLiquidacion? Metodo(Guid productoId) => _articulos.FirstOrDefault(a => a.ProductoId == productoId)?.Metodo;

        public IReadOnlyList<MuestraCategoria>? Clasificacion(Guid partidaId) => _clasificaciones.GetValueOrDefault(partidaId);

        public PrecioAplicable? Precio(Guid productoId, Guid? categoriaId, DateOnly fecha) => Precio(productoId, categoriaId, fecha, null);

        // Día > periodo > general; en cada tipo, el del envase de la entrega antes que el general.
        public PrecioAplicable? Precio(Guid productoId, Guid? categoriaId, DateOnly fecha, Guid? envaseProductoId) =>
            _precios.Where(p => p.ProductoId == productoId && p.CategoriaId == categoriaId && p.Vigente(fecha)
                                && (p.EnvaseProductoId is null || p.EnvaseProductoId == envaseProductoId))
                .OrderByDescending(p => p.Prioridad).ThenBy(p => p.Hasta.DayNumber - p.Desde.DayNumber)
                .Select(p => new PrecioAplicable(p.Id, p.PrecioKg)).FirstOrDefault();
    }

    // ------------------------------------------------------------------ DTO
    private async Task<LiquidacionDto> DtoAsync(Liquidacion l, CancellationToken ct)
    {
        var agricultor = await _repo.AgricultorAsync(l.AgricultorId, ct).ConfigureAwait(false);
        var categorias = (await _repo.CategoriasAsync(l.EmpresaId, ct).ConfigureAwait(false)).ToDictionary(c => c.Id, c => c.Nombre);
        var recepciones = (await _repo.RecepcionesAsync(l.Lineas.Select(x => x.RecepcionId).Distinct().ToList(), ct).ConfigureAwait(false))
            .ToDictionary(r => r.Id, r => r.NumeroCompleto);
        return new LiquidacionDto(l.Id, l.NumeroCompleto, l.AgricultorId, agricultor?.Nombre, l.CampanaId, l.Desde, l.Hasta, l.Fecha, l.Regimen.ToString(),
            l.CodigoImpuesto, l.PorcentajeImpuesto, l.PorcentajeRetencion, l.Kilos, l.Bruto, l.TotalDescuentos, l.BaseImponible, l.CuotaImpuesto, l.Retencion,
            l.TotalFactura, l.APagar, l.Estado.ToString(), l.GastoId, l.MotivoAnulacion,
            l.Lineas.OrderBy(x => x.FechaRecepcion).ThenBy(x => recepciones.GetValueOrDefault(x.RecepcionId), StringComparer.Ordinal)
                .Select(x => new LineaLiquidacionDto(x.LineaRecepcionId, x.RecepcionId, recepciones.GetValueOrDefault(x.RecepcionId), x.PartidaId, x.CategoriaId,
                    x.CategoriaId is { } c ? categorias.GetValueOrDefault(c) : null, x.FechaRecepcion, x.Kilos, x.PrecioKg, x.Importe, x.PrecioId)).ToList(),
            l.Descuentos.Select(d => new DescuentoLiquidacionDto(d.ConceptoId, d.Nombre, d.Tipo.ToString(), d.Valor, d.Base, d.Importe)).ToList());
    }

    private static ClasificacionDto ClasificacionDe(ClasificacionPartida c, IReadOnlyDictionary<Guid, string> nombres) => new(
        c.Id, c.PartidaId, c.Fecha, c.Estado.ToString(), c.Observaciones, c.KgMuestra,
        c.Lineas.Select(l => new LineaClasificacionDto(l.CategoriaId, nombres.GetValueOrDefault(l.CategoriaId), l.KgMuestra,
            c.KgMuestra == 0m ? 0m : decimal.Round(l.KgMuestra * 100m / c.KgMuestra, 2))).ToList());

    private static Resultado<T> NoEncontrada<T>() => Resultado.Fallo<T>(Error.NoEncontrado("liquidacion.no_encontrada", "La liquidación no existe."));
}
