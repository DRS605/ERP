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
    Guid? ActividadNegocioId = null,
    Guid? TarifaId = null)
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
        c.EsAdministracionPublica, c.Dir3OficinaContable, c.Dir3OrganoGestor, c.Dir3UnidadTramitadora, c.ActividadNegocioId, c.TarifaId);
}

/// <summary>Repositorio de clientes (escritura).</summary>
public interface IRepositorioClientes
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(Cliente cliente);

    void Eliminar(Cliente cliente);
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

/// <summary>Caso de uso: asignar (o quitar) la tarifa de precios de un cliente. La existencia de la tarifa la comprueba quien llama.</summary>
public sealed class AsignarTarifaCliente
{
    private readonly IRepositorioClientes _clientes;
    private readonly IUnidadDeTrabajoTerceros _unidadDeTrabajo;

    public AsignarTarifaCliente(IRepositorioClientes clientes, IUnidadDeTrabajoTerceros unidadDeTrabajo)
    {
        _clientes = clientes;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<AlxorCore.Nucleo.Resultados.Resultado<ClienteDto>> EjecutarAsync(Guid clienteId, Guid? tarifaId, CancellationToken ct = default)
    {
        var cliente = await _clientes.ObtenerPorIdAsync(clienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return AlxorCore.Nucleo.Resultados.Resultado.Fallo<ClienteDto>(
                AlxorCore.Nucleo.Resultados.Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        cliente.AsignarTarifa(tarifaId);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return AlxorCore.Nucleo.Resultados.Resultado.Ok(ClienteDto.Desde(cliente));
    }
}
