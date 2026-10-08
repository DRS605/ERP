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
            Fecha: datos.Fecha,
            NumeroFactura: datos.NumeroFactura,
            FechaFactura: datos.FechaFactura,
            RectificaGastoId: datos.Rectificacion?.RectificaGastoId,
            NumeroRectificado: datos.Rectificacion?.NumeroRectificado,
            FechaRectificada: datos.Rectificacion?.FechaRectificada,
            MotivoRectificacion: datos.Rectificacion?.Motivo,
            Lineas: Lineas(datos));

        var resultado = await _registrarGasto.EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
        return resultado.EsFallo
            ? Resultado.Fallo<ResultadoContabilizacion>(resultado.Error)
            : Resultado.Ok(new ResultadoContabilizacion(resultado.Valor.Id));
    }

    /// <summary>Las líneas del gasto: las indicadas (o una con la base) y, aparte, los suplidos (sin impuesto, a su cuenta).</summary>
    private static List<LineaGastoComando>? Lineas(DatosContabilizacion datos)
    {
        if (datos.Lineas is not { Count: > 0 } && datos.Suplidos is not { Count: > 0 })
        {
            return null;
        }

        var lineas = datos.Lineas is { Count: > 0 } l
            ? l.Select(x => new LineaGastoComando(x.Base, datos.CodigoIva, x.Descripcion, CuentaGasto: x.Cuenta)).ToList()
            : [new LineaGastoComando(datos.BaseImponible, datos.CodigoIva, datos.Concepto)];
        lineas.AddRange((datos.Suplidos ?? []).Select(s => new LineaGastoComando(s.Importe, datos.CodigoIva, s.Descripcion, CuentaGasto: s.Cuenta, Suplido: true)));
        return lineas;
    }
}
