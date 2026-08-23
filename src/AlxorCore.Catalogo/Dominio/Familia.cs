using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Catalogo.Dominio;

/// <summary>
/// Familia (categoría) de artículos, organizada en <b>árbol</b>: cada familia puede colgar de una
/// familia padre (<see cref="PadreId"/>), lo que permite crear <b>subfamilias</b> con la profundidad
/// que haga falta (p. ej. «Alimentación &gt; Bebidas &gt; Refrescos»). Las familias raíz no tienen
/// padre. Sustituye al antiguo texto libre del artículo por un catálogo normalizado y reutilizable
/// (evita erratas como «Servicios» vs «servicios») y sigue sirviendo para elegir la cuenta contable
/// mediante las reglas de contabilización.
/// </summary>
public sealed class Familia : RaizAgregadoGrupo<Guid>
{
    public const int LongitudMaximaNombre = 80;
    public const int LongitudMaximaCodigo = 20;

    // Constructor sin parámetros para EF Core.
    private Familia(Guid id)
        : base(id, Guid.Empty)
    {
        Nombre = null!;
    }

    private Familia(Guid id, Guid grupoId, string nombre, Guid? padreId, string? codigo, DateTimeOffset ahora)
        : base(id, grupoId)
    {
        Nombre = nombre;
        PadreId = padreId;
        Codigo = codigo;
        Activo = true;
        CreadoEn = ahora;
        ActualizadoEn = ahora;
    }

    /// <summary>Nombre visible de la familia.</summary>
    public string Nombre { get; private set; }

    /// <summary>Código corto opcional de la familia (para clasificaciones internas). Null = sin código.</summary>
    public string? Codigo { get; private set; }

    /// <summary>Familia padre de la que cuelga esta. Null = familia raíz (primer nivel).</summary>
    public Guid? PadreId { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public DateTimeOffset ActualizadoEn { get; private set; }

    public static Resultado<Familia> Crear(Guid grupoId, string? nombre, Guid? padreId, string? codigo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(ref nombre, ref codigo);
        if (error is not null)
        {
            return Resultado.Fallo<Familia>(error);
        }

        return Resultado.Ok(new Familia(Guid.NewGuid(), grupoId, nombre!, padreId, codigo, reloj.AhoraUtc));
    }

    /// <summary>Renombra la familia y/o cambia su código.</summary>
    public Resultado Actualizar(string? nombre, string? codigo, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        var error = Validar(ref nombre, ref codigo);
        if (error is not null)
        {
            return Resultado.Fallo(error);
        }

        Nombre = nombre!;
        Codigo = codigo;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    /// <summary>
    /// Mueve la familia bajo otra (o a la raíz si <paramref name="nuevoPadreId"/> es null). La
    /// comprobación de ciclos (no colgar una familia de sí misma o de un descendiente) la hace el
    /// caso de uso, que conoce el árbol completo.
    /// </summary>
    public Resultado Reubicar(Guid? nuevoPadreId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);

        if (nuevoPadreId == Id)
        {
            return Resultado.Fallo(Error.Validacion("familia.padre_si_misma", "Una familia no puede ser su propio padre."));
        }

        PadreId = nuevoPadreId;
        ActualizadoEn = reloj.AhoraUtc;
        return Resultado.Ok();
    }

    public void Activar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Activo = true;
        ActualizadoEn = reloj.AhoraUtc;
    }

    public void Desactivar(IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        Activo = false;
        ActualizadoEn = reloj.AhoraUtc;
    }

    private static Error? Validar(ref string? nombre, ref string? codigo)
    {
        nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre.Trim();
        if (nombre is null)
        {
            return Error.Validacion("familia.nombre_vacio", "La familia necesita un nombre.");
        }

        if (nombre.Length > LongitudMaximaNombre)
        {
            return Error.Validacion("familia.nombre_largo", $"El nombre no puede superar {LongitudMaximaNombre} caracteres.");
        }

        codigo = string.IsNullOrWhiteSpace(codigo) ? null : codigo.Trim();
        if (codigo is not null && codigo.Length > LongitudMaximaCodigo)
        {
            return Error.Validacion("familia.codigo_largo", $"El código no puede superar {LongitudMaximaCodigo} caracteres.");
        }

        return null;
    }
}
