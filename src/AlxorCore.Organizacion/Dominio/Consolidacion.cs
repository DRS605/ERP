using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>Cómo entra una empresa del grupo en la consolidación.</summary>
public enum MetodoConsolidacion
{
    /// <summary>Integración global: sus saldos al 100 %; la parte de otros socios va a socios externos.</summary>
    Global = 1,

    /// <summary>Integración proporcional: sus saldos en el porcentaje de participación.</summary>
    Proporcional = 2,

    /// <summary>Fuera del perímetro: no se consolida (sus operaciones con el grupo son con terceros).</summary>
    Excluida = 3,
}

/// <summary>
/// Participación del grupo en una empresa y su método de consolidación (el perímetro). Sin registro, la empresa entra
/// por integración global al 100 %.
/// </summary>
public sealed class PerimetroConsolidacion : RaizAgregadoGrupo<Guid>
{
    private PerimetroConsolidacion(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private PerimetroConsolidacion(Guid id, Guid grupoId, Guid empresaId)
        : base(id, grupoId)
    {
        EmpresaId = empresaId;
        Porcentaje = 100m;
        Metodo = MetodoConsolidacion.Global;
    }

    public Guid EmpresaId { get; private set; }

    /// <summary>Porcentaje de participación del grupo (0 a 100).</summary>
    public decimal Porcentaje { get; private set; }

    public MetodoConsolidacion Metodo { get; private set; }

    /// <summary>Empresa del grupo que tiene la participación (la inversión que se elimina contra el patrimonio neto).</summary>
    public Guid? TitularId { get; private set; }

    /// <summary>Cuenta (o prefijo) de la inversión en la titular; por defecto 2403, participaciones en empresas del grupo.</summary>
    public string? CuentaInversion { get; private set; }

    /// <summary>Coste de la participación; sin él, el saldo de <see cref="CuentaInversion"/> en la titular.</summary>
    public decimal? CosteInversion { get; private set; }

    /// <summary>Patrimonio neto de la empresa en la fecha de adquisición; sin él, el actual (sin reservas posteriores).</summary>
    public decimal? PatrimonioAdquisicion { get; private set; }

    public const string CuentaInversionPorDefecto = "2403";

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static PerimetroConsolidacion Crear(Guid grupoId, Guid empresaId) => new(Guid.NewGuid(), grupoId, empresaId);

    public Resultado Fijar(decimal porcentaje, MetodoConsolidacion metodo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (!Enum.IsDefined(metodo))
        {
            return Resultado.Fallo(Error.Validacion("consolidacion.metodo", "El método de consolidación no es válido."));
        }

        if (porcentaje is <= 0m or > 100m || decimal.Round(porcentaje, 4) != porcentaje)
        {
            return Resultado.Fallo(Error.Validacion("consolidacion.porcentaje", "La participación va de más de 0 % a 100 % (hasta 4 decimales)."));
        }

        Porcentaje = porcentaje;
        Metodo = metodo;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>
    /// Fija la participación que se elimina contra el patrimonio neto: quién la tiene, en qué cuenta, su coste y el
    /// patrimonio neto de la empresa al adquirirla. Sin titular, no hay eliminación inversión-patrimonio neto.
    /// </summary>
    public Resultado FijarInversion(Guid? titularId, string? cuenta, decimal? coste, decimal? patrimonioAdquisicion)
    {
        if (titularId is null)
        {
            TitularId = null;
            CuentaInversion = null;
            CosteInversion = null;
            PatrimonioAdquisicion = null;
            return Resultado.Ok();
        }

        if (titularId == EmpresaId)
        {
            return Resultado.Fallo(Error.Validacion("consolidacion.titular", "La participación la tiene otra empresa del grupo, no ella misma."));
        }

        var c = string.IsNullOrWhiteSpace(cuenta) ? CuentaInversionPorDefecto : cuenta.Trim();
        if (c.Length is < 3 or > 12 || !c.All(char.IsAsciiDigit))
        {
            return Resultado.Fallo(Error.Validacion("consolidacion.cuenta_inversion", "La cuenta de la inversión es un código de 3 a 12 dígitos."));
        }

        if (coste is < 0m || (coste is { } k && decimal.Round(k, 2) != k) || (patrimonioAdquisicion is { } pn && decimal.Round(pn, 2) != pn))
        {
            return Resultado.Fallo(Error.Validacion("consolidacion.importes", "El coste (≥ 0) y el patrimonio neto van en euros con 2 decimales."));
        }

        TitularId = titularId;
        CuentaInversion = c;
        CosteInversion = coste;
        PatrimonioAdquisicion = patrimonioAdquisicion;
        return Resultado.Ok();
    }
}

/// <summary>
/// Correspondencia de cuentas entre dos empresas del grupo (la «CuentaContableCorrespondenciaEmpresasGrupo» de
/// Hispatec): la cuenta de una empresa recoge el saldo recíproco de la cuenta de la otra (p. ej. un préstamo, 5523 en
/// la prestamista y 5133 en la prestataria). En la consolidación se eliminan los dos saldos, y la diferencia entre
/// ellos es un descuadre intragrupo.
/// </summary>
public sealed class CorrespondenciaCuentas : RaizAgregadoGrupo<Guid>
{
    public const int LongitudDescripcion = 120;

    private CorrespondenciaCuentas(Guid id)
        : base(id, Guid.Empty)
    {
        CuentaA = null!;
        CuentaB = null!;
        Descripcion = null!;
    }

    private CorrespondenciaCuentas(Guid id, Guid grupoId)
        : base(id, grupoId)
    {
        CuentaA = null!;
        CuentaB = null!;
        Descripcion = null!;
    }

    public Guid EmpresaAId { get; private set; }

    /// <summary>Cuenta (o prefijo de cuenta, de 3 a 12 dígitos) en la empresa A.</summary>
    public string CuentaA { get; private set; }

    public Guid EmpresaBId { get; private set; }

    public string CuentaB { get; private set; }

    public string Descripcion { get; private set; }

    public static Resultado<CorrespondenciaCuentas> Crear(Guid grupoId, Guid empresaAId, string? cuentaA, Guid empresaBId, string? cuentaB, string? descripcion)
    {
        var c = new CorrespondenciaCuentas(Guid.NewGuid(), grupoId);
        var r = c.Fijar(empresaAId, cuentaA, empresaBId, cuentaB, descripcion);
        return r.EsFallo ? Resultado.Fallo<CorrespondenciaCuentas>(r.Error) : Resultado.Ok(c);
    }

    public Resultado Fijar(Guid empresaAId, string? cuentaA, Guid empresaBId, string? cuentaB, string? descripcion)
    {
        cuentaA = cuentaA?.Trim();
        cuentaB = cuentaB?.Trim();
        if (empresaAId == Guid.Empty || empresaBId == Guid.Empty || empresaAId == empresaBId)
        {
            return Resultado.Fallo(Error.Validacion("correspondencia.empresas", "Una correspondencia une cuentas de dos empresas distintas del grupo."));
        }

        if (!CuentaValida(cuentaA) || !CuentaValida(cuentaB))
        {
            return Resultado.Fallo(Error.Validacion("correspondencia.cuenta", "Cada cuenta tiene de 3 a 12 dígitos."));
        }

        var texto = string.IsNullOrWhiteSpace(descripcion) ? $"{cuentaA} ↔ {cuentaB}" : descripcion.Trim();
        EmpresaAId = empresaAId;
        CuentaA = cuentaA!;
        EmpresaBId = empresaBId;
        CuentaB = cuentaB!;
        Descripcion = texto.Length > LongitudDescripcion ? texto[..LongitudDescripcion] : texto;
        return Resultado.Ok();
    }

    private static bool CuentaValida(string? c) => c is { Length: >= 3 and <= 12 } && c.All(char.IsAsciiDigit);
}
