using AlxorCore.Divisas.Aplicacion;
using AlxorCore.Divisas.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Divisas.Infraestructura;

/// <summary>Contexto de persistencia del módulo Divisas.</summary>
public sealed class DivisasDbContext : DbContextEmpresaBase, IUnidadDeTrabajoDivisas
{
    public DivisasDbContext(DbContextOptions<DivisasDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "divisas";

    public DbSet<TipoCambio> TiposCambio => Set<TipoCambio>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DivisasDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionTipoCambio : IEntityTypeConfiguration<TipoCambio>
{
    public void Configure(EntityTypeBuilder<TipoCambio> b)
    {
        b.ToTable("tipo_cambio");
        b.HasKey(t => t.Id);
        b.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(t => t.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(t => t.Divisa).HasColumnName("divisa").HasMaxLength(3).IsRequired();
        b.Property(t => t.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(t => t.TasaEur).HasColumnName("tasa_eur").HasColumnType("numeric(18,6)").IsRequired();
        b.Property(t => t.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(t => t.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        b.HasIndex(t => new { t.EmpresaId, t.Divisa, t.Fecha }).IsUnique().HasDatabaseName("ux_tipo_cambio_empresa_divisa_fecha");
        b.Ignore(t => t.EventosDominio);
    }
}

internal sealed class RepositorioTiposCambio : IRepositorioTiposCambio, IConsultaTiposCambio
{
    private readonly DivisasDbContext _ctx;

    public RepositorioTiposCambio(DivisasDbContext ctx) => _ctx = ctx;

    public Task<TipoCambio?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.TiposCambio.SingleOrDefaultAsync(t => t.Id == id, ct);

    public Task<TipoCambio?> ObtenerPorDivisaFechaAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default) =>
        _ctx.TiposCambio.SingleOrDefaultAsync(t => t.EmpresaId == empresaId && t.Divisa == divisa && t.Fecha == fecha, ct);

    public void Agregar(TipoCambio tipoCambio) => _ctx.TiposCambio.Add(tipoCambio);

    public async Task<IReadOnlyList<TipoCambio>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.TiposCambio.Where(t => t.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<decimal?> TasaVigenteAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default)
    {
        var codigo = divisa.Trim().ToUpperInvariant();
        var tasa = await _ctx.TiposCambio
            .Where(t => t.EmpresaId == empresaId && t.Divisa == codigo && t.Fecha <= fecha)
            .OrderByDescending(t => t.Fecha)
            .Select(t => (decimal?)t.TasaEur)
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);
        return tasa;
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class DivisasDbContextFactory : IDesignTimeDbContextFactory<DivisasDbContext>
{
    public DivisasDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<DivisasDbContext>().UseNpgsql(conexion).Options;
        return new DivisasDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
