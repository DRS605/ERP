using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Tesoreria.Dominio;

/// <summary>Tipo de cuenta de tesorería: un banco (572…) o una caja de efectivo (570…).</summary>
public enum TipoCuentaTesoreria
{
    Banco = 1,
    Caja = 2,
}

/// <summary>Validación y normalización de IBAN (ISO 13616: longitud por país y dígitos de control módulo 97).</summary>
public static class ValidadorIban
{
    /// <summary>Quita espacios y guiones y pasa a mayúsculas; null si viene vacío.</summary>
    public static string? Normalizar(string? iban) =>
        string.IsNullOrWhiteSpace(iban) ? null : new string(iban.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray()).ToUpperInvariant();

    /// <summary>¿Es un IBAN bien formado con dígitos de control correctos? (España: 24 caracteres).</summary>
    public static bool EsValido(string? iban)
    {
        var n = Normalizar(iban);
        if (n is null || n.Length is < 15 or > 34 || !char.IsAsciiLetter(n[0]) || !char.IsAsciiLetter(n[1]) || !char.IsAsciiDigit(n[2]) || !char.IsAsciiDigit(n[3]))
        {
            return false;
        }

        if (n.StartsWith("ES", StringComparison.Ordinal) && n.Length != 24)
        {
            return false;
        }

        var reordenado = n[4..] + n[..4];
        var resto = 0;
        foreach (var c in reordenado)
        {
            if (char.IsAsciiDigit(c))
            {
                resto = ((resto * 10) + (c - '0')) % 97;
            }
            else if (char.IsAsciiLetterUpper(c))
            {
                var v = c - 'A' + 10;
                resto = ((resto * 100) + v) % 97;
            }
            else
            {
                return false;
            }
        }

        return resto == 1;
    }
}

/// <summary>
/// Cuenta de tesorería de la empresa: una cuenta bancaria (con su IBAN) o una caja. Cada una tiene su subcuenta
/// contable (5720001, 5700001…), a la que van los asientos de los cobros, pagos y remesas que se hacen por ella.
/// Una sola cuenta bancaria es la <b>predeterminada</b>: la que se usa cuando no se elige otra. La subcuenta no cambia
/// una vez creada (los asientos ya la referencian); una cuenta que ya no se usa se desactiva.
/// </summary>
public sealed class CuentaBancaria : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudNombre = 100;

    private CuentaBancaria(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        Subcuenta = null!;
    }

    private CuentaBancaria(Guid id, Guid empresaId, TipoCuentaTesoreria tipo, string nombre, string? iban, string? bic, string subcuenta, decimal saldoInicial,
        DateOnly? fechaSaldoInicial, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Tipo = tipo;
        Nombre = nombre;
        Iban = iban;
        Bic = bic;
        Subcuenta = subcuenta;
        SaldoInicial = saldoInicial;
        FechaSaldoInicial = fechaSaldoInicial;
        Activa = true;
        CreadoEn = ahora;
    }

    public TipoCuentaTesoreria Tipo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>IBAN normalizado (obligatorio en un banco; una caja no tiene).</summary>
    public string? Iban { get; private set; }

    public string? Bic { get; private set; }

    /// <summary>Subcuenta contable (572… o 570…).</summary>
    public string Subcuenta { get; private set; }

    public bool Activa { get; private set; }

    public bool Predeterminada { get; private set; }

    /// <summary>
    /// Saldo de la cuenta antes de empezar a registrar sus movimientos en ALXOR (para el saldo por movimientos cuando
    /// la empresa no lleva contabilidad completa).
    /// </summary>
    public decimal SaldoInicial { get; private set; }

    public DateOnly? FechaSaldoInicial { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<CuentaBancaria> Crear(Guid empresaId, TipoCuentaTesoreria tipo, string? nombre, string? iban, string? bic, string subcuenta,
        decimal saldoInicial, DateOnly? fechaSaldoInicial, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var datos = Validar(tipo, nombre, iban, bic);
        if (datos.EsFallo)
        {
            return Resultado.Fallo<CuentaBancaria>(datos.Error);
        }

        if (string.IsNullOrWhiteSpace(subcuenta) || !subcuenta.All(char.IsAsciiDigit) || subcuenta.Length > 12
            || !subcuenta.StartsWith(tipo == TipoCuentaTesoreria.Caja ? "570" : "572", StringComparison.Ordinal))
        {
            return Resultado.Fallo<CuentaBancaria>(Error.Validacion("banco.subcuenta",
                tipo == TipoCuentaTesoreria.Caja ? "La subcuenta de una caja empieza por 570." : "La subcuenta de un banco empieza por 572."));
        }

        var (n, i, b) = datos.Valor;
        return Resultado.Ok(new CuentaBancaria(Guid.NewGuid(), empresaId, tipo, n, i, b, subcuenta, Math.Round(saldoInicial, 2), fechaSaldoInicial, reloj.AhoraUtc));
    }

    public Resultado Actualizar(string? nombre, string? iban, string? bic, bool activa, decimal saldoInicial, DateOnly? fechaSaldoInicial)
    {
        var datos = Validar(Tipo, nombre, iban, bic);
        if (datos.EsFallo)
        {
            return Resultado.Fallo(datos.Error);
        }

        (Nombre, Iban, Bic) = datos.Valor;
        Activa = activa;
        SaldoInicial = Math.Round(saldoInicial, 2);
        FechaSaldoInicial = fechaSaldoInicial;
        if (!activa)
        {
            Predeterminada = false;
        }

        return Resultado.Ok();
    }

    /// <summary>Marca o desmarca la cuenta como predeterminada (solo un banco activo puede serlo).</summary>
    public Resultado CambiarPredeterminada(bool predeterminada)
    {
        if (predeterminada && (Tipo != TipoCuentaTesoreria.Banco || !Activa))
        {
            return Resultado.Fallo(Error.Validacion("banco.predeterminada", "Solo una cuenta bancaria activa puede ser la predeterminada."));
        }

        Predeterminada = predeterminada;
        return Resultado.Ok();
    }

    private static Resultado<(string Nombre, string? Iban, string? Bic)> Validar(TipoCuentaTesoreria tipo, string? nombre, string? iban, string? bic)
    {
        if (!Enum.IsDefined(tipo))
        {
            return Resultado.Fallo<(string, string?, string?)>(Error.Validacion("banco.tipo", "El tipo debe ser Banco o Caja."));
        }

        var n = nombre?.Trim();
        if (string.IsNullOrEmpty(n) || n.Length > LongitudNombre)
        {
            return Resultado.Fallo<(string, string?, string?)>(Error.Validacion("banco.nombre", $"Indica el nombre de la cuenta (máximo {LongitudNombre} caracteres)."));
        }

        var i = ValidadorIban.Normalizar(iban);
        if (tipo == TipoCuentaTesoreria.Banco && i is null)
        {
            return Resultado.Fallo<(string, string?, string?)>(Error.Validacion("banco.iban_obligatorio", "Indica el IBAN de la cuenta bancaria."));
        }

        if (i is not null && !ValidadorIban.EsValido(i))
        {
            return Resultado.Fallo<(string, string?, string?)>(Error.Validacion("banco.iban_invalido", $"El IBAN {i} no es válido (revisa los dígitos de control)."));
        }

        var b = string.IsNullOrWhiteSpace(bic) ? null : bic.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        if (b is not null && (b.Length is not (8 or 11) || !b.All(char.IsAsciiLetterOrDigit)))
        {
            return Resultado.Fallo<(string, string?, string?)>(Error.Validacion("banco.bic_invalido", "El BIC debe tener 8 u 11 caracteres alfanuméricos."));
        }

        return Resultado.Ok((n, i, b));
    }
}
