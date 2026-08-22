using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Puertos;

namespace AlxorCore.Facturacion.Aplicacion;

/// <summary>Datos para anular una factura.</summary>
public sealed record AnularFacturaComando(string Motivo);

/// <summary>
/// Caso de uso: <b>anular</b> una factura emitida conforme a la ley antifraude. La factura no se
/// borra ni se modifica en sus importes: cambia a estado <c>Anulada</c>, se guarda el motivo y se
/// genera un <b>registro de anulación</b> VeriFactu con su huella encadenada.
/// </summary>
public sealed class AnularFactura
{
    private readonly IRepositorioFacturas _facturas;
    private readonly IConsultaEmpresas _empresas;
    private readonly IUnidadDeTrabajoFacturacion _unidadDeTrabajo;
    private readonly IReloj _reloj;

    public AnularFactura(IRepositorioFacturas facturas, IConsultaEmpresas empresas, IUnidadDeTrabajoFacturacion unidadDeTrabajo, IReloj reloj)
    {
        _facturas = facturas;
        _empresas = empresas;
        _unidadDeTrabajo = unidadDeTrabajo;
        _reloj = reloj;
    }

    public async Task<Resultado<FacturaDto>> EjecutarAsync(Guid empresaId, Guid facturaId, AnularFacturaComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var factura = await _facturas.ObtenerPorIdAsync(facturaId, ct).ConfigureAwait(false);
        if (factura is null)
        {
            return Resultado.Fallo<FacturaDto>(Error.NoEncontrado("factura.no_encontrada", "La factura no existe."));
        }

        var emisor = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
        var huellaAnterior = await _facturas.UltimaHuellaAsync(empresaId, ct).ConfigureAwait(false);

        var r = factura.Anular(comando.Motivo, emisor?.Nif ?? string.Empty, huellaAnterior, _reloj.AhoraUtc);
        if (r.EsFallo)
        {
            return Resultado.Fallo<FacturaDto>(r.Error);
        }

        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(FacturaDto.Desde(factura));
    }
}
