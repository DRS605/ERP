using AlxorCore.Compras.Aplicacion;
using AlxorCore.Compras.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Compras.Infraestructura;

/// <summary>Contexto de persistencia del módulo Compras.</summary>
public sealed class ComprasDbContext : DbContextEmpresaBase, IUnidadDeTrabajoCompras
{
    public ComprasDbContext(DbContextOptions<ComprasDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "compras";

    public DbSet<SolicitudCompra> Solicitudes => Set<SolicitudCompra>();

    public DbSet<PedidoCompra> Pedidos => Set<PedidoCompra>();

    public DbSet<AlbaranCompra> Albaranes => Set<AlbaranCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ComprasDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionSolicitud : IEntityTypeConfiguration<SolicitudCompra>
{
    public void Configure(EntityTypeBuilder<SolicitudCompra> builder)
    {
        builder.ToTable("solicitud_compra");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(s => s.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(s => s.ProveedorSugerido).HasColumnName("proveedor_sugerido").HasMaxLength(SolicitudCompra.LongitudMaximaTexto);
        builder.Property(s => s.Notas).HasColumnName("notas").HasMaxLength(500);
        builder.Property(s => s.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(s => s.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.OwnsMany(s => s.Lineas, l =>
        {
            l.ToTable("linea_solicitud");
            l.WithOwner().HasForeignKey("solicitud_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(SolicitudCompra.LongitudMaximaTexto).IsRequired();
            l.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        });
        builder.HasIndex(s => new { s.EmpresaId, s.Estado }).HasDatabaseName("ix_solicitud_empresa_estado");
        builder.Ignore(s => s.EventosDominio);
    }
}

internal sealed class ConfiguracionPedido : IEntityTypeConfiguration<PedidoCompra>
{
    public void Configure(EntityTypeBuilder<PedidoCompra> builder)
    {
        builder.ToTable("pedido_compra");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(p => p.ProveedorTexto).HasColumnName("proveedor_texto").HasMaxLength(PedidoCompra.LongitudMaximaTexto).IsRequired();
        builder.Property(p => p.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(p => p.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(p => p.Numero).HasColumnName("numero").IsRequired();
        builder.Property(p => p.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Ignore(p => p.NumeroCompleto);
        builder.Property(p => p.SolicitudOrigenId).HasColumnName("solicitud_origen_id");
        builder.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.OwnsMany(p => p.Lineas, l =>
        {
            l.ToTable("linea_pedido");
            l.WithOwner().HasForeignKey("pedido_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.ProductoId).HasColumnName("producto_id");
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(PedidoCompra.LongitudMaximaTexto).IsRequired();
            l.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.CantidadRecibida).HasColumnName("cantidad_recibida").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.CantidadFacturada).HasColumnName("cantidad_facturada").HasColumnType("numeric(14,3)").IsRequired();
        });
        builder.HasIndex(p => new { p.EmpresaId, p.Estado }).HasDatabaseName("ix_pedido_empresa_estado");
        builder.HasIndex(p => new { p.EmpresaId, p.Ejercicio, p.ProveedorId, p.Numero })
            .IsUnique().HasDatabaseName("ux_pedido_serie_proveedor");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class ConfiguracionAlbaran : IEntityTypeConfiguration<AlbaranCompra>
{
    public void Configure(EntityTypeBuilder<AlbaranCompra> builder)
    {
        builder.ToTable("albaran_compra");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.PedidoId).HasColumnName("pedido_id").IsRequired();
        builder.Property(a => a.Numero).HasColumnName("numero").IsRequired();
        builder.Property(a => a.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Ignore(a => a.NumeroCompleto);
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.Referencia).HasColumnName("referencia").HasMaxLength(120);
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.OwnsMany(a => a.Lineas, l =>
        {
            l.ToTable("linea_albaran");
            l.WithOwner().HasForeignKey("albaran_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.LineaPedidoId).HasColumnName("linea_pedido_id").IsRequired();
            l.Property(x => x.ProductoId).HasColumnName("producto_id");
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(PedidoCompra.LongitudMaximaTexto).IsRequired();
            l.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        });
        builder.HasIndex(a => new { a.EmpresaId, a.PedidoId }).HasDatabaseName("ix_albaran_empresa_pedido");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class RepositorioSolicitudes : IRepositorioSolicitudes
{
    private readonly ComprasDbContext _contexto;
    public RepositorioSolicitudes(ComprasDbContext contexto) => _contexto = contexto;

    public Task<SolicitudCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Solicitudes.SingleOrDefaultAsync(s => s.Id == id, ct);

    public void Agregar(SolicitudCompra solicitud) => _contexto.Solicitudes.Add(solicitud);

    public async Task<IReadOnlyList<SolicitudDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _contexto.Solicitudes.AsNoTracking().Where(s => s.EmpresaId == empresaId)
            .OrderByDescending(s => s.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(SolicitudDto.Desde).ToList();
    }

    public async Task<SolicitudDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _contexto.Solicitudes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return s is null ? null : SolicitudDto.Desde(s);
    }
}

internal sealed class RepositorioPedidos : IRepositorioPedidos
{
    private readonly ComprasDbContext _contexto;
    public RepositorioPedidos(ComprasDbContext contexto) => _contexto = contexto;

    public Task<PedidoCompra?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Pedidos.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PedidoCompra pedido) => _contexto.Pedidos.Add(pedido);

    public async Task<IReadOnlyList<PedidoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _contexto.Pedidos.AsNoTracking().Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(PedidoDto.Desde).ToList();
    }

    public async Task<PedidoDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _contexto.Pedidos.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return p is null ? null : PedidoDto.Desde(p);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, Guid? proveedorId, CancellationToken ct = default)
    {
        var query = _contexto.Pedidos.AsNoTracking().Where(p => p.EmpresaId == empresaId && p.Ejercicio == ejercicio);
        query = proveedorId is null
            ? query.Where(p => p.ProveedorId == null)
            : query.Where(p => p.ProveedorId == proveedorId);
        var max = await query.MaxAsync(p => (int?)p.Numero, ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

internal sealed class RepositorioAlbaranes : IRepositorioAlbaranes
{
    private readonly ComprasDbContext _contexto;
    public RepositorioAlbaranes(ComprasDbContext contexto) => _contexto = contexto;

    public void Agregar(AlbaranCompra albaran) => _contexto.Albaranes.Add(albaran);

    public async Task<IReadOnlyList<AlbaranDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default)
    {
        var lista = await _contexto.Albaranes.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.PedidoId == pedidoId)
            .OrderBy(a => a.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(AlbaranDto.Desde).ToList();
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var desde = new DateOnly(ejercicio, 1, 1);
        var hasta = new DateOnly(ejercicio, 12, 31);
        var max = await _contexto.Albaranes.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Fecha >= desde && a.Fecha <= hasta)
            .MaxAsync(a => (int?)a.Numero, ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class ComprasDbContextFactory : IDesignTimeDbContextFactory<ComprasDbContext>
{
    public ComprasDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ComprasDbContext>().UseNpgsql(conexion).Options;
        return new ComprasDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
