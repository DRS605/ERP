using System.Security.Claims;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.CasosDeUso;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Comprueba que el usuario autenticado puede <b>elegir</b> una actividad de negocio concreta en un
/// área (pantalla) al crear un documento. Reutiliza las reglas de visibilidad: si el usuario no tiene
/// reglas en el área, puede elegir cualquiera (abierto por defecto); si las tiene, solo las
/// concedidas. Elegir «sin actividad» (null) siempre está permitido.
/// </summary>
public static class AccesoActividad
{
    /// <summary>Devuelve <c>null</c> si el usuario puede usar la actividad indicada, o un error 403 si no.</summary>
    public static async Task<Error?> ValidarAsync(
        ClaimsPrincipal usuario, IConsultaVisibilidad visibilidad, AreaVisibilidad area, Guid? actividadNegocioId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        ArgumentNullException.ThrowIfNull(visibilidad);

        if (actividadNegocioId is not { } actividad || actividad == Guid.Empty)
        {
            return null;
        }

        var usuarioId = usuario.ObtenerUsuarioId();
        if (usuarioId is null)
        {
            return Error.NoAutenticado("auth.token_invalido", "El token no identifica al usuario.");
        }

        var permitidas = await visibilidad.ActividadesPermitidasAsync(usuarioId.Value, area, ct).ConfigureAwait(false);
        if (permitidas is null || permitidas.Contains(actividad))
        {
            return null;
        }

        return Error.Prohibido("actividad.sin_acceso", "No tienes acceso a la actividad de negocio seleccionada.");
    }
}
