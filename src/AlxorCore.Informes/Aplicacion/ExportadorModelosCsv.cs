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

    /// <summary>Casillas del 115 (alquileres) o el 123 (capital mobiliario) del trimestre.</summary>
    public static string Resumen(ResumenRetencionesDto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = Cabecera($"Modelo {m.Modelo} · Retenciones {(m.Modelo == "115" ? "sobre alquileres" : "sobre capital mobiliario")} · {m.Anio} · {m.Trimestre}T");
        sb.Append("Nº de perceptores;").Append(m.NumeroPerceptores.ToString(CultureInfo.InvariantCulture)).Append('\n');
        Fila(sb, "Base de las retenciones", m.BaseRetenciones);
        Fila(sb, "Retenciones", m.Retenciones);
        Fila(sb, "Resultado a ingresar", m.TotalAIngresar);
        return sb.ToString();
    }

    /// <summary>Detalle por perceptor del 180 (arrendadores) o el 193 (socios), con los que no tienen NIF al final.</summary>
    public static string Anual(AnualRetencionesDto m)
    {
        ArgumentNullException.ThrowIfNull(m);
        var sb = new StringBuilder();
        sb.Append(Escapar($"Modelo {m.Modelo} · Resumen anual · {m.Anio}")).Append('\n');
        sb.Append("NIF;Perceptor;Provincia;Clave;Base;Retención;Observaciones\n");
        foreach (var (p, sinNif) in m.Perceptores.Select(p => (p, false)).Concat(m.PerceptoresSinNif.Select(p => (p, true))))
        {
            sb.Append(Escapar(p.Nif ?? string.Empty)).Append(';').Append(Escapar(p.Nombre)).Append(';').Append(Escapar(p.Provincia ?? string.Empty)).Append(';')
                .Append(p.Clave).Append(';').Append(Redondeo.Formatear(p.BasePercepciones)).Append(';').Append(Redondeo.Formatear(p.Retenciones)).Append(';')
                .Append(sinNif ? "Sin NIF: complétalo en la ficha" : string.Empty).Append('\n');
        }

        sb.Append("Total;;;;").Append(Redondeo.Formatear(m.TotalBase)).Append(';').Append(Redondeo.Formatear(m.TotalRetenciones)).Append(";\n");
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
