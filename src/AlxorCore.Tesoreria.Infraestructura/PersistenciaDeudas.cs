using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Tesoreria.Infraestructura;

internal sealed class ConfiguracionConfiguracionCartera : IEntityTypeConfiguration<ConfiguracionCartera>
{
    public void Configure(EntityTypeBuilder<ConfiguracionCartera> b)
    {
        b.ToTable("configuracion_cartera");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ImpagadosA4315).HasColumnName("impagados_a_4315").IsRequired();
        b.Property(x => x.PorcentajeRenovacion).HasColumnName("porcentaje_renovacion").HasColumnType("numeric(5,2)").IsRequired();
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_cartera_empresa");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionSituacionDeuda : IEntityTypeConfiguration<SituacionDeuda>
{
    public void Configure(EntityTypeBuilder<SituacionDeuda> b)
    {
        b.ToTable("situacion_deuda");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.DocumentoId).HasColumnName("documento_id").IsRequired();
        b.Property(x => x.Documento).HasColumnName("documento").HasMaxLength(200).IsRequired();
        b.Property(x => x.TerceroId).HasColumnName("tercero_id");
        b.Property(x => x.TerceroNombre).HasColumnName("tercero_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.CuentaOrigen).HasColumnName("cuenta_origen").HasMaxLength(12);
        b.Property(x => x.Cuenta).HasColumnName("cuenta").HasMaxLength(12);
        b.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Clasificacion).HasColumnName("clasificacion").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.FechaClasificacion).HasColumnName("fecha_clasificacion");
        b.Property(x => x.Dotado).HasColumnName("dotado").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.IncobrableEl).HasColumnName("incobrable_el");
        b.HasIndex(x => new { x.EmpresaId, x.TipoDocumento, x.DocumentoId }).IsUnique().HasDatabaseName("ux_situacion_deuda_documento");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionRegularizacionDeuda : IEntityTypeConfiguration<RegularizacionDeuda>
{
    public void Configure(EntityTypeBuilder<RegularizacionDeuda> b)
    {
        b.ToTable("regularizacion_deuda");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.SituacionId).HasColumnName("situacion_id").IsRequired();
        b.Property(x => x.MovimientoId).HasColumnName("movimiento_id").IsRequired();
        b.Property(x => x.Cuenta).HasColumnName("cuenta").HasMaxLength(12).IsRequired();
        b.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Dotacion).HasColumnName("dotacion").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Deshecha).HasColumnName("deshecha").IsRequired();
        b.HasIndex(x => x.MovimientoId).IsUnique().HasDatabaseName("ux_regularizacion_deuda_movimiento");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionRenovacionEfecto : IEntityTypeConfiguration<RenovacionEfecto>
{
    public void Configure(EntityTypeBuilder<RenovacionEfecto> b)
    {
        b.ToTable("renovacion_efecto");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.DocumentoId).HasColumnName("documento_id").IsRequired();
        b.Property(x => x.Documento).HasColumnName("documento").HasMaxLength(200).IsRequired();
        b.Property(x => x.TerceroId).HasColumnName("tercero_id");
        b.Property(x => x.TerceroNombre).HasColumnName("tercero_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Gastos).HasColumnName("gastos").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.MovimientoId).HasColumnName("movimiento_id").IsRequired();
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.AnuladaEn).HasColumnName("anulada_en");
        b.HasIndex(x => new { x.EmpresaId, x.TipoDocumento, x.DocumentoId }).HasDatabaseName("ix_renovacion_efecto_documento");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioDeudas : IRepositorioDeudas
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioDeudas(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public Task<ConfiguracionCartera?> ConfiguracionAsync(CancellationToken ct = default) => _ctx.Set<ConfiguracionCartera>().FirstOrDefaultAsync(ct);

    public async Task<SituacionDeuda?> SituacionAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default) =>
        _ctx.Set<SituacionDeuda>().Local.FirstOrDefault(s => s.TipoDocumento == tipo && s.DocumentoId == documentoId)
        ?? await _ctx.Set<SituacionDeuda>().FirstOrDefaultAsync(s => s.TipoDocumento == tipo && s.DocumentoId == documentoId, ct).ConfigureAwait(false);

    public Task<SituacionDeuda?> SituacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<SituacionDeuda>().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SituacionDeuda>> SituacionesAsync(CancellationToken ct = default) =>
        await _ctx.Set<SituacionDeuda>().AsNoTracking().Where(s => s.Cuenta != null || s.IncobrableEl != null).OrderBy(s => s.TerceroNombre)
            .ToListAsync(ct).ConfigureAwait(false);

    public Task<RegularizacionDeuda?> RegularizacionDeAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Set<RegularizacionDeuda>().FirstOrDefaultAsync(r => r.MovimientoId == movimientoId && !r.Deshecha, ct);

    public Task<RenovacionEfecto?> RenovacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<RenovacionEfecto>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<RenovacionEfecto>> RenovacionesAsync(CancellationToken ct = default) =>
        await _ctx.Set<RenovacionEfecto>().AsNoTracking().OrderByDescending(r => r.Fecha).ThenByDescending(r => r.CreadaEn).Take(500).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> RegularizoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Set<RegularizacionDeuda>().AnyAsync(r => r.MovimientoId == movimientoId, ct);

    public Task<bool> EsMovimientoDeRenovacionAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Set<RenovacionEfecto>().AnyAsync(r => r.MovimientoId == movimientoId && r.AnuladaEn == null, ct);
}
