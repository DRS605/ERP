using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

internal sealed class ConfiguracionLineaPlanta : IEntityTypeConfiguration<LineaPlanta>
{
    public void Configure(EntityTypeBuilder<LineaPlanta> b)
    {
        Columnas.Base(b, "linea_planta");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(LineaPlanta.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(LineaPlanta.LongitudNombre).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.CapacidadKgHora).HasColumnName("capacidad_kg_hora").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.HorasTurno).HasColumnName("horas_turno").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Turnos).HasColumnName("turnos").IsRequired();
        b.Property(x => x.CentroAnaliticoId).HasColumnName("centro_analitico_id");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Ignore(x => x.CapacidadDia);
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_linea_planta_codigo");
        b.OwnsMany(x => x.Salidas, s =>
        {
            s.ToTable("salida_calibradora");
            s.WithOwner().HasForeignKey("linea_id");
            s.HasKey(x => x.Id);
            s.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            s.Property<Guid>("linea_id").HasColumnName("linea_id");
            s.Property(x => x.Numero).HasColumnName("numero").IsRequired();
            s.Property(x => x.Calibre).HasColumnName("calibre").HasMaxLength(20);
            s.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            s.Property(x => x.Destrio).HasColumnName("destrio").IsRequired();
            s.HasIndex("linea_id", nameof(SalidaCalibradora.Numero)).IsUnique().HasDatabaseName("ux_salida_calibradora_numero");
            s.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_salida_calibradora_categoria");
        });
        b.Navigation(x => x.Salidas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionCalibrado : IEntityTypeConfiguration<Calibrado>
{
    public void Configure(EntityTypeBuilder<Calibrado> b)
    {
        Columnas.Base(b, "calibrado");
        b.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
        b.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.KilosEntrada).HasColumnName("kilos_entrada").HasColumnType(Columnas.Kilos).IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.Referencia).HasColumnName("referencia").HasMaxLength(60);
        b.Property(x => x.ClasificacionId).HasColumnName("clasificacion_id");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.KilosSalida);
        b.Ignore(x => x.Merma);
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_calibrado_fecha");
        b.HasIndex(x => x.PartidaId).HasDatabaseName("ix_calibrado_partida");
        b.HasIndex(x => x.LineaId).HasDatabaseName("ix_calibrado_linea");
        b.HasIndex(x => x.ClasificacionId).HasDatabaseName("ix_calibrado_clasificacion");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_calibrado");
            l.WithOwner().HasForeignKey("calibrado_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("calibrado_id").HasColumnName("calibrado_id");
            l.Property(x => x.Calibre).HasColumnName("calibre").HasMaxLength(20);
            l.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            l.Property(x => x.Destrio).HasColumnName("destrio").IsRequired();
            l.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            l.Property(x => x.Piezas).HasColumnName("piezas");
            l.Ignore(x => x.GramosPieza);
            l.HasIndex("calibrado_id").HasDatabaseName("ix_linea_calibrado_calibrado");
            l.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_linea_calibrado_categoria");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionOrdenLinea : IEntityTypeConfiguration<OrdenLinea>
{
    public void Configure(EntityTypeBuilder<OrdenLinea> b)
    {
        Columnas.Base(b, "orden_linea");
        b.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Turno).HasColumnName("turno").IsRequired();
        b.Property(x => x.Secuencia).HasColumnName("secuencia").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Cajas).HasColumnName("cajas");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.PedidoVentaId).HasColumnName("pedido_venta_id");
        b.Property(x => x.LineaPlanId).HasColumnName("linea_plan_id");
        b.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(200);
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.ParteConfeccionId).HasColumnName("parte_confeccion_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_orden_linea_fecha");
        b.HasIndex(x => x.LineaId).HasDatabaseName("ix_orden_linea_linea");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_orden_linea_producto");
        b.HasIndex(x => x.ClienteId).HasDatabaseName("ix_orden_linea_cliente");
        b.HasIndex(x => x.PedidoVentaId).HasDatabaseName("ix_orden_linea_pedido");
        b.HasIndex(x => x.LineaPlanId).HasDatabaseName("ix_orden_linea_plan");
        b.HasIndex(x => x.ParteConfeccionId).IsUnique().HasDatabaseName("ux_orden_linea_parte");
    }
}

internal sealed class ConfiguracionParadaLinea : IEntityTypeConfiguration<ParadaLinea>
{
    public void Configure(EntityTypeBuilder<ParadaLinea> b)
    {
        Columnas.Base(b, "parada_linea");
        b.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Turno).HasColumnName("turno").IsRequired();
        b.Property(x => x.Minutos).HasColumnName("minutos").HasColumnType("numeric(7,2)").IsRequired();
        Columnas.Enum(b.Property(x => x.Motivo), "motivo");
        b.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(200);
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_parada_linea_fecha");
        b.HasIndex(x => x.LineaId).HasDatabaseName("ix_parada_linea_linea");
    }
}

internal sealed class RepositorioPlanta : IRepositorioPlanta
{
    private readonly AgroDbContext _ctx;

    public RepositorioPlanta(AgroDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<LineaPlanta>> LineasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<LineaPlanta>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<LineaPlanta?> LineaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<LineaPlanta>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<bool> LineaEnUsoAsync(Guid id, CancellationToken ct = default) =>
        await _ctx.Set<Calibrado>().AnyAsync(x => x.LineaId == id, ct).ConfigureAwait(false)
        || await _ctx.Set<OrdenLinea>().AnyAsync(x => x.LineaId == id, ct).ConfigureAwait(false)
        || await _ctx.Set<ParadaLinea>().AnyAsync(x => x.LineaId == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Calibrado>> CalibradosAsync(Guid empresaId, Guid? partidaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _ctx.Set<Calibrado>().Where(x => x.EmpresaId == empresaId && (partidaId == null || x.PartidaId == partidaId)
            && (desde == null || x.Fecha >= desde) && (hasta == null || x.Fecha <= hasta)).ToListAsync(ct).ConfigureAwait(false);

    public Task<Calibrado?> CalibradoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Calibrado>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<OrdenLinea>> OrdenesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Set<OrdenLinea>().Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha <= hasta).ToListAsync(ct).ConfigureAwait(false);

    public Task<OrdenLinea?> OrdenAsync(Guid id, CancellationToken ct = default) => _ctx.Set<OrdenLinea>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ParadaLinea>> ParadasAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Set<ParadaLinea>().Where(x => x.EmpresaId == empresaId && x.Fecha >= desde && x.Fecha <= hasta).ToListAsync(ct).ConfigureAwait(false);

    public Task<ParadaLinea?> ParadaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ParadaLinea>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);
}
