using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionOrdenCarga : IEntityTypeConfiguration<OrdenCarga>
{
    public void Configure(EntityTypeBuilder<OrdenCarga> b)
    {
        Columnas.Base(b, "orden_carga");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.FechaCarga).HasColumnName("fecha_carga").IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.Muelle).HasColumnName("muelle").HasMaxLength(50);
        b.Property(x => x.TransportistaId).HasColumnName("transportista_id");
        b.Property(x => x.VehiculoId).HasColumnName("vehiculo_id");
        b.Property(x => x.Matricula).HasColumnName("matricula").HasMaxLength(20);
        b.Property(x => x.Conductor).HasColumnName("conductor").HasMaxLength(100);
        b.Property(x => x.TemperaturaConsigna).HasColumnName("temperatura_consigna").HasColumnType("numeric(5,1)");
        b.Property(x => x.Filas).HasColumnName("filas");
        b.Property(x => x.Columnas).HasColumnName("columnas");
        b.Property(x => x.CartaPorte).HasColumnName("carta_porte").IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.FinalizadaEn).HasColumnName("finalizada_en");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_orden_carga_numero");

        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_orden_carga");
            l.WithOwner().HasForeignKey("orden_carga_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            l.Property(x => x.PedidoVentaId).HasColumnName("pedido_venta_id").IsRequired();
            l.Property(x => x.LineaPedidoId).HasColumnName("linea_pedido_id").IsRequired();
            l.Property(x => x.ProductoId).HasColumnName("producto_id");
            l.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            l.Property(x => x.PalesPrevistos).HasColumnName("pales_previstos").IsRequired();
            l.Property(x => x.Fila).HasColumnName("fila");
            l.Property(x => x.Columna).HasColumnName("columna");
            l.HasIndex("orden_carga_id", nameof(LineaOrdenCarga.LineaPedidoId)).IsUnique().HasDatabaseName("ux_linea_orden_carga_linea_pedido");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.OwnsMany(x => x.Cargados, c =>
        {
            c.ToTable("pale_orden_carga");
            c.WithOwner().HasForeignKey("orden_carga_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
            c.Property(x => x.PaleId).HasColumnName("pale_id").IsRequired();
            c.Property(x => x.Sscc).HasColumnName("sscc").HasMaxLength(20).IsRequired();
            c.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            c.Property(x => x.Fila).HasColumnName("fila");
            c.Property(x => x.Columna).HasColumnName("columna");
            c.Property(x => x.CargadoEn).HasColumnName("cargado_en").IsRequired();
            c.Property(x => x.AlbaranId).HasColumnName("albaran_id");
            c.Ignore(x => x.EstaExpedido);
            c.HasIndex("orden_carga_id", nameof(PaleCargado.PaleId)).IsUnique().HasDatabaseName("ux_pale_orden_carga_pale");
            c.HasIndex(x => x.PaleId).HasDatabaseName("ix_pale_orden_carga_pale");
        });
        b.Navigation(x => x.Cargados).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class RepositorioOrdenesCarga : IRepositorioOrdenesCarga
{
    private readonly AgroDbContext _ctx;

    public RepositorioOrdenesCarga(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(OrdenCarga orden) => _ctx.Add(orden);

    public Task<OrdenCarga?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Set<OrdenCarga>().FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IReadOnlyList<OrdenCarga>> ListarAsync(Guid empresaId, EstadoOrdenCarga? estado, CancellationToken ct = default)
    {
        var q = _ctx.Set<OrdenCarga>().AsNoTracking().Where(o => o.EmpresaId == empresaId);
        if (estado is { } e)
        {
            q = q.Where(o => o.Estado == e);
        }

        return await q.OrderByDescending(o => o.FechaCarga).ThenByDescending(o => o.Numero).Take(500).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<OrdenCarga>> AbiertasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<OrdenCarga>().AsNoTracking()
            .Where(o => o.EmpresaId == empresaId && (o.Estado == EstadoOrdenCarga.Propuesta || o.Estado == EstadoOrdenCarga.Pendiente || o.Estado == EstadoOrdenCarga.EnCarga))
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<OrdenCarga>().Where(o => o.EmpresaId == empresaId && o.Ejercicio == ejercicio).Select(o => (int?)o.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;
}
