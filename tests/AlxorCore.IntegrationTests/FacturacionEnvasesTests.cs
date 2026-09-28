using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Envases a facturar, como en Hispatec: un cliente con gestión «facturar» o «facturar el exceso» recibe un albarán de
/// venta con los envases, que salen de su saldo; anular el movimiento anula el albarán. Además: el libro del agricultor
/// llega al libro por tercero, el fichero de declaración a un pool, el stock en terceros y los justificantes en PDF.
/// </summary>
public sealed class FacturacionEnvasesTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public FacturacionEnvasesTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo, string Title);
    private sealed record MovimientoResp(Guid Id, string Numero, string Origen, bool Anulado);
    private sealed record LineaResp(Guid EnvaseProductoId, int Cantidad);
    private sealed record FacturacionResp(Guid CuentaId, Guid MovimientoId, string Movimiento, Guid AlbaranId, string Albaran, List<LineaResp> Lineas);
    private sealed record MasivaResp(List<FacturacionResp> Facturadas, List<string> Omitidas);
    private sealed record SaldoResp(Guid EnvaseProductoId, int Saldo);
    private sealed record CuentaResp(Guid Id, string Tipo, Guid TerceroId, string Gestion, int SaldoTotal, List<SaldoResp> Saldos);
    private sealed record AlbaranResp(Guid Id, string Estado, decimal Base);
    private sealed record StockResp(Guid EnvaseProductoId, int Clientes, int Proveedores, int Pools, int Total);

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

    private async Task<HttpClient> ApiAgroAsync()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return api;
    }

    private static Task<HttpResponseMessage> MoverAsync(HttpClient api, Guid cuenta, params (Guid Envase, int Cantidad)[] lineas) =>
        api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuenta, Lineas = lineas.Select(l => new { EnvaseProductoId = l.Envase, l.Cantidad }).ToArray() });

    [Fact]
    public async Task Se_factura_el_exceso_sobre_el_limite_con_un_albaran_y_al_anularlo_vuelve_al_saldo()
    {
        var api = await ApiAgroAsync();
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja de madera", PrecioUnitario = 1.5m, Tipo = "Bien", Unidad = "ud" });
        var pale = await IdAsync(api, "/productos", new { Nombre = "Palé retornable", PrecioUnitario = 8m, Tipo = "Bien", Unidad = "ud" });
        var norte = await IdAsync(api, "/clientes", new { Nombre = "Mercados Norte", NifFiscal = Ayudas.GenerarNif() });
        var sur = await IdAsync(api, "/clientes", new { Nombre = "Fruterías Sur", NifFiscal = Ayudas.GenerarNif() });
        var cuenta = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = norte });
        var cuentaSur = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = sur });
        await OkAsync<MovimientoResp>(await MoverAsync(api, cuenta, (caja, 30), (pale, 2)));
        await OkAsync<MovimientoResp>(await MoverAsync(api, cuentaSur, (caja, 5)));

        // Solo a clientes, y una cuenta que retorna sus envases necesita que se diga qué se le factura.
        (await ProblemaAsync(await api.PostAsJsonAsync($"/agro/envases/cuentas/{cuentaSur}/facturar", new { }))).Codigo.Should().Be("envases.nada_que_facturar");
        (await ProblemaAsync(await api.PostAsJsonAsync($"/agro/envases/cuentas/{cuenta}/facturar", new
        {
            Lineas = new[] { new { EnvaseProductoId = pale, Cantidad = 3 } },
        }))).Codigo.Should().Be("envases.supera_saldo");

        // Factura el exceso sobre el límite de 20 cajas: 10 cajas a su precio de artículo.
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}", new { Gestion = "FacturarExceso", Activa = true })).EnsureSuccessStatusCode();
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuenta}/limites", new
        {
            Control = "Aviso", Envases = new[] { new { EnvaseProductoId = caja, Limite = (int?)20, Minimo = (int?)null } },
        })).EnsureSuccessStatusCode();
        var masiva = await OkAsync<MasivaResp>(await api.PostAsJsonAsync("/agro/envases/facturar", new { }));
        masiva.Omitidas.Should().BeEmpty();
        var f = masiva.Facturadas.Should().ContainSingle().Subject;
        f.CuentaId.Should().Be(cuenta);
        f.Lineas.Should().ContainSingle(l => l.EnvaseProductoId == caja && l.Cantidad == -10);
        var albaran = (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{f.AlbaranId}"))!;
        albaran.Base.Should().Be(15m);

        var cuentas = (await api.GetFromJsonAsync<List<CuentaResp>>("/agro/envases/cuentas"))!;
        cuentas.Single(c => c.Id == cuenta).Should().Match<CuentaResp>(c => c.Gestion == "FacturarExceso" && c.SaldoTotal == 22);
        cuentas.Single(c => c.Id == cuenta).Saldos.Single(s => s.EnvaseProductoId == caja).Saldo.Should().Be(20);
        // Ya no hay exceso: la masiva no vuelve a facturar.
        (await OkAsync<MasivaResp>(await api.PostAsJsonAsync("/agro/envases/facturar", new { }))).Facturadas.Should().BeEmpty();

        // Todo el saldo, con precio propio.
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaSur}", new { Gestion = "Facturar", Activa = true })).EnsureSuccessStatusCode();
        var sola = await OkAsync<FacturacionResp>(await api.PostAsJsonAsync($"/agro/envases/cuentas/{cuentaSur}/facturar", new
        {
            Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 5, Precio = (decimal?)2m } },
        }));
        (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{sola.AlbaranId}"))!.Base.Should().Be(10m);

        // Anular el movimiento anula el albarán y devuelve las cajas al saldo.
        await OkAsync<MovimientoResp>(await api.PostAsJsonAsync($"/agro/envases/movimientos/{f.MovimientoId}/anular", new { Motivo = "Las devuelve" }));
        (await api.GetFromJsonAsync<AlbaranResp>($"/albaranes-venta/{f.AlbaranId}"))!.Estado.Should().Be("Anulado");
        (await api.GetFromJsonAsync<List<CuentaResp>>("/agro/envases/cuentas"))!.Single(c => c.Id == cuenta).Saldos
            .Single(s => s.EnvaseProductoId == caja).Saldo.Should().Be(30);

        // Solo se factura a clientes.
        (await ProblemaAsync(await api.PutAsJsonAsync($"/agro/envases/cuentas/{await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Transportista", TerceroId = Guid.NewGuid(), Nombre = "Transportes Ruta" })}",
            new { Gestion = "Facturar", Activa = true }))).Codigo.Should().Be("envases.gestion_cliente");
    }

    [Fact]
    public async Task El_libro_del_agricultor_llega_al_de_terceros_y_el_pool_recibe_su_fichero()
    {
        var api = await ApiAgroAsync();
        var palot = await IdAsync(api, "/productos", new { Nombre = "Palot", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var chepPale = await IdAsync(api, "/productos", new { Referencia = "CHEP-EUR", Nombre = "Palé CHEP", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Huertos Llanos", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });

        // Entrega de vacíos al agricultor: aparece en su cuenta de proveedor del libro por tercero.
        (await api.PostAsJsonAsync($"/agro/agricultores/{agricultor}/envases", new { EnvaseProductoId = palot, Cantidad = 50 })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var cuentaAgricultor = (await api.GetFromJsonAsync<List<CuentaResp>>("/agro/envases/cuentas"))!.Single(c => c.TerceroId == proveedor);
        cuentaAgricultor.Tipo.Should().Be("Proveedor");
        cuentaAgricultor.Saldos.Should().ContainSingle(s => s.EnvaseProductoId == palot && s.Saldo == 50);
        var movAgricultor = (await api.GetFromJsonAsync<List<MovimientoResp>>($"/agro/envases/movimientos?cuentaId={cuentaAgricultor.Id}"))!.Single();
        movAgricultor.Origen.Should().Be("Recepcion");
        (await ProblemaAsync(await api.PostAsJsonAsync($"/agro/envases/movimientos/{movAgricultor.Id}/anular", new { }))).Codigo.Should().Be("envases.de_recepcion");

        // El pool (CHEP) declara sus palés: los movimientos con los clientes en el periodo.
        var chep = await IdAsync(api, "/proveedores", new { Nombre = "CHEP España", NifFiscal = Ayudas.GenerarNif() });
        var cuentaPool = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Pool", TerceroId = chep });
        var hoy = DateOnly.FromDateTime(DateTime.Today);
        (await ProblemaAsync(await api.GetAsync($"/agro/envases/cuentas/{cuentaPool}/fichero-pool?desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}")))
            .Codigo.Should().Be("envases.pool_sin_envases");
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaPool}", new { EnvasesPool = new[] { chepPale }, Activa = true })).EnsureSuccessStatusCode();
        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Cadena Levante", NifFiscal = Ayudas.GenerarNif() });
        var cuentaCliente = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = cliente });
        var entrega = await OkAsync<MovimientoResp>(await MoverAsync(api, cuentaCliente, (chepPale, 12), (palot, 3)));
        await OkAsync<MovimientoResp>(await MoverAsync(api, cuentaCliente, (chepPale, -4)));
        await OkAsync<MovimientoResp>(await MoverAsync(api, cuentaPool, (chepPale, -20)));

        var fichero = await api.GetAsync($"/agro/envases/cuentas/{cuentaPool}/fichero-pool?desde={hoy:yyyy-MM-dd}&hasta={hoy:yyyy-MM-dd}");
        fichero.StatusCode.Should().Be(HttpStatusCode.OK);
        fichero.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var filas = (await fichero.Content.ReadAsStringAsync()).TrimStart('﻿').Split('\n', StringSplitOptions.RemoveEmptyEntries);
        filas[0].Should().StartWith("Fecha;Movimiento;Tipo;Tercero;NIF");
        filas.Should().HaveCount(4, "la cabecera y los tres movimientos de palés CHEP (los palots no son del pool)");
        filas.Should().Contain(f => f.Contains($";{entrega.Numero};Cliente;Cadena Levante;") && f.Contains(";CHEP-EUR;Palé CHEP;12;0;"));
        filas.Should().Contain(f => f.Contains(";Cliente;Cadena Levante;") && f.Contains(";0;4;"));

        // Stock en terceros: los palés CHEP en el cliente y los que debemos al pool.
        var stock = (await api.GetFromJsonAsync<List<StockResp>>("/agro/envases/stock-terceros"))!;
        stock.Single(s => s.EnvaseProductoId == chepPale).Should().Match<StockResp>(s => s.Clientes == 8 && s.Pools == -20 && s.Total == -12);
        stock.Single(s => s.EnvaseProductoId == palot).Should().Match<StockResp>(s => s.Clientes == 3 && s.Proveedores == 50);

        // Justificante del movimiento y extracto de la cuenta, en PDF.
        var pdf = await api.GetAsync($"/agro/envases/movimientos/{entrega.Id}/pdf");
        pdf.StatusCode.Should().Be(HttpStatusCode.OK, await pdf.Content.ReadAsStringAsync());
        (await pdf.Content.ReadAsByteArrayAsync())[..4].Should().Equal("%PDF"u8.ToArray());
        var extracto = await api.GetAsync($"/agro/envases/cuentas/{cuentaCliente}/extracto/pdf");
        extracto.StatusCode.Should().Be(HttpStatusCode.OK, await extracto.Content.ReadAsStringAsync());
        (await extracto.Content.ReadAsByteArrayAsync())[..4].Should().Equal("%PDF"u8.ToArray());
    }
}
