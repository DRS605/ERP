using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Vista de una cuenta de tesorería (banco o caja).</summary>
public sealed record CuentaBancariaDto(
    Guid Id, string Tipo, string Nombre, string? Iban, string? Bic, string Subcuenta, bool Activa, bool Predeterminada, decimal SaldoInicial,
    DateOnly? FechaSaldoInicial)
{
    public static CuentaBancariaDto Desde(CuentaBancaria c) =>
        new(c.Id, c.Tipo.ToString(), c.Nombre, c.Iban, c.Bic, c.Subcuenta, c.Activa, c.Predeterminada, c.SaldoInicial, c.FechaSaldoInicial);
}

/// <summary>
/// Datos de una cuenta de tesorería. Sin <paramref name="Subcuenta"/> se autonumera (5720001, 5700001…) y se da de alta
/// en el plan de cuentas.
/// </summary>
public sealed record GuardarCuentaBancariaComando(
    string Nombre, string? Iban = null, string? Bic = null, TipoCuentaTesoreria Tipo = TipoCuentaTesoreria.Banco, string? Subcuenta = null,
    bool Activa = true, bool Predeterminada = false, decimal SaldoInicial = 0m, DateOnly? FechaSaldoInicial = null);

/// <summary>Saldo de una cuenta de tesorería: contable (si hay contabilidad completa) y por movimientos.</summary>
public sealed record SaldoCuentaTesoreriaDto(
    Guid? Id, string Nombre, string Tipo, string Subcuenta, decimal? SaldoContable, decimal SaldoMovimientos, decimal Saldo);

/// <summary>
/// Saldos de tesorería de la empresa. <paramref name="Fuente"/> dice de dónde sale el saldo de cada cuenta:
/// «Contabilidad» (saldo de su subcuenta) o «Movimientos» (saldo inicial + cobros − pagos + apuntes contabilizados).
/// </summary>
public sealed record SaldosTesoreriaDto(IReadOnlyList<SaldoCuentaTesoreriaDto> Cuentas, decimal Total, string Fuente, DateOnly Fecha);

/// <summary>Repositorio de cuentas de tesorería.</summary>
public interface IRepositorioCuentasBancarias
{
    void Agregar(CuentaBancaria cuenta);

    void Eliminar(CuentaBancaria cuenta);

    Task<CuentaBancaria?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<CuentaBancaria>> ListarAsync(CancellationToken ct = default);

    /// <summary>¿La cuenta tiene movimientos, remesas o extractos? (entonces no se borra: se desactiva).</summary>
    Task<bool> EnUsoAsync(Guid id, CancellationToken ct = default);

    /// <summary>Cobros menos pagos de una cuenta (null: los movimientos sin cuenta) entre dos fechas.</summary>
    Task<decimal> NetoMovimientosAsync(Guid? cuentaBancariaId, DateOnly? desde, DateOnly hasta, CancellationToken ct = default);

    /// <summary>Importe con signo de los apuntes del extracto contabilizados con un asiento directo (comisiones, intereses…).</summary>
    Task<decimal> NetoApuntesConAsientoAsync(Guid cuentaBancariaId, DateOnly? desde, DateOnly hasta, CancellationToken ct = default);
}

/// <summary>
/// Elige la cuenta de tesorería de un cobro o un pago: la indicada; si no, en efectivo la caja activa y en lo demás el
/// banco predeterminado. Sin cuentas dadas de alta devuelve null y el asiento va a 570/572 como siempre.
/// </summary>
public sealed class ResolutorCuentaTesoreria
{
    private readonly IRepositorioCuentasBancarias _cuentas;

    public ResolutorCuentaTesoreria(IRepositorioCuentasBancarias cuentas) => _cuentas = cuentas;

    public static bool EsEfectivo(string? metodo) => metodo is not null && metodo.Contains("efectivo", StringComparison.OrdinalIgnoreCase);

    public async Task<Resultado<CuentaBancaria?>> ResolverAsync(Guid? cuentaBancariaId, string? metodo, CancellationToken ct = default)
    {
        if (cuentaBancariaId is { } id)
        {
            var cuenta = await _cuentas.ObtenerAsync(id, ct).ConfigureAwait(false);
            if (cuenta is null)
            {
                return Resultado.Fallo<CuentaBancaria?>(Error.NoEncontrado("banco.no_encontrado", "La cuenta bancaria no existe."));
            }

            return cuenta.Activa
                ? Resultado.Ok<CuentaBancaria?>(cuenta)
                : Resultado.Fallo<CuentaBancaria?>(Error.Conflicto("banco.inactiva", $"La cuenta «{cuenta.Nombre}» está desactivada."));
        }

        var todas = await _cuentas.ListarAsync(ct).ConfigureAwait(false);
        var elegida = EsEfectivo(metodo)
            ? todas.Where(c => c.Activa && c.Tipo == TipoCuentaTesoreria.Caja).OrderBy(c => c.CreadoEn).FirstOrDefault()
            : todas.FirstOrDefault(c => c.Activa && c.Predeterminada);
        return Resultado.Ok(elegida);
    }
}

/// <summary>Alta, modificación, baja y saldos de las cuentas bancarias y cajas de la empresa.</summary>
public sealed class GestionCuentasBancarias
{
    private readonly IRepositorioCuentasBancarias _cuentas;
    private readonly IPlanCuentasTesoreria _plan;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    public GestionCuentasBancarias(IRepositorioCuentasBancarias cuentas, IPlanCuentasTesoreria plan, IUnidadDeTrabajoTesoreria unidad, IReloj reloj)
    {
        _cuentas = cuentas;
        _plan = plan;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<CuentaBancariaDto>> ListarAsync(bool soloActivas, CancellationToken ct = default) =>
        (await _cuentas.ListarAsync(ct).ConfigureAwait(false))
            .Where(c => !soloActivas || c.Activa)
            .OrderByDescending(c => c.Predeterminada).ThenBy(c => c.Tipo).ThenBy(c => c.Nombre, StringComparer.CurrentCulture)
            .Select(CuentaBancariaDto.Desde).ToList();

    public async Task<Resultado<CuentaBancariaDto>> CrearAsync(Guid empresaId, GuardarCuentaBancariaComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var raiz = c.Tipo == TipoCuentaTesoreria.Caja ? "570" : "572";
        var preferida = string.IsNullOrWhiteSpace(c.Subcuenta) ? null : c.Subcuenta.Trim();
        var existentes = await _cuentas.ListarAsync(ct).ConfigureAwait(false);
        if (preferida is not null)
        {
            if (!preferida.All(char.IsAsciiDigit) || !preferida.StartsWith(raiz, StringComparison.Ordinal) || preferida.Length is <= 3 or > 12)
            {
                return Resultado.Fallo<CuentaBancariaDto>(Error.Validacion("banco.subcuenta", $"La subcuenta debe ser numérica, empezar por {raiz} y tener más de 3 dígitos."));
            }

            if (existentes.Any(x => x.Subcuenta == preferida))
            {
                return Resultado.Fallo<CuentaBancariaDto>(Error.Conflicto("banco.subcuenta_repetida", $"La subcuenta {preferida} ya es de otra cuenta."));
            }
        }

        // Valida antes de tocar el plan de cuentas.
        var prueba = CuentaBancaria.Crear(empresaId, c.Tipo, c.Nombre, c.Iban, c.Bic, preferida ?? raiz + "0", c.SaldoInicial, c.FechaSaldoInicial, _reloj);
        if (prueba.EsFallo)
        {
            return Resultado.Fallo<CuentaBancariaDto>(prueba.Error);
        }

        if (prueba.Valor.Iban is { } iban && existentes.Any(x => x.Iban == iban))
        {
            return Resultado.Fallo<CuentaBancariaDto>(Error.Conflicto("banco.iban_repetido", $"Ya hay una cuenta con el IBAN {iban}."));
        }

        var subcuenta = await _plan.AsegurarSubcuentaAsync(empresaId, raiz, preferida, prueba.Valor.Nombre, ct).ConfigureAwait(false);
        if (existentes.Any(x => x.Subcuenta == subcuenta))
        {
            return Resultado.Fallo<CuentaBancariaDto>(Error.Conflicto("banco.subcuenta_repetida", $"La subcuenta {subcuenta} ya es de otra cuenta: indica otra."));
        }

        var cuenta = CuentaBancaria.Crear(empresaId, c.Tipo, c.Nombre, c.Iban, c.Bic, subcuenta, c.SaldoInicial, c.FechaSaldoInicial, _reloj);
        if (cuenta.EsFallo)
        {
            return Resultado.Fallo<CuentaBancariaDto>(cuenta.Error);
        }

        // El primer banco activo es el predeterminado. Se quita antes la marca de la anterior (índice único).
        var hayPredeterminada = existentes.Any(x => x.Predeterminada);
        if ((c.Predeterminada || !hayPredeterminada) && c.Tipo == TipoCuentaTesoreria.Banco && c.Activa)
        {
            if (hayPredeterminada)
            {
                foreach (var otra in existentes.Where(x => x.Predeterminada))
                {
                    otra.CambiarPredeterminada(false);
                }

                await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            }

            cuenta.Valor.CambiarPredeterminada(true);
        }

        if (!c.Activa)
        {
            cuenta.Valor.Actualizar(cuenta.Valor.Nombre, cuenta.Valor.Iban, cuenta.Valor.Bic, false, cuenta.Valor.SaldoInicial, cuenta.Valor.FechaSaldoInicial);
        }

        _cuentas.Agregar(cuenta.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CuentaBancariaDto.Desde(cuenta.Valor));
    }

    /// <summary>Modifica nombre, IBAN, BIC, estado, predeterminada y saldo inicial. La subcuenta no cambia.</summary>
    public async Task<Resultado<CuentaBancariaDto>> ActualizarAsync(Guid id, GuardarCuentaBancariaComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var cuenta = await _cuentas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo<CuentaBancariaDto>(Error.NoEncontrado("banco.no_encontrado", "La cuenta bancaria no existe."));
        }

        if (!string.IsNullOrWhiteSpace(c.Subcuenta) && c.Subcuenta.Trim() != cuenta.Subcuenta)
        {
            return Resultado.Fallo<CuentaBancariaDto>(Error.Conflicto("banco.subcuenta_fija", "La subcuenta contable no se cambia (los asientos ya la usan): crea otra cuenta y desactiva esta."));
        }

        var otras = (await _cuentas.ListarAsync(ct).ConfigureAwait(false)).Where(x => x.Id != id).ToList();
        if (ValidadorIban.Normalizar(c.Iban) is { } iban && otras.Any(x => x.Iban == iban))
        {
            return Resultado.Fallo<CuentaBancariaDto>(Error.Conflicto("banco.iban_repetido", $"Ya hay otra cuenta con el IBAN {iban}."));
        }

        var r = cuenta.Actualizar(c.Nombre, c.Iban, c.Bic, c.Activa, c.SaldoInicial, c.FechaSaldoInicial);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CuentaBancariaDto>(r.Error);
        }

        if (c.Predeterminada && !cuenta.Predeterminada)
        {
            if (cuenta.Tipo != TipoCuentaTesoreria.Banco || !cuenta.Activa)
            {
                return Resultado.Fallo<CuentaBancariaDto>(Error.Validacion("banco.predeterminada", "Solo una cuenta bancaria activa puede ser la predeterminada."));
            }

            // Primero se quita la marca de la anterior (índice único de predeterminada por empresa).
            if (otras.Any(x => x.Predeterminada))
            {
                foreach (var otra in otras.Where(x => x.Predeterminada))
                {
                    otra.CambiarPredeterminada(false);
                }

                await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
            }

            cuenta.CambiarPredeterminada(true);
        }
        else if (!c.Predeterminada && cuenta.Predeterminada)
        {
            cuenta.CambiarPredeterminada(false);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CuentaBancariaDto.Desde(cuenta));
    }

    /// <summary>Borra una cuenta sin uso. Si ya tiene movimientos, remesas o extractos, hay que desactivarla.</summary>
    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var cuenta = await _cuentas.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (cuenta is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("banco.no_encontrado", "La cuenta bancaria no existe."));
        }

        if (await _cuentas.EnUsoAsync(id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("banco.en_uso", $"La cuenta «{cuenta.Nombre}» tiene movimientos, remesas o extractos: desactívala en lugar de borrarla."));
        }

        _cuentas.Eliminar(cuenta);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Saldo de cada cuenta a una fecha: el contable de su subcuenta si la empresa lleva contabilidad completa y, en
    /// todo caso, el calculado con los movimientos (saldo inicial + cobros − pagos + apuntes contabilizados). Incluye
    /// una fila para lo registrado sin cuenta bancaria (570/572 genéricas).
    /// </summary>
    public async Task<SaldosTesoreriaDto> SaldosAsync(Guid empresaId, DateOnly? fecha, CancellationToken ct = default)
    {
        var hasta = fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var cuentas = (await _cuentas.ListarAsync(ct).ConfigureAwait(false)).Where(c => c.Activa).OrderBy(c => c.Tipo).ThenBy(c => c.Nombre, StringComparer.CurrentCulture).ToList();
        var codigos = cuentas.Select(c => c.Subcuenta).Concat(["570", "572"]).ToList();
        var contables = await _plan.SaldosAsync(empresaId, codigos, hasta, ct).ConfigureAwait(false);

        var filas = new List<SaldoCuentaTesoreriaDto>();
        foreach (var c in cuentas)
        {
            var desde = c.FechaSaldoInicial;
            var neto = await _cuentas.NetoMovimientosAsync(c.Id, desde, hasta, ct).ConfigureAwait(false)
                       + await _cuentas.NetoApuntesConAsientoAsync(c.Id, desde, hasta, ct).ConfigureAwait(false);
            var porMovimientos = Redondeo.Dos(c.SaldoInicial + neto);
            decimal? contable = contables?.GetValueOrDefault(c.Subcuenta);
            filas.Add(new SaldoCuentaTesoreriaDto(c.Id, c.Nombre, c.Tipo.ToString(), c.Subcuenta, contable, porMovimientos, contable ?? porMovimientos));
        }

        var sinCuenta = Redondeo.Dos(await _cuentas.NetoMovimientosAsync(null, null, hasta, ct).ConfigureAwait(false));
        decimal? contableGenerico = contables is null ? null : Redondeo.Dos(contables.GetValueOrDefault("570") + contables.GetValueOrDefault("572"));
        if ((contableGenerico ?? sinCuenta) != 0m || cuentas.Count == 0)
        {
            filas.Add(new SaldoCuentaTesoreriaDto(null, "Sin cuenta bancaria asignada", "Generica", "570/572", contableGenerico, sinCuenta, contableGenerico ?? sinCuenta));
        }

        return new SaldosTesoreriaDto(filas, Redondeo.Dos(filas.Sum(f => f.Saldo)), contables is null ? "Movimientos" : "Contabilidad", hasta);
    }
}
