using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Nucleo.Autorizacion;

/// <summary>
/// Rol de negocio dentro de una empresa: un conjunto de <see cref="Permisos"/>. Hay tres roles fijos y cada empresa
/// puede crear los suyos (por puesto de trabajo, desde <see cref="CatalogoPermisos.Plantillas"/>); la asignación de un
/// rol a un usuario dentro de una empresa (la membresía) pertenece al módulo Organización.
/// </summary>
public sealed class Rol
{
    /// <summary>Acceso total: el dueño de la empresa.</summary>
    public static readonly Rol Propietario = new(
        "propietario",
        "Propietario",
        Permisos.Todos);

    /// <summary>Operativa diaria, sin gestión de usuarios ni ajustes sensibles.</summary>
    public static readonly Rol Usuario = new(
        "usuario",
        "Usuario",
        new HashSet<string>(StringComparer.Ordinal)
        {
            Permisos.FacturaLeer, Permisos.FacturaCrear, Permisos.FacturaEmitir,
            Permisos.GastoLeer, Permisos.GastoGestionar,
            Permisos.RecepcionLeer, Permisos.RecepcionGestionar, Permisos.RecepcionContabilizar,
            Permisos.ContabilidadLeer, Permisos.ContabilidadGestionar,
            Permisos.CompraLeer, Permisos.CompraGestionar,
            Permisos.InventarioLeer, Permisos.InventarioGestionar,
            Permisos.ProduccionLeer, Permisos.ProduccionGestionar,
            Permisos.PersonalLeer, Permisos.PersonalGestionar,
            Permisos.ProyectoLeer, Permisos.ProyectoGestionar,
            Permisos.AgroLeer, Permisos.AgroGestionar, Permisos.AgroLiquidar,
            Permisos.AgroRecepcionar, Permisos.AgroConfeccionar, Permisos.AgroExpedir, Permisos.AgroCalidad, Permisos.AgroCampo,
            Permisos.CobroRegistrar, Permisos.PagoRegistrar,
            Permisos.ClienteGestionar, Permisos.ProductoGestionar,
            Permisos.InformeLeer, Permisos.DatosExportar,
        });

    /// <summary>Solo consulta y exportación (por ejemplo, la gestoría).</summary>
    public static readonly Rol SoloLectura = new(
        "solo_lectura",
        "Solo lectura",
        new HashSet<string>(StringComparer.Ordinal)
        {
            Permisos.FacturaLeer, Permisos.GastoLeer, Permisos.RecepcionLeer, Permisos.CompraLeer, Permisos.ContabilidadLeer, Permisos.InventarioLeer, Permisos.ProduccionLeer, Permisos.PersonalLeer, Permisos.ProyectoLeer, Permisos.AgroLeer, Permisos.InformeLeer, Permisos.DatosExportar,
        });

    private static readonly Dictionary<string, Rol> PorCodigo =
        new[] { Propietario, Usuario, SoloLectura }.ToDictionary(r => r.Codigo, StringComparer.Ordinal);

    private Rol(string codigo, string nombre, IReadOnlySet<string> permisos)
    {
        Codigo = codigo;
        Nombre = nombre;
        PermisosConcedidos = permisos;
    }

    /// <summary>Prefijo del código de los roles propios de una empresa (seguido de su identificador).</summary>
    public const string PrefijoPropio = "rol_";

    /// <summary>¿Es el código de un rol propio de la empresa (no de uno fijo)?</summary>
    public static bool EsPropio(string? codigo) => codigo is not null && codigo.StartsWith(PrefijoPropio, StringComparison.Ordinal);

    /// <summary>Un rol propio de la empresa, con los permisos que se le dieron (solo los del catálogo).</summary>
    public static Rol Propio(string codigo, string nombre, IEnumerable<string> permisos)
    {
        ArgumentNullException.ThrowIfNull(permisos);
        return new Rol(codigo, nombre, permisos.Where(Permisos.Todos.Contains).ToHashSet(StringComparer.Ordinal));
    }

    /// <summary>Código estable del rol (persistible, apto para el token).</summary>
    public string Codigo { get; }

    /// <summary>Nombre legible en español.</summary>
    public string Nombre { get; }

    /// <summary>Permisos que otorga el rol.</summary>
    public IReadOnlySet<string> PermisosConcedidos { get; }

    /// <summary>Todos los roles disponibles.</summary>
    public static IReadOnlyCollection<Rol> Todos => PorCodigo.Values;

    /// <summary>Indica si el rol concede el permiso indicado.</summary>
    public bool Concede(string permiso) => PermisosConcedidos.Contains(permiso);

    /// <summary>Resuelve un rol por su código.</summary>
    public static Resultado<Rol> PorCodigoRol(string? codigo)
    {
        if (!string.IsNullOrWhiteSpace(codigo) && PorCodigo.TryGetValue(codigo, out var rol))
        {
            return Resultado.Ok(rol);
        }

        return Resultado.Fallo<Rol>(Error.Validacion("rol.desconocido", $"El rol «{codigo}» no existe."));
    }
}
