using AlxorCore.Integraciones.Aplicacion;
using AlxorCore.Integraciones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Integraciones.Infraestructura;

/// <summary>Contexto de persistencia del módulo Integraciones (claves de API y webhooks).</summary>
public sealed class IntegracionesDbContext : DbContextEmpresaBase, IUnidadDeTrabajoIntegraciones
{
    public IntegracionesDbContext(DbContextOptions<IntegracionesDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "integraciones";

    public DbSet<ClaveApi> Claves => Set<ClaveApi>();

    public DbSet<SuscripcionWebhook> Suscripciones => Set<SuscripcionWebhook>();

    public DbSet<EntregaWebhook> Entregas => Set<EntregaWebhook>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IntegracionesDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionClaveApi : IEntityTypeConfiguration<ClaveApi>
{
    public void Configure(EntityTypeBuilder<ClaveApi> b)
    {
        b.ToTable("clave_api");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(ClaveApi.LongitudMaximaNombre).IsRequired();
        b.Property(c => c.Prefijo).HasColumnName("prefijo").HasMaxLength(16).IsRequired();
        b.Property(c => c.HashSecreto).HasColumnName("hash_secreto").HasMaxLength(64).IsRequired();
        b.Property(c => c.Activa).HasColumnName("activa").IsRequired();
        b.Property(c => c.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(c => c.UltimoUsoEn).HasColumnName("ultimo_uso_en");
        b.Property(c => c.RevocadaEn).HasColumnName("revocada_en");

        // Índice único por hash: es la clave de búsqueda en la autenticación de la API pública.
        b.HasIndex(c => c.HashSecreto).IsUnique().HasDatabaseName("ux_clave_api_hash");
        b.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionSuscripcion : IEntityTypeConfiguration<SuscripcionWebhook>
{
    public void Configure(EntityTypeBuilder<SuscripcionWebhook> b)
    {
        b.ToTable("suscripcion_webhook");
        b.HasKey(s => s.Id);
        b.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(s => s.Url).HasColumnName("url").HasMaxLength(SuscripcionWebhook.LongitudMaximaUrl).IsRequired();
        b.Property(s => s.Secreto).HasColumnName("secreto").HasMaxLength(80).IsRequired();
        b.Property(s => s.Eventos).HasColumnName("eventos").HasMaxLength(500).IsRequired();
        b.Property(s => s.Activa).HasColumnName("activa").IsRequired();
        b.Property(s => s.CreadoEn).HasColumnName("creado_en").IsRequired();

        b.HasIndex(s => new { s.EmpresaId, s.Activa }).HasDatabaseName("ix_suscripcion_webhook_empresa");
        b.Ignore(s => s.EventosDominio);
    }
}

internal sealed class ConfiguracionEntrega : IEntityTypeConfiguration<EntregaWebhook>
{
    public void Configure(EntityTypeBuilder<EntregaWebhook> b)
    {
        b.ToTable("entrega_webhook");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(e => e.SuscripcionId).HasColumnName("suscripcion_id").IsRequired();
        b.Property(e => e.Url).HasColumnName("url").HasMaxLength(SuscripcionWebhook.LongitudMaximaUrl).IsRequired();
        b.Property(e => e.SecretoFirma).HasColumnName("secreto_firma").HasMaxLength(80).IsRequired();
        b.Property(e => e.Evento).HasColumnName("evento").HasMaxLength(60).IsRequired();
        b.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        b.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(e => e.Intentos).HasColumnName("intentos").IsRequired();
        b.Property(e => e.ProximoIntento).HasColumnName("proximo_intento").IsRequired();
        b.Property(e => e.UltimaRespuesta).HasColumnName("ultima_respuesta").HasMaxLength(300);
        b.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(e => e.EntregadaEn).HasColumnName("entregada_en");

        b.HasIndex(e => new { e.Estado, e.ProximoIntento }).HasDatabaseName("ix_entrega_webhook_estado");
        b.Ignore(e => e.EventosDominio);
    }
}

internal sealed class RepositorioClavesApi : IRepositorioClavesApi
{
    private readonly IntegracionesDbContext _ctx;

    public RepositorioClavesApi(IntegracionesDbContext ctx) => _ctx = ctx;

    public void Agregar(ClaveApi clave) => _ctx.Claves.Add(clave);

    public Task<ClaveApi?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Claves.SingleOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ClaveApi>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Claves.Where(c => c.EmpresaId == empresaId).OrderByDescending(c => c.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    // Autenticación: se busca sin filtro de empresa (aún no hay tenant resuelto). El hash es único.
    public Task<ClaveApi?> ObtenerPorHashAsync(string hashSecreto, CancellationToken ct = default) =>
        _ctx.Claves.IgnoreQueryFilters().SingleOrDefaultAsync(c => c.HashSecreto == hashSecreto, ct);
}

internal sealed class RepositorioSuscripciones : IRepositorioSuscripciones
{
    private readonly IntegracionesDbContext _ctx;

    public RepositorioSuscripciones(IntegracionesDbContext ctx) => _ctx = ctx;

    public void Agregar(SuscripcionWebhook suscripcion) => _ctx.Suscripciones.Add(suscripcion);

    public Task<SuscripcionWebhook?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Suscripciones.SingleOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SuscripcionWebhook>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Suscripciones.Where(s => s.EmpresaId == empresaId).OrderByDescending(s => s.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<SuscripcionWebhook>> ListarActivasPorEventoAsync(Guid empresaId, string evento, CancellationToken ct = default)
    {
        // Los eventos se guardan como CSV; se filtran en memoria sobre las suscripciones activas.
        var activas = await _ctx.Suscripciones.Where(s => s.EmpresaId == empresaId && s.Activa).ToListAsync(ct).ConfigureAwait(false);
        return activas.Where(s => s.Suscrito(evento)).ToList();
    }
}

internal sealed class RepositorioEntregas : IRepositorioEntregas
{
    private readonly IntegracionesDbContext _ctx;

    public RepositorioEntregas(IntegracionesDbContext ctx) => _ctx = ctx;

    public void Agregar(EntregaWebhook entrega) => _ctx.Entregas.Add(entrega);

    public async Task<IReadOnlyList<EntregaWebhook>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Entregas.Where(e => e.EmpresaId == empresaId).OrderByDescending(e => e.CreadoEn).Take(200).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<EntregaWebhook>> PendientesAsync(Guid empresaId, DateTimeOffset ahora, CancellationToken ct = default) =>
        await _ctx.Entregas
            .Where(e => e.EmpresaId == empresaId && e.Estado == EstadoEntrega.Pendiente && e.ProximoIntento <= ahora)
            .OrderBy(e => e.CreadoEn)
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Guid>> EmpresasConPendientesAsync(DateTimeOffset ahora, CancellationToken ct = default) =>
        await _ctx.Entregas.IgnoreQueryFilters()
            .Where(e => e.Estado == EstadoEntrega.Pendiente && e.ProximoIntento <= ahora)
            .Select(e => e.EmpresaId).Distinct()
            .ToListAsync(ct).ConfigureAwait(false);
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class IntegracionesDbContextFactory : IDesignTimeDbContextFactory<IntegracionesDbContext>
{
    public IntegracionesDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<IntegracionesDbContext>().UseNpgsql(conexion).Options;
        return new IntegracionesDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
