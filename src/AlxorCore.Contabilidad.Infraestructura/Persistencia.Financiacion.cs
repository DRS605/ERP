using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>Mapeo EF Core de las operaciones de financiación (préstamos, leasing y pólizas) y sus hechos.</summary>
internal sealed class ConfiguracionOperacionFinanciacion : IEntityTypeConfiguration<OperacionFinanciacion>
{
    public void Configure(EntityTypeBuilder<OperacionFinanciacion> builder)
    {
        builder.ToTable("operacion_financiacion");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(o => o.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(o => o.Tipo).HasColumnName("tipo").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(o => o.Codigo).HasColumnName("codigo").HasMaxLength(OperacionFinanciacion.LongitudCodigo).IsRequired();
        builder.Property(o => o.Descripcion).HasColumnName("descripcion").HasMaxLength(OperacionFinanciacion.LongitudDescripcion).IsRequired();
        builder.Property(o => o.Entidad).HasColumnName("entidad").HasMaxLength(OperacionFinanciacion.LongitudEntidad);
        builder.Property(o => o.Estado).HasColumnName("estado").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(o => o.FechaFormalizacion).HasColumnName("fecha_formalizacion").IsRequired();
        builder.Property(o => o.Capital).HasColumnName("capital").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(o => o.TipoInteres).HasColumnName("tipo_interes").HasColumnType("numeric(7,4)").IsRequired();
        builder.Property(o => o.Sistema).HasColumnName("sistema").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(o => o.Periodicidad).HasColumnName("periodicidad").HasMaxLength(12).HasConversion<string>().IsRequired();
        builder.Property(o => o.NumeroCuotas).HasColumnName("numero_cuotas").IsRequired();
        builder.Property(o => o.CuotasCarencia).HasColumnName("cuotas_carencia").IsRequired();
        builder.Property(o => o.FechaPrimeraCuota).HasColumnName("fecha_primera_cuota");
        builder.Property(o => o.ValorResidual).HasColumnName("valor_residual").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(o => o.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(o => o.FechaVencimiento).HasColumnName("fecha_vencimiento");
        builder.Property(o => o.ComisionNoDisponible).HasColumnName("comision_no_disponible").HasColumnType("numeric(6,4)").IsRequired();
        builder.Property(o => o.ContabilizarDesde).HasColumnName("contabilizar_desde");
        builder.Property(o => o.CuentaLargoPlazo).HasColumnName("cuenta_largo_plazo").HasMaxLength(Cuenta.LongitudMaximaCodigo);
        builder.Property(o => o.CuentaCortoPlazo).HasColumnName("cuenta_corto_plazo").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(o => o.CuentaIntereses).HasColumnName("cuenta_intereses").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(o => o.CuentaTesoreria).HasColumnName("cuenta_tesoreria").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(o => o.CuentaComisiones).HasColumnName("cuenta_comisiones").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(o => o.CuentaActivo).HasColumnName("cuenta_activo").HasMaxLength(Cuenta.LongitudMaximaCodigo);
        builder.Ignore(o => o.EsPoliza);
        builder.Ignore(o => o.Revisiones);
        builder.Ignore(o => o.Anticipos);
        builder.Ignore(o => o.HorizonteCortoPlazo);
        builder.Ignore(o => o.InicioSiguienteLiquidacion);
        builder.Ignore(o => o.UltimoEvento);
        builder.Ignore(o => o.TieneAsientos);
        builder.OwnsMany(o => o.Eventos, e =>
        {
            e.ToTable("evento_financiacion");
            e.WithOwner().HasForeignKey("operacion_id");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            e.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(24).HasConversion<string>().IsRequired();
            e.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            e.Property(x => x.Numero).HasColumnName("numero");
            e.Property(x => x.Ejercicio).HasColumnName("ejercicio");
            e.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            e.Property(x => x.Intereses).HasColumnName("intereses").HasColumnType("numeric(14,2)").IsRequired();
            e.Property(x => x.Comision).HasColumnName("comision").HasColumnType("numeric(14,2)").IsRequired();
            e.Property(x => x.Iva).HasColumnName("iva").HasColumnType("numeric(14,2)").IsRequired();
            e.Property(x => x.TipoInteres).HasColumnName("tipo_interes").HasColumnType("numeric(7,4)");
            e.Property(x => x.Desde).HasColumnName("desde");
            e.Property(x => x.AsientoId).HasColumnName("asiento_id");
            e.HasIndex("operacion_id", nameof(EventoFinanciacion.Orden)).IsUnique().HasDatabaseName("ux_evento_financiacion_orden");
            e.HasIndex(x => x.AsientoId).HasDatabaseName("ix_evento_financiacion_asiento");
        });
        builder.HasIndex(o => new { o.EmpresaId, o.Codigo }).IsUnique().HasDatabaseName("ux_operacion_financiacion_codigo");
        builder.Ignore(o => o.EventosDominio);
    }
}

internal sealed class RepositorioFinanciacion : IRepositorioFinanciacion
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioFinanciacion(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<OperacionFinanciacion>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.OperacionesFinanciacion.Where(o => o.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<OperacionFinanciacion?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.OperacionesFinanciacion.SingleOrDefaultAsync(o => o.Id == id, ct);

    public Task<bool> ExisteCodigoAsync(Guid empresaId, string codigo, CancellationToken ct = default) =>
        _contexto.OperacionesFinanciacion.AnyAsync(o => o.EmpresaId == empresaId && o.Codigo == codigo, ct);

    public void Agregar(OperacionFinanciacion operacion) => _contexto.OperacionesFinanciacion.Add(operacion);

    public void Eliminar(OperacionFinanciacion operacion) => _contexto.OperacionesFinanciacion.Remove(operacion);
}
