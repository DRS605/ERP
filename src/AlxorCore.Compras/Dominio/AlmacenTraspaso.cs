using AlxorCore.Nucleo.Dominio;

namespace AlxorCore.Compras.Dominio;

/// <summary>
/// Almacén en que entra la mercancía traspasada desde otra empresa del grupo. Una fila por empresa de origen, o una
/// general (<see cref="EmpresaOrigenId"/> nulo) para las demás. Sin almacén (<see cref="AlmacenId"/> nulo), el
/// traspaso registra la recepción sin entrada en inventario. Sin fila, entra en el almacén activo de código más bajo.
/// </summary>
public sealed class AlmacenTraspaso : RaizAgregadoEmpresa<Guid>
{
    private AlmacenTraspaso(Guid id) : base(id, Guid.Empty) { }

    private AlmacenTraspaso(Guid id, Guid empresaId, Guid? empresaOrigenId, Guid? almacenId) : base(id, empresaId)
    {
        EmpresaOrigenId = empresaOrigenId;
        AlmacenId = almacenId;
    }

    public Guid? EmpresaOrigenId { get; private set; }

    public Guid? AlmacenId { get; private set; }

    public static AlmacenTraspaso Crear(Guid empresaId, Guid? empresaOrigenId, Guid? almacenId) => new(Guid.NewGuid(), empresaId, empresaOrigenId, almacenId);

    public void CambiarAlmacen(Guid? almacenId) => AlmacenId = almacenId;
}
