using System.Security.Claims;
using AlxorCore.Nucleo.Multiempresa;
using AlxorCore.Nucleo.Seguridad;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Resuelve la empresa activa a partir del claim <c>empresa_id</c> del usuario de la petición.
/// Es la fuente que usan la persistencia (filtro global + RLS) y la autorización. También admite
/// <b>fijar</b> la empresa por código (<see cref="Fijar"/>) para los procesos en segundo plano, que
/// no tienen petición HTTP; en ese caso la empresa fijada tiene prioridad sobre el claim.
/// </summary>
public sealed class ContextoEmpresaHttp : IContextoEmpresaMutable
{
    private readonly IHttpContextAccessor _accessor;
    private Guid? _fijada;
    private Guid? _grupoFijado;

    public ContextoEmpresaHttp(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid? EmpresaId
    {
        get
        {
            if (_fijada is not null)
            {
                return _fijada;
            }

            var valor = _accessor.HttpContext?.User.FindFirstValue(ClaimsAlxor.EmpresaId);
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public Guid? GrupoId
    {
        get
        {
            if (_grupoFijado is not null)
            {
                return _grupoFijado;
            }

            var valor = _accessor.HttpContext?.User.FindFirstValue(ClaimsAlxor.GrupoId);
            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }

    public void Fijar(Guid empresaId) => _fijada = empresaId;

    public void FijarGrupo(Guid grupoId) => _grupoFijado = grupoId;
}
