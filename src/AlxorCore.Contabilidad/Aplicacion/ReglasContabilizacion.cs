using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Vista de una regla de contabilización.</summary>
public sealed record ReglaContabilizacionDto(
    Guid Id, string Sentido, string? Familia, string? TipoTercero, string CuentaCodigo, int Especificidad)
{
    public static ReglaContabilizacionDto Desde(ReglaContabilizacion r) => new(
        r.Id, r.Sentido.ToString(), r.Familia, r.TipoTercero, r.CuentaCodigo, r.Especificidad);
}

/// <summary>Repositorio de reglas de contabilización.</summary>
public interface IRepositorioReglasContabilizacion
{
    void Agregar(ReglaContabilizacion regla);
    Task<ReglaContabilizacion?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ReglaContabilizacion>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    void Eliminar(ReglaContabilizacion regla);
}

/// <summary>
/// Resuelve la cuenta de resultado aplicando las reglas configuradas: entre las que encajan (por
/// familia y/o tipo de tercero) gana la más específica; si ninguna encaja, usa la cuenta genérica.
/// </summary>
public sealed class ResolverCuentasReglas : IResolverCuentas
{
    private readonly IRepositorioReglasContabilizacion _reglas;

    public ResolverCuentasReglas(IRepositorioReglasContabilizacion reglas) => _reglas = reglas;

    public async Task<string> CuentaResultadoAsync(Guid empresaId, SentidoContable sentido, string? familia, string? tipoTercero, CancellationToken ct = default)
    {
        var reglas = await _reglas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var mejor = reglas
            .Where(r => r.Sentido == sentido)
            .Where(r => r.Familia is null || string.Equals(r.Familia, familia, StringComparison.OrdinalIgnoreCase))
            .Where(r => r.TipoTercero is null || string.Equals(r.TipoTercero, tipoTercero, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Especificidad)
            .ThenByDescending(r => r.Familia is not null)
            .FirstOrDefault();

        if (mejor is not null)
        {
            return mejor.CuentaCodigo;
        }

        return sentido == SentidoContable.Venta ? PlanBasico.CuentaVentas : PlanBasico.CuentaCompras;
    }
}

/// <summary>Lista las reglas de contabilización de la empresa (más específicas primero).</summary>
public sealed class ListarReglasContabilizacion
{
    private readonly IRepositorioReglasContabilizacion _reglas;

    public ListarReglasContabilizacion(IRepositorioReglasContabilizacion reglas) => _reglas = reglas;

    public async Task<IReadOnlyList<ReglaContabilizacionDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var reglas = await _reglas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        return reglas
            .OrderBy(r => r.Sentido)
            .ThenByDescending(r => r.Especificidad)
            .Select(ReglaContabilizacionDto.Desde)
            .ToList();
    }
}

/// <summary>Datos de una regla de contabilización (crear/actualizar).</summary>
public sealed record DatosReglaContabilizacion(SentidoContable Sentido, string? Familia, string? TipoTercero, string CuentaCodigo);

/// <summary>Crea o actualiza una regla de contabilización.</summary>
public sealed class GuardarReglaContabilizacion
{
    private readonly IRepositorioReglasContabilizacion _reglas;
    private readonly IRepositorioCuentas _cuentas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public GuardarReglaContabilizacion(IRepositorioReglasContabilizacion reglas, IRepositorioCuentas cuentas, IUnidadDeTrabajoContabilidad unidad)
    {
        _reglas = reglas; _cuentas = cuentas; _unidad = unidad;
    }

    public async Task<Resultado<ReglaContabilizacionDto>> EjecutarAsync(Guid empresaId, Guid? id, DatosReglaContabilizacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        // La cuenta debe existir en el plan (se siembra y persiste si aún no estaba, para poder
        // comprobarla contra la base de datos).
        if (await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false) > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        var codigos = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        var codigo = (datos.CuentaCodigo ?? string.Empty).Trim();
        if (!codigos.Contains(codigo))
        {
            return Resultado.Fallo<ReglaContabilizacionDto>(Error.Validacion("regla.cuenta_inexistente", $"La cuenta «{codigo}» no existe en el plan."));
        }

        ReglaContabilizacion regla;
        if (id is { } reglaId)
        {
            var existente = await _reglas.ObtenerAsync(reglaId, ct).ConfigureAwait(false);
            if (existente is null || existente.EmpresaId != empresaId)
            {
                return Resultado.Fallo<ReglaContabilizacionDto>(Error.NoEncontrado("regla.no_encontrada", "La regla no existe."));
            }

            var r = existente.Actualizar(datos.Sentido, datos.Familia, datos.TipoTercero, datos.CuentaCodigo);
            if (r.EsFallo)
            {
                return Resultado.Fallo<ReglaContabilizacionDto>(r.Error);
            }

            regla = existente;
        }
        else
        {
            var creada = ReglaContabilizacion.Crear(empresaId, datos.Sentido, datos.Familia, datos.TipoTercero, datos.CuentaCodigo);
            if (creada.EsFallo)
            {
                return Resultado.Fallo<ReglaContabilizacionDto>(creada.Error);
            }

            regla = creada.Valor;
            _reglas.Agregar(regla);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaContabilizacionDto.Desde(regla));
    }
}

/// <summary>Elimina una regla de contabilización.</summary>
public sealed class EliminarReglaContabilizacion
{
    private readonly IRepositorioReglasContabilizacion _reglas;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public EliminarReglaContabilizacion(IRepositorioReglasContabilizacion reglas, IUnidadDeTrabajoContabilidad unidad)
    {
        _reglas = reglas; _unidad = unidad;
    }

    public async Task<Resultado> EjecutarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var regla = await _reglas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (regla is null || regla.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("regla.no_encontrada", "La regla no existe."));
        }

        _reglas.Eliminar(regla);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
