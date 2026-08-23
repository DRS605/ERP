using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Área o pantalla a efectos de visibilidad: dónde aplica el permiso de ver una actividad de negocio.
/// Un usuario puede tener actividades distintas permitidas según el área (p. ej. en Ventas ve unas,
/// en Compras otras).
/// </summary>
public enum AreaVisibilidad
{
    /// <summary>Ventas (clientes, presupuestos, facturas…).</summary>
    Ventas = 1,

    /// <summary>Compras (proveedores, gastos, pedidos de compra…).</summary>
    Compras = 2,

    /// <summary>Artículos / catálogo.</summary>
    Articulos = 3,

    /// <summary>Uso general / resto de pantallas.</summary>
    General = 4,
}

/// <summary>
/// <b>Actividad de negocio</b>: dimensión transversal (línea o división de negocio) del grupo con la
/// que se clasifican los datos maestros (clientes, proveedores, artículos…). Sirve para segmentar la
/// información y para controlar qué ve cada usuario. Pertenece al grupo (compartida entre empresas).
/// </summary>
public sealed class ActividadNegocio : RaizAgregadoGrupo<Guid>
{
    public const int LongitudMaximaNombre = 120;

    private ActividadNegocio(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private ActividadNegocio(Guid id, Guid grupoId, string nombre, DateTimeOffset ahora)
        : base(id, grupoId)
    {
        Nombre = nombre;
        Activa = true;
        CreadoEn = ahora;
    }

    public string Nombre { get; private set; }

    public bool Activa { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<ActividadNegocio> Crear(Guid grupoId, string? nombre, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var limpio = (nombre ?? string.Empty).Trim();
        if (limpio.Length == 0)
        {
            return Resultado.Fallo<ActividadNegocio>(Error.Validacion("actividad.nombre_vacio", "El nombre de la actividad es obligatorio."));
        }

        if (limpio.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo<ActividadNegocio>(Error.Validacion("actividad.nombre_largo", "El nombre de la actividad es demasiado largo."));
        }

        var actividad = new ActividadNegocio(Guid.NewGuid(), grupoId, limpio, reloj.AhoraUtc)
        {
            ActualizadoEn = reloj.AhoraUtc,
        };
        return Resultado.Ok(actividad);
    }

    /// <summary>Renombra la actividad.</summary>
    public Resultado Renombrar(string? nombre, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var limpio = (nombre ?? string.Empty).Trim();
        if (limpio.Length == 0 || limpio.Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo(Error.Validacion("actividad.nombre_invalido", "El nombre de la actividad no es válido."));
        }

        Nombre = limpio;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>Activa o desactiva la actividad (una inactiva no se ofrece para clasificar nuevos maestros).</summary>
    public void FijarActiva(bool activa, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Activa = activa;
        ActualizadoEn = reloj.AhoraUtc;
    }
}

/// <summary>
/// Regla de <b>visibilidad</b>: concede a un usuario ver una actividad de negocio concreta en un área
/// (pantalla). Si un usuario no tiene ninguna regla para un área, ve <b>todas</b> las actividades en
/// ella (abierto por defecto); en cuanto tiene alguna, solo ve las concedidas (más los maestros sin
/// actividad, que son visibles para todos).
/// </summary>
public sealed class VisibilidadActividad : RaizAgregadoGrupo<Guid>
{
    private VisibilidadActividad(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private VisibilidadActividad(Guid id, Guid grupoId, Guid usuarioId, AreaVisibilidad area, Guid actividadNegocioId, DateTimeOffset ahora)
        : base(id, grupoId)
    {
        UsuarioId = usuarioId;
        Area = area;
        ActividadNegocioId = actividadNegocioId;
        CreadoEn = ahora;
    }

    public Guid UsuarioId { get; private set; }

    public AreaVisibilidad Area { get; private set; }

    public Guid ActividadNegocioId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static VisibilidadActividad Crear(Guid grupoId, Guid usuarioId, AreaVisibilidad area, Guid actividadNegocioId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new VisibilidadActividad(Guid.NewGuid(), grupoId, usuarioId, area, actividadNegocioId, reloj.AhoraUtc);
    }
}
