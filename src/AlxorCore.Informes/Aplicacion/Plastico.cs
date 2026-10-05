using AlxorCore.Informes.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Informes.Aplicacion;

public interface IRepositorioFichasPlastico
{
    void Agregar(FichaPlastico ficha);

    void Eliminar(FichaPlastico ficha);

    Task<IReadOnlyList<FichaPlastico>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<FichaPlastico?> DeProductoAsync(Guid empresaId, Guid productoId, CancellationToken ct = default);

    Task GuardarAsync(CancellationToken ct = default);
}

public sealed record DatosFichaPlastico(ClavePlastico Clave, decimal KgPorUnidad, decimal KgRecicladoPorUnidad = 0m, bool Exento = false, string? MotivoExencion = null);

public sealed record FichaPlasticoDto(Guid Id, Guid ProductoId, string Clave, decimal KgPorUnidad, decimal KgRecicladoPorUnidad, decimal KgNoRecicladoPorUnidad, bool Exento,
    string? MotivoExencion)
{
    public static FichaPlasticoDto De(FichaPlastico f) =>
        new(f.Id, f.ProductoId, f.Clave.ToString(), f.KgPorUnidad, f.KgRecicladoPorUnidad, f.KgNoRecicladoPorUnidad, f.Exento, f.MotivoExencion);
}

/// <summary>Fichas de plástico de los artículos (impuesto especial sobre los envases de plástico no reutilizables).</summary>
public sealed class FichasPlastico
{
    private readonly IRepositorioFichasPlastico _repo;

    public FichasPlastico(IRepositorioFichasPlastico repo) => _repo = repo;

    public async Task<IReadOnlyList<FichaPlasticoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(FichaPlasticoDto.De).ToList();

    /// <summary>Crea o cambia la ficha de un artículo.</summary>
    public async Task<Resultado<FichaPlasticoDto>> FijarAsync(Guid empresaId, Guid productoId, DatosFichaPlastico d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var f = await _repo.DeProductoAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (f is null)
        {
            var nueva = FichaPlastico.Crear(empresaId, productoId, d.Clave, d.KgPorUnidad, d.KgRecicladoPorUnidad, d.Exento, d.MotivoExencion);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<FichaPlasticoDto>(nueva.Error);
            }

            _repo.Agregar(nueva.Valor);
            f = nueva.Valor;
        }
        else if (f.Cambiar(d.Clave, d.KgPorUnidad, d.KgRecicladoPorUnidad, d.Exento, d.MotivoExencion) is { EsFallo: true } r)
        {
            return Resultado.Fallo<FichaPlasticoDto>(r.Error);
        }

        await _repo.GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FichaPlasticoDto.De(f));
    }

    public async Task<Resultado> QuitarAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var f = await _repo.DeProductoAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (f is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("plastico.no_encontrada", "El artículo no tiene ficha de plástico."));
        }

        _repo.Eliminar(f);
        await _repo.GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}
