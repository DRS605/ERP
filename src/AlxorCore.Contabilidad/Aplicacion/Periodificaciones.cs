using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

public sealed record CuotaPeriodificacionDto(Guid Id, int Ejercicio, int Mes, DateOnly Fecha, decimal Importe, Guid? AsientoId);

public sealed record PeriodificacionDto(Guid Id, string Descripcion, string Tipo, string CuentaResultado, string CuentaPeriodificacion, decimal Importe,
    DateOnly Fecha, int EjercicioInicio, int MesInicio, int Meses, string Estado, decimal Imputado, decimal Pendiente, Guid? AsientoReclasificacionId,
    Guid? AsientoCancelacionId, IReadOnlyList<CuotaPeriodificacionDto> Cuotas)
{
    public static PeriodificacionDto Desde(Periodificacion p) => new(p.Id, p.Descripcion, p.Tipo.ToString(), p.CuentaResultado, p.CuentaPeriodificacion,
        p.Importe, p.Fecha, p.EjercicioInicio, p.MesInicio, p.Meses, p.Estado.ToString(), p.Imputado, p.Pendiente, p.AsientoReclasificacionId,
        p.AsientoCancelacionId, p.Cuotas.OrderBy(c => c.Ejercicio).ThenBy(c => c.Mes)
            .Select(c => new CuotaPeriodificacionDto(c.Id, c.Ejercicio, c.Mes, c.Fecha, c.Importe, c.AsientoId)).ToList());
}

/// <summary>
/// Alta de una periodificación. Sin inicio, empieza el mes de la fecha. Con <see cref="Reclasificar"/>, un asiento en
/// esa fecha saca el importe de la cuenta de resultados (donde lo dejó la factura) a la de periodificación.
/// </summary>
public sealed record DatosPeriodificacion(string? Descripcion, TipoPeriodificacion Tipo, string? CuentaResultado, decimal Importe, DateOnly Fecha,
    int Meses, int? AnioInicio = null, int? MesInicio = null, string? CuentaPeriodificacion = null, bool Reclasificar = true);

/// <summary>Resultado de generar las cuotas hasta una fecha.</summary>
public sealed record GeneracionPeriodificacionesDto(int Asientos, decimal Importe, int EnMesesCerrados);

public interface IRepositorioPeriodificaciones
{
    Task<IReadOnlyList<Periodificacion>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<Periodificacion?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(Periodificacion periodificacion);

    void Eliminar(Periodificacion periodificacion);
}

/// <summary>Periodificaciones de gastos e ingresos: alta, cuotas mensuales, cancelación y anulación.</summary>
public sealed class GestionPeriodificaciones
{
    public const string Origen = "Periodificacion";

    private readonly IRepositorioPeriodificaciones _repo;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public GestionPeriodificaciones(IRepositorioPeriodificaciones repo, IRepositorioAsientos asientos, IRepositorioCuentas cuentas,
        IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _repo = repo; _asientos = asientos; _cuentas = cuentas; _unidad = unidad; _reloj = reloj;
    }

    public async Task<IReadOnlyList<PeriodificacionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .OrderByDescending(p => p.Estado == EstadoPeriodificacion.Activa).ThenByDescending(p => p.Fecha).Select(PeriodificacionDto.Desde).ToList();

    public async Task<Resultado<PeriodificacionDto>> CrearAsync(Guid empresaId, DatosPeriodificacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var p = Periodificacion.Crear(empresaId, datos.Descripcion, datos.Tipo, datos.CuentaResultado, datos.CuentaPeriodificacion, datos.Importe,
            datos.Fecha, datos.AnioInicio ?? datos.Fecha.Year, datos.MesInicio ?? datos.Fecha.Month, datos.Meses);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PeriodificacionDto>(p.Error);
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        if (datos.Reclasificar)
        {
            // Gasto: 480 al debe, 6xx al haber. Ingreso: 7xx al debe, 485 al haber.
            var gasto = p.Valor.Tipo == TipoPeriodificacion.Gasto;
            var r = await AsientoAsync(empresaId, datos.Fecha, $"Periodificación · {p.Valor.Descripcion}",
                gasto ? p.Valor.CuentaPeriodificacion : p.Valor.CuentaResultado, gasto ? p.Valor.CuentaResultado : p.Valor.CuentaPeriodificacion, p.Valor.Importe, ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo<PeriodificacionDto>(r.Error);
            }

            p.Valor.MarcarReclasificada(r.Valor.Id);
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PeriodificacionDto.Desde(p.Valor));
    }

    /// <summary>
    /// Contabiliza las cuotas pendientes de las periodificaciones activas hasta la fecha (al último día de cada mes). Las
    /// de meses o ejercicios cerrados no se contabilizan: se cuentan aparte.
    /// </summary>
    public async Task<Resultado<GeneracionPeriodificacionesDto>> GenerarAsync(Guid empresaId, DateOnly hasta, CancellationToken ct = default)
    {
        var cerradoHasta = await _asientos.CerradoHastaAsync(empresaId, ct).ConfigureAwait(false);
        var ejerciciosCerrados = new Dictionary<int, bool>();
        int asientos = 0, cerradas = 0;
        var importe = 0m;
        foreach (var p in (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Estado == EstadoPeriodificacion.Activa))
        {
            foreach (var c in p.Cuotas.Where(c => c.AsientoId is null && c.Fecha <= hasta).OrderBy(c => c.Ejercicio).ThenBy(c => c.Mes).ToList())
            {
                if (!ejerciciosCerrados.TryGetValue(c.Ejercicio, out var cerrado))
                {
                    cerrado = await _asientos.TieneCierreAsync(empresaId, c.Ejercicio, ct).ConfigureAwait(false);
                    ejerciciosCerrados[c.Ejercicio] = cerrado;
                }

                if (cerrado || PeriodosContables.Cerrado(cerradoHasta, c.Fecha) is not null)
                {
                    cerradas++;
                    continue;
                }

                // Gasto: 6xx al debe, 480 al haber. Ingreso: 485 al debe, 7xx al haber.
                var gasto = p.Tipo == TipoPeriodificacion.Gasto;
                var r = await AsientoAsync(empresaId, c.Fecha, $"Periodificación {c.Mes:D2}/{c.Ejercicio} · {p.Descripcion}",
                    gasto ? p.CuentaResultado : p.CuentaPeriodificacion, gasto ? p.CuentaPeriodificacion : p.CuentaResultado, c.Importe, ct, comprobar: false).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    return Resultado.Fallo<GeneracionPeriodificacionesDto>(r.Error);
                }

                p.ContabilizarCuota(c.Id, r.Valor.Id);
                asientos++;
                importe += c.Importe;
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new GeneracionPeriodificacionesDto(asientos, importe, cerradas));
    }

    /// <summary>Termina antes: lo pendiente de imputar va a resultados de una vez, en la fecha indicada.</summary>
    public async Task<Resultado<PeriodificacionDto>> CancelarAsync(Guid empresaId, Guid id, DateOnly fecha, CancellationToken ct = default)
    {
        var p = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return NoEncontrada();
        }

        if (p.Estado != EstadoPeriodificacion.Activa)
        {
            return Resultado.Fallo<PeriodificacionDto>(Error.Conflicto("periodificacion.estado", "Solo se cancela una periodificación activa."));
        }

        var gasto = p.Tipo == TipoPeriodificacion.Gasto;
        var r = await AsientoAsync(empresaId, fecha, $"Cancelación de la periodificación · {p.Descripcion}",
            gasto ? p.CuentaResultado : p.CuentaPeriodificacion, gasto ? p.CuentaPeriodificacion : p.CuentaResultado, p.Pendiente, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PeriodificacionDto>(r.Error);
        }

        p.Cancelar(r.Valor.Id);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PeriodificacionDto.Desde(p));
    }

    /// <summary>Anula todos sus asientos con contraasientos en la fecha indicada (el importe vuelve a donde estaba).</summary>
    public async Task<Resultado<PeriodificacionDto>> AnularAsync(Guid empresaId, Guid id, DateOnly fecha, CancellationToken ct = default)
    {
        var p = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return NoEncontrada();
        }

        if (p.Estado == EstadoPeriodificacion.Anulada)
        {
            return Resultado.Fallo<PeriodificacionDto>(Error.Conflicto("periodificacion.anulada", "La periodificación ya está anulada."));
        }

        if (await ComprobarFechaAsync(empresaId, fecha, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<PeriodificacionDto>(error);
        }

        foreach (var asientoId in p.Asientos())
        {
            var original = await _asientos.ObtenerAsync(asientoId, ct).ConfigureAwait(false);
            if (original is null || await _asientos.EstaAnuladoAsync(asientoId, ct).ConfigureAwait(false))
            {
                continue;
            }

            if (fecha < original.Fecha)
            {
                return Resultado.Fallo<PeriodificacionDto>(Error.Validacion("periodificacion.fecha_anulacion",
                    $"La anulación no puede ser anterior a sus asientos (el último es del {original.Fecha:dd/MM/yyyy})."));
            }

            var numero = await _asientos.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
            var contra = Asiento.CrearAnulacion(original, numero, fecha, _reloj);
            if (contra.EsFallo)
            {
                return Resultado.Fallo<PeriodificacionDto>(contra.Error);
            }

            _asientos.Agregar(contra.Valor);
        }

        p.Anular();
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PeriodificacionDto.Desde(p));
    }

    /// <summary>Elimina una periodificación sin asientos (dada de alta por error).</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var p = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("periodificacion.no_encontrada", "La periodificación no existe."));
        }

        if (p.TieneAsientos)
        {
            return Resultado.Fallo(Error.Conflicto("periodificacion.con_asientos", "Tiene asientos: anúlala en lugar de eliminarla."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<Periodificacion?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p && p.EmpresaId == empresaId ? p : null;

    private static Resultado<PeriodificacionDto> NoEncontrada() =>
        Resultado.Fallo<PeriodificacionDto>(Error.NoEncontrado("periodificacion.no_encontrada", "La periodificación no existe."));

    private async Task<Error?> ComprobarFechaAsync(Guid empresaId, DateOnly fecha, CancellationToken ct)
    {
        if (await _asientos.TieneCierreAsync(empresaId, fecha.Year, ct).ConfigureAwait(false))
        {
            return Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {fecha.Year} está cerrado; indica una fecha de un ejercicio abierto.");
        }

        return await PeriodosContables.ComprobarAsync(_asientos, empresaId, fecha, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<Asiento>> AsientoAsync(Guid empresaId, DateOnly fecha, string concepto, string cuentaDebe, string cuentaHaber, decimal importe,
        CancellationToken ct, bool comprobar = true)
    {
        if (comprobar && await ComprobarFechaAsync(empresaId, fecha, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<Asiento>(error);
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var texto = concepto.Length > Asiento.LongitudMaximaConcepto ? concepto[..Asiento.LongitudMaximaConcepto] : concepto;
        var a = Asiento.Crear(empresaId, fecha.Year, numero, fecha, texto, Origen,
            [new LineaAsiento(cuentaDebe, importe, 0m, texto), new LineaAsiento(cuentaHaber, 0m, importe, texto)], _reloj);
        if (a.EsCorrecto)
        {
            _asientos.Agregar(a.Valor);
        }

        return a;
    }
}
