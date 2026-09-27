using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Participación y método de consolidación de una empresa del grupo.</summary>
public sealed record PerimetroDto(Guid EmpresaId, string RazonSocial, decimal Porcentaje, MetodoConsolidacion Metodo);

/// <summary>Correspondencia de cuentas recíprocas entre dos empresas del grupo.</summary>
public sealed record CorrespondenciaDto(Guid Id, Guid EmpresaAId, string CuentaA, Guid EmpresaBId, string CuentaB, string Descripcion)
{
    public static CorrespondenciaDto De(CorrespondenciaCuentas c) => new(c.Id, c.EmpresaAId, c.CuentaA, c.EmpresaBId, c.CuentaB, c.Descripcion);
}

/// <summary>Datos de una correspondencia de cuentas.</summary>
public sealed record DatosCorrespondencia(Guid EmpresaAId, string? CuentaA, Guid EmpresaBId, string? CuentaB, string? Descripcion = null);

/// <summary>Datos del perímetro de una empresa.</summary>
public sealed record DatosPerimetro(decimal Porcentaje, MetodoConsolidacion Metodo);

/// <summary>Repositorio de la configuración de la consolidación del grupo.</summary>
public interface IRepositorioConsolidacion
{
    Task<IReadOnlyList<PerimetroConsolidacion>> PerimetroAsync(Guid grupoId, CancellationToken ct = default);

    Task<IReadOnlyList<CorrespondenciaCuentas>> CorrespondenciasAsync(Guid grupoId, CancellationToken ct = default);

    Task<CorrespondenciaCuentas?> CorrespondenciaAsync(Guid id, CancellationToken ct = default);

    void Agregar(object entidad);

    void Eliminar(object entidad);
}

/// <summary>
/// Configuración de la consolidación del grupo: la participación y el método de cada empresa (el perímetro) y las
/// correspondencias de cuentas recíprocas entre empresas.
/// </summary>
public sealed class ConfiguracionConsolidacion
{
    private readonly IRepositorioConsolidacion _repo;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoOrganizacion _unidad;
    private readonly IReloj _reloj;

    public ConfiguracionConsolidacion(IRepositorioConsolidacion repo, IConsultaEmpresas empresas, IUnidadDeTrabajoOrganizacion unidad, IReloj reloj)
    {
        _repo = repo;
        _empresas = empresas;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>El perímetro de todas las empresas del grupo (las que no tienen registro, global al 100 %).</summary>
    public async Task<IReadOnlyList<PerimetroDto>> PerimetroAsync(Guid grupoId, CancellationToken ct = default)
    {
        var fijado = (await _repo.PerimetroAsync(grupoId, ct).ConfigureAwait(false)).ToDictionary(p => p.EmpresaId);
        return (await _empresas.EmpresasDelGrupoAsync(grupoId, ct).ConfigureAwait(false))
            .Select(e => fijado.TryGetValue(e.Id, out var p)
                ? new PerimetroDto(e.Id, e.RazonSocial, p.Porcentaje, p.Metodo)
                : new PerimetroDto(e.Id, e.RazonSocial, 100m, MetodoConsolidacion.Global))
            .ToList();
    }

    public async Task<Resultado<PerimetroDto>> FijarPerimetroAsync(Guid grupoId, Guid empresaId, DatosPerimetro datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var empresa = (await _empresas.EmpresasDelGrupoAsync(grupoId, ct).ConfigureAwait(false)).FirstOrDefault(e => e.Id == empresaId);
        if (empresa is null)
        {
            return Resultado.Fallo<PerimetroDto>(Error.NoEncontrado("consolidacion.empresa", "La empresa no es de este grupo."));
        }

        var p = (await _repo.PerimetroAsync(grupoId, ct).ConfigureAwait(false)).FirstOrDefault(x => x.EmpresaId == empresaId);
        if (p is null)
        {
            p = PerimetroConsolidacion.Crear(grupoId, empresaId);
            _repo.Agregar(p);
        }

        var r = p.Fijar(datos.Porcentaje, datos.Metodo, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<PerimetroDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new PerimetroDto(empresaId, empresa.RazonSocial, p.Porcentaje, p.Metodo));
    }

    public async Task<IReadOnlyList<CorrespondenciaDto>> CorrespondenciasAsync(Guid grupoId, CancellationToken ct = default) =>
        (await _repo.CorrespondenciasAsync(grupoId, ct).ConfigureAwait(false)).Select(CorrespondenciaDto.De).ToList();

    public async Task<Resultado<CorrespondenciaDto>> CrearCorrespondenciaAsync(Guid grupoId, DatosCorrespondencia datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        if (await EmpresasInvalidasAsync(grupoId, datos, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<CorrespondenciaDto>(error);
        }

        var c = CorrespondenciaCuentas.Crear(grupoId, datos.EmpresaAId, datos.CuentaA, datos.EmpresaBId, datos.CuentaB, datos.Descripcion);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CorrespondenciaDto>(c.Error);
        }

        _repo.Agregar(c.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CorrespondenciaDto.De(c.Valor));
    }

    public async Task<Resultado<CorrespondenciaDto>> ActualizarCorrespondenciaAsync(Guid grupoId, Guid id, DatosCorrespondencia datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var c = await _repo.CorrespondenciaAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CorrespondenciaDto>(Error.NoEncontrado("correspondencia.no_encontrada", "La correspondencia no existe."));
        }

        if (await EmpresasInvalidasAsync(grupoId, datos, ct).ConfigureAwait(false) is { } error)
        {
            return Resultado.Fallo<CorrespondenciaDto>(error);
        }

        var r = c.Fijar(datos.EmpresaAId, datos.CuentaA, datos.EmpresaBId, datos.CuentaB, datos.Descripcion);
        if (r.EsFallo)
        {
            return Resultado.Fallo<CorrespondenciaDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(CorrespondenciaDto.De(c));
    }

    public async Task<Resultado> EliminarCorrespondenciaAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _repo.CorrespondenciaAsync(id, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("correspondencia.no_encontrada", "La correspondencia no existe."));
        }

        _repo.Eliminar(c);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<Error?> EmpresasInvalidasAsync(Guid grupoId, DatosCorrespondencia datos, CancellationToken ct)
    {
        var ids = (await _empresas.EmpresasDelGrupoAsync(grupoId, ct).ConfigureAwait(false)).Select(e => e.Id).ToHashSet();
        return ids.Contains(datos.EmpresaAId) && ids.Contains(datos.EmpresaBId)
            ? null
            : Error.Validacion("correspondencia.empresas", "Las dos empresas tienen que ser del grupo.");
    }
}
