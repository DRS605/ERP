using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Persistencia;
using AlxorCore.Produccion.Aplicacion;
using AlxorCore.Produccion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Produccion.Infraestructura;

/// <summary>Contexto de persistencia del módulo Producción.</summary>
public sealed class ProduccionDbContext : DbContextEmpresaBase, IUnidadDeTrabajoProduccion
{
    public ProduccionDbContext(DbContextOptions<ProduccionDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "produccion";

    public DbSet<OrdenFabricacion> Ordenes => Set<OrdenFabricacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProduccionDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionOrden : IEntityTypeConfiguration<OrdenFabricacion>
{
    public void Configure(EntityTypeBuilder<OrdenFabricacion> b)
    {
        b.ToTable("orden_fabricacion");
        b.HasKey(o => o.Id);
        b.Property(o => o.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(o => o.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(o => o.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(o => o.Numero).HasColumnName("numero").IsRequired();
        b.Property(o => o.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(o => o.ProductoNombre).HasColumnName("producto_nombre").HasMaxLength(OrdenFabricacion.LongitudMaximaTexto).IsRequired();
        b.Property(o => o.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        b.Property(o => o.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(o => o.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(o => o.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(o => o.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(o => o.TerminadaEn).HasColumnName("terminada_en");
        b.OwnsMany(o => o.Componentes, c =>
        {
            c.ToTable("componente_plan");
            c.WithOwner().HasForeignKey("orden_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.ComponenteId).HasColumnName("componente_id").IsRequired();
            c.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(OrdenFabricacion.LongitudMaximaTexto).IsRequired();
            c.Property(x => x.CantidadUnitaria).HasColumnName("cantidad_unitaria").HasColumnType("numeric(14,3)").IsRequired();
            c.Property(x => x.CantidadTotal).HasColumnName("cantidad_total").HasColumnType("numeric(14,3)").IsRequired();
        });
        b.HasIndex(o => new { o.EmpresaId, o.Ejercicio, o.Numero }).IsUnique().HasDatabaseName("ux_orden_serie");
        b.Ignore(o => o.EventosDominio);
    }
}

internal sealed class RepositorioOrdenes : IRepositorioOrdenes
{
    private readonly ProduccionDbContext _ctx;
    public RepositorioOrdenes(ProduccionDbContext ctx) => _ctx = ctx;

    public void Agregar(OrdenFabricacion orden) => _ctx.Ordenes.Add(orden);

    public Task<OrdenFabricacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Ordenes.SingleOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<OrdenFabricacionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _ctx.Ordenes.AsNoTracking().Where(o => o.EmpresaId == empresaId)
            .OrderByDescending(o => o.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(OrdenFabricacionDto.Desde).ToList();
    }

    public async Task<OrdenFabricacionDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default)
    {
        var o = await _ctx.Ordenes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return o is null ? null : OrdenFabricacionDto.Desde(o);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var max = await _ctx.Ordenes.AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && o.Ejercicio == ejercicio)
            .MaxAsync(o => (int?)o.Numero, ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

/// <summary>Adaptador de <see cref="IConsultaListaMateriales"/> sobre el módulo Catálogo.</summary>
internal sealed class ConsultaListaMaterialesCatalogo : IConsultaListaMateriales
{
    private readonly ObtenerComposicion _composicion;
    private readonly ObtenerProducto _producto;

    public ConsultaListaMaterialesCatalogo(ObtenerComposicion composicion, ObtenerProducto producto)
    {
        _composicion = composicion; _producto = producto;
    }

    public async Task<(string Nombre, IReadOnlyList<(Guid ComponenteId, string Nombre, decimal Cantidad)> Componentes)?> ObtenerAsync(Guid productoId, CancellationToken ct = default)
    {
        var comp = await _composicion.EjecutarAsync(productoId, ct).ConfigureAwait(false);
        if (comp.EsFallo || !comp.Valor.EsCompuesto || comp.Valor.Componentes.Count == 0)
        {
            return null;
        }

        var prod = await _producto.EjecutarAsync(productoId, ct).ConfigureAwait(false);
        var nombre = prod.EsCorrecto ? prod.Valor.Nombre : "Artículo";
        var componentes = comp.Valor.Componentes.Select(c => (c.ComponenteId, c.Nombre, c.Cantidad)).ToList();
        return (nombre, componentes);
    }
}

/// <summary>Adaptador de <see cref="IMontajeProduccion"/> sobre el montaje de Inventario.</summary>
internal sealed class MontajeProduccionInventario : IMontajeProduccion
{
    private readonly MontajeArticulo _montaje;
    public MontajeProduccionInventario(MontajeArticulo montaje) => _montaje = montaje;

    public async Task<Resultado> MontarAsync(Guid empresaId, Guid productoId, decimal cantidad, Guid almacenId, CancellationToken ct = default)
    {
        var r = await _montaje.EjecutarAsync(empresaId, new MontajeComando(productoId, cantidad, almacenId), ct).ConfigureAwait(false);
        return r.EsCorrecto ? Resultado.Ok() : Resultado.Fallo(r.Error);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class ProduccionDbContextFactory : IDesignTimeDbContextFactory<ProduccionDbContext>
{
    public ProduccionDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ProduccionDbContext>().UseNpgsql(conexion).Options;
        return new ProduccionDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
