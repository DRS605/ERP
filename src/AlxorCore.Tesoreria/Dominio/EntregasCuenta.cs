using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Estado de una entrega a cuenta, derivado de lo cancelado (nunca se fija a mano).</summary>
public enum EstadoEntregaCuenta
{
    Pendiente = 1,
    Parcial = 2,
    Cancelada = 3,
    Anulada = 4,
}

/// <summary>
/// Cancelación de (parte de) una entrega a cuenta contra una factura del proveedor: un pago de esa factura con la cuenta
/// puente 407 (proveedores a anticipos a proveedores). Las de signo negativo deshacen otra al anular su liquidación.
/// </summary>
public sealed class CancelacionEntregaCuenta
{
    private CancelacionEntregaCuenta()
    {
    }

    internal CancelacionEntregaCuenta(Guid gastoId, decimal importe, DateOnly fecha, Guid movimientoId, Guid? liquidacionId)
    {
        Id = Guid.NewGuid();
        GastoId = gastoId;
        Importe = importe;
        Fecha = fecha;
        MovimientoId = movimientoId;
        LiquidacionId = liquidacionId;
    }

    public Guid Id { get; private set; }

    public Guid GastoId { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Pago de la factura (o su anulación) que refleja la cancelación.</summary>
    public Guid MovimientoId { get; private set; }

    /// <summary>Liquidación de pagos que la hizo (null si se aplicó a mano).</summary>
    public Guid? LiquidacionId { get; private set; }
}

/// <summary>
/// Entrega a cuenta a un proveedor o agricultor, como en Hispatec (<c>EntregasCuentaProveedor</c>): dinero adelantado
/// <b>sin IVA</b> antes de su factura o liquidación. Se contabiliza 407 a tesorería y se cancela después contra sus
/// facturas (400 a 407), a mano o en la liquidación de pagos. Nunca se cancela más de lo pendiente.
/// </summary>
public sealed class EntregaCuentaProveedor : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudConcepto = 200;

    private readonly List<CancelacionEntregaCuenta> _cancelaciones = [];

    private EntregaCuentaProveedor(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
    }

    private EntregaCuentaProveedor(Guid id, Guid empresaId, Guid proveedorId, decimal importe, DateOnly fecha, string concepto, string? metodo, Guid? cuentaBancariaId,
        DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProveedorId = proveedorId;
        Importe = importe;
        Fecha = fecha;
        Concepto = concepto;
        Metodo = metodo;
        CuentaBancariaId = cuentaBancariaId;
        CreadaEn = ahora;
    }

    public Guid ProveedorId { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Concepto { get; private set; }

    public string? Metodo { get; private set; }

    /// <summary>Banco o caja por el que salió el dinero.</summary>
    public Guid? CuentaBancariaId { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public IReadOnlyList<CancelacionEntregaCuenta> Cancelaciones => _cancelaciones;

    public decimal Cancelado => Redondeo.Dos(_cancelaciones.Sum(c => c.Importe));

    public decimal Pendiente => AnuladaEn is null ? Redondeo.Dos(Importe - Cancelado) : 0m;

    public EstadoEntregaCuenta Estado => AnuladaEn is not null ? EstadoEntregaCuenta.Anulada
        : Cancelado == 0 ? EstadoEntregaCuenta.Pendiente : Pendiente == 0 ? EstadoEntregaCuenta.Cancelada : EstadoEntregaCuenta.Parcial;

    public static Resultado<EntregaCuentaProveedor> Registrar(Guid empresaId, Guid proveedorId, decimal importe, DateOnly fecha, string? concepto, string? metodo,
        Guid? cuentaBancariaId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var importeRedondeado = Redondeo.Dos(importe);
        if (importeRedondeado <= 0)
        {
            return Resultado.Fallo<EntregaCuentaProveedor>(Error.Validacion("entregacuenta.importe", "El importe de la entrega a cuenta debe ser mayor que cero."));
        }

        var texto = string.IsNullOrWhiteSpace(concepto) ? "Entrega a cuenta" : concepto.Trim();
        var metodoLimpio = string.IsNullOrWhiteSpace(metodo) ? null : metodo.Trim()[..Math.Min(metodo.Trim().Length, 60)];
        return Resultado.Ok(new EntregaCuentaProveedor(Guid.NewGuid(), empresaId, proveedorId, importeRedondeado, fecha, texto[..Math.Min(texto.Length, LongitudConcepto)],
            metodoLimpio, cuentaBancariaId, reloj.AhoraUtc));
    }

    /// <summary>Comprueba que se pueden cancelar <paramref name="importe"/> euros (sin anotar nada).</summary>
    public Resultado ValidarCancelacion(decimal importe)
    {
        if (AnuladaEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("entregacuenta.anulada", "La entrega a cuenta está anulada."));
        }

        if (Redondeo.Dos(importe) <= 0)
        {
            return Resultado.Fallo(Error.Validacion("entregacuenta.importe", "El importe a cancelar debe ser mayor que cero."));
        }

        return Redondeo.Dos(importe) > Pendiente
            ? Resultado.Fallo(Error.Conflicto("entregacuenta.supera_pendiente", $"La entrega a cuenta solo tiene {Pendiente:0.00} pendientes de cancelar."))
            : Resultado.Ok();
    }

    /// <summary>Anota la cancelación contra una factura del proveedor (el pago con 407 ya está creado).</summary>
    public Resultado Cancelar(Guid gastoId, decimal importe, DateOnly fecha, Guid movimientoId, Guid? liquidacionId)
    {
        var v = ValidarCancelacion(importe);
        if (v.EsFallo)
        {
            return v;
        }

        _cancelaciones.Add(new CancelacionEntregaCuenta(gastoId, Redondeo.Dos(importe), fecha, movimientoId, liquidacionId));
        return Resultado.Ok();
    }

    /// <summary>Deshace la cancelación de un pago que se anula (con su anulación): la entrega vuelve a quedar pendiente.</summary>
    public void RevertirCancelacion(Guid movimientoId, Guid anulacionId, DateOnly fecha)
    {
        var c = _cancelaciones.Single(x => x.MovimientoId == movimientoId);
        _cancelaciones.Add(new CancelacionEntregaCuenta(c.GastoId, -c.Importe, fecha, anulacionId, c.LiquidacionId));
    }

    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (AnuladaEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("entregacuenta.anulada", "La entrega a cuenta ya está anulada."));
        }

        if (Cancelado != 0)
        {
            return Resultado.Fallo(Error.Conflicto("entregacuenta.cancelada",
                "La entrega ya está cancelada (total o parcialmente) contra facturas: anula antes esas cancelaciones o su liquidación."));
        }

        AnuladaEn = reloj.AhoraUtc;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }
}

/// <summary>Qué representa una línea de la liquidación de pagos.</summary>
public enum TipoLineaLiquidacionPagos
{
    /// <summary>Parte de una factura del proveedor saldada con una entrega a cuenta (400 a 407).</summary>
    EntregaCuenta = 1,

    /// <summary>Parte de una factura del proveedor saldada con lo que él debe como cliente (400 a 555).</summary>
    Compensacion = 2,

    /// <summary>Parte de una factura del mismo NIF como cliente cobrada por compensación (555 a 430).</summary>
    CobroCompensado = 3,

    /// <summary>Pago del líquido de una factura por banco o caja.</summary>
    Pago = 4,
}

/// <summary>Cómo se paga el líquido de la liquidación.</summary>
public enum FormaPagoLiquidacion
{
    /// <summary>El líquido queda pendiente en las facturas (se pagará después).</summary>
    Pendiente = 1,

    /// <summary>Se registra ya el pago por el banco o la caja indicados.</summary>
    Directo = 2,

    /// <summary>Se incluye en una remesa SEPA de transferencias (pain.001); el pago se registra al liquidar la remesa.</summary>
    Remesa = 3,
}

public enum EstadoLiquidacionPagos
{
    Emitida = 1,
    Anulada = 2,
}

/// <summary>Una línea de la liquidación: el documento, lo aplicado y el movimiento de tesorería que lo refleja.</summary>
public sealed class LineaLiquidacionPagos
{
    private LineaLiquidacionPagos()
    {
        Documento = null!;
    }

    internal LineaLiquidacionPagos(TipoLineaLiquidacionPagos tipo, Guid documentoId, string documento, decimal importe, Guid? movimientoId, Guid? entregaId)
    {
        Id = Guid.NewGuid();
        Tipo = tipo;
        DocumentoId = documentoId;
        Documento = documento.Length > 60 ? documento[..60] : documento;
        Importe = importe;
        MovimientoId = movimientoId;
        EntregaId = entregaId;
    }

    public Guid Id { get; private set; }

    public TipoLineaLiquidacionPagos Tipo { get; private set; }

    /// <summary>Factura del proveedor (gasto) o, en un cobro compensado, factura de venta.</summary>
    public Guid DocumentoId { get; private set; }

    public string Documento { get; private set; }

    public decimal Importe { get; private set; }

    public Guid? MovimientoId { get; private set; }

    public Guid? EntregaId { get; private set; }
}

/// <summary>
/// Liquidación de pagos a un proveedor o agricultor, como la de Hispatec (<c>Liquidaciones</c>): reúne sus facturas
/// pendientes hasta una fecha, cancela sus <b>entregas a cuenta</b>, <b>compensa</b> lo que debe él como cliente (mismo
/// NIF) y paga el <b>líquido</b> (directo o en remesa). Numerada sin huecos por ejercicio (<c>LP-2026-000001</c>). Nunca
/// sale negativa: si él debe más, se compensa solo hasta cubrir lo que se le debe.
/// </summary>
public sealed class LiquidacionPagos : RaizAgregadoEmpresa<Guid>
{
    public const string Serie = "LP";

    private readonly List<LineaLiquidacionPagos> _lineas = [];

    private LiquidacionPagos(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private LiquidacionPagos(Guid id, Guid empresaId, int numero, DateOnly fecha, DateOnly hasta, Guid proveedorId, Guid? clienteId, FormaPagoLiquidacion forma,
        Guid? cuentaBancariaId, Guid? loteId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        Fecha = fecha;
        Hasta = hasta;
        ProveedorId = proveedorId;
        ClienteId = clienteId;
        FormaPago = forma;
        CuentaBancariaId = cuentaBancariaId;
        LoteId = loteId;
        Estado = EstadoLiquidacionPagos.Emitida;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"{Serie}-{Ejercicio}-{Numero:D6}";

    public DateOnly Fecha { get; private set; }

    /// <summary>Fecha de corte: entran las facturas hasta ese día.</summary>
    public DateOnly Hasta { get; private set; }

    public Guid ProveedorId { get; private set; }

    /// <summary>Ficha de cliente del mismo NIF cuyas facturas se compensaron (si la hay).</summary>
    public Guid? ClienteId { get; private set; }

    /// <summary>Suma de lo pendiente de las facturas del proveedor incluidas.</summary>
    public decimal APagar { get; private set; }

    public decimal EntregasCanceladas { get; private set; }

    public decimal Compensado { get; private set; }

    /// <summary>Lo que se le paga: a pagar − entregas − compensado.</summary>
    public decimal Liquido { get; private set; }

    public FormaPagoLiquidacion FormaPago { get; private set; }

    public Guid? CuentaBancariaId { get; private set; }

    /// <summary>Remesa de transferencias en la que va el líquido.</summary>
    public Guid? RemesaId { get; private set; }

    /// <summary>Liquidación masiva de la que forma parte (todas las del lote comparten este id).</summary>
    public Guid? LoteId { get; private set; }

    public EstadoLiquidacionPagos Estado { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public IReadOnlyList<LineaLiquidacionPagos> Lineas => _lineas;

    public static LiquidacionPagos Crear(Guid empresaId, int numero, DateOnly fecha, DateOnly hasta, Guid proveedorId, Guid? clienteId, FormaPagoLiquidacion forma,
        Guid? cuentaBancariaId, decimal aPagar, IReloj reloj, Guid? loteId = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new LiquidacionPagos(Guid.NewGuid(), empresaId, numero, fecha, hasta, proveedorId, clienteId, forma, cuentaBancariaId, loteId, reloj.AhoraUtc)
        {
            APagar = Redondeo.Dos(aPagar),
        };
    }

    public void AgregarLinea(TipoLineaLiquidacionPagos tipo, Guid documentoId, string documento, decimal importe, Guid? movimientoId, Guid? entregaId = null)
    {
        _lineas.Add(new LineaLiquidacionPagos(tipo, documentoId, documento, Redondeo.Dos(importe), movimientoId, entregaId));
        EntregasCanceladas = Redondeo.Dos(_lineas.Where(l => l.Tipo == TipoLineaLiquidacionPagos.EntregaCuenta).Sum(l => l.Importe));
        Compensado = Redondeo.Dos(_lineas.Where(l => l.Tipo == TipoLineaLiquidacionPagos.Compensacion).Sum(l => l.Importe));
        Liquido = Redondeo.Dos(APagar - EntregasCanceladas - Compensado);
    }

    /// <summary>El líquido va en esta remesa de transferencias (el pago se registra al liquidarla).</summary>
    public void IncluirEnRemesa(Guid remesaId)
    {
        RemesaId = remesaId;
        FormaPago = FormaPagoLiquidacion.Remesa;
    }

    /// <summary>Sin remesa (no se pudo generar): el líquido queda pendiente en las facturas.</summary>
    public void DejarPendiente() => FormaPago = FormaPagoLiquidacion.Pendiente;

    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado == EstadoLiquidacionPagos.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacionpagos.anulada", "La liquidación ya está anulada."));
        }

        Estado = EstadoLiquidacionPagos.Anulada;
        AnuladaEn = reloj.AhoraUtc;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }
}
