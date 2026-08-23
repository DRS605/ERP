using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Terceros.Dominio;

/// <summary>Se ha creado un cliente.</summary>
public sealed record ClienteCreado(Guid ClienteId, Guid EmpresaId, DateTimeOffset OcurridoEn) : IEventoDominio;

/// <summary>
/// Cliente de una empresa. Guarda los datos fiscales necesarios para facturarle. El identificador
/// fiscal (<see cref="NifFiscal"/>) es opcional y se acepta como texto: un cliente puede ser
/// extranjero y no tener NIF español.
/// </summary>
public sealed class Cliente : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 200;
    public const int LongitudMaximaTipo = 80;
    public const int LongitudMaximaDir3 = 20;
    public const decimal IrpfMaximo = 60m;

    private Cliente(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
        Direccion = Direccion.Vacia;
    }

    private Cliente(Guid id, Guid empresaId, string nombre, string? nifFiscal, string? email, Direccion direccion, decimal irpf, bool recargoEquivalencia, string? iban, string? mandatoReferencia, DateOnly? mandatoFecha, string? nifIva, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Nombre = nombre;
        NifFiscal = nifFiscal;
        Email = email;
        Direccion = direccion;
        PorcentajeIrpfDefecto = irpf;
        RecargoEquivalencia = recargoEquivalencia;
        Iban = NormalizarIban(iban);
        MandatoReferencia = Normalizar(mandatoReferencia);
        MandatoFecha = mandatoFecha;
        NifIva = NormalizarIva(nifIva);
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public string Nombre { get; private set; }

    public string? NifFiscal { get; private set; }

    public string? Email { get; private set; }

    public Direccion Direccion { get; private set; }

    /// <summary>Retención de IRPF por defecto (0–60 %). Se prerrellena al facturar.</summary>
    public decimal PorcentajeIrpfDefecto { get; private set; }

    /// <summary>El cliente está en régimen de recargo de equivalencia (minorista): al facturarle se aplica por defecto.</summary>
    public bool RecargoEquivalencia { get; private set; }

    /// <summary>IBAN del cliente para domiciliar sus recibos (adeudos SEPA). Opcional.</summary>
    public string? Iban { get; private set; }

    /// <summary>Referencia única del mandato de domiciliación firmado por el cliente. Opcional.</summary>
    public string? MandatoReferencia { get; private set; }

    /// <summary>Fecha de firma del mandato de domiciliación. Opcional.</summary>
    public DateOnly? MandatoFecha { get; private set; }

    /// <summary>
    /// NIF-IVA intracomunitario (VIES) del cliente: código de país (2 letras) + número, p. ej.
    /// «DE123456789». Su presencia marca al cliente como operador intracomunitario (modelo 349).
    /// </summary>
    public string? NifIva { get; private set; }

    /// <summary>¿Tiene los datos necesarios para domiciliar (IBAN, mandato y fecha)?</summary>
    public bool DomiciliacionCompleta => !string.IsNullOrWhiteSpace(Iban) && !string.IsNullOrWhiteSpace(MandatoReferencia) && MandatoFecha is not null;

    /// <summary>
    /// Tipo o categoría del cliente (p. ej. «Nacional», «Intracomunitario», «Minorista»). Sirve para
    /// elegir la cuenta contable de ingreso mediante reglas de contabilización. Null = sin tipo.
    /// </summary>
    public string? Tipo { get; private set; }

    /// <summary>Forma de pago habitual del cliente (referencia opcional al catálogo de Organización).</summary>
    public Guid? FormaPagoDefectoId { get; private set; }

    /// <summary>Límite de riesgo (crédito) concedido al cliente. Null = sin límite. Editable.</summary>
    public decimal? LimiteRiesgo { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    /// <summary>
    /// El cliente es una <b>Administración Pública</b>: al facturarle, la factura electrónica
    /// (Facturae) debe remitirse por FACe con los centros administrativos DIR3.
    /// </summary>
    public bool EsAdministracionPublica { get; private set; }

    /// <summary>Código DIR3 de la <b>Oficina Contable</b> (rol fiscal 01) del destinatario AAPP.</summary>
    public string? Dir3OficinaContable { get; private set; }

    /// <summary>Código DIR3 del <b>Órgano Gestor</b> (rol fiscal 02) del destinatario AAPP.</summary>
    public string? Dir3OrganoGestor { get; private set; }

    /// <summary>Código DIR3 de la <b>Unidad Tramitadora</b> (rol fiscal 03) del destinatario AAPP.</summary>
    public string? Dir3UnidadTramitadora { get; private set; }

    /// <summary>¿Tiene los tres centros DIR3 necesarios para enviar la Facturae por FACe?</summary>
    public bool CentrosDir3Completos =>
        EsAdministracionPublica
        && !string.IsNullOrWhiteSpace(Dir3OficinaContable)
        && !string.IsNullOrWhiteSpace(Dir3OrganoGestor)
        && !string.IsNullOrWhiteSpace(Dir3UnidadTramitadora);

    /// <summary>Establece el tipo/categoría del cliente (se recorta; vacío = sin tipo).</summary>
    public void EstablecerTipo(string? tipo)
    {
        var limpia = string.IsNullOrWhiteSpace(tipo) ? null : tipo.Trim();
        if (limpia is not null && limpia.Length > LongitudMaximaTipo)
        {
            limpia = limpia[..LongitudMaximaTipo];
        }

        Tipo = limpia;
    }

    /// <summary>Fija la forma de pago habitual del cliente (null = sin defecto).</summary>
    public void EstablecerFormaPagoDefecto(Guid? formaPagoId) => FormaPagoDefectoId = formaPagoId;

    /// <summary>Fija el límite de riesgo del cliente (null o negativo = sin límite).</summary>
    public void EstablecerLimiteRiesgo(decimal? limite) => LimiteRiesgo = limite is > 0m ? limite : null;

    /// <summary>
    /// Marca al cliente como Administración Pública y fija sus centros administrativos <b>DIR3</b>
    /// (Oficina Contable, Órgano Gestor y Unidad Tramitadora), necesarios para remitir la factura
    /// electrónica (Facturae) a través de FACe. Si no es AAPP, se limpian los códigos.
    /// </summary>
    public void EstablecerCentrosDir3(bool esAdministracionPublica, string? oficinaContable, string? organoGestor, string? unidadTramitadora)
    {
        EsAdministracionPublica = esAdministracionPublica;
        if (!esAdministracionPublica)
        {
            Dir3OficinaContable = null;
            Dir3OrganoGestor = null;
            Dir3UnidadTramitadora = null;
            return;
        }

        Dir3OficinaContable = NormalizarDir3(oficinaContable);
        Dir3OrganoGestor = NormalizarDir3(organoGestor);
        Dir3UnidadTramitadora = NormalizarDir3(unidadTramitadora);
    }

    public static Resultado<Cliente> Crear(
        Guid empresaId,
        string? nombre,
        string? nifFiscal,
        string? email,
        Direccion direccion,
        decimal porcentajeIrpfDefecto,
        IReloj reloj,
        bool recargoEquivalencia = false,
        string? iban = null,
        string? mandatoReferencia = null,
        DateOnly? mandatoFecha = null,
        string? nifIva = null)
    {
        ArgumentNullException.ThrowIfNull(direccion);
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, porcentajeIrpfDefecto);
        if (error is not null)
        {
            return Resultado.Fallo<Cliente>(error);
        }

        var cliente = new Cliente(
            Guid.NewGuid(), empresaId, nombre!.Trim(), Normalizar(nifFiscal), Normalizar(email), direccion, porcentajeIrpfDefecto, recargoEquivalencia, iban, mandatoReferencia, mandatoFecha, nifIva, reloj.AhoraUtc);
        cliente.RegistrarEvento(new ClienteCreado(cliente.Id, empresaId, reloj.AhoraUtc));
        return Resultado.Ok(cliente);
    }

    public Resultado Actualizar(string? nombre, string? nifFiscal, string? email, Direccion direccion, decimal porcentajeIrpfDefecto, IReloj reloj, bool recargoEquivalencia = false, string? iban = null, string? mandatoReferencia = null, DateOnly? mandatoFecha = null, string? nifIva = null)
    {
        ArgumentNullException.ThrowIfNull(direccion);
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, porcentajeIrpfDefecto);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!.Trim();
        NifFiscal = Normalizar(nifFiscal);
        Email = Normalizar(email);
        Direccion = direccion;
        PorcentajeIrpfDefecto = porcentajeIrpfDefecto;
        RecargoEquivalencia = recargoEquivalencia;
        Iban = NormalizarIban(iban);
        MandatoReferencia = Normalizar(mandatoReferencia);
        MandatoFecha = mandatoFecha;
        NifIva = NormalizarIva(nifIva);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public void Desactivar(IReloj reloj)
    {
        Activo = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    private static Error? Validar(string? nombre, decimal irpf)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("cliente.nombre_vacio", "El nombre del cliente es obligatorio.");
        }

        if (nombre.Trim().Length > LongitudMaximaNombre)
        {
            return Error.Validacion("cliente.nombre_largo", "El nombre del cliente es demasiado largo.");
        }

        if (irpf is < 0 or > IrpfMaximo)
        {
            return Error.Validacion("cliente.irpf_invalido", "El porcentaje de IRPF no es válido.");
        }

        return null;
    }

    private static string? Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static string? NormalizarIban(string? iban) =>
        string.IsNullOrWhiteSpace(iban) ? null : iban.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string? NormalizarIva(string? nifIva) =>
        string.IsNullOrWhiteSpace(nifIva) ? null : nifIva.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string? NormalizarDir3(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpia = valor.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        return limpia.Length > LongitudMaximaDir3 ? limpia[..LongitudMaximaDir3] : limpia;
    }
}
