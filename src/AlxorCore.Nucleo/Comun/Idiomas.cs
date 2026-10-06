namespace AlxorCore.Nucleo.Comun;

/// <summary>
/// Idiomas en que se imprimen los documentos para clientes y proveedores (código ISO 639-1). El castellano es el de
/// siempre: un tercero sin idioma recibe los documentos en castellano.
/// </summary>
public static class IdiomasDocumento
{
    public const string Castellano = "es";

    public static readonly IReadOnlyList<(string Codigo, string Nombre)> Disponibles =
    [
        ("es", "Castellano"), ("en", "Inglés"), ("fr", "Francés"), ("de", "Alemán"), ("it", "Italiano"), ("pt", "Portugués"),
    ];

    /// <summary>Código en minúsculas, o null si viene vacío.</summary>
    public static string? Normalizar(string? idioma) => string.IsNullOrWhiteSpace(idioma) ? null : idioma.Trim().ToLowerInvariant();

    public static bool EsValido(string? codigo) => codigo is null || Disponibles.Any(d => d.Codigo == codigo);

    /// <summary>El idioma en que se imprime (castellano si no se indica o no se reconoce).</summary>
    public static string Efectivo(string? idioma) => Normalizar(idioma) is { } c && EsValido(c) ? c : Castellano;
}
