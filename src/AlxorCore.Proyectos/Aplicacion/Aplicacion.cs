using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Proyectos.Dominio;

namespace AlxorCore.Proyectos.Aplicacion;

// ---------------------------------------------------------------------------- DTOs
public sealed record ImputacionDto(Guid Id, string Tipo, DateOnly Fecha, string Descripcion, Guid? ReferenciaId,
    decimal Cantidad, decimal CosteUnitario, decimal Importe)
{
    public static ImputacionDto Desde(Imputacion i) =>
        new(i.Id, i.Tipo.ToString(), i.Fecha, i.Descripcion, i.ReferenciaId, i.Cantidad, i.CosteUnitario, i.Importe);
}

/// <summary>Resumen de un proyecto (para el listado).</summary>
public sealed record ProyectoDto(Guid Id, int Ejercicio, int Numero, string Nombre, Guid? ClienteId, string? ClienteNombre,
    string Estado, DateOnly FechaInicio, decimal Presupuesto, decimal CosteReal, decimal Desviacion)
{
    public static ProyectoDto Desde(Proyecto p) => new(p.Id, p.Ejercicio, p.Numero, p.Nombre, p.ClienteId, p.ClienteNombre,
        p.Estado.ToString(), p.FechaInicio, p.Presupuesto, p.CosteReal, p.Desviacion);
}

/// <summary>Detalle de un proyecto con sus imputaciones y subtotales por tipo.</summary>
public sealed record ProyectoDetalleDto(Guid Id, int Ejercicio, int Numero, string Nombre, Guid? ClienteId, string? ClienteNombre,
    string Estado, DateOnly FechaInicio, decimal Presupuesto, decimal CosteManoObra, decimal CosteMateriales,
    decimal CosteGastos, decimal CosteReal, decimal Desviacion, IReadOnlyList<ImputacionDto> Imputaciones)
{
    public static ProyectoDetalleDto Desde(Proyecto p) => new(p.Id, p.Ejercicio, p.Numero, p.Nombre, p.ClienteId, p.ClienteNombre,
        p.Estado.ToString(), p.FechaInicio, p.Presupuesto, p.CosteManoObra, p.CosteMateriales, p.CosteGastos, p.CosteReal,
        p.Desviacion, p.Imputaciones.OrderBy(i => i.Fecha).Select(ImputacionDto.Desde).ToList());
}

// ---------------------------------------------------------------------------- Puertos
public interface IRepositorioProyectos
{
    void Agregar(Proyecto proyecto);
    Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ProyectoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<ProyectoDetalleDto?> ObtenerDetalleAsync(Guid id, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

public interface IUnidadDeTrabajoProyectos : IUnidadDeTrabajo;

/// <summary>Tarifa/hora de una persona del módulo Personal (para imputar mano de obra).</summary>
public interface ITarifaPersona
{
    /// <summary>Nombre y tarifa de la persona; null si no existe en la empresa.</summary>
    Task<(string Nombre, decimal TarifaHora)?> ObtenerAsync(Guid personaId, CancellationToken ct = default);
}

/// <summary>Nombre de un artículo del Catálogo (para describir la imputación de material).</summary>
public interface IConsultaArticuloProyectos
{
    Task<string?> NombreAsync(Guid productoId, CancellationToken ct = default);
}

// ---------------------------------------------------------------------------- Comandos
public sealed record DatosProyecto(string Nombre, decimal Presupuesto, Guid? ClienteId = null, string? ClienteNombre = null, DateOnly? FechaInicio = null);
public sealed record ImputarManoObraComando(Guid PersonaId, decimal Horas, DateOnly? Fecha = null, string? Descripcion = null);
public sealed record ImputarMaterialComando(Guid ProductoId, decimal Cantidad, DateOnly? Fecha = null);
public sealed record ImputarGastoComando(string Concepto, decimal Importe, DateOnly? Fecha = null);

// ---------------------------------------------------------------------------- Casos de uso
public sealed class CrearProyecto
{
    private readonly IRepositorioProyectos _repo;
    private readonly IUnidadDeTrabajoProyectos _unidad;
    private readonly IReloj _reloj;

    public CrearProyecto(IRepositorioProyectos repo, IUnidadDeTrabajoProyectos unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<ProyectoDto>> EjecutarAsync(Guid empresaId, DatosProyecto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var fecha = datos.FechaInicio ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _repo.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var proyecto = Proyecto.Crear(empresaId, numero, datos.Nombre, datos.ClienteId, datos.ClienteNombre, datos.Presupuesto, fecha, _reloj);
        if (proyecto.EsFallo)
        {
            return Resultado.Fallo<ProyectoDto>(proyecto.Error);
        }

        _repo.Agregar(proyecto.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProyectoDto.Desde(proyecto.Valor));
    }
}

public sealed class ActualizarProyecto
{
    private readonly IRepositorioProyectos _repo;
    private readonly IUnidadDeTrabajoProyectos _unidad;
    private readonly IReloj _reloj;

    public ActualizarProyecto(IRepositorioProyectos repo, IUnidadDeTrabajoProyectos unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<ProyectoDto>> EjecutarAsync(Guid id, DatosProyecto datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var proyecto = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (proyecto is null)
        {
            return Resultado.Fallo<ProyectoDto>(Error.NoEncontrado("proyecto.no_encontrado", "No se encontró el proyecto."));
        }

        var r = proyecto.ActualizarDatos(datos.Nombre, datos.ClienteId, datos.ClienteNombre, datos.Presupuesto, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ProyectoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProyectoDto.Desde(proyecto));
    }
}

public sealed class CambiarEstadoProyecto
{
    private readonly IRepositorioProyectos _repo;
    private readonly IUnidadDeTrabajoProyectos _unidad;
    private readonly IReloj _reloj;

    public CambiarEstadoProyecto(IRepositorioProyectos repo, IUnidadDeTrabajoProyectos unidad, IReloj reloj)
    {
        _repo = repo; _unidad = unidad; _reloj = reloj;
    }

    public Task<Resultado<ProyectoDto>> CerrarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Cerrar(_reloj), ct);
    public Task<Resultado<ProyectoDto>> ReabrirAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Reabrir(_reloj), ct);
    public Task<Resultado<ProyectoDto>> CancelarAsync(Guid id, CancellationToken ct = default) => CambiarAsync(id, p => p.Cancelar(_reloj), ct);

    private async Task<Resultado<ProyectoDto>> CambiarAsync(Guid id, Func<Proyecto, Resultado> accion, CancellationToken ct)
    {
        var proyecto = await _repo.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (proyecto is null)
        {
            return Resultado.Fallo<ProyectoDto>(Error.NoEncontrado("proyecto.no_encontrado", "No se encontró el proyecto."));
        }

        var r = accion(proyecto);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ProyectoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProyectoDto.Desde(proyecto));
    }
}

/// <summary>Imputa costes (mano de obra, material o gasto) a un proyecto y elimina imputaciones.</summary>
public sealed class ImputarCostes
{
    private readonly IRepositorioProyectos _repo;
    private readonly ITarifaPersona _tarifas;
    private readonly IValoracionArticulos _valoracion;
    private readonly IConsultaArticuloProyectos _articulos;
    private readonly IUnidadDeTrabajoProyectos _unidad;
    private readonly IReloj _reloj;

    public ImputarCostes(IRepositorioProyectos repo, ITarifaPersona tarifas, IValoracionArticulos valoracion,
        IConsultaArticuloProyectos articulos, IUnidadDeTrabajoProyectos unidad, IReloj reloj)
    {
        _repo = repo; _tarifas = tarifas; _valoracion = valoracion; _articulos = articulos; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<ProyectoDetalleDto>> ManoObraAsync(Guid empresaId, Guid proyectoId, ImputarManoObraComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var proyecto = await CargarAbierto(proyectoId, ct).ConfigureAwait(false);
        if (proyecto.EsFallo) return Resultado.Fallo<ProyectoDetalleDto>(proyecto.Error);

        var persona = await _tarifas.ObtenerAsync(c.PersonaId, ct).ConfigureAwait(false);
        if (persona is null)
        {
            return Resultado.Fallo<ProyectoDetalleDto>(Error.Validacion("imputacion.persona", "La persona no existe."));
        }

        var fecha = c.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var desc = string.IsNullOrWhiteSpace(c.Descripcion) ? persona.Value.Nombre : c.Descripcion;
        var r = proyecto.Valor.ImputarManoObra(c.PersonaId, desc, c.Horas, persona.Value.TarifaHora, fecha, _reloj);
        return await Finalizar(proyecto.Valor, r, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<ProyectoDetalleDto>> MaterialAsync(Guid empresaId, Guid proyectoId, ImputarMaterialComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var proyecto = await CargarAbierto(proyectoId, ct).ConfigureAwait(false);
        if (proyecto.EsFallo) return Resultado.Fallo<ProyectoDetalleDto>(proyecto.Error);

        var nombre = await _articulos.NombreAsync(c.ProductoId, ct).ConfigureAwait(false);
        if (nombre is null)
        {
            return Resultado.Fallo<ProyectoDetalleDto>(Error.Validacion("imputacion.articulo", "El artículo no existe."));
        }

        // Coste unitario según el método de valoración de la empresa (estándar/última compra/PMP/FIFO).
        var coste = await _valoracion.ValorUnitarioAsync(empresaId, c.ProductoId, null, ct).ConfigureAwait(false);
        var fecha = c.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var r = proyecto.Valor.ImputarMaterial(c.ProductoId, nombre, c.Cantidad, coste, fecha, _reloj);
        return await Finalizar(proyecto.Valor, r, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<ProyectoDetalleDto>> GastoAsync(Guid empresaId, Guid proyectoId, ImputarGastoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var proyecto = await CargarAbierto(proyectoId, ct).ConfigureAwait(false);
        if (proyecto.EsFallo) return Resultado.Fallo<ProyectoDetalleDto>(proyecto.Error);

        var fecha = c.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var r = proyecto.Valor.ImputarGasto(c.Concepto, c.Importe, fecha, _reloj);
        return await Finalizar(proyecto.Valor, r, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<ProyectoDetalleDto>> EliminarAsync(Guid proyectoId, Guid imputacionId, CancellationToken ct = default)
    {
        var proyecto = await _repo.ObtenerPorIdAsync(proyectoId, ct).ConfigureAwait(false);
        if (proyecto is null)
        {
            return Resultado.Fallo<ProyectoDetalleDto>(Error.NoEncontrado("proyecto.no_encontrado", "No se encontró el proyecto."));
        }

        var r = proyecto.EliminarImputacion(imputacionId, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ProyectoDetalleDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProyectoDetalleDto.Desde(proyecto));
    }

    private async Task<Resultado<Proyecto>> CargarAbierto(Guid proyectoId, CancellationToken ct)
    {
        var proyecto = await _repo.ObtenerPorIdAsync(proyectoId, ct).ConfigureAwait(false);
        if (proyecto is null)
        {
            return Resultado.Fallo<Proyecto>(Error.NoEncontrado("proyecto.no_encontrado", "No se encontró el proyecto."));
        }

        return Resultado.Ok(proyecto);
    }

    private async Task<Resultado<ProyectoDetalleDto>> Finalizar(Proyecto proyecto, Resultado<Imputacion> r, CancellationToken ct)
    {
        if (r.EsFallo)
        {
            return Resultado.Fallo<ProyectoDetalleDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ProyectoDetalleDto.Desde(proyecto));
    }
}

public sealed class ListarProyectos
{
    private readonly IRepositorioProyectos _repo;
    public ListarProyectos(IRepositorioProyectos repo) => _repo = repo;
    public Task<IReadOnlyList<ProyectoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);
}

public sealed class ObtenerProyecto
{
    private readonly IRepositorioProyectos _repo;
    public ObtenerProyecto(IRepositorioProyectos repo) => _repo = repo;
    public Task<ProyectoDetalleDto?> EjecutarAsync(Guid id, CancellationToken ct = default) => _repo.ObtenerDetalleAsync(id, ct);
}
