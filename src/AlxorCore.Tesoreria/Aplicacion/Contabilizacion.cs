using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Repositorio de la bandeja de salida (outbox) del módulo Tesorería.</summary>
public interface IRepositorioSalidaTesoreria
{
    void Agregar(MensajeSalida mensaje);

    Task<IReadOnlyList<MensajeSalida>> PendientesAsync(int maximo, CancellationToken ct = default);
}

/// <summary>
/// Contabilización de la tesorería. Cada cobro, pago o anticipo (y su anulación) deja en la bandeja de salida, en la
/// misma transacción, el documento que genera su asiento:
/// <list type="bullet">
/// <item>Cobro de una factura o de un efecto a cobrar: tesorería (572, o 570 en efectivo) a la cuenta del cliente.</item>
/// <item>Pago de un gasto o de un efecto a pagar: la cuenta del proveedor a tesorería.</item>
/// <item>Anticipo recibido: tesorería a 438 (anticipos de clientes); al aplicarlo a una factura, 438 a la cuenta del cliente.</item>
/// <item>Anulaciones: el contraasiento.</item>
/// </list>
/// Después, <see cref="DespacharAsync"/> entrega los mensajes a la cola de contabilización (que decide según el modo
/// de la empresa: en modo Simple no hay asientos).
/// </summary>
public sealed class ContabilizacionTesoreria
{
    public const string OrigenMovimiento = "Movimiento";
    public const string OrigenAnticipo = "Anticipo";
    public const string OrigenEntregaCuenta = "EntregaCuenta";

    private readonly IRepositorioSalidaTesoreria _salida;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioCartera _cartera;
    private readonly IColaContabilizacion _cola;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly IRepositorioCuentasBancarias? _bancos;
    private readonly IRepositorioDeudas? _deudas;

    public const string OrigenDeuda = "SituacionDeuda";

    public ContabilizacionTesoreria(IRepositorioSalidaTesoreria salida, IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioCartera cartera,
        IColaContabilizacion cola, IUnidadDeTrabajoTesoreria unidad, IReloj reloj, IRepositorioCuentasBancarias? bancos = null, IRepositorioDeudas? deudas = null)
    {
        _deudas = deudas;
        _bancos = bancos;
        _salida = salida;
        _facturas = facturas;
        _gastos = gastos;
        _cartera = cartera;
        _cola = cola;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Cuenta de tesorería según el método: efectivo a caja, lo demás al banco.</summary>
    public static string CuentaTesoreria(string? metodo) =>
        metodo is not null && metodo.Contains("efectivo", StringComparison.OrdinalIgnoreCase) ? "570" : "572";

    /// <summary>
    /// Subcuenta de tesorería de un movimiento: la de su cuenta bancaria o caja; sin ella, 570 en efectivo y 572 en lo demás.
    /// </summary>
    public async Task<string> CuentaTesoreriaAsync(Guid? cuentaBancariaId, string? metodo, CancellationToken ct = default)
    {
        if (cuentaBancariaId is { } id && _bancos is not null && await _bancos.ObtenerAsync(id, ct).ConfigureAwait(false) is { } banco)
        {
            return banco.Subcuenta;
        }

        return CuentaTesoreria(metodo);
    }

    /// <summary>
    /// Encola el asiento de un movimiento (o de su anulación, con <paramref name="original"/>). Si el cobro es la
    /// aplicación de un anticipo, la contrapartida es 438 en lugar de la tesorería. <paramref name="prefijo"/> cambia el
    /// «Anulación:» del concepto (p. ej. «Devolución AM04:»).
    /// </summary>
    public async Task EncolarMovimientoAsync(Movimiento movimiento, bool aplicacionAnticipo, Movimiento? original = null, string? prefijo = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(movimiento);
        var datos = original ?? movimiento;
        var (terceroId, tercero, documento) = await DocumentoAsync(datos, ct).ConfigureAwait(false);
        var sentido = datos.Sentido == SentidoMovimiento.Cobro ? SentidoContable.Cobro : SentidoContable.Pago;
        var tipo = sentido == SentidoContable.Cobro ? "Cobro" : "Pago";
        var referencia = Recortar((original is null ? "" : (prefijo ?? "Anulación") + ": ") + $"{tipo} {documento}".Trim());
        var tesoreria = datos.CuentaPuente ?? (aplicacionAnticipo ? "438" : null) ?? await CuentaTesoreriaAsync(datos.CuentaBancariaId, datos.Metodo, ct).ConfigureAwait(false);
        var cuentaDocumento = await CuentaDocumentoAsync(datos, ct).ConfigureAwait(false);
        _salida.Agregar(MensajeSalida.Crear(movimiento.EmpresaId, MensajeSalida.TipoContabilizacion, SalidaJson.Serializar(new DocumentoContabilizable(
            sentido, OrigenMovimiento, movimiento.Id, referencia, terceroId, tercero, movimiento.Fecha,
            0m, string.Empty, 0m, 0m, 0m, Math.Abs(movimiento.Importe), Anulacion: (original is not null) != (datos.Importe < 0m), CuentaTesoreria: tesoreria, CuentaTercero: cuentaDocumento)),
            _reloj.AhoraUtc));

        // Deuda fuera de su cuenta (impagado 4315, dudoso 436): lo cobrado vuelve antes a la de origen y su deterioro se
        // revierte en proporción; si se anula el cobro, se deshace.
        if (_deudas is not null && datos.Sentido == SentidoMovimiento.Cobro && datos.TipoDocumento is TipoDocumentoTesoreria.Factura or TipoDocumentoTesoreria.Cartera)
        {
            if (original is null)
            {
                var situacion = await _deudas.SituacionAsync(datos.TipoDocumento, datos.DocumentoId, ct).ConfigureAwait(false);
                if (situacion is { Cuenta: { } cuenta, Importe: > 0m })
                {
                    var (importe, dotacion) = situacion.Regularizar(Math.Abs(movimiento.Importe));
                    EncolarTraspaso(movimiento.EmpresaId, Derivado(movimiento.Id, "deuda"), $"Cobro de {documento} ({NombreCuenta(cuenta)})", movimiento.Fecha, importe,
                        situacion.CuentaOrigen, cuenta, terceroId, tercero);
                    if (dotacion > 0m)
                    {
                        EncolarTraspaso(movimiento.EmpresaId, Derivado(movimiento.Id, "reversion"), $"Reversión deterioro {documento}", movimiento.Fecha, dotacion,
                            CuentasDeuda.Deterioro, CuentasDeuda.Reversion, null, null);
                    }

                    _deudas.Agregar(RegularizacionDeuda.Crear(situacion, movimiento.Id, cuenta, importe, dotacion));
                }
            }
            else if (await _deudas.RegularizacionDeAsync(original.Id, ct).ConfigureAwait(false) is { } reg
                && await _deudas.SituacionAsync(reg.SituacionId, ct).ConfigureAwait(false) is { } situacion)
            {
                situacion.Deshacer(reg.Cuenta, reg.Importe, reg.Dotacion);
                EncolarTraspaso(movimiento.EmpresaId, Derivado(movimiento.Id, "deuda"), $"Anulación del cobro de {documento} ({NombreCuenta(reg.Cuenta)})", movimiento.Fecha,
                    reg.Importe, reg.Cuenta, situacion.CuentaOrigen, terceroId, tercero);
                if (reg.Dotacion > 0m)
                {
                    EncolarTraspaso(movimiento.EmpresaId, Derivado(movimiento.Id, "dotacion"), $"Deterioro {documento}", movimiento.Fecha, reg.Dotacion,
                        CuentasDeuda.Dotacion, CuentasDeuda.Deterioro, null, null);
                }

                reg.Deshacer();
            }
        }
    }

    /// <summary>
    /// Encola un traspaso entre dos cuentas: <paramref name="debe"/> al debe y <paramref name="haber"/> al haber. Una cuenta
    /// null es la del tercero (el cliente).
    /// </summary>
    public void EncolarTraspaso(Guid empresaId, Guid origenId, string referencia, DateOnly fecha, decimal importe, string? debe, string? haber, Guid? terceroId,
        string? terceroNombre)
    {
        if (importe <= 0m)
        {
            return;
        }

        if (debe is not null)
        {
            EncolarAsientoDirecto(empresaId, OrigenDeuda, origenId, SentidoMovimiento.Cobro, referencia, fecha, importe, debe, haber ?? string.Empty,
                terceroId: haber is null ? terceroId : null, terceroNombre: haber is null ? terceroNombre : null);
        }
        else if (haber is not null)
        {
            EncolarAsientoDirecto(empresaId, OrigenDeuda, origenId, SentidoMovimiento.Cobro, referencia, fecha, importe, haber, string.Empty, anulacion: true,
                terceroId: terceroId, terceroNombre: terceroNombre);
        }
    }

    private static string NombreCuenta(string cuenta) => cuenta switch
    {
        CuentasDeuda.Impagados => "impagado",
        CuentasDeuda.Dudoso => "dudoso cobro",
        _ => cuenta,
    };

    /// <summary>Cuenta propia del documento (la del efecto, si la tiene); null: la del tercero.</summary>
    public async Task<string?> CuentaDocumentoAsync(Movimiento m, CancellationToken ct = default) =>
        m.TipoDocumento == TipoDocumentoTesoreria.Cartera ? (await _cartera.ObtenerAsync(m.DocumentoId, ct).ConfigureAwait(false))?.CuentaContable : null;

    /// <summary>
    /// Encola un asiento de tesorería contra una cuenta concreta (sin documento de tercero): gastos bancarios, intereses o
    /// comisiones de un apunte del extracto, o gastos de devolución de un recibo. Un cobro va de la tesorería
    /// (<paramref name="cuentaTesoreria"/>) a la <paramref name="contrapartida"/>; un pago, de la contrapartida a la tesorería.
    /// </summary>
    public void EncolarAsientoDirecto(Guid empresaId, string origenTipo, Guid origenId, SentidoMovimiento sentido, string referencia, DateOnly fecha,
        decimal importe, string cuentaTesoreria, string contrapartida, bool anulacion = false, Guid? terceroId = null, string? terceroNombre = null)
    {
        _salida.Agregar(MensajeSalida.Crear(empresaId, MensajeSalida.TipoContabilizacion, SalidaJson.Serializar(new DocumentoContabilizable(
            sentido == SentidoMovimiento.Cobro ? SentidoContable.Cobro : SentidoContable.Pago, origenTipo, origenId, Recortar(referencia), terceroId,
            terceroNombre ?? string.Empty, fecha, 0m, string.Empty, 0m, 0m, 0m, Math.Abs(importe), Anulacion: anulacion,
            CuentaTesoreria: cuentaTesoreria, CuentaTercero: string.IsNullOrWhiteSpace(contrapartida) ? null : contrapartida)), _reloj.AhoraUtc));
    }

    /// <summary>Encola el asiento de un anticipo recibido (tesorería a 438) o de su anulación.</summary>
    public void EncolarAnticipo(Anticipo anticipo, string? clienteNombre, bool anulacion)
    {
        ArgumentNullException.ThrowIfNull(anticipo);
        var referencia = Recortar((anulacion ? "Anulación: " : "") + "Anticipo · " + anticipo.Concepto);
        // La anulación usa su propio origen (el anticipo ya encoló su alta con su id).
        var origenId = anulacion ? DeterministaAnulacion(anticipo.Id) : anticipo.Id;
        _salida.Agregar(MensajeSalida.Crear(anticipo.EmpresaId, MensajeSalida.TipoContabilizacion, SalidaJson.Serializar(new DocumentoContabilizable(
            SentidoContable.Cobro, OrigenAnticipo, origenId, referencia, anticipo.ClienteId, clienteNombre ?? string.Empty,
            anulacion ? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime) : anticipo.Fecha,
            0m, string.Empty, 0m, 0m, 0m, anticipo.Importe, Anulacion: anulacion,
            CuentaTesoreria: CuentaTesoreria(anticipo.Metodo), CuentaTercero: "438")), _reloj.AhoraUtc));
    }

    /// <summary>Entrega los mensajes pendientes a la cola de contabilización (idempotente, con reintento).</summary>
    public async Task<int> DespacharAsync(int maximo = 100, CancellationToken ct = default)
    {
        var pendientes = await _salida.PendientesAsync(maximo, ct).ConfigureAwait(false);
        var procesados = 0;
        foreach (var mensaje in pendientes)
        {
            try
            {
                if (mensaje.Tipo == MensajeSalida.TipoContabilizacion && SalidaJson.DeserializarContabilizacion(mensaje.Carga) is { } doc)
                {
                    await _cola.EncolarAsync(mensaje.EmpresaId, doc, ct).ConfigureAwait(false);
                }

                mensaje.MarcarProcesado(_reloj.AhoraUtc);
                procesados++;
            }
            catch (Exception ex)
            {
                mensaje.RegistrarError(ex.Message);
            }
        }

        if (pendientes.Count > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return procesados;
    }

    /// <summary>Tercero y número del documento que se cobra o se paga.</summary>
    private async Task<(Guid? TerceroId, string Tercero, string Documento)> DocumentoAsync(Movimiento m, CancellationToken ct)
    {
        switch (m.TipoDocumento)
        {
            case TipoDocumentoTesoreria.Factura:
                var f = await _facturas.ObtenerAsync(m.DocumentoId, ct).ConfigureAwait(false);
                return (f?.ClienteId, f?.ClienteNombre ?? string.Empty, f?.NumeroCompleto ?? string.Empty);
            case TipoDocumentoTesoreria.Gasto:
                var g = await _gastos.ObtenerAsync(m.DocumentoId, ct).ConfigureAwait(false);
                return (g?.ProveedorId, g?.ProveedorTexto ?? string.Empty, g?.Concepto ?? string.Empty);
            case TipoDocumentoTesoreria.Cartera:
                var e = await _cartera.ObtenerAsync(m.DocumentoId, ct).ConfigureAwait(false);
                return (e?.TerceroId, e?.TerceroNombre ?? string.Empty, e?.Documento ?? string.Empty);
            default:
                return (null, string.Empty, string.Empty);
        }
    }

    private static string Recortar(string texto) => texto.Length > 80 ? texto[..80] : texto;

    /// <summary>Identificador estable derivado de otro y una etiqueta (varios asientos de un mismo documento, cola idempotente).</summary>
    internal static Guid Derivado(Guid id, string etiqueta)
    {
        var h = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(id.ToString("N") + ":" + etiqueta));
        return new Guid(h.AsSpan(0, 16));
    }

    /// <summary>Identificador estable para la anulación de un anticipo (la cola es idempotente por origen).</summary>
    internal static Guid DeterministaAnulacion(Guid id)
    {
        var b = id.ToByteArray();
        b[0] ^= 0xA5;
        b[15] ^= 0x5A;
        return new Guid(b);
    }
}
