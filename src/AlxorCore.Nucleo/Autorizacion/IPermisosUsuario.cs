namespace AlxorCore.Nucleo.Autorizacion;

/// <summary>Permisos del usuario de la petición en curso (para decisiones dentro de un caso de uso, no solo en la ruta).</summary>
public interface IPermisosUsuario
{
    bool Tiene(string permiso);

    /// <summary>Si la petición pide pasar por encima del límite de riesgo (cabecera <c>X-Forzar-Riesgo</c>).</summary>
    bool PideForzarRiesgo => false;

    /// <summary>Forzar el riesgo: lo pide la petición y el usuario tiene <see cref="Permisos.RiesgoForzar"/>.</summary>
    bool FuerzaRiesgo => PideForzarRiesgo && Tiene(Permisos.RiesgoForzar);
}
