namespace AlxorCore.Nucleo.Comun;

/// <summary>Incoterms 2020 (Cámara de Comercio Internacional).</summary>
public static class Incoterms
{
    public static readonly IReadOnlyDictionary<string, string> Todos = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["EXW"] = "En fábrica",
        ["FCA"] = "Franco transportista",
        ["CPT"] = "Transporte pagado hasta",
        ["CIP"] = "Transporte y seguro pagados hasta",
        ["DAP"] = "Entregada en un punto",
        ["DPU"] = "Entregada y descargada",
        ["DDP"] = "Entregada con derechos pagados",
        ["FAS"] = "Franco al costado del buque",
        ["FOB"] = "Franco a bordo",
        ["CFR"] = "Coste y flete",
        ["CIF"] = "Coste, seguro y flete",
    };

    /// <summary>Solo marítimos o fluviales (FAS, FOB, CFR, CIF).</summary>
    public static bool EsMaritimo(string codigo) => codigo is "FAS" or "FOB" or "CFR" or "CIF";

    public static string? Normalizar(string? codigo) => string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim().ToUpperInvariant();

    public static bool EsValido(string? codigo) => codigo is null || Todos.ContainsKey(codigo);
}

/// <summary>Países (ISO 3166-1 alfa-2): normaliza el país de una dirección («España», «Francia»…) a su código.</summary>
public static class Paises
{
    private static readonly Dictionary<string, string> PorNombre = new(StringComparer.OrdinalIgnoreCase)
    {
        ["España"] = "ES", ["Espana"] = "ES", ["Spain"] = "ES", ["Francia"] = "FR", ["France"] = "FR", ["Portugal"] = "PT", ["Italia"] = "IT", ["Italy"] = "IT",
        ["Alemania"] = "DE", ["Germany"] = "DE", ["Deutschland"] = "DE", ["Países Bajos"] = "NL", ["Holanda"] = "NL", ["Netherlands"] = "NL", ["Bélgica"] = "BE",
        ["Belgium"] = "BE", ["Luxemburgo"] = "LU", ["Austria"] = "AT", ["Suiza"] = "CH", ["Switzerland"] = "CH", ["Reino Unido"] = "GB", ["United Kingdom"] = "GB",
        ["Irlanda"] = "IE", ["Dinamarca"] = "DK", ["Suecia"] = "SE", ["Noruega"] = "NO", ["Finlandia"] = "FI", ["Polonia"] = "PL", ["Chequia"] = "CZ",
        ["República Checa"] = "CZ", ["Eslovaquia"] = "SK", ["Hungría"] = "HU", ["Rumanía"] = "RO", ["Bulgaria"] = "BG", ["Grecia"] = "GR", ["Croacia"] = "HR",
        ["Eslovenia"] = "SI", ["Estonia"] = "EE", ["Letonia"] = "LV", ["Lituania"] = "LT", ["Malta"] = "MT", ["Chipre"] = "CY", ["Marruecos"] = "MA",
        ["Andorra"] = "AD", ["Estados Unidos"] = "US", ["Canadá"] = "CA", ["México"] = "MX", ["Brasil"] = "BR", ["China"] = "CN", ["Emiratos Árabes Unidos"] = "AE",
    };

    /// <summary>Estados miembros de la Unión Europea (para distinguir entregas intracomunitarias de exportaciones).</summary>
    public static readonly IReadOnlySet<string> UnionEuropea = new HashSet<string>(StringComparer.Ordinal)
    {
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE",
    };

    /// <summary>Código de país: el propio si ya son dos letras; si no, por su nombre. Nulo si no se reconoce.</summary>
    public static string? Codigo(string? pais)
    {
        if (string.IsNullOrWhiteSpace(pais))
        {
            return null;
        }

        var p = pais.Trim();
        return p.Length == 2 && p.All(char.IsAsciiLetter) ? p.ToUpperInvariant() : PorNombre.GetValueOrDefault(p);
    }
}

/// <summary>Código arancelario: la Nomenclatura Combinada (8 dígitos) o el TARIC (10).</summary>
public static class CodigosArancelarios
{
    /// <summary>Solo dígitos (sin puntos ni espacios); nulo si no tiene 8 ni 10.</summary>
    public static string? Normalizar(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return null;
        }

        var d = new string(codigo.Where(c => !char.IsWhiteSpace(c) && c != '.').ToArray());
        return d.Length is 8 or 10 && d.All(char.IsAsciiDigit) ? d : null;
    }
}
