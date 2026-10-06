using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Contabilidad.Dominio;

/// <summary>Diario de sistema: existe en todas las empresas y recoge los asientos de unos orígenes.</summary>
public sealed record DiarioSistema(string Codigo, string Nombre, IReadOnlyList<string> Origenes);

/// <summary>
/// Diarios (series) de asientos. Cada asiento va a un diario y lleva, además del número correlativo del libro diario
/// (único por empresa y ejercicio), su número dentro del diario. Los de sistema existen siempre; la empresa puede crear
/// los suyos y llevar a ellos los asientos de algunos orígenes (p. ej. los cobros y pagos a «BAN»). La base de datos
/// asigna el diario y su número al dar de alta el asiento (<c>contabilidad.asiento_diario</c>).
/// </summary>
public static class DiariosContables
{
    public const string General = "GEN";

    /// <summary>Diarios de sistema. Debe coincidir con el <c>CASE</c> de <c>contabilidad.asiento_diario()</c>.</summary>
    public static readonly IReadOnlyList<DiarioSistema> Sistema =
    [
        new(General, "General", ["Manual"]),
        new("VEN", "Ventas", ["Venta"]),
        new("COM", "Compras", ["Compra"]),
        new("TES", "Cobros y pagos", ["Cobro", "Pago"]),
        new("INM", "Inmovilizado", ["Amortizacion", "BajaInmovilizado", "Enajenacion", "ImpuestoDiferido"]),
        new("PER", "Periodificaciones", ["Periodificacion"]),
        new("CIE", "Regularización y cierre", ["Existencias", "Regularizacion", "Cierre"]),
        new("APE", "Apertura", ["Apertura"]),
    ];

    /// <summary>Orígenes que un diario propio puede recoger (el cierre, la apertura y las anulaciones, no).</summary>
    public static readonly IReadOnlyList<string> OrigenesAsignables = ["Manual", "Venta", "Compra", "Cobro", "Pago", "Amortizacion", "BajaInmovilizado", "Enajenacion", "ImpuestoDiferido", "Periodificacion", "Financiacion"];

    public static bool EsDeSistema(string codigo) => Sistema.Any(d => string.Equals(d.Codigo, codigo, StringComparison.OrdinalIgnoreCase));

    /// <summary>Diario de sistema de un origen (los que no tienen, al general).</summary>
    public static string DeOrigen(string origen) => Sistema.FirstOrDefault(d => d.Origenes.Contains(origen))?.Codigo ?? General;
}

/// <summary>Diario propio de una empresa (además de los de sistema).</summary>
public sealed class DiarioContable : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 10;
    public const int LongitudNombre = 80;

    private List<string> _origenes = [];

    private DiarioContable(Guid id) : base(id, Guid.Empty) { Codigo = null!; Nombre = null!; }

    private DiarioContable(Guid id, Guid empresaId, string codigo, string nombre) : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    /// <summary>Orígenes cuyos asientos van a este diario en lugar de al de sistema.</summary>
    public IReadOnlyList<string> Origenes => _origenes;

    public bool Activo { get; private set; }

    public static Resultado<DiarioContable> Crear(Guid empresaId, string? codigo, string? nombre, IReadOnlyList<string>? origenes)
    {
        var c = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        if (c.Length is 0 or > LongitudCodigo || !c.All(char.IsAsciiLetterOrDigit))
        {
            return Resultado.Fallo<DiarioContable>(Error.Validacion("diario.codigo", $"El código del diario lleva de 1 a {LongitudCodigo} letras o números."));
        }

        if (DiariosContables.EsDeSistema(c))
        {
            return Resultado.Fallo<DiarioContable>(Error.Conflicto("diario.de_sistema", $"«{c}» es un diario de sistema."));
        }

        var d = new DiarioContable(Guid.NewGuid(), empresaId, c, string.Empty);
        var r = d.Modificar(nombre, origenes, true);
        return r.EsFallo ? Resultado.Fallo<DiarioContable>(r.Error) : Resultado.Ok(d);
    }

    public Resultado Modificar(string? nombre, IReadOnlyList<string>? origenes, bool activo)
    {
        var n = (nombre ?? string.Empty).Trim();
        if (n.Length is 0 or > LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("diario.nombre", $"El nombre del diario es obligatorio (hasta {LongitudNombre} caracteres)."));
        }

        var lista = (origenes ?? []).Select(o => o.Trim()).Where(o => o.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        if (lista.FirstOrDefault(o => !DiariosContables.OrigenesAsignables.Contains(o)) is { } malo)
        {
            return Resultado.Fallo(Error.Validacion("diario.origen", $"«{malo}» no es un origen que pueda ir a un diario propio."));
        }

        Nombre = n;
        _origenes = lista;
        Activo = activo;
        return Resultado.Ok();
    }
}
