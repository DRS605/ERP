using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Límites y avisos de envases, como en Hispatec: límite y mínimo por envase (además del general), aviso o bloqueo con
/// opción de forzar (queda anotado), cierre de periodo que impide registrar y anular, e informe de cuentas sobre el
/// límite, bajo el mínimo y sin movimientos.
/// </summary>
public sealed class LimitesEnvasesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public LimitesEnvasesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo, string Title);
    private sealed record MovimientoResp(Guid Id, string? Observaciones, string? Aviso);
    private sealed record LimiteResp(Guid EnvaseProductoId, int? Limite, int? Minimo, int Saldo);
    private sealed record LimitesResp(string ControlLimite, int? LimiteGeneral, List<LimiteResp> Envases);
    private sealed record FilaResp(Guid CuentaId, string Cuenta, List<string> Incidencias);
    private sealed record InformeResp(List<FilaResp> SobreLimite, List<FilaResp> BajoMinimo, List<FilaResp> SinMovimientos);

    private static async Task<Guid> IdAsync(HttpClient api, string ruta, object cuerpo)
    {
        var r = await api.PostAsJsonAsync(ruta, cuerpo);
        r.StatusCode.Should().Be(HttpStatusCode.Created, $"{ruta}: {await r.Content.ReadAsStringAsync()}");
        return (await r.Content.ReadFromJsonAsync<IdResp>())!.Id;
    }

    private static async Task<T> OkAsync<T>(HttpResponseMessage r)
    {
        r.StatusCode.Should().BeOneOf([HttpStatusCode.OK, HttpStatusCode.Created], await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<T>())!;
    }

    private static async Task<ProblemaResp> ProblemaAsync(HttpResponseMessage r)
    {
        r.IsSuccessStatusCode.Should().BeFalse(await r.Content.ReadAsStringAsync());
        return (await r.Content.ReadFromJsonAsync<ProblemaResp>())!;
    }

    [Fact]
    public async Task Los_limites_por_envase_avisan_o_bloquean_y_el_cierre_impide_mover_el_periodo()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja IFCO", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var pale = await IdAsync(api, "/productos", new { Nombre = "Palé europeo", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var norte = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Norte", NifFiscal = Ayudas.GenerarNif() });
        var sur = await IdAsync(api, "/clientes", new { Nombre = "Frutas Sur", NifFiscal = Ayudas.GenerarNif() });
        var cuenta = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = norte });
        var cuentaSur = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = sur });

        // Límite de 100 cajas y mínimo de 10; los palés, sin límite. De momento, solo avisa.
        (await ProblemaAsync(await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Aviso", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)10, Minimo = (int?)20 } },
        }))).Codigo.Should().Be("envases.limite");
        var limites = await OkAsync<LimitesResp>(await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Aviso", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)100, Minimo = (int?)10 } },
        }));
        limites.Envases.Should().ContainSingle(l => l.EnvaseProductoId == caja && l.Limite == 100 && l.Minimo == 10);

        var m1 = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new
        {
            CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 90 }, new { EnvaseProductoId = pale, Cantidad = 3 } },
        }));
        m1.Aviso.Should().BeNull();
        var m2 = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 20 } } }));
        m2.Aviso.Should().Contain("supera el límite de Caja IFCO: 110 de 100");

        // En bloqueo, entregar más no pasa; forzado sí, y queda anotado. Recoger por debajo del mínimo tampoco pasa.
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Bloqueo", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)100, Minimo = (int?)10 } },
        })).EnsureSuccessStatusCode();
        (await ProblemaAsync(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 5 } } })))
            .Codigo.Should().Be("envases.limite");
        var forzado = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new
        {
            CuentaId = cuenta, Forzar = true, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 5 } },
        }));
        forzado.Observaciones.Should().StartWith("Forzado:");
        (await ProblemaAsync(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = -110 } } })))
            .Title.Should().Contain("por debajo del mínimo de Caja IFCO: 5 (mínimo 10)");
        (await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = -50 } } })))
            .Aviso.Should().BeNull("quedan 65: dentro de los márgenes");
        (await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuentaSur, Lineas = new[] { new { EnvaseProductoId = pale, Cantidad = 2 } } })))
            .Aviso.Should().BeNull("otra cuenta sin límites");

        // Informe: nadie supera ni baja; con una fecha futura, las dos cuentas con saldo están quietas.
        var informe = (await api.GetFromJsonAsync<InformeResp>($"/agro/envases/informe-limites?sinMovimientosDesde={hoy.AddDays(1):yyyy-MM-dd}"))!;
        informe.SobreLimite.Should().BeEmpty();
        informe.SinMovimientos.Select(f => f.CuentaId).Should().BeEquivalentTo([cuenta, cuentaSur]);
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Aviso", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)60, Minimo = (int?)70 } },
        })).StatusCode.Should().Be(HttpStatusCode.BadRequest, "el mínimo no supera el límite");
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Aviso", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)60, Minimo = (int?)null }, new { EnvaseProductoId = pale, Limite = (int?)null, Minimo = (int?)5 } },
        })).EnsureSuccessStatusCode();
        informe = (await api.GetFromJsonAsync<InformeResp>("/agro/envases/informe-limites"))!;
        informe.SobreLimite.Should().ContainSingle(f => f.CuentaId == cuenta).Which.Incidencias.Should().Equal("Caja IFCO: 65 de 60");
        informe.BajoMinimo.Should().ContainSingle(f => f.CuentaId == cuenta).Which.Incidencias.Should().Equal("Palé europeo: 3 (mínimo 5)");
        informe.SinMovimientos.Should().BeEmpty("sin fecha no se pide");

        // Cierre hasta hoy: no se registra ni se anula nada del periodo; reabierto, sí.
        (await api.PutAsJsonAsync("/agro/envases/configuracion", new { FechaCierre = hoy })).EnsureSuccessStatusCode();
        (await ProblemaAsync(await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuentaSur, Lineas = new[] { new { EnvaseProductoId = pale, Cantidad = 1 } } })))
            .Codigo.Should().Be("envases.periodo_cerrado");
        (await ProblemaAsync(await api.PostAsJsonAsync($"/agro/envases/movimientos/{m1.Id}/anular", new { Motivo = "Error" }))).Codigo.Should().Be("envases.periodo_cerrado");
        (await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new
        {
            CuentaId = cuentaSur, Fecha = hoy.AddDays(1), Lineas = new[] { new { EnvaseProductoId = pale, Cantidad = 1 } },
        }))).Should().NotBeNull("después del cierre sí");
        (await api.PutAsJsonAsync("/agro/envases/configuracion", new { FechaCierre = (DateOnly?)null })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync($"/agro/envases/movimientos/{m1.Id}/anular", new { Motivo = "Error" })).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
