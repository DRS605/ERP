using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Cómo se subasta: a la baja (reloj que baja desde el precio de salida) o al alza (pujas que suben).</summary>
public enum TipoSubasta
{
    /// <summary>Reloj a la baja (subasta holandesa): se adjudica al primero que para el reloj, como mucho al precio de salida.</summary>
    Baja = 1,

    /// <summary>Pujas al alza: se adjudica a la mejor puja, como poco al precio de salida.</summary>
    Alza = 2,
}

public enum EstadoSesionSubasta
{
    /// <summary>Se están subastando sus lotes.</summary>
    Abierta = 1,

    /// <summary>Cerrada: lo adjudicado salió con un albarán por comprador.</summary>
    Cerrada = 2,

    Anulada = 3,
}

public enum EstadoLoteSubasta
{
    /// <summary>Aún sin subastar.</summary>
    Pendiente = 1,

    Adjudicado = 2,

    /// <summary>Sin comprador (o retirado): la fruta sigue en la partida.</summary>
    Desierto = 3,
}

/// <summary>Puja de un comprador por un lote (en las subastas al alza).</summary>
public sealed class PujaSubasta
{
    private PujaSubasta()
    {
        CompradorNombre = null!;
    }

    internal PujaSubasta(Guid loteId, Guid compradorId, string compradorNombre, decimal precioKg, DateTimeOffset ahora)
    {
        Id = Guid.NewGuid();
        LoteId = loteId;
        CompradorId = compradorId;
        CompradorNombre = compradorNombre;
        PrecioKg = precioKg;
        CreadaEn = ahora;
    }

    public Guid Id { get; private set; }

    public Guid LoteId { get; private set; }

    public Guid CompradorId { get; private set; }

    public string CompradorNombre { get; private set; }

    public decimal PrecioKg { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }
}

/// <summary>
/// Lote de la sesión: kilos sueltos de una partida (con su agricultor) que se subastan juntos. Adjudicado, lleva el
/// comprador, el precio por kilo y el importe; al cerrar la sesión, el albarán del comprador.
/// </summary>
public sealed class LoteSubasta
{
    public const int LongitudDescripcion = 200;

    private LoteSubasta()
    {
        Descripcion = null!;
    }

    internal LoteSubasta(int orden, Guid partidaId, string partidaCodigo, Guid? agricultorId, Guid productoId, string descripcion, decimal kilos, int envases,
        decimal? precioSalida)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        PartidaId = partidaId;
        PartidaCodigo = partidaCodigo;
        AgricultorId = agricultorId;
        ProductoId = productoId;
        Descripcion = descripcion;
        Kilos = kilos;
        Envases = envases;
        PrecioSalida = precioSalida;
        Estado = EstadoLoteSubasta.Pendiente;
    }

    public Guid Id { get; private set; }

    /// <summary>Número del lote en la sesión (1, 2…), el que se canta en la subasta.</summary>
    public int Orden { get; private set; }

    public Guid PartidaId { get; private set; }

    public string? PartidaCodigo { get; private set; }

    public Guid? AgricultorId { get; private set; }

    public Guid ProductoId { get; private set; }

    public string Descripcion { get; private set; }

    public decimal Kilos { get; private set; }

    /// <summary>Bultos del lote (cajas, palots…), informativo.</summary>
    public int Envases { get; private set; }

    /// <summary>Precio de salida por kilo: el máximo en la subasta a la baja y el mínimo en la de al alza.</summary>
    public decimal? PrecioSalida { get; private set; }

    public EstadoLoteSubasta Estado { get; private set; }

    public Guid? CompradorId { get; private set; }

    public string? CompradorNombre { get; private set; }

    public decimal? PrecioKg { get; private set; }

    /// <summary>Kilos × precio, al céntimo.</summary>
    public decimal Importe { get; private set; }

    public DateTimeOffset? AdjudicadoEn { get; private set; }

    /// <summary>Albarán de venta al comprador (al cerrar la sesión).</summary>
    public Guid? AlbaranId { get; private set; }

    public string? AlbaranNumero { get; private set; }

    internal void Adjudicar(Guid compradorId, string compradorNombre, decimal precioKg, DateTimeOffset ahora)
    {
        Estado = EstadoLoteSubasta.Adjudicado;
        CompradorId = compradorId;
        CompradorNombre = compradorNombre;
        PrecioKg = precioKg;
        Importe = Redondeo.Dos(Kilos * precioKg);
        AdjudicadoEn = ahora;
    }

    internal void Desierto()
    {
        Estado = EstadoLoteSubasta.Desierto;
        CompradorId = null;
        CompradorNombre = null;
        PrecioKg = null;
        Importe = 0m;
        AdjudicadoEn = null;
    }

    internal void Reabrir()
    {
        Desierto();
        Estado = EstadoLoteSubasta.Pendiente;
    }

    internal void Albaran(Guid? albaranId, string? numero)
    {
        AlbaranId = albaranId;
        AlbaranNumero = numero;
    }
}

/// <summary>
/// Sesión de subasta (alhóndiga): un día de subasta con sus lotes, sacados de los kilos sueltos de las partidas que han
/// entrado. Cada lote se adjudica a un comprador (cliente) a un precio por kilo —al reloj a la baja o a la mejor puja al
/// alza— o queda desierto. Al cerrarla, lo adjudicado sale de sus partidas con un albarán de venta por comprador, y el
/// precio de cada lote es el que se paga al agricultor en su liquidación. Cerrada, solo se anula entera.
/// </summary>
public sealed class SesionSubasta : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudObservaciones = 500;
    public const int MaximoLotes = 2000;

    private readonly List<LoteSubasta> _lotes = [];
    private readonly List<PujaSubasta> _pujas = [];

    private SesionSubasta(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private SesionSubasta(Guid id, Guid empresaId, int ejercicio, int numero, DateOnly fecha, TipoSubasta tipo, string? observaciones, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        Fecha = fecha;
        Tipo = tipo;
        Observaciones = observaciones;
        Estado = EstadoSesionSubasta.Abierta;
        CreadaEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string NumeroCompleto => $"SU{Ejercicio}/{Numero:0000}";

    public DateOnly Fecha { get; private set; }

    public TipoSubasta Tipo { get; private set; }

    public EstadoSesionSubasta Estado { get; private set; }

    public string? Observaciones { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public DateTimeOffset? CerradaEn { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public IReadOnlyList<LoteSubasta> Lotes => _lotes;

    public IReadOnlyList<PujaSubasta> Pujas => _pujas;

    public static Resultado<SesionSubasta> Crear(Guid empresaId, int numero, DateOnly fecha, TipoSubasta tipo, string? observaciones, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<SesionSubasta>(Error.Validacion("subasta.tipo", "El tipo de subasta es a la baja o al alza."));
        }

        var texto = Texto(observaciones);
        if (texto?.Length > LongitudObservaciones)
        {
            return Resultado.Fallo<SesionSubasta>(Error.Validacion("subasta.observaciones", $"Las observaciones admiten hasta {LongitudObservaciones} caracteres."));
        }

        return Resultado.Ok(new SesionSubasta(Guid.NewGuid(), empresaId, fecha.Year, numero, fecha, tipo, texto, reloj.AhoraUtc));
    }

    public Resultado Cambiar(DateOnly fecha, TipoSubasta tipo, string? observaciones)
    {
        if (Abierta() is { EsFallo: true } r)
        {
            return r;
        }

        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo(Error.Validacion("subasta.tipo", "El tipo de subasta es a la baja o al alza."));
        }

        if (tipo != Tipo && _pujas.Count > 0)
        {
            return Resultado.Fallo(Error.Conflicto("subasta.con_pujas", "La sesión ya tiene pujas: no se cambia el tipo de subasta."));
        }

        if (fecha.Year != Ejercicio)
        {
            return Resultado.Fallo(Error.Validacion("subasta.fecha", "La fecha no puede cambiar de año (cambiaría la numeración)."));
        }

        var texto = Texto(observaciones);
        if (texto?.Length > LongitudObservaciones)
        {
            return Resultado.Fallo(Error.Validacion("subasta.observaciones", $"Las observaciones admiten hasta {LongitudObservaciones} caracteres."));
        }

        Fecha = fecha;
        Tipo = tipo;
        Observaciones = texto;
        return Resultado.Ok();
    }

    public Resultado<LoteSubasta> AgregarLote(Guid partidaId, string partidaCodigo, Guid? agricultorId, Guid productoId, string descripcion, decimal kilos, int envases,
        decimal? precioSalida)
    {
        if (Abierta() is { EsFallo: true } r)
        {
            return Resultado.Fallo<LoteSubasta>(r.Error);
        }

        if (_lotes.Count >= MaximoLotes)
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.lotes", $"Una sesión admite hasta {MaximoLotes} lotes."));
        }

        if (kilos <= 0m || decimal.Round(kilos, 3) != kilos)
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.kilos", "Los kilos del lote deben ser positivos (hasta 3 decimales)."));
        }

        if (envases < 0)
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.envases", "Los bultos no pueden ser negativos."));
        }

        if (precioSalida is < 0m || (precioSalida is { } p && decimal.Round(p, 6) != p))
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.precio_salida", "El precio de salida no puede ser negativo (hasta 6 decimales)."));
        }

        var texto = Texto(descripcion) ?? partidaCodigo;
        if (texto.Length > LoteSubasta.LongitudDescripcion)
        {
            texto = texto[..LoteSubasta.LongitudDescripcion];
        }

        var lote = new LoteSubasta(_lotes.Count == 0 ? 1 : _lotes.Max(l => l.Orden) + 1, partidaId, partidaCodigo, agricultorId, productoId, texto, kilos, envases,
            precioSalida);
        _lotes.Add(lote);
        return Resultado.Ok(lote);
    }

    public Resultado QuitarLote(Guid loteId)
    {
        if (Abierta() is { EsFallo: true } r)
        {
            return r;
        }

        var lote = _lotes.SingleOrDefault(l => l.Id == loteId);
        if (lote is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("subasta.lote_no_encontrado", "El lote no es de esta sesión."));
        }

        if (lote.Estado != EstadoLoteSubasta.Pendiente || _pujas.Any(p => p.LoteId == loteId))
        {
            return Resultado.Fallo(Error.Conflicto("subasta.lote_subastado", "El lote ya tiene pujas o se ha subastado: deshaz antes la adjudicación."));
        }

        _lotes.Remove(lote);
        return Resultado.Ok();
    }

    /// <summary>Puja al alza: por encima de la mejor puja del lote y, como poco, al precio de salida.</summary>
    public Resultado<PujaSubasta> Pujar(Guid loteId, Guid compradorId, string compradorNombre, decimal precioKg, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var lote = LotePendiente(loteId);
        if (lote.EsFallo)
        {
            return Resultado.Fallo<PujaSubasta>(lote.Error);
        }

        if (Tipo != TipoSubasta.Alza)
        {
            return Resultado.Fallo<PujaSubasta>(Error.Validacion("subasta.sin_pujas", "En la subasta a la baja no hay pujas: se adjudica al que para el reloj."));
        }

        if (Precio(precioKg) is { EsFallo: true } p)
        {
            return Resultado.Fallo<PujaSubasta>(p.Error);
        }

        if (lote.Valor.PrecioSalida is { } salida && precioKg < salida)
        {
            return Resultado.Fallo<PujaSubasta>(Error.Validacion("subasta.bajo_salida", $"La puja no llega al precio de salida ({Redondeo.Formatear(salida)} €/kg)."));
        }

        if (MejorPuja(loteId) is { } mejor && precioKg <= mejor.PrecioKg)
        {
            return Resultado.Fallo<PujaSubasta>(Error.Validacion("subasta.puja_baja", $"Hay que superar la mejor puja ({Redondeo.Formatear(mejor.PrecioKg)} €/kg)."));
        }

        var puja = new PujaSubasta(loteId, compradorId, compradorNombre, precioKg, reloj.AhoraUtc);
        _pujas.Add(puja);
        return Resultado.Ok(puja);
    }

    public PujaSubasta? MejorPuja(Guid loteId) =>
        _pujas.Where(p => p.LoteId == loteId).OrderByDescending(p => p.PrecioKg).ThenBy(p => p.CreadaEn).FirstOrDefault();

    /// <summary>
    /// Adjudica el lote. A la baja, al comprador y precio indicados (como mucho el de salida). Al alza, sin indicar nada, a
    /// la mejor puja; con comprador y precio, a mano (no por debajo de la mejor puja ni del precio de salida).
    /// </summary>
    public Resultado<LoteSubasta> Adjudicar(Guid loteId, Guid? compradorId, string? compradorNombre, decimal? precioKg, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var lote = LotePendiente(loteId);
        if (lote.EsFallo)
        {
            return lote;
        }

        var l = lote.Valor;
        if (Tipo == TipoSubasta.Alza && compradorId is null && precioKg is null)
        {
            if (MejorPuja(loteId) is not { } mejor)
            {
                return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.sin_pujas", "El lote no tiene pujas: adjudícalo a mano o déjalo desierto."));
            }

            l.Adjudicar(mejor.CompradorId, mejor.CompradorNombre, mejor.PrecioKg, reloj.AhoraUtc);
            return Resultado.Ok(l);
        }

        if (compradorId is not { } comprador || precioKg is not { } precio || string.IsNullOrWhiteSpace(compradorNombre))
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.adjudicacion", "Indica el comprador y el precio por kilo."));
        }

        if (Precio(precio) is { EsFallo: true } p)
        {
            return Resultado.Fallo<LoteSubasta>(p.Error);
        }

        if (l.PrecioSalida is { } salida && (Tipo == TipoSubasta.Baja ? precio > salida : precio < salida))
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.precio_salida", Tipo == TipoSubasta.Baja
                ? $"A la baja no se adjudica por encima del precio de salida ({Redondeo.Formatear(salida)} €/kg)."
                : $"Al alza no se adjudica por debajo del precio de salida ({Redondeo.Formatear(salida)} €/kg)."));
        }

        if (Tipo == TipoSubasta.Alza && MejorPuja(loteId) is { } m && precio < m.PrecioKg)
        {
            return Resultado.Fallo<LoteSubasta>(Error.Validacion("subasta.puja_baja", $"Hay una puja mejor ({Redondeo.Formatear(m.PrecioKg)} €/kg)."));
        }

        l.Adjudicar(comprador, compradorNombre.Trim(), precio, reloj.AhoraUtc);
        return Resultado.Ok(l);
    }

    public Resultado DejarDesierto(Guid loteId)
    {
        var lote = LotePendiente(loteId);
        if (lote.EsFallo)
        {
            return Resultado.Fallo(lote.Error);
        }

        lote.Valor.Desierto();
        return Resultado.Ok();
    }

    /// <summary>Vuelve a dejar pendiente un lote adjudicado o desierto (mientras la sesión está abierta).</summary>
    public Resultado Deshacer(Guid loteId)
    {
        if (Abierta() is { EsFallo: true } r)
        {
            return r;
        }

        var lote = _lotes.SingleOrDefault(l => l.Id == loteId);
        if (lote is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("subasta.lote_no_encontrado", "El lote no es de esta sesión."));
        }

        if (lote.Estado == EstadoLoteSubasta.Pendiente)
        {
            return Resultado.Fallo(Error.Conflicto("subasta.lote_pendiente", "El lote aún no se ha subastado."));
        }

        lote.Reabrir();
        return Resultado.Ok();
    }

    /// <summary>Cierra la sesión: lo que quede sin subastar queda desierto. Hace falta algún lote adjudicado.</summary>
    public Resultado Cerrar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Abierta() is { EsFallo: true } r)
        {
            return r;
        }

        if (!_lotes.Any(l => l.Estado == EstadoLoteSubasta.Adjudicado))
        {
            return Resultado.Fallo(Error.Validacion("subasta.nada_adjudicado", "No hay ningún lote adjudicado: sin ventas, anula la sesión."));
        }

        foreach (var l in _lotes.Where(l => l.Estado == EstadoLoteSubasta.Pendiente))
        {
            l.Desierto();
        }

        Estado = EstadoSesionSubasta.Cerrada;
        CerradaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Anota el albarán de los lotes de un comprador (al cerrar) o lo quita (al anular).</summary>
    public void AsignarAlbaran(Guid compradorId, Guid? albaranId, string? numero)
    {
        foreach (var l in _lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado && l.CompradorId == compradorId))
        {
            l.Albaran(albaranId, numero);
        }
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado == EstadoSesionSubasta.Anulada)
        {
            return Resultado.Fallo(Error.Conflicto("subasta.anulada", "La sesión ya está anulada."));
        }

        var texto = Texto(motivo);
        if (texto is null)
        {
            return Resultado.Fallo(Error.Validacion("subasta.motivo", "Indica el motivo de la anulación."));
        }

        Estado = EstadoSesionSubasta.Anulada;
        MotivoAnulacion = texto.Length > 200 ? texto[..200] : texto;
        return Resultado.Ok();
    }

    private Resultado Abierta() => Estado == EstadoSesionSubasta.Abierta
        ? Resultado.Ok()
        : Resultado.Fallo(Error.Conflicto("subasta.cerrada", "La sesión ya está cerrada o anulada."));

    private Resultado<LoteSubasta> LotePendiente(Guid loteId)
    {
        if (Abierta() is { EsFallo: true } r)
        {
            return Resultado.Fallo<LoteSubasta>(r.Error);
        }

        var lote = _lotes.SingleOrDefault(l => l.Id == loteId);
        if (lote is null)
        {
            return Resultado.Fallo<LoteSubasta>(Error.NoEncontrado("subasta.lote_no_encontrado", "El lote no es de esta sesión."));
        }

        return lote.Estado == EstadoLoteSubasta.Pendiente
            ? Resultado.Ok(lote)
            : Resultado.Fallo<LoteSubasta>(Error.Conflicto("subasta.lote_subastado", "El lote ya se ha subastado: deshaz antes la adjudicación."));
    }

    private static Resultado Precio(decimal precioKg) => precioKg <= 0m || decimal.Round(precioKg, 6) != precioKg
        ? Resultado.Fallo(Error.Validacion("subasta.precio", "El precio por kilo debe ser positivo (hasta 6 decimales)."))
        : Resultado.Ok();

    private static string? Texto(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

/// <summary>
/// Precio de subasta de cada partida para la liquidación al agricultor: la media ponderada de lo adjudicado de la partida
/// en las sesiones cerradas (importe entre kilos). Una partida con lotes en una sesión abierta está pendiente.
/// </summary>
public static class PrecioSubastaPartida
{
    public static IReadOnlyDictionary<Guid, PrecioAplicable> Calcular(IEnumerable<SesionSubasta> sesiones)
    {
        ArgumentNullException.ThrowIfNull(sesiones);
        return sesiones.Where(s => s.Estado == EstadoSesionSubasta.Cerrada)
            .SelectMany(s => s.Lotes.Where(l => l.Estado == EstadoLoteSubasta.Adjudicado).Select(l => (Sesion: s, Lote: l)))
            .GroupBy(x => x.Lote.PartidaId)
            .ToDictionary(g => g.Key, g =>
            {
                var kilos = g.Sum(x => x.Lote.Kilos);
                // El id del precio es el de la última sesión: mientras una liquidación lo use, la sesión no se anula.
                var ultima = g.OrderByDescending(x => x.Sesion.Fecha).ThenByDescending(x => x.Sesion.Numero).First().Sesion;
                return new PrecioAplicable(ultima.Id, decimal.Round(g.Sum(x => x.Lote.Importe) / kilos, 6), Subasta: true);
            });
    }
}
