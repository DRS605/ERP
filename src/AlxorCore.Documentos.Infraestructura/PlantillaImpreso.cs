using AlxorCore.Organizacion.Aplicacion.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>
/// Utilidades compartidas para pintar la <b>plantilla de documentos</b> de la empresa (logo, color
/// corporativo, dirección, contacto y texto de pie) en los PDF de factura, ticket y presupuesto.
/// </summary>
internal static class PlantillaImpreso
{
    /// <summary>Cian de ALXOR usado cuando la empresa no ha fijado un color propio.</summary>
    public const string ColorPorDefecto = "#0EA5B7";

    public static Color ColorMarca(EmpresaDto emisor) =>
        string.IsNullOrWhiteSpace(emisor.ColorPrincipal) ? Color.FromHex(ColorPorDefecto) : Color.FromHex(emisor.ColorPrincipal);

    /// <summary>Dirección en una línea («Calle, 28001 Madrid (Madrid)»), o null si no hay dirección.</summary>
    public static string? LineaDireccion(EmpresaDto e)
    {
        var partes = new List<string>();
        if (!string.IsNullOrWhiteSpace(e.Calle)) partes.Add(e.Calle);
        var loc = string.Join(" ", new[] { e.CodigoPostal, e.Poblacion }.Where(s => !string.IsNullOrWhiteSpace(s)));
        if (!string.IsNullOrWhiteSpace(e.Provincia) && !string.IsNullOrWhiteSpace(loc)) loc += $" ({e.Provincia})";
        if (!string.IsNullOrWhiteSpace(loc)) partes.Add(loc);
        return partes.Count == 0 ? null : string.Join(", ", partes);
    }

    /// <summary>Contacto en una línea (teléfono · web · correo), o null si no hay ninguno.</summary>
    public static string? LineaContacto(EmpresaDto e)
    {
        var partes = new[] { e.Telefono, e.Web, e.EmailContacto }.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
        return partes.Count == 0 ? null : string.Join(" · ", partes);
    }

    /// <summary>Pinta la identidad del emisor en la cabecera A4 (logo o razón social, NIF, dirección y contacto).</summary>
    public static void EscribirEmisor(ColumnDescriptor col, EmpresaDto e, Color color)
    {
        if (e.LogoPng is { Length: > 0 })
        {
            col.Item().Height(52).AlignLeft().Image(e.LogoPng).FitHeight();
            col.Item().PaddingTop(4).Text(e.RazonSocial).SemiBold().FontSize(11);
        }
        else
        {
            col.Item().Text(e.RazonSocial).Bold().FontSize(16).FontColor(color);
        }

        col.Item().Text($"NIF: {e.Nif}").FontSize(9);
        var dir = LineaDireccion(e);
        if (dir is not null) col.Item().Text(dir).FontSize(9);
        var contacto = LineaContacto(e);
        if (contacto is not null) col.Item().Text(contacto).FontSize(9).FontColor(Colors.Grey.Darken1);
    }

    /// <summary>Pie de página: el texto configurado por la empresa, o el pie por defecto de ALXOR.</summary>
    public static void EscribirPie(TextDescriptor texto, EmpresaDto e)
    {
        if (!string.IsNullOrWhiteSpace(e.TextoPie))
        {
            texto.Span(e.TextoPie).FontColor(Colors.Grey.Darken1).FontSize(8);
        }
        else
        {
            texto.Span("ALXOR Core · ").FontColor(Colors.Grey.Medium);
            texto.Span(e.RazonSocial).FontColor(Colors.Grey.Medium);
        }
    }
}
