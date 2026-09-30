using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Resultados;

namespace AlxorCore.Api.Comun;

/// <summary>La factura de gastos del comisionista (comisión, portes, aduanas…) como factura de proveedor.</summary>
public sealed class GastosComisionistaGastos : IGastosComisionista
{
    private readonly RegistrarGasto _registrar;
    private readonly AnularGasto _anular;

    public GastosComisionistaGastos(RegistrarGasto registrar, AnularGasto anular)
    {
        _registrar = registrar; _anular = anular;
    }

    public async Task<Resultado<Guid>> RegistrarAsync(Guid empresaId, Guid proveedorId, DateOnly fecha, string numeroFactura, string? codigoIva,
        IReadOnlyList<LineaGastoComisionista> lineas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        var r = await _registrar.EjecutarAsync(empresaId, new RegistrarGastoComando($"Gastos de la venta en comisión {numeroFactura}", lineas.Sum(l => l.Base), proveedorId,
            CodigoIva: codigoIva, Fecha: fecha, NumeroFactura: numeroFactura, FechaFactura: fecha,
            Lineas: lineas.Select(l => new LineaGastoComando(l.Base, codigoIva, l.Descripcion, CuentaGasto: l.CuentaGasto)).ToList()), ct).ConfigureAwait(false);
        return r.EsCorrecto ? Resultado.Ok(r.Valor.Id) : Resultado.Fallo<Guid>(r.Error);
    }

    public Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default) => _anular.EjecutarAsync(gastoId, ct);
}
