using AlxorCore.Agro.Aplicacion;
using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Agro.Infraestructura;

/// <summary>Contexto de persistencia del módulo agro (esquema <c>agro</c>).</summary>
public sealed class AgroDbContext : DbContextEmpresaBase, IUnidadDeTrabajoAgro
{
    public const string Esquema = "agro";

    public AgroDbContext(DbContextOptions<AgroDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    /// <summary>Tablas raíz del módulo (las líneas caen en cascada con su cabecera).</summary>
    private const string SqlBorradoEmpresa = """
        DELETE FROM agro.rectificacion_recepcion WHERE empresa_id = {0};
        DELETE FROM agro.etiqueta_campo WHERE empresa_id = {0};
        DELETE FROM agro.muestreo_calidad WHERE empresa_id = {0};
        DELETE FROM agro.plantilla_calidad WHERE empresa_id = {0};
        DELETE FROM agro.correccion_expedicion WHERE empresa_id = {0};
        DELETE FROM agro.tolerancia_merma_familia WHERE empresa_id = {0};
        DELETE FROM agro.repaletizado WHERE empresa_id = {0};
        DELETE FROM agro.regla_transformacion WHERE empresa_id = {0};
        DELETE FROM agro.descalificacion_partida WHERE empresa_id = {0};
        DELETE FROM agro.tara_envase WHERE empresa_id = {0};
        DELETE FROM agro.certificado_agro WHERE empresa_id = {0};
        DELETE FROM agro.declaracion_articulo WHERE empresa_id = {0};
        DELETE FROM agro.genealogia WHERE empresa_id = {0};
        DELETE FROM agro.movimiento_partida WHERE empresa_id = {0};
        DELETE FROM agro.movimiento_envase WHERE empresa_id = {0};
        DELETE FROM agro.movimiento_envases WHERE empresa_id = {0};
        DELETE FROM agro.reserva_pale WHERE empresa_id = {0};
        DELETE FROM agro.cuenta_envases WHERE empresa_id = {0};
        DELETE FROM agro.configuracion_envases WHERE empresa_id = {0};
        DELETE FROM agro.clasificacion WHERE empresa_id = {0};
        DELETE FROM agro.liquidacion WHERE empresa_id = {0};
        DELETE FROM agro.parte_confeccion WHERE empresa_id = {0};
        DELETE FROM agro.partida WHERE empresa_id = {0};
        DELETE FROM agro.orden_carga WHERE empresa_id = {0};
        DELETE FROM agro.pale WHERE empresa_id = {0};
        DELETE FROM agro.plantilla_pale WHERE empresa_id = {0};
        DELETE FROM agro.recepcion WHERE empresa_id = {0};
        DELETE FROM agro.precio_liquidacion WHERE empresa_id = {0};
        DELETE FROM agro.articulo_campana WHERE empresa_id = {0};
        DELETE FROM agro.concepto_liquidacion WHERE empresa_id = {0};
        DELETE FROM agro.tarifa_coste WHERE empresa_id = {0};
        DELETE FROM agro.tratamiento_parcela WHERE empresa_id = {0};
        DELETE FROM agro.parcela WHERE empresa_id = {0};
        DELETE FROM agro.agricultor WHERE empresa_id = {0};
        DELETE FROM agro.categoria WHERE empresa_id = {0};
        DELETE FROM agro.campana WHERE empresa_id = {0};
        DELETE FROM agro.configuracion WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgroDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todos los datos agro de la empresa (solo dentro de <c>BorradoEmpresa</c>, que lo autoriza).</summary>
    public Task BorrarEmpresaAsync(Guid empresaId, CancellationToken ct = default) =>
        Database.ExecuteSqlRawAsync(SqlBorradoEmpresa, [empresaId], ct);
}

internal static class Columnas
{
    public const string Kilos = "numeric(12,3)";
    public const string Importe = "numeric(14,2)";
    public const string Porcentaje = "numeric(7,2)";
    public const string PrecioKg = "numeric(12,6)";
    public const string Unitario = "numeric(12,4)";

    public static void Base<T>(EntityTypeBuilder<T> b, string tabla)
        where T : RaizAgregadoEmpresa<Guid>
    {
        b.ToTable(tabla);
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        b.Property(x => x.EmpresaId).HasColumnName("empresa_id").IsRequired();
        b.Ignore(x => x.EventosDominio);
    }

    public static PropertyBuilder<TEnum> Enum<TEnum>(PropertyBuilder<TEnum> p, string columna) =>
        p.HasColumnName(columna).HasMaxLength(30).HasConversion<string>().IsRequired();
}

internal sealed class ConfiguracionCampana : IEntityTypeConfiguration<Campana>
{
    public void Configure(EntityTypeBuilder<Campana> b)
    {
        Columnas.Base(b, "campana");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_campana_codigo");
    }
}

internal sealed class ConfiguracionAgricultor : IEntityTypeConfiguration<Agricultor>
{
    public void Configure(EntityTypeBuilder<Agricultor> b)
    {
        Columnas.Base(b, "agricultor");
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        Columnas.Enum(b.Property(x => x.Regimen), "regimen");
        b.Property(x => x.CodigoImpuesto).HasColumnName("codigo_impuesto").HasMaxLength(10).IsRequired();
        b.Property(x => x.PorcentajeRetencion).HasColumnName("porcentaje_retencion").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.AutofacturacionDesde).HasColumnName("autofacturacion_desde");
        b.Property(x => x.MotivoBloqueo).HasColumnName("motivo_bloqueo").HasMaxLength(200);
        b.Ignore(x => x.Bloqueado);
        b.HasIndex(x => new { x.EmpresaId, x.ProveedorId }).IsUnique().HasDatabaseName("ux_agricultor_proveedor");
    }
}

internal sealed class ConfiguracionParcela : IEntityTypeConfiguration<Parcela>
{
    public void Configure(EntityTypeBuilder<Parcela> b)
    {
        Columnas.Base(b, "parcela");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
        b.Property(x => x.ReferenciaSigpac).HasColumnName("referencia_sigpac").HasMaxLength(60);
        b.Property(x => x.SuperficieHa).HasColumnName("superficie_ha").HasColumnType("numeric(10,4)");
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.Variedad).HasColumnName("variedad").HasMaxLength(80);
        b.Property(x => x.CentroAnaliticoId).HasColumnName("centro_analitico_id");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_parcela_codigo");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_parcela_agricultor");
    }
}

internal sealed class ConfiguracionCategoria : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> b)
    {
        Columnas.Base(b, "categoria");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
        b.Property(x => x.EsDestrio).HasColumnName("es_destrio").IsRequired();
        b.Property(x => x.Orden).HasColumnName("orden").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_categoria_codigo");
    }
}

internal sealed class ConfiguracionArticuloCampana : IEntityTypeConfiguration<ArticuloCampana>
{
    public void Configure(EntityTypeBuilder<ArticuloCampana> b)
    {
        Columnas.Base(b, "articulo_campana");
        b.Property(x => x.CampanaId).HasColumnName("campana_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        Columnas.Enum(b.Property(x => x.Metodo), "metodo");
        b.HasIndex(x => new { x.CampanaId, x.ProductoId }).IsUnique().HasDatabaseName("ux_articulo_campana");
    }
}

internal sealed class ConfiguracionPrecio : IEntityTypeConfiguration<PrecioLiquidacion>
{
    public void Configure(EntityTypeBuilder<PrecioLiquidacion> b)
    {
        Columnas.Base(b, "precio_liquidacion");
        b.Property(x => x.CampanaId).HasColumnName("campana_id").IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.CategoriaId).HasColumnName("categoria_id");
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        b.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg).IsRequired();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(10).HasConversion<string>().IsRequired().HasDefaultValue(TipoPrecioLiquidacion.Periodo);
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
        b.HasIndex(x => new { x.CampanaId, x.ProductoId, x.Desde }).HasDatabaseName("ix_precio_liquidacion_campana");
        b.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_precio_liquidacion_categoria");
    }
}

internal sealed class ConfiguracionConcepto : IEntityTypeConfiguration<ConceptoLiquidacion>
{
    public void Configure(EntityTypeBuilder<ConceptoLiquidacion> b)
    {
        Columnas.Base(b, "concepto_liquidacion");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Valor).HasColumnName("valor").HasColumnType(Columnas.PrecioKg).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").IsRequired();
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id");
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
        b.Property(x => x.Abono).HasColumnName("abono").IsRequired();
        b.Ignore(x => x.Filtrado);
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_concepto_liquidacion_codigo");
    }
}

internal sealed class ConfiguracionRendimiento : IEntityTypeConfiguration<RendimientoConfeccion>
{
    public void Configure(EntityTypeBuilder<RendimientoConfeccion> b)
    {
        Columnas.Base(b, "rendimiento_confeccion");
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
        b.Property(x => x.CajasHora).HasColumnName("cajas_hora").HasColumnType("numeric(10,2)").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId, x.EnvaseProductoId }).IsUnique().AreNullsDistinct(false).HasDatabaseName("ux_rendimiento_confeccion");
    }
}

internal sealed class ConfiguracionTarifa : IEntityTypeConfiguration<TarifaCoste>
{
    public void Configure(EntityTypeBuilder<TarifaCoste> b)
    {
        Columnas.Base(b, "tarifa_coste");
        Columnas.Enum(b.Property(x => x.Recurso), "recurso");
        b.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        Columnas.Enum(b.Property(x => x.TipoHora), "tipo_hora");
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta");
        b.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType(Columnas.Unitario).IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Recurso, x.Categoria, x.TipoHora, x.Desde }).HasDatabaseName("ix_tarifa_coste_busqueda");
    }
}

internal sealed class ConfiguracionAjustes : IEntityTypeConfiguration<ConfiguracionAgro>
{
    public void Configure(EntityTypeBuilder<ConfiguracionAgro> b)
    {
        Columnas.Base(b, "configuracion");
        b.Property(x => x.PrefijoGs1).HasColumnName("prefijo_gs1").HasMaxLength(10).IsRequired();
        b.Property(x => x.ReflejarPartidasEnInventario).HasColumnName("reflejar_partidas_inventario").IsRequired();
        b.Property(x => x.ReflejarEnvasesEnInventario).HasColumnName("reflejar_envases_inventario").IsRequired();
        b.Property(x => x.DigitoExtension).HasColumnName("digito_extension").IsRequired();
        b.Property(x => x.ToleranciaMermaPct).HasColumnName("tolerancia_merma_pct").HasColumnType("numeric(5,2)");
        b.Property(x => x.CertificacionPorParcela).HasColumnName("certificacion_por_parcela").IsRequired();
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_empresa");
    }
}

internal sealed class ConfiguracionRecepcion : IEntityTypeConfiguration<Recepcion>
{
    public void Configure(EntityTypeBuilder<Recepcion> b)
    {
        Columnas.Base(b, "recepcion");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.CampanaId).HasColumnName("campana_id").IsRequired();
        b.Property(x => x.Matricula).HasColumnName("matricula").HasMaxLength(Recepcion.LongitudTexto);
        b.Property(x => x.Conductor).HasColumnName("conductor").HasMaxLength(Recepcion.LongitudTexto);
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(Recepcion.LongitudTexto);
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(Recepcion.LongitudTexto);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.ConfirmadaEn).HasColumnName("confirmada_en");
        b.Property(x => x.AnuladaEn).HasColumnName("anulada_en");
        b.Ignore(x => x.NumeroCompleto);
        b.Ignore(x => x.NetoKg);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_recepcion_numero");
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_recepcion_fecha");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_recepcion_agricultor");
        b.HasIndex(x => x.CampanaId).HasDatabaseName("ix_recepcion_campana");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_recepcion");
            l.WithOwner().HasForeignKey("recepcion_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("recepcion_id").HasColumnName("recepcion_id");
            l.Property(x => x.NumeroLinea).HasColumnName("numero_linea").IsRequired();
            l.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            l.Property(x => x.ProductoNombre).HasColumnName("producto_nombre").HasMaxLength(200).IsRequired();
            l.Property(x => x.ParcelaId).HasColumnName("parcela_id");
            l.Property(x => x.FechaRecoleccion).HasColumnName("fecha_recoleccion");
            l.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
            l.Property(x => x.PrecioEstimadoKg).HasColumnName("precio_estimado_kg").HasColumnType(Columnas.PrecioKg);
            l.Property(x => x.Calibre).HasColumnName("calibre").HasMaxLength(30);
            l.Property(x => x.MotivoDescalificacion).HasColumnName("motivo_descalificacion").HasMaxLength(300);
            l.Property(x => x.KilosLiquidacion).HasColumnName("kilos_liquidacion").HasColumnType(Columnas.Kilos);
            l.Property(x => x.MotivoKilosLiquidacion).HasColumnName("motivo_kilos_liquidacion").HasMaxLength(300);
            l.Ignore(x => x.KilosALiquidar);
            l.Property(x => x.PartidaId).HasColumnName("partida_id");
            l.Property(x => x.NetoKg).HasColumnName("neto_kg").HasColumnType(Columnas.Kilos);
            l.Property(x => x.Envases).HasColumnName("envases");
            l.HasIndex("recepcion_id", nameof(LineaRecepcion.NumeroLinea)).IsUnique().HasDatabaseName("ux_linea_recepcion_numero");
            l.HasIndex(x => x.ParcelaId).HasDatabaseName("ix_linea_recepcion_parcela");
            l.HasIndex(x => x.PartidaId).HasDatabaseName("ix_linea_recepcion_partida");
        });
        b.OwnsMany(x => x.Pesadas, p =>
        {
            p.ToTable("pesada");
            p.WithOwner().HasForeignKey("recepcion_id");
            p.HasKey(x => x.Id);
            p.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            p.Property<Guid>("recepcion_id").HasColumnName("recepcion_id");
            p.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
            p.Property(x => x.Secuencia).HasColumnName("secuencia").IsRequired();
            p.Property(x => x.BrutoKg).HasColumnName("bruto_kg").HasColumnType(Columnas.Kilos).IsRequired();
            p.Property(x => x.TaraKg).HasColumnName("tara_kg").HasColumnType(Columnas.Kilos).IsRequired();
            p.Property(x => x.Envases).HasColumnName("envases").IsRequired();
            p.Property(x => x.Bascula).HasColumnName("bascula").HasMaxLength(60);
            p.Property(x => x.TaraCamionKg).HasColumnName("tara_camion_kg").HasColumnType(Columnas.Kilos);
            p.Property(x => x.TaraEnvasesKg).HasColumnName("tara_envases_kg").HasColumnType(Columnas.Kilos).HasDefaultValue(0m).IsRequired();
            p.Property(x => x.GrupoCamion).HasColumnName("grupo_camion");
            p.Property(x => x.BrutoCamionKg).HasColumnName("bruto_camion_kg").HasColumnType(Columnas.Kilos);
            p.Ignore(x => x.NetoKg);
            p.HasIndex("recepcion_id").HasDatabaseName("ix_pesada_recepcion");
            p.HasIndex(x => new { x.LineaId, x.Secuencia }).IsUnique().HasDatabaseName("ux_pesada_secuencia");
        });
        b.OwnsMany(x => x.EnvasesPesadas, e =>
        {
            e.ToTable("pesada_envase");
            e.WithOwner().HasForeignKey("recepcion_id");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property<Guid>("recepcion_id").HasColumnName("recepcion_id");
            e.Property(x => x.PesadaId).HasColumnName("pesada_id").IsRequired();
            e.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
            e.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired();
            e.Property(x => x.TaraUnitariaKg).HasColumnName("tara_unitaria_kg").HasColumnType(Columnas.Kilos).IsRequired();
            e.Property(x => x.TaraEnvaseId).HasColumnName("tara_envase_id");
            e.Ignore(x => x.TaraKg);
            e.HasIndex("recepcion_id").HasDatabaseName("ix_pesada_envase_recepcion");
            e.HasIndex(x => new { x.PesadaId, x.EnvaseProductoId }).IsUnique().HasDatabaseName("ux_pesada_envase_tipo");
            e.HasIndex(x => x.EnvaseProductoId).HasDatabaseName("ix_pesada_envase_envase");
            e.HasIndex(x => x.TaraEnvaseId).HasDatabaseName("ix_pesada_envase_tara");
        });
        b.OwnsMany(x => x.PalesEntrada, e =>
        {
            e.ToTable("pale_entrada");
            e.WithOwner().HasForeignKey("recepcion_id");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            e.Property<Guid>("recepcion_id").HasColumnName("recepcion_id");
            e.Property(x => x.LineaId).HasColumnName("linea_id").IsRequired();
            e.Property(x => x.Numero).HasColumnName("numero").IsRequired();
            e.Property(x => x.SerieOrigen).HasColumnName("serie_origen").HasMaxLength(60);
            e.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
            e.Property(x => x.Envases).HasColumnName("envases").IsRequired();
            e.Property(x => x.KilosNetos).HasColumnName("kilos_netos").HasColumnType(Columnas.Kilos);
            e.Property(x => x.PaleId).HasColumnName("pale_id");
            e.Property(x => x.KilosAsignados).HasColumnName("kilos_asignados").HasColumnType(Columnas.Kilos);
            e.HasIndex("recepcion_id").HasDatabaseName("ix_pale_entrada_recepcion");
            e.HasIndex(x => new { x.LineaId, x.Numero }).IsUnique().HasDatabaseName("ux_pale_entrada_numero");
            e.HasIndex(x => x.PaleId).IsUnique().HasDatabaseName("ux_pale_entrada_pale");
            e.HasIndex(x => x.EnvaseProductoId).HasDatabaseName("ix_pale_entrada_envase");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Pesadas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.EnvasesPesadas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.PalesEntrada).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionPlantillaCalidad : IEntityTypeConfiguration<PlantillaCalidad>
{
    public void Configure(EntityTypeBuilder<PlantillaCalidad> b)
    {
        Columnas.Base(b, "plantilla_calidad");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.FamiliaId).HasColumnName("familia_id");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_plantilla_calidad_codigo");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_plantilla_calidad_producto");
        b.HasIndex(x => x.FamiliaId).HasDatabaseName("ix_plantilla_calidad_familia");
        b.OwnsMany(x => x.Defectos, d =>
        {
            d.ToTable("defecto_calidad");
            d.WithOwner().HasForeignKey("plantilla_id");
            d.HasKey(x => x.Id);
            d.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            d.Property<Guid>("plantilla_id").HasColumnName("plantilla_id");
            d.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(80).IsRequired();
            d.Property(x => x.Orden).HasColumnName("orden").IsRequired();
            d.Property(x => x.DescuentaPeso).HasColumnName("descuenta_peso").IsRequired();
            d.Property(x => x.ToleranciaPct).HasColumnName("tolerancia_pct").HasColumnType("numeric(5,2)");
            d.Property(x => x.MaximoPct).HasColumnName("maximo_pct").HasColumnType("numeric(5,2)");
            d.HasIndex("plantilla_id").HasDatabaseName("ix_defecto_calidad_plantilla");
        });
        b.Navigation(x => x.Defectos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionMuestreoCalidad : IEntityTypeConfiguration<MuestreoCalidad>
{
    public void Configure(EntityTypeBuilder<MuestreoCalidad> b)
    {
        Columnas.Base(b, "muestreo_calidad");
        b.Property(x => x.RecepcionId).HasColumnName("recepcion_id").IsRequired();
        b.Property(x => x.LineaRecepcionId).HasColumnName("linea_recepcion_id").IsRequired();
        b.Property(x => x.PlantillaId).HasColumnName("plantilla_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.PesoMuestraKg).HasColumnName("peso_muestra_kg").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Definitivo).HasColumnName("definitivo").IsRequired();
        b.Property(x => x.DescuentoPct).HasColumnName("descuento_pct").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.Anulado).HasColumnName("anulado").IsRequired();
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(300);
        b.HasIndex(x => x.RecepcionId).HasDatabaseName("ix_muestreo_calidad_recepcion");
        b.HasIndex(x => x.LineaRecepcionId).HasDatabaseName("ix_muestreo_calidad_linea");
        b.HasIndex(x => x.PlantillaId).HasDatabaseName("ix_muestreo_calidad_plantilla");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_muestreo_calidad_usuario");
        b.HasIndex(x => x.LineaRecepcionId).IsUnique().HasFilter("definitivo AND NOT anulado").HasDatabaseName("ux_muestreo_calidad_definitivo");
        b.OwnsMany(x => x.Resultados, r =>
        {
            r.ToTable("resultado_muestreo");
            r.WithOwner().HasForeignKey("muestreo_id");
            r.HasKey(x => x.Id);
            r.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            r.Property<Guid>("muestreo_id").HasColumnName("muestreo_id");
            r.Property(x => x.DefectoId).HasColumnName("defecto_id").IsRequired();
            r.Property(x => x.Defecto).HasColumnName("defecto").HasMaxLength(80).IsRequired();
            r.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            r.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)").IsRequired();
            r.Property(x => x.DescuentaPeso).HasColumnName("descuenta_peso").IsRequired();
            r.HasIndex("muestreo_id").HasDatabaseName("ix_resultado_muestreo_muestreo");
            r.HasIndex(x => x.DefectoId).HasDatabaseName("ix_resultado_muestreo_defecto");
        });
        b.Navigation(x => x.Resultados).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionEtiquetaCampo : IEntityTypeConfiguration<EtiquetaCampo>
{
    public void Configure(EntityTypeBuilder<EtiquetaCampo> b)
    {
        Columnas.Base(b, "etiqueta_campo");
        b.Property(x => x.Sscc).HasColumnName("sscc").HasMaxLength(18).IsRequired();
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id");
        b.Property(x => x.EmitidaEn).HasColumnName("emitida_en").IsRequired();
        b.Property(x => x.PaleId).HasColumnName("pale_id");
        b.Ignore(x => x.Usada);
        b.HasIndex(x => new { x.EmpresaId, x.Sscc }).IsUnique().HasDatabaseName("ux_etiqueta_campo_sscc");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_etiqueta_campo_agricultor");
        b.HasIndex(x => x.ParcelaId).HasDatabaseName("ix_etiqueta_campo_parcela");
        b.HasIndex(x => x.PaleId).IsUnique().HasDatabaseName("ux_etiqueta_campo_pale");
    }
}

internal sealed class ConfiguracionRectificacion : IEntityTypeConfiguration<RectificacionRecepcion>
{
    public void Configure(EntityTypeBuilder<RectificacionRecepcion> b)
    {
        Columnas.Base(b, "rectificacion_recepcion");
        b.Property(x => x.RecepcionId).HasColumnName("recepcion_id").IsRequired();
        b.Property(x => x.LineaRecepcionId).HasColumnName("linea_recepcion_id").IsRequired();
        b.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.NetoAnteriorKg).HasColumnName("neto_anterior_kg").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.DiferenciaKg).HasColumnName("diferencia_kg").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.KilosLiquidacion).HasColumnName("kilos_liquidacion").HasColumnType(Columnas.Kilos);
        b.Property(x => x.BrutoKg).HasColumnName("bruto_kg").HasColumnType(Columnas.Kilos);
        b.Property(x => x.TaraKg).HasColumnName("tara_kg").HasColumnType(Columnas.Kilos);
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(300).IsRequired();
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Ignore(x => x.NetoNuevoKg);
        b.HasIndex(x => x.RecepcionId).HasDatabaseName("ix_rectificacion_recepcion_recepcion");
        b.HasIndex(x => x.LineaRecepcionId).HasDatabaseName("ix_rectificacion_recepcion_linea");
        b.HasIndex(x => x.PartidaId).HasDatabaseName("ix_rectificacion_recepcion_partida");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_rectificacion_recepcion_usuario");
    }
}

internal sealed class ConfiguracionCorreccionExpedicion : IEntityTypeConfiguration<CorreccionExpedicion>
{
    public void Configure(EntityTypeBuilder<CorreccionExpedicion> b)
    {
        Columnas.Base(b, "correccion_expedicion");
        b.Property(x => x.PaleId).HasColumnName("pale_id").IsRequired();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(20).IsRequired();
        b.Property(x => x.ClienteAnteriorId).HasColumnName("cliente_anterior_id");
        b.Property(x => x.ClienteNuevoId).HasColumnName("cliente_nuevo_id");
        b.Property(x => x.ReferenciaAnterior).HasColumnName("referencia_anterior").HasMaxLength(80);
        b.Property(x => x.ReferenciaNueva).HasColumnName("referencia_nueva").HasMaxLength(80);
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(300);
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.En).HasColumnName("en").IsRequired();
        b.HasIndex(x => x.PaleId).HasDatabaseName("ix_correccion_expedicion_pale");
        b.HasIndex(x => x.ClienteAnteriorId).HasDatabaseName("ix_correccion_expedicion_cliente_anterior");
        b.HasIndex(x => x.ClienteNuevoId).HasDatabaseName("ix_correccion_expedicion_cliente_nuevo");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_correccion_expedicion_usuario");
    }
}

internal sealed class ConfiguracionToleranciaMerma : IEntityTypeConfiguration<ToleranciaMermaFamilia>
{
    public void Configure(EntityTypeBuilder<ToleranciaMermaFamilia> b)
    {
        Columnas.Base(b, "tolerancia_merma_familia");
        b.Property(x => x.FamiliaId).HasColumnName("familia_id").IsRequired();
        b.Property(x => x.MermaMaximaPct).HasColumnName("merma_maxima_pct").HasColumnType("numeric(5,2)").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.FamiliaId }).IsUnique().HasDatabaseName("ux_tolerancia_merma_familia");
        b.HasIndex(x => x.FamiliaId).HasDatabaseName("ix_tolerancia_merma_familia_familia");
    }
}

internal sealed class ConfiguracionReglaTransformacion : IEntityTypeConfiguration<ReglaTransformacion>
{
    public void Configure(EntityTypeBuilder<ReglaTransformacion> b)
    {
        Columnas.Base(b, "regla_transformacion");
        b.Property(x => x.ProductoOrigenId).HasColumnName("producto_origen_id").IsRequired();
        b.Property(x => x.ProductoDestinoId).HasColumnName("producto_destino_id").IsRequired();
        b.Property(x => x.MermaMaximaPct).HasColumnName("merma_maxima_pct").HasColumnType("numeric(5,2)");
        b.HasIndex(x => new { x.EmpresaId, x.ProductoOrigenId, x.ProductoDestinoId }).IsUnique().HasDatabaseName("ux_regla_transformacion");
        b.HasIndex(x => x.ProductoOrigenId).HasDatabaseName("ix_regla_transformacion_origen");
        b.HasIndex(x => x.ProductoDestinoId).HasDatabaseName("ix_regla_transformacion_destino");
    }
}

internal sealed class ConfiguracionRepaletizado : IEntityTypeConfiguration<Repaletizado>
{
    public void Configure(EntityTypeBuilder<Repaletizado> b)
    {
        Columnas.Base(b, "repaletizado");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.DestinoPaleId).HasColumnName("destino_pale_id").IsRequired();
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(300);
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.Kilos);
        b.HasIndex(x => x.DestinoPaleId).HasDatabaseName("ix_repaletizado_destino");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_repaletizado_usuario");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_repaletizado");
            l.WithOwner().HasForeignKey("repaletizado_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.OrigenPaleId).HasColumnName("origen_pale_id").IsRequired();
            l.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
            l.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            l.Property(x => x.Cajas).HasColumnName("cajas").IsRequired();
            l.HasIndex(x => x.OrigenPaleId).HasDatabaseName("ix_linea_repaletizado_origen");
            l.HasIndex(x => x.PartidaId).HasDatabaseName("ix_linea_repaletizado_partida");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionTaraEnvase : IEntityTypeConfiguration<TaraEnvase>
{
    public void Configure(EntityTypeBuilder<TaraEnvase> b)
    {
        Columnas.Base(b, "tara_envase");
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
        b.Property(x => x.TaraKg).HasColumnName("tara_kg").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(200);
        b.HasIndex(x => new { x.EmpresaId, x.EnvaseProductoId, x.Desde }).IsUnique().HasDatabaseName("ux_tara_envase_desde");
        b.HasIndex(x => x.EnvaseProductoId).HasDatabaseName("ix_tara_envase_envase");
    }
}

internal sealed class ConfiguracionPartida : IEntityTypeConfiguration<Partida>
{
    public void Configure(EntityTypeBuilder<Partida> b)
    {
        Columnas.Base(b, "partida");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        Columnas.Enum(b.Property(x => x.Origen), "origen");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.KilosIniciales).HasColumnName("kilos_iniciales").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.RecepcionId).HasColumnName("recepcion_id");
        b.Property(x => x.LineaRecepcionId).HasColumnName("linea_recepcion_id");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id");
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id");
        b.Property(x => x.CampanaId).HasColumnName("campana_id");
        b.Property(x => x.Calibre).HasColumnName("calibre").HasMaxLength(30);
        b.Property(x => x.ParteConfeccionId).HasColumnName("parte_confeccion_id");
        b.Property(x => x.CosteKg).HasColumnName("coste_kg").HasColumnType("numeric(14,6)");
        b.Property(x => x.Anulada).HasColumnName("anulada").IsRequired();
        b.Property(x => x.Certificaciones).HasColumnName("certificaciones").HasConversion<int>().HasDefaultValue(Certificaciones.Ninguna).IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_partida_codigo");
        b.HasIndex(x => x.RecepcionId).HasDatabaseName("ix_partida_recepcion");
        b.HasIndex(x => x.LineaRecepcionId).IsUnique().HasDatabaseName("ux_partida_linea_recepcion");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_partida_agricultor");
        b.HasIndex(x => x.ParcelaId).HasDatabaseName("ix_partida_parcela");
        b.HasIndex(x => x.CampanaId).HasDatabaseName("ix_partida_campana");
        b.HasIndex(x => x.ParteConfeccionId).HasDatabaseName("ix_partida_parte");
    }
}

internal sealed class ConfiguracionMovimientoPartida : IEntityTypeConfiguration<MovimientoPartida>
{
    public void Configure(EntityTypeBuilder<MovimientoPartida> b)
    {
        Columnas.Base(b, "movimiento_partida");
        b.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.PaleId).HasColumnName("pale_id");
        b.Property(x => x.Cajas).HasColumnName("cajas").HasDefaultValue(0).IsRequired();
        b.Property(x => x.DocumentoTipo).HasColumnName("documento_tipo").HasMaxLength(30);
        b.Property(x => x.DocumentoId).HasColumnName("documento_id");
        b.Property(x => x.Concepto).HasColumnName("concepto").HasMaxLength(MovimientoPartida.LongitudConcepto);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.PartidaId, x.PaleId }).HasDatabaseName("ix_movimiento_partida_partida");
        b.HasIndex(x => x.PaleId).HasDatabaseName("ix_movimiento_partida_pale");
    }
}

internal sealed class ConfiguracionPale : IEntityTypeConfiguration<Pale>
{
    public void Configure(EntityTypeBuilder<Pale> b)
    {
        Columnas.Base(b, "pale");
        b.Property(x => x.Sscc).HasColumnName("sscc").HasMaxLength(18).IsFixedLength().IsRequired();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasMaxLength(40);
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.FechaExpedicion).HasColumnName("fecha_expedicion");
        b.Property(x => x.ReferenciaExpedicion).HasColumnName("referencia_expedicion").HasMaxLength(80);
        b.Property(x => x.PlantillaId).HasColumnName("plantilla_id");
        b.Property(x => x.CartaPorteId).HasColumnName("carta_porte_id");
        b.Property(x => x.AlbaranId).HasColumnName("albaran_id");
        b.Property(x => x.LineaRecepcionId).HasColumnName("linea_recepcion_id");
        b.Property(x => x.SerieOrigen).HasColumnName("serie_origen").HasMaxLength(60);
        b.Property(x => x.CajasPorPale).HasColumnName("cajas_por_pale");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Sscc }).IsUnique().HasDatabaseName("ux_pale_sscc");
        b.HasIndex(x => x.LineaRecepcionId).HasDatabaseName("ix_pale_linea_recepcion");
        b.HasIndex(x => x.PlantillaId).HasDatabaseName("ix_pale_plantilla");
        b.HasIndex(x => x.CartaPorteId).HasDatabaseName("ix_pale_carta_porte");
        b.HasIndex(x => x.AlbaranId).HasDatabaseName("ix_pale_albaran");
    }
}

internal sealed class ConfiguracionPlantillaPale : IEntityTypeConfiguration<PlantillaPale>
{
    public void Configure(EntityTypeBuilder<PlantillaPale> b)
    {
        Columnas.Base(b, "plantilla_pale");
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
        b.Property(x => x.TipoPale).HasColumnName("tipo_pale").HasMaxLength(40);
        b.Property(x => x.ProductoId).HasColumnName("producto_id");
        b.Property(x => x.Marca).HasColumnName("marca").HasMaxLength(60);
        b.Property(x => x.CajasPorPale).HasColumnName("cajas_por_pale").IsRequired();
        b.Property(x => x.KilosPorCaja).HasColumnName("kilos_por_caja").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Filas).HasColumnName("filas");
        b.Property(x => x.Columnas).HasColumnName("columnas");
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
        b.Property(x => x.PaleProductoId).HasColumnName("pale_producto_id");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Codigo }).IsUnique().HasDatabaseName("ux_plantilla_pale_codigo");
    }
}

internal sealed class ConfiguracionCuentaEnvases : IEntityTypeConfiguration<CuentaEnvases>
{
    public void Configure(EntityTypeBuilder<CuentaEnvases> b)
    {
        Columnas.Base(b, "cuenta_envases");
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.TerceroId).HasColumnName("tercero_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(CuentaEnvases.LongitudNombre).IsRequired();
        b.Property(x => x.AgrupadoraId).HasColumnName("agrupadora_id");
        b.Property(x => x.ImputarATransportista).HasColumnName("imputar_a_transportista").IsRequired();
        Columnas.Enum(b.Property(x => x.Bloqueo), "bloqueo");
        b.Property(x => x.MotivoBloqueo).HasColumnName("motivo_bloqueo").HasMaxLength(200);
        b.Property(x => x.Limite).HasColumnName("limite");
        b.Property(x => x.Activa).HasColumnName("activa").IsRequired();
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.Tipo, x.TerceroId }).IsUnique().HasDatabaseName("ux_cuenta_envases_tercero");
        Columnas.Enum(b.Property(x => x.ControlLimite), "control_limite").HasDefaultValue(ControlLimiteEnvases.Aviso);
        b.OwnsMany(x => x.Limites, l =>
        {
            l.ToTable("limite_envase");
            l.WithOwner().HasForeignKey("cuenta_envases_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
            l.Property(x => x.Limite).HasColumnName("limite");
            l.Property(x => x.Minimo).HasColumnName("minimo");
            l.HasIndex("cuenta_envases_id", nameof(LimiteEnvase.EnvaseProductoId)).IsUnique().HasDatabaseName("ux_limite_envase");
        });
        b.Navigation(x => x.Limites).UsePropertyAccessMode(PropertyAccessMode.Field);
        Columnas.Enum(b.Property(x => x.Gestion), "gestion").HasDefaultValue(GestionEnvases.Retornar);
        b.OwnsMany(x => x.EnvasesPool, l =>
        {
            l.ToTable("envase_pool");
            l.WithOwner().HasForeignKey("cuenta_envases_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
            l.HasIndex("cuenta_envases_id", nameof(EnvasePool.EnvaseProductoId)).IsUnique().HasDatabaseName("ux_envase_pool");
        });
        b.Navigation(x => x.EnvasesPool).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionConfiguracionEnvases : IEntityTypeConfiguration<ConfiguracionEnvases>
{
    public void Configure(EntityTypeBuilder<ConfiguracionEnvases> b)
    {
        Columnas.Base(b, "configuracion_envases");
        b.Property(x => x.FechaCierre).HasColumnName("fecha_cierre");
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_envases_empresa");
    }
}

internal sealed class ConfiguracionMovimientoEnvases : IEntityTypeConfiguration<MovimientoEnvases>
{
    public void Configure(EntityTypeBuilder<MovimientoEnvases> b)
    {
        Columnas.Base(b, "movimiento_envases");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Ignore(x => x.NumeroCompleto);
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.CuentaId).HasColumnName("cuenta_id").IsRequired();
        b.Property(x => x.CuentaSolicitadaId).HasColumnName("cuenta_solicitada_id").IsRequired();
        Columnas.Enum(b.Property(x => x.Origen), "origen");
        b.Property(x => x.DocumentoId).HasColumnName("documento_id");
        b.Property(x => x.TransportistaId).HasColumnName("transportista_id");
        b.Property(x => x.Matricula).HasColumnName("matricula").HasMaxLength(20);
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(300);
        b.Property(x => x.AnulaMovimientoId).HasColumnName("anula_movimiento_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_movimiento_envases");
            l.WithOwner().HasForeignKey("movimiento_envases_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
            l.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired();
            l.HasIndex(x => x.EnvaseProductoId).HasDatabaseName("ix_linea_movimiento_envases_envase");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_movimiento_envases_numero");
        b.HasIndex(x => new { x.CuentaId, x.Fecha }).HasDatabaseName("ix_movimiento_envases_cuenta");
        b.HasIndex(x => x.DocumentoId).HasDatabaseName("ix_movimiento_envases_documento");
        // Un movimiento solo se anula una vez.
        b.HasIndex(x => x.AnulaMovimientoId).IsUnique().HasFilter("anula_movimiento_id IS NOT NULL").HasDatabaseName("ux_movimiento_envases_anula");
    }
}

internal sealed class ConfiguracionReservaPale : IEntityTypeConfiguration<ReservaPale>
{
    public void Configure(EntityTypeBuilder<ReservaPale> b)
    {
        Columnas.Base(b, "reserva_pale");
        b.Property(x => x.PaleId).HasColumnName("pale_id").IsRequired();
        b.Property(x => x.PedidoVentaId).HasColumnName("pedido_venta_id").IsRequired();
        b.Property(x => x.LineaPedidoId).HasColumnName("linea_pedido_id").IsRequired();
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Cajas).HasColumnName("cajas").IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.CreadaEn).HasColumnName("creada_en").IsRequired();
        b.Property(x => x.ConsumidaEl).HasColumnName("consumida_el");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        // Un palé tiene como mucho una reserva activa.
        b.HasIndex(x => x.PaleId).IsUnique().HasFilter("estado = 'Activa'").HasDatabaseName("ux_reserva_pale_activa");
        b.HasIndex(x => x.PedidoVentaId).HasDatabaseName("ix_reserva_pale_pedido");
        b.HasOne<Pale>().WithMany().HasForeignKey(x => x.PaleId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class RepositorioReservas : IRepositorioReservas
{
    private readonly AgroDbContext _ctx;

    public RepositorioReservas(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(ReservaPale reserva) => _ctx.Add(reserva);

    public Task<ReservaPale?> ReservaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ReservaPale>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<ReservaPale>> DePedidoAsync(Guid pedidoVentaId, CancellationToken ct = default) =>
        await _ctx.Set<ReservaPale>().Where(r => r.PedidoVentaId == pedidoVentaId).OrderBy(r => r.CreadaEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ReservaPale>> ActivasDePalesAsync(IReadOnlyCollection<Guid> paleIds, CancellationToken ct = default) =>
        await _ctx.Set<ReservaPale>().Where(r => paleIds.Contains(r.PaleId) && r.Estado == EstadoReservaPale.Activa).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ReservaPale>> DePaleAsync(Guid paleId, CancellationToken ct = default) =>
        await _ctx.Set<ReservaPale>().Where(r => r.PaleId == paleId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ReservaPale>> ActivasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ReservaPale>().Where(r => r.EmpresaId == empresaId && r.Estado == EstadoReservaPale.Activa).ToListAsync(ct).ConfigureAwait(false);
}

/// <summary>Cuentas y libro de envases por tercero.</summary>
internal sealed class RepositorioEnvases : IRepositorioEnvases
{
    private readonly AgroDbContext _ctx;

    public RepositorioEnvases(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public Task<CuentaEnvases?> CuentaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<CuentaEnvases>().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<CuentaEnvases?> CuentaDeAsync(Guid empresaId, TipoCuentaEnvases tipo, Guid terceroId, CancellationToken ct = default) =>
        _ctx.Set<CuentaEnvases>().FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.Tipo == tipo && c.TerceroId == terceroId, ct);

    public async Task<IReadOnlyList<CuentaEnvases>> CuentasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<CuentaEnvases>().AsNoTracking().Where(c => c.EmpresaId == empresaId).OrderBy(c => c.Nombre).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<(Guid CuentaId, Guid EnvaseProductoId, int Saldo)>> SaldosAsync(Guid empresaId, DateOnly? hasta, CancellationToken ct = default)
    {
        var q = _ctx.Set<MovimientoEnvases>().AsNoTracking().Where(m => m.EmpresaId == empresaId);
        if (hasta is { } h)
        {
            q = q.Where(m => m.Fecha <= h);
        }

        var filas = await q.SelectMany(m => m.Lineas.Select(l => new { m.CuentaId, l.EnvaseProductoId, l.Cantidad }))
            .GroupBy(x => new { x.CuentaId, x.EnvaseProductoId })
            .Select(g => new { g.Key.CuentaId, g.Key.EnvaseProductoId, Saldo = g.Sum(x => x.Cantidad) })
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(f => (f.CuentaId, f.EnvaseProductoId, f.Saldo)).ToList();
    }

    public async Task<IReadOnlyList<MovimientoEnvases>> MovimientosAsync(Guid empresaId, Guid? cuentaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var q = _ctx.Set<MovimientoEnvases>().AsNoTracking().Where(m => m.EmpresaId == empresaId);
        if (cuentaId is { } c)
        {
            q = q.Where(m => m.CuentaId == c || m.CuentaSolicitadaId == c);
        }

        if (desde is { } d)
        {
            q = q.Where(m => m.Fecha >= d);
        }

        if (hasta is { } h)
        {
            q = q.Where(m => m.Fecha <= h);
        }

        return await q.OrderBy(m => m.Fecha).ThenBy(m => m.Ejercicio).ThenBy(m => m.Numero).ToListAsync(ct).ConfigureAwait(false);
    }

    public Task<MovimientoEnvases?> MovimientoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<MovimientoEnvases>().FirstOrDefaultAsync(m => m.Id == id, ct);

    public async Task<IReadOnlyList<MovimientoEnvases>> DeDocumentoAsync(Guid documentoId, CancellationToken ct = default) =>
        await _ctx.Set<MovimientoEnvases>().Where(m => m.DocumentoId == documentoId).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> AnuladoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Set<MovimientoEnvases>().AnyAsync(m => m.AnulaMovimientoId == movimientoId, ct);

    public Task<ConfiguracionEnvases?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        _ctx.Set<ConfiguracionEnvases>().FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct);

    public async Task<IReadOnlyDictionary<Guid, DateOnly>> UltimosMovimientosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<MovimientoEnvases>().AsNoTracking().Where(m => m.EmpresaId == empresaId).GroupBy(m => m.CuentaId)
            .Select(g => new { g.Key, Fecha = g.Max(m => m.Fecha) }).ToDictionaryAsync(x => x.Key, x => x.Fecha, ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default) =>
        ((await _ctx.Set<MovimientoEnvases>().Where(m => m.EmpresaId == empresaId && m.Ejercicio == ejercicio).Select(m => (int?)m.Numero).MaxAsync(ct).ConfigureAwait(false)) ?? 0) + 1;
}

internal sealed class ConfiguracionMovimientoEnvase : IEntityTypeConfiguration<MovimientoEnvase>
{
    public void Configure(EntityTypeBuilder<MovimientoEnvase> b)
    {
        Columnas.Base(b, "movimiento_envase");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Cantidad).HasColumnName("cantidad").IsRequired();
        b.Property(x => x.RecepcionId).HasColumnName("recepcion_id");
        b.Property(x => x.Concepto).HasColumnName("concepto").HasMaxLength(MovimientoPartida.LongitudConcepto);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.HasIndex(x => new { x.AgricultorId, x.EnvaseProductoId }).HasDatabaseName("ix_movimiento_envase_agricultor");
        b.HasIndex(x => x.RecepcionId).HasDatabaseName("ix_movimiento_envase_recepcion");
    }
}

internal sealed class ConfiguracionClasificacion : IEntityTypeConfiguration<ClasificacionPartida>
{
    public void Configure(EntityTypeBuilder<ClasificacionPartida> b)
    {
        Columnas.Base(b, "clasificacion");
        b.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.KgMuestra);
        b.HasIndex(x => x.PartidaId).HasDatabaseName("ix_clasificacion_partida");
        b.HasIndex(x => x.PartidaId).IsUnique().HasFilter("estado = 'Definitiva'").HasDatabaseName("ux_clasificacion_definitiva");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_clasificacion");
            l.WithOwner().HasForeignKey("clasificacion_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("clasificacion_id").HasColumnName("clasificacion_id");
            l.Property(x => x.CategoriaId).HasColumnName("categoria_id").IsRequired();
            l.Property(x => x.KgMuestra).HasColumnName("kg_muestra").HasColumnType(Columnas.Kilos).IsRequired();
            l.HasIndex("clasificacion_id", nameof(LineaClasificacion.CategoriaId)).IsUnique().HasDatabaseName("ux_linea_clasificacion_categoria");
            l.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_linea_clasificacion_categoria");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionLiquidacion : IEntityTypeConfiguration<Liquidacion>
{
    public void Configure(EntityTypeBuilder<Liquidacion> b)
    {
        Columnas.Base(b, "liquidacion");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.CampanaId).HasColumnName("campana_id").IsRequired();
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero");
        Columnas.Enum(b.Property(x => x.Regimen), "regimen");
        b.Property(x => x.CodigoImpuesto).HasColumnName("codigo_impuesto").HasMaxLength(10).IsRequired();
        b.Property(x => x.PorcentajeImpuesto).HasColumnName("porcentaje_impuesto").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.PorcentajeRetencion).HasColumnName("porcentaje_retencion").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
        b.Property(x => x.Bruto).HasColumnName("bruto").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.TotalDescuentos).HasColumnName("total_descuentos").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.BaseImponible).HasColumnName("base_imponible").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.CuotaImpuesto).HasColumnName("cuota_impuesto").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.Retencion).HasColumnName("retencion").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.TotalFactura).HasColumnName("total_factura").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.APagar).HasColumnName("a_pagar").HasColumnType(Columnas.Importe).IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.GastoId).HasColumnName("gasto_id");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(Recepcion.LongitudTexto);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.EmitidaEn).HasColumnName("emitida_en");
        b.Ignore(x => x.NumeroCompleto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_liquidacion_numero");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_liquidacion_agricultor");
        b.HasIndex(x => x.CampanaId).HasDatabaseName("ix_liquidacion_campana");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_liquidacion");
            l.WithOwner().HasForeignKey("liquidacion_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property<Guid>("liquidacion_id").HasColumnName("liquidacion_id");
            l.Property(x => x.LineaRecepcionId).HasColumnName("linea_recepcion_id").IsRequired();
            l.Property(x => x.RecepcionId).HasColumnName("recepcion_id").IsRequired();
            l.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
            l.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            l.Property(x => x.PrecioId).HasColumnName("precio_id").IsRequired();
            l.Property(x => x.FechaRecepcion).HasColumnName("fecha_recepcion").IsRequired();
            l.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            l.Property(x => x.PrecioKg).HasColumnName("precio_kg").HasColumnType(Columnas.PrecioKg).IsRequired();
            l.Property(x => x.Importe).HasColumnName("importe").HasColumnType(Columnas.Importe).IsRequired();
            l.HasIndex("liquidacion_id").HasDatabaseName("ix_linea_liquidacion_liquidacion");
            l.HasIndex(x => x.LineaRecepcionId).HasDatabaseName("ix_linea_liquidacion_linea_recepcion");
            l.HasIndex(x => x.RecepcionId).HasDatabaseName("ix_linea_liquidacion_recepcion");
            l.HasIndex(x => x.PartidaId).HasDatabaseName("ix_linea_liquidacion_partida");
            l.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_linea_liquidacion_categoria");
            l.HasIndex(x => x.PrecioId).HasDatabaseName("ix_linea_liquidacion_precio");
        });
        b.OwnsMany(x => x.Descuentos, d =>
        {
            d.ToTable("descuento_liquidacion");
            d.WithOwner().HasForeignKey("liquidacion_id");
            d.HasKey(x => x.Id);
            d.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            d.Property<Guid>("liquidacion_id").HasColumnName("liquidacion_id");
            d.Property(x => x.ConceptoId).HasColumnName("concepto_id").IsRequired();
            d.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
            Columnas.Enum(d.Property(x => x.Tipo), "tipo");
            d.Property(x => x.Valor).HasColumnName("valor").HasColumnType(Columnas.PrecioKg).IsRequired();
            d.Property(x => x.Base).HasColumnName("base").HasColumnType(Columnas.Importe).IsRequired();
            d.Property(x => x.Importe).HasColumnName("importe").HasColumnType(Columnas.Importe).IsRequired();
            d.HasIndex("liquidacion_id").HasDatabaseName("ix_descuento_liquidacion_liquidacion");
            d.HasIndex(x => x.ConceptoId).HasDatabaseName("ix_descuento_liquidacion_concepto");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(x => x.Descuentos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionParte : IEntityTypeConfiguration<ParteConfeccion>
{
    public void Configure(EntityTypeBuilder<ParteConfeccion> b)
    {
        Columnas.Base(b, "parte_confeccion");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Numero).HasColumnName("numero");
        b.Property(x => x.CampanaId).HasColumnName("campana_id");
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(Recepcion.LongitudTexto);
        b.Property(x => x.CentroAnaliticoId).HasColumnName("centro_analitico_id");
        b.Property(x => x.PorcentajeIndirectos).HasColumnName("porcentaje_indirectos").HasColumnType(Columnas.Porcentaje).IsRequired();
        Columnas.Enum(b.Property(x => x.Reparto), "reparto");
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        foreach (var (propiedad, columna) in new[]
        {
            (nameof(ParteConfeccion.CosteFruta), "coste_fruta"), (nameof(ParteConfeccion.CosteMateriales), "coste_materiales"),
            (nameof(ParteConfeccion.CosteManoObra), "coste_mano_obra"), (nameof(ParteConfeccion.CosteMaquinaria), "coste_maquinaria"),
            (nameof(ParteConfeccion.CosteIndirectos), "coste_indirectos"), (nameof(ParteConfeccion.CosteTotal), "coste_total"),
        })
        {
            b.Property<decimal>(propiedad).HasColumnName(columna).HasColumnType(Columnas.Importe).IsRequired();
        }

        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Property(x => x.ValidadoEn).HasColumnName("validado_en");
        b.Property(x => x.MermaAprobadaPorId).HasColumnName("merma_aprobada_por_id");
        b.Property(x => x.MermaAprobadaPor).HasColumnName("merma_aprobada_por").HasMaxLength(150);
        b.Property(x => x.MotivoMerma).HasColumnName("motivo_merma").HasMaxLength(300);
        b.Property(x => x.MermaAprobadaEn).HasColumnName("merma_aprobada_en");
        b.Property(x => x.MermaAprobadaPct).HasColumnName("merma_aprobada_pct").HasColumnType("numeric(7,2)");
        b.Property(x => x.ToleranciaMermaPct).HasColumnName("tolerancia_merma_pct").HasColumnType("numeric(5,2)");
        b.Ignore(x => x.PorcentajeMerma);
        b.Ignore(x => x.MermaAprobada);
        b.HasIndex(x => x.MermaAprobadaPorId).HasDatabaseName("ix_parte_confeccion_aprobador");
        b.Ignore(x => x.NumeroCompleto);
        b.Ignore(x => x.KilosConsumidos);
        b.Ignore(x => x.KilosObtenidos);
        b.Ignore(x => x.Merma);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio, x.Numero }).IsUnique().HasDatabaseName("ux_parte_confeccion_numero");
        b.HasIndex(x => new { x.EmpresaId, x.Fecha }).HasDatabaseName("ix_parte_confeccion_fecha");
        b.HasIndex(x => x.CampanaId).HasDatabaseName("ix_parte_confeccion_campana");
        b.OwnsMany(x => x.Consumos, c =>
        {
            c.ToTable("consumo_parte");
            c.WithOwner().HasForeignKey("parte_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property<Guid>("parte_id").HasColumnName("parte_id");
            c.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
            c.Property(x => x.PaleId).HasColumnName("pale_id");
            c.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            c.Property(x => x.CosteKg).HasColumnName("coste_kg").HasColumnType("numeric(14,6)").IsRequired();
            c.Property(x => x.Coste).HasColumnName("coste").HasColumnType(Columnas.Importe).IsRequired();
            c.HasIndex("parte_id").HasDatabaseName("ix_consumo_parte_parte");
            c.HasIndex(x => x.PartidaId).HasDatabaseName("ix_consumo_parte_partida");
            c.HasIndex(x => x.PaleId).HasDatabaseName("ix_consumo_parte_pale");
        });
        b.OwnsMany(x => x.ManoObra, m =>
        {
            m.ToTable("mano_obra_parte");
            m.WithOwner().HasForeignKey("parte_id");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            m.Property<Guid>("parte_id").HasColumnName("parte_id");
            m.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
            m.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
            Columnas.Enum(m.Property(x => x.TipoHora), "tipo_hora");
            m.Property(x => x.Horas).HasColumnName("horas").HasColumnType("numeric(10,2)").IsRequired();
            m.Property(x => x.Piezas).HasColumnName("piezas").HasColumnType(Columnas.Kilos);
            m.Property(x => x.TarifaId).HasColumnName("tarifa_id");
            m.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType(Columnas.Unitario).IsRequired();
            m.Property(x => x.Coste).HasColumnName("coste").HasColumnType(Columnas.Importe).IsRequired();
            m.Property(x => x.Confeccion).HasColumnName("confeccion").IsRequired().HasDefaultValue(true);
            m.HasIndex("parte_id").HasDatabaseName("ix_mano_obra_parte_parte");
            m.HasIndex(x => x.TarifaId).HasDatabaseName("ix_mano_obra_parte_tarifa");
        });
        b.OwnsMany(x => x.Maquinas, m =>
        {
            m.ToTable("maquina_parte");
            m.WithOwner().HasForeignKey("parte_id");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            m.Property<Guid>("parte_id").HasColumnName("parte_id");
            m.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(ReglasAgro.LongitudNombre).IsRequired();
            m.Property(x => x.Categoria).HasColumnName("categoria").HasMaxLength(ReglasAgro.LongitudCodigo).IsRequired();
            m.Property(x => x.Horas).HasColumnName("horas").HasColumnType("numeric(10,2)").IsRequired();
            m.Property(x => x.TarifaId).HasColumnName("tarifa_id");
            m.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType(Columnas.Unitario).IsRequired();
            m.Property(x => x.Coste).HasColumnName("coste").HasColumnType(Columnas.Importe).IsRequired();
            m.HasIndex("parte_id").HasDatabaseName("ix_maquina_parte_parte");
            m.HasIndex(x => x.TarifaId).HasDatabaseName("ix_maquina_parte_tarifa");
        });
        b.OwnsMany(x => x.Materiales, m =>
        {
            m.ToTable("material_parte");
            m.WithOwner().HasForeignKey("parte_id");
            m.HasKey(x => x.Id);
            m.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            m.Property<Guid>("parte_id").HasColumnName("parte_id");
            m.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            m.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            m.Property(x => x.Cantidad).HasColumnName("cantidad").HasColumnType(Columnas.Kilos).IsRequired();
            m.Property(x => x.CosteUnitario).HasColumnName("coste_unitario").HasColumnType(Columnas.Unitario).IsRequired();
            m.Property(x => x.Coste).HasColumnName("coste").HasColumnType(Columnas.Importe).IsRequired();
            m.HasIndex("parte_id").HasDatabaseName("ix_material_parte_parte");
        });
        b.OwnsMany(x => x.Salidas, s =>
        {
            s.ToTable("salida_parte");
            s.WithOwner().HasForeignKey("parte_id");
            s.HasKey(x => x.Id);
            s.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            s.Property<Guid>("parte_id").HasColumnName("parte_id");
            s.Property(x => x.NumeroLinea).HasColumnName("numero_linea").IsRequired();
            s.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
            s.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            s.Property(x => x.Kilos).HasColumnName("kilos").HasColumnType(Columnas.Kilos).IsRequired();
            s.Property(x => x.Factor).HasColumnName("factor").HasColumnType("numeric(8,4)").IsRequired();
            s.Property(x => x.Calibre).HasColumnName("calibre").HasMaxLength(30);
            s.Property(x => x.MotivoDescalificacion).HasColumnName("motivo_descalificacion").HasMaxLength(300);
            s.Property(x => x.CategoriaId).HasColumnName("categoria_id");
            s.Property(x => x.PaleId).HasColumnName("pale_id");
            s.Property(x => x.PartidaId).HasColumnName("partida_id");
            s.Property(x => x.Coste).HasColumnName("coste").HasColumnType(Columnas.Importe).IsRequired();
            s.Property(x => x.Cajas).HasColumnName("cajas");
            s.Property(x => x.EnvaseProductoId).HasColumnName("envase_producto_id");
            s.Property(x => x.SegundosTeoricos).HasColumnName("segundos_teoricos").HasColumnType("numeric(14,2)");
            s.Property(x => x.CosteConfeccion).HasColumnName("coste_confeccion").HasColumnType(Columnas.Importe).IsRequired().HasDefaultValue(0m);
            s.Ignore(x => x.CosteKg);
            s.HasIndex("parte_id", nameof(SalidaParte.NumeroLinea)).IsUnique().HasDatabaseName("ux_salida_parte_numero");
            s.HasIndex(x => x.CategoriaId).HasDatabaseName("ix_salida_parte_categoria");
            s.HasIndex(x => x.PaleId).HasDatabaseName("ix_salida_parte_pale");
            s.HasIndex(x => x.PartidaId).HasDatabaseName("ix_salida_parte_partida");
        });
        foreach (var nav in new[] { nameof(ParteConfeccion.Consumos), nameof(ParteConfeccion.ManoObra), nameof(ParteConfeccion.Maquinas), nameof(ParteConfeccion.Materiales), nameof(ParteConfeccion.Salidas) })
        {
            b.Navigation(nav).UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

internal sealed class ConfiguracionCertificado : IEntityTypeConfiguration<CertificadoAgro>
{
    public void Configure(EntityTypeBuilder<CertificadoAgro> b)
    {
        Columnas.Base(b, "certificado_agro");
        b.Property(x => x.AgricultorId).HasColumnName("agricultor_id").IsRequired();
        b.Property(x => x.ParcelaId).HasColumnName("parcela_id");
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<int>().IsRequired();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(40).IsRequired();
        b.Property(x => x.Organismo).HasColumnName("organismo").HasMaxLength(100);
        b.Property(x => x.Desde).HasColumnName("desde").IsRequired();
        b.Property(x => x.Hasta).HasColumnName("hasta");
        b.Property(x => x.Baja).HasColumnName("baja").IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.AgricultorId }).HasDatabaseName("ix_certificado_agro_agricultor");
        b.HasIndex(x => x.AgricultorId).HasDatabaseName("ix_certificado_agro_agricultor_id");
        b.HasIndex(x => x.ParcelaId).HasDatabaseName("ix_certificado_agro_parcela");
    }
}

internal sealed class ConfiguracionDeclaracionArticulo : IEntityTypeConfiguration<DeclaracionArticulo>
{
    public void Configure(EntityTypeBuilder<DeclaracionArticulo> b)
    {
        Columnas.Base(b, "declaracion_articulo");
        b.Property(x => x.ProductoId).HasColumnName("producto_id").IsRequired();
        b.Property(x => x.Exige).HasColumnName("exige").HasConversion<int>().IsRequired();
        b.HasIndex(x => new { x.EmpresaId, x.ProductoId }).IsUnique().HasDatabaseName("ux_declaracion_articulo_producto");
        b.HasIndex(x => x.ProductoId).HasDatabaseName("ix_declaracion_articulo_producto");
    }
}

internal sealed class ConfiguracionDescalificacion : IEntityTypeConfiguration<DescalificacionPartida>
{
    public void Configure(EntityTypeBuilder<DescalificacionPartida> b)
    {
        Columnas.Base(b, "descalificacion_partida");
        b.Property(x => x.PartidaId).HasColumnName("partida_id").IsRequired();
        b.Property(x => x.Quitadas).HasColumnName("quitadas").HasConversion<int>().IsRequired();
        b.Property(x => x.Motivo).HasColumnName("motivo").HasMaxLength(300).IsRequired();
        b.Property(x => x.DocumentoTipo).HasColumnName("documento_tipo").HasMaxLength(30);
        b.Property(x => x.DocumentoId).HasColumnName("documento_id");
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.En).HasColumnName("en").IsRequired();
        b.HasIndex(x => x.PartidaId).HasDatabaseName("ix_descalificacion_partida_partida");
        b.HasIndex(x => x.UsuarioId).HasDatabaseName("ix_descalificacion_partida_usuario");
    }
}

internal sealed class ConfiguracionGenealogia : IEntityTypeConfiguration<Genealogia>
{
    public void Configure(EntityTypeBuilder<Genealogia> b)
    {
        Columnas.Base(b, "genealogia");
        b.Property(x => x.ParteId).HasColumnName("parte_id").IsRequired();
        b.Property(x => x.OrigenId).HasColumnName("origen_id").IsRequired();
        b.Property(x => x.DestinoId).HasColumnName("destino_id").IsRequired();
        b.Property(x => x.KilosOrigen).HasColumnName("kilos_origen").HasColumnType(Columnas.Kilos).IsRequired();
        b.HasIndex(x => x.ParteId).HasDatabaseName("ix_genealogia_parte");
        b.HasIndex(x => new { x.OrigenId, x.DestinoId }).IsUnique().HasDatabaseName("ux_genealogia_origen_destino");
        b.HasIndex(x => x.DestinoId).HasDatabaseName("ix_genealogia_destino");
    }
}

/// <summary>Repositorio del módulo agro.</summary>
internal sealed class RepositorioAgro : IRepositorioAgro
{
    private readonly AgroDbContext _ctx;

    public RepositorioAgro(AgroDbContext ctx) => _ctx = ctx;

    public void Agregar(object entidad) => _ctx.Add(entidad);

    public void Eliminar(object entidad) => _ctx.Remove(entidad);

    public async Task<IReadOnlyList<Campana>> CampanasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Campana>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Campana?> CampanaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Campana>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Agricultor>> AgricultoresAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Agricultor>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Agricultor?> AgricultorAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Agricultor>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> ExisteAgricultorAsync(Guid empresaId, Guid proveedorId, CancellationToken ct = default) =>
        _ctx.Set<Agricultor>().AnyAsync(x => x.EmpresaId == empresaId && x.ProveedorId == proveedorId, ct);

    public async Task<IReadOnlyList<Parcela>> ParcelasAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default) =>
        await _ctx.Set<Parcela>().Where(x => x.EmpresaId == empresaId && (agricultorId == null || x.AgricultorId == agricultorId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<Parcela?> ParcelaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Parcela>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<RectificacionRecepcion>> RectificacionesAsync(IReadOnlyCollection<Guid> lineaRecepcionIds, CancellationToken ct = default) =>
        await _ctx.Set<RectificacionRecepcion>().Where(r => lineaRecepcionIds.Contains(r.LineaRecepcionId)).OrderBy(r => r.CreadaEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<PlantillaCalidad>> PlantillasCalidadAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<PlantillaCalidad>().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<PlantillaCalidad?> PlantillaCalidadAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PlantillaCalidad>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> PlantillaCalidadEnUsoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<MuestreoCalidad>().AnyAsync(x => x.PlantillaId == id, ct);

    public async Task<IReadOnlyList<MuestreoCalidad>> MuestreosAsync(IReadOnlyCollection<Guid> lineaRecepcionIds, CancellationToken ct = default) =>
        await _ctx.Set<MuestreoCalidad>().Where(x => lineaRecepcionIds.Contains(x.LineaRecepcionId)).OrderBy(x => x.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public Task<MuestreoCalidad?> MuestreoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<MuestreoCalidad>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<CorreccionExpedicion>> CorreccionesExpedicionAsync(Guid paleId, CancellationToken ct = default) =>
        await _ctx.Set<CorreccionExpedicion>().Where(x => x.PaleId == paleId).OrderBy(x => x.En).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ToleranciaMermaFamilia>> ToleranciasMermaAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ToleranciaMermaFamilia>().Where(t => t.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ReglaTransformacion>> ReglasTransformacionAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ReglaTransformacion>().Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ReglaTransformacion?> ReglaTransformacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ReglaTransformacion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Repaletizado>> RepaletizadosAsync(Guid empresaId, Guid? paleId, CancellationToken ct = default) =>
        await _ctx.Set<Repaletizado>().Where(r => r.EmpresaId == empresaId && (paleId == null || r.DestinoPaleId == paleId || r.Lineas.Any(l => l.OrigenPaleId == paleId)))
            .OrderByDescending(r => r.CreadoEn).Take(500).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<TaraEnvase>> TarasAsync(Guid empresaId, Guid? envaseProductoId, CancellationToken ct = default) =>
        await _ctx.Set<TaraEnvase>().Where(t => t.EmpresaId == empresaId && (envaseProductoId == null || t.EnvaseProductoId == envaseProductoId))
            .OrderBy(t => t.EnvaseProductoId).ThenByDescending(t => t.Desde).ToListAsync(ct).ConfigureAwait(false);

    public Task<TaraEnvase?> TaraAsync(Guid id, CancellationToken ct = default) => _ctx.Set<TaraEnvase>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlySet<Guid>> TarasUsadasAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        (await _ctx.Set<Recepcion>().SelectMany(r => r.EnvasesPesadas).Where(e => e.TaraEnvaseId != null && ids.Contains(e.TaraEnvaseId.Value))
            .Select(e => e.TaraEnvaseId!.Value).Distinct().ToListAsync(ct).ConfigureAwait(false)).ToHashSet();

    public async Task<IReadOnlyList<CertificadoAgro>> CertificadosAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default) =>
        await _ctx.Set<CertificadoAgro>().Where(c => c.EmpresaId == empresaId && (agricultorId == null || c.AgricultorId == agricultorId))
            .OrderBy(c => c.AgricultorId).ThenBy(c => c.Tipo).ThenByDescending(c => c.Desde).ToListAsync(ct).ConfigureAwait(false);

    public Task<CertificadoAgro?> CertificadoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<CertificadoAgro>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<DeclaracionArticulo>> DeclaracionesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<DeclaracionArticulo>().Where(d => d.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<DeclaracionArticulo?> DeclaracionAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) =>
        _ctx.Set<DeclaracionArticulo>().Local.FirstOrDefault(d => d.EmpresaId == empresaId && d.ProductoId == productoId)
        ?? await _ctx.Set<DeclaracionArticulo>().SingleOrDefaultAsync(d => d.EmpresaId == empresaId && d.ProductoId == productoId, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DescalificacionPartida>> DescalificacionesAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        await _ctx.Set<DescalificacionPartida>().Where(d => partidaIds.Contains(d.PartidaId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Categoria>> CategoriasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Categoria>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Categoria?> CategoriaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Categoria>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ArticuloCampana>> ArticulosCampanaAsync(Guid campanaId, CancellationToken ct = default) =>
        await _ctx.Set<ArticuloCampana>().Where(x => x.CampanaId == campanaId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<PrecioLiquidacion>> PreciosAsync(Guid campanaId, CancellationToken ct = default) =>
        await _ctx.Set<PrecioLiquidacion>().Where(x => x.CampanaId == campanaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PrecioLiquidacion?> PrecioAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PrecioLiquidacion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> PrecioEnUsoAsync(Guid precioId, CancellationToken ct = default) =>
        _ctx.Set<Liquidacion>().Where(l => l.Estado != EstadoLiquidacion.Anulada).SelectMany(l => l.Lineas).AnyAsync(x => x.PrecioId == precioId, ct);

    public async Task<IReadOnlyList<ConceptoLiquidacion>> ConceptosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<ConceptoLiquidacion>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ConceptoLiquidacion?> ConceptoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ConceptoLiquidacion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<TarifaCoste>> TarifasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<TarifaCoste>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<TarifaCoste?> TarifaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<TarifaCoste>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<RendimientoConfeccion>> RendimientosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<RendimientoConfeccion>().Where(x => x.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<RendimientoConfeccion?> RendimientoAsync(Guid id, CancellationToken ct = default) =>
        _ctx.Set<RendimientoConfeccion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<ConfiguracionAgro?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        _ctx.Set<ConfiguracionAgro>().SingleOrDefaultAsync(x => x.EmpresaId == empresaId, ct);

    public Task<Recepcion?> RecepcionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Recepcion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Recepcion>> RecepcionesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, Guid? agricultorId, CancellationToken ct = default) =>
        await _ctx.Set<Recepcion>().Where(x => x.EmpresaId == empresaId && (desde == null || x.Fecha >= desde) && (hasta == null || x.Fecha <= hasta)
            && (agricultorId == null || x.AgricultorId == agricultorId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Recepcion>> RecepcionesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        ids.Count == 0 ? [] : await _ctx.Set<Recepcion>().Where(x => ids.Contains(x.Id)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> UltimoNumeroAsync(Guid empresaId, string serie, int ejercicio, CancellationToken ct = default)
    {
        int? max = serie switch
        {
            Recepcion.Serie => await _ctx.Set<Recepcion>().Where(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio).MaxAsync(x => x.Numero, ct).ConfigureAwait(false),
            Liquidacion.Serie => await _ctx.Set<Liquidacion>().Where(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio).MaxAsync(x => x.Numero, ct).ConfigureAwait(false),
            ParteConfeccion.Serie => await _ctx.Set<ParteConfeccion>().Where(x => x.EmpresaId == empresaId && x.Ejercicio == ejercicio).MaxAsync(x => x.Numero, ct).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(serie), serie, "Serie desconocida."),
        };
        return max ?? 0;
    }

    public Task<Partida?> PartidaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Partida>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Partida>> PartidasAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        ids.Count == 0 ? [] : await _ctx.Set<Partida>().Where(x => ids.Contains(x.Id)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Partida>> PartidasDeRecepcionAsync(Guid recepcionId, CancellationToken ct = default) =>
        await _ctx.Set<Partida>().Where(x => x.RecepcionId == recepcionId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Partida>> PartidasConSaldoAsync(Guid empresaId, CancellationToken ct = default)
    {
        var conSaldo = _ctx.Set<MovimientoPartida>().Where(m => m.EmpresaId == empresaId).GroupBy(m => m.PartidaId)
            .Where(g => g.Sum(m => m.Kilos) > 0m).Select(g => g.Key);
        return await _ctx.Set<Partida>().Where(p => conSaldo.Contains(p.Id)).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<SaldoPartida>> SaldosAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default)
    {
        if (partidaIds.Count == 0)
        {
            return [];
        }

        var filas = await _ctx.Set<MovimientoPartida>().Where(m => partidaIds.Contains(m.PartidaId)).GroupBy(m => new { m.PartidaId, m.PaleId })
            .Select(g => new { g.Key.PartidaId, g.Key.PaleId, Kilos = g.Sum(m => m.Kilos), Cajas = g.Sum(m => m.Cajas) }).ToListAsync(ct).ConfigureAwait(false);
        return filas.Where(f => f.Kilos != 0m).Select(f => new SaldoPartida(f.PartidaId, f.PaleId, f.Kilos, f.Cajas)).ToList();
    }

    public async Task<IReadOnlyList<SaldoPartida>> ContenidoPaleAsync(Guid paleId, CancellationToken ct = default)
    {
        var filas = await _ctx.Set<MovimientoPartida>().Where(m => m.PaleId == paleId).GroupBy(m => m.PartidaId)
            .Select(g => new { PartidaId = g.Key, Kilos = g.Sum(m => m.Kilos), Cajas = g.Sum(m => m.Cajas) }).ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(f => new SaldoPartida(f.PartidaId, paleId, f.Kilos, f.Cajas)).ToList();
    }

    public async Task<IReadOnlyList<MovimientoPartida>> MovimientosDePaleAsync(Guid paleId, CancellationToken ct = default) =>
        await _ctx.Set<MovimientoPartida>().Where(m => m.PaleId == paleId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<MovimientoPartida>> MovimientosAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        partidaIds.Count == 0 ? [] : await _ctx.Set<MovimientoPartida>().Where(m => partidaIds.Contains(m.PartidaId)).ToListAsync(ct).ConfigureAwait(false);

    public Task<bool> TieneMovimientosPosterioresAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        _ctx.Set<MovimientoPartida>().AnyAsync(m => partidaIds.Contains(m.PartidaId) && m.Tipo != TipoMovimientoPartida.Entrada, ct);

    public async Task<IReadOnlyList<(Guid EnvaseProductoId, int Saldo)>> SaldoEnvasesAsync(Guid agricultorId, CancellationToken ct = default)
    {
        var filas = await _ctx.Set<MovimientoEnvase>().Where(m => m.AgricultorId == agricultorId).GroupBy(m => m.EnvaseProductoId)
            .Select(g => new { g.Key, Saldo = g.Sum(m => m.Cantidad) }).ToListAsync(ct).ConfigureAwait(false);
        return filas.Select(f => (f.Key, f.Saldo)).ToList();
    }

    public async Task<IReadOnlyList<MovimientoEnvase>> MovimientosEnvaseAsync(Guid agricultorId, CancellationToken ct = default) =>
        await _ctx.Set<MovimientoEnvase>().Where(m => m.AgricultorId == agricultorId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Pale?> PaleAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Pale>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<Pale?> PalePorSsccAsync(Guid empresaId, string sscc, CancellationToken ct = default) =>
        _ctx.Set<Pale>().SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.Sscc == sscc, ct);

    public async Task<IReadOnlyList<Pale>> PalesAsync(Guid empresaId, EstadoPale? estado, CancellationToken ct = default) =>
        await _ctx.Set<Pale>().Where(x => x.EmpresaId == empresaId && (estado == null || x.Estado == estado)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Pale>> PalesAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        ids.Count == 0 ? [] : await _ctx.Set<Pale>().Where(x => ids.Contains(x.Id)).ToListAsync(ct).ConfigureAwait(false);

    /// <summary>Palés de la empresa, contando los dados de alta en esta unidad de trabajo y aún sin guardar (para numerar el SSCC).</summary>
    /// <summary>SSCC ya dados: los palés y las etiquetas de campo aún sin palé (las usadas ya cuentan como palé).</summary>
    public async Task<int> PalesCreadosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Pale>().CountAsync(x => x.EmpresaId == empresaId, ct).ConfigureAwait(false)
        + _ctx.ChangeTracker.Entries<Pale>().Count(e => e.State == EntityState.Added && e.Entity.EmpresaId == empresaId)
        + await _ctx.Set<EtiquetaCampo>().CountAsync(x => x.EmpresaId == empresaId && x.PaleId == null, ct).ConfigureAwait(false)
        + _ctx.ChangeTracker.Entries<EtiquetaCampo>().Count(e => e.State == EntityState.Added && e.Entity.EmpresaId == empresaId);

    public async Task<IReadOnlyList<EtiquetaCampo>> EtiquetasCampoAsync(Guid empresaId, Guid? agricultorId, bool soloLibres, CancellationToken ct = default) =>
        await _ctx.Set<EtiquetaCampo>().Where(x => x.EmpresaId == empresaId && (agricultorId == null || x.AgricultorId == agricultorId) && (!soloLibres || x.PaleId == null))
            .OrderByDescending(x => x.EmitidaEn).ThenBy(x => x.Sscc).Take(2000).ToListAsync(ct).ConfigureAwait(false);

    public Task<EtiquetaCampo?> EtiquetaCampoAsync(Guid empresaId, string sscc, CancellationToken ct = default) =>
        _ctx.Set<EtiquetaCampo>().SingleOrDefaultAsync(x => x.EmpresaId == empresaId && x.Sscc == sscc, ct);

    public async Task<IReadOnlyList<EtiquetaCampo>> EtiquetasCampoPorIdAsync(IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        await _ctx.Set<EtiquetaCampo>().Where(x => ids.Contains(x.Id)).OrderBy(x => x.Sscc).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Pale>> PalesDeCartaPorteAsync(Guid cartaPorteId, CancellationToken ct = default) =>
        await _ctx.Set<Pale>().Where(x => x.CartaPorteId == cartaPorteId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Pale>> PalesDeAlbaranAsync(Guid albaranId, CancellationToken ct = default) =>
        await _ctx.Set<Pale>().Where(x => x.AlbaranId == albaranId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<PlantillaPale>> PlantillasPaleAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<PlantillaPale>().Where(x => x.EmpresaId == empresaId).OrderBy(x => x.Codigo).ToListAsync(ct).ConfigureAwait(false);

    public Task<PlantillaPale?> PlantillaPaleAsync(Guid id, CancellationToken ct = default) => _ctx.Set<PlantillaPale>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public Task<bool> PlantillaPaleEnUsoAsync(Guid plantillaId, CancellationToken ct = default) => _ctx.Set<Pale>().AnyAsync(x => x.PlantillaId == plantillaId, ct);

    public async Task<IReadOnlyList<ClasificacionPartida>> ClasificacionesAsync(Guid partidaId, CancellationToken ct = default) =>
        await _ctx.Set<ClasificacionPartida>().Where(x => x.PartidaId == partidaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ClasificacionPartida?> ClasificacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ClasificacionPartida>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ClasificacionPartida>> DefinitivasAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        partidaIds.Count == 0 ? [] : await _ctx.Set<ClasificacionPartida>().Where(x => partidaIds.Contains(x.PartidaId) && x.Estado == EstadoClasificacion.Definitiva)
            .ToListAsync(ct).ConfigureAwait(false);

    public Task<Liquidacion?> LiquidacionAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Liquidacion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<Liquidacion>> LiquidacionesAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default) =>
        await _ctx.Set<Liquidacion>().Where(x => x.EmpresaId == empresaId && (agricultorId == null || x.AgricultorId == agricultorId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlySet<Guid>> LineasEnLiquidacionAsync(IReadOnlyCollection<Guid> lineaRecepcionIds, CancellationToken ct = default)
    {
        if (lineaRecepcionIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var ids = await _ctx.Set<Liquidacion>().Where(l => l.Estado != EstadoLiquidacion.Anulada)
            .SelectMany(l => l.Lineas).Where(x => lineaRecepcionIds.Contains(x.LineaRecepcionId)).Select(x => x.LineaRecepcionId)
            .Distinct().ToListAsync(ct).ConfigureAwait(false);
        return ids.ToHashSet();
    }

    public Task<bool> PartidasEnLiquidacionAsync(IReadOnlyCollection<Guid> partidaIds, CancellationToken ct = default) =>
        _ctx.Set<Liquidacion>().Where(l => l.Estado != EstadoLiquidacion.Anulada).SelectMany(l => l.Lineas).AnyAsync(x => partidaIds.Contains(x.PartidaId), ct);

    public Task<ParteConfeccion?> ParteAsync(Guid id, CancellationToken ct = default) => _ctx.Set<ParteConfeccion>().SingleOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<ParteConfeccion>> PartesAsync(Guid empresaId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        await _ctx.Set<ParteConfeccion>().Where(x => x.EmpresaId == empresaId && (desde == null || x.Fecha >= desde) && (hasta == null || x.Fecha <= hasta))
            .ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Genealogia>> OrigenesAsync(IReadOnlyCollection<Guid> destinoIds, CancellationToken ct = default) =>
        destinoIds.Count == 0 ? [] : await _ctx.Set<Genealogia>().Where(g => destinoIds.Contains(g.DestinoId)).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Genealogia>> DestinosAsync(IReadOnlyCollection<Guid> origenIds, CancellationToken ct = default) =>
        origenIds.Count == 0 ? [] : await _ctx.Set<Genealogia>().Where(g => origenIds.Contains(g.OrigenId)).ToListAsync(ct).ConfigureAwait(false);
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class AgroDbContextFactory : IDesignTimeDbContextFactory<AgroDbContext>
{
    public AgroDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<AgroDbContext>().UseNpgsql(conexion).Options;
        return new AgroDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
