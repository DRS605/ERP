using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

internal sealed class ConfiguracionAgenteComercial : IEntityTypeConfiguration<AgenteComercial>
{
    public void Configure(EntityTypeBuilder<AgenteComercial> b)
    {
        b.ToTable("agente_comercial");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(AgenteComercial.LongitudNombre).IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Devengo).HasColumnName("devengo").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.HasIndex(x => x.ProveedorId).HasDatabaseName("ix_agente_comercial_proveedor");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionAsignacionAgente : IEntityTypeConfiguration<AsignacionAgente>
{
    public void Configure(EntityTypeBuilder<AsignacionAgente> b)
    {
        b.ToTable("asignacion_agente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.AgenteId).HasColumnName("agente_id");
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ClienteId, x.Desde }).IsUnique().HasDatabaseName("ux_asignacion_agente");
        b.HasIndex(x => x.AgenteId).HasDatabaseName("ix_asignacion_agente_agente");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_asignacion_agente_cliente");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionReglaComision : IEntityTypeConfiguration<ReglaComision>
{
    public void Configure(EntityTypeBuilder<ReglaComision> b)
    {
        b.ToTable("regla_comision");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.AgenteId).HasColumnName("agente_id").IsRequired();
        b.Property(x => x.FamiliaId).HasColumnName("familia_id");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)").IsRequired();
        b.HasIndex(x => x.AgenteId).HasDatabaseName("ix_regla_comision_agente");
        b.HasIndex(x => x.FamiliaId).HasDatabaseName("ix_regla_comision_familia");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_regla_comision_cliente");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class ConfiguracionLiquidacionAgente : IEntityTypeConfiguration<LiquidacionAgente>
{
    public void Configure(EntityTypeBuilder<LiquidacionAgente> b)
    {
        b.ToTable("liquidacion_agente");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.AgenteId).HasColumnName("agente_id").IsRequired();
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.GastoId).HasColumnName("gasto_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.Importe);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_liquidacion_agente_numero");
        b.HasIndex(x => x.AgenteId).HasDatabaseName("ix_liquidacion_agente_agente");
        b.HasIndex(x => x.GastoId).HasDatabaseName("ix_liquidacion_agente_gasto");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_liquidacion_agente");
            l.WithOwner().HasForeignKey("liquidacion_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("liquidacion_id").HasColumnName("liquidacion_id");
            l.Property(x => x.FacturaId).HasColumnName("factura_id").IsRequired();
            l.Property(x => x.Factura).HasColumnName("factura").HasMaxLength(40).IsRequired();
            l.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            l.Property(x => x.Cliente).HasColumnName("cliente").HasMaxLength(200);
            l.Property(x => x.BaseVenta).HasColumnName("base_venta").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.PorcentajeDevengado).HasColumnName("porcentaje_devengado").HasColumnType("numeric(7,4)").IsRequired();
            l.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            l.HasIndex("liquidacion_id").HasDatabaseName("ix_linea_liquidacion_agente_liquidacion");
            l.HasIndex(x => x.FacturaId).HasDatabaseName("ix_linea_liquidacion_agente_factura");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioComisiones : IRepositorioComisiones
{
    private readonly FacturacionDbContext _ctx;

    public RepositorioComisiones(FacturacionDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<AgenteComercial>> AgentesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<AgenteComercial>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<AgenteComercial?> AgenteAsync(Guid id, CancellationToken ct = default) => _ctx.Set<AgenteComercial>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<AsignacionAgente>> AsignacionesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<AsignacionAgente>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ReglaComision>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ReglaComision>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ReglaComision?> ReglaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ReglaComision>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LiquidacionAgente>> LiquidacionesAsync(Guid empresaId, Guid? agenteId, CancellationToken ct = default) =>
        await _ctx.Set<LiquidacionAgente>().Where(x => x.EmpresaId == empresaId && (agenteId == null || x.AgenteId == agenteId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<LiquidacionAgente?> LiquidacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<LiquidacionAgente>().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<int> SiguienteNumeroLiquidacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        await _ctx.BloquearAsync($"facturacion.liquidacion_agente.{empresaId}.{ejercicio}", ct).ConfigureAwait(false);
        var max = await _ctx.Set<LiquidacionAgente>().AsNoTracking().Where(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio)
            .MaxAsync(x => (int?)x.Numero, ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);
}
