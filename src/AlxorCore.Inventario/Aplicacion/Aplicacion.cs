using AlxorCore.Inventario.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Inventario.Aplicacion;

// ---------------------------------------------------------------------------- DTOs
public sealed record AlmacenDto(Guid Id, string Codigo, string Nombre, bool Activo)
{
    public static AlmacenDto Desde(Almacen a) => new(a.Id, a.Codigo, a.Nombre, a.Activo);
}

public sealed record UbicacionDto(Guid Id, Guid AlmacenId, string Codigo, string Nombre)
{
    public static UbicacionDto Desde(Ubicacion u) => new(u.Id, u.AlmacenId, u.Codigo, u.Nombre);
}

public sealed record ExistenciaDto(Guid ProductoId, Guid AlmacenId, string AlmacenNombre, Guid? UbicacionId, string? UbicacionCodigo, decimal Cantidad);

public sealed record MovimientoDto(Guid Id, Guid ProductoId, Guid AlmacenId, Guid? UbicacionId, string Tipo, decimal Cantidad, DateOnly Fecha, string? Motivo, string? Referencia);

public sealed record UbicacionDefectoDto(Guid Id, Guid ProductoId, Guid AlmacenId, string AlmacenNombre, Guid? ProveedorId, Guid UbicacionId, string UbicacionCodigo);

// ---------------------------------------------------------------------------- Puertos
public interface IRepositorioAlmacenes
{
    void Agregar(Almacen almacen);
    Task<Almacen?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AlmacenDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    void AgregarUbicacion(Ubicacion ubicacion);
    Task<IReadOnlyList<UbicacionDto>> ListarUbicacionesAsync(Guid empresaId, Guid? almacenId = null, CancellationToken ct = default);
}

public interface IRepositorioExistencias
{
    Task<Existencia?> ObtenerAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, CancellationToken ct = default);
    void Agregar(Existencia existencia);
    Task<IReadOnlyList<ExistenciaDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);
    Task<IReadOnlyList<ExistenciaDto>> ListarPorAlmacenAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default);
}

public interface IRepositorioMovimientos
{
    void Agregar(MovimientoInventario movimiento);
    Task<IReadOnlyList<MovimientoDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);
}

public interface IRepositorioUbicacionesDefecto
{
    void Agregar(UbicacionDefecto regla);
    Task<UbicacionDefecto?> ObtenerAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, CancellationToken ct = default);
    Task<IReadOnlyList<UbicacionDefectoDto>> ListarPorProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);
}

public interface IUnidadDeTrabajoInventario : IUnidadDeTrabajo;

// ---------------------------------------------------------------------------- Comandos
public sealed record CrearAlmacenComando(string Codigo, string Nombre);
public sealed record CrearUbicacionComando(Guid AlmacenId, string Codigo, string? Nombre = null);
public sealed record MovimientoComando(Guid ProductoId, Guid AlmacenId, decimal Cantidad, Guid? UbicacionId = null, DateOnly? Fecha = null, string? Motivo = null, string? Referencia = null);
public sealed record TraspasoComando(Guid ProductoId, decimal Cantidad, Guid AlmacenOrigenId, Guid AlmacenDestinoId, Guid? UbicacionOrigenId = null, Guid? UbicacionDestinoId = null, DateOnly? Fecha = null);
public sealed record UbicacionDefectoComando(Guid ProductoId, Guid AlmacenId, Guid UbicacionId, Guid? ProveedorId = null);

// ---------------------------------------------------------------------------- Almacenes y ubicaciones
public sealed class GestionAlmacenes
{
    private readonly IRepositorioAlmacenes _repo;
    private readonly IUnidadDeTrabajoInventario _unidad;

    public GestionAlmacenes(IRepositorioAlmacenes repo, IUnidadDeTrabajoInventario unidad) { _repo = repo; _unidad = unidad; }

    public async Task<Resultado<AlmacenDto>> CrearAlmacenAsync(Guid empresaId, CrearAlmacenComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var almacen = Almacen.Crear(empresaId, comando.Codigo, comando.Nombre);
        if (almacen.EsFallo)
        {
            return Resultado.Fallo<AlmacenDto>(almacen.Error);
        }

        _repo.Agregar(almacen.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(AlmacenDto.Desde(almacen.Valor));
    }

    public Task<IReadOnlyList<AlmacenDto>> ListarAlmacenesAsync(Guid empresaId, CancellationToken ct = default) => _repo.ListarAsync(empresaId, ct);

    public async Task<Resultado<UbicacionDto>> CrearUbicacionAsync(Guid empresaId, CrearUbicacionComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var almacen = await _repo.ObtenerAsync(comando.AlmacenId, ct).ConfigureAwait(false);
        if (almacen is null)
        {
            return Resultado.Fallo<UbicacionDto>(Error.NoEncontrado("almacen.no_encontrado", "No se encontró el almacén."));
        }

        var ubicacion = Ubicacion.Crear(empresaId, comando.AlmacenId, comando.Codigo, comando.Nombre);
        if (ubicacion.EsFallo)
        {
            return Resultado.Fallo<UbicacionDto>(ubicacion.Error);
        }

        _repo.AgregarUbicacion(ubicacion.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(UbicacionDto.Desde(ubicacion.Valor));
    }

    public Task<IReadOnlyList<UbicacionDto>> ListarUbicacionesAsync(Guid empresaId, Guid? almacenId, CancellationToken ct = default) => _repo.ListarUbicacionesAsync(empresaId, almacenId, ct);
}

// ---------------------------------------------------------------------------- Movimientos de stock
public sealed class MovimientosInventario
{
    private readonly IRepositorioExistencias _existencias;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoInventario _unidad;
    private readonly IReloj _reloj;

    public MovimientosInventario(IRepositorioExistencias existencias, IRepositorioMovimientos movimientos, IUnidadDeTrabajoInventario unidad, IReloj reloj)
    {
        _existencias = existencias; _movimientos = movimientos; _unidad = unidad; _reloj = reloj;
    }

    private async Task<Existencia> ObtenerOCrearAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? ubicacionId, CancellationToken ct)
    {
        var e = await _existencias.ObtenerAsync(empresaId, productoId, almacenId, ubicacionId, ct).ConfigureAwait(false);
        if (e is null)
        {
            e = Existencia.Nueva(empresaId, productoId, almacenId, ubicacionId);
            _existencias.Agregar(e);
        }

        return e;
    }

    public async Task<Resultado<ExistenciaDto>> EntradaAsync(Guid empresaId, MovimientoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Cantidad <= 0m)
        {
            return Resultado.Fallo<ExistenciaDto>(Error.Validacion("inventario.cantidad_invalida", "La cantidad debe ser mayor que cero."));
        }

        var e = await ObtenerOCrearAsync(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId, ct).ConfigureAwait(false);
        e.Aumentar(c.Cantidad);
        _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId,
            TipoMovimientoInventario.Entrada, c.Cantidad, c.Fecha ?? Hoy(), c.Motivo, c.Referencia, _reloj));
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ExistenciaDto(e.ProductoId, e.AlmacenId, string.Empty, e.UbicacionId, null, e.Cantidad));
    }

    public async Task<Resultado<ExistenciaDto>> SalidaAsync(Guid empresaId, MovimientoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Cantidad <= 0m)
        {
            return Resultado.Fallo<ExistenciaDto>(Error.Validacion("inventario.cantidad_invalida", "La cantidad debe ser mayor que cero."));
        }

        var e = await _existencias.ObtenerAsync(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId, ct).ConfigureAwait(false);
        if (e is null)
        {
            return Resultado.Fallo<ExistenciaDto>(Error.Validacion("existencia.insuficiente", "No hay stock de ese artículo en el almacén."));
        }

        var dis = e.Disminuir(c.Cantidad);
        if (dis.EsFallo)
        {
            return Resultado.Fallo<ExistenciaDto>(dis.Error);
        }

        _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId,
            TipoMovimientoInventario.Salida, -c.Cantidad, c.Fecha ?? Hoy(), c.Motivo, c.Referencia, _reloj));
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ExistenciaDto(e.ProductoId, e.AlmacenId, string.Empty, e.UbicacionId, null, e.Cantidad));
    }

    /// <summary>Ajuste por recuento: fija la cantidad contada y registra el movimiento por la diferencia.</summary>
    public async Task<Resultado<ExistenciaDto>> AjustarAsync(Guid empresaId, MovimientoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Cantidad < 0m)
        {
            return Resultado.Fallo<ExistenciaDto>(Error.Validacion("inventario.cantidad_invalida", "La cantidad contada no puede ser negativa."));
        }

        var e = await ObtenerOCrearAsync(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId, ct).ConfigureAwait(false);
        var diferencia = Math.Round(c.Cantidad - e.Cantidad, 3, MidpointRounding.AwayFromZero);
        e.Fijar(c.Cantidad);
        if (diferencia != 0m)
        {
            _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, c.ProductoId, c.AlmacenId, c.UbicacionId,
                TipoMovimientoInventario.Ajuste, diferencia, c.Fecha ?? Hoy(), c.Motivo ?? "Recuento", c.Referencia, _reloj));
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ExistenciaDto(e.ProductoId, e.AlmacenId, string.Empty, e.UbicacionId, null, e.Cantidad));
    }

    public async Task<Resultado> TraspasarAsync(Guid empresaId, TraspasoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        if (c.Cantidad <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("inventario.cantidad_invalida", "La cantidad debe ser mayor que cero."));
        }

        var origen = await _existencias.ObtenerAsync(empresaId, c.ProductoId, c.AlmacenOrigenId, c.UbicacionOrigenId, ct).ConfigureAwait(false);
        if (origen is null)
        {
            return Resultado.Fallo(Error.Validacion("existencia.insuficiente", "No hay stock en el origen."));
        }

        var dis = origen.Disminuir(c.Cantidad);
        if (dis.EsFallo)
        {
            return Resultado.Fallo(dis.Error);
        }

        var destino = await ObtenerOCrearAsync(empresaId, c.ProductoId, c.AlmacenDestinoId, c.UbicacionDestinoId, ct).ConfigureAwait(false);
        destino.Aumentar(c.Cantidad);

        var fecha = c.Fecha ?? Hoy();
        _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, c.ProductoId, c.AlmacenOrigenId, c.UbicacionOrigenId, TipoMovimientoInventario.TraspasoSalida, -c.Cantidad, fecha, "Traspaso", null, _reloj));
        _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, c.ProductoId, c.AlmacenDestinoId, c.UbicacionDestinoId, TipoMovimientoInventario.TraspasoEntrada, c.Cantidad, fecha, "Traspaso", null, _reloj));
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private DateOnly Hoy() => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
}

public sealed class ConsultasInventario
{
    private readonly IRepositorioExistencias _existencias;
    private readonly IRepositorioMovimientos _movimientos;

    public ConsultasInventario(IRepositorioExistencias existencias, IRepositorioMovimientos movimientos)
    {
        _existencias = existencias; _movimientos = movimientos;
    }

    public Task<IReadOnlyList<ExistenciaDto>> StockDeProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) => _existencias.ListarPorProductoAsync(empresaId, productoId, ct);
    public Task<IReadOnlyList<ExistenciaDto>> StockDeAlmacenAsync(Guid empresaId, Guid almacenId, CancellationToken ct = default) => _existencias.ListarPorAlmacenAsync(empresaId, almacenId, ct);
    public Task<IReadOnlyList<MovimientoDto>> MovimientosDeProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) => _movimientos.ListarPorProductoAsync(empresaId, productoId, ct);
}

// ---------------------------------------------------------------------------- Ubicación por defecto
public sealed class UbicacionesPorDefecto
{
    private readonly IRepositorioUbicacionesDefecto _repo;
    private readonly IUnidadDeTrabajoInventario _unidad;

    public UbicacionesPorDefecto(IRepositorioUbicacionesDefecto repo, IUnidadDeTrabajoInventario unidad) { _repo = repo; _unidad = unidad; }

    public async Task<Resultado> FijarAsync(Guid empresaId, UbicacionDefectoComando c, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(c);
        var existente = await _repo.ObtenerAsync(empresaId, c.ProductoId, c.AlmacenId, c.ProveedorId, ct).ConfigureAwait(false);
        if (existente is null)
        {
            _repo.Agregar(new UbicacionDefecto(empresaId, c.ProductoId, c.AlmacenId, c.ProveedorId, c.UbicacionId));
        }
        else
        {
            existente.CambiarUbicacion(c.UbicacionId);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public Task<IReadOnlyList<UbicacionDefectoDto>> ListarAsync(Guid empresaId, Guid productoId, CancellationToken ct = default) => _repo.ListarPorProductoAsync(empresaId, productoId, ct);

    /// <summary>Resuelve la ubicación por defecto: primero la regla del proveedor y, si no, la general del almacén.</summary>
    public async Task<Guid?> ResolverAsync(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, CancellationToken ct = default)
    {
        if (proveedorId is not null)
        {
            var especifica = await _repo.ObtenerAsync(empresaId, productoId, almacenId, proveedorId, ct).ConfigureAwait(false);
            if (especifica is not null)
            {
                return especifica.UbicacionId;
            }
        }

        var general = await _repo.ObtenerAsync(empresaId, productoId, almacenId, null, ct).ConfigureAwait(false);
        return general?.UbicacionId;
    }
}
