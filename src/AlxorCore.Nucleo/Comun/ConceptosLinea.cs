using System.Text.Json;
using System.Text.Json.Serialization;

namespace AlxorCore.Nucleo.Comun;

/// <summary>En qué documentos se puede usar un concepto de línea.</summary>
public enum AmbitoConcepto
{
    Ventas,
    Compras,
    Ambos,
}

/// <summary>
/// Qué hace el concepto: <see cref="Precio"/> cambia el importe de la línea (sale en el documento y en su
/// total); <see cref="Coste"/> no toca el documento, solo suma o resta al coste de la línea (margen de la
/// venta, coste de entrada en almacén de la compra).
/// </summary>
public enum EfectoConcepto
{
    Precio,
    Coste,
}

/// <summary>Si el concepto aumenta (<see cref="Suma"/>) o reduce (<see cref="Resta"/>) el importe o el coste.</summary>
public enum SentidoConcepto
{
    Suma,
    Resta,
}

/// <summary>
/// Cómo se calcula el importe: porcentaje sobre la base de la línea (tras el descuento), un valor por
/// unidad, por kilo neto, o un importe fijo (que, puesto a todo el documento, se reparte entre las líneas).
/// </summary>
public enum CalculoConcepto
{
    Porcentaje,
    PorUnidad,
    PorKilo,
    Importe,
}

/// <summary>
/// Sobre qué base se calcula un porcentaje: la línea (tras el descuento) o, en <see cref="Cascada"/>, la línea más los
/// conceptos que cambian el importe y van antes en el orden (como la «base importe calculado» de Hispatec).
/// </summary>
public enum BasePorcentajeConcepto
{
    Linea,
    Cascada,
}

/// <summary>Cómo se reparte entre las líneas un importe fijo puesto al documento entero.</summary>
public enum RepartoConcepto
{
    PorImporte,
    PorCantidad,
    PorPeso,
}

/// <summary>
/// Concepto aplicado a una línea de un documento: copia de la definición del concepto en ese momento
/// (código, nombre, efecto, sentido y cálculo), el valor usado y el importe resultante, con signo
/// (negativo si resta). <see cref="Repartido"/> indica que viene de un concepto puesto al documento.
/// </summary>
public sealed record ConceptoAplicado(
    Guid ConceptoId,
    string Codigo,
    string Nombre,
    EfectoConcepto Efecto,
    SentidoConcepto Sentido,
    CalculoConcepto Calculo,
    decimal Valor,
    decimal Importe,
    bool Repartido = false,
    bool Cascada = false,
    Guid? AcreedorId = null,
    string? CuentaContable = null);

/// <summary>Cálculo, reparto y serialización de los conceptos de línea (común a ventas y compras).</summary>
public static class ConceptosLinea
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Importe con signo de un concepto sobre una línea.</summary>
    public static decimal Calcular(CalculoConcepto calculo, SentidoConcepto sentido, decimal valor, decimal baseLinea, decimal cantidad, decimal? kilos)
    {
        var importe = calculo switch
        {
            CalculoConcepto.Porcentaje => baseLinea * valor / 100m,
            CalculoConcepto.PorUnidad => cantidad * valor,
            CalculoConcepto.PorKilo => (kilos ?? 0m) * valor,
            _ => valor,
        };
        importe = Redondeo.Dos(importe);
        return sentido == SentidoConcepto.Resta ? -importe : importe;
    }

    /// <summary>Vuelve a calcular un concepto ya aplicado sobre otra línea (al copiar un documento en otro).</summary>
    public static ConceptoAplicado Recalcular(ConceptoAplicado c, decimal baseLinea, decimal cantidad, decimal? kilos)
    {
        ArgumentNullException.ThrowIfNull(c);
        return c with { Importe = Calcular(c.Calculo, c.Sentido, c.Valor, baseLinea, cantidad, kilos) };
    }

    /// <summary>
    /// Vuelve a calcular en orden los conceptos de una línea sobre su nueva base (al copiarla o al cambiar su precio). Los
    /// importes fijos repartidos se conservan; los porcentajes en cascada suman los conceptos de importe anteriores.
    /// </summary>
    public static List<ConceptoAplicado> RecalcularTodos(IEnumerable<ConceptoAplicado>? conceptos, decimal baseLinea, decimal cantidad, decimal? kilos)
    {
        var resultado = new List<ConceptoAplicado>();
        foreach (var c in conceptos ?? [])
        {
            if (c.Calculo == CalculoConcepto.Importe && c.Repartido)
            {
                resultado.Add(c);
                continue;
            }

            var baseConcepto = c.Cascada ? baseLinea + SumaPrecio(resultado) : baseLinea;
            resultado.Add(Recalcular(c, baseConcepto, cantidad, kilos));
        }

        return resultado;
    }

    /// <summary>
    /// Vuelve a calcular solo los porcentajes sobre una nueva base de la línea (al cambiar su precio); los importes por
    /// unidad, por kilo y fijos no dependen del precio y se conservan.
    /// </summary>
    public static List<ConceptoAplicado> RecalcularPorcentajes(IEnumerable<ConceptoAplicado>? conceptos, decimal baseLinea)
    {
        var resultado = new List<ConceptoAplicado>();
        foreach (var c in conceptos ?? [])
        {
            var baseConcepto = c.Cascada ? baseLinea + SumaPrecio(resultado) : baseLinea;
            resultado.Add(c.Calculo == CalculoConcepto.Porcentaje ? c with { Importe = Calcular(c.Calculo, c.Sentido, c.Valor, baseConcepto, 0m, null) } : c);
        }

        return resultado;
    }

    /// <summary>
    /// Reparte un importe entre partes proporcionales a los pesos dados, al céntimo y sin descuadre (los
    /// céntimos sobrantes van a las partes con mayor resto). Si todos los pesos son cero, reparte a partes iguales.
    /// </summary>
    public static IReadOnlyList<decimal> Repartir(decimal importe, IReadOnlyList<decimal> pesos)
    {
        ArgumentNullException.ThrowIfNull(pesos);
        if (pesos.Count == 0)
        {
            return [];
        }

        var efectivos = pesos.Sum(p => Math.Max(0m, p)) > 0m ? pesos.Select(p => Math.Max(0m, p)).ToList() : pesos.Select(_ => 1m).ToList();
        var total = efectivos.Sum();
        var centimos = (long)Math.Round(Redondeo.Dos(importe) * 100m, MidpointRounding.AwayFromZero);
        var exactos = efectivos.Select(p => centimos * p / total).ToList();
        var partes = exactos.Select(e => (long)Math.Floor(e)).ToList();
        var sobran = centimos - partes.Sum();
        foreach (var i in exactos.Select((e, i) => (Resto: e - Math.Floor(e), i)).OrderByDescending(x => x.Resto).ThenBy(x => x.i).Take((int)sobran).Select(x => x.i))
        {
            partes[i]++;
        }

        return partes.Select(p => p / 100m).ToList();
    }

    /// <summary>Suma de los conceptos que cambian el importe de la línea.</summary>
    public static decimal SumaPrecio(IEnumerable<ConceptoAplicado>? conceptos) =>
        Redondeo.Dos((conceptos ?? []).Where(c => c.Efecto == EfectoConcepto.Precio).Sum(c => c.Importe));

    /// <summary>Suma de los conceptos que solo cambian el coste de la línea.</summary>
    public static decimal SumaCoste(IEnumerable<ConceptoAplicado>? conceptos) =>
        Redondeo.Dos((conceptos ?? []).Where(c => c.Efecto == EfectoConcepto.Coste).Sum(c => c.Importe));

    /// <summary>Serializa a JSON (claves en camelCase y enumerados como texto: así los lee la base de datos).</summary>
    public static string AJson(IReadOnlyList<ConceptoAplicado>? conceptos) => JsonSerializer.Serialize(conceptos ?? [], Json);

    public static List<ConceptoAplicado> DesdeJson(string? json) =>
        string.IsNullOrWhiteSpace(json) ? [] : JsonSerializer.Deserialize<List<ConceptoAplicado>>(json, Json) ?? [];
}
