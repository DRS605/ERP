using System.Globalization;
using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>
/// Etiqueta logística GS1 del palé en A6 (105 × 148 mm): datos en texto arriba y, abajo, los códigos GS1-128 con el
/// contenido (peso neto, cajas y lote) y el SSCC. El módulo es de 0,495 mm o menos si el código no cabe.
/// </summary>
internal sealed class GeneradorEtiquetaLogisticaQuestPdf : IGeneradorEtiquetaLogistica
{
    private const float PuntosPorMm = 72f / 25.4f;
    private const float AnchoUtilMm = 95f;
    private const float ModuloMaximoMm = 0.495f;

    public byte[] Generar(EtiquetaLogistica etiqueta, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(etiqueta);
        ArgumentNullException.ThrowIfNull(emisor);

        var sscc = new List<(string, string)> { ("00", etiqueta.Sscc) };
        var contenido = new List<(string, string)> { Gs1128.PesoNeto(etiqueta.KilosNetos) };
        if (etiqueta.Cajas > 0)
        {
            contenido.Add(("37", etiqueta.Cajas.ToString(CultureInfo.InvariantCulture)));
        }

        if (LoteCodificable(etiqueta.Lote))
        {
            contenido.Add(("10", etiqueta.Lote!));
        }

        var documento = Document.Create(c => c.Page(pagina =>
        {
            pagina.Size(PageSizes.A6);
            pagina.Margin(5, Unit.Millimetre);
            pagina.DefaultTextStyle(x => x.FontSize(9));
            pagina.Content().Column(col =>
            {
                col.Spacing(4);
                col.Item().Text(emisor.RazonSocial).Bold().FontSize(11);
                col.Item().LineHorizontal(1);
                col.Item().Text("SSCC").FontSize(7).FontColor(Colors.Grey.Darken1);
                col.Item().Text(etiqueta.Sscc).Bold().FontSize(14);
                Campo(col, "Producto", etiqueta.Producto);
                Campo(col, "Marca", etiqueta.Marca);
                col.Item().Row(f =>
                {
                    f.RelativeItem().Column(x => Campo(x, "Cajas", etiqueta.Cajas > 0 ? etiqueta.Cajas.ToString(CultureInfo.InvariantCulture) : null));
                    f.RelativeItem().Column(x => Campo(x, "Peso neto", Redondeo.Formatear(etiqueta.KilosNetos, 3) + " kg"));
                });
                col.Item().Row(f =>
                {
                    f.RelativeItem().Column(x => Campo(x, "Lote", etiqueta.Lote));
                    f.RelativeItem().Column(x => Campo(x, "Fecha", etiqueta.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
                });
                Campo(col, "Tipo de palé", etiqueta.TipoPale);
                Campo(col, "Destinatario", etiqueta.Destinatario);
                col.Item().LineHorizontal(1);
                Codigo(col, contenido);
                Codigo(col, sscc);
            });
        }));
        return documento.GeneratePdf();
    }

    /// <summary>El IA 10 admite hasta 20 caracteres del juego GS1 (se omite en el código si el lote no cumple).</summary>
    private static bool LoteCodificable(string? lote) =>
        !string.IsNullOrWhiteSpace(lote) && lote.Length <= 20 && lote.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '.' or '/' or '_');

    private static void Campo(ColumnDescriptor col, string titulo, string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return;
        }

        col.Item().Text(t =>
        {
            t.Span(titulo + ": ").FontSize(7).FontColor(Colors.Grey.Darken1);
            t.Span(valor).SemiBold();
        });
    }

    private static void Codigo(ColumnDescriptor col, IReadOnlyList<(string Ia, string Valor)> elementos)
    {
        var anchuras = Gs1128.Anchuras(Gs1128.Componer(elementos));
        var modulos = anchuras.Sum() + 20;
        var moduloMm = Math.Min(ModuloMaximoMm, AnchoUtilMm / modulos);
        var modulo = moduloMm * PuntosPorMm;
        col.Item().PaddingTop(4).AlignCenter().Height(18, Unit.Millimetre).Row(fila =>
        {
            fila.ConstantItem(10 * modulo);
            for (var i = 0; i < anchuras.Count; i++)
            {
                var barra = fila.ConstantItem(anchuras[i] * modulo);
                if (i % 2 == 0)
                {
                    barra.Background(Colors.Black);
                }
            }

            fila.ConstantItem(10 * modulo);
        });
        col.Item().AlignCenter().Text(Gs1128.Legible(elementos)).FontSize(8);
    }
}
