using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Compras.Dominio;

/// <summary>Estado de una solicitud de compra (petición interna de aprovisionamiento).</summary>
public enum EstadoSolicitud
{
    Borrador = 1,
    Aprobada = 2,
    Rechazada = 3,

    /// <summary>Ya se convirtió en pedido de compra.</summary>
    Convertida = 4,
}

/// <summary>Línea de una solicitud de compra.</summary>
public sealed class LineaSolicitud
{
    private LineaSolicitud() { Descripcion = null!; }

    internal LineaSolicitud(Guid id, string descripcion, decimal cantidad)
    {
        Id = id;
        Descripcion = descripcion;
        Cantidad = cantidad;
    }

    public Guid Id { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Cantidad { get; private set; }
}

/// <summary>
/// Solicitud de compra: petición interna (quién necesita qué) que, una vez aprobada, se convierte
/// en un pedido a proveedor. Primer eslabón de la cadena de compras.
/// </summary>
public sealed class SolicitudCompra : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTexto = 200;
    private readonly List<LineaSolicitud> _lineas = new();

    private SolicitudCompra(Guid id) : base(id, Guid.Empty) { }

    private SolicitudCompra(Guid id, Guid empresaId, string? proveedorSugerido, string? notas, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        ProveedorSugerido = proveedorSugerido;
        Notas = notas;
        Estado = EstadoSolicitud.Borrador;
        CreadoEn = ahora;
    }

    public string? ProveedorSugerido { get; private set; }

    public string? Notas { get; private set; }

    public EstadoSolicitud Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaSolicitud> Lineas => _lineas;

    public static Resultado<SolicitudCompra> Crear(Guid empresaId, string? proveedorSugerido, string? notas,
        IReadOnlyList<(string Descripcion, decimal Cantidad)> lineas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(lineas);
        if (lineas.Count == 0)
        {
            return Resultado.Fallo<SolicitudCompra>(Error.Validacion("solicitud.sin_lineas", "La solicitud necesita al menos una línea."));
        }

        foreach (var l in lineas)
        {
            if (string.IsNullOrWhiteSpace(l.Descripcion))
            {
                return Resultado.Fallo<SolicitudCompra>(Error.Validacion("solicitud.descripcion_vacia", "Cada línea necesita una descripción."));
            }

            if (l.Cantidad <= 0m)
            {
                return Resultado.Fallo<SolicitudCompra>(Error.Validacion("solicitud.cantidad_invalida", "La cantidad debe ser mayor que cero."));
            }
        }

        var solicitud = new SolicitudCompra(Guid.NewGuid(), empresaId, proveedorSugerido?.Trim(), notas?.Trim(), reloj.AhoraUtc);
        foreach (var l in lineas)
        {
            solicitud._lineas.Add(new LineaSolicitud(Guid.NewGuid(), l.Descripcion.Trim(), l.Cantidad));
        }

        return Resultado.Ok(solicitud);
    }

    public Resultado Aprobar()
    {
        if (Estado is not EstadoSolicitud.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("solicitud.estado", "Solo se puede aprobar una solicitud en borrador."));
        }

        Estado = EstadoSolicitud.Aprobada;
        return Resultado.Ok();
    }

    public Resultado Rechazar(string? motivo)
    {
        if (Estado is EstadoSolicitud.Convertida)
        {
            return Resultado.Fallo(Error.Conflicto("solicitud.convertida", "La solicitud ya se convirtió en pedido."));
        }

        Notas = string.IsNullOrWhiteSpace(motivo) ? Notas : motivo.Trim();
        Estado = EstadoSolicitud.Rechazada;
        return Resultado.Ok();
    }

    public Resultado MarcarConvertida()
    {
        if (Estado is not EstadoSolicitud.Aprobada)
        {
            return Resultado.Fallo(Error.Conflicto("solicitud.no_aprobada", "Aprueba la solicitud antes de convertirla en pedido."));
        }

        Estado = EstadoSolicitud.Convertida;
        return Resultado.Ok();
    }
}
