using AlxorCore.Facturacion.Dominio;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Nucleo.Tiempo;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Facturacion.Aplicacion;

public interface IRepositorioLiquidacionesComision
{
    void Agregar(LiquidacionComision liquidacion);

    void Eliminar(LiquidacionComision liquidacion);

    Task<LiquidacionComision?> ObtenerAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<LiquidacionComision>> ListarAsync(Guid empresaId, Guid? clienteId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);

    /// <summary>Liquidaciones en borrador o confirmadas que tienen alguna línea de estos albaranes.</summary>
    Task<IReadOnlyList<LiquidacionComision>> VivasDeAlbaranesAsync(IReadOnlyCollection<Guid> albaranIds, CancellationToken ct = default);

    Task<int> SiguienteNumeroAsync(Guid empresaId, int ejercicio, CancellationToken ct = default);
}

/// <summary>Línea de la factura de gastos del comisionista.</summary>
public sealed record LineaGastoComisionista(string Descripcion, decimal Base, string CuentaGasto);

/// <summary>
/// Puerto hacia las facturas de proveedor: la factura de gastos del comisionista (su comisión, portes, aduanas…) cuando la
/// venta se liquida a bruto. Lo implementa la API sobre Gastos.
/// </summary>
public interface IGastosComisionista
{
    Task<Resultado<Guid>> RegistrarAsync(Guid empresaId, Guid proveedorId, DateOnly fecha, string numeroFactura, string? codigoIva, IReadOnlyList<LineaGastoComisionista> lineas,
        CancellationToken ct = default);

    Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default);
}

public sealed record LineaPendienteComisionDto(Guid AlbaranId, string AlbaranNumero, DateOnly Fecha, int Orden, Guid? ProductoId, string Descripcion, decimal Cantidad,
    decimal PrecioEstimado, decimal ImporteEstimado);

public sealed record LineaLiquidacionComisionDto(Guid AlbaranId, string AlbaranNumero, DateOnly AlbaranFecha, int Orden, Guid? ProductoId, string Descripcion, decimal CantidadEnviada,
    decimal CantidadVendida, decimal Merma, decimal PrecioBruto, decimal ImporteBruto, decimal Gastos, decimal ImporteNeto, decimal PrecioAlbaran, decimal PrecioEstimado,
    decimal? DiferenciaEstimado);

public sealed record GastoLiquidacionComisionDto(string Tipo, string? Descripcion, decimal? Porcentaje, decimal Importe);

public sealed record LiquidacionComisionDto(Guid Id, string? Numero, Guid ClienteId, string Cliente, DateOnly Fecha, string? ReferenciaCliente, string Modo, string Estado,
    Guid? ProveedorId, Guid? GastoId, decimal ImporteBruto, decimal TotalGastos, decimal ImporteNeto, decimal? PorcentajeGastos, string? Observaciones, string? MotivoAnulacion,
    IReadOnlyList<LineaLiquidacionComisionDto> Lineas, IReadOnlyList<GastoLiquidacionComisionDto> Gastos);

public sealed record DatosLineaLiquidacion(Guid AlbaranId, int Orden, decimal CantidadVendida, decimal PrecioBruto);

public sealed record DatosNuevaLiquidacionComision(Guid ClienteId, DateOnly Fecha, ModoLiquidacionComision Modo, IReadOnlyList<DatosLineaLiquidacion> Lineas,
    IReadOnlyList<DatosGastoComision>? Gastos = null, string? ReferenciaCliente = null, Guid? ProveedorId = null, string? CodigoIvaGastos = null, string? Observaciones = null);

/// <summary>Resultado de la venta en comisión de un cliente y un producto en el periodo.</summary>
public sealed record RentabilidadComisionDto(Guid ClienteId, string Cliente, Guid? ProductoId, string Producto, int Liquidaciones, decimal CantidadEnviada, decimal CantidadVendida,
    decimal PorcentajeMerma, decimal ImporteBruto, decimal Gastos, decimal ImporteNeto, decimal? PorcentajeGastos, decimal? PrecioBrutoMedio, decimal? PrecioNetoMedio,
    decimal? PrecioNetoPorEnviado);

/// <summary>
/// Venta en comisión: los albaranes enviados a precio por fijar se valoran con la liquidación del cliente (lo vendido, el
/// precio bruto y sus gastos). Confirmada, sus albaranes se facturan como cualquier otro, y el precio fijado es el que usa
/// la liquidación a resultas del agricultor.
/// </summary>
public sealed class LiquidacionesComision
{
    private readonly IRepositorioLiquidacionesComision _repo;
    private readonly IRepositorioAlbaranesVenta _albaranes;
    private readonly IConsultaClientes _clientes;
    private readonly IGastosComisionista _gastos;
    private readonly IUnidadDeTrabajoFacturacion _unidad;
    private readonly IReloj _reloj;

    public LiquidacionesComision(IRepositorioLiquidacionesComision repo, IRepositorioAlbaranesVenta albaranes, IConsultaClientes clientes, IGastosComisionista gastos,
        IUnidadDeTrabajoFacturacion unidad, IReloj reloj)
    {
        _repo = repo; _albaranes = albaranes; _clientes = clientes; _gastos = gastos; _unidad = unidad; _reloj = reloj;
    }

    public async Task<IReadOnlyList<LiquidacionComisionDto>> ListarAsync(Guid empresaId, Guid? clienteId, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default) =>
        (await _repo.ListarAsync(empresaId, clienteId, desde, hasta, ct).ConfigureAwait(false)).OrderByDescending(l => l.Fecha).ThenByDescending(l => l.Numero ?? int.MaxValue)
            .Select(Dto).ToList();

    public async Task<LiquidacionComisionDto?> ObtenerAsync(Guid empresaId, Guid id, CancellationToken ct = default) =>
        await DeEmpresaAsync(empresaId, id, ct).ConfigureAwait(false) is { } l ? Dto(l) : null;

    /// <summary>Líneas de los albaranes del cliente enviadas a precio por fijar que ninguna liquidación recoge todavía.</summary>
    public async Task<IReadOnlyList<LineaPendienteComisionDto>> PendientesAsync(Guid empresaId, Guid clienteId, CancellationToken ct = default)
    {
        var dtos = await _albaranes.ListarAsync(empresaId, new FiltroAlbaranesVenta(ClienteId: clienteId, Estado: EstadoAlbaranVenta.PendienteValorar), ct).ConfigureAwait(false);
        var albaranes = await _albaranes.ObtenerVariosAsync(dtos.Select(a => a.Id).ToList(), ct).ConfigureAwait(false);
        var ocupadas = await OcupadasAsync(albaranes.Select(a => a.Id).ToList(), null, ct).ConfigureAwait(false);
        return albaranes.OrderBy(a => a.Fecha).ThenBy(a => a.Numero)
            .SelectMany(a => a.Lineas.Where(l => !l.PrecioFijado && !ocupadas.Contains((a.Id, l.Orden)))
                .Select(l => new LineaPendienteComisionDto(a.Id, a.NumeroCompleto, a.Fecha, l.Orden, l.ProductoId, l.Descripcion, l.Cantidad, l.PrecioUnitario, l.BaseBruta)))
            .ToList();
    }

    public async Task<Resultado<LiquidacionComisionDto>> CrearAsync(Guid empresaId, DatosNuevaLiquidacionComision d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        var cliente = await _clientes.ObtenerAsync(d.ClienteId, ct).ConfigureAwait(false);
        if (cliente is null)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(Error.NoEncontrado("cliente.no_encontrado", "El cliente no existe."));
        }

        var datos = await DatosAsync(empresaId, null, d, ct).ConfigureAwait(false);
        if (datos.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(datos.Error);
        }

        var l = LiquidacionComision.Crear(empresaId, d.ClienteId, cliente.Nombre, datos.Valor, _reloj.AhoraUtc);
        if (l.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(l.Error);
        }

        _repo.Agregar(l.Valor);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l.Valor));
    }

    public async Task<Resultado<LiquidacionComisionDto>> CambiarAsync(Guid empresaId, Guid id, DatosNuevaLiquidacionComision d, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(d);
        if (await DeEmpresaAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return NoExiste();
        }

        if (d.ClienteId != l.ClienteId)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(Error.Validacion("liquidacion_comision.cliente", "La liquidación no cambia de cliente."));
        }

        var datos = await DatosAsync(empresaId, l.Id, d, ct).ConfigureAwait(false);
        if (datos.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(datos.Error);
        }

        var r = l.Cambiar(datos.Valor);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(r.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l));
    }

    public async Task<Resultado> EliminarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await DeEmpresaAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return Resultado.Fallo(Error.NoEncontrado("liquidacion_comision.no_encontrada", "La liquidación no existe."));
        }

        if (l.Estado != EstadoLiquidacionComision.Borrador)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion_comision.no_borrador", "Solo se elimina un borrador; la confirmada se anula."));
        }

        _repo.Eliminar(l);
        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok();
    }

    /// <summary>
    /// Confirma la liquidación: numera, valora las líneas de los albaranes (a neto o a bruto) y, a bruto, registra la factura
    /// de gastos del comisionista. Las líneas tienen que seguir por valorar y no estar en otra liquidación.
    /// </summary>
    public async Task<Resultado<LiquidacionComisionDto>> ConfirmarAsync(Guid empresaId, Guid id, CancellationToken ct = default)
    {
        if (await DeEmpresaAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return NoExiste();
        }

        if (l.Estado != EstadoLiquidacionComision.Borrador)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(Error.Conflicto("liquidacion_comision.no_borrador", "La liquidación ya está confirmada o anulada."));
        }

        var albaranes = (await _albaranes.ObtenerVariosAsync(l.Lineas.Select(x => x.AlbaranVentaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(a => a.Id);
        var ocupadas = await OcupadasAsync(albaranes.Keys.ToList(), l.Id, ct).ConfigureAwait(false);
        foreach (var x in l.Lineas)
        {
            if (!albaranes.TryGetValue(x.AlbaranVentaId, out var a) || a.Estado is EstadoAlbaranVenta.Anulado or EstadoAlbaranVenta.Facturado
                || a.Lineas.SingleOrDefault(y => y.Orden == x.OrdenAlbaran) is not { PrecioFijado: false } || ocupadas.Contains((x.AlbaranVentaId, x.OrdenAlbaran)))
            {
                return Resultado.Fallo<LiquidacionComisionDto>(Error.Conflicto("liquidacion_comision.linea_no_pendiente",
                    $"La línea {x.OrdenAlbaran} del albarán {x.AlbaranNumero} ya no está por valorar (valorada, facturada, anulada o en otra liquidación)."));
            }
        }

        foreach (var g in l.Lineas.GroupBy(x => x.AlbaranVentaId))
        {
            var r = albaranes[g.Key].Valorar(g.Select(x => (x.OrdenAlbaran, x.PrecioAlbaran, (decimal?)0m)).ToList());
            if (r.EsFallo)
            {
                return Resultado.Fallo<LiquidacionComisionDto>(r.Error);
            }
        }

        var numero = await _repo.SiguienteNumeroAsync(empresaId, l.Fecha.Year, ct).ConfigureAwait(false);
        Guid? gastoId = null;
        if (l.Modo == ModoLiquidacionComision.BrutoConFacturaGastos && l.TotalGastos > 0m)
        {
            var lineas = l.Gastos.GroupBy(g => (g.Tipo, g.Descripcion))
                .Select(g => new LineaGastoComisionista(g.Key.Descripcion ?? Texto(g.Key.Tipo), Redondeo.Dos(g.Sum(x => x.Importe)), Cuenta(g.Key.Tipo))).ToList();
            var gasto = await _gastos.RegistrarAsync(empresaId, l.ProveedorId!.Value, l.Fecha,
                l.ReferenciaCliente ?? $"{LiquidacionComision.Serie}-{l.Fecha.Year}-{numero:D6}", l.CodigoIvaGastos, lineas, ct).ConfigureAwait(false);
            if (gasto.EsFallo)
            {
                return Resultado.Fallo<LiquidacionComisionDto>(gasto.Error);
            }

            gastoId = gasto.Valor;
        }

        var ok = l.Confirmar(numero, gastoId);
        if (ok.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(ok.Error);
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l));
    }

    /// <summary>Anula la liquidación: sus albaranes (sin facturar) vuelven a su precio estimado y se anula la factura de gastos.</summary>
    public async Task<Resultado<LiquidacionComisionDto>> AnularAsync(Guid empresaId, Guid id, string? motivo, CancellationToken ct = default)
    {
        if (await DeEmpresaAsync(empresaId, id, ct).ConfigureAwait(false) is not { } l)
        {
            return NoExiste();
        }

        var r = l.Anular(motivo);
        if (r.EsFallo)
        {
            return Resultado.Fallo<LiquidacionComisionDto>(r.Error);
        }

        var albaranes = (await _albaranes.ObtenerVariosAsync(l.Lineas.Select(x => x.AlbaranVentaId).Distinct().ToList(), ct).ConfigureAwait(false)).ToDictionary(a => a.Id);
        foreach (var g in l.Lineas.GroupBy(x => x.AlbaranVentaId))
        {
            if (!albaranes.TryGetValue(g.Key, out var a))
            {
                continue;
            }

            var q = a.QuitarValoracion(g.Select(x => (x.OrdenAlbaran, x.PrecioEstimado, x.DescuentoAnterior)).ToList());
            if (q.EsFallo)
            {
                return Resultado.Fallo<LiquidacionComisionDto>(q.Error);
            }
        }

        if (l.GastoId is { } gasto)
        {
            var anulado = await _gastos.AnularAsync(gasto, ct).ConfigureAwait(false);
            if (anulado.EsFallo)
            {
                return Resultado.Fallo<LiquidacionComisionDto>(anulado.Error);
            }
        }

        await _unidad.GuardarCambiosAsync(ct).ConfigureAwait(false);
        return Resultado.Ok(Dto(l));
    }

    /// <summary>Lo que dejó la venta en comisión por cliente y producto: mermas, gastos y precio neto medio.</summary>
    public async Task<IReadOnlyList<RentabilidadComisionDto>> RentabilidadAsync(Guid empresaId, DateOnly desde, DateOnly hasta, Guid? clienteId, CancellationToken ct = default)
    {
        var liqs = (await _repo.ListarAsync(empresaId, clienteId, desde, hasta, ct).ConfigureAwait(false)).Where(l => l.Estado == EstadoLiquidacionComision.Confirmada).ToList();
        return liqs.SelectMany(l => l.Lineas.Select(x => (Liq: l, Linea: x)))
            .GroupBy(x => (x.Liq.ClienteId, x.Linea.ProductoId))
            .Select(g =>
            {
                var enviada = g.Sum(x => x.Linea.CantidadEnviada);
                var vendida = g.Sum(x => x.Linea.CantidadVendida);
                var bruto = Redondeo.Dos(g.Sum(x => x.Linea.ImporteBruto));
                var gastos = Redondeo.Dos(g.Sum(x => x.Linea.Gastos));
                var neto = Redondeo.Dos(bruto - gastos);
                return new RentabilidadComisionDto(g.Key.ClienteId, g.First().Liq.ClienteNombre, g.Key.ProductoId, g.First().Linea.Descripcion,
                    g.Select(x => x.Liq.Id).Distinct().Count(), enviada, vendida, Pct(enviada - vendida, enviada) ?? 0m, bruto, gastos, neto, Pct(gastos, bruto),
                    Precio(bruto, vendida), Precio(neto, vendida), Precio(neto, enviada));
            })
            .OrderByDescending(r => r.PrecioNetoPorEnviado ?? 0m).ToList();
    }

    // ------------------------------------------------------------------ Apoyo
    private async Task<Resultado<DatosLiquidacionComision>> DatosAsync(Guid empresaId, Guid? propia, DatosNuevaLiquidacionComision d, CancellationToken ct)
    {
        var albaranes = (await _albaranes.ObtenerVariosAsync((d.Lineas ?? []).Select(x => x.AlbaranId).Distinct().ToList(), ct).ConfigureAwait(false))
            .Where(a => a.EmpresaId == empresaId).ToDictionary(a => a.Id);
        var ocupadas = await OcupadasAsync(albaranes.Keys.ToList(), propia, ct).ConfigureAwait(false);
        var lineas = new List<DatosLineaComision>();
        foreach (var x in d.Lineas ?? [])
        {
            if (!albaranes.TryGetValue(x.AlbaranId, out var a) || a.Lineas.SingleOrDefault(y => y.Orden == x.Orden) is not { } linea)
            {
                return Resultado.Fallo<DatosLiquidacionComision>(Error.NoEncontrado("liquidacion_comision.albaran", "Alguna línea de albarán no existe."));
            }

            if (a.Estado is EstadoAlbaranVenta.Anulado or EstadoAlbaranVenta.Facturado || linea.PrecioFijado)
            {
                return Resultado.Fallo<DatosLiquidacionComision>(Error.Conflicto("liquidacion_comision.linea_no_pendiente",
                    $"La línea {x.Orden} del albarán {a.NumeroCompleto} no está por valorar: solo se liquida lo enviado a precio por fijar."));
            }

            if (ocupadas.Contains((a.Id, x.Orden)))
            {
                return Resultado.Fallo<DatosLiquidacionComision>(Error.Conflicto("liquidacion_comision.linea_en_otra",
                    $"La línea {x.Orden} del albarán {a.NumeroCompleto} ya está en otra liquidación."));
            }

            lineas.Add(new DatosLineaComision(new LineaAlbaranLiquidable(a.Id, a.NumeroCompleto, a.Fecha, a.ClienteId, linea.Orden, linea.ProductoId, linea.Descripcion,
                linea.Cantidad, linea.PrecioUnitario, linea.PorcentajeDescuento), x.CantidadVendida, x.PrecioBruto));
        }

        return Resultado.Ok(new DatosLiquidacionComision(d.Fecha, d.Modo, lineas, d.Gastos, d.ReferenciaCliente, d.ProveedorId, d.CodigoIvaGastos, d.Observaciones));
    }

    private async Task<HashSet<(Guid, int)>> OcupadasAsync(IReadOnlyCollection<Guid> albaranes, Guid? excepto, CancellationToken ct) =>
        (await _repo.VivasDeAlbaranesAsync(albaranes, ct).ConfigureAwait(false)).Where(l => l.Id != excepto)
            .SelectMany(l => l.Lineas.Select(x => (x.AlbaranVentaId, x.OrdenAlbaran))).ToHashSet();

    private async Task<LiquidacionComision?> DeEmpresaAsync(Guid empresaId, Guid id, CancellationToken ct) =>
        await _repo.ObtenerAsync(id, ct).ConfigureAwait(false) is { } l && l.EmpresaId == empresaId ? l : null;

    private static string Texto(TipoGastoComision t) => t switch
    {
        TipoGastoComision.Comision => "Comisión de venta",
        TipoGastoComision.Transporte => "Transporte",
        TipoGastoComision.Aduanas => "Aduanas",
        TipoGastoComision.Manipulacion => "Manipulación",
        TipoGastoComision.Frio => "Frío y almacenaje",
        TipoGastoComision.Publicidad => "Publicidad",
        _ => "Otros gastos",
    };

    /// <summary>Cuenta del PGC del gasto: comisiones a la 623, transportes a la 624, publicidad a la 627 y el resto a la 629.</summary>
    private static string Cuenta(TipoGastoComision t) => t switch
    {
        TipoGastoComision.Comision => "623",
        TipoGastoComision.Transporte => "624",
        TipoGastoComision.Publicidad => "627",
        _ => "629",
    };

    private static decimal? Pct(decimal parte, decimal total) => total == 0m ? null : Math.Round(parte * 100m / total, 2, MidpointRounding.AwayFromZero);

    private static decimal? Precio(decimal importe, decimal cantidad) => cantidad == 0m ? null : Math.Round(importe / cantidad, 4, MidpointRounding.AwayFromZero);

    private static Resultado<LiquidacionComisionDto> NoExiste() =>
        Resultado.Fallo<LiquidacionComisionDto>(Error.NoEncontrado("liquidacion_comision.no_encontrada", "La liquidación no existe."));

    private static LiquidacionComisionDto Dto(LiquidacionComision l) => new(l.Id, l.NumeroCompleto, l.ClienteId, l.ClienteNombre, l.Fecha, l.ReferenciaCliente, l.Modo.ToString(),
        l.Estado.ToString(), l.ProveedorId, l.GastoId, l.ImporteBruto, l.TotalGastos, l.ImporteNeto, Pct(l.TotalGastos, l.ImporteBruto), l.Observaciones, l.MotivoAnulacion,
        l.Lineas.OrderBy(x => x.AlbaranFecha).ThenBy(x => x.AlbaranNumero, StringComparer.Ordinal).ThenBy(x => x.OrdenAlbaran)
            .Select(x => new LineaLiquidacionComisionDto(x.AlbaranVentaId, x.AlbaranNumero, x.AlbaranFecha, x.OrdenAlbaran, x.ProductoId, x.Descripcion, x.CantidadEnviada,
                x.CantidadVendida, x.Merma, x.PrecioBruto, x.ImporteBruto, x.Gastos, x.ImporteNeto, x.PrecioAlbaran, x.PrecioEstimado,
                x.PrecioEstimado == 0m ? null : Math.Round(x.PrecioAlbaran - x.PrecioEstimado, 4, MidpointRounding.AwayFromZero))).ToList(),
        l.Gastos.Select(g => new GastoLiquidacionComisionDto(g.Tipo.ToString(), g.Descripcion ?? Texto(g.Tipo), g.Porcentaje, g.Importe)).ToList());
}
