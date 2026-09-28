using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

public sealed record EfectoCarteraDto(
    Guid Id, string Sentido, Guid? TerceroId, string TerceroNombre, string Documento, DateOnly? FechaDocumento, DateOnly Vencimiento,
    decimal Importe, decimal Liquidado, decimal Pendiente, string Estado, string? Origen, string? OrigenReferencia, bool Anulado = false);

public sealed record CrearEfectoCarteraComando(
    SentidoCartera Sentido, string TerceroNombre, string Documento, DateOnly Vencimiento, decimal Importe, Guid? TerceroId = null,
    DateOnly? FechaDocumento = null, string? Origen = null, string? OrigenReferencia = null);

public sealed record MovimientoCarteraComando(decimal Importe, DateOnly? Fecha = null, string? Metodo = null, Guid? CuentaBancariaId = null);

public interface IRepositorioCartera
{
    void Agregar(EfectoCartera efecto);

    Task<EfectoCartera?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<EfectoCartera>> ListarAsync(Guid empresaId, SentidoCartera? sentido, CancellationToken ct = default);

    Task<EfectoCartera?> PorOrigenAsync(Guid empresaId, string origen, string origenReferencia, CancellationToken ct = default);

    void Agregar(AnulacionEfecto anulacion);

    Task<IReadOnlySet<Guid>> AnuladosAsync(IReadOnlyCollection<Guid> efectoIds, CancellationToken ct = default);
}

/// <summary>Cartera de efectos sin documento en ALXOR: alta, consulta con su saldo, y cobro o pago.</summary>
public sealed class GestionCartera
{
    private readonly IRepositorioCartera _cartera;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IConsultaTesoreria _consulta;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    private readonly ContabilizacionTesoreria? _contabilizacion;
    private readonly ResolutorCuentaTesoreria? _resolutor;

    public GestionCartera(IRepositorioCartera cartera, IRepositorioMovimientos movimientos, IConsultaTesoreria consulta, IUnidadDeTrabajoTesoreria unidad, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null, ResolutorCuentaTesoreria? resolutor = null)
    {
        _resolutor = resolutor;
        _contabilizacion = contabilizacion;
        _cartera = cartera;
        _movimientos = movimientos;
        _consulta = consulta;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<EfectoCarteraDto>> ListarAsync(Guid empresaId, SentidoCartera? sentido, bool soloPendientes, CancellationToken ct = default)
    {
        var efectos = await _cartera.ListarAsync(empresaId, sentido, ct).ConfigureAwait(false);
        var ids = efectos.Select(e => e.Id).ToList();
        var liquidado = await _consulta.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Cartera, ids, ct).ConfigureAwait(false);
        var anulados = await _cartera.AnuladosAsync(ids, ct).ConfigureAwait(false);
        return efectos.Select(e => Dto(e, liquidado.GetValueOrDefault(e.Id), anulados.Contains(e.Id)))
            .Where(e => !soloPendientes || (e.Pendiente > 0m && !e.Anulado))
            .OrderBy(e => e.Vencimiento).ThenBy(e => e.TerceroNombre, StringComparer.CurrentCulture).ToList();
    }

    /// <summary>Saldo del efecto con sus movimientos (para verlos y anularlos).</summary>
    public async Task<Resultado<SaldoDto>> SaldoAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _cartera.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("cartera.no_encontrado", "El efecto no existe."));
        }

        var movimientos = await _movimientos.ListarAsync(TipoDocumentoTesoreria.Cartera, id, ct).ConfigureAwait(false);
        var liquidado = Redondeo.Dos(movimientos.Sum(m => m.Importe));
        return Resultado.Ok(new SaldoDto(TipoDocumentoTesoreria.Cartera.ToString(), id, e.Importe, liquidado, Redondeo.Dos(e.Importe - liquidado),
            Movimiento.DerivarEstado(e.Importe, liquidado).ToString(), MovimientoDto.DesdeLista(movimientos)));
    }

    /// <summary>Da de alta un efecto. Con origen y referencia es idempotente: si ya existe, lo devuelve sin duplicarlo.</summary>
    public async Task<Resultado<EfectoCarteraDto>> CrearAsync(Guid empresaId, CrearEfectoCarteraComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Origen is { Length: > 0 } origen && c.OrigenReferencia is { Length: > 0 } referencia
            && await _cartera.PorOrigenAsync(empresaId, origen, referencia, ct).ConfigureAwait(false) is { } existente)
        {
            return Resultado.Ok(Dto(existente, 0m));
        }

        var efecto = EfectoCartera.Crear(empresaId, c.Sentido, c.TerceroId, c.TerceroNombre, c.Documento, c.FechaDocumento, c.Vencimiento, c.Importe,
            c.Origen, c.OrigenReferencia, _reloj);
        if (efecto.EsFallo)
        {
            return Resultado.Fallo<EfectoCarteraDto>(efecto.Error);
        }

        _cartera.Agregar(efecto.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(efecto.Valor, 0m));
    }

    /// <summary>Cobra o paga (total o parcialmente, sin superar el pendiente) un efecto.</summary>
    public async Task<Resultado<SaldoDto>> LiquidarAsync(Guid empresaId, Guid efectoId, MovimientoCarteraComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var efecto = await _cartera.ObtenerAsync(efectoId, ct).ConfigureAwait(false);
        if (efecto is null)
        {
            return Resultado.Fallo<SaldoDto>(Error.NoEncontrado("cartera.no_encontrado", "El efecto no existe."));
        }

        if ((await _cartera.AnuladosAsync([efectoId], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<SaldoDto>(Error.Conflicto("cartera.anulado", "El efecto está anulado: no se cobra ni se paga."));
        }

        var cuenta = await RegistrarCobro.ResolverCuentaAsync(_resolutor, c.CuentaBancariaId, c.Metodo, ct).ConfigureAwait(false);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<SaldoDto>(cuenta.Error);
        }

        return await RegistrarCobro.RegistrarAsync(empresaId, TipoDocumentoTesoreria.Cartera, efecto.Id,
            efecto.Sentido == SentidoCartera.Cobro ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago,
            c.Importe, efecto.Importe, c.Fecha, c.Metodo, _movimientos, _unidad, _reloj, ct, contabilizacion: _contabilizacion,
            cuentaBancariaId: cuenta.Valor).ConfigureAwait(false);
    }

    private static EfectoCarteraDto Dto(EfectoCartera e, decimal liquidado, bool anulado = false) => new(
        e.Id, e.Sentido.ToString(), e.TerceroId, e.TerceroNombre, e.Documento, e.FechaDocumento, e.Vencimiento, e.Importe, liquidado,
        anulado ? 0m : e.Importe - liquidado, anulado ? "Anulado" : Movimiento.DerivarEstado(e.Importe, liquidado).ToString(), e.Origen, e.OrigenReferencia, anulado);

    /// <summary>
    /// Anula un efecto dado de alta por error o saldado fuera de ALXOR. Si tiene cobros o pagos vivos, hay que anularlos
    /// antes (para que la tesorería no cuente un dinero que no se movió).
    /// </summary>
    public async Task<Resultado<EfectoCarteraDto>> AnularAsync(Guid efectoId, string? motivo, CancellationToken ct = default)
    {
        var efecto = await _cartera.ObtenerAsync(efectoId, ct).ConfigureAwait(false);
        if (efecto is null)
        {
            return Resultado.Fallo<EfectoCarteraDto>(Error.NoEncontrado("cartera.no_encontrado", "El efecto no existe."));
        }

        if ((await _cartera.AnuladosAsync([efectoId], ct).ConfigureAwait(false)).Count > 0)
        {
            return Resultado.Fallo<EfectoCarteraDto>(Error.Conflicto("cartera.ya_anulado", "El efecto ya está anulado."));
        }

        if (efecto.Origen == "Renovacion")
        {
            return Resultado.Fallo<EfectoCarteraDto>(Error.Conflicto("cartera.de_renovacion", "El efecto sale de una renovación: anula la renovación."));
        }

        if (efecto.Origen == "Liquidacion")
        {
            return Resultado.Fallo<EfectoCarteraDto>(Error.Conflicto("cartera.de_liquidacion", "Es el pagaré de una liquidación de pagos: anula la liquidación."));
        }

        var liquidado = await _movimientos.SumaAsync(TipoDocumentoTesoreria.Cartera, efectoId, ct).ConfigureAwait(false);
        if (liquidado != 0m)
        {
            return Resultado.Fallo<EfectoCarteraDto>(Error.Conflicto("cartera.con_movimientos",
                $"El efecto tiene {Redondeo.Formatear(liquidado)} € cobrados o pagados: anula antes esos movimientos."));
        }

        var anulacion = AnulacionEfecto.Crear(efecto, motivo, _reloj);
        _cartera.Agregar(anulacion.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(efecto, 0m, true));
    }
}
