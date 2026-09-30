using System.Globalization;
using System.IO.Compression;
using System.Text;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Paquete para legalizar los libros contables en el Registro Mercantil con Legalia: el libro Diario y el de Inventarios y
/// Cuentas Anuales en PDF, y unas instrucciones. Legalia firma y envía los ficheros; aquí solo se preparan.
/// </summary>
public static class LegalizacionLibros
{
    // Globalización invariante: los importes, con el formato español del núcleo.
    private static readonly CultureInfo Es = CultureInfo.InvariantCulture;

    private static string I(decimal v) => v == 0m ? string.Empty : Redondeo.Formatear(v);

    private static string S(decimal v) => Redondeo.Formatear(v);

    public static byte[] Zip(LibrosLegalizacionDto l, string empresa, string nif, IGeneradorPdfLibro pdf)
    {
        ArgumentNullException.ThrowIfNull(l);
        ArgumentNullException.ThrowIfNull(pdf);
        var periodo = $"Ejercicio {l.Ejercicio} (01/01/{l.Ejercicio} - 31/12/{l.Ejercicio})";
        using var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
        {
            Entrada(zip, $"Libro_Diario_{l.Ejercicio}.pdf", pdf.Generar(Diario(l, empresa, nif, periodo)));
            Entrada(zip, $"Libro_Inventarios_y_Cuentas_Anuales_{l.Ejercicio}.pdf", pdf.Generar(Inventarios(l, empresa, nif, periodo)));
            Entrada(zip, "LEEME.txt", Encoding.UTF8.GetBytes(Leeme(l, empresa, nif)));
        }

        return ms.ToArray();
    }

    private static void Entrada(ZipArchive zip, string nombre, byte[] datos)
    {
        using var s = zip.CreateEntry(nombre, CompressionLevel.Optimal).Open();
        s.Write(datos);
    }

    public static LibroImpreso Diario(LibrosLegalizacionDto l, string empresa, string nif, string periodo)
    {
        var filas = new List<FilaLibro>();
        foreach (var a in l.Diario)
        {
            filas.Add(new FilaLibro([a.Numero.ToString(Es), a.Fecha.ToString("dd/MM/yyyy", Es), a.Concepto, string.Empty, string.Empty, string.Empty], true));
            filas.AddRange(a.Apuntes.Select(p => new FilaLibro([string.Empty, string.Empty, p.Concepto ?? string.Empty,
                $"{p.CuentaCodigo} {l.NombresCuentas.GetValueOrDefault(p.CuentaCodigo, string.Empty)}".Trim(), I(p.Debe), I(p.Haber)])));
        }

        var debe = l.Diario.SelectMany(a => a.Apuntes).Sum(p => p.Debe);
        var haber = l.Diario.SelectMany(a => a.Apuntes).Sum(p => p.Haber);
        filas.Add(new FilaLibro(["", "", "Total del ejercicio", "", S(debe), S(haber)], true));
        var tabla = new TablaLibro(null, ["Asiento", "Fecha", "Concepto", "Cuenta", "Debe", "Haber"], [false, false, false, false, true, true], filas, [0.7f, 0.9f, 3f, 3f, 1.2f, 1.2f]);
        return new LibroImpreso("Libro Diario", empresa, nif, periodo,
            [new SeccionLibro($"Libro Diario · {l.Ejercicio}", $"{l.Diario.Count} asientos, numerados por orden de registro.", [tabla])], Apaisado: true);
    }

    public static LibroImpreso Inventarios(LibrosLegalizacionDto l, string empresa, string nif, string periodo)
    {
        var secciones = new List<SeccionLibro>();
        foreach (var t in l.Trimestres)
        {
            secciones.Add(new SeccionLibro($"Balance de comprobación de sumas y saldos · {t.Trimestre}.º trimestre", $"Saldos a {t.Hasta:dd/MM/yyyy}.", [Sumas(t.Cuentas)]));
        }

        secciones.Add(new SeccionLibro("Inventario de cierre del ejercicio", $"Saldo de cada cuenta de balance a 31/12/{l.Ejercicio}, antes del asiento de cierre.", [Sumas(l.Inventario)]));
        var ca = l.CuentasAnuales;
        secciones.Add(new SeccionLibro("Cuentas anuales · Balance abreviado", null, [Partidas(ca.Balance, l.Ejercicio)]));
        secciones.Add(new SeccionLibro("Cuentas anuales · Cuenta de pérdidas y ganancias abreviada", null, [Partidas(ca.PerdidasGanancias, l.Ejercicio)]));
        return new LibroImpreso("Libro de Inventarios y Cuentas Anuales", empresa, nif, periodo, secciones);
    }

    private static TablaLibro Sumas(IReadOnlyList<SumasSaldosDto> cuentas)
    {
        var filas = cuentas.Select(c => new FilaLibro([c.Cuenta, c.Nombre, I(c.SumaDebe), I(c.SumaHaber), I(c.SaldoDeudor), I(c.SaldoAcreedor)])).ToList();
        filas.Add(new FilaLibro(["", "Totales", S(cuentas.Sum(c => c.SumaDebe)), S(cuentas.Sum(c => c.SumaHaber)), S(cuentas.Sum(c => c.SaldoDeudor)), S(cuentas.Sum(c => c.SaldoAcreedor))], true));
        return new TablaLibro(null, ["Cuenta", "Nombre", "Suma debe", "Suma haber", "Saldo deudor", "Saldo acreedor"], [false, false, true, true, true, true], filas, [0.8f, 3f, 1.1f, 1.1f, 1.1f, 1.1f]);
    }

    private static TablaLibro Partidas(IReadOnlyList<PartidaDepositoDto> partidas, int ejercicio) =>
        new(null, ["Clave", "Partida", ejercicio.ToString(Es), (ejercicio - 1).ToString(Es)], [false, false, true, true],
            partidas.Select(p => new FilaLibro([p.Clave, new string(' ', p.Nivel * 3) + p.Concepto, S(p.Actual), S(p.Anterior)], p.Nivel <= 1)).ToList(), [0.7f, 4f, 1.2f, 1.2f]);

    public static string DepositoCsv(ModeloDepositoDto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = new StringBuilder();
        sb.Append("Apartado;Clave;Partida;").Append(m.Ejercicio).Append(';').Append(m.Ejercicio - 1).Append('\n');
        foreach (var (apartado, lista) in new[] { ("Balance", m.Balance), ("Pérdidas y ganancias", m.PerdidasGanancias) })
        {
            foreach (var p in lista)
            {
                sb.Append(apartado).Append(';').Append(p.Clave).Append(';').Append(p.Concepto.Replace(";", ",", StringComparison.Ordinal)).Append(';')
                    .Append(S(p.Actual)).Append(';').Append(S(p.Anterior)).Append('\n');
            }
        }

        return sb.ToString();
    }

    private static string Leeme(LibrosLegalizacionDto l, string empresa, string nif) => $"""
        Legalización de libros · {empresa} ({nif}) · ejercicio {l.Ejercicio}

        Contenido:
          - Libro_Diario_{l.Ejercicio}.pdf: todos los asientos del ejercicio ({l.Diario.Count}), en orden.
          - Libro_Inventarios_y_Cuentas_Anuales_{l.Ejercicio}.pdf: balances de comprobación trimestrales, inventario de
            cierre y cuentas anuales (balance y pérdidas y ganancias abreviados, con el ejercicio anterior).

        Cómo presentarlo:
          1. Abre Legalia 2 (Colegio de Registradores), crea la presentación del ejercicio {l.Ejercicio} y da de alta los
             dos libros con su número correlativo (el siguiente al último legalizado de cada uno).
          2. Añade a cada libro su PDF, genera la instancia y firma con el certificado de la sociedad o del representante.
          3. Envíalo al Registro Mercantil de tu domicilio social. El plazo es de cuatro meses desde el cierre del ejercicio.

        {(l.EjercicioCerrado ? "El ejercicio está cerrado." : "Atención: el ejercicio aún no está cerrado; cierra antes para que el diario lleve la regularización y el cierre.")}
        Los libros de actas y de socios (cooperativas) se preparan desde su módulo.
        """;
}
