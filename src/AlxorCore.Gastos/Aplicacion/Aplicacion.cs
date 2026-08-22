using AlxorCore.Gastos.Dominio;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Organizacion.Aplicacion.Modelos;
using AlxorCore.Organizacion.Aplicacion.Puertos;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Gastos.Aplicacion;

/// <summary>Vista de un gasto.</summary>
public sealed record GastoDto(
    Guid Id, Guid? ProveedorId, string? ProveedorTexto, string Concepto, DateOnly Fecha,
    decimal BaseImponible, string CodigoIva, decimal PorcentajeIva, decimal CuotaIva,
    decimal PorcentajeIrpf, decimal RetencionIrpf, decimal Total, string Estado, string? AvisoRiesgo = null)
{
    public static GastoDto Desde(Gasto g) => new(
        g.Id, g.ProveedorId, g.ProveedorTexto, g.Concepto, g.Fecha, g.BaseImponible, g.CodigoIva, g.PorcentajeIva, g.CuotaIva,
        g.PorcentajeIrpf, g.RetencionIrpf, g.Total, g.Estado.ToString());
}

/// <summary>Repositorio de gastos (escritura).</summary>
public interface IRepositorioGastos
{
    Task<Gasto?> ObtenerPorIdAsync(Guid id, CancellationToken ct = default);

    void Agregar(Gasto gasto);
}

/// <summary>Consultas de lectura de gastos (las usan la API, Tesorería e Informes).</summary>
public interface IConsultaGastos
{
    Task<GastoDto?> ObtenerAsync(Guid gastoId, CancellationToken ct = default);

    Task<IReadOnlyList<GastoDto>> ListarAsync(Guid empresaId, CancellationToken ct = default);
}

/// <summary>Unidad de trabajo del módulo Gastos.</summary>
public interface IUnidadDeTrabajoGastos : IUnidadDeTrabajo;

/// <summary>Datos para registrar un gasto.</summary>
public sealed record RegistrarGastoComando(
    string Concepto,
    decimal BaseImponible,
    Guid? ProveedorId = null,
    string? ProveedorTexto = null,
    string? CodigoIva = null,
    decimal PorcentajeIrpf = 0m,
    DateOnly? Fecha = null,
    Guid? FormaPagoId = null);

/// <summary>Caso de uso: registrar un gasto. Si se indica un proveedor, se copia su nombre.</summary>
public sealed class RegistrarGasto
{
    private readonly IRepositorioGastos _gastos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IUnidadDeTrabajoGastos _unidadDeTrabajo;
    private readonly IColaContabilizacion _cola;
    private readonly IConsultaFormasPago _formasPago;
    private readonly IPagosAutomaticos _pagos;
    private readonly IConsultaRiesgo _riesgo;
    private readonly IConsultaEmpresas _empresas;
    private readonly IReloj _reloj;

    public RegistrarGasto(
        IRepositorioGastos gastos,
        IConsultaProveedores proveedores,
        IUnidadDeTrabajoGastos unidadDeTrabajo,
        IColaContabilizacion cola,
        IConsultaFormasPago formasPago,
        IPagosAutomaticos pagos,
        IConsultaRiesgo riesgo,
        IConsultaEmpresas empresas,
        IReloj reloj)
    {
        _gastos = gastos;
        _proveedores = proveedores;
        _unidadDeTrabajo = unidadDeTrabajo;
        _cola = cola;
        _formasPago = formasPago;
        _pagos = pagos;
        _riesgo = riesgo;
        _empresas = empresas;
        _reloj = reloj;
    }

    public async Task<Resultado<GastoDto>> EjecutarAsync(Guid empresaId, RegistrarGastoComando comando, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var proveedorTexto = comando.ProveedorTexto;
        string? tipoTercero = null;
        Guid? formaPagoDefectoId = null;
        decimal? limiteRiesgo = null;
        Guid? proveedorRiesgoId = null;
        if (comando.ProveedorId is { } provId)
        {
            var proveedor = await _proveedores.ObtenerAsync(provId, ct).ConfigureAwait(false);
            if (proveedor is null)
            {
                return Resultado.Fallo<GastoDto>(Error.NoEncontrado("proveedor.no_encontrado", "El proveedor no existe."));
            }

            proveedorTexto = proveedor.Nombre;
            tipoTercero = proveedor.Tipo;
            formaPagoDefectoId = proveedor.FormaPagoDefectoId;
            limiteRiesgo = proveedor.LimiteRiesgo;
            proveedorRiesgoId = provId;
        }

        var formaPagoId = comando.FormaPagoId ?? formaPagoDefectoId;
        FormaPagoDto? formaPago = formaPagoId is { } fpid
            ? await _formasPago.ObtenerAsync(fpid, ct).ConfigureAwait(false)
            : null;

        var fecha = comando.Fecha ?? DateOnly.FromDateTime(_reloj.AhoraUtc.UtcDateTime);
        var gasto = Gasto.Registrar(empresaId, comando.ProveedorId, proveedorTexto, comando.Concepto, fecha, comando.BaseImponible, comando.CodigoIva, comando.PorcentajeIrpf, _reloj);
        if (gasto.EsFallo)
        {
            return Resultado.Fallo<GastoDto>(gasto.Error);
        }

        // Control de riesgo del proveedor (antes de guardar). Configurable por empresa: avisar o bloquear.
        string? avisoRiesgo = null;
        if (limiteRiesgo is { } limite && proveedorRiesgoId is { } provRiesgoId)
        {
            var riesgoVivo = await _riesgo.RiesgoVivoProveedorAsync(empresaId, provRiesgoId, ct).ConfigureAwait(false);
            var total = gasto.Valor.Total;
            if (riesgoVivo + total > limite)
            {
                var emp = await _empresas.ObtenerAsync(empresaId, ct).ConfigureAwait(false);
                if ((emp?.ControlRiesgo ?? ControlRiesgo.Aviso) == ControlRiesgo.Bloqueo)
                {
                    return Resultado.Fallo<GastoDto>(Error.Conflicto("riesgo.superado",
                        $"El proveedor supera su límite de riesgo ({limite:F2} €): riesgo vivo {riesgoVivo:F2} € + este gasto {total:F2} €."));
                }

                avisoRiesgo = $"El proveedor supera su límite de riesgo ({limite:F2} €). Riesgo tras este gasto: {riesgoVivo + total:F2} €.";
            }
        }

        _gastos.Agregar(gasto.Valor);
        await _unidadDeTrabajo.GuardarCambiosAsync(ct).ConfigureAwait(false);

        // Encola el gasto para contabilizar (queda pendiente salvo contabilización automática).
        var g = gasto.Valor;
        await _cola.EncolarAsync(empresaId, new DocumentoContabilizable(
            SentidoContable.Compra, "Gasto", g.Id, g.Concepto, g.ProveedorId, g.ProveedorTexto ?? g.Concepto,
            g.Fecha, g.BaseImponible, g.CodigoIva, g.CuotaIva, g.PorcentajeIrpf, g.RetencionIrpf, g.Total, TipoTercero: tipoTercero), ct).ConfigureAwait(false);

        // Forma de pago «ya pagada»: registra el pago total en el acto (no genera vencimiento abierto).
        if (formaPago?.RegistrarPagoAutomatico == true)
        {
            await _pagos.RegistrarPagoTotalAsync(empresaId, g.Id, g.Total, g.Fecha, ct).ConfigureAwait(false);
        }

        return Resultado.Ok(GastoDto.Desde(g) with { AvisoRiesgo = avisoRiesgo });
    }
}

/// <summary>Caso de uso: listar los gastos de la empresa activa.</summary>
public sealed class ListarGastos
{
    private readonly IConsultaGastos _consulta;

    public ListarGastos(IConsultaGastos consulta) => _consulta = consulta;

    public Task<IReadOnlyList<GastoDto>> EjecutarAsync(Guid empresaId, CancellationToken ct = default) =>
        _consulta.ListarAsync(empresaId, ct);
}

/// <summary>Caso de uso: obtener un gasto.</summary>
public sealed class ObtenerGasto
{
    private readonly IConsultaGastos _consulta;

    public ObtenerGasto(IConsultaGastos consulta) => _consulta = consulta;

    public async Task<Resultado<GastoDto>> EjecutarAsync(Guid gastoId, CancellationToken ct = default)
    {
        var gasto = await _consulta.ObtenerAsync(gastoId, ct).ConfigureAwait(false);
        return gasto is null
            ? Resultado.Fallo<GastoDto>(Error.NoEncontrado("gasto.no_encontrado", "El gasto no existe."))
            : Resultado.Ok(gasto);
    }
}
