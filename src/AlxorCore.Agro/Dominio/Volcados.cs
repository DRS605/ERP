using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

public enum EstadoVolcado
{
    /// <summary>Volcado en la línea, pendiente de pasar a un parte de confección.</summary>
    Registrado = 1,

    /// <summary>Sus kilos están en un parte de confección.</summary>
    EnParte = 2,

    /// <summary>Anulado: fue un error de lectura.</summary>
    Anulado = 3,
}

/// <summary>
/// Volcado de un palot (o palé de entrada) en una línea de la planta, leído en el terminal de la línea: qué palé, en qué
/// línea y orden, y cuándo se volcó. No mueve existencias: los kilos salen de la partida con el parte de confección que se
/// genera desde los volcados. El terminal puede trabajar sin conexión y mandar los volcados más tarde: cada lectura lleva
/// una clave que pone el terminal, así que reenviarla no la duplica.
/// </summary>
public sealed class VolcadoPalot : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudClave = 64;
    public const int LongitudTerminal = 120;

    /// <summary>Margen para la hora del terminal por delante del servidor (relojes desajustados).</summary>
    public static readonly TimeSpan MargenReloj = TimeSpan.FromMinutes(10);

    private VolcadoPalot(Guid id) : base(id, Guid.Empty)
    {
        Sscc = null!;
        ClaveTerminal = null!;
        Terminal = null!;
    }

    private VolcadoPalot(Guid id, Guid empresaId) : base(id, empresaId)
    {
        Sscc = null!;
        ClaveTerminal = null!;
        Terminal = null!;
    }

    public Guid LineaId { get; private set; }

    public Guid? OrdenLineaId { get; private set; }

    public Guid PaleId { get; private set; }

    public string Sscc { get; private set; }

    /// <summary>Kilos que llevaba el palé al volcarlo (informativo: el parte toma lo que lleve al generarse).</summary>
    public decimal Kilos { get; private set; }

    /// <summary>Hora del volcado en el terminal (con su desfase horario).</summary>
    public DateTimeOffset VolcadoEn { get; private set; }

    /// <summary>Día del volcado en la hora local del terminal.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Cuándo llegó al servidor (con la cola sin conexión, puede ser bastante después).</summary>
    public DateTimeOffset RegistradoEn { get; private set; }

    /// <summary>Clave única de la lectura que pone el terminal: reenviarla devuelve el mismo volcado.</summary>
    public string ClaveTerminal { get; private set; }

    /// <summary>Terminal o usuario que lo registró.</summary>
    public string Terminal { get; private set; }

    public EstadoVolcado Estado { get; private set; }

    public Guid? ParteConfeccionId { get; private set; }

    public string? MotivoAnulacion { get; private set; }

    public static Resultado<VolcadoPalot> Registrar(Guid empresaId, string? clave, LineaPlanta linea, OrdenLinea? orden, Guid paleId, string sscc, decimal kilos,
        DateTimeOffset volcadoEn, string terminal, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(linea);
        ArgumentNullException.ThrowIfNull(reloj);
        if (string.IsNullOrWhiteSpace(clave) || clave.Trim().Length > LongitudClave)
        {
            return Resultado.Fallo<VolcadoPalot>(Error.Validacion("volcado.clave", $"Cada volcado necesita su clave de lectura (hasta {LongitudClave} caracteres)."));
        }

        if (!linea.Activa)
        {
            return Resultado.Fallo<VolcadoPalot>(Error.Conflicto("volcado.linea_inactiva", $"La línea {linea.Codigo} está dada de baja."));
        }

        if (orden is not null && (orden.LineaId != linea.Id || orden.Estado is EstadoOrdenLinea.Terminada or EstadoOrdenLinea.Cancelada))
        {
            return Resultado.Fallo<VolcadoPalot>(Error.Conflicto("volcado.orden",
                orden.LineaId != linea.Id ? "La orden es de otra línea." : "La orden ya está terminada o cancelada."));
        }

        if (volcadoEn > reloj.AhoraUtc + MargenReloj)
        {
            return Resultado.Fallo<VolcadoPalot>(Error.Validacion("volcado.futuro", "La hora del volcado es posterior a la actual: revisa el reloj del terminal."));
        }

        if (kilos <= 0m)
        {
            return Resultado.Fallo<VolcadoPalot>(Error.Conflicto("volcado.pale_vacio", $"El palé {sscc} no lleva kilos en ninguna partida."));
        }

        return Resultado.Ok(new VolcadoPalot(Guid.NewGuid(), empresaId)
        {
            ClaveTerminal = clave.Trim(), LineaId = linea.Id, OrdenLineaId = orden?.Id, PaleId = paleId, Sscc = sscc, Kilos = kilos, VolcadoEn = volcadoEn,
            Fecha = DateOnly.FromDateTime(volcadoEn.DateTime), RegistradoEn = reloj.AhoraUtc, Terminal = LineaPlanta.Texto(terminal, LongitudTerminal) ?? "?",
            Estado = EstadoVolcado.Registrado,
        });
    }

    public Resultado Anular(string? motivo)
    {
        if (Estado != EstadoVolcado.Registrado)
        {
            return Resultado.Fallo(Error.Conflicto("volcado.no_registrado",
                Estado == EstadoVolcado.Anulado ? "El volcado ya está anulado." : "El volcado está en un parte de confección: quita antes el parte."));
        }

        Estado = EstadoVolcado.Anulado;
        MotivoAnulacion = LineaPlanta.Texto(motivo, 200) ?? "Lectura errónea";
        return Resultado.Ok();
    }

    public Resultado PasarAParte(Guid parteId)
    {
        if (Estado != EstadoVolcado.Registrado)
        {
            return Resultado.Fallo(Error.Conflicto("volcado.no_registrado", "El volcado ya no está pendiente."));
        }

        Estado = EstadoVolcado.EnParte;
        ParteConfeccionId = parteId;
        return Resultado.Ok();
    }

    /// <summary>El parte se borró o se anuló: el volcado vuelve a quedar pendiente.</summary>
    public void Liberar()
    {
        if (Estado == EstadoVolcado.EnParte)
        {
            Estado = EstadoVolcado.Registrado;
            ParteConfeccionId = null;
        }
    }
}
