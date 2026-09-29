using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Agro en el inventario del almacén (si se activa): la recepción entra los kilos del artículo y la anulación los saca;
/// los ajustes de la partida salen; los envases entregados a terceros salen del almacén y los recogidos entran.
/// </summary>
public sealed class InventarioAgroTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public InventarioAgroTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, string Estado, List<LineaResp> Lineas);
    private sealed record ProductoResp(Guid Id, decimal Stock);

    private static readonly int Anio = DateTime.UtcNow.Year;

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<decimal> StockAsync(HttpClient api, Guid producto) => (await api.GetFromJsonAsync<ProductoResp>($"/productos/{producto}"))!.Stock;

    [Fact]
    public async Task Las_recepciones_ajustes_y_envases_mueven_el_inventario_si_se_activa()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var dia = new DateOnly(Anio, 3, 10);
        var campana = await IdAsync(api, "/agro/campanas", new { Codigo = $"{Anio}", Nombre = $"Campaña {Anio}", Desde = new DateOnly(Anio, 1, 1), Hasta = new DateOnly(Anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Ana Hortelana", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var naranja = await IdAsync(api, "/productos", new { Nombre = "Naranja", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg", ControlarStock = true });
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud", ControlarStock = true });
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Mercado Central", NifFiscal = Ayudas.GenerarNif() });

        async Task<RecepcionResp> RecibirAsync()
        {
            var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = dia });
            var r = (await (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = naranja, EnvaseProductoId = palot })).Content.ReadFromJsonAsync<RecepcionResp>())!;
            (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas.Single().Id}/pesadas", new { BrutoKg = 1_200m, TaraKg = 200m, Envases = 5 })).EnsureSuccessStatusCode();
            var confirmada = await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null);
            confirmada.StatusCode.Should().Be(HttpStatusCode.OK, await confirmada.Content.ReadAsStringAsync());
            return (await confirmada.Content.ReadFromJsonAsync<RecepcionResp>())!;
        }

        // Sin activarlo, nada cambia en el inventario.
        await RecibirAsync();
        (await StockAsync(api, naranja)).Should().Be(0m);

        (await api.PutAsJsonAsync("/agro/configuracion", new { PrefijoGs1 = "8400000", DigitoExtension = 0, ReflejarPartidasEnInventario = true, ReflejarEnvasesEnInventario = true }))
            .EnsureSuccessStatusCode();
        var r = await RecibirAsync();
        (await StockAsync(api, naranja)).Should().Be(1_000m);
        (await StockAsync(api, palot)).Should().Be(5m, "los palots llenos vuelven de la finca al almacén");

        (await api.PostAsJsonAsync($"/agro/partidas/{r.Lineas.Single().PartidaId}/ajustes", new { Kilos = -50m, Concepto = "Merma" })).EnsureSuccessStatusCode();
        (await StockAsync(api, naranja)).Should().Be(950m);

        // Envases entregados a un cliente: salen del almacén; los recogidos entran.
        var cuenta = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = cliente });
        (await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 12 } } })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = -4 } } })).EnsureSuccessStatusCode();
        (await StockAsync(api, caja)).Should().Be(-8m);

        // Otra recepción, anulada: sus kilos y sus palots vuelven a salir.
        var otra = await RecibirAsync();
        (await StockAsync(api, naranja)).Should().Be(1_950m);
        (await api.PostAsJsonAsync($"/agro/recepciones/{otra.Id}/anular", new { Motivo = "Error de báscula" })).EnsureSuccessStatusCode();
        (await StockAsync(api, naranja)).Should().Be(950m);
        (await StockAsync(api, palot)).Should().Be(5m);
    }
}
