using AlxorCore.Nucleo.Multiempresa;

namespace AlxorCore.Nucleo.Dominio;

/// <summary>
/// Raíz de agregado que pertenece a un <b>grupo</b> (holding) y no a una empresa concreta: los datos
/// maestros compartidos entre todas las empresas del grupo (clientes, proveedores, artículos…). La
/// persistencia filtra por <see cref="GrupoId"/> y aplica RLS por grupo.
/// </summary>
/// <typeparam name="TId">Tipo del identificador.</typeparam>
public abstract class RaizAgregadoGrupo<TId> : RaizAgregado<TId>, IEntidadGrupo
    where TId : notnull
{
    protected RaizAgregadoGrupo(TId id, Guid grupoId)
        : base(id)
    {
        GrupoId = grupoId;
    }

    /// <summary>Grupo (holding) al que pertenece el agregado.</summary>
    public Guid GrupoId { get; protected init; }
}
