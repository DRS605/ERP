using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Persistencia;
using AlxorCore.Terceros.Aplicacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Facturacion.Infraestructura;

/// <summary>Contexto de persistencia del módulo Facturación.</summary>
public sealed class FacturacionDbContext : DbContextEmpresaBase, IUnidadDeTrabajoFacturacion
{
    public FacturacionDbContext(DbContextOptions<FacturacionDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    public const string Esquema = "facturacion";

    public DbSet<Factura> Facturas => Set<Factura>();

    public DbSet<FacturaRecurrente> FacturasRecurrentes => Set<FacturaRecurrente>();

    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();

    public DbSet<PedidoVenta> PedidosVenta => Set<PedidoVenta>();

    public DbSet<AlbaranVenta> AlbaranesVenta => Set<AlbaranVenta>();

    public DbSet<CartaPorte> CartasPorte => Set<CartaPorte>();

    public DbSet<Transportista> Transportistas => Set<Transportista>();

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();

    public DbSet<DespachoAduanero> Despachos => Set<DespachoAduanero>();

    public DbSet<CertificadoFitosanitario> CertificadosFitosanitarios => Set<CertificadoFitosanitario>();

    public DbSet<MensajeSalida> MensajesSalida => Set<MensajeSalida>();

    public DbSet<DevolucionVenta> DevolucionesVenta => Set<DevolucionVenta>();

    public DbSet<ReclamacionVenta> Reclamaciones => Set<ReclamacionVenta>();

    public DbSet<ConceptoReclamacion> ConceptosReclamacion => Set<ConceptoReclamacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FacturacionDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }
}

internal sealed class ConfiguracionFactura : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.ToTable("factura");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.EmpresaId).HasColumnName("empresa_id").IsRequired();

        builder.Property(f => f.Prefijo).HasColumnName("prefijo").HasMaxLength(10).IsRequired();
        builder.Property(f => f.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(f => f.Numero).HasColumnName("numero").IsRequired();
        builder.Property(f => f.NumeroCompleto).HasColumnName("numero_completo").HasMaxLength(30).IsRequired();
        builder.HasIndex(f => new { f.EmpresaId, f.Prefijo, f.Ejercicio, f.Numero })
            .IsUnique().HasDatabaseName("ux_factura_numero");

        builder.Property(f => f.FechaEmision).HasColumnName("fecha_emision").IsRequired();
        builder.Property(f => f.FechaOperacion).HasColumnName("fecha_operacion").IsRequired();
        builder.Property(f => f.FechaVencimiento).HasColumnName("fecha_vencimiento").IsRequired();

        builder.Property(f => f.ClienteId).HasColumnName("cliente_id");
        builder.Property(f => f.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        builder.Property(f => f.ClienteNif).HasColumnName("cliente_nif").HasMaxLength(20);
        builder.Property(f => f.ClienteCalle).HasColumnName("cliente_calle").HasMaxLength(200);
        builder.Property(f => f.ClienteCodigoPostal).HasColumnName("cliente_cp").HasMaxLength(10);
        builder.Property(f => f.ClientePoblacion).HasColumnName("cliente_poblacion").HasMaxLength(120);
        builder.Property(f => f.ClienteProvincia).HasColumnName("cliente_provincia").HasMaxLength(120);
        builder.Property(f => f.Pais).HasColumnName("pais").HasMaxLength(2).IsRequired();
        builder.Property(f => f.ActividadNegocioId).HasColumnName("actividad_negocio_id");
        builder.Property(f => f.CentroId).HasColumnName("centro_id");
        builder.Property(f => f.CajaId).HasColumnName("caja_id");
        builder.HasIndex(f => f.CentroId).HasDatabaseName("ix_factura_centro");
        builder.Property(f => f.MencionFiscal).HasColumnName("mencion_fiscal").HasMaxLength(500);

        builder.Property(f => f.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(f => f.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(f => f.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(f => f.RetencionIrpf).HasColumnName("retencion_irpf").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(f => f.RecargoEquivalencia).HasColumnName("recargo_equivalencia").IsRequired();
        builder.Property(f => f.RecargoTotal).HasColumnName("recargo_total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(f => f.Total).HasColumnName("total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(f => f.Suplidos).HasColumnName("suplidos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
        builder.Property(f => f.Moneda).HasColumnName("moneda").HasMaxLength(3);
        builder.Property(f => f.TasaCambio).HasColumnName("tasa_cambio").HasColumnType("numeric(18,8)");
        builder.Property(f => f.BaseDivisa).HasColumnName("base_divisa").HasColumnType("numeric(14,2)");
        builder.Property(f => f.CuotaDivisa).HasColumnName("cuota_divisa").HasColumnType("numeric(14,2)");
        builder.Property(f => f.TotalDivisa).HasColumnName("total_divisa").HasColumnType("numeric(14,2)");

        builder.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(f => f.TipoFactura).HasColumnName("tipo_factura").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(f => f.RectificaFacturaId).HasColumnName("rectifica_factura_id");
        builder.Property(f => f.MotivoRectificacion).HasColumnName("motivo_rectificacion").HasMaxLength(300);

        builder.Property(f => f.CreadoEn).HasColumnName("creado_en").IsRequired();

        // Campos VeriFactu/SII reservados (nullable, sin lógica en el MVP).
        builder.Property(f => f.Huella).HasColumnName("huella").HasMaxLength(128);
        builder.Property(f => f.HuellaAnterior).HasColumnName("huella_anterior").HasMaxLength(128);
        builder.Property(f => f.IdRegistro).HasColumnName("id_registro").HasMaxLength(64);
        builder.Property(f => f.TipoOperacion).HasColumnName("tipo_operacion").HasMaxLength(20);
        builder.Property(f => f.EstadoEnvioAeat).HasColumnName("estado_envio_aeat").HasMaxLength(20);
        builder.Property(f => f.Impuesto).HasColumnName("impuesto").HasMaxLength(10).HasConversion<string>().IsRequired()
            .HasDefaultValue(TipoImpuesto.Iva).HasSentinel((TipoImpuesto)0);
        builder.Property(f => f.FechaHoraGenRegistro).HasColumnName("fecha_hora_gen_registro");
        builder.Property(f => f.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(300);
        builder.Property(f => f.HuellaAnulacion).HasColumnName("huella_anulacion").HasMaxLength(128);
        builder.Property(f => f.FechaHoraAnulacion).HasColumnName("fecha_hora_anulacion");

        builder.Ignore(f => f.EventosDominio);

        builder.OwnsMany(f => f.Lineas, linea =>
        {
            linea.ToTable("linea_factura");
            linea.WithOwner().HasForeignKey("factura_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id");
            linea.Property(l => l.EmpresaId).HasColumnName("empresa_id").IsRequired();
            linea.Property(l => l.ProductoId).HasColumnName("producto_id");
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,4)").IsRequired();
            linea.Property(l => l.CosteUnitario).HasColumnName("coste_unitario").HasColumnType("numeric(14,4)").IsRequired();
            linea.Property(l => l.PorcentajeDescuento).HasColumnName("descuento").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
            linea.Property(l => l.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.PorcentajeRecargo).HasColumnName("porcentaje_recargo").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.Base).HasColumnName("base").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.CuotaRecargo).HasColumnName("cuota_recargo").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.Conceptos).ComoConceptos();
            linea.Property(l => l.ImporteConceptos).HasColumnName("importe_conceptos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
            linea.Property(l => l.SuplidosConceptos).HasColumnName("suplidos_conceptos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
            linea.Property(l => l.CosteConceptos).HasColumnName("coste_conceptos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
            linea.Property(l => l.CuentaContable).HasColumnName("cuenta_contable").HasMaxLength(12);
            linea.Property(l => l.Orden).HasColumnName("orden").HasDefaultValue(0).IsRequired();
            linea.Property(l => l.AnticipoId).HasColumnName("anticipo_id");
            linea.HasIndex(l => l.AnticipoId).HasDatabaseName("ix_linea_factura_anticipo");
            linea.Property(l => l.AlbaranVentaId).HasColumnName("albaran_venta_id");
            linea.Property(l => l.EnvaseProductoId).HasColumnName("envase_producto_id");
            linea.Property(l => l.PrecioDivisa).HasColumnName("precio_divisa").HasColumnType("numeric(14,4)");
            linea.Property(l => l.BaseDivisa).HasColumnName("base_divisa").HasColumnType("numeric(14,2)");
            linea.HasIndex(l => l.AlbaranVentaId).HasDatabaseName("ix_linea_factura_albaran_venta");
            linea.Ignore(l => l.CosteTotal);
            linea.Ignore(l => l.SuplidosDivisa);
            linea.Ignore(l => l.Margen);
        });
        builder.Navigation(f => f.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionFacturaRecurrente : IEntityTypeConfiguration<FacturaRecurrente>
{
    public void Configure(EntityTypeBuilder<FacturaRecurrente> builder)
    {
        builder.ToTable("factura_recurrente");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();

        builder.Property(r => r.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        builder.Property(r => r.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(r => r.Periodicidad).HasColumnName("periodicidad").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(r => r.ProximaEmision).HasColumnName("proxima_emision").IsRequired();
        builder.Property(r => r.FechaFin).HasColumnName("fecha_fin");
        builder.Property(r => r.PorcentajeIrpf).HasColumnName("porcentaje_irpf").HasColumnType("numeric(5,2)").IsRequired();
        builder.Property(r => r.Activa).HasColumnName("activa").IsRequired();
        builder.Property(r => r.FacturasGeneradas).HasColumnName("facturas_generadas").IsRequired();
        builder.Property(r => r.UltimaEmision).HasColumnName("ultima_emision");
        builder.Property(r => r.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.HasIndex(r => new { r.EmpresaId, r.Activa, r.ProximaEmision }).HasDatabaseName("ix_recurrente_vencidas");

        builder.Ignore(r => r.EventosDominio);

        builder.OwnsMany(r => r.Lineas, linea =>
        {
            linea.Property(x => x.Orden).HasColumnName("orden").HasDefaultValue(0).IsRequired();
            linea.ToTable("linea_recurrente");
            linea.WithOwner().HasForeignKey("factura_recurrente_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.EmpresaId).HasColumnName("empresa_id").IsRequired();
            linea.Property(l => l.ProductoId).HasColumnName("producto_id");
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,4)").IsRequired();
            linea.Property(l => l.PorcentajeDescuento).HasColumnName("descuento").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
            linea.Property(l => l.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.Base).HasColumnName("base").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.Conceptos).HasColumnName("conceptos").HasColumnType("jsonb")
                .HasConversion(v => ConceptosPlantillaJson.AJson(v), s => ConceptosPlantillaJson.DesdeJsonONulo(s),
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<ConceptoPlantilla>?>((a, b) => ConceptosPlantillaJson.AJson(a) == ConceptosPlantillaJson.AJson(b),
                        v => (ConceptosPlantillaJson.AJson(v) ?? string.Empty).GetHashCode(StringComparison.Ordinal), v => ConceptosPlantillaJson.DesdeJsonONulo(ConceptosPlantillaJson.AJson(v))));
        });
        builder.Navigation(r => r.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Property(r => r.ConceptosDocumento).HasColumnName("conceptos_documento").HasColumnType("jsonb").HasDefaultValueSql("'[]'::jsonb").IsRequired()
            .HasConversion(v => ConceptosPlantillaJson.AJson(v) ?? "[]", s => ConceptosPlantillaJson.DesdeJsonONulo(s) ?? new List<ConceptoPlantilla>(),
                new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<IReadOnlyList<ConceptoPlantilla>>((a, b) => ConceptosPlantillaJson.AJson(a) == ConceptosPlantillaJson.AJson(b),
                    v => (ConceptosPlantillaJson.AJson(v) ?? string.Empty).GetHashCode(StringComparison.Ordinal),
                    v => ConceptosPlantillaJson.DesdeJsonONulo(ConceptosPlantillaJson.AJson(v)) ?? new List<ConceptoPlantilla>()));
    }
}

internal sealed class RepositorioFacturasRecurrentes : IRepositorioFacturasRecurrentes, IConsultaFacturasRecurrentes
{
    private readonly FacturacionDbContext _contexto;
    private readonly IConsultaClientes _clientes;

    public RepositorioFacturasRecurrentes(FacturacionDbContext contexto, IConsultaClientes clientes)
    {
        _contexto = contexto;
        _clientes = clientes;
    }

    public void Agregar(FacturaRecurrente recurrente) => _contexto.FacturasRecurrentes.Add(recurrente);

    public void Eliminar(FacturaRecurrente recurrente) => _contexto.FacturasRecurrentes.Remove(recurrente);

    public Task<FacturaRecurrente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.FacturasRecurrentes.SingleOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<FacturaRecurrente>> ListarVencidasAsync(DateOnly hoy, CancellationToken ct = default)
    {
        return await _contexto.FacturasRecurrentes
            .Where(r => r.Activa && r.ProximaEmision <= hoy && (r.FechaFin == null || r.ProximaEmision <= r.FechaFin))
            .OrderBy(r => r.ProximaEmision)
            .ToListAsync(ct).ConfigureAwait(false);
    }

    // Recorre TODAS las empresas: ignora el filtro multiempresa a propósito (solo lo usa el proceso
    // automático en segundo plano, que luego opera empresa por empresa con su contexto fijado).
    public async Task<IReadOnlyList<Guid>> EmpresasConVencidasAsync(DateOnly hoy, CancellationToken ct = default)
    {
        return await _contexto.FacturasRecurrentes
            .IgnoreQueryFilters()
            .Where(r => r.Activa && r.ProximaEmision <= hoy && (r.FechaFin == null || r.ProximaEmision <= r.FechaFin))
            .Select(r => r.EmpresaId)
            .Distinct()
            .ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<FacturaRecurrenteDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var recurrente = await _contexto.FacturasRecurrentes.SingleOrDefaultAsync(r => r.Id == id, ct).ConfigureAwait(false);
        return recurrente is null ? null : FacturaRecurrenteDto.Desde(recurrente);
    }

    public async Task<IReadOnlyList<FacturaRecurrenteResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var recurrentes = await _contexto.FacturasRecurrentes
            .Where(r => r.EmpresaId == empresaId)
            .OrderByDescending(r => r.Activa).ThenBy(r => r.ProximaEmision)
            .ToListAsync(ct).ConfigureAwait(false);

        var nombresCliente = new Dictionary<Guid, string>();
        var resumenes = new List<FacturaRecurrenteResumen>(recurrentes.Count);
        foreach (var r in recurrentes)
        {
            if (!nombresCliente.TryGetValue(r.ClienteId, out var nombre))
            {
                var cliente = await _clientes.ObtenerAsync(r.ClienteId, ct).ConfigureAwait(false);
                nombre = cliente?.Nombre ?? "—";
                nombresCliente[r.ClienteId] = nombre;
            }

            var dto = FacturaRecurrenteDto.Desde(r);
            resumenes.Add(new FacturaRecurrenteResumen(r.Id, r.Nombre, nombre, r.Periodicidad.ToString(), r.ProximaEmision, r.Activa, dto.Total));
        }

        return resumenes;
    }
}

internal sealed class RepositorioFacturas : IRepositorioFacturas, IConsultaFacturas
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioFacturas(FacturacionDbContext contexto) => _contexto = contexto;

    public Task<Factura?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Facturas.SingleOrDefaultAsync(f => f.Id == id, ct);

    public void Agregar(Factura factura) => _contexto.Facturas.Add(factura);

    /// <summary>Clave del bloqueo de la numeración y de la cadena VeriFactu de una empresa (la usa también el trigger de la base).</summary>
    internal static string ClaveBloqueo(Guid empresaId) => $"alxor.facturacion:{empresaId:D}";

    public async Task<string?> UltimaHuellaAsync(Guid empresaId, CancellationToken ct = default)
    {
        // Encadenar exige leer la última huella y guardar la nueva sin que otra emisión se cuele:
        // se bloquea la cadena de la empresa hasta el guardado.
        await _contexto.BloquearAsync(ClaveBloqueo(empresaId), ct).ConfigureAwait(false);

        // La cadena antifraude incluye los registros de alta y los de anulación de la empresa; se
        // devuelve la huella del más reciente por su instante de generación (solo se lee uno de cada).
        var alta = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.Huella != null)
            .OrderByDescending(f => f.FechaHoraGenRegistro)
            .Select(f => new { Fecha = f.FechaHoraGenRegistro, f.Huella })
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);
        var anulacion = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.HuellaAnulacion != null)
            .OrderByDescending(f => f.FechaHoraAnulacion)
            .Select(f => new { Fecha = f.FechaHoraAnulacion, Huella = f.HuellaAnulacion })
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (anulacion is null)
        {
            return alta?.Huella;
        }

        return alta is null || anulacion.Fecha > alta.Fecha ? anulacion.Huella : alta.Huella;
    }

    public async Task<Resultado<NumeroFactura>> ReservarNumeroAsync(Guid empresaId, string? serie, DateOnly fecha, CancellationToken ct = default)
    {
        var prefijo = string.IsNullOrWhiteSpace(serie) ? "FA" : serie.Trim().ToUpperInvariant();
        if (prefijo.Length > 10)
        {
            return Resultado.Fallo<NumeroFactura>(Error.Validacion("serie.prefijo_largo", "El prefijo de la serie admite como máximo 10 caracteres."));
        }

        await _contexto.BloquearAsync(ClaveBloqueo(empresaId), ct).ConfigureAwait(false);

        var ultima = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.Prefijo == prefijo && f.Ejercicio == fecha.Year)
            .OrderByDescending(f => f.Numero)
            .Select(f => new { f.Numero, f.FechaEmision, f.NumeroCompleto })
            .FirstOrDefaultAsync(ct).ConfigureAwait(false);

        if (ultima is not null && fecha < ultima.FechaEmision)
        {
            return Resultado.Fallo<NumeroFactura>(Error.Conflicto("factura.fecha_no_correlativa",
                $"La última factura de la serie {prefijo} ({ultima.NumeroCompleto}) es del {ultima.FechaEmision:dd/MM/yyyy}: " +
                "una factura posterior en número no puede tener una fecha anterior. Usa esa fecha o una posterior, u otra serie."));
        }

        return Resultado.Ok(new NumeroFactura(prefijo, fecha.Year, (ultima?.Numero ?? 0) + 1));
    }

    public async Task<IReadOnlyList<DesgloseImpuestoDto>> DesgloseImpuestoAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        // Mismas facturas que las autoliquidaciones: las emitidas (no las anuladas ni las sustituidas
        // por una rectificativa, que aporta ya los importes corregidos).
        var lineas = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.Estado == EstadoFactura.Emitida && f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .SelectMany(f => f.Lineas.Select(l => new { f.Impuesto, l.CodigoIva, l.PorcentajeIva, l.Base, l.CuotaIva }))
            .ToListAsync(ct).ConfigureAwait(false);

        return lineas
            .GroupBy(l => (l.Impuesto, Codigo: l.CodigoIva.ToUpperInvariant(), l.PorcentajeIva))
            .Select(g => new DesgloseImpuestoDto(g.Key.Impuesto, g.Key.Codigo, g.Key.PorcentajeIva, g.Sum(l => l.Base), g.Sum(l => l.CuotaIva)))
            .OrderBy(d => d.Impuesto).ThenByDescending(d => d.Porcentaje).ThenBy(d => d.CodigoIva, StringComparer.Ordinal)
            .ToList();
    }

    public async Task<IReadOnlyList<FacturaDto>> EnDivisaAsync(Guid empresaId, DateOnly hasta, CancellationToken ct = default) =>
        (await _contexto.Facturas.AsNoTracking()
            .Where(f => f.EmpresaId == empresaId && f.Moneda != null && f.FechaEmision <= hasta && f.Estado != EstadoFactura.Anulada)
            .ToListAsync(ct).ConfigureAwait(false)).Select(FacturaDto.Desde).ToList();

    public async Task<FacturaDto?> ObtenerAsync(Guid facturaId, CancellationToken ct = default)
    {
        var factura = await _contexto.Facturas.SingleOrDefaultAsync(f => f.Id == facturaId, ct).ConfigureAwait(false);
        return factura is null ? null : FacturaDto.Desde(factura);
    }

    public async Task<IReadOnlyList<FacturaResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var facturas = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId)
            .OrderByDescending(f => f.FechaEmision).ThenByDescending(f => f.Numero)
            .ToListAsync(ct).ConfigureAwait(false);

        return facturas
            .Select(f => new FacturaResumen(
                f.Id, f.NumeroCompleto, f.FechaEmision, f.FechaVencimiento, f.ClienteNombre, f.ClienteNif, f.BaseImponible, f.CuotaIva, f.RetencionIrpf, f.Total, f.Estado.ToString(), f.TipoFactura.ToString(), f.ClienteId, f.ActividadNegocioId, f.Impuesto, f.CentroId, f.CajaId))
            .ToList();
    }

    public async Task<IReadOnlyList<FacturaFiltrada>> FiltradasAsync(Guid empresaId, FiltroFacturas filtro, CancellationToken ct = default)
    {
        var filas = await Filtrar(empresaId, filtro)
            .Select(f => new { f.Id, f.Estado, f.FechaVencimiento, f.BaseImponible, f.CuotaIva, f.RetencionIrpf, f.Total })
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(f => new FacturaFiltrada(f.Id, f.Estado.ToString(), f.FechaVencimiento, f.BaseImponible, f.CuotaIva, f.RetencionIrpf, f.Total)).ToList();
    }

    public async Task<PaginaResultado<FacturaResumen>> BuscarAsync(Guid empresaId, FiltroFacturas filtro, Paginacion paginacion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(paginacion);
        var consulta = Filtrar(empresaId, filtro);

        var total = await consulta.CountAsync(ct).ConfigureAwait(false);
        var d = filtro.Descendente;
        IOrderedQueryable<Factura> ordenada = (filtro.Orden ?? "fecha").Trim().ToUpperInvariant() switch
        {
            "NUMERO" => d ? consulta.OrderByDescending(f => f.Prefijo).ThenByDescending(f => f.Ejercicio).ThenByDescending(f => f.Numero) : consulta.OrderBy(f => f.Prefijo).ThenBy(f => f.Ejercicio).ThenBy(f => f.Numero),
            "CLIENTE" => d ? consulta.OrderByDescending(f => f.ClienteNombre) : consulta.OrderBy(f => f.ClienteNombre),
            "BASE" => d ? consulta.OrderByDescending(f => f.BaseImponible) : consulta.OrderBy(f => f.BaseImponible),
            "IMPUESTOS" => d ? consulta.OrderByDescending(f => f.CuotaIva) : consulta.OrderBy(f => f.CuotaIva),
            "TOTAL" => d ? consulta.OrderByDescending(f => f.Total) : consulta.OrderBy(f => f.Total),
            _ => d ? consulta.OrderByDescending(f => f.FechaEmision) : consulta.OrderBy(f => f.FechaEmision),
        };
        var facturas = await (d ? ordenada.ThenByDescending(f => f.FechaEmision).ThenByDescending(f => f.Numero) : ordenada.ThenBy(f => f.FechaEmision).ThenBy(f => f.Numero))
            .Skip(paginacion.Saltar).Take(paginacion.TamanoPagina)
            .ToListAsync(ct).ConfigureAwait(false);

        var elementos = facturas
            .Select(f => new FacturaResumen(
                f.Id, f.NumeroCompleto, f.FechaEmision, f.FechaVencimiento, f.ClienteNombre, f.ClienteNif, f.BaseImponible, f.CuotaIva, f.RetencionIrpf, f.Total, f.Estado.ToString(), f.TipoFactura.ToString(), f.ClienteId, f.ActividadNegocioId, f.Impuesto, f.CentroId, f.CajaId))
            .ToList();
        return PaginaResultado<FacturaResumen>.Crear(elementos, total, paginacion);
    }

    /// <summary>Consulta de facturas con los filtros del listado aplicados (en la base de datos).</summary>
    private IQueryable<Factura> Filtrar(Guid empresaId, FiltroFacturas filtro)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        var consulta = _contexto.Facturas.Where(f => f.EmpresaId == empresaId);

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var patron = $"%{filtro.Texto.Trim()}%";
            consulta = consulta.Where(f =>
                EF.Functions.ILike(f.NumeroCompleto, patron) ||
                EF.Functions.ILike(f.ClienteNombre, patron) ||
                (f.ClienteNif != null && EF.Functions.ILike(f.ClienteNif, patron)));
        }

        if (!string.IsNullOrWhiteSpace(filtro.Estado) && Enum.TryParse<EstadoFactura>(filtro.Estado, ignoreCase: true, out var estado))
        {
            consulta = consulta.Where(f => f.Estado == estado);
        }

        if (filtro.Desde is DateOnly desde)
        {
            consulta = consulta.Where(f => f.FechaEmision >= desde);
        }

        if (filtro.Hasta is DateOnly hasta)
        {
            consulta = consulta.Where(f => f.FechaEmision <= hasta);
        }

        if (filtro.ImporteMin is decimal min)
        {
            consulta = consulta.Where(f => f.Total >= min);
        }

        if (filtro.ImporteMax is decimal max)
        {
            consulta = consulta.Where(f => f.Total <= max);
        }

        if (filtro.ClienteId is Guid clienteId)
        {
            consulta = consulta.Where(f => f.ClienteId == clienteId);
        }

        if (!string.IsNullOrWhiteSpace(filtro.Serie))
        {
            var serie = filtro.Serie.Trim();
            consulta = consulta.Where(f => f.Prefijo == serie);
        }

        if (filtro.Ids is { } ids)
        {
            var lista = ids.ToList();
            consulta = consulta.Where(f => lista.Contains(f.Id));
        }

        if (filtro.Centros is { } centros)
        {
            var permitidos = centros.ToList();
            consulta = consulta.Where(f => f.CentroId != null && permitidos.Contains(f.CentroId.Value));
        }

        return consulta;
    }

    public async Task<IReadOnlyList<ConceptoDocumentoDto>> ListarConceptosAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var facturas = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.Estado != EstadoFactura.Anulada && f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .ToListAsync(ct).ConfigureAwait(false);
        return facturas
            .SelectMany(f => f.Lineas.SelectMany(l => l.Conceptos.Select(c => new ConceptoDocumentoDto(f.Id, f.NumeroCompleto, f.FechaEmision, f.ClienteNombre, l.Descripcion, c))))
            .ToList();
    }

    public async Task<IReadOnlyList<LineaMargenDto>> ListarLineasMargenAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var facturas = await _contexto.Facturas
            .Where(f => f.EmpresaId == empresaId && f.Estado == EstadoFactura.Emitida && f.FechaEmision >= desde && f.FechaEmision <= hasta)
            .ToListAsync(ct).ConfigureAwait(false);

        return facturas
            .SelectMany(f => f.Lineas.Select(l => new LineaMargenDto(l.ProductoId, l.Descripcion, l.Cantidad, l.Base, l.CosteTotal)))
            .ToList();
    }
}

internal sealed class ConfiguracionPresupuesto : IEntityTypeConfiguration<Presupuesto>
{
    public void Configure(EntityTypeBuilder<Presupuesto> builder)
    {
        builder.ToTable("presupuesto");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CentroId).HasColumnName("centro_id");
        builder.Property(p => p.Moneda).HasColumnName("moneda").HasMaxLength(3);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.NumeroCompleto).HasColumnName("numero_completo").HasMaxLength(30).IsRequired();
        builder.Property(p => p.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(p => p.ClienteNombre).HasColumnName("cliente_nombre").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(p => p.Validez).HasColumnName("validez").IsRequired();
        builder.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.BaseImponible).HasColumnName("base_imponible").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(p => p.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(p => p.Total).HasColumnName("total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(p => p.FacturaId).HasColumnName("factura_id");
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();

        builder.OwnsMany(p => p.Lineas, linea =>
        {
            linea.Property(x => x.Orden).HasColumnName("orden").HasDefaultValue(0).IsRequired();
            linea.ToTable("linea_presupuesto");
            linea.WithOwner().HasForeignKey("presupuesto_id");
            linea.HasKey(l => l.Id);
            linea.Property(l => l.Id).HasColumnName("id").ValueGeneratedNever();
            linea.Property(l => l.EmpresaId).HasColumnName("empresa_id").IsRequired();
            linea.Property(l => l.ProductoId).HasColumnName("producto_id");
            linea.Property(l => l.EnvaseProductoId).HasColumnName("envase_producto_id");
            linea.Property(l => l.Descripcion).HasColumnName("descripcion").HasMaxLength(300).IsRequired();
            linea.Property(l => l.Cantidad).HasColumnName("cantidad").HasColumnType("numeric(14,3)").IsRequired();
            linea.Property(l => l.PrecioUnitario).HasColumnName("precio_unitario").HasColumnType("numeric(14,4)").IsRequired();
            linea.Property(l => l.PorcentajeDescuento).HasColumnName("descuento").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.CodigoIva).HasColumnName("codigo_iva").HasMaxLength(10).IsRequired();
            linea.Property(l => l.PorcentajeIva).HasColumnName("porcentaje_iva").HasColumnType("numeric(5,2)").IsRequired();
            linea.Property(l => l.Base).HasColumnName("base").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.CuotaIva).HasColumnName("cuota_iva").HasColumnType("numeric(14,2)").IsRequired();
            linea.Property(l => l.Conceptos).ComoConceptos();
            linea.Property(l => l.ImporteConceptos).HasColumnName("importe_conceptos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
            linea.Property(l => l.CosteConceptos).HasColumnName("coste_conceptos").HasColumnType("numeric(14,2)").HasDefaultValue(0m).IsRequired();
            linea.Ignore(l => l.BaseBruta);
        });
        builder.Navigation(p => p.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.EmpresaId, p.Fecha }).HasDatabaseName("ix_presupuesto_empresa_fecha");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class RepositorioPresupuestos : IRepositorioPresupuestos, IConsultaPresupuestos
{
    private readonly FacturacionDbContext _contexto;

    public RepositorioPresupuestos(FacturacionDbContext contexto) => _contexto = contexto;

    public Task<Presupuesto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default) =>
        _contexto.Presupuestos.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(Presupuesto presupuesto) => _contexto.Presupuestos.Add(presupuesto);

    public async Task<long> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default)
    {
        var count = await _contexto.Presupuestos
            .Where(p => p.EmpresaId == empresaId && p.Fecha.Year == ejercicio)
            .CountAsync(ct).ConfigureAwait(false);
        return count + 1;
    }

    public async Task<PresupuestoDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _contexto.Presupuestos.SingleOrDefaultAsync(x => x.Id == id, ct).ConfigureAwait(false);
        return p is null ? null : PresupuestoDto.Desde(p);
    }

    public async Task<IReadOnlyList<PresupuestoResumen>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var presupuestos = await _contexto.Presupuestos
            .Where(p => p.EmpresaId == empresaId)
            .OrderByDescending(p => p.Fecha).ThenByDescending(p => p.NumeroCompleto)
            .ToListAsync(ct).ConfigureAwait(false);
        return presupuestos
            .Select(p => new PresupuestoResumen(p.Id, p.NumeroCompleto, p.Fecha, p.Validez, p.ClienteNombre, p.Total, p.Estado.ToString(), p.FacturaId, p.BaseImponible, p.CuotaIva, p.ClienteId, p.CentroId))
            .ToList();
    }
}

/// <summary>Factoría en tiempo de diseño para migraciones.</summary>
public sealed class FacturacionDbContextFactory : IDesignTimeDbContextFactory<FacturacionDbContext>
{
    public FacturacionDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<FacturacionDbContext>().UseNpgsql(conexion).Options;
        return new FacturacionDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
    }

    private sealed class PublicadorInactivo : IPublicadorEventos
    {
        public Task PublicarAsync(IReadOnlyCollection<IEventoDominio> eventos, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class ContextoVacio : IContextoEmpresa
    {
        public Guid? EmpresaId => null;
    }
}

/// <summary>Serialización de los conceptos propios de las plantillas periódicas (null: los automáticos).</summary>
internal static class ConceptosPlantillaJson
{
    private static readonly System.Text.Json.JsonSerializerOptions Opciones = new() { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase };

    public static string? AJson(IReadOnlyList<ConceptoPlantilla>? conceptos) =>
        conceptos is null ? null : System.Text.Json.JsonSerializer.Serialize(conceptos, Opciones);

    public static IReadOnlyList<ConceptoPlantilla>? DesdeJsonONulo(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : System.Text.Json.JsonSerializer.Deserialize<List<ConceptoPlantilla>>(json, Opciones);
}
