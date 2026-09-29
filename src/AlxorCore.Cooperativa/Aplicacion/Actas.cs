using AlxorCore.Cooperativa.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Cooperativa.Aplicacion;

public sealed record ActaDto(Guid Id, OrganoSocial Organo, int? Numero, DateOnly Fecha, CaracterSesion Caracter, string? Lugar, int Presentes, int Representados,
    string? Presidente, string? Secretario, string OrdenDelDia, string Acuerdos, EstadoActa Estado, DateTimeOffset? AprobadaEn)
{
    public static ActaDto De(Acta a) => new(a.Id, a.Organo, a.Numero, a.Fecha, a.Caracter, a.Lugar, a.Presentes, a.Representados, a.Presidente, a.Secretario, a.OrdenDelDia,
        a.Acuerdos, a.Estado, a.AprobadaEn);
}

public sealed record DatosNuevaActa(OrganoSocial Organo, DatosActa Acta);

/// <summary>Libros de actas de la asamblea general y del órgano de gobierno: el acta se redacta en borrador y, al aprobarla, recibe su número y queda fija.</summary>
public sealed class ActasCooperativa
{
    private readonly IRepositorioCooperativa _repo;
    private readonly IUnidadDeTrabajoCooperativa _unidad;
    private readonly IReloj _reloj;

    public ActasCooperativa(IRepositorioCooperativa repo, IUnidadDeTrabajoCooperativa unidad, IReloj reloj)
    {
        _repo = repo;
        _unidad = unidad;
        _reloj = reloj;
    }

    public async Task<IReadOnlyList<ActaDto>> ListarAsync(Guid empresaId, OrganoSocial? organo, CancellationToken ct = default) =>
        (await _repo.ActasAsync(empresaId, ct).ConfigureAwait(false)).Where(a => organo is null || a.Organo == organo)
        .OrderBy(a => a.Organo).ThenBy(a => a.Numero is null).ThenBy(a => a.Numero).ThenBy(a => a.Fecha).Select(ActaDto.De).ToList();

    public async Task<ActaDto?> ObtenerAsync(Guid id, CancellationToken ct = default) =>
        await _repo.ActaAsync(id, ct).ConfigureAwait(false) is { } a ? ActaDto.De(a) : null;

    public async Task<Resultado<ActaDto>> CrearAsync(Guid empresaId, DatosNuevaActa d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var a = Acta.Crear(empresaId, d.Organo, d.Acta);
        if (a.EsFallo)
        {
            return Resultado.Fallo<ActaDto>(a.Error);
        }

        _repo.Agregar(a.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ActaDto.De(a.Valor));
    }

    public async Task<Resultado<ActaDto>> CambiarAsync(Guid id, DatosActa d, CancellationToken ct = default)
    {
        var a = await _repo.ActaAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<ActaDto>(NoExiste());
        }

        var r = a.Cambiar(d);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ActaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ActaDto.De(a));
    }

    /// <summary>Aprueba el acta: recibe el siguiente número del libro de su órgano y ya no cambia.</summary>
    public async Task<Resultado<ActaDto>> AprobarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.ActaAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo<ActaDto>(NoExiste());
        }

        await _unidad.BloquearAsync($"cooperativa.acta.{a.EmpresaId}.{a.Organo}", ct).ConfigureAwait(false);
        var numero = await _repo.UltimoNumeroActaAsync(a.EmpresaId, a.Organo, ct).ConfigureAwait(false) + 1;
        var r = a.Aprobar(numero, _reloj.AhoraUtc);
        if (r.EsFallo)
        {
            return Resultado.Fallo<ActaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(ActaDto.De(a));
    }

    public async Task<Resultado> EliminarAsync(Guid id, CancellationToken ct = default)
    {
        var a = await _repo.ActaAsync(id, ct).ConfigureAwait(false);
        if (a is null)
        {
            return Resultado.Fallo(NoExiste());
        }

        if (a.Estado != EstadoActa.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("acta.aprobada", "El acta ya está aprobada: forma parte del libro y no se elimina."));
        }

        _repo.Eliminar(a);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private static Error NoExiste() => Error.NoEncontrado("acta.no_encontrada", "El acta no existe.");
}
