using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Tesoreria.Infraestructura;

internal sealed class ConfiguracionPolizaSeguro : IEntityTypeConfiguration<PolizaSeguroCredito>
{
    public void Configure(EntityTypeBuilder<PolizaSeguroCredito> b)
    {
        b.ToTable("poliza_seguro");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Aseguradora).HasColumnName("aseguradora").HasMaxLength(120).IsRequired();
        b.Property(x => x.NumeroPoliza).HasColumnName("numero_poliza").HasMaxLength(40).IsRequired();
        b.Property(x => x.PorcentajeCobertura).HasColumnName("porcentaje_cobertura").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.PlazoAvisoDias).HasColumnName("plazo_aviso_dias").IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta");
        b.Property(x => x.BloquearSinCobertura).HasColumnName("bloquear_sin_cobertura").IsRequired();
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionClasificacionSeguro : IEntityTypeConfiguration<ClasificacionSeguro>
{
    public void Configure(EntityTypeBuilder<ClasificacionSeguro> b)
    {
        b.ToTable("clasificacion_seguro");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.PolizaId).HasColumnName("poliza_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(40);
        b.Property(x => x.Solicitado).HasColumnName("solicitado").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Concedido).HasColumnName("concedido").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.Efecto).HasColumnName("efecto").IsRequired();
        b.Property(x => x.Vence).HasColumnName("vence");
        b.HasOne<PolizaSeguroCredito>().WithMany().HasForeignKey(x => x.PolizaId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_clasificacion_poliza");
        b.HasIndex(x => new { x.PolizaId, x.ClienteId }).IsUnique().HasDatabaseName("ux_clasificacion_poliza_cliente");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_clasificacion_cliente");
        b.Ignore(x => x.EventosDominio);
        b.OwnsMany(x => x.Historial, h =>
        {
            h.ToTable("cambio_clasificacion");
            h.WithOwner().HasForeignKey("clasificacion_id");
            h.HasKey(x => x.Id);
            h.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            h.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            h.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
            h.Property(x => x.Concedido).HasColumnName("concedido").HasColumnType("numeric(14,2)").IsRequired();
            h.Property(x => x.Nota).HasColumnName("nota").HasMaxLength(300);
            h.Property(x => x.RegistradoEn).HasColumnName("registrado_en").IsRequired();
        });
        b.Navigation(x => x.Historial).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionAvisoImpago : IEntityTypeConfiguration<AvisoImpago>
{
    public void Configure(EntityTypeBuilder<AvisoImpago> b)
    {
        b.ToTable("aviso_impago");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.PolizaId).HasColumnName("poliza_id").IsRequired();
        b.Property(x => x.FacturaId).HasColumnName("factura_id").IsRequired();
        b.Property(x => x.Factura).HasColumnName("factura").HasMaxLength(40).IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Vencimiento).HasColumnName("vencimiento").IsRequired();
        b.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(60);
        b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.FechaCierre).HasColumnName("fecha_cierre");
        b.Property(x => x.Indemnizacion).HasColumnName("indemnizacion").HasColumnType("numeric(14,2)");
        b.Property(x => x.Nota).HasColumnName("nota").HasMaxLength(300);
        b.HasOne<PolizaSeguroCredito>().WithMany().HasForeignKey(x => x.PolizaId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_aviso_impago_poliza");
        b.HasIndex(x => x.FacturaId).IsUnique().HasFilter("estado = 'Comunicado'").HasDatabaseName("ux_aviso_impago_abierto");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_aviso_impago_cliente");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioSeguroCredito : IRepositorioSeguroCredito
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioSeguroCredito(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public async Task<IReadOnlyList<PolizaSeguroCredito>> PolizasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<PolizaSeguroCredito>().Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PolizaSeguroCredito?> PolizaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PolizaSeguroCredito>().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<ClasificacionSeguro>> ClasificacionesAsync(Guid empresaId, Guid? clienteId = null, CancellationToken ct = default) =>
        await _ctx.Set<ClasificacionSeguro>().Where(c => c.EmpresaId == empresaId && (clienteId == null || c.ClienteId == clienteId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<ClasificacionSeguro?> ClasificacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ClasificacionSeguro>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<AvisoImpago>> AvisosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<AvisoImpago>().Where(a => a.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<AvisoImpago?> AvisoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<AvisoImpago>().FirstOrDefaultAsync(a => a.Id == id, ct);
}
