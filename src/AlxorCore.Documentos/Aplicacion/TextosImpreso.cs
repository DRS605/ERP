using System.Globalization;
using AlxorCore.Nucleo.Comun;

namespace AlxorCore.Documentos.Aplicacion;

/// <summary>
/// Textos fijos de los documentos para clientes y proveedores (factura, presupuesto, albarán y pedidos) en los idiomas
/// de <see cref="IdiomasDocumento"/>: castellano, inglés, francés, alemán, italiano y portugués. Lo que no se encuentra
/// sale en castellano.
/// </summary>
public static class TextosImpreso
{
    // Orden de cada fila: es, en, fr, de, it, pt.
    private static readonly string[] Orden = ["es", "en", "fr", "de", "it", "pt"];

    private static readonly Dictionary<string, string[]> Textos = new(StringComparer.Ordinal)
    {
        ["Factura"] = ["Factura", "Invoice", "Facture", "Rechnung", "Fattura", "Fatura"],
        ["Presupuesto"] = ["Presupuesto", "Quotation", "Devis", "Angebot", "Preventivo", "Orçamento"],
        ["Albarán"] = ["Albarán", "Delivery note", "Bon de livraison", "Lieferschein", "Documento di trasporto", "Guia de remessa"],
        ["Pedido de venta"] = ["Pedido de venta", "Order confirmation", "Confirmation de commande", "Auftragsbestätigung", "Conferma d'ordine", "Confirmação de encomenda"],
        ["Pedido de compra"] = ["Pedido de compra", "Purchase order", "Bon de commande", "Bestellung", "Ordine d'acquisto", "Nota de encomenda"],
        ["Fecha"] = ["Fecha", "Date", "Date", "Datum", "Data", "Data"],
        ["Válido hasta"] = ["Válido hasta", "Valid until", "Valable jusqu'au", "Gültig bis", "Valido fino al", "Válido até"],
        ["Vencimiento"] = ["Vencimiento", "Due date", "Échéance", "Fällig am", "Scadenza", "Vencimento"],
        ["Cliente"] = ["Cliente", "Customer", "Client", "Kunde", "Cliente", "Cliente"],
        ["Proveedor"] = ["Proveedor", "Supplier", "Fournisseur", "Lieferant", "Fornitore", "Fornecedor"],
        ["NIF"] = ["NIF", "Tax ID", "N° fiscal", "Steuer-Nr.", "Cod. fiscale", "NIF"],
        ["Descripción"] = ["Descripción", "Description", "Désignation", "Beschreibung", "Descrizione", "Descrição"],
        ["Cantidad"] = ["Cantidad", "Quantity", "Quantité", "Menge", "Quantità", "Quantidade"],
        ["Precio"] = ["Precio", "Price", "Prix", "Preis", "Prezzo", "Preço"],
        ["Dto."] = ["Dto.", "Disc.", "Rem.", "Rabatt", "Sconto", "Desc."],
        ["Importe"] = ["Importe", "Amount", "Montant", "Betrag", "Importo", "Montante"],
        ["Base"] = ["Base", "Net", "Montant HT", "Netto", "Imponibile", "Valor líquido"],
        ["Base imponible"] = ["Base imponible", "Taxable amount", "Total HT", "Nettobetrag", "Imponibile", "Base tributável"],
        ["TOTAL"] = ["TOTAL", "TOTAL", "TOTAL TTC", "GESAMTBETRAG", "TOTALE", "TOTAL"],
        ["Recargo de equivalencia"] = ["Recargo de equivalencia", "Equivalence surcharge", "Supplément d'équivalence", "Ausgleichszuschlag", "Sovrapprezzo di equivalenza", "Recargo de equivalência"],
        ["Retención IRPF"] = ["Retención IRPF", "Withholding tax (IRPF)", "Retenue à la source (IRPF)", "Quellensteuer (IRPF)", "Ritenuta d'acconto (IRPF)", "Retenção na fonte (IRPF)"],
        ["suplido"] = ["{0} (suplido, sin {1})", "{0} (disbursement, without {1})", "{0} (débours, sans {1})", "{0} (durchlaufender Posten, ohne {1})", "{0} (anticipazione, senza {1})", "{0} (despesa reembolsável, sem {1})"],
        ["Observaciones"] = ["Observaciones", "Notes", "Observations", "Bemerkungen", "Note", "Observações"],
        ["Página"] = ["Página", "Page", "Page", "Seite", "Pagina", "Página"],
        ["de"] = ["de", "of", "sur", "von", "di", "de"],
        ["Referencia"] = ["Referencia", "Reference", "Référence", "Referenz", "Riferimento", "Referência"],
        ["Estado"] = ["Estado", "Status", "État", "Status", "Stato", "Estado"],
        ["ANULADO"] = ["ANULADO", "CANCELLED", "ANNULÉ", "STORNIERT", "ANNULLATO", "ANULADO"],
        ["Precio por fijar"] = ["Precio por fijar", "Price to be set", "Prix à fixer", "Preis wird festgelegt", "Prezzo da definire", "Preço a definir"],
        ["Servido"] = ["Servido", "Delivered", "Livré", "Geliefert", "Consegnato", "Entregue"],
        ["Entrega"] = ["Entrega", "Delivery", "Livraison", "Lieferung", "Consegna", "Entrega"],
        ["IVA"] = ["IVA", "VAT", "TVA", "MwSt.", "IVA", "IVA"],
        ["Leyenda albarán"] = [
            "Importes sin impuestos: el IVA se aplica en la factura.",
            "Amounts exclusive of tax: VAT is applied on the invoice.",
            "Montants hors taxes : la TVA est appliquée sur la facture.",
            "Beträge ohne Steuern: Die MwSt. wird in der Rechnung berechnet.",
            "Importi al netto delle imposte: l'IVA si applica in fattura.",
            "Valores sem impostos: o IVA é aplicado na fatura.",
        ],
        ["Contravalor en euros"] = ["Contravalor en euros", "Equivalent in euros", "Contre-valeur en euros", "Gegenwert in Euro", "Controvalore in euro", "Contravalor em euros"],
        ["Tipo de cambio"] = ["tipo de cambio", "exchange rate", "taux de change", "Wechselkurs", "tasso di cambio", "taxa de câmbio"],
        ["Total"] = ["Total", "Total", "Total", "Gesamt", "Totale", "Total"],
        ["Leyenda pedido venta"] = [
            "Confirmación de pedido. Importes sin impuestos.",
            "Order confirmation. Amounts exclusive of tax.",
            "Confirmation de commande. Montants hors taxes.",
            "Auftragsbestätigung. Beträge ohne Steuern.",
            "Conferma d'ordine. Importi al netto delle imposte.",
            "Confirmação de encomenda. Valores sem impostos.",
        ],
        ["Leyenda pedido compra"] = [
            "Rogamos confirmen la recepción de este pedido.",
            "Please confirm receipt of this order.",
            "Merci de bien vouloir accuser réception de cette commande.",
            "Bitte bestätigen Sie den Erhalt dieser Bestellung.",
            "Vi preghiamo di confermare la ricezione del presente ordine.",
            "Agradecemos a confirmação da receção desta encomenda.",
        ],
        ["Leyenda presupuesto"] = [
            "Este documento es un presupuesto (oferta) y no tiene carácter de factura. Los importes son válidos hasta la fecha indicada.",
            "This document is a quotation (offer) and is not an invoice. Prices are valid until the date shown.",
            "Ce document est un devis (offre) et ne constitue pas une facture. Les montants sont valables jusqu'à la date indiquée.",
            "Dieses Dokument ist ein Angebot und keine Rechnung. Die Beträge gelten bis zum angegebenen Datum.",
            "Il presente documento è un preventivo (offerta) e non ha valore di fattura. Gli importi sono validi fino alla data indicata.",
            "Este documento é um orçamento (proposta) e não tem valor de fatura. Os valores são válidos até à data indicada.",
        ],
        ["Asunto factura"] = ["Factura {0}", "Invoice {0}", "Facture {0}", "Rechnung {0}", "Fattura {0}", "Fatura {0}"],
        ["Cuerpo factura"] = [
            "Adjuntamos su factura. Gracias por su confianza.",
            "Please find your invoice attached. Thank you for your business.",
            "Veuillez trouver ci-joint votre facture. Merci de votre confiance.",
            "Anbei erhalten Sie Ihre Rechnung. Vielen Dank für Ihr Vertrauen.",
            "In allegato la sua fattura. Grazie per la fiducia.",
            "Segue em anexo a sua fatura. Obrigado pela sua confiança.",
        ],
        ["Asunto presupuesto"] = ["Presupuesto {0}", "Quotation {0}", "Devis {0}", "Angebot {0}", "Preventivo {0}", "Orçamento {0}"],
        ["Cuerpo presupuesto"] = [
            "Adjuntamos nuestro presupuesto. Quedamos a su disposición para cualquier duda.",
            "Please find our quotation attached. Do not hesitate to contact us with any questions.",
            "Veuillez trouver ci-joint notre devis. Nous restons à votre disposition pour toute question.",
            "Anbei erhalten Sie unser Angebot. Für Rückfragen stehen wir Ihnen gerne zur Verfügung.",
            "In allegato il nostro preventivo. Restiamo a disposizione per qualsiasi chiarimento.",
            "Segue em anexo o nosso orçamento. Ficamos ao dispor para qualquer esclarecimento.",
        ],
    };

    /// <summary>
    /// Menciones fiscales de la factura (las de los tipos de IVA predeterminados) en los otros idiomas. La mención en
    /// castellano se imprime siempre; la traducción va detrás.
    /// </summary>
    private static readonly Dictionary<string, string[]> Menciones = new(StringComparer.Ordinal)
    {
        ["Operación exenta de IVA (art. 20 Ley 37/1992)."] = [
            "VAT-exempt transaction (art. 20 Spanish VAT Act 37/1992).",
            "Opération exonérée de TVA (art. 20 de la loi espagnole 37/1992).",
            "Steuerfreier Umsatz (Art. 20 span. UStG 37/1992).",
            "Operazione esente da IVA (art. 20 Legge spagnola 37/1992).",
            "Operação isenta de IVA (art. 20 da Lei espanhola 37/1992).",
        ],
        ["Operación no sujeta a IVA (art. 7 Ley 37/1992)."] = [
            "Transaction not subject to VAT (art. 7 Spanish VAT Act 37/1992).",
            "Opération non soumise à la TVA (art. 7 de la loi espagnole 37/1992).",
            "Nicht steuerbarer Umsatz (Art. 7 span. UStG 37/1992).",
            "Operazione non soggetta a IVA (art. 7 Legge spagnola 37/1992).",
            "Operação não sujeita a IVA (art. 7 da Lei espanhola 37/1992).",
        ],
        ["Inversión del sujeto pasivo (art. 84.Uno.2º Ley 37/1992)."] = [
            "Reverse charge (art. 84.One.2 Spanish VAT Act 37/1992).",
            "Autoliquidation (art. 84.Uno.2º de la loi espagnole 37/1992).",
            "Steuerschuldnerschaft des Leistungsempfängers (Art. 84.Uno.2º span. UStG 37/1992).",
            "Inversione contabile (art. 84.Uno.2º Legge spagnola 37/1992).",
            "Autoliquidação (art. 84.Uno.2º da Lei espanhola 37/1992).",
        ],
        ["Entrega intracomunitaria exenta (art. 25 Ley 37/1992)."] = [
            "Exempt intra-Community supply (art. 25 Spanish VAT Act 37/1992).",
            "Livraison intracommunautaire exonérée (art. 25 de la loi espagnole 37/1992).",
            "Steuerfreie innergemeinschaftliche Lieferung (Art. 25 span. UStG 37/1992).",
            "Cessione intracomunitaria non imponibile (art. 25 Legge spagnola 37/1992).",
            "Transmissão intracomunitária isenta (art. 25 da Lei espanhola 37/1992).",
        ],
        ["Operación exenta. Exportación de bienes (art. 21 Ley 37/1992)."] = [
            "Exempt transaction. Export of goods (art. 21 Spanish VAT Act 37/1992).",
            "Opération exonérée. Exportation de biens (art. 21 de la loi espagnole 37/1992).",
            "Steuerfreie Ausfuhrlieferung (Art. 21 span. UStG 37/1992).",
            "Operazione non imponibile. Esportazione di beni (art. 21 Legge spagnola 37/1992).",
            "Operação isenta. Exportação de bens (art. 21 da Lei espanhola 37/1992).",
        ],
    };

    /// <summary>Formato inglés: punto decimal y coma de miles.</summary>
    private static readonly NumberFormatInfo Ingles = new() { NumberDecimalSeparator = ".", NumberGroupSeparator = ",", NumberGroupSizes = [3] };

    /// <summary>El texto en el idioma (castellano si no está traducido); con argumentos, se formatea.</summary>
    public static string T(string? idioma, string clave, params object[] argumentos)
    {
        var i = Array.IndexOf(Orden, IdiomasDocumento.Efectivo(idioma));
        var texto = Textos.TryGetValue(clave, out var fila) ? fila[Math.Max(i, 0)] : clave;
        return argumentos.Length == 0 ? texto : string.Format(CultureInfo.InvariantCulture, texto, argumentos);
    }

    /// <summary>Siglas del impuesto: el IVA se traduce (VAT, TVA, MwSt.); el IGIC, no.</summary>
    public static string Impuesto(string? idioma, string siglas) => siglas == "IVA" ? T(idioma, "IVA") : siglas;

    /// <summary>Importe con 2 decimales (o los indicados) en el formato del idioma.</summary>
    public static string Numero(string? idioma, decimal valor, int decimales = 2) =>
        IdiomasDocumento.Efectivo(idioma) == "en"
            ? Math.Round(valor, decimales, MidpointRounding.AwayFromZero).ToString("N" + decimales.ToString(CultureInfo.InvariantCulture), Ingles)
            : decimales == 2 ? Redondeo.Formatear(valor) : Redondeo.Formatear(valor, decimales);

    /// <summary>La mención fiscal con su traducción detrás (las menciones conocidas; el resto, en castellano).</summary>
    public static string? MencionFiscal(string? idioma, string? mencion)
    {
        var i = Array.IndexOf(Orden, IdiomasDocumento.Efectivo(idioma));
        if (string.IsNullOrWhiteSpace(mencion) || i <= 0)
        {
            return mencion;
        }

        var traducidas = Menciones.Where(m => mencion.Contains(m.Key, StringComparison.Ordinal)).Select(m => m.Value[i - 1]).ToList();
        return traducidas.Count == 0 ? mencion : $"{mencion}\n{string.Join(" · ", traducidas)}";
    }
}
