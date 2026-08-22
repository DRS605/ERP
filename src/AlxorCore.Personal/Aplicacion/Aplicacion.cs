using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Personal.Dominio;

namespace AlxorCore.Personal.Aplicacion;

/// <summary>Vista de una persona.</summary>
public sealed record PersonaDto(Guid Id, string Nombre, string? Puesto, decimal TarifaHora, bool Activo)
{
    public static PersonaDto Desde(Persona p) => new(p.Id, p.Nombre, p.Puesto, p.TarifaHora, p.Activo);
}

/// <summary>Datos para crear o actualizar una persona.</summary>
public sealed record DatosPersona(string Nombre, decimal TarifaHora, string? Puesto = null, bool Activo = true);

/// <summary>Repositorio de personas (escritura).</summary>
public interface IRepositorioPersonas
{
    void Agregar(Persona persona);
    Task<Persona?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
}

/// <summary>Consultas de personas (las usan la API y Proyectos para imputar mano de obra).</summary>
public interface IConsultaPersonas
{
    Task<PersonaDto?> ObtenerAsync(Guid personaId, CancellationToken ct = default);
    Task<IReadOnlyList<PersonaDto>> ListarAsync(Guid empresaId, bool incluirInactivas = false, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Personal.</summary>
public interface IUnidadDeTrabajoPersonal : IUnidadDeTrabajo;

public sealed class CrearPersona
{
    private readonly IRepositorioPersonas _repo;
    private readonly IUnidadDeTrabajoPersonal _unidad;
    private readonly IReloj _reloj;

    public CrearPersona(IRepositorioPersonas repo, IUnidadDeTrabajoPersonal unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<PersonaDto>> EjecutarAsync(Guid empresaId, DatosPersona datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var persona = Persona.Crear(empresaId, datos.Nombre, datos.Puesto, datos.TarifaHora, _reloj);
        if (persona.EsFallo)
        {
            return Resultado.Fallo<PersonaDto>(persona.Error);
        }

        _repo.Agregar(persona.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PersonaDto.Desde(persona.Valor));
    }
}

public sealed class ActualizarPersona
{
    private readonly IRepositorioPersonas _repo;
    private readonly IUnidadDeTrabajoPersonal _unidad;
    private readonly IReloj _reloj;

    public ActualizarPersona(IRepositorioPersonas repo, IUnidadDeTrabajoPersonal unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<PersonaDto>> EjecutarAsync(Guid id, DatosPersona datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var persona = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (persona is null)
        {
            return Resultado.Fallo<PersonaDto>(Error.NoEncontrado("persona.no_encontrada", "La persona no existe."));
        }

        var r = persona.Actualizar(datos.Nombre, datos.Puesto, datos.TarifaHora, datos.Activo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PersonaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PersonaDto.Desde(persona));
    }
}

public sealed class ListarPersonas
{
    private readonly IConsultaPersonas _consulta;
    public ListarPersonas(IConsultaPersonas consulta) => _consulta = consulta;
    public Task<IReadOnlyList<PersonaDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _consulta.ListarAsync(empresaId, false, ct);
}
