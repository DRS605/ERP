using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Inventario.Dominio;

/// <summary>Un almacén de la empresa (multi-almacén).</summary>
public sealed class Almacen : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudMaximaCodigo = 20;
    public const int LongitudMaximaNombre = 120;

    private Almacen(Guid id) : base(id, Guid.Empty) { Codigo = null!; Nombre = null!; }

    private Almacen(Guid id, Guid empresaId, string codigo, string nombre)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = nombre;
        Activo = true;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo { get; private set; }

    public static Resultado<Almacen> Crear(Guid empresaId, string? codigo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Resultado.Fallo<Almacen>(Error.Validacion("almacen.codigo_vacio", "El código del almacén es obligatorio."));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return Resultado.Fallo<Almacen>(Error.Validacion("almacen.nombre_vacio", "El nombre del almacén es obligatorio."));
        }

        return Resultado.Ok(new Almacen(Guid.NewGuid(), empresaId, codigo.Trim(), nombre.Trim()));
    }

    public void Desactivar() => Activo = false;
}

/// <summary>Una ubicación dentro de un almacén (p. ej. «PASILLO-A / ESTANTE-3»).</summary>
public sealed class Ubicacion : RaizAgregadoEmpresa<Guid>
{
    private Ubicacion(Guid id) : base(id, Guid.Empty) { Codigo = null!; Nombre = null!; }

    private Ubicacion(Guid id, Guid empresaId, Guid almacenId, string codigo, string nombre)
        : base(id, empresaId)
    {
        AlmacenId = almacenId;
        Codigo = codigo;
        Nombre = nombre;
    }

    public Guid AlmacenId { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public static Resultado<Ubicacion> Crear(Guid empresaId, Guid almacenId, string? codigo, string? nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return Resultado.Fallo<Ubicacion>(Error.Validacion("ubicacion.codigo_vacio", "El código de la ubicación es obligatorio."));
        }

        return Resultado.Ok(new Ubicacion(Guid.NewGuid(), empresaId, almacenId,
            codigo.Trim(), string.IsNullOrWhiteSpace(nombre) ? codigo.Trim() : nombre.Trim()));
    }
}
