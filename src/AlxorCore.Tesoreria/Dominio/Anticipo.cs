using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Estado de un anticipo, derivado de lo aplicado (nunca se fija a mano).</summary>
public enum EstadoAnticipo
{
    Disponible = 1,
    Parcial = 2,
    Aplicado = 3,

    /// <summary>Registrado por error o devuelto al cliente: ya no se puede aplicar.</summary>
    Anulado = 4,
}

/// <summary>Aplicación de (parte de) un anticipo a una factura. Genera un cobro de esa factura.</summary>
public sealed class AplicacionAnticipo
{
    private AplicacionAnticipo()
    {
    }

    internal AplicacionAnticipo(Guid facturaId, decimal importe, DateOnly fecha, Guid movimientoId)
    {
        Id = Guid.NewGuid();
        FacturaId = facturaId;
        Importe = importe;
        Fecha = fecha;
        MovimientoId = movimientoId;
    }

    public Guid Id { get; private set; }

    public Guid FacturaId { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Cobro que generó la aplicación en la factura.</summary>
    public Guid MovimientoId { get; private set; }
}

/// <summary>
/// Anticipo (entrega a cuenta) de un cliente: dinero cobrado antes de facturar. Se aplica después a
/// una o varias facturas del mismo cliente; cada aplicación es un cobro de esa factura. Nunca se
/// aplica más de lo que queda disponible.
/// </summary>
public sealed class Anticipo : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaConcepto = 200;

    private readonly List<AplicacionAnticipo> _aplicaciones = [];

    private Anticipo(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
    }

    private Anticipo(Guid id, Guid empresaId, Guid clienteId, decimal importe, DateOnly fecha, string concepto, string? metodo, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ClienteId = clienteId;
        Importe = importe;
        Fecha = fecha;
        Concepto = concepto;
        Metodo = metodo;
        CreadoEn = ahora;
    }

    public Guid ClienteId { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Concepto { get; private set; }

    public string? Metodo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<AplicacionAnticipo> Aplicaciones => _aplicaciones;

    public decimal Aplicado => Redondeo.Dos(_aplicaciones.Sum(a => a.Importe));

    public decimal Disponible => Redondeo.Dos(Importe - Aplicado);

    /// <summary>Cuándo se anuló (anticipo registrado por error o devuelto); null si está vivo.</summary>
    public DateTimeOffset? AnuladoEn { get; private set; }

    public EstadoAnticipo Estado => AnuladoEn is not null ? EstadoAnticipo.Anulado
        : Aplicado == 0 ? EstadoAnticipo.Disponible : Disponible == 0 ? EstadoAnticipo.Aplicado : EstadoAnticipo.Parcial;

    /// <summary>Anula el anticipo: solo si no hay nada aplicado (si lo hay, anula antes esos cobros).</summary>
    public Resultado Anular(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (AnuladoEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("anticipo.ya_anulado", "El anticipo ya está anulado."));
        }

        if (Aplicado != 0m)
        {
            return Resultado.Fallo(Error.Conflicto("anticipo.aplicado",
                $"Hay {Redondeo.Formatear(Aplicado)} € aplicados a facturas: anula antes esos cobros (Cobros → Cobros de la factura)."));
        }

        AnuladoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public static Resultado<Anticipo> Registrar(
        Guid empresaId, Guid clienteId, decimal importe, DateOnly fecha, string? concepto, string? metodo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var total = Redondeo.Dos(importe);
        if (total <= 0)
        {
            return Resultado.Fallo<Anticipo>(Error.Validacion("anticipo.importe_invalido", "El importe del anticipo debe ser mayor que cero."));
        }

        var texto = (concepto ?? string.Empty).Trim();
        if (texto.Length == 0)
        {
            texto = "Entrega a cuenta";
        }

        if (texto.Length > LongitudMaximaConcepto)
        {
            return Resultado.Fallo<Anticipo>(Error.Validacion("anticipo.concepto_largo", "El concepto es demasiado largo."));
        }

        return Resultado.Ok(new Anticipo(Guid.NewGuid(), empresaId, clienteId, total, fecha, texto, metodo, reloj.AhoraUtc));
    }

    /// <summary>
    /// Comprueba si se puede aplicar <paramref name="importe"/> a una factura del cliente
    /// <paramref name="clienteFactura"/>, y devuelve el importe redondeado.
    /// </summary>
    public Resultado<decimal> ValidarAplicacion(Guid? clienteFactura, decimal importe)
    {
        if (AnuladoEn is not null)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("anticipo.anulado", "El anticipo está anulado."));
        }

        if (clienteFactura != ClienteId)
        {
            return Resultado.Fallo<decimal>(Error.Validacion("anticipo.otro_cliente", "El anticipo es de otro cliente: solo se aplica a sus facturas."));
        }

        var i = Redondeo.Dos(importe);
        if (i <= 0)
        {
            return Resultado.Fallo<decimal>(Error.Validacion("anticipo.importe_invalido", "El importe a aplicar debe ser mayor que cero."));
        }

        if (i > Disponible)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("anticipo.sin_saldo",
                $"El anticipo solo tiene {Redondeo.Formatear(Disponible)} € disponibles."));
        }

        return Resultado.Ok(i);
    }

    /// <summary>Anota una aplicación ya validada (el cobro de la factura se registra aparte).</summary>
    public void AnotarAplicacion(Guid facturaId, decimal importe, DateOnly fecha, Guid movimientoId) =>
        _aplicaciones.Add(new AplicacionAnticipo(facturaId, importe, fecha, movimientoId));
}
