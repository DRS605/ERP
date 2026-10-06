using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Tesoreria.Dominio;

namespace AlxorCore.Tesoreria.Aplicacion;

public sealed record LineaRevalorizacionDto(string TipoDocumento, Guid DocumentoId, string Referencia, string TerceroNombre, string Moneda, decimal PendienteDivisa,
    decimal ValorLibros, decimal TasaCierre, decimal ValorCierre, decimal Diferencia, string Cuenta);

public sealed record RevalorizacionDivisaDto(Guid? Id, int Ejercicio, DateOnly FechaCierre, DateOnly FechaReversion, bool Anulada, decimal Ganancias, decimal Perdidas,
    IReadOnlyList<LineaRevalorizacionDto> Lineas)
{
    public static RevalorizacionDivisaDto Desde(RevalorizacionDivisa r, Guid? id = null) => new(id, r.Ejercicio, r.FechaCierre, r.FechaReversion, r.Anulada,
        Redondeo.Dos(r.Lineas.Where(l => l.EsGanancia).Sum(l => Math.Abs(l.Diferencia))), Redondeo.Dos(r.Lineas.Where(l => !l.EsGanancia).Sum(l => Math.Abs(l.Diferencia))),
        r.Lineas.Select(l => new LineaRevalorizacionDto(l.TipoDocumento.ToString(), l.DocumentoId, l.Referencia, l.TerceroNombre, l.Moneda,
            l.PendienteDivisa, l.ValorLibros, l.TasaCierre, l.ValorCierre, l.Diferencia, l.Diferencia == 0m ? "" : l.EsGanancia ? "768" : "668")).ToList());
}

public interface IRepositorioRevalorizaciones
{
    Task<IReadOnlyList<RevalorizacionDivisa>> ListarAsync(Guid empresaId, CancellationToken ct = default);

    Task<RevalorizacionDivisa?> ObtenerAsync(Guid id, CancellationToken ct = default);

    void Agregar(RevalorizacionDivisa revalorizacion);
}

/// <summary>
/// Revalorización de los cobros y pagos pendientes en divisa al cierre (PGC, NRV 11ª): lo pendiente de cada factura y
/// gasto en divisa a 31/12 se valora al tipo de cambio de ese día; la diferencia con su valor en libros (al tipo del
/// documento) va a 768 o 668 contra el cliente o proveedor, y se revierte el 1/1. Se calcula sin guardar para verla antes.
/// </summary>
public sealed class GestionRevalorizacionDivisa
{
    public const string Origen = "RevalorizacionDivisa";

    private readonly IRepositorioRevalorizaciones _repo;
    private readonly IConsultaFacturas _facturas;
    private readonly IConsultaGastos _gastos;
    private readonly IRepositorioMovimientos _movimientos;
    private readonly IConversorDivisa _conversor;
    private readonly ContabilizacionTesoreria _contabilizacion;
    private readonly IUnidadDeTrabajoTesoreria _unidad;
    private readonly IReloj _reloj;

    public GestionRevalorizacionDivisa(IRepositorioRevalorizaciones repo, IConsultaFacturas facturas, IConsultaGastos gastos, IRepositorioMovimientos movimientos,
        IConversorDivisa conversor, ContabilizacionTesoreria contabilizacion, IUnidadDeTrabajoTesoreria unidad, IReloj reloj)
    {
        _repo = repo; _facturas = facturas; _gastos = gastos; _movimientos = movimientos; _conversor = conversor; _contabilizacion = contabilizacion; _unidad = unidad; _reloj = reloj;
    }

    public async Task<IReadOnlyList<RevalorizacionDivisaDto>> ListarAsync(Guid empresaId, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).OrderByDescending(r => r.Ejercicio).ThenBy(r => r.Anulada)
            .Select(r => RevalorizacionDivisaDto.Desde(r, r.Id)).ToList();

    /// <summary>Calcula la revalorización del ejercicio; con <paramref name="guardar"/>, la registra con sus asientos.</summary>
    public async Task<Resultado<RevalorizacionDivisaDto>> CalcularAsync(Guid empresaId, int ejercicio, bool guardar, CancellationToken ct = default)
    {
        if (ejercicio is < 2000 or > 2100)
        {
            return Resultado.Fallo<RevalorizacionDivisaDto>(Error.Validacion("revalorizacion.ejercicio", "Indica el ejercicio."));
        }

        if (guardar && (await _repo.ListarAsync(empresaId, ct).ConfigureAwait(false)).Any(r => r.Ejercicio == ejercicio && !r.Anulada))
        {
            return Resultado.Fallo<RevalorizacionDivisaDto>(Error.Conflicto("revalorizacion.hecha", $"El ejercicio {ejercicio} ya está revalorizado: anúlalo para repetirlo."));
        }

        var cierre = new DateOnly(ejercicio, 12, 31);
        var pendientes = new List<(TipoDocumentoTesoreria Tipo, Guid Id, string Referencia, Guid? TerceroId, string Tercero, string Moneda, decimal Tasa, decimal Total, decimal TotalDivisa)>();
        foreach (var f in await _facturas.EnDivisaAsync(empresaId, cierre, ct).ConfigureAwait(false))
        {
            pendientes.Add((TipoDocumentoTesoreria.Factura, f.Id, f.NumeroCompleto, f.ClienteId, f.ClienteNombre, f.Moneda!, f.TasaCambio!.Value, f.Total, f.TotalDivisa!.Value));
        }

        foreach (var g in await _gastos.EnDivisaAsync(empresaId, cierre, ct).ConfigureAwait(false))
        {
            pendientes.Add((TipoDocumentoTesoreria.Gasto, g.Id, g.NumeroFactura ?? g.Concepto, g.ProveedorId, g.ProveedorTexto ?? g.Concepto, g.Moneda!, g.TasaCambio!.Value, g.Total, g.TotalDivisa!.Value));
        }

        var tasas = new Dictionary<string, decimal>();
        var lineas = new List<LineaRevalorizacion>();
        foreach (var p in pendientes)
        {
            // Lo liquidado hasta el cierre (los cobros y pagos de enero no cuentan).
            var movs = (await _movimientos.ListarAsync(p.Tipo, p.Id, ct).ConfigureAwait(false)).Where(m => m.Fecha <= cierre).ToList();
            var pendienteDivisa = Redondeo.Dos(p.TotalDivisa - movs.Sum(m => m.ImporteDivisa ?? 0m));
            var libros = Redondeo.Dos(p.Total - movs.Sum(m => m.Importe));
            if (pendienteDivisa <= 0m || libros <= 0m)
            {
                continue;
            }

            if (!tasas.TryGetValue(p.Moneda, out var tasa))
            {
                if (await _conversor.TasaVigenteAsync(empresaId, p.Moneda, cierre, ct).ConfigureAwait(false) is not { } t)
                {
                    return Resultado.Fallo<RevalorizacionDivisaDto>(Error.Validacion("revalorizacion.sin_tipo_cambio",
                        $"No hay tipo de cambio de {p.Moneda} a {cierre:dd/MM/yyyy}: regístralo en Divisas."));
                }

                tasa = t;
                tasas[p.Moneda] = t;
            }

            lineas.Add(new LineaRevalorizacion(p.Tipo, p.Id, p.Referencia, p.TerceroId, p.Tercero, p.Moneda, pendienteDivisa, libros, tasa, Redondeo.Dos(pendienteDivisa * tasa)));
        }

        var r = new RevalorizacionDivisa(empresaId, ejercicio, _reloj.AhoraUtc, lineas);
        if (!guardar)
        {
            return Resultado.Ok(RevalorizacionDivisaDto.Desde(r));
        }

        foreach (var l in r.Lineas.Where(l => l.Diferencia != 0m))
        {
            Encolar(r, l, reversion: false, deshacer: false);
            Encolar(r, l, reversion: true, deshacer: false);
        }

        _repo.Agregar(r);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(RevalorizacionDivisaDto.Desde(r, r.Id));
    }

    /// <summary>Anula una revalorización: contraasientos de su ajuste de cierre y de su reversión.</summary>
    public async Task<Resultado<RevalorizacionDivisaDto>> AnularAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        var r = await _repo.ObtenerAsync(id, ct).ConfigureAwait(false);
        if (r is null || r.EmpresaId != empresaId)
        {
            return Resultado.Fallo<RevalorizacionDivisaDto>(Error.NoEncontrado("revalorizacion.no_encontrada", "La revalorización no existe."));
        }

        if (r.Anulada)
        {
            return Resultado.Fallo<RevalorizacionDivisaDto>(Error.Conflicto("revalorizacion.anulada", "La revalorización ya está anulada."));
        }

        foreach (var l in r.Lineas.Where(l => l.Diferencia != 0m))
        {
            Encolar(r, l, reversion: false, deshacer: true);
            Encolar(r, l, reversion: true, deshacer: true);
        }

        r.Anular();
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        await _contabilizacion.DespacharAsync(ct: ct).ConfigureAwait(false);
        return Resultado.Ok(RevalorizacionDivisaDto.Desde(r, r.Id));
    }

    /// <summary>
    /// Asiento de una línea contra la cuenta del cliente (cobro) o del proveedor (pago). Ganancia: cliente al debe y 768 al
    /// haber (o proveedor al debe y 768); pérdida: 668 al debe y el cliente o proveedor al haber. La reversión y la anulación
    /// van al revés.
    /// </summary>
    private void Encolar(RevalorizacionDivisa r, LineaRevalorizacion l, bool reversion, bool deshacer)
    {
        var cobro = l.TipoDocumento == TipoDocumentoTesoreria.Factura;
        // En el asiento directo, un cobro lleva la cuenta indicada al debe y la del cliente al haber; un pago, la del
        // proveedor al debe y la indicada al haber. «Anulación» le da la vuelta.
        var alReves = cobro ? l.EsGanancia : !l.EsGanancia;
        var anulacion = alReves ^ reversion ^ deshacer;
        var etiqueta = (deshacer ? "anul-" : "") + (reversion ? "reversion" : "cierre");
        var texto = $"{(deshacer ? "Anulación: " : "")}{(reversion ? "Reversión" : "Revalorización")} {l.Moneda} {l.Referencia}";
        _contabilizacion.EncolarAsientoDirecto(r.EmpresaId, Origen, ContabilizacionTesoreria.Derivado(l.Id, etiqueta),
            cobro ? SentidoMovimiento.Cobro : SentidoMovimiento.Pago, texto, reversion ? r.FechaReversion : r.FechaCierre, Math.Abs(l.Diferencia),
            l.EsGanancia ? "768" : "668", string.Empty, anulacion, l.TerceroId, l.TerceroNombre);
    }
}
