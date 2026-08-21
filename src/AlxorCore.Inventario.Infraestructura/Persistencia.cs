using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Inventario.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Inventario.Infraestructura;

/// <summary>Contexto de persistencia del módulo Inventario.</summary>
public sealed class InventarioDbContext : DbContextEmpresaBase, IUnidadDeTrabajoInventario
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "inventario";

    public DbSet<Almacen> Almacenes => Set<Almacen>();
    public DbSet<Ubicacion> Ubicaciones => Set<Ubicacion>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<UbicacionDefecto> UbicacionesDefecto => Set<UbicacionDefecto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventarioDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionAlmacen : IEntityTypeConfiguration<Almacen>
{
    public void Configure(EntityTypeBuilder<Almacen> b)
    {
        b.ToTable("almacen");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(Almacen.LongitudMaximaCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(Almacen.LongitudMaximaNombre).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_almacen_empresa_codigo");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionUbicacion : IEntityTypeConfiguration<Ubicacion>
{
    public void Configure(EntityTypeBuilder<Ubicacion> b)
    {
        b.ToTable("ubicacion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.AlmacenId, x.Codigo }).IsUnique().HasDatabaseName("ux_ubicacion_almacen_codigo");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionExistencia : IEntityTypeConfiguration<Existencia>
{
    public void Configure(EntityTypeBuilder<Existencia> b)
    {
        b.ToTable("existencia");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        b.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId, x.AlmacenId, x.UbicacionId }).HasDatabaseName("ix_existencia_clave");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionMovimiento : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> b)
    {
        b.ToTable("movimiento_inventario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        b.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(200);
        b.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(120);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId }).HasDatabaseName("ix_movimiento_empresa_producto");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionUbicacionDefecto : IEntityTypeConfiguration<UbicacionDefecto>
{
    public void Configure(EntityTypeBuilder<UbicacionDefecto> b)
    {
        b.ToTable("ubicacion_defecto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.UbicacionId).HasColumnName("ubicacion_id").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId, x.AlmacenId, x.ProveedorId }).HasDatabaseName("ix_ubidef_clave");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioAlmacenes : IRepositorioAlmacenes
{
    private readonly InventarioDbContext _ctx;
    public RepositorioAlmacenes(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(Almacen almacen) => _ctx.Almacenes.Add(almacen);
    public Task<Almacen?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Almacenes.SingleOrDefaultAsync(a => a.Id == id, ct);
    public void AgregarUbicacion(Ubicacion ubicacion) => _ctx.Ubicaciones.Add(ubicacion);

    public async Task<IReadOnlyList<AlmacenDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _ctx.Almacenes.AsNoTracking().Where(a => a.EmpresaId == empresaId).OrderBy(a => a.Codigo).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(AlmacenDto.Desde).ToList();
    }

    public async Task<IReadOnlyList<UbicacionDto>> ListarUbicacionesAsync(Guid empresaId, Guid? almacenId = null, CancellationToken ct = default)
    {
        var q = _ctx.Ubicaciones.AsNoTracking().Where(u => u.EmpresaId == empresaId);
        if (almacenId is { } aid)
        {
            q = q.Where(u => u.AlmacenId == aid);
        }

        var lista = await q.OrderBy(u => u.Codigo).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(UbicacionDto.Desde).ToList();
    }
}

internal sealed class RepositorioExistencias : IRepositorioExistencias
{
    private readonly InventarioDbContext _ctx;
    public RepositorioExistencias(InventarioDbContext ctx) => _ctx = ctx;

    public Task<Existencia?> ObtenerAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, CancellationToken ct = default) =>
        _ctx.Existencias.FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProductoId == productoId && e.AlmacenId == almacenId && e.UbicacionId == ubicacionId, ct);

    public void Agregar(Existencia existencia) => _ctx.Existencias.Add(existencia);

    public Task<IReadOnlyList<ExistenciaDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        ProyectarAsync(_ctx.Existencias.AsNoTracking().Where(e => e.EmpresaId == empresaId && e.ProductoId == productoId), ct);

    public Task<IReadOnlyList<ExistenciaDto>> ListarPorAlmacenAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default) =>
        ProyectarAsync(_ctx.Existencias.AsNoTracking().Where(e => e.EmpresaId == empresaId && e.AlmacenId == almacenId), ct);

    private async Task<IReadOnlyList<ExistenciaDto>> ProyectarAsync(IQueryable<Existencia> origen, CancellationToken ct)
    {
        var consulta =
            from e in origen
            join a in _ctx.Almacenes.AsNoTracking() on e.AlmacenId equals a.Id
            join u in _ctx.Ubicaciones.AsNoTracking() on e.UbicacionId equals u.Id into uj
            from u in uj.DefaultIfEmpty()
            select new ExistenciaDto(e.ProductoId, e.AlmacenId, a.Nombre, e.UbicacionId, u != null ? u.Codigo : null, e.Cantidad);
        return await consulta.ToListAsync(ct).ConfigureAwait(false);
    }
}

internal sealed class RepositorioMovimientos : IRepositorioMovimientos
{
    private readonly InventarioDbContext _ctx;
    public RepositorioMovimientos(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(MovimientoInventario movimiento) => _ctx.Movimientos.Add(movimiento);

    public async Task<IReadOnlyList<MovimientoDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var lista = await _ctx.Movimientos.AsNoTracking()
            .Where(m => m.EmpresaId == empresaId && m.ProductoId == productoId)
            .OrderByDescending(m => m.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(m => new MovimientoDto(m.Id, m.ProductoId, m.AlmacenId, m.UbicacionId, m.Tipo.ToString(), m.Cantidad, m.Fecha, m.Motivo, m.Referencia)).ToList();
    }
}

internal sealed class RepositorioUbicacionesDefecto : IRepositorioUbicacionesDefecto
{
    private readonly InventarioDbContext _ctx;
    public RepositorioUbicacionesDefecto(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(UbicacionDefecto regla) => _ctx.UbicacionesDefecto.Add(regla);

    public Task<UbicacionDefecto?> ObtenerAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, CancellationToken ct = default) =>
        _ctx.UbicacionesDefecto.FirstOrDefaultAsync(r => r.EmpresaId == empresaId && r.ProductoId == productoId && r.AlmacenId == almacenId && r.ProveedorId == proveedorId, ct);

    public async Task<IReadOnlyList<UbicacionDefectoDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var consulta =
            from r in _ctx.UbicacionesDefecto.AsNoTracking().Where(r => r.EmpresaId == empresaId && r.ProductoId == productoId)
            join a in _ctx.Almacenes.AsNoTracking() on r.AlmacenId equals a.Id
            join u in _ctx.Ubicaciones.AsNoTracking() on r.UbicacionId equals u.Id
            select new UbicacionDefectoDto(r.Id, r.ProductoId, r.AlmacenId, a.Nombre, r.ProveedorId, r.UbicacionId, u.Codigo);
        return await consulta.ToListAsync(ct).ConfigureAwait(false);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class InventarioDbContextFactory : IDesignTimeDbContextFactory<InventarioDbContext>
{
    public InventarioDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<InventarioDbContext>().UseNpgsql(conexion).Options;
        return new InventarioDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
