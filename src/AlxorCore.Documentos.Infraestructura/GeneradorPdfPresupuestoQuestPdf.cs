using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>
/// Genera el PDF de un presupuesto con QuestPDF. Diseño limpio y sobrio (español).
/// A diferencia de la factura, deja claro que <b>no es un documento fiscal</b>: no lleva
/// numeración legal, IRPF ni VeriFactu; muestra la fecha de validez de la oferta.
/// </summary>
internal sealed class GeneradorPdfPresupuestoQuestPdf : IGeneradorPdfPresupuesto
{
    public byte[] Generar(PresupuestoDto presupuesto, EmpresaDto emisor, string? idioma = null)
    {
        ArgumentNullException.ThrowIfNull(presupuesto);
        ArgumentNullException.ThrowIfNull(emisor);
        var lengua = IdiomasDocumento.Efectivo(idioma);
        string T(string clave) => TextosImpreso.T(lengua, clave);
        string N(decimal v) => TextosImpreso.Numero(lengua, v);
        var siglas = TextosImpreso.Impuesto(lengua, emisor.ImpuestoIndirecto.Siglas());

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(40);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                var color = PlantillaImpreso.ColorMarca(emisor);
                pagina.Header().Row(fila =>
                {
                    fila.RelativeItem().Column(col => PlantillaImpreso.EscribirEmisor(col, emisor, color, lengua));
                    fila.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text(T("Presupuesto").ToUpperInvariant()).Bold().FontSize(16).FontColor(color);
                        col.Item().Text(presupuesto.NumeroCompleto);
                        col.Item().Text($"{T("Fecha")}: {presupuesto.Fecha:dd/MM/yyyy}");
                        col.Item().Text($"{T("Válido hasta")}: {presupuesto.Validez:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                    });
                });

                pagina.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().PaddingBottom(10).Column(cliente =>
                    {
                        cliente.Item().Text(T("Cliente")).Bold();
                        cliente.Item().Text(presupuesto.ClienteNombre);
                    });

                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columnas =>
                        {
                            columnas.RelativeColumn(4);
                            columnas.RelativeColumn(1);
                            columnas.RelativeColumn(1);
                            columnas.RelativeColumn(1);
                            columnas.RelativeColumn(1);
                        });

                        tabla.Header(encabezado =>
                        {
                            static QuestPDF.Infrastructure.IContainer Celda(QuestPDF.Infrastructure.IContainer c, QuestPDF.Infrastructure.Color color) => c.BorderBottom(1.5f).BorderColor(color).PaddingBottom(3);
                            Celda(encabezado.Cell(), color).Text(T("Descripción")).Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text(T("Cantidad")).Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text(T("Precio")).Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text(siglas).Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text(T("Base")).Bold().FontColor(color);
                        });

                        foreach (var linea in presupuesto.Lineas)
                        {
                            tabla.Cell().Text(linea.Descripcion);
                            tabla.Cell().AlignRight().Text(N(linea.Cantidad));
                            tabla.Cell().AlignRight().Text(N(linea.PrecioUnitario));
                            tabla.Cell().AlignRight().Text($"{Porcentaje(linea.PorcentajeIva)}%");
                            tabla.Cell().AlignRight().Text(N(linea.Base - linea.ImporteConceptos));
                            foreach (var c in (linea.Conceptos ?? []).Where(c => c.Efecto == EfectoConcepto.Precio))
                            {
                                tabla.Cell().PaddingLeft(10).Text($"· {c.Nombre}{(c.Calculo == CalculoConcepto.Porcentaje ? $" ({N(c.Valor)} %)" : string.Empty)}").FontSize(8).FontColor(Colors.Grey.Darken2);
                                tabla.Cell();
                                tabla.Cell();
                                tabla.Cell();
                                tabla.Cell().AlignRight().Text(N(c.Importe)).FontSize(8).FontColor(Colors.Grey.Darken2);
                            }
                        }
                    });

                    col.Item().AlignRight().PaddingTop(15).Column(totales =>
                    {
                        totales.Item().Text($"{T("Base imponible")}: {N(presupuesto.BaseImponible)} €");
                        totales.Item().Text($"{siglas}: {N(presupuesto.CuotaIva)} €");
                        totales.Item().Text($"{T("TOTAL")}: {N(presupuesto.Total)} €").Bold().FontSize(13).FontColor(color);
                    });

                    col.Item().PaddingTop(24).Text(T("Leyenda presupuesto"))
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                pagina.Footer().AlignCenter().Text(texto => PlantillaImpreso.EscribirPie(texto, emisor));
            });
        });

        return documento.GeneratePdf();
    }

    /// <summary>Porcentaje con sus decimales y coma decimal (10,5 %, 9,5 %; no «11 %» ni «10 %»).</summary>
    private static string Porcentaje(decimal porcentaje) =>
        porcentaje.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',');
}
