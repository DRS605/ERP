using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Facturacion.Dominio;

/// <summary>
/// Despacho de exportación de una factura (el DUA de exportación que presenta el agente de aduanas): su MRN, la fecha
/// del despacho y, cuando la mercancía sale de la UE, la fecha de salida. Es la prueba de la exención del art. 21 LIVA.
/// Una factura puede tener varios (envíos parciales).
/// </summary>
public sealed class DespachoAduanero : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMrn = 18;

    private DespachoAduanero(Guid id) : base(id, Guid.Empty) { Mrn = null!; }

    private DespachoAduanero(Guid id, Guid empresaId, Guid facturaId) : base(id, empresaId) { Mrn = null!; FacturaId = facturaId; }

    public Guid FacturaId { get; private set; }

    /// <summary>Número de referencia del movimiento (MRN): 18 caracteres, p. ej. 26ES00461120012345.</summary>
    public string Mrn { get; private set; }

    public DateOnly FechaDespacho { get; private set; }

    /// <summary>Salida efectiva de la UE (la confirma la aduana de salida). Nula mientras no se conoce.</summary>
    public DateOnly? FechaSalida { get; private set; }

    /// <summary>Aduana de exportación o de salida (código de la oficina o nombre).</summary>
    public string? Aduana { get; private set; }

    public string? Observaciones { get; private set; }

    public static Resultado<DespachoAduanero> Crear(Guid empresaId, Guid facturaId, string? mrn, DateOnly fechaDespacho, DateOnly? fechaSalida,
        string? aduana, string? observaciones)
    {
        var d = new DespachoAduanero(Guid.NewGuid(), empresaId, facturaId);
        var r = d.Modificar(mrn, fechaDespacho, fechaSalida, aduana, observaciones);
        return r.EsFallo ? Resultado.Fallo<DespachoAduanero>(r.Error) : Resultado.Ok(d);
    }

    public Resultado Modificar(string? mrn, DateOnly fechaDespacho, DateOnly? fechaSalida, string? aduana, string? observaciones)
    {
        var m = NormalizarMrn(mrn);
        if (m is null)
        {
            return Resultado.Fallo(Error.Validacion("despacho.mrn", "El MRN tiene 18 caracteres: año (2 dígitos), país (2 letras) y 14 letras o números."));
        }

        if (fechaSalida is { } s && s < fechaDespacho)
        {
            return Resultado.Fallo(Error.Validacion("despacho.fecha_salida", "La salida no puede ser anterior al despacho."));
        }

        Mrn = m;
        FechaDespacho = fechaDespacho;
        FechaSalida = fechaSalida;
        Aduana = Transportista.Texto(aduana, 60);
        Observaciones = Transportista.Texto(observaciones, 300);
        return Resultado.Ok();
    }

    public static string? NormalizarMrn(string? mrn)
    {
        var m = new string((mrn ?? string.Empty).Where(char.IsAsciiLetterOrDigit).ToArray()).ToUpperInvariant();
        return m.Length == LongitudMrn && char.IsAsciiDigit(m[0]) && char.IsAsciiDigit(m[1]) && char.IsAsciiLetter(m[2]) && char.IsAsciiLetter(m[3]) ? m : null;
    }
}
