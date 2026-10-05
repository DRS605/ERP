using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionSesionSubasta : IEntityTypeConfiguration<SesionSubasta>
{
    public void Configure(EntityTypeBuilder<SesionSubasta> b)
    {
        Columnas.Base(b, "sesion_subasta");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(SesionSubasta.LongitudObservaciones);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.CerradaEn).HasColumnName("cerrada_en");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_sesion_subasta_numero");
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_sesion_subasta_fecha");

        b.OwnsMany(x => x.Lotes, l =>
        {
            l.ToTable("lote_subasta");
            l.WithOwner().HasForeignKey("sesion_subasta_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            l.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
            l.Property(x => x.PartidaCodigo).HasColumnName("partida_codigo").HasMaxLength(40);
            l.Property(x => x.AgricultorId).HasColumnName("agricultor_id");
            l.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(LoteSubasta.LongitudDescripcion).IsRequired();
            l.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            l.Property(x => x.Envases).HasColumnName("envases").IsRequired();
            l.Property(x => x.PrecioSalida).HasColumnName("precio_salida").HasColumnType(Columnas.PrecioKg);
            l.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(30).HasConversion<string>().IsRequired();
            l.Property(x => x.CompradorId).HasColumnName("comprador_id");
            l.Property(x => x.CompradorNombre).HasColumnName("comprador_nombre").HasMaxLength(200);
            l.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg);
            l.Property(x => x.Importe).HasColumnName("importe").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.AdjudicadoEn).HasColumnName("adjudicado_en");
            l.Property(x => x.AlbaranId).HasColumnName("albaran_id");
            l.Property(x => x.AlbaranNumero).HasColumnName("albaran_numero").HasMaxLength(40);
            l.HasIndex("sesion_subasta_id", nameof(LoteSubasta.Orden)).IsUnique().HasDatabaseName("ux_lote_subasta_orden");
            l.HasIndex(x => x.PartidaId).HasDatabaseName("ix_lote_subasta_partida");
            l.HasIndex(x => x.CompradorId).HasDatabaseName("ix_lote_subasta_comprador");
        });
        b.Navigation(x => x.Lotes).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(x => x.Pujas, p =>
        {
            p.ToTable("puja_subasta");
            p.WithOwner().HasForeignKey("sesion_subasta_id");
            p.HasKey(x => x.Id);
            p.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            p.Property(x => x.LoteId).HasColumnName("lote_id").IsRequired();
            p.Property(x => x.CompradorId).HasColumnName("comprador_id").IsRequired();
            p.Property(x => x.CompradorNombre).HasColumnName("comprador_nombre").HasMaxLength(200).IsRequired();
            p.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg).IsRequired();
            p.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
            p.HasIndex(x => x.LoteId).HasDatabaseName("ix_puja_subasta_lote");
        });
        b.Navigation(x => x.Pujas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RepositorioSubastas : IRepositorioSubastas
{
    private readonly AgroDbContext _ctx;

    public RepositorioSubastas(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(SesionSubasta sesion) => _ctx.Add(sesion);

    public Task<SesionSubasta?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Set<SesionSubasta>().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SesionSubasta>> ListarAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _ctx.Set<SesionSubasta>().AsNoTracking()
            .Where(s => s.EmpresaId == empresaId && (desde == null || s.Fecha >= desde) && (hasta == null || s.Fecha <= hasta))
            .OrderByDescending(s => s.Fecha).ThenByDescending(s => s.Numero).Take(1000).ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<SesionSubasta>().Where(s => s.EmpresaId == empresaId && s.Ejercicio == ejercicio).Select(s => (int?)s.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;

    public async Task<IReadOnlyList<SesionSubasta>> ConPartidasAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        partidaIds.Count == 0 ? []
            : await _ctx.Set<SesionSubasta>().AsNoTracking()
                .Where(s => s.Estado != EstadoSesionSubasta.Anulada && s.Lotes.Any(l => partidaIds.Contains(l.PartidaId)))
                .ToListAsync(ct).ConfigureAwait(false);
}
