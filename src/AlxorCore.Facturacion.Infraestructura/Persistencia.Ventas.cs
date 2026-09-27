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
        builder.Property(a => a.AnuladoEn).HasColumnName("anulado_en");
        builder.Property(a => a.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);

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

    public Task<AlbaranVenta?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) => _contexto.AlbaranesVenta.SingleOrDefaultAsync(a => a.Id == id, ct);

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
        builder.Property(c => c.AnuladaEn).HasColumnName("anulada_en");
        builder.Property(c => c.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(CartaPorte.LongitudMaximaTexto);
        builder.Property(c => c.Tipo).HasColumnName("tipo").HasMaxLength(15).HasConversion<string>().IsRequired().HasDefaultValue(TipoCartaPorte.Nacional).HasSentinel((TipoCartaPorte)0);
        builder.Property(c => c.Modo).HasColumnName("modo").HasMaxLength(15).HasConversion<string>().IsRequired().HasDefaultValue(ModoTransporte.Carretera).HasSentinel((ModoTransporte)0);
        builder.Property(c => c.Incoterm).HasColumnName("incoterm").HasMaxLength(3);
        builder.Property(c => c.LugarIncoterm).HasColumnName("lugar_incoterm").HasMaxLength(100);
        builder.Property(c => c.Portes).HasColumnName("portes").HasMaxLength(10).HasConversion<string>();
        builder.Property(c => c.DocumentosAnexos).HasColumnName("documentos_anexos").HasMaxLength(300);
        builder.Property(c => c.Instrucciones).HasColumnName("instrucciones").HasMaxLength(500);
        builder.Property(c => c.PaisOrigen).HasColumnName("pais_origen").HasMaxLength(2);
        builder.Property(c => c.PaisDestino).HasColumnName("pais_destino").HasMaxLength(2);
        builder.Property(c => c.TransportistaId).HasColumnName("transportista_id");
        builder.Property(c => c.VehiculoId).HasColumnName("vehiculo_id");
        builder.Property(c => c.MatriculaRemolque).HasColumnName("matricula_remolque").HasMaxLength(20);
        builder.Property(c => c.Conductor).HasColumnName("conductor").HasMaxLength(100);
        builder.Property(c => c.Conductor2).HasColumnName("conductor2").HasMaxLength(100);
        builder.Property(c => c.TemperaturaConsigna).HasColumnName("temperatura_consigna").HasColumnType("numeric(5,1)");
        builder.Property(c => c.Termografo).HasColumnName("termografo").HasMaxLength(40);
        builder.Property(c => c.Naviera).HasColumnName("naviera").HasMaxLength(100);
        builder.Property(c => c.Buque).HasColumnName("buque").HasMaxLength(100);
        builder.Property(c => c.Contenedor).HasColumnName("contenedor").HasMaxLength(20);
        builder.Property(c => c.Precinto).HasColumnName("precinto").HasMaxLength(40);
        builder.Property(c => c.PuertoCarga).HasColumnName("puerto_carga").HasMaxLength(100);
        builder.Property(c => c.PuertoDestino).HasColumnName("puerto_destino").HasMaxLength(100);
        builder.Property(c => c.CompaniaAerea).HasColumnName("compania_aerea").HasMaxLength(100);
        builder.Property(c => c.Vuelo).HasColumnName("vuelo").HasMaxLength(20);
        builder.Property(c => c.Awb).HasColumnName("awb").HasMaxLength(20);
        builder.Property(c => c.Reserva).HasColumnName("reserva").HasMaxLength(40);
        builder.HasIndex(c => c.TransportistaId).HasDatabaseName("ix_carta_porte_transportista");
        builder.HasIndex(c => c.VehiculoId).HasDatabaseName("ix_carta_porte_vehiculo");
        builder.HasIndex(c => c.AlbaranId).HasDatabaseName("ix_carta_porte_albaran");
        builder.Ignore(c => c.TotalPesoNetoKg);
        builder.Ignore(c => c.TotalVolumenM3);

        builder.OwnsMany(c => c.Lineas, linea =>
        {
            linea.ToTable("linea_carta_porte");
            linea.WithOwner().HasForeignKey("carta_porte_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Bultos).HasColumnName("bultos").IsRequired();
            linea.Property(l => l.PesoKg).HasColumnName("peso_kg").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.Marcas).HasColumnName("marcas").HasMaxLength(CartaPorte.LongitudMaximaTexto);
            linea.Property(l => l.Embalaje).HasColumnName("embalaje").HasMaxLength(CartaPorte.LongitudMaximaTexto);
            linea.Property(l => l.PesoNetoKg).HasColumnName("peso_neto_kg").HasColumnType("numeric(14,3)");
            linea.Property(l => l.VolumenM3).HasColumnName("volumen_m3").HasColumnType("numeric(12,3)");
            linea.Property(l => l.CodigoArancelario).HasColumnName("codigo_arancelario").HasMaxLength(10);
        });

        builder.HasIndex(c => new { c.EmpresaId, c.Serie, c.Ejercicio, c.Numero })
            .IsUnique().HasDatabaseName("ux_carta_porte_empresa_serie_ejercicio_numero");
        builder.Ignore(c => c.EventosDominio);
        builder.Ignore(c => c.TotalBultos);
        builder.Ignore(c => c.TotalPesoKg);
    }
}

internal sealed class ConfiguracionTransportista : IEntityTypeConfiguration<Transportista>
{
    public void Configure(EntityTypeBuilder<Transportista> builder)
    {
        builder.ToTable("transportista");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(t => t.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(t => t.Nombre).HasColumnName("nombre").HasMaxLength(Transportista.LongitudTexto).IsRequired();
        builder.Property(t => t.Nif).HasColumnName("nif").HasMaxLength(20);
        builder.Property(t => t.Direccion).HasColumnName("direccion").HasMaxLength(Transportista.LongitudTexto);
        builder.Property(t => t.Pais).HasColumnName("pais").HasMaxLength(2);
        builder.Property(t => t.Telefono).HasColumnName("telefono").HasMaxLength(30);
        builder.Property(t => t.Activo).HasColumnName("activo").IsRequired();
        builder.HasIndex(t => new { t.EmpresaId, t.Nombre }).HasDatabaseName("ix_transportista_empresa_nombre");
        builder.Ignore(t => t.EventosDominio);
    }
}

internal sealed class ConfiguracionVehiculo : IEntityTypeConfiguration<Vehiculo>
{
    public void Configure(EntityTypeBuilder<Vehiculo> builder)
    {
        builder.ToTable("vehiculo");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(v => v.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(v => v.Matricula).HasColumnName("matricula").HasMaxLength(20).IsRequired();
        builder.Property(v => v.MatriculaRemolque).HasColumnName("matricula_remolque").HasMaxLength(20);
        builder.Property(v => v.Descripcion).HasColumnName("descripcion").HasMaxLength(Transportista.LongitudTexto);
        builder.Property(v => v.TaraKg).HasColumnName("tara_kg").HasColumnType("numeric(10,1)");
        builder.Property(v => v.Frigorifico).HasColumnName("frigorifico").IsRequired();
        builder.Property(v => v.TransportistaId).HasColumnName("transportista_id");
        builder.Property(v => v.Activo).HasColumnName("activo").IsRequired();
        builder.HasIndex(v => new { v.EmpresaId, v.Matricula }).IsUnique().HasDatabaseName("ux_vehiculo_empresa_matricula");
        builder.HasIndex(v => v.TransportistaId).HasDatabaseName("ix_vehiculo_transportista");
        builder.Ignore(v => v.EventosDominio);
    }
}

internal sealed class ConfiguracionDespachoAduanero : IEntityTypeConfiguration<DespachoAduanero>
{
    public void Configure(EntityTypeBuilder<DespachoAduanero> builder)
    {
        builder.ToTable("despacho_aduanero");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.FacturaId).HasColumnName("factura_id").IsRequired();
        builder.Property(d => d.Mrn).HasColumnName("mrn").HasMaxLength(DespachoAduanero.LongitudMrn).IsRequired();
        builder.Property(d => d.FechaDespacho).HasColumnName("fecha_despacho").IsRequired();
        builder.Property(d => d.FechaSalida).HasColumnName("fecha_salida");
        builder.Property(d => d.Aduana).HasColumnName("aduana").HasMaxLength(60);
        builder.Property(d => d.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.HasIndex(d => new { d.EmpresaId, d.Mrn }).IsUnique().HasDatabaseName("ux_despacho_empresa_mrn");
        builder.HasIndex(d => d.FacturaId).HasDatabaseName("ix_despacho_factura");
        builder.Ignore(d => d.EventosDominio);
    }
}

internal sealed class ConfiguracionCertificadoFitosanitario : IEntityTypeConfiguration<CertificadoFitosanitario>
{
    public void Configure(EntityTypeBuilder<CertificadoFitosanitario> builder)
    {
        builder.ToTable("certificado_fitosanitario");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.CartaPorteId).HasColumnName("carta_porte_id").IsRequired();
        builder.Property(c => c.Tipo).HasColumnName("tipo").HasMaxLength(25).HasConversion<string>().IsRequired();
        builder.Property(c => c.Numero).HasColumnName("numero").HasMaxLength(40).IsRequired();
        builder.Property(c => c.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(c => c.PaisDestino).HasColumnName("pais_destino").HasMaxLength(2);
        builder.Property(c => c.Organismo).HasColumnName("organismo").HasMaxLength(120);
        builder.Property(c => c.Mercancia).HasColumnName("mercancia").HasMaxLength(200);
        builder.Property(c => c.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        builder.Property(c => c.DocumentoNombre).HasColumnName("documento_nombre").HasMaxLength(150);
        builder.Property(c => c.DocumentoTipo).HasColumnName("documento_tipo").HasMaxLength(40);
        builder.Property(c => c.Documento).HasColumnName("documento");
        builder.HasIndex(c => c.CartaPorteId).HasDatabaseName("ix_certificado_fitosanitario_carta");
        builder.HasIndex(c => new { c.EmpresaId, c.Numero }).HasDatabaseName("ix_certificado_fitosanitario_numero");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class RepositorioCertificadosFitosanitarios : IRepositorioCertificadosFitosanitarios
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioCertificadosFitosanitarios(FacturacionDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<CertificadoFitosanitario>> DeCartaAsync(Guid cartaPorteId, CancellationToken ct = default) =>
        await _contexto.CertificadosFitosanitarios.AsNoTracking().Where(c => c.CartaPorteId == cartaPorteId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<(Guid CartaPorteId, string Tipo, string Numero)>> NumerosAsync(IReadOnlyCollection<Guid> cartas, CancellationToken ct = default) =>
        (await _contexto.CertificadosFitosanitarios.AsNoTracking().Where(c => cartas.Contains(c.CartaPorteId))
            .Select(c => new { c.CartaPorteId, c.Tipo, c.Numero }).ToListAsync(ct).ConfigureAwait(false))
        .Select(c => (c.CartaPorteId, c.Tipo.ToString(), c.Numero)).ToList();

    public Task<CertificadoFitosanitario?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.CertificadosFitosanitarios.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Agregar(CertificadoFitosanitario certificado) => _contexto.CertificadosFitosanitarios.Add(certificado);

    public void Eliminar(CertificadoFitosanitario certificado) => _contexto.CertificadosFitosanitarios.Remove(certificado);
}

internal sealed class RepositorioTransporte : IRepositorioTransporte
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioTransporte(FacturacionDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<Transportista>> TransportistasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Transportistas.Where(t => t.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Transportista?> TransportistaAsync(Guid id, CancellationToken ct = default) => _contexto.Transportistas.SingleOrDefaultAsync(t => t.Id == id, ct);

    public async Task<IReadOnlyList<Vehiculo>> VehiculosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Vehiculos.Where(v => v.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Vehiculo?> VehiculoAsync(Guid id, CancellationToken ct = default) => _contexto.Vehiculos.SingleOrDefaultAsync(v => v.Id == id, ct);

    public void Agregar(object entidad) => _contexto.Add(entidad);

    public void Eliminar(object entidad) => _contexto.Remove(entidad);

    public Task<int> CartasConAsync(Guid? transportistaId, Guid? vehiculoId, CancellationToken ct = default) =>
        _contexto.CartasPorte.CountAsync(c => (transportistaId != null && c.TransportistaId == transportistaId) || (vehiculoId != null && c.VehiculoId == vehiculoId), ct);
}

internal sealed class RepositorioDespachos : IRepositorioDespachos
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioDespachos(FacturacionDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<DespachoAduanero>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default) =>
        await _contexto.Despachos.Where(d => d.FacturaId == facturaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DespachoAduanero>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.Despachos.AsNoTracking().Where(d => d.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<DespachoAduanero?> ObtenerAsync(Guid id, CancellationToken ct = default) => _contexto.Despachos.SingleOrDefaultAsync(d => d.Id == id, ct);

    public Task<bool> ExisteMrnAsync(Guid empresaId, string mrn, Guid? salvo, CancellationToken ct = default) =>
        _contexto.Despachos.AnyAsync(d => d.EmpresaId == empresaId && d.Mrn == mrn && d.Id != salvo, ct);

    public void Agregar(DespachoAduanero despacho) => _contexto.Despachos.Add(despacho);

    public void Eliminar(DespachoAduanero despacho) => _contexto.Despachos.Remove(despacho);
}

internal sealed class ConsultaFacturasPorIva : IConsultaFacturasPorIva
{
    private readonly FacturacionDbContext _contexto;

    public ConsultaFacturasPorIva(FacturacionDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<(Guid Id, string Numero, DateOnly Fecha, string Cliente, Guid? ClienteId, string Pais, decimal Base)>> ConCodigosAsync(
        Guid empresaId, IReadOnlyCollection<string> codigosIva, CancellationToken ct = default)
    {
        var codigos = codigosIva.Select(c => c.ToUpperInvariant()).ToList();
        var filas = await _contexto.Facturas.AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && f.Estado != EstadoFactura.Anulada
                && f.Lineas.Any(l => codigos.Contains(l.CodigoIva.ToUpper())))
            .Select(f => new { f.Id, f.NumeroCompleto, f.FechaEmision, f.ClienteNombre, f.ClienteId, f.Pais,
                Base = f.Lineas.Where(l => codigos.Contains(l.CodigoIva.ToUpper())).Sum(l => l.Base) })
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(f => (f.Id, f.NumeroCompleto, f.FechaEmision, f.ClienteNombre, f.ClienteId, f.Pais, f.Base)).ToList();
    }
}

internal sealed class RepositorioCartasPorte : IRepositorioCartasPorte, IConsultaCartasPorte
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioCartasPorte(FacturacionDbContext contexto) => _contexto = contexto;

    public void Agregar(CartaPorte cartaPorte) => _contexto.CartasPorte.Add(cartaPorte);

    public Task<CartaPorte?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) => _contexto.CartasPorte.SingleOrDefaultAsync(c => c.Id == id, ct);

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
        return carta is null ? null : (await ConCertificadosAsync([CartaPorteDto.Desde(carta)], ct).ConfigureAwait(false))[0];
    }

    public async Task<IReadOnlyList<CartaPorteResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var cartas = await _contexto.CartasPorte.AsNoTracking().Include(c => c.Lineas)
            .Where(c => c.EmpresaId == empresaId)
            .OrderByDescending(c => c.FechaExpedicion).ThenByDescending(c => c.Numero)
            .ToListAsync(ct).ConfigureAwait(false);
        return cartas
            .Select(c => new CartaPorteResumen(c.Id, c.NumeroCompleto, c.FechaExpedicion, c.DestinatarioNombre, c.LugarDestino, c.TotalBultos, c.TotalPesoKg,
                c.LugarOrigen, c.TransportistaNombre, c.Matricula, c.AnuladaEn is not null, c.MotivoAnulacion, c.Tipo.ToString(), c.PaisDestino, c.Incoterm))
            .ToList();
    }

    public async Task<IReadOnlyList<CartaPorteDto>> DeFacturaAsync(Guid facturaId, CancellationToken ct = default)
    {
        var albaranes = _contexto.AlbaranesVenta.Where(a => _contexto.PedidosVenta.Any(p => p.Id == a.PedidoId && p.FacturaId == facturaId)).Select(a => a.Id);
        var cartas = await _contexto.CartasPorte.AsNoTracking().Include(c => c.Lineas)
            .Where(c => c.AlbaranId != null && albaranes.Contains(c.AlbaranId.Value)).OrderBy(c => c.FechaExpedicion).ToListAsync(ct).ConfigureAwait(false);
        return await ConCertificadosAsync(cartas.Select(CartaPorteDto.Desde).ToList(), ct).ConfigureAwait(false);
    }

    /// <summary>Añade a cada carta los certificados fitosanitarios que la acompañan (para el CMR y la aduana).</summary>
    private async Task<IReadOnlyList<CartaPorteDto>> ConCertificadosAsync(IReadOnlyList<CartaPorteDto> cartas, CancellationToken ct)
    {
        var ids = cartas.Select(c => c.Id).ToList();
        var certificados = (await _contexto.CertificadosFitosanitarios.AsNoTracking().Where(c => ids.Contains(c.CartaPorteId))
                .Select(c => new { c.CartaPorteId, c.Tipo, c.Numero, c.FechaEmision }).ToListAsync(ct).ConfigureAwait(false))
            .GroupBy(c => c.CartaPorteId).ToDictionary(g => g.Key, g => g.OrderBy(c => c.FechaEmision).Select(c => c.Tipo switch
            {
                TipoCertificadoFitosanitario.PasaporteFitosanitario => $"Pasaporte fitosanitario nº {c.Numero}",
                TipoCertificadoFitosanitario.Reexportacion => $"Certificado fitosanitario de reexportación nº {c.Numero}",
                _ => $"Certificado fitosanitario nº {c.Numero}",
            }).ToList());
        return cartas.Select(c => certificados.TryGetValue(c.Id, out var l) ? c with { Certificados = l } : c).ToList();
    }
}
