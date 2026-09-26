using AlxorCore.Migracion.Dominio;
using AlxorCore.Migracion.Hispatec;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Migracion.Infraestructura;

/// <summary>Contexto de la migración desde otros ERP (esquema <c>migracion</c>).</summary>
public sealed class MigracionDbContext : DbContextEmpresaBase, IUnidadDeTrabajoMigracion
{
    public const string Esquema = "migracion";

    public MigracionDbContext(DbContextOptions<MigracionDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public DbSet<Correspondencia> Correspondencias => Set<Correspondencia>();

    public DbSet<EjecucionMigracion> Ejecuciones => Set<EjecucionMigracion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MigracionDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionCorrespondencia : IEntityTypeConfiguration<Correspondencia>
{
    public void Configure(EntityTypeBuilder<Correspondencia> b)
    {
        b.ToTable("correspondencia");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Origen).HasColumnName("origen").HasMaxLength(30).IsRequired();
        b.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(30).IsRequired();
        b.Property(x => x.OrigenId).HasColumnName("origen_id").HasMaxLength(60).IsRequired();
        b.Property(x => x.DestinoId).HasColumnName("destino_id").IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Origen, x.Entidad, x.OrigenId }).IsUnique().HasDatabaseName("ux_correspondencia_origen");
        b.HasIndex(x => new { x.EmpresaId, x.DestinoId }).HasDatabaseName("ix_correspondencia_destino");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionEjecucion : IEntityTypeConfiguration<EjecucionMigracion>
{
    public void Configure(EntityTypeBuilder<EjecucionMigracion> b)
    {
        b.ToTable("ejecucion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Origen).HasColumnName("origen").HasMaxLength(30).IsRequired();
        b.Property(x => x.FechaCorte).HasColumnName("fecha_corte").IsRequired();
        b.Property(x => x.Resumen).HasColumnName("resumen").HasMaxLength(2000).IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.CreadoEn }).HasDatabaseName("ix_ejecucion_empresa");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioCorrespondencias : IRepositorioCorrespondencias
{
    private readonly MigracionDbContext _ctx;
    private readonly IReloj _reloj;

    public RepositorioCorrespondencias(MigracionDbContext ctx, IReloj reloj)
    {
        _ctx = ctx;
        _reloj = reloj;
    }

    public async Task<IReadOnlyDictionary<string, Guid>> TodasAsync(Guid empresaId, string entidad, CancellationToken ct = default) =>
        await _ctx.Correspondencias.AsNoTracking().Where(c => c.EmpresaId == empresaId && c.Origen == CargaHispatec.Origen && c.Entidad == entidad)
            .ToDictionaryAsync(c => c.OrigenId, c => c.DestinoId, StringComparer.Ordinal, ct).ConfigureAwait(false);

    public void Registrar(Guid empresaId, string entidad, string origenId, Guid destinoId) =>
        _ctx.Correspondencias.Add(new Correspondencia(empresaId, CargaHispatec.Origen, entidad, origenId, destinoId, _reloj.AhoraUtc));

    public void RegistrarEjecucion(Guid empresaId, DateOnly fechaCorte, string resumen) =>
        _ctx.Ejecuciones.Add(new EjecucionMigracion(empresaId, CargaHispatec.Origen, fechaCorte, resumen, _reloj.AhoraUtc));

    public async Task<IReadOnlyList<EjecucionMigracionDto>> EjecucionesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Ejecuciones.AsNoTracking().Where(e => e.EmpresaId == empresaId).OrderByDescending(e => e.CreadoEn)
            .Select(e => new EjecucionMigracionDto(e.Id, e.FechaCorte, e.Resumen, e.CreadoEn)).ToListAsync(ct).ConfigureAwait(false);
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class MigracionDbContextFactory : IDesignTimeDbContextFactory<MigracionDbContext>
{
    public MigracionDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        return new MigracionDbContext(new DbContextOptionsBuilder<MigracionDbContext>().UseNpgsql(conexion).Options, new PublicadorInactivo(), new ContextoVacio());
    }

    private sealed class PublicadorInactivo : IPublicadorEventos
    {
        public Task PublicarAsync(IReadOnlyCollection<IEventoDominio> eventos, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ContextoVacio : IContextoEmpresa
    {
        public Guid? EmpresaId => null;
    }
}
