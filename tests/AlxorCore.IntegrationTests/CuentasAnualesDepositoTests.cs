using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Cuentas anuales con las claves del modelo de depósito (con el ejercicio anterior, también ya cerrado), el paquete de
/// legalización de libros y el modo contable por defecto según la forma jurídica.
/// </summary>
public sealed class CuentasAnualesDepositoTests : IClassFixture<FabricaApiPruebas>
{
    private static int _cif = Random.Shared.Next(1_000_000, 8_000_000);
    private readonly FabricaApiPruebas _fabrica;

    public CuentasAnualesDepositoTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record PartidaResp(string Clave, string Concepto, decimal Actual, decimal Anterior);
    private sealed record DepositoResp(int Ejercicio, List<PartidaResp> Balance, List<PartidaResp> PerdidasGanancias, decimal TotalActivo, bool Cuadra, List<string> CuentasSinClasificar);
    private sealed record ConfigResp(string Modo);
    private sealed record EmpresaResp(Guid Id);
    private sealed record SeleccionResp(Guid EmpresaId, string Token);
    private sealed record ProblemaResp(string Codigo);

    private static async Task AsientoAsync(HttpClient c, string fecha, params object[] lineas) =>
        (await c.PostAsJsonAsync("/contabilidad/asientos", new { Fecha = fecha, Concepto = "Operación", Lineas = lineas })).StatusCode.Should().Be(HttpStatusCode.Created);

    private static object L(string cuenta, decimal debe, decimal haber) => new { CuentaCodigo = cuenta, Debe = debe, Haber = haber };

    /// <summary>CIF de sociedad limitada (B) con su dígito de control.</summary>
    private static string Cif()
    {
        var n = Interlocked.Increment(ref _cif).ToString("D7", System.Globalization.CultureInfo.InvariantCulture);
        var suma = 0;
        for (var i = 0; i < 7; i++)
        {
            var d = n[i] - '0';
            suma += i % 2 == 0 ? (d * 2 / 10) + (d * 2 % 10) : d;
        }

        return $"B{n}{(10 - (suma % 10)) % 10}";
    }

    [Fact]
    public async Task Balance_y_pyg_con_claves_oficiales_y_el_ejercicio_anterior_cerrado()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();
        await AsientoAsync(api, "2025-01-10", L("572", 3000m, 0m), L("100", 0m, 3000m));
        await AsientoAsync(api, "2025-03-01", L("430", 1210m, 0m), L("705", 0m, 1000m), L("477", 0m, 210m));
        await AsientoAsync(api, "2025-04-01", L("629", 400m, 0m), L("472", 84m, 0m), L("400", 0m, 484m));
        (await api.PostAsync("/contabilidad/cierre?ejercicio=2025", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        await AsientoAsync(api, "2026-02-01", L("430", 2420m, 0m), L("700", 0m, 2000m), L("477", 0m, 420m));
        await AsientoAsync(api, "2026-02-15", L("572", 1210m, 0m), L("430", 0m, 1210m));

        var d = (await api.GetFromJsonAsync<DepositoResp>("/contabilidad/cuentas-anuales/deposito?ejercicio=2026"))!;
        PartidaResp P(List<PartidaResp> l, string clave) => l.Single(p => p.Clave == clave);
        d.Cuadra.Should().BeTrue();
        d.CuentasSinClasificar.Should().BeEmpty();
        P(d.PerdidasGanancias, "40100").Should().Match<PartidaResp>(p => p.Actual == 2000m && p.Anterior == 1000m, "el cerrado muestra sus cifras, no ceros");
        P(d.PerdidasGanancias, "40700").Anterior.Should().Be(-400m);
        P(d.PerdidasGanancias, "49500").Should().Match<PartidaResp>(p => p.Actual == 2000m && p.Anterior == 600m);
        P(d.Balance, "12700").Should().Match<PartidaResp>(p => p.Actual == 4210m && p.Anterior == 3000m);
        P(d.Balance, "12300").Should().Match<PartidaResp>(p => p.Actual == 2504m && p.Anterior == 1294m, "clientes y el IVA soportado");
        P(d.Balance, "21700").Should().Match<PartidaResp>(p => p.Actual == 2000m && p.Anterior == 600m);
        P(d.Balance, "21500").Actual.Should().Be(600m, "el resultado de 2025 sin aplicar");
        P(d.Balance, "32500").Actual.Should().Be(1114m, "proveedor y el IVA repercutido");
        P(d.Balance, "10000").Actual.Should().Be(P(d.Balance, "30000").Actual).And.Be(6714m);
        (await api.GetStringAsync("/contabilidad/cuentas-anuales/deposito/csv?ejercicio=2026")).Should().Contain("Balance;12700;").And.Contain("4.210,00;3.000,00");

        // Legalización del ejercicio cerrado: el Diario y el de Inventarios y Cuentas Anuales en PDF, e instrucciones.
        var zip = await api.GetAsync("/contabilidad/legalizacion?ejercicio=2025");
        zip.StatusCode.Should().Be(HttpStatusCode.OK, await zip.Content.ReadAsStringAsync());
        using var archivo = new ZipArchive(await zip.Content.ReadAsStreamAsync());
        archivo.Entries.Select(e => e.Name).Should().BeEquivalentTo(["Libro_Diario_2025.pdf", "Libro_Inventarios_y_Cuentas_Anuales_2025.pdf", "LEEME.txt"]);
        foreach (var pdf in archivo.Entries.Where(e => e.Name.EndsWith(".pdf", StringComparison.Ordinal)))
        {
            using var s = pdf.Open();
            var cabecera = new byte[4];
            s.ReadExactly(cabecera);
            System.Text.Encoding.ASCII.GetString(cabecera).Should().Be("%PDF");
        }

        (await (await api.GetAsync("/contabilidad/legalizacion?ejercicio=2020")).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("legalizacion.sin_asientos");
    }

    [Fact]
    public async Task Una_sociedad_empieza_en_modo_completo_y_una_persona_fisica_en_simple()
    {
        var (fisica, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await fisica.GetFromJsonAsync<ConfigResp>("/contabilidad/config"))!.Modo.Should().Be("Simple");

        var api = await Ayudas.AutenticadoAsync(_fabrica);
        var crear = await api.PostAsJsonAsync("/empresas", new { Nif = Cif(), RazonSocial = "Frutas del Valle SL" });
        crear.StatusCode.Should().Be(HttpStatusCode.Created, await crear.Content.ReadAsStringAsync());
        var empresa = (await crear.Content.ReadFromJsonAsync<EmpresaResp>())!;
        var sel = (await (await api.PostAsync(new Uri($"/empresas/{empresa.Id}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sel.Token);
        (await api.GetFromJsonAsync<ConfigResp>("/contabilidad/config"))!.Modo.Should().Be("Completo");

        // La elección de la empresa manda sobre el valor por defecto.
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Simple" })).EnsureSuccessStatusCode();
        (await api.GetFromJsonAsync<ConfigResp>("/contabilidad/config"))!.Modo.Should().Be("Simple");
    }
}
