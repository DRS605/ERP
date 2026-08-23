using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Dominio;

namespace AlxorCore.Terceros.Aplicacion;

/// <summary>Datos de un cliente para crear o actualizar.</summary>
public sealed record DatosCliente(
    string Nombre,
    string? NifFiscal = null,
    string? Email = null,
    string? Calle = null,
    string? CodigoPostal = null,
    string? Poblacion = null,
    string? Provincia = null,
    string? Pais = null,
    decimal PorcentajeIrpfDefecto = 0m,
    bool RecargoEquivalencia = false,
    string? Iban = null,
    string? MandatoReferencia = null,
    DateOnly? MandatoFecha = null,
    string? NifIva = null,
    string? Tipo = null,
    Guid? FormaPagoDefectoId = null,
    decimal? LimiteRiesgo = null,
    bool EsAdministracionPublica = false,
    string? Dir3OficinaContable = null,
    string? Dir3OrganoGestor = null,
    string? Dir3UnidadTramitadora = null,
    Guid? ActividadNegocioId = null);

/// <summary>Caso de uso: crear un cliente en la empresa activa.</summary>
public sealed class CrearCliente
{
    private readonly IRepositorioClientes _clientes;
    private readonly IUnidadDeTrabajoTerceros _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearCliente(IRepositorioClientes clientes, IUnidadDeTrabajoTerceros unidadDeTrabajo, IReloj reloj)
    {
        _clientes = clientes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ClienteDto>> EjecutarAsync(Guid grupoId, DatosCliente datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var direccion = Direccion.Crear(datos.Calle, datos.CodigoPostal, datos.Poblacion, datos.Provincia, datos.Pais);
        var cliente = Cliente.Crear(grupoId, datos.Nombre, datos.NifFiscal, datos.Email, direccion, datos.PorcentajeIrpfDefecto, _reloj, datos.RecargoEquivalencia, datos.Iban, datos.MandatoReferencia, datos.MandatoFecha, datos.NifIva);
        if (cliente.EsFallo)
        {
            return Resultado.Fallo<ClienteDto>(cliente.Error);
        }

        cliente.Valor.EstablecerTipo(datos.Tipo);
        cliente.Valor.EstablecerFormaPagoDefecto(datos.FormaPagoDefectoId);
        cliente.Valor.EstablecerLimiteRiesgo(datos.LimiteRiesgo);
        cliente.Valor.EstablecerCentrosDir3(datos.EsAdministracionPublica, datos.Dir3OficinaContable, datos.Dir3OrganoGestor, datos.Dir3UnidadTramitadora);
        cliente.Valor.EstablecerActividad(datos.ActividadNegocioId);
        _clientes.Agregar(cliente.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ClienteDto.Desde(cliente.Valor));
    }
}

/// <summary>Caso de uso: actualizar un cliente existente.</summary>
public sealed class ActualizarCliente
{
    private readonly IRepositorioClientes _clientes;
    private readonly IUnidadDeTrabajoTerceros _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarCliente(IRepositorioClientes clientes, IUnidadDeTrabajoTerceros unidadDeTrabajo, IReloj reloj)
    {
        _clientes = clientes;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<ClienteDto>> EjecutarAsync(Guid clienteId, DatosCliente datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var cliente = await _clientes.ObtenerPorIdAsync(clienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<ClienteDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var direccion = Direccion.Crear(datos.Calle, datos.CodigoPostal, datos.Poblacion, datos.Provincia, datos.Pais);
        var actualizado = cliente.Actualizar(datos.Nombre, datos.NifFiscal, datos.Email, direccion, datos.PorcentajeIrpfDefecto, _reloj, datos.RecargoEquivalencia, datos.Iban, datos.MandatoReferencia, datos.MandatoFecha, datos.NifIva);
        if (actualizado.EsFallo)
        {
            return Resultado.Fallo<ClienteDto>(actualizado.Error);
        }

        cliente.EstablecerTipo(datos.Tipo);
        cliente.EstablecerFormaPagoDefecto(datos.FormaPagoDefectoId);
        cliente.EstablecerLimiteRiesgo(datos.LimiteRiesgo);
        cliente.EstablecerCentrosDir3(datos.EsAdministracionPublica, datos.Dir3OficinaContable, datos.Dir3OrganoGestor, datos.Dir3UnidadTramitadora);
        cliente.EstablecerActividad(datos.ActividadNegocioId);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ClienteDto.Desde(cliente));
    }
}

/// <summary>Caso de uso: listar los clientes de la empresa activa.</summary>
public sealed class ListarClientes
{
    private readonly IConsultaClientes _consulta;

    public ListarClientes(IConsultaClientes consulta) => _consulta = consulta;

    public Task<IReadOnlyList<ClienteDto>> EjecutarAsync(Guid grupoId, IReadOnlyCollection<Guid>? actividadesPermitidas = null, CancellationToken ct = default) =>
        _consulta.ListarAsync(grupoId, false, actividadesPermitidas, ct);
}

/// <summary>Caso de uso: buscar clientes con filtros y paginación (en servidor).</summary>
public sealed class BuscarClientes
{
    private readonly IConsultaClientes _consulta;

    public BuscarClientes(IConsultaClientes consulta) => _consulta = consulta;

    public Task<Nucleo.Consultas.PaginaResultado<ClienteDto>> EjecutarAsync(Guid grupoId, FiltroTerceros filtro, Nucleo.Consultas.Paginacion paginacion, CancellationToken ct = default) =>
        _consulta.BuscarAsync(grupoId, filtro, paginacion, ct);
}

/// <summary>Caso de uso: obtener un cliente por su identificador.</summary>
public sealed class ObtenerCliente
{
    private readonly IConsultaClientes _consulta;

    public ObtenerCliente(IConsultaClientes consulta) => _consulta = consulta;

    public async Task<Resultado<ClienteDto>> EjecutarAsync(Guid clienteId, CancellationToken ct = default)
    {
        var cliente = await _consulta.ObtenerAsync(clienteId, ct).ConfigureAwait(false);
        return cliente is null
            ? Resultado.Fallo<ClienteDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."))
            : Resultado.Ok(cliente);
    }
}
