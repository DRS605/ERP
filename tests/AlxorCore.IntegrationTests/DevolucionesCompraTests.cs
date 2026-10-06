using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Devoluciones a proveedor: salen del almacén, se descuentan de la factura del pedido si aún no estaba facturado y, si
/// lo estaba, se abonan con una factura rectificativa recibida que se aplica a la factura pendiente.
/// </summary>
public sealed class DevolucionesCompraTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DevolucionesCompraTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaPedResp(Guid Id);
    private sealed record PedidoResp(Guid Id, string Estado, decimal Total, List<LineaPedResp> Lineas);
    private sealed record ExistenciaResp(Guid AlmacenId, decimal Cantidad);
    private sealed record DevolubleResp(Guid LineaPedidoId, decimal Recibido, decimal Devuelto, decimal Devolvible, decimal PrecioUnitario);
    private sealed record DevolucionResp(Guid Id, string Numero, string Estado, decimal Base, Guid? GastoAbonoId);
    private sealed record GastoResp(Guid Id, string Concepto, decimal BaseImponible, decimal CuotaIva, decimal Total, string? NumeroFactura, bool EsRectificativa,
        Guid? RectificaGastoId, string? NumeroRectificado);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(string Origen, List<ApunteResp> Apuntes);
    private sealed record AplicacionResp(Guid GastoId, decimal Importe, decimal PendienteAbono, decimal PendienteFactura);

    private static async Task<T> OkAsync<T>(Task<HttpResponseMessage> peticion)
    {
        var r = await peticion;
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> FalloAsync(Task<HttpResponseMessage> peticion, HttpStatusCode esperado)
    {
        var r = await peticion;
        r.StatusCode.Should().Be(esperado, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static async Task<PedidoResp> PedidoRecibidoAsync(HttpClient api, Guid proveedor, Guid producto, Guid almacen, decimal cantidad, decimal precio)
    {
        var p = await OkAsync<PedidoResp>(api.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = proveedor, Lineas = new[] { new { ProductoId = producto, Descripcion = "Caja de herramientas", Cantidad = cantidad, PrecioUnitario = precio } },
        }));
        (await api.PostAsync(new Uri($"/compras/pedidos/{p.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{p.Id}/recibir", new { AlmacenId = almacen, Lineas = new[] { new { LineaPedidoId = p.Lineas[0].Id, Cantidad = cantidad } } }))
            .EnsureSuccessStatusCode();
        return p;
    }

    private static async Task<decimal> StockAsync(HttpClient api, Guid producto) =>
        (await OkAsync<List<ExistenciaResp>>(api.GetAsync($"/inventario/stock/producto/{producto}"))).Sum(e => e.Cantidad);

    [Fact]
    public async Task Devolucion_antes_y_despues_de_facturar_y_abono_aplicado_a_la_factura()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync("/contabilidad/contabilizacion-automatica", new { Automatica = true })).EnsureSuccessStatusCode();
        var proveedor = (await OkAsync<IdResp>(api.PostAsJsonAsync("/proveedores", new { Nombre = "Ferretería Industrial SL", NifFiscal = Ayudas.GenerarNif() }))).Id;
        var producto = (await OkAsync<IdResp>(api.PostAsJsonAsync("/productos", new { Nombre = "Caja de herramientas", PrecioUnitario = 40m, CodigoIva = "IVA21" }))).Id;
        var almacen = (await OkAsync<IdResp>(api.PostAsJsonAsync("/inventario/almacenes", new { Codigo = "C", Nombre = "Central" }))).Id;

        // Pedido 1 sin facturar: se devuelven 3 de 10.
        var p1 = await PedidoRecibidoAsync(api, proveedor, producto, almacen, 10m, 20m);
        var d1 = await OkAsync<DevolucionResp>(api.PostAsJsonAsync("/compras/devoluciones", new
        {
            PedidoId = p1.Id, AlmacenId = almacen, Motivo = "Llegaron golpeadas", Lineas = new[] { new { LineaPedidoId = p1.Lineas[0].Id, Cantidad = 3m } },
        }));
        d1.Should().Match<DevolucionResp>(d => d.Estado == "Registrada" && d.Base == 60m && d.Numero.StartsWith("DVC-"));
        (await StockAsync(api, producto)).Should().Be(7m);
        (await OkAsync<List<DevolubleResp>>(api.GetAsync($"/compras/devoluciones/devolubles/{p1.Id}"))).Single()
            .Should().Be(new DevolubleResp(p1.Lineas[0].Id, 10m, 3m, 7m, 20m));
        (await FalloAsync(api.PostAsJsonAsync("/compras/devoluciones", new { PedidoId = p1.Id, Lineas = new[] { new { LineaPedidoId = p1.Lineas[0].Id, Cantidad = 8m } } }),
            HttpStatusCode.BadRequest)).Should().Be("devolucion_compra.excede");
        (await FalloAsync(api.PostAsJsonAsync($"/compras/devoluciones/{d1.Id}/abonar", new { NumeroAbono = "X" }), HttpStatusCode.Conflict))
            .Should().Be("devolucion_compra.pedido_sin_facturar");

        // Al facturarlo, la factura sale sin lo devuelto.
        (await api.PostAsJsonAsync($"/compras/pedidos/{p1.Id}/facturar", new { CodigoIva = "IVA21", PorcentajeIrpf = 0m, NumeroFactura = "F-100" })).EnsureSuccessStatusCode();
        var gastos = await OkAsync<List<GastoResp>>(api.GetAsync("/gastos"));
        gastos.Single(g => g.NumeroFactura == "F-100").BaseImponible.Should().Be(140m);
        (await OkAsync<DevolucionResp>(api.GetAsync($"/compras/devoluciones/{d1.Id}"))).Estado.Should().Be("EnFactura");

        // Pedido 2 ya facturado: la devolución se abona con una rectificativa en negativo.
        var p2 = await PedidoRecibidoAsync(api, proveedor, producto, almacen, 5m, 100m);
        (await api.PostAsJsonAsync($"/compras/pedidos/{p2.Id}/facturar", new { CodigoIva = "IVA21", PorcentajeIrpf = 0m, NumeroFactura = "F-200", FechaFactura = DateOnly.FromDateTime(DateTime.UtcNow) }))
            .EnsureSuccessStatusCode();
        var factura2 = (await OkAsync<List<GastoResp>>(api.GetAsync("/gastos"))).Single(g => g.NumeroFactura == "F-200");
        factura2.Total.Should().Be(605m);
        var d2 = await OkAsync<DevolucionResp>(api.PostAsJsonAsync("/compras/devoluciones", new
        {
            PedidoId = p2.Id, AlmacenId = almacen, Motivo = "Modelo equivocado", Lineas = new[] { new { LineaPedidoId = p2.Lineas[0].Id, Cantidad = 2m } },
        }));
        (await StockAsync(api, producto)).Should().Be(10m, "7 + 5 recibidas − 2 devueltas");
        var abonada = await OkAsync<DevolucionResp>(api.PostAsJsonAsync($"/compras/devoluciones/{d2.Id}/abonar", new { NumeroAbono = "AB-7" }));
        abonada.Estado.Should().Be("Abonada");
        var abono = (await OkAsync<List<GastoResp>>(api.GetAsync("/gastos"))).Single(g => g.Id == abonada.GastoAbonoId);
        abono.Should().Match<GastoResp>(g => g.BaseImponible == -200m && g.CuotaIva == -42m && g.Total == -242m && g.EsRectificativa
            && g.RectificaGastoId == factura2.Id && g.NumeroRectificado == "F-200" && g.NumeroFactura == "AB-7");
        (await FalloAsync(api.PostAsync(new Uri($"/compras/devoluciones/{d2.Id}/anular", UriKind.Relative), null), HttpStatusCode.Conflict))
            .Should().Be("devolucion_compra.no_anulable");

        // El abono se aplica a la factura que rectifica: ya solo se debe la diferencia.
        var aplicado = await OkAsync<AplicacionResp>(api.PostAsJsonAsync($"/pagos/abonos/{abono.Id}/aplicar", new { }));
        aplicado.Should().Be(new AplicacionResp(factura2.Id, 242m, 0m, 363m));
        (await FalloAsync(api.PostAsJsonAsync($"/pagos/abonos/{abono.Id}/aplicar", new { }), HttpStatusCode.Conflict)).Should().Be("abono.aplicado");
        (await FalloAsync(api.PostAsJsonAsync($"/pagos/abonos/{factura2.Id}/aplicar", new { }), HttpStatusCode.BadRequest)).Should().Be("abono.no_es_abono");

        // En contabilidad: la 555 queda a cero y al proveedor se le deben las dos facturas menos el abono.
        var diario = await OkAsync<List<AsientoResp>>(api.GetAsync($"/contabilidad/diario?ejercicio={DateTime.UtcNow.Year}"));
        decimal Saldo(string prefijo) => diario.SelectMany(a => a.Apuntes).Where(x => x.CuentaCodigo.StartsWith(prefijo, StringComparison.Ordinal)).Sum(x => x.Debe - x.Haber);
        Saldo("555").Should().Be(0m);
        Saldo("400").Should().Be(-(169.40m + 605m - 242m));
        Saldo("472").Should().Be(29.40m + 105m - 42m);

        // Otra devolución que al final no sale: se anula y la mercancía vuelve al almacén. Otra, sin abono (el proveedor repone).
        var d3 = await OkAsync<DevolucionResp>(api.PostAsJsonAsync("/compras/devoluciones", new
        {
            PedidoId = p2.Id, AlmacenId = almacen, Lineas = new[] { new { LineaPedidoId = p2.Lineas[0].Id, Cantidad = 1m } },
        }));
        (await StockAsync(api, producto)).Should().Be(9m);
        (await api.PostAsync(new Uri($"/compras/devoluciones/{d3.Id}/anular", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await StockAsync(api, producto)).Should().Be(10m);
        var d4 = await OkAsync<DevolucionResp>(api.PostAsJsonAsync("/compras/devoluciones", new { PedidoId = p2.Id, Lineas = new[] { new { LineaPedidoId = p2.Lineas[0].Id, Cantidad = 1m } } }));
        (await OkAsync<DevolucionResp>(api.PostAsync(new Uri($"/compras/devoluciones/{d4.Id}/cerrar-sin-abono", UriKind.Relative), null))).Estado.Should().Be("SinAbono");
        (await StockAsync(api, producto)).Should().Be(10m, "sin almacén indicado, la devolución no mueve stock");
        (await OkAsync<List<DevolucionResp>>(api.GetAsync("/compras/devoluciones"))).Should().HaveCount(4);

        // Sin existencias que devolver, no se registra.
        (await FalloAsync(api.PostAsJsonAsync("/inventario/ajuste", new { ProductoId = producto, AlmacenId = almacen, Cantidad = 0m }).ContinueWith(_ =>
            api.PostAsJsonAsync("/compras/devoluciones", new { PedidoId = p2.Id, AlmacenId = almacen, Lineas = new[] { new { LineaPedidoId = p2.Lineas[0].Id, Cantidad = 1m } } })).Unwrap(),
            HttpStatusCode.Conflict)).Should().Be("devolucion_compra.sin_existencias");
    }
}
