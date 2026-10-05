using AlxorCore.Bodega.Aplicacion;
using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador de <see cref="IExistenciasBodega"/> y de <see cref="AlxorCore.Vivero.Aplicacion.IExistenciasVivero"/>: las botellas embotelladas y las
/// plantas que pasan a la venta entran (y al anular, salen) en las existencias del artículo.
/// </summary>
public sealed class ExistenciasBodegaCatalogo : IExistenciasBodega, AlxorCore.Vivero.Aplicacion.IExistenciasVivero
{
    private readonly RegistrarMovimientoStock _movimiento;

    public ExistenciasBodegaCatalogo(RegistrarMovimientoStock movimiento) => _movimiento = movimiento;

    public Task<Resultado> EntrarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default) =>
        MoverAsync(empresaId, productoId, AlxorCore.Catalogo.Dominio.TipoMovimientoStock.Entrada, cantidad, motivo, ct);

    public Task<Resultado> SacarAsync(Guid empresaId, Guid productoId, decimal cantidad, string motivo, CancellationToken ct = default) =>
        MoverAsync(empresaId, productoId, AlxorCore.Catalogo.Dominio.TipoMovimientoStock.Salida, cantidad, motivo, ct);

    private async Task<Resultado> MoverAsync(Guid empresaId, Guid productoId, AlxorCore.Catalogo.Dominio.TipoMovimientoStock tipo, decimal cantidad, string motivo,
        CancellationToken ct)
    {
        var r = await _movimiento.EjecutarAsync(empresaId, productoId, new DatosMovimientoStock(tipo, cantidad, motivo), ct).ConfigureAwait(false);
        // Un artículo sin control de stock no lleva existencias: no es un error.
        return r.EsFallo && r.Error.Codigo != "producto.sin_control_stock" ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }
}

/// <summary>
/// Adaptador de <see cref="IVentasBodega"/> y de <see cref="AlxorCore.Vivero.Aplicacion.IVentasVivero"/>: la venta a granel (en litros) y la entrega
/// de un encargo de planta son un albarán de venta directo de una línea.
/// </summary>
public sealed class VentasBodegaFacturacion : IVentasBodega, AlxorCore.Vivero.Aplicacion.IVentasVivero
{
    private readonly CrearAlbaranVenta _crear;
    private readonly AnularAlbaranVenta _anular;

    public VentasBodegaFacturacion(CrearAlbaranVenta crear, AnularAlbaranVenta anular)
    {
        _crear = crear;
        _anular = anular;
    }

    public async Task<Resultado<(Guid Id, string Numero)>> AlbaranAsync(Guid empresaId, Guid clienteId, DateOnly fecha, string referencia, Guid productoId, decimal cantidad,
        decimal? precio, CancellationToken ct = default)
    {
        var r = await _crear.EjecutarAsync(empresaId, new CrearAlbaranVentaComando(clienteId, [new LineaAlbaranComando(cantidad, PrecioUnitario: precio, ProductoId: productoId)],
            fecha, referencia), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<(Guid, string)>(r.Error) : Resultado.Ok((r.Valor.Id, r.Valor.NumeroCompleto));
    }

    public async Task<Resultado> AnularAlbaranAsync(Guid albaranId, string motivo, CancellationToken ct = default)
    {
        var r = await _anular.EjecutarAsync(null, albaranId, motivo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }
}

/// <summary>Adaptador de <see cref="IAutofacturasBodega"/>: la uva comprada se registra como la autofactura agro (un gasto del viticultor).</summary>
public sealed class AutofacturasBodegaGastos : IAutofacturasBodega
{
    private readonly AutofacturasGastos _autofacturas;

    public AutofacturasBodegaGastos(AutofacturasGastos autofacturas) => _autofacturas = autofacturas;

    public Task<Resultado<(Guid GastoId, decimal Total)>> RegistrarAsync(Guid empresaId, Guid proveedorId, string concepto, DateOnly fecha, decimal baseImponible,
        string codigoImpuesto, decimal porcentajeRetencion, CancellationToken ct = default) =>
        _autofacturas.RegistrarAsync(empresaId, new AlxorCore.Agro.Aplicacion.AutofacturaAgro(proveedorId, concepto, fecha, baseImponible, codigoImpuesto, porcentajeRetencion), ct);

    public Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default) => _autofacturas.AnularAsync(gastoId, ct);
}
