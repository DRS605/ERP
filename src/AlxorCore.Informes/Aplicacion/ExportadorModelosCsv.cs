using System.Globalization;
using System.Text;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>
/// Genera CSV con las <b>casillas</b> de los modelos que se presentan por formulario (303, 390, 111),
/// para pasárselas a la gestoría. Formato español: separador «;», decimales con coma.
/// </summary>
public static class ExportadorModelosCsv
{
    public static string Modelo303(Modelo303Dto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = Cabecera($"Modelo 303 · IVA · {m.Anio} · {m.Trimestre}T");
        Fila(sb, "IVA devengado — base", m.IvaDevengadoBase);
        Fila(sb, "IVA devengado — cuota", m.IvaDevengadoCuota);
        Fila(sb, "IVA deducible — base", m.IvaDeducibleBase);
        Fila(sb, "IVA deducible — cuota", m.IvaDeducibleCuota);
        Fila(sb, "Resultado", m.Resultado);
        return sb.ToString();
    }

    public static string Modelo390(Modelo390Dto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = Cabecera($"Modelo 390 · Resumen anual de IVA · {m.Anio}");
        Fila(sb, "IVA devengado — base", m.IvaDevengadoBase);
        Fila(sb, "IVA devengado — cuota", m.IvaDevengadoCuota);
        Fila(sb, "IVA deducible — base", m.IvaDeducibleBase);
        Fila(sb, "IVA deducible — cuota", m.IvaDeducibleCuota);
        Fila(sb, "Resultado anual", m.Resultado);
        return sb.ToString();
    }

    public static string Modelo111(Modelo111Dto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = Cabecera($"Modelo 111 · Retenciones IRPF · {m.Anio} · {m.Trimestre}T");
        sb.Append("Nº de perceptores;").Append(m.NumeroPerceptores.ToString(CultureInfo.InvariantCulture)).Append('\n');
        Fila(sb, "Base de las percepciones", m.BasePercepciones);
        Fila(sb, "Retenciones", m.Retenciones);
        Fila(sb, "Resultado a ingresar", m.TotalAIngresar);
        return sb.ToString();
    }

    private static StringBuilder Cabecera(string titulo)
    {
        var sb = new StringBuilder();
        sb.Append(Escapar(titulo)).Append('\n');
        sb.Append("Concepto;Importe\n");
        return sb;
    }

    private static void Fila(StringBuilder sb, string concepto, decimal importe) =>
        sb.Append(Escapar(concepto)).Append(';').Append(Redondeo.Formatear(importe)).Append('\n');

    private static string Escapar(string valor) =>
        valor.Contains(';', StringComparison.Ordinal) || valor.Contains('"', StringComparison.Ordinal)
            ? "\"" + valor.Replace("\"", "\"\"", StringComparison.Ordinal) + "\""
            : valor;
}
