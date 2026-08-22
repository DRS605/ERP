namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Convierte importes de una divisa extranjera a la moneda funcional (EUR) usando los tipos de cambio
/// de la empresa. Lo implementa el módulo Divisas y lo pueden consumir Facturación, Gastos, etc. para
/// registrar documentos en divisa. El EUR se convierte a sí mismo (tasa 1).
/// </summary>
public interface IConversorDivisa
{
    /// <summary>Tasa vigente (EUR por 1 unidad de la divisa) a una fecha, o null si no hay tipo de cambio.</summary>
    Task<decimal?> TasaVigenteAsync(Guid empresaId, string divisa, DateOnly fecha, CancellationToken ct = default);

    /// <summary>Convierte un importe en divisa a euros a la fecha indicada, o null si falta el tipo de cambio.</summary>
    Task<decimal?> AEurosAsync(Guid empresaId, string divisa, decimal importe, DateOnly fecha, CancellationToken ct = default);
}
