using AlxorCore.Agro.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Agro.Aplicacion;

public sealed record DatosCertificado(Guid AgricultorId, Certificaciones Tipo, string? Codigo, DateOnly Desde, DateOnly? Hasta = null, Guid? ParcelaId = null,
    string? Organismo = null);

public sealed record CertificadoDto(Guid Id, Guid AgricultorId, Guid? ParcelaId, string Tipo, string Codigo, string? Organismo, DateOnly Desde, DateOnly? Hasta, bool Baja,
    bool VigenteHoy);

public sealed record DatosDeclaracion(Certificaciones Exige);

public sealed record DeclaracionDto(Guid ProductoId, string Exige, string Texto);

public sealed record DatosDescalificacion(Certificaciones Quitar, string? Motivo);

public sealed record DescalificacionDto(Guid Id, Guid PartidaId, string Quitadas, string Motivo, string? DocumentoTipo, Guid? DocumentoId, Guid? UsuarioId, DateTimeOffset En);

/// <summary>
/// Certificaciones agrícolas: los certificados de agricultores y parcelas (ecológico, GlobalG.A.P., GRASP) con su vigencia,
/// cómo se vende cada artículo y la descalificación explícita de una partida (con motivo, registrada).
/// </summary>
public sealed class CertificacionesAgro
{
    private readonly IRepositorioAgro _repo;
    private readonly IUnidadDeTrabajoAgro _unidad;
    private readonly IReloj _reloj;

    public CertificacionesAgro(IRepositorioAgro repo, IUnidadDeTrabajoAgro unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    private DateOnly Hoy => DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);

    public async Task<IReadOnlyList<CertificadoDto>> CertificadosAsync(Guid empresaId, Guid? agricultorId, CancellationToken ct = default) =>
        (await _repo.CertificadosAsync(empresaId, agricultorId, ct).ConfigureAwait(false)).Select(Dto).ToList();

    public async Task<Resultado<CertificadoDto>> CrearCertificadoAsync(Guid empresaId, DatosCertificado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var comprobado = await ComprobarTitularAsync(empresaId, d.AgricultorId, d.ParcelaId, ct).ConfigureAwait(false);
        if (comprobado.EsFallo)
        {
            return Resultado.Fallo<CertificadoDto>(comprobado.Error);
        }

        var c = CertificadoAgro.Crear(empresaId, d.AgricultorId, d.ParcelaId, d.Tipo, d.Codigo, d.Organismo, d.Desde, d.Hasta);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CertificadoDto>(c.Error);
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(c.Valor));
    }

    /// <summary>Cambia los datos y la vigencia. No cambia lo ya recibido: la partida guarda las certificaciones de su día.</summary>
    public async Task<Resultado<CertificadoDto>> ActualizarCertificadoAsync(Guid id, DatosCertificado d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var c = await _repo.CertificadoAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CertificadoDto>(NoExiste());
        }

        var r = c.Actualizar(d.Tipo, d.Codigo, d.Organismo, d.Desde, d.Hasta);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CertificadoDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(c));
    }

    /// <summary>Da de baja el certificado (retirado o suspendido): se conserva para la historia de lo recibido.</summary>
    public async Task<Resultado<CertificadoDto>> BajaCertificadoAsync(Guid id, bool baja, CancellationToken ct = default)
    {
        var c = await _repo.CertificadoAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CertificadoDto>(NoExiste());
        }

        c.DarDeBaja(baja);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(c));
    }

    public async Task<IReadOnlyList<DeclaracionDto>> DeclaracionesAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.DeclaracionesAsync(empresaId, ct).ConfigureAwait(false)).Select(d => new DeclaracionDto(d.ProductoId, d.Exige.ToString(), ReglasCertificacion.Texto(d.Exige)))
        .ToList();

    /// <summary>Declara cómo se vende el artículo (Ninguna = convencional). Rige para lo que se reciba o confeccione a partir de ahora.</summary>
    public async Task<Resultado<DeclaracionDto>> DeclararAsync(Guid empresaId, Guid productoId, DatosDeclaracion d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if ((d.Exige & ~ReglasCertificacion.Todas) != Certificaciones.Ninguna)
        {
            return Resultado.Fallo<DeclaracionDto>(Error.Validacion("certificacion.tipo", "Certificación desconocida."));
        }

        var declaracion = await _repo.DeclaracionAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (declaracion is null)
        {
            declaracion = DeclaracionArticulo.Crear(empresaId, productoId, d.Exige);
            _repo.Agregar(declaracion);
        }
        else
        {
            declaracion.Cambiar(d.Exige);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new DeclaracionDto(productoId, d.Exige.ToString(), ReglasCertificacion.Texto(d.Exige)));
    }

    /// <summary>Quita la declaración: el artículo deja de comprobarse.</summary>
    public async Task<Resultado> QuitarDeclaracionAsync(Guid empresaId, Guid productoId, CancellationToken ct = default)
    {
        var declaracion = await _repo.DeclaracionAsync(empresaId, productoId, ct).ConfigureAwait(false);
        if (declaracion is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("certificacion.sin_declaracion", "El artículo no tiene declaración."));
        }

        _repo.Eliminar(declaracion);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>Descalifica una partida (por ejemplo, pierde el ecológico por una retirada del certificado): con motivo y registrada.</summary>
    public async Task<Resultado<IReadOnlyList<DescalificacionDto>>> DescalificarAsync(Guid partidaId, DatosDescalificacion d, Guid? usuarioId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var partida = await _repo.PartidaAsync(partidaId, ct).ConfigureAwait(false);
        if (partida is null)
        {
            return Resultado.Fallo<IReadOnlyList<DescalificacionDto>>(Error.NoEncontrado("partida.no_encontrada", "La partida no existe."));
        }

        var r = partida.Descalificar(d.Quitar, d.Motivo, "Manual", null, usuarioId, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<DescalificacionDto>>(r.Error);
        }

        _repo.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(await DescalificacionesAsync(partidaId, ct).ConfigureAwait(false));
    }

    public async Task<IReadOnlyList<DescalificacionDto>> DescalificacionesAsync(Guid partidaId, CancellationToken ct = default) =>
        (await _repo.DescalificacionesAsync([partidaId], ct).ConfigureAwait(false)).OrderBy(x => x.En)
        .Select(x => new DescalificacionDto(x.Id, x.PartidaId, x.Quitadas.ToString(), x.Motivo, x.DocumentoTipo, x.DocumentoId, x.UsuarioId, x.En)).ToList();

    private async Task<Resultado> ComprobarTitularAsync(Guid empresaId, Guid agricultorId, Guid? parcelaId, CancellationToken ct)
    {
        var agricultor = await _repo.AgricultorAsync(agricultorId, ct).ConfigureAwait(false);
        if (agricultor is null || agricultor.EmpresaId != empresaId)
        {
            return Resultado.Fallo(Error.NoEncontrado("agricultor.no_encontrado", "El agricultor no existe."));
        }

        if (parcelaId is { } pid && (await _repo.ParcelaAsync(pid, ct).ConfigureAwait(false))?.AgricultorId != agricultorId)
        {
            return Resultado.Fallo(Error.Validacion("certificado.parcela", "La parcela no es de ese agricultor."));
        }

        return Resultado.Ok();
    }

    private CertificadoDto Dto(CertificadoAgro c) => new(c.Id, c.AgricultorId, c.ParcelaId, c.Tipo.ToString(), c.Codigo, c.Organismo, c.Desde, c.Hasta, c.Baja, c.VigenteEl(Hoy));

    private static Error NoExiste() => Error.NoEncontrado("certificado.no_encontrado", "El certificado no existe.");
}
