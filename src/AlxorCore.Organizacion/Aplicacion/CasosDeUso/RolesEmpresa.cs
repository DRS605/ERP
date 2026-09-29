using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Organizacion.Dominio;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>Un rol de la empresa (fijo o propio) con sus permisos y cuántos miembros activos lo tienen.</summary>
public sealed record RolDto(Guid? Id, string Codigo, string Nombre, string? Descripcion, bool Propio, IReadOnlyList<string> Permisos, int Miembros);

public sealed record DatosRol(string? Nombre, string? Descripcion, IReadOnlyList<string>? Permisos, string? Plantilla = null);

/// <summary>
/// Roles de una empresa: los tres fijos (Propietario, Usuario, Solo lectura) y los propios, por puesto de trabajo
/// (Báscula, Jefe de almacén…), creados a partir de una plantilla o permiso a permiso.
/// </summary>
public sealed class RolesEmpresa
{
    private readonly IRepositorioRolesEmpresa _roles;
    private readonly IRepositorioMembresias _membresias;
    private readonly IUnidadDeTrabajoOrganizacion _unidad;
    private readonly IReloj _reloj;

    public RolesEmpresa(IRepositorioRolesEmpresa roles, IRepositorioMembresias membresias, IUnidadDeTrabajoOrganizacion unidad, IReloj reloj)
    {
        _roles = roles;
        _membresias = membresias;
        _unidad = unidad;
        _reloj = reloj;
    }

    /// <summary>El rol de un código en la empresa: uno fijo o uno propio de esa empresa.</summary>
    public static async Task<Resultado<Rol>> ResolverAsync(IRepositorioRolesEmpresa roles, Guid empresaId, string? codigo, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(roles);
        if (!Rol.EsPropio(codigo))
        {
            return Rol.PorCodigoRol(codigo);
        }

        var propio = Guid.TryParseExact(codigo![Rol.PrefijoPropio.Length..], "N", out var id) ? await roles.ObtenerAsync(id, ct).ConfigureAwait(false) : null;
        return propio is null || propio.EmpresaId != empresaId
            ? Resultado.Fallo<Rol>(Error.Validacion("rol.desconocido", $"El rol «{codigo}» no existe en esta empresa."))
            : Resultado.Ok(propio.ComoRol());
    }

    public async Task<IReadOnlyList<RolDto>> ListarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var activos = (await _membresias.ListarPorEmpresaAsync(empresaId, ct).ConfigureAwait(false)).Where(m => m.EstaActiva).GroupBy(m => m.RolCodigo)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.Ordinal);
        var fijos = Rol.Todos.Select(r => new RolDto(null, r.Codigo, r.Nombre, null, false, r.PermisosConcedidos.OrderBy(p => p, StringComparer.Ordinal).ToList(),
            activos.GetValueOrDefault(r.Codigo)));
        var propios = (await _roles.ListarAsync(empresaId, ct).ConfigureAwait(false)).Select(r => Dto(r, activos.GetValueOrDefault(r.Codigo)));
        return fijos.Concat(propios).ToList();
    }

    /// <summary>Crea un rol propio: con los permisos indicados o, si no se indican, con los de la plantilla.</summary>
    public async Task<Resultado<RolDto>> CrearAsync(Guid empresaId, DatosRol datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var plantilla = datos.Plantilla is null ? null : CatalogoPermisos.Plantillas.FirstOrDefault(p => p.Codigo == datos.Plantilla);
        if (datos.Plantilla is not null && plantilla is null)
        {
            return Resultado.Fallo<RolDto>(Error.Validacion("rol.plantilla", $"La plantilla «{datos.Plantilla}» no existe."));
        }

        var nombre = string.IsNullOrWhiteSpace(datos.Nombre) ? plantilla?.Nombre : datos.Nombre;
        if ((await _roles.ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(r => string.Equals(r.Nombre, nombre?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<RolDto>(Error.Conflicto("rol.repetido", $"Ya hay un rol «{nombre}» en la empresa."));
        }

        var r = RolEmpresa.Crear(empresaId, nombre, datos.Descripcion ?? plantilla?.Descripcion, datos.Permisos is { Count: > 0 } ? datos.Permisos : plantilla?.Permisos, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RolDto>(r.Error);
        }

        _roles.Agregar(r.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(r.Valor, 0));
    }

    /// <summary>Cambia el nombre o los permisos. Los miembros que lo tienen los reciben al volver a entrar en la empresa.</summary>
    public async Task<Resultado<RolDto>> CambiarAsync(Guid empresaId, Guid id, DatosRol datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var rol = await _roles.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (rol is null || rol.EmpresaId != empresaId)
        {
            return Resultado.Fallo<RolDto>(NoExiste());
        }

        if ((await _roles.ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(r => r.Id != id && string.Equals(r.Nombre, datos.Nombre?.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            return Resultado.Fallo<RolDto>(Error.Conflicto("rol.repetido", $"Ya hay un rol «{datos.Nombre}» en la empresa."));
        }

        var r = rol.Cambiar(datos.Nombre, datos.Descripcion, datos.Permisos, _reloj);
        if (r.EsFallo)
        {
            return Resultado.Fallo<RolDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(rol, await MiembrosAsync(empresaId, rol.Codigo, ct).ConfigureAwait(false)));
    }

    /// <summary>Borra un rol propio que ningún miembro activo tiene.</summary>
    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var rol = await _roles.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (rol is null || rol.EmpresaId != empresaId)
        {
            return Resultado.Fallo(NoExiste());
        }

        var miembros = await MiembrosAsync(empresaId, rol.Codigo, ct).ConfigureAwait(false);
        if (miembros > 0)
        {
            return Resultado.Fallo(Error.Conflicto("rol.en_uso", $"{miembros} miembro(s) tienen el rol «{rol.Nombre}»: cámbiales antes el rol."));
        }

        _roles.Eliminar(rol);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    private async Task<int> MiembrosAsync(Guid empresaId, string codigo, CancellationToken ct) =>
        (await _membresias.ListarPorEmpresaAsync(empresaId, ct).ConfigureAwait(false)).Count(m => m.EstaActiva && m.RolCodigo == codigo);

    private static RolDto Dto(RolEmpresa r, int miembros) => new(r.Id, r.Codigo, r.Nombre, r.Descripcion, true, r.Permisos, miembros);

    private static Error NoExiste() => Error.NoEncontrado("rol.no_encontrado", "El rol no existe en esta empresa.");
}
