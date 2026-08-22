using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>Mapeo EF Core del inmovilizado y sus colecciones (dotaciones y ajustes fiscales).</summary>
internal sealed class ConfiguracionInmovilizado : IEntityTypeConfiguration<Inmovilizado>
{
    public void Configure(EntityTypeBuilder<Inmovilizado> builder)
    {
        builder.ToTable("inmovilizado");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(i => i.Codigo).HasColumnName("codigo").HasMaxLength(Inmovilizado.LongitudMaximaCodigo).IsRequired();
        builder.Property(i => i.Descripcion).HasColumnName("descripcion").HasMaxLength(Inmovilizado.LongitudMaximaDescripcion).IsRequired();
        builder.Property(i => i.CuentaActivo).HasColumnName("cuenta_activo").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(i => i.CuentaAmortizacion).HasColumnName("cuenta_amortizacion").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(i => i.CuentaDotacion).HasColumnName("cuenta_dotacion").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(i => i.FechaAdquisicion).HasColumnName("fecha_adquisicion").IsRequired();
        builder.Property(i => i.FechaAlta).HasColumnName("fecha_alta").IsRequired();
        builder.Property(i => i.ValorAdquisicion).HasColumnName("valor_adquisicion").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(i => i.ValorResidual).HasColumnName("valor_residual").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(i => i.Periodicidad).HasColumnName("periodicidad").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(i => i.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(i => i.FechaBaja).HasColumnName("fecha_baja");
        builder.Property(i => i.ValorEnajenacion).HasColumnName("valor_enajenacion").HasColumnType("numeric(14,2)");

        builder.OwnsOne(i => i.Contable, plan =>
        {
            plan.Property(p => p.Metodo).HasColumnName("metodo_contable").HasMaxLength(20).HasConversion<string>().IsRequired();
            plan.Property(p => p.VidaUtilAnios).HasColumnName("vida_util_contable").IsRequired();
            plan.Property(p => p.PorcentajeDegresivo).HasColumnName("porcentaje_degresivo_contable").HasColumnType("numeric(6,2)").IsRequired();
        });
        builder.Navigation(i => i.Contable).IsRequired();

        builder.OwnsOne(i => i.Fiscal, plan =>
        {
            plan.Property(p => p.Metodo).HasColumnName("metodo_fiscal").HasMaxLength(20).HasConversion<string>().IsRequired();
            plan.Property(p => p.VidaUtilAnios).HasColumnName("vida_util_fiscal").IsRequired();
            plan.Property(p => p.PorcentajeDegresivo).HasColumnName("porcentaje_degresivo_fiscal").HasColumnType("numeric(6,2)").IsRequired();
        });
        builder.Navigation(i => i.Fiscal).IsRequired();

        builder.OwnsMany(i => i.Dotaciones, d =>
        {
            d.ToTable("dotacion_amortizacion");
            d.WithOwner().HasForeignKey("inmovilizado_id");
            d.HasKey(x => x.Id);
            d.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            d.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
            d.Property(x => x.Mes).HasColumnName("mes").IsRequired();
            d.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            d.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            d.Property(x => x.AsientoId).HasColumnName("asiento_id").IsRequired();
        });

        builder.OwnsMany(i => i.AjustesFiscales, a =>
        {
            a.ToTable("ajuste_fiscal_amortizacion");
            a.WithOwner().HasForeignKey("inmovilizado_id");
            a.HasKey(x => x.Id);
            a.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            a.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
            a.Property(x => x.DiferenciaTemporaria).HasColumnName("diferencia_temporaria").HasColumnType("numeric(14,2)").IsRequired();
            a.Property(x => x.TipoImpositivo).HasColumnName("tipo_impositivo").HasColumnType("numeric(6,4)").IsRequired();
            a.Property(x => x.AsientoId).HasColumnName("asiento_id").IsRequired();
        });

        builder.HasIndex(i => new { i.EmpresaId, i.Codigo }).IsUnique().HasDatabaseName("ux_inmovilizado_empresa_codigo");
        builder.Ignore(i => i.EventosDominio);
    }
}

internal sealed class RepositorioInmovilizado : IRepositorioInmovilizado
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioInmovilizado(ContabilidadDbContext contexto) => _contexto = contexto;

    public void Agregar(Inmovilizado inmovilizado) => _contexto.Inmovilizados.Add(inmovilizado);

    public Task<Inmovilizado?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct = default) =>
        _contexto.Inmovilizados.FirstOrDefaultAsync(i => i.Id == id && i.EmpresaId == empresaId, ct);

    public async Task<IReadOnlyList<Inmovilizado>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Inmovilizados.AsNoTracking()
            .Where(i => i.EmpresaId == empresaId).OrderBy(i => i.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Inmovilizado>> ListarActivosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Inmovilizados
            .Where(i => i.EmpresaId == empresaId && i.Estado == EstadoInmovilizado.Activo)
            .OrderBy(i => i.Codigo).ToListAsync(ct).ConfigureAwait(false);
}
