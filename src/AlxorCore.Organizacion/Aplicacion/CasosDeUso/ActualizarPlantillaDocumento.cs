using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>
/// Datos que el usuario puede personalizar de sus documentos (facturas, tickets, presupuestos):
/// razón social y dirección que encabezan el impreso, datos de contacto, color corporativo, texto de
/// pie y logotipo (PNG en Base64; cadena vacía = quitar el logo, null = no tocarlo).
/// </summary>
public sealed record PlantillaDocumentoComando(
    string? RazonSocial,
    string? Calle,
    string? CodigoPostal,
    string? Poblacion,
    string? Provincia,
    string? Telefono,
    string? Web,
    string? Email,
    string? ColorPrincipal,
    string? TextoPie,
    string? LogoPngBase64);

/// <summary>Caso de uso: configurar la plantilla de documentos de la empresa activa.</summary>
public sealed class ActualizarPlantillaDocumento
{
    private readonly IRepositorioEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarPlantillaDocumento(IRepositorioEmpresas empresas, IUnidadDeTrabajoOrganizacion unidadDeTrabajo, IReloj reloj)
    {
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<EmpresaDto>> EjecutarAsync(Guid empresaId, PlantillaDocumentoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var empresa = await _empresas.ObtenerPorIdAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<EmpresaDto>(Error.NoEncontrado("empresa.no_encontrada", "La empresa no existe."));
        }

        byte[]? logo = null;
        if (comando.LogoPngBase64 is not null)
        {
            if (comando.LogoPngBase64.Length == 0)
            {
                logo = Array.Empty<byte>(); // quitar el logo
            }
            else if (!TryDecodificarBase64(comando.LogoPngBase64, out logo))
            {
                return Resultado.Fallo<EmpresaDto>(Error.Validacion("empresa.logo_base64", "El logotipo no es un Base64 válido."));
            }
        }

        // Razón social y dirección (cabecera del impreso); el régimen de IVA no se toca aquí.
        var direccion = Direccion.Crear(comando.Calle, comando.CodigoPostal, comando.Poblacion, comando.Provincia, empresa.Direccion.Pais);
        empresa.ActualizarDatos(comando.RazonSocial ?? empresa.RazonSocial, direccion, empresa.RegimenIva, _reloj);

        var plantilla = empresa.EstablecerPlantillaDocumento(comando.Telefono, comando.Web, comando.Email, comando.ColorPrincipal, comando.TextoPie, logo, _reloj);
        if (plantilla.EsFallo)
        {
            return Resultado.Fallo<EmpresaDto>(plantilla.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(EmpresaDto.Desde(empresa));
    }

    private static bool TryDecodificarBase64(string valor, out byte[]? datos)
    {
        datos = null;
        // Admite el prefijo «data:image/png;base64,» de los data URI.
        var coma = valor.IndexOf(',', StringComparison.Ordinal);
        var limpio = valor.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && coma >= 0 ? valor[(coma + 1)..] : valor;
        var buffer = new byte[((limpio.Length * 3) + 3) / 4];
        if (Convert.TryFromBase64String(limpio, buffer, out var escritos))
        {
            datos = buffer[..escritos];
            return true;
        }

        return false;
    }
}
