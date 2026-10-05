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
        return GenerarVarias([etiqueta], emisor);
    }

    public byte[] GenerarVarias(IReadOnlyList<EtiquetaLogistica> etiquetas, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(etiquetas);
        ArgumentNullException.ThrowIfNull(emisor);
        if (etiquetas.Count == 0)
        {
            throw new ArgumentException("Indica al menos una etiqueta.", nameof(etiquetas));
        }

        return Document.Create(c =>
        {
            foreach (var etiqueta in etiquetas)
            {
                Pagina(c, etiqueta, emisor);
            }
        }).GeneratePdf();
    }

    private static void Pagina(IDocumentContainer c, EtiquetaLogistica etiqueta, EmpresaDto emisor)
    {

        var sscc = new List<(string, string)> { ("00", etiqueta.Sscc) };
        // Sin kilos (una etiqueta de campo, antes de pesar) no se codifica el peso.
        var contenido = etiqueta.KilosNetos > 0m ? new List<(string, string)> { Gs1128.PesoNeto(etiqueta.KilosNetos) } : [];
        List<(string, string)>? articulo = null;
        if (etiqueta.Gtin is { Length: 14 } gtin)
        {
            articulo = [("02", gtin)];
            if (etiqueta.FechaCaducidad is { } cad)
            {
                articulo.Add(("17", cad.ToString("yyMMdd", CultureInfo.InvariantCulture)));
            }

            if (etiqueta.Cajas > 0)
            {
                articulo.Add(("37", etiqueta.Cajas.ToString(CultureInfo.InvariantCulture)));
            }
        }
        else if (etiqueta.Cajas > 0)
        {
            contenido.Add(("37", etiqueta.Cajas.ToString(CultureInfo.InvariantCulture)));
        }

        if (LoteCodificable(etiqueta.Lote))
        {
            contenido.Add(("10", etiqueta.Lote!));
        }

        if (etiqueta.Diseno is { } diseno)
        {
            PaginaConDiseno(c, etiqueta, emisor, diseno, articulo, contenido, sscc);
            return;
        }

        c.Page(pagina =>
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
                    f.RelativeItem().Column(x => Campo(x, "Peso neto", etiqueta.KilosNetos > 0m ? Redondeo.Formatear(etiqueta.KilosNetos, 3) + " kg" : null));
                });
                col.Item().Row(f =>
                {
                    f.RelativeItem().Column(x => Campo(x, "Lote", etiqueta.Lote));
                    f.RelativeItem().Column(x => Campo(x, etiqueta.FechaCaducidad is null ? "Fecha" : "Caducidad",
                        (etiqueta.FechaCaducidad ?? etiqueta.Fecha).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)));
                });
                if (etiqueta.Gtin is not null || etiqueta.KilosBrutos is not null)
                {
                    col.Item().Row(f =>
                    {
                        f.RelativeItem().Column(x => Campo(x, "GTIN", etiqueta.Gtin));
                        f.RelativeItem().Column(x => Campo(x, "Peso bruto", etiqueta.KilosBrutos is { } b ? Redondeo.Formatear(b, 3) + " kg" : null));
                    });
                }

                Campo(col, "Tipo de palé", etiqueta.TipoPale);
                Campo(col, "Origen", etiqueta.Origen);
                Campo(col, "Destinatario", etiqueta.Destinatario);
                col.Item().LineHorizontal(1);
                if (articulo is not null)
                {
                    Codigo(col, articulo);
                }

                if (contenido.Count > 0)
                {
                    Codigo(col, contenido);
                }

                Codigo(col, sscc);
            });
        });
    }

    /// <summary>Los datos de la etiqueta por campo (título y valor; sin valor no se pintan).</summary>
    private static IEnumerable<(string Titulo, string? Valor)> Datos(EtiquetaLogistica e, IReadOnlyList<string> campos)
    {
        foreach (var campo in campos)
        {
            yield return campo switch
            {
                "Producto" => ("Producto", e.Producto),
                "Marca" => ("Marca", e.Marca),
                "Cajas" => ("Cajas", e.Cajas > 0 ? e.Cajas.ToString(CultureInfo.InvariantCulture) : null),
                "PesoNeto" => ("Peso neto", e.KilosNetos > 0m ? Redondeo.Formatear(e.KilosNetos, 3) + " kg" : null),
                "PesoBruto" => ("Peso bruto", e.KilosBrutos is { } b ? Redondeo.Formatear(b, 3) + " kg" : null),
                "Lote" => ("Lote", e.Lote),
                "Fecha" => ("Fecha", e.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                "Caducidad" => ("Consumo preferente", e.FechaCaducidad?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)),
                "Gtin" => ("GTIN", e.Gtin),
                "TipoPale" => ("Tipo de palé", e.TipoPale),
                "Origen" => ("Origen", e.Origen),
                "Destinatario" => ("Destinatario", e.Destinatario),
                "ReferenciaCliente" => ("Ref. cliente", e.ReferenciaCliente),
                _ => (string.Empty, null),
            };
        }
    }

    private static void PaginaConDiseno(IDocumentContainer c, EtiquetaLogistica etiqueta, EmpresaDto emisor, DisenoEtiqueta diseno, List<(string, string)>? articulo,
        List<(string, string)> contenido, List<(string, string)> sscc)
    {
        var ancho = diseno.Rollo100x150 ? 90f : AnchoUtilMm;
        c.Page(pagina =>
        {
            if (diseno.Rollo100x150)
            {
                pagina.Size(100, 150, Unit.Millimetre);
            }
            else
            {
                pagina.Size(PageSizes.A6);
            }

            pagina.Margin(5, Unit.Millimetre);
            pagina.DefaultTextStyle(x => x.FontSize(9));
            pagina.Content().Column(col =>
            {
                col.Spacing(4);
                col.Item().Text(emisor.RazonSocial).Bold().FontSize(11);
                col.Item().LineHorizontal(1);
                col.Item().Text("SSCC").FontSize(7).FontColor(Colors.Grey.Darken1);
                col.Item().Text(etiqueta.Sscc).Bold().FontSize(14);
                foreach (var campo in diseno.Campos)
                {
                    if (campo == "TextoLibre")
                    {
                        if (!string.IsNullOrWhiteSpace(diseno.TextoLibre))
                        {
                            col.Item().Text(diseno.TextoLibre).SemiBold();
                        }

                        continue;
                    }

                    foreach (var (titulo, valor) in Datos(etiqueta, [campo]))
                    {
                        Campo(col, titulo, valor);
                    }
                }

                col.Item().LineHorizontal(1);
                if (articulo is not null)
                {
                    Codigo(col, articulo, ancho);
                }

                if (contenido.Count > 0)
                {
                    Codigo(col, contenido, ancho);
                }

                Codigo(col, sscc, ancho);
            });
        });
    }

    /// <summary>
    /// ZPL II a 203 ppp (8 puntos por mm) para un rollo de 100 × 150 mm: los datos en texto y los códigos GS1-128 en modo
    /// UCC/EAN (<c>^BC…D</c>, la impresora pone el FNC1).
    /// </summary>
    public string GenerarZpl(EtiquetaLogistica etiqueta, EmpresaDto emisor)
    {
        ArgumentNullException.ThrowIfNull(etiqueta);
        ArgumentNullException.ThrowIfNull(emisor);
        static string Z(string? t) => (t ?? string.Empty).Replace('^', ' ').Replace('~', '-');
        var sb = new System.Text.StringBuilder();
        sb.Append("^XA^CI28^PW800^LL1200\n");
        sb.Append(CultureInfo.InvariantCulture, $"^FO40,30^A0N,40,40^FD{Z(emisor.RazonSocial)}^FS\n");
        sb.Append("^FO40,80^GB720,3,3^FS\n");
        sb.Append("^FO40,95^A0N,24,24^FDSSCC^FS\n");
        sb.Append(CultureInfo.InvariantCulture, $"^FO40,122^A0N,48,48^FD{Z(etiqueta.Sscc)}^FS\n");
        var y = 185;
        var campos = etiqueta.Diseno?.Campos ?? ["Producto", "Marca", "Cajas", "PesoNeto", "Lote", "Caducidad", "Gtin", "PesoBruto", "TipoPale", "Origen", "Destinatario"];
        foreach (var campo in campos)
        {
            if (y > 700)
            {
                break;
            }

            if (campo == "TextoLibre")
            {
                if (!string.IsNullOrWhiteSpace(etiqueta.Diseno?.TextoLibre))
                {
                    sb.Append(CultureInfo.InvariantCulture, $"^FO40,{y}^FB720,3,0,L^A0N,28,28^FD{Z(etiqueta.Diseno.TextoLibre)}^FS\n");
                    y += 90;
                }

                continue;
            }

            foreach (var (titulo, valor) in Datos(etiqueta, [campo]).Where(d => !string.IsNullOrWhiteSpace(d.Valor)))
            {
                sb.Append(CultureInfo.InvariantCulture, $"^FO40,{y}^A0N,30,30^FD{Z(titulo)}: {Z(valor)}^FS\n");
                y += 40;
            }
        }

        var codigos = new List<IReadOnlyList<(string, string)>>();
        if (etiqueta.Gtin is { Length: 14 } gtin)
        {
            var art = new List<(string, string)> { ("02", gtin) };
            if (etiqueta.FechaCaducidad is { } cad)
            {
                art.Add(("17", cad.ToString("yyMMdd", CultureInfo.InvariantCulture)));
            }

            if (etiqueta.Cajas > 0)
            {
                art.Add(("37", etiqueta.Cajas.ToString(CultureInfo.InvariantCulture)));
            }

            codigos.Add(art);
        }

        var cont = new List<(string, string)>();
        if (etiqueta.KilosNetos > 0m)
        {
            cont.Add(Gs1128.PesoNeto(etiqueta.KilosNetos));
        }

        if (LoteCodificable(etiqueta.Lote))
        {
            cont.Add(("10", etiqueta.Lote!));
        }

        if (cont.Count > 0)
        {
            codigos.Add(cont);
        }

        codigos.Add([("00", etiqueta.Sscc)]);
        y = Math.Max(y + 20, 1200 - (codigos.Count * 160) - 20);
        foreach (var c in codigos)
        {
            var datos = string.Concat(c.Select(x => $"({x.Item1}){x.Item2}"));
            sb.Append(CultureInfo.InvariantCulture, $"^FO50,{y}^BY3^BCN,110,Y,N,N,D^FD{datos}^FS\n");
            y += 160;
        }

        sb.Append("^XZ\n");
        return sb.ToString();
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

    private static void Codigo(ColumnDescriptor col, IReadOnlyList<(string Ia, string Valor)> elementos, float anchoUtilMm = AnchoUtilMm)
    {
        var anchuras = Gs1128.Anchuras(Gs1128.Componer(elementos));
        var modulos = anchuras.Sum() + 20;
        var moduloMm = Math.Min(ModuloMaximoMm, (anchoUtilMm - 1f) / modulos);
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
