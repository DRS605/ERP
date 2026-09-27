using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>
/// Extracto bancario importado (fichero Norma 43) de una cuenta bancaria de la empresa. Sus apuntes se guardan
/// (<see cref="ApunteBancario"/>) y se concilian uno a uno; así la conciliación sobrevive a recargar la pantalla. El
/// mismo fichero no se importa dos veces en la misma cuenta (huella del contenido).
/// </summary>
public sealed class ExtractoImportado : RaizAgregadoEmpresa<Guid>
{
    private ExtractoImportado(Guid id)
        : base(id, Guid.Empty)
    {
        CuentaFichero = null!;
        NombreArchivo = null!;
        Huella = null!;
    }

    private ExtractoImportado(Guid id, Guid empresaId, Guid cuentaBancariaId, string cuentaFichero, DateOnly? desde, DateOnly? hasta, decimal? saldoInicial,
        decimal? saldoFinal, string nombreArchivo, string huella, int numeroApuntes, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        CuentaBancariaId = cuentaBancariaId;
        CuentaFichero = cuentaFichero;
        Desde = desde;
        Hasta = hasta;
        SaldoInicial = saldoInicial;
        SaldoFinal = saldoFinal;
        NombreArchivo = nombreArchivo;
        Huella = huella;
        NumeroApuntes = numeroApuntes;
        CreadoEn = ahora;
    }

    public Guid CuentaBancariaId { get; private set; }

    /// <summary>Cuenta tal como viene en el fichero (entidad, oficina y número).</summary>
    public string CuentaFichero { get; private set; }

    public DateOnly? Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    public decimal? SaldoInicial { get; private set; }

    public decimal? SaldoFinal { get; private set; }

    public string NombreArchivo { get; private set; }

    /// <summary>SHA-256 del contenido (evita importar el mismo fichero dos veces).</summary>
    public string Huella { get; private set; }

    public int NumeroApuntes { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static ExtractoImportado Crear(Guid empresaId, Guid cuentaBancariaId, string cuentaFichero, DateOnly? desde, DateOnly? hasta, decimal? saldoInicial,
        decimal? saldoFinal, string? nombreArchivo, string huella, int numeroApuntes, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var nombre = string.IsNullOrWhiteSpace(nombreArchivo) ? "extracto.n43" : nombreArchivo.Trim();
        return new ExtractoImportado(Guid.NewGuid(), empresaId, cuentaBancariaId, Recortar(cuentaFichero, 30), desde, hasta, saldoInicial, saldoFinal,
            Recortar(nombre, 200), huella, numeroApuntes, reloj.AhoraUtc);
    }

    private static string Recortar(string texto, int max) => texto.Length > max ? texto[..max] : texto;
}

/// <summary>Estado de conciliación de un apunte bancario.</summary>
public enum EstadoApunte
{
    Pendiente = 1,

    /// <summary>Casado por la conciliación automática (importe, fecha y pistas de referencia).</summary>
    ConciliadoAutomatico = 2,

    /// <summary>Casado a mano con uno o varios cobros, pagos o documentos.</summary>
    ConciliadoManual = 3,

    /// <summary>Contabilizado directamente con un asiento (comisiones, gastos, intereses…).</summary>
    ConAsiento = 4,
}

/// <summary>
/// Casación de un apunte con un movimiento de tesorería (cobro o pago). Si la conciliación registró el movimiento
/// (al casar con un documento pendiente), <see cref="MovimientoCreado"/> es cierto y deshacerla lo anula.
/// </summary>
public sealed class CasacionApunte
{
    private CasacionApunte()
    {
    }

    internal CasacionApunte(Guid movimientoId, TipoDocumentoTesoreria tipoDocumento, Guid documentoId, decimal importe, bool movimientoCreado)
    {
        Id = Guid.NewGuid();
        MovimientoId = movimientoId;
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        Importe = importe;
        MovimientoCreado = movimientoCreado;
    }

    public Guid Id { get; private set; }

    public Guid MovimientoId { get; private set; }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    /// <summary>Importe con signo del extracto: positivo si es un cobro, negativo si es un pago.</summary>
    public decimal Importe { get; private set; }

    public bool MovimientoCreado { get; private set; }
}

/// <summary>
/// Apunte de un extracto bancario importado, con su estado de conciliación: pendiente, casado (automática o
/// manualmente) con uno o varios movimientos cuya suma es la del apunte, o contabilizado con un asiento directo
/// contra la cuenta que se elija (comisiones 626, intereses 669/769…). La casación se puede deshacer.
/// </summary>
public sealed class ApunteBancario : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudConcepto = 500;

    private readonly List<CasacionApunte> _casaciones = [];

    private ApunteBancario(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
    }

    private ApunteBancario(Guid id, Guid empresaId, Guid extractoId, Guid cuentaBancariaId, int orden, DateOnly fecha, DateOnly? fechaValor, decimal importe,
        string concepto, string? conceptoComun, string? documento, string? referencia1, string? referencia2)
        : base(id, empresaId)
    {
        ExtractoId = extractoId;
        CuentaBancariaId = cuentaBancariaId;
        Orden = orden;
        Fecha = fecha;
        FechaValor = fechaValor;
        Importe = importe;
        Concepto = concepto;
        ConceptoComun = conceptoComun;
        Documento = documento;
        Referencia1 = referencia1;
        Referencia2 = referencia2;
        Estado = EstadoApunte.Pendiente;
    }

    public Guid ExtractoId { get; private set; }

    public Guid CuentaBancariaId { get; private set; }

    public int Orden { get; private set; }

    public DateOnly Fecha { get; private set; }

    public DateOnly? FechaValor { get; private set; }

    /// <summary>Importe con signo: positivo es un abono (entra dinero), negativo un cargo.</summary>
    public decimal Importe { get; private set; }

    public string Concepto { get; private set; }

    /// <summary>Concepto común AEB (01 talones, 02 abonarés, 03 recibos, 04 transferencias, 17 comisiones…).</summary>
    public string? ConceptoComun { get; private set; }

    public string? Documento { get; private set; }

    public string? Referencia1 { get; private set; }

    public string? Referencia2 { get; private set; }

    public EstadoApunte Estado { get; private set; }

    /// <summary>Contrapartida del asiento directo (estado ConAsiento).</summary>
    public string? CuentaAsiento { get; private set; }

    public string? ConceptoAsiento { get; private set; }

    /// <summary>Identificador de origen del asiento directo en la cola de contabilización.</summary>
    public Guid? AsientoOrigenId { get; private set; }

    public DateTimeOffset? ConciliadoEn { get; private set; }

    public IReadOnlyList<CasacionApunte> Casaciones => _casaciones;

    public static ApunteBancario Crear(ExtractoImportado extracto, int orden, DateOnly fecha, DateOnly? fechaValor, decimal importe, string concepto,
        string? conceptoComun, string? documento, string? referencia1, string? referencia2)
    {
        ArgumentNullException.ThrowIfNull(extracto);
        static string? Limpio(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : (t.Trim().Length > max ? t.Trim()[..max] : t.Trim());
        return new ApunteBancario(Guid.NewGuid(), extracto.EmpresaId, extracto.Id, extracto.CuentaBancariaId, orden, fecha, fechaValor, Math.Round(importe, 2),
            Limpio(concepto, LongitudConcepto) ?? "Movimiento bancario", Limpio(conceptoComun, 2), Limpio(documento, 10), Limpio(referencia1, 12), Limpio(referencia2, 16));
    }

    /// <summary>Casa el apunte con movimientos cuya suma (con signo) es su importe.</summary>
    public Resultado Conciliar(IReadOnlyList<(Guid MovimientoId, TipoDocumentoTesoreria Tipo, Guid DocumentoId, decimal Importe, bool Creado)> casaciones,
        bool automatico, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(casaciones);
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoApunte.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("apunte.conciliado", "El apunte ya está conciliado: deshaz antes su conciliación."));
        }

        if (casaciones.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("apunte.sin_casacion", "Elige al menos un cobro, pago o documento."));
        }

        var suma = Math.Round(casaciones.Sum(c => c.Importe), 2);
        if (suma != Importe)
        {
            return Resultado.Fallo(Error.Validacion("apunte.descuadre",
                $"La suma de lo elegido ({Redondeo.Formatear(suma)} €) no coincide con el apunte ({Redondeo.Formatear(Importe)} €)."));
        }

        foreach (var c in casaciones)
        {
            _casaciones.Add(new CasacionApunte(c.MovimientoId, c.Tipo, c.DocumentoId, c.Importe, c.Creado));
        }

        Estado = automatico ? EstadoApunte.ConciliadoAutomatico : EstadoApunte.ConciliadoManual;
        ConciliadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Contabiliza el apunte con un asiento directo contra la cuenta indicada.</summary>
    public Resultado ContabilizarDirecto(string? cuenta, string? concepto, Guid origenId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado != EstadoApunte.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("apunte.conciliado", "El apunte ya está conciliado: deshaz antes su conciliación."));
        }

        var c = cuenta?.Trim();
        if (string.IsNullOrEmpty(c) || c.Length is < 3 or > 12 || !c.All(char.IsAsciiDigit))
        {
            return Resultado.Fallo(Error.Validacion("apunte.cuenta", "Indica la cuenta contable de contrapartida (3 a 12 dígitos, p. ej. 626)."));
        }

        if (c.StartsWith("57", StringComparison.Ordinal))
        {
            return Resultado.Fallo(Error.Validacion("apunte.cuenta_tesoreria", "La contrapartida no puede ser otra cuenta de tesorería (57…): usa un traspaso."));
        }

        var texto = string.IsNullOrWhiteSpace(concepto) ? Concepto : concepto.Trim();
        CuentaAsiento = c;
        ConceptoAsiento = texto.Length > 80 ? texto[..80] : texto;
        AsientoOrigenId = origenId;
        Estado = EstadoApunte.ConAsiento;
        ConciliadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Vuelve a dejar el apunte pendiente (el llamador anula antes lo que la conciliación registró).</summary>
    public Resultado Deshacer()
    {
        if (Estado == EstadoApunte.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("apunte.pendiente", "El apunte no está conciliado."));
        }

        _casaciones.Clear();
        CuentaAsiento = null;
        ConceptoAsiento = null;
        AsientoOrigenId = null;
        ConciliadoEn = null;
        Estado = EstadoApunte.Pendiente;
        return Resultado.Ok();
    }
}
