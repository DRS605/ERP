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
    public DbSet<LoteArticulo> Lotes => Set<LoteArticulo>();

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
        b.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(80);
        b.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId, x.AlmacenId, x.UbicacionId, x.Lote }).HasDatabaseName("ix_existencia_clave");
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
        b.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(80);
        b.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType("numeric(14,4)");
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

internal sealed class ConfiguracionLote : IEntityTypeConfiguration<LoteArticulo>
{
    public void Configure(EntityTypeBuilder<LoteArticulo> b)
    {
        b.ToTable("lote_articulo");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(LoteArticulo.LongitudCodigo).IsRequired();
        b.Property(x => x.FechaCaducidad).HasColumnName("fecha_caducidad");
        b.Property(x => x.FechaFabricacion).HasColumnName("fecha_fabricacion");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId, x.Codigo }).IsUnique().HasDatabaseName("ux_lote_articulo");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioLotes : IRepositorioLotes
{
    private readonly InventarioDbContext _ctx;

    public RepositorioLotes(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(LoteArticulo lote) => _ctx.Lotes.Add(lote);

    public void Eliminar(LoteArticulo lote) => _ctx.Lotes.Remove(lote);

    public Task<LoteArticulo?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Lotes.FirstOrDefaultAsync(l => l.Id == id, ct);

    public Task<LoteArticulo?> ObtenerAsync(Guid empresaId, Guid productoId, string codigo, CancellationToken ct = default) =>
        _ctx.Lotes.FirstOrDefaultAsync(l => l.EmpresaId == empresaId && l.ProductoId == productoId && l.Codigo == codigo, ct);

    public async Task<IReadOnlyList<LoteArticulo>> ListarAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default) =>
        await _ctx.Lotes.AsNoTracking().Where(l => l.EmpresaId == empresaId && (productoId == null || l.ProductoId == productoId))
            .OrderBy(l => l.FechaCaducidad).ThenBy(l => l.Codigo).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioAlmacenes : IRepositorioAlmacenes
{
    private readonly InventarioDbContext _ctx;
    public RepositorioAlmacenes(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(Almacen almacen) => _ctx.Almacenes.Add(almacen);
    public Task<Almacen?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Almacenes.SingleOrDefaultAsync(a => a.Id == id, ct);
    public void AgregarUbicacion(Ubicacion ubicacion) => _ctx.Ubicaciones.Add(ubicacion);
    public Task<Ubicacion?> ObtenerUbicacionAsync(Guid id, CancellationToken ct = default) => _ctx.Ubicaciones.SingleOrDefaultAsync(u => u.Id == id, ct);
    public async Task<IReadOnlyList<Ubicacion>> UbicacionesDeAsync(Guid almacenId, CancellationToken ct = default) =>
        await _ctx.Ubicaciones.Where(u => u.AlmacenId == almacenId).ToListAsync(ct).ConfigureAwait(false);
    public void Eliminar(Almacen almacen) => _ctx.Almacenes.Remove(almacen);
    public void EliminarUbicacion(Ubicacion ubicacion) => _ctx.Ubicaciones.Remove(ubicacion);

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

    public Task<Existencia?> ObtenerAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, string? lote, CancellationToken ct = default) =>
        _ctx.Existencias.FirstOrDefaultAsync(e => e.EmpresaId == empresaId && e.ProductoId == productoId && e.AlmacenId == almacenId && e.UbicacionId == ubicacionId && e.Lote == lote, ct);

    public void Agregar(Existencia existencia) => _ctx.Existencias.Add(existencia);

    public Task<IReadOnlyList<ExistenciaDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        ProyectarAsync(_ctx.Existencias.AsNoTracking().Where(e => e.EmpresaId == empresaId && e.ProductoId == productoId), ct);

    public Task<IReadOnlyList<ExistenciaDto>> ListarPorAlmacenAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default) =>
        ProyectarAsync(_ctx.Existencias.AsNoTracking().Where(e => e.EmpresaId == empresaId && e.AlmacenId == almacenId), ct);

    public Task<IReadOnlyList<ExistenciaDto>> ListarPorLoteAsync(Guid empresaId, Guid productoId, string lote, CancellationToken ct = default) =>
        ProyectarAsync(_ctx.Existencias.AsNoTracking().Where(e => e.EmpresaId == empresaId && e.ProductoId == productoId && e.Lote == lote && e.Cantidad != 0m), ct);

    private async Task<IReadOnlyList<ExistenciaDto>> ProyectarAsync(IQueryable<Existencia> origen, CancellationToken ct)
    {
        var consulta =
            from e in origen
            join a in _ctx.Almacenes.AsNoTracking() on e.AlmacenId equals a.Id
            join u in _ctx.Ubicaciones.AsNoTracking() on e.UbicacionId equals u.Id into uj
            from u in uj.DefaultIfEmpty()
            select new ExistenciaDto(e.ProductoId, e.AlmacenId, a.Nombre, e.UbicacionId, u != null ? u.Codigo : null, e.Cantidad, e.Lote);
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
        return lista.Select(Proyectar).ToList();
    }

    public async Task<IReadOnlyList<MovimientoDto>> ListarPorLoteAsync(Guid empresaId, Guid productoId, string lote, CancellationToken ct = default)
    {
        var lista = await _ctx.Movimientos.AsNoTracking()
            .Where(m => m.EmpresaId == empresaId && m.ProductoId == productoId && m.Lote == lote)
            .OrderByDescending(m => m.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(Proyectar).ToList();
    }

    private static MovimientoDto Proyectar(MovimientoInventario m) =>
        new(m.Id, m.ProductoId, m.AlmacenId, m.UbicacionId, m.Tipo.ToString(), m.Cantidad, m.Fecha, m.Motivo, m.Referencia, m.Lote);
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

internal sealed class ConfiguracionRecuento : IEntityTypeConfiguration<RecuentoInventario>
{
    public void Configure(EntityTypeBuilder<RecuentoInventario> b)
    {
        b.ToTable("recuento_inventario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(RecuentoInventario.LongitudDescripcion);
        b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.AbiertoEn).HasColumnName("abierto_en").IsRequired();
        b.Property(x => x.CerradoEn).HasColumnName("cerrado_en");
        b.Property(x => x.NoContadosACero).HasColumnName("no_contados_a_cero").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_recuento_inventario_codigo");
        b.HasIndex(x => x.AlmacenId).HasDatabaseName("ix_recuento_inventario_almacen");
        b.HasIndex(x => x.UbicacionId).HasDatabaseName("ix_recuento_inventario_ubicacion");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_recuento");
            l.WithOwner().HasForeignKey("recuento_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("recuento_id").HasColumnName("recuento_id");
            l.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            l.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
            l.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(80);
            l.Property(x => x.Teorico).HasColumnName("teorico").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.Contado).HasColumnName("contado").HasColumnType("numeric(14,3)");
            l.Property(x => x.Diferencia).HasColumnName("diferencia").HasColumnType("numeric(14,3)");
            l.Property(x => x.Añadida).HasColumnName("anadida").IsRequired();
            l.HasIndex("recuento_id").HasDatabaseName("ix_linea_recuento_recuento");
            l.HasIndex(x => x.ProductoId).HasDatabaseName("ix_linea_recuento_producto");
            l.HasIndex(x => x.UbicacionId).HasDatabaseName("ix_linea_recuento_ubicacion");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioRecuentos : IRepositorioRecuentos
{
    private readonly InventarioDbContext _ctx;

    public RepositorioRecuentos(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(RecuentoInventario recuento) => _ctx.Add(recuento);

    public Task<RecuentoInventario?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Set<RecuentoInventario>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<RecuentoInventario>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<RecuentoInventario>().Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<int> ContarDelAnioAsync(Guid empresaId, int anio, CancellationToken ct = default) =>
        _ctx.Set<RecuentoInventario>().CountAsync(r => r.EmpresaId == empresaId && r.Fecha.Year == anio, ct);
}

internal sealed class ConfiguracionReglaReaprovisionamiento : IEntityTypeConfiguration<ReglaReaprovisionamiento>
{
    public void Configure(EntityTypeBuilder<ReglaReaprovisionamiento> b)
    {
        b.ToTable("regla_reaprovisionamiento");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id");
        b.Property(x => x.Minimo).HasColumnName("minimo").HasColumnType("numeric(14,3)").IsRequired();
        b.Property(x => x.Maximo).HasColumnName("maximo").HasColumnType("numeric(14,3)").IsRequired();
        b.Property(x => x.Multiplo).HasColumnName("multiplo").HasColumnType("numeric(14,3)").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_regla_reaprovisionamiento_producto");
        b.HasIndex(x => x.AlmacenId).HasDatabaseName("ix_regla_reaprovisionamiento_almacen");
        b.HasIndex(x => x.ProveedorId).HasDatabaseName("ix_regla_reaprovisionamiento_proveedor");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioReglasReaprovisionamiento : IRepositorioReglasReaprovisionamiento
{
    private readonly InventarioDbContext _ctx;

    public RepositorioReglasReaprovisionamiento(InventarioDbContext ctx) => _ctx = ctx;

    public void Agregar(ReglaReaprovisionamiento regla) => _ctx.Add(regla);

    public void Eliminar(ReglaReaprovisionamiento regla) => _ctx.Remove(regla);

    public Task<ReglaReaprovisionamiento?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Set<ReglaReaprovisionamiento>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ReglaReaprovisionamiento>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ReglaReaprovisionamiento>().Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);
}
