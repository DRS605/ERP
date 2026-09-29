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
    /// hay, los fijados en la recepción; si no, el neto real rectificado, menos el descuento del muestreo de calidad
    /// definitivo.
    /// </summary>
    public static decimal KilosALiquidar(LineaRecepcion linea, IEnumerable<RectificacionRecepcion> rectificaciones, IEnumerable<MuestreoCalidad>? muestreos = null)
    {
        ArgumentNullException.ThrowIfNull(linea);
        var lista = (rectificaciones ?? []).Where(r => r.LineaRecepcionId == linea.Id).OrderBy(r => r.CreadaEn).ToList();
        var neto = (linea.NetoKg ?? 0m) + lista.Sum(r => r.DiferenciaKg);
        // Sin kilos de liquidación fijados, el muestreo de calidad definitivo descuenta sus defectos del neto real.
        var calidad = (muestreos ?? []).FirstOrDefault(m => m.LineaRecepcionId == linea.Id && m.Definitivo && !m.Anulado);
        return lista.LastOrDefault(r => r.KilosLiquidacion is not null)?.KilosLiquidacion
               ?? linea.KilosLiquidacion
               ?? (calidad is null ? neto : calidad.Aplicar(neto));
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
/// Registro de una corrección de expedición: su anulación (el palé vuelve) o el cambio del cliente o la referencia de un
/// palé ya expedido. Guarda el antes y el después, el motivo y quién. Es de solo inserción.
/// </summary>
public sealed class CorreccionExpedicion : RaizAgregadoEmpresa<Guid>
{
    public const string Anulacion = "Anulacion";
    public const string Datos = "Datos";

    private CorreccionExpedicion(Guid id)
        : base(id, Guid.Empty)
    {
        Tipo = null!;
    }

    private CorreccionExpedicion(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Tipo = null!;
    }

    public Guid PaleId { get; private set; }

    public string Tipo { get; private set; }

    public Guid? ClienteAnteriorId { get; private set; }

    public Guid? ClienteNuevoId { get; private set; }

    public string? ReferenciaAnterior { get; private set; }

    public string? ReferenciaNueva { get; private set; }

    public string? Motivo { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public DateTimeOffset En { get; private set; }

    public static CorreccionExpedicion Crear(Guid empresaId, Guid paleId, string tipo, Guid? clienteAnterior, Guid? clienteNuevo, string? referenciaAnterior,
        string? referenciaNueva, string? motivo, Guid? usuarioId, DateTimeOffset en) => new(Guid.NewGuid(), empresaId)
    {
        PaleId = paleId, Tipo = tipo, ClienteAnteriorId = clienteAnterior, ClienteNuevoId = clienteNuevo, ReferenciaAnterior = referenciaAnterior,
        ReferenciaNueva = referenciaNueva, Motivo = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim(),
        UsuarioId = usuarioId, En = en,
    };
}

/// <summary>
/// Merma máxima de la confección para una familia de artículos (pimiento, sandía, melón…): cada familia merma distinto. Se
/// aplica a lo consumido de sus artículos, salvo que una transformación permitida fije una más estricta; sin familia, la
/// general de la configuración.
/// </summary>
public sealed class ToleranciaMermaFamilia : RaizAgregadoEmpresa<Guid>
{
    private ToleranciaMermaFamilia(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ToleranciaMermaFamilia(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public Guid FamiliaId { get; private set; }

    public decimal MermaMaximaPct { get; private set; }

    public static Resultado<ToleranciaMermaFamilia> Crear(Guid empresaId, Guid familiaId, decimal mermaMaxima)
    {
        var t = new ToleranciaMermaFamilia(Guid.NewGuid(), empresaId) { FamiliaId = familiaId };
        var c = t.Cambiar(mermaMaxima);
        return c.EsFallo ? Resultado.Fallo<ToleranciaMermaFamilia>(c.Error) : Resultado.Ok(t);
    }

    public Resultado Cambiar(decimal mermaMaxima)
    {
        if (mermaMaxima is < 0m or > 100m || decimal.Round(mermaMaxima, 2) != mermaMaxima)
        {
            return Resultado.Fallo(Error.Validacion("tolerancia.merma", "La merma máxima va de 0 a 100 % (hasta 2 decimales)."));
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
