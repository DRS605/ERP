using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Motivo SEPA de devolución con su descripción.</summary>
public sealed record MotivoDevolucionDto(string Codigo, string Descripcion);

/// <summary>
/// Datos de una devolución: el cobro devuelto, el motivo SEPA, la fecha y los gastos que cobra el banco.
/// Con <paramref name="RepercutirGastos"/> los gastos se reclaman al cliente (efecto a cobrar) en vez de ir a 626.
/// </summary>
public sealed record RegistrarDevolucionComando(Guid MovimientoId, string Motivo, DateOnly? Fecha = null, decimal Gastos = 0m, bool RepercutirGastos = false,
    string? Nota = null);

public sealed record DevolucionDto(
    Guid Id, Guid MovimientoId, Guid AnulacionMovimientoId, string TipoDocumento, Guid DocumentoId, string Documento, string Tercero, Guid? RemesaId,
    DateOnly Fecha, string Motivo, string MotivoDescripcion, decimal Importe, decimal Gastos, bool GastosRepercutidos, Guid? EfectoGastosId, string? Nota);

public interface IRepositorioDevoluciones
{
    void Agregar(DevolucionRecibo devolucion);

    Task<bool> ExisteDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default);

    Task<IReadOnlyList<DevolucionRecibo>> ListarAsync(CancellationToken ct = default);

    /// <summary>Devoluciones de unos documentos (la última de cada uno sirve para los impagados).</summary>
    Task<IReadOnlyList<DevolucionRecibo>> DeDocumentosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default);

    /// <summary>Motivo de la devolución de cada movimiento devuelto (movimiento → código).</summary>
    Task<IReadOnlyDictionary<Guid, string>> MotivosPorMovimientoAsync(IReadOnlyCollection<Guid> movimientoIds, CancellationToken ct = default);
}

/// <summary>
/// Devoluciones de recibos domiciliados. Registrar una devolución anula el cobro (movimiento en negativo, trazable a
/// la devolución y al cobro original): la factura o el efecto vuelve a tener pendiente y aparece en Impagados con el
/// motivo. Se contabiliza el contraasiento (cliente contra el banco) y los gastos de devolución: a 626 (servicios
/// bancarios) contra el banco o, si se repercuten, a la cuenta del cliente con un efecto a cobrar por ese importe.
/// </summary>
public sealed class GestionDevoluciones
{
    private readonly IRepositorioDevoluciones _devoluciones;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioRemesas _remesas;
    private readonly IRepositorioCartera _cartera;
    private readonly IConsultaFacturas _facturas;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;

    public GestionDevoluciones(IRepositorioDevoluciones devoluciones, IRepositorioMovimientos movimientos, IRepositorioRemesas remesas, IRepositorioCartera cartera,
        IConsultaFacturas facturas, IUnidadDeTrabajoTesoreria unidad, IReloj reloj, ContabilizacionTesoreria? contabilizacion = null)
    {
        _devoluciones = devoluciones;
        _movimientos = movimientos;
        _remesas = remesas;
        _cartera = cartera;
        _facturas = facturas;
        _unidad = unidad;
        _reloj = reloj;
        _contabilizacion = contabilizacion;
    }

    public static IReadOnlyList<MotivoDevolucionDto> Motivos() =>
        MotivosDevolucionSepa.Todos.Select(kv => new MotivoDevolucionDto(kv.Key, kv.Value)).ToList();

    /// <summary>¿Es un cobro por domiciliación (método «Domiciliación…» o cobrado en una remesa de adeudos)?</summary>
    public static bool EsDomiciliado(Movimiento m, bool enRemesa) =>
        m.Sentido == SentidoMovimiento.Cobro && (enRemesa || (m.Metodo is { } metodo && metodo.StartsWith("Domicili", StringComparison.OrdinalIgnoreCase)));

    public async Task<Resultado<DevolucionDto>> RegistrarAsync(Guid empresaId, RegistrarDevolucionComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var cobro = await _movimientos.ObtenerAsync(c.MovimientoId, ct).ConfigureAwait(false);
        if (cobro is null || cobro.EmpresaId != empresaId)
        {
            return Resultado.Fallo<DevolucionDto>(Error.NoEncontrado("movimiento.no_encontrado", "El cobro no existe."));
        }

        if (cobro.AnulaMovimientoId is not null || cobro.TipoDocumento == TipoDocumentoTesoreria.Gasto)
        {
            return Resultado.Fallo<DevolucionDto>(Error.Conflicto("devolucion.no_es_cobro", "Solo se devuelve un cobro de una factura o de un efecto."));
        }

        var remesa = await _remesas.DeMovimientoAsync(cobro.Id, ct).ConfigureAwait(false);
        if (!EsDomiciliado(cobro, remesa is { Tipo: TipoRemesa.Cobro }))
        {
            return Resultado.Fallo<DevolucionDto>(Error.Conflicto("devolucion.no_domiciliado",
                "Solo se registran devoluciones de cobros por domiciliación (recibos de una remesa). Para otro cobro, anúlalo."));
        }

        if (await _devoluciones.ExisteDeMovimientoAsync(cobro.Id, ct).ConfigureAwait(false) || await _movimientos.EstaAnuladoAsync(cobro.Id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<DevolucionDto>(Error.Conflicto("devolucion.ya_devuelto", "Este cobro ya está devuelto o anulado."));
        }

        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var fecha = c.Fecha ?? (hoy < cobro.Fecha ? cobro.Fecha : hoy);
        var motivo = (c.Motivo ?? string.Empty).Trim().ToUpperInvariant();
        var anulacion = Movimiento.CrearAnulacion(cobro, fecha, _reloj, $"Devolución {motivo}");
        if (anulacion.EsFallo)
        {
            return Resultado.Fallo<DevolucionDto>(anulacion.Error);
        }

        var (terceroId, tercero, documento) = await DocumentoAsync(cobro, ct).ConfigureAwait(false);
        var gastos = Math.Round(c.Gastos, 2);
        EfectoCartera? efectoGastos = null;
        if (c.RepercutirGastos && gastos > 0m)
        {
            if (terceroId is null)
            {
                return Resultado.Fallo<DevolucionDto>(Error.Validacion("devolucion.sin_cliente", "El documento no tiene cliente: no se le pueden repercutir los gastos."));
            }

            var efecto = EfectoCartera.Crear(empresaId, SentidoCartera.Cobro, terceroId, tercero, $"Gastos devolución {documento}".Trim(), fecha, fecha, gastos,
                "Devolucion", anulacion.Valor.Id.ToString("N"), _reloj);
            if (efecto.EsFallo)
            {
                return Resultado.Fallo<DevolucionDto>(efecto.Error);
            }

            efectoGastos = efecto.Valor;
        }

        var devolucion = DevolucionRecibo.Crear(cobro, anulacion.Valor, remesa?.Id, fecha, motivo, gastos, c.RepercutirGastos, efectoGastos?.Id, c.Nota, _reloj);
        if (devolucion.EsFallo)
        {
            return Resultado.Fallo<DevolucionDto>(devolucion.Error);
        }

        _movimientos.Agregar(anulacion.Valor);
        if (efectoGastos is not null)
        {
            _cartera.Agregar(efectoGastos);
        }

        _devoluciones.Agregar(devolucion.Valor);
        if (_contabilizacion is not null)
        {
            // Contraasiento del cobro (cliente al debe, banco al haber) y, si hay, los gastos de devolución.
            await _contabilizacion.EncolarMovimientoAsync(anulacion.Valor, aplicacionAnticipo: false, cobro, $"Devolución {motivo}", ct).ConfigureAwait(false);
            if (gastos > 0m)
            {
                var banco = await _contabilizacion.CuentaTesoreriaAsync(cobro.CuentaBancariaId, cobro.Metodo, ct).ConfigureAwait(false);
                if (efectoGastos is not null)
                {
                    // Cobro «anulado» sin contrapartida fija: cuenta del cliente al debe, banco al haber.
                    _contabilizacion.EncolarAsientoDirecto(empresaId, "DevolucionGastos", devolucion.Valor.Id, SentidoMovimiento.Cobro,
                        $"Gastos devolución {documento} (a cargo del cliente)", fecha, gastos, banco, contrapartida: string.Empty, anulacion: true,
                        terceroId: terceroId, terceroNombre: tercero);
                }
                else
                {
                    _contabilizacion.EncolarAsientoDirecto(empresaId, "DevolucionGastos", devolucion.Valor.Id, SentidoMovimiento.Pago,
                        $"Gastos devolución {documento} ({motivo})", fecha, gastos, banco, "626");
                }
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        return Resultado.Ok(Dto(devolucion.Valor, documento, tercero));
    }

    public async Task<IReadOnlyList<DevolucionDto>> ListarAsync(CancellationToken ct = default)
    {
        var lista = await _devoluciones.ListarAsync(ct).ConfigureAwait(false);
        var res = new List<DevolucionDto>();
        foreach (var d in lista.OrderByDescending(x => x.Fecha).ThenByDescending(x => x.CreadoEn))
        {
            var (_, tercero, documento) = await DocumentoAsync(d.TipoDocumento, d.DocumentoId, ct).ConfigureAwait(false);
            res.Add(Dto(d, documento, tercero));
        }

        return res;
    }

    private Task<(Guid? TerceroId, string Tercero, string Documento)> DocumentoAsync(Movimiento m, CancellationToken ct) => DocumentoAsync(m.TipoDocumento, m.DocumentoId, ct);

    private async Task<(Guid? TerceroId, string Tercero, string Documento)> DocumentoAsync(TipoDocumentoTesoreria tipo, Guid id, CancellationToken ct)
    {
        if (tipo == TipoDocumentoTesoreria.Factura)
        {
            var f = await _facturas.ObtenerAsync(id, ct).ConfigureAwait(false);
            return (f?.ClienteId, f?.ClienteNombre ?? string.Empty, f?.NumeroCompleto ?? string.Empty);
        }

        var e = await _cartera.ObtenerAsync(id, ct).ConfigureAwait(false);
        return (e?.TerceroId, e?.TerceroNombre ?? string.Empty, e?.Documento ?? string.Empty);
    }

    private static DevolucionDto Dto(DevolucionRecibo d, string documento, string tercero) => new(
        d.Id, d.MovimientoId, d.AnulacionMovimientoId, d.TipoDocumento.ToString(), d.DocumentoId, documento, tercero, d.RemesaId, d.Fecha, d.Motivo,
        MotivosDevolucionSepa.Describir(d.Motivo), d.Importe, d.Gastos, d.GastosRepercutidos, d.EfectoGastosId, d.Nota);
}
