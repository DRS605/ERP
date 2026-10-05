using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;

namespace AlxorCore.Organizacion.Dominio;

/// <summary>Caja (punto de venta) de un centro. Sus tickets pueden llevar una serie propia (asignación de ámbito Caja).</summary>
public sealed class CajaCentro
{
    private CajaCentro()
    {
        Codigo = null!;
        Nombre = null!;
    }

    internal CajaCentro(string codigo, string nombre)
    {
        Id = Guid.NewGuid();
        Codigo = codigo;
        Nombre = nombre;
        Activa = true;
    }

    public Guid Id { get; private set; }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public bool Activa { get; private set; }

    internal void Cambiar(string nombre, bool activa)
    {
        Nombre = nombre;
        Activa = activa;
    }
}

/// <summary>
/// <b>Centro</b> de trabajo dentro de la empresa (tienda, almacén regional, delegación, planta…): con sus series
/// (asignaciones de ámbito Centro), su almacén habitual y sus cajas. Los documentos llevan el centro, y a un usuario
/// se le pueden limitar los centros con que trabaja.
/// </summary>
public sealed class Centro : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 10;
    public const int LongitudNombre = 120;

    private readonly List<CajaCentro> _cajas = [];

    private Centro(Guid id)
        : base(id, Guid.Empty)
    {
        Codigo = null!;
        Nombre = null!;
    }

    private Centro(Guid id, Guid empresaId, string codigo, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        Codigo = codigo;
        Nombre = codigo;
        Activo = true;
        CreadoEn = ahora;
    }

    public string Codigo { get; private set; }

    public string Nombre { get; private set; }

    public string? Direccion { get; private set; }

    /// <summary>Almacén habitual del centro: de él salen sus ventas y a él entran sus compras, si no se indica otro.</summary>
    public Guid? AlmacenId { get; private set; }

    public bool Activo { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public IReadOnlyList<CajaCentro> Cajas => _cajas;

    public static Resultado<Centro> Crear(Guid empresaId, string? codigo, string? nombre, string? direccion, Guid? almacenId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        var c = Codigo_(codigo);
        if (c.EsFallo)
        {
            return Resultado.Fallo<Centro>(c.Error);
        }

        var centro = new Centro(Guid.NewGuid(), empresaId, c.Valor, reloj.AhoraUtc);
        var r = centro.Cambiar(nombre, direccion, almacenId, true);
        return r.EsFallo ? Resultado.Fallo<Centro>(r.Error) : Resultado.Ok(centro);
    }

    public Resultado Cambiar(string? nombre, string? direccion, Guid? almacenId, bool activo)
    {
        var n = string.IsNullOrWhiteSpace(nombre) ? Codigo : nombre.Trim();
        if (n.Length > LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("centro.nombre", $"El nombre admite hasta {LongitudNombre} caracteres."));
        }

        var d = string.IsNullOrWhiteSpace(direccion) ? null : direccion.Trim();
        if (d?.Length > 300)
        {
            return Resultado.Fallo(Error.Validacion("centro.direccion", "La dirección admite hasta 300 caracteres."));
        }

        Nombre = n;
        Direccion = d;
        AlmacenId = almacenId == Guid.Empty ? null : almacenId;
        Activo = activo;
        return Resultado.Ok();
    }

    public Resultado<CajaCentro> AgregarCaja(string? codigo, string? nombre)
    {
        var c = Codigo_(codigo);
        if (c.EsFallo)
        {
            return Resultado.Fallo<CajaCentro>(c.Error);
        }

        if (_cajas.Any(x => x.Codigo == c.Valor))
        {
            return Resultado.Fallo<CajaCentro>(Error.Conflicto("centro.caja_duplicada", $"Ya hay una caja {c.Valor} en el centro."));
        }

        var n = string.IsNullOrWhiteSpace(nombre) ? $"Caja {c.Valor}" : nombre.Trim();
        if (n.Length > LongitudNombre)
        {
            return Resultado.Fallo<CajaCentro>(Error.Validacion("centro.caja_nombre", $"El nombre admite hasta {LongitudNombre} caracteres."));
        }

        var caja = new CajaCentro(c.Valor, n);
        _cajas.Add(caja);
        return Resultado.Ok(caja);
    }

    public Resultado CambiarCaja(Guid cajaId, string? nombre, bool activa)
    {
        var caja = _cajas.FirstOrDefault(x => x.Id == cajaId);
        if (caja is null)
        {
            return Resultado.Fallo(Error.NoEncontrado("centro.caja_no_encontrada", "La caja no existe en el centro."));
        }

        var n = string.IsNullOrWhiteSpace(nombre) ? caja.Nombre : nombre.Trim();
        if (n.Length > LongitudNombre)
        {
            return Resultado.Fallo(Error.Validacion("centro.caja_nombre", $"El nombre admite hasta {LongitudNombre} caracteres."));
        }

        caja.Cambiar(n, activa);
        return Resultado.Ok();
    }

    private static Resultado<string> Codigo_(string? codigo)
    {
        var c = (codigo ?? string.Empty).Trim().ToUpperInvariant();
        return c.Length == 0 || c.Length > LongitudCodigo || !c.All(ch => char.IsAsciiLetterOrDigit(ch) || ch is '-' or '_')
            ? Resultado.Fallo<string>(Error.Validacion("centro.codigo", $"El código lleva letras, números, guion o guion bajo (hasta {LongitudCodigo})."))
            : Resultado.Ok(c);
    }
}

/// <summary>
/// Acceso de un usuario a un centro. Un usuario sin accesos trabaja con todos los centros (y con los documentos sin
/// centro); con alguno, solo con los suyos: los ve y crea documentos en ellos.
/// </summary>
public sealed class AccesoCentro : RaizAgregadoEmpresa<Guid>
{
    private AccesoCentro(Guid id)
        : base(id, Guid.Empty)
    {
    }

    private AccesoCentro(Guid id, Guid empresaId, Guid usuarioId, Guid centroId, DateTimeOffset ahora)
        : base(id, empresaId)
    {
        UsuarioId = usuarioId;
        CentroId = centroId;
        CreadoEn = ahora;
    }

    public Guid UsuarioId { get; private set; }

    public Guid CentroId { get; private set; }

    public DateTimeOffset CreadoEn { get; private set; }

    public static AccesoCentro Crear(Guid empresaId, Guid usuarioId, Guid centroId, IReloj reloj)
    {
        ArgumentNullException.ThrowIfNull(reloj);
        return new AccesoCentro(Guid.NewGuid(), empresaId, usuarioId, centroId, reloj.AhoraUtc);
    }
}
