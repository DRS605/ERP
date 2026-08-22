using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using AlxorCore.Personal.Aplicacion;
using AlxorCore.Proyectos.Aplicacion;
using AlxorCore.Proyectos.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Proyectos.Infraestructura;

/// <summary>Contexto de persistencia del módulo Proyectos.</summary>
public sealed class ProyectosDbContext : DbContextEmpresaBase, IUnidadDeTrabajoProyectos
{
    public ProyectosDbContext(DbContextOptions<ProyectosDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "proyectos";

    public DbSet<Proyecto> Proyectos => Set<Proyecto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProyectosDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionProyecto : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> b)
    {
        b.ToTable("proyecto");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(p => p.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(p => p.Numero).HasColumnName("numero").IsRequired();
        b.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(Proyecto.LongitudMaximaNombre).IsRequired();
        b.Property(p => p.ClienteId).HasColumnName("cliente_id");
        b.Property(p => p.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(Proyecto.LongitudMaximaNombre);
        b.Property(p => p.Presupuesto).HasColumnName("presupuesto").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(p => p.FechaInicio).HasColumnName("fecha_inicio").IsRequired();
        b.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(p => p.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.Property(p => p.CerradoEn).HasColumnName("cerrado_en");
        b.OwnsMany(p => p.Imputaciones, i =>
        {
            i.ToTable("imputacion");
            i.WithOwner().HasForeignKey("proyecto_id");
            i.HasKey(x => x.Id);
            i.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            i.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
            i.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            i.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(Proyecto.LongitudMaximaDescripcion).IsRequired();
            i.Property(x => x.ReferenciaId).HasColumnName("referencia_id");
            i.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            i.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType("numeric(14,4)").IsRequired();
            i.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        });
        b.Ignore(p => p.EventosDominio);
        b.Ignore(p => p.CosteManoObra);
        b.Ignore(p => p.CosteMateriales);
        b.Ignore(p => p.CosteGastos);
        b.Ignore(p => p.CosteReal);
        b.Ignore(p => p.Desviacion);
        b.HasIndex(p => new { p.EmpresaId, p.Ejercicio, p.Numero }).IsUnique().HasDatabaseName("ux_proyecto_serie");
    }
}

internal sealed class RepositorioProyectos : IRepositorioProyectos
{
    private readonly ProyectosDbContext _ctx;
    public RepositorioProyectos(ProyectosDbContext ctx) => _ctx = ctx;

    public void Agregar(Proyecto proyecto) => _ctx.Proyectos.Add(proyecto);

    public Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Proyectos.Include(p => p.Imputaciones).SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<ProyectoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _ctx.Proyectos.AsNoTracking().Include(p => p.Imputaciones)
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.Ejercicio).ThenByDescending(p => p.Numero)
            .ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(ProyectoDto.Desde).ToList();
    }

    public async Task<ProyectoDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _ctx.Proyectos.AsNoTracking().Include(x => x.Imputaciones).SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return p is null ? null : ProyectoDetalleDto.Desde(p);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var max = await _ctx.Proyectos.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId && p.Ejercicio == ejercicio)
            .MaxAsync(p => (int?)p.Numero, ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

/// <summary>Adaptador de <see cref="ITarifaPersona"/> sobre el módulo Personal.</summary>
internal sealed class TarifaPersonaPersonal : ITarifaPersona
{
    private readonly IConsultaPersonas _personas;
    public TarifaPersonaPersonal(IConsultaPersonas personas) => _personas = personas;

    public async Task<(string Nombre, decimal TarifaHora)?> ObtenerAsync(Guid personaId, CancellationToken ct = default)
    {
        var p = await _personas.ObtenerAsync(personaId, ct).ConfigureAwait(false);
        return p is null ? null : (p.Nombre, p.TarifaHora);
    }
}

/// <summary>Adaptador de <see cref="IConsultaArticuloProyectos"/> sobre el módulo Catálogo.</summary>
internal sealed class ConsultaArticuloProyectosCatalogo : IConsultaArticuloProyectos
{
    private readonly ObtenerProducto _producto;
    public ConsultaArticuloProyectosCatalogo(ObtenerProducto producto) => _producto = producto;

    public async Task<string?> NombreAsync(Guid productoId, CancellationToken ct = default)
    {
        var r = await _producto.EjecutarAsync(productoId, ct).ConfigureAwait(false);
        return r.EsCorrecto ? r.Valor.Nombre : null;
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class ProyectosDbContextFactory : IDesignTimeDbContextFactory<ProyectosDbContext>
{
    public ProyectosDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ProyectosDbContext>().UseNpgsql(conexion).Options;
        return new ProyectosDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
