using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Vista de una instalación vendida (panel de la plataforma).</summary>
public sealed record InstalacionDto(
    Guid Id, string Nombre, string? Contacto, string? Email, string? Telefono, string? Poblacion,
    PlanSuscripcion Plan, EstadoInstalacion Estado, decimal CuotaMensual, DateOnly FechaAlta, DateOnly? ProximoCobro,
    string Clave, string? Notas, DateTimeOffset? UltimaConexion, bool LicenciaValida)
{
    public static InstalacionDto Desde(Instalacion i) =>
        new(i.Id, i.Nombre, i.Contacto, i.Email, i.Telefono, i.Poblacion, i.Plan, i.Estado, i.CuotaMensual,
            i.FechaAlta, i.ProximoCobro, i.Clave, i.Notas, i.UltimaConexion, i.LicenciaValida);
}

/// <summary>Resumen del panel: recuentos por estado e ingresos recurrentes (MRR).</summary>
public sealed record ResumenPlataformaDto(
    int Activas, int Prospectos, int Suspendidas, int Bajas, decimal MrrActivo, int CobrosProximos7Dias);

/// <summary>Respuesta de validación de licencia que recibe la instalación del cliente.</summary>
public sealed record LicenciaDto(bool Valida, EstadoInstalacion Estado, PlanSuscripcion Plan, string Mensaje);

/// <summary>Datos de entrada para crear o editar una instalación.</summary>
public sealed record DatosInstalacion(
    string Nombre, string? Contacto, string? Email, string? Telefono, string? Poblacion,
    PlanSuscripcion Plan, EstadoInstalacion Estado, decimal CuotaMensual, DateOnly? ProximoCobro, string? Notas);

/// <summary>Caso de uso: listar todas las instalaciones.</summary>
public sealed class ListarInstalaciones
{
    private readonly IRepositorioInstalaciones _repo;

    public ListarInstalaciones(IRepositorioInstalaciones repo) => _repo = repo;

    public async Task<IReadOnlyList<InstalacionDto>> EjecutarAsync(CancellationToken ct = default)
    {
        var lista = await _repo.ListarAsync(ct).ConfigureAwait(false);
        return lista.Select(InstalacionDto.Desde).ToList();
    }
}

/// <summary>Caso de uso: resumen (recuentos + MRR) del panel.</summary>
public sealed class ResumenPlataforma
{
    private readonly IRepositorioInstalaciones _repo;
    private readonly IReloj _reloj;

    public ResumenPlataforma(IRepositorioInstalaciones repo, IReloj reloj)
    {
        _repo = repo;
        _reloj = reloj;
    }

    public async Task<ResumenPlataformaDto> EjecutarAsync(CancellationToken ct = default)
    {
        var lista = await _repo.ListarAsync(ct).ConfigureAwait(false);
        var hoy = DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var limite = hoy.AddDays(7);
        return new ResumenPlataformaDto(
            Activas: lista.Count(i => i.Estado == EstadoInstalacion.Activa),
            Prospectos: lista.Count(i => i.Estado == EstadoInstalacion.Prospecto),
            Suspendidas: lista.Count(i => i.Estado == EstadoInstalacion.Suspendida),
            Bajas: lista.Count(i => i.Estado == EstadoInstalacion.Baja),
            MrrActivo: lista.Where(i => i.Estado == EstadoInstalacion.Activa).Sum(i => i.CuotaMensual),
            CobrosProximos7Dias: lista.Count(i => i.Estado == EstadoInstalacion.Activa && i.ProximoCobro is { } p && p <= limite));
    }
}

/// <summary>Caso de uso: alta de una instalación (genera su clave de licencia).</summary>
public sealed class CrearInstalacion
{
    private readonly IRepositorioInstalaciones _repo;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public CrearInstalacion(IRepositorioInstalaciones repo, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _repo = repo;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<InstalacionDto>> EjecutarAsync(DatosInstalacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var creada = Instalacion.Crear(datos.Nombre, datos.Contacto, datos.Email, datos.Telefono, datos.Poblacion,
            datos.Plan, datos.Estado, datos.CuotaMensual, datos.ProximoCobro, datos.Notas, _reloj);
        if (creada.EsFallo)
        {
            return Resultado.Fallo<InstalacionDto>(creada.Error);
        }

        _repo.Agregar(creada.Valor);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InstalacionDto.Desde(creada.Valor));
    }
}

/// <summary>Caso de uso: editar una instalación.</summary>
public sealed class ActualizarInstalacion
{
    private readonly IRepositorioInstalaciones _repo;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public ActualizarInstalacion(IRepositorioInstalaciones repo, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _repo = repo;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<InstalacionDto>> EjecutarAsync(Guid id, DatosInstalacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var instalacion = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (instalacion is null)
        {
            return Resultado.Fallo<InstalacionDto>(Error.NoEncontrado("instalacion.no_encontrada", "La instalación no existe."));
        }

        var r = instalacion.Actualizar(datos.Nombre, datos.Contacto, datos.Email, datos.Telefono, datos.Poblacion,
            datos.Plan, datos.Estado, datos.CuotaMensual, datos.ProximoCobro, datos.Notas, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<InstalacionDto>(r.Error);
        }

        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InstalacionDto.Desde(instalacion));
    }
}

/// <summary>Caso de uso: cambiar sólo el estado (vender, suspender por impago, baja…).</summary>
public sealed class CambiarEstadoInstalacion
{
    private readonly IRepositorioInstalaciones _repo;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public CambiarEstadoInstalacion(IRepositorioInstalaciones repo, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _repo = repo;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<Resultado<InstalacionDto>> EjecutarAsync(Guid id, EstadoInstalacion estado, CancellationToken ct = default)
    {
        var instalacion = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (instalacion is null)
        {
            return Resultado.Fallo<InstalacionDto>(Error.NoEncontrado("instalacion.no_encontrada", "La instalación no existe."));
        }

        instalacion.CambiarEstado(estado, _reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(InstalacionDto.Desde(instalacion));
    }
}

/// <summary>Caso de uso: validar una licencia por su clave (lo llama la instalación del cliente).</summary>
public sealed class VerificarLicencia
{
    private readonly IRepositorioInstalaciones _repo;
    private readonly IUnidadDeTrabajoOrganizacion _uow;
    private readonly IReloj _reloj;

    public VerificarLicencia(IRepositorioInstalaciones repo, IUnidadDeTrabajoOrganizacion uow, IReloj reloj)
    {
        _repo = repo;
        _uow = uow;
        _reloj = reloj;
    }

    public async Task<LicenciaDto> EjecutarAsync(string? clave, CancellationToken ct = default)
    {
        var instalacion = string.IsNullOrWhiteSpace(clave)
            ? null
            : await _repo.ObtenerPorClaveAsync(clave.Trim(), ct).ConfigureAwait(false);

        if (instalacion is null)
        {
            return new LicenciaDto(false, EstadoInstalacion.Baja, PlanSuscripcion.Autonomo, "Licencia no reconocida.");
        }

        instalacion.RegistrarConexion(_reloj);
        await _uow.GuardarCambiosAsync(ct).ConfigureAwait(false);

        var mensaje = instalacion.LicenciaValida
            ? "Licencia activa."
            : "Licencia suspendida. Contacta con tu proveedor.";
        return new LicenciaDto(instalacion.LicenciaValida, instalacion.Estado, instalacion.Plan, mensaje);
    }
}
