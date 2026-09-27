using AlxorCore.Nucleo.Comun;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlxorCore.Persistencia;

/// <summary>
/// Mapeo de los conceptos aplicados a una línea: una columna jsonb «conceptos» con la copia de cada concepto (la base
/// de datos la suma con <see cref="GarantiasConceptosSql.Funcion"/> para comprobar los importes de la línea).
/// </summary>
public static class ConceptosEf
{
    public static PropertyBuilder<IReadOnlyList<ConceptoAplicado>> ComoConceptos(this PropertyBuilder<IReadOnlyList<ConceptoAplicado>> propiedad)
    {
        ArgumentNullException.ThrowIfNull(propiedad);
        return propiedad
            .HasConversion(
                v => ConceptosLinea.AJson(v),
                s => ConceptosLinea.DesdeJson(s),
                new ValueComparer<IReadOnlyList<ConceptoAplicado>>(
                    (a, b) => ConceptosLinea.AJson(a) == ConceptosLinea.AJson(b),
                    v => ConceptosLinea.AJson(v).GetHashCode(StringComparison.Ordinal),
                    v => ConceptosLinea.DesdeJson(ConceptosLinea.AJson(v))))
            .HasColumnName("conceptos")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .IsRequired();
    }
}

/// <summary>SQL común de las garantías de los conceptos de línea.</summary>
public static class GarantiasConceptosSql
{
    /// <summary>Suma de los importes de los conceptos con un efecto (Precio o Coste) de una columna jsonb.</summary>
    public const string Funcion = """
        CREATE OR REPLACE FUNCTION public.alxor_suma_conceptos(p_conceptos jsonb, p_efecto text) RETURNS numeric
        LANGUAGE sql IMMUTABLE AS $f$
            SELECT coalesce(sum((e ->> 'importe')::numeric), 0)
              FROM jsonb_array_elements(coalesce(p_conceptos, '[]'::jsonb)) e
             WHERE e ->> 'efecto' = p_efecto
        $f$;
        """;

    /// <summary>Restricciones: la columna es una lista y sus totales cuadran con los conceptos que contiene.</summary>
    public static string Comprobar(string esquema, string tabla) => $"""
        ALTER TABLE {esquema}.{tabla}
            ADD CONSTRAINT ck_{tabla}_conceptos CHECK (jsonb_typeof(conceptos) = 'array'
                AND importe_conceptos = public.alxor_suma_conceptos(conceptos, 'Precio')
                AND coste_conceptos = public.alxor_suma_conceptos(conceptos, 'Coste'));
        """;

    public static string QuitarComprobacion(string esquema, string tabla) =>
        $"ALTER TABLE {esquema}.{tabla} DROP CONSTRAINT IF EXISTS ck_{tabla}_conceptos;";
}
