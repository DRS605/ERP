using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>
/// A qué cuenta de existencias (grupo 3) va el stock de una familia de artículos al cierre, y contra qué cuenta de
/// variación (610-612 aprovisionamientos, 71x productos). Sin familia es la cuenta por defecto.
/// </summary>
public sealed class CuentaExistencias : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudFamilia = 100;
    public const string CuentaPorDefecto = "300";

    private CuentaExistencias(Guid id) : base(id, Guid.Empty) { CuentaStock = null!; CuentaVariacion = null!; }

    private CuentaExistencias(Guid id, Guid empresaId) : base(id, empresaId) { CuentaStock = null!; CuentaVariacion = null!; }

    /// <summary>Familia del artículo (su nombre); null para las que no tienen otra asignada.</summary>
    public string? Familia { get; private set; }

    public string CuentaStock { get; private set; }

    public string CuentaVariacion { get; private set; }

    public static Resultado<CuentaExistencias> Crear(Guid empresaId, string? familia, string? cuentaStock, string? cuentaVariacion)
    {
        var stock = cuentaStock?.Trim() ?? string.Empty;
        if (stock.Length is < 3 or > Cuenta.LongitudMaximaCodigo || !stock.All(char.IsAsciiDigit) || stock[0] != '3' || stock.StartsWith("39", StringComparison.Ordinal))
        {
            return Resultado.Fallo<CuentaExistencias>(Error.Validacion("existencias.cuenta", "La cuenta de existencias es del grupo 3 (no la 39, de deterioro)."));
        }

        var variacion = string.IsNullOrWhiteSpace(cuentaVariacion) ? VariacionDe(stock) : cuentaVariacion.Trim();
        if (variacion.Length is < 3 or > Cuenta.LongitudMaximaCodigo || !variacion.All(char.IsAsciiDigit) || !(variacion.StartsWith("61", StringComparison.Ordinal) || variacion.StartsWith("71", StringComparison.Ordinal)))
        {
            return Resultado.Fallo<CuentaExistencias>(Error.Validacion("existencias.variacion", "La cuenta de variación es una 61x (aprovisionamientos) o una 71x (productos)."));
        }

        var f = string.IsNullOrWhiteSpace(familia) ? null : familia.Trim();
        if (f is { Length: > LongitudFamilia })
        {
            return Resultado.Fallo<CuentaExistencias>(Error.Validacion("existencias.familia", "Nombre de familia demasiado largo."));
        }

        return Resultado.Ok(new CuentaExistencias(Guid.NewGuid(), empresaId) { Familia = f, CuentaStock = stock, CuentaVariacion = variacion });
    }

    /// <summary>
    /// Cuenta de variación del PGC para una de existencias: 30 → 610, 31 → 611, 32 → 612, 33 → 710, 34 → 711,
    /// 35 → 712, 36 → 713.
    /// </summary>
    public static string VariacionDe(string cuentaStock) => cuentaStock.Length < 2 ? "610" : cuentaStock[1] switch
    {
        '1' => "611",
        '2' => "612",
        '3' => "710",
        '4' => "711",
        '5' => "712",
        '6' => "713",
        _ => "610",
    };

    public static string NombreDe(string codigo) => codigo[..Math.Min(2, codigo.Length)] switch
    {
        "30" => "Comerciales",
        "31" => "Materias primas",
        "32" => "Otros aprovisionamientos",
        "33" => "Productos en curso",
        "34" => "Productos semiterminados",
        "35" => "Productos terminados",
        "36" => "Subproductos, residuos y materiales recuperados",
        "61" => codigo.StartsWith("611", StringComparison.Ordinal) ? "Variación de existencias de materias primas"
            : codigo.StartsWith("612", StringComparison.Ordinal) ? "Variación de existencias de otros aprovisionamientos" : "Variación de existencias de mercaderías",
        _ => codigo.StartsWith("710", StringComparison.Ordinal) ? "Variación de existencias de productos en curso"
            : codigo.StartsWith("711", StringComparison.Ordinal) ? "Variación de existencias de productos semiterminados"
            : codigo.StartsWith("713", StringComparison.Ordinal) ? "Variación de existencias de subproductos y residuos" : "Variación de existencias de productos terminados",
    };
}
