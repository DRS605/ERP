using AlxorCore.Inventario.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Inventario.Aplicacion;

public interface IRepositorioReglasReaprovisionamiento
{
    void Agregar(ReglaReaprovisionamiento regla);
    void Eliminar(ReglaReaprovisionamiento regla);
    Task<ReglaReaprovisionamiento?> ObtenerAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ReglaReaprovisionamiento>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

public sealed record DatosReglaReaprovisionamiento(Guid ProductoId, decimal Minimo, decimal Maximo, Guid? AlmacenId = null, decimal Multiplo = 0m, Guid? ProveedorId = null,
    bool Activa = true);

public sealed record ReglaReaprovisionamientoDto(Guid Id, Guid ProductoId, Guid? AlmacenId, decimal Minimo, decimal Maximo, decimal Multiplo, Guid? ProveedorId, bool Activa)
{
    public static ReglaReaprovisionamientoDto De(ReglaReaprovisionamiento r) => new(r.Id, r.ProductoId, r.AlmacenId, r.Minimo, r.Maximo, r.Multiplo, r.ProveedorId, r.Activa);
}

/// <summary>Reglas de stock mínimo y máximo de los artículos (una por artículo y almacén, o una para todos los almacenes).</summary>
public sealed class ReglasReaprovisionamiento
{
    private readonly IRepositorioReglasReaprovisionamiento _repo;
    private readonly IRepositorioAlmacenes _almacenes;
    private readonly IUnidadDeTrabajoInventario _unidad;

    public ReglasReaprovisionamiento(IRepositorioReglasReaprovisionamiento repo, IRepositorioAlmacenes almacenes, IUnidadDeTrabajoInventario unidad)
    {
        _repo = repo; _almacenes = almacenes; _unidad = unidad;
    }

    public async Task<IReadOnlyList<ReglaReaprovisionamientoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(ReglaReaprovisionamientoDto.De).ToList();

    public async Task<Resultado<ReglaReaprovisionamientoDto>> FijarAsync(Guid empresaId, DatosReglaReaprovisionamiento d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.AlmacenId is { } a && (await _almacenes.ObtenerAsync(a, ct).ConfigureAwait(false))?.EmpresaId != empresaId)
        {
            return Resultado.Fallo<ReglaReaprovisionamientoDto>(Error.NoEncontrado("reaprovisionamiento.almacen", "El almacén no existe."));
        }

        var existente = (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).FirstOrDefault(r => r.ProductoId == d.ProductoId && r.AlmacenId == d.AlmacenId);
        if (existente is not null)
        {
            var c = existente.Cambiar(d.Minimo, d.Maximo, d.Multiplo, d.ProveedorId, d.Activa);
            if (c.EsFallo)
            {
                return Resultado.Fallo<ReglaReaprovisionamientoDto>(c.Error);
            }
        }
        else
        {
            var n = ReglaReaprovisionamiento.Crear(empresaId, d.ProductoId, d.AlmacenId, d.Minimo, d.Maximo, d.Multiplo, d.ProveedorId);
            if (n.EsFallo)
            {
                return Resultado.Fallo<ReglaReaprovisionamientoDto>(n.Error);
            }

            existente = n.Valor;
            _repo.Agregar(existente);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReglaReaprovisionamientoDto.De(existente));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("reaprovisionamiento.no_encontrada", "La regla no existe."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
