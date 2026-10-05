using System.Globalization;
using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Extensiones.Aplicacion;

public interface IUnidadDeTrabajoExtensiones : IUnidadDeTrabajo
{
    Task BloquearAsync(string clave, CancellationToken ct = default);
}

public interface IRepositorioExtensiones
{
    void Agregar(object entidad);

    void Eliminar(object entidad);

    Task<IReadOnlyList<DefinicionCampo>> CamposAsync(Guid empresaId, string? entidad, CancellationToken ct = default);

    Task<DefinicionCampo?> CampoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ValorCampo>> ValoresAsync(string entidad, Guid entidadId, CancellationToken ct = default);

    Task<IReadOnlyList<ValorCampo>> ValoresDeCampoAsync(Guid definicionId, CancellationToken ct = default);

    Task<bool> CampoConValoresAsync(Guid definicionId, CancellationToken ct = default);

    Task<IReadOnlyList<Adjunto>> AdjuntosAsync(string entidad, Guid entidadId, CancellationToken ct = default);

    /// <summary>El adjunto con su contenido.</summary>
    Task<Adjunto?> AdjuntoAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, int>> CuentaAdjuntosAsync(string entidad, IReadOnlyCollection<Guid> entidadIds, CancellationToken ct = default);

    Task<IReadOnlyList<ReglaAlerta>> ReglasAsync(Guid empresaId, CancellationToken ct = default);

    Task<ReglaAlerta?> ReglaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<Alerta>> AlertasVivasAsync(Guid empresaId, Guid? reglaId, CancellationToken ct = default);

    Task<IReadOnlyList<Alerta>> AlertasAsync(Guid empresaId, bool incluirResueltas, int maximo, CancellationToken ct = default);

    Task<Alerta?> AlertaAsync(Guid id, CancellationToken ct = default);

    Task<bool> AlertaConClaveAsync(Guid reglaId, string clave, CancellationToken ct = default);

    Task<IReadOnlySet<Guid>> LeidasAsync(Guid usuarioId, IReadOnlyCollection<Guid> alertaIds, CancellationToken ct = default);

    Task<bool> ReglaConAlertasAsync(Guid reglaId, CancellationToken ct = default);
}

/// <summary>Lo que ha encontrado una comprobación: la clave lo identifica (para no repetir la alerta mientras siga ahí).</summary>
public sealed record HallazgoAlerta(string Clave, string Titulo, string? Detalle = null, string? Entidad = null, Guid? EntidadId = null);

/// <summary>Comprobaciones que dependen de otros módulos (cobros, riesgo, SII). Las implementa la API.</summary>
public interface IFuentesAlertas
{
    Task<IReadOnlyList<HallazgoAlerta>> FacturasVencidasAsync(Guid empresaId, int dias, CancellationToken ct = default);

    Task<IReadOnlyList<HallazgoAlerta>> RiesgoSuperadoAsync(Guid empresaId, decimal porcentaje, CancellationToken ct = default);

    Task<IReadOnlyList<HallazgoAlerta>> CertificadoCaducidadAsync(Guid empresaId, int dias, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- Contratos
public sealed record EntidadExtensibleDto(string Codigo, string Nombre, string Grupo);

public sealed record DatosCampo(string? Entidad, string? Codigo, TipoCampo Tipo, string? Etiqueta = null, bool Obligatorio = false, string? ValorPorDefecto = null,
    IReadOnlyList<string>? Opciones = null, int Orden = 0, string? Ayuda = null, bool Activo = true);

public sealed record CampoDto(Guid Id, string Entidad, string Codigo, string Etiqueta, string Tipo, bool Obligatorio, string? ValorPorDefecto, IReadOnlyList<string> Opciones,
    int Orden, string? Ayuda, bool Activo)
{
    public static CampoDto De(DefinicionCampo d) => new(d.Id, d.Entidad, d.Codigo, d.Etiqueta, d.Tipo.ToString(), d.Obligatorio, d.ValorPorDefecto,
        d.Opciones.OrderBy(o => o.Orden).Select(o => o.Valor).ToList(), d.Orden, d.Ayuda, d.Activo);
}

/// <summary>Un campo con su valor en un registro (o el valor por defecto, si aún no tiene).</summary>
public sealed record ValorCampoDto(Guid CampoId, string Codigo, string Etiqueta, string Tipo, bool Obligatorio, IReadOnlyList<string> Opciones, string? Ayuda, string? Valor,
    bool PorDefecto, DateTimeOffset? ActualizadoEn);

public sealed record DatosValores(IReadOnlyDictionary<string, string?> Valores);

public sealed record DatosAdjunto(string? Nombre, string? ContenidoBase64, string? Descripcion = null);

public sealed record AdjuntoDto(Guid Id, string Entidad, Guid EntidadId, string Nombre, string TipoMime, long Tamano, string Huella, string? Descripcion, string? SubidoPor,
    DateTimeOffset SubidoEn)
{
    public static AdjuntoDto De(Adjunto a) => new(a.Id, a.Entidad, a.EntidadId, a.Nombre, a.TipoMime, a.Tamano, a.Huella, a.Descripcion, a.SubidoPor, a.SubidoEn);
}

public sealed record DatosRegla(TipoReglaAlerta Tipo, string? Nombre, int? Dias = null, decimal? Porcentaje = null, Guid? CampoId = null, string? Evento = null,
    string? PermisoDestino = null, bool Activa = true);

public sealed record ReglaDto(Guid Id, string Nombre, string Tipo, int? Dias, decimal? Porcentaje, Guid? CampoId, string? Evento, string PermisoDestino, bool Activa,
    DateTimeOffset? UltimaEvaluacion, int Vivas)
{
    public static ReglaDto De(ReglaAlerta r, int vivas) => new(r.Id, r.Nombre, r.Tipo.ToString(), r.Dias, r.Porcentaje, r.CampoId, r.Evento, r.PermisoDestino, r.Activa,
        r.UltimaEvaluacion, vivas);
}

public sealed record AlertaDto(Guid Id, Guid ReglaId, string Regla, string Titulo, string? Detalle, string? Entidad, Guid? EntidadId, DateTimeOffset CreadaEn,
    DateTimeOffset? ResueltaEn, string? ResueltaPor, bool Leida);

public sealed record ResumenAlertasDto(int Vivas, int SinLeer, IReadOnlyList<AlertaDto> Alertas);

public sealed record EvaluacionDto(int Nuevas, int Resueltas);

// ----------------------------------------------------------------------------- Campos personalizados
/// <summary>Campos personalizados (datos complementarios) de cualquier registro extensible y sus valores.</summary>
public sealed class CamposPersonalizados
{
    private readonly IRepositorioExtensiones _repo;
    private readonly IUnidadDeTrabajoExtensiones _unidad;
    private readonly IReloj _reloj;

    public CamposPersonalizados(IRepositorioExtensiones repo, IUnidadDeTrabajoExtensiones unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public static IReadOnlyList<EntidadExtensibleDto> Entidades() =>
        EntidadesExtensibles.Todas.Select(e => new EntidadExtensibleDto(e.Codigo, e.Nombre, e.Grupo)).ToList();

    public async Task<IReadOnlyList<CampoDto>> CamposAsync(Guid empresaId, string? entidad, CancellationToken ct = default) =>
        (await _repo.CamposAsync(empresaId, entidad, ct).ConfigureAwait(false)).OrderBy(c => c.Entidad, StringComparer.Ordinal).ThenBy(c => c.Orden)
            .ThenBy(c => c.Etiqueta, StringComparer.Ordinal).Select(CampoDto.De).ToList();

    public async Task<Resultado<CampoDto>> CrearAsync(Guid empresaId, DatosCampo datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var d = DefinicionCampo.Crear(empresaId, datos.Entidad, datos.Codigo, datos.Tipo, datos.Etiqueta, datos.Obligatorio, datos.ValorPorDefecto, datos.Opciones,
            datos.Orden, datos.Ayuda);
        if (d.EsFallo)
        {
            return Resultado.Fallo<CampoDto>(d.Error);
        }

        if ((await _repo.CamposAsync(empresaId, d.Valor.Entidad, ct).ConfigureAwait(false)).Any(c => c.Codigo == d.Valor.Codigo))
        {
            return Resultado.Fallo<CampoDto>(Error.Conflicto("campo.duplicado", $"Ya hay un campo «{d.Valor.Codigo}» en ese registro."));
        }

        _repo.Agregar(d.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CampoDto.De(d.Valor));
    }

    public async Task<Resultado<CampoDto>> CambiarAsync(Guid id, DatosCampo datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var d = await _repo.CampoAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo<CampoDto>(Error.NoEncontrado("campo.no_encontrado", "El campo no existe."));
        }

        var r = d.Cambiar(datos.Etiqueta, datos.Obligatorio, datos.ValorPorDefecto, datos.Opciones, datos.Orden, datos.Ayuda, datos.Activo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CampoDto>(r.Error);
        }

        // Una lista que pierde opciones no puede dejar valores que ya no están en ella.
        if (d.Tipo == TipoCampo.Lista)
        {
            var huerfanos = (await _repo.ValoresDeCampoAsync(id, ct).ConfigureAwait(false)).Select(v => v.Valor)
                .Where(v => d.Normalizar(v).EsFallo).Distinct(StringComparer.Ordinal).ToList();
            if (huerfanos.Count > 0)
            {
                return Resultado.Fallo<CampoDto>(Error.Conflicto("campo.opciones_en_uso", $"Hay registros con {string.Join(", ", huerfanos)}: no se pueden quitar esas opciones."));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CampoDto.De(d));
    }

    /// <summary>Borra un campo sin valores; con valores, se desactiva (deja de pedirse y se conservan).</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _repo.CampoAsync(id, ct).ConfigureAwait(false);
        if (d is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("campo.no_encontrado", "El campo no existe."));
        }

        if (await _repo.CampoConValoresAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("campo.con_valores", "El campo ya tiene valores: desactívalo en lugar de borrarlo."));
        }

        _repo.Eliminar(d);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Los campos activos del tipo de registro con su valor en ese registro (o el de por defecto).</summary>
    public async Task<Resultado<IReadOnlyList<ValorCampoDto>>> ValoresAsync(Guid empresaId, string entidad, Guid entidadId, CancellationToken ct = default)
    {
        var e = EntidadesExtensibles.Buscar(entidad);
        if (e is null)
        {
            return Resultado.Fallo<IReadOnlyList<ValorCampoDto>>(Error.Validacion("campo.entidad", "Ese tipo de registro no admite campos personalizados."));
        }

        var campos = (await _repo.CamposAsync(empresaId, e.Codigo, ct).ConfigureAwait(false)).Where(c => c.Activo).OrderBy(c => c.Orden).ThenBy(c => c.Etiqueta, StringComparer.Ordinal);
        var valores = (await _repo.ValoresAsync(e.Codigo, entidadId, ct).ConfigureAwait(false)).ToDictionary(v => v.DefinicionId);
        return Resultado.Ok<IReadOnlyList<ValorCampoDto>>(campos.Select(c =>
        {
            var v = valores.GetValueOrDefault(c.Id);
            return new ValorCampoDto(c.Id, c.Codigo, c.Etiqueta, c.Tipo.ToString(), c.Obligatorio, c.Opciones.OrderBy(o => o.Orden).Select(o => o.Valor).ToList(), c.Ayuda,
                v?.Valor ?? c.ValorPorDefecto, v is null && c.ValorPorDefecto is not null, v?.ActualizadoEn);
        }).ToList());
    }

    /// <summary>
    /// Guarda los valores (por código de campo) de un registro. Un valor vacío lo quita, salvo en un campo obligatorio. Los
    /// obligatorios que no se mandan tienen que tener ya valor (o uno por defecto, que se guarda). Todo o nada.
    /// </summary>
    public async Task<Resultado<IReadOnlyList<ValorCampoDto>>> GuardarAsync(Guid empresaId, string entidad, Guid entidadId, DatosValores datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var e = EntidadesExtensibles.Buscar(entidad);
        if (e is null)
        {
            return Resultado.Fallo<IReadOnlyList<ValorCampoDto>>(Error.Validacion("campo.entidad", "Ese tipo de registro no admite campos personalizados."));
        }

        if (entidadId == Guid.Empty)
        {
            return Resultado.Fallo<IReadOnlyList<ValorCampoDto>>(Error.Validacion("campo.registro", "Indica el registro."));
        }

        var campos = (await _repo.CamposAsync(empresaId, e.Codigo, ct).ConfigureAwait(false)).Where(c => c.Activo).ToDictionary(c => c.Codigo, StringComparer.OrdinalIgnoreCase);
        var valores = (await _repo.ValoresAsync(e.Codigo, entidadId, ct).ConfigureAwait(false)).ToDictionary(v => v.DefinicionId);
        var errores = new List<string>();
        var entrada = datos.Valores ?? new Dictionary<string, string?>();
        foreach (var codigo in entrada.Keys.Where(k => !campos.ContainsKey(k)))
        {
            errores.Add($"El campo «{codigo}» no existe en {e.Nombre.ToLowerInvariant()}.");
        }

        foreach (var c in campos.Values)
        {
            var mandado = entrada.TryGetValue(c.Codigo, out var texto);
            var existente = valores.GetValueOrDefault(c.Id);
            if (!mandado)
            {
                if (c.Obligatorio && existente is null)
                {
                    if (c.ValorPorDefecto is { } defecto)
                    {
                        _repo.Agregar(ValorCampo.Nuevo(c, entidadId, defecto, _reloj));
                    }
                    else
                    {
                        errores.Add($"«{c.Etiqueta}» es obligatorio.");
                    }
                }

                continue;
            }

            if (string.IsNullOrWhiteSpace(texto))
            {
                if (c.Obligatorio)
                {
                    errores.Add($"«{c.Etiqueta}» es obligatorio.");
                }
                else if (existente is not null)
                {
                    _repo.Eliminar(existente);
                }

                continue;
            }

            var normal = c.Normalizar(texto);
            if (normal.EsFallo)
            {
                errores.Add(normal.Error.Mensaje);
                continue;
            }

            if (existente is null)
            {
                _repo.Agregar(ValorCampo.Nuevo(c, entidadId, normal.Valor, _reloj));
            }
            else if (existente.Valor != normal.Valor)
            {
                existente.Cambiar(normal.Valor, _reloj);
            }
        }

        if (errores.Count > 0)
        {
            return Resultado.Fallo<IReadOnlyList<ValorCampoDto>>(Error.Validacion("campo.valores", string.Join(" ", errores)));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return await ValoresAsync(empresaId, e.Codigo, entidadId, ct).ConfigureAwait(false);
    }

    /// <summary>Registros con un valor en un campo (texto que contiene, el resto igual; número o fecha también con desde y hasta).</summary>
    public async Task<Resultado<IReadOnlyList<Guid>>> BuscarAsync(Guid empresaId, string entidad, string codigo, string? valor, string? desde, string? hasta,
        CancellationToken ct = default)
    {
        var campo = (await _repo.CamposAsync(empresaId, entidad, ct).ConfigureAwait(false)).FirstOrDefault(c => string.Equals(c.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        if (campo is null)
        {
            return Resultado.Fallo<IReadOnlyList<Guid>>(Error.NoEncontrado("campo.no_encontrado", "El campo no existe en ese registro."));
        }

        var valores = await _repo.ValoresDeCampoAsync(campo.Id, ct).ConfigureAwait(false);
        string? N(string? t) => string.IsNullOrWhiteSpace(t) ? null : campo.Normalizar(t) is { EsCorrecto: true } r ? r.Valor : t.Trim();
        var (igual, min, max) = (N(valor), N(desde), N(hasta));
        bool Cumple(string v) => campo.Tipo switch
        {
            TipoCampo.Texto => igual is null || v.Contains(igual, StringComparison.OrdinalIgnoreCase),
            TipoCampo.Numero => Comparar(v, igual, min, max, s => decimal.Parse(s, CultureInfo.InvariantCulture)),
            TipoCampo.Fecha => Comparar(v, igual, min, max, s => DateOnly.ParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture)),
            _ => igual is null || string.Equals(v, igual, StringComparison.OrdinalIgnoreCase),
        };
        return Resultado.Ok<IReadOnlyList<Guid>>(valores.Where(v => Cumple(v.Valor)).Select(v => v.EntidadId).Distinct().ToList());
    }

    private static bool Comparar<T>(string v, string? igual, string? min, string? max, Func<string, T> leer)
        where T : IComparable<T>
    {
        try
        {
            var x = leer(v);
            return (igual is null || x.CompareTo(leer(igual)) == 0) && (min is null || x.CompareTo(leer(min)) >= 0) && (max is null || x.CompareTo(leer(max)) <= 0);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

// ----------------------------------------------------------------------------- Adjuntos
public sealed class AdjuntosRegistros
{
    private readonly IRepositorioExtensiones _repo;
    private readonly IUnidadDeTrabajoExtensiones _unidad;
    private readonly IReloj _reloj;

    public AdjuntosRegistros(IRepositorioExtensiones repo, IUnidadDeTrabajoExtensiones unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<AdjuntoDto>> ListarAsync(string entidad, Guid entidadId, CancellationToken ct = default) =>
        (await _repo.AdjuntosAsync(EntidadesExtensibles.Buscar(entidad)?.Codigo ?? entidad, entidadId, ct).ConfigureAwait(false))
            .OrderByDescending(a => a.SubidoEn).Select(AdjuntoDto.De).ToList();

    public Task<IReadOnlyDictionary<Guid, int>> ContarAsync(string entidad, IReadOnlyCollection<Guid> ids, CancellationToken ct = default) =>
        _repo.CuentaAdjuntosAsync(EntidadesExtensibles.Buscar(entidad)?.Codigo ?? entidad, ids, ct);

    public async Task<Resultado<AdjuntoDto>> SubirAsync(Guid empresaId, string entidad, Guid entidadId, DatosAdjunto datos, Guid? usuarioId, string? usuario,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(datos.ContenidoBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            return Resultado.Fallo<AdjuntoDto>(Error.Validacion("adjunto.contenido", "El contenido no es base64 válido."));
        }

        var a = Adjunto.Crear(empresaId, entidad, entidadId, datos.Nombre, bytes, datos.Descripcion, usuarioId, usuario, _reloj);
        if (a.EsFallo)
        {
            return Resultado.Fallo<AdjuntoDto>(a.Error);
        }

        _repo.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AdjuntoDto.De(a.Valor));
    }

    public async Task<Adjunto?> ObtenerAsync(Guid id, CancellationToken ct = default) => await _repo.AdjuntoAsync(id, ct).ConfigureAwait(false);

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.AdjuntoAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("adjunto.no_encontrado", "El adjunto no existe."));
        }

        _repo.Eliminar(a);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

// ----------------------------------------------------------------------------- Alertas
/// <summary>
/// Reglas de alerta y sus alertas. Las reglas de comprobación se evalúan al consultar las alertas (si hace más de
/// <see cref="Frescura"/> de la última vez) o al pedirlo: crean las alertas nuevas y resuelven solas las que ya no se
/// encuentran. Las de evento crean una alerta cuando ocurre. Cada usuario ve las alertas de los permisos que tiene.
/// </summary>
public sealed class AlertasEmpresa
{
    public static readonly TimeSpan Frescura = TimeSpan.FromMinutes(15);

    private readonly IRepositorioExtensiones _repo;
    private readonly IUnidadDeTrabajoExtensiones _unidad;
    private readonly IReloj _reloj;
    private readonly IFuentesAlertas? _fuentes;

    public AlertasEmpresa(IRepositorioExtensiones repo, IUnidadDeTrabajoExtensiones unidad, IReloj reloj, IFuentesAlertas? fuentes = null)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
        _fuentes = fuentes;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<ReglaDto>> ReglasAsync(Guid empresaId, CancellationToken ct = default)
    {
        var vivas = (await _repo.AlertasVivasAsync(empresaId, null, ct).ConfigureAwait(false)).GroupBy(a => a.ReglaId).ToDictionary(g => g.Key, g => g.Count());
        return (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(r => r.Nombre, StringComparer.Ordinal)
            .Select(r => ReglaDto.De(r, vivas.GetValueOrDefault(r.Id))).ToList();
    }

    public async Task<Resultado<ReglaDto>> CrearReglaAsync(Guid empresaId, DatosRegla datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var campo = await CampoFechaAsync(empresaId, datos, ct).ConfigureAwait(false);
        if (campo.EsFallo)
        {
            return Resultado.Fallo<ReglaDto>(campo.Error);
        }

        var r = ReglaAlerta.Crear(empresaId, datos.Tipo, datos.Nombre, datos.Dias, datos.Porcentaje, datos.CampoId, datos.Evento, datos.PermisoDestino);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ReglaDto>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaDto.De(r.Valor, 0));
    }

    public async Task<Resultado<ReglaDto>> CambiarReglaAsync(Guid empresaId, Guid id, DatosRegla datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var r = await _repo.ReglaAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<ReglaDto>(Error.NoEncontrado("alerta.regla_no_encontrada", "La regla no existe."));
        }

        var campo = await CampoFechaAsync(empresaId, datos with { Tipo = r.Tipo }, ct).ConfigureAwait(false);
        if (campo.EsFallo)
        {
            return Resultado.Fallo<ReglaDto>(campo.Error);
        }

        var c = r.Cambiar(datos.Nombre, datos.Dias, datos.Porcentaje, datos.CampoId, datos.Evento, datos.PermisoDestino, datos.Activa);
        if (c.EsFallo)
        {
            return Resultado.Fallo<ReglaDto>(c.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaDto.De(r, (await _repo.AlertasVivasAsync(empresaId, id, ct).ConfigureAwait(false)).Count));
    }

    /// <summary>Borra una regla que no ha dado alertas; si las ha dado, se desactiva (para conservar el histórico).</summary>
    public async Task<Resultado> EliminarReglaAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ReglaAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("alerta.regla_no_encontrada", "La regla no existe."));
        }

        if (await _repo.ReglaConAlertasAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("alerta.regla_con_alertas", "La regla ya ha dado alertas: desactívala en lugar de borrarla."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Evalúa las reglas de comprobación activas: crea las alertas nuevas y resuelve las que ya no se encuentran.</summary>
    public async Task<EvaluacionDto> EvaluarAsync(Guid empresaId, CancellationToken ct = default)
    {
        await _unidad.BloquearAsync($"extensiones:alertas:{empresaId}", ct).ConfigureAwait(false);
        var nuevas = 0;
        var resueltas = 0;
        foreach (var regla in (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).Where(r => r.Activa && r.Tipo != TipoReglaAlerta.Evento))
        {
            var hallazgos = await HallazgosAsync(empresaId, regla, ct).ConfigureAwait(false);
            if (hallazgos is null)
            {
                continue;
            }

            var vivas = (await _repo.AlertasVivasAsync(empresaId, regla.Id, ct).ConfigureAwait(false)).ToDictionary(a => a.Clave, StringComparer.Ordinal);
            foreach (var h in hallazgos.GroupBy(h => h.Clave, StringComparer.Ordinal).Select(g => g.First()))
            {
                if (vivas.Remove(h.Clave))
                {
                    continue;
                }

                _repo.Agregar(Alerta.Nueva(regla, h.Clave, h.Titulo, h.Detalle, h.Entidad, h.EntidadId, _reloj));
                nuevas++;
            }

            // Lo que ya no se encuentra (la factura se cobró, el cliente bajó de su límite…) se resuelve solo.
            foreach (var a in vivas.Values)
            {
                a.Resolver(null, _reloj);
                resueltas++;
            }

            regla.Evaluada(_reloj.AhoraUtc);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return new EvaluacionDto(nuevas, resueltas);
    }

    /// <summary>Ha ocurrido un evento: una alerta por cada regla activa de ese evento.</summary>
    public async Task RegistrarEventoAsync(Guid empresaId, string evento, string titulo, string? detalle, string? entidad, Guid? entidadId, Guid? eventoId,
        CancellationToken ct = default)
    {
        var reglas = (await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false)).Where(r => r.Activa && r.Tipo == TipoReglaAlerta.Evento && r.Evento == evento).ToList();
        if (reglas.Count == 0)
        {
            return;
        }

        foreach (var r in reglas)
        {
            var clave = $"{evento}:{eventoId ?? Guid.NewGuid()}";
            if (!await _repo.AlertaConClaveAsync(r.Id, clave, ct).ConfigureAwait(false))
            {
                _repo.Agregar(Alerta.Nueva(r, clave, titulo, detalle, entidad, entidadId, _reloj));
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
    }

    /// <summary>Alertas que ve el usuario (las de sus permisos), con si las ha leído. Antes, si toca, evalúa las reglas.</summary>
    public async Task<ResumenAlertasDto> ParaUsuarioAsync(Guid empresaId, Guid usuarioId, IReadOnlySet<string> permisos, bool incluirResueltas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(permisos);
        var reglas = await _repo.ReglasAsync(empresaId, ct).ConfigureAwait(false);
        if (reglas.Any(r => r.Activa && r.Tipo != TipoReglaAlerta.Evento && (r.UltimaEvaluacion is null || _reloj.AhoraUtc - r.UltimaEvaluacion > Frescura)))
        {
            await EvaluarAsync(empresaId, ct).ConfigureAwait(false);
        }

        var nombres = reglas.ToDictionary(r => r.Id, r => r.Nombre);
        var alertas = (await _repo.AlertasAsync(empresaId, incluirResueltas, 500, ct).ConfigureAwait(false)).Where(a => permisos.Contains(a.PermisoDestino)).ToList();
        var leidas = await _repo.LeidasAsync(usuarioId, alertas.Select(a => a.Id).ToList(), ct).ConfigureAwait(false);
        var dtos = alertas.OrderBy(a => a.ResueltaEn is not null).ThenByDescending(a => a.CreadaEn)
            .Select(a => new AlertaDto(a.Id, a.ReglaId, nombres.GetValueOrDefault(a.ReglaId) ?? "?", a.Titulo, a.Detalle, a.Entidad, a.EntidadId, a.CreadaEn, a.ResueltaEn,
                a.ResueltaPor, leidas.Contains(a.Id))).ToList();
        return new ResumenAlertasDto(dtos.Count(a => a.ResueltaEn is null), dtos.Count(a => a.ResueltaEn is null && !a.Leida), dtos);
    }

    public async Task<Resultado> MarcarLeidaAsync(Guid id, Guid usuarioId, IReadOnlySet<string> permisos, CancellationToken ct = default)
    {
        var a = await VisibleAsync(id, permisos, ct).ConfigureAwait(false);
        if (a.EsFallo)
        {
            return Resultado.Fallo(a.Error);
        }

        if (!(await _repo.LeidasAsync(usuarioId, [id], ct).ConfigureAwait(false)).Contains(id))
        {
            _repo.Agregar(LecturaAlerta.Nueva(a.Valor, usuarioId, _reloj));
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        return Resultado.Ok();
    }

    public async Task<Resultado> ResolverAsync(Guid id, string? quien, IReadOnlySet<string> permisos, CancellationToken ct = default)
    {
        var a = await VisibleAsync(id, permisos, ct).ConfigureAwait(false);
        if (a.EsFallo)
        {
            return Resultado.Fallo(a.Error);
        }

        var r = a.Valor.Resolver(quien, _reloj);
        if (r.EsFallo)
        {
            return r;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<Resultado<Alerta>> VisibleAsync(Guid id, IReadOnlySet<string> permisos, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(permisos);
        var a = await _repo.AlertaAsync(id, ct).ConfigureAwait(false);
        return a is null || !permisos.Contains(a.PermisoDestino)
            ? Resultado.Fallo<Alerta>(Error.NoEncontrado("alerta.no_encontrada", "La alerta no existe."))
            : Resultado.Ok(a);
    }

    private async Task<Resultado> CampoFechaAsync(Guid empresaId, DatosRegla datos, CancellationToken ct)
    {
        if (datos.Tipo != TipoReglaAlerta.FechaCampo || datos.CampoId is not { } id)
        {
            return Resultado.Ok();
        }

        var campo = await _repo.CampoAsync(id, ct).ConfigureAwait(false);
        return campo is null || campo.EmpresaId != empresaId || campo.Tipo != TipoCampo.Fecha
            ? Resultado.Fallo(Error.Validacion("alerta.campo", "El campo vigilado tiene que ser un campo personalizado de tipo fecha."))
            : Resultado.Ok();
    }

    /// <summary>Lo que encuentra ahora la regla (null si no se puede comprobar: falta la fuente).</summary>
    private async Task<IReadOnlyList<HallazgoAlerta>?> HallazgosAsync(Guid empresaId, ReglaAlerta regla, CancellationToken ct)
    {
        switch (regla.Tipo)
        {
            case TipoReglaAlerta.FechaCampo:
                var campo = regla.CampoId is { } id ? await _repo.CampoAsync(id, ct).ConfigureAwait(false) : null;
                if (campo is null)
                {
                    return [];
                }

                var limite = Hoy.AddDays(regla.Dias ?? 30);
                var nombre = EntidadesExtensibles.Buscar(campo.Entidad)?.Nombre ?? campo.Entidad;
                return (await _repo.ValoresDeCampoAsync(campo.Id, ct).ConfigureAwait(false))
                    .Select(v => (v, Fecha: DateOnly.TryParseExact(v.Valor, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var f) ? f : (DateOnly?)null))
                    .Where(x => x.Fecha is { } f && f <= limite)
                    .Select(x => new HallazgoAlerta($"{campo.Id}:{x.v.EntidadId}:{x.v.Valor}",
                        x.Fecha < Hoy ? $"{campo.Etiqueta}: vencido el {x.Fecha:dd/MM/yyyy}" : $"{campo.Etiqueta}: vence el {x.Fecha:dd/MM/yyyy}",
                        $"{nombre} · {(x.Fecha!.Value.DayNumber - Hoy.DayNumber) switch { < 0 => $"hace {Hoy.DayNumber - x.Fecha.Value.DayNumber} días", 0 => "hoy", var d => $"dentro de {d} días" }}",
                        campo.Entidad, x.v.EntidadId))
                    .ToList();
            case TipoReglaAlerta.FacturasVencidas:
                return _fuentes is null ? null : await _fuentes.FacturasVencidasAsync(empresaId, regla.Dias ?? 30, ct).ConfigureAwait(false);
            case TipoReglaAlerta.RiesgoSuperado:
                return _fuentes is null ? null : await _fuentes.RiesgoSuperadoAsync(empresaId, regla.Porcentaje ?? 100m, ct).ConfigureAwait(false);
            case TipoReglaAlerta.CertificadoCaducidad:
                return _fuentes is null ? null : await _fuentes.CertificadoCaducidadAsync(empresaId, regla.Dias ?? 30, ct).ConfigureAwait(false);
            default:
                return null;
        }
    }
}
