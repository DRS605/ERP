using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Cooperativa.Aplicacion;

/// <summary>Actividad indicada a mano para un socio (sustituye a la que sale de las liquidaciones).</summary>
public sealed record ActividadManual(Guid SocioId, decimal Actividad);

/// <summary>
/// Datos del reparto del excedente de un ejercicio. Sin porcentajes, los mínimos de los ajustes. La actividad sale de las
/// liquidaciones emitidas en el ejercicio (o en las fechas indicadas, p. ej. la campaña), salvo que se dé a mano.
/// </summary>
public sealed record DatosReparto(int Ejercicio, decimal Excedente, DateOnly? Fecha = null, DateOnly? FechaAsamblea = null, decimal? PorcentajeFro = null,
    decimal? PorcentajeFep = null, decimal ReservasVoluntarias = 0m, decimal PorcentajeIntereses = 0m, DateOnly? ActividadDesde = null, DateOnly? ActividadHasta = null,
    IReadOnlyList<ActividadManual>? Actividad = null, IReadOnlyList<Guid>? Capitalizan = null, string? Observaciones = null);

public sealed record LineaRepartoDto(Guid SocioId, int NumeroSocio, string Socio, decimal Actividad, decimal Porcentaje, decimal Capital, decimal Intereses, decimal Retorno,
    decimal Retencion, decimal Capitalizado, decimal Neto);

public sealed record RepartoDto(Guid Id, int Ejercicio, DateOnly Fecha, DateOnly? FechaAsamblea, EstadoReparto Estado, BaseRetorno Base, decimal Excedente, decimal PorcentajeFro,
    decimal ImporteFro, decimal PorcentajeFep, decimal ImporteFep, decimal ReservasVoluntarias, decimal PorcentajeIntereses, decimal ImporteIntereses, decimal ImporteRetorno,
    decimal PorcentajeRetencion, decimal TotalRetencion, decimal TotalCapitalizado, decimal TotalNeto, string? Observaciones, Guid? AsientoId, string? MotivoAnulacion,
    IReadOnlyList<LineaRepartoDto> Lineas);

/// <summary>Lo retenido a un socio en un año (para los modelos 123 y 193).</summary>
public sealed record RetencionSocioDto(Guid SocioId, int NumeroSocio, string Socio, string? Nif, decimal Integro, decimal Retencion, decimal Neto);

public sealed record RetencionesDto(int Ejercicio, decimal Integro, decimal Retencion, IReadOnlyList<RetencionSocioDto> Socios);

/// <summary>
/// Reparto del excedente del ejercicio: propuesta (se recalcula hasta que la asamblea la aprueba), contabilización (el
/// asiento de distribución y el retorno capitalizado en el capital de cada socio) y anulación. Y las retenciones del año.
/// </summary>
public sealed class RepartosCooperativa
{
    private readonly IRepositorioCooperativa _repo;
    private readonly IUnidadDeTrabajoCooperativa _unidad;
    private readonly IConsultaProveedores _proveedores;
    private readonly IReloj _reloj;
    private readonly IActividadSocios? _actividad;
    private readonly IContabilidadCooperativa? _contabilidad;

    public RepartosCooperativa(IRepositorioCooperativa repo, IUnidadDeTrabajoCooperativa unidad, IConsultaProveedores proveedores, IReloj reloj,
        IActividadSocios? actividad = null, IContabilidadCooperativa? contabilidad = null)
    {
        _repo = repo;
        _unidad = unidad;
        _proveedores = proveedores;
        _reloj = reloj;
        _actividad = actividad;
        _contabilidad = contabilidad;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<RepartoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var numeros = await NumerosAsync(empresaId, ct).ConfigureAwait(false);
        return (await _repo.RepartosAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(r => r.Ejercicio).ThenByDescending(r => r.CreadoEn)
            .Select(r => Dto(r, numeros)).ToList();
    }

    public async Task<RepartoDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.RepartoAsync(id, ct).ConfigureAwait(false);
        return r is null ? null : Dto(r, await NumerosAsync(r.EmpresaId, ct).ConfigureAwait(false));
    }

    /// <summary>Calcula el reparto sin guardarlo (para ver la propuesta antes de llevarla a la asamblea).</summary>
    public async Task<Resultado<RepartoDto>> SimularAsync(Guid empresaId, DatosReparto d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var reparto = Reparto.Nuevo(empresaId, d.Ejercicio, _reloj.AhoraUtc);
        var r = await CalcularAsync(reparto, d, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<RepartoDto>(r.Error) : Resultado.Ok(Dto(reparto, await NumerosAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>Guarda la propuesta de reparto (en borrador). Solo hay un reparto vivo (borrador o contabilizado) por ejercicio.</summary>
    public async Task<Resultado<RepartoDto>> CrearAsync(Guid empresaId, DatosReparto d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((await _repo.RepartosAsync(empresaId, ct).ConfigureAwait(false)).Any(r => r.Ejercicio == d.Ejercicio && r.Estado != EstadoReparto.Anulado))
        {
            return Resultado.Fallo<RepartoDto>(Error.Conflicto("reparto.duplicado", $"El ejercicio {d.Ejercicio} ya tiene un reparto: cámbialo o anúlalo."));
        }

        var reparto = Reparto.Nuevo(empresaId, d.Ejercicio, _reloj.AhoraUtc);
        var r = await CalcularAsync(reparto, d, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RepartoDto>(r.Error);
        }

        _repo.Agregar(reparto);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(reparto, await NumerosAsync(empresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>Recalcula la propuesta con otros datos (solo en borrador; el ejercicio no cambia).</summary>
    public async Task<Resultado<RepartoDto>> RecalcularAsync(Guid id, DatosReparto d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var reparto = await _repo.RepartoAsync(id, ct).ConfigureAwait(false);
        if (reparto is null)
        {
            return Resultado.Fallo<RepartoDto>(NoExiste());
        }

        if (d.Ejercicio != reparto.Ejercicio)
        {
            return Resultado.Fallo<RepartoDto>(Error.Validacion("reparto.ejercicio", "El ejercicio del reparto no cambia: haz otro reparto."));
        }

        var r = await CalcularAsync(reparto, d, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RepartoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(reparto, await NumerosAsync(reparto.EmpresaId, ct).ConfigureAwait(false)));
    }

    /// <summary>
    /// Contabiliza el reparto aprobado: 129 al debe por el excedente; al haber los fondos, las reservas, lo que se paga a
    /// los socios (526), las retenciones (4751) y el capital por lo capitalizado, que entra en el capital de cada socio.
    /// </summary>
    public async Task<Resultado<RepartoDto>> ContabilizarAsync(Guid id, CancellationToken ct = default)
    {
        var reparto = await _repo.RepartoAsync(id, ct).ConfigureAwait(false);
        if (reparto is null)
        {
            return Resultado.Fallo<RepartoDto>(NoExiste());
        }

        if (reparto.Estado != EstadoReparto.Borrador)
        {
            return Resultado.Fallo<RepartoDto>(Error.Conflicto("reparto.no_borrador", "El reparto ya está contabilizado o anulado."));
        }

        var config = await SociosCooperativa.ConfiguracionDeAsync(_repo, reparto.EmpresaId, ct).ConfigureAwait(false);
        var socios = (await _repo.SociosAsync(reparto.EmpresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id);
        if (reparto.Lineas.FirstOrDefault(l => l.Capitalizado > 0m && socios.TryGetValue(l.SocioId, out var s) && s.DeBaja) is { } deBaja)
        {
            return Resultado.Fallo<RepartoDto>(Error.Conflicto("reparto.capitaliza_baja", $"{deBaja.Nombre} está de baja: no puede capitalizar su retorno. Recalcula el reparto."));
        }

        Guid? asiento = null;
        if (config.Contabilizar && _contabilidad is not null)
        {
            var c = config.Cuentas;
            var apuntes = SociosCooperativa.Neto(
            [
                new(c.Resultado, reparto.Excedente, 0m, "Excedente del ejercicio"),
                new(c.Fro, 0m, reparto.ImporteFro, "Fondo de reserva obligatorio"),
                new(c.Fep, 0m, reparto.ImporteFep, "Fondo de educación y promoción"),
                new(c.ReservasVoluntarias, 0m, reparto.ReservasVoluntarias, "Reservas voluntarias"),
                new(c.Retornos, 0m, reparto.TotalNeto, "Retornos e intereses a pagar"),
                new(c.Retenciones, 0m, reparto.TotalRetencion, "Retenciones"),
                new(c.Capital, 0m, reparto.TotalCapitalizado, "Retorno capitalizado"),
            ]);
            var r = await _contabilidad.RegistrarAsync(reparto.EmpresaId, reparto.Fecha, $"Distribución del excedente del ejercicio {reparto.Ejercicio}", apuntes, ct)
                .ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<RepartoDto>(r.Error);
            }

            asiento = r.Valor;
        }

        var ok = reparto.Contabilizar(asiento);
        if (ok.EsFallo)
        {
            return Resultado.Fallo<RepartoDto>(ok.Error);
        }

        foreach (var l in reparto.Lineas.Where(l => l.Capitalizado > 0m))
        {
            _repo.Agregar(MovimientoCapital.RetornoCapitalizado(reparto.EmpresaId, l.SocioId, reparto.Fecha, l.Capitalizado, reparto.Id,
                $"Retorno cooperativo del ejercicio {reparto.Ejercicio} capitalizado", _reloj.AhoraUtc));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(reparto, socios.Values.ToDictionary(s => s.Id, s => s.Numero)));
    }

    /// <summary>Anula un reparto contabilizado: contraasiento y anulación de lo capitalizado (si ese capital ya se devolvió, la base de datos no lo deja).</summary>
    public async Task<Resultado<RepartoDto>> AnularAsync(Guid id, string? motivo, DateOnly? fecha, CancellationToken ct = default)
    {
        var reparto = await _repo.RepartoAsync(id, ct).ConfigureAwait(false);
        if (reparto is null)
        {
            return Resultado.Fallo<RepartoDto>(NoExiste());
        }

        var dia = fecha ?? Hoy;
        if (dia < reparto.Fecha)
        {
            return Resultado.Fallo<RepartoDto>(Error.Validacion("reparto.fecha_anulacion", "La anulación no puede ser anterior al reparto."));
        }

        var r = reparto.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RepartoDto>(r.Error);
        }

        if (reparto.AsientoId is { } asiento && _contabilidad is not null)
        {
            var a = await _contabilidad.AnularAsync(reparto.EmpresaId, asiento, dia, ct).ConfigureAwait(false);
            if (a.EsFallo)
            {
                return Resultado.Fallo<RepartoDto>(a.Error);
            }
        }

        foreach (var m in (await _repo.MovimientosAsync(reparto.EmpresaId, null, ct).ConfigureAwait(false))
                     .Where(m => m.RepartoId == reparto.Id && m.Tipo == TipoMovimientoCapital.RetornoCapitalizado))
        {
            _repo.Agregar(m.Anulacion(dia, reparto.MotivoAnulacion!, _reloj.AhoraUtc));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(reparto, await NumerosAsync(reparto.EmpresaId, ct).ConfigureAwait(false)));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var reparto = await _repo.RepartoAsync(id, ct).ConfigureAwait(false);
        if (reparto is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        if (reparto.Estado != EstadoReparto.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("reparto.no_borrador", "Solo se elimina un reparto en borrador; uno contabilizado se anula."));
        }

        _repo.Eliminar(reparto);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Lo retenido en el año a cada socio por retornos e intereses (repartos contabilizados con fecha en ese año): la base
    /// de los modelos 123 (trimestral) y 193 (resumen anual). Lo capitalizado no lleva retención.
    /// </summary>
    public Task<RetencionesDto> RetencionesAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        RetencionesAsync(empresaId, new DateOnly(ejercicio, 1, 1), new DateOnly(ejercicio, 12, 31), ct);

    /// <summary>Lo retenido entre dos fechas (por la fecha del reparto): el trimestre del modelo 123.</summary>
    public async Task<RetencionesDto> RetencionesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var ejercicio = hasta.Year;
        var socios = (await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id);
        var lineas = (await _repo.RepartosAsync(empresaId, ct).ConfigureAwait(false))
            .Where(r => r.Estado == EstadoReparto.Contabilizado && r.Fecha >= desde && r.Fecha <= hasta).SelectMany(r => r.Lineas).ToList();
        var lista = new List<RetencionSocioDto>();
        foreach (var g in lineas.GroupBy(l => l.SocioId))
        {
            var s = socios.GetValueOrDefault(g.Key);
            var nif = s is null ? null : (await _proveedores.ObtenerAsync(s.ProveedorId, ct).ConfigureAwait(false))?.NifFiscal ?? s.Nif;
            var integro = g.Sum(l => l.Retorno - l.Capitalizado + l.Intereses);
            if (integro == 0m)
            {
                continue;
            }

            lista.Add(new RetencionSocioDto(g.Key, s?.Numero ?? 0, s?.Nombre ?? g.First().Nombre, nif, integro, g.Sum(l => l.Retencion), g.Sum(l => l.Neto)));
        }

        lista = lista.OrderBy(x => x.NumeroSocio).ToList();
        return new RetencionesDto(ejercicio, lista.Sum(x => x.Integro), lista.Sum(x => x.Retencion), lista);
    }

    // ------------------------------------------------------------------ Apoyo
    private async Task<Resultado> CalcularAsync(Reparto reparto, DatosReparto d, CancellationToken ct)
    {
        var empresaId = reparto.EmpresaId;
        var config = await SociosCooperativa.ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false);
        var inicio = new DateOnly(d.Ejercicio, 1, 1);
        var fin = new DateOnly(d.Ejercicio, 12, 31);
        var desde = d.ActividadDesde ?? inicio;
        var hasta = d.ActividadHasta ?? fin;
        if (hasta < desde)
        {
            return Resultado.Fallo(Error.Validacion("reparto.periodo", "El periodo de la actividad acaba después de empezar."));
        }

        // Socios del ejercicio: de alta en algún momento del periodo (el que se fue a mitad cobra por lo que entregó).
        var socios = (await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false)).Where(s => s.FechaAlta <= fin && (s.FechaBaja is null || s.FechaBaja >= inicio)).ToList();
        var movimientos = (await _repo.MovimientosAsync(empresaId, null, ct).ConfigureAwait(false)).Where(m => m.Fecha <= fin).ToLookup(m => m.SocioId);
        var manual = d.Actividad?.ToDictionary(a => a.SocioId, a => a.Actividad);
        if (manual is not null && manual.Keys.Any(k => socios.All(s => s.Id != k)))
        {
            return Resultado.Fallo(Error.Validacion("reparto.socio", "La actividad indicada es de alguien que no es socio en el ejercicio."));
        }

        var entregas = manual is null && config.Base != BaseRetorno.Capital && _actividad is not null
            ? await _actividad.PorTerceroAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)
            : new Dictionary<Guid, ActividadTercero>();
        var capitalizan = (d.Capitalizan ?? []).ToHashSet();
        if (capitalizan.Any(id => socios.FirstOrDefault(s => s.Id == id) is not { DeBaja: false }))
        {
            return Resultado.Fallo(Error.Validacion("reparto.capitalizar", "Solo capitalizan su retorno los socios que siguen de alta."));
        }

        var enReparto = socios.Select(s =>
        {
            var capital = SaldoCapital.De(movimientos[s.Id]).Sum(x => x.Desembolsado);
            decimal actividad;
            if (!s.ConRetorno(config.Forma))
            {
                actividad = 0m;
            }
            else if (manual is not null)
            {
                actividad = manual.GetValueOrDefault(s.Id);
            }
            else
            {
                actividad = config.Base switch
                {
                    BaseRetorno.Capital => capital,
                    BaseRetorno.Importe => entregas.GetValueOrDefault(s.ProveedorId)?.Importe ?? 0m,
                    _ => entregas.GetValueOrDefault(s.ProveedorId)?.Kilos ?? 0m,
                };
            }

            return new SocioEnReparto(s.Id, s.Nombre, actividad, capital, capitalizan.Contains(s.Id));
        }).Where(s => s.Actividad > 0m || s.CapitalDesembolsado > 0m).ToList();

        return reparto.Calcular(config, d.Fecha ?? Hoy, d.FechaAsamblea, d.Excedente, d.PorcentajeFro, d.PorcentajeFep, d.ReservasVoluntarias, d.PorcentajeIntereses,
            enReparto, d.Observaciones);
    }

    private async Task<IReadOnlyDictionary<Guid, int>> NumerosAsync(Guid empresaId, CancellationToken ct) =>
        (await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id, s => s.Numero);

    private static RepartoDto Dto(Reparto r, IReadOnlyDictionary<Guid, int> numeros)
    {
        var actividad = r.Lineas.Sum(l => l.Actividad);
        return new RepartoDto(r.Id, r.Ejercicio, r.Fecha, r.FechaAsamblea, r.Estado, r.Base, r.Excedente, r.PorcentajeFro, r.ImporteFro, r.PorcentajeFep, r.ImporteFep,
            r.ReservasVoluntarias, r.PorcentajeIntereses, r.ImporteIntereses, r.ImporteRetorno, r.PorcentajeRetencion, r.TotalRetencion, r.TotalCapitalizado, r.TotalNeto,
            r.Observaciones, r.AsientoId, r.MotivoAnulacion,
            r.Lineas.OrderBy(l => l.Numero).Select(l => new LineaRepartoDto(l.SocioId, numeros.GetValueOrDefault(l.SocioId), l.Nombre, l.Actividad,
                actividad == 0m ? 0m : Math.Round(100m * l.Actividad / actividad, 4), l.Capital, l.Intereses, l.Retorno, l.Retencion, l.Capitalizado, l.Neto)).ToList());
    }

    private static Error NoExiste() => Error.NoEncontrado("reparto.no_encontrado", "El reparto no existe.");
}
