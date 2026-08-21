using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Inventario.Dominio;

/// <summary>
/// Regla de ubicación por defecto de un artículo. Puede ser <b>solo por almacén</b>
/// (<see cref="ProveedorId"/> nulo) o <b>por proveedor y almacén</b> (<see cref="ProveedorId"/>
/// informado). Al dar entrada a mercancía se resuelve primero la regla específica del proveedor
/// y, si no existe, la general del almacén.
/// </summary>
public sealed class UbicacionDefecto : RaizAgregadoEmpresa<Guid>
{
    private UbicacionDefecto() : base(Guid.Empty, Guid.Empty) { }

    public UbicacionDefecto(Guid empresaId, Guid productoId, Guid almacenId, Guid? proveedorId, Guid ubicacionId)
        : base(Guid.NewGuid(), empresaId)
    {
        ProductoId = productoId;
        AlmacenId = almacenId;
        ProveedorId = proveedorId;
        UbicacionId = ubicacionId;
    }

    public Guid ProductoId { get; private set; }

    public Guid AlmacenId { get; private set; }

    /// <summary>Proveedor al que aplica la regla (nulo = regla general del almacén).</summary>
    public Guid? ProveedorId { get; private set; }

    public Guid UbicacionId { get; private set; }

    public void CambiarUbicacion(Guid ubicacionId) => UbicacionId = ubicacionId;
}
