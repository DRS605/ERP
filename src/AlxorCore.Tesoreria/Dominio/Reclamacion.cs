using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Nivel de reclamación: cuántos días después del vencimiento toca y qué se dice.</summary>
public sealed record NivelReclamacion(int Nivel, int DiasTrasVencimiento, string Asunto, string Texto)
{
    /// <summary>Niveles por defecto: recordatorio amable, segundo aviso y aviso final.</summary>
    public static IReadOnlyList<NivelReclamacion> PorDefecto { get; } =
    [
        new(1, 7, "Recordatorio: factura {factura} vencida",
            "Hola, {cliente}: le recordamos que la factura {factura} venció el {vencimiento} y tiene {pendiente} € pendientes. Si ya la ha pagado, ignore este mensaje."),
        new(2, 30, "Segundo aviso: factura {factura} pendiente",
            "Hola, {cliente}: la factura {factura} lleva {dias} días vencida ({pendiente} € pendientes). Le rogamos que regularice el pago."),
        new(3, 60, "Aviso final: factura {factura} impagada",
            "{cliente}: la factura {factura} sigue impagada tras {dias} días ({pendiente} €). Si no recibimos el pago, iniciaremos las acciones de cobro oportunas."),
    ];

    /// <summary>Valida una lista de niveles: numerados 1..n, con días crecientes y textos no vacíos.</summary>
    public static Error? Validar(IReadOnlyList<NivelReclamacion> niveles)
    {
        ArgumentNullException.ThrowIfNull(niveles);
        if (niveles.Count is 0 or > 10)
        {
            return Error.Validacion("reclamacion.niveles", "Define entre 1 y 10 niveles de reclamación.");
        }

        for (var i = 0; i < niveles.Count; i++)
        {
            var n = niveles[i];
            if (n.Nivel != i + 1)
            {
                return Error.Validacion("reclamacion.niveles", "Los niveles se numeran seguidos desde 1.");
            }

            if (n.DiasTrasVencimiento < 0 || (i > 0 && n.DiasTrasVencimiento <= niveles[i - 1].DiasTrasVencimiento))
            {
                return Error.Validacion("reclamacion.niveles", $"El nivel {n.Nivel} debe llegar más días después del vencimiento que el anterior.");
            }

            if (string.IsNullOrWhiteSpace(n.Asunto) || string.IsNullOrWhiteSpace(n.Texto))
            {
                return Error.Validacion("reclamacion.niveles", $"El nivel {n.Nivel} necesita asunto y texto.");
            }
        }

        return null;
    }

    /// <summary>Nivel que toca con <paramref name="diasRetraso"/> días de retraso (el más alto alcanzado), o null si aún no toca.</summary>
    public static NivelReclamacion? QueToca(IReadOnlyList<NivelReclamacion> niveles, int diasRetraso) =>
        niveles.Where(n => n.DiasTrasVencimiento <= diasRetraso).MaxBy(n => n.Nivel);

    // La aplicación corre en modo de globalización invariante (sin datos de culturas): el formato
    // español se fija a mano en lugar de pedir la cultura es-ES, que lanzaría una excepción.
    private static readonly System.Globalization.NumberFormatInfo ComaDecimal = new() { NumberDecimalSeparator = "," };

    /// <summary>Sustituye las variables {cliente}, {factura}, {vencimiento}, {pendiente} y {dias}.</summary>
    public static string Componer(string plantilla, string cliente, string factura, DateOnly vencimiento, decimal pendiente, int dias) =>
        plantilla.Replace("{cliente}", cliente, StringComparison.Ordinal)
            .Replace("{factura}", factura, StringComparison.Ordinal)
            .Replace("{vencimiento}", vencimiento.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal)
            .Replace("{pendiente}", pendiente.ToString("0.00", ComaDecimal), StringComparison.Ordinal)
            .Replace("{dias}", dias.ToString(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
}

/// <summary>Cómo se hizo la reclamación.</summary>
public enum CanalReclamacion
{
    Email = 1,
    Telefono = 2,
    Carta = 3,
    Otro = 4,
}

/// <summary>
/// Reclamación hecha a un cliente por una factura vencida. Es un registro histórico (no se edita):
/// deja constancia de qué nivel se envió, cuándo, por qué canal y con qué pendiente.
/// </summary>
public sealed class Reclamacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNota = 500;

    private Reclamacion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private Reclamacion(Guid id, Guid empresaId, Guid facturaId, int nivel, CanalReclamacion canal, decimal pendiente, int diasRetraso, string? nota, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        FacturaId = facturaId;
        Nivel = nivel;
        Canal = canal;
        Pendiente = pendiente;
        DiasRetraso = diasRetraso;
        Nota = nota;
        RealizadaEn = ahora;
    }

    public Guid FacturaId { get; private set; }

    public int Nivel { get; private set; }

    public CanalReclamacion Canal { get; private set; }

    /// <summary>Importe pendiente en el momento de reclamar.</summary>
    public decimal Pendiente { get; private set; }

    public int DiasRetraso { get; private set; }

    public string? Nota { get; private set; }

    public DateTimeOffset RealizadaEn { get; private set; }

    public static Resultado<Reclamacion> Registrar(
        Guid empresaId, Guid facturaId, int nivel, CanalReclamacion canal, decimal pendiente, int diasRetraso, string? nota, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (nivel < 1)
        {
            return Resultado.Fallo<Reclamacion>(Error.Validacion("reclamacion.nivel", "El nivel de reclamación empieza en 1."));
        }

        var n = string.IsNullOrWhiteSpace(nota) ? null : nota.Trim();
        if (n is { Length: > LongitudMaximaNota })
        {
            return Resultado.Fallo<Reclamacion>(Error.Validacion("reclamacion.nota_larga", "La nota es demasiado larga."));
        }

        return Resultado.Ok(new Reclamacion(Guid.NewGuid(), empresaId, facturaId, nivel, canal, pendiente, diasRetraso, n, reloj.AhoraUtc));
    }
}

/// <summary>Configuración de reclamaciones de la empresa (sus niveles). Si no existe, se usan los niveles por defecto.</summary>
public sealed class ConfiguracionReclamaciones : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionReclamaciones(Guid id)
        : base(id, Guid.Empty)
    {
        Niveles = [];
    }

    private ConfiguracionReclamaciones(Guid id, Guid empresaId, List<NivelReclamacion> niveles)
        : base(id, empresaId)
    {
        Niveles = niveles;
    }

    public List<NivelReclamacion> Niveles { get; private set; }

    public static Resultado<ConfiguracionReclamaciones> Crear(Guid empresaId, IReadOnlyList<NivelReclamacion> niveles)
    {
        var error = NivelReclamacion.Validar(niveles);
        return error is not null
            ? Resultado.Fallo<ConfiguracionReclamaciones>(error)
            : Resultado.Ok(new ConfiguracionReclamaciones(Guid.NewGuid(), empresaId, [.. niveles]));
    }

    public Resultado Cambiar(IReadOnlyList<NivelReclamacion> niveles)
    {
        var error = NivelReclamacion.Validar(niveles);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Niveles = [.. niveles];
        return Resultado.Ok();
    }
}
