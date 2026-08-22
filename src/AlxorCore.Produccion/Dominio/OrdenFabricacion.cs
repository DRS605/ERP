using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Produccion.Dominio;

/// <summary>Estado de una orden de fabricación.</summary>
public enum EstadoOrdenFabricacion
{
    /// <summary>Creada y a la espera de empezar.</summary>
    Planificada = 1,

    /// <summary>En curso (se está fabricando).</summary>
    EnCurso = 2,

    /// <summary>Terminada: se han consumido los componentes y producido el artículo.</summary>
    Terminada = 3,

    /// <summary>Anulada antes de terminar.</summary>
    Cancelada = 4,
}

/// <summary>
/// Componente planificado de una orden de fabricación: instantánea de la lista de materiales del
/// artículo en el momento de crear la orden (qué y cuánto se prevé consumir).
/// </summary>
public sealed class ComponentePlan
{
    private ComponentePlan() { Nombre = null!; }

    internal ComponentePlan(Guid id, Guid componenteId, string nombre, decimal cantidadUnitaria, decimal cantidadTotal)
    {
        Id = id;
        ComponenteId = componenteId;
        Nombre = nombre;
        CantidadUnitaria = cantidadUnitaria;
        CantidadTotal = cantidadTotal;
    }

    public Guid Id { get; private set; }

    public Guid ComponenteId { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Cantidad de componente por unidad del artículo fabricado.</summary>
    public decimal CantidadUnitaria { get; private set; }

    /// <summary>Cantidad total prevista (unitaria × cantidad de la orden).</summary>
    public decimal CantidadTotal { get; private set; }
}

/// <summary>
/// Orden de fabricación: fabricar una cantidad de un artículo compuesto. Al terminarla se consumen
/// del almacén los componentes de su lista de materiales y se da entrada del artículo fabricado.
/// Documento de producción apoyado en el escandallo del Catálogo.
/// </summary>
public sealed class OrdenFabricacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaTexto = 200;
    private readonly List<ComponentePlan> _componentes = new();

    private OrdenFabricacion(Guid id) : base(id, Guid.Empty) { ProductoNombre = null!; }

    private OrdenFabricacion(Guid id, Guid empresaId, int ejercicio, int numero, Guid productoId, string productoNombre,
        decimal cantidad, Guid almacenId, DateOnly fecha, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        ProductoId = productoId;
        ProductoNombre = productoNombre;
        Cantidad = cantidad;
        AlmacenId = almacenId;
        Fecha = fecha;
        Estado = EstadoOrdenFabricacion.Planificada;
        CreadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public Guid ProductoId { get; private set; }

    public string ProductoNombre { get; private set; }

    public decimal Cantidad { get; private set; }

    public Guid AlmacenId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public EstadoOrdenFabricacion Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset? TerminadaEn { get; private set; }

    public IReadOnlyList<ComponentePlan> Componentes => _componentes;

    public static Resultado<OrdenFabricacion> Crear(Guid empresaId, int numero, Guid productoId, string? productoNombre,
        decimal cantidad, Guid almacenId, DateOnly fecha,
        IReadOnlyList<(Guid ComponenteId, string Nombre, decimal CantidadUnitaria)> componentes, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ArgumentNullException.ThrowIfNull(componentes);

        if (string.IsNullOrWhiteSpace(productoNombre))
        {
            return Resultado.Fallo<OrdenFabricacion>(Error.Validacion("orden.producto_vacio", "El artículo a fabricar es obligatorio."));
        }

        if (cantidad <= 0m)
        {
            return Resultado.Fallo<OrdenFabricacion>(Error.Validacion("orden.cantidad", "La cantidad a fabricar debe ser mayor que cero."));
        }

        if (componentes.Count == 0)
        {
            return Resultado.Fallo<OrdenFabricacion>(Error.Validacion("orden.sin_componentes", "El artículo no tiene lista de materiales (no es un artículo compuesto)."));
        }

        var orden = new OrdenFabricacion(Guid.NewGuid(), empresaId, fecha.Year, numero, productoId, productoNombre.Trim(), cantidad, almacenId, fecha, reloj.AhoraUtc);
        foreach (var c in componentes)
        {
            var total = Math.Round(c.CantidadUnitaria * cantidad, 3, MidpointRounding.AwayFromZero);
            orden._componentes.Add(new ComponentePlan(Guid.NewGuid(), c.ComponenteId, c.Nombre?.Trim() ?? string.Empty, c.CantidadUnitaria, total));
        }

        return Resultado.Ok(orden);
    }

    public Resultado Iniciar()
    {
        if (Estado is not EstadoOrdenFabricacion.Planificada)
        {
            return Resultado.Fallo(Error.Conflicto("orden.estado", "Solo se inicia una orden planificada."));
        }

        Estado = EstadoOrdenFabricacion.EnCurso;
        return Resultado.Ok();
    }

    /// <summary>Marca la orden como terminada. La consumición/producción de stock la orquesta el caso de uso.</summary>
    public Resultado Terminar(DateTimeOffset ahora)
    {
        if (Estado is EstadoOrdenFabricacion.Terminada)
        {
            return Resultado.Fallo(Error.Conflicto("orden.ya_terminada", "La orden ya está terminada."));
        }

        if (Estado is EstadoOrdenFabricacion.Cancelada)
        {
            return Resultado.Fallo(Error.Conflicto("orden.cancelada", "No se puede terminar una orden cancelada."));
        }

        Estado = EstadoOrdenFabricacion.Terminada;
        TerminadaEn = ahora;
        return Resultado.Ok();
    }

    public Resultado Cancelar()
    {
        if (Estado is EstadoOrdenFabricacion.Terminada)
        {
            return Resultado.Fallo(Error.Conflicto("orden.terminada", "No se puede cancelar una orden terminada."));
        }

        Estado = EstadoOrdenFabricacion.Cancelada;
        return Resultado.Ok();
    }
}
