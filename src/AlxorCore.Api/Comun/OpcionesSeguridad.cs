namespace AlxorCore.Api.Comun;

/// <summary>Ajustes de seguridad (limitación de peticiones en autenticación).</summary>
public sealed class OpcionesSeguridad
{
    public const string Seccion = "Seguridad";

    /// <summary>Nombre de la política de <i>rate limiting</i> aplicada a los endpoints de autenticación.</summary>
    public const string PoliticaAuth = "auth";

    /// <summary>Número máximo de peticiones de autenticación por ventana y por cliente (IP).</summary>
    public int RateLimitPeticiones { get; set; } = 10;

    /// <summary>Duración de la ventana en segundos.</summary>
    public int RateLimitVentanaSegundos { get; set; } = 60;
}
