using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Divisas.Dominio;

/// <summary>Una divisa del catálogo (código ISO 4217, nombre y número de decimales habituales).</summary>
public sealed record DivisaInfo(string Codigo, string Nombre, int Decimales);

/// <summary>
/// Catálogo de divisas admitidas. La moneda funcional del ERP es el <b>euro</b> (EUR); el resto son
/// divisas extranjeras que requieren un tipo de cambio para convertirse a euros.
/// </summary>
public static class DivisasConocidas
{
    public const string Euro = "EUR";

    /// <summary>Divisas admitidas (las más habituales para una pyme española que opere fuera de la zona euro).</summary>
    public static readonly IReadOnlyList<DivisaInfo> Todas =
    [
        new("EUR", "Euro", 2),
        new("USD", "Dólar estadounidense", 2),
        new("GBP", "Libra esterlina", 2),
        new("CHF", "Franco suizo", 2),
        new("JPY", "Yen japonés", 0),
        new("CNY", "Yuan chino", 2),
        new("CAD", "Dólar canadiense", 2),
        new("MXN", "Peso mexicano", 2),
        new("BRL", "Real brasileño", 2),
        new("ARS", "Peso argentino", 2),
        new("SEK", "Corona sueca", 2),
        new("NOK", "Corona noruega", 2),
        new("DKK", "Corona danesa", 2),
        new("PLN", "Esloti polaco", 2),
        new("MAD", "Dírham marroquí", 2),
    ];

    public static bool EsConocida(string? codigo) =>
        !string.IsNullOrWhiteSpace(codigo) && Todas.Any(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    public static bool EsEuro(string? codigo) => string.Equals(codigo, Euro, StringComparison.OrdinalIgnoreCase);

    public static string? Normalizar(string? codigo)
    {
        var c = codigo?.Trim().ToUpperInvariant();
        return EsConocida(c) ? c : null;
    }
}

/// <summary>
/// Tipo de cambio de una divisa frente al euro en una fecha. <see cref="TasaEur"/> es cuántos euros
/// vale <b>una unidad</b> de la divisa (p. ej. USD con tasa 0,92 ⇒ 1 USD = 0,92 €). Se guarda por
/// empresa; la tasa vigente a una fecha es la del registro más reciente con fecha ≤ la buscada.
/// </summary>
public sealed class TipoCambio : RaizAgregadoEmpresa<Guid>
{
    private TipoCambio(Guid id)
        : base(id, Guid.Empty)
    {
        Divisa = null!;
    }

    private TipoCambio(Guid id, Guid empresaId, string divisa, DateOnly fecha, decimal tasaEur, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Divisa = divisa;
        Fecha = fecha;
        TasaEur = tasaEur;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Código ISO 4217 de la divisa (nunca EUR: el euro no necesita tipo de cambio).</summary>
    public string Divisa { get; private set; }

    /// <summary>Fecha a la que aplica el tipo de cambio.</summary>
    public DateOnly Fecha { get; private set; }

    /// <summary>Euros por una unidad de la divisa.</summary>
    public decimal TasaEur { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<TipoCambio> Crear(Guid empresaId, string? divisa, DateOnly fecha, decimal tasaEur, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var codigo = DivisasConocidas.Normalizar(divisa);
        if (codigo is null)
        {
            return Resultado.Fallo<TipoCambio>(Error.Validacion("divisa.desconocida", "La divisa no está en el catálogo admitido."));
        }

        if (DivisasConocidas.EsEuro(codigo))
        {
            return Resultado.Fallo<TipoCambio>(Error.Validacion("divisa.euro", "El euro es la moneda funcional; no necesita tipo de cambio."));
        }

        if (tasaEur <= 0m)
        {
            return Resultado.Fallo<TipoCambio>(Error.Validacion("divisa.tasa_invalida", "La tasa debe ser mayor que cero."));
        }

        return Resultado.Ok(new TipoCambio(Guid.NewGuid(), empresaId, codigo, fecha, tasaEur, reloj.AhoraUtc));
    }

    public Resultado Actualizar(decimal tasaEur, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (tasaEur <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("divisa.tasa_invalida", "La tasa debe ser mayor que cero."));
        }

        TasaEur = tasaEur;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }
}

/// <summary>Impacto en resultados de una diferencia de cambio (importe en euros y cuenta contable).</summary>
public sealed record ResultadoDiferenciaCambio(decimal DiferenciaEur, bool EsIngreso, string Cuenta);

/// <summary>
/// Cálculos puros de conversión de divisa y de <b>diferencias de cambio</b>. Todo se redondea a dos
/// decimales (la moneda funcional es el euro).
/// </summary>
public static class CalculadoraDivisa
{
    /// <summary>Cuenta de diferencias positivas de cambio (ingreso financiero).</summary>
    public const string CuentaDiferenciasPositivas = "768";

    /// <summary>Cuenta de diferencias negativas de cambio (gasto financiero).</summary>
    public const string CuentaDiferenciasNegativas = "668";

    /// <summary>Convierte un importe en divisa a euros aplicando la tasa (euros por unidad).</summary>
    public static decimal AEuros(decimal importeDivisa, decimal tasaEur) => Redondeo.Dos(importeDivisa * tasaEur);

    /// <summary>Convierte un importe en euros a la divisa aplicando la tasa (euros por unidad).</summary>
    public static decimal DeEuros(decimal importeEur, decimal tasaEur) => tasaEur > 0m ? Redondeo.Dos(importeEur / tasaEur) : 0m;

    /// <summary>
    /// Diferencia de cambio en euros de una posición en divisa entre dos tasas. Para un <b>activo</b>
    /// (saldo a cobrar) el resultado es <c>valorNuevo − valorOrigen</c>; para un <b>pasivo</b> (saldo a
    /// pagar) es el inverso (si la divisa se aprecia, se debe más y hay pérdida). Un resultado positivo
    /// es un ingreso (cuenta 768) y uno negativo, un gasto (668).
    /// </summary>
    public static ResultadoDiferenciaCambio Diferencia(decimal importeDivisa, decimal tasaOrigen, decimal tasaValoracion, bool esActivo)
    {
        var valorOrigen = AEuros(importeDivisa, tasaOrigen);
        var valorNuevo = AEuros(importeDivisa, tasaValoracion);
        var resultado = Redondeo.Dos(esActivo ? valorNuevo - valorOrigen : valorOrigen - valorNuevo);
        var esIngreso = resultado >= 0m;
        return new ResultadoDiferenciaCambio(resultado, esIngreso, esIngreso ? CuentaDiferenciasPositivas : CuentaDiferenciasNegativas);
    }
}
