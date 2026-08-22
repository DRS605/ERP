using System.Globalization;
using System.Text;
using System.Xml;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Informes.Aplicacion;

/// <summary>Libro del Suministro Inmediato de Información (SII) a generar.</summary>
public enum TipoLibroSii
{
    /// <summary>Libro registro de facturas expedidas (emitidas).</summary>
    Emitidas = 1,

    /// <summary>Libro registro de facturas recibidas (gastos).</summary>
    Recibidas = 2,
}

/// <summary>
/// Genera el XML del <b>Suministro Inmediato de Información (SII)</b> de un periodo: el libro registro
/// de facturas expedidas o recibidas que se remitiría al servicio web de la AEAT (obligatorio para
/// grandes empresas, &gt;6 M€). Se genera con la estructura y espacios de nombres del SII; es
/// <b>mejor esfuerzo, a validar</b> con el esquema oficial. El envío en vivo (SOAP + certificado) es el
/// paso posterior, que solo requiere conectar el certificado sin rehacer esta generación.
/// </summary>
public sealed class GenerarSii
{
    private const string NsSuministro = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroInformacion.xsd";
    private const string NsLr = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws/SuministroLR.xsd";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaEmpresas _empresas;

    public GenerarSii(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaEmpresas empresas)
    {
        _facturas = facturas;
        _gastos = gastos;
        _empresas = empresas;
    }

    public async Task<Resultado<string>> EjecutarAsync(Guid empresaId, TipoLibroSii tipo, int ejercicio, int periodo, CancellationToken ct = default)
    {
        if (periodo is < 1 or > 12)
        {
            return Resultado.Fallo<string>(Error.Validacion("sii.periodo", "El periodo (mes) debe estar entre 1 y 12."));
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<string>(Error.NoEncontrado("empresa.no_encontrada", "Empresa no encontrada."));
        }

        var desde = new DateOnly(ejercicio, periodo, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);
        var periodoTexto = periodo.ToString("D2", Inv);

        var sb = new StringBuilder();
        using var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false });
        w.WriteStartDocument();

        if (tipo == TipoLibroSii.Emitidas)
        {
            var facturas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta)
                .OrderBy(f => f.FechaEmision).ThenBy(f => f.NumeroCompleto).ToList();
            EscribirEmitidas(w, empresa, ejercicio, periodoTexto, facturas);
        }
        else
        {
            var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(g => g.Fecha >= desde && g.Fecha <= hasta)
                .OrderBy(g => g.Fecha).ToList();
            EscribirRecibidas(w, empresa, ejercicio, periodoTexto, gastos);
        }

        w.WriteEndDocument();
        w.Flush();
        return Resultado.Ok(sb.ToString());
    }

    private static void EscribirCabecera(XmlWriter w, EmpresaDto empresa)
    {
        w.WriteStartElement("sii", "Cabecera", NsSuministro);
        w.WriteStartElement("IDVersionSii", NsSuministro);
        w.WriteString("1.1");
        w.WriteEndElement();
        w.WriteStartElement("Titular", NsSuministro);
        w.WriteElementString("NombreRazon", NsSuministro, empresa.RazonSocial);
        w.WriteElementString("NIF", NsSuministro, empresa.Nif);
        w.WriteEndElement();
        w.WriteElementString("TipoComunicacion", NsSuministro, "A0"); // A0 = alta de registros
        w.WriteEndElement();
    }

    private static void EscribirEmitidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<FacturaResumen> facturas)
    {
        w.WriteStartElement("siiLR", "SuministroLRFacturasEmitidas", NsLr);
        EscribirCabecera(w, empresa);

        foreach (var f in facturas)
        {
            w.WriteStartElement("RegistroLRFacturasEmitidas", NsLr);

            EscribirPeriodo(w, ejercicio, periodo);

            w.WriteStartElement("IDFactura", NsLr);
            w.WriteStartElement("IDEmisorFactura", NsLr);
            w.WriteElementString("NIF", NsLr, empresa.Nif);
            w.WriteEndElement();
            w.WriteElementString("NumSerieFacturaEmisor", NsLr, f.NumeroCompleto);
            w.WriteElementString("FechaExpedicionFacturaEmisor", NsLr, f.FechaEmision.ToString("dd-MM-yyyy", Inv));
            w.WriteEndElement();

            w.WriteStartElement("FacturaExpedida", NsLr);
            w.WriteElementString("TipoFactura", NsLr, f.Tipo == "Rectificativa" ? "R1" : "F1");
            w.WriteElementString("ClaveRegimenEspecialOTrascendencia", NsLr, "01"); // régimen general
            w.WriteElementString("ImporteTotal", NsLr, Importe(f.Total));
            w.WriteElementString("DescripcionOperacion", NsLr, "Venta");

            w.WriteStartElement("Contraparte", NsLr);
            w.WriteElementString("NombreRazon", NsLr, f.ClienteNombre);
            if (!string.IsNullOrWhiteSpace(f.ClienteNif))
            {
                w.WriteElementString("NIF", NsLr, f.ClienteNif);
            }

            w.WriteEndElement();

            EscribirDesgloseSujeta(w, "CuotaRepercutida", f.BaseImponible, f.CuotaIva);
            w.WriteEndElement(); // FacturaExpedida
            w.WriteEndElement(); // RegistroLRFacturasEmitidas
        }

        w.WriteEndElement();
    }

    private static void EscribirRecibidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<GastoDto> gastos)
    {
        w.WriteStartElement("siiLR", "SuministroLRFacturasRecibidas", NsLr);
        EscribirCabecera(w, empresa);

        var n = 0;
        foreach (var g in gastos)
        {
            n++;
            w.WriteStartElement("RegistroLRFacturasRecibidas", NsLr);

            EscribirPeriodo(w, ejercicio, periodo);

            w.WriteStartElement("IDFactura", NsLr);
            w.WriteStartElement("IDEmisorFactura", NsLr);
            // NIF del proveedor si se conoce; si no, se deja el nombre como contraparte (a completar).
            w.WriteElementString("NombreRazon", NsLr, g.ProveedorTexto ?? "Proveedor");
            w.WriteEndElement();
            w.WriteElementString("NumSerieFacturaEmisor", NsLr, $"G{ejercicio}-{n:D4}");
            w.WriteElementString("FechaExpedicionFacturaEmisor", NsLr, g.Fecha.ToString("dd-MM-yyyy", Inv));
            w.WriteEndElement();

            w.WriteStartElement("FacturaRecibida", NsLr);
            w.WriteElementString("TipoFactura", NsLr, "F1");
            w.WriteElementString("ClaveRegimenEspecialOTrascendencia", NsLr, "01");
            w.WriteElementString("ImporteTotal", NsLr, Importe(g.Total));
            w.WriteElementString("DescripcionOperacion", NsLr, string.IsNullOrWhiteSpace(g.Concepto) ? "Gasto" : g.Concepto);

            EscribirDesgloseIva(w, "CuotaSoportada", g.BaseImponible, g.CuotaIva);

            w.WriteElementString("CuotaDeducible", NsLr, Importe(g.CuotaIva));
            w.WriteElementString("FechaRegContable", NsLr, g.Fecha.ToString("dd-MM-yyyy", Inv));
            w.WriteEndElement(); // FacturaRecibida
            w.WriteEndElement(); // RegistroLRFacturasRecibidas
        }

        w.WriteEndElement();
    }

    private static void EscribirPeriodo(XmlWriter w, int ejercicio, string periodo)
    {
        w.WriteStartElement("PeriodoLiquidacion", NsLr);
        w.WriteElementString("Ejercicio", NsLr, ejercicio.ToString(Inv));
        w.WriteElementString("Periodo", NsLr, periodo);
        w.WriteEndElement();
    }

    private static void EscribirDesgloseSujeta(XmlWriter w, string nombreCuota, decimal baseImponible, decimal cuota)
    {
        w.WriteStartElement("TipoDesglose", NsLr);
        w.WriteStartElement("DesgloseFactura", NsLr);
        w.WriteStartElement("Sujeta", NsLr);
        w.WriteStartElement("NoExenta", NsLr);
        w.WriteElementString("TipoNoExenta", NsLr, "S1");
        EscribirDetalleIva(w, nombreCuota, baseImponible, cuota);
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static void EscribirDesgloseIva(XmlWriter w, string nombreCuota, decimal baseImponible, decimal cuota)
    {
        w.WriteStartElement("DesgloseFactura", NsLr);
        EscribirDetalleIva(w, nombreCuota, baseImponible, cuota);
        w.WriteEndElement();
    }

    private static void EscribirDetalleIva(XmlWriter w, string nombreCuota, decimal baseImponible, decimal cuota)
    {
        var tipo = baseImponible > 0m ? Redondeo.Dos(cuota / baseImponible * 100m) : 0m;
        w.WriteStartElement("DesgloseIVA", NsLr);
        w.WriteStartElement("DetalleIVA", NsLr);
        w.WriteElementString("TipoImpositivo", NsLr, tipo.ToString("0.##", Inv));
        w.WriteElementString("BaseImponible", NsLr, Importe(baseImponible));
        w.WriteElementString(nombreCuota, NsLr, Importe(cuota));
        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static string Importe(decimal valor) => Redondeo.Dos(valor).ToString("0.00", Inv);
}
