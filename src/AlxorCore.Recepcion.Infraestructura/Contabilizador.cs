using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Recepcion.Aplicacion;

namespace AlxorCore.Recepcion.Infraestructura;

/// <summary>
/// Contabilizador en modo "un libro": contabilizar una factura de proveedor equivale a
/// registrar un <c>Gasto</c> con su IVA soportado, que alimenta el Libro de IVA y los modelos
/// 303/130. Es la implementación por defecto del puerto <see cref="IContabilizador"/>.
/// Cuando se añada la partida doble, un segundo adaptador generará además el asiento contable
/// sin tocar el resto del módulo Recepción.
/// </summary>
internal sealed class ContabilizadorGastos : IContabilizador
{
    private readonly RegistrarGasto _registrarGasto;

    public ContabilizadorGastos(RegistrarGasto registrarGasto) => _registrarGasto = registrarGasto;

    public async Task<Resultado<ResultadoContabilizacion>> ContabilizarAsync(Guid empresaId, DatosContabilizacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);
        var comando = new RegistrarGastoComando(
            Concepto: datos.Concepto,
            BaseImponible: datos.BaseImponible,
            ProveedorId: datos.ProveedorId,
            ProveedorTexto: datos.ProveedorTexto,
            CodigoIva: datos.CodigoIva,
            PorcentajeIrpf: datos.PorcentajeIrpf,
            Fecha: datos.Fecha);

        var resultado = await _registrarGasto.EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
        return resultado.EsFallo
            ? Resultado.Fallo<ResultadoContabilizacion>(resultado.Error)
            : Resultado.Ok(new ResultadoContabilizacion(resultado.Valor.Id));
    }
}
