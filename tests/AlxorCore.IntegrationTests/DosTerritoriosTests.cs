using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Un mismo NIF con actividad en la Península y en Canarias: cada factura va con IVA o con IGIC según sus tipos, el 303 suma
/// solo las del IVA y el 420 solo las del IGIC, y una factura nunca mezcla los dos. Sin marcarlo, la empresa solo factura con
/// el impuesto de su territorio.
/// </summary>
public sealed class DosTerritoriosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DosTerritoriosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record FacturaResp(Guid Id, string Impuesto, decimal BaseImponible);
    private sealed record M303(decimal IvaDevengadoBase);
    private sealed record ResumenResp(M303 Modelo303);
    private sealed record M420(decimal DevengadoBase);
    private sealed record TipoResp(string Codigo, string Impuesto);
    private sealed record FichaResp(List<string> Sugeridos);

    private static Task<HttpResponseMessage> FacturarAsync(HttpClient api, Guid cliente, string codigo, decimal precio) =>
        api.PostAsJsonAsync("/facturas", new { ClienteId = cliente, Lineas = new[] { new { Cantidad = 1m, Descripcion = "Fruta", PrecioUnitario = precio, CodigoIva = codigo } } });

    [Fact]
    public async Task Factura_con_iva_o_con_igic_y_cada_modelo_suma_lo_suyo()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var cliente = (await (await api.PostAsJsonAsync("/clientes", new { Nombre = "Mercado SL", NifFiscal = Ayudas.GenerarNif() })).Content.ReadFromJsonAsync<FacturaResp>())!.Id;

        // Sin marcar los dos territorios, una línea con IGIC no se acepta en una empresa de la Península.
        (await FacturarAsync(api, cliente, "IGIC7", 100m)).StatusCode.Should().Be(HttpStatusCode.BadRequest);

        (await api.PutAsJsonAsync("/empresas/actual/perfil-fiscal", new { Perfil = new { OperaEnAmbosTerritorios = true, Modelos = new[] { "303", "420" } } }))
            .IsSuccessStatusCode.Should().BeTrue();
        var tipos = (await api.GetFromJsonAsync<List<TipoResp>>("/tipos-iva"))!;
        tipos.Should().Contain(t => t.Impuesto == "Igic").And.Contain(t => t.Impuesto == "Iva");
        (await api.GetFromJsonAsync<FichaResp>("/empresas/actual/perfil-fiscal"))!.Sugeridos.Should().Contain(["303", "420", "425"]);

        var peninsula = await FacturarAsync(api, cliente, "IVA21", 100m);
        peninsula.IsSuccessStatusCode.Should().BeTrue(await peninsula.Content.ReadAsStringAsync());
        (await peninsula.Content.ReadFromJsonAsync<FacturaResp>())!.Impuesto.Should().Be("Iva");
        var canarias = await FacturarAsync(api, cliente, "IGIC7", 300m);
        canarias.IsSuccessStatusCode.Should().BeTrue(await canarias.Content.ReadAsStringAsync());
        (await canarias.Content.ReadFromJsonAsync<FacturaResp>())!.Impuesto.Should().Be("Igic");

        // Una factura no mezcla los dos impuestos.
        var mixta = await api.PostAsJsonAsync("/facturas", new
        {
            ClienteId = cliente,
            Lineas = new[] { new { Cantidad = 1m, Descripcion = "A", PrecioUnitario = 10m, CodigoIva = "IGIC7" }, new { Cantidad = 1m, Descripcion = "B", PrecioUnitario = 10m, CodigoIva = "IVA21" } },
        });
        mixta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var anio = DateTime.Today.Year;
        var trimestre = (DateTime.Today.Month - 1) / 3 + 1;
        (await api.GetFromJsonAsync<ResumenResp>($"/informes/resumen-trimestral?anio={anio}&trimestre={trimestre}"))!.Modelo303.IvaDevengadoBase.Should().Be(100m);
        (await api.GetFromJsonAsync<M420>($"/impuestos/modelo-420?anio={anio}&trimestre={trimestre}"))!.DevengadoBase.Should().Be(300m);
    }
}
