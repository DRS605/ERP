using AlxorCore.Extensiones.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Dominio;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>Lo que vigilan las alertas en otros módulos: impagados (tesorería), riesgo de clientes y el certificado del SII.</summary>
public sealed class FuentesAlertasErp : IFuentesAlertas
{
    private readonly GestionImpagados _impagados;
    private readonly IConsultaClientes _clientes;
    private readonly IConsultaRiesgo _riesgo;
    private readonly IRepositorioEmpresas _empresas;
    private readonly AlxorCore.Informes.Aplicacion.GestionCertificadoSii _certificado;
    private readonly AlxorCore.Nucleo.Tiempo.IReloj _reloj;

    public FuentesAlertasErp(GestionImpagados impagados, IConsultaClientes clientes, IConsultaRiesgo riesgo, IRepositorioEmpresas empresas,
        AlxorCore.Informes.Aplicacion.GestionCertificadoSii certificado, AlxorCore.Nucleo.Tiempo.IReloj reloj)
    {
        _impagados = impagados;
        _clientes = clientes;
        _riesgo = riesgo;
        _empresas = empresas;
        _certificado = certificado;
        _reloj = reloj;
    }

    private static string Eur(decimal v) => AlxorCore.Nucleo.Comun.Redondeo.Formatear(v) + " €";

    public async Task<IReadOnlyList<HallazgoAlerta>> FacturasVencidasAsync(Guid empresaId, int dias, CancellationToken ct = default) =>
        (await _impagados.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(i => i.DiasRetraso >= dias && i.Pendiente > 0)
            .Select(i => new HallazgoAlerta($"factura:{i.FacturaId}", $"Factura {i.Numero} vencida hace {i.DiasRetraso} días",
                $"{i.ClienteNombre} · pendiente {Eur(i.Pendiente)} · vencía el {i.Vencimiento:dd/MM/yyyy}", "factura", i.FacturaId))
            .ToList();

    public async Task<IReadOnlyList<HallazgoAlerta>> RiesgoSuperadoAsync(Guid empresaId, decimal porcentaje, CancellationToken ct = default)
    {
        if (await _empresas.ObtenerGrupoIdAsync(empresaId, ct).ConfigureAwait(false) is not { } grupo)
        {
            return [];
        }

        var hallazgos = new List<HallazgoAlerta>();
        foreach (var c in (await _clientes.ListarAsync(grupo, false, null, ct).ConfigureAwait(false)).Where(c => c.LimiteRiesgo is > 0))
        {
            var limite = c.LimiteRiesgo!.Value;
            var vivo = await _riesgo.RiesgoVivoClienteAsync(empresaId, c.Id, ct).ConfigureAwait(false)
                + await _riesgo.PendienteFacturarClienteAsync(empresaId, c.Id, null, ct).ConfigureAwait(false);
            if (vivo >= limite * porcentaje / 100m)
            {
                hallazgos.Add(new HallazgoAlerta($"cliente:{c.Id}", $"{c.Nombre}: riesgo al {Math.Round(vivo * 100m / limite, 0, MidpointRounding.AwayFromZero)} % del límite",
                    $"Riesgo {Eur(vivo)} sobre un límite de {Eur(limite)}", "cliente", c.Id));
            }
        }

        return hallazgos;
    }

    public async Task<IReadOnlyList<HallazgoAlerta>> CertificadoCaducidadAsync(Guid empresaId, int dias, CancellationToken ct = default)
    {
        var c = await _certificado.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        if (c is null || c.CaducaEn > _reloj.AhoraUtc.AddDays(dias))
        {
            return [];
        }

        var restan = (int)Math.Floor((c.CaducaEn - _reloj.AhoraUtc).TotalDays);
        return
        [
            new HallazgoAlerta($"certificado:{c.CaducaEn:yyyyMMdd}", c.Caducado ? "El certificado del SII ha caducado" : $"El certificado del SII caduca en {restan} días",
                $"{c.Titular} · caduca el {c.CaducaEn:dd/MM/yyyy}"),
        ];
    }
}

/// <summary>Traduce un evento de dominio a una alerta de las reglas de evento.</summary>
public static class EventosAlertas
{
    public static (string Titulo, string? Detalle, string? Entidad, Guid? EntidadId, Guid? EventoId)? Describir(IEventoDominio evento) => evento switch
    {
        AlxorCore.Facturacion.Dominio.FacturaEmitida e => ($"Factura {e.NumeroCompleto} emitida", $"Total {Eur(e.Total)}", "factura", e.FacturaId, e.FacturaId),
        AlxorCore.Facturacion.Dominio.FacturaAnulada e => ($"Factura {e.NumeroCompleto} anulada", e.Motivo, "factura", e.FacturaId, e.FacturaId),
        AlxorCore.Facturacion.Dominio.AlbaranVentaEmitido e => ("Albarán de venta emitido", null, "albaran_venta", e.AlbaranId, e.AlbaranId),
        AlxorCore.Facturacion.Dominio.AlbaranVentaAnulado e => ($"Albarán {e.NumeroCompleto} anulado", e.Motivo, "albaran_venta", e.AlbaranId, e.AlbaranId),
        AlxorCore.Gastos.Dominio.GastoRegistrado e => ("Factura de proveedor registrada", $"Total {Eur(e.Total)}", "gasto", e.GastoId, e.GastoId),
        AlxorCore.Tesoreria.Dominio.MovimientoRegistrado e => (e.Importe >= 0 ? "Cobro registrado" : "Pago registrado", Eur(Math.Abs(e.Importe)), null, null, e.MovimientoId),
        AlxorCore.Terceros.Dominio.ClienteCreado e => ("Cliente nuevo", null, "cliente", e.ClienteId, e.ClienteId),
        AlxorCore.Terceros.Dominio.ProveedorCreado e => ("Proveedor nuevo", null, "proveedor", e.ProveedorId, e.ProveedorId),
        AlxorCore.Catalogo.Dominio.ProductoCreado e => ("Artículo nuevo", null, "producto", e.ProductoId, e.ProductoId),
        _ => null,
    };

    private static string Eur(decimal v) => AlxorCore.Nucleo.Comun.Redondeo.Formatear(v) + " €";
}
