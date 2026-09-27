using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Vista de un movimiento de tesorería.</summary>
public sealed record MovimientoDto(Guid Id, string Sentido, decimal Importe, DateOnly Fecha, string? Metodo, Guid? AnulaMovimientoId = null, bool Anulado = false,
    Guid? CuentaBancariaId = null)
{
    public static MovimientoDto Desde(Movimiento m) => new(m.Id, m.Sentido.ToString(), m.Importe, m.Fecha, m.Metodo, m.AnulaMovimientoId, CuentaBancariaId: m.CuentaBancariaId);

    /// <summary>Lista de movimientos de un documento, marcando los que ya tienen su anulación.</summary>
    public static IReadOnlyList<MovimientoDto> DesdeLista(IReadOnlyList<Movimiento> movimientos)
    {
        ArgumentNullException.ThrowIfNull(movimientos);
        var anulados = movimientos.Where(m => m.AnulaMovimientoId is not null).Select(m => m.AnulaMovimientoId!.Value).ToHashSet();
        return movimientos.Select(m => Desde(m) with { Anulado = anulados.Contains(m.Id) }).ToList();
    }
}

/// <summary>Saldo de un documento (total, liquidado, pendiente y estado derivado).</summary>
public sealed record SaldoDto(
    string TipoDocumento, Guid DocumentoId, decimal Total, decimal Liquidado, decimal Pendiente, string Estado,
    IReadOnlyList<MovimientoDto> Movimientos);

/// <summary>Repositorio y consultas de movimientos de tesorería.</summary>
public interface IRepositorioMovimientos
{
    void Agregar(Movimiento movimiento);

    Task<decimal> SumaAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default);

    Task<IReadOnlyList<Movimiento>> ListarAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default);

    Task<Movimiento?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<bool> EstaAnuladoAsync(Guid id, CancellationToken ct = default);

    /// <summary>Anticipo cuya aplicación generó el movimiento (null si no viene de un anticipo).</summary>
    Task<Anticipo?> AnticipoDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Tesorería.</summary>
public interface IUnidadDeTrabajoTesoreria : IUnidadDeTrabajo;

/// <summary>Consultas agregadas de tesorería (las usa Informes).</summary>
public interface IConsultaTesoreria
{
    /// <summary>Total liquidado (cobrado o pagado) de todos los documentos de un tipo en la empresa activa.</summary>
    Task<decimal> TotalLiquidadoAsync(TipoDocumentoTesoreria tipo, CancellationToken ct = default);

    /// <summary>Movimientos (cobros y pagos) de la empresa en un rango de fechas (para el cierre de caja).</summary>
    Task<IReadOnlyList<MovimientoDto>> ListarPorPeriodoAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    /// <summary>
    /// Total liquidado por cada documento de un conjunto (un solo GROUP BY). Devuelve un diccionario
    /// documento→importe liquidado; los documentos sin movimientos no aparecen. Lo usan los informes
    /// de cartera (aging) y los extractos de tercero para calcular el pendiente sin N consultas.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default);
}

/// <summary>
/// Datos para registrar un cobro contra una factura. <paramref name="CuentaBancariaId"/> es el banco o la caja por la
/// que entra el dinero (sin indicarla: la caja en efectivo, el banco predeterminado en lo demás).
/// </summary>
public sealed record RegistrarCobroComando(Guid FacturaId, decimal Importe, DateOnly? Fecha = null, string? Metodo = null, Guid? CuentaBancariaId = null);

/// <summary>Datos para registrar un pago contra un gasto (con la cuenta de tesorería por la que sale, opcional).</summary>
public sealed record RegistrarPagoComando(Guid GastoId, decimal Importe, DateOnly? Fecha = null, string? Metodo = null, Guid? CuentaBancariaId = null);

/// <summary>Caso de uso: registrar un cobro contra una factura (total o parcial, sin sobrepago).</summary>
public sealed class RegistrarCobro
{
    private readonly IConsultaFacturas _facturas;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;
    private readonly ResolutorCuentaTesoreria? _resolutor;

    public RegistrarCobro(IConsultaFacturas facturas, IRepositorioMovimientos movimientos, IUnidadDeTrabajoTesoreria unidadDeTrabajo, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null, ResolutorCuentaTesoreria? resolutor = null)
    {
        _facturas = facturas;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
        _resolutor = resolutor;
    }

    public async Task<Resultado<SaldoDto>> EjecutarAsync(Guid empresaId, RegistrarCobroComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var factura = await _facturas.ObtenerAsync(comando.FacturaId, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        if (factura.Estado == "Anulada")
        {
            return Resultado.Fallo<SaldoDto>(Error.Conflicto("factura.anulada", $"La factura {factura.NumeroCompleto} está anulada: no admite cobros."));
        }

        var cuenta = await ResolverCuentaAsync(_resolutor, comando.CuentaBancariaId, comando.Metodo, ct).ConfigureAwait(false);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<SaldoDto>(cuenta.Error);
        }

        return await RegistrarAsync(empresaId, TipoDocumentoTesoreria.Factura, comando.FacturaId, SentidoMovimiento.Cobro,
            comando.Importe, factura.Total, comando.Fecha, comando.Metodo, _movimientos, _unidadDeTrabajo, _reloj, ct,
            contabilizacion: _contabilizacion, cuentaBancariaId: cuenta.Valor).ConfigureAwait(false);
    }

    /// <summary>Cuenta de tesorería del movimiento (null si no hay cuentas o no hay resolutor: asiento a 570/572).</summary>
    internal static async Task<Resultado<Guid?>> ResolverCuentaAsync(ResolutorCuentaTesoreria? resolutor, Guid? cuentaBancariaId, string? metodo, CancellationToken ct)
    {
        if (resolutor is null)
        {
            return Resultado.Ok(cuentaBancariaId);
        }

        var r = await resolutor.ResolverAsync(cuentaBancariaId, metodo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<Guid?>(r.Error) : Resultado.Ok(r.Valor?.Id);
    }

    internal static async Task<Resultado<SaldoDto>> RegistrarAsync(
        Guid empresaId, TipoDocumentoTesoreria tipo, Guid documentoId, SentidoMovimiento sentido, decimal importe, decimal totalDocumento,
        DateOnly? fecha, string? metodo, IRepositorioMovimientos movimientos, IUnidadDeTrabajo unidadDeTrabajo, IReloj reloj, CancellationToken ct,
        Action<Movimiento>? antesDeGuardar = null, ContabilizacionTesoreria? contabilizacion = null, bool aplicacionAnticipo = false,
        Guid? cuentaBancariaId = null)
    {
        var importeRedondeado = Redondeo.Dos(importe);
        if (importeRedondeado <= 0)
        {
            return Resultado.Fallo<SaldoDto>(Error.Validacion("movimiento.importe_invalido", "El importe debe ser mayor que cero."));
        }

        var liquidado = await movimientos.SumaAsync(tipo, documentoId, ct).ConfigureAwait(false);
        if (liquidado + importeRedondeado > totalDocumento)
        {
            return Resultado.Fallo<SaldoDto>(Error.Conflicto("movimiento.sobrepago", "El importe supera el pendiente del documento."));
        }

        var fechaMovimiento = fecha ?? DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
        var movimiento = Movimiento.Crear(empresaId, tipo, documentoId, sentido, importeRedondeado, fechaMovimiento, metodo, reloj, cuentaBancariaId);
        if (movimiento.EsFallo)
        {
            return Resultado.Fallo<SaldoDto>(movimiento.Error);
        }

        movimientos.Agregar(movimiento.Valor);
        antesDeGuardar?.Invoke(movimiento.Valor);   // p. ej. anotar la aplicación de un anticipo en la misma transacción
        if (contabilizacion is not null)
        {
            // Su asiento, en la bandeja de salida de la misma transacción.
            await contabilizacion.EncolarMovimientoAsync(movimiento.Valor, aplicacionAnticipo, ct: ct).ConfigureAwait(false);
        }

        await unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (contabilizacion is not null)
        {
            await contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        var nuevoLiquidado = Redondeo.Dos(liquidado + importeRedondeado);
        var pendiente = Redondeo.Dos(totalDocumento - nuevoLiquidado);
        var estado = Movimiento.DerivarEstado(totalDocumento, nuevoLiquidado);
        return Resultado.Ok(new SaldoDto(tipo.ToString(), documentoId, totalDocumento, nuevoLiquidado, pendiente, estado.ToString(), []));
    }
}

/// <summary>Caso de uso: registrar un pago contra un gasto (total o parcial, sin sobrepago).</summary>
public sealed class RegistrarPago
{
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoTesoreria _unidadDeTrabajo;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;
    private readonly ResolutorCuentaTesoreria? _resolutor;

    public RegistrarPago(IConsultaGastos gastos, IRepositorioMovimientos movimientos, IUnidadDeTrabajoTesoreria unidadDeTrabajo, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null, ResolutorCuentaTesoreria? resolutor = null)
    {
        _gastos = gastos;
        _movimientos = movimientos;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
        _resolutor = resolutor;
    }

    public async Task<Resultado<SaldoDto>> EjecutarAsync(Guid empresaId, RegistrarPagoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var gasto = await _gastos.ObtenerAsync(comando.GastoId, ct).ConfigureAwait(false);
        if (gasto is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        if (gasto.Estado == "Anulado")
        {
            return Resultado.Fallo<SaldoDto>(Error.Conflicto("gasto.anulado", "El gasto está anulado: no admite pagos."));
        }

        var cuenta = await RegistrarCobro.ResolverCuentaAsync(_resolutor, comando.CuentaBancariaId, comando.Metodo, ct).ConfigureAwait(false);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<SaldoDto>(cuenta.Error);
        }

        return await RegistrarCobro.RegistrarAsync(empresaId, TipoDocumentoTesoreria.Gasto, comando.GastoId, SentidoMovimiento.Pago,
            comando.Importe, gasto.Total, comando.Fecha, comando.Metodo, _movimientos, _unidadDeTrabajo, _reloj, ct, contabilizacion: _contabilizacion,
            cuentaBancariaId: cuenta.Valor).ConfigureAwait(false);
    }
}

/// <summary>Caso de uso: consultar el saldo (y los movimientos) de un documento.</summary>
public sealed class ConsultarSaldo
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioMovimientos _movimientos;

    public ConsultarSaldo(IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioMovimientos movimientos)
    {
        _facturas = facturas;
        _gastos = gastos;
        _movimientos = movimientos;
    }

    public async Task<Resultado<SaldoDto>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _facturas.ObtenerAsync(facturaId, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        return await ConstruirAsync(TipoDocumentoTesoreria.Factura, facturaId, factura.Total, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<SaldoDto>> DeGastoAsync(Guid gastoId, CancellationToken ct = default)
    {
        var gasto = await _gastos.ObtenerAsync(gastoId, ct).ConfigureAwait(false);
        if (gasto is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."));
        }

        return await ConstruirAsync(TipoDocumentoTesoreria.Gasto, gastoId, gasto.Total, ct).ConfigureAwait(false);
    }

    private async Task<Resultado<SaldoDto>> ConstruirAsync(TipoDocumentoTesoreria tipo, Guid documentoId, decimal total, CancellationToken ct)
    {
        var movimientos = await _movimientos.ListarAsync(tipo, documentoId, ct).ConfigureAwait(false);
        var liquidado = Redondeo.Dos(movimientos.Sum(m => m.Importe));
        var pendiente = Redondeo.Dos(total - liquidado);
        var estado = Movimiento.DerivarEstado(total, liquidado);
        return Resultado.Ok(new SaldoDto(
            tipo.ToString(), documentoId, total, liquidado, pendiente, estado.ToString(),
            MovimientoDto.DesdeLista(movimientos)));
    }
}

/// <summary>Saldo resumido de un documento (sin sus movimientos), para listados.</summary>
public sealed record SaldoDocumentoDto(Guid DocumentoId, decimal Total, decimal Liquidado, decimal Pendiente, string Estado, string EstadoDocumento);

/// <summary>
/// Saldos de todos los documentos de un tipo (facturas o gastos) de la empresa en una sola consulta
/// agrupada: evita que los listados de cobros, pagos y previsión pidan el saldo documento a documento.
/// </summary>
public sealed class ConsultarSaldos
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaTesoreria _tesoreria;

    public ConsultarSaldos(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaTesoreria tesoreria)
    {
        _facturas = facturas;
        _gastos = gastos;
        _tesoreria = tesoreria;
    }

    /// <param name="ids">Si se indica, solo esos documentos; si no, todos los del tipo.</param>
    public async Task<IReadOnlyList<SaldoDocumentoDto>> EjecutarAsync(Guid empresaId, TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid>? ids, CancellationToken ct = default)
    {
        List<(Guid Id, decimal Total, string Estado)> documentos = tipo switch
        {
            TipoDocumentoTesoreria.Factura => (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(f => (f.Id, f.Total, f.Estado)).ToList(),
            TipoDocumentoTesoreria.Gasto => (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(g => (g.Id, g.Total, g.Estado)).ToList(),
            _ => [],
        };

        if (ids is { Count: > 0 })
        {
            var filtro = ids.ToHashSet();
            documentos = documentos.Where(d => filtro.Contains(d.Id)).ToList();
        }

        var liquidados = await _tesoreria.LiquidadoPorDocumentosAsync(tipo, documentos.Select(d => d.Id).ToList(), ct).ConfigureAwait(false);
        return documentos.Select(d =>
        {
            var liquidado = Redondeo.Dos(liquidados.GetValueOrDefault(d.Id));
            return new SaldoDocumentoDto(d.Id, d.Total, liquidado, Redondeo.Dos(d.Total - liquidado),
                Movimiento.DerivarEstado(d.Total, liquidado).ToString(), d.Estado);
        }).ToList();
    }
}

/// <summary>
/// Anula un cobro o un pago mal registrado: añade su anulación (importe en negativo), con lo que el documento vuelve a
/// tener pendiente ese importe. Los movimientos no se borran ni se modifican. Si el cobro era la aplicación de un
/// anticipo, el anticipo recupera ese saldo disponible.
/// </summary>
public sealed class AnularMovimiento
{
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;

    public AnularMovimiento(IRepositorioMovimientos movimientos, IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria? contabilizacion = null)
    {
        _movimientos = movimientos;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
    }

    public async Task<Resultado<MovimientoDto>> EjecutarAsync(Guid empresaId, Guid movimientoId, DateOnly? fecha, CancellationToken ct = default)
    {
        var original = await _movimientos.ObtenerAsync(movimientoId, ct).ConfigureAwait(false);
        if (original is null || original.EmpresaId != empresaId)
        {
            return Resultado.Fallo<MovimientoDto>(Error.NoEncontrado("movimiento.no_encontrado", "El cobro o pago no existe."));
        }

        if (await _movimientos.EstaAnuladoAsync(movimientoId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<MovimientoDto>(Error.Conflicto("movimiento.ya_anulado", "Este cobro o pago ya está anulado."));
        }

        var anulacion = Movimiento.CrearAnulacion(original, fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime), _reloj);
        if (anulacion.EsFallo)
        {
            return Resultado.Fallo<MovimientoDto>(anulacion.Error);
        }

        _movimientos.Agregar(anulacion.Valor);
        var anticipo = await _movimientos.AnticipoDeMovimientoAsync(movimientoId, ct).ConfigureAwait(false);
        if (anticipo is not null)
        {
            var aplicacion = anticipo.Aplicaciones.Single(a => a.MovimientoId == movimientoId);
            anticipo.AnotarAplicacion(aplicacion.FacturaId, -aplicacion.Importe, anulacion.Valor.Fecha, anulacion.Valor.Id);
        }

        if (_contabilizacion is not null)
        {
            await _contabilizacion.EncolarMovimientoAsync(anulacion.Valor, anticipo is not null, original, ct: ct).ConfigureAwait(false);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }
        return Resultado.Ok(MovimientoDto.Desde(anulacion.Valor));
    }
}

// ---------------------------------------------------------------------------- Previsiones (ingresos/gastos previstos)

/// <summary>Vista de una previsión de tesorería.</summary>
public sealed record PrevisionDto(Guid Id, string Sentido, string Concepto, decimal Importe, DateOnly Fecha)
{
    public static PrevisionDto Desde(PrevisionTesoreria p) => new(p.Id, p.Sentido.ToString(), p.Concepto, p.Importe, p.Fecha);
}

/// <summary>Datos para crear una previsión.</summary>
public sealed record CrearPrevisionComando(SentidoPrevision Sentido, string Concepto, decimal Importe, DateOnly Fecha);

/// <summary>Repositorio de previsiones de tesorería.</summary>
public interface IRepositorioPrevisiones
{
    void Agregar(PrevisionTesoreria prevision);
    Task<PrevisionTesoreria?> ObtenerAsync(Guid id, CancellationToken ct = default);
    void Eliminar(PrevisionTesoreria prevision);
    Task<IReadOnlyList<PrevisionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Caso de uso: añadir una previsión (ingreso o gasto previsto).</summary>
public sealed class CrearPrevision
{
    private readonly IRepositorioPrevisiones _repo;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    public CrearPrevision(IRepositorioPrevisiones repo, IUnidadDeTrabajoTesoreria unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<PrevisionDto>> EjecutarAsync(Guid empresaId, CrearPrevisionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var prevision = PrevisionTesoreria.Crear(empresaId, comando.Sentido, comando.Concepto, comando.Importe, comando.Fecha, _reloj);
        if (prevision.EsFallo)
        {
            return Resultado.Fallo<PrevisionDto>(prevision.Error);
        }

        _repo.Agregar(prevision.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PrevisionDto.Desde(prevision.Valor));
    }
}

/// <summary>Caso de uso: listar las previsiones de la empresa activa (más próximas primero).</summary>
public sealed class ListarPrevisiones
{
    private readonly IRepositorioPrevisiones _repo;
    public ListarPrevisiones(IRepositorioPrevisiones repo) => _repo = repo;
    public Task<IReadOnlyList<PrevisionDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);
}

/// <summary>Caso de uso: eliminar una previsión.</summary>
public sealed class ActualizarPrevision
{
    private readonly IRepositorioPrevisiones _repo;
    private readonly IUnidadDeTrabajoTesoreria _unidad;

    public ActualizarPrevision(IRepositorioPrevisiones repo, IUnidadDeTrabajoTesoreria unidad) { _repo = repo; _unidad = unidad; }

    public async Task<Resultado<PrevisionDto>> EjecutarAsync(Guid id, CrearPrevisionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var prevision = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (prevision is null)
        {
            return Resultado.Fallo<PrevisionDto>(Error.NoEncontrado("prevision.no_encontrada", "No se encontró la previsión."));
        }

        var r = prevision.Actualizar(comando.Sentido, comando.Concepto, comando.Importe, comando.Fecha);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PrevisionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PrevisionDto.Desde(prevision));
    }
}

public sealed class EliminarPrevision
{
    private readonly IRepositorioPrevisiones _repo;
    private readonly IUnidadDeTrabajoTesoreria _unidad;

    public EliminarPrevision(IRepositorioPrevisiones repo, IUnidadDeTrabajoTesoreria unidad) { _repo = repo; _unidad = unidad; }

    public async Task<Resultado> EjecutarAsync(Guid id, CancellationToken ct = default)
    {
        var prevision = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (prevision is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("prevision.no_encontrada", "No se encontró la previsión."));
        }

        _repo.Eliminar(prevision);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
