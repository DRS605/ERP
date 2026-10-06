using AlxorCore.Catalogo.Aplicacion;
using AlxorCore.Compras.Aplicacion;
using AlxorCore.Inventario.Aplicacion;
using AlxorCore.Nucleo.Comun;
using AlxorCore.Nucleo.Resultados;
using AlxorCore.Produccion.Aplicacion;
using AlxorCore.Terceros.Aplicacion;

namespace AlxorCore.Api.Comun;

public sealed record LineaPropuestaCompraDto(Guid ProductoId, string Producto, Guid? AlmacenId, string Motivo, decimal Stock, decimal PendienteRecibir,
    decimal NecesidadFabricacion, decimal Disponible, decimal? Minimo, decimal? Maximo, decimal CantidadBase, string Unidad, decimal CantidadCompra, string UnidadCompra,
    Guid? ProveedorId, string? Proveedor, decimal PrecioCompra, decimal Importe);

public sealed record PropuestaCompraDto(IReadOnlyList<LineaPropuestaCompraDto> Lineas, decimal Importe, int SinProveedor);

public sealed record LineaGenerarPedido(Guid ProductoId, Guid? ProveedorId, decimal Cantidad, decimal? PrecioUnitario = null);

public sealed record DatosGenerarPedidos(IReadOnlyList<LineaGenerarPedido>? Lineas);

public sealed record PedidosGeneradosDto(IReadOnlyList<PedidoDto> Pedidos, IReadOnlyList<string> Omitidas);

/// <summary>
/// Propuesta de compra: para cada artículo con regla de reaprovisionamiento, lo disponible (existencias + pendiente de
/// recibir de los pedidos de compra, también los borradores, − lo que necesitan las órdenes de fabricación abiertas)
/// frente a su mínimo; si baja, se propone reponer hasta el máximo. Los componentes que faltan para fabricar se proponen
/// aunque no tengan regla (cálculo de necesidades). La propuesta se convierte en pedidos en borrador, uno por proveedor.
/// </summary>
public sealed class PropuestaCompra
{
    private readonly IRepositorioReglasReaprovisionamiento _reglas;
    private readonly IRepositorioExistencias _existencias;
    private readonly IConsultaProductos _productos;
    private readonly IConsultaProveedores _proveedores;
    private readonly IRepositorioPedidos _pedidos;
    private readonly ListarOrdenes _ordenes;
    private readonly CrearPedido _crearPedido;

    public PropuestaCompra(IRepositorioReglasReaprovisionamiento reglas, IRepositorioExistencias existencias, IConsultaProductos productos, IConsultaProveedores proveedores,
        IRepositorioPedidos pedidos, ListarOrdenes ordenes, CrearPedido crearPedido)
    {
        _reglas = reglas; _existencias = existencias; _productos = productos; _proveedores = proveedores; _pedidos = pedidos; _ordenes = ordenes; _crearPedido = crearPedido;
    }

    public async Task<PropuestaCompraDto> CalcularAsync(Guid empresaId, CancellationToken ct = default)
    {
        var reglas = (await _reglas.ListarAsync(empresaId, ct).ConfigureAwait(false)).Where(r => r.Activa).ToList();

        // Pendiente de recibir por artículo (en unidad de compra): borradores y confirmados sin recibir del todo.
        var pendienteCompra = (await _pedidos.ListarAsync(empresaId, ct).ConfigureAwait(false))
            .Where(p => p.Estado is "Borrador" or "Confirmado" or "Recibido")
            .SelectMany(p => p.Lineas).Where(l => l.ProductoId is not null && l.PendienteRecibir > 0m)
            .GroupBy(l => l.ProductoId!.Value).ToDictionary(g => g.Key, g => g.Sum(l => l.PendienteRecibir));

        // Necesidades de fabricación: componentes de las órdenes planificadas o en curso, por almacén.
        var necesidades = (await _ordenes.EjecutarAsync(empresaId, ct).ConfigureAwait(false)).Where(o => o.Estado is "Planificada" or "EnCurso")
            .SelectMany(o => o.Componentes.Select(c => (c.ComponenteId, o.AlmacenId, c.CantidadTotal))).ToList();

        var productos = reglas.Select(r => r.ProductoId).Concat(necesidades.Select(n => n.ComponenteId)).Distinct().ToList();
        var lineas = new List<LineaPropuestaCompraDto>();
        var proveedores = new Dictionary<Guid, string?>();
        foreach (var productoId in productos)
        {
            var producto = await _productos.ObtenerAsync(productoId, ct).ConfigureAwait(false);
            if (producto is null || !producto.Activo)
            {
                continue;
            }

            var factor = producto.FactorCompra > 0m ? producto.FactorCompra : 1m;
            var existencias = await _existencias.ListarPorProductoAsync(empresaId, productoId, ct).ConfigureAwait(false);
            var pendienteBase = Math.Round(pendienteCompra.GetValueOrDefault(productoId) * factor, 3, MidpointRounding.AwayFromZero);
            var reglasProducto = reglas.Where(r => r.ProductoId == productoId).ToList();
            if (reglasProducto.Count == 0)
            {
                // Sin regla: solo lo que falta para fabricar.
                var stock = existencias.Sum(e => e.Cantidad);
                var necesidad = necesidades.Where(n => n.ComponenteId == productoId).Sum(n => n.CantidadTotal);
                var disponible = stock + pendienteBase - necesidad;
                if (disponible < 0m)
                {
                    lineas.Add(await LineaAsync(producto, null, "Fabricación", stock, pendienteBase, necesidad, disponible, null, null, -disponible, null, proveedores, ct)
                        .ConfigureAwait(false));
                }

                continue;
            }

            // Lo pendiente de recibir no lleva almacén: se cuenta en la regla general o, si no la hay, en la primera.
            var reglaPendiente = reglasProducto.FirstOrDefault(r => r.AlmacenId is null) ?? reglasProducto[0];
            foreach (var r in reglasProducto)
            {
                var stock = existencias.Where(e => r.AlmacenId is null || e.AlmacenId == r.AlmacenId).Sum(e => e.Cantidad);
                var necesidad = necesidades.Where(n => n.ComponenteId == productoId && (r.AlmacenId is null || n.AlmacenId == r.AlmacenId)).Sum(n => n.CantidadTotal);
                var pendiente = ReferenceEquals(r, reglaPendiente) ? pendienteBase : 0m;
                var disponible = stock + pendiente - necesidad;
                var cantidad = r.APedir(disponible);
                if (cantidad > 0m)
                {
                    lineas.Add(await LineaAsync(producto, r.AlmacenId, necesidad > 0m && stock + pendiente >= r.Minimo ? "Fabricación" : "Bajo mínimo", stock, pendiente, necesidad,
                        disponible, r.Minimo, r.Maximo, cantidad, r.ProveedorId, proveedores, ct).ConfigureAwait(false));
                }
            }
        }

        var ordenadas = lineas.OrderBy(l => l.Proveedor ?? "~", StringComparer.CurrentCulture).ThenBy(l => l.Producto, StringComparer.CurrentCulture).ToList();
        return new PropuestaCompraDto(ordenadas, Redondeo.Dos(ordenadas.Sum(l => l.Importe)), ordenadas.Count(l => l.ProveedorId is null));
    }

    /// <summary>Pedidos de compra en borrador con las líneas elegidas, uno por proveedor.</summary>
    public async Task<Resultado<PedidosGeneradosDto>> GenerarPedidosAsync(Guid empresaId, DatosGenerarPedidos d, CancellationToken ct = default)
    {
        if (d?.Lineas is not { Count: > 0 } lineas)
        {
            return Resultado.Fallo<PedidosGeneradosDto>(Error.Validacion("propuesta.sin_lineas", "Elige qué comprar."));
        }

        var omitidas = new List<string>();
        var pedidos = new List<PedidoDto>();
        foreach (var grupo in lineas.Where(l => l.Cantidad > 0m).GroupBy(l => l.ProveedorId))
        {
            var detalle = new List<LineaPedidoComando>();
            foreach (var l in grupo)
            {
                var producto = await _productos.ObtenerAsync(l.ProductoId, ct).ConfigureAwait(false);
                if (producto is null)
                {
                    omitidas.Add($"{l.ProductoId}: el artículo no existe.");
                    continue;
                }

                if (grupo.Key is null)
                {
                    omitidas.Add($"{producto.Nombre}: sin proveedor (ponle proveedor habitual o en su regla).");
                    continue;
                }

                detalle.Add(new LineaPedidoComando(producto.Nombre, l.Cantidad, l.PrecioUnitario ?? producto.PrecioCompraPorUnidadCompra, producto.Id));
            }

            if (grupo.Key is { } proveedorId && detalle.Count > 0)
            {
                var p = await _crearPedido.EjecutarAsync(empresaId, new CrearPedidoComando(null, detalle, proveedorId), ct).ConfigureAwait(false);
                if (p.EsFallo)
                {
                    omitidas.Add($"Pedido al proveedor {proveedorId}: {p.Error.Mensaje}");
                }
                else
                {
                    pedidos.Add(p.Valor);
                }
            }
        }

        return pedidos.Count == 0
            ? Resultado.Fallo<PedidosGeneradosDto>(Error.Validacion("propuesta.sin_pedidos", omitidas.Count > 0 ? string.Join(" ", omitidas) : "No había nada que pedir."))
            : Resultado.Ok(new PedidosGeneradosDto(pedidos, omitidas));
    }

    private async Task<LineaPropuestaCompraDto> LineaAsync(ProductoDto producto, Guid? almacenId, string motivo, decimal stock, decimal pendiente, decimal necesidad,
        decimal disponible, decimal? minimo, decimal? maximo, decimal cantidadBase, Guid? proveedorRegla, Dictionary<Guid, string?> proveedores, CancellationToken ct)
    {
        var factor = producto.FactorCompra > 0m ? producto.FactorCompra : 1m;
        var cantidadCompra = Math.Ceiling(Math.Round(cantidadBase / factor, 6));
        var proveedorId = proveedorRegla ?? producto.ProveedorHabitualId;
        string? proveedor = null;
        if (proveedorId is { } p && !proveedores.TryGetValue(p, out proveedor))
        {
            proveedor = (await _proveedores.ObtenerAsync(p, ct).ConfigureAwait(false))?.Nombre;
            proveedores[p] = proveedor;
        }

        var precio = producto.PrecioCompraPorUnidadCompra;
        return new LineaPropuestaCompraDto(producto.Id, producto.Nombre, almacenId, motivo, stock, pendiente, necesidad, disponible, minimo, maximo, cantidadBase,
            producto.Unidad, cantidadCompra, string.IsNullOrWhiteSpace(producto.UnidadCompra) ? producto.Unidad : producto.UnidadCompra!, proveedorId, proveedor, precio,
            Redondeo.Dos(cantidadCompra * precio));
    }
}
