using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>
/// Línea de un albarán de venta: qué se entrega (referencia a la línea del pedido, si viene de uno) y a qué precio. El precio
/// puede estar <b>por fijar</b> (venta a resultas: se entrega con un precio estimado y se valora después, antes de facturar).
/// </summary>
public sealed class LineaAlbaranVenta
{
    /// <summary>Número de la línea en el documento (1, 2, 3…).</summary>
    public int Orden { get; internal set; }

    private LineaAlbaranVenta()
    {
        Descripcion = null!;
    }

    internal LineaAlbaranVenta(Guid id, NuevaLineaAlbaran datos)
    {
        Id = id;
        LineaPedidoId = datos.LineaPedidoId;
        ProductoId = datos.ProductoId;
        Descripcion = datos.Descripcion?.Trim() ?? string.Empty;
        Cantidad = Math.Round(datos.Cantidad, 3, MidpointRounding.AwayFromZero);
        PrecioUnitario = Math.Round(datos.PrecioUnitario ?? 0m, 4, MidpointRounding.AwayFromZero);
        PorcentajeDescuento = Redondeo.Dos(datos.PorcentajeDescuento);
        CodigoIva = string.IsNullOrWhiteSpace(datos.CodigoIva) ? "IVA21" : datos.CodigoIva.Trim();
        PrecioFijado = datos.PrecioUnitario is not null && !datos.PrecioEstimado;
    }

    public Guid Id { get; private set; }

    /// <summary>Línea del pedido que sirve (null en un albarán directo, sin pedido).</summary>
    public Guid? LineaPedidoId { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    /// <summary>Precio unitario: el definitivo o, si <see cref="PrecioFijado"/> es falso, el estimado (0 si no se conoce).</summary>
    public decimal PrecioUnitario { get; private set; }

    public decimal PorcentajeDescuento { get; private set; }

    public string CodigoIva { get; private set; } = "IVA21";

    /// <summary>Si el precio es el definitivo. Un albarán con precios por fijar no se factura hasta valorarlo.</summary>
    public bool PrecioFijado { get; private set; }

    /// <summary>Base de la línea (con el precio estimado mientras no se fije).</summary>
    public decimal Base => Redondeo.Dos(Cantidad * PrecioUnitario * (1m - PorcentajeDescuento / 100m));

    internal void Valorar(decimal precio, decimal? descuento)
    {
        PrecioUnitario = Math.Round(precio, 4, MidpointRounding.AwayFromZero);
        if (descuento is { } d)
        {
            PorcentajeDescuento = Redondeo.Dos(d);
        }

        PrecioFijado = true;
    }
}

/// <summary>Datos de una línea al crear un albarán. Sin precio (o con <c>PrecioEstimado</c>), la línea queda por valorar.</summary>
public sealed record NuevaLineaAlbaran(Guid? LineaPedidoId, Guid? ProductoId, string Descripcion, decimal Cantidad,
    decimal? PrecioUnitario = null, decimal PorcentajeDescuento = 0m, string? CodigoIva = null, bool PrecioEstimado = false);

/// <summary>Estado de un albarán de venta.</summary>
public enum EstadoAlbaranVenta
{
    /// <summary>Entregado con algún precio por fijar: hay que valorarlo antes de facturar.</summary>
    PendienteValorar,

    /// <summary>Valorado y pendiente de facturar.</summary>
    PendienteFacturar,

    Facturado,

    Anulado,
}

/// <summary>
/// Albarán de venta: el documento central de la venta, como en el sector hortofrutícola. Documenta qué, cuánto y a qué
/// precio se entregó al cliente, con pedido (actualiza lo servido) o directo. <b>La salida de existencias la hace el
/// albarán</b>; la factura que lo recoge ya no mueve stock. Se factura solo o junto con otros albaranes del cliente, y si
/// se entregó a precio por fijar se valora antes.
/// </summary>
public sealed class AlbaranVenta : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaAlbaranVenta> _lineas = new();

    private AlbaranVenta(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
    }

    private AlbaranVenta(Guid id, Guid empresaId, Guid? pedidoId, Guid clienteId, string clienteNombre, int numero, DateOnly fecha, string? referencia, string? serie, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        PedidoId = pedidoId;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Numero = numero;
        Fecha = fecha;
        Referencia = referencia;
        Serie = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        CreadoEn = ahora;
    }

    /// <summary>Pedido que sirve (null en un albarán directo).</summary>
    public Guid? PedidoId { get; private set; }

    /// <summary>Factura que lo recoge (null mientras está pendiente de facturar).</summary>
    public Guid? FacturaId { get; private set; }

    public string? Observaciones { get; private set; }

    public EstadoAlbaranVenta Estado => AnuladoEn is not null ? EstadoAlbaranVenta.Anulado
        : FacturaId is not null ? EstadoAlbaranVenta.Facturado
        : _lineas.All(l => l.PrecioFijado) ? EstadoAlbaranVenta.PendienteFacturar
        : EstadoAlbaranVenta.PendienteValorar;

    /// <summary>Base del albarán (con los precios estimados de las líneas por fijar).</summary>
    public decimal Base => Redondeo.Dos(_lineas.Sum(l => l.Base));

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string? Serie { get; private set; }

    public string NumeroCompleto => Serie is { Length: > 0 } ? $"{Serie}{Fecha.Year}/{Numero:D5}" : Numero.ToString(System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>Referencia libre del albarán (opcional).</summary>
    public string? Referencia { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    /// <summary>Líneas en su orden (la base de datos no garantiza el orden en que las devuelve).</summary>
    public IReadOnlyList<LineaAlbaranVenta> Lineas => _lineas.OrderBy(l => l.Orden).ToList().AsReadOnly();

    /// <summary>Añade una línea con el número siguiente.</summary>
    private void AgregarLinea(LineaAlbaranVenta linea)
    {
        linea.Orden = _lineas.Count + 1;
        _lineas.Add(linea);
    }

    /// <summary>Fecha de la anulación (la entrega no se hizo o se registró por error). Null si está vigente.</summary>
    public DateTimeOffset? AnuladoEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    /// <summary>Anula el albarán (su número queda usado). Las cantidades vuelven a quedar pendientes de servir en el pedido.</summary>
    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (AnuladoEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.anulado", "El albarán ya está anulado."));
        }

        if (FacturaId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.facturado", "El albarán está facturado: anula antes la factura o corrígela con una rectificativa."));
        }

        AnuladoEn = reloj.AhoraUtc;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        RegistrarEvento(new AlbaranVentaAnulado(Id, EmpresaId, NumeroCompleto, MotivoAnulacion, reloj.AhoraUtc));
        return Resultado.Ok();
    }

    /// <summary>Fija el precio de las líneas (valoración a posteriori). Solo antes de facturar.</summary>
    public Resultado Valorar(IReadOnlyList<(int Orden, decimal Precio, decimal? Descuento)> precios)
    {
        ArgumentNullException.ThrowIfNull(precios);
        if (AnuladoEn is not null || FacturaId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.no_valorable", "Solo se valora un albarán pendiente de facturar."));
        }

        foreach (var (orden, precio, descuento) in precios)
        {
            var linea = _lineas.SingleOrDefault(l => l.Orden == orden);
            if (linea is null)
            {
                return Resultado.Fallo(Error.Validacion("albaranventa.linea_desconocida", $"El albarán no tiene la línea {orden}."));
            }

            if (precio < 0m || descuento is < 0m or > 100m)
            {
                return Resultado.Fallo(Error.Validacion("albaranventa.precio_invalido", $"Precio o descuento no válido en la línea {orden}."));
            }
        }

        foreach (var (orden, precio, descuento) in precios)
        {
            _lineas.Single(l => l.Orden == orden).Valorar(precio, descuento);
        }

        return Resultado.Ok();
    }

    /// <summary>Si se puede facturar: vigente, sin factura y con todos los precios fijados.</summary>
    public Resultado PuedeFacturarse()
    {
        if (AnuladoEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.anulado", $"El albarán {NumeroCompleto} está anulado."));
        }

        if (FacturaId is not null)
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.facturado", $"El albarán {NumeroCompleto} ya está facturado."));
        }

        if (_lineas.Any(l => !l.PrecioFijado))
        {
            return Resultado.Fallo(Error.Conflicto("albaranventa.sin_valorar", $"El albarán {NumeroCompleto} tiene precios por fijar: valóralo antes de facturarlo."));
        }

        return Resultado.Ok();
    }

    /// <summary>Lo recoge una factura. Tiene que estar valorado.</summary>
    public Resultado Facturar(Guid facturaId)
    {
        var puede = PuedeFacturarse();
        if (puede.EsFallo)
        {
            return puede;
        }

        FacturaId = facturaId;
        return Resultado.Ok();
    }

    /// <summary>
    /// Si su mercancía salió del almacén al emitirlo (los albaranes anteriores a este cambio no la sacaban: la sacaba la
    /// factura). La factura que lo recoge solo mueve existencias si es falso.
    /// </summary>
    public bool StockDescontado { get; private set; }

    /// <summary>Se anuló la factura que lo recogía: vuelve a estar pendiente de facturar.</summary>
    public void LiberarFactura(Guid facturaId)
    {
        if (FacturaId == facturaId)
        {
            FacturaId = null;
        }
    }

    public static Resultado<AlbaranVenta> Crear(Guid empresaId, Guid? pedidoId, Guid clienteId, string clienteNombre, int numero, DateOnly fecha, string? referencia,
        IReadOnlyList<NuevaLineaAlbaran> lineas, IReloj reloj, string? serie = null, string? observaciones = null)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<AlbaranVenta>(Error.Validacion("albaranventa.sin_lineas", "El albarán necesita al menos una línea entregada."));
        }

        foreach (var l in lineas)
        {
            if (l.Cantidad <= 0m)
            {
                return Resultado.Fallo<AlbaranVenta>(Error.Validacion("albaranventa.cantidad", "La cantidad entregada debe ser mayor que cero."));
            }

            if (string.IsNullOrWhiteSpace(l.Descripcion))
            {
                return Resultado.Fallo<AlbaranVenta>(Error.Validacion("albaranventa.linea_sin_descripcion", "Cada línea necesita una descripción."));
            }

            if (l.PrecioUnitario is < 0m || l.PorcentajeDescuento is < 0m or > 100m)
            {
                return Resultado.Fallo<AlbaranVenta>(Error.Validacion("albaranventa.precio_invalido", "Precio o descuento no válido."));
            }
        }

        var albaran = new AlbaranVenta(Guid.NewGuid(), empresaId, pedidoId, clienteId, clienteNombre?.Trim() ?? string.Empty, numero, fecha, referencia?.Trim(), serie, reloj.AhoraUtc)
        {
            Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()[..Math.Min(observaciones.Trim().Length, 500)],
            StockDescontado = true,
        };
        foreach (var l in lineas)
        {
            albaran.AgregarLinea(new LineaAlbaranVenta(Guid.NewGuid(), l));
        }

        albaran.RegistrarEvento(new AlbaranVentaEmitido(albaran.Id, empresaId, pedidoId, clienteId, reloj.AhoraUtc));
        return Resultado.Ok(albaran);
    }
}
