using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>
/// Maqueta con QuestPDF cualquier documento comercial (albarán, pedido, liquidación): cabecera con la plantilla de la
/// empresa, tercero y datos del documento, líneas (con o sin precios), totales y leyenda.
/// </summary>
internal sealed class GeneradorPdfDocumentoQuestPdf : IGeneradorPdfDocumento
{
    /// <summary>Cantidad con los decimales que tiene (hasta 3): 6.000 kg, 12,5 cajas.</summary>
    private static string Cantidad(decimal v, string idioma) => TextosImpreso.Numero(idioma, v, v == decimal.Round(v) ? 0 : v * 10 == decimal.Round(v * 10) ? 1 : v * 100 == decimal.Round(v * 100) ? 2 : 3);

    public byte[] Generar(DocumentoImpreso d, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(d);
        var conDescuento = d.Lineas.Any(l => l.Descuento is > 0);
        ArgumentNullException.ThrowIfNull(emisor);
        var color = PlantillaImpreso.ColorMarca(emisor);
        var idioma = IdiomasDocumento.Efectivo(d.Idioma);
        string T(string clave) => TextosImpreso.T(idioma, clave);
        string N(decimal v, int decimales = 2) => TextosImpreso.Numero(idioma, v, decimales);

        return Document.Create(contenedor => contenedor.Page(pagina =>
        {
            pagina.Size(PageSizes.A4);
            pagina.Margin(40);
            pagina.DefaultTextStyle(x => x.FontSize(10));

            pagina.Header().Row(fila =>
            {
                fila.RelativeItem().Column(col => PlantillaImpreso.EscribirEmisor(col, emisor, color, idioma));
                fila.ConstantItem(220).AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text(d.Titulo.ToUpperInvariant()).Bold().FontSize(15).FontColor(color);
                    col.Item().AlignRight().Text(d.Numero);
                    col.Item().AlignRight().Text($"{T("Fecha")}: {d.Fecha:dd/MM/yyyy}");
                });
            });

            pagina.Content().PaddingVertical(15).Column(col =>
            {
                col.Item().PaddingBottom(10).Row(fila =>
                {
                    fila.RelativeItem().Column(t =>
                    {
                        t.Item().Text(d.Tercero.Etiqueta).Bold();
                        t.Item().Text(d.Tercero.Nombre);
                        if (!string.IsNullOrWhiteSpace(d.Tercero.Nif)) t.Item().Text($"{T("NIF")}: {d.Tercero.Nif}").FontSize(9);
                        if (!string.IsNullOrWhiteSpace(d.Tercero.Direccion)) t.Item().Text(d.Tercero.Direccion).FontSize(9);
                    });
                    if (d.Datos is { Count: > 0 })
                    {
                        fila.ConstantItem(220).Column(t =>
                        {
                            foreach (var (etiqueta, valor) in d.Datos)
                            {
                                t.Item().AlignRight().Text(txt =>
                                {
                                    txt.Span(etiqueta + ": ").FontColor(Colors.Grey.Darken1).FontSize(9);
                                    txt.Span(valor).FontSize(9);
                                });
                            }
                        });
                    }
                });

                col.Item().Table(tabla =>
                {
                    tabla.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(5);
                        c.RelativeColumn(1.4f);
                        if (d.Valorado)
                        {
                            c.RelativeColumn(1.3f);
                            if (conDescuento) c.RelativeColumn(0.9f);
                            c.RelativeColumn(1.4f);
                        }
                    });

                    tabla.Header(h =>
                    {
                        IContainer Celda(IContainer c) => c.BorderBottom(1.5f).BorderColor(color).PaddingBottom(3);
                        Celda(h.Cell()).Text(T("Descripción")).Bold().FontColor(color);
                        Celda(h.Cell()).AlignRight().Text(d.TituloCantidad ?? T("Cantidad")).Bold().FontColor(color);
                        if (d.Valorado)
                        {
                            Celda(h.Cell()).AlignRight().Text(T("Precio")).Bold().FontColor(color);
                            if (conDescuento) Celda(h.Cell()).AlignRight().Text(T("Dto.")).Bold().FontColor(color);
                            Celda(h.Cell()).AlignRight().Text(T("Importe")).Bold().FontColor(color);
                        }
                    });

                    foreach (var l in d.Lineas)
                    {
                        tabla.Cell().PaddingTop(3).Column(c =>
                        {
                            c.Item().Text(l.Descripcion);
                            if (!string.IsNullOrWhiteSpace(l.Detalle)) c.Item().Text(l.Detalle).FontSize(8).FontColor(Colors.Grey.Darken2);
                        });
                        tabla.Cell().PaddingTop(3).AlignRight().Text($"{Cantidad(l.Cantidad, idioma)}{(l.Unidad is null ? "" : " " + l.Unidad)}");
                        if (d.Valorado)
                        {
                            tabla.Cell().PaddingTop(3).AlignRight().Text(l.Precio is { } p ? N(p, 4) : "—");
                            if (conDescuento) tabla.Cell().PaddingTop(3).AlignRight().Text(l.Descuento is > 0 ? $"{N(l.Descuento.Value)} %" : "");
                            tabla.Cell().PaddingTop(3).AlignRight().Text(l.Importe is { } i ? N(i) : "—");
                        }
                    }
                });

                if (d.Totales.Count > 0)
                {
                    col.Item().AlignRight().PaddingTop(15).Width(260).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                        });
                        foreach (var total in d.Totales)
                        {
                            if (total.Destacado)
                            {
                                t.Cell().BorderTop(1).BorderColor(color).PaddingTop(4).Text(total.Etiqueta).Bold().FontSize(12).FontColor(color);
                                t.Cell().BorderTop(1).BorderColor(color).PaddingTop(4).AlignRight().Text($"{N(total.Importe)} €").Bold().FontSize(12).FontColor(color);
                            }
                            else
                            {
                                t.Cell().Text(total.Etiqueta);
                                t.Cell().AlignRight().Text($"{N(total.Importe)} €");
                            }
                        }
                    });
                }

                if (!string.IsNullOrWhiteSpace(d.Observaciones))
                {
                    col.Item().PaddingTop(18).Text(txt =>
                    {
                        txt.Span(T("Observaciones") + ": ").SemiBold();
                        txt.Span(d.Observaciones);
                    });
                }

                if (!string.IsNullOrWhiteSpace(d.Leyenda))
                {
                    col.Item().PaddingTop(18).Text(d.Leyenda).FontSize(8).FontColor(Colors.Grey.Darken1);
                }
            });

            pagina.Footer().AlignCenter().Text(texto =>
            {
                PlantillaImpreso.EscribirPie(texto, emisor);
                texto.Span($"   ·   {T("Página")} ").FontSize(8).FontColor(Colors.Grey.Medium);
                texto.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                texto.Span($" {T("de")} ").FontSize(8).FontColor(Colors.Grey.Medium);
                texto.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        })).GeneratePdf();
    }
}
