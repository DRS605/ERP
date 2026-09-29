using AlxorCore.Cooperativa.Aplicacion;
using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Cooperativa.Infraestructura;

/// <summary>Contexto de persistencia del módulo de cooperativas y SAT (esquema <c>cooperativa</c>).</summary>
public sealed class CooperativaDbContext : DbContextEmpresaBase, IUnidadDeTrabajoCooperativa
{
    public const string Esquema = "cooperativa";

    public CooperativaDbContext(DbContextOptions<CooperativaDbContext> opciones, IPublicadorEventos publicador, IContextoEmpresa contexto)
        : base(opciones, publicador, contexto)
    {
    }

    private const string SqlBorradoEmpresa = """
        DELETE FROM cooperativa.movimiento_capital WHERE empresa_id = {0};
        DELETE FROM cooperativa.reparto WHERE empresa_id = {0};
        DELETE FROM cooperativa.acta WHERE empresa_id = {0};
        DELETE FROM cooperativa.socio WHERE empresa_id = {0};
        DELETE FROM cooperativa.configuracion WHERE empresa_id = {0};
        """;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Esquema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CooperativaDbContext).Assembly);
        AplicarFiltroMultiempresa(modelBuilder);
    }

    /// <summary>Borra todos los datos de la cooperativa de la empresa (solo dentro de <c>BorradoEmpresa</c>).</summary>
    public Task BorrarEmpresaAsync(Guid empresaId, CancellationToken ct = default) =>
        Database.ExecuteSqlRawAsync(SqlBorradoEmpresa, [empresaId], ct);
}

internal static class Columnas
{
    public const string Importe = "numeric(14,2)";
    public const string Porcentaje = "numeric(7,2)";
    public const string Actividad = "numeric(16,3)";

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

internal sealed class ConfiguracionConfiguracion : IEntityTypeConfiguration<ConfiguracionCooperativa>
{
    public void Configure(EntityTypeBuilder<ConfiguracionCooperativa> b)
    {
        Columnas.Base(b, "configuracion");
        Columnas.Enum(b.Property(x => x.Forma), "forma");
        b.Property(x => x.AportacionObligatoria).HasColumnName("aportacion_obligatoria").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.PorcentajeFroMinimo).HasColumnName("porcentaje_fro_minimo").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.PorcentajeFepMinimo).HasColumnName("porcentaje_fep_minimo").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.InteresMaximoCapital).HasColumnName("interes_maximo_capital").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.PorcentajeRetencion).HasColumnName("porcentaje_retencion").HasColumnType(Columnas.Porcentaje).IsRequired();
        Columnas.Enum(b.Property(x => x.Base), "base_retorno");
        b.Property(x => x.DeduccionMaximaExpulsion).HasColumnName("deduccion_maxima_expulsion").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.DeduccionMaximaNoJustificada).HasColumnName("deduccion_maxima_no_justificada").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.Contabilizar).HasColumnName("contabilizar").IsRequired();
        b.OwnsOne(x => x.Cuentas, c =>
        {
            c.Property(x => x.Capital).HasColumnName("cuenta_capital").HasMaxLength(20).IsRequired();
            c.Property(x => x.DesembolsosPendientes).HasColumnName("cuenta_desembolsos_pendientes").HasMaxLength(20).IsRequired();
            c.Property(x => x.Tesoreria).HasColumnName("cuenta_tesoreria").HasMaxLength(20).IsRequired();
            c.Property(x => x.Reembolsos).HasColumnName("cuenta_reembolsos").HasMaxLength(20).IsRequired();
            c.Property(x => x.Resultado).HasColumnName("cuenta_resultado").HasMaxLength(20).IsRequired();
            c.Property(x => x.Fro).HasColumnName("cuenta_fro").HasMaxLength(20).IsRequired();
            c.Property(x => x.Fep).HasColumnName("cuenta_fep").HasMaxLength(20).IsRequired();
            c.Property(x => x.ReservasVoluntarias).HasColumnName("cuenta_reservas_voluntarias").HasMaxLength(20).IsRequired();
            c.Property(x => x.Retornos).HasColumnName("cuenta_retornos").HasMaxLength(20).IsRequired();
            c.Property(x => x.Retenciones).HasColumnName("cuenta_retenciones").HasMaxLength(20).IsRequired();
            c.Ignore(x => x.Todas);
        });
        b.Navigation(x => x.Cuentas).IsRequired();
        b.HasIndex(x => x.EmpresaId).IsUnique().HasDatabaseName("ux_configuracion_cooperativa_empresa");
    }
}

internal sealed class ConfiguracionSocio : IEntityTypeConfiguration<Socio>
{
    public void Configure(EntityTypeBuilder<Socio> b)
    {
        Columnas.Base(b, "socio");
        b.Property(x => x.Numero).HasColumnName("numero").IsRequired();
        b.Property(x => x.ProveedorId).HasColumnName("proveedor_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
        b.Property(x => x.Nif).HasColumnName("nif").HasMaxLength(20);
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        b.Property(x => x.FechaAlta).HasColumnName("fecha_alta").IsRequired();
        b.Property(x => x.FechaBaja).HasColumnName("fecha_baja");
        b.Property(x => x.MotivoBaja).HasColumnName("motivo_baja").HasMaxLength(30).HasConversion<string>();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(Socio.LongitudObservaciones);
        b.Ignore(x => x.DeBaja);
        b.HasIndex(x => new { x.EmpresaId, x.Numero }).IsUnique().HasDatabaseName("ux_socio_numero");
        b.HasIndex(x => new { x.EmpresaId, x.ProveedorId }).IsUnique().HasFilter("fecha_baja IS NULL").HasDatabaseName("ux_socio_proveedor_activo");
        b.HasIndex(x => x.ProveedorId).HasDatabaseName("ix_socio_proveedor");
    }
}

internal sealed class ConfiguracionMovimientoCapital : IEntityTypeConfiguration<MovimientoCapital>
{
    public void Configure(EntityTypeBuilder<MovimientoCapital> b)
    {
        Columnas.Base(b, "movimiento_capital");
        b.Property(x => x.SocioId).HasColumnName("socio_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Tipo), "tipo");
        Columnas.Enum(b.Property(x => x.Clase), "clase");
        b.Property(x => x.Suscrito).HasColumnName("suscrito").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.Desembolsado).HasColumnName("desembolsado").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.Deduccion).HasColumnName("deduccion").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.Concepto).HasColumnName("concepto").HasMaxLength(200).IsRequired();
        b.Property(x => x.GrupoId).HasColumnName("grupo_id");
        b.Property(x => x.RepartoId).HasColumnName("reparto_id");
        b.Property(x => x.AnulaId).HasColumnName("anula_id");
        b.Property(x => x.AsientoId).HasColumnName("asiento_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.ADevolver);
        b.HasOne<Socio>().WithMany().HasForeignKey(x => x.SocioId).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_movimiento_capital_socio");
        b.HasIndex(x => new { x.SocioId, x.Fecha }).HasDatabaseName("ix_movimiento_capital_socio");
        b.HasIndex(x => x.AnulaId).IsUnique().HasFilter("anula_id IS NOT NULL").HasDatabaseName("ux_movimiento_capital_anula");
        b.HasIndex(x => x.GrupoId).HasDatabaseName("ix_movimiento_capital_grupo");
        b.HasIndex(x => x.RepartoId).HasDatabaseName("ix_movimiento_capital_reparto");
    }
}

internal sealed class ConfiguracionReparto : IEntityTypeConfiguration<Reparto>
{
    public void Configure(EntityTypeBuilder<Reparto> b)
    {
        Columnas.Base(b, "reparto");
        b.Property(x => x.Ejercicio).HasColumnName("ejercicio").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.FechaAsamblea).HasColumnName("fecha_asamblea");
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        Columnas.Enum(b.Property(x => x.Base), "base_retorno");
        b.Property(x => x.Excedente).HasColumnName("excedente").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.PorcentajeFro).HasColumnName("porcentaje_fro").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.PorcentajeFep).HasColumnName("porcentaje_fep").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.ImporteFro).HasColumnName("importe_fro").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.ImporteFep).HasColumnName("importe_fep").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.ReservasVoluntarias).HasColumnName("reservas_voluntarias").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.PorcentajeIntereses).HasColumnName("porcentaje_intereses").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.ImporteIntereses).HasColumnName("importe_intereses").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.ImporteRetorno).HasColumnName("importe_retorno").HasColumnType(Columnas.Importe).IsRequired();
        b.Property(x => x.PorcentajeRetencion).HasColumnName("porcentaje_retencion").HasColumnType(Columnas.Porcentaje).IsRequired();
        b.Property(x => x.Observaciones).HasColumnName("observaciones").HasMaxLength(500);
        b.Property(x => x.AsientoId).HasColumnName("asiento_id");
        b.Property(x => x.MotivoAnulacion).HasColumnName("motivo_anulacion").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en").IsRequired();
        b.Ignore(x => x.TotalRetencion);
        b.Ignore(x => x.TotalCapitalizado);
        b.Ignore(x => x.TotalNeto);
        b.HasIndex(x => new { x.EmpresaId, x.Ejercicio }).IsUnique().HasFilter("estado <> 'Anulado'").HasDatabaseName("ux_reparto_ejercicio_vivo");
        b.OwnsMany(x => x.Lineas, l =>
        {
            l.ToTable("linea_reparto");
            l.WithOwner().HasForeignKey("reparto_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.Numero).HasColumnName("numero").IsRequired();
            l.Property(x => x.SocioId).HasColumnName("socio_id").IsRequired();
            l.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(200).IsRequired();
            l.Property(x => x.Actividad).HasColumnName("actividad").HasColumnType(Columnas.Actividad).IsRequired();
            l.Property(x => x.Capital).HasColumnName("capital").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.Intereses).HasColumnName("intereses").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.Retorno).HasColumnName("retorno").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.Retencion).HasColumnName("retencion").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.Capitalizado).HasColumnName("capitalizado").HasColumnType(Columnas.Importe).IsRequired();
            l.Property(x => x.Neto).HasColumnName("neto").HasColumnType(Columnas.Importe).IsRequired();
            l.HasIndex("reparto_id", nameof(LineaReparto.SocioId)).IsUnique().HasDatabaseName("ux_linea_reparto_socio");
            l.HasIndex(x => x.SocioId).HasDatabaseName("ix_linea_reparto_socio");
        });
        b.Navigation(x => x.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class ConfiguracionActa : IEntityTypeConfiguration<Acta>
{
    public void Configure(EntityTypeBuilder<Acta> b)
    {
        Columnas.Base(b, "acta");
        Columnas.Enum(b.Property(x => x.Organo), "organo");
        b.Property(x => x.Numero).HasColumnName("numero");
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        Columnas.Enum(b.Property(x => x.Caracter), "caracter");
        b.Property(x => x.Lugar).HasColumnName("lugar").HasMaxLength(200);
        b.Property(x => x.Presentes).HasColumnName("presentes").IsRequired();
        b.Property(x => x.Representados).HasColumnName("representados").IsRequired();
        b.Property(x => x.Presidente).HasColumnName("presidente").HasMaxLength(200);
        b.Property(x => x.Secretario).HasColumnName("secretario").HasMaxLength(200);
        b.Property(x => x.OrdenDelDia).HasColumnName("orden_del_dia").HasMaxLength(Acta.LongitudTexto).IsRequired();
        b.Property(x => x.Acuerdos).HasColumnName("acuerdos").HasMaxLength(Acta.LongitudTexto).IsRequired();
        Columnas.Enum(b.Property(x => x.Estado), "estado");
        b.Property(x => x.AprobadaEn).HasColumnName("aprobada_en");
        b.HasIndex(x => new { x.EmpresaId, x.Organo, x.Numero }).IsUnique().HasFilter("numero IS NOT NULL").HasDatabaseName("ux_acta_numero");
    }
}

internal sealed class RepositorioCooperativa : IRepositorioCooperativa
{
    private readonly CooperativaDbContext _ctx;

    public RepositorioCooperativa(CooperativaDbContext ctx) => _ctx = ctx;

    public async Task<ConfiguracionCooperativa?> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        _ctx.Set<ConfiguracionCooperativa>().Local.FirstOrDefault(c => c.EmpresaId == empresaId)
        ?? await _ctx.Set<ConfiguracionCooperativa>().FirstOrDefaultAsync(c => c.EmpresaId == empresaId, ct).ConfigureAwait(false);

    public void Agregar(ConfiguracionCooperativa configuracion) => _ctx.Add(configuracion);

    public async Task<IReadOnlyList<Socio>> SociosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Socio>().Where(s => s.EmpresaId == empresaId).OrderBy(s => s.Numero).ToListAsync(ct).ConfigureAwait(false);

    public Task<Socio?> SocioAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Socio>().FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<int> UltimoNumeroSocioAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Socio>().Where(s => s.EmpresaId == empresaId).MaxAsync(s => (int?)s.Numero, ct).ConfigureAwait(false) ?? 0;

    public void Agregar(Socio socio) => _ctx.Add(socio);

    public void Eliminar(Socio socio) => _ctx.Remove(socio);

    public async Task<IReadOnlyList<MovimientoCapital>> MovimientosAsync(Guid empresaId, Guid? socioId, CancellationToken ct = default) =>
        await _ctx.Set<MovimientoCapital>().Where(m => m.EmpresaId == empresaId && (socioId == null || m.SocioId == socioId))
            .OrderBy(m => m.Fecha).ThenBy(m => m.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public Task<MovimientoCapital?> MovimientoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<MovimientoCapital>().FirstOrDefaultAsync(m => m.Id == id, ct);

    public void Agregar(MovimientoCapital movimiento) => _ctx.Add(movimiento);

    public async Task<IReadOnlyList<Reparto>> RepartosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Reparto>().Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Reparto?> RepartoAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Reparto>().FirstOrDefaultAsync(r => r.Id == id, ct);

    public void Agregar(Reparto reparto) => _ctx.Add(reparto);

    public void Eliminar(Reparto reparto) => _ctx.Remove(reparto);

    public async Task<IReadOnlyList<Acta>> ActasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _ctx.Set<Acta>().Where(a => a.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<Acta?> ActaAsync(Guid id, CancellationToken ct = default) => _ctx.Set<Acta>().FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<int> UltimoNumeroActaAsync(Guid empresaId, OrganoSocial organo, CancellationToken ct = default) =>
        await _ctx.Set<Acta>().Where(a => a.EmpresaId == empresaId && a.Organo == organo).MaxAsync(a => a.Numero, ct).ConfigureAwait(false) ?? 0;

    public void Agregar(Acta acta) => _ctx.Add(acta);

    public void Eliminar(Acta acta) => _ctx.Remove(acta);
}

/// <summary>Factoría en tiempo de diseño para las migraciones.</summary>
public sealed class CooperativaDbContextFactory : IDesignTimeDbContextFactory<CooperativaDbContext>
{
    public CooperativaDbContext CreateDbContext(string[] args)
    {
        var conexion = Environment.GetEnvironmentVariable("ALXOR_MIGRACIONES_CONEXION")
            ?? "Host=localhost;Port=5432;Database=alxor;Username=postgres;Password=postgres";
        var opciones = new DbContextOptionsBuilder<CooperativaDbContext>()
            .UseNpgsql(conexion, npgsql => npgsql.MigrationsHistoryTable("__historial_migraciones", CooperativaDbContext.Esquema)).Options;
        return new CooperativaDbContext(opciones, new PublicadorInactivo(), new ContextoVacio());
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
