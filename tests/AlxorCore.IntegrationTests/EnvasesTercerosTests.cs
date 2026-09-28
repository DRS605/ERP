using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace AlxorCore.IntegrationTests;

/// <summary>
/// Envases retornables por tercero, como en Hispatec: al expedir palés se entregan al cliente las cajas y el palé de la
/// plantilla (o a su transportista); el libro es numerado y de solo inserción, se anula con su contrario y la anulación
/// de la expedición devuelve los envases; hay cuentas agrupadoras, bloqueos, límites y extracto.
/// </summary>
public sealed class EnvasesTercerosTests : IClassFixture<FabricaApiPruebas>
{
    private readonly FabricaApiPruebas _fabrica;

    public EnvasesTercerosTests(FabricaApiPruebas fabrica) => _fabrica = fabrica;

    private sealed record IdResp(Guid Id);
    private sealed record SeleccionResp(string Token);
    private sealed record ProblemaResp(string Codigo);
    private sealed record LineaResp(Guid Id, Guid? PartidaId);
    private sealed record RecepcionResp(Guid Id, List<LineaResp> Lineas);
    private sealed record PaleResp(Guid Id, string Estado);
    private sealed record SaldoResp(Guid EnvaseProductoId, string Envase, int Saldo);
    private sealed record CuentaResp(Guid Id, string Tipo, Guid TerceroId, string Nombre, int SaldoTotal, List<SaldoResp> Saldos, bool SuperaLimite);
    private sealed record LineaMovResp(Guid EnvaseProductoId, int Cantidad);
    private sealed record MovimientoResp(Guid Id, string Numero, Guid CuentaId, Guid CuentaSolicitadaId, string Origen, bool Anulado, Guid? AnulaMovimientoId,
        List<LineaMovResp> Lineas, string? Aviso);
    private sealed record LineaExtractoResp(MovimientoResp Movimiento, int Neto, int Acumulado);
    private sealed record ExtractoResp(int SaldoInicial, List<LineaExtractoResp> Movimientos, int SaldoFinal);

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

    private static async Task<List<CuentaResp>> CuentasAsync(HttpClient api) => (await api.GetFromJsonAsync<List<CuentaResp>>("/agro/envases/cuentas"))!;

    [Fact]
    public async Task Expedir_entrega_los_envases_y_el_libro_lleva_saldo_y_extracto()
    {
        var (api, empresa) = await Ayudas.ConEmpresaAsync(_fabrica);
        (await api.PutAsJsonAsync("/empresas/actual/plan", new { Edicion = "completa", ModulosAdicionales = new[] { "agro" } })).EnsureSuccessStatusCode();
        var token = (await (await api.PostAsync(new Uri($"/empresas/{empresa}/seleccionar", UriKind.Relative), null)).Content.ReadFromJsonAsync<SeleccionResp>())!.Token;
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var anio = DateTime.UtcNow.Year;
        var dia = DateOnly.FromDateTime(DateTime.Today);

        // Partida de 4.000 kg de tomate y plantilla de palé de 200 cajas de 10 kg con caja y palé retornables.
        await IdAsync(api, "/agro/campanas", new { Codigo = $"{anio}", Nombre = $"Campaña {anio}", Desde = new DateOnly(anio, 1, 1), Hasta = new DateOnly(anio, 12, 31) });
        var proveedor = await IdAsync(api, "/proveedores", new { Nombre = "Juan Labrador", NifFiscal = Ayudas.GenerarNif() });
        var agricultor = await IdAsync(api, "/agro/agricultores", new { ProveedorId = proveedor, Regimen = "Reagp" });
        var tomate = await IdAsync(api, "/productos", new { Nombre = "Tomate", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "kg" });
        var caja = await IdAsync(api, "/productos", new { Nombre = "Caja IFCO 6420", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var paleEuro = await IdAsync(api, "/productos", new { Nombre = "Palé europeo", PrecioUnitario = 0m, Tipo = "Bien", Unidad = "ud" });
        var rec = await IdAsync(api, "/agro/recepciones", new { AgricultorId = agricultor, Fecha = dia });
        var r = await OkAsync<RecepcionResp>(await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas", new { ProductoId = tomate, FechaRecoleccion = dia }));
        (await api.PostAsJsonAsync($"/agro/recepciones/{rec}/lineas/{r.Lineas[0].Id}/pesadas", new { BrutoKg = 4_500m, TaraKg = 500m })).EnsureSuccessStatusCode();
        var partida = (await OkAsync<RecepcionResp>(await api.PostAsync(new Uri($"/agro/recepciones/{rec}/confirmar", UriKind.Relative), null))).Lineas[0].PartidaId!.Value;
        var plantilla = await IdAsync(api, "/agro/plantillas-pale", new
        {
            Codigo = "EUR200", Nombre = "Europeo 200 cajas", CajasPorPale = 200, KilosPorCaja = 10m, EnvaseProductoId = caja, PaleProductoId = paleEuro,
        });
        var pales = await OkAsync<List<PaleResp>>(await api.PostAsJsonAsync("/agro/pales/montar", new { PlantillaId = plantilla, PartidaId = partida }));
        pales.Should().HaveCount(2);

        var cliente = await IdAsync(api, "/clientes", new { Nombre = "Supermercados Norte", NifFiscal = Ayudas.GenerarNif() });
        var transportista = await IdAsync(api, "/transporte/transportistas", new { Nombre = "Frío Rápido SL", Nif = "B98765432", Pais = "ES" });

        // Primer palé al cliente: 200 cajas y 1 palé a su cuenta (que se abre sola).
        (await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pales[0].Id }, ClienteId = cliente, Referencia = "EXP-1" })).EnsureSuccessStatusCode();
        var cuentaCliente = (await CuentasAsync(api)).Single(c => c.TerceroId == cliente);
        cuentaCliente.Saldos.Select(s => (s.Envase, s.Saldo)).Should().BeEquivalentTo([("Caja IFCO 6420", 200), ("Palé europeo", 1)]);

        // El cliente lleva sus envases al transportista: el segundo palé va a la cuenta del transportista.
        var cuentaTransp = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Transportista", TerceroId = transportista, Nombre = "Frío Rápido SL" });
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaCliente.Id}", new { ImputarATransportista = true })).EnsureSuccessStatusCode();
        (await api.PostAsJsonAsync("/agro/expediciones", new { PaleIds = new[] { pales[1].Id }, ClienteId = cliente, TransportistaId = transportista, Referencia = "EXP-2" }))
            .EnsureSuccessStatusCode();
        (await CuentasAsync(api)).Single(c => c.Id == cuentaTransp).SaldoTotal.Should().Be(201);
        var movs = (await api.GetFromJsonAsync<List<MovimientoResp>>($"/agro/envases/movimientos?cuentaId={cuentaCliente.Id}"))!;
        movs.Should().HaveCount(2);
        movs.Should().ContainSingle(m => m.CuentaId == cuentaTransp && m.CuentaSolicitadaId == cuentaCliente.Id && m.Origen == "Expedicion");
        movs.Select(m => m.Numero).Should().Equal($"ENV-{anio}-000001", $"ENV-{anio}-000002");

        // El cliente devuelve 150 cajas vacías.
        var devolucion = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new
        {
            CuentaId = cuentaCliente.Id, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = -150 } }, Observaciones = "Devuelve vacíos",
        }));
        (await CuentasAsync(api)).Single(c => c.Id == cuentaCliente.Id).Saldos.Single(s => s.EnvaseProductoId == caja).Saldo.Should().Be(50);

        // Se anula la devolución (con su contrario) y no se puede anular dos veces.
        var contra = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync($"/agro/envases/movimientos/{devolucion.Id}/anular", new { Motivo = "Contadas mal" }));
        contra.Should().Match<MovimientoResp>(m => m.Origen == "Anulacion" && m.AnulaMovimientoId == devolucion.Id && m.Lineas.Single().Cantidad == 150);
        var otra = await api.PostAsJsonAsync($"/agro/envases/movimientos/{devolucion.Id}/anular", new { });
        (await otra.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("envases.ya_anulado");

        // Anular la expedición del primer palé devuelve sus envases.
        (await api.PostAsync(new Uri($"/agro/pales/{pales[0].Id}/anular-expedicion", UriKind.Relative), null)).EnsureSuccessStatusCode();
        (await CuentasAsync(api)).Single(c => c.Id == cuentaCliente.Id).SaldoTotal.Should().Be(0);

        // Extracto del transportista: 201 entregados.
        var extracto = await OkAsync<ExtractoResp>(await api.GetAsync(new Uri($"/agro/envases/cuentas/{cuentaTransp}/extracto", UriKind.Relative)));
        extracto.Should().Match<ExtractoResp>(x => x.SaldoInicial == 0 && x.SaldoFinal == 201 && x.Movimientos.Count == 1 && x.Movimientos[0].Acumulado == 201);

        // Límite y bloqueo: por encima del límite se avisa; bloqueada, no se mueve nada.
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaTransp}", new { Limite = 100 })).EnsureSuccessStatusCode();
        (await CuentasAsync(api)).Single(c => c.Id == cuentaTransp).SuperaLimite.Should().BeTrue();
        var entrega = await OkAsync<MovimientoResp>(await api.PostAsJsonAsync("/agro/envases/movimientos", new
        {
            CuentaId = cuentaTransp, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 10 } },
        }));
        entrega.Aviso.Should().Contain("supera su límite");
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaTransp}", new { Bloqueo = "Bloqueo", MotivoBloqueo = "Impago de envases" })).EnsureSuccessStatusCode();
        var bloqueada = await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuentaTransp, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 5 } } });
        (await bloqueada.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("envases.cuenta_bloqueada");

        // Cuenta agrupadora: la tienda acumula en la central.
        var central = await IdAsync(api, "/clientes", new { Nombre = "Central de compras", NifFiscal = Ayudas.GenerarNif() });
        var tienda = await IdAsync(api, "/clientes", new { Nombre = "Tienda 12", NifFiscal = Ayudas.GenerarNif() });
        var cuentaCentral = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = central });
        var cuentaTienda = await IdAsync(api, "/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = tienda });
        (await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaTienda}", new { AgrupadoraId = cuentaCentral })).EnsureSuccessStatusCode();
        var ciclo = await api.PutAsJsonAsync($"/agro/envases/cuentas/{cuentaCentral}", new { AgrupadoraId = cuentaTienda });
        (await ciclo.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("envases.agrupadora");
        (await api.PostAsJsonAsync("/agro/envases/movimientos", new { CuentaId = cuentaTienda, Lineas = new[] { new { EnvaseProductoId = caja, Cantidad = 30 } } }))
            .EnsureSuccessStatusCode();
        (await CuentasAsync(api)).Single(c => c.Id == cuentaCentral).SaldoTotal.Should().Be(30);
        var duplicada = await api.PostAsJsonAsync("/agro/envases/cuentas", new { Tipo = "Cliente", TerceroId = tienda });
        (await duplicada.Content.ReadFromJsonAsync<ProblemaResp>())!.Codigo.Should().Be("envases.cuenta_existe");
    }
}
