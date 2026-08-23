using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

/// <summary>Mapeo EF Core del pedido de venta y sus líneas.</summary>
internal sealed class ConfiguracionPedidoVenta : IEntityTypeConfiguration<PedidoVenta>
{
    public void Configure(EntityTypeBuilder<PedidoVenta> builder)
    {
        builder.ToTable("pedido_venta");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(p => p.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(PedidoVenta.LongitudMaximaTexto).IsRequired();
        builder.Property(p => p.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(p => p.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(p => p.Numero).HasColumnName("numero").IsRequired();
        builder.Property(p => p.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Property(p => p.PresupuestoOrigenId).HasColumnName("presupuesto_origen_id");
        builder.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.FacturaId).HasColumnName("factura_id");
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.OwnsMany(p => p.Lineas, linea =>
        {
            linea.ToTable("linea_pedido_venta");
            linea.WithOwner().HasForeignKey("pedido_venta_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.ProductoId).HasColumnName("producto_id");
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,4)").IsRequired();
            linea.Property(l => l.PorcentajeDescuento).HasColumnName("descuento").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
            linea.Property(l => l.CantidadServida).HasColumnName("cantidad_servida").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.CantidadFacturada).HasColumnName("cantidad_facturada").HasColumnType("numeric(14,3)").IsRequired();
        });

        builder.HasIndex(p => new { p.EmpresaId, p.Ejercicio, p.Numero }).IsUnique().HasDatabaseName("ux_pedido_venta_empresa_ejercicio_numero");
        builder.Ignore(p => p.EventosDominio);
    }
}

/// <summary>Mapeo EF Core del albarán de venta y sus líneas.</summary>
internal sealed class ConfiguracionAlbaranVenta : IEntityTypeConfiguration<AlbaranVenta>
{
    public void Configure(EntityTypeBuilder<AlbaranVenta> builder)
    {
        builder.ToTable("albaran_venta");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.PedidoId).HasColumnName("pedido_id").IsRequired();
        builder.Property(a => a.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(a => a.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        builder.Property(a => a.Numero).HasColumnName("numero").IsRequired();
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Property(a => a.Referencia).HasColumnName("referencia").HasMaxLength(200);
        builder.Property(a => a.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.OwnsMany(a => a.Lineas, linea =>
        {
            linea.ToTable("linea_albaran_venta");
            linea.WithOwner().HasForeignKey("albaran_venta_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.LineaPedidoId).HasColumnName("linea_pedido_id").IsRequired();
            linea.Property(l => l.ProductoId).HasColumnName("producto_id");
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
        });

        builder.HasIndex(a => new { a.EmpresaId, a.PedidoId }).HasDatabaseName("ix_albaran_venta_empresa_pedido");
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class RepositorioPedidosVenta : IRepositorioPedidosVenta
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioPedidosVenta(FacturacionDbContext contexto) => _contexto = contexto;

    public Task<PedidoVenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.PedidosVenta.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PedidoVenta pedido) => _contexto.PedidosVenta.Add(pedido);

    public async Task<IReadOnlyList<PedidoVentaDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var pedidos = await _contexto.PedidosVenta.AsNoTracking()
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.Fecha).ThenByDescending(p => p.Numero).ToListAsync(ct).ConfigureAwait(false);
        return pedidos.Select(PedidoVentaDto.Desde).ToList();
    }

    public async Task<PedidoVentaDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _contexto.PedidosVenta.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return p is null ? null : PedidoVentaDto.Desde(p);
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var max = await _contexto.PedidosVenta.Where(p => p.EmpresaId == empresaId && p.Ejercicio == ejercicio)
            .Select(p => (int?)p.Numero).MaxAsync(ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

internal sealed class RepositorioAlbaranesVenta : IRepositorioAlbaranesVenta
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioAlbaranesVenta(FacturacionDbContext contexto) => _contexto = contexto;

    public void Agregar(AlbaranVenta albaran) => _contexto.AlbaranesVenta.Add(albaran);

    public async Task<IReadOnlyList<AlbaranVentaDto>> ListarPorPedidoAsync(Guid empresaId, Guid pedidoId, CancellationToken ct = default)
    {
        var albaranes = await _contexto.AlbaranesVenta.AsNoTracking()
            .Where(a => a.EmpresaId == empresaId && a.PedidoId == pedidoId)
            .OrderBy(a => a.Numero).ToListAsync(ct).ConfigureAwait(false);
        return albaranes.Select(AlbaranVentaDto.Desde).ToList();
    }

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var max = await _contexto.AlbaranesVenta.Where(a => a.EmpresaId == empresaId && a.Fecha.Year == ejercicio)
            .Select(a => (int?)a.Numero).MaxAsync(ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }
}

/// <summary>Mapeo EF Core de la carta de porte y sus líneas de mercancía.</summary>
internal sealed class ConfiguracionCartaPorte : IEntityTypeConfiguration<CartaPorte>
{
    public void Configure(EntityTypeBuilder<CartaPorte> builder)
    {
        builder.ToTable("carta_porte");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Serie).HasColumnName("serie").HasMaxLength(10);
        builder.Property(c => c.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(c => c.Numero).HasColumnName("numero").IsRequired();
        builder.Property(c => c.FechaExpedicion).HasColumnName("fecha_expedicion").IsRequired();
        builder.Property(c => c.RemitenteNombre).HasColumnName("remitente_nombre").HasMaxLength(CartaPorte.LongitudMaximaTexto).IsRequired();
        builder.Property(c => c.RemitenteNif).HasColumnName("remitente_nif").HasMaxLength(20);
        builder.Property(c => c.DestinatarioClienteId).HasColumnName("destinatario_cliente_id");
        builder.Property(c => c.DestinatarioNombre).HasColumnName("destinatario_nombre").HasMaxLength(CartaPorte.LongitudMaximaTexto).IsRequired();
        builder.Property(c => c.DestinatarioNif).HasColumnName("destinatario_nif").HasMaxLength(20);
        builder.Property(c => c.TransportistaNombre).HasColumnName("transportista_nombre").HasMaxLength(CartaPorte.LongitudMaximaTexto);
        builder.Property(c => c.TransportistaNif).HasColumnName("transportista_nif").HasMaxLength(20);
        builder.Property(c => c.Matricula).HasColumnName("matricula").HasMaxLength(20);
        builder.Property(c => c.LugarOrigen).HasColumnName("lugar_origen").HasMaxLength(CartaPorte.LongitudMaximaTexto).IsRequired();
        builder.Property(c => c.LugarDestino).HasColumnName("lugar_destino").HasMaxLength(CartaPorte.LongitudMaximaTexto).IsRequired();
        builder.Property(c => c.FechaCarga).HasColumnName("fecha_carga");
        builder.Property(c => c.Observaciones).HasColumnName("observaciones").HasMaxLength(CartaPorte.LongitudMaximaTexto);
        builder.Property(c => c.AlbaranId).HasColumnName("albaran_id");
        builder.Property(c => c.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.OwnsMany(c => c.Lineas, linea =>
        {
            linea.ToTable("linea_carta_porte");
            linea.WithOwner().HasForeignKey("carta_porte_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Bultos).HasColumnName("bultos").IsRequired();
            linea.Property(l => l.PesoKg).HasColumnName("peso_kg").HasColumnType("numeric(14,3)").IsRequired();
        });

        builder.HasIndex(c => new { c.EmpresaId, c.Serie, c.Ejercicio, c.Numero })
            .IsUnique().HasDatabaseName("ux_carta_porte_empresa_serie_ejercicio_numero");
        builder.Ignore(c => c.EventosDominio);
        builder.Ignore(c => c.TotalBultos);
        builder.Ignore(c => c.TotalPesoKg);
    }
}

internal sealed class RepositorioCartasPorte : IRepositorioCartasPorte, IConsultaCartasPorte
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioCartasPorte(FacturacionDbContext contexto) => _contexto = contexto;

    public void Agregar(CartaPorte cartaPorte) => _contexto.CartasPorte.Add(cartaPorte);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, string? serie, int ejercicio, CancellationToken ct = default)
    {
        var s = string.IsNullOrWhiteSpace(serie) ? null : serie.Trim().ToUpperInvariant();
        var max = await _contexto.CartasPorte
            .Where(c => c.EmpresaId == empresaId && c.Serie == s && c.Ejercicio == ejercicio)
            .Select(c => (int?)c.Numero).MaxAsync(ct).ConfigureAwait(false);
        return (max ?? 0) + 1;
    }

    public async Task<CartaPorteDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var carta = await _contexto.CartasPorte.AsNoTracking()
            .Include(c => c.Lineas).SingleOrDefaultAsync(c => c.Id == id, ct).ConfigureAwait(false);
        return carta is null ? null : CartaPorteDto.Desde(carta);
    }

    public async Task<IReadOnlyList<CartaPorteResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var cartas = await _contexto.CartasPorte.AsNoTracking().Include(c => c.Lineas)
            .Where(c => c.EmpresaId == empresaId)
            .OrderByDescending(c => c.FechaExpedicion).ThenByDescending(c => c.Numero)
            .ToListAsync(ct).ConfigureAwait(false);
        return cartas
            .Select(c => new CartaPorteResumen(c.Id, c.NumeroCompleto, c.FechaExpedicion, c.DestinatarioNombre, c.LugarDestino, c.TotalBultos, c.TotalPesoKg))
            .ToList();
    }
}
