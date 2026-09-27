using System.Globalization;
using System.Text;
using System.Xml;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

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
    private readonly IConsultaProveedores _proveedores;

    public GenerarSii(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaEmpresas empresas, IConsultaProveedores proveedores)
    {
        _facturas = facturas;
        _gastos = gastos;
        _empresas = empresas;
        _proveedores = proveedores;
    }

    /// <summary>Base, cuota y recargo de un tipo impositivo dentro de una factura (un DetalleIVA).</summary>
    private sealed record DetalleTipo(decimal Tipo, decimal Base, decimal Cuota, decimal TipoRecargo, decimal CuotaRecargo);

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

            // Desglose por tipo impositivo de cada factura (un DetalleIVA por tipo, no un tipo medio).
            var desgloses = new Dictionary<Guid, IReadOnlyList<DetalleTipo>>();
            foreach (var f in facturas)
            {
                var detalle = await _facturas.ObtenerAsync(f.Id, ct).ConfigureAwait(false);
                desgloses[f.Id] = detalle is { Lineas.Count: > 0 }
                    ? detalle.Lineas.GroupBy(l => (l.PorcentajeIva, l.PorcentajeRecargo)).OrderByDescending(g => g.Key.PorcentajeIva)
                        .Select(g => new DetalleTipo(g.Key.PorcentajeIva, Redondeo.Dos(g.Sum(l => l.Base)), Redondeo.Dos(g.Sum(l => l.CuotaIva)),
                            g.Key.PorcentajeRecargo, Redondeo.Dos(g.Sum(l => l.CuotaRecargo)))).ToList()
                    : [new DetalleTipo(TipoMedio(f.BaseImponible, f.CuotaIva), f.BaseImponible, f.CuotaIva, 0m, 0m)];
            }

            EscribirEmitidas(w, empresa, ejercicio, periodoTexto, facturas, desgloses);
        }
        else
        {
            // Los gastos anulados no se registran en el libro de recibidas.
            var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(g => g.Fecha >= desde && g.Fecha <= hasta && !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase))
                .OrderBy(g => g.Fecha).ToList();

            var proveedores = new Dictionary<Guid, ProveedorDto>();
            foreach (var id in gastos.Where(g => g.ProveedorId is not null).Select(g => g.ProveedorId!.Value).Distinct())
            {
                if (await _proveedores.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p)
                {
                    proveedores[id] = p;
                }
            }

            EscribirRecibidas(w, empresa, ejercicio, periodoTexto, gastos, proveedores);
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

    private static void EscribirEmitidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<FacturaResumen> facturas,
        IReadOnlyDictionary<Guid, IReadOnlyList<DetalleTipo>> desgloses)
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

            EscribirDesgloseSujeta(w, "CuotaRepercutida", desgloses[f.Id]);
            w.WriteEndElement(); // FacturaExpedida
            w.WriteEndElement(); // RegistroLRFacturasEmitidas
        }

        w.WriteEndElement();
    }

    private static void EscribirRecibidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<GastoDto> gastos,
        IReadOnlyDictionary<Guid, ProveedorDto> proveedores)
    {
        w.WriteStartElement("siiLR", "SuministroLRFacturasRecibidas", NsLr);
        EscribirCabecera(w, empresa);

        var n = 0;
        foreach (var g in gastos)
        {
            n++;
            var proveedor = g.ProveedorId is { } pid ? proveedores.GetValueOrDefault(pid) : null;
            var nombre = proveedor?.Nombre ?? g.ProveedorTexto ?? "Proveedor";
            var nif = proveedor?.NifFiscal;

            w.WriteStartElement("RegistroLRFacturasRecibidas", NsLr);

            EscribirPeriodo(w, ejercicio, periodo);

            w.WriteStartElement("IDFactura", NsLr);
            w.WriteStartElement("IDEmisorFactura", NsLr);
            EscribirIdentificacion(w, nif, nombre);
            w.WriteEndElement();
            if (g.NumeroFactura is null)
            {
                w.WriteComment(" Gasto sin número de factura del proveedor: completar antes de enviar ");
            }

            w.WriteElementString("NumSerieFacturaEmisor", NsLr, g.NumeroFactura ?? $"G{ejercicio}-{n:D4}");
            w.WriteElementString("FechaExpedicionFacturaEmisor", NsLr, (g.FechaFactura ?? g.Fecha).ToString("dd-MM-yyyy", Inv));
            w.WriteEndElement();

            w.WriteStartElement("FacturaRecibida", NsLr);
            w.WriteElementString("TipoFactura", NsLr, g.EsRectificativa ? "R1" : "F1");
            if (g.EsRectificativa)
            {
                // Rectificativa por diferencias: los importes van con su signo (negativos si es un abono).
                w.WriteElementString("TipoRectificativa", NsLr, "I");
                if (g.NumeroRectificado is not null)
                {
                    w.WriteStartElement("FacturasRectificadas", NsLr);
                    w.WriteStartElement("IDFacturaRectificada", NsLr);
                    w.WriteElementString("NumSerieFacturaEmisor", NsLr, g.NumeroRectificado);
                    w.WriteElementString("FechaExpedicionFacturaEmisor", NsLr, (g.FechaRectificada ?? g.Fecha).ToString("dd-MM-yyyy", Inv));
                    w.WriteEndElement();
                    w.WriteEndElement();
                }
            }

            w.WriteElementString("ClaveRegimenEspecialOTrascendencia", NsLr, "01");
            w.WriteElementString("ImporteTotal", NsLr, Importe(g.Total));
            w.WriteElementString("DescripcionOperacion", NsLr, string.IsNullOrWhiteSpace(g.Concepto) ? "Gasto" : g.Concepto);

            // Un detalle por tipo; lo autoliquidado (inversión del sujeto pasivo) va en su bloque.
            static DetalleTipo Detalle(DesgloseIvaDto d) =>
                new(d.PorcentajeIva, d.Base, d.Cuota, d.Base != 0m ? Redondeo.Dos(d.CuotaRecargo / d.Base * 100m) : 0m, d.CuotaRecargo);
            var desglose = g.DesgloseIva;
            w.WriteStartElement("DesgloseFactura", NsLr);
            if (desglose.Any(d => d.Autoliquidada))
            {
                w.WriteStartElement("InversionSujetoPasivo", NsLr);
                foreach (var d in desglose.Where(d => d.Autoliquidada))
                {
                    w.WriteStartElement("DetalleIVA", NsLr);
                    w.WriteElementString("TipoImpositivo", NsLr, d.PorcentajeIva.ToString("0.##", Inv));
                    w.WriteElementString("BaseImponible", NsLr, Importe(d.Base));
                    w.WriteElementString("CuotaSoportada", NsLr, Importe(d.Cuota));
                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            if (desglose.Any(d => !d.Autoliquidada))
            {
                EscribirDetallesIva(w, "CuotaSoportada", desglose.Where(d => !d.Autoliquidada).Select(Detalle).ToList());
            }

            w.WriteEndElement();

            w.WriteStartElement("Contraparte", NsLr);
            w.WriteElementString("NombreRazon", NsLr, nombre);
            EscribirIdentificacion(w, nif, nombre);
            w.WriteEndElement();

            w.WriteElementString("FechaRegContable", NsLr, g.Fecha.ToString("dd-MM-yyyy", Inv));
            w.WriteElementString("CuotaDeducible", NsLr, Importe(g.DesgloseIva.Sum(d => d.CuotaDeducible)));
            w.WriteEndElement(); // FacturaRecibida
            w.WriteEndElement(); // RegistroLRFacturasRecibidas
        }

        w.WriteEndElement();
    }

    /// <summary>
    /// Identifica al emisor/contraparte por su NIF. Sin NIF conocido se deja un IDOtro con el nombre
    /// (tipo 07, «no censado») para que se complete antes del envío.
    /// </summary>
    private static void EscribirIdentificacion(XmlWriter w, string? nif, string nombre)
    {
        if (!string.IsNullOrWhiteSpace(nif))
        {
            w.WriteElementString("NIF", NsLr, nif.Trim().ToUpperInvariant());
            return;
        }

        w.WriteComment(" Proveedor sin NIF en la ficha: completar antes de enviar ");
        w.WriteStartElement("IDOtro", NsLr);
        w.WriteElementString("IDType", NsLr, "07");
        w.WriteElementString("ID", NsLr, nombre);
        w.WriteEndElement();
    }

    private static void EscribirPeriodo(XmlWriter w, int ejercicio, string periodo)
    {
        w.WriteStartElement("PeriodoLiquidacion", NsLr);
        w.WriteElementString("Ejercicio", NsLr, ejercicio.ToString(Inv));
        w.WriteElementString("Periodo", NsLr, periodo);
        w.WriteEndElement();
    }

    private static void EscribirDesgloseSujeta(XmlWriter w, string nombreCuota, IReadOnlyList<DetalleTipo> detalles)
    {
        w.WriteStartElement("TipoDesglose", NsLr);
        w.WriteStartElement("DesgloseFactura", NsLr);
        w.WriteStartElement("Sujeta", NsLr);
        w.WriteStartElement("NoExenta", NsLr);
        w.WriteElementString("TipoNoExenta", NsLr, "S1");
        EscribirDetallesIva(w, nombreCuota, detalles);
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();
    }

    /// <summary>Un DesgloseIVA con un DetalleIVA por cada tipo impositivo (y su recargo de equivalencia, si lo hay).</summary>
    private static void EscribirDetallesIva(XmlWriter w, string nombreCuota, IReadOnlyList<DetalleTipo> detalles)
    {
        w.WriteStartElement("DesgloseIVA", NsLr);
        foreach (var d in detalles)
        {
            w.WriteStartElement("DetalleIVA", NsLr);
            w.WriteElementString("TipoImpositivo", NsLr, d.Tipo.ToString("0.##", Inv));
            w.WriteElementString("BaseImponible", NsLr, Importe(d.Base));
            w.WriteElementString(nombreCuota, NsLr, Importe(d.Cuota));
            if (d.CuotaRecargo != 0m)
            {
                w.WriteElementString("TipoRecargoEquivalencia", NsLr, d.TipoRecargo.ToString("0.##", Inv));
                w.WriteElementString("CuotaRecargoEquivalencia", NsLr, Importe(d.CuotaRecargo));
            }

            w.WriteEndElement();
        }

        w.WriteEndElement();
    }

    private static decimal TipoMedio(decimal baseImponible, decimal cuota) => baseImponible != 0m ? Redondeo.Dos(cuota / baseImponible * 100m) : 0m;

    private static string Importe(decimal valor) => Redondeo.Dos(valor).ToString("0.00", Inv);
}
