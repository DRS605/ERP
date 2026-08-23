using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Consultas;
using AlxorCore.Terceros.Dominio;

namespace AlxorCore.Terceros.Aplicacion;

/// <summary>
/// Filtros de búsqueda de terceros (clientes o proveedores) en servidor. <paramref name="Texto"/>
/// busca en el nombre, el NIF y el correo. <paramref name="ActividadesPermitidas"/> restringe el
/// resultado a las actividades de negocio que el usuario puede ver en la pantalla (más los terceros
/// sin actividad, visibles siempre); <c>null</c> = sin restricción por actividad (ve todas).
/// </summary>
public sealed record FiltroTerceros(string? Texto = null, bool IncluirInactivos = false, IReadOnlyCollection<Guid>? ActividadesPermitidas = null);

/// <summary>Vista de un cliente.</summary>
public sealed record ClienteDto(
    Guid Id,
    string Nombre,
    string? NifFiscal,
    string? Email,
    string Calle,
    string CodigoPostal,
    string Poblacion,
    string Provincia,
    string Pais,
    decimal PorcentajeIrpfDefecto,
    bool Activo,
    bool RecargoEquivalencia,
    string? Iban,
    string? MandatoReferencia,
    DateOnly? MandatoFecha,
    string? NifIva,
    string? Tipo,
    Guid? FormaPagoDefectoId,
    decimal? LimiteRiesgo,
    bool EsAdministracionPublica = false,
    string? Dir3OficinaContable = null,
    string? Dir3OrganoGestor = null,
    string? Dir3UnidadTramitadora = null,
    Guid? ActividadNegocioId = null)
{
    /// <summary>¿Tiene los tres centros DIR3 necesarios para enviar la Facturae por FACe?</summary>
    public bool CentrosDir3Completos =>
        EsAdministracionPublica
        && !string.IsNullOrWhiteSpace(Dir3OficinaContable)
        && !string.IsNullOrWhiteSpace(Dir3OrganoGestor)
        && !string.IsNullOrWhiteSpace(Dir3UnidadTramitadora);

    public static ClienteDto Desde(Cliente c) => new(
        c.Id, c.Nombre, c.NifFiscal, c.Email,
        c.Direccion.Calle, c.Direccion.CodigoPostal, c.Direccion.Poblacion, c.Direccion.Provincia, c.Direccion.Pais,
        c.PorcentajeIrpfDefecto, c.Activo, c.RecargoEquivalencia, c.Iban, c.MandatoReferencia, c.MandatoFecha, c.NifIva, c.Tipo, c.FormaPagoDefectoId, c.LimiteRiesgo,
        c.EsAdministracionPublica, c.Dir3OficinaContable, c.Dir3OrganoGestor, c.Dir3UnidadTramitadora, c.ActividadNegocioId);
}

/// <summary>Repositorio de clientes (escritura).</summary>
public interface IRepositorioClientes
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(Cliente cliente);
}

/// <summary>Consultas de lectura de clientes (las usan la propia API y otros módulos como Facturación).</summary>
public interface IConsultaClientes
{
    Task<ClienteDto?> ObtenerAsync(Guid clienteId, CancellationToken ct = default);

    Task<IReadOnlyList<ClienteDto>> ListarAsync(Guid grupoId, bool incluirInactivos = false, IReadOnlyCollection<Guid>? actividadesPermitidas = null, CancellationToken ct = default);

    /// <summary>Búsqueda paginada y filtrada de clientes (el filtrado ocurre en la base de datos).</summary>
    Task<PaginaResultado<ClienteDto>> BuscarAsync(Guid grupoId, FiltroTerceros filtro, Paginacion paginacion, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Terceros.</summary>
public interface IUnidadDeTrabajoTerceros : IUnidadDeTrabajo;
