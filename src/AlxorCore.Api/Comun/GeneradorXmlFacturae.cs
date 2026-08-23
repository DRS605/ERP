using System.Globalization;
using System.Text;
using System.Xml;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Genera la <b>factura electrónica española (Facturae) versión 3.2.2</b> en XML a partir de una
/// factura emitida. Es el formato estructurado que exige la Administración (FACe) y que admiten
/// gestorías y clientes que trabajan con factura-e. Cuando el destinatario es una Administración
/// Pública, se incluyen sus <b>centros administrativos DIR3</b> (Oficina Contable, Órgano Gestor y
/// Unidad Tramitadora), que FACe usa para el enrutado del documento.
/// </summary>
/// <remarks>
/// Se genera el documento de negocio <b>sin firmar</b>. La <b>firma electrónica XAdES</b> (envuelta,
/// obligatoria para presentar en FACe) y el <b>envío al servicio web de FACe</b> son el paso
/// posterior, que solo requiere conectar el certificado de la empresa sin rehacer este XML — igual
/// que en VeriFactu, donde el envío en vivo a la AEAT queda a expensas del certificado.
/// </remarks>
public static class GeneradorXmlFacturae
{
    private const string Ns = "http://www.facturae.gob.es/formato/Versiones/Facturaev3_2_2.xml";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Genera el XML Facturae 3.2.2 de la factura. <paramref name="cliente"/> aporta los centros DIR3 y el email del destinatario (puede ser nulo en tickets).</summary>
    public static string Generar(FacturaDto factura, EmpresaDto emisor, ClienteDto? cliente)
    {
        ArgumentNullException.ThrowIfNull(factura);
        ArgumentNullException.ThrowIfNull(emisor);

        var sb = new StringBuilder();
        var ajustes = new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false };
        using var w = XmlWriter.Create(sb, ajustes);

        w.WriteStartDocument();
        w.WriteStartElement("Facturae", Ns);

        EscribirCabecera(w, factura, emisor);
        EscribirPartes(w, factura, emisor, cliente);

        Ini(w, "Invoices");
        EscribirFactura(w, factura);
        w.WriteEndElement(); // Invoices

        w.WriteEndElement(); // Facturae
        w.WriteEndDocument();
        w.Flush();
        return sb.ToString();
    }

    private static void EscribirCabecera(XmlWriter w, FacturaDto factura, EmpresaDto emisor)
    {
        var invoiceTotal = InvoiceTotal(factura);
        var ejecutable = factura.Total;

        Ini(w, "FileHeader");
        El(w, "SchemaVersion", "3.2.2");
        El(w, "Modality", "I"); // I = factura individual
        El(w, "InvoiceIssuerType", "EM"); // EM = emisor
        Ini(w, "Batch");
        El(w, "BatchIdentifier", emisor.Nif + factura.NumeroCompleto);
        El(w, "InvoicesCount", "1");
        Ini(w, "TotalInvoicesAmount");
        El(w, "TotalAmount", Dec(invoiceTotal));
        w.WriteEndElement();
        Ini(w, "TotalOutstandingAmount");
        El(w, "TotalAmount", Dec(invoiceTotal));
        w.WriteEndElement();
        Ini(w, "TotalExecutableAmount");
        El(w, "TotalAmount", Dec(ejecutable));
        w.WriteEndElement();
        El(w, "InvoiceCurrencyCode", "EUR");
        w.WriteEndElement(); // Batch
        w.WriteEndElement(); // FileHeader
    }

    private static void EscribirPartes(XmlWriter w, FacturaDto factura, EmpresaDto emisor, ClienteDto? cliente)
    {
        Ini(w, "Parties");

        // --- Vendedor (la empresa) ---
        Ini(w, "SellerParty");
        EscribirIdentificacionFiscal(w, emisor.Nif, emisor.Pais);
        EscribirPersona(
            w, emisor.Nif, emisor.RazonSocial,
            emisor.Calle, emisor.CodigoPostal, emisor.Poblacion, emisor.Provincia, emisor.Pais,
            emisor.EmailContacto, centros: null);
        w.WriteEndElement(); // SellerParty

        // --- Comprador (el cliente de la factura) ---
        Ini(w, "BuyerParty");
        EscribirIdentificacionFiscal(w, factura.ClienteNif, factura.ClientePais);
        EscribirPersona(
            w, factura.ClienteNif, factura.ClienteNombre,
            factura.ClienteCalle, factura.ClienteCodigoPostal, factura.ClientePoblacion, factura.ClienteProvincia, factura.ClientePais,
            cliente?.Email, centros: cliente is { CentrosDir3Completos: true } ? cliente : null);
        w.WriteEndElement(); // BuyerParty

        w.WriteEndElement(); // Parties
    }

    private static void EscribirIdentificacionFiscal(XmlWriter w, string? nif, string pais)
    {
        Ini(w, "TaxIdentification");
        El(w, "PersonTypeCode", TipoPersona(nif));
        El(w, "ResidenceTypeCode", EsEspana(pais) ? "R" : "E");
        El(w, "TaxIdentificationNumber", nif ?? string.Empty);
        w.WriteEndElement();
    }

    private static void EscribirPersona(
        XmlWriter w, string? nif, string nombre,
        string calle, string cp, string poblacion, string provincia, string pais,
        string? email, ClienteDto? centros)
    {
        // Los centros administrativos DIR3 van antes de la entidad (orden del esquema Facturae).
        if (centros is not null)
        {
            Ini(w, "AdministrativeCentres");
            EscribirCentro(w, centros, centros.Dir3OficinaContable!, "01", "Oficina contable", calle, cp, poblacion, provincia, pais);
            EscribirCentro(w, centros, centros.Dir3OrganoGestor!, "02", "Órgano gestor", calle, cp, poblacion, provincia, pais);
            EscribirCentro(w, centros, centros.Dir3UnidadTramitadora!, "03", "Unidad tramitadora", calle, cp, poblacion, provincia, pais);
            w.WriteEndElement();
        }

        if (TipoPersona(nif) == "F")
        {
            // Persona física (autónomo): nombre y primer apellido en un único hueco disponible.
            Ini(w, "Individual");
            El(w, "Name", Recorta(nombre, 40));
            El(w, "FirstSurname", "-");
            EscribirDireccion(w, calle, cp, poblacion, provincia, pais);
            EscribirContacto(w, email);
            w.WriteEndElement();
        }
        else
        {
            Ini(w, "LegalEntity");
            El(w, "CorporateName", Recorta(nombre, 80));
            EscribirDireccion(w, calle, cp, poblacion, provincia, pais);
            EscribirContacto(w, email);
            w.WriteEndElement();
        }
    }

    private static void EscribirCentro(
        XmlWriter w, ClienteDto c, string codigo, string rol, string nombre,
        string calle, string cp, string poblacion, string provincia, string pais)
    {
        Ini(w, "AdministrativeCentre");
        El(w, "CentreCode", codigo);
        El(w, "RoleTypeCode", rol);
        El(w, "Name", nombre);
        EscribirDireccion(w, calle, cp, poblacion, provincia, pais);
        w.WriteEndElement();
    }

    private static void EscribirDireccion(XmlWriter w, string calle, string cp, string poblacion, string provincia, string pais)
    {
        if (EsEspana(pais))
        {
            Ini(w, "AddressInSpain");
            El(w, "Address", PorDefecto(calle, "-"));
            El(w, "PostCode", PorDefecto(cp, "00000"));
            El(w, "Town", PorDefecto(poblacion, "-"));
            El(w, "Province", PorDefecto(provincia, "-"));
            El(w, "CountryCode", "ESP");
            w.WriteEndElement();
        }
        else
        {
            Ini(w, "OverseasAddress");
            El(w, "Address", PorDefecto(calle, "-"));
            El(w, "PostCodeAndTown", $"{cp} {poblacion}".Trim());
            El(w, "Province", PorDefecto(provincia, "-"));
            El(w, "CountryCode", CodigoPais(pais));
            w.WriteEndElement();
        }
    }

    private static void EscribirContacto(XmlWriter w, string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        Ini(w, "ContactDetails");
        El(w, "ElectronicMail", email.Trim());
        w.WriteEndElement();
    }

    private static void EscribirFactura(XmlWriter w, FacturaDto factura)
    {
        Ini(w, "Invoice");

        Ini(w, "InvoiceHeader");
        El(w, "InvoiceNumber", factura.NumeroCompleto);
        El(w, "InvoiceDocumentType", factura.Tipo == "Simplificada" ? "FA" : "FC");
        El(w, "InvoiceClass", factura.Tipo == "Rectificativa" ? "OR" : "OO");
        w.WriteEndElement();

        Ini(w, "InvoiceIssueData");
        El(w, "IssueDate", factura.FechaEmision.ToString("yyyy-MM-dd", Inv));
        El(w, "InvoiceCurrencyCode", "EUR");
        El(w, "TaxCurrencyCode", "EUR");
        El(w, "LanguageName", "es");
        w.WriteEndElement();

        // IVA (y recargo de equivalencia) repercutido, agrupado por tipo.
        Ini(w, "TaxesOutputs");
        foreach (var grupo in factura.Lineas.GroupBy(l => l.PorcentajeIva))
        {
            EscribirTax(
                w, "01", grupo.Key,
                grupo.Sum(l => l.Base), grupo.Sum(l => l.CuotaIva),
                grupo.Where(l => l.CuotaRecargo > 0m).Select(l => l.PorcentajeRecargo).DefaultIfEmpty(0m).First(),
                grupo.Sum(l => l.CuotaRecargo));
        }

        w.WriteEndElement();

        // Retención de IRPF (impuesto retenido a nivel de factura).
        if (factura.RetencionIrpf > 0m)
        {
            Ini(w, "TaxesWithheld");
            EscribirTax(w, "04", factura.PorcentajeIrpf, factura.BaseImponible, factura.RetencionIrpf, 0m, 0m);
            w.WriteEndElement();
        }

        EscribirTotales(w, factura);
        EscribirLineas(w, factura);

        w.WriteEndElement(); // Invoice
    }

    private static void EscribirTotales(XmlWriter w, FacturaDto factura)
    {
        var invoiceTotal = InvoiceTotal(factura);

        Ini(w, "InvoiceTotals");
        El(w, "TotalGrossAmount", Dec(factura.BaseImponible));
        El(w, "TotalGrossAmountBeforeTaxes", Dec(factura.BaseImponible));
        El(w, "TotalTaxOutputs", Dec(factura.CuotaIva + factura.RecargoTotal));
        El(w, "TotalTaxesWithheld", Dec(factura.RetencionIrpf));
        El(w, "InvoiceTotal", Dec(invoiceTotal));
        El(w, "TotalOutstandingAmount", Dec(invoiceTotal));
        El(w, "TotalExecutableAmount", Dec(factura.Total));
        w.WriteEndElement();
    }

    private static void EscribirLineas(XmlWriter w, FacturaDto factura)
    {
        Ini(w, "Items");
        foreach (var l in factura.Lineas)
        {
            var totalCoste = Redondear(l.Cantidad * l.PrecioUnitario);
            var descuento = Redondear(totalCoste - l.Base);

            Ini(w, "InvoiceLine");
            El(w, "ItemDescription", Recorta(l.Descripcion, 2500));
            El(w, "Quantity", Cant(l.Cantidad));
            El(w, "UnitOfMeasure", "01"); // 01 = unidades
            El(w, "UnitPriceWithoutTax", Precio(l.PrecioUnitario));
            El(w, "TotalCost", Dec(totalCoste));

            if (descuento > 0m)
            {
                Ini(w, "DiscountsAndRebates");
                Ini(w, "Discount");
                El(w, "DiscountReason", "Descuento");
                El(w, "DiscountRate", l.PorcentajeDescuento.ToString("F2", Inv));
                El(w, "DiscountAmount", Dec(descuento));
                w.WriteEndElement();
                w.WriteEndElement();
            }

            El(w, "GrossAmount", Dec(l.Base));

            Ini(w, "TaxesOutputs");
            EscribirTax(w, "01", l.PorcentajeIva, l.Base, l.CuotaIva, l.PorcentajeRecargo, l.CuotaRecargo);
            w.WriteEndElement();

            w.WriteEndElement(); // InvoiceLine
        }

        w.WriteEndElement(); // Items
    }

    private static void EscribirTax(XmlWriter w, string tipo, decimal tasa, decimal baseImp, decimal cuota, decimal tasaRecargo, decimal cuotaRecargo)
    {
        Ini(w, "Tax");
        El(w, "TaxTypeCode", tipo);
        El(w, "TaxRate", tasa.ToString("F2", Inv));
        Ini(w, "TaxableBase");
        El(w, "TotalAmount", Dec(Redondear(baseImp)));
        w.WriteEndElement();
        Ini(w, "TaxAmount");
        El(w, "TotalAmount", Dec(Redondear(cuota)));
        w.WriteEndElement();
        if (cuotaRecargo > 0m)
        {
            El(w, "EquivalenceSurcharge", tasaRecargo.ToString("F2", Inv));
            Ini(w, "EquivalenceSurchargeAmount");
            El(w, "TotalAmount", Dec(Redondear(cuotaRecargo)));
            w.WriteEndElement();
        }

        w.WriteEndElement(); // Tax
    }

    // --- Utilidades ---
    private static decimal InvoiceTotal(FacturaDto f) => Redondear(f.BaseImponible + f.CuotaIva + f.RecargoTotal);

    private static void Ini(XmlWriter w, string nombre) => w.WriteStartElement(nombre, Ns);

    private static void El(XmlWriter w, string nombre, string valor) => w.WriteElementString(nombre, Ns, valor);

    private static string Dec(decimal v) => Redondear(v).ToString("F2", Inv);

    private static string Cant(decimal v) => v.ToString("0.######", Inv);

    private static string Precio(decimal v) => Math.Round(v, 6, MidpointRounding.AwayFromZero).ToString("0.000000", Inv);

    private static decimal Redondear(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static bool EsEspana(string? pais) => string.IsNullOrWhiteSpace(pais) || pais.Trim().ToUpperInvariant() is "ES" or "ESP";

    private static string PorDefecto(string valor, string defecto) => string.IsNullOrWhiteSpace(valor) ? defecto : valor.Trim();

    private static string Recorta(string valor, int max)
    {
        var limpio = (valor ?? string.Empty).Trim();
        return limpio.Length > max ? limpio[..max] : limpio;
    }

    /// <summary>Tipo de persona Facturae: F = física (NIF de persona), J = jurídica (resto). Sin NIF se asume jurídica.</summary>
    private static string TipoPersona(string? nif)
    {
        if (string.IsNullOrWhiteSpace(nif))
        {
            return "J";
        }

        var primera = char.ToUpperInvariant(nif.Trim()[0]);

        // NIF de persona física: empieza por dígito (DNI) o por las letras de NIE/especiales.
        return char.IsDigit(primera) || primera is 'X' or 'Y' or 'Z' or 'K' or 'L' or 'M' ? "F" : "J";
    }

    /// <summary>Convierte un código de país ISO alfa-2 al alfa-3 que usa Facturae (mapa de los habituales).</summary>
    private static string CodigoPais(string? pais)
    {
        var p = (pais ?? "ES").Trim().ToUpperInvariant();
        return p switch
        {
            "ES" or "ESP" => "ESP",
            "PT" => "PRT",
            "FR" => "FRA",
            "DE" => "DEU",
            "IT" => "ITA",
            "GB" or "UK" => "GBR",
            "US" => "USA",
            "NL" => "NLD",
            "BE" => "BEL",
            "IE" => "IRL",
            _ => p.Length == 3 ? p : "ESP",
        };
    }
}
