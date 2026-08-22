using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Sentido de una previsión de tesorería.</summary>
public enum SentidoPrevision
{
    /// <summary>Entrada de dinero prevista (ingreso).</summary>
    Ingreso = 1,

    /// <summary>Salida de dinero prevista (gasto).</summary>
    Gasto = 2,
}

/// <summary>
/// Ingreso o gasto <b>previsto</b> (aún no facturado ni documentado) que el usuario añade a mano para
/// completar la previsión de tesorería. No mueve saldos reales: solo proyecta. Se elimina con
/// facilidad y se distingue a simple vista de los vencimientos reales.
/// </summary>
public sealed class PrevisionTesoreria : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaConcepto = 200;

    private PrevisionTesoreria(Guid id)
        : base(id, Guid.Empty)
    {
        Concepto = null!;
    }

    private PrevisionTesoreria(Guid id, Guid empresaId, SentidoPrevision sentido, string concepto, decimal importe, DateOnly fecha, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Sentido = sentido;
        Concepto = concepto;
        Importe = importe;
        Fecha = fecha;
        CreadoEn = ahora;
    }

    public SentidoPrevision Sentido { get; private set; }

    public string Concepto { get; private set; }

    /// <summary>Importe previsto, siempre positivo. El signo lo da el <see cref="Sentido"/>.</summary>
    public decimal Importe { get; private set; }

    /// <summary>Fecha prevista del cobro/pago.</summary>
    public DateOnly Fecha { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<PrevisionTesoreria> Crear(Guid empresaId, SentidoPrevision sentido, string? concepto, decimal importe, DateOnly fecha, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (string.IsNullOrWhiteSpace(concepto))
        {
            return Resultado.Fallo<PrevisionTesoreria>(Error.Validacion("prevision.concepto_vacio", "El concepto es obligatorio."));
        }

        if (importe <= 0m)
        {
            return Resultado.Fallo<PrevisionTesoreria>(Error.Validacion("prevision.importe_invalido", "El importe debe ser mayor que cero."));
        }

        var conceptoLimpio = concepto.Trim();
        if (conceptoLimpio.Length > LongitudMaximaConcepto)
        {
            return Resultado.Fallo<PrevisionTesoreria>(Error.Validacion("prevision.concepto_largo", "El concepto es demasiado largo."));
        }

        return Resultado.Ok(new PrevisionTesoreria(Guid.NewGuid(), empresaId, sentido, conceptoLimpio, Redondeo.Dos(importe), fecha, reloj.AhoraUtc));
    }
}
