using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Migracion.Dominio;

/// <summary>Un registro del sistema de origen y el que le corresponde en ALXOR (creado o reutilizado).</summary>
public sealed class Correspondencia : RaizAgregadoEmpresa<Guid>
{
    private Correspondencia(Guid id)
        : base(id, Guid.Empty)
    {
        Origen = null!;
        Entidad = null!;
        OrigenId = null!;
    }

    public Correspondencia(Guid empresaId, string origen, string entidad, string origenId, Guid destinoId, DateTimeOffset ahora)
        : base(Guid.NewGuid(), empresaId)
    {
        Origen = origen;
        Entidad = entidad;
        OrigenId = origenId;
        DestinoId = destinoId;
        CreadoEn = ahora;
    }

    public string Origen { get; private set; }

    public string Entidad { get; private set; }

    public string OrigenId { get; private set; }

    public Guid DestinoId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }
}

/// <summary>Registro de una carga (qué se hizo, a qué fecha de corte).</summary>
public sealed class EjecucionMigracion : RaizAgregadoEmpresa<Guid>
{
    private EjecucionMigracion(Guid id)
        : base(id, Guid.Empty)
    {
        Origen = null!;
        Resumen = null!;
    }

    public EjecucionMigracion(Guid empresaId, string origen, DateOnly fechaCorte, string resumen, DateTimeOffset ahora)
        : base(Guid.NewGuid(), empresaId)
    {
        Origen = origen;
        FechaCorte = fechaCorte;
        Resumen = resumen;
        CreadoEn = ahora;
    }

    public string Origen { get; private set; }

    public DateOnly FechaCorte { get; private set; }

    public string Resumen { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }
}
