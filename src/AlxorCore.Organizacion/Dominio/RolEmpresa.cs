using AlxorCore.Nucleo.Autorizacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>
/// Rol propio de una empresa: un nombre (Báscula, Jefe de almacén…) y los permisos que da. Se asigna a los miembros
/// igual que los roles fijos; su código es <see cref="Rol.PrefijoPropio"/> + su identificador. No se borra si algún
/// miembro activo lo tiene: se le cambia antes el rol.
/// </summary>
public sealed class RolEmpresa : RaizAgregado<Guid>
{
    public const int LongitudMaximaNombre = 60;

    private List<string> _permisos = [];

    private RolEmpresa(Guid id)
        : base(id)
    {
        Nombre = null!;
    }

    public Guid EmpresaId { get; private set; }

    public string Nombre { get; private set; }

    public string? Descripcion { get; private set; }

    public IReadOnlyList<string> Permisos => _permisos;

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public string Codigo => Rol.PrefijoPropio + Id.ToString("N");

    public Rol ComoRol() => Rol.Propio(Codigo, Nombre, _permisos);

    public static Resultado<RolEmpresa> Crear(Guid empresaId, string? nombre, string? descripcion, IEnumerable<string>? permisos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var rol = new RolEmpresa(Guid.NewGuid()) { EmpresaId = empresaId, CreadoEn = reloj.AhoraUtc };
        var r = rol.Cambiar(nombre, descripcion, permisos, reloj);
        return r.EsFallo ? Resultado.Fallo<RolEmpresa>(r.Error) : Resultado.Ok(rol);
    }

    public Resultado Cambiar(string? nombre, string? descripcion, IEnumerable<string>? permisos, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > LongitudMaximaNombre)
        {
            return Resultado.Fallo(Error.Validacion("rol.nombre", $"El rol necesita un nombre de hasta {LongitudMaximaNombre} caracteres."));
        }

        var lista = (permisos ?? []).Select(p => p?.Trim() ?? "").Distinct(StringComparer.Ordinal).ToList();
        var desconocidos = lista.Where(p => !AlxorCore.Nucleo.Autorizacion.Permisos.Todos.Contains(p)).ToList();
        if (desconocidos.Count > 0)
        {
            return Resultado.Fallo(Error.Validacion("rol.permiso_desconocido", $"Permisos que no existen: {string.Join(", ", desconocidos)}."));
        }

        if (lista.Count == 0)
        {
            return Resultado.Fallo(Error.Validacion("rol.sin_permisos", "Marca al menos un permiso."));
        }

        Nombre = nombre.Trim();
        Descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim().Length > 300 ? descripcion.Trim()[..300] : descripcion.Trim();
        _permisos = lista.OrderBy(p => p, StringComparer.Ordinal).ToList();
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }
}
