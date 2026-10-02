using System.Globalization;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>Periodicidad de las autoliquidaciones del IVA/IGIC y de las retenciones.</summary>
public enum PeriodicidadImpuesto
{
    Trimestral = 1,
    Mensual = 2,
}

/// <summary>Administrador, apoderado o representante legal de la empresa.</summary>
public sealed record AdministradorEmpresa(string Nombre, string? Nif, string? Cargo);

/// <summary>Un modelo tributario que la empresa puede presentar.</summary>
public sealed record ModeloFiscal(string Codigo, string Nombre, string Tipo, string Descripcion);

/// <summary>Vencimiento de una declaración: qué modelo, de qué periodo y hasta qué día se presenta.</summary>
public sealed record VencimientoFiscal(string Modelo, string Nombre, string Periodo, DateOnly Desde, DateOnly Hasta);

/// <summary>
/// Ficha fiscal de la empresa: datos de identificación que no son la razón social (nombre comercial, CNAE, IAE,
/// constitución, datos registrales, administradores), cómo tributa (periodicidad, gran empresa, REDEME, criterio de caja,
/// SII) y qué modelos presenta. La prorrata no está aquí: es de cada ejercicio (<see cref="ProrrataEjercicio"/>).
/// </summary>
public sealed record PerfilFiscal
{
    public const int LongitudMaximaTexto = 200;
    public const int LongitudMaximaRegistro = 300;

    public string? NombreComercial { get; init; }
    public string? Cnae { get; init; }
    public string? EpigrafeIae { get; init; }
    public DateOnly? FechaConstitucion { get; init; }
    public DateOnly? FechaInicioActividad { get; init; }

    /// <summary>Mes en que empieza el ejercicio (1 = enero, el año natural).</summary>
    public int MesInicioEjercicio { get; init; } = 1;

    /// <summary>Registro Mercantil, tomo, folio, hoja e inscripción (las sociedades lo ponen en sus facturas).</summary>
    public string? DatosRegistrales { get; init; }

    public IReadOnlyList<AdministradorEmpresa> Administradores { get; init; } = [];

    public PeriodicidadImpuesto Periodicidad { get; init; } = PeriodicidadImpuesto.Trimestral;

    /// <summary>Volumen de operaciones de más de 6 010 121,04 € el año anterior: declara mensualmente y está en el SII.</summary>
    public bool GranEmpresa { get; init; }

    /// <summary>Inscrita en el registro de devolución mensual: declara cada mes y está en el SII.</summary>
    public bool Redeme { get; init; }

    /// <summary>Régimen especial del criterio de caja (art. 163 decies LIVA).</summary>
    public bool CriterioCaja { get; init; }

    /// <summary>Lleva los libros de IVA por el Suministro Inmediato de Información.</summary>
    public bool Sii { get; init; }

    /// <summary>Desde cuándo está en el SII (las facturas anteriores no se envían).</summary>
    public DateOnly? FechaAltaSii { get; init; }

    /// <summary>
    /// Tiene actividad en los dos territorios con el mismo NIF (establecimientos en la Península o Baleares y en Canarias):
    /// factura con IVA las operaciones de un lado y con IGIC las del otro, y presenta los modelos de los dos impuestos
    /// (303/390 a la AEAT y 420/425 a la Agencia Tributaria Canaria).
    /// </summary>
    public bool OperaEnAmbosTerritorios { get; init; }

    /// <summary>Códigos de los modelos que presenta (303, 111…). Vacío: no se ha indicado.</summary>
    public IReadOnlyList<string> Modelos { get; init; } = [];

    public static PerfilFiscal Vacio { get; } = new();

    /// <summary>Modelos que el programa sabe preparar o recordar.</summary>
    public static IReadOnlyList<ModeloFiscal> Catalogo { get; } =
    [
        new("303", "IVA · autoliquidación", "Periodico", "IVA devengado y deducible del trimestre o del mes."),
        new("390", "IVA · resumen anual", "Anual", "Resumen de los 303 del año. No lo presentan quienes están en el SII."),
        new("420", "IGIC · autoliquidación", "Periodico", "IGIC de Canarias del trimestre o del mes."),
        new("425", "IGIC · resumen anual", "Anual", "Resumen de los 420 del año."),
        new("349", "Operaciones intracomunitarias", "Periodico", "Entregas y adquisiciones a otros países de la UE."),
        new("347", "Operaciones con terceros", "Anual", "Clientes y proveedores de más de 3 005,06 € al año. No lo presentan quienes están en el SII."),
        new("111", "Retenciones de trabajo y profesionales", "Periodico", "IRPF retenido a empleados, profesionales y agricultores."),
        new("190", "Retenciones de trabajo · resumen anual", "Anual", "Resumen de los 111 del año, por perceptor."),
        new("115", "Retenciones de alquileres", "Periodico", "IRPF retenido en los alquileres de locales."),
        new("180", "Retenciones de alquileres · resumen anual", "Anual", "Resumen de los 115 del año, por arrendador."),
        new("123", "Retenciones del capital mobiliario", "Periodico", "Retenciones de intereses, dividendos y retornos a socios."),
        new("193", "Capital mobiliario · resumen anual", "Anual", "Resumen de los 123 del año, por perceptor."),
        new("130", "IRPF · pago fraccionado (estimación directa)", "Trimestral", "Autónomos en estimación directa."),
        new("131", "IRPF · pago fraccionado (módulos)", "Trimestral", "Autónomos en estimación objetiva."),
        new("200", "Impuesto sobre sociedades", "Anual", "Declaración anual de las sociedades."),
        new("202", "Sociedades · pagos fraccionados", "Fraccionado", "Pagos a cuenta de abril, octubre y diciembre."),
        new("184", "Entidades en atribución de rentas", "Anual", "Comunidades de bienes y sociedades civiles sin personalidad jurídica."),
        new("232", "Operaciones vinculadas", "Anual", "Operaciones con socios, administradores y empresas del grupo por encima de los umbrales."),
    ];

    /// <summary>Valida y normaliza la ficha.</summary>
    public Resultado<PerfilFiscal> Validar()
    {
        string? Limpio(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : s.Trim()[..Math.Min(s.Trim().Length, max)];

        var cnae = Limpio(Cnae, 10)?.Replace(".", "", StringComparison.Ordinal);
        if (cnae is not null && (cnae.Length != 4 || !cnae.All(char.IsDigit)))
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.cnae", "El CNAE son cuatro cifras (por ejemplo 4631, comercio al por mayor de frutas)."));
        }

        if (MesInicioEjercicio is < 1 or > 12)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.mes_ejercicio", "El ejercicio empieza en un mes del 1 al 12."));
        }

        if ((GranEmpresa || Redeme) && Periodicidad != PeriodicidadImpuesto.Mensual)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.periodicidad",
                "Las grandes empresas y las inscritas en el REDEME declaran el IVA cada mes: cambia la periodicidad a mensual."));
        }

        if ((GranEmpresa || Redeme) && !Sii)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.sii_obligatorio",
                "Las grandes empresas y las inscritas en el REDEME tienen que llevar los libros por el SII."));
        }

        if (Sii && FechaAltaSii is null)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.fecha_sii", "Indica desde qué fecha está en el SII."));
        }

        var modelos = (Modelos ?? []).Select(m => (m ?? string.Empty).Trim()).Where(m => m.Length > 0).Distinct(StringComparer.Ordinal).ToList();
        if (modelos.FirstOrDefault(m => Catalogo.All(c => c.Codigo != m)) is { } desconocido)
        {
            return Resultado.Fallo<PerfilFiscal>(Error.Validacion("perfil.modelo", $"El modelo {desconocido} no está en la lista."));
        }

        var administradores = (Administradores ?? []).Where(a => !string.IsNullOrWhiteSpace(a.Nombre))
            .Select(a => new AdministradorEmpresa(a.Nombre.Trim()[..Math.Min(a.Nombre.Trim().Length, LongitudMaximaTexto)],
                string.IsNullOrWhiteSpace(a.Nif) ? null : a.Nif.Trim().ToUpperInvariant(), Limpio(a.Cargo, 80)))
            .ToList();

        return Resultado.Ok(this with
        {
            NombreComercial = Limpio(NombreComercial, LongitudMaximaTexto),
            Cnae = cnae,
            EpigrafeIae = Limpio(EpigrafeIae, 10),
            DatosRegistrales = Limpio(DatosRegistrales, LongitudMaximaRegistro),
            FechaAltaSii = Sii ? FechaAltaSii : null,
            Modelos = Catalogo.Select(c => c.Codigo).Where(modelos.Contains).ToList(),
            Administradores = administradores,
        });
    }

    /// <summary>
    /// Modelos que suele presentar una empresa como esta: el impuesto indirecto de su territorio (303 o 420 y su resumen),
    /// el 347 y el 390 si no está en el SII, las retenciones de trabajo y, según sea sociedad o persona física, el impuesto
    /// sobre sociedades o los pagos del IRPF. Es una propuesta: cada empresa marca los suyos.
    /// </summary>
    public static IReadOnlyList<string> Sugeridos(string? nif, TerritorioFiscal territorio, bool sii, bool ambosTerritorios = false)
    {
        var lista = new List<string>();
        if (territorio == TerritorioFiscal.Canarias || ambosTerritorios)
        {
            lista.AddRange(["420", "425"]);
        }

        if (territorio != TerritorioFiscal.Canarias || ambosTerritorios)
        {
            lista.Add("303");
            if (!sii)
            {
                lista.Add("390");
            }
        }

        if (!sii)
        {
            lista.Add("347");
        }

        lista.AddRange(["111", "190"]);
        var letra = string.IsNullOrEmpty(nif) ? ' ' : char.ToUpperInvariant(nif.Trim()[0]);
        if ("ABCDFGNPQRSUVW".Contains(letra, StringComparison.Ordinal))
        {
            lista.AddRange(["200", "202"]);
        }
        else if (letra is 'E' or 'J' or 'H')
        {
            lista.Add("184");
        }
        else
        {
            lista.Add("130");
        }

        return Catalogo.Select(c => c.Codigo).Where(lista.Contains).ToList();
    }

    /// <summary>
    /// Vencimientos del año: las declaraciones de los modelos que presenta cuyo plazo acaba ese año, con los plazos
    /// generales de la AEAT (un plazo que acaba en sábado o domingo pasa al lunes). Los festivos no se tienen en cuenta.
    /// </summary>
    public IReadOnlyList<VencimientoFiscal> Calendario(int anio)
    {
        var lista = new List<VencimientoFiscal>();
        var mensual = Periodicidad == PeriodicidadImpuesto.Mensual;
        foreach (var codigo in Modelos)
        {
            var m = Catalogo.First(c => c.Codigo == codigo);
            void Add(string periodo, DateOnly desde, DateOnly hasta) => lista.Add(new VencimientoFiscal(codigo, m.Nombre, periodo, desde, Habil(hasta)));

            switch (m.Tipo)
            {
                case "Periodico" when mensual:
                    // El mes m se declara el mes siguiente: el 303 y el 420 hasta el último día; las demás hasta el 20.
                    // Julio, hasta septiembre.
                    for (var mes = 0; mes < 12; mes++)
                    {
                        var (a, me) = mes == 0 ? (anio - 1, 12) : (anio, mes);
                        var siguiente = new DateOnly(a, me, 1).AddMonths(me == 7 ? 2 : 1);
                        var hasta = codigo is "303" or "420"
                            ? siguiente.AddMonths(1).AddDays(-1)
                            : new DateOnly(siguiente.Year, siguiente.Month, 20);
                        if (codigo is "303" or "420" && me == 12)
                        {
                            hasta = new DateOnly(siguiente.Year, 1, 30);
                        }

                        Add(NombreMes(me) + " " + a.ToString(CultureInfo.InvariantCulture), new DateOnly(siguiente.Year, siguiente.Month, 1), hasta);
                    }

                    break;
                case "Periodico" or "Trimestral":
                    Add($"4T {anio - 1}", new DateOnly(anio, 1, 1), new DateOnly(anio, 1, codigo is "111" or "115" or "123" ? 20 : 30));
                    Add($"1T {anio}", new DateOnly(anio, 4, 1), new DateOnly(anio, 4, 20));
                    Add($"2T {anio}", new DateOnly(anio, 7, 1), new DateOnly(anio, 7, 20));
                    Add($"3T {anio}", new DateOnly(anio, 10, 1), new DateOnly(anio, 10, 20));
                    break;
                case "Fraccionado":
                    Add($"1P {anio}", new DateOnly(anio, 4, 1), new DateOnly(anio, 4, 20));
                    Add($"2P {anio}", new DateOnly(anio, 10, 1), new DateOnly(anio, 10, 20));
                    Add($"3P {anio}", new DateOnly(anio, 12, 1), new DateOnly(anio, 12, 20));
                    break;
                case "Anual":
                    var ejercicio = $"{anio - 1}";
                    switch (codigo)
                    {
                        case "390" or "425":
                            Add(ejercicio, new DateOnly(anio, 1, 1), new DateOnly(anio, 1, 30));
                            break;
                        case "190" or "180" or "193" or "184":
                            Add(ejercicio, new DateOnly(anio, 1, 1), new DateOnly(anio, 1, 31));
                            break;
                        case "347":
                            Add(ejercicio, new DateOnly(anio, 2, 1), new DateOnly(anio, 3, 1).AddDays(-1));
                            break;
                        case "200" or "232":
                            // Sociedades: los 25 días siguientes a los seis meses del cierre (25 de julio en el año natural).
                            // Operaciones vinculadas: el mes siguiente a los diez meses del cierre (noviembre en el año natural).
                            for (var y = anio - 1; y <= anio; y++)
                            {
                                var cierre = new DateOnly(y, MesInicioEjercicio, 1).AddDays(-1);
                                var periodo = MesInicioEjercicio == 1 ? cierre.Year.ToString(CultureInfo.InvariantCulture) : $"cierre {cierre:dd/MM/yyyy}";
                                if (codigo == "200")
                                {
                                    var inicio = cierre.AddDays(1).AddMonths(6);
                                    Add(periodo, inicio, inicio.AddDays(24));
                                }
                                else
                                {
                                    var inicio = cierre.AddDays(1).AddMonths(10);
                                    Add(periodo, inicio, inicio.AddMonths(1).AddDays(-1));
                                }
                            }

                            break;
                    }

                    break;
            }
        }

        return lista.Where(v => v.Hasta.Year == anio).OrderBy(v => v.Hasta).ThenBy(v => v.Modelo, StringComparer.Ordinal).ToList();
    }

    private static DateOnly Habil(DateOnly d) => d.DayOfWeek switch
    {
        DayOfWeek.Saturday => d.AddDays(2),
        DayOfWeek.Sunday => d.AddDays(1),
        _ => d,
    };

    private static readonly string[] Meses =
        ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

    private static string NombreMes(int mes) => Meses[mes - 1];
}
