using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Orden de carga ligera, como en Hispatec: se monta con líneas de pedidos en firme pendientes de servir, se cargan
/// palés validados contra la línea (artículo, previstos) y su reserva, y al finalizar se expide un albarán por pedido.
/// </summary>
public sealed class OrdenesCargaTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public OrdenesCargaTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record PaleResp(Guid Id, string Sscc, string Estado, Guid? AlbaranId);
    private sealed record LineaPedidoResp(Guid Id);
    private sealed record PedidoResp(Guid Id, List<LineaPedidoResp> Lineas);
    private sealed record PendienteResp(Guid PedidoVentaId, Guid LineaPedidoId, decimal Pendiente, int PalesReservados, int PalesEnOrdenes);
    private sealed record LineaOrdenResp(Guid Id, int Orden, Guid PedidoVentaId, int PalesPrevistos, int PalesCargados, decimal KilosCargados, int? Fila, int? Columna);
    private sealed record CargadoResp(Guid PaleId, string Sscc, Guid LineaId, int? Fila, int? Columna, Guid? AlbaranId);
    private sealed record OrdenResp(Guid Id, string Numero, string Estado, List<LineaOrdenResp> Lineas, List<CargadoResp> Cargados, List<Guid> Albaranes);
    private sealed record FinalizacionResp(OrdenResp Orden, List<string> Errores);

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

    private static async Task<string> CodigoAsync(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

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
    public async Task La_orden_carga_palés_validados_y_al_finalizar_expide_un_albaran_por_pedido()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var anio = DateTime.UtcNow.Year;
        var dia = DateOnly.FromDateTime(DateTime.Today);

        // Cinco palés de 2.000 kg de tomate.
        await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var pepino = await IdAsync(api, "/productos", new { Nombre = "Pepino", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = dia });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = dia }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 10_500m, TaraKg = 500m })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null))).Lineas[0].PartidaId!.Value;
        var plantilla = await IdAsync(api, "/agro/plantillas-pale", new { Codigo = "EUR200", Nombre = "Europeo 200 cajas", CajasPorPale = 200, KilosPorCaja = 10m });
        var pales = await OkAsync<List<PaleResp>>(await api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla, PartidaId = partida }));
        pales.Should().HaveCount(5).And.OnlyContain(p => p.Estado == "Cerrado");

        // Tres pedidos: A (4.000 kg), B (2.000 kg) y C (2.000 kg, que no irá en la orden); un palé reservado a C.
        var norte = await IdAsync(api, "/clientes", new { Nombre = "Frutas del Norte SA", NifFiscal = Ayudas.GenerarNif() });
        var sur = await IdAsync(api, "/clientes", new { Nombre = "Mercado Sur SL", NifFiscal = Ayudas.GenerarNif() });
        var a = await PedidoAsync(api, norte, tomate, 4_000m);
        var b = await PedidoAsync(api, sur, tomate, 2_000m);
        var c = await PedidoAsync(api, sur, tomate, 2_000m);
        var d = await PedidoAsync(api, sur, pepino, 500m);
        (await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = c.Id, LineaPedidoId = c.Lineas[0].Id, PaleIds = new[] { pales[4].Id } })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/agro/reservas", new { PedidoVentaId = a.Id, LineaPedidoId = a.Lineas[0].Id, PaleIds = new[] { pales[0].Id } })).EnsureSuccessStatusCode();

        var pendientes = (await api.GetFromJsonAsync<List<PendienteResp>>("/agro/ordenes-carga/pendientes"))!;
        pendientes.Should().ContainSingle(p => p.LineaPedidoId == a.Lineas[0].Id && p.Pendiente == 4_000m && p.PalesReservados == 1);

        // Orden en propuesta con camión de 2 × 2: no se carga hasta liberarla.
        var orden = await OkAsync<OrdenResp>(await api.PostAsJsonAsync("/agro/ordenes-carga", new
        {
            FechaCarga = dia, Muelle = "M2", Matricula = "1234-KLM", Conductor = "Pedro", TemperaturaConsigna = 8m, Filas = 2, Columnas = 2, Propuesta = true,
        }));
        orden.Should().Match<OrdenResp>(o => o.Estado == "Propuesta" && o.Numero == $"OC-{anio}-000001");
        var o1 = await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/lineas", new { PedidoVentaId = a.Id, LineaPedidoId = a.Lineas[0].Id, Pales = 2, Fila = 1, Columna = 1 }));
        await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/lineas", new { PedidoVentaId = b.Id, LineaPedidoId = b.Lineas[0].Id, Pales = 1 }));
        await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/lineas", new { PedidoVentaId = d.Id, LineaPedidoId = d.Lineas[0].Id, Pales = 1 }));
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/lineas", new { PedidoVentaId = b.Id, LineaPedidoId = b.Lineas[0].Id, Pales = 1 })))
            .Should().Be("ordencarga.linea_repetida");
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/lineas", new { PedidoVentaId = c.Id, LineaPedidoId = c.Lineas[0].Id, Pales = 1, Fila = 3, Columna = 1 })))
            .Should().Be("ordencarga.posicion");
        (await api.GetFromJsonAsync<List<PendienteResp>>("/agro/ordenes-carga/pendientes"))!.Single(p => p.LineaPedidoId == a.Lineas[0].Id).PalesEnOrdenes.Should().Be(2);
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[0].Sscc })))
            .Should().Be("ordencarga.no_cargable");
        await OkAsync<OrdenResp>(await api.PostAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/liberar", UriKind.Relative), null));

        // El reservado a A va a su línea solo; el reservado a C no entra; uno libre vale para A y B: hay que decir cuál.
        var cargada = await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[0].Sscc }));
        cargada.Estado.Should().Be("EnCarga");
        cargada.Cargados.Single().Should().Match<CargadoResp>(x => x.LineaId == o1.Lineas[0].Id && x.Fila == 1 && x.Columna == 1);
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[4].Sscc }))).Should().Be("ordencarga.pale_reservado");
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[1].Sscc }))).Should().Be("ordencarga.elige_linea");
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[1].Sscc, LineaId = cargada.Lineas[2].Id })))
            .Should().Be("ordencarga.pale_otro_articulo", "la línea del pepino");
        await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[1].Sscc, LineaId = cargada.Lineas[0].Id, Fila = 1, Columna = 2 }));
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[2].Sscc, LineaId = cargada.Lineas[0].Id })))
            .Should().Be("ordencarga.linea_completa");
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[1].Sscc, LineaId = cargada.Lineas[0].Id })))
            .Should().Be("ordencarga.pale_cargado");
        var tercera = await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/cargar", new { Pale = pales[2].Sscc, LineaId = cargada.Lineas[1].Id, Fila = 2, Columna = 1 }));
        tercera.Lineas.Select(l => l.PalesCargados).Should().Equal(2, 1, 0);

        // Otra orden no puede cargar un palé que ya está en esta; la línea con palés no se quita, la de pepino sí.
        var otra = await OkAsync<OrdenResp>(await api.PostAsJsonAsync("/agro/ordenes-carga", new { FechaCarga = dia }));
        await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{otra.Id}/lineas", new { PedidoVentaId = b.Id, LineaPedidoId = b.Lineas[0].Id, Pales = 1 }));
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{otra.Id}/cargar", new { Pale = pales[2].Sscc }))).Should().Be("ordencarga.pale_en_otra");
        (await CodigoAsync(await api.DeleteAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/lineas/{cargada.Lineas[0].Id}", UriKind.Relative)))).Should().Be("ordencarga.linea_cargada");
        (await OkAsync<OrdenResp>(await api.DeleteAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/lineas/{cargada.Lineas[2].Id}", UriKind.Relative)))).Lineas.Should().HaveCount(2);

        // Hoja de carga en PDF.
        var pdf = await api.GetAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/pdf", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK, await pdf.Content.ReadAsStringAsync());
        System.Text.Encoding.ASCII.GetString((await pdf.Content.ReadAsByteArrayAsync())[..4]).Should().Be("%PDF");

        // Finalizar: un albarán por pedido (A con 2 palés, B con 1); los palés quedan expedidos.
        var fin = await OkAsync<FinalizacionResp>(await api.PostAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/finalizar", UriKind.Relative), null));
        fin.Errores.Should().BeEmpty();
        fin.Orden.Should().Match<OrdenResp>(o => o.Estado == "Finalizada" && o.Albaranes.Count == 2 && o.Cargados.All(x => x.AlbaranId != null));
        var expedidos = (await api.GetFromJsonAsync<List<PaleResp>>("/agro/pales?estado=Expedido"))!;
        expedidos.Select(p => p.Id).Should().BeEquivalentTo([pales[0].Id, pales[1].Id, pales[2].Id]);
        (await CodigoAsync(await api.PostAsync(new Uri($"/agro/ordenes-carga/{orden.Id}/finalizar", UriKind.Relative), null))).Should().Be("ordencarga.sin_carga");
        (await CodigoAsync(await api.PostAsJsonAsync($"/agro/ordenes-carga/{orden.Id}/anular", new { Motivo = "x" }))).Should().Be("ordencarga.cerrada");
        (await api.GetFromJsonAsync<List<PendienteResp>>("/agro/ordenes-carga/pendientes"))!.Should().NotContain(p => p.PedidoVentaId == b.Id, "B ya está servido");

        // La otra orden se anula.
        (await OkAsync<OrdenResp>(await api.PostAsJsonAsync($"/agro/ordenes-carga/{otra.Id}/anular", new { Motivo = "No viene el camión" }))).Estado.Should().Be("Anulada");
    }
}
