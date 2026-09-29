using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>
/// Calcula el riesgo vivo de un tercero sumando el pendiente (total − liquidado) de sus documentos.
/// Implementa el puerto compartido <see cref="IConsultaRiesgo"/>.
/// </summary>
public sealed class ConsultaRiesgo : IConsultaRiesgo
{
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IRepositorioPedidosVenta? _pedidos;
    private readonly IRepositorioAlbaranesVenta? _albaranes;

    public ConsultaRiesgo(IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioMovimientos movimientos, IRepositorioPedidosVenta? pedidos = null,
        IRepositorioAlbaranesVenta? albaranes = null)
    {
        _facturas = facturas;
        _gastos = gastos;
        _movimientos = movimientos;
        _pedidos = pedidos;
        _albaranes = albaranes;
    }

    public async Task<decimal> PendienteFacturarClienteAsync(Guid empresaId, Guid clienteId, Guid? excluirPedidoId = null, CancellationToken ct = default)
    {
        var pendiente = 0m;
        if (_pedidos is not null)
        {
            // Lo que falta por facturar de cada línea, en proporción a su base (con sus conceptos).
            foreach (var p in (await _pedidos.ListarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(p => p.ClienteId == clienteId && p.Id != excluirPedidoId && p.Estado is "Confirmado" or "Servido"))
            {
                pendiente += p.Lineas.Where(l => l.Cantidad > 0m && l.CantidadFacturada < l.Cantidad)
                    .Sum(l => l.Base * (l.Cantidad - l.CantidadFacturada) / l.Cantidad);
            }
        }

        if (_albaranes is not null)
        {
            // Los albaranes de un pedido ya cuentan en su pedido; los directos, por su base.
            pendiente += (await _albaranes.ListarAsync(empresaId, new FiltroAlbaranesVenta(ClienteId: clienteId), ct).ConfigureAwait(false))
                .Where(a => a.PedidoId is null && a.FacturaId is null && !a.Anulado && a.Estado != "Facturado").Sum(a => a.Base);
        }

        return Redondeo.Dos(pendiente);
    }

    public async Task<decimal> RiesgoVivoClienteAsync(Guid empresaId, Guid clienteId, CancellationToken ct = default)
    {
        var facturas = await _facturas.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var riesgo = 0m;
        foreach (var f in facturas.Where(f => f.ClienteId == clienteId && f.Estado == "Emitida"))
        {
            var liquidado = await _movimientos.SumaAsync(TipoDocumentoTesoreria.Factura, f.Id, ct).ConfigureAwait(false);
            var pendiente = Redondeo.Dos(f.Total - liquidado);
            if (pendiente > 0m)
            {
                riesgo += pendiente;
            }
        }

        return Redondeo.Dos(riesgo);
    }

    public async Task<decimal> RiesgoVivoProveedorAsync(Guid empresaId, Guid proveedorId, CancellationToken ct = default)
    {
        var gastos = await _gastos.ListarAsync(empresaId, ct).ConfigureAwait(false);
        var riesgo = 0m;
        foreach (var g in gastos.Where(g => g.ProveedorId == proveedorId))
        {
            var liquidado = await _movimientos.SumaAsync(TipoDocumentoTesoreria.Gasto, g.Id, ct).ConfigureAwait(false);
            var pendiente = Redondeo.Dos(g.Total - liquidado);
            if (pendiente > 0m)
            {
                riesgo += pendiente;
            }
        }

        return Redondeo.Dos(riesgo);
    }
}
