using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>Genera el PDF de una carta de porte (documento de control del transporte) con QuestPDF.</summary>
internal sealed class GeneradorPdfCartaPorteQuestPdf : IGeneradorPdfCartaPorte
{
    public byte[] Generar(CartaPorteDto carta, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(carta);
        ArgumentNullException.ThrowIfNull(emisor);

        var color = PlantillaImpreso.ColorMarca(emisor);
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(40);
                pagina.DefaultTextStyle(x => x.FontSize(10));

                pagina.Header().Row(fila =>
                {
                    fila.RelativeItem().Column(col => PlantillaImpreso.EscribirEmisor(col, emisor, color));
                    fila.ConstantItem(210).AlignRight().Column(col =>
                    {
                        col.Item().Text("CARTA DE PORTE").Bold().FontSize(16).FontColor(color);
                        col.Item().Text(carta.NumeroCompleto);
                        col.Item().Text($"Expedición: {carta.FechaExpedicion:dd/MM/yyyy}");
                        if (carta.FechaCarga is { } fc)
                        {
                            col.Item().Text($"Carga: {fc:dd/MM/yyyy}");
                        }
                    });
                });

                pagina.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(fila =>
                    {
                        fila.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("Remitente (cargador)").Bold().FontColor(color);
                            c.Item().Text(carta.RemitenteNombre);
                            if (!string.IsNullOrWhiteSpace(carta.RemitenteNif))
                            {
                                c.Item().Text(carta.RemitenteNif!);
                            }
                        });
                        fila.ConstantItem(12);
                        fila.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                        {
                            c.Item().Text("Destinatario").Bold().FontColor(color);
                            c.Item().Text(carta.DestinatarioNombre);
                            if (!string.IsNullOrWhiteSpace(carta.DestinatarioNif))
                            {
                                c.Item().Text(carta.DestinatarioNif!);
                            }
                        });
                    });

                    col.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Lugar de origen").Bold();
                            c.Item().Text(string.IsNullOrWhiteSpace(carta.LugarOrigen) ? "—" : carta.LugarOrigen);
                        });
                        fila.ConstantItem(12);
                        fila.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Lugar de destino").Bold();
                            c.Item().Text(string.IsNullOrWhiteSpace(carta.LugarDestino) ? "—" : carta.LugarDestino);
                        });
                    });

                    col.Item().Row(fila =>
                    {
                        fila.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Transportista").Bold();
                            c.Item().Text(string.IsNullOrWhiteSpace(carta.TransportistaNombre) ? "—" : carta.TransportistaNombre!);
                            if (!string.IsNullOrWhiteSpace(carta.TransportistaNif))
                            {
                                c.Item().Text(carta.TransportistaNif!);
                            }
                        });
                        fila.ConstantItem(12);
                        fila.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Vehículo (matrícula)").Bold();
                            c.Item().Text(string.IsNullOrWhiteSpace(carta.Matricula) ? "—" : carta.Matricula!);
                        });
                    });

                    // Relación de mercancías.
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn();
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(90);
                        });

                        tabla.Header(h =>
                        {
                            h.Cell().BorderBottom(1).BorderColor(color).Padding(4).Text("Mercancía").Bold();
                            h.Cell().BorderBottom(1).BorderColor(color).Padding(4).AlignRight().Text("Bultos").Bold();
                            h.Cell().BorderBottom(1).BorderColor(color).Padding(4).AlignRight().Text("Peso (kg)").Bold();
                        });

                        foreach (var l in carta.Lineas)
                        {
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).Text(l.Descripcion);
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(l.Bultos.ToString(System.Globalization.CultureInfo.InvariantCulture));
                            tabla.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(4).AlignRight().Text(l.PesoKg.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                        }

                        tabla.Cell().Padding(4).Text("Total").Bold();
                        tabla.Cell().Padding(4).AlignRight().Text(carta.TotalBultos.ToString(System.Globalization.CultureInfo.InvariantCulture)).Bold();
                        tabla.Cell().Padding(4).AlignRight().Text(carta.TotalPesoKg.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)).Bold();
                    });

                    if (!string.IsNullOrWhiteSpace(carta.Observaciones))
                    {
                        col.Item().PaddingTop(4).Column(c =>
                        {
                            c.Item().Text("Observaciones").Bold();
                            c.Item().Text(carta.Observaciones!);
                        });
                    }

                    col.Item().PaddingTop(20).Row(fila =>
                    {
                        fila.RelativeItem().Column(c => { c.Item().Text("Firma del remitente").FontColor(Colors.Grey.Darken1); c.Item().PaddingTop(28).LineHorizontal(1).LineColor(Colors.Grey.Lighten1); });
                        fila.ConstantItem(24);
                        fila.RelativeItem().Column(c => { c.Item().Text("Firma del transportista").FontColor(Colors.Grey.Darken1); c.Item().PaddingTop(28).LineHorizontal(1).LineColor(Colors.Grey.Lighten1); });
                        fila.ConstantItem(24);
                        fila.RelativeItem().Column(c => { c.Item().Text("Firma del destinatario").FontColor(Colors.Grey.Darken1); c.Item().PaddingTop(28).LineHorizontal(1).LineColor(Colors.Grey.Lighten1); });
                    });
                });

                pagina.Footer().AlignCenter().Text(txt => PlantillaImpreso.EscribirPie(txt, emisor));
            });
        });

        return documento.GeneratePdf();
    }
}
