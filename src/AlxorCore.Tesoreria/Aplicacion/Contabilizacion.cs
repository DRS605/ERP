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

    private readonly IRepositorioSalidaTesoreria _salida;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioCartera _cartera;
    private readonly IColaContabilizacion _cola;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    public ContabilizacionTesoreria(IRepositorioSalidaTesoreria salida, IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioCartera cartera,
        IColaContabilizacion cola, IUnidadDeTrabajoTesoreria unidad, IReloj reloj)
    {
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
    /// Encola el asiento de un movimiento (o de su anulación, con <paramref name="original"/>). Si el cobro es la
    /// aplicación de un anticipo, la contrapartida es 438 en lugar de la tesorería.
    /// </summary>
    public async Task EncolarMovimientoAsync(Movimiento movimiento, bool aplicacionAnticipo, Movimiento? original = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(movimiento);
        var datos = original ?? movimiento;
        var (terceroId, tercero, documento) = await DocumentoAsync(datos, ct).ConfigureAwait(false);
        var sentido = datos.Sentido == SentidoMovimiento.Cobro ? SentidoContable.Cobro : SentidoContable.Pago;
        var tipo = sentido == SentidoContable.Cobro ? "Cobro" : "Pago";
        var referencia = Recortar((original is null ? "" : "Anulación: ") + $"{tipo} {documento}".Trim());
        var tesoreria = aplicacionAnticipo ? "438" : CuentaTesoreria(datos.Metodo);
        _salida.Agregar(MensajeSalida.Crear(movimiento.EmpresaId, MensajeSalida.TipoContabilizacion, SalidaJson.Serializar(new DocumentoContabilizable(
            sentido, OrigenMovimiento, movimiento.Id, referencia, terceroId, tercero, movimiento.Fecha,
            0m, string.Empty, 0m, 0m, 0m, Math.Abs(movimiento.Importe), Anulacion: original is not null, CuentaTesoreria: tesoreria)), _reloj.AhoraUtc));
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

    /// <summary>Identificador estable para la anulación de un anticipo (la cola es idempotente por origen).</summary>
    private static Guid DeterministaAnulacion(Guid id)
    {
        var b = id.ToByteArray();
        b[0] ^= 0xA5;
        b[15] ^= 0x5A;
        return new Guid(b);
    }
}
