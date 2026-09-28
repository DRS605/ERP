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
