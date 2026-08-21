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
    }

    public ModoContabilidad Modo { get; private set; }

    public void CambiarModo(ModoContabilidad modo) => Modo = modo;
}
