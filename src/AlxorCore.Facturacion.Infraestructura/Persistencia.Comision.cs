using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

internal sealed class ConfiguracionLiquidacionComision : IEntityTypeConfiguration<LiquidacionComision>
{
    public void Configure(EntityTypeBuilder<LiquidacionComision> b)
    {
        b.ToTable("liquidacion_comision");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero");
        b.Property(x => x.ReferenciaCliente).HasColumnName("referencia_cliente").HasMaxLength(60);
        b.Property(x => x.Modo).HasColumnName("modo").HasMaxLength(30).HasConversion<string>().IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id");
        b.Property(x => x.CodigoIvaGastos).HasColumnName("codigo_iva_gastos").HasMaxLength(20);
        b.Property(x => x.GastoId).HasColumnName("gasto_id");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.Ignore(x => x.ImporteBruto);
        b.Ignore(x => x.TotalGastos);
        b.Ignore(x => x.ImporteNeto);
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_liquidacion_comision");
            l.WithOwner().HasForeignKey("liquidacion_comision_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("liquidacion_comision_id").HasColumnName("liquidacion_comision_id");
            l.Property(x => x.AlbaranVentaId).HasColumnName("albaran_venta_id").IsRequired();
            l.Property(x => x.AlbaranNumero).HasColumnName("albaran_numero").HasMaxLength(30).IsRequired();
            l.Property(x => x.AlbaranFecha).HasColumnName("albaran_fecha").IsRequired();
            l.Property(x => x.OrdenAlbaran).HasColumnName("orden_albaran").IsRequired();
            l.Property(x => x.ProductoId).HasColumnName("producto_id");
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            l.Property(x => x.CantidadEnviada).HasColumnName("cantidad_enviada").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.CantidadVendida).HasColumnName("cantidad_vendida").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.PrecioBruto).HasColumnName("precio_bruto").HasColumnType("numeric(14,4)").IsRequired();
            l.Property(x => x.ImporteBruto).HasColumnName("importe_bruto").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.Gastos).HasColumnName("gastos").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.PrecioAlbaran).HasColumnName("precio_albaran").HasColumnType("numeric(14,4)").IsRequired();
            l.Property(x => x.PrecioEstimado).HasColumnName("precio_estimado").HasColumnType("numeric(14,4)").IsRequired();
            l.Property(x => x.DescuentoAnterior).HasColumnName("descuento_anterior").HasColumnType("numeric(5,2)").IsRequired();
            l.Ignore(x => x.ImporteNeto);
            l.Ignore(x => x.Merma);
            l.HasIndex("liquidacion_comision_id").HasDatabaseName("ix_linea_liquidacion_comision_liquidacion");
            l.HasIndex(x => new { x.AlbaranVentaId, x.OrdenAlbaran }).HasDatabaseName("ix_linea_liquidacion_comision_albaran");
            l.HasIndex(x => x.ProductoId).HasDatabaseName("ix_linea_liquidacion_comision_producto");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.OwnsMany(x => x.Gastos, g =>
        {
            g.ToTable("gasto_liquidacion_comision");
            g.WithOwner().HasForeignKey("liquidacion_comision_id");
            g.HasKey(x => x.Id);
            g.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            g.Property<Guid>("liquidacion_comision_id").HasColumnName("liquidacion_comision_id");
            g.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
            g.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(120);
            g.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)");
            g.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            g.HasIndex("liquidacion_comision_id").HasDatabaseName("ix_gasto_liquidacion_comision_liquidacion");
        });
        b.Navigation(x => x.Gastos).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_liquidacion_comision_fecha");
        b.HasIndex(x => new { x.EmpresaId, x.Numero }).HasDatabaseName("ix_liquidacion_comision_numero");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_liquidacion_comision_cliente");
        b.HasIndex(x => x.ProveedorId).HasDatabaseName("ix_liquidacion_comision_proveedor");
        b.HasIndex(x => x.GastoId).HasDatabaseName("ix_liquidacion_comision_gasto");
        b.Ignore(x => x.EventosDominio);
    }
}

internal sealed class RepositorioLiquidacionesComision : IRepositorioLiquidacionesComision
{
    private readonly FacturacionDbContext _ctx;

    public RepositorioLiquidacionesComision(FacturacionDbContext ctx) => _ctx = ctx;

    public void Agregar(LiquidacionComision liquidacion) => _ctx.Set<LiquidacionComision>().Add(liquidacion);

    public void Eliminar(LiquidacionComision liquidacion) => _ctx.Set<LiquidacionComision>().Remove(liquidacion);

    public Task<LiquidacionComision?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Set<LiquidacionComision>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LiquidacionComision>> ListarAsync(Guid empresaId, Guid? clienteId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _ctx.Set<LiquidacionComision>().Where(x => x.EmpresaId == empresaId && (clienteId == null || x.ClienteId == clienteId)
            && (desde == null || x.Fecha >= desde) && (hasta == null || x.Fecha <= hasta)).OrderByDescending(x => x.Fecha).Take(2000).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LiquidacionComision>> VivasDeAlbaranesAsync(IReadOnlyCollection<Guid> albaranIds, CancellationToken ct = default) =>
        albaranIds.Count == 0 ? [] : await _ctx.Set<LiquidacionComision>()
            .Where(x => x.Estado != EstadoLiquidacionComision.Anulada && x.Lineas.Any(l => albaranIds.Contains(l.AlbaranVentaId))).ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        (await _ctx.Set<LiquidacionComision>().Where(x => x.EmpresaId == empresaId && x.Fecha.Year == ejercicio).Select(x => x.Numero).MaxAsync(ct).ConfigureAwait(false) ?? 0) + 1;
}
