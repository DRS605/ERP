using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del cierre contable, la cuenta de PyG y el balance de situación.</summary>
public sealed class CierreContableEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public CierreContableEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record LineaResp(string Codigo, string Nombre, decimal Importe);
    private sealed record PyGResp(int Ejercicio, List<LineaResp> Ingresos, List<LineaResp> Gastos, decimal TotalIngresos, decimal TotalGastos, decimal Resultado);
    private sealed record BalanceResp(decimal TotalActivo, decimal TotalPatrimonioNetoYPasivo, bool Cuadra);
    private sealed record CierreResp(int Ejercicio, decimal Resultado);
    private sealed record AsientoResp(int Numero, string Origen, decimal Total);

    private static async Task PonerModoCompletoAsync(HttpClient c) =>
        (await c.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).StatusCode.Should().Be(HttpStatusCode.OK);

    private static async Task AsientoAsync(HttpClient c, string fecha, string concepto, params object[] lineas) =>
        (await c.PostAsJsonAsync("/contabilidad/asientos", new { Fecha = fecha, Concepto = concepto, Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.Created);

    private static async Task SembrarEjercicioAsync(HttpClient c)
    {
        // Venta: ingreso 705 = 1000, IVA 477 = 210, cliente 430 = 1210.
        await AsientoAsync(c, "2026-03-01", "Venta",
            new { CuentaCodigo = "430", Debe = 1210m, Haber = 0m },
            new { CuentaCodigo = "705", Debe = 0m, Haber = 1000m },
            new { CuentaCodigo = "477", Debe = 0m, Haber = 210m });
        // Compra: gasto 629 = 400, IVA 472 = 84, proveedor 400 = 484.
        await AsientoAsync(c, "2026-04-01", "Compra",
            new { CuentaCodigo = "629", Debe = 400m, Haber = 0m },
            new { CuentaCodigo = "472", Debe = 84m, Haber = 0m },
            new { CuentaCodigo = "400", Debe = 0m, Haber = 484m });
    }

    [Fact]
    public async Task La_pyg_resta_gastos_a_ingresos()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        await SembrarEjercicioAsync(cliente);

        var pyg = await cliente.GetFromJsonAsync<PyGResp>("/contabilidad/pyg?ejercicio=2026");
        pyg!.TotalIngresos.Should().Be(1000m);
        pyg.TotalGastos.Should().Be(400m);
        pyg.Resultado.Should().Be(600m);
        pyg.Ingresos.Should().Contain(l => l.Codigo == "705" && l.Importe == 1000m);
        pyg.Gastos.Should().Contain(l => l.Codigo == "629" && l.Importe == 400m);
    }

    [Fact]
    public async Task El_balance_de_situacion_cuadra_incluyendo_el_resultado_en_patrimonio()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        await SembrarEjercicioAsync(cliente);

        var bal = await cliente.GetFromJsonAsync<BalanceResp>("/contabilidad/balance-situacion?ejercicio=2026");
        bal!.Cuadra.Should().BeTrue();
        bal.TotalActivo.Should().Be(1294m);              // 430 (1210) + 472 (84)
        bal.TotalPatrimonioNetoYPasivo.Should().Be(1294m); // 400 (484) + 477 (210) + resultado (600)
    }

    [Fact]
    public async Task El_cierre_regulariza_cierra_y_abre_el_siguiente_ejercicio()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        await SembrarEjercicioAsync(cliente);

        var cierre = await cliente.PostAsync("/contabilidad/cierre?ejercicio=2026", null);
        cierre.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cierre.Content.ReadFromJsonAsync<CierreResp>())!.Resultado.Should().Be(600m);

        // El diario de 2026 tiene regularización y cierre.
        var diario2026 = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario2026!.Should().Contain(a => a.Origen == "Regularizacion");
        diario2026.Should().Contain(a => a.Origen == "Cierre");

        // El 2027 tiene la apertura.
        var diario2027 = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2027");
        diario2027!.Should().Contain(a => a.Origen == "Apertura");

        // Tras la PyG del cierre, los grupos 6 y 7 quedan a cero.
        var pyg = await cliente.GetFromJsonAsync<PyGResp>("/contabilidad/pyg?ejercicio=2026");
        pyg!.TotalIngresos.Should().Be(0m);
        pyg.TotalGastos.Should().Be(0m);
    }

    [Fact]
    public async Task No_se_puede_cerrar_dos_veces_ni_asentar_en_un_ejercicio_cerrado()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await PonerModoCompletoAsync(cliente);
        await SembrarEjercicioAsync(cliente);
        (await cliente.PostAsync("/contabilidad/cierre?ejercicio=2026", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Segundo cierre → conflicto.
        (await cliente.PostAsync("/contabilidad/cierre?ejercicio=2026", null)).StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Asiento manual en el ejercicio cerrado → conflicto.
        var manual = await cliente.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2026-06-01",
            Concepto = "Tardío",
            Lineas = new[]
            {
                new { CuentaCodigo = "572", Debe = 100m, Haber = 0m },
                new { CuentaCodigo = "705", Debe = 0m, Haber = 100m },
            },
        });
        manual.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
