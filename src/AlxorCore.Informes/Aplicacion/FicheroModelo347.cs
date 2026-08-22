namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Genera el fichero telemático del <b>modelo 347</b> (declaración anual de operaciones con terceros)
/// según el diseño de registro de la AEAT: registros de 500 posiciones, ISO-8859-1, un registro de
/// declarante (tipo 1) seguido de un registro por tercero declarado (tipo 2).
///
/// <para><b>Nota de exactitud:</b> las convenciones y las posiciones del importe anual/clave están
/// implementadas según el diseño oficial. Los offsets del desglose por trimestres siguen el patrón
/// documentado (signo + importe por trimestre). Valida el fichero con el servicio de predeclaración
/// de la AEAT antes de presentarlo (las posiciones están centralizadas como constantes).</para>
/// </summary>
public static class FicheroModelo347
{
    // Cabecera común (tipos 1 y 2 comparten 1..17).
    private const int PosTipoRegistro = 1;
    private const int ModeloDesde = 2, ModeloHasta = 4;
    private const int EjercicioDesde = 5, EjercicioHasta = 8;
    private const int NifDeclaranteDesde = 9, NifDeclaranteHasta = 17;

    // Tipo 1 (declarante / resumen).
    private const int RazonSocialDesde = 18, RazonSocialHasta = 57;
    private const int PosTipoSoporte = 58;
    private const int NumeroDeclaradosDesde = 136, NumeroDeclaradosHasta = 144;
    private const int SignoImporteTotal = 145;
    private const int ImporteTotalDesde = 146, ImporteTotalHasta = 160; // 13 enteros + 2 decimales

    // Tipo 2 (declarado / operación con tercero).
    private const int NifDeclaradoDesde = 18, NifDeclaradoHasta = 26;
    private const int NombreDeclaradoDesde = 36, NombreDeclaradoHasta = 75;
    private const int ProvinciaDesde = 76, ProvinciaHasta = 77;
    private const int SignoImporteAnual = 83;
    private const int ImporteAnualDesde = 84, ImporteAnualHasta = 98; // 13 enteros + 2 decimales (verificado)
    private const int PosClaveOperacion = 99;                          // A/B (verificado)
    // Desglose trimestral: patrón «signo(1) + importe(15 = 13+2)» por trimestre a partir de la 100.
    private const int Trimestre1Signo = 100, Trimestre1Desde = 101, Trimestre1Hasta = 115;
    private const int Trimestre2Signo = 116, Trimestre2Desde = 117, Trimestre2Hasta = 131;
    private const int Trimestre3Signo = 132, Trimestre3Desde = 133, Trimestre3Hasta = 147;
    private const int Trimestre4Signo = 148, Trimestre4Desde = 149, Trimestre4Hasta = 163;

    public static byte[] Generar(DeclaranteAeat declarante, int anio, IReadOnlyList<Modelo347LineaDto> lineas)
    {
        ArgumentNullException.ThrowIfNull(declarante);
        ArgumentNullException.ThrowIfNull(lineas);

        var registros = new List<RegistroAeat> { Cabecera(declarante, anio, lineas) };
        registros.AddRange(lineas.Select(l => Declarado(declarante, anio, l)));
        return RegistroAeat.AFichero(registros);
    }

    private static RegistroAeat Cabecera(DeclaranteAeat declarante, int anio, IReadOnlyList<Modelo347LineaDto> lineas)
    {
        var total = lineas.Sum(l => l.ImporteAnual);
        return new RegistroAeat()
            .Car(PosTipoRegistro, '1')
            .Alfa(ModeloDesde, ModeloHasta, "347")
            .Num(EjercicioDesde, EjercicioHasta, anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(RazonSocialDesde, RazonSocialHasta, declarante.RazonSocial)
            .Car(PosTipoSoporte, 'T')
            .Num(NumeroDeclaradosDesde, NumeroDeclaradosHasta, lineas.Count)
            .Importe(SignoImporteTotal, ImporteTotalDesde, ImporteTotalHasta, total);
    }

    private static RegistroAeat Declarado(DeclaranteAeat declarante, int anio, Modelo347LineaDto l)
    {
        return new RegistroAeat()
            .Car(PosTipoRegistro, '2')
            .Alfa(ModeloDesde, ModeloHasta, "347")
            .Num(EjercicioDesde, EjercicioHasta, anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(NifDeclaradoDesde, NifDeclaradoHasta, l.Nif)
            .Alfa(NombreDeclaradoDesde, NombreDeclaradoHasta, l.Nombre)
            .Alfa(ProvinciaDesde, ProvinciaHasta, ProvinciasAeat.NoResidenteODesconocida)
            .Importe(SignoImporteAnual, ImporteAnualDesde, ImporteAnualHasta, l.ImporteAnual)
            .Alfa(PosClaveOperacion, PosClaveOperacion, l.ClaveOperacion)
            .Importe(Trimestre1Signo, Trimestre1Desde, Trimestre1Hasta, l.T1)
            .Importe(Trimestre2Signo, Trimestre2Desde, Trimestre2Hasta, l.T2)
            .Importe(Trimestre3Signo, Trimestre3Desde, Trimestre3Hasta, l.T3)
            .Importe(Trimestre4Signo, Trimestre4Desde, Trimestre4Hasta, l.T4);
    }
}
