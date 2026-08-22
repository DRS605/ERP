using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas de las Cuentas Anuales normalizadas y la liquidación del modelo 200 (IS).</summary>
public sealed class CuentasAnualesEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CuentasAnualesEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record EpigrafeResp(string Concepto, decimal Importe);
    private sealed record BalanceResp(List<EpigrafeResp> ActivoNoCorriente, List<EpigrafeResp> ActivoCorriente, decimal TotalActivo,
        List<EpigrafeResp> PatrimonioNeto, List<EpigrafeResp> PasivoNoCorriente, List<EpigrafeResp> PasivoCorriente, decimal TotalPatrimonioNetoYPasivo, bool Cuadra);
    private sealed record PyGResp(decimal ResultadoExplotacion, decimal ResultadoAntesImpuestos, decimal ImpuestoSociedades, decimal ResultadoEjercicio);
    private sealed record CuentasAnualesResp(int Ejercicio, BalanceResp Balance, PyGResp PerdidasGanancias);
    private sealed record Modelo200Resp(decimal ResultadoContableAntesImpuestos, decimal BaseImponible, decimal TipoGravamen,
        decimal CuotaIntegra, decimal CuotaLiquida, decimal RetencionesYPagosACuenta, decimal CuotaDiferencial);

    private static async Task AsientoAsync(HttpClient c, string fecha, string concepto, params object[] lineas) =>
        (await c.PostAsJsonAsync("/contabilidad/asientos", new { Fecha = fecha, Concepto = concepto, Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.Created);

    private static async Task SembrarAsync(HttpClient c)
    {
        await AsientoAsync(c, "2026-03-01", "Venta",
            new { CuentaCodigo = "430", Debe = 1210m, Haber = 0m },
            new { CuentaCodigo = "705", Debe = 0m, Haber = 1000m },
            new { CuentaCodigo = "477", Debe = 0m, Haber = 210m });
        await AsientoAsync(c, "2026-04-01", "Compra",
            new { CuentaCodigo = "629", Debe = 400m, Haber = 0m },
            new { CuentaCodigo = "472", Debe = 84m, Haber = 0m },
            new { CuentaCodigo = "400", Debe = 0m, Haber = 484m });
    }

    [Fact]
    public async Task El_balance_normalizado_cuadra_y_la_pyg_da_el_resultado()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await SembrarAsync(cliente);

        var ca = await cliente.GetFromJsonAsync<CuentasAnualesResp>("/contabilidad/cuentas-anuales?ejercicio=2026");
        ca!.Balance.Cuadra.Should().BeTrue();
        ca.Balance.TotalActivo.Should().Be(1294m);                 // 430 (1210) + 472 (84)
        ca.Balance.TotalPatrimonioNetoYPasivo.Should().Be(1294m);  // 400 (484) + 477 (210) + resultado (600)
        ca.PerdidasGanancias.ResultadoExplotacion.Should().Be(600m);
        ca.PerdidasGanancias.ResultadoEjercicio.Should().Be(600m);
        ca.Balance.PatrimonioNeto.Should().Contain(e => e.Concepto.Contains("Resultado") && e.Importe == 600m);
    }

    [Fact]
    public async Task El_modelo_200_calcula_base_y_cuota_al_tipo_indicado()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await SembrarAsync(cliente);

        var m = await cliente.GetFromJsonAsync<Modelo200Resp>("/contabilidad/modelo-200?ejercicio=2026&tipo=0.25");
        m!.ResultadoContableAntesImpuestos.Should().Be(600m);
        m.BaseImponible.Should().Be(600m);
        m.CuotaIntegra.Should().Be(150m);   // 600 × 25%
        m.CuotaLiquida.Should().Be(150m);
        m.CuotaDiferencial.Should().Be(150m); // sin retenciones
    }

    [Fact]
    public async Task Los_ajustes_extracontables_modifican_la_base_imponible()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await SembrarAsync(cliente);

        // Ajuste positivo de 100 (gasto no deducible) → base 700, cuota 175.
        var m = await cliente.GetFromJsonAsync<Modelo200Resp>("/contabilidad/modelo-200?ejercicio=2026&tipo=0.25&ajustesAumentos=100");
        m!.BaseImponible.Should().Be(700m);
        m.CuotaIntegra.Should().Be(175m);
    }
}
