using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>
/// Póliza de seguro de crédito (Crédito y Caución, Solunion, Coface, Cesce…): qué porcentaje del impagado cubre, en
/// cuántos días desde el vencimiento hay que avisar de un impago y si se vende a crédito sin clasificación.
/// </summary>
public sealed class PolizaSeguroCredito : RaizAgregadoEmpresa<Guid>
{
    private PolizaSeguroCredito(Guid id)
        : base(id, Guid.Empty)
    {
        Aseguradora = null!;
        NumeroPoliza = null!;
    }

    private PolizaSeguroCredito(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Aseguradora = string.Empty;
        NumeroPoliza = string.Empty;
        Activa = true;
    }

    public string Aseguradora { get; private set; }

    public string NumeroPoliza { get; private set; }

    /// <summary>Porcentaje del crédito asegurado que indemniza la aseguradora (p. ej. 85 %).</summary>
    public decimal PorcentajeCobertura { get; private set; }

    /// <summary>Días desde el vencimiento para comunicar el impago (aviso de siniestro o de prórroga).</summary>
    public int PlazoAvisoDias { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    /// <summary>Si es cierto, no se factura ni se confirma un pedido a crédito sin cobertura (salvo con permiso para forzar el riesgo).</summary>
    public bool BloquearSinCobertura { get; private set; }

    public bool Activa { get; private set; }

    public bool VigenteEl(DateOnly dia) => Activa && Desde <= dia && (Hasta is null || Hasta >= dia);

    public static Resultado<PolizaSeguroCredito> Crear(Guid empresaId, string? aseguradora, string? numero, decimal cobertura, int plazoAviso, DateOnly desde,
        DateOnly? hasta, bool bloquear)
    {
        var p = new PolizaSeguroCredito(Guid.NewGuid(), empresaId);
        var r = p.Cambiar(aseguradora, numero, cobertura, plazoAviso, desde, hasta, bloquear, true);
        return r.EsFallo ? Resultado.Fallo<PolizaSeguroCredito>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Cambiar(string? aseguradora, string? numero, decimal cobertura, int plazoAviso, DateOnly desde, DateOnly? hasta, bool bloquear, bool activa)
    {
        var a = aseguradora?.Trim();
        if (string.IsNullOrEmpty(a) || a.Length > 120)
        {
            return Resultado.Fallo(Error.Validacion("seguro.aseguradora", "Indica la aseguradora (hasta 120 caracteres)."));
        }

        var n = (numero ?? string.Empty).Trim();
        if (n.Length > 40)
        {
            return Resultado.Fallo(Error.Validacion("seguro.poliza", "El número de póliza admite hasta 40 caracteres."));
        }

        if (cobertura is <= 0m or > 100m)
        {
            return Resultado.Fallo(Error.Validacion("seguro.cobertura", "La cobertura va de 1 a 100 %."));
        }

        if (plazoAviso is < 1 or > 365)
        {
            return Resultado.Fallo(Error.Validacion("seguro.plazo", "El plazo de aviso va de 1 a 365 días."));
        }

        if (hasta is { } h && h < desde)
        {
            return Resultado.Fallo(Error.Validacion("seguro.fechas", "La póliza no puede acabar antes de empezar."));
        }

        Aseguradora = a;
        NumeroPoliza = n;
        PorcentajeCobertura = Redondeo.Dos(cobertura);
        PlazoAvisoDias = plazoAviso;
        Desde = desde;
        Hasta = hasta;
        BloquearSinCobertura = bloquear;
        Activa = activa;
        return Resultado.Ok();
    }
}

/// <summary>Estado de la clasificación (límite de crédito) de un cliente en la póliza.</summary>
public enum EstadoClasificacion
{
    Solicitada = 1,
    Concedida = 2,

    /// <summary>Concedida por menos de lo solicitado o rebajada después.</summary>
    Reducida = 3,
    Denegada = 4,

    /// <summary>Retirada por la aseguradora: las ventas desde la fecha de efecto ya no están cubiertas.</summary>
    Anulada = 5,
}

/// <summary>Un cambio en la clasificación de un cliente (lo que comunicó la aseguradora).</summary>
public sealed class CambioClasificacion
{
    private CambioClasificacion()
    {
    }

    internal CambioClasificacion(DateOnly fecha, EstadoClasificacion estado, decimal concedido, string? nota, DateTimeOffset ahora)
    {
        Id = Guid.NewGuid();
        Fecha = fecha;
        Estado = estado;
        Concedido = concedido;
        Nota = nota;
        RegistradoEn = ahora;
    }

    public Guid Id { get; private set; }

    public DateOnly Fecha { get; private set; }

    public EstadoClasificacion Estado { get; private set; }

    public decimal Concedido { get; private set; }

    public string? Nota { get; private set; }

    public DateTimeOffset RegistradoEn { get; private set; }
}

/// <summary>
/// Clasificación de un cliente en la póliza: lo solicitado, lo concedido (límite asegurado) y su historial. Las
/// ventas por encima de lo concedido, o sin clasificación vigente, no están aseguradas.
/// </summary>
public sealed class ClasificacionSeguro : RaizAgregadoEmpresa<Guid>
{
    private readonly List<CambioClasificacion> _historial = [];

    private ClasificacionSeguro(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
    }

    private ClasificacionSeguro(Guid id, Guid empresaId, Guid polizaId, Guid clienteId, string clienteNombre)
        : base(id, empresaId)
    {
        PolizaId = polizaId;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
    }

    public Guid PolizaId { get; private set; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    /// <summary>Referencia de la aseguradora (número de clasificación o de deudor).</summary>
    public string? Referencia { get; private set; }

    public decimal Solicitado { get; private set; }

    public decimal Concedido { get; private set; }

    public EstadoClasificacion Estado { get; private set; }

    /// <summary>Desde cuándo rige el estado actual.</summary>
    public DateOnly Efecto { get; private set; }

    /// <summary>Hasta cuándo rige (clasificaciones temporales), si lo dijo la aseguradora.</summary>
    public DateOnly? Vence { get; private set; }

    public IReadOnlyList<CambioClasificacion> Historial => _historial;

    /// <summary>Lo concedido el día indicado (0 si no estaba vigente).</summary>
    public decimal ConcedidoEl(DateOnly dia) =>
        Estado is EstadoClasificacion.Concedida or EstadoClasificacion.Reducida && Efecto <= dia && (Vence is null || Vence >= dia) ? Concedido : 0m;

    public static Resultado<ClasificacionSeguro> Solicitar(Guid empresaId, Guid polizaId, Guid clienteId, string clienteNombre, decimal solicitado, string? referencia,
        DateOnly fecha, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (solicitado <= 0m)
        {
            return Resultado.Fallo<ClasificacionSeguro>(Error.Validacion("seguro.solicitado", "Indica el importe solicitado."));
        }

        var c = new ClasificacionSeguro(Guid.NewGuid(), empresaId, polizaId, clienteId, clienteNombre)
        {
            Solicitado = Redondeo.Dos(solicitado),
            Referencia = Texto(referencia, 40),
        };
        c.Aplicar(fecha, EstadoClasificacion.Solicitada, 0m, null, null, reloj);
        return Resultado.Ok(c);
    }

    /// <summary>Registra lo que comunica la aseguradora (concesión, reducción, denegación o anulación) con su fecha de efecto.</summary>
    public Resultado Comunicar(EstadoClasificacion estado, decimal concedido, DateOnly efecto, DateOnly? vence, string? nota, decimal? solicitado, string? referencia,
        IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(estado))
        {
            return Resultado.Fallo(Error.Validacion("seguro.estado", "Estado de clasificación no válido."));
        }

        if (_historial.Count > 0 && efecto < _historial.Max(h => h.Fecha))
        {
            return Resultado.Fallo(Error.Validacion("seguro.efecto_anterior", "La fecha de efecto no puede ser anterior al último cambio."));
        }

        if (vence is { } v && v < efecto)
        {
            return Resultado.Fallo(Error.Validacion("seguro.vence", "La clasificación no puede vencer antes de su efecto."));
        }

        var importe = estado is EstadoClasificacion.Concedida or EstadoClasificacion.Reducida ? Redondeo.Dos(concedido) : 0m;
        if (estado is EstadoClasificacion.Concedida or EstadoClasificacion.Reducida && importe <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("seguro.concedido", "Indica el importe concedido."));
        }

        if (solicitado is { } s && s > 0m)
        {
            Solicitado = Redondeo.Dos(s);
        }

        if (referencia is not null)
        {
            Referencia = Texto(referencia, 40);
        }

        if (estado == EstadoClasificacion.Concedida && importe < Solicitado)
        {
            estado = EstadoClasificacion.Reducida;
        }

        Aplicar(efecto, estado, importe, vence, Texto(nota, 300), reloj);
        return Resultado.Ok();
    }

    private void Aplicar(DateOnly fecha, EstadoClasificacion estado, decimal concedido, DateOnly? vence, string? nota, IReloj reloj)
    {
        Estado = estado;
        Concedido = concedido;
        Efecto = fecha;
        Vence = vence;
        _historial.Add(new CambioClasificacion(fecha, estado, concedido, nota, reloj.AhoraUtc));
    }

    private static string? Texto(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim() is var x && x.Length > max ? x[..max] : t.Trim();
}

/// <summary>Estado de un aviso de impago a la aseguradora.</summary>
public enum EstadoAvisoImpago
{
    /// <summary>Comunicado a la aseguradora.</summary>
    Comunicado = 1,

    /// <summary>El cliente pagó después del aviso.</summary>
    Cobrado = 2,

    /// <summary>La aseguradora indemnizó.</summary>
    Indemnizado = 3,

    /// <summary>Retirado (error, acuerdo con el cliente…).</summary>
    Retirado = 4,
}

/// <summary>Aviso de impago (siniestro) de una factura a la aseguradora, y cómo acabó.</summary>
public sealed class AvisoImpago : RaizAgregadoEmpresa<Guid>
{
    private AvisoImpago(Guid id)
        : base(id, Guid.Empty)
    {
        Factura = null!;
        ClienteNombre = null!;
    }

    private AvisoImpago(Guid id, Guid empresaId, Guid polizaId, Guid facturaId, string factura, Guid clienteId, string clienteNombre, DateOnly vencimiento,
        decimal importe, DateOnly fecha, string? referencia)
        : base(id, empresaId)
    {
        PolizaId = polizaId;
        FacturaId = facturaId;
        Factura = factura;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Vencimiento = vencimiento;
        Importe = importe;
        Fecha = fecha;
        Referencia = referencia;
        Estado = EstadoAvisoImpago.Comunicado;
    }

    public Guid PolizaId { get; private set; }

    public Guid FacturaId { get; private set; }

    public string Factura { get; private set; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public DateOnly Vencimiento { get; private set; }

    /// <summary>Importe pendiente comunicado.</summary>
    public decimal Importe { get; private set; }

    /// <summary>Día en que se comunicó.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Número de siniestro o expediente de la aseguradora.</summary>
    public string? Referencia { get; private set; }

    public EstadoAvisoImpago Estado { get; private set; }

    public DateOnly? FechaCierre { get; private set; }

    public decimal? Indemnizacion { get; private set; }

    public string? Nota { get; private set; }

    public static Resultado<AvisoImpago> Comunicar(Guid empresaId, Guid polizaId, Guid facturaId, string factura, Guid clienteId, string clienteNombre,
        DateOnly vencimiento, decimal importe, DateOnly fecha, string? referencia)
    {
        if (importe <= 0m)
        {
            return Resultado.Fallo<AvisoImpago>(Error.Validacion("seguro.aviso_importe", "La factura no tiene nada pendiente."));
        }

        if (fecha < vencimiento)
        {
            return Resultado.Fallo<AvisoImpago>(Error.Validacion("seguro.aviso_fecha", "No se avisa de un impago antes del vencimiento."));
        }

        return Resultado.Ok(new AvisoImpago(Guid.NewGuid(), empresaId, polizaId, facturaId, factura, clienteId, clienteNombre, vencimiento, Redondeo.Dos(importe), fecha,
            string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim()));
    }

    public Resultado Cerrar(EstadoAvisoImpago estado, DateOnly fecha, decimal? indemnizacion, string? nota)
    {
        if (Estado != EstadoAvisoImpago.Comunicado)
        {
            return Resultado.Fallo(Error.Conflicto("seguro.aviso_cerrado", "El aviso ya está cerrado."));
        }

        if (estado == EstadoAvisoImpago.Comunicado || !Enum.IsDefined(estado))
        {
            return Resultado.Fallo(Error.Validacion("seguro.aviso_estado", "Indica cómo acaba: cobrado, indemnizado o retirado."));
        }

        if (fecha < Fecha)
        {
            return Resultado.Fallo(Error.Validacion("seguro.aviso_fecha", "El cierre no puede ser anterior al aviso."));
        }

        if (estado == EstadoAvisoImpago.Indemnizado && indemnizacion is not > 0m)
        {
            return Resultado.Fallo(Error.Validacion("seguro.indemnizacion", "Indica lo que indemniza la aseguradora."));
        }

        Estado = estado;
        FechaCierre = fecha;
        Indemnizacion = estado == EstadoAvisoImpago.Indemnizado ? Redondeo.Dos(indemnizacion!.Value) : null;
        Nota = string.IsNullOrWhiteSpace(nota) ? null : nota.Trim();
        return Resultado.Ok();
    }
}
