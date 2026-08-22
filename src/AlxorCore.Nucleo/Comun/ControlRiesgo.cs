namespace AlxorCore.Nucleo.Comun;

/// <summary>
/// Cómo actúa la empresa cuando un cliente/proveedor supera su límite de riesgo al emitir o registrar
/// un documento. Transversal: lo consumen Facturación y Gastos.
/// </summary>
public enum ControlRiesgo
{
    /// <summary>Avisa del exceso pero permite continuar (por defecto).</summary>
    Aviso = 1,

    /// <summary>Bloquea la operación si se supera el límite.</summary>
    Bloqueo = 2,
}
