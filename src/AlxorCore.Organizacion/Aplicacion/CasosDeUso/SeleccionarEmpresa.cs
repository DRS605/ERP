using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Seguridad;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Organizacion.Aplicacion.CasosDeUso;

/// <summary>
/// Caso de uso: seleccionar la empresa activa. Verifica que el usuario tiene una membresía activa
/// en esa empresa y emite un nuevo token con el alcance de la empresa (empresa, rol y permisos),
/// que el resto de módulos usarán para autorizar y aislar los datos.
/// </summary>
public sealed class SeleccionarEmpresa
{
    private readonly IRepositorioMembresias _membresias;
    private readonly IRepositorioEmpresas _empresas;
    private readonly IProveedorTokens _tokens;

    public SeleccionarEmpresa(IRepositorioMembresias membresias, IRepositorioEmpresas empresas, IProveedorTokens tokens)
    {
        _membresias = membresias;
        _empresas = empresas;
        _tokens = tokens;
    }

    public async Task<Resultado<ResultadoSeleccionEmpresa>> EjecutarAsync(
        IdentidadUsuario usuario,
        Guid empresaId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(usuario);

        var membresia = await _membresias.ObtenerAsync(usuario.Id, empresaId, ct).ConfigureAwait(false);
        if (membresia is null || !membresia.EstaActiva)
        {
            return Resultado.Fallo<ResultadoSeleccionEmpresa>(
                Error.Prohibido("empresa.sin_acceso", "No tienes acceso a esa empresa."));
        }

        var rol = Rol.PorCodigoRol(membresia.RolCodigo);
        if (rol.EsFallo)
        {
            return Resultado.Fallo<ResultadoSeleccionEmpresa>(rol.Error);
        }

        var grupoId = await _empresas.ObtenerGrupoIdAsync(empresaId, ct).ConfigureAwait(false);
        if (grupoId is null)
        {
            return Resultado.Fallo<ResultadoSeleccionEmpresa>(
                Error.NoEncontrado("empresa.sin_grupo", "La empresa no tiene grupo asignado."));
        }

        var permisos = rol.Valor.PermisosConcedidos.ToList();
        var alcance = new AlcanceEmpresa(empresaId, grupoId.Value, rol.Valor.Codigo, permisos);
        var token = _tokens.GenerarToken(usuario, alcance);

        return Resultado.Ok(new ResultadoSeleccionEmpresa(
            token.Token,
            token.ExpiraEn,
            empresaId,
            rol.Valor.Codigo,
            permisos));
    }
}
