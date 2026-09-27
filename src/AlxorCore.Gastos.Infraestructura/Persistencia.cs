using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Gastos.Infraestructura;

/// <summary>Contexto de persistencia del módulo Gastos.</summary>
public sealed class GastosDbContext : DbContextEmpresaBase, IUnidadDeTrabajoGastos
{
    public GastosDbContext(DbContextOptions<GastosDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "gastos";

    public DbSet<Gasto> Gastos => Set<Gasto>();

    public DbSet<MensajeSalida> MensajesSalida => Set<MensajeSalida>();

    public DbSet<DuaImportacion> DuasImportacion => Set<DuaImportacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GastosDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionGasto : IEntityTypeConfiguration<Gasto>
{
    public void Configure(EntityTypeBuilder<Gasto> builder)
    {
        builder.ToTable("gasto");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(g => g.ProveedorId).HasColumnName("proveedor_id");
        builder.Property(g => g.ProveedorTexto).HasColumnName("proveedor_texto").HasMaxLength(200);
        builder.Property(g => g.ActividadNegocioId).HasColumnName("actividad_negocio_id");
        builder.Property(g => g.Afectacion).HasColumnName("afectacion").HasMaxLength(20).HasConversion<string>().IsRequired()
            .HasDefaultValue(AfectacionIva.Comun).HasSentinel((AfectacionIva)0);
        builder.Property(g => g.Concepto).HasColumnName("concepto").HasMaxLength(Gasto.LongitudMaximaConcepto).IsRequired();
        builder.Property(g => g.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(g => g.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(g => g.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
        builder.Property(g => g.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(g => g.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(g => g.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(g => g.RetencionIrpf).HasColumnName("retencion_irpf").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(g => g.Total).HasColumnName("total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(g => g.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(g => g.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(g => g.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();

        builder.Property(g => g.NumeroFactura).HasColumnName("numero_factura").HasMaxLength(Gasto.LongitudMaximaNumeroFactura);
        builder.Property(g => g.FechaFactura).HasColumnName("fecha_factura");
        builder.Property(g => g.Revision).HasColumnName("revision").HasDefaultValue(0).IsRequired();
        builder.Property(g => g.EsRectificativa).HasColumnName("es_rectificativa").HasDefaultValue(false).IsRequired();
        builder.Property(g => g.RectificaGastoId).HasColumnName("rectifica_gasto_id");
        builder.Property(g => g.NumeroRectificado).HasColumnName("numero_rectificado").HasMaxLength(Gasto.LongitudMaximaNumeroFactura);
        builder.Property(g => g.FechaRectificada).HasColumnName("fecha_rectificada");
        builder.Property(g => g.MotivoRectificacion).HasColumnName("motivo_rectificacion").HasMaxLength(Gasto.LongitudMaximaConcepto);
        builder.HasIndex(g => g.RectificaGastoId).HasDatabaseName("ix_gasto_rectifica");
        builder.Property(g => g.RecargoTotal).HasColumnName("recargo_total").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();

        builder.OwnsMany(g => g.Lineas, l =>
        {
            l.ToTable("linea_gasto");
            l.WithOwner().HasForeignKey("gasto_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(Gasto.LongitudMaximaConcepto);
            l.Property(x => x.CuentaGasto).HasColumnName("cuenta_gasto").HasMaxLength(12);
            l.Property(x => x.Base).HasColumnName("base").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
            l.Property(x => x.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
            l.Property(x => x.Cuota).HasColumnName("cuota").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.Autoliquidada).HasColumnName("autoliquidada").IsRequired();
            l.Property(x => x.PorcentajeRecargo).HasColumnName("porcentaje_recargo").HasColumnType("numeric(5,2)").IsRequired();
            l.Property(x => x.CuotaRecargo).HasColumnName("cuota_recargo").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.PorcentajeDeducible).HasColumnName("porcentaje_deducible").HasColumnType("numeric(5,2)").IsRequired();
            l.Property(x => x.CuotaDeducible).HasColumnName("cuota_deducible").HasColumnType("numeric(14,2)").IsRequired();
            l.HasIndex("gasto_id").HasDatabaseName("ix_linea_gasto_gasto");
        });
        builder.Navigation(g => g.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(g => g.Vencimientos, v =>
        {
            v.ToTable("vencimiento_gasto");
            v.WithOwner().HasForeignKey("gasto_id");
            v.Property<Guid>("Id").HasColumnName("id").ValueGeneratedOnAdd();
            v.HasKey("Id");
            v.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            v.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            v.HasIndex("gasto_id").HasDatabaseName("ix_vencimiento_gasto_gasto");
        });
        builder.Navigation(g => g.Vencimientos).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(g => new { g.EmpresaId, g.Fecha }).HasDatabaseName("ix_gasto_empresa_fecha");
        builder.Ignore(g => g.EventosDominio);
    }
}

internal sealed class ConfiguracionDuaImportacion : IEntityTypeConfiguration<DuaImportacion>
{
    public void Configure(EntityTypeBuilder<DuaImportacion> builder)
    {
        builder.ToTable("dua_importacion");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.GastoId).HasColumnName("gasto_id").IsRequired();
        builder.Property(d => d.Mrn).HasColumnName("mrn").HasMaxLength(DuaImportacion.LongitudMrn).IsRequired();
        builder.Property(d => d.FechaAdmision).HasColumnName("fecha_admision").IsRequired();
        builder.Property(d => d.Aduana).HasColumnName("aduana").HasMaxLength(60);
        builder.Property(d => d.BaseIva).HasColumnName("base_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.Aranceles).HasColumnName("aranceles").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.HasIndex(d => new { d.EmpresaId, d.Mrn }).IsUnique().HasDatabaseName("ux_dua_importacion_empresa_mrn");
        builder.HasIndex(d => d.GastoId).HasDatabaseName("ix_dua_importacion_gasto");
        builder.Ignore(d => d.EventosDominio);
    }
}

internal sealed class RepositorioDuasImportacion : IRepositorioDuasImportacion
{
    private readonly GastosDbContext _contexto;

    public RepositorioDuasImportacion(GastosDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<DuaImportacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.DuasImportacion.AsNoTracking().Where(d => d.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<DuaImportacion?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.DuasImportacion.SingleOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExisteMrnAsync(Guid empresaId, string mrn, Guid? salvo, CancellationToken ct = default) =>
        _contexto.DuasImportacion.AnyAsync(d => d.EmpresaId == empresaId && d.Mrn == mrn && d.Id != salvo, ct);

    public void Agregar(DuaImportacion dua) => _contexto.DuasImportacion.Add(dua);

    public void Eliminar(DuaImportacion dua) => _contexto.DuasImportacion.Remove(dua);
}

internal sealed class RepositorioGastos : IRepositorioGastos, IConsultaGastos
{
    private readonly GastosDbContext _contexto;

    public RepositorioGastos(GastosDbContext contexto) => _contexto = contexto;

    public Task<Gasto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Gastos.SingleOrDefaultAsync(g => g.Id == id, ct);

    public void Agregar(Gasto gasto) => _contexto.Gastos.Add(gasto);

    public Task<bool> ExisteFacturaAsync(Guid empresaId, Guid proveedorId, string numero, int anio, Guid? excluirId, CancellationToken ct = default)
    {
        // Mismo número sin distinguir mayúsculas (ILIKE sin comodines: se escapan).
        var patron = numero.Trim().Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);
        return _contexto.Gastos.AnyAsync(g => g.EmpresaId == empresaId && g.ProveedorId == proveedorId && g.Estado == EstadoGasto.Registrado
            && g.NumeroFactura != null && EF.Functions.ILike(g.NumeroFactura, patron) && (g.FechaFactura ?? g.Fecha).Year == anio && g.Id != excluirId, ct);
    }

    public async Task<GastoDto?> ObtenerAsync(Guid gastoId, CancellationToken ct = default)
    {
        var gasto = await _contexto.Gastos.SingleOrDefaultAsync(g => g.Id == gastoId, ct).ConfigureAwait(false);
        return gasto is null ? null : GastoDto.Desde(gasto);
    }

    public async Task<IReadOnlyList<GastoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var gastos = await _contexto.Gastos
            .Where(g => g.EmpresaId == empresaId)
            .OrderByDescending(g => g.Fecha)
            .ToListAsync(ct).ConfigureAwait(false);
        return gastos.Select(GastoDto.Desde).ToList();
    }

    public async Task<PaginaResultado<GastoDto>> BuscarAsync(Guid empresaId, FiltroGastos filtro, Paginacion paginacion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        ArgumentNullException.ThrowIfNull(paginacion);

        var consulta = _contexto.Gastos.Where(g => g.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var patron = $"%{filtro.Texto.Trim()}%";
            consulta = consulta.Where(g =>
                EF.Functions.ILike(g.Concepto, patron) ||
                (g.ProveedorTexto != null && EF.Functions.ILike(g.ProveedorTexto, patron)) ||
                (g.NumeroFactura != null && EF.Functions.ILike(g.NumeroFactura, patron)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoGasto>(filtro.Estado, ignoreCase: true, out var estado))
        {
            consulta = consulta.Where(g => g.Estado == estado);
        }

        if (filtro.Desde is DateOnly desde)
        {
            consulta = consulta.Where(g => g.Fecha >= desde);
        }

        if (filtro.Hasta is DateOnly hasta)
        {
            consulta = consulta.Where(g => g.Fecha <= hasta);
        }

        if (filtro.ImporteMin is decimal min)
        {
            consulta = consulta.Where(g => g.Total >= min);
        }

        if (filtro.ImporteMax is decimal max)
        {
            consulta = consulta.Where(g => g.Total <= max);
        }

        if (filtro.ProveedorId is Guid proveedorId)
        {
            consulta = consulta.Where(g => g.ProveedorId == proveedorId);
        }

        var total = await consulta.CountAsync(ct).ConfigureAwait(false);
        var gastos = await consulta
            .OrderByDescending(g => g.Fecha)
            .Skip(paginacion.Saltar).Take(paginacion.TamanoPagina)
            .ToListAsync(ct).ConfigureAwait(false);
        return PaginaResultado<GastoDto>.Crear(gastos.Select(GastoDto.Desde).ToList(), total, paginacion);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class GastosDbContextFactory : IDesignTimeDbContextFactory<GastosDbContext>
{
    public GastosDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<GastosDbContext>().UseNpgsql(conexion).Options;
        return new GastosDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
