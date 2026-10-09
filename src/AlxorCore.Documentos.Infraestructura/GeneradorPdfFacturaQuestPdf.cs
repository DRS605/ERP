using AlxorCore.Documentos.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AlxorCore.Documentos.Infraestructura;

/// <summary>Genera el PDF de una factura con QuestPDF. Diseño limpio y sobrio (español).</summary>
internal sealed class GeneradorPdfFacturaQuestPdf : IGeneradorPdfFactura
{
    public byte[] Generar(FacturaDto factura, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(factura);
        ArgumentNullException.ThrowIfNull(emisor);

        // Un ticket (factura simplificada) se imprime en formato rollo de 80 mm; el resto en A4.
        return string.Equals(factura.Tipo, "Simplificada", StringComparison.OrdinalIgnoreCase)
            ? GenerarTicket(factura, emisor)
            : GenerarFacturaA4(factura, emisor);
    }

    /// <summary>Genera el PNG del QR de cotejo VeriFactu, o null si la factura aún no tiene huella.</summary>
    private static byte[]? GenerarQr(FacturaDto factura, EmpresaDto emisor)
    {
        if (string.IsNullOrEmpty(factura.Huella))
        {
            return null;
        }

        var url = Verifactu.UrlCotejo(emisor.Nif, factura.NumeroCompleto, factura.FechaEmision, factura.Total);
        using var generador = new QRCodeGenerator();
        var datos = generador.CreateQrCode(url, QRCodeGenerator.ECCLevel.M);
        return new PngByteQRCode(datos).GetGraphic(12);
    }

    private static byte[] GenerarFacturaA4(FacturaDto factura, EmpresaDto emisor)
    {
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
                        col.Item().Text("FACTURA").Bold().FontSize(16).FontColor(color);
                        col.Item().Text(factura.NumeroCompleto);
                        col.Item().Text($"Fecha: {factura.FechaEmision:dd/MM/yyyy}");
                    });
                });

                pagina.Content().PaddingVertical(15).Column(col =>
                {
                    col.Item().PaddingBottom(10).Column(cliente =>
                    {
                        cliente.Item().Text("Cliente").Bold();
                        cliente.Item().Text(factura.ClienteNombre);
                        if (!string.IsNullOrWhiteSpace(factura.ClienteNif))
                        {
                            cliente.Item().Text($"NIF: {factura.ClienteNif}");
                        }
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
                            static IContainer Celda(IContainer c, Color color) => c.BorderBottom(1.5f).BorderColor(color).PaddingBottom(3);
                            Celda(encabezado.Cell(), color).Text("Descripción").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Cantidad").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Precio").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("IVA").Bold().FontColor(color);
                            Celda(encabezado.Cell(), color).AlignRight().Text("Base").Bold().FontColor(color);
                        });

                        foreach (var linea in factura.Lineas)
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
                        totales.Item().Text($"Base imponible: {Redondeo.Formatear(factura.BaseImponible)} €");
                        totales.Item().Text($"IVA: {Redondeo.Formatear(factura.CuotaIva)} €");
                        if (factura.RecargoTotal > 0)
                        {
                            totales.Item().Text($"Recargo de equivalencia: {Redondeo.Formatear(factura.RecargoTotal)} €");
                        }

                        if (factura.RetencionIrpf > 0)
                        {
                            totales.Item().Text($"Retención IRPF ({factura.PorcentajeIrpf:0}%): -{Redondeo.Formatear(factura.RetencionIrpf)} €");
                        }

                        totales.Item().Text($"TOTAL: {Redondeo.Formatear(factura.Total)} €").Bold().FontSize(13).FontColor(color);
                    });

                    // Mención fiscal obligatoria (exención, inversión del sujeto pasivo, no sujeto,
                    // operación intracomunitaria, etc.) cuando alguna línea la requiere.
                    if (!string.IsNullOrWhiteSpace(factura.MencionFiscal))
                    {
                        col.Item().PaddingTop(12).BorderTop(0.75f).BorderColor(Colors.Grey.Lighten1).PaddingTop(6)
                            .Text(factura.MencionFiscal).FontSize(8).Italic().FontColor(Colors.Grey.Darken2);
                    }

                    var qr = GenerarQr(factura, emisor);
                    if (qr is not null)
                    {
                        col.Item().PaddingTop(20).Row(fila =>
                        {
                            fila.ConstantItem(90).Image(qr);
                            fila.RelativeItem().PaddingLeft(12).AlignBottom().Column(vf =>
                            {
                                vf.Item().Text("Factura verificable en la sede electrónica de la AEAT").FontSize(8).FontColor(Colors.Grey.Darken1);
                                vf.Item().Text("VERI*FACTU").Bold().FontSize(9);
                                vf.Item().Text($"Huella: {factura.Huella![..16]}…").FontSize(7).FontColor(Colors.Grey.Medium);
                            });
                        });
                    }
                });

                pagina.Footer().AlignCenter().Text(texto => PlantillaImpreso.EscribirPie(texto, emisor));
            });
        });

        return documento.GeneratePdf();
    }

    /// <summary>Genera el PDF de un ticket (factura simplificada) en formato rollo de 80 mm.</summary>
    private static byte[] GenerarTicket(FacturaDto factura, EmpresaDto emisor)
    {
        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.ContinuousSize(72, Unit.Millimetre);
                pagina.Margin(6, Unit.Millimetre);
                // Todo el texto del ticket en NEGRO: el gris no imprime bien en impresora térmica.
                pagina.DefaultTextStyle(x => x.FontSize(8).FontFamily(Fonts.Calibri).FontColor(Colors.Black));

                pagina.Content().Column(col =>
                {
                    col.Spacing(2);

                    // Emblema: el logo de la empresa si lo hay; si no, la cesta por defecto.
                    var emblema = emisor.LogoPng is { Length: > 0 } ? emisor.LogoPng : EmblemaTicket.Cesta;
                    col.Item().AlignCenter().Height(38).Image(emblema).FitHeight();

                    // Nombre completo, en una sola tipografía/tamaño, en negrita.
                    col.Item().AlignCenter().PaddingTop(2).Text(emisor.RazonSocial).Bold().FontSize(13);
                    var dirTicket = PlantillaImpreso.LineaDireccion(emisor);
                    var cabecera = dirTicket is not null ? $"NIF {emisor.Nif} · {dirTicket}" : $"NIF {emisor.Nif}";
                    col.Item().AlignCenter().Text(cabecera).FontSize(7);
                    var contactoTicket = PlantillaImpreso.LineaContacto(emisor);
                    if (contactoTicket is not null) col.Item().AlignCenter().Text(contactoTicket).FontSize(7);
                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Black);

                    col.Item().AlignCenter().Text($"Ticket {factura.NumeroCompleto} · {factura.FechaEmision:dd/MM/yyyy}").FontSize(8);
                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Black);

                    foreach (var linea in factura.Lineas)
                    {
                        // Nombre + importe final (negrita) y, debajo, cantidad × precio con IVA y el % de IVA.
                        var precioFinalUnidad = linea.PrecioUnitario * (1 + (linea.PorcentajeIva / 100m));
                        col.Item().Row(fila =>
                        {
                            fila.RelativeItem().Text(linea.Descripcion).Bold();
                            fila.ConstantItem(70).AlignRight().Text($"{Redondeo.Formatear(linea.Base + linea.CuotaIva)} €").Bold();
                        });
                        col.Item().Row(fila =>
                        {
                            fila.RelativeItem().Text($"{Redondeo.Formatear(linea.Cantidad)} × {Redondeo.Formatear(precioFinalUnidad)} €").FontSize(7);
                            fila.ConstantItem(70).AlignRight().Text($"IVA {linea.PorcentajeIva:0}%").FontSize(7);
                        });
                    }

                    col.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor(Colors.Black);

                    col.Item().Row(f => { f.RelativeItem().Text("Base imponible").FontSize(7); f.ConstantItem(70).AlignRight().Text($"{Redondeo.Formatear(factura.BaseImponible)} €").FontSize(7); });
                    col.Item().Row(f => { f.RelativeItem().Text("Cuota IVA").FontSize(7); f.ConstantItem(70).AlignRight().Text($"{Redondeo.Formatear(factura.CuotaIva)} €").FontSize(7); });
                    col.Item().PaddingTop(3).BorderTop(1.4f).BorderColor(Colors.Black).PaddingTop(3).Row(f =>
                    {
                        f.RelativeItem().Text("TOTAL").Bold().FontSize(13);
                        f.ConstantItem(80).AlignRight().Text($"{Redondeo.Formatear(factura.Total)} €").Bold().FontSize(13);
                    });
                    col.Item().AlignCenter().Text("IVA incluido").FontSize(7);

                    if (!string.IsNullOrWhiteSpace(factura.MencionFiscal))
                    {
                        col.Item().PaddingTop(3).AlignCenter().Text(factura.MencionFiscal).FontSize(7).Italic();
                    }

                    col.Item().PaddingVertical(4).LineHorizontal(0.5f).LineColor(Colors.Black);
                    col.Item().AlignCenter().Text(string.IsNullOrWhiteSpace(emisor.TextoPie) ? "Gracias por su compra" : emisor.TextoPie).Bold().FontSize(11);
                    col.Item().AlignCenter().PaddingTop(4).Text("Emitido con Core Evolution · Map Technology").FontSize(7);
                });
            });
        });

        return documento.GeneratePdf();
    }
}
