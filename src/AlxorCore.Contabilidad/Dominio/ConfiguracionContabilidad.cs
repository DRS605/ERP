using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

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

    /// <summary>
    /// Último día de los meses cerrados: no se registran asientos con fecha igual o anterior (salvo la regularización y
    /// el cierre del ejercicio). Nulo: ningún mes cerrado.
    /// </summary>
    public DateOnly? CerradoHasta { get; private set; }

    /// <summary>¿Se pueden registrar asientos con esta fecha?</summary>
    public bool Admite(DateOnly fecha) => CerradoHasta is not { } h || fecha > h;

    /// <summary>Cierra hasta el final del mes indicado (no deja abrir huecos: solo avanza).</summary>
    public Resultado CerrarHasta(int anio, int mes)
    {
        if (mes is < 1 or > 12 || anio is < 2000 or > 2100)
        {
            return Resultado.Fallo(Error.Validacion("periodo.mes", "Indica un mes válido."));
        }

        var fin = new DateOnly(anio, mes, DateTime.DaysInMonth(anio, mes));
        if (CerradoHasta is { } h && fin <= h)
        {
            return Resultado.Fallo(Error.Conflicto("periodo.ya_cerrado", $"Ya está cerrado hasta el {h:dd/MM/yyyy}."));
        }

        CerradoHasta = fin;
        return Resultado.Ok();
    }

    /// <summary>Reabre desde el mes indicado (y todos los posteriores).</summary>
    public Resultado ReabrirDesde(int anio, int mes)
    {
        if (mes is < 1 or > 12 || anio is < 2000 or > 2100)
        {
            return Resultado.Fallo(Error.Validacion("periodo.mes", "Indica un mes válido."));
        }

        var inicio = new DateOnly(anio, mes, 1);
        if (CerradoHasta is not { } h || inicio > h)
        {
            return Resultado.Fallo(Error.Conflicto("periodo.abierto", "Ese mes ya está abierto."));
        }

        CerradoHasta = inicio.AddDays(-1) is var anterior && anterior.Year >= 2000 ? anterior : null;
        return Resultado.Ok();
    }

    public void CambiarModo(ModoContabilidad modo) => Modo = modo;

    public void CambiarContabilizacionAutomatica(bool automatica) => ContabilizacionAutomatica = automatica;

    public void CambiarLongitudSubcuenta(int longitud)
    {
        LongitudSubcuenta = Math.Clamp(longitud, LongitudSubcuentaMinima, LongitudSubcuentaMaxima);
    }
}
