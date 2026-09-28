using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Reservas de palés a líneas de pedidos de venta, como en Hispatec: un palé cerrado del artículo se aparta para un pedido
/// (una sola reserva activa), no se reserva más de lo pendiente, no sale con otro pedido ni sin él, la expedición del pedido
/// la consume y, si se anula, vuelve a estar activa.
/// </summary>
public sealed class ReservasPalesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ReservasPalesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado);
    private sealed record LineaPedidoResp(Guid Id);
    private sealed record PedidoResp(Guid Id, List<LineaPedidoResp> Lineas);
    private sealed record ReservaResp(Guid Id, Guid PaleId, string Estado, decimal Kilos, int Cajas);
    private sealed record LineaReservasResp(Guid LineaPedidoId, decimal Cantidad, decimal Reservado, decimal PendienteReservar, int PalesReservados);
    private sealed record DisponibleResp(Guid PaleId);
    private sealed record ReservasResp(List<LineaReservasResp> Lineas, List<ReservaResp> Reservas, List<DisponibleResp> Disponibles);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    private static async Task<PedidoResp> PedidoAsync(HttpClient api, Guid cliente, Guid producto, decimal kilos)
    {
        var p = await OkAsync<PedidoResp>(await api.PostAsJsonAsync("/pedidos-venta", new
        {
            ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Descripcion = "Tomate", Cantidad = kilos, PrecioUnitario = 1m, CodigoIva = "IVA4" } },
        }));
        (await api.PostAsync(new Uri($"/pedidos-venta/{p.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        return p;
    }

    [Fact]
    public async Task Un_pale_reservado_sale_solo_con_su_pedido()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var anio = DateTime.UtcNow.Year;
        var dia = DateOnly.FromDateTime(DateTime.Today);

        await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = dia });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = dia }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 4_500m, TaraKg = 500m })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null))).Lineas[0].PartidaId!.Value;
        var plantilla = await IdAsync(api, "/agro/plantillas-pale", new { Codigo = "EUR200", Nombre = "Europeo 200 cajas", CajasPorPale = 200, KilosPorCaja = 10m });
        var pales = await OkAsync<List<PaleResp>>(await api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla, PartidaId = partida }));
        pales.Should().HaveCount(2).And.OnlyContain(p => p.Estado == "Cerrado");

        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        var otroCliente = await IdAsync(api, "/clientes", new { Nombre = "Mercado Central SA", NifFiscal = Ayudas.GenerarNif() });
        var pedido = await PedidoAsync(api, cliente, tomate, 2_500m);
        var otroPedido = await PedidoAsync(api, otroCliente, tomate, 2_000m);
        var linea = pedido.Lineas[0].Id;

        var antes = await OkAsync<ReservasResp>(await api.GetAsync(new Uri($"/agro/reservas?pedidoVentaId={pedido.Id}", UriKind.Relative)));
        antes.Disponibles.Should().HaveCount(2);
        antes.Lineas.Single().PendienteReservar.Should().Be(2_500m);

        // Reservar el primer palé (2.000 kg); el segundo supera lo pendiente (500 kg).
        var reservas = await OkAsync<List<ReservaResp>>(await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = pedido.Id, LineaPedidoId = linea, PaleIds = new[] { pales[0].Id } }));
        reservas.Single().Should().Match<ReservaResp>(x => x.Estado == "Activa" && x.Kilos == 2_000m && x.Cajas == 200);
        (await CodigoAsync(await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = pedido.Id, LineaPedidoId = linea, PaleIds = new[] { pales[1].Id } })))
            .Should().Be("reserva.supera_pendiente");
        (await CodigoAsync(await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = otroPedido.Id, LineaPedidoId = otroPedido.Lineas[0].Id, PaleIds = new[] { pales[0].Id } })))
            .Should().Be("reserva.pale_reservado");
        var tras = await OkAsync<ReservasResp>(await api.GetAsync(new Uri($"/agro/reservas?pedidoVentaId={pedido.Id}", UriKind.Relative)));
        tras.Lineas.Single().Should().Match<LineaReservasResp>(l => l.Reservado == 2_000m && l.PendienteReservar == 500m && l.PalesReservados == 1);
        tras.Disponibles.Select(d => d.PaleId).Should().Equal(pales[1].Id);

        // Reservado, no sale sin pedido ni con otro pedido.
        (await CodigoAsync(await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pales[0].Id }, ClienteId = otroCliente }))).Should().Be("pale.reservado");
        (await CodigoAsync(await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pales[0].Id }, PedidoVentaId = otroPedido.Id }))).Should().Be("pale.reservado");

        // Con su pedido sale y la reserva queda consumida; anulada la expedición, vuelve a estar activa.
        (await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pales[0].Id }, PedidoVentaId = pedido.Id })).EnsureSuccessStatusCode();
        (await OkAsync<ReservasResp>(await api.GetAsync(new Uri($"/agro/reservas?pedidoVentaId={pedido.Id}", UriKind.Relative)))).Reservas.Single().Estado.Should().Be("Consumida");
        (await api.PostAsync(new Uri($"/agro/pales/{pales[0].Id}/anular-expedicion", UriKind.Relative), null)).EnsureSuccessStatusCode();
        var reactivada = (await OkAsync<ReservasResp>(await api.GetAsync(new Uri($"/agro/reservas?pedidoVentaId={pedido.Id}", UriKind.Relative)))).Reservas.Single();
        reactivada.Estado.Should().Be("Activa");

        // Anulada la reserva, el palé queda libre para el otro pedido.
        (await api.PostAsJsonAsync($"/agro/reservas/{reactivada.Id}/anular", new { Motivo = "El cliente lo quiere mañana" })).EnsureSuccessStatusCode();
        (await OkAsync<List<ReservaResp>>(await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = otroPedido.Id, LineaPedidoId = otroPedido.Lineas[0].Id, PaleIds = new[] { pales[0].Id } })))
            .Single().Estado.Should().Be("Activa");
        (await api.GetFromJsonAsync<List<ReservaResp>>("/agro/reservas/activas"))!.Should().ContainSingle(x => x.PaleId == pales[0].Id);
    }
}
