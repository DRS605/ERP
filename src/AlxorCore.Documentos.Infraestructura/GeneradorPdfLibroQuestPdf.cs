using AlxorCore.Documentos.Aplicacion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>PDF de un libro oficial con QuestPDF: hojas numeradas, la empresa y el periodo en cada cabecera.</summary>
internal sealed class GeneradorPdfLibroQuestPdf : IGeneradorPdfLibro
{
    public byte[] Generar(LibroImpreso libro)
    {
        ArgumentNullException.ThrowIfNull(libro);
        return Document.Create(doc =>
        {
            foreach (var seccion in libro.Secciones)
            {
                doc.Page(pagina =>
                {
                    pagina.Size(libro.Apaisado ? PageSizes.A4.Landscape() : PageSizes.A4);
                    pagina.Margin(30);
                    pagina.DefaultTextStyle(x => x.FontSize(8));
                    pagina.Header().PaddingBottom(6).BorderBottom(1).BorderColor(Colors.Grey.Medium).Row(fila =>
                    {
                        fila.RelativeItem().Column(c =>
                        {
                            c.Item().Text(libro.Empresa).Bold().FontSize(10);
                            c.Item().Text($"NIF {libro.Nif}");
                        });
                        fila.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text(libro.Titulo).Bold().FontSize(10);
                            c.Item().AlignRight().Text(libro.Periodo);
                        });
                    });
                    pagina.Content().PaddingTop(8).Column(col =>
                    {
                        col.Spacing(8);
                        col.Item().Text(seccion.Titulo).Bold().FontSize(11);
                        if (!string.IsNullOrWhiteSpace(seccion.Texto))
                        {
                            col.Item().Text(seccion.Texto!);
                        }

                        foreach (var t in seccion.Tablas)
                        {
                            col.Item().Element(e => Tabla(e, t));
                        }
                    });
                    pagina.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Hoja ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                });
            }
        }).GeneratePdf();
    }

    private static void Tabla(IContainer contenedor, TablaLibro t)
    {
        contenedor.Column(col =>
        {
            if (!string.IsNullOrWhiteSpace(t.Titulo))
            {
                col.Item().PaddingBottom(2).Text(t.Titulo!).SemiBold();
            }

            col.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(c =>
                {
                    for (var i = 0; i < t.Columnas.Count; i++)
                    {
                        c.RelativeColumn(t.Anchos is { } a && i < a.Count ? a[i] : 1f);
                    }
                });
                tabla.Header(h =>
                {
                    for (var i = 0; i < t.Columnas.Count; i++)
                    {
                        var celda = h.Cell().Background(Colors.Grey.Lighten3).Padding(2);
                        (t.Numericas[i] ? celda.AlignRight() : celda).Text(t.Columnas[i]).Bold();
                    }
                });
                foreach (var f in t.Filas)
                {
                    for (var i = 0; i < t.Columnas.Count; i++)
                    {
                        var celda = tabla.Cell().BorderBottom(0.3f).BorderColor(Colors.Grey.Lighten2).Padding(2);
                        var texto = (t.Numericas[i] ? celda.AlignRight() : celda).Text(i < f.Celdas.Count ? f.Celdas[i] : string.Empty);
                        if (f.Negrita)
                        {
                            texto.Bold();
                        }
                    }
                }
            });
        });
    }
}
