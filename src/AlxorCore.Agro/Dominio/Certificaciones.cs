using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>Certificaciones de la fruta: van con la partida desde la finca hasta el cliente.</summary>
[Flags]
public enum Certificaciones
{
    Ninguna = 0,

    /// <summary>Producción ecológica (ES-ECO): solo se vende como ecológica si toda la cadena lo es.</summary>
    Ecologico = 1,

    /// <summary>GlobalG.A.P. (GGN del productor).</summary>
    GlobalGap = 2,

    /// <summary>GRASP (evaluación social de GlobalG.A.P.).</summary>
    Grasp = 4,
}

/// <summary>
/// Certificado de un agricultor (todas sus parcelas) o de una parcela concreta, con su vigencia: el número de operador
/// ecológico o el GGN, y el organismo que lo emite. La fruta recibida tiene las certificaciones vigentes el día de la
/// recepción.
/// </summary>
public sealed class CertificadoAgro : RaizAgregadoEmpresa<Guid>
{
    private CertificadoAgro(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
    }

    private CertificadoAgro(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Codigo = null!;
    }

    public Guid AgricultorId { get; private set; }

    /// <summary>Solo esa parcela; sin parcela, todas las del agricultor.</summary>
    public Guid? ParcelaId { get; private set; }

    public Certificaciones Tipo { get; private set; }

    /// <summary>Número de operador ecológico, GGN…</summary>
    public string Codigo { get; private set; }

    public string? Organismo { get; private set; }

    public DateOnly Desde { get; private set; }

    public DateOnly? Hasta { get; private set; }

    /// <summary>Dado de baja (retirado o suspendido): ya no certifica, aunque su vigencia no haya acabado.</summary>
    public bool Baja { get; private set; }

    public bool VigenteEl(DateOnly fecha) => !Baja && Desde <= fecha && (Hasta is null || fecha <= Hasta);

    public static Resultado<CertificadoAgro> Crear(Guid empresaId, Guid agricultorId, Guid? parcelaId, Certificaciones tipo, string? codigo, string? organismo, DateOnly desde,
        DateOnly? hasta)
    {
        var c = new CertificadoAgro(Guid.NewGuid(), empresaId) { AgricultorId = agricultorId, ParcelaId = parcelaId };
        var r = c.Actualizar(tipo, codigo, organismo, desde, hasta);
        return r.EsFallo ? Resultado.Fallo<CertificadoAgro>(r.Error) : Resultado.Ok(c);
    }

    public Resultado Actualizar(Certificaciones tipo, string? codigo, string? organismo, DateOnly desde, DateOnly? hasta)
    {
        if (tipo is not (Certificaciones.Ecologico or Certificaciones.GlobalGap or Certificaciones.Grasp))
        {
            return Resultado.Fallo(Error.Validacion("certificado.tipo", "El certificado es de un solo tipo: Ecologico, GlobalGap o Grasp."));
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Resultado.Fallo(Error.Validacion("certificado.codigo", "Indica el número del certificado (operador ecológico, GGN…)."));
        }

        if (hasta is { } h && h < desde)
        {
            return Resultado.Fallo(Error.Validacion("certificado.vigencia", "La vigencia acaba antes de empezar."));
        }

        Tipo = tipo;
        Codigo = codigo.Trim().Length > 40 ? codigo.Trim()[..40] : codigo.Trim();
        Organismo = string.IsNullOrWhiteSpace(organismo) ? null : organismo.Trim().Length > 100 ? organismo.Trim()[..100] : organismo.Trim();
        Desde = desde;
        Hasta = hasta;
        return Resultado.Ok();
    }

    public void DarDeBaja(bool baja) => Baja = baja;
}

/// <summary>
/// Cómo se vende un artículo: las certificaciones que exige a su fruta (un «pimiento verde ecológico» exige
/// <see cref="Certificaciones.Ecologico"/>). Un artículo declarado sin ecológico es convencional: meter en él fruta
/// ecológica es una descalificación, que se hace de forma explícita y con motivo. Sin declaración no se comprueba nada.
/// </summary>
public sealed class DeclaracionArticulo : RaizAgregadoEmpresa<Guid>
{
    private DeclaracionArticulo(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private DeclaracionArticulo(Guid id, Guid empresaId, Guid productoId, Certificaciones exige)
        : base(id, empresaId)
    {
        ProductoId = productoId;
        Exige = exige;
    }

    public Guid ProductoId { get; private set; }

    public Certificaciones Exige { get; private set; }

    public static DeclaracionArticulo Crear(Guid empresaId, Guid productoId, Certificaciones exige) => new(Guid.NewGuid(), empresaId, productoId, exige);

    public void Cambiar(Certificaciones exige) => Exige = exige;
}

/// <summary>
/// Registro de una descalificación: qué certificaciones se le quitaron a una partida, por qué, quién y en qué
/// documento. Es de solo inserción, y la partida no pierde una certificación sin su registro.
/// </summary>
public sealed class DescalificacionPartida : RaizAgregadoEmpresa<Guid>
{
    private DescalificacionPartida(Guid id)
        : base(id, Guid.Empty)
    {
        Motivo = null!;
    }

    private DescalificacionPartida(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Motivo = null!;
    }

    public Guid PartidaId { get; private set; }

    public Certificaciones Quitadas { get; private set; }

    public string Motivo { get; private set; }

    public string? DocumentoTipo { get; private set; }

    public Guid? DocumentoId { get; private set; }

    public Guid? UsuarioId { get; private set; }

    public DateTimeOffset En { get; private set; }

    public static DescalificacionPartida Crear(Guid empresaId, Guid partidaId, Certificaciones quitadas, string motivo, string? documentoTipo, Guid? documentoId, Guid? usuarioId,
        DateTimeOffset en) => new(Guid.NewGuid(), empresaId)
    {
        PartidaId = partidaId, Quitadas = quitadas, Motivo = motivo.Trim().Length > 300 ? motivo.Trim()[..300] : motivo.Trim(), DocumentoTipo = documentoTipo,
        DocumentoId = documentoId, UsuarioId = usuarioId, En = en,
    };
}

/// <summary>Reglas de la certificación de una partida nueva (en la recepción o en la confección).</summary>
public static class ReglasCertificacion
{
    public const Certificaciones Todas = Certificaciones.Ecologico | Certificaciones.GlobalGap | Certificaciones.Grasp;

    /// <summary>Lo que tienen en común varias partidas: al mezclar ecológico con convencional, el resultado es convencional.</summary>
    public static Certificaciones Comunes(IEnumerable<Certificaciones> origenes)
    {
        ArgumentNullException.ThrowIfNull(origenes);
        var comunes = Todas;
        var alguna = false;
        foreach (var c in origenes)
        {
            comunes &= c;
            alguna = true;
        }

        return alguna ? comunes : Certificaciones.Ninguna;
    }

    /// <summary>
    /// Certificaciones de la partida que se crea con el artículo <paramref name="articulo"/> a partir de fruta con
    /// <paramref name="origen"/>:
    /// <list type="bullet">
    /// <item>lo que el artículo exige tiene que estar en el origen (no se vende como ecológico lo que no lo es);</item>
    /// <item>fruta ecológica en un artículo que no es ecológico se descalifica solo con motivo;</item>
    /// <item>sin declaración del artículo, la partida conserva las del origen.</item>
    /// </list>
    /// Devuelve las certificaciones de la partida y las que se le quitan (para registrar la descalificación).
    /// </summary>
    public static Resultado<(Certificaciones Resultado, Certificaciones Quitadas)> Aplicar(Certificaciones origen, Certificaciones? exige, string? motivoDescalificacion,
        string articulo)
    {
        if (exige is not { } declarada)
        {
            return Resultado.Ok((origen, Certificaciones.Ninguna));
        }

        var faltan = declarada & ~origen;
        if (faltan != Certificaciones.Ninguna)
        {
            return Resultado.Fallo<(Certificaciones, Certificaciones)>(Error.Conflicto("certificacion.falta",
                $"«{articulo}» se vende como {Texto(faltan)} y la fruta no lo es (o su certificado no está vigente)."));
        }

        var quitar = origen & Certificaciones.Ecologico & ~declarada;
        if (quitar != Certificaciones.Ninguna && string.IsNullOrWhiteSpace(motivoDescalificacion))
        {
            return Resultado.Fallo<(Certificaciones, Certificaciones)>(Error.Conflicto("certificacion.descalificar",
                $"La fruta es ecológica y «{articulo}» es convencional: elige el artículo ecológico o indica el motivo para venderla como convencional."));
        }

        return Resultado.Ok((origen & ~quitar, quitar));
    }

    public static string Texto(Certificaciones c)
    {
        var partes = new List<string>();
        if (c.HasFlag(Certificaciones.Ecologico))
        {
            partes.Add("ecológico");
        }

        if (c.HasFlag(Certificaciones.GlobalGap))
        {
            partes.Add("GlobalG.A.P.");
        }

        if (c.HasFlag(Certificaciones.Grasp))
        {
            partes.Add("GRASP");
        }

        return partes.Count == 0 ? "convencional" : string.Join(" + ", partes);
    }
}
