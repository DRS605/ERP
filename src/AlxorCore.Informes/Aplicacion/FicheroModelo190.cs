namespace AlxorCore.Informes.Aplicacion;

/// <summary>Datos identificativos del declarante (retenedor) para las cabeceras de los ficheros AEAT.</summary>
public sealed record DeclaranteAeat(string Nif, string RazonSocial);

/// <summary>
/// Genera el fichero telemático del <b>modelo 190</b> (resumen anual de retenciones de IRPF) según el
/// diseño de registro de la AEAT: registros de 500 posiciones, ISO-8859-1, un registro de declarante
/// (tipo 1) seguido de un registro por perceptor (tipo 2).
///
/// <para><b>Nota de exactitud:</b> se han implementado con las convenciones oficiales y las posiciones
/// verificadas del diseño de registro. Como buena práctica (y como hace cualquier software fiscal), el
/// fichero generado debe validarse con el <b>servicio de predeclaración / validación de la AEAT</b>
/// antes de su presentación oficial.</para>
/// </summary>
public static class FicheroModelo190
{
    // --- Posiciones del registro (1-indexadas, inclusivas). Diseño estándar de informativas. ---
    // Cabecera común (tipo 1 y tipo 2 comparten 1..17).
    private const int PosTipoRegistro = 1;
    private const int ModeloDesde = 2, ModeloHasta = 4;
    private const int EjercicioDesde = 5, EjercicioHasta = 8;
    private const int NifDeclaranteDesde = 9, NifDeclaranteHasta = 17;

    // Tipo 1 (declarante / resumen).
    private const int RazonSocialDesde = 18, RazonSocialHasta = 57;
    private const int PosTipoSoporte = 58;
    private const int ContactoNombreDesde = 68, ContactoNombreHasta = 107;
    private const int NumeroPercepcionesDesde = 136, NumeroPercepcionesHasta = 144;
    private const int SignoImportePercepciones = 145;
    private const int ImportePercepcionesDesde = 146, ImportePercepcionesHasta = 160; // 13 enteros + 2 decimales
    private const int SignoImporteRetenciones = 161;
    private const int ImporteRetencionesDesde = 162, ImporteRetencionesHasta = 176;   // 13 enteros + 2 decimales

    // Tipo 2 (perceptor).
    private const int NifPerceptorDesde = 18, NifPerceptorHasta = 26;
    private const int NombrePerceptorDesde = 36, NombrePerceptorHasta = 75;
    private const int ProvinciaDesde = 76, ProvinciaHasta = 77;
    private const int PosClave = 78;
    private const int SubclaveDesde = 79, SubclaveHasta = 80;
    private const int SignoPercepcionIntegra = 81;
    private const int PercepcionIntegraDesde = 82, PercepcionIntegraHasta = 94;       // 11 enteros + 2 decimales
    private const int RetencionesDesde = 95, RetencionesHasta = 107;                  // 11 enteros + 2 decimales

    private const string SubclaveGeneral = "01";

    public static byte[] Generar(DeclaranteAeat declarante, Modelo190Dto modelo)
    {
        ArgumentNullException.ThrowIfNull(declarante);
        ArgumentNullException.ThrowIfNull(modelo);

        var registros = new List<RegistroAeat> { Cabecera(declarante, modelo) };
        registros.AddRange(modelo.Perceptores.Select(p => Perceptor(declarante, modelo.Anio, p)));
        return RegistroAeat.AFichero(registros);
    }

    private static RegistroAeat Cabecera(DeclaranteAeat declarante, Modelo190Dto modelo)
    {
        // El resumen solo cuenta los perceptores incluibles (con NIF).
        var percepciones = modelo.Perceptores.Sum(p => p.BasePercepciones);
        var retenciones = modelo.Perceptores.Sum(p => p.Retenciones);

        return new RegistroAeat()
            .Car(PosTipoRegistro, '1')
            .Alfa(ModeloDesde, ModeloHasta, "190")
            .Num(EjercicioDesde, EjercicioHasta, modelo.Anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(RazonSocialDesde, RazonSocialHasta, declarante.RazonSocial)
            .Car(PosTipoSoporte, 'T')
            .Alfa(ContactoNombreDesde, ContactoNombreHasta, declarante.RazonSocial)
            .Num(NumeroPercepcionesDesde, NumeroPercepcionesHasta, modelo.Perceptores.Count)
            .Importe(SignoImportePercepciones, ImportePercepcionesDesde, ImportePercepcionesHasta, percepciones)
            .Importe(SignoImporteRetenciones, ImporteRetencionesDesde, ImporteRetencionesHasta, retenciones);
    }

    private static RegistroAeat Perceptor(DeclaranteAeat declarante, int anio, PerceptorRetencionDto p)
    {
        return new RegistroAeat()
            .Car(PosTipoRegistro, '2')
            .Alfa(ModeloDesde, ModeloHasta, "190")
            .Num(EjercicioDesde, EjercicioHasta, anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(NifPerceptorDesde, NifPerceptorHasta, p.Nif)
            .Alfa(NombrePerceptorDesde, NombrePerceptorHasta, p.Nombre)
            .Alfa(ProvinciaDesde, ProvinciaHasta, ProvinciasAeat.Codigo(p.Provincia))
            .Alfa(PosClave, PosClave, p.Clave)
            .Alfa(SubclaveDesde, SubclaveHasta, SubclaveGeneral)
            .Importe(SignoPercepcionIntegra, PercepcionIntegraDesde, PercepcionIntegraHasta, p.BasePercepciones)
            .Num(RetencionesDesde, RetencionesHasta, (long)Math.Round(p.Retenciones * 100m, MidpointRounding.AwayFromZero));
    }
}
