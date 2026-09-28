using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

internal sealed class ConfiguracionDevolucionVenta : IEntityTypeConfiguration<DevolucionVenta>
{
    public void Configure(EntityTypeBuilder<DevolucionVenta> b)
    {
        b.ToTable("devolucion_venta");
        b.HasKey(d => d.Id);
        b.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(d => d.Numero).HasColumnName("numero").IsRequired();
        b.Property(d => d.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(d => d.AlbaranId).HasColumnName("albaran_venta_id").IsRequired();
        b.Property(d => d.AlbaranNumero).HasColumnName("albaran_numero").HasMaxLength(30).IsRequired();
        b.Property(d => d.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(d => d.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(d => d.Motivo).HasColumnName("motivo").HasMaxLength(300).IsRequired();
        b.Property(d => d.ReclamacionId).HasColumnName("reclamacion_id");
        b.Property(d => d.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(d => d.FormaAbono).HasColumnName("forma_abono").HasMaxLength(30).HasConversion<string>();
        b.Property(d => d.FacturaAbonoId).HasColumnName("factura_abono_id");
        b.Property(d => d.AbonadaEn).HasColumnName("abonada_en");
        b.Property(d => d.AnuladaEn).HasColumnName("anulada_en");
        b.Property(d => d.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(d => d.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(d => d.NumeroCompleto);
        b.Ignore(d => d.Base);
        b.OwnsMany(d => d.Lineas, l =>
        {
            l.ToTable("linea_devolucion_venta");
            l.WithOwner().HasForeignKey("devolucion_venta_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.OrdenAlbaran).HasColumnName("orden_albaran").IsRequired();
            l.Property(x => x.ProductoId).HasColumnName("producto_id");
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            l.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            l.Property(x => x.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,4)").IsRequired();
            l.Property(x => x.PorcentajeDescuento).HasColumnName("porcentaje_descuento").HasColumnType("numeric(5,2)").IsRequired();
            l.Property(x => x.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(20).IsRequired();
            l.Property(x => x.Reingresa).HasColumnName("reingresa").IsRequired();
            l.Ignore(x => x.Base);
        });
        b.Navigation(d => d.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(d => new { d.EmpresaId, d.Fecha, d.Numero }).HasDatabaseName("ix_devolucion_venta_numero");
        b.HasIndex(d => d.AlbaranId).HasDatabaseName("ix_devolucion_venta_albaran");
        b.HasIndex(d => d.FacturaAbonoId).HasDatabaseName("ix_devolucion_venta_factura");
        b.Ignore(d => d.EventosDominio);
    }
}

internal sealed class ConfiguracionConceptoReclamacion : IEntityTypeConfiguration<ConceptoReclamacion>
{
    public void Configure(EntityTypeBuilder<ConceptoReclamacion> b)
    {
        b.ToTable("concepto_reclamacion");
        b.HasKey(c => c.Id);
        b.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(c => c.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        b.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        b.Property(c => c.Activo).HasColumnName("activo").IsRequired();
        b.HasIndex(c => new { c.EmpresaId, c.Codigo }).IsUnique().HasDatabaseName("ux_concepto_reclamacion_codigo");
        b.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionReclamacionVenta : IEntityTypeConfiguration<ReclamacionVenta>
{
    public void Configure(EntityTypeBuilder<ReclamacionVenta> b)
    {
        b.ToTable("reclamacion_venta");
        b.HasKey(r => r.Id);
        b.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Property(r => r.Numero).HasColumnName("numero").IsRequired();
        b.Property(r => r.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(r => r.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(r => r.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        b.Property(r => r.AlbaranId).HasColumnName("albaran_venta_id");
        b.Property(r => r.FacturaId).HasColumnName("factura_id");
        b.Property(r => r.ConceptoId).HasColumnName("concepto_id").IsRequired();
        b.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(1000).IsRequired();
        b.Property(r => r.ImporteReclamado).HasColumnName("importe_reclamado").HasColumnType("numeric(14,2)");
        b.Property(r => r.Responsable).HasColumnName("responsable").HasMaxLength(100);
        b.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        b.Property(r => r.Resolucion).HasColumnName("resolucion").HasMaxLength(20).HasConversion<string>();
        b.Property(r => r.ImporteReconocido).HasColumnName("importe_reconocido").HasColumnType("numeric(14,2)");
        b.Property(r => r.TextoResolucion).HasColumnName("texto_resolucion").HasMaxLength(1000);
        b.Property(r => r.ResueltaEn).HasColumnName("resuelta_en");
        b.Property(r => r.DevolucionId).HasColumnName("devolucion_venta_id");
        b.Property(r => r.FacturaAbonoId).HasColumnName("factura_abono_id");
        b.Property(r => r.AnuladaEn).HasColumnName("anulada_en");
        b.Property(r => r.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(r => r.NumeroCompleto);
        b.HasIndex(r => new { r.EmpresaId, r.Fecha, r.Numero }).HasDatabaseName("ix_reclamacion_venta_numero");
        b.HasIndex(r => new { r.EmpresaId, r.ClienteId }).HasDatabaseName("ix_reclamacion_venta_cliente");
        b.Ignore(r => r.EventosDominio);
    }
}

internal sealed class RepositorioDevolucionesVenta : IRepositorioDevolucionesVenta
{
    private readonly FacturacionDbContext _ctx;

    public RepositorioDevolucionesVenta(FacturacionDbContext ctx) => _ctx = ctx;

    public void Agregar(DevolucionVenta devolucion) => _ctx.DevolucionesVenta.Add(devolucion);

    public Task<DevolucionVenta?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.DevolucionesVenta.SingleOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DevolucionVenta>> DeAlbaranesAsync(IReadOnlyCollection<Guid> albaranIds, CancellationToken ct = default) =>
        await _ctx.DevolucionesVenta.Where(d => albaranIds.Contains(d.AlbaranId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DevolucionVenta>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default) =>
        await _ctx.DevolucionesVenta.Where(d => d.FacturaAbonoId == facturaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DevolucionVenta>> ListarAsync(Guid empresaId, FiltroDevoluciones filtro, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var q = _ctx.DevolucionesVenta.AsNoTracking().Where(d => d.EmpresaId == empresaId);
        if (filtro.ClienteId is { } c) q = q.Where(d => d.ClienteId == c);
        if (filtro.AlbaranId is { } a) q = q.Where(d => d.AlbaranId == a);
        if (filtro.Estado is { } e) q = q.Where(d => d.Estado == e);
        if (filtro.Desde is { } desde) q = q.Where(d => d.Fecha >= desde);
        if (filtro.Hasta is { } hasta) q = q.Where(d => d.Fecha <= hasta);
        return await q.OrderByDescending(d => d.Fecha).ThenByDescending(d => d.Numero).Take(2000).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        (await _ctx.DevolucionesVenta.Where(d => d.EmpresaId == empresaId && d.Fecha.Year == ejercicio).Select(d => (int?)d.Numero).MaxAsync(ct).ConfigureAwait(false) ?? 0) + 1;

    public async Task<Guid?> RectificativaDeAsync(Guid facturaId, CancellationToken ct = default) =>
        await _ctx.Facturas.Where(f => f.RectificaFacturaId == facturaId && f.Estado != EstadoFactura.Anulada)
            .OrderByDescending(f => f.FechaEmision).Select(f => (Guid?)f.Id).FirstOrDefaultAsync(ct).ConfigureAwait(false);
}

internal sealed class RepositorioReclamaciones : IRepositorioReclamaciones
{
    private readonly FacturacionDbContext _ctx;

    public RepositorioReclamaciones(FacturacionDbContext ctx) => _ctx = ctx;

    public void Agregar(ReclamacionVenta reclamacion) => _ctx.Reclamaciones.Add(reclamacion);

    public Task<ReclamacionVenta?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Reclamaciones.SingleOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ReclamacionVenta>> ListarAsync(Guid empresaId, FiltroReclamaciones filtro, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var q = _ctx.Reclamaciones.AsNoTracking().Where(r => r.EmpresaId == empresaId);
        if (filtro.ClienteId is { } c) q = q.Where(r => r.ClienteId == c);
        if (filtro.Estado is { } e) q = q.Where(r => r.Estado == e);
        if (filtro.ConceptoId is { } k) q = q.Where(r => r.ConceptoId == k);
        if (filtro.Desde is { } desde) q = q.Where(r => r.Fecha >= desde);
        if (filtro.Hasta is { } hasta) q = q.Where(r => r.Fecha <= hasta);
        return await q.OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Numero).Take(2000).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        (await _ctx.Reclamaciones.Where(r => r.EmpresaId == empresaId && r.Fecha.Year == ejercicio).Select(r => (int?)r.Numero).MaxAsync(ct).ConfigureAwait(false) ?? 0) + 1;

    public void Agregar(ConceptoReclamacion concepto) => _ctx.ConceptosReclamacion.Add(concepto);

    public Task<ConceptoReclamacion?> ConceptoAsync(Guid id, CancellationToken ct = default) => _ctx.ConceptosReclamacion.SingleOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ConceptoReclamacion>> ConceptosAsync(CancellationToken ct = default) =>
        await _ctx.ConceptosReclamacion.OrderBy(c => c.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> CodigoConceptoUsadoAsync(string codigo, Guid? salvo, CancellationToken ct = default) =>
        _ctx.ConceptosReclamacion.AnyAsync(c => c.Codigo == codigo && c.Id != salvo, ct);
}
