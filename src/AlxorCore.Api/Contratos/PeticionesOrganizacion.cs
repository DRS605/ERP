using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Api.Contratos;

/// <summary>Cuerpo para crear una empresa.</summary>
public sealed record CrearEmpresaPeticion(
    string Nif,
    string RazonSocial,
    string? Calle = null,
    string? CodigoPostal = null,
    string? Poblacion = null,
    string? Provincia = null,
    RegimenIva RegimenIva = RegimenIva.General,
    Guid? GrupoId = null,
    AlxorCore.Nucleo.Comun.TerritorioFiscal? TerritorioFiscal = null,
    string? Telefono = null,
    string? Email = null,
    string? Web = null,
    string? Iban = null,
    string? IdentificadorAcreedor = null,
    string? Edicion = null,
    IReadOnlyList<string>? ModulosAdicionales = null,
    AlxorCore.Nucleo.Comun.MetodoValoracion? MetodoValoracion = null,
    AlxorCore.Nucleo.Comun.ControlRiesgo? ControlRiesgo = null);

/// <summary>Cuerpo para crear una serie de numeración.</summary>
public sealed record CrearSeriePeticion(TipoDocumento TipoDocumento, int Ejercicio, string Prefijo);
