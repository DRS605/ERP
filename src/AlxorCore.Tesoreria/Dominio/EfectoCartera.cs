using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Sentido de un efecto de cartera: a cobrar a un cliente o a pagar a un proveedor.</summary>
public enum SentidoCartera
{
    Cobro = 1,
    Pago = 2,
}

/// <summary>
/// Efecto (vencimiento) pendiente de cobro o de pago que no tiene factura ni gasto en ALXOR: típicamente,
/// la cartera viva que se trae de otro ERP al arrancar. Se cobra o se paga con movimientos de tesorería
/// como cualquier documento; su saldo contable llega aparte, en el asiento de apertura (430/400).
/// </summary>
public sealed class EfectoCartera : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudTexto = 200;

    private EfectoCartera(Guid id)
        : base(id, Guid.Empty)
    {
        TerceroNombre = null!;
        Documento = null!;
    }

    private EfectoCartera(Guid id, Guid empresaId, SentidoCartera sentido, Guid? terceroId, string terceroNombre, string documento,
        DateOnly? fechaDocumento, DateOnly vencimiento, decimal importe, string? origen, string? origenReferencia, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Sentido = sentido;
        TerceroId = terceroId;
        TerceroNombre = terceroNombre;
        Documento = documento;
        FechaDocumento = fechaDocumento;
        Vencimiento = vencimiento;
        Importe = importe;
        Origen = origen;
        OrigenReferencia = origenReferencia;
        CreadoEn = ahora;
    }

    public SentidoCartera Sentido { get; private set; }

    /// <summary>Cliente (cobro) o proveedor (pago) de Terceros, si se conoce.</summary>
    public Guid? TerceroId { get; private set; }

    public string TerceroNombre { get; private set; }

    /// <summary>Factura o documento que originó el efecto.</summary>
    public string Documento { get; private set; }

    public DateOnly? FechaDocumento { get; private set; }

    public DateOnly Vencimiento { get; private set; }

    /// <summary>Importe pendiente del efecto al darlo de alta.</summary>
    public decimal Importe { get; private set; }

    /// <summary>Sistema de procedencia (p. ej. «Hispatec»).</summary>
    public string? Origen { get; private set; }

    /// <summary>Identificador del efecto en el sistema de procedencia (evita darlo de alta dos veces).</summary>
    public string? OrigenReferencia { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<EfectoCartera> Crear(
        Guid empresaId, SentidoCartera sentido, Guid? terceroId, string? terceroNombre, string? documento, DateOnly? fechaDocumento,
        DateOnly vencimiento, decimal importe, string? origen, string? origenReferencia, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(sentido))
        {
            return Resultado.Fallo<EfectoCartera>(Error.Validacion("cartera.sentido", "El sentido debe ser Cobro o Pago."));
        }

        var nombre = terceroNombre?.Trim();
        var doc = documento?.Trim();
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Length > LongitudTexto || string.IsNullOrWhiteSpace(doc) || doc.Length > LongitudTexto)
        {
            return Resultado.Fallo<EfectoCartera>(Error.Validacion("cartera.datos", "Indica el tercero y el documento del efecto."));
        }

        if (importe <= 0m || Redondeo.Dos(importe) != importe)
        {
            return Resultado.Fallo<EfectoCartera>(Error.Validacion("cartera.importe", "El importe debe ser positivo, con dos decimales como máximo."));
        }

        return Resultado.Ok(new EfectoCartera(Guid.NewGuid(), empresaId, sentido, terceroId, nombre, doc, fechaDocumento, vencimiento, importe,
            string.IsNullOrWhiteSpace(origen) ? null : origen.Trim(), string.IsNullOrWhiteSpace(origenReferencia) ? null : origenReferencia.Trim(), reloj.AhoraUtc));
    }
}
