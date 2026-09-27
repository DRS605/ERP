using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Contabilidad.Aplicacion;

// --------------------------------------------------------------------------------------------
//  Contratos
// --------------------------------------------------------------------------------------------

/// <summary>Apunte imputable (grupos 6 y 7) con los datos del documento que lo originó.</summary>
public sealed record ApunteAnalitico(
    Guid ApunteId, Guid AsientoId, int NumeroAsiento, DateOnly Fecha, string CuentaCodigo, string? Concepto,
    decimal Importe, Guid? TerceroId, Guid? ActividadNegocioId, string? Familia)
{
    public ContextoImputacion Contexto => new(CuentaCodigo, Fecha, TerceroId, ActividadNegocioId, Familia);
}

/// <summary>Persistencia de la analítica.</summary>
public interface IRepositorioAnalitica
{
    Task<IReadOnlyList<CentroAnalitico>> CentrosAsync(CancellationToken ct = default);

    Task<CentroAnalitico?> CentroAsync(Guid id, CancellationToken ct = default);

    void Agregar(CentroAnalitico centro);

    Task<IReadOnlyList<PartidaAnalitica>> PartidasAsync(CancellationToken ct = default);

    Task<PartidaAnalitica?> PartidaAsync(Guid id, CancellationToken ct = default);

    void Agregar(PartidaAnalitica partida);

    Task<IReadOnlyList<ClaveReparto>> ClavesAsync(CancellationToken ct = default);

    Task<ClaveReparto?> ClaveAsync(Guid id, CancellationToken ct = default);

    void Agregar(ClaveReparto clave);

    /// <summary>Elimina un maestro analítico (centro, partida o clave) ya comprobado que no se usa.</summary>
    void EliminarMaestro(object maestro);

    Task<IReadOnlyList<ReglaAnalitica>> ReglasAsync(Guid empresaId, CancellationToken ct = default);

    Task<ReglaAnalitica?> ReglaAsync(Guid id, CancellationToken ct = default);

    void Agregar(ReglaAnalitica regla);

    void Eliminar(ReglaAnalitica regla);

    Task<IReadOnlyList<PeriodoAnalitico>> PeriodosAsync(Guid empresaId, CancellationToken ct = default);

    Task<PeriodoAnalitico?> PeriodoAsync(Guid id, CancellationToken ct = default);

    void Agregar(PeriodoAnalitico periodo);

    Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesDeApunteAsync(Guid apunteId, CancellationToken ct = default);

    Task<IReadOnlyList<ImputacionAnalitica>> ImputacionesDeEjecucionAsync(Guid ejecucionId, CancellationToken ct = default);

    void Agregar(ImputacionAnalitica imputacion);

    void Eliminar(ImputacionAnalitica imputacion);

    Task<IReadOnlyList<EjecucionAnalitica>> EjecucionesAsync(Guid empresaId, CancellationToken ct = default);

    Task<EjecucionAnalitica?> EjecucionAsync(Guid id, CancellationToken ct = default);

    void Agregar(EjecucionAnalitica ejecucion);

    void Eliminar(EjecucionAnalitica ejecucion);

    /// <summary>Apuntes de gastos e ingresos del periodo (sin regularización, cierre ni apertura).</summary>
    Task<IReadOnlyList<ApunteAnalitico>> ApuntesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default);

    Task<ApunteAnalitico?> ApunteAsync(Guid apunteId, CancellationToken ct = default);
}

// --------------------------------------------------------------------------------------------
//  Vistas
// --------------------------------------------------------------------------------------------

public sealed record CentroAnaliticoDto(Guid Id, string Codigo, string Nombre, TipoCentroAnalitico Tipo, Guid? PadreId, bool Activo)
{
    public static CentroAnaliticoDto Desde(CentroAnalitico c) => new(c.Id, c.Codigo, c.Nombre, c.Tipo, c.PadreId, c.Activo);
}

public sealed record PartidaAnaliticaDto(Guid Id, string Codigo, string Nombre, NaturalezaAnalitica Naturaleza, Guid? PadreId, bool Activo)
{
    public static PartidaAnaliticaDto Desde(PartidaAnalitica p) => new(p.Id, p.Codigo, p.Nombre, p.Naturaleza, p.PadreId, p.Activo);
}

public sealed record ClaveRepartoDto(Guid Id, string Codigo, string Nombre, bool Activa, IReadOnlyList<PorcentajeReparto> Reparto)
{
    public static ClaveRepartoDto Desde(ClaveReparto c) => new(c.Id, c.Codigo, c.Nombre, c.Activa, c.Reparto);
}

public sealed record ReglaAnaliticaDto(
    Guid Id, string Descripcion, string? PrefijoCuenta, Guid? TerceroId, Guid? ActividadNegocioId, string? Familia,
    DateOnly? VigenteDesde, DateOnly? VigenteHasta, Guid? CentroId, Guid? ClaveRepartoId, Guid? PartidaId, int Prioridad)
{
    public static ReglaAnaliticaDto Desde(ReglaAnalitica r) => new(
        r.Id, r.Descripcion, r.PrefijoCuenta, r.TerceroId, r.ActividadNegocioId, r.Familia, r.VigenteDesde, r.VigenteHasta,
        r.CentroId, r.ClaveRepartoId, r.PartidaId, r.Prioridad);
}

public sealed record PeriodoAnaliticoDto(Guid Id, string Codigo, string Nombre, DateOnly Desde, DateOnly Hasta, bool Cerrado)
{
    public static PeriodoAnaliticoDto De(PeriodoAnalitico p) => new(p.Id, p.Codigo, p.Nombre, p.Desde, p.Hasta, p.Cerrado);
}

public sealed record ImputacionDto(Guid Id, Guid CentroId, Guid? PartidaId, decimal Importe, OrigenImputacion Origen)
{
    public static ImputacionDto Desde(ImputacionAnalitica i) => new(i.Id, i.CentroId, i.PartidaId, i.Importe, i.Origen);
}

public sealed record EjecucionAnaliticaDto(
    Guid Id, TipoEjecucionAnalitica Tipo, DateOnly Desde, DateOnly Hasta, string Descripcion, Guid? CentroOrigenId, Guid? ClaveRepartoId,
    DateTimeOffset CreadoEn, int Imputaciones = 0, int SinRegla = 0)
{
    public static EjecucionAnaliticaDto De(EjecucionAnalitica e, int imputaciones = 0, int sinRegla = 0) =>
        new(e.Id, e.Tipo, e.Desde, e.Hasta, e.Descripcion, e.CentroOrigenId, e.ClaveRepartoId, e.CreadoEn, imputaciones, sinRegla);
}

/// <summary>Apunte con importe todavía sin imputar, y la regla que se le aplicaría (si la hay).</summary>
public sealed record PendienteAnaliticoDto(
    Guid ApunteId, int NumeroAsiento, DateOnly Fecha, string CuentaCodigo, string? Concepto, decimal Importe, decimal Pendiente,
    string? ReglaAplicable);

/// <summary>Resultado analítico de un centro o una partida (incluye sus descendientes).</summary>
public sealed record FilaAnaliticaDto(
    Guid? Id, string Codigo, string Nombre, Guid? PadreId, int Nivel, decimal Ingresos, decimal Gastos, decimal Resultado,
    decimal? MargenPorcentaje, IReadOnlyList<CuentaAnaliticaDto> Cuentas);

public sealed record CuentaAnaliticaDto(string CuentaCodigo, decimal Ingresos, decimal Gastos);

/// <summary>Cuenta de resultados analítica del periodo.</summary>
public sealed record InformeAnaliticoDto(
    DateOnly Desde, DateOnly Hasta, DimensionAnalitica Dimension, IReadOnlyList<FilaAnaliticaDto> Filas,
    decimal TotalIngresos, decimal TotalGastos, decimal Resultado, decimal SinAsignarIngresos, decimal SinAsignarGastos, decimal PorcentajeImputado);

/// <summary>Por qué dimensión se agrupa el informe.</summary>
public enum DimensionAnalitica
{
    Centro = 1,
    Partida = 2,
}

// --------------------------------------------------------------------------------------------
//  Imputación automática por reglas (al contabilizar y en el proceso de pendientes)
// --------------------------------------------------------------------------------------------

/// <summary>Aplica las reglas analíticas a apuntes. No guarda: añade las imputaciones a la unidad de trabajo.</summary>
public sealed class ImputadorAnalitico
{
    private readonly IRepositorioAnalitica _repo;

    public ImputadorAnalitico(IRepositorioAnalitica repo) => _repo = repo;

    /// <summary>Imputa por reglas los apuntes indicados. Devuelve (imputados, sin regla).</summary>
    public async Task<(int Imputados, int SinRegla)> ImputarAsync(
        Guid empresaId, IEnumerable<ApunteAnalitico> apuntes, OrigenImputacion origen, Guid? ejecucionId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(apuntes);
        var lista = apuntes.Where(a => a.Importe != 0m && MotorAnalitico.NaturalezaDe(a.CuentaCodigo) is not null).ToList();
        if (lista.Count == 0)
        {
            return (0, 0);
        }

        var reglas = await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false);
        if (reglas.Count == 0)
        {
            return (0, lista.Count);
        }

        var activos = (await _repo.CentrosAsync(ct).ConfigureAwait(false)).Where(c => c.Activo).Select(c => c.Id).ToHashSet();
        var claves = (await _repo.ClavesAsync(ct).ConfigureAwait(false)).Where(c => c.Activa).ToDictionary(c => c.Id);

        int imputados = 0, sinRegla = 0;
        foreach (var apunte in lista)
        {
            var destino = Destino(MotorAnalitico.ReglaQueAplica(reglas, apunte.Contexto), activos, claves);
            if (destino is null)
            {
                sinRegla++;
                continue;
            }

            var (centros, partida) = destino.Value;
            var importes = MotorAnalitico.Repartir(apunte.Importe, centros.Select(c => c.Porcentaje).ToList());
            for (var i = 0; i < centros.Count; i++)
            {
                if (importes[i] != 0m)
                {
                    _repo.Agregar(ImputacionAnalitica.DeApunte(empresaId, apunte.ApunteId, apunte.AsientoId, apunte.Fecha, apunte.CuentaCodigo,
                        centros[i].CentroId, partida, importes[i], origen, ejecucionId));
                }
            }

            imputados++;
        }

        return (imputados, sinRegla);
    }

    /// <summary>Regla que se aplicaría a un apunte (para explicar al usuario qué pasará).</summary>
    public async Task<Func<ApunteAnalitico, string?>> ExplicadorAsync(Guid empresaId, CancellationToken ct = default)
    {
        var reglas = await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false);
        return a => MotorAnalitico.ReglaQueAplica(reglas, a.Contexto)?.Descripcion;
    }

    /// <summary>Imputaciones de un asiento recién creado con los datos de su documento.</summary>
    public static IEnumerable<ApunteAnalitico> ApuntesDe(Asiento asiento, Guid? terceroId, Guid? actividadId, string? familia)
    {
        ArgumentNullException.ThrowIfNull(asiento);
        return asiento.Apuntes
            .Where(p => MotorAnalitico.NaturalezaDe(p.CuentaCodigo) is not null)
            .Select(p => new ApunteAnalitico(p.Id, asiento.Id, asiento.Numero, asiento.Fecha, p.CuentaCodigo, p.Concepto,
                MotorAnalitico.ImporteNatural(p.CuentaCodigo, p.Debe, p.Haber), terceroId, actividadId, familia));
    }

    private static (IReadOnlyList<PorcentajeReparto> Centros, Guid? Partida)? Destino(
        ReglaAnalitica? regla, HashSet<Guid> activos, Dictionary<Guid, ClaveReparto> claves)
    {
        if (regla is null)
        {
            return null;
        }

        if (regla.CentroId is { } centro)
        {
            return activos.Contains(centro) ? ([new PorcentajeReparto(centro, 100m)], regla.PartidaId) : null;
        }

        return regla.ClaveRepartoId is { } claveId && claves.TryGetValue(claveId, out var clave) && clave.Reparto.All(r => activos.Contains(r.CentroId))
            ? (clave.Reparto, regla.PartidaId)
            : null;
    }
}

// --------------------------------------------------------------------------------------------
//  Maestros
// --------------------------------------------------------------------------------------------

public sealed record DatosCentroAnalitico(string? Codigo, string? Nombre, TipoCentroAnalitico Tipo = TipoCentroAnalitico.CentroCoste, Guid? PadreId = null, bool Activo = true);

public sealed record DatosPartidaAnalitica(string? Codigo, string? Nombre, NaturalezaAnalitica Naturaleza = NaturalezaAnalitica.Gasto, Guid? PadreId = null, bool Activo = true);

public sealed record DatosClaveReparto(string? Codigo, string? Nombre, IReadOnlyList<PorcentajeReparto>? Reparto, bool Activa = true);

public sealed record DatosPeriodoAnalitico(string? Codigo, string? Nombre, DateOnly Desde, DateOnly Hasta);

/// <summary>Casos de uso de los maestros analíticos (centros, partidas, claves, reglas y periodos).</summary>
public sealed class MaestrosAnaliticos
{
    private readonly IRepositorioAnalitica _repo;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    private readonly IComprobadorUso? _uso;

    public MaestrosAnaliticos(IRepositorioAnalitica repo, IUnidadDeTrabajoContabilidad unidad, IComprobadorUso? uso = null)
    {
        _repo = repo;
        _unidad = unidad;
        _uso = uso;
    }

    public async Task<Resultado<BajaDto>> EliminarCentroAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.CentroAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.CentroCoste, "centro_analitico", "el centro", ct).ConfigureAwait(false);

    public async Task<Resultado<BajaDto>> EliminarPartidaAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.PartidaAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.PartidaAnalitica, "partida_analitica", "la partida", ct).ConfigureAwait(false);

    public async Task<Resultado<BajaDto>> EliminarClaveAsync(Guid id, CancellationToken ct = default) =>
        await EliminarAsync(await _repo.ClaveAsync(id, ct).ConfigureAwait(false), id, TiposRegistro.ClaveReparto, "clave_reparto", "la clave de reparto", ct).ConfigureAwait(false);

    /// <summary>Un maestro sin uso se elimina; uno usado se desactiva desde su ficha (sigue en los informes).</summary>
    private async Task<Resultado<BajaDto>> EliminarAsync(object? maestro, Guid id, string tipo, string prefijo, string nombre, CancellationToken ct)
    {
        if (maestro is null)
        {
            return Resultado.Fallo<BajaDto>(Error.NoEncontrado(prefijo + ".no_encontrado", $"No existe {nombre}."));
        }

        if (_uso is not null && await _uso.BuscarUsoAsync(tipo, id, ct).ConfigureAwait(false) is { } uso)
        {
            return Resultado.Fallo<BajaDto>(Error.Conflicto(prefijo + Bajas.SufijoEnUso,
                $"No se puede eliminar {nombre} porque ya tiene {uso}. Desactívalo en su ficha: deja de ofrecerse y se conserva en los informes."));
        }

        _repo.EliminarMaestro(maestro);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new BajaDto(id, true, false));
    }

    public async Task<IReadOnlyList<CentroAnaliticoDto>> CentrosAsync(CancellationToken ct = default) =>
        (await _repo.CentrosAsync(ct).ConfigureAwait(false)).OrderBy(c => c.Codigo, StringComparer.Ordinal).Select(CentroAnaliticoDto.Desde).ToList();

    public async Task<Resultado<CentroAnaliticoDto>> CrearCentroAsync(Guid grupoId, DatosCentroAnalitico datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.CentrosAsync(ct).ConfigureAwait(false);
        var error = CodigoRepetido(existentes.Select(c => c.Codigo), datos.Codigo) ?? PadreValido(existentes, null, datos.PadreId);
        if (error is not null)
        {
            return Resultado.Fallo<CentroAnaliticoDto>(error);
        }

        var centro = CentroAnalitico.Crear(grupoId, datos.Codigo, datos.Nombre, datos.Tipo, datos.PadreId);
        if (centro.EsFallo)
        {
            return Resultado.Fallo<CentroAnaliticoDto>(centro.Error);
        }

        _repo.Agregar(centro.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroAnaliticoDto.Desde(centro.Valor));
    }

    public async Task<Resultado<CentroAnaliticoDto>> ActualizarCentroAsync(Guid id, DatosCentroAnalitico datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.CentrosAsync(ct).ConfigureAwait(false);
        var centro = existentes.FirstOrDefault(c => c.Id == id);
        if (centro is null)
        {
            return Resultado.Fallo<CentroAnaliticoDto>(Error.NoEncontrado("centro.no_encontrado", "El centro no existe."));
        }

        var error = PadreValido(existentes, id, datos.PadreId);
        if (error is not null)
        {
            return Resultado.Fallo<CentroAnaliticoDto>(error);
        }

        var r = centro.Actualizar(datos.Nombre, datos.Tipo, datos.PadreId, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CentroAnaliticoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CentroAnaliticoDto.Desde(centro));
    }

    public async Task<IReadOnlyList<PartidaAnaliticaDto>> PartidasAsync(CancellationToken ct = default) =>
        (await _repo.PartidasAsync(ct).ConfigureAwait(false)).OrderBy(p => p.Codigo, StringComparer.Ordinal).Select(PartidaAnaliticaDto.Desde).ToList();

    public async Task<Resultado<PartidaAnaliticaDto>> CrearPartidaAsync(Guid grupoId, DatosPartidaAnalitica datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.PartidasAsync(ct).ConfigureAwait(false);
        var error = CodigoRepetido(existentes.Select(p => p.Codigo), datos.Codigo) ?? PadrePartidaValido(existentes, null, datos.PadreId, datos.Naturaleza);
        if (error is not null)
        {
            return Resultado.Fallo<PartidaAnaliticaDto>(error);
        }

        var partida = PartidaAnalitica.Crear(grupoId, datos.Codigo, datos.Nombre, datos.Naturaleza, datos.PadreId);
        if (partida.EsFallo)
        {
            return Resultado.Fallo<PartidaAnaliticaDto>(partida.Error);
        }

        _repo.Agregar(partida.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PartidaAnaliticaDto.Desde(partida.Valor));
    }

    public async Task<Resultado<PartidaAnaliticaDto>> ActualizarPartidaAsync(Guid id, DatosPartidaAnalitica datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.PartidasAsync(ct).ConfigureAwait(false);
        var partida = existentes.FirstOrDefault(p => p.Id == id);
        if (partida is null)
        {
            return Resultado.Fallo<PartidaAnaliticaDto>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe."));
        }

        var error = PadrePartidaValido(existentes, id, datos.PadreId, partida.Naturaleza);
        if (error is not null)
        {
            return Resultado.Fallo<PartidaAnaliticaDto>(error);
        }

        var r = partida.Actualizar(datos.Nombre, datos.PadreId, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PartidaAnaliticaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PartidaAnaliticaDto.Desde(partida));
    }

    public async Task<IReadOnlyList<ClaveRepartoDto>> ClavesAsync(CancellationToken ct = default) =>
        (await _repo.ClavesAsync(ct).ConfigureAwait(false)).OrderBy(c => c.Codigo, StringComparer.Ordinal).Select(ClaveRepartoDto.Desde).ToList();

    public async Task<Resultado<ClaveRepartoDto>> CrearClaveAsync(Guid grupoId, DatosClaveReparto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.ClavesAsync(ct).ConfigureAwait(false);
        var error = CodigoRepetido(existentes.Select(c => c.Codigo), datos.Codigo) ?? await CentrosExistenAsync(datos.Reparto, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<ClaveRepartoDto>(error);
        }

        var clave = ClaveReparto.Crear(grupoId, datos.Codigo, datos.Nombre, datos.Reparto ?? []);
        if (clave.EsFallo)
        {
            return Resultado.Fallo<ClaveRepartoDto>(clave.Error);
        }

        _repo.Agregar(clave.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ClaveRepartoDto.Desde(clave.Valor));
    }

    public async Task<Resultado<ClaveRepartoDto>> ActualizarClaveAsync(Guid id, DatosClaveReparto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var clave = await _repo.ClaveAsync(id, ct).ConfigureAwait(false);
        if (clave is null)
        {
            return Resultado.Fallo<ClaveRepartoDto>(Error.NoEncontrado("clave.no_encontrada", "La clave de reparto no existe."));
        }

        var error = await CentrosExistenAsync(datos.Reparto, ct).ConfigureAwait(false);
        var r = error is null ? clave.Actualizar(datos.Nombre, datos.Reparto ?? [], datos.Activa) : Resultado.Fallo(error);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ClaveRepartoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ClaveRepartoDto.Desde(clave));
    }

    public async Task<IReadOnlyList<ReglaAnaliticaDto>> ReglasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false))
            .OrderByDescending(r => r.Especificidad).Select(ReglaAnaliticaDto.Desde).ToList();

    public async Task<Resultado<ReglaAnaliticaDto>> GuardarReglaAsync(Guid empresaId, Guid? id, DatosReglaAnalitica datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var error = await DestinoValidoAsync(datos, ct).ConfigureAwait(false);
        if (error is not null)
        {
            return Resultado.Fallo<ReglaAnaliticaDto>(error);
        }

        ReglaAnalitica regla;
        if (id is { } existente)
        {
            var encontrada = await _repo.ReglaAsync(existente, ct).ConfigureAwait(false);
            if (encontrada is null)
            {
                return Resultado.Fallo<ReglaAnaliticaDto>(Error.NoEncontrado("regla.no_encontrada", "La regla no existe."));
            }

            regla = encontrada;
            var r = regla.Aplicar(datos);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ReglaAnaliticaDto>(r.Error);
            }
        }
        else
        {
            var nueva = ReglaAnalitica.Crear(empresaId, datos);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<ReglaAnaliticaDto>(nueva.Error);
            }

            regla = nueva.Valor;
            _repo.Agregar(regla);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaAnaliticaDto.Desde(regla));
    }

    public async Task<Resultado> EliminarReglaAsync(Guid id, CancellationToken ct = default)
    {
        var regla = await _repo.ReglaAsync(id, ct).ConfigureAwait(false);
        if (regla is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("regla.no_encontrada", "La regla no existe."));
        }

        _repo.Eliminar(regla);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<PeriodoAnaliticoDto>> PeriodosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.PeriodosAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(p => p.Desde).Select(PeriodoAnaliticoDto.De).ToList();

    public async Task<Resultado<PeriodoAnaliticoDto>> CrearPeriodoAsync(Guid empresaId, DatosPeriodoAnalitico datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var existentes = await _repo.PeriodosAsync(empresaId, ct).ConfigureAwait(false);
        var solapa = existentes.FirstOrDefault(p => p.Desde <= datos.Hasta && datos.Desde <= p.Hasta);
        if (solapa is not null)
        {
            return Resultado.Fallo<PeriodoAnaliticoDto>(Error.Conflicto("periodo.solapado", $"Se solapa con el periodo {solapa.Codigo} ({solapa.Desde:dd/MM/yyyy}–{solapa.Hasta:dd/MM/yyyy})."));
        }

        var error = CodigoRepetido(existentes.Select(p => p.Codigo), datos.Codigo);
        var periodo = error is null ? PeriodoAnalitico.Crear(empresaId, datos.Codigo, datos.Nombre, datos.Desde, datos.Hasta) : Resultado.Fallo<PeriodoAnalitico>(error);
        if (periodo.EsFallo)
        {
            return Resultado.Fallo<PeriodoAnaliticoDto>(periodo.Error);
        }

        _repo.Agregar(periodo.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PeriodoAnaliticoDto.De(periodo.Valor));
    }

    public async Task<Resultado<PeriodoAnaliticoDto>> CerrarPeriodoAsync(Guid id, bool cerrado, CancellationToken ct = default)
    {
        var periodo = await _repo.PeriodoAsync(id, ct).ConfigureAwait(false);
        if (periodo is null)
        {
            return Resultado.Fallo<PeriodoAnaliticoDto>(Error.NoEncontrado("periodo.no_encontrado", "El periodo no existe."));
        }

        periodo.FijarCerrado(cerrado);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PeriodoAnaliticoDto.De(periodo));
    }

    private static Error? CodigoRepetido(IEnumerable<string> existentes, string? codigo) =>
        codigo is not null && existentes.Contains(codigo.Trim().ToUpperInvariant(), StringComparer.Ordinal)
            ? Error.Conflicto("analitica.codigo_repetido", $"Ya existe el código {codigo.Trim().ToUpperInvariant()}.")
            : null;

    private static Error? PadreValido(IReadOnlyList<CentroAnalitico> centros, Guid? id, Guid? padreId)
    {
        if (padreId is null)
        {
            return null;
        }

        var porId = centros.ToDictionary(c => c.Id);
        if (!porId.ContainsKey(padreId.Value))
        {
            return Error.Validacion("centro.padre", "El centro padre no existe.");
        }

        // Subiendo desde el padre no se puede llegar al propio centro (sería un ciclo).
        for (Guid? actual = padreId; actual is { } a; actual = porId.TryGetValue(a, out var c) ? c.PadreId : null)
        {
            if (a == id)
            {
                return Error.Validacion("centro.padre", "Ese padre crearía un ciclo en el árbol de centros.");
            }
        }

        return null;
    }

    private static Error? PadrePartidaValido(IReadOnlyList<PartidaAnalitica> partidas, Guid? id, Guid? padreId, NaturalezaAnalitica naturaleza)
    {
        if (padreId is null)
        {
            return null;
        }

        var porId = partidas.ToDictionary(p => p.Id);
        if (!porId.TryGetValue(padreId.Value, out var padre))
        {
            return Error.Validacion("partida.padre", "La partida padre no existe.");
        }

        if (padre.Naturaleza != naturaleza)
        {
            return Error.Validacion("partida.padre", "Una partida y su padre deben tener la misma naturaleza (ingreso o gasto).");
        }

        for (Guid? actual = padreId; actual is { } a; actual = porId.TryGetValue(a, out var p) ? p.PadreId : null)
        {
            if (a == id)
            {
                return Error.Validacion("partida.padre", "Ese padre crearía un ciclo en el árbol de partidas.");
            }
        }

        return null;
    }

    private async Task<Error?> CentrosExistenAsync(IReadOnlyList<PorcentajeReparto>? reparto, CancellationToken ct)
    {
        var centros = (await _repo.CentrosAsync(ct).ConfigureAwait(false)).ToDictionary(c => c.Id);
        return (reparto ?? []).FirstOrDefault(r => !centros.ContainsKey(r.CentroId)) is { } falta
            ? Error.Validacion("clave.centro", $"El centro {falta.CentroId} no existe.")
            : null;
    }

    private async Task<Error?> DestinoValidoAsync(DatosReglaAnalitica datos, CancellationToken ct)
    {
        if (datos.CentroId is { } c && await _repo.CentroAsync(c, ct).ConfigureAwait(false) is null)
        {
            return Error.Validacion("regla.centro", "El centro no existe.");
        }

        if (datos.ClaveRepartoId is { } k && await _repo.ClaveAsync(k, ct).ConfigureAwait(false) is null)
        {
            return Error.Validacion("regla.clave", "La clave de reparto no existe.");
        }

        return datos.PartidaId is { } p && await _repo.PartidaAsync(p, ct).ConfigureAwait(false) is null
            ? Error.Validacion("regla.partida", "La partida no existe.")
            : null;
    }
}

// --------------------------------------------------------------------------------------------
//  Imputación manual, procesos y deshacer
// --------------------------------------------------------------------------------------------

/// <summary>Una parte de la imputación manual: por porcentaje o por importe (uno de los dos).</summary>
public sealed record LineaImputacionComando(Guid CentroId, Guid? PartidaId = null, decimal? Porcentaje = null, decimal? Importe = null);

public sealed record RepartoSecundarioComando(Guid CentroOrigenId, Guid ClaveRepartoId, DateOnly Desde, DateOnly Hasta, string? Descripcion = null);

/// <summary>Casos de uso que crean o deshacen imputaciones.</summary>
public sealed class ImputacionesAnaliticas
{
    private readonly IRepositorioAnalitica _repo;
    private readonly ImputadorAnalitico _imputador;
    private readonly IUnidadDeTrabajoContabilidad _unidad;
    private readonly IReloj _reloj;

    public ImputacionesAnaliticas(IRepositorioAnalitica repo, ImputadorAnalitico imputador, IUnidadDeTrabajoContabilidad unidad, IReloj reloj)
    {
        _repo = repo;
        _imputador = imputador;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>Imputa a mano un apunte (sustituye lo que tuviera). Puede quedar parte sin imputar.</summary>
    public async Task<Resultado<IReadOnlyList<ImputacionDto>>> ImputarApunteAsync(
        Guid empresaId, Guid apunteId, IReadOnlyList<LineaImputacionComando> lineas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        var apunte = await _repo.ApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (apunte is null || MotorAnalitico.NaturalezaDe(apunte.CuentaCodigo) is null)
        {
            return Fallo(Error.NoEncontrado("apunte.no_imputable", "El apunte no existe o no es de gastos (6) ni de ingresos (7)."));
        }

        var bloqueo = await PeriodoCerradoAsync(empresaId, apunte.Fecha, ct).ConfigureAwait(false);
        if (bloqueo is not null)
        {
            return Fallo(bloqueo);
        }

        if (lineas.Count == 0 || lineas.Any(l => (l.Porcentaje is null) == (l.Importe is null)))
        {
            return Fallo(Error.Validacion("imputacion.lineas", "Cada parte lleva un porcentaje o un importe (uno de los dos)."));
        }

        var centros = (await _repo.CentrosAsync(ct).ConfigureAwait(false)).ToDictionary(c => c.Id);
        var partidas = (await _repo.PartidasAsync(ct).ConfigureAwait(false)).ToDictionary(p => p.Id);
        foreach (var l in lineas)
        {
            if (!centros.TryGetValue(l.CentroId, out var c) || !c.Activo)
            {
                return Fallo(Error.Validacion("imputacion.centro", "Algún centro no existe o está inactivo."));
            }

            if (l.PartidaId is { } p && (!partidas.TryGetValue(p, out var partida) || partida.Naturaleza != MotorAnalitico.NaturalezaDe(apunte.CuentaCodigo)))
            {
                return Fallo(Error.Validacion("imputacion.partida", "Alguna partida no existe o no es de la naturaleza del apunte (ingreso o gasto)."));
            }
        }

        var importes = ImportesDe(apunte.Importe, lineas);
        var suma = importes.Sum();
        if (Math.Sign(suma) * Math.Sign(apunte.Importe) < 0 || Math.Abs(suma) > Math.Abs(apunte.Importe) || importes.Any(i => Math.Sign(i) * Math.Sign(apunte.Importe) < 0))
        {
            return Fallo(Error.Validacion("imputacion.importe", $"Lo imputado ({suma:F2}) no puede superar el importe del apunte ({apunte.Importe:F2}) ni tener otro signo."));
        }

        foreach (var anterior in await _repo.ImputacionesDeApunteAsync(apunteId, ct).ConfigureAwait(false))
        {
            _repo.Eliminar(anterior);
        }

        var nuevas = lineas.Select((l, i) => ImputacionAnalitica.DeApunte(empresaId, apunte.ApunteId, apunte.AsientoId, apunte.Fecha, apunte.CuentaCodigo,
                l.CentroId, l.PartidaId, importes[i], OrigenImputacion.Manual))
            .Where(i => i.Importe != 0m)
            .ToList();
        nuevas.ForEach(_repo.Agregar);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok<IReadOnlyList<ImputacionDto>>(nuevas.Select(ImputacionDto.Desde).ToList());
    }

    /// <summary>Quita las imputaciones de un apunte (queda pendiente de imputar).</summary>
    public async Task<Resultado> QuitarAsync(Guid empresaId, Guid apunteId, CancellationToken ct = default)
    {
        var imputaciones = await _repo.ImputacionesDeApunteAsync(apunteId, ct).ConfigureAwait(false);
        if (imputaciones.Count > 0 && await PeriodoCerradoAsync(empresaId, imputaciones[0].Fecha, ct).ConfigureAwait(false) is { } bloqueo)
        {
            return Resultado.Fallo(bloqueo);
        }

        foreach (var i in imputaciones)
        {
            _repo.Eliminar(i);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Apuntes del periodo con importe sin imputar, con la regla que se les aplicaría.</summary>
    public async Task<IReadOnlyList<PendienteAnaliticoDto>> PendientesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var apuntes = await _repo.ApuntesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var imputado = (await _repo.ImputacionesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .Where(i => i.ApunteId is not null).GroupBy(i => i.ApunteId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => i.Importe));
        var explicar = await _imputador.ExplicadorAsync(empresaId, ct).ConfigureAwait(false);

        return apuntes
            .Select(a => (Apunte: a, Pendiente: a.Importe - imputado.GetValueOrDefault(a.ApunteId)))
            .Where(x => x.Pendiente != 0m)
            .OrderBy(x => x.Apunte.Fecha).ThenBy(x => x.Apunte.NumeroAsiento)
            .Select(x => new PendienteAnaliticoDto(x.Apunte.ApunteId, x.Apunte.NumeroAsiento, x.Apunte.Fecha, x.Apunte.CuentaCodigo, x.Apunte.Concepto,
                x.Apunte.Importe, x.Pendiente, imputado.ContainsKey(x.Apunte.ApunteId) ? null : explicar(x.Apunte)))
            .ToList();
    }

    /// <summary>
    /// Aplica las reglas vigentes a los apuntes del periodo que no tienen ninguna imputación (los que se
    /// contabilizaron antes de crear la regla, las amortizaciones…). Queda registrado y se puede deshacer.
    /// </summary>
    public async Task<Resultado<EjecucionAnaliticaDto>> ImputarPendientesAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        if (hasta < desde)
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(Error.Validacion("analitica.fechas", "El periodo termina antes de empezar."));
        }

        var apuntes = await _repo.ApuntesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var conImputacion = (await _repo.ImputacionesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false))
            .Where(i => i.ApunteId is not null).Select(i => i.ApunteId!.Value).ToHashSet();
        var cerrados = (await _repo.PeriodosAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Cerrado).ToList();
        var pendientes = apuntes.Where(a => !conImputacion.Contains(a.ApunteId) && !cerrados.Any(p => p.Contiene(a.Fecha))).ToList();

        var ejecucion = EjecucionAnalitica.Imputacion(empresaId, desde, hasta, _reloj.AhoraUtc);
        var (imputados, sinRegla) = await _imputador.ImputarAsync(empresaId, pendientes, OrigenImputacion.Proceso, ejecucion.Id, ct).ConfigureAwait(false);
        if (imputados > 0)
        {
            _repo.Agregar(ejecucion);
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return Resultado.Ok(EjecucionAnaliticaDto.De(ejecucion, imputados, sinRegla));
    }

    /// <summary>
    /// Reparto secundario: traspasa lo imputado a un centro en el periodo (por ejemplo, Administración)
    /// a otros centros con una clave, cuenta a cuenta y partida a partida. El centro origen queda a cero.
    /// </summary>
    public async Task<Resultado<EjecucionAnaliticaDto>> RepartirAsync(Guid empresaId, RepartoSecundarioComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.Hasta < comando.Desde)
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(Error.Validacion("analitica.fechas", "El periodo termina antes de empezar."));
        }

        var clave = await _repo.ClaveAsync(comando.ClaveRepartoId, ct).ConfigureAwait(false);
        var origen = await _repo.CentroAsync(comando.CentroOrigenId, ct).ConfigureAwait(false);
        if (clave is null || origen is null)
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(Error.NoEncontrado("reparto.datos", "El centro de origen o la clave de reparto no existen."));
        }

        if (clave.Reparto.Any(r => r.CentroId == origen.Id))
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(Error.Validacion("reparto.circular", "La clave no puede repartir al propio centro de origen."));
        }

        if (await PeriodoCerradoAsync(empresaId, comando.Hasta, ct).ConfigureAwait(false) is { } bloqueo)
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(bloqueo);
        }

        var grupos = (await _repo.ImputacionesAsync(empresaId, comando.Desde, comando.Hasta, ct).ConfigureAwait(false))
            .Where(i => i.CentroId == origen.Id)
            .GroupBy(i => (i.CuentaCodigo, i.Naturaleza, i.PartidaId))
            .Select(g => (g.Key, Importe: g.Sum(i => i.Importe)))
            .Where(g => g.Importe != 0m)
            .ToList();
        if (grupos.Count == 0)
        {
            return Resultado.Fallo<EjecucionAnaliticaDto>(Error.Conflicto("reparto.vacio", $"El centro {origen.Codigo} no tiene importes que repartir en el periodo."));
        }

        var descripcion = string.IsNullOrWhiteSpace(comando.Descripcion) ? $"Reparto de {origen.Codigo} con la clave {clave.Codigo}" : comando.Descripcion.Trim();
        var ejecucion = EjecucionAnalitica.Reparto(empresaId, comando.Desde, comando.Hasta, descripcion, origen.Id, clave.Id, _reloj.AhoraUtc);
        var n = 0;
        foreach (var ((cuenta, naturaleza, partida), importe) in grupos)
        {
            _repo.Agregar(ImputacionAnalitica.DeReparto(empresaId, ejecucion.Id, comando.Hasta, cuenta, naturaleza, origen.Id, partida, -importe));
            var partes = MotorAnalitico.Repartir(importe, clave.Reparto.Select(r => r.Porcentaje).ToList());
            for (var i = 0; i < partes.Count; i++)
            {
                _repo.Agregar(ImputacionAnalitica.DeReparto(empresaId, ejecucion.Id, comando.Hasta, cuenta, naturaleza, clave.Reparto[i].CentroId, partida, partes[i]));
            }

            n += partes.Count + 1;
        }

        _repo.Agregar(ejecucion);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EjecucionAnaliticaDto.De(ejecucion, n));
    }

    public async Task<IReadOnlyList<EjecucionAnaliticaDto>> EjecucionesAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.EjecucionesAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(e => e.CreadoEn).Select(e => EjecucionAnaliticaDto.De(e)).ToList();

    /// <summary>Deshace una ejecución: borra todas las imputaciones que generó.</summary>
    public async Task<Resultado> DeshacerAsync(Guid empresaId, Guid ejecucionId, CancellationToken ct = default)
    {
        var ejecucion = await _repo.EjecucionAsync(ejecucionId, ct).ConfigureAwait(false);
        if (ejecucion is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("ejecucion.no_encontrada", "La ejecución no existe."));
        }

        var imputaciones = await _repo.ImputacionesDeEjecucionAsync(ejecucionId, ct).ConfigureAwait(false);
        foreach (var fecha in imputaciones.Select(i => i.Fecha).Distinct())
        {
            if (await PeriodoCerradoAsync(empresaId, fecha, ct).ConfigureAwait(false) is { } bloqueo)
            {
                return Resultado.Fallo(bloqueo);
            }
        }

        foreach (var i in imputaciones)
        {
            _repo.Eliminar(i);
        }

        _repo.Eliminar(ejecucion);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private static Resultado<IReadOnlyList<ImputacionDto>> Fallo(Error error) => Resultado.Fallo<IReadOnlyList<ImputacionDto>>(error);

    /// <summary>Importes de cada parte: los porcentajes se reparten al céntimo; los importes se toman tal cual.</summary>
    private static List<decimal> ImportesDe(decimal importeApunte, IReadOnlyList<LineaImputacionComando> lineas)
    {
        var porPorcentaje = lineas.Select((l, i) => (l, i)).Where(x => x.l.Porcentaje is not null).ToList();
        var repartidos = MotorAnalitico.Repartir(importeApunte, porPorcentaje.Select(x => x.l.Porcentaje!.Value).ToList());
        var importes = lineas.Select(l => l.Importe is { } imp ? decimal.Round(imp, 2) : 0m).ToList();
        for (var k = 0; k < porPorcentaje.Count; k++)
        {
            importes[porPorcentaje[k].i] = repartidos[k];
        }

        return importes;
    }

    private async Task<Error?> PeriodoCerradoAsync(Guid empresaId, DateOnly fecha, CancellationToken ct) =>
        (await _repo.PeriodosAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(p => p.Cerrado && p.Contiene(fecha)) is { } p
            ? Error.Conflicto("analitica.periodo_cerrado", $"El periodo analítico {p.Codigo} está cerrado: sus imputaciones no se pueden cambiar.")
            : null;
}

// --------------------------------------------------------------------------------------------
//  Informe
// --------------------------------------------------------------------------------------------

/// <summary>Caso de uso: cuenta de resultados analítica por centro o por partida, con árbol y sin asignar.</summary>
public sealed class InformeAnalitico
{
    private readonly IRepositorioAnalitica _repo;

    public InformeAnalitico(IRepositorioAnalitica repo) => _repo = repo;

    public async Task<InformeAnaliticoDto> EjecutarAsync(
        Guid empresaId, DateOnly desde, DateOnly hasta, DimensionAnalitica dimension = DimensionAnalitica.Centro, CancellationToken ct = default)
    {
        var imputaciones = await _repo.ImputacionesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);
        var apuntes = await _repo.ApuntesAsync(empresaId, desde, hasta, ct).ConfigureAwait(false);

        // Lo que queda sin imputar de cada apunte (los repartos secundarios no cambian el total).
        var imputadoPorApunte = imputaciones.Where(i => i.ApunteId is not null).GroupBy(i => i.ApunteId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => i.Importe));
        var sinAsignar = apuntes
            .Select(a => (a.CuentaCodigo, Naturaleza: MotorAnalitico.NaturalezaDe(a.CuentaCodigo)!.Value, Importe: a.Importe - imputadoPorApunte.GetValueOrDefault(a.ApunteId)))
            .Where(x => x.Importe != 0m)
            .ToList();

        IReadOnlyList<Nodo> nodos;
        Func<ImputacionAnalitica, Guid?> clave;
        if (dimension == DimensionAnalitica.Partida)
        {
            nodos = (await _repo.PartidasAsync(ct).ConfigureAwait(false)).Select(p => new Nodo(p.Id, p.Codigo, p.Nombre, p.PadreId)).ToList();
            clave = i => i.PartidaId;
        }
        else
        {
            nodos = (await _repo.CentrosAsync(ct).ConfigureAwait(false)).Select(c => new Nodo(c.Id, c.Codigo, c.Nombre, c.PadreId)).ToList();
            clave = i => i.CentroId;
        }

        var filas = new List<FilaAnaliticaDto>();
        var hijos = nodos.ToLookup(n => n.PadreId);
        var directos = imputaciones.Where(i => clave(i) is not null).ToLookup(i => clave(i)!.Value);

        // Recorre el árbol en profundidad: cada fila suma lo suyo y lo de sus descendientes.
        List<(string Cuenta, NaturalezaAnalitica Nat, decimal Importe)> Visitar(Nodo nodo, int nivel)
        {
            var indice = filas.Count;
            filas.Add(null!);
            var movimientos = directos[nodo.Id].Select(i => (i.CuentaCodigo, i.Naturaleza, i.Importe)).ToList();
            foreach (var hijo in hijos[nodo.Id].OrderBy(h => h.Codigo, StringComparer.Ordinal))
            {
                movimientos.AddRange(Visitar(hijo, nivel + 1));
            }

            filas[indice] = Fila(nodo.Id, nodo.Codigo, nodo.Nombre, nodo.PadreId, nivel, movimientos);
            return movimientos;
        }

        foreach (var raiz in hijos[null].OrderBy(n => n.Codigo, StringComparer.Ordinal))
        {
            Visitar(raiz, 0);
        }

        var sinDimension = imputaciones.Where(i => clave(i) is null).Select(i => (i.CuentaCodigo, i.Naturaleza, i.Importe))
            .Concat(sinAsignar.Select(s => (s.CuentaCodigo, s.Naturaleza, s.Importe))).ToList();
        if (sinDimension.Count > 0)
        {
            filas.Add(Fila(null, "—", dimension == DimensionAnalitica.Partida ? "Sin partida (por cuenta)" : "Sin asignar", null, 0, sinDimension));
        }

        // Totales: todo lo de gastos e ingresos del periodo (imputado o no). Los repartos suman cero.
        var totalIngresos = apuntes.Where(a => MotorAnalitico.NaturalezaDe(a.CuentaCodigo) == NaturalezaAnalitica.Ingreso).Sum(a => a.Importe);
        var totalGastos = apuntes.Where(a => MotorAnalitico.NaturalezaDe(a.CuentaCodigo) == NaturalezaAnalitica.Gasto).Sum(a => a.Importe);
        var sinIng = sinAsignar.Where(s => s.Naturaleza == NaturalezaAnalitica.Ingreso).Sum(s => s.Importe);
        var sinGas = sinAsignar.Where(s => s.Naturaleza == NaturalezaAnalitica.Gasto).Sum(s => s.Importe);
        var base_ = Math.Abs(totalIngresos) + Math.Abs(totalGastos);
        var porcentaje = base_ == 0m ? 100m : Math.Round((base_ - Math.Abs(sinIng) - Math.Abs(sinGas)) * 100m / base_, 2);

        return new InformeAnaliticoDto(desde, hasta, dimension, filas, totalIngresos, totalGastos, totalIngresos - totalGastos, sinIng, sinGas, porcentaje);
    }

    private static FilaAnaliticaDto Fila(Guid? id, string codigo, string nombre, Guid? padre, int nivel,
        IReadOnlyList<(string Cuenta, NaturalezaAnalitica Nat, decimal Importe)> movimientos)
    {
        var ingresos = movimientos.Where(m => m.Nat == NaturalezaAnalitica.Ingreso).Sum(m => m.Importe);
        var gastos = movimientos.Where(m => m.Nat == NaturalezaAnalitica.Gasto).Sum(m => m.Importe);
        var cuentas = movimientos.GroupBy(m => m.Cuenta)
            .Select(g => new CuentaAnaliticaDto(g.Key, g.Where(m => m.Nat == NaturalezaAnalitica.Ingreso).Sum(m => m.Importe), g.Where(m => m.Nat == NaturalezaAnalitica.Gasto).Sum(m => m.Importe)))
            .Where(c => c.Ingresos != 0m || c.Gastos != 0m)
            .OrderBy(c => c.CuentaCodigo, StringComparer.Ordinal)
            .ToList();
        var resultado = ingresos - gastos;
        return new FilaAnaliticaDto(id, codigo, nombre, padre, nivel, ingresos, gastos, resultado,
            ingresos == 0m ? null : Math.Round(resultado * 100m / ingresos, 2), cuentas);
    }

    private sealed record Nodo(Guid Id, string Codigo, string Nombre, Guid? PadreId);
}
