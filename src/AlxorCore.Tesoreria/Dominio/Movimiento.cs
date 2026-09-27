using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Documento al que se asocia un movimiento de tesorería.</summary>
public enum TipoDocumentoTesoreria
{
    Factura = 1,
    Gasto = 2,

    /// <summary>Efecto de la cartera pendiente sin documento en ALXOR (p. ej. migrado de otro ERP).</summary>
    Cartera = 3,
}

/// <summary>Sentido del movimiento.</summary>
public enum SentidoMovimiento
{
    /// <summary>Cobro (entra dinero; asociado a una factura).</summary>
    Cobro = 1,

    /// <summary>Pago (sale dinero; asociado a un gasto).</summary>
    Pago = 2,
}

/// <summary>Estado de cobro/pago de un documento, derivado del saldo (invariante P2).</summary>
public enum EstadoSaldo
{
    Pendiente = 1,
    Parcial = 2,
    Liquidado = 3,
}

/// <summary>Se ha registrado un movimiento de tesorería.</summary>
public sealed record MovimientoRegistrado(Guid MovimientoId, Guid EmpresaId, decimal Importe, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>
/// Movimiento de tesorería: un cobro (contra una factura) o un pago (contra un gasto), total o
/// parcial. El estado de saldo del documento se deriva de la suma de sus movimientos, no se fija a
/// mano (invariante P2). El caso de uso impide el sobrepago (invariante P1).
/// </summary>
public sealed class Movimiento : RaizAgregadoEmpresa<Guid>
{
    private Movimiento(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Movimiento(Guid id, Guid empresaId, TipoDocumentoTesoreria tipoDocumento, Guid documentoId, SentidoMovimiento sentido, decimal importe, DateOnly fecha, string? metodo, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        Sentido = sentido;
        Importe = importe;
        Fecha = fecha;
        Metodo = metodo;
        CreadoEn = ahora;
    }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    public SentidoMovimiento Sentido { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Metodo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>
    /// Si es una anulación: el movimiento que anula. La anulación lleva el importe en negativo, así que cualquier
    /// suma de movimientos (saldo del documento, riesgo, cierre de caja) ya la descuenta. Solo una por movimiento.
    /// </summary>
    public Guid? AnulaMovimientoId { get; private set; }

    /// <summary>
    /// Cuenta de tesorería (banco o caja) por la que entra o sale el dinero; su subcuenta es la del asiento. Null en los
    /// movimientos anteriores a las cuentas bancarias o cuando la empresa no tiene ninguna (asiento a 572 o 570).
    /// </summary>
    public Guid? CuentaBancariaId { get; private set; }

    public const string MetodoAnulacion = "Anulación";

    /// <summary>Anulación de un cobro o pago: mismo documento y sentido, importe en negativo.</summary>
    public static Resultado<Movimiento> CrearAnulacion(Movimiento original, DateOnly fecha, IReloj reloj, string? metodo = null)
    {
        ArgumentNullException.ThrowIfNull(original);
        ArgumentNullException.ThrowIfNull(reloj);
        if (original.AnulaMovimientoId is not null)
        {
            return Resultado.Fallo<Movimiento>(Error.Conflicto("movimiento.es_anulacion", "Este movimiento ya es una anulación."));
        }

        if (fecha < original.Fecha)
        {
            return Resultado.Fallo<Movimiento>(Error.Validacion("movimiento.fecha_anulacion", "La anulación no puede ser anterior al movimiento que anula."));
        }

        var anulacion = new Movimiento(Guid.NewGuid(), original.EmpresaId, original.TipoDocumento, original.DocumentoId, original.Sentido,
            -original.Importe, fecha, string.IsNullOrWhiteSpace(metodo) ? MetodoAnulacion : Recortar(metodo.Trim()), reloj.AhoraUtc)
        {
            AnulaMovimientoId = original.Id,
            CuentaBancariaId = original.CuentaBancariaId,
        };
        return Resultado.Ok(anulacion);
    }

    public static Resultado<Movimiento> Crear(
        Guid empresaId, TipoDocumentoTesoreria tipoDocumento, Guid documentoId, SentidoMovimiento sentido, decimal importe, DateOnly fecha, string? metodo, IReloj reloj,
        Guid? cuentaBancariaId = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (importe <= 0)
        {
            return Resultado.Fallo<Movimiento>(Error.Validacion("movimiento.importe_invalido", "El importe debe ser mayor que cero."));
        }

        var movimiento = new Movimiento(
            Guid.NewGuid(), empresaId, tipoDocumento, documentoId, sentido, Redondeo.Dos(importe), fecha,
            string.IsNullOrWhiteSpace(metodo) ? null : Recortar(metodo.Trim()), reloj.AhoraUtc)
        {
            CuentaBancariaId = cuentaBancariaId,
        };
        movimiento.RegistrarEvento(new MovimientoRegistrado(movimiento.Id, empresaId, movimiento.Importe, reloj.AhoraUtc));
        return Resultado.Ok(movimiento);
    }

    private static string Recortar(string texto) => texto.Length > 40 ? texto[..40] : texto;

    /// <summary>Deriva el estado de saldo de un documento a partir de su total y lo ya liquidado (P2).</summary>
    public static EstadoSaldo DerivarEstado(decimal total, decimal liquidado)
    {
        if (liquidado <= 0)
        {
            return EstadoSaldo.Pendiente;
        }

        return liquidado >= total ? EstadoSaldo.Liquidado : EstadoSaldo.Parcial;
    }
}
