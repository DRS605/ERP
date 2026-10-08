using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Recepcion.Aplicacion;

namespace AlxorCore.Contabilidad.Infraestructura;

/// <summary>
/// Implementación del puerto <see cref="IContabilizador"/> del módulo Recepción: contabilizar una
/// factura de proveedor equivale a registrar un <c>Gasto</c> con su IVA soportado (alimenta el Libro
/// de IVA y los modelos 303/130). El asiento de partida doble ya <b>no</b> se genera aquí: al
/// registrar el gasto se encola un documento pendiente de contabilizar y el asiento se genera desde
/// el panel del contable (o en el acto si la empresa tiene la contabilización automática activada).
/// Así se cumple el requisito de que las facturas no contabilicen por defecto y de que el contable
/// pueda ajustar la fecha de registro de las recibidas.
/// </summary>
internal sealed class ContabilizadorSegunModo : IContabilizador
{
    private readonly RegistrarGasto _registrarGasto;

    public ContabilizadorSegunModo(RegistrarGasto registrarGasto) => _registrarGasto = registrarGasto;

    public async Task<Resultado<ResultadoContabilizacion>> ContabilizarAsync(Guid empresaId, DatosContabilizacion datos, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(datos);

        // El gasto con IVA soportado es la base de los modelos fiscales; al registrarlo se encola el
        // documento para su asiento de partida doble (diferido salvo contabilización automática).
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

        var gasto = await _registrarGasto.EjecutarAsync(empresaId, comando, ct).ConfigureAwait(false);
        return gasto.EsFallo
            ? Resultado.Fallo<ResultadoContabilizacion>(gasto.Error)
            : Resultado.Ok(new ResultadoContabilizacion(gasto.Valor.Id));
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
