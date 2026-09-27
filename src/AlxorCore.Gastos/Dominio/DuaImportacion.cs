using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Gastos.Dominio;

/// <summary>
/// DUA de importación de una compra fuera de la UE, ligado a la factura del proveedor (el gasto): su MRN, la fecha de
/// admisión (levante), el valor en aduana, los aranceles y el IVA a la importación que liquida la aduana (el que se
/// deduce, no el de la factura del proveedor, que no lleva IVA español).
/// </summary>
public sealed class DuaImportacion : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMrn = 18;

    private DuaImportacion(Guid id) : base(id, Guid.Empty) { Mrn = null!; }

    private DuaImportacion(Guid id, Guid empresaId, Guid gastoId) : base(id, empresaId) { Mrn = null!; GastoId = gastoId; }

    public Guid GastoId { get; private set; }

    public string Mrn { get; private set; }

    public DateOnly FechaAdmision { get; private set; }

    public string? Aduana { get; private set; }

    /// <summary>Base del IVA a la importación: valor en aduana más aranceles y gastos hasta el destino.</summary>
    public decimal BaseIva { get; private set; }

    /// <summary>Derechos arancelarios pagados.</summary>
    public decimal Aranceles { get; private set; }

    /// <summary>IVA a la importación liquidado en el DUA.</summary>
    public decimal CuotaIva { get; private set; }

    public string? Observaciones { get; private set; }

    public static Resultado<DuaImportacion> Crear(Guid empresaId, Guid gastoId, string? mrn, DateOnly fechaAdmision, string? aduana, decimal baseIva, decimal aranceles,
        decimal cuotaIva, string? observaciones)
    {
        var d = new DuaImportacion(Guid.NewGuid(), empresaId, gastoId);
        var r = d.Modificar(mrn, fechaAdmision, aduana, baseIva, aranceles, cuotaIva, observaciones);
        return r.EsFallo ? Resultado.Fallo<DuaImportacion>(r.Error) : Resultado.Ok(d);
    }

    public Resultado Modificar(string? mrn, DateOnly fechaAdmision, string? aduana, decimal baseIva, decimal aranceles, decimal cuotaIva, string? observaciones)
    {
        var m = NormalizarMrn(mrn);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("dua.mrn", "El MRN tiene 18 caracteres: año (2 dígitos), país (2 letras) y 14 letras o números."));
        }

        if (baseIva < 0m || aranceles < 0m || cuotaIva < 0m || Redondeo.Dos(baseIva) != baseIva || Redondeo.Dos(aranceles) != aranceles || Redondeo.Dos(cuotaIva) != cuotaIva)
        {
            return Resultado.Fallo(Error.Validacion("dua.importes", "Los importes son positivos, con 2 decimales."));
        }

        Mrn = m;
        FechaAdmision = fechaAdmision;
        Aduana = Texto(aduana, 60);
        BaseIva = baseIva;
        Aranceles = aranceles;
        CuotaIva = cuotaIva;
        Observaciones = Texto(observaciones, 300);
        return Resultado.Ok();
    }

    public static string? NormalizarMrn(string? mrn)
    {
        var m = new string((mrn ?? string.Empty).Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
        return m.Length == LongitudMrn && char.IsAsciiDigit(m[0]) && char.IsAsciiDigit(m[1]) && char.IsAsciiLetter(m[2]) && char.IsAsciiLetter(m[3]) ? m : null;
    }

    private static string? Texto(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : s.Trim() is var t && t.Length > max ? t[..max] : s.Trim();
}
