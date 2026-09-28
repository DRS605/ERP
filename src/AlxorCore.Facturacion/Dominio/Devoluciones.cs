using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Estado de una devolución de venta.</summary>
public enum EstadoDevolucionVenta
{
    /// <summary>Registrada: la mercancía volvió (y, si se indicó, entró en el almacén); falta abonarla.</summary>
    Registrada,

    /// <summary>Abonada en la factura del albarán, con una rectificativa, o cerrada sin abono.</summary>
    Abonada,

    Anulada,
}

/// <summary>Cómo se abona una devolución, como <c>TipoFormaDevolucion</c> de Hispatec.</summary>
public enum FormaAbonoDevolucion
{
    /// <summary>El albarán aún no estaba facturado: su factura ya sale con lo devuelto descontado.</summary>
    EnFacturaDelAlbaran,

    /// <summary>El albarán estaba facturado: rectificativa de la factura con las cantidades devueltas descontadas.</summary>
    Rectificativa,

    /// <summary>Sin abono (mercancía repuesta o devolución no aceptada económicamente).</summary>
    SinAbono,
}

/// <summary>Línea devuelta: una línea del albarán de origen, la cantidad y si vuelve al almacén.</summary>
public sealed class LineaDevolucionVenta
{
    private LineaDevolucionVenta()
    {
        Descripcion = null!;
        CodigoIva = null!;
    }

    internal LineaDevolucionVenta(LineaAlbaranVenta origen, decimal cantidad, bool reingresa)
    {
        Id = Guid.NewGuid();
        OrdenAlbaran = origen.Orden;
        ProductoId = origen.ProductoId;
        Descripcion = origen.Descripcion;
        Cantidad = Math.Round(cantidad, 3, MidpointRounding.AwayFromZero);
        PrecioUnitario = origen.PrecioUnitario;
        PorcentajeDescuento = origen.PorcentajeDescuento;
        CodigoIva = origen.CodigoIva;
        Reingresa = reingresa && origen.ProductoId is not null;
    }

    public Guid Id { get; private set; }

    /// <summary>Número de la línea del albarán que se devuelve.</summary>
    public int OrdenAlbaran { get; private set; }

    public Guid? ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal PrecioUnitario { get; private set; }

    public decimal PorcentajeDescuento { get; private set; }

    public string CodigoIva { get; private set; }

    /// <summary>Si la mercancía vuelve al almacén (falso: merma, destruida en destino).</summary>
    public bool Reingresa { get; private set; }

    /// <summary>Importe devuelto de la línea (sin impuestos).</summary>
    public decimal Base => Redondeo.Dos(Cantidad * PrecioUnitario * (1m - PorcentajeDescuento / 100m));
}

/// <summary>
/// Devolución de venta sobre un albarán, como <c>DevolucionesVenta</c> y la gestión de devoluciones de mercancía de Hispatec:
/// qué líneas y cuánto vuelve (nunca más de lo entregado, sumando las devoluciones anteriores), si entra en el almacén, y
/// cómo se abona: en la factura del albarán si aún no se había facturado o con una rectificativa si ya lo estaba.
/// </summary>
public sealed class DevolucionVenta : RaizAgregadoEmpresa<Guid>
{
    public const string Prefijo = "DV";

    private readonly List<LineaDevolucionVenta> _lineas = new();

    private DevolucionVenta(Guid id)
        : base(id, Guid.Empty)
    {
        AlbaranNumero = null!;
        ClienteNombre = null!;
        Motivo = null!;
    }

    private DevolucionVenta(Guid id, Guid empresaId, AlbaranVenta albaran, int numero, DateOnly fecha, string motivo, Guid? reclamacionId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        AlbaranId = albaran.Id;
        AlbaranNumero = albaran.NumeroCompleto;
        ClienteId = albaran.ClienteId;
        ClienteNombre = albaran.ClienteNombre;
        Numero = numero;
        Fecha = fecha;
        Motivo = motivo;
        ReclamacionId = reclamacionId;
        CreadoEn = ahora;
    }

    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string NumeroCompleto => $"{Prefijo}{Fecha.Year}/{Numero:D5}";

    public Guid AlbaranId { get; private set; }

    public string AlbaranNumero { get; private set; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public string Motivo { get; private set; }

    /// <summary>Reclamación que la origina (opcional).</summary>
    public Guid? ReclamacionId { get; private set; }

    public EstadoDevolucionVenta Estado { get; private set; }

    public FormaAbonoDevolucion? FormaAbono { get; private set; }

    /// <summary>Factura que la abona: la del albarán (con lo devuelto descontado) o la rectificativa.</summary>
    public Guid? FacturaAbonoId { get; private set; }

    public DateTimeOffset? AbonadaEn { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaDevolucionVenta> Lineas => _lineas.OrderBy(l => l.OrdenAlbaran).ToList().AsReadOnly();

    public decimal Base => Redondeo.Dos(_lineas.Sum(l => l.Base));

    /// <summary>
    /// Registra la devolución. <paramref name="yaDevuelto"/> es lo devuelto antes de cada línea del albarán (en devoluciones
    /// no anuladas): lo devuelto no puede superar lo entregado.
    /// </summary>
    public static Resultado<DevolucionVenta> Crear(Guid empresaId, AlbaranVenta albaran, int numero, DateOnly fecha, string? motivo,
        IReadOnlyList<(int OrdenAlbaran, decimal Cantidad, bool Reingresa)> lineas, IReadOnlyDictionary<int, decimal> yaDevuelto, IReloj reloj, Guid? reclamacionId = null)
    {
        ArgumentNullException.ThrowIfNull(albaran);
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(yaDevuelto);
        ArgumentNullException.ThrowIfNull(reloj);
        if (albaran.Estado == EstadoAlbaranVenta.Anulado)
        {
            return Resultado.Fallo<DevolucionVenta>(Error.Conflicto("devolucion.albaran_anulado", "El albarán está anulado: no hay nada que devolver."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.sin_motivo", "Indica el motivo de la devolución."));
        }

        if (fecha < albaran.Fecha)
        {
            return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.fecha", "La devolución no puede ser anterior al albarán."));
        }

        if (lineas.Count == 0 || lineas.Select(l => l.OrdenAlbaran).Distinct().Count() != lineas.Count)
        {
            return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.sin_lineas", "Indica cada línea devuelta una sola vez."));
        }

        var d = new DevolucionVenta(Guid.NewGuid(), empresaId, albaran, numero, fecha, motivo.Trim()[..Math.Min(motivo.Trim().Length, 300)], reclamacionId, reloj.AhoraUtc);
        foreach (var (orden, cantidad, reingresa) in lineas)
        {
            var origen = albaran.Lineas.SingleOrDefault(l => l.Orden == orden);
            if (origen is null)
            {
                return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.linea_desconocida", $"El albarán no tiene la línea {orden}."));
            }

            if (cantidad <= 0m)
            {
                return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.cantidad", "La cantidad devuelta debe ser mayor que cero."));
            }

            var previo = yaDevuelto.TryGetValue(orden, out var p) ? p : 0m;
            if (previo + cantidad > origen.Cantidad)
            {
                return Resultado.Fallo<DevolucionVenta>(Error.Validacion("devolucion.excede",
                    $"Línea {orden} ({origen.Descripcion}): se entregaron {Redondeo.Formatear(origen.Cantidad, 3)} y ya se han devuelto {Redondeo.Formatear(previo, 3)}; no se pueden devolver {Redondeo.Formatear(cantidad, 3)} más."));
            }

            d._lineas.Add(new LineaDevolucionVenta(origen, cantidad, reingresa));
        }

        d.Estado = EstadoDevolucionVenta.Registrada;
        return Resultado.Ok(d);
    }

    public Resultado Abonar(FormaAbonoDevolucion forma, Guid? facturaId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoDevolucionVenta.Registrada)
        {
            return Resultado.Fallo(Error.Conflicto("devolucion.no_pendiente", $"La devolución {NumeroCompleto} ya está {(Estado == EstadoDevolucionVenta.Anulada ? "anulada" : "abonada")}."));
        }

        if (forma != FormaAbonoDevolucion.SinAbono && facturaId is null)
        {
            throw new InvalidOperationException("Un abono necesita su factura.");
        }

        Estado = EstadoDevolucionVenta.Abonada;
        FormaAbono = forma;
        FacturaAbonoId = facturaId;
        AbonadaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Se anuló la factura del albarán que la descontaba: vuelve a estar pendiente de abono.</summary>
    public void LiberarFactura(Guid facturaId)
    {
        if (FacturaAbonoId == facturaId && FormaAbono == FormaAbonoDevolucion.EnFacturaDelAlbaran)
        {
            Estado = EstadoDevolucionVenta.Registrada;
            FormaAbono = null;
            FacturaAbonoId = null;
            AbonadaEn = null;
        }
    }

    /// <summary>Anula una devolución aún no abonada (la mercancía que reingresó vuelve a salir).</summary>
    public Resultado Anular(string? motivo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoDevolucionVenta.Registrada)
        {
            return Resultado.Fallo(Error.Conflicto("devolucion.no_anulable",
                Estado == EstadoDevolucionVenta.Anulada ? "La devolución ya está anulada." : "La devolución está abonada: corrige el abono con otra rectificativa."));
        }

        Estado = EstadoDevolucionVenta.Anulada;
        AnuladaEn = reloj.AhoraUtc;
        MotivoAnulacion = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        return Resultado.Ok();
    }
}

/// <summary>Concepto de reclamación (maestro «Conceptos para reclamaciones» de Hispatec): calidad, calibre, retraso, rotura…</summary>
public sealed class ConceptoReclamacion : RaizAgregadoEmpresa<Guid>
{
    private ConceptoReclamacion(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private ConceptoReclamacion(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Codigo = string.Empty;
        Nombre = string.Empty;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<ConceptoReclamacion> Crear(Guid empresaId, string? codigo, string? nombre)
    {
        var c = new ConceptoReclamacion(Guid.NewGuid(), empresaId);
        var r = c.Cambiar(codigo, nombre);
        return r.EsFallo ? Resultado.Fallo<ConceptoReclamacion>(r.Error) : Resultado.Ok(c);
    }

    public Resultado Cambiar(string? codigo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > 20 || string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 100)
        {
            return Resultado.Fallo(Error.Validacion("reclamacion.concepto", "El concepto necesita un código (hasta 20) y un nombre (hasta 100)."));
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        Nombre = nombre.Trim();
        return Resultado.Ok();
    }

    public void DarDeBaja() => Activo = false;

    public void Reactivar() => Activo = true;
}

public enum EstadoReclamacion
{
    Abierta,
    EnTramite,
    Resuelta,
    Anulada,
}

public enum ResolucionReclamacion
{
    Aceptada,
    AceptadaParcial,
    Rechazada,
}

/// <summary>
/// Reclamación sobre mercancía servida, como «Reclamaciones sobre ventas» de Hispatec: del cliente, sobre un albarán o una
/// factura, con su concepto, lo reclamado y su tramitación hasta la resolución (aceptada, parcial o rechazada) con lo
/// reconocido y, si lo hubo, la devolución o la rectificativa que la compensa.
/// </summary>
public sealed class ReclamacionVenta : RaizAgregadoEmpresa<Guid>
{
    public const string Prefijo = "RC";

    private ReclamacionVenta(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
        Descripcion = null!;
    }

    private ReclamacionVenta(Guid id, Guid empresaId, int numero, DateOnly fecha, Guid clienteId, string clienteNombre, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Numero = numero;
        Fecha = fecha;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Descripcion = string.Empty;
        CreadoEn = ahora;
    }

    public int Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string NumeroCompleto => $"{Prefijo}{Fecha.Year}/{Numero:D5}";

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public Guid? AlbaranId { get; private set; }

    public Guid? FacturaId { get; private set; }

    public Guid ConceptoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal? ImporteReclamado { get; private set; }

    public string? Responsable { get; private set; }

    public EstadoReclamacion Estado { get; private set; }

    public ResolucionReclamacion? Resolucion { get; private set; }

    public decimal? ImporteReconocido { get; private set; }

    public string? TextoResolucion { get; private set; }

    public DateTimeOffset? ResueltaEn { get; private set; }

    /// <summary>Devolución de mercancía con que se resolvió (opcional).</summary>
    public Guid? DevolucionId { get; private set; }

    /// <summary>Rectificativa con que se abonó (opcional).</summary>
    public Guid? FacturaAbonoId { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<ReclamacionVenta> Crear(Guid empresaId, int numero, DateOnly fecha, Guid clienteId, string clienteNombre, Guid? albaranId, Guid? facturaId,
        Guid conceptoId, string? descripcion, decimal? importeReclamado, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var r = new ReclamacionVenta(Guid.NewGuid(), empresaId, numero, fecha, clienteId, clienteNombre, reloj.AhoraUtc);
        var c = r.Cambiar(albaranId, facturaId, conceptoId, descripcion, importeReclamado);
        return c.EsFallo ? Resultado.Fallo<ReclamacionVenta>(c.Error) : Resultado.Ok(r);
    }

    public Resultado Cambiar(Guid? albaranId, Guid? facturaId, Guid conceptoId, string? descripcion, decimal? importeReclamado)
    {
        if (Estado is EstadoReclamacion.Resuelta or EstadoReclamacion.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("reclamacion.cerrada", "La reclamación está cerrada: reábrela para cambiarla."));
        }

        if (string.IsNullOrWhiteSpace(descripcion))
        {
            return Resultado.Fallo(Error.Validacion("reclamacion.sin_descripcion", "Describe qué reclama el cliente."));
        }

        if (importeReclamado is < 0m)
        {
            return Resultado.Fallo(Error.Validacion("reclamacion.importe", "El importe reclamado no puede ser negativo."));
        }

        AlbaranId = albaranId;
        FacturaId = facturaId;
        ConceptoId = conceptoId;
        Descripcion = descripcion.Trim()[..Math.Min(descripcion.Trim().Length, 1000)];
        ImporteReclamado = importeReclamado is { } i ? Redondeo.Dos(i) : null;
        return Resultado.Ok();
    }

    public Resultado Tramitar(string? responsable)
    {
        if (Estado is not (EstadoReclamacion.Abierta or EstadoReclamacion.EnTramite))
        {
            return Resultado.Fallo(Error.Conflicto("reclamacion.cerrada", "Solo se tramita una reclamación abierta."));
        }

        Estado = EstadoReclamacion.EnTramite;
        Responsable = string.IsNullOrWhiteSpace(responsable) ? Responsable : responsable.Trim()[..Math.Min(responsable.Trim().Length, 100)];
        return Resultado.Ok();
    }

    public Resultado Resolver(ResolucionReclamacion resolucion, decimal? importeReconocido, string? texto, Guid? devolucionId, Guid? facturaAbonoId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado is EstadoReclamacion.Resuelta or EstadoReclamacion.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("reclamacion.cerrada", "La reclamación ya está cerrada."));
        }

        if (string.IsNullOrWhiteSpace(texto))
        {
            return Resultado.Fallo(Error.Validacion("reclamacion.sin_resolucion", "Explica la resolución."));
        }

        if (importeReconocido is < 0m || (resolucion == ResolucionReclamacion.Rechazada && importeReconocido is > 0m))
        {
            return Resultado.Fallo(Error.Validacion("reclamacion.importe", "Lo reconocido no puede ser negativo, y una reclamación rechazada no reconoce importe."));
        }

        Estado = EstadoReclamacion.Resuelta;
        Resolucion = resolucion;
        ImporteReconocido = resolucion == ResolucionReclamacion.Rechazada ? 0m : importeReconocido is { } i ? Redondeo.Dos(i) : null;
        TextoResolucion = texto.Trim()[..Math.Min(texto.Trim().Length, 1000)];
        DevolucionId = devolucionId;
        FacturaAbonoId = facturaAbonoId;
        ResueltaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Reabrir()
    {
        if (Estado != EstadoReclamacion.Resuelta)
        {
            return Resultado.Fallo(Error.Conflicto("reclamacion.no_resuelta", "Solo se reabre una reclamación resuelta."));
        }

        Estado = EstadoReclamacion.EnTramite;
        Resolucion = null;
        ImporteReconocido = null;
        TextoResolucion = null;
        ResueltaEn = null;
        DevolucionId = null;
        FacturaAbonoId = null;
        return Resultado.Ok();
    }

    public Resultado Anular(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado == EstadoReclamacion.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("reclamacion.anulada", "La reclamación ya está anulada."));
        }

        Estado = EstadoReclamacion.Anulada;
        AnuladaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }
}
