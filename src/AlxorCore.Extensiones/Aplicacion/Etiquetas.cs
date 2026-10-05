using AlxorCore.Extensiones.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Extensiones.Aplicacion;

public interface IRepositorioEtiquetas
{
    void Agregar(object entidad);

    void Eliminar(object entidad);

    Task<IReadOnlyList<PlantillaEtiqueta>> PlantillasAsync(Guid empresaId, CancellationToken ct = default);

    Task<PlantillaEtiqueta?> PlantillaAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<ReferenciaCliente>> ReferenciasAsync(Guid empresaId, Guid? clienteId, CancellationToken ct = default);

    Task<ReferenciaCliente?> ReferenciaAsync(Guid id, CancellationToken ct = default);
}

public sealed record DatosPlantillaEtiqueta(string? Nombre, Guid? ClienteId = null, string? Marca = null, IReadOnlyList<CampoEtiqueta>? Campos = null, string? TextoLibre = null,
    FormatoEtiqueta Formato = FormatoEtiqueta.A6, bool Activa = true);

public sealed record PlantillaEtiquetaDto(Guid Id, string Nombre, Guid? ClienteId, string? Marca, IReadOnlyList<string> Campos, string? TextoLibre, string Formato, bool Activa)
{
    public static PlantillaEtiquetaDto De(PlantillaEtiqueta p) =>
        new(p.Id, p.Nombre, p.ClienteId, p.Marca, p.ListaCampos.Select(c => c.ToString()).ToList(), p.TextoLibre, p.Formato.ToString(), p.Activa);
}

public sealed record DatosReferenciaCliente(Guid ClienteId, Guid ProductoId, string? Codigo, string? Descripcion = null, string? Gtin = null);

public sealed record ReferenciaClienteDto(Guid Id, Guid ClienteId, Guid ProductoId, string Codigo, string? Descripcion, string? Gtin)
{
    public static ReferenciaClienteDto De(ReferenciaCliente r) => new(r.Id, r.ClienteId, r.ProductoId, r.Codigo, r.Descripcion, r.Gtin);
}

/// <summary>Lo que hay que aplicar a la etiqueta de un palé para un cliente y un artículo.</summary>
public sealed record DisenoResuelto(string Plantilla, string? Marca, IReadOnlyList<string> Campos, string? TextoLibre, string Formato, string? ReferenciaCodigo,
    string? ReferenciaDescripcion, string? ReferenciaGtin);

/// <summary>Plantillas de etiqueta por cliente o plataforma y referencias de los artículos en cada cliente.</summary>
public sealed class DisenoEtiquetas
{
    private readonly IRepositorioEtiquetas _repo;
    private readonly IUnidadDeTrabajoExtensiones _unidad;

    public DisenoEtiquetas(IRepositorioEtiquetas repo, IUnidadDeTrabajoExtensiones unidad)
    {
        _repo = repo;
        _unidad = unidad;
    }

    public async Task<IReadOnlyList<PlantillaEtiquetaDto>> PlantillasAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.PlantillasAsync(empresaId, ct).ConfigureAwait(false)).OrderBy(p => p.ClienteId is not null).ThenBy(p => p.Nombre, StringComparer.CurrentCulture)
            .Select(PlantillaEtiquetaDto.De).ToList();

    public async Task<Resultado<PlantillaEtiquetaDto>> CrearPlantillaAsync(Guid empresaId, DatosPlantillaEtiqueta d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = PlantillaEtiqueta.Crear(empresaId, d.ClienteId, d.Nombre, d.Marca, d.Campos, d.TextoLibre, d.Formato);
        if (p.EsFallo)
        {
            return Resultado.Fallo<PlantillaEtiquetaDto>(p.Error);
        }

        if (await OtraActivaAsync(empresaId, p.Valor.ClienteId, null, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<PlantillaEtiquetaDto>(Error.Conflicto("etiqueta.duplicada",
                p.Valor.ClienteId is null ? "Ya hay una plantilla general activa." : "Ese cliente ya tiene una plantilla activa."));
        }

        _repo.Agregar(p.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlantillaEtiquetaDto.De(p.Valor));
    }

    public async Task<Resultado<PlantillaEtiquetaDto>> CambiarPlantillaAsync(Guid empresaId, Guid id, DatosPlantillaEtiqueta d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var p = await _repo.PlantillaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo<PlantillaEtiquetaDto>(Error.NoEncontrado("etiqueta.no_encontrada", "La plantilla no existe."));
        }

        if (d.Activa && await OtraActivaAsync(empresaId, p.ClienteId, id, ct).ConfigureAwait(false))
        {
            return Resultado.Fallo<PlantillaEtiquetaDto>(Error.Conflicto("etiqueta.duplicada", "Ya hay otra plantilla activa para ese cliente."));
        }

        var r = p.Cambiar(d.Nombre, d.Marca, d.Campos, d.TextoLibre, d.Formato, d.Activa);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PlantillaEtiquetaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(PlantillaEtiquetaDto.De(p));
    }

    public async Task<Resultado> EliminarPlantillaAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.PlantillaAsync(id, ct).ConfigureAwait(false);
        if (p is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("etiqueta.no_encontrada", "La plantilla no existe."));
        }

        _repo.Eliminar(p);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<IReadOnlyList<ReferenciaClienteDto>> ReferenciasAsync(Guid empresaId, Guid? clienteId, CancellationToken ct = default) =>
        (await _repo.ReferenciasAsync(empresaId, clienteId, ct).ConfigureAwait(false)).OrderBy(r => r.Codigo, StringComparer.Ordinal).Select(ReferenciaClienteDto.De).ToList();

    /// <summary>Crea o cambia la referencia de un artículo en un cliente.</summary>
    public async Task<Resultado<ReferenciaClienteDto>> FijarReferenciaAsync(Guid empresaId, DatosReferenciaCliente d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var r = (await _repo.ReferenciasAsync(empresaId, d.ClienteId, ct).ConfigureAwait(false)).FirstOrDefault(x => x.ProductoId == d.ProductoId);
        if (r is null)
        {
            var nueva = ReferenciaCliente.Crear(empresaId, d.ClienteId, d.ProductoId, d.Codigo, d.Descripcion, d.Gtin);
            if (nueva.EsFallo)
            {
                return Resultado.Fallo<ReferenciaClienteDto>(nueva.Error);
            }

            _repo.Agregar(nueva.Valor);
            r = nueva.Valor;
        }
        else if (r.Cambiar(d.Codigo, d.Descripcion, d.Gtin) is { EsFallo: true } c)
        {
            return Resultado.Fallo<ReferenciaClienteDto>(c.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ReferenciaClienteDto.De(r));
    }

    public async Task<Resultado> EliminarReferenciaAsync(Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ReferenciaAsync(id, ct).ConfigureAwait(false);
        if (r is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("referencia.no_encontrada", "La referencia no existe."));
        }

        _repo.Eliminar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>La plantilla del cliente (o la general) y la referencia del artículo en el cliente; null si no hay nada que aplicar.</summary>
    public async Task<DisenoResuelto?> ResolverAsync(Guid empresaId, Guid? clienteId, Guid? productoId, CancellationToken ct = default)
    {
        var plantillas = (await _repo.PlantillasAsync(empresaId, ct).ConfigureAwait(false)).Where(p => p.Activa).ToList();
        var plantilla = plantillas.FirstOrDefault(p => clienteId is not null && p.ClienteId == clienteId) ?? plantillas.FirstOrDefault(p => p.ClienteId is null);
        var referencia = clienteId is { } c && productoId is { } pr
            ? (await _repo.ReferenciasAsync(empresaId, c, ct).ConfigureAwait(false)).FirstOrDefault(r => r.ProductoId == pr)
            : null;
        if (plantilla is null && referencia is null)
        {
            return null;
        }

        var campos = (plantilla?.ListaCampos ?? PlantillaEtiqueta.CamposPorDefecto).ToList();
        if (referencia is not null && !campos.Contains(CampoEtiqueta.ReferenciaCliente))
        {
            campos.Insert(1, CampoEtiqueta.ReferenciaCliente);
        }

        return new DisenoResuelto(plantilla?.Nombre ?? "General", plantilla?.Marca, campos.Select(x => x.ToString()).ToList(), plantilla?.TextoLibre,
            (plantilla?.Formato ?? FormatoEtiqueta.A6).ToString(), referencia?.Codigo, referencia?.Descripcion, referencia?.Gtin);
    }

    private async Task<bool> OtraActivaAsync(Guid empresaId, Guid? clienteId, Guid? excepto, CancellationToken ct) =>
        (await _repo.PlantillasAsync(empresaId, ct).ConfigureAwait(false)).Any(p => p.Activa && p.ClienteId == clienteId && p.Id != excepto);
}
