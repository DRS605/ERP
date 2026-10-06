using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Documento en divisa revalorizado al cierre: lo pendiente, su valor en libros y su valor al tipo de cierre.</summary>
public sealed class LineaRevalorizacion
{
    private LineaRevalorizacion() { Referencia = null!; TerceroNombre = null!; Moneda = null!; }

    public LineaRevalorizacion(TipoDocumentoTesoreria tipoDocumento, Guid documentoId, string referencia, Guid? terceroId, string terceroNombre, string moneda,
        decimal pendienteDivisa, decimal valorLibros, decimal tasaCierre, decimal valorCierre)
    {
        Id = Guid.NewGuid();
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        Referencia = referencia;
        TerceroId = terceroId;
        TerceroNombre = terceroNombre;
        Moneda = moneda;
        PendienteDivisa = pendienteDivisa;
        ValorLibros = valorLibros;
        TasaCierre = tasaCierre;
        ValorCierre = valorCierre;
    }

    public Guid Id { get; private set; }

    /// <summary>Factura (a cobrar) o Gasto (a pagar).</summary>
    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    public string Referencia { get; private set; }

    public Guid? TerceroId { get; private set; }

    public string TerceroNombre { get; private set; }

    public string Moneda { get; private set; }

    public decimal PendienteDivisa { get; private set; }

    /// <summary>Lo pendiente en euros al tipo del documento.</summary>
    public decimal ValorLibros { get; private set; }

    public decimal TasaCierre { get; private set; }

    /// <summary>Lo pendiente en euros al tipo de cierre.</summary>
    public decimal ValorCierre { get; private set; }

    /// <summary>Valor de cierre − valor en libros: en un cobro, positiva es ganancia; en un pago, pérdida.</summary>
    public decimal Diferencia => ValorCierre - ValorLibros;

    /// <summary>¿La diferencia es un ingreso (768)? Un cobro que vale más, o un pago que vale menos.</summary>
    public bool EsGanancia => (TipoDocumento == TipoDocumentoTesoreria.Factura) == (Diferencia > 0m);
}

/// <summary>
/// Revalorización de los saldos en divisa al cierre de un ejercicio: cada factura o gasto en divisa con algo pendiente se
/// valora al tipo de cambio del 31/12, la diferencia va a 668/768 contra el cliente o el proveedor y se revierte el 1/1
/// del año siguiente (el cobro o pago posterior calcula su diferencia contra el tipo del documento).
/// </summary>
public sealed class RevalorizacionDivisa : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaRevalorizacion> _lineas = [];

    private RevalorizacionDivisa(Guid id) : base(id, Guid.Empty) { }

    public RevalorizacionDivisa(Guid empresaId, int ejercicio, DateTimeOffset ahora, IEnumerable<LineaRevalorizacion> lineas) : base(Guid.NewGuid(), empresaId)
    {
        Ejercicio = ejercicio;
        CreadaEn = ahora;
        _lineas.AddRange(lineas);
    }

    public int Ejercicio { get; private set; }

    public DateOnly FechaCierre => new(Ejercicio, 12, 31);

    public DateOnly FechaReversion => new(Ejercicio + 1, 1, 1);

    public bool Anulada { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public IReadOnlyList<LineaRevalorizacion> Lineas => _lineas;

    public void Anular() => Anulada = true;
}
