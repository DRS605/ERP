using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de integración del módulo Divisas (tipos de cambio, conversión y diferencias).</summary>
public sealed class DivisasEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public DivisasEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record DivisaResp(string Codigo, string Nombre, int Decimales);
    private sealed record TipoCambioResp(Guid Id, string Divisa, DateOnly Fecha, decimal TasaEur);
    private sealed record ConversionResp(string Divisa, decimal Importe, decimal TasaEur, decimal ImporteEur);
    private sealed record LineaDif(string Referencia, string Divisa, decimal DiferenciaEur, bool EsIngreso, string Cuenta);
    private sealed record DifResp(decimal TotalDiferencia, decimal TotalIngresos, decimal TotalGastos, List<LineaDif> Lineas);

    [Fact]
    public async Task El_catalogo_incluye_las_divisas_habituales()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var divisas = await cli.GetFromJsonAsync<List<DivisaResp>>("/divisas");
        divisas.Should().Contain(d => d.Codigo == "USD");
        divisas.Should().Contain(d => d.Codigo == "EUR");
    }

    [Fact]
    public async Task Registrar_actualizar_y_usar_la_tasa_vigente()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);

        // Dos tasas de USD en fechas distintas.
        (await cli.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-01-01", TasaEur = 0.90m })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await cli.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-06-01", TasaEur = 0.95m })).StatusCode.Should().Be(HttpStatusCode.Created);

        var lista = await cli.GetFromJsonAsync<List<TipoCambioResp>>("/tipos-cambio");
        lista!.Should().HaveCount(2);

        // Conversión al 15/03: usa la tasa vigente (la de 01/01 = 0,90).
        var c1 = await cli.GetFromJsonAsync<ConversionResp>("/divisas/convertir?divisa=USD&importe=1000&fecha=2026-03-15");
        c1!.TasaEur.Should().Be(0.90m);
        c1.ImporteEur.Should().Be(900m);

        // Conversión al 01/07: usa la de 01/06 = 0,95.
        var c2 = await cli.GetFromJsonAsync<ConversionResp>("/divisas/convertir?divisa=USD&importe=1000&fecha=2026-07-01");
        c2!.TasaEur.Should().Be(0.95m);
        c2.ImporteEur.Should().Be(950m);

        // Reenviar misma divisa+fecha actualiza (no duplica).
        await cli.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-01-01", TasaEur = 0.91m });
        (await cli.GetFromJsonAsync<List<TipoCambioResp>>("/tipos-cambio"))!.Should().HaveCount(2);
    }

    [Fact]
    public async Task El_euro_se_convierte_a_si_mismo_sin_tasa()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var c = await cli.GetFromJsonAsync<ConversionResp>("/divisas/convertir?divisa=EUR&importe=500&fecha=2026-03-01");
        c!.TasaEur.Should().Be(1m);
        c.ImporteEur.Should().Be(500m);
    }

    [Fact]
    public async Task Convertir_sin_tasa_da_error()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await cli.GetAsync("/divisas/convertir?divisa=USD&importe=100&fecha=2026-03-01")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Diferencias_de_cambio_reparten_768_y_668()
    {
        var (cli, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await cli.PostAsJsonAsync("/tipos-cambio", new { Divisa = "USD", Fecha = "2026-12-31", TasaEur = 0.95m });

        var comando = new
        {
            FechaValoracion = "2026-12-31",
            Posiciones = new[]
            {
                new { Referencia = "FA2026/1", Divisa = "USD", ImporteDivisa = 1000m, TasaOrigen = 0.90m, EsActivo = true },  // sube ⇒ +50 (768)
                new { Referencia = "GA-7",     Divisa = "USD", ImporteDivisa = 2000m, TasaOrigen = 0.98m, EsActivo = false }, // pasivo, baja ⇒ ganancia? 0,98→0,95 pasivo ⇒ +60
            },
        };
        var r = await cli.PostAsJsonAsync("/divisas/diferencias-cambio", comando);
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var dif = await r.Content.ReadFromJsonAsync<DifResp>();
        dif!.Lineas.Should().HaveCount(2);
        dif.Lineas.Single(l => l.Referencia == "FA2026/1").DiferenciaEur.Should().Be(50m);
        dif.Lineas.Single(l => l.Referencia == "FA2026/1").Cuenta.Should().Be("768");
        // pasivo 2000 USD: (0,98−0,95)×2000 = 60 a favor.
        dif.Lineas.Single(l => l.Referencia == "GA-7").DiferenciaEur.Should().Be(60m);
        dif.TotalIngresos.Should().Be(110m);
        dif.TotalGastos.Should().Be(0m);
    }
}
