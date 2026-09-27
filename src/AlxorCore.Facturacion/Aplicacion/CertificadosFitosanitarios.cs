using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Aplicacion;

public sealed record CertificadoFitosanitarioDto(Guid Id, Guid CartaPorteId, string Tipo, string Numero, DateOnly FechaEmision, string? PaisDestino, string? Organismo,
    string? Mercancia, string? Observaciones, string? DocumentoNombre, bool TieneDocumento)
{
    public static CertificadoFitosanitarioDto Desde(CertificadoFitosanitario c) => new(c.Id, c.CartaPorteId, c.Tipo.ToString(), c.Numero, c.FechaEmision, c.PaisDestino,
        c.Organismo, c.Mercancia, c.Observaciones, c.DocumentoNombre, c.Documento is not null);
}

/// <summary>Datos de un certificado. El documento va en base64 (PDF, JPEG o PNG); sin él, se conserva el que hubiera.</summary>
public sealed record DatosCertificadoFitosanitario(TipoCertificadoFitosanitario Tipo, string? Numero, DateOnly FechaEmision, string? PaisDestino = null,
    string? Organismo = null, string? Mercancia = null, string? Observaciones = null, string? DocumentoNombre = null, string? DocumentoTipo = null,
    string? DocumentoBase64 = null, bool QuitarDocumento = false);

public interface IRepositorioCertificadosFitosanitarios
{
    Task<IReadOnlyList<CertificadoFitosanitario>> DeCartaAsync(Guid cartaPorteId, CancellationToken ct = default);

    Task<IReadOnlyList<(Guid CartaPorteId, string Tipo, string Numero)>> NumerosAsync(IReadOnlyCollection<Guid> cartas, CancellationToken ct = default);

    Task<CertificadoFitosanitario?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(CertificadoFitosanitario certificado);

    void Eliminar(CertificadoFitosanitario certificado);
}

/// <summary>Certificados fitosanitarios de las expediciones (una carta de porte puede llevar varios).</summary>
public sealed class CertificadosFitosanitarios
{
    private readonly IRepositorioCertificadosFitosanitarios _repo;
    private readonly IRepositorioCartasPorte _cartas;
    private readonly IUnidadDeTrabajoFacturacion _unidad;

    public CertificadosFitosanitarios(IRepositorioCertificadosFitosanitarios repo, IRepositorioCartasPorte cartas, IUnidadDeTrabajoFacturacion unidad)
    {
        _repo = repo; _cartas = cartas; _unidad = unidad;
    }

    public async Task<IReadOnlyList<CertificadoFitosanitarioDto>> DeCartaAsync(Guid cartaPorteId, CancellationToken ct = default) =>
        (await _repo.DeCartaAsync(cartaPorteId, ct).ConfigureAwait(false)).OrderBy(c => c.FechaEmision).Select(CertificadoFitosanitarioDto.Desde).ToList();

    public async Task<Resultado<CertificadoFitosanitarioDto>> RegistrarAsync(Guid empresaId, Guid cartaPorteId, DatosCertificadoFitosanitario d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var carta = await _cartas.ObtenerPorIdAsync(cartaPorteId, ct).ConfigureAwait(false);
        if (carta is null || carta.EmpresaId != empresaId)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(Error.NoEncontrado("cartaporte.no_encontrada", "La carta de porte no existe."));
        }

        if (carta.AnuladaEn is not null)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(Error.Conflicto("certificado.carta_anulada", "La carta de porte está anulada."));
        }

        var c = CertificadoFitosanitario.Crear(empresaId, cartaPorteId, d.Tipo, d.Numero, d.FechaEmision, d.PaisDestino ?? carta.PaisDestino, d.Organismo, d.Mercancia, d.Observaciones);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(c.Error);
        }

        if (Adjuntar(c.Valor, d) is { } error)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(error);
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CertificadoFitosanitarioDto.Desde(c.Valor));
    }

    public async Task<Resultado<CertificadoFitosanitarioDto>> ModificarAsync(Guid empresaId, Guid id, DatosCertificadoFitosanitario d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } c || c.EmpresaId != empresaId)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(Error.NoEncontrado("certificado.no_encontrado", "El certificado no existe."));
        }

        var r = c.Modificar(d.Tipo, d.Numero, d.FechaEmision, d.PaisDestino, d.Organismo, d.Mercancia, d.Observaciones);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(r.Error);
        }

        if (Adjuntar(c, d) is { } error)
        {
            return Resultado.Fallo<CertificadoFitosanitarioDto>(error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CertificadoFitosanitarioDto.Desde(c));
    }

    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } c || c.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("certificado.no_encontrado", "El certificado no existe."));
        }

        _repo.Eliminar(c);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    public async Task<Resultado<(byte[] Contenido, string Tipo, string Nombre)>> DocumentoAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is not { } c || c.EmpresaId != empresaId || c.Documento is null)
        {
            return Resultado.Fallo<(byte[], string, string)>(Error.NoEncontrado("certificado.sin_documento", "El certificado no tiene documento."));
        }

        return Resultado.Ok((c.Documento, c.DocumentoTipo ?? "application/octet-stream", c.DocumentoNombre ?? "certificado"));
    }

    private static Error? Adjuntar(CertificadoFitosanitario c, DatosCertificadoFitosanitario d)
    {
        if (d.QuitarDocumento)
        {
            c.Adjuntar(null, null, null);
            return null;
        }

        if (string.IsNullOrWhiteSpace(d.DocumentoBase64))
        {
            return null;
        }

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(d.DocumentoBase64);
        }
        catch (FormatException)
        {
            return Error.Validacion("certificado.documento", "El documento no es base64 válido.");
        }

        var r = c.Adjuntar(d.DocumentoNombre, d.DocumentoTipo, bytes);
        return r.EsFallo ? r.Error : null;
    }
}
