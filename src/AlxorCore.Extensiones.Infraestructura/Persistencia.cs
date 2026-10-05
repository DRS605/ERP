using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Extensiones.Infraestructura;

/// <summary>Contexto de persistencia de las extensiones: campos personalizados, adjuntos y alertas (esquema <c>extensiones</c>).</summary>
public sealed class ExtensionesDbContext : DbContextEmpresaBase, IUnidadDeTrabajoExtensiones
{
    public const string Esquema = "extensiones";

    public ExtensionesDbContext(DbContextOptions<ExtensionesDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    private const string SqlBorradoEmpresa = """
        DELETE FROM extensiones.lectura_alerta WHERE empresa_id = {0};
        DELETE FROM extensiones.alerta WHERE empresa_id = {0};
        DELETE FROM extensiones.regla_alerta WHERE empresa_id = {0};
        DELETE FROM extensiones.valor_campo WHERE empresa_id = {0};
        DELETE FROM extensiones.definicion_campo WHERE empresa_id = {0};
        DELETE FROM extensiones.adjunto WHERE empresa_id = {0};
        DELETE FROM extensiones.plantilla_etiqueta WHERE empresa_id = {0};
        DELETE FROM extensiones.referencia_cliente WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExtensionesDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todas las extensiones de la empresa (solo dentro de <c>BorradoEmpresa</c>).</summary>
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

internal sealed class ConfiguracionDefinicionCampo : IEntityTypeConfiguration<DefinicionCampo>
{
    public void Configure(EntityTypeBuilder<DefinicionCampo> b)
    {
        Columnas.Base(b, "definicion_campo");
        b.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(40).IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(DefinicionCampo.LongitudCodigo).IsRequired();
        b.Property(x => x.Etiqueta).HasColumnName("etiqueta").HasMaxLength(DefinicionCampo.LongitudEtiqueta).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Obligatorio).HasColumnName("obligatorio").IsRequired();
        b.Property(x => x.ValorPorDefecto).HasColumnName("valor_por_defecto").HasMaxLength(DefinicionCampo.LongitudValor);
        b.Property(x => x.Ayuda).HasColumnName("ayuda").HasMaxLength(300);
        b.Property(x => x.Orden).HasColumnName("orden").IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Entidad, x.Codigo }).IsUnique().HasDatabaseName("ux_definicion_campo_codigo");
        b.OwnsMany(x => x.Opciones, o =>
        {
            o.ToTable("opcion_campo");
            o.WithOwner().HasForeignKey("definicion_campo_id");
            o.HasKey(x => x.Id);
            o.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            o.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            o.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(DefinicionCampo.LongitudEtiqueta).IsRequired();
            o.HasIndex("definicion_campo_id", nameof(OpcionCampo.Orden)).HasDatabaseName("ix_opcion_campo_orden");
        });
        b.Navigation(x => x.Opciones).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionValorCampo : IEntityTypeConfiguration<ValorCampo>
{
    public void Configure(EntityTypeBuilder<ValorCampo> b)
    {
        Columnas.Base(b, "valor_campo");
        b.Property(x => x.DefinicionId).HasColumnName("definicion_id").IsRequired();
        b.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(40).IsRequired();
        b.Property(x => x.EntidadId).HasColumnName("entidad_id").IsRequired();
        b.Property(x => x.Valor).HasColumnName("valor").HasMaxLength(DefinicionCampo.LongitudValor).IsRequired();
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.HasOne<DefinicionCampo>().WithMany().HasForeignKey(x => x.DefinicionId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_valor_campo_definicion");
        b.HasIndex(x => new { x.DefinicionId, x.EntidadId }).IsUnique().HasDatabaseName("ux_valor_campo_registro");
        b.HasIndex(x => new { x.Entidad, x.EntidadId }).HasDatabaseName("ix_valor_campo_entidad");
    }
}

internal sealed class ConfiguracionAdjunto : IEntityTypeConfiguration<Adjunto>
{
    public void Configure(EntityTypeBuilder<Adjunto> b)
    {
        Columnas.Base(b, "adjunto");
        b.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(40).IsRequired();
        b.Property(x => x.EntidadId).HasColumnName("entidad_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(Adjunto.LongitudNombre).IsRequired();
        b.Property(x => x.TipoMime).HasColumnName("tipo_mime").HasMaxLength(100).IsRequired();
        b.Property(x => x.Tamano).HasColumnName("tamano").IsRequired();
        b.Property(x => x.Huella).HasColumnName("huella").HasMaxLength(64).IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        b.Property(x => x.SubidoPorId).HasColumnName("subido_por_id");
        b.Property(x => x.SubidoPor).HasColumnName("subido_por").HasMaxLength(200);
        b.Property(x => x.SubidoEn).HasColumnName("subido_en").IsRequired();
        b.HasOne(x => x.Contenido).WithOne().HasForeignKey<ContenidoAdjunto>(x => x.AdjuntoId).OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_adjunto_contenido_adjunto");
        b.HasIndex(x => new { x.Entidad, x.EntidadId }).HasDatabaseName("ix_adjunto_entidad");
    }
}

internal sealed class ConfiguracionContenidoAdjunto : IEntityTypeConfiguration<ContenidoAdjunto>
{
    public void Configure(EntityTypeBuilder<ContenidoAdjunto> b)
    {
        b.ToTable("adjunto_contenido");
        b.HasKey(x => x.AdjuntoId);
        b.Property(x => x.AdjuntoId).HasColumnName("adjunto_id").ValueGeneratedNever();
        b.Property(x => x.Datos).HasColumnName("datos").IsRequired();
    }
}

internal sealed class ConfiguracionReglaAlerta : IEntityTypeConfiguration<ReglaAlerta>
{
    public void Configure(EntityTypeBuilder<ReglaAlerta> b)
    {
        Columnas.Base(b, "regla_alerta");
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Dias).HasColumnName("dias");
        b.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(7,2)");
        b.Property(x => x.CampoId).HasColumnName("campo_id");
        b.Property(x => x.Evento).HasColumnName("evento").HasMaxLength(60);
        b.Property(x => x.PermisoDestino).HasColumnName("permiso_destino").HasMaxLength(80).IsRequired();
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Property(x => x.UltimaEvaluacion).HasColumnName("ultima_evaluacion");
        b.HasOne<DefinicionCampo>().WithMany().HasForeignKey(x => x.CampoId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_regla_alerta_campo");
        b.HasIndex(x => x.CampoId).HasDatabaseName("ix_regla_alerta_campo");
    }
}

internal sealed class ConfiguracionAlerta : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> b)
    {
        Columnas.Base(b, "alerta");
        b.Property(x => x.ReglaId).HasColumnName("regla_id").IsRequired();
        b.Property(x => x.Clave).HasColumnName("clave").HasMaxLength(200).IsRequired();
        b.Property(x => x.Titulo).HasColumnName("titulo").HasMaxLength(200).IsRequired();
        b.Property(x => x.Detalle).HasColumnName("detalle").HasMaxLength(1000);
        b.Property(x => x.Entidad).HasColumnName("entidad").HasMaxLength(40);
        b.Property(x => x.EntidadId).HasColumnName("entidad_id");
        b.Property(x => x.PermisoDestino).HasColumnName("permiso_destino").HasMaxLength(80).IsRequired();
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.ResueltaEn).HasColumnName("resuelta_en");
        b.Property(x => x.ResueltaPor).HasColumnName("resuelta_por").HasMaxLength(200);
        b.Ignore(x => x.Viva);
        b.HasOne<ReglaAlerta>().WithMany().HasForeignKey(x => x.ReglaId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_alerta_regla");
        b.HasIndex(x => new { x.ReglaId, x.Clave }).IsUnique().HasFilter("resuelta_en IS NULL").HasDatabaseName("ux_alerta_viva_clave");
        b.HasIndex(x => new { x.EmpresaId, x.CreadaEn }).HasDatabaseName("ix_alerta_empresa_fecha");
    }
}

internal sealed class ConfiguracionLecturaAlerta : IEntityTypeConfiguration<LecturaAlerta>
{
    public void Configure(EntityTypeBuilder<LecturaAlerta> b)
    {
        Columnas.Base(b, "lectura_alerta");
        b.Property(x => x.AlertaId).HasColumnName("alerta_id").IsRequired();
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.LeidaEn).HasColumnName("leida_en").IsRequired();
        b.HasOne<Alerta>().WithMany().HasForeignKey(x => x.AlertaId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("fk_lectura_alerta_alerta");
        b.HasIndex(x => new { x.AlertaId, x.UsuarioId }).IsUnique().HasDatabaseName("ux_lectura_alerta_usuario");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_lectura_alerta_usuario");
    }
}

internal sealed class RepositorioExtensiones : IRepositorioExtensiones
{
    private readonly ExtensionesDbContext _ctx;

    public RepositorioExtensiones(ExtensionesDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);

    public async Task<IReadOnlyList<DefinicionCampo>> CamposAsync(Guid empresaId, string? entidad, CancellationToken ct = default)
    {
        var codigo = EntidadesExtensibles.Buscar(entidad)?.Codigo ?? entidad?.Trim();
        return await _ctx.Set<DefinicionCampo>().Where(c => c.EmpresaId == empresaId && (codigo == null || c.Entidad == codigo)).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task<DefinicionCampo?> CampoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<DefinicionCampo>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ValorCampo>> ValoresAsync(string entidad, Guid entidadId, CancellationToken ct = default) =>
        await _ctx.Set<ValorCampo>().Where(v => v.Entidad == entidad && v.EntidadId == entidadId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ValorCampo>> ValoresDeCampoAsync(Guid definicionId, CancellationToken ct = default) =>
        await _ctx.Set<ValorCampo>().AsNoTracking().Where(v => v.DefinicionId == definicionId).Take(20000).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> CampoConValoresAsync(Guid definicionId, CancellationToken ct = default) =>
        _ctx.Set<ValorCampo>().AnyAsync(v => v.DefinicionId == definicionId, ct);

    public async Task<IReadOnlyList<Adjunto>> AdjuntosAsync(string entidad, Guid entidadId, CancellationToken ct = default) =>
        await _ctx.Set<Adjunto>().AsNoTracking().Where(a => a.Entidad == entidad && a.EntidadId == entidadId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Adjunto?> AdjuntoAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Set<Adjunto>().Include(a => a.Contenido).FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyDictionary<Guid, int>> CuentaAdjuntosAsync(string entidad, IReadOnlyCollection<Guid> entidadIds, CancellationToken ct = default)
    {
        var ids = entidadIds.Distinct().ToList();
        return await _ctx.Set<Adjunto>().Where(a => a.Entidad == entidad && ids.Contains(a.EntidadId)).GroupBy(a => a.EntidadId)
            .Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N, ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ReglaAlerta>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ReglaAlerta>().Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ReglaAlerta?> ReglaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ReglaAlerta>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Alerta>> AlertasVivasAsync(Guid empresaId, Guid? reglaId, CancellationToken ct = default) =>
        await _ctx.Set<Alerta>().Where(a => a.EmpresaId == empresaId && a.ResueltaEn == null && (reglaId == null || a.ReglaId == reglaId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Alerta>> AlertasAsync(Guid empresaId, bool incluirResueltas, int maximo, CancellationToken ct = default) =>
        await _ctx.Set<Alerta>().AsNoTracking().Where(a => a.EmpresaId == empresaId && (incluirResueltas || a.ResueltaEn == null)).OrderByDescending(a => a.CreadaEn)
            .Take(maximo).ToListAsync(ct).ConfigureAwait(false);

    public Task<Alerta?> AlertaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Alerta>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> AlertaConClaveAsync(Guid reglaId, string clave, CancellationToken ct = default) =>
        _ctx.Set<Alerta>().AnyAsync(a => a.ReglaId == reglaId && a.Clave == clave, ct);

    public async Task<IReadOnlySet<Guid>> LeidasAsync(Guid usuarioId, IReadOnlyCollection<Guid> alertaIds, CancellationToken ct = default)
    {
        var ids = alertaIds.Distinct().ToList();
        return (await _ctx.Set<LecturaAlerta>().Where(l => l.UsuarioId == usuarioId && ids.Contains(l.AlertaId)).Select(l => l.AlertaId).ToListAsync(ct).ConfigureAwait(false))
            .ToHashSet();
    }

    public Task<bool> ReglaConAlertasAsync(Guid reglaId, CancellationToken ct = default) => _ctx.Set<Alerta>().AnyAsync(a => a.ReglaId == reglaId, ct);
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class ExtensionesDbContextFactory : IDesignTimeDbContextFactory<ExtensionesDbContext>
{
    public ExtensionesDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<ExtensionesDbContext>()
            .UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", ExtensionesDbContext.Esquema)).Options;
        return new ExtensionesDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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

internal sealed class ConfiguracionPlantillaEtiqueta : IEntityTypeConfiguration<PlantillaEtiqueta>
{
    public void Configure(EntityTypeBuilder<PlantillaEtiqueta> b)
    {
        Columnas.Base(b, "plantilla_etiqueta");
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.Marca).HasColumnName("marca").HasMaxLength(60);
        b.Property(x => x.Campos).HasColumnName("campos").HasMaxLength(400).IsRequired();
        b.Property(x => x.TextoLibre).HasColumnName("texto_libre").HasMaxLength(300);
        Columnas.Enum(b.Property(x => x.Formato), "formato");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Ignore(x => x.ListaCampos);
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_plantilla_etiqueta_cliente");
    }
}

internal sealed class ConfiguracionReferenciaCliente : IEntityTypeConfiguration<ReferenciaCliente>
{
    public void Configure(EntityTypeBuilder<ReferenciaCliente> b)
    {
        Columnas.Base(b, "referencia_cliente");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(120);
        b.Property(x => x.Gtin).HasColumnName("gtin").HasMaxLength(14);
        b.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.ProductoId }).IsUnique().HasDatabaseName("ux_referencia_cliente_producto");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_referencia_cliente_producto");
    }
}

internal sealed class RepositorioEtiquetas : IRepositorioEtiquetas
{
    private readonly ExtensionesDbContext _ctx;

    public RepositorioEtiquetas(ExtensionesDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);

    public async Task<IReadOnlyList<PlantillaEtiqueta>> PlantillasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<PlantillaEtiqueta>().Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PlantillaEtiqueta?> PlantillaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PlantillaEtiqueta>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<ReferenciaCliente>> ReferenciasAsync(Guid empresaId, Guid? clienteId, CancellationToken ct = default) =>
        await _ctx.Set<ReferenciaCliente>().Where(r => r.EmpresaId == empresaId && (clienteId == null || r.ClienteId == clienteId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<ReferenciaCliente?> ReferenciaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ReferenciaCliente>().FirstOrDefaultAsync(r => r.Id == id, ct);
}
