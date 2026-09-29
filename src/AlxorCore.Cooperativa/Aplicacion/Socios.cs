using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Cooperativa.Aplicacion;

public sealed record ConfiguracionCooperativaDto(FormaJuridica Forma, decimal AportacionObligatoria, decimal PorcentajeFroMinimo, decimal PorcentajeFepMinimo,
    decimal InteresMaximoCapital, decimal PorcentajeRetencion, BaseRetorno Base, decimal DeduccionMaximaExpulsion, decimal DeduccionMaximaNoJustificada, bool Contabilizar,
    CuentasCooperativa Cuentas)
{
    public static ConfiguracionCooperativaDto De(ConfiguracionCooperativa c) => new(c.Forma, c.AportacionObligatoria, c.PorcentajeFroMinimo, c.PorcentajeFepMinimo,
        c.InteresMaximoCapital, c.PorcentajeRetencion, c.Base, c.DeduccionMaximaExpulsion, c.DeduccionMaximaNoJustificada, c.Contabilizar, c.Cuentas);
}

public sealed record DatosConfiguracionCooperativa(FormaJuridica Forma, decimal AportacionObligatoria = 0m, decimal? PorcentajeFroMinimo = null, decimal? PorcentajeFepMinimo = null,
    decimal InteresMaximoCapital = 9.25m, decimal PorcentajeRetencion = 19m, BaseRetorno? Base = null, decimal DeduccionMaximaExpulsion = 30m,
    decimal DeduccionMaximaNoJustificada = 20m, bool Contabilizar = true, CuentasCooperativa? Cuentas = null);

public sealed record SaldoCapitalDto(decimal ObligatorioSuscrito, decimal ObligatorioDesembolsado, decimal VoluntarioSuscrito, decimal VoluntarioDesembolsado)
{
    public decimal Suscrito => ObligatorioSuscrito + VoluntarioSuscrito;

    public decimal Desembolsado => ObligatorioDesembolsado + VoluntarioDesembolsado;

    public decimal Pendiente => Suscrito - Desembolsado;

    public static SaldoCapitalDto De(IEnumerable<MovimientoCapital> movimientos)
    {
        var s = SaldoCapital.De(movimientos);
        var o = s.First(x => x.Clase == ClaseAportacion.Obligatoria);
        var v = s.First(x => x.Clase == ClaseAportacion.Voluntaria);
        return new(o.Suscrito, o.Desembolsado, v.Suscrito, v.Desembolsado);
    }
}

public sealed record SocioDto(Guid Id, int Numero, Guid ProveedorId, string Nombre, string? Nif, TipoSocio Tipo, DateOnly FechaAlta, DateOnly? FechaBaja, MotivoBaja? MotivoBaja,
    string? Observaciones, SaldoCapitalDto Capital, decimal FaltaParaElMinimo);

public sealed record DatosSocio(Guid ProveedorId, TipoSocio Tipo = TipoSocio.Comun, DateOnly? FechaAlta = null, string? Observaciones = null, decimal? Aportacion = null,
    decimal? Desembolsado = null);

public sealed record DatosCambioSocio(TipoSocio Tipo, string? Observaciones = null);

/// <summary>Baja del socio. Con <paramref name="Reembolsar"/>, se le devuelve todo su capital (con la deducción indicada sobre el obligatorio).</summary>
public sealed record DatosBaja(DateOnly? Fecha, MotivoBaja Motivo, bool Reembolsar = true, decimal PorcentajeDeduccion = 0m);

public sealed record DatosSuscripcion(Guid SocioId, ClaseAportacion Clase, decimal Suscrito, decimal Desembolsado = 0m, DateOnly? Fecha = null, string? Concepto = null);

public sealed record DatosDesembolso(Guid SocioId, ClaseAportacion Clase, decimal Importe, DateOnly? Fecha = null, string? Concepto = null);

/// <summary>Reembolso de capital: sin importe, todo; con importe, se reduce primero lo pendiente de desembolsar y luego lo desembolsado.</summary>
public sealed record DatosReembolso(Guid SocioId, ClaseAportacion Clase, decimal? Importe = null, decimal PorcentajeDeduccion = 0m, DateOnly? Fecha = null, string? Concepto = null);

public sealed record DatosTransmision(Guid DeSocioId, Guid ASocioId, ClaseAportacion Clase, decimal Importe, DateOnly? Fecha = null, string? Concepto = null);

public sealed record DatosAnulacionCapital(string? Motivo, DateOnly? Fecha = null);

public sealed record MovimientoCapitalDto(Guid Id, Guid SocioId, int NumeroSocio, string Socio, DateOnly Fecha, TipoMovimientoCapital Tipo, ClaseAportacion Clase,
    decimal Suscrito, decimal Desembolsado, decimal Deduccion, decimal ADevolver, string Concepto, Guid? GrupoId, Guid? RepartoId, Guid? AnulaId, bool Anulado,
    Guid? AsientoId, decimal SuscritoAcumulado, decimal DesembolsadoAcumulado);

public sealed record CapitalSocioDto(SocioDto Socio, IReadOnlyList<MovimientoCapitalDto> Movimientos);

public sealed record ResumenCapitalDto(int Socios, int SociosActivos, SaldoCapitalDto Total, decimal AportacionObligatoria, int SociosBajoElMinimo, IReadOnlyList<SocioDto> PorSocio);

/// <summary>Fila del libro registro de socios, con los datos de su ficha al día.</summary>
public sealed record LibroSocioDto(int Numero, string Nombre, string? Nif, string? Domicilio, TipoSocio Tipo, DateOnly FechaAlta, DateOnly? FechaBaja, MotivoBaja? MotivoBaja,
    SaldoCapitalDto Capital);

/// <summary>
/// Socios y capital social: alta y baja de socios, suscripciones, desembolsos, reembolsos con deducción, transmisiones y
/// anulaciones (cada operación con su asiento), y los libros registro de socios y de aportaciones al capital.
/// </summary>
public sealed class SociosCooperativa
{
    private readonly IRepositorioCooperativa _repo;
    private readonly IUnidadDeTrabajoCooperativa _unidad;
    private readonly IConsultaProveedores _proveedores;
    private readonly IReloj _reloj;
    private readonly IContabilidadCooperativa? _contabilidad;

    public SociosCooperativa(IRepositorioCooperativa repo, IUnidadDeTrabajoCooperativa unidad, IConsultaProveedores proveedores, IReloj reloj,
        IContabilidadCooperativa? contabilidad = null)
    {
        _repo = repo;
        _unidad = unidad;
        _proveedores = proveedores;
        _reloj = reloj;
        _contabilidad = contabilidad;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    // ------------------------------------------------------------------ Ajustes
    internal static async Task<ConfiguracionCooperativa> ConfiguracionDeAsync(IRepositorioCooperativa repo, Guid empresaId, CancellationToken ct) =>
        await repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false) ?? ConfiguracionCooperativa.PorDefecto(empresaId);

    public async Task<ConfiguracionCooperativaDto> ConfiguracionAsync(Guid empresaId, CancellationToken ct = default) =>
        ConfiguracionCooperativaDto.De(await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false));

    /// <summary>Fija los ajustes. Sin porcentajes de fondos ni base, los de la forma jurídica: 20 % y 5 % por actividad en una cooperativa; nada y por capital en una SAT.</summary>
    public async Task<Resultado<ConfiguracionCooperativaDto>> FijarConfiguracionAsync(Guid empresaId, DatosConfiguracionCooperativa d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var config = await _repo.ConfiguracionAsync(empresaId, ct).ConfigureAwait(false);
        var nueva = config is null;
        config ??= ConfiguracionCooperativa.PorDefecto(empresaId);
        var sat = d.Forma == FormaJuridica.Sat;
        var r = config.Fijar(d.Forma, d.AportacionObligatoria, d.PorcentajeFroMinimo ?? (sat ? 0m : 20m), d.PorcentajeFepMinimo ?? (sat ? 0m : 5m), d.InteresMaximoCapital,
            d.PorcentajeRetencion, d.Base ?? (sat ? BaseRetorno.Capital : BaseRetorno.Kilos), d.DeduccionMaximaExpulsion, d.DeduccionMaximaNoJustificada, d.Contabilizar,
            (d.Cuentas ?? config.Cuentas).Limpias());
        if (r.EsFallo)
        {
            return Resultado.Fallo<ConfiguracionCooperativaDto>(r.Error);
        }

        if (nueva)
        {
            _repo.Agregar(config);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ConfiguracionCooperativaDto.De(config));
    }

    // ------------------------------------------------------------------ Socios
    public async Task<IReadOnlyList<SocioDto>> ListarAsync(Guid empresaId, bool incluirBajas, CancellationToken ct = default)
    {
        var config = await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false);
        var movimientos = (await _repo.MovimientosAsync(empresaId, null, ct).ConfigureAwait(false)).ToLookup(m => m.SocioId);
        return (await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false)).Where(s => incluirBajas || !s.DeBaja).OrderBy(s => s.Numero)
            .Select(s => Dto(s, movimientos[s.Id], config)).ToList();
    }

    public async Task<SocioDto?> ObtenerAsync(Guid id, CancellationToken ct = default)
    {
        var s = await _repo.SocioAsync(id, ct).ConfigureAwait(false);
        if (s is null)
        {
            return null;
        }

        var config = await ConfiguracionDeAsync(_repo, s.EmpresaId, ct).ConfigureAwait(false);
        return Dto(s, await _repo.MovimientosAsync(s.EmpresaId, s.Id, ct).ConfigureAwait(false), config);
    }

    /// <summary>Alta de un socio (un proveedor de la empresa) con el siguiente número y, si se indica, su aportación obligatoria.</summary>
    public async Task<Resultado<SocioDto>> CrearAsync(Guid empresaId, DatosSocio d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var proveedor = await _proveedores.ObtenerAsync(d.ProveedorId, ct).ConfigureAwait(false);
        if (proveedor is null)
        {
            return Resultado.Fallo<SocioDto>(Error.NoEncontrado("proveedor.no_encontrado", "El tercero no existe: da de alta antes su ficha de proveedor."));
        }

        await _unidad.BloquearAsync($"cooperativa.socio.{empresaId}", ct).ConfigureAwait(false);
        var socios = await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false);
        if (socios.Any(s => s.ProveedorId == d.ProveedorId && !s.DeBaja))
        {
            return Resultado.Fallo<SocioDto>(Error.Conflicto("socio.duplicado", $"{proveedor.Nombre} ya es socio."));
        }

        var config = await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false);
        if (d.Aportacion is { } aportacion && aportacion < config.AportacionObligatoria)
        {
            return Resultado.Fallo<SocioDto>(Error.Validacion("socio.aportacion_minima",
                $"La aportación obligatoria es de {Redondeo.Formatear(config.AportacionObligatoria, 2)} € como mínimo."));
        }

        var numero = await _repo.UltimoNumeroSocioAsync(empresaId, ct).ConfigureAwait(false) + 1;
        var fecha = d.FechaAlta ?? Hoy;
        var socio = Socio.Crear(empresaId, numero, d.ProveedorId, proveedor.Nombre, proveedor.NifFiscal, d.Tipo, fecha, d.Observaciones);
        if (socio.EsFallo)
        {
            return Resultado.Fallo<SocioDto>(socio.Error);
        }

        _repo.Agregar(socio.Valor);
        var movimientos = new List<MovimientoCapital>();
        if (d.Aportacion is > 0m)
        {
            var m = MovimientoCapital.Suscripcion(empresaId, socio.Valor.Id, fecha, ClaseAportacion.Obligatoria, d.Aportacion.Value, d.Desembolsado ?? 0m,
                "Aportación obligatoria al ingresar", _reloj.AhoraUtc);
            if (m.EsFallo)
            {
                return Resultado.Fallo<SocioDto>(m.Error);
            }

            if (await ContabilizarAsync(config, socio.Valor, m.Valor, ct).ConfigureAwait(false) is { } mal)
            {
                return Resultado.Fallo<SocioDto>(mal);
            }

            _repo.Agregar(m.Valor);
            movimientos.Add(m.Valor);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(socio.Valor, movimientos, config));
    }

    public async Task<Resultado<SocioDto>> ActualizarAsync(Guid id, DatosCambioSocio d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var socio = await _repo.SocioAsync(id, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return Resultado.Fallo<SocioDto>(NoExiste());
        }

        var r = socio.Actualizar(d.Tipo, d.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SocioDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await ObtenerAsync(id, ct).ConfigureAwait(false))!);
    }

    /// <summary>
    /// Baja del socio y, si se pide, reembolso de todo su capital: sobre lo desembolsado obligatorio se aplica la deducción
    /// (como mucho la de los ajustes para el motivo: expulsión o baja no justificada), que va al fondo de reserva.
    /// </summary>
    public async Task<Resultado<SocioDto>> DarDeBajaAsync(Guid id, DatosBaja d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var socio = await _repo.SocioAsync(id, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return Resultado.Fallo<SocioDto>(NoExiste());
        }

        var config = await ConfiguracionDeAsync(_repo, socio.EmpresaId, ct).ConfigureAwait(false);
        var maxima = config.DeduccionMaxima(d.Motivo);
        if (d.PorcentajeDeduccion < 0m || d.PorcentajeDeduccion > maxima)
        {
            return Resultado.Fallo<SocioDto>(Error.Validacion("socio.deduccion",
                $"Con este motivo de baja la deducción va de 0 al {Redondeo.Formatear(maxima, 2)} % de las aportaciones obligatorias."));
        }

        var fecha = d.Fecha ?? Hoy;
        var r = socio.DarDeBaja(fecha, d.Motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<SocioDto>(r.Error);
        }

        if (d.Reembolsar)
        {
            var saldos = SaldoCapital.De(await _repo.MovimientosAsync(socio.EmpresaId, socio.Id, ct).ConfigureAwait(false));
            foreach (var s in saldos.Where(s => s.Suscrito > 0m))
            {
                var deduccion = s.Clase == ClaseAportacion.Obligatoria ? Redondeo.Dos(s.Desembolsado * d.PorcentajeDeduccion / 100m) : 0m;
                var m = MovimientoCapital.Reembolso(socio.EmpresaId, socio.Id, fecha, s.Clase, s.Suscrito, s.Desembolsado, deduccion,
                    $"Reembolso por baja ({Texto(s.Clase)})", _reloj.AhoraUtc);
                if (m.EsFallo)
                {
                    return Resultado.Fallo<SocioDto>(m.Error);
                }

                if (await ContabilizarAsync(config, socio, m.Valor, ct).ConfigureAwait(false) is { } mal)
                {
                    return Resultado.Fallo<SocioDto>(mal);
                }

                _repo.Agregar(m.Valor);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok((await ObtenerAsync(id, ct).ConfigureAwait(false))!);
    }

    /// <summary>Elimina un socio dado de alta por error (sin movimientos de capital ni repartos); si ya los tiene, se le da de baja.</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var socio = await _repo.SocioAsync(id, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        if ((await _repo.MovimientosAsync(socio.EmpresaId, socio.Id, ct).ConfigureAwait(false)).Count > 0
            || (await _repo.RepartosAsync(socio.EmpresaId, ct).ConfigureAwait(false)).Any(r => r.Lineas.Any(l => l.SocioId == id)))
        {
            return Resultado.Fallo(Error.Conflicto("socio.con_movimientos", "El socio tiene movimientos de capital o repartos: dale de baja en lugar de eliminarlo."));
        }

        _repo.Eliminar(socio);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    // ------------------------------------------------------------------ Capital
    public async Task<Resultado<MovimientoCapitalDto>> SuscribirAsync(Guid empresaId, DatosSuscripcion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var (socio, config, error) = await SocioActivoAsync(empresaId, d.SocioId, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(error);
        }

        var m = MovimientoCapital.Suscripcion(empresaId, socio!.Id, d.Fecha ?? Hoy, d.Clase, d.Suscrito, d.Desembolsado, d.Concepto, _reloj.AhoraUtc);
        return await GuardarAsync(config!, socio, m, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<MovimientoCapitalDto>> DesembolsarAsync(Guid empresaId, DatosDesembolso d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var (socio, config, error) = await SocioActivoAsync(empresaId, d.SocioId, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(error);
        }

        var saldo = SaldoCapital.De(await _repo.MovimientosAsync(empresaId, socio!.Id, ct).ConfigureAwait(false)).First(s => s.Clase == d.Clase);
        if (d.Importe > saldo.Pendiente)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(Error.Validacion("capital.sin_pendiente",
                $"Solo quedan {Redondeo.Formatear(saldo.Pendiente, 2)} € suscritos sin desembolsar ({Texto(d.Clase)})."));
        }

        var m = MovimientoCapital.Desembolso(empresaId, socio.Id, d.Fecha ?? Hoy, d.Clase, d.Importe, d.Concepto, _reloj.AhoraUtc);
        return await GuardarAsync(config!, socio, m, ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Reembolso de capital. El obligatorio solo se devuelve con el socio de baja (la baja ya lo reembolsa todo); el
    /// voluntario, cuando se quiera. La deducción solo cabe en el obligatorio.
    /// </summary>
    public async Task<Resultado<MovimientoCapitalDto>> ReembolsarAsync(Guid empresaId, DatosReembolso d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var socio = await _repo.SocioAsync(d.SocioId, ct).ConfigureAwait(false);
        if (socio is null || socio.EmpresaId != empresaId)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(NoExiste());
        }

        if (d.Clase == ClaseAportacion.Obligatoria && !socio.DeBaja)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(Error.Validacion("capital.obligatorio_sin_baja",
                "El capital obligatorio solo se reembolsa al dar de baja al socio."));
        }

        var config = await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false);
        var maxima = d.Clase == ClaseAportacion.Obligatoria && socio.MotivoBaja is { } motivo ? config.DeduccionMaxima(motivo) : 0m;
        if (d.PorcentajeDeduccion < 0m || d.PorcentajeDeduccion > maxima)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(Error.Validacion("socio.deduccion", $"La deducción va de 0 al {Redondeo.Formatear(maxima, 2)} %."));
        }

        var saldo = SaldoCapital.De(await _repo.MovimientosAsync(empresaId, socio.Id, ct).ConfigureAwait(false)).First(s => s.Clase == d.Clase);
        var suscrito = d.Importe ?? saldo.Suscrito;
        if (suscrito <= 0m || suscrito > saldo.Suscrito)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(Error.Validacion("capital.sin_saldo",
                $"El socio tiene {Redondeo.Formatear(saldo.Suscrito, 2)} € suscritos ({Texto(d.Clase)})."));
        }

        var desembolsado = Math.Max(0m, suscrito - saldo.Pendiente);
        var m = MovimientoCapital.Reembolso(empresaId, socio.Id, d.Fecha ?? Hoy, d.Clase, suscrito, desembolsado, Redondeo.Dos(desembolsado * d.PorcentajeDeduccion / 100m),
            d.Concepto, _reloj.AhoraUtc);
        return await GuardarAsync(config, socio, m, ct).ConfigureAwait(false);
    }

    /// <summary>Transmisión de capital desembolsado entre socios (el que recibe tiene que estar de alta). No lleva asiento: el capital total no cambia.</summary>
    public async Task<Resultado<IReadOnlyList<MovimientoCapitalDto>>> TransmitirAsync(Guid empresaId, DatosTransmision d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var de = await _repo.SocioAsync(d.DeSocioId, ct).ConfigureAwait(false);
        var (a, _, error) = await SocioActivoAsync(empresaId, d.ASocioId, ct).ConfigureAwait(false);
        if (de is null || de.EmpresaId != empresaId)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(NoExiste());
        }

        if (error is not null)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(error);
        }

        var saldo = SaldoCapital.De(await _repo.MovimientosAsync(empresaId, de.Id, ct).ConfigureAwait(false)).First(s => s.Clase == d.Clase);
        if (d.Importe > saldo.Desembolsado)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Validacion("capital.sin_saldo",
                $"Solo se transmite capital desembolsado: {de.Nombre} tiene {Redondeo.Formatear(saldo.Desembolsado, 2)} € ({Texto(d.Clase)})."));
        }

        var t = MovimientoCapital.Transmision(empresaId, de.Id, a!.Id, d.Fecha ?? Hoy, d.Clase, d.Importe,
            d.Concepto ?? $"Transmisión de capital del socio {de.Numero} al {a.Numero}", _reloj.AhoraUtc);
        if (t.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(t.Error);
        }

        _repo.Agregar(t.Valor.Salida);
        _repo.Agregar(t.Valor.Entrada);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var nombres = new Dictionary<Guid, Socio> { [de.Id] = de, [a.Id] = a };
        return Resultado.Ok<IReadOnlyList<MovimientoCapitalDto>>([MovDto(t.Valor.Salida, nombres, false), MovDto(t.Valor.Entrada, nombres, false)]);
    }

    /// <summary>
    /// Anula un movimiento de capital (y su asiento). Una transmisión se anula entera; el retorno capitalizado, anulando
    /// su reparto. La base de datos no deja anular si el capital resultante no cuadra (p. ej. lo ya reembolsado).
    /// </summary>
    public async Task<Resultado<IReadOnlyList<MovimientoCapitalDto>>> AnularAsync(Guid id, DatosAnulacionCapital d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var m = await _repo.MovimientoAsync(id, ct).ConfigureAwait(false);
        if (m is null)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.NoEncontrado("capital.no_encontrado", "El movimiento no existe."));
        }

        if (string.IsNullOrWhiteSpace(d.Motivo))
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Validacion("capital.motivo", "Indica el motivo de la anulación."));
        }

        if (m.Tipo == TipoMovimientoCapital.Anulacion)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Conflicto("capital.es_anulacion", "Es una anulación: registra la operación de nuevo."));
        }

        if (m.Tipo == TipoMovimientoCapital.RetornoCapitalizado)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Conflicto("capital.de_reparto", "Es un retorno capitalizado: anula el reparto."));
        }

        var todos = await _repo.MovimientosAsync(m.EmpresaId, null, ct).ConfigureAwait(false);
        var anulados = todos.Where(x => x.AnulaId is not null).Select(x => x.AnulaId!.Value).ToHashSet();
        if (anulados.Contains(m.Id))
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Conflicto("capital.ya_anulado", "El movimiento ya está anulado."));
        }

        var grupo = m.GrupoId is { } g ? todos.Where(x => x.GrupoId == g && x.AnulaId is null).ToList() : [m];
        var fecha = d.Fecha ?? Hoy;
        if (fecha < m.Fecha)
        {
            return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(Error.Validacion("capital.fecha_anulacion", "La anulación no puede ser anterior al movimiento."));
        }

        var nuevas = new List<MovimientoCapital>();
        foreach (var x in grupo)
        {
            var anulacion = x.Anulacion(fecha, d.Motivo, _reloj.AhoraUtc);
            if (x.AsientoId is { } asiento && _contabilidad is not null)
            {
                var r = await _contabilidad.AnularAsync(x.EmpresaId, asiento, fecha, ct).ConfigureAwait(false);
                if (r.EsFallo)
                {
                    return Resultado.Fallo<IReadOnlyList<MovimientoCapitalDto>>(r.Error);
                }
            }

            _repo.Agregar(anulacion);
            nuevas.Add(anulacion);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        var socios = (await _repo.SociosAsync(m.EmpresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id);
        return Resultado.Ok<IReadOnlyList<MovimientoCapitalDto>>(nuevas.Select(n => MovDto(n, socios, false)).ToList());
    }

    public async Task<CapitalSocioDto?> CapitalSocioAsync(Guid id, CancellationToken ct = default)
    {
        var socio = await ObtenerAsync(id, ct).ConfigureAwait(false);
        if (socio is null)
        {
            return null;
        }

        var empresaId = (await _repo.SocioAsync(id, ct).ConfigureAwait(false))!.EmpresaId;
        return new CapitalSocioDto(socio, await LibroAportacionesAsync(empresaId, id, null, null, ct).ConfigureAwait(false));
    }

    public async Task<ResumenCapitalDto> ResumenAsync(Guid empresaId, CancellationToken ct = default)
    {
        var config = await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false);
        var socios = await ListarAsync(empresaId, true, ct).ConfigureAwait(false);
        var movimientos = await _repo.MovimientosAsync(empresaId, null, ct).ConfigureAwait(false);
        return new ResumenCapitalDto(socios.Count, socios.Count(s => s.FechaBaja is null), SaldoCapitalDto.De(movimientos), config.AportacionObligatoria,
            socios.Count(s => s.FechaBaja is null && s.FaltaParaElMinimo > 0m), socios);
    }

    /// <summary>Libro registro de aportaciones al capital: cada movimiento con el capital acumulado del socio tras él.</summary>
    public async Task<IReadOnlyList<MovimientoCapitalDto>> LibroAportacionesAsync(Guid empresaId, Guid? socioId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var socios = (await _repo.SociosAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(s => s.Id);
        var movimientos = await _repo.MovimientosAsync(empresaId, socioId, ct).ConfigureAwait(false);
        var anulados = movimientos.Where(m => m.AnulaId is not null).Select(m => m.AnulaId!.Value).ToHashSet();
        var lista = new List<MovimientoCapitalDto>();
        foreach (var g in movimientos.GroupBy(m => m.SocioId).OrderBy(g => socios.TryGetValue(g.Key, out var s) ? s.Numero : int.MaxValue))
        {
            decimal suscrito = 0m, desembolsado = 0m;
            foreach (var m in g.OrderBy(m => m.Fecha).ThenBy(m => m.CreadoEn).ThenBy(m => m.Tipo))
            {
                suscrito += m.Suscrito;
                desembolsado += m.Desembolsado;
                if ((desde is null || m.Fecha >= desde) && (hasta is null || m.Fecha <= hasta))
                {
                    lista.Add(MovDto(m, socios, anulados.Contains(m.Id)) with { SuscritoAcumulado = suscrito, DesembolsadoAcumulado = desembolsado });
                }
            }
        }

        return lista;
    }

    /// <summary>Libro registro de socios: todos, también los de baja, con los datos de su ficha al día y su capital.</summary>
    public async Task<IReadOnlyList<LibroSocioDto>> LibroSociosAsync(Guid empresaId, CancellationToken ct = default)
    {
        var lista = new List<LibroSocioDto>();
        foreach (var s in await ListarAsync(empresaId, true, ct).ConfigureAwait(false))
        {
            var p = await _proveedores.ObtenerAsync(s.ProveedorId, ct).ConfigureAwait(false);
            var domicilio = p is null ? null
                : string.Join(", ", new[] { p.Calle, $"{p.CodigoPostal} {p.Poblacion}".Trim(), p.Provincia }.Where(x => !string.IsNullOrWhiteSpace(x)));
            lista.Add(new LibroSocioDto(s.Numero, p?.Nombre ?? s.Nombre, p?.NifFiscal ?? s.Nif, string.IsNullOrWhiteSpace(domicilio) ? null : domicilio, s.Tipo, s.FechaAlta,
                s.FechaBaja, s.MotivoBaja, s.Capital));
        }

        return lista;
    }

    // ------------------------------------------------------------------ Apoyo
    private async Task<(Socio? Socio, ConfiguracionCooperativa? Config, Error? Error)> SocioActivoAsync(Guid empresaId, Guid socioId, CancellationToken ct)
    {
        var socio = await _repo.SocioAsync(socioId, ct).ConfigureAwait(false);
        if (socio is null || socio.EmpresaId != empresaId)
        {
            return (null, null, NoExiste());
        }

        if (socio.DeBaja)
        {
            return (null, null, Error.Conflicto("socio.de_baja", $"{socio.Nombre} está de baja: solo se le puede reembolsar el capital."));
        }

        return (socio, await ConfiguracionDeAsync(_repo, empresaId, ct).ConfigureAwait(false), null);
    }

    private async Task<Resultado<MovimientoCapitalDto>> GuardarAsync(ConfiguracionCooperativa config, Socio socio, Resultado<MovimientoCapital> m, CancellationToken ct)
    {
        if (m.EsFallo)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(m.Error);
        }

        if (await ContabilizarAsync(config, socio, m.Valor, ct).ConfigureAwait(false) is { } mal)
        {
            return Resultado.Fallo<MovimientoCapitalDto>(mal);
        }

        _repo.Agregar(m.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(MovDto(m.Valor, new Dictionary<Guid, Socio> { [socio.Id] = socio }, false));
    }

    /// <summary>Asiento del movimiento: suscripción (103 contra 100), desembolso (572 contra 103) o reembolso (100 contra 103, 552 y el fondo de reserva por la deducción).</summary>
    private async Task<Error?> ContabilizarAsync(ConfiguracionCooperativa config, Socio socio, MovimientoCapital m, CancellationToken ct)
    {
        if (!config.Contabilizar || _contabilidad is null)
        {
            return null;
        }

        var c = config.Cuentas;
        var apuntes = new List<ApunteCooperativa>();
        switch (m.Tipo)
        {
            case TipoMovimientoCapital.Suscripcion:
                apuntes.Add(new(c.DesembolsosPendientes, m.Suscrito, 0m));
                apuntes.Add(new(c.Capital, 0m, m.Suscrito));
                apuntes.Add(new(c.Tesoreria, m.Desembolsado, 0m));
                apuntes.Add(new(c.DesembolsosPendientes, 0m, m.Desembolsado));
                break;
            case TipoMovimientoCapital.Desembolso:
                apuntes.Add(new(c.Tesoreria, m.Desembolsado, 0m));
                apuntes.Add(new(c.DesembolsosPendientes, 0m, m.Desembolsado));
                break;
            case TipoMovimientoCapital.Reembolso:
                apuntes.Add(new(c.Capital, -m.Suscrito, 0m));
                apuntes.Add(new(c.DesembolsosPendientes, 0m, m.Desembolsado - m.Suscrito));
                apuntes.Add(new(c.Reembolsos, 0m, m.ADevolver));
                apuntes.Add(new(c.Fro, 0m, m.Deduccion));
                break;
            default:
                return null;
        }

        var neto = Neto(apuntes);
        if (neto.Count == 0)
        {
            return null;
        }

        var r = await _contabilidad.RegistrarAsync(m.EmpresaId, m.Fecha, $"Socio {socio.Numero} {socio.Nombre}: {m.Concepto}", neto, ct).ConfigureAwait(false);
        if (r.EsFallo)
        {
            return r.Error;
        }

        m.Contabilizado(r.Valor);
        return null;
    }

    /// <summary>Agrupa los apuntes por cuenta y deja su saldo en el debe o en el haber.</summary>
    internal static IReadOnlyList<ApunteCooperativa> Neto(IEnumerable<ApunteCooperativa> apuntes) =>
        apuntes.GroupBy(a => a.Cuenta).Select(g => (g.Key, Saldo: g.Sum(a => a.Debe - a.Haber), Concepto: g.Select(a => a.Concepto).FirstOrDefault(x => x is not null)))
            .Where(x => x.Saldo != 0m)
            .Select(x => new ApunteCooperativa(x.Key, x.Saldo > 0m ? x.Saldo : 0m, x.Saldo < 0m ? -x.Saldo : 0m, x.Concepto))
            .ToList();

    private static SocioDto Dto(Socio s, IEnumerable<MovimientoCapital> movimientos, ConfiguracionCooperativa config)
    {
        var capital = SaldoCapitalDto.De(movimientos);
        var falta = s.DeBaja ? 0m : Math.Max(0m, config.AportacionObligatoria - capital.ObligatorioSuscrito);
        return new SocioDto(s.Id, s.Numero, s.ProveedorId, s.Nombre, s.Nif, s.Tipo, s.FechaAlta, s.FechaBaja, s.MotivoBaja, s.Observaciones, capital, falta);
    }

    private static MovimientoCapitalDto MovDto(MovimientoCapital m, IReadOnlyDictionary<Guid, Socio> socios, bool anulado)
    {
        var s = socios.GetValueOrDefault(m.SocioId);
        return new MovimientoCapitalDto(m.Id, m.SocioId, s?.Numero ?? 0, s?.Nombre ?? "-", m.Fecha, m.Tipo, m.Clase, m.Suscrito, m.Desembolsado, m.Deduccion, m.ADevolver,
            m.Concepto, m.GrupoId, m.RepartoId, m.AnulaId, anulado, m.AsientoId, 0m, 0m);
    }

    internal static string Texto(ClaseAportacion c) => c == ClaseAportacion.Obligatoria ? "obligatorio" : "voluntario";

    private static Error NoExiste() => Error.NoEncontrado("socio.no_encontrado", "El socio no existe.");
}
