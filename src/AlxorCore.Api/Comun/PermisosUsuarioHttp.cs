using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Seguridad;

namespace AlxorCore.Api.Comun;

/// <summary>Permisos del usuario de la petición, leídos de los claims de su token.</summary>
public sealed class PermisosUsuarioHttp : IPermisosUsuario
{
    public const string CabeceraForzarRiesgo = "X-Forzar-Riesgo";

    private readonly IHttpContextAccessor _http;

    public PermisosUsuarioHttp(IHttpContextAccessor http) => _http = http;

    public bool Tiene(string permiso) => _http.HttpContext?.User.HasClaim(ClaimsAlxor.Permiso, permiso) ?? false;

    public bool PideForzarRiesgo =>
        _http.HttpContext?.Request.Headers.TryGetValue(CabeceraForzarRiesgo, out var v) == true && string.Equals(v.ToString(), "true", StringComparison.OrdinalIgnoreCase);
}
