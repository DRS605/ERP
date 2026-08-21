using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Contabilidad.Dominio;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Recepcion.Aplicacion;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>
/// Implementación del puerto <see cref="IContabilizador"/> que decide según el <b>modo de
/// contabilidad</b> de la empresa:
/// <list type="bullet">
///   <item><b>Simple</b>: crea un gasto con IVA soportado (alimenta Libro de IVA / 303 / 130).</item>
///   <item><b>Completo</b>: además, genera el asiento de partida doble (libro diario y mayor).</item>
/// </list>
/// Sustituye al adaptador por defecto del módulo Recepción (se registra después).
/// </summary>
internal sealed class ContabilizadorSegunModo : IContabilizador
{
    private readonly RegistrarGasto _registrarGasto;
    private readonly GenerarAsientoCompra _generarAsiento;
    private readonly ObtenerModoContabilidad _obtenerModo;

    public ContabilizadorSegunModo(RegistrarGasto registrarGasto, GenerarAsientoCompra generarAsiento, ObtenerModoContabilidad obtenerModo)
    {
        _registrarGasto = registrarGasto;
        _generarAsiento = generarAsiento;
        _obtenerModo = obtenerModo;
    }

    public async Task<Resultado<ResultadoContabilizacion>> ContabilizarAsync(Guid empresaId, DatosContabilizacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        // 1) Siempre: el gasto con IVA soportado (base de los modelos fiscales).
        var comando = new RegistrarGastoComando(
            Concepto: datos.Concepto,
            BaseImponible: datos.BaseImponible,
            ProveedorId: datos.ProveedorId,
            ProveedorTexto: datos.ProveedorTexto,
            CodigoIva: datos.CodigoIva,
            PorcentajeIrpf: datos.PorcentajeIrpf,
            Fecha: datos.Fecha);
        var gasto = await _registrarGasto.EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<ResultadoContabilizacion>(gasto.Error);
        }

        // 2) En modo Completo: además, el asiento de partida doble.
        Guid? asientoId = null;
        var modo = await _obtenerModo.EjecutarAsync(empresaId, ct).ConfigureAwait(false);
        if (modo == ModoContabilidad.Completo)
        {
            var asiento = await _generarAsiento.EjecutarAsync(empresaId, datos.Concepto, datos.Fecha,
                datos.BaseImponible, datos.CodigoIva, datos.PorcentajeIrpf, ct).ConfigureAwait(false);
            if (asiento.EsFallo)
            {
                return Resultado.Fallo<ResultadoContabilizacion>(asiento.Error);
            }

            asientoId = asiento.Valor.Id;
        }

        return Resultado.Ok(new ResultadoContabilizacion(gasto.Valor.Id, asientoId));
    }
}
