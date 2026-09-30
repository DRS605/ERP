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

    public DbSet<ReglaContabilizacion> ReglasContabilizacion => Set<ReglaContabilizacion>();

    public DbSet<PlantillaAsiento> PlantillasAsiento => Set<PlantillaAsiento>();

    public DbSet<Inmovilizado> Inmovilizados => Set<Inmovilizado>();

    public DbSet<CentroAnalitico> CentrosAnaliticos => Set<CentroAnalitico>();

    public DbSet<PartidaAnalitica> PartidasAnaliticas => Set<PartidaAnalitica>();

    public DbSet<ClaveReparto> ClavesReparto => Set<ClaveReparto>();

    public DbSet<ReglaAnalitica> ReglasAnaliticas => Set<ReglaAnalitica>();

    public DbSet<PeriodoAnalitico> PeriodosAnaliticos => Set<PeriodoAnalitico>();

    public DbSet<ImputacionAnalitica> ImputacionesAnaliticas => Set<ImputacionAnalitica>();

    public DbSet<EjecucionAnalitica> EjecucionesAnaliticas => Set<EjecucionAnalitica>();

    public DbSet<PresupuestoContable> PresupuestosContables => Set<PresupuestoContable>();

    public DbSet<DiarioContable> Diarios => Set<DiarioContable>();

    public DbSet<Periodificacion> Periodificaciones => Set<Periodificacion>();

    public DbSet<CuentaExistencias> CuentasExistencias => Set<CuentaExistencias>();

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
        builder.Property(c => c.TerceroId).HasColumnName("tercero_id");
        builder.Property(c => c.TipoTercero).HasColumnName("tipo_tercero").HasMaxLength(20);
        builder.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique().HasDatabaseName("ux_cuenta_empresa_codigo");
        builder.HasIndex(c => new { c.EmpresaId, c.TerceroId }).HasDatabaseName("ix_cuenta_empresa_tercero");
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
        builder.Property(a => a.AnulaAsientoId).HasColumnName("anula_asiento_id");
        builder.HasIndex(a => a.AnulaAsientoId).IsUnique().HasFilter("anula_asiento_id IS NOT NULL").HasDatabaseName("ux_asiento_anula");
        // Diario: lo asigna el trigger contabilidad.asiento_diario al insertar. El número dentro del diario no se guarda:
        // es el orden del asiento (por su número correlativo) en su diario y ejercicio, estable porque los asientos no
        // se borran y la numeración solo crece.
        builder.Property(a => a.Diario).HasColumnName("diario").HasMaxLength(DiarioContable.LongitudCodigo).IsRequired().ValueGeneratedOnAdd();
        builder.HasIndex(a => new { a.EmpresaId, a.Ejercicio, a.Diario, a.Numero }).HasDatabaseName("ix_asiento_diario");

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
        builder.Property(c => c.LongitudSubcuenta).HasColumnName("longitud_subcuenta").IsRequired().HasDefaultValue(ConfiguracionContabilidad.LongitudSubcuentaDefecto);
        builder.Property(c => c.CerradoHasta).HasColumnName("cerrado_hasta");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionDiarioContable : IEntityTypeConfiguration<DiarioContable>
{
    public void Configure(EntityTypeBuilder<DiarioContable> builder)
    {
        builder.ToTable("diario_contable");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.Codigo).HasColumnName("codigo").HasMaxLength(DiarioContable.LongitudCodigo).IsRequired();
        builder.Property(d => d.Nombre).HasColumnName("nombre").HasMaxLength(DiarioContable.LongitudNombre).IsRequired();
        builder.Property<List<string>>("_origenes").HasColumnName("origenes").HasColumnType("text[]").IsRequired();
        builder.Ignore(d => d.Origenes);
        builder.Property(d => d.Activo).HasColumnName("activo").IsRequired();
        builder.HasIndex(d => new { d.EmpresaId, d.Codigo }).IsUnique().HasDatabaseName("ux_diario_empresa_codigo");
        builder.Ignore(d => d.EventosDominio);
    }
}

internal sealed class ConfiguracionPeriodificacion : IEntityTypeConfiguration<Periodificacion>
{
    public void Configure(EntityTypeBuilder<Periodificacion> builder)
    {
        builder.ToTable("periodificacion");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.Descripcion).HasColumnName("descripcion").HasMaxLength(Periodificacion.LongitudDescripcion).IsRequired();
        builder.Property(p => p.Tipo).HasColumnName("tipo").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(p => p.CuentaResultado).HasColumnName("cuenta_resultado").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(p => p.CuentaPeriodificacion).HasColumnName("cuenta_periodificacion").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(p => p.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(p => p.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(p => p.EjercicioInicio).HasColumnName("ejercicio_inicio").IsRequired();
        builder.Property(p => p.MesInicio).HasColumnName("mes_inicio").IsRequired();
        builder.Property(p => p.Meses).HasColumnName("meses").IsRequired();
        builder.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(12).HasConversion<string>().IsRequired();
        builder.Property(p => p.AsientoReclasificacionId).HasColumnName("asiento_reclasificacion_id");
        builder.Property(p => p.AsientoCancelacionId).HasColumnName("asiento_cancelacion_id");
        builder.Ignore(p => p.Imputado);
        builder.Ignore(p => p.Pendiente);
        builder.Ignore(p => p.TieneAsientos);
        builder.OwnsMany(p => p.Cuotas, c =>
        {
            c.ToTable("cuota_periodificacion");
            c.WithOwner().HasForeignKey("periodificacion_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
            c.Property(x => x.Mes).HasColumnName("mes").IsRequired();
            c.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            c.Property(x => x.AsientoId).HasColumnName("asiento_id");
            c.Ignore(x => x.Fecha);
            c.HasIndex("periodificacion_id", nameof(CuotaPeriodificacion.Ejercicio), nameof(CuotaPeriodificacion.Mes)).IsUnique().HasDatabaseName("ux_cuota_periodificacion_mes");
            c.HasIndex(x => x.AsientoId).HasDatabaseName("ix_cuota_periodificacion_asiento");
        });
        builder.HasIndex(p => new { p.EmpresaId, p.Estado }).HasDatabaseName("ix_periodificacion_empresa_estado");
        builder.HasIndex(p => p.AsientoReclasificacionId).HasDatabaseName("ix_periodificacion_asiento_reclasificacion");
        builder.HasIndex(p => p.AsientoCancelacionId).HasDatabaseName("ix_periodificacion_asiento_cancelacion");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class ConfiguracionCuentaExistencias : IEntityTypeConfiguration<CuentaExistencias>
{
    public void Configure(EntityTypeBuilder<CuentaExistencias> builder)
    {
        builder.ToTable("cuenta_existencias");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Familia).HasColumnName("familia").HasMaxLength(CuentaExistencias.LongitudFamilia);
        builder.Property(c => c.CuentaStock).HasColumnName("cuenta_stock").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(c => c.CuentaVariacion).HasColumnName("cuenta_variacion").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.HasIndex(c => c.EmpresaId).HasDatabaseName("ix_cuenta_existencias_empresa");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class RepositorioCuentasExistencias : IRepositorioCuentasExistencias
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioCuentasExistencias(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<CuentaExistencias>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.CuentasExistencias.Where(c => c.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public void Agregar(CuentaExistencias cuenta) => _contexto.CuentasExistencias.Add(cuenta);

    public void Eliminar(CuentaExistencias cuenta) => _contexto.CuentasExistencias.Remove(cuenta);
}

internal sealed class RepositorioPeriodificaciones : IRepositorioPeriodificaciones
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioPeriodificaciones(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<Periodificacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Periodificaciones.Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Periodificacion?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.Periodificaciones.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(Periodificacion periodificacion) => _contexto.Periodificaciones.Add(periodificacion);

    public void Eliminar(Periodificacion periodificacion) => _contexto.Periodificaciones.Remove(periodificacion);
}

internal sealed class RepositorioDiarios : IRepositorioDiarios
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioDiarios(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<DiarioContable>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Diarios.Where(d => d.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<DiarioContable?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.Diarios.SingleOrDefaultAsync(d => d.Id == id, ct);

    public void Agregar(DiarioContable diario) => _contexto.Diarios.Add(diario);

    public void Eliminar(DiarioContable diario) => _contexto.Diarios.Remove(diario);

    public async Task<IReadOnlyDictionary<string, int>> AsientosPorDiarioAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Asientos.AsNoTracking().Where(a => a.EmpresaId == empresaId && a.Diario != null).GroupBy(a => a.Diario!)
            .Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, StringComparer.Ordinal, ct).ConfigureAwait(false);
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
        builder.Property(d => d.CuentaTesoreria).HasColumnName("cuenta_tesoreria").HasMaxLength(Cuenta.LongitudMaximaCodigo);
        builder.Property(d => d.CuentaTercero).HasColumnName("cuenta_tercero").HasMaxLength(Cuenta.LongitudMaximaCodigo);
        builder.Property(d => d.Lineas).HasColumnName("lineas").HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb").IsRequired()
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                s => System.Text.Json.JsonSerializer.Deserialize<List<AlxorCore.Nucleo.Aplicacion.LineaContable>>(s, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<AlxorCore.Nucleo.Aplicacion.LineaContable>(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<AlxorCore.Nucleo.Aplicacion.LineaContable>>(
                    (a, b) => System.Text.Json.JsonSerializer.Serialize(a, (System.Text.Json.JsonSerializerOptions?)null) == System.Text.Json.JsonSerializer.Serialize(b, (System.Text.Json.JsonSerializerOptions?)null),
                    v => v.Count,
                    v => v.ToList()));
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
        builder.Property(d => d.Afectacion).HasColumnName("afectacion").HasMaxLength(20);
        builder.Property(d => d.ActividadNegocioId).HasColumnName("actividad_negocio_id");
        builder.Property(d => d.Anulacion).HasColumnName("anulacion").HasDefaultValue(false).IsRequired();
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

    public Task<bool> ExistePorOrigenAsync(Guid empresaId, string origenTipo, Guid origenId, CancellationToken ct = default) =>
        _contexto.DocumentosPendientes.AnyAsync(d => d.EmpresaId == empresaId && d.OrigenTipo == origenTipo && d.OrigenId == origenId, ct);

    public async Task<IReadOnlyList<DocumentoPendiente>> ListarPendientesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.DocumentosPendientes.AsNoTracking()
            .Where(d => d.EmpresaId == empresaId && d.Estado == Dominio.EstadoContabilizacion.Pendiente)
            .OrderBy(d => d.FechaDocumento).ThenBy(d => d.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DocumentoPendiente>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.DocumentosPendientes.AsNoTracking()
            .Where(d => d.EmpresaId == empresaId)
            .OrderByDescending(d => d.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class ConfiguracionReglaContabilizacion : IEntityTypeConfiguration<ReglaContabilizacion>
{
    public void Configure(EntityTypeBuilder<ReglaContabilizacion> builder)
    {
        builder.ToTable("regla_contabilizacion");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(r => r.Sentido).HasColumnName("sentido").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(r => r.Familia).HasColumnName("familia").HasMaxLength(ReglaContabilizacion.LongitudMaximaClave);
        builder.Property(r => r.TipoTercero).HasColumnName("tipo_tercero").HasMaxLength(ReglaContabilizacion.LongitudMaximaClave);
        builder.Property(r => r.CuentaCodigo).HasColumnName("cuenta_codigo").HasMaxLength(ReglaContabilizacion.LongitudMaximaCuenta).IsRequired();
        builder.HasIndex(r => new { r.EmpresaId, r.Sentido }).HasDatabaseName("ix_regla_contabilizacion_empresa_sentido");
        builder.Ignore(r => r.EventosDominio);
    }
}

internal sealed class ConfiguracionPlantillaAsiento : IEntityTypeConfiguration<PlantillaAsiento>
{
    private static readonly System.Text.Json.JsonSerializerOptions Json = new(System.Text.Json.JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public void Configure(EntityTypeBuilder<PlantillaAsiento> builder)
    {
        builder.ToTable("plantilla_asiento");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.Sentido).HasColumnName("sentido").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(p => p.OrigenTipo).HasColumnName("origen_tipo").HasMaxLength(PlantillaAsiento.LongitudOrigen);
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Concepto).HasColumnName("concepto").HasMaxLength(PlantillaAsiento.LongitudConcepto);
        builder.Property(p => p.Diario).HasColumnName("diario").HasMaxLength(10);
        builder.Property(p => p.Activa).HasColumnName("activa").IsRequired();
        builder.Property(p => p.Lineas).HasColumnName("lineas").HasColumnType("jsonb").IsRequired()
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, Json),
                v => System.Text.Json.JsonSerializer.Deserialize<List<LineaPlantillaAsiento>>(v, Json) ?? new List<LineaPlantillaAsiento>(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<LineaPlantillaAsiento>>(
                    (a, b) => a!.SequenceEqual(b!), v => v.Aggregate(0, (h, l) => HashCode.Combine(h, l.GetHashCode())), v => v.ToList()));
        builder.HasIndex(p => new { p.EmpresaId, p.Sentido, p.OrigenTipo }).IsUnique().AreNullsDistinct(false).HasDatabaseName("ux_plantilla_asiento_sentido_origen");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class RepositorioPlantillasAsiento : IRepositorioPlantillasAsiento
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioPlantillasAsiento(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(PlantillaAsiento plantilla) => _contexto.PlantillasAsiento.Add(plantilla);

    public void Eliminar(PlantillaAsiento plantilla) => _contexto.PlantillasAsiento.Remove(plantilla);

    public Task<PlantillaAsiento?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.PlantillasAsiento.SingleOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<PlantillaAsiento>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.PlantillasAsiento.AsNoTracking().Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioReglasContabilizacion : IRepositorioReglasContabilizacion
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioReglasContabilizacion(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(ReglaContabilizacion regla) => _contexto.ReglasContabilizacion.Add(regla);

    public Task<ReglaContabilizacion?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.ReglasContabilizacion.SingleOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ReglaContabilizacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.ReglasContabilizacion.AsNoTracking()
            .Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public void Eliminar(ReglaContabilizacion regla) => _contexto.ReglasContabilizacion.Remove(regla);
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
        var conjunto = codigos.ToHashSet(StringComparer.Ordinal);

        // Incluye también las cuentas ya añadidas pero aún NO guardadas en esta unidad de trabajo, para
        // no volver a sembrar el plan (ni recrear una subcuenta) al procesar varios documentos en un
        // mismo lote antes de guardar (evita duplicar el código y violar el índice único).
        foreach (var e in _contexto.ChangeTracker.Entries<Cuenta>().Where(e => e.State == EntityState.Added && e.Entity.EmpresaId == empresaId))
        {
            conjunto.Add(e.Entity.Codigo);
        }

        return conjunto;
    }

    public Task<Cuenta?> ObtenerPorTerceroAsync(Guid empresaId, Guid terceroId, CancellationToken ct = default) =>
        _contexto.Cuentas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.TerceroId == terceroId, ct);

    public Task<Cuenta?> ObtenerPorCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default) =>
        _contexto.Cuentas.FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Codigo == codigo, ct);

    public async Task<IReadOnlyDictionary<Guid, string>> SubcuentasPorTipoAsync(Guid empresaId, string tipoTercero, CancellationToken ct = default)
    {
        var filas = await _contexto.Cuentas.AsNoTracking()
            .Where(c => c.EmpresaId == empresaId && c.TipoTercero == tipoTercero && c.TerceroId != null)
            .Select(c => new { c.TerceroId, c.Codigo }).ToListAsync(ct).ConfigureAwait(false);
        return filas.ToDictionary(f => f.TerceroId!.Value, f => f.Codigo);
    }
}

internal sealed class RepositorioAsientos : IRepositorioAsientos
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioAsientos(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(Asiento asiento) => _contexto.Asientos.Add(asiento);

    public async Task<DateOnly?> CerradoHastaAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Configuraciones.AsNoTracking().Where(c => c.EmpresaId == empresaId).Select(c => c.CerradoHasta).FirstOrDefaultAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        // Bloqueo por empresa y ejercicio hasta guardar: dos contabilizaciones simultáneas no pueden
        // calcular el mismo número (la base de datos exige además que no haya huecos).
        await _contexto.BloquearAsync($"alxor.contabilidad:{empresaId:D}:{ejercicio}", ct).ConfigureAwait(false);

        var maxBd = await _contexto.Asientos.Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio)
            .Select(a => (int?)a.Numero).MaxAsync(ct).ConfigureAwait(false) ?? 0;

        // También cuenta los asientos ya añadidos pero aún NO guardados en esta unidad de trabajo. Así,
        // al crear varios asientos en un mismo lote —contabilizar varios pendientes de una vez, el cierre
        // de ejercicio, la amortización— cada uno recibe un número distinto sin necesidad de guardar entre
        // medias (antes colisionaban en el índice único). La colisión entre peticiones concurrentes sigue
        // respaldada por el índice único «ux_asiento_empresa_ejercicio_numero» (imposible duplicar).
        var maxLocal = _contexto.ChangeTracker.Entries<Asiento>()
            .Where(e => e.State == EntityState.Added && e.Entity.EmpresaId == empresaId && e.Entity.Ejercicio == ejercicio)
            .Select(e => e.Entity.Numero)
            .DefaultIfEmpty(0)
            .Max();

        return Math.Max(maxBd, maxLocal) + 1;
    }

    public async Task<IReadOnlyList<AsientoDto>> DiarioAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var asientos = await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio)
            .OrderBy(a => a.Fecha).ThenBy(a => a.Numero).ToListAsync(ct).ConfigureAwait(false);
        var ids = asientos.Select(a => a.Id).ToList();
        var anulados = await _contexto.Asientos.AsNoTracking()
            .Where(a => a.AnulaAsientoId != null && ids.Contains(a.AnulaAsientoId.Value))
            .Select(a => new { Anulado = a.AnulaAsientoId!.Value, Por = a.Id })
            .ToDictionaryAsync(x => x.Anulado, x => x.Por, ct).ConfigureAwait(false);
        // Número dentro del diario: el orden del asiento (por su número correlativo) en su diario.
        var enDiario = asientos.GroupBy(a => a.Diario ?? string.Empty, StringComparer.Ordinal)
            .SelectMany(g => g.OrderBy(a => a.Numero).Select((a, i) => (a.Id, N: i + 1))).ToDictionary(x => x.Id, x => x.N);
        return asientos.Select(a => AsientoDto.Desde(a, anulados.TryGetValue(a.Id, out var por) ? por : null) with { NumeroDiario = enDiario[a.Id] }).ToList();
    }

    public Task<Asiento?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Asientos.SingleOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> EstaAnuladoAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Asientos.AnyAsync(a => a.AnulaAsientoId == id, ct);

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

    public async Task<IReadOnlyList<SaldoCuentaAgregado>> SaldosHastaAsync(Guid empresaId, IReadOnlyCollection<string> codigos, DateOnly hasta, CancellationToken ct = default)
    {
        if (codigos.Count == 0)
        {
            return [];
        }

        var lista = codigos.ToList();
        return await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Fecha <= hasta)
            .SelectMany(a => a.Apuntes)
            .Where(p => lista.Contains(p.CuentaCodigo))
            .GroupBy(p => p.CuentaCodigo)
            .Select(g => new SaldoCuentaAgregado(g.Key, g.Sum(x => x.Debe), g.Sum(x => x.Haber)))
            .ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SaldoCuentaAgregado>> SaldosAgregadosAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        await _contexto.Asientos.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio)
            .SelectMany(a => a.Apuntes)
            .GroupBy(p => p.CuentaCodigo)
            .Select(g => new SaldoCuentaAgregado(g.Key, g.Sum(x => x.Debe), g.Sum(x => x.Haber)))
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ApunteOrigen>> ApuntesDeOrigenesAsync(Guid empresaId, IReadOnlyCollection<Guid> origenIds, CancellationToken ct = default)
    {
        if (origenIds.Count == 0)
        {
            return [];
        }

        var documentos = await _contexto.DocumentosPendientes.AsNoTracking()
            .Where(d => d.EmpresaId == empresaId && origenIds.Contains(d.OrigenId) && d.AsientoId != null)
            .Select(d => new { d.OrigenId, AsientoId = d.AsientoId!.Value }).ToListAsync(ct).ConfigureAwait(false);
        var ids = documentos.Select(d => d.AsientoId).Distinct().ToList();
        var asientos = await _contexto.Asientos.AsNoTracking().Where(a => ids.Contains(a.Id)).ToListAsync(ct).ConfigureAwait(false);
        var porId = asientos.ToDictionary(a => a.Id);
        return documentos.Where(d => porId.ContainsKey(d.AsientoId))
            .SelectMany(d => porId[d.AsientoId].Apuntes.Select(p => new ApunteOrigen(d.OrigenId, p.CuentaCodigo, p.Debe, p.Haber))).ToList();
    }

    public Task<bool> TieneCierreAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        _contexto.Asientos.AnyAsync(a => a.EmpresaId == empresaId && a.Ejercicio == ejercicio && a.Origen == "Cierre", ct);
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
