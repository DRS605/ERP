using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Grupo (holding): agrupa varias empresas que <b>comparten los datos maestros</b> (clientes,
/// proveedores, artículos…). Es el tenant de los maestros compartidos, por encima de la empresa.
/// Cada empresa pertenece a un grupo; varias empresas del mismo grupo ven los mismos maestros.
/// </summary>
public sealed class Grupo : RaizAgregado<Guid>
{
    public const int LongitudMaximaNombre = 200;

    private Grupo(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    private Grupo(Guid id, string nombre, DateTimeOffset ahora)
        : base(id)
    {
        Nombre = nombre;
        CreadoEn = ahora;
    }

    public string Nombre { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static Resultado<Grupo> Crear(string? nombre, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var limpio = (nombre ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            return Resultado.Fallo<Grupo>(Error.Validacion("grupo.nombre_vacio", "El nombre del grupo es obligatorio."));
        }

        if (limpio.Length > LongitudMaximaNombre)
        {
            limpio = limpio[..LongitudMaximaNombre];
        }

        return Resultado.Ok(new Grupo(Guid.NewGuid(), limpio, reloj.AhoraUtc));
    }
}
