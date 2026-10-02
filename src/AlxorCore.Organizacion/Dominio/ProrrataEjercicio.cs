using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>Regla de prorrata del IVA/IGIC soportado (arts. 102-106 Ley 37/1992; equivalente en el IGIC).</summary>
public enum RegimenProrrata
{
    /// <summary>Se deduce el porcentaje de prorrata de todo el impuesto soportado.</summary>
    General = 1,

    /// <summary>
    /// Se deduce entero el soportado en bienes y servicios usados solo en operaciones con derecho a
    /// deducción, nada del usado solo en operaciones sin derecho, y la prorrata del de uso común.
    /// </summary>
    Especial = 2,
}

/// <summary>
/// Prorrata de un ejercicio: la aplica la empresa que realiza a la vez operaciones con derecho a
/// deducir (sujetas, exportaciones…) y sin él (exentas del art. 20). Durante el año se deduce con el
/// <b>porcentaje provisional</b> (el definitivo del año anterior, o uno estimado el primer año); en la
/// última autoliquidación se calcula el definitivo y se regulariza la diferencia.
/// </summary>
public sealed class ProrrataEjercicio : RaizAgregadoEmpresa<Guid>
{
    private ProrrataEjercicio(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ProrrataEjercicio(Guid id, Guid empresaId, int ejercicio, RegimenProrrata regimen, int porcentajeProvisional, TipoImpuesto impuesto)
        : base(id, empresaId)
    {
        Impuesto = impuesto;
        Ejercicio = ejercicio;
        Regimen = regimen;
        PorcentajeProvisional = porcentajeProvisional;
    }

    public int Ejercicio { get; private set; }

    /// <summary>Impuesto al que se aplica (IVA o IGIC): con actividad en los dos territorios, cada uno tiene la suya.</summary>
    public TipoImpuesto Impuesto { get; private set; } = TipoImpuesto.Iva;

    public RegimenProrrata Regimen { get; private set; }

    /// <summary>Porcentaje (entero, 0-100) con el que se deduce durante el ejercicio.</summary>
    public int PorcentajeProvisional { get; private set; }

    public static Resultado<ProrrataEjercicio> Crear(Guid empresaId, int ejercicio, RegimenProrrata regimen, int porcentajeProvisional,
        TipoImpuesto impuesto = TipoImpuesto.Iva)
    {
        var error = Validar(ejercicio, regimen, porcentajeProvisional);
        return error is not null
            ? Resultado.Fallo<ProrrataEjercicio>(error)
            : Resultado.Ok(new ProrrataEjercicio(Guid.NewGuid(), empresaId, ejercicio, regimen, porcentajeProvisional, impuesto));
    }

    public Resultado Cambiar(RegimenProrrata regimen, int porcentajeProvisional)
    {
        var error = Validar(Ejercicio, regimen, porcentajeProvisional);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Regimen = regimen;
        PorcentajeProvisional = porcentajeProvisional;
        return Resultado.Ok();
    }

    private static Error? Validar(int ejercicio, RegimenProrrata regimen, int porcentaje)
    {
        if (ejercicio is < 2000 or > 2100)
        {
            return Error.Validacion("prorrata.ejercicio", "El ejercicio no es válido.");
        }

        if (!Enum.IsDefined(regimen))
        {
            return Error.Validacion("prorrata.regimen", "El régimen de prorrata debe ser General o Especial.");
        }

        return porcentaje is < 0 or > 100
            ? Error.Validacion("prorrata.porcentaje", "El porcentaje de prorrata es un entero entre 0 y 100.")
            : null;
    }
}
