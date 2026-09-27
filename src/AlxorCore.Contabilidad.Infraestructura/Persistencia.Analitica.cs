using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Contabilidad.Infraestructura;

internal sealed class ConfiguracionCentroAnalitico : IEntityTypeConfiguration<CentroAnalitico>
{
    public void Configure(EntityTypeBuilder<CentroAnalitico> builder)
    {
        builder.ToTable("centro_analitico");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(c => c.Codigo).HasColumnName("codigo").HasMaxLength(MaestroAnaliticoLimites.Codigo).IsRequired();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(MaestroAnaliticoLimites.Nombre).IsRequired();
        builder.Property(c => c.Tipo).HasColumnName("tipo").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(c => c.PadreId).HasColumnName("padre_id");
        builder.Property(c => c.Activo).HasColumnName("activo").IsRequired();
        builder.HasOne<CentroAnalitico>().WithMany().HasForeignKey(c => c.PadreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(c => new { c.GrupoId, c.Codigo }).IsUnique().HasDatabaseName("ux_centro_analitico_grupo_codigo");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionPartidaAnalitica : IEntityTypeConfiguration<PartidaAnalitica>
{
    public void Configure(EntityTypeBuilder<PartidaAnalitica> builder)
    {
        builder.ToTable("partida_analitica");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(MaestroAnaliticoLimites.Codigo).IsRequired();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(MaestroAnaliticoLimites.Nombre).IsRequired();
        builder.Property(p => p.Naturaleza).HasColumnName("naturaleza").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(p => p.PadreId).HasColumnName("padre_id");
        builder.Property(p => p.Activo).HasColumnName("activo").IsRequired();
        builder.HasOne<PartidaAnalitica>().WithMany().HasForeignKey(p => p.PadreId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(p => new { p.GrupoId, p.Codigo }).IsUnique().HasDatabaseName("ux_partida_analitica_grupo_codigo");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class ConfiguracionClaveReparto : IEntityTypeConfiguration<ClaveReparto>
{
    public void Configure(EntityTypeBuilder<ClaveReparto> builder)
    {
        builder.ToTable("clave_reparto");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.GrupoId).HasColumnName("grupo_id").IsRequired();
        builder.Property(c => c.Codigo).HasColumnName("codigo").HasMaxLength(MaestroAnaliticoLimites.Codigo).IsRequired();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(MaestroAnaliticoLimites.Nombre).IsRequired();
        builder.Property(c => c.Activa).HasColumnName("activa").IsRequired();
        builder.OwnsMany(c => c.Lineas, l =>
        {
            l.ToTable("linea_clave_reparto");
            l.WithOwner().HasForeignKey("clave_reparto_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.CentroId).HasColumnName("centro_id").IsRequired();
            l.Property(x => x.Porcentaje).HasColumnName("porcentaje").HasColumnType("numeric(5,2)").IsRequired();
            l.HasOne<CentroAnalitico>().WithMany().HasForeignKey(x => x.CentroId).OnDelete(DeleteBehavior.Restrict);
            l.HasIndex("clave_reparto_id").HasDatabaseName("ix_linea_clave_reparto_clave");
        });
        builder.Navigation(c => c.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(c => c.Reparto);
        builder.HasIndex(c => new { c.GrupoId, c.Codigo }).IsUnique().HasDatabaseName("ux_clave_reparto_grupo_codigo");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class ConfiguracionReglaAnalitica : IEntityTypeConfiguration<ReglaAnalitica>
{
    public void Configure(EntityTypeBuilder<ReglaAnalitica> builder)
    {
        builder.ToTable("regla_analitica");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(r => r.Descripcion).HasColumnName("descripcion").HasMaxLength(ReglaAnalitica.LongitudMaximaDescripcion).IsRequired();
        builder.Property(r => r.PrefijoCuenta).HasColumnName("prefijo_cuenta").HasMaxLength(12);
        builder.Property(r => r.TerceroId).HasColumnName("tercero_id");
        builder.Property(r => r.ActividadNegocioId).HasColumnName("actividad_negocio_id");
        builder.Property(r => r.Familia).HasColumnName("familia").HasMaxLength(80);
        builder.Property(r => r.VigenteDesde).HasColumnName("vigente_desde");
        builder.Property(r => r.VigenteHasta).HasColumnName("vigente_hasta");
        builder.Property(r => r.CentroId).HasColumnName("centro_id");
        builder.Property(r => r.ClaveRepartoId).HasColumnName("clave_reparto_id");
        builder.Property(r => r.PartidaId).HasColumnName("partida_id");
        builder.Property(r => r.Prioridad).HasColumnName("prioridad").IsRequired();
        builder.HasOne<CentroAnalitico>().WithMany().HasForeignKey(r => r.CentroId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ClaveReparto>().WithMany().HasForeignKey(r => r.ClaveRepartoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PartidaAnalitica>().WithMany().HasForeignKey(r => r.PartidaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.EmpresaId).HasDatabaseName("ix_regla_analitica_empresa");
        builder.Ignore(r => r.Especificidad);
        builder.Ignore(r => r.EventosDominio);
    }
}

internal sealed class ConfiguracionPeriodoAnalitico : IEntityTypeConfiguration<PeriodoAnalitico>
{
    public void Configure(EntityTypeBuilder<PeriodoAnalitico> builder)
    {
        builder.ToTable("periodo_analitico");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(MaestroAnaliticoLimites.Codigo).IsRequired();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(MaestroAnaliticoLimites.Nombre).IsRequired();
        builder.Property(p => p.Desde).HasColumnName("desde").IsRequired();
        builder.Property(p => p.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(p => p.Cerrado).HasColumnName("cerrado").IsRequired();
        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique().HasDatabaseName("ux_periodo_analitico_empresa_codigo");
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class ConfiguracionEjecucionAnalitica : IEntityTypeConfiguration<EjecucionAnalitica>
{
    public void Configure(EntityTypeBuilder<EjecucionAnalitica> builder)
    {
        builder.ToTable("ejecucion_analitica");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.Tipo).HasColumnName("tipo").HasMaxLength(30).HasConversion<string>().IsRequired();
        builder.Property(e => e.Desde).HasColumnName("desde").IsRequired();
        builder.Property(e => e.Hasta).HasColumnName("hasta").IsRequired();
        builder.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(200).IsRequired();
        builder.Property(e => e.CentroOrigenId).HasColumnName("centro_origen_id");
        builder.Property(e => e.ClaveRepartoId).HasColumnName("clave_reparto_id");
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasOne<CentroAnalitico>().WithMany().HasForeignKey(e => e.CentroOrigenId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<ClaveReparto>().WithMany().HasForeignKey(e => e.ClaveRepartoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.EmpresaId).HasDatabaseName("ix_ejecucion_analitica_empresa");
        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class ConfiguracionImputacionAnalitica : IEntityTypeConfiguration<ImputacionAnalitica>
{
    public void Configure(EntityTypeBuilder<ImputacionAnalitica> builder)
    {
        builder.ToTable("imputacion_analitica");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(i => i.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(i => i.ApunteId).HasColumnName("apunte_id");
        builder.Property(i => i.AsientoId).HasColumnName("asiento_id");
        builder.Property(i => i.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(i => i.CuentaCodigo).HasColumnName("cuenta_codigo").HasMaxLength(Cuenta.LongitudMaximaCodigo).IsRequired();
        builder.Property(i => i.Naturaleza).HasColumnName("naturaleza").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(i => i.CentroId).HasColumnName("centro_id").IsRequired();
        builder.Property(i => i.PartidaId).HasColumnName("partida_id");
        builder.Property(i => i.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(i => i.Origen).HasColumnName("origen").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(i => i.EjecucionId).HasColumnName("ejecucion_id");

        // Las claves foráneas al apunte y al asiento (tablas del asiento, con apuntes como tipo
        // propiedad en EF) se crean en la migración; aquí solo sus índices.
        builder.HasOne<CentroAnalitico>().WithMany().HasForeignKey(i => i.CentroId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<PartidaAnalitica>().WithMany().HasForeignKey(i => i.PartidaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EjecucionAnalitica>().WithMany().HasForeignKey(i => i.EjecucionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(i => i.ApunteId).HasDatabaseName("ix_imputacion_analitica_apunte");
        builder.HasIndex(i => i.AsientoId).HasDatabaseName("ix_imputacion_analitica_asiento");
        builder.HasIndex(i => new { i.EmpresaId, i.Fecha }).HasDatabaseName("ix_imputacion_analitica_empresa_fecha");
        builder.Ignore(i => i.EventosDominio);
    }
}

/// <summary>Longitudes de los maestros analíticos (las del dominio).</summary>
internal static class MaestroAnaliticoLimites
{
    public const int Codigo = 20;
    public const int Nombre = 100;
}

internal sealed class RepositorioAnalitica : IRepositorioAnalitica
{
    /// <summary>Asientos que no son de gestión: los del cierre del ejercicio.</summary>
    private static readonly string[] OrigenesExcluidos = ["Regularizacion", "Cierre", "Apertura"];

    private readonly ContabilidadDbContext _contexto;

    public RepositorioAnalitica(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<CentroAnalitico>> CentrosAsync(CancellationToken ct = default) =>
        await _contexto.CentrosAnaliticos.ToListAsync(ct).ConfigureAwait(false);

    public Task<CentroAnalitico?> CentroAsync(Guid id, CancellationToken ct = default) =>
        _contexto.CentrosAnaliticos.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Agregar(CentroAnalitico centro) => _contexto.CentrosAnaliticos.Add(centro);

    public async Task<IReadOnlyList<PartidaAnalitica>> PartidasAsync(CancellationToken ct = default) =>
        await _contexto.PartidasAnaliticas.ToListAsync(ct).ConfigureAwait(false);

    public Task<PartidaAnalitica?> PartidaAsync(Guid id, CancellationToken ct = default) =>
        _contexto.PartidasAnaliticas.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PartidaAnalitica partida) => _contexto.PartidasAnaliticas.Add(partida);

    public async Task<IReadOnlyList<ClaveReparto>> ClavesAsync(CancellationToken ct = default) =>
        await _contexto.ClavesReparto.ToListAsync(ct).ConfigureAwait(false);

    public Task<ClaveReparto?> ClaveAsync(Guid id, CancellationToken ct = default) =>
        _contexto.ClavesReparto.SingleOrDefaultAsync(c => c.Id == id, ct);

    public void Agregar(ClaveReparto clave) => _contexto.ClavesReparto.Add(clave);

    public void EliminarMaestro(object maestro) => _contexto.Remove(maestro);

    public async Task<IReadOnlyList<ReglaAnalitica>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.ReglasAnaliticas.Where(r => r.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<ReglaAnalitica?> ReglaAsync(Guid id, CancellationToken ct = default) =>
        _contexto.ReglasAnaliticas.SingleOrDefaultAsync(r => r.Id == id, ct);

    public void Agregar(ReglaAnalitica regla) => _contexto.ReglasAnaliticas.Add(regla);

    public void Eliminar(ReglaAnalitica regla) => _contexto.ReglasAnaliticas.Remove(regla);

    public async Task<IReadOnlyList<PeriodoAnalitico>> PeriodosAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.PeriodosAnaliticos.Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PeriodoAnalitico?> PeriodoAsync(Guid id, CancellationToken ct = default) =>
        _contexto.PeriodosAnaliticos.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PeriodoAnalitico periodo) => _contexto.PeriodosAnaliticos.Add(periodo);

    public async Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await _contexto.ImputacionesAnaliticas.Where(i => i.EmpresaId == empresaId && i.Fecha >= desde && i.Fecha <= hasta).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesDeApunteAsync(Guid apunteId, CancellationToken ct = default) =>
        await _contexto.ImputacionesAnaliticas.Where(i => i.ApunteId == apunteId).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesDeEjecucionAsync(Guid ejecucionId, CancellationToken ct = default) =>
        await _contexto.ImputacionesAnaliticas.Where(i => i.EjecucionId == ejecucionId).ToListAsync(ct).ConfigureAwait(false);

    public void Agregar(ImputacionAnalitica imputacion) => _contexto.ImputacionesAnaliticas.Add(imputacion);

    public void Eliminar(ImputacionAnalitica imputacion) => _contexto.ImputacionesAnaliticas.Remove(imputacion);

    public async Task<IReadOnlyList<EjecucionAnalitica>> EjecucionesAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.EjecucionesAnaliticas.Where(e => e.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<EjecucionAnalitica?> EjecucionAsync(Guid id, CancellationToken ct = default) =>
        _contexto.EjecucionesAnaliticas.SingleOrDefaultAsync(e => e.Id == id, ct);

    public void Agregar(EjecucionAnalitica ejecucion) => _contexto.EjecucionesAnaliticas.Add(ejecucion);

    public void Eliminar(EjecucionAnalitica ejecucion) => _contexto.EjecucionesAnaliticas.Remove(ejecucion);

    public async Task<IReadOnlyList<ApunteAnalitico>> ApuntesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var filas = await _contexto.Asientos
            .Where(a => a.EmpresaId == empresaId && a.Fecha >= desde && a.Fecha <= hasta && !OrigenesExcluidos.Contains(a.Origen))
            .SelectMany(a => a.Apuntes
                .Where(p => EF.Functions.Like(p.CuentaCodigo, "6%") || EF.Functions.Like(p.CuentaCodigo, "7%"))
                .Select(p => new { p.Id, AsientoId = a.Id, a.Numero, a.Fecha, p.CuentaCodigo, p.Concepto, p.Debe, p.Haber }))
            .ToListAsync(ct).ConfigureAwait(false);
        return await ConContextoAsync(filas.Select(f => (f.Id, f.AsientoId, f.Numero, f.Fecha, f.CuentaCodigo, f.Concepto, f.Debe, f.Haber)).ToList(), ct)
            .ConfigureAwait(false);
    }

    public async Task<ApunteAnalitico?> ApunteAsync(Guid apunteId, CancellationToken ct = default)
    {
        var fila = await _contexto.Asientos
            .SelectMany(a => a.Apuntes.Where(p => p.Id == apunteId)
                .Select(p => new { p.Id, AsientoId = a.Id, a.Numero, a.Fecha, p.CuentaCodigo, p.Concepto, p.Debe, p.Haber }))
            .SingleOrDefaultAsync(ct).ConfigureAwait(false);
        return fila is null
            ? null
            : (await ConContextoAsync([(fila.Id, fila.AsientoId, fila.Numero, fila.Fecha, fila.CuentaCodigo, fila.Concepto, fila.Debe, fila.Haber)], ct)
                .ConfigureAwait(false))[0];
    }

    /// <summary>Añade a cada apunte el tercero, la actividad y la familia de su documento (si vino de uno).</summary>
    private async Task<IReadOnlyList<ApunteAnalitico>> ConContextoAsync(
        IReadOnlyList<(Guid Id, Guid AsientoId, int Numero, DateOnly Fecha, string Cuenta, string? Concepto, decimal Debe, decimal Haber)> filas,
        CancellationToken ct)
    {
        var asientos = filas.Select(f => (Guid?)f.AsientoId).Distinct().ToList();
        var documentos = asientos.Count == 0
            ? []
            : await _contexto.DocumentosPendientes
                .Where(d => asientos.Contains(d.AsientoId))
                .Select(d => new { d.AsientoId, d.TerceroId, d.ActividadNegocioId, d.Familia })
                .ToListAsync(ct).ConfigureAwait(false);
        var porAsiento = documentos.GroupBy(d => d.AsientoId!.Value).ToDictionary(g => g.Key, g => g.First());

        return filas.Select(f =>
        {
            var doc = porAsiento.GetValueOrDefault(f.AsientoId);
            return new ApunteAnalitico(f.Id, f.AsientoId, f.Numero, f.Fecha, f.Cuenta, f.Concepto,
                MotorAnalitico.ImporteNatural(f.Cuenta, f.Debe, f.Haber), doc?.TerceroId, doc?.ActividadNegocioId, doc?.Familia);
        }).ToList();
    }
}

internal sealed class ConfiguracionPresupuestoContable : IEntityTypeConfiguration<PresupuestoContable>
{
    public void Configure(EntityTypeBuilder<PresupuestoContable> builder)
    {
        builder.ToTable("presupuesto_contable");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(p => p.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(p => p.Codigo).HasColumnName("codigo").HasMaxLength(MaestroAnaliticoLimites.Codigo).IsRequired();
        builder.Property(p => p.Nombre).HasColumnName("nombre").HasMaxLength(MaestroAnaliticoLimites.Nombre).IsRequired();
        builder.Property(p => p.Desde).HasColumnName("desde").IsRequired();
        builder.Property(p => p.Meses).HasColumnName("meses").IsRequired();
        builder.Property(p => p.Estado).HasColumnName("estado").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(p => p.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.OwnsMany(p => p.Lineas, l =>
        {
            l.ToTable("linea_presupuesto");
            l.WithOwner().HasForeignKey("presupuesto_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.CuentaCodigo).HasColumnName("cuenta_codigo").HasMaxLength(12).IsRequired();
            l.Property(x => x.CentroId).HasColumnName("centro_id");
            l.Property(x => x.PartidaId).HasColumnName("partida_id");
            l.Property(x => x.Importes).HasColumnName("importes").HasColumnType("numeric(14,2)[]").IsRequired();
            l.HasOne<CentroAnalitico>().WithMany().HasForeignKey(x => x.CentroId).OnDelete(DeleteBehavior.Restrict);
            l.HasOne<PartidaAnalitica>().WithMany().HasForeignKey(x => x.PartidaId).OnDelete(DeleteBehavior.Restrict);
            l.HasIndex("presupuesto_id").HasDatabaseName("ix_linea_presupuesto_presupuesto");
            l.Ignore(x => x.Naturaleza);
            l.Ignore(x => x.Total);
        });
        builder.Navigation(p => p.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasIndex(p => new { p.EmpresaId, p.Codigo }).IsUnique().HasDatabaseName("ux_presupuesto_contable_empresa_codigo");
        builder.Ignore(p => p.Hasta);
        builder.Ignore(p => p.EventosDominio);
    }
}

internal sealed class RepositorioPresupuestosContables : IRepositorioPresupuestosContables
{
    private readonly ContabilidadDbContext _contexto;

    public RepositorioPresupuestosContables(ContabilidadDbContext contexto) => _contexto = contexto;

    public async Task<IReadOnlyList<PresupuestoContable>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        await _contexto.PresupuestosContables.Where(p => p.EmpresaId == empresaId).ToListAsync(ct).ConfigureAwait(false);

    public Task<PresupuestoContable?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        _contexto.PresupuestosContables.SingleOrDefaultAsync(p => p.Id == id, ct);

    public void Agregar(PresupuestoContable presupuesto) => _contexto.PresupuestosContables.Add(presupuesto);

    public void Eliminar(PresupuestoContable presupuesto) => _contexto.PresupuestosContables.Remove(presupuesto);
}
