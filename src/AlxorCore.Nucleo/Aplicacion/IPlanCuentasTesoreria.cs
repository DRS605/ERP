namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Puerto entre Tesorería y Contabilidad para las cuentas de tesorería (bancos 572 y caja 570): da de alta la
/// subcuenta de una cuenta bancaria en el plan de cuentas y consulta el saldo contable de esas subcuentas. Lo
/// implementa Contabilidad; Tesorería no conoce su modelo.
/// </summary>
public interface IPlanCuentasTesoreria
{
    /// <summary>
    /// Devuelve la subcuenta para una cuenta de tesorería y la crea en el plan si no existe. Sin
    /// <paramref name="codigoPreferido"/>, autonumera la siguiente libre bajo <paramref name="raiz"/> (572 o 570) con la
    /// longitud de subcuenta de la empresa (p. ej. 5720001). Guarda sus propios cambios.
    /// </summary>
    Task<string> AsegurarSubcuentaAsync(Guid empresaId, string raiz, string? codigoPreferido, string nombre, CancellationToken ct = default);

    /// <summary>
    /// Saldo contable (debe − haber de todos los asientos hasta <paramref name="hasta"/>) de cada código indicado.
    /// Devuelve null si la empresa no lleva contabilidad completa (modo Simple: no hay asientos).
    /// </summary>
    Task<IReadOnlyDictionary<string, decimal>?> SaldosAsync(Guid empresaId, IReadOnlyCollection<string> codigos, DateOnly hasta, CancellationToken ct = default);
}
