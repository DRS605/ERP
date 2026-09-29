using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>
/// Datos de la explotación del agricultor para el cuaderno digital (SIEX, RD 1054/2022): su código en el registro de
/// explotaciones (REGEPA), el asesor en gestión integrada de plagas y el aplicador y el equipo habituales, que se
/// usan en las labores que no traen los suyos.
/// </summary>
public sealed class ExplotacionSiex : RaizAgregadoEmpresa<Guid>
{
    private ExplotacionSiex(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private ExplotacionSiex(Guid id, Guid empresaId, Guid agricultorId)
        : base(id, empresaId)
    {
        AgricultorId = agricultorId;
    }

    public Guid AgricultorId { get; private set; }

    /// <summary>Código de la explotación en el REGEPA (o el registro autonómico).</summary>
    public string? CodigoRegepa { get; private set; }

    public string? AsesorNombre { get; private set; }

    /// <summary>Número ROPO del asesor.</summary>
    public string? AsesorRopo { get; private set; }

    /// <summary>Carné de aplicador (ROPO) de quien trata normalmente.</summary>
    public string? CarneAplicador { get; private set; }

    /// <summary>Equipo de aplicación habitual (número ROMA).</summary>
    public string? EquipoRoma { get; private set; }

    /// <summary>Si la explotación está obligada al plan de abonado (RD 1051/2022).</summary>
    public bool PlanAbonadoObligatorio { get; private set; }

    public static ExplotacionSiex Nueva(Guid empresaId, Guid agricultorId) => new(Guid.NewGuid(), empresaId, agricultorId);

    public Resultado Fijar(DatosExplotacionSiex d)
    {
        ArgumentNullException.ThrowIfNull(d);
        var regepa = T(d.CodigoRegepa, 20)?.ToUpperInvariant();
        if (regepa is not null && !regepa.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '/' or '.'))
        {
            return Resultado.Fallo(Error.Validacion("explotacion.regepa", "El código REGEPA lleva letras, números y, como mucho, guiones, barras o puntos."));
        }

        CodigoRegepa = regepa;
        AsesorNombre = T(d.AsesorNombre, 150);
        AsesorRopo = T(d.AsesorRopo, 30);
        CarneAplicador = T(d.CarneAplicador, 30);
        EquipoRoma = T(d.EquipoRoma, 30);
        PlanAbonadoObligatorio = d.PlanAbonadoObligatorio;
        return Resultado.Ok();
    }

    internal static string? T(string? t, int max) => string.IsNullOrWhiteSpace(t) ? null : t.Trim()[..Math.Min(t.Trim().Length, max)];
}

public sealed record DatosExplotacionSiex(string? CodigoRegepa = null, string? AsesorNombre = null, string? AsesorRopo = null, string? CarneAplicador = null,
    string? EquipoRoma = null, bool PlanAbonadoObligatorio = false);

public enum TipoAnalisis
{
    Suelo = 1,
    Agua = 2,
    Foliar = 3,

    /// <summary>Residuos de fitosanitarios en el fruto.</summary>
    Residuos = 4,

    /// <summary>Estiércol, purín u otro abono orgánico.</summary>
    Abono = 5,
}

/// <summary>Análisis de una parcela (suelo, agua, hoja, residuos o abono), con el laboratorio y su resultado.</summary>
public sealed class AnalisisAgro : RaizAgregadoEmpresa<Guid>
{
    private AnalisisAgro(Guid id)
        : base(id, Guid.Empty)
    {
        Laboratorio = null!;
    }

    private AnalisisAgro(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Laboratorio = null!;
    }

    public Guid ParcelaId { get; private set; }

    public DateOnly Fecha { get; private set; }

    public TipoAnalisis Tipo { get; private set; }

    public string Laboratorio { get; private set; }

    public string? Boletin { get; private set; }

    /// <summary>Nitrógeno disponible (kg/ha), para el plan de abonado.</summary>
    public decimal? NitrogenoKgHa { get; private set; }

    public decimal? MateriaOrganicaPct { get; private set; }

    public decimal? Ph { get; private set; }

    /// <summary>Residuos: si se detectó algo por encima del límite.</summary>
    public bool? SuperaLimites { get; private set; }

    /// <summary>Resultado del análisis y lo que se concluye.</summary>
    public string? Conclusion { get; private set; }

    public static Resultado<AnalisisAgro> Crear(Guid empresaId, Guid parcelaId, DatosAnalisis d)
    {
        var a = new AnalisisAgro(Guid.NewGuid(), empresaId) { ParcelaId = parcelaId };
        var r = a.Cambiar(d);
        return r.EsFallo ? Resultado.Fallo<AnalisisAgro>(r.Error) : Resultado.Ok(a);
    }

    public Resultado Cambiar(DatosAnalisis d)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (!Enum.IsDefined(d.Tipo))
        {
            return Resultado.Fallo(Error.Validacion("analisis.tipo", "El análisis es de suelo, agua, foliar, residuos o abono."));
        }

        if (string.IsNullOrWhiteSpace(d.Laboratorio))
        {
            return Resultado.Fallo(Error.Validacion("analisis.laboratorio", "Indica el laboratorio."));
        }

        if (d.NitrogenoKgHa is < 0m || d.MateriaOrganicaPct is < 0m or > 100m || d.Ph is < 0m or > 14m)
        {
            return Resultado.Fallo(Error.Validacion("analisis.valores", "Nitrógeno no negativo, materia orgánica entre 0 y 100 % y pH entre 0 y 14."));
        }

        Fecha = d.Fecha;
        Tipo = d.Tipo;
        Laboratorio = ExplotacionSiex.T(d.Laboratorio, 150)!;
        Boletin = ExplotacionSiex.T(d.Boletin, 60);
        NitrogenoKgHa = d.NitrogenoKgHa;
        MateriaOrganicaPct = d.MateriaOrganicaPct;
        Ph = d.Ph;
        SuperaLimites = d.Tipo == TipoAnalisis.Residuos ? d.SuperaLimites : null;
        Conclusion = ExplotacionSiex.T(d.Conclusion, 2000);
        return Resultado.Ok();
    }
}

public sealed record DatosAnalisis(DateOnly Fecha, TipoAnalisis Tipo, string? Laboratorio, string? Boletin = null, decimal? NitrogenoKgHa = null, decimal? MateriaOrganicaPct = null,
    decimal? Ph = null, bool? SuperaLimites = null, string? Conclusion = null);

/// <summary>
/// Plan de abonado de una parcela para un año (RD 1051/2022): las unidades fertilizantes que necesita el cultivo para
/// la producción esperada. El balance lo compara con lo aportado en los abonados del cuaderno.
/// </summary>
public sealed class PlanAbonado : RaizAgregadoEmpresa<Guid>
{
    private PlanAbonado(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private PlanAbonado(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
    }

    public Guid ParcelaId { get; private set; }

    public int Anio { get; private set; }

    public decimal? ProduccionEsperadaKgHa { get; private set; }

    public decimal NitrogenoKgHa { get; private set; }

    public decimal FosforoKgHa { get; private set; }

    public decimal PotasioKgHa { get; private set; }

    public string? Observaciones { get; private set; }

    public static Resultado<PlanAbonado> Crear(Guid empresaId, Guid parcelaId, int anio, DatosPlanAbonado d)
    {
        if (anio is < 2000 or > 2100)
        {
            return Resultado.Fallo<PlanAbonado>(Error.Validacion("plan_abonado.anio", "Año no válido."));
        }

        var p = new PlanAbonado(Guid.NewGuid(), empresaId) { ParcelaId = parcelaId, Anio = anio };
        var r = p.Cambiar(d);
        return r.EsFallo ? Resultado.Fallo<PlanAbonado>(r.Error) : Resultado.Ok(p);
    }

    public Resultado Cambiar(DatosPlanAbonado d)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (d.NitrogenoKgHa < 0m || d.FosforoKgHa < 0m || d.PotasioKgHa < 0m || d.ProduccionEsperadaKgHa is < 0m)
        {
            return Resultado.Fallo(Error.Validacion("plan_abonado.valores", "Las necesidades y la producción esperada no pueden ser negativas."));
        }

        ProduccionEsperadaKgHa = d.ProduccionEsperadaKgHa;
        NitrogenoKgHa = d.NitrogenoKgHa;
        FosforoKgHa = d.FosforoKgHa;
        PotasioKgHa = d.PotasioKgHa;
        Observaciones = ExplotacionSiex.T(d.Observaciones, 500);
        return Resultado.Ok();
    }
}

public sealed record DatosPlanAbonado(decimal NitrogenoKgHa, decimal FosforoKgHa, decimal PotasioKgHa, decimal? ProduccionEsperadaKgHa = null, string? Observaciones = null);
