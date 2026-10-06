using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

/// <summary>Aplicar un abono a una factura del mismo proveedor. Sin factura, la que rectifica; sin importe, todo lo que se pueda.</summary>
public sealed record AplicarAbonoComando(Guid? GastoId = null, decimal? Importe = null, DateOnly? Fecha = null);

public sealed record AplicacionAbonoDto(Guid AbonoId, Guid GastoId, decimal Importe, decimal PendienteAbono, decimal PendienteFactura);

/// <summary>
/// Un abono del proveedor (factura rectificativa recibida, total en negativo) no se paga: se aplica a lo que se le debe.
/// La factura queda pagada por ese importe y el abono liquidado, los dos contra la cuenta puente de compensaciones (555),
/// que queda a cero; la deuda con el proveedor (400) baja lo que el abono descuenta.
/// </summary>
public sealed class AplicarAbonoProveedor
{
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;
    private readonly ContabilizacionTesoreria? _contabilizacion;

    public AplicarAbonoProveedor(IConsultaGastos gastos, IRepositorioMovimientos movimientos, IUnidadDeTrabajoTesoreria unidad, IReloj reloj,
        ContabilizacionTesoreria? contabilizacion = null)
    {
        _gastos = gastos; _movimientos = movimientos; _unidad = unidad; _reloj = reloj; _contabilizacion = contabilizacion;
    }

    public async Task<Resultado<AplicacionAbonoDto>> EjecutarAsync(Guid empresaId, Guid abonoId, AplicarAbonoComando? c, CancellationToken ct = default)
    {
        c ??= new AplicarAbonoComando();
        var abono = await _gastos.ObtenerAsync(abonoId, ct).ConfigureAwait(false);
        if (abono is null)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.NoEncontrado("gasto.no_encontrado", "El abono no existe."));
        }

        if (abono.Estado == "Anulado" || abono.Total >= 0m)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Validacion("abono.no_es_abono", "Solo se aplica un abono (factura rectificativa en negativo) vigente."));
        }

        var disponible = -Redondeo.Dos(abono.Total - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Gasto, abonoId, ct).ConfigureAwait(false));
        if (disponible <= 0m)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Conflicto("abono.aplicado", "El abono ya está aplicado entero."));
        }

        var destinoId = c.GastoId ?? abono.RectificaGastoId;
        if (destinoId is null)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Validacion("abono.sin_factura", "Indica a qué factura del proveedor se aplica."));
        }

        var factura = await _gastos.ObtenerAsync(destinoId.Value, ct).ConfigureAwait(false);
        if (factura is null || factura.Estado == "Anulado" || factura.Total <= 0m)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Validacion("abono.factura", "La factura no existe, está anulada o no es una factura a pagar."));
        }

        if (factura.ProveedorId != abono.ProveedorId || factura.ProveedorId is null)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Validacion("abono.otro_proveedor", "El abono y la factura tienen que ser del mismo proveedor."));
        }

        var pendiente = Redondeo.Dos(factura.Total - await _movimientos.SumaAsync(TipoDocumentoTesoreria.Gasto, factura.Id, ct).ConfigureAwait(false));
        var importe = Redondeo.Dos(Math.Min(c.Importe ?? decimal.MaxValue, Math.Min(disponible, pendiente)));
        if (importe <= 0m || c.Importe is { } pedido && pedido > Math.Min(disponible, pendiente))
        {
            return Resultado.Fallo<AplicacionAbonoDto>(Error.Validacion("abono.importe",
                $"Se pueden aplicar como mucho {Redondeo.Formatear(Math.Min(disponible, pendiente))} € (pendiente del abono {Redondeo.Formatear(disponible)} €, de la factura {Redondeo.Formatear(pendiente)} €)."));
        }

        var fecha = c.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var documento = abono.NumeroFactura ?? abono.Concepto;
        var pago = Movimiento.Crear(empresaId, TipoDocumentoTesoreria.Gasto, factura.Id, SentidoMovimiento.Pago, importe, fecha, $"Abono {documento}", _reloj, null,
            CuentasPuente.Compensaciones);
        var aplicacion = Movimiento.CrearAplicacionAbono(empresaId, abonoId, importe, fecha, $"Aplicado a {factura.NumeroFactura ?? factura.Concepto}",
            CuentasPuente.Compensaciones, _reloj);
        if (pago.EsFallo || aplicacion.EsFallo)
        {
            return Resultado.Fallo<AplicacionAbonoDto>(pago.EsFallo ? pago.Error : aplicacion.Error);
        }

        _movimientos.Agregar(pago.Valor);
        _movimientos.Agregar(aplicacion.Valor);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.EncolarMovimientoAsync(pago.Valor, aplicacionAnticipo: false, ct: ct).ConfigureAwait(false);
            await _contabilizacion.EncolarMovimientoAsync(aplicacion.Valor, aplicacionAnticipo: false, ct: ct).ConfigureAwait(false);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        if (_contabilizacion is not null)
        {
            await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        }

        return Resultado.Ok(new AplicacionAbonoDto(abonoId, factura.Id, importe, Redondeo.Dos(disponible - importe), Redondeo.Dos(pendiente - importe)));
    }
}
