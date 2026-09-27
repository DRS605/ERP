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
        if (carta.Tipo == "Internacional")
        {
            return GeneradorCmr.Generar(carta, emisor);
        }

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

                    var t = carta.Transporte;
                    var extra = new[]
                    {
                        t?.MatriculaRemolque is { } rem ? $"Remolque: {rem}" : null,
                        t?.Conductor is { } con ? $"Conductor: {con}{(t.Conductor2 is { } c2 ? $" · {c2}" : string.Empty)}" : null,
                        t?.TemperaturaConsigna is { } tc ? $"Temperatura de consigna: {tc.ToString("0.#", AlxorCore.Nucleo.Comun.Redondeo.NumeroEspanol)} °C" : null,
                        t?.Termografo is { } tg ? $"Termógrafo: {tg}" : null,
                        t?.Incoterm is { } inc ? $"Incoterm: {inc} {t.LugarIncoterm}".Trim() : null,
                        t?.Portes is { } po ? $"Portes {po.ToString().ToLowerInvariant()}" : null,
                        t?.Contenedor is { } cn ? $"Contenedor: {cn}{(t.Precinto is { } pr ? $" · precinto {pr}" : string.Empty)}" : null,
                    }.Where(x => x is not null).ToList();
                    if (extra.Count > 0)
                    {
                        col.Item().Text(string.Join("   ·   ", extra)).FontSize(9);
                    }

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

/// <summary>
/// CMR (Convenio relativo al contrato de transporte internacional de mercancías por carretera, Ginebra 1956): las 24
/// casillas del modelo, en un ejemplar por página (remitente, consignatario y transportista).
/// </summary>
internal static class GeneradorCmr
{
    private static readonly (string Texto, string Color)[] Ejemplares =
    [
        ("1 · Ejemplar para el remitente / Copy for sender", "#C62828"),
        ("2 · Ejemplar para el consignatario / Copy for consignee", "#1565C0"),
        ("3 · Ejemplar para el transportista / Copy for carrier", "#2E7D32"),
    ];

    private static readonly System.Globalization.NumberFormatInfo Es = AlxorCore.Nucleo.Comun.Redondeo.NumeroEspanol;

    public static byte[] Generar(CartaPorteDto carta, EmpresaDto emisor)
    {
        var t = carta.Transporte ?? new AlxorCore.Facturacion.Dominio.TransporteCarta();
        return Document.Create(doc =>
        {
            foreach (var (ejemplar, color) in Ejemplares)
            {
                doc.Page(p =>
                {
                    p.Size(PageSizes.A4);
                    p.Margin(22);
                    p.DefaultTextStyle(x => x.FontSize(7.5f));
                    p.Content().Border(1).BorderColor(color).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Element(c => Casilla(c, color, "1", "Remitente (nombre, domicilio, país) / Sender", x =>
                            {
                                x.Item().Text(carta.RemitenteNombre).Bold();
                                x.Item().Text(string.Join(", ", new[] { emisor.Calle, $"{emisor.CodigoPostal} {emisor.Poblacion}".Trim(), emisor.Pais }.Where(v => !string.IsNullOrWhiteSpace(v))));
                                if (carta.RemitenteNif is { } n)
                                {
                                    x.Item().Text($"NIF {n} · EORI {(AlxorCore.Nucleo.Comun.Paises.Codigo(emisor.Pais) ?? "ES")}{n}");
                                }
                            }));
                            r.RelativeItem().BorderLeft(1).BorderColor(color).Padding(6).Column(x =>
                            {
                                x.Item().Text("CARTA DE PORTE INTERNACIONAL").Bold().FontSize(12).FontColor(color);
                                x.Item().Text("INTERNATIONAL CONSIGNMENT NOTE · CMR").Bold().FontSize(9).FontColor(color);
                                x.Item().PaddingTop(4).Text($"Nº {carta.NumeroCompleto}").Bold().FontSize(11);
                                x.Item().PaddingTop(4).Text("Este transporte queda sometido, no obstante cualquier cláusula en contrario, al Convenio relativo al contrato de transporte internacional de mercancías por carretera (CMR).")
                                    .FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                                x.Item().PaddingTop(4).Text(ejemplar).Bold().FontColor(color);
                            });
                        });
                        Fila(col, color,
                            ("2", "Consignatario (nombre, domicilio, país) / Consignee", x =>
                            {
                                x.Item().Text(carta.DestinatarioNombre).Bold();
                                x.Item().Text(carta.LugarDestino);
                                if (carta.DestinatarioNif is { } n)
                                {
                                    x.Item().Text(n);
                                }
                            }),
                            ("16", "Transportista (nombre, domicilio, país) / Carrier", x =>
                            {
                                x.Item().Text(carta.TransportistaNombre ?? " ").Bold();
                                if (carta.TransportistaNif is { } n)
                                {
                                    x.Item().Text(n);
                                }

                                x.Item().Text($"Matrícula: {carta.Matricula ?? "—"}{(t.MatriculaRemolque is { } rem ? $" · remolque {rem}" : string.Empty)}");
                                if (t.Conductor is { } con)
                                {
                                    x.Item().Text($"Conductor: {con}{(t.Conductor2 is { } c2 ? $" · {c2}" : string.Empty)}");
                                }
                            }));
                        Fila(col, color,
                            ("3", "Lugar de entrega de la mercancía (lugar, país) / Place of delivery", x => x.Item().Text($"{carta.LugarDestino}{Pais(t.PaisDestino)}")),
                            ("17", "Porteadores sucesivos / Successive carriers", _ => { }));
                        Fila(col, color,
                            ("4", "Lugar y fecha de carga (lugar, país, fecha) / Place and date of taking over", x =>
                                x.Item().Text($"{carta.LugarOrigen}{Pais(t.PaisOrigen)} · {(carta.FechaCarga ?? carta.FechaExpedicion):dd/MM/yyyy}")),
                            ("18", "Reservas y observaciones del transportista / Carrier's reservations", _ => { }));
                        Fila(col, color, ("5", "Documentos anexos / Documents attached", x =>
                        {
                            x.Item().Text(t.DocumentosAnexos ?? " ");
                            foreach (var cert in carta.Certificados ?? [])
                            {
                                x.Item().Text(cert);
                            }
                        }));

                        // 6 a 12: la mercancía.
                        col.Item().BorderTop(1).BorderColor(color).Table(tabla =>
                        {
                            tabla.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(1.3f); c.ConstantColumn(44); c.RelativeColumn(1.1f); c.RelativeColumn(2.4f); c.ConstantColumn(62); c.ConstantColumn(58); c.ConstantColumn(44);
                            });
                            string[] cab = ["6 Marcas y números", "7 Nº bultos", "8 Embalaje", "9 Naturaleza de la mercancía", "10 Nº estadístico", "11 Peso bruto kg", "12 Volumen m³"];
                            tabla.Header(h =>
                            {
                                foreach (var c in cab)
                                {
                                    h.Cell().BorderBottom(1).BorderColor(color).Padding(3).Text(c).FontSize(6.5f).FontColor(color);
                                }
                            });
                            foreach (var l in carta.Lineas)
                            {
                                tabla.Cell().Padding(3).Text(l.Marcas ?? string.Empty);
                                tabla.Cell().Padding(3).AlignRight().Text(l.Bultos.ToString(Es));
                                tabla.Cell().Padding(3).Text(l.Embalaje ?? string.Empty);
                                tabla.Cell().Padding(3).Text(l.PesoNetoKg is { } pn ? $"{l.Descripcion} (neto {pn.ToString("#,##0.###", Es)} kg)" : l.Descripcion);
                                tabla.Cell().Padding(3).Text(l.CodigoArancelario ?? string.Empty);
                                tabla.Cell().Padding(3).AlignRight().Text(l.PesoKg.ToString("#,##0.###", Es));
                                tabla.Cell().Padding(3).AlignRight().Text(l.VolumenM3?.ToString("0.###", Es) ?? string.Empty);
                            }

                            tabla.Cell().ColumnSpan(1).BorderTop(1).BorderColor(color).Padding(3).Text("Total").Bold();
                            tabla.Cell().BorderTop(1).BorderColor(color).Padding(3).AlignRight().Text(carta.TotalBultos.ToString(Es)).Bold();
                            tabla.Cell().ColumnSpan(3).BorderTop(1).BorderColor(color).Padding(3).Text(string.Empty);
                            tabla.Cell().BorderTop(1).BorderColor(color).Padding(3).AlignRight().Text(carta.TotalPesoKg.ToString("#,##0.###", Es)).Bold();
                            tabla.Cell().BorderTop(1).BorderColor(color).Padding(3).AlignRight().Text(carta.TotalVolumenM3?.ToString("0.###", Es) ?? string.Empty);
                        });

                        Fila(col, color,
                            ("13", "Instrucciones del remitente / Sender's instructions", x =>
                            {
                                if (t.TemperaturaConsigna is { } tc)
                                {
                                    x.Item().Text($"Mercancía perecedera: transportar a {tc.ToString("0.#", Es)} °C{(t.Termografo is { } tg ? $" · termógrafo {tg}" : string.Empty)}").Bold();
                                }

                                foreach (var texto in new[] { t.Instrucciones, carta.Observaciones }.Where(v => !string.IsNullOrWhiteSpace(v)))
                                {
                                    x.Item().Text(texto!);
                                }
                            }),
                            ("19", "Estipulaciones particulares / Special agreements", x =>
                            {
                                if (t.Incoterm is { } inc)
                                {
                                    x.Item().Text($"Incoterms® 2020: {inc} {t.LugarIncoterm}".Trim()).Bold();
                                }

                                if (t.Contenedor is { } cn)
                                {
                                    x.Item().Text($"Contenedor {cn}{(t.Precinto is { } pr ? $" · precinto {pr}" : string.Empty)}");
                                }
                            }));
                        Fila(col, color,
                            ("14", "Forma de pago / Instructions as to payment for carriage", x =>
                            {
                                x.Item().Text($"[{(t.Portes == AlxorCore.Facturacion.Dominio.Portes.Pagados ? "X" : " ")}] Portes pagados / Carriage paid");
                                x.Item().Text($"[{(t.Portes == AlxorCore.Facturacion.Dominio.Portes.Debidos ? "X" : " ")}] Portes debidos / Carriage forward");
                            }),
                            ("15", "Reembolso / Cash on delivery", _ => { }));
                        Fila(col, color,
                            ("21", "Formalizado en / Established in", x => x.Item().Text($"{carta.LugarOrigen}, a {carta.FechaExpedicion:dd/MM/yyyy}")),
                            ("20", "A pagar por / To be paid by", _ => { }));
                        col.Item().BorderTop(1).BorderColor(color).MinHeight(92).Extend().Row(r =>
                        {
                            r.RelativeItem().Element(c => Casilla(c, color, "22", "Firma y sello del remitente / Signature and stamp of the sender", _ => { }));
                            r.RelativeItem().BorderLeft(1).BorderColor(color).Element(c => Casilla(c, color, "23", "Firma y sello del transportista / Signature and stamp of the carrier", _ => { }));
                            r.RelativeItem().BorderLeft(1).BorderColor(color).Element(c => Casilla(c, color, "24", "Recibo de la mercancía: lugar, fecha, firma y sello del consignatario / Goods received", _ => { }));
                        });
                    });
                });
            }
        }).GeneratePdf();
    }

    private static string Pais(string? codigo) => codigo is null ? string.Empty : $" ({codigo})";

    private static void Fila(ColumnDescriptor col, string color, params (string Numero, string Titulo, Action<ColumnDescriptor> Contenido)[] casillas)
    {
        col.Item().BorderTop(1).BorderColor(color).Row(r =>
        {
            for (var i = 0; i < casillas.Length; i++)
            {
                var (n, titulo, contenido) = casillas[i];
                var item = r.RelativeItem();
                (i == 0 ? item : item.BorderLeft(1).BorderColor(color)).Element(c => Casilla(c, color, n, titulo, contenido));
            }

            if (casillas.Length == 1)
            {
                r.RelativeItem().BorderLeft(1).BorderColor(color);
            }
        });
    }

    private static void Casilla(IContainer c, string color, string numero, string titulo, Action<ColumnDescriptor> contenido) =>
        c.MinHeight(38).Padding(4).Column(x =>
        {
            x.Item().Text($"{numero}  {titulo}").FontSize(6.5f).FontColor(color);
            contenido(x);
        });
}
