using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Motivos de devolución de adeudos SEPA (códigos R de la norma ISO 20022, los que usan los bancos españoles).</summary>
public static class MotivosDevolucionSepa
{
    public static IReadOnlyDictionary<string, string> Todos { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["AC01"] = "Número de cuenta incorrecto (IBAN no válido)",
        ["AC04"] = "Cuenta cancelada",
        ["AC06"] = "Cuenta bloqueada o no admite adeudos",
        ["AC13"] = "El deudor es un consumidor (esquema B2B no admitido)",
        ["AG01"] = "Operación no permitida en esta cuenta",
        ["AG02"] = "Código de operación o formato no válido",
        ["AM04"] = "Fondos insuficientes",
        ["AM05"] = "Adeudo duplicado",
        ["BE05"] = "Acreedor no reconocido",
        ["FF01"] = "Formato de fichero no válido",
        ["MD01"] = "Sin mandato o mandato no válido",
        ["MD02"] = "Faltan datos obligatorios del mandato",
        ["MD06"] = "Devolución solicitada por el cliente (no conforme)",
        ["MD07"] = "Deudor fallecido",
        ["MS02"] = "Motivo no especificado por el cliente",
        ["MS03"] = "Motivo no especificado por la entidad",
        ["RC01"] = "BIC incorrecto",
        ["RR01"] = "Faltan datos del deudor (normativa)",
        ["RR02"] = "Faltan nombre o dirección del deudor",
        ["RR03"] = "Faltan nombre o dirección del acreedor",
        ["RR04"] = "Motivos regulatorios",
        ["SL01"] = "Servicios específicos del banco del deudor (filtro de adeudos)",
    };

    public static bool Existe(string? codigo) => codigo is not null && Todos.ContainsKey(codigo);

    public static string Describir(string codigo) => Todos.TryGetValue(codigo, out var d) ? d : codigo;
}

/// <summary>
/// Devolución de un recibo domiciliado: el banco devuelve un cobro ya abonado (fondos insuficientes, orden del cliente…).
/// Se registra la anulación del cobro (el documento vuelve a tener pendiente), el contraasiento contra el banco y, si
/// los hay, los gastos de devolución que cobra el banco (626, o a cargo del cliente). Es de solo inserción: la
/// devolución se «corrige» volviendo a cobrar el recibo.
/// </summary>
public sealed class DevolucionRecibo : RaizAgregadoEmpresa<Guid>
{
    private DevolucionRecibo(Guid id)
        : base(id, Guid.Empty)
    {
        Motivo = null!;
    }

    private DevolucionRecibo(Guid id, Guid empresaId, Guid movimientoId, Guid anulacionMovimientoId, TipoDocumentoTesoreria tipoDocumento, Guid documentoId,
        Guid? remesaId, Guid? cuentaBancariaId, DateOnly fecha, string motivo, decimal importe, decimal gastos, bool gastosRepercutidos, Guid? efectoGastosId,
        string? nota, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        MovimientoId = movimientoId;
        AnulacionMovimientoId = anulacionMovimientoId;
        TipoDocumento = tipoDocumento;
        DocumentoId = documentoId;
        RemesaId = remesaId;
        CuentaBancariaId = cuentaBancariaId;
        Fecha = fecha;
        Motivo = motivo;
        Importe = importe;
        Gastos = gastos;
        GastosRepercutidos = gastosRepercutidos;
        EfectoGastosId = efectoGastosId;
        Nota = nota;
        CreadoEn = ahora;
    }

    /// <summary>Cobro devuelto.</summary>
    public Guid MovimientoId { get; private set; }

    /// <summary>Movimiento en negativo que lo anula (reabre el pendiente del documento).</summary>
    public Guid AnulacionMovimientoId { get; private set; }

    public TipoDocumentoTesoreria TipoDocumento { get; private set; }

    public Guid DocumentoId { get; private set; }

    /// <summary>Remesa en la que se cobró el recibo (si vino de una).</summary>
    public Guid? RemesaId { get; private set; }

    public Guid? CuentaBancariaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Código SEPA del motivo (AM04, MD06…).</summary>
    public string Motivo { get; private set; }

    public decimal Importe { get; private set; }

    /// <summary>Gastos de devolución que carga el banco.</summary>
    public decimal Gastos { get; private set; }

    /// <summary>Los gastos se reclaman al cliente (efecto a cobrar) en lugar de ir a gasto (626).</summary>
    public bool GastosRepercutidos { get; private set; }

    /// <summary>Efecto de cartera con los gastos a cobrar al cliente (si se repercuten).</summary>
    public Guid? EfectoGastosId { get; private set; }

    public string? Nota { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<DevolucionRecibo> Crear(Movimiento cobro, Movimiento anulacion, Guid? remesaId, DateOnly fecha, string? motivo, decimal gastos,
        bool gastosRepercutidos, Guid? efectoGastosId, string? nota, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(cobro);
        ArgumentNullException.ThrowIfNull(anulacion);
        ArgumentNullException.ThrowIfNull(reloj);
        var codigo = motivo?.Trim().ToUpperInvariant();
        if (!MotivosDevolucionSepa.Existe(codigo))
        {
            return Resultado.Fallo<DevolucionRecibo>(Error.Validacion("devolucion.motivo", "Indica un motivo de devolución SEPA válido (AM04, MD06…)."));
        }

        if (gastos < 0m || Math.Round(gastos, 2) != gastos)
        {
            return Resultado.Fallo<DevolucionRecibo>(Error.Validacion("devolucion.gastos", "Los gastos de devolución deben ser positivos, con dos decimales como máximo."));
        }

        var n = string.IsNullOrWhiteSpace(nota) ? null : (nota.Trim().Length > 200 ? nota.Trim()[..200] : nota.Trim());
        return Resultado.Ok(new DevolucionRecibo(Guid.NewGuid(), cobro.EmpresaId, cobro.Id, anulacion.Id, cobro.TipoDocumento, cobro.DocumentoId, remesaId,
            cobro.CuentaBancariaId, fecha, codigo!, cobro.Importe, gastos, gastosRepercutidos && gastos > 0m, efectoGastosId, n, reloj.AhoraUtc));
    }
}
