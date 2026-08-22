using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Produccion.Dominio;

namespace AlxorCore.Produccion.Aplicacion;

// ---------------------------------------------------------------------------- DTOs
public sealed record ComponentePlanDto(Guid ComponenteId, string Nombre, decimal CantidadUnitaria, decimal CantidadTotal);

public sealed record OrdenFabricacionDto(Guid Id, int Ejercicio, int Numero, Guid ProductoId, string ProductoNombre,
    decimal Cantidad, Guid AlmacenId, DateOnly Fecha, string Estado, DateTimeOffset CreadoEn, DateTimeOffset? TerminadaEn,
    IReadOnlyList<ComponentePlanDto> Componentes)
{
    public static OrdenFabricacionDto Desde(OrdenFabricacion o) => new(o.Id, o.Ejercicio, o.Numero, o.ProductoId, o.ProductoNombre,
        o.Cantidad, o.AlmacenId, o.Fecha, o.Estado.ToString(), o.CreadoEn, o.TerminadaEn,
        o.Componentes.Select(c => new ComponentePlanDto(c.ComponenteId, c.Nombre, c.CantidadUnitaria, c.CantidadTotal)).ToList());
}

// ---------------------------------------------------------------------------- Puertos
public interface IRepositorioOrdenes
{
    void Agregar(OrdenFabricacion orden);
    Task<OrdenFabricacion?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenFabricacionDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<OrdenFabricacionDto?> ObtenerDtoAsync(Guid id, CancellationToken ct = default);
    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

public interface IUnidadDeTrabajoProduccion : IUnidadDeTrabajo;

/// <summary>Lee la lista de materiales (escandallo) de un artículo del Catálogo para planificar la orden.</summary>
public interface IConsultaListaMateriales
{
    /// <summary>Devuelve nombre del artículo + sus componentes; null si el artículo no es compuesto o no existe.</summary>
    Task<(string Nombre, IReadOnlyList<(Guid ComponenteId, string Nombre, decimal Cantidad)> Componentes)?> ObtenerAsync(Guid productoId, CancellationToken ct = default);
}

/// <summary>Ejecuta el montaje real en Inventario (consume componentes y produce el compuesto).</summary>
public interface IMontajeProduccion
{
    Task<Resultado> MontarAsync(Guid empresaId, Guid productoId, decimal cantidad, Guid almacenId, CancellationToken ct = default);
}

// ---------------------------------------------------------------------------- Comandos
public sealed record CrearOrdenComando(Guid ProductoId, decimal Cantidad, Guid AlmacenId, DateOnly? Fecha = null);

// ---------------------------------------------------------------------------- Casos de uso
public sealed class CrearOrden
{
    private readonly IRepositorioOrdenes _ordenes;
    private readonly IConsultaListaMateriales _listaMateriales;
    private readonly IUnidadDeTrabajoProduccion _unidad;
    private readonly IReloj _reloj;

    public CrearOrden(IRepositorioOrdenes ordenes, IConsultaListaMateriales listaMateriales, IUnidadDeTrabajoProduccion unidad, IReloj reloj)
    {
        _ordenes = ordenes; _listaMateriales = listaMateriales; _unidad = unidad; _reloj = reloj;
    }

    public async Task<Resultado<OrdenFabricacionDto>> EjecutarAsync(Guid empresaId, CrearOrdenComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        if (comando.Cantidad <= 0m)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.Validacion("orden.cantidad", "La cantidad a fabricar debe ser mayor que cero."));
        }

        var bom = await _listaMateriales.ObtenerAsync(comando.ProductoId, ct).ConfigureAwait(false);
        if (bom is null)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.Validacion("orden.no_compuesto", "El artículo no existe o no tiene lista de materiales."));
        }

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var numero = await _ordenes.SiguienteNumeroAsync(empresaId, fecha.Year, ct).ConfigureAwait(false);
        var orden = OrdenFabricacion.Crear(empresaId, numero, comando.ProductoId, bom.Value.Nombre, comando.Cantidad, comando.AlmacenId, fecha,
            bom.Value.Componentes.Select(c => (c.ComponenteId, c.Nombre, c.Cantidad)).ToList(), _reloj);
        if (orden.EsFallo)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(orden.Error);
        }

        _ordenes.Agregar(orden.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(OrdenFabricacionDto.Desde(orden.Valor));
    }
}

public sealed class DecidirOrden
{
    private readonly IRepositorioOrdenes _ordenes;
    private readonly IMontajeProduccion _montaje;
    private readonly IUnidadDeTrabajoProduccion _unidad;
    private readonly IReloj _reloj;

    public DecidirOrden(IRepositorioOrdenes ordenes, IMontajeProduccion montaje, IUnidadDeTrabajoProduccion unidad, IReloj reloj)
    {
        _ordenes = ordenes; _montaje = montaje; _unidad = unidad; _reloj = reloj;
    }

    public Task<Resultado<OrdenFabricacionDto>> IniciarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
        => CambiarAsync(id, o => o.Iniciar(), ct);

    public Task<Resultado<OrdenFabricacionDto>> CancelarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
        => CambiarAsync(id, o => o.Cancelar(), ct);

    /// <summary>Termina la orden: consume componentes y produce el artículo (montaje), y marca la orden terminada.</summary>
    public async Task<Resultado<OrdenFabricacionDto>> TerminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var orden = await _ordenes.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (orden is null)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.NoEncontrado("orden.no_encontrada", "No se encontró la orden."));
        }

        if (orden.Estado is EstadoOrdenFabricacion.Terminada)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.Conflicto("orden.ya_terminada", "La orden ya está terminada."));
        }

        if (orden.Estado is EstadoOrdenFabricacion.Cancelada)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.Conflicto("orden.cancelada", "No se puede terminar una orden cancelada."));
        }

        var montaje = await _montaje.MontarAsync(empresaId, orden.ProductoId, orden.Cantidad, orden.AlmacenId, ct).ConfigureAwait(false);
        if (montaje.EsFallo)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(montaje.Error);
        }

        var terminar = orden.Terminar(_reloj.AhoraUtc);
        if (terminar.EsFallo)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(terminar.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(OrdenFabricacionDto.Desde(orden));
    }

    private async Task<Resultado<OrdenFabricacionDto>> CambiarAsync(Guid id, Func<OrdenFabricacion, Resultado> accion, CancellationToken ct)
    {
        var orden = await _ordenes.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (orden is null)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(Error.NoEncontrado("orden.no_encontrada", "No se encontró la orden."));
        }

        var r = accion(orden);
        if (r.EsFallo)
        {
            return Resultado.Fallo<OrdenFabricacionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(OrdenFabricacionDto.Desde(orden));
    }
}

public sealed class ListarOrdenes
{
    private readonly IRepositorioOrdenes _ordenes;
    public ListarOrdenes(IRepositorioOrdenes ordenes) => _ordenes = ordenes;
    public Task<IReadOnlyList<OrdenFabricacionDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) => _ordenes.ListarAsync(empresaId, ct);
}

public sealed class ObtenerOrden
{
    private readonly IRepositorioOrdenes _ordenes;
    public ObtenerOrden(IRepositorioOrdenes ordenes) => _ordenes = ordenes;
    public Task<OrdenFabricacionDto?> EjecutarAsync(Guid id, CancellationToken ct = default) => _ordenes.ObtenerDtoAsync(id, ct);
}
