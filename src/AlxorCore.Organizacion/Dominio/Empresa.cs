using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Dominio.Eventos;

// El método de valoración (MetodoValoracion) vive en el núcleo por ser transversal.

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Empresa: la entidad tenant de ALXOR Core. No hereda de la base multiempresa porque ella misma
/// define el tenant; el acceso a una empresa se controla por las membresías del usuario.
/// </summary>
public sealed class Empresa : RaizAgregado<Guid>
{
    public const int LongitudMaximaRazonSocial = 200;
    public const int LongitudMaximaTexto = 500;
    public const int TamanoMaximoLogoBytes = 512 * 1024; // 512 KB

    private Empresa(Guid id)
        : base(id)
    {
        Nif = null!;
        RazonSocial = null!;
        Direccion = null!;
    }

    private Empresa(Guid id, Nif nif, string razonSocial, Direccion direccion, RegimenIva regimenIva, DateTimeOffset ahora)
        : base(id)
    {
        Nif = nif;
        RazonSocial = razonSocial;
        Direccion = direccion;
        RegimenIva = regimenIva;
        Moneda = "EUR";
        Pais = "ES";
        MetodoValoracion = MetodoValoracion.Estandar;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public Nif Nif { get; private set; }

    public string RazonSocial { get; private set; }

    public Direccion Direccion { get; private set; }

    public RegimenIva RegimenIva { get; private set; }

    public string Moneda { get; private set; } = "EUR";

    public string Pais { get; private set; } = "ES";

    /// <summary>IBAN de la empresa donde ingresar los adeudos domiciliados (remesas SEPA). Opcional.</summary>
    public string? Iban { get; private set; }

    /// <summary>Identificador del acreedor SEPA (lo asigna el banco). Necesario para emitir remesas. Opcional.</summary>
    public string? IdentificadorAcreedor { get; private set; }

    /// <summary>Método de valoración de existencias/consumos elegido en la implantación. Por defecto, estándar.</summary>
    public MetodoValoracion MetodoValoracion { get; private set; } = MetodoValoracion.Estandar;

    /// <summary>Cómo actúa ante el exceso de límite de riesgo de un tercero (avisar o bloquear).</summary>
    public ControlRiesgo ControlRiesgo { get; private set; } = ControlRiesgo.Aviso;

    // --- Plantilla de documentos (lo que aparece en facturas, tickets y presupuestos) ---

    /// <summary>Teléfono de contacto que se imprime en los documentos. Opcional.</summary>
    public string? Telefono { get; private set; }

    /// <summary>Página web que se imprime en los documentos. Opcional.</summary>
    public string? Web { get; private set; }

    /// <summary>Correo de contacto que se imprime en los documentos. Opcional.</summary>
    public string? EmailContacto { get; private set; }

    /// <summary>Color corporativo (hex «#RRGGBB») para títulos y líneas de los documentos. Opcional.</summary>
    public string? ColorPrincipal { get; private set; }

    /// <summary>Texto de pie de página (condiciones, forma de pago, agradecimiento…). Opcional.</summary>
    public string? TextoPie { get; private set; }

    /// <summary>Logotipo (PNG) que se imprime en la cabecera de los documentos. Opcional.</summary>
    public byte[]? LogoPng { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Empresa> Crear(Nif nif, string? razonSocial, Direccion direccion, RegimenIva regimenIva, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(nif);
        ArgumentNullException.ThrowIfNull(direccion);
        ArgumentNullException.ThrowIfNull(reloj);

        var nombre = (razonSocial ?? string.Empty).Trim();
        if (nombre.Length == 0)
        {
            return Resultado.Fallo<Empresa>(Error.Validacion("empresa.razon_social_vacia", "La razón social es obligatoria."));
        }

        if (nombre.Length > LongitudMaximaRazonSocial)
        {
            return Resultado.Fallo<Empresa>(Error.Validacion("empresa.razon_social_larga", "La razón social es demasiado larga."));
        }

        var empresa = new Empresa(Guid.NewGuid(), nif, nombre, direccion, regimenIva, reloj.AhoraUtc);
        empresa.RegistrarEvento(new EmpresaCreada(empresa.Id, nif.Valor, reloj.AhoraUtc));
        return Resultado.Ok(empresa);
    }

    public void ActualizarDatos(string? razonSocial, Direccion direccion, RegimenIva regimenIva, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(direccion);
        ArgumentNullException.ThrowIfNull(reloj);

        var nombre = (razonSocial ?? string.Empty).Trim();
        if (nombre.Length > 0)
        {
            RazonSocial = nombre;
        }

        Direccion = direccion;
        RegimenIva = regimenIva;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>
    /// Configura la <b>plantilla de documentos</b>: datos de contacto (teléfono, web, correo), color
    /// corporativo, texto de pie y logotipo, que aparecen en facturas, tickets y presupuestos. Los
    /// valores en blanco se guardan como null (no se imprimen). Un color mal formado o un logo que no
    /// sea PNG o que supere el tamaño máximo se rechazan.
    /// </summary>
    public Resultado EstablecerPlantillaDocumento(string? telefono, string? web, string? emailContacto, string? colorPrincipal, string? textoPie, byte[]? logoPng, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var color = string.IsNullOrWhiteSpace(colorPrincipal) ? null : colorPrincipal.Trim();
        if (color is not null && !EsColorHexValido(color))
        {
            return Resultado.Fallo(Error.Validacion("empresa.color_invalido", "El color debe ser un hex «#RRGGBB» (p. ej. #0EA5B7)."));
        }

        var pie = Recortar(textoPie, LongitudMaximaTexto);
        if (logoPng is not null && logoPng.Length > 0)
        {
            if (logoPng.Length > TamanoMaximoLogoBytes)
            {
                return Resultado.Fallo(Error.Validacion("empresa.logo_grande", "El logotipo no puede superar los 512 KB."));
            }

            if (!EsPng(logoPng))
            {
                return Resultado.Fallo(Error.Validacion("empresa.logo_formato", "El logotipo debe ser una imagen PNG."));
            }
        }

        Telefono = Recortar(telefono, 40);
        Web = Recortar(web, 120);
        EmailContacto = Recortar(emailContacto, 254);
        ColorPrincipal = color;
        TextoPie = pie;
        if (logoPng is not null)
        {
            LogoPng = logoPng.Length == 0 ? null : logoPng; // array vacío = quitar logo
        }

        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private static string? Recortar(string? valor, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var v = valor.Trim();
        return v.Length > max ? v[..max] : v;
    }

    private static bool EsColorHexValido(string color) =>
        color.Length == 7 && color[0] == '#' && color[1..].All(Uri.IsHexDigit);

    private static bool EsPng(byte[] datos) =>
        datos.Length >= 8 && datos[0] == 0x89 && datos[1] == 0x50 && datos[2] == 0x4E && datos[3] == 0x47
        && datos[4] == 0x0D && datos[5] == 0x0A && datos[6] == 0x1A && datos[7] == 0x0A;

    /// <summary>Fija los datos de cobro por domiciliación (IBAN de ingreso e identificador del acreedor SEPA).</summary>
    public void EstablecerDatosCobro(string? iban, string? identificadorAcreedor, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        Iban = string.IsNullOrWhiteSpace(iban) ? null : iban.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        IdentificadorAcreedor = string.IsNullOrWhiteSpace(identificadorAcreedor) ? null : identificadorAcreedor.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Fija el método de valoración de la empresa (parámetro de implantación).</summary>
    public void EstablecerMetodoValoracion(MetodoValoracion metodo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        MetodoValoracion = metodo;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Fija el control de riesgo de la empresa (avisar o bloquear al superar el límite).</summary>
    public void EstablecerControlRiesgo(ControlRiesgo control, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        ControlRiesgo = control;
        ActualizadoEn = reloj.AhoraUtc;
    }
}
