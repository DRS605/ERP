using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using AlxorCore.Personal.Aplicacion;
using AlxorCore.Personal.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Personal.Infraestructura;

/// <summary>Contexto de persistencia del módulo Personal.</summary>
public sealed class PersonalDbContext : DbContextEmpresaBase, IUnidadDeTrabajoPersonal
{
    public PersonalDbContext(DbContextOptions<PersonalDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "personal";

    public DbSet<Persona> Personas => Set<Persona>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PersonalDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionPersona : IEntityTypeConfiguration<Persona>
{
    public void Configure(EntityTypeBuilder<Persona> b)
    {
        b.ToTable("persona");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(Persona.LongitudMaximaNombre).IsRequired();
        b.Property(p => p.Puesto).HasColumnName("puesto").HasMaxLength(120);
        b.Property(p => p.TarifaHora).HasColumnName("tarifa_hora").HasColumnType("numeric(12,2)").IsRequired();
        b.Property(p => p.Activo).HasColumnName("activo").IsRequired();
        b.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(p => p.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.HasIndex(p => new { p.EmpresaId, p.Activo }).HasDatabaseName("ix_persona_empresa");
        b.Ignore(p => p.EventosDominio);
    }
}

internal sealed class RepositorioPersonas : IRepositorioPersonas, IConsultaPersonas
{
    private readonly PersonalDbContext _ctx;
    public RepositorioPersonas(PersonalDbContext ctx) => _ctx = ctx;

    public void Agregar(Persona persona) => _ctx.Personas.Add(persona);

    public Task<Persona?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Personas.SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<PersonaDto?> ObtenerAsync(Guid personaId, CancellationToken ct = default)
    {
        var p = await _ctx.Personas.AsNoTracking().SingleOrDefaultAsync(x => x.Id == personaId, ct).ConfigureAwait(false);
        return p is null ? null : PersonaDto.Desde(p);
    }

    public async Task<IReadOnlyList<PersonaDto>> ListarAsync(Guid empresaId, bool incluirInactivas = false, CancellationToken ct = default)
    {
        var q = _ctx.Personas.AsNoTracking().Where(p => p.EmpresaId == empresaId);
        if (!incluirInactivas)
        {
            q = q.Where(p => p.Activo);
        }

        var lista = await q.OrderBy(p => p.Nombre).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(PersonaDto.Desde).ToList();
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class PersonalDbContextFactory : IDesignTimeDbContextFactory<PersonalDbContext>
{
    public PersonalDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<PersonalDbContext>().UseNpgsql(conexion).Options;
        return new PersonalDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
