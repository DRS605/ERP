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

/// <summary>
/// Modalidad de una remesa de cobro, como en Hispatec: al vencimiento (el banco abona cada recibo al cobrarlo), en
/// gestión de cobro (igual, con comisión e IVA) o al descuento (el banco adelanta el nominal menos intereses y gastos
/// y el riesgo queda vivo en 5208 hasta el vencimiento).
/// </summary>
public enum ModalidadRemesa
{
    Vencimiento = 1,
    GestionCobro = 2,
    Descuento = 3,
}

/// <summary>Condiciones del banco para una remesa en gestión de cobro o al descuento (porcentajes en %).</summary>
public sealed record CondicionesRemesa(decimal PorcentajeInteres = 0m, int DiasMinimos = 0, decimal GastosFijos = 0m, decimal GastosPorEfecto = 0m,
    decimal Timbres = 0m, decimal OtrosGastos = 0m, decimal PorcentajeComision = 0m, decimal PorcentajeIvaComision = 0m);

/// <summary>Lo que liquida el banco: intereses, comisión con su IVA, gastos y el líquido que abona.</summary>
public sealed record CalculoLiquidacionRemesa(int Dias, decimal Intereses, decimal Comision, decimal IvaComision, decimal Gastos, decimal Liquido);

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

    public ModalidadRemesa Modalidad { get; private set; } = ModalidadRemesa.Vencimiento;

    public decimal PorcentajeInteres { get; private set; }

    public int DiasMinimos { get; private set; }

    public decimal GastosFijos { get; private set; }

    public decimal GastosPorEfecto { get; private set; }

    public decimal Timbres { get; private set; }

    public decimal OtrosGastos { get; private set; }

    public decimal PorcentajeComision { get; private set; }

    public decimal PorcentajeIvaComision { get; private set; }

    /// <summary>Intereses del descuento (liquidada).</summary>
    public decimal Intereses { get; private set; }

    public decimal Comision { get; private set; }

    public decimal IvaComision { get; private set; }

    /// <summary>Gastos fijos, por efecto, timbres y otros (liquidada).</summary>
    public decimal Gastos { get; private set; }

    /// <summary>Lo que abonó el banco: el nominal menos intereses, comisión, IVA y gastos.</summary>
    public decimal? Liquido { get; private set; }

    /// <summary>Al descuento: fecha en que venció y se canceló el riesgo (5208 contra 4311). Null mientras siga vivo.</summary>
    public DateOnly? RiesgoCanceladoEn { get; private set; }

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

    /// <summary>Fija la modalidad y las condiciones del banco (solo remesas de cobro, antes de liquidarlas).</summary>
    public Resultado Condiciones(ModalidadRemesa modalidad, CondicionesRemesa? condiciones)
    {
        if (!Enum.IsDefined(modalidad))
        {
            return Resultado.Fallo(Error.Validacion("remesa.modalidad", "Modalidad no válida: Vencimiento, GestionCobro o Descuento."));
        }

        if (modalidad != ModalidadRemesa.Vencimiento && Tipo != TipoRemesa.Cobro)
        {
            return Resultado.Fallo(Error.Validacion("remesa.modalidad", "Solo las remesas de cobro se llevan en gestión de cobro o al descuento."));
        }

        if (!EstaViva)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.estado", "La remesa ya no está viva: no se cambian sus condiciones."));
        }

        var c = modalidad == ModalidadRemesa.Vencimiento ? new CondicionesRemesa() : condiciones ?? new CondicionesRemesa();
        if (c.PorcentajeInteres < 0 || c.DiasMinimos < 0 || c.GastosFijos < 0 || c.GastosPorEfecto < 0 || c.Timbres < 0 || c.OtrosGastos < 0
            || c.PorcentajeComision < 0 || c.PorcentajeIvaComision < 0 || c.PorcentajeInteres > 100 || c.PorcentajeComision > 100 || c.PorcentajeIvaComision > 100)
        {
            return Resultado.Fallo(Error.Validacion("remesa.condiciones", "Los porcentajes y gastos no pueden ser negativos (ni los porcentajes mayores que 100)."));
        }

        Modalidad = modalidad;
        PorcentajeInteres = modalidad == ModalidadRemesa.Descuento ? c.PorcentajeInteres : 0m;
        DiasMinimos = modalidad == ModalidadRemesa.Descuento ? c.DiasMinimos : 0;
        GastosFijos = c.GastosFijos;
        GastosPorEfecto = c.GastosPorEfecto;
        Timbres = modalidad == ModalidadRemesa.Descuento ? c.Timbres : 0m;
        OtrosGastos = c.OtrosGastos;
        PorcentajeComision = c.PorcentajeComision;
        PorcentajeIvaComision = c.PorcentajeIvaComision;
        return Resultado.Ok();
    }

    /// <summary>
    /// Lo que liquida el banco en <paramref name="fecha"/>, como en Hispatec: intereses = nominal × % × máx(días hasta el
    /// vencimiento, días mínimos) / 360 (solo al descuento); comisión = nominal × %; su IVA; gastos = fijos + por efecto ×
    /// nº de efectos + timbres + otros. Líquido = nominal − todo eso.
    /// </summary>
    public CalculoLiquidacionRemesa Calcular(DateOnly fecha)
    {
        var dias = Math.Max(FechaCargo.DayNumber - fecha.DayNumber, 0);
        var intereses = Modalidad == ModalidadRemesa.Descuento ? Math.Round(Total * PorcentajeInteres / 100m * Math.Max(dias, DiasMinimos) / 360m, 2) : 0m;
        var comision = Modalidad == ModalidadRemesa.Vencimiento ? 0m : Math.Round(Total * PorcentajeComision / 100m, 2);
        var iva = Math.Round(comision * PorcentajeIvaComision / 100m, 2);
        var gastos = Modalidad == ModalidadRemesa.Vencimiento ? 0m : Math.Round(GastosFijos + GastosPorEfecto * _lineas.Count + Timbres + OtrosGastos, 2);
        return new CalculoLiquidacionRemesa(dias, intereses, comision, iva, gastos, Math.Round(Total - intereses - comision - iva - gastos, 2));
    }

    /// <summary>Al descuento, al vencer: el riesgo con el banco (5208) se cancela contra los efectos descontados (4311).</summary>
    public Resultado CancelarRiesgo(DateOnly fecha)
    {
        if (Modalidad != ModalidadRemesa.Descuento || Estado != EstadoRemesa.Liquidada)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.no_descontada", "Solo se cancela el riesgo de una remesa al descuento ya abonada por el banco."));
        }

        if (RiesgoCanceladoEn is not null)
        {
            return Resultado.Fallo(Error.Conflicto("remesa.riesgo_cancelado", "El riesgo de esta remesa ya está cancelado."));
        }

        if (fecha < FechaCargo)
        {
            return Resultado.Fallo(Error.Validacion("remesa.no_vencida", $"El riesgo se cancela al vencimiento ({FechaCargo:dd/MM/yyyy}), no antes."));
        }

        RiesgoCanceladoEn = fecha;
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
        if (Modalidad != ModalidadRemesa.Vencimiento)
        {
            var c = Calcular(fecha);
            Intereses = c.Intereses;
            Comision = c.Comision;
            IvaComision = c.IvaComision;
            Gastos = c.Gastos;
            Liquido = c.Liquido;
        }
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
