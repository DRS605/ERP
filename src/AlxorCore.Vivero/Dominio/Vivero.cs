using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Vivero.Dominio;

/// <summary>Fase de un lote de planta. Solo se avanza (se puede saltar una, p. ej. sin injerto).</summary>
public enum FasePlanta
{
    /// <summary>Sembrada en bandejas o semillero.</summary>
    Semillero = 1,

    /// <summary>Injertada sobre el portainjerto.</summary>
    Injerto = 2,

    /// <summary>Crecimiento y aclimatación.</summary>
    Crecimiento = 3,

    /// <summary>Lista para entregar o vender.</summary>
    Lista = 4,
}

public enum TipoMovimientoLote
{
    /// <summary>Plantas iniciales del lote.</summary>
    Siembra = 1,

    /// <summary>Plantas que se pierden (mortandad, descarte…).</summary>
    Baja = 2,

    /// <summary>Plantas entregadas a un cliente con un encargo.</summary>
    Entrega = 3,

    /// <summary>Plantas que pasan a las existencias del artículo para venderlas.</summary>
    PasoExistencias = 4,

    /// <summary>Vuelta de lo de otro movimiento al anularlo.</summary>
    Anulacion = 5,

    /// <summary>Cambio de fase o de ubicación (sin plantas).</summary>
    Cambio = 6,
}

public static class ReglasVivero
{
    public const int LongitudCodigo = 30;
    public const int LongitudTexto = 120;
    public const int LongitudObservaciones = 500;

    public static string? Texto(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public static Resultado TextoValido(string? texto, int maximo, string codigo, string que) =>
        texto?.Length > maximo ? Resultado.Fallo(Error.Validacion(codigo, $"{que} admite hasta {maximo} caracteres.")) : Resultado.Ok();
}

/// <summary>Datos del vivero para el pasaporte fitosanitario: el código de registro del operador (ROPVEG) y el país.</summary>
public sealed class ConfiguracionVivero : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionVivero(Guid id)
        : base(id, Guid.Empty)
    {
        CodigoRegistro = null!;
        PaisOrigen = null!;
    }

    private ConfiguracionVivero(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        CodigoRegistro = string.Empty;
        PaisOrigen = "ES";
    }

    /// <summary>Código del operador profesional en el registro oficial (ROPVEG), p. ej. <c>ES-04-12-0123</c>: la letra B del pasaporte.</summary>
    public string CodigoRegistro { get; private set; }

    /// <summary>País de origen (ISO 3166, dos letras): la letra D del pasaporte.</summary>
    public string PaisOrigen { get; private set; }

    public static ConfiguracionVivero Nueva(Guid empresaId) => new(Guid.NewGuid(), empresaId);

    public Resultado Fijar(string? codigoRegistro, string? paisOrigen)
    {
        var c = ReglasVivero.Texto(codigoRegistro)?.ToUpperInvariant();
        var p = ReglasVivero.Texto(paisOrigen)?.ToUpperInvariant() ?? "ES";
        if (c is null || c.Length > 40)
        {
            return Resultado.Fallo(Error.Validacion("vivero.registro", "Indica el código de registro del operador (ROPVEG), hasta 40 caracteres."));
        }

        if (p.Length != 2 || !p.All(char.IsAsciiLetterUpper))
        {
            return Resultado.Fallo(Error.Validacion("vivero.pais", "El país de origen son dos letras (ES, PT…)."));
        }

        CodigoRegistro = c;
        PaisOrigen = p;
        return Resultado.Ok();
    }
}

/// <summary>Movimiento del libro del lote (solo se insertan; se corrigen con una anulación).</summary>
public sealed class MovimientoLote
{
    private MovimientoLote()
    {
    }

    internal MovimientoLote(int orden, DateOnly fecha, TipoMovimientoLote tipo, int plantas, FasePlanta fase, string? ubicacion, string? concepto, Guid? documentoId,
        Guid? anulaId, DateTimeOffset ahora)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Fecha = fecha;
        Tipo = tipo;
        Plantas = plantas;
        Fase = fase;
        Ubicacion = ubicacion;
        Concepto = concepto;
        DocumentoId = documentoId;
        AnulaId = anulaId;
        CreadoEn = ahora;
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public DateOnly Fecha { get; private set; }

    public TipoMovimientoLote Tipo { get; private set; }

    /// <summary>Plantas con signo: positivas las que entran (siembra, anulación de una salida), negativas las que salen.</summary>
    public int Plantas { get; private set; }

    /// <summary>Fase del lote después del movimiento.</summary>
    public FasePlanta Fase { get; private set; }

    public string? Ubicacion { get; private set; }

    public string? Concepto { get; private set; }

    /// <summary>Encargo de la entrega (o el lote, en los demás).</summary>
    public Guid? DocumentoId { get; private set; }

    /// <summary>Movimiento que anula (en las anulaciones).</summary>
    public Guid? AnulaId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }
}

/// <summary>
/// Lote de planta del vivero: especie (nombre botánico), variedad, portainjerto, artículo con que se vende, fecha de
/// siembra, fase, ubicación y plantas vivas. El código del lote es el de trazabilidad del pasaporte fitosanitario.
/// Su libro de movimientos explica cada planta: lo sembrado, las bajas, lo entregado y lo pasado a existencias.
/// </summary>
public sealed class LotePlanta : RaizAgregadoEmpresa<Guid>
{
    private readonly List<MovimientoLote> _movimientos = [];

    private LotePlanta(Guid id)
        : base(id, Guid.Empty)
    {
        Especie = null!;
    }

    private LotePlanta(Guid id, Guid empresaId, int numero, DateOnly fecha, string especie, Guid productoId)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        FechaSiembra = fecha;
        Especie = especie;
        ProductoId = productoId;
        Fase = FasePlanta.Semillero;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    /// <summary>Código del lote: el de trazabilidad (letra C) del pasaporte fitosanitario.</summary>
    public string Codigo => $"LP{Ejercicio}-{Numero:0000}";

    /// <summary>Especie con su nombre botánico (letra A del pasaporte).</summary>
    public string Especie { get; private set; }

    public string? Variedad { get; private set; }

    public string? Portainjerto { get; private set; }

    /// <summary>Artículo con que se vende la planta.</summary>
    public Guid ProductoId { get; private set; }

    /// <summary>Origen de la semilla o del material vegetal (proveedor y lote).</summary>
    public string? OrigenMaterial { get; private set; }

    public DateOnly FechaSiembra { get; private set; }

    public DateOnly? FechaPrevistaLista { get; private set; }

    public FasePlanta Fase { get; private set; }

    public string? Ubicacion { get; private set; }

    public int PlantasIniciales { get; private set; }

    public int PlantasVivas { get; private set; }

    public string? Observaciones { get; private set; }

    public bool Anulado { get; private set; }

    public IReadOnlyList<MovimientoLote> Movimientos => _movimientos;

    public bool Terminado => !Anulado && PlantasVivas == 0;

    public static Resultado<LotePlanta> Crear(Guid empresaId, int numero, DateOnly fecha, string? especie, string? variedad, string? portainjerto, Guid productoId,
        int plantas, FasePlanta fase, string? ubicacion, string? origenMaterial, DateOnly? prevista, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var e = ReglasVivero.Texto(especie);
        if (e is null || e.Length > ReglasVivero.LongitudTexto)
        {
            return Resultado.Fallo<LotePlanta>(Error.Validacion("lote_planta.especie", "Indica la especie (nombre botánico)."));
        }

        if (plantas <= 0)
        {
            return Resultado.Fallo<LotePlanta>(Error.Validacion("lote_planta.plantas", "Las plantas del lote deben ser positivas."));
        }

        if (!Enum.IsDefined(fase))
        {
            return Resultado.Fallo<LotePlanta>(Error.Validacion("lote_planta.fase", "Fase no válida."));
        }

        var l = new LotePlanta(Guid.NewGuid(), empresaId, numero, fecha, e, productoId) { Fase = fase };
        var r = l.Cambiar(variedad, portainjerto, origenMaterial, prevista, observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LotePlanta>(r.Error);
        }

        var u = ReglasVivero.Texto(ubicacion);
        if (ReglasVivero.TextoValido(u, ReglasVivero.LongitudTexto, "lote_planta.ubicacion", "La ubicación") is { EsFallo: true } ru)
        {
            return Resultado.Fallo<LotePlanta>(ru.Error);
        }

        l.Ubicacion = u;
        l.PlantasIniciales = plantas;
        l.Anotar(fecha, TipoMovimientoLote.Siembra, plantas, "Siembra", null, null, reloj);
        return Resultado.Ok(l);
    }

    public Resultado Cambiar(string? variedad, string? portainjerto, string? origenMaterial, DateOnly? prevista, string? observaciones)
    {
        var v = ReglasVivero.Texto(variedad);
        var p = ReglasVivero.Texto(portainjerto);
        var o = ReglasVivero.Texto(origenMaterial);
        var obs = ReglasVivero.Texto(observaciones);
        foreach (var r in new[]
                 {
                     ReglasVivero.TextoValido(v, ReglasVivero.LongitudTexto, "lote_planta.variedad", "La variedad"),
                     ReglasVivero.TextoValido(p, ReglasVivero.LongitudTexto, "lote_planta.portainjerto", "El portainjerto"),
                     ReglasVivero.TextoValido(o, ReglasVivero.LongitudTexto, "lote_planta.origen", "El origen del material"),
                     ReglasVivero.TextoValido(obs, ReglasVivero.LongitudObservaciones, "lote_planta.observaciones", "Las observaciones"),
                 })
        {
            if (r.EsFallo)
            {
                return r;
            }
        }

        if (prevista is { } f && f < FechaSiembra)
        {
            return Resultado.Fallo(Error.Validacion("lote_planta.prevista", "La fecha prevista no puede ser anterior a la siembra."));
        }

        Variedad = v;
        Portainjerto = p;
        OrigenMaterial = o;
        FechaPrevistaLista = prevista;
        Observaciones = obs;
        return Resultado.Ok();
    }

    /// <summary>Avanza de fase (nunca hacia atrás) o cambia de ubicación.</summary>
    public Resultado Avanzar(DateOnly fecha, FasePlanta? fase, string? ubicacion, IReloj reloj)
    {
        if (Vivo() is { EsFallo: true } r)
        {
            return r;
        }

        var nueva = fase ?? Fase;
        if (!Enum.IsDefined(nueva) || nueva < Fase)
        {
            return Resultado.Fallo(Error.Validacion("lote_planta.fase", $"El lote está en {Fase}: la fase solo avanza."));
        }

        var u = ReglasVivero.Texto(ubicacion) ?? Ubicacion;
        if (ReglasVivero.TextoValido(u, ReglasVivero.LongitudTexto, "lote_planta.ubicacion", "La ubicación") is { EsFallo: true } ru)
        {
            return ru;
        }

        if (nueva == Fase && string.Equals(u, Ubicacion, StringComparison.Ordinal))
        {
            return Resultado.Fallo(Error.Validacion("lote_planta.sin_cambio", "Indica la nueva fase o la nueva ubicación."));
        }

        var texto = nueva != Fase ? $"De {Fase} a {nueva}" : $"Traslado a {u}";
        Fase = nueva;
        Ubicacion = u;
        Anotar(fecha, TipoMovimientoLote.Cambio, 0, texto, null, null, reloj);
        return Resultado.Ok();
    }

    /// <summary>Plantas que salen del lote (baja, entrega o paso a existencias), sin tocar las reservadas a otros encargos.</summary>
    public Resultado<MovimientoLote> Sacar(DateOnly fecha, TipoMovimientoLote tipo, int plantas, int reservadasOtros, string? concepto, Guid? documentoId, IReloj reloj)
    {
        if (Vivo() is { EsFallo: true } r)
        {
            return Resultado.Fallo<MovimientoLote>(r.Error);
        }

        if (tipo is not (TipoMovimientoLote.Baja or TipoMovimientoLote.Entrega or TipoMovimientoLote.PasoExistencias))
        {
            return Resultado.Fallo<MovimientoLote>(Error.Validacion("lote_planta.tipo", "Movimiento de salida no válido."));
        }

        if (plantas <= 0)
        {
            return Resultado.Fallo<MovimientoLote>(Error.Validacion("lote_planta.plantas", "Las plantas deben ser positivas."));
        }

        if (tipo != TipoMovimientoLote.Baja && Fase != FasePlanta.Lista)
        {
            return Resultado.Fallo<MovimientoLote>(Error.Conflicto("lote_planta.no_lista", $"El lote {Codigo} está en {Fase}: solo se entrega o se vende cuando está lista."));
        }

        var disponibles = PlantasVivas - reservadasOtros;
        if (plantas > PlantasVivas || (tipo != TipoMovimientoLote.Baja && plantas > disponibles))
        {
            return Resultado.Fallo<MovimientoLote>(Error.Conflicto("lote_planta.sin_plantas",
                $"El lote {Codigo} tiene {PlantasVivas} plantas vivas ({Math.Max(0, disponibles)} sin reservar) y salen {plantas}."));
        }

        if (tipo == TipoMovimientoLote.Baja && ReglasVivero.Texto(concepto) is null)
        {
            return Resultado.Fallo<MovimientoLote>(Error.Validacion("lote_planta.motivo", "Indica el motivo de la baja (mortandad, descarte…)."));
        }

        return Resultado.Ok(Anotar(fecha, tipo, -plantas, concepto, documentoId, null, reloj));
    }

    /// <summary>Anula una baja, una entrega o un paso a existencias: las plantas vuelven al lote.</summary>
    public Resultado<MovimientoLote> AnularMovimiento(Guid movimientoId, DateOnly fecha, string? motivo, IReloj reloj)
    {
        if (Anulado)
        {
            return Resultado.Fallo<MovimientoLote>(Error.Conflicto("lote_planta.anulado", "El lote está anulado."));
        }

        var m = _movimientos.SingleOrDefault(x => x.Id == movimientoId);
        if (m is null || m.Tipo is not (TipoMovimientoLote.Baja or TipoMovimientoLote.Entrega or TipoMovimientoLote.PasoExistencias))
        {
            return Resultado.Fallo<MovimientoLote>(Error.NoEncontrado("lote_planta.movimiento", "Solo se anulan bajas, entregas y pasos a existencias del lote."));
        }

        if (_movimientos.Any(x => x.AnulaId == movimientoId))
        {
            return Resultado.Fallo<MovimientoLote>(Error.Conflicto("lote_planta.ya_anulado", "Ese movimiento ya está anulado."));
        }

        var texto = ReglasVivero.Texto(motivo);
        if (texto is null)
        {
            return Resultado.Fallo<MovimientoLote>(Error.Validacion("lote_planta.motivo", "Indica el motivo de la anulación."));
        }

        return Resultado.Ok(Anotar(fecha, TipoMovimientoLote.Anulacion, -m.Plantas, $"Anula {m.Tipo}: {texto}", m.DocumentoId, m.Id, reloj));
    }

    /// <summary>Anula el lote entero si no ha tenido más que la siembra y cambios de fase.</summary>
    public Resultado Anular()
    {
        if (Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("lote_planta.anulado", "El lote ya está anulado."));
        }

        if (_movimientos.Any(m => m.Plantas != 0 && m.Tipo != TipoMovimientoLote.Siembra))
        {
            return Resultado.Fallo(Error.Conflicto("lote_planta.con_movimientos", "El lote ya tiene bajas, entregas o ventas: no se anula."));
        }

        Anulado = true;
        return Resultado.Ok();
    }

    public bool MovimientoAnulado(Guid movimientoId) => _movimientos.Any(x => x.AnulaId == movimientoId);

    private Resultado Vivo() => Anulado
        ? Resultado.Fallo(Error.Conflicto("lote_planta.anulado", "El lote está anulado."))
        : Resultado.Ok();

    private MovimientoLote Anotar(DateOnly fecha, TipoMovimientoLote tipo, int plantas, string? concepto, Guid? documentoId, Guid? anulaId, IReloj reloj)
    {
        var texto = ReglasVivero.Texto(concepto);
        if (texto?.Length > 200)
        {
            texto = texto[..200];
        }

        var m = new MovimientoLote(_movimientos.Count + 1, fecha, tipo, plantas, Fase, Ubicacion, texto, documentoId, anulaId, reloj.AhoraUtc);
        _movimientos.Add(m);
        PlantasVivas += plantas;
        return m;
    }
}

public enum EstadoEncargo
{
    /// <summary>Pedido, sin lote.</summary>
    Pendiente = 1,

    /// <summary>Con un lote que le reserva las plantas.</summary>
    Reservado = 2,

    /// <summary>Entregado con su albarán.</summary>
    Servido = 3,

    Anulado = 4,
}

/// <summary>
/// Encargo de planta de un cliente: artículo, plantas, fecha de entrega y precio. Se le reserva un lote (sus plantas no
/// se entregan a otros) y se sirve con un albarán, que lleva el pasaporte fitosanitario del lote.
/// </summary>
public sealed class EncargoPlanta : RaizAgregadoEmpresa<Guid>
{
    private EncargoPlanta(Guid id)
        : base(id, Guid.Empty)
    {
        ClienteNombre = null!;
    }

    private EncargoPlanta(Guid id, Guid empresaId, int numero, DateOnly fecha, Guid clienteId, string cliente, Guid productoId, int plantas, DateOnly entrega,
        decimal? precio, string? observaciones)
        : base(id, empresaId)
    {
        Ejercicio = fecha.Year;
        Numero = numero;
        Fecha = fecha;
        ClienteId = clienteId;
        ClienteNombre = cliente;
        ProductoId = productoId;
        Plantas = plantas;
        FechaEntrega = entrega;
        PrecioPlanta = precio;
        Observaciones = observaciones;
        Estado = EstadoEncargo.Pendiente;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"EP{Ejercicio}/{Numero:0000}";

    public DateOnly Fecha { get; private set; }

    public Guid ClienteId { get; private set; }

    public string ClienteNombre { get; private set; }

    public Guid ProductoId { get; private set; }

    public int Plantas { get; private set; }

    public DateOnly FechaEntrega { get; private set; }

    /// <summary>Precio por planta (null: el de la tarifa del cliente o el del artículo).</summary>
    public decimal? PrecioPlanta { get; private set; }

    public string? Observaciones { get; private set; }

    public EstadoEncargo Estado { get; private set; }

    public Guid? LoteId { get; private set; }

    public Guid? MovimientoId { get; private set; }

    public Guid? AlbaranId { get; private set; }

    public string? AlbaranNumero { get; private set; }

    public static Resultado<EncargoPlanta> Crear(Guid empresaId, int numero, DateOnly fecha, Guid clienteId, string cliente, Guid productoId, int plantas, DateOnly entrega,
        decimal? precio, string? observaciones)
    {
        if (plantas <= 0)
        {
            return Resultado.Fallo<EncargoPlanta>(Error.Validacion("encargo.plantas", "Las plantas del encargo deben ser positivas."));
        }

        if (precio is < 0m)
        {
            return Resultado.Fallo<EncargoPlanta>(Error.Validacion("encargo.precio", "El precio no puede ser negativo."));
        }

        if (entrega < fecha)
        {
            return Resultado.Fallo<EncargoPlanta>(Error.Validacion("encargo.entrega", "La entrega no puede ser anterior al encargo."));
        }

        var o = ReglasVivero.Texto(observaciones);
        if (ReglasVivero.TextoValido(o, ReglasVivero.LongitudObservaciones, "encargo.observaciones", "Las observaciones") is { EsFallo: true } r)
        {
            return Resultado.Fallo<EncargoPlanta>(r.Error);
        }

        return Resultado.Ok(new EncargoPlanta(Guid.NewGuid(), empresaId, numero, fecha, clienteId, cliente, productoId, plantas, entrega, precio, o));
    }

    public Resultado Reservar(LotePlanta lote, int reservadasOtros)
    {
        ArgumentNullException.ThrowIfNull(lote);
        if (Estado is not (EstadoEncargo.Pendiente or EstadoEncargo.Reservado))
        {
            return Resultado.Fallo(Error.Conflicto("encargo.cerrado", "El encargo ya está servido o anulado."));
        }

        if (lote.Anulado || lote.ProductoId != ProductoId)
        {
            return Resultado.Fallo(Error.Validacion("encargo.lote", "El lote no es del artículo del encargo (o está anulado)."));
        }

        if (lote.PlantasVivas - reservadasOtros < Plantas)
        {
            return Resultado.Fallo(Error.Conflicto("encargo.sin_plantas",
                $"El lote {lote.Codigo} tiene {Math.Max(0, lote.PlantasVivas - reservadasOtros)} plantas sin reservar y el encargo pide {Plantas}."));
        }

        LoteId = lote.Id;
        Estado = EstadoEncargo.Reservado;
        return Resultado.Ok();
    }

    public Resultado Liberar()
    {
        if (Estado != EstadoEncargo.Reservado)
        {
            return Resultado.Fallo(Error.Conflicto("encargo.sin_reserva", "El encargo no tiene reserva."));
        }

        LoteId = null;
        Estado = EstadoEncargo.Pendiente;
        return Resultado.Ok();
    }

    public void Servido(Guid movimientoId, Guid albaranId, string numero)
    {
        MovimientoId = movimientoId;
        AlbaranId = albaranId;
        AlbaranNumero = numero;
        Estado = EstadoEncargo.Servido;
    }

    /// <summary>Anula el encargo. Servido, la entrega se deshace aparte (devuelve las plantas al lote y anula el albarán).</summary>
    public Resultado Anular()
    {
        if (Estado == EstadoEncargo.Anulado)
        {
            return Resultado.Fallo(Error.Conflicto("encargo.anulado", "El encargo ya está anulado."));
        }

        if (Estado != EstadoEncargo.Servido)
        {
            LoteId = null;
        }

        Estado = EstadoEncargo.Anulado;
        return Resultado.Ok();
    }
}
