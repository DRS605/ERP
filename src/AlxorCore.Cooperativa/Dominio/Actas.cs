using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Cooperativa.Dominio;

/// <summary>Órgano que se reúne: cada uno lleva su libro de actas, numerado aparte.</summary>
public enum OrganoSocial
{
    AsambleaGeneral = 1,

    /// <summary>Consejo rector (cooperativa).</summary>
    ConsejoRector = 2,

    /// <summary>Junta rectora (SAT).</summary>
    JuntaRectora = 3,
}

public enum CaracterSesion
{
    Ordinaria = 1,
    Extraordinaria = 2,

    /// <summary>Universal: sin convocatoria, con todos los socios presentes o representados.</summary>
    Universal = 3,
}

public enum EstadoActa
{
    /// <summary>Redactándose: se puede cambiar o eliminar.</summary>
    Borrador = 1,

    /// <summary>Aprobada y firmada: forma parte del libro y ya no cambia.</summary>
    Aprobada = 2,
}

/// <summary>Acta de una reunión de la asamblea general o del órgano de gobierno, con su número correlativo en el libro de ese órgano.</summary>
public sealed class Acta : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudTexto = 20000;

    private Acta(Guid id)
        : base(id, Guid.Empty)
    {
        OrdenDelDia = null!;
        Acuerdos = null!;
    }

    private Acta(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        OrdenDelDia = null!;
        Acuerdos = null!;
    }

    public OrganoSocial Organo { get; private set; }

    /// <summary>Número en el libro de su órgano: se da al aprobarla, así el libro no tiene huecos.</summary>
    public int? Numero { get; private set; }

    public DateOnly Fecha { get; private set; }

    public CaracterSesion Caracter { get; private set; }

    public string? Lugar { get; private set; }

    public int Presentes { get; private set; }

    public int Representados { get; private set; }

    public string? Presidente { get; private set; }

    public string? Secretario { get; private set; }

    public string OrdenDelDia { get; private set; }

    public string Acuerdos { get; private set; }

    public EstadoActa Estado { get; private set; }

    public DateTimeOffset? AprobadaEn { get; private set; }

    public static Resultado<Acta> Crear(Guid empresaId, OrganoSocial organo, DatosActa d)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (!Enum.IsDefined(organo))
        {
            return Resultado.Fallo<Acta>(Error.Validacion("acta.organo", "El acta es de la asamblea general, del consejo rector o de la junta rectora."));
        }

        var a = new Acta(Guid.NewGuid(), empresaId) { Organo = organo, Estado = EstadoActa.Borrador };
        var r = a.Cambiar(d);
        return r.EsFallo ? Resultado.Fallo<Acta>(r.Error) : Resultado.Ok(a);
    }

    public Resultado Cambiar(DatosActa d)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (Estado != EstadoActa.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("acta.aprobada", "El acta ya está aprobada: forma parte del libro y no se cambia."));
        }

        if (!Enum.IsDefined(d.Caracter))
        {
            return Resultado.Fallo(Error.Validacion("acta.caracter", "La sesión es ordinaria, extraordinaria o universal."));
        }

        if (d.Presentes < 0 || d.Representados < 0)
        {
            return Resultado.Fallo(Error.Validacion("acta.asistentes", "Presentes y representados no pueden ser negativos."));
        }

        if (string.IsNullOrWhiteSpace(d.OrdenDelDia) || string.IsNullOrWhiteSpace(d.Acuerdos))
        {
            return Resultado.Fallo(Error.Validacion("acta.texto", "El acta recoge el orden del día y los acuerdos."));
        }

        Fecha = d.Fecha;
        Caracter = d.Caracter;
        Lugar = Socio.Recortar(d.Lugar, 200);
        Presentes = d.Presentes;
        Representados = d.Representados;
        Presidente = Socio.Recortar(d.Presidente, 200);
        Secretario = Socio.Recortar(d.Secretario, 200);
        OrdenDelDia = Socio.Recortar(d.OrdenDelDia, LongitudTexto)!;
        Acuerdos = Socio.Recortar(d.Acuerdos, LongitudTexto)!;
        return Resultado.Ok();
    }

    public Resultado Aprobar(int numero, DateTimeOffset ahora)
    {
        if (Estado != EstadoActa.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("acta.aprobada", "El acta ya está aprobada."));
        }

        Estado = EstadoActa.Aprobada;
        Numero = numero;
        AprobadaEn = ahora;
        return Resultado.Ok();
    }
}

public sealed record DatosActa(DateOnly Fecha, CaracterSesion Caracter, string? OrdenDelDia, string? Acuerdos, string? Lugar = null, int Presentes = 0,
    int Representados = 0, string? Presidente = null, string? Secretario = null);
