using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>Contexto de persistencia del módulo Contabilidad.</summary>
public sealed class ContabilidadDbContext : DbContextEmpresaBase, IUnidadDeTrabajoContabilidad
{
    public ContabilidadDbContext(DbContextOptions<ContabilidadDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "contabilidad";

    public DbSet<Cuenta> Cuentas => Set<Cuenta>();

    public DbSet<Asiento> Asientos => Set<Asiento>();

    public DbSet<ConfiguracionContabilidad> Configuraciones => Set<ConfiguracionContabilidad>();

    public DbSet<DocumentoPendiente> DocumentosPendientes => Set<DocumentoPendiente>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ContabilidadDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionCuenta : IEntityTypeConfiguration<Cuenta>
{
    public void Configure(EntityTypeBuilder<Cuenta> builder)
    {
        builder.ToTable("cuenta");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Codigo).HasColumnName("codigo").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(Cuenta.LongitudMaximaNombre).IsRequired();
        builder.Property(c => c.Grupo).HasColumnName("grupo").IsRequired();
        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique().HasDatabaseName("ux_cuenta_empresa_codigo");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionAsiento : IEntityTypeConfiguration<Asiento>
{
    public void Configure(EntityTypeBuilder<Asiento> builder)
    {
        builder.ToTable("asiento");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(a => a.Numero).HasColumnName("numero").IsRequired();
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.Concepto).HasColumnName("concepto").HasMaxLength(Asiento.LongitudMaximaConcepto).IsRequired();
        builder.Property(a => a.Origen).HasColumnName("origen").HasMaxLength(20).IsRequired();
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.OwnsMany(a => a.Apuntes, apunte =>
        {
            apunte.ToTable("apunte");
            apunte.WithOwner().HasForeignKey("asiento_id");
            apunte.HasKey(p => p.Id);
            apunte.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
            apunte.Property(p => p.CuentaCodigo).HasColumnName("cuenta_codigo").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
            apunte.Property(p => p.Concepto).HasColumnName("concepto").HasMaxLength(Asiento.LongitudMaximaConcepto);
            apunte.Property(p => p.Debe).HasColumnName("debe").HasColumnType("numeric(14,2)").IsRequired();
            apunte.Property(p => p.Haber).HasColumnName("haber").HasColumnType("numeric(14,2)").IsRequired();
        });

        builder.HasIndex(a => new { a.EmpresaId, a.Ejercicio, a.Numero }).IsUnique().HasDatabaseName("ux_asiento_empresa_ejercicio_numero");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class ConfiguracionConfigContabilidad : IEntityTypeConfiguration<ConfiguracionContabilidad>
{
    public void Configure(EntityTypeBuilder<ConfiguracionContabilidad> builder)
    {
        builder.ToTable("config_contabilidad");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Modo).HasColumnName("modo").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(c => c.ContabilizacionAutomatica).HasColumnName("contabilizacion_automatica").IsRequired();
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionDocumentoPendiente : IEntityTypeConfiguration<DocumentoPendiente>
{
    public void Configure(EntityTypeBuilder<DocumentoPendiente> builder)
    {
        builder.ToTable("documento_pendiente");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.Sentido).HasColumnName("sentido").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(d => d.OrigenTipo).HasColumnName("origen_tipo").HasMaxLength(40).IsRequired();
        builder.Property(d => d.OrigenId).HasColumnName("origen_id").IsRequired();
        builder.Property(d => d.Referencia).HasColumnName("referencia").HasMaxLength(80).IsRequired();
        builder.Property(d => d.TerceroId).HasColumnName("tercero_id");
        builder.Property(d => d.TerceroNombre).HasColumnName("tercero_nombre").HasMaxLength(200).IsRequired();
        builder.Property(d => d.FechaDocumento).HasColumnName("fecha_documento").IsRequired();
        builder.Property(d => d.FechaRegistro).HasColumnName("fecha_registro").IsRequired();
        builder.Property(d => d.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
        builder.Property(d => d.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(d => d.RetencionIrpf).HasColumnName("retencion_irpf").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.Total).HasColumnName("total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.ProductoId).HasColumnName("producto_id");
        builder.Property(d => d.Familia).HasColumnName("familia").HasMaxLength(80);
        builder.Property(d => d.TipoTercero).HasColumnName("tipo_tercero").HasMaxLength(80);
        builder.Property(d => d.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(d => d.AsientoId).HasColumnName("asiento_id");
        builder.Property(d => d.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasIndex(d => new { d.EmpresaId, d.Estado }).HasDatabaseName("ix_documento_pendiente_empresa_estado");
        builder.Ignore(d => d.EventosDominio);
    }
}

internal sealed class RepositorioDocumentosPendientes : IRepositorioDocumentosPendientes
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioDocumentosPendientes(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(DocumentoPendiente documento) => _contexto.DocumentosPendientes.Add(documento);

    public Task<DocumentoPendiente?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.DocumentosPendientes.SingleOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DocumentoPendiente>> ListarPendientesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.DocumentosPendientes.AsNoTracking()
            .Where(d => d.EmpresaId == empresaId && d.Estado == Dominio.EstadoContabilizacion.Pendiente)
            .OrderBy(d => d.FechaDocumento).ThenBy(d => d.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DocumentoPendiente>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.DocumentosPendientes.AsNoTracking()
            .Where(d => d.EmpresaId == empresaId)
            .OrderByDescending(d => d.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioCuentas : IRepositorioCuentas
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioCuentas(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<CuentaDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var cuentas = await _contexto.Cuentas.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId).OrderBy(c => c.Codigo).ToListAsync(ct).ConfigureAwait(false);
        return cuentas.Select(CuentaDto.Desde).ToList();
    }

    public void Agregar(Cuenta cuenta) => _contexto.Cuentas.Add(cuenta);

    public async Task<IReadOnlySet<string>> CodigosExistentesAsync(Guid empresaId, CancellationToken ct = default)
    {
        var codigos = await _contexto.Cuentas.Where(c => c.EmpresaId == empresaId).Select(c => c.Codigo).ToListAsync(ct).ConfigureAwait(false);
        return codigos.ToHashSet(StringComparer.Ordinal);
    }
}

internal sealed class RepositorioAsientos : IRepositorioAsientos
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioAsientos(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(Asiento asiento) => _contexto.Asientos.Add(asiento);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var max = await _contexto.Asientos.Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio)
            .Select(a => (int?)a.Numero).MaxAsync(ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }

    public async Task<IReadOnlyList<AsientoDto>> DiarioAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var asientos = await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio)
            .OrderBy(a => a.Fecha).ThenBy(a => a.Numero).ToListAsync(ct).ConfigureAwait(false);
        return asientos.Select(AsientoDto.Desde).ToList();
    }

    public async Task<IReadOnlyList<AsientoDto>> AsientosDeCuentaAsync(Guid empresaId, int ejercicio, string cuentaCodigo, CancellationToken ct = default)
    {
        var asientos = await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio && a.Apuntes.Any(p => p.CuentaCodigo == cuentaCodigo))
            .OrderBy(a => a.Fecha).ThenBy(a => a.Numero).ToListAsync(ct).ConfigureAwait(false);
        return asientos.Select(AsientoDto.Desde).ToList();
    }

    public async Task<IReadOnlyList<AsientoDto>> TodosAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var asientos = await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio).ToListAsync(ct).ConfigureAwait(false);
        return asientos.Select(AsientoDto.Desde).ToList();
    }
}

internal sealed class RepositorioConfigContabilidad : IRepositorioConfigContabilidad
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioConfigContabilidad(ContabilidadDbContext contexto) => _contexto = contexto;

    public Task<ConfiguracionContabilidad?> ObtenerAsync(Guid empresaId, CancellationToken ct = default) =>
        _contexto.Configuraciones.SingleOrDefaultAsync(c => c.Id == empresaId, ct);

    public void Agregar(ConfiguracionContabilidad config) => _contexto.Configuraciones.Add(config);
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class ContabilidadDbContextFactory : IDesignTimeDbContextFactory<ContabilidadDbContext>
{
    public ContabilidadDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ContabilidadDbContext>().UseNpgsql(conexion).Options;
        return new ContabilidadDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
