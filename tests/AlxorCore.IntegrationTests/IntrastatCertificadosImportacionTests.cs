using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Intrastat (expediciones e introducciones del mes), certificados fitosanitarios de las expediciones y DUA de
/// importación de las compras de fuera de la UE.
/// </summary>
public sealed class IntrastatCertificadosImportacionTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public IntrastatCertificadosImportacionTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record LineaPedResp(Guid Id);
    private sealed record PedidoResp(Guid Id, List<LineaPedResp> Lineas);
    private sealed record LineaIntrastatResp(string EstadoMiembro, string? CondicionesEntrega, string NaturalezaTransaccion, string ModalidadTransporte, string? CodigoMercancias,
        string? PaisOrigen, decimal? MasaNetaKg, decimal ImporteFacturado, string? NifContraparte);
    private sealed record IntrastatResp(string Flujo, List<LineaIntrastatResp> Lineas, decimal TotalImporte, decimal AcumuladoAnual, bool SuperaUmbral, List<string> Avisos);
    private sealed record CertificadoResp(Guid Id, string Tipo, string Numero, string? PaisDestino, bool TieneDocumento);
    private sealed record CartaResp(Guid Id, List<string>? Certificados);
    private sealed record DuaResp(Guid Id, string Mrn, decimal CuotaIva);
    private sealed record ImportacionResp(Guid GastoId, string? Pais, string Situacion, decimal CuotaIvaImportacion, List<DuaResp> Duas);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    [Fact]
    public async Task Intrastat_recoge_las_entregas_intracomunitarias_y_las_llegadas_de_proveedores_de_la_ue()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode();
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 1.2m, Tipo = "Bien", Unidad = "kg", CodigoArancelario = "0805102200", PaisOrigen = "ES" });
        var sinCodigo = await IdAsync(api, "/productos", new { Nombre = "Cajas", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "ud" });
        var aleman = await IdAsync(api, "/clientes", new { Nombre = "Obst GmbH", Pais = "DE", NifIva = "DE123456789", Incoterm = "DAP", LugarIncoterm = "Hamburg" });
        var frances = await IdAsync(api, "/clientes", new { Nombre = "Fruits SARL", Pais = "FR" });
        foreach (var (cliente, producto, cantidad) in new[] { (aleman, naranja, 1_000m), (aleman, naranja, 500m), (frances, sinCodigo, 10m) })
        {
            var f = await api.PostAsJsonAsync("/facturas", new
            {
                ClienteId = cliente, Lineas = new[] { new { ProductoId = producto, Descripcion = "Mercancía", Cantidad = cantidad, PrecioUnitario = 1.2m, CodigoIva = "INTRA" } },
            });
            f.IsSuccessStatusCode.Should().BeTrue(await f.Content.ReadAsStringAsync());
        }

        var hoy = DateTime.UtcNow;
        var exp = (await api.GetFromJsonAsync<IntrastatResp>($"/aduanas/intrastat?anio={hoy.Year}&mes={hoy.Month}&flujo=Expedicion"))!;
        exp.Lineas.Single(l => l.EstadoMiembro == "DE").Should().BeEquivalentTo(
            new LineaIntrastatResp("DE", "DAP", "11", "3", "08051022", "ES", 1_500m, 1_800m, "DE123456789"), "las dos facturas a Alemania se agrupan");
        exp.Avisos.Should().Contain(a => a.Contains("NIF-IVA", StringComparison.Ordinal)).And.Contain(a => a.Contains("código arancelario", StringComparison.Ordinal));
        exp.TotalImporte.Should().Be(1_812m);
        exp.AcumuladoAnual.Should().Be(1_812m);
        exp.SuperaUmbral.Should().BeFalse();
        var csv = await (await api.GetAsync(new Uri($"/aduanas/intrastat?anio={hoy.Year}&mes={hoy.Month}&flujo=Expedicion&formato=csv", UriKind.Relative))).Content.ReadAsStringAsync();
        csv.Should().Contain("Estado miembro destino").And.Contain("DE;").And.Contain("08051022");

        // Introducción: 500 kg de un proveedor italiano, a 2 €.
        var italiano = await IdAsync(api, "/proveedores", new { Nombre = "Agrumi SRL", Pais = "IT", NifIva = "IT12345678901" });
        var pedido = (await (await api.PostAsJsonAsync("/compras/pedidos", new
        {
            ProveedorId = italiano, Lineas = new[] { new { ProductoId = naranja, Descripcion = "Naranja", Cantidad = 500m, PrecioUnitario = 2m } },
        })).Content.ReadFromJsonAsync<PedidoResp>())!;
        (await api.PostAsync(new Uri($"/compras/pedidos/{pedido.Id}/confirmar", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/compras/pedidos/{pedido.Id}/recibir", new { Lineas = new[] { new { LineaPedidoId = pedido.Lineas[0].Id, Cantidad = 500m } } }))
            .EnsureSuccessStatusCode();
        var intro = (await api.GetFromJsonAsync<IntrastatResp>($"/aduanas/intrastat?anio={hoy.Year}&mes={hoy.Month}&flujo=Introduccion"))!;
        intro.Lineas.Should().ContainSingle().Which.Should().Match<LineaIntrastatResp>(l =>
            l.EstadoMiembro == "IT" && l.CodigoMercancias == "08051022" && l.MasaNetaKg == 500m && l.ImporteFacturado == 1_000m && l.NifContraparte == null);
    }

    [Fact]
    public async Task La_expedicion_lleva_sus_certificados_fitosanitarios_con_su_documento()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Fresh Produce Ltd", Pais = "GB" });
        var carta = await IdAsync(api, "/cartas-porte", new { DestinatarioClienteId = cliente, Lineas = new[] { new { Descripcion = "Naranja", Bultos = 10, PesoKg = 200m } } });

        var pdf = Convert.ToBase64String(Encoding.ASCII.GetBytes("%PDF-1.4 certificado"));
        var r = await api.PostAsJsonAsync($"/cartas-porte/{carta}/certificados", new
        {
            Tipo = "Exportacion", Numero = "ES-2026-0001234", FechaEmision = "2026-09-25", Organismo = "MAPA · Inspección fitosanitaria de Valencia",
            DocumentoNombre = "certificado.pdf", DocumentoTipo = "application/pdf", DocumentoBase64 = pdf,
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var cert = (await r.Content.ReadFromJsonAsync<CertificadoResp>())!;
        cert.Should().Match<CertificadoResp>(c => c.PaisDestino == "GB" && c.TieneDocumento, "el país es el de destino de la carta");

        (await CodigoAsync(await api.PostAsJsonAsync($"/cartas-porte/{carta}/certificados", new
        {
            Tipo = "Exportacion", Numero = "X", FechaEmision = "2026-09-25", DocumentoTipo = "image/gif", DocumentoBase64 = pdf,
        }))).Should().Be("certificado.documento");

        var doc = await api.GetAsync(new Uri($"/certificados-fitosanitarios/{cert.Id}/documento", UriKind.Relative));
        doc.Content.Headers.ContentType!.MediaType.Should().Be("application/pdf");
        (await doc.Content.ReadAsStringAsync()).Should().StartWith("%PDF");

        (await api.GetFromJsonAsync<CartaResp>($"/cartas-porte/{carta}"))!.Certificados.Should().Equal("Certificado fitosanitario nº ES-2026-0001234");
        (await api.GetAsync(new Uri($"/cartas-porte/{carta}/pdf", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.OK);

        (await api.PutAsJsonAsync($"/certificados-fitosanitarios/{cert.Id}", new { Tipo = "Exportacion", Numero = "ES-2026-0001234", FechaEmision = "2026-09-25", QuitarDocumento = true }))
            .EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<CertificadoResp>>($"/cartas-porte/{carta}/certificados"))!.Single().TieneDocumento.Should().BeFalse();
        (await api.DeleteAsync(new Uri($"/certificados-fitosanitarios/{cert.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await api.GetFromJsonAsync<List<CertificadoResp>>($"/cartas-porte/{carta}/certificados"))!.Should().BeEmpty();
    }

    [Fact]
    public async Task La_compra_de_fuera_de_la_ue_pide_su_dua_de_importacion()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var chino = await IdAsync(api, "/proveedores", new { Nombre = "Ningbo Packaging Co", Pais = "CN" });
        var local = await IdAsync(api, "/proveedores", new { Nombre = "Papelera Local", Pais = "ES" });
        var gasto = await IdAsync(api, "/gastos", new { ProveedorId = chino, Concepto = "Cajas de cartón (importación)", BaseImponible = 10_000m, CodigoIva = "IVA0" });
        await IdAsync(api, "/gastos", new { ProveedorId = local, Concepto = "Papel", BaseImponible = 100m, CodigoIva = "IVA21" });

        var lista = (await api.GetFromJsonAsync<List<ImportacionResp>>("/aduanas/importaciones"))!;
        lista.Should().ContainSingle().Which.Should().Match<ImportacionResp>(i => i.GastoId == gasto && i.Pais == "CN" && i.Situacion == "Sin DUA");

        (await CodigoAsync(await api.PostAsJsonAsync($"/aduanas/gastos/{gasto}/duas", new { Mrn = "XX", FechaAdmision = "2026-09-10", BaseIva = 10_800m, CuotaIva = 2_268m })))
            .Should().Be("dua.mrn");
        var d = await api.PostAsJsonAsync($"/aduanas/gastos/{gasto}/duas", new
        {
            Mrn = "26ES00281130098765", FechaAdmision = "2026-09-10", BaseIva = 10_800m, Aranceles = 300m, CuotaIva = 2_268m, Aduana = "Valencia",
        });
        d.StatusCode.Should().Be(HttpStatusCode.Created, await d.Content.ReadAsStringAsync());
        var dua = (await d.Content.ReadFromJsonAsync<DuaResp>())!;
        (await api.PostAsJsonAsync($"/aduanas/gastos/{gasto}/duas", new { Mrn = "26ES00281130098765", FechaAdmision = "2026-09-10", BaseIva = 1m, CuotaIva = 0m }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await api.GetFromJsonAsync<List<ImportacionResp>>("/aduanas/importaciones"))!.Single()
            .Should().Match<ImportacionResp>(i => i.Situacion == "Con DUA" && i.CuotaIvaImportacion == 2_268m);

        (await api.DeleteAsync(new Uri($"/aduanas/duas-importacion/{dua.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await api.GetFromJsonAsync<List<ImportacionResp>>("/aduanas/importaciones"))!.Single().Situacion.Should().Be("Sin DUA");
    }
}
