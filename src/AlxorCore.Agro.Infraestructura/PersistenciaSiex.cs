using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionExplotacionSiex : IEntityTypeConfiguration<ExplotacionSiex>
{
    public void Configure(EntityTypeBuilder<ExplotacionSiex> b)
    {
        Columnas.Base(b, "explotacion_siex");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.CodigoRegepa).HasColumnName("codigo_regepa").HasMaxLength(20);
        b.Property(x => x.AsesorNombre).HasColumnName("asesor_nombre").HasMaxLength(150);
        b.Property(x => x.AsesorRopo).HasColumnName("asesor_ropo").HasMaxLength(30);
        b.Property(x => x.CarneAplicador).HasColumnName("carne_aplicador").HasMaxLength(30);
        b.Property(x => x.EquipoRoma).HasColumnName("equipo_roma").HasMaxLength(30);
        b.Property(x => x.PlanAbonadoObligatorio).HasColumnName("plan_abonado_obligatorio").IsRequired();
        b.HasIndex(x => x.AgricultorId).IsUnique().HasDatabaseName("ux_explotacion_siex_agricultor");
    }
}

internal sealed class ConfiguracionAnalisisAgro : IEntityTypeConfiguration<AnalisisAgro>
{
    public void Configure(EntityTypeBuilder<AnalisisAgro> b)
    {
        Columnas.Base(b, "analisis_agro");
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Laboratorio).HasColumnName("laboratorio").HasMaxLength(150).IsRequired();
        b.Property(x => x.Boletin).HasColumnName("boletin").HasMaxLength(60);
        b.Property(x => x.NitrogenoKgHa).HasColumnName("nitrogeno_kg_ha").HasColumnType("numeric(10,3)");
        b.Property(x => x.MateriaOrganicaPct).HasColumnName("materia_organica_pct").HasColumnType("numeric(6,3)");
        b.Property(x => x.Ph).HasColumnName("ph").HasColumnType("numeric(4,2)");
        b.Property(x => x.SuperaLimites).HasColumnName("supera_limites");
        b.Property(x => x.Conclusion).HasColumnName("conclusion").HasMaxLength(2000);
        b.HasIndex(x => new { x.ParcelaId, x.Fecha }).HasDatabaseName("ix_analisis_agro_parcela");
    }
}

internal sealed class ConfiguracionPlanAbonado : IEntityTypeConfiguration<PlanAbonado>
{
    public void Configure(EntityTypeBuilder<PlanAbonado> b)
    {
        Columnas.Base(b, "plan_abonado");
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id").IsRequired();
        b.Property(x => x.Anio).HasColumnName("anio").IsRequired();
        b.Property(x => x.ProduccionEsperadaKgHa).HasColumnName("produccion_esperada_kg_ha").HasColumnType("numeric(12,3)");
        b.Property(x => x.NitrogenoKgHa).HasColumnName("nitrogeno_kg_ha").HasColumnType("numeric(10,3)").IsRequired();
        b.Property(x => x.FosforoKgHa).HasColumnName("fosforo_kg_ha").HasColumnType("numeric(10,3)").IsRequired();
        b.Property(x => x.PotasioKgHa).HasColumnName("potasio_kg_ha").HasColumnType("numeric(10,3)").IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        b.HasIndex(x => new { x.ParcelaId, x.Anio }).IsUnique().HasDatabaseName("ux_plan_abonado_parcela_anio");
    }
}

internal sealed class RepositorioSiex : IRepositorioSiex
{
    private readonly AgroDbContext _ctx;

    public RepositorioSiex(AgroDbContext ctx) => _ctx = ctx;

    public Task<ExplotacionSiex?> ExplotacionAsync(Guid agricultorId, CancellationToken ct = default) =>
        _ctx.Set<ExplotacionSiex>().FirstOrDefaultAsync(e => e.AgricultorId == agricultorId, ct);

    public void Agregar(ExplotacionSiex explotacion) => _ctx.Add(explotacion);

    public async Task<IReadOnlyList<AnalisisAgro>> AnalisisAsync(Guid empresaId, IReadOnlyCollection<Guid> parcelas, CancellationToken ct = default) =>
        await _ctx.Set<AnalisisAgro>().AsNoTracking().Where(a => a.EmpresaId == empresaId && parcelas.Contains(a.ParcelaId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<AnalisisAgro?> AnalisisPorIdAsync(Guid id, CancellationToken ct = default) => _ctx.Set<AnalisisAgro>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public void Agregar(AnalisisAgro analisis) => _ctx.Add(analisis);

    public void Eliminar(AnalisisAgro analisis) => _ctx.Remove(analisis);

    public async Task<IReadOnlyList<PlanAbonado>> PlanesAbonadoAsync(Guid empresaId, IReadOnlyCollection<Guid> parcelas, int? anio, CancellationToken ct = default) =>
        await _ctx.Set<PlanAbonado>().AsNoTracking().Where(p => p.EmpresaId == empresaId && parcelas.Contains(p.ParcelaId) && (anio == null || p.Anio == anio))
            .ToListAsync(ct).ConfigureAwait(false);

    public Task<PlanAbonado?> PlanAbonadoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PlanAbonado>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PlanAbonado plan) => _ctx.Add(plan);

    public void Eliminar(PlanAbonado plan) => _ctx.Remove(plan);
}
