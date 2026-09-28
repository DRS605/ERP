using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Tesoreria.Infraestructura;

internal sealed class ConfiguracionEntregaCuenta : IEntityTypeConfiguration<EntregaCuentaProveedor>
{
    public void Configure(EntityTypeBuilder<EntregaCuentaProveedor> builder)
    {
        builder.ToTable("entrega_cuenta_proveedor");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(e => e.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(e => e.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(e => e.Concepto).HasColumnName("concepto").HasMaxLength(EntregaCuentaProveedor.LongitudConcepto).IsRequired();
        builder.Property(e => e.Metodo).HasColumnName("metodo").HasMaxLength(60);
        builder.Property(e => e.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
        builder.Property(e => e.CreadaEn).HasColumnName("creada_en").IsRequired();
        builder.Property(e => e.AnuladaEn).HasColumnName("anulada_en");
        builder.Property(e => e.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        builder.OwnsMany(e => e.Cancelaciones, c =>
        {
            c.ToTable("cancelacion_entrega_cuenta");
            c.WithOwner().HasForeignKey("entrega_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.GastoId).HasColumnName("gasto_id").IsRequired();
            c.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            c.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
            c.Property(x => x.MovimientoId).HasColumnName("movimiento_id").IsRequired();
            c.Property(x => x.LiquidacionId).HasColumnName("liquidacion_id");
            c.HasIndex("entrega_id").HasDatabaseName("ix_cancelacion_entrega_cuenta_entrega");
            c.HasIndex(x => x.MovimientoId).IsUnique().HasDatabaseName("ux_cancelacion_entrega_cuenta_movimiento");
        });
        builder.Navigation(e => e.Cancelaciones).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(e => e.Cancelado);
        builder.Ignore(e => e.Pendiente);
        builder.Ignore(e => e.Estado);
        builder.HasIndex(e => new { e.EmpresaId, e.ProveedorId }).HasDatabaseName("ix_entrega_cuenta_proveedor_empresa_proveedor");
        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class ConfiguracionLiquidacionPagos : IEntityTypeConfiguration<LiquidacionPagos>
{
    public void Configure(EntityTypeBuilder<LiquidacionPagos> builder)
    {
        builder.ToTable("liquidacion_pagos");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(l => l.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(l => l.Numero).HasColumnName("numero").IsRequired();
        builder.Property(l => l.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(l => l.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(l => l.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        builder.Property(l => l.ClienteId).HasColumnName("cliente_id");
        builder.Property(l => l.APagar).HasColumnName("a_pagar").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(l => l.EntregasCanceladas).HasColumnName("entregas_canceladas").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(l => l.Compensado).HasColumnName("compensado").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(l => l.Liquido).HasColumnName("liquido").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(l => l.FormaPago).HasColumnName("forma_pago").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(l => l.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
        builder.Property(l => l.RemesaId).HasColumnName("remesa_id");
        builder.Property(l => l.LoteId).HasColumnName("lote_id");
        builder.Property(l => l.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(l => l.CreadaEn).HasColumnName("creada_en").IsRequired();
        builder.Property(l => l.AnuladaEn).HasColumnName("anulada_en");
        builder.Property(l => l.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        builder.Ignore(l => l.NumeroCompleto);
        builder.OwnsMany(l => l.Lineas, x =>
        {
            x.ToTable("linea_liquidacion_pagos");
            x.WithOwner().HasForeignKey("liquidacion_id");
            x.HasKey(y => y.Id);
            x.Property(y => y.Id).HasColumnName("id").ValueGeneratedNever();
            x.Property(y => y.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
            x.Property(y => y.DocumentoId).HasColumnName("documento_id").IsRequired();
            x.Property(y => y.Documento).HasColumnName("documento").HasMaxLength(60).IsRequired();
            x.Property(y => y.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            x.Property(y => y.MovimientoId).HasColumnName("movimiento_id");
            x.Property(y => y.EntregaId).HasColumnName("entrega_id");
            x.HasIndex("liquidacion_id").HasDatabaseName("ix_linea_liquidacion_pagos_liquidacion");
            x.HasIndex(y => y.MovimientoId).IsUnique().HasDatabaseName("ux_linea_liquidacion_pagos_movimiento");
        });
        builder.Navigation(l => l.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(l => new { l.EmpresaId, l.Ejercicio, l.Numero }).IsUnique().HasDatabaseName("ux_liquidacion_pagos_numero");
        builder.HasIndex(l => new { l.EmpresaId, l.ProveedorId }).HasDatabaseName("ix_liquidacion_pagos_proveedor");
        builder.HasIndex(l => l.LoteId).HasDatabaseName("ix_liquidacion_pagos_lote");
        builder.Ignore(l => l.EventosDominio);
    }
}

internal sealed class RepositorioLiquidacionesPagos : IRepositorioLiquidacionesPagos
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioLiquidacionesPagos(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(EntregaCuentaProveedor entrega) => _ctx.EntregasCuenta.Add(entrega);

    public void Agregar(LiquidacionPagos liquidacion) => _ctx.LiquidacionesPagos.Add(liquidacion);

    public Task<EntregaCuentaProveedor?> EntregaAsync(Guid id, CancellationToken ct = default) =>
        _ctx.EntregasCuenta.SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<EntregaCuentaProveedor>> EntregasAsync(Guid? proveedorId, bool soloPendientes, CancellationToken ct = default)
    {
        var lista = await _ctx.EntregasCuenta.Where(e => proveedorId == null || e.ProveedorId == proveedorId)
            .Where(e => !soloPendientes || e.AnuladaEn == null)
            .OrderByDescending(e => e.Fecha).ThenByDescending(e => e.CreadaEn).ToListAsync(ct).ConfigureAwait(false);
        return soloPendientes ? lista.Where(e => e.Pendiente > 0).ToList() : lista;
    }

    public Task<EntregaCuentaProveedor?> EntregaDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.EntregasCuenta.SingleOrDefaultAsync(e => e.Cancelaciones.Any(c => c.MovimientoId == movimientoId), ct);

    public Task<LiquidacionPagos?> LiquidacionAsync(Guid id, CancellationToken ct = default) =>
        _ctx.LiquidacionesPagos.SingleOrDefaultAsync(l => l.Id == id, ct);

    public async Task<IReadOnlyList<LiquidacionPagos>> LiquidacionesAsync(Guid? proveedorId, Guid? loteId, CancellationToken ct = default) =>
        await _ctx.LiquidacionesPagos.Where(l => (proveedorId == null || l.ProveedorId == proveedorId) && (loteId == null || l.LoteId == loteId))
            .OrderByDescending(l => l.Ejercicio).ThenByDescending(l => l.Numero).ToListAsync(ct).ConfigureAwait(false);

    public Task<LiquidacionPagos?> LiquidacionDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.LiquidacionesPagos.SingleOrDefaultAsync(l => l.Lineas.Any(x => x.MovimientoId == movimientoId), ct);

    public async Task<int> UltimoNumeroAsync(int ejercicio, CancellationToken ct = default) =>
        await _ctx.LiquidacionesPagos.Where(l => l.Ejercicio == ejercicio).MaxAsync(l => (int?)l.Numero, ct).ConfigureAwait(false) ?? 0;

    public Task BloquearAsync(string clave, CancellationToken ct = default) => _ctx.BloquearAsync(clave, ct);
}
