using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Devoluciones de venta y reclamaciones, como en Hispatec: la devolución de un albarán no supera lo entregado, reingresa
/// en el almacén lo que vuelve en buen estado, se descuenta en la factura del albarán si aún no estaba facturado y, si
/// ya lo estaba, se abona con una rectificativa. La reclamación del cliente se tramita y se resuelve con la devolución.
/// </summary>
public sealed class DevolucionesReclamacionesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DevolucionesReclamacionesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Codigo);
    private sealed record ProductoResp(Guid Id, decimal Stock);
    private sealed record AlbaranResp(Guid Id, string NumeroCompleto, string Estado, Guid? FacturaId);
    private sealed record LineaFacturaResp(string Descripcion, decimal Cantidad, decimal Base);
    private sealed record FacturaResp(Guid Id, string Estado, decimal BaseImponible, Guid? RectificaFacturaId, List<LineaFacturaResp> Lineas);
    private sealed record DevolucionResp(Guid Id, string Numero, string Estado, string? FormaAbono, Guid? FacturaAbonoId, decimal Base);
    private sealed record DevolubleResp(int Orden, decimal Entregado, decimal Devuelto, decimal Devolvible);
    private sealed record DevolublesResp(List<DevolubleResp> Lineas);
    private sealed record ReclamacionResp(Guid Id, string Numero, string Estado, string? Concepto, string? Resolucion, decimal? ImporteReconocido, Guid? DevolucionId, int? DiasResolucion);
    private sealed record FilaResp(string Clave, int Numero, int Aceptadas, decimal Reclamado, decimal Reconocido);
    private sealed record InformeResp(int Total, int Abiertas, int Resueltas, decimal Reclamado, decimal Reconocido, List<FilaResp> PorConcepto);

    private static async Task<T> CreadoAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.Created, HttpStatusCode.OK], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;
    }

    private static async Task<decimal> StockAsync(HttpClient api, Guid producto) => (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock;

    [Fact]
    public async Task La_devolucion_reingresa_se_descuenta_o_se_abona_y_resuelve_la_reclamacion()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var cliente = (await CreadoAsync<IdResp>(await api.PostAsJsonAsync("/clientes", new { Nombre = "Frutas del Norte SL" }))).Id;
        var tomate = (await CreadoAsync<IdResp>(await api.PostAsJsonAsync("/productos", new { Nombre = "Tomate rama", PrecioUnitario = 1.20m, Tipo = "Bien", Unidad = "kg", ControlarStock = true, CodigoIva = "IVA4" }))).Id;
        var pepino = (await CreadoAsync<IdResp>(await api.PostAsJsonAsync("/productos", new { Nombre = "Pepino", PrecioUnitario = 0.80m, Tipo = "Bien", Unidad = "kg", ControlarStock = true, CodigoIva = "IVA4" }))).Id;
        (await api.PostAsJsonAsync($"/productos/{tomate}/stock", new { Tipo = "Entrada", Cantidad = 1000m })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/productos/{pepino}/stock", new { Tipo = "Entrada", Cantidad = 1000m })).EnsureSuccessStatusCode();

        // Albarán sin facturar: 100 kg de tomate y 50 de pepino.
        var a1 = await CreadoAsync<AlbaranResp>(await api.PostAsJsonAsync("/albaranes-venta", new
        {
            ClienteId = cliente, Lineas = new object[] { new { ProductoId = tomate, Cantidad = 100m }, new { ProductoId = pepino, Cantidad = 50m } },
        }));
        (await StockAsync(api, tomate)).Should().Be(900m);

        // Devuelve 20 kg de tomate en buen estado y 10 de pepino que se tiran; no puede devolver más de lo entregado.
        (await CodigoAsync(await api.PostAsJsonAsync("/devoluciones-venta", new { AlbaranId = a1.Id, Motivo = "Calibre", Lineas = new[] { new { OrdenAlbaran = 1, Cantidad = 101m } } })))
            .Should().Be("devolucion.excede");
        (await CodigoAsync(await api.PostAsJsonAsync("/devoluciones-venta", new { AlbaranId = a1.Id, Motivo = " ", Lineas = new[] { new { OrdenAlbaran = 1, Cantidad = 1m } } })))
            .Should().Be("devolucion.sin_motivo");
        var d1 = await CreadoAsync<DevolucionResp>(await api.PostAsJsonAsync("/devoluciones-venta", new
        {
            AlbaranId = a1.Id, Motivo = "Llegó blando",
            Lineas = new object[] { new { OrdenAlbaran = 1, Cantidad = 20m, Reingresa = true }, new { OrdenAlbaran = 2, Cantidad = 10m, Reingresa = false } },
        }));
        d1.Should().Match<DevolucionResp>(d => d.Estado == "Registrada" && d.Base == 32m && d.Numero.StartsWith("DV", StringComparison.Ordinal));
        (await StockAsync(api, tomate)).Should().Be(920m, "el tomate devuelto vuelve al almacén");
        (await StockAsync(api, pepino)).Should().Be(950m, "el pepino devuelto se tira");
        var devolubles = (await api.GetFromJsonAsync<DevolublesResp>($"/devoluciones-venta/devolubles/{a1.Id}"))!;
        devolubles.Lineas.Should().Equal(new DevolubleResp(1, 100m, 20m, 80m), new DevolubleResp(2, 50m, 10m, 40m));
        (await CodigoAsync(await api.PostAsync(new Uri($"/devoluciones-venta/{d1.Id}/abonar", UriKind.Relative), null))).Should().Be("devolucion.albaran_sin_factura");
        (await CodigoAsync(await api.PostAsJsonAsync($"/albaranes-venta/{a1.Id}/anular", new { Motivo = "x" }))).Should().Be("albaranventa.con_devoluciones");

        // La factura del albarán sale con lo devuelto descontado: 80 × 1,20 + 40 × 0,80 = 128.
        var f1 = await CreadoAsync<FacturaResp>(await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a1.Id } }));
        f1.BaseImponible.Should().Be(128m);
        f1.Lineas.Select(l => l.Cantidad).Should().Equal(80m, 40m);
        (await api.GetFromJsonAsync<DevolucionResp>($"/devoluciones-venta/{d1.Id}"))!
            .Should().Match<DevolucionResp>(d => d.Estado == "Abonada" && d.FormaAbono == "EnFacturaDelAlbaran" && d.FacturaAbonoId == f1.Id);
        (await CodigoAsync(await api.PostAsJsonAsync($"/devoluciones-venta/{d1.Id}/anular", new { Motivo = "x" }))).Should().Be("devolucion.no_anulable");

        // Reclamación sobre el albarán ya facturado: se tramita y se resuelve con una segunda devolución abonada con rectificativa.
        var calidad = (await CreadoAsync<IdResp>(await api.PostAsJsonAsync("/reclamaciones/conceptos", new { Codigo = "cal", Nombre = "Calidad" }))).Id;
        (await CodigoAsync(await api.PostAsJsonAsync("/reclamaciones/conceptos", new { Codigo = "CAL", Nombre = "Otra" }))).Should().Be("reclamacion.concepto_duplicado");
        var rec = await CreadoAsync<ReclamacionResp>(await api.PostAsJsonAsync("/reclamaciones", new
        {
            ClienteId = cliente, ConceptoId = calidad, AlbaranId = a1.Id, FacturaId = f1.Id, Descripcion = "Otros 30 kg de tomate con podredumbre", ImporteReclamado = 36m,
        }));
        rec.Should().Match<ReclamacionResp>(r => r.Estado == "Abierta" && r.Concepto == "Calidad" && r.Numero.StartsWith("RC", StringComparison.Ordinal));
        (await CreadoAsync<ReclamacionResp>(await api.PostAsJsonAsync($"/reclamaciones/{rec.Id}/tramitar", new { Responsable = "Calidad · Marta" }))).Estado.Should().Be("EnTramite");

        var d2 = await CreadoAsync<DevolucionResp>(await api.PostAsJsonAsync("/devoluciones-venta", new
        {
            AlbaranId = a1.Id, Motivo = "Podredumbre", ReclamacionId = rec.Id, Lineas = new[] { new { OrdenAlbaran = 1, Cantidad = 30m, Reingresa = false } },
        }));
        (await CodigoAsync(await api.PostAsJsonAsync("/devoluciones-venta", new { AlbaranId = a1.Id, Motivo = "x", Lineas = new[] { new { OrdenAlbaran = 1, Cantidad = 51m } } })))
            .Should().Be("devolucion.excede", "20 + 30 ya devueltos de 100");
        var abonada = await CreadoAsync<DevolucionResp>(await api.PostAsync(new Uri($"/devoluciones-venta/{d2.Id}/abonar", UriKind.Relative), null));
        abonada.Should().Match<DevolucionResp>(d => d.Estado == "Abonada" && d.FormaAbono == "Rectificativa");
        var rect = (await api.GetFromJsonAsync<FacturaResp>($"/facturas/{abonada.FacturaAbonoId}"))!;
        rect.RectificaFacturaId.Should().Be(f1.Id);
        rect.Lineas.Select(l => l.Cantidad).Should().Equal(50m, 40m);
        rect.BaseImponible.Should().Be(92m, "50 × 1,20 + 40 × 0,80");
        (await api.GetFromJsonAsync<FacturaResp>($"/facturas/{f1.Id}"))!.Estado.Should().Be("Rectificada");

        // Una tercera devolución rectifica la rectificativa vigente, no la original.
        var d3 = await CreadoAsync<DevolucionResp>(await api.PostAsJsonAsync("/devoluciones-venta", new
        {
            AlbaranId = a1.Id, Motivo = "Pepino roto", Lineas = new[] { new { OrdenAlbaran = 2, Cantidad = 5m } },
        }));
        var r3 = await CreadoAsync<DevolucionResp>(await api.PostAsync(new Uri($"/devoluciones-venta/{d3.Id}/abonar", UriKind.Relative), null));
        var rect2 = (await api.GetFromJsonAsync<FacturaResp>($"/facturas/{r3.FacturaAbonoId}"))!;
        rect2.RectificaFacturaId.Should().Be(rect.Id);
        rect2.Lineas.Select(l => l.Cantidad).Should().Equal(50m, 35m);

        var resuelta = await CreadoAsync<ReclamacionResp>(await api.PostAsJsonAsync($"/reclamaciones/{rec.Id}/resolver", new
        {
            Resolucion = "Aceptada", Texto = "Se abonan los 30 kg", ImporteReconocido = 36m, DevolucionId = d2.Id,
        }));
        resuelta.Should().Match<ReclamacionResp>(r => r.Estado == "Resuelta" && r.Resolucion == "Aceptada" && r.DevolucionId == d2.Id && r.DiasResolucion == 0);
        (await CodigoAsync(await api.PostAsJsonAsync($"/reclamaciones/{rec.Id}/resolver", new { Resolucion = "Rechazada", Texto = "x" }))).Should().Be("reclamacion.cerrada");

        // Otra reclamación rechazada, y el informe por concepto.
        var retraso = (await CreadoAsync<IdResp>(await api.PostAsJsonAsync("/reclamaciones/conceptos", new { Codigo = "RET", Nombre = "Retraso" }))).Id;
        var rec2 = await CreadoAsync<ReclamacionResp>(await api.PostAsJsonAsync("/reclamaciones", new { ClienteId = cliente, ConceptoId = retraso, Descripcion = "Llegó tarde", ImporteReclamado = 20m }));
        (await CodigoAsync(await api.PostAsJsonAsync($"/reclamaciones/{rec2.Id}/resolver", new { Resolucion = "Rechazada", Texto = "Fuera de plazo", ImporteReconocido = 5m })))
            .Should().Be("reclamacion.importe");
        await CreadoAsync<ReclamacionResp>(await api.PostAsJsonAsync($"/reclamaciones/{rec2.Id}/resolver", new { Resolucion = "Rechazada", Texto = "Fuera de plazo" }));
        var informe = (await api.GetFromJsonAsync<InformeResp>("/reclamaciones/informe"))!;
        informe.Should().Match<InformeResp>(i => i.Total == 2 && i.Resueltas == 2 && i.Abiertas == 0 && i.Reclamado == 56m && i.Reconocido == 36m);
        informe.PorConcepto.Single(f => f.Clave == "Calidad").Should().Match<FilaResp>(f => f.Numero == 1 && f.Aceptadas == 1 && f.Reconocido == 36m);

        // Un concepto dado de baja no sirve para reclamaciones nuevas.
        (await api.PostAsync(new Uri($"/reclamaciones/conceptos/{retraso}/baja", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await CodigoAsync(await api.PostAsJsonAsync("/reclamaciones", new { ClienteId = cliente, ConceptoId = retraso, Descripcion = "Otra vez" }))).Should().Be("reclamacion.concepto");

        // Una devolución no abonada se anula y lo que reingresó vuelve a salir.
        var a2 = await CreadoAsync<AlbaranResp>(await api.PostAsJsonAsync("/albaranes-venta", new { ClienteId = cliente, Lineas = new[] { new { ProductoId = tomate, Cantidad = 10m } } }));
        var d4 = await CreadoAsync<DevolucionResp>(await api.PostAsJsonAsync("/devoluciones-venta", new { AlbaranId = a2.Id, Motivo = "Error", Lineas = new[] { new { OrdenAlbaran = 1, Cantidad = 10m } } }));
        (await StockAsync(api, tomate)).Should().Be(920m);
        (await CodigoAsync(await api.PostAsJsonAsync("/albaranes-venta/facturar", new { AlbaranIds = new[] { a2.Id } }))).Should().Be("albaranventa.devuelto_entero");
        (await CreadoAsync<DevolucionResp>(await api.PostAsJsonAsync($"/devoluciones-venta/{d4.Id}/anular", new { Motivo = "Registrada por error" }))).Estado.Should().Be("Anulada");
        (await StockAsync(api, tomate)).Should().Be(910m);
    }
}
