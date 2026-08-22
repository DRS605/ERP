using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Personal.Dominio;

/// <summary>
/// Persona (empleado/colaborador) de la empresa con una tarifa por hora, para imputar mano de obra a
/// proyectos y partes de producción. Multiempresa (RLS).
/// </summary>
public sealed class Persona : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaNombre = 150;

    private Persona(Guid id) : base(id, Guid.Empty) { Nombre = null!; }

    private Persona(Guid id, Guid empresaId, string nombre, string? puesto, decimal tarifaHora, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Nombre = nombre;
        Puesto = puesto;
        TarifaHora = tarifaHora;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    public string Nombre { get; private set; }

    /// <summary>Puesto o rol (opcional): «Oficial 1ª», «Ingeniero»…</summary>
    public string? Puesto { get; private set; }

    /// <summary>Coste por hora de la persona (€/h).</summary>
    public decimal TarifaHora { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Persona> Crear(Guid empresaId, string? nombre, string? puesto, decimal tarifaHora, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, tarifaHora);
        if (error is not null)
        {
            return Resultado.Fallo<Persona>(error);
        }

        return Resultado.Ok(new Persona(Guid.NewGuid(), empresaId, nombre!.Trim(), Normalizar(puesto), Redondeo.Dos(tarifaHora), reloj.AhoraUtc));
    }

    public Resultado Actualizar(string? nombre, string? puesto, decimal tarifaHora, bool activo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(nombre, tarifaHora);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!.Trim();
        Puesto = Normalizar(puesto);
        TarifaHora = Redondeo.Dos(tarifaHora);
        Activo = activo;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    private static Error? Validar(string? nombre, decimal tarifaHora)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Error.Validacion("persona.nombre_vacio", "El nombre es obligatorio.");
        }

        if (nombre.Trim().Length > LongitudMaximaNombre)
        {
            return Error.Validacion("persona.nombre_largo", "El nombre es demasiado largo.");
        }

        if (tarifaHora < 0m)
        {
            return Error.Validacion("persona.tarifa_negativa", "La tarifa por hora no puede ser negativa.");
        }

        return null;
    }

    private static string? Normalizar(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
