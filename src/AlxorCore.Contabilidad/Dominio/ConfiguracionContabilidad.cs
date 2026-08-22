using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Modo de contabilidad elegido por la empresa.</summary>
public enum ModoContabilidad
{
    /// <summary>"Un libro": contabilizar registra un gasto con IVA soportado (autónomos/pymes pequeñas).</summary>
    Simple = 1,

    /// <summary>Partida doble: además del gasto, genera el asiento contable (libro diario y mayor).</summary>
    Completo = 2,
}

/// <summary>Ajustes de contabilidad de una empresa. Su clave es el propio <c>empresa_id</c>.</summary>
public sealed class ConfiguracionContabilidad : RaizAgregadoEmpresa<Guid>
{
    private ConfiguracionContabilidad()
        : base(Guid.Empty, Guid.Empty)
    {
    }

    public ConfiguracionContabilidad(Guid empresaId, ModoContabilidad modo)
        : base(empresaId, empresaId)
    {
        Modo = modo;
        LongitudSubcuenta = LongitudSubcuentaDefecto;
    }

    /// <summary>Longitud por defecto de las subcuentas de tercero (estándar ContaPlus/a3: 8 dígitos).</summary>
    public const int LongitudSubcuentaDefecto = 8;

    public const int LongitudSubcuentaMinima = 4;

    public const int LongitudSubcuentaMaxima = 12;

    public ModoContabilidad Modo { get; private set; }

    /// <summary>
    /// Si es <c>true</c>, los documentos se contabilizan automáticamente al crearse; si es
    /// <c>false</c> (por defecto), quedan <b>pendientes de contabilizar</b> a la espera de que el
    /// contable los revise y confirme (pudiendo ajustar la fecha de registro).
    /// </summary>
    public bool ContabilizacionAutomatica { get; private set; }

    /// <summary>
    /// Longitud total (dígitos) de las subcuentas individuales de cliente/proveedor/trabajador. Con
    /// raíz 430 y longitud 8, un cliente sería 43000001. Solo aplica en modo Completo; en Simple los
    /// terceros comparten la cuenta raíz (430/400/465).
    /// </summary>
    public int LongitudSubcuenta { get; private set; } = LongitudSubcuentaDefecto;

    public void CambiarModo(ModoContabilidad modo) => Modo = modo;

    public void CambiarContabilizacionAutomatica(bool automatica) => ContabilizacionAutomatica = automatica;

    public void CambiarLongitudSubcuenta(int longitud)
    {
        LongitudSubcuenta = Math.Clamp(longitud, LongitudSubcuentaMinima, LongitudSubcuentaMaxima);
    }
}
