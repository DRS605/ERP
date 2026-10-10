using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// <b>Instalación vendida</b> (registro del panel de la plataforma Map Technology). No es un dato de
/// tenant: es global, sólo lo gestiona el administrador de la plataforma. Guarda a qué cliente se ha
/// vendido, su plan y cuota, el estado comercial y una <b>clave de licencia</b> que la instalación
/// del cliente usará para validarse contra el servidor.
/// </summary>
public sealed class Instalacion : RaizAgregado<Guid>
{
    public const int LongitudMaximaNombre = 200;
    public const int LongitudMaximaTexto = 200;
    public const int LongitudMaximaNotas = 2000;

    private Instalacion(Guid id)
        : base(id)
    {
        Nombre = null!;
        Clave = null!;
    }

    private Instalacion(Guid id, string nombre, string? contacto, string? email, string? telefono, string? poblacion,
        PlanSuscripcion plan, EstadoInstalacion estado, decimal cuotaMensual, DateOnly fechaAlta, DateOnly? proximoCobro,
        string clave, string? notas, DateTimeOffset ahora)
        : base(id)
    {
        Nombre = nombre;
        Contacto = contacto;
        Email = email;
        Telefono = telefono;
        Poblacion = poblacion;
        Plan = plan;
        Estado = estado;
        CuotaMensual = cuotaMensual;
        FechaAlta = fechaAlta;
        ProximoCobro = proximoCobro;
        Clave = clave;
        Notas = notas;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Nombre del comercio/cliente de la instalación.</summary>
    public string Nombre { get; private set; }

    public string? Contacto { get; private set; }

    public string? Email { get; private set; }

    public string? Telefono { get; private set; }

    public string? Poblacion { get; private set; }

    /// <summary>Plan contratado.</summary>
    public PlanSuscripcion Plan { get; private set; }

    /// <summary>Estado comercial/operativo (determina si la licencia es válida).</summary>
    public EstadoInstalacion Estado { get; private set; }

    /// <summary>Cuota mensual acordada (€).</summary>
    public decimal CuotaMensual { get; private set; }

    public DateOnly FechaAlta { get; private set; }

    /// <summary>Fecha del próximo cobro (para el control de impagos). Opcional.</summary>
    public DateOnly? ProximoCobro { get; private set; }

    /// <summary>Clave de licencia única; la instalación del cliente la envía para validarse.</summary>
    public string Clave { get; private set; }

    public string? Notas { get; private set; }

    /// <summary>Última vez que la instalación se validó contra el servidor (si aplica).</summary>
    public DateTimeOffset? UltimaConexion { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    /// <summary>¿La licencia es válida ahora mismo? Sólo lo es en estado <see cref="EstadoInstalacion.Activa"/>.</summary>
    public bool LicenciaValida => Estado == EstadoInstalacion.Activa;

    public static Resultado<Instalacion> Crear(string? nombre, string? contacto, string? email, string? telefono,
        string? poblacion, PlanSuscripcion plan, EstadoInstalacion estado, decimal cuotaMensual, DateOnly? proximoCobro,
        string? notas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(ref nombre, cuotaMensual);
        if (error is not null)
        {
            return Resultado.Fallo<Instalacion>(error);
        }

        var hoy = DateOnly.FromDateTime(reloj.AhoraUtc.UtcDateTime);
        var instalacion = new Instalacion(Guid.NewGuid(), nombre!, Recortar(contacto, LongitudMaximaTexto),
            Recortar(email, LongitudMaximaTexto), Recortar(telefono, 40), Recortar(poblacion, LongitudMaximaTexto),
            plan, estado, cuotaMensual, hoy, proximoCobro, GenerarClave(), Recortar(notas, LongitudMaximaNotas), reloj.AhoraUtc);
        return Resultado.Ok(instalacion);
    }

    public Resultado Actualizar(string? nombre, string? contacto, string? email, string? telefono, string? poblacion,
        PlanSuscripcion plan, EstadoInstalacion estado, decimal cuotaMensual, DateOnly? proximoCobro, string? notas, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(ref nombre, cuotaMensual);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!;
        Contacto = Recortar(contacto, LongitudMaximaTexto);
        Email = Recortar(email, LongitudMaximaTexto);
        Telefono = Recortar(telefono, 40);
        Poblacion = Recortar(poblacion, LongitudMaximaTexto);
        Plan = plan;
        Estado = estado;
        CuotaMensual = cuotaMensual;
        ProximoCobro = proximoCobro;
        Notas = Recortar(notas, LongitudMaximaNotas);
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Cambia sólo el estado (vender, suspender por impago, dar de baja…).</summary>
    public void CambiarEstado(EstadoInstalacion estado, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Estado = estado;
        ActualizadoEn = reloj.AhoraUtc;
    }

    /// <summary>Registra que la instalación del cliente se ha validado ahora.</summary>
    public void RegistrarConexion(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        UltimaConexion = reloj.AhoraUtc;
    }

    private static Error? Validar(ref string? nombre, decimal cuotaMensual)
    {
        nombre = Recortar(nombre, LongitudMaximaNombre);
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("instalacion.nombre_vacio", "El nombre de la instalación es obligatorio.");
        }

        if (cuotaMensual < 0m)
        {
            return Error.Validacion("instalacion.cuota_invalida", "La cuota no puede ser negativa.");
        }

        return null;
    }

    /// <summary>Genera una clave de licencia legible, tipo <c>CE-XXXX-XXXX-XXXX</c>.</summary>
    private static string GenerarClave()
    {
        var hex = Guid.NewGuid().ToString("N").ToUpperInvariant();
        return $"CE-{hex[..4]}-{hex[4..8]}-{hex[8..12]}";
    }

    private static string? Recortar(string? valor, int max)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        var limpio = valor.Trim();
        return limpio.Length > max ? limpio[..max] : limpio;
    }
}
