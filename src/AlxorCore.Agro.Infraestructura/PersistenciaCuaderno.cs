using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionTratamientoParcela : IEntityTypeConfiguration<TratamientoParcela>
{
    public void Configure(EntityTypeBuilder<TratamientoParcela> b)
    {
        Columnas.Base(b, "tratamiento_parcela");
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo").HasDefaultValue(TipoLabor.Fitosanitario);
        b.Property(x => x.NitrogenoKgHa).HasColumnName("nitrogeno_kg_ha").HasColumnType("numeric(10,3)");
        b.Property(x => x.FosforoKgHa).HasColumnName("fosforo_kg_ha").HasColumnType("numeric(10,3)");
        b.Property(x => x.PotasioKgHa).HasColumnName("potasio_kg_ha").HasColumnType("numeric(10,3)");
        b.Property(x => x.VolumenM3).HasColumnName("volumen_m3").HasColumnType("numeric(12,3)");
        b.Property(x => x.Producto).HasColumnName("producto").HasMaxLength(TratamientoParcela.LongitudTexto).IsRequired();
        b.Property(x => x.NumeroRegistro).HasColumnName("numero_registro").HasMaxLength(30);
        b.Property(x => x.MateriaActiva).HasColumnName("materia_activa").HasMaxLength(TratamientoParcela.LongitudTexto);
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(TratamientoParcela.LongitudTexto);
        b.Property(x => x.Dosis).HasColumnName("dosis").HasColumnType("numeric(12,4)");
        b.Property(x => x.UnidadDosis).HasColumnName("unidad_dosis").HasMaxLength(20);
        b.Property(x => x.SuperficieTratadaHa).HasColumnName("superficie_tratada_ha").HasColumnType("numeric(10,4)");
        b.Property(x => x.PlazoSeguridadDias).HasColumnName("plazo_seguridad_dias").IsRequired();
        b.Property(x => x.Aplicador).HasColumnName("aplicador").HasMaxLength(TratamientoParcela.LongitudTexto);
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        b.Property(x => x.Anulado).HasColumnName("anulado").IsRequired();
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.RecolectableDesde);
        b.Property(x => x.FitosanitarioId).HasColumnName("fitosanitario_id");
        b.Property(x => x.Cultivo).HasColumnName("cultivo").HasMaxLength(TratamientoParcela.LongitudTexto);
        b.Property(x => x.ArticuloId).HasColumnName("articulo_id");
        b.Property(x => x.AlmacenId).HasColumnName("almacen_id");
        b.Property(x => x.Lote).HasColumnName("lote").HasMaxLength(60);
        b.Property(x => x.CantidadConsumida).HasColumnName("cantidad_consumida").HasColumnType("numeric(14,4)");
        b.HasIndex(x => x.FitosanitarioId).HasDatabaseName("ix_tratamiento_parcela_fitosanitario");
        b.HasIndex(x => new { x.ArticuloId, x.Lote }).HasDatabaseName("ix_tratamiento_parcela_lote");
        b.HasIndex(x => new { x.ParcelaId, x.Fecha }).HasDatabaseName("ix_tratamiento_parcela_fecha");
    }
}

internal sealed class RepositorioCuaderno : IRepositorioCuaderno
{
    private readonly AgroDbContext _ctx;

    public RepositorioCuaderno(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(TratamientoParcela tratamiento) => _ctx.Add(tratamiento);

    public Task<TratamientoParcela?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Set<TratamientoParcela>().FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<TratamientoParcela>> ListarAsync(Guid empresaId, IReadOnlyCollection<Guid>? parcelas, DateOnly? desde, DateOnly? hasta,
        CancellationToken ct = default)
    {
        var q = _ctx.Set<TratamientoParcela>().AsNoTracking().Where(t => t.EmpresaId == empresaId);
        if (parcelas is not null)
        {
            q = q.Where(t => parcelas.Contains(t.ParcelaId));
        }

        if (desde is { } d)
        {
            q = q.Where(t => t.Fecha >= d);
        }

        if (hasta is { } h)
        {
            q = q.Where(t => t.Fecha <= h);
        }

        return await q.OrderBy(t => t.Fecha).ThenBy(t => t.CreadoEn).ToListAsync(ct).ConfigureAwait(false);
    }
}

internal sealed class ConfiguracionProductoFitosanitario : IEntityTypeConfiguration<ProductoFitosanitario>
{
    public void Configure(EntityTypeBuilder<ProductoFitosanitario> b)
    {
        Columnas.Base(b, "fitosanitario");
        b.Property(x => x.NumeroRegistro).HasColumnName("numero_registro").HasMaxLength(ProductoFitosanitario.LongitudNumero).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ProductoFitosanitario.LongitudTexto).IsRequired();
        b.Property(x => x.Titular).HasColumnName("titular").HasMaxLength(ProductoFitosanitario.LongitudTexto);
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.FechaCaducidad).HasColumnName("fecha_caducidad");
        b.Property(x => x.FechaLimiteVenta).HasColumnName("fecha_limite_venta");
        b.Property(x => x.FechaLimiteUso).HasColumnName("fecha_limite_uso");
        b.Property(x => x.FechaCancelacion).HasColumnName("fecha_cancelacion");
        b.Property(x => x.Formulado).HasColumnName("formulado").HasMaxLength(ProductoFitosanitario.LongitudTexto);
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.NumeroRegistro }).IsUnique().HasDatabaseName("ux_fitosanitario_numero");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_fitosanitario_producto");
        b.OwnsMany(x => x.MateriasActivas, m =>
        {
            m.ToTable("fitosanitario_materia_activa");
            m.WithOwner().HasForeignKey("fitosanitario_id");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            m.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ProductoFitosanitario.LongitudTexto).IsRequired();
            m.Property(x => x.Riqueza).HasColumnName("riqueza").HasMaxLength(60);
        });
        b.Navigation(x => x.MateriasActivas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.OwnsMany(x => x.Usos, u =>
        {
            u.ToTable("fitosanitario_uso");
            u.WithOwner().HasForeignKey("fitosanitario_id");
            u.HasKey(x => x.Id);
            u.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            u.Property(x => x.Cultivo).HasColumnName("cultivo").HasMaxLength(ProductoFitosanitario.LongitudTexto).IsRequired();
            u.Property(x => x.Plaga).HasColumnName("plaga").HasMaxLength(ProductoFitosanitario.LongitudTexto).IsRequired();
            u.Property(x => x.DosisMinima).HasColumnName("dosis_minima").HasColumnType("numeric(12,4)");
            u.Property(x => x.DosisMaxima).HasColumnName("dosis_maxima").HasColumnType("numeric(12,4)");
            u.Property(x => x.UnidadDosis).HasColumnName("unidad_dosis").HasMaxLength(20);
            u.Property(x => x.PlazoSeguridadDias).HasColumnName("plazo_seguridad_dias");
            u.Property(x => x.Aplicaciones).HasColumnName("aplicaciones");
            u.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        });
        b.Navigation(x => x.Usos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionCambioFitosanitario : IEntityTypeConfiguration<CambioFitosanitario>
{
    public void Configure(EntityTypeBuilder<CambioFitosanitario> b)
    {
        Columnas.Base(b, "cambio_fitosanitario");
        b.Property(x => x.FitosanitarioId).HasColumnName("fitosanitario_id").IsRequired();
        b.Property(x => x.NumeroRegistro).HasColumnName("numero_registro").HasMaxLength(ProductoFitosanitario.LongitudNumero).IsRequired();
        b.Property(x => x.Producto).HasColumnName("producto").HasMaxLength(ProductoFitosanitario.LongitudTexto).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Detalle).HasColumnName("detalle").HasMaxLength(1000).IsRequired();
        b.Property(x => x.DetectadoEn).HasColumnName("detectado_en").IsRequired();
        b.Property(x => x.Revisado).HasColumnName("revisado").IsRequired();
        b.Ignore(x => x.Restrictivo);
        b.HasIndex(x => x.FitosanitarioId).HasDatabaseName("ix_cambio_fitosanitario_producto");
        b.HasIndex(x => new { x.EmpresaId, x.DetectadoEn }).HasDatabaseName("ix_cambio_fitosanitario_fecha");
    }
}

internal sealed class RepositorioFitosanitarios : IRepositorioFitosanitarios
{
    private readonly AgroDbContext _ctx;

    public RepositorioFitosanitarios(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(ProductoFitosanitario producto) => _ctx.Add(producto);

    public void Agregar(CambioFitosanitario cambio) => _ctx.Add(cambio);

    public void Eliminar(ProductoFitosanitario producto) => _ctx.Remove(producto);

    public Task<ProductoFitosanitario?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ProductoFitosanitario>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<ProductoFitosanitario>> ListarAsync(Guid empresaId, string? buscar, CancellationToken ct = default)
    {
        var q = _ctx.Set<ProductoFitosanitario>().AsNoTracking().Where(p => p.EmpresaId == empresaId);
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var t = $"%{buscar.Trim()}%";
            q = q.Where(p => EF.Functions.ILike(p.Nombre, t) || EF.Functions.ILike(p.NumeroRegistro, t) || p.MateriasActivas.Any(m => EF.Functions.ILike(m.Nombre, t)));
        }

        return await q.OrderBy(p => p.Nombre).Take(500).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ProductoFitosanitario>> TodosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ProductoFitosanitario>().Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<CambioFitosanitario?> CambioAsync(Guid id, CancellationToken ct = default) => _ctx.Set<CambioFitosanitario>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CambioFitosanitario>> CambiosAsync(Guid empresaId, DateOnly? desde, bool soloPendientes, CancellationToken ct = default)
    {
        var q = _ctx.Set<CambioFitosanitario>().AsNoTracking().Where(c => c.EmpresaId == empresaId && (!soloPendientes || !c.Revisado));
        if (desde is { } d)
        {
            var inicio = new DateTimeOffset(d.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            q = q.Where(c => c.DetectadoEn >= inicio);
        }

        return await q.OrderByDescending(c => c.DetectadoEn).ThenBy(c => c.NumeroRegistro).Take(1000).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task<bool> UsadoAsync(Guid fitosanitarioId, CancellationToken ct = default) =>
        _ctx.Set<TratamientoParcela>().AnyAsync(t => t.FitosanitarioId == fitosanitarioId, ct);
}
