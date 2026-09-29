using System.Globalization;
using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record DatosPlan(TipoPlan Tipo, Guid CampanaId, string? Nombre, IReadOnlyList<DatosLineaPlan>? Lineas = null);

public sealed record DatosCambioPlan(string? Nombre, IReadOnlyList<DatosLineaPlan> Lineas);

public sealed record LineaPlanDto(Guid Id, int Numero, DateOnly Desde, DateOnly Hasta, Guid ProductoId, decimal Kilos, Guid? ClienteId, Guid? AgricultorId, Guid? ParcelaId,
    string? LineaConfeccion, int? Cajas, decimal? PrecioKg, string? Notas);

public sealed record PlanDto(Guid Id, string Tipo, Guid CampanaId, string Nombre, int Version, string Estado, decimal Kilos, DateTimeOffset CreadoEn, DateTimeOffset? AprobadoEn,
    IReadOnlyList<LineaPlanDto> Lineas);

public sealed record SeguimientoLineaDto(Guid LineaId, int Numero, DateOnly Desde, DateOnly Hasta, Guid ProductoId, Guid? ClienteId, Guid? AgricultorId, Guid? ParcelaId,
    string? LineaConfeccion, decimal KilosPrevistos, decimal KilosReales, decimal Desviacion, decimal? Cumplimiento, int? CajasPrevistas, int CajasReales, decimal? ImportePrevisto);

public sealed record FueraDePlanDto(Guid ProductoId, Guid? ClienteId, Guid? AgricultorId, decimal Kilos, int Cajas);

/// <summary>Plan frente a realidad: cada línea con lo real que le toca, y lo real que no encaja en ninguna línea.</summary>
public sealed record SeguimientoDto(Guid PlanId, string Tipo, string Nombre, decimal KilosPrevistos, decimal KilosReales, decimal? Cumplimiento, decimal KilosFueraDePlan,
    IReadOnlyList<SeguimientoLineaDto> Lineas, IReadOnlyList<FueraDePlanDto> FueraDePlan);

/// <summary>Una semana de un producto en el cuadro: entradas, producción y ventas, previstas y reales, y lo disponible.</summary>
public sealed record CuadroSemanaDto(DateOnly Lunes, string Semana, Guid ProductoId, decimal EntradasPrevistas, decimal EntradasReales, decimal ProduccionPrevista,
    decimal ProduccionReal, decimal VentasPrevistas, decimal VentasReales, decimal DisponiblePrevisto, decimal DisponibleReal);

public sealed record CuadroDto(Guid CampanaId, Guid? PlanComercialId, Guid? PlanProduccionId, Guid? PlanEntradasId, IReadOnlyList<CuadroSemanaDto> Semanas);

/// <summary>
/// Planificación de la campaña y seguimiento de la realidad: plan comercial (ventas por periodo, cliente y producto),
/// plan de producción (confección por día y producto) y previsión de entradas (aforo por agricultor y parcela). Lo real
/// sale del libro (expediciones, partes validados y recepciones), así que el seguimiento siempre está al día.
/// </summary>
public sealed class PlanificacionAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    public PlanificacionAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<PlanDto>> ListarAsync(Guid empresaId, Guid? campanaId, TipoPlan? tipo, CancellationToken ct = default) =>
        (await _repo.PlanesAsync(empresaId, ct).ConfigureAwait(false)).Where(p => (campanaId is null || p.CampanaId == campanaId) && (tipo is null || p.Tipo == tipo))
        .OrderBy(p => p.Tipo).ThenByDescending(p => p.Version).Select(Dto).ToList();

    public async Task<PlanDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _repo.PlanAsync(id, ct).ConfigureAwait(false) is { } p ? Dto(p) : null;

    public async Task<Resultado<PlanDto>> CrearAsync(Guid empresaId, DatosPlan d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var campana = await _repo.CampanaAsync(d.CampanaId, ct).ConfigureAwait(false);
        if (campana is null || campana.EmpresaId != empresaId)
        {
            return Resultado.Fallo<PlanDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        var version = (await _repo.PlanesAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Tipo == d.Tipo && p.CampanaId == d.CampanaId).Select(p => p.Version)
            .DefaultIfEmpty(0).Max() + 1;
        var plan = Plan.Crear(empresaId, d.Tipo, d.CampanaId, d.Nombre, version, _reloj.AhoraUtc);
        if (plan.EsFallo)
        {
            return Resultado.Fallo<PlanDto>(plan.Error);
        }

        if (d.Lineas is { Count: > 0 } lineas && plan.Valor.FijarLineas(lineas, campana.Desde, campana.Hasta) is { EsFallo: true } mal)
        {
            return Resultado.Fallo<PlanDto>(mal.Error);
        }

        _repo.Agregar(plan.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(plan.Valor));
    }

    /// <summary>Cambia el nombre y las líneas (el plan se edita entero, como una hoja). Solo en borrador.</summary>
    public async Task<Resultado<PlanDto>> CambiarAsync(Guid id, DatosCambioPlan d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var plan = await _repo.PlanAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo<PlanDto>(NoExiste());
        }

        var campana = await _repo.CampanaAsync(plan.CampanaId, ct).ConfigureAwait(false);
        var r = plan.Renombrar(d.Nombre);
        if (r.EsCorrecto)
        {
            r = plan.FijarLineas(d.Lineas ?? [], campana!.Desde, campana.Hasta);
        }

        if (r.EsFallo)
        {
            return Resultado.Fallo<PlanDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(plan));
    }

    /// <summary>Aprueba el plan: pasa a ser el que se sigue, y el aprobado anterior del mismo tipo y campaña queda cerrado.</summary>
    public async Task<Resultado<PlanDto>> AprobarAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _repo.PlanAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo<PlanDto>(NoExiste());
        }

        var r = plan.Aprobar(_reloj.AhoraUtc);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PlanDto>(r.Error);
        }

        foreach (var anterior in (await _repo.PlanesAsync(plan.EmpresaId, ct).ConfigureAwait(false))
                     .Where(p => p.Id != plan.Id && p.Tipo == plan.Tipo && p.CampanaId == plan.CampanaId && p.Estado == EstadoPlan.Aprobado))
        {
            anterior.Cerrar();
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(plan));
    }

    /// <summary>Copia del plan como versión nueva en borrador, para replanificar sin perder el aprobado.</summary>
    public async Task<Resultado<PlanDto>> NuevaVersionAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _repo.PlanAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo<PlanDto>(NoExiste());
        }

        var version = (await _repo.PlanesAsync(plan.EmpresaId, ct).ConfigureAwait(false)).Where(p => p.Tipo == plan.Tipo && p.CampanaId == plan.CampanaId).Max(p => p.Version) + 1;
        var copia = plan.NuevaVersion(version, _reloj.AhoraUtc);
        _repo.Agregar(copia);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(copia));
    }

    /// <summary>Borra un plan en borrador (uno aprobado o cerrado se conserva como historia).</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _repo.PlanAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        if (plan.Estado != EstadoPlan.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("plan.no_borrador", "Solo se borra un plan en borrador; los aprobados se conservan."));
        }

        _repo.Eliminar(plan);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<Resultado<SeguimientoDto>> SeguimientoAsync(Guid id, CancellationToken ct = default)
    {
        var plan = await _repo.PlanAsync(id, ct).ConfigureAwait(false);
        if (plan is null)
        {
            return Resultado.Fallo<SeguimientoDto>(NoExiste());
        }

        var campana = await _repo.CampanaAsync(plan.CampanaId, ct).ConfigureAwait(false);
        var hechos = await HechosAsync(plan.EmpresaId, plan.Tipo, campana!.Desde, campana.Hasta, ct).ConfigureAwait(false);
        var (porLinea, fuera) = SeguimientoPlan.Repartir(plan.Lineas, hechos);
        var lineas = plan.Lineas.OrderBy(l => l.Numero).Select(l =>
        {
            var (kilos, cajas) = porLinea[l.Id];
            return new SeguimientoLineaDto(l.Id, l.Numero, l.Desde, l.Hasta, l.ProductoId, l.ClienteId, l.AgricultorId, l.ParcelaId, l.LineaConfeccion, l.Kilos, kilos, kilos - l.Kilos,
                Porcentaje(kilos, l.Kilos), l.Cajas, cajas, l.PrecioKg is { } p ? Math.Round(p * l.Kilos, 2, MidpointRounding.AwayFromZero) : null);
        }).ToList();
        var fueraDto = fuera.GroupBy(h => (h.ProductoId, h.ClienteId, h.AgricultorId)).Select(g => new FueraDePlanDto(g.Key.ProductoId, g.Key.ClienteId, g.Key.AgricultorId,
            g.Sum(h => h.Kilos), g.Sum(h => h.Cajas))).Where(f => f.Kilos != 0m).OrderByDescending(f => f.Kilos).ToList();
        var previstos = lineas.Sum(l => l.KilosPrevistos);
        var reales = lineas.Sum(l => l.KilosReales);
        return Resultado.Ok(new SeguimientoDto(plan.Id, plan.Tipo.ToString(), plan.Nombre, previstos, reales, Porcentaje(reales, previstos), fueraDto.Sum(f => f.Kilos), lineas,
            fueraDto));
    }

    /// <summary>
    /// Cuadro semanal por producto: entradas previstas y reales, producción prevista y real, ventas previstas y reales, y
    /// lo disponible acumulado (entradas − ventas) previsto y real. Usa el último plan aprobado de cada tipo (o el indicado).
    /// Las líneas de varios días se reparten por igual entre sus días.
    /// </summary>
    public async Task<Resultado<CuadroDto>> CuadroAsync(Guid empresaId, Guid campanaId, Guid? comercialId, Guid? produccionId, Guid? entradasId, CancellationToken ct = default)
    {
        var campana = await _repo.CampanaAsync(campanaId, ct).ConfigureAwait(false);
        if (campana is null || campana.EmpresaId != empresaId)
        {
            return Resultado.Fallo<CuadroDto>(Error.NoEncontrado("campana.no_encontrada", "La campaña no existe."));
        }

        var planes = (await _repo.PlanesAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.CampanaId == campanaId).ToList();
        Plan? Elegir(TipoPlan tipo, Guid? id) => id is { } x ? planes.FirstOrDefault(p => p.Id == x && p.Tipo == tipo)
            : planes.Where(p => p.Tipo == tipo && p.Estado == EstadoPlan.Aprobado).MaxBy(p => p.Version);
        var comercial = Elegir(TipoPlan.Comercial, comercialId);
        var produccion = Elegir(TipoPlan.Produccion, produccionId);
        var entradas = Elegir(TipoPlan.Entradas, entradasId);

        var celdas = new Dictionary<(DateOnly Lunes, Guid Producto), decimal[]>();
        void Sumar(DateOnly fecha, Guid producto, int columna, decimal kilos)
        {
            var clave = (Lunes(fecha), producto);
            if (!celdas.TryGetValue(clave, out var fila))
            {
                celdas[clave] = fila = new decimal[6];
            }

            fila[columna] += kilos;
        }

        void Prever(Plan? plan, int columna)
        {
            foreach (var l in plan?.Lineas ?? [])
            {
                var dias = l.Hasta.DayNumber - l.Desde.DayNumber + 1;
                for (var d = l.Desde; d <= l.Hasta; d = d.AddDays(1))
                {
                    Sumar(d, l.ProductoId, columna, l.Kilos / dias);
                }
            }
        }

        Prever(entradas, 0);
        Prever(produccion, 2);
        Prever(comercial, 4);
        foreach (var h in await HechosAsync(empresaId, TipoPlan.Entradas, campana.Desde, campana.Hasta, ct).ConfigureAwait(false))
        {
            Sumar(h.Fecha, h.ProductoId, 1, h.Kilos);
        }

        foreach (var h in await HechosAsync(empresaId, TipoPlan.Produccion, campana.Desde, campana.Hasta, ct).ConfigureAwait(false))
        {
            Sumar(h.Fecha, h.ProductoId, 3, h.Kilos);
        }

        foreach (var h in await HechosAsync(empresaId, TipoPlan.Comercial, campana.Desde, campana.Hasta, ct).ConfigureAwait(false))
        {
            Sumar(h.Fecha, h.ProductoId, 5, h.Kilos);
        }

        var semanas = new List<CuadroSemanaDto>();
        foreach (var producto in celdas.Keys.Select(k => k.Producto).Distinct())
        {
            decimal previsto = 0m, real = 0m;
            foreach (var (clave, f) in celdas.Where(c => c.Key.Producto == producto).OrderBy(c => c.Key.Lunes))
            {
                var r = f.Select(x => Math.Round(x, 3, MidpointRounding.AwayFromZero)).ToArray();
                previsto += r[0] - r[4];
                real += r[1] - r[5];
                semanas.Add(new CuadroSemanaDto(clave.Lunes, Semana(clave.Lunes), producto, r[0], r[1], r[2], r[3], r[4], r[5], previsto, real));
            }
        }

        return Resultado.Ok(new CuadroDto(campanaId, comercial?.Id, produccion?.Id, entradas?.Id, semanas.OrderBy(s => s.Lunes).ThenBy(s => s.ProductoId).ToList()));
    }

    /// <summary>Lo real de cada tipo de plan: lo expedido (neto de anulaciones), lo confeccionado en partes validados o lo recibido.</summary>
    private async Task<IReadOnlyList<HechoReal>> HechosAsync(Guid empresaId, TipoPlan tipo, DateOnly desde, DateOnly hasta, CancellationToken ct)
    {
        switch (tipo)
        {
            case TipoPlan.Comercial:
            {
                var movs = (await _repo.MovimientosPorTipoAsync(empresaId, [TipoMovimientoPartida.Expedicion, TipoMovimientoPartida.Anulacion], desde, hasta, ct).ConfigureAwait(false))
                    .Where(m => m.DocumentoTipo == PalesAgro.DocumentoExpedicion).ToList();
                var partidas = (await _repo.PartidasAsync(movs.Select(m => m.PartidaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
                var pales = (await _repo.PalesAsync(movs.Where(m => m.PaleId is not null).Select(m => m.PaleId!.Value).Distinct().ToList(), ct).ConfigureAwait(false))
                    .ToDictionary(p => p.Id);
                // La anulación vuelve con la fecha de la anulación, pero resta de la venta: se cuenta en la fecha de su expedición.
                return movs.Select(m =>
                {
                    var pale = m.PaleId is { } pid ? pales.GetValueOrDefault(pid) : null;
                    var fecha = m.Tipo == TipoMovimientoPartida.Anulacion
                        ? movs.Where(x => x.PaleId == m.PaleId && x.PartidaId == m.PartidaId && x.Tipo == TipoMovimientoPartida.Expedicion && x.CreadoEn <= m.CreadoEn)
                            .OrderByDescending(x => x.CreadoEn).FirstOrDefault()?.Fecha ?? m.Fecha
                        : m.Fecha;
                    return new HechoReal(fecha, partidas.GetValueOrDefault(m.PartidaId)?.ProductoId ?? Guid.Empty, -m.Kilos, -m.Cajas, pale?.ClienteId);
                }).ToList();
            }

            case TipoPlan.Produccion:
                return (await _repo.PartesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false)).Where(p => p.Estado == EstadoParte.Validado)
                    .SelectMany(p => p.Salidas.Select(s => new HechoReal(p.Fecha, s.ProductoId, s.Kilos, 0))).ToList();

            default:
                return (await _repo.RecepcionesAsync(empresaId, desde, hasta, null, ct).ConfigureAwait(false)).Where(r => r.Estado == EstadoRecepcion.Confirmada)
                    .SelectMany(r => r.Lineas.Select(l => new HechoReal(r.Fecha, l.ProductoId, l.NetoKg ?? r.NetoDe(l.Id), l.Envases ?? 0, null, r.AgricultorId, l.ParcelaId)))
                    .ToList();
        }
    }

    private static DateOnly Lunes(DateOnly d) => d.AddDays(-(((int)d.DayOfWeek + 6) % 7));

    private static string Semana(DateOnly lunes) =>
        $"{ISOWeek.GetYear(lunes.ToDateTime(TimeOnly.MinValue))}-S{ISOWeek.GetWeekOfYear(lunes.ToDateTime(TimeOnly.MinValue)):D2}";

    private static decimal? Porcentaje(decimal real, decimal previsto) => previsto == 0m ? null : Math.Round(real * 100m / previsto, 1, MidpointRounding.AwayFromZero);

    private static PlanDto Dto(Plan p) => new(p.Id, p.Tipo.ToString(), p.CampanaId, p.Nombre, p.Version, p.Estado.ToString(), p.Kilos, p.CreadoEn, p.AprobadoEn,
        p.Lineas.OrderBy(l => l.Numero).Select(l => new LineaPlanDto(l.Id, l.Numero, l.Desde, l.Hasta, l.ProductoId, l.Kilos, l.ClienteId, l.AgricultorId, l.ParcelaId, l.LineaConfeccion,
            l.Cajas, l.PrecioKg, l.Notas)).ToList());

    private static Error NoExiste() => Error.NoEncontrado("plan.no_encontrado", "El plan no existe.");
}
