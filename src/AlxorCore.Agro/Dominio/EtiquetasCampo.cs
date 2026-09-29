using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Agro.Dominio;

/// <summary>
/// Etiqueta SSCC emitida antes de la recepción para ponerla en la finca, si allí hay impresora: el palé o palot sale del
/// campo ya identificado. En la báscula se lee y el palé de entrada se queda con ese SSCC, así que la traza empieza en la
/// parcela. Sale del mismo contador de SSCC que los palés y solo se usa una vez.
/// </summary>
public sealed class EtiquetaCampo : RaizAgregadoEmpresa<Guid>
{
    private EtiquetaCampo(Guid id)
        : base(id, Guid.Empty)
    {
        Sscc = null!;
    }

    private EtiquetaCampo(Guid id, Guid empresaId)
        : base(id, empresaId)
    {
        Sscc = null!;
    }

    public string Sscc { get; private set; }

    public Guid AgricultorId { get; private set; }

    public Guid? ParcelaId { get; private set; }

    public DateTimeOffset EmitidaEn { get; private set; }

    /// <summary>El palé de entrada que la lleva (al confirmar la recepción); null mientras no se usa.</summary>
    public Guid? PaleId { get; private set; }

    public bool Usada => PaleId is not null;

    public static Resultado<EtiquetaCampo> Emitir(Guid empresaId, string sscc, Guid agricultorId, Guid? parcelaId, DateTimeOffset ahora)
    {
        if (sscc?.Length != 18 || !ReglasAgro.Gs1Valido(sscc))
        {
            return Resultado.Fallo<EtiquetaCampo>(Error.Validacion("pale.sscc", "El SSCC debe tener 18 dígitos con el dígito de control GS1 correcto."));
        }

        return Resultado.Ok(new EtiquetaCampo(Guid.NewGuid(), empresaId) { Sscc = sscc, AgricultorId = agricultorId, ParcelaId = parcelaId, EmitidaEn = ahora });
    }

    public Resultado Usar(Guid paleId)
    {
        if (Usada)
        {
            return Resultado.Fallo(Error.Conflicto("etiqueta_campo.usada", $"La etiqueta {Sscc} ya se usó en otra recepción."));
        }

        PaleId = paleId;
        return Resultado.Ok();
    }

    /// <summary>
    /// El SSCC de una lectura (18 dígitos, «(00)…», «00…» de 20 dígitos o el código GS1-128 completo), o null si no lo
    /// es. Sirve para reconocer la etiqueta de campo en la serie leída en la báscula.
    /// </summary>
    public static string? SsccDeLectura(string? lectura)
    {
        if (string.IsNullOrWhiteSpace(lectura))
        {
            return null;
        }

        var t = lectura.Trim();
        if (t.StartsWith("]C1", StringComparison.Ordinal))
        {
            t = t[3..];
        }

        t = t.Replace("(00)", "00", StringComparison.Ordinal);
        var digitos = new string(t.Where(char.IsAsciiDigit).ToArray());
        var candidato = digitos.Length switch
        {
            18 => digitos,
            >= 20 when digitos.StartsWith("00", StringComparison.Ordinal) => digitos[2..20],
            _ => null,
        };
        return candidato is not null && ReglasAgro.Gs1Valido(candidato) ? candidato : null;
    }
}
