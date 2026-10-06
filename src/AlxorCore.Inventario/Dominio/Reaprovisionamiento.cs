using AlxorCore.Nucleo.Dominio;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Inventario.Dominio;

/// <summary>
/// Regla de reaprovisionamiento de un artículo (en un almacén o en todos): cuando lo disponible baja del mínimo, se
/// propone comprar hasta el máximo, redondeando al múltiplo de compra (en unidades base).
/// </summary>
public sealed class ReglaReaprovisionamiento : RaizAgregadoEmpresa<Guid>
{
    private ReglaReaprovisionamiento(Guid id) : base(id, Guid.Empty) { }

    private ReglaReaprovisionamiento(Guid id, Guid empresaId) : base(id, empresaId) { }

    public Guid ProductoId { get; private set; }

    /// <summary>Almacén (null: el total de todos los almacenes).</summary>
    public Guid? AlmacenId { get; private set; }

    public decimal Minimo { get; private set; }

    public decimal Maximo { get; private set; }

    /// <summary>La cantidad propuesta se redondea hacia arriba a este múltiplo (0: sin redondeo).</summary>
    public decimal Multiplo { get; private set; }

    /// <summary>Proveedor al que se compra (null: el habitual del artículo).</summary>
    public Guid? ProveedorId { get; private set; }

    public bool Activa { get; private set; }

    public static Resultado<ReglaReaprovisionamiento> Crear(Guid empresaId, Guid productoId, Guid? almacenId, decimal minimo, decimal maximo, decimal multiplo, Guid? proveedorId)
    {
        var r = new ReglaReaprovisionamiento(Guid.NewGuid(), empresaId) { ProductoId = productoId, AlmacenId = almacenId, Activa = true };
        var c = r.Cambiar(minimo, maximo, multiplo, proveedorId, true);
        return c.EsFallo ? Resultado.Fallo<ReglaReaprovisionamiento>(c.Error) : Resultado.Ok(r);
    }

    public Resultado Cambiar(decimal minimo, decimal maximo, decimal multiplo, Guid? proveedorId, bool activa)
    {
        if (minimo < 0m || multiplo < 0m)
        {
            return Resultado.Fallo(Error.Validacion("reaprovisionamiento.valores", "El mínimo y el múltiplo no pueden ser negativos."));
        }

        if (maximo < minimo || maximo <= 0m)
        {
            return Resultado.Fallo(Error.Validacion("reaprovisionamiento.maximo", "El máximo (hasta dónde se repone) tiene que ser mayor que cero y no menor que el mínimo."));
        }

        Minimo = minimo;
        Maximo = maximo;
        Multiplo = multiplo;
        ProveedorId = proveedorId;
        Activa = activa;
        return Resultado.Ok();
    }

    /// <summary>Cantidad a comprar (unidades base) con lo disponible: 0 si no baja del mínimo.</summary>
    public decimal APedir(decimal disponible)
    {
        if (!Activa || disponible >= Minimo)
        {
            return 0m;
        }

        var falta = Maximo - disponible;
        return Multiplo > 0m ? Math.Ceiling(falta / Multiplo) * Multiplo : Math.Round(falta, 3, MidpointRounding.AwayFromZero);
    }
}
