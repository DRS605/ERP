using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Informes.Dominio;

/// <summary>Clase de producto del impuesto sobre los envases de plástico no reutilizables (Ley 7/2022, modelo 592).</summary>
public enum ClavePlastico
{
    /// <summary>A: envases (bandejas, tarrinas, bolsas, botellas, film de envasado…).</summary>
    Envase = 1,

    /// <summary>B: productos semielaborados (preformas, láminas termoplásticas) para fabricar envases.</summary>
    Semielaborado = 2,

    /// <summary>C: productos de plástico para cierre, comercialización o presentación de envases (tapas, precintos, etiquetas…).</summary>
    Cierre = 3,
}

/// <summary>
/// Plástico de un artículo a efectos del impuesto especial: kilos de plástico por unidad del artículo y de ellos cuántos
/// son reciclados (no tributan). Un artículo exento (medicamentos, sanitarios, envases de importación para uso propio…)
/// se anota con el motivo y no suma cuota.
/// </summary>
public sealed class FichaPlastico : RaizAgregadoEmpresa<Guid>
{
    /// <summary>Tipo del impuesto: euros por kilo de plástico no reciclado.</summary>
    public const decimal TipoPorKg = 0.45m;

    private FichaPlastico(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private FichaPlastico(Guid id, Guid empresaId, Guid productoId)
        : base(id, empresaId)
    {
        ProductoId = productoId;
    }

    public Guid ProductoId { get; private set; }

    public ClavePlastico Clave { get; private set; }

    public decimal KgPorUnidad { get; private set; }

    public decimal KgRecicladoPorUnidad { get; private set; }

    public bool Exento { get; private set; }

    public string? MotivoExencion { get; private set; }

    public decimal KgNoRecicladoPorUnidad => KgPorUnidad - KgRecicladoPorUnidad;

    public static Resultado<FichaPlastico> Crear(Guid empresaId, Guid productoId, ClavePlastico clave, decimal kg, decimal reciclado, bool exento, string? motivo)
    {
        var f = new FichaPlastico(Guid.NewGuid(), empresaId, productoId);
        var r = f.Cambiar(clave, kg, reciclado, exento, motivo);
        return r.EsFallo ? Resultado.Fallo<FichaPlastico>(r.Error) : Resultado.Ok(f);
    }

    public Resultado Cambiar(ClavePlastico clave, decimal kg, decimal reciclado, bool exento, string? motivo)
    {
        if (!Enum.IsDefined(clave))
        {
            return Resultado.Fallo(Error.Validacion("plastico.clave", "La clave es A (envases), B (semielaborados) o C (cierre y presentación)."));
        }

        if (kg is <= 0m or > 1000m)
        {
            return Resultado.Fallo(Error.Validacion("plastico.kg", "Indica los kilos de plástico por unidad (más de 0)."));
        }

        if (reciclado < 0m || reciclado > kg)
        {
            return Resultado.Fallo(Error.Validacion("plastico.reciclado", "El plástico reciclado va de 0 a los kilos de plástico de la unidad."));
        }

        var m = string.IsNullOrWhiteSpace(motivo) ? null : motivo.Trim();
        if (exento && m is null)
        {
            return Resultado.Fallo(Error.Validacion("plastico.motivo", "Indica el motivo de la exención."));
        }

        Clave = clave;
        KgPorUnidad = Math.Round(kg, 6, MidpointRounding.AwayFromZero);
        KgRecicladoPorUnidad = Math.Round(reciclado, 6, MidpointRounding.AwayFromZero);
        Exento = exento;
        MotivoExencion = exento ? (m!.Length > 200 ? m[..200] : m) : null;
        return Resultado.Ok();
    }
}
