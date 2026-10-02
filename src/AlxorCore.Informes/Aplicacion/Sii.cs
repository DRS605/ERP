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
/// Administración a la que va el libro: la AEAT (facturas con IVA) o la Agencia Tributaria Canaria (facturas con IGIC).
/// Una empresa con actividad en la Península y en Canarias lleva los dos SII.
/// </summary>
public enum AdministracionSii
{
    /// <summary>SII de la Agencia Estatal de Administración Tributaria (IVA).</summary>
    Aeat = 1,

    /// <summary>SII-IGIC de la Agencia Tributaria Canaria.</summary>
    Atc = 2,
}

/// <summary>
/// Datos del servicio web del SII-IGIC de la Agencia Tributaria Canaria, de la configuración (<c>Sii:Atc</c>). El suministro
/// tiene la misma estructura que el de la AEAT; lo que cambia son el espacio de nombres de los esquemas y las direcciones,
/// que se toman de la documentación técnica vigente de la ATC. Sin ellos se puede generar y revisar el XML, pero no enviarlo.
/// </summary>
public sealed record OpcionesSiiAtc(string? EspacioNombres = null, string? ServidorPruebas = null, string? ServidorProduccion = null,
    string? RutaEmitidas = null, string? RutaRecibidas = null)
{
    /// <summary>Espacio de nombres provisional mientras no se configure el oficial (el XML así generado no se puede enviar).</summary>
    public const string EspacioNombresProvisional = "urn:alxor:sii-igic-atc:configurar-Sii-Atc-EspacioNombres";

    public bool Configurado => !string.IsNullOrWhiteSpace(EspacioNombres) && !string.IsNullOrWhiteSpace(ServidorPruebas)
        && !string.IsNullOrWhiteSpace(ServidorProduccion) && !string.IsNullOrWhiteSpace(RutaEmitidas) && !string.IsNullOrWhiteSpace(RutaRecibidas);

    /// <summary>Dirección del servicio del libro en el entorno indicado, o null si falta configurarla.</summary>
    public Uri? Url(TipoLibroSii libro, AlxorCore.Informes.Dominio.EntornoSii entorno)
    {
        if (!Configurado)
        {
            return null;
        }

        var servidor = (entorno == AlxorCore.Informes.Dominio.EntornoSii.Produccion ? ServidorProduccion : ServidorPruebas)!.TrimEnd('/');
        var ruta = (libro == TipoLibroSii.Emitidas ? RutaEmitidas : RutaRecibidas)!;
        return new Uri(servidor + (ruta.StartsWith('/') ? ruta : "/" + ruta), UriKind.Absolute);
    }
}

/// <summary>Un documento (factura emitida o gasto) del libro, con la huella de sus datos tal como se enviarían.</summary>
public sealed record DocumentoSii(Guid Id, string Numero, DateOnly FechaExpedicion, string Huella);

/// <summary>Documentos de un libro y mes, y la forma de escribir el XML de alta, modificación o baja de un subconjunto.</summary>
public sealed class LoteSii
{
    private readonly Func<IReadOnlyCollection<Guid>, string, string> _alta;
    private readonly Func<IReadOnlyCollection<Guid>, string> _baja;

    internal LoteSii(TipoLibroSii libro, int ejercicio, int periodo, IReadOnlyList<DocumentoSii> documentos, IReadOnlyList<DocumentoSii> anulados,
        Func<IReadOnlyCollection<Guid>, string, string> alta, Func<IReadOnlyCollection<Guid>, string> baja, AdministracionSii administracion = AdministracionSii.Aeat)
    {
        Administracion = administracion;
        Libro = libro;
        Ejercicio = ejercicio;
        Periodo = periodo;
        Documentos = documentos;
        Anulados = anulados;
        _alta = alta;
        _baja = baja;
    }

    public TipoLibroSii Libro { get; }

    /// <summary>A quién va el libro: la AEAT (IVA) o la Agencia Tributaria Canaria (IGIC).</summary>
    public AdministracionSii Administracion { get; }

    public int Ejercicio { get; }

    public int Periodo { get; }

    /// <summary>Documentos vivos del mes (se envían como alta A0 o modificación A1).</summary>
    public IReadOnlyList<DocumentoSii> Documentos { get; }

    /// <summary>Documentos anulados del mes (si se enviaron, se dan de baja).</summary>
    public IReadOnlyList<DocumentoSii> Anulados { get; }

    /// <summary>XML de alta (A0) o modificación (A1) de los documentos indicados.</summary>
    public string Alta(IReadOnlyCollection<Guid> ids, string tipoComunicacion) => _alta(ids, tipoComunicacion);

    /// <summary>XML de baja de los documentos anulados indicados.</summary>
    public string Baja(IReadOnlyCollection<Guid> ids) => _baja(ids);
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
    /// <summary>Raíz de los esquemas del SII de la AEAT; en el SII-IGIC se sustituye por la de la ATC.</summary>
    private const string RaizEsquemasAeat = "https://www2.agenciatributaria.gob.es/static_files/common/internet/dep/aplicaciones/es/aeat/ssii/fact/ws";
    private const string NsSuministro = RaizEsquemasAeat + "/SuministroInformacion.xsd";
    private const string NsLr = RaizEsquemasAeat + "/SuministroLR.xsd";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IConsultaEmpresas _empresas;
    private readonly IConsultaProveedores _proveedores;
    private readonly AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? _tipos;
    private readonly OpcionesSiiAtc _atc;

    public GenerarSii(IConsultaFacturas facturas, IConsultaGastos gastos, IConsultaEmpresas empresas, IConsultaProveedores proveedores,
        AlxorCore.Catalogo.Aplicacion.IResolverIvaEmpresa? tipos = null, OpcionesSiiAtc? atc = null)
    {
        _tipos = tipos;
        _atc = atc ?? new OpcionesSiiAtc();
        _facturas = facturas;
        _gastos = gastos;
        _empresas = empresas;
        _proveedores = proveedores;
    }

    /// <summary>Base, cuota y recargo de un tipo impositivo dentro de una factura (un DetalleIVA), con la clase de operación del tipo.</summary>
    private sealed record DetalleTipo(decimal Tipo, decimal Base, decimal Cuota, decimal TipoRecargo, decimal CuotaRecargo,
        AlxorCore.Catalogo.Dominio.ClaseIva Clase = AlxorCore.Catalogo.Dominio.ClaseIva.Ordinario);

    /// <summary>Contraparte extranjera de una emitida (país distinto de España): va con IDOtro, no con NIF.</summary>
    private sealed record ContraparteSii(string? Pais);

    /// <summary>
    /// Clave de régimen especial o trascendencia de una factura emitida, por la clase de sus tipos de IVA: 02 exportación,
    /// 03 bienes usados (REBU), 05 agencias de viajes, 07 criterio de caja; si no, 01 régimen general.
    /// </summary>
    private static string ClaveEmitida(IReadOnlyList<DetalleTipo> detalles) =>
        detalles.Select(d => d.Clase switch
        {
            AlxorCore.Catalogo.Dominio.ClaseIva.Exportacion or AlxorCore.Catalogo.Dominio.ClaseIva.Viajeros => "02",
            AlxorCore.Catalogo.Dominio.ClaseIva.BienesUsados => "03",
            AlxorCore.Catalogo.Dominio.ClaseIva.AgenciasViajes => "05",
            AlxorCore.Catalogo.Dominio.ClaseIva.CriterioCaja => "07",
            _ => null,
        }).FirstOrDefault(c => c is not null) ?? "01";

    /// <summary>Causa de exención del SII de un tipo exento: E2 exportación (art. 21), E5 intracomunitaria (art. 25), E1 art. 20, E6 otras.</summary>
    private static string CausaExencion(AlxorCore.Catalogo.Dominio.ClaseIva clase) => clase switch
    {
        AlxorCore.Catalogo.Dominio.ClaseIva.Exportacion or AlxorCore.Catalogo.Dominio.ClaseIva.Viajeros => "E2",
        AlxorCore.Catalogo.Dominio.ClaseIva.Intracomunitario => "E5",
        AlxorCore.Catalogo.Dominio.ClaseIva.Exento => "E1",
        _ => "E6",
    };

    private static bool EsExenta(AlxorCore.Catalogo.Dominio.ClaseIva c) => c is AlxorCore.Catalogo.Dominio.ClaseIva.Exento or AlxorCore.Catalogo.Dominio.ClaseIva.Exportacion
        or AlxorCore.Catalogo.Dominio.ClaseIva.Viajeros or AlxorCore.Catalogo.Dominio.ClaseIva.Intracomunitario or AlxorCore.Catalogo.Dominio.ClaseIva.OroInversion;

    public async Task<Resultado<string>> EjecutarAsync(Guid empresaId, TipoLibroSii tipo, int ejercicio, int periodo, AdministracionSii? administracion = null,
        CancellationToken ct = default)
    {
        var lote = await PrepararAsync(empresaId, tipo, ejercicio, periodo, administracion, ct).ConfigureAwait(false);
        return lote.EsFallo ? Resultado.Fallo<string>(lote.Error) : Resultado.Ok(lote.Valor.Alta(lote.Valor.Documentos.Select(d => d.Id).ToList(), "A0"));
    }

    /// <summary>
    /// Reúne los documentos del libro y del mes (los vivos, para alta o modificación, y los anulados, para una posible
    /// baja) con la huella de lo que se enviaría de cada uno, y permite escribir el XML de cualquier subconjunto.
    /// </summary>
    public async Task<Resultado<LoteSii>> PrepararAsync(Guid empresaId, TipoLibroSii tipo, int ejercicio, int periodo, AdministracionSii? administracion = null,
        CancellationToken ct = default)
    {
        if (periodo is < 1 or > 12)
        {
            return Resultado.Fallo<LoteSii>(Error.Validacion("sii.periodo", "El periodo (mes) debe estar entre 1 y 12."));
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (empresa is null)
        {
            return Resultado.Fallo<LoteSii>(Error.NoEncontrado("empresa.no_encontrada", "Empresa no encontrada."));
        }

        // Sin indicarlo, el SII del territorio de la empresa. Con actividad en los dos, cada libro lleva solo lo de su impuesto.
        var adm = administracion ?? (empresa.ImpuestoIndirecto == TipoImpuesto.Igic ? AdministracionSii.Atc : AdministracionSii.Aeat);
        var impuesto = adm == AdministracionSii.Atc ? TipoImpuesto.Igic : TipoImpuesto.Iva;
        if (!empresa.OperaEnAmbosTerritorios && impuesto != empresa.ImpuestoIndirecto)
        {
            return Resultado.Fallo<LoteSii>(Error.Validacion("sii.administracion", adm == AdministracionSii.Atc
                ? "La empresa no tributa por IGIC: no lleva el SII de la Agencia Tributaria Canaria."
                : "La empresa solo tributa por IGIC: su SII es el de la Agencia Tributaria Canaria."));
        }

        var raizAtc = string.IsNullOrWhiteSpace(_atc.EspacioNombres) ? OpcionesSiiAtc.EspacioNombresProvisional : _atc.EspacioNombres.TrimEnd('/');
        string Esquemas(string xml) => adm == AdministracionSii.Atc ? xml.Replace(RaizEsquemasAeat, raizAtc, StringComparison.Ordinal) : xml;

        var desde = new DateOnly(ejercicio, periodo, 1);
        var hasta = desde.AddMonths(1).AddDays(-1);
        var periodoTexto = periodo.ToString("D2", Inv);

        if (tipo == TipoLibroSii.Emitidas)
        {
            var todas = (await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false))
                // Con actividad en los dos territorios, al SII de la AEAT van las del IVA y al de la Agencia Tributaria Canaria las del IGIC.
                .Where(f => f.FechaEmision >= desde && f.FechaEmision <= hasta && (!empresa.OperaEnAmbosTerritorios || f.Impuesto == impuesto))
                .OrderBy(f => f.FechaEmision).ThenBy(f => f.NumeroCompleto, StringComparer.Ordinal).ToList();

            // Desglose por tipo impositivo de cada factura (un DetalleIVA por tipo, no un tipo medio).
            var desgloses = new Dictionary<Guid, IReadOnlyList<DetalleTipo>>();
            var contrapartes = new Dictionary<Guid, ContraparteSii>();
            var clases = new Dictionary<string, AlxorCore.Catalogo.Dominio.ClaseIva>(StringComparer.OrdinalIgnoreCase);
            async Task<AlxorCore.Catalogo.Dominio.ClaseIva> ClaseAsync(string codigo)
            {
                if (!clases.TryGetValue(codigo, out var c))
                {
                    c = (_tipos is null ? null : await _tipos.ResolverAsync(empresaId, codigo, ct).ConfigureAwait(false))?.Clase
                        ?? (string.Equals(codigo, Impuesto.IvaExento.Codigo, StringComparison.OrdinalIgnoreCase) ? AlxorCore.Catalogo.Dominio.ClaseIva.Exento
                            : AlxorCore.Catalogo.Dominio.ClaseIva.Ordinario);
                    clases[codigo] = c;
                }

                return c;
            }

            foreach (var f in todas)
            {
                var detalle = await _facturas.ObtenerAsync(f.Id, ct).ConfigureAwait(false);
                if (detalle is { Lineas.Count: > 0 })
                {
                    var lista = new List<DetalleTipo>();
                    foreach (var g in detalle.Lineas.GroupBy(l => (l.CodigoIva, l.PorcentajeIva, l.PorcentajeRecargo)).OrderByDescending(g => g.Key.PorcentajeIva))
                    {
                        lista.Add(new DetalleTipo(g.Key.PorcentajeIva, Redondeo.Dos(g.Sum(l => l.Base)), Redondeo.Dos(g.Sum(l => l.CuotaIva)),
                            g.Key.PorcentajeRecargo, Redondeo.Dos(g.Sum(l => l.CuotaRecargo)), await ClaseAsync(g.Key.CodigoIva).ConfigureAwait(false)));
                    }

                    desgloses[f.Id] = lista;
                    contrapartes[f.Id] = new ContraparteSii(detalle.ClientePais);
                }
                else
                {
                    desgloses[f.Id] = [new DetalleTipo(TipoMedio(f.BaseImponible, f.CuotaIva), f.BaseImponible, f.CuotaIva, 0m, 0m)];
                }
            }

            // Una factura anulada (VeriFactu) no se da de alta; si se envió antes, se da de baja.
            var vivas = todas.Where(f => f.Estado != "Anulada").ToList();
            var anuladas = todas.Where(f => f.Estado == "Anulada").ToList();
            string Alta(IReadOnlyCollection<Guid> ids, string tipoComunicacion) => Esquemas(Documento(w =>
                EscribirEmitidas(w, empresa, ejercicio, periodoTexto, vivas.Where(f => ids.Contains(f.Id)).ToList(), desgloses, contrapartes, tipoComunicacion)));
            string Baja(IReadOnlyCollection<Guid> ids) => Esquemas(Documento(w =>
                EscribirBajas(w, empresa, ejercicio, periodoTexto, "BajaLRFacturasEmitidas", "RegistroLRBajaExpedidas",
                    anuladas.Where(f => ids.Contains(f.Id)).Select(f => (Nif: (string?)empresa.Nif, Nombre: empresa.RazonSocial, f.NumeroCompleto, f.FechaEmision)).ToList())));

            return Resultado.Ok(new LoteSii(tipo, ejercicio, periodo,
                vivas.Select(f => new DocumentoSii(f.Id, f.NumeroCompleto, f.FechaEmision, Huella(Alta([f.Id], "A0")))).ToList(),
                anuladas.Select(f => new DocumentoSii(f.Id, f.NumeroCompleto, f.FechaEmision, string.Empty)).ToList(),
                Alta, Baja, adm));
        }

        var gastos = (await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(g => g.Fecha >= desde && g.Fecha <= hasta && (!empresa.OperaEnAmbosTerritorios || Impuesto.TipoDeCodigo(g.CodigoIva) == impuesto))
            .OrderBy(g => g.Fecha).ThenBy(g => g.Id).ToList();
        var proveedores = new Dictionary<Guid, ProveedorDto>();
        foreach (var id in gastos.Where(g => g.ProveedorId is not null).Select(g => g.ProveedorId!.Value).Distinct())
        {
            if (await _proveedores.ObtenerAsync(id, ct).ConfigureAwait(false) is { } p)
            {
                proveedores[id] = p;
            }
        }

        // Los gastos anulados no se registran en el libro de recibidas (y se dan de baja si se habían enviado).
        var vivos = gastos.Where(g => !string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase)).ToList();
        var anulados = gastos.Where(g => string.Equals(g.Estado, "Anulado", StringComparison.OrdinalIgnoreCase)).ToList();
        string AltaR(IReadOnlyCollection<Guid> ids, string tipoComunicacion) => Esquemas(Documento(w =>
            EscribirRecibidas(w, empresa, ejercicio, periodoTexto, vivos.Where(g => ids.Contains(g.Id)).ToList(), proveedores, tipoComunicacion)));
        string BajaR(IReadOnlyCollection<Guid> ids) => Esquemas(Documento(w =>
            EscribirBajas(w, empresa, ejercicio, periodoTexto, "BajaLRFacturasRecibidas", "RegistroLRBajaRecibidas",
                anulados.Where(g => ids.Contains(g.Id)).Select(g =>
                {
                    var p = g.ProveedorId is { } pid ? proveedores.GetValueOrDefault(pid) : null;
                    return (Nif: p?.NifFiscal, Nombre: p?.Nombre ?? g.ProveedorTexto ?? "Proveedor", NumeroRecibida(g, ejercicio), g.FechaFactura ?? g.Fecha);
                }).ToList())));

        return Resultado.Ok(new LoteSii(tipo, ejercicio, periodo,
            vivos.Select(g => new DocumentoSii(g.Id, NumeroRecibida(g, ejercicio), g.FechaFactura ?? g.Fecha, Huella(AltaR([g.Id], "A0")))).ToList(),
            anulados.Select(g => new DocumentoSii(g.Id, NumeroRecibida(g, ejercicio), g.FechaFactura ?? g.Fecha, string.Empty)).ToList(),
            AltaR, BajaR, adm));
    }

    /// <summary>Número de la factura recibida; sin el del proveedor, uno estable a partir del gasto (a completar antes de enviar).</summary>
    private static string NumeroRecibida(GastoDto g, int ejercicio) => g.NumeroFactura ?? $"G{ejercicio}-{g.Id.ToString("N", Inv)[..8].ToUpperInvariant()}";

    private static string Documento(Action<XmlWriter> escribir)
    {
        var sb = new StringBuilder();
        using (var w = XmlWriter.Create(sb, new XmlWriterSettings { Indent = true, Encoding = Encoding.UTF8, OmitXmlDeclaration = false }))
        {
            w.WriteStartDocument();
            escribir(w);
            w.WriteEndDocument();
        }

        return sb.ToString();
    }

    private static string Huella(string xml) =>
        Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(xml)));

    private static void EscribirBajas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, string raiz, string registro,
        IReadOnlyList<(string? Nif, string Nombre, string Numero, DateOnly Fecha)> documentos)
    {
        w.WriteStartElement("siiLR", raiz, NsLr);
        EscribirCabecera(w, empresa, null);
        foreach (var d in documentos)
        {
            w.WriteStartElement(registro, NsLr);
            EscribirPeriodo(w, ejercicio, periodo);
            w.WriteStartElement("IDFactura", NsLr);
            w.WriteStartElement("IDEmisorFactura", NsLr);
            EscribirIdentificacion(w, d.Nif, d.Nombre);
            w.WriteEndElement();
            w.WriteElementString("NumSerieFacturaEmisor", NsLr, d.Numero);
            w.WriteElementString("FechaExpedicionFacturaEmisor", NsLr, d.Fecha.ToString("dd-MM-yyyy", Inv));
            w.WriteEndElement();
            w.WriteEndElement();
        }

        w.WriteEndElement();
    }

    private static void EscribirCabecera(XmlWriter w, EmpresaDto empresa, string? tipoComunicacion)
    {
        w.WriteStartElement("sii", "Cabecera", NsSuministro);
        w.WriteStartElement("IDVersionSii", NsSuministro);
        w.WriteString("1.1");
        w.WriteEndElement();
        w.WriteStartElement("Titular", NsSuministro);
        w.WriteElementString("NombreRazon", NsSuministro, empresa.RazonSocial);
        w.WriteElementString("NIF", NsSuministro, empresa.Nif);
        w.WriteEndElement();
        // A0 = alta, A1 = modificación; las bajas no llevan tipo de comunicación.
        if (tipoComunicacion is not null)
        {
            w.WriteElementString("TipoComunicacion", NsSuministro, tipoComunicacion);
        }

        w.WriteEndElement();
    }

    private static void EscribirEmitidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<FacturaResumen> facturas,
        IReadOnlyDictionary<Guid, IReadOnlyList<DetalleTipo>> desgloses, IReadOnlyDictionary<Guid, ContraparteSii> contrapartes, string tipoComunicacion)
    {
        w.WriteStartElement("siiLR", "SuministroLRFacturasEmitidas", NsLr);
        EscribirCabecera(w, empresa, tipoComunicacion);

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
            var detalles = desgloses[f.Id];
            w.WriteElementString("ClaveRegimenEspecialOTrascendencia", NsLr, ClaveEmitida(detalles));
            w.WriteElementString("ImporteTotal", NsLr, Importe(f.Total));
            w.WriteElementString("DescripcionOperacion", NsLr, "Venta");

            // Contraparte extranjera: IDOtro con el país (02 NIF-IVA en la UE, 04 documento del país fuera de ella).
            var pais = contrapartes.GetValueOrDefault(f.Id)?.Pais?.Trim().ToUpperInvariant();
            var extranjera = pais is { Length: 2 } && pais != "ES";
            w.WriteStartElement("Contraparte", NsLr);
            w.WriteElementString("NombreRazon", NsLr, f.ClienteNombre);
            if (extranjera)
            {
                w.WriteStartElement("IDOtro", NsLr);
                w.WriteElementString("CodigoPais", NsLr, pais!);
                w.WriteElementString("IDType", NsLr, detalles.Any(d => d.Clase == AlxorCore.Catalogo.Dominio.ClaseIva.Intracomunitario) || PaisesUe.Contains(pais!) ? "02" : "04");
                w.WriteElementString("ID", NsLr, string.IsNullOrWhiteSpace(f.ClienteNif) ? f.ClienteNombre : f.ClienteNif);
                w.WriteEndElement();
            }
            else if (!string.IsNullOrWhiteSpace(f.ClienteNif))
            {
                w.WriteElementString("NIF", NsLr, f.ClienteNif);
            }

            w.WriteEndElement();

            EscribirDesgloseEmitida(w, detalles, extranjera);
            w.WriteEndElement(); // FacturaExpedida
            w.WriteEndElement(); // RegistroLRFacturasEmitidas
        }

        w.WriteEndElement();
    }

    private static void EscribirRecibidas(XmlWriter w, EmpresaDto empresa, int ejercicio, string periodo, IReadOnlyList<GastoDto> gastos,
        IReadOnlyDictionary<Guid, ProveedorDto> proveedores, string tipoComunicacion)
    {
        w.WriteStartElement("siiLR", "SuministroLRFacturasRecibidas", NsLr);
        EscribirCabecera(w, empresa, tipoComunicacion);

        foreach (var g in gastos)
        {
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

            w.WriteElementString("NumSerieFacturaEmisor", NsLr, NumeroRecibida(g, ejercicio));
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

            // Clave 02: compensaciones del régimen especial de la agricultura (autofacturas o recibos REAGP a agricultores).
            var reagp = g.DesgloseIva.Any(d => d.CodigoIva.StartsWith("REAGP", StringComparison.OrdinalIgnoreCase));
            w.WriteElementString("ClaveRegimenEspecialOTrascendencia", NsLr, reagp ? "02" : "01");
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

            if (reagp)
            {
                // La compensación REAGP no es IVA: se declara con su porcentaje e importe de compensación.
                w.WriteStartElement("DesgloseIVA", NsLr);
                foreach (var d in desglose.Where(d => !d.Autoliquidada))
                {
                    w.WriteStartElement("DetalleIVA", NsLr);
                    w.WriteElementString("BaseImponible", NsLr, Importe(d.Base));
                    w.WriteElementString("PorcentCompensacionREAGYP", NsLr, d.PorcentajeIva.ToString("0.##", Inv));
                    w.WriteElementString("ImporteCompensacionREAGYP", NsLr, Importe(d.Cuota));
                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }
            else if (desglose.Any(d => !d.Autoliquidada))
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

    private static readonly HashSet<string> PaisesUe = new(StringComparer.Ordinal)
    {
        "AT", "BE", "BG", "CY", "CZ", "DE", "DK", "EE", "EL", "GR", "FI", "FR", "HR", "HU", "IE", "IT", "LT", "LU", "LV", "MT", "NL", "PL", "PT", "RO", "SE", "SI", "SK",
    };

    /// <summary>
    /// Desglose de una emitida: sujeta y exenta (con su causa), sujeta y no exenta (S1, o S2 si es inversión del sujeto
    /// pasivo) y no sujeta. Con contraparte extranjera va en DesgloseTipoOperacion (entrega de bienes), como pide la AEAT.
    /// </summary>
    private static void EscribirDesgloseEmitida(XmlWriter w, IReadOnlyList<DetalleTipo> detalles, bool extranjera)
    {
        var exentas = detalles.Where(d => EsExenta(d.Clase)).ToList();
        var noSujetas = detalles.Where(d => d.Clase == AlxorCore.Catalogo.Dominio.ClaseIva.NoSujeto).ToList();
        var isp = detalles.Where(d => d.Clase == AlxorCore.Catalogo.Dominio.ClaseIva.InversionSujetoPasivo).ToList();
        var noExentas = detalles.Except(exentas).Except(noSujetas).Except(isp).ToList();
        if (exentas.Count == 0 && noSujetas.Count == 0 && isp.Count == 0 && !extranjera)
        {
            EscribirDesgloseSujeta(w, "CuotaRepercutida", detalles);
            return;
        }

        w.WriteStartElement("TipoDesglose", NsLr);
        w.WriteStartElement(extranjera ? "DesgloseTipoOperacion" : "DesgloseFactura", NsLr);
        if (extranjera)
        {
            w.WriteStartElement("Entrega", NsLr);
        }

        if (exentas.Count > 0 || noExentas.Count > 0 || isp.Count > 0)
        {
            w.WriteStartElement("Sujeta", NsLr);
            if (exentas.Count > 0)
            {
                w.WriteStartElement("Exenta", NsLr);
                foreach (var g in exentas.GroupBy(d => CausaExencion(d.Clase)))
                {
                    w.WriteStartElement("DetalleExenta", NsLr);
                    w.WriteElementString("CausaExencion", NsLr, g.Key);
                    w.WriteElementString("BaseImponible", NsLr, Importe(g.Sum(d => d.Base)));
                    w.WriteEndElement();
                }

                w.WriteEndElement();
            }

            if (noExentas.Count > 0 || isp.Count > 0)
            {
                w.WriteStartElement("NoExenta", NsLr);
                w.WriteElementString("TipoNoExenta", NsLr, noExentas.Count > 0 && isp.Count > 0 ? "S3" : isp.Count > 0 ? "S2" : "S1");
                EscribirDetallesIva(w, "CuotaRepercutida", noExentas.Concat(isp).ToList());
                w.WriteEndElement();
            }

            w.WriteEndElement();
        }

        if (noSujetas.Count > 0)
        {
            w.WriteStartElement("NoSujeta", NsLr);
            w.WriteElementString("ImportePorArticulos7_14_Otros", NsLr, Importe(noSujetas.Sum(d => d.Base)));
            w.WriteEndElement();
        }

        if (extranjera)
        {
            w.WriteEndElement();
        }

        w.WriteEndElement();
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
