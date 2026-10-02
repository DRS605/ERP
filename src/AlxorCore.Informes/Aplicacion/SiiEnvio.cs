using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using AlxorCore.Informes.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Informes.Aplicacion;

// ----------------------------------------------------------------------------- Puertos
/// <summary>Persistencia del SII: certificado de la empresa, envíos y situación de cada factura.</summary>
public interface IRepositorioSii
{
    Task<CertificadoSii?> CertificadoAsync(Guid empresaId, CancellationToken ct = default);
    void Agregar(CertificadoSii certificado);
    void Eliminar(CertificadoSii certificado);
    Task<IReadOnlyList<RegistroSii>> RegistrosAsync(Guid empresaId, string libro, int ejercicio, int periodo, CancellationToken ct = default);
    void Agregar(RegistroSii registro);
    void Agregar(EnvioSii envio);
    Task<IReadOnlyList<EnvioSii>> EnviosAsync(Guid empresaId, int limite, CancellationToken ct = default);
    Task<EnvioSii?> EnvioAsync(Guid id, CancellationToken ct = default);
    Task GuardarAsync(CancellationToken ct = default);
}

/// <summary>Cifra y descifra secretos en reposo (el certificado y su contraseña).</summary>
public interface IProtectorSecretos
{
    byte[] Proteger(byte[] datos);
    byte[] Desproteger(byte[] datos);
}

/// <summary>Respuesta HTTP del servicio web (o el error de comunicación, si no llegó).</summary>
public sealed record RespuestaTransporteSii(int? CodigoHttp, string? Cuerpo, string? Error);

/// <summary>Llamada SOAP al servicio web del SII con el certificado de la empresa (autenticación TLS de cliente).</summary>
public interface ITransporteSii
{
    Task<RespuestaTransporteSii> EnviarAsync(Uri destino, TipoLibroSii libro, EntornoSii entorno, X509Certificate2 certificado, string sobreSoap, CancellationToken ct = default);
}

// ----------------------------------------------------------------------------- DTOs
public sealed record CertificadoSiiDto(string Titular, string? Nif, DateTimeOffset CaducaEn, string Entorno, bool Caducado, DateTimeOffset ActualizadoEn);

public sealed record SubirCertificadoSiiComando(string PfxBase64, string Clave, EntornoSii Entorno = EntornoSii.Pruebas);

public sealed record EnviarSiiComando(TipoLibroSii Libro, int Ejercicio, int Periodo, AdministracionSii? Administracion = null);

public sealed record EnvioSiiDto(Guid Id, string Libro, int Ejercicio, int Periodo, string TipoComunicacion, string Entorno, int Registros, int Correctos,
    int ConErrores, int Incorrectos, string Estado, string? Csv, string? Error, DateTimeOffset EnviadoEn)
{
    public static EnvioSiiDto Desde(EnvioSii e) => new(e.Id, e.Libro, e.Ejercicio, e.Periodo, e.TipoComunicacion, e.Entorno.ToString(), e.Registros, e.Correctos,
        e.ConErrores, e.Incorrectos, e.Estado.ToString(), e.Csv, e.Error, e.EnviadoEn);
}

/// <summary>Situación de un documento del mes en el SII (sin enviar, enviado y su estado, o modificado desde el envío).</summary>
public sealed record SituacionSiiDto(Guid DocumentoId, string Numero, DateOnly FechaExpedicion, string Situacion, string? Estado, string? CodigoError,
    string? DescripcionError, string? Csv, DateTimeOffset? EnviadoEn);

public sealed record ResultadoEnvioSiiDto(IReadOnlyList<EnvioSiiDto> Envios, IReadOnlyList<SituacionSiiDto> Situacion);

// ----------------------------------------------------------------------------- Respuesta de la AEAT
/// <summary>Lectura de la respuesta del SII (RespuestaSuministro.xsd): estado global, CSV y el resultado de cada factura.</summary>
public static class RespuestaSii
{
    public sealed record Linea(string Numero, string Estado, string? Codigo, string? Descripcion);

    public sealed record Resultado(string? EstadoEnvio, string? Csv, IReadOnlyList<Linea> Lineas, string? Fallo);

    /// <summary>Interpreta el cuerpo SOAP (por nombre local, sin depender de prefijos). Un Fault o un XML ilegible es un fallo global.</summary>
    public static Resultado Leer(string? cuerpo)
    {
        if (string.IsNullOrWhiteSpace(cuerpo))
        {
            return new(null, null, [], "Respuesta vacía de la AEAT.");
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(cuerpo);
        }
        catch (System.Xml.XmlException ex)
        {
            return new(null, null, [], "Respuesta no válida de la AEAT: " + ex.Message);
        }

        var todos = doc.Descendants().ToList();
        if (todos.FirstOrDefault(e => e.Name.LocalName == "Fault") is { } fault)
        {
            var texto = fault.Descendants().FirstOrDefault(e => e.Name.LocalName == "faultstring")?.Value ?? fault.Value;
            return new(null, null, [], "La AEAT rechazó el envío: " + texto.Trim());
        }

        string? Hijo(XElement e, string nombre) => e.Elements().FirstOrDefault(x => x.Name.LocalName == nombre)?.Value.Trim();
        var raiz = todos.FirstOrDefault(e => e.Name.LocalName.StartsWith("RespuestaLR", StringComparison.Ordinal)
            || e.Name.LocalName.StartsWith("RespuestaLRBaja", StringComparison.Ordinal));
        if (raiz is null)
        {
            return new(null, null, [], "La respuesta de la AEAT no trae el resultado del envío.");
        }

        var lineas = raiz.Elements().Where(e => e.Name.LocalName == "RespuestaLinea").Select(l =>
        {
            var numero = l.Descendants().FirstOrDefault(x => x.Name.LocalName == "NumSerieFacturaEmisor")?.Value.Trim() ?? string.Empty;
            return new Linea(numero, Hijo(l, "EstadoRegistro") ?? "Incorrecto", Hijo(l, "CodigoErrorRegistro"), Hijo(l, "DescripcionErrorRegistro"));
        }).ToList();
        return new(Hijo(raiz, "EstadoEnvio"), Hijo(raiz, "CSV"), lineas, null);
    }
}

// ----------------------------------------------------------------------------- Certificado
/// <summary>Caso de uso: guardar (cifrado), consultar y quitar el certificado electrónico de la empresa para el SII.</summary>
public sealed class GestionCertificadoSii
{
    private readonly IRepositorioSii _repo;
    private readonly IProtectorSecretos _protector;
    private readonly IReloj _reloj;

    public GestionCertificadoSii(IRepositorioSii repo, IProtectorSecretos protector, IReloj reloj)
    {
        _repo = repo;
        _protector = protector;
        _reloj = reloj;
    }

    public async Task<CertificadoSiiDto?> ObtenerAsync(Guid empresaId, CancellationToken ct = default) =>
        await _repo.CertificadoAsync(empresaId, ct).ConfigureAwait(false) is { } c ? Dto(c) : null;

    public async Task<Resultado<CertificadoSiiDto>> GuardarAsync(Guid empresaId, SubirCertificadoSiiComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        byte[] pfx;
        try
        {
            pfx = Convert.FromBase64String(comando.PfxBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            return Resultado.Fallo<CertificadoSiiDto>(Error.Validacion("sii.certificado_formato", "El certificado debe ir en base64 (fichero .pfx o .p12)."));
        }

        X509Certificate2 certificado;
        try
        {
            certificado = new X509Certificate2(pfx, comando.Clave, X509KeyStorageFlags.EphemeralKeySet);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return Resultado.Fallo<CertificadoSiiDto>(Error.Validacion("sii.certificado_clave", "No se puede abrir el certificado: comprueba el fichero y su contraseña."));
        }

        using (certificado)
        {
            if (!certificado.HasPrivateKey)
            {
                return Resultado.Fallo<CertificadoSiiDto>(Error.Validacion("sii.certificado_sin_clave", "El fichero no incluye la clave privada del certificado."));
            }

            var caduca = new DateTimeOffset(certificado.NotAfter.ToUniversalTime(), TimeSpan.Zero);
            if (caduca < _reloj.AhoraUtc)
            {
                return Resultado.Fallo<CertificadoSiiDto>(Error.Validacion("sii.certificado_caducado", $"El certificado caducó el {caduca:dd/MM/yyyy}."));
            }

            var titular = certificado.GetNameInfo(X509NameType.SimpleName, false);
            var nif = NifDelSujeto(certificado.Subject);
            var pfxCifrado = _protector.Proteger(pfx);
            var claveCifrada = Convert.ToBase64String(_protector.Proteger(System.Text.Encoding.UTF8.GetBytes(comando.Clave ?? string.Empty)));
            var existente = await _repo.CertificadoAsync(empresaId, ct).ConfigureAwait(false);
            if (existente is null)
            {
                existente = new CertificadoSii(empresaId, pfxCifrado, claveCifrada, titular, nif, caduca, comando.Entorno, _reloj.AhoraUtc);
                _repo.Agregar(existente);
            }
            else
            {
                existente.Reemplazar(pfxCifrado, claveCifrada, titular, nif, caduca, comando.Entorno, _reloj.AhoraUtc);
            }

            await _repo.GuardarAsync(ct).ConfigureAwait(false);
            return Resultado.Ok(Dto(existente));
        }
    }

    public async Task<Resultado<CertificadoSiiDto>> CambiarEntornoAsync(Guid empresaId, EntornoSii entorno, CancellationToken ct = default)
    {
        var c = await _repo.CertificadoAsync(empresaId, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<CertificadoSiiDto>(Error.NoEncontrado("sii.sin_certificado", "La empresa no tiene certificado para el SII."));
        }

        c.CambiarEntorno(entorno, _reloj.AhoraUtc);
        await _repo.GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(c));
    }

    public async Task<Resultado<bool>> EliminarAsync(Guid empresaId, CancellationToken ct = default)
    {
        var c = await _repo.CertificadoAsync(empresaId, ct).ConfigureAwait(false);
        if (c is null)
        {
            return Resultado.Fallo<bool>(Error.NoEncontrado("sii.sin_certificado", "La empresa no tiene certificado para el SII."));
        }

        _repo.Eliminar(c);
        await _repo.GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(true);
    }

    /// <summary>Abre el certificado guardado (para el envío).</summary>
    internal X509Certificate2 Abrir(CertificadoSii c)
    {
        var clave = System.Text.Encoding.UTF8.GetString(_protector.Desproteger(Convert.FromBase64String(c.ClaveCifrada)));
        return new X509Certificate2(_protector.Desproteger(c.PfxCifrado), clave, X509KeyStorageFlags.EphemeralKeySet);
    }

    private CertificadoSiiDto Dto(CertificadoSii c) => new(c.Titular, c.Nif, c.CaducaEn, c.Entorno.ToString(), c.CaducaEn < _reloj.AhoraUtc, c.ActualizadoEn);

    /// <summary>NIF del sujeto (SERIALNUMBER, a veces con prefijo IDCES-) o, en un certificado de representante, el de la entidad (OID 2.5.4.97).</summary>
    private static string? NifDelSujeto(string sujeto)
    {
        foreach (var parte in sujeto.Split(',', StringSplitOptions.TrimEntries))
        {
            var kv = parte.Split('=', 2);
            if (kv.Length == 2 && (kv[0] is "SERIALNUMBER" or "OID.2.5.4.97" or "2.5.4.97"))
            {
                var valor = kv[1].Replace("IDCES-", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("VATES-", string.Empty, StringComparison.OrdinalIgnoreCase);
                return valor.Trim().ToUpperInvariant();
            }
        }

        return null;
    }
}

// ----------------------------------------------------------------------------- Envío
/// <summary>
/// Caso de uso: <b>enviar el SII</b> de un libro y un mes. Compara cada documento con lo que consta enviado: los nuevos (o
/// rechazados) van como alta A0, los aceptados que han cambiado como modificación A1, y los anulados que se habían
/// aceptado se dan de baja. Guarda la petición, la respuesta (con el CSV) y el estado de cada factura; lo aceptado sin
/// cambios no se vuelve a enviar.
/// </summary>
public sealed class EnviarSii
{
    private const string NsSoap = "http://schemas.xmlsoap.org/soap/envelope/";

    /// <summary>La AEAT admite hasta 10.000 facturas por envío.</summary>
    public const int MaximoPorEnvio = 10000;

    private readonly GenerarSii _generar;
    private readonly IRepositorioSii _repo;
    private readonly GestionCertificadoSii _certificados;
    private readonly ITransporteSii _transporte;
    private readonly IReloj _reloj;
    private readonly OpcionesSiiAtc _atc;

    public EnviarSii(GenerarSii generar, IRepositorioSii repo, GestionCertificadoSii certificados, ITransporteSii transporte, IReloj reloj,
        OpcionesSiiAtc? atc = null)
    {
        _atc = atc ?? new OpcionesSiiAtc();
        _generar = generar;
        _repo = repo;
        _certificados = certificados;
        _transporte = transporte;
        _reloj = reloj;
    }

    /// <summary>Nombre del libro en el registro de envíos: el de la ATC lleva el prefijo «Igic» (son libros distintos).</summary>
    public static string NombreLibro(TipoLibroSii libro, AdministracionSii administracion = AdministracionSii.Aeat) =>
        (administracion == AdministracionSii.Atc ? "Igic" : "") + (libro == TipoLibroSii.Emitidas ? "Emitidas" : "Recibidas");

    /// <summary>Situación de cada documento del mes respecto al SII, sin enviar nada.</summary>
    public async Task<Resultado<IReadOnlyList<SituacionSiiDto>>> SituacionAsync(Guid empresaId, TipoLibroSii libro, int ejercicio, int periodo,
        AdministracionSii? administracion = null, CancellationToken ct = default)
    {
        var lote = await _generar.PrepararAsync(empresaId, libro, ejercicio, periodo, administracion, ct).ConfigureAwait(false);
        if (lote.EsFallo)
        {
            return Resultado.Fallo<IReadOnlyList<SituacionSiiDto>>(lote.Error);
        }

        var registros = (await _repo.RegistrosAsync(empresaId, NombreLibro(libro, lote.Valor.Administracion), ejercicio, periodo, ct).ConfigureAwait(false))
            .ToDictionary(r => r.DocumentoId);
        return Resultado.Ok<IReadOnlyList<SituacionSiiDto>>(Situacion(lote.Valor, registros));
    }

    public async Task<Resultado<ResultadoEnvioSiiDto>> EjecutarAsync(Guid empresaId, EnviarSiiComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var certificado = await _repo.CertificadoAsync(empresaId, ct).ConfigureAwait(false);
        if (certificado is null)
        {
            return Resultado.Fallo<ResultadoEnvioSiiDto>(Error.Validacion("sii.sin_certificado", "Sube antes el certificado electrónico de la empresa para el SII."));
        }

        if (certificado.CaducaEn < _reloj.AhoraUtc)
        {
            return Resultado.Fallo<ResultadoEnvioSiiDto>(Error.Validacion("sii.certificado_caducado", "El certificado de la empresa ha caducado: sube uno vigente."));
        }

        var preparado = await _generar.PrepararAsync(empresaId, comando.Libro, comando.Ejercicio, comando.Periodo, comando.Administracion, ct).ConfigureAwait(false);
        if (preparado.EsFallo)
        {
            return Resultado.Fallo<ResultadoEnvioSiiDto>(preparado.Error);
        }

        var lote = preparado.Valor;
        var destino = lote.Administracion == AdministracionSii.Atc ? _atc.Url(comando.Libro, certificado.Entorno) : DireccionesSii.Url(comando.Libro, certificado.Entorno);
        if (destino is null)
        {
            return Resultado.Fallo<ResultadoEnvioSiiDto>(Error.Validacion("sii.atc_sin_configurar",
                "Falta configurar el servicio del SII-IGIC de la Agencia Tributaria Canaria (Sii:Atc: EspacioNombres, ServidorPruebas, " +
                "ServidorProduccion, RutaEmitidas y RutaRecibidas, según su documentación técnica). Hasta entonces se puede descargar el XML para revisarlo."));
        }

        var libro = NombreLibro(comando.Libro, lote.Administracion);
        var registros = (await _repo.RegistrosAsync(empresaId, libro, comando.Ejercicio, comando.Periodo, ct).ConfigureAwait(false)).ToDictionary(r => r.DocumentoId);

        // Qué va en cada tipo de comunicación.
        var altas = lote.Documentos.Where(d => !registros.TryGetValue(d.Id, out var r) || !r.Aceptado).ToList();
        var modificaciones = lote.Documentos.Where(d => registros.TryGetValue(d.Id, out var r) && r.Aceptado
            && (r.Huella != d.Huella || r.Estado == EstadoRegistroSii.AceptadoConErrores)).ToList();
        var bajas = lote.Anulados.Where(d => registros.TryGetValue(d.Id, out var r) && r.Aceptado).ToList();
        if (altas.Count + modificaciones.Count + bajas.Count == 0)
        {
            return Resultado.Fallo<ResultadoEnvioSiiDto>(Error.Conflicto("sii.nada_que_enviar", "No hay nada pendiente de enviar en ese mes: todo consta aceptado y sin cambios."));
        }

        using var x509 = _certificados.Abrir(certificado);
        var envios = new List<EnvioSii>();
        foreach (var (tipo, documentos) in new[] { ("A0", altas), ("A1", modificaciones), ("B", bajas) })
        {
            foreach (var bloque in documentos.Chunk(MaximoPorEnvio))
            {
                var ids = bloque.Select(d => d.Id).ToList();
                var cuerpo = tipo == "B" ? lote.Baja(ids) : lote.Alta(ids, tipo);
                envios.Add(await EnviarBloqueAsync(empresaId, comando, destino, libro, tipo, certificado.Entorno, x509, bloque, cuerpo, registros, ct).ConfigureAwait(false));
            }
        }

        await _repo.GuardarAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(new ResultadoEnvioSiiDto(envios.Select(EnvioSiiDto.Desde).ToList(), Situacion(lote, registros)));
    }

    public async Task<IReadOnlyList<EnvioSiiDto>> EnviosAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.EnviosAsync(empresaId, 100, ct).ConfigureAwait(false)).Select(EnvioSiiDto.Desde).ToList();

    public async Task<(string Peticion, string? Respuesta)?> DetalleEnvioAsync(Guid id, CancellationToken ct = default) =>
        await _repo.EnvioAsync(id, ct).ConfigureAwait(false) is { } e ? (e.Peticion, e.Respuesta) : null;

    private async Task<EnvioSii> EnviarBloqueAsync(Guid empresaId, EnviarSiiComando comando, Uri destino, string libro, string tipo, EntornoSii entorno, X509Certificate2 x509,
        IReadOnlyList<DocumentoSii> documentos, string cuerpo, Dictionary<Guid, RegistroSii> registros, CancellationToken ct)
    {
        var sobre = Sobre(cuerpo);
        var envio = new EnvioSii(empresaId, libro, comando.Ejercicio, comando.Periodo, tipo, entorno, documentos.Count, sobre, _reloj.AhoraUtc);
        _repo.Agregar(envio);

        var transporte = await _transporte.EnviarAsync(destino, comando.Libro, entorno, x509, sobre, ct).ConfigureAwait(false);
        var respuesta = transporte.Error is null ? RespuestaSii.Leer(transporte.Cuerpo) : new RespuestaSii.Resultado(null, null, [], transporte.Error);
        if (respuesta.Fallo is not null)
        {
            envio.Responder(EstadoEnvioSii.ErrorComunicacion, null, 0, 0, 0, transporte.Cuerpo, respuesta.Fallo);
            return envio;
        }

        var porNumero = respuesta.Lineas.GroupBy(l => l.Numero, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last(), StringComparer.Ordinal);
        int correctos = 0, conErrores = 0, incorrectos = 0;
        foreach (var d in documentos)
        {
            if (!registros.TryGetValue(d.Id, out var registro))
            {
                registro = new RegistroSii(empresaId, libro, d.Id, d.Numero, d.FechaExpedicion, comando.Ejercicio, comando.Periodo);
                registros[d.Id] = registro;
                _repo.Agregar(registro);
            }

            var linea = porNumero.GetValueOrDefault(d.Numero);
            // Sin línea para la factura, la AEAT la ha tratado según el estado global del envío.
            var estadoTexto = linea?.Estado ?? (respuesta.EstadoEnvio == "Correcto" ? "Correcto" : "Incorrecto");
            var estado = estadoTexto switch
            {
                "Correcto" => tipo == "B" ? EstadoRegistroSii.DadoDeBaja : EstadoRegistroSii.Correcto,
                "AceptadoConErrores" => EstadoRegistroSii.AceptadoConErrores,
                _ => EstadoRegistroSii.Incorrecto,
            };

            // 3000: la factura ya estaba dada de alta (se envió antes por otro medio). Consta aceptada: se modificará con A1.
            if (estado == EstadoRegistroSii.Incorrecto && tipo == "A0" && linea?.Codigo == "3000")
            {
                estado = EstadoRegistroSii.AceptadoConErrores;
            }

            registro.Anotar(envio.Id, estado, linea?.Codigo, linea?.Descripcion, respuesta.Csv, d.Huella, _reloj.AhoraUtc);
            switch (estado)
            {
                case EstadoRegistroSii.Correcto or EstadoRegistroSii.DadoDeBaja:
                    correctos++;
                    break;
                case EstadoRegistroSii.AceptadoConErrores:
                    conErrores++;
                    break;
                default:
                    incorrectos++;
                    break;
            }
        }

        var global = respuesta.EstadoEnvio switch
        {
            "Correcto" => EstadoEnvioSii.Correcto,
            "ParcialmenteCorrecto" => EstadoEnvioSii.ParcialmenteCorrecto,
            "Incorrecto" => EstadoEnvioSii.Incorrecto,
            _ => incorrectos == 0 ? EstadoEnvioSii.Correcto : correctos + conErrores > 0 ? EstadoEnvioSii.ParcialmenteCorrecto : EstadoEnvioSii.Incorrecto,
        };
        envio.Responder(global, respuesta.Csv, correctos, conErrores, incorrectos, transporte.Cuerpo, null);
        return envio;
    }

    /// <summary>Mete el suministro (sin la declaración XML) en el cuerpo de un sobre SOAP 1.1.</summary>
    internal static string Sobre(string suministro)
    {
        var cuerpo = XDocument.Parse(suministro).Root!;
        var sobre = new XDocument(new XElement(XName.Get("Envelope", NsSoap),
            new XAttribute(XNamespace.Xmlns + "soapenv", NsSoap),
            new XElement(XName.Get("Header", NsSoap)),
            new XElement(XName.Get("Body", NsSoap), cuerpo)));
        return sobre.ToString(SaveOptions.DisableFormatting);
    }

    private static List<SituacionSiiDto> Situacion(LoteSii lote, Dictionary<Guid, RegistroSii> registros)
    {
        SituacionSiiDto Una(DocumentoSii d, bool anulado)
        {
            var r = registros.GetValueOrDefault(d.Id);
            var situacion = r is null ? (anulado ? "Anulada sin enviar" : "Pendiente")
                : anulado ? (r.Estado == EstadoRegistroSii.DadoDeBaja ? "Dada de baja" : r.Aceptado ? "Anulada: pendiente de baja" : "Anulada sin enviar")
                : !r.Aceptado ? "Rechazada: corregir y reenviar"
                : r.Huella != d.Huella ? "Modificada: pendiente de enviar"
                : r.Estado == EstadoRegistroSii.AceptadoConErrores ? "Aceptada con errores" : "Enviada";
            return new SituacionSiiDto(d.Id, d.Numero, d.FechaExpedicion, situacion, r?.Estado.ToString(), r?.CodigoError, r?.DescripcionError, r?.Csv, r?.EnviadoEn);
        }

        return [.. lote.Documentos.Select(d => Una(d, false)), .. lote.Anulados.Where(d => registros.ContainsKey(d.Id)).Select(d => Una(d, true))];
    }
}

/// <summary>Direcciones del servicio web del SII (certificado de persona o de representante; los de sello usan www10).</summary>
public static class DireccionesSii
{
    public static Uri Url(TipoLibroSii libro, EntornoSii entorno)
    {
        var servidor = entorno == EntornoSii.Produccion ? "https://www1.agenciatributaria.gob.es" : "https://prewww1.aeat.es";
        var ruta = libro == TipoLibroSii.Emitidas ? "/wlpl/SSII-FACT/ws/fe/SiiFactFEV1SOAP" : "/wlpl/SSII-FACT/ws/fr/SiiFactFRV1SOAP";
        return new Uri(servidor + ruta, UriKind.Absolute);
    }
}
