using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using AlxorCore.Recepcion.Aplicacion;
using AlxorCore.Recepcion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Recepcion.Infraestructura;

/// <summary>Contexto de persistencia del módulo Recepción de facturas de proveedor.</summary>
public sealed class RecepcionDbContext : DbContextEmpresaBase, IUnidadDeTrabajoRecepcion
{
    public RecepcionDbContext(DbContextOptions<RecepcionDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "recepcion";

    public DbSet<FacturaRecibida> FacturasRecibidas => Set<FacturaRecibida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecepcionDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionFacturaRecibida : IEntityTypeConfiguration<FacturaRecibida>
{
    public void Configure(EntityTypeBuilder<FacturaRecibida> builder)
    {
        builder.ToTable("factura_recibida");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(f => f.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(f => f.Origen).HasColumnName("origen").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(f => f.FechaRecepcion).HasColumnName("fecha_recepcion").IsRequired();
        builder.Property(f => f.RemitenteCorreo).HasColumnName("remitente_correo").HasMaxLength(320);
        builder.Property(f => f.AsuntoCorreo).HasColumnName("asunto_correo").HasMaxLength(500);
        builder.Property(f => f.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(FacturaRecibida.LongitudMaximaArchivo).IsRequired();
        builder.Property(f => f.TipoContenido).HasColumnName("tipo_contenido").HasMaxLength(120).IsRequired();
        builder.Property(f => f.Contenido).HasColumnName("contenido").HasColumnType("bytea").IsRequired();
        builder.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(f => f.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(f => f.ProveedorTexto).HasColumnName("proveedor_texto").HasMaxLength(200);
        builder.Property(f => f.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(60);
        builder.Property(f => f.FechaFactura).HasColumnName("fecha_factura");
        builder.Property(f => f.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(14,2)");
        builder.Property(f => f.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10);
        builder.Property(f => f.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)");
        builder.Property(f => f.GastoId).HasColumnName("gasto_id");
        builder.Property(f => f.MotivoRechazo).HasColumnName("motivo_rechazo").HasMaxLength(500);

        builder.HasIndex(f => new { f.EmpresaId, f.Estado }).HasDatabaseName("ix_factura_recibida_empresa_estado");
        builder.Ignore(f => f.EventosDominio);
    }
}

internal sealed class RepositorioFacturasRecibidas : IRepositorioFacturasRecibidas, IConsultaFacturasRecibidas
{
    private readonly RecepcionDbContext _contexto;

    public RepositorioFacturasRecibidas(RecepcionDbContext contexto) => _contexto = contexto;

    public Task<FacturaRecibida?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.FacturasRecibidas.SingleOrDefaultAsync(f => f.Id == id, ct);

    public void Agregar(FacturaRecibida factura) => _contexto.FacturasRecibidas.Add(factura);

    public async Task<FacturaRecibidaDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _contexto.FacturasRecibidas.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct).ConfigureAwait(false);
        return factura is null ? null : FacturaRecibidaDto.Desde(factura);
    }

    public async Task<IReadOnlyList<FacturaRecibidaDto>> ListarAsync(Guid empresaId, string? estado = null, CancellationToken ct = default)
    {
        var consulta = _contexto.FacturasRecibidas.AsNoTracking().Where(f => f.EmpresaId == empresaId);
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoRecepcion>(estado, ignoreCase: true, out var e))
        {
            consulta = consulta.Where(f => f.Estado == e);
        }

        var facturas = await consulta.OrderByDescending(f => f.FechaRecepcion).ToListAsync(ct).ConfigureAwait(false);
        return facturas.Select(FacturaRecibidaDto.Desde).ToList();
    }

    public async Task<ContenidoAdjunto?> ObtenerContenidoAsync(Guid id, CancellationToken ct = default)
    {
        var factura = await _contexto.FacturasRecibidas.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct).ConfigureAwait(false);
        return factura is null ? null : new ContenidoAdjunto(factura.NombreArchivo, factura.TipoContenido, factura.Contenido);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class RecepcionDbContextFactory : IDesignTimeDbContextFactory<RecepcionDbContext>
{
    public RecepcionDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<RecepcionDbContext>().UseNpgsql(conexion).Options;
        return new RecepcionDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
