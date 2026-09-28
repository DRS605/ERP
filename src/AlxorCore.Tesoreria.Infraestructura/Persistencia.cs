using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Tesoreria.Infraestructura;

/// <summary>Contexto de persistencia del módulo Tesorería.</summary>
public sealed class TesoreriaDbContext : DbContextEmpresaBase, IUnidadDeTrabajoTesoreria
{
    public TesoreriaDbContext(DbContextOptions<TesoreriaDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "tesoreria";

    public DbSet<Movimiento> Movimientos => Set<Movimiento>();

    public DbSet<PrevisionTesoreria> Previsiones => Set<PrevisionTesoreria>();

    public DbSet<Anticipo> Anticipos => Set<Anticipo>();

    public DbSet<EfectoCartera> Cartera => Set<EfectoCartera>();

    public DbSet<AnulacionEfecto> AnulacionesCartera => Set<AnulacionEfecto>();

    public DbSet<MensajeSalida> MensajesSalida => Set<MensajeSalida>();

    public DbSet<Reclamacion> Reclamaciones => Set<Reclamacion>();

    public DbSet<ConfiguracionReclamaciones> ConfiguracionesReclamacion => Set<ConfiguracionReclamaciones>();

    public DbSet<CuentaBancaria> CuentasBancarias => Set<CuentaBancaria>();

    public DbSet<Remesa> Remesas => Set<Remesa>();

    public DbSet<DevolucionRecibo> Devoluciones => Set<DevolucionRecibo>();

    public DbSet<ExtractoImportado> Extractos => Set<ExtractoImportado>();

    public DbSet<ApunteBancario> Apuntes => Set<ApunteBancario>();

    public DbSet<EntregaCuentaProveedor> EntregasCuenta => Set<EntregaCuentaProveedor>();

    public DbSet<LiquidacionPagos> LiquidacionesPagos => Set<LiquidacionPagos>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TesoreriaDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionMovimiento : IEntityTypeConfiguration<Movimiento>
{
    public void Configure(EntityTypeBuilder<Movimiento> builder)
    {
        builder.ToTable("movimiento");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(m => m.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(m => m.DocumentoId).HasColumnName("documento_id").IsRequired();
        builder.Property(m => m.Sentido).HasColumnName("sentido").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(m => m.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(m => m.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(m => m.Metodo).HasColumnName("metodo").HasMaxLength(40);
        builder.Property(m => m.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.Property(m => m.AnulaMovimientoId).HasColumnName("anula_movimiento_id");
        builder.Property(m => m.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
        builder.Property(m => m.CuentaPuente).HasColumnName("cuenta_puente").HasMaxLength(Movimiento.LongitudCuentaPuente);
        builder.HasOne<CuentaBancaria>().WithMany().HasForeignKey(m => m.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(m => m.CuentaBancariaId).HasDatabaseName("ix_movimiento_cuenta_bancaria");

        builder.HasIndex(m => new { m.EmpresaId, m.TipoDocumento, m.DocumentoId }).HasDatabaseName("ix_movimiento_documento");
        builder.HasIndex(m => m.AnulaMovimientoId).IsUnique().HasFilter("anula_movimiento_id IS NOT NULL").HasDatabaseName("ux_movimiento_anula");
        builder.Ignore(m => m.EventosDominio);
    }
}

internal sealed class ConfiguracionEfectoCartera : IEntityTypeConfiguration<EfectoCartera>
{
    public void Configure(EntityTypeBuilder<EfectoCartera> builder)
    {
        builder.ToTable("efecto_cartera");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Sentido).HasColumnName("sentido").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(e => e.TerceroId).HasColumnName("tercero_id");
        builder.Property(e => e.TerceroNombre).HasColumnName("tercero_nombre").HasMaxLength(EfectoCartera.LongitudTexto).IsRequired();
        builder.Property(e => e.Documento).HasColumnName("documento").HasMaxLength(EfectoCartera.LongitudTexto).IsRequired();
        builder.Property(e => e.FechaDocumento).HasColumnName("fecha_documento");
        builder.Property(e => e.Vencimiento).HasColumnName("vencimiento").IsRequired();
        builder.Property(e => e.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(e => e.Origen).HasColumnName("origen").HasMaxLength(40);
        builder.Property(e => e.OrigenReferencia).HasColumnName("origen_referencia").HasMaxLength(80);
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasIndex(e => new { e.EmpresaId, e.Sentido, e.Vencimiento }).HasDatabaseName("ix_efecto_cartera_vencimiento");
        builder.HasIndex(e => new { e.EmpresaId, e.Origen, e.OrigenReferencia }).IsUnique().HasDatabaseName("ux_efecto_cartera_origen");
        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class RepositorioCartera : IRepositorioCartera
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioCartera(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(EfectoCartera efecto) => _ctx.Cartera.Add(efecto);

    public Task<EfectoCartera?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Cartera.SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<EfectoCartera>> ListarAsync(Guid empresaId, SentidoCartera? sentido, CancellationToken ct = default) =>
        await _ctx.Cartera.AsNoTracking().Where(e => e.EmpresaId == empresaId && (sentido == null || e.Sentido == sentido)).ToListAsync(ct).ConfigureAwait(false);

    public Task<EfectoCartera?> PorOrigenAsync(Guid empresaId, string origen, string origenReferencia, CancellationToken ct = default) =>
        _ctx.Cartera.SingleOrDefaultAsync(e => e.EmpresaId == empresaId && e.Origen == origen && e.OrigenReferencia == origenReferencia, ct);

    public void Agregar(AnulacionEfecto anulacion) => _ctx.AnulacionesCartera.Add(anulacion);

    public async Task<IReadOnlySet<Guid>> AnuladosAsync(IReadOnlyCollection<Guid> efectoIds, CancellationToken ct = default) =>
        (await _ctx.AnulacionesCartera.AsNoTracking().Where(a => efectoIds.Contains(a.EfectoId)).Select(a => a.EfectoId).ToListAsync(ct).ConfigureAwait(false)).ToHashSet();
}

internal sealed class ConfiguracionAnulacionEfecto : IEntityTypeConfiguration<AnulacionEfecto>
{
    public void Configure(EntityTypeBuilder<AnulacionEfecto> builder)
    {
        builder.ToTable("anulacion_efecto_cartera");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.EfectoId).HasColumnName("efecto_id").IsRequired();
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.Motivo).HasColumnName("motivo").HasMaxLength(EfectoCartera.LongitudTexto).IsRequired();
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasOne<EfectoCartera>().WithMany().HasForeignKey(a => a.EfectoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => a.EfectoId).IsUnique().HasDatabaseName("ux_anulacion_efecto_cartera");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class ConfiguracionPrevision : IEntityTypeConfiguration<PrevisionTesoreria>
{
    public void Configure(EntityTypeBuilder<PrevisionTesoreria> builder)
    {
        builder.ToTable("prevision");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.Sentido).HasColumnName("sentido").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.Concepto).HasColumnName("concepto").HasMaxLength(PrevisionTesoreria.LongitudMaximaConcepto).IsRequired();
        builder.Property(p => p.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(p => p.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasIndex(p => new { p.EmpresaId, p.Fecha }).HasDatabaseName("ix_prevision_empresa_fecha");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class RepositorioPrevisiones : IRepositorioPrevisiones
{
    private readonly TesoreriaDbContext _contexto;
    public RepositorioPrevisiones(TesoreriaDbContext contexto) => _contexto = contexto;

    public void Agregar(PrevisionTesoreria prevision) => _contexto.Previsiones.Add(prevision);
    public Task<PrevisionTesoreria?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Previsiones.SingleOrDefaultAsync(p => p.Id == id, ct);
    public void Eliminar(PrevisionTesoreria prevision) => _contexto.Previsiones.Remove(prevision);

    public async Task<IReadOnlyList<PrevisionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = await _contexto.Previsiones.AsNoTracking().Where(p => p.EmpresaId == empresaId)
            .OrderBy(p => p.Fecha).ToListAsync(ct).ConfigureAwait(false);
        return lista.Select(PrevisionDto.Desde).ToList();
    }
}

internal sealed class RepositorioMovimientos : IRepositorioMovimientos, IConsultaTesoreria
{
    private readonly TesoreriaDbContext _contexto;

    public RepositorioMovimientos(TesoreriaDbContext contexto) => _contexto = contexto;

    public void Agregar(Movimiento movimiento) => _contexto.Movimientos.Add(movimiento);

    public async Task<decimal> SumaAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default)
    {
        var suma = await _contexto.Movimientos
            .Where(m => m.TipoDocumento == tipo && m.DocumentoId == documentoId)
            .SumAsync(m => (decimal?)m.Importe, ct).ConfigureAwait(false);
        return suma ?? 0m;
    }

    public async Task<IReadOnlyList<Movimiento>> ListarAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default) =>
        await _contexto.Movimientos
            .Where(m => m.TipoDocumento == tipo && m.DocumentoId == documentoId)
            .OrderBy(m => m.Fecha).ThenBy(m => m.CreadoEn)
            .ToListAsync(ct).ConfigureAwait(false);

    public Task<Movimiento?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Movimientos.SingleOrDefaultAsync(m => m.Id == id, ct);

    public Task<bool> EstaAnuladoAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Movimientos.AnyAsync(m => m.AnulaMovimientoId == id, ct);

    public Task<Anticipo?> AnticipoDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _contexto.Anticipos.Include(a => a.Aplicaciones).SingleOrDefaultAsync(a => a.Aplicaciones.Any(x => x.MovimientoId == movimientoId), ct);

    public async Task<decimal> TotalLiquidadoAsync(TipoDocumentoTesoreria tipo, CancellationToken ct = default)
    {
        var suma = await _contexto.Movimientos
            .Where(m => m.TipoDocumento == tipo)
            .SumAsync(m => (decimal?)m.Importe, ct).ConfigureAwait(false);
        return suma ?? 0m;
    }

    public async Task<IReadOnlyList<MovimientoDto>> ListarPorPeriodoAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var movimientos = await _contexto.Movimientos
            .Where(m => m.EmpresaId == empresaId && m.Fecha >= desde && m.Fecha <= hasta)
            .OrderBy(m => m.Fecha)
            .ToListAsync(ct).ConfigureAwait(false);
        return movimientos.Select(MovimientoDto.Desde).ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> LiquidadoPorDocumentosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(documentoIds);
        if (documentoIds.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var filas = await _contexto.Movimientos
            .Where(m => m.TipoDocumento == tipo && documentoIds.Contains(m.DocumentoId))
            .GroupBy(m => m.DocumentoId)
            .Select(g => new { g.Key, Suma = g.Sum(m => m.Importe) })
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.ToDictionary(f => f.Key, f => f.Suma);
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class TesoreriaDbContextFactory : IDesignTimeDbContextFactory<TesoreriaDbContext>
{
    public TesoreriaDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<TesoreriaDbContext>().UseNpgsql(conexion).Options;
        return new TesoreriaDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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

internal sealed class ConfiguracionAnticipo : IEntityTypeConfiguration<Anticipo>
{
    public void Configure(EntityTypeBuilder<Anticipo> builder)
    {
        builder.ToTable("anticipo");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(a => a.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.Concepto).HasColumnName("concepto").HasMaxLength(Anticipo.LongitudMaximaConcepto).IsRequired();
        builder.Property(a => a.Metodo).HasColumnName("metodo").HasMaxLength(60);
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(a => a.AnuladoEn).HasColumnName("anulado_en");
        builder.Property(a => a.FacturaId).HasColumnName("factura_id");
        builder.Property(a => a.FacturaNumero).HasColumnName("factura_numero").HasMaxLength(40);
        builder.Property(a => a.BaseFacturada).HasColumnName("base_facturada").HasColumnType("numeric(14,2)");
        builder.Property(a => a.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10);
        builder.HasIndex(a => a.FacturaId).HasDatabaseName("ix_anticipo_factura");
        builder.Ignore(a => a.Facturado);
        builder.Ignore(a => a.DisponibleBase);
        builder.OwnsMany(a => a.Aplicaciones, ap =>
        {
            ap.ToTable("aplicacion_anticipo");
            ap.WithOwner().HasForeignKey("anticipo_id");
            ap.HasKey(x => x.Id);
            ap.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            ap.Property(x => x.FacturaId).HasColumnName("factura_id").IsRequired();
            ap.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            ap.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            ap.Property(x => x.MovimientoId).HasColumnName("movimiento_id");
            ap.Property(x => x.Base).HasColumnName("base").HasColumnType("numeric(14,2)");
            ap.HasIndex("anticipo_id").HasDatabaseName("ix_aplicacion_anticipo_anticipo");
            ap.HasIndex(x => x.MovimientoId).IsUnique().HasDatabaseName("ux_aplicacion_anticipo_movimiento");
        });
        builder.Navigation(a => a.Aplicaciones).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(a => a.Aplicado);
        builder.Ignore(a => a.Disponible);
        builder.Ignore(a => a.Estado);
        builder.HasIndex(a => new { a.EmpresaId, a.ClienteId }).HasDatabaseName("ix_anticipo_empresa_cliente");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class ConfiguracionReclamacion : IEntityTypeConfiguration<Reclamacion>
{
    public void Configure(EntityTypeBuilder<Reclamacion> builder)
    {
        builder.ToTable("reclamacion");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(r => r.FacturaId).HasColumnName("factura_id").IsRequired();
        builder.Property(r => r.Nivel).HasColumnName("nivel").IsRequired();
        builder.Property(r => r.Canal).HasColumnName("canal").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(r => r.Pendiente).HasColumnName("pendiente").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(r => r.DiasRetraso).HasColumnName("dias_retraso").IsRequired();
        builder.Property(r => r.Nota).HasColumnName("nota").HasMaxLength(Reclamacion.LongitudMaximaNota);
        builder.Property(r => r.RealizadaEn).HasColumnName("realizada_en").IsRequired();
        builder.HasIndex(r => new { r.EmpresaId, r.FacturaId }).HasDatabaseName("ix_reclamacion_empresa_factura");
        builder.Ignore(r => r.EventosDominio);
    }
}

internal sealed class ConfiguracionConfiguracionReclamaciones : IEntityTypeConfiguration<ConfiguracionReclamaciones>
{
    private static readonly System.Text.Json.JsonSerializerOptions Json = new(System.Text.Json.JsonSerializerDefaults.Web);

    public void Configure(EntityTypeBuilder<ConfiguracionReclamaciones> builder)
    {
        builder.ToTable("configuracion_reclamaciones");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Niveles).HasColumnName("niveles").HasColumnType("jsonb").IsRequired()
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, Json),
                v => System.Text.Json.JsonSerializer.Deserialize<List<NivelReclamacion>>(v, Json) ?? new List<NivelReclamacion>(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<NivelReclamacion>>(
                    (a, b) => a!.SequenceEqual(b!), v => v.Aggregate(0, (h, n) => HashCode.Combine(h, n.GetHashCode())), v => v.ToList()));
        builder.HasIndex(c => c.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_reclamaciones_empresa");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class RepositorioAnticipos : IRepositorioAnticipos
{
    private readonly TesoreriaDbContext _contexto;

    public RepositorioAnticipos(TesoreriaDbContext contexto) => _contexto = contexto;

    public void Agregar(Anticipo anticipo) => _contexto.Anticipos.Add(anticipo);

    public Task<Anticipo?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Anticipos.SingleOrDefaultAsync(a => a.Id == id, ct);

    public Task<Anticipo?> ObtenerPorFacturaAsync(Guid facturaId, CancellationToken ct = default) =>
        _contexto.Anticipos.SingleOrDefaultAsync(a => a.FacturaId == facturaId, ct);

    public async Task<IReadOnlyList<Anticipo>> ListarAsync(Guid? clienteId, CancellationToken ct = default) =>
        await _contexto.Anticipos.Where(a => clienteId == null || a.ClienteId == clienteId)
            .OrderByDescending(a => a.Fecha).ToListAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioReclamaciones : IRepositorioReclamaciones
{
    private readonly TesoreriaDbContext _contexto;

    public RepositorioReclamaciones(TesoreriaDbContext contexto) => _contexto = contexto;

    public void Agregar(Reclamacion reclamacion) => _contexto.Reclamaciones.Add(reclamacion);

    public async Task<IReadOnlyList<Reclamacion>> ListarAsync(IReadOnlyCollection<Guid> facturaIds, CancellationToken ct = default) =>
        await _contexto.Reclamaciones.Where(r => facturaIds.Contains(r.FacturaId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<ConfiguracionReclamaciones?> ConfiguracionAsync(CancellationToken ct = default) =>
        _contexto.ConfiguracionesReclamacion.SingleOrDefaultAsync(ct);

    public void AgregarConfiguracion(ConfiguracionReclamaciones configuracion) => _contexto.ConfiguracionesReclamacion.Add(configuracion);
}

/// <summary>Mapeo de la bandeja de salida (outbox) de Tesorería: los asientos de cobros, pagos y anticipos.</summary>
internal sealed class ConfiguracionMensajeSalida : IEntityTypeConfiguration<MensajeSalida>
{
    public void Configure(EntityTypeBuilder<MensajeSalida> builder)
    {
        builder.ToTable("mensaje_salida");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(m => m.Tipo).HasColumnName("tipo").HasMaxLength(40).IsRequired();
        builder.Property(m => m.Carga).HasColumnName("carga").HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.Procesado).HasColumnName("procesado").IsRequired();
        builder.Property(m => m.Intentos).HasColumnName("intentos").IsRequired();
        builder.Property(m => m.UltimoError).HasColumnName("ultimo_error").HasMaxLength(500);
        builder.Property(m => m.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(m => m.ProcesadoEn).HasColumnName("procesado_en");
        builder.HasIndex(m => new { m.EmpresaId, m.Procesado }).HasDatabaseName("ix_mensaje_salida_empresa_procesado");
        builder.Ignore(m => m.EventosDominio);
    }
}

internal sealed class RepositorioSalidaTesoreria : IRepositorioSalidaTesoreria
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioSalidaTesoreria(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(MensajeSalida mensaje) => _ctx.MensajesSalida.Add(mensaje);

    public async Task<IReadOnlyList<MensajeSalida>> PendientesAsync(int maximo, CancellationToken ct = default) =>
        await _ctx.MensajesSalida.Where(m => !m.Procesado).OrderBy(m => m.CreadoEn).Take(maximo).ToListAsync(ct).ConfigureAwait(false);
}
