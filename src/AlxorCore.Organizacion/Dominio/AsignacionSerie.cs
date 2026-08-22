using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Asignación de una serie (prefijo) a un tipo de documento, bien como <b>serie por defecto de la
/// empresa</b>, bien para un <b>cliente</b> o <b>proveedor</b> concreto. Al emitir un documento se
/// resuelve la serie más específica: la del tercero si existe, si no la de la empresa.
///
/// <para>El tercero se guarda en <see cref="TerceroId"/>; para el ámbito <see cref="AmbitoSerie.Empresa"/>
/// vale <see cref="Guid.Empty"/> (así el índice único casa sin nulos).</para>
/// </summary>
public sealed class AsignacionSerie : RaizAgregadoEmpresa<Guid>
{
    private AsignacionSerie(Guid id)
        : base(id, Guid.Empty)
    {
        Prefijo = null!;
    }

    private AsignacionSerie(Guid id, Guid empresaId, TipoDocumento tipoDocumento, AmbitoSerie ambito, Guid terceroId, string prefijo, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        TipoDocumento = tipoDocumento;
        Ambito = ambito;
        TerceroId = terceroId;
        Prefijo = prefijo;
        CreadoEn = ahora;
    }

    public TipoDocumento TipoDocumento { get; private set; }

    public AmbitoSerie Ambito { get; private set; }

    /// <summary>Cliente o proveedor al que aplica; <see cref="Guid.Empty"/> para el ámbito Empresa.</summary>
    public Guid TerceroId { get; private set; }

    public string Prefijo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<AsignacionSerie> Crear(
        Guid empresaId, TipoDocumento tipoDocumento, AmbitoSerie ambito, Guid? terceroId, string? prefijo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var pref = (prefijo ?? string.Empty).Trim().ToUpperInvariant();
        if (pref.Length == 0)
        {
            return Resultado.Fallo<AsignacionSerie>(Error.Validacion("serie.prefijo_vacio", "El prefijo de la serie es obligatorio."));
        }

        if (pref.Length > SerieNumeracion.LongitudMaximaPrefijo)
        {
            return Resultado.Fallo<AsignacionSerie>(Error.Validacion("serie.prefijo_largo", "El prefijo de la serie es demasiado largo."));
        }

        if (ambito == AmbitoSerie.Empresa)
        {
            terceroId = Guid.Empty;
        }
        else if (terceroId is null || terceroId == Guid.Empty)
        {
            return Resultado.Fallo<AsignacionSerie>(Error.Validacion("serie.tercero_obligatorio", "Debes indicar el cliente o proveedor de la serie."));
        }

        return Resultado.Ok(new AsignacionSerie(Guid.NewGuid(), empresaId, tipoDocumento, ambito, terceroId ?? Guid.Empty, pref, reloj.AhoraUtc));
    }
}
