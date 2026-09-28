using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>De quién es la cuenta de envases.</summary>
public enum TipoCuentaEnvases
{
    Cliente,
    Proveedor,
    Transportista,

    /// <summary>Empresa de alquiler de envases o palés (CHEP, IFCO, Euro Pool, LPR…), dada de alta como proveedor.</summary>
    Pool,
}

/// <summary>Qué pasa al mover envases de una cuenta bloqueada.</summary>
public enum BloqueoEnvases
{
    Ninguno,

    /// <summary>Se avisa, pero se deja seguir.</summary>
    Aviso,

    /// <summary>No se admite ningún movimiento.</summary>
    Bloqueo,
}

/// <summary>De dónde sale un movimiento de envases.</summary>
public enum OrigenMovimientoEnvases
{
    Manual,
    Expedicion,
    Regularizacion,

    /// <summary>Contramovimiento que anula otro (el libro es de solo inserción).</summary>
    Anulacion,
}

/// <summary>
/// Cuenta de envases retornables de un tercero, como en Hispatec: cliente, proveedor, transportista o pool. El saldo
/// (envases de la empresa que tiene el tercero) sale del libro de movimientos. Una cuenta puede acumular su saldo en una
/// <see cref="AgrupadoraId"/> (la «cuenta familiar»: una cadena con varias tiendas) y, en un cliente, llevar los envases
/// al transportista que los recoge (<see cref="ImputarATransportista"/>).
/// </summary>
public sealed class CuentaEnvases : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudNombre = 200;

    private CuentaEnvases(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private CuentaEnvases(Guid id, Guid empresaId, TipoCuentaEnvases tipo, Guid terceroId, string nombre, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        TerceroId = terceroId;
        Nombre = nombre;
        Activa = true;
        CreadaEn = ahora;
    }

    public TipoCuentaEnvases Tipo { get; private set; }

    /// <summary>Cliente, proveedor o transportista (según el tipo).</summary>
    public Guid TerceroId { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Cuenta en la que se acumulan los movimientos de esta (null: la propia).</summary>
    public Guid? AgrupadoraId { get; private set; }

    /// <summary>En un cliente: lo que se le entrega va a la cuenta del transportista de la expedición.</summary>
    public bool ImputarATransportista { get; private set; }

    public BloqueoEnvases Bloqueo { get; private set; }

    public string? MotivoBloqueo { get; private set; }

    /// <summary>Envases (en total) que puede tener el tercero; por encima se avisa.</summary>
    public int? Limite { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public static Resultado<CuentaEnvases> Crear(Guid empresaId, TipoCuentaEnvases tipo, Guid terceroId, string? nombre, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var texto = (nombre ?? string.Empty).Trim();
        if (texto.Length == 0)
        {
            return Resultado.Fallo<CuentaEnvases>(Error.Validacion("envases.nombre", "La cuenta de envases necesita el nombre del tercero."));
        }

        return Resultado.Ok(new CuentaEnvases(Guid.NewGuid(), empresaId, tipo, terceroId, texto[..Math.Min(texto.Length, LongitudNombre)], reloj.AhoraUtc));
    }

    public Resultado Configurar(Guid? agrupadoraId, bool imputarATransportista, BloqueoEnvases bloqueo, string? motivo, int? limite, bool activa)
    {
        if (agrupadoraId == Id)
        {
            return Resultado.Fallo(Error.Validacion("envases.agrupadora", "Una cuenta no puede agruparse en sí misma."));
        }

        if (limite is < 0)
        {
            return Resultado.Fallo(Error.Validacion("envases.limite", "El límite no puede ser negativo."));
        }

        if (!Enum.IsDefined(bloqueo))
        {
            return Resultado.Fallo(Error.Validacion("envases.bloqueo", "Bloqueo no válido: Ninguno, Aviso o Bloqueo."));
        }

        AgrupadoraId = agrupadoraId;
        ImputarATransportista = imputarATransportista && Tipo == TipoCuentaEnvases.Cliente;
        Bloqueo = bloqueo;
        MotivoBloqueo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim()[..Math.Min(motivo.Trim().Length, 200)];
        Limite = limite;
        Activa = activa;
        return Resultado.Ok();
    }
}

/// <summary>Una línea del movimiento: el envase (artículo del catálogo) y la cantidad con signo (+ entregado al tercero, − recogido).</summary>
public sealed class LineaMovimientoEnvases
{
    private LineaMovimientoEnvases()
    {
    }

    internal LineaMovimientoEnvases(Guid envaseProductoId, int cantidad)
    {
        Id = Guid.NewGuid();
        EnvaseProductoId = envaseProductoId;
        Cantidad = cantidad;
    }

    public Guid Id { get; private set; }

    public Guid EnvaseProductoId { get; private set; }

    public int Cantidad { get; private set; }
}

/// <summary>
/// Movimiento de envases con un tercero: numerado sin huecos por ejercicio (ENV-2026-000001) y de <b>solo inserción</b>. La
/// regla de signo es la de Hispatec: + lo entregado al tercero, − lo recogido; saldo &gt; 0, el tercero nos debe envases.
/// Se anula con un contramovimiento (<see cref="OrigenMovimientoEnvases.Anulacion"/>) que apunta al anulado.
/// </summary>
public sealed class MovimientoEnvases : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaMovimientoEnvases> _lineas = [];

    private MovimientoEnvases(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private MovimientoEnvases(Guid id, Guid empresaId, int ejercicio, int numero, DateOnly fecha, Guid cuentaId, Guid cuentaSolicitadaId, OrigenMovimientoEnvases origen,
        Guid? documentoId, Guid? transportistaId, string? matricula, string? observaciones, Guid? anulaMovimientoId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        Fecha = fecha;
        CuentaId = cuentaId;
        CuentaSolicitadaId = cuentaSolicitadaId;
        Origen = origen;
        DocumentoId = documentoId;
        TransportistaId = transportistaId;
        Matricula = matricula;
        Observaciones = observaciones;
        AnulaMovimientoId = anulaMovimientoId;
        CreadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"ENV-{Ejercicio}-{Numero:D6}";

    public DateOnly Fecha { get; private set; }

    /// <summary>Cuenta donde cuenta el saldo (la agrupadora o la del transportista, si se imputa allí).</summary>
    public Guid CuentaId { get; private set; }

    /// <summary>Cuenta del tercero del documento (la misma que <see cref="CuentaId"/> si no se redirigió).</summary>
    public Guid CuentaSolicitadaId { get; private set; }

    public OrigenMovimientoEnvases Origen { get; private set; }

    /// <summary>Palé expedido, albarán… que lo originó.</summary>
    public Guid? DocumentoId { get; private set; }

    public Guid? TransportistaId { get; private set; }

    public string? Matricula { get; private set; }

    public string? Observaciones { get; private set; }

    /// <summary>En un contramovimiento, el movimiento que anula.</summary>
    public Guid? AnulaMovimientoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaMovimientoEnvases> Lineas => _lineas;

    public static Resultado<MovimientoEnvases> Crear(Guid empresaId, int numero, DateOnly fecha, Guid cuentaId, Guid cuentaSolicitadaId, OrigenMovimientoEnvases origen,
        IReadOnlyList<(Guid EnvaseProductoId, int Cantidad)> lineas, IReloj reloj, Guid? documentoId = null, Guid? transportistaId = null, string? matricula = null,
        string? observaciones = null, Guid? anulaMovimientoId = null)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        ArgumentNullException.ThrowIfNull(reloj);
        var agrupadas = lineas.GroupBy(l => l.EnvaseProductoId).Select(g => (Envase: g.Key, Cantidad: g.Sum(l => l.Cantidad))).Where(l => l.Cantidad != 0).ToList();
        if (agrupadas.Count == 0)
        {
            return Resultado.Fallo<MovimientoEnvases>(Error.Validacion("envases.sin_lineas", "Indica qué envases se entregan o se recogen (cantidad distinta de cero)."));
        }

        static string? Texto(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
        var m = new MovimientoEnvases(Guid.NewGuid(), empresaId, fecha.Year, numero, fecha, cuentaId, cuentaSolicitadaId, origen, documentoId, transportistaId,
            Texto(matricula, 20), Texto(observaciones, 300), anulaMovimientoId, reloj.AhoraUtc);
        m._lineas.AddRange(agrupadas.Select(l => new LineaMovimientoEnvases(l.Envase, l.Cantidad)));
        return Resultado.Ok(m);
    }
}
