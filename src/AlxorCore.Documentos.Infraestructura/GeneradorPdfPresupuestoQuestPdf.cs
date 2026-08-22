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
    public byte[] Generar(PresupuestoDto presupuesto, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(presupuesto);
        ArgumentNullException.ThrowIfNull(emisor);

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
                    fila.RelativeItem().Column(col => PlantillaImpreso.EscribirEmisor(col, emisor, color));
                    fila.ConstantItem(200).AlignRight().Column(col =>
                    {
                        col.Item().Text("PRESUPUESTO").Bold().FontSize(16).FontColor(color);
                        col.Item().Text(presupuesto.NumeroCompleto);
                        col.Item().Text($"Fecha: {presupuesto.Fecha:dd/MM/yyyy}");
                        col.Item().Text($"Válido hasta: {presupuesto.Validez:dd/MM/yyyy}").FontColor(Colors.Grey.Darken1);
                    });
                });

                pagina.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().PaddingBottom(10).Column(cliente =>
                    {
                        cliente.Item().Text("Cliente").Bold();
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
                            Celda(encabezado.Cell(), color).Text("Descripción").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Cantidad").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Precio").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("IVA").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Base").Bold().FontColor(color);
                        });

                        foreach (var linea in presupuesto.Lineas)
                        {
                            tabla.Cell().Text(linea.Descripcion);
                            tabla.Cell().AlignRight().Text(Redondeo.Formatear(linea.Cantidad));
                            tabla.Cell().AlignRight().Text(Redondeo.Formatear(linea.PrecioUnitario));
                            tabla.Cell().AlignRight().Text($"{linea.PorcentajeIva:0}%");
                            tabla.Cell().AlignRight().Text(Redondeo.Formatear(linea.Base));
                        }
                    });

                    col.Item().AlignRight().PaddingTop(15).Column(totales =>
                    {
                        totales.Item().Text($"Base imponible: {Redondeo.Formatear(presupuesto.BaseImponible)} €");
                        totales.Item().Text($"IVA: {Redondeo.Formatear(presupuesto.CuotaIva)} €");
                        totales.Item().Text($"TOTAL: {Redondeo.Formatear(presupuesto.Total)} €").Bold().FontSize(13).FontColor(color);
                    });

                    col.Item().PaddingTop(24).Text("Este documento es un presupuesto (oferta) y no tiene carácter de factura. Los importes son válidos hasta la fecha indicada.")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                pagina.Footer().AlignCenter().Text(texto => PlantillaImpreso.EscribirPie(texto, emisor));
            });
        });

        return documento.GeneratePdf();
    }
}
