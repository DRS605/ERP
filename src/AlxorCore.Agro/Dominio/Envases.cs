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

/// <summary>Qué pasa si un movimiento deja la cuenta por encima de un límite o por debajo de un mínimo.</summary>
public enum ControlLimiteEnvases
{
    /// <summary>Se avisa y se registra.</summary>
    Aviso,

    /// <summary>No se registra salvo que se fuerce (queda anotado en el movimiento).</summary>
    Bloqueo,
}

/// <summary>Límite y mínimo de un envase en una cuenta (Hispatec: límite y mínimo por cuenta, empresa y artículo).</summary>
public sealed class LimiteEnvase
{
    private LimiteEnvase()
    {
    }

    internal LimiteEnvase(Guid envaseProductoId, int? limite, int? minimo)
    {
        Id = Guid.NewGuid();
        EnvaseProductoId = envaseProductoId;
        Limite = limite;
        Minimo = minimo;
    }

    public Guid Id { get; private set; }

    public Guid EnvaseProductoId { get; private set; }

    /// <summary>Máximo de ese envase que puede tener el tercero.</summary>
    public int? Limite { get; private set; }

    /// <summary>Mínimo que debe conservar (una recogida no lo deja por debajo).</summary>
    public int? Minimo { get; private set; }
}

/// <summary>Envase que pertenece a un pool (CHEP, IFCO…): sus movimientos con los clientes se le declaran.</summary>
public sealed class EnvasePool
{
    private EnvasePool()
    {
    }

    internal EnvasePool(Guid envaseProductoId)
    {
        Id = Guid.NewGuid();
        EnvaseProductoId = envaseProductoId;
    }

    public Guid Id { get; private set; }

    public Guid EnvaseProductoId { get; private set; }
}

/// <summary>
/// Configuración de envases de la empresa: la fecha de cierre (<c>FechaBloqueoMovimientoArticRetor</c> de Hispatec). Hasta
/// esa fecha, inclusive, no se registra ni se anula ningún movimiento: el periodo está cerrado.
/// </summary>
public sealed class ConfiguracionEnvases : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionEnvases(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ConfiguracionEnvases(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public DateOnly? FechaCierre { get; private set; }

    public static ConfiguracionEnvases Crear(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public void Cerrar(DateOnly? fecha) => FechaCierre = fecha;

    public bool Cerrado(DateOnly fecha) => FechaCierre is { } c && fecha <= c;
}

/// <summary>
/// Qué se hace con los envases que tiene un cliente (Hispatec: envases a retornar o a facturar). Con <see cref="Facturar"/>
/// se le factura todo su saldo; con <see cref="FacturarExceso"/>, solo lo que pasa de su límite por envase.
/// </summary>
public enum GestionEnvases
{
    Retornar,
    Facturar,
    FacturarExceso,
}

/// <summary>De dónde sale un movimiento de envases.</summary>
public enum OrigenMovimientoEnvases
{
    Manual,
    Expedicion,
    Regularizacion,

    /// <summary>Contramovimiento que anula otro (el libro es de solo inserción).</summary>
    Anulacion,

    /// <summary>Envases llenos que trae un agricultor en una recepción (o vacíos que se le entregan): el libro del agricultor.</summary>
    Recepcion,

    /// <summary>Envases que se le venden al cliente: salen de su saldo con un albarán de venta.</summary>
    Facturacion,
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

    /// <summary>Qué se hace al superar el límite (general o de un envase) o bajar del mínimo.</summary>
    public ControlLimiteEnvases ControlLimite { get; private set; }

    /// <summary>En un cliente: si sus envases se retornan o se le facturan (todos o el exceso sobre el límite).</summary>
    public GestionEnvases Gestion { get; private set; }

    private readonly List<LimiteEnvase> _limites = [];

    private readonly List<EnvasePool> _envasesPool = [];

    /// <summary>En un pool: los envases que son suyos (los que se le declaran).</summary>
    public IReadOnlyList<EnvasePool> EnvasesPool => _envasesPool;

    public Resultado FijarGestion(GestionEnvases gestion)
    {
        if (!Enum.IsDefined(gestion))
        {
            return Resultado.Fallo(Error.Validacion("envases.gestion", "Gestión no válida: Retornar, Facturar o FacturarExceso."));
        }

        if (gestion != GestionEnvases.Retornar && Tipo != TipoCuentaEnvases.Cliente)
        {
            return Resultado.Fallo(Error.Validacion("envases.gestion_cliente", "Solo se facturan envases a clientes."));
        }

        Gestion = gestion;
        return Resultado.Ok();
    }

    /// <summary>Sustituye los envases de un pool.</summary>
    public Resultado FijarEnvasesPool(IReadOnlyList<Guid> envases)
    {
        ArgumentNullException.ThrowIfNull(envases);
        if (Tipo != TipoCuentaEnvases.Pool && envases.Count > 0)
        {
            return Resultado.Fallo(Error.Validacion("envases.no_pool", "Solo una cuenta de pool tiene envases propios."));
        }

        _envasesPool.Clear();
        _envasesPool.AddRange(envases.Distinct().Select(e => new EnvasePool(e)));
        return Resultado.Ok();
    }

    /// <summary>
    /// Lo que se le factura a un cliente de su saldo por envase: todo lo positivo, o lo que pasa de su límite por envase
    /// (sin límite de ese envase, nada).
    /// </summary>
    public IReadOnlyList<(Guid EnvaseProductoId, int Cantidad)> AFacturar(IReadOnlyDictionary<Guid, int> saldo)
    {
        ArgumentNullException.ThrowIfNull(saldo);
        return Gestion switch
        {
            GestionEnvases.Facturar => saldo.Where(s => s.Value > 0).Select(s => (s.Key, s.Value)).ToList(),
            GestionEnvases.FacturarExceso => saldo
                .Select(s => (s.Key, Cantidad: _limites.FirstOrDefault(l => l.EnvaseProductoId == s.Key)?.Limite is { } max ? s.Value - max : 0))
                .Where(x => x.Cantidad > 0).ToList(),
            _ => [],
        };
    }

    public IReadOnlyList<LimiteEnvase> Limites => _limites;

    /// <summary>Sustituye los límites y mínimos por envase y fija cómo se controlan.</summary>
    public Resultado FijarLimites(ControlLimiteEnvases control, IReadOnlyList<(Guid EnvaseProductoId, int? Limite, int? Minimo)> limites)
    {
        ArgumentNullException.ThrowIfNull(limites);
        if (!Enum.IsDefined(control))
        {
            return Resultado.Fallo(Error.Validacion("envases.control", "Control no válido: Aviso o Bloqueo."));
        }

        if (limites.Select(l => l.EnvaseProductoId).Distinct().Count() != limites.Count)
        {
            return Resultado.Fallo(Error.Validacion("envases.limite_repetido", "Cada envase aparece una sola vez."));
        }

        foreach (var (_, limite, minimo) in limites)
        {
            if (limite is < 0 || minimo is < 0 || (limite is { } l && minimo is { } m && m > l))
            {
                return Resultado.Fallo(Error.Validacion("envases.limite", "Límite y mínimo no pueden ser negativos, y el mínimo no puede superar el límite."));
            }
        }

        ControlLimite = control;
        _limites.Clear();
        _limites.AddRange(limites.Where(l => l.Limite is not null || l.Minimo is not null).Select(l => new LimiteEnvase(l.EnvaseProductoId, l.Limite, l.Minimo)));
        return Resultado.Ok();
    }

    /// <summary>
    /// Incumplimientos que causaría un movimiento: el total por encima del límite general, un envase por encima de su
    /// límite (si el movimiento lo entrega) o por debajo de su mínimo (si lo recoge).
    /// </summary>
    public IReadOnlyList<string> Incumplimientos(IReadOnlyDictionary<Guid, int> saldoActual, IReadOnlyList<(Guid EnvaseProductoId, int Cantidad)> movimiento,
        IReadOnlyDictionary<Guid, string> nombres)
    {
        ArgumentNullException.ThrowIfNull(saldoActual);
        ArgumentNullException.ThrowIfNull(movimiento);
        ArgumentNullException.ThrowIfNull(nombres);
        var avisos = new List<string>();
        var neto = movimiento.Sum(l => l.Cantidad);
        var total = saldoActual.Values.Sum() + neto;
        if (Limite is { } general && neto > 0 && total > general)
        {
            avisos.Add($"{Nombre} supera su límite de envases: {total} de {general}.");
        }

        foreach (var g in movimiento.GroupBy(l => l.EnvaseProductoId))
        {
            var cantidad = g.Sum(l => l.Cantidad);
            var saldo = saldoActual.GetValueOrDefault(g.Key) + cantidad;
            var limite = _limites.FirstOrDefault(l => l.EnvaseProductoId == g.Key);
            var nombre = nombres.GetValueOrDefault(g.Key, "envase");
            if (limite?.Limite is { } max && cantidad > 0 && saldo > max)
            {
                avisos.Add($"{Nombre} supera el límite de {nombre}: {saldo} de {max}.");
            }

            if (limite?.Minimo is { } min && cantidad < 0 && saldo < min)
            {
                avisos.Add($"{Nombre} queda por debajo del mínimo de {nombre}: {saldo} (mínimo {min}).");
            }
        }

        return avisos;
    }

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
