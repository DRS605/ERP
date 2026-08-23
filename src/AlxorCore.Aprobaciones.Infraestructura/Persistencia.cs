using AlxorCore.Aprobaciones.Aplicacion;
using AlxorCore.Aprobaciones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Aprobaciones.Infraestructura;

/// <summary>Contexto de persistencia del módulo Aprobaciones.</summary>
public sealed class AprobacionesDbContext : DbContextEmpresaBase, IUnidadDeTrabajoAprobaciones
{
    public AprobacionesDbContext(DbContextOptions<AprobacionesDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "aprobaciones";

    public DbSet<ReglaAprobacion> Reglas => Set<ReglaAprobacion>();

    public DbSet<SolicitudAprobacion> Solicitudes => Set<SolicitudAprobacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AprobacionesDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionRegla : IEntityTypeConfiguration<ReglaAprobacion>
{
    public void Configure(EntityTypeBuilder<ReglaAprobacion> b)
    {
        b.ToTable("regla_aprobacion");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(r => r.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(ReglaAprobacion.LongitudMaximaTipo).IsRequired();
        b.Property(r => r.UmbralImporte).HasColumnName("umbral_importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(r => r.Activa).HasColumnName("activa").IsRequired();
        b.Property(r => r.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(r => r.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        b.HasIndex(r => new { r.EmpresaId, r.TipoDocumento }).IsUnique().HasDatabaseName("ux_regla_aprobacion_empresa_tipo");
        b.Ignore(r => r.EventosDominio);
    }
}

internal sealed class ConfiguracionSolicitud : IEntityTypeConfiguration<SolicitudAprobacion>
{
    public void Configure(EntityTypeBuilder<SolicitudAprobacion> b)
    {
        b.ToTable("solicitud_aprobacion");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(s => s.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(ReglaAprobacion.LongitudMaximaTipo).IsRequired();
        b.Property(s => s.DocumentoId).HasColumnName("documento_id").IsRequired();
        b.Property(s => s.Referencia).HasColumnName("referencia").HasMaxLength(SolicitudAprobacion.LongitudMaximaReferencia).IsRequired();
        b.Property(s => s.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(s => s.SolicitanteUsuarioId).HasColumnName("solicitante_usuario_id").IsRequired();
        b.Property(s => s.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(s => s.AprobadorUsuarioId).HasColumnName("aprobador_usuario_id");
        b.Property(s => s.Motivo).HasColumnName("motivo").HasMaxLength(SolicitudAprobacion.LongitudMaximaMotivo);
        b.Property(s => s.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(s => s.ResueltaEn).HasColumnName("resuelta_en");

        b.HasIndex(s => new { s.EmpresaId, s.Estado, s.CreadoEn }).HasDatabaseName("ix_solicitud_aprobacion_empresa_estado");
        b.Ignore(s => s.EventosDominio);
    }
}

internal sealed class RepositorioReglas : IRepositorioReglas
{
    private readonly AprobacionesDbContext _ctx;

    public RepositorioReglas(AprobacionesDbContext ctx) => _ctx = ctx;

    public Task<ReglaAprobacion?> ObtenerPorTipoAsync(Guid empresaId, string tipoDocumento, CancellationToken ct = default) =>
        _ctx.Reglas.SingleOrDefaultAsync(r => r.EmpresaId == empresaId && r.TipoDocumento == tipoDocumento, ct);

    public void Agregar(ReglaAprobacion regla) => _ctx.Reglas.Add(regla);

    public async Task<IReadOnlyList<ReglaAprobacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Reglas.Where(r => r.EmpresaId == empresaId).OrderBy(r => r.TipoDocumento).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioSolicitudes : IRepositorioSolicitudes
{
    private readonly AprobacionesDbContext _ctx;

    public RepositorioSolicitudes(AprobacionesDbContext ctx) => _ctx = ctx;

    public Task<SolicitudAprobacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Solicitudes.SingleOrDefaultAsync(s => s.Id == id, ct);

    public void Agregar(SolicitudAprobacion solicitud) => _ctx.Solicitudes.Add(solicitud);

    public async Task<IReadOnlyList<SolicitudAprobacion>> ListarAsync(Guid empresaId, EstadoAprobacion? estado, CancellationToken ct = default)
    {
        var consulta = _ctx.Solicitudes.Where(s => s.EmpresaId == empresaId);
        if (estado is EstadoAprobacion e)
        {
            consulta = consulta.Where(s => s.Estado == e);
        }

        return await consulta.OrderByDescending(s => s.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class AprobacionesDbContextFactory : IDesignTimeDbContextFactory<AprobacionesDbContext>
{
    public AprobacionesDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<AprobacionesDbContext>().UseNpgsql(conexion).Options;
        return new AprobacionesDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
