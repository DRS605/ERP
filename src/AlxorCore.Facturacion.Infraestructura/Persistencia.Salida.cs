using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

/// <summary>Mapeo EF Core de la bandeja de salida (outbox transaccional).</summary>
internal sealed class ConfiguracionMensajeSalida : IEntityTypeConfiguration<MensajeSalida>
{
    public void Configure(EntityTypeBuilder<MensajeSalida> builder)
    {
        builder.ToTable("mensaje_salida");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(m => m.Tipo).HasColumnName("tipo").HasMaxLength(40).IsRequired();
        builder.Property(m => m.Carga).HasColumnName("carga").HasColumnType("jsonb").IsRequired();
        builder.Property(m => m.Procesado).HasColumnName("procesado").IsRequired();
        builder.Property(m => m.Intentos).HasColumnName("intentos").IsRequired();
        builder.Property(m => m.UltimoError).HasColumnName("ultimo_error").HasMaxLength(500);
        builder.Property(m => m.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(m => m.ProcesadoEn).HasColumnName("procesado_en");
        builder.HasIndex(m => new { m.EmpresaId, m.Procesado }).HasDatabaseName("ix_mensaje_salida_empresa_procesado");
        builder.Ignore(m => m.EventosDominio);
    }
}

internal sealed class RepositorioSalida : IRepositorioSalida
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioSalida(FacturacionDbContext contexto) => _contexto = contexto;

    public void Agregar(MensajeSalida mensaje) => _contexto.MensajesSalida.Add(mensaje);

    public async Task<IReadOnlyList<MensajeSalida>> PendientesAsync(int maximo, CancellationToken ct = default) =>
        await _contexto.MensajesSalida
            .Where(m => !m.Procesado)
            .OrderBy(m => m.CreadoEn)
            .Take(maximo)
            .ToListAsync(ct).ConfigureAwait(false);
}
