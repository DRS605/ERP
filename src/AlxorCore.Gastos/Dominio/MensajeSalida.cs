using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Gastos.Dominio;

/// <summary>
/// Mensaje de la <b>bandeja de salida</b> (outbox transaccional) del módulo Gastos. Se guarda en la
/// misma transacción que el gasto, de modo que "registrar gasto" y "encolar su contabilización" son
/// atómicos. Un despachador lee los pendientes y ejecuta su efecto con reintento.
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

    public string Tipo { get; private set; }

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
