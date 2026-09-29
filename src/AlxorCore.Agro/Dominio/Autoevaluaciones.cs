using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Dominio;

/// <summary>Nivel de un punto de control de GlobalG.A.P.: la certificación pide cumplir todas las obligaciones mayores y el 95 % de las menores.</summary>
public enum NivelPuntoControl
{
    Mayor,
    Menor,
    Recomendacion,
}

/// <summary>Tipo de revisión: la autoevaluación del productor o la auditoría interna (del grupo de productores o de la empresa).</summary>
public enum TipoAutoevaluacion
{
    Autoevaluacion,
    AuditoriaInterna,
}

public enum ResultadoPunto
{
    Cumple,
    NoCumple,
    NoAplica,
}

/// <summary>Punto de la lista de control: su código en la norma (p. ej. «CB 7.6.1»), el texto y el nivel.</summary>
public sealed class PuntoControl
{
    private PuntoControl()
    {
        Codigo = null!;
        Texto = null!;
    }

    internal PuntoControl(int orden, string codigo, string texto, NivelPuntoControl nivel)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Codigo = codigo;
        Texto = texto;
        Nivel = nivel;
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public string Codigo { get; private set; }

    public string Texto { get; private set; }

    public NivelPuntoControl Nivel { get; private set; }
}

/// <summary>
/// Lista de puntos de control y criterios de cumplimiento (la de GlobalG.A.P. en su versión, o una propia). Las
/// autoevaluaciones copian sus puntos al abrirse, así que cambiar la lista no altera las ya hechas.
/// </summary>
public sealed class ListaControl : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 30;
    public const int LongitudNombre = 150;
    public const int LongitudTexto = 1000;

    private readonly List<PuntoControl> _puntos = [];

    private ListaControl(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private ListaControl(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Codigo = null!;
        Nombre = null!;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Versión de la norma (p. ej. «IFA v6 Smart»).</summary>
    public string? Version { get; private set; }

    public bool Activa { get; private set; }

    public IReadOnlyList<PuntoControl> Puntos => _puntos;

    public static Resultado<ListaControl> Crear(Guid empresaId, string? codigo, string? nombre, string? version,
        IReadOnlyList<(string? Codigo, string? Texto, NivelPuntoControl Nivel)> puntos)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > LongitudCodigo)
        {
            return Resultado.Fallo<ListaControl>(Error.Validacion("lista_control.codigo", $"Indica un código de hasta {LongitudCodigo} caracteres."));
        }

        var lista = new ListaControl(Guid.NewGuid(), empresaId) { Codigo = codigo.Trim().ToUpperInvariant(), Activa = true };
        var r = lista.Actualizar(nombre, version, true, puntos);
        return r.EsFallo ? Resultado.Fallo<ListaControl>(r.Error) : Resultado.Ok(lista);
    }

    public Resultado Actualizar(string? nombre, string? version, bool activa, IReadOnlyList<(string? Codigo, string? Texto, NivelPuntoControl Nivel)> puntos)
    {
        ArgumentNullException.ThrowIfNull(puntos);
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("lista_control.nombre", $"Indica un nombre de hasta {LongitudNombre} caracteres."));
        }

        if (puntos.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("lista_control.sin_puntos", "La lista necesita al menos un punto de control."));
        }

        var nuevos = new List<PuntoControl>();
        var codigos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (c, t, n) in puntos)
        {
            if (string.IsNullOrWhiteSpace(c) || c.Trim().Length > LongitudCodigo || string.IsNullOrWhiteSpace(t) || !Enum.IsDefined(n))
            {
                return Resultado.Fallo(Error.Validacion("lista_control.punto",
                    $"Cada punto necesita código (hasta {LongitudCodigo} caracteres), texto y nivel (Mayor, Menor o Recomendacion)."));
            }

            if (!codigos.Add(c.Trim()))
            {
                return Resultado.Fallo(Error.Validacion("lista_control.punto_repetido", $"El punto «{c.Trim()}» está repetido."));
            }

            var texto = t.Trim();
            nuevos.Add(new PuntoControl(nuevos.Count + 1, c.Trim(), texto[..Math.Min(texto.Length, LongitudTexto)], n));
        }

        Nombre = nombre.Trim();
        Version = string.IsNullOrWhiteSpace(version) ? null : version.Trim()[..Math.Min(version.Trim().Length, 50)];
        Activa = activa;
        _puntos.Clear();
        _puntos.AddRange(nuevos);
        return Resultado.Ok();
    }

    public void DarDeBaja() => Activa = false;
}

/// <summary>Respuesta a un punto en una autoevaluación, con la copia del punto tal como estaba en la lista.</summary>
public sealed class RespuestaPunto
{
    private RespuestaPunto()
    {
        Codigo = null!;
        Texto = null!;
    }

    internal RespuestaPunto(int orden, string codigo, string texto, NivelPuntoControl nivel)
    {
        Id = Guid.NewGuid();
        Orden = orden;
        Codigo = codigo;
        Texto = texto;
        Nivel = nivel;
    }

    public Guid Id { get; private set; }

    public int Orden { get; private set; }

    public string Codigo { get; private set; }

    public string Texto { get; private set; }

    public NivelPuntoControl Nivel { get; private set; }

    public ResultadoPunto? Resultado { get; private set; }

    /// <summary>Evidencia o justificación (obligatoria si no aplica).</summary>
    public string? Comentario { get; private set; }

    /// <summary>Acción correctiva (obligatoria si no cumple).</summary>
    public string? AccionCorrectiva { get; private set; }

    public DateOnly? FechaLimite { get; private set; }

    internal void Responder(ResultadoPunto? resultado, string? comentario, string? accion, DateOnly? fechaLimite)
    {
        static string? T(string? t) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, 500)];
        Resultado = resultado;
        Comentario = T(comentario);
        AccionCorrectiva = resultado == ResultadoPunto.NoCumple ? T(accion) : null;
        FechaLimite = resultado == ResultadoPunto.NoCumple ? fechaLimite : null;
    }
}

/// <summary>
/// Autoevaluación o auditoría interna de GlobalG.A.P. sobre una lista de control, del productor (agricultor) o de la
/// empresa. Se responde cada punto (cumple, no cumple con su acción correctiva, o no aplica con su justificación) y se
/// cierra: cerrada, no cambia. Supera la evaluación si cumple todas las obligaciones mayores y al menos el 95 % de las
/// menores que aplican.
/// </summary>
public sealed class Autoevaluacion : RaizAgregadoEmpresa<Guid>
{
    public const decimal MinimoMenores = 95m;

    private readonly List<RespuestaPunto> _respuestas = [];

    private Autoevaluacion(Guid id)
        : base(id, Guid.Empty)
    {
        ListaCodigo = null!;
        Auditor = null!;
    }

    private Autoevaluacion(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        ListaCodigo = null!;
        Auditor = null!;
    }

    public Guid ListaControlId { get; private set; }

    public string ListaCodigo { get; private set; }

    /// <summary>Productor evaluado; sin él, la evaluación es de la empresa (central, almacén).</summary>
    public Guid? AgricultorId { get; private set; }

    public TipoAutoevaluacion Tipo { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Auditor { get; private set; }

    public string? Observaciones { get; private set; }

    public bool Cerrada { get; private set; }

    public DateTimeOffset? CerradaEn { get; private set; }

    public DateTimeOffset CreadaEn { get; private set; }

    public IReadOnlyList<RespuestaPunto> Respuestas => _respuestas;

    public static Resultado<Autoevaluacion> Abrir(Guid empresaId, ListaControl lista, Guid? agricultorId, TipoAutoevaluacion tipo, DateOnly fecha, string? auditor,
        IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(lista);
        ArgumentNullException.ThrowIfNull(reloj);
        if (!lista.Activa)
        {
            return Resultado.Fallo<Autoevaluacion>(Error.Conflicto("autoevaluacion.lista_de_baja", "La lista de control está de baja."));
        }

        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<Autoevaluacion>(Error.Validacion("autoevaluacion.tipo", "Tipo no válido: Autoevaluacion o AuditoriaInterna."));
        }

        if (string.IsNullOrWhiteSpace(auditor))
        {
            return Resultado.Fallo<Autoevaluacion>(Error.Validacion("autoevaluacion.auditor", "Indica quién hace la evaluación."));
        }

        var a = new Autoevaluacion(Guid.NewGuid(), empresaId)
        {
            ListaControlId = lista.Id, ListaCodigo = lista.Codigo, AgricultorId = agricultorId, Tipo = tipo, Fecha = fecha,
            Auditor = auditor.Trim()[..Math.Min(auditor.Trim().Length, 150)], CreadaEn = reloj.AhoraUtc,
        };
        a._respuestas.AddRange(lista.Puntos.OrderBy(p => p.Orden).Select(p => new RespuestaPunto(p.Orden, p.Codigo, p.Texto, p.Nivel)));
        return Resultado.Ok(a);
    }

    public Resultado Responder(IReadOnlyList<(string Codigo, ResultadoPunto? Resultado, string? Comentario, string? AccionCorrectiva, DateOnly? FechaLimite)> respuestas,
        string? auditor, string? observaciones)
    {
        ArgumentNullException.ThrowIfNull(respuestas);
        if (Cerrada)
        {
            return Resultado.Fallo(Error.Conflicto("autoevaluacion.cerrada", "La evaluación está cerrada: no se modifica."));
        }

        foreach (var r in respuestas)
        {
            if (r.Resultado is { } v && !Enum.IsDefined(v))
            {
                return Resultado.Fallo(Error.Validacion("autoevaluacion.resultado", "Resultado no válido: Cumple, NoCumple o NoAplica."));
            }

            var punto = _respuestas.FirstOrDefault(p => string.Equals(p.Codigo, r.Codigo, StringComparison.OrdinalIgnoreCase));
            if (punto is null)
            {
                return Resultado.Fallo(Error.Validacion("autoevaluacion.punto", $"El punto «{r.Codigo}» no está en la lista."));
            }

            punto.Responder(r.Resultado, r.Comentario, r.AccionCorrectiva, r.FechaLimite);
        }

        if (!string.IsNullOrWhiteSpace(auditor))
        {
            Auditor = auditor.Trim()[..Math.Min(auditor.Trim().Length, 150)];
        }

        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? Observaciones : observaciones.Trim()[..Math.Min(observaciones.Trim().Length, 1000)];
        return Resultado.Ok();
    }

    public Resultado Cerrar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Cerrada)
        {
            return Resultado.Fallo(Error.Conflicto("autoevaluacion.cerrada", "La evaluación ya está cerrada."));
        }

        if (_respuestas.FirstOrDefault(r => r.Resultado is null) is { } sin)
        {
            return Resultado.Fallo(Error.Validacion("autoevaluacion.sin_responder", $"Falta responder el punto «{sin.Codigo}»."));
        }

        if (_respuestas.FirstOrDefault(r => r.Resultado == ResultadoPunto.NoAplica && r.Comentario is null) is { } na)
        {
            return Resultado.Fallo(Error.Validacion("autoevaluacion.justificacion", $"El punto «{na.Codigo}» no aplica: justifica por qué."));
        }

        if (_respuestas.FirstOrDefault(r => r.Resultado == ResultadoPunto.NoCumple && r.Nivel != NivelPuntoControl.Recomendacion && r.AccionCorrectiva is null) is { } nc)
        {
            return Resultado.Fallo(Error.Validacion("autoevaluacion.accion_correctiva", $"El punto «{nc.Codigo}» no se cumple: indica la acción correctiva."));
        }

        Cerrada = true;
        CerradaEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Porcentaje de cumplimiento de un nivel sobre los puntos que aplican (100 si no aplica ninguno).</summary>
    public decimal Cumplimiento(NivelPuntoControl nivel)
    {
        var aplican = _respuestas.Where(r => r.Nivel == nivel && r.Resultado is ResultadoPunto.Cumple or ResultadoPunto.NoCumple).ToList();
        return aplican.Count == 0 ? 100m : Math.Round(100m * aplican.Count(r => r.Resultado == ResultadoPunto.Cumple) / aplican.Count, 2);
    }

    /// <summary>Supera la evaluación: todas las mayores y al menos el 95 % de las menores.</summary>
    public bool Supera => Cumplimiento(NivelPuntoControl.Mayor) == 100m && Cumplimiento(NivelPuntoControl.Menor) >= MinimoMenores;
}
