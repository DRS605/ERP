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

    public ConsultaRiesgo(IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioMovimientos movimientos)
    {
        _facturas = facturas;
        _gastos = gastos;
        _movimientos = movimientos;
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
