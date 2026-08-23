using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>Número correlativo de una factura (prefijo + ejercicio + número).</summary>
public sealed record NumeroFactura(string Prefijo, int Ejercicio, long Numero)
{
    public string Completo => $"{Prefijo}{Ejercicio}/{Numero:D6}";
}

/// <summary>
/// Factura emitida. Es el agregado central de la fiscalidad de ALXOR Core. Una vez emitida es
/// <b>inmutable</b> (invariante F2): no se edita ni se borra; su corrección se hará mediante una
/// factura rectificativa. Los datos del cliente y de las líneas se "congelan" al emitir (F4).
/// </summary>
public sealed class Factura : RaizAgregadoEmpresa<Guid>
{
    public const decimal IrpfMaximo = 60m;

    /// <summary>
    /// Importe total máximo de una factura simplificada (ticket). Se usa el límite de 3.000 € que la
    /// normativa (art. 4 RD 1619/2012) permite en sectores como comercio minorista y hostelería, que
    /// son el caso de uso del TPV. Por encima de ese importe debe emitirse factura ordinaria.
    /// </summary>
    public const decimal TicketImporteMaximo = 3000m;

    private readonly List<LineaFactura> _lineas = [];

    private Factura(Guid id)
        : base(id, Guid.Empty)
    {
        Prefijo = null!;
        NumeroCompleto = null!;
        ClienteNombre = null!;
        Pais = null!;
    }

    private Factura(Guid id, Guid empresaId, NumeroFactura numero, DateOnly fechaEmision, DateOnly fechaOperacion, DateOnly fechaVencimiento, ClienteFacturado cliente, decimal porcentajeIrpf, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Prefijo = numero.Prefijo;
        Ejercicio = numero.Ejercicio;
        Numero = numero.Numero;
        NumeroCompleto = numero.Completo;
        FechaEmision = fechaEmision;
        FechaOperacion = fechaOperacion;
        FechaVencimiento = fechaVencimiento;
        ClienteId = cliente.ClienteId;
        ClienteNombre = cliente.Nombre;
        ClienteNif = cliente.Nif;
        ClienteCalle = cliente.Calle;
        ClienteCodigoPostal = cliente.CodigoPostal;
        ClientePoblacion = cliente.Poblacion;
        ClienteProvincia = cliente.Provincia;
        Pais = cliente.Pais;
        ActividadNegocioId = cliente.ActividadNegocioId;
        PorcentajeIrpf = porcentajeIrpf;
        Estado = EstadoFactura.Emitida;
        TipoFactura = TipoFactura.Ordinaria;
        CreadoEn = ahora;
    }

    // --- Numeración ---
    public string Prefijo { get; private set; }
    public int Ejercicio { get; private set; }
    public long Numero { get; private set; }
    public string NumeroCompleto { get; private set; }

    // --- Fechas fiscales ---
    public DateOnly FechaEmision { get; private set; }
    public DateOnly FechaOperacion { get; private set; }

    /// <summary>Fecha de vencimiento del cobro (plazo de pago). Por defecto, la de emisión (contado).</summary>
    public DateOnly FechaVencimiento { get; private set; }

    // --- Cliente (snapshot congelado); nulo en tickets sin cliente identificado ---
    public Guid? ClienteId { get; private set; }
    public string ClienteNombre { get; private set; }
    public string? ClienteNif { get; private set; }
    public string ClienteCalle { get; private set; } = string.Empty;
    public string ClienteCodigoPostal { get; private set; } = string.Empty;
    public string ClientePoblacion { get; private set; } = string.Empty;
    public string ClienteProvincia { get; private set; } = string.Empty;
    public string Pais { get; private set; }

    /// <summary>
    /// Actividad de negocio del cliente en el momento de emitir (snapshot). Permite segmentar las
    /// ventas por línea/división de negocio en los informes. Null = sin actividad.
    /// </summary>
    public Guid? ActividadNegocioId { get; private set; }

    /// <summary>
    /// Mención(es) legal(es) que deben figurar en la factura por la naturaleza del IVA de sus líneas
    /// (exención, no sujeción, inversión del sujeto pasivo, operación intracomunitaria…). Null = ninguna.
    /// </summary>
    public string? MencionFiscal { get; private set; }

    /// <summary>Fija la mención fiscal de la factura (se establece al emitir a partir de los tipos de IVA).</summary>
    public void EstablecerMencionFiscal(string? mencion) =>
        MencionFiscal = string.IsNullOrWhiteSpace(mencion) ? null : mencion.Trim();

    // --- Importes ---
    public decimal BaseImponible { get; private set; }
    public decimal CuotaIva { get; private set; }
    public decimal PorcentajeIrpf { get; private set; }
    public decimal RetencionIrpf { get; private set; }

    /// <summary>Indica si la factura lleva recargo de equivalencia (cliente minorista en ese régimen).</summary>
    public bool RecargoEquivalencia { get; private set; }

    /// <summary>Cuota total de recargo de equivalencia (0 si no aplica).</summary>
    public decimal RecargoTotal { get; private set; }

    public decimal Total { get; private set; }

    // --- Estado y tipo ---
    public EstadoFactura Estado { get; private set; }
    public TipoFactura TipoFactura { get; private set; }
    public Guid? RectificaFacturaId { get; private set; }

    /// <summary>Motivo de la rectificación (obligatorio en las rectificativas).</summary>
    public string? MotivoRectificacion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    // --- Campos VeriFactu/SII ---
    public string? Huella { get; private set; }
    public string? HuellaAnterior { get; private set; }
    public string? IdRegistro { get; private set; }
    public string? TipoOperacion { get; private set; }
    public string? EstadoEnvioAeat { get; private set; }

    /// <summary>Instante de generación del registro VeriFactu (con huso), parte de la huella.</summary>
    public DateTimeOffset? FechaHoraGenRegistro { get; private set; }

    /// <summary>Motivo de la anulación (obligatorio al anular). Nulo si la factura no está anulada.</summary>
    public string? MotivoAnulacion { get; private set; }

    /// <summary>Huella del registro de anulación VeriFactu (encadenada). Nula si no se ha anulado.</summary>
    public string? HuellaAnulacion { get; private set; }

    /// <summary>Instante del registro de anulación (con huso).</summary>
    public DateTimeOffset? FechaHoraAnulacion { get; private set; }

    /// <summary>
    /// Genera el <b>registro de alta VeriFactu</b> de la factura: calcula su huella encadenándola con
    /// la del registro anterior de la empresa y deja el registro almacenado localmente (pendiente de
    /// envío a la AEAT). Se invoca una sola vez, al emitir.
    /// </summary>
    public void RegistrarVerifactu(string nifEmisor, string? huellaAnterior, DateTimeOffset generadoEn)
    {
        HuellaAnterior = huellaAnterior;
        FechaHoraGenRegistro = generadoEn;
        Huella = Verifactu.CalcularHuella(
            nifEmisor, NumeroCompleto, FechaEmision, Verifactu.TipoCodigo(TipoFactura), CuotaIva, Total, huellaAnterior, generadoEn);
        IdRegistro = Id.ToString("N");
        EstadoEnvioAeat = "Registrado";
    }

    public IReadOnlyList<LineaFactura> Lineas => _lineas.AsReadOnly();

    /// <summary>
    /// Emite una factura ordinaria, calculando sus importes (IVA por línea + retención de IRPF) y
    /// congelando los datos. El número debe haberse asignado antes de forma atómica y correlativa.
    /// </summary>
    public static Resultado<Factura> Emitir(
        Guid empresaId,
        NumeroFactura numero,
        DateOnly fechaEmision,
        DateOnly fechaOperacion,
        ClienteFacturado cliente,
        IReadOnlyList<NuevaLinea> lineas,
        decimal porcentajeIrpf,
        IReloj reloj,
        DateOnly? fechaVencimiento = null)
    {
        ArgumentNullException.ThrowIfNull(numero);
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.sin_lineas", "La factura debe tener al menos una línea."));
        }

        if (fechaOperacion > fechaEmision)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.fechas", "La fecha de operación no puede ser posterior a la de emisión."));
        }

        if (fechaVencimiento is not null && fechaVencimiento < fechaEmision)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.vencimiento", "El vencimiento no puede ser anterior a la emisión."));
        }

        if (porcentajeIrpf is < 0 or > IrpfMaximo)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.irpf_invalido", "El porcentaje de IRPF no es válido."));
        }

        foreach (var linea in lineas)
        {
            var error = ValidarLinea(linea);
            if (error is not null)
            {
                return Resultado.Fallo<Factura>(error);
            }
        }

        var factura = new Factura(Guid.NewGuid(), empresaId, numero, fechaEmision, fechaOperacion, fechaVencimiento ?? fechaEmision, cliente, porcentajeIrpf, reloj.AhoraUtc);
        foreach (var datos in lineas)
        {
            factura._lineas.Add(new LineaFactura(empresaId, datos));
        }

        factura.BaseImponible = Redondeo.Dos(factura._lineas.Sum(l => l.Base));
        factura.CuotaIva = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaIva));
        factura.RecargoTotal = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaRecargo));
        factura.RecargoEquivalencia = factura.RecargoTotal > 0m;
        factura.RetencionIrpf = Redondeo.Dos(factura.BaseImponible * porcentajeIrpf / 100m);
        factura.Total = Redondeo.Dos(factura.BaseImponible + factura.CuotaIva + factura.RecargoTotal - factura.RetencionIrpf);

        factura.RegistrarEvento(new FacturaEmitida(factura.Id, empresaId, factura.NumeroCompleto, factura.Total, reloj.AhoraUtc));
        return Resultado.Ok(factura);
    }

    /// <summary>
    /// Emite una <b>factura simplificada</b> (ticket): igual que una ordinaria pero sin retención de
    /// IRPF, admitiendo un destinatario sin identificar y con el tope de importe
    /// <see cref="TicketImporteMaximo"/>. Comparte el cálculo de importes y el congelado de datos.
    /// </summary>
    public static Resultado<Factura> EmitirSimplificada(
        Guid empresaId,
        NumeroFactura numero,
        DateOnly fecha,
        ClienteFacturado cliente,
        IReadOnlyList<NuevaLinea> lineas,
        IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(numero);
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.sin_lineas", "El ticket debe tener al menos una línea."));
        }

        foreach (var linea in lineas)
        {
            var error = ValidarLinea(linea);
            if (error is not null)
            {
                return Resultado.Fallo<Factura>(error);
            }
        }

        var factura = new Factura(Guid.NewGuid(), empresaId, numero, fecha, fecha, fecha, cliente, 0m, reloj.AhoraUtc)
        {
            TipoFactura = TipoFactura.Simplificada,
        };
        foreach (var datos in lineas)
        {
            factura._lineas.Add(new LineaFactura(empresaId, datos));
        }

        factura.BaseImponible = Redondeo.Dos(factura._lineas.Sum(l => l.Base));
        factura.CuotaIva = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaIva));
        factura.RecargoTotal = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaRecargo));
        factura.RecargoEquivalencia = factura.RecargoTotal > 0m;
        factura.RetencionIrpf = 0m;
        factura.Total = Redondeo.Dos(factura.BaseImponible + factura.CuotaIva + factura.RecargoTotal);

        if (factura.Total > TicketImporteMaximo)
        {
            return Resultado.Fallo<Factura>(Error.Validacion(
                "ticket.importe_excedido",
                $"Un ticket (factura simplificada) no puede superar {TicketImporteMaximo:0} €. Emite una factura ordinaria."));
        }

        factura.RegistrarEvento(new FacturaEmitida(factura.Id, empresaId, factura.NumeroCompleto, factura.Total, reloj.AhoraUtc));
        return Resultado.Ok(factura);
    }

    /// <summary>
    /// Emite una <b>factura rectificativa</b> (por sustitución) que corrige a otra factura: referencia
    /// a la original, motivo obligatorio y tipo R1. Calcula sus importes como una factura ordinaria.
    /// La factura original debe marcarse aparte con <see cref="MarcarRectificada"/> (invariante F6).
    /// </summary>
    public static Resultado<Factura> EmitirRectificativa(
        Guid empresaId,
        NumeroFactura numero,
        DateOnly fechaEmision,
        ClienteFacturado cliente,
        IReadOnlyList<NuevaLinea> lineas,
        decimal porcentajeIrpf,
        Guid facturaOriginalId,
        string? motivo,
        IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(numero);
        ArgumentNullException.ThrowIfNull(cliente);
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo<Factura>(Error.Validacion("rectificativa.sin_motivo", "La rectificativa necesita un motivo."));
        }

        if (lineas.Count == 0)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.sin_lineas", "La factura debe tener al menos una línea."));
        }

        if (porcentajeIrpf is < 0 or > IrpfMaximo)
        {
            return Resultado.Fallo<Factura>(Error.Validacion("factura.irpf_invalido", "El porcentaje de IRPF no es válido."));
        }

        foreach (var linea in lineas)
        {
            var error = ValidarLinea(linea);
            if (error is not null)
            {
                return Resultado.Fallo<Factura>(error);
            }
        }

        var factura = new Factura(Guid.NewGuid(), empresaId, numero, fechaEmision, fechaEmision, fechaEmision, cliente, porcentajeIrpf, reloj.AhoraUtc)
        {
            TipoFactura = TipoFactura.Rectificativa,
            RectificaFacturaId = facturaOriginalId,
            MotivoRectificacion = motivo.Trim(),
        };
        foreach (var datos in lineas)
        {
            factura._lineas.Add(new LineaFactura(empresaId, datos));
        }

        factura.BaseImponible = Redondeo.Dos(factura._lineas.Sum(l => l.Base));
        factura.CuotaIva = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaIva));
        factura.RecargoTotal = Redondeo.Dos(factura._lineas.Sum(l => l.CuotaRecargo));
        factura.RecargoEquivalencia = factura.RecargoTotal > 0m;
        factura.RetencionIrpf = Redondeo.Dos(factura.BaseImponible * porcentajeIrpf / 100m);
        factura.Total = Redondeo.Dos(factura.BaseImponible + factura.CuotaIva + factura.RecargoTotal - factura.RetencionIrpf);

        factura.RegistrarEvento(new FacturaEmitida(factura.Id, empresaId, factura.NumeroCompleto, factura.Total, reloj.AhoraUtc));
        return Resultado.Ok(factura);
    }

    /// <summary>Marca esta factura como rectificada por otra. Solo una factura emitida puede rectificarse.</summary>
    public Resultado MarcarRectificada()
    {
        if (Estado != EstadoFactura.Emitida)
        {
            return Resultado.Fallo(Error.Conflicto("factura.no_rectificable", "Solo una factura emitida puede rectificarse."));
        }

        Estado = EstadoFactura.Rectificada;
        return Resultado.Ok();
    }

    /// <summary>
    /// Anula la factura conforme a la ley antifraude: <b>no la borra</b> ni la modifica en sus
    /// importes; cambia su estado a <see cref="EstadoFactura.Anulada"/>, guarda el motivo y genera un
    /// <b>registro de anulación</b> VeriFactu con su propia huella encadenada. Solo una factura
    /// emitida puede anularse (una rectificada o ya anulada, no).
    /// </summary>
    public Resultado Anular(string? motivo, string nifEmisor, string? huellaAnterior, DateTimeOffset generadoEn)
    {
        if (Estado != EstadoFactura.Emitida)
        {
            return Resultado.Fallo(Error.Conflicto("factura.no_anulable", "Solo una factura emitida puede anularse."));
        }

        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo(Error.Validacion("factura.motivo_anulacion", "El motivo de la anulación es obligatorio."));
        }

        Estado = EstadoFactura.Anulada;
        MotivoAnulacion = motivo.Trim();
        FechaHoraAnulacion = generadoEn;
        HuellaAnulacion = Verifactu.CalcularHuellaAnulacion(nifEmisor, NumeroCompleto, huellaAnterior, generadoEn);
        return Resultado.Ok();
    }

    private static Error? ValidarLinea(NuevaLinea linea)
    {
        if (string.IsNullOrWhiteSpace(linea.Descripcion))
        {
            return Error.Validacion("factura.linea_sin_descripcion", "Cada línea necesita una descripción.");
        }

        if (linea.Cantidad <= 0)
        {
            return Error.Validacion("factura.linea_cantidad", "La cantidad debe ser mayor que cero.");
        }

        if (linea.PrecioUnitario < 0)
        {
            return Error.Validacion("factura.linea_precio", "El precio no puede ser negativo.");
        }

        if (linea.PorcentajeDescuento is < 0 or > 100)
        {
            return Error.Validacion("factura.linea_descuento", "El descuento debe estar entre 0 y 100.");
        }

        if (linea.PorcentajeIva < 0)
        {
            return Error.Validacion("factura.linea_iva", "El IVA no puede ser negativo.");
        }

        return null;
    }
}
