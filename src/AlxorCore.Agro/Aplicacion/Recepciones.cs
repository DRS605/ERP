using AlxorCore.Nucleo.Comun;
using AlxorCore.Agro.Dominio;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record PesadaDto(Guid Id, Guid LineaId, int Secuencia, decimal BrutoKg, decimal TaraKg, decimal NetoKg, int Envases, string? Bascula, decimal? TaraCamionKg = null,
    decimal TaraEnvasesKg = 0m, Guid? GrupoCamion = null, decimal? BrutoCamionKg = null);

public sealed record LineaRecepcionDto(
    Guid Id, int NumeroLinea, Guid ProductoId, string ProductoNombre, Guid? ParcelaId, DateOnly? FechaRecoleccion, Guid? EnvaseProductoId,
    decimal? PrecioEstimadoKg, string? Calibre, decimal NetoKg, int Envases, Guid? PartidaId, string? MotivoDescalificacion = null, decimal? KilosLiquidacion = null,
    string? MotivoKilosLiquidacion = null, int Pales = 0);

public sealed record RecepcionDto(
    Guid Id, string? Numero, DateOnly Fecha, Guid AgricultorId, string? AgricultorNombre, Guid CampanaId, string? Matricula, string? Conductor,
    string? Observaciones, string Estado, string? MotivoAnulacion, decimal NetoKg, IReadOnlyList<LineaRecepcionDto> Lineas, IReadOnlyList<PesadaDto> Pesadas,
    IReadOnlyList<EnvasePesadaDto> EnvasesPesadas, IReadOnlyList<PaleEntradaDto> PalesEntrada, IReadOnlyList<RectificacionDto> Rectificaciones)
{
    public static RecepcionDto De(Recepcion r, string? agricultor, IReadOnlyList<RectificacionRecepcion>? rectificaciones = null) => new(
        r.Id, r.NumeroCompleto, r.Fecha, r.AgricultorId, agricultor, r.CampanaId, r.Matricula, r.Conductor, r.Observaciones, r.Estado.ToString(),
        r.MotivoAnulacion, r.NetoKg,
        r.Lineas.OrderBy(l => l.NumeroLinea).Select(l => new LineaRecepcionDto(l.Id, l.NumeroLinea, l.ProductoId, l.ProductoNombre, l.ParcelaId,
            l.FechaRecoleccion, l.EnvaseProductoId, l.PrecioEstimadoKg, l.Calibre, l.NetoKg ?? r.NetoDe(l.Id), l.Envases ?? r.EnvasesDe(l.Id), l.PartidaId,
            l.MotivoDescalificacion, l.KilosLiquidacion, l.MotivoKilosLiquidacion, r.PalesEntrada.Count(p => p.LineaId == l.Id))).ToList(),
        r.Pesadas.OrderBy(p => p.Secuencia).Select(p => new PesadaDto(p.Id, p.LineaId, p.Secuencia, p.BrutoKg, p.TaraKg, p.NetoKg, p.Envases, p.Bascula, p.TaraCamionKg,
            p.TaraEnvasesKg, p.GrupoCamion, p.BrutoCamionKg)).ToList(),
        r.EnvasesPesadas.Select(e => new EnvasePesadaDto(e.Id, e.PesadaId, e.EnvaseProductoId, e.Cantidad, e.TaraUnitariaKg, e.TaraKg, e.TaraEnvaseId)).ToList(),
        r.PalesEntrada.OrderBy(p => p.LineaId).ThenBy(p => p.Numero).Select(p => new PaleEntradaDto(p.Id, p.LineaId, p.Numero, p.SerieOrigen, p.EnvaseProductoId, p.Envases,
            p.KilosNetos, p.PaleId, p.KilosAsignados)).ToList(),
        (rectificaciones ?? []).OrderBy(x => x.CreadaEn).Select(x => new RectificacionDto(x.Id, x.LineaRecepcionId, x.PartidaId, x.Fecha, x.NetoAnteriorKg, x.DiferenciaKg,
            x.NetoNuevoKg, x.KilosLiquidacion, x.BrutoKg, x.TaraKg, x.Motivo, x.UsuarioId, x.CreadaEn)).ToList());
}

public sealed record RecepcionResumenDto(Guid Id, string? Numero, DateOnly Fecha, Guid AgricultorId, string? AgricultorNombre, string Estado, int Lineas, decimal NetoKg);

public sealed record PartidaDto(
    Guid Id, string Codigo, Guid ProductoId, string Origen, DateOnly Fecha, decimal KilosIniciales, decimal Saldo, Guid? AgricultorId, Guid? ParcelaId,
    Guid? CampanaId, string? Calibre, decimal? CosteKg, bool Anulada, Guid? RecepcionId, Guid? ParteConfeccionId, string Certificaciones = "Ninguna");

public sealed record MovimientoPartidaDto(Guid Id, Guid PartidaId, DateOnly Fecha, string Tipo, decimal Kilos, Guid? PaleId, string? DocumentoTipo, Guid? DocumentoId, string? Concepto);

public sealed record SaldoEnvaseDto(Guid EnvaseProductoId, int Saldo);

public sealed record MovimientoEnvaseDto(Guid Id, Guid EnvaseProductoId, DateOnly Fecha, int Cantidad, Guid? RecepcionId, string? Concepto);

public sealed record DatosRecepcion(Guid AgricultorId, DateOnly Fecha, Guid? CampanaId = null, string? Matricula = null, string? Conductor = null, string? Observaciones = null);

public sealed record DatosLinea(Guid ProductoId, Guid? ParcelaId = null, DateOnly? FechaRecoleccion = null, Guid? EnvaseProductoId = null, decimal? PrecioEstimadoKg = null, string? Calibre = null,
    string? MotivoDescalificacion = null, decimal? KilosLiquidacion = null, string? MotivoKilosLiquidacion = null);

/// <summary>Envases de un tipo contados en la pesada; sin tara unitaria se aplica la vigente del envase en la fecha de la recepción.</summary>
public sealed record DatosEnvasePesada(Guid EnvaseProductoId, int Cantidad, decimal? TaraUnitariaKg = null);

/// <summary>
/// Pesada de báscula. Completa (recomendada): bruto, <paramref name="TaraCamionKg"/> y <paramref name="EnvasesPorTipo"/>
/// (la tara se calcula). Simple: bruto y tara total tecleada, con los envases del envase de la línea.
/// </summary>
public sealed record DatosPesada(decimal BrutoKg, decimal TaraKg = 0m, int Envases = 0, string? Bascula = null, decimal? TaraCamionKg = null,
    IReadOnlyList<DatosEnvasePesada>? EnvasesPorTipo = null);

/// <summary>Pesada del camión entero: un bruto, la tara del camión y los envases contados de cada línea.</summary>
public sealed record DatosPesadaCamion(decimal BrutoKg, decimal TaraCamionKg, IReadOnlyList<DatosLineaPesadaCamion> Lineas, string? Bascula = null);

/// <summary>Una línea en la pesada del camión: sus envases contados y, si se reparte por kilos, los kilos que se declaran de ella.</summary>
public sealed record DatosLineaPesadaCamion(Guid LineaId, IReadOnlyList<DatosEnvasePesada> EnvasesPorTipo, decimal? KilosDeclarados = null);

public sealed record DatosPaleEntrada(string? SerieOrigen = null, Guid? EnvaseProductoId = null, int Envases = 0, decimal? KilosNetos = null);

public sealed record DatosEtiquetasCampo(Guid AgricultorId, int Cantidad, Guid? ParcelaId = null);

public sealed record EtiquetaCampoDto(Guid Id, string Sscc, Guid AgricultorId, Guid? ParcelaId, DateTimeOffset EmitidaEn, Guid? PaleId);

public sealed record DatosKilosLiquidacion(decimal? Kilos, string? Motivo);

/// <summary>Rectificación de una línea confirmada: el neto real correcto y/o los kilos de liquidación nuevos, con motivo (y la nueva pesada, si la hay).</summary>
public sealed record DatosRectificacion(string? Motivo, decimal? NetoKg = null, decimal? KilosLiquidacion = null, decimal? BrutoKg = null, decimal? TaraKg = null,
    DateOnly? Fecha = null);

public sealed record RectificacionDto(Guid Id, Guid LineaRecepcionId, Guid PartidaId, DateOnly Fecha, decimal NetoAnteriorKg, decimal DiferenciaKg, decimal NetoNuevoKg,
    decimal? KilosLiquidacion, decimal? BrutoKg, decimal? TaraKg, string Motivo, Guid? UsuarioId, DateTimeOffset CreadaEn);

public sealed record EnvasePesadaDto(Guid Id, Guid PesadaId, Guid EnvaseProductoId, int Cantidad, decimal TaraUnitariaKg, decimal TaraKg, Guid? TaraEnvaseId);

public sealed record PaleEntradaDto(Guid Id, Guid LineaId, int Numero, string? SerieOrigen, Guid? EnvaseProductoId, int Envases, decimal? KilosNetos, Guid? PaleId,
    decimal? KilosAsignados);

public sealed record DatosMovimientoEnvase(Guid EnvaseProductoId, int Cantidad, DateOnly? Fecha = null, string? Concepto = null);

public sealed record DatosAjustePartida(decimal Kilos, string? Concepto, Guid? PaleId = null, DateOnly? Fecha = null);

/// <summary>Recepciones de fruta, partidas y envases de los agricultores.</summary>
public sealed class RecepcionesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IConsultaProductos _productos;
    private readonly IReloj _reloj;
    private readonly EnvasesTerceros? _envases;
    private readonly CuadernoCampoAgro? _cuaderno;
    private readonly TarasAgro? _taras;

    public RecepcionesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IConsultaProductos productos, IReloj reloj, EnvasesTerceros? envases = null,
        CuadernoCampoAgro? cuaderno = null, TarasAgro? taras = null)
    {
        _taras = taras;
        _cuaderno = cuaderno;
        _envases = envases;
        _repo = repo;
        _unidad = unidad;
        _productos = productos;
        _reloj = reloj;
    }

    public static string ClaveNumeracion(Guid empresaId, string serie) => $"alxor.agro.{serie}:{empresaId}";

    public async Task<IReadOnlyList<RecepcionResumenDto>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? agricultorId, CancellationToken ct = default)
    {
        var lista = await _repo.RecepcionesAsync(empresaId, desde, hasta, agricultorId, ct).ConfigureAwait(false);
        var nombres = (await _repo.AgricultoresAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return lista.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Numero ?? int.MaxValue)
            .Select(r => new RecepcionResumenDto(r.Id, r.NumeroCompleto, r.Fecha, r.AgricultorId, nombres.GetValueOrDefault(r.AgricultorId), r.Estado.ToString(), r.Lineas.Count, r.NetoKg))
            .ToList();
    }

    public async Task<RecepcionDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(id, ct).ConfigureAwait(false);
        return r is null ? null : await DtoAsync(r, ct).ConfigureAwait(false);
    }

    private async Task<RecepcionDto> DtoAsync(Recepcion r, CancellationToken ct) =>
        RecepcionDto.De(r, (await _repo.AgricultorAsync(r.AgricultorId, ct).ConfigureAwait(false))?.Nombre,
            await _repo.RectificacionesAsync(r.Lineas.Select(l => l.Id).ToList(), ct).ConfigureAwait(false));

    /// <summary>
    /// Rectifica una línea ya confirmada (aunque sus kilos ya se hayan confeccionado o vendido): corrige su neto real con un
    /// movimiento compensatorio sobre la misma partida (y sus palés de entrada) y/o sus kilos de liquidación. No se toca
    /// la recepción ni sus pesadas originales. Si la entrega está en una liquidación viva, antes hay que anularla.
    /// </summary>
    public async Task<Resultado<RecepcionDto>> RectificarAsync(Guid recepcionId, Guid lineaId, DatosRectificacion d, Guid? usuarioId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var linea = r.Lineas.FirstOrDefault(l => l.Id == lineaId);
        if (r.Estado != EstadoRecepcion.Confirmada || linea?.PartidaId is not { } partidaId)
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("rectificacion.no_confirmada",
                "Solo se rectifica una línea de una recepción confirmada (un borrador se corrige directamente)."));
        }

        var partida = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (partida is null || partida.Anulada)
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("rectificacion.partida_anulada", "La partida de la línea está anulada."));
        }

        if ((await _repo.LineasEnLiquidacionAsync([lineaId], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("rectificacion.liquidada",
                "La entrega está en una liquidación: anúlala (o elimínala si es un borrador), rectifica y vuelve a liquidar."));
        }

        await _unidad.BloquearAsync($"alxor.agro.rectificacion:{partidaId}", ct).ConfigureAwait(false);
        var previas = await _repo.RectificacionesAsync([lineaId], ct).ConfigureAwait(false);
        var netoActual = (linea.NetoKg ?? 0m) + previas.Sum(x => x.DiferenciaKg);
        var fecha = d.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var rect = RectificacionRecepcion.Crear(r.EmpresaId, r.Id, lineaId, partidaId, fecha, netoActual, d.NetoKg, d.KilosLiquidacion, d.BrutoKg, d.TaraKg, d.Motivo, usuarioId,
            _reloj.AhoraUtc);
        if (rect.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(rect.Error);
        }

        var diferencia = rect.Valor.DiferenciaKg;
        if (diferencia != 0m)
        {
            // Sobre los palés de entrada (en proporción a lo que queda en cada uno) o, si no hay, sobre los kilos sueltos.
            var saldos = (await _repo.SaldosAsync([partidaId], ct).ConfigureAwait(false)).Where(x => x.Kilos > 0m).ToList();
            var pales = r.PalesEntrada.Where(p => p.LineaId == lineaId && p.PaleId is not null).Select(p => p.PaleId!.Value).ToHashSet();
            var destinos = pales.Count == 0
                ? [(PaleId: (Guid?)null, Kilos: saldos.Where(x => x.PaleId is null).Sum(x => x.Kilos))]
                : saldos.Where(x => x.PaleId is { } pid && pales.Contains(pid)).Select(x => (PaleId: x.PaleId, x.Kilos)).ToList();
            if (diferencia > 0m && destinos.All(x => x.Kilos <= 0m))
            {
                destinos = [(null, 0m)]; // los palés ya salieron: lo que faltaba entra suelto
            }

            var disponible = destinos.Sum(x => x.Kilos);
            if (diferencia < 0m && -diferencia > disponible)
            {
                return Resultado.Fallo<RecepcionDto>(Error.Conflicto("rectificacion.sin_saldo",
                    $"De la partida {partida.Codigo} quedan {Redondeo.Formatear(disponible, 3)} kg en almacén y se quitan {Redondeo.Formatear(-diferencia, 3)}: " +
                    "el resto ya se confeccionó o se vendió. Rectifica lo que queda y regulariza la diferencia en el parte o la expedición que lo usó."));
            }

            var resto = diferencia;
            for (var i = 0; i < destinos.Count; i++)
            {
                var kilos = i == destinos.Count - 1 ? resto
                    : Math.Round(disponible == 0m ? diferencia / destinos.Count : diferencia * destinos[i].Kilos / disponible, 3, MidpointRounding.AwayFromZero);
                resto -= kilos;
                if (kilos != 0m)
                {
                    _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, partidaId, fecha, TipoMovimientoPartida.Rectificacion, kilos, destinos[i].PaleId, "Rectificacion",
                        rect.Valor.Id, $"Rectificación de {r.NumeroCompleto} línea {linea.NumeroLinea}", _reloj).Valor);
                }
            }
        }

        _repo.Agregar(rect.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<RecepcionDto>> CrearAsync(Guid empresaId, DatosRecepcion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var agricultor = await _repo.AgricultorAsync(datos.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (agricultor.Bloqueado)
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("agricultor.bloqueado", $"El agricultor está bloqueado: {agricultor.MotivoBloqueo}"));
        }

        var campana = await CampanaDeAsync(empresaId, datos.CampanaId, datos.Fecha, ct).ConfigureAwait(false);
        if (campana.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(campana.Error);
        }

        var r = Recepcion.Crear(empresaId, agricultor.Id, campana.Valor.Id, datos.Fecha, datos.Matricula, datos.Conductor, datos.Observaciones, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(RecepcionDto.De(r.Valor, agricultor.Nombre));
    }

    public async Task<Resultado<RecepcionDto>> AgregarLineaAsync(Guid recepcionId, DatosLinea datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var producto = await _productos.ObtenerAsync(datos.ProductoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("producto.no_encontrado", "El producto no existe."));
        }

        if (!EsKilo(producto.Unidad))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Validacion("recepcion.unidad", $"{producto.Nombre} se mide en «{producto.Unidad}»: la fruta se recibe en kilos (unidad «kg»)."));
        }

        if (datos.ParcelaId is { } parcelaId)
        {
            var parcela = await _repo.ParcelaAsync(parcelaId, ct).ConfigureAwait(false);
            if (parcela is null || parcela.AgricultorId != r.AgricultorId || !parcela.Activa)
            {
                return Resultado.Fallo<RecepcionDto>(Error.Validacion("recepcion.parcela", "La parcela no es del agricultor o está inactiva."));
            }
        }

        if (datos.EnvaseProductoId is { } envaseId && await _productos.ObtenerAsync(envaseId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo<RecepcionDto>(Error.NoEncontrado("recepcion.envase", "El artículo de envase no existe."));
        }

        var linea = r.AgregarLinea(new DatosLineaRecepcion(producto.Id, producto.Nombre, datos.ParcelaId, datos.FechaRecoleccion, datos.EnvaseProductoId,
            datos.PrecioEstimadoKg, datos.Calibre, datos.MotivoDescalificacion, datos.KilosLiquidacion, datos.MotivoKilosLiquidacion));
        return await GuardarAsync(r, linea.EsFallo ? Resultado.Fallo(linea.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> QuitarLineaAsync(Guid recepcionId, Guid lineaId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.QuitarLinea(lineaId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> AgregarPesadaAsync(Guid recepcionId, Guid lineaId, DatosPesada datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        Resultado<Pesada> p;
        if (datos.EnvasesPorTipo is { } tipos)
        {
            if (datos.TaraCamionKg is not { } taraCamion)
            {
                return Resultado.Fallo<RecepcionDto>(Error.Validacion("pesada.tara_camion", "En la pesada completa indica la tara del camión (pesado vacío)."));
            }

            var contados = await ContarEnvasesAsync(r, tipos, ct).ConfigureAwait(false);
            if (contados.EsFallo)
            {
                return Resultado.Fallo<RecepcionDto>(contados.Error);
            }

            p = r.AgregarPesadaCompleta(lineaId, datos.BrutoKg, taraCamion, contados.Valor, datos.Bascula);
        }
        else
        {
            p = r.AgregarPesada(lineaId, datos.BrutoKg, datos.TaraKg, datos.Envases, datos.Bascula);
        }

        return await GuardarAsync(r, p.EsFallo ? Resultado.Fallo(p.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    /// <summary>Pesada del camión entero con varios productos: el neto se reparte entre las líneas en proporción a sus envases.</summary>
    public async Task<Resultado<RecepcionDto>> AgregarPesadaCamionAsync(Guid recepcionId, DatosPesadaCamion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var lineas = new List<(Guid, IReadOnlyList<EnvaseContado>)>();
        foreach (var l in datos.Lineas ?? [])
        {
            var contados = await ContarEnvasesAsync(r, l.EnvasesPorTipo ?? [], ct).ConfigureAwait(false);
            if (contados.EsFallo)
            {
                return Resultado.Fallo<RecepcionDto>(contados.Error);
            }

            lineas.Add((l.LineaId, contados.Valor));
        }

        var declarados = (datos.Lineas ?? []).Where(l => l.KilosDeclarados is not null).ToDictionary(l => l.LineaId, l => l.KilosDeclarados!.Value);
        var p = r.AgregarPesadaCamion(datos.BrutoKg, datos.TaraCamionKg, lineas, datos.Bascula, declarados.Count > 0 ? declarados : null);
        return await GuardarAsync(r, p.EsFallo ? Resultado.Fallo(p.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    /// <summary>Los envases contados con su tara: la indicada a mano o la vigente el día de la recepción.</summary>
    private async Task<Resultado<IReadOnlyList<EnvaseContado>>> ContarEnvasesAsync(Recepcion r, IReadOnlyList<DatosEnvasePesada> tipos, CancellationToken ct)
    {
        var contados = new List<EnvaseContado>();
        foreach (var t in tipos)
        {
            if (t.TaraUnitariaKg is { } manual)
            {
                contados.Add(new EnvaseContado(t.EnvaseProductoId, t.Cantidad, manual, null));
                continue;
            }

            var vigente = _taras is null ? null : await _taras.VigenteAsync(r.EmpresaId, t.EnvaseProductoId, r.Fecha, ct).ConfigureAwait(false);
            if (vigente is null)
            {
                var nombre = (await _productos.ObtenerAsync(t.EnvaseProductoId, ct).ConfigureAwait(false))?.Nombre ?? "el envase";
                return Resultado.Fallo<IReadOnlyList<EnvaseContado>>(Error.Validacion("tara.falta",
                    $"No hay tara de {nombre} vigente el {r.Fecha:dd/MM/yyyy}: dala de alta en las taras de envases (o indica la tara unitaria)."));
            }

            contados.Add(new EnvaseContado(t.EnvaseProductoId, t.Cantidad, vigente.TaraKg, vigente.Id));
        }

        return Resultado.Ok<IReadOnlyList<EnvaseContado>>(contados);
    }

    public async Task<Resultado<RecepcionDto>> AgregarPaleEntradaAsync(Guid recepcionId, Guid lineaId, DatosPaleEntrada datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        // Si la serie leída es una etiqueta de campo nuestra, se guarda su SSCC: tiene que ser de este agricultor (y de la
        // parcela de la línea, si la etiqueta es de una parcela) y no estar usada.
        var serie = datos.SerieOrigen;
        if (EtiquetaCampo.SsccDeLectura(serie) is { } sscc && await _repo.EtiquetaCampoAsync(r.EmpresaId, sscc, ct).ConfigureAwait(false) is { } etiqueta)
        {
            var linea = r.Lineas.FirstOrDefault(l => l.Id == lineaId);
            if (etiqueta.AgricultorId != r.AgricultorId || (etiqueta.ParcelaId is { } parcela && linea is not null && linea.ParcelaId != parcela))
            {
                return Resultado.Fallo<RecepcionDto>(Error.Conflicto("etiqueta_campo.otro_origen",
                    $"La etiqueta {sscc} se emitió para otro agricultor o parcela: no puede entrar en esta línea."));
            }

            if (etiqueta.Usada)
            {
                return Resultado.Fallo<RecepcionDto>(Error.Conflicto("etiqueta_campo.usada", $"La etiqueta {sscc} ya se usó en otra recepción."));
            }

            serie = sscc;
        }

        var p = r.AgregarPaleEntrada(lineaId, serie, datos.EnvaseProductoId, datos.Envases, datos.KilosNetos);
        return await GuardarAsync(r, p.EsFallo ? Resultado.Fallo(p.Error) : Resultado.Ok(), ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<EtiquetaCampoDto>> EtiquetasCampoAsync(Guid empresaId, Guid? agricultorId, bool soloLibres, CancellationToken ct = default) =>
        (await _repo.EtiquetasCampoAsync(empresaId, agricultorId, soloLibres, ct).ConfigureAwait(false)).Select(DtoEtiqueta).ToList();

    /// <summary>
    /// Emite etiquetas SSCC para un agricultor (y parcela) antes de que llegue la fruta, para imprimirlas en la finca. Salen
    /// del mismo contador que los palés, así que nunca coinciden con otro SSCC.
    /// </summary>
    public async Task<Resultado<IReadOnlyList<EtiquetaCampoDto>>> EmitirEtiquetasCampoAsync(Guid empresaId, DatosEtiquetasCampo datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (datos.Cantidad is < 1 or > 500)
        {
            return Resultado.Fallo<IReadOnlyList<EtiquetaCampoDto>>(Error.Validacion("etiqueta_campo.cantidad", "Se emiten de 1 a 500 etiquetas de una vez."));
        }

        var agricultor = await _repo.AgricultorAsync(datos.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor is null || agricultor.EmpresaId != empresaId)
        {
            return Resultado.Fallo<IReadOnlyList<EtiquetaCampoDto>>(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (datos.ParcelaId is { } pid && (await _repo.ParcelaAsync(pid, ct).ConfigureAwait(false))?.AgricultorId != agricultor.Id)
        {
            return Resultado.Fallo<IReadOnlyList<EtiquetaCampoDto>>(Error.Validacion("etiqueta_campo.parcela", "La parcela no es de ese agricultor."));
        }

        await _unidad.BloquearAsync(ClaveNumeracion(empresaId, "SSCC"), ct).ConfigureAwait(false);
        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false) ?? ConfiguracionAgro.Crear(empresaId);
        var serie = await _repo.PalesCreadosAsync(empresaId, ct).ConfigureAwait(false);
        var lista = new List<EtiquetaCampo>();
        for (var i = 1; i <= datos.Cantidad; i++)
        {
            var sscc = config.Sscc(serie + i);
            var etiqueta = sscc.EsFallo ? Resultado.Fallo<EtiquetaCampo>(sscc.Error) : EtiquetaCampo.Emitir(empresaId, sscc.Valor, agricultor.Id, datos.ParcelaId, _reloj.AhoraUtc);
            if (etiqueta.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<EtiquetaCampoDto>>(etiqueta.Error);
            }

            _repo.Agregar(etiqueta.Valor);
            lista.Add(etiqueta.Valor);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<EtiquetaCampoDto>>(lista.Select(DtoEtiqueta).ToList());
    }

    /// <summary>Lo que se imprime en las etiquetas de campo indicadas: el SSCC y el origen (agricultor y parcela).</summary>
    public async Task<IReadOnlyList<(string Sscc, string Origen, DateOnly Fecha)>> ImpresionEtiquetasCampoAsync(Guid empresaId, IReadOnlyCollection<Guid> ids, CancellationToken ct = default)
    {
        var lista = new List<(string, string, DateOnly)>();
        foreach (var e in (await _repo.EtiquetasCampoPorIdAsync(ids, ct).ConfigureAwait(false)).Where(e => e.EmpresaId == empresaId))
        {
            var agricultor = await _repo.AgricultorAsync(e.AgricultorId, ct).ConfigureAwait(false);
            var parcela = e.ParcelaId is { } pid ? await _repo.ParcelaAsync(pid, ct).ConfigureAwait(false) : null;
            lista.Add((e.Sscc, agricultor?.Nombre + (parcela is null ? "" : $" · {parcela.Codigo} {parcela.Nombre}"), DateOnly.FromDateTime(e.EmitidaEn.UtcDateTime)));
        }

        return lista;
    }

    private static EtiquetaCampoDto DtoEtiqueta(EtiquetaCampo e) => new(e.Id, e.Sscc, e.AgricultorId, e.ParcelaId, e.EmitidaEn, e.PaleId);

    public async Task<Resultado<RecepcionDto>> QuitarPaleEntradaAsync(Guid recepcionId, Guid paleEntradaId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.QuitarPaleEntrada(paleEntradaId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> FijarKilosLiquidacionAsync(Guid recepcionId, Guid lineaId, DatosKilosLiquidacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.FijarKilosLiquidacion(lineaId, datos.Kilos, datos.Motivo), ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecepcionDto>> QuitarPesadaAsync(Guid recepcionId, Guid pesadaId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        return r is null ? NoEncontrada<RecepcionDto>() : await GuardarAsync(r, r.QuitarPesada(pesadaId), ct).ConfigureAwait(false);
    }

    public async Task<Resultado> EliminarAsync(Guid recepcionId, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.no_encontrada", "La recepción no existe."));
        }

        if (r.Estado != EstadoRecepcion.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("recepcion.no_borrador", "Solo se elimina un borrador; una recepción confirmada se anula."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Confirma la recepción: la numera sin huecos, crea una partida por línea con sus kilos netos y registra
    /// la devolución de los envases en que llega la fruta. Devuelve todos los problemas a la vez.
    /// </summary>
    public async Task<Resultado<RecepcionDto>> ConfirmarAsync(Guid empresaId, Guid recepcionId, CancellationToken ct = default)
    {
        // Primero el bloqueo y luego la lectura: dos confirmaciones a la vez no numeran dos veces la misma recepción.
        await _unidad.BloquearAsync(ClaveNumeracion(empresaId, Recepcion.Serie), ct).ConfigureAwait(false);
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        // Las taras de los envases, las vigentes en la fecha de la recepción (por si la fecha cambió en el borrador).
        var taras = new Dictionary<Guid, (decimal, Guid)?>();
        foreach (var envase in r.EnvasesPesadas.Where(e => e.TaraEnvaseId is not null).Select(e => e.EnvaseProductoId).Distinct())
        {
            var t = _taras is null ? null : await _taras.VigenteAsync(r.EmpresaId, envase, r.Fecha, ct).ConfigureAwait(false);
            taras[envase] = t is null ? null : (t.TaraKg, t.Id);
        }

        var retaradas = r.AplicarTaras(e => taras.GetValueOrDefault(e));
        if (retaradas.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(retaradas.Error);
        }

        var errores = r.ErroresConfirmacion().ToList();
        var agricultor = await _repo.AgricultorAsync(r.AgricultorId, ct).ConfigureAwait(false);
        if (agricultor?.Bloqueado == true)
        {
            errores.Add(Error.Conflicto("agricultor.bloqueado", $"El agricultor está bloqueado: {agricultor.MotivoBloqueo}"));
        }

        var campana = await _repo.CampanaAsync(r.CampanaId, ct).ConfigureAwait(false);
        if (campana is null || !campana.Contiene(r.Fecha))
        {
            errores.Add(Error.Validacion("recepcion.fuera_campana", "La fecha de la recepción no está dentro de su campaña."));
        }

        // Cuaderno de campo: ninguna línea recolectada dentro del plazo de seguridad de un tratamiento de su parcela.
        if (_cuaderno is not null)
        {
            errores.AddRange(await _cuaderno.ComprobarPlazosAsync(r, ct).ConfigureAwait(false));
        }

        // Certificaciones: las vigentes el día de la recepción para el agricultor (o esa parcela), ajustadas al artículo.
        var certificados = await _repo.CertificadosAsync(r.EmpresaId, r.AgricultorId, ct).ConfigureAwait(false);
        var porParcela = (await _repo.ConfiguracionAsync(r.EmpresaId, ct).ConfigureAwait(false))?.CertificacionPorParcela == true;
        var certificaciones = new Dictionary<Guid, (Certificaciones Resultado, Certificaciones Quitadas)>();
        foreach (var l in r.Lineas)
        {
            var vigentes = certificados.Where(c => c.VigenteEl(r.Fecha) && (c.ParcelaId is null || c.ParcelaId == l.ParcelaId))
                .Aggregate(Certificaciones.Ninguna, (a, c) => a | c.Tipo);
            var declaracion = await _repo.DeclaracionAsync(r.EmpresaId, l.ProductoId, ct).ConfigureAwait(false);
            // Si la empresa certifica por parcela, lo que se vende certificado tiene que saber de qué parcela viene.
            if (porParcela && declaracion is { Exige: not Certificaciones.Ninguna } && l.ParcelaId is null)
            {
                errores.Add(Error.Validacion("recepcion.parcela_certificada",
                    $"Línea {l.NumeroLinea}: «{l.ProductoNombre}» se vende como {ReglasCertificacion.Texto(declaracion.Exige)}: indica la parcela de la que viene."));
                continue;
            }

            var c = ReglasCertificacion.Aplicar(vigentes, declaracion?.Exige, l.MotivoDescalificacion, l.ProductoNombre);
            if (c.EsFallo)
            {
                errores.Add(Error.Conflicto(c.Error.Codigo, $"Línea {l.NumeroLinea}: {c.Error.Mensaje}"));
            }
            else
            {
                certificaciones[l.Id] = c.Valor;
            }
        }

        if (errores.Count > 0)
        {
            return Resultado.Fallo<RecepcionDto>(Valoracion.Resumen(errores));
        }

        var numero = await _repo.UltimoNumeroAsync(r.EmpresaId, Recepcion.Serie, r.Ejercicio, ct).ConfigureAwait(false) + 1;

        var partidas = new Dictionary<Guid, Guid>();
        var palesEntrada = new Dictionary<Guid, Guid>();
        var codigo = $"{Recepcion.Serie}-{r.Ejercicio}-{numero:D6}";
        var envasesRecibidos = new List<(Guid, int)>();
        foreach (var l in r.Lineas)
        {
            var kilos = r.NetoDe(l.Id);
            var (certs, quitadas) = certificaciones[l.Id];
            var partida = Partida.DeRecepcion(r.EmpresaId, r, l, kilos, _reloj, certs);
            partida.AsignarCodigo($"{codigo}/{l.NumeroLinea}");
            _repo.Agregar(partida);
            if (quitadas != Certificaciones.Ninguna)
            {
                _repo.Agregar(DescalificacionPartida.Crear(r.EmpresaId, partida.Id, quitadas, l.MotivoDescalificacion!, "Recepcion", r.Id, null, _reloj.AhoraUtc));
            }

            partidas[l.Id] = partida.Id;
            // Con palés de entrada, la fruta entra ya en sus palés (cada uno con su SSCC y la serie con que llegó); si no, suelta.
            var palesLinea = r.KilosPalesEntrada(l.Id);
            if (palesLinea.Count == 0)
            {
                _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, partida.Id, r.Fecha, TipoMovimientoPartida.Entrada, kilos, null, "Recepcion", r.Id,
                    $"Recepción {codigo}", _reloj).Valor);
            }
            else
            {
                // Los que llegan con etiqueta de campo conservan su SSCC; al resto se le da uno nuevo (se etiqueta en la báscula).
                var etiquetas = new Dictionary<Guid, EtiquetaCampo>();
                foreach (var (entrada, _) in palesLinea)
                {
                    if (EtiquetaCampo.SsccDeLectura(entrada.SerieOrigen) is { } ssccCampo
                        && await _repo.EtiquetaCampoAsync(r.EmpresaId, ssccCampo, ct).ConfigureAwait(false) is { } etiqueta && etiqueta.AgricultorId == r.AgricultorId)
                    {
                        etiquetas[entrada.Id] = etiqueta;
                    }
                }

                var sinEtiqueta = palesLinea.Where(x => !etiquetas.ContainsKey(x.Pale.Id)).ToList();
                var nuevos = await PalesAgro.NuevosPalesAsync(_repo, _unidad, _reloj, r.EmpresaId, sinEtiqueta.Count, "Entrada", null, ct,
                    (sscc, i) => Pale.DeEntrada(r.EmpresaId, sscc, l.Id, sinEtiqueta[i].Pale.SerieOrigen, "Entrada", _reloj)).ConfigureAwait(false);
                if (nuevos.EsFallo)
                {
                    return Resultado.Fallo<RecepcionDto>(nuevos.Error);
                }

                var palePorEntrada = new Dictionary<Guid, Pale>();
                for (var i = 0; i < sinEtiqueta.Count; i++)
                {
                    palePorEntrada[sinEtiqueta[i].Pale.Id] = nuevos.Valor[i];
                }

                foreach (var (entradaId, etiqueta) in etiquetas)
                {
                    var pale = Pale.DeEntrada(r.EmpresaId, etiqueta.Sscc, l.Id, etiqueta.Sscc, "Entrada", _reloj);
                    var usada = pale.EsFallo ? Resultado.Fallo(pale.Error) : etiqueta.Usar(pale.Valor.Id);
                    if (usada.EsFallo)
                    {
                        return Resultado.Fallo<RecepcionDto>(usada.Error);
                    }

                    _repo.Agregar(pale.Valor);
                    palePorEntrada[entradaId] = pale.Valor;
                }

                for (var i = 0; i < palesLinea.Count; i++)
                {
                    var (entrada, kilosPale) = palesLinea[i];
                    var paleEntrada = palePorEntrada[entrada.Id];
                    palesEntrada[entrada.Id] = paleEntrada.Id;
                    _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, partida.Id, r.Fecha, TipoMovimientoPartida.Entrada, kilosPale, paleEntrada.Id, "Recepcion", r.Id,
                        $"Recepción {codigo} · palé {entrada.SerieOrigen ?? entrada.Numero.ToString(System.Globalization.CultureInfo.InvariantCulture)}", _reloj,
                        entrada.Envases).Valor);
                }
            }

            var envases = r.EnvasesDe(l.Id);
            if (envases > 0)
            {
                _repo.Agregar(MovimientoEnvase.Crear(r.EmpresaId, r.AgricultorId, l.EnvaseProductoId!.Value, r.Fecha, -envases, r.Id, $"Recepción {codigo}", _reloj).Valor);
                envasesRecibidos.Add((l.EnvaseProductoId!.Value, -envases));
            }
        }

        // El mismo movimiento, en el libro de envases por tercero (la cuenta del proveedor del agricultor).
        if (_envases is not null && agricultor is not null && envasesRecibidos.Count > 0)
        {
            var libro = await _envases.RegistrarAgricultorAsync(r.EmpresaId, agricultor.ProveedorId, agricultor.Nombre, envasesRecibidos, r.Fecha, r.Id,
                $"Recepción {codigo}", ct).ConfigureAwait(false);
            if (libro.EsFallo)
            {
                return Resultado.Fallo<RecepcionDto>(libro.Error);
            }
        }

        var confirmada = r.Confirmar(numero, partidas, _reloj, palesEntrada);
        return await GuardarAsync(r, confirmada, ct).ConfigureAwait(false);
    }

    /// <summary>Anula una recepción confirmada si sus partidas no se han usado ni liquidado.</summary>
    public async Task<Resultado<RecepcionDto>> AnularAsync(Guid recepcionId, string? motivo, CancellationToken ct = default)
    {
        var r = await _repo.RecepcionAsync(recepcionId, ct).ConfigureAwait(false);
        if (r is null)
        {
            return NoEncontrada<RecepcionDto>();
        }

        var partidas = await _repo.PartidasDeRecepcionAsync(r.Id, ct).ConfigureAwait(false);
        var ids = partidas.Select(p => p.Id).ToList();
        if (await _repo.TieneMovimientosPosterioresAsync(ids, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("recepcion.partidas_usadas", "Sus partidas ya se han confeccionado, paletizado o ajustado: anula antes esos movimientos."));
        }

        if (await _repo.PartidasEnLiquidacionAsync(ids, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<RecepcionDto>(Error.Conflicto("recepcion.liquidada", "La recepción está en una liquidación: elimina o anula antes la liquidación."));
        }

        var anulada = r.Anular(motivo, _reloj);
        if (anulada.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(anulada.Error);
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        foreach (var p in partidas)
        {
            p.Anular();
            var enPales = r.PalesEntrada.Where(e => e.LineaId == p.LineaRecepcionId && e.PaleId is not null).ToList();
            if (enPales.Count == 0)
            {
                _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, p.Id, hoy, TipoMovimientoPartida.Anulacion, -p.KilosIniciales, null, "Recepcion", r.Id,
                    $"Anulación de {r.NumeroCompleto}", _reloj).Valor);
            }

            foreach (var e in enPales)
            {
                _repo.Agregar(MovimientoPartida.Crear(r.EmpresaId, p.Id, hoy, TipoMovimientoPartida.Anulacion, -e.KilosAsignados!.Value, e.PaleId, "Recepcion", r.Id,
                    $"Anulación de {r.NumeroCompleto}", _reloj, -e.Envases).Valor);
            }
        }

        foreach (var l in r.Lineas.Where(l => l.Envases > 0))
        {
            _repo.Agregar(MovimientoEnvase.Crear(r.EmpresaId, r.AgricultorId, l.EnvaseProductoId!.Value, hoy, l.Envases!.Value, r.Id, $"Anulación de {r.NumeroCompleto}", _reloj).Valor);
        }

        if (_envases is not null)
        {
            var libro = await _envases.AnularDeDocumentoAsync(r.Id, $"Anulación de {r.NumeroCompleto}", ct).ConfigureAwait(false);
            if (libro.EsFallo)
            {
                return Resultado.Fallo<RecepcionDto>(libro.Error);
            }
        }

        return await GuardarAsync(r, Resultado.Ok(), ct).ConfigureAwait(false);
    }

    // ------------------------------------------------------------------ Partidas
    public async Task<IReadOnlyList<PartidaDto>> ExistenciasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var partidas = await _repo.PartidasConSaldoAsync(empresaId, ct).ConfigureAwait(false);
        var saldos = (await _repo.SaldosAsync(partidas.Select(p => p.Id).ToList(), ct).ConfigureAwait(false))
            .GroupBy(s => s.PartidaId).ToDictionary(g => g.Key, g => g.Sum(s => s.Kilos));
        return partidas.OrderBy(p => p.Fecha).ThenBy(p => p.Codigo, StringComparer.Ordinal).Select(p => PartidaDe(p, saldos.GetValueOrDefault(p.Id))).ToList();
    }

    public async Task<(PartidaDto Partida, IReadOnlyList<SaldoPartida> Saldos, IReadOnlyList<MovimientoPartidaDto> Movimientos)?> PartidaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PartidaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return null;
        }

        var saldos = await _repo.SaldosAsync([id], ct).ConfigureAwait(false);
        var movimientos = await _repo.MovimientosAsync([id], ct).ConfigureAwait(false);
        return (PartidaDe(p, saldos.Sum(s => s.Kilos)), saldos,
            movimientos.OrderBy(m => m.CreadoEn).Select(m => new MovimientoPartidaDto(m.Id, m.PartidaId, m.Fecha, m.Tipo.ToString(), m.Kilos, m.PaleId, m.DocumentoTipo, m.DocumentoId, m.Concepto)).ToList());
    }

    /// <summary>Merma, destrío retirado o regularización de inventario de una partida (con signo).</summary>
    public async Task<Resultado> AjustarPartidaAsync(Guid partidaId, DatosAjustePartida datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (p is null || p.Anulada)
        {
            return Resultado.Fallo(Error.NoEncontrado("partida.no_encontrada", "La partida no existe o está anulada."));
        }

        if (string.IsNullOrWhiteSpace(datos.Concepto))
        {
            return Resultado.Fallo(Error.Validacion("partida.concepto", "Indica el motivo del ajuste."));
        }

        var m = MovimientoPartida.Crear(p.EmpresaId, p.Id, datos.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), TipoMovimientoPartida.Ajuste,
            datos.Kilos, datos.PaleId, "Ajuste", null, datos.Concepto, _reloj);
        if (m.EsFallo)
        {
            return Resultado.Fallo(m.Error);
        }

        var saldo = (await _repo.SaldosAsync([p.Id], ct).ConfigureAwait(false)).Where(s => s.PaleId == datos.PaleId).Sum(s => s.Kilos);
        if (saldo + datos.Kilos < 0m)
        {
            return Resultado.Fallo(Error.Conflicto("partida.saldo_insuficiente", $"La partida solo tiene {Redondeo.Formatear(saldo, 3)} kg ahí."));
        }

        _repo.Agregar(m.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Envases
    public async Task<IReadOnlyList<SaldoEnvaseDto>> SaldoEnvasesAsync(Guid agricultorId, CancellationToken ct = default) =>
        (await _repo.SaldoEnvasesAsync(agricultorId, ct).ConfigureAwait(false)).Select(s => new SaldoEnvaseDto(s.EnvaseProductoId, s.Saldo)).ToList();

    public async Task<IReadOnlyList<MovimientoEnvaseDto>> MovimientosEnvaseAsync(Guid agricultorId, CancellationToken ct = default) =>
        (await _repo.MovimientosEnvaseAsync(agricultorId, ct).ConfigureAwait(false)).OrderByDescending(m => m.Fecha).ThenByDescending(m => m.CreadoEn)
            .Select(m => new MovimientoEnvaseDto(m.Id, m.EnvaseProductoId, m.Fecha, m.Cantidad, m.RecepcionId, m.Concepto)).ToList();

    /// <summary>Entrega (+) o devolución (−) de envases vacíos a un agricultor.</summary>
    public async Task<Resultado> MoverEnvasesAsync(Guid empresaId, Guid agricultorId, DatosMovimientoEnvase datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await _repo.AgricultorAsync(agricultorId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (await _productos.ObtenerAsync(datos.EnvaseProductoId, ct).ConfigureAwait(false) is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("recepcion.envase", "El artículo de envase no existe."));
        }

        var m = MovimientoEnvase.Crear(empresaId, agricultorId, datos.EnvaseProductoId, datos.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime),
            datos.Cantidad, null, datos.Concepto ?? (datos.Cantidad > 0 ? "Entrega de envases vacíos" : "Devolución de envases vacíos"), _reloj);
        if (m.EsFallo)
        {
            return Resultado.Fallo(m.Error);
        }

        _repo.Agregar(m.Valor);
        if (_envases is not null && await _repo.AgricultorAsync(agricultorId, ct).ConfigureAwait(false) is { } agricultor)
        {
            var libro = await _envases.RegistrarAgricultorAsync(empresaId, agricultor.ProveedorId, agricultor.Nombre, [(datos.EnvaseProductoId, datos.Cantidad)], m.Valor.Fecha,
                m.Valor.Id, m.Valor.Concepto, ct).ConfigureAwait(false);
            if (libro.EsFallo)
            {
                return Resultado.Fallo(libro.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    internal static PartidaDto PartidaDe(Partida p, decimal saldo) => new(
        p.Id, p.Codigo, p.ProductoId, p.Origen.ToString(), p.Fecha, p.KilosIniciales, saldo, p.AgricultorId, p.ParcelaId, p.CampanaId, p.Calibre,
        p.CosteKg, p.Anulada, p.RecepcionId, p.ParteConfeccionId, p.Certificaciones.ToString());

    internal static bool EsKilo(string? unidad) =>
        string.Equals(unidad?.Trim(), "kg", StringComparison.OrdinalIgnoreCase) || string.Equals(unidad?.Trim(), "kilo", StringComparison.OrdinalIgnoreCase);

    private async Task<Resultado<Campana>> CampanaDeAsync(Guid empresaId, Guid? campanaId, DateOnly fecha, CancellationToken ct)
    {
        if (campanaId is { } id)
        {
            var c = await _repo.CampanaAsync(id, ct).ConfigureAwait(false);
            if (c is null)
            {
                return Resultado.Fallo<Campana>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
            }

            return c.Contiene(fecha) ? Resultado.Ok(c) : Resultado.Fallo<Campana>(Error.Validacion("recepcion.fuera_campana", "La fecha no está dentro de la campaña."));
        }

        var abierta = (await _repo.CampanasAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(c => c.Contiene(fecha));
        return abierta is null
            ? Resultado.Fallo<Campana>(Error.Validacion("recepcion.sin_campana", $"No hay ninguna campaña que incluya el {fecha:dd/MM/yyyy}."))
            : Resultado.Ok(abierta);
    }

    private async Task<Resultado<RecepcionDto>> GuardarAsync(Recepcion r, Resultado resultado, CancellationToken ct)
    {
        if (resultado.EsFallo)
        {
            return Resultado.Fallo<RecepcionDto>(resultado.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DtoAsync(r, ct).ConfigureAwait(false));
    }

    private static Resultado<T> NoEncontrada<T>() => Resultado.Fallo<T>(Error.NoEncontrado("recepcion.no_encontrada", "La recepción no existe."));
}
