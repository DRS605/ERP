using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Persistencia de la situación de la deuda (impagados, dudosos), sus regularizaciones y las renovaciones.</summary>
public interface IRepositorioDeudas
{
    void Agregar(object entidad);

    Task<ConfiguracionCartera?> ConfiguracionAsync(CancellationToken ct = default);

    Task<SituacionDeuda?> SituacionAsync(TipoDocumentoTesoreria tipo, Guid documentoId, CancellationToken ct = default);

    Task<SituacionDeuda?> SituacionAsync(Guid id, CancellationToken ct = default);

    /// <summary>Documentos con la deuda fuera de su cuenta de origen (impagados, dudosos) o declarados incobrables.</summary>
    Task<IReadOnlyList<SituacionDeuda>> SituacionesAsync(CancellationToken ct = default);

    Task<RegularizacionDeuda?> RegularizacionDeAsync(Guid movimientoId, CancellationToken ct = default);

    Task<RenovacionEfecto?> RenovacionAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<RenovacionEfecto>> RenovacionesAsync(CancellationToken ct = default);

    Task<bool> EsMovimientoDeRenovacionAsync(Guid movimientoId, CancellationToken ct = default);

    /// <summary>Si el cobro sacó deuda de su cuenta (aunque luego se deshiciera).</summary>
    Task<bool> RegularizoAsync(Guid movimientoId, CancellationToken ct = default);
}
