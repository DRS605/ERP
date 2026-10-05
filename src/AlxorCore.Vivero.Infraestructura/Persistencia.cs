using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using AlxorCore.Vivero.Aplicacion;
using AlxorCore.Vivero.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Vivero.Infraestructura;

/// <summary>Contexto de persistencia del módulo de viveros (esquema <c>vivero</c>).</summary>
public sealed class ViveroDbContext : DbContextEmpresaBase, IUnidadDeTrabajoVivero
{
    public const string Esquema = "vivero";

    public ViveroDbContext(DbContextOptions<ViveroDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    private const string SqlBorradoEmpresa = """
        DELETE FROM vivero.encargo WHERE empresa_id = {0};
        DELETE FROM vivero.lote_planta WHERE empresa_id = {0};
        DELETE FROM vivero.configuracion WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ViveroDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todos los datos del vivero de la empresa (solo dentro de <c>BorradoEmpresa</c>).</summary>
    public Task BorrarEmpresaAsync(Guid empresaId, CancellationToken ct = default) =>
        Database.ExecuteSqlRawAsync(SqlBorradoEmpresa, [empresaId], ct);
}

internal static class Columnas
{
    public static void Base<T>(EntityTypeBuilder<T> b, string tabla)
        where T : RaizAgregadoEmpresa<Guid>
    {
        b.ToTable(tabla);
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Ignore(x => x.EventosDominio);
    }

    public static PropertyBuilder<TEnum> Enum<TEnum>(PropertyBuilder<TEnum> p, string columna) =>
        p.HasColumnName(columna).HasMaxLength(30).HasConversion<string>().IsRequired();
}

internal sealed class ConfiguracionConfiguracionVivero : IEntityTypeConfiguration<ConfiguracionVivero>
{
    public void Configure(EntityTypeBuilder<ConfiguracionVivero> b)
    {
        Columnas.Base(b, "configuracion");
        b.Property(x => x.CodigoRegistro).HasColumnName("codigo_registro").HasMaxLength(40).IsRequired();
        b.Property(x => x.PaisOrigen).HasColumnName("pais_origen").HasMaxLength(2).IsRequired();
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_vivero_empresa");
    }
}

internal sealed class ConfiguracionLotePlanta : IEntityTypeConfiguration<LotePlanta>
{
    public void Configure(EntityTypeBuilder<LotePlanta> b)
    {
        Columnas.Base(b, "lote_planta");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Especie).HasColumnName("especie").HasMaxLength(ReglasVivero.LongitudTexto).IsRequired();
        b.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(ReglasVivero.LongitudTexto);
        b.Property(x => x.Portainjerto).HasColumnName("portainjerto").HasMaxLength(ReglasVivero.LongitudTexto);
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.OrigenMaterial).HasColumnName("origen_material").HasMaxLength(ReglasVivero.LongitudTexto);
        b.Property(x => x.FechaSiembra).HasColumnName("fecha_siembra").IsRequired();
        b.Property(x => x.FechaPrevistaLista).HasColumnName("fecha_prevista_lista");
        Columnas.Enum(b.Property(x => x.Fase), "fase");
        b.Property(x => x.Ubicacion).HasColumnName("ubicacion").HasMaxLength(ReglasVivero.LongitudTexto);
        b.Property(x => x.PlantasIniciales).HasColumnName("plantas_iniciales").IsRequired();
        b.Property(x => x.PlantasVivas).HasColumnName("plantas_vivas").IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(ReglasVivero.LongitudObservaciones);
        b.Property(x => x.Anulado).HasColumnName("anulado").IsRequired();
        b.Ignore(x => x.Codigo);
        b.Ignore(x => x.Terminado);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_lote_planta_numero");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_lote_planta_producto");
        b.OwnsMany(x => x.Movimientos, m =>
        {
            m.ToTable("movimiento_lote");
            m.WithOwner().HasForeignKey("lote_planta_id");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            m.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            m.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            m.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(30).HasConversion<string>().IsRequired();
            m.Property(x => x.Plantas).HasColumnName("plantas").IsRequired();
            m.Property(x => x.Fase).HasColumnName("fase").HasMaxLength(30).HasConversion<string>().IsRequired();
            m.Property(x => x.Ubicacion).HasColumnName("ubicacion").HasMaxLength(ReglasVivero.LongitudTexto);
            m.Property(x => x.Concepto).HasColumnName("concepto").HasMaxLength(200);
            m.Property(x => x.DocumentoId).HasColumnName("documento_id");
            m.Property(x => x.AnulaId).HasColumnName("anula_id");
            m.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
            m.HasIndex("lote_planta_id", nameof(MovimientoLote.Orden)).IsUnique().HasDatabaseName("ux_movimiento_lote_orden");
            m.HasIndex(x => x.AnulaId).IsUnique().HasFilter("anula_id IS NOT NULL").HasDatabaseName("ux_movimiento_lote_anula");
            m.HasIndex(x => x.Fecha).HasDatabaseName("ix_movimiento_lote_fecha");
        });
        b.Navigation(x => x.Movimientos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionEncargo : IEntityTypeConfiguration<EncargoPlanta>
{
    public void Configure(EntityTypeBuilder<EncargoPlanta> b)
    {
        Columnas.Base(b, "encargo");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Plantas).HasColumnName("plantas").IsRequired();
        b.Property(x => x.FechaEntrega).HasColumnName("fecha_entrega").IsRequired();
        b.Property(x => x.PrecioPlanta).HasColumnName("precio_planta").HasColumnType("numeric(12,4)");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(ReglasVivero.LongitudObservaciones);
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.LoteId).HasColumnName("lote_id");
        b.Property(x => x.MovimientoId).HasColumnName("movimiento_id");
        b.Property(x => x.AlbaranId).HasColumnName("albaran_id");
        b.Property(x => x.AlbaranNumero).HasColumnName("albaran_numero").HasMaxLength(40);
        b.Ignore(x => x.NumeroCompleto);
        b.HasOne<LotePlanta>().WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_encargo_lote");
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_encargo_numero");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_encargo_cliente");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_encargo_producto");
        b.HasIndex(x => x.LoteId).HasDatabaseName("ix_encargo_lote");
    }
}

internal sealed class RepositorioVivero : IRepositorioVivero
{
    private readonly ViveroDbContext _ctx;

    public RepositorioVivero(ViveroDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public async Task<ConfiguracionVivero?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        _ctx.Set<ConfiguracionVivero>().Local.FirstOrDefault(c => c.EmpresaId == empresaId)
        ?? await _ctx.Set<ConfiguracionVivero>().FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LotePlanta>> LotesAsync(Guid empresaId, bool incluirTerminados, CancellationToken ct = default) =>
        await _ctx.Set<LotePlanta>().AsNoTracking().Where(l => l.EmpresaId == empresaId && (incluirTerminados || (!l.Anulado && l.PlantasVivas > 0)))
            .Take(5000).ToListAsync(ct).ConfigureAwait(false);

    public Task<LotePlanta?> LoteAsync(Guid id, CancellationToken ct = default) => _ctx.Set<LotePlanta>().FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<int> SiguienteNumeroLoteAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<LotePlanta>().Where(l => l.EmpresaId == empresaId && l.Ejercicio == ejercicio).Select(l => (int?)l.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;

    public async Task<IReadOnlyList<EncargoPlanta>> EncargosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<EncargoPlanta>().AsNoTracking().Where(e => e.EmpresaId == empresaId).Take(5000).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<EncargoPlanta>> EncargosDeLoteAsync(Guid loteId, CancellationToken ct = default) =>
        await _ctx.Set<EncargoPlanta>().Where(e => e.LoteId == loteId).ToListAsync(ct).ConfigureAwait(false);

    public Task<EncargoPlanta?> EncargoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<EncargoPlanta>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<int> SiguienteNumeroEncargoAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<EncargoPlanta>().Where(e => e.EmpresaId == empresaId && e.Ejercicio == ejercicio).Select(e => (int?)e.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class ViveroDbContextFactory : IDesignTimeDbContextFactory<ViveroDbContext>
{
    public ViveroDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ViveroDbContext>()
            .UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", ViveroDbContext.Esquema)).Options;
        return new ViveroDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
