using AlxorCore.Logistica.Aplicacion;
using AlxorCore.Logistica.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Logistica.Infraestructura;

/// <summary>Contexto de persistencia del módulo de logística (esquema <c>logistica</c>).</summary>
public sealed class LogisticaDbContext : DbContextEmpresaBase, IUnidadDeTrabajoLogistica
{
    public const string Esquema = "logistica";

    public LogisticaDbContext(DbContextOptions<LogisticaDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    private const string SqlBorradoEmpresa = """
        DELETE FROM logistica.unidad_logistica WHERE empresa_id = {0};
        DELETE FROM logistica.plantilla_paletizado WHERE empresa_id = {0};
        DELETE FROM logistica.ficha_logistica WHERE empresa_id = {0};
        DELETE FROM logistica.tipo_soporte WHERE empresa_id = {0};
        DELETE FROM logistica.configuracion WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LogisticaDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todos los datos logísticos de la empresa (solo dentro de <c>BorradoEmpresa</c>).</summary>
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

internal sealed class ConfiguracionConfiguracion : IEntityTypeConfiguration<ConfiguracionLogistica>
{
    public void Configure(EntityTypeBuilder<ConfiguracionLogistica> b)
    {
        Columnas.Base(b, "configuracion");
        b.Property(x => x.PrefijoGs1).HasColumnName("prefijo_gs1").HasMaxLength(10).IsRequired();
        b.Property(x => x.DigitoExtension).HasColumnName("digito_extension").IsRequired();
        b.Property(x => x.UltimaSerie).HasColumnName("ultima_serie").IsRequired();
        b.Property(x => x.MezclarLotes).HasColumnName("mezclar_lotes").IsRequired();
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_logistica_empresa");
    }
}

internal sealed class ConfiguracionSoporte : IEntityTypeConfiguration<TipoSoporte>
{
    public void Configure(EntityTypeBuilder<TipoSoporte> b)
    {
        Columnas.Base(b, "tipo_soporte");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        b.Property(x => x.LargoMm).HasColumnName("largo_mm").IsRequired();
        b.Property(x => x.AnchoMm).HasColumnName("ancho_mm").IsRequired();
        b.Property(x => x.AltoMm).HasColumnName("alto_mm").IsRequired();
        b.Property(x => x.TaraKg).HasColumnName("tara_kg").HasColumnType("numeric(10,3)").IsRequired();
        b.Property(x => x.CargaMaxKg).HasColumnName("carga_max_kg").HasColumnType("numeric(10,3)");
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_tipo_soporte_codigo");
        b.HasIndex(x => x.EnvaseProductoId).HasDatabaseName("ix_tipo_soporte_envase");
    }
}

internal sealed class ConfiguracionFicha : IEntityTypeConfiguration<FichaLogistica>
{
    public void Configure(EntityTypeBuilder<FichaLogistica> b)
    {
        Columnas.Base(b, "ficha_logistica");
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Gtin).HasColumnName("gtin").HasMaxLength(14);
        b.Property(x => x.GtinCaja).HasColumnName("gtin_caja").HasMaxLength(14);
        b.Property(x => x.UnidadesPorCaja).HasColumnName("unidades_por_caja").IsRequired();
        b.Property(x => x.PesoNetoUnidadKg).HasColumnName("peso_neto_unidad_kg").HasColumnType("numeric(12,4)");
        b.Property(x => x.PesoBrutoCajaKg).HasColumnName("peso_bruto_caja_kg").HasColumnType("numeric(12,4)");
        b.Property(x => x.LargoCajaMm).HasColumnName("largo_caja_mm");
        b.Property(x => x.AnchoCajaMm).HasColumnName("ancho_caja_mm");
        b.Property(x => x.AltoCajaMm).HasColumnName("alto_caja_mm");
        b.Property(x => x.CajasPorCapa).HasColumnName("cajas_por_capa");
        b.Property(x => x.Capas).HasColumnName("capas");
        b.Property(x => x.SoporteId).HasColumnName("soporte_id");
        b.Property(x => x.AlturaMaxPaleMm).HasColumnName("altura_max_pale_mm");
        b.Property(x => x.PesoMaxPaleKg).HasColumnName("peso_max_pale_kg").HasColumnType("numeric(10,3)");
        b.Property(x => x.Remontable).HasColumnName("remontable").IsRequired();
        b.Property(x => x.TemperaturaMinC).HasColumnName("temperatura_min_c");
        b.Property(x => x.TemperaturaMaxC).HasColumnName("temperatura_max_c");
        b.Property(x => x.VidaUtilDias).HasColumnName("vida_util_dias");
        b.Property(x => x.VidaMinimaEntregaDias).HasColumnName("vida_minima_entrega_dias");
        b.Property(x => x.GestionLotes).HasColumnName("gestion_lotes").IsRequired();
        b.Ignore(x => x.PesoNetoCajaKg);
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId }).IsUnique().HasDatabaseName("ux_ficha_logistica_producto");
        b.HasIndex(x => new { x.EmpresaId, x.Gtin }).HasDatabaseName("ix_ficha_logistica_gtin");
        b.HasIndex(x => new { x.EmpresaId, x.GtinCaja }).HasDatabaseName("ix_ficha_logistica_gtin_caja");
        b.HasIndex(x => x.SoporteId).HasDatabaseName("ix_ficha_logistica_soporte");
    }
}

internal sealed class ConfiguracionPlantilla : IEntityTypeConfiguration<PlantillaPaletizado>
{
    public void Configure(EntityTypeBuilder<PlantillaPaletizado> b)
    {
        Columnas.Base(b, "plantilla_paletizado");
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.SoporteId).HasColumnName("soporte_id").IsRequired();
        b.Property(x => x.CajasPorCapa).HasColumnName("cajas_por_capa").IsRequired();
        b.Property(x => x.Capas).HasColumnName("capas").IsRequired();
        b.Property(x => x.AlturaMaxMm).HasColumnName("altura_max_mm");
        b.Property(x => x.PesoMaxKg).HasColumnName("peso_max_kg").HasColumnType("numeric(10,3)");
        b.Property(x => x.Instrucciones).HasColumnName("instrucciones").HasMaxLength(500);
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Ignore(x => x.CajasPorPale);
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId }).HasDatabaseName("ix_plantilla_paletizado_producto");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_plantilla_paletizado_cliente");
        b.HasIndex(x => x.SoporteId).HasDatabaseName("ix_plantilla_paletizado_soporte");
    }
}

internal sealed class ConfiguracionUnidad : IEntityTypeConfiguration<UnidadLogistica>
{
    public void Configure(EntityTypeBuilder<UnidadLogistica> b)
    {
        Columnas.Base(b, "unidad_logistica");
        b.Property(x => x.Sscc).HasColumnName("sscc").HasMaxLength(18).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        Columnas.Enum(b.Property(x => x.Origen), "origen");
        b.Property(x => x.SoporteId).HasColumnName("soporte_id");
        b.Property(x => x.TaraKg).HasColumnName("tara_kg").HasColumnType("numeric(10,3)").IsRequired();
        b.Property(x => x.PadreId).HasColumnName("padre_id");
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id").IsRequired();
        b.Property(x => x.UbicacionId).HasColumnName("ubicacion_id");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.PedidoVentaId).HasColumnName("pedido_venta_id");
        b.Property(x => x.OrdenFabricacionId).HasColumnName("orden_fabricacion_id");
        b.Property(x => x.PlantillaId).HasColumnName("plantilla_id");
        b.Property(x => x.CajasCompleta).HasColumnName("cajas_completa");
        b.Property(x => x.AlturaMm).HasColumnName("altura_mm");
        b.Property(x => x.PesoBrutoKg).HasColumnName("peso_bruto_kg").HasColumnType("numeric(12,3)").IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.CerradaEn).HasColumnName("cerrada_en");
        b.Property(x => x.FechaExpedicion).HasColumnName("fecha_expedicion");
        b.Property(x => x.ReferenciaExpedicion).HasColumnName("referencia_expedicion").HasMaxLength(60);
        b.Ignore(x => x.Cajas);
        b.Ignore(x => x.PesoNetoKg);
        b.Ignore(x => x.Completa);
        b.Ignore(x => x.Viva);
        b.HasIndex(x => new { x.EmpresaId, x.Sscc }).IsUnique().HasDatabaseName("ux_unidad_logistica_sscc");
        b.HasIndex(x => new { x.EmpresaId, x.Estado, x.AlmacenId }).HasDatabaseName("ix_unidad_logistica_estado");
        b.HasIndex(x => x.PadreId).HasDatabaseName("ix_unidad_logistica_padre");
        b.HasIndex(x => x.PedidoVentaId).HasDatabaseName("ix_unidad_logistica_pedido");
        b.HasIndex(x => x.OrdenFabricacionId).HasDatabaseName("ix_unidad_logistica_orden");
        b.HasIndex(x => x.SoporteId).HasDatabaseName("ix_unidad_logistica_soporte");
        b.HasIndex(x => x.PlantillaId).HasDatabaseName("ix_unidad_logistica_plantilla");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_unidad_logistica_cliente");
        b.HasIndex(x => x.AlmacenId).HasDatabaseName("ix_unidad_logistica_almacen");
        b.HasIndex(x => x.UbicacionId).HasDatabaseName("ix_unidad_logistica_ubicacion");
        b.OwnsMany(x => x.Contenido, l =>
        {
            l.ToTable("linea_unidad_logistica");
            l.WithOwner().HasForeignKey("unidad_logistica_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            l.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(60);
            l.Property(x => x.FechaCaducidad).HasColumnName("fecha_caducidad");
            l.Property(x => x.Cajas).HasColumnName("cajas").IsRequired();
            l.Property(x => x.Unidades).HasColumnName("unidades").HasColumnType("numeric(14,4)").IsRequired();
            l.Property(x => x.PesoNetoKg).HasColumnName("peso_neto_kg").HasColumnType("numeric(12,3)").IsRequired();
            l.Property(x => x.PesoBrutoKg).HasColumnName("peso_bruto_kg").HasColumnType("numeric(12,3)").IsRequired();
            l.HasIndex(x => new { x.ProductoId, x.Lote }).HasDatabaseName("ix_linea_unidad_logistica_producto");
        });
        b.Navigation(x => x.Contenido).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RepositorioLogistica : IRepositorioLogistica
{
    private readonly LogisticaDbContext _ctx;

    public RepositorioLogistica(LogisticaDbContext ctx) => _ctx = ctx;

    public async Task<ConfiguracionLogistica?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        _ctx.Set<ConfiguracionLogistica>().Local.FirstOrDefault(c => c.EmpresaId == empresaId)
        ?? await _ctx.Set<ConfiguracionLogistica>().FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct).ConfigureAwait(false);

    public void Agregar(ConfiguracionLogistica configuracion) => _ctx.Add(configuracion);

    public async Task<IReadOnlyList<TipoSoporte>> SoportesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<TipoSoporte>().Where(s => s.EmpresaId == empresaId).OrderBy(s => s.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<TipoSoporte?> SoporteAsync(Guid id, CancellationToken ct = default) => _ctx.Set<TipoSoporte>().FirstOrDefaultAsync(s => s.Id == id, ct);

    public void Agregar(TipoSoporte soporte) => _ctx.Add(soporte);

    public void Eliminar(TipoSoporte soporte) => _ctx.Remove(soporte);

    public async Task<bool> SoporteUsadoAsync(Guid soporteId, CancellationToken ct = default) =>
        await _ctx.Set<UnidadLogistica>().AnyAsync(u => u.SoporteId == soporteId, ct).ConfigureAwait(false)
        || await _ctx.Set<PlantillaPaletizado>().AnyAsync(p => p.SoporteId == soporteId, ct).ConfigureAwait(false)
        || await _ctx.Set<FichaLogistica>().AnyAsync(f => f.SoporteId == soporteId, ct).ConfigureAwait(false);

    public Task<FichaLogistica?> FichaAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        _ctx.Set<FichaLogistica>().FirstOrDefaultAsync(f => f.EmpresaId == empresaId && f.ProductoId == productoId, ct);

    public async Task<IReadOnlyList<FichaLogistica>> FichasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<FichaLogistica>().AsNoTracking().Where(f => f.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<FichaLogistica?> FichaPorGtinAsync(Guid empresaId, string gtin14, CancellationToken ct = default) =>
        _ctx.Set<FichaLogistica>().FirstOrDefaultAsync(f => f.EmpresaId == empresaId && (f.Gtin == gtin14 || f.GtinCaja == gtin14), ct);

    public void Agregar(FichaLogistica ficha) => _ctx.Add(ficha);

    public void Eliminar(FichaLogistica ficha) => _ctx.Remove(ficha);

    public async Task<IReadOnlyList<PlantillaPaletizado>> PlantillasAsync(Guid empresaId, Guid? productoId, CancellationToken ct = default) =>
        await _ctx.Set<PlantillaPaletizado>().Where(p => p.EmpresaId == empresaId && (productoId == null || p.ProductoId == productoId))
            .OrderBy(p => p.ProductoId).ThenBy(p => p.ClienteId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PlantillaPaletizado?> PlantillaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PlantillaPaletizado>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PlantillaPaletizado plantilla) => _ctx.Add(plantilla);

    public void Eliminar(PlantillaPaletizado plantilla) => _ctx.Remove(plantilla);

    public Task<bool> PlantillaUsadaAsync(Guid plantillaId, CancellationToken ct = default) => _ctx.Set<UnidadLogistica>().AnyAsync(u => u.PlantillaId == plantillaId, ct);

    public Task<UnidadLogistica?> UnidadAsync(Guid id, CancellationToken ct = default) => _ctx.Set<UnidadLogistica>().FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<UnidadLogistica?> UnidadPorSsccAsync(Guid empresaId, string sscc, CancellationToken ct = default) =>
        _ctx.Set<UnidadLogistica>().FirstOrDefaultAsync(u => u.EmpresaId == empresaId && u.Sscc == sscc, ct);

    public async Task<IReadOnlyList<UnidadLogistica>> UnidadesAsync(Guid empresaId, FiltroUnidades filtro, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var q = _ctx.Set<UnidadLogistica>().Where(u => u.EmpresaId == empresaId);
        if (filtro.Estado is { } e)
        {
            q = q.Where(u => u.Estado == e);
        }

        if (filtro.AlmacenId is { } a)
        {
            q = q.Where(u => u.AlmacenId == a);
        }

        if (filtro.ProductoId is { } p)
        {
            q = q.Where(u => u.Contenido.Any(l => l.ProductoId == p && (filtro.Lote == null || l.Lote == filtro.Lote)));
        }
        else if (filtro.Lote is { } lote)
        {
            q = q.Where(u => u.Contenido.Any(l => l.Lote == lote));
        }

        if (filtro.PedidoVentaId is { } pv)
        {
            q = q.Where(u => u.PedidoVentaId == pv);
        }

        if (filtro.OrdenFabricacionId is { } of)
        {
            q = q.Where(u => u.OrdenFabricacionId == of);
        }

        if (filtro.SoloRaiz)
        {
            q = q.Where(u => u.PadreId == null);
        }

        return await q.OrderByDescending(u => u.CreadaEn).Take(Math.Clamp(filtro.Maximo, 1, 5000)).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<UnidadLogistica>> HijasAsync(Guid padreId, CancellationToken ct = default) =>
        await _ctx.Set<UnidadLogistica>().Where(u => u.PadreId == padreId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<string, decimal>> PaletizadoAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? salvoUnidadId, CancellationToken ct = default)
    {
        var lineas = await _ctx.Set<UnidadLogistica>().AsNoTracking()
            .Where(u => u.EmpresaId == empresaId && u.AlmacenId == almacenId && u.Id != salvoUnidadId
                && (u.Estado == EstadoUnidadLogistica.Abierta || u.Estado == EstadoUnidadLogistica.Cerrada))
            .SelectMany(u => u.Contenido).Where(l => l.ProductoId == productoId).Select(l => new { l.Lote, l.Unidades }).ToListAsync(ct).ConfigureAwait(false);
        return lineas.GroupBy(l => l.Lote ?? string.Empty, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Sum(l => l.Unidades), StringComparer.Ordinal);
    }

    public void Agregar(UnidadLogistica unidad) => _ctx.Add(unidad);
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class LogisticaDbContextFactory : IDesignTimeDbContextFactory<LogisticaDbContext>
{
    public LogisticaDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<LogisticaDbContext>()
            .UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", LogisticaDbContext.Esquema)).Options;
        return new LogisticaDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
