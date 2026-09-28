using AlxorCore.Informes.Aplicacion;
using Microsoft.AspNetCore.DataProtection;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Cifrado de secretos en reposo (certificados electrónicos) con la protección de datos de ASP.NET Core. Las claves se
/// guardan donde indique <c>ProteccionDatos:Ruta</c>; sin esa ruta, en el perfil del proceso (válido en desarrollo).
/// </summary>
public sealed class ProtectorSecretos : IProtectorSecretos
{
    private readonly IDataProtector _protector;

    public ProtectorSecretos(IDataProtectionProvider proveedor)
    {
        ArgumentNullException.ThrowIfNull(proveedor);
        _protector = proveedor.CreateProtector("AlxorCore.Secretos.v1");
    }

    public byte[] Proteger(byte[] datos) => _protector.Protect(datos);

    public byte[] Desproteger(byte[] datos) => _protector.Unprotect(datos);
}
