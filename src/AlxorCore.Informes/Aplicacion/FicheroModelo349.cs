namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Genera el fichero telemático del <b>modelo 349</b> (operaciones intracomunitarias) según el diseño
/// de registro de la AEAT: registros de 500 posiciones, ISO-8859-1, un registro de declarante
/// (tipo 1, con el período) seguido de un registro por operador intracomunitario (tipo 2).
///
/// <para><b>Nota de exactitud:</b> las convenciones (500 pos., ISO-8859-1, numéricos con 2 decimales
/// implícitos, alfanuméricos en mayúsculas) y las longitudes de los campos del tipo 2 (NIF-IVA 17,
/// nombre 40, clave 1, base 13) están tomadas del diseño oficial. Los offsets de inicio del bloque
/// de totales del tipo 1 siguen el diseño estándar y deben confirmarse; <b>valida el fichero con el
/// servicio de predeclaración de la AEAT</b> antes de presentarlo. Posiciones centralizadas abajo.</para>
/// </summary>
public static class FicheroModelo349
{
    // Cabecera común.
    private const int PosTipoRegistro = 1;
    private const int ModeloDesde = 2, ModeloHasta = 4;
    private const int EjercicioDesde = 5, EjercicioHasta = 8;
    private const int NifDeclaranteDesde = 9, NifDeclaranteHasta = 17;

    // Tipo 1 (declarante / resumen).
    private const int RazonSocialDesde = 18, RazonSocialHasta = 57;
    private const int PosTipoSoporte = 58;
    private const int PeriodoDesde = 136, PeriodoHasta = 137;                 // «1T».."4T"
    private const int NumeroOperadoresDesde = 138, NumeroOperadoresHasta = 146;
    private const int SignoBaseTotal = 147;
    private const int BaseTotalDesde = 148, BaseTotalHasta = 160;             // 11 enteros + 2 decimales

    // Tipo 2 (operador intracomunitario).
    private const int NifIvaDesde = 18, NifIvaHasta = 34;                     // país (2) + número (15)
    private const int NombreDesde = 35, NombreHasta = 74;
    private const int PosClave = 75;
    private const int BaseDesde = 76, BaseHasta = 88;                         // 11 enteros + 2 decimales

    public static byte[] Generar(DeclaranteAeat declarante, Modelo349Dto modelo)
    {
        ArgumentNullException.ThrowIfNull(declarante);
        ArgumentNullException.ThrowIfNull(modelo);

        var registros = new List<RegistroAeat> { Cabecera(declarante, modelo) };
        registros.AddRange(modelo.Operadores.Select(o => Operador(declarante, modelo.Anio, o)));
        return RegistroAeat.AFichero(registros);
    }

    private static RegistroAeat Cabecera(DeclaranteAeat declarante, Modelo349Dto modelo)
    {
        return new RegistroAeat()
            .Car(PosTipoRegistro, '1')
            .Alfa(ModeloDesde, ModeloHasta, "349")
            .Num(EjercicioDesde, EjercicioHasta, modelo.Anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(RazonSocialDesde, RazonSocialHasta, declarante.RazonSocial)
            .Car(PosTipoSoporte, 'T')
            .Alfa(PeriodoDesde, PeriodoHasta, modelo.Periodo)
            .Num(NumeroOperadoresDesde, NumeroOperadoresHasta, modelo.Operadores.Count)
            .Importe(SignoBaseTotal, BaseTotalDesde, BaseTotalHasta, modelo.BaseTotal);
    }

    private static RegistroAeat Operador(DeclaranteAeat declarante, int anio, Modelo349OperadorDto o)
    {
        return new RegistroAeat()
            .Car(PosTipoRegistro, '2')
            .Alfa(ModeloDesde, ModeloHasta, "349")
            .Num(EjercicioDesde, EjercicioHasta, anio)
            .Alfa(NifDeclaranteDesde, NifDeclaranteHasta, declarante.Nif)
            .Alfa(NifIvaDesde, NifIvaHasta, o.NifIva)
            .Alfa(NombreDesde, NombreHasta, o.Nombre)
            .Alfa(PosClave, PosClave, o.Clave)
            .Num(BaseDesde, BaseHasta, (long)System.Math.Round(o.BaseImponible * 100m, System.MidpointRounding.AwayFromZero));
    }
}
