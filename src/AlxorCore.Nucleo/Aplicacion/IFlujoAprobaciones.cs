namespace AlxorCore.Nucleo.Aplicacion;

/// <summary>
/// Puerto transversal del flujo de aprobaciones. Permite a otros módulos (Facturación, Gastos,
/// Tesorería…) preguntar si una operación supera el umbral que exige aprobación y, en su caso, abrir
/// una solicitud de aprobación. Lo implementa el módulo Aprobaciones. <paramref name="tipoDocumento"/>
/// es un código libre (p. ej. «Factura», «Gasto», «Pago»).
/// </summary>
public interface IFlujoAprobaciones
{
    /// <summary>¿La operación de ese tipo y con ese importe requiere aprobación según las reglas?</summary>
    Task<bool> RequiereAprobacionAsync(Guid empresaId, string tipoDocumento, decimal importe, CancellationToken ct = default);

    /// <summary>Abre una solicitud de aprobación para un documento y devuelve su identificador.</summary>
    Task<Guid> AbrirSolicitudAsync(Guid empresaId, string tipoDocumento, Guid documentoId, string referencia, decimal importe, Guid solicitanteUsuarioId, CancellationToken ct = default);
}
