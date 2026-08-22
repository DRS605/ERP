using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>Tipo de tercero al que pertenece una subcuenta contable.</summary>
public enum TipoTerceroContable
{
    Cliente = 1,
    Proveedor = 2,
    Trabajador = 3,
}

/// <summary>Subcuenta contable asignada a un tercero.</summary>
public sealed record SubcuentaTerceroDto(string Tipo, Guid TerceroId, string CuentaCodigo, bool Individual);

/// <summary>
/// Lógica compartida de subcuentas de tercero: raíz por tipo (430/400/465) y cálculo del <b>código
/// siguiente</b> con la longitud configurada. En modo Simple los terceros comparten la raíz; en modo
/// Completo cada uno tiene su subcuenta individual (p. ej. 43000001).
/// </summary>
public static class SubcuentasTerceros
{
    public static string Raiz(TipoTerceroContable tipo) => tipo switch
    {
        TipoTerceroContable.Cliente => PlanBasico.CuentaClientes,      // 430
        TipoTerceroContable.Proveedor => PlanBasico.CuentaProveedores, // 400
        TipoTerceroContable.Trabajador => PlanBasico.CuentaTrabajadores, // 465
        _ => PlanBasico.CuentaClientes,
    };

    /// <summary>
    /// Siguiente código libre para una raíz y longitud dadas, a partir de los códigos ya existentes.
    /// Si la longitud no deja sitio para secuencia, devuelve la propia raíz.
    /// </summary>
    public static string SiguienteCodigo(string raiz, int longitud, IEnumerable<string> codigosExistentes)
    {
        if (longitud <= raiz.Length)
        {
            return raiz;
        }

        var secuenciaLongitud = longitud - raiz.Length;
        long maximo = 0;
        foreach (var c in codigosExistentes)
        {
            if (c.Length == longitud && c.StartsWith(raiz, StringComparison.Ordinal)
                && long.TryParse(c.AsSpan(raiz.Length), out var n))
            {
                maximo = Math.Max(maximo, n);
            }
        }

        var siguiente = (maximo + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (siguiente.Length > secuenciaLongitud)
        {
            return raiz + new string('9', secuenciaLongitud); // secuencia agotada (caso extremo)
        }

        return raiz + siguiente.PadLeft(secuenciaLongitud, '0');
    }

    /// <summary>
    /// Devuelve la subcuenta a usar para un tercero, creándola si hace falta (solo en modo Completo).
    /// No guarda: lo hace el llamador. En modo Simple devuelve la raíz común.
    /// </summary>
    public static async Task<string> AsegurarAsync(Guid empresaId, TipoTerceroContable tipo, Guid terceroId,
        string nombre, ModoContabilidad modo, int longitud, IRepositorioCuentas cuentas, CancellationToken ct)
    {
        var raiz = Raiz(tipo);
        if (modo != ModoContabilidad.Completo)
        {
            return raiz;
        }

        var existente = await cuentas.ObtenerPorTerceroAsync(empresaId, terceroId, ct).ConfigureAwait(false);
        if (existente is not null)
        {
            return existente.Codigo;
        }

        var codigos = await cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        var codigo = SiguienteCodigo(raiz, longitud, codigos);
        var cuenta = Cuenta.CrearSubcuenta(empresaId, codigo, NombreSubcuenta(nombre), terceroId, tipo.ToString());
        if (cuenta.EsCorrecto)
        {
            cuentas.Agregar(cuenta.Valor);
        }

        return codigo;
    }

    internal static string NombreSubcuenta(string? nombre) =>
        string.IsNullOrWhiteSpace(nombre) ? "Tercero"
            : (nombre.Length > Cuenta.LongitudMaximaNombre ? nombre[..Cuenta.LongitudMaximaNombre] : nombre.Trim());
}

// --------------------------------------------------------------------------------------------
//  Casos de uso
// --------------------------------------------------------------------------------------------

/// <summary>Sugiere el «código siguiente» de subcuenta para un tipo de tercero (para la ficha).</summary>
public sealed class ObtenerSiguienteSubcuenta
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public ObtenerSiguienteSubcuenta(IRepositorioCuentas cuentas, IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _config = config;
        _unidad = unidad;
    }

    public async Task<SubcuentaTerceroDto> EjecutarAsync(Guid empresaId, TipoTerceroContable tipo, CancellationToken ct = default)
    {
        var creadas = await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        if (creadas > 0)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        }

        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var modo = config?.Modo ?? ModoContabilidad.Simple;
        var longitud = config?.LongitudSubcuenta ?? ConfiguracionContabilidad.LongitudSubcuentaDefecto;
        var raiz = SubcuentasTerceros.Raiz(tipo);
        if (modo != ModoContabilidad.Completo)
        {
            return new SubcuentaTerceroDto(tipo.ToString(), Guid.Empty, raiz, false);
        }

        var codigos = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
        return new SubcuentaTerceroDto(tipo.ToString(), Guid.Empty, SubcuentasTerceros.SiguienteCodigo(raiz, longitud, codigos), true);
    }
}

/// <summary>Lista las subcuentas individuales de un tipo de tercero (para enriquecer los listados).</summary>
public sealed class ListarSubcuentasTercero
{
    private readonly IRepositorioCuentas _cuentas;

    public ListarSubcuentasTercero(IRepositorioCuentas cuentas) => _cuentas = cuentas;

    public async Task<IReadOnlyList<SubcuentaTerceroDto>> EjecutarAsync(Guid empresaId, TipoTerceroContable tipo, CancellationToken ct = default)
    {
        var dict = await _cuentas.SubcuentasPorTipoAsync(empresaId, tipo.ToString(), ct).ConfigureAwait(false);
        return dict.Select(kv => new SubcuentaTerceroDto(tipo.ToString(), kv.Key, kv.Value, true)).ToList();
    }
}

/// <summary>Obtiene la subcuenta asignada a un tercero concreto (o la raíz común en modo Simple).</summary>
public sealed class ObtenerSubcuentaTercero
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IRepositorioConfigContabilidad _config;

    public ObtenerSubcuentaTercero(IRepositorioCuentas cuentas, IRepositorioConfigContabilidad config)
    {
        _cuentas = cuentas;
        _config = config;
    }

    public async Task<SubcuentaTerceroDto> EjecutarAsync(Guid empresaId, TipoTerceroContable tipo, Guid terceroId, CancellationToken ct = default)
    {
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var modo = config?.Modo ?? ModoContabilidad.Simple;
        var raiz = SubcuentasTerceros.Raiz(tipo);
        if (modo != ModoContabilidad.Completo)
        {
            return new SubcuentaTerceroDto(tipo.ToString(), terceroId, raiz, false);
        }

        var cuenta = await _cuentas.ObtenerPorTerceroAsync(empresaId, terceroId, ct).ConfigureAwait(false);
        return new SubcuentaTerceroDto(tipo.ToString(), terceroId, cuenta?.Codigo ?? raiz, cuenta is not null);
    }
}

/// <summary>Datos para asignar/editar la subcuenta de un tercero.</summary>
public sealed record AsignarSubcuentaComando(TipoTerceroContable Tipo, Guid TerceroId, string Nombre, string? CuentaPreferida);

/// <summary>
/// Asigna la subcuenta de un tercero: autonumera la siguiente, o usa la que indique el usuario
/// (permitiendo que no empiece por la raíz). Solo tiene efecto en modo Completo.
/// </summary>
public sealed class AsignarSubcuentaTercero
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public AsignarSubcuentaTercero(IRepositorioCuentas cuentas, IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _config = config;
        _unidad = unidad;
    }

    public async Task<Resultado<SubcuentaTerceroDto>> EjecutarAsync(Guid empresaId, AsignarSubcuentaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);

        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var modo = config?.Modo ?? ModoContabilidad.Simple;
        var longitud = config?.LongitudSubcuenta ?? ConfiguracionContabilidad.LongitudSubcuentaDefecto;
        var raiz = SubcuentasTerceros.Raiz(comando.Tipo);

        // Modo Simple: no hay subcuenta individual; todos comparten la raíz.
        if (modo != ModoContabilidad.Completo)
        {
            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(new SubcuentaTerceroDto(comando.Tipo.ToString(), comando.TerceroId, raiz, false));
        }

        var preferida = comando.CuentaPreferida?.Trim();
        var actual = await _cuentas.ObtenerPorTerceroAsync(empresaId, comando.TerceroId, ct).ConfigureAwait(false);

        // Código objetivo: el que indique el usuario, o el actual, o el siguiente autonumerado.
        string objetivo;
        if (!string.IsNullOrWhiteSpace(preferida))
        {
            if (!preferida.All(char.IsDigit))
            {
                return Resultado.Fallo<SubcuentaTerceroDto>(Error.Validacion("subcuenta.codigo_invalido", "El código de subcuenta debe ser numérico."));
            }

            objetivo = preferida;
        }
        else if (actual is not null)
        {
            objetivo = actual.Codigo;
        }
        else
        {
            var codigos = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
            objetivo = SubcuentasTerceros.SiguienteCodigo(raiz, longitud, codigos);
        }

        // Si el objetivo es la raíz genérica, se interpreta como "sin subcuenta individual".
        if (objetivo == raiz)
        {
            if (actual is not null)
            {
                actual.EnlazarTercero(null, null);
            }

            await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(new SubcuentaTerceroDto(comando.Tipo.ToString(), comando.TerceroId, raiz, false));
        }

        // Suelta la subcuenta anterior del tercero si cambia de código.
        if (actual is not null && actual.Codigo != objetivo)
        {
            actual.EnlazarTercero(null, null);
        }

        var enObjetivo = await _cuentas.ObtenerPorCodigoAsync(empresaId, objetivo, ct).ConfigureAwait(false);
        if (enObjetivo is null)
        {
            var nueva = Cuenta.CrearSubcuenta(empresaId, objetivo, SubcuentasTerceros.NombreSubcuenta(comando.Nombre), comando.TerceroId, comando.Tipo.ToString());
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<SubcuentaTerceroDto>(nueva.Error);
            }

            _cuentas.Agregar(nueva.Valor);
        }
        else
        {
            enObjetivo.EnlazarTercero(comando.TerceroId, comando.Tipo.ToString());
            enObjetivo.Renombrar(SubcuentasTerceros.NombreSubcuenta(comando.Nombre));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new SubcuentaTerceroDto(comando.Tipo.ToString(), comando.TerceroId, objetivo, true));
    }
}
