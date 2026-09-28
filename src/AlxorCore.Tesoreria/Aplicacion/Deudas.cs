using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

public sealed record ConfiguracionCarteraDto(bool ImpagadosA4315, decimal PorcentajeRenovacion);

public sealed record SituacionDeudaDto(Guid Id, string TipoDocumento, Guid DocumentoId, string Documento, Guid? TerceroId, string Tercero, string? Cuenta,
    decimal Importe, string Clasificacion, DateOnly? FechaClasificacion, decimal Dotado, DateOnly? IncobrableEl, decimal Pendiente);

public sealed record ClasificarDeudaComando(ClasificacionDeuda Clasificacion, DateOnly? Fecha = null, decimal? PorcentajeDotacion = null);

public sealed record DotarDeudaComando(decimal? Importe = null, decimal? Porcentaje = null, DateOnly? Fecha = null);

public sealed record IncobrableComando(DateOnly? Fecha = null, string? Motivo = null);

public sealed record VencimientoRenovacion(DateOnly Fecha, decimal Importe);

public sealed record RenovarComando(IReadOnlyList<VencimientoRenovacion> Vencimientos, decimal? Gastos = null, DateOnly? Fecha = null);

public sealed record EfectoRenovacionDto(Guid Id, string Documento, DateOnly Vencimiento, decimal Importe, decimal Pendiente);

public sealed record RenovacionDto(Guid Id, string TipoDocumento, Guid DocumentoId, string Documento, string Tercero, DateOnly Fecha, decimal Importe, decimal Gastos,
    bool Anulada, IReadOnlyList<EfectoRenovacionDto> Efectos);

/// <summary>
/// Situación de la deuda de clientes, como la gestión de impagados y de dudoso cobro de Hispatec:
/// <list type="bullet">
/// <item>Impagados: con la opción de la empresa, un recibo devuelto pasa de la cuenta del cliente a 4315.</item>
/// <item>Clasificación (dudoso, precontencioso, contencioso, moroso): la deuda pasa a 436 y se dota su deterioro (694/490).</item>
/// <item>Incobrable: la deuda que queda se lleva a pérdidas (650) y se aplica el deterioro (490/794).</item>
/// <item>Renovación: el documento se cancela contra efectos en cartera (4310) y se crean los efectos nuevos, con gastos a 769.</item>
/// </list>
/// Al cobrarse, lo cobrado vuelve antes a la cuenta de origen (lo hace la contabilización del cobro).
/// </summary>
public sealed class GestionDeudas
{
    private readonly IRepositorioDeudas _deudas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioCartera _cartera;
    private readonly IConsultaFacturas _facturas;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria _contabilizacion;

    public GestionDeudas(IRepositorioDeudas deudas, IRepositorioMovimientos movimientos, IRepositorioCartera cartera, IConsultaFacturas facturas,
        IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria contabilizacion)
    {
        _deudas = deudas;
        _movimientos = movimientos;
        _cartera = cartera;
        _facturas = facturas;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ configuración
    public async Task<ConfiguracionCarteraDto> ConfiguracionAsync(CancellationToken ct = default) =>
        await _deudas.ConfiguracionAsync(ct).ConfigureAwait(false) is { } c ? new(c.ImpagadosA4315, c.PorcentajeRenovacion) : new(false, 0m);

    public async Task<Resultado<ConfiguracionCarteraDto>> GuardarConfiguracionAsync(Guid empresaId, ConfiguracionCarteraDto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _deudas.ConfiguracionAsync(ct).ConfigureAwait(false);
        if (c is null)
        {
            c = ConfiguracionCartera.Crear(empresaId);
            _deudas.Agregar(c);
        }

        var r = c.Cambiar(datos.ImpagadosA4315, datos.PorcentajeRenovacion);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionCarteraDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ConfiguracionCarteraDto(c.ImpagadosA4315, c.PorcentajeRenovacion));
    }

    // ------------------------------------------------------------------ consulta
    public async Task<IReadOnlyList<SituacionDeudaDto>> SituacionesAsync(CancellationToken ct = default)
    {
        var lista = new List<SituacionDeudaDto>();
        foreach (var s in await _deudas.SituacionesAsync(ct).ConfigureAwait(false))
        {
            var doc = await DocumentoAsync(s.TipoDocumento, s.DocumentoId, ct).ConfigureAwait(false);
            lista.Add(Dto(s, doc?.Pendiente ?? 0m));
        }

        return lista;
    }

    // ------------------------------------------------------------------ impagados
    /// <summary>
    /// Tras la devolución de un recibo: si la empresa lleva los impagados a 4315, la deuda que vuelve pasa allí (salvo que
    /// ya estuviera en una cuenta propia, a la que vuelve con el contraasiento del cobro).
    /// </summary>
    public async Task AlDevolverAsync(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, decimal importe, DateOnly fecha, Guid origenId, Guid cobroId,
        CancellationToken ct = default)
    {
        var config = await _deudas.ConfiguracionAsync(ct).ConfigureAwait(false);
        // Si el cobro devuelto había sacado la deuda de su cuenta (4315, 436), su contraasiento ya la ha devuelto allí.
        if (config is not { ImpagadosA4315: true } || tipo == TipoDocumentoTesoreria.Gasto || await _deudas.RegularizoAsync(cobroId, ct).ConfigureAwait(false))
        {
            return;
        }

        var doc = await DocumentoAsync(tipo, documentoId, ct).ConfigureAwait(false);
        if (doc is null)
        {
            return;
        }

        var s = await SituacionAsync(empresaId, tipo, documentoId, doc, ct).ConfigureAwait(false);
        if (s.Cuenta is not null && s.Cuenta != CuentasDeuda.Impagados)
        {
            return;
        }

        s.Mover(CuentasDeuda.Impagados, importe);
        _contabilizacion.EncolarTraspaso(empresaId, ContabilizacionTesoreria.Derivado(origenId, "impagado"), $"Impagado {doc.Documento}", fecha, importe,
            CuentasDeuda.Impagados, s.CuentaOrigen, doc.TerceroId, doc.Tercero);
    }

    // ------------------------------------------------------------------ dudosos
    public async Task<Resultado<SituacionDeudaDto>> ClasificarAsync(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, ClasificarDeudaComando c,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var doc = await DocumentoAsync(tipo, documentoId, ct).ConfigureAwait(false);
        if (doc is null || tipo == TipoDocumentoTesoreria.Gasto)
        {
            return Resultado.Fallo<SituacionDeudaDto>(Error.NoEncontrado("deuda.documento", "La factura o el efecto no existe."));
        }

        if (doc.Pendiente <= 0m)
        {
            return Resultado.Fallo<SituacionDeudaDto>(Error.Conflicto("deuda.sin_pendiente", $"{doc.Documento} no tiene nada pendiente de cobro."));
        }

        var fecha = c.Fecha ?? Hoy;
        var s = await SituacionAsync(empresaId, tipo, documentoId, doc, ct).ConfigureAwait(false);
        var r = s.Clasificar(c.Clasificacion, fecha);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SituacionDeudaDto>(r.Error);
        }

        if (s.Cuenta != CuentasDeuda.Dudoso)
        {
            // Todo lo pendiente pasa a dudoso cobro: lo que estaba en impagados y el resto, de la cuenta de origen.
            var previa = s.Cuenta;
            var enPrevia = previa is null ? 0m : Math.Min(s.Importe, doc.Pendiente);
            var resto = Redondeo.Dos(doc.Pendiente - enPrevia);
            s.Mover(CuentasDeuda.Dudoso, doc.Pendiente);
            var marca = _reloj.AhoraUtc.UtcTicks;
            _contabilizacion.EncolarTraspaso(empresaId, ContabilizacionTesoreria.Derivado(s.Id, $"dudoso-previa:{marca}"), $"Dudoso cobro {doc.Documento}", fecha,
                enPrevia, CuentasDeuda.Dudoso, previa, doc.TerceroId, doc.Tercero);
            _contabilizacion.EncolarTraspaso(empresaId, ContabilizacionTesoreria.Derivado(s.Id, $"dudoso:{marca}"), $"Dudoso cobro {doc.Documento}", fecha,
                resto, CuentasDeuda.Dudoso, s.CuentaOrigen, doc.TerceroId, doc.Tercero);
        }

        if (c.PorcentajeDotacion is { } pct)
        {
            var dotado = AjustarDotacion(s, Redondeo.Dos(s.Importe * pct / 100m), fecha, doc);
            if (dotado.EsFallo)
            {
                return Resultado.Fallo<SituacionDeudaDto>(dotado.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s, doc.Pendiente));
    }

    public async Task<Resultado<SituacionDeudaDto>> DotarAsync(Guid id, DotarDeudaComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var s = await _deudas.SituacionAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return Resultado.Fallo<SituacionDeudaDto>(NoEncontrada());
        }

        var doc = await DocumentoAsync(s.TipoDocumento, s.DocumentoId, ct).ConfigureAwait(false);
        var objetivo = c.Importe ?? Redondeo.Dos(s.Importe * (c.Porcentaje ?? 100m) / 100m);
        var r = AjustarDotacion(s, objetivo, c.Fecha ?? Hoy, doc!);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SituacionDeudaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s, doc!.Pendiente));
    }

    /// <summary>Deja de ser dudoso: la deuda vuelve a su cuenta de origen y se revierte el deterioro.</summary>
    public async Task<Resultado<SituacionDeudaDto>> DesclasificarAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _deudas.SituacionAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return Resultado.Fallo<SituacionDeudaDto>(NoEncontrada());
        }

        if (s.Cuenta != CuentasDeuda.Dudoso)
        {
            return Resultado.Fallo<SituacionDeudaDto>(Error.Conflicto("deuda.no_dudosa", "La deuda no está clasificada como dudosa."));
        }

        var doc = await DocumentoAsync(s.TipoDocumento, s.DocumentoId, ct).ConfigureAwait(false);
        var marca = _reloj.AhoraUtc.UtcTicks;
        var (importe, dotacion) = s.Desclasificar();
        _contabilizacion.EncolarTraspaso(s.EmpresaId, ContabilizacionTesoreria.Derivado(s.Id, $"desclasificar:{marca}"), $"Sale de dudoso {s.Documento}", Hoy, importe,
            s.CuentaOrigen, CuentasDeuda.Dudoso, s.TerceroId, s.TerceroNombre);
        _contabilizacion.EncolarTraspaso(s.EmpresaId, ContabilizacionTesoreria.Derivado(s.Id, $"revertir:{marca}"), $"Reversión deterioro {s.Documento}", Hoy, dotacion,
            CuentasDeuda.Deterioro, CuentasDeuda.Reversion, null, null);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s, doc?.Pendiente ?? 0m));
    }

    /// <summary>
    /// Declara incobrable lo pendiente: se da por cobrado contra pérdidas (650). Al contabilizarlo, la deuda sale antes de
    /// su cuenta (436, 4315) y el deterioro dotado se aplica.
    /// </summary>
    public async Task<Resultado<SituacionDeudaDto>> IncobrableAsync(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, IncobrableComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var doc = await DocumentoAsync(tipo, documentoId, ct).ConfigureAwait(false);
        if (doc is null || tipo == TipoDocumentoTesoreria.Gasto)
        {
            return Resultado.Fallo<SituacionDeudaDto>(Error.NoEncontrado("deuda.documento", "La factura o el efecto no existe."));
        }

        if (doc.Pendiente <= 0m)
        {
            return Resultado.Fallo<SituacionDeudaDto>(Error.Conflicto("deuda.sin_pendiente", $"{doc.Documento} no tiene nada pendiente de cobro."));
        }

        var fecha = c.Fecha ?? Hoy;
        var s = await SituacionAsync(empresaId, tipo, documentoId, doc, ct).ConfigureAwait(false);
        var m = Movimiento.Crear(empresaId, tipo, documentoId, SentidoMovimiento.Cobro, doc.Pendiente, fecha, CuentasDeuda.MetodoIncobrable, _reloj,
            cuentaPuente: CuentasDeuda.Incobrables);
        if (m.EsFallo)
        {
            return Resultado.Fallo<SituacionDeudaDto>(m.Error);
        }

        _movimientos.Agregar(m.Valor);
        await _contabilizacion.EncolarMovimientoAsync(m.Valor, false, ct: ct).ConfigureAwait(false);
        s.MarcarIncobrable(fecha);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(s, 0m));
    }

    // ------------------------------------------------------------------ renovación
    public async Task<IReadOnlyList<RenovacionDto>> RenovacionesAsync(CancellationToken ct = default)
    {
        var lista = new List<RenovacionDto>();
        foreach (var r in await _deudas.RenovacionesAsync(ct).ConfigureAwait(false))
        {
            lista.Add(await RenovacionDtoAsync(r, ct).ConfigureAwait(false));
        }

        return lista;
    }

    public async Task<Resultado<RenovacionDto>> RenovarAsync(Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, RenovarComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var doc = await DocumentoAsync(tipo, documentoId, ct).ConfigureAwait(false);
        if (doc is null || tipo == TipoDocumentoTesoreria.Gasto)
        {
            return Resultado.Fallo<RenovacionDto>(Error.NoEncontrado("deuda.documento", "La factura o el efecto no existe."));
        }

        var vencimientos = (c.Vencimientos ?? []).ToList();
        if (vencimientos.Count == 0 || vencimientos.Any(v => v.Importe <= 0m || Redondeo.Dos(v.Importe) != v.Importe))
        {
            return Resultado.Fallo<RenovacionDto>(Error.Validacion("renovacion.vencimientos", "Indica los vencimientos nuevos, cada uno con su importe positivo."));
        }

        var fecha = c.Fecha ?? Hoy;
        if (vencimientos.Any(v => v.Fecha <= fecha))
        {
            return Resultado.Fallo<RenovacionDto>(Error.Validacion("renovacion.fecha", "Los vencimientos nuevos tienen que ser posteriores a la renovación."));
        }

        var config = await _deudas.ConfiguracionAsync(ct).ConfigureAwait(false);
        var gastos = Redondeo.Dos(c.Gastos ?? doc.Pendiente * (config?.PorcentajeRenovacion ?? 0m) / 100m);
        var total = Redondeo.Dos(vencimientos.Sum(v => v.Importe));
        if (total != Redondeo.Dos(doc.Pendiente + gastos))
        {
            return Resultado.Fallo<RenovacionDto>(Error.Validacion("renovacion.cuadre",
                $"Los vencimientos suman {Redondeo.Formatear(total)} € y tienen que sumar lo pendiente más los gastos ({Redondeo.Formatear(doc.Pendiente + gastos)} €)."));
        }

        var renovacion = RenovacionEfecto.Crear(empresaId, tipo, documentoId, doc.Documento, doc.TerceroId, doc.Tercero, fecha, doc.Pendiente, gastos, _reloj);
        if (renovacion.EsFallo)
        {
            return Resultado.Fallo<RenovacionDto>(renovacion.Error);
        }

        var ren = renovacion.Valor;
        var i = 0;
        foreach (var v in vencimientos.OrderBy(v => v.Fecha))
        {
            i++;
            var efecto = EfectoCartera.Crear(empresaId, SentidoCartera.Cobro, doc.TerceroId, doc.Tercero, $"{doc.Documento} R{i}", fecha, v.Fecha, v.Importe,
                "Renovacion", $"{ren.Id:N}-{i}", _reloj, CuentasDeuda.EnCartera);
            if (efecto.EsFallo)
            {
                return Resultado.Fallo<RenovacionDto>(efecto.Error);
            }

            _cartera.Agregar(efecto.Valor);
        }

        // Lo pendiente se cancela contra efectos en cartera (y, si estaba en 4315 o 436, sale antes de allí).
        var m = Movimiento.Crear(empresaId, tipo, documentoId, SentidoMovimiento.Cobro, doc.Pendiente, fecha, CuentasDeuda.MetodoRenovacion, _reloj,
            cuentaPuente: CuentasDeuda.EnCartera);
        if (m.EsFallo)
        {
            return Resultado.Fallo<RenovacionDto>(m.Error);
        }

        _movimientos.Agregar(m.Valor);
        ren.AsignarMovimiento(m.Valor.Id);
        _deudas.Agregar(ren);
        await _contabilizacion.EncolarMovimientoAsync(m.Valor, false, ct: ct).ConfigureAwait(false);
        _contabilizacion.EncolarTraspaso(empresaId, ContabilizacionTesoreria.Derivado(ren.Id, "gastos"), $"Gastos renovación {doc.Documento}", fecha, gastos,
            CuentasDeuda.EnCartera, CuentasDeuda.IngresosRenovacion, null, null);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(await RenovacionDtoAsync(ren, ct).ConfigureAwait(false));
    }

    /// <summary>Deshace una renovación cuyos efectos nuevos no tienen cobros: se anulan, y el documento vuelve a estar pendiente.</summary>
    public async Task<Resultado<RenovacionDto>> AnularRenovacionAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var ren = await _deudas.RenovacionAsync(id, ct).ConfigureAwait(false);
        if (ren is null)
        {
            return Resultado.Fallo<RenovacionDto>(Error.NoEncontrado("renovacion.no_encontrada", "La renovación no existe."));
        }

        if (ren.AnuladaEn is not null)
        {
            return Resultado.Fallo<RenovacionDto>(Error.Conflicto("renovacion.anulada", "La renovación ya está anulada."));
        }

        var efectos = await EfectosDeAsync(ren, ct).ConfigureAwait(false);
        foreach (var e in efectos)
        {
            if (await _movimientos.SumaAsync(TipoDocumentoTesoreria.Cartera, e.Id, ct).ConfigureAwait(false) != 0m)
            {
                return Resultado.Fallo<RenovacionDto>(Error.Conflicto("renovacion.con_cobros", $"El efecto {e.Documento} ya tiene cobros: anúlalos antes."));
            }
        }

        var anulados = await _cartera.AnuladosAsync(efectos.Select(e => e.Id).ToList(), ct).ConfigureAwait(false);
        foreach (var e in efectos.Where(e => !anulados.Contains(e.Id)))
        {
            _cartera.Agregar(AnulacionEfecto.Crear(e, $"Anulación de la renovación de {ren.Documento}", _reloj).Valor);
        }

        var original = await _movimientos.ObtenerAsync(ren.MovimientoId, ct).ConfigureAwait(false);
        var anulacion = Movimiento.CrearAnulacion(original!, Hoy, _reloj);
        if (anulacion.EsFallo)
        {
            return Resultado.Fallo<RenovacionDto>(anulacion.Error);
        }

        _movimientos.Agregar(anulacion.Valor);
        await _contabilizacion.EncolarMovimientoAsync(anulacion.Valor, false, original, "Anulación renovación", ct).ConfigureAwait(false);
        _contabilizacion.EncolarTraspaso(empresaId, ContabilizacionTesoreria.Derivado(ren.Id, "gastos-anulacion"), $"Anulación gastos renovación {ren.Documento}", Hoy,
            ren.Gastos, CuentasDeuda.IngresosRenovacion, CuentasDeuda.EnCartera, null, null);
        ren.Anular(_reloj);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(await RenovacionDtoAsync(ren, ct).ConfigureAwait(false));
    }

    // ------------------------------------------------------------------ apoyo
    private sealed record DocumentoDeuda(string Documento, Guid? TerceroId, string Tercero, string? Cuenta, decimal Pendiente);

    private async Task<DocumentoDeuda?> DocumentoAsync(TipoDocumentoTesoreria tipo, Guid id, CancellationToken ct)
    {
        var cobrado = await _movimientos.SumaAsync(tipo, id, ct).ConfigureAwait(false);
        if (tipo == TipoDocumentoTesoreria.Factura)
        {
            var f = await _facturas.ObtenerAsync(id, ct).ConfigureAwait(false);
            return f is null ? null : new DocumentoDeuda(f.NumeroCompleto, f.ClienteId, f.ClienteNombre, null, Redondeo.Dos(f.Total - cobrado));
        }

        if (tipo == TipoDocumentoTesoreria.Cartera)
        {
            var e = await _cartera.ObtenerAsync(id, ct).ConfigureAwait(false);
            if (e is null || e.Sentido != SentidoCartera.Cobro)
            {
                return null;
            }

            var anulado = (await _cartera.AnuladosAsync([e.Id], ct).ConfigureAwait(false)).Contains(e.Id);
            return new DocumentoDeuda(e.Documento, e.TerceroId, e.TerceroNombre, e.CuentaContable, anulado ? 0m : Redondeo.Dos(e.Importe - cobrado));
        }

        return null;
    }

    private async Task<SituacionDeuda> SituacionAsync(Guid empresaId, TipoDocumentoTesoreria tipo, Guid id, DocumentoDeuda doc, CancellationToken ct)
    {
        var s = await _deudas.SituacionAsync(tipo, id, ct).ConfigureAwait(false);
        if (s is null)
        {
            s = SituacionDeuda.Crear(empresaId, tipo, id, doc.Documento, doc.TerceroId, doc.Tercero, doc.Cuenta);
            _deudas.Agregar(s);
        }

        return s;
    }

    private Resultado AjustarDotacion(SituacionDeuda s, decimal objetivo, DateOnly fecha, DocumentoDeuda doc)
    {
        var ajuste = s.Dotar(objetivo);
        if (ajuste.EsFallo)
        {
            return Resultado.Fallo(ajuste.Error);
        }

        var marca = _reloj.AhoraUtc.UtcTicks;
        if (ajuste.Valor > 0m)
        {
            _contabilizacion.EncolarTraspaso(s.EmpresaId, ContabilizacionTesoreria.Derivado(s.Id, $"dotacion:{marca}"), $"Deterioro {doc.Documento}", fecha, ajuste.Valor,
                CuentasDeuda.Dotacion, CuentasDeuda.Deterioro, null, null);
        }
        else if (ajuste.Valor < 0m)
        {
            _contabilizacion.EncolarTraspaso(s.EmpresaId, ContabilizacionTesoreria.Derivado(s.Id, $"reversion:{marca}"), $"Reversión deterioro {doc.Documento}", fecha, -ajuste.Valor,
                CuentasDeuda.Deterioro, CuentasDeuda.Reversion, null, null);
        }

        return Resultado.Ok();
    }

    private async Task<List<EfectoCartera>> EfectosDeAsync(RenovacionEfecto ren, CancellationToken ct)
    {
        var efectos = new List<EfectoCartera>();
        for (var i = 1; ; i++)
        {
            var e = await _cartera.PorOrigenAsync(ren.EmpresaId, "Renovacion", $"{ren.Id:N}-{i}", ct).ConfigureAwait(false);
            if (e is null)
            {
                return efectos;
            }

            efectos.Add(e);
        }
    }

    private async Task<RenovacionDto> RenovacionDtoAsync(RenovacionEfecto r, CancellationToken ct)
    {
        var efectos = new List<EfectoRenovacionDto>();
        foreach (var e in await EfectosDeAsync(r, ct).ConfigureAwait(false))
        {
            var cobrado = await _movimientos.SumaAsync(TipoDocumentoTesoreria.Cartera, e.Id, ct).ConfigureAwait(false);
            efectos.Add(new EfectoRenovacionDto(e.Id, e.Documento, e.Vencimiento, e.Importe, r.AnuladaEn is null ? Redondeo.Dos(e.Importe - cobrado) : 0m));
        }

        return new RenovacionDto(r.Id, r.TipoDocumento.ToString(), r.DocumentoId, r.Documento, r.TerceroNombre, r.Fecha, r.Importe, r.Gastos, r.AnuladaEn is not null, efectos);
    }

    private static SituacionDeudaDto Dto(SituacionDeuda s, decimal pendiente) => new(s.Id, s.TipoDocumento.ToString(), s.DocumentoId, s.Documento, s.TerceroId,
        s.TerceroNombre, s.Cuenta, s.Importe, s.Clasificacion.ToString(), s.FechaClasificacion, s.Dotado, s.IncobrableEl, pendiente);

    private static Error NoEncontrada() => Error.NoEncontrado("deuda.no_encontrada", "No hay ninguna situación de deuda con ese identificador.");
}
