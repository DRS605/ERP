using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Tipo de remesa: adeudos SEPA a clientes (cobros) o transferencias SEPA a proveedores (pagos).</summary>
public enum TipoRemesa
{
    Cobro = 1,
    Pago = 2,
}

/// <summary>
/// Ciclo de vida de una remesa. <see cref="Generada"/> y <see cref="Presentada"/> son estados «vivos»: sus documentos no
/// pueden entrar en otra remesa. <see cref="Liquidada"/> es cobrada (adeudos) o pagada (transferencias): ya se han
/// registrado los movimientos. <see cref="Anulada"/> libera los documentos.
/// </summary>
public enum EstadoRemesa
{
    Generada = 1,
    Presentada = 2,
    Liquidada = 3,
    Anulada = 4,
}

/// <summary>Línea de una remesa: un documento (factura, gasto o efecto) por el importe que se cobra o paga.</summary>
public sealed class LineaRemesa
{
    private LineaRemesa()
    {
        Documento = null!;
        TerceroNombre = null!;
    }

    internal LineaRemesa(TipoDocumentoTesoreria tipoDocumento, Guid documentoId, string documento, string terceroNombre, string? iban, string? mandato,
        DateOnly? mandatoFecha, decimal importe)
    {
        Id = Guid.NewGuid();
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        Documento = Recortar(documento, 80);
        TerceroNombre = Recortar(terceroNombre, 200);
        Iban = iban;
        Mandato = mandato;
        MandatoFecha = mandatoFecha;
        Importe = importe;
        Viva = true;
    }

    public Guid Id { get; private set; }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    /// <summary>Número de la factura, concepto del gasto o documento del efecto (para listarla).</summary>
    public string Documento { get; private set; }

    public string TerceroNombre { get; private set; }

    public string? Iban { get; private set; }

    public string? Mandato { get; private set; }

    public DateOnly? MandatoFecha { get; private set; }

    public decimal Importe { get; private set; }

    /// <summary>Movimiento (cobro o pago) registrado al liquidar la remesa.</summary>
    public Guid? MovimientoId { get; private set; }

    /// <summary>
    /// La línea bloquea su documento (la remesa está generada o presentada). Un índice único parcial en la base de
    /// datos impide que el mismo documento esté vivo en dos remesas a la vez.
    /// </summary>
    public bool Viva { get; private set; }

    internal void Liquidar(Guid movimientoId)
    {
        MovimientoId = movimientoId;
        Viva = false;
    }

    internal void Liberar() => Viva = false;

    private static string Recortar(string texto, int max) => texto.Length > max ? texto[..max] : texto;
}

/// <summary>
/// Remesa SEPA registrada: adeudos directos (pain.008, Norma 19) para cobrar facturas y efectos por domiciliación, o
/// transferencias (pain.001, Cuaderno 34) para pagar gastos. Se numera por tipo y ejercicio, guarda el fichero generado
/// (se puede volver a descargar) y avanza Generada → Presentada → Liquidada (cobrada o pagada). Al liquidarla se
/// registra un movimiento por línea contra la cuenta bancaria de la remesa.
/// </summary>
public sealed class Remesa : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaRemesa> _lineas = [];

    private Remesa(Guid id)
        : base(id, Guid.Empty)
    {
        Fichero = null!;
        NombreArchivo = null!;
        MensajeId = null!;
    }

    private Remesa(Guid id, Guid empresaId, TipoRemesa tipo, int ejercicio, int numero, DateOnly fecha, DateOnly fechaCargo, Guid? cuentaBancariaId,
        string? esquema, string? secuencia, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        Ejercicio = ejercicio;
        Numero = numero;
        Fecha = fecha;
        FechaCargo = fechaCargo;
        CuentaBancariaId = cuentaBancariaId;
        Esquema = esquema;
        Secuencia = secuencia;
        Estado = EstadoRemesa.Generada;
        Fichero = string.Empty;
        NombreArchivo = string.Empty;
        MensajeId = string.Empty;
        CreadoEn = ahora;
    }

    public TipoRemesa Tipo { get; private set; }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    /// <summary>Fecha de generación.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Fecha de cobro (adeudos) o de ejecución (transferencias) pedida al banco.</summary>
    public DateOnly FechaCargo { get; private set; }

    /// <summary>Cuenta bancaria de la empresa por la que se cobra o paga (null: la de la ficha de la empresa, sin subcuenta).</summary>
    public Guid? CuentaBancariaId { get; private set; }

    /// <summary>CORE o B2B (solo adeudos).</summary>
    public string? Esquema { get; private set; }

    /// <summary>OOFF, FRST, RCUR o FNAL (solo adeudos).</summary>
    public string? Secuencia { get; private set; }

    public EstadoRemesa Estado { get; private set; }

    public decimal Total { get; private set; }

    /// <summary>Fichero XML generado (se guarda para poder descargarlo de nuevo).</summary>
    public string Fichero { get; private set; }

    public string NombreArchivo { get; private set; }

    /// <summary>Identificador del mensaje SEPA (MsgId).</summary>
    public string MensajeId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? PresentadaEn { get; private set; }

    /// <summary>Fecha en que el banco abonó o cargó la remesa (la de los movimientos).</summary>
    public DateOnly? FechaLiquidacion { get; private set; }

    public DateTimeOffset? AnuladaEn { get; private set; }

    public IReadOnlyList<LineaRemesa> Lineas => _lineas;

    public string Codigo => $"{Ejercicio}/{Numero}";

    public bool EstaViva => Estado is EstadoRemesa.Generada or EstadoRemesa.Presentada;

    public static Resultado<Remesa> Crear(Guid empresaId, TipoRemesa tipo, int numero, DateOnly fecha, DateOnly fechaCargo, Guid? cuentaBancariaId,
        string? esquema, string? secuencia, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<Remesa>(Error.Validacion("remesa.tipo", "El tipo de remesa debe ser Cobro o Pago."));
        }

        if (fechaCargo < fecha)
        {
            return Resultado.Fallo<Remesa>(Error.Validacion("remesa.fecha_cargo", "La fecha de cobro o pago no puede ser anterior a hoy."));
        }

        return Resultado.Ok(new Remesa(Guid.NewGuid(), empresaId, tipo, fecha.Year, numero, fecha, fechaCargo, cuentaBancariaId,
            tipo == TipoRemesa.Cobro ? esquema : null, tipo == TipoRemesa.Cobro ? secuencia : null, reloj.AhoraUtc));
    }

    public Resultado AgregarLinea(TipoDocumentoTesoreria tipoDocumento, Guid documentoId, string documento, string terceroNombre, string? iban, string? mandato,
        DateOnly? mandatoFecha, decimal importe)
    {
        if (Estado != EstadoRemesa.Generada || !string.IsNullOrEmpty(Fichero))
        {
            return Resultado.Fallo(Error.Conflicto("remesa.cerrada", "La remesa ya está generada: no admite más líneas."));
        }

        if (importe <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("remesa.importe", "El importe de cada línea debe ser positivo."));
        }

        if (_lineas.Any(l => l.TipoDocumento == tipoDocumento && l.DocumentoId == documentoId))
        {
            return Resultado.Fallo(Error.Conflicto("remesa.documento_repetido", $"{documento} ya está en la remesa."));
        }

        _lineas.Add(new LineaRemesa(tipoDocumento, documentoId, documento, terceroNombre, iban, mandato, mandatoFecha, importe));
        Total = Math.Round(_lineas.Sum(l => l.Importe), 2);
        return Resultado.Ok();
    }

    /// <summary>Guarda el fichero generado con las líneas de la remesa.</summary>
    public void AdjuntarFichero(string fichero, string nombreArchivo, string mensajeId)
    {
        Fichero = fichero;
        NombreArchivo = nombreArchivo;
        MensajeId = mensajeId;
    }

    /// <summary>La remesa se ha enviado al banco.</summary>
    public Resultado Presentar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoRemesa.Generada)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.estado", $"Solo se presenta una remesa generada (esta está {Descripcion(Tipo, Estado).ToLowerInvariant()})."));
        }

        Estado = EstadoRemesa.Presentada;
        PresentadaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Comprueba que la remesa se puede liquidar (cobrada o pagada).</summary>
    public Resultado PuedeLiquidarse(DateOnly fecha)
    {
        if (!EstaViva)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.estado", $"La remesa está {Descripcion(Tipo, Estado).ToLowerInvariant()}: no se puede liquidar."));
        }

        return fecha < Fecha
            ? Resultado.Fallo(Error.Validacion("remesa.fecha_liquidacion", "La fecha de cobro no puede ser anterior a la de la remesa."))
            : Resultado.Ok();
    }

    /// <summary>Registra la liquidación: cada línea con su movimiento.</summary>
    public void Liquidar(DateOnly fecha, IReadOnlyDictionary<Guid, Guid> movimientoPorLinea)
    {
        ArgumentNullException.ThrowIfNull(movimientoPorLinea);
        foreach (var linea in _lineas)
        {
            linea.Liquidar(movimientoPorLinea[linea.Id]);
        }

        Estado = EstadoRemesa.Liquidada;
        FechaLiquidacion = fecha;
    }

    /// <summary>Anula una remesa aún no liquidada (p. ej. rechazada por el banco): sus documentos quedan libres.</summary>
    public Resultado Anular(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!EstaViva)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.no_anulable", Estado == EstadoRemesa.Liquidada
                ? "La remesa ya está liquidada: registra la devolución de los recibos devueltos o anula sus movimientos."
                : "La remesa ya está anulada."));
        }

        foreach (var linea in _lineas)
        {
            linea.Liberar();
        }

        Estado = EstadoRemesa.Anulada;
        AnuladaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Estado para mostrar: «Cobrada» o «Pagada» cuando está liquidada.</summary>
    public static string Descripcion(TipoRemesa tipo, EstadoRemesa estado) => estado switch
    {
        EstadoRemesa.Generada => "Generada",
        EstadoRemesa.Presentada => "Presentada",
        EstadoRemesa.Liquidada => tipo == TipoRemesa.Cobro ? "Cobrada" : "Pagada",
        _ => "Anulada",
    };
}
