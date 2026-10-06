using System.Net;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Transporte y aduanas: transportistas y vehículos, carta de porte internacional (CMR) con sus datos de comercio
/// exterior, y el DUA de exportación de las facturas exentas (MRN, salida y datos para el agente de aduanas).
/// </summary>
public sealed class TransporteAduanasTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public TransporteAduanasTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record TransportistaResp(Guid Id, string Nombre, string? Pais);
    private sealed record VehiculoResp(Guid Id, string Matricula, string? MatriculaRemolque, bool Frigorifico);
    private sealed record TransporteResp(string? Incoterm, string? LugarIncoterm, string? PaisOrigen, string? PaisDestino, decimal? TemperaturaConsigna, string? Portes, string? MatriculaRemolque);
    private sealed record LineaResp(string Descripcion, string? CodigoArancelario, decimal? PesoNetoKg);
    private sealed record CartaResp(Guid Id, string Tipo, string? TransportistaNombre, string? Matricula, TransporteResp Transporte, List<LineaResp> Lineas);
    private sealed record DespachoResp(Guid Id, string Mrn, DateOnly? FechaSalida);
    private sealed record ExportacionResp(Guid FacturaId, string Situacion, string? PaisDestino, decimal BaseExportacion);
    private sealed record PartidaResp(string? CodigoArancelario, string? PaisOrigen, decimal Cantidad, decimal? PesoNetoKg, decimal Valor);
    private sealed record DatosAduanaResp(string ExportadorEori, string? DestinatarioEori, string? PaisDestino, string? Incoterm, decimal ValorTotal, decimal? PesoNetoKg,
        List<PartidaResp> Partidas, List<string> Avisos);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.IsSuccessStatusCode.Should().BeTrue($"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<string> CodigoAsync(HttpResponseMessage r) => (await r.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo;

    [Fact]
    public async Task La_carta_de_porte_a_otro_pais_es_un_cmr_con_transporte_temperatura_incoterm_y_codigo_arancelario()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var transportista = (await (await api.PostAsJsonAsync("/transporte/transportistas", new { Nombre = "Frigoríficos del Turia SL", Nif = "B98765432", Pais = "España" }))
            .Content.ReadFromJsonAsync<TransportistaResp>())!;
        transportista.Pais.Should().Be("ES");
        var vehiculo = (await (await api.PostAsJsonAsync("/transporte/vehiculos", new
        {
            Matricula = "1234 abc", MatriculaRemolque = "R-9876-BCD", TaraKg = 14_500m, Frigorifico = true, TransportistaId = transportista.Id,
        })).Content.ReadFromJsonAsync<VehiculoResp>())!;
        vehiculo.Matricula.Should().Be("1234ABC");
        (await api.PostAsJsonAsync("/transporte/vehiculos", new { Matricula = "1234-ABC" })).StatusCode.Should().Be(HttpStatusCode.Conflict);

        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja Navel", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg", CodigoArancelario = "0805 10 22", PaisOrigen = "ES" });
        (await CodigoAsync(await api.PostAsJsonAsync("/productos", new { Nombre = "Mal", PrecioUnitario = 1m, CodigoArancelario = "0805" })))
            .Should().Be("producto.codigo_arancelario");
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Primeurs de Paris SARL", Poblacion = "Rungis", Pais = "Francia", Incoterm = "dap", LugarIncoterm = "Rungis" });
        (await CodigoAsync(await api.PostAsJsonAsync("/clientes", new { Nombre = "Mal", Incoterm = "XYZ" }))).Should().Be("cliente.incoterm");

        var r = await api.PostAsJsonAsync("/cartas-porte", new
        {
            DestinatarioClienteId = cliente,
            Transporte = new { VehiculoId = vehiculo.Id, TemperaturaConsigna = 6m, Termografo = "TG-778", Portes = "Pagados", Conductor = "Juan Pérez", DocumentosAnexos = "Factura, certificado fitosanitario" },
            Lineas = new[] { new { Descripcion = "Naranja Navel", Bultos = 1_056, PesoKg = 23_100m, PesoNetoKg = 21_120m, Embalaje = "Cajas en 22 palés", ProductoId = naranja } },
        });
        r.StatusCode.Should().Be(HttpStatusCode.Created, await r.Content.ReadAsStringAsync());
        var carta = (await r.Content.ReadFromJsonAsync<CartaResp>())!;
        carta.Tipo.Should().Be("Internacional", "sale de España hacia Francia");
        carta.TransportistaNombre.Should().Be("Frigoríficos del Turia SL", "el transportista es el del vehículo");
        carta.Matricula.Should().Be("1234ABC");
        carta.Transporte.Should().BeEquivalentTo(new TransporteResp("DAP", "Rungis", "ES", "FR", 6m, "Pagados", "R9876BCD"));
        carta.Lineas.Single().CodigoArancelario.Should().Be("08051022", "sale del artículo");

        var pdf = await api.GetAsync(new Uri($"/cartas-porte/{carta.Id}/pdf", UriKind.Relative));
        pdf.StatusCode.Should().Be(HttpStatusCode.OK);
        var bytes = await pdf.Content.ReadAsByteArrayAsync();
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");
        Encoding.Latin1.GetString(bytes).Should().Contain("/Count 3", "tres ejemplares: remitente, consignatario y transportista");

        // Nacional, y validaciones.
        var local = await IdAsync(api, "/clientes", new { Nombre = "Frutas Valencia SL", Pais = "España" });
        var nacional = await api.PostAsJsonAsync("/cartas-porte", new { DestinatarioClienteId = local, Lineas = new[] { new { Descripcion = "Naranja", Bultos = 10, PesoKg = 200m } } });
        (await nacional.Content.ReadFromJsonAsync<CartaResp>())!.Tipo.Should().Be("Nacional");
        (await CodigoAsync(await api.PostAsJsonAsync("/cartas-porte", new
        {
            DestinatarioClienteId = local, Transporte = new { Incoterm = "FOBX" }, Lineas = new[] { new { Descripcion = "Naranja", Bultos = 1, PesoKg = 1m } },
        }))).Should().Be("cartaporte.incoterm");

        // Lo usado no se elimina: se da de baja.
        (await api.DeleteAsync(new Uri($"/transporte/vehiculos/{vehiculo.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await api.PutAsJsonAsync($"/transporte/vehiculos/{vehiculo.Id}", new { Matricula = "1234ABC", Activo = false })).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task La_factura_de_exportacion_pide_su_dua_y_da_los_datos_para_el_agente_de_aduanas()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.GetAsync(new Uri("/tipos-iva", UriKind.Relative))).EnsureSuccessStatusCode(); // siembra el catálogo (EXPORT)
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja naranja 10 kg", PrecioUnitario = 9m, Tipo = "Bien", Unidad = "caja", PesoKg = 10m, CodigoArancelario = "0805102200", PaisOrigen = "ES" });
        var sinCodigo = await IdAsync(api, "/productos", new { Nombre = "Limón", PrecioUnitario = 1m, Tipo = "Bien", Unidad = "kg" });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Fresh Produce Ltd", Poblacion = "London", Pais = "Reino Unido", Eori = "gb 123456789000", Incoterm = "FCA", LugarIncoterm = "Valencia" });
        var f = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new object[]
            {
                new { ProductoId = caja, Descripcion = "Caja naranja 10 kg", Cantidad = 100m, PrecioUnitario = 9m, CodigoIva = "EXPORT" },
                new { ProductoId = sinCodigo, Descripcion = "Limón", Cantidad = 50m, PrecioUnitario = 1m, CodigoIva = "EXPORT" },
            },
        });
        f.IsSuccessStatusCode.Should().BeTrue(await f.Content.ReadAsStringAsync());
        var facturaId = (await f.Content.ReadFromJsonAsync<IdResp>())!.Id;

        var exp = (await api.GetFromJsonAsync<List<ExportacionResp>>("/aduanas/exportaciones"))!.Single();
        exp.Should().Be(new ExportacionResp(facturaId, "Sin DUA", "GB", 950m));

        var datos = (await api.GetFromJsonAsync<DatosAduanaResp>($"/aduanas/facturas/{facturaId}"))!;
        datos.ExportadorEori.Should().StartWith("ES");
        datos.DestinatarioEori.Should().Be("GB123456789000");
        datos.Incoterm.Should().Be("FCA");
        datos.ValorTotal.Should().Be(950m);
        datos.Partidas.Should().ContainEquivalentOf(new PartidaResp("0805102200", "ES", 100m, 1_000m, 900m), "100 cajas de 10 kg");
        datos.Partidas.Should().ContainEquivalentOf(new PartidaResp(null, null, 50m, 50m, 50m), "el limón se vende por kilos");
        datos.Avisos.Should().Contain(a => a.Contains("Limón", StringComparison.Ordinal) && a.Contains("código arancelario", StringComparison.Ordinal));
        datos.Avisos.Should().Contain(a => a.Contains("carta de porte", StringComparison.Ordinal));
        var csv = await (await api.GetAsync(new Uri($"/aduanas/facturas/{facturaId}?formato=csv", UriKind.Relative))).Content.ReadAsStringAsync();
        csv.Should().Contain("Código arancelario").And.Contain("0805102200");

        // DUA: MRN válido y único; con la salida, la exportación queda probada.
        (await CodigoAsync(await api.PostAsJsonAsync($"/aduanas/facturas/{facturaId}/despachos", new { Mrn = "123", FechaDespacho = "2026-09-20" }))).Should().Be("despacho.mrn");
        var d = await api.PostAsJsonAsync($"/aduanas/facturas/{facturaId}/despachos", new { Mrn = "26es00461120012345", FechaDespacho = "2026-09-20", Aduana = "Valencia (4600)" });
        d.StatusCode.Should().Be(HttpStatusCode.Created, await d.Content.ReadAsStringAsync());
        var despacho = (await d.Content.ReadFromJsonAsync<DespachoResp>())!;
        despacho.Mrn.Should().Be("26ES00461120012345");
        (await api.PostAsJsonAsync($"/aduanas/facturas/{facturaId}/despachos", new { Mrn = "26ES00461120012345", FechaDespacho = "2026-09-20" }))
            .StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await api.GetFromJsonAsync<List<ExportacionResp>>("/aduanas/exportaciones"))!.Single().Situacion.Should().Be("Despachada");

        (await CodigoAsync(await api.PutAsJsonAsync($"/aduanas/despachos/{despacho.Id}", new { Mrn = despacho.Mrn, FechaDespacho = "2026-09-20", FechaSalida = "2026-09-19" })))
            .Should().Be("despacho.fecha_salida");
        (await api.PutAsJsonAsync($"/aduanas/despachos/{despacho.Id}", new { Mrn = despacho.Mrn, FechaDespacho = "2026-09-20", FechaSalida = "2026-09-22" })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<List<ExportacionResp>>("/aduanas/exportaciones"))!.Single().Situacion.Should().Be("Salida confirmada");

        (await api.DeleteAsync(new Uri($"/aduanas/despachos/{despacho.Id}", UriKind.Relative))).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await api.GetFromJsonAsync<List<ExportacionResp>>("/aduanas/exportaciones"))!.Single().Situacion.Should().Be("Sin DUA");
    }
}
