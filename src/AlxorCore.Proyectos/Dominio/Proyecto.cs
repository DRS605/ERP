using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Proyectos.Dominio;

/// <summary>Estado de un proyecto.</summary>
public enum EstadoProyecto
{
    /// <summary>Abierto: admite imputaciones.</summary>
    Abierto = 1,

    /// <summary>Cerrado: terminado, no admite más imputaciones (puede reabrirse).</summary>
    Cerrado = 2,

    /// <summary>Cancelado.</summary>
    Cancelado = 3,
}

/// <summary>Tipo de imputación a un proyecto.</summary>
public enum TipoImputacion
{
    /// <summary>Mano de obra: horas de una persona por su tarifa.</summary>
    ManoObra = 1,

    /// <summary>Material: cantidad de un artículo por su coste unitario (según el método de la empresa).</summary>
    Material = 2,

    /// <summary>Gasto directo (importe suelto).</summary>
    Gasto = 3,
}

/// <summary>
/// Línea de imputación de coste a un proyecto. Unifica los tres tipos: para mano de obra
/// <c>Cantidad</c> son horas y <c>CosteUnitario</c> la tarifa; para material, <c>Cantidad</c> son
/// unidades y <c>CosteUnitario</c> el valor unitario congelado; para un gasto, <c>Cantidad</c> = 1 y
/// <c>CosteUnitario</c> es el importe. En los tres casos <c>Importe = Cantidad × CosteUnitario</c>.
/// </summary>
public sealed class Imputacion
{
    private Imputacion() { Descripcion = null!; }

    internal Imputacion(Guid id, TipoImputacion tipo, DateOnly fecha, string descripcion, Guid? referenciaId,
        decimal cantidad, decimal costeUnitario, decimal importe)
    {
        Id = id;
        Tipo = tipo;
        Fecha = fecha;
        Descripcion = descripcion;
        ReferenciaId = referenciaId;
        Cantidad = cantidad;
        CosteUnitario = costeUnitario;
        Importe = importe;
    }

    public Guid Id { get; private set; }

    public TipoImputacion Tipo { get; private set; }

    public DateOnly Fecha { get; private set; }

    public string Descripcion { get; private set; }

    /// <summary>Persona (mano de obra) o artículo (material) al que se refiere; nulo para un gasto.</summary>
    public Guid? ReferenciaId { get; private set; }

    public decimal Cantidad { get; private set; }

    public decimal CosteUnitario { get; private set; }

    public decimal Importe { get; private set; }
}

/// <summary>
/// Proyecto sobre el que se imputan costes de mano de obra (personas × horas × tarifa), materiales
/// (valorados según el método de la empresa) y gastos directos, para compararlos con el presupuesto.
/// Multiempresa (RLS). Numeración correlativa por empresa y ejercicio.
/// </summary>
public sealed class Proyecto : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 200;
    public const int LongitudMaximaDescripcion = 300;
    private readonly List<Imputacion> _imputaciones = new();

    private Proyecto(Guid id) : base(id, Guid.Empty) { Nombre = null!; }

    private Proyecto(Guid id, Guid empresaId, int ejercicio, int numero, string nombre, Guid? clienteId,
        string? clienteNombre, decimal presupuesto, DateOnly fechaInicio, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Ejercicio = ejercicio;
        Numero = numero;
        Nombre = nombre;
        ClienteId = clienteId;
        ClienteNombre = clienteNombre;
        Presupuesto = presupuesto;
        FechaInicio = fechaInicio;
        Estado = EstadoProyecto.Abierto;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public int Ejercicio { get; private set; }

    public int Numero { get; private set; }

    public string Nombre { get; private set; }

    public Guid? ClienteId { get; private set; }

    /// <summary>Nombre del cliente en el momento de asociarlo (instantánea informativa).</summary>
    public string? ClienteNombre { get; private set; }

    /// <summary>Presupuesto (coste objetivo) del proyecto.</summary>
    public decimal Presupuesto { get; private set; }

    public DateOnly FechaInicio { get; private set; }

    public EstadoProyecto Estado { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public DateTimeOffset? CerradoEn { get; private set; }

    public IReadOnlyList<Imputacion> Imputaciones => _imputaciones;

    // Totales derivados (no se persisten).
    public decimal CosteManoObra => Suma(TipoImputacion.ManoObra);

    public decimal CosteMateriales => Suma(TipoImputacion.Material);

    public decimal CosteGastos => Suma(TipoImputacion.Gasto);

    public decimal CosteReal => Redondeo.Dos(_imputaciones.Sum(i => i.Importe));

    /// <summary>Presupuesto − coste real. Positivo = por debajo del presupuesto.</summary>
    public decimal Desviacion => Redondeo.Dos(Presupuesto - CosteReal);

    private decimal Suma(TipoImputacion tipo) => Redondeo.Dos(_imputaciones.Where(i => i.Tipo == tipo).Sum(i => i.Importe));

    public static Resultado<Proyecto> Crear(Guid empresaId, int numero, string? nombre, Guid? clienteId,
        string? clienteNombre, decimal presupuesto, DateOnly fechaInicio, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = ValidarDatos(nombre, presupuesto);
        if (error is not null)
        {
            return Resultado.Fallo<Proyecto>(error);
        }

        return Resultado.Ok(new Proyecto(Guid.NewGuid(), empresaId, fechaInicio.Year, numero, nombre!.Trim(),
            clienteId, Normalizar(clienteNombre), Redondeo.Dos(presupuesto), fechaInicio, reloj.AhoraUtc));
    }

    public Resultado ActualizarDatos(string? nombre, Guid? clienteId, string? clienteNombre, decimal presupuesto, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (Estado is EstadoProyecto.Cancelado)
        {
            return Resultado.Fallo(Error.Conflicto("proyecto.cancelado", "No se puede editar un proyecto cancelado."));
        }

        var error = ValidarDatos(nombre, presupuesto);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!.Trim();
        ClienteId = clienteId;
        ClienteNombre = Normalizar(clienteNombre);
        Presupuesto = Redondeo.Dos(presupuesto);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Imputa mano de obra: <paramref name="horas"/> de una persona a la tarifa indicada.</summary>
    public Resultado<Imputacion> ImputarManoObra(Guid personaId, string? nombrePersona, decimal horas, decimal tarifaHora, DateOnly fecha, IReloj reloj)
        => Imputar(TipoImputacion.ManoObra, personaId, nombrePersona, horas, tarifaHora, fecha, reloj, "horas");

    /// <summary>Imputa material: <paramref name="cantidad"/> de un artículo al coste unitario indicado.</summary>
    public Resultado<Imputacion> ImputarMaterial(Guid productoId, string? nombreArticulo, decimal cantidad, decimal costeUnitario, DateOnly fecha, IReloj reloj)
        => Imputar(TipoImputacion.Material, productoId, nombreArticulo, cantidad, costeUnitario, fecha, reloj, "cantidad");

    /// <summary>Imputa un gasto directo (importe suelto).</summary>
    public Resultado<Imputacion> ImputarGasto(string? concepto, decimal importe, DateOnly fecha, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (Estado is not EstadoProyecto.Abierto)
        {
            return Resultado.Fallo<Imputacion>(Error.Conflicto("proyecto.no_abierto", "Solo se imputan costes a un proyecto abierto."));
        }

        if (string.IsNullOrWhiteSpace(concepto))
        {
            return Resultado.Fallo<Imputacion>(Error.Validacion("imputacion.concepto", "El concepto del gasto es obligatorio."));
        }

        if (importe < 0m)
        {
            return Resultado.Fallo<Imputacion>(Error.Validacion("imputacion.importe", "El importe no puede ser negativo."));
        }

        var imp = new Imputacion(Guid.NewGuid(), TipoImputacion.Gasto, fecha, Recortar(concepto), null, 1m,
            Redondeo.Dos(importe), Redondeo.Dos(importe));
        _imputaciones.Add(imp);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok(imp);
    }

    public Resultado EliminarImputacion(Guid imputacionId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (Estado is not EstadoProyecto.Abierto)
        {
            return Resultado.Fallo(Error.Conflicto("proyecto.no_abierto", "Solo se editan las imputaciones de un proyecto abierto."));
        }

        var imp = _imputaciones.SingleOrDefault(i => i.Id == imputacionId);
        if (imp is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("imputacion.no_encontrada", "No se encontró la imputación."));
        }

        _imputaciones.Remove(imp);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Cerrar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado is EstadoProyecto.Cancelado)
        {
            return Resultado.Fallo(Error.Conflicto("proyecto.cancelado", "Un proyecto cancelado no se puede cerrar."));
        }

        Estado = EstadoProyecto.Cerrado;
        CerradoEn = reloj.AhoraUtc;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Reabrir(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (Estado is EstadoProyecto.Cancelado)
        {
            return Resultado.Fallo(Error.Conflicto("proyecto.cancelado", "Un proyecto cancelado no se puede reabrir."));
        }

        Estado = EstadoProyecto.Abierto;
        CerradoEn = null;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public Resultado Cancelar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Estado = EstadoProyecto.Cancelado;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private Resultado<Imputacion> Imputar(TipoImputacion tipo, Guid referenciaId, string? descripcion, decimal cantidad,
        decimal costeUnitario, DateOnly fecha, IReloj reloj, string nombreCantidad)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (Estado is not EstadoProyecto.Abierto)
        {
            return Resultado.Fallo<Imputacion>(Error.Conflicto("proyecto.no_abierto", "Solo se imputan costes a un proyecto abierto."));
        }

        if (cantidad <= 0m)
        {
            return Resultado.Fallo<Imputacion>(Error.Validacion("imputacion.cantidad", $"La {nombreCantidad} debe ser mayor que cero."));
        }

        if (costeUnitario < 0m)
        {
            return Resultado.Fallo<Imputacion>(Error.Validacion("imputacion.coste", "El coste unitario no puede ser negativo."));
        }

        var importe = Redondeo.Dos(cantidad * costeUnitario);
        var texto = string.IsNullOrWhiteSpace(descripcion) ? (tipo == TipoImputacion.ManoObra ? "Mano de obra" : "Material") : Recortar(descripcion);
        var imp = new Imputacion(Guid.NewGuid(), tipo, fecha, texto, referenciaId, cantidad, Redondeo.Dos(costeUnitario), importe);
        _imputaciones.Add(imp);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok(imp);
    }

    private static Error? ValidarDatos(string? nombre, decimal presupuesto)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("proyecto.nombre_vacio", "El nombre del proyecto es obligatorio.");
        }

        if (nombre.Trim().Length > LongitudMaximaNombre)
        {
            return Error.Validacion("proyecto.nombre_largo", "El nombre es demasiado largo.");
        }

        if (presupuesto < 0m)
        {
            return Error.Validacion("proyecto.presupuesto", "El presupuesto no puede ser negativo.");
        }

        return null;
    }

    private static string Recortar(string valor)
    {
        var v = valor.Trim();
        return v.Length > LongitudMaximaDescripcion ? v[..LongitudMaximaDescripcion] : v;
    }

    private static string? Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
