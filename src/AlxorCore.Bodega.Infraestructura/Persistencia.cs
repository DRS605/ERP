using AlxorCore.Bodega.Aplicacion;
using AlxorCore.Bodega.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Bodega.Infraestructura;

/// <summary>Contexto de persistencia del módulo de bodegas (esquema <c>bodega</c>).</summary>
public sealed class BodegaDbContext : DbContextEmpresaBase, IUnidadDeTrabajoBodega
{
    public const string Esquema = "bodega";

    public BodegaDbContext(DbContextOptions<BodegaDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    private const string SqlBorradoEmpresa = """
        DELETE FROM bodega.liquidacion_uva WHERE empresa_id = {0};
        DELETE FROM bodega.operacion WHERE empresa_id = {0};
        DELETE FROM bodega.entrada_uva WHERE empresa_id = {0};
        DELETE FROM bodega.precio_uva WHERE empresa_id = {0};
        DELETE FROM bodega.deposito WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BodegaDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todos los datos de la bodega de la empresa (solo dentro de <c>BorradoEmpresa</c>).</summary>
    public Task BorrarEmpresaAsync(Guid empresaId, CancellationToken ct = default) =>
        Database.ExecuteSqlRawAsync(SqlBorradoEmpresa, [empresaId], ct);
}

internal static class Columnas
{
    public const string Litros = "numeric(12,2)";
    public const string Kilos = "numeric(12,2)";
    public const string Importe = "numeric(14,2)";
    public const string PrecioKg = "numeric(12,6)";
    public const string Grado = "numeric(5,2)";

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

internal sealed class ConfiguracionDeposito : IEntityTypeConfiguration<Deposito>
{
    public void Configure(EntityTypeBuilder<Deposito> b)
    {
        Columnas.Base(b, "deposito");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasBodega.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasBodega.LongitudNombre).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.CapacidadLitros).HasColumnName("capacidad_litros").HasColumnType(Columnas.Litros).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.Property(x => x.Litros).HasColumnName("litros").HasColumnType(Columnas.Litros).IsRequired();
        b.Property(x => x.Producto).HasColumnName("producto").HasMaxLength(30).HasConversion<string>();
        b.Property(x => x.Calificacion).HasColumnName("calificacion").HasMaxLength(ReglasBodega.LongitudCalificacion);
        b.Property(x => x.UltimaOperacionId).HasColumnName("ultima_operacion_id");
        b.Ignore(x => x.Vacio);
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_deposito_codigo");
        b.OwnsMany(x => x.Composicion, c =>
        {
            c.ToTable("componente_deposito");
            c.WithOwner().HasForeignKey("deposito_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(ReglasBodega.LongitudVariedad).IsRequired();
            c.Property(x => x.Anada).HasColumnName("anada").IsRequired();
            c.Property(x => x.Litros).HasColumnName("litros").HasColumnType(Columnas.Litros).IsRequired();
            c.HasIndex("deposito_id", nameof(ComponenteDeposito.Variedad), nameof(ComponenteDeposito.Anada)).IsUnique().HasDatabaseName("ux_componente_deposito");
        });
        b.Navigation(x => x.Composicion).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionEntradaUva : IEntityTypeConfiguration<EntradaUva>
{
    public void Configure(EntityTypeBuilder<EntradaUva> b)
    {
        Columnas.Base(b, "entrada_uva");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Anada).HasColumnName("anada").IsRequired();
        b.Property(x => x.ViticultorId).HasColumnName("viticultor_id").IsRequired();
        b.Property(x => x.ViticultorNombre).HasColumnName("viticultor_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(ReglasBodega.LongitudVariedad).IsRequired();
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.GradoBaume).HasColumnName("grado_baume").HasColumnType(Columnas.Grado).IsRequired();
        b.Property(x => x.Parcela).HasColumnName("parcela").HasMaxLength(ReglasBodega.LongitudNombre);
        b.Property(x => x.Calificacion).HasColumnName("calificacion").HasMaxLength(ReglasBodega.LongitudCalificacion);
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(ReglasBodega.LongitudObservaciones);
        b.Property(x => x.OperacionId).HasColumnName("operacion_id");
        b.Property(x => x.LiquidacionId).HasColumnName("liquidacion_id");
        b.Property(x => x.Anulada).HasColumnName("anulada").IsRequired();
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_entrada_uva_numero");
        b.HasIndex(x => new { x.EmpresaId, x.ViticultorId, x.Fecha }).HasDatabaseName("ix_entrada_uva_viticultor");
        b.HasIndex(x => x.ViticultorId).HasDatabaseName("ix_entrada_uva_proveedor");
        b.HasIndex(x => x.OperacionId).HasDatabaseName("ix_entrada_uva_operacion");
        b.HasIndex(x => x.LiquidacionId).HasDatabaseName("ix_entrada_uva_liquidacion");
    }
}

internal sealed class ConfiguracionPrecioUva : IEntityTypeConfiguration<PrecioUva>
{
    public void Configure(EntityTypeBuilder<PrecioUva> b)
    {
        Columnas.Base(b, "precio_uva");
        b.Property(x => x.Anada).HasColumnName("anada").IsRequired();
        b.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(ReglasBodega.LongitudVariedad).IsRequired();
        b.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg).IsRequired();
        b.Property(x => x.GradoReferencia).HasColumnName("grado_referencia").HasColumnType(Columnas.Grado).IsRequired();
        b.Property(x => x.PorcentajePorGrado).HasColumnName("porcentaje_por_grado").HasColumnType("numeric(7,2)").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Anada, x.Variedad }).IsUnique().HasDatabaseName("ux_precio_uva");
    }
}

internal sealed class ConfiguracionLiquidacionUva : IEntityTypeConfiguration<LiquidacionUva>
{
    public void Configure(EntityTypeBuilder<LiquidacionUva> b)
    {
        Columnas.Base(b, "liquidacion_uva");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.ViticultorId).HasColumnName("viticultor_id").IsRequired();
        b.Property(x => x.ViticultorNombre).HasColumnName("viticultor_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        b.Property(x => x.CodigoImpuesto).HasColumnName("codigo_impuesto").HasMaxLength(20).IsRequired();
        b.Property(x => x.PorcentajeRetencion).HasColumnName("porcentaje_retencion").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.BaseImponible).HasColumnName("base_imponible").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.TotalFactura).HasColumnName("total_factura").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.GastoId).HasColumnName("gasto_id");
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_liquidacion_uva_numero");
        b.HasIndex(x => x.ViticultorId).HasDatabaseName("ix_liquidacion_uva_proveedor");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_liquidacion_uva");
            l.WithOwner().HasForeignKey("liquidacion_uva_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.EntradaUvaId).HasColumnName("entrada_uva_id").IsRequired();
            l.Property(x => x.Entrada).HasColumnName("entrada").HasMaxLength(30).IsRequired();
            l.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(ReglasBodega.LongitudVariedad).IsRequired();
            l.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            l.Property(x => x.GradoBaume).HasColumnName("grado_baume").HasColumnType(Columnas.Grado).IsRequired();
            l.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg).IsRequired();
            l.Property(x => x.Importe).HasColumnName("importe").HasColumnType(Columnas.Importe).IsRequired();
            l.HasIndex(x => x.EntradaUvaId).HasDatabaseName("ix_linea_liquidacion_uva_entrada");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionOperacion : IEntityTypeConfiguration<OperacionBodega>
{
    public void Configure(EntityTypeBuilder<OperacionBodega> b)
    {
        Columnas.Base(b, "operacion");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(ReglasBodega.LongitudObservaciones);
        b.Property(x => x.MermaLitros).HasColumnName("merma_litros").HasColumnType(Columnas.Litros).IsRequired();
        b.Property(x => x.KilosUva).HasColumnName("kilos_uva").HasColumnType(Columnas.Kilos);
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.Botellas).HasColumnName("botellas");
        b.Property(x => x.FormatoLitros).HasColumnName("formato_litros").HasColumnType("numeric(6,3)");
        b.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(40);
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.AlbaranId).HasColumnName("albaran_id");
        b.Property(x => x.AlbaranNumero).HasColumnName("albaran_numero").HasMaxLength(40);
        b.Property(x => x.Anulada).HasColumnName("anulada").IsRequired();
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_operacion_bodega_numero");
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_operacion_bodega_fecha");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_operacion_bodega_cliente");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_operacion_bodega_producto");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_operacion");
            l.WithOwner().HasForeignKey("operacion_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            l.Property(x => x.DepositoId).HasColumnName("deposito_id").IsRequired();
            l.Property(x => x.DepositoCodigo).HasColumnName("deposito_codigo").HasMaxLength(ReglasBodega.LongitudCodigo).IsRequired();
            l.Property(x => x.Clase).HasColumnName("clase").HasMaxLength(30).HasConversion<string>().IsRequired();
            l.Property(x => x.Litros).HasColumnName("litros").HasColumnType(Columnas.Litros).IsRequired();
            l.Property(x => x.Producto).HasColumnName("producto").HasMaxLength(30).HasConversion<string>().IsRequired();
            l.Property(x => x.Calificacion).HasColumnName("calificacion").HasMaxLength(ReglasBodega.LongitudCalificacion);
            l.Property(x => x.EstadoAnterior).HasColumnName("estado_anterior").HasColumnType("jsonb");
            l.HasOne<Deposito>().WithMany().HasForeignKey(x => x.DepositoId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_linea_operacion_deposito");
            l.HasIndex("operacion_id", nameof(LineaOperacionBodega.Orden)).IsUnique().HasDatabaseName("ux_linea_operacion_orden");
            l.HasIndex(x => x.DepositoId).HasDatabaseName("ix_linea_operacion_deposito");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RepositorioBodega : IRepositorioBodega
{
    private readonly BodegaDbContext _ctx;

    public RepositorioBodega(BodegaDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);

    public async Task<IReadOnlyList<Deposito>> DepositosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Deposito>().Where(d => d.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Deposito>> DepositosAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await _ctx.Set<Deposito>().Where(d => ids.Contains(d.Id)).ToListAsync(ct).ConfigureAwait(false);

    public Task<Deposito?> DepositoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Deposito>().FirstOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExisteDepositoAsync(Guid empresaId, string codigo, CancellationToken ct = default) =>
        _ctx.Set<Deposito>().AnyAsync(d => d.EmpresaId == empresaId && d.Codigo == codigo, ct);

    public Task<bool> DepositoConOperacionesAsync(Guid depositoId, CancellationToken ct = default) =>
        _ctx.Set<OperacionBodega>().AnyAsync(o => o.Lineas.Any(l => l.DepositoId == depositoId), ct);

    public async Task<IReadOnlyList<EntradaUva>> EntradasAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? viticultorId, CancellationToken ct = default) =>
        await _ctx.Set<EntradaUva>()
            .Where(e => e.EmpresaId == empresaId && (desde == null || e.Fecha >= desde) && (hasta == null || e.Fecha <= hasta) && (viticultorId == null || e.ViticultorId == viticultorId))
            .OrderByDescending(e => e.Fecha).ThenByDescending(e => e.Numero).Take(5000).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<EntradaUva>> EntradasAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await _ctx.Set<EntradaUva>().Where(e => ids.Contains(e.Id)).ToListAsync(ct).ConfigureAwait(false);

    public Task<EntradaUva?> EntradaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<EntradaUva>().FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<EntradaUva>> EntradasDeOperacionesAsync(IReadOnlyCollection<Guid> operacionIds, CancellationToken ct = default) =>
        operacionIds.Count == 0 ? []
            : await _ctx.Set<EntradaUva>().Where(e => e.OperacionId != null && operacionIds.Contains(e.OperacionId.Value)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroEntradaAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<EntradaUva>().Where(e => e.EmpresaId == empresaId && e.Ejercicio == ejercicio).Select(e => (int?)e.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;

    public async Task<IReadOnlyList<PrecioUva>> PreciosAsync(Guid empresaId, int? anada, CancellationToken ct = default) =>
        await _ctx.Set<PrecioUva>().Where(p => p.EmpresaId == empresaId && (anada == null || p.Anada == anada)).ToListAsync(ct).ConfigureAwait(false);

    public Task<PrecioUva?> PrecioAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PrecioUva>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<OperacionBodega>> OperacionesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _ctx.Set<OperacionBodega>().AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && (desde == null || o.Fecha >= desde) && (hasta == null || o.Fecha <= hasta))
            .OrderByDescending(o => o.Fecha).ThenByDescending(o => o.Numero).Take(2000).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<OperacionBodega>> OperacionesDeDepositoAsync(Guid depositoId, CancellationToken ct = default) =>
        await _ctx.Set<OperacionBodega>().AsNoTracking().Where(o => o.Lineas.Any(l => l.DepositoId == depositoId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<OperacionBodega?> OperacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<OperacionBodega>().FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<int> SiguienteNumeroOperacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<OperacionBodega>().Where(o => o.EmpresaId == empresaId && o.Ejercicio == ejercicio).Select(o => (int?)o.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;

    public async Task<IReadOnlyList<MovimientoLibroBodega>> LibroAsync(Guid empresaId, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Set<OperacionBodega>().AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && !o.Anulada && o.Fecha <= hasta)
            .SelectMany(o => o.Lineas.Select(l => new MovimientoLibroBodega(o.Id, o.Fecha, o.Tipo, l.Clase, l.DepositoId, l.Litros, l.Producto, l.Calificacion)))
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LiquidacionUva>> LiquidacionesAsync(Guid empresaId, Guid? viticultorId, CancellationToken ct = default) =>
        await _ctx.Set<LiquidacionUva>().AsNoTracking().Where(l => l.EmpresaId == empresaId && (viticultorId == null || l.ViticultorId == viticultorId))
            .OrderByDescending(l => l.Fecha).ThenByDescending(l => l.Numero).Take(1000).ToListAsync(ct).ConfigureAwait(false);

    public Task<LiquidacionUva?> LiquidacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<LiquidacionUva>().FirstOrDefaultAsync(l => l.Id == id, ct);

    public async Task<int> SiguienteNumeroLiquidacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<LiquidacionUva>().Where(l => l.EmpresaId == empresaId && l.Ejercicio == ejercicio).Select(l => (int?)l.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class BodegaDbContextFactory : IDesignTimeDbContextFactory<BodegaDbContext>
{
    public BodegaDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<BodegaDbContext>()
            .UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", BodegaDbContext.Esquema)).Options;
        return new BodegaDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
