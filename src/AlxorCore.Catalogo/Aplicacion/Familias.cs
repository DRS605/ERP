using AlxorCore.Catalogo.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Aplicacion;

/// <summary>Vista plana de una familia, con su ruta completa y nivel calculados.</summary>
public sealed record FamiliaDto(
    Guid Id,
    string Nombre,
    string? Codigo,
    Guid? PadreId,
    bool Activo,
    string RutaCompleta,
    int Nivel);

/// <summary>Nodo del árbol de familias (una familia con sus subfamilias anidadas).</summary>
public sealed record FamiliaArbolDto(
    Guid Id,
    string Nombre,
    string? Codigo,
    bool Activo,
    IReadOnlyList<FamiliaArbolDto> Hijos);

/// <summary>Datos para crear o actualizar una familia.</summary>
public sealed record DatosFamilia(string Nombre, string? Codigo = null, Guid? PadreId = null);

/// <summary>Repositorio de familias (escritura y carga completa del árbol).</summary>
public interface IRepositorioFamilias
{
    Task<Familia?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Carga todas las familias de la empresa (para construir el árbol y detectar ciclos).</summary>
    Task<IReadOnlyList<Familia>> ListarTodasAsync(Guid grupoId, CancellationToken ct = default);

    void Agregar(Familia familia);

    void Eliminar(Familia familia);

    /// <summary>¿Hay artículos enlazados a esta familia? (para proteger el borrado).</summary>
    Task<bool> TieneArticulosAsync(Guid familiaId, CancellationToken ct = default);
}

/// <summary>Consulta de lectura de familias (la usan la API y otros casos de uso).</summary>
public interface IConsultaFamilias
{
    Task<FamiliaDto?> ObtenerAsync(Guid familiaId, CancellationToken ct = default);
}

/// <summary>
/// Aplica a un producto la familia indicada en <see cref="DatosProducto"/>: si trae
/// <c>FamiliaId</c>, la valida y sincroniza el texto de familia con su nombre; si no, usa el texto
/// libre <c>Familia</c> (compatibilidad con importaciones).
/// </summary>
internal static class ResolverFamilia
{
    public static async Task<Resultado> AplicarAsync(IConsultaFamilias familias, Producto producto, DatosProducto datos, CancellationToken ct)
    {
        if (datos.FamiliaId is Guid familiaId)
        {
            var familia = await familias.ObtenerAsync(familiaId, ct).ConfigureAwait(false);
            if (familia is null || !familia.Activo)
            {
                return Resultado.Fallo(Error.Validacion("producto.familia_no_valida", "La familia indicada no existe o está desactivada."));
            }

            producto.EstablecerFamiliaId(familiaId, familia.Nombre);
        }
        else
        {
            producto.EstablecerFamiliaId(null, null);
            producto.EstablecerFamilia(datos.Familia);
        }

        return Resultado.Ok();
    }
}

/// <summary>Utilidades para trabajar con el árbol de familias en memoria.</summary>
public static class ArbolFamilias
{
    /// <summary>Construye la ruta completa de una familia («Padre &gt; Hija &gt; Nieta») y su nivel.</summary>
    public static (string Ruta, int Nivel) RutaYNivel(Familia familia, IReadOnlyDictionary<Guid, Familia> porId)
    {
        var nombres = new List<string>();
        var actual = familia;
        var visitados = new HashSet<Guid>();
        while (actual is not null && visitados.Add(actual.Id))
        {
            nombres.Insert(0, actual.Nombre);
            actual = actual.PadreId is Guid p && porId.TryGetValue(p, out var padre) ? padre : null;
        }

        return (string.Join(" > ", nombres), nombres.Count - 1);
    }

    /// <summary>¿Es <paramref name="posibleAncestro"/> ancestro (o el mismo) de <paramref name="familiaId"/>?</summary>
    public static bool EsAncestro(Guid posibleAncestro, Guid familiaId, IReadOnlyDictionary<Guid, Familia> porId)
    {
        var actual = porId.TryGetValue(familiaId, out var f) ? f : null;
        var visitados = new HashSet<Guid>();
        while (actual is not null && visitados.Add(actual.Id))
        {
            if (actual.Id == posibleAncestro)
            {
                return true;
            }

            actual = actual.PadreId is Guid p && porId.TryGetValue(p, out var padre) ? padre : null;
        }

        return false;
    }
}

/// <summary>Caso de uso: crear una familia (o subfamilia si se indica padre).</summary>
public sealed class CrearFamilia
{
    private readonly IRepositorioFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public CrearFamilia(IRepositorioFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<FamiliaDto>> EjecutarAsync(Guid grupoId, DatosFamilia datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        if (datos.PadreId is Guid padreId)
        {
            var padre = await _familias.ObtenerPorIdAsync(padreId, ct).ConfigureAwait(false);
            if (padre is null)
            {
                return Resultado.Fallo<FamiliaDto>(Error.Validacion("familia.padre_no_encontrado", "La familia padre no existe."));
            }
        }

        var familia = Familia.Crear(grupoId, datos.Nombre, datos.PadreId, datos.Codigo, _reloj);
        if (familia.EsFallo)
        {
            return Resultado.Fallo<FamiliaDto>(familia.Error);
        }

        _familias.Agregar(familia.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await ComponerDtoAsync(grupoId, familia.Valor, ct).ConfigureAwait(false));
    }

    private async Task<FamiliaDto> ComponerDtoAsync(Guid grupoId, Familia familia, CancellationToken ct)
    {
        var todas = await _familias.ListarTodasAsync(grupoId, ct).ConfigureAwait(false);
        var porId = todas.ToDictionary(f => f.Id);
        var (ruta, nivel) = ArbolFamilias.RutaYNivel(familia, porId);
        return new FamiliaDto(familia.Id, familia.Nombre, familia.Codigo, familia.PadreId, familia.Activo, ruta, nivel);
    }
}

/// <summary>Caso de uso: actualizar una familia (nombre, código, padre y estado activo).</summary>
public sealed class ActualizarFamilia
{
    private readonly IRepositorioFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public ActualizarFamilia(IRepositorioFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo, IReloj reloj)
    {
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<FamiliaDto>> EjecutarAsync(Guid familiaId, DatosFamilia datos, bool activo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        var familia = await _familias.ObtenerPorIdAsync(familiaId, ct).ConfigureAwait(false);
        if (familia is null)
        {
            return Resultado.Fallo<FamiliaDto>(Error.NoEncontrado("familia.no_encontrada", "La familia no existe."));
        }

        var todas = await _familias.ListarTodasAsync(familia.GrupoId, ct).ConfigureAwait(false);
        var porId = todas.ToDictionary(f => f.Id);

        // Reubicar bajo un nuevo padre: no puede colgar de sí misma ni de un descendiente (ciclo).
        if (datos.PadreId != familia.PadreId)
        {
            if (datos.PadreId is Guid nuevoPadre)
            {
                if (!porId.TryGetValue(nuevoPadre, out _))
                {
                    return Resultado.Fallo<FamiliaDto>(Error.Validacion("familia.padre_no_encontrado", "La familia padre no existe."));
                }

                if (ArbolFamilias.EsAncestro(familiaId, nuevoPadre, porId))
                {
                    return Resultado.Fallo<FamiliaDto>(Error.Validacion("familia.ciclo", "No se puede mover una familia dentro de una de sus subfamilias."));
                }
            }

            var reubicar = familia.Reubicar(datos.PadreId, _reloj);
            if (reubicar.EsFallo)
            {
                return Resultado.Fallo<FamiliaDto>(reubicar.Error);
            }
        }

        var actualizar = familia.Actualizar(datos.Nombre, datos.Codigo, _reloj);
        if (actualizar.EsFallo)
        {
            return Resultado.Fallo<FamiliaDto>(actualizar.Error);
        }

        if (activo)
        {
            familia.Activar(_reloj);
        }
        else
        {
            familia.Desactivar(_reloj);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        porId[familia.Id] = familia;
        var (ruta, nivel) = ArbolFamilias.RutaYNivel(familia, porId);
        return Resultado.Ok(new FamiliaDto(familia.Id, familia.Nombre, familia.Codigo, familia.PadreId, familia.Activo, ruta, nivel));
    }
}

/// <summary>Caso de uso: eliminar una familia (solo si no tiene subfamilias ni artículos).</summary>
public sealed class EliminarFamilia
{
    private readonly IRepositorioFamilias _familias;
    private readonly IUnidadDeTrabajoCatalogo _unidadDeTrabajo;

    public EliminarFamilia(IRepositorioFamilias familias, IUnidadDeTrabajoCatalogo unidadDeTrabajo)
    {
        _familias = familias;
        _unidadDeTrabajo = unidadDeTrabajo;
    }

    public async Task<Resultado> EjecutarAsync(Guid familiaId, CancellationToken ct = default)
    {
        var familia = await _familias.ObtenerPorIdAsync(familiaId, ct).ConfigureAwait(false);
        if (familia is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("familia.no_encontrada", "La familia no existe."));
        }

        var todas = await _familias.ListarTodasAsync(familia.GrupoId, ct).ConfigureAwait(false);
        if (todas.Any(f => f.PadreId == familiaId))
        {
            return Resultado.Fallo(Error.Conflicto("familia.con_subfamilias", "No se puede eliminar: la familia tiene subfamilias. Muévelas o elimínalas primero."));
        }

        if (await _familias.TieneArticulosAsync(familiaId, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo(Error.Conflicto("familia.con_articulos", "No se puede eliminar: hay artículos en esta familia. Reasígnalos o desactiva la familia."));
        }

        _familias.Eliminar(familia);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }
}

/// <summary>Caso de uso: listar todas las familias en plano, ordenadas por su ruta.</summary>
public sealed class ListarFamilias
{
    private readonly IRepositorioFamilias _familias;

    public ListarFamilias(IRepositorioFamilias familias) => _familias = familias;

    public async Task<IReadOnlyList<FamiliaDto>> EjecutarAsync(Guid grupoId, CancellationToken ct = default)
    {
        var todas = await _familias.ListarTodasAsync(grupoId, ct).ConfigureAwait(false);
        var porId = todas.ToDictionary(f => f.Id);
        return todas
            .Select(f =>
            {
                var (ruta, nivel) = ArbolFamilias.RutaYNivel(f, porId);
                return new FamiliaDto(f.Id, f.Nombre, f.Codigo, f.PadreId, f.Activo, ruta, nivel);
            })
            .OrderBy(f => f.RutaCompleta, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}

/// <summary>Caso de uso: obtener el árbol de familias (raíces con sus subfamilias anidadas).</summary>
public sealed class ListarArbolFamilias
{
    private readonly IRepositorioFamilias _familias;

    public ListarArbolFamilias(IRepositorioFamilias familias) => _familias = familias;

    public async Task<IReadOnlyList<FamiliaArbolDto>> EjecutarAsync(Guid grupoId, CancellationToken ct = default)
    {
        var todas = await _familias.ListarTodasAsync(grupoId, ct).ConfigureAwait(false);
        var porPadre = todas.ToLookup(f => f.PadreId);

        List<FamiliaArbolDto> Hijos(Guid? padreId) =>
            porPadre[padreId]
                .OrderBy(f => f.Nombre, StringComparer.OrdinalIgnoreCase)
                .Select(f => new FamiliaArbolDto(f.Id, f.Nombre, f.Codigo, f.Activo, Hijos(f.Id)))
                .ToList();

        return Hijos(null);
    }
}
