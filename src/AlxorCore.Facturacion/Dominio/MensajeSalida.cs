using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>
/// Mensaje de la <b>bandeja de salida</b> (outbox transaccional). Se guarda en la <b>misma
/// transacción</b> que el documento que lo origina (p. ej. la factura), garantizando que "emitir la
/// factura" y "encolar su contabilización" son atómicos: o se guardan los dos, o ninguno. Un
/// despachador lee los mensajes pendientes y ejecuta su efecto (con reintento), de modo que nunca se
/// pierde una contabilización aunque el módulo de destino falle en ese instante.
/// </summary>
public sealed class MensajeSalida : RaizAgregadoEmpresa<Guid>
{
    public const string TipoContabilizacion = "Contabilizacion";

    private MensajeSalida(Guid id)
        : base(id, Guid.Empty)
    {
        Tipo = null!;
        Carga = null!;
    }

    private MensajeSalida(Guid id, Guid empresaId, string tipo, string carga, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        Carga = carga;
        CreadoEn = ahora;
    }

    /// <summary>Tipo de mensaje (determina cómo se despacha).</summary>
    public string Tipo { get; private set; }

    /// <summary>Carga útil serializada (JSON).</summary>
    public string Carga { get; private set; }

    public bool Procesado { get; private set; }

    public int Intentos { get; private set; }

    public string? UltimoError { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? ProcesadoEn { get; private set; }

    public static MensajeSalida Crear(Guid empresaId, string tipo, string carga, DateTimeOffset ahora) =>
        new(Guid.NewGuid(), empresaId, tipo, carga, ahora);

    public void MarcarProcesado(DateTimeOffset ahora)
    {
        Procesado = true;
        ProcesadoEn = ahora;
        Intentos++;
        UltimoError = null;
    }

    public void RegistrarError(string error)
    {
        Intentos++;
        UltimoError = error.Length > 500 ? error[..500] : error;
    }
}
