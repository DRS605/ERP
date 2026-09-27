using AlxorCore.Agro.Aplicacion;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador de <see cref="IAutofacturas"/>: la autofactura de una liquidación agrícola es un gasto del
/// proveedor-agricultor, con la compensación REAGP (o el IVA) y la retención; así entra en los libros de IVA,
/// en el 303 (casillas de compensaciones), en el 111, en contabilidad y en los pagos.
/// </summary>
public sealed class AutofacturasGastos : IAutofacturas
{
    private readonly RegistrarGasto _registrar;
    private readonly AnularGasto _anular;
    private readonly ConsultarSaldo _saldo;

    public AutofacturasGastos(RegistrarGasto registrar, AnularGasto anular, ConsultarSaldo saldo)
    {
        _registrar = registrar;
        _anular = anular;
        _saldo = saldo;
    }

    public async Task<Resultado<(Guid GastoId, decimal Total)>> RegistrarAsync(Guid empresaId, AutofacturaAgro autofactura, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(autofactura);
        var gasto = await _registrar.EjecutarAsync(empresaId, new RegistrarGastoComando(
            autofactura.Concepto, autofactura.BaseImponible, autofactura.ProveedorId, CodigoIva: autofactura.CodigoImpuesto,
            PorcentajeIrpf: autofactura.PorcentajeRetencion, Fecha: autofactura.Fecha), ct).ConfigureAwait(false);
        return gasto.EsFallo
            ? Resultado.Fallo<(Guid, decimal)>(gasto.Error)
            : Resultado.Ok((gasto.Valor.Id, gasto.Valor.Total));
    }

    public async Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default)
    {
        var saldo = await _saldo.DeGastoAsync(gastoId, ct).ConfigureAwait(false);
        if (saldo.EsCorrecto && saldo.Valor.Liquidado != 0m)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.pagada",
                $"La autofactura ya tiene pagos ({AlxorCore.Nucleo.Comun.Redondeo.Formatear(saldo.Valor.Liquidado, 2)} €): deshaz antes el pago."));
        }

        return await _anular.EjecutarAsync(gastoId, ct).ConfigureAwait(false);
    }
}

/// <summary>Adaptador de <see cref="ICosteAnalitico"/> sobre el informe de analítica (centros con sus descendientes).</summary>
public sealed class CosteAnaliticoContabilidad : ICosteAnalitico
{
    private readonly InformeAnalitico _informe;

    public CosteAnaliticoContabilidad(InformeAnalitico informe) => _informe = informe;

    public async Task<IReadOnlyDictionary<Guid, (decimal Gastos, decimal Ingresos)>?> PorCentroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var informe = await _informe.EjecutarAsync(empresaId, desde, hasta, DimensionAnalitica.Centro, ct).ConfigureAwait(false);
        var filas = informe.Filas.Where(f => f.Id is not null).ToList();
        return filas.Count == 0 ? null : filas.ToDictionary(f => f.Id!.Value, f => (f.Gastos, f.Ingresos));
    }
}

/// <summary>Adaptador de <see cref="IDocumentosExpedicion"/>: la carta de porte de una expedición de palés, en facturación.</summary>
public sealed class DocumentosExpedicionFacturacion : IDocumentosExpedicion
{
    private readonly CrearCartaPorte _crear;
    private readonly AnularCartaPorte _anular;

    public DocumentosExpedicionFacturacion(CrearCartaPorte crear, AnularCartaPorte anular)
    {
        _crear = crear;
        _anular = anular;
    }

    public async Task<Resultado<(Guid Id, string Numero)>> EmitirCartaPorteAsync(Guid empresaId, CartaPorteExpedicion carta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(carta);
        var r = await _crear.EjecutarAsync(empresaId, new CrearCartaPorteComando(
            carta.Lineas.Select(l => new LineaCartaPorteComando(l.Descripcion, l.Bultos, l.Kilos)).ToList(),
            FechaExpedicion: carta.Fecha, DestinatarioClienteId: carta.ClienteId, TransportistaNombre: carta.Transportista, Matricula: carta.Matricula,
            LugarOrigen: carta.LugarOrigen, LugarDestino: carta.LugarDestino, FechaCarga: carta.Fecha, Observaciones: carta.Observaciones), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<(Guid, string)>(r.Error) : Resultado.Ok((r.Valor.Id, r.Valor.NumeroCompleto));
    }

    public async Task<Resultado> AnularCartaPorteAsync(Guid cartaPorteId, string motivo, CancellationToken ct = default)
    {
        var r = await _anular.EjecutarAsync(cartaPorteId, motivo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }
}
