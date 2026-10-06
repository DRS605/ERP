using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  DTOs
// --------------------------------------------------------------------------------------------

public sealed record OperacionFinanciacionDto(
    Guid Id, string Tipo, string Codigo, string Descripcion, string? Entidad, string Estado, DateOnly FechaFormalizacion, decimal Capital,
    decimal TipoInteres, decimal TipoVigente, string Sistema, string Periodicidad, int NumeroCuotas, int CuotasCarencia, DateOnly? FechaPrimeraCuota,
    decimal ValorResidual, decimal PorcentajeIva, DateOnly? FechaVencimiento, decimal ComisionNoDisponible, DateOnly? ContabilizarDesde,
    string? CuentaLargoPlazo, string CuentaCortoPlazo, string CuentaIntereses, string CuentaTesoreria, string CuentaComisiones, string? CuentaActivo,
    decimal Pendiente, decimal CortoPlazo, decimal LargoPlazo, decimal Dispuesto, decimal Disponible, int CuotasPagadas, int CuotasTotales,
    DateOnly? ProximaFecha, decimal? ProximoImporte)
{
    public static OperacionFinanciacionDto Desde(OperacionFinanciacion o)
    {
        var cuadro = o.Cuadro();
        var (corto, largo) = o.Reparto();
        var proxima = cuadro.FirstOrDefault(c => !o.Pagada(c));
        var dispuesto = o.EsPoliza ? o.Dispuesto() : 0m;
        return new(o.Id, o.Tipo.ToString(), o.Codigo, o.Descripcion, o.Entidad, o.Estado.ToString(), o.FechaFormalizacion, o.Capital,
            o.TipoInteres, o.Revisiones.Select(r => (decimal?)r.TipoInteres).LastOrDefault() ?? o.TipoInteres, o.Sistema.ToString(), o.Periodicidad.ToString(),
            o.NumeroCuotas, o.CuotasCarencia, o.FechaPrimeraCuota, o.ValorResidual, o.PorcentajeIva, o.FechaVencimiento, o.ComisionNoDisponible,
            o.ContabilizarDesde, o.CuentaLargoPlazo, o.CuentaCortoPlazo, o.CuentaIntereses, o.CuentaTesoreria, o.CuentaComisiones, o.CuentaActivo,
            o.CapitalPendiente(), corto, largo, dispuesto, o.EsPoliza ? Redondeo.Dos(o.Capital - dispuesto) : 0m,
            cuadro.Count(o.Pagada), cuadro.Count, proxima?.Fecha ?? (o.EsPoliza && dispuesto > 0m ? o.FechaVencimiento : null),
            proxima is null ? (o.EsPoliza && dispuesto > 0m ? dispuesto : null) : GestionFinanciacion.TotalCuota(o, proxima));
    }
}

/// <summary>Fila del cuadro: Pagada (con asiento), Previa (pagada antes de llevarla aquí), Vencida (sin contabilizar) o Pendiente.</summary>
public sealed record FilaCuadroFinanciacionDto(int Numero, DateOnly Fecha, decimal Intereses, decimal Capital, decimal Cuota, decimal Iva, decimal Total,
    decimal Pendiente, bool OpcionCompra, bool Carencia, string Estado, Guid? AsientoId);

public sealed record EventoFinanciacionDto(Guid Id, int Orden, string Tipo, DateOnly Fecha, int? Numero, int? Ejercicio, decimal Importe, decimal Intereses,
    decimal Comision, decimal Iva, decimal? TipoInteres, DateOnly? Desde, Guid? AsientoId);

public sealed record DetalleFinanciacionDto(OperacionFinanciacionDto Operacion, IReadOnlyList<FilaCuadroFinanciacionDto> Cuadro,
    IReadOnlyList<EventoFinanciacionDto> Eventos, IReadOnlyList<TramoPoliza> Tramos);

/// <summary>Cuota o vencimiento pendiente de pago (para la previsión de tesorería).</summary>
public sealed record VencimientoFinanciacionDto(Guid OperacionId, string Codigo, string Concepto, DateOnly Fecha, decimal Importe, bool Vencido);

/// <summary>
/// Alta de una operación. Préstamo y leasing: capital, tipo, sistema, periodicidad, cuotas (y carencia), primera cuota
/// (por defecto un periodo después de la formalización); el leasing, además, el IVA de las cuotas, la opción de compra y
/// la cuenta del bien. Póliza: el capital es el límite, con vencimiento y comisión de no disponibilidad.
/// <see cref="ContabilizarFormalizacion"/> = false no hace el asiento de alta (la deuda ya está en la apertura) y
/// <see cref="ContabilizarDesde"/> da por pagadas fuera las cuotas anteriores a esa fecha.
/// </summary>
public sealed record DatosOperacionFinanciacion(
    TipoFinanciacion Tipo, string? Codigo, string? Descripcion, string? Entidad, DateOnly FechaFormalizacion, decimal Capital, decimal TipoInteres,
    SistemaAmortizacionPrestamo Sistema = SistemaAmortizacionPrestamo.Frances, PeriodicidadCuota Periodicidad = PeriodicidadCuota.Mensual,
    int NumeroCuotas = 0, int CuotasCarencia = 0, DateOnly? FechaPrimeraCuota = null, decimal ValorResidual = 0m, decimal PorcentajeIva = 0m,
    DateOnly? FechaVencimiento = null, decimal ComisionNoDisponible = 0m, DateOnly? ContabilizarDesde = null, bool ContabilizarFormalizacion = true,
    string? CuentaLargoPlazo = null, string? CuentaCortoPlazo = null, string? CuentaIntereses = null, string? CuentaTesoreria = null,
    string? CuentaComisiones = null, string? CuentaActivo = null);

public sealed record ResultadoLoteFinanciacionDto(int Asientos, decimal Importe, int EnPeriodosCerrados);

public interface IRepositorioFinanciacion
{
    Task<IReadOnlyList<OperacionFinanciacion>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<OperacionFinanciacion?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default);

    void Agregar(OperacionFinanciacion operacion);

    void Eliminar(OperacionFinanciacion operacion);
}

// --------------------------------------------------------------------------------------------
//  Casos de uso
// --------------------------------------------------------------------------------------------

/// <summary>
/// Préstamos, leasing y pólizas de crédito: alta (con su asiento), cuadro de amortización, cuotas contabilizadas
/// (capital, intereses e IVA del leasing contra el banco), revisión del tipo, amortización anticipada, paso de largo a
/// corto plazo al cierre, disposiciones y reintegros de la póliza y liquidación de sus intereses. Cada hecho con asiento
/// se deshace (del último al primero) con su contraasiento.
/// </summary>
public sealed class GestionFinanciacion
{
    public const string Origen = "Financiacion";

    private readonly IRepositorioFinanciacion _repo;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public GestionFinanciacion(IRepositorioFinanciacion repo, IRepositorioAsientos asientos, IRepositorioCuentas cuentas,
        IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _repo = repo; _asientos = asientos; _cuentas = cuentas; _unidad = unidad; _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<OperacionFinanciacionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .OrderBy(o => o.Estado).ThenBy(o => o.Codigo, StringComparer.Ordinal).Select(OperacionFinanciacionDto.Desde).ToList();

    public async Task<Resultado<DetalleFinanciacionDto>> DetalleAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        return o is null ? NoEncontrada<DetalleFinanciacionDto>() : Resultado.Ok(Detalle(o));
    }

    public async Task<Resultado<DetalleFinanciacionDto>> CrearAsync(Guid empresaId, DatosOperacionFinanciacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = OperacionFinanciacion.Crear(empresaId, datos.Tipo, datos.Codigo, datos.Descripcion, datos.Entidad, datos.FechaFormalizacion, datos.Capital,
            datos.TipoInteres, datos.Sistema, datos.Periodicidad, datos.NumeroCuotas, datos.CuotasCarencia, datos.FechaPrimeraCuota, datos.ValorResidual,
            datos.PorcentajeIva, datos.FechaVencimiento, datos.ComisionNoDisponible, datos.ContabilizarDesde, datos.CuentaLargoPlazo, datos.CuentaCortoPlazo,
            datos.CuentaIntereses, datos.CuentaTesoreria, datos.CuentaComisiones, datos.CuentaActivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(r.Error);
        }

        var o = r.Valor;
        if (await _repo.ExisteCodigoAsync(empresaId, o.Codigo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(Error.Conflicto("financiacion.codigo_duplicado", $"Ya hay una operación con el código {o.Codigo}."));
        }

        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        Guid? asientoId = null;
        if (datos.ContabilizarFormalizacion && !o.EsPoliza)
        {
            // Préstamo: el banco recibe el capital. Leasing: se da de alta el bien. Contra la deuda a corto y largo plazo.
            var (corto, largo) = o.Reparto();
            var lineas = new List<LineaAsiento> { new(o.Tipo == TipoFinanciacion.Leasing ? o.CuentaActivo! : o.CuentaTesoreria, o.Capital, 0m) };
            Haber(lineas, o.CuentaCortoPlazo, corto);
            Haber(lineas, o.CuentaLargoPlazo!, largo);
            var a = await AsientoAsync(empresaId, o.FechaFormalizacion, $"Formalización {Nombre(o)}", lineas, ct).ConfigureAwait(false);
            if (a.EsFallo)
            {
                return Resultado.Fallo<DetalleFinanciacionDto>(a.Error);
            }

            asientoId = a.Valor.Id;
        }

        o.AnotarFormalizacion(asientoId);
        _repo.Agregar(o);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>
    /// Contabiliza las cuotas vencidas hasta la fecha (de una operación o de todas las vigentes): capital desde la deuda a
    /// corto (o a largo, si aún no se ha reclasificado), intereses y, en el leasing, el IVA, contra el banco. Las de meses o
    /// ejercicios cerrados se quedan pendientes y se cuentan aparte.
    /// </summary>
    public async Task<Resultado<ResultadoLoteFinanciacionDto>> ContabilizarCuotasAsync(Guid empresaId, DateOnly hasta, Guid? operacionId = null,
        CancellationToken ct = default)
    {
        var operaciones = await OperacionesAsync(empresaId, operacionId, ct).ConfigureAwait(false);
        if (operaciones is null)
        {
            return NoEncontrada<ResultadoLoteFinanciacionDto>();
        }

        var guarda = await GuardaAsync(empresaId, ct).ConfigureAwait(false);
        int asientos = 0, cerradas = 0;
        var importe = 0m;
        foreach (var o in operaciones.Where(o => !o.EsPoliza && o.Estado == EstadoFinanciacion.Vigente))
        {
            foreach (var c in o.Cuadro().Where(c => !o.Pagada(c) && c.Fecha <= hasta).ToList())
            {
                if (await guarda.CerradoAsync(c.Fecha, ct).ConfigureAwait(false))
                {
                    cerradas++;
                    break; // las cuotas se pagan en orden
                }

                var iva = Iva(o, c);
                var lineas = new List<LineaAsiento>();
                Debe(lineas, c.Fecha.Year <= o.HorizonteCortoPlazo ? o.CuentaCortoPlazo : o.CuentaLargoPlazo!, c.Capital);
                Debe(lineas, o.CuentaIntereses, c.Intereses);
                Debe(lineas, PlanBasico.CuentaIvaSoportado, iva);
                var total = Redondeo.Dos(c.Capital + c.Intereses + iva);
                Guid? asientoId = null;
                if (total > 0m)
                {
                    Haber(lineas, o.CuentaTesoreria, total);
                    var a = await AsientoAsync(empresaId, c.Fecha, $"{(c.OpcionCompra ? "Opción de compra" : $"Cuota {c.Numero}/{o.NumeroCuotas}")} {Nombre(o)}",
                        lineas, ct, comprobar: false).ConfigureAwait(false);
                    if (a.EsFallo)
                    {
                        return Resultado.Fallo<ResultadoLoteFinanciacionDto>(a.Error);
                    }

                    asientoId = a.Valor.Id;
                    asientos++;
                    importe += total;
                }

                o.AnotarCuota(c, iva, asientoId);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoLoteFinanciacionDto(asientos, Redondeo.Dos(importe), cerradas));
    }

    /// <summary>
    /// Revisión del tipo (préstamo variable): desde la primera cuota posterior a la fecha, se recalcula la cuota de lo que
    /// queda con el nuevo tipo. Si cambia el capital que vence a corto plazo, un asiento lo ajusta entre largo y corto.
    /// </summary>
    public async Task<Resultado<DetalleFinanciacionDto>> RevisarTipoAsync(Guid empresaId, Guid id, DateOnly fecha, decimal tipoInteres, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        if (o.EsPoliza || o.Estado != EstadoFinanciacion.Vigente)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.estado", "Se revisa el tipo de un préstamo o leasing vigente.");
        }

        if (tipoInteres is < 0m or > 50m)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(Error.Validacion("financiacion.tipo_interes", "El tipo de interés anual va de 0 a 50 %."));
        }

        var cuadro = o.Cuadro();
        var desde = cuadro.Count(c => c.Fecha <= fecha) + 1;
        if (cuadro.Where(o.Pagada).Select(c => c.Numero).DefaultIfEmpty(0).Max() >= desde || desde > o.NumeroCuotas)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.revision_fecha", "La revisión se aplica a cuotas aún no pagadas: usa una fecha posterior a la última cuota pagada.");
        }

        var revisiones = o.Revisiones.Append(new RevisionCuadro(desde, tipoInteres)).ToList();
        var ajuste = await AjusteCortoPlazoAsync(empresaId, o, fecha, o.Cuadro(revisiones, o.Anticipos), $"Revisión del tipo al {Redondeo.Formatear(tipoInteres)} % {Nombre(o)}", ct).ConfigureAwait(false);
        if (ajuste.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(ajuste.Error);
        }

        o.AnotarRevision(fecha, desde, tipoInteres, ajuste.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>
    /// Amortización anticipada: se devuelve capital (y la comisión, si la hay) en la fecha, con las cuotas vencidas ya
    /// pagadas. Se mantiene el plazo y baja la cuota; si se devuelve todo lo pendiente, la operación queda cancelada.
    /// En el leasing solo se cancela del todo.
    /// </summary>
    public async Task<Resultado<DetalleFinanciacionDto>> AmortizarAnticipadamenteAsync(Guid empresaId, Guid id, DateOnly fecha, decimal importe, decimal comision,
        CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        if (o.EsPoliza || o.Estado != EstadoFinanciacion.Vigente)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.estado", "Se amortiza anticipadamente un préstamo o leasing vigente.");
        }

        var cuadro = o.Cuadro();
        var vencidas = cuadro.Where(c => c.Fecha <= fecha).ToList();
        if (vencidas.Any(c => !o.Pagada(c)))
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.cuotas_pendientes", "Contabiliza antes las cuotas vencidas hasta esa fecha.");
        }

        var tras = vencidas.Count;
        if (tras >= o.NumeroCuotas)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.sin_cuotas", "Ya no quedan cuotas que anticipar.");
        }

        var pendiente = o.CapitalPendiente();
        if (importe <= 0m || decimal.Round(importe, 2) != importe || importe > pendiente)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(Error.Validacion("financiacion.importe", $"El importe es positivo y como mucho el capital pendiente ({Redondeo.Formatear(pendiente)})."));
        }

        if (o.Tipo == TipoFinanciacion.Leasing && importe != pendiente)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.leasing_parcial", "El leasing se cancela anticipadamente por todo el capital pendiente (opción de compra incluida).");
        }

        if (comision < 0m || decimal.Round(comision, 2) != comision)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(Error.Validacion("financiacion.comision", "La comisión no puede ser negativa (2 decimales como mucho)."));
        }

        var antes = o.Reparto();
        var despues = o.Reparto(o.Cuadro(o.Revisiones, o.Anticipos.Append(new AnticipoCuadro(tras, importe)).ToList()), o.HorizonteCortoPlazo);
        var lineas = new List<LineaAsiento>();
        Debe(lineas, o.CuentaCortoPlazo, Redondeo.Dos(antes.Corto - despues.Corto));
        Debe(lineas, o.CuentaLargoPlazo!, Redondeo.Dos(antes.Largo - despues.Largo));
        Debe(lineas, o.CuentaComisiones, comision);
        Haber(lineas, o.CuentaTesoreria, Redondeo.Dos(importe + comision));
        var a = await AsientoAsync(empresaId, fecha, $"{(importe == pendiente ? "Cancelación" : "Amortización")} anticipada {Nombre(o)}", lineas, ct).ConfigureAwait(false);
        if (a.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(a.Error);
        }

        o.AnotarAnticipo(fecha, tras, importe, comision, a.Valor.Id);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>
    /// Cierre del ejercicio: pasa a corto plazo el capital de las cuotas que vencen el año siguiente (asiento a 31/12, largo
    /// al debe y corto al haber), en todas las operaciones vigentes. Si faltan ejercicios anteriores, los hace también.
    /// </summary>
    public async Task<Resultado<ResultadoLoteFinanciacionDto>> ReclasificarAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var guarda = await GuardaAsync(empresaId, ct).ConfigureAwait(false);
        int asientos = 0, cerradas = 0;
        var importe = 0m;
        foreach (var o in (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(o => !o.EsPoliza && o.Estado == EstadoFinanciacion.Vigente))
        {
            while (o.HorizonteCortoPlazo <= ejercicio)
            {
                var anio = o.HorizonteCortoPlazo;
                var fecha = new DateOnly(anio, 12, 31);
                var capital = Redondeo.Dos(o.Cuadro().Where(c => !o.Pagada(c) && c.Fecha.Year == anio + 1).Sum(c => c.Capital));
                Guid? asientoId = null;
                if (capital > 0m)
                {
                    if (await guarda.CerradoAsync(fecha, ct).ConfigureAwait(false))
                    {
                        cerradas++;
                        break;
                    }

                    var a = await AsientoAsync(empresaId, fecha, $"Traspaso a corto plazo {anio + 1} {Nombre(o)}",
                        [new LineaAsiento(o.CuentaLargoPlazo!, capital, 0m), new LineaAsiento(o.CuentaCortoPlazo, 0m, capital)], ct, comprobar: false).ConfigureAwait(false);
                    if (a.EsFallo)
                    {
                        return Resultado.Fallo<ResultadoLoteFinanciacionDto>(a.Error);
                    }

                    asientoId = a.Valor.Id;
                    asientos++;
                    importe += capital;
                }

                o.AnotarReclasificacion(anio, capital, asientoId);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoLoteFinanciacionDto(asientos, Redondeo.Dos(importe), cerradas));
    }

    /// <summary>Póliza: disposición (el banco abona, crece la deuda) o reintegro (se devuelve), en orden de fechas.</summary>
    public async Task<Resultado<DetalleFinanciacionDto>> MoverPolizaAsync(Guid empresaId, Guid id, bool disposicion, DateOnly fecha, decimal importe,
        CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        if (!o.EsPoliza || o.Estado != EstadoFinanciacion.Vigente)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.estado", "Se dispone o reintegra de una póliza de crédito abierta.");
        }

        if (importe <= 0m || decimal.Round(importe, 2) != importe)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(Error.Validacion("financiacion.importe", "El importe es positivo, con 2 decimales como mucho."));
        }

        var ultimo = o.Tramos()[^1].Desde;
        if (fecha < o.FechaFormalizacion || fecha < ultimo || fecha < o.InicioSiguienteLiquidacion)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.fecha", $"Los movimientos van en orden: la fecha es del {Max(ultimo, o.InicioSiguienteLiquidacion):dd/MM/yyyy} en adelante (y no en un periodo ya liquidado).");
        }

        if (disposicion && fecha > o.FechaVencimiento)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.vencida", "La póliza ha vencido: ya no se dispone de ella.");
        }

        var dispuesto = o.Dispuesto();
        if (disposicion && dispuesto + importe > o.Capital)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.limite", $"Supera el límite: quedan {Redondeo.Formatear(o.Capital - dispuesto)} disponibles.");
        }

        if (!disposicion && importe > dispuesto)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.reintegro", $"Solo hay {Redondeo.Formatear(dispuesto)} dispuestos.");
        }

        LineaAsiento[] lineas = disposicion
            ? [new(o.CuentaTesoreria, importe, 0m), new(o.CuentaCortoPlazo, 0m, importe)]
            : [new(o.CuentaCortoPlazo, importe, 0m), new(o.CuentaTesoreria, 0m, importe)];
        var a = await AsientoAsync(empresaId, fecha, $"{(disposicion ? "Disposición" : "Reintegro")} {Nombre(o)}", lineas, ct).ConfigureAwait(false);
        if (a.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(a.Error);
        }

        o.AnotarMovimientoPoliza(disposicion, fecha, importe, a.Valor.Id);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>
    /// Póliza: liquida los intereses desde la anterior liquidación hasta la fecha: lo dispuesto cada día por el tipo
    /// (año de 365 días) y la comisión de no disponibilidad sobre lo no dispuesto; asiento de intereses y comisión contra el banco.
    /// </summary>
    public async Task<Resultado<DetalleFinanciacionDto>> LiquidarPolizaAsync(Guid empresaId, Guid id, DateOnly hasta, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        if (!o.EsPoliza)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.no_es_poliza", "Los intereses se liquidan en las pólizas; los préstamos los llevan en sus cuotas.");
        }

        var desde = o.InicioSiguienteLiquidacion;
        if (hasta < desde)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.liquidada", $"Ya está liquidada hasta el {desde.AddDays(-1):dd/MM/yyyy}.");
        }

        var (intereses, comision) = InteresesPoliza(o, desde, hasta);
        Guid? asientoId = null;
        if (intereses + comision > 0m)
        {
            var lineas = new List<LineaAsiento>();
            Debe(lineas, o.CuentaIntereses, intereses);
            Debe(lineas, o.CuentaComisiones, comision);
            Haber(lineas, o.CuentaTesoreria, Redondeo.Dos(intereses + comision));
            var a = await AsientoAsync(empresaId, hasta, $"Liquidación de intereses {desde:dd/MM/yyyy}-{hasta:dd/MM/yyyy} {Nombre(o)}", lineas, ct).ConfigureAwait(false);
            if (a.EsFallo)
            {
                return Resultado.Fallo<DetalleFinanciacionDto>(a.Error);
            }

            asientoId = a.Valor.Id;
        }

        o.AnotarLiquidacion(desde, hasta, intereses, comision, asientoId);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>Intereses (por lo dispuesto) y comisión de no disponibilidad (por lo no dispuesto) de un periodo, día a día.</summary>
    public static (decimal Intereses, decimal Comision) InteresesPoliza(OperacionFinanciacion o, DateOnly desde, DateOnly hasta)
    {
        ArgumentNullException.ThrowIfNull(o);
        var tramos = o.Tramos();
        decimal dispuestoDias = 0m, libreDias = 0m;
        for (var d = desde; d <= hasta; d = d.AddDays(1))
        {
            var saldo = tramos.Last(t => t.Desde <= d).Dispuesto;
            dispuestoDias += saldo;
            libreDias += o.Capital - saldo;
        }

        return (Redondeo.Dos(dispuestoDias * o.TipoInteres / 100m / 365m), Redondeo.Dos(libreDias * o.ComisionNoDisponible / 100m / 365m));
    }

    /// <summary>Deshace el último hecho de la operación con el contraasiento de su asiento (en su fecha o en la indicada).</summary>
    public async Task<Resultado<DetalleFinanciacionDto>> DeshacerAsync(Guid empresaId, Guid id, Guid eventoId, DateOnly? fecha, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        var e = o.UltimoEvento;
        if (e is null || e.Id != eventoId)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.no_es_el_ultimo", "Solo se deshace el último hecho de la operación.");
        }

        if (e.Tipo == TipoEventoFinanciacion.Formalizacion)
        {
            return Conflicto<DetalleFinanciacionDto>("financiacion.formalizacion", "La formalización no se deshace: elimina la operación.");
        }

        if (e.AsientoId is { } asientoId)
        {
            var anulado = await AnularAsientoAsync(empresaId, asientoId, fecha, ct).ConfigureAwait(false);
            if (anulado is not null)
            {
                return Resultado.Fallo<DetalleFinanciacionDto>(anulado);
            }
        }

        var r = o.QuitarUltimo(eventoId);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>Cambia la descripción y la entidad (lo demás se corrige eliminando y volviendo a dar de alta).</summary>
    public async Task<Resultado<DetalleFinanciacionDto>> RenombrarAsync(Guid empresaId, Guid id, string? descripcion, string? entidad, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        var r = o.Renombrar(descripcion, entidad);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    public async Task<Resultado<DetalleFinanciacionDto>> CerrarPolizaAsync(Guid empresaId, Guid id, bool cerrar, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return NoEncontrada<DetalleFinanciacionDto>();
        }

        var r = o.CerrarPoliza(cerrar);
        if (r.EsFallo)
        {
            return Resultado.Fallo<DetalleFinanciacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Detalle(o));
    }

    /// <summary>Elimina una operación dada de alta por error: sin más hechos que la formalización (cuyo asiento se anula).</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var o = await ObtenerAsync(empresaId, id, ct).ConfigureAwait(false);
        if (o is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("financiacion.no_encontrada", "La operación no existe."));
        }

        if (o.Eventos.Any(e => e.Tipo != TipoEventoFinanciacion.Formalizacion))
        {
            return Resultado.Fallo(Error.Conflicto("financiacion.con_hechos", "Tiene cuotas, movimientos o revisiones: deshazlos antes de eliminarla."));
        }

        if ((o.Eventos.Count > 0 ? o.Eventos[0].AsientoId : null) is { } asientoId && await AnularAsientoAsync(empresaId, asientoId, null, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo(error);
        }

        _repo.Eliminar(o);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Cuotas pendientes de las operaciones vigentes y el vencimiento de lo dispuesto de las pólizas.</summary>
    public async Task<IReadOnlyList<VencimientoFinanciacionDto>> VencimientosAsync(Guid empresaId, DateOnly? hasta, CancellationToken ct = default)
    {
        var hoy = Hoy;
        var lista = new List<VencimientoFinanciacionDto>();
        foreach (var o in (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(o => o.Estado == EstadoFinanciacion.Vigente))
        {
            if (o.EsPoliza)
            {
                var dispuesto = o.Dispuesto();
                if (dispuesto > 0m && (hasta is null || o.FechaVencimiento <= hasta))
                {
                    lista.Add(new(o.Id, o.Codigo, $"Vencimiento de la póliza {o.Codigo}", o.FechaVencimiento!.Value, dispuesto, o.FechaVencimiento < hoy));
                }

                continue;
            }

            foreach (var c in o.Cuadro().Where(c => !o.Pagada(c) && (hasta is null || c.Fecha <= hasta)))
            {
                var texto = c.OpcionCompra ? "opción de compra" : $"cuota {c.Numero}/{o.NumeroCuotas}";
                lista.Add(new(o.Id, o.Codigo, $"{Nombre(o)} · {texto}", c.Fecha, TotalCuota(o, c), c.Fecha < hoy));
            }
        }

        return lista.OrderBy(v => v.Fecha).ToList();
    }

    // ------------------------------------------------------------------ auxiliares

    internal static decimal Iva(OperacionFinanciacion o, CuotaCuadro c) =>
        o.Tipo == TipoFinanciacion.Leasing ? Redondeo.Dos(c.Cuota * o.PorcentajeIva / 100m) : 0m;

    internal static decimal TotalCuota(OperacionFinanciacion o, CuotaCuadro c) => Redondeo.Dos(c.Cuota + Iva(o, c));

    private DetalleFinanciacionDto Detalle(OperacionFinanciacion o)
    {
        var hoy = Hoy;
        var filas = o.Cuadro().Select(c =>
        {
            var evento = o.EventoCuota(c.Numero);
            var iva = evento?.Iva ?? Iva(o, c);
            var estado = evento is not null ? "Pagada" : o.EsPrevia(c) ? "Previa" : c.Fecha < hoy ? "Vencida" : "Pendiente";
            return new FilaCuadroFinanciacionDto(c.Numero, c.Fecha, c.Intereses, c.Capital, c.Cuota, iva, Redondeo.Dos(c.Cuota + iva), c.Pendiente,
                c.OpcionCompra, c.Numero <= o.CuotasCarencia, estado, evento?.AsientoId);
        }).ToList();
        var eventos = o.Eventos.OrderBy(e => e.Orden).Select(e => new EventoFinanciacionDto(e.Id, e.Orden, e.Tipo.ToString(), e.Fecha, e.Numero, e.Ejercicio,
            e.Importe, e.Intereses, e.Comision, e.Iva, e.TipoInteres, e.Desde, e.AsientoId)).ToList();
        return new DetalleFinanciacionDto(OperacionFinanciacionDto.Desde(o), filas, eventos, o.EsPoliza ? o.Tramos() : []);
    }

    /// <summary>Asiento que lleva entre largo y corto plazo la diferencia del capital a corto plazo con un cuadro nuevo.</summary>
    private async Task<Resultado<Guid?>> AjusteCortoPlazoAsync(Guid empresaId, OperacionFinanciacion o, DateOnly fecha, IReadOnlyList<CuotaCuadro> nuevo,
        string concepto, CancellationToken ct)
    {
        var antes = o.Reparto();
        var despues = o.Reparto(nuevo, o.HorizonteCortoPlazo);
        var delta = Redondeo.Dos(despues.Corto - antes.Corto);
        if (delta == 0m)
        {
            return Resultado.Ok<Guid?>(null);
        }

        LineaAsiento[] lineas = delta > 0m
            ? [new(o.CuentaLargoPlazo!, delta, 0m), new(o.CuentaCortoPlazo, 0m, delta)]
            : [new(o.CuentaCortoPlazo, -delta, 0m), new(o.CuentaLargoPlazo!, 0m, -delta)];
        var a = await AsientoAsync(empresaId, fecha, concepto, lineas, ct).ConfigureAwait(false);
        return a.EsFallo ? Resultado.Fallo<Guid?>(a.Error) : Resultado.Ok<Guid?>(a.Valor.Id);
    }

    private async Task<Error?> AnularAsientoAsync(Guid empresaId, Guid asientoId, DateOnly? fecha, CancellationToken ct)
    {
        var original = await _asientos.ObtenerAsync(asientoId, ct).ConfigureAwait(false);
        if (original is null || await _asientos.EstaAnuladoAsync(asientoId, ct).ConfigureAwait(false))
        {
            return null;
        }

        var dia = fecha ?? original.Fecha;
        if (dia < original.Fecha)
        {
            return Error.Validacion("financiacion.fecha_anulacion", $"La anulación no puede ser anterior al asiento ({original.Fecha:dd/MM/yyyy}).");
        }

        if (await ComprobarFechaAsync(empresaId, dia, ct).ConfigureAwait(false) is { } error)
        {
            return error;
        }

        var numero = await _asientos.SiguienteNumeroAsync(empresaId, dia.Year, ct).ConfigureAwait(false);
        var contra = Asiento.CrearAnulacion(original, numero, dia, _reloj);
        if (contra.EsFallo)
        {
            return contra.Error;
        }

        _asientos.Agregar(contra.Valor);
        return null;
    }

    private async Task<Error?> ComprobarFechaAsync(Guid empresaId, DateOnly fecha, CancellationToken ct)
    {
        if (await _asientos.TieneCierreAsync(empresaId, fecha.Year, ct).ConfigureAwait(false))
        {
            return Error.Conflicto("asiento.ejercicio_cerrado", $"El ejercicio {fecha.Year} está cerrado; indica una fecha de un ejercicio abierto.");
        }

        return await PeriodosContables.ComprobarAsync(_asientos, empresaId, fecha, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<Asiento>> AsientoAsync(Guid empresaId, DateOnly fecha, string concepto, IReadOnlyList<LineaAsiento> lineas, CancellationToken ct,
        bool comprobar = true)
    {
        if (comprobar && await ComprobarFechaAsync(empresaId, fecha, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<Asiento>(error);
        }

        var texto = concepto.Length > Asiento.LongitudMaximaConcepto ? concepto[..Asiento.LongitudMaximaConcepto] : concepto;
        var numero = await _asientos.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var a = Asiento.Crear(empresaId, fecha.Year, numero, fecha, texto, Origen, lineas.Select(l => l with { Concepto = l.Concepto ?? texto }).ToList(), _reloj);
        if (a.EsCorrecto)
        {
            _asientos.Agregar(a.Valor);
        }

        return a;
    }

    private async Task<GuardaPeriodos> GuardaAsync(Guid empresaId, CancellationToken ct) =>
        new(_asientos, empresaId, await _asientos.CerradoHastaAsync(empresaId, ct).ConfigureAwait(false));

    /// <summary>Recuerda qué ejercicios están cerrados durante un lote.</summary>
    private sealed class GuardaPeriodos(IRepositorioAsientos asientos, Guid empresaId, DateOnly? cerradoHasta)
    {
        private readonly Dictionary<int, bool> _ejercicios = [];

        public async Task<bool> CerradoAsync(DateOnly fecha, CancellationToken ct)
        {
            if (!_ejercicios.TryGetValue(fecha.Year, out var cerrado))
            {
                cerrado = await asientos.TieneCierreAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
                _ejercicios[fecha.Year] = cerrado;
            }

            return cerrado || PeriodosContables.Cerrado(cerradoHasta, fecha) is not null;
        }
    }

    private async Task<IReadOnlyList<OperacionFinanciacion>?> OperacionesAsync(Guid empresaId, Guid? id, CancellationToken ct)
    {
        if (id is null)
        {
            return await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false);
        }

        return await ObtenerAsync(empresaId, id.Value, ct).ConfigureAwait(false) is { } o ? [o] : null;
    }

    private async Task<OperacionFinanciacion?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is { } o && o.EmpresaId == empresaId ? o : null;

    private static string Nombre(OperacionFinanciacion o) => $"{(o.Tipo switch
    {
        TipoFinanciacion.Leasing => "leasing",
        TipoFinanciacion.Poliza => "póliza",
        _ => "préstamo",
    })} {o.Codigo}";

    private static void Debe(List<LineaAsiento> lineas, string cuenta, decimal importe)
    {
        if (importe > 0m)
        {
            lineas.Add(new LineaAsiento(cuenta, importe, 0m));
        }
    }

    private static void Haber(List<LineaAsiento> lineas, string cuenta, decimal importe)
    {
        if (importe > 0m)
        {
            lineas.Add(new LineaAsiento(cuenta, 0m, importe));
        }
    }

    private static DateOnly Max(DateOnly a, DateOnly b) => a > b ? a : b;

    private static Resultado<T> NoEncontrada<T>() => Resultado.Fallo<T>(Error.NoEncontrado("financiacion.no_encontrada", "La operación no existe."));

    private static Resultado<T> Conflicto<T>(string codigo, string mensaje) => Resultado.Fallo<T>(Error.Conflicto(codigo, mensaje));
}
