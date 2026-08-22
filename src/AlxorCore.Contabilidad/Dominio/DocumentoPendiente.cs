using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Estado de un documento en la cola de contabilización.</summary>
public enum EstadoContabilizacion
{
    /// <summary>Pendiente de que el contable lo revise y contabilice.</summary>
    Pendiente = 1,

    /// <summary>Ya contabilizado (tiene su asiento/gasto).</summary>
    Contabilizado = 2,
}

/// <summary>
/// Documento a la espera de ser contabilizado. Recoge lo necesario para que el contable lo revise en
/// un panel único, ajuste la <b>fecha de registro</b> y lo contabilice. Referencia al documento de
/// origen (factura, gasto…) sin depender de su módulo.
/// </summary>
public sealed class DocumentoPendiente : RaizAgregadoEmpresa<Guid>
{
    private DocumentoPendiente(Guid id) : base(id, Guid.Empty)
    {
        OrigenTipo = null!;
        Referencia = null!;
        TerceroNombre = null!;
        CodigoIva = null!;
    }

    private DocumentoPendiente(Guid id, Guid empresaId, DocumentoContabilizable d, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Sentido = d.Sentido;
        OrigenTipo = d.OrigenTipo;
        OrigenId = d.OrigenId;
        Referencia = d.Referencia;
        TerceroId = d.TerceroId;
        TerceroNombre = d.TerceroNombre;
        FechaDocumento = d.FechaDocumento;
        FechaRegistro = d.FechaDocumento;
        BaseImponible = d.BaseImponible;
        CodigoIva = d.CodigoIva;
        CuotaIva = d.CuotaIva;
        PorcentajeIrpf = d.PorcentajeIrpf;
        RetencionIrpf = d.RetencionIrpf;
        Total = d.Total;
        ProductoId = d.ProductoId;
        Familia = d.Familia;
        TipoTercero = d.TipoTercero;
        Estado = EstadoContabilizacion.Pendiente;
        CreadoEn = ahora;
    }

    public SentidoContable Sentido { get; private set; }

    public string OrigenTipo { get; private set; }

    public Guid OrigenId { get; private set; }

    public string Referencia { get; private set; }

    public Guid? TerceroId { get; private set; }

    public string TerceroNombre { get; private set; }

    /// <summary>Fecha del documento (emisión / factura).</summary>
    public DateOnly FechaDocumento { get; private set; }

    /// <summary>Fecha con la que se registrará el asiento (editable por el contable; por defecto la del documento).</summary>
    public DateOnly FechaRegistro { get; private set; }

    public decimal BaseImponible { get; private set; }

    public string CodigoIva { get; private set; }

    public decimal CuotaIva { get; private set; }

    public decimal PorcentajeIrpf { get; private set; }

    public decimal RetencionIrpf { get; private set; }

    public decimal Total { get; private set; }

    public Guid? ProductoId { get; private set; }

    /// <summary>Familia del artículo (si aplica), para elegir la cuenta contable.</summary>
    public string? Familia { get; private set; }

    /// <summary>Tipo/categoría del tercero (si aplica), para elegir la cuenta contable.</summary>
    public string? TipoTercero { get; private set; }

    public EstadoContabilizacion Estado { get; private set; }

    public Guid? AsientoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static DocumentoPendiente Crear(Guid empresaId, DocumentoContabilizable d, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(d);
        return new DocumentoPendiente(Guid.NewGuid(), empresaId, d, ahora);
    }

    public Resultado CambiarFechaRegistro(DateOnly fecha)
    {
        if (Estado != EstadoContabilizacion.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("pendiente.ya_contabilizado", "El documento ya está contabilizado."));
        }

        FechaRegistro = fecha;
        return Resultado.Ok();
    }

    public Resultado MarcarContabilizado(Guid? asientoId)
    {
        if (Estado != EstadoContabilizacion.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("pendiente.ya_contabilizado", "El documento ya está contabilizado."));
        }

        Estado = EstadoContabilizacion.Contabilizado;
        AsientoId = asientoId;
        return Resultado.Ok();
    }
}
