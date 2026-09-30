using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Regularización de existencias al cierre: la inicial (saldo del grupo 3) sale contra la variación y la final (stock del
/// inventario a 31/12, valorado, por la cuenta de la familia) entra; se anula con un contraasiento y va antes del cierre.
/// </summary>
public sealed class ExistenciasContablesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public ExistenciasContablesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record ProblemaResp(string Title, string Codigo);
    private sealed record LineaResp(string CuentaStock, string CuentaVariacion, decimal Inicial, decimal Final, decimal Calculado, decimal Variacion);
    private sealed record RegResp(int Ejercicio, bool Contabilizada, Guid? AsientoId, bool EjercicioCerrado, decimal Inicial, decimal Final, List<LineaResp> Lineas);
    private sealed record SaldoResp(string CuentaCodigo, decimal SumaDebe, decimal SumaHaber);
    private sealed record AsientoResp(Guid Id, string Origen, string Diario);
    private sealed record CuentaResp(string? Familia, string CuentaStock, string CuentaVariacion);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<decimal> SaldoAsync(HttpClient api, string cuenta, int ejercicio = 2025) =>
        (await api.GetFromJsonAsync<List<SaldoResp>>($"/contabilidad/balance?ejercicio={ejercicio}"))!.Where(s => s.CuentaCodigo == cuenta).Sum(s => s.SumaDebe - s.SumaHaber);

    private static async Task<RegResp> OkAsync(HttpResponseMessage r)
    {
        r.StatusCode.Should().Be(HttpStatusCode.OK, await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<RegResp>())!;
    }

    [Fact]
    public async Task Regulariza_por_familia_con_ajustes_se_anula_y_pasa_a_la_apertura()
    {
        var (api, _) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/contabilidad/modo", new { Modo = "Completo" })).EnsureSuccessStatusCode();

        // Cuentas: la fruta a materias primas (310 → 611); lo demás, a la 300 por defecto.
        (await api.PutAsJsonAsync("/contabilidad/existencias/cuentas", new[] { new { Familia = (string?)null, CuentaStock = "390", CuentaVariacion = (string?)null } }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest, "la 39 es de deterioro");
        var cuentas = await api.PutAsJsonAsync("/contabilidad/existencias/cuentas", new[] { new { Familia = (string?)"Fruta", CuentaStock = "310", CuentaVariacion = (string?)null } });
        cuentas.StatusCode.Should().Be(HttpStatusCode.OK, await cuentas.Content.ReadAsStringAsync());
        (await cuentas.Content.ReadFromJsonAsync<List<CuentaResp>>())!.Should().BeEquivalentTo(
            [new CuentaResp(null, "300", "610"), new CuentaResp("Fruta", "310", "611")]);

        var fruta = await IdAsync(api, "/familias", new { Nombre = "Fruta", Codigo = "FRU" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 3m, Tipo = "Bien", Unidad = "kg", FamiliaId = fruta, PrecioCompra = 2m, ControlarStock = true });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja", PrecioUnitario = 12m, Tipo = "Bien", PrecioCompra = 10m, ControlarStock = true });
        var almacen = await IdAsync(api, "/inventario/almacenes", new { Codigo = "A1", Nombre = "Central" });
        async Task Mover(string tipo, Guid producto, decimal cantidad, string fecha, decimal? coste = null) =>
            (await api.PostAsJsonAsync($"/inventario/{tipo}", new { ProductoId = producto, AlmacenId = almacen, Cantidad = cantidad, Fecha = fecha, CosteUnitario = coste }))
                .EnsureSuccessStatusCode();
        await Mover("entrada", naranja, 100m, "2025-06-01", 2m);
        await Mover("salida", naranja, 40m, "2025-11-01");
        await Mover("entrada", caja, 5m, "2025-03-01", 10m);
        await Mover("entrada", naranja, 10m, "2026-01-10", 3m); // Posterior al 31/12: no cuenta.

        // Existencia inicial de 2025 en la 300 (la de la apertura).
        (await api.PostAsJsonAsync("/contabilidad/cuentas", new { Codigo = "300", Nombre = "Mercaderías" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await api.PostAsJsonAsync("/contabilidad/asientos", new
        {
            Fecha = "2025-01-01",
            Concepto = "Existencias iniciales",
            Lineas = new object[] { new { CuentaCodigo = "300", Debe = 30m, Haber = 0m }, new { CuentaCodigo = "100", Debe = 0m, Haber = 30m } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        var prevision = (await api.GetFromJsonAsync<RegResp>("/contabilidad/existencias/2025"))!;
        prevision.Contabilizada.Should().BeFalse();
        prevision.Lineas.Should().BeEquivalentTo([new LineaResp("300", "610", 30m, 50m, 50m, 20m), new LineaResp("310", "611", 0m, 120m, 120m, 120m)]);

        // Con el recuento, la fruta vale 100.
        var reg = await OkAsync(await api.PostAsJsonAsync("/contabilidad/existencias/2025", new { Ajustes = new[] { new { CuentaStock = "310", Final = 100m } } }));
        reg.Contabilizada.Should().BeTrue();
        reg.Lineas.Single(l => l.CuentaStock == "310").Final.Should().Be(100m);
        (await SaldoAsync(api, "300")).Should().Be(50m);
        (await SaldoAsync(api, "310")).Should().Be(100m);
        (await SaldoAsync(api, "610")).Should().Be(-20m, "sube la existencia de mercaderías: la variación es acreedora");
        (await SaldoAsync(api, "611")).Should().Be(-100m);
        var asiento = (await api.GetFromJsonAsync<List<AsientoResp>>("/contabilidad/diario?ejercicio=2025"))!.Single(a => a.Id == reg.AsientoId);
        asiento.Should().BeEquivalentTo(new AsientoResp(reg.AsientoId!.Value, "Existencias", "CIE"));

        var otra = await api.PostAsJsonAsync("/contabilidad/existencias/2025", new { });
        (await otra.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("existencias.ya_regularizado");
        (await (await api.PostAsJsonAsync($"/contabilidad/asientos/{reg.AsientoId}/anular", new { })).Content.ReadFromJsonAsync<ProblemaResp>())!
            .Codigo.Should().Be("asiento.de_documento", "se anula desde la regularización");

        // Se anula y se repite con el valor del inventario.
        var anulada = await OkAsync(await api.PostAsync("/contabilidad/existencias/2025/anular", null));
        anulada.Contabilizada.Should().BeFalse();
        anulada.Lineas.Single(l => l.CuentaStock == "300").Inicial.Should().Be(30m, "la anulada no cuenta en la inicial");
        (await SaldoAsync(api, "300")).Should().Be(30m);
        reg = await OkAsync(await api.PostAsJsonAsync("/contabilidad/existencias/2025", new { }));
        reg.Final.Should().Be(170m);

        // El cierre lleva las existencias finales a la apertura de 2026 y ya no se puede regularizar.
        var cierre = await api.PostAsync("/contabilidad/cierre?ejercicio=2025", null);
        cierre.StatusCode.Should().Be(HttpStatusCode.OK, await cierre.Content.ReadAsStringAsync());
        (await SaldoAsync(api, "310", 2026)).Should().Be(120m);
        (await SaldoAsync(api, "300", 2026)).Should().Be(50m);
        (await api.GetFromJsonAsync<RegResp>("/contabilidad/existencias/2025"))!.Should().Match<RegResp>(r => r.Contabilizada && r.EjercicioCerrado && r.Final == 170m);
        (await (await api.PostAsync("/contabilidad/existencias/2025/anular", null)).Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("asiento.ejercicio_cerrado");
    }
}
