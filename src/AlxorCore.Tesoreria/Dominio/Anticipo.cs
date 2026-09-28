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

    internal AplicacionAnticipo(Guid facturaId, decimal importe, DateOnly fecha, Guid? movimientoId, decimal? baseAplicada = null)
    {
        Id = Guid.NewGuid();
        FacturaId = facturaId;
        Importe = importe;
        Fecha = fecha;
        MovimientoId = movimientoId;
        Base = baseAplicada;
    }

    public Guid Id { get; private set; }

    public Guid FacturaId { get; private set; }

    public decimal Importe { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Cobro que generó la aplicación en la factura.</summary>
    /// <summary>Cobro de la factura con el anticipo; null si el anticipo está facturado y se descuenta en la factura final.</summary>
    public Guid? MovimientoId { get; private set; }

    /// <summary>Base descontada en la factura final (anticipos facturados).</summary>
    public decimal? Base { get; private set; }
}

/// <summary>Factura del anticipo: el IVA se devenga al cobrarlo (art. 75.2 LIVA) y se descuenta en la factura final.</summary>
public sealed record DatosFacturaAnticipo(Guid FacturaId, string Numero, decimal Base, string CodigoIva);

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

    /// <summary>Factura del anticipo (null en los anticipos sin factura, que se aplican como cobro de la factura final).</summary>
    public Guid? FacturaId { get; private set; }

    public string? FacturaNumero { get; private set; }

    /// <summary>Base imponible de la factura del anticipo.</summary>
    public decimal? BaseFacturada { get; private set; }

    public string? CodigoIva { get; private set; }

    public bool Facturado => FacturaId is not null;

    /// <summary>Base que queda por descontar en facturas finales.</summary>
    public decimal DisponibleBase => Facturado ? Redondeo.Dos(BaseFacturada!.Value - _aplicaciones.Sum(a => a.Base ?? 0m)) : 0m;

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

        if (Facturado)
        {
            return Resultado.Fallo(Error.Conflicto("anticipo.facturado",
                $"El anticipo tiene la factura {FacturaNumero}: para devolverlo, rectifica esa factura y registra la devolución del cobro."));
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
        Guid empresaId, Guid clienteId, decimal importe, DateOnly fecha, string? concepto, string? metodo, IReloj reloj, DatosFacturaAnticipo? factura = null)
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

        var anticipo = new Anticipo(Guid.NewGuid(), empresaId, clienteId, total, fecha, texto, metodo, reloj.AhoraUtc);
        if (factura is not null)
        {
            if (factura.Base <= 0m || factura.Base > total)
            {
                return Resultado.Fallo<Anticipo>(Error.Validacion("anticipo.base_invalida", "La base de la factura del anticipo no es válida."));
            }

            anticipo.FacturaId = factura.FacturaId;
            anticipo.FacturaNumero = factura.Numero;
            anticipo.BaseFacturada = Redondeo.Dos(factura.Base);
            anticipo.CodigoIva = factura.CodigoIva;
        }

        return Resultado.Ok(anticipo);
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

        if (Facturado)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("anticipo.facturado",
                $"El anticipo ya tiene la factura {FacturaNumero} con su IVA: se descuenta en la factura final (línea de deducción), no como cobro."));
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

    /// <summary>
    /// Base que se puede descontar de este anticipo facturado en una factura del cliente (lo pedido o todo lo que queda),
    /// o un error si no se puede.
    /// </summary>
    public Resultado<decimal> BaseDescontable(Guid? clienteFactura, decimal? baseSolicitada)
    {
        if (!Facturado)
        {
            return Resultado.Fallo<decimal>(Error.Validacion("anticipo.sin_factura", "El anticipo no tiene factura: se aplica como cobro de la factura final."));
        }

        if (AnuladoEn is not null)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("anticipo.anulado", "El anticipo está anulado."));
        }

        if (clienteFactura != ClienteId)
        {
            return Resultado.Fallo<decimal>(Error.Validacion("anticipo.otro_cliente", "El anticipo es de otro cliente: solo se descuenta en sus facturas."));
        }

        var b = Redondeo.Dos(baseSolicitada ?? DisponibleBase);
        if (b <= 0m || b > DisponibleBase)
        {
            return Resultado.Fallo<decimal>(Error.Conflicto("anticipo.sin_saldo", $"Del anticipo {FacturaNumero} quedan {Redondeo.Formatear(DisponibleBase)} € de base por descontar."));
        }

        return Resultado.Ok(b);
    }

    /// <summary>Anota el descuento del anticipo en una factura final (sin movimiento de dinero: la factura ya va neta).</summary>
    public Resultado AnotarDescuento(Guid facturaId, decimal baseDescontada, decimal importe, DateOnly fecha)
    {
        var b = BaseDescontable(ClienteId, baseDescontada);
        if (b.EsFallo)
        {
            return Resultado.Fallo(b.Error);
        }

        _aplicaciones.Add(new AplicacionAnticipo(facturaId, Math.Min(Redondeo.Dos(importe), Disponible), fecha, null, b.Valor));
        return Resultado.Ok();
    }

    /// <summary>Su factura se ha anulado: el anticipo queda anulado (solo si no se ha descontado en ninguna factura).</summary>
    public Resultado AnularPorFactura(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Aplicado != 0m)
        {
            return Resultado.Fallo(Error.Conflicto("anticipo.descontado",
                $"El anticipo de la factura {FacturaNumero} ya se ha descontado en otra factura: anula antes esa factura."));
        }

        AnuladoEn ??= reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Deshace los descuentos en una factura final que se ha anulado: el anticipo vuelve a estar disponible.</summary>
    /// <remarks>Las aplicaciones no se borran (la tabla es de solo inserción): se anota la contraria.</remarks>
    public bool RevertirDescuentos(Guid facturaId, DateOnly fecha)
    {
        var descuentos = _aplicaciones.Where(a => a.FacturaId == facturaId && a.MovimientoId is null).ToList();
        var importe = Redondeo.Dos(descuentos.Sum(a => a.Importe));
        var baseNeta = Redondeo.Dos(descuentos.Sum(a => a.Base ?? 0m));
        if (importe == 0m && baseNeta == 0m)
        {
            return false;
        }

        _aplicaciones.Add(new AplicacionAnticipo(facturaId, -importe, fecha, null, -baseNeta));
        return true;
    }
}
