using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Contabilidad.Aplicacion;

/// <summary>
/// Implementa <see cref="IPlanCuentasTesoreria"/>: las subcuentas de los bancos (572…) y cajas (570…) se crean en el plan
/// de la empresa igual que las de los terceros (código siguiente con la longitud de subcuenta configurada), aunque la
/// empresa esté en modo Simple: así, si pasa a modo Completo, cada banco ya tiene su cuenta.
/// </summary>
public sealed class PlanCuentasTesoreria : IPlanCuentasTesoreria
{
    private readonly IRepositorioCuentas _cuentas;
    private readonly IRepositorioAsientos _asientos;
    private readonly IRepositorioConfigContabilidad _config;
    private readonly IUnidadDeTrabajoContabilidad _unidad;

    public PlanCuentasTesoreria(IRepositorioCuentas cuentas, IRepositorioAsientos asientos, IRepositorioConfigContabilidad config, IUnidadDeTrabajoContabilidad unidad)
    {
        _cuentas = cuentas;
        _asientos = asientos;
        _config = config;
        _unidad = unidad;
    }

    public async Task<string> AsegurarSubcuentaAsync(Guid empresaId, string raiz, string? codigoPreferido, string nombre, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(raiz);
        await SembradorPlan.AsegurarAsync(empresaId, _cuentas, ct).ConfigureAwait(false);
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var longitud = config?.LongitudSubcuenta ?? ConfiguracionContabilidad.LongitudSubcuentaDefecto;

        var codigo = codigoPreferido?.Trim();
        if (string.IsNullOrEmpty(codigo))
        {
            var existentes = await _cuentas.CodigosExistentesAsync(empresaId, ct).ConfigureAwait(false);
            codigo = SubcuentasTerceros.SiguienteCodigo(raiz, Math.Max(longitud, raiz.Length + 1), existentes);
        }

        if (await _cuentas.ObtenerPorCodigoAsync(empresaId, codigo, ct).ConfigureAwait(false) is null)
        {
            var cuenta = Cuenta.Crear(empresaId, codigo, SubcuentasTerceros.NombreSubcuenta(nombre));
            if (cuenta.EsCorrecto)
            {
                _cuentas.Agregar(cuenta.Valor);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return codigo;
    }

    public async Task<IReadOnlyDictionary<string, decimal>?> SaldosAsync(Guid empresaId, IReadOnlyCollection<string> codigos, DateOnly hasta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(codigos);
        var config = await _config.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if ((config?.Modo ?? await _config.ModoPorDefectoAsync(empresaId, ct).ConfigureAwait(false)) != ModoContabilidad.Completo)
        {
            return null;
        }

        var filas = await _asientos.SaldosHastaAsync(empresaId, codigos, hasta, ct).ConfigureAwait(false);
        var porCodigo = filas.ToDictionary(f => f.CuentaCodigo, f => Redondeo.Dos(f.Debe - f.Haber), StringComparer.Ordinal);
        return codigos.Distinct(StringComparer.Ordinal).ToDictionary(c => c, c => porCodigo.GetValueOrDefault(c), StringComparer.Ordinal);
    }
}
