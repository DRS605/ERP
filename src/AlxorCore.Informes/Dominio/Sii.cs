using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Informes.Dominio;

/// <summary>Entorno del servicio web del SII de la AEAT.</summary>
public enum EntornoSii
{
    /// <summary>Entorno de pruebas (prewww1.aeat.es): no tiene efectos tributarios.</summary>
    Pruebas,

    /// <summary>Entorno real (www1.agenciatributaria.gob.es).</summary>
    Produccion,
}

/// <summary>Estado de un registro (una factura) en el SII según la respuesta de la AEAT.</summary>
public enum EstadoRegistroSii
{
    /// <summary>Aceptado sin errores.</summary>
    Correcto,

    /// <summary>Aceptado, pero con errores que hay que corregir con una modificación (A1).</summary>
    AceptadoConErrores,

    /// <summary>Rechazado: no consta en el SII; hay que corregirlo y volver a enviarlo como alta.</summary>
    Incorrecto,

    /// <summary>Dado de baja en el SII (la factura se anuló después de enviarla).</summary>
    DadoDeBaja,
}

/// <summary>Resultado global de un envío.</summary>
public enum EstadoEnvioSii
{
    Correcto,
    ParcialmenteCorrecto,
    Incorrecto,

    /// <summary>No hubo respuesta válida de la AEAT (conexión, certificado, error SOAP): nada consta como enviado.</summary>
    ErrorComunicacion,
}

/// <summary>
/// Certificado electrónico de la empresa para el SII (y, más adelante, VeriFactu). El PKCS#12 y su contraseña se guardan
/// cifrados con la protección de datos de la aplicación; nunca salen por la API.
/// </summary>
public sealed class CertificadoSii : RaizAgregadoEmpresa<Guid>
{
    private CertificadoSii(Guid id)
        : base(id, Guid.Empty)
    {
        PfxCifrado = [];
        ClaveCifrada = string.Empty;
        Titular = string.Empty;
    }

    public CertificadoSii(Guid empresaId, byte[] pfxCifrado, string claveCifrada, string titular, string? nif, DateTimeOffset caducaEn, EntornoSii entorno, DateTimeOffset ahora)
        : base(Guid.NewGuid(), empresaId)
    {
        PfxCifrado = pfxCifrado;
        ClaveCifrada = claveCifrada;
        Titular = titular;
        Nif = nif;
        CaducaEn = caducaEn;
        Entorno = entorno;
        ActualizadoEn = ahora;
    }

    public byte[] PfxCifrado { get; private set; }

    public string ClaveCifrada { get; private set; }

    /// <summary>Sujeto del certificado (para mostrarlo).</summary>
    public string Titular { get; private set; }

    public string? Nif { get; private set; }

    public DateTimeOffset CaducaEn { get; private set; }

    public EntornoSii Entorno { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public void Reemplazar(byte[] pfxCifrado, string claveCifrada, string titular, string? nif, DateTimeOffset caducaEn, EntornoSii entorno, DateTimeOffset ahora)
    {
        PfxCifrado = pfxCifrado;
        ClaveCifrada = claveCifrada;
        Titular = titular;
        Nif = nif;
        CaducaEn = caducaEn;
        Entorno = entorno;
        ActualizadoEn = ahora;
    }

    public void CambiarEntorno(EntornoSii entorno, DateTimeOffset ahora)
    {
        Entorno = entorno;
        ActualizadoEn = ahora;
    }
}

/// <summary>Un envío al SII: la petición, la respuesta y su resultado. Es el histórico de lo remitido a la AEAT.</summary>
public sealed class EnvioSii : RaizAgregadoEmpresa<Guid>
{
    private EnvioSii(Guid id)
        : base(id, Guid.Empty)
    {
        Libro = string.Empty;
        TipoComunicacion = string.Empty;
        Peticion = string.Empty;
    }

    public EnvioSii(Guid empresaId, string libro, int ejercicio, int periodo, string tipoComunicacion, EntornoSii entorno, int registros, string peticion, DateTimeOffset ahora)
        : base(Guid.NewGuid(), empresaId)
    {
        Libro = libro;
        Ejercicio = ejercicio;
        Periodo = periodo;
        TipoComunicacion = tipoComunicacion;
        Entorno = entorno;
        Registros = registros;
        Peticion = peticion;
        EnviadoEn = ahora;
        Estado = EstadoEnvioSii.ErrorComunicacion;
    }

    /// <summary>«Emitidas» o «Recibidas».</summary>
    public string Libro { get; private set; }

    public int Ejercicio { get; private set; }

    public int Periodo { get; private set; }

    /// <summary>A0 (alta), A1 (modificación) o B (baja).</summary>
    public string TipoComunicacion { get; private set; }

    public EntornoSii Entorno { get; private set; }

    public int Registros { get; private set; }

    public int Correctos { get; private set; }

    public int ConErrores { get; private set; }

    public int Incorrectos { get; private set; }

    public EstadoEnvioSii Estado { get; private set; }

    /// <summary>Código seguro de verificación que devuelve la AEAT (justificante del envío).</summary>
    public string? Csv { get; private set; }

    public string? Error { get; private set; }

    public string Peticion { get; private set; }

    public string? Respuesta { get; private set; }

    public DateTimeOffset EnviadoEn { get; private set; }

    public void Responder(EstadoEnvioSii estado, string? csv, int correctos, int conErrores, int incorrectos, string? respuesta, string? error)
    {
        Estado = estado;
        Csv = csv;
        Correctos = correctos;
        ConErrores = conErrores;
        Incorrectos = incorrectos;
        Respuesta = respuesta;
        Error = error is null ? null : error[..Math.Min(error.Length, 2000)];
    }
}

/// <summary>
/// Situación de una factura (emitida o recibida) en el SII: su último estado, el error de la AEAT si lo hubo y la huella
/// de lo enviado, para saber si ha cambiado y hay que modificarla (A1). Una factura aceptada queda bloqueada para el alta.
/// </summary>
public sealed class RegistroSii : RaizAgregadoEmpresa<Guid>
{
    private RegistroSii(Guid id)
        : base(id, Guid.Empty)
    {
        Libro = string.Empty;
        Numero = string.Empty;
        Huella = string.Empty;
    }

    public RegistroSii(Guid empresaId, string libro, Guid documentoId, string numero, DateOnly fechaExpedicion, int ejercicio, int periodo)
        : base(Guid.NewGuid(), empresaId)
    {
        Libro = libro;
        DocumentoId = documentoId;
        Numero = numero;
        FechaExpedicion = fechaExpedicion;
        Ejercicio = ejercicio;
        Periodo = periodo;
        Huella = string.Empty;
        Estado = EstadoRegistroSii.Incorrecto;
    }

    public string Libro { get; private set; }

    /// <summary>Factura (emitidas) o gasto (recibidas).</summary>
    public Guid DocumentoId { get; private set; }

    public string Numero { get; private set; }

    public DateOnly FechaExpedicion { get; private set; }

    public int Ejercicio { get; private set; }

    public int Periodo { get; private set; }

    public EstadoRegistroSii Estado { get; private set; }

    /// <summary>Si alguna vez fue aceptado (Correcto o con errores): a partir de ahí, los cambios van como modificación (A1).</summary>
    public bool Aceptado { get; private set; }

    public string? CodigoError { get; private set; }

    public string? DescripcionError { get; private set; }

    public string? Csv { get; private set; }

    /// <summary>Huella de los datos enviados en el último envío aceptado.</summary>
    public string Huella { get; private set; }

    public Guid? UltimoEnvioId { get; private set; }

    public DateTimeOffset? EnviadoEn { get; private set; }

    public void Anotar(Guid envioId, EstadoRegistroSii estado, string? codigoError, string? descripcion, string? csv, string huella, DateTimeOffset ahora)
    {
        Estado = estado;
        CodigoError = codigoError;
        DescripcionError = descripcion is null ? null : descripcion[..Math.Min(descripcion.Length, 500)];
        UltimoEnvioId = envioId;
        EnviadoEn = ahora;
        if (estado is EstadoRegistroSii.Correcto or EstadoRegistroSii.AceptadoConErrores or EstadoRegistroSii.DadoDeBaja)
        {
            Aceptado = estado != EstadoRegistroSii.DadoDeBaja;
            Huella = huella;
            Csv = csv;
        }
    }
}
