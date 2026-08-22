using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Modalidad de pago de una empresa (contado, transferencia a 30 días, domiciliación…). Define si el
/// documento genera un <b>vencimiento</b> y si al emitirlo/registrarlo se registra el <b>pago
/// automáticamente</b> (para las que ya están pagadas al contado). Si el pago ya se registró a mano,
/// se elige una modalidad sin registro automático para no duplicarlo.
/// </summary>
public sealed class FormaPago : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 80;
    public const int DiasMaximos = 3650;

    private FormaPago(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private FormaPago(Guid id, Guid empresaId, string nombre, bool generaVencimiento, int diasVencimiento, bool registrarPagoAutomatico)
        : base(id, empresaId)
    {
        Nombre = nombre;
        GeneraVencimiento = generaVencimiento;
        DiasVencimiento = diasVencimiento;
        RegistrarPagoAutomatico = registrarPagoAutomatico;
        Activo = true;
    }

    public string Nombre { get; private set; }

    /// <summary>Si es <c>false</c>, el documento no genera un vencimiento abierto (se considera al contado).</summary>
    public bool GeneraVencimiento { get; private set; }

    /// <summary>Días desde la fecha del documento hasta el vencimiento (solo si <see cref="GeneraVencimiento"/>).</summary>
    public int DiasVencimiento { get; private set; }

    /// <summary>
    /// Si es <c>true</c>, al emitir/registrar el documento se registra el cobro/pago por el total en la
    /// fecha del documento (queda saldado). Se deja en <c>false</c> cuando el pago se registra aparte.
    /// </summary>
    public bool RegistrarPagoAutomatico { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<FormaPago> Crear(Guid empresaId, string? nombre, bool generaVencimiento, int diasVencimiento, bool registrarPagoAutomatico)
    {
        var error = Validar(ref nombre, ref diasVencimiento, generaVencimiento);
        if (error is not null)
        {
            return Resultado.Fallo<FormaPago>(error);
        }

        return Resultado.Ok(new FormaPago(Guid.NewGuid(), empresaId, nombre!, generaVencimiento, diasVencimiento, registrarPagoAutomatico));
    }

    public Resultado Actualizar(string? nombre, bool generaVencimiento, int diasVencimiento, bool registrarPagoAutomatico)
    {
        var error = Validar(ref nombre, ref diasVencimiento, generaVencimiento);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!;
        GeneraVencimiento = generaVencimiento;
        DiasVencimiento = diasVencimiento;
        RegistrarPagoAutomatico = registrarPagoAutomatico;
        return Resultado.Ok();
    }

    public void Desactivar() => Activo = false;

    private static Error? Validar(ref string? nombre, ref int diasVencimiento, bool generaVencimiento)
    {
        nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();
        if (nombre is null)
        {
            return Error.Validacion("forma_pago.nombre_vacio", "El nombre de la forma de pago es obligatorio.");
        }

        if (nombre.Length > LongitudMaximaNombre)
        {
            return Error.Validacion("forma_pago.nombre_largo", "El nombre de la forma de pago es demasiado largo.");
        }

        if (!generaVencimiento)
        {
            diasVencimiento = 0;
        }

        if (diasVencimiento is < 0 or > DiasMaximos)
        {
            return Error.Validacion("forma_pago.dias_invalidos", "Los días de vencimiento no son válidos.");
        }

        return null;
    }
}
