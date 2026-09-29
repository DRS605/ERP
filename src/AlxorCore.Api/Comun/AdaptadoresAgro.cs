using AlxorCore.Agro.Aplicacion;
using AlxorCore.Contabilidad.Aplicacion;
using AlxorCore.Facturacion.Aplicacion;
using AlxorCore.Gastos.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Tesoreria.Aplicacion;

namespace AlxorCore.Api.Comun;

/// <summary>
/// Adaptador de <see cref="IAutofacturas"/>: la autofactura de una liquidación agrícola es un gasto del
/// proveedor-agricultor, con la compensación REAGP (o el IVA) y la retención; así entra en los libros de IVA,
/// en el 303 (casillas de compensaciones), en el 111, en contabilidad y en los pagos.
/// </summary>
public sealed class AutofacturasGastos : IAutofacturas
{
    private readonly RegistrarGasto _registrar;
    private readonly AnularGasto _anular;
    private readonly ConsultarSaldo _saldo;

    public AutofacturasGastos(RegistrarGasto registrar, AnularGasto anular, ConsultarSaldo saldo)
    {
        _registrar = registrar;
        _anular = anular;
        _saldo = saldo;
    }

    public async Task<Resultado<(Guid GastoId, decimal Total)>> RegistrarAsync(Guid empresaId, AutofacturaAgro autofactura, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(autofactura);
        var gasto = await _registrar.EjecutarAsync(empresaId, new RegistrarGastoComando(
            autofactura.Concepto, autofactura.BaseImponible, autofactura.ProveedorId, CodigoIva: autofactura.CodigoImpuesto,
            PorcentajeIrpf: autofactura.PorcentajeRetencion, Fecha: autofactura.Fecha), ct).ConfigureAwait(false);
        return gasto.EsFallo
            ? Resultado.Fallo<(Guid, decimal)>(gasto.Error)
            : Resultado.Ok((gasto.Valor.Id, gasto.Valor.Total));
    }

    public async Task<Resultado> AnularAsync(Guid gastoId, CancellationToken ct = default)
    {
        var saldo = await _saldo.DeGastoAsync(gastoId, ct).ConfigureAwait(false);
        if (saldo.EsCorrecto && saldo.Valor.Liquidado != 0m)
        {
            return Resultado.Fallo(Error.Conflicto("liquidacion.pagada",
                $"La autofactura ya tiene pagos ({AlxorCore.Nucleo.Comun.Redondeo.Formatear(saldo.Valor.Liquidado, 2)} €): deshaz antes el pago."));
        }

        return await _anular.EjecutarAsync(gastoId, ct).ConfigureAwait(false);
    }
}

/// <summary>Adaptador de <see cref="ICosteAnalitico"/> sobre el informe de analítica (centros con sus descendientes).</summary>
public sealed class CosteAnaliticoContabilidad : ICosteAnalitico
{
    private readonly InformeAnalitico _informe;

    public CosteAnaliticoContabilidad(InformeAnalitico informe) => _informe = informe;

    public async Task<IReadOnlyDictionary<Guid, (decimal Gastos, decimal Ingresos)>?> PorCentroAsync(Guid empresaId, DateOnly desde, DateOnly hasta, CancellationToken ct = default)
    {
        var informe = await _informe.EjecutarAsync(empresaId, desde, hasta, DimensionAnalitica.Centro, ct).ConfigureAwait(false);
        var filas = informe.Filas.Where(f => f.Id is not null).ToList();
        return filas.Count == 0 ? null : filas.ToDictionary(f => f.Id!.Value, f => (f.Gastos, f.Ingresos));
    }
}

/// <summary>
/// Adaptador de <see cref="IDocumentosExpedicion"/> sobre facturación:
/// <list type="bullet">
/// <item>la carta de porte de una expedición de palés;</item>
/// <item>el albarán de venta del pedido, con lo expedido repartido en sus líneas pendientes del mismo artículo (en kilos
/// si el artículo se vende por kilos; en cajas si no).</item>
/// </list>
/// </summary>
public sealed class DocumentosExpedicionFacturacion : IDocumentosExpedicion
{
    private readonly CrearCartaPorte _crear;
    private readonly AnularCartaPorte _anular;
    private readonly ObtenerPedidoVenta _pedido;
    private readonly EntregarPedido _entregar;
    private readonly AnularAlbaranVenta _anularAlbaran;
    private readonly AlxorCore.Catalogo.Aplicacion.IConsultaProductos _productos;

    private readonly ListarPedidosVenta? _listar;
    private readonly CrearAlbaranVenta? _crearAlbaran;
    private readonly GestionDevolucionesVenta? _devoluciones;

    public DocumentosExpedicionFacturacion(CrearCartaPorte crear, AnularCartaPorte anular, ObtenerPedidoVenta pedido, EntregarPedido entregar,
        AnularAlbaranVenta anularAlbaran, AlxorCore.Catalogo.Aplicacion.IConsultaProductos productos, ListarPedidosVenta? listar = null,
        CrearAlbaranVenta? crearAlbaran = null, GestionDevolucionesVenta? devoluciones = null)
    {
        _devoluciones = devoluciones;
        _crearAlbaran = crearAlbaran;
        _listar = listar;
        _crear = crear;
        _anular = anular;
        _pedido = pedido;
        _entregar = entregar;
        _anularAlbaran = anularAlbaran;
        _productos = productos;
    }

    public async Task<Resultado<(Guid Id, string Numero)>> EmitirCartaPorteAsync(Guid empresaId, CartaPorteExpedicion carta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(carta);
        // Cada línea: los kilos de fruta son el peso neto (y, sin la tara de envases y palés, también el bruto), el
        // embalaje son sus cajas en palés y el código arancelario sale del artículo.
        var lineas = carta.Lineas.Select((l, i) =>
        {
            var d = carta.Detalle is { } det && i < det.Count ? det[i] : (ProductoId: (Guid?)null, Pales: 0);
            return new LineaCartaPorteComando(l.Descripcion, l.Bultos, l.Kilos, Embalaje: d.Pales > 0 ? $"Cajas en {d.Pales} palé(s)" : null,
                PesoNetoKg: l.Kilos, ProductoId: d.ProductoId);
        }).ToList();
        var transporte = new AlxorCore.Facturacion.Dominio.TransporteCarta
        {
            TransportistaId = carta.TransportistaId, VehiculoId = carta.VehiculoId, TemperaturaConsigna = carta.TemperaturaConsigna, Termografo = carta.Termografo,
        };
        var r = await _crear.EjecutarAsync(empresaId, new CrearCartaPorteComando(lineas,
            FechaExpedicion: carta.Fecha, DestinatarioClienteId: carta.ClienteId, TransportistaNombre: carta.Transportista, Matricula: carta.Matricula,
            LugarOrigen: carta.LugarOrigen, LugarDestino: carta.LugarDestino, FechaCarga: carta.Fecha, Observaciones: carta.Observaciones, AlbaranId: carta.AlbaranId,
            Transporte: transporte), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<(Guid, string)>(r.Error) : Resultado.Ok((r.Valor.Id, r.Valor.NumeroCompleto));
    }

    public async Task<Resultado> AnularCartaPorteAsync(Guid cartaPorteId, string motivo, CancellationToken ct = default)
    {
        var r = await _anular.EjecutarAsync(cartaPorteId, motivo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }

    public async Task<Guid?> ClienteDePedidoAsync(Guid pedidoVentaId, CancellationToken ct = default) =>
        (await _pedido.EjecutarAsync(pedidoVentaId, ct).ConfigureAwait(false))?.ClienteId;

    public async Task<PedidoParaReservas?> PedidoParaReservasAsync(Guid pedidoVentaId, CancellationToken ct = default) =>
        await _pedido.EjecutarAsync(pedidoVentaId, ct).ConfigureAwait(false) is { } p
            ? new PedidoParaReservas(p.Id, p.NumeroCompleto, p.ClienteId, p.ClienteNombre, p.Estado is "Confirmado" or "Servido",
                p.Lineas.Select(l => new LineaPedidoParaReservas(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.CantidadServida)).ToList())
            : null;

    public async Task<IReadOnlyList<PedidoParaReservas>> PedidosPendientesAsync(Guid empresaId, CancellationToken ct = default) =>
        _listar is null ? []
            : (await _listar.EjecutarAsync(empresaId, ct).ConfigureAwait(false))
                .Where(p => p.Estado is "Confirmado" or "Servido" && !p.ServidoCompleto)
                .OrderBy(p => p.Fecha).ThenBy(p => p.Numero)
                .Select(p => new PedidoParaReservas(p.Id, p.NumeroCompleto, p.ClienteId, p.ClienteNombre, true,
                    p.Lineas.Select(l => new LineaPedidoParaReservas(l.Id, l.ProductoId, l.Descripcion, l.Cantidad, l.CantidadServida)).ToList()))
                .ToList();

    public async Task<Resultado<(Guid Id, string Numero)>> EmitirAlbaranAsync(Guid empresaId, AlbaranExpedicion albaran, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(albaran);
        var pedido = await _pedido.EjecutarAsync(albaran.PedidoVentaId, ct).ConfigureAwait(false);
        if (pedido is null)
        {
            return Resultado.Fallo<(Guid, string)>(Error.NoEncontrado("pedidoventa.no_encontrado", "No se encontró el pedido de venta."));
        }

        var entregas = new List<EntregaLineaComando>();
        foreach (var (productoId, kilos, cajas) in albaran.Lineas)
        {
            var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
            var porKilos = cajas == 0 || (producto?.Unidad ?? string.Empty).Trim().ToLowerInvariant() is "kg" or "kilo" or "kilos" or "kilogramo" or "kilogramos";
            var restante = porKilos ? kilos : cajas;
            // Coste unitario de la línea: el del kilo de los palés, o el de la caja si se vende por cajas.
            decimal? coste = albaran.CosteKg is { } costes && costes.TryGetValue(productoId, out var kg)
                ? porKilos ? kg : decimal.Round(kg * kilos / cajas, 6)
                : null;
            // Bultos (cajas) y palés reales de lo expedido, repartidos entre las líneas en proporción a lo que toma cada una.
            var total = porKilos ? kilos : cajas;
            var pales = albaran.Pales is { } p && p.TryGetValue(productoId, out var n) ? n : 0;
            foreach (var linea in pedido.Lineas.Where(l => l.ProductoId == productoId && l.PendienteServir > 0m))
            {
                var toma = Math.Min(restante, linea.PendienteServir);
                var parte = total == 0m ? 0m : toma / total;
                entregas.Add(new EntregaLineaComando(linea.Id, toma, coste, decimal.Round(cajas * parte, 3), decimal.Round(pales * parte, 3)));
                restante -= toma;
                if (restante <= 0m)
                {
                    break;
                }
            }

            if (restante > 0m)
            {
                return Resultado.Fallo<(Guid, string)>(Error.Validacion("expedicion.fuera_de_pedido",
                    $"El pedido {pedido.NumeroCompleto} no tiene pendientes de servir {Redondeo.Formatear(restante, 3)} {(porKilos ? "kg" : "cajas")} de {producto?.Nombre ?? "un artículo"}."));
            }
        }

        var r = await _entregar.EjecutarAsync(empresaId, albaran.PedidoVentaId, new EntregarPedidoComando(entregas, albaran.Fecha, albaran.Referencia), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<(Guid, string)>(r.Error) : Resultado.Ok((r.Valor.Id, r.Valor.NumeroCompleto));
    }

    public async Task<Resultado<string?>> DevolverParcialAsync(Guid empresaId, Guid albaranId, IReadOnlyList<(Guid ProductoId, decimal Kilos, int Cajas)> lineas, string motivo,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (_devoluciones is null)
        {
            return Resultado.Ok<string?>(null);
        }

        var devolubles = await _devoluciones.DevolublesAsync(albaranId, ct).ConfigureAwait(false);
        if (devolubles.EsFallo)
        {
            return Resultado.Fallo<string?>(devolubles.Error);
        }

        // Lo que trae cada artículo se reparte entre sus líneas del albarán con algo por devolver, como al emitirlo.
        var comando = new List<LineaDevolucionComando>();
        foreach (var (productoId, kilos, cajas) in lineas)
        {
            var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
            var porKilos = cajas == 0 || (producto?.Unidad ?? string.Empty).Trim().ToLowerInvariant() is "kg" or "kilo" or "kilos" or "kilogramo" or "kilogramos";
            var restante = porKilos ? kilos : cajas;
            foreach (var l in devolubles.Valor.Lineas.Where(l => l.ProductoId == productoId && l.Devolvible > 0m))
            {
                var toma = Math.Min(restante, l.Devolvible);
                comando.Add(new LineaDevolucionComando(l.Orden, toma));
                restante -= toma;
                if (restante <= 0m)
                {
                    break;
                }
            }
        }

        if (comando.Count == 0)
        {
            return Resultado.Ok<string?>(null);
        }

        var r = await _devoluciones.CrearAsync(empresaId, new CrearDevolucionComando(albaranId, motivo, comando), ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<string?>(r.Error) : Resultado.Ok<string?>(r.Valor.Numero);
    }

    /// <summary>Motivo con que empiezan las devoluciones de una vuelta parcial de palés.</summary>
    public const string MotivoVuelta = "Vuelta del palé";

    public async Task<Resultado> AnularDevolucionesDeVueltaAsync(Guid empresaId, Guid albaranId, CancellationToken ct = default)
    {
        if (_devoluciones is null)
        {
            return Resultado.Ok();
        }

        foreach (var d in (await _devoluciones.ListarAsync(empresaId, new FiltroDevoluciones(AlbaranId: albaranId), ct).ConfigureAwait(false))
                     .Where(d => d.Estado != "Anulada" && d.Motivo.StartsWith(MotivoVuelta, StringComparison.Ordinal)))
        {
            var r = await _devoluciones.AnularAsync(empresaId, d.Id, "Han vuelto todos los palés: se anula el albarán", ct).ConfigureAwait(false);
            if (r.EsFallo)
            {
                return Resultado.Fallo(r.Error);
            }
        }

        return Resultado.Ok();
    }

    public async Task<Resultado<(Guid Id, string Numero)>> EmitirAlbaranDirectoAsync(Guid empresaId, Guid clienteId, DateOnly fecha, string? referencia,
        IReadOnlyList<(Guid ProductoId, decimal Cantidad, decimal? Precio)> lineas, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(lineas);
        if (_crearAlbaran is null)
        {
            return Resultado.Fallo<(Guid, string)>(Error.Validacion("expedicion.sin_albaran", "No se pueden emitir albaranes."));
        }

        var r = await _crearAlbaran.EjecutarAsync(empresaId, new CrearAlbaranVentaComando(clienteId,
            lineas.Select(l => new LineaAlbaranComando(l.Cantidad, PrecioUnitario: l.Precio, ProductoId: l.ProductoId)).ToList(), fecha, referencia), ct)
            .ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo<(Guid, string)>(r.Error) : Resultado.Ok((r.Valor.Id, r.Valor.NumeroCompleto));
    }

    public async Task<Resultado> AnularAlbaranAsync(Guid albaranId, string motivo, CancellationToken ct = default)
    {
        var r = await _anularAlbaran.EjecutarAsync(null, albaranId, motivo, ct).ConfigureAwait(false);
        return r.EsFallo ? Resultado.Fallo(r.Error) : Resultado.Ok();
    }
}
