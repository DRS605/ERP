using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Inventario.Dominio;

/// <summary>
/// Datos de un lote de un artículo: su caducidad y, si se conoce, su fabricación. Las existencias y los movimientos
/// llevan el código del lote; esto le añade las fechas (por ejemplo, la caducidad de un fitosanitario).
/// </summary>
public sealed class LoteArticulo : RaizAgregadoEmpresa<Guid>
{
    public const int LongitudCodigo = 60;

    private LoteArticulo()
        : base(Guid.Empty, Guid.Empty)
    {
        Codigo = null!;
    }

    private LoteArticulo(Guid empresaId, Guid productoId, string codigo)
        : base(Guid.NewGuid(), empresaId)
    {
        ProductoId = productoId;
        Codigo = codigo;
    }

    public Guid ProductoId { get; private set; }

    public string Codigo { get; private set; }

    public DateOnly? FechaCaducidad { get; private set; }

    public DateOnly? FechaFabricacion { get; private set; }

    public string? Observaciones { get; private set; }

    public static Resultado<LoteArticulo> Crear(Guid empresaId, Guid productoId, string? codigo, DateOnly? caducidad, DateOnly? fabricacion, string? observaciones)
    {
        if (string.IsNullOrWhiteSpace(codigo) || codigo.Trim().Length > LongitudCodigo)
        {
            return Resultado.Fallo<LoteArticulo>(Error.Validacion("lote.codigo", $"Indica el código del lote (hasta {LongitudCodigo} caracteres)."));
        }

        var l = new LoteArticulo(empresaId, productoId, codigo.Trim());
        var r = l.Fijar(caducidad, fabricacion, observaciones);
        return r.EsFallo ? Resultado.Fallo<LoteArticulo>(r.Error) : Resultado.Ok(l);
    }

    public Resultado Fijar(DateOnly? caducidad, DateOnly? fabricacion, string? observaciones)
    {
        if (caducidad is { } c && fabricacion is { } f && c < f)
        {
            return Resultado.Fallo(Error.Validacion("lote.fechas", "La caducidad no puede ser anterior a la fabricación."));
        }

        FechaCaducidad = caducidad;
        FechaFabricacion = fabricacion;
        Observaciones = string.IsNullOrWhiteSpace(observaciones) ? null : observaciones.Trim()[..Math.Min(observaciones.Trim().Length, 300)];
        return Resultado.Ok();
    }

    public bool CaducadoEl(DateOnly fecha) => FechaCaducidad is { } c && fecha > c;
}
