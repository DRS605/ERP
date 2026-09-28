using System.Globalization;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Aplicacion;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Anticipos facturados (art. 75.2 LIVA: el IVA se devenga al cobrar el anticipo). Une Facturación y Tesorería:
/// <list type="bullet">
///   <item>Alta: emite la factura del anticipo (base a la 438 y su IVA), registra su cobro y el anticipo enlazado.</item>
///   <item>Factura final: añade las líneas negativas que descuentan el anticipo (base e IVA, a la 438) y lo anota.</item>
///   <item>Anulación: la factura final devuelve el anticipo; la del anticipo lo anula (si no se ha descontado).</item>
/// </list>
/// </summary>
public sealed class AnticiposFacturacion : IAnticiposFactura
{
    /// <summary>Cuenta de anticipos de clientes del PGC.</summary>
    public const string CuentaAnticipos = "438";

    private readonly DescuentosAnticipo _descuentos;
    private readonly IReloj _reloj;

    public AnticiposFacturacion(DescuentosAnticipo descuentos, IReloj reloj)
    {
        _descuentos = descuentos;
        _reloj = reloj;
    }

    public async Task<Resultado<IReadOnlyList<LineaComando>>> LineasDescuentoAsync(Guid clienteId, IReadOnlyList<DescuentoAnticipoSolicitado> solicitados, decimal baseMaxima, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(solicitados);
        var lineas = new List<LineaComando>();
        var resto = Redondeo.Dos(baseMaxima);
        foreach (var s in solicitados.GroupBy(x => x.AnticipoId).Select(g => g.First()))
        {
            if (resto <= 0m)
            {
                break;
            }

            var p = await _descuentos.PrepararAsync(s.AnticipoId, clienteId, s.Base, ct).ConfigureAwait(false);
            if (p.EsFallo)
            {
                return Resultado.Fallo<IReadOnlyList<LineaComando>>(p.Error);
            }

            var (anticipo, baseAnticipo) = p.Valor;
            var b = Math.Min(baseAnticipo, resto);
            resto = Redondeo.Dos(resto - b);
            lineas.Add(new LineaComando(1m, $"Anticipo a cuenta, fra. {anticipo.FacturaNumero} de {anticipo.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}", -b,
                anticipo.CodigoIva, CuentaContable: CuentaAnticipos, AnticipoId: anticipo.Id));
        }

        return Resultado.Ok<IReadOnlyList<LineaComando>>(lineas);
    }

    public async Task<Resultado> AnotarAsync(Factura factura, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factura);
        var descuentos = factura.Lineas.Where(l => l.AnticipoId is not null)
            .Select(l => (l.AnticipoId!.Value, -l.Base, -(l.Base + l.CuotaIva + l.CuotaRecargo))).ToList();
        return await _descuentos.AnotarAsync(factura.Id, factura.FechaEmision, descuentos, ct).ConfigureAwait(false);
    }

    public Task<Error?> ComprobarAnulacionAsync(Factura factura, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factura);
        return _descuentos.ComprobarAnulacionAsync(factura.Id, ct);
    }

    public async Task FacturaAnuladaAsync(Factura factura, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(factura);
        await _descuentos.RevertirAsync(factura.Lineas.Where(l => l.AnticipoId is not null).Select(l => l.AnticipoId!.Value), factura.Id, ct).ConfigureAwait(false);
        await _descuentos.FacturaAnticipoAnuladaAsync(factura.Id, _reloj, ct).ConfigureAwait(false);
    }
}

/// <summary>
/// Alta de un anticipo con su factura: emite la factura (base a la 438 e IVA; la base se ajusta para que el total sea
/// lo cobrado), registra el cobro de esa factura (57x a 430) y el anticipo enlazado a ella.
/// </summary>
public sealed class RegistrarAnticipoFacturado
{
    private readonly EmitirFactura _emitir;
    private readonly RegistrarCobro _cobro;
    private readonly RegistrarAnticipo _anticipo;

    public RegistrarAnticipoFacturado(EmitirFactura emitir, RegistrarCobro cobro, RegistrarAnticipo anticipo)
    {
        _emitir = emitir;
        _cobro = cobro;
        _anticipo = anticipo;
    }

    public async Task<Resultado<AnticipoDto>> EjecutarAsync(Guid empresaId, RegistrarAnticipoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);
        var importe = Redondeo.Dos(comando.Importe);
        if (importe <= 0m)
        {
            return Resultado.Fallo<AnticipoDto>(Error.Validacion("anticipo.importe_invalido", "El importe del anticipo debe ser mayor que cero."));
        }

        var codigoIva = string.IsNullOrWhiteSpace(comando.CodigoIva) ? "IVA21" : comando.CodigoIva.Trim();
        var concepto = string.IsNullOrWhiteSpace(comando.Concepto) ? "Entrega a cuenta" : comando.Concepto.Trim();
        EmitirFacturaComando Factura(decimal baseImponible) => new(
            comando.ClienteId, [new LineaComando(1m, $"Anticipo a cuenta: {concepto}", baseImponible, codigoIva, CuentaContable: AnticiposFacturacion.CuentaAnticipos)],
            FechaEmision: comando.Fecha, PorcentajeIrpf: 0m, DiasVencimiento: 0);

        // Base que da exactamente lo cobrado (con el tipo del impuesto y el recargo del cliente, si lo tiene).
        var prueba = await _emitir.SimularAsync(empresaId, Factura(100m), ct).ConfigureAwait(false);
        if (prueba.EsFallo)
        {
            return Resultado.Fallo<AnticipoDto>(prueba.Error);
        }

        var factor = prueba.Valor.Total / 100m;
        var aproximada = Redondeo.Dos(importe / factor);
        var baseImponible = aproximada;
        foreach (var candidata in new[] { aproximada, aproximada - 0.01m, aproximada + 0.01m, aproximada - 0.02m, aproximada + 0.02m })
        {
            var s = await _emitir.SimularAsync(empresaId, Factura(candidata), ct).ConfigureAwait(false);
            if (s.EsCorrecto && s.Valor.Total == importe)
            {
                baseImponible = candidata;
                break;
            }
        }

        var factura = await _emitir.EjecutarAsync(empresaId, Factura(baseImponible), ct).ConfigureAwait(false);
        if (factura.EsFallo)
        {
            return Resultado.Fallo<AnticipoDto>(factura.Error);
        }

        var f = factura.Valor;
        var cobro = await _cobro.EjecutarAsync(empresaId, new RegistrarCobroComando(f.Id, f.Total, comando.Fecha, comando.Metodo, comando.CuentaBancariaId), ct).ConfigureAwait(false);
        // «movimiento.sobrepago»: la forma de pago del cliente ya lo cobró al emitir (pago automático); nada que añadir.
        if (cobro.EsFallo && cobro.Error.Codigo != "movimiento.sobrepago")
        {
            return Resultado.Fallo<AnticipoDto>(Error.Conflicto(cobro.Error.Codigo,
                $"Se ha emitido la factura {f.NumeroCompleto} del anticipo, pero no su cobro: {cobro.Error.Mensaje} Regístralo en Cobros."));
        }

        return await _anticipo.EjecutarAsync(empresaId, comando with
        {
            Importe = f.Total,
            Concepto = concepto,
            Factura = new DatosFacturaAnticipo(f.Id, f.NumeroCompleto, f.BaseImponible, codigoIva),
        }, ct).ConfigureAwait(false);
    }
}
