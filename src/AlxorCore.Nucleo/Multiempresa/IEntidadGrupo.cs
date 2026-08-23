namespace AlxorCore.Nucleo.Multiempresa;

/// <summary>
/// Marca una entidad cuyo tenant es el <b>grupo</b> (holding) y no una empresa concreta: los
/// <i>datos maestros compartidos</i> (clientes, proveedores, artículos…) que valen para todas las
/// empresas del mismo grupo. La infraestructura aplica automáticamente el filtro por
/// <see cref="GrupoId"/> y la Row-Level Security por grupo, igual que <see cref="IEntidadEmpresa"/>
/// hace por empresa.
/// </summary>
public interface IEntidadGrupo
{
    /// <summary>Grupo (holding) al que pertenece la entidad.</summary>
    Guid GrupoId { get; }
}
