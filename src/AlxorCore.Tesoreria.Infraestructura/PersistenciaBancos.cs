using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Tesoreria.Infraestructura;

// ------------------------------------------------------------------ cuentas bancarias

internal sealed class ConfiguracionCuentaBancaria : IEntityTypeConfiguration<CuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CuentaBancaria> builder)
    {
        builder.ToTable("cuenta_bancaria");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(c => c.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(c => c.Tipo).HasColumnName("tipo").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(c => c.Nombre).HasColumnName("nombre").HasMaxLength(CuentaBancaria.LongitudNombre).IsRequired();
        builder.Property(c => c.Iban).HasColumnName("iban").HasMaxLength(34);
        builder.Property(c => c.Bic).HasColumnName("bic").HasMaxLength(11);
        builder.Property(c => c.Subcuenta).HasColumnName("subcuenta").HasMaxLength(12).IsRequired();
        builder.Property(c => c.Activa).HasColumnName("activa").IsRequired();
        builder.Property(c => c.Predeterminada).HasColumnName("predeterminada").IsRequired();
        builder.Property(c => c.SaldoInicial).HasColumnName("saldo_inicial").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(c => c.FechaSaldoInicial).HasColumnName("fecha_saldo_inicial");
        builder.Property(c => c.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasIndex(c => new { c.EmpresaId, c.Subcuenta }).IsUnique().HasDatabaseName("ux_cuenta_bancaria_subcuenta");
        builder.HasIndex(c => new { c.EmpresaId, c.Iban }).IsUnique().HasFilter("iban IS NOT NULL").HasDatabaseName("ux_cuenta_bancaria_iban");
        builder.HasIndex(c => c.EmpresaId).IsUnique().HasFilter("predeterminada").HasDatabaseName("ux_cuenta_bancaria_predeterminada");
        builder.Ignore(c => c.EventosDominio);
    }
}

internal sealed class RepositorioCuentasBancarias : IRepositorioCuentasBancarias
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioCuentasBancarias(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(CuentaBancaria cuenta) => _ctx.CuentasBancarias.Add(cuenta);

    public void Eliminar(CuentaBancaria cuenta) => _ctx.CuentasBancarias.Remove(cuenta);

    public Task<CuentaBancaria?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.CuentasBancarias.SingleOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<CuentaBancaria>> ListarAsync(CancellationToken ct = default) =>
        await _ctx.CuentasBancarias.ToListAsync(ct).ConfigureAwait(false);

    public async Task<bool> EnUsoAsync(Guid id, CancellationToken ct = default) =>
        await _ctx.Movimientos.AnyAsync(m => m.CuentaBancariaId == id, ct).ConfigureAwait(false)
        || await _ctx.Remesas.AnyAsync(r => r.CuentaBancariaId == id, ct).ConfigureAwait(false)
        || await _ctx.Extractos.AnyAsync(e => e.CuentaBancariaId == id, ct).ConfigureAwait(false);

    public async Task<decimal> NetoMovimientosAsync(Guid? cuentaBancariaId, DateOnly? desde, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Movimientos
            .Where(m => m.CuentaBancariaId == cuentaBancariaId && m.Fecha <= hasta && (desde == null || m.Fecha >= desde))
            .SumAsync(m => (decimal?)(m.Sentido == SentidoMovimiento.Cobro ? m.Importe : -m.Importe), ct).ConfigureAwait(false) ?? 0m;

    public async Task<decimal> NetoApuntesConAsientoAsync(Guid cuentaBancariaId, DateOnly? desde, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Apuntes
            .Where(a => a.CuentaBancariaId == cuentaBancariaId && a.Estado == EstadoApunte.ConAsiento && a.Fecha <= hasta && (desde == null || a.Fecha >= desde))
            .SumAsync(a => (decimal?)a.Importe, ct).ConfigureAwait(false) ?? 0m;
}

// ------------------------------------------------------------------ remesas

internal sealed class ConfiguracionRemesa : IEntityTypeConfiguration<Remesa>
{
    public void Configure(EntityTypeBuilder<Remesa> builder)
    {
        builder.ToTable("remesa");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(r => r.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(r => r.Tipo).HasColumnName("tipo").HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(r => r.Ejercicio).HasColumnName("ejercicio").IsRequired();
        builder.Property(r => r.Numero).HasColumnName("numero").IsRequired();
        builder.Property(r => r.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(r => r.FechaCargo).HasColumnName("fecha_cargo").IsRequired();
        builder.Property(r => r.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
        builder.Property(r => r.Esquema).HasColumnName("esquema").HasMaxLength(4);
        builder.Property(r => r.Secuencia).HasColumnName("secuencia").HasMaxLength(4);
        builder.Property(r => r.Estado).HasColumnName("estado").HasMaxLength(12).HasConversion<string>().IsRequired();
        builder.Property(r => r.Total).HasColumnName("total").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(r => r.Fichero).HasColumnName("fichero").IsRequired();
        builder.Property(r => r.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(120).IsRequired();
        builder.Property(r => r.MensajeId).HasColumnName("mensaje_id").HasMaxLength(35).IsRequired();
        builder.Property(r => r.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.Property(r => r.PresentadaEn).HasColumnName("presentada_en");
        builder.Property(r => r.FechaLiquidacion).HasColumnName("fecha_liquidacion");
        builder.Property(r => r.AnuladaEn).HasColumnName("anulada_en");
        builder.HasOne<CuentaBancaria>().WithMany().HasForeignKey(r => r.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(r => r.CuentaBancariaId).HasDatabaseName("ix_remesa_cuenta_bancaria");
        builder.HasIndex(r => new { r.EmpresaId, r.Tipo, r.Ejercicio, r.Numero }).IsUnique().HasDatabaseName("ux_remesa_numero");
        builder.OwnsMany(r => r.Lineas, l =>
        {
            l.ToTable("linea_remesa");
            l.WithOwner().HasForeignKey("remesa_id");
            l.HasKey(x => x.Id);
            l.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            l.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
            l.Property(x => x.DocumentoId).HasColumnName("documento_id").IsRequired();
            l.Property(x => x.Documento).HasColumnName("documento").HasMaxLength(80).IsRequired();
            l.Property(x => x.TerceroNombre).HasColumnName("tercero_nombre").HasMaxLength(200).IsRequired();
            l.Property(x => x.Iban).HasColumnName("iban").HasMaxLength(34);
            l.Property(x => x.Mandato).HasColumnName("mandato").HasMaxLength(35);
            l.Property(x => x.MandatoFecha).HasColumnName("mandato_fecha");
            l.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            l.Property(x => x.MovimientoId).HasColumnName("movimiento_id");
            l.Property(x => x.Viva).HasColumnName("viva").IsRequired();
            l.HasIndex("remesa_id").HasDatabaseName("ix_linea_remesa_remesa");
            l.HasIndex(x => x.MovimientoId).IsUnique().HasFilter("movimiento_id IS NOT NULL").HasDatabaseName("ux_linea_remesa_movimiento");
            l.HasIndex(x => new { x.TipoDocumento, x.DocumentoId }).IsUnique().HasFilter("viva").HasDatabaseName("ux_linea_remesa_documento_viva");
        });
        builder.Navigation(r => r.Lineas).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(r => r.Codigo);
        builder.Ignore(r => r.EstaViva);
        builder.Ignore(r => r.EventosDominio);
    }
}

internal sealed class RepositorioRemesas : IRepositorioRemesas
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioRemesas(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(Remesa remesa) => _ctx.Remesas.Add(remesa);

    public Task<Remesa?> ObtenerAsync(Guid id, CancellationToken ct = default) => _ctx.Remesas.SingleOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IReadOnlyList<Remesa>> ListarAsync(TipoRemesa? tipo, CancellationToken ct = default) =>
        await _ctx.Remesas.AsNoTracking().Where(r => tipo == null || r.Tipo == tipo)
            .OrderByDescending(r => r.Ejercicio).ThenByDescending(r => r.Numero).ToListAsync(ct).ConfigureAwait(false);

    public async Task<int> SiguienteNumeroAsync(TipoRemesa tipo, int ejercicio, CancellationToken ct = default) =>
        (await _ctx.Remesas.Where(r => r.Tipo == tipo && r.Ejercicio == ejercicio).MaxAsync(r => (int?)r.Numero, ct).ConfigureAwait(false) ?? 0) + 1;

    public async Task<IReadOnlyDictionary<Guid, string>> DocumentosVivosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default)
    {
        if (documentoIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var ids = documentoIds.ToList();
        var filas = await _ctx.Remesas.AsNoTracking()
            .Where(r => r.Estado == EstadoRemesa.Generada || r.Estado == EstadoRemesa.Presentada)
            .SelectMany(r => r.Lineas.Where(l => l.Viva && l.TipoDocumento == tipo && ids.Contains(l.DocumentoId)).Select(l => new { l.DocumentoId, r.Ejercicio, r.Numero }))
            .ToListAsync(ct).ConfigureAwait(false);
        return filas.GroupBy(f => f.DocumentoId).ToDictionary(g => g.Key, g => $"{g.First().Ejercicio}/{g.First().Numero}");
    }

    public Task<Remesa?> DeMovimientoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Remesas.AsNoTracking().FirstOrDefaultAsync(r => r.Lineas.Any(l => l.MovimientoId == movimientoId), ct);
}

// ------------------------------------------------------------------ devoluciones

internal sealed class ConfiguracionDevolucion : IEntityTypeConfiguration<DevolucionRecibo>
{
    public void Configure(EntityTypeBuilder<DevolucionRecibo> builder)
    {
        builder.ToTable("devolucion_recibo");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(d => d.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(d => d.MovimientoId).HasColumnName("movimiento_id").IsRequired();
        builder.Property(d => d.AnulacionMovimientoId).HasColumnName("anulacion_movimiento_id").IsRequired();
        builder.Property(d => d.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
        builder.Property(d => d.DocumentoId).HasColumnName("documento_id").IsRequired();
        builder.Property(d => d.RemesaId).HasColumnName("remesa_id");
        builder.Property(d => d.CuentaBancariaId).HasColumnName("cuenta_bancaria_id");
        builder.Property(d => d.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(d => d.Motivo).HasColumnName("motivo").HasMaxLength(4).IsRequired();
        builder.Property(d => d.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.Gastos).HasColumnName("gastos").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(d => d.GastosRepercutidos).HasColumnName("gastos_repercutidos").IsRequired();
        builder.Property(d => d.EfectoGastosId).HasColumnName("efecto_gastos_id");
        builder.Property(d => d.Nota).HasColumnName("nota").HasMaxLength(200);
        builder.Property(d => d.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasOne<Movimiento>().WithMany().HasForeignKey(d => d.MovimientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Movimiento>().WithMany().HasForeignKey(d => d.AnulacionMovimientoId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Remesa>().WithMany().HasForeignKey(d => d.RemesaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<CuentaBancaria>().WithMany().HasForeignKey(d => d.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EfectoCartera>().WithMany().HasForeignKey(d => d.EfectoGastosId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(d => d.MovimientoId).IsUnique().HasDatabaseName("ux_devolucion_recibo_movimiento");
        builder.HasIndex(d => d.AnulacionMovimientoId).IsUnique().HasDatabaseName("ux_devolucion_recibo_anulacion");
        builder.HasIndex(d => d.RemesaId).HasDatabaseName("ix_devolucion_recibo_remesa");
        builder.HasIndex(d => d.CuentaBancariaId).HasDatabaseName("ix_devolucion_recibo_cuenta_bancaria");
        builder.HasIndex(d => d.EfectoGastosId).HasDatabaseName("ix_devolucion_recibo_efecto_gastos");
        builder.HasIndex(d => new { d.EmpresaId, d.TipoDocumento, d.DocumentoId }).HasDatabaseName("ix_devolucion_recibo_documento");
        builder.Ignore(d => d.EventosDominio);
    }
}

internal sealed class RepositorioDevoluciones : IRepositorioDevoluciones
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioDevoluciones(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(DevolucionRecibo devolucion) => _ctx.Devoluciones.Add(devolucion);

    public Task<bool> ExisteDeMovimientoAsync(Guid movimientoId, CancellationToken ct = default) =>
        _ctx.Devoluciones.AnyAsync(d => d.MovimientoId == movimientoId, ct);

    public async Task<IReadOnlyList<DevolucionRecibo>> ListarAsync(CancellationToken ct = default) =>
        await _ctx.Devoluciones.AsNoTracking().ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<DevolucionRecibo>> DeDocumentosAsync(TipoDocumentoTesoreria tipo, IReadOnlyCollection<Guid> documentoIds, CancellationToken ct = default)
    {
        var ids = documentoIds.ToList();
        return await _ctx.Devoluciones.AsNoTracking().Where(d => d.TipoDocumento == tipo && ids.Contains(d.DocumentoId)).ToListAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> MotivosPorMovimientoAsync(IReadOnlyCollection<Guid> movimientoIds, CancellationToken ct = default)
    {
        var ids = movimientoIds.ToList();
        return await _ctx.Devoluciones.AsNoTracking().Where(d => ids.Contains(d.MovimientoId))
            .ToDictionaryAsync(d => d.MovimientoId, d => d.Motivo, ct).ConfigureAwait(false);
    }
}

// ------------------------------------------------------------------ conciliación bancaria

internal sealed class ConfiguracionExtracto : IEntityTypeConfiguration<ExtractoImportado>
{
    public void Configure(EntityTypeBuilder<ExtractoImportado> builder)
    {
        builder.ToTable("extracto_bancario");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(e => e.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(e => e.CuentaBancariaId).HasColumnName("cuenta_bancaria_id").IsRequired();
        builder.Property(e => e.CuentaFichero).HasColumnName("cuenta_fichero").HasMaxLength(30).IsRequired();
        builder.Property(e => e.Desde).HasColumnName("desde");
        builder.Property(e => e.Hasta).HasColumnName("hasta");
        builder.Property(e => e.SaldoInicial).HasColumnName("saldo_inicial").HasColumnType("numeric(14,2)");
        builder.Property(e => e.SaldoFinal).HasColumnName("saldo_final").HasColumnType("numeric(14,2)");
        builder.Property(e => e.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Huella).HasColumnName("huella").HasMaxLength(64).IsRequired();
        builder.Property(e => e.NumeroApuntes).HasColumnName("numero_apuntes").IsRequired();
        builder.Property(e => e.CreadoEn).HasColumnName("creado_en").IsRequired();
        builder.HasOne<CuentaBancaria>().WithMany().HasForeignKey(e => e.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.CuentaBancariaId, e.Huella }).IsUnique().HasDatabaseName("ux_extracto_bancario_huella");
        builder.Ignore(e => e.EventosDominio);
    }
}

internal sealed class ConfiguracionApunteBancario : IEntityTypeConfiguration<ApunteBancario>
{
    public void Configure(EntityTypeBuilder<ApunteBancario> builder)
    {
        builder.ToTable("apunte_bancario");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(a => a.EmpresaId).HasColumnName("empresa_id").IsRequired();
        builder.Property(a => a.ExtractoId).HasColumnName("extracto_id").IsRequired();
        builder.Property(a => a.CuentaBancariaId).HasColumnName("cuenta_bancaria_id").IsRequired();
        builder.Property(a => a.Orden).HasColumnName("orden").IsRequired();
        builder.Property(a => a.Fecha).HasColumnName("fecha").IsRequired();
        builder.Property(a => a.FechaValor).HasColumnName("fecha_valor");
        builder.Property(a => a.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
        builder.Property(a => a.Concepto).HasColumnName("concepto").HasMaxLength(ApunteBancario.LongitudConcepto).IsRequired();
        builder.Property(a => a.ConceptoComun).HasColumnName("concepto_comun").HasMaxLength(2);
        builder.Property(a => a.Documento).HasColumnName("documento").HasMaxLength(10);
        builder.Property(a => a.Referencia1).HasColumnName("referencia1").HasMaxLength(12);
        builder.Property(a => a.Referencia2).HasColumnName("referencia2").HasMaxLength(16);
        builder.Property(a => a.Estado).HasColumnName("estado").HasMaxLength(24).HasConversion<string>().IsRequired();
        builder.Property(a => a.CuentaAsiento).HasColumnName("cuenta_asiento").HasMaxLength(12);
        builder.Property(a => a.ConceptoAsiento).HasColumnName("concepto_asiento").HasMaxLength(80);
        builder.Property(a => a.AsientoOrigenId).HasColumnName("asiento_origen_id");
        builder.Property(a => a.ConciliadoEn).HasColumnName("conciliado_en");
        builder.HasOne<ExtractoImportado>().WithMany().HasForeignKey(a => a.ExtractoId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<CuentaBancaria>().WithMany().HasForeignKey(a => a.CuentaBancariaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(a => new { a.ExtractoId, a.Orden }).IsUnique().HasDatabaseName("ux_apunte_bancario_orden");
        builder.HasIndex(a => new { a.CuentaBancariaId, a.Estado }).HasDatabaseName("ix_apunte_bancario_cuenta_estado");
        builder.OwnsMany(a => a.Casaciones, c =>
        {
            c.ToTable("casacion_apunte");
            c.WithOwner().HasForeignKey("apunte_id");
            c.HasKey(x => x.Id);
            c.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
            c.Property(x => x.MovimientoId).HasColumnName("movimiento_id").IsRequired();
            c.Property(x => x.TipoDocumento).HasColumnName("tipo_documento").HasMaxLength(20).HasConversion<string>().IsRequired();
            c.Property(x => x.DocumentoId).HasColumnName("documento_id").IsRequired();
            c.Property(x => x.Importe).HasColumnName("importe").HasColumnType("numeric(14,2)").IsRequired();
            c.Property(x => x.MovimientoCreado).HasColumnName("movimiento_creado").IsRequired();
            c.HasIndex("apunte_id").HasDatabaseName("ix_casacion_apunte_apunte");
            c.HasIndex(x => x.MovimientoId).IsUnique().HasDatabaseName("ux_casacion_apunte_movimiento");
        });
        builder.Navigation(a => a.Casaciones).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(a => a.EventosDominio);
    }
}

internal sealed class RepositorioConciliacion : IRepositorioConciliacion
{
    private readonly TesoreriaDbContext _ctx;

    public RepositorioConciliacion(TesoreriaDbContext ctx) => _ctx = ctx;

    public void Agregar(ExtractoImportado extracto) => _ctx.Extractos.Add(extracto);

    public void Agregar(ApunteBancario apunte) => _ctx.Apuntes.Add(apunte);

    public Task<ExtractoImportado?> ObtenerExtractoAsync(Guid id, CancellationToken ct = default) => _ctx.Extractos.SingleOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<ExtractoImportado>> ListarExtractosAsync(Guid? cuentaBancariaId, CancellationToken ct = default) =>
        await _ctx.Extractos.AsNoTracking().Where(e => cuentaBancariaId == null || e.CuentaBancariaId == cuentaBancariaId)
            .OrderByDescending(e => e.CreadoEn).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyDictionary<Guid, int>> PendientesPorExtractoAsync(IReadOnlyCollection<Guid> extractoIds, CancellationToken ct = default)
    {
        var ids = extractoIds.ToList();
        return await _ctx.Apuntes.AsNoTracking().Where(a => ids.Contains(a.ExtractoId) && a.Estado == EstadoApunte.Pendiente)
            .GroupBy(a => a.ExtractoId).Select(g => new { g.Key, N = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.N, ct).ConfigureAwait(false);
    }

    public Task<bool> ExisteHuellaAsync(Guid cuentaBancariaId, string huella, CancellationToken ct = default) =>
        _ctx.Extractos.AnyAsync(e => e.CuentaBancariaId == cuentaBancariaId && e.Huella == huella, ct);

    public async Task<IReadOnlyList<ApunteBancario>> ApuntesAsync(Guid extractoId, CancellationToken ct = default) =>
        await _ctx.Apuntes.Where(a => a.ExtractoId == extractoId).OrderBy(a => a.Orden).ToListAsync(ct).ConfigureAwait(false);

    public Task<ApunteBancario?> ObtenerApunteAsync(Guid id, CancellationToken ct = default) => _ctx.Apuntes.SingleOrDefaultAsync(a => a.Id == id, ct);

    public void Eliminar(ExtractoImportado extracto, IReadOnlyList<ApunteBancario> apuntes)
    {
        _ctx.Apuntes.RemoveRange(apuntes);
        _ctx.Extractos.Remove(extracto);
    }

    public async Task<IReadOnlyList<Movimiento>> MovimientosLibresAsync(Guid cuentaBancariaId, bool incluirSinCuenta, DateOnly desde, DateOnly hasta, CancellationToken ct = default) =>
        await _ctx.Movimientos.AsNoTracking()
            .Where(m => (m.CuentaBancariaId == cuentaBancariaId || (incluirSinCuenta && m.CuentaBancariaId == null))
                        && m.AnulaMovimientoId == null && m.CuentaPuente == null && m.Fecha >= desde && m.Fecha <= hasta
                        && !_ctx.Movimientos.Any(a => a.AnulaMovimientoId == m.Id)
                        && !_ctx.Apuntes.Any(ap => ap.Casaciones.Any(c => c.MovimientoId == m.Id)))
            .OrderBy(m => m.Fecha).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlySet<Guid>> ConciliadosAsync(IReadOnlyCollection<Guid> movimientoIds, CancellationToken ct = default)
    {
        if (movimientoIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        var ids = movimientoIds.ToList();
        return (await _ctx.Apuntes.AsNoTracking().SelectMany(a => a.Casaciones).Where(c => ids.Contains(c.MovimientoId)).Select(c => c.MovimientoId)
            .ToListAsync(ct).ConfigureAwait(false)).ToHashSet();
    }
}
