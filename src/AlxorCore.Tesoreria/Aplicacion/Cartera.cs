using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

public sealed record EfectoCarteraDto(
    Guid Id, string Sentido, Guid? TerceroId, string TerceroNombre, string Documento, DateOnly? FechaDocumento, DateOnly Vencimiento,
    decimal Importe, decimal Liquidado, decimal Pendiente, string Estado, string? Origen, string? OrigenReferencia);

public sealed record CrearEfectoCarteraComando(
    SentidoCartera Sentido, string TerceroNombre, string Documento, DateOnly Vencimiento, decimal Importe, Guid? TerceroId = null,
    DateOnly? FechaDocumento = null, string? Origen = null, string? OrigenReferencia = null);

public sealed record MovimientoCarteraComando(decimal Importe, DateOnly? Fecha = null, string? Metodo = null);

public interface IRepositorioCartera
{
    void Agregar(EfectoCartera efecto);

    Task<EfectoCartera?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<EfectoCartera>> ListarAsync(Guid empresaId, SentidoCartera? sentido, CancellationToken ct = default);

    Task<EfectoCartera?> PorOrigenAsync(Guid empresaId, string origen, string origenReferencia, CancellationToken ct = default);
}

/// <summary>Cartera de efectos sin documento en ALXOR: alta, consulta con su saldo, y cobro o pago.</summary>
public sealed class GestionCartera
{
    private readonly IRepositorioCartera _cartera;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IConsultaTesoreria _consulta;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    public GestionCartera(IRepositorioCartera cartera, IRepositorioMovimientos movimientos, IConsultaTesoreria consulta, IUnidadDeTrabajoTesoreria unidad, IReloj reloj)
    {
        _cartera = cartera;
        _movimientos = movimientos;
        _consulta = consulta;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<EfectoCarteraDto>> ListarAsync(Guid empresaId, SentidoCartera? sentido, bool soloPendientes, CancellationToken ct = default)
    {
        var efectos = await _cartera.ListarAsync(empresaId, sentido, ct).ConfigureAwait(false);
        var liquidado = await _consulta.LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria.Cartera, efectos.Select(e => e.Id).ToList(), ct).ConfigureAwait(false);
        return efectos.Select(e => Dto(e, liquidado.GetValueOrDefault(e.Id)))
            .Where(e => !soloPendientes || e.Pendiente > 0m)
            .OrderBy(e => e.Vencimiento).ThenBy(e => e.TerceroNombre, StringComparer.CurrentCulture).ToList();
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

        return await RegistrarCobro.RegistrarAsync(empresaId, TipoDocumentoTesoreria.Cartera, efecto.Id,
            efecto.Sentido == SentidoCartera.Cobro ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago,
            c.Importe, efecto.Importe, c.Fecha, c.Metodo, _movimientos, _unidad, _reloj, ct).ConfigureAwait(false);
    }

    private static EfectoCarteraDto Dto(EfectoCartera e, decimal liquidado) => new(
        e.Id, e.Sentido.ToString(), e.TerceroId, e.TerceroNombre, e.Documento, e.FechaDocumento, e.Vencimiento, e.Importe, liquidado,
        e.Importe - liquidado, Movimiento.DerivarEstado(e.Importe, liquidado).ToString(), e.Origen, e.OrigenReferencia);
}
