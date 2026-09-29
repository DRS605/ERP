using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>
/// Rectificación de una línea de recepción ya confirmada (y quizá ya usada): corrige su neto real, sus kilos de
/// liquidación o ambos, sobre la misma partida y los mismos palés, con un movimiento compensatorio. Nunca se borra ni se
/// rehace la recepción: la genealogía y lo vendido siguen apuntando a la misma partida. Es de solo inserción.
/// </summary>
public sealed class RectificacionRecepcion : RaizAgregadoEmpresa<Guid>
{
    private RectificacionRecepcion(Guid id)
        : base(id, Guid.Empty)
    {
        Motivo = null!;
    }

    private RectificacionRecepcion(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Motivo = null!;
    }

    public Guid RecepcionId { get; private set; }

    public Guid LineaRecepcionId { get; private set; }

    public Guid PartidaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    /// <summary>Neto real antes de rectificar (el de la recepción más las rectificaciones anteriores).</summary>
    public decimal NetoAnteriorKg { get; private set; }

    /// <summary>Kilos que se suman (o restan) al neto real de la partida.</summary>
    public decimal DiferenciaKg { get; private set; }

    /// <summary>Kilos de liquidación nuevos, si también cambian.</summary>
    public decimal? KilosLiquidacion { get; private set; }

    /// <summary>La nueva pesada que justifica la corrección (bruto y tara), si la hay.</summary>
    public decimal? BrutoKg { get; private set; }

    public decimal? TaraKg { get; private set; }

    public string Motivo { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public decimal NetoNuevoKg => NetoAnteriorKg + DiferenciaKg;

    public static Resultado<RectificacionRecepcion> Crear(Guid empresaId, Guid recepcionId, Guid lineaId, Guid partidaId, DateOnly fecha, decimal netoAnterior,
        decimal? netoNuevo, decimal? kilosLiquidacion, decimal? bruto, decimal? tara, string? motivo, Guid? usuarioId, DateTimeOffset ahora)
    {
        if (string.IsNullOrWhiteSpace(motivo))
        {
            return Resultado.Fallo<RectificacionRecepcion>(Error.Validacion("rectificacion.motivo", "Indica el motivo de la rectificación."));
        }

        if (netoNuevo is null && kilosLiquidacion is null)
        {
            return Resultado.Fallo<RectificacionRecepcion>(Error.Validacion("rectificacion.nada", "Indica el neto correcto o los kilos de liquidación nuevos."));
        }

        if (bruto is { } b && tara is { } t && netoNuevo is { } n && b - t != n)
        {
            return Resultado.Fallo<RectificacionRecepcion>(Error.Validacion("rectificacion.pesada", $"La nueva pesada da {b - t:0.###} kg netos y se indican {n:0.###}."));
        }

        if (netoNuevo is <= 0m || kilosLiquidacion is <= 0m || (netoNuevo is { } nn && decimal.Round(nn, 3) != nn))
        {
            return Resultado.Fallo<RectificacionRecepcion>(Error.Validacion("rectificacion.kilos", "Los kilos son positivos (hasta 3 decimales)."));
        }

        var diferencia = (netoNuevo ?? netoAnterior) - netoAnterior;
        if (diferencia == 0m && kilosLiquidacion is null)
        {
            return Resultado.Fallo<RectificacionRecepcion>(Error.Validacion("rectificacion.nada", "El neto indicado es el que ya tiene la línea."));
        }

        return Resultado.Ok(new RectificacionRecepcion(Guid.NewGuid(), empresaId)
        {
            RecepcionId = recepcionId, LineaRecepcionId = lineaId, PartidaId = partidaId, Fecha = fecha, NetoAnteriorKg = netoAnterior, DiferenciaKg = diferencia,
            KilosLiquidacion = kilosLiquidacion, BrutoKg = bruto, TaraKg = tara, Motivo = motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim(),
            UsuarioId = usuarioId, CreadaEn = ahora,
        });
    }

    /// <summary>
    /// Kilos por los que se liquida la línea tras sus rectificaciones: los de liquidación de la última que los fijó; si no
    /// hay, los fijados en la recepción; si no, el neto real rectificado.
    /// </summary>
    public static decimal KilosALiquidar(LineaRecepcion linea, IEnumerable<RectificacionRecepcion> rectificaciones)
    {
        ArgumentNullException.ThrowIfNull(linea);
        var lista = (rectificaciones ?? []).Where(r => r.LineaRecepcionId == linea.Id).OrderBy(r => r.CreadaEn).ToList();
        return lista.LastOrDefault(r => r.KilosLiquidacion is not null)?.KilosLiquidacion
               ?? linea.KilosLiquidacion
               ?? (linea.NetoKg ?? 0m) + lista.Sum(r => r.DiferenciaKg);
    }
}

/// <summary>
/// Qué producto puede salir de cuál en la confección (pimiento verde → pimiento verde confeccionado, → destrío…), con la
/// merma máxima admitida en esa transformación. Si un producto tiene reglas, solo puede transformarse en lo que dicen.
/// </summary>
public sealed class ReglaTransformacion : RaizAgregadoEmpresa<Guid>
{
    private ReglaTransformacion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ReglaTransformacion(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public Guid ProductoOrigenId { get; private set; }

    public Guid ProductoDestinoId { get; private set; }

    /// <summary>Merma máxima (% de lo consumido) en esta transformación; sin valor, la general de la configuración.</summary>
    public decimal? MermaMaximaPct { get; private set; }

    public static Resultado<ReglaTransformacion> Crear(Guid empresaId, Guid origen, Guid destino, decimal? mermaMaxima)
    {
        var r = new ReglaTransformacion(Guid.NewGuid(), empresaId) { ProductoOrigenId = origen, ProductoDestinoId = destino };
        var c = r.Cambiar(mermaMaxima);
        return c.EsFallo ? Resultado.Fallo<ReglaTransformacion>(c.Error) : Resultado.Ok(r);
    }

    public Resultado Cambiar(decimal? mermaMaxima)
    {
        if (mermaMaxima is < 0m or > 100m)
        {
            return Resultado.Fallo(Error.Validacion("regla.merma", "La merma máxima va de 0 a 100 %."));
        }

        MermaMaximaPct = mermaMaxima;
        return Resultado.Ok();
    }
}

/// <summary>
/// Repaletizado: pasar kilos (o palés enteros) de unos palés a otro, sin cambiar de producto ni de partida, en una sola
/// operación registrada con sus aristas palé de origen → palé de destino. Es de solo inserción.
/// </summary>
public sealed class Repaletizado : RaizAgregadoEmpresa<Guid>
{
    private readonly List<LineaRepaletizado> _lineas = [];

    private Repaletizado(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Repaletizado(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public DateOnly Fecha { get; private set; }

    public Guid DestinoPaleId { get; private set; }

    public string? Motivo { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<LineaRepaletizado> Lineas => _lineas;

    public decimal Kilos => _lineas.Sum(l => l.Kilos);

    public static Resultado<Repaletizado> Crear(Guid empresaId, DateOnly fecha, Guid destinoPaleId, IReadOnlyList<(Guid OrigenPaleId, Guid PartidaId, decimal Kilos, int Cajas)> lineas,
        string? motivo, Guid? usuarioId, DateTimeOffset ahora)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (lineas.Count == 0 || lineas.Any(l => l.Kilos <= 0m || l.OrigenPaleId == destinoPaleId))
        {
            return Resultado.Fallo<Repaletizado>(Error.Validacion("repaletizado.lineas", "Indica qué se pasa (kilos positivos) desde palés distintos del de destino."));
        }

        var r = new Repaletizado(Guid.NewGuid(), empresaId)
        {
            Fecha = fecha, DestinoPaleId = destinoPaleId, UsuarioId = usuarioId, CreadoEn = ahora,
            Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim(),
        };
        r._lineas.AddRange(lineas.Select(l => new LineaRepaletizado(Guid.NewGuid(), l.OrigenPaleId, l.PartidaId, l.Kilos, l.Cajas)));
        return Resultado.Ok(r);
    }
}

/// <summary>Arista del repaletizado: kilos de una partida que pasan de un palé de origen al de destino.</summary>
public sealed class LineaRepaletizado : EntidadBase<Guid>
{
    private LineaRepaletizado(Guid id)
        : base(id)
    {
    }

    internal LineaRepaletizado(Guid id, Guid origenPaleId, Guid partidaId, decimal kilos, int cajas)
        : base(id)
    {
        OrigenPaleId = origenPaleId;
        PartidaId = partidaId;
        Kilos = kilos;
        Cajas = cajas;
    }

    public Guid OrigenPaleId { get; private set; }

    public Guid PartidaId { get; private set; }

    public decimal Kilos { get; private set; }

    public int Cajas { get; private set; }
}
