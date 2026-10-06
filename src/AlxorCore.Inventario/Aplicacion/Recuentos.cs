using AlxorCore.Inventario.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Inventario.Aplicacion;

public interface IRepositorioRecuentos
{
    void Agregar(RecuentoInventario recuento);
    Task<RecuentoInventario?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<RecuentoInventario>> ListarAsync(Guid empresaId, CancellationToken ct = default);
    Task<int> ContarDelAnioAsync(Guid empresaId, int anio, CancellationToken ct = default);
}

public sealed record DatosRecuento(Guid AlmacenId, Guid? UbicacionId = null, DateOnly? Fecha = null, string? Descripcion = null,
    IReadOnlyList<Guid>? Productos = null);

/// <summary>Una cantidad contada: por artículo, ubicación y lote o número de serie. Cantidad null deja la línea sin contar.</summary>
public sealed record ConteoLinea(Guid ProductoId, decimal? Cantidad, Guid? UbicacionId = null, string? Lote = null);

public sealed record DatosConteo(IReadOnlyList<ConteoLinea>? Lineas);

public sealed record DatosCierreRecuento(bool NoContadosACero = false);

public sealed record LineaRecuentoDto(Guid Id, Guid ProductoId, Guid? UbicacionId, string? Ubicacion, string? Lote, decimal Teorico, decimal? Contado, decimal? Diferencia,
    bool Añadida);

public sealed record RecuentoDto(Guid Id, string Codigo, Guid AlmacenId, string? Almacen, Guid? UbicacionId, DateOnly Fecha, string? Descripcion, string Estado,
    DateTimeOffset AbiertoEn, DateTimeOffset? CerradoEn, int Lineas, int Contadas, int ConDiferencia, IReadOnlyList<LineaRecuentoDto>? Detalle = null);

/// <summary>
/// Inventario físico: abre un recuento con el stock teórico congelado del almacén (o de una ubicación, o de unos
/// artículos), anota lo contado y al cerrarlo regulariza cada diferencia con un ajuste sobre el stock de ese momento.
/// </summary>
public sealed class RecuentosInventario
{
    private readonly IRepositorioRecuentos _recuentos;
    private readonly IRepositorioExistencias _existencias;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioAlmacenes _almacenes;
    private readonly MovimientosInventario _inventario;
    private readonly IUnidadDeTrabajoInventario _unidad;
    private readonly IReloj _reloj;
    private readonly IAvisoExistencias? _aviso;

    public RecuentosInventario(IRepositorioRecuentos recuentos, IRepositorioExistencias existencias, IRepositorioMovimientos movimientos, IRepositorioAlmacenes almacenes,
        MovimientosInventario inventario, IUnidadDeTrabajoInventario unidad, IReloj reloj, IAvisoExistencias? aviso = null)
    {
        _recuentos = recuentos; _existencias = existencias; _movimientos = movimientos; _almacenes = almacenes; _inventario = inventario; _unidad = unidad;
        _reloj = reloj; _aviso = aviso;
    }

    public async Task<IReadOnlyList<RecuentoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var almacenes = (await _almacenes.ListarAsync(empresaId, ct).ConfigureAwait(false)).ToDictionary(a => a.Id, a => a.Nombre);
        return (await _recuentos.ListarAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(r => r.AbiertoEn)
            .Select(r => Dto(r, almacenes.GetValueOrDefault(r.AlmacenId), null)).ToList();
    }

    public async Task<RecuentoDto?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var r = await _recuentos.ObtenerAsync(id, ct).ConfigureAwait(false);
        return r is null ? null : await DetalleAsync(empresaId, r, ct).ConfigureAwait(false);
    }

    public async Task<Resultado<RecuentoDto>> AbrirAsync(Guid empresaId, DatosRecuento d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var almacen = await _almacenes.ObtenerAsync(d.AlmacenId, ct).ConfigureAwait(false);
        if (almacen is null || almacen.EmpresaId != empresaId)
        {
            return Resultado.Fallo<RecuentoDto>(Error.NoEncontrado("recuento.almacen", "El almacén no existe."));
        }

        if (d.UbicacionId is { } u && (await _almacenes.ObtenerUbicacionAsync(u, ct).ConfigureAwait(false))?.AlmacenId != d.AlmacenId)
        {
            return Resultado.Fallo<RecuentoDto>(Error.Validacion("recuento.ubicacion", "La ubicación no es de ese almacén."));
        }

        var abiertos = (await _recuentos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(r => r.Estado == EstadoRecuento.Abierto && r.AlmacenId == d.AlmacenId).ToList();
        if (abiertos.Any(r => r.UbicacionId is null || d.UbicacionId is null || r.UbicacionId == d.UbicacionId))
        {
            return Resultado.Fallo<RecuentoDto>(Error.Conflicto("recuento.abierto", "Ya hay un recuento abierto de ese almacén o ubicación: ciérralo o anúlalo antes."));
        }

        var productos = d.Productos is { Count: > 0 } p ? p.ToHashSet() : null;
        var teoricos = (await _existencias.ListarPorAlmacenAsync(empresaId, d.AlmacenId, ct).ConfigureAwait(false))
            .Where(e => (d.UbicacionId is null || e.UbicacionId == d.UbicacionId) && (productos is null || productos.Contains(e.ProductoId)) && e.Cantidad != 0m)
            .Select(e => (e.ProductoId, e.UbicacionId, e.Lote, e.Cantidad));
        var fecha = d.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var codigo = $"INV-{fecha.Year}-{await _recuentos.ContarDelAnioAsync(empresaId, fecha.Year, ct).ConfigureAwait(false) + 1:D4}";
        var r = RecuentoInventario.Abrir(empresaId, codigo, d.AlmacenId, d.UbicacionId, fecha, d.Descripcion, teoricos, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RecuentoDto>(r.Error);
        }

        _recuentos.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DetalleAsync(empresaId, r.Valor, ct).ConfigureAwait(false));
    }

    public async Task<Resultado<RecuentoDto>> ContarAsync(Guid empresaId, Guid id, DatosConteo d, CancellationToken ct = default)
    {
        var r = await _recuentos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<RecuentoDto>(NoEncontrado());
        }

        if (d?.Lineas is not { Count: > 0 } lineas)
        {
            return Resultado.Fallo<RecuentoDto>(Error.Validacion("recuento.sin_lineas", "Indica qué se ha contado."));
        }

        foreach (var l in lineas)
        {
            if (l.Cantidad is { } c && await _inventario.SeguimientoAsync(l.ProductoId, ct).ConfigureAwait(false) == SeguimientoStock.Serie
                && (string.IsNullOrWhiteSpace(l.Lote) || c is not (0m or 1m)))
            {
                return Resultado.Fallo<RecuentoDto>(Error.Validacion("recuento.serie", "Con número de serie se cuenta cada número: 1 si está, 0 si no."));
            }

            var x = r.Contar(l.ProductoId, l.UbicacionId, l.Lote, l.Cantidad);
            if (x.EsFallo)
            {
                return Resultado.Fallo<RecuentoDto>(x.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DetalleAsync(empresaId, r, ct).ConfigureAwait(false));
    }

    /// <summary>Cierra y regulariza: a cada stock de ahora se le suma su diferencia (contado − teórico al abrir).</summary>
    public async Task<Resultado<RecuentoDto>> CerrarAsync(Guid empresaId, Guid id, DatosCierreRecuento? d, CancellationToken ct = default)
    {
        var r = await _recuentos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo<RecuentoDto>(NoEncontrado());
        }

        var diferencias = r.Cerrar(d?.NoContadosACero ?? false, _reloj);
        if (diferencias.EsFallo)
        {
            return Resultado.Fallo<RecuentoDto>(diferencias.Error);
        }

        var productos = new HashSet<Guid>();
        foreach (var (linea, diferencia) in diferencias.Valor)
        {
            var e = await _existencias.ObtenerAsync(empresaId, linea.ProductoId, r.AlmacenId, linea.UbicacionId, linea.Lote, ct).ConfigureAwait(false);
            if (e is null)
            {
                e = Existencia.Nueva(empresaId, linea.ProductoId, r.AlmacenId, linea.UbicacionId, linea.Lote);
                _existencias.Agregar(e);
            }

            e.Fijar(e.Cantidad + diferencia);
            _movimientos.Agregar(MovimientoInventario.Registrar(empresaId, linea.ProductoId, r.AlmacenId, linea.UbicacionId, TipoMovimientoInventario.Ajuste, diferencia,
                r.Fecha, $"Recuento {r.Codigo}", r.Codigo, _reloj, linea.Lote));
            productos.Add(linea.ProductoId);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_aviso is not null && productos.Count > 0)
        {
            await _aviso.CambiaronAsync(empresaId, productos, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(await DetalleAsync(empresaId, r, ct).ConfigureAwait(false));
    }

    public async Task<Resultado> AnularAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _recuentos.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(NoEncontrado());
        }

        var a = r.Anular();
        if (a.EsFallo)
        {
            return a;
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<RecuentoDto> DetalleAsync(Guid empresaId, RecuentoInventario r, CancellationToken ct)
    {
        var almacen = (await _almacenes.ListarAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(a => a.Id == r.AlmacenId)?.Nombre;
        var ubicaciones = (await _almacenes.ListarUbicacionesAsync(empresaId, r.AlmacenId, ct).ConfigureAwait(false)).ToDictionary(u => u.Id, u => u.Codigo);
        var detalle = r.Lineas.OrderBy(l => l.UbicacionId.HasValue ? ubicaciones.GetValueOrDefault(l.UbicacionId.Value) : string.Empty, StringComparer.Ordinal)
            .ThenBy(l => l.ProductoId).ThenBy(l => l.Lote, StringComparer.Ordinal)
            .Select(l => new LineaRecuentoDto(l.Id, l.ProductoId, l.UbicacionId, l.UbicacionId is { } u ? ubicaciones.GetValueOrDefault(u) : null, l.Lote, l.Teorico,
                l.Contado, l.Diferencia ?? (l.Contado is { } c ? c - l.Teorico : null), l.Añadida)).ToList();
        return Dto(r, almacen, detalle);
    }

    private static RecuentoDto Dto(RecuentoInventario r, string? almacen, IReadOnlyList<LineaRecuentoDto>? detalle) =>
        new(r.Id, r.Codigo, r.AlmacenId, almacen, r.UbicacionId, r.Fecha, r.Descripcion, r.Estado.ToString(), r.AbiertoEn, r.CerradoEn, r.Lineas.Count,
            r.Lineas.Count(l => l.Contado is not null), r.Lineas.Count(l => (l.Diferencia ?? (l.Contado is { } c ? c - l.Teorico : 0m)) != 0m), detalle);

    private static Error NoEncontrado() => Error.NoEncontrado("recuento.no_encontrado", "El recuento no existe.");
}
