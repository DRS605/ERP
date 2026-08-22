using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>Pruebas del inmovilizado: amortización contable/fiscal, impuesto diferido, baja y enajenación.</summary>
public sealed class InmovilizadoEndpointsTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InmovilizadoEndpointsTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record InmoResp(Guid Id, string Codigo, string Estado, decimal AmortizacionAcumulada, decimal ValorNetoContable);
    private sealed record ApunteResp(string CuentaCodigo, decimal Debe, decimal Haber);
    private sealed record AsientoResp(int Numero, string Origen, decimal Total, List<ApunteResp> Apuntes);
    private sealed record AmortizacionResp(int Ejercicio, int AsientosGenerados, decimal TotalDotado, decimal TotalImpuestoDiferido);
    private sealed record FilaResp(int Ejercicio, decimal Contable, decimal Fiscal, decimal DiferenciaTemporaria, decimal Contabilizado);
    private sealed record CuadroResp(decimal BaseAmortizable, decimal AmortizacionAcumulada, List<FilaResp> Filas);
    private sealed record BalanceResp(decimal TotalActivo, decimal TotalPatrimonioNetoYPasivo, bool Cuadra);

    // Furgoneta: 12.000 €, sin residual. Contable lineal 4 años (3.000/año), fiscal lineal 2 años (6.000/año).
    private static object NuevaFurgoneta(string codigo = "INM-1") => new
    {
        Codigo = codigo,
        Descripcion = "Furgoneta de reparto",
        CuentaActivo = "218",
        CuentaAmortizacion = "281",
        CuentaDotacion = "681",
        FechaAdquisicion = "2026-01-01",
        FechaAlta = "2026-01-01",
        ValorAdquisicion = 12000m,
        ValorResidual = 0m,
        Periodicidad = "Anual",
        MetodoContable = "Lineal",
        VidaUtilContable = 4,
        PorcentajeDegresivoContable = 0m,
        MetodoFiscal = "Lineal",
        VidaUtilFiscal = 2,
        PorcentajeDegresivoFiscal = 0m,
    };

    private static async Task<Guid> CrearAsync(HttpClient c, object cuerpo)
    {
        var r = await c.PostAsJsonAsync("/contabilidad/inmovilizado", cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await r.Content.ReadFromJsonAsync<InmoResp>())!.Id;
    }

    private static async Task AsientoManualAsync(HttpClient c, string fecha, string concepto, params object[] lineas) =>
        (await c.PostAsJsonAsync("/contabilidad/asientos", new { Fecha = fecha, Concepto = concepto, Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.Created);

    [Fact]
    public async Task La_amortizacion_anual_dota_contable_y_genera_el_impuesto_diferido()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CrearAsync(cliente, NuevaFurgoneta());

        var r = await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2026&tipoImpuesto=0.25", null);
        r.StatusCode.Should().Be(HttpStatusCode.OK);
        var res = (await r.Content.ReadFromJsonAsync<AmortizacionResp>())!;
        res.TotalDotado.Should().Be(3000m);              // 12000 / 4 años
        res.AsientosGenerados.Should().Be(2);            // dotación + impuesto diferido

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        // Dotación: 681 al debe 3000, 281 al haber 3000.
        diario!.Should().Contain(a => a.Origen == "Amortizacion"
            && a.Apuntes.Any(p => p.CuentaCodigo == "681" && p.Debe == 3000m)
            && a.Apuntes.Any(p => p.CuentaCodigo == "281" && p.Haber == 3000m));
        // Impuesto diferido: fiscal 6000 − contable 3000 = 3000; × 25% = 750 al pasivo 479.
        diario.Should().Contain(a => a.Origen == "ImpuestoDiferido"
            && a.Apuntes.Any(p => p.CuentaCodigo == "479" && p.Haber == 750m)
            && a.Apuntes.Any(p => p.CuentaCodigo == "6301" && p.Debe == 750m));
    }

    [Fact]
    public async Task La_amortizacion_es_idempotente()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CrearAsync(cliente, NuevaFurgoneta());

        (await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2026", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var segunda = await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2026", null);
        var res = (await segunda.Content.ReadFromJsonAsync<AmortizacionResp>())!;
        res.AsientosGenerados.Should().Be(0); // ya estaba contabilizado
    }

    [Fact]
    public async Task El_cuadro_muestra_contable_fiscal_y_diferencia()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var id = await CrearAsync(cliente, NuevaFurgoneta());

        var cuadro = await cliente.GetFromJsonAsync<CuadroResp>($"/contabilidad/inmovilizado/{id}/cuadro");
        cuadro!.BaseAmortizable.Should().Be(12000m);
        cuadro.Filas.Should().Contain(f => f.Ejercicio == 2026 && f.Contable == 3000m && f.Fiscal == 6000m && f.DiferenciaTemporaria == 3000m);
        // La vida contable (4 años) es la más larga: el cuadro llega hasta 2029.
        cuadro.Filas.Select(f => f.Ejercicio).Should().Contain(new[] { 2026, 2027, 2028, 2029 });
        cuadro.Filas.Sum(f => f.Contable).Should().Be(12000m);
    }

    [Fact]
    public async Task El_balance_de_situacion_cuadra_con_inmovilizado_y_amortizacion_acumulada()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        await CrearAsync(cliente, NuevaFurgoneta());
        // Alta contable de la compra del activo: 218 al debe / 572 al haber.
        await AsientoManualAsync(cliente, "2026-01-01", "Compra furgoneta",
            new { CuentaCodigo = "218", Debe = 12000m, Haber = 0m },
            new { CuentaCodigo = "572", Debe = 0m, Haber = 12000m });
        (await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2026&tipoImpuesto=0.25", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var bal = await cliente.GetFromJsonAsync<BalanceResp>("/contabilidad/balance-situacion?ejercicio=2026");
        bal!.Cuadra.Should().BeTrue();
        bal.TotalActivo.Should().Be(9000m); // 218 (12000) − amortización acumulada 281 (3000)
    }

    [Fact]
    public async Task La_enajenacion_con_beneficio_genera_el_resultado_y_bloquea_futuras_amortizaciones()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var id = await CrearAsync(cliente, NuevaFurgoneta());
        await AsientoManualAsync(cliente, "2026-01-01", "Compra furgoneta",
            new { CuentaCodigo = "218", Debe = 12000m, Haber = 0m },
            new { CuentaCodigo = "572", Debe = 0m, Haber = 12000m });
        (await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2026", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Valor neto tras 2026 = 12000 − 3000 = 9000. Venta por 10000 → beneficio 1000.
        var r = await cliente.PostAsJsonAsync($"/contabilidad/inmovilizado/{id}/enajenar", new { Fecha = "2027-06-01", ValorEnajenacion = 10000m, PorcentajeIva = 0m });
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var diario2027 = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2027");
        diario2027!.Should().Contain(a => a.Origen == "Enajenacion"
            && a.Apuntes.Any(p => p.CuentaCodigo == "771" && p.Haber == 1000m)   // beneficio
            && a.Apuntes.Any(p => p.CuentaCodigo == "281" && p.Debe == 3000m)    // cancela amortización acumulada
            && a.Apuntes.Any(p => p.CuentaCodigo == "218" && p.Haber == 12000m)); // baja del activo

        var lista = await cliente.GetFromJsonAsync<List<InmoResp>>("/contabilidad/inmovilizado");
        lista!.Single(i => i.Id == id).Estado.Should().Be("Enajenado");

        // Ya enajenado: no amortiza más en 2027.
        var amort2027 = await cliente.PostAsync("/contabilidad/inmovilizado/amortizar?ejercicio=2027", null);
        (await amort2027.Content.ReadFromJsonAsync<AmortizacionResp>())!.AsientosGenerados.Should().Be(0);
    }

    [Fact]
    public async Task La_baja_sin_contraprestacion_lleva_el_valor_neto_a_perdidas()
    {
        var (cliente, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        var id = await CrearAsync(cliente, new
        {
            Codigo = "INM-EQ",
            Descripcion = "Equipo informático obsoleto",
            CuentaActivo = "217",
            CuentaAmortizacion = "281",
            CuentaDotacion = "681",
            FechaAdquisicion = "2026-01-01",
            FechaAlta = "2026-01-01",
            ValorAdquisicion = 5000m,
            ValorResidual = 0m,
            Periodicidad = "Anual",
            MetodoContable = "Lineal",
            VidaUtilContable = 5,
            PorcentajeDegresivoContable = 0m,
            MetodoFiscal = "Lineal",
            VidaUtilFiscal = 5,
            PorcentajeDegresivoFiscal = 0m,
        });

        // Baja sin amortizar: todo el valor (5000) va a pérdidas (671).
        var r = await cliente.PostAsJsonAsync($"/contabilidad/inmovilizado/{id}/baja", new { Fecha = "2026-09-01" });
        r.StatusCode.Should().Be(HttpStatusCode.OK);

        var diario = await cliente.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2026");
        diario!.Should().Contain(a => a.Origen == "BajaInmovilizado"
            && a.Apuntes.Any(p => p.CuentaCodigo == "671" && p.Debe == 5000m)
            && a.Apuntes.Any(p => p.CuentaCodigo == "217" && p.Haber == 5000m));
    }
}
