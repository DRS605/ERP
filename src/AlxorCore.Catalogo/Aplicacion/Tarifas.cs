using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>Vista de una línea de tarifa.</summary>
public sealed record LineaTarifaDto(
    Guid? ProductoId, Guid? FamiliaId, decimal CantidadMinima, decimal? Precio, decimal PorcentajeDescuento, DateOnly? Desde, DateOnly? Hasta);

/// <summary>Vista de una tarifa de precios.</summary>
public sealed record TarifaDto(Guid Id, string Codigo, string Nombre, bool Activa, IReadOnlyList<LineaTarifaDto> Lineas)
{
    public static TarifaDto Desde(Tarifa t) => new(t.Id, t.Codigo, t.Nombre, t.Activa,
        t.Lineas.Select(l => new LineaTarifaDto(l.ProductoId, l.FamiliaId, l.CantidadMinima, l.Precio, l.PorcentajeDescuento, l.Desde, l.Hasta)).ToList());
}

/// <summary>Datos para crear una tarifa.</summary>
public sealed record CrearTarifaComando(string Codigo, string Nombre, IReadOnlyList<DatosLineaTarifa>? Lineas);

/// <summary>Datos para actualizar una tarifa (sustituye todas sus líneas).</summary>
public sealed record ActualizarTarifaComando(string Nombre, bool Activa, IReadOnlyList<DatosLineaTarifa>? Lineas);

/// <summary>Precio de venta resuelto para una línea, con el motivo (tarifa y línea aplicadas, o precio del producto).</summary>
public sealed record PrecioVentaDto(decimal PrecioUnitario, decimal PorcentajeDescuento, string Origen);

/// <summary>Repositorio de tarifas.</summary>
public interface IRepositorioTarifas
{
    Task<Tarifa?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);

    Task<IReadOnlyList<Tarifa>> ListarAsync(CancellationToken ct = default);

    void Agregar(Tarifa tarifa);

    void Eliminar(Tarifa tarifa);
}

/// <summary>
/// Puerto de precio de venta con tarifa. Lo usan Facturación (al emitir sin precio) y la API (consulta
/// de precio para la interfaz). Devuelve null si no hay tarifa o ninguna línea aplica: en ese caso se
/// usa el precio del producto sin descuento.
/// </summary>
public interface IResolverPrecioVenta
{
    Task<PrecioVentaDto?> ResolverAsync(Guid? tarifaId, Guid productoId, decimal cantidad, DateOnly fecha, CancellationToken ct = default);
}

/// <summary>Validaciones comunes de las líneas: los productos y familias deben existir.</summary>
internal static class ValidarReferenciasTarifa
{
    public static async Task<Error?> ComprobarAsync(
        IReadOnlyList<DatosLineaTarifa>? lineas, IConsultaProductos productos, IRepositorioFamilias familias, CancellationToken ct)
    {
        foreach (var (l, i) in (lineas ?? []).Select((l, i) => (l, i + 1)))
        {
            if (l.ProductoId is { } p && await productos.ObtenerAsync(p, ct).ConfigureAwait(false) is null)
            {
                return Error.Validacion("tarifa.producto_no_encontrado", $"El producto de la línea {i} no existe.");
            }

            if (l.FamiliaId is { } f && await familias.ObtenerPorIdAsync(f, ct).ConfigureAwait(false) is null)
            {
                return Error.Validacion("tarifa.familia_no_encontrada", $"La familia de la línea {i} no existe.");
            }
        }

        return null;
    }
}

/// <summary>Caso de uso: crear una tarifa de precios.</summary>
public sealed class CrearTarifa
{
    private readonly IRepositorioTarifas _tarifas;
    private readonly IConsultaProductos _productos;
    private readonly IRepositorioFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearTarifa(IRepositorioTarifas tarifas, IConsultaProductos productos, IRepositorioFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _tarifas = tarifas;
        _productos = productos;
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<TarifaDto>> EjecutarAsync(Guid grupoId, CrearTarifaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var tarifa = Tarifa.Crear(grupoId, comando.Codigo, comando.Nombre, comando.Lineas, _reloj);
        if (tarifa.EsFallo)
        {
            return Resultado.Fallo<TarifaDto>(tarifa.Error);
        }

        if (await _tarifas.ExisteCodigoAsync(tarifa.Valor.Codigo, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<TarifaDto>(Error.Conflicto("tarifa.codigo_duplicado", $"Ya existe una tarifa con el código {tarifa.Valor.Codigo}."));
        }

        if (await ValidarReferenciasTarifa.ComprobarAsync(comando.Lineas, _productos, _familias, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<TarifaDto>(error);
        }

        _tarifas.Agregar(tarifa.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TarifaDto.Desde(tarifa.Valor));
    }
}

/// <summary>Caso de uso: actualizar una tarifa (nombre, estado y líneas).</summary>
public sealed class ActualizarTarifa
{
    private readonly IRepositorioTarifas _tarifas;
    private readonly IConsultaProductos _productos;
    private readonly IRepositorioFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarTarifa(IRepositorioTarifas tarifas, IConsultaProductos productos, IRepositorioFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _tarifas = tarifas;
        _productos = productos;
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<TarifaDto>> EjecutarAsync(Guid id, ActualizarTarifaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var tarifa = await _tarifas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        if (tarifa is null)
        {
            return Resultado.Fallo<TarifaDto>(Error.NoEncontrado("tarifa.no_encontrada", "La tarifa no existe."));
        }

        if (await ValidarReferenciasTarifa.ComprobarAsync(comando.Lineas, _productos, _familias, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<TarifaDto>(error);
        }

        var r = tarifa.Actualizar(comando.Nombre, comando.Activa, comando.Lineas, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<TarifaDto>(r.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(TarifaDto.Desde(tarifa));
    }
}

/// <summary>Caso de uso: listar y consultar tarifas.</summary>
public sealed class ConsultarTarifas
{
    private readonly IRepositorioTarifas _tarifas;

    public ConsultarTarifas(IRepositorioTarifas tarifas) => _tarifas = tarifas;

    public async Task<IReadOnlyList<TarifaDto>> ListarAsync(CancellationToken ct = default) =>
        (await _tarifas.ListarAsync(ct).ConfigureAwait(false)).Select(TarifaDto.Desde).ToList();

    public async Task<Resultado<TarifaDto>> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _tarifas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false) is { } t
            ? Resultado.Ok(TarifaDto.Desde(t))
            : Resultado.Fallo<TarifaDto>(Error.NoEncontrado("tarifa.no_encontrada", "La tarifa no existe."));
}

/// <summary>
/// Implementación del puerto de precio de venta: carga la tarifa, el producto y la cadena de familias
/// del producto (de la más cercana a la raíz) y aplica la regla de la tarifa.
/// </summary>
public sealed class ResolverPrecioVenta : IResolverPrecioVenta
{
    private readonly IRepositorioTarifas _tarifas;
    private readonly IConsultaProductos _productos;
    private readonly IRepositorioFamilias _familias;

    public ResolverPrecioVenta(IRepositorioTarifas tarifas, IConsultaProductos productos, IRepositorioFamilias familias)
    {
        _tarifas = tarifas;
        _productos = productos;
        _familias = familias;
    }

    public async Task<PrecioVentaDto?> ResolverAsync(Guid? tarifaId, Guid productoId, decimal cantidad, DateOnly fecha, CancellationToken ct = default)
    {
        if (tarifaId is not { } id)
        {
            return null;
        }

        var tarifa = await _tarifas.ObtenerPorIdAsync(id, ct).ConfigureAwait(false);
        var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
        if (tarifa is null || producto is null)
        {
            return null;
        }

        var cadena = new List<Guid>();
        if (producto.FamiliaId is { } familiaId)
        {
            var porId = (await _familias.ListarTodasAsync(tarifa.GrupoId, ct).ConfigureAwait(false)).ToDictionary(f => f.Id);
            for (Guid? f = familiaId; f is { } actual && porId.TryGetValue(actual, out var fam) && !cadena.Contains(actual); f = fam.PadreId)
            {
                cadena.Add(actual);
            }
        }

        var precio = tarifa.Resolver(new SolicitudPrecio(productoId, cadena, producto.PrecioUnitario, cantidad, fecha));
        return precio is null ? null : new PrecioVentaDto(precio.PrecioUnitario, precio.PorcentajeDescuento, precio.Origen);
    }
}
