using AlxorCore.Facturacion.Dominio;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Agentes comerciales, sus clientes, sus reglas de comisión y sus liquidaciones.</summary>
public interface IRepositorioComisiones
{
    Task<IReadOnlyList<AgenteComercial>> AgentesAsync(Guid empresaId, CancellationToken ct = default);
    Task<AgenteComercial?> AgenteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<AsignacionAgente>> AsignacionesAsync(Guid empresaId, CancellationToken ct = default);
    Task<IReadOnlyList<ReglaComision>> ReglasAsync(Guid empresaId, CancellationToken ct = default);
    Task<ReglaComision?> ReglaAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LiquidacionAgente>> LiquidacionesAsync(Guid empresaId, Guid? agenteId, CancellationToken ct = default);
    Task<LiquidacionAgente?> LiquidacionAsync(Guid id, CancellationToken ct = default);
    Task<int> SiguienteNumeroLiquidacionAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
    void Agregar(object entidad);
    void Eliminar(object entidad);
}
