using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Catalogo.Infraestructura;

/// <summary>Contexto de persistencia del módulo Catálogo.</summary>
public sealed class CatalogoDbContext : DbContextEmpresaBase, IUnidadDeTrabajoCatalogo
{
    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "catalogo";

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<Familia> Familias => Set<Familia>();

    public DbSet<HistoricoPrecio> HistoricoPrecios => Set<HistoricoPrecio>();

    public DbSet<MovimientoStock> MovimientosStock => Set<MovimientoStock>();

    public DbSet<ExistenciaSimple> Existencias => Set<ExistenciaSimple>();

    public DbSet<TipoIva> TiposIva => Set<TipoIva>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogoDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionProducto : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("producto");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(p => p.Referencia).HasColumnName("referencia").HasMaxLength(60);
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(Producto.LongitudMaximaNombre).IsRequired();
        builder.Property(p => p.Familia).HasColumnName("familia").HasMaxLength(Producto.LongitudMaximaFamilia);
        builder.Property(p => p.FamiliaId).HasColumnName("familia_id");
        builder.Property(p => p.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(p => p.PrecioCompra).HasColumnName("precio_compra").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(p => p.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
        builder.Property(p => p.Unidad).HasColumnName("unidad").HasMaxLength(20).IsRequired();
        builder.Property(p => p.UnidadCompra).HasColumnName("unidad_compra").HasMaxLength(20);
        builder.Property(p => p.FactorCompra).HasColumnName("factor_compra").HasColumnType("numeric(14,4)").IsRequired();
        builder.Property(p => p.UnidadVenta).HasColumnName("unidad_venta").HasMaxLength(20);
        builder.Property(p => p.FactorVenta).HasColumnName("factor_venta").HasColumnType("numeric(14,4)").IsRequired();
        builder.Property(p => p.Seguimiento).HasColumnName("seguimiento").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(p => p.EsCompuesto).HasColumnName("es_compuesto").IsRequired();
        builder.Property(p => p.ProductoPadreId).HasColumnName("producto_padre_id");
        builder.Property(p => p.EsPlantilla).HasColumnName("es_plantilla").IsRequired();
        builder.Property(p => p.ProveedorHabitualId).HasColumnName("proveedor_habitual_id");
        builder.Property(p => p.ControlarStock).HasColumnName("controlar_stock").IsRequired();
        builder.Property(p => p.ActividadNegocioId).HasColumnName("actividad_negocio_id");
        builder.Property(p => p.Activo).HasColumnName("activo").IsRequired();
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(p => p.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.HasIndex(p => new { p.GrupoId, p.Nombre }).HasDatabaseName("ix_producto_grupo_nombre");
        builder.HasIndex(p => p.FamiliaId).HasDatabaseName("ix_producto_familia");
        builder.Ignore(p => p.EventosDominio);
        // Propiedades calculadas (no se persisten).
        builder.Ignore(p => p.UnidadCompraEfectiva);
        builder.Ignore(p => p.UnidadVentaEfectiva);
        builder.Ignore(p => p.PrecioCompraPorUnidadCompra);
        builder.Ignore(p => p.PrecioVentaPorUnidadVenta);
        builder.Ignore(p => p.RequiereLoteOSerie);
        builder.Ignore(p => p.EsVariante);
        builder.Ignore(p => p.ResumenVariante);
        builder.OwnsMany(p => p.Componentes, c =>
        {
            c.ToTable("componente_articulo");
            c.WithOwner().HasForeignKey("producto_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.ComponenteId).HasColumnName("componente_id").IsRequired();
            c.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        });
        builder.OwnsMany(p => p.Atributos, a =>
        {
            a.ToTable("atributo_variante");
            a.WithOwner().HasForeignKey("producto_id");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            a.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(60).IsRequired();
            a.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(80).IsRequired();
        });
    }
}

internal sealed class ConfiguracionFamilia : IEntityTypeConfiguration<Familia>
{
    public void Configure(EntityTypeBuilder<Familia> builder)
    {
        builder.ToTable("familia");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(f => f.Nombre).HasColumnName("nombre").HasMaxLength(Familia.LongitudMaximaNombre).IsRequired();
        builder.Property(f => f.Codigo).HasColumnName("codigo").HasMaxLength(Familia.LongitudMaximaCodigo);
        builder.Property(f => f.PadreId).HasColumnName("padre_id");
        builder.Property(f => f.Activo).HasColumnName("activo").IsRequired();
        builder.Property(f => f.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(f => f.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.HasIndex(f => new { f.GrupoId, f.PadreId }).HasDatabaseName("ix_familia_grupo_padre");
        builder.Ignore(f => f.EventosDominio);
    }
}

internal sealed class ConfiguracionHistoricoPrecio : IEntityTypeConfiguration<HistoricoPrecio>
{
    public void Configure(EntityTypeBuilder<HistoricoPrecio> builder)
    {
        builder.ToTable("historico_precio");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(h => h.ProductoId).HasColumnName("producto_id").IsRequired();
        builder.Property(h => h.PrecioVenta).HasColumnName("precio_venta").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(h => h.PrecioCompra).HasColumnName("precio_compra").HasColumnType("numeric(12,2)").IsRequired();
        builder.Property(h => h.RegistradoEn).HasColumnName("registrado_en").IsRequired();

        builder.HasIndex(h => new { h.GrupoId, h.ProductoId, h.RegistradoEn }).HasDatabaseName("ix_historico_precio_producto");
        builder.Ignore(h => h.EventosDominio);
    }
}

internal sealed class ConfiguracionMovimientoStock : IEntityTypeConfiguration<MovimientoStock>
{
    public void Configure(EntityTypeBuilder<MovimientoStock> builder)
    {
        builder.ToTable("movimiento_stock");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(m => m.ProductoId).HasColumnName("producto_id").IsRequired();
        builder.Property(m => m.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(m => m.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        builder.Property(m => m.StockResultante).HasColumnName("stock_resultante").HasColumnType("numeric(14,3)").IsRequired();
        builder.Property(m => m.Motivo).HasColumnName("motivo").HasMaxLength(200);
        builder.Property(m => m.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(m => new { m.EmpresaId, m.ProductoId, m.CreadoEn }).HasDatabaseName("ix_movimiento_stock_producto");
        builder.Ignore(m => m.EventosDominio);
    }
}

internal sealed class ConfiguracionTipoIva : IEntityTypeConfiguration<TipoIva>
{
    public void Configure(EntityTypeBuilder<TipoIva> builder)
    {
        builder.ToTable("tipo_iva");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(t => t.Codigo).HasColumnName("codigo").HasMaxLength(TipoIva.LongitudMaximaCodigo).IsRequired();
        builder.Property(t => t.Nombre).HasColumnName("nombre").HasMaxLength(TipoIva.LongitudMaximaNombre).IsRequired();
        builder.Property(t => t.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(t => t.RecargoEquivalencia).HasColumnName("recargo_equivalencia").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(t => t.Clase).HasColumnName("clase").HasMaxLength(30).HasConversion<string>().IsRequired();
        builder.Property(t => t.MencionFactura).HasColumnName("mencion_factura").HasMaxLength(TipoIva.LongitudMaximaMencion);
        builder.Property(t => t.Activo).HasColumnName("activo").IsRequired();
        builder.Property(t => t.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(t => t.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.HasIndex(t => new { t.EmpresaId, t.Codigo }).IsUnique().HasDatabaseName("ux_tipo_iva_empresa_codigo");
        builder.Ignore(t => t.EventosDominio);
        builder.Ignore(t => t.PorcentajeRepercutido);
    }
}

internal sealed class RepositorioTiposIva : IRepositorioTiposIva, IResolverIvaEmpresa
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioTiposIva(CatalogoDbContext contexto) => _contexto = contexto;

    public void Agregar(TipoIva tipo) => _contexto.TiposIva.Add(tipo);

    public Task<TipoIva?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.TiposIva.SingleOrDefaultAsync(t => t.Id == id, ct);

    public Task<TipoIva?> ObtenerPorCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default)
    {
        var c = codigo.Trim().ToUpperInvariant();
        return _contexto.TiposIva.SingleOrDefaultAsync(t => t.EmpresaId == empresaId && t.Codigo == c, ct);
    }

    public async Task<IReadOnlyList<TipoIva>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.TiposIva.Where(t => t.EmpresaId == empresaId).OrderBy(t => t.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default)
    {
        var c = codigo.Trim().ToUpperInvariant();
        return _contexto.TiposIva.AnyAsync(t => t.EmpresaId == empresaId && t.Codigo == c, ct);
    }

    public async Task<IvaResuelto?> ResolverAsync(Guid empresaId, string codigo, CancellationToken ct = default)
    {
        var t = await ObtenerPorCodigoAsync(empresaId, codigo, ct).ConfigureAwait(false);
        return t is null
            ? null
            : new IvaResuelto(t.Codigo, t.Porcentaje, t.RecargoEquivalencia, t.Clase, t.PorcentajeRepercutido, t.MencionFactura);
    }
}

internal sealed class ConfiguracionExistenciaSimple : IEntityTypeConfiguration<ExistenciaSimple>
{
    public void Configure(EntityTypeBuilder<ExistenciaSimple> builder)
    {
        builder.ToTable("existencia_simple");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.ProductoId).HasColumnName("producto_id").IsRequired();
        builder.Property(e => e.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        builder.Property(e => e.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.HasIndex(e => new { e.EmpresaId, e.ProductoId }).IsUnique().HasDatabaseName("ux_existencia_simple_empresa_producto");
        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class RepositorioExistenciasSimples : IRepositorioExistenciasSimples
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioExistenciasSimples(CatalogoDbContext contexto) => _contexto = contexto;

    public Task<ExistenciaSimple?> ObtenerPorProductoAsync(Guid productoId, CancellationToken ct = default) =>
        _contexto.Existencias.SingleOrDefaultAsync(e => e.ProductoId == productoId, ct);

    public void Agregar(ExistenciaSimple existencia) => _contexto.Existencias.Add(existencia);
}

internal sealed class RepositorioMovimientosStock : IRepositorioMovimientosStock, IConsultaMovimientosStock
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioMovimientosStock(CatalogoDbContext contexto) => _contexto = contexto;

    public void Agregar(MovimientoStock movimiento) => _contexto.MovimientosStock.Add(movimiento);

    public async Task<IReadOnlyList<MovimientoStockDto>> ListarPorProductoAsync(Guid productoId, CancellationToken ct = default)
    {
        var filas = await _contexto.MovimientosStock
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.CreadoEn)
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(MovimientoStockDto.Desde).ToList();
    }
}

internal sealed class RepositorioHistoricoPrecios : IRepositorioHistoricoPrecios, IConsultaHistoricoPrecios
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioHistoricoPrecios(CatalogoDbContext contexto) => _contexto = contexto;

    public void Agregar(HistoricoPrecio historico) => _contexto.HistoricoPrecios.Add(historico);

    public async Task<IReadOnlyList<HistoricoPrecioDto>> ListarPorProductoAsync(Guid productoId, CancellationToken ct = default)
    {
        var filas = await _contexto.HistoricoPrecios
            .Where(h => h.ProductoId == productoId)
            .OrderByDescending(h => h.RegistradoEn)
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(HistoricoPrecioDto.Desde).ToList();
    }
}

internal sealed class RepositorioProductos : IRepositorioProductos, IConsultaProductos
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioProductos(CatalogoDbContext contexto) => _contexto = contexto;

    public Task<Producto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Productos.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(Producto producto) => _contexto.Productos.Add(producto);

    public async Task<ProductoDto?> ObtenerAsync(Guid productoId, CancellationToken ct = default)
    {
        var producto = await _contexto.Productos.SingleOrDefaultAsync(p => p.Id == productoId, ct).ConfigureAwait(false);
        if (producto is null)
        {
            return null;
        }

        var stock = await _contexto.Existencias.Where(e => e.ProductoId == productoId)
            .Select(e => (decimal?)e.Cantidad).SingleOrDefaultAsync(ct).ConfigureAwait(false) ?? 0m;
        return ProductoDto.Desde(producto, stock);
    }

    // El catálogo se comparte por grupo (filtro global); las existencias son por empresa (filtro
    // global de ExistenciaSimple). El tenant que llega es el grupo, pero la RLS/filtro ya lo aplica.
    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(Guid grupoId, bool incluirInactivos = false, IReadOnlyCollection<Guid>? actividadesPermitidas = null, CancellationToken ct = default)
    {
        var consulta = _contexto.Productos.AsQueryable();
        if (!incluirInactivos)
        {
            consulta = consulta.Where(p => p.Activo);
        }

        consulta = FiltrarPorActividad(consulta, actividadesPermitidas);

        var productos = await consulta.OrderBy(p => p.Nombre).ToListAsync(ct).ConfigureAwait(false);
        return await ConDtoAsync(productos, ct).ConfigureAwait(false);
    }

    public async Task<PaginaResultado<ProductoDto>> BuscarAsync(Guid grupoId, FiltroProductos filtro, Paginacion paginacion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ArgumentNullException.ThrowIfNull(paginacion);

        var consulta = _contexto.Productos.AsQueryable();
        if (!filtro.IncluirInactivos)
        {
            consulta = consulta.Where(p => p.Activo);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var patron = $"%{filtro.Texto.Trim()}%";
            consulta = consulta.Where(p =>
                EF.Functions.ILike(p.Nombre, patron) ||
                (p.Referencia != null && EF.Functions.ILike(p.Referencia, patron)));
        }

        if (filtro.FamiliaId is Guid familiaId)
        {
            consulta = consulta.Where(p => p.FamiliaId == familiaId);
        }

        consulta = FiltrarPorActividad(consulta, filtro.ActividadesPermitidas);

        var total = await consulta.CountAsync(ct).ConfigureAwait(false);
        var productos = await consulta
            .OrderBy(p => p.Nombre)
            .Skip(paginacion.Saltar).Take(paginacion.TamanoPagina)
            .ToListAsync(ct).ConfigureAwait(false);
        return PaginaResultado<ProductoDto>.Crear(await ConDtoAsync(productos, ct).ConfigureAwait(false), total, paginacion);
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarVariantesAsync(Guid padreId, CancellationToken ct = default)
    {
        var variantes = await _contexto.Productos.Where(p => p.ProductoPadreId == padreId)
            .OrderBy(p => p.Nombre).ToListAsync(ct).ConfigureAwait(false);
        return await ConDtoAsync(variantes, ct).ConfigureAwait(false);
    }

    // Visibilidad por actividad (área Artículos): null => sin restricción; con conjunto => solo esas
    // actividades más los artículos sin actividad (visibles siempre).
    private static IQueryable<Producto> FiltrarPorActividad(IQueryable<Producto> consulta, IReadOnlyCollection<Guid>? actividadesPermitidas)
    {
        if (actividadesPermitidas is null)
        {
            return consulta;
        }

        var ids = actividadesPermitidas as IReadOnlyList<Guid> ?? actividadesPermitidas.ToList();
        return consulta.Where(p => p.ActividadNegocioId == null || ids.Contains(p.ActividadNegocioId.Value));
    }

    // Completa cada DTO con las existencias de la empresa activa (una consulta para todos los ids).
    private async Task<IReadOnlyList<ProductoDto>> ConDtoAsync(IReadOnlyList<Producto> productos, CancellationToken ct)
    {
        if (productos.Count == 0)
        {
            return Array.Empty<ProductoDto>();
        }

        var ids = productos.Select(p => p.Id).ToList();
        var stocks = await _contexto.Existencias.Where(e => ids.Contains(e.ProductoId))
            .ToDictionaryAsync(e => e.ProductoId, e => e.Cantidad, ct).ConfigureAwait(false);
        return productos.Select(p => ProductoDto.Desde(p, stocks.GetValueOrDefault(p.Id))).ToList();
    }
}

internal sealed class RepositorioFamilias : IRepositorioFamilias, IConsultaFamilias
{
    private readonly CatalogoDbContext _contexto;

    public RepositorioFamilias(CatalogoDbContext contexto) => _contexto = contexto;

    public Task<Familia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Familias.SingleOrDefaultAsync(f => f.Id == id, ct);

    public async Task<IReadOnlyList<Familia>> ListarTodasAsync(Guid grupoId, CancellationToken ct = default) =>
        await _contexto.Familias.ToListAsync(ct).ConfigureAwait(false);

    public void Agregar(Familia familia) => _contexto.Familias.Add(familia);

    public void Eliminar(Familia familia) => _contexto.Familias.Remove(familia);

    public Task<bool> TieneArticulosAsync(Guid familiaId, CancellationToken ct = default) =>
        _contexto.Productos.AnyAsync(p => p.FamiliaId == familiaId, ct);

    public async Task<FamiliaDto?> ObtenerAsync(Guid familiaId, CancellationToken ct = default)
    {
        var familia = await _contexto.Familias.SingleOrDefaultAsync(f => f.Id == familiaId, ct).ConfigureAwait(false);
        if (familia is null)
        {
            return null;
        }

        var todas = await _contexto.Familias.ToListAsync(ct).ConfigureAwait(false);
        var porId = todas.ToDictionary(f => f.Id);
        var (ruta, nivel) = ArbolFamilias.RutaYNivel(familia, porId);
        return new FamiliaDto(familia.Id, familia.Nombre, familia.Codigo, familia.PadreId, familia.Activo, ruta, nivel);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class CatalogoDbContextFactory : IDesignTimeDbContextFactory<CatalogoDbContext>
{
    public CatalogoDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<CatalogoDbContext>().UseNpgsql(conexion).Options;
        return new CatalogoDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
